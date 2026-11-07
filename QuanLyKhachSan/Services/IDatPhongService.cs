using System;
using System.Data;
using QuanLyKhachSan.Models;

namespace QuanLyKhachSan.Services
{
    /// <summary>Phần nghiệp vụ mà Presenter cần; cho phép thay bằng bản giả khi kiểm thử.</summary>
    public interface IDatPhongService
    {
        DataTable TimKiem(DateTime? tuNgay = null, DateTime? denNgay = null, string trangThai = null);
        DataTable LayDangDat();
        int Them(DatPhong x);
        void Huy(int maDatPhong);
        void DanhDauKhongDen(int maDatPhong);
    }
}
