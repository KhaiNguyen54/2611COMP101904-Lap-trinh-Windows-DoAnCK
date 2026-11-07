using System;
using System.Threading;
using System.Threading.Tasks;
using QuanLyKhachSan.Models;
using QuanLyKhachSan.Services;
using QuanLyKhachSan.Views;

namespace QuanLyKhachSan.Presenters
{
    /// <summary>
    /// Điều phối đăng nhập: kiểm tra dữ liệu nhập, tạm khóa sau nhiều lần sai, gọi Service và
    /// yêu cầu View hiển thị kết quả. Không chứa mã WinForms nên kiểm thử được mà không mở giao diện.
    /// </summary>
    public class DangNhapPresenter
    {
        public const int SoLanSaiToiDa = 5;
        public const int GiayKhoa = 30;

        private readonly IDangNhapView _view;
        private readonly INguoiDungService _service;
        private readonly Func<DateTime> _bayGio;
        private int _soLanSai;
        private DateTime _khoaDen = DateTime.MinValue;

        public DangNhapPresenter(IDangNhapView view, INguoiDungService service, Func<DateTime> bayGio = null)
        {
            _view = view ?? throw new ArgumentNullException(nameof(view));
            _service = service ?? throw new ArgumentNullException(nameof(service));
            _bayGio = bayGio ?? (() => DateTime.Now);
        }

        public async Task DangNhapAsync(CancellationToken huy)
        {
            if (string.IsNullOrWhiteSpace(_view.TenDangNhap) || string.IsNullOrEmpty(_view.MatKhau))
            {
                _view.HienThongBao("Thiếu dữ liệu", "Vui lòng nhập tên đăng nhập và mật khẩu.", MucThongBao.CanhBao);
                return;
            }

            DateTime bay = _bayGio();
            if (bay < _khoaDen)
            {
                int giay = (int)Math.Ceiling((_khoaDen - bay).TotalSeconds);
                _view.HienThongBao("Tạm khóa đăng nhập", "Đã nhập sai quá nhiều lần. Vui lòng thử lại sau " + giay + " giây.", MucThongBao.CanhBao);
                return;
            }

            _view.DatDangXuLy(true);
            try
            {
                NguoiDung nguoi = await _service.DangNhapAsync(_view.TenDangNhap, _view.MatKhau, huy);
                if (huy.IsCancellationRequested) return;
                if (nguoi == null)
                {
                    if (++_soLanSai >= SoLanSaiToiDa)
                    {
                        _soLanSai = 0;
                        _khoaDen = _bayGio().AddSeconds(GiayKhoa);
                    }
                    _view.HienThongBao("Không thể đăng nhập", "Tên đăng nhập hoặc mật khẩu không đúng, hoặc tài khoản đã bị khóa.", MucThongBao.CanhBao);
                    _view.XoaMatKhau();
                    return;
                }
                _soLanSai = 0;
                _view.DongThanhCong(nguoi);
            }
            catch (Exception ex)
            {
                if (huy.IsCancellationRequested) return;
                _view.HienThongBao("Lỗi đăng nhập", "Không thể đăng nhập. Hãy kiểm tra cấu hình và kết nối cơ sở dữ liệu.\n" + ex.Message, MucThongBao.Loi);
            }
            finally
            {
                if (!huy.IsCancellationRequested) _view.DatDangXuLy(false);
            }
        }
    }
}
