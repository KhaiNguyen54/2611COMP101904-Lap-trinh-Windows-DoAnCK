using System;
using System.Collections.Generic;
using System.Data;
using QuanLyKhachSan.Models;
using QuanLyKhachSan.Services;
using QuanLyKhachSan.Views;

namespace QuanLyKhachSan.Presenters
{
    /// <summary>Điều phối đổi phòng khi khách đang lưu trú (NV08).</summary>
    public class DoiPhongPresenter
    {
        private readonly IDoiPhongView _view;
        private readonly ILuuTruService _luuTru;
        private readonly NguoiDung _nguoiDung;
        private DataTable _dsPhieu;
        private DataTable _dsPhong;

        public DoiPhongPresenter(IDoiPhongView view, ILuuTruService luuTru, NguoiDung nguoiDung)
        {
            _view = view ?? throw new ArgumentNullException(nameof(view));
            _luuTru = luuTru ?? throw new ArgumentNullException(nameof(luuTru));
            _nguoiDung = nguoiDung ?? throw new ArgumentNullException(nameof(nguoiDung));
        }

        public void TaiPhieu()
        {
            try
            {
                _dsPhieu = _luuTru.LayPhieuDangO();
                TrinhBay.ThemNhan(_dsPhieu, TrinhBay.NhanPhieuDoiPhong);
                _view.HienPhieu(_dsPhieu);
                if (_dsPhieu.Rows.Count == 0) _view.DatLoiNhac("Chưa có phòng nào đang có khách.");
            }
            catch (Exception ex) { Loi("Không tải được phòng đang ở", ex); }
        }

        public void TaiPhongMoi()
        {
            DataRow p = Phieu();
            if (p == null) { _dsPhong = null; _view.HienPhongMoi(null); return; }
            try
            {
                _dsPhong = _luuTru.LayPhongCoTheDoi(Convert.ToInt32(p["MaPhong"]), Convert.ToDateTime(p["NgayDiDuKien"]));
                TrinhBay.ThemNhan(_dsPhong, TrinhBay.NhanPhongMoi);
                _view.HienPhongMoi(_dsPhong);
                if (_dsPhong.Rows.Count == 0) _view.DatLoiNhac("Không có phòng trống trọn thời gian còn lại của kỳ lưu trú.");
                else CapNhatLoiNhac();
            }
            catch (Exception ex) { Loi("Không tải được danh sách phòng trống", ex); }
        }

        public void CapNhatLoiNhac()
        {
            DataRow p = Phieu();
            DataRow m = PhongMoi();
            if (p == null || m == null) return;
            string nhac = QuyTacDoiPhong.LoiNhacGia(Convert.ToDecimal(p["GiaPhongMoiDem"]), Convert.ToDecimal(m["GiaMoiDem"]));
            _view.DatLoiNhac(nhac ?? "Phòng mới cùng mức giá; dịch vụ đã ghi vẫn giữ nguyên trong phiếu thuê.");
        }

        public void TaiLichSu()
        {
            DataRow p = Phieu();
            if (p == null) { _view.HienLichSu(null); return; }
            try { _view.HienLichSu(_luuTru.LayLichSuDoiPhong(Convert.ToInt32(p["MaPhieuThue"]))); }
            catch (Exception ex) { Loi("Không tải được lịch sử đổi phòng", ex); }
        }

        public void Doi()
        {
            DataRow p = Phieu();
            DataRow m = PhongMoi();
            if (p == null || m == null)
            {
                _view.HienThongBao("Thiếu dữ liệu", "Chọn phòng đang ở và phòng mới.", MucThongBao.CanhBao);
                return;
            }
            string cauHoi = string.Format("Đổi khách {0} từ phòng {1} sang phòng {2}?\nPhòng cũ sẽ chuyển sang Dọn dẹp.",
                p["HoTen"], p["SoPhong"], m["SoPhong"]);
            if (!_view.XacNhan(cauHoi)) return;
            try
            {
                _luuTru.DoiPhong(Convert.ToInt32(p["MaPhieuThue"]), Convert.ToInt32(m["MaPhong"]), _nguoiDung.MaNguoiDung, _view.LyDo);
                _view.HienThongBao("Thành công", "Đã đổi phòng.", MucThongBao.ThanhCong);
                _view.XoaLyDo();
                TaiPhieu();
            }
            catch (Exception ex) { Loi("Không thể đổi phòng", ex); }
        }

        private DataRow Phieu()
        {
            return Tim(_dsPhieu, "MaPhieuThue", _view.PhieuChon);
        }

        private DataRow PhongMoi()
        {
            return Tim(_dsPhong, "MaPhong", _view.PhongMoiChon);
        }

        private static DataRow Tim(DataTable t, string cot, int? ma)
        {
            if (t == null || !ma.HasValue) return null;
            foreach (DataRow r in t.Rows)
                if (Convert.ToInt32(r[cot]) == ma.Value) return r;
            return null;
        }

        private void Loi(string tieuDe, Exception ex)
        {
            _view.HienThongBao(tieuDe, ex.Message, MucThongBao.CanhBao);
        }
    }
}
