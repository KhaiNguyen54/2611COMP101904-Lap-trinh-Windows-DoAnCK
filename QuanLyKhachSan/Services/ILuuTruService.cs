using System;
using System.Data;
using QuanLyKhachSan.Models;

namespace QuanLyKhachSan.Services
{
    /// <summary>Phần nghiệp vụ mà Presenter cần; cho phép thay bằng bản giả khi kiểm thử.</summary>
    public interface ILuuTruService
    {
        int NhanPhong(int maPhong, int maKhach, int maNguoiDung, DateTime ngayDiDuKien, int? maDatPhong = null);
        void GhiDichVu(int maPhieuThue, int maDichVu, decimal soLuong);
        void XoaDichVu(int maChiTietDichVu);
        DataTable LayDichVu(int maPhieuThue);
        DataTable LayPhieuDangO();
        DataTable LayKhachDangO();
        DataTable LayPhongCoTheDoi(int maPhongHienTai, DateTime ngayDiDuKien);
        void DoiPhong(int maPhieuThue, int maPhongMoi, int maNguoiDung, string lyDo);
        DataTable LayLichSuDoiPhong(int maPhieuThue);
        BaoGiaHoaDon TinhTamTinh(int maPhieuThue, DateTime ngayTra);
        int TraPhong(int maPhieuThue, int maNguoiDung, DateTime ngayTra, decimal giamGia, string hinhThuc, bool laQuanLy, int? maNguoiDuyet = null);
        int XacThucNguoiDuyet(string tenDangNhap, string matKhau);
    }
}
