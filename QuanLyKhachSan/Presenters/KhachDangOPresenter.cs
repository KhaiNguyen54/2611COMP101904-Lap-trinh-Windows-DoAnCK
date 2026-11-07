using System;
using System.Data;
using QuanLyKhachSan.Services;
using QuanLyKhachSan.Views;

namespace QuanLyKhachSan.Presenters
{
    /// <summary>Điều phối danh sách khách đang lưu trú: nạp dữ liệu, lọc theo tên hoặc số phòng.</summary>
    public class KhachDangOPresenter
    {
        private readonly IKhachDangOView _view;
        private readonly ILuuTruService _luuTru;
        private DataTable _nguon;

        public KhachDangOPresenter(IKhachDangOView view, ILuuTruService luuTru)
        {
            _view = view ?? throw new ArgumentNullException(nameof(view));
            _luuTru = luuTru ?? throw new ArgumentNullException(nameof(luuTru));
        }

        public void Tai()
        {
            try
            {
                _nguon = _luuTru.LayKhachDangO();
                _view.HienDanhSach(_nguon);
                Loc();
            }
            catch (Exception ex) { _view.HienThongBao("Không tải được danh sách khách", ex.Message, MucThongBao.CanhBao); }
        }

        public void Loc()
        {
            if (_nguon == null) return;
            string k = (_view.TuKhoaLoc ?? string.Empty).Trim().Replace("'", "''").Replace("[", "[[]").Replace("%", "[%]").Replace("*", "[*]");
            _nguon.DefaultView.RowFilter = k.Length == 0 ? string.Empty : "HoTen LIKE '%" + k + "%' OR SoPhong LIKE '%" + k + "%'";
            _view.HienSoLuong(_nguon.DefaultView.Count + " khách đang lưu trú");
        }
    }
}
