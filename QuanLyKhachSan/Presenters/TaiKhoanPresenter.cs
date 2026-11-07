using System;
using QuanLyKhachSan.Models;
using QuanLyKhachSan.Services;
using QuanLyKhachSan.Views;

namespace QuanLyKhachSan.Presenters
{
    /// <summary>Điều phối màn hình quản lý tài khoản: tạo, đặt lại mật khẩu, khóa.</summary>
    public class TaiKhoanPresenter
    {
        private readonly ITaiKhoanView _view;
        private readonly INguoiDungService _nguoiDung;
        private readonly NguoiDung _nguoiQuanLy;

        public TaiKhoanPresenter(ITaiKhoanView view, INguoiDungService nguoiDung, NguoiDung nguoiQuanLy)
        {
            _view = view ?? throw new ArgumentNullException(nameof(view));
            _nguoiDung = nguoiDung ?? throw new ArgumentNullException(nameof(nguoiDung));
            _nguoiQuanLy = nguoiQuanLy;
        }

        public void TaiDuLieu()
        {
            try { _view.HienDanhSach(_nguoiDung.LayDanhSach(_nguoiQuanLy)); }
            catch (Exception ex) { _view.HienThongBao("Không tải được danh sách tài khoản", ex.Message, MucThongBao.CanhBao); }
        }

        public void Tao()
        {
            try
            {
                _nguoiDung.TaoTaiKhoan(_nguoiQuanLy, _view.TenDangNhap, _view.HoTen, _view.VaiTro, _view.MatKhau);
                _view.XoaFormTao();
                TaiDuLieu();
                _view.HienThongBao("Thành công", "Đã tạo tài khoản.", MucThongBao.ThanhCong);
            }
            catch (Exception ex) { _view.HienThongBao("Không thể tạo tài khoản", ex.Message, MucThongBao.CanhBao); }
        }

        public void DatLaiMatKhau()
        {
            try
            {
                _nguoiDung.DatLaiMatKhau(_nguoiQuanLy, _view.MaDangChon, _view.MatKhau);
                _view.XoaMatKhau();
                _view.HienThongBao("Thành công", "Đã đặt lại mật khẩu.", MucThongBao.ThanhCong);
            }
            catch (Exception ex) { _view.HienThongBao("Không thể đặt lại mật khẩu", ex.Message, MucThongBao.CanhBao); }
        }

        public void Khoa()
        {
            try
            {
                int ma = _view.MaDangChon;
                if (ma == 0 || !_view.XacNhan("Khóa tài khoản đang chọn?")) return;
                _nguoiDung.KhoaTaiKhoan(_nguoiQuanLy, ma);
                TaiDuLieu();
            }
            catch (Exception ex) { _view.HienThongBao("Không thể khóa tài khoản", ex.Message, MucThongBao.CanhBao); }
        }
    }
}
