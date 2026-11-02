# Kịch bản kiểm thử và nhật ký xác minh

> Các kết quả SQL/build bên dưới là ghi nhận đã có trong tài liệu trước lần hoàn thiện này; chúng chưa được chạy lại trong lần cập nhật tài liệu và không kèm log độc lập tại đây. Các kịch bản giao diện là bước kiểm thử thủ công, chưa phải test tự động.

## Kiểm thử SQL đã chạy trên SQL Server local

Các thao tác tạo dữ liệu kiểm thử được bao trong transaction và rollback sau khi assert, không giữ bản ghi thử.

| ID | Kịch bản | Kết quả |
|---|---|---|
| DB-01 | Chạy `Scripts/Database.sql` trên database thử nghiệm chưa tồn tại | Ghi nhận trước đây: tạo database, 9 bảng, 4 stored procedure và dữ liệu mẫu; cần chạy lại để xác nhận |
| DB-02 | Chạy lại script trên database thử nghiệm | Ghi nhận trước đây: chạy lại không tạo trùng object/seed; cần chạy lại để xác nhận |
| DB-03 | Kiểm tra lịch của phiếu thuê đang ở | Ghi nhận trước đây: `sp_KiemTraTrungLich` phát hiện giao nhau; cần chạy lại để xác nhận |
| DB-04 | Thêm dịch vụ và trả phòng trong transaction thử nghiệm | Ghi nhận trước đây: hóa đơn 400.000 tiền phòng + 30.000 dịch vụ = 430.000; cần chạy lại để xác nhận |
| DB-05 | Đặt phòng có khoảng ngày giao với đặt phòng đang hiệu lực | Ghi nhận trước đây: bị từ chối do trùng; cần chạy lại để xác nhận |
| DB-06 | Chạy lại script và kiểm tra index | Ghi nhận trước đây: script chạy hai lần; cần chạy lại để xác nhận index lọc và index tra cứu |
| BUILD-01 | `dotnet build QuanLyKhachSan.sln --no-restore` (target `net10.0-windows`) | Ghi nhận trước đây: 0 cảnh báo, 0 lỗi; cần build lại trên môi trường có .NET 10 SDK để xác nhận |

## Kiểm thử GUI cần thực hiện trên máy có SQL Server cấu hình đúng

| ID | Kịch bản | Kết quả mong đợi |
|---|---|---|
| UI-01 | Đăng nhập sai / tài khoản bị khóa | Từ chối đăng nhập, không mở màn hình chính |
| UI-02 | Đăng nhập admin và lễ tân | Menu quản lý tài khoản, danh mục cấu hình và báo cáo chỉ hiện với Quản lý |
| UI-03 | Đổi mật khẩu sai mật khẩu hiện tại hoặc xác nhận không khớp | Không cập nhật mật khẩu |
| UI-04 | Tạo đặt phòng có ngày đến >= ngày đi hoặc trùng lịch | Báo lỗi, không lưu đặt phòng |
| UI-05 | Nhận phòng, ghi dịch vụ, trả phòng | Tạo phiếu, lưu snapshot đơn giá, tạo hóa đơn một lần và chuyển phòng sang Dọn dẹp |
| UI-06 | Trả phòng lần nữa cùng phiếu | Từ chối, không tạo hóa đơn thứ hai |
| UI-07 | Tìm và xem trước/in hóa đơn | Nội dung in khớp hóa đơn được chọn |
| UI-08 | Tạo khách với giấy tờ/điện thoại sai định dạng | BLL báo lỗi trước khi gọi DAL |
| UI-09 | Trả phòng rồi xác nhận hoàn tất dọn phòng trên sơ đồ | Phòng ở trạng thái Dọn dẹp không được nhận khách; sau khi xác nhận chuyển sang Trống hoặc Đã đặt nếu có lịch sắp tới |
| UI-10 | Đặt phòng tương lai cho phòng đang có khách hoặc đang dọn | Chỉ cho chọn phòng nếu lịch không giao nhau; phòng đang dọn hoặc bảo trì không được chọn cho đến khi hoàn tất xử lý |

Các kịch bản UI trong bảng là các bước cần thực hiện thủ công. Cấu hình mặc định trong `appsettings.json` cần được đối chiếu với instance SQL Server thực tế. Chỉ chạy DB-01/02 trên database thử nghiệm hoặc sau khi sao lưu; không dùng database đang có dữ liệu thật. Không suy ra kết quả UI từ việc script SQL hoặc build thành công.

## Kiểm thử logic tự động (không cần CSDL)

Project `QuanLyKhachSan.Tests` kiểm tra số đêm tối thiểu (QT40), phụ thu trả muộn theo mốc 1/3/6 giờ, tổng thanh toán không âm (QT45), cọc tối đa (QT15), gom doanh thu theo ngày/tháng/loại phòng (kể cả ngày trống), tỷ lệ công suất trung bình và xuất CSV (thoát dấu phẩy, nháy kép, chặn công thức). Chạy: `dotnet run --project QuanLyKhachSan.Tests` (mã thoát 1 nếu có ca lỗi).

Kết quả trong phiên hoàn thiện này: 26/26 ca đạt khi biên dịch bằng Mono; chưa chạy bằng `dotnet` vì môi trường không có .NET SDK. Giao diện được chụp bằng dữ liệu giả để kiểm tra chồng chéo/cắt chữ, không thay thế các kịch bản UI-01…UI-12.

| ID | Kịch bản bổ sung | Kết quả mong đợi |
|---|---|---|
| UI-11 | Đánh dấu "Khách không đến" / hủy đặt rồi đặt lại cùng phòng và ngày | Phòng được giải phóng, đặt được lại |
| UI-12 | Lễ tân mở Loại phòng/Phòng/Dịch vụ; Quản lý mở Báo cáo công suất | Lễ tân chỉ xem; Quản lý thấy biểu đồ, in và xuất CSV |
