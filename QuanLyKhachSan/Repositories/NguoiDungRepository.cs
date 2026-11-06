using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using QuanLyKhachSan.Models;

namespace QuanLyKhachSan.Repositories
{
    public class NguoiDungRepository
    {
        public DataTable LayDanhSach()=>Database.TruyVan("SELECT MaNguoiDung,TenDangNhap,HoTen,VaiTro,HoatDong,NgayTao FROM dbo.NguoiDung ORDER BY TenDangNhap");

        public int TaoTaiKhoan(string tenDangNhap,string hoTen,string vaiTro,string hash,string salt,int soVongLap)
        {
            const string sql="INSERT dbo.NguoiDung(TenDangNhap,HoTen,VaiTro,MatKhauHash,MatKhauSalt,SoVongLap) VALUES(@TenDangNhap,@HoTen,@VaiTro,@Hash,@Salt,@VongLap)";
            return Database.ThucThi(sql,new[]{new SqlParameter("@TenDangNhap",SqlDbType.NVarChar,50){Value=tenDangNhap},new SqlParameter("@HoTen",SqlDbType.NVarChar,100){Value=hoTen},new SqlParameter("@VaiTro",SqlDbType.VarChar,20){Value=vaiTro},new SqlParameter("@Hash",SqlDbType.VarChar,128){Value=hash},new SqlParameter("@Salt",SqlDbType.VarChar,64){Value=salt},new SqlParameter("@VongLap",SqlDbType.Int){Value=soVongLap}});
        }

        public int DatLaiMatKhau(int ma,string hash,string salt,int soVongLap)=>Database.ThucThi("UPDATE dbo.NguoiDung SET MatKhauHash=@Hash,MatKhauSalt=@Salt,SoVongLap=@VongLap WHERE MaNguoiDung=@Ma AND HoatDong=1",new[]{new SqlParameter("@Hash",SqlDbType.VarChar,128){Value=hash},new SqlParameter("@Salt",SqlDbType.VarChar,64){Value=salt},new SqlParameter("@VongLap",SqlDbType.Int){Value=soVongLap},new SqlParameter("@Ma",SqlDbType.Int){Value=ma}});

        public void KhoaTaiKhoan(int ma)
        {
            Database.ThucHienTrongTransaction((connection,transaction)=>
            {
                using(SqlCommand command=new SqlCommand("SELECT VaiTro FROM dbo.NguoiDung WITH (UPDLOCK,HOLDLOCK) WHERE MaNguoiDung=@Ma AND HoatDong=1",connection,transaction))
                {
                    command.Parameters.Add("@Ma",SqlDbType.Int).Value=ma;object role=command.ExecuteScalar();
                    if(role==null)throw new InvalidOperationException("Tài khoản không tồn tại hoặc đã khóa.");
                    if(Convert.ToString(role)==VaiTroNguoiDung.QuanLy)
                    {
                        using(SqlCommand count=new SqlCommand("SELECT COUNT(*) FROM dbo.NguoiDung WITH (UPDLOCK,HOLDLOCK) WHERE VaiTro='QuanLy' AND HoatDong=1",connection,transaction))
                            if(Convert.ToInt32(count.ExecuteScalar())<=1)throw new InvalidOperationException("Phải duy trì ít nhất một tài khoản Quản lý đang hoạt động.");
                    }
                }
                using(SqlCommand update=new SqlCommand("UPDATE dbo.NguoiDung SET HoatDong=0 WHERE MaNguoiDung=@Ma",connection,transaction)){update.Parameters.Add("@Ma",SqlDbType.Int).Value=ma;update.ExecuteNonQuery();}
                return 0;
            },IsolationLevel.Serializable);
        }

        public NguoiDung TimTheoTenDangNhap(string tenDangNhap)
        {
            const string sql = @"SELECT TOP (1) MaNguoiDung,TenDangNhap,MatKhauHash,MatKhauSalt,SoVongLap,HoTen,VaiTro,HoatDong
                                 FROM dbo.NguoiDung WHERE TenDangNhap=@TenDangNhap";
            using (SqlConnection conn = Database.TaoKetNoi())
            using (SqlCommand cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.Add("@TenDangNhap", SqlDbType.NVarChar, 50).Value = tenDangNhap;
                conn.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (!reader.Read()) return null;
                    return new NguoiDung
                    {
                        MaNguoiDung = reader.GetInt32(0), TenDangNhap = reader.GetString(1),
                        MatKhauHash = reader.GetString(2), MatKhauSalt = reader.GetString(3),
                        SoVongLap = reader.GetInt32(4), HoTen = reader.GetString(5),
                        VaiTro = reader.GetString(6), HoatDong = reader.GetBoolean(7)
                    };
                }
            }
        }

        public async Task<NguoiDung> TimTheoTenDangNhapAsync(string tenDangNhap, CancellationToken cancellationToken = default)
        {
            const string sql = @"SELECT TOP (1) MaNguoiDung,TenDangNhap,MatKhauHash,MatKhauSalt,SoVongLap,HoTen,VaiTro,HoatDong
                                 FROM dbo.NguoiDung WHERE TenDangNhap=@TenDangNhap";
            using (SqlConnection conn = Database.TaoKetNoi())
            using (SqlCommand cmd = new SqlCommand(sql, conn))
            {
                cmd.CommandTimeout = 10;
                cmd.Parameters.Add("@TenDangNhap", SqlDbType.NVarChar, 50).Value = tenDangNhap;
                await conn.OpenAsync(cancellationToken).ConfigureAwait(false);
                using (SqlDataReader reader = await cmd.ExecuteReaderAsync(CommandBehavior.SingleRow, cancellationToken).ConfigureAwait(false))
                {
                    if (!await reader.ReadAsync(cancellationToken).ConfigureAwait(false)) return null;
                    return new NguoiDung
                    {
                        MaNguoiDung = reader.GetInt32(0), TenDangNhap = reader.GetString(1),
                        MatKhauHash = reader.GetString(2), MatKhauSalt = reader.GetString(3),
                        SoVongLap = reader.GetInt32(4), HoTen = reader.GetString(5),
                        VaiTro = reader.GetString(6), HoatDong = reader.GetBoolean(7)
                    };
                }
            }
        }

        public void CapNhatMatKhau(int maNguoiDung, string hash, string salt, int soVongLap)
        {
            const string sql = @"UPDATE dbo.NguoiDung SET MatKhauHash=@Hash,MatKhauSalt=@Salt,SoVongLap=@SoVongLap
                                 WHERE MaNguoiDung=@Ma AND HoatDong=1";
            using (SqlConnection conn = Database.TaoKetNoi())
            using (SqlCommand cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.Add("@Hash", SqlDbType.VarChar, 128).Value = hash;
                cmd.Parameters.Add("@Salt", SqlDbType.VarChar, 64).Value = salt;
                cmd.Parameters.Add("@SoVongLap", SqlDbType.Int).Value = soVongLap;
                cmd.Parameters.Add("@Ma", SqlDbType.Int).Value = maNguoiDung;
                conn.Open();
                if (cmd.ExecuteNonQuery() != 1) throw new InvalidOperationException("Không tìm thấy tài khoản đang hoạt động.");
            }
        }
    }
}
