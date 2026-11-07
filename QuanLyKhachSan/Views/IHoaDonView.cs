using System;
using System.Data;

namespace QuanLyKhachSan.Views
{
    /// <summary>View của màn hình tra cứu hóa đơn.</summary>
    public interface IHoaDonView : IThongBaoView
    {
        DateTime TuNgay { get; }
        DateTime DenNgay { get; }
        string TuKhoa { get; }

        /// <summary>Dòng hóa đơn đang chọn; null nếu chưa chọn.</summary>
        DataRow HoaDonChon { get; }

        void HienDanhSach(DataTable bang, string tong);
        void MoBanIn(DataRow hoaDon, DataTable dichVu);
    }
}
