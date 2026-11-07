using System;
using System.Collections.Generic;
using System.Data;
using QuanLyKhachSan.Models;
using QuanLyKhachSan.Services;

namespace QuanLyKhachSan.Views
{
    /// <summary>View của màn hình nhận phòng.</summary>
    public interface INhanPhongView : IThongBaoView
    {
        bool CheDoTuDat { get; }
        int? MaDatPhongChon { get; }
        int? KhachChon { get; }
        int? PhongChon { get; }
        DateTime NgayDiDuKien { get; }

        void HienDatPhong(DataTable dat, int? chon);
        void HienKhach(DataTable khach, int? chon);
        void HienPhongTrong(DataTable phong);
        void ChuyenSangVangLai();
        void DatCheDo(bool tuDat);
        void DatNgayDi(DateTime ngay);
        void DatThongTin(string noiDung);
        void DatChoPhepNhan(bool duocNhan);
        void DongThanhCong();
    }
}
