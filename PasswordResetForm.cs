using System;
using System.Drawing;
using System.Windows.Forms;
using SporSalonuTakip.BLL;

namespace SporSalonuTakip
{
    public class PasswordResetForm : Form
    {
        private readonly TextBox email = new TextBox();
        private readonly TextBox token = new TextBox();
        private readonly TextBox password = new TextBox();
        private readonly TextBox confirm = new TextBox();

        public PasswordResetForm()
        {
            Text = "Parola Sıfırlama"; StartPosition = FormStartPosition.CenterParent; ClientSize = new Size(470, 330); FormBorderStyle = FormBorderStyle.FixedDialog; MaximizeBox = false; MinimizeBox = false;
            Controls.Add(new Label { Text = "Kayıtlı e-posta adresinize tek kullanımlık kod gönderilir.", Location = new Point(20, 18), AutoSize = true });
            AddField("E-posta", email, 58); AddField("E-posta kodu", token, 108); AddField("Yeni parola", password, 158); AddField("Yeni parola (tekrar)", confirm, 208);
            password.UseSystemPasswordChar = true; confirm.UseSystemPasswordChar = true;
            Button send = new Button { Text = "Kodu Gönder", Bounds = new Rectangle(20, 260, 135, 35) }; send.Click += SendClick; Controls.Add(send);
            Button reset = new Button { Text = "Parolayı Sıfırla", Bounds = new Rectangle(170, 260, 145, 35) }; reset.Click += ResetClick; Controls.Add(reset);
            Button cancel = new Button { Text = "Kapat", Bounds = new Rectangle(330, 260, 110, 35) }; cancel.Click += (s,e) => Close(); Controls.Add(cancel);
            Controls.Add(new Label { Text = "Kod 15 dakika geçerlidir ve yalnızca bir kez kullanılabilir.", Location = new Point(20, 305), AutoSize = true });
        }

        private void AddField(string label, TextBox box, int y)
        {
            Controls.Add(new Label { Text = label, Location = new Point(20, y), AutoSize = true });
            box.SetBounds(175, y - 4, 265, 28); Controls.Add(box);
        }

        private void SendClick(object sender, EventArgs e)
        {
            try
            {
                AuthService.RequestPasswordReset(email.Text.Trim());
                MessageBox.Show("Adres sistemde kayıtlıysa sıfırlama kodu gönderilmiştir.", "İstek alındı", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex) { MessageBox.Show(ex.Message, "Kod gönderilemedi", MessageBoxButtons.OK, MessageBoxIcon.Warning); }
        }

        private void ResetClick(object sender, EventArgs e)
        {
            if (password.Text != confirm.Text) { MessageBox.Show("Yeni parola alanları aynı değil."); return; }
            try
            {
                AuthService.ResetPassword(email.Text.Trim(), token.Text.Trim(), password.Text);
                MessageBox.Show("Parolanız yenilendi. Yeni parolanızla giriş yapabilirsiniz.", "Tamam", MessageBoxButtons.OK, MessageBoxIcon.Information);
                Close();
            }
            catch (Exception ex) { MessageBox.Show(ex.Message, "Parola sıfırlanamadı", MessageBoxButtons.OK, MessageBoxIcon.Warning); }
        }
    }
}
