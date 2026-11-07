using System;
using System.Collections.Generic;
using System.Data;
using QuanLyKhachSan.Models;
using QuanLyKhachSan.Services;

namespace QuanLyKhachSan.Views
{
    /// <summary>View của màn hình trả phòng và lập hóa đơn.</summary>
    public interface ITraPhongView : IThongBaoView
    {
        int? PhieuChon { get; }
        DateTime NgayTra { get; }
        decimal GiamGia { get; }
        string HinhThuc { get; }

        void HienPhieu(DataTable phieu);
        void HienBaoGia(string soDem, string tienPhong, string tienDichVu, string phuThu, string tienCoc);
        void DatGiamToiDa(decimal toiDa);
        void HienGiamVaTong(string giam, string tong);
        void DatGhiChu(string noiDung);
        void DatChoPhepTra(bool duocTra);
        /// <summary>Hiện hộp thoại để Quản lý duyệt; trả về mã người duyệt hoặc null nếu hủy.</summary>
        int? YeuCauQuanLyDuyet();
        void MoBanIn(int maHoaDon, int maPhieu);
        void DongThanhCong();
    }
}
