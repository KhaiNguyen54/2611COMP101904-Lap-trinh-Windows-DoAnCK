using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Drawing.Printing;
using System.Globalization;
using System.Windows.Forms;

namespace QuanLyKhachSan.Views
{
    /// <summary>In hóa đơn lưu trú (NV07): xem trước rồi in, có phân trang khi nhiều dòng dịch vụ.</summary>
    internal sealed class HoaDonPrinter
    {
        private readonly DataRow _hoaDon;
        private readonly DataTable _dichVu;
        private int _dongDaIn;

        private HoaDonPrinter(DataRow hoaDon, DataTable dichVu)
        {
            _hoaDon = hoaDon;
            _dichVu = dichVu;
        }

        /// <param name="hoaDon">Một dòng từ HoaDonService.TimKiem.</param>
        /// <param name="dichVu">Các dòng dịch vụ của phiếu thuê (LuuTruService.LayDichVu).</param>
        public static void XemTruoc(IWin32Window chu, DataRow hoaDon, DataTable dichVu)
        {
            var printer = new HoaDonPrinter(hoaDon, dichVu);
            using (var doc = new PrintDocument { DocumentName = "Hóa đơn " + hoaDon["MaHoaDon"] })
            using (var dlg = new PrintPreviewDialog { Document = doc, Width = 900, Height = 720 })
            {
                doc.BeginPrint += delegate { printer._dongDaIn = 0; };
                doc.PrintPage += printer.InTrang;
                dlg.ShowDialog(chu);
            }
        }

        private static string Tien(object v)
        {
            return Convert.ToDecimal(v).ToString("N0", CultureInfo.CurrentCulture) + " đ";
        }

        private void InTrang(object sender, PrintPageEventArgs e)
        {
            Graphics g = e.Graphics;
            Rectangle m = e.MarginBounds;
            int y = m.Top;

            using (var fTieuDe = new Font("Segoe UI", 20, FontStyle.Bold))
            using (var fThuong = new Font("Segoe UI", 11))
            using (var fDam = new Font("Segoe UI", 11, FontStyle.Bold))
            using (var fTong = new Font("Segoe UI", 14, FontStyle.Bold))
            using (var giua = new StringFormat { Alignment = StringAlignment.Center })
            using (var phai = new StringFormat { Alignment = StringAlignment.Far })
            using (var but = new Pen(Color.Black, 1f))
            {
                bool trangDau = _dongDaIn == 0;
                if (trangDau)
                {
                    g.DrawString("HÓA ĐƠN LƯU TRÚ", fTieuDe, Brushes.Black, new RectangleF(m.Left, y, m.Width, 40), giua);
                    y += 60;
                    y = DongThongTin(g, fThuong, fDam, m, y, "Số hóa đơn:", Convert.ToString(_hoaDon["MaHoaDon"]));
                    y = DongThongTin(g, fThuong, fDam, m, y, "Ngày lập:",
                        Convert.ToDateTime(_hoaDon["NgayLap"]).ToString(UiHelper.DinhDangNgayGio, CultureInfo.InvariantCulture));
                    y = DongThongTin(g, fThuong, fDam, m, y, "Khách hàng:", Convert.ToString(_hoaDon["HoTen"]));
                    y = DongThongTin(g, fThuong, fDam, m, y, "Phòng:", Convert.ToString(_hoaDon["SoPhong"]));
                    y = DongThongTin(g, fThuong, fDam, m, y, "Thanh toán:", Convert.ToString(_hoaDon["HinhThucThanhToan"]));
                    y += 14;
                }

                // Bảng dịch vụ
                float[] tiLe = { 0.46f, 0.12f, 0.20f, 0.22f };
                string[] tieuDeCot = { "Dịch vụ", "SL", "Đơn giá", "Thành tiền" };
                y = VeDongBang(g, fDam, m, y, tiLe, tieuDeCot, true);
                g.DrawLine(but, m.Left, y, m.Right, y);
                y += 4;

                int hangCao = (int)fThuong.GetHeight(g) + 6;
                int danhChoTong = 7 * (hangCao + 2) + 30; // chừa chỗ cho khối tổng cộng ở trang cuối
                while (_dichVu != null && _dongDaIn < _dichVu.Rows.Count)
                {
                    if (y + hangCao > m.Bottom - danhChoTong && _dongDaIn < _dichVu.Rows.Count)
                    {
                        g.DrawString("(còn tiếp ở trang sau)", fThuong, Brushes.Gray, m.Left, y);
                        e.HasMorePages = true;
                        return;
                    }

                    DataRow r = _dichVu.Rows[_dongDaIn++];
                    string[] cells =
                    {
                        Convert.ToString(r["TenDichVu"]),
                        Convert.ToDecimal(r["SoLuong"]).ToString("0.##", CultureInfo.CurrentCulture),
                        Tien(r["DonGia"]),
                        Tien(r["ThanhTien"])
                    };
                    y = VeDongBang(g, fThuong, m, y, tiLe, cells, false);
                }

                if (_dichVu == null || _dichVu.Rows.Count == 0)
                {
                    g.DrawString("(Không phát sinh dịch vụ)", fThuong, Brushes.Gray, m.Left, y);
                    y += hangCao;
                }

                // Khối tổng cộng
                y += 8;
                g.DrawLine(but, m.Left, y, m.Right, y);
                y += 8;
                y = DongTien(g, fThuong, phai, m, y, "Tiền phòng", Tien(_hoaDon["TienPhong"]));
                y = DongTien(g, fThuong, phai, m, y, "Tiền dịch vụ", Tien(_hoaDon["TienDichVu"]));
                y = DongTien(g, fThuong, phai, m, y, "Phụ thu trả muộn", Tien(_hoaDon["PhuThu"]));
                y = DongTien(g, fThuong, phai, m, y, "Giảm giá", "- " + Tien(_hoaDon["GiamGia"]));
                if (_hoaDon.Table.Columns.Contains("NguoiDuyetGiam") && !(_hoaDon["NguoiDuyetGiam"] is DBNull))
                    y = DongTien(g, fThuong, phai, m, y, "Quản lý duyệt giảm giá", Convert.ToString(_hoaDon["NguoiDuyetGiam"]));
                y = DongTien(g, fThuong, phai, m, y, "Tiền cọc đã nhận", "- " + Tien(_hoaDon["TienCoc"]));
                y += 6;
                g.DrawLine(but, m.Left, y, m.Right, y);
                y += 8;
                g.DrawString("TỔNG THANH TOÁN", fTong, Brushes.Black, m.Left, y);
                g.DrawString(Tien(_hoaDon["TongThanhToan"]), fTong, Brushes.Black,
                    new RectangleF(m.Left, y, m.Width, 30), phai);
                e.HasMorePages = false;
            }
        }

        private static int DongThongTin(Graphics g, Font thuong, Font dam, Rectangle m, int y, string nhan, string giaTri)
        {
            g.DrawString(nhan, thuong, Brushes.Black, m.Left, y);
            g.DrawString(giaTri, dam, Brushes.Black, m.Left + 130, y);
            return y + (int)thuong.GetHeight(g) + 6;
        }

        private static int DongTien(Graphics g, Font f, StringFormat phai, Rectangle m, int y, string nhan, string giaTri)
        {
            g.DrawString(nhan, f, Brushes.Black, m.Left, y);
            g.DrawString(giaTri, f, Brushes.Black, new RectangleF(m.Left, y, m.Width, f.GetHeight(g) + 2), phai);
            return y + (int)f.GetHeight(g) + 8;
        }

        private static int VeDongBang(Graphics g, Font f, Rectangle m, int y, float[] tiLe, string[] cells, bool tieuDe)
        {
            float x = m.Left;
            using (var trai = new StringFormat { Trimming = StringTrimming.EllipsisCharacter, FormatFlags = StringFormatFlags.NoWrap })
            using (var phai = new StringFormat { Alignment = StringAlignment.Far, FormatFlags = StringFormatFlags.NoWrap })
            {
                for (int i = 0; i < cells.Length; i++)
                {
                    float rong = m.Width * tiLe[i];
                    var o = new RectangleF(x, y, rong - 4, f.GetHeight(g) + 2);
                    g.DrawString(cells[i], f, Brushes.Black, o, i == 0 ? trai : phai);
                    x += rong;
                }
            }
            return y + (int)f.GetHeight(g) + 6;
        }
    }

    /// <summary>In báo cáo (BC03): tiêu đề, ghi chú, biểu đồ (nếu có) và bảng số liệu, phân trang tự động.</summary>
    internal sealed class BaoCaoPrinter
    {
        private readonly string _tieuDe;
        private readonly string[] _ghiChu;
        private readonly string[] _cot;
        private readonly List<string[]> _dong;
        private readonly bool[] _canPhai;
        private readonly Bitmap _bieuDo;
        private int _dongDaIn;
        private int _soTrang;

        private BaoCaoPrinter(string tieuDe, string[] ghiChu, string[] cot, bool[] canPhai, List<string[]> dong, Bitmap bieuDo)
        {
            _tieuDe = tieuDe;
            _ghiChu = ghiChu;
            _cot = cot;
            _canPhai = canPhai;
            _dong = dong;
            _bieuDo = bieuDo;
        }

        /// <summary>Xem trước và in. <paramref name="bieuDo"/> được giải phóng sau khi đóng cửa sổ xem trước.</summary>
        public static void XemTruoc(IWin32Window chu, string tieuDe, string[] ghiChu, string[] cot, bool[] canPhai,
            List<string[]> dong, Bitmap bieuDo)
        {
            var printer = new BaoCaoPrinter(tieuDe, ghiChu, cot, canPhai, dong, bieuDo);
            try
            {
                using (var doc = new PrintDocument { DocumentName = tieuDe })
                using (var dlg = new PrintPreviewDialog { Document = doc, Width = 950, Height = 740 })
                {
                    doc.BeginPrint += delegate { printer._dongDaIn = 0; printer._soTrang = 0; };
                    doc.PrintPage += printer.InTrang;
                    dlg.ShowDialog(chu);
                }
            }
            finally
            {
                if (bieuDo != null) bieuDo.Dispose();
            }
        }

        private void InTrang(object sender, PrintPageEventArgs e)
        {
            Graphics g = e.Graphics;
            Rectangle m = e.MarginBounds;
            int y = m.Top;
            _soTrang++;

            using (var fTieuDe = new Font("Segoe UI", 16, FontStyle.Bold))
            using (var fThuong = new Font("Segoe UI", 10))
            using (var fDam = new Font("Segoe UI", 10, FontStyle.Bold))
            using (var but = new Pen(Color.Black, 1f))
            {
                g.DrawString(_tieuDe, fTieuDe, Brushes.Black, m.Left, y);
                y += (int)fTieuDe.GetHeight(g) + 6;
                foreach (string dong in _ghiChu)
                {
                    g.DrawString(dong, fThuong, Brushes.Black, m.Left, y);
                    y += (int)fThuong.GetHeight(g) + 2;
                }
                y += 8;

                if (_soTrang == 1 && _bieuDo != null)
                {
                    int rong = m.Width;
                    int cao = (int)((double)_bieuDo.Height / _bieuDo.Width * rong);
                    cao = Math.Min(cao, 330);
                    g.DrawImage(_bieuDo, m.Left, y, rong, cao);
                    y += cao + 12;
                }

                y = VeDong(g, fDam, m, y, _cot);
                g.DrawLine(but, m.Left, y, m.Right, y);
                y += 3;

                int hangCao = (int)fThuong.GetHeight(g) + 4;
                while (_dongDaIn < _dong.Count)
                {
                    if (y + hangCao > m.Bottom - 24)
                    {
                        e.HasMorePages = true;
                        GhiSoTrang(g, fThuong, m);
                        return;
                    }
                    y = VeDong(g, fThuong, m, y, _dong[_dongDaIn++]);
                }
                GhiSoTrang(g, fThuong, m);
                e.HasMorePages = false;
            }
        }

        private int VeDong(Graphics g, Font f, Rectangle m, int y, string[] cells)
        {
            float rong = (float)m.Width / _cot.Length;
            using (var trai = new StringFormat { Trimming = StringTrimming.EllipsisCharacter, FormatFlags = StringFormatFlags.NoWrap })
            using (var phai = new StringFormat { Alignment = StringAlignment.Far, FormatFlags = StringFormatFlags.NoWrap })
            {
                for (int i = 0; i < cells.Length; i++)
                {
                    var o = new RectangleF(m.Left + i * rong, y, rong - 6, f.GetHeight(g) + 2);
                    g.DrawString(cells[i], f, Brushes.Black, o, _canPhai[i] ? phai : trai);
                }
            }
            return y + (int)f.GetHeight(g) + 4;
        }

        private void GhiSoTrang(Graphics g, Font f, Rectangle m)
        {
            using (var phai = new StringFormat { Alignment = StringAlignment.Far })
                g.DrawString("Trang " + _soTrang, f, Brushes.Gray, new RectangleF(m.Left, m.Bottom - 6, m.Width, 20), phai);
        }
    }
}
