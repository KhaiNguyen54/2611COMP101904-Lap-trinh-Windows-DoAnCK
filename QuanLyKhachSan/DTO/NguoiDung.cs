namespace QuanLyKhachSan.DTO
{
    /// <summary>
    /// Đối tượng người dùng (tài khoản đăng nhập). Dùng chung giữa GUI, BLL, DAL.
    /// </summary>
    public class NguoiDung
    {
        public int MaNguoiDung { get; set; }
        public string TenDangNhap { get; set; }
        public string MatKhauHash { get; set; }
        public string MatKhauSalt { get; set; }
        public int SoVongLap { get; set; }
        public string HoTen { get; set; }

        /// <summary>Giá trị lấy từ <see cref="VaiTroNguoiDung"/>.</summary>
        public string VaiTro { get; set; }

        public bool HoatDong { get; set; }
    }

    /// <summary>
    /// Hai vai trò sử dụng hệ thống.
    /// </summary>
    public static class VaiTroNguoiDung
    {
        public const string LeTan = "LeTan";
        public const string QuanLy = "QuanLy";
    }
}
