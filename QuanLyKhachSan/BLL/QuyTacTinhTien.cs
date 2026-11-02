using System;

namespace QuanLyKhachSan.BLL
{
    /// <summary>
    /// Các quy tắc tính tiền thuần (không truy cập CSDL), tương ứng QT40–QT46 trong docs/03_QuyTac_NghiepVu.md.
    /// Tách riêng để kiểm thử được mà không cần SQL Server.
    /// </summary>
    public static class QuyTacTinhTien
    {
        /// <summary>Giờ trả phòng chuẩn (QT43).</summary>
        public static readonly TimeSpan GioTraChuan = TimeSpan.FromHours(12);

        /// <summary>Tỷ lệ giảm giá tối đa lễ tân tự duyệt (QT44).</summary>
        public const decimal TyLeGiamToiDaLeTan = 0.10m;

        /// <summary>Số đêm = số ngày giữa ngày nhận và ngày trả (chỉ tính phần ngày), tối thiểu 1 (QT40, QT46).</summary>
        public static int TinhSoDem(DateTime ngayNhan, DateTime ngayTra)
        {
            return Math.Max(1, (ngayTra.Date - ngayNhan.Date).Days);
        }

        /// <summary>
        /// Phụ thu trả muộn so với 12:00 của <b>ngày trả thực tế</b> (QT43).
        /// Các đêm lưu quá ngày đi dự kiến đã nằm trong số đêm nên không bị tính phụ thu thêm lần nữa.
        /// </summary>
        public static decimal TinhPhuThuTraMuon(DateTime ngayTra, decimal giaMoiDem)
        {
            double gioTre = (ngayTra - ngayTra.Date.Add(GioTraChuan)).TotalHours;
            if (gioTre > 6) return giaMoiDem;
            if (gioTre > 3) return giaMoiDem * 0.50m;
            if (gioTre > 1) return giaMoiDem * 0.30m;
            return 0m;
        }

        /// <summary>Tổng thanh toán = phòng + dịch vụ + phụ thu − giảm giá − cọc, không âm (QT45).</summary>
        public static decimal TinhTongThanhToan(decimal tienPhong, decimal tienDichVu, decimal phuThu,
            decimal giamGia, decimal tienCoc)
        {
            return Math.Max(0m, tienPhong + tienDichVu + phuThu - giamGia - tienCoc);
        }

        /// <summary>Cơ sở tính giảm giá: tiền phòng + dịch vụ + phụ thu.</summary>
        public static decimal CoSoGiamGia(decimal tienPhong, decimal tienDichVu, decimal phuThu)
        {
            return Math.Max(0m, tienPhong + tienDichVu + phuThu);
        }

        /// <summary>Tiền cọc tối đa = giá 1 đêm × số đêm đặt (QT15).</summary>
        public static decimal TienCocToiDa(decimal giaMoiDem, DateTime ngayDen, DateTime ngayDi)
        {
            return giaMoiDem * Math.Max(1, (ngayDi.Date - ngayDen.Date).Days);
        }
    }
}
