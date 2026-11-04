using System;
using System.Data;
using Microsoft.Data.SqlClient;
using QuanLyKhachSan.DTO;

namespace QuanLyKhachSan.DAL
{
    public class PhieuThueDAL
    {
        public int NhanPhong(int maPhong,int maKhach,int maNguoiDung,DateTime ngayDiDuKien,int? maDatPhong=null)
        {
            var output=new SqlParameter("@MaPhieuThue",SqlDbType.Int){Direction=ParameterDirection.Output};
            Database.ThucThi("dbo.sp_NhanPhong",new[]{new SqlParameter("@MaPhong",SqlDbType.Int){Value=maPhong},new SqlParameter("@MaKhachHang",SqlDbType.Int){Value=maKhach},new SqlParameter("@MaNguoiDung",SqlDbType.Int){Value=maNguoiDung},new SqlParameter("@NgayDiDuKien",SqlDbType.Date){Value=ngayDiDuKien.Date},new SqlParameter("@MaDatPhong",SqlDbType.Int){Value=(object)maDatPhong??DBNull.Value},output},null,CommandType.StoredProcedure);
            return Convert.ToInt32(output.Value);
        }

        public DataTable LayPhieuDangO()
        {
            const string sql=@"SELECT pt.MaPhieuThue,pt.MaPhong,p.SoPhong,pt.MaKhachHang,k.HoTen,pt.NgayNhan,pt.NgayDiDuKien,pt.GiaPhongMoiDem
                               FROM dbo.PhieuThue pt JOIN dbo.Phong p ON pt.MaPhong=p.MaPhong JOIN dbo.KhachHang k ON pt.MaKhachHang=k.MaKhachHang WHERE pt.TrangThai=N'Đang ở' ORDER BY p.SoPhong";
            return Database.TruyVan(sql);
        }

        /// <summary>Danh sách khách đang lưu trú (phục vụ tra cứu và xuất CSV).</summary>
        public DataTable LayKhachDangO()
        {
            const string sql = @"
                SELECT pt.MaPhieuThue, p.SoPhong, p.Tang, l.TenLoaiPhong, k.HoTen, k.SoGiayTo, k.SoDienThoai,
                       pt.NgayNhan, pt.NgayDiDuKien,
                       DATEDIFF(day, CONVERT(date, pt.NgayNhan), CONVERT(date, SYSDATETIME())) AS SoDemDaO
                FROM dbo.PhieuThue pt
                JOIN dbo.Phong p ON pt.MaPhong = p.MaPhong
                JOIN dbo.LoaiPhong l ON p.MaLoaiPhong = l.MaLoaiPhong
                JOIN dbo.KhachHang k ON pt.MaKhachHang = k.MaKhachHang
                WHERE pt.TrangThai = N'Đang ở'
                ORDER BY p.Tang, p.SoPhong";
            return Database.TruyVan(sql);
        }

        /// <summary>Đổi phiếu thuê đang ở sang phòng khác (sp_DoiPhong, một transaction, ghi lịch sử).</summary>
        public void DoiPhong(int maPhieuThue, int maPhongMoi, int maNguoiDung, string lyDo)
        {
            Database.ThucThi("dbo.sp_DoiPhong", new[]
            {
                new SqlParameter("@MaPhieuThue", SqlDbType.Int) { Value = maPhieuThue },
                new SqlParameter("@MaPhongMoi", SqlDbType.Int) { Value = maPhongMoi },
                new SqlParameter("@MaNguoiDung", SqlDbType.Int) { Value = maNguoiDung },
                new SqlParameter("@LyDo", SqlDbType.NVarChar, 200) { Value = (object)lyDo ?? DBNull.Value }
            }, null, CommandType.StoredProcedure);
        }

        public DataTable LayLichSuDoiPhong(int maPhieuThue)
        {
            const string sql = @"
                SELECT ls.ThoiDiemDoi, pc.SoPhong AS PhongCu, pm.SoPhong AS PhongMoi, ls.GiaCu, ls.GiaMoi, ls.LyDo, nd.HoTen AS NguoiThucHien
                FROM dbo.LichSuDoiPhong ls
                JOIN dbo.Phong pc ON ls.MaPhongCu = pc.MaPhong
                JOIN dbo.Phong pm ON ls.MaPhongMoi = pm.MaPhong
                JOIN dbo.NguoiDung nd ON ls.MaNguoiDung = nd.MaNguoiDung
                WHERE ls.MaPhieuThue = @Ma
                ORDER BY ls.ThoiDiemDoi DESC, ls.MaDoiPhong DESC";
            return Database.TruyVan(sql, new[] { new SqlParameter("@Ma", SqlDbType.Int) { Value = maPhieuThue } });
        }
    }
}
