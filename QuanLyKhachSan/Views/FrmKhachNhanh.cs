using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using QuanLyKhachSan.Services;
using QuanLyKhachSan.Models;

namespace QuanLyKhachSan.Views
{
    /// <summary>Hộp thoại thêm nhanh khách hàng tại quầy (dùng khi đặt/nhận phòng cho khách mới).</summary>
    public sealed class FrmKhachNhanh : Form
    {
        private readonly TextBox _ten = new TextBox();
        private readonly TextBox _giayTo = new TextBox();
        private readonly TextBox _sdt = new TextBox();
        private readonly TextBox _email = new TextBox();
        private readonly TextBox _diaChi = new TextBox();

        /// <summary>Mã khách vừa tạo (0 nếu hủy).</summary>
        public int MaKhachMoi { get; private set; }

        public FrmKhachNhanh()
        {
            Text = "Thêm khách hàng";
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            AutoScaleMode = AutoScaleMode.Dpi;
            AutoSize = true;
            AutoSizeMode = AutoSizeMode.GrowAndShrink;
            MinimumSize = new Size(420, 0);

            var bang = GiaoDien.BangNhap(
                new KeyValuePair<string, Control>("Họ tên (*)", _ten),
                new KeyValuePair<string, Control>("Số giấy tờ (*)", _giayTo),
                new KeyValuePair<string, Control>("Điện thoại", _sdt),
                new KeyValuePair<string, Control>("Email", _email),
                new KeyValuePair<string, Control>("Địa chỉ", _diaChi));

            var luu = GiaoDien.TaoNut("Lưu", Luu_Click, true);
            var huy = GiaoDien.TaoNut("Hủy");
            huy.DialogResult = DialogResult.Cancel;
            var nut = GiaoDien.HangNut(FlowDirection.RightToLeft);
            nut.Controls.Add(huy);
            nut.Controls.Add(luu);

            var goc = GiaoDien.GocTuCo();
            foreach (Control c in new Control[] { bang, nut })
            {
                c.Dock = DockStyle.Fill;
                goc.RowStyles.Add(new RowStyle(SizeType.AutoSize));
                goc.Controls.Add(c, 0, goc.RowCount++);
            }
            Controls.Add(goc);
            AcceptButton = luu;
            CancelButton = huy;
            GiaoDien.ApDung(this);
        }

        private void Luu_Click(object sender, EventArgs e)
        {
            try
            {
                var khach = new KhachHang { HoTen = _ten.Text, SoGiayTo = _giayTo.Text, SoDienThoai = _sdt.Text, Email = _email.Text, DiaChi = _diaChi.Text };
                new KhachHangService().Them(khach);
                var kq = new KhachHangService().TimKiem(_giayTo.Text.Trim());
                if (kq.Rows.Count > 0) MaKhachMoi = Convert.ToInt32(kq.Rows[0]["MaKhachHang"]);
                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                UiHelper.ThongBaoLoi(this, "Không thể lưu khách hàng", ex);
            }
        }
    }
}
