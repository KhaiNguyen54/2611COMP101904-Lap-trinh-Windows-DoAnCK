-- KiemThu.sql - Kiểm thử SQL tự động cho Database.sql (SQL Server).
-- Chạy sau Database.sql trên database THỬ NGHIỆM trống dữ liệu nghiệp vụ (chỉ có dữ liệu mẫu).
-- In kết quả ĐẠT/LỖI từng ca; có ca lỗi thì kết thúc với mã lỗi (dùng sqlcmd -b).
-- Script tự dọn dữ liệu đã tạo và trả các phòng thử về trạng thái Trống.

USE QuanLyKhachSan;
SET NOCOUNT ON;
SET XACT_ABORT OFF;
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
SET ANSI_PADDING ON;
SET ANSI_WARNINGS ON;
SET CONCAT_NULL_YIELDS_NULL ON;
SET ARITHABORT ON;
SET NUMERIC_ROUNDABORT OFF;

IF OBJECT_ID(N'tempdb..#KQ') IS NOT NULL DROP TABLE #KQ;
CREATE TABLE #KQ (Id int IDENTITY(1,1), Ma varchar(20), Ten nvarchar(200), Dat bit, ChiTiet nvarchar(300));
IF OBJECT_ID(N'tempdb..#R') IS NOT NULL DROP TABLE #R;
CREATE TABLE #R (MaHoaDon int, SoDem int, TienPhong decimal(18,2), TienDichVu decimal(18,2), PhuThu decimal(18,2),
                 GiamGia decimal(18,2), TienCoc decimal(18,2), TongThanhToan decimal(18,2));
IF OBJECT_ID(N'tempdb..#T') IS NOT NULL DROP TABLE #T;
CREATE TABLE #T (Bi bit);

DECLARE @h date = CONVERT(date, SYSDATETIME());
DECLARE @nd int = (SELECT MaNguoiDung FROM dbo.NguoiDung WHERE TenDangNhap = N'admin');
DECLARE @d2 date = DATEADD(day,2,@h), @d3 date = DATEADD(day,3,@h), @d6 date = DATEADD(day,6,@h), @d8 date = DATEADD(day,8,@h), @d9 date = DATEADD(day,9,@h), @d10 date = DATEADD(day,10,@h);
DECLARE @t2 datetime2(0) = DATEADD(day,2,SYSDATETIME()), @t3 datetime2(0) = DATEADD(day,3,SYSDATETIME());
DECLARE @err int, @n int, @bi bit;
DECLARE @kh1 int, @kh2 int, @p101 int, @p102 int, @p103 int, @p201 int, @p202 int;
DECLARE @dvNuoc int, @dvGiat int, @gNuoc decimal(18,2), @gGiat decimal(18,2);
DECLARE @pt1 int, @pt2 int, @dp1 int, @out int;

SELECT @p101 = MaPhong FROM dbo.Phong WHERE SoPhong = N'101';
SELECT @p102 = MaPhong FROM dbo.Phong WHERE SoPhong = N'102';
SELECT @p103 = MaPhong FROM dbo.Phong WHERE SoPhong = N'103';
SELECT @p201 = MaPhong FROM dbo.Phong WHERE SoPhong = N'201';
SELECT @p202 = MaPhong FROM dbo.Phong WHERE SoPhong = N'202';
SELECT @dvNuoc = MaDichVu, @gNuoc = DonGia FROM dbo.DichVu WHERE TenDichVu = N'Nước suối';
SELECT @dvGiat = MaDichVu, @gGiat = DonGia FROM dbo.DichVu WHERE TenDichVu = N'Giặt ủi';

-- ===== DB-00: điều kiện đầu vào =====
SELECT @n = COUNT(*) FROM dbo.Phong WHERE MaPhong IN (@p101,@p102,@p103,@p201,@p202) AND TrangThai = N'Trống' AND HoatDong = 1;
INSERT #KQ VALUES ('DB-00', N'Phòng 101,102,103,201,202 có sẵn và đang Trống', CASE WHEN @n = 5 AND @nd IS NOT NULL AND @dvNuoc IS NOT NULL AND @dvGiat IS NOT NULL THEN 1 ELSE 0 END, CONCAT(N'Số phòng sẵn sàng: ', @n));
IF @n <> 5 OR @nd IS NULL OR @dvNuoc IS NULL OR @dvGiat IS NULL GOTO Ket;

-- ===== DB-01: đối tượng được tạo =====
SELECT @n = COUNT(*) FROM sys.tables WHERE name IN (N'NguoiDung',N'LoaiPhong',N'Phong',N'KhachHang',N'DichVu',N'DatPhong',N'PhieuThue',N'ChiTietDichVu',N'HoaDon',N'LichSuDoiPhong');
INSERT #KQ VALUES ('DB-01a', N'Đủ 10 bảng', CASE WHEN @n = 10 THEN 1 ELSE 0 END, CONCAT(N'Số bảng: ', @n));
SELECT @n = COUNT(*) FROM sys.procedures WHERE name IN (N'sp_KiemTraTrungLich',N'sp_NhanPhong',N'sp_TraPhong',N'sp_DoiPhong',N'sp_ThongKeDoanhThu',N'sp_ThongKeCongSuat');
INSERT #KQ VALUES ('DB-01b', N'Đủ 6 stored procedure', CASE WHEN @n = 6 THEN 1 ELSE 0 END, CONCAT(N'Số thủ tục: ', @n));
SELECT @n = COUNT(*) FROM dbo.LoaiPhong;
INSERT #KQ VALUES ('DB-01c', N'Có dữ liệu mẫu loại phòng và tài khoản admin', CASE WHEN @n >= 3 AND @nd IS NOT NULL THEN 1 ELSE 0 END, CONCAT(N'Số loại phòng: ', @n));

-- ===== DB-06: index =====
SELECT @n = COUNT(*) FROM sys.indexes WHERE name IN (N'UX_KhachHang_SoDienThoai', N'IX_DatPhong_DatDangDat_Phong_NgayDen', N'IX_LichSuDoiPhong_PhieuThue', N'IX_HoaDon_NgayLap', N'IX_ChiTietDichVu_PhieuThue_ThoiDiem', N'UX_PhieuThue_DatPhong');
INSERT #KQ VALUES ('DB-06', N'Có các index lọc và tra cứu', CASE WHEN @n = 6 THEN 1 ELSE 0 END, CONCAT(N'Số index tìm thấy: ', @n));

-- ===== Dữ liệu thử =====
INSERT dbo.KhachHang(HoTen, SoGiayTo, SoDienThoai) VALUES (N'Khách thử một', '099999999991', '0999999991');
SET @kh1 = SCOPE_IDENTITY();
INSERT dbo.KhachHang(HoTen, SoGiayTo, SoDienThoai) VALUES (N'Khách thử hai', '099999999992', '0999999992');
SET @kh2 = SCOPE_IDENTITY();

-- ===== Ràng buộc dữ liệu =====
SET @err = NULL;
BEGIN TRY INSERT dbo.KhachHang(HoTen, SoGiayTo, SoDienThoai) VALUES (N'Sai giấy tờ', '12ab', '0999999993'); END TRY
BEGIN CATCH SET @err = ERROR_NUMBER(); END CATCH
INSERT #KQ VALUES ('DB-07a', N'CHECK từ chối số giấy tờ sai định dạng', CASE WHEN @err = 547 THEN 1 ELSE 0 END, CONCAT(N'Mã lỗi: ', ISNULL(CONVERT(varchar(10), @err), N'không lỗi')));
SET @err = NULL;
BEGIN TRY INSERT dbo.DatPhong(MaKhachHang, MaPhong, MaNguoiDung, NgayDen, NgayDi) VALUES (@kh1, @p201, @nd, DATEADD(day,2,@h), @h); END TRY
BEGIN CATCH SET @err = ERROR_NUMBER(); END CATCH
INSERT #KQ VALUES ('DB-07b', N'CHECK từ chối đặt phòng có ngày đến không trước ngày đi', CASE WHEN @err = 547 THEN 1 ELSE 0 END, CONCAT(N'Mã lỗi: ', ISNULL(CONVERT(varchar(10), @err), N'không lỗi')));

-- ===== DB-03 / DB-05: kiểm tra trùng lịch =====
INSERT dbo.DatPhong(MaKhachHang, MaPhong, MaNguoiDung, NgayDen, NgayDi, TienCoc) VALUES (@kh1, @p201, @nd, DATEADD(day,5,@h), DATEADD(day,8,@h), 0);
SET @dp1 = SCOPE_IDENTITY();
DELETE #T; INSERT #T EXEC dbo.sp_KiemTraTrungLich @MaPhong = @p201, @NgayDen = @d6, @NgayDi = @d9;
SELECT @bi = Bi FROM #T;
INSERT #KQ VALUES ('DB-03a', N'Khoảng ngày giao với đặt phòng đang hiệu lực bị báo trùng', CASE WHEN @bi = 1 THEN 1 ELSE 0 END, CONCAT(N'BiTrungLich = ', @bi));
DELETE #T; INSERT #T EXEC dbo.sp_KiemTraTrungLich @MaPhong = @p201, @NgayDen = @d8, @NgayDi = @d10;
SELECT @bi = Bi FROM #T;
INSERT #KQ VALUES ('DB-03b', N'Khoảng ngày liền sau (nửa mở) không bị báo trùng', CASE WHEN @bi = 0 THEN 1 ELSE 0 END, CONCAT(N'BiTrungLich = ', @bi));
SET @err = NULL;
BEGIN TRY EXEC dbo.sp_KiemTraTrungLich @MaPhong = @p201, @NgayDen = @d3, @NgayDi = @h; END TRY
BEGIN CATCH SET @err = ERROR_NUMBER(); END CATCH
INSERT #KQ VALUES ('DB-05', N'Ngày đến không trước ngày đi bị từ chối (50001)', CASE WHEN @err = 50001 THEN 1 ELSE 0 END, CONCAT(N'Mã lỗi: ', ISNULL(CONVERT(varchar(10), @err), N'không lỗi')));
UPDATE dbo.DatPhong SET TrangThai = N'Đã hủy' WHERE MaDatPhong = @dp1;
DELETE #T; INSERT #T EXEC dbo.sp_KiemTraTrungLich @MaPhong = @p201, @NgayDen = @d6, @NgayDi = @d9;
SELECT @bi = Bi FROM #T;
INSERT #KQ VALUES ('DB-03c', N'Đặt phòng đã hủy được giải phóng lịch', CASE WHEN @bi = 0 THEN 1 ELSE 0 END, CONCAT(N'BiTrungLich = ', @bi));

-- ===== DB-04: nhận phòng, ghi dịch vụ, trả phòng =====
SET @out = NULL;
EXEC dbo.sp_NhanPhong @MaPhong = @p101, @MaKhachHang = @kh1, @MaNguoiDung = @nd, @NgayDiDuKien = @d2, @MaDatPhong = NULL, @MaPhieuThue = @out OUTPUT;
SET @pt1 = @out;
SELECT @n = COUNT(*) FROM dbo.Phong WHERE MaPhong = @p101 AND TrangThai = N'Đang ở';
INSERT #KQ VALUES ('DB-04a', N'Nhận phòng tạo phiếu thuê và phòng chuyển Đang ở', CASE WHEN @pt1 IS NOT NULL AND @n = 1 THEN 1 ELSE 0 END, CONCAT(N'MaPhieuThue = ', ISNULL(CONVERT(varchar(10), @pt1), N'NULL')));
SELECT @n = COUNT(*) FROM dbo.PhieuThue WHERE MaPhieuThue = @pt1 AND GiaPhongMoiDem = (SELECT l.GiaMoiDem FROM dbo.Phong p JOIN dbo.LoaiPhong l ON p.MaLoaiPhong = l.MaLoaiPhong WHERE p.MaPhong = @p101);
INSERT #KQ VALUES ('DB-04b', N'Phiếu thuê lưu snapshot giá phòng', CASE WHEN @n = 1 THEN 1 ELSE 0 END, CONCAT(N'Số phiếu khớp giá: ', @n));
SET @err = NULL;
BEGIN TRY EXEC dbo.sp_NhanPhong @MaPhong = @p101, @MaKhachHang = @kh2, @MaNguoiDung = @nd, @NgayDiDuKien = @d2, @MaDatPhong = NULL, @MaPhieuThue = @out OUTPUT; END TRY
BEGIN CATCH SET @err = ERROR_NUMBER(); END CATCH
INSERT #KQ VALUES ('DB-04c', N'Không nhận phòng đang có khách', CASE WHEN @err IN (50003, 50004) THEN 1 ELSE 0 END, CONCAT(N'Mã lỗi: ', ISNULL(CONVERT(varchar(10), @err), N'không lỗi')));
INSERT dbo.ChiTietDichVu(MaPhieuThue, MaDichVu, SoLuong, DonGia) VALUES (@pt1, @dvNuoc, 2, @gNuoc);
DELETE #R;
INSERT #R EXEC dbo.sp_TraPhong @MaPhieuThue = @pt1, @MaNguoiDung = @nd, @NgayTra = @t2;
SELECT @n = COUNT(*) FROM #R r WHERE r.SoDem = 2
    AND r.TienPhong = 2 * (SELECT GiaPhongMoiDem FROM dbo.PhieuThue WHERE MaPhieuThue = @pt1)
    AND r.TienDichVu = 2 * @gNuoc
    AND r.TongThanhToan = r.TienPhong + r.TienDichVu;
INSERT #KQ VALUES ('DB-04d', N'Hóa đơn = tiền phòng 2 đêm + dịch vụ, không cọc', CASE WHEN @n = 1 THEN 1 ELSE 0 END, (SELECT CONCAT(N'Phòng ', TienPhong, N', dịch vụ ', TienDichVu, N', tổng ', TongThanhToan) FROM #R));
SELECT @n = COUNT(*) FROM dbo.Phong WHERE MaPhong = @p101 AND TrangThai = N'Dọn dẹp';
INSERT #KQ VALUES ('DB-04e', N'Trả phòng chuyển phòng sang Dọn dẹp', CASE WHEN @n = 1 THEN 1 ELSE 0 END, CONCAT(N'Số phòng Dọn dẹp: ', @n));
SET @err = NULL;
BEGIN TRY EXEC dbo.sp_TraPhong @MaPhieuThue = @pt1, @MaNguoiDung = @nd, @NgayTra = @t2; END TRY
BEGIN CATCH SET @err = ERROR_NUMBER(); END CATCH
SELECT @n = COUNT(*) FROM dbo.HoaDon WHERE MaPhieuThue = @pt1;
INSERT #KQ VALUES ('DB-04f', N'Trả phòng lần hai bị từ chối, chỉ có một hóa đơn', CASE WHEN @err = 50006 AND @n = 1 THEN 1 ELSE 0 END, CONCAT(N'Mã lỗi: ', ISNULL(CONVERT(varchar(10), @err), N'không lỗi'), N'; số hóa đơn: ', @n));

-- ===== NV08: đổi phòng =====
SET @out = NULL;
EXEC dbo.sp_NhanPhong @MaPhong = @p102, @MaKhachHang = @kh2, @MaNguoiDung = @nd, @NgayDiDuKien = @d3, @MaDatPhong = NULL, @MaPhieuThue = @out OUTPUT;
SET @pt2 = @out;
INSERT dbo.ChiTietDichVu(MaPhieuThue, MaDichVu, SoLuong, DonGia) VALUES (@pt2, @dvGiat, 1, @gGiat);
INSERT dbo.DatPhong(MaKhachHang, MaPhong, MaNguoiDung, NgayDen, NgayDi, TienCoc) VALUES (@kh1, @p201, @nd, DATEADD(day,1,@h), DATEADD(day,2,@h), 0);
UPDATE dbo.Phong SET TrangThai = N'Bảo trì' WHERE MaPhong = @p202;

SET @err = NULL;
BEGIN TRY EXEC dbo.sp_DoiPhong @MaPhieuThue = @pt2, @MaPhongMoi = @p102, @MaNguoiDung = @nd; END TRY
BEGIN CATCH SET @err = ERROR_NUMBER(); END CATCH
INSERT #KQ VALUES ('DB-08a', N'Đổi sang chính phòng hiện tại bị từ chối (50013)', CASE WHEN @err = 50013 THEN 1 ELSE 0 END, CONCAT(N'Mã lỗi: ', ISNULL(CONVERT(varchar(10), @err), N'không lỗi')));
SET @err = NULL;
BEGIN TRY EXEC dbo.sp_DoiPhong @MaPhieuThue = @pt2, @MaPhongMoi = @p202, @MaNguoiDung = @nd; END TRY
BEGIN CATCH SET @err = ERROR_NUMBER(); END CATCH
INSERT #KQ VALUES ('DB-08b', N'Đổi sang phòng đang bảo trì bị từ chối (50014)', CASE WHEN @err = 50014 THEN 1 ELSE 0 END, CONCAT(N'Mã lỗi: ', ISNULL(CONVERT(varchar(10), @err), N'không lỗi')));
SET @err = NULL;
BEGIN TRY EXEC dbo.sp_DoiPhong @MaPhieuThue = @pt2, @MaPhongMoi = @p201, @MaNguoiDung = @nd; END TRY
BEGIN CATCH SET @err = ERROR_NUMBER(); END CATCH
INSERT #KQ VALUES ('DB-08c', N'Đổi sang phòng có đặt chỗ giao nhau bị từ chối (50015)', CASE WHEN @err = 50015 THEN 1 ELSE 0 END, CONCAT(N'Mã lỗi: ', ISNULL(CONVERT(varchar(10), @err), N'không lỗi')));
SET @err = NULL;
BEGIN TRY EXEC dbo.sp_DoiPhong @MaPhieuThue = @pt1, @MaPhongMoi = @p103, @MaNguoiDung = @nd; END TRY
BEGIN CATCH SET @err = ERROR_NUMBER(); END CATCH
INSERT #KQ VALUES ('DB-08d', N'Đổi phòng cho phiếu thuê đã trả bị từ chối (50012)', CASE WHEN @err = 50012 THEN 1 ELSE 0 END, CONCAT(N'Mã lỗi: ', ISNULL(CONVERT(varchar(10), @err), N'không lỗi')));
SELECT @n = COUNT(*) FROM dbo.PhieuThue WHERE MaPhieuThue = @pt2 AND MaPhong = @p102;
INSERT #KQ VALUES ('DB-08e', N'Các lần đổi bị từ chối không làm thay đổi phiếu thuê', CASE WHEN @n = 1 THEN 1 ELSE 0 END, CONCAT(N'Phiếu vẫn ở phòng cũ: ', @n));

EXEC dbo.sp_DoiPhong @MaPhieuThue = @pt2, @MaPhongMoi = @p103, @MaNguoiDung = @nd, @LyDo = N'  Máy lạnh hỏng  ';
SELECT @n = COUNT(*) FROM dbo.PhieuThue WHERE MaPhieuThue = @pt2 AND MaPhong = @p103;
INSERT #KQ VALUES ('DB-08f', N'Đổi phòng thành công: phiếu thuê gắn phòng mới', CASE WHEN @n = 1 THEN 1 ELSE 0 END, CONCAT(N'Số phiếu ở phòng mới: ', @n));
SELECT @n = COUNT(*) FROM dbo.Phong WHERE (MaPhong = @p102 AND TrangThai = N'Dọn dẹp') OR (MaPhong = @p103 AND TrangThai = N'Đang ở');
INSERT #KQ VALUES ('DB-08g', N'Phòng cũ sang Dọn dẹp, phòng mới sang Đang ở', CASE WHEN @n = 2 THEN 1 ELSE 0 END, CONCAT(N'Số phòng đúng trạng thái: ', @n));
SELECT @n = COUNT(*) FROM dbo.LichSuDoiPhong WHERE MaPhieuThue = @pt2 AND MaPhongCu = @p102 AND MaPhongMoi = @p103
    AND GiaCu < GiaMoi AND LyDo = N'Máy lạnh hỏng' AND MaNguoiDung = @nd;
INSERT #KQ VALUES ('DB-08h', N'Ghi lịch sử đổi phòng (giá cũ/mới, lý do đã cắt khoảng trắng)', CASE WHEN @n = 1 THEN 1 ELSE 0 END, CONCAT(N'Số dòng lịch sử khớp: ', @n));
SELECT @n = COUNT(*) FROM dbo.ChiTietDichVu WHERE MaPhieuThue = @pt2;
INSERT #KQ VALUES ('DB-08i', N'Dịch vụ đã ghi vẫn thuộc phiếu thuê sau khi đổi phòng', CASE WHEN @n = 1 THEN 1 ELSE 0 END, CONCAT(N'Số dòng dịch vụ: ', @n));

DELETE #R;
INSERT #R EXEC dbo.sp_TraPhong @MaPhieuThue = @pt2, @MaNguoiDung = @nd, @NgayTra = @t3;
SELECT @n = COUNT(*) FROM #R r WHERE r.SoDem = 3
    AND r.TienPhong = 3 * (SELECT GiaCu FROM dbo.LichSuDoiPhong WHERE MaPhieuThue = @pt2)
    AND r.TienDichVu = @gGiat;
INSERT #KQ VALUES ('DB-08j', N'Trả phòng sau khi đổi tính theo đơn giá phòng ban đầu (QT49)', CASE WHEN @n = 1 THEN 1 ELSE 0 END, (SELECT CONCAT(N'Phòng ', TienPhong, N', dịch vụ ', TienDichVu) FROM #R));

-- ===== DB-09: thống kê chạy được =====
SET @err = NULL;
BEGIN TRY
    EXEC dbo.sp_ThongKeDoanhThu @TuNgay = @h, @DenNgay = @h;
END TRY BEGIN CATCH SET @err = ERROR_NUMBER(); END CATCH
INSERT #KQ VALUES ('DB-09a', N'sp_ThongKeDoanhThu chạy không lỗi', CASE WHEN @err IS NULL THEN 1 ELSE 0 END, CONCAT(N'Mã lỗi: ', ISNULL(CONVERT(varchar(10), @err), N'không lỗi')));

-- ===== DB-10: công suất theo phòng khi có đổi phòng =====
IF OBJECT_ID(N'tempdb..#CS0') IS NOT NULL DROP TABLE #CS0;
IF OBJECT_ID(N'tempdb..#CS1') IS NOT NULL DROP TABLE #CS1;
CREATE TABLE #CS0 (Ngay date, SoPhongCoKhach int, TongPhong int, TyLe decimal(5,2));
CREATE TABLE #CS1 (Ngay date, SoPhongCoKhach int, TongPhong int, TyLe decimal(5,2));
DECLARE @m6 date = DATEADD(day,-6,@h), @m5 date = DATEADD(day,-5,@h), @m3 date = DATEADD(day,-3,@h), @m1 date = DATEADD(day,-1,@h);
INSERT #CS0 EXEC dbo.sp_ThongKeCongSuat @TuNgay = @m6, @DenNgay = @m1;
-- A: phòng 202 từ h-5 đến h-3. B: phòng 201 từ h-5, đổi sang 202 vào h-3, trả h-1.
INSERT dbo.PhieuThue(MaKhachHang, MaPhong, MaNguoiDung, NgayNhan, NgayDiDuKien, NgayDiThucTe, GiaPhongMoiDem, TrangThai)
VALUES (@kh1, @p202, @nd, @m5, @m3, @m3, 400000, N'Đã trả');
INSERT dbo.PhieuThue(MaKhachHang, MaPhong, MaNguoiDung, NgayNhan, NgayDiDuKien, NgayDiThucTe, GiaPhongMoiDem, TrangThai)
VALUES (@kh1, @p202, @nd, @m5, @m1, @m1, 400000, N'Đã trả');
SET @pt2 = SCOPE_IDENTITY();
INSERT dbo.LichSuDoiPhong(MaPhieuThue, MaPhongCu, MaPhongMoi, GiaCu, GiaMoi, ThoiDiemDoi, MaNguoiDung)
VALUES (@pt2, @p201, @p202, 400000, 400000, @m3, @nd);
INSERT #CS1 EXEC dbo.sp_ThongKeCongSuat @TuNgay = @m6, @DenNgay = @m1;
SELECT @n = COUNT(*) FROM #CS1 b JOIN #CS0 a ON a.Ngay = b.Ngay
WHERE b.SoPhongCoKhach - a.SoPhongCoKhach = CASE WHEN b.Ngay IN (@m5, DATEADD(day,1,@m5)) THEN 2 ELSE 0 END
  AND b.Ngay IN (@m6, @m5, DATEADD(day,1,@m5));
INSERT #KQ VALUES ('DB-10a', N'Các đêm trước ngày đổi tính cho phòng cũ (hai phòng cùng có khách)', CASE WHEN @n = 3 THEN 1 ELSE 0 END, CONCAT(N'Số ngày khớp: ', @n, N'/3'));
SELECT @n = COUNT(*) FROM #CS1 b JOIN #CS0 a ON a.Ngay = b.Ngay
WHERE b.SoPhongCoKhach - a.SoPhongCoKhach = CASE WHEN b.Ngay IN (@m3, DATEADD(day,1,@m3)) THEN 1 ELSE 0 END
  AND b.Ngay IN (@m3, DATEADD(day,1,@m3), @m1);
INSERT #KQ VALUES ('DB-10b', N'Từ ngày đổi chỉ tính phòng mới; ngày trả phòng không tính', CASE WHEN @n = 3 THEN 1 ELSE 0 END, CONCAT(N'Số ngày khớp: ', @n, N'/3'));

-- ===== Dọn dẹp =====
Ket:
DELETE h FROM dbo.HoaDon h JOIN dbo.PhieuThue p ON h.MaPhieuThue = p.MaPhieuThue WHERE p.MaKhachHang IN (SELECT MaKhachHang FROM dbo.KhachHang WHERE SoGiayTo IN ('099999999991','099999999992'));
DELETE c FROM dbo.ChiTietDichVu c JOIN dbo.PhieuThue p ON c.MaPhieuThue = p.MaPhieuThue WHERE p.MaKhachHang IN (SELECT MaKhachHang FROM dbo.KhachHang WHERE SoGiayTo IN ('099999999991','099999999992'));
DELETE l FROM dbo.LichSuDoiPhong l JOIN dbo.PhieuThue p ON l.MaPhieuThue = p.MaPhieuThue WHERE p.MaKhachHang IN (SELECT MaKhachHang FROM dbo.KhachHang WHERE SoGiayTo IN ('099999999991','099999999992'));
DELETE FROM dbo.PhieuThue WHERE MaKhachHang IN (SELECT MaKhachHang FROM dbo.KhachHang WHERE SoGiayTo IN ('099999999991','099999999992'));
DELETE FROM dbo.DatPhong WHERE MaKhachHang IN (SELECT MaKhachHang FROM dbo.KhachHang WHERE SoGiayTo IN ('099999999991','099999999992'));
DELETE FROM dbo.KhachHang WHERE SoGiayTo IN ('099999999991','099999999992');
UPDATE dbo.Phong SET TrangThai = N'Trống' WHERE SoPhong IN (N'101', N'102', N'103', N'201', N'202') AND TrangThai <> N'Trống';

-- ===== Kết quả =====
SELECT Ma, Ten, CASE Dat WHEN 1 THEN N'ĐẠT' ELSE N'LỖI' END AS KetQua, ChiTiet FROM #KQ ORDER BY Id;
SELECT @n = COUNT(*) FROM #KQ WHERE Dat = 0;
DECLARE @tong int = (SELECT COUNT(*) FROM #KQ);
PRINT CONCAT(N'Tổng ', @tong, N' ca, đạt ', @tong - @n, N', lỗi ', @n);
IF @n > 0 RAISERROR(N'Có ca kiểm thử SQL bị lỗi.', 16, 1);
