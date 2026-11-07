using System;
using System.Collections.Generic;
using System.Data;
using QuanLyKhachSan.Models;
using QuanLyKhachSan.Services;

namespace QuanLyKhachSan.Views
{
    /// <summary>View của màn hình đổi phòng.</summary>
    public interface IDoiPhongView : IThongBaoView
    {
        int? PhieuChon { get; }
        int? PhongMoiChon { get; }
        string LyDo { get; }

        void HienPhieu(DataTable phieu);
        void HienPhongMoi(DataTable phong);
        void HienLichSu(DataTable lichSu);
        void DatLoiNhac(string noiDung);
        void XoaLyDo();
    }
}
