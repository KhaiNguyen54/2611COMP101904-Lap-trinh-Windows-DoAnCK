using System;
using System.Data;
using QuanLyKhachSan.DAL;
using QuanLyKhachSan.DTO;

namespace QuanLyKhachSan.BLL
{
    public class HoaDonBLL
    {
        private readonly HoaDonDAL _dal = new HoaDonDAL();

        public DataTable TimKiem(DateTime? tuNgay = null, DateTime? denNgay = null, string tuKhoa = null)
        {
            return _dal.TimKiem(tuNgay, denNgay, tuKhoa);
        }

        /// <summary>Tính tạm tiền cho phiếu thuê đang ở nếu trả phòng vào thời điểm <paramref name="ngayTra"/>.</summary>
        public BaoGiaHoaDon TinhTamTinh(int maPhieuThue, DateTime ngayTra)
        {
            DataTable dt = _dal.TinhTamTinh(maPhieuThue, ngayTra);
            if (dt.Rows.Count == 0)
                throw new InvalidOperationException("Không tìm thấy phiếu thuê đang ở.");

            DataRow r = dt.Rows[0];
            DateTime ngayNhan = Convert.ToDateTime(r["NgayNhan"]);
            decimal gia = Convert.ToDecimal(r["GiaPhongMoiDem"]);
            int soDem = QuyTacTinhTien.TinhSoDem(ngayNhan, ngayTra);

            return new BaoGiaHoaDon
            {
                SoDem = soDem,
                TienPhong = soDem * gia,
                TienDichVu = Convert.ToDecimal(r["TienDichVu"]),
                PhuThu = QuyTacTinhTien.TinhPhuThuTraMuon(ngayTra, gia),
                TienCoc = Convert.ToDecimal(r["TienCoc"])
            };
        }

        /// <summary>Trả phòng và lập hóa đơn (một transaction trong sp_TraPhong). Trả về mã hóa đơn.</summary>
        public int TraPhong(int maPhieuThue, int maNguoiDung, DateTime ngayTra, decimal giamGia,
            string hinhThuc, bool laQuanLy)
        {
            BaoGiaHoaDon baoGia = TinhTamTinh(maPhieuThue, ngayTra);
            decimal coSoGiam = QuyTacTinhTien.CoSoGiamGia(baoGia.TienPhong, baoGia.TienDichVu, baoGia.PhuThu);

            if (giamGia < 0 || giamGia > coSoGiam)
                throw new ArgumentException("Giảm giá không hợp lệ.");
            if (!laQuanLy && giamGia > coSoGiam * QuyTacTinhTien.TyLeGiamToiDaLeTan)
                throw new UnauthorizedAccessException("Lễ tân chỉ được giảm tối đa 10%; cần quản lý duyệt mức giảm này.");
            if (string.IsNullOrWhiteSpace(hinhThuc))
                throw new ArgumentException("Chọn hình thức thanh toán.");

            return _dal.TraPhong(maPhieuThue, maNguoiDung, ngayTra, baoGia.PhuThu, giamGia, hinhThuc);
        }
    }
}
