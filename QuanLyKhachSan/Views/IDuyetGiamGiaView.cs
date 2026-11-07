using System;
using System.Collections.Generic;
using System.Data;
using QuanLyKhachSan.Models;
using QuanLyKhachSan.Services;

namespace QuanLyKhachSan.Views
{
    /// <summary>View của hộp thoại Quản lý duyệt giảm giá.</summary>
    public interface IDuyetGiamGiaView : IThongBaoView
    {
        string TenDangNhap { get; }
        string MatKhau { get; }
        void XoaMatKhau();
        void DongThanhCong(int maNguoiDuyet);
    }
}
