using System;
using System.Text.RegularExpressions;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using QuanLyKhachSan.DAL;
using QuanLyKhachSan.DTO;

namespace QuanLyKhachSan.BLL
{
    public class NguoiDungBLL
    {
        private readonly NguoiDungDAL _dal;
        public NguoiDungBLL() : this(new NguoiDungDAL()) { }
        public NguoiDungBLL(NguoiDungDAL dal) { _dal = dal ?? throw new ArgumentNullException(nameof(dal)); }

        public NguoiDung DangNhap(string tenDangNhap, string matKhau)
        {
            if (string.IsNullOrWhiteSpace(tenDangNhap) || string.IsNullOrEmpty(matKhau))
                throw new ArgumentException("Vui lòng nhập tên đăng nhập và mật khẩu.");
            NguoiDung nguoiDung = _dal.TimTheoTenDangNhap(tenDangNhap.Trim());
            if (nguoiDung == null || !nguoiDung.HoatDong ||
                !PasswordHasher.KiemTra(matKhau, nguoiDung.MatKhauHash, nguoiDung.MatKhauSalt, nguoiDung.SoVongLap))
                return null;
            return nguoiDung;
        }

        public async Task<NguoiDung> DangNhapAsync(string tenDangNhap, string matKhau, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(tenDangNhap) || string.IsNullOrEmpty(matKhau))
                throw new ArgumentException("Vui lòng nhập tên đăng nhập và mật khẩu.");
            NguoiDung nguoiDung = await _dal.TimTheoTenDangNhapAsync(tenDangNhap.Trim(), cancellationToken).ConfigureAwait(false);
            if (nguoiDung == null || !nguoiDung.HoatDong ||
                !PasswordHasher.KiemTra(matKhau, nguoiDung.MatKhauHash, nguoiDung.MatKhauSalt, nguoiDung.SoVongLap))
                return null;
            return nguoiDung;
        }

        public DataTable LayDanhSach(NguoiDung nguoiQuanLy){KiemTraQuyenQuanLy(nguoiQuanLy);return _dal.LayDanhSach();}

        public void TaoTaiKhoan(NguoiDung nguoiQuanLy,string tenDangNhap,string hoTen,string vaiTro,string matKhau)
        {
            KiemTraQuyenQuanLy(nguoiQuanLy);
            if(string.IsNullOrWhiteSpace(tenDangNhap)||tenDangNhap.Length>50||string.IsNullOrWhiteSpace(hoTen)||hoTen.Length>100)throw new ArgumentException("Tên đăng nhập hoặc họ tên không hợp lệ.");
            if(vaiTro!=VaiTroNguoiDung.LeTan&&vaiTro!=VaiTroNguoiDung.QuanLy)throw new ArgumentException("Vai trò không hợp lệ.");
            KiemTraMatKhauManh(matKhau);PasswordHasher.TaoHash(matKhau,out string hash,out string salt,out int rounds);_dal.TaoTaiKhoan(tenDangNhap.Trim(),hoTen.Trim(),vaiTro,hash,salt,rounds);
        }

        public void DatLaiMatKhau(NguoiDung nguoiQuanLy,int ma,string matKhauMoi)
        {
            KiemTraQuyenQuanLy(nguoiQuanLy);KiemTraMatKhauManh(matKhauMoi);PasswordHasher.TaoHash(matKhauMoi,out string hash,out string salt,out int rounds);
            if(_dal.DatLaiMatKhau(ma,hash,salt,rounds)!=1)throw new InvalidOperationException("Không tìm thấy tài khoản đang hoạt động.");
        }

        public void KhoaTaiKhoan(NguoiDung nguoiQuanLy,int ma)
        {
            KiemTraQuyenQuanLy(nguoiQuanLy);if(nguoiQuanLy.MaNguoiDung==ma)throw new InvalidOperationException("Không thể tự khóa tài khoản của mình.");_dal.KhoaTaiKhoan(ma);
        }

        public void DoiMatKhau(NguoiDung nguoiDung, string matKhauCu, string matKhauMoi)
        {
            if (nguoiDung == null) throw new ArgumentNullException(nameof(nguoiDung));
            NguoiDung hienTai = _dal.TimTheoTenDangNhap(nguoiDung.TenDangNhap);
            if (hienTai == null || !hienTai.HoatDong ||
                !PasswordHasher.KiemTra(matKhauCu, hienTai.MatKhauHash, hienTai.MatKhauSalt, hienTai.SoVongLap))
                throw new InvalidOperationException("Mật khẩu hiện tại không đúng.");
            KiemTraMatKhauManh(matKhauMoi);
            PasswordHasher.TaoHash(matKhauMoi, out string hash, out string salt, out int vongLap);
            _dal.CapNhatMatKhau(hienTai.MaNguoiDung, hash, salt, vongLap);
        }

        public static void KiemTraMatKhauManh(string matKhau)
        {
            if (string.IsNullOrEmpty(matKhau) || matKhau.Length < 8 ||
                !Regex.IsMatch(matKhau, "[A-Za-z]") || !Regex.IsMatch(matKhau, "[0-9]"))
                throw new ArgumentException("Mật khẩu phải có ít nhất 8 ký tự, gồm chữ cái và chữ số.");
        }

        public static bool CoQuyenQuanLy(NguoiDung nguoiDung)
        {
            return nguoiDung != null && nguoiDung.HoatDong && nguoiDung.VaiTro == VaiTroNguoiDung.QuanLy;
        }

        private static void KiemTraQuyenQuanLy(NguoiDung nguoiDung){if(!CoQuyenQuanLy(nguoiDung))throw new UnauthorizedAccessException("Chức năng này chỉ dành cho Quản lý.");}
    }
}
