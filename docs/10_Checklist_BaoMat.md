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
