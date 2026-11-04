using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using QuanLyKhachSan.DTO;

namespace QuanLyKhachSan.GUI
{
    /// <summary>
    /// Shared visual defaults for the WinForms screens. Layout remains owned by each form.
    /// </summary>
    internal static class GiaoDien
    {
        /// <summary>Bộ màu của một chế độ giao diện (sáng hoặc tối).</summary>
        internal sealed class BoMau
        {
            public Color Nen, Chu, Phu, BeMat, Vien, MauThu, CamLa, TieuDeLuoi, ChonLuoi, XenKe;
            public Color DaDat, DangO, DonDep, BaoTri, Trong;

            /// <summary>Mọi màu theo thứ tự cố định, dùng để đổi màu từ bộ này sang bộ kia.</summary>
            public Color[] Tat()
            {
                return new[] { Nen, Chu, Phu, BeMat, Vien, MauThu, CamLa, TieuDeLuoi, ChonLuoi, XenKe, DaDat, DangO, DonDep, BaoTri, Trong };
            }
        }

        private static readonly BoMau BoSang = new BoMau
        {
            Nen = Color.FromArgb(248, 244, 237), Chu = Color.FromArgb(62, 47, 38), Phu = Color.FromArgb(130, 113, 99),
            BeMat = Color.FromArgb(255, 252, 247), Vien = Color.FromArgb(231, 219, 204),
            MauThu = Color.FromArgb(177, 82, 55), CamLa = Color.FromArgb(202, 133, 57),
            TieuDeLuoi = Color.FromArgb(243, 233, 219), ChonLuoi = Color.FromArgb(247, 225, 203), XenKe = Color.FromArgb(250, 246, 240),
            DaDat = Color.FromArgb(246, 224, 187), DangO = Color.FromArgb(234, 190, 173), DonDep = Color.FromArgb(205, 218, 232),
            BaoTri = Color.FromArgb(210, 197, 184), Trong = Color.FromArgb(214, 226, 200)
        };

        private static readonly BoMau BoToi = new BoMau
        {
            Nen = Color.FromArgb(31, 26, 22), Chu = Color.FromArgb(241, 231, 218), Phu = Color.FromArgb(185, 168, 151),
            BeMat = Color.FromArgb(42, 35, 29), Vien = Color.FromArgb(71, 59, 48),
            MauThu = Color.FromArgb(224, 128, 95), CamLa = Color.FromArgb(226, 160, 86),
            TieuDeLuoi = Color.FromArgb(58, 48, 40), ChonLuoi = Color.FromArgb(96, 62, 46), XenKe = Color.FromArgb(36, 30, 25),
            DaDat = Color.FromArgb(110, 84, 45), DangO = Color.FromArgb(118, 63, 48), DonDep = Color.FromArgb(52, 77, 104),
            BaoTri = Color.FromArgb(80, 72, 64), Trong = Color.FromArgb(58, 84, 52)
        };

        private static BoMau _hien = BoSang;

        static GiaoDien()
        {
            if (DocCheDoDaLuu()) _hien = BoToi;
        }

        /// <summary>true khi đang dùng giao diện tối.</summary>
        public static bool Toi { get { return _hien == BoToi; } }

        public static Color Nen { get { return _hien.Nen; } }
        public static Color Chu { get { return _hien.Chu; } }
        public static Color Phu { get { return _hien.Phu; } }
        public static Color BeMat { get { return _hien.BeMat; } }
        public static Color Vien { get { return _hien.Vien; } }
        public static Color MauThu { get { return _hien.MauThu; } }
        public static Color CamLa { get { return _hien.CamLa; } }

        /// <summary>Màu nền thẻ phòng theo trạng thái (đã chỉnh cho từng chế độ).</summary>
        public static Color MauPhong(string trangThai)
        {
            switch (trangThai)
            {
                case TrangThaiPhong.DaDat: return _hien.DaDat;
                case TrangThaiPhong.DangO: return _hien.DangO;
                case TrangThaiPhong.DonDep: return _hien.DonDep;
                case TrangThaiPhong.BaoTri: return _hien.BaoTri;
                default: return _hien.Trong;
            }
        }

        private static string DuongDanCaiDat()
        {
            return System.IO.Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "QuanLyKhachSan", "giaodien.txt");
        }

        private static bool DocCheDoDaLuu()
        {
            try
            {
                string p = DuongDanCaiDat();
                return System.IO.File.Exists(p) && System.IO.File.ReadAllText(p).Trim() == "toi";
            }
            catch (Exception) { return false; }
        }

        private static void LuuCheDo(bool toi)
        {
            try
            {
                string p = DuongDanCaiDat();
                System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(p));
                System.IO.File.WriteAllText(p, toi ? "toi" : "sang");
            }
            catch (Exception) { /* Không lưu được thì lần sau mở lại dùng mặc định; không làm phiền người dùng. */ }
        }

        /// <summary>Đổi chế độ sáng/tối, lưu lại và áp dụng ngay cho mọi cửa sổ đang mở.</summary>
        public static void DatCheDo(bool toi)
        {
            if (toi == Toi) return;
            BoMau cu = _hien;
            _hien = toi ? BoToi : BoSang;
            LuuCheDo(toi);
            foreach (Form f in new List<Form>(Application.OpenForms.Cast<Form>()))
                TaiMauLai(f, cu);
        }

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

        private static Icon _bieuTuong;
        private static Bitmap _logo;

        /// <summary>Biểu tượng cửa sổ (Resources/logo.ico); null nếu thiếu tệp.</summary>
        public static Icon BieuTuong
        {
            get
            {
                if (_bieuTuong == null)
                {
                    try
                    {
                        string f = System.IO.Path.Combine(AppContext.BaseDirectory, "Resources", "logo.ico");
                        if (System.IO.File.Exists(f)) _bieuTuong = new Icon(f);
                    }
                    catch (Exception) { }
                }
                return _bieuTuong;
            }
        }

        /// <summary>Logo dạng ảnh (Resources/logo.png) để đặt trên giao diện; null nếu thiếu tệp.</summary>
        public static Bitmap Logo
        {
            get
            {
                if (_logo == null)
                {
                    try
                    {
                        string f = System.IO.Path.Combine(AppContext.BaseDirectory, "Resources", "logo.png");
                        if (System.IO.File.Exists(f))
                            using (Image g = Image.FromFile(f)) _logo = new Bitmap(g);
                    }
                    catch (Exception) { }
                }
                return _logo;
            }
        }

        public static void ApDung(Form form)
        {
            if (BieuTuong != null) form.Icon = BieuTuong;
            form.Font = new Font("Microsoft Sans Serif", 8.25F, FontStyle.Regular, GraphicsUnit.Point);
            form.ForeColor = Chu;
            form.BackColor = Nen;
            form.AutoScaleMode = AutoScaleMode.Dpi;
            if (form.FormBorderStyle != FormBorderStyle.FixedDialog)
                form.MinimumSize = new Size(Math.Max(form.MinimumSize.Width, 520), Math.Max(form.MinimumSize.Height, 360));

            foreach (Control control in form.Controls)
                ApDungChoControl(control);

            form.HandleCreated += delegate { TieuDeToi(form); };
            TieuDeToi(form);
        }

        /// <summary>Đổi màu một cửa sổ đang mở từ bộ màu cũ sang bộ màu hiện tại.</summary>
        private static void TaiMauLai(Form form, BoMau cu)
        {
            form.SuspendLayout();
            form.ForeColor = Chu;
            form.BackColor = Doi(form.BackColor, cu);
            foreach (Control c in form.Controls) DoiMauCay(c, cu);
            foreach (Control c in form.Controls) ApDungChoControl(c);
            form.ResumeLayout(true);
            TieuDeToi(form);
            form.Invalidate(true);
        }

        private static Color Doi(Color mau, BoMau cu)
        {
            Color[] a = cu.Tat(), b = _hien.Tat();
            for (int i = 0; i < a.Length; i++)
                if (mau.ToArgb() == a[i].ToArgb()) return b[i];
            return mau;
        }

        private static void DoiMauCay(Control c, BoMau cu)
        {
            c.BackColor = Doi(c.BackColor, cu);
            c.ForeColor = Doi(c.ForeColor, cu);
            var link = c as LinkLabel;
            if (link != null)
            {
                link.LinkColor = Doi(link.LinkColor, cu);
                link.ActiveLinkColor = Doi(link.ActiveLinkColor, cu);
                link.VisitedLinkColor = Doi(link.VisitedLinkColor, cu);
            }
            var nut = c as Button;
            if (nut != null && nut.FlatStyle == FlatStyle.Flat)
                nut.FlatAppearance.BorderColor = Doi(nut.FlatAppearance.BorderColor, cu);
            foreach (Control con in c.Controls) DoiMauCay(con, cu);
            c.Invalidate();
        }

        [DllImport("dwmapi.dll")]
        private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int value, int size);

        /// <summary>Thanh tiêu đề tối trên Windows 10/11; bỏ qua lặng lẽ nếu hệ điều hành không hỗ trợ.</summary>
        private static void TieuDeToi(Form form)
        {
            if (Environment.OSVersion.Platform != PlatformID.Win32NT || !form.IsHandleCreated) return;
            try
            {
                int v = Toi ? 1 : 0;
                if (DwmSetWindowAttribute(form.Handle, 20, ref v, sizeof(int)) != 0)
                    DwmSetWindowAttribute(form.Handle, 19, ref v, sizeof(int));
            }
            catch (Exception) { }
        }

        /// <summary>Tô màu thanh menu/thanh trạng thái/menu chuột phải theo giao diện hiện tại.</summary>
        public static void DungToolStrip(ToolStrip strip)
        {
            if (Toi) strip.Renderer = new ToolStripProfessionalRenderer(new BangMauToi());
            else strip.RenderMode = ToolStripRenderMode.System;
            strip.BackColor = BeMat;
            strip.ForeColor = Chu;
            foreach (ToolStripItem it in strip.Items) TuMauMuc(it);
        }

        private static void TuMauMuc(ToolStripItem it)
        {
            it.ForeColor = Chu;
            var m = it as ToolStripMenuItem;
            if (m != null)
            {
                m.DropDown.BackColor = BeMat;
                m.DropDown.ForeColor = Chu;
                if (Toi) m.DropDown.Renderer = new ToolStripProfessionalRenderer(new BangMauToi());
                else m.DropDown.RenderMode = ToolStripRenderMode.System;
                foreach (ToolStripItem con in m.DropDownItems) TuMauMuc(con);
            }
        }

        /// <summary>Bảng màu menu cho giao diện tối (renderer chuyên nghiệp cho phép tự chọn màu).</summary>
        private sealed class BangMauToi : ProfessionalColorTable
        {
            public BangMauToi() { UseSystemColors = false; }
            public override Color ToolStripDropDownBackground { get { return BoToi.BeMat; } }
            public override Color MenuBorder { get { return BoToi.Vien; } }
            public override Color MenuItemBorder { get { return BoToi.MauThu; } }
            public override Color MenuItemSelected { get { return BoToi.ChonLuoi; } }
            public override Color MenuItemSelectedGradientBegin { get { return BoToi.ChonLuoi; } }
            public override Color MenuItemSelectedGradientEnd { get { return BoToi.ChonLuoi; } }
            public override Color MenuItemPressedGradientBegin { get { return BoToi.TieuDeLuoi; } }
            public override Color MenuItemPressedGradientMiddle { get { return BoToi.TieuDeLuoi; } }
            public override Color MenuItemPressedGradientEnd { get { return BoToi.TieuDeLuoi; } }
            public override Color MenuStripGradientBegin { get { return BoToi.BeMat; } }
            public override Color MenuStripGradientEnd { get { return BoToi.BeMat; } }
            public override Color ImageMarginGradientBegin { get { return BoToi.BeMat; } }
            public override Color ImageMarginGradientMiddle { get { return BoToi.BeMat; } }
            public override Color ImageMarginGradientEnd { get { return BoToi.BeMat; } }
            public override Color SeparatorDark { get { return BoToi.Vien; } }
            public override Color SeparatorLight { get { return BoToi.Vien; } }
            public override Color ToolStripBorder { get { return BoToi.Vien; } }
            public override Color ToolStripGradientBegin { get { return BoToi.BeMat; } }
            public override Color ToolStripGradientMiddle { get { return BoToi.BeMat; } }
            public override Color ToolStripGradientEnd { get { return BoToi.BeMat; } }
            public override Color StatusStripGradientBegin { get { return BoToi.BeMat; } }
            public override Color StatusStripGradientEnd { get { return BoToi.BeMat; } }
            public override Color CheckBackground { get { return BoToi.ChonLuoi; } }
            public override Color CheckSelectedBackground { get { return BoToi.ChonLuoi; } }
            public override Color CheckPressedBackground { get { return BoToi.TieuDeLuoi; } }
            public override Color ButtonSelectedHighlight { get { return BoToi.ChonLuoi; } }
            public override Color ButtonSelectedGradientBegin { get { return BoToi.ChonLuoi; } }
            public override Color ButtonSelectedGradientMiddle { get { return BoToi.ChonLuoi; } }
            public override Color ButtonSelectedGradientEnd { get { return BoToi.ChonLuoi; } }
            public override Color GripDark { get { return BoToi.Phu; } }
            public override Color GripLight { get { return BoToi.Vien; } }
        }

        private static readonly HashSet<TabControl> _tabDaNoi = new HashSet<TabControl>();

        /// <summary>Vẽ nhãn tab theo bảng màu hiện tại (nhãn tab hệ thống không đổi màu được).</summary>
        private static void VeThanhTab(object sender, DrawItemEventArgs e)
        {
            var tabs = (TabControl)sender;
            bool chon = e.Index == tabs.SelectedIndex;
            Rectangle r = tabs.GetTabRect(e.Index);
            using (var nen = new SolidBrush(chon ? BeMat : _hien.TieuDeLuoi))
                e.Graphics.FillRectangle(nen, r);
            TextRenderer.DrawText(e.Graphics, tabs.TabPages[e.Index].Text, tabs.Font, r, chon ? MauThu : Chu,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
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
                grid.ColumnHeadersDefaultCellStyle.BackColor = _hien.TieuDeLuoi;
                grid.ColumnHeadersDefaultCellStyle.ForeColor = Chu;
                grid.ColumnHeadersDefaultCellStyle.Font = new Font("Microsoft Sans Serif", 8.25F, FontStyle.Bold);
                grid.ColumnHeadersDefaultCellStyle.SelectionBackColor = grid.ColumnHeadersDefaultCellStyle.BackColor;
                grid.DefaultCellStyle.BackColor = BeMat;
                grid.DefaultCellStyle.ForeColor = Chu;
                grid.DefaultCellStyle.SelectionBackColor = _hien.ChonLuoi;
                grid.DefaultCellStyle.SelectionForeColor = Chu;
                grid.AlternatingRowsDefaultCellStyle.BackColor = _hien.XenKe;
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
                var ngay = control as DateTimePicker;
                if (ngay != null)
                {
                    // Ô nhập ngày của Windows luôn vẽ nền sáng hệ thống: giữ chữ tối để không bị mờ ở chế độ tối.
                    ngay.BackColor = SystemColors.Window;
                    ngay.ForeColor = SystemColors.WindowText;
                    ngay.CalendarMonthBackground = BeMat;
                    ngay.CalendarForeColor = Chu;
                    ngay.CalendarTitleBackColor = _hien.TieuDeLuoi;
                    ngay.CalendarTitleForeColor = Chu;
                    ngay.CalendarTrailingForeColor = Phu;
                }
            }
            else if (control is FlowLayoutPanel || control is TableLayoutPanel)
            {
                control.ForeColor = Chu;
                if (control is TableLayoutPanel || control.Dock == DockStyle.Top)
                    control.BackColor = BeMat;
            }
            else if (control is TabControl tabs)
            {
                tabs.DrawMode = TabDrawMode.OwnerDrawFixed;
                if (_tabDaNoi.Add(tabs)) tabs.DrawItem += VeThanhTab;
                tabs.Invalidate();
            }
            else if (control is ToolStrip strip)
            {
                DungToolStrip(strip);
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
