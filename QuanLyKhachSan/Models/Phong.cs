namespace QuanLyKhachSan.Models
{
    public class Phong
    {
        public int MaPhong { get; set; }
        public string SoPhong { get; set; }
        public int Tang { get; set; }
        public int MaLoaiPhong { get; set; }
        public string TenLoaiPhong { get; set; }
        public string TrangThai { get; set; }
        public string GhiChu { get; set; }
        public bool HoatDong { get; set; }
    }

    /// <summary>Các giá trị hợp lệ của Phong.TrangThai (khớp ràng buộc CK_Phong_TrangThai).</summary>
    public static class TrangThaiPhong
    {
        public const string Trong = "Trống";
        public const string DaDat = "Đã đặt";
        public const string DangO = "Đang ở";
        public const string DonDep = "Dọn dẹp";
        public const string BaoTri = "Bảo trì";
    }
}
