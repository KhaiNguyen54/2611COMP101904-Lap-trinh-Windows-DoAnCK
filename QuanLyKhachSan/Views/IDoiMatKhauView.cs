namespace QuanLyKhachSan.Views
{
    /// <summary>View của hộp thoại đổi mật khẩu.</summary>
    public interface IDoiMatKhauView : IThongBaoView
    {
        string MatKhauCu { get; }
        string MatKhauMoi { get; }
        string XacNhanMatKhau { get; }
        void Dong();
    }
}
