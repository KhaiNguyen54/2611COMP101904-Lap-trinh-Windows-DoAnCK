using System;
using System.Collections.Generic;
using System.Data;
using QuanLyKhachSan.Models;
using QuanLyKhachSan.Services;
using QuanLyKhachSan.Views;

namespace QuanLyKhachSan.Presenters
{
    /// <summary>Điều phối ghi nhận và xóa dịch vụ phát sinh cho phiếu thuê đang ở (NV04).</summary>
    public class GhiDichVuPresenter
    {
        private readonly IGhiDichVuView _view;
        private readonly ILuuTruService _luuTru;
        private readonly IDichVuService _dichVu;

        public GhiDichVuPresenter(IGhiDichVuView view, ILuuTruService luuTru, IDichVuService dichVu)
        {
            _view = view ?? throw new ArgumentNullException(nameof(view));
            _luuTru = luuTru ?? throw new ArgumentNullException(nameof(luuTru));
            _dichVu = dichVu ?? throw new ArgumentNullException(nameof(dichVu));
        }

        public void TaiBanDau()
        {
            TaiPhieu();
            TaiDichVu();
        }

        public void TaiPhieu()
        {
            try
            {
                DataTable t = _luuTru.LayPhieuDangO();
                TrinhBay.ThemNhan(t, TrinhBay.NhanPhieuDangO);
                _view.HienPhieu(t);
                if (t.Rows.Count == 0) _view.DatTong("Chưa có phòng nào đang có khách.");
            }
            catch (Exception ex) { Loi("Không tải được phòng đang ở", ex); }
        }

        public void TaiDichVu()
        {
            try
            {
                DataTable t = _dichVu.TimKiem(null, true);
                TrinhBay.ThemNhan(t, TrinhBay.NhanDichVu);
                _view.HienDichVu(t);
            }
            catch (Exception ex) { Loi("Không tải được dịch vụ", ex); }
        }

        public void TaiChiTiet()
        {
            if (!_view.PhieuChon.HasValue)
            {
                _view.HienChiTiet(null);
                return;
            }
            try
            {
                DataTable t = _luuTru.LayDichVu(_view.PhieuChon.Value);
                _view.HienChiTiet(t);
                decimal tong = 0;
                foreach (DataRow r in t.Rows) tong += Convert.ToDecimal(r["ThanhTien"]);
                _view.DatTong("Tổng dịch vụ: " + tong.ToString("N0") + " đ (" + t.Rows.Count + " dòng)");
            }
            catch (Exception ex) { Loi("Không tải được dịch vụ đã ghi", ex); }
        }

        public void Ghi()
        {
            if (!_view.PhieuChon.HasValue || !_view.DichVuChon.HasValue)
            {
                _view.HienThongBao("Thiếu dữ liệu", "Chọn phòng đang ở và dịch vụ.", MucThongBao.CanhBao);
                return;
            }
            try
            {
                _luuTru.GhiDichVu(_view.PhieuChon.Value, _view.DichVuChon.Value, _view.SoLuong);
                _view.DatLaiSoLuong();
                TaiChiTiet();
            }
            catch (Exception ex) { Loi("Không thể ghi dịch vụ", ex); }
        }

        public void Xoa()
        {
            if (!_view.ChiTietChon.HasValue) return;
            if (!_view.XacNhan("Xóa dòng dịch vụ đã chọn?")) return;
            try
            {
                _luuTru.XoaDichVu(_view.ChiTietChon.Value);
                TaiChiTiet();
            }
            catch (Exception ex) { Loi("Không thể xóa", ex); }
        }

        private void Loi(string tieuDe, Exception ex)
        {
            _view.HienThongBao(tieuDe, ex.Message, MucThongBao.CanhBao);
        }
    }
}
