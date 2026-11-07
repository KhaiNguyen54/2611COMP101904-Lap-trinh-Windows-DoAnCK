using System;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using QuanLyKhachSan.Presenters;
using QuanLyKhachSan.Services;
using QuanLyKhachSan.Models;

namespace QuanLyKhachSan.Views
{

    public class FrmDangNhap : Form, IDangNhapView
    {
        private readonly TextBox txtTenDangNhap = new TextBox();
        private readonly TextBox txtMatKhau = new TextBox();
        private readonly Button btnDangNhap = new Button();
        private readonly Button btnThoat = new Button();
        private readonly DangNhapPresenter _presenter;
        private readonly CancellationTokenSource _dangNhapCancellation = new CancellationTokenSource();

        public NguoiDung NguoiDungDangNhap { get; private set; }

        public FrmDangNhap()
        {
            KhoiTaoGiaoDien();
            _presenter = new DangNhapPresenter(this, new NguoiDungService());
            GiaoDien.ApDung(this);
            FormClosed += (sender, e) => _dangNhapCancellation.Cancel();
        }

        private void KhoiTaoGiaoDien()
        {
            Text = "Đăng nhập - Quản lý khách sạn";
            ClientSize = new Size(360, 210);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            StartPosition = FormStartPosition.CenterScreen;
            AutoScaleMode = AutoScaleMode.Dpi;
            MaximizeBox = false;
            MinimizeBox = false;
            Padding = new Padding(12);

            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(8),
                ColumnCount = 2,
                RowCount = 4,
                Margin = new Padding(0)
            };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120F));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 34F));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 34F));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 44F));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 32F));

            var lblTenDangNhap = new Label { Text = "Tên đăng nhập", AutoSize = true, Anchor = AnchorStyles.Left, TextAlign = ContentAlignment.MiddleLeft, Margin = new Padding(0, 6, 0, 0) };
            txtTenDangNhap.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            txtTenDangNhap.Margin = new Padding(0, 4, 0, 4);
            txtTenDangNhap.Size = new Size(0, 24);

            var lblMatKhau = new Label { Text = "Mật khẩu", AutoSize = true, Anchor = AnchorStyles.Left, TextAlign = ContentAlignment.MiddleLeft, Margin = new Padding(0, 6, 0, 0) };
            txtMatKhau.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            txtMatKhau.Margin = new Padding(0, 4, 0, 4);
            txtMatKhau.Size = new Size(0, 24);
            txtMatKhau.UseSystemPasswordChar = true;

            btnDangNhap.Text = "Đăng nhập";
            btnDangNhap.Tag = GiaoDien.NutChinhTag;
            btnDangNhap.AutoSize = true;
            btnDangNhap.MinimumSize = new Size(100, 30);
            btnDangNhap.Click += BtnDangNhap_Click;

            btnThoat.Text = "Thoát";
            btnThoat.AutoSize = true;
            btnThoat.MinimumSize = new Size(90, 30);
            btnThoat.DialogResult = DialogResult.Cancel;

            var buttonWrap = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.RightToLeft,
                WrapContents = false,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                Padding = new Padding(0),
                Margin = new Padding(0)
            };
            buttonWrap.Controls.Add(btnThoat);
            buttonWrap.Controls.Add(btnDangNhap);

            layout.Controls.Add(lblTenDangNhap, 0, 0);
            layout.Controls.Add(txtTenDangNhap, 1, 0);
            layout.Controls.Add(lblMatKhau, 0, 1);
            layout.Controls.Add(txtMatKhau, 1, 1);
            layout.Controls.Add(buttonWrap, 0, 2);
            layout.SetColumnSpan(buttonWrap, 2);

            AcceptButton = btnDangNhap;
            CancelButton = btnThoat;

            Controls.Add(layout);
        }

        private async void BtnDangNhap_Click(object sender, EventArgs e)
        {
            await _presenter.DangNhapAsync(_dangNhapCancellation.Token);
        }

        string IDangNhapView.TenDangNhap { get { return txtTenDangNhap.Text; } }
        string IDangNhapView.MatKhau { get { return txtMatKhau.Text; } }

        public void DatDangXuLy(bool dangXuLy)
        {
            if (IsDisposed) return;
            btnDangNhap.Enabled = !dangXuLy;
            btnDangNhap.Text = dangXuLy ? "Đang đăng nhập..." : "Đăng nhập";
        }

        public void XoaMatKhau()
        {
            txtMatKhau.Clear();
            txtMatKhau.Focus();
        }

        public void DongThanhCong(NguoiDung nguoiDung)
        {
            NguoiDungDangNhap = nguoiDung;
            DialogResult = DialogResult.OK;
            Close();
        }

        public void HienThongBao(string tieuDe, string noiDung, MucThongBao muc)
        {
            if (!IsDisposed) UiHelper.HienThongBao(this, tieuDe, noiDung, muc);
        }

        public bool XacNhan(string noiDung)
        {
            return UiHelper.XacNhan(this, noiDung);
        }
    }
}
