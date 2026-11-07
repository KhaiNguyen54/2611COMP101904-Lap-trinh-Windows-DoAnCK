using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using QuanLyKhachSan.Presenters;
using QuanLyKhachSan.Services;
using QuanLyKhachSan.Models;

namespace QuanLyKhachSan.Views
{
    /// <summary>Lập, tra cứu, hủy đặt phòng và đánh dấu khách không đến (NV01, NV02).</summary>
    public sealed class FrmDatPhong : Form, IDatPhongView
    {
        private readonly NguoiDung _user;
        private readonly DatPhongPresenter _presenter;
        private readonly ComboBox _khach = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
        private readonly ComboBox _phong = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
        private readonly DateTimePicker _den = new DateTimePicker { Format = DateTimePickerFormat.Short };
        private readonly DateTimePicker _di = new DateTimePicker { Format = DateTimePickerFormat.Short };
        private readonly NumericUpDown _coc = new NumericUpDown { Maximum = 1000000000, Increment = 50000, ThousandsSeparator = true };
        private readonly ComboBox _loc = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
        private readonly DataGridView _grid = UiHelper.TaoLuoi();
        private readonly Button _taoDatPhong;
        private readonly Label _goiY = new Label { AutoSize = true, ForeColor = GiaoDien.Phu, Margin = new Padding(0, 6, 0, 0) };
        private bool _dangDieuChinhNgay;

        public FrmDatPhong(NguoiDung user)
        {
            _user = user;
            _presenter = new DatPhongPresenter(this, new DatPhongService(), new KhachHangService(), new PhongService(), user);
            Text = "Đặt phòng";
            ClientSize = new Size(1040, 600);
            MinimumSize = new Size(760, 460);
            StartPosition = FormStartPosition.CenterParent;
            AutoScaleMode = AutoScaleMode.Dpi;

            _taoDatPhong = GiaoDien.TaoNut("Tạo đặt phòng", delegate { _presenter.TaoDatPhong(); }, true);
            _khach.DropDownWidth = 360;

            var khachGroup = new FlowLayoutPanel { AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, WrapContents = false, Margin = new Padding(0) };
            _khach.Width = 200;
            khachGroup.Controls.Add(_khach);
            var khachMoi = GiaoDien.TaoNut("Khách mới…", KhachMoi_Click);
            khachMoi.MinimumSize = new Size(84, 24);
            khachMoi.Margin = new Padding(6, 0, 0, 0);
            khachGroup.Controls.Add(khachMoi);

            var nhap = GiaoDien.ThanhNgang();
            nhap.Controls.Add(GiaoDien.Nhom("Khách hàng", khachGroup));
            nhap.Controls.Add(GiaoDien.Nhom("Ngày đến", _den, 120));
            nhap.Controls.Add(GiaoDien.Nhom("Ngày đi", _di, 120));
            nhap.Controls.Add(GiaoDien.Nhom("Phòng trống", _phong, 160));
            nhap.Controls.Add(GiaoDien.Nhom("Tiền cọc (đ)", _coc, 120));
            var hanhDong = new FlowLayoutPanel { AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, WrapContents = false, Margin = new Padding(0, 16, 0, 6) };
            hanhDong.Controls.Add(_taoDatPhong);
            hanhDong.Controls.Add(_goiY);
            nhap.Controls.Add(hanhDong);

            _loc.Items.AddRange(new object[] { "Tất cả", "Đã đặt", "Đã nhận", "Đã hủy", "Không đến" });
            _loc.SelectedIndex = 1;
            _loc.SelectedIndexChanged += delegate { _presenter.TaiDanhSach(); };
            var tool = GiaoDien.ThanhNgang();
            tool.Controls.Add(GiaoDien.Nhom("Lọc trạng thái", _loc, 120));
            var nutHang = new FlowLayoutPanel { AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, WrapContents = false, Margin = new Padding(0, 16, 0, 6) };
            nutHang.Controls.Add(GiaoDien.TaoNut("Nhận phòng từ đặt này", delegate { _presenter.NhanPhongTuDatNay(); }));
            nutHang.Controls.Add(GiaoDien.TaoNut("Hủy đặt phòng", delegate { _presenter.Huy(); }));
            nutHang.Controls.Add(GiaoDien.TaoNut("Khách không đến", delegate { _presenter.KhachKhongDen(); }));
            nutHang.Controls.Add(GiaoDien.TaoNut("Làm mới", delegate { _presenter.LamMoi(); }));
            tool.Controls.Add(nutHang);

            Controls.Add(_grid);
            Controls.Add(tool);
            Controls.Add(nhap);

            _den.MinDate = DateTime.Today;
            _den.Value = DateTime.Today;
            _di.MinDate = DateTime.Today.AddDays(1);
            _di.Value = DateTime.Today.AddDays(1);
            _den.ValueChanged += NgayDen_ValueChanged;
            _di.ValueChanged += delegate { if (!_dangDieuChinhNgay) _presenter.TaiPhongTrong(); };
            _phong.SelectedIndexChanged += delegate { _presenter.CapNhatGoiY(); };
            _phong.DisplayMember = "SoPhong";
            _phong.ValueMember = "MaPhong";
            Load += delegate { _presenter.TaiBanDau(); };
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

        private void NgayDen_ValueChanged(object sender, EventArgs e)
        {
            _dangDieuChinhNgay = true;
            try
            {
                DateTime toiThieu = _den.Value.Date.AddDays(1);
                _di.MinDate = toiThieu;
                if (_di.Value.Date < toiThieu) _di.Value = toiThieu;
            }
            finally { _dangDieuChinhNgay = false; }
            _presenter.TaiPhongTrong();
        }

        // ----- IDatPhongView -----
        public int? KhachChon { get { return _khach.SelectedValue == null ? (int?)null : Convert.ToInt32(_khach.SelectedValue); } }
        public int? PhongChon { get { return _phong.SelectedValue == null ? (int?)null : Convert.ToInt32(_phong.SelectedValue); } }

        public decimal? GiaPhongChon
        {
            get
            {
                DataRowView r = _phong.SelectedItem as DataRowView;
                if (r == null || !r.DataView.Table.Columns.Contains("GiaMoiDem")) return null;
                return Convert.ToDecimal(r["GiaMoiDem"]);
            }
        }

        public DateTime NgayDen { get { return _den.Value; } }
        public DateTime NgayDi { get { return _di.Value; } }
        public decimal TienCoc { get { return _coc.Value; } }
        public string LocTrangThai { get { return Convert.ToString(_loc.SelectedItem); } }

        public int? MaDatPhongChon
        {
            get { return _grid.CurrentRow == null ? (int?)null : Convert.ToInt32(_grid.CurrentRow.Cells["MaDatPhong"].Value); }
        }

        public string TrangThaiDatChon
        {
            get { return _grid.CurrentRow == null ? null : Convert.ToString(_grid.CurrentRow.Cells["TrangThai"].Value); }
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
            object dangChon = _phong.SelectedValue;
            _phong.DataSource = phong;
            if (dangChon != null)
                foreach (DataRow r in phong.Rows)
                    if (Convert.ToInt32(r["MaPhong"]) == Convert.ToInt32(dangChon)) { _phong.SelectedValue = dangChon; break; }
            _phong.Enabled = phong.Rows.Count > 0;
        }

        public void HienDanhSachDat(DataTable dat)
        {
            UiHelper.GanNguon(_grid, dat, "-MaDatPhong", "-MaKhachHang", "-MaPhong", "HoTen|Khách hàng", "SoPhong|Phòng", "Tang|Tầng",
                "NgayDen|Ngày đến|dd/MM/yyyy", "NgayDi|Ngày đi|dd/MM/yyyy", "TienCoc|Tiền cọc|N0", "TrangThai|Trạng thái");
        }

        public void DatChoPhepDat(bool duocDat) { _taoDatPhong.Enabled = duocDat; }
        public void DatGoiY(string noiDung) { _goiY.Text = noiDung; }
        public void XoaTienCoc() { _coc.Value = 0; }

        public void MoNhanPhong(int maDatPhong)
        {
            using (var f = new FrmNhanPhong(_user, maDatPhong))
                f.ShowDialog(this);
        }

        public void HienThongBao(string tieuDe, string noiDung, MucThongBao muc)
        {
            UiHelper.HienThongBao(this, tieuDe, noiDung, muc);
        }

        public bool XacNhan(string noiDung) { return UiHelper.XacNhan(this, noiDung); }
    }
}
