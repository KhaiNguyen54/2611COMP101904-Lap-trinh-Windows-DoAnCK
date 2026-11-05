# Hướng dẫn sử dụng và kịch bản demo

## Chuẩn bị
1. Chạy `QuanLyKhachSan/Scripts/Database.sql` trên SQL Server (nên dùng `sqlcmd -f 65001 -i Database.sql` để giữ đúng tiếng Việt).
2. Kiểm tra `QuanLyKhachSan/appsettings.json` trỏ đúng máy chủ.
3. Mở `QuanLyKhachSan.sln` bằng Visual Studio 2022+ (.NET 10 SDK), chạy dự án `QuanLyKhachSan`.
4. Tài khoản mẫu: xem phần dữ liệu mẫu trong `Database.sql` (Quản lý `admin`, Lễ tân `letan`).

5. (Tùy chọn, cho mã QR) Chạy `dotnet run --project QuanLyKhachSan.Web --urls http://localhost:5080`; muốn điện thoại quét được, đặt `Web:BaseUrl` trong `QuanLyKhachSan/appsettings.json` thành IP nội bộ của máy chạy web và mở cổng 5080 trên tường lửa.

## Phím tắt
F2 Đặt phòng · F3 Nhận phòng · F4 Ghi dịch vụ · F5 Sơ đồ phòng · F6 Trả phòng · F7 Hóa đơn · F8 Đổi phòng · Ctrl+D Giao diện sáng/tối.

## Kịch bản demo (khoảng 10 phút)
1. Đăng nhập Lễ tân: xem màn hình chính (thẻ thống kê, sơ đồ theo tầng, "Vận hành hôm nay").
2. **Đặt phòng** (F2): chọn khách (hoặc "Khách mới…"), chọn ngày, phòng trống, cọc; thử đặt trùng lịch để thấy báo lỗi.
3. **Nhận phòng** (F3): nhận từ đặt trước; thử thêm một khách vãng lai.
4. **Ghi dịch vụ** (F4): ghi nước/giặt ủi, xóa một dòng.
5. **Trả phòng** (F6): xem bảng tính tiền, thử giảm giá > 10% (Lễ tân phải nhờ Quản lý nhập tài khoản duyệt), trả phòng và in hóa đơn.
6. **Sơ đồ phòng** (F5): phòng vừa trả ở "Dọn dẹp" → bấm "Đã dọn xong".
7. **Giao diện tối**: nhấn Ctrl+D, mở thêm vài cửa sổ để thấy cả hai chế độ; đóng và mở lại ứng dụng để thấy lựa chọn được nhớ.
8. **Mã QR**: Tra cứu → Mã QR phòng và giá, chọn một phòng, lưu PNG, quét bằng điện thoại và xem trang phòng; thử chọn ngày đến/đi.
9. Đăng xuất, đăng nhập Quản lý: Báo cáo doanh thu (đổi cách gom), công suất, in, xuất CSV; chuyển một phòng Trống sang Bảo trì; thử Quản lý tài khoản.
