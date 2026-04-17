using ShopManagementApp.Business.Services;
using ShopManagementApp.Models;
using ShopManagementApp.Utils;

namespace ShopManagementApp.UI.Forms
{
    /// <summary>
    /// Billing Form — Create a new walk-in sale bill.
    ///
    /// CUSTOM ITEM FEATURE:
    ///   Selecting "✏  Other / Custom Item..." in the Product dropdown
    ///   reveals an inline panel where the user types any name + price.
    ///   Custom items use ProductId = 0 so they bypass stock deduction.
    /// </summary>
    public class BillingForm : Form
    {
        private readonly BillingService   _billing   = new BillingService();
        private readonly InventoryService _inventory = new InventoryService();

        private readonly List<SaleItem> _cartItems   = new List<SaleItem>();
        private List<Product>           _allProducts = new List<Product>();
        private Sale? _lastSavedSale;

        // ── Main controls ─────────────────────────────────────────────────────
        private ComboBox      _cmbPayment    = null!;
        private TextBox       _txtNotes      = null!;
        private ComboBox      _cmbProduct    = null!;
        private NumericUpDown _numQty        = null!;
        private Label         _lblUnitPrice  = null!;
        private DataGridView  _dgvCart       = null!;
        private Label         _lblSubTotal   = null!;
        private NumericUpDown _numDiscount   = null!;
        private Label         _lblFinalTotal = null!;
        private Button        _btnPrint      = null!;

        // ── Custom-item inline panel controls ─────────────────────────────────
        private Panel         _pnlCustom     = null!;   // shown only when "Other" selected
        private TextBox       _txtCustomName = null!;   // custom product name
        private NumericUpDown _numCustomRate = null!;   // custom unit price

        // Sentinel value — last item in ComboBox
        private const string CustomItemText = "✏  Other / Custom Item...";

        // Light-theme colors
        private static readonly Color C_Blue   = Color.FromArgb(99,  102, 241);
        private static readonly Color C_Green  = Color.FromArgb(34,  197, 94);
        private static readonly Color C_Red    = Color.FromArgb(239, 68,  68);
        private static readonly Color C_Orange = Color.FromArgb(234, 88,  12);
        private static readonly Color C_PageBg = Color.FromArgb(245, 247, 252);
        private static readonly Color C_Text   = Color.FromArgb(30,  30,  46);
        private static readonly Color C_Muted  = Color.FromArgb(107, 114, 128);

        // ════════════════════════════════════════════════════════════════════════
        public BillingForm()
        {
            Padding        = new Padding(28, 22, 22, 20);
            DoubleBuffered = true;
            BackColor      = C_PageBg;
            BuildUI();
            LoadProducts();
        }

        // ════════════════════════════════════════════════════════════════════════
        //  UI Build
        // ════════════════════════════════════════════════════════════════════════
        private void BuildUI()
        {
            // ── Page title ────────────────────────────────────────────────────
            Controls.Add(new Label
            {
                Text = "🧾  New Bill / Sale", Location = new Point(0, 0), AutoSize = true,
                Font = new Font("Segoe UI", 17f, FontStyle.Bold), ForeColor = C_Text
            });

            // ── Row 1: Payment + Notes ────────────────────────────────────────
            int y = 44;
            Lbl("Payment:", new Point(0, y + 4));
            _cmbPayment = new ComboBox
            {
                Location = new Point(72, y), Size = new Size(120, 28),
                DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 9.5f)
            };
            _cmbPayment.Items.AddRange(Constants.PaymentModes);
            _cmbPayment.SelectedIndex = 0;
            Controls.Add(_cmbPayment);

            Lbl("Notes / Desc:", new Point(210, y + 4));
            _txtNotes = new TextBox
            {
                Location = new Point(310, y), Size = new Size(620, 26),
                Font = new Font("Segoe UI", 9.5f),
                PlaceholderText = "Optional — customer name, repair details, remarks..."
            };
            Controls.Add(_txtNotes);

            // ── Section: Add Products ─────────────────────────────────────────
            y += 46;
            Controls.Add(new Label
            {
                Text = "Add Products", Location = new Point(0, y), AutoSize = true,
                Font = new Font("Segoe UI", 11f, FontStyle.Bold), ForeColor = C_Blue
            });

            y += 28;
            Lbl("Product:", new Point(0, y + 4));

            // DropDownList so custom entry doesn't pollute the list
            _cmbProduct = new ComboBox
            {
                Location = new Point(70, y), Size = new Size(280, 28),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 9.5f)
            };
            _cmbProduct.SelectedIndexChanged += CmbProduct_SelectedIndexChanged;
            Controls.Add(_cmbProduct);

            Lbl("Qty:", new Point(365, y + 4));
            _numQty = new NumericUpDown
            {
                Location = new Point(398, y), Size = new Size(70, 28),
                Minimum = 1, Maximum = 9999, Value = 1,
                Font = new Font("Segoe UI", 9.5f)
            };
            Controls.Add(_numQty);

            Lbl("Rate: ₹", new Point(482, y + 4));
            _lblUnitPrice = new Label
            {
                Text = "0.00", Location = new Point(540, y + 4), AutoSize = true,
                Font = new Font("Segoe UI", 10f, FontStyle.Bold), ForeColor = C_Blue
            };
            Controls.Add(_lblUnitPrice);

            var btnAdd = Btn("➕  Add", C_Blue, new Point(620, y), new Size(90, 28));
            btnAdd.Click += BtnAddToCart_Click;

            // ── Custom-item inline panel (hidden by default) ──────────────────
            y += 36;
            _pnlCustom = BuildCustomPanel(y);
            Controls.Add(_pnlCustom);

            // ── Cart DataGridView ─────────────────────────────────────────────
            int cartY = y + _pnlCustom.Height + 6;
            _dgvCart = new DataGridView
            {
                Location = new Point(0, cartY), Size = new Size(950, 270),
                Anchor   = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                AllowUserToAddRows  = false, ReadOnly = true,
                SelectionMode       = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor     = Color.White, BorderStyle = BorderStyle.FixedSingle,
                RowHeadersVisible   = false, Font = new Font("Segoe UI", 9.5f),
                GridColor           = Color.FromArgb(220, 220, 230)
            };
            _dgvCart.ColumnHeadersDefaultCellStyle.BackColor = C_Blue;
            _dgvCart.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            _dgvCart.ColumnHeadersDefaultCellStyle.Font      = new Font("Segoe UI", 9.5f, FontStyle.Bold);
            _dgvCart.EnableHeadersVisualStyles               = false;
            Controls.Add(_dgvCart);
            SetupCartColumns();

            var btnRem = Btn("🗑  Remove Row", C_Red, new Point(0, cartY + 278), new Size(140, 32));
            btnRem.Click += BtnRemoveRow_Click;

            // ── Totals section ────────────────────────────────────────────────
            int ty = cartY + 322;
            Lbl("Sub Total: ₹", new Point(560, ty));
            _lblSubTotal = new Label
            {
                Text = "0.00", Location = new Point(660, ty), AutoSize = true,
                Font = new Font("Segoe UI", 11f, FontStyle.Bold), ForeColor = C_Blue
            };
            Controls.Add(_lblSubTotal);

            ty += 26;
            Lbl("Discount: ₹", new Point(560, ty));
            _numDiscount = new NumericUpDown
            {
                Location = new Point(660, ty), Size = new Size(120, 26),
                Minimum = 0, Maximum = 9999999, DecimalPlaces = 2,
                Font = new Font("Segoe UI", 9.5f)
            };
            _numDiscount.ValueChanged += (_, _) => UpdateTotals();
            Controls.Add(_numDiscount);

            ty += 32;
            Controls.Add(new Panel { BackColor = Color.FromArgb(210, 210, 225), Bounds = new Rectangle(560, ty, 390, 1) });
            ty += 6;
            Controls.Add(new Label
            {
                Text = "TOTAL:  ₹", Location = new Point(560, ty), AutoSize = true,
                Font = new Font("Segoe UI", 13f, FontStyle.Bold), ForeColor = C_Text
            });
            _lblFinalTotal = new Label
            {
                Text = "0.00", Location = new Point(680, ty), AutoSize = true,
                Font = new Font("Segoe UI", 16f, FontStyle.Bold), ForeColor = C_Green
            };
            Controls.Add(_lblFinalTotal);

            ty += 40;
            var btnSave = Btn("💾  Save Bill", C_Green, new Point(560, ty), new Size(120, 38));
            _btnPrint = Btn("🖨  Print", C_Blue, new Point(690, ty), new Size(90, 38));
            _btnPrint.Enabled   = false;
            _btnPrint.BackColor = Color.FromArgb(150, 150, 180);
            var btnClear = Btn("🔄  Clear", Color.Gray, new Point(790, ty), new Size(80, 38));

            btnSave.Click   += BtnSave_Click;
            _btnPrint.Click += BtnPrint_Click;
            btnClear.Click  += (_, _) => ClearForm();
        }

        // ════════════════════════════════════════════════════════════════════════
        //  Custom-item inline panel
        // ════════════════════════════════════════════════════════════════════════
        private Panel BuildCustomPanel(int y)
        {
            var pnl = new Panel
            {
                Location  = new Point(0, y),
                Size      = new Size(780, 42),
                BackColor = Color.FromArgb(255, 248, 230),   // warm amber tint
                Visible   = false   // hidden until "Other" is selected
            };

            // Left accent bar
            pnl.Controls.Add(new Panel
            {
                BackColor = C_Orange,
                Bounds    = new Rectangle(0, 0, 4, 42)
            });

            // Label
            pnl.Controls.Add(new Label
            {
                Text      = "Custom Item — Name:",
                Location  = new Point(12, 13),
                AutoSize  = true,
                Font      = new Font("Segoe UI", 9f, FontStyle.Bold),
                ForeColor = C_Orange
            });

            // Custom name textbox
            _txtCustomName = new TextBox
            {
                Location        = new Point(160, 9),
                Size            = new Size(240, 26),
                Font            = new Font("Segoe UI", 9.5f),
                PlaceholderText = "e.g. Labour Charge, Misc Part..."
            };
            pnl.Controls.Add(_txtCustomName);

            // Rate label
            pnl.Controls.Add(new Label
            {
                Text      = "Rate ₹:",
                Location  = new Point(414, 13),
                AutoSize  = true,
                Font      = new Font("Segoe UI", 9f, FontStyle.Bold),
                ForeColor = C_Orange
            });

            // Custom rate NumericUpDown
            _numCustomRate = new NumericUpDown
            {
                Location      = new Point(460, 9),
                Size          = new Size(110, 26),
                Minimum       = 0,
                Maximum       = 9999999,
                DecimalPlaces = 2,
                Font          = new Font("Segoe UI", 9.5f)
            };
            // Mirror the rate label so user sees it at a glance
            _numCustomRate.ValueChanged += (_, _) =>
                _lblUnitPrice.Text = _numCustomRate.Value.ToString("N2");
            pnl.Controls.Add(_numCustomRate);

            // Tiny help text
            pnl.Controls.Add(new Label
            {
                Text      = "ℹ Stock not deducted for custom items",
                Location  = new Point(588, 14),
                AutoSize  = true,
                Font      = new Font("Segoe UI", 7.5f, FontStyle.Italic),
                ForeColor = Color.FromArgb(150, 100, 30)
            });

            return pnl;
        }

        // ════════════════════════════════════════════════════════════════════════
        //  Product ComboBox selection changed
        // ════════════════════════════════════════════════════════════════════════
        private void CmbProduct_SelectedIndexChanged(object? sender, EventArgs e)
        {
            bool isCustom = _cmbProduct.SelectedItem?.ToString() == CustomItemText;

            // Show/hide the custom panel
            _pnlCustom.Visible = isCustom;

            if (isCustom)
            {
                // Reset custom fields
                _txtCustomName.Text    = "";
                _numCustomRate.Value   = 0;
                _lblUnitPrice.Text     = "0.00";
                _txtCustomName.Focus();
            }
            else
            {
                UpdateUnitPrice();   // normal product → show its price
            }
        }

        // ════════════════════════════════════════════════════════════════════════
        //  Load product list
        // ════════════════════════════════════════════════════════════════════════
        private void LoadProducts()
        {
            try
            {
                _allProducts = _inventory.GetAllProducts();
                _cmbProduct.Items.Clear();

                foreach (var p in _allProducts)
                    _cmbProduct.Items.Add(p.Name);

                // ── "Other / Custom Item" always last in the list ──
                _cmbProduct.Items.Add(new ToolStripSeparator());   // visual gap
                _cmbProduct.Items.Add(CustomItemText);

                if (_cmbProduct.Items.Count > 0) _cmbProduct.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                ValidationHelper.ShowError("Could not load products: " + ex.Message);
            }
        }

        private void UpdateUnitPrice()
        {
            int idx = _cmbProduct.SelectedIndex;
            if (idx < 0 || idx >= _allProducts.Count)
            { _lblUnitPrice.Text = "0.00"; return; }
            _lblUnitPrice.Text = _allProducts[idx].SellingPrice.ToString("N2");
        }

        // ════════════════════════════════════════════════════════════════════════
        //  Add to cart
        // ════════════════════════════════════════════════════════════════════════
        private void BtnAddToCart_Click(object? sender, EventArgs e)
        {
            bool isCustom = _cmbProduct.SelectedItem?.ToString() == CustomItemText;

            if (isCustom)
                AddCustomItemToCart();
            else
                AddInventoryItemToCart();
        }

        private void AddInventoryItemToCart()
        {
            if (_cmbProduct.SelectedIndex < 0 || _cmbProduct.SelectedIndex >= _allProducts.Count)
            { ValidationHelper.ShowError("Please select a product."); return; }

            var prod = _allProducts[_cmbProduct.SelectedIndex];
            int qty  = (int)_numQty.Value;

            // Merge with existing cart row if same product
            var existing = _cartItems.FirstOrDefault(ci => ci.ProductId == prod.ProductId);
            if (existing != null)
                existing.Quantity += qty;
            else
                _cartItems.Add(new SaleItem
                {
                    ProductId   = prod.ProductId,
                    ProductName = prod.Name,
                    Quantity    = qty,
                    UnitPrice   = prod.SellingPrice
                });

            RebuildCartGrid();
            UpdateTotals();
        }

        private void AddCustomItemToCart()
        {
            string name = _txtCustomName.Text.Trim();
            if (string.IsNullOrEmpty(name))
            {
                ValidationHelper.ShowError("Please enter a name for the custom item.");
                _txtCustomName.Focus();
                return;
            }

            decimal rate = _numCustomRate.Value;
            if (rate <= 0)
            {
                ValidationHelper.ShowError("Please enter a price greater than 0 for the custom item.");
                _numCustomRate.Focus();
                return;
            }

            int qty = (int)_numQty.Value;

            // Custom items always get ProductId = 0 (bypasses stock deduction on save)
            _cartItems.Add(new SaleItem
            {
                ProductId   = 0,        // ← 0 = custom / non-inventory item
                ProductName = name,
                Quantity    = qty,
                UnitPrice   = rate
            });

            RebuildCartGrid();
            UpdateTotals();

            // Reset custom fields for next entry
            _txtCustomName.Text  = "";
            _numCustomRate.Value = 0;
            _txtCustomName.Focus();
        }

        // ════════════════════════════════════════════════════════════════════════
        //  Cart helpers
        // ════════════════════════════════════════════════════════════════════════
        private void SetupCartColumns()
        {
            _dgvCart.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "#",           Name = "SrNo",    Width = 42 });
            _dgvCart.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Product",      Name = "Product", FillWeight = 45 });
            _dgvCart.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Qty",          Name = "Qty",     Width = 65 });
            _dgvCart.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Unit Price ₹", Name = "Rate",    FillWeight = 25 });
            _dgvCart.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Total ₹",      Name = "Total",   FillWeight = 25 });
        }

        private void RebuildCartGrid()
        {
            _dgvCart.Rows.Clear();
            for (int i = 0; i < _cartItems.Count; i++)
            {
                var item = _cartItems[i];
                var row  = _dgvCart.Rows.Add(
                    i + 1,
                    item.ProductName,
                    item.Quantity,
                    item.UnitPrice.ToString("N2"),
                    item.TotalPrice.ToString("N2"));

                // Highlight custom items with a soft amber tint
                if (item.ProductId == 0)
                {
                    _dgvCart.Rows[row].DefaultCellStyle.BackColor = Color.FromArgb(255, 248, 230);
                    _dgvCart.Rows[row].DefaultCellStyle.ForeColor = Color.FromArgb(150, 80, 0);
                }
            }
        }

        private void BtnRemoveRow_Click(object? sender, EventArgs e)
        {
            if (_dgvCart.SelectedRows.Count == 0)
            { ValidationHelper.ShowError("Select a row to remove."); return; }

            int idx = _dgvCart.SelectedRows[0].Index;
            if (idx >= 0 && idx < _cartItems.Count)
            {
                _cartItems.RemoveAt(idx);
                RebuildCartGrid();
                UpdateTotals();
            }
        }

        private void UpdateTotals()
        {
            decimal sub   = _cartItems.Sum(i => i.TotalPrice);
            decimal disc  = _numDiscount.Value;
            decimal total = Math.Max(0, sub - disc);
            _lblSubTotal.Text   = sub.ToString("N2");
            _lblFinalTotal.Text = total.ToString("N2");
        }

        // ════════════════════════════════════════════════════════════════════════
        //  Save
        // ════════════════════════════════════════════════════════════════════════
        private void BtnSave_Click(object? sender, EventArgs e)
        {
            try
            {
                var (ok, msg, saved) = _billing.CreateSale(
                    _cartItems.ToList(),
                    _numDiscount.Value,
                    _cmbPayment.SelectedItem?.ToString() ?? "Cash",
                    _txtNotes.Text.Trim());

                if (ok)
                {
                    _lastSavedSale      = saved;
                    _btnPrint.Enabled   = true;
                    _btnPrint.BackColor = C_Blue;
                    ValidationHelper.ShowSuccess(msg);
                    ClearForm();
                }
                else ValidationHelper.ShowError(msg);
            }
            catch (Exception ex)
            {
                ValidationHelper.ShowError("Error saving bill: " + ex.Message);
            }
        }

        private void BtnPrint_Click(object? sender, EventArgs e)
        {
            if (_lastSavedSale == null)
            { ValidationHelper.ShowError("No bill to print. Please save a bill first."); return; }
            PrintHelper.PrintReceipt(_lastSavedSale);
        }

        private void ClearForm()
        {
            _cartItems.Clear();
            _dgvCart.Rows.Clear();
            _numDiscount.Value = 0;
            _txtNotes.Text     = "";
            _pnlCustom.Visible = false;
            if (_cmbPayment.Items.Count > 0) _cmbPayment.SelectedIndex = 0;
            UpdateTotals();
            LoadProducts();
        }

        // ════════════════════════════════════════════════════════════════════════
        //  Helpers
        // ════════════════════════════════════════════════════════════════════════
        private void Lbl(string text, Point pt) =>
            Controls.Add(new Label
            {
                Text = text, Location = pt, AutoSize = true,
                Font = new Font("Segoe UI", 9.5f), ForeColor = C_Muted
            });

        private Button Btn(string text, Color color, Point loc, Size size)
        {
            var b = new Button
            {
                Text = text, BackColor = color, ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat, Location = loc, Size = size,
                Cursor = Cursors.Hand, Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                FlatAppearance = { BorderSize = 0 }, Tag = "accent"
            };
            Controls.Add(b);
            return b;
        }
    }
}
