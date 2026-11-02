using System;
using System.Collections.Generic;
using System.Data;
using QuanLyKhachSan.BLL;

namespace QuanLyKhachSan.Tests
{
    /// <summary>
    /// Kiểm thử tự động cho các quy tắc nghiệp vụ thuần (không cần CSDL):
    /// chạy bằng "dotnet run --project QuanLyKhachSan.Tests"; thoát mã 1 nếu có ca lỗi.
    /// </summary>
    internal static class Program
    {
        private static int _dat, _loi;

        private static void Kiem(string ten, bool dieuKien)
        {
            if (dieuKien) _dat++;
            else
            {
                _loi++;
                Console.WriteLine("LỖI: " + ten);
            }
        }

        private static void Bang<T>(string ten, T thucTe, T kyVong)
        {
            Kiem(ten + " (thực tế " + thucTe + ", kỳ vọng " + kyVong + ")", EqualityComparer<T>.Default.Equals(thucTe, kyVong));
        }

        private static int Main()
        {
            SoDem();
            PhuThuTraMuon();
            TongThanhToan();
            Coc();
            GomDoanhThu();
            Csv();

            Console.WriteLine("Đạt " + _dat + ", lỗi " + _loi);
            return _loi == 0 ? 0 : 1;
        }

        private static void SoDem()
        {
            var d = new DateTime(2026, 10, 1, 14, 0, 0);
            Bang("QT40 tối thiểu 1 đêm khi trả cùng ngày", QuyTacTinhTien.TinhSoDem(d, d.AddHours(2)), 1);
            Bang("QT40 2 đêm", QuyTacTinhTien.TinhSoDem(d, d.AddDays(2).AddHours(-3)), 2);
            Bang("QT40 đếm theo ngày lịch", QuyTacTinhTien.TinhSoDem(new DateTime(2026, 10, 1, 23, 0, 0), new DateTime(2026, 10, 2, 8, 0, 0)), 1);
        }

        private static void PhuThuTraMuon()
        {
            var ngay = new DateTime(2026, 10, 3);
            decimal gia = 400000m;
            Bang("Trả trước 12:00 không phụ thu", QuyTacTinhTien.TinhPhuThuTraMuon(ngay.AddHours(11), gia), 0m);
            Bang("Trễ đúng 1 giờ miễn phí", QuyTacTinhTien.TinhPhuThuTraMuon(ngay.AddHours(13), gia), 0m);
            Bang("Trễ 2 giờ: 30%", QuyTacTinhTien.TinhPhuThuTraMuon(ngay.AddHours(14), gia), 120000m);
            Bang("Trễ 4 giờ: 50%", QuyTacTinhTien.TinhPhuThuTraMuon(ngay.AddHours(16), gia), 200000m);
            Bang("Trễ 7 giờ: 100%", QuyTacTinhTien.TinhPhuThuTraMuon(ngay.AddHours(19), gia), 400000m);
        }

        private static void TongThanhToan()
        {
            Bang("QT45 tổng", QuyTacTinhTien.TinhTongThanhToan(800000m, 95000m, 0m, 0m, 200000m), 695000m);
            Bang("QT45 không âm", QuyTacTinhTien.TinhTongThanhToan(100000m, 0m, 0m, 0m, 500000m), 0m);
            Bang("Cơ sở giảm giá", QuyTacTinhTien.CoSoGiamGia(800000m, 95000m, 105000m), 1000000m);
            Bang("Giảm tối đa lễ tân", QuyTacTinhTien.TyLeGiamToiDaLeTan, 0.10m);
        }

        private static void Coc()
        {
            Bang("QT15 cọc tối đa = 1 đêm × số đêm", QuyTacTinhTien.TienCocToiDa(400000m, new DateTime(2026, 10, 1), new DateTime(2026, 10, 4)), 1200000m);
        }

        private static void GomDoanhThu()
        {
            var goc = new DataTable();
            goc.Columns.Add("Ngay", typeof(DateTime));
            goc.Columns.Add("TenLoaiPhong", typeof(string));
            goc.Columns.Add("SoHoaDon", typeof(int));
            goc.Columns.Add("DoanhThu", typeof(decimal));
            goc.Rows.Add(new DateTime(2026, 10, 1), "Tiêu chuẩn", 2, 1000000m);
            goc.Rows.Add(new DateTime(2026, 10, 1), "Cao cấp", 1, 500000m);
            goc.Rows.Add(new DateTime(2026, 10, 3), "Tiêu chuẩn", 1, 300000m);

            DataTable ngay = BaoCaoBLL.GomDoanhThu(goc, new DateTime(2026, 10, 1), new DateTime(2026, 10, 3), KieuGomDoanhThu.Ngay);
            Bang("Gom ngày: đủ 3 ngày kể cả ngày trống", ngay.Rows.Count, 3);
            Bang("Gom ngày: ngày 1", Convert.ToDecimal(ngay.Rows[0]["DoanhThu"]), 1500000m);
            Bang("Gom ngày: ngày trống = 0", Convert.ToDecimal(ngay.Rows[1]["DoanhThu"]), 0m);
            Bang("Tổng doanh thu", BaoCaoBLL.TongDoanhThu(ngay), 1800000m);
            Bang("Tổng số hóa đơn", BaoCaoBLL.TongSoHoaDon(ngay), 4L);

            DataTable loai = BaoCaoBLL.GomDoanhThu(goc, new DateTime(2026, 10, 1), new DateTime(2026, 10, 3), KieuGomDoanhThu.LoaiPhong);
            Bang("Gom loại phòng: 2 loại", loai.Rows.Count, 2);
            Bang("Gom loại phòng: loại cao nhất đứng đầu", Convert.ToString(loai.Rows[0]["Nhan"]), "Tiêu chuẩn");

            DataTable thang = BaoCaoBLL.GomDoanhThu(goc, new DateTime(2026, 10, 1), new DateTime(2026, 11, 15), KieuGomDoanhThu.Thang);
            Bang("Gom tháng: 2 tháng", thang.Rows.Count, 2);

            var cs = new DataTable();
            cs.Columns.Add("TyLe", typeof(decimal));
            cs.Rows.Add(50m);
            cs.Rows.Add(100m);
            Bang("Tỷ lệ trung bình", BaoCaoBLL.TyLeTrungBinh(cs), 75m);
        }

        private static void Csv()
        {
            Bang("CSV: dấu phẩy được bọc nháy", CsvExporter.ThoatO("a,b"), "\"a,b\"");
            Bang("CSV: nháy kép nhân đôi", CsvExporter.ThoatO("a\"b"), "\"a\"\"b\"");
            Kiem("CSV: chặn công thức (=)", !CsvExporter.ThoatO("=1+1").StartsWith("=", StringComparison.Ordinal));
            string nd = CsvExporter.TaoNoiDung(new[] { "A", "B" }, new List<IList<string>> { new[] { "1", "x,y" } });
            Kiem("CSV: có tiêu đề và dòng dữ liệu", nd.Contains("A,B") && nd.Contains("1,\"x,y\""));
        }
    }
}
