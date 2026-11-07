using System;
using System.Collections.Generic;
using System.Data;
using QuanLyKhachSan.Models;
using QuanLyKhachSan.Services;
using QuanLyKhachSan.Views;

namespace QuanLyKhachSan.Presenters
{
    /// <summary>Xác thực tài khoản Quản lý duyệt mức giảm giá trên 10% (QT44).</summary>
    public class DuyetGiamGiaPresenter
    {
        private readonly IDuyetGiamGiaView _view;
        private readonly ILuuTruService _luuTru;

        public DuyetGiamGiaPresenter(IDuyetGiamGiaView view, ILuuTruService luuTru)
        {
            _view = view ?? throw new ArgumentNullException(nameof(view));
            _luuTru = luuTru ?? throw new ArgumentNullException(nameof(luuTru));
        }

        public void Duyet()
        {
            try
            {
                int ma = _luuTru.XacThucNguoiDuyet(_view.TenDangNhap, _view.MatKhau);
                _view.DongThanhCong(ma);
            }
            catch (Exception ex)
            {
                _view.HienThongBao("Không thể duyệt", ex.Message, MucThongBao.CanhBao);
                _view.XoaMatKhau();
            }
        }
    }
}
