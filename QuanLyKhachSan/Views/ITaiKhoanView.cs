using System.Data;

namespace QuanLyKhachSan.Views
{
    /// <summary>View của màn hình quản lý tài khoản (chỉ Quản lý).</summary>
    public interface ITaiKhoanView : IThongBaoView
    {
        string TenDangNhap { get; }
        string HoTen { get; }
        string MatKhau { get; }
        string VaiTro { get; }

        /// <summary>Mã tài khoản đang chọn trong danh sách; 0 nếu chưa chọn.</summary>
        int MaDangChon { get; }

        void HienDanhSach(DataTable bang);
        void XoaFormTao();
        void XoaMatKhau();
    }
}
