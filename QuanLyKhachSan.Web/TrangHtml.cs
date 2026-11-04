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

        /// <summary>Logo Autumn Hotel (lá thu có mái nhà và cửa); trùng với Resources/logo.svg của ứng dụng.</summary>
        public const string LogoSvg =
            "<svg xmlns=\"http://www.w3.org/2000/svg\" viewBox=\"0 0 64 64\">"
            + "<rect width=\"64\" height=\"64\" rx=\"15\" fill=\"#17382F\"/>"
            + "<path d=\"M32 8.5C47 18 51.5 34 32 56C12.5 34 17 18 32 8.5Z\" fill=\"#E0A03C\"/>"
            + "<path d=\"M20.5 37.5 32 26 43.5 37.5\" fill=\"none\" stroke=\"#17382F\" stroke-width=\"3.4\" stroke-linecap=\"round\" stroke-linejoin=\"round\"/>"
            + "<path d=\"M28.2 56V45.5a3.8 3.8 0 0 1 7.6 0V56Z\" fill=\"#17382F\"/></svg>";

        public static string Trang(string tieuDe, string noiDung)
        {
            return "<!doctype html><html lang=\"vi\"><head><meta charset=\"utf-8\">"
                + "<meta name=\"viewport\" content=\"width=device-width,initial-scale=1\">"
                + "<meta name=\"theme-color\" content=\"#17382F\"><link rel=\"icon\" type=\"image/svg+xml\" href=\"/logo.svg\">"
                + "<title>" + E(tieuDe) + " · Autumn Hotel</title><style>" + Css + "</style></head><body>"
                + "<header><div class=\"thanh\"><a class=\"logo\" href=\"/\" aria-label=\"Autumn Hotel, trang chủ\"><img src=\"/logo.svg\" alt=\"\" width=\"36\" height=\"36\"><span>Autumn Hotel</span></a>"
                + "<nav><a href=\"/\">Tìm phòng</a><a href=\"/gia\">Bảng giá</a></nav></div></header>"
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
            ":root{--pine:#17382f;--nen:#eef3ef;--chu:#14221d;--phu:#566861;--be:#ffffff;--vien:#d5dfd8;--nhan:#9a5a0c;--la:#e0a03c;--nut:#17382f;--nutchu:#ffffff;--tot:#e3f1e6;--totchu:#14522a;--loi:#fbeae6;--loichu:#7a1f1a;--bong:0 1px 2px rgba(20,34,29,.06),0 8px 24px -12px rgba(20,34,29,.18);--bongcao:0 2px 4px rgba(20,34,29,.08),0 16px 32px -14px rgba(20,34,29,.28)}"
            + "@media(prefers-color-scheme:dark){:root{--nen:#0d1714;--chu:#e8efea;--phu:#9eb0a8;--be:#14231e;--vien:#26392f;--nhan:#e8b25a;--nut:#e0a03c;--nutchu:#1a1204;--tot:#173a25;--totchu:#a9e3bb;--loi:#3d1d1a;--loichu:#f4b9b1;--bong:0 1px 2px rgba(0,0,0,.4),0 8px 24px -12px rgba(0,0,0,.6);--bongcao:0 2px 4px rgba(0,0,0,.5),0 16px 32px -14px rgba(0,0,0,.7)}}"
            + "*{box-sizing:border-box}html{scroll-behavior:smooth;-webkit-text-size-adjust:100%}"
            + "body{margin:0;font:16px/1.55 system-ui,-apple-system,'Segoe UI',Roboto,sans-serif;background:var(--nen);color:var(--chu);-webkit-font-smoothing:antialiased}"
            + "header{background:var(--pine);color:#f2f6f3;box-shadow:0 1px 0 rgba(255,255,255,.06) inset,0 6px 20px -10px rgba(0,0,0,.5)}"
            + ".thanh{max-width:720px;margin:0 auto;padding:10px 16px;display:flex;justify-content:space-between;align-items:center;gap:8px 16px;flex-wrap:wrap}"
            + ".logo{display:flex;align-items:center;gap:10px;color:#fff;text-decoration:none;font-weight:650;font-size:1.15rem;letter-spacing:.01em}.logo img{display:block;border-radius:9px}"
            + "nav{display:flex;gap:4px}nav a{color:#d5e3db;text-decoration:none;padding:8px 12px;border-radius:999px;transition:background-color .18s ease,color .18s ease}nav a:hover{background:rgba(255,255,255,.12);color:#fff}"
            + "main{max-width:720px;margin:0 auto;padding:22px 16px 12px;animation:vao .32s ease-out both}@keyframes vao{from{opacity:0;transform:translateY(6px)}to{opacity:1;transform:none}}"
            + "h1{font-size:1.7rem;line-height:1.2;letter-spacing:-.015em;margin:.2em 0 .5em;text-wrap:balance}h2{font-size:1.1rem;line-height:1.3;margin:.2em 0;text-wrap:balance}p{margin:.4em 0}"
            + ".luoi{display:grid;gap:14px;grid-template-columns:repeat(auto-fit,minmax(min(100%,260px),1fr))}"
            + ".the{background:var(--be);border:1px solid var(--vien);border-radius:14px;padding:16px 18px;margin:12px 0;box-shadow:var(--bong);transition:transform .2s ease,box-shadow .2s ease,border-color .2s ease}.luoi .the{margin:0}.the:hover{transform:translateY(-2px);box-shadow:var(--bongcao);border-color:var(--la)}"
            + ".gia{font-size:1.65rem;font-weight:700;color:var(--nhan);margin:.15em 0;font-variant-numeric:tabular-nums}.gia small{font-size:.9rem;font-weight:400;color:var(--phu)}.phu{color:var(--phu)}"
            + ".bieumau{display:flex;gap:12px;flex-wrap:wrap;align-items:flex-end;margin:12px 0 16px}.bieumau label{display:flex;flex-direction:column;gap:4px;font-size:.9rem;color:var(--phu)}"
            + "input,button{font:inherit;min-height:44px;padding:8px 12px;border-radius:10px;border:1px solid var(--vien);background:var(--be);color:var(--chu);transition:border-color .18s ease,box-shadow .18s ease,background-color .18s ease,transform .12s ease}"
            + "input:hover{border-color:var(--phu)}button{background:var(--nut);color:var(--nutchu);border-color:var(--nut);font-weight:600;padding:8px 20px;cursor:pointer}button:hover{box-shadow:var(--bongcao);transform:translateY(-1px)}button:active{transform:none}"
            + "a:focus-visible,input:focus-visible,button:focus-visible{outline:3px solid var(--la);outline-offset:2px}"
            + "table{width:100%;border-collapse:separate;border-spacing:0;background:var(--be);border:1px solid var(--vien);border-radius:14px;overflow:hidden;box-shadow:var(--bong);font-variant-numeric:tabular-nums}"
            + "th,td{padding:11px 12px;border-bottom:1px solid var(--vien);text-align:left}th{font-size:.85rem;color:var(--phu);font-weight:600;background:color-mix(in srgb,var(--vien) 35%,var(--be))}tbody tr{transition:background-color .15s ease}tbody tr:hover{background:color-mix(in srgb,var(--la) 12%,var(--be))}tr:last-child td{border-bottom:0}th.s,td.s{text-align:right;white-space:nowrap}"
            + "a{color:var(--nhan);text-underline-offset:3px}td a{font-weight:650}"
            + ".canhbao{background:var(--loi);color:var(--loichu);border-radius:12px;padding:12px 16px;margin:12px 0}.tot{background:var(--tot);color:var(--totchu);border-radius:12px;padding:12px 16px;margin:12px 0}"
            + "footer{max-width:720px;margin:0 auto;padding:12px 16px 32px;color:var(--phu);font-size:.85rem}"
            + "@media(max-width:480px){.bieumau label{flex:1 1 40%}.bieumau button{flex:1 1 100%}th,td{padding:10px 8px}}"
            + "@media(prefers-reduced-motion:reduce){html{scroll-behavior:auto}*,*::before,*::after{animation:none!important;transition:none!important}}";
    }
}
