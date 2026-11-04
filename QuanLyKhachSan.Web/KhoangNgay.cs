using System;
using System.Globalization;

namespace QuanLyKhachSan.Web
{
    /// <summary>Đọc và kiểm tra khoảng ngày ?den=&amp;di= của trang tra cứu (logic thuần, dễ kiểm thử).</summary>
    public static class KhoangNgay
    {
        public const string DinhDang = "yyyy-MM-dd";

        /// <summary>
        /// Mặc định hôm nay → ngày mai. Trả về thông báo lỗi tiếng Việt nếu không hợp lệ, ngược lại trả null.
        /// </summary>
        public static string Doc(string sDen, string sDi, DateTime homNay, out DateTime den, out DateTime di)
        {
            homNay = homNay.Date;
            den = homNay;
            di = homNay.AddDays(1);
            if (!string.IsNullOrEmpty(sDen) && !DateTime.TryParseExact(sDen, DinhDang, CultureInfo.InvariantCulture, DateTimeStyles.None, out den))
                return "Ngày đến không hợp lệ.";
            if (string.IsNullOrEmpty(sDen)) den = homNay;
            if (!string.IsNullOrEmpty(sDi))
            {
                if (!DateTime.TryParseExact(sDi, DinhDang, CultureInfo.InvariantCulture, DateTimeStyles.None, out di))
                    return "Ngày đi không hợp lệ.";
            }
            else di = den.AddDays(1);
            if (den.Date < homNay) return "Ngày đến không được trước hôm nay.";
            if (den.Date >= di.Date) return "Ngày đến phải trước ngày đi.";
            return null;
        }
    }
}
