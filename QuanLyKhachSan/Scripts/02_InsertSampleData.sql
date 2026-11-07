-- 02_InsertSampleData.sql - Dữ liệu mẫu để demo (SQL Server)
-- Chạy sau 01_CreateDatabase.sql. Chỉ chèn khi bảng còn trống nên chạy lại không tạo trùng.
-- Tài khoản mẫu: admin (Quản lý) và letan (Lễ tân), cùng mật khẩu ban đầu Admin@123.

SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
SET ANSI_PADDING ON;
SET ANSI_WARNINGS ON;
SET CONCAT_NULL_YIELDS_NULL ON;
SET ARITHABORT ON;
SET NUMERIC_ROUNDABORT OFF;

USE QuanLyKhachSan;
GO

-- Tài khoản mẫu dùng cùng mật khẩu tạm Admin@123; hash PBKDF2-SHA256, 100000 vòng.
IF NOT EXISTS (SELECT 1 FROM dbo.NguoiDung WHERE TenDangNhap=N'admin')
INSERT dbo.NguoiDung(TenDangNhap,MatKhauHash,MatKhauSalt,SoVongLap,HoTen,VaiTro)
VALUES (N'admin','jXI04eT1JoCJm3XNggxB83cbmILvimcXN/OhnXTpjMg=','ABEiM0RVZneImaq7zN3u/w==',100000,N'Quản lý mẫu','QuanLy');
IF NOT EXISTS (SELECT 1 FROM dbo.NguoiDung WHERE TenDangNhap=N'letan')
INSERT dbo.NguoiDung(TenDangNhap,MatKhauHash,MatKhauSalt,SoVongLap,HoTen,VaiTro)
VALUES (N'letan','UgSH7VM5emccCUEEWUdIsGgITpns5MFpeoWPtLKTs/Q=','//Ll2Mu+saSXin1wY1ZJPA==',100000,N'Lễ tân mẫu','LeTan');

IF NOT EXISTS (SELECT 1 FROM dbo.LoaiPhong)
INSERT dbo.LoaiPhong(TenLoaiPhong,MoTa,GiaMoiDem,SucChua) VALUES
(N'Tiêu chuẩn',N'Phòng ấm cúng, tiện nghi cơ bản',400000,2),
(N'Cao cấp',N'Phòng rộng, tiện nghi nâng cao',700000,2),
(N'Gia đình',N'Phòng dành cho gia đình',1000000,4);
IF NOT EXISTS (SELECT 1 FROM dbo.Phong)
INSERT dbo.Phong(SoPhong,Tang,MaLoaiPhong) SELECT v.SoPhong,v.Tang,l.MaLoaiPhong FROM (VALUES
(N'101',1,N'Tiêu chuẩn'),(N'102',1,N'Tiêu chuẩn'),(N'103',1,N'Cao cấp'),
(N'201',2,N'Tiêu chuẩn'),(N'202',2,N'Cao cấp'),(N'203',2,N'Gia đình'),
(N'301',3,N'Cao cấp'),(N'302',3,N'Gia đình')) v(SoPhong,Tang,TenLoai)
JOIN dbo.LoaiPhong l ON l.TenLoaiPhong=v.TenLoai;
IF NOT EXISTS (SELECT 1 FROM dbo.KhachHang)
INSERT dbo.KhachHang(HoTen,SoGiayTo,SoDienThoai,Email,DiaChi) VALUES
(N'Nguyễn Minh An','079123456789','0901234567','an@example.test',N'TP. Hồ Chí Minh'),
(N'Trần Thu Hà','079987654321','0912345678','ha@example.test',N'Hà Nội');
IF NOT EXISTS (SELECT 1 FROM dbo.DichVu)
INSERT dbo.DichVu(TenDichVu,DonGia,DonViTinh) VALUES
(N'Nước suối',15000,N'chai'),(N'Giặt ủi',50000,N'lần'),(N'Ăn sáng',80000,N'suất');
GO
