using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.Windows.Forms;

namespace QuanLyKhachSan.GUI
{
    public enum KieuBieuDo
    {
        Cot,
        Duong
    }

    /// <summary>
    /// Biểu đồ cột/đường tự vẽ bằng GDI+ (BC02): có trục, lưới, nhãn giá trị và gợi ý khi rê chuột.
    /// Dùng được cả để vẽ lên màn hình lẫn lên bitmap khi in.
    /// </summary>
    public sealed class ChartPanel : Control
    {
        private static readonly Color MauChinh = Color.FromArgb(177, 82, 55);
        private static readonly Color MauLuoi = Color.FromArgb(231, 219, 204);

        private readonly ToolTip _goiY = new ToolTip();
        private readonly List<string> _nhan = new List<string>();
        private readonly List<double> _giaTri = new List<double>();
        private int _chiSoGoiY = -1;

        public ChartPanel()
        {
            DoubleBuffered = true;
            ResizeRedraw = true;
            BackColor = Color.White;
            Height = 260;
        }

        [Browsable(true)]
        [DefaultValue("")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public string TieuDe { get; set; } = string.Empty;

        [Browsable(true)]
        [DefaultValue(KieuBieuDo.Cot)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public KieuBieuDo Kieu { get; set; }

        /// <summary>Hậu tố giá trị trong gợi ý và trục, ví dụ "đ" hoặc "%".</summary>
        [Browsable(true)]
        [DefaultValue("")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public string DonVi { get; set; } = string.Empty;

        /// <summary>Giá trị trục Y cố định ở mức tối đa (ví dụ 100 cho phần trăm); null = tự chọn theo dữ liệu.</summary>
        [Browsable(true)]
        [DefaultValue(null)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public double? TrucYToiDa { get; set; }

        public void DatDuLieu(IList<string> nhan, IList<double> giaTri)
        {
            if (nhan.Count != giaTri.Count)
                throw new ArgumentException("Số nhãn và số giá trị phải bằng nhau.");
            _nhan.Clear();
            _giaTri.Clear();
            _nhan.AddRange(nhan);
            _giaTri.AddRange(giaTri);
            _chiSoGoiY = -1;
            Invalidate();
        }

        public void XoaDuLieu()
        {
            _nhan.Clear();
            _giaTri.Clear();
            Invalidate();
        }

        /// <summary>Vẽ biểu đồ ra bitmap kích thước tùy ý (dùng khi in báo cáo).</summary>
        public Bitmap VeRaBitmap(int rong, int cao)
        {
            var bmp = new Bitmap(rong, cao);
            using (Graphics g = Graphics.FromImage(bmp))
                Ve(g, new Rectangle(0, 0, rong, cao), Font);
            return bmp;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Ve(e.Graphics, ClientRectangle, Font);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            int i = ChiSoTaiDiem(e.X);
            if (i == _chiSoGoiY) return;
            _chiSoGoiY = i;
            if (i < 0) _goiY.Hide(this);
            else _goiY.Show(_nhan[i] + ": " + DinhDangGiaTri(_giaTri[i]), this, e.X + 12, e.Y + 16, 4000);
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            _chiSoGoiY = -1;
            _goiY.Hide(this);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) _goiY.Dispose();
            base.Dispose(disposing);
        }

        // ---- vẽ ----

        private Rectangle VungVe(Rectangle r)
        {
            int trai = 64, phai = 16, tren = string.IsNullOrEmpty(TieuDe) ? 14 : 36, duoi = 44;
            return new Rectangle(r.Left + trai, r.Top + tren, Math.Max(10, r.Width - trai - phai),
                Math.Max(10, r.Height - tren - duoi));
        }

        private int ChiSoTaiDiem(int x)
        {
            if (_giaTri.Count == 0) return -1;
            Rectangle v = VungVe(ClientRectangle);
            if (x < v.Left || x > v.Right) return -1;
            double rongOCot = (double)v.Width / _giaTri.Count;
            int i = (int)((x - v.Left) / rongOCot);
            return i >= 0 && i < _giaTri.Count ? i : -1;
        }

        private void Ve(Graphics g, Rectangle r, Font font)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
            using (var nen = new SolidBrush(Color.White))
                g.FillRectangle(nen, r);

            using (var chu = new SolidBrush(Color.FromArgb(60, 66, 78)))
            using (var chuNhat = new SolidBrush(Color.FromArgb(110, 118, 130)))
            using (var fontTieuDe = new Font(font, FontStyle.Bold))
            {
                if (!string.IsNullOrEmpty(TieuDe))
                    g.DrawString(TieuDe, fontTieuDe, chu, r.Left + 8, r.Top + 8);

                Rectangle v = VungVe(r);
                if (_giaTri.Count == 0)
                {
                    VeChuGiua(g, "Chưa có dữ liệu trong khoảng đã chọn.", font, chuNhat, v);
                    return;
                }

                double max = TrucYToiDa ?? ChonMax(MaxGiaTri());
                VeLuoiVaTrucY(g, v, max, font, chuNhat);
                if (Kieu == KieuBieuDo.Cot) VeCot(g, v, max, font, chu);
                else VeDuong(g, v, max);
                VeNhanTrucX(g, v, font, chuNhat);
            }
        }

        private void VeLuoiVaTrucY(Graphics g, Rectangle v, double max, Font font, Brush chu)
        {
            const int soBac = 5;
            using (var butLuoi = new Pen(MauLuoi))
            using (var dinhDang = new StringFormat { Alignment = StringAlignment.Far, LineAlignment = StringAlignment.Center })
            {
                for (int i = 0; i <= soBac; i++)
                {
                    float y = v.Bottom - (float)v.Height * i / soBac;
                    g.DrawLine(butLuoi, v.Left, y, v.Right, y);
                    g.DrawString(RutGon(max * i / soBac), font, chu, new RectangleF(0, y - 10, v.Left - 6, 20), dinhDang);
                }
            }
        }

        private void VeCot(Graphics g, Rectangle v, double max, Font font, Brush chu)
        {
            int n = _giaTri.Count;
            float o = (float)v.Width / n;
            float rongCot = Math.Max(2f, o * 0.68f);
            bool hienGiaTri = o >= 44f;

            using (var to = new SolidBrush(MauChinh))
            using (var dinhDang = new StringFormat { Alignment = StringAlignment.Center })
            {
                for (int i = 0; i < n; i++)
                {
                    float cao = (float)(Math.Min(_giaTri[i], max) / max * v.Height);
                    float x = v.Left + i * o + (o - rongCot) / 2f;
                    if (cao > 0f) g.FillRectangle(to, x, v.Bottom - cao, rongCot, cao);
                    if (hienGiaTri && _giaTri[i] > 0)
                        g.DrawString(RutGon(_giaTri[i]), font, chu,
                            new RectangleF(x - 10, v.Bottom - cao - 18, rongCot + 20, 16), dinhDang);
                }
            }
        }

        private void VeDuong(Graphics g, Rectangle v, double max)
        {
            int n = _giaTri.Count;
            float o = (float)v.Width / n;
            var diem = new PointF[n];
            for (int i = 0; i < n; i++)
            {
                float y = v.Bottom - (float)(Math.Min(_giaTri[i], max) / max * v.Height);
                diem[i] = new PointF(v.Left + o * (i + 0.5f), y);
            }

            if (n >= 2)
            {
                var vung = new PointF[n + 2];
                Array.Copy(diem, 0, vung, 1, n);
                vung[0] = new PointF(diem[0].X, v.Bottom);
                vung[n + 1] = new PointF(diem[n - 1].X, v.Bottom);
                using (var phu = new SolidBrush(Color.FromArgb(36, MauChinh)))
                    g.FillPolygon(phu, vung);
                using (var but = new Pen(MauChinh, 2f) { LineJoin = LineJoin.Round })
                    g.DrawLines(but, diem);
            }

            if (n <= 60)
            {
                using (var cham = new SolidBrush(MauChinh))
                    foreach (PointF p in diem)
                        g.FillEllipse(cham, p.X - 3f, p.Y - 3f, 6f, 6f);
            }
        }

        private void VeNhanTrucX(Graphics g, Rectangle v, Font font, Brush chu)
        {
            int n = _nhan.Count;
            float o = (float)v.Width / n;
            // Bỏ bớt nhãn khi quá dày để không chồng lên nhau.
            int buoc = Math.Max(1, (int)Math.Ceiling(60f / Math.Max(1f, o)));
            using (var dinhDang = new StringFormat { Alignment = StringAlignment.Center, Trimming = StringTrimming.EllipsisCharacter })
            {
                for (int i = 0; i < n; i += buoc)
                {
                    float rong = Math.Max(o * buoc, 40f);
                    float tam = v.Left + o * (i + 0.5f);
                    g.DrawString(_nhan[i], font, chu, new RectangleF(tam - rong / 2f, v.Bottom + 6, rong, 32), dinhDang);
                }
            }
        }

        private static void VeChuGiua(Graphics g, string chu, Font font, Brush but, Rectangle v)
        {
            using (var dinhDang = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
                g.DrawString(chu, font, but, v, dinhDang);
        }

        // ---- tính toán ----

        private double MaxGiaTri()
        {
            double max = 0;
            foreach (double v in _giaTri)
                if (v > max) max = v;
            return max;
        }

        /// <summary>Làm tròn lên mốc "đẹp" (1, 2, 5 × 10^k) để các vạch trục đều và dễ đọc.</summary>
        public static double ChonMax(double max)
        {
            if (max <= 0) return 1;
            double luyThua = Math.Pow(10, Math.Floor(Math.Log10(max)));
            double chuan = max / luyThua;
            double dep = chuan <= 1 ? 1 : chuan <= 2 ? 2 : chuan <= 5 ? 5 : 10;
            return dep * luyThua;
        }

        private string DinhDangGiaTri(double v)
        {
            string so = DonVi == "%" ? v.ToString("0.##", CultureInfo.CurrentCulture) : v.ToString("N0", CultureInfo.CurrentCulture);
            return string.IsNullOrEmpty(DonVi) ? so : (DonVi == "%" ? so + "%" : so + " " + DonVi);
        }

        /// <summary>Rút gọn số lớn cho trục: 1,5 tỷ / 2,4 tr / 350 k / 12.</summary>
        public static string RutGon(double v)
        {
            double a = Math.Abs(v);
            CultureInfo ci = CultureInfo.CurrentCulture;
            if (a >= 1e9) return (v / 1e9).ToString("0.##", ci) + " tỷ";
            if (a >= 1e6) return (v / 1e6).ToString("0.##", ci) + " tr";
            if (a >= 1e4) return (v / 1e3).ToString("0.#", ci) + " k";
            return v.ToString("0.##", ci);
        }
    }
}
