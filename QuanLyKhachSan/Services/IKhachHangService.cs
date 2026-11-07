using System;
using System.Data;
using QuanLyKhachSan.Models;

namespace QuanLyKhachSan.Services
{
    /// <summary>Phần nghiệp vụ mà Presenter cần; cho phép thay bằng bản giả khi kiểm thử.</summary>
    public interface IKhachHangService
    {
        DataTable TimKiem(string tuKhoa = null);
    }
}
