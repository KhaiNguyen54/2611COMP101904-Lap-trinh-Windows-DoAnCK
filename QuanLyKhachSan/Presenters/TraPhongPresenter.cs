using System;
using System.Collections.Generic;
using System.Data;
using QuanLyKhachSan.Models;
using QuanLyKhachSan.Services;
using QuanLyKhachSan.Views;

namespace QuanLyKhachSan.Presenters
{
    /// <summary>Điều phối trả phòng: tạm tính, giảm giá có duyệt, lập hóa đơn (NV05, NV06).</summary>
    public class TraPhongPresenter
    {
        private readonly ITraPhongView _view;
        private readonly ILuuTruService _luuTru;
        private readonly NguoiDung _nguoiDung;
        private BaoGiaHoaDon _baoGia;

        public TraPhongPresenter(ITraPhongView view, ILuuTruService luuTru, NguoiDung nguoiDung)
        {
            _view = view ?? throw new ArgumentNullException(nameof(view));
            _luuTru = luuTru ?? throw new ArgumentNullException(nameof(luuTru));
            _nguoiDung = nguoiDung ?? throw new ArgumentNullException(nameof(nguoiDung));
        }

        private bool LaQuanLy { get { return _nguoiDung.VaiTro == VaiTroNguoiDung.QuanLy; } }

        public void TaiPhieu()
        {
            try
            {
                DataTable t = _luuTru.LayPhieuDangO();
                TrinhBay.ThemNhan(t, TrinhBay.NhanPhieuDangO);
                _view.HienPhieu(t);
                _view.DatChoPhepTra(t.Rows.Count > 0);
                if (t.Rows.Count == 0) _view.DatGhiChu("Chưa có phòng nào đang có khách.");
            }
            catch (Exception ex) { Loi("Không tải được phòng đang ở", ex); }
        }

        public void TinhLai()
        {
            if (!_view.PhieuChon.HasValue) return;
            try
            {
                _baoGia = _luuTru.TinhTamTinh(_view.PhieuChon.Value, _view.NgayTra);
                _view.HienBaoGia(_baoGia.SoDem.ToString(), TrinhBay.Tien(_baoGia.TienPhong), TrinhBay.Tien(_baoGia.TienDichVu),
                    TrinhBay.Tien(_baoGia.PhuThu), (_baoGia.TienCoc > 0 ? "-" : string.Empty) + TrinhBay.Tien(_baoGia.TienCoc));
                decimal co = QuyTacTinhTien.CoSoGiamGia(_baoGia.TienPhong, _baoGia.TienDichVu, _baoGia.PhuThu);
                _view.DatGiamToiDa(Math.Max(0, co));
                _view.DatGhiChu("Trả sau 12:00: ≤1 giờ miễn phí, 1–3 giờ 30%, 3–6 giờ 50%, trên 6 giờ 100% giá một đêm. " +
                                "Giảm giá tối đa 10% tiền hàng; vượt mức cần Quản lý duyệt" +
                                (LaQuanLy ? " (bạn là Quản lý nên được ghi là người duyệt)." : " (nhập tài khoản Quản lý khi trả phòng)."));
                _view.DatChoPhepTra(true);
                HienTong();
            }
            catch (Exception ex)
            {
                _view.DatChoPhepTra(false);
                _baoGia = null;
                _view.DatGhiChu(ex.Message);
            }
        }

        public void HienTong()
        {
            if (_baoGia == null) return;
            decimal giam = _view.GiamGia;
            decimal tong = QuyTacTinhTien.TinhTongThanhToan(_baoGia.TienPhong, _baoGia.TienDichVu, _baoGia.PhuThu, giam, _baoGia.TienCoc);
            _view.HienGiamVaTong((giam > 0 ? "-" : string.Empty) + TrinhBay.Tien(giam), TrinhBay.Tien(tong));
        }

        public void Tra()
        {
            if (!_view.PhieuChon.HasValue) return;
            if (!_view.XacNhan("Xác nhận trả phòng và lập hóa đơn? Thao tác này không hoàn tác được.")) return;
            try
            {
                int maPhieu = _view.PhieuChon.Value;
                decimal giam = _view.GiamGia;
                int? nguoiDuyet = null;
                if (_baoGia != null && !LaQuanLy && QuyTacTinhTien.CanQuanLyDuyet(giam,
                        QuyTacTinhTien.CoSoGiamGia(_baoGia.TienPhong, _baoGia.TienDichVu, _baoGia.PhuThu)))
                {
                    nguoiDuyet = _view.YeuCauQuanLyDuyet();
                    if (!nguoiDuyet.HasValue) return;
                }
                int maHd = _luuTru.TraPhong(maPhieu, _nguoiDung.MaNguoiDung, _view.NgayTra, giam, _view.HinhThuc, LaQuanLy, nguoiDuyet);
                if (_view.XacNhan("Đã lập hóa đơn số " + maHd + ". In hóa đơn ngay?")) _view.MoBanIn(maHd, maPhieu);
                _view.DongThanhCong();
            }
            catch (Exception ex) { Loi("Không thể trả phòng", ex); }
        }

        private void Loi(string tieuDe, Exception ex)
        {
            _view.HienThongBao(tieuDe, ex.Message, MucThongBao.CanhBao);
        }
    }
}
