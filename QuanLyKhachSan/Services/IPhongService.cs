using System;
using System.Data;
using QuanLyKhachSan.Models;

namespace QuanLyKhachSan.Services
{
    /// <summary>Phần nghiệp vụ mà Presenter cần; cho phép thay bằng bản giả khi kiểm thử.</summary>
    public interface IPhongService
    {
        DataTable TimKiem(string tuKhoa = null, int? tang = null, int? maLoai = null, string trangThai = null);
        DataTable TimKiemTrongKhoang(DateTime ngayDen, DateTime ngayDi);
        int Them(Phong x);
        int CapNhat(Phong x);
        int NgungSuDung(int ma);
    }
}
