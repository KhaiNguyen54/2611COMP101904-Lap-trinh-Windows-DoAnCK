# Quản lý khách sạn

Ứng dụng desktop quản lý hoạt động khách sạn, xây dựng bằng C# Windows Forms và SQL Server. Project dùng các lớp DTO, DAL và BLL để tách giao diện, nghiệp vụ và truy cập dữ liệu.

## Chức năng

- Đăng nhập (khóa tạm sau 5 lần sai), đăng xuất, đổi mật khẩu, quản lý tài khoản; menu và quyền theo vai trò Lễ tân/Quản lý.
- Quản lý loại phòng, phòng, khách hàng, dịch vụ (Lễ tân chỉ xem ba danh mục đầu).
- Đặt phòng với kiểm tra trùng lịch và cọc tối đa; hủy đặt, đánh dấu khách không đến; nhận phòng từ đặt trước hoặc khách vãng lai.
- Ghi/xóa dịch vụ khi đang lưu trú; trả phòng có bảng tính tiền, phụ thu trả muộn, giảm giá có giới hạn, hóa đơn trong transaction.
- Sơ đồ phòng theo tầng (dọn phòng, bảo trì), danh sách khách đang ở.
- Tra cứu, xem trước và in hóa đơn; báo cáo doanh thu (ngày/tháng/loại phòng) và công suất phòng, có biểu đồ, in và xuất CSV.
- Phím tắt: F2 đặt phòng, F3 nhận phòng, F4 dịch vụ, F5 sơ đồ, F6 trả phòng, F7 hóa đơn. Hướng dẫn demo: [`docs/09_HuongDan_Demo.md`](docs/09_HuongDan_Demo.md).

Phạm vi đã triển khai có thể thay đổi theo phiên bản source hiện tại. Các tài liệu yêu cầu, quy tắc, use case và kịch bản được tổng hợp tại [`docs/README.md`](docs/README.md).

## Công nghệ và yêu cầu

- Windows 10/11.
- .NET 10 SDK (project target `net10.0-windows`) hoặc Visual Studio có hỗ trợ .NET 10 và workload **.NET desktop development**.
- SQL Server Express, LocalDB hoặc SQL Server có quyền tạo database và bảng.
- Kết nối Windows Authentication theo mặc định; máy chạy ứng dụng cần truy cập được SQL Server.

NuGet dependency được khai báo trong project: `Microsoft.Data.SqlClient` 5.2.2. Khi restore/build lần đầu cần có kết nối NuGet.

## Cài đặt và chạy

### 1. Tạo database

Mở `QuanLyKhachSan.sln` trong Visual Studio, hoặc chạy từ thư mục chứa solution:

```powershell
sqlcmd -S localhost -E -C -b -f 65001 -i .\QuanLyKhachSan\Scripts\Database.sql
```

Có thể mở `QuanLyKhachSan/Scripts/Database.sql` bằng SQL Server Management Studio và chọn **Execute**. Script tạo database `QuanLyKhachSan`, schema, stored procedure, index và dữ liệu mẫu; script được thiết kế để chạy lại mà không xóa dữ liệu hiện có.

### 2. Cấu hình kết nối

Chuỗi mặc định trong [`QuanLyKhachSan/appsettings.json`](QuanLyKhachSan/appsettings.json) dùng SQL Server instance mặc định trên máy (`localhost`) và Windows Authentication. Nếu instance của bạn khác, thay giá trị `ConnectionStrings:KhachSan`, ví dụ:
```json
{
  "ConnectionStrings": {
    "KhachSan": "Server=localhost;Database=QuanLyKhachSan;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
```

Tài khoản chạy ứng dụng cần quyền truy cập database. Không lưu mật khẩu SQL thật trong Git; nếu dùng SQL Authentication, giữ cấu hình riêng trên máy phát triển.

### 3. Build và chạy

```powershell
dotnet restore .\QuanLyKhachSan.sln
dotnet build .\QuanLyKhachSan.sln
dotnet run --project .\QuanLyKhachSan\QuanLyKhachSan.csproj
```

Hoặc mở solution bằng Visual Studio, đặt `QuanLyKhachSan` làm startup project và nhấn **F5**.

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
└── QuanLyKhachSan/
    ├── BLL/                      # Kiểm tra và xử lý nghiệp vụ
    ├── DAL/                      # Truy vấn SQL, stored procedure, transaction
    ├── DTO/                      # Đối tượng truyền dữ liệu
    ├── GUI/                      # WinForms và UserControl
    ├── Scripts/Database.sql      # Schema, dữ liệu mẫu, index, stored procedure
    ├── appsettings.json          # Cấu hình kết nối mặc định
    ├── Program.cs
    └── QuanLyKhachSan.csproj
```

Luồng gọi chính: **GUI → BLL → DAL → SQL Server**. DTO được dùng để trao đổi dữ liệu giữa các tầng. Các truy vấn DAL sử dụng tham số SQL.

## Kiểm tra nhanh

Sau khi build thành công và database đã kết nối được:

1. Đăng nhập bằng từng vai trò; xác nhận menu quản lý chỉ mở với tài khoản Quản lý.
2. Tạo hai lượt đặt cùng phòng có ngày giao nhau; lượt sau phải bị từ chối. Hai lượt nối tiếp cùng ngày trả/nhận phải được chấp nhận.
3. Nhận phòng, ghi một dịch vụ, trả phòng; kiểm tra hóa đơn gồm tiền phòng, dịch vụ, phụ thu/giảm giá và tiền cọc.
4. Mở lại hóa đơn để xem trước và in; xác nhận trả phòng lần hai không tạo hóa đơn mới.
5. Đổi bộ lọc thời gian trên báo cáo và đối chiếu với hóa đơn trong khoảng đã chọn.

Kiểm thử logic tự động: `dotnet run --project QuanLyKhachSan.Tests` (quy tắc tính tiền, gom báo cáo, CSV).

Kịch bản chi tiết và trạng thái xác minh được ghi tại [`docs/07_KichBanKiemThu.md`](docs/07_KichBanKiemThu.md). Các bước trên là hướng dẫn kiểm thử thủ công; chưa hàm ý rằng mọi kịch bản giao diện đã được chạy tự động.

## Bảo mật và dữ liệu

- Dùng tham số cho câu truy vấn SQL; mật khẩu tài khoản mẫu lưu dưới dạng hash, không lưu rõ.
- Tài khoản mẫu có mật khẩu mặc định dùng chung; cần đổi ngay khi đăng nhập.
- `appsettings.json` hiện chứa cấu hình mặc định Windows Authentication. Không commit thông tin đăng nhập riêng hoặc dữ liệu khách thật.
- Script khởi tạo có thể chạy lại và thêm dữ liệu mẫu còn thiếu; sao lưu database trước khi thử chỉnh sửa script trên database đang dùng.

## Tài liệu

Xem [mục lục tài liệu](docs/README.md) để đọc theo thứ tự giai đoạn, từ phạm vi và quy trình đến thiết kế database và kịch bản kiểm thử.
