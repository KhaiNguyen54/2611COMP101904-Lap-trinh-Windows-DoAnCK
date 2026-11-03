using System;
using System.Collections.Generic;
using System.Data;
using QuanLyKhachSan.BLL;

namespace QuanLyKhachSan.Web
{
    public sealed class GiaLoaiPhong
    {
        public string TenLoai { get; set; }
        public string MoTa { get; set; }
        public decimal GiaMoiDem { get; set; }
        public int SucChua { get; set; }
    }

    public sealed class ThongTinPhong
    {
        public string SoPhong { get; set; }
        public int Tang { get; set; }
        public string TenLoai { get; set; }
        public string MoTa { get; set; }
        public decimal GiaMoiDem { get; set; }
        public int SucChua { get; set; }
        /// <summary>Trạng thái hiển thị công khai (đã bỏ thông tin khách đang ở).</summary>
        public string TrangThai { get; set; }
    }

    /// <summary>
    /// Truy vấn chỉ đọc cho trang tra cứu công khai. Không bao giờ trả về thông tin khách hàng, hóa đơn hay tài khoản.
    /// </summary>
    public static class TraCuuService
    {
        public const int ToiDaDem = 60;

        public static List<GiaLoaiPhong> LayBangGia()
        {
            var kq = new List<GiaLoaiPhong>();
            foreach (DataRow r in new LoaiPhongBLL().TimKiem().Rows)
            {
                if (!Convert.ToBoolean(r["HoatDong"])) continue;
                kq.Add(new GiaLoaiPhong
                {
                    TenLoai = Convert.ToString(r["TenLoaiPhong"]),
                    MoTa = r["MoTa"] == DBNull.Value ? "" : Convert.ToString(r["MoTa"]),
                    GiaMoiDem = Convert.ToDecimal(r["GiaMoiDem"]),
                    SucChua = Convert.ToInt32(r["SucChua"])
                });
            }
            return kq;
        }

        public static ThongTinPhong LayPhong(string soPhong)
        {
            if (string.IsNullOrWhiteSpace(soPhong) || soPhong.Length > 10) return null;
            string so = soPhong.Trim();

            var loai = new Dictionary<int, DataRow>();
            foreach (DataRow r in new LoaiPhongBLL().TimKiem().Rows)
                loai[Convert.ToInt32(r["MaLoaiPhong"])] = r;

            foreach (DataRow r in new PhongBLL().TimKiem(so).Rows)
            {
                if (!string.Equals(Convert.ToString(r["SoPhong"]).Trim(), so, StringComparison.OrdinalIgnoreCase)) continue;
                if (!Convert.ToBoolean(r["HoatDong"])) return null;
                DataRow l;
                if (!loai.TryGetValue(Convert.ToInt32(r["MaLoaiPhong"]), out l)) return null;
                return new ThongTinPhong
                {
                    SoPhong = Convert.ToString(r["SoPhong"]).Trim(),
                    Tang = Convert.ToInt32(r["Tang"]),
                    TenLoai = Convert.ToString(l["TenLoaiPhong"]),
                    MoTa = l["MoTa"] == DBNull.Value ? "" : Convert.ToString(l["MoTa"]),
                    GiaMoiDem = Convert.ToDecimal(l["GiaMoiDem"]),
                    SucChua = Convert.ToInt32(l["SucChua"]),
                    TrangThai = Convert.ToString(r["TrangThai"])
                };
            }
            return null;
        }

        /// <summary>Các phòng còn trống trọn khoảng [den, di) cùng giá; ném ArgumentException nếu khoảng ngày không hợp lệ.</summary>
        public static List<ThongTinPhong> LayPhongTrong(DateTime den, DateTime di)
        {
            if ((di.Date - den.Date).TotalDays > ToiDaDem)
                throw new ArgumentException("Chỉ tra cứu tối đa " + ToiDaDem + " đêm một lần.");
            var kq = new List<ThongTinPhong>();
            foreach (DataRow r in new PhongBLL().TimKiemTrongKhoang(den, di).Rows)
                kq.Add(new ThongTinPhong
                {
                    SoPhong = Convert.ToString(r["SoPhong"]).Trim(),
                    Tang = Convert.ToInt32(r["Tang"]),
                    TenLoai = Convert.ToString(r["TenLoaiPhong"]),
                    GiaMoiDem = Convert.ToDecimal(r["GiaMoiDem"]),
                    TrangThai = "Trống"
                });
            return kq;
        }

        /// <summary>Tên trạng thái thân thiện cho khách (không tiết lộ ai đang ở).</summary>
        public static string NhanTrangThai(string trangThai)
        {
            switch (trangThai)
            {
                case "Trống": return "Hiện đang trống";
                case "Đã đặt": return "Đã có lịch đặt sắp tới";
                case "Đang ở": return "Hiện đang có khách";
                case "Dọn dẹp": return "Đang được dọn";
                case "Bảo trì": return "Đang bảo trì";
                default: return trangThai;
            }
        }
    }
}
