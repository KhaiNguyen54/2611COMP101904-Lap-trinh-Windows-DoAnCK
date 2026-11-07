using System;
using QuanLyKhachSan.Models;
using QuanLyKhachSan.Services;
using QuanLyKhachSan.Views;

namespace QuanLyKhachSan.Presenters
{
    /// <summary>Điều phối hộp thoại đổi mật khẩu của người đang đăng nhập.</summary>
    public class DoiMatKhauPresenter
    {
        private readonly IDoiMatKhauView _view;
        private readonly INguoiDungService _nguoiDung;
        private readonly NguoiDung _nguoiHienTai;

        public DoiMatKhauPresenter(IDoiMatKhauView view, INguoiDungService nguoiDung, NguoiDung nguoiHienTai)
        {
            _view = view ?? throw new ArgumentNullException(nameof(view));
            _nguoiDung = nguoiDung ?? throw new ArgumentNullException(nameof(nguoiDung));
            _nguoiHienTai = nguoiHienTai ?? throw new ArgumentNullException(nameof(nguoiHienTai));
        }

        public void Luu()
        {
            if (_view.MatKhauMoi != _view.XacNhanMatKhau)
            {
                _view.HienThongBao("Kiểm tra dữ liệu", "Mật khẩu xác nhận không khớp.", MucThongBao.CanhBao);
                return;
            }
            try
            {
                _nguoiDung.DoiMatKhau(_nguoiHienTai, _view.MatKhauCu, _view.MatKhauMoi);
                _view.HienThongBao("Thành công", "Đã đổi mật khẩu.", MucThongBao.ThanhCong);
                _view.Dong();
            }
            catch (Exception ex) { _view.HienThongBao("Không thể đổi mật khẩu", ex.Message, MucThongBao.CanhBao); }
        }
    }
}
