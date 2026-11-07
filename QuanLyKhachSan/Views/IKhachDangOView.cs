using System.Data;

namespace QuanLyKhachSan.Views
{
    /// <summary>View của màn hình khách đang lưu trú.</summary>
    public interface IKhachDangOView : IThongBaoView
    {
        string TuKhoaLoc { get; }
        void HienDanhSach(DataTable bang);
        void HienSoLuong(string noiDung);
    }
}
