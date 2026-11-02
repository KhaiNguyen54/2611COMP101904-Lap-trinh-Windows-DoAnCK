using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace QuanLyKhachSan.GUI
{
    /// <summary>
    /// Shared visual defaults for the WinForms screens. Layout remains owned by each form.
    /// </summary>
    internal static class GiaoDien
    {
        public static readonly Color Nen = Color.FromArgb(248, 244, 237);
        public static readonly Color Chu = Color.FromArgb(62, 47, 38);
        public static readonly Color Phu = Color.FromArgb(130, 113, 99);
        public static readonly Color BeMat = Color.FromArgb(255, 252, 247);
        public static readonly Color Vien = Color.FromArgb(231, 219, 204);
        public static readonly Color MauThu = Color.FromArgb(177, 82, 55);
        public static readonly Color CamLa = Color.FromArgb(202, 133, 57);

        /// <summary>Tag đánh dấu nút hành động chính (tô màu nhấn).</summary>
        public const string NutChinhTag = "chinh";

        /// <summary>Tag đánh dấu nút tự quản lý màu (ví dụ thẻ phòng).</summary>
        public const string KhongTo = "khongto";

        /// <summary>Nút tự co theo chữ, có kích thước tối thiểu; không bao giờ cắt chữ.</summary>
        public static Button TaoNut(string chu, EventHandler click = null, bool chinh = false)
        {
            var nut = new Button
            {
                Text = chu,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                MinimumSize = new Size(88, 28),
                Padding = new Padding(8, 2, 8, 2),
                Margin = new Padding(0, 0, 8, 0),
                Tag = chinh ? NutChinhTag : null
            };
            if (click != null) nut.Click += click;
            return nut;
        }

        /// <summary>Nhóm "nhãn phía trên + ô nhập" tự co theo nội dung, dùng trong thanh lọc dạng dòng.</summary>
        public static Control Nhom(string nhan, Control o, int rong = 150)
        {
            var nhom = new TableLayoutPanel
            {
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                ColumnCount = 1,
                RowCount = 2,
                Margin = new Padding(0, 0, 14, 6),
                Padding = new Padding(0)
            };
            nhom.Controls.Add(new Label { Text = nhan, AutoSize = true, Margin = new Padding(0, 0, 0, 2) }, 0, 0);
            o.Margin = new Padding(0);
            if (!(o is Button)) o.Width = rong;
            nhom.Controls.Add(o, 0, 1);
            return nhom;
        }

        /// <summary>Thanh ngang tự xuống dòng khi hẹp, chiều cao theo nội dung.</summary>
        public static FlowLayoutPanel ThanhNgang(DockStyle dock = DockStyle.Top)
        {
            return new FlowLayoutPanel
            {
                Dock = dock,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                WrapContents = true,
                Padding = new Padding(10, 8, 10, 4),
                Margin = new Padding(0)
            };
        }

        /// <summary>Hàng nút sát phải, không xuống dòng.</summary>
        public static FlowLayoutPanel HangNut(FlowDirection huong = FlowDirection.LeftToRight)
        {
            return new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                WrapContents = true,
                FlowDirection = huong,
                Padding = new Padding(10, 4, 10, 8),
                Margin = new Padding(0)
            };
        }

        /// <summary>Bảng 2 cột nhãn - ô nhập, hàng tự co theo nội dung.</summary>
        public static TableLayoutPanel BangNhap(params KeyValuePair<string, Control>[] dong)
        {
            var bang = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                ColumnCount = 2,
                Padding = new Padding(10, 6, 10, 6)
            };
            bang.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            bang.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            foreach (var d in dong)
            {
                int r = bang.RowCount++;
                bang.RowStyles.Add(new RowStyle(SizeType.AutoSize));
                bang.Controls.Add(new Label { Text = d.Key, AutoSize = true, Anchor = AnchorStyles.Left, Margin = new Padding(0, 6, 12, 6) }, 0, r);
                d.Value.Anchor = AnchorStyles.Left | AnchorStyles.Right;
                d.Value.Margin = new Padding(0, 4, 0, 4);
                bang.Controls.Add(d.Value, 1, r);
            }
            return bang;
        }

        /// <summary>Khung gốc 1 cột cho hộp thoại tự co: mỗi hàng cao theo nội dung.</summary>
        public static TableLayoutPanel GocTuCo()
        {
            var goc = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                ColumnCount = 1,
                Padding = new Padding(4)
            };
            goc.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            return goc;
        }

        public static Label NhanDam(string chu, float co = 9F)
        {
            return new Label { Text = chu, AutoSize = true, Font = new Font("Microsoft Sans Serif", co, FontStyle.Bold), ForeColor = Chu };
        }

        public static void ApDung(Form form)
        {
            form.Font = new Font("Microsoft Sans Serif", 8.25F, FontStyle.Regular, GraphicsUnit.Point);
            form.ForeColor = Chu;
            form.BackColor = Nen;
            form.AutoScaleMode = AutoScaleMode.Dpi;
            if (form.FormBorderStyle != FormBorderStyle.FixedDialog)
                form.MinimumSize = new Size(Math.Max(form.MinimumSize.Width, 520), Math.Max(form.MinimumSize.Height, 360));

            foreach (Control control in form.Controls)
                ApDungChoControl(control);
        }

        private static void ApDungChoControl(Control control)
        {
            // Room status cards use their own status colors and typography.
            if (control is UcTrangThaiPhong) return;

            if (control is DataGridView grid)
            {
                grid.BackgroundColor = BeMat;
                grid.BorderStyle = BorderStyle.Fixed3D;
                grid.CellBorderStyle = DataGridViewCellBorderStyle.Single;
                grid.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single;
                grid.EnableHeadersVisualStyles = false;
                grid.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(243, 233, 219);
                grid.ColumnHeadersDefaultCellStyle.ForeColor = Chu;
                grid.ColumnHeadersDefaultCellStyle.Font = new Font("Microsoft Sans Serif", 8.25F, FontStyle.Bold);
                grid.ColumnHeadersDefaultCellStyle.SelectionBackColor = grid.ColumnHeadersDefaultCellStyle.BackColor;
                grid.DefaultCellStyle.BackColor = BeMat;
                grid.DefaultCellStyle.ForeColor = Chu;
                grid.DefaultCellStyle.SelectionBackColor = Color.FromArgb(247, 225, 203);
                grid.DefaultCellStyle.SelectionForeColor = Chu;
                grid.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(250, 246, 240);
                grid.RowHeadersVisible = false;
                grid.ColumnHeadersHeight = 26;
                grid.RowTemplate.Height = 24;
                grid.GridColor = Vien;
            }
            else if (control is Button button && !(button.Tag is string tag && tag == KhongTo))
            {
                button.FlatStyle = FlatStyle.Standard;
                button.UseVisualStyleBackColor = false;
                button.Cursor = Cursors.Default;
                bool chinh = button.Tag is string t && t == NutChinhTag;
                button.BackColor = chinh ? MauThu : BeMat;
                button.ForeColor = chinh ? Color.White : Chu;
            }
            else if (control is TextBox || control is ComboBox || control is DateTimePicker || control is NumericUpDown)
            {
                control.BackColor = BeMat;
                control.ForeColor = Chu;
            }
            else if (control is FlowLayoutPanel || control is TableLayoutPanel)
            {
                control.ForeColor = Chu;
                if (control is TableLayoutPanel || control.Dock == DockStyle.Top)
                    control.BackColor = BeMat;
            }
            else if (control is ToolStrip strip)
            {
                strip.BackColor = BeMat;
                strip.ForeColor = Chu;
                strip.RenderMode = ToolStripRenderMode.System;
                strip.GripStyle = ToolStripGripStyle.Visible;
                strip.Padding = new Padding(4, 2, 4, 2);
            }

            foreach (Control child in control.Controls)
                ApDungChoControl(child);
        }
    }

    /// <summary>
    /// Illustration slot for a hotel image. Set BackgroundImage when hotel artwork is available.
    /// </summary>
    internal sealed class KhungAnhKhachSan : Panel
    {
        public KhungAnhKhachSan()
        {
            BackColor = Color.FromArgb(246, 231, 213);
            BackgroundImageLayout = ImageLayout.Zoom;
            DoubleBuffered = true;
            MinimumSize = new Size(220, 150);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            if (BackgroundImage != null) return;

            Graphics g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            using (var sky = new System.Drawing.Drawing2D.LinearGradientBrush(ClientRectangle,
                Color.FromArgb(231, 214, 199), Color.FromArgb(250, 237, 215), 90F))
                g.FillRectangle(sky, ClientRectangle);

            int width = ClientSize.Width;
            int height = ClientSize.Height;
            using (var sun = new SolidBrush(Color.FromArgb(235, 174, 105)))
                g.FillEllipse(sun, width - 92, 22, 46, 46);
            using (var hill = new SolidBrush(Color.FromArgb(192, 142, 91)))
                g.FillEllipse(hill, -width / 5, height / 2, width * 4 / 5, height);
            using (var hill = new SolidBrush(Color.FromArgb(157, 91, 68)))
                g.FillEllipse(hill, width / 3, height / 2 - 8, width, height);

            int buildingWidth = Math.Min(112, width / 3);
            int buildingHeight = Math.Min(128, height * 2 / 3);
            int x = (width - buildingWidth) / 2;
            int y = height - buildingHeight - 12;
            using (var building = new SolidBrush(Color.FromArgb(246, 242, 229)))
                g.FillRectangle(building, x, y, buildingWidth, buildingHeight);
            using (var roof = new SolidBrush(Color.FromArgb(101, 67, 54)))
            {
                Point[] points = { new Point(x - 8, y + 10), new Point(x + buildingWidth / 2, y - 18), new Point(x + buildingWidth + 8, y + 10) };
                g.FillPolygon(roof, points);
            }
            using (var window = new SolidBrush(Color.FromArgb(212, 155, 96)))
            {
                int windowSize = Math.Max(8, buildingWidth / 8);
                for (int row = 0; row < 3; row++)
                    for (int col = 0; col < 3; col++)
                        g.FillRectangle(window, x + 14 + col * (windowSize + 10), y + 20 + row * (windowSize + 11), windowSize, windowSize);
            }
            using (var door = new SolidBrush(Color.FromArgb(126, 76, 55)))
                g.FillRectangle(door, x + buildingWidth / 2 - 9, y + buildingHeight - 28, 18, 28);

            var caption = new Rectangle(0, height - 42, width, 42);
            using (var shade = new SolidBrush(Color.FromArgb(175, 62, 47, 38)))
                g.FillRectangle(shade, caption);
            using (var titleFont = new Font("Segoe UI", 9F, FontStyle.Bold))
            using (var detailFont = new Font("Segoe UI", 8F))
            using (var textBrush = new SolidBrush(Color.White))
            {
                g.DrawString("HÌNH ẢNH KHÁCH SẠN", titleFont, textBrush, 12, height - 37);
                g.DrawString("hotel.png", detailFont, textBrush, 12, height - 19);
            }
        }
    }
}
