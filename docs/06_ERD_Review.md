# Giai đoạn 2 – ERD và biên bản review CSDL

```mermaid
erDiagram
    LoaiPhong ||--o{ Phong : phan_loai
    KhachHang ||--o{ DatPhong : dat
    Phong ||--o{ DatPhong : duoc_dat
    NguoiDung ||--o{ DatPhong : lap
    DatPhong o|--o| PhieuThue : chuyen_thanh
    KhachHang ||--o{ PhieuThue : luu_tru
    Phong ||--o{ PhieuThue : duoc_thue
    NguoiDung ||--o{ PhieuThue : tiep_nhan
    PhieuThue ||--o{ ChiTietDichVu : phat_sinh
    DichVu ||--o{ ChiTietDichVu : su_dung
    PhieuThue ||--o| HoaDon : thanh_toan
    NguoiDung ||--o{ HoaDon : lap
    LoaiPhong { int MaLoaiPhong PK; nvarchar TenLoaiPhong UK; nvarchar MoTa; decimal GiaMoiDem; int SucChua; bit HoatDong }
    Phong { int MaPhong PK; nvarchar SoPhong UK; int Tang; int MaLoaiPhong FK; nvarchar TrangThai; nvarchar GhiChu; bit HoatDong }
    KhachHang { int MaKhachHang PK; nvarchar HoTen; varchar SoGiayTo UK; varchar SoDienThoai UK; nvarchar Email; nvarchar DiaChi; datetime2 NgayTao }
    NguoiDung { int MaNguoiDung PK; nvarchar TenDangNhap UK; varchar MatKhauHash; varchar MatKhauSalt; int SoVongLap; nvarchar HoTen; varchar VaiTro; bit HoatDong; datetime2 NgayTao }
    DichVu { int MaDichVu PK; nvarchar TenDichVu UK; decimal DonGia; nvarchar DonViTinh; bit HoatDong }
    DatPhong { int MaDatPhong PK; int MaKhachHang FK; int MaPhong FK; int MaNguoiDung FK; date NgayDen; date NgayDi; decimal TienCoc; nvarchar TrangThai; datetime2 NgayTao }
    PhieuThue { int MaPhieuThue PK; int MaDatPhong FK_UK; int MaKhachHang FK; int MaPhong FK; int MaNguoiDung FK; datetime2 NgayNhan; date NgayDiDuKien; datetime2 NgayDiThucTe; decimal GiaPhongMoiDem; nvarchar TrangThai }
    ChiTietDichVu { int MaChiTietDichVu PK; int MaPhieuThue FK; int MaDichVu FK; decimal SoLuong; decimal DonGia; datetime2 ThoiDiemSuDung }
    HoaDon { int MaHoaDon PK; int MaPhieuThue FK_UK; int MaNguoiDung FK; datetime2 NgayLap; int SoDem; decimal TienPhong; decimal TienDichVu; decimal PhuThu; decimal GiamGia; decimal TienCoc; decimal TongThanhToan; nvarchar HinhThucThanhToan }
```

## Quyết định thiết kế

- Thêm `DatPhong` ngoài tám thực thể được nêu để tách đặt trước khỏi lưu trú; đặt phòng chuyển thành phiếu thuê khi nhận phòng, còn khách vãng lai tạo phiếu thuê trực tiếp.
- Đặt phòng dùng `date` và khoảng nửa mở `[NgàyDen, NgàyDi)`; phiếu thuê ghi thời điểm nhận/trả và giá phòng snapshot.
- Giá dịch vụ cũng được snapshot ở `ChiTietDichVu`; hóa đơn giữ snapshot các khoản tiền và có `UNIQUE(MaPhieuThue)` để chặn hóa đơn thứ hai.
- Loại phòng, phòng và dịch vụ được ngừng hoạt động bằng cờ `HoatDong`, không xóa lịch sử được tham chiếu bởi khóa ngoại.
- Mật khẩu mẫu dùng PBKDF2-SHA256, salt riêng, 100000 vòng; không lưu mật khẩu rõ. Tài khoản mẫu tạm dùng `Admin@123` và phải đổi trước khi dùng thật.
- Đã đặt `UNIQUE`, `CHECK`, FK cho mã/tên/số phòng, vai trò/trạng thái, giá, ngày, số lượng, số giấy tờ và điện thoại.
- Kiểm tra trùng lịch cần nằm cùng transaction ghi ở DAL (SERIALIZABLE); SP kiểm tra dùng khoảng `[đến, đi)` cho đặt chỗ và phiếu đang ở. `CHECK` không thể đảm bảo bất biến liên bảng trong SQL Server.

`Database.sql` có thể chạy nhiều lần mà không xóa dữ liệu; script tạo ba loại phòng, tám phòng trên ba tầng, hai khách, ba dịch vụ và hai tài khoản. DTO tương ứng nằm trong `QuanLyKhachSan/DTO`, bao gồm `DatPhong` là bảng nghiệp vụ cần có thêm.

## Ràng buộc cần lưu ý

- ERD phản ánh schema trong `QuanLyKhachSan/Scripts/Database.sql`; `SoDienThoai` cũng có unique index dù không khai báo inline trong `CREATE TABLE`.
- `PhieuThue.MaDatPhong` nullable và unique: khách vãng lai không cần đặt trước; mỗi đặt phòng chỉ chuyển thành tối đa một phiếu thuê.
- `HoaDon.MaPhieuThue` unique: mỗi phiếu thuê có tối đa một hóa đơn. Trạng thái đặt phòng/phiếu thuê và phương thức thanh toán có `CHECK` constraint.
- Khoảng thời gian giao nhau là ràng buộc liên bản ghi, không được bảo đảm bởi `CHECK` hoặc index unique thông thường. Stored procedure và transaction ở DAL thực hiện kiểm tra khi ghi.
- Bảng `Phong` lưu `TrangThai`, nhưng trạng thái hiển thị `Đang ở` và `Đã đặt` được suy ra từ phiếu thuê/đặt phòng khi tra cứu; không xem ERD như một bản sao toàn bộ logic hiển thị.
- Script dùng `CREATE OR ALTER` cho stored procedure và tạo database nếu chưa tồn tại. Chạy lần đầu cần quyền tạo database; chạy lại trên database có dữ liệu cần được kiểm soát như mọi thay đổi schema.
