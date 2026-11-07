using System.Threading;
using System.Threading.Tasks;
using QuanLyKhachSan.Models;

namespace QuanLyKhachSan.Services
{
    /// <summary>Phần nghiệp vụ tài khoản mà Presenter cần; cho phép thay bằng bản giả khi kiểm thử.</summary>
    public interface INguoiDungService
    {
        Task<NguoiDung> DangNhapAsync(string tenDangNhap, string matKhau, CancellationToken cancellationToken = default);
    }
}
