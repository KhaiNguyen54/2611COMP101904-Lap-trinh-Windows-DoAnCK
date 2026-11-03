using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Text;

namespace QuanLyKhachSan.Web
{
    /// <summary>Dựng HTML đơn giản, hiển thị tốt trên điện thoại (khách quét QR). Mọi dữ liệu đều được mã hóa HTML.</summary>
    internal static class TrangHtml
    {
        private static readonly NumberFormatInfo Tien = new NumberFormatInfo { NumberGroupSeparator = ".", NumberDecimalDigits = 0 };

        public static string E(string s) { return WebUtility.HtmlEncode(s ?? ""); }

        public static string Gia(decimal v) { return v.ToString("N0", Tien) + " đ"; }

        public static string Trang(string tieuDe, string noiDung)
        {
            return "<!doctype html><html lang=\"vi\"><head><meta charset=\"utf-8\">"
                + "<meta name=\"viewport\" content=\"width=device-width,initial-scale=1\">"
                + "<title>" + E(tieuDe) + " · Autumn Hotel</title><style>" + Css + "</style></head><body>"
                + "<header><a class=\"logo\" href=\"/\">Autumn Hotel</a><nav><a href=\"/\">Tìm phòng</a><a href=\"/gia\">Bảng giá</a></nav></header>"
                + "<main>" + noiDung + "</main>"
                + "<footer>Thông tin chỉ mang tính tham khảo; giá và tình trạng phòng có thể thay đổi. Vui lòng liên hệ lễ tân để đặt phòng.</footer>"
                + "</body></html>";
        }

        public static string Loi(string thongBao)
        {
            return "<div class=\"canhbao\">" + E(thongBao) + "</div>";
        }

        public static string BangGia(IList<GiaLoaiPhong> ds)
        {
            var sb = new StringBuilder("<h1>Bảng giá phòng</h1>");
            if (ds.Count == 0) return sb.Append("<p>Chưa có dữ liệu giá.</p>").ToString();
            sb.Append("<div class=\"luoi\">");
            foreach (GiaLoaiPhong g in ds)
                sb.Append("<section class=\"the\"><h2>").Append(E(g.TenLoai)).Append("</h2>")
                  .Append("<p class=\"gia\">").Append(Gia(g.GiaMoiDem)).Append("<small> / đêm</small></p>")
                  .Append("<p>Sức chứa: ").Append(g.SucChua).Append(" khách</p>")
                  .Append(string.IsNullOrWhiteSpace(g.MoTa) ? "" : "<p class=\"phu\">" + E(g.MoTa) + "</p>")
                  .Append("</section>");
            return sb.Append("</div>").ToString();
        }

        public static string ChiTietPhong(ThongTinPhong p, DateTime? den, DateTime? di, bool? conTrong)
        {
            var sb = new StringBuilder();
            sb.Append("<h1>Phòng ").Append(E(p.SoPhong)).Append("</h1><section class=\"the\">");
            sb.Append("<h2>").Append(E(p.TenLoai)).Append("</h2>");
            sb.Append("<p class=\"gia\">").Append(Gia(p.GiaMoiDem)).Append("<small> / đêm</small></p>");
            sb.Append("<p>Tầng ").Append(p.Tang).Append(" · Sức chứa ").Append(p.SucChua).Append(" khách</p>");
            if (!string.IsNullOrWhiteSpace(p.MoTa)) sb.Append("<p class=\"phu\">").Append(E(p.MoTa)).Append("</p>");
            sb.Append("<p>Tình trạng hiện tại: <b>").Append(E(TraCuuService.NhanTrangThai(p.TrangThai))).Append("</b></p></section>");

            sb.Append("<h2>Kiểm tra theo ngày</h2><form method=\"get\" class=\"bieumau\">")
              .Append("<label>Ngày đến<input type=\"date\" name=\"den\" value=\"").Append((den ?? DateTime.Today).ToString("yyyy-MM-dd")).Append("\"></label>")
              .Append("<label>Ngày đi<input type=\"date\" name=\"di\" value=\"").Append((di ?? DateTime.Today.AddDays(1)).ToString("yyyy-MM-dd")).Append("\"></label>")
              .Append("<button>Kiểm tra</button></form>");
            if (den.HasValue && di.HasValue && conTrong.HasValue)
            {
                int dem = (int)(di.Value.Date - den.Value.Date).TotalDays;
                if (conTrong.Value)
                    sb.Append("<div class=\"tot\">Phòng còn trống từ ").Append(den.Value.ToString("dd/MM/yyyy"))
                      .Append(" đến ").Append(di.Value.ToString("dd/MM/yyyy")).Append(" (").Append(dem).Append(" đêm): <b>")
                      .Append(Gia(p.GiaMoiDem * dem)).Append("</b></div>");
                else
                    sb.Append(Loi("Phòng không còn trống trọn khoảng ngày này. Hãy thử ngày khác hoặc xem phòng khác ở trang Tìm phòng."));
            }
            return sb.ToString();
        }

        public static string TimPhong(DateTime den, DateTime di, IList<ThongTinPhong> ds, string loi)
        {
            int dem = Math.Max(1, (int)(di.Date - den.Date).TotalDays);
            var sb = new StringBuilder("<h1>Tìm phòng trống</h1><form method=\"get\" class=\"bieumau\">")
              .Append("<label>Ngày đến<input type=\"date\" name=\"den\" value=\"").Append(den.ToString("yyyy-MM-dd")).Append("\"></label>")
              .Append("<label>Ngày đi<input type=\"date\" name=\"di\" value=\"").Append(di.ToString("yyyy-MM-dd")).Append("\"></label>")
              .Append("<button>Tìm phòng</button></form>");
            if (loi != null) return sb.Append(Loi(loi)).ToString();
            sb.Append("<p>").Append(ds.Count).Append(" phòng trống từ ").Append(den.ToString("dd/MM/yyyy"))
              .Append(" đến ").Append(di.ToString("dd/MM/yyyy")).Append(" (").Append(dem).Append(" đêm)</p>");
            if (ds.Count > 0)
            {
                sb.Append("<table><thead><tr><th>Phòng</th><th>Tầng</th><th>Loại</th><th class=\"s\">Giá/đêm</th><th class=\"s\">Tổng</th></tr></thead><tbody>");
                foreach (ThongTinPhong p in ds)
                    sb.Append("<tr><td><a href=\"/phong/").Append(Uri.EscapeDataString(p.SoPhong)).Append("?den=").Append(den.ToString("yyyy-MM-dd"))
                      .Append("&amp;di=").Append(di.ToString("yyyy-MM-dd")).Append("\">").Append(E(p.SoPhong)).Append("</a></td><td>")
                      .Append(p.Tang).Append("</td><td>").Append(E(p.TenLoai)).Append("</td><td class=\"s\">").Append(Gia(p.GiaMoiDem))
                      .Append("</td><td class=\"s\">").Append(Gia(p.GiaMoiDem * dem)).Append("</td></tr>");
                sb.Append("</tbody></table>");
            }
            return sb.ToString();
        }

        private const string Css =
            ":root{--nen:#f8f4ed;--chu:#3e2f26;--phu:#826f61;--be:#fffcf7;--vien:#e7dbcc;--nhan:#b15237;--tot:#e6f3e6}"
            + "@media(prefers-color-scheme:dark){:root{--nen:#1f1a16;--chu:#f1e7da;--phu:#b9a897;--be:#2a231d;--vien:#473b30;--nhan:#e0805f;--tot:#25402a}}"
            + "*{box-sizing:border-box}body{margin:0;font:16px/1.5 system-ui,sans-serif;background:var(--nen);color:var(--chu)}"
            + "header{display:flex;justify-content:space-between;align-items:center;gap:8px;flex-wrap:wrap;padding:12px 16px;background:var(--be);border-bottom:1px solid var(--vien)}"
            + ".logo{font-weight:700;font-size:1.2rem;color:var(--nhan);text-decoration:none}nav a{margin-left:14px;color:var(--chu)}"
            + "main{max-width:720px;margin:0 auto;padding:16px}h1{font-size:1.5rem;margin:.4em 0}h2{font-size:1.1rem;margin:.2em 0}"
            + ".luoi{display:grid;gap:12px}.the{background:var(--be);border:1px solid var(--vien);border-radius:12px;padding:14px 16px;margin:10px 0}"
            + ".gia{font-size:1.6rem;font-weight:700;color:var(--nhan);margin:.2em 0}.gia small{font-size:.9rem;font-weight:400;color:var(--phu)}.phu{color:var(--phu)}"
            + ".bieumau{display:flex;gap:10px;flex-wrap:wrap;align-items:flex-end;margin:10px 0}.bieumau label{display:flex;flex-direction:column;font-size:.9rem;color:var(--phu)}"
            + "input,button{font:inherit;padding:8px 10px;border-radius:8px;border:1px solid var(--vien);background:var(--be);color:var(--chu)}button{background:var(--nhan);color:#fff;border-color:var(--nhan);cursor:pointer}"
            + "table{width:100%;border-collapse:collapse;background:var(--be);border-radius:10px;overflow:hidden}th,td{padding:8px 10px;border-bottom:1px solid var(--vien);text-align:left}th.s,td.s{text-align:right}a{color:var(--nhan)}"
            + ".canhbao{background:#fdecea;color:#7a1f1a;border-radius:10px;padding:10px 14px;margin:10px 0}.tot{background:var(--tot);border-radius:10px;padding:10px 14px;margin:10px 0}"
            + "footer{max-width:720px;margin:0 auto;padding:16px;color:var(--phu);font-size:.85rem}";
    }
}
