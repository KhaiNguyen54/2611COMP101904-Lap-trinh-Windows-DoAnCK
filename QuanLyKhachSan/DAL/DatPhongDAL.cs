using System;
using System.Data;
using Microsoft.Data.SqlClient;
using QuanLyKhachSan.DTO;

namespace QuanLyKhachSan.DAL
{
    public class DatPhongDAL
    {
        public DataTable TimKiem(DateTime? tuNgay = null, DateTime? denNgay = null, string trangThai = null)
        {
            const string sql = @"
                SELECT d.MaDatPhong, d.MaKhachHang, d.MaPhong, d.NgayDen, d.NgayDi, d.TienCoc, d.TrangThai,
                       k.HoTen, p.SoPhong, p.Tang
                FROM dbo.DatPhong d
                JOIN dbo.KhachHang k ON d.MaKhachHang = k.MaKhachHang
                JOIN dbo.Phong p ON d.MaPhong = p.MaPhong
                WHERE (@Tu IS NULL OR d.NgayDen >= @Tu)
                  AND (@Den IS NULL OR d.NgayDen <= @Den)
                  AND (@TrangThai IS NULL OR d.TrangThai = @TrangThai)
                ORDER BY d.NgayDen, p.SoPhong
                OPTION (RECOMPILE)";

            return Database.TruyVan(sql, new[]
            {
                new SqlParameter("@Tu", SqlDbType.Date) { Value = (object)tuNgay ?? DBNull.Value },
                new SqlParameter("@Den", SqlDbType.Date) { Value = (object)denNgay ?? DBNull.Value },
                new SqlParameter("@TrangThai", SqlDbType.NVarChar, 20) { Value = (object)trangThai ?? DBNull.Value }
            });
        }

        public bool KiemTraTrungLich(int maPhong, DateTime ngayDen, DateTime ngayDi, int? boQua = null)
        {
            object result = Database.ThucThiGiaTri("dbo.sp_KiemTraTrungLich", new[]
            {
                new SqlParameter("@MaPhong", SqlDbType.Int) { Value = maPhong },
                new SqlParameter("@NgayDen", SqlDbType.Date) { Value = ngayDen.Date },
                new SqlParameter("@NgayDi", SqlDbType.Date) { Value = ngayDi.Date },
                new SqlParameter("@MaDatPhongBoQua", SqlDbType.Int) { Value = (object)boQua ?? DBNull.Value }
            }, null, CommandType.StoredProcedure);
            return Convert.ToBoolean(result);
        }

        /// <summary>
        /// Thêm đặt phòng trong một transaction Serializable: kiểm tra phòng (hoạt động, không bảo trì),
        /// kiểm tra giao nhau khoảng ngày rồi mới ghi.
        /// </summary>
        public int Them(DatPhong x)
        {
            return Database.ThucHienTrongTransaction((connection, transaction) =>
            {
                KiemTraPhong(connection, transaction, x);

                using (SqlCommand check = new SqlCommand("dbo.sp_KiemTraTrungLich", connection, transaction))
                {
                    check.CommandType = CommandType.StoredProcedure;
                    check.Parameters.Add("@MaPhong", SqlDbType.Int).Value = x.MaPhong;
                    check.Parameters.Add("@NgayDen", SqlDbType.Date).Value = x.NgayDen.Date;
                    check.Parameters.Add("@NgayDi", SqlDbType.Date).Value = x.NgayDi.Date;
                    check.Parameters.Add("@MaDatPhongBoQua", SqlDbType.Int).Value = DBNull.Value;
                    if (Convert.ToBoolean(check.ExecuteScalar()))
                        throw new InvalidOperationException("Phòng đã có lịch trong khoảng ngày này.");
                }

                const string sql = @"
                    INSERT dbo.DatPhong(MaKhachHang, MaPhong, MaNguoiDung, NgayDen, NgayDi, TienCoc)
                    VALUES (@Khach, @Phong, @NguoiDung, @Den, @Di, @Coc);
                    SELECT CAST(SCOPE_IDENTITY() AS int);";
                using (SqlCommand cmd = new SqlCommand(sql, connection, transaction))
                {
                    cmd.Parameters.Add("@Khach", SqlDbType.Int).Value = x.MaKhachHang;
                    cmd.Parameters.Add("@Phong", SqlDbType.Int).Value = x.MaPhong;
                    cmd.Parameters.Add("@NguoiDung", SqlDbType.Int).Value = x.MaNguoiDung;
                    cmd.Parameters.Add("@Den", SqlDbType.Date).Value = x.NgayDen.Date;
                    cmd.Parameters.Add("@Di", SqlDbType.Date).Value = x.NgayDi.Date;
                    SqlParameter coc = cmd.Parameters.Add("@Coc", SqlDbType.Decimal);
                    coc.Precision = 18;
                    coc.Scale = 2;
                    coc.Value = x.TienCoc;
                    return Convert.ToInt32(cmd.ExecuteScalar());
                }
            }, IsolationLevel.Serializable);
        }

        private static void KiemTraPhong(SqlConnection connection, SqlTransaction transaction, DatPhong x)
        {
            const string sql = @"
                SELECT HoatDong, TrangThai FROM dbo.Phong WITH (UPDLOCK) WHERE MaPhong = @Phong";

            bool hoatDong;
            string trangThai;
            using (SqlCommand cmd = new SqlCommand(sql, connection, transaction))
            {
                cmd.Parameters.Add("@Phong", SqlDbType.Int).Value = x.MaPhong;
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (!reader.Read())
                        throw new InvalidOperationException("Phòng không tồn tại.");
                    hoatDong = reader.GetBoolean(0);
                    trangThai = reader.GetString(1);
                }
            }

            if (!hoatDong)
                throw new InvalidOperationException("Phòng đã ngừng sử dụng.");
            if (trangThai == TrangThaiPhong.BaoTri)
                throw new InvalidOperationException("Phòng đang bảo trì, không thể đặt.");
        }

        /// <summary>Giá thuê 1 đêm hiện hành của phòng (theo loại phòng). Null nếu không có phòng.</summary>
        public decimal? LayGiaPhong(int maPhong)
        {
            object v = Database.ThucThiGiaTri(
                "SELECT l.GiaMoiDem FROM dbo.Phong p JOIN dbo.LoaiPhong l ON p.MaLoaiPhong = l.MaLoaiPhong WHERE p.MaPhong = @Ma",
                new[] { new SqlParameter("@Ma", SqlDbType.Int) { Value = maPhong } });
            return v == null || v == DBNull.Value ? (decimal?)null : Convert.ToDecimal(v);
        }

        public int Huy(int ma)
        {
            return CapNhatTrangThai(ma, "Đã hủy", "TrangThai = N'Đã đặt'");
        }

        /// <summary>Đánh dấu "Không đến" cho lượt đặt đã quá ngày đến mà khách chưa nhận phòng.</summary>
        public int DanhDauKhongDen(int ma)
        {
            return CapNhatTrangThai(ma, "Không đến", "TrangThai = N'Đã đặt' AND NgayDen < CONVERT(date, SYSDATETIME())");
        }

        private static int CapNhatTrangThai(int ma, string trangThaiMoi, string dieuKien)
        {
            string sql = "UPDATE dbo.DatPhong SET TrangThai = @Moi WHERE MaDatPhong = @Ma AND " + dieuKien;
            return Database.ThucThi(sql, new[]
            {
                new SqlParameter("@Moi", SqlDbType.NVarChar, 20) { Value = trangThaiMoi },
                new SqlParameter("@Ma", SqlDbType.Int) { Value = ma }
            });
        }
    }
}
