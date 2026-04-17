using ShopManagementApp.Models;
using System.Drawing.Printing;

namespace ShopManagementApp.Utils
{
    /// <summary>
    /// Handles printing receipts and repair job cards using PrintDocument.
    /// Shows a PrintPreviewDialog so the user can review before printing.
    /// </summary>
    public static class PrintHelper
    {
        // Static fields to hold the data being printed
        private static Sale? _currentSale;
        private static Repair? _currentRepair;

        // ── Print Sales Receipt ───────────────────────────────────────────────

        /// <summary>Opens a print preview for a sales receipt.</summary>
        public static void PrintReceipt(Sale sale)
        {
            _currentSale = sale;
            var printDoc = new PrintDocument();
            printDoc.DefaultPageSettings.PaperSize =
                new PaperSize("Receipt", 320, 600); // 80mm thermal width approx
            printDoc.PrintPage += PrintReceiptPage;

            var preview = new PrintPreviewDialog
            {
                Document = printDoc,
                Width = 500,
                Height = 700,
                Text = $"Receipt Preview - Sale #{sale.SaleId}"
            };
            preview.ShowDialog();
        }

        private static void PrintReceiptPage(object sender, PrintPageEventArgs e)
        {
            if (_currentSale == null || e.Graphics == null) return;

            var g = e.Graphics;
            float y = 15f;
            float pw = e.PageBounds.Width; // page width

            // Fonts
            var fTitle  = new Font("Consolas", 12, FontStyle.Bold);
            var fBold   = new Font("Consolas", 9,  FontStyle.Bold);
            var fNormal = new Font("Consolas", 8,  FontStyle.Regular);
            var fTotal  = new Font("Consolas", 10, FontStyle.Bold);
            var center  = new StringFormat { Alignment = StringAlignment.Center };
            var right   = new StringFormat { Alignment = StringAlignment.Far };

            // ── Header ──
            g.DrawString(Constants.ShopName,    fTitle,  Brushes.Black, new RectangleF(0, y, pw, 25), center); y += 22;
            g.DrawString(Constants.ShopAddress, fNormal, Brushes.Black, new RectangleF(0, y, pw, 18), center); y += 15;
            g.DrawString($"Ph: {Constants.ShopPhone}", fNormal, Brushes.Black, new RectangleF(0, y, pw, 18), center); y += 15;
            g.DrawString(Constants.ShopGST,     fNormal, Brushes.Black, new RectangleF(0, y, pw, 18), center); y += 12;

            DrawDashedLine(g, pw, y); y += 10;

            // ── Bill Info ──
            g.DrawString($"Receipt #: {_currentSale.SaleId}", fBold, Brushes.Black, 10, y);
            g.DrawString($"Date: {_currentSale.SaleDate:dd/MM/yy HH:mm}", fNormal, Brushes.Black,
                new RectangleF(0, y, pw - 10, 18), right); y += 16;
            g.DrawString($"Payment : {_currentSale.PaymentMode}", fNormal, Brushes.Black, 10, y); y += 14;
            if (!string.IsNullOrEmpty(_currentSale.Notes))
            { g.DrawString($"Notes   : {_currentSale.Notes}", fNormal, Brushes.Black, 10, y); y += 14; }

            DrawDashedLine(g, pw, y); y += 8;

            // ── Column Headers ──
            g.DrawString("Item",  fBold, Brushes.Black, 10,       y);
            g.DrawString("Qty",   fBold, Brushes.Black, 175,      y);
            g.DrawString("Rate",  fBold, Brushes.Black, 215,      y);
            g.DrawString("Amt",   fBold, Brushes.Black, pw - 40,  y); y += 16;
            DrawDashedLine(g, pw, y); y += 6;

            // ── Items ──
            foreach (var item in _currentSale.Items)
            {
                // Truncate long names
                string name = item.ProductName.Length > 22
                    ? item.ProductName[..22] : item.ProductName;
                g.DrawString(name,                         fNormal, Brushes.Black, 10,      y);
                g.DrawString(item.Quantity.ToString(),     fNormal, Brushes.Black, 175,     y);
                g.DrawString($"{item.UnitPrice:N0}",       fNormal, Brushes.Black, 215,     y);
                g.DrawString($"{item.TotalPrice:N0}",      fNormal, Brushes.Black, pw - 40, y);
                y += 15;
            }

            DrawDashedLine(g, pw, y); y += 8;

            // ── Totals ──
            g.DrawString($"Sub Total  :",     fBold,  Brushes.Black, 120, y);
            g.DrawString($"Rs.{_currentSale.SubTotal:N2}", fNormal, Brushes.Black, pw - 80, y); y += 15;

            if (_currentSale.Discount > 0)
            {
                g.DrawString($"Discount   :",   fBold,  Brushes.Black, 120, y);
                g.DrawString($"-Rs.{_currentSale.Discount:N2}", fNormal, Brushes.Black, pw - 80, y); y += 15;
            }

            DrawSolidLine(g, pw, y); y += 6;
            g.DrawString("TOTAL :", fTotal, Brushes.Black, 100, y);
            g.DrawString($"Rs.{_currentSale.FinalAmount:N2}", fTotal, Brushes.Black, pw - 90, y); y += 30;

            // ── Footer ──
            DrawDashedLine(g, pw, y); y += 8;
            g.DrawString("** Thank You! Visit Again **", fNormal, Brushes.Black,
                new RectangleF(0, y, pw, 18), center);
        }

        // ── Print Repair Job Card ─────────────────────────────────────────────

        /// <summary>Opens a print preview for a repair job card.</summary>
        public static void PrintRepairToken(Repair repair)
        {
            _currentRepair = repair;
            var printDoc = new PrintDocument();
            printDoc.PrintPage += PrintRepairPage;

            var preview = new PrintPreviewDialog
            {
                Document = printDoc,
                Width = 600,
                Height = 650,
                Text = $"Repair Job Card - R-{repair.RepairId}"
            };
            preview.ShowDialog();
        }

        private static void PrintRepairPage(object sender, PrintPageEventArgs e)
        {
            if (_currentRepair == null || e.Graphics == null) return;

            var g  = e.Graphics;
            float y  = 20f;
            float pw = e.PageBounds.Width;

            var fTitle  = new Font("Arial", 14, FontStyle.Bold);
            var fHead   = new Font("Arial", 11, FontStyle.Bold);
            var fLabel  = new Font("Arial", 9,  FontStyle.Bold);
            var fNormal = new Font("Arial", 9,  FontStyle.Regular);
            var center  = new StringFormat { Alignment = StringAlignment.Center };

            g.DrawString(Constants.ShopName, fTitle, Brushes.Black, new RectangleF(0, y, pw, 30), center); y += 28;
            g.DrawString("REPAIR JOB CARD", fHead, Brushes.Black, new RectangleF(0, y, pw, 22), center); y += 25;

            DrawSolidLine(g, pw, y); y += 10;

            void Row(string label, string value)
            {
                g.DrawString(label, fLabel,  Brushes.DarkGray, 30,  y);
                g.DrawString(value, fNormal, Brushes.Black,    180, y);
                y += 20;
            }

            Row("Job No     :", $"R-{_currentRepair.RepairId}");
            Row("Date       :", _currentRepair.ReceivedDate.ToString("dd/MM/yyyy"));
            Row("Customer   :", _currentRepair.CustomerName);
            Row("Phone      :", _currentRepair.CustomerPhone);
            Row("Device     :", _currentRepair.DeviceType);
            Row("Fault      :", _currentRepair.FaultDescription);
            Row("Technician :", _currentRepair.Technician);
            Row("Est. Cost  :", $"Rs.{_currentRepair.EstimatedCost:N2}");
            Row("Status     :", _currentRepair.Status);
            if (!string.IsNullOrEmpty(_currentRepair.Notes))
                Row("Notes      :", _currentRepair.Notes);

            y += 15;
            DrawSolidLine(g, pw, y); y += 10;
            g.DrawString("Customer Signature: ____________________", fNormal, Brushes.Black, 30, y);
        }

        // ── Drawing Helpers ───────────────────────────────────────────────────

        private static void DrawDashedLine(Graphics g, float pw, float y)
        {
            using var pen = new Pen(Color.Gray) { DashStyle = System.Drawing.Drawing2D.DashStyle.Dash };
            g.DrawLine(pen, 10, y, pw - 10, y);
        }

        private static void DrawSolidLine(Graphics g, float pw, float y)
        {
            g.DrawLine(Pens.Black, 10, y, pw - 10, y);
        }
    }
}
