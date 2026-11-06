using System;

namespace QuanLyKhachSan.Services
{
    /// <summary>Ghép đường dẫn trang web tra cứu phòng/giá để mã hóa vào mã QR (thuần logic, không cần CSDL).</summary>
    public static class TraCuuUrl
    {
        /// <summary>Chuẩn hóa địa chỉ gốc: bắt buộc http/https, bỏ dấu "/" cuối.</summary>
        public static string ChuanHoaGoc(string goc)
        {
            if (string.IsNullOrWhiteSpace(goc))
                throw new ArgumentException("Chưa cấu hình địa chỉ trang tra cứu (Web:BaseUrl trong appsettings.json).");
            string g = goc.Trim().TrimEnd('/');
            Uri uri;
            if (!Uri.TryCreate(g, UriKind.Absolute, out uri) || (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
                throw new ArgumentException("Địa chỉ trang tra cứu phải bắt đầu bằng http:// hoặc https://.");
            return g;
        }

        public static string Phong(string goc, string soPhong)
        {
            if (string.IsNullOrWhiteSpace(soPhong))
                throw new ArgumentException("Số phòng không hợp lệ.", nameof(soPhong));
            return ChuanHoaGoc(goc) + "/phong/" + Uri.EscapeDataString(soPhong.Trim());
        }

        public static string BangGia(string goc)
        {
            return ChuanHoaGoc(goc) + "/gia";
        }
    }
}
