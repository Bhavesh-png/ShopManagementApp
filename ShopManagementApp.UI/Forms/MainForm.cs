using ShopManagementApp.Business.Services;
using ShopManagementApp.Utils;

namespace ShopManagementApp.UI.Forms
{
    /// <summary>
    /// MainForm — the application shell.
    ///
    /// Layout (fixed, never changes):
    ///   [TopBar   — DockStyle.Top  — 52px]   shop name + hamburger
    ///   [Sidebar  — DockStyle.Left — 0-210px] navigation, animated
    ///   [PageHost — DockStyle.Fill]            ONE embedded Form at a time
    ///
    /// Navigation rule:
    ///   ALL pages (including Dashboard) are loaded with LoadPage().
    ///   LoadPage() always: clear → embed Form → Show.
    ///   This guarantees ZERO overlap — only one Form ever lives in PageHost.
    ///
    /// ROOT CAUSE OF PREVIOUS OVERLAP:
    ///   Old code added dashboard Labels/Panels directly to the content panel,
    ///   then OpenChild() cleared those and added a Form. On fast clicks the
    ///   old controls were still painting. Now EVERYTHING goes through LoadPage().
    /// </summary>
    public class MainForm : Form
    {
        // ── Services (used only for dashboard stats) ──────────────────────────
        private readonly BillingService   _billing   = new();
        private readonly RepairService    _repair    = new();
        private readonly InventoryService _inventory = new();

        // ── Layout panels ─────────────────────────────────────────────────────
        private Panel       _topBar   = null!;
        private Panel       _sidebar  = null!;
        private SmoothPanel _pageHost = null!;   // SmoothPanel = double-buffered, zero flicker

        // ── Top-bar controls ──────────────────────────────────────────────────
        private Button _btnHamburger = null!;
        private Label  _lblShopName  = null!;
        private Label  _lblPageTitle = null!;   // shows current page name in top bar

        // ── Sidebar animation ─────────────────────────────────────────────────
        private const int SidebarMaxW  = 210;
        private bool  _sidebarOpen     = false;   // starts CLOSED for clean first look
        private int   _sidebarTarget   = 0;
        private readonly System.Windows.Forms.Timer _animTimer = new() { Interval = 8 };

        // ── Active nav button tracking ────────────────────────────────────────
        private Button? _activeNav;

        // ── Page colors (single light theme) ─────────────────────────────────
        private static readonly Color C_Sidebar  = Color.FromArgb(24, 24, 37);
        private static readonly Color C_TopBar   = Color.White;
        private static readonly Color C_PageBg   = Color.FromArgb(245, 247, 252);
        private static readonly Color C_TextPri  = Color.FromArgb(30,  30,  46);
        private static readonly Color C_TextMut  = Color.FromArgb(107, 114, 128);
        private static readonly Color C_Border   = Color.FromArgb(220, 220, 228);
        private static readonly Color C_Blue     = Color.FromArgb(99,  102, 241);
        private static readonly Color C_Green    = Color.FromArgb(34,  197, 94);
        private static readonly Color C_Orange   = Color.FromArgb(249, 115, 22);
        private static readonly Color C_Red      = Color.FromArgb(239, 68,  68);

        // ════════════════════════════════════════════════════════════════════════
        public MainForm()
        {
            SuspendLayout();
            _animTimer.Tick += AnimTick;
            Build();
            LoadAppIcon();   // set window + taskbar icon
            ResumeLayout(true);
            LoadPage(new DashboardForm(), "📊  Dashboard");
        }

        /// <summary>Load AppIcon.ico from the Assets folder next to the EXE.</summary>
        private void LoadAppIcon()
        {
            try
            {
                string icoPath = Path.Combine(
                    AppDomain.CurrentDomain.BaseDirectory, "Assets", "AppIcon.ico");

                if (File.Exists(icoPath))
                    Icon = new Icon(icoPath);
            }
            catch
            {
                // If icon file is missing, use default WinForms icon — never crash
            }
        }

        // ── Build the fixed shell ─────────────────────────────────────────────
        private void Build()
        {
            Text            = Constants.ShopName + "  –  Shop Management";
            Size            = new Size(1200, 720);   // fallback if user restores window
            MinimumSize     = new Size(900, 560);
            StartPosition   = FormStartPosition.CenterScreen;
            WindowState     = FormWindowState.Maximized;  // ← start maximized, no flash
            BackColor       = C_PageBg;
            Font            = new Font("Segoe UI", 9.5f);
            DoubleBuffered  = true;


            // ⚠️ DOCKING ORDER IS CRITICAL in WinForms:
            //
            // WinForms docks controls in REVERSE Controls-collection order.
            // The control at the HIGHEST index (added LAST) is docked FIRST.
            //
            //   Controls[0] = PageHost  (Dock=Fill)  → processed LAST  → fills remaining space
            //   Controls[1] = Sidebar   (Dock=Left)  → processed 2nd   → takes left edge
            //   Controls[2] = TopBar    (Dock=Top)   → processed FIRST → claims top 52px
            //
            // OLD WRONG ORDER: TopBar(0) → Sidebar(1) → PageHost(2)
            //   PageHost was at index 2 so it was docked FIRST → grabbed FULL form area
            //   TopBar then PAINTED OVER PageHost → Dashboard content cut off by header
            //
            // CORRECT ORDER: PageHost first, Sidebar second, TopBar LAST ↓
            BuildPageHost();    // ← index 0 — Dock=Fill  — processed last, fills remainder
            BuildSidebar();     // ← index 1 — Dock=Left  — processed 2nd
            BuildTopBar();      // ← index 2 — Dock=Top   — processed FIRST, claims top 52px
        }

        // ── Top bar ───────────────────────────────────────────────────────────
        private void BuildTopBar()
        {
            _topBar = new Panel
            {
                Dock      = DockStyle.Top,
                Height    = 52,
                BackColor = C_TopBar
            };
            Controls.Add(_topBar);

            // Hamburger
            _btnHamburger = new Button
            {
                Text           = "☰",
                Size           = new Size(52, 52),
                Location       = new Point(0, 0),
                FlatStyle      = FlatStyle.Flat,
                Cursor         = Cursors.Hand,
                Font           = new Font("Segoe UI", 15f, FontStyle.Bold),
                BackColor      = C_TopBar,
                ForeColor      = C_TextPri,
                FlatAppearance = { BorderSize = 0 }
            };
            _btnHamburger.Click += (_, _) => ToggleSidebar();
            _topBar.Controls.Add(_btnHamburger);

            // ── Logo image (32×32) ─────────────────────────────────────
            int logoX = 58;
            try
            {
                string pngPath = Path.Combine(
                    AppDomain.CurrentDomain.BaseDirectory, "Assets", "AppIcon.png");

                if (File.Exists(pngPath))
                {
                    var logo = new PictureBox
                    {
                        Image    = Image.FromFile(pngPath),
                        Size     = new Size(34, 34),
                        Location = new Point(logoX, 9),
                        SizeMode = PictureBoxSizeMode.Zoom,
                        BackColor= C_TopBar
                    };
                    _topBar.Controls.Add(logo);
                    logoX += 40;   // shop name starts after logo
                }
            }
            catch { /* icon file missing — skip, no crash */ }

            // Shop name label (starts after logo)
            _lblShopName = new Label
            {
                Text      = Constants.ShopName,
                Location  = new Point(logoX, 0),
                Size      = new Size(380, 52),
                TextAlign = ContentAlignment.MiddleLeft,
                Font      = new Font("Segoe UI", 12f, FontStyle.Bold),
                ForeColor = C_TextPri,
                BackColor = C_TopBar
            };
            _topBar.Controls.Add(_lblShopName);

            // Current page indicator (right side)
            _lblPageTitle = new Label
            {
                Text      = "",
                Size      = new Size(300, 52),
                TextAlign = ContentAlignment.MiddleRight,
                Font      = new Font("Segoe UI", 9.5f),
                ForeColor = C_TextMut,
                BackColor = C_TopBar,
                Anchor    = AnchorStyles.Top | AnchorStyles.Right
            };
            _lblPageTitle.Location = new Point(_topBar.Width - 310, 0);
            _topBar.Controls.Add(_lblPageTitle);
            _topBar.Resize += (_, _) => _lblPageTitle.Location = new Point(_topBar.Width - 310, 0);

            // Bottom separator line
            _topBar.Paint += (_, e) =>
            {
                using var pen = new Pen(C_Border);
                e.Graphics.DrawLine(pen, 0, _topBar.Height - 1, _topBar.Width, _topBar.Height - 1);
            };
        }

        // ── Sidebar ───────────────────────────────────────────────────────────
        private void BuildSidebar()
        {
            _sidebar = new Panel
            {
                Dock      = DockStyle.Left,
                Width     = 0,            // starts collapsed
                BackColor = C_Sidebar
            };
            Controls.Add(_sidebar);

            // Shop name inside sidebar (visible when open)
            _sidebar.Controls.Add(new Label
            {
                Text      = Constants.ShopName,
                ForeColor = Color.FromArgb(140, 140, 190),
                Font      = new Font("Segoe UI", 8.5f, FontStyle.Bold),
                Bounds    = new Rectangle(14, 10, 182, 42),
                TextAlign = ContentAlignment.MiddleLeft
            });
            _sidebar.Controls.Add(new Panel
            {
                BackColor = Color.FromArgb(50, 50, 72),
                Bounds    = new Rectangle(14, 54, 182, 1)
            });

            int y = 64;
            NavBtn("🏠  Dashboard",   ref y, () => LoadPage(new DashboardForm(),  "📊  Dashboard"));
            NavBtn("🧾  Billing",     ref y, () => LoadPage(new BillingForm(),     "🧾  Billing"));
            NavBtn("🔧  Repairs",     ref y, () => LoadPage(new RepairForm(),      "🔧  Repairs"));
            NavBtn("📦  Inventory",   ref y, () => LoadPage(new InventoryForm(),   "📦  Inventory"));
            NavBtn("🔒  Admin Panel", ref y, ShowAdminPanel);

            // Exit pinned to bottom
            var btnExit = MakeNavBtn("✖  Exit");
            btnExit.ForeColor = C_Red;
            btnExit.Anchor    = AnchorStyles.Bottom | AnchorStyles.Left;
            btnExit.Location  = new Point(0, 600);
            btnExit.Click    += (_, _) => { if (ValidationHelper.Confirm("Exit the application?")) Application.Exit(); };
            _sidebar.Controls.Add(btnExit);
        }

        private void NavBtn(string text, ref int y, Action action)
        {
            var btn = MakeNavBtn(text);
            btn.Location = new Point(0, y);
            y += 47;
            btn.Click += (_, _) =>
            {
                HighlightNav(btn);
                action();
                // Auto-close sidebar after navigating (smooth UX)
                if (_sidebarOpen) ToggleSidebar();
            };
            _sidebar.Controls.Add(btn);
        }

        private static Button MakeNavBtn(string text) => new()
        {
            Text           = text,
            Size           = new Size(SidebarMaxW, 45),
            FlatStyle      = FlatStyle.Flat,
            TextAlign      = ContentAlignment.MiddleLeft,
            Font           = new Font("Segoe UI", 10f),
            Cursor         = Cursors.Hand,
            ForeColor      = Color.White,
            BackColor      = Color.Transparent,
            Padding        = new Padding(18, 0, 0, 0),
            FlatAppearance = { BorderSize = 0 }
        };

        private void HighlightNav(Button btn)
        {
            foreach (Control c in _sidebar.Controls)
                if (c is Button b) { b.BackColor = Color.Transparent; b.ForeColor = Color.White; }
            btn.BackColor = C_Blue;
            btn.ForeColor = Color.White;
            _activeNav = btn;
        }

        // ── Page host (center content area — no padding, no extra styling) ────
        private void BuildPageHost()
        {
            _pageHost = new SmoothPanel   // ← double-buffered: zero flicker on page change
            {
                Dock      = DockStyle.Fill,
                BackColor = C_PageBg,
                Padding   = Padding.Empty
            };
            Controls.Add(_pageHost);
            _pageHost.Click += (_, _) => { if (_sidebarOpen) ToggleSidebar(); };
        }

        // ── THE core navigation method ────────────────────────────────────────
        /// <summary>
        /// Loads any Form as a full-page embedded panel.
        /// SuspendLayout prevents the blank-frame flash during swap.
        /// </summary>
        private void LoadPage(Form page, string pageTitle)
        {
            // Freeze all painting on _pageHost until new page is fully wired up
            _pageHost.SuspendLayout();
            try
            {
                // 1. Dispose the current page (prevents memory leaks)
                if (_pageHost.Controls.Count > 0)
                {
                    var old = _pageHost.Controls[0];
                    _pageHost.Controls.Clear();
                    old.Dispose();
                }
                else
                {
                    _pageHost.Controls.Clear();
                }

                // 2. Configure as embedded panel (not a standalone window)
                page.TopLevel        = false;
                page.FormBorderStyle = FormBorderStyle.None;
                page.Dock            = DockStyle.Fill;
                page.BackColor       = C_PageBg;

                // 3. Add — still no paint because layout is suspended
                _pageHost.Controls.Add(page);
            }
            finally
            {
                // 4. ONE repaint, new page already in place — no blank frame
                _pageHost.ResumeLayout(true);
            }

            // 5. Show after layout is stable
            page.Show();
            page.BringToFront();

            // 6. Update breadcrumb in top bar
            _lblPageTitle.Text = pageTitle;

            // 7. Refresh dashboard stats on every navigation back
            if (page is DashboardForm df) df.RefreshStats();
        }

        // ── Public navigation (used by DashboardForm quick-action buttons) ───
        /// <summary>Navigate to a named page from outside (e.g., quick-action buttons).</summary>
        public void NavigateTo(string pageName)
        {
            // Find and highlight the matching nav button
            foreach (Control c in _sidebar.Controls)
            {
                if (c is Button b && b.Text.Contains(pageName, StringComparison.OrdinalIgnoreCase))
                    HighlightNav(b);
            }

            switch (pageName)
            {
                case "Dashboard": LoadPage(new DashboardForm(),  "📊  Dashboard"); break;
                case "Billing":   LoadPage(new BillingForm(),    "🧾  Billing");   break;
                case "Repairs":   LoadPage(new RepairForm(),     "🔧  Repairs");   break;
                case "Inventory": LoadPage(new InventoryForm(),  "📦  Inventory"); break;
                case "Admin":     ShowAdminPanel();                                break;
            }
        }

        // ── Admin Panel (requires login) ──────────────────────────────────────
        private void ShowAdminPanel()
        {
            var login = new AdminLoginForm();
            if (login.ShowDialog(this) == DialogResult.OK)
            {
                var panel = new AdminPanelForm();
                panel.ShowDialog(this);
            }
        }

        // ── Sidebar smooth animation ──────────────────────────────────────────
        private void ToggleSidebar()
        {
            _sidebarOpen   = !_sidebarOpen;
            _sidebarTarget = _sidebarOpen ? SidebarMaxW : 0;
            _animTimer.Start();
        }

        private void AnimTick(object? sender, EventArgs e)
        {
            int cur  = _sidebar.Width;
            int diff = _sidebarTarget - cur;
            int step = Math.Max(4, Math.Abs(diff) / 3);   // ease-out

            if (Math.Abs(diff) <= step)
            {
                _sidebar.Width = _sidebarTarget;
                _animTimer.Stop();
            }
            else
            {
                _sidebar.Width = cur + (diff > 0 ? step : -step);
            }
        }

        // ── Anti-flicker: composite entire form off-screen ────────────────────
        // WS_EX_COMPOSITED tells Windows to render the whole form into a back
        // buffer before showing — zero tearing or blank-frame flashes.
        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.ExStyle |= 0x02000000;   // WS_EX_COMPOSITED
                return cp;
            }
        }
    }

    // ══════════════════════════════════════════════════════════════════════════
    //  SmoothPanel — double-buffered Panel
    //
    //  WHY THIS IS NEEDED:
    //    WinForms Panel does not expose a public DoubleBuffered property.
    //    The only way to enable it is to subclass and set it in the constructor
    //    (or use reflection, which is fragile).
    //
    //  WHAT IT DOES:
    //    When _pageHost uses SmoothPanel instead of Panel, every page swap
    //    renders to an off-screen buffer first, then blits in one operation.
    //    Result: zero "white flash" when switching between Dashboard/Billing/etc.
    // ══════════════════════════════════════════════════════════════════════════
    internal sealed class SmoothPanel : Panel
    {
        public SmoothPanel()
        {
            // Enable the hidden DoubleBuffered property (inherited from Control)
            DoubleBuffered = true;

            // These three styles together give the smoothest possible rendering:
            SetStyle(ControlStyles.AllPaintingInWmPaint,  true);  // no erase before paint
            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);  // render off-screen first
            SetStyle(ControlStyles.UserPaint,             true);  // we own the paint cycle
            UpdateStyles();
        }
    }
}
