using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using QuanLyKhachSan.DAL;

namespace QuanLyKhachSan.BLL
{
    /// <summary>Cách gom doanh thu trong báo cáo (BC01).</summary>
    public enum KieuGomDoanhThu
    {
        Ngay,
        Thang,
        LoaiPhong
    }

    public class BaoCaoBLL
    {
        private const int SoNgayToiDa = 366;
        private readonly BaoCaoDAL _dal = new BaoCaoDAL();

        /// <summary>
        /// Báo cáo doanh thu gom theo ngày, tháng hoặc loại phòng. Kết quả có các cột
        /// Nhan (string), SoHoaDon (long), DoanhThu (decimal). Theo ngày/tháng có đủ mọi kỳ, kỳ không có hóa đơn = 0.
        /// </summary>
        public DataTable DoanhThu(DateTime tuNgay, DateTime denNgay, KieuGomDoanhThu kieu)
        {
            KiemTraKhoang(tuNgay, denNgay);
            if (kieu == KieuGomDoanhThu.Ngay && (denNgay.Date - tuNgay.Date).Days >= SoNgayToiDa)
                throw new ArgumentException("Báo cáo theo ngày tối đa " + SoNgayToiDa + " ngày; hãy chọn gom theo tháng.");

            DataTable goc = _dal.DoanhThu(tuNgay.Date, denNgay.Date);
            return GomDoanhThu(goc, tuNgay.Date, denNgay.Date, kieu);
        }

        /// <summary>Gom bảng doanh thu gốc (Ngay, TenLoaiPhong, SoHoaDon, DoanhThu). Tách ra để kiểm thử không cần CSDL.</summary>
        public static DataTable GomDoanhThu(DataTable goc, DateTime tuNgay, DateTime denNgay, KieuGomDoanhThu kieu)
        {
            var thuTu = new List<string>();
            var soHoaDon = new Dictionary<string, long>();
            var doanhThu = new Dictionary<string, decimal>();

            Action<string> khoiTao = delegate(string nhan)
            {
                if (soHoaDon.ContainsKey(nhan)) return;
                thuTu.Add(nhan);
                soHoaDon[nhan] = 0;
                doanhThu[nhan] = 0m;
            };

            // Theo ngày/tháng: tạo sẵn mọi kỳ trong khoảng để biểu đồ không bị đứt quãng.
            if (kieu == KieuGomDoanhThu.Ngay)
            {
                for (DateTime d = tuNgay.Date; d <= denNgay.Date; d = d.AddDays(1))
                    khoiTao(NhanKy(d, kieu, null));
            }
            else if (kieu == KieuGomDoanhThu.Thang)
            {
                var d = new DateTime(tuNgay.Year, tuNgay.Month, 1);
                var cuoi = new DateTime(denNgay.Year, denNgay.Month, 1);
                for (; d <= cuoi; d = d.AddMonths(1))
                    khoiTao(NhanKy(d, kieu, null));
            }

            foreach (DataRow row in goc.Rows)
            {
                string nhan = NhanKy(Convert.ToDateTime(row["Ngay"]), kieu, Convert.ToString(row["TenLoaiPhong"]));
                khoiTao(nhan);
                soHoaDon[nhan] += Convert.ToInt64(row["SoHoaDon"]);
                doanhThu[nhan] += Convert.ToDecimal(row["DoanhThu"]);
            }

            if (kieu == KieuGomDoanhThu.LoaiPhong)
                thuTu.Sort(delegate(string a, string b) { return doanhThu[b].CompareTo(doanhThu[a]); });

            var ketQua = new DataTable();
            ketQua.Columns.Add("Nhan", typeof(string));
            ketQua.Columns.Add("SoHoaDon", typeof(long));
            ketQua.Columns.Add("DoanhThu", typeof(decimal));
            foreach (string nhan in thuTu)
                ketQua.Rows.Add(nhan, soHoaDon[nhan], doanhThu[nhan]);
            return ketQua;
        }

        /// <summary>Công suất phòng theo ngày (BC04): Ngay, SoPhongCoKhach, TongPhong, TyLe.</summary>
        public DataTable CongSuat(DateTime tuNgay, DateTime denNgay)
        {
            KiemTraKhoang(tuNgay, denNgay);
            if ((denNgay.Date - tuNgay.Date).Days >= SoNgayToiDa)
                throw new ArgumentException("Thống kê công suất tối đa " + SoNgayToiDa + " ngày.");
            return _dal.CongSuat(tuNgay.Date, denNgay.Date);
        }

        public static decimal TongDoanhThu(DataTable doanhThu)
        {
            decimal tong = 0m;
            foreach (DataRow row in doanhThu.Rows)
                tong += Convert.ToDecimal(row["DoanhThu"]);
            return tong;
        }

        public static long TongSoHoaDon(DataTable doanhThu)
        {
            long tong = 0;
            foreach (DataRow row in doanhThu.Rows)
                tong += Convert.ToInt64(row["SoHoaDon"]);
            return tong;
        }

        /// <summary>Tỷ lệ lấp đầy trung bình (%) của bảng công suất; 0 nếu rỗng.</summary>
        public static decimal TyLeTrungBinh(DataTable congSuat)
        {
            if (congSuat.Rows.Count == 0) return 0m;
            decimal tong = 0m;
            foreach (DataRow row in congSuat.Rows)
                tong += Convert.ToDecimal(row["TyLe"]);
            return Math.Round(tong / congSuat.Rows.Count, 2);
        }

        private static string NhanKy(DateTime ngay, KieuGomDoanhThu kieu, string tenLoaiPhong)
        {
            switch (kieu)
            {
                case KieuGomDoanhThu.Ngay:
                    return ngay.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);
                case KieuGomDoanhThu.Thang:
                    return ngay.ToString("MM/yyyy", CultureInfo.InvariantCulture);
                default:
                    return tenLoaiPhong ?? string.Empty;
            }
        }

        private static void KiemTraKhoang(DateTime tuNgay, DateTime denNgay)
        {
            if (tuNgay.Date > denNgay.Date)
                throw new ArgumentException("Từ ngày phải nhỏ hơn hoặc bằng đến ngày.");
        }
    }
}
