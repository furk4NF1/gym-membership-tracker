using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using SporSalonuTakip.BLL;
using SporSalonuTakip.Entities;

namespace SporSalonuTakip
{
    public class UserManagementForm : Form
    {
        private readonly AppUser actor;
        private readonly DataGridView grid = new DataGridView();
        private readonly TextBox username = new TextBox();
        private readonly TextBox email = new TextBox();
        private readonly ComboBox role = new ComboBox();

        public UserManagementForm(AppUser actor)
        {
            this.actor = actor;
            Text = "Kullanıcı ve Rol Yönetimi"; StartPosition = FormStartPosition.CenterParent; ClientSize = new Size(760, 490); MinimumSize = new Size(700, 440);
            Controls.Add(new Label { Text = "Yeni kullanıcı", Font = new Font("Segoe UI", 12, FontStyle.Bold), Location = new Point(20, 15), AutoSize = true });
            AddLabel("Kullanıcı adı", 20, 52); username.SetBounds(20, 74, 170, 27); Controls.Add(username);
            AddLabel("E-posta", 205, 52); email.SetBounds(205, 74, 220, 27); Controls.Add(email);
            AddLabel("Rol", 440, 52); role.SetBounds(440, 74, 115, 27); role.DropDownStyle = ComboBoxStyle.DropDownList; role.Items.AddRange(new object[] { "User", "Admin" }); role.SelectedIndex = 0; Controls.Add(role);
            Button add = new Button { Text = "Kullanıcı Oluştur", Bounds = new Rectangle(570, 70, 165, 34) }; add.Click += AddClick; Controls.Add(add);
            grid.SetBounds(20, 125, 715, 275); grid.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right; grid.ReadOnly = true; grid.AllowUserToAddRows = false; grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect; grid.MultiSelect = false; grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill; Controls.Add(grid);
            Button change = new Button { Text = "Seçili Kullanıcının Rolünü Değiştir", Bounds = new Rectangle(20, 415, 270, 35), Anchor = AnchorStyles.Bottom | AnchorStyles.Left }; change.Click += ChangeRoleClick; Controls.Add(change);
            LoadUsers();
        }

        private void AddLabel(string text, int x, int y) { Controls.Add(new Label { Text = text, Location = new Point(x,y), AutoSize = true }); }

        private void LoadUsers()
        {
            grid.DataSource = UserService.GetUsers(actor);
            if (grid.Columns.Contains("UserId")) grid.Columns["UserId"].HeaderText = "No";
            if (grid.Columns.Contains("Username")) grid.Columns["Username"].HeaderText = "Kullanıcı adı";
            if (grid.Columns.Contains("Email")) grid.Columns["Email"].HeaderText = "E-posta";
            if (grid.Columns.Contains("Role")) grid.Columns["Role"].HeaderText = "Rol";
            if (grid.Columns.Contains("IsActive")) grid.Columns["IsActive"].HeaderText = "Etkin";
            if (grid.Columns.Contains("MustChangePassword")) grid.Columns["MustChangePassword"].HeaderText = "İlk giriş değişimi";
        }

        private void AddClick(object sender, EventArgs e)
        {
            try
            {
                string temporary = UserService.CreateUser(actor, username.Text, email.Text, role.Text);
                MessageBox.Show("Kullanıcı oluşturuldu. Geçici parolayı kullanıcıya güvenli biçimde iletin:\n\n" + temporary + "\n\nİlk girişte yeni parola belirlemesi istenir.", "Geçici parola", MessageBoxButtons.OK, MessageBoxIcon.Information);
                username.Clear(); email.Clear(); role.SelectedIndex = 0; LoadUsers();
            }
            catch (Exception ex) { MessageBox.Show(ex.Message, "Kullanıcı oluşturulamadı", MessageBoxButtons.OK, MessageBoxIcon.Warning); }
        }

        private void ChangeRoleClick(object sender, EventArgs e)
        {
            if (grid.CurrentRow == null) { MessageBox.Show("Önce bir kullanıcı seçin."); return; }
            int id = Convert.ToInt32(grid.CurrentRow.Cells["UserId"].Value);
            string oldRole = Convert.ToString(grid.CurrentRow.Cells["Role"].Value);
            string newRole = oldRole == "Admin" ? "User" : "Admin";
            try { UserService.ChangeRole(actor, id, newRole); MessageBox.Show("Rol güncellendi: " + newRole); LoadUsers(); }
            catch (Exception ex) { MessageBox.Show(ex.Message, "Rol değiştirilemedi", MessageBoxButtons.OK, MessageBoxIcon.Warning); }
        }
    }
}
