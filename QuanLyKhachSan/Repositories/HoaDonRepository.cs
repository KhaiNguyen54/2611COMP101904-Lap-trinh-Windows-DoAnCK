using System;
using System.Data;
using Microsoft.Data.SqlClient;

namespace QuanLyKhachSan.Repositories
{
    public class HoaDonRepository
    {
        public DataTable TinhTamTinh(int maPhieuThue,DateTime ngayTra)
        {
            const string sql=@"SELECT pt.NgayNhan,pt.NgayDiDuKien,pt.GiaPhongMoiDem,
                               COALESCE((SELECT SUM(ct.SoLuong*ct.DonGia) FROM dbo.ChiTietDichVu ct WHERE ct.MaPhieuThue=pt.MaPhieuThue),0) AS TienDichVu,
                               COALESCE(dp.TienCoc,0) AS TienCoc
                               FROM dbo.PhieuThue pt LEFT JOIN dbo.DatPhong dp ON pt.MaDatPhong=dp.MaDatPhong
                               WHERE pt.MaPhieuThue=@Ma AND pt.TrangThai=N'Đang ở'";
            return Database.TruyVan(sql,new[]{new SqlParameter("@Ma",SqlDbType.Int){Value=maPhieuThue}});
        }

        public DataTable TimKiem(DateTime? tuNgay=null,DateTime? denNgay=null,string tuKhoa=null)
        {
            const string sql=@"SELECT h.MaHoaDon,h.MaPhieuThue,h.NgayLap,k.HoTen,p.SoPhong,h.TienPhong,h.TienDichVu,h.PhuThu,h.GiamGia,h.TienCoc,h.TongThanhToan,h.HinhThucThanhToan,duyet.HoTen AS NguoiDuyetGiam
                               FROM dbo.HoaDon h LEFT JOIN dbo.NguoiDung duyet ON h.MaNguoiDuyetGiam=duyet.MaNguoiDung JOIN dbo.PhieuThue pt ON h.MaPhieuThue=pt.MaPhieuThue JOIN dbo.KhachHang k ON pt.MaKhachHang=k.MaKhachHang JOIN dbo.Phong p ON pt.MaPhong=p.MaPhong
                               WHERE (@Tu IS NULL OR h.NgayLap>=@Tu) AND (@Den IS NULL OR h.NgayLap<DATEADD(day,1,@Den))
                               AND (@TuKhoa IS NULL OR (@MaHoaDon IS NOT NULL AND h.MaHoaDon=@MaHoaDon) OR k.HoTen LIKE @TuKhoa OR p.SoPhong LIKE @TuKhoa)
                               ORDER BY h.NgayLap DESC OPTION (RECOMPILE)";
            string keyword = string.IsNullOrWhiteSpace(tuKhoa) ? null : tuKhoa.Trim();
            int maHoaDon;
            object maHoaDonValue = int.TryParse(keyword, out maHoaDon) ? (object)maHoaDon : DBNull.Value;
            object keywordValue = keyword == null ? DBNull.Value : (object)(keyword + "%");
            return Database.TruyVan(sql,new[]{new SqlParameter("@Tu",SqlDbType.Date){Value=(object)tuNgay??DBNull.Value},new SqlParameter("@Den",SqlDbType.Date){Value=(object)denNgay??DBNull.Value},new SqlParameter("@TuKhoa",SqlDbType.NVarChar,101){Value=keywordValue},new SqlParameter("@MaHoaDon",SqlDbType.Int){Value=maHoaDonValue}});
        }

        public int TraPhong(int maPhieuThue,int maNguoiDung,DateTime ngayTra,decimal phuThu,decimal giamGia,string hinhThuc,int? maNguoiDuyet=null)
        {
            const string sql="dbo.sp_TraPhong";
            var p=new[]{new SqlParameter("@MaPhieuThue",SqlDbType.Int){Value=maPhieuThue},new SqlParameter("@MaNguoiDung",SqlDbType.Int){Value=maNguoiDung},new SqlParameter("@NgayTra",SqlDbType.DateTime2){Value=ngayTra},new SqlParameter("@PhuThu",SqlDbType.Decimal){Precision=18,Scale=2,Value=phuThu},new SqlParameter("@GiamGia",SqlDbType.Decimal){Precision=18,Scale=2,Value=giamGia},new SqlParameter("@HinhThucThanhToan",SqlDbType.NVarChar,20){Value=hinhThuc},new SqlParameter("@MaNguoiDuyetGiam",SqlDbType.Int){Value=(object)maNguoiDuyet??DBNull.Value}};
            DataTable result=Database.TruyVan(sql,p,null,CommandType.StoredProcedure);
            if(result.Rows.Count==0) throw new InvalidOperationException("Stored procedure trả phòng không trả về hóa đơn.");
            return Convert.ToInt32(result.Rows[0]["MaHoaDon"]);
        }
    }
}
