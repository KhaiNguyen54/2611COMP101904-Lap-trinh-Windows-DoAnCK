using System;
using System.Collections.Generic;
using System.Data;
using QuanLyKhachSan.BLL;
using QuanLyKhachSan.Web;

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
            Qr();
            DuongDanTraCuu();
            KhoangNgayWeb();
            DichVuTraCuu();
            TrangWeb();

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
    
        private static bool Nem(Action a)
        {
            try { a(); return false; }
            catch (ArgumentException) { return true; }
        }

        private static void Qr()
        {
            bool[,] a = QrCode.Tao("A");
            Bang("QR: 1 byte -> phiên bản 1 (21x21)", a.GetLength(0), 21);
            bool[,] url = QrCode.Tao("http://192.168.1.10:5080/phong/101");
            Bang("QR: URL 34 byte -> phiên bản 3 (29x29)", url.GetLength(0), 29);
            Bang("QR: 213 byte -> phiên bản 10 (57x57)", QrCode.Tao(new string('a', 213)).GetLength(0), 57);

            int n = url.GetLength(0);
            Kiem("QR: dấu định vị góc trên trái", url[0, 0] && url[0, 6] && url[6, 0] && url[6, 6] && url[3, 3] && !url[1, 1] && !url[7, 0] && !url[0, 7]);
            Kiem("QR: dấu định vị góc trên phải và dưới trái", url[0, n - 1] && url[n - 1, 0] && url[3, n - 4] && url[n - 4, 3]);
            bool timing = true;
            for (int i = 8; i < n - 8; i++) timing &= url[6, i] == (i % 2 == 0) && url[i, 6] == (i % 2 == 0);
            Kiem("QR: đường timing xen kẽ đen/trắng", timing);
            Kiem("QR: ô đen cố định (dark module)", url[n - 8, 8]);

            // Hai bản sao thông tin định dạng phải giống nhau và là từ mã BCH(15,5) hợp lệ.
            int b1 = 0, b2 = 0;
            for (int i = 0; i <= 5; i++) b1 |= (url[i, 8] ? 1 : 0) << i;
            b1 |= (url[7, 8] ? 1 : 0) << 6; b1 |= (url[8, 8] ? 1 : 0) << 7; b1 |= (url[8, 7] ? 1 : 0) << 8;
            for (int i = 9; i < 15; i++) b1 |= (url[8, 14 - i] ? 1 : 0) << i;
            for (int i = 0; i < 8; i++) b2 |= (url[8, n - 1 - i] ? 1 : 0) << i;
            for (int i = 8; i < 15; i++) b2 |= (url[n - 15 + i, 8] ? 1 : 0) << i;
            Bang("QR: hai bản sao thông tin định dạng khớp nhau", b1, b2);
            int rem = b1 ^ 0x5412;
            for (int bit = 14; bit >= 10; bit--)
                if (((rem >> bit) & 1) != 0) rem ^= 0x537 << (bit - 10);
            Bang("QR: thông tin định dạng là từ mã BCH hợp lệ", rem, 0);
            Bang("QR: mức sửa lỗi M (2 bit đầu = 00)", ((b1 ^ 0x5412) >> 13) & 3, 0);

            bool[,] lai = QrCode.Tao("http://192.168.1.10:5080/phong/101");
            bool giong = true;
            for (int y = 0; y < n; y++) for (int x = 0; x < n; x++) giong &= lai[y, x] == url[y, x];
            Kiem("QR: cùng nội dung cho cùng ma trận", giong);

            Kiem("QR: nội dung rỗng bị từ chối", Nem(delegate { QrCode.Tao(""); }));
            Kiem("QR: nội dung quá dài bị từ chối", Nem(delegate { QrCode.Tao(new string('a', 214)); }));
            Bang("QR: tiếng Việt UTF-8 tạo được", QrCode.Tao("Phòng 101 – 450.000đ").GetLength(0) >= 21, true);
        }

        private static void DuongDanTraCuu()
        {
            Bang("URL: phòng", TraCuuUrl.Phong("http://may:5080/", "101"), "http://may:5080/phong/101");
            Bang("URL: bảng giá", TraCuuUrl.BangGia("https://khachsan.vn"), "https://khachsan.vn/gia");
            Bang("URL: thoát ký tự đặc biệt trong số phòng", TraCuuUrl.Phong("http://a", "A 1/2"), "http://a/phong/A%201%2F2");
            Kiem("URL: thiếu http bị từ chối", Nem(delegate { TraCuuUrl.ChuanHoaGoc("localhost:5080"); }));
            Kiem("URL: giao thức ftp bị từ chối", Nem(delegate { TraCuuUrl.ChuanHoaGoc("ftp://x"); }));
            Kiem("URL: địa chỉ rỗng bị từ chối", Nem(delegate { TraCuuUrl.ChuanHoaGoc("  "); }));
            Kiem("URL: số phòng rỗng bị từ chối", Nem(delegate { TraCuuUrl.Phong("http://a", " "); }));
        }
        private static void KhoangNgayWeb()
        {
            DateTime den, di;
            var hom = new DateTime(2026, 11, 4, 15, 30, 0);

            Bang("Ngày: mặc định không lỗi", KhoangNgay.Doc(null, null, hom, out den, out di), (string)null);
            Bang("Ngày: mặc định đến hôm nay", den, new DateTime(2026, 11, 4));
            Bang("Ngày: mặc định đi ngày mai", di, new DateTime(2026, 11, 5));

            Bang("Ngày: hợp lệ", KhoangNgay.Doc("2026-11-10", "2026-11-12", hom, out den, out di), (string)null);
            Bang("Ngày: đến hợp lệ", den, new DateTime(2026, 11, 10));
            Bang("Ngày: đi hợp lệ", di, new DateTime(2026, 11, 12));

            Bang("Ngày: chỉ có ngày đến thì đi = đến + 1", KhoangNgay.Doc("2026-11-10", "", hom, out den, out di), (string)null);
            Bang("Ngày: đi = đến + 1", di, new DateTime(2026, 11, 11));

            Bang("Ngày: đến là hôm nay được chấp nhận", KhoangNgay.Doc("2026-11-04", "2026-11-05", hom, out den, out di), (string)null);
            Bang("Ngày: đến sai định dạng", KhoangNgay.Doc("04/11/2026", null, hom, out den, out di), "Ngày đến không hợp lệ.");
            Bang("Ngày: đến là chữ", KhoangNgay.Doc("abc", null, hom, out den, out di), "Ngày đến không hợp lệ.");
            Bang("Ngày: đi sai định dạng", KhoangNgay.Doc("2026-11-10", "2026-13-40", hom, out den, out di), "Ngày đi không hợp lệ.");
            Bang("Ngày: đến trước hôm nay", KhoangNgay.Doc("2026-11-03", "2026-11-05", hom, out den, out di), "Ngày đến không được trước hôm nay.");
            Bang("Ngày: đến bằng đi", KhoangNgay.Doc("2026-11-10", "2026-11-10", hom, out den, out di), "Ngày đến phải trước ngày đi.");
            Bang("Ngày: đến sau đi", KhoangNgay.Doc("2026-11-10", "2026-11-09", hom, out den, out di), "Ngày đến phải trước ngày đi.");
        }

        private static void DichVuTraCuu()
        {
            Bang("Nhãn: trống", TraCuuService.NhanTrangThai("Trống"), "Hiện đang trống");
            Bang("Nhãn: đang ở không nêu khách", TraCuuService.NhanTrangThai("Đang ở"), "Hiện đang có khách");
            Bang("Nhãn: đã đặt", TraCuuService.NhanTrangThai("Đã đặt"), "Đã có lịch đặt sắp tới");
            Bang("Nhãn: dọn dẹp", TraCuuService.NhanTrangThai("Dọn dẹp"), "Đang được dọn");
            Bang("Nhãn: bảo trì", TraCuuService.NhanTrangThai("Bảo trì"), "Đang bảo trì");
            Bang("Nhãn: giá trị lạ giữ nguyên", TraCuuService.NhanTrangThai("Khác"), "Khác");

            // Các nhánh sau trả về trước khi truy cập CSDL nên kiểm thử được mà không cần SQL Server.
            Kiem("Phòng: số phòng rỗng → null", TraCuuService.LayPhong("") == null);
            Kiem("Phòng: chỉ khoảng trắng → null", TraCuuService.LayPhong("   ") == null);
            Kiem("Phòng: null → null", TraCuuService.LayPhong(null) == null);
            Kiem("Phòng: dài quá 10 ký tự → null", TraCuuService.LayPhong("12345678901") == null);

            var tuNgay = new DateTime(2026, 11, 10);
            Kiem("Phòng trống: quá 60 đêm bị từ chối", Nem(delegate { TraCuuService.LayPhongTrong(tuNgay, tuNgay.AddDays(TraCuuService.ToiDaDem + 1)); }));
            Bang("Giới hạn số đêm", TraCuuService.ToiDaDem, 60);
        }

        /// <summary>HtmlEncode có thể đổi ký tự có dấu thành thực thể số; giải mã lại để so sánh nội dung tiếng Việt.</summary>
        private static string Dec(string html) { return System.Net.WebUtility.HtmlDecode(html); }

        private static void TrangWeb()
        {
            Bang("HTML: mã hóa thẻ script", TrangHtml.E("<script>alert(1)</script>"), "&lt;script&gt;alert(1)&lt;/script&gt;");
            Bang("HTML: null thành rỗng", TrangHtml.E(null), "");
            Bang("HTML: giá có dấu chấm ngăn nghìn", TrangHtml.Gia(450000m), "450.000 đ");
            Bang("HTML: giá triệu", TrangHtml.Gia(1250000m), "1.250.000 đ");

            var xau = "<img src=x onerror=alert(1)>";
            var ds = new List<GiaLoaiPhong> { new GiaLoaiPhong { TenLoai = xau, MoTa = xau, GiaMoiDem = 450000m, SucChua = 2 } };
            string bangGia = TrangHtml.BangGia(ds);
            Kiem("HTML: bảng giá không chứa thẻ img thô", bangGia.IndexOf("<img", StringComparison.Ordinal) < 0);
            Kiem("HTML: bảng giá chứa nội dung đã mã hóa", bangGia.Contains("&lt;img"));
            Kiem("HTML: bảng giá rỗng có thông báo", TrangHtml.BangGia(new List<GiaLoaiPhong>()).Contains("Chưa có dữ liệu giá"));

            var phong = new ThongTinPhong { SoPhong = "<b>1</b>", Tang = 1, TenLoai = xau, MoTa = "", GiaMoiDem = 450000m, SucChua = 2, TrangThai = "Đang ở" };
            string ct = TrangHtml.ChiTietPhong(phong, null, null, null);
            Kiem("HTML: chi tiết phòng mã hóa số phòng", ct.IndexOf("<b>1</b>", StringComparison.Ordinal) < 0 && ct.Contains("&lt;b&gt;1"));
            Kiem("HTML: chi tiết phòng chỉ hiện nhãn rút gọn", Dec(ct).Contains("Hiện đang có khách"));

            var den = new DateTime(2026, 11, 10);
            var di = new DateTime(2026, 11, 12);
            var ok = new ThongTinPhong { SoPhong = "101", Tang = 1, TenLoai = "Standard", MoTa = "", GiaMoiDem = 450000m, SucChua = 2, TrangThai = "Trống" };
            Kiem("HTML: còn trống hiện tổng 2 đêm", TrangHtml.ChiTietPhong(ok, den, di, true).Contains("900.000 đ"));
            Kiem("HTML: không còn trống hiện cảnh báo", Dec(TrangHtml.ChiTietPhong(ok, den, di, false)).Contains("không còn trống"));

            var trong = new List<ThongTinPhong> { new ThongTinPhong { SoPhong = "A 1/2", Tang = 1, TenLoai = "Standard", GiaMoiDem = 450000m, TrangThai = "Trống" } };
            string tim = TrangHtml.TimPhong(den, di, trong, null);
            Kiem("HTML: liên kết phòng mã hóa URL", tim.Contains("/phong/A%201%2F2?den=2026-11-10"));
            Kiem("HTML: tổng tiền theo số đêm", tim.Contains("900.000 đ"));
            Kiem("HTML: lỗi hiển thị thay danh sách", Dec(TrangHtml.TimPhong(den, di, new List<ThongTinPhong>(), "Lỗi thử")).Contains("Lỗi thử"));
            Kiem("HTML: trang có ngôn ngữ vi và meta viewport", TrangHtml.Trang("T", "x").Contains("lang=\"vi\"") && TrangHtml.Trang("T", "x").Contains("viewport"));
            Kiem("HTML: tiêu đề trang được mã hóa", TrangHtml.Trang("<x>", "").IndexOf("<title><x>", StringComparison.Ordinal) < 0);
        }
    }
}
