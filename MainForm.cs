using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using SporSalonuTakip.BLL;
using SporSalonuTakip.Entities;

namespace SporSalonuTakip
{
    public class MainForm : Form
    {
        private static class Palette
        {
            public static readonly Color Navy = Color.FromArgb(24, 37, 59);
            public static readonly Color NavyLight = Color.FromArgb(37, 54, 81);
            public static readonly Color Blue = Color.FromArgb(48, 103, 224);
            public static readonly Color Teal = Color.FromArgb(34, 190, 164);
            public static readonly Color Canvas = Color.FromArgb(244, 247, 251);
            public static readonly Color Border = Color.FromArgb(226, 232, 240);
            public static readonly Color Text = Color.FromArgb(34, 47, 68);
            public static readonly Color Muted = Color.FromArgb(119, 133, 153);
        }

        private readonly AppUser user;
        private readonly TextBox nameBox = new TextBox();
        private readonly TextBox phoneBox = new TextBox();
        private readonly ComboBox typeBox = new ComboBox();
        private readonly DateTimePicker dateBox = new DateTimePicker();
        private readonly DataGridView grid = new DataGridView();
        private readonly Label sessionStatus = new Label();
        private readonly Label memberCount = new Label();
        private readonly Label totalValue = new Label();
        private readonly Label activeValue = new Label();
        private readonly Label endingValue = new Label();
        private readonly Label expiredValue = new Label();
        private int? selectedId;
        private Button saveButton;
        private Button deleteButton;
        private Button clearButton;
        private Button usersButton;

        public MainForm(AppUser currentUser)
        {
            user = currentUser;
            Text = "Spor Salonu Üye Takip Sistemi";
            StartPosition = FormStartPosition.CenterScreen;
            ClientSize = new Size(1240, 790);
            MinimumSize = new Size(1050, 680);
            BackColor = Palette.Canvas;
            Font = new Font("Segoe UI", 9F);

            BuildInterface();

            saveButton.Enabled = deleteButton.Enabled = clearButton.Enabled = user.IsAdmin;
            usersButton.Enabled = user.IsAdmin;
            SessionManager.Warning += SessionWarning;
            SessionManager.Expired += SessionExpired;
            FormClosed += MainFormClosed;
            LoadMembers();
        }

        private void BuildInterface()
        {
            Panel sidebar = BuildSidebar();
            Panel content = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Palette.Canvas,
                Padding = new Padding(28, 22, 28, 20)
            };

            TableLayoutPanel page = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 4,
                BackColor = Palette.Canvas,
                Margin = Padding.Empty
            };
            page.RowStyles.Add(new RowStyle(SizeType.Absolute, 70));
            page.RowStyles.Add(new RowStyle(SizeType.Absolute, 118));
            page.RowStyles.Add(new RowStyle(SizeType.Absolute, 164));
            page.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            page.Controls.Add(BuildHeader(), 0, 0);
            page.Controls.Add(BuildStats(), 0, 1);
            page.Controls.Add(BuildMemberForm(), 0, 2);
            page.Controls.Add(BuildMemberList(), 0, 3);
            content.Controls.Add(page);

            Controls.Add(content);
            Controls.Add(sidebar);
        }

        private Panel BuildSidebar()
        {
            Panel sidebar = new Panel
            {
                Dock = DockStyle.Left,
                Width = 226,
                BackColor = Palette.Navy
            };

            Panel logo = new Panel
            {
                Bounds = new Rectangle(20, 26, 42, 42),
                BackColor = Palette.Teal
            };
            Label logoLetter = new Label
            {
                Text = "F",
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Segoe UI", 20F, FontStyle.Bold),
                ForeColor = Palette.Navy
            };
            logo.Controls.Add(logoLetter);
            sidebar.Controls.Add(logo);
            sidebar.Controls.Add(new Label
            {
                Text = "FITCLUB",
                Bounds = new Rectangle(73, 25, 132, 25),
                Font = new Font("Segoe UI", 14F, FontStyle.Bold),
                ForeColor = Color.White
            });
            sidebar.Controls.Add(new Label
            {
                Text = "ÜYE TAKİP SİSTEMİ",
                Bounds = new Rectangle(74, 50, 135, 18),
                Font = new Font("Segoe UI", 7.5F, FontStyle.Bold),
                ForeColor = Color.FromArgb(165, 180, 200)
            });

            Panel divider = new Panel
            {
                Bounds = new Rectangle(18, 88, 190, 1),
                BackColor = Color.FromArgb(55, 70, 94)
            };
            sidebar.Controls.Add(divider);
            sidebar.Controls.Add(new Label
            {
                Text = "KISAYOLLAR",
                Bounds = new Rectangle(22, 111, 180, 20),
                Font = new Font("Segoe UI", 8F, FontStyle.Bold),
                ForeColor = Color.FromArgb(144, 161, 183)
            });

            Button memberShortcut = MakeButton("Üye listesine git", Palette.NavyLight, Color.White);
            memberShortcut.Bounds = new Rectangle(14, 140, 198, 40);
            memberShortcut.TextAlign = ContentAlignment.MiddleLeft;
            memberShortcut.Padding = new Padding(14, 0, 0, 0);
            memberShortcut.Click += (s, e) => grid.Focus();
            sidebar.Controls.Add(memberShortcut);

            Button newShortcut = MakeButton("Yeni üye kaydı", Palette.Navy, Color.FromArgb(218, 227, 239));
            newShortcut.Bounds = new Rectangle(14, 186, 198, 40);
            newShortcut.TextAlign = ContentAlignment.MiddleLeft;
            newShortcut.Padding = new Padding(14, 0, 0, 0);
            newShortcut.FlatAppearance.MouseOverBackColor = Palette.NavyLight;
            newShortcut.Click += (s, e) => nameBox.Focus();
            sidebar.Controls.Add(newShortcut);

            Panel account = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 202,
                Padding = new Padding(15, 12, 15, 12),
                BackColor = Palette.Navy
            };
            account.Controls.Add(new Label
            {
                Text = "HESAP",
                Bounds = new Rectangle(6, 4, 178, 20),
                Font = new Font("Segoe UI", 8F, FontStyle.Bold),
                ForeColor = Color.FromArgb(144, 161, 183)
            });
            account.Controls.Add(new Label
            {
                Text = user.Username,
                Bounds = new Rectangle(6, 29, 178, 23),
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = Color.White,
                AutoEllipsis = true
            });
            account.Controls.Add(new Label
            {
                Text = user.Role,
                Bounds = new Rectangle(6, 51, 178, 18),
                Font = new Font("Segoe UI", 8.5F),
                ForeColor = Color.FromArgb(165, 180, 200)
            });

            Button changePassword = MakeButton("Parolamı değiştir", Palette.NavyLight, Color.White);
            changePassword.Bounds = new Rectangle(4, 82, 178, 34);
            changePassword.Click += ChangePasswordClick;
            account.Controls.Add(changePassword);

            usersButton = MakeButton("Kullanıcılar / Roller", Palette.Navy, Color.FromArgb(218, 227, 239));
            usersButton.Bounds = new Rectangle(4, 122, 178, 34);
            usersButton.FlatAppearance.BorderSize = 1;
            usersButton.FlatAppearance.BorderColor = Color.FromArgb(72, 88, 112);
            usersButton.Click += UsersClick;
            account.Controls.Add(usersButton);
            account.Controls.Add(new Label
            {
                Text = "Güvenli oturum açık",
                Bounds = new Rectangle(6, 168, 178, 18),
                Font = new Font("Segoe UI", 8F),
                ForeColor = Color.FromArgb(144, 161, 183)
            });
            sidebar.Controls.Add(account);
            return sidebar;
        }

        private Control BuildHeader()
        {
            Panel header = new Panel { Dock = DockStyle.Fill, BackColor = Palette.Canvas };
            header.Controls.Add(new Label
            {
                Text = "Genel Bakış",
                Location = new Point(2, 1),
                AutoSize = true,
                Font = new Font("Segoe UI", 21F, FontStyle.Bold),
                ForeColor = Palette.Text
            });
            header.Controls.Add(new Label
            {
                Text = "Üyeliklerini ve kayıtlarını tek ekrandan takip et.",
                Location = new Point(4, 39),
                AutoSize = true,
                Font = new Font("Segoe UI", 9F),
                ForeColor = Palette.Muted
            });
            Label session = new Label
            {
                Text = "Merhaba, " + user.Username,
                AutoSize = true,
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                ForeColor = Palette.Text
            };
            header.Controls.Add(session);
            header.Resize += (s, e) => session.Location = new Point(Math.Max(0, header.ClientSize.Width - session.Width - 4), 20);
            return header;
        }

        private Control BuildStats()
        {
            TableLayoutPanel stats = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 4,
                RowCount = 1,
                BackColor = Palette.Canvas,
                Margin = Padding.Empty
            };
            for (int i = 0; i < 4; i++) stats.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));

            stats.Controls.Add(CreateStatCard("TOPLAM ÜYE", totalValue, Palette.Blue), 0, 0);
            stats.Controls.Add(CreateStatCard("AKTİF ÜYE", activeValue, Color.FromArgb(39, 165, 112)), 1, 0);
            stats.Controls.Add(CreateStatCard("YAKINDA BİTİYOR", endingValue, Color.FromArgb(222, 158, 45)), 2, 0);
            stats.Controls.Add(CreateStatCard("SÜRESİ DOLMUŞ", expiredValue, Color.FromArgb(218, 92, 92)), 3, 0);
            return stats;
        }

        private Control CreateStatCard(string title, Label value, Color accent)
        {
            Panel card = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Margin = new Padding(5, 3, 5, 8),
                Padding = new Padding(17, 13, 12, 10)
            };
            StyleCard(card);
            Panel stripe = new Panel { Dock = DockStyle.Left, Width = 4, BackColor = accent };
            Label caption = new Label
            {
                Text = title,
                Dock = DockStyle.Top,
                Height = 20,
                Font = new Font("Segoe UI", 8F, FontStyle.Bold),
                ForeColor = Palette.Muted
            };
            value.Text = "0";
            value.Dock = DockStyle.Fill;
            value.Font = new Font("Segoe UI", 21F, FontStyle.Bold);
            value.ForeColor = Palette.Text;
            value.TextAlign = ContentAlignment.MiddleLeft;
            card.Controls.Add(value);
            card.Controls.Add(caption);
            card.Controls.Add(stripe);
            return card;
        }

        private Control BuildMemberForm()
        {
            Panel card = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Margin = new Padding(5, 0, 5, 10),
                Padding = new Padding(17, 12, 17, 12)
            };
            StyleCard(card);
            card.Controls.Add(new Label
            {
                Text = "Üye bilgileri",
                Location = new Point(17, 10),
                AutoSize = true,
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                ForeColor = Palette.Text
            });
            card.Controls.Add(new Label
            {
                Text = "Yeni kayıt oluştur veya listeden seçili üyeyi güncelle.",
                Location = new Point(118, 13),
                AutoSize = true,
                Font = new Font("Segoe UI", 8.5F),
                ForeColor = Palette.Muted
            });

            typeBox.DropDownStyle = ComboBoxStyle.DropDownList;
            typeBox.Items.AddRange(new object[] { "Aylık", "3 Aylık", "Yıllık" });
            typeBox.SelectedIndex = 0;
            dateBox.Format = DateTimePickerFormat.Short;
            dateBox.Value = DateTime.Today;
            StyleInput(nameBox);
            StyleInput(phoneBox);
            StyleInput(typeBox);
            StyleInput(dateBox);

            TableLayoutPanel fields = new TableLayoutPanel
            {
                Bounds = new Rectangle(17, 43, 900, 78),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom,
                ColumnCount = 5,
                RowCount = 1,
                Margin = Padding.Empty
            };
            fields.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 29F));
            fields.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 17F));
            fields.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 18F));
            fields.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 18F));
            fields.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 18F));
            fields.Controls.Add(CreateField("AD SOYAD", nameBox), 0, 0);
            fields.Controls.Add(CreateField("TELEFON", phoneBox), 1, 0);
            fields.Controls.Add(CreateField("ÜYELİK TİPİ", typeBox), 2, 0);
            fields.Controls.Add(CreateField("BAŞLANGIÇ TARİHİ", dateBox), 3, 0);

            Panel actions = new Panel { Dock = DockStyle.Fill, Margin = new Padding(4, 0, 0, 0) };
            saveButton = MakeButton("Kaydet", Palette.Blue, Color.White);
            saveButton.SetBounds(0, 22, 86, 34);
            saveButton.Anchor = AnchorStyles.Top | AnchorStyles.Left;
            saveButton.Click += SaveClick;
            clearButton = MakeButton("Temizle", Color.FromArgb(237, 241, 247), Palette.Text);
            clearButton.SetBounds(91, 22, 77, 34);
            clearButton.Anchor = AnchorStyles.Top | AnchorStyles.Left;
            clearButton.Click += (s, e) => ClearForm();
            actions.Controls.Add(saveButton);
            actions.Controls.Add(clearButton);
            fields.Controls.Add(actions, 4, 0);
            card.Controls.Add(fields);
            card.Resize += (s, e) => fields.Width = Math.Max(100, card.ClientSize.Width - 34);
            return card;
        }

        private Control CreateField(string caption, Control input)
        {
            Panel field = new Panel { Dock = DockStyle.Fill, Margin = new Padding(0, 0, 9, 0) };
            field.Controls.Add(new Label
            {
                Text = caption,
                Location = new Point(0, 0),
                AutoSize = true,
                Font = new Font("Segoe UI", 7.5F, FontStyle.Bold),
                ForeColor = Palette.Muted
            });
            input.SetBounds(0, 21, 160, 32);
            input.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            field.Controls.Add(input);
            field.Resize += (s, e) => input.Width = Math.Max(70, field.ClientSize.Width);
            return field;
        }

        private Control BuildMemberList()
        {
            Panel card = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Margin = new Padding(5, 0, 5, 0),
                Padding = new Padding(12)
            };
            StyleCard(card);

            TableLayoutPanel layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 3,
                Margin = Padding.Empty
            };
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 43));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 42));

            Panel listHeader = new Panel { Dock = DockStyle.Fill };
            listHeader.Controls.Add(new Label
            {
                Text = "Üye kayıtları",
                Location = new Point(3, 3),
                AutoSize = true,
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                ForeColor = Palette.Text
            });
            memberCount.Text = "0 üye";
            memberCount.AutoSize = true;
            memberCount.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            memberCount.Font = new Font("Segoe UI", 8.5F, FontStyle.Bold);
            memberCount.ForeColor = Palette.Muted;
            listHeader.Controls.Add(memberCount);
            listHeader.Resize += (s, e) => memberCount.Location = new Point(Math.Max(0, listHeader.ClientSize.Width - memberCount.Width - 4), 8);

            ConfigureGrid();
            Panel footer = new Panel { Dock = DockStyle.Fill };
            sessionStatus.Text = "Üyelik durumları tarihe göre takip ediliyor.";
            sessionStatus.SetBounds(3, 10, 440, 22);
            sessionStatus.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            sessionStatus.Font = new Font("Segoe UI", 8F);
            sessionStatus.ForeColor = Palette.Muted;
            footer.Controls.Add(sessionStatus);

            FlowLayoutPanel actions = new FlowLayoutPanel
            {
                Dock = DockStyle.Right,
                Width = 250,
                FlowDirection = FlowDirection.RightToLeft,
                WrapContents = false,
                Padding = Padding.Empty,
                Margin = Padding.Empty
            };
            Button refresh = MakeButton("Yenile", Color.FromArgb(237, 241, 247), Palette.Text);
            refresh.Size = new Size(92, 32);
            refresh.Margin = new Padding(0, 4, 0, 0);
            refresh.Click += (s, e) => LoadMembers();
            deleteButton = MakeButton("Üyeyi sil", Color.FromArgb(255, 239, 239), Color.FromArgb(177, 55, 55));
            deleteButton.Size = new Size(112, 32);
            deleteButton.Margin = new Padding(0, 4, 8, 0);
            deleteButton.Click += DeleteClick;
            actions.Controls.Add(refresh);
            actions.Controls.Add(deleteButton);
            footer.Controls.Add(actions);

            layout.Controls.Add(listHeader, 0, 0);
            layout.Controls.Add(grid, 0, 1);
            layout.Controls.Add(footer, 0, 2);
            card.Controls.Add(layout);
            return card;
        }

        private void ConfigureGrid()
        {
            grid.Dock = DockStyle.Fill;
            grid.BackgroundColor = Color.White;
            grid.BorderStyle = BorderStyle.None;
            grid.GridColor = Palette.Border;
            grid.EnableHeadersVisualStyles = false;
            grid.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single;
            grid.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(247, 249, 252);
            grid.ColumnHeadersDefaultCellStyle.ForeColor = Palette.Muted;
            grid.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 8F, FontStyle.Bold);
            grid.ColumnHeadersHeight = 38;
            grid.RowHeadersVisible = false;
            grid.DefaultCellStyle.Font = new Font("Segoe UI", 9F);
            grid.DefaultCellStyle.ForeColor = Palette.Text;
            grid.DefaultCellStyle.BackColor = Color.White;
            grid.DefaultCellStyle.SelectionBackColor = Color.FromArgb(226, 237, 255);
            grid.DefaultCellStyle.SelectionForeColor = Palette.Text;
            grid.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(250, 251, 253);
            grid.RowTemplate.Height = 34;
            grid.AllowUserToAddRows = false;
            grid.AllowUserToDeleteRows = false;
            grid.AllowUserToResizeRows = false;
            grid.ReadOnly = true;
            grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            grid.MultiSelect = false;
            grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            grid.AutoGenerateColumns = true;
            grid.CellFormatting += GridCellFormatting;
            grid.SelectionChanged += GridSelectionChanged;
        }

        private void StyleInput(Control input)
        {
            input.Font = new Font("Segoe UI", 9.5F);
            if (input is TextBox) ((TextBox)input).BorderStyle = BorderStyle.FixedSingle;
            if (input is ComboBox) ((ComboBox)input).FlatStyle = FlatStyle.Flat;
        }

        private Button MakeButton(string text, Color background, Color foreground)
        {
            Button button = new Button
            {
                Text = text,
                BackColor = background,
                ForeColor = foreground,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
                Cursor = Cursors.Hand,
                TextAlign = ContentAlignment.MiddleCenter
            };
            button.FlatAppearance.BorderSize = 0;
            button.FlatAppearance.MouseOverBackColor = ControlPaint.Light(background, 0.08F);
            button.FlatAppearance.MouseDownBackColor = ControlPaint.Dark(background, 0.08F);
            return button;
        }

        private void StyleCard(Control control)
        {
            control.Paint += (s, e) =>
            {
                using (Pen pen = new Pen(Palette.Border))
                    e.Graphics.DrawRectangle(pen, 0, 0, Math.Max(0, control.Width - 1), Math.Max(0, control.Height - 1));
            };
        }

        private void LoadMembers()
        {
            try
            {
                DataTable members = MemberService.GetMembers(user);
                grid.DataSource = members;
                SetColumn("MemberId", "No", 42F);
                SetColumn("FullName", "Ad Soyad", 120F);
                SetColumn("Phone", "Telefon", 88F);
                SetColumn("MembershipType", "Üyelik", 85F);
                SetColumn("StartDate", "Başlangıç", 90F);
                SetColumn("EndDate", "Bitiş tarihi", 90F);
                SetColumn("MembershipStatus", "Durum", 100F);
                if (grid.Columns.Contains("StartDate")) grid.Columns["StartDate"].DefaultCellStyle.Format = "dd.MM.yyyy";
                if (grid.Columns.Contains("EndDate")) grid.Columns["EndDate"].DefaultCellStyle.Format = "dd.MM.yyyy";
                memberCount.Text = members.Rows.Count + " üye";
                UpdateStats(members);
                grid.ClearSelection();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Üyeler yüklenemedi: " + ex.Message, "Liste yüklenemedi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void SetColumn(string name, string header, float weight)
        {
            if (!grid.Columns.Contains(name)) return;
            grid.Columns[name].HeaderText = header;
            grid.Columns[name].FillWeight = weight;
            grid.Columns[name].MinimumWidth = name == "FullName" ? 120 : 70;
        }

        private void UpdateStats(DataTable members)
        {
            int active = 0;
            int ending = 0;
            int expired = 0;
            foreach (DataRow row in members.Rows)
            {
                if (!members.Columns.Contains("MembershipStatus")) break;
                string status = Convert.ToString(row["MembershipStatus"]);
                if (status == "Aktif") active++;
                else if (status == "Yakında bitiyor") ending++;
                else if (status == "Süresi dolmuş") expired++;
            }
            totalValue.Text = members.Rows.Count.ToString();
            activeValue.Text = active.ToString();
            endingValue.Text = ending.ToString();
            expiredValue.Text = expired.ToString();
        }

        private void SaveClick(object sender, EventArgs e)
        {
            try
            {
                MemberService.SaveMember(user, selectedId, nameBox.Text, phoneBox.Text, typeBox.Text, dateBox.Value.Date);
                MessageBox.Show(selectedId.HasValue ? "Üye güncellendi." : "Üye eklendi.", "Tamamlandı", MessageBoxButtons.OK, MessageBoxIcon.Information);
                ClearForm();
                LoadMembers();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Kayıt yapılamadı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void DeleteClick(object sender, EventArgs e)
        {
            if (selectedId == null)
            {
                MessageBox.Show("Önce listeden bir üye seçin.", "Üye seçilmedi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            if (MessageBox.Show("Seçili üyeyi silmek istiyor musunuz?", "Silme onayı", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
            try
            {
                MemberService.DeleteMember(user, selectedId.Value);
                ClearForm();
                LoadMembers();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Silinemedi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void GridSelectionChanged(object sender, EventArgs e)
        {
            if (grid.SelectedRows.Count == 0 || grid.CurrentRow == null || grid.CurrentRow.DataBoundItem == null) return;
            DataRowView row = grid.CurrentRow.DataBoundItem as DataRowView;
            if (row == null) return;
            selectedId = Convert.ToInt32(row["MemberId"]);
            nameBox.Text = Convert.ToString(row["FullName"]);
            phoneBox.Text = Convert.ToString(row["Phone"]);
            typeBox.Text = Convert.ToString(row["MembershipType"]);
            dateBox.Value = Convert.ToDateTime(row["StartDate"]);
            saveButton.Text = "Güncelle";
        }

        private void GridCellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0 || grid.Columns[e.ColumnIndex].Name != "MembershipStatus") return;
            string status = Convert.ToString(e.Value);
            if (status == "Süresi dolmuş")
            {
                e.CellStyle.ForeColor = Color.FromArgb(166, 55, 55);
                e.CellStyle.BackColor = Color.FromArgb(255, 239, 239);
            }
            else if (status == "Yakında bitiyor")
            {
                e.CellStyle.ForeColor = Color.FromArgb(157, 105, 17);
                e.CellStyle.BackColor = Color.FromArgb(255, 247, 224);
            }
            else if (status == "Aktif")
            {
                e.CellStyle.ForeColor = Color.FromArgb(31, 126, 82);
                e.CellStyle.BackColor = Color.FromArgb(232, 248, 239);
            }
        }

        private void ClearForm()
        {
            selectedId = null;
            nameBox.Clear();
            phoneBox.Clear();
            typeBox.SelectedIndex = 0;
            dateBox.Value = DateTime.Today;
            grid.ClearSelection();
            saveButton.Text = "Kaydet";
        }

        private void ChangePasswordClick(object sender, EventArgs e)
        {
            using (ChangePasswordForm form = new ChangePasswordForm(user, null, false)) form.ShowDialog(this);
        }

        private void UsersClick(object sender, EventArgs e)
        {
            if (!user.IsAdmin)
            {
                MessageBox.Show("Bu işlem yalnızca Admin rolüne açıktır.");
                return;
            }
            using (UserManagementForm form = new UserManagementForm(user)) form.ShowDialog(this);
        }

        private void SessionWarning(object sender, EventArgs e)
        {
            sessionStatus.Text = "Uzun süredir işlem yapılmadı. Oturumunuz 1 dakika içinde kapatılacak.";
            sessionStatus.ForeColor = Color.FromArgb(177, 109, 20);
        }

        private void SessionExpired(object sender, EventArgs e)
        {
            sessionStatus.Text = "Güvenlik nedeniyle oturum kapatıldı.";
            MessageBox.Show("15 dakika işlem yapılmadığı için oturum kapatıldı.", "Oturum süresi", MessageBoxButtons.OK, MessageBoxIcon.Information);
            Close();
        }

        private void MainFormClosed(object sender, FormClosedEventArgs e)
        {
            SessionManager.Warning -= SessionWarning;
            SessionManager.Expired -= SessionExpired;
            SessionManager.Stop();
        }
    }
}
