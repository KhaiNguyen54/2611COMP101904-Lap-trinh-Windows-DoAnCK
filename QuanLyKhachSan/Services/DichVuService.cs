using System;
using System.Data;
using QuanLyKhachSan.Repositories;
using QuanLyKhachSan.Models;

namespace QuanLyKhachSan.Services
{
    public class DichVuService
    {
        private readonly DichVuRepository _dal=new DichVuRepository();
        public DataTable TimKiem(string tuKhoa=null,bool dangHoatDong=false)=>_dal.TimKiem(tuKhoa,dangHoatDong);
        public int Them(DichVu x){KiemTra(x);KiemTraTrung(x);return _dal.Them(x);}
        public int CapNhat(DichVu x){KiemTra(x);KiemTraTrung(x);return _dal.CapNhat(x);}
        public int NgungSuDung(int ma)=>_dal.NgungSuDung(ma);
        private static void KiemTra(DichVu x){if(x==null||string.IsNullOrWhiteSpace(x.TenDichVu)||string.IsNullOrWhiteSpace(x.DonViTinh))throw new ArgumentException("Tên dịch vụ và đơn vị tính bắt buộc.");if(x.DonGia<0)throw new ArgumentException("Đơn giá không được âm.");}
        private void KiemTraTrung(DichVu x){DataTable dt=_dal.TimKiem();foreach(DataRow row in dt.Rows)if(Convert.ToInt32(row["MaDichVu"])!=x.MaDichVu&&string.Equals(Convert.ToString(row["TenDichVu"]).Trim(),x.TenDichVu.Trim(),StringComparison.OrdinalIgnoreCase))throw new ArgumentException("Tên dịch vụ đã tồn tại.");}
    }
}
