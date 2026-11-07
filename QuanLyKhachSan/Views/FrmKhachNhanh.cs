using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using QuanLyKhachSan.Presenters;
using QuanLyKhachSan.Services;
using QuanLyKhachSan.Models;

namespace QuanLyKhachSan.Views
{
    /// <summary>Hộp thoại thêm nhanh khách hàng tại quầy (dùng khi đặt/nhận phòng cho khách mới).</summary>
    public sealed class FrmKhachNhanh : Form, IKhachNhanhView
    {
        private readonly KhachNhanhPresenter _presenter;
        private readonly TextBox _ten = new TextBox();
        private readonly TextBox _giayTo = new TextBox();
        private readonly TextBox _sdt = new TextBox();
        private readonly TextBox _email = new TextBox();
        private readonly TextBox _diaChi = new TextBox();

        /// <summary>Mã khách vừa tạo (0 nếu hủy).</summary>
        public int MaKhachMoi { get; private set; }

        public FrmKhachNhanh()
        {
            _presenter = new KhachNhanhPresenter(this, new KhachHangService());
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

            var luu = GiaoDien.TaoNut("Lưu", delegate { _presenter.Luu(); }, true);
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

        // ----- IKhachNhanhView -----
        public string HoTen { get { return _ten.Text; } }
        public string SoGiayTo { get { return _giayTo.Text; } }
        public string SoDienThoai { get { return _sdt.Text; } }
        public string Email { get { return _email.Text; } }
        public string DiaChi { get { return _diaChi.Text; } }

        public void DongThanhCong(int maKhach)
        {
            MaKhachMoi = maKhach;
            DialogResult = DialogResult.OK;
            Close();
        }

        public void HienThongBao(string tieuDe, string noiDung, MucThongBao muc)
        {
            UiHelper.HienThongBao(this, tieuDe, noiDung, muc);
        }

        public bool XacNhan(string noiDung) { return UiHelper.XacNhan(this, noiDung); }
    }
}
