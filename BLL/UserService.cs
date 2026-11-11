using System;
using System.Data;
using System.Net.Mail;
using SporSalonuTakip.Entities;
using SporSalonuTakip.Helpers;

namespace SporSalonuTakip.BLL
{
    public static class UserService
    {
        public static DataTable GetUsers(AppUser actor)
        {
            RequireAdmin(actor);
            return Database.GetUsers();
        }

        public static string CreateUser(AppUser actor, string username, string email, string role)
        {
            RequireAdmin(actor);
            username = (username ?? "").Trim(); email = (email ?? "").Trim();
            if (username.Length < 3 || username.Length > 50) throw new InvalidOperationException("Kullanıcı adı 3-50 karakter olmalıdır.");
            try { new MailAddress(email); } catch { throw new InvalidOperationException("Geçerli bir e-posta adresi yazın."); }
            if (role != "Admin" && role != "User") throw new InvalidOperationException("Rol Admin veya User olmalıdır.");
            string temporaryPassword;
            do { temporaryPassword = TemporaryPasswordGenerator.Generate(); }
            while (PasswordPolicy.Validate(temporaryPassword, username, null) != null);
            Database.CreateUser(username, email, role, PasswordHasher.Create(temporaryPassword));
            return temporaryPassword;
        }

        public static void ChangeRole(AppUser actor, int targetUserId, string newRole)
        {
            RequireAdmin(actor);
            if (targetUserId == actor.UserId) throw new UnauthorizedAccessException("Kendi rolünüzü değiştiremezsiniz.");
            if (newRole != "Admin" && newRole != "User") throw new InvalidOperationException("Rol Admin veya User olmalıdır.");
            DataTable users = Database.GetUsers();
            DataRow[] target = users.Select("UserId=" + targetUserId);
            if (target.Length == 0) throw new InvalidOperationException("Kullanıcı bulunamadı.");
            if (Convert.ToString(target[0]["Role"]) == "Admin" && newRole != "Admin" && Database.CountAdmins() <= 1)
                throw new InvalidOperationException("Sistemde en az bir etkin Admin kalmalıdır.");
            Database.ChangeRole(targetUserId, newRole);
        }

        private static void RequireAdmin(AppUser actor)
        {
            if (actor == null) throw new UnauthorizedAccessException("Oturum açmanız gerekir.");
            string currentRole = Database.GetActiveRole(actor.UserId);
            if (currentRole == null) throw new UnauthorizedAccessException("Hesabınız etkin değil; oturum açın.");
            actor.Role = currentRole;
            if (!actor.IsAdmin) throw new UnauthorizedAccessException("Kullanıcı yönetimi yalnızca Admin rolüne açıktır.");
        }
    }
}
