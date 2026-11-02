using System.Data;
using Microsoft.Data.SqlClient;
using QuanLyKhachSan.DTO;

namespace QuanLyKhachSan.DAL
{
    public class ChiTietDichVuDAL
    {
        public DataTable LayTheoPhieuThue(int maPhieuThue)
        {
            const string sql=@"SELECT ct.MaChiTietDichVu,ct.MaDichVu,d.TenDichVu,ct.SoLuong,ct.DonGia,ct.ThoiDiemSuDung,ct.SoLuong*ct.DonGia AS ThanhTien
                               FROM dbo.ChiTietDichVu ct JOIN dbo.DichVu d ON ct.MaDichVu=d.MaDichVu WHERE ct.MaPhieuThue=@Ma ORDER BY ct.ThoiDiemSuDung";
            return Database.TruyVan(sql,new[]{new SqlParameter("@Ma",SqlDbType.Int){Value=maPhieuThue}});
        }

        public int Them(ChiTietDichVu x)
        {
            const string sql=@"INSERT dbo.ChiTietDichVu(MaPhieuThue,MaDichVu,SoLuong,DonGia)
                               SELECT @Phieu,@DichVu,@SoLuong,d.DonGia FROM dbo.DichVu d
                               JOIN dbo.PhieuThue pt ON pt.MaPhieuThue=@Phieu AND pt.TrangThai=N'Đang ở'
                               WHERE d.MaDichVu=@DichVu AND d.HoatDong=1";
            return Database.ThucThi(sql,new[]{new SqlParameter("@Phieu",SqlDbType.Int){Value=x.MaPhieuThue},new SqlParameter("@DichVu",SqlDbType.Int){Value=x.MaDichVu},new SqlParameter("@SoLuong",SqlDbType.Decimal){Precision=12,Scale=2,Value=x.SoLuong}});
        }

        /// <summary>Xóa một dòng dịch vụ, chỉ khi phiếu thuê còn đang ở (QT32).</summary>
        public int Xoa(int maChiTietDichVu)
        {
            const string sql = @"
                DELETE ct FROM dbo.ChiTietDichVu ct
                JOIN dbo.PhieuThue pt ON pt.MaPhieuThue = ct.MaPhieuThue AND pt.TrangThai = N'Đang ở'
                WHERE ct.MaChiTietDichVu = @Ma";
            return Database.ThucThi(sql, new[] { new SqlParameter("@Ma", SqlDbType.Int) { Value = maChiTietDichVu } });
        }
    }
}
