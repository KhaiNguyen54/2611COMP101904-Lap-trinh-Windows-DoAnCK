using System;
using System.Collections.Generic;
using System.Data;
using QuanLyKhachSan.Models;
using QuanLyKhachSan.Services;

namespace QuanLyKhachSan.Views
{
    /// <summary>View của màn hình ghi dịch vụ.</summary>
    public interface IGhiDichVuView : IThongBaoView
    {
        int? PhieuChon { get; }
        int? DichVuChon { get; }
        decimal SoLuong { get; }
        int? ChiTietChon { get; }

        void HienPhieu(DataTable phieu);
        void HienDichVu(DataTable dichVu);
        void HienChiTiet(DataTable chiTiet);
        void DatTong(string noiDung);
        void DatLaiSoLuong();
    }
}
