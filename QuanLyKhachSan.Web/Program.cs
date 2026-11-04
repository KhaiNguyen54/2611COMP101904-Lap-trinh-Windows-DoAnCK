using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using QuanLyKhachSan.Web;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

// Tiêu đề bảo mật cơ bản cho trang công khai (không dùng script).
app.Use(async (ctx, next) =>
{
    ctx.Response.Headers["X-Content-Type-Options"] = "nosniff";
    ctx.Response.Headers["Referrer-Policy"] = "no-referrer";
    ctx.Response.Headers["X-Frame-Options"] = "DENY";
    ctx.Response.Headers["Content-Security-Policy"] = "default-src 'none'; style-src 'unsafe-inline'; form-action 'self'; base-uri 'none'; frame-ancestors 'none'";
    await next();
});

app.MapGet("/", (HttpContext http) => Chay(http, () =>
{
    DateTime den, di;
    string loi = DocKhoangNgay(http, out den, out di);
    var ds = new System.Collections.Generic.List<ThongTinPhong>();
    if (loi == null)
    {
        try { ds = TraCuuService.LayPhongTrong(den, di); }
        catch (ArgumentException ex) { loi = ex.Message; }
    }
    return Html(http, 200, "Tìm phòng", TrangHtml.TimPhong(den, di, ds, loi));
}));

app.MapGet("/gia", (HttpContext http) => Chay(http, () =>
    Html(http, 200, "Bảng giá", TrangHtml.BangGia(TraCuuService.LayBangGia()))));

app.MapGet("/phong/{so}", (string so, HttpContext http) => Chay(http, () =>
{
    ThongTinPhong p = TraCuuService.LayPhong(so);
    if (p == null)
        return Html(http, 404, "Không tìm thấy", TrangHtml.Loi("Không tìm thấy phòng này. Vui lòng kiểm tra lại mã QR hoặc hỏi lễ tân."));

    DateTime den, di;
    bool coNgay = http.Request.Query.ContainsKey("den") || http.Request.Query.ContainsKey("di");
    if (!coNgay)
        return Html(http, 200, "Phòng " + p.SoPhong, TrangHtml.ChiTietPhong(p, null, null, null));

    string loi = DocKhoangNgay(http, out den, out di);
    if (loi != null)
        return Html(http, 200, "Phòng " + p.SoPhong, TrangHtml.ChiTietPhong(p, null, null, null) + TrangHtml.Loi(loi));
    bool conTrong = false;
    try
    {
        foreach (ThongTinPhong t in TraCuuService.LayPhongTrong(den, di))
            if (string.Equals(t.SoPhong, p.SoPhong, StringComparison.OrdinalIgnoreCase)) { conTrong = true; break; }
    }
    catch (ArgumentException ex)
    {
        return Html(http, 200, "Phòng " + p.SoPhong, TrangHtml.ChiTietPhong(p, null, null, null) + TrangHtml.Loi(ex.Message));
    }
    return Html(http, 200, "Phòng " + p.SoPhong, TrangHtml.ChiTietPhong(p, den, di, conTrong));
}));

// JSON cho ứng dụng khác (ví dụ ứng dụng di động sau này).
app.MapGet("/api/gia", (HttpContext http) => Chay(http, () => Results.Json(TraCuuService.LayBangGia())));
app.MapGet("/api/phong/{so}", (string so, HttpContext http) => Chay(http, () =>
{
    ThongTinPhong p = TraCuuService.LayPhong(so);
    return p == null ? Results.NotFound(new { loi = "Không tìm thấy phòng." }) : Results.Json(p);
}));
app.MapGet("/api/phong-trong", (HttpContext http) => Chay(http, () =>
{
    DateTime den, di;
    string loi = DocKhoangNgay(http, out den, out di);
    if (loi != null) return Results.BadRequest(new { loi });
    try { return Results.Json(TraCuuService.LayPhongTrong(den, di)); }
    catch (ArgumentException ex) { return Results.BadRequest(new { loi = ex.Message }); }
}));

app.Run();

// ---- Hàm dùng chung ----
static IResult Html(HttpContext http, int ma, string tieuDe, string noiDung)
{
    return Results.Content(TrangHtml.Trang(tieuDe, noiDung), "text/html; charset=utf-8", System.Text.Encoding.UTF8, ma);
}

/// <summary>Đọc ?den=&di= từ yêu cầu (xem KhoangNgay.Doc).</summary>
static string DocKhoangNgay(HttpContext http, out DateTime den, out DateTime di)
{
    return KhoangNgay.Doc(http.Request.Query["den"], http.Request.Query["di"], DateTime.Today, out den, out di);
}

/// <summary>Chạy xử lý; lỗi cơ sở dữ liệu chỉ ghi log nội bộ, khách chỉ thấy thông báo chung (không lộ chuỗi kết nối).</summary>
static IResult Chay(HttpContext http, Func<IResult> xuLy)
{
    try { return xuLy(); }
    catch (Exception ex)
    {
        http.RequestServices.GetRequiredService<ILoggerFactory>().CreateLogger("TraCuu").LogError(ex, "Lỗi tra cứu");
        return Html(http, 503, "Tạm thời không khả dụng", TrangHtml.Loi("Hệ thống tạm thời không truy cập được dữ liệu. Vui lòng thử lại sau hoặc liên hệ lễ tân."));
    }
}
