# 1.3 – Quy tắc nghiệp vụ

## 1. Phòng và giá

| Mã | Quy tắc |
|---|---|
| QT01 | Mỗi phòng thuộc đúng một loại phòng và nằm ở một tầng. **Số phòng là duy nhất** |
| QT02 | Giá phòng cấu hình **theo loại phòng**, đơn vị VNĐ/đêm, giá > 0 |
| QT03 | Hóa đơn lưu **đơn giá tại thời điểm lập phiếu thuê** (đổi giá sau này không ảnh hưởng hóa đơn cũ) |
| QT04 | Không xóa phòng hoặc loại phòng đã có đặt phòng/phiếu thuê. Chỉ cho đổi trạng thái sang **Ngừng sử dụng** |
| QT05 | Phòng ở trạng thái Bảo trì không được đặt hoặc nhận |

## 2. Đặt phòng

| Mã | Quy tắc |
|---|---|
| QT10 | Ngày đến **<** ngày đi; ngày đến không được trước ngày hiện tại (trừ khi sửa dữ liệu cũ bởi Quản lý) |
| QT11 | Khoảng ngày đặt là **[ngày đến, ngày đi)**: đêm của ngày đi không tính. Khách trả phòng ngày 5 và khách khác nhận ngày 5 **không** xung đột |
| QT12 | **Hai khoảng [a1, a2) và [b1, b2) giao nhau khi `a1 < b2 AND b1 < a2`.** Một phòng không được có hai khoảng giao nhau trong các đặt phòng (Đã đặt) và phiếu thuê (Đang ở) |
| QT13 | Khi sửa một đặt phòng, kiểm tra giao nhau và **loại trừ chính nó** |
| QT14 | Đặt phòng đã hủy hoặc không đến thì không tính vào kiểm tra giao nhau |
| QT15 | Tiền cọc là tùy chọn, tối đa bằng tiền phòng 1 đêm × số đêm; trừ vào hóa đơn khi trả phòng. Source đã áp mức tối đa này ở Service. |
| QT16 | Một đặt phòng gắn một khách đại diện (bắt buộc có thông tin khách) |

## 3. Nhận phòng

| Mã | Quy tắc |
|---|---|
| QT20 | Giờ nhận phòng chuẩn **14:00**. Cho nhận sớm nếu phòng đang Trống và không vướng đặt phòng khác. |
| QT21 | Chỉ nhận phòng ở trạng thái Trống hoặc Đã đặt (đúng đặt phòng của khách đó) |
| QT22 | Khách phải có họ tên, số giấy tờ (CMND/CCCD/hộ chiếu), số điện thoại |
| QT23 | Số giấy tờ khách là duy nhất; khách cũ được tái sử dụng, không tạo mới |
| QT24 | Nhận phòng tạo **PhieuThue** (Đang ở) và chuyển phòng sang Đang ở trong **cùng một thao tác** |

## 4. Dịch vụ

| Mã | Quy tắc |
|---|---|
| QT30 | Chỉ ghi dịch vụ vào phiếu thuê **Đang ở** |
| QT31 | Số lượng > 0. Lưu **đơn giá tại thời điểm ghi** vào dòng chi tiết |
| QT32 | Thành tiền dịch vụ = số lượng × đơn giá. Sửa hoặc xóa dòng dịch vụ chỉ khi phiếu thuê chưa trả |
| QT33 | Dịch vụ ngừng cung cấp thì ẩn khỏi danh sách chọn, không xóa dữ liệu cũ |

## 5. Tính tiền

| Mã | Quy tắc |
|---|---|
| QT40 | **Số đêm = số ngày giữa ngày đến và ngày đi** (chỉ tính phần ngày), **tối thiểu 1 đêm** |
| QT41 | **Tiền phòng = số đêm × đơn giá phòng** |
| QT42 | **Tiền dịch vụ = tổng thành tiền các dòng dịch vụ** |
| QT43 | Giờ trả phòng chuẩn **12:00**. Phụ thu tính theo bảng bên dưới. |
| QT44 | Giảm giá nhập tay bằng số tiền, không vượt tiền phòng + dịch vụ + phụ thu. Giảm tối đa 10% tiền hàng thì Lễ tân tự quyết; vượt 10% cần một Quản lý duyệt bằng tài khoản của mình ngay khi trả phòng (Quản lý đang đăng nhập tự là người duyệt). Hóa đơn ghi người duyệt (`MaNguoiDuyetGiam`) và in trên hóa đơn; stored procedure từ chối bằng lỗi 50016 nếu thiếu Quản lý hợp lệ. |
| QT45 | **Tổng thanh toán = tiền phòng + tiền dịch vụ + phụ thu − giảm giá − tiền cọc** |
| QT46 | Trả phòng sớm: tính theo số đêm thực tế (ngày đi thực tế), tối thiểu 1 đêm |
| QT47 | **Đổi phòng** chỉ áp dụng cho phiếu thuê Đang ở. Phòng mới phải khác phòng hiện tại, đang hoạt động, ở trạng thái Trống hoặc Đã đặt, không có khách và không vướng đặt chỗ nào từ hôm nay đến ngày đi dự kiến (tối thiểu 1 đêm) |
| QT48 | Đổi phòng chạy trong **một transaction**: phiếu thuê chuyển sang phòng mới, phòng cũ → Dọn dẹp, phòng mới → Đang ở, ghi một dòng lịch sử (phòng cũ/mới, giá cũ/mới, lý do tùy chọn tối đa 200 ký tự, người thực hiện). Dịch vụ đã ghi vẫn gắn với phiếu thuê |
| QT49 | Đơn giá của phiếu thuê được **giữ nguyên** sau khi đổi phòng (theo QT03); nếu giá phòng mới khác, màn hình nhắc lễ tân và lịch sử lưu cả hai mức giá để xử lý chênh lệch khi trả phòng (ví dụ qua giảm giá) |

**Phụ thu trả muộn**

| Trả muộn so với 12:00 | Phụ thu |
|---|---|
| ≤ 1 giờ | Miễn phí |
| Trên 1 đến 3 giờ | 30% giá phòng 1 đêm |
| Trên 3 đến 6 giờ | 50% giá phòng 1 đêm |
| Trên 6 giờ | 100% giá phòng 1 đêm |

**Ví dụ tính tiền**

Phòng đơn 400.000 đ/đêm; nhận 10/11, trả 13/11 lúc 15:00 (muộn 3 giờ); dịch vụ 2 chai nước (15.000 đ) và 1 giặt ủi (50.000 đ); cọc 200.000 đ; không giảm giá.

- Số đêm = 3 → tiền phòng = 1.200.000 đ
- Dịch vụ = 2 × 15.000 + 50.000 = 80.000 đ
- Phụ thu muộn 3 giờ = 30% × 400.000 = 120.000 đ
- Tổng thanh toán = 1.200.000 + 80.000 + 120.000 − 200.000 = **1.200.000 đ**

## 6. Trả phòng và hóa đơn

| Mã | Quy tắc |
|---|---|
| QT50 | **Trả phòng chạy trong một Transaction**: (1) tính tiền, (2) tạo HoaDon, (3) đóng PhieuThue (Đã trả, ghi ngày đi thực tế), (4) chuyển phòng sang Dọn dẹp, (5) cập nhật đặt phòng liên quan. Một bước lỗi thì **rollback toàn bộ** |
| QT51 | Mỗi phiếu thuê chỉ có **một** hóa đơn; không trả phòng hai lần |
| QT52 | Hóa đơn đã lập không sửa, không xóa. Sai sót xử lý bằng hóa đơn điều chỉnh (ngoài phạm vi, ghi chú cho báo cáo) |
| QT53 | Hóa đơn ghi hình thức thanh toán (Tiền mặt / Chuyển khoản / Thẻ) |
| QT54 | Có thể in lại hóa đơn nhiều lần |

## 7. Khách hàng và tài khoản

| Mã | Quy tắc |
|---|---|
| QT60 | CMND/CCCD: 9 hoặc 12 chữ số; số điện thoại: 10 chữ số bắt đầu bằng 0; email (nếu có) đúng định dạng |
| QT61 | Không xóa khách đã có đặt phòng/phiếu thuê |
| QT62 | Mật khẩu **băm** (không lưu thô), tối thiểu 8 ký tự, có chữ và số |
| QT63 | Tài khoản bị khóa không đăng nhập được. Quản lý không tự khóa hoặc tự xóa chính mình |
| QT64 | Phải có ít nhất một tài khoản Quản lý đang hoạt động. Source đã cưỡng chế điều kiện này khi khóa tài khoản. |

## 8. Phân quyền

| Chức năng | Lễ tân | Quản lý |
|---|---|---|
| Đặt phòng, nhận phòng, ghi dịch vụ, trả phòng, in hóa đơn | ✔ | ✔ |
| Quản lý khách hàng | ✔ | ✔ |
| Xem danh mục phòng, dịch vụ | ✔ (chỉ xem) | ✔ |
| Thêm, sửa, xóa loại phòng, phòng, dịch vụ, cấu hình giá | ✘ | ✔ |
| Quản lý tài khoản | ✘ | ✔ |
| Báo cáo, biểu đồ, thống kê | ✘ | ✔ |
| Giảm giá vượt 10% | Cần Quản lý duyệt | ✔ |
| Đổi mật khẩu của chính mình | ✔ | ✔ |

## 9. Mẫu SQL kiểm tra giao nhau (để dùng ở bước 2.4)

```sql
-- Có trùng lịch không? (@DenMoi, @DiMoi là khoảng muốn đặt)
SELECT COUNT(*)
FROM DatPhong
WHERE MaPhong = @MaPhong
  AND TrangThai = N'Đã đặt'
  AND MaDatPhong <> @MaDatPhongBoQua   -- NULL hoặc 0 khi thêm mới
  AND NgayDen < @DiMoi
  AND @DenMoi < NgayDi;
-- Làm tương tự với PhieuThue (Đang ở), so khoảng [NgayDen, NgayDiDuKien).
```

## 10. Đối chiếu với source hiện tại

| Quy tắc | Hành vi trong source | Còn thiếu / lưu ý |
|---|---|---|
| QT05 | Phòng Bảo trì không đặt/nhận được; chỉ Quản lý đưa phòng Trống vào/ra bảo trì (sơ đồ phòng) | – |
| QT10–QT14 | Service chặn ngày đến quá khứ và ngày đến không trước ngày đi; Repository kiểm tra giao nhau trong transaction Serializable, từ chối phòng không tồn tại/ngừng/bảo trì | Không có giao diện sửa đặt phòng để xác minh QT13 |
| QT15 | Cọc ≤ giá 1 đêm × số đêm (`QuyTacTinhTien.TienCocToiDa`), cọc được trừ khi trả phòng; tổng phải trả không âm | – |
| QT20–QT24 | Nhận phòng kiểm tra ngày đi dự kiến, phòng hoạt động/sẵn sàng, đặt phòng tương ứng, xung đột; lưu snapshot giá phòng | – |
| QT30–QT33 | Số lượng dịch vụ dương; đơn giá lưu theo chi tiết; xóa dòng dịch vụ chỉ khi phiếu còn "Đang ở" | Chưa có sửa số lượng (xóa rồi ghi lại) |
| QT40–QT46 | Số đêm tối thiểu 1; phụ thu muộn đo so với 12:00 của ngày trả thực tế (≤1 giờ miễn phí, 1–3 giờ 30%, 3–6 giờ 50%, trên 6 giờ 100%); giảm trên 10% cần Quản lý duyệt, ghi người duyệt vào hóa đơn | – |
| QT47–QT49 | `sp_DoiPhong` (transaction, khóa dòng) kiểm tra phòng mới và đặt chỗ; `QuyTacDoiPhong` tính khoảng cần trống, chuẩn hóa lý do, lời nhắc giá; bảng `LichSuDoiPhong` | Chưa tách tiền phòng theo từng chặng giá. Báo cáo công suất tính theo từng chặng phòng: các đêm trước ngày đổi tính cho phòng cũ, từ ngày đổi tính cho phòng mới |
| QT50–QT54 | Stored procedure trả phòng dùng transaction, một hóa đơn cho mỗi phiếu thuê, đóng phiếu, phòng chuyển Dọn dẹp | Chưa có hóa đơn điều chỉnh |
| QT65–QT70 | Mã QR chỉ chứa đường dẫn; `Web:BaseUrl` có mặc định; giới hạn 213 byte; QR chỉ cho phòng hoạt động; trang web chỉ đọc, ẩn tên khách, kiểm tra ngày; giao diện sáng/tối lưu theo máy (`TraCuuUrl`, `QrCode`, `TraCuuService`, `GiaoDien`) | Web chưa có xác thực/HTTPS; chưa có giới hạn tần suất truy cập |
| QT60–QT64 | Kiểm tra giấy tờ, điện thoại, email, mật khẩu mạnh, băm PBKDF2; tài khoản tự khóa bị chặn; luôn còn một Quản lý hoạt động (khóa trong transaction Serializable) | Chưa xác minh điều kiện email ở CSDL |

Quy tắc SQL kiểm tra giao nhau ở mục 9 chỉ là truy vấn minh họa. Source hiện dùng `sp_KiemTraTrungLich` và transaction ở Repository để kiểm tra trước khi tạo đặt phòng.

## 11. Tra cứu công khai, mã QR và giao diện

| Mã | Quy tắc |
|---|---|
| QT65 | Mã QR chỉ chứa đường dẫn tra cứu công khai (`/phong/{số phòng}` hoặc `/gia`); không chứa thông tin khách, hóa đơn hay tài khoản |
| QT66 | Địa chỉ gốc lấy từ khóa cấu hình `Web:BaseUrl`; thiếu thì dùng mặc định `http://localhost:5080`; dấu `/` thừa ở cuối được loại bỏ |
| QT67 | Nội dung mã QR không rỗng và tối đa 213 byte (UTF-8); vượt giới hạn thì từ chối thay vì tạo mã sai |
| QT68 | Chỉ tạo mã QR cho phòng đang hoạt động; số phòng được mã hóa URL khi đưa vào đường dẫn |
| QT69 | Trang web công khai chỉ đọc: chỉ trả về loại phòng và phòng đang hoạt động; tình trạng hiển thị rút gọn (không nêu tên khách); ngày đến không trước hôm nay, ngày đến trước ngày đi, tối đa 60 đêm mỗi lần tra cứu |
| QT70 | Lựa chọn giao diện sáng/tối được lưu theo từng máy và áp dụng ngay cho mọi cửa sổ đang mở; báo cáo in luôn dùng nền sáng |
| QT71 | Bộ lọc tìm phòng trên web: loại phòng tối đa 50 ký tự (so khớp không phân biệt hoa thường); giá tối đa là số nguyên dương không quá 100.000.000; giá sai định dạng bị từ chối bằng thông báo, không ảnh hưởng truy vấn CSDL (lọc thực hiện sau khi lấy danh sách phòng trống) |
