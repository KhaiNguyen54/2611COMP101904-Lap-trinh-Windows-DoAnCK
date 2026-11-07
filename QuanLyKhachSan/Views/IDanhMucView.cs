using System.Data;

namespace QuanLyKhachSan.Views
{
    public enum LoaiDanhMuc { KhachHang, LoaiPhong, Phong, DichVu }

    /// <summary>
    /// View của màn hình danh mục dùng chung (Khách hàng, Loại phòng, Phòng, Dịch vụ).
    /// Các ô nhập được đánh số theo thứ tự hiển thị của từng loại danh mục.
    /// </summary>
    public interface IDanhMucView : IThongBaoView
    {
        string TuKhoa { get; }

        /// <summary>Mã bản ghi đang nạp vào ô nhập; 0 khi đang tạo mới.</summary>
        int MaDangSua { get; }

        string DocChu(int viTri);
        decimal DocSo(int viTri);
        int? DocChon(int viTri);

        void HienDanhSach(DataTable bang);
        void HienLoaiPhong(DataTable loaiPhong);
        void LamMoiFormNhap();
        void DatTrangThai(string noiDung);
    }
}
