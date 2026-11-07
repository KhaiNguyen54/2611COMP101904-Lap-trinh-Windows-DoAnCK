# Kịch bản kiểm thử và nhật ký xác minh

> Các kết quả SQL/build bên dưới là ghi nhận đã có trong tài liệu trước lần hoàn thiện này; chúng chưa được chạy lại trong lần cập nhật tài liệu và không kèm log độc lập tại đây. Các kịch bản giao diện là bước kiểm thử thủ công, chưa phải test tự động.

## Kiểm thử SQL đã chạy trên SQL Server local

Các thao tác tạo dữ liệu kiểm thử được bao trong transaction và rollback sau khi assert, không giữ bản ghi thử.

| ID | Kịch bản | Kết quả |
|---|---|---|
| DB-01 | Chạy `Scripts/01_CreateDatabase.sql` rồi `02_InsertSampleData.sql` trên database thử nghiệm chưa tồn tại | Tạo database, 10 bảng, 6 stored procedure và dữ liệu mẫu; chạy lại lần hai không lỗi |
| DB-02 | Chạy lại script trên database thử nghiệm | Đạt (GitHub Actions): chạy lại lần hai không lỗi, không tạo trùng object/seed |
| DB-03 | Kiểm tra lịch của phiếu thuê đang ở | Đạt (DB-03a/b/c): `sp_KiemTraTrungLich` báo trùng khi giao nhau, không báo khi liền kề |
| DB-04 | Thêm dịch vụ và trả phòng trong transaction thử nghiệm | Đạt (DB-04a–f): hóa đơn tính đủ tiền phòng theo số đêm và dịch vụ, phòng sang Dọn dẹp |
| DB-05 | Đặt phòng có khoảng ngày giao với đặt phòng đang hiệu lực | Đạt (DB-05): đặt phòng giao nhau bị từ chối; đặt phòng đã hủy giải phóng lịch |
| DB-06 | Chạy lại script và kiểm tra index | Đạt: script chạy hai lần, đủ 6 index kiểm tra (gồm `UX_PhieuThue_DatPhong` lọc) |
| BUILD-01 | `dotnet build QuanLyKhachSan.sln --no-restore` (target `net10.0-windows`) | Đạt (GitHub Actions, Windows, .NET 10): build toàn bộ solution thành công, 208/208 test logic |

## Kiểm thử giao diện cần thực hiện trên máy có SQL Server cấu hình đúng

| ID | Kịch bản | Kết quả mong đợi |
|---|---|---|
| UI-01 | Đăng nhập sai / tài khoản bị khóa | Từ chối đăng nhập, không mở màn hình chính |
| UI-02 | Đăng nhập admin và lễ tân | Menu quản lý tài khoản, danh mục cấu hình và báo cáo chỉ hiện với Quản lý |
| UI-03 | Đổi mật khẩu sai mật khẩu hiện tại hoặc xác nhận không khớp | Không cập nhật mật khẩu |
| UI-04 | Tạo đặt phòng có ngày đến >= ngày đi hoặc trùng lịch | Báo lỗi, không lưu đặt phòng |
| UI-05 | Nhận phòng, ghi dịch vụ, trả phòng | Tạo phiếu, lưu snapshot đơn giá, tạo hóa đơn một lần và chuyển phòng sang Dọn dẹp |
| UI-06 | Trả phòng lần nữa cùng phiếu | Từ chối, không tạo hóa đơn thứ hai |
| UI-07 | Tìm và xem trước/in hóa đơn | Nội dung in khớp hóa đơn được chọn |
| UI-08 | Tạo khách với giấy tờ/điện thoại sai định dạng | Service báo lỗi trước khi gọi Repository |
| UI-09 | Trả phòng rồi xác nhận hoàn tất dọn phòng trên sơ đồ | Phòng ở trạng thái Dọn dẹp không được nhận khách; sau khi xác nhận chuyển sang Trống hoặc Đã đặt nếu có lịch sắp tới |
| UI-10 | Đặt phòng tương lai cho phòng đang có khách hoặc đang dọn | Chỉ cho chọn phòng nếu lịch không giao nhau; phòng đang dọn hoặc bảo trì không được chọn cho đến khi hoàn tất xử lý |

Các kịch bản UI trong bảng là các bước cần thực hiện thủ công. Cấu hình mặc định trong `appsettings.json` cần được đối chiếu với instance SQL Server thực tế. Chỉ chạy DB-01/02 trên database thử nghiệm hoặc sau khi sao lưu; không dùng database đang có dữ liệu thật. Không suy ra kết quả UI từ việc script SQL hoặc build thành công.

## Kiểm thử logic tự động (không cần CSDL)

Project `QuanLyKhachSan.Tests` gồm **208 ca** chia thành 14 nhóm (bảng dưới). Chạy: `dotnet run --project QuanLyKhachSan.Tests` (mã thoát 1 nếu có ca lỗi). Kết quả: 208/208 ca đạt. Các kiểm tra này không thay thế các kịch bản UI-01…UI-12.

| ID | Kịch bản bổ sung | Kết quả mong đợi |
|---|---|---|
| UI-11 | Đánh dấu "Khách không đến" / hủy đặt rồi đặt lại cùng phòng và ngày | Phòng được giải phóng, đặt được lại |
| UI-12 | Lễ tân mở Loại phòng/Phòng/Dịch vụ; Quản lý mở Báo cáo công suất | Lễ tân chỉ xem; Quản lý thấy biểu đồ, in và xuất CSV |
| UI-18 | Mở Đổi phòng (F8), chọn phòng đang ở, chọn phòng trống khác loại giá, nhập lý do, xác nhận | Có lời nhắc giá; phòng cũ → Dọn dẹp, phòng mới → Đang ở trên sơ đồ; lịch sử hiện đúng một dòng; dịch vụ đã ghi còn nguyên; trả phòng sau đó tính theo đơn giá cũ |
| UI-19 | Đổi phòng khi phòng mới vừa bị đặt ở máy khác; đổi khi không còn phòng trống | Báo lỗi rõ, dữ liệu không đổi; thông báo "không có phòng trống" khi danh sách rỗng |

### Chi tiết 208 ca theo nhóm

| Nhóm (hàm kiểm thử) | Số ca | Quy tắc | Nội dung kiểm tra |
|---|---|---|---|
| `SoDem` | 3 | QT40 | Số đêm tối thiểu 1; cách làm tròn theo ngày |
| `PhuThuTraMuon` | 5 | QT43 | Mốc phụ thu 1/3/6 giờ so với 12:00 (≤1 giờ miễn phí, 30%, 50%, 100%) |
| `GiamGia` | 3 | QT44 | Đúng 10% không cần duyệt; vượt 10% cần Quản lý duyệt; không giảm không cần duyệt |
| `TongThanhToan` | 4 | QT45 | Tổng = tiền phòng + dịch vụ + phụ thu − giảm giá − cọc; không âm |
| `Coc` | 1 | QT15 | Cọc tối đa bằng giá 1 đêm × số đêm |
| `GomDoanhThu` | 9 | – | Gom doanh thu theo ngày/tháng/loại phòng, kể cả ngày không phát sinh; tỷ lệ công suất trung bình |
| `Csv` | 4 | – | Thoát dấu phẩy và nháy kép; chặn chèn công thức |
| `Qr` | 14 | QT65–QT67 | Kích thước theo phiên bản, mẫu định vị/thời gian, ô tối cố định, thông tin định dạng BCH, mức sửa lỗi, tính xác định, chuỗi tiếng Việt, giới hạn 213 byte, đầu vào rỗng/quá dài |
| `DuongDanTraCuu` | 7 | QT66, QT68 | Địa chỉ gốc `Web:BaseUrl`, bỏ dấu `/` thừa, mã hóa URL số phòng, đường dẫn `/phong/{số}` và `/gia` |
| `KhoangNgayWeb` | 15 | QT69 | Mặc định hôm nay → ngày mai; ngày sai định dạng; ngày đến trước hôm nay; ngày đi không sau ngày đến; giới hạn 60 đêm |
| `DichVuTraCuu` | 12 | QT69 | Nhãn tình trạng phòng rút gọn, giới hạn số đêm, dữ liệu trả về không chứa tên khách |
| `TrangWeb` | 16 | QT69 | Sinh HTML: mã hóa chống chèn mã, định dạng giá, liên kết phòng, trang lỗi và trang bảng giá |
| `LocPhongWeb` | 26 | QT71 | Đọc bộ lọc (rỗng, hợp lệ, giá chữ/âm/0/quá lớn/thập phân, loại quá dài); lọc theo loại không phân biệt hoa thường, theo giá tối đa, kết hợp; form lọc chọn đúng loại, giữ giá đã nhập, gợi ý khi không có kết quả, mã hóa tên loại; hình minh họa (1/2/3 giường theo sức chứa, ổn định, mã hóa tên, chịu tên rỗng) |
| `DoiPhong` | 13 | QT47–QT49 | Khoảng phòng cần trống (đi dự kiến là hôm nay hoặc đã quá hạn vẫn tối thiểu 1 đêm); chuẩn hóa lý do (rỗng, cắt khoảng trắng, đúng 200 và 201 ký tự); lời nhắc khi giá phòng mới cao hơn/thấp hơn/bằng; từ chối thiếu mã phiếu, thiếu phòng mới, lý do quá dài |
| `PresenterTests` | 76 | Kiến trúc MVP | Presenter chạy với View và Service giả, không cần giao diện hay CSDL: đăng nhập (thiếu dữ liệu, khóa 30 giây sau 5 lần sai, hết khóa, lỗi CSDL); đặt phòng (thiếu khách, chuyển dữ liệu, hủy cần xác nhận, nhận phòng từ đặt, gợi ý cọc tối đa); nhận phòng; ghi/xóa dịch vụ; trả phòng (giảm >10% cần Quản lý duyệt, hủy duyệt thì dừng, Quản lý không bị hỏi lại, tổng thanh toán); đổi phòng; duyệt giảm giá; báo cáo; danh mục dùng chung (quyền chỉ xem, lưu mới/sửa cho 4 loại, ngừng sử dụng), tài khoản, thêm nhanh khách, đổi mật khẩu, tra cứu và in hóa đơn, lọc khách đang ở |

## Kiểm thử SQL và build tự động (GitHub Actions)

Workflow `.github/workflows/kiem-thu.yml` chạy mỗi lần push lên `main`, khi có pull request hoặc chạy tay:

| Job | Môi trường | Nội dung |
|---|---|---|
| Kiểm thử SQL Server | Ubuntu, SQL Server 2022 (container) | Chạy 01 và 02 hai lần (tạo mới, chạy lại), rồi `Scripts/KiemThu.sql` |
| Build và kiểm thử logic (.NET) | Windows, .NET 10 | `dotnet build` toàn bộ solution (WinForms, Web, Tests), chạy bộ test 208 ca |

`Scripts/KiemThu.sql` gồm **34 ca**, in ĐẠT/LỖI từng ca và thoát với mã lỗi nếu có ca lỗi; script tự dọn dữ liệu thử và trả các phòng thử về Trống.

| Nhóm | Ca | Nội dung |
|---|---|---|
| Môi trường | DB-00 | Phòng 101, 102, 103, 201, 202 có sẵn và đang Trống |
| Đối tượng (DB-01, DB-06) | 4 | Đủ 10 bảng, 6 stored procedure, dữ liệu mẫu, 6 index (gồm index lọc) |
| Ràng buộc (DB-07) | 2 | CHECK từ chối số giấy tờ sai định dạng và đặt phòng có ngày đến không trước ngày đi |
| Trùng lịch (DB-03, DB-05) | 4 | Khoảng giao nhau bị báo trùng, khoảng liền sau không trùng, ngày đảo bị từ chối (50001), đặt phòng đã hủy giải phóng lịch |
| Nhận và trả phòng (DB-04) | 6 | Phiếu thuê và trạng thái phòng, snapshot giá, không nhận phòng đang có khách, hóa đơn 2 đêm + dịch vụ, phòng sang Dọn dẹp, trả lần hai bị từ chối |
| Đổi phòng (DB-08) | 10 | Từ chối đổi sang chính phòng đó (50013), phòng bảo trì (50014), phòng có đặt chỗ giao nhau (50015), phiếu đã trả (50012); đổi thành công, trạng thái hai phòng, lịch sử, dịch vụ giữ nguyên, trả phòng theo đơn giá ban đầu (QT49) |
| Thống kê (DB-09) | 1 | `sp_ThongKeDoanhThu` chạy không lỗi |
| Giảm giá cần duyệt (DB-11) | 4 | Giảm 20% không người duyệt bị từ chối (50016) và phiếu vẫn Đang ở; người duyệt không hợp lệ bị từ chối; có Quản lý duyệt thì lập hóa đơn và ghi người duyệt; giảm đúng 10% không cần duyệt |
| Công suất khi đổi phòng (DB-10) | 2 | Phiếu đổi phòng được tách đoạn: các đêm trước ngày đổi tính cho phòng cũ (hai phòng cùng có khách), từ ngày đổi chỉ tính phòng mới, ngày trả phòng không tính |

Chạy thủ công trên database thử nghiệm: `sqlcmd -S . -E -C -b -f 65001 -i QuanLyKhachSan\Scripts\01_CreateDatabase.sql`, `-i QuanLyKhachSan\Scripts\02_InsertSampleData.sql`, rồi `-i QuanLyKhachSan\Scripts\KiemThu.sql`.

## Kiểm thử chức năng mở rộng (mã QR, cổng web, giao diện tối)

Gồm các kịch bản giao diện (UI-13…UI-19) và kịch bản cho cổng web (WEB-01…WEB-08); bộ mã hóa QR được đối chiếu bằng cách giải mã lại 23/23 mã mẫu (nhiều độ dài/phiên bản, có chuỗi tiếng Việt) với `QRCodeDetector` của OpenCV.

| ID | Kịch bản | Kết quả mong đợi |
|---|---|---|
| UI-13 | Mở Mã QR phòng và giá, chọn phòng, lưu PNG rồi quét bằng điện thoại | Mở đúng trang `/phong/{số}`; mã còn quét được khi in |
| UI-14 | Bật/tắt giao diện tối bằng Ctrl+D khi đang mở nhiều cửa sổ; đóng và mở lại ứng dụng | Mọi cửa sổ đổi màu ngay, chữ đọc được; lựa chọn được ghi nhớ; báo cáo in vẫn nền sáng |
| UI-15 | Mở trang web với số phòng không tồn tại, ngày sai định dạng, ngày đến trước hôm nay, khoảng quá 60 đêm | Thông báo lỗi thân thiện, không lộ thông tin kỹ thuật |
| UI-16 | Ngắt kết nối CSDL rồi tải trang web | Trang 503 chung; không lộ chuỗi kết nối; không thấy tên khách ở bất kỳ trang nào |
| WEB-01 | Tải `/`, `/gia`, `/phong/101`, `/api/gia`, `/api/phong/101` | 200; nội dung đúng dữ liệu; số tiền định dạng `450.000 đ` |
| WEB-02 | `/phong/999`, phòng ngừng hoạt động, `/phong/<script>` | 404, không phản hồi chuỗi người dùng nhập |
| WEB-03 | `?den=` sai định dạng, ngày quá khứ, `den ≥ di` | Thông báo lỗi tiếng Việt, mã 200 |
| WEB-04 | Giả lập lỗi mở kết nối | 503, chuỗi kết nối chỉ có trong nhật ký máy chủ |
| WEB-05 | Phòng đang có khách | Chỉ hiện "Hiện đang có khách", không có tên khách |
| WEB-06 | Tải `/logo.svg` và mở một trang bất kỳ | 200, `image/svg+xml`; logo và favicon hiển thị; CSP vẫn chỉ cho ảnh cùng nguồn |
| WEB-07 | `/?loai=Deluxe&gia=800000`, `/?gia=abc`, `/?gia=-5`, `/?loai=` (quá 50 ký tự) | Lọc đúng; giá/loại sai hiện thông báo lỗi, vẫn còn form; không lỗi 500 |
| WEB-08 | Mở `/gia` và `/phong/101` ở chế độ sáng, tối và màn hình hẹp | Hình minh họa hiển thị đủ, số giường theo sức chứa, màu theo loại phòng, chữ tên loại được mã hóa |
| UI-17 | Mở ứng dụng, xem biểu tượng cửa sổ, thanh tác vụ và màn hình chính | Logo hiển thị ở biểu tượng exe/cửa sổ và cạnh lời chào; vẫn chạy bình thường nếu thiếu `Resources/logo.*` |

## Bảng test case Excel (`TestCases/TestCase.xlsx`)
Bảng liệt kê từng ca một: sheet `KiemThuTuDong` (208 ca của `QuanLyKhachSan.Tests`, có mã, nhóm, quy tắc, mô tả, kỳ vọng, kết quả), `CSDL_Build` (các ca `KiemThu.sql` và build), `GiaoDien_Web` (UI-xx và WEB-xx làm thủ công, có cột kết quả thực tế, người thử, ngày thử), `TongHop` (số ca đạt/lỗi/chưa chạy) và `HuongDan`.

Quy ước: mỗi khi thêm hoặc sửa một ca kiểm thử (test tự động, `KiemThu.sql` hoặc kịch bản trong tài liệu này), phải lập lại bảng Excel trong cùng lần thay đổi:
1. `dotnet run --project QuanLyKhachSan.Tests -c Release -- --xuat TestCases/ketqua_tudong.tsv`
2. `python3 TestCases/tao_testcase.py TestCases/ketqua_tudong.tsv` (cần `openpyxl`).

Script giữ nguyên phần đã điền ở sheet `GiaoDien_Web`. Tệp `.tsv` chỉ là trung gian, không đưa vào Git.

## Tệp sao lưu `Database/QuanLyKhachSan.bak`
Job `sao-luu` của GitHub Actions tạo database từ `01_CreateDatabase.sql` và `02_InsertSampleData.sql`, sao lưu bằng `BACKUP DATABASE ... WITH CHECKSUM`, xóa database, khôi phục từ chính tệp `.bak` rồi chạy `KiemThu.sql` (34 ca). Tệp chỉ dùng được trên SQL Server 2022 trở lên; muốn tương thích rộng hơn dùng hai script. Khi đổi cấu trúc hoặc dữ liệu mẫu, chạy lại workflow với tùy chọn `commit_bak` để cập nhật tệp.
