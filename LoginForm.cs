using System;
using System.Drawing;
using System.Windows.Forms;
using SporSalonuTakip.BLL;
using SporSalonuTakip.Entities;

namespace SporSalonuTakip
{
    public class LoginForm : Form
    {
        private static class Palette
        {
            public static readonly Color Navy = Color.FromArgb(24, 37, 59);
            public static readonly Color NavyLight = Color.FromArgb(37, 54, 81);
            public static readonly Color Teal = Color.FromArgb(34, 190, 164);
            public static readonly Color Blue = Color.FromArgb(48, 103, 224);
            public static readonly Color Canvas = Color.FromArgb(244, 247, 251);
            public static readonly Color Text = Color.FromArgb(34, 47, 68);
            public static readonly Color Muted = Color.FromArgb(119, 133, 153);
            public static readonly Color Border = Color.FromArgb(220, 227, 237);
        }

        private readonly TextBox username = new TextBox();
        private readonly TextBox password = new TextBox();

        public LoginForm()
        {
            Text = "Spor Salonu - Giriş";
            StartPosition = FormStartPosition.CenterScreen;
            ClientSize = new Size(790, 510);
            MinimumSize = new Size(790, 510);
            MaximumSize = new Size(790, 510);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            BackColor = Color.White;
            Font = new Font("Segoe UI", 9F);

            BuildInterface();
        }

        private void BuildInterface()
        {
            TableLayoutPanel layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                Margin = Padding.Empty,
                BackColor = Color.White
            };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 255));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

            Panel brandPanel = BuildBrandPanel();
            Panel loginPanel = BuildLoginPanel();
            layout.Controls.Add(brandPanel, 0, 0);
            layout.Controls.Add(loginPanel, 1, 0);
            Controls.Add(layout);
        }

        private Panel BuildBrandPanel()
        {
            Panel brand = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Palette.Navy
            };

            Panel mark = new Panel
            {
                Bounds = new Rectangle(32, 38, 54, 54),
                BackColor = Palette.Teal
            };
            mark.Controls.Add(new Label
            {
                Text = "F",
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Segoe UI", 26F, FontStyle.Bold),
                ForeColor = Palette.Navy
            });
            brand.Controls.Add(mark);

            brand.Controls.Add(new Label
            {
                Text = "FITCLUB",
                Location = new Point(32, 125),
                AutoSize = true,
                Font = new Font("Segoe UI", 23F, FontStyle.Bold),
                ForeColor = Color.White
            });
            brand.Controls.Add(new Label
            {
                Text = "ÜYE TAKİP SİSTEMİ",
                Location = new Point(34, 164),
                AutoSize = true,
                Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
                ForeColor = Palette.Teal
            });
            brand.Controls.Add(new Label
            {
                Text = "Üyelikleri düzenle.\nDurumları kolayca izle.\nHer şey tek ekranda.",
                Location = new Point(34, 220),
                AutoSize = true,
                Font = new Font("Segoe UI", 10F),
                ForeColor = Color.FromArgb(199, 210, 224)
            });

            Panel accent = new Panel
            {
                Bounds = new Rectangle(34, 344, 44, 3),
                BackColor = Palette.Teal
            };
            brand.Controls.Add(accent);
            brand.Controls.Add(new Label
            {
                Text = "Yönetim paneline\ngüvenli giriş",
                Location = new Point(34, 367),
                AutoSize = true,
                Font = new Font("Segoe UI", 9F),
                ForeColor = Color.FromArgb(151, 169, 191)
            });
            return brand;
        }

        private Panel BuildLoginPanel()
        {
            Panel panel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Padding = new Padding(50, 52, 50, 35)
            };

            panel.Controls.Add(new Label
            {
                Text = "Tekrar hoş geldin",
                Location = new Point(50, 55),
                AutoSize = true,
                Font = new Font("Segoe UI", 21F, FontStyle.Bold),
                ForeColor = Palette.Text
            });
            panel.Controls.Add(new Label
            {
                Text = "Devam etmek için hesabınla giriş yap.",
                Location = new Point(52, 96),
                AutoSize = true,
                Font = new Font("Segoe UI", 9F),
                ForeColor = Palette.Muted
            });

            AddFieldLabel(panel, "KULLANICI ADI", 52, 152);
            username.SetBounds(52, 176, 380, 38);
            username.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            username.Font = new Font("Segoe UI", 10F);
            username.BorderStyle = BorderStyle.FixedSingle;
            username.MaxLength = 50;
            panel.Controls.Add(username);

            AddFieldLabel(panel, "PAROLA", 52, 231);
            password.SetBounds(52, 255, 380, 38);
            password.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            password.Font = new Font("Segoe UI", 10F);
            password.BorderStyle = BorderStyle.FixedSingle;
            password.UseSystemPasswordChar = true;
            password.MaxLength = 128;
            panel.Controls.Add(password);

            Button login = MakeButton("Giriş yap", Palette.Blue, Color.White);
            login.SetBounds(52, 323, 185, 42);
            login.Anchor = AnchorStyles.Top | AnchorStyles.Left;
            login.Click += LoginClick;
            panel.Controls.Add(login);

            Button cancel = MakeButton("Çıkış", Color.FromArgb(237, 241, 247), Palette.Text);
            cancel.SetBounds(247, 323, 185, 42);
            cancel.Anchor = AnchorStyles.Top | AnchorStyles.Left;
            cancel.Click += (s, e) => Close();
            panel.Controls.Add(cancel);

            Button reset = new Button
            {
                Text = "Parolanı mı unuttun?",
                Location = new Point(52, 389),
                AutoSize = true,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.White,
                ForeColor = Palette.Blue,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Cursor = Cursors.Hand,
                TextAlign = ContentAlignment.MiddleLeft
            };
            reset.FlatAppearance.BorderSize = 0;
            reset.FlatAppearance.MouseOverBackColor = Color.White;
            reset.Click += (s, e) =>
            {
                using (PasswordResetForm form = new PasswordResetForm()) form.ShowDialog(this);
            };
            panel.Controls.Add(reset);

            AcceptButton = login;
            CancelButton = cancel;
            return panel;
        }

        private void AddFieldLabel(Control parent, string text, int x, int y)
        {
            parent.Controls.Add(new Label
            {
                Text = text,
                Location = new Point(x, y),
                AutoSize = true,
                Font = new Font("Segoe UI", 8F, FontStyle.Bold),
                ForeColor = Palette.Muted
            });
        }

        private Button MakeButton(string text, Color background, Color foreground)
        {
            Button button = new Button
            {
                Text = text,
                BackColor = background,
                ForeColor = foreground,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            button.FlatAppearance.BorderSize = 0;
            button.FlatAppearance.MouseOverBackColor = ControlPaint.Light(background, 0.08F);
            button.FlatAppearance.MouseDownBackColor = ControlPaint.Dark(background, 0.08F);
            return button;
        }

        private void LoginClick(object sender, EventArgs e)
        {
            try
            {
                AppUser user = AuthService.Login(username.Text.Trim(), password.Text);
                if (user == null)
                {
                    MessageBox.Show("Kullanıcı adı veya parola hatalı.", "Giriş başarısız", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                if (user.MustChangePassword)
                {
                    using (ChangePasswordForm change = new ChangePasswordForm(user, password.Text, true))
                        if (change.ShowDialog(this) != DialogResult.OK) return;
                }
                SessionManager.Start();
                Hide();
                using (MainForm form = new MainForm(user)) form.ShowDialog();
                SessionManager.Stop();
                password.Clear();
                Show();
                Activate();
            }
            catch (Exception ex)
            {
                MessageBox.Show("İşlem tamamlanamadı. SQL Server bağlantısını ve kurulum betiğini kontrol edin.\n\n" + ex.Message, "Bağlantı / giriş hatası", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
