using System;
using System.Data;
using SporSalonuTakip.Entities;

namespace SporSalonuTakip.BLL
{
    public static class MemberService
    {
        public static DataTable GetMembers(AppUser user)
        {
            RequireSignedIn(user);
            DataTable members = Database.GetMembers();
            members.Columns.Add("EndDate", typeof(DateTime));
            members.Columns.Add("MembershipStatus", typeof(string));

            foreach (DataRow row in members.Rows)
            {
                DateTime startDate = Convert.ToDateTime(row["StartDate"]).Date;
                DateTime? endDate = CalculateEndDate(startDate, Convert.ToString(row["MembershipType"]));
                if (!endDate.HasValue)
                {
                    row["EndDate"] = DBNull.Value;
                    row["MembershipStatus"] = "Tanımsız üyelik";
                    continue;
                }

                row["EndDate"] = endDate.Value;
                int daysRemaining = (endDate.Value.Date - DateTime.Today).Days;
                row["MembershipStatus"] = daysRemaining < 0
                    ? "Süresi dolmuş"
                    : daysRemaining <= 7 ? "Yakında bitiyor" : "Aktif";
            }

            return members;
        }

        public static void SaveMember(AppUser user, int? id, string name, string phone, string type, DateTime startDate)
        {
            RequireAdmin(user);
            if (String.IsNullOrWhiteSpace(name) || String.IsNullOrWhiteSpace(phone)) throw new InvalidOperationException("Ad soyad ve telefon alanları zorunludur.");
            type = type == null ? String.Empty : type.Trim();
            if (type != "Aylık" && type != "3 Aylık" && type != "Yıllık") throw new InvalidOperationException("Geçerli bir üyelik türü seçin.");
            Database.SaveMember(id, name.Trim(), phone.Trim(), type, startDate.Date);
        }

        public static void DeleteMember(AppUser user, int id)
        {
            RequireAdmin(user);
            Database.DeleteMember(id);
        }

        private static void RequireSignedIn(AppUser user)
        {
            if (user == null) throw new UnauthorizedAccessException("Bu işlem için oturum açmalısınız.");
            string currentRole = Database.GetActiveRole(user.UserId);
            if (currentRole == null) throw new UnauthorizedAccessException("Hesabınız etkin değil; oturum açın.");
            user.Role = currentRole;
        }

        private static void RequireAdmin(AppUser user)
        {
            RequireSignedIn(user);
            if (!user.IsAdmin) throw new UnauthorizedAccessException("Bu işlem yalnızca Admin rolüne açıktır.");
        }

        private static DateTime? CalculateEndDate(DateTime startDate, string membershipType)
        {
            int months;
            switch (membershipType)
            {
                case "Aylık": months = 1; break;
                case "3 Aylık": months = 3; break;
                case "Yıllık": months = 12; break;
                default: return null;
            }

            return startDate.AddMonths(months).AddDays(-1);
        }
    }
}
