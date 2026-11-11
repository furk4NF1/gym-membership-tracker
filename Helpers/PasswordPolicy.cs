using System;
using System.Linq;

namespace SporSalonuTakip.Helpers
{
    internal static class PasswordPolicy
    {
        public static string Validate(string password, string username, string fullName)
        {
            if (String.IsNullOrWhiteSpace(password) || password.Length < 6)
                return "Parola en az 6 karakter olmalıdır.";
            if (!password.Any(Char.IsUpper) || !password.Any(Char.IsLower) || !password.Any(Char.IsDigit) || !password.Any(c => !Char.IsLetterOrDigit(c)))
                return "Parola büyük harf, küçük harf, rakam ve özel karakter içermelidir.";
            string lower = password.ToLowerInvariant();
            if (!String.IsNullOrWhiteSpace(username) && lower.Contains(username.Trim().ToLowerInvariant()))
                return "Parola kullanıcı adını içeremez.";
            if (!String.IsNullOrWhiteSpace(fullName))
            {
                foreach (string part in fullName.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries))
                    if (part.Length >= 3 && lower.Contains(part.ToLowerInvariant())) return "Parola ad veya soyad bilgisini içeremez.";
            }
            if (System.Text.RegularExpressions.Regex.IsMatch(lower, @"\d{4}"))
                return "Parola doğum yılı olarak kullanılabilecek dört basamaklı bir sayı içeremez.";
            string[] sequences = { "qwerty", "asdfgh", "zxcvbn" };
            if (sequences.Any(s => lower.Contains(s)) || HasSequentialCharacters(lower)) return "Ardışık veya klavye sıralı parola kullanılamaz.";
            return null;
        }

        private static bool HasSequentialCharacters(string value)
        {
            for (int i = 0; i <= value.Length - 4; i++)
            {
                bool ascending = true, descending = true;
                for (int j = 1; j < 4; j++)
                {
                    ascending &= value[i + j] == value[i + j - 1] + 1;
                    descending &= value[i + j] == value[i + j - 1] - 1;
                }
                if (ascending || descending) return true;
            }
            return false;
        }
    }
}
