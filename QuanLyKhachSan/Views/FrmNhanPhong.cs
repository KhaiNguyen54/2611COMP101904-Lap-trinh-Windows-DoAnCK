using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using QuanLyKhachSan.Presenters;
using QuanLyKhachSan.Services;
using QuanLyKhachSan.Models;

namespace QuanLyKhachSan.Views
{
    /// <summary>Nhận phòng từ đặt trước hoặc cho khách vãng lai (NV03).</summary>
    public sealed class FrmNhanPhong : Form, INhanPhongView
    {
        private readonly NhanPhongPresenter _presenter;
        private readonly RadioButton _tuDat = new RadioButton { Text = "Khách đã đặt trước", AutoSize = true, Checked = true };
        private readonly RadioButton _vangLai = new RadioButton { Text = "Khách vãng lai", AutoSize = true };
        private readonly ComboBox _datPhong = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
        private readonly ComboBox _khach = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
        private readonly ComboBox _phong = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
        private readonly DateTimePicker _di = new DateTimePicker { Format = DateTimePickerFormat.Short };
        private readonly Label _thongTin = new Label { AutoSize = true, ForeColor = GiaoDien.Phu, MaximumSize = new Size(460, 0) };
        private readonly Button _nhan;
        private bool _dangNap;

        public FrmNhanPhong(NguoiDung user, int? maDatPhong = null)
        {
            _presenter = new NhanPhongPresenter(this, new LuuTruService(), new DatPhongService(),
                new KhachHangService(), new PhongService(), user);
            Text = "Nhận phòng";
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            AutoScaleMode = AutoScaleMode.Dpi;
            AutoSize = true;
            AutoSizeMode = AutoSizeMode.GrowAndShrink;
            MinimumSize = new Size(520, 0);

            var chonCheDo = GiaoDien.ThanhNgang();
            chonCheDo.Controls.Add(_tuDat);
            chonCheDo.Controls.Add(_vangLai);

            var khachNhom = new FlowLayoutPanel { AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, WrapContents = false, Margin = new Padding(0) };
            _khach.Width = 240;
            khachNhom.Controls.Add(_khach);
            var moi = GiaoDien.TaoNut("Khách mới…", KhachMoi_Click);
            moi.MinimumSize = new Size(84, 24);
            moi.Margin = new Padding(6, 0, 0, 0);
            khachNhom.Controls.Add(moi);

            _datPhong.DropDownWidth = 420;
            var bang = GiaoDien.BangNhap(
                new KeyValuePair<string, Control>("Đặt phòng", _datPhong),
                new KeyValuePair<string, Control>("Khách hàng", khachNhom),
                new KeyValuePair<string, Control>("Phòng", _phong),
                new KeyValuePair<string, Control>("Ngày đi dự kiến", _di));
            bang.Padding = new Padding(10, 2, 10, 2);

            _thongTin.Margin = new Padding(10, 4, 10, 6);
            _nhan = GiaoDien.TaoNut("Nhận phòng", delegate { _presenter.Nhan(); }, true);
            var dong = GiaoDien.TaoNut("Đóng");
            dong.DialogResult = DialogResult.Cancel;
            var nut = GiaoDien.HangNut(FlowDirection.RightToLeft);
            nut.Controls.Add(dong);
            nut.Controls.Add(_nhan);

            var goc = GiaoDien.GocTuCo();
            nut.Dock = DockStyle.Fill;
            foreach (Control c in new Control[] { chonCheDo, bang, _thongTin, nut })
            {
                c.Dock = DockStyle.Fill;
                goc.RowStyles.Add(new RowStyle(SizeType.AutoSize));
                goc.Controls.Add(c, 0, goc.RowCount++);
            }
            Controls.Add(goc);
            AcceptButton = _nhan;
            CancelButton = dong;

            _di.MinDate = DateTime.Today.AddDays(1);
            _di.Value = DateTime.Today.AddDays(1);
            _tuDat.CheckedChanged += delegate { _presenter.CapNhatCheDo(); };
            _datPhong.SelectedIndexChanged += delegate { if (!_dangNap) _presenter.ChonDatPhong(); };
            _di.ValueChanged += delegate { if (!_dangNap && _vangLai.Checked) _presenter.TaiPhongTrong(); };
            Load += delegate { _presenter.KhoiTao(maDatPhong); };
            GiaoDien.ApDung(this);
        }

        private void KhachMoi_Click(object sender, EventArgs e)
        {
            using (var f = new FrmKhachNhanh())
            {
                if (f.ShowDialog(this) != DialogResult.OK) return;
                _presenter.TaiKhach(f.MaKhachMoi > 0 ? f.MaKhachMoi : (int?)null);
            }
        }

        // ----- INhanPhongView -----
        public bool CheDoTuDat { get { return _tuDat.Checked; } }
        public int? MaDatPhongChon { get { return _datPhong.SelectedValue == null ? (int?)null : Convert.ToInt32(_datPhong.SelectedValue); } }
        public int? KhachChon { get { return _khach.SelectedValue == null ? (int?)null : Convert.ToInt32(_khach.SelectedValue); } }
        public int? PhongChon { get { return _phong.SelectedValue == null ? (int?)null : Convert.ToInt32(_phong.SelectedValue); } }
        public DateTime NgayDiDuKien { get { return _di.Value; } }

        public void HienDatPhong(DataTable dat, int? chon)
        {
            _dangNap = true;
            _datPhong.DisplayMember = "Nhan";
            _datPhong.ValueMember = "MaDatPhong";
            _datPhong.DataSource = dat;
            _dangNap = false;
            if (chon.HasValue) _datPhong.SelectedValue = chon.Value;
        }

        public void HienKhach(DataTable khach, int? chon)
        {
            _khach.DisplayMember = "HoTen";
            _khach.ValueMember = "MaKhachHang";
            _khach.DataSource = khach;
            if (chon.HasValue) _khach.SelectedValue = chon.Value;
        }

        public void HienPhongTrong(DataTable phong)
        {
            _phong.DisplayMember = "SoPhong";
            _phong.ValueMember = "MaPhong";
            _phong.DataSource = phong;
        }

        public void ChuyenSangVangLai() { _vangLai.Checked = true; }

        public void DatCheDo(bool tuDat)
        {
            _datPhong.Enabled = tuDat;
            _khach.Enabled = !tuDat;
            _phong.Enabled = !tuDat;
        }

        public void DatNgayDi(DateTime ngay)
        {
            _dangNap = true;
            _di.MinDate = DateTime.Today.AddDays(1);
            _di.Value = ngay;
            _dangNap = false;
        }

        public void DatThongTin(string noiDung) { _thongTin.Text = noiDung; }
        public void DatChoPhepNhan(bool duocNhan) { _nhan.Enabled = duocNhan; }

        public void DongThanhCong()
        {
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
