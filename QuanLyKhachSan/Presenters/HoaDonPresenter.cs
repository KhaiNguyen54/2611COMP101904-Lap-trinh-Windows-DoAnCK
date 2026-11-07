using System;
using System.Data;
using QuanLyKhachSan.Services;
using QuanLyKhachSan.Views;

namespace QuanLyKhachSan.Presenters
{
    /// <summary>Điều phối màn hình tra cứu và in hóa đơn.</summary>
    public class HoaDonPresenter
    {
        private readonly IHoaDonView _view;
        private readonly IHoaDonService _hoaDon;
        private readonly ILuuTruService _luuTru;

        public HoaDonPresenter(IHoaDonView view, IHoaDonService hoaDon, ILuuTruService luuTru)
        {
            _view = view ?? throw new ArgumentNullException(nameof(view));
            _hoaDon = hoaDon ?? throw new ArgumentNullException(nameof(hoaDon));
            _luuTru = luuTru ?? throw new ArgumentNullException(nameof(luuTru));
        }

        public void Tai()
        {
            try
            {
                if (_view.TuNgay.Date > _view.DenNgay.Date)
                {
                    _view.HienThongBao("Khoảng ngày", "Từ ngày phải trước hoặc bằng đến ngày.", MucThongBao.CanhBao);
                    return;
                }
                DataTable t = _hoaDon.TimKiem(_view.TuNgay.Date, _view.DenNgay.Date, _view.TuKhoa);
                decimal tong = 0;
                foreach (DataRow r in t.Rows) tong += Convert.ToDecimal(r["TongThanhToan"]);
                _view.HienDanhSach(t, t.Rows.Count + " hóa đơn · Tổng thanh toán: " + tong.ToString("N0") + " đ");
            }
            catch (Exception ex) { _view.HienThongBao("Không thể tra cứu hóa đơn", ex.Message, MucThongBao.CanhBao); }
        }

        public void In()
        {
            DataRow hd = _view.HoaDonChon;
            if (hd == null)
            {
                _view.HienThongBao("Chưa chọn", "Chọn một hóa đơn để in.", MucThongBao.ThongTin);
                return;
            }
            try
            {
                DataTable dv = _luuTru.LayDichVu(Convert.ToInt32(hd["MaPhieuThue"]));
                _view.MoBanIn(hd, dv);
            }
            catch (Exception ex) { _view.HienThongBao("Không thể in hóa đơn", ex.Message, MucThongBao.CanhBao); }
        }
    }
}
