using ShopManagementApp.Business.Services;
using ShopManagementApp.Models;
using ShopManagementApp.Utils;

namespace ShopManagementApp.UI.Forms
{
    /// <summary>
    /// Admin Panel — accessed only after successful AdminLoginForm.
    /// Tabs: Dashboard | All Sales | All Repairs | Products | Change Password
    /// </summary>
    public class AdminPanelForm : Form
    {
        private readonly AdminService     _adminSvc   = new AdminService();
        private readonly BillingService   _billingSvc = new BillingService();
        private readonly RepairService    _repairSvc  = new RepairService();
        private readonly InventoryService _invSvc     = new InventoryService();

        // Color palette (light theme)
        private static readonly Color C_Dark    = Color.FromArgb(30,  30,  48);
        private static readonly Color C_Blue    = Color.FromArgb(99,  102, 241);
        private static readonly Color C_Green   = Color.FromArgb(34,  197, 94);
        private static readonly Color C_Orange  = Color.FromArgb(249, 115, 22);
        private static readonly Color C_Red     = Color.FromArgb(239, 68,  68);
        private static readonly Color C_PageBg  = Color.FromArgb(245, 247, 252);
        private static readonly Color C_CardBg  = Color.White;
        private static readonly Color C_Text    = Color.FromArgb(30,  30,  46);
        private static readonly Color C_Muted   = Color.FromArgb(107, 114, 128);

        public AdminPanelForm()
        {
            Text            = "Admin Panel — " + Constants.ShopName;
            Size            = new Size(1080, 680);
            StartPosition   = FormStartPosition.CenterParent;
            MinimumSize     = new Size(900, 580);
            BackColor       = C_PageBg;
            Font            = new Font("Segoe UI", 9.5f);
            BuildUI();
        }

        private void BuildUI()
        {
            // ── Top header ──
            var header = new Panel
            {
                Dock = DockStyle.Top, Height = 56, BackColor = C_Dark
            };
            Controls.Add(header);

            header.Controls.Add(new Label
            {
                Text = "⚙  Admin Panel  —  " + Constants.ShopName,
                Font = new Font("Segoe UI", 14f, FontStyle.Bold),
                ForeColor = Color.White, AutoSize = true, Location = new Point(18, 14)
            });

            var btnClose = new Button
            {
                Text = "✕  Close", BackColor = C_Red, ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat, FlatAppearance = { BorderSize = 0 },
                Size = new Size(90, 34), Cursor = Cursors.Hand,
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            btnClose.Location = new Point(header.Width - 100, 11);
            btnClose.Anchor   = AnchorStyles.Top | AnchorStyles.Right;
            btnClose.Click   += (_, _) => Close();
            header.Controls.Add(btnClose);

            // ── TabControl ──
            var tabs = new TabControl
            {
                Dock      = DockStyle.Fill,
                Font      = new Font("Segoe UI", 10f),
                Padding   = new Point(16, 6)
            };
            Controls.Add(tabs);
            tabs.BringToFront();

            tabs.TabPages.Add(BuildDashboardTab());
            tabs.TabPages.Add(BuildSalesTab());
            tabs.TabPages.Add(BuildRepairsTab());
            tabs.TabPages.Add(BuildProductsTab());
            tabs.TabPages.Add(BuildPasswordTab());
        }

        // ── Tab 1: Dashboard ──────────────────────────────────────────────────
        private TabPage BuildDashboardTab()
        {
            var page = Tab("📊  Dashboard");
            page.BackColor = C_PageBg;

            var sales   = _billingSvc.GetAllSales();
            var repairs = _repairSvc.GetAllRepairs();
            var prods   = _invSvc.GetAllProducts();

            decimal totalRevenue   = sales.Sum(s => s.FinalAmount);
            decimal todayRevenue   = _billingSvc.GetTodayRevenue();
            int     totalRepairs   = repairs.Count;
            int     activeRepairs  = repairs.Count(r => r.Status != "Delivered");
            int     lowStock       = prods.Count(p => p.StockQuantity <= Constants.LowStockThreshold);

            var flow = new FlowLayoutPanel
            {
                Location = new Point(20, 20), AutoSize = true,
                FlowDirection = FlowDirection.LeftToRight, WrapContents = true,
                BackColor = C_PageBg
            };
            page.Controls.Add(flow);

            StatCard(flow, "Today's Revenue",   $"₹ {todayRevenue:N0}",   C_Blue);
            StatCard(flow, "Total Revenue",      $"₹ {totalRevenue:N0}",   C_Green);
            StatCard(flow, "Total Bills",        $"{sales.Count}",          C_Blue);
            StatCard(flow, "Active Repairs",     $"{activeRepairs}",        C_Orange);
            StatCard(flow, "Total Repairs",      $"{totalRepairs}",         C_Orange);
            StatCard(flow, "Low Stock Items",    $"{lowStock}",             C_Red);
            StatCard(flow, "Total Products",     $"{prods.Count}",          C_Green);

            return page;
        }

        // ── Tab 2: All Sales ──────────────────────────────────────────────────
        private TabPage BuildSalesTab()
        {
            var page = Tab("🧾  All Sales");
            page.BackColor = C_PageBg;

            var dgv = MakeDgv(new Point(10, 10), new Size(1020, 560));
            dgv.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom;
            dgv.ColumnHeadersDefaultCellStyle.BackColor = C_Blue;
            dgv.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgv.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9.5f, FontStyle.Bold);
            dgv.EnableHeadersVisualStyles = false;
            page.Controls.Add(dgv);

            dgv.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Bill #",    Width = 65  });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Date/Time", Width = 140 });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Items",     Width = 55  });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "SubTotal ₹",FillWeight = 20 });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Discount ₹",FillWeight = 20 });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Total ₹",   FillWeight = 20 });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Payment",   Width = 90  });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Notes",     FillWeight = 30 });

            foreach (var s in _billingSvc.GetAllSales())
                dgv.Rows.Add($"#{s.SaleId}", s.SaleDate.ToString("dd/MM/yy HH:mm"),
                    s.Items.Count, $"₹{s.SubTotal:N0}", $"₹{s.Discount:N0}",
                    $"₹{s.FinalAmount:N0}", s.PaymentMode, s.Notes);

            return page;
        }

        // ── Tab 3: All Repairs ────────────────────────────────────────────────
        private TabPage BuildRepairsTab()
        {
            var page = Tab("🔧  All Repairs");
            page.BackColor = C_PageBg;

            var dgv = MakeDgv(new Point(10, 10), new Size(1020, 560));
            dgv.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom;
            dgv.ColumnHeadersDefaultCellStyle.BackColor = C_Orange;
            dgv.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgv.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9.5f, FontStyle.Bold);
            dgv.EnableHeadersVisualStyles = false;
            page.Controls.Add(dgv);

            dgv.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Job #",     Width = 65  });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Customer",  FillWeight = 25 });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Phone",     Width = 108 });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Device",    FillWeight = 20 });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Fault",     FillWeight = 30 });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Est. ₹",   Width = 80  });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Status",    Width = 100 });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Received",  Width = 90  });

            foreach (var r in _repairSvc.GetAllRepairs())
            {
                int ri = dgv.Rows.Add($"R-{r.RepairId}", r.CustomerName, r.CustomerPhone,
                    r.DeviceType, r.FaultDescription, $"₹{r.EstimatedCost:N0}",
                    r.Status, r.ReceivedDate.ToString("dd/MM/yy"));

                Color rowColor = r.Status switch
                {
                    "Pending"     => Color.FromArgb(255, 248, 225),
                    "In Progress" => Color.FromArgb(224, 235, 255),
                    "Completed"   => Color.FromArgb(220, 252, 231),
                    _             => Color.White
                };
                dgv.Rows[ri].DefaultCellStyle.BackColor = rowColor;
            }
            return page;
        }

        // ── Tab 4: Products (read-only view + launch Inventory) ────────────────
        private TabPage BuildProductsTab()
        {
            var page = Tab("📦  Products");
            page.BackColor = C_PageBg;

            var btnOpen = new Button
            {
                Text = "📦  Open Full Inventory Manager",
                BackColor = C_Green, ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat, FlatAppearance = { BorderSize = 0 },
                Size = new Size(260, 36), Location = new Point(10, 10),
                Cursor = Cursors.Hand, Font = new Font("Segoe UI", 10f, FontStyle.Bold)
            };
            btnOpen.Click += (_, _) => { var f = new InventoryForm(); f.ShowDialog(this); LoadProductsGrid(page); };
            page.Controls.Add(btnOpen);

            LoadProductsGrid(page);
            return page;
        }

        private DataGridView? _dgvProducts;

        private void LoadProductsGrid(TabPage page)
        {
            // FIXED Bug 6: remove from parent before disposing to prevent orphaned controls
            if (_dgvProducts != null)
            {
                page.Controls.Remove(_dgvProducts);
                _dgvProducts.Dispose();
                _dgvProducts = null;
            }

            var dgv = MakeDgv(new Point(10, 56), new Size(1020, 514));
            dgv.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom;
            dgv.ColumnHeadersDefaultCellStyle.BackColor = C_Green;
            dgv.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgv.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9.5f, FontStyle.Bold);
            dgv.EnableHeadersVisualStyles = false;
            page.Controls.Add(dgv);
            _dgvProducts = dgv;

            dgv.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "ID",       Width = 50  });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Name",     FillWeight = 30 });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Category", FillWeight = 20 });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Brand",    FillWeight = 15 });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Buy ₹",   Width = 80  });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Sell ₹",  Width = 80  });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Stock",    Width = 65  });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Unit",     Width = 60  });

            foreach (var p in _invSvc.GetAllProducts())
            {
                int ri = dgv.Rows.Add(p.ProductId, p.Name, p.Category, p.Brand,
                    $"₹{p.PurchasePrice:N0}", $"₹{p.SellingPrice:N0}", p.StockQuantity, p.Unit);
                if (p.StockQuantity <= Constants.LowStockThreshold)
                    dgv.Rows[ri].DefaultCellStyle.BackColor = Color.FromArgb(255, 220, 220);
            }
        }

        // ── Tab 5: Change Password ─────────────────────────────────────────────
        private TabPage BuildPasswordTab()
        {
            var page = Tab("🔑  Change Password");
            page.BackColor = C_PageBg;

            var grp = new GroupBox
            {
                Text = " Change Admin Password ", Location = new Point(40, 30),
                Size = new Size(380, 240), Font = new Font("Segoe UI", 10f),
                BackColor = C_CardBg, ForeColor = C_Text
            };
            page.Controls.Add(grp);

            int y = 35;
            var txtCurrent = PwdField(grp, "Current Password:", ref y);
            var txtNew     = PwdField(grp, "New Password:",     ref y);
            var txtConfirm = PwdField(grp, "Confirm New Password:", ref y);

            var lblMsg = new Label { Location = new Point(14, y), AutoSize = true,
                Font = new Font("Segoe UI", 9f, FontStyle.Bold) };
            grp.Controls.Add(lblMsg);

            var btnChange = new Button
            {
                Text = "🔑  Change Password", BackColor = C_Blue, ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat, FlatAppearance = { BorderSize = 0 },
                Location = new Point(14, y + 28), Size = new Size(200, 36),
                Cursor = Cursors.Hand, Font = new Font("Segoe UI", 10f, FontStyle.Bold)
            };
            btnChange.Click += (_, _) =>
            {
                if (txtNew.Text != txtConfirm.Text)
                { lblMsg.ForeColor = C_Red; lblMsg.Text = "❌  New passwords do not match."; return; }
                var (ok, msg) = _adminSvc.ChangePassword(txtCurrent.Text, txtNew.Text);
                lblMsg.ForeColor = ok ? C_Green : C_Red;
                lblMsg.Text = ok ? "✅  " + msg : "❌  " + msg;
                if (ok) { txtCurrent.Clear(); txtNew.Clear(); txtConfirm.Clear(); }
            };
            grp.Controls.Add(btnChange);

            return page;
        }

        // ── Helpers ───────────────────────────────────────────────────────────

        private static TabPage Tab(string title)
            => new TabPage(title) { Padding = new Padding(6) };

        private static DataGridView MakeDgv(Point loc, Size size)
            => new DataGridView
            {
                Location            = loc, Size = size,
                AllowUserToAddRows  = false, ReadOnly = true,
                SelectionMode       = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor     = Color.White, BorderStyle = BorderStyle.FixedSingle,
                RowHeadersVisible   = false, Font = new Font("Segoe UI", 9.5f),
                GridColor           = Color.FromArgb(220, 220, 230)
            };

        private static void StatCard(FlowLayoutPanel fp, string title, string value, Color accent)
        {
            var card = new Panel
            {
                Size = new Size(200, 110), BackColor = Color.White,
                Margin = new Padding(0, 0, 16, 16)
            };

            // left accent bar
            card.Controls.Add(new Panel { BackColor = accent, Bounds = new Rectangle(0, 0, 5, 110) });

            card.Controls.Add(new Label
            {
                Text = title, Location = new Point(14, 14), AutoSize = true,
                Font = new Font("Segoe UI", 9f), ForeColor = Color.FromArgb(107, 114, 128)
            });
            card.Controls.Add(new Label
            {
                Text = value, Location = new Point(14, 40), AutoSize = true,
                Font = new Font("Segoe UI", 22f, FontStyle.Bold), ForeColor = accent
            });
            fp.Controls.Add(card);
        }

        private static TextBox PwdField(GroupBox g, string label, ref int y)
        {
            g.Controls.Add(new Label { Text = label, Location = new Point(14, y), AutoSize = true,
                Font = new Font("Segoe UI", 9f), ForeColor = Color.FromArgb(107, 114, 128) });
            y += 18;
            var tb = new TextBox { Location = new Point(14, y), Size = new Size(340, 26),
                UseSystemPasswordChar = true, Font = new Font("Segoe UI", 10f) };
            g.Controls.Add(tb);
            y += 42;
            return tb;
        }
    }
}
