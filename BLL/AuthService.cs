using System;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;
using SporSalonuTakip.Entities;
using SporSalonuTakip.Helpers;

namespace SporSalonuTakip.BLL
{
    public static class AuthService
    {
        public static AppUser Login(string username, string password)
        {
            UserRecord user = Database.FindUser(username.Trim());
            if (user == null || !user.IsActive) return null;
            bool valid = user.IsLegacyHash
                ? PasswordHasher.VerifyLegacySha256(password, user.PasswordHash)
                : PasswordHasher.Verify(password, new PasswordCredential { Hash=user.PasswordHash, Salt=user.PasswordSalt, Iterations=user.PasswordIterations });
            if (!valid) return null;
            if (user.IsLegacyHash) Database.UpgradeLegacyPassword(user.UserId, PasswordHasher.Create(password));
            return ToAppUser(user);
        }

        public static void ChangePassword(AppUser currentUser, string currentPassword, string newPassword)
        {
            UserRecord stored = Database.FindUser(currentUser.Username);
            if (stored == null || !stored.IsActive || !VerifyPassword(stored, currentPassword)) throw new InvalidOperationException("Mevcut parola doğru değil.");
            ValidateNewPassword(newPassword, stored.Username, null);
            if (VerifyPassword(stored, newPassword)) throw new InvalidOperationException("Yeni parola mevcut parolayla aynı olamaz.");
            foreach (PasswordCredential old in Database.GetPasswordHistory(stored.UserId))
                if (PasswordHasher.Verify(newPassword, old)) throw new InvalidOperationException("Son üç parolanızdan birini yeniden kullanamazsınız.");
            Database.ChangePassword(stored.UserId, PasswordHasher.Create(newPassword));
            currentUser.MustChangePassword = false;
        }

        public static void RequestPasswordReset(string email)
        {
            EmailSender.EnsureConfigured();
            try { new MailAddress(email); } catch { return; }
            UserRecord user = Database.FindUserByEmail(email.Trim());
            if (user == null || !user.IsActive) return;
            string token = CreateToken();
            int id = Database.CreateResetToken(email.Trim(), HashToken(token), Environment.MachineName);
            if (id != 0) EmailSender.SendResetCode(email.Trim(), token);
        }

        public static void ResetPassword(string email, string token, string newPassword)
        {
            UserRecord user = Database.FindValidResetToken(email.Trim(), HashToken(token.Trim()), out int tokenId);
            if (user == null) throw new InvalidOperationException("Kod geçersiz, kullanılmış veya süresi dolmuş.");
            ValidateNewPassword(newPassword, user.Username, null);
            if (VerifyPassword(user, newPassword)) throw new InvalidOperationException("Yeni parola mevcut parolayla aynı olamaz.");
            foreach (PasswordCredential old in Database.GetPasswordHistory(user.UserId))
                if (PasswordHasher.Verify(newPassword, old)) throw new InvalidOperationException("Son üç parolanızdan birini yeniden kullanamazsınız.");
            if (!Database.CompletePasswordReset(user.UserId, tokenId, PasswordHasher.Create(newPassword)))
                throw new InvalidOperationException("Kod geçersiz, kullanılmış veya süresi dolmuş.");
        }

        private static bool VerifyPassword(UserRecord user, string password)
        {
            return user.IsLegacyHash ? PasswordHasher.VerifyLegacySha256(password, user.PasswordHash)
                : PasswordHasher.Verify(password, new PasswordCredential { Hash=user.PasswordHash, Salt=user.PasswordSalt, Iterations=user.PasswordIterations });
        }

        private static void ValidateNewPassword(string password, string username, string fullName)
        {
            string error = PasswordPolicy.Validate(password, username, fullName);
            if (error != null) throw new InvalidOperationException(error);
        }

        private static AppUser ToAppUser(UserRecord user)
        {
            return new AppUser { UserId=user.UserId, Username=user.Username, Email=user.Email, Role=user.Role, MustChangePassword=user.MustChangePassword };
        }

        private static string CreateToken()
        {
            byte[] bytes = new byte[32];
            using (RandomNumberGenerator rng = RandomNumberGenerator.Create()) rng.GetBytes(bytes);
            return Convert.ToBase64String(bytes).TrimEnd('=').Replace('+','-').Replace('/','_');
        }

        private static byte[] HashToken(string token)
        {
            using (SHA256 sha = SHA256.Create()) return sha.ComputeHash(Encoding.UTF8.GetBytes(token));
        }
    }
}
