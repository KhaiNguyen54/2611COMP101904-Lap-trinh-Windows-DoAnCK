using System;
using System.Windows.Forms;
using QuanLyKhachSan.GUI;

namespace QuanLyKhachSan
{
    internal static class Program
    {
        /// <summary>
        /// Điểm vào ứng dụng: đăng nhập → form chính; "Đăng xuất" quay lại màn hình đăng nhập.
        /// </summary>
        [STAThread]
        private static void Main()
        {
            ApplicationConfiguration.Initialize();

            while (true)
            {
                FrmMain chinh;
                using (var frmDangNhap = new FrmDangNhap())
                {
                    if (frmDangNhap.ShowDialog() != DialogResult.OK)
                        return;
                    chinh = new FrmMain(frmDangNhap.NguoiDungDangNhap);
                }

                using (chinh)
                {
                    Application.Run(chinh);
                    if (!chinh.DangXuat)
                        return;
                }
            }
        }
    }
}
