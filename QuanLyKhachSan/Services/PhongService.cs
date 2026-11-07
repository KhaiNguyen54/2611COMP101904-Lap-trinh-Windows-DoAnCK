using System;
using System.Data;
using QuanLyKhachSan.Repositories;
using QuanLyKhachSan.Models;

namespace QuanLyKhachSan.Services
{
    public class PhongService : IPhongService
    {
        private readonly PhongRepository _dal = new PhongRepository();

        public DataTable TimKiem(string tuKhoa = null, int? tang = null, int? maLoai = null, string trangThai = null)
        {
            return _dal.TimKiem(tuKhoa, tang, maLoai, trangThai);
        }

        /// <summary>Dữ liệu sơ đồ phòng theo tầng (kèm khách đang ở, giá phòng, trạng thái lưu trong bảng).</summary>
        public DataTable LaySoDo()
        {
            return _dal.LaySoDo();
        }

        public DataTable TimKiemTrongKhoang(DateTime ngayDen, DateTime ngayDi)
        {
            if (ngayDen.Date < DateTime.Today)
                throw new ArgumentException("Ngày đến không được trước hôm nay.", nameof(ngayDen));
            if (ngayDen.Date >= ngayDi.Date)
                throw new ArgumentException("Ngày đến phải trước ngày đi.", nameof(ngayDi));
            return _dal.TimKiemTrongKhoang(ngayDen, ngayDi);
        }

        public int Them(Phong x)
        {
            KiemTra(x);
            KiemTraTrung(x);
            return _dal.Them(x);
        }

        public int CapNhat(Phong x)
        {
            KiemTra(x);
            KiemTraTrung(x);
            return _dal.CapNhat(x);
        }

        /// <summary>Phòng đã dọn xong: Dọn dẹp → Trống. Lễ tân và Quản lý đều được thực hiện.</summary>
        public void HoanTatDonDep(int maPhong)
        {
            if (maPhong <= 0)
                throw new ArgumentOutOfRangeException(nameof(maPhong), "Phòng không hợp lệ.");
            if (_dal.HoanTatDonDep(maPhong) != 1)
                throw new InvalidOperationException(
                    "Phòng không còn ở trạng thái dọn dẹp hoặc đã được cập nhật. Hãy làm mới sơ đồ phòng.");
        }

        /// <summary>Đưa phòng Trống vào bảo trì (chỉ Quản lý, QT05).</summary>
        public void BatDauBaoTri(int maPhong, bool laQuanLy)
        {
            if (!laQuanLy)
                throw new UnauthorizedAccessException("Chỉ Quản lý được chuyển phòng sang Bảo trì.");
            if (_dal.BatDauBaoTri(maPhong) != 1)
                throw new InvalidOperationException(
                    "Chỉ phòng Trống, không có khách và không còn lượt đặt sắp tới mới chuyển sang Bảo trì được.");
        }

        /// <summary>Kết thúc bảo trì: Bảo trì → Trống (chỉ Quản lý).</summary>
        public void KetThucBaoTri(int maPhong, bool laQuanLy)
        {
            if (!laQuanLy)
                throw new UnauthorizedAccessException("Chỉ Quản lý được kết thúc bảo trì.");
            if (_dal.KetThucBaoTri(maPhong) != 1)
                throw new InvalidOperationException("Phòng không ở trạng thái Bảo trì.");
        }

        public int NgungSuDung(int ma)
        {
            return _dal.NgungSuDung(ma);
        }

        private static void KiemTra(Phong x)
        {
            if (x == null || string.IsNullOrWhiteSpace(x.SoPhong) || x.SoPhong.Trim().Length > 10)
                throw new ArgumentException("Số phòng bắt buộc, tối đa 10 ký tự.");
            if (x.Tang < 1 || x.MaLoaiPhong < 1)
                throw new ArgumentException("Chọn tầng và loại phòng hợp lệ.");
            x.SoPhong = x.SoPhong.Trim();
        }

        private void KiemTraTrung(Phong x)
        {
            foreach (DataRow row in _dal.TimKiem().Rows)
            {
                bool khacBanGhi = Convert.ToInt32(row["MaPhong"]) != x.MaPhong;
                bool trungSo = string.Equals(Convert.ToString(row["SoPhong"]).Trim(), x.SoPhong.Trim(),
                    StringComparison.OrdinalIgnoreCase);
                if (khacBanGhi && trungSo)
                    throw new ArgumentException("Số phòng đã tồn tại.");
            }
        }
    }
}
