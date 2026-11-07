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
            DanhMucPresenterTests();
            TaiKhoanPresenterTests();
            KhachNhanhPresenterTests();
            DoiMatKhauPresenterTests();
            HoaDonPresenterTests();
            KhachDangOPresenterTests();
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
            public NguoiDung DaTao;
            public string MatKhauTao, VaiTroTao, MatKhauDatLai, MatKhauCuDoi, MatKhauMoiDoi;
            public int? DaDatLai, DaKhoa;
            public Exception LoiTao;
            public DataTable DanhSach = new DataTable();
            public void TaoTaiKhoan(NguoiDung nguoiQuanLy, string tenDangNhap, string hoTen, string vaiTro, string matKhau)
            {
                if (LoiTao != null) throw LoiTao;
                DaTao = new NguoiDung { TenDangNhap = tenDangNhap, HoTen = hoTen };
                VaiTroTao = vaiTro; MatKhauTao = matKhau;
            }
            public DataTable LayDanhSach(NguoiDung nguoiQuanLy) { return DanhSach; }
            public void DatLaiMatKhau(NguoiDung nguoiQuanLy, int ma, string matKhauMoi) { DaDatLai = ma; MatKhauDatLai = matKhauMoi; }
            public void KhoaTaiKhoan(NguoiDung nguoiQuanLy, int ma) { DaKhoa = ma; }
            public void DoiMatKhau(NguoiDung nguoiDung, string matKhauCu, string matKhauMoi)
            {
                if (LoiTao != null) throw LoiTao;
                MatKhauCuDoi = matKhauCu; MatKhauMoiDoi = matKhauMoi;
            }
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
            public DataTable KetQuaTim = new DataTable();
            public string TuKhoaTim;
            public KhachHang DaThem, DaCapNhat;
            public Exception LoiThem;
            public DataTable TimKiem(string tuKhoa = null) { TuKhoaTim = tuKhoa; return KetQuaTim; }
            public int Them(KhachHang x) { if (LoiThem != null) throw LoiThem; DaThem = x; return 1; }
            public int CapNhat(KhachHang x) { DaCapNhat = x; return 1; }
        }

        private sealed class PhongGia : IPhongService
        {
            public DataTable KetQua = new DataTable();
            public Phong DaThem, DaCapNhat;
            public int KetQuaNgung = 1;
            public int? DaNgung;
            public DataTable TimKiem(string tuKhoa = null, int? tang = null, int? maLoai = null, string trangThai = null) { return new DataTable(); }
            public DataTable TimKiemTrongKhoang(DateTime ngayDen, DateTime ngayDi) { return KetQua; }
            public int Them(Phong x) { DaThem = x; return 1; }
            public int CapNhat(Phong x) { DaCapNhat = x; return 1; }
            public int NgungSuDung(int ma) { DaNgung = ma; return KetQuaNgung; }
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
            public DataTable KhachDangO = new DataTable();
            public DataTable LayKhachDangO() { return KhachDangO; }
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
            public DichVu DaThem, DaCapNhat;
            public int KetQuaNgung = 1;
            public int? DaNgung;
            public DataTable TimKiem(string tuKhoa = null, bool dangHoatDong = false) { return new DataTable(); }
            public int Them(DichVu x) { DaThem = x; return 1; }
            public int CapNhat(DichVu x) { DaCapNhat = x; return 1; }
            public int NgungSuDung(int ma) { DaNgung = ma; return KetQuaNgung; }
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

        // ---------- Danh mục ----------
        private sealed class LoaiPhongGia : ILoaiPhongService
        {
            public LoaiPhong DaThem, DaCapNhat;
            public int KetQuaNgung = 1;
            public int? DaNgung;
            public DataTable DanhSach = new DataTable();
            public DataTable TimKiem(string tuKhoa = null) { return DanhSach; }
            public int Them(LoaiPhong x) { DaThem = x; return 1; }
            public int CapNhat(LoaiPhong x) { DaCapNhat = x; return 1; }
            public int NgungSuDung(int ma) { DaNgung = ma; return KetQuaNgung; }
        }

        private sealed class DanhMucViewGia : ViewGia, IDanhMucView
        {
            public string TuKhoa { get; set; }
            public int MaDangSua { get; set; }
            public string[] Chu = new string[6];
            public decimal[] So = new decimal[6];
            public int? Chon;
            public int SoLanHienDanhSach, SoLanLamMoi, SoLanHienLoaiPhong;
            public string TrangThai;
            public string DocChu(int viTri) { return Chu[viTri]; }
            public decimal DocSo(int viTri) { return So[viTri]; }
            public int? DocChon(int viTri) { return Chon; }
            public void HienDanhSach(DataTable bang) { SoLanHienDanhSach++; }
            public void HienLoaiPhong(DataTable loaiPhong) { SoLanHienLoaiPhong++; }
            public void LamMoiFormNhap() { SoLanLamMoi++; }
            public void DatTrangThai(string noiDung) { TrangThai = noiDung; }
        }

        private static void DanhMucPresenterTests()
        {
            Kiem("MVP-DM01 Lễ tân chỉ được sửa Khách hàng, các danh mục khác chỉ xem",
                !DanhMucPresenter.ChiXem(LoaiDanhMuc.KhachHang, NguoiLeTan()) && DanhMucPresenter.ChiXem(LoaiDanhMuc.Phong, NguoiLeTan())
                && DanhMucPresenter.ChiXem(LoaiDanhMuc.DichVu, NguoiLeTan()) && DanhMucPresenter.ChiXem(LoaiDanhMuc.LoaiPhong, null));
            Kiem("MVP-DM02 Quản lý sửa được mọi danh mục", !DanhMucPresenter.ChiXem(LoaiDanhMuc.LoaiPhong, NguoiQuanLy()) && !DanhMucPresenter.ChiXem(LoaiDanhMuc.Phong, NguoiQuanLy()));

            var gio = new DateTime(2026, 11, 7, 9, 5, 3);
            var view = new DanhMucViewGia { MaDangSua = 0 };
            view.Chu[0] = "Nguyễn Văn A"; view.Chu[1] = "012345678901"; view.Chu[2] = "0901234567"; view.Chu[3] = "a@x.vn"; view.Chu[4] = "Hà Nội";
            var khach = new KhachGia();
            var pr = new DanhMucPresenter(view, LoaiDanhMuc.KhachHang, null, null, khach, null, () => gio);
            pr.Luu();
            Kiem("MVP-DM03 khách hàng mới: gọi Them với đủ 5 trường, làm mới form, ghi giờ lưu",
                khach.DaThem != null && khach.DaThem.HoTen == "Nguyễn Văn A" && khach.DaThem.DiaChi == "Hà Nội" && khach.DaCapNhat == null
                && view.SoLanLamMoi == 1 && view.TrangThai == "Đã lưu lúc 09:05:03");
            view.MaDangSua = 7;
            pr.Luu();
            Kiem("MVP-DM04 đang sửa bản ghi: gọi CapNhat với đúng mã", khach.DaCapNhat != null && khach.DaCapNhat.MaKhachHang == 7);

            view = new DanhMucViewGia { Chon = null };
            view.Chu[0] = "101"; view.So[1] = 1;
            var phong = new PhongGia();
            new DanhMucPresenter(view, LoaiDanhMuc.Phong, null, phong, null, null).Luu();
            Kiem("MVP-DM05 phòng chưa chọn loại: báo 'Chọn loại phòng', không lưu", phong.DaThem == null && view.CuoiCung.NoiDung == "Chọn loại phòng." && view.CuoiCung.Muc == MucThongBao.CanhBao);
            view.Chon = 3; view.So[1] = 2; view.Chu[3] = "Gần thang máy";
            new DanhMucPresenter(view, LoaiDanhMuc.Phong, null, phong, null, null).Luu();
            Kiem("MVP-DM06 phòng hợp lệ: truyền số phòng, tầng, loại, ghi chú", phong.DaThem != null && phong.DaThem.SoPhong == "101" && phong.DaThem.Tang == 2 && phong.DaThem.MaLoaiPhong == 3 && phong.DaThem.GhiChu == "Gần thang máy");

            view = new DanhMucViewGia();
            view.Chu[0] = "Deluxe"; view.Chu[1] = "View biển"; view.So[2] = 900000m; view.So[3] = 3;
            var loai = new LoaiPhongGia();
            new DanhMucPresenter(view, LoaiDanhMuc.LoaiPhong, loai, null, null, null).Luu();
            Kiem("MVP-DM07 loại phòng: giá và sức chứa lấy từ ô số", loai.DaThem != null && loai.DaThem.GiaMoiDem == 900000m && loai.DaThem.SucChua == 3 && loai.DaThem.TenLoaiPhong == "Deluxe");

            view = new DanhMucViewGia();
            view.Chu[0] = "Giặt ủi"; view.So[1] = 50000m; view.Chu[2] = "kg";
            var dv = new DichVuGia();
            new DanhMucPresenter(view, LoaiDanhMuc.DichVu, null, null, null, dv).Luu();
            Kiem("MVP-DM08 dịch vụ: tên, đơn giá, đơn vị", dv.DaThem != null && dv.DaThem.DonGia == 50000m && dv.DaThem.DonViTinh == "kg");

            view = new DanhMucViewGia();
            var khachLoi = new KhachGia { LoiThem = new ArgumentException("Số giấy tờ đã tồn tại.") };
            new DanhMucPresenter(view, LoaiDanhMuc.KhachHang, null, null, khachLoi, null).Luu();
            Kiem("MVP-DM09 lỗi nghiệp vụ khi lưu: báo cảnh báo, không làm mới form", view.CuoiCung.Muc == MucThongBao.CanhBao && view.CuoiCung.NoiDung == "Số giấy tờ đã tồn tại." && view.SoLanLamMoi == 0);

            view = new DanhMucViewGia { MaDangSua = 0 };
            dv = new DichVuGia();
            var pn = new DanhMucPresenter(view, LoaiDanhMuc.DichVu, null, null, null, dv);
            pn.Ngung();
            Kiem("MVP-DM10 ngừng khi chưa chọn dòng: nhắc chọn, không gọi Service", dv.DaNgung == null && view.CuoiCung.TieuDe == "Chưa chọn");
            view.MaDangSua = 4; view.TraLoiXacNhan = false;
            pn.Ngung();
            Kiem("MVP-DM11 không xác nhận thì không ngừng", dv.DaNgung == null);
            view.TraLoiXacNhan = true;
            pn.Ngung();
            Kiem("MVP-DM12 xác nhận thì ngừng đúng mã và làm mới", dv.DaNgung == 4 && view.SoLanLamMoi == 1);
            dv.KetQuaNgung = 0;
            pn.Ngung();
            Kiem("MVP-DM13 bản ghi đang được dùng (kết quả 0): báo không thể ngừng", view.CuoiCung.TieuDe == "Không thể ngừng");

            view = new DanhMucViewGia();
            loai = new LoaiPhongGia();
            loai.DanhSach.Columns.Add("HoatDong", typeof(bool));
            new DanhMucPresenter(view, LoaiDanhMuc.Phong, loai, new PhongGia(), null, null).TaiBanDau();
            Kiem("MVP-DM14 danh mục Phòng nạp thêm danh sách loại phòng; loại khác thì không", view.SoLanHienLoaiPhong == 1 && view.SoLanHienDanhSach == 1);
            view = new DanhMucViewGia();
            new DanhMucPresenter(view, LoaiDanhMuc.DichVu, null, null, null, new DichVuGia()).TaiBanDau();
            Kiem("MVP-DM15 danh mục Dịch vụ chỉ nạp danh sách", view.SoLanHienLoaiPhong == 0 && view.SoLanHienDanhSach == 1);
        }

        // ---------- Tài khoản ----------
        private sealed class TaiKhoanViewGia : ViewGia, ITaiKhoanView
        {
            public string TenDangNhap { get; set; }
            public string HoTen { get; set; }
            public string MatKhau { get; set; }
            public string VaiTro { get; set; }
            public int MaDangChon { get; set; }
            public int SoLanHien, SoLanXoaForm, SoLanXoaMatKhau;
            public void HienDanhSach(DataTable bang) { SoLanHien++; }
            public void XoaFormTao() { SoLanXoaForm++; }
            public void XoaMatKhau() { SoLanXoaMatKhau++; }
        }

        private static void TaiKhoanPresenterTests()
        {
            var view = new TaiKhoanViewGia { TenDangNhap = "letan2", HoTen = "Lê Tân", MatKhau = "Abcdef12", VaiTro = "LeTan" };
            var sv = new NguoiDungGia();
            var p = new TaiKhoanPresenter(view, sv, NguoiQuanLy());
            p.Tao();
            Kiem("MVP-TK01 tạo tài khoản: chuyển đúng dữ liệu, xóa form, tải lại danh sách, báo thành công",
                sv.DaTao != null && sv.DaTao.TenDangNhap == "letan2" && sv.VaiTroTao == "LeTan" && view.SoLanXoaForm == 1 && view.SoLanHien == 1 && view.CuoiCung.Muc == MucThongBao.ThanhCong);

            view = new TaiKhoanViewGia();
            sv = new NguoiDungGia { LoiTao = new ArgumentException("Mật khẩu yếu.") };
            new TaiKhoanPresenter(view, sv, NguoiQuanLy()).Tao();
            Kiem("MVP-TK02 mật khẩu yếu: cảnh báo, không xóa form", view.CuoiCung.Muc == MucThongBao.CanhBao && view.SoLanXoaForm == 0);

            view = new TaiKhoanViewGia { MaDangChon = 5, MatKhau = "MatKhauMoi1" };
            sv = new NguoiDungGia();
            p = new TaiKhoanPresenter(view, sv, NguoiQuanLy());
            p.DatLaiMatKhau();
            Kiem("MVP-TK03 đặt lại mật khẩu: đúng mã, xóa ô mật khẩu, báo thành công", sv.DaDatLai == 5 && sv.MatKhauDatLai == "MatKhauMoi1" && view.SoLanXoaMatKhau == 1 && view.CuoiCung.Muc == MucThongBao.ThanhCong);

            view.MaDangChon = 0;
            p = new TaiKhoanPresenter(view, sv, NguoiQuanLy());
            p.Khoa();
            Kiem("MVP-TK04 khóa khi chưa chọn dòng: không hỏi, không khóa", sv.DaKhoa == null && view.SoLanXacNhan == 0);
            view.MaDangChon = 6; view.TraLoiXacNhan = false;
            p.Khoa();
            Kiem("MVP-TK05 không xác nhận thì không khóa", sv.DaKhoa == null && view.SoLanXacNhan == 1);
            view.TraLoiXacNhan = true;
            p.Khoa();
            Kiem("MVP-TK06 xác nhận thì khóa đúng tài khoản và tải lại danh sách", sv.DaKhoa == 6 && view.SoLanHien == 1);
        }

        // ---------- Thêm nhanh khách ----------
        private sealed class KhachNhanhViewGia : ViewGia, IKhachNhanhView
        {
            public string HoTen { get; set; }
            public string SoGiayTo { get; set; }
            public string SoDienThoai { get; set; }
            public string Email { get; set; }
            public string DiaChi { get; set; }
            public int? MaDong;
            public void DongThanhCong(int maKhach) { MaDong = maKhach; }
        }

        private static void KhachNhanhPresenterTests()
        {
            var view = new KhachNhanhViewGia { HoTen = "Trần B", SoGiayTo = " 079123456789 " };
            var khach = new KhachGia();
            khach.KetQuaTim.Columns.Add("MaKhachHang", typeof(int));
            khach.KetQuaTim.Rows.Add(42);
            new KhachNhanhPresenter(view, khach).Luu();
            Kiem("MVP-KN01 thêm nhanh: tra lại theo giấy tờ đã cắt khoảng trắng và trả mã khách", khach.DaThem != null && khach.TuKhoaTim == "079123456789" && view.MaDong == 42);

            view = new KhachNhanhViewGia { HoTen = "Trần B", SoGiayTo = "1" };
            khach = new KhachGia();
            new KhachNhanhPresenter(view, khach).Luu();
            Kiem("MVP-KN02 không tìm lại được khách thì trả mã 0 nhưng vẫn đóng", view.MaDong == 0);

            view = new KhachNhanhViewGia();
            khach = new KhachGia { LoiThem = new ArgumentException("Họ tên bắt buộc.") };
            new KhachNhanhPresenter(view, khach).Luu();
            Kiem("MVP-KN03 dữ liệu sai: cảnh báo và không đóng hộp thoại", view.MaDong == null && view.CuoiCung.Muc == MucThongBao.CanhBao);
        }

        // ---------- Đổi mật khẩu ----------
        private sealed class DoiMatKhauViewGia : ViewGia, IDoiMatKhauView
        {
            public string MatKhauCu { get; set; }
            public string MatKhauMoi { get; set; }
            public string XacNhanMatKhau { get; set; }
            public bool DaDong;
            public void Dong() { DaDong = true; }
        }

        private static void DoiMatKhauPresenterTests()
        {
            var view = new DoiMatKhauViewGia { MatKhauCu = "a", MatKhauMoi = "Abcdef12", XacNhanMatKhau = "Abcdef13" };
            var sv = new NguoiDungGia();
            new DoiMatKhauPresenter(view, sv, NguoiLeTan()).Luu();
            Kiem("MVP-MK01 xác nhận không khớp: cảnh báo, không gọi Service", sv.MatKhauMoiDoi == null && view.CuoiCung.Muc == MucThongBao.CanhBao && !view.DaDong);

            view.XacNhanMatKhau = "Abcdef12";
            new DoiMatKhauPresenter(view, sv, NguoiLeTan()).Luu();
            Kiem("MVP-MK02 đổi thành công: truyền mật khẩu cũ và mới, báo thành công, đóng hộp thoại", sv.MatKhauCuDoi == "a" && sv.MatKhauMoiDoi == "Abcdef12" && view.DaDong && view.ThongBaos.Exists(t => t.Muc == MucThongBao.ThanhCong));

            view = new DoiMatKhauViewGia { MatKhauCu = "sai", MatKhauMoi = "Abcdef12", XacNhanMatKhau = "Abcdef12" };
            sv = new NguoiDungGia { LoiTao = new InvalidOperationException("Mật khẩu hiện tại không đúng.") };
            new DoiMatKhauPresenter(view, sv, NguoiLeTan()).Luu();
            Kiem("MVP-MK03 mật khẩu cũ sai: báo lỗi, giữ hộp thoại mở", !view.DaDong && view.CuoiCung.NoiDung == "Mật khẩu hiện tại không đúng.");
        }

        // ---------- Hóa đơn ----------
        private sealed class HoaDonGia : IHoaDonService
        {
            public DataTable KetQua = new DataTable();
            public DateTime? TuNgay, DenNgay;
            public string TuKhoa;
            public DataTable TimKiem(DateTime? tuNgay = null, DateTime? denNgay = null, string tuKhoa = null) { TuNgay = tuNgay; DenNgay = denNgay; TuKhoa = tuKhoa; return KetQua; }
        }

        private sealed class HoaDonViewGia : ViewGia, IHoaDonView
        {
            public DateTime TuNgay { get; set; }
            public DateTime DenNgay { get; set; }
            public string TuKhoa { get; set; }
            public DataRow HoaDonChon { get; set; }
            public string Tong;
            public DataRow DaIn;
            public void HienDanhSach(DataTable bang, string tong) { Tong = tong; }
            public void MoBanIn(DataRow hoaDon, DataTable dichVu) { DaIn = hoaDon; }
        }

        private static void HoaDonPresenterTests()
        {
            var view = new HoaDonViewGia { TuNgay = new DateTime(2026, 11, 10), DenNgay = new DateTime(2026, 11, 1) };
            var hd = new HoaDonGia();
            new HoaDonPresenter(view, hd, new LuuTruGia()).Tai();
            Kiem("MVP-HD01 từ ngày sau đến ngày: cảnh báo, không tra cứu", hd.TuNgay == null && view.CuoiCung.TieuDe == "Khoảng ngày");

            var bang = new DataTable();
            bang.Columns.Add("MaPhieuThue", typeof(int));
            bang.Columns.Add("TongThanhToan", typeof(decimal));
            bang.Rows.Add(1, 600000m);
            bang.Rows.Add(2, 400000m);
            view = new HoaDonViewGia { TuNgay = new DateTime(2026, 11, 1), DenNgay = new DateTime(2026, 11, 7), TuKhoa = "101" };
            hd = new HoaDonGia { KetQua = bang };
            new HoaDonPresenter(view, hd, new LuuTruGia()).Tai();
            Kiem("MVP-HD02 tra cứu: truyền khoảng ngày và từ khóa, tổng 2 hóa đơn = 1.000.000", hd.TuKhoa == "101" && view.Tong.Contains("2 hóa đơn") && view.Tong.Contains(N(1000000m)));

            view = new HoaDonViewGia();
            new HoaDonPresenter(view, new HoaDonGia(), new LuuTruGia()).In();
            Kiem("MVP-HD03 in khi chưa chọn hóa đơn: nhắc chọn", view.DaIn == null && view.CuoiCung.TieuDe == "Chưa chọn");

            view = new HoaDonViewGia { HoaDonChon = bang.Rows[1] };
            new HoaDonPresenter(view, new HoaDonGia(), new LuuTruGia()).In();
            Kiem("MVP-HD04 in hóa đơn đã chọn: lấy dịch vụ theo phiếu rồi mở bản in", ReferenceEquals(view.DaIn, bang.Rows[1]));
        }

        // ---------- Khách đang ở ----------
        private sealed class KhachDangOViewGia : ViewGia, IKhachDangOView
        {
            public string TuKhoaLoc { get; set; }
            public string SoLuong;
            public void HienDanhSach(DataTable bang) { }
            public void HienSoLuong(string noiDung) { SoLuong = noiDung; }
        }

        private static void KhachDangOPresenterTests()
        {
            var bang = new DataTable();
            bang.Columns.Add("HoTen", typeof(string));
            bang.Columns.Add("SoPhong", typeof(string));
            bang.Rows.Add("Nguyễn Văn A", "101");
            bang.Rows.Add("Trần Thị B", "202");
            bang.Rows.Add("Lê C", "102");
            var lt = new LuuTruGia { KhachDangO = bang };
            var view = new KhachDangOViewGia { TuKhoaLoc = string.Empty };
            var p = new KhachDangOPresenter(view, lt);
            p.Tai();
            Kiem("MVP-KO01 nạp danh sách: hiển thị đủ 3 khách", view.SoLuong == "3 khách đang lưu trú");
            view.TuKhoaLoc = "10";
            p.Loc();
            Kiem("MVP-KO02 lọc theo số phòng '10' còn 2 khách", view.SoLuong == "2 khách đang lưu trú");
            view.TuKhoaLoc = "trần";
            p.Loc();
            Kiem("MVP-KO03 lọc theo tên không phân biệt hoa thường còn 1 khách", view.SoLuong == "1 khách đang lưu trú");
            view.TuKhoaLoc = "' OR 1=1 --";
            p.Loc();
            Kiem("MVP-KO04 ký tự đặc biệt không phá bộ lọc và không khớp dòng nào", view.SoLuong == "0 khách đang lưu trú");
        }
    }
}
