# 1.1 – Phạm vi và danh sách chức năng

Đề tài 12: Quản lý Khách sạn (Windows Forms, C#, SQL Server, mô hình 3 lớp).

## 1. Phạm vi

**Trong phạm vi**
- Ứng dụng Desktop Windows Forms, dữ liệu lưu SQL Server.
- 2 vai trò: **Lễ tân**, **Quản lý**.
- Quản lý phòng, khách, đặt phòng, nhận/trả phòng, dịch vụ phát sinh, hóa đơn, báo cáo.
- Mở rộng (ngoài đặc tả bắt buộc): giao diện sáng/tối, mã QR tra cứu phòng và giá, cổng web **chỉ đọc** cho khách tra cứu (xem nhóm E).

**Ngoài phạm vi** (theo đề bài)
- Website hoặc Web API cho khách **tự đặt phòng**, ứng dụng di động, hệ thống phân tán. (Cổng web tra cứu chỉ đọc ở nhóm E không cho đặt phòng, không đăng nhập và không hiển thị thông tin khách nên không thuộc mục này.)
- Thanh toán online, tích hợp cổng thanh toán, kênh đặt phòng OTA.
- Quản lý nhân sự, lương, kho, kế toán.

## 2. Danh sách chức năng

Cột "Loại": **BB** = bắt buộc, **NC** = nâng cao (làm nếu còn thời gian).
Cột "Quyền": **LT** = Lễ tân, **QL** = Quản lý.

### Nhóm A – Hệ thống

| Mã | Chức năng | Loại | Quyền |
|---|---|---|---|
| HT01 | Đăng nhập | BB | LT, QL |
| HT02 | Đổi mật khẩu | BB | LT, QL |
| HT03 | Phân quyền hiển thị menu và chức năng | BB | LT, QL |
| HT04 | Quản lý tài khoản (thêm, sửa, khóa, đặt lại mật khẩu) | BB | QL |

### Nhóm B – Danh mục

| Mã | Chức năng | Loại | Quyền |
|---|---|---|---|
| DM01 | Loại phòng: thêm, sửa, xóa, tìm kiếm, kiểm tra trùng | BB | QL |
| DM02 | Phòng: thêm, sửa, xóa, tìm kiếm, lọc theo tầng/loại/trạng thái | BB | QL (xem: LT) |
| DM03 | Cấu hình giá phòng theo loại phòng | BB | QL |
| DM04 | Khách hàng: thêm, sửa, tìm kiếm, kiểm tra trùng CMND/CCCD | BB | LT, QL |
| DM05 | Dịch vụ: thêm, sửa, xóa, tìm kiếm, cấu hình đơn giá | BB | QL (xem: LT) |

### Nhóm C – Nghiệp vụ

| Mã | Chức năng | Loại | Quyền |
|---|---|---|---|
| NV01 | Đặt phòng theo khoảng ngày, kiểm tra giao nhau | BB | LT, QL |
| NV02 | Hủy hoặc sửa đặt phòng | BB | LT, QL |
| NV03 | Hiển thị trạng thái phòng bằng màu | BB | LT, QL |
| NV04 | Nhận phòng (từ đặt phòng hoặc khách vãng lai), lập phiếu thuê | BB | LT, QL |
| NV05 | Ghi nhận dịch vụ phát sinh vào phiếu thuê | BB | LT, QL |
| NV06 | Trả phòng bằng Transaction, tính tiền, lập hóa đơn | BB | LT, QL |
| NV07 | In hóa đơn lưu trú (xem trước và in) | BB | LT, QL |
| NV08 | Đổi phòng trong thời gian lưu trú | NC | LT, QL |

### Nhóm D – Tra cứu và báo cáo

| Mã | Chức năng | Loại | Quyền |
|---|---|---|---|
| TC01 | Tra cứu phòng, đặt phòng, phiếu thuê, hóa đơn theo từ khóa, khoảng thời gian, trạng thái | BB | LT, QL |
| BC01 | Báo cáo doanh thu theo ngày/tháng và theo loại phòng | BB | QL |
| BC02 | Biểu đồ GDI+ (doanh thu, công suất phòng) | BB | QL |
| BC03 | Xem trước và in báo cáo | BB | QL |
| BC04 | Thống kê công suất phòng | NC | QL |
| BC05 | Sơ đồ phòng theo tầng | NC | LT, QL |
| BC06 | Xuất danh sách khách đang lưu trú (CSV/Excel) | NC | LT, QL |

### Nhóm E – Mở rộng (ngoài đặc tả bắt buộc)

Cột "Loại": **MR** = mở rộng, bổ sung sau khi hoàn thành đặc tả ban đầu.

| Mã | Chức năng | Loại | Quyền |
|---|---|---|---|
| MR01 | Giao diện sáng/tối: chuyển bằng menu hoặc Ctrl+D, ghi nhớ theo máy | MR | LT, QL |
| MR02 | Tạo mã QR tra cứu phòng và bảng giá: xem, lưu PNG, in, xuất hàng loạt, sao chép liên kết | MR | LT, QL |
| MR03 | Cổng web tra cứu cho khách: bảng giá, tình trạng phòng, tìm phòng trống theo ngày (chỉ đọc) | MR | Khách (công khai) |

## 3. Đối chiếu với đề bài

Mọi mục trong "Chức năng chính" và "Chức năng Hệ thống" của đề tài đều có ít nhất một mã chức năng ở trên:

- Quản lý phòng, cấu hình giá → DM01–DM03
- Đặt phòng, kiểm tra giao nhau → NV01
- Nhận/trả phòng, màu trạng thái, Transaction → NV03, NV04, NV06
- Dịch vụ và hóa đơn → NV05, NV07
- Mở rộng (sơ đồ tầng, công suất, xuất danh sách) → BC04–BC06
- Tài khoản, danh mục, tra cứu, báo cáo → HT01–HT04, DM01–DM05, TC01, BC01–BC03

## 4. Thống kê

- Bắt buộc: 20 chức năng
- Nâng cao: 4 chức năng (NV08, BC04, BC05, BC06)
- Mở rộng: 3 chức năng (MR01–MR03), không tính vào 24 chức năng ở trên

## 5. Đối chiếu source hiện tại

Bảng chức năng phía trên mô tả phạm vi yêu cầu, không phải xác nhận mọi chức năng đã hoàn thành. Tình trạng dưới đây được đối chiếu với source trong `QuanLyKhachSan/`:

| Nhóm | Đã có trong source | Chưa có hoặc chưa hoàn tất |
|---|---|---|
| Hệ thống | Đăng nhập (khóa 30 giây sau 5 lần sai), đăng xuất, đổi mật khẩu; Quản lý tạo, khóa, đặt lại mật khẩu; luôn còn ít nhất một Quản lý hoạt động | Chưa có sửa thông tin tài khoản, xóa tài khoản |
| Danh mục | Khách hàng, loại phòng, phòng, dịch vụ: thêm, sửa, tìm kiếm, ngừng sử dụng; Lễ tân chỉ xem ba danh mục sau | Chưa có lọc theo tầng/loại trong màn hình Phòng (có ở sơ đồ phòng) |
| Nghiệp vụ | Tạo/hủy đặt phòng, đánh dấu khách không đến, cọc tối đa 1 đêm × số đêm; nhận phòng từ đặt trước hoặc vãng lai; ghi và xóa dịch vụ; đổi phòng khi đang ở (F8, có lịch sử); trả phòng có transaction và hóa đơn; sơ đồ phòng theo tầng; bảo trì/dọn dẹp | Chưa có sửa đặt phòng; đổi phòng giữ nguyên đơn giá phiếu thuê, chưa tách tiền phòng theo từng chặng giá |
| Tra cứu/báo cáo | Tra cứu hóa đơn, khách đang ở; doanh thu theo ngày/tháng/loại phòng; công suất phòng; biểu đồ; xem trước/in; xuất CSV | Chưa có xuất Excel (.xlsx) |
| Mở rộng | Giao diện sáng/tối (Ctrl+D, ghi nhớ); mã QR phòng/giá (lưu PNG, in, xuất tất cả); cổng web `QuanLyKhachSan.Web` (tìm phòng trống có lọc theo loại và giá tối đa, bảng giá, chi tiết phòng có hình minh họa, API JSON) | Web chưa cho đặt phòng, chưa có xác thực/HTTPS |

Phòng ở trạng thái Dọn dẹp có thể được xác nhận hoàn tất trên sơ đồ; đây là thao tác cập nhật trạng thái, khác với chức năng Đổi phòng (NV08). Ma trận này cần được cập nhật cùng kịch bản kiểm thử khi source thay đổi.
