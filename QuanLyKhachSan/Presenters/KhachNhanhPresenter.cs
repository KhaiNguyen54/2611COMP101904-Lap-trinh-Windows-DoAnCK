using System;
using System.Data;
using QuanLyKhachSan.Models;
using QuanLyKhachSan.Services;
using QuanLyKhachSan.Views;

namespace QuanLyKhachSan.Presenters
{
    /// <summary>Điều phối hộp thoại thêm nhanh khách hàng tại quầy.</summary>
    public class KhachNhanhPresenter
    {
        private readonly IKhachNhanhView _view;
        private readonly IKhachHangService _khach;

        public KhachNhanhPresenter(IKhachNhanhView view, IKhachHangService khach)
        {
            _view = view ?? throw new ArgumentNullException(nameof(view));
            _khach = khach ?? throw new ArgumentNullException(nameof(khach));
        }

        public void Luu()
        {
            try
            {
                var khach = new KhachHang { HoTen = _view.HoTen, SoGiayTo = _view.SoGiayTo, SoDienThoai = _view.SoDienThoai, Email = _view.Email, DiaChi = _view.DiaChi };
                _khach.Them(khach);
                DataTable kq = _khach.TimKiem((_view.SoGiayTo ?? string.Empty).Trim());
                int ma = kq.Rows.Count > 0 ? Convert.ToInt32(kq.Rows[0]["MaKhachHang"]) : 0;
                _view.DongThanhCong(ma);
            }
            catch (Exception ex) { _view.HienThongBao("Không thể lưu khách hàng", ex.Message, MucThongBao.CanhBao); }
        }
    }
}
