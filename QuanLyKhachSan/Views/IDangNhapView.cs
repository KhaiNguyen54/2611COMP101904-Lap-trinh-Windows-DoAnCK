using QuanLyKhachSan.Models;

namespace QuanLyKhachSan.Views
{
    /// <summary>View của màn hình đăng nhập.</summary>
    public interface IDangNhapView : IThongBaoView
    {
        string TenDangNhap { get; }
        string MatKhau { get; }
        void DatDangXuLy(bool dangXuLy);
        void XoaMatKhau();
        void DongThanhCong(NguoiDung nguoiDung);
    }
}
