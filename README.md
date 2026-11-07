# Quản lý khách sạn

Ứng dụng desktop quản lý hoạt động khách sạn, xây dựng bằng C# Windows Forms và SQL Server. Project theo mô hình MVP (Model – View – Presenter): View là các Form, Presenter điều phối luồng nghiệp vụ, Service và Repository xử lý nghiệp vụ và truy cập dữ liệu.

## Chức năng

- Đăng nhập (khóa tạm sau 5 lần sai), đăng xuất, đổi mật khẩu, quản lý tài khoản; menu và quyền theo vai trò Lễ tân/Quản lý.
- Quản lý loại phòng, phòng, khách hàng, dịch vụ (Lễ tân chỉ xem ba danh mục đầu).
- Đặt phòng với kiểm tra trùng lịch và cọc tối đa; hủy đặt, đánh dấu khách không đến; nhận phòng từ đặt trước hoặc khách vãng lai; đổi phòng khi đang ở (F8) có lịch sử và giữ nguyên đơn giá phiếu thuê.
- Ghi/xóa dịch vụ khi đang lưu trú; trả phòng có bảng tính tiền, phụ thu trả muộn, giảm giá có giới hạn, hóa đơn trong transaction.
- Sơ đồ phòng theo tầng (dọn phòng, bảo trì), danh sách khách đang ở.
- Tra cứu, xem trước và in hóa đơn; báo cáo doanh thu (ngày/tháng/loại phòng) và công suất phòng, có biểu đồ, in và xuất CSV.
- Giao diện sáng/tối: menu **Hệ thống → Giao diện tối** (Ctrl+D), áp dụng ngay cho mọi cửa sổ và được nhớ cho lần mở sau (lưu tại `%AppData%\QuanLyKhachSan\giaodien.txt`). Bản in báo cáo luôn dùng nền sáng.
- Logo Autumn Hotel (lá thu, mái nhà và cửa): `QuanLyKhachSan/Resources/logo.svg|png|ico`; dùng làm biểu tượng ứng dụng, đầu trang chủ và cổng web (`/logo.svg`).
- Mã QR tra cứu: menu **Tra cứu → Mã QR phòng và giá** tạo QR cho từng phòng và bảng giá (lưu PNG, in, xuất tất cả, sao chép liên kết). Bộ mã hóa QR tự viết, không cần thư viện ngoài.
- Cổng web cho khách (`QuanLyKhachSan.Web`): quét QR để xem giá, tình trạng và phòng trống theo ngày (lọc theo loại và giá tối đa, có hình minh họa phòng); chỉ đọc, không hiển thị thông tin khách.
- Phím tắt: F2 đặt phòng, F3 nhận phòng, F4 dịch vụ, F5 sơ đồ, F6 trả phòng, F7 hóa đơn, F8 đổi phòng. Hướng dẫn demo: [`docs/09_HuongDan_Demo.md`](docs/09_HuongDan_Demo.md).

Phạm vi đã triển khai có thể thay đổi theo phiên bản source hiện tại. Các tài liệu yêu cầu, quy tắc, use case và kịch bản được tổng hợp tại [`docs/README.md`](docs/README.md).

## Công nghệ và yêu cầu

- Windows 10/11.
- Visual Studio 2026 (phiên bản 18.0 trở lên) với workload **.NET desktop development**; Visual Studio 2022 (17.x) không build được project target `net10.0`. Có thể dùng riêng .NET 10 SDK (project target `net10.0-windows`) và chạy bằng dòng lệnh.
- SQL Server Express, LocalDB hoặc SQL Server có quyền tạo database và bảng; bộ kiểm thử tự động chạy trên SQL Server 2022.
- Kết nối Windows Authentication theo mặc định; máy chạy ứng dụng cần truy cập được SQL Server.

NuGet dependency được khai báo trong project: `Microsoft.Data.SqlClient` 5.2.2 (cả ứng dụng desktop và cổng web). Cổng web dùng ASP.NET Core đi kèm .NET 10 SDK; mã QR do bộ mã hóa tự viết trong `Services/QrCode.cs` nên không cần thư viện QR. Khi restore/build lần đầu cần có kết nối NuGet.

## Cài đặt và chạy

### 1. Tạo database

Mở `QuanLyKhachSan.sln` trong Visual Studio, hoặc chạy từ thư mục chứa solution:

```powershell
sqlcmd -S localhost -E -C -b -f 65001 -i .\QuanLyKhachSan\Scripts\Database.sql
```

Có thể mở `QuanLyKhachSan/Scripts/Database.sql` bằng SQL Server Management Studio và chọn **Execute**. Script tạo database `QuanLyKhachSan`, schema, stored procedure, index và dữ liệu mẫu; script được thiết kế để chạy lại mà không xóa dữ liệu hiện có.

Nếu đã tạo CSDL từ phiên bản trước, chạy lại `Database.sql` để bổ sung bảng `LichSuDoiPhong` và `sp_DoiPhong` (đổi phòng khi đang ở); script không xóa dữ liệu hiện có.

Bộ test case dạng bảng tính (giao diện, cổng web, CSDL và nhóm kiểm thử tự động): [`TestCases/TestCase.xlsx`](TestCases/TestCase.xlsx).

Kiểm thử tự động trên GitHub Actions (`.github/workflows/kiem-thu.yml`): chạy `Database.sql` hai lần trên SQL Server 2022, 34 ca trong `Scripts/KiemThu.sql`, build toàn bộ solution trên Windows và bộ test 208 ca.

### 2. Cấu hình kết nối

Chuỗi mặc định trong [`QuanLyKhachSan/appsettings.json`](QuanLyKhachSan/appsettings.json) dùng SQL Server instance mặc định trên máy (`localhost`) và Windows Authentication. Nếu instance khác, thay giá trị `ConnectionStrings:KhachSan`, ví dụ:
```json
{
  "ConnectionStrings": {
    "KhachSan": "Server=localhost;Database=QuanLyKhachSan;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
```

Tài khoản chạy ứng dụng cần quyền truy cập database. Không lưu mật khẩu SQL thật trong Git; nếu dùng SQL Authentication, giữ cấu hình riêng trên máy phát triển.

### 3. Khởi động nhanh

Từ thư mục gốc của repository:

```powershell
dotnet restore .\QuanLyKhachSan.sln
dotnet build .\QuanLyKhachSan.sln
```

#### 3.1. Chạy ứng dụng desktop quản lý khách sạn

```powershell
dotnet run --project .\QuanLyKhachSan\QuanLyKhachSan.csproj
```

Hoặc mở solution bằng Visual Studio, đặt `QuanLyKhachSan` làm startup project và nhấn **F5**.

#### 3.2. Chạy cổng web tra cứu dành cho khách

```powershell
dotnet run --project .\QuanLyKhachSan.Web\QuanLyKhachSan.Web.csproj --urls http://localhost:5080
```

Sau khi chạy, mở trình duyệt và truy cập:

```text
http://localhost:5080/
```

Các đường dẫn chính:

- `/` → tìm phòng trống
- `/gia` → bảng giá
- `/phong/{soPhong}` → chi tiết phòng
- `/api/gia` → JSON bảng giá
- `/api/phong/{soPhong}` → JSON chi tiết phòng
- `/api/phong-trong` → JSON phòng trống theo ngày

> Lưu ý: nếu chạy bằng `dotnet run` mà web không mở được, hãy đảm bảo dùng lệnh có `--urls http://localhost:5080`, vì đây là cổng mặc định trong project web. Nếu cần truy cập từ điện thoại cùng mạng LAN, đổi `Web:BaseUrl` trong [QuanLyKhachSan/appsettings.json](QuanLyKhachSan/appsettings.json) thành IP nội bộ của máy chạy web (ví dụ `http://192.168.1.20:5080`) và mở cổng 5080 trên tường lửa.

## Tài khoản dữ liệu mẫu

| Vai trò | Tên đăng nhập | Mật khẩu ban đầu |
|---|---|---|
| Quản lý | `admin` | `Admin@123` |
| Lễ tân | `letan` | `Admin@123` |

Mật khẩu mẫu được lưu dạng hash PBKDF2-SHA256 cùng salt; hai tài khoản dùng cùng mật khẩu khởi tạo để đăng nhập lần đầu. Đổi mật khẩu sau khi đăng nhập. Không sử dụng thông tin mẫu trong môi trường thật.

## Kiến trúc và cấu trúc

```text
QuanLyKhachSan/
├── QuanLyKhachSan.sln
├── README.md
├── docs/                         # Yêu cầu, quy trình, quy tắc và kịch bản
├── Latex/                        # Báo cáo LaTeX và ảnh minh họa
├── QuanLyKhachSan.Tests/         # Kiểm thử logic tự động (không cần CSDL)
├── QuanLyKhachSan.Web/           # Cổng web tra cứu (ASP.NET Core, chỉ đọc)
└── QuanLyKhachSan/
    ├── Views/                    # Form WinForms và interface View (IXxxView); GiaoDien.cs: bộ màu sáng/tối
    ├── Presenters/               # Presenter điều phối giao diện và nghiệp vụ, không dùng WinForms
    ├── Services/                 # Nghiệp vụ và interface IXxxService; QrCode.cs, TraCuuUrl.cs cho mã QR
    ├── Repositories/             # Truy vấn SQL, stored procedure, transaction
    ├── Models/                   # Đối tượng dữ liệu
    ├── Scripts/Database.sql      # Schema, dữ liệu mẫu, index, stored procedure
    ├── appsettings.json          # Kết nối CSDL và Web:BaseUrl cho mã QR
    ├── Program.cs
    └── QuanLyKhachSan.csproj
```

Cổng web biên dịch lại một số tệp Models/Repositories/Services của ứng dụng desktop (khai báo trong `QuanLyKhachSan.Web.csproj`) nên không nhân đôi nghiệp vụ.

Luồng gọi chính: **View → Presenter → Service → Repository → SQL Server**. View chỉ hiển thị và chuyển sự kiện cho Presenter qua interface; Presenter không tham chiếu WinForms nên kiểm thử được bằng View giả. Model dùng để trao đổi dữ liệu giữa các lớp. Các truy vấn Repository sử dụng tham số SQL.

## Kiểm tra nhanh

Sau khi build thành công và database đã kết nối được:

1. Đăng nhập bằng từng vai trò; xác nhận menu quản lý chỉ mở với tài khoản Quản lý.
2. Tạo hai lượt đặt cùng phòng có ngày giao nhau; lượt sau phải bị từ chối. Hai lượt nối tiếp cùng ngày trả/nhận phải được chấp nhận.
3. Nhận phòng, ghi một dịch vụ, trả phòng; kiểm tra hóa đơn gồm tiền phòng, dịch vụ, phụ thu/giảm giá và tiền cọc.
4. Mở lại hóa đơn để xem trước và in; xác nhận trả phòng lần hai không tạo hóa đơn mới.
5. Đổi bộ lọc thời gian trên báo cáo và đối chiếu với hóa đơn trong khoảng đã chọn.
6. Nhấn Ctrl+D để đổi giao diện sáng/tối; mở thêm vài cửa sổ và xác nhận chữ vẫn đọc được, đóng/mở lại ứng dụng để xác nhận lựa chọn được nhớ.
7. Tra cứu → Mã QR phòng và giá: tạo QR cho một phòng, quét bằng điện thoại cùng mạng và xác nhận mở đúng trang phòng của cổng web.

Kiểm thử logic tự động: `dotnet run --project QuanLyKhachSan.Tests` (quy tắc tính tiền, gom báo cáo, CSV, bộ mã hóa QR, đường dẫn tra cứu và logic cổng web và các Presenter MVP chạy với View giả; 208 ca).

Kịch bản chi tiết và trạng thái xác minh được ghi tại [`docs/07_KichBanKiemThu.md`](docs/07_KichBanKiemThu.md). Các bước trên là hướng dẫn kiểm thử thủ công; chưa hàm ý rằng mọi kịch bản giao diện đã được chạy tự động.

## Bảo mật và dữ liệu

- Dùng tham số cho câu truy vấn SQL; mật khẩu tài khoản mẫu lưu dưới dạng hash, không lưu rõ.
- Tài khoản mẫu có mật khẩu mặc định dùng chung; cần đổi ngay khi đăng nhập.
- `appsettings.json` hiện chứa cấu hình mặc định Windows Authentication. Không commit thông tin đăng nhập riêng hoặc dữ liệu khách thật.
- Cổng web tra cứu chỉ đọc và không hiển thị thông tin khách, nhưng mặc định chạy HTTP, không xác thực và lắng nghe mọi địa chỉ; chỉ dùng trong mạng nội bộ, nên cấp tài khoản SQL chỉ có quyền đọc, và thêm HTTPS trước khi mở ra Internet.
- Script khởi tạo có thể chạy lại và thêm dữ liệu mẫu còn thiếu; sao lưu database trước khi thử chỉnh sửa script trên database đang dùng.

## Tài liệu

Xem [mục lục tài liệu](docs/README.md) để đọc theo thứ tự giai đoạn, từ phạm vi và quy trình đến thiết kế database và kịch bản kiểm thử.
