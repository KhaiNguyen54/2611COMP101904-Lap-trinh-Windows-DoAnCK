using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace QuanLyKhachSan.BLL
{
    /// <summary>Tạo và ghi tệp CSV (BC06). Ghi UTF-8 có BOM để Excel đọc đúng tiếng Việt.</summary>
    public static class CsvExporter
    {
        /// <summary>Dựng nội dung CSV, dòng kết thúc CRLF. Có thể thêm dòng "sep=," để Excel luôn tách theo dấu phẩy.</summary>
        public static string TaoNoiDung(IList<string> tieuDe, IEnumerable<IList<string>> dong, bool kemSepHint = false)
        {
            var sb = new StringBuilder();
            if (kemSepHint) sb.Append("sep=,\r\n");
            GhiDong(sb, tieuDe);
            foreach (IList<string> d in dong)
                GhiDong(sb, d);
            return sb.ToString();
        }

        public static void Ghi(string duongDan, IList<string> tieuDe, IEnumerable<IList<string>> dong, bool kemSepHint = true)
        {
            if (string.IsNullOrWhiteSpace(duongDan))
                throw new ArgumentException("Chưa chọn nơi lưu tệp.");
            File.WriteAllText(duongDan, TaoNoiDung(tieuDe, dong, kemSepHint), new UTF8Encoding(true));
        }

        /// <summary>
        /// Bọc ô trong dấu nháy khi chứa dấu phẩy, nháy kép hoặc xuống dòng; nhân đôi nháy kép.
        /// Ô văn bản bắt đầu bằng = + - @ (không phải số) được thêm dấu ' phía trước để Excel không chạy như công thức.
        /// </summary>
        public static string ThoatO(string giaTri)
        {
            string s = giaTri ?? string.Empty;
            if (s.Length > 0 && "=+-@".IndexOf(s[0]) >= 0 && !LaSo(s))
                s = "'" + s;
            if (s.IndexOfAny(new[] { ',', '"', '\r', '\n' }) >= 0)
                s = "\"" + s.Replace("\"", "\"\"") + "\"";
            return s;
        }

        private static void GhiDong(StringBuilder sb, IList<string> o)
        {
            for (int i = 0; i < o.Count; i++)
            {
                if (i > 0) sb.Append(',');
                sb.Append(ThoatO(o[i]));
            }
            sb.Append("\r\n");
        }

        private static bool LaSo(string s)
        {
            decimal v;
            return decimal.TryParse(s, NumberStyles.Number, CultureInfo.InvariantCulture, out v)
                   || decimal.TryParse(s, NumberStyles.Number, CultureInfo.CurrentCulture, out v);
        }
    }
}
