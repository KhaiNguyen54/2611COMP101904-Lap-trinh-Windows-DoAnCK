using System;
using System.Data;
using QuanLyKhachSan.Models;

namespace QuanLyKhachSan.Services
{
    /// <summary>Phần nghiệp vụ mà Presenter cần; cho phép thay bằng bản giả khi kiểm thử.</summary>
    public interface IHoaDonService
    {
        DataTable TimKiem(DateTime? tuNgay = null, DateTime? denNgay = null, string tuKhoa = null);
    }
}
