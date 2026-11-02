using System;
using System.Data;
using Microsoft.Data.SqlClient;
using QuanLyKhachSan.DTO;

namespace QuanLyKhachSan.DAL
{
    public class LoaiPhongDAL
    {
        public DataTable TimKiem(string tuKhoa = null)
        {
            const string sql = @"SELECT MaLoaiPhong,TenLoaiPhong,MoTa,GiaMoiDem,SucChua,HoatDong
                                 FROM dbo.LoaiPhong WHERE @TuKhoa IS NULL OR TenLoaiPhong LIKE @TuKhoa
                                 ORDER BY TenLoaiPhong OPTION (RECOMPILE)";
            string keyword = string.IsNullOrWhiteSpace(tuKhoa) ? null : tuKhoa.Trim() + "%";
            return Database.TruyVan(sql, new[] { new SqlParameter("@TuKhoa", SqlDbType.NVarChar, 81) { Value = (object)keyword ?? DBNull.Value } });
        }

        public int Them(LoaiPhong item) => Database.ThucThi(
            "INSERT dbo.LoaiPhong(TenLoaiPhong,MoTa,GiaMoiDem,SucChua) VALUES(@Ten,@MoTa,@Gia,@SucChua)", ThamSo(item));

        public int CapNhat(LoaiPhong item) => Database.ThucThi(
            "UPDATE dbo.LoaiPhong SET TenLoaiPhong=@Ten,MoTa=@MoTa,GiaMoiDem=@Gia,SucChua=@SucChua WHERE MaLoaiPhong=@Ma",
            ThamSo(item, true));

        public int NgungSuDung(int ma) => Database.ThucThi(
            "UPDATE dbo.LoaiPhong SET HoatDong=0 WHERE MaLoaiPhong=@Ma AND NOT EXISTS(SELECT 1 FROM dbo.Phong WHERE MaLoaiPhong=@Ma AND HoatDong=1)",
            new[] { new SqlParameter("@Ma", SqlDbType.Int) { Value = ma } });

        private static SqlParameter[] ThamSo(LoaiPhong x, bool coMa = false)
        {
            var p = new[] {
                new SqlParameter("@Ten",SqlDbType.NVarChar,80){Value=x.TenLoaiPhong},
                new SqlParameter("@MoTa",SqlDbType.NVarChar,300){Value=(object)x.MoTa??DBNull.Value},
                new SqlParameter("@Gia",SqlDbType.Decimal){Precision=18,Scale=2,Value=x.GiaMoiDem},
                new SqlParameter("@SucChua",SqlDbType.Int){Value=x.SucChua}
            };
            if (!coMa) return p;
            Array.Resize(ref p, p.Length + 1); p[p.Length - 1] = new SqlParameter("@Ma", SqlDbType.Int) { Value = x.MaLoaiPhong }; return p;
        }
    }
}
