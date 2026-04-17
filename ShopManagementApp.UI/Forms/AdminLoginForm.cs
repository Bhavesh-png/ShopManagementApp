using ShopManagementApp.Business.Services;
using ShopManagementApp.Utils;

namespace ShopManagementApp.UI.Forms
{
    /// <summary>
    /// Secure Admin Login dialog.
    /// Shows before AdminPanelForm — DialogResult.OK means credentials accepted.
    /// Default: admin / 1234 (changeable from Admin Panel → Change Password).
    /// </summary>
    public class AdminLoginForm : Form
    {
        private readonly AdminService _adminService = new AdminService();
        private TextBox _txtUsername = null!;
        private TextBox _txtPassword = null!;
        private Label   _lblError    = null!;

        public AdminLoginForm()
        {
            Text            = "Admin Login — " + Constants.ShopName;
            Size            = new Size(400, 340);
            StartPosition   = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox     = MinimizeBox = false;
            BackColor       = ThemeManager.PageBg;
            Font            = new Font("Segoe UI", 9.5f);
            BuildUI();
        }

        private void BuildUI()
        {
            // ── Dark header bar ──
            var header = new Panel
            {
                Dock      = DockStyle.Top,
                Height    = 80,
                BackColor = Color.FromArgb(30, 30, 48)
            };
            Controls.Add(header);

            header.Controls.Add(new Label
            {
                Text      = "🔒  Admin Login",
                Font      = new Font("Segoe UI", 16f, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize  = true,
                Location  = new Point(20, 14)
            });
            header.Controls.Add(new Label
            {
                Text      = Constants.ShopName,
                Font      = new Font("Segoe UI", 9f),
                ForeColor = Color.FromArgb(160, 160, 210),
                AutoSize  = true,
                Location  = new Point(22, 52)
            });

            // ── Fields ──
            int y = 100;

            Controls.Add(new Label { Text = "Username:", Location = new Point(44, y), AutoSize = true,
                Font = new Font("Segoe UI", 9f, FontStyle.Bold), ForeColor = ThemeManager.TextMuted });
            _txtUsername = new TextBox
            {
                Location = new Point(44, y + 20), Size = new Size(304, 28),
                Font = new Font("Segoe UI", 10.5f), Text = "admin"
            };
            Controls.Add(_txtUsername);

            y += 68;
            Controls.Add(new Label { Text = "Password:", Location = new Point(44, y), AutoSize = true,
                Font = new Font("Segoe UI", 9f, FontStyle.Bold), ForeColor = ThemeManager.TextMuted });
            _txtPassword = new TextBox
            {
                Location = new Point(44, y + 20), Size = new Size(304, 28),
                Font = new Font("Segoe UI", 10.5f), UseSystemPasswordChar = true
            };
            Controls.Add(_txtPassword);

            y += 68;
            _lblError = new Label
            {
                Location = new Point(44, y), AutoSize = true,
                ForeColor = Constants.Colors.AccentRed,
                Font = new Font("Segoe UI", 9f, FontStyle.Bold)
            };
            Controls.Add(_lblError);

            y += 24;
            var btnLogin = Btn("🔓  Login", Constants.Colors.AccentBlue, new Point(44, y), new Size(140, 38));
            btnLogin.Click += BtnLogin_Click;

            var btnCancel = Btn("Cancel", Color.Gray, new Point(200, y), new Size(100, 38));
            btnCancel.DialogResult = DialogResult.Cancel;

            AcceptButton = btnLogin;
            CancelButton = btnCancel;
        }

        private void BtnLogin_Click(object? sender, EventArgs e)
        {
            if (_adminService.ValidateLogin(_txtUsername.Text, _txtPassword.Text))
            {
                DialogResult = DialogResult.OK;
                Close();
            }
            else
            {
                _lblError.Text = "❌  Invalid username or password.";
                _txtPassword.Clear();
                _txtPassword.Focus();
            }
        }

        private Button Btn(string text, Color color, Point loc, Size size)
        {
            var b = new Button
            {
                Text = text, BackColor = color, ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat, Location = loc, Size = size,
                Cursor = Cursors.Hand, Font = new Font("Segoe UI", 10f, FontStyle.Bold),
                FlatAppearance = { BorderSize = 0 }
            };
            Controls.Add(b);
            return b;
        }
    }
}
