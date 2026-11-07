using System;
using System.Data;
using System.Net.Mail;
using QuanLyKhachSan.Repositories;
using QuanLyKhachSan.Models;

namespace QuanLyKhachSan.Services
{
    public class KhachHangService : IKhachHangService
    {
        private readonly KhachHangRepository _dal=new KhachHangRepository();
        public DataTable TimKiem(string tuKhoa=null)=>_dal.TimKiem(tuKhoa);
        public int Them(KhachHang x){KiemTra(x);KiemTraTrung(x);return _dal.Them(x);}
        public int CapNhat(KhachHang x){KiemTra(x);KiemTraTrung(x);return _dal.CapNhat(x);}
        private static void KiemTra(KhachHang x){if(x==null||string.IsNullOrWhiteSpace(x.HoTen))throw new ArgumentException("Họ tên bắt buộc.");if(x.SoGiayTo==null||!(x.SoGiayTo.Length==9||x.SoGiayTo.Length==12)||!ChiLaSo(x.SoGiayTo))throw new ArgumentException("Số giấy tờ phải gồm 9 hoặc 12 chữ số.");if(x.SoDienThoai==null||x.SoDienThoai.Length!=10||x.SoDienThoai[0]!='0'||!ChiLaSo(x.SoDienThoai))throw new ArgumentException("Số điện thoại phải gồm 10 chữ số và bắt đầu bằng 0.");if(!string.IsNullOrWhiteSpace(x.Email)){try{new MailAddress(x.Email);}catch(FormatException){throw new ArgumentException("Email không hợp lệ.");}}}
        private void KiemTraTrung(KhachHang x){DataTable dt=_dal.TimKiem();foreach(DataRow row in dt.Rows)if(Convert.ToInt32(row["MaKhachHang"])!=x.MaKhachHang){if(string.Equals(Convert.ToString(row["SoGiayTo"]),x.SoGiayTo,StringComparison.OrdinalIgnoreCase))throw new ArgumentException("Số giấy tờ đã tồn tại.");if(string.Equals(Convert.ToString(row["SoDienThoai"]),x.SoDienThoai,StringComparison.OrdinalIgnoreCase))throw new ArgumentException("Số điện thoại đã tồn tại.");}}
        private static bool ChiLaSo(string value){foreach(char c in value)if(c<'0'||c>'9')return false;return true;}
    }
}
