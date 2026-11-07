using System;
using System.Collections.Generic;
using System.Data;
using QuanLyKhachSan.Models;
using QuanLyKhachSan.Services;
using QuanLyKhachSan.Views;

namespace QuanLyKhachSan.Presenters
{
    /// <summary>Điều phối nhận phòng từ đặt trước hoặc cho khách vãng lai (NV03).</summary>
    public class NhanPhongPresenter
    {
        private readonly INhanPhongView _view;
        private readonly ILuuTruService _luuTru;
        private readonly IDatPhongService _datPhong;
        private readonly IKhachHangService _khach;
        private readonly IPhongService _phong;
        private readonly NguoiDung _nguoiDung;
        private readonly Func<DateTime> _homNay;
        private DataTable _dsDat;

        public NhanPhongPresenter(INhanPhongView view, ILuuTruService luuTru, IDatPhongService datPhong,
            IKhachHangService khach, IPhongService phong, NguoiDung nguoiDung, Func<DateTime> homNay = null)
        {
            _view = view ?? throw new ArgumentNullException(nameof(view));
            _luuTru = luuTru ?? throw new ArgumentNullException(nameof(luuTru));
            _datPhong = datPhong ?? throw new ArgumentNullException(nameof(datPhong));
            _khach = khach ?? throw new ArgumentNullException(nameof(khach));
            _phong = phong ?? throw new ArgumentNullException(nameof(phong));
            _nguoiDung = nguoiDung ?? throw new ArgumentNullException(nameof(nguoiDung));
            _homNay = homNay ?? (() => DateTime.Today);
        }

        public void KhoiTao(int? maDatPhong)
        {
            TaiDat(maDatPhong);
            TaiKhach(null);
            CapNhatCheDo();
        }

        public void CapNhatCheDo()
        {
            bool dat = _view.CheDoTuDat;
            _view.DatCheDo(dat);
            if (dat) ChonDatPhong();
            else
            {
                TaiPhongTrong();
                _view.DatThongTin("Khách vãng lai: chỉ hiện phòng trống từ hôm nay đến ngày đi dự kiến.");
            }
        }

        public void TaiDat(int? maDatPhong)
        {
            try
            {
                _dsDat = _datPhong.LayDangDat();
                TrinhBay.ThemNhan(_dsDat, TrinhBay.NhanDatPhong);
                _view.HienDatPhong(_dsDat, maDatPhong);
                if (_dsDat.Rows.Count == 0) _view.ChuyenSangVangLai();
            }
            catch (Exception ex) { Loi("Không tải được đặt phòng", ex); }
        }

        public void TaiKhach(int? chon)
        {
            try { _view.HienKhach(_khach.TimKiem(), chon); }
            catch (Exception ex) { Loi("Không tải được khách hàng", ex); }
        }

        public void ChonDatPhong()
        {
            if (!_view.CheDoTuDat) return;
            DataRow r = DongDat(_view.MaDatPhongChon);
            if (r == null)
            {
                _view.DatThongTin("Không có đặt phòng nào đang chờ nhận.");
                _view.DatChoPhepNhan(false);
                return;
            }
            _view.DatChoPhepNhan(true);
            DateTime homNay = _homNay().Date;
            DateTime ngayDi = Convert.ToDateTime(r["NgayDi"]).Date;
            DateTime den = Convert.ToDateTime(r["NgayDen"]).Date;
            _view.DatNgayDi(ngayDi > homNay ? ngayDi : homNay.AddDays(1));
            _view.DatThongTin(string.Format("Cọc đã nhận: {0:N0} đ. Ngày đến đặt: {1:dd/MM/yyyy}{2}", r["TienCoc"], den,
                den > homNay ? " (khách đến sớm hơn lịch đặt)." : "."));
        }

        public void TaiPhongTrong()
        {
            try
            {
                DataTable t = _phong.TimKiemTrongKhoang(_homNay().Date, _view.NgayDiDuKien.Date);
                _view.HienPhongTrong(t);
                _view.DatChoPhepNhan(t.Rows.Count > 0);
                if (t.Rows.Count == 0) _view.DatThongTin("Không có phòng trống trong khoảng này.");
            }
            catch (Exception ex) { Loi("Không tải được phòng trống", ex); }
        }

        public void Nhan()
        {
            try
            {
                int maPhong, maKhach;
                int? maDat = null;
                if (_view.CheDoTuDat)
                {
                    DataRow r = DongDat(_view.MaDatPhongChon);
                    if (r == null) return;
                    maDat = Convert.ToInt32(r["MaDatPhong"]);
                    maPhong = Convert.ToInt32(r["MaPhong"]);
                    maKhach = Convert.ToInt32(r["MaKhachHang"]);
                }
                else
                {
                    if (!_view.PhongChon.HasValue || !_view.KhachChon.HasValue)
                    {
                        _view.HienThongBao("Thiếu dữ liệu", "Chọn khách hàng và phòng.", MucThongBao.CanhBao);
                        return;
                    }
                    maPhong = _view.PhongChon.Value;
                    maKhach = _view.KhachChon.Value;
                }
                int ma = _luuTru.NhanPhong(maPhong, maKhach, _nguoiDung.MaNguoiDung, _view.NgayDiDuKien.Date, maDat);
                _view.HienThongBao("Thành công", "Đã nhận phòng. Mã phiếu thuê: " + ma, MucThongBao.ThanhCong);
                _view.DongThanhCong();
            }
            catch (Exception ex) { Loi("Không thể nhận phòng", ex); }
        }

        private DataRow DongDat(int? maDat)
        {
            if (_dsDat == null || !maDat.HasValue) return null;
            foreach (DataRow r in _dsDat.Rows)
                if (Convert.ToInt32(r["MaDatPhong"]) == maDat.Value) return r;
            return null;
        }

        private void Loi(string tieuDe, Exception ex)
        {
            _view.HienThongBao(tieuDe, ex.Message, MucThongBao.CanhBao);
        }
    }
}
