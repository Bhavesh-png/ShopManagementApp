using System.Drawing;
using System.Windows.Forms;

namespace ShopManagementApp.Utils
{
    /// <summary>
    /// Fixed light-theme color palette.
    /// Dark/Light toggle removed — app uses a single professional light theme.
    /// Keep Apply() so existing forms compile without modification.
    /// </summary>
    public static class ThemeManager
    {
        // ── Light theme colors (fixed) ─────────────────────────────────────────
        public static readonly Color PageBg      = Color.FromArgb(245, 247, 252);
        public static readonly Color CardBg      = Color.White;
        public static readonly Color InputBg     = Color.White;
        public static readonly Color GroupBg     = Color.FromArgb(248, 249, 253);
        public static readonly Color GridBg      = Color.White;
        public static readonly Color GridAlt     = Color.FromArgb(245, 247, 255);
        public static readonly Color GridLine    = Color.FromArgb(220, 220, 230);
        public static readonly Color TextPrimary = Color.FromArgb(30,  30,  46);
        public static readonly Color TextMuted   = Color.FromArgb(107, 114, 128);
        public static readonly Color InputFg     = Color.FromArgb(30,  30,  46);
        public static readonly Color BorderCol   = Color.FromArgb(210, 210, 225);

        // IsDark always false — kept for binary compatibility with any remaining references
        public static bool IsDark => false;

        // ── Apply: walks the control tree and applies light theme ─────────────
        public static void Apply(Control root)
        {
            root.SuspendLayout();
            if (!IsAccent(root.BackColor)) root.BackColor = PageBg;
            WalkChildren(root);
            root.ResumeLayout(true);
            root.Invalidate(true);
        }

        private static void WalkChildren(Control parent)
        {
            foreach (Control c in parent.Controls) Walk(c);
        }

        private static bool IsAccent(Color c)
        {
            int[] accents =
            {
                Color.FromArgb(99,  102, 241).ToArgb(),
                Color.FromArgb(34,  197, 94 ).ToArgb(),
                Color.FromArgb(249, 115, 22 ).ToArgb(),
                Color.FromArgb(239, 68,  68 ).ToArgb(),
                Color.FromArgb(30,  30,  48 ).ToArgb(),  // sidebar dark
                Color.Transparent.ToArgb()
            };
            return accents.Contains(c.ToArgb()) || c.ToArgb() == Color.Gray.ToArgb();
        }

        private static void Walk(Control ctrl)
        {
            if (ctrl.Tag?.ToString() == "accent") { if (ctrl.HasChildren) WalkChildren(ctrl); return; }

            switch (ctrl)
            {
                case DataGridView dgv:
                    dgv.BackgroundColor = GridBg;
                    dgv.GridColor       = GridLine;
                    dgv.DefaultCellStyle.BackColor          = GridBg;
                    dgv.DefaultCellStyle.ForeColor          = TextPrimary;
                    dgv.DefaultCellStyle.SelectionBackColor = Color.FromArgb(99, 102, 241);
                    dgv.DefaultCellStyle.SelectionForeColor = Color.White;
                    dgv.AlternatingRowsDefaultCellStyle.BackColor = GridAlt;
                    return;

                case GroupBox gb:
                    gb.BackColor = GroupBg; gb.ForeColor = TextPrimary;
                    WalkChildren(gb); return;

                case TextBox tb:
                    tb.BackColor = InputBg; tb.ForeColor = InputFg; return;

                case NumericUpDown n:
                    n.BackColor = InputBg; n.ForeColor = InputFg; return;

                case ComboBox cb:
                    cb.BackColor = InputBg; cb.ForeColor = InputFg; return;

                case Button btn:
                    if (!IsAccent(btn.BackColor))
                    { btn.BackColor = CardBg; btn.ForeColor = TextPrimary; }
                    return;

                case Label lbl:
                    if (!IsAccent(lbl.ForeColor) && lbl.ForeColor != Color.White)
                        lbl.ForeColor = TextPrimary;
                    if (!IsAccent(lbl.BackColor)) lbl.BackColor = Color.Transparent;
                    return;

                case FlowLayoutPanel flp:
                    if (!IsAccent(flp.BackColor)) flp.BackColor = PageBg;
                    WalkChildren(flp); return;

                case Panel p:
                    if (!IsAccent(p.BackColor)) p.BackColor = PageBg;
                    WalkChildren(p); return;
            }
            if (ctrl.HasChildren) WalkChildren(ctrl);
        }
    }
}
