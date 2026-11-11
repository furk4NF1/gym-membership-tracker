using System;
using System.Drawing;
using System.Windows.Forms;
using SporSalonuTakip.BLL;
using SporSalonuTakip.Entities;

namespace SporSalonuTakip
{
    public class ChangePasswordForm : Form
    {
        private readonly AppUser user;
        private readonly TextBox current = new TextBox();
        private readonly TextBox next = new TextBox();
        private readonly TextBox confirm = new TextBox();

        public ChangePasswordForm(AppUser user, string currentPassword, bool required)
        {
            this.user = user;
            Text = required ? "İlk Giriş - Parola Değiştir" : "Parolamı Değiştir";
            StartPosition = FormStartPosition.CenterParent; ClientSize = new Size(430, 300); FormBorderStyle = FormBorderStyle.FixedDialog; MaximizeBox = false; MinimizeBox = false;
            Controls.Add(new Label { Text = required ? "Güvenliğiniz için geçici parolanızı değiştirin." : "Yeni parolanızı belirleyin.", Location = new Point(22, 18), AutoSize = true });
            AddField("Mevcut parola", current, 58); current.Text = currentPassword ?? ""; current.UseSystemPasswordChar = true; current.ReadOnly = required;
            AddField("Yeni parola", next, 112); next.UseSystemPasswordChar = true;
            AddField("Yeni parola (tekrar)", confirm, 166); confirm.UseSystemPasswordChar = true;
            Controls.Add(new Label { Text = "En az 6 karakter; büyük/küçük harf, rakam ve özel karakter.", Location = new Point(22, 216), AutoSize = true });
            Button save = new Button { Text = "Parolayı Değiştir", Bounds = new Rectangle(22, 250, 180, 34) }; save.Click += SaveClick; Controls.Add(save);
            Button cancel = new Button { Text = required ? "Çıkış" : "İptal", Bounds = new Rectangle(218, 250, 150, 34) }; cancel.Click += (s,e) => { DialogResult = DialogResult.Cancel; Close(); }; Controls.Add(cancel);
            AcceptButton = save; CancelButton = cancel;
        }

        private void AddField(string label, TextBox box, int y)
        {
            Controls.Add(new Label { Text = label, Location = new Point(22, y), AutoSize = true });
            box.SetBounds(190, y - 4, 210, 28); Controls.Add(box);
        }

        private void SaveClick(object sender, EventArgs e)
        {
            if (next.Text != confirm.Text) { MessageBox.Show("Yeni parola alanları aynı değil."); return; }
            try
            {
                AuthService.ChangePassword(user, current.Text, next.Text);
                MessageBox.Show("Parolanız değiştirildi.", "Tamam", MessageBoxButtons.OK, MessageBoxIcon.Information);
                DialogResult = DialogResult.OK; Close();
            }
            catch (Exception ex) { MessageBox.Show(ex.Message, "Parola değiştirilemedi", MessageBoxButtons.OK, MessageBoxIcon.Warning); }
        }
    }
}
