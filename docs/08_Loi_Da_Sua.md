# Lỗi đã phát hiện và sửa trong đợt hoàn thiện

| # | Mức | Lỗi | Cách sửa |
|---|---|---|---|
| 1 | Cao | Phụ thu trả muộn bị tính hai lần khi khách ở quá ngày đi dự kiến | `QuyTacTinhTien.TinhPhuThuTraMuon` đo so với 12:00 của ngày trả thực tế |
| 2 | Cao | Cọc không bị giới hạn theo số đêm (QT15) | `DatPhongBLL.Them` kiểm tra cọc ≤ giá 1 đêm × số đêm (giá lấy qua `DatPhongDAL.LayGiaPhong`) |
| 3 | Cao | Đặt phòng cho phòng không tồn tại, ngừng hoạt động hoặc đang bảo trì | `DatPhongDAL.KiemTraPhong` trong transaction Serializable |
| 4 | Cao | Phòng kẹt ở "Dọn dẹp" | `PhongBLL.HoanTatDonDep` và thao tác trên sơ đồ phòng |
| 5 | Trung bình | DAL tham chiếu BLL (sai tầng) | Chuyển kiểm tra cọc sang BLL |
| 6 | Trung bình | Không đánh dấu được khách không đến, không xóa được dòng dịch vụ | Thêm `DanhDauKhongDen`, `XoaDichVu` (chỉ khi phiếu còn "Đang ở") |
| 7 | Trung bình | Khóa được Quản lý cuối cùng | Kiểm tra trong transaction Serializable ở `NguoiDungDAL` |
| 8 | Giao diện | Số liệu thẻ thống kê và chữ "Làm mới" bị cắt; hàng/cột kích thước cố định | Viết lại bố cục: `TableLayoutPanel`/`FlowLayoutPanel` tự co, `MinimumSize`, `GiaoDien.Nhom/ThanhNgang/GocTuCo`, cỡ thẻ phòng tính từ cỡ chữ |
| 9 | Giao diện | Tô màu nút theo nội dung chữ (dễ sai) | Nút hành động chính đánh dấu `Tag = "chinh"` |
| 10 | Giao diện | Biểu đồ/in báo cáo/xuất CSV chưa có | `ChartPanel`, `InAn`, `CsvExporter` |
| 11 | Trung bình | Dự án web: khai báo `None Update` cho `appsettings.json` không có tác dụng (Web SDK coi đây là `Content`), build báo trùng mục | Đổi thành `Content Update` có `CopyToOutputDirectory` |
| 12 | Giao diện | Khi thêm bộ màu sáng/tối, một phép thay màu hàng loạt làm hỏng khai báo bộ màu sáng gây lỗi khởi tạo `GiaoDien` | Khôi phục giá trị màu gốc; xác nhận lại bằng chụp giao diện cả hai chế độ |
| 13 | Giao diện | Ô chọn ngày hệ thống vẽ nền sáng, chữ sáng ở chế độ tối nên mờ | Giữ màu hệ thống cho `DateTimePicker`; nhãn tab được vẽ lại theo bộ màu |
