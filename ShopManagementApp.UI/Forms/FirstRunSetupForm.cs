using ShopManagementApp.Business.Services;
using ShopManagementApp.Utils;

namespace ShopManagementApp.UI.Forms
{
    /// <summary>
    /// Modal setup dialog shown on first launch (when no shop info is stored).
    /// The user MUST supply at least a Shop Name before continuing.
    /// On successful save, DialogResult is set to OK and MainForm opens.
    /// </summary>
    public class FirstRunSetupForm : Form
    {
        private readonly AdminService _adminSvc = new AdminService();

        // Color palette
        private static readonly Color C_Dark   = Color.FromArgb(24,  24,  37);
        private static readonly Color C_Blue   = Color.FromArgb(99, 102, 241);
        private static readonly Color C_Green  = Color.FromArgb(34, 197,  94);
        private static readonly Color C_Red    = Color.FromArgb(239, 68,  68);
        private static readonly Color C_PageBg = Color.FromArgb(245, 247, 252);
        private static readonly Color C_CardBg = Color.White;
        private static readonly Color C_Text   = Color.FromArgb(30,  30,  46);
        private static readonly Color C_Muted  = Color.FromArgb(107, 114, 128);

        // Input fields
        private TextBox _txtName    = null!;
        private TextBox _txtAddress = null!;
        private TextBox _txtPhone   = null!;
        private TextBox _txtGST     = null!;
        private Label   _lblStatus  = null!;

        public FirstRunSetupForm()
        {
            Text            = "Welcome — Shop Setup";
            Size            = new Size(540, 580);
            StartPosition   = FormStartPosition.CenterScreen;
            MinimumSize     = new Size(480, 520);
            BackColor       = C_PageBg;
            Font            = new Font("Segoe UI", 9.5f);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox     = false;
            MinimizeBox     = false;
            // Closing without saving exits the whole app (handled in Program.cs)
            BuildUI();
        }

        private void BuildUI()
        {
            // ── Header bar ─────────────────────────────────────────────────────
            var header = new Panel { Dock = DockStyle.Top, Height = 64, BackColor = C_Dark };
            Controls.Add(header);

            header.Controls.Add(new Label
            {
                Text      = "🏪  Welcome — Set Up Your Shop",
                Font      = new Font("Segoe UI", 14f, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize  = true,
                Location  = new Point(18, 18)
            });

            // ── Card panel ─────────────────────────────────────────────────────
            var card = new Panel
            {
                BackColor = C_CardBg,
                Bounds    = new Rectangle(30, 84, 464, 390),
                Anchor    = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            Controls.Add(card);

            // Paint a subtle rounded border
            card.Paint += (_, e) =>
            {
                using var pen = new Pen(Color.FromArgb(220, 220, 232), 1.5f);
                e.Graphics.DrawRectangle(pen, 0, 0, card.Width - 1, card.Height - 1);
            };

            int y = 24;

            // Sub-title
            card.Controls.Add(new Label
            {
                Text      = "Please enter your shop details below.\nThese will appear on receipts and invoices.",
                Font      = new Font("Segoe UI", 9.5f),
                ForeColor = C_Muted,
                Bounds    = new Rectangle(20, y, 420, 40),
                AutoSize  = false
            });
            y += 50;

            // ── Fields ──
            _txtName    = AddField(card, "Shop Name *",      ref y, false, placeholder: "e.g. Gayatri Electronics & Hardware");
            _txtAddress = AddField(card, "Shop Address",      ref y, true,  placeholder: "e.g. Main Road, City, State, PIN");
            _txtPhone   = AddField(card, "Mobile Number",     ref y, false, placeholder: "e.g. +91 98765 43210");
            _txtGST     = AddField(card, "GST Number",        ref y, false, placeholder: "e.g. 27AAACX0000X1Z5  (leave blank if N/A)");

            // ── Status label ──
            _lblStatus = new Label
            {
                Bounds    = new Rectangle(20, y + 6, 420, 22),
                Font      = new Font("Segoe UI", 9f, FontStyle.Bold),
                ForeColor = C_Red,
                AutoSize  = false,
                Text      = ""
            };
            card.Controls.Add(_lblStatus);

            // ── Save button ────────────────────────────────────────────────────
            var btnSave = new Button
            {
                Text      = "💾  Save & Open App",
                BackColor = C_Blue,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                FlatAppearance = { BorderSize = 0 },
                Bounds    = new Rectangle(30, 494, 220, 42),
                Cursor    = Cursors.Hand,
                Font      = new Font("Segoe UI", 11f, FontStyle.Bold),
                Anchor    = AnchorStyles.Bottom | AnchorStyles.Left
            };
            btnSave.Click += OnSave;
            Controls.Add(btnSave);

            // ── Skip / cancel note ─────────────────────────────────────────────
            var lblNote = new Label
            {
                Text      = "You can update these anytime from Admin Panel → Shop Settings.",
                Font      = new Font("Segoe UI", 8.5f, FontStyle.Italic),
                ForeColor = C_Muted,
                Bounds    = new Rectangle(30, 500, 340, 30),
                Anchor    = AnchorStyles.Bottom | AnchorStyles.Left,
                AutoSize  = false
            };
            Controls.Add(lblNote);
            // Shift the note below the button
            lblNote.Location = new Point(30, btnSave.Bottom + 6);
        }

        // ── Field factory ─────────────────────────────────────────────────────
        private static TextBox AddField(
            Panel parent, string label, ref int y,
            bool multiLine, string placeholder = "")
        {
            parent.Controls.Add(new Label
            {
                Text      = label,
                Font      = new Font("Segoe UI", 9f, FontStyle.Bold),
                ForeColor = Color.FromArgb(55, 55, 80),
                Bounds    = new Rectangle(20, y, 420, 18),
                AutoSize  = false
            });
            y += 20;

            int h = multiLine ? 52 : 30;
            var tb = new TextBox
            {
                Bounds        = new Rectangle(20, y, 420, h),
                Font          = new Font("Segoe UI", 10f),
                BorderStyle   = BorderStyle.FixedSingle,
                Multiline     = multiLine,
                ScrollBars    = multiLine ? ScrollBars.Vertical : ScrollBars.None,
                PlaceholderText = placeholder
            };
            parent.Controls.Add(tb);
            y += h + 16;
            return tb;
        }

        // ── Save handler ──────────────────────────────────────────────────────
        private void OnSave(object? sender, EventArgs e)
        {
            string name    = _txtName.Text.Trim();
            string address = _txtAddress.Text.Trim();
            string phone   = _txtPhone.Text.Trim();
            string gst     = _txtGST.Text.Trim();

            if (string.IsNullOrWhiteSpace(name))
            {
                _lblStatus.ForeColor = C_Red;
                _lblStatus.Text = "❌  Shop Name is required.";
                _txtName.Focus();
                return;
            }

            var (ok, msg) = _adminSvc.SaveShopInfo(name, address, phone, gst);
            if (ok)
            {
                DialogResult = DialogResult.OK;
                Close();
            }
            else
            {
                _lblStatus.ForeColor = C_Red;
                _lblStatus.Text = "❌  " + msg;
            }
        }
    }
}
