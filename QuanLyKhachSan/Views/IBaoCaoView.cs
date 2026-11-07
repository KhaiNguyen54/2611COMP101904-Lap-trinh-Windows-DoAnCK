using System;
using System.Collections.Generic;
using System.Data;
using QuanLyKhachSan.Models;
using QuanLyKhachSan.Services;

namespace QuanLyKhachSan.Views
{
    /// <summary>View của màn hình báo cáo doanh thu và công suất.</summary>
    public interface IBaoCaoView : IThongBaoView
    {
        DateTime DoanhThuTu { get; }
        DateTime DoanhThuDen { get; }
        KieuGomDoanhThu KieuGom { get; }
        DateTime CongSuatTu { get; }
        DateTime CongSuatDen { get; }

        void HienDoanhThu(DataTable bang, IList<string> nhan, IList<double> giaTri, string tong);
        void HienCongSuat(DataTable bang, IList<string> nhan, IList<double> giaTri, string tong);
    }
}
