namespace QuanLyKhachSan.Views
{
    /// <summary>View của hộp thoại thêm nhanh khách hàng tại quầy.</summary>
    public interface IKhachNhanhView : IThongBaoView
    {
        string HoTen { get; }
        string SoGiayTo { get; }
        string SoDienThoai { get; }
        string Email { get; }
        string DiaChi { get; }

        /// <summary>Đóng hộp thoại với kết quả OK; maKhach là 0 nếu không tìm lại được khách vừa thêm.</summary>
        void DongThanhCong(int maKhach);
    }
}
