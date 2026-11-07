using System;
using System.Collections.Generic;
using System.Data;
using QuanLyKhachSan.Models;
using QuanLyKhachSan.Services;

namespace QuanLyKhachSan.Views
{
    /// <summary>View của màn hình đặt phòng.</summary>
    public interface IDatPhongView : IThongBaoView
    {
        int? KhachChon { get; }
        int? PhongChon { get; }
        decimal? GiaPhongChon { get; }
        DateTime NgayDen { get; }
        DateTime NgayDi { get; }
        decimal TienCoc { get; }
        string LocTrangThai { get; }
        int? MaDatPhongChon { get; }
        string TrangThaiDatChon { get; }

        void HienKhach(DataTable khach, int? chon);
        void HienPhongTrong(DataTable phong);
        void HienDanhSachDat(DataTable dat);
        void DatChoPhepDat(bool duocDat);
        void DatGoiY(string noiDung);
        void XoaTienCoc();
        void MoNhanPhong(int maDatPhong);
    }
}
