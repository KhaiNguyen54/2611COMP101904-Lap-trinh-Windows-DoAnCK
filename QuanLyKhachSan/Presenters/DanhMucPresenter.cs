using System;
using System.Data;
using QuanLyKhachSan.Models;
using QuanLyKhachSan.Services;
using QuanLyKhachSan.Views;

namespace QuanLyKhachSan.Presenters
{
    /// <summary>
    /// Điều phối màn hình danh mục dùng chung. Thứ tự ô nhập theo từng loại:
    /// Loại phòng (tên, mô tả, giá, sức chứa); Phòng (số phòng, tầng, loại phòng, ghi chú);
    /// Khách hàng (họ tên, giấy tờ, điện thoại, email, địa chỉ); Dịch vụ (tên, đơn giá, đơn vị).
    /// </summary>
    public class DanhMucPresenter
    {
        private readonly IDanhMucView _view;
        private readonly LoaiDanhMuc _loai;
        private readonly ILoaiPhongService _loaiPhong;
        private readonly IPhongService _phong;
        private readonly IKhachHangService _khach;
        private readonly IDichVuService _dichVu;
        private readonly Func<DateTime> _gio;

        public DanhMucPresenter(IDanhMucView view, LoaiDanhMuc loai, ILoaiPhongService loaiPhong, IPhongService phong,
            IKhachHangService khach, IDichVuService dichVu, Func<DateTime> gio = null)
        {
            _view = view ?? throw new ArgumentNullException(nameof(view));
            _loai = loai;
            _loaiPhong = loaiPhong;
            _phong = phong;
            _khach = khach;
            _dichVu = dichVu;
            _gio = gio ?? (() => DateTime.Now);
        }

        /// <summary>Lễ tân chỉ được sửa Khách hàng; các danh mục còn lại chỉ Quản lý được sửa (docs/04).</summary>
        public static bool ChiXem(LoaiDanhMuc loai, NguoiDung nguoiDung)
        {
            return loai != LoaiDanhMuc.KhachHang && (nguoiDung == null || nguoiDung.VaiTro != VaiTroNguoiDung.QuanLy);
        }

        public static string Ten(LoaiDanhMuc loai)
        {
            switch (loai)
            {
                case LoaiDanhMuc.LoaiPhong: return "Loại phòng";
                case LoaiDanhMuc.Phong: return "Phòng";
                case LoaiDanhMuc.KhachHang: return "Khách hàng";
                default: return "Dịch vụ";
            }
        }

        public void TaiBanDau()
        {
            NapLoaiPhong();
            TaiDuLieu();
        }

        private void NapLoaiPhong()
        {
            if (_loai != LoaiDanhMuc.Phong) return;
            try
            {
                DataTable ds = _loaiPhong.TimKiem();
                ds.DefaultView.RowFilter = "HoatDong = true";
                _view.HienLoaiPhong(ds);
            }
            catch (Exception ex) { _view.HienThongBao("Không tải được loại phòng", ex.Message, MucThongBao.CanhBao); }
        }

        public void TaiDuLieu()
        {
            try
            {
                string tuKhoa = _view.TuKhoa;
                DataTable t;
                switch (_loai)
                {
                    case LoaiDanhMuc.LoaiPhong: t = _loaiPhong.TimKiem(tuKhoa); break;
                    case LoaiDanhMuc.Phong: t = _phong.TimKiem(tuKhoa); break;
                    case LoaiDanhMuc.KhachHang: t = _khach.TimKiem(tuKhoa); break;
                    default: t = _dichVu.TimKiem(tuKhoa); break;
                }
                _view.HienDanhSach(t);
            }
            catch (Exception ex) { _view.HienThongBao("Không tải được dữ liệu", ex.Message, MucThongBao.CanhBao); }
        }

        public void Luu()
        {
            try
            {
                int ma = _view.MaDangSua;
                switch (_loai)
                {
                    case LoaiDanhMuc.LoaiPhong:
                        {
                            var x = new LoaiPhong { MaLoaiPhong = ma, TenLoaiPhong = _view.DocChu(0), MoTa = _view.DocChu(1), GiaMoiDem = _view.DocSo(2), SucChua = (int)_view.DocSo(3) };
                            if (ma == 0) _loaiPhong.Them(x); else _loaiPhong.CapNhat(x);
                            break;
                        }
                    case LoaiDanhMuc.Phong:
                        {
                            int? maLoai = _view.DocChon(2);
                            if (!maLoai.HasValue) throw new ArgumentException("Chọn loại phòng.");
                            var x = new Phong { MaPhong = ma, SoPhong = _view.DocChu(0), Tang = (int)_view.DocSo(1), MaLoaiPhong = maLoai.Value, GhiChu = _view.DocChu(3) };
                            if (ma == 0) _phong.Them(x); else _phong.CapNhat(x);
                            break;
                        }
                    case LoaiDanhMuc.KhachHang:
                        {
                            var x = new KhachHang { MaKhachHang = ma, HoTen = _view.DocChu(0), SoGiayTo = _view.DocChu(1), SoDienThoai = _view.DocChu(2), Email = _view.DocChu(3), DiaChi = _view.DocChu(4) };
                            if (ma == 0) _khach.Them(x); else _khach.CapNhat(x);
                            break;
                        }
                    default:
                        {
                            var x = new DichVu { MaDichVu = ma, TenDichVu = _view.DocChu(0), DonGia = _view.DocSo(1), DonViTinh = _view.DocChu(2) };
                            if (ma == 0) _dichVu.Them(x); else _dichVu.CapNhat(x);
                            break;
                        }
                }
                _view.LamMoiFormNhap();
                TaiDuLieu();
                _view.DatTrangThai("Đã lưu lúc " + _gio().ToString("HH:mm:ss"));
            }
            catch (Exception ex) { _view.HienThongBao("Không thể lưu", ex.Message, MucThongBao.CanhBao); }
        }

        public void Ngung()
        {
            int ma = _view.MaDangSua;
            if (ma == 0)
            {
                _view.HienThongBao("Chưa chọn", "Chọn một dòng cần ngừng sử dụng.", MucThongBao.ThongTin);
                return;
            }
            if (!_view.XacNhan("Ngừng sử dụng bản ghi đã chọn?")) return;
            try
            {
                int kq = _loai == LoaiDanhMuc.LoaiPhong ? _loaiPhong.NgungSuDung(ma)
                       : _loai == LoaiDanhMuc.Phong ? _phong.NgungSuDung(ma)
                       : _dichVu.NgungSuDung(ma);
                if (kq == 0)
                    _view.HienThongBao("Không thể ngừng", "Không thể ngừng: bản ghi đang được sử dụng (còn phòng/đặt phòng/khách liên quan) hoặc không tồn tại.", MucThongBao.ThongTin);
                _view.LamMoiFormNhap();
                TaiDuLieu();
            }
            catch (Exception ex) { _view.HienThongBao("Không thể ngừng sử dụng", ex.Message, MucThongBao.CanhBao); }
        }
    }
}
