using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using QuanLyKhachSan.Models;
using QuanLyKhachSan.Presenters;
using QuanLyKhachSan.Services;
using QuanLyKhachSan.Views;

namespace QuanLyKhachSan.Tests
{
    /// <summary>
    /// Kiểm thử các Presenter bằng View và Service giả: không mở giao diện, không cần CSDL.
    /// </summary>
    internal static partial class Program
    {
        private sealed class ThongBao
        {
            public string TieuDe, NoiDung;
            public MucThongBao Muc;
        }

        private abstract class ViewGia : IThongBaoView
        {
            public readonly List<ThongBao> ThongBaos = new List<ThongBao>();
            public bool TraLoiXacNhan = true;
            public int SoLanXacNhan;

            public void HienThongBao(string tieuDe, string noiDung, MucThongBao muc)
            {
                ThongBaos.Add(new ThongBao { TieuDe = tieuDe, NoiDung = noiDung, Muc = muc });
            }

            public bool XacNhan(string noiDung)
            {
                SoLanXacNhan++;
                return TraLoiXacNhan;
            }

            public ThongBao CuoiCung { get { return ThongBaos.Count == 0 ? null : ThongBaos[ThongBaos.Count - 1]; } }
        }

        // Định dạng số theo văn hóa hiện hành để kiểm thử không phụ thuộc máy chạy.
        private static string N(decimal v) { return v.ToString("N0"); }

        private static NguoiDung NguoiLeTan() { return new NguoiDung { MaNguoiDung = 2, VaiTro = VaiTroNguoiDung.LeTan, HoatDong = true }; }
        private static NguoiDung NguoiQuanLy() { return new NguoiDung { MaNguoiDung = 1, VaiTro = VaiTroNguoiDung.QuanLy, HoatDong = true }; }

        private static void PresenterTests()
        {
            DangNhapPresenterTests();
            DatPhongPresenterTests();
            NhanPhongPresenterTests();
            GhiDichVuPresenterTests();
            TraPhongPresenterTests();
            DoiPhongPresenterTests();
            DuyetGiamGiaPresenterTests();
            BaoCaoPresenterTests();
        }

        // ---------- Đăng nhập ----------
        private sealed class DangNhapViewGia : ViewGia, IDangNhapView
        {
            public string TenDangNhap { get; set; }
            public string MatKhau { get; set; }
            public readonly List<bool> TrangThaiXuLy = new List<bool>();
            public int SoLanXoaMatKhau;
            public NguoiDung KetQua;
            public void DatDangXuLy(bool dangXuLy) { TrangThaiXuLy.Add(dangXuLy); }
            public void XoaMatKhau() { SoLanXoaMatKhau++; }
            public void DongThanhCong(NguoiDung nguoiDung) { KetQua = nguoiDung; }
        }

        private sealed class NguoiDungGia : INguoiDungService
        {
            public NguoiDung TraVe;
            public Exception Loi;
            public int SoLanGoi;
            public Task<NguoiDung> DangNhapAsync(string tenDangNhap, string matKhau, CancellationToken cancellationToken = default(CancellationToken))
            {
                SoLanGoi++;
                if (Loi != null) throw Loi;
                return Task.FromResult(TraVe);
            }
        }

        private static void DangNhapPresenterTests()
        {
            var view = new DangNhapViewGia { TenDangNhap = " ", MatKhau = "" };
            var sv = new NguoiDungGia();
            var p = new DangNhapPresenter(view, sv);
            p.DangNhapAsync(CancellationToken.None).Wait();
            Kiem("MVP-DN01 thiếu dữ liệu: cảnh báo và không gọi Service", view.CuoiCung != null && view.CuoiCung.Muc == MucThongBao.CanhBao && sv.SoLanGoi == 0);

            view = new DangNhapViewGia { TenDangNhap = "admin", MatKhau = "sai" };
            sv = new NguoiDungGia { TraVe = null };
            p = new DangNhapPresenter(view, sv);
            p.DangNhapAsync(CancellationToken.None).Wait();
            Kiem("MVP-DN02 sai mật khẩu: báo lỗi, xóa mật khẩu, trả lại trạng thái nút", view.SoLanXoaMatKhau == 1 && view.KetQua == null
                && view.TrangThaiXuLy.Count == 2 && view.TrangThaiXuLy[0] && !view.TrangThaiXuLy[1]);

            var gio = new DateTime(2026, 11, 1, 8, 0, 0);
            view = new DangNhapViewGia { TenDangNhap = "admin", MatKhau = "sai" };
            sv = new NguoiDungGia();
            p = new DangNhapPresenter(view, sv, () => gio);
            for (int i = 0; i < DangNhapPresenter.SoLanSaiToiDa; i++) p.DangNhapAsync(CancellationToken.None).Wait();
            int truoc = sv.SoLanGoi;
            p.DangNhapAsync(CancellationToken.None).Wait();
            Kiem("MVP-DN03 sai 5 lần thì tạm khóa, không gọi Service nữa", truoc == 5 && sv.SoLanGoi == 5 && view.CuoiCung.TieuDe == "Tạm khóa đăng nhập");
            gio = gio.AddSeconds(DangNhapPresenter.GiayKhoa + 1);
            p.DangNhapAsync(CancellationToken.None).Wait();
            Kiem("MVP-DN04 hết thời gian khóa thì thử lại được", sv.SoLanGoi == 6);

            view = new DangNhapViewGia { TenDangNhap = "admin", MatKhau = "dung" };
            var nd = new NguoiDung { MaNguoiDung = 9, TenDangNhap = "admin" };
            sv = new NguoiDungGia { TraVe = nd };
            new DangNhapPresenter(view, sv).DangNhapAsync(CancellationToken.None).Wait();
            Kiem("MVP-DN05 đăng nhập đúng: View nhận người dùng", ReferenceEquals(view.KetQua, nd));

            view = new DangNhapViewGia { TenDangNhap = "admin", MatKhau = "x" };
            sv = new NguoiDungGia { Loi = new InvalidOperationException("mất kết nối") };
            new DangNhapPresenter(view, sv).DangNhapAsync(CancellationToken.None).Wait();
            Kiem("MVP-DN06 lỗi CSDL: báo lỗi và mở lại nút", view.CuoiCung.Muc == MucThongBao.Loi && view.TrangThaiXuLy.Count == 2 && !view.TrangThaiXuLy[1]);
        }

        // ---------- Đặt phòng ----------
        private sealed class DatPhongViewGia : ViewGia, IDatPhongView
        {
            public int? KhachChon { get; set; }
            public int? PhongChon { get; set; }
            public decimal? GiaPhongChon { get; set; }
            public DateTime NgayDen { get; set; }
            public DateTime NgayDi { get; set; }
            public decimal TienCoc { get; set; }
            public string LocTrangThai { get; set; }
            public int? MaDatPhongChon { get; set; }
            public string TrangThaiDatChon { get; set; }
            public bool? ChoPhepDat;
            public string GoiY;
            public int SoLanXoaCoc;
            public int? DaMoNhanPhong;
            public void HienKhach(DataTable khach, int? chon) { }
            public void HienPhongTrong(DataTable phong) { }
            public void HienDanhSachDat(DataTable dat) { }
            public void DatChoPhepDat(bool duocDat) { ChoPhepDat = duocDat; }
            public void DatGoiY(string noiDung) { GoiY = noiDung; }
            public void XoaTienCoc() { SoLanXoaCoc++; }
            public void MoNhanPhong(int maDatPhong) { DaMoNhanPhong = maDatPhong; }
        }

        private sealed class DatPhongGia : IDatPhongService
        {
            public DatPhong DaThem;
            public int? DaHuy, DaDanhDauKhongDen;
            public DataTable TimKiem(DateTime? tuNgay = null, DateTime? denNgay = null, string trangThai = null) { return new DataTable(); }
            public DataTable LayDangDat() { return new DataTable(); }
            public int Them(DatPhong x) { DaThem = x; return 1; }
            public void Huy(int maDatPhong) { DaHuy = maDatPhong; }
            public void DanhDauKhongDen(int maDatPhong) { DaDanhDauKhongDen = maDatPhong; }
        }

        private sealed class KhachGia : IKhachHangService
        {
            public DataTable TimKiem(string tuKhoa = null) { return new DataTable(); }
        }

        private sealed class PhongGia : IPhongService
        {
            public DataTable KetQua = new DataTable();
            public DataTable TimKiemTrongKhoang(DateTime ngayDen, DateTime ngayDi) { return KetQua; }
        }

        private static DatPhongViewGia ViewDatPhong()
        {
            return new DatPhongViewGia { NgayDen = new DateTime(2026, 11, 10), NgayDi = new DateTime(2026, 11, 12), KhachChon = 3, PhongChon = 5, TienCoc = 100000m, LocTrangThai = "Đã đặt" };
        }

        private static void DatPhongPresenterTests()
        {
            var view = ViewDatPhong();
            view.KhachChon = null;
            var dp = new DatPhongGia();
            new DatPhongPresenter(view, dp, new KhachGia(), new PhongGia(), NguoiLeTan()).TaoDatPhong();
            Kiem("MVP-DP01 thiếu khách: cảnh báo, không lưu", dp.DaThem == null && view.CuoiCung.Muc == MucThongBao.CanhBao);

            view = ViewDatPhong();
            dp = new DatPhongGia();
            new DatPhongPresenter(view, dp, new KhachGia(), new PhongGia(), NguoiLeTan()).TaoDatPhong();
            Kiem("MVP-DP02 tạo đặt phòng: chuyển đúng dữ liệu từ View vào Service", dp.DaThem != null && dp.DaThem.MaKhachHang == 3 && dp.DaThem.MaPhong == 5
                && dp.DaThem.MaNguoiDung == 2 && dp.DaThem.NgayDen == new DateTime(2026, 11, 10) && dp.DaThem.TienCoc == 100000m);
            Kiem("MVP-DP03 tạo xong thì xóa ô tiền cọc và báo thành công", view.SoLanXoaCoc == 1 && view.CuoiCung.Muc == MucThongBao.ThanhCong);

            view = ViewDatPhong();
            dp = new DatPhongGia();
            var pr = new DatPhongPresenter(view, dp, new KhachGia(), new PhongGia(), NguoiLeTan());
            pr.Huy();
            Kiem("MVP-DP04 hủy khi chưa chọn dòng: nhắc chọn, không gọi Service", dp.DaHuy == null && view.CuoiCung.TieuDe == "Chưa chọn");
            view.MaDatPhongChon = 8;
            view.TraLoiXacNhan = false;
            pr.Huy();
            Kiem("MVP-DP05 không xác nhận thì không hủy", dp.DaHuy == null);
            view.TraLoiXacNhan = true;
            pr.Huy();
            Kiem("MVP-DP06 xác nhận thì hủy đúng mã đặt phòng", dp.DaHuy == 8);

            view = ViewDatPhong();
            view.MaDatPhongChon = 4;
            view.TrangThaiDatChon = "Đã hủy";
            new DatPhongPresenter(view, new DatPhongGia(), new KhachGia(), new PhongGia(), NguoiLeTan()).NhanPhongTuDatNay();
            Kiem("MVP-DP07 đặt phòng đã hủy không được nhận", view.DaMoNhanPhong == null);
            view.TrangThaiDatChon = "Đã đặt";
            new DatPhongPresenter(view, new DatPhongGia(), new KhachGia(), new PhongGia(), NguoiLeTan()).NhanPhongTuDatNay();
            Kiem("MVP-DP08 đặt phòng 'Đã đặt' mở màn hình nhận phòng", view.DaMoNhanPhong == 4);

            view = ViewDatPhong();
            new DatPhongPresenter(view, new DatPhongGia(), new KhachGia(), new PhongGia(), NguoiLeTan()).TaiPhongTrong();
            Kiem("MVP-DP09 không còn phòng trống: khóa nút đặt và báo", view.ChoPhepDat == false && view.GoiY.Contains("Không có phòng trống"));

            view = ViewDatPhong();
            view.GiaPhongChon = 400000m;
            var phong = new PhongGia();
            phong.KetQua.Columns.Add("MaPhong", typeof(int));
            phong.KetQua.Rows.Add(5);
            new DatPhongPresenter(view, new DatPhongGia(), new KhachGia(), phong, NguoiLeTan()).TaiPhongTrong();
            Kiem("MVP-DP10 có phòng trống: gợi ý giá và cọc tối đa (400.000 x 2 đêm)", view.ChoPhepDat == true && view.GoiY.Contains("1 phòng") && view.GoiY.Contains(N(800000m)));
        }

        // ---------- Nhận phòng ----------
        private sealed class NhanPhongViewGia : ViewGia, INhanPhongView
        {
            public bool CheDoTuDat { get; set; }
            public int? MaDatPhongChon { get; set; }
            public int? KhachChon { get; set; }
            public int? PhongChon { get; set; }
            public DateTime NgayDiDuKien { get; set; }
            public bool DaChuyenVangLai, DaDong;
            public string ThongTin;
            public DateTime? NgayDiDat;
            public bool? ChoPhepNhan;
            public void HienDatPhong(DataTable dat, int? chon) { }
            public void HienKhach(DataTable khach, int? chon) { }
            public void HienPhongTrong(DataTable phong) { }
            public void ChuyenSangVangLai() { DaChuyenVangLai = true; }
            public void DatCheDo(bool tuDat) { }
            public void DatNgayDi(DateTime ngay) { NgayDiDat = ngay; }
            public void DatThongTin(string noiDung) { ThongTin = noiDung; }
            public void DatChoPhepNhan(bool duocNhan) { ChoPhepNhan = duocNhan; }
            public void DongThanhCong() { DaDong = true; }
        }

        private sealed class LuuTruGia : ILuuTruService
        {
            public DataTable PhieuDangO = new DataTable();
            public BaoGiaHoaDon BaoGia = new BaoGiaHoaDon { SoDem = 2, TienPhong = 800000m, TienDichVu = 100000m, PhuThu = 0m, TienCoc = 200000m };
            public Exception LoiTinhTien;
            public int MaHoaDon = 77;
            public object[] DaNhanPhong, DaGhiDichVu, DaDoiPhong, DaTraPhong;
            public int? DaXoaDichVu;
            public int? NguoiDuyetTraVe = 1;
            public Exception LoiXacThuc;
            public DataTable ChiTiet = new DataTable();
            public int NhanPhong(int maPhong, int maKhach, int maNguoiDung, DateTime ngayDiDuKien, int? maDatPhong = null)
            {
                DaNhanPhong = new object[] { maPhong, maKhach, maNguoiDung, ngayDiDuKien, maDatPhong };
                return 55;
            }
            public void GhiDichVu(int maPhieuThue, int maDichVu, decimal soLuong) { DaGhiDichVu = new object[] { maPhieuThue, maDichVu, soLuong }; }
            public void XoaDichVu(int maChiTietDichVu) { DaXoaDichVu = maChiTietDichVu; }
            public DataTable LayDichVu(int maPhieuThue) { return ChiTiet; }
            public DataTable LayPhieuDangO() { return PhieuDangO; }
            public DataTable LayPhongCoTheDoi(int maPhongHienTai, DateTime ngayDiDuKien) { return PhongDoi; }
            public DataTable PhongDoi = new DataTable();
            public void DoiPhong(int maPhieuThue, int maPhongMoi, int maNguoiDung, string lyDo) { DaDoiPhong = new object[] { maPhieuThue, maPhongMoi, maNguoiDung, lyDo }; }
            public DataTable LayLichSuDoiPhong(int maPhieuThue) { return new DataTable(); }
            public BaoGiaHoaDon TinhTamTinh(int maPhieuThue, DateTime ngayTra)
            {
                if (LoiTinhTien != null) throw LoiTinhTien;
                return BaoGia;
            }
            public int TraPhong(int maPhieuThue, int maNguoiDung, DateTime ngayTra, decimal giamGia, string hinhThuc, bool laQuanLy, int? maNguoiDuyet = null)
            {
                DaTraPhong = new object[] { maPhieuThue, maNguoiDung, giamGia, hinhThuc, laQuanLy, maNguoiDuyet };
                return MaHoaDon;
            }
            public int XacThucNguoiDuyet(string tenDangNhap, string matKhau)
            {
                if (LoiXacThuc != null) throw LoiXacThuc;
                return NguoiDuyetTraVe.Value;
            }
        }

        private static DataTable BangDat(int maDat, int maPhong, int maKhach, DateTime den, DateTime di)
        {
            var t = new DataTable();
            foreach (string c in new[] { "MaDatPhong", "MaPhong", "MaKhachHang" }) t.Columns.Add(c, typeof(int));
            foreach (string c in new[] { "NgayDen", "NgayDi" }) t.Columns.Add(c, typeof(DateTime));
            t.Columns.Add("SoPhong", typeof(string));
            t.Columns.Add("HoTen", typeof(string));
            t.Columns.Add("TienCoc", typeof(decimal));
            t.Rows.Add(maDat, maPhong, maKhach, den, di, "101", "Nguyễn Văn A", 200000m);
            return t;
        }

        private sealed class DatPhongNhanGia : IDatPhongService
        {
            public DataTable Dat;
            public DataTable TimKiem(DateTime? tuNgay = null, DateTime? denNgay = null, string trangThai = null) { return new DataTable(); }
            public DataTable LayDangDat() { return Dat; }
            public int Them(DatPhong x) { return 0; }
            public void Huy(int maDatPhong) { }
            public void DanhDauKhongDen(int maDatPhong) { }
        }

        private static void NhanPhongPresenterTests()
        {
            var homNay = new DateTime(2026, 11, 5);
            var view = new NhanPhongViewGia { CheDoTuDat = true, MaDatPhongChon = 4, NgayDiDuKien = homNay.AddDays(2) };
            var luuTru = new LuuTruGia();
            var dat = new DatPhongNhanGia { Dat = BangDat(4, 11, 6, homNay.AddDays(1), homNay.AddDays(3)) };
            var p = new NhanPhongPresenter(view, luuTru, dat, new KhachGia(), new PhongGia(), NguoiLeTan(), () => homNay);
            p.KhoiTao(4);
            Kiem("MVP-NP01 chọn đặt phòng: gợi ý ngày đi và ghi chú khách đến sớm", view.NgayDiDat == homNay.AddDays(3) && view.ThongTin.Contains("sớm"));
            p.Nhan();
            Kiem("MVP-NP02 nhận từ đặt trước: lấy phòng, khách từ dòng đặt", luuTru.DaNhanPhong != null && (int)luuTru.DaNhanPhong[0] == 11 && (int)luuTru.DaNhanPhong[1] == 6
                && (int)luuTru.DaNhanPhong[2] == 2 && (int?)luuTru.DaNhanPhong[4] == 4 && view.DaDong);

            view = new NhanPhongViewGia { CheDoTuDat = false, NgayDiDuKien = homNay.AddDays(1) };
            luuTru = new LuuTruGia();
            new NhanPhongPresenter(view, luuTru, dat, new KhachGia(), new PhongGia(), NguoiLeTan(), () => homNay).Nhan();
            Kiem("MVP-NP03 vãng lai thiếu khách hoặc phòng: cảnh báo, không nhận", luuTru.DaNhanPhong == null && view.CuoiCung.Muc == MucThongBao.CanhBao && !view.DaDong);

            view = new NhanPhongViewGia { CheDoTuDat = false, KhachChon = 6, PhongChon = 12, NgayDiDuKien = homNay.AddDays(1) };
            luuTru = new LuuTruGia();
            new NhanPhongPresenter(view, luuTru, dat, new KhachGia(), new PhongGia(), NguoiLeTan(), () => homNay).Nhan();
            Kiem("MVP-NP04 vãng lai hợp lệ: nhận phòng không kèm mã đặt phòng", luuTru.DaNhanPhong != null && luuTru.DaNhanPhong[4] == null && (int)luuTru.DaNhanPhong[0] == 12);

            view = new NhanPhongViewGia { CheDoTuDat = true };
            new NhanPhongPresenter(view, new LuuTruGia(), new DatPhongNhanGia { Dat = new DataTable() }, new KhachGia(), new PhongGia(), NguoiLeTan()).TaiDat(null);
            Kiem("MVP-NP05 không có đặt phòng chờ nhận thì chuyển sang khách vãng lai", view.DaChuyenVangLai);
        }

        // ---------- Ghi dịch vụ ----------
        private sealed class GhiDichVuViewGia : ViewGia, IGhiDichVuView
        {
            public int? PhieuChon { get; set; }
            public int? DichVuChon { get; set; }
            public decimal SoLuong { get; set; }
            public int? ChiTietChon { get; set; }
            public string Tong;
            public int SoLanDatLaiSoLuong;
            public void HienPhieu(DataTable phieu) { }
            public void HienDichVu(DataTable dichVu) { }
            public void HienChiTiet(DataTable chiTiet) { }
            public void DatTong(string noiDung) { Tong = noiDung; }
            public void DatLaiSoLuong() { SoLanDatLaiSoLuong++; }
        }

        private sealed class DichVuGia : IDichVuService
        {
            public DataTable TimKiem(string tuKhoa = null, bool dangHoatDong = false) { return new DataTable(); }
        }

        private static void GhiDichVuPresenterTests()
        {
            var view = new GhiDichVuViewGia { PhieuChon = 3 };
            var lt = new LuuTruGia();
            var p = new GhiDichVuPresenter(view, lt, new DichVuGia());
            p.Ghi();
            Kiem("MVP-DV01 chưa chọn dịch vụ: cảnh báo, không ghi", lt.DaGhiDichVu == null && view.CuoiCung.Muc == MucThongBao.CanhBao);

            view = new GhiDichVuViewGia { PhieuChon = 3, DichVuChon = 7, SoLuong = 2 };
            lt = new LuuTruGia();
            lt.ChiTiet.Columns.Add("ThanhTien", typeof(decimal));
            lt.ChiTiet.Rows.Add(30000m);
            lt.ChiTiet.Rows.Add(50000m);
            new GhiDichVuPresenter(view, lt, new DichVuGia()).Ghi();
            Kiem("MVP-DV02 ghi dịch vụ: gọi Service, đặt lại số lượng và cập nhật tổng", (int)lt.DaGhiDichVu[0] == 3 && (int)lt.DaGhiDichVu[1] == 7 && (decimal)lt.DaGhiDichVu[2] == 2m
                && view.SoLanDatLaiSoLuong == 1 && view.Tong.Contains(N(80000m)) && view.Tong.Contains("2 dòng"));

            view = new GhiDichVuViewGia { ChiTietChon = 9, TraLoiXacNhan = false };
            lt = new LuuTruGia();
            var pr = new GhiDichVuPresenter(view, lt, new DichVuGia());
            pr.Xoa();
            Kiem("MVP-DV03 không xác nhận thì không xóa dịch vụ", lt.DaXoaDichVu == null);
            view.TraLoiXacNhan = true;
            pr.Xoa();
            Kiem("MVP-DV04 xác nhận thì xóa đúng dòng dịch vụ", lt.DaXoaDichVu == 9);
        }

        // ---------- Trả phòng ----------
        private sealed class TraPhongViewGia : ViewGia, ITraPhongView
        {
            public int? PhieuChon { get; set; }
            public DateTime NgayTra { get; set; }
            public decimal GiamGia { get; set; }
            public string HinhThuc { get; set; }
            public int? KetQuaDuyet;
            public int SoLanHoiDuyet, SoLanMoBanIn;
            public string Tong, Giam, GhiChu;
            public bool? ChoPhepTra, DaDong;
            public void HienPhieu(DataTable phieu) { }
            public void HienBaoGia(string soDem, string tienPhong, string tienDichVu, string phuThu, string tienCoc) { }
            public void DatGiamToiDa(decimal toiDa) { }
            public void HienGiamVaTong(string giam, string tong) { Giam = giam; Tong = tong; }
            public void DatGhiChu(string noiDung) { GhiChu = noiDung; }
            public void DatChoPhepTra(bool duocTra) { ChoPhepTra = duocTra; }
            public int? YeuCauQuanLyDuyet() { SoLanHoiDuyet++; return KetQuaDuyet; }
            public void MoBanIn(int maHoaDon, int maPhieu) { SoLanMoBanIn++; }
            public void DongThanhCong() { DaDong = true; }
        }

        private static TraPhongViewGia ViewTra(decimal giam)
        {
            return new TraPhongViewGia { PhieuChon = 6, NgayTra = new DateTime(2026, 11, 5, 11, 0, 0), GiamGia = giam, HinhThuc = "Tiền mặt" };
        }

        private static void TraPhongPresenterTests()
        {
            // Tiền hàng = 800.000 + 100.000 + 0 = 900.000; mức 10% = 90.000
            var view = ViewTra(100000m);
            var lt = new LuuTruGia();
            var p = new TraPhongPresenter(view, lt, NguoiLeTan());
            p.TinhLai();
            p.Tra();
            Kiem("MVP-TP01 Lễ tân giảm vượt 10% mà Quản lý không duyệt: không trả phòng", view.SoLanHoiDuyet == 1 && lt.DaTraPhong == null && view.DaDong != true);

            view = ViewTra(100000m);
            view.KetQuaDuyet = 1;
            lt = new LuuTruGia();
            p = new TraPhongPresenter(view, lt, NguoiLeTan());
            p.TinhLai();
            p.Tra();
            Kiem("MVP-TP02 Quản lý duyệt: trả phòng kèm mã người duyệt, hỏi in hóa đơn", lt.DaTraPhong != null && (int?)lt.DaTraPhong[5] == 1 && view.SoLanMoBanIn == 1 && view.DaDong == true);

            view = ViewTra(90000m);
            lt = new LuuTruGia();
            p = new TraPhongPresenter(view, lt, NguoiLeTan());
            p.TinhLai();
            p.Tra();
            Kiem("MVP-TP03 giảm đúng 10%: Lễ tân tự trả, không cần duyệt", view.SoLanHoiDuyet == 0 && lt.DaTraPhong != null && lt.DaTraPhong[5] == null);

            view = ViewTra(300000m);
            lt = new LuuTruGia();
            p = new TraPhongPresenter(view, lt, NguoiQuanLy());
            p.TinhLai();
            p.Tra();
            Kiem("MVP-TP04 người dùng là Quản lý: không hỏi lại tài khoản duyệt", view.SoLanHoiDuyet == 0 && lt.DaTraPhong != null && (bool)lt.DaTraPhong[4]);

            view = ViewTra(50000m);
            lt = new LuuTruGia();
            p = new TraPhongPresenter(view, lt, NguoiLeTan());
            p.TinhLai();
            Kiem("MVP-TP05 tổng thanh toán = 800.000 + 100.000 - 50.000 - 200.000 cọc", view.Tong == N(650000m) + " đ" && view.Giam == "-" + N(50000m) + " đ");

            view = ViewTra(0m);
            lt = new LuuTruGia { LoiTinhTien = new InvalidOperationException("Phiếu đã trả") };
            new TraPhongPresenter(view, lt, NguoiLeTan()).TinhLai();
            Kiem("MVP-TP06 lỗi khi tạm tính: khóa nút trả và hiện nguyên nhân", view.ChoPhepTra == false && view.GhiChu == "Phiếu đã trả");

            view = ViewTra(0m);
            view.TraLoiXacNhan = false;
            lt = new LuuTruGia();
            new TraPhongPresenter(view, lt, NguoiLeTan()).Tra();
            Kiem("MVP-TP07 không xác nhận thì không trả phòng", lt.DaTraPhong == null);
        }

        // ---------- Đổi phòng ----------
        private sealed class DoiPhongViewGia : ViewGia, IDoiPhongView
        {
            public int? PhieuChon { get; set; }
            public int? PhongMoiChon { get; set; }
            public string LyDo { get; set; }
            public string LoiNhac;
            public int SoLanXoaLyDo;
            public void HienPhieu(DataTable phieu) { }
            public void HienPhongMoi(DataTable phong) { }
            public void HienLichSu(DataTable lichSu) { }
            public void DatLoiNhac(string noiDung) { LoiNhac = noiDung; }
            public void XoaLyDo() { SoLanXoaLyDo++; }
        }

        private static DataTable BangPhieuDoi()
        {
            var t = new DataTable();
            t.Columns.Add("MaPhieuThue", typeof(int));
            t.Columns.Add("MaPhong", typeof(int));
            t.Columns.Add("SoPhong", typeof(string));
            t.Columns.Add("HoTen", typeof(string));
            t.Columns.Add("NgayDiDuKien", typeof(DateTime));
            t.Columns.Add("GiaPhongMoiDem", typeof(decimal));
            t.Rows.Add(30, 1, "101", "Nguyễn Văn A", new DateTime(2026, 11, 8), 400000m);
            return t;
        }

        private static DataTable BangPhongDoi(decimal gia)
        {
            var t = new DataTable();
            t.Columns.Add("MaPhong", typeof(int));
            t.Columns.Add("SoPhong", typeof(string));
            t.Columns.Add("Tang", typeof(int));
            t.Columns.Add("TenLoaiPhong", typeof(string));
            t.Columns.Add("GiaMoiDem", typeof(decimal));
            t.Rows.Add(2, "102", 1, "Đơn", gia);
            return t;
        }

        private static void DoiPhongPresenterTests()
        {
            var view = new DoiPhongViewGia();
            var lt = new LuuTruGia { PhieuDangO = BangPhieuDoi(), PhongDoi = BangPhongDoi(500000m) };
            var p = new DoiPhongPresenter(view, lt, NguoiLeTan());
            p.TaiPhieu();
            p.Doi();
            Kiem("MVP-DOI01 chưa chọn phòng mới: cảnh báo, không đổi", lt.DaDoiPhong == null && view.CuoiCung.Muc == MucThongBao.CanhBao);

            view.PhieuChon = 30;
            p.TaiPhongMoi();
            view.PhongMoiChon = 2;
            p.CapNhatLoiNhac();
            Kiem("MVP-DOI02 phòng mới khác giá: nhắc giữ đơn giá phiếu", view.LoiNhac != null && !view.LoiNhac.StartsWith("Phòng mới cùng mức giá"));

            view.LyDo = "Khách muốn tầng cao";
            view.TraLoiXacNhan = false;
            p.Doi();
            Kiem("MVP-DOI03 không xác nhận thì không đổi phòng", lt.DaDoiPhong == null && view.SoLanXacNhan == 1);

            view.TraLoiXacNhan = true;
            p.Doi();
            Kiem("MVP-DOI04 đổi phòng: gọi Service đúng mã và lý do, xóa ô lý do", lt.DaDoiPhong != null && (int)lt.DaDoiPhong[0] == 30 && (int)lt.DaDoiPhong[1] == 2
                && (int)lt.DaDoiPhong[2] == 2 && (string)lt.DaDoiPhong[3] == "Khách muốn tầng cao" && view.SoLanXoaLyDo == 1);
        }

        // ---------- Duyệt giảm giá ----------
        private sealed class DuyetViewGia : ViewGia, IDuyetGiamGiaView
        {
            public string TenDangNhap { get; set; }
            public string MatKhau { get; set; }
            public int SoLanXoaMatKhau;
            public int? MaDuyet;
            public void XoaMatKhau() { SoLanXoaMatKhau++; }
            public void DongThanhCong(int maNguoiDuyet) { MaDuyet = maNguoiDuyet; }
        }

        private static void DuyetGiamGiaPresenterTests()
        {
            var view = new DuyetViewGia { TenDangNhap = "admin", MatKhau = "x" };
            new DuyetGiamGiaPresenter(view, new LuuTruGia { NguoiDuyetTraVe = 1 }).Duyet();
            Kiem("MVP-DUYET01 tài khoản Quản lý hợp lệ: trả về mã người duyệt", view.MaDuyet == 1 && view.SoLanXoaMatKhau == 0);

            view = new DuyetViewGia { TenDangNhap = "letan", MatKhau = "x" };
            new DuyetGiamGiaPresenter(view, new LuuTruGia { LoiXacThuc = new UnauthorizedAccessException("Tài khoản không đúng hoặc không phải Quản lý.") }).Duyet();
            Kiem("MVP-DUYET02 tài khoản không hợp lệ: báo lỗi, xóa mật khẩu, không duyệt", view.MaDuyet == null && view.SoLanXoaMatKhau == 1 && view.CuoiCung.Muc == MucThongBao.CanhBao);
        }

        // ---------- Báo cáo ----------
        private sealed class BaoCaoViewGia : ViewGia, IBaoCaoView
        {
            public DateTime DoanhThuTu { get; set; }
            public DateTime DoanhThuDen { get; set; }
            public KieuGomDoanhThu KieuGom { get; set; }
            public DateTime CongSuatTu { get; set; }
            public DateTime CongSuatDen { get; set; }
            public IList<string> Nhan;
            public IList<double> GiaTri;
            public string Tong;
            public void HienDoanhThu(DataTable bang, IList<string> nhan, IList<double> giaTri, string tong) { Nhan = nhan; GiaTri = giaTri; Tong = tong; }
            public void HienCongSuat(DataTable bang, IList<string> nhan, IList<double> giaTri, string tong) { Nhan = nhan; GiaTri = giaTri; Tong = tong; }
        }

        private sealed class BaoCaoGia : IBaoCaoService
        {
            public DataTable DT, CS;
            public Exception Loi;
            public DataTable DoanhThu(DateTime tuNgay, DateTime denNgay, KieuGomDoanhThu kieu) { if (Loi != null) throw Loi; return DT; }
            public DataTable CongSuat(DateTime tuNgay, DateTime denNgay) { if (Loi != null) throw Loi; return CS; }
        }

        private static void BaoCaoPresenterTests()
        {
            var dt = new DataTable();
            dt.Columns.Add("Nhan", typeof(string));
            dt.Columns.Add("SoHoaDon", typeof(long));
            dt.Columns.Add("DoanhThu", typeof(decimal));
            dt.Rows.Add("05/11/2026", 2L, 1500000m);
            dt.Rows.Add("06/11/2026", 1L, 500000m);
            var view = new BaoCaoViewGia { KieuGom = KieuGomDoanhThu.Ngay, DoanhThuTu = new DateTime(2026, 11, 1), DoanhThuDen = new DateTime(2026, 11, 30) };
            new BaoCaoPresenter(view, new BaoCaoGia { DT = dt }).TaiDoanhThu();
            Kiem("MVP-BC01 doanh thu theo ngày: nhãn rút gọn dd/MM, tổng 2.000.000 đ và 3 hóa đơn", view.Nhan != null && view.Nhan[0] == "05/11" && view.GiaTri[1] == 500000d
                && view.Tong.Contains(N(2000000m)) && view.Tong.Contains("3 hóa đơn"));

            var cs = new DataTable();
            cs.Columns.Add("Ngay", typeof(DateTime));
            cs.Columns.Add("SoPhongCoKhach", typeof(int));
            cs.Columns.Add("TongPhong", typeof(int));
            cs.Columns.Add("TyLe", typeof(decimal));
            cs.Rows.Add(new DateTime(2026, 11, 5), 5, 10, 50m);
            cs.Rows.Add(new DateTime(2026, 11, 6), 7, 10, 70m);
            view = new BaoCaoViewGia { CongSuatTu = new DateTime(2026, 11, 5), CongSuatDen = new DateTime(2026, 11, 6) };
            new BaoCaoPresenter(view, new BaoCaoGia { CS = cs }).TaiCongSuat();
            Kiem("MVP-BC02 công suất: tỷ lệ trung bình 60%", view.Nhan[1] == "06/11" && view.Tong.Contains("60"));

            view = new BaoCaoViewGia { KieuGom = KieuGomDoanhThu.Ngay };
            new BaoCaoPresenter(view, new BaoCaoGia { Loi = new InvalidOperationException("Lỗi kết nối") }).TaiDoanhThu();
            Kiem("MVP-BC03 lỗi khi thống kê: báo cho người dùng, không văng lỗi", view.CuoiCung != null && view.CuoiCung.NoiDung == "Lỗi kết nối");
        }
    }
}
