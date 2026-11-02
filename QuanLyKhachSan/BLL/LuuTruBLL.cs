using System;
using System.Data;
using QuanLyKhachSan.DAL;
using QuanLyKhachSan.DTO;

namespace QuanLyKhachSan.BLL
{
    /// <summary>Nghiệp vụ lưu trú: nhận phòng, ghi dịch vụ, trả phòng.</summary>
    public class LuuTruBLL
    {
        private readonly PhieuThueDAL _phieuThueDal = new PhieuThueDAL();
        private readonly ChiTietDichVuDAL _dichVuDal = new ChiTietDichVuDAL();
        private readonly HoaDonBLL _hoaDonBll = new HoaDonBLL();

        public int NhanPhong(int maPhong, int maKhach, int maNguoiDung, DateTime ngayDiDuKien, int? maDatPhong = null)
        {
            if (maPhong <= 0 || maKhach <= 0)
                throw new ArgumentException("Chọn khách hàng và phòng.");
            if (ngayDiDuKien.Date <= DateTime.Today)
                throw new ArgumentException("Ngày đi dự kiến phải sau ngày nhận.");
            return _phieuThueDal.NhanPhong(maPhong, maKhach, maNguoiDung, ngayDiDuKien, maDatPhong);
        }

        public void GhiDichVu(int maPhieuThue, int maDichVu, decimal soLuong)
        {
            if (maPhieuThue <= 0 || maDichVu <= 0)
                throw new ArgumentException("Chọn phiếu thuê và dịch vụ.");
            if (soLuong <= 0)
                throw new ArgumentException("Số lượng phải lớn hơn 0.");

            int count = _dichVuDal.Them(new ChiTietDichVu
            {
                MaPhieuThue = maPhieuThue,
                MaDichVu = maDichVu,
                SoLuong = soLuong
            });
            if (count != 1)
                throw new InvalidOperationException("Phiếu thuê đã trả hoặc dịch vụ không còn hoạt động.");
        }

        /// <summary>Xóa dòng dịch vụ ghi nhầm (chỉ khi phiếu thuê chưa trả, QT32).</summary>
        public void XoaDichVu(int maChiTietDichVu)
        {
            if (maChiTietDichVu <= 0)
                throw new ArgumentException("Chọn dòng dịch vụ cần xóa.");
            if (_dichVuDal.Xoa(maChiTietDichVu) != 1)
                throw new InvalidOperationException("Không xóa được: dòng không tồn tại hoặc phiếu thuê đã trả.");
        }

        public DataTable LayDichVu(int maPhieuThue)
        {
            return _dichVuDal.LayTheoPhieuThue(maPhieuThue);
        }

        public DataTable LayPhieuDangO()
        {
            return _phieuThueDal.LayPhieuDangO();
        }

        public DataTable LayKhachDangO()
        {
            return _phieuThueDal.LayKhachDangO();
        }

        public BaoGiaHoaDon TinhTamTinh(int maPhieuThue, DateTime ngayTra)
        {
            return _hoaDonBll.TinhTamTinh(maPhieuThue, ngayTra);
        }

        public int TraPhong(int maPhieuThue, int maNguoiDung, DateTime ngayTra, decimal giamGia,
            string hinhThuc, bool laQuanLy)
        {
            return _hoaDonBll.TraPhong(maPhieuThue, maNguoiDung, ngayTra, giamGia, hinhThuc, laQuanLy);
        }
    }
}
