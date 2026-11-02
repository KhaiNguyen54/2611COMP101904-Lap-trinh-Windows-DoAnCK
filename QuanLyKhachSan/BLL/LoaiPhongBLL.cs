using System;
using System.Data;
using QuanLyKhachSan.DAL;
using QuanLyKhachSan.DTO;

namespace QuanLyKhachSan.BLL
{
    public class LoaiPhongBLL
    {
        private readonly LoaiPhongDAL _dal=new LoaiPhongDAL();
        public DataTable TimKiem(string tuKhoa=null)=>_dal.TimKiem(tuKhoa);
        public int Them(LoaiPhong x){KiemTra(x);KiemTraTrung(x);return _dal.Them(x);}
        public int CapNhat(LoaiPhong x){KiemTra(x);KiemTraTrung(x);return _dal.CapNhat(x);}
        public int NgungSuDung(int ma)=>_dal.NgungSuDung(ma);
        private static void KiemTra(LoaiPhong x){if(x==null||string.IsNullOrWhiteSpace(x.TenLoaiPhong))throw new ArgumentException("Tên loại phòng bắt buộc.");if(x.GiaMoiDem<=0||x.SucChua<=0)throw new ArgumentException("Giá và sức chứa phải lớn hơn 0.");}
        private void KiemTraTrung(LoaiPhong x){DataTable dt=_dal.TimKiem();foreach(DataRow row in dt.Rows)if(Convert.ToInt32(row["MaLoaiPhong"])!=x.MaLoaiPhong&&string.Equals(Convert.ToString(row["TenLoaiPhong"]).Trim(),x.TenLoaiPhong.Trim(),StringComparison.OrdinalIgnoreCase))throw new ArgumentException("Tên loại phòng đã tồn tại.");}
    }
}
