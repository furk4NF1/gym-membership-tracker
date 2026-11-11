using System;
using System.Configuration;
using System.Net;
using System.Net.Mail;

namespace SporSalonuTakip.Helpers
{
    internal static class EmailSender
    {
        public static void EnsureConfigured()
        {
            int port;
            if (String.IsNullOrWhiteSpace(ConfigurationManager.AppSettings["SmtpHost"]) ||
                String.IsNullOrWhiteSpace(ConfigurationManager.AppSettings["SmtpFrom"]) ||
                !Int32.TryParse(ConfigurationManager.AppSettings["SmtpPort"], out port))
                throw new InvalidOperationException("Önce App.config içindeki SMTP ayarlarını doldurun.");
        }

        public static void SendResetCode(string recipient, string token)
        {
            EnsureConfigured();
            string host = ConfigurationManager.AppSettings["SmtpHost"];
            string from = ConfigurationManager.AppSettings["SmtpFrom"];
            int port;
            Int32.TryParse(ConfigurationManager.AppSettings["SmtpPort"], out port);

            using (MailMessage message = new MailMessage(from, recipient))
            {
                message.Subject = "Spor Salonu Üye Takip - Parola sıfırlama kodu";
                message.Body = "Parola sıfırlama kodunuz: " + token + "\r\nKod 15 dakika geçerlidir ve bir kez kullanılabilir. Bu isteği siz yapmadıysanız bu iletiyi yok sayın.";
                using (SmtpClient client = new SmtpClient(host, port))
                {
                    client.EnableSsl = String.Equals(ConfigurationManager.AppSettings["SmtpEnableSsl"], "true", StringComparison.OrdinalIgnoreCase);
                    string username = ConfigurationManager.AppSettings["SmtpUsername"];
                    string password = ConfigurationManager.AppSettings["SmtpPassword"];
                    if (!String.IsNullOrWhiteSpace(username)) client.Credentials = new NetworkCredential(username, password);
                    client.Send(message);
                }
            }
        }
    }
}
