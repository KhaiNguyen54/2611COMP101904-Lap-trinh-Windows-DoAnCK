using System;
using System.Data;
using Microsoft.Data.SqlClient;

namespace QuanLyKhachSan.Repositories
{
    /// <summary>Truy vấn báo cáo qua stored procedure.</summary>
    public class BaoCaoRepository
    {
        /// <summary>Doanh thu theo ngày lập hóa đơn và loại phòng: Ngay, TenLoaiPhong, SoHoaDon, DoanhThu.</summary>
        public DataTable DoanhThu(DateTime tuNgay, DateTime denNgay)
        {
            return ChayThongKe("dbo.sp_ThongKeDoanhThu", tuNgay, denNgay);
        }

        /// <summary>Công suất phòng theo ngày: Ngay, SoPhongCoKhach, TongPhong, TyLe (%).</summary>
        public DataTable CongSuat(DateTime tuNgay, DateTime denNgay)
        {
            return ChayThongKe("dbo.sp_ThongKeCongSuat", tuNgay, denNgay);
        }

        private static DataTable ChayThongKe(string tenSp, DateTime tuNgay, DateTime denNgay)
        {
            return Database.TruyVan(tenSp, new[]
            {
                new SqlParameter("@TuNgay", SqlDbType.Date) { Value = tuNgay.Date },
                new SqlParameter("@DenNgay", SqlDbType.Date) { Value = denNgay.Date }
            }, null, CommandType.StoredProcedure);
        }
    }
}
