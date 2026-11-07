# Tài liệu dự án Quản lý khách sạn

Thư mục này lưu yêu cầu, mô hình nghiệp vụ, quy tắc tính toán, thiết kế dữ liệu và kịch bản kiểm thử của đồ án. README ở thư mục gốc hướng dẫn cài đặt và chạy ứng dụng; trang này là mục lục và hướng dẫn đọc bộ tài liệu.

## Bắt đầu nhanh

1. Đọc [phạm vi và danh sách chức năng](01_PhamVi_ChucNang.md) để biết yêu cầu và vai trò.
2. Đọc [quy trình nghiệp vụ](02_QuyTrinh_NghiepVu.md), sau đó đối chiếu [quy tắc nghiệp vụ](03_QuyTac_NghiepVu.md) trước khi thay đổi xử lý.
3. Dùng [Use Case](04_UseCase.md) để tra luồng chính, ngoại lệ và quyền truy cập.
4. Đối chiếu [ERD và review database](06_ERD_Review.md) với `QuanLyKhachSan/Scripts/Database.sql`.
5. Chạy theo hướng dẫn trong [README dự án](../README.md); dùng [kịch bản kiểm thử](07_KichBanKiemThu.md) để xác nhận thay đổi.

## Danh mục tài liệu

| Tài liệu | Nội dung | Khi cần tra cứu |
|---|---|---|
| [01 — Phạm vi và chức năng](01_PhamVi_ChucNang.md) | Phạm vi đề tài, vai trò, mã chức năng bắt buộc/nâng cao/mở rộng (MR01–MR03) | Chốt tính năng và giới hạn đồ án |
| [02 — Quy trình nghiệp vụ](02_QuyTrinh_NghiepVu.md) | Đặt phòng → nhận phòng → dịch vụ → trả phòng → hóa đơn; vòng đời trạng thái; quy trình khách tra cứu bằng mã QR | Thay đổi luồng thao tác hoặc trạng thái |
| [03 — Quy tắc nghiệp vụ](03_QuyTac_NghiepVu.md) | Quy tắc ngày, trùng lịch, dữ liệu khách, giá, phụ thu, giảm giá, phân quyền; tra cứu công khai và mã QR (QT65–QT70) | Viết/đánh giá validation và tính tiền |
| [04 — Use Case](04_UseCase.md) | Tác nhân (gồm Khách), 22 use case, luồng chính/ngoại lệ và ma trận quyền | Thiết kế giao diện hoặc kiểm tra quyền |
| [05 — Git và quy ước](05_Git_QuyUoc.md) | Nhánh, commit, đặt tên và tích hợp nhóm | Làm việc nhóm hoặc chuẩn bị pull request |
| [06 — ERD và review CSDL](06_ERD_Review.md) | ERD, quyết định thiết kế, khóa và ràng buộc; ghi chú cổng web không đổi schema | Sửa schema, Model hoặc Repository |
| [07 — Kịch bản kiểm thử](07_KichBanKiemThu.md) | Kết quả xác minh SQL/build, kiểm thử giao diện và chức năng mở rộng | Kiểm tra trước khi tích hợp/nộp bài |
| [08 — Lỗi đã sửa](08_Loi_Da_Sua.md) | Danh sách lỗi và cách sửa | Review, báo cáo |
| [09 — Hướng dẫn và demo](09_HuongDan_Demo.md) | Cài đặt, phím tắt (gồm Ctrl+D), kịch bản demo kèm mã QR | Chuẩn bị demo |
| [10 — Checklist bảo mật](10_Checklist_BaoMat.md) | Kiểm tra bảo mật cơ bản, gồm cổng web và mã QR | Trước khi nộp |

## Liên kết với kế hoạch

Kế hoạch và timeline gốc nằm tại [`../KeHoach_QuanLyKhachSan.xlsx`](../KeHoach_QuanLyKhachSan.xlsx). Các tài liệu trong thư mục này mô tả yêu cầu và thiết kế; trạng thái triển khai thực tế cần được xác nhận bằng source code và kết quả kiểm thử, không suy ra chỉ từ việc có một chức năng trong bảng yêu cầu.

## Phạm vi triển khai hiện tại

Đối chiếu source WinForms hiện có: ứng dụng dùng SQL Server, các lớp View → Service → Repository và Model. Source có đăng nhập/đổi mật khẩu/quản lý tài khoản, danh mục, tạo và hủy đặt phòng, nhận phòng, ghi dịch vụ, trả phòng/lập hóa đơn trong transaction, tra cứu/xem trước/in hóa đơn, sơ đồ trạng thái phòng và báo cáo doanh thu dạng bảng/biểu đồ. Ngoài ra có ba chức năng mở rộng: giao diện sáng/tối, tạo mã QR phòng/giá và cổng web tra cứu chỉ đọc (`QuanLyKhachSan.Web`).

| Tình trạng | Hạng mục |
|---|---|
| Có trong source | Đăng nhập/đăng xuất, đổi mật khẩu, quản lý tài khoản; danh mục; đặt, hủy, không đến; nhận/trả phòng; dịch vụ; hóa đơn; sơ đồ phòng theo tầng; bảo trì và dọn phòng; báo cáo doanh thu và công suất; biểu đồ; in; xuất CSV; khách đang ở |
| Mở rộng | Giao diện sáng/tối (Ctrl+D); mã QR phòng/giá; cổng web tra cứu chỉ đọc (xem tình trạng xác minh ở tài liệu 07) |
| Chưa có hoặc chưa hoàn tất trong source hiện được rà soát | Sửa đặt phòng; tách tiền phòng theo từng chặng giá khi đổi phòng; xuất .xlsx; hóa đơn điều chỉnh; khách tự đặt phòng qua web; xác thực/HTTPS cho cổng web |

Đây là đối chiếu mã nguồn, không thay thế kiểm thử giao diện. Trạng thái xác minh theo lần chạy được ghi riêng trong [kịch bản kiểm thử](07_KichBanKiemThu.md); không đánh dấu chức năng đạt chỉ vì có yêu cầu hoặc mục menu.

## Quy ước cập nhật tài liệu

- Giữ các mã chức năng, mã quy tắc và ID use case ổn định để tiện đối chiếu với timeline, source và kiểm thử.
- Khi thay đổi quy tắc nghiệp vụ, cập nhật quy tắc trước, sau đó cập nhật quy trình/use case và kịch bản liên quan.
- Khi thay đổi schema, cập nhật ERD/review và script SQL cùng một lần.
- Ghi rõ kết quả đã chạy, môi trường và giới hạn xác minh; không ghi kết quả mong đợi như kết quả đã đạt.
- Ảnh minh họa phải là ảnh chụp ứng dụng thật, lưu trong `docs/assets/` và có alt text mô tả đúng nội dung. Chưa có ảnh thì để ghi chú placeholder, không trỏ thẻ ảnh tới file chưa tồn tại.

## Cấu trúc thư mục

```text
docs/
├── README.md
├── 01_PhamVi_ChucNang.md
├── 02_QuyTrinh_NghiepVu.md
├── 03_QuyTac_NghiepVu.md
├── 04_UseCase.md
├── 05_Git_QuyUoc.md
├── 06_ERD_Review.md
├── 07_KichBanKiemThu.md
├── 08_Loi_Da_Sua.md
├── 09_HuongDan_Demo.md
└── 10_Checklist_BaoMat.md
```

Sơ đồ dùng Mermaid. GitHub/GitLab và VS Code Markdown Preview có thể hiển thị các sơ đồ này.
