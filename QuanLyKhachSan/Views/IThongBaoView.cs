namespace QuanLyKhachSan.Views
{
    public enum MucThongBao { ThongTin, ThanhCong, CanhBao, Loi }

    /// <summary>
    /// Phần chung của mọi View trong mô hình MVP: Presenter chỉ biết hiển thị thông báo
    /// và hỏi xác nhận qua interface này, không biết MessageBox hay control cụ thể.
    /// </summary>
    public interface IThongBaoView
    {
        void HienThongBao(string tieuDe, string noiDung, MucThongBao muc);
        bool XacNhan(string noiDung);
    }
}
