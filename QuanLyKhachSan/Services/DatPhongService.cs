using System;
using System.Data;
using QuanLyKhachSan.Repositories;
using QuanLyKhachSan.Models;

namespace QuanLyKhachSan.Services
{
    public class DatPhongService
    {
        private readonly DatPhongRepository _dal = new DatPhongRepository();

        public DataTable TimKiem(DateTime? tuNgay = null, DateTime? denNgay = null, string trangThai = null)
        {
            return _dal.TimKiem(tuNgay, denNgay, trangThai);
        }

        /// <summary>Các lượt đặt còn hiệu lực (Đã đặt), dùng cho màn hình nhận phòng.</summary>
        public DataTable LayDangDat()
        {
            return _dal.TimKiem(null, null, "Đã đặt");
        }

        public int Them(DatPhong x)
        {
            if (x == null) throw new ArgumentNullException(nameof(x));
            if (x.MaKhachHang <= 0 || x.MaPhong <= 0 || x.MaNguoiDung <= 0)
                throw new ArgumentException("Chọn khách hàng, phòng và nhân viên hợp lệ.");
            if (x.NgayDen.Date < DateTime.Today)
                throw new ArgumentException("Ngày đến không được trước hôm nay.");
            if (x.NgayDen.Date >= x.NgayDi.Date)
                throw new ArgumentException("Ngày đến phải trước ngày đi.");
            if (x.TienCoc < 0)
                throw new ArgumentException("Tiền cọc không được âm.");

            decimal? gia = _dal.LayGiaPhong(x.MaPhong);
            if (gia == null)
                throw new ArgumentException("Phòng không tồn tại.");
            decimal cocToiDa = QuyTacTinhTien.TienCocToiDa(gia.Value, x.NgayDen, x.NgayDi);
            if (x.TienCoc > cocToiDa)
                throw new ArgumentException(
                    string.Format("Tiền cọc tối đa bằng tiền phòng 1 đêm × số đêm đặt ({0:N0} đ).", cocToiDa));

            return _dal.Them(x);
        }

        /// <summary>Hủy lượt đặt còn hiệu lực.</summary>
        public void Huy(int maDatPhong)
        {
            if (_dal.Huy(maDatPhong) != 1)
                throw new InvalidOperationException("Chỉ hủy được lượt đặt đang ở trạng thái Đã đặt.");
        }

        /// <summary>Đánh dấu khách không đến (chỉ khi đã quá ngày đến).</summary>
        public void DanhDauKhongDen(int maDatPhong)
        {
            if (_dal.DanhDauKhongDen(maDatPhong) != 1)
                throw new InvalidOperationException("Chỉ đánh dấu được lượt đặt Đã đặt có ngày đến đã qua.");
        }
    }
}
