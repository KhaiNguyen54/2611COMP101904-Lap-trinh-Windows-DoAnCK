using System;
using System.Collections.Generic;
using System.Data;
using QuanLyKhachSan.Models;
using QuanLyKhachSan.Services;
using QuanLyKhachSan.Views;

namespace QuanLyKhachSan.Presenters
{
    /// <summary>Điều phối thống kê doanh thu (BC01–BC03) và công suất phòng (BC04).</summary>
    public class BaoCaoPresenter
    {
        private readonly IBaoCaoView _view;
        private readonly IBaoCaoService _baoCao;

        public BaoCaoPresenter(IBaoCaoView view, IBaoCaoService baoCao)
        {
            _view = view ?? throw new ArgumentNullException(nameof(view));
            _baoCao = baoCao ?? throw new ArgumentNullException(nameof(baoCao));
        }

        public void TaiDoanhThu()
        {
            try
            {
                KieuGomDoanhThu kieu = _view.KieuGom;
                DataTable bang = _baoCao.DoanhThu(_view.DoanhThuTu.Date, _view.DoanhThuDen.Date, kieu);
                var nhan = new List<string>();
                var giaTri = new List<double>();
                foreach (DataRow r in bang.Rows)
                {
                    string n = Convert.ToString(r["Nhan"]);
                    nhan.Add(kieu == KieuGomDoanhThu.Ngay && n.Length >= 5 ? n.Substring(0, 5) : n);
                    giaTri.Add(Convert.ToDouble(r["DoanhThu"]));
                }
                string tong = "Tổng doanh thu: " + BaoCaoService.TongDoanhThu(bang).ToString("N0") + " đ · " + BaoCaoService.TongSoHoaDon(bang) + " hóa đơn";
                _view.HienDoanhThu(bang, nhan, giaTri, tong);
            }
            catch (Exception ex) { _view.HienThongBao("Không thể thống kê doanh thu", ex.Message, MucThongBao.CanhBao); }
        }

        public void TaiCongSuat()
        {
            try
            {
                DataTable bang = _baoCao.CongSuat(_view.CongSuatTu.Date, _view.CongSuatDen.Date);
                var nhan = new List<string>();
                var giaTri = new List<double>();
                foreach (DataRow r in bang.Rows)
                {
                    nhan.Add(Convert.ToDateTime(r["Ngay"]).ToString("dd/MM"));
                    giaTri.Add(Convert.ToDouble(r["TyLe"]));
                }
                string tong = "Tỷ lệ lấp đầy trung bình: " + BaoCaoService.TyLeTrungBinh(bang).ToString("0.##") + "%";
                _view.HienCongSuat(bang, nhan, giaTri, tong);
            }
            catch (Exception ex) { _view.HienThongBao("Không thể thống kê công suất", ex.Message, MucThongBao.CanhBao); }
        }
    }
}
