using System;

namespace QuanLyKhachSan.Models
{
    public class PhieuThue
    {
        public int MaPhieuThue { get; set; }
        public int? MaDatPhong { get; set; }
        public int MaKhachHang { get; set; }
        public int MaPhong { get; set; }
        public int MaNguoiDung { get; set; }
        public DateTime NgayNhan { get; set; }
        public DateTime NgayDiDuKien { get; set; }
        public DateTime? NgayDiThucTe { get; set; }
        public decimal GiaPhongMoiDem { get; set; }
        public string TrangThai { get; set; }
    }
}
