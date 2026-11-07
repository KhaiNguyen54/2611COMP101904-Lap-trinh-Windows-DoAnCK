# 1.5 – Mô tả Git repo, quy ước đặt tên, nhánh, commit

> Tài liệu này đề xuất quy trình làm việc nhóm cho đồ án; tên nhánh, bảo vệ nhánh và quy tắc Pull Request chỉ có hiệu lực khi được cấu hình trên máy chủ Git của nhóm.

## 1. Repo

- **Tên repo:** `QuanLyKhachSan` (không dấu, không khoảng trắng).
- **Nơi lưu:** GitHub hoặc GitLab, chế độ **Private**; mời cả nhóm với quyền Write.
- **Nhánh mặc định:** `main`, bật bảo vệ (không push trực tiếp, chỉ merge qua Pull Request, cần ít nhất 1 người duyệt).
- **Commit đầu tiên:** thêm `README.md`, `.gitignore` và thư mục `docs/` (có thể thêm cả khung solution từ bước 1.6).

### Cấu trúc 

```text
2611COMP101904-Lap-trinh-Windows-DoAnCK/
├── README.md
├── QuanLyKhachSan.sln
├── docs/                    # tài liệu yêu cầu, thiết kế và kiểm thử
├── Latex/                   # báo cáo LaTeX (bao_cao_full.tex), ảnh minh họa
├── QuanLyKhachSan/          # project WinForms
│   ├── Models/  Repositories/  Services/  Presenters/  Views/  Resources/  Scripts/
│   ├── Program.cs
│   └── QuanLyKhachSan.csproj
├── QuanLyKhachSan.Web/      # cổng web tra cứu (ASP.NET Core), dùng chung Models/Repositories/Services
└── QuanLyKhachSan.Tests/    # kiểm thử logic tự động
```

Tên thư mục workspace hiện tại có hậu tố đồ án; `QuanLyKhachSan` là tên solution/project. Có thể dùng tên repo không dấu `QuanLyKhachSan` nếu nhóm thống nhất.

### `.gitignore` tối thiểu cho Visual Studio và LaTeX

```gitignore
.vs/
bin/
obj/
*.user
*.suo
*.log
packages/
# Tệp phụ trợ khi biên dịch LaTeX
*.aux
*.toc
*.out
# Không đưa chuỗi kết nối thật hoặc dữ liệu cá nhân vào repo
appsettings.local.json
```

Nếu dùng .NET, có thể tạo bằng `dotnet new gitignore` rồi bổ sung dòng `appsettings.local.json`.

## 2. Quy ước nhánh

| Nhánh | Mục đích | Ai tạo/merge |
|---|---|---|
| `main` | Bản ổn định, luôn build được | Chỉ merge qua Pull Request |
| `develop` | Nhánh tích hợp hằng ngày | Merge từ nhánh tính năng |
| `feature/<mã-việc>-<mô-tả-ngắn>` | Làm từng công việc | Mỗi người tự tạo từ `develop` |
| `fix/<mã-việc>-<mô-tả-ngắn>` | Sửa lỗi | Tạo từ `develop` |

- **Mã việc** lấy theo bảng kế hoạch (ID cột A), ví dụ `feature/3.3-phong-dal`, `feature/6.5-tra-phong`, `fix/8.4-loi-trung-lich`.
- Nhánh sống ngắn: xong việc thì merge và xóa.
- Gộp `develop` → `main` vào các mốc M1–M5 (xem sheet "Mốc kiểm tra").
- Trước khi tạo Pull Request: `git pull origin develop` (hoặc rebase) để lấy bản mới nhất, build thành công, chạy thử chức năng mình làm.

## 3. Quy ước commit

Dạng: `<loại>(<phạm vi>): <mô tả ngắn, thể mệnh lệnh>`

| Loại | Dùng khi |
|---|---|
| `feat` | Thêm chức năng |
| `fix` | Sửa lỗi |
| `docs` | Tài liệu |
| `refactor` | Sửa cấu trúc, không đổi hành vi |
| `style` | Định dạng, đổi tên, không đổi logic |
| `test` | Thêm/sửa test |
| `chore` | Cấu hình, script, công việc vặt |

Phạm vi: `dto`, `dal`, `bll`, `gui`, `sql`, `docs`, `web` (cổng web tra cứu). Commit cấu hình/công việc chung không gắn phạm vi (ví dụ `chore: cập nhật .gitignore`).

Ví dụ:
```text
feat(dal): thêm PhongRepository với các hàm CRUD
feat(bll): kiểm tra giao nhau khoảng ngày khi đặt phòng
fix(gui): sửa lỗi không tải lại lưới sau khi xóa khách
docs: bổ sung quy tắc phụ thu trả muộn
chore(sql): thêm dữ liệu mẫu 20 phòng
feat(web): thêm trang tra cứu phòng trống cho khách
```

Quy tắc:
- Mỗi commit làm **một việc**, build được.
- Tiêu đề không quá khoảng 72 ký tự, không có dấu chấm cuối.
- Có thể thêm mã việc ở thân commit: `Refs: 3.3`.
- Thông điệp commit chỉ gồm một dòng tiêu đề theo dạng trên; không chèn đường dẫn phiên làm việc hay thông tin nội bộ vào commit.
- Không commit file `bin/`, `obj/`, `.vs/`, mật khẩu, chuỗi kết nối thật.

## 4. Quy ước đặt tên trong code

| Đối tượng | Quy ước | Ví dụ |
|---|---|---|
| Namespace | `QuanLyKhachSan.<Tầng>` | `QuanLyKhachSan.Repository` |
| Lớp Model | PascalCase, tên thực thể | `Phong`, `KhachHang`, `PhieuThue` |
| Lớp Repository | `<Thực thể>Repository` | `PhongRepository` |
| Lớp Service | `<Thực thể>Service` | `PhongService` |
| Form | `Frm<ChứcNăng>` | `FrmDatPhong`, `FrmTraPhong` |
| UserControl | `Uc<Tên>` | `UcSoDoPhong` |
| Phương thức | PascalCase, động từ | `LayTatCa()`, `Them()`, `KiemTraTrungLich()` |
| Trường private | `_camelCase` | `_phongDal` |
| Biến cục bộ, tham số | camelCase | `ngayDen` |
| Control trên form | tiền tố kiểu + PascalCase | `txtHoTen`, `btnLuu`, `dgvPhong`, `cboLoaiPhong`, `dtpNgayDen` |
| Bảng SQL | PascalCase, số ít | `Phong`, `DatPhong`, `ChiTietDichVu` |
| Cột khóa chính | `Ma<Thực thể>` | `MaPhong` |
| Stored Procedure | `sp_<ĐộngTừ><ĐốiTượng>` | `sp_TraPhong`, `sp_KiemTraTrungLich` |

- Tên **tiếng Việt không dấu** nhất quán cho thực thể nghiệp vụ; từ kỹ thuật dùng tiếng Anh (`Database`, `Connection`).
- View chỉ gọi **Presenter**; Presenter gọi **Service**; Service gọi **Repository**; Repository chỉ làm việc với CSDL. View không chứa nghiệp vụ và không gọi thẳng Service/Repository; ngoại lệ là khung chính, sơ đồ trạng thái phòng, hộp thoại mã QR và giới thiệu (chỉ điều hướng hoặc vẽ giao diện).
- Câu lệnh SQL luôn dùng **tham số**, không nối chuỗi.

## 5. Quy trình làm việc hằng ngày

1. `git checkout develop && git pull`
2. `git checkout -b feature/<mã-việc>-<mô-tả>`
3. Làm việc, commit nhỏ và thường xuyên.
4. `git push -u origin feature/...` và mở Pull Request vào `develop`.
5. Một thành viên khác review, build, thử nhanh rồi merge.
6. Cập nhật cột Trạng thái trong file kế hoạch.

## 6. Xử lý xung đột

- File dễ xung đột: `*.csproj`, `*.Designer.cs`, `Database.sql`. Hạn chế nhiều người cùng sửa một form; tách việc theo module (B: phòng, C: khách và dịch vụ, D: hệ thống).
- `Database.sql`: chỉ trưởng nhóm (A) sửa; người khác cần đổi CSDL thì báo A hoặc gửi đoạn script để A gộp.
- Khi xung đột: người mở Pull Request tự giải quyết, build lại rồi mới push.
