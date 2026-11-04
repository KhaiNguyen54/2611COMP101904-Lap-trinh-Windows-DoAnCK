using System;

namespace QuanLyKhachSan.BLL
{
    /// <summary>
    /// Quy tắc thuần cho đổi phòng khi đang lưu trú (QT47–QT49 trong docs/03_QuyTac_NghiepVu.md).
    /// Tách riêng để kiểm thử được mà không cần SQL Server.
    /// </summary>
    public static class QuyTacDoiPhong
    {
        public const int LyDoToiDa = 200;

        /// <summary>
        /// Khoảng [den, di) mà phòng mới phải trống: từ hôm nay đến ngày đi dự kiến, tối thiểu 1 đêm (QT47).
        /// </summary>
        public static void KhoangCanTrong(DateTime homNay, DateTime ngayDiDuKien, out DateTime den, out DateTime di)
        {
            den = homNay.Date;
            di = ngayDiDuKien.Date > den ? ngayDiDuKien.Date : den.AddDays(1);
        }

        /// <summary>Kiểm tra lý do đổi phòng (tùy chọn, tối đa 200 ký tự); trả về nội dung đã cắt khoảng trắng hoặc null.</summary>
        public static string ChuanHoaLyDo(string lyDo)
        {
            if (string.IsNullOrWhiteSpace(lyDo)) return null;
            string s = lyDo.Trim();
            if (s.Length > LyDoToiDa)
                throw new ArgumentException("Lý do đổi phòng tối đa " + LyDoToiDa + " ký tự.");
            return s;
        }

        /// <summary>
        /// Lời nhắc khi giá phòng mới khác giá đang áp dụng (QT49): đơn giá của phiếu thuê được giữ nguyên,
        /// chênh lệch nếu có do lễ tân xử lý khi trả phòng. Trả về null nếu hai mức giá bằng nhau.
        /// </summary>
        public static string LoiNhacGia(decimal giaDangApDung, decimal giaPhongMoi)
        {
            if (giaPhongMoi == giaDangApDung) return null;
            string huong = giaPhongMoi > giaDangApDung ? "cao hơn" : "thấp hơn";
            return string.Format("Giá phòng mới {0} giá đang áp dụng ({1:N0} đ so với {2:N0} đ/đêm). Đơn giá của phiếu thuê được giữ nguyên; xử lý chênh lệch (nếu có) khi trả phòng.",
                huong, giaPhongMoi, giaDangApDung);
        }
    }
}
