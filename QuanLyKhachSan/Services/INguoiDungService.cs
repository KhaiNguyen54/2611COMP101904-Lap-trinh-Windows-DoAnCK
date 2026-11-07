using System.Data;
using System.Threading;
using System.Threading.Tasks;
using QuanLyKhachSan.Models;

namespace QuanLyKhachSan.Services
{
    /// <summary>Phần nghiệp vụ tài khoản mà Presenter cần; cho phép thay bằng bản giả khi kiểm thử.</summary>
    public interface INguoiDungService
    {
        Task<NguoiDung> DangNhapAsync(string tenDangNhap, string matKhau, CancellationToken cancellationToken = default(CancellationToken));
        DataTable LayDanhSach(NguoiDung nguoiQuanLy);
        void TaoTaiKhoan(NguoiDung nguoiQuanLy, string tenDangNhap, string hoTen, string vaiTro, string matKhau);
        void DatLaiMatKhau(NguoiDung nguoiQuanLy, int ma, string matKhauMoi);
        void KhoaTaiKhoan(NguoiDung nguoiQuanLy, int ma);
        void DoiMatKhau(NguoiDung nguoiDung, string matKhauCu, string matKhauMoi);
    }
}
