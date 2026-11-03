using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using QuanLyKhachSan.BLL;

namespace QuanLyKhachSan.GUI
{
    /// <summary>Vẽ ma trận QR thành ảnh (nền trắng, vùng lặng 4 ô theo chuẩn) và thẻ QR có chú thích để lưu/in.</summary>
    internal static class QrHinh
    {
        public static Bitmap Ve(bool[,] ma, int pxMoiO)
        {
            int n = ma.GetLength(0);
            const int vien = 4;
            int canh = (n + 2 * vien) * pxMoiO;
            var anh = new Bitmap(canh, canh);
            using (Graphics g = Graphics.FromImage(anh))
            {
                g.Clear(Color.White);
                g.SmoothingMode = SmoothingMode.None;
                g.PixelOffsetMode = PixelOffsetMode.Half;
                using (var den = new SolidBrush(Color.Black))
                    for (int y = 0; y < n; y++)
                        for (int x = 0; x < n; x++)
                            if (ma[y, x]) g.FillRectangle(den, (x + vien) * pxMoiO, (y + vien) * pxMoiO, pxMoiO, pxMoiO);
            }
            return anh;
        }

        /// <summary>Thẻ gồm tiêu đề, mã QR, dòng phụ và đường dẫn — dùng để lưu PNG hoặc in dán lên cửa phòng.</summary>
        public static Bitmap TaoThe(string url, string tieuDe, string phuDe)
        {
            using (Bitmap qr = Ve(QrCode.Tao(url), 10))
            using (var fTieuDe = new Font("Microsoft Sans Serif", 22F, FontStyle.Bold))
            using (var fPhu = new Font("Microsoft Sans Serif", 14F))
            using (var fUrl = new Font("Consolas", 9F))
            {
                int rong = Math.Max(qr.Width + 80, 520);
                int cao = 24 + 44 + 8 + qr.Height + 8 + 30 + 24 + 24;
                var the = new Bitmap(rong, cao);
                using (Graphics g = Graphics.FromImage(the))
                {
                    g.Clear(Color.White);
                    g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;
                    var giua = new StringFormat { Alignment = StringAlignment.Center, Trimming = StringTrimming.EllipsisCharacter };
                    int y = 24;
                    g.DrawString(tieuDe, fTieuDe, Brushes.Black, new RectangleF(10, y, rong - 20, 44), giua);
                    y += 44 + 8;
                    g.DrawImage(qr, (rong - qr.Width) / 2, y, qr.Width, qr.Height);
                    y += qr.Height + 8;
                    g.DrawString(phuDe, fPhu, Brushes.Black, new RectangleF(10, y, rong - 20, 30), giua);
                    y += 30 + 24;
                    g.DrawString(url, fUrl, Brushes.DimGray, new RectangleF(10, y, rong - 20, 24), giua);
                    using (var vien = new Pen(Color.Black, 2F))
                        g.DrawRectangle(vien, 1, 1, rong - 3, cao - 3);
                }
                return the;
            }
        }
    }
}
