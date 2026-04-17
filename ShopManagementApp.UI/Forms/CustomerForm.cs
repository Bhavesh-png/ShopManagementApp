using ShopManagementApp.Business.Services;
using ShopManagementApp.Models;
using ShopManagementApp.Utils;

namespace ShopManagementApp.UI.Forms
{
    /// <summary>
    /// Customer Form — Add, edit, delete, and search customers.
    /// Also shows a customer's purchase and repair history.
    /// </summary>
    public class CustomerForm : Form
    {
        private readonly CustomerService _customerService = new CustomerService();
        private readonly BillingService  _billingService  = new BillingService();
        private readonly RepairService   _repairService   = new RepairService();

        private TextBox    _txtName    = null!;
        private TextBox    _txtPhone   = null!;
        private TextBox    _txtEmail   = null!;
        private TextBox    _txtAddress = null!;
        private TextBox    _txtSearch  = null!;
        private DataGridView _dgvCustomers = null!;
        private Label      _lblHistory     = null!;

        private int _selectedCustomerId = 0;

        public CustomerForm()
        {
            BuildUI();
            ThemeManager.Apply(this);
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }

        private void BuildUI()
        {
            AddTitle("👤 Customer Management", new Point(20, 15));

            // ══════════ INPUT FORM ════════════════════════════════════════════
            var box = new GroupBox
            {
                Text     = " Customer Details ",
                Location = new Point(20, 55),
                Size     = new Size(340, 260),
                Font     = new Font("Segoe UI", 9.5f)
            };
            Controls.Add(box);

            int y = 28;
            _txtName    = AddField(box, "Full Name *",     ref y);
            _txtPhone   = AddField(box, "Phone (10-digit)*", ref y);
            _txtEmail   = AddField(box, "Email (optional)", ref y);
            _txtAddress = AddField(box, "Address",          ref y, height: 50);

            var btnAdd    = MakeBtn(box, "➕ Add",    Constants.Colors.AccentGreen,  new Point(10, y + 5), new Size(90, 34));
            var btnUpdate = MakeBtn(box, "✏ Update",  Constants.Colors.AccentBlue,   new Point(110, y + 5), new Size(85, 34));
            var btnDelete = MakeBtn(box, "🗑 Delete",  Constants.Colors.AccentRed,    new Point(205, y + 5), new Size(80, 34));
            var btnClear  = MakeBtn(box, "🔄 Clear",  Color.Gray,                    new Point(295, y + 5), new Size(35, 34));

            btnAdd.Click    += BtnAdd_Click;
            btnUpdate.Click += BtnUpdate_Click;
            btnDelete.Click += BtnDelete_Click;
            btnClear.Click  += (_, _) => ClearForm();

            // ══════════ SEARCH + CUSTOMER LIST ════════════════════════════════
            AddLabel("🔍 Search:", new Point(375, 62));
            _txtSearch = new TextBox
            {
                Location        = new Point(440, 59),
                Size            = new Size(200, 28),
                PlaceholderText = "Name or phone..."
            };
            _txtSearch.TextChanged += (_, _) => LoadCustomers();
            Controls.Add(_txtSearch);

            _dgvCustomers = BuildGrid(new Point(375, 95), new Size(565, 220),
                Constants.Colors.AccentBlue, "CustId",
                new[] { ("ID", 50), ("Name", 0), ("Phone", 120), ("Email", 160), ("Address", 0) });
            _dgvCustomers.CellClick += DgvCustomers_Click;

            // ══════════ HISTORY PANEL ═════════════════════════════════════════
            _lblHistory = new Label
            {
                Text      = "📋 Customer History",
                Location  = new Point(20, 340),
                AutoSize  = true,
                Font      = new Font("Segoe UI", 12f, FontStyle.Bold),
                ForeColor = Constants.Colors.TextDark
            };
            Controls.Add(_lblHistory);

            // Purchases sub-grid
            AddLabel("Recent Purchases:", new Point(20, 375));
            var dgvSales = BuildGrid(new Point(20, 397), new Size(460, 180),
                Constants.Colors.AccentGreen, null,
                new[] { ("Sale#", 60), ("Date", 100), ("Items", 0), ("Amount ₹", 100), ("Payment", 100) });

            // Repairs sub-grid
            AddLabel("Repair Jobs:", new Point(500, 375));
            var dgvRepairs = BuildGrid(new Point(500, 397), new Size(440, 180),
                Constants.Colors.AccentOrange, null,
                new[] { ("Job#", 60), ("Device", 0), ("Status", 100), ("Est. ₹", 90), ("Date", 95) });

            // Store refs for refresh
            _dgvSalesHistory  = dgvSales;
            _dgvRepairHistory = dgvRepairs;

            LoadCustomers();
        }

        private DataGridView _dgvSalesHistory  = null!;
        private DataGridView _dgvRepairHistory = null!;

        private DataGridView BuildGrid(Point loc, Size size, Color headerColor, string? hiddenCol, (string header, int width)[] cols)
        {
            var dgv = new DataGridView
            {
                Location          = loc,
                Size              = size,
                AllowUserToAddRows = false,
                ReadOnly          = true,
                SelectionMode     = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor   = Color.White,
                BorderStyle       = BorderStyle.None,
                RowHeadersVisible = false,
                Font              = new Font("Segoe UI", 9f)
            };
            dgv.ColumnHeadersDefaultCellStyle.BackColor = headerColor;
            dgv.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgv.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
            dgv.EnableHeadersVisualStyles = false;

            if (hiddenCol != null)
                dgv.Columns.Add(new DataGridViewTextBoxColumn { Name = hiddenCol, Visible = false });

            foreach (var (header, width) in cols)
            {
                var col = new DataGridViewTextBoxColumn { HeaderText = header };
                if (width > 0) col.Width = width; else col.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                dgv.Columns.Add(col);
            }

            Controls.Add(dgv);
            return dgv;
        }

        private void LoadCustomers()
        {
            var customers = _customerService.SearchCustomers(_txtSearch.Text);
            _dgvCustomers.Rows.Clear();
            foreach (var c in customers)
                // hidden CustId | visible: ID, Name, Phone, Email, Address
                _dgvCustomers.Rows.Add(c.CustomerId, c.CustomerId, c.Name, c.Phone, c.Email, c.Address);
        }

        private void DgvCustomers_Click(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            int id       = Convert.ToInt32(_dgvCustomers.Rows[e.RowIndex].Cells["CustId"].Value);
            var customer = _customerService.GetById(id);
            if (customer == null) return;

            _selectedCustomerId = customer.CustomerId;
            _txtName.Text    = customer.Name;
            _txtPhone.Text   = customer.Phone;
            _txtEmail.Text   = customer.Email;
            _txtAddress.Text = customer.Address;

            LoadHistory(customer.CustomerId);
        }

        private void LoadHistory(int customerId)
        {
            // Sales history disabled — customer tracking removed
            _dgvSalesHistory.Rows.Clear();

            // Load repair history
            var repairs = _repairService.GetByCustomer(customerId);
            _dgvRepairHistory.Rows.Clear();
            foreach (var r in repairs)
                _dgvRepairHistory.Rows.Add($"R-{r.RepairId}", r.DeviceType,
                    r.Status, $"₹{r.EstimatedCost:N0}", r.ReceivedDate.ToString("dd/MM/yy"));
        }

        private void BtnAdd_Click(object? sender, EventArgs e)
        {
            var (success, message, customer) = _customerService.AddCustomer(BuildCustomer());
            if (success) { ValidationHelper.ShowSuccess(message); ClearForm(); LoadCustomers(); }
            else ValidationHelper.ShowError(message);
        }

        private void BtnUpdate_Click(object? sender, EventArgs e)
        {
            if (_selectedCustomerId == 0) { ValidationHelper.ShowError("Click a customer to select them first."); return; }
            var c = BuildCustomer();
            c.CustomerId = _selectedCustomerId;
            var (success, message) = _customerService.UpdateCustomer(c);
            if (success) { ValidationHelper.ShowSuccess(message); ClearForm(); LoadCustomers(); }
            else ValidationHelper.ShowError(message);
        }

        private void BtnDelete_Click(object? sender, EventArgs e)
        {
            if (_selectedCustomerId == 0) { ValidationHelper.ShowError("Select a customer first."); return; }
            if (!ValidationHelper.Confirm("Delete this customer?")) return;
            var (success, message) = _customerService.DeleteCustomer(_selectedCustomerId);
            if (success) { ValidationHelper.ShowSuccess(message); ClearForm(); LoadCustomers(); }
            else ValidationHelper.ShowError(message);
        }

        private Customer BuildCustomer() => new Customer
        {
            Name    = _txtName.Text.Trim(),
            Phone   = _txtPhone.Text.Trim(),
            Email   = _txtEmail.Text.Trim(),
            Address = _txtAddress.Text.Trim()
        };

        private void ClearForm()
        {
            _selectedCustomerId = 0;
            _txtName.Text = _txtPhone.Text = _txtEmail.Text = _txtAddress.Text = "";
            _dgvSalesHistory.Rows.Clear();
            _dgvRepairHistory.Rows.Clear();
        }

        // ── UI Helpers ────────────────────────────────────────────────────────
        private void AddTitle(string t, Point p) =>
            Controls.Add(new Label { Text = t, Location = p, AutoSize = true,
                Font = new Font("Segoe UI", 16f, FontStyle.Bold), ForeColor = Constants.Colors.TextDark });

        private void AddLabel(string t, Point p) =>
            Controls.Add(new Label { Text = t, Location = p, AutoSize = true, Font = new Font("Segoe UI", 9.5f) });

        private static TextBox AddField(GroupBox box, string label, ref int y, int height = 27)
        {
            box.Controls.Add(new Label { Text = label, Location = new Point(10, y), AutoSize = true, Font = new Font("Segoe UI", 9f) });
            y += 18;
            var tb = new TextBox { Location = new Point(10, y), Size = new Size(310, height), Font = new Font("Segoe UI", 10f), Multiline = height > 30 };
            box.Controls.Add(tb);
            y += height + 8;
            return tb;
        }

        private static Button MakeBtn(Control parent, string text, Color color, Point loc, Size size)
        {
            var btn = new Button { Text = text, BackColor = color, ForeColor = Color.White, FlatStyle = FlatStyle.Flat,
                Location = loc, Size = size, Cursor = Cursors.Hand, Font = new Font("Segoe UI", 8.5f),
                FlatAppearance = { BorderSize = 0 }, Tag = "accent" };  // Tag prevents ThemeManager override
            parent.Controls.Add(btn);
            return btn;
        }
    }
}
