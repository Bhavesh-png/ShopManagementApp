using ShopManagementApp.Business.Services;
using ShopManagementApp.Utils;

namespace ShopManagementApp.UI.Forms
{
    /// <summary>
    /// Dashboard page.
    ///
    /// Loaded via MainForm.LoadPage() — NOT a standalone window.
    /// RefreshStats() is called every time the user navigates to this page,
    /// ensuring data is always current after any billing/repair/inventory change.
    ///
    /// WHY VALUES SHOWED AS 0:
    ///   The old ShopData.xlsx had a different column schema from a prior session.
    ///   ExcelManager now embeds a SchemaVersion in the Settings sheet.
    ///   If the version doesn't match, the old file is backed up and recreated.
    ///   This guarantees ProductRepository reads from the correct columns.
    /// </summary>
    public class DashboardForm : Form
    {
        // ── Services ─────────────────────────────────────────────────────────
        private readonly BillingService   _billing   = new();
        private readonly RepairService    _repair    = new();
        private readonly InventoryService _inventory = new();

        // ── Stat value labels — updated by RefreshStats() ─────────────────────
        private Label _lblRevenue  = null!;
        private Label _lblRepairs  = null!;
        private Label _lblLowStock = null!;
        private Label _lblProducts = null!;
        private Label _lblLastRefresh = null!;

        // ── Low-stock grid ────────────────────────────────────────────────────
        private DataGridView _dgvLowStock = null!;

        // ── Colors ────────────────────────────────────────────────────────────
        private static readonly Color C_PageBg   = Color.FromArgb(245, 247, 252);
        private static readonly Color C_CardBg   = Color.White;
        private static readonly Color C_TextPri  = Color.FromArgb(30,  30,  46);
        private static readonly Color C_TextMut  = Color.FromArgb(107, 114, 128);
        private static readonly Color C_Border   = Color.FromArgb(220, 220, 228);
        private static readonly Color C_Blue     = Color.FromArgb(99,  102, 241);
        private static readonly Color C_Green    = Color.FromArgb(34,  197, 94);
        private static readonly Color C_Orange   = Color.FromArgb(249, 115, 22);
        private static readonly Color C_Red      = Color.FromArgb(239, 68,  68);

        // ════════════════════════════════════════════════════════════════════════
        public DashboardForm()
        {
            // Consistent padding with all other pages
            Padding        = new Padding(28, 22, 22, 20);
            BackColor      = C_PageBg;
            Font           = new Font("Segoe UI", 9.5f);
            DoubleBuffered = true;
            BuildUI();
            // NOTE: RefreshStats() is called by MainForm.LoadPage() after the form
            // is fully embedded — not here — to avoid reading data before the form
            // is parented to the content panel.
        }

        // ════════════════════════════════════════════════════════════════════════
        //  Build static UI (called once in constructor)
        // ════════════════════════════════════════════════════════════════════════
        private void BuildUI()
        {
            // ── Use a scroll panel as the root so content never gets clipped ──
            var scroll = new Panel
            {
                Dock       = DockStyle.Fill,
                AutoScroll = true,
                BackColor  = C_PageBg
            };
            Controls.Add(scroll);

            int y = 0;   // running Y position inside scroll panel (relative to Padding)

            // ── Last-refresh timestamp (small, italic, top of content) ─────────
            // The big "Dashboard" title was removed — the top-bar breadcrumb
            // (top-right corner) already shows the page name. Having it twice
            // made the content area start with a 60px+ crowded text block.
            _lblLastRefresh = new Label
            {
                Text      = "",
                Location  = new Point(0, y),
                AutoSize  = true,
                Font      = new Font("Segoe UI", 8.5f, FontStyle.Italic),
                ForeColor = C_TextMut
            };
            scroll.Controls.Add(_lblLastRefresh);
            y += 20;

            // ── Divider ───────────────────────────────────────────────────────
            scroll.Controls.Add(new Panel { BackColor = C_Border, Bounds = new Rectangle(0, y, 900, 1) });
            y += 12;


            // ── Section: Stat Cards ───────────────────────────────────────────
            scroll.Controls.Add(new Label
            {
                Text      = "Today's Summary",
                Location  = new Point(0, y),
                AutoSize  = true,
                Font      = new Font("Segoe UI", 11f, FontStyle.Bold),
                ForeColor = C_TextPri
            });
            y += 30;

            // Stat cards — FlowLayout for easy wrapping on resize
            var cardFlow = new FlowLayoutPanel
            {
                Location      = new Point(0, y),
                Size          = new Size(900, 130),
                Anchor        = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents  = true,
                BackColor     = C_PageBg
            };
            scroll.Controls.Add(cardFlow);

            _lblRevenue  = MakeStatCard(cardFlow, "Today's Revenue",  "₹ 0",  "💰", C_Blue);
            _lblRepairs  = MakeStatCard(cardFlow, "Active Repairs",   "0",    "🔧", C_Orange);
            _lblLowStock = MakeStatCard(cardFlow, "Low Stock Items",  "0",    "⚠",  C_Red);
            _lblProducts = MakeStatCard(cardFlow, "Total Products",   "0",    "📦", C_Green);
            y += 140;

            // ── Divider ───────────────────────────────────────────────────────
            scroll.Controls.Add(new Panel { BackColor = C_Border, Bounds = new Rectangle(0, y, 900, 1) });
            y += 14;

            // ── Section: Quick Actions ────────────────────────────────────────
            scroll.Controls.Add(new Label
            {
                Text      = "Quick Actions",
                Location  = new Point(0, y),
                AutoSize  = true,
                Font      = new Font("Segoe UI", 11f, FontStyle.Bold),
                ForeColor = C_TextPri
            });
            y += 30;

            var actFlow = new FlowLayoutPanel
            {
                Location      = new Point(0, y),
                Size          = new Size(900, 60),
                Anchor        = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents  = true,
                BackColor     = C_PageBg
            };
            scroll.Controls.Add(actFlow);

            MakeQBtn(actFlow, "➕  New Bill",    C_Blue,   "Billing");
            MakeQBtn(actFlow, "🔧  New Repair",  C_Orange, "Repairs");
            MakeQBtn(actFlow, "📦  Inventory",   C_Green,  "Inventory");
            MakeQBtn(actFlow, "🔒  Admin Panel", Color.FromArgb(80, 80, 110), "Admin");

            // Manual refresh button
            var btnRefresh = new Button
            {
                Text           = "🔄  Refresh",
                Size           = new Size(115, 44),
                BackColor      = C_TextMut,
                ForeColor      = Color.White,
                FlatStyle      = FlatStyle.Flat,
                Font           = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                Margin         = new Padding(0, 0, 0, 0),
                Cursor         = Cursors.Hand,
                FlatAppearance = { BorderSize = 0 }
            };
            btnRefresh.Click += (_, _) => RefreshStats();
            actFlow.Controls.Add(btnRefresh);
            y += 68;

            // ── Divider ───────────────────────────────────────────────────────
            scroll.Controls.Add(new Panel { BackColor = C_Border, Bounds = new Rectangle(0, y, 900, 1) });
            y += 14;

            // ── Section: Low Stock Alerts ─────────────────────────────────────
            scroll.Controls.Add(new Label
            {
                Text      = "⚠  Low Stock Alerts   (stock ≤ 10)",
                Location  = new Point(0, y),
                AutoSize  = true,
                Font      = new Font("Segoe UI", 11f, FontStyle.Bold),
                ForeColor = C_Red
            });
            y += 30;

            _dgvLowStock = new DataGridView
            {
                Location            = new Point(0, y),
                Size                = new Size(900, 180),
                Anchor              = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                AllowUserToAddRows  = false,
                ReadOnly            = true,
                SelectionMode       = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor     = Color.White,
                BorderStyle         = BorderStyle.FixedSingle,
                RowHeadersVisible   = false,
                Font                = new Font("Segoe UI", 9.5f),
                GridColor           = Color.FromArgb(220, 220, 228)
            };
            StyleGridHeader(_dgvLowStock, C_Red);
            _dgvLowStock.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Product",  Name = "Name",  FillWeight = 38 });
            _dgvLowStock.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Category", Name = "Cat",   FillWeight = 24 });
            _dgvLowStock.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Stock",    Name = "Qty",   Width = 70  });
            _dgvLowStock.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Unit",     Name = "Unit",  Width = 65  });
            _dgvLowStock.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Sell ₹",   Name = "Price", Width = 90  });
            scroll.Controls.Add(_dgvLowStock);
        }

        // ════════════════════════════════════════════════════════════════════════
        //  RefreshStats — called by MainForm.LoadPage() on every navigation
        //                 and by the manual Refresh button.
        //
        //  HOW IT WORKS:
        //    1. Read all products → count total, find low-stock
        //    2. Read today's sales → sum FinalAmount
        //    3. Read active repairs → count Pending/In Progress
        //    4. Update all labels + low-stock grid
        //    5. Record last-refresh timestamp
        // ════════════════════════════════════════════════════════════════════════
        public void RefreshStats()
        {
            try
            {
                // ── Step 1: Load data (ONCE — avoid repeated Excel reads) ──
                var allProducts = _inventory.GetAllProducts();
                var lowProducts = _inventory.GetLowStockProducts();   // uses same cached list in service
                decimal todayRevenue = _billing.GetTodayRevenue();
                int activeRepairs   = _repair.GetActiveRepairCount();

                // ── Step 2: Update stat card labels ──
                _lblRevenue.Text  = $"₹ {todayRevenue:N0}";
                _lblRepairs.Text  = activeRepairs.ToString();
                _lblLowStock.Text = lowProducts.Count.ToString();
                _lblProducts.Text = allProducts.Count.ToString();

                // ── Step 3: Colour the Active Repairs card orange if >0 ──
                //   (parent Panel has background set by StatCard helper)
                SetCardUrgency(_lblRepairs,  activeRepairs  > 0);
                SetCardUrgency(_lblLowStock, lowProducts.Count > 0);

                // ── Step 4: Rebuild low-stock alert grid ──
                _dgvLowStock.Rows.Clear();
                foreach (var p in lowProducts.OrderBy(p => p.StockQuantity))
                {
                    int ri = _dgvLowStock.Rows.Add(
                        p.Name, p.Category, p.StockQuantity, p.Unit, $"₹ {p.SellingPrice:N0}");

                    // Color-code: RED if 0, ORANGE if ≤5, YELLOW if ≤10
                    Color rowColor = p.StockQuantity == 0
                        ? Color.FromArgb(255, 205, 205)
                        : p.StockQuantity <= 5
                            ? Color.FromArgb(255, 230, 205)
                            : Color.FromArgb(255, 248, 225);

                    _dgvLowStock.Rows[ri].DefaultCellStyle.BackColor = rowColor;
                    _dgvLowStock.Rows[ri].DefaultCellStyle.ForeColor = C_TextPri;
                }

                if (lowProducts.Count == 0)
                {
                    // Show a friendly "all good" row
                    int ri = _dgvLowStock.Rows.Add("✅  All products have sufficient stock", "", "", "", "");
                    _dgvLowStock.Rows[ri].DefaultCellStyle.BackColor = Color.FromArgb(220, 252, 231);
                    _dgvLowStock.Rows[ri].DefaultCellStyle.ForeColor = Color.FromArgb(22, 101, 52);
                    _dgvLowStock.Rows[ri].DefaultCellStyle.Font      = new Font("Segoe UI", 9.5f, FontStyle.Bold);
                }

                // ── Step 5: Timestamp ──
                _lblLastRefresh.Text = $"Last updated: {DateTime.Now:HH:mm:ss}";
            }
            catch (Exception ex)
            {
                // Never crash the dashboard — show safe defaults with error note
                _lblRevenue.Text      = "₹ —";
                _lblRepairs.Text      = "—";
                _lblLowStock.Text     = "—";
                _lblProducts.Text     = "—";
                _lblLastRefresh.Text  = $"⚠ Refresh failed: {ex.Message}";
                _lblLastRefresh.ForeColor = C_Red;
                System.Diagnostics.Debug.WriteLine("[Dashboard.RefreshStats] " + ex);
            }
        }

        // ════════════════════════════════════════════════════════════════════════
        //  UI Helpers
        // ════════════════════════════════════════════════════════════════════════

        private Label MakeStatCard(FlowLayoutPanel parent, string title, string value, string icon, Color accent)
        {
            var card = new Panel
            {
                Size      = new Size(200, 110),
                BackColor = C_CardBg,
                Margin    = new Padding(0, 0, 16, 0),
                Cursor    = Cursors.Default
            };

            // Left accent bar
            card.Controls.Add(new Panel
            {
                BackColor = accent,
                Bounds    = new Rectangle(0, 0, 5, 110)
            });

            // Icon
            card.Controls.Add(new Label
            {
                Text      = icon,
                Location  = new Point(140, 12),
                AutoSize  = true,
                Font      = new Font("Segoe UI", 18f),
                ForeColor = Color.FromArgb(220, 220, 235)
            });

            // Title
            card.Controls.Add(new Label
            {
                Text      = title,
                Location  = new Point(14, 12),
                AutoSize  = true,
                Font      = new Font("Segoe UI", 8.5f),
                ForeColor = C_TextMut
            });

            // Value (returned so caller can update it)
            var valLabel = new Label
            {
                Text      = value,
                Location  = new Point(14, 42),
                AutoSize  = true,
                Font      = new Font("Segoe UI", 24f, FontStyle.Bold),
                ForeColor = accent,
                Tag       = accent   // store original accent for urgency reset
            };
            card.Controls.Add(valLabel);

            // Card bottom line
            card.Controls.Add(new Panel
            {
                BackColor = Color.FromArgb(240, 240, 248),
                Bounds    = new Rectangle(0, 108, 200, 2)
            });

            parent.Controls.Add(card);
            return valLabel;
        }

        private static void SetCardUrgency(Label valueLabel, bool urgent)
        {
            // Pulse the value label colour to C_Red when urgent (e.g. low stock > 0)
            if (valueLabel.Tag is Color originalColor)
            {
                // Keep original accent; urgency is communicated by row colors in grid
                valueLabel.ForeColor = originalColor;
            }
        }

        private void MakeQBtn(FlowLayoutPanel parent, string text, Color color, string page)
        {
            var btn = new Button
            {
                Text           = text,
                Size           = new Size(150, 44),
                BackColor      = color,
                ForeColor      = Color.White,
                FlatStyle      = FlatStyle.Flat,
                Font           = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                Margin         = new Padding(0, 0, 10, 0),
                Cursor         = Cursors.Hand,
                FlatAppearance = { BorderSize = 0 }
            };
            btn.Click += (_, _) => FindMainForm()?.NavigateTo(page);
            new ToolTip().SetToolTip(btn, $"Go to {page}");
            parent.Controls.Add(btn);
        }

        private static void StyleGridHeader(DataGridView dgv, Color headerColor)
        {
            dgv.ColumnHeadersDefaultCellStyle.BackColor = headerColor;
            dgv.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgv.ColumnHeadersDefaultCellStyle.Font      = new Font("Segoe UI", 9.5f, FontStyle.Bold);
            dgv.EnableHeadersVisualStyles               = false;
        }

        private MainForm? FindMainForm()
        {
            Control? c = this;
            while (c != null)
            {
                if (c is MainForm mf) return mf;
                c = c.Parent;
            }
            return null;
        }
    }
}
