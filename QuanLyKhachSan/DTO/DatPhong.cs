using System;

namespace QuanLyKhachSan.DTO
{
    public class DatPhong
    {
        public int MaDatPhong { get; set; }
        public int MaKhachHang { get; set; }
        public int MaPhong { get; set; }
        public int MaNguoiDung { get; set; }
        public DateTime NgayDen { get; set; }
        public DateTime NgayDi { get; set; }
        public decimal TienCoc { get; set; }
        public string TrangThai { get; set; }
    }
}
