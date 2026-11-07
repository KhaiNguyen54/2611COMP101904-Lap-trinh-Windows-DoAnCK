using System;
using System.Data;
using QuanLyKhachSan.Repositories;
using QuanLyKhachSan.Models;

namespace QuanLyKhachSan.Services
{
    /// <summary>Nghiệp vụ lưu trú: nhận phòng, ghi dịch vụ, trả phòng.</summary>
    public class LuuTruService : ILuuTruService
    {
        private readonly PhieuThueRepository _phieuThueDal = new PhieuThueRepository();
        private readonly ChiTietDichVuRepository _dichVuDal = new ChiTietDichVuRepository();
        private readonly HoaDonService _hoaDonBll = new HoaDonService();

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

        /// <summary>
        /// Các phòng có thể đổi sang (trống trọn khoảng còn lại của kỳ lưu trú, QT47), không gồm phòng hiện tại.
        /// </summary>
        public DataTable LayPhongCoTheDoi(int maPhongHienTai, DateTime ngayDiDuKien)
        {
            DateTime den, di;
            QuyTacDoiPhong.KhoangCanTrong(DateTime.Today, ngayDiDuKien, out den, out di);
            DataTable t = new PhongService().TimKiemTrongKhoang(den, di);
            for (int i = t.Rows.Count - 1; i >= 0; i--)
                if (Convert.ToInt32(t.Rows[i]["MaPhong"]) == maPhongHienTai || Convert.ToString(t.Rows[i]["TrangThai"]) == "Đang ở") t.Rows.RemoveAt(i);
            return t;
        }

        /// <summary>Đổi phòng khi khách đang ở (NV08, QT47–QT49).</summary>
        public void DoiPhong(int maPhieuThue, int maPhongMoi, int maNguoiDung, string lyDo)
        {
            if (maPhieuThue <= 0)
                throw new ArgumentException("Chọn phiếu thuê đang ở.");
            if (maPhongMoi <= 0)
                throw new ArgumentException("Chọn phòng mới.");
            _phieuThueDal.DoiPhong(maPhieuThue, maPhongMoi, maNguoiDung, QuyTacDoiPhong.ChuanHoaLyDo(lyDo));
        }

        public DataTable LayLichSuDoiPhong(int maPhieuThue)
        {
            return _phieuThueDal.LayLichSuDoiPhong(maPhieuThue);
        }

        public BaoGiaHoaDon TinhTamTinh(int maPhieuThue, DateTime ngayTra)
        {
            return _hoaDonBll.TinhTamTinh(maPhieuThue, ngayTra);
        }

        public int TraPhong(int maPhieuThue, int maNguoiDung, DateTime ngayTra, decimal giamGia,
            string hinhThuc, bool laQuanLy, int? maNguoiDuyet = null)
        {
            return _hoaDonBll.TraPhong(maPhieuThue, maNguoiDung, ngayTra, giamGia, hinhThuc, laQuanLy, maNguoiDuyet);
        }

        public int XacThucNguoiDuyet(string tenDangNhap, string matKhau)
        {
            return _hoaDonBll.XacThucNguoiDuyet(tenDangNhap, matKhau);
        }
    }
}
