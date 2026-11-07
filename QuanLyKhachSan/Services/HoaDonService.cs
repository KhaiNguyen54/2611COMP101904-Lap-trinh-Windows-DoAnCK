using System;
using System.Data;
using QuanLyKhachSan.Repositories;
using QuanLyKhachSan.Models;

namespace QuanLyKhachSan.Services
{
    public class HoaDonService : IHoaDonService
    {
        private readonly HoaDonRepository _dal = new HoaDonRepository();

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

        /// <summary>Xác thực tài khoản Quản lý duyệt giảm giá; trả về mã người duyệt.</summary>
        public int XacThucNguoiDuyet(string tenDangNhap, string matKhau)
        {
            NguoiDung nguoi = new NguoiDungService().DangNhap(tenDangNhap, matKhau);
            if (!NguoiDungService.CoQuyenQuanLy(nguoi))
                throw new UnauthorizedAccessException("Tài khoản không đúng hoặc không phải Quản lý.");
            return nguoi.MaNguoiDung;
        }

        /// <summary>Trả phòng và lập hóa đơn (một transaction trong sp_TraPhong). Trả về mã hóa đơn.</summary>
        public int TraPhong(int maPhieuThue, int maNguoiDung, DateTime ngayTra, decimal giamGia,
            string hinhThuc, bool laQuanLy, int? maNguoiDuyet = null)
        {
            BaoGiaHoaDon baoGia = TinhTamTinh(maPhieuThue, ngayTra);
            decimal coSoGiam = QuyTacTinhTien.CoSoGiamGia(baoGia.TienPhong, baoGia.TienDichVu, baoGia.PhuThu);

            if (giamGia < 0 || giamGia > coSoGiam)
                throw new ArgumentException("Giảm giá không hợp lệ.");
            int? nguoiDuyet = null;
            if (QuyTacTinhTien.CanQuanLyDuyet(giamGia, coSoGiam))
            {
                if (laQuanLy) nguoiDuyet = maNguoiDung;
                else if (maNguoiDuyet.HasValue) nguoiDuyet = maNguoiDuyet;
                else throw new UnauthorizedAccessException("Giảm giá trên 10% cần Quản lý duyệt.");
            }
            if (string.IsNullOrWhiteSpace(hinhThuc))
                throw new ArgumentException("Chọn hình thức thanh toán.");

            return _dal.TraPhong(maPhieuThue, maNguoiDung, ngayTra, baoGia.PhuThu, giamGia, hinhThuc, nguoiDuyet);
        }
    }
}
