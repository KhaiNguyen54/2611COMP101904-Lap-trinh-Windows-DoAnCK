using System;
using System.Collections.Generic;
using System.Data;
using QuanLyKhachSan.Models;
using QuanLyKhachSan.Services;
using QuanLyKhachSan.Views;

namespace QuanLyKhachSan.Presenters
{
    /// <summary>Điều phối đặt phòng: tải dữ liệu, kiểm tra nhập, lập/hủy đặt phòng, đánh dấu khách không đến (NV01, NV02).</summary>
    public class DatPhongPresenter
    {
        private readonly IDatPhongView _view;
        private readonly IDatPhongService _datPhong;
        private readonly IKhachHangService _khach;
        private readonly IPhongService _phong;
        private readonly NguoiDung _nguoiDung;
        private bool _dangTai;
        private int _soPhongTrong;

        public DatPhongPresenter(IDatPhongView view, IDatPhongService datPhong, IKhachHangService khach, IPhongService phong, NguoiDung nguoiDung)
        {
            _view = view ?? throw new ArgumentNullException(nameof(view));
            _datPhong = datPhong ?? throw new ArgumentNullException(nameof(datPhong));
            _khach = khach ?? throw new ArgumentNullException(nameof(khach));
            _phong = phong ?? throw new ArgumentNullException(nameof(phong));
            _nguoiDung = nguoiDung ?? throw new ArgumentNullException(nameof(nguoiDung));
        }

        public void TaiBanDau()
        {
            TaiKhach(null);
            TaiPhongTrong();
            TaiDanhSach();
        }

        public void TaiKhach(int? chon)
        {
            try { _view.HienKhach(_khach.TimKiem(), chon); }
            catch (Exception ex) { Loi("Không tải được danh sách khách", ex); }
        }

        public void TaiPhongTrong()
        {
            if (_view.NgayDi.Date <= _view.NgayDen.Date) return;
            try
            {
                DataTable phong = _phong.TimKiemTrongKhoang(_view.NgayDen.Date, _view.NgayDi.Date);
                _soPhongTrong = phong.Rows.Count;
                _view.HienPhongTrong(phong);
                bool co = _soPhongTrong > 0;
                _view.DatChoPhepDat(co);
                if (!co) _view.DatGoiY("Không có phòng trống trong khoảng ngày này.");
                else CapNhatGoiY();
            }
            catch (Exception ex) { Loi("Không tải được phòng trống", ex); }
        }

        public void CapNhatGoiY()
        {
            decimal? gia = _view.GiaPhongChon;
            if (!gia.HasValue) return;
            decimal toiDa = QuyTacTinhTien.TienCocToiDa(gia.Value, _view.NgayDen.Date, _view.NgayDi.Date);
            _view.DatGoiY(string.Format("{0} phòng phù hợp · giá {1:N0} đ/đêm · cọc tối đa {2:N0} đ", _soPhongTrong, gia.Value, toiDa));
        }

        public void TaiDanhSach()
        {
            if (_dangTai) return;
            _dangTai = true;
            try
            {
                string tt = _view.LocTrangThai;
                _view.HienDanhSachDat(_datPhong.TimKiem(null, null, string.IsNullOrEmpty(tt) || tt == "Tất cả" ? null : tt));
            }
            catch (Exception ex) { Loi("Không tải được danh sách đặt phòng", ex); }
            finally { _dangTai = false; }
        }

        public void LamMoi()
        {
            TaiDanhSach();
            TaiPhongTrong();
        }

        public void TaoDatPhong()
        {
            if (!_view.KhachChon.HasValue || !_view.PhongChon.HasValue)
            {
                _view.HienThongBao("Thiếu dữ liệu", "Chọn khách hàng và phòng.", MucThongBao.CanhBao);
                return;
            }
            try
            {
                _datPhong.Them(new DatPhong
                {
                    MaKhachHang = _view.KhachChon.Value,
                    MaPhong = _view.PhongChon.Value,
                    MaNguoiDung = _nguoiDung.MaNguoiDung,
                    NgayDen = _view.NgayDen.Date,
                    NgayDi = _view.NgayDi.Date,
                    TienCoc = _view.TienCoc
                });
                _view.XoaTienCoc();
                TaiDanhSach();
                TaiPhongTrong();
                _view.HienThongBao("Thành công", "Đã tạo đặt phòng.", MucThongBao.ThanhCong);
            }
            catch (Exception ex) { Loi("Không thể đặt phòng", ex); }
        }

        public void Huy()
        {
            int? ma = DongDuocChon();
            if (!ma.HasValue || !_view.XacNhan("Hủy đặt phòng này? Phòng sẽ được giải phóng cho khoảng ngày đó.")) return;
            try { _datPhong.Huy(ma.Value); LamMoi(); }
            catch (Exception ex) { Loi("Không thể hủy", ex); }
        }

        public void KhachKhongDen()
        {
            int? ma = DongDuocChon();
            if (!ma.HasValue || !_view.XacNhan("Đánh dấu khách không đến? Phòng sẽ được giải phóng.")) return;
            try { _datPhong.DanhDauKhongDen(ma.Value); LamMoi(); }
            catch (Exception ex) { Loi("Không thể cập nhật", ex); }
        }

        public void NhanPhongTuDatNay()
        {
            int? ma = DongDuocChon();
            if (!ma.HasValue) return;
            if (_view.TrangThaiDatChon != "Đã đặt")
            {
                _view.HienThongBao("Không thể nhận", "Chỉ đặt phòng ở trạng thái 'Đã đặt' mới nhận phòng được.", MucThongBao.ThongTin);
                return;
            }
            _view.MoNhanPhong(ma.Value);
            LamMoi();
        }

        private int? DongDuocChon()
        {
            int? ma = _view.MaDatPhongChon;
            if (!ma.HasValue) _view.HienThongBao("Chưa chọn", "Hãy chọn một dòng đặt phòng.", MucThongBao.ThongTin);
            return ma;
        }

        private void Loi(string tieuDe, Exception ex)
        {
            _view.HienThongBao(tieuDe, ex.Message, MucThongBao.CanhBao);
        }
    }
}
