using System;
using System.Data;
using Microsoft.Data.SqlClient;
using QuanLyKhachSan.DTO;

namespace QuanLyKhachSan.DAL
{
    public class DichVuDAL
    {
        public DataTable TimKiem(string tuKhoa=null,bool chiDangHoatDong=false)
        {
            const string sql="SELECT MaDichVu,TenDichVu,DonGia,DonViTinh,HoatDong FROM dbo.DichVu WHERE (@TuKhoa IS NULL OR TenDichVu LIKE @TuKhoa) AND (@Active=0 OR HoatDong=1) ORDER BY TenDichVu OPTION (RECOMPILE)";
            string keyword=string.IsNullOrWhiteSpace(tuKhoa)?null:tuKhoa.Trim()+"%";
            return Database.TruyVan(sql,new[]{new SqlParameter("@TuKhoa",SqlDbType.NVarChar,101){Value=(object)keyword??DBNull.Value},new SqlParameter("@Active",SqlDbType.Bit){Value=chiDangHoatDong}});
        }
        public int Them(DichVu x)=>Database.ThucThi("INSERT dbo.DichVu(TenDichVu,DonGia,DonViTinh) VALUES(@Ten,@Gia,@DonVi)",ThamSo(x));
        public int CapNhat(DichVu x)=>Database.ThucThi("UPDATE dbo.DichVu SET TenDichVu=@Ten,DonGia=@Gia,DonViTinh=@DonVi WHERE MaDichVu=@Ma",ThamSo(x,true));
        public int NgungSuDung(int ma)=>Database.ThucThi("UPDATE dbo.DichVu SET HoatDong=0 WHERE MaDichVu=@Ma",new[]{new SqlParameter("@Ma",SqlDbType.Int){Value=ma}});
        private static SqlParameter[] ThamSo(DichVu x,bool coMa=false){var p=new[]{new SqlParameter("@Ten",SqlDbType.NVarChar,100){Value=x.TenDichVu},new SqlParameter("@Gia",SqlDbType.Decimal){Precision=18,Scale=2,Value=x.DonGia},new SqlParameter("@DonVi",SqlDbType.NVarChar,30){Value=x.DonViTinh}};if(!coMa)return p;System.Array.Resize(ref p,p.Length+1);p[p.Length-1]=new SqlParameter("@Ma",SqlDbType.Int){Value=x.MaDichVu};return p;}
    }
}
