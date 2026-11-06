namespace QuanLyKhachSan.Models
{
    public class BaoGiaHoaDon
    {
        public int SoDem { get; set; }
        public decimal TienPhong { get; set; }
        public decimal TienDichVu { get; set; }
        public decimal PhuThu { get; set; }
        public decimal TienCoc { get; set; }
        public decimal TamTinh { get { return TienPhong + TienDichVu + PhuThu - TienCoc; } }
    }
}
