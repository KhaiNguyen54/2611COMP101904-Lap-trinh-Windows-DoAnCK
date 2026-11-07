using System;
using System.Collections.Generic;
using System.Data;
using QuanLyKhachSan.Models;
using QuanLyKhachSan.Services;
using QuanLyKhachSan.Views;

namespace QuanLyKhachSan.Presenters
{
    /// <summary>Định dạng nhãn hiển thị dùng chung cho các Presenter (không phụ thuộc WinForms).</summary>
    public static class TrinhBay
    {
        public static void ThemNhan(DataTable t, Func<DataRow, string> dinhDang)
        {
            if (!t.Columns.Contains("Nhan")) t.Columns.Add("Nhan", typeof(string));
            foreach (DataRow r in t.Rows) r["Nhan"] = dinhDang(r);
        }

        public static string NhanPhieuDangO(DataRow r)
        {
            return string.Format("Phòng {0} · {1} · từ {2:dd/MM}", r["SoPhong"], r["HoTen"], r["NgayNhan"]);
        }

        public static string NhanPhieuDoiPhong(DataRow r)
        {
            return string.Format("Phòng {0} · {1} · đến {2:dd/MM}", r["SoPhong"], r["HoTen"], r["NgayDiDuKien"]);
        }

        public static string NhanDatPhong(DataRow r)
        {
            return string.Format("Phòng {0} · {1} · {2:dd/MM}–{3:dd/MM}", r["SoPhong"], r["HoTen"], r["NgayDen"], r["NgayDi"]);
        }

        public static string NhanDichVu(DataRow r)
        {
            return string.Format("{0} — {1:N0} đ/{2}", r["TenDichVu"], r["DonGia"], r["DonViTinh"]);
        }

        public static string NhanPhongMoi(DataRow r)
        {
            return string.Format("{0} · tầng {1} · {2} · {3:N0} đ", r["SoPhong"], r["Tang"], r["TenLoaiPhong"], r["GiaMoiDem"]);
        }

        public static string Tien(decimal v)
        {
            return v.ToString("N0") + " đ";
        }
    }
}
