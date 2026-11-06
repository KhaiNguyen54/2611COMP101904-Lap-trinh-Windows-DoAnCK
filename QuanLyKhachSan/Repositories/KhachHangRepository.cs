using System;
using System.Data;
using Microsoft.Data.SqlClient;
using QuanLyKhachSan.Models;

namespace QuanLyKhachSan.Repositories
{
    public class KhachHangRepository
    {
        public DataTable TimKiem(string tuKhoa = null)
        {
            const string sql = @"SELECT MaKhachHang,HoTen,SoGiayTo,SoDienThoai,Email,DiaChi FROM dbo.KhachHang
                                 WHERE @TuKhoa IS NULL OR HoTen LIKE @TuKhoa OR SoGiayTo LIKE @TuKhoa OR SoDienThoai LIKE @TuKhoa
                                 ORDER BY HoTen OPTION (RECOMPILE)";
            string keyword = string.IsNullOrWhiteSpace(tuKhoa) ? null : tuKhoa.Trim() + "%";
            return Database.TruyVan(sql,new[]{new SqlParameter("@TuKhoa",SqlDbType.NVarChar,121){Value=(object)keyword??DBNull.Value}});
        }
        public int Them(KhachHang x) => Database.ThucThi("INSERT dbo.KhachHang(HoTen,SoGiayTo,SoDienThoai,Email,DiaChi) VALUES(@Ten,@GiayTo,@DienThoai,@Email,@DiaChi)", ThamSo(x));
        public int CapNhat(KhachHang x) => Database.ThucThi("UPDATE dbo.KhachHang SET HoTen=@Ten,SoGiayTo=@GiayTo,SoDienThoai=@DienThoai,Email=@Email,DiaChi=@DiaChi WHERE MaKhachHang=@Ma", ThamSo(x,true));
        private static SqlParameter[] ThamSo(KhachHang x,bool coMa=false)
        {
            var p=new[]{new SqlParameter("@Ten",SqlDbType.NVarChar,120){Value=x.HoTen},new SqlParameter("@GiayTo",SqlDbType.VarChar,12){Value=x.SoGiayTo},new SqlParameter("@DienThoai",SqlDbType.VarChar,10){Value=x.SoDienThoai},new SqlParameter("@Email",SqlDbType.NVarChar,254){Value=(object)x.Email??DBNull.Value},new SqlParameter("@DiaChi",SqlDbType.NVarChar,300){Value=(object)x.DiaChi??DBNull.Value}};
            if(!coMa)return p;System.Array.Resize(ref p,p.Length+1);p[p.Length-1]=new SqlParameter("@Ma",SqlDbType.Int){Value=x.MaKhachHang};return p;
        }
    }
}
