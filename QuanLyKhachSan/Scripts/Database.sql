-- Database.sql - Quản lý khách sạn (SQL Server)
-- Giai đoạn 2: lược đồ, dữ liệu minh họa và stored procedure.
-- Có thể chạy lại; không xóa dữ liệu hiện có.

SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
SET ANSI_PADDING ON;
SET ANSI_WARNINGS ON;
SET CONCAT_NULL_YIELDS_NULL ON;
SET ARITHABORT ON;
SET NUMERIC_ROUNDABORT OFF;

IF DB_ID(N'QuanLyKhachSan') IS NULL
BEGIN
    CREATE DATABASE QuanLyKhachSan;
END
GO

USE QuanLyKhachSan;
GO

IF OBJECT_ID(N'dbo.NguoiDung', N'U') IS NULL
CREATE TABLE dbo.NguoiDung (
    MaNguoiDung int IDENTITY(1,1) CONSTRAINT PK_NguoiDung PRIMARY KEY,
    TenDangNhap nvarchar(50) NOT NULL CONSTRAINT UQ_NguoiDung_TenDangNhap UNIQUE,
    MatKhauHash varchar(128) NOT NULL,
    MatKhauSalt varchar(64) NOT NULL,
    SoVongLap int NOT NULL CONSTRAINT DF_NguoiDung_SoVongLap DEFAULT 100000,
    HoTen nvarchar(100) NOT NULL,
    VaiTro varchar(20) NOT NULL CONSTRAINT CK_NguoiDung_VaiTro CHECK (VaiTro IN ('LeTan','QuanLy')),
    HoatDong bit NOT NULL CONSTRAINT DF_NguoiDung_HoatDong DEFAULT 1,
    NgayTao datetime2(0) NOT NULL CONSTRAINT DF_NguoiDung_NgayTao DEFAULT SYSDATETIME(),
    CONSTRAINT CK_NguoiDung_SoVongLap CHECK (SoVongLap >= 10000)
);
GO

IF OBJECT_ID(N'dbo.LoaiPhong', N'U') IS NULL
CREATE TABLE dbo.LoaiPhong (
    MaLoaiPhong int IDENTITY(1,1) CONSTRAINT PK_LoaiPhong PRIMARY KEY,
    TenLoaiPhong nvarchar(80) NOT NULL CONSTRAINT UQ_LoaiPhong_Ten UNIQUE,
    MoTa nvarchar(300) NULL,
    GiaMoiDem decimal(18,2) NOT NULL CONSTRAINT CK_LoaiPhong_Gia CHECK (GiaMoiDem > 0),
    SucChua int NOT NULL CONSTRAINT CK_LoaiPhong_SucChua CHECK (SucChua > 0),
    HoatDong bit NOT NULL CONSTRAINT DF_LoaiPhong_HoatDong DEFAULT 1
);
GO

IF OBJECT_ID(N'dbo.Phong', N'U') IS NULL
CREATE TABLE dbo.Phong (
    MaPhong int IDENTITY(1,1) CONSTRAINT PK_Phong PRIMARY KEY,
    SoPhong nvarchar(10) NOT NULL CONSTRAINT UQ_Phong_SoPhong UNIQUE,
    Tang int NOT NULL CONSTRAINT CK_Phong_Tang CHECK (Tang > 0),
    MaLoaiPhong int NOT NULL,
    TrangThai nvarchar(20) NOT NULL CONSTRAINT DF_Phong_TrangThai DEFAULT N'Trống',
    GhiChu nvarchar(300) NULL,
    HoatDong bit NOT NULL CONSTRAINT DF_Phong_HoatDong DEFAULT 1,
    CONSTRAINT FK_Phong_LoaiPhong FOREIGN KEY (MaLoaiPhong) REFERENCES dbo.LoaiPhong(MaLoaiPhong),
    CONSTRAINT CK_Phong_TrangThai CHECK (TrangThai IN (N'Trống',N'Đã đặt',N'Đang ở',N'Dọn dẹp',N'Bảo trì'))
);
GO

IF OBJECT_ID(N'dbo.KhachHang', N'U') IS NULL
CREATE TABLE dbo.KhachHang (
    MaKhachHang int IDENTITY(1,1) CONSTRAINT PK_KhachHang PRIMARY KEY,
    HoTen nvarchar(120) NOT NULL,
    SoGiayTo varchar(12) NOT NULL CONSTRAINT UQ_KhachHang_SoGiayTo UNIQUE,
    SoDienThoai varchar(10) NOT NULL,
    Email nvarchar(254) NULL,
    DiaChi nvarchar(300) NULL,
    NgayTao datetime2(0) NOT NULL CONSTRAINT DF_KhachHang_NgayTao DEFAULT SYSDATETIME(),
    CONSTRAINT CK_KhachHang_SoGiayTo CHECK (SoGiayTo NOT LIKE '%[^0-9]%' AND LEN(SoGiayTo) IN (9,12)),
    CONSTRAINT CK_KhachHang_SoDienThoai CHECK (SoDienThoai NOT LIKE '%[^0-9]%' AND LEN(SoDienThoai)=10 AND LEFT(SoDienThoai,1)='0')
);
GO

IF NOT EXISTS(SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID(N'dbo.KhachHang') AND name=N'UX_KhachHang_SoDienThoai')
    CREATE UNIQUE INDEX UX_KhachHang_SoDienThoai ON dbo.KhachHang(SoDienThoai);
GO

IF OBJECT_ID(N'dbo.DichVu', N'U') IS NULL
CREATE TABLE dbo.DichVu (
    MaDichVu int IDENTITY(1,1) CONSTRAINT PK_DichVu PRIMARY KEY,
    TenDichVu nvarchar(100) NOT NULL CONSTRAINT UQ_DichVu_Ten UNIQUE,
    DonGia decimal(18,2) NOT NULL CONSTRAINT CK_DichVu_DonGia CHECK (DonGia >= 0),
    DonViTinh nvarchar(30) NOT NULL,
    HoatDong bit NOT NULL CONSTRAINT DF_DichVu_HoatDong DEFAULT 1
);
GO

IF OBJECT_ID(N'dbo.DatPhong', N'U') IS NULL
CREATE TABLE dbo.DatPhong (
    MaDatPhong int IDENTITY(1,1) CONSTRAINT PK_DatPhong PRIMARY KEY,
    MaKhachHang int NOT NULL,
    MaPhong int NOT NULL,
    MaNguoiDung int NOT NULL,
    NgayDen date NOT NULL,
    NgayDi date NOT NULL,
    TienCoc decimal(18,2) NOT NULL CONSTRAINT DF_DatPhong_TienCoc DEFAULT 0,
    TrangThai nvarchar(20) NOT NULL CONSTRAINT DF_DatPhong_TrangThai DEFAULT N'Đã đặt',
    NgayTao datetime2(0) NOT NULL CONSTRAINT DF_DatPhong_NgayTao DEFAULT SYSDATETIME(),
    CONSTRAINT FK_DatPhong_KhachHang FOREIGN KEY (MaKhachHang) REFERENCES dbo.KhachHang(MaKhachHang),
    CONSTRAINT FK_DatPhong_Phong FOREIGN KEY (MaPhong) REFERENCES dbo.Phong(MaPhong),
    CONSTRAINT FK_DatPhong_NguoiDung FOREIGN KEY (MaNguoiDung) REFERENCES dbo.NguoiDung(MaNguoiDung),
    CONSTRAINT CK_DatPhong_Ngay CHECK (NgayDen < NgayDi),
    CONSTRAINT CK_DatPhong_TienCoc CHECK (TienCoc >= 0),
    CONSTRAINT CK_DatPhong_TrangThai CHECK (TrangThai IN (N'Đã đặt',N'Đã nhận',N'Đã hủy',N'Không đến'))
);
GO
-- Replace broad booking index with an index over current reservations only.
IF EXISTS(SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID(N'dbo.DatPhong') AND name=N'IX_DatPhong_Phong_Ngay')
    DROP INDEX IX_DatPhong_Phong_Ngay ON dbo.DatPhong;
IF NOT EXISTS(SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID(N'dbo.DatPhong') AND name=N'IX_DatPhong_DatDangDat_Phong_NgayDen')
    CREATE INDEX IX_DatPhong_DatDangDat_Phong_NgayDen ON dbo.DatPhong(MaPhong, NgayDen)
        INCLUDE (NgayDi, MaDatPhong) WHERE TrangThai=N'Đã đặt';
GO

IF OBJECT_ID(N'dbo.PhieuThue', N'U') IS NULL
CREATE TABLE dbo.PhieuThue (
    MaPhieuThue int IDENTITY(1,1) CONSTRAINT PK_PhieuThue PRIMARY KEY,
    MaDatPhong int NULL CONSTRAINT UQ_PhieuThue_DatPhong UNIQUE,
    MaKhachHang int NOT NULL,
    MaPhong int NOT NULL,
    MaNguoiDung int NOT NULL,
    NgayNhan datetime2(0) NOT NULL CONSTRAINT DF_PhieuThue_NgayNhan DEFAULT SYSDATETIME(),
    NgayDiDuKien date NOT NULL,
    NgayDiThucTe datetime2(0) NULL,
    GiaPhongMoiDem decimal(18,2) NOT NULL CONSTRAINT CK_PhieuThue_Gia CHECK (GiaPhongMoiDem > 0),
    TrangThai nvarchar(20) NOT NULL CONSTRAINT DF_PhieuThue_TrangThai DEFAULT N'Đang ở',
    CONSTRAINT FK_PhieuThue_DatPhong FOREIGN KEY (MaDatPhong) REFERENCES dbo.DatPhong(MaDatPhong),
    CONSTRAINT FK_PhieuThue_KhachHang FOREIGN KEY (MaKhachHang) REFERENCES dbo.KhachHang(MaKhachHang),
    CONSTRAINT FK_PhieuThue_Phong FOREIGN KEY (MaPhong) REFERENCES dbo.Phong(MaPhong),
    CONSTRAINT FK_PhieuThue_NguoiDung FOREIGN KEY (MaNguoiDung) REFERENCES dbo.NguoiDung(MaNguoiDung),
    CONSTRAINT CK_PhieuThue_TrangThai CHECK (TrangThai IN (N'Đang ở',N'Đã trả'))
);
GO
IF EXISTS(SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID(N'dbo.PhieuThue') AND name=N'IX_PhieuThue_Phong_Ngay')
    DROP INDEX IX_PhieuThue_Phong_Ngay ON dbo.PhieuThue;
IF NOT EXISTS(SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID(N'dbo.PhieuThue') AND name=N'IX_PhieuThue_DangO_Phong_NgayNhan')
    CREATE INDEX IX_PhieuThue_DangO_Phong_NgayNhan ON dbo.PhieuThue(MaPhong, NgayNhan)
        INCLUDE (NgayDiDuKien, NgayDiThucTe) WHERE TrangThai=N'Đang ở';
GO

IF OBJECT_ID(N'dbo.ChiTietDichVu', N'U') IS NULL
CREATE TABLE dbo.ChiTietDichVu (
    MaChiTietDichVu int IDENTITY(1,1) CONSTRAINT PK_ChiTietDichVu PRIMARY KEY,
    MaPhieuThue int NOT NULL,
    MaDichVu int NOT NULL,
    SoLuong decimal(12,2) NOT NULL CONSTRAINT CK_ChiTietDichVu_SoLuong CHECK (SoLuong > 0),
    DonGia decimal(18,2) NOT NULL CONSTRAINT CK_ChiTietDichVu_DonGia CHECK (DonGia >= 0),
    ThoiDiemSuDung datetime2(0) NOT NULL CONSTRAINT DF_ChiTietDichVu_ThoiDiem DEFAULT SYSDATETIME(),
    CONSTRAINT FK_ChiTietDichVu_PhieuThue FOREIGN KEY (MaPhieuThue) REFERENCES dbo.PhieuThue(MaPhieuThue),
    CONSTRAINT FK_ChiTietDichVu_DichVu FOREIGN KEY (MaDichVu) REFERENCES dbo.DichVu(MaDichVu)
);
GO

IF NOT EXISTS(SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID(N'dbo.ChiTietDichVu') AND name=N'IX_ChiTietDichVu_PhieuThue_ThoiDiem')
    CREATE INDEX IX_ChiTietDichVu_PhieuThue_ThoiDiem ON dbo.ChiTietDichVu(MaPhieuThue,ThoiDiemSuDung)
        INCLUDE (MaDichVu,SoLuong,DonGia);
GO

IF OBJECT_ID(N'dbo.HoaDon', N'U') IS NULL
CREATE TABLE dbo.HoaDon (
    MaHoaDon int IDENTITY(1,1) CONSTRAINT PK_HoaDon PRIMARY KEY,
    MaPhieuThue int NOT NULL CONSTRAINT UQ_HoaDon_PhieuThue UNIQUE,
    MaNguoiDung int NOT NULL,
    NgayLap datetime2(0) NOT NULL CONSTRAINT DF_HoaDon_NgayLap DEFAULT SYSDATETIME(),
    SoDem int NOT NULL CONSTRAINT CK_HoaDon_SoDem CHECK (SoDem >= 1),
    TienPhong decimal(18,2) NOT NULL CONSTRAINT CK_HoaDon_TienPhong CHECK (TienPhong >= 0),
    TienDichVu decimal(18,2) NOT NULL CONSTRAINT CK_HoaDon_TienDichVu CHECK (TienDichVu >= 0),
    PhuThu decimal(18,2) NOT NULL CONSTRAINT DF_HoaDon_PhuThu DEFAULT 0 CONSTRAINT CK_HoaDon_PhuThu CHECK (PhuThu >= 0),
    GiamGia decimal(18,2) NOT NULL CONSTRAINT DF_HoaDon_GiamGia DEFAULT 0 CONSTRAINT CK_HoaDon_GiamGia CHECK (GiamGia >= 0),
    TienCoc decimal(18,2) NOT NULL CONSTRAINT DF_HoaDon_TienCoc DEFAULT 0 CONSTRAINT CK_HoaDon_TienCoc CHECK (TienCoc >= 0),
    TongThanhToan decimal(18,2) NOT NULL CONSTRAINT CK_HoaDon_Tong CHECK (TongThanhToan >= 0),
    HinhThucThanhToan nvarchar(20) NOT NULL,
    CONSTRAINT FK_HoaDon_PhieuThue FOREIGN KEY (MaPhieuThue) REFERENCES dbo.PhieuThue(MaPhieuThue),
    CONSTRAINT FK_HoaDon_NguoiDung FOREIGN KEY (MaNguoiDung) REFERENCES dbo.NguoiDung(MaNguoiDung),
    CONSTRAINT CK_HoaDon_HinhThuc CHECK (HinhThucThanhToan IN (N'Tiền mặt',N'Chuyển khoản',N'Thẻ'))
);
GO

IF NOT EXISTS(SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID(N'dbo.HoaDon') AND name=N'IX_HoaDon_NgayLap')
    CREATE INDEX IX_HoaDon_NgayLap ON dbo.HoaDon(NgayLap DESC) INCLUDE (MaPhieuThue,TongThanhToan);
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

CREATE OR ALTER PROCEDURE dbo.sp_KiemTraTrungLich
    @MaPhong int, @NgayDen date, @NgayDi date, @MaDatPhongBoQua int = NULL
AS
BEGIN
    SET NOCOUNT ON;
    IF @NgayDen >= @NgayDi THROW 50001, N'Ngày đến phải trước ngày đi.', 1;
    IF NOT EXISTS (
        SELECT 1 FROM dbo.Phong WITH (UPDLOCK, HOLDLOCK)
        WHERE MaPhong=@MaPhong AND HoatDong=1 AND TrangThai NOT IN (N'Bảo trì',N'Dọn dẹp')
    ) THROW 50009, N'Phòng không hoạt động, đang bảo trì hoặc chưa hoàn tất dọn dẹp.', 1;
    SELECT CAST(CASE WHEN EXISTS (
        SELECT 1 FROM dbo.DatPhong WITH (UPDLOCK, HOLDLOCK)
        WHERE MaPhong=@MaPhong AND TrangThai=N'Đã đặt'
          AND (@MaDatPhongBoQua IS NULL OR MaDatPhong<>@MaDatPhongBoQua)
          AND NgayDen < @NgayDi AND @NgayDen < NgayDi
        UNION ALL
        SELECT 1 FROM dbo.PhieuThue WITH (UPDLOCK, HOLDLOCK)
        WHERE MaPhong=@MaPhong AND TrangThai=N'Đang ở'
          AND NgayNhan < CAST(@NgayDi AS datetime2)
          AND @NgayDen < NgayDiDuKien
    ) THEN 1 ELSE 0 END AS bit) AS BiTrungLich;
END;
GO

CREATE OR ALTER PROCEDURE dbo.sp_NhanPhong
    @MaPhong int, @MaKhachHang int, @MaNguoiDung int, @NgayDiDuKien date,
    @MaDatPhong int = NULL, @MaPhieuThue int OUTPUT
AS
BEGIN
    SET NOCOUNT ON; SET XACT_ABORT ON;
    BEGIN TRANSACTION;
    BEGIN TRY
        DECLARE @NgayDen date=CONVERT(date,SYSDATETIME());
        IF @NgayDiDuKien<=@NgayDen THROW 50002,N'Ngày đi dự kiến phải sau ngày nhận.',1;
        IF NOT EXISTS(SELECT 1 FROM dbo.Phong WITH (UPDLOCK,HOLDLOCK) WHERE MaPhong=@MaPhong AND HoatDong=1 AND TrangThai IN (N'Trống',N'Đã đặt'))
            THROW 50003,N'Phòng không sẵn sàng nhận.',1;
        IF EXISTS(SELECT 1 FROM dbo.PhieuThue WITH (UPDLOCK,HOLDLOCK) WHERE MaPhong=@MaPhong AND TrangThai=N'Đang ở')
            THROW 50004,N'Phòng đang có khách lưu trú.',1;
        IF EXISTS(SELECT 1 FROM dbo.DatPhong WITH (UPDLOCK,HOLDLOCK)
                  WHERE MaPhong=@MaPhong AND TrangThai=N'Đã đặt'
                    AND (@MaDatPhong IS NULL OR MaDatPhong<>@MaDatPhong)
                    AND NgayDen<@NgayDiDuKien AND @NgayDen<NgayDi)
            THROW 50010,N'Phòng có đặt chỗ khác trong khoảng lưu trú dự kiến.',1;
        IF @MaDatPhong IS NOT NULL AND NOT EXISTS(SELECT 1 FROM dbo.DatPhong WHERE MaDatPhong=@MaDatPhong AND MaPhong=@MaPhong AND MaKhachHang=@MaKhachHang AND TrangThai=N'Đã đặt')
            THROW 50005,N'Đặt phòng không hợp lệ.',1;
        DECLARE @Gia decimal(18,2)=(SELECT l.GiaMoiDem FROM dbo.Phong p JOIN dbo.LoaiPhong l ON p.MaLoaiPhong=l.MaLoaiPhong WHERE p.MaPhong=@MaPhong);
        INSERT dbo.PhieuThue(MaDatPhong,MaKhachHang,MaPhong,MaNguoiDung,NgayDiDuKien,GiaPhongMoiDem)
        VALUES(@MaDatPhong,@MaKhachHang,@MaPhong,@MaNguoiDung,@NgayDiDuKien,@Gia);
        SET @MaPhieuThue=SCOPE_IDENTITY();
        UPDATE dbo.Phong SET TrangThai=N'Đang ở' WHERE MaPhong=@MaPhong;
        IF @MaDatPhong IS NOT NULL UPDATE dbo.DatPhong SET TrangThai=N'Đã nhận' WHERE MaDatPhong=@MaDatPhong;
        COMMIT;
    END TRY BEGIN CATCH
        IF @@TRANCOUNT>0 ROLLBACK; THROW;
    END CATCH
END;
GO

CREATE OR ALTER PROCEDURE dbo.sp_TraPhong
    @MaPhieuThue int, @MaNguoiDung int, @NgayTra datetime2(0),
    @PhuThu decimal(18,2)=0, @GiamGia decimal(18,2)=0,
    @HinhThucThanhToan nvarchar(20)=N'Tiền mặt'
AS
BEGIN
    SET NOCOUNT ON; SET XACT_ABORT ON;
    BEGIN TRANSACTION;
    BEGIN TRY
        DECLARE @MaPhong int,@MaDatPhong int,@NgayNhan datetime2(0),@Gia decimal(18,2),@TienCoc decimal(18,2)=0,
                @TienDichVu decimal(18,2),@SoDem int,@TienPhong decimal(18,2),@Tong decimal(18,2);
        SELECT @MaPhong=MaPhong,@MaDatPhong=MaDatPhong,@NgayNhan=NgayNhan,@Gia=GiaPhongMoiDem
        FROM dbo.PhieuThue WITH (UPDLOCK,HOLDLOCK) WHERE MaPhieuThue=@MaPhieuThue AND TrangThai=N'Đang ở';
        IF @MaPhong IS NULL THROW 50006,N'Phiếu thuê không tồn tại hoặc đã trả.',1;
        IF @NgayTra<=@NgayNhan THROW 50007,N'Thời điểm trả phải sau thời điểm nhận.',1;
        IF @MaDatPhong IS NOT NULL SELECT @TienCoc=TienCoc FROM dbo.DatPhong WHERE MaDatPhong=@MaDatPhong;
        SET @SoDem=CASE WHEN DATEDIFF(day,CONVERT(date,@NgayNhan),CONVERT(date,@NgayTra))<1 THEN 1 ELSE DATEDIFF(day,CONVERT(date,@NgayNhan),CONVERT(date,@NgayTra)) END;
        SET @TienPhong=@SoDem*@Gia;
        SELECT @TienDichVu=COALESCE(SUM(SoLuong*DonGia),0) FROM dbo.ChiTietDichVu WHERE MaPhieuThue=@MaPhieuThue;
        IF @PhuThu<0 OR @GiamGia<0 OR @GiamGia>@TienPhong+@TienDichVu+@PhuThu THROW 50008,N'Phụ thu hoặc giảm giá không hợp lệ.',1;
        SET @Tong=@TienPhong+@TienDichVu+@PhuThu-@GiamGia-@TienCoc;
        IF @Tong<0 SET @Tong=0;
        INSERT dbo.HoaDon(MaPhieuThue,MaNguoiDung,SoDem,TienPhong,TienDichVu,PhuThu,GiamGia,TienCoc,TongThanhToan,HinhThucThanhToan)
        VALUES(@MaPhieuThue,@MaNguoiDung,@SoDem,@TienPhong,@TienDichVu,@PhuThu,@GiamGia,@TienCoc,@Tong,@HinhThucThanhToan);
        UPDATE dbo.PhieuThue SET TrangThai=N'Đã trả',NgayDiThucTe=@NgayTra WHERE MaPhieuThue=@MaPhieuThue;
        UPDATE dbo.Phong SET TrangThai=N'Dọn dẹp' WHERE MaPhong=@MaPhong;
        COMMIT;
        SELECT CAST(SCOPE_IDENTITY() AS int) AS MaHoaDon,@SoDem AS SoDem,@TienPhong AS TienPhong,
               @TienDichVu AS TienDichVu,@PhuThu AS PhuThu,@GiamGia AS GiamGia,@TienCoc AS TienCoc,@Tong AS TongThanhToan;
    END TRY BEGIN CATCH
        IF @@TRANCOUNT>0 ROLLBACK; THROW;
    END CATCH
END;
GO

CREATE OR ALTER PROCEDURE dbo.sp_ThongKeDoanhThu
    @TuNgay date, @DenNgay date
AS
BEGIN
    SET NOCOUNT ON;
    IF @TuNgay>@DenNgay THROW 50009,N'Từ ngày phải nhỏ hơn hoặc bằng đến ngày.',1;
    SELECT CONVERT(date,h.NgayLap) AS Ngay, l.TenLoaiPhong,
           COUNT_BIG(*) AS SoHoaDon, SUM(h.TongThanhToan) AS DoanhThu
    FROM dbo.HoaDon h JOIN dbo.PhieuThue pt ON h.MaPhieuThue=pt.MaPhieuThue
    JOIN dbo.Phong p ON pt.MaPhong=p.MaPhong JOIN dbo.LoaiPhong l ON p.MaLoaiPhong=l.MaLoaiPhong
    WHERE h.NgayLap>=@TuNgay AND h.NgayLap<DATEADD(day,1,@DenNgay)
    GROUP BY CONVERT(date,h.NgayLap),l.TenLoaiPhong ORDER BY Ngay,l.TenLoaiPhong;
END;
GO

CREATE OR ALTER PROCEDURE dbo.sp_ThongKeCongSuat
    @TuNgay date, @DenNgay date
AS
BEGIN
    SET NOCOUNT ON;
    IF @TuNgay>@DenNgay THROW 50009,N'Từ ngày phải nhỏ hơn hoặc bằng đến ngày.',1;
    IF DATEDIFF(day,@TuNgay,@DenNgay)>365 THROW 50011,N'Khoảng thống kê tối đa 366 ngày.',1;
    DECLARE @TongPhong int=(SELECT COUNT(*) FROM dbo.Phong WHERE HoatDong=1);
    DECLARE @Homnay date=CONVERT(date,SYSDATETIME());
    -- Mỗi phiếu thuê chiếm phòng trong khoảng [ngày nhận, ngày đi); tối thiểu 1 đêm.
    -- Phiếu đang ở quá hạn dự kiến vẫn tính đến hết hôm nay.
    ;WITH Ngay AS (
        SELECT TOP (DATEDIFF(day,@TuNgay,@DenNgay)+1)
               DATEADD(day,ROW_NUMBER() OVER (ORDER BY (SELECT NULL))-1,@TuNgay) AS Ngay
        FROM sys.all_objects a CROSS JOIN sys.all_objects b
    ), Luu AS (
        SELECT pt.MaPhong, CONVERT(date,pt.NgayNhan) AS TuNgay,
               CASE WHEN x.NgayDi > CONVERT(date,pt.NgayNhan) THEN x.NgayDi
                    ELSE DATEADD(day,1,CONVERT(date,pt.NgayNhan)) END AS DenNgay
        FROM dbo.PhieuThue pt
        CROSS APPLY (SELECT COALESCE(CONVERT(date,pt.NgayDiThucTe),
                                     CASE WHEN pt.NgayDiDuKien > @Homnay THEN pt.NgayDiDuKien
                                          ELSE DATEADD(day,1,@Homnay) END) AS NgayDi) x
    )
    SELECT n.Ngay, COUNT(DISTINCT l.MaPhong) AS SoPhongCoKhach, @TongPhong AS TongPhong,
           CAST(CASE WHEN @TongPhong=0 THEN 0
                     ELSE 100.0*COUNT(DISTINCT l.MaPhong)/@TongPhong END AS decimal(5,2)) AS TyLe
    FROM Ngay n LEFT JOIN Luu l ON l.TuNgay<=n.Ngay AND n.Ngay<l.DenNgay
    GROUP BY n.Ngay ORDER BY n.Ngay;
END;
GO
