# Checklist bảo mật cơ bản

| Hạng mục | Tình trạng | Ghi chú |
|---|---|---|
| Truy vấn có tham số (không nối chuỗi từ người dùng) | Đạt khi rà soát DAL | Từ khóa tìm kiếm dùng `SqlParameter`; bộ lọc `RowFilter` ở `FrmKhachDangO` đã thoát ký tự đặc biệt |
| Băm mật khẩu | Đạt | PBKDF2 kèm salt riêng, số vòng lặp lưu trong CSDL |
| Mật khẩu mạnh | Đạt | ≥ 8 ký tự, chữ hoa/thường và số (`NguoiDungBLL.KiemTraMatKhauManh`) |
| Giới hạn đăng nhập sai | Đạt (phía ứng dụng) | Khóa 30 giây sau 5 lần sai |
| Phân quyền | Đạt | Menu ẩn/khóa theo vai trò, BLL kiểm tra lại quyền (giảm giá, bảo trì, tài khoản) |
| Quản lý cuối cùng không bị khóa | Đạt | Transaction Serializable |
| Chuỗi kết nối | Cần lưu ý | `appsettings.json` chỉ chứa cấu hình máy phát triển; không commit mật khẩu thật, dùng tài khoản SQL quyền tối thiểu khi triển khai |
| Xuất CSV | Đạt | Ô bắt đầu bằng `= + - @` được chặn để tránh chèn công thức khi mở bằng Excel |
| Thông báo lỗi | Đạt | Hiển thị `Exception.Message` nghiệp vụ; không in chuỗi kết nối |
| Mã QR | Đạt | Chỉ chứa đường dẫn công khai (QT65); không chứa dữ liệu khách/hóa đơn; giới hạn 213 byte (QT67) |
| Cổng web: dữ liệu công khai | Đạt khi rà soát | Chỉ đọc; không dùng truy vấn sơ đồ phòng (có tên khách); tình trạng hiển thị rút gọn; chỉ phòng/loại đang hoạt động |
| Cổng web: chống chèn mã | Đạt | Mọi dữ liệu vào HTML dùng `HtmlEncode`; ngày phân tích theo `yyyy-MM-dd`; số phòng giới hạn độ dài; tham số SQL của DAL |
| Cổng web: tiêu đề bảo mật | Đạt | `Content-Security-Policy` không cho script, `X-Frame-Options: DENY`, `X-Content-Type-Options: nosniff`, `Referrer-Policy: no-referrer` |
| Cổng web: thông báo lỗi | Đạt | Lỗi CSDL chỉ ghi nhật ký máy chủ; khách thấy trang 503 chung |
| Cổng web: truyền tải và truy cập | Cần lưu ý | Mặc định HTTP, không xác thực, lắng nghe mọi địa chỉ (`0.0.0.0:5080`); chỉ nên chạy trong mạng nội bộ, dùng tài khoản SQL chỉ SELECT, cân nhắc HTTPS/giới hạn tần suất trước khi mở ra Internet |
