using ShopManagementApp.Business.Services;
using ShopManagementApp.Models;
using ShopManagementApp.Utils;

namespace ShopManagementApp.UI.Forms
{
    /// <summary>
    /// Inventory Management Form.
    ///
    /// Layout:
    ///   [Title row]
    ///   [Left: Product Details GroupBox] | [Right: Search + Filter + DataGridView]
    ///   GroupBox contains all input fields + Add / Update / Clear buttons
    ///   Right panel has Search textbox, Category filter, Filter button,
    ///   the product DataGridView (Anchored), and Delete button below it.
    /// </summary>
    public class InventoryForm : Form
    {
        private readonly InventoryService _service = new InventoryService();

        // ── Left-panel input fields ────────────────────────────────────────────
        private TextBox       _txtName          = null!;
        private ComboBox      _cmbCategory      = null!;
        private TextBox       _txtBrand         = null!;
        private NumericUpDown _numPurchasePrice  = null!;
        private NumericUpDown _numSellingPrice   = null!;
        private NumericUpDown _numStock          = null!;
        private ComboBox      _cmbUnit           = null!;
        private Label         _lblMode           = null!; // shows "Add Mode" or "Edit: <name>"

        // ── Right-panel controls ───────────────────────────────────────────────
        private TextBox      _txtSearch    = null!;
        private ComboBox     _cmbFilter    = null!;
        private Label        _lblLowStock  = null!;
        private DataGridView _dgv          = null!;

        private int _selectedProductId = 0;  // 0 = add mode

        // ── Constructor ───────────────────────────────────────────────────────
        public InventoryForm()
        {
            // AutoScroll removed — causes scrollbars when embedded with Dock=Fill
            // Padding added because MainForm's PageHost has no padding (forms handle their own)
            Padding        = new Padding(28, 22, 22, 20);
            DoubleBuffered = true;
            BackColor      = Color.FromArgb(245, 247, 252);
            BuildUI();
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }

        // ══════════════════════════════════════════════════════════════════════
        private void BuildUI()
        {
            // ── Page title ────────────────────────────────────────────────────
            var lblTitle = new Label
            {
                Text      = "📦  Inventory Management",
                Font      = new Font("Segoe UI", 17f, FontStyle.Bold),
                ForeColor = ThemeManager.TextPrimary,
                AutoSize  = true,
                Location  = new Point(0, 0)
            };
            Controls.Add(lblTitle);

            // ══════════ LEFT PANEL — Product Details GroupBox ════════════════
            var grp = new GroupBox
            {
                Text     = " Product Details ",
                Location = new Point(0, 46),
                Size     = new Size(308, 540),   // taller: fits 2-row button layout
                Font     = new Font("Segoe UI", 9.5f)
            };
            Controls.Add(grp);

            // Mode indicator label (Add / Edit)
            _lblMode = new Label
            {
                Text      = "➕  Add New Product",
                Font      = new Font("Segoe UI", 9f, FontStyle.Bold),
                ForeColor = Constants.Colors.AccentGreen,
                Location  = new Point(10, 22),
                AutoSize  = true
            };
            grp.Controls.Add(_lblMode);

            // Build fields
            int gy = 46;  // y position inside GroupBox, starting below mode label
            _txtName          = GrpField(grp, "Product Name *",      ref gy, 280);
            _cmbCategory      = GrpCombo(grp, "Category *",          ref gy, Constants.ProductCategories);
            _txtBrand         = GrpField(grp, "Brand",               ref gy, 280);
            _numPurchasePrice = GrpNumeric(grp, "Purchase Price ₹",  ref gy);
            _numSellingPrice  = GrpNumeric(grp, "Selling Price ₹ *", ref gy);
            _numStock         = GrpNumeric(grp, "Stock Quantity *",   ref gy, 99999, 0);
            _cmbUnit          = GrpCombo(grp, "Unit",                 ref gy, Constants.Units);

            // ── Action buttons inside GroupBox — 2 rows ──
            int btnY = gy + 8;

            // Row 1: Add  |  Update
            var btnAdd    = GrpBtn(grp, "  Add",    Constants.Colors.AccentGreen, new Point(10,  btnY), new Size(135, 34));
            var btnUpdate = GrpBtn(grp, "  Update",  Constants.Colors.AccentBlue,  new Point(154, btnY), new Size(135, 34));
            btnY += 42;

            // Row 2: Delete  |  Clear
            var btnDelete = GrpBtn(grp, "  Delete", Constants.Colors.AccentRed,   new Point(10,  btnY), new Size(135, 34));
            var btnClear  = GrpBtn(grp, "  Clear",  Color.FromArgb(100,100,110),   new Point(154, btnY), new Size(135, 34));

            btnAdd.Click    += BtnAdd_Click;
            btnUpdate.Click += BtnUpdate_Click;
            btnDelete.Click += BtnDeleteForm_Click;   // ← new: delete from form panel
            btnClear.Click  += (_, _) => ClearForm();


            // ══════════ RIGHT PANEL — Search / Filter / Grid ════════════════
            const int RX = 320;   // right panel X origin

            // ── Row 1: Search + Category Filter + Filter button ──
            AddLbl("🔍 Search:", new Point(RX, 50));
            _txtSearch = new TextBox
            {
                Location        = new Point(RX + 68, 47),
                Size            = new Size(195, 28),
                PlaceholderText = "Name, brand or category...",
                Font            = new Font("Segoe UI", 9.5f)
            };
            _txtSearch.TextChanged += (_, _) => LoadProducts();
            Controls.Add(_txtSearch);

            AddLbl("Filter:", new Point(RX + 275, 50));
            _cmbFilter = new ComboBox
            {
                Location      = new Point(RX + 318, 47),
                Size          = new Size(160, 28),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font          = new Font("Segoe UI", 9.5f)
            };
            _cmbFilter.Items.Add("All Categories");
            _cmbFilter.Items.AddRange(Constants.ProductCategories);
            _cmbFilter.SelectedIndex = 0;
            _cmbFilter.SelectedIndexChanged += (_, _) => LoadProducts();
            Controls.Add(_cmbFilter);

            var btnRefresh = AccentBtn("🔄", Constants.Colors.AccentBlue,
                new Point(RX + 488, 46), new Size(36, 28));
            btnRefresh.Click += (_, _) => LoadProducts();
            btnRefresh.Tag = "accent";

            // ── Low-stock warning ──
            _lblLowStock = new Label
            {
                Text      = "",
                Font      = new Font("Segoe UI", 9f, FontStyle.Bold),
                ForeColor = Constants.Colors.AccentRed,
                AutoSize  = true,
                Location  = new Point(RX, 82)
            };
            Controls.Add(_lblLowStock);

            // ── DataGridView — anchored so it resizes with the window ──
            _dgv = new DataGridView
            {
                Location            = new Point(RX, 104),
                Size                = new Size(640, 390),
                Anchor              = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom,
                AllowUserToAddRows  = false,
                ReadOnly            = true,
                SelectionMode       = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor     = Color.White,
                BorderStyle         = BorderStyle.FixedSingle,
                RowHeadersVisible   = false,
                Font                = new Font("Segoe UI", 9.5f),
                GridColor           = Color.FromArgb(220, 220, 230)
            };
            _dgv.ColumnHeadersDefaultCellStyle.BackColor = Constants.Colors.AccentGreen;
            _dgv.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            _dgv.ColumnHeadersDefaultCellStyle.Font      = new Font("Segoe UI", 9.5f, FontStyle.Bold);
            _dgv.EnableHeadersVisualStyles = false;
            _dgv.CellDoubleClick          += DgvDoubleClick;
            Controls.Add(_dgv);
            SetupColumns();

            LoadProducts();
        }


        // ── Columns ──────────────────────────────────────────────────────────
        private void SetupColumns()
        {
            _dgv.Columns.Add(new DataGridViewTextBoxColumn { Name = "ProductId", Visible = false });
            _dgv.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "#",            Name = "SrNo",    Width = 38 });
            _dgv.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Name",          Name = "Name",    FillWeight = 28 });
            _dgv.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Category",      Name = "Cat",     FillWeight = 20 });
            _dgv.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Brand",         Name = "Brand",   FillWeight = 14 });
            _dgv.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Buy ₹",         Name = "Buy",     Width = 80 });
            _dgv.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Sell ₹",        Name = "Sell",    Width = 80 });
            _dgv.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Stock",         Name = "Stock",   Width = 60 });
            _dgv.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Unit",          Name = "Unit",    Width = 56 });
        }

        // ── Load / filter products ────────────────────────────────────────────
        private void LoadProducts()
        {
            var products = _service.GetAllProducts();

            // Text search
            string q = _txtSearch.Text.Trim().ToLower();
            if (!string.IsNullOrEmpty(q))
                products = products.Where(p =>
                    p.Name.ToLower().Contains(q) ||
                    p.Brand.ToLower().Contains(q) ||
                    p.Category.ToLower().Contains(q)).ToList();

            // Category filter
            if (_cmbFilter.SelectedIndex > 0)
            {
                string cat = _cmbFilter.SelectedItem?.ToString() ?? "";
                products = products.Where(p => p.Category == cat).ToList();
            }

            int lowCount = products.Count(p => p.StockQuantity <= Constants.LowStockThreshold);
            _lblLowStock.Text = lowCount > 0 ? $"⚠  {lowCount} low-stock item(s)" : "";

            _dgv.Rows.Clear();
            int sr = 1;
            foreach (var p in products)
            {
                int ri = _dgv.Rows.Add(
                    p.ProductId, sr++, p.Name, p.Category, p.Brand,
                    p.PurchasePrice.ToString("N2"),
                    p.SellingPrice.ToString("N2"),
                    p.StockQuantity, p.Unit);

                if (p.StockQuantity <= Constants.LowStockThreshold)
                    _dgv.Rows[ri].DefaultCellStyle.BackColor = Color.FromArgb(255, 220, 220);
            }
        }

        // ── Double-click row → load into form ────────────────────────────────
        private void DgvDoubleClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            int id = Convert.ToInt32(_dgv.Rows[e.RowIndex].Cells["ProductId"].Value);
            var p  = _service.GetProductById(id);
            if (p == null) return;

            _selectedProductId         = p.ProductId;
            _txtName.Text              = p.Name;
            _cmbCategory.SelectedItem  = p.Category;
            _txtBrand.Text             = p.Brand;
            _numPurchasePrice.Value    = p.PurchasePrice;
            _numSellingPrice.Value     = p.SellingPrice;
            _numStock.Value            = p.StockQuantity;
            _cmbUnit.SelectedItem      = p.Unit;
            _lblMode.Text              = $"✏  Editing: {p.Name}";
            _lblMode.ForeColor         = Constants.Colors.AccentBlue;
        }

        // ── CRUD handlers ──────────────────────────────────────────────────────
        private void BtnAdd_Click(object? sender, EventArgs e)
        {
            var (ok, msg) = _service.AddProduct(BuildProduct());
            if (ok) { ValidationHelper.ShowSuccess(msg); ClearForm(); LoadProducts(); }
            else    ValidationHelper.ShowError(msg);
        }

        private void BtnUpdate_Click(object? sender, EventArgs e)
        {
            if (_selectedProductId == 0)
            { ValidationHelper.ShowError("Double-click a product row to select it for editing."); return; }
            var p = BuildProduct();
            p.ProductId = _selectedProductId;
            var (ok, msg) = _service.UpdateProduct(p);
            if (ok) { ValidationHelper.ShowSuccess(msg); ClearForm(); LoadProducts(); }
            else    ValidationHelper.ShowError(msg);
        }

        private void BtnDelete_Click(object? sender, EventArgs e)
        {
            if (_dgv.SelectedRows.Count == 0)
            { ValidationHelper.ShowError("Select a product row to delete."); return; }
            if (!ValidationHelper.Confirm("Permanently delete this product?")) return;
            int id = Convert.ToInt32(_dgv.SelectedRows[0].Cells["ProductId"].Value);
            var (ok, msg) = _service.DeleteProduct(id);
            if (ok) { ValidationHelper.ShowSuccess(msg); ClearForm(); LoadProducts(); }
            else    ValidationHelper.ShowError(msg);
        }

        /// <summary>
        /// Delete triggered from the FORM PANEL (left side) Delete button.
        /// Requires the user to first double-click a row to load it into the form.
        /// Uses _selectedProductId (same flow as Update).
        /// </summary>
        private void BtnDeleteForm_Click(object? sender, EventArgs e)
        {
            if (_selectedProductId == 0)
            {
                ValidationHelper.ShowError(
                    "No product selected.\n\nDouble-click a product row to load it, then click Delete.");
                return;
            }

            // Find the name for a meaningful confirmation message
            string name = _txtName.Text.Trim();
            if (!ValidationHelper.Confirm(
                $"Permanently delete '{name}'?\n\nThis cannot be undone.")) return;

            var (ok, msg) = _service.DeleteProduct(_selectedProductId);
            if (ok) { ValidationHelper.ShowSuccess(msg); ClearForm(); LoadProducts(); }
            else    ValidationHelper.ShowError(msg);
        }

        private Product BuildProduct() => new Product
        {
            Name          = _txtName.Text.Trim(),
            Category      = _cmbCategory.SelectedItem?.ToString() ?? "",
            Brand         = _txtBrand.Text.Trim(),
            PurchasePrice = _numPurchasePrice.Value,
            SellingPrice  = _numSellingPrice.Value,
            StockQuantity = (int)_numStock.Value,
            Unit          = _cmbUnit.SelectedItem?.ToString() ?? "Pcs"
        };

        private void ClearForm()
        {
            _selectedProductId     = 0;
            _txtName.Text          = "";
            _cmbCategory.SelectedIndex = 0;
            _txtBrand.Text         = "";
            _numPurchasePrice.Value = 0;
            _numSellingPrice.Value  = 0;
            _numStock.Value         = 0;
            _cmbUnit.SelectedIndex  = 0;
            _lblMode.Text           = "➕  Add New Product";
            _lblMode.ForeColor      = Constants.Colors.AccentGreen;
        }

        // ══════════════════════════════════════════════════════════════════════
        // GroupBox field builders
        // ══════════════════════════════════════════════════════════════════════

        /// <summary>Adds a label + TextBox row to the GroupBox.</summary>
        private static TextBox GrpField(GroupBox g, string label, ref int y, int w)
        {
            g.Controls.Add(new Label { Text = label, Location = new Point(10, y), AutoSize = true,
                Font = new Font("Segoe UI", 8.5f), ForeColor = Color.FromArgb(120, 120, 150) });
            y += 17;
            var tb = new TextBox { Location = new Point(10, y), Size = new Size(w, 26),
                Font = new Font("Segoe UI", 10f) };
            g.Controls.Add(tb);
            y += 34;
            return tb;
        }

        /// <summary>Adds a label + NumericUpDown row to the GroupBox.</summary>
        private static NumericUpDown GrpNumeric(GroupBox g, string label, ref int y,
            decimal max = 9999999, int dec = 2)
        {
            g.Controls.Add(new Label { Text = label, Location = new Point(10, y), AutoSize = true,
                Font = new Font("Segoe UI", 8.5f), ForeColor = Color.FromArgb(120, 120, 150) });
            y += 17;
            var n = new NumericUpDown { Location = new Point(10, y), Size = new Size(160, 26),
                DecimalPlaces = dec, Maximum = max, Minimum = 0, Font = new Font("Segoe UI", 10f) };
            g.Controls.Add(n);
            y += 34;
            return n;
        }

        /// <summary>Adds a label + ComboBox row to the GroupBox.</summary>
        private static ComboBox GrpCombo(GroupBox g, string label, ref int y, string[] items)
        {
            g.Controls.Add(new Label { Text = label, Location = new Point(10, y), AutoSize = true,
                Font = new Font("Segoe UI", 8.5f), ForeColor = Color.FromArgb(120, 120, 150) });
            y += 17;
            var cb = new ComboBox { Location = new Point(10, y), Size = new Size(280, 26),
                DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 10f) };
            cb.Items.AddRange(items);
            cb.SelectedIndex = 0;
            g.Controls.Add(cb);
            y += 34;
            return cb;
        }

        /// <summary>Creates a button inside a GroupBox with an accent colour.</summary>
        private static Button GrpBtn(GroupBox g, string text, Color color, Point loc, Size size)
        {
            var btn = new Button { Text = text, BackColor = color, ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat, Location = loc, Size = size,
                Cursor = Cursors.Hand, Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                FlatAppearance = { BorderSize = 0 }, Tag = "accent" };
            g.Controls.Add(btn);
            return btn;
        }

        /// <summary>Creates a Button on this form (not inside a GroupBox).</summary>
        private Button AccentBtn(string text, Color color, Point loc, Size size)
        {
            var btn = new Button { Text = text, BackColor = color, ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat, Location = loc, Size = size,
                Cursor = Cursors.Hand, Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                FlatAppearance = { BorderSize = 0 }, Tag = "accent" };
            Controls.Add(btn);
            return btn;
        }

        private void AddLbl(string text, Point pt) =>
            Controls.Add(new Label { Text = text, Location = pt, AutoSize = true,
                Font = new Font("Segoe UI", 9.5f) });
    }
}
