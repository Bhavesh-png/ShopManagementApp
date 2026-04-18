using ShopManagementApp.Business.Services;
using ShopManagementApp.Models;
using ShopManagementApp.Utils;

namespace ShopManagementApp.UI.Forms
{
    /// <summary>
    /// Admin Panel — left-sidebar navigation with 6 content panels:
    ///   Dashboard | All Sales | All Repairs | Products | Change Password | Shop Settings
    /// </summary>
    public class AdminPanelForm : Form
    {
        private readonly AdminService     _adminSvc   = new AdminService();
        private readonly BillingService   _billingSvc = new BillingService();
        private readonly RepairService    _repairSvc  = new RepairService();
        private readonly InventoryService _invSvc     = new InventoryService();

        // ── Palette ───────────────────────────────────────────────────────────
        private static readonly Color C_Dark    = Color.FromArgb(24,  24,  37);
        private static readonly Color C_NavHov  = Color.FromArgb(40,  40,  58);
        private static readonly Color C_Blue    = Color.FromArgb(99,  102, 241);
        private static readonly Color C_Green   = Color.FromArgb(34,  197,  94);
        private static readonly Color C_Orange  = Color.FromArgb(249, 115,  22);
        private static readonly Color C_Red     = Color.FromArgb(239,  68,  68);
        private static readonly Color C_PageBg  = Color.FromArgb(245, 247, 252);
        private static readonly Color C_CardBg  = Color.White;
        private static readonly Color C_Text    = Color.FromArgb(30,  30,  46);
        private static readonly Color C_Muted   = Color.FromArgb(107, 114, 128);
        private static readonly Color C_Border  = Color.FromArgb(220, 220, 232);

        // ── Layout ────────────────────────────────────────────────────────────
        private Panel   _sidebar    = null!;
        private Panel   _content    = null!;
        private Label   _lblHead    = null!;
        private Label   _lblTitle   = null!;
        private Button? _activeNav;

        // ── Cached content panels ─────────────────────────────────────────────
        private Panel? _pgDashboard, _pgSales, _pgRepairs, _pgProducts, _pgPassword, _pgShop;
        private DataGridView? _dgvProducts;

        // ═════════════════════════════════════════════════════════════════════
        public AdminPanelForm()
        {
            Text          = "Admin Panel — " + Constants.ShopInfo.Name;
            Size          = new Size(1120, 700);
            MinimumSize   = new Size(900,  580);
            StartPosition = FormStartPosition.CenterParent;
            BackColor     = C_PageBg;
            Font          = new Font("Segoe UI", 9.5f);
            DoubleBuffered = true;
            BuildShell();
            Navigate(_pgDashboard = BuildDashboard(), "📊  Dashboard");
        }

        // ═════════════════════════════════════════════════════════════════════
        //  Shell (header + sidebar + content host)
        // ═════════════════════════════════════════════════════════════════════
        private void BuildShell()
        {
            // ── Top header bar ────────────────────────────────────────────────
            var header = new Panel { Dock = DockStyle.Top, Height = 56, BackColor = C_Dark };

            _lblHead = new Label
            {
                Text = "⚙  Admin Panel  —  " + Constants.ShopInfo.Name,
                Font = new Font("Segoe UI", 13f, FontStyle.Bold),
                ForeColor = Color.White, AutoSize = true, Location = new Point(18, 14)
            };

            _lblTitle = new Label
            {
                Text = "Dashboard",
                Font = new Font("Segoe UI", 11f),
                ForeColor = Color.FromArgb(180, 180, 220),
                AutoSize = false,
                Size = new Size(300, 56),
                TextAlign = ContentAlignment.MiddleRight,
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };

            var btnClose = new Button
            {
                Text = "✕  Close", BackColor = C_Red, ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat, FlatAppearance = { BorderSize = 0 },
                Size = new Size(100, 34), Cursor = Cursors.Hand,
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };

            btnClose.Location = new Point(header.Width - 116, 11);
            _lblTitle.Location = new Point(header.Width - 430, 0);

            header.Controls.AddRange(new Control[] { _lblHead, _lblTitle, btnClose });
            header.Resize += (_, _) => 
            {
                btnClose.Location = new Point(header.Width - 116, 11);
                _lblTitle.Location = new Point(header.Width - 430, 0);
            };
            Controls.Add(header);

            // ── Left sidebar ──────────────────────────────────────────────────
            _sidebar = new Panel
            {
                Dock = DockStyle.Left, Width = 172, BackColor = C_Dark
            };

            int ny = 12; // Start buttons higher since shop name is now in the top header
            NavBtn("📊  Dashboard",      ref ny, ShowDashboard);
            NavBtn("🧾  All Sales",      ref ny, ShowSales);
            NavBtn("🔧  All Repairs",    ref ny, ShowRepairs);
            NavBtn("📦  Products",       ref ny, ShowProducts);
            NavBtn("🔑  Change Password",ref ny, ShowPassword);
            NavBtn("🏪  Shop Settings",  ref ny, ShowShopSettings);

            Controls.Add(_sidebar);

            // ── Content host ──────────────────────────────────────────────────
            _content = new Panel { Dock = DockStyle.Fill, BackColor = C_PageBg, Padding = new Padding(24) };
            Controls.Add(_content);
            _content.BringToFront();
        }

        // ═════════════════════════════════════════════════════════════════════
        //  Navigation helpers
        // ═════════════════════════════════════════════════════════════════════
        private void NavBtn(string text, ref int y, Action action)
        {
            var btn = new Button
            {
                Text = text, Size = new Size(172, 44),
                Location = new Point(0, y),
                FlatStyle = FlatStyle.Flat,
                FlatAppearance = { BorderSize = 0, MouseOverBackColor = C_NavHov },
                TextAlign = ContentAlignment.MiddleLeft,
                Font = new Font("Segoe UI", 9.5f), Cursor = Cursors.Hand,
                ForeColor = Color.FromArgb(200, 200, 220),
                BackColor = Color.Transparent,
                Padding = new Padding(16, 0, 0, 0)
            };
            btn.Click += (_, _) => { ActivateNav(btn); action(); };
            _sidebar.Controls.Add(btn);
            y += 44;
        }

        private void ActivateNav(Button btn)
        {
            if (_activeNav != null)
            { _activeNav.BackColor = Color.Transparent; _activeNav.ForeColor = Color.FromArgb(200, 200, 220); }
            btn.BackColor = C_Blue;
            btn.ForeColor = Color.White;
            _activeNav = btn;
        }

        private void Navigate(Panel panel, string title)
        {
            _content.Controls.Clear();
            panel.Dock = DockStyle.Fill;
            _content.Controls.Add(panel);
            _lblTitle.Text = title;
        }

        // ── Show methods (lazy-build each panel once) ─────────────────────────
        private void ShowDashboard()  => Navigate(_pgDashboard  ??= BuildDashboard(),  "Dashboard");
        private void ShowSales()      => Navigate(_pgSales      ??= BuildSales(),      "All Sales");
        private void ShowRepairs()    => Navigate(_pgRepairs    ??= BuildRepairs(),    "All Repairs");
        private void ShowProducts()   { _pgProducts = BuildProducts(); Navigate(_pgProducts, "Products"); }
        private void ShowPassword()   => Navigate(_pgPassword   ??= BuildPassword(),   "Change Password");
        private void ShowShopSettings()=> Navigate(_pgShop      ??= BuildShopSettings(),"Shop Settings");

        // ═════════════════════════════════════════════════════════════════════
        //  Page 1: Dashboard
        // ═════════════════════════════════════════════════════════════════════
        private Panel BuildDashboard()
        {
            var pg = Page();

            var sales   = _billingSvc.GetAllSales();
            var repairs = _repairSvc.GetAllRepairs();
            var prods   = _invSvc.GetAllProducts();

            decimal totalRev  = sales.Sum(s => s.FinalAmount);
            decimal todayRev  = _billingSvc.GetTodayRevenue();
            int activeRep     = repairs.Count(r => r.Status != "Delivered");
            int lowStock      = prods.Count(p => p.StockQuantity <= Constants.LowStockThreshold);

            // Section title
            pg.Controls.Add(SectionLabel("📊  Dashboard Overview", 0));

            var flow = new FlowLayoutPanel
            {
                Location      = new Point(0, 36),
                Width         = 900,
                AutoSize      = true,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents  = true,
                BackColor     = C_PageBg,
                Anchor        = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            pg.Controls.Add(flow);
            pg.Resize += (_, _) => flow.Width = pg.Width - 4;

            StatCard(flow, "Today's Revenue",  $"₹ {todayRev:N0}",     C_Blue);
            StatCard(flow, "Total Revenue",    $"₹ {totalRev:N0}",     C_Green);
            StatCard(flow, "Total Bills",      $"{sales.Count}",        C_Blue);
            StatCard(flow, "Active Repairs",   $"{activeRep}",          C_Orange);
            StatCard(flow, "Total Repairs",    $"{repairs.Count}",      C_Orange);
            StatCard(flow, "Low Stock Items",  $"{lowStock}",           C_Red);
            StatCard(flow, "Total Products",   $"{prods.Count}",        C_Green);

            return pg;
        }

        // ═════════════════════════════════════════════════════════════════════
        //  Page 2: All Sales
        // ═════════════════════════════════════════════════════════════════════
        private Panel BuildSales()
        {
            var pg = Page();
            pg.Controls.Add(SectionLabel("🧾  All Sales", 0));

            var dgv = Grid(new Point(0, 36));
            dgv.ColumnHeadersDefaultCellStyle.BackColor = C_Blue;
            dgv.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgv.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9.5f, FontStyle.Bold);
            dgv.EnableHeadersVisualStyles = false;
            pg.Controls.Add(dgv);

            dgv.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Bill #",     Width = 70 });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Date / Time",Width = 140 });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Items",      Width = 60 });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "SubTotal ₹", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Discount ₹", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Total ₹",    AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Payment",    Width = 90 });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Notes",      AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill });

            foreach (var s in _billingSvc.GetAllSales())
                dgv.Rows.Add($"#{s.SaleId}", s.SaleDate.ToString("dd/MM/yy HH:mm"),
                    s.Items.Count, $"₹{s.SubTotal:N0}", $"₹{s.Discount:N0}",
                    $"₹{s.FinalAmount:N0}", s.PaymentMode, s.Notes);

            return pg;
        }

        // ═════════════════════════════════════════════════════════════════════
        //  Page 3: All Repairs
        // ═════════════════════════════════════════════════════════════════════
        private Panel BuildRepairs()
        {
            var pg = Page();
            pg.Controls.Add(SectionLabel("🔧  All Repairs", 0));

            var dgv = Grid(new Point(0, 36));
            dgv.ColumnHeadersDefaultCellStyle.BackColor = C_Orange;
            dgv.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgv.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9.5f, FontStyle.Bold);
            dgv.EnableHeadersVisualStyles = false;
            pg.Controls.Add(dgv);

            dgv.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Job #",    Width = 70 });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Customer", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Phone",    Width = 115 });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Device",   AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Fault",    AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Est. ₹",  Width = 80 });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Status",   Width = 105 });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Received", Width = 90 });

            foreach (var r in _repairSvc.GetAllRepairs())
            {
                int ri = dgv.Rows.Add($"R-{r.RepairId}", r.CustomerName, r.CustomerPhone,
                    r.DeviceType, r.FaultDescription, $"₹{r.EstimatedCost:N0}",
                    r.Status, r.ReceivedDate.ToString("dd/MM/yy"));

                dgv.Rows[ri].DefaultCellStyle.BackColor = r.Status switch
                {
                    "Pending"     => Color.FromArgb(255, 248, 225),
                    "In Progress" => Color.FromArgb(224, 235, 255),
                    "Completed"   => Color.FromArgb(220, 252, 231),
                    _             => Color.White
                };
            }
            return pg;
        }

        // ═════════════════════════════════════════════════════════════════════
        //  Page 4: Products
        // ═════════════════════════════════════════════════════════════════════
        private Panel BuildProducts()
        {
            var pg = Page();
            pg.Controls.Add(SectionLabel("📦  Products", 0));

            var btnOpen = Btn("📦  Open Inventory Manager", C_Green, new Point(0, 36), new Size(240, 34));
            btnOpen.Click += (_, _) => { var f = new InventoryForm(); f.ShowDialog(this); RefreshProductsGrid(pg); };
            pg.Controls.Add(btnOpen);

            RefreshProductsGrid(pg);
            return pg;
        }

        private void RefreshProductsGrid(Panel pg)
        {
            if (_dgvProducts != null) { pg.Controls.Remove(_dgvProducts); _dgvProducts.Dispose(); }

            var dgv = Grid(new Point(0, 80));
            dgv.ColumnHeadersDefaultCellStyle.BackColor = C_Green;
            dgv.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgv.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9.5f, FontStyle.Bold);
            dgv.EnableHeadersVisualStyles = false;
            pg.Controls.Add(dgv);
            _dgvProducts = dgv;

            dgv.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "ID",       Width = 52 });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Name",     AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Category", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Brand",    AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Buy ₹",   Width = 82 });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Sell ₹",  Width = 82 });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Stock",    Width = 65 });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Unit",     Width = 60 });

            foreach (var p in _invSvc.GetAllProducts())
            {
                int ri = dgv.Rows.Add(p.ProductId, p.Name, p.Category, p.Brand,
                    $"₹{p.PurchasePrice:N0}", $"₹{p.SellingPrice:N0}", p.StockQuantity, p.Unit);
                if (p.StockQuantity <= Constants.LowStockThreshold)
                    dgv.Rows[ri].DefaultCellStyle.BackColor = Color.FromArgb(255, 220, 220);
            }
        }

        // ═════════════════════════════════════════════════════════════════════
        //  Page 5: Change Password
        // ═════════════════════════════════════════════════════════════════════
        private Panel BuildPassword()
        {
            var pg = Page();
            pg.Controls.Add(SectionLabel("🔑  Change Admin Password", 0));

            var card = Card(new Point(0, 50), new Size(440, 300));
            pg.Controls.Add(card);

            int y = 20;
            card.Controls.Add(FieldLabel("Current Password:", 16, y)); y += 22;
            var txtCur = PwdBox(card, 16, y); y += 46;
            card.Controls.Add(FieldLabel("New Password:", 16, y)); y += 22;
            var txtNew = PwdBox(card, 16, y); y += 46;
            card.Controls.Add(FieldLabel("Confirm New Password:", 16, y)); y += 22;
            var txtCon = PwdBox(card, 16, y); y += 50;

            var lblMsg = new Label { Location = new Point(16, y), AutoSize = true, Font = new Font("Segoe UI", 9f, FontStyle.Bold) };
            card.Controls.Add(lblMsg);

            var btnChange = Btn("🔑  Change Password", C_Blue, new Point(16, y + 26), new Size(200, 36));
            btnChange.Click += (_, _) =>
            {
                if (txtNew.Text != txtCon.Text)
                { lblMsg.ForeColor = C_Red; lblMsg.Text = "❌  Passwords do not match."; return; }
                var (ok, msg) = _adminSvc.ChangePassword(txtCur.Text, txtNew.Text);
                lblMsg.ForeColor = ok ? C_Green : C_Red;
                lblMsg.Text = (ok ? "✅  " : "❌  ") + msg;
                if (ok) { txtCur.Clear(); txtNew.Clear(); txtCon.Clear(); }
            };
            card.Controls.Add(btnChange);

            return pg;
        }

        // ═════════════════════════════════════════════════════════════════════
        //  Page 6: Shop Settings
        // ═════════════════════════════════════════════════════════════════════
        private Panel BuildShopSettings()
        {
            var pg = Page();
            pg.Controls.Add(SectionLabel("🏪  Shop Settings", 0));

            pg.Controls.Add(new Label
            {
                Text = "Changes are saved to ShopData.xlsx and take effect immediately.",
                Font = new Font("Segoe UI", 9f, FontStyle.Italic),
                ForeColor = C_Muted, Location = new Point(0, 32), AutoSize = true
            });

            var card = Card(new Point(0, 62), new Size(560, 390));
            pg.Controls.Add(card);

            int y = 18;

            card.Controls.Add(FieldLabel("Shop Name  *", 16, y)); y += 22;
            var txtName = Field(card, 16, y, 516, false); txtName.Text = Constants.ShopInfo.Name; y += 38;

            card.Controls.Add(FieldLabel("Address", 16, y)); y += 22;
            var txtAddr = Field(card, 16, y, 516, true);  txtAddr.Text = Constants.ShopInfo.Address; y += 68;

            card.Controls.Add(FieldLabel("Mobile Number", 16, y)); y += 22;
            var txtPhone= Field(card, 16, y, 516, false); txtPhone.Text = Constants.ShopInfo.Phone; y += 38;

            card.Controls.Add(FieldLabel("GST Number", 16, y)); y += 22;
            var txtGST  = Field(card, 16, y, 516, false); txtGST.Text = Constants.ShopInfo.GST;  y += 42;

            var lblMsg = new Label { Location = new Point(16, y), AutoSize = true, Font = new Font("Segoe UI", 9f, FontStyle.Bold) };
            card.Controls.Add(lblMsg);

            var btnSave = Btn("💾  Save Settings", C_Blue, new Point(16, y + 26), new Size(200, 36));
            btnSave.Click += (_, _) =>
            {
                var (ok, msg) = _adminSvc.SaveShopInfo(txtName.Text, txtAddr.Text, txtPhone.Text, txtGST.Text);
                lblMsg.ForeColor = ok ? C_Green : C_Red;
                lblMsg.Text = (ok ? "✅  " : "❌  ") + msg;
                if (ok) 
                {
                    Text = "Admin Panel — " + Constants.ShopInfo.Name;
                    _lblHead.Text = "⚙  Admin Panel  —  " + Constants.ShopInfo.Name;
                }
            };
            card.Controls.Add(btnSave);

            return pg;
        }

        // ═════════════════════════════════════════════════════════════════════
        //  Factory helpers
        // ═════════════════════════════════════════════════════════════════════

        /// <summary>Blank content page — fills the content host.</summary>
        private static Panel Page() => new Panel { BackColor = Color.Transparent, Dock = DockStyle.Fill };

        /// <summary>White card panel with a subtle border.</summary>
        private static Panel Card(Point loc, Size size)
        {
            var p = new Panel { Location = loc, Size = size, BackColor = Color.White };
            p.Paint += (_, e) =>
            {
                using var pen = new Pen(Color.FromArgb(215, 215, 228), 1f);
                e.Graphics.DrawRectangle(pen, 0, 0, p.Width - 1, p.Height - 1);
            };
            return p;
        }

        /// <summary>Section title label.</summary>
        private static Label SectionLabel(string text, int y) => new Label
        {
            Text = text, Location = new Point(0, y), AutoSize = true,
            Font = new Font("Segoe UI", 13f, FontStyle.Bold), ForeColor = Color.FromArgb(30, 30, 46)
        };

        /// <summary>Small muted field label.</summary>
        private static Label FieldLabel(string text, int x, int y) => new Label
        {
            Text = text, Location = new Point(x, y), AutoSize = true,
            Font = new Font("Segoe UI", 9f, FontStyle.Bold), ForeColor = Color.FromArgb(80, 80, 110)
        };

        /// <summary>Full-anchored DataGridView starting at <paramref name="loc"/>.</summary>
        private static DataGridView Grid(Point loc) => new DataGridView
        {
            Location            = loc,
            Anchor              = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom,
            AllowUserToAddRows  = false, ReadOnly = true,
            SelectionMode       = DataGridViewSelectionMode.FullRowSelect,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            BackgroundColor     = Color.White, BorderStyle = BorderStyle.FixedSingle,
            RowHeadersVisible   = false, Font = new Font("Segoe UI", 9.5f),
            GridColor           = Color.FromArgb(220, 220, 230),
            ColumnHeadersHeight = 32, RowTemplate = { Height = 28 }
        };

        /// <summary>Styled text input.</summary>
        private static TextBox Field(Control parent, int x, int y, int w, bool multi)
        {
            int h = multi ? 52 : 28;
            var tb = new TextBox
            {
                Location = new Point(x, y), Size = new Size(w, h),
                Font = new Font("Segoe UI", 10f),
                BorderStyle = BorderStyle.FixedSingle,
                Multiline = multi,
                ScrollBars = multi ? ScrollBars.Vertical : ScrollBars.None
            };
            parent.Controls.Add(tb);
            return tb;
        }

        /// <summary>Password text box.</summary>
        private static TextBox PwdBox(Control parent, int x, int y)
        {
            var tb = new TextBox
            {
                Location = new Point(x, y), Size = new Size(390, 30),
                Font = new Font("Segoe UI", 10f),
                UseSystemPasswordChar = true, BorderStyle = BorderStyle.FixedSingle
            };
            parent.Controls.Add(tb);
            return tb;
        }

        /// <summary>Styled flat button.</summary>
        private static Button Btn(string text, Color back, Point loc, Size size) => new Button
        {
            Text = text, Location = loc, Size = size,
            BackColor = back, ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat, FlatAppearance = { BorderSize = 0 },
            Font = new Font("Segoe UI", 10f, FontStyle.Bold), Cursor = Cursors.Hand
        };

        /// <summary>Stat card for dashboard.</summary>
        private static void StatCard(FlowLayoutPanel fp, string title, string value, Color accent)
        {
            var card = new Panel
            {
                Size = new Size(210, 115), BackColor = Color.White,
                Margin = new Padding(0, 0, 16, 16)
            };
            card.Paint += (_, e) =>
            {
                using var pen = new Pen(Color.FromArgb(215, 215, 228));
                e.Graphics.DrawRectangle(pen, 0, 0, card.Width - 1, card.Height - 1);
            };
            // Left accent bar
            card.Controls.Add(new Panel { BackColor = accent, Bounds = new Rectangle(0, 0, 5, 115) });
            card.Controls.Add(new Label
            {
                Text = title, Location = new Point(14, 14), AutoSize = true,
                Font = new Font("Segoe UI", 9f), ForeColor = Color.FromArgb(107, 114, 128)
            });
            card.Controls.Add(new Label
            {
                Text = value, Location = new Point(14, 42), AutoSize = true,
                Font = new Font("Segoe UI", 20f, FontStyle.Bold), ForeColor = accent
            });
            fp.Controls.Add(card);
        }
    }
}
