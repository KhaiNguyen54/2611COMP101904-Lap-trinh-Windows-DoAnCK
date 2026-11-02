using System;

namespace QuanLyKhachSan.DTO
{
    public class ChiTietDichVu
    {
        public int MaChiTietDichVu { get; set; }
        public int MaPhieuThue { get; set; }
        public int MaDichVu { get; set; }
        public decimal SoLuong { get; set; }
        public decimal DonGia { get; set; }
        public DateTime ThoiDiemSuDung { get; set; }
        public decimal ThanhTien { get { return SoLuong * DonGia; } }
    }
}
