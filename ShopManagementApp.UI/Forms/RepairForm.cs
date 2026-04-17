using ShopManagementApp.Business.Services;
using ShopManagementApp.Models;
using ShopManagementApp.Utils;

namespace ShopManagementApp.UI.Forms
{
    /// <summary>
    /// Repair Form — Log and manage repair jobs.
    ///
    /// Layout:
    ///  [Top]    Input GroupBox: customer, device, fault, technician, cost
    ///  [Middle] Filter/Search row  +  DataGridView of repairs
    ///  [Bottom] Buttons: Save | Update Status | Print Token | Delete
    /// </summary>
    public class RepairForm : Form
    {
        private readonly RepairService _repairService = new RepairService();

        // Input controls
        private TextBox       _txtCustomerName  = null!;
        private TextBox       _txtCustomerPhone = null!;
        private ComboBox      _cmbDeviceType    = null!;
        private TextBox       _txtFault         = null!;
        private TextBox       _txtTechnician    = null!;
        private NumericUpDown _numEstCost       = null!;
        private TextBox       _txtNotes         = null!;
        private TextBox       _txtSearch        = null!;
        private DataGridView  _dgvRepairs       = null!;
        private ComboBox      _cmbStatusFilter  = null!;

        // Currently selected repair ID (for update/delete)
        private int _selectedRepairId = 0;

        public RepairForm()
        {
            Padding        = new Padding(28, 22, 22, 20);
            DoubleBuffered = true;
            BackColor      = Color.FromArgb(245, 247, 252);
            BuildUI();
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }

        // ════════════════════════════════════════════════════════════════════════
        private void BuildUI()
        {
            AddTitle("🔧 Repair Management", new Point(20, 15));

            // ── Input GroupBox ──
            var inputBox = new GroupBox
            {
                Text     = " New Repair Entry ",
                Location = new Point(20, 55),
                Size     = new Size(940, 180),
                Font     = new Font("Segoe UI", 9.5f)
            };
            Controls.Add(inputBox);

            // Row 1: Customer Name | Phone | Device Type
            AddLabelTo(inputBox, "Customer Name:", new Point(10, 30));
            _txtCustomerName = AddTextBoxTo(inputBox, new Point(130, 27), 180, "Full name");

            AddLabelTo(inputBox, "Phone:", new Point(325, 30));
            _txtCustomerPhone = AddTextBoxTo(inputBox, new Point(385, 27), 140, "10-digit");

            AddLabelTo(inputBox, "Device Type:", new Point(540, 30));
            _cmbDeviceType = new ComboBox
            {
                Location = new Point(638, 27), Size = new Size(180, 28),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            _cmbDeviceType.Items.AddRange(Constants.DeviceTypes);
            _cmbDeviceType.SelectedIndex = 0;
            inputBox.Controls.Add(_cmbDeviceType);

            // Row 2: Fault Description | Technician
            AddLabelTo(inputBox, "Fault Description:", new Point(10, 75));
            _txtFault = AddTextBoxTo(inputBox, new Point(135, 72), 360, "Describe the problem...");

            AddLabelTo(inputBox, "Technician:", new Point(515, 75));
            _txtTechnician = AddTextBoxTo(inputBox, new Point(600, 72), 180, "Tech name");

            // Row 3: Estimated Cost | Notes | Save | Clear
            AddLabelTo(inputBox, "Est. Cost ₹:", new Point(10, 120));
            _numEstCost = new NumericUpDown
            {
                Location = new Point(100, 117), Size = new Size(130, 28),
                Minimum = 0, Maximum = 999999, DecimalPlaces = 2
            };
            inputBox.Controls.Add(_numEstCost);

            AddLabelTo(inputBox, "Notes:", new Point(250, 120));
            _txtNotes = AddTextBoxTo(inputBox, new Point(305, 117), 400, "Optional notes");

            var btnSave = MakeButtonOn(inputBox, "💾 Save Repair", Constants.Colors.AccentBlue, new Point(715, 112), new Size(130, 35));
            btnSave.Click += BtnSave_Click;

            var btnClear = MakeButtonOn(inputBox, "🔄 Clear", Color.Gray, new Point(855, 112), new Size(75, 35));
            btnClear.Click += (_, _) => ClearForm();

            // ── Filter / Search row ──
            AddLabel("Filter Status:", new Point(20, 252));
            _cmbStatusFilter = new ComboBox
            {
                Location = new Point(115, 249), Size = new Size(140, 28),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            _cmbStatusFilter.Items.Add("All");
            _cmbStatusFilter.Items.AddRange(Constants.RepairStatuses);
            _cmbStatusFilter.SelectedIndex = 0;
            _cmbStatusFilter.SelectedIndexChanged += (_, _) => LoadRepairs();
            Controls.Add(_cmbStatusFilter);

            AddLabel("Search:", new Point(275, 252));
            _txtSearch = new TextBox
            {
                Location = new Point(330, 249), Size = new Size(200, 28),
                PlaceholderText = "Name or phone..."
            };
            _txtSearch.TextChanged += (_, _) => LoadRepairs();
            Controls.Add(_txtSearch);

            // ── Repairs DataGridView ──
            _dgvRepairs = new DataGridView
            {
                Location           = new Point(20, 290),
                Size               = new Size(940, 280),
                Anchor             = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom,
                AllowUserToAddRows = false,
                ReadOnly           = true,
                SelectionMode      = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor    = Color.White,
                BorderStyle        = BorderStyle.FixedSingle,
                RowHeadersVisible  = false,
                GridColor          = Color.FromArgb(220, 220, 230),
                Font               = new Font("Segoe UI", 9.5f)
            };
            _dgvRepairs.ColumnHeadersDefaultCellStyle.BackColor = Constants.Colors.AccentOrange;
            _dgvRepairs.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            _dgvRepairs.ColumnHeadersDefaultCellStyle.Font      = new Font("Segoe UI", 9.5f, FontStyle.Bold);
            _dgvRepairs.EnableHeadersVisualStyles               = false;
            _dgvRepairs.CellDoubleClick += DgvRepairs_DoubleClick;
            Controls.Add(_dgvRepairs);
            SetupRepairColumns();

            // ── Action buttons ──
            var btnUpdate = MakeButton("🔄 Update Status", Constants.Colors.AccentOrange, new Point(20,  585), new Size(150, 38));
            var btnPrint  = MakeButton("🖨 Print Token",   Constants.Colors.AccentBlue,   new Point(185, 585), new Size(130, 38));
            var btnDelete = MakeButton("🗑 Delete",        Constants.Colors.AccentRed,    new Point(330, 585), new Size(100, 38));

            btnUpdate.Click += BtnUpdateStatus_Click;
            btnPrint.Click  += BtnPrint_Click;
            btnDelete.Click += BtnDelete_Click;

            LoadRepairs();
        }

        // ── Grid columns: 9 total (1 hidden + 8 visible) ──────────────────────
        private void SetupRepairColumns()
        {
            _dgvRepairs.Columns.Add(new DataGridViewTextBoxColumn { Name = "RepairId", Visible = false });
            _dgvRepairs.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Job#",     Name = "JobNo",    Width = 58 });
            _dgvRepairs.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Customer", Name = "Customer", FillWeight = 35 });
            _dgvRepairs.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Phone",    Name = "Phone",    Width = 105 });
            _dgvRepairs.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Device",   Name = "Device",   FillWeight = 20 });
            _dgvRepairs.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Fault",    Name = "Fault",    FillWeight = 30 });
            _dgvRepairs.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Est. ₹",  Name = "EstCost",  Width = 72 });
            _dgvRepairs.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Status",   Name = "Status",   Width = 100 });
            _dgvRepairs.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Received", Name = "Date",     Width = 88 });
            // Total: 9 columns (1 hidden + 8 visible) — Rows.Add must pass exactly 9 values
        }

        // ── Load & filter repairs ─────────────────────────────────────────────
        private void LoadRepairs()
        {
            var repairs = _repairService.GetAllRepairs();

            string statusFilter = _cmbStatusFilter.SelectedItem?.ToString() ?? "All";
            if (statusFilter != "All")
                repairs = repairs.Where(r => r.Status == statusFilter).ToList();

            string search = _txtSearch.Text.ToLower().Trim();
            if (!string.IsNullOrEmpty(search))
                repairs = repairs.Where(r =>
                    r.CustomerName.ToLower().Contains(search) ||
                    r.CustomerPhone.Contains(search)).ToList();

            _dgvRepairs.Rows.Clear();
            foreach (var r in repairs)
            {
                // FIXED Bug 2: pass exactly 9 values matching 9 columns (removed Technician)
                int rowIdx = _dgvRepairs.Rows.Add(
                    r.RepairId,
                    $"R-{r.RepairId}",
                    r.CustomerName,
                    r.CustomerPhone,
                    r.DeviceType,
                    r.FaultDescription,
                    r.EstimatedCost.ToString("N0"),   // ← was: r.Technician (wrong column)
                    r.Status,
                    r.ReceivedDate.ToString("dd/MM/yy"));

                // FIXED Bug 5: light theme only — no IsDark branch needed
                Color rowColor = r.Status switch
                {
                    "Pending"     => Color.FromArgb(255, 243, 220),
                    "In Progress" => Color.FromArgb(219, 234, 254),
                    "Completed"   => Color.FromArgb(220, 252, 231),
                    "Delivered"   => Color.FromArgb(243, 244, 246),
                    _             => Color.White
                };
                _dgvRepairs.Rows[rowIdx].DefaultCellStyle.BackColor = rowColor;
                _dgvRepairs.Rows[rowIdx].DefaultCellStyle.ForeColor = Color.FromArgb(30, 30, 46);
            }
        }

        // ── Double-click loads repair into input form for editing ─────────────
        private void DgvRepairs_DoubleClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            int repairId = Convert.ToInt32(_dgvRepairs.Rows[e.RowIndex].Cells["RepairId"].Value);
            var repair   = _repairService.GetById(repairId);
            if (repair == null) return;

            _selectedRepairId           = repair.RepairId;
            _txtCustomerName.Text       = repair.CustomerName;
            _txtCustomerPhone.Text      = repair.CustomerPhone;
            _cmbDeviceType.SelectedItem = repair.DeviceType;
            _txtFault.Text              = repair.FaultDescription;
            _txtTechnician.Text         = repair.Technician;
            _numEstCost.Value           = Math.Min(repair.EstimatedCost, _numEstCost.Maximum);
            _txtNotes.Text              = repair.Notes;
        }

        // ── Save button ───────────────────────────────────────────────────────
        private void BtnSave_Click(object? sender, EventArgs e)
        {
            var repair = new Repair
            {
                RepairId         = _selectedRepairId,
                CustomerName     = _txtCustomerName.Text.Trim(),
                CustomerPhone    = _txtCustomerPhone.Text.Trim(),
                DeviceType       = _cmbDeviceType.SelectedItem?.ToString() ?? "",
                FaultDescription = _txtFault.Text.Trim(),
                Technician       = _txtTechnician.Text.Trim(),
                EstimatedCost    = _numEstCost.Value,
                Notes            = _txtNotes.Text.Trim()
            };

            if (_selectedRepairId > 0)
            {
                // Update existing
                repair.Status = _repairService.GetById(_selectedRepairId)?.Status ?? "Pending";
                var (ok, msg) = _repairService.UpdateRepair(repair);
                if (ok) { ValidationHelper.ShowSuccess(msg); ClearForm(); LoadRepairs(); }
                else      ValidationHelper.ShowError(msg);
            }
            else
            {
                // Create new
                var (ok, msg, _) = _repairService.CreateRepair(repair);
                if (ok) { ValidationHelper.ShowSuccess(msg); ClearForm(); LoadRepairs(); }
                else      ValidationHelper.ShowError(msg);
            }
        }

        // ── Update Status dialog ──────────────────────────────────────────────
        private void BtnUpdateStatus_Click(object? sender, EventArgs e)
        {
            if (_dgvRepairs.SelectedRows.Count == 0)
            { ValidationHelper.ShowError("Select a repair job from the list first."); return; }

            int repairId = Convert.ToInt32(_dgvRepairs.SelectedRows[0].Cells["RepairId"].Value);

            var dlg = new Form
            {
                Text = "Update Repair Status", Size = new Size(320, 200),
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedDialog, MaximizeBox = false
            };
            var cmb = new ComboBox { Location = new Point(20, 30), Size = new Size(260, 28), DropDownStyle = ComboBoxStyle.DropDownList };
            cmb.Items.AddRange(Constants.RepairStatuses);
            cmb.SelectedIndex = 0;

            var lblCost = new Label { Text = "Final Cost ₹ (if Delivered):", Location = new Point(20, 75), AutoSize = true };
            var numCost = new NumericUpDown { Location = new Point(20, 95), Size = new Size(150, 28), Minimum = 0, Maximum = 999999, DecimalPlaces = 2 };
            var btnOk   = new Button { Text = "Update", Location = new Point(180, 130), Size = new Size(100, 35),
                BackColor = Constants.Colors.AccentOrange, ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat, FlatAppearance = { BorderSize = 0 }, DialogResult = DialogResult.OK };

            dlg.Controls.AddRange(new Control[] { cmb, lblCost, numCost, btnOk });
            dlg.AcceptButton = btnOk;

            if (dlg.ShowDialog(this) == DialogResult.OK)
            {
                var (ok, msg) = _repairService.UpdateStatus(repairId, cmb.SelectedItem?.ToString() ?? "Pending", numCost.Value);
                if (ok) { ValidationHelper.ShowSuccess(msg); LoadRepairs(); }
                else      ValidationHelper.ShowError(msg);
            }
        }

        // ── Print Token ───────────────────────────────────────────────────────
        private void BtnPrint_Click(object? sender, EventArgs e)
        {
            if (_dgvRepairs.SelectedRows.Count == 0)
            { ValidationHelper.ShowError("Select a repair job to print."); return; }

            int repairId = Convert.ToInt32(_dgvRepairs.SelectedRows[0].Cells["RepairId"].Value);
            var repair   = _repairService.GetById(repairId);
            if (repair != null) PrintHelper.PrintRepairToken(repair);
        }

        // ── Delete ────────────────────────────────────────────────────────────
        private void BtnDelete_Click(object? sender, EventArgs e)
        {
            if (_dgvRepairs.SelectedRows.Count == 0)
            { ValidationHelper.ShowError("Select a repair job to delete."); return; }

            if (!ValidationHelper.Confirm("Delete this repair job? This cannot be undone.")) return;

            int repairId = Convert.ToInt32(_dgvRepairs.SelectedRows[0].Cells["RepairId"].Value);
            var (ok, msg) = _repairService.DeleteRepair(repairId);
            if (ok) { ValidationHelper.ShowSuccess(msg); LoadRepairs(); }
            else      ValidationHelper.ShowError(msg);
        }

        // ── Clear form ────────────────────────────────────────────────────────
        private void ClearForm()
        {
            _selectedRepairId = 0;
            _txtCustomerName.Text  = "";
            _txtCustomerPhone.Text = "";
            _cmbDeviceType.SelectedIndex = 0;
            _txtFault.Text     = "";
            _txtTechnician.Text = "";
            _numEstCost.Value  = 0;
            _txtNotes.Text     = "";
        }

        // ── UI Helpers ────────────────────────────────────────────────────────
        private void AddTitle(string text, Point pt) =>
            Controls.Add(new Label { Text = text, Location = pt, AutoSize = true,
                Font = new Font("Segoe UI", 16f, FontStyle.Bold), ForeColor = Constants.Colors.TextDark });

        private void AddLabel(string text, Point pt) =>
            Controls.Add(new Label { Text = text, Location = pt, AutoSize = true, Font = new Font("Segoe UI", 9.5f) });

        private static void AddLabelTo(Control parent, string text, Point pt) =>
            parent.Controls.Add(new Label { Text = text, Location = pt, AutoSize = true, Font = new Font("Segoe UI", 9.5f) });

        private static TextBox AddTextBoxTo(Control parent, Point pt, int width, string placeholder = "")
        {
            var tb = new TextBox { Location = pt, Size = new Size(width, 28), Font = new Font("Segoe UI", 10f), PlaceholderText = placeholder };
            parent.Controls.Add(tb);
            return tb;
        }

        private Button MakeButton(string text, Color color, Point location, Size size)
        {
            var btn = new Button { Text = text, BackColor = color, ForeColor = Color.White, FlatStyle = FlatStyle.Flat,
                Location = location, Size = size, Cursor = Cursors.Hand, Font = new Font("Segoe UI", 9.5f),
                FlatAppearance = { BorderSize = 0 }, Tag = "accent" };
            Controls.Add(btn);
            return btn;
        }

        private static Button MakeButtonOn(Control parent, string text, Color color, Point location, Size size)
        {
            var btn = new Button { Text = text, BackColor = color, ForeColor = Color.White, FlatStyle = FlatStyle.Flat,
                Location = location, Size = size, Cursor = Cursors.Hand, Font = new Font("Segoe UI", 9.5f),
                FlatAppearance = { BorderSize = 0 }, Tag = "accent" };
            parent.Controls.Add(btn);
            return btn;
        }
    }
}
