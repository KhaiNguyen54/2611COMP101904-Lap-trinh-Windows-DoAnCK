using System;
using System.Data;
using Microsoft.Data.SqlClient;
using QuanLyKhachSan.DTO;

namespace QuanLyKhachSan.DAL
{
    public class PhongDAL
    {
        /// <summary>
        /// Trạng thái hiển thị của phòng, suy ra từ dữ liệu thật: có phiếu thuê đang ở → Đang ở; Dọn dẹp/Bảo trì giữ nguyên;
        /// còn lượt đặt chưa tới ngày đi → Đã đặt; ngược lại Trống. Dùng chung cho tìm kiếm và sơ đồ phòng.
        /// </summary>
        private const string ApDungTrangThaiHienTai = @"
            CROSS APPLY (SELECT CASE
                WHEN EXISTS (SELECT 1 FROM dbo.PhieuThue pt WHERE pt.MaPhong = p.MaPhong AND pt.TrangThai = N'Đang ở') THEN N'Đang ở'
                WHEN p.TrangThai = N'Dọn dẹp' THEN N'Dọn dẹp'
                WHEN p.TrangThai = N'Bảo trì' THEN N'Bảo trì'
                WHEN EXISTS (SELECT 1 FROM dbo.DatPhong d WHERE d.MaPhong = p.MaPhong AND d.TrangThai = N'Đã đặt'
                             AND d.NgayDi > CONVERT(date, SYSDATETIME())) THEN N'Đã đặt'
                ELSE N'Trống' END AS TrangThai) AS trangThaiHienTai";

        public DataTable TimKiem(string tuKhoa = null, int? tang = null, int? maLoai = null, string trangThai = null)
        {
            const string sql = @"
                SELECT p.MaPhong, p.SoPhong, p.Tang, p.MaLoaiPhong, l.TenLoaiPhong,
                       trangThaiHienTai.TrangThai, p.GhiChu, p.HoatDong
                FROM dbo.Phong p JOIN dbo.LoaiPhong l ON p.MaLoaiPhong = l.MaLoaiPhong"
                + ApDungTrangThaiHienTai + @"
                WHERE (@TuKhoa IS NULL OR p.SoPhong LIKE @TuKhoa)
                  AND (@Tang IS NULL OR p.Tang = @Tang)
                  AND (@Loai IS NULL OR p.MaLoaiPhong = @Loai)
                  AND (@TrangThai IS NULL OR trangThaiHienTai.TrangThai = @TrangThai)
                ORDER BY p.Tang, p.SoPhong
                OPTION (RECOMPILE)";

            string keyword = string.IsNullOrWhiteSpace(tuKhoa) ? null : tuKhoa.Trim() + "%";
            return Database.TruyVan(sql, new[]
            {
                new SqlParameter("@TuKhoa", SqlDbType.NVarChar, 11) { Value = (object)keyword ?? DBNull.Value },
                new SqlParameter("@Tang", SqlDbType.Int) { Value = (object)tang ?? DBNull.Value },
                new SqlParameter("@Loai", SqlDbType.Int) { Value = (object)maLoai ?? DBNull.Value },
                new SqlParameter("@TrangThai", SqlDbType.NVarChar, 20) { Value = (object)trangThai ?? DBNull.Value }
            });
        }

        /// <summary>
        /// Dữ liệu cho sơ đồ phòng theo tầng: các cột của <see cref="TimKiem"/> cộng giá phòng, trạng thái lưu trong bảng
        /// (TrangThaiGoc), khách đang ở và ngày đi dự kiến.
        /// </summary>
        public DataTable LaySoDo()
        {
            const string sql = @"
                SELECT p.MaPhong, p.SoPhong, p.Tang, l.TenLoaiPhong, l.GiaMoiDem,
                       p.TrangThai AS TrangThaiGoc, trangThaiHienTai.TrangThai,
                       k.HoTen AS KhachDangO, ptO.NgayDiDuKien
                FROM dbo.Phong p JOIN dbo.LoaiPhong l ON p.MaLoaiPhong = l.MaLoaiPhong"
                + ApDungTrangThaiHienTai + @"
                LEFT JOIN dbo.PhieuThue ptO ON ptO.MaPhong = p.MaPhong AND ptO.TrangThai = N'Đang ở'
                LEFT JOIN dbo.KhachHang k ON k.MaKhachHang = ptO.MaKhachHang
                WHERE p.HoatDong = 1
                ORDER BY p.Tang, p.SoPhong";
            return Database.TruyVan(sql);
        }

        /// <summary>Phòng hoạt động, không bảo trì/dọn dẹp và không vướng lịch trong khoảng [ngayDen, ngayDi).</summary>
        public DataTable TimKiemTrongKhoang(DateTime ngayDen, DateTime ngayDi)
        {
            const string sql = @"
                SELECT p.MaPhong, p.SoPhong, p.Tang, p.MaLoaiPhong, l.TenLoaiPhong, l.GiaMoiDem
                FROM dbo.Phong p JOIN dbo.LoaiPhong l ON p.MaLoaiPhong = l.MaLoaiPhong
                WHERE p.HoatDong = 1 AND p.TrangThai NOT IN (N'Bảo trì', N'Dọn dẹp')
                  AND NOT EXISTS (
                      SELECT 1 FROM dbo.DatPhong d
                      WHERE d.MaPhong = p.MaPhong AND d.TrangThai = N'Đã đặt'
                        AND d.NgayDen < @NgayDi AND @NgayDen < d.NgayDi)
                  AND NOT EXISTS (
                      SELECT 1 FROM dbo.PhieuThue pt
                      WHERE pt.MaPhong = p.MaPhong AND pt.TrangThai = N'Đang ở'
                        AND pt.NgayNhan < CONVERT(datetime2, @NgayDi)
                        AND CONVERT(datetime2, @NgayDen) < CONVERT(datetime2, pt.NgayDiDuKien))
                ORDER BY p.Tang, p.SoPhong
                OPTION (RECOMPILE)";
            return Database.TruyVan(sql, new[]
            {
                new SqlParameter("@NgayDen", SqlDbType.Date) { Value = ngayDen.Date },
                new SqlParameter("@NgayDi", SqlDbType.Date) { Value = ngayDi.Date }
            });
        }

        public int Them(Phong x)
        {
            return Database.ThucThi(
                "INSERT dbo.Phong(SoPhong,Tang,MaLoaiPhong,GhiChu) VALUES(@SoPhong,@Tang,@Loai,@GhiChu)",
                ThamSo(x));
        }

        public int CapNhat(Phong x)
        {
            return Database.ThucThi(
                "UPDATE dbo.Phong SET SoPhong=@SoPhong,Tang=@Tang,MaLoaiPhong=@Loai,GhiChu=@GhiChu WHERE MaPhong=@Ma",
                ThamSo(x, true));
        }

        /// <summary>Dọn xong: Dọn dẹp → Trống (khi không còn phiếu thuê đang ở).</summary>
        public int HoanTatDonDep(int maPhong)
        {
            const string sql = @"
                UPDATE dbo.Phong SET TrangThai = N'Trống'
                WHERE MaPhong = @Ma AND HoatDong = 1 AND TrangThai = N'Dọn dẹp'
                  AND NOT EXISTS (SELECT 1 FROM dbo.PhieuThue WHERE MaPhong = @Ma AND TrangThai = N'Đang ở')";
            return Database.ThucThi(sql, new[] { new SqlParameter("@Ma", SqlDbType.Int) { Value = maPhong } });
        }

        /// <summary>Đưa phòng Trống vào Bảo trì, chỉ khi không còn lượt đặt sắp tới (QT05).</summary>
        public int BatDauBaoTri(int maPhong)
        {
            const string sql = @"
                UPDATE dbo.Phong SET TrangThai = N'Bảo trì'
                WHERE MaPhong = @Ma AND HoatDong = 1 AND TrangThai = N'Trống'
                  AND NOT EXISTS (SELECT 1 FROM dbo.PhieuThue WHERE MaPhong = @Ma AND TrangThai = N'Đang ở')
                  AND NOT EXISTS (SELECT 1 FROM dbo.DatPhong
                                  WHERE MaPhong = @Ma AND TrangThai = N'Đã đặt' AND NgayDi > CONVERT(date, SYSDATETIME()))";
            return Database.ThucThi(sql, new[] { new SqlParameter("@Ma", SqlDbType.Int) { Value = maPhong } });
        }

        /// <summary>Kết thúc bảo trì: Bảo trì → Trống.</summary>
        public int KetThucBaoTri(int maPhong)
        {
            return Database.ThucThi(
                "UPDATE dbo.Phong SET TrangThai = N'Trống' WHERE MaPhong = @Ma AND HoatDong = 1 AND TrangThai = N'Bảo trì'",
                new[] { new SqlParameter("@Ma", SqlDbType.Int) { Value = maPhong } });
        }

        public int NgungSuDung(int ma)
        {
            const string sql = @"
                UPDATE dbo.Phong SET HoatDong = 0
                WHERE MaPhong = @Ma AND TrangThai IN (N'Trống', N'Bảo trì')
                  AND NOT EXISTS (SELECT 1 FROM dbo.DatPhong WHERE MaPhong = @Ma AND TrangThai = N'Đã đặt')
                  AND NOT EXISTS (SELECT 1 FROM dbo.PhieuThue WHERE MaPhong = @Ma AND TrangThai = N'Đang ở')";
            return Database.ThucThi(sql, new[] { new SqlParameter("@Ma", SqlDbType.Int) { Value = ma } });
        }

        private static SqlParameter[] ThamSo(Phong x, bool coMa = false)
        {
            var p = new[]
            {
                new SqlParameter("@SoPhong", SqlDbType.NVarChar, 10) { Value = x.SoPhong },
                new SqlParameter("@Tang", SqlDbType.Int) { Value = x.Tang },
                new SqlParameter("@Loai", SqlDbType.Int) { Value = x.MaLoaiPhong },
                new SqlParameter("@GhiChu", SqlDbType.NVarChar, 300) { Value = (object)x.GhiChu ?? DBNull.Value }
            };
            if (!coMa) return p;

            Array.Resize(ref p, p.Length + 1);
            p[p.Length - 1] = new SqlParameter("@Ma", SqlDbType.Int) { Value = x.MaPhong };
            return p;
        }
    }
}
