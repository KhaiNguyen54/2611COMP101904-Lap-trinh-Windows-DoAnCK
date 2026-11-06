using System;

namespace QuanLyKhachSan.Models
{
    public class HoaDon
    {
        public int MaHoaDon { get; set; }
        public int MaPhieuThue { get; set; }
        public int MaNguoiDung { get; set; }
        public DateTime NgayLap { get; set; }
        public int SoDem { get; set; }
        public decimal TienPhong { get; set; }
        public decimal TienDichVu { get; set; }
        public decimal PhuThu { get; set; }
        public decimal GiamGia { get; set; }
        public decimal TienCoc { get; set; }
        public decimal TongThanhToan { get; set; }
        public string HinhThucThanhToan { get; set; }
    }
}
