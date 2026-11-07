using System;
using System.Data;
using QuanLyKhachSan.Models;

namespace QuanLyKhachSan.Services
{
    /// <summary>Phần nghiệp vụ mà Presenter cần; cho phép thay bằng bản giả khi kiểm thử.</summary>
    public interface ILoaiPhongService
    {
        DataTable TimKiem(string tuKhoa = null);
        int Them(LoaiPhong x);
        int CapNhat(LoaiPhong x);
        int NgungSuDung(int ma);
    }
}
