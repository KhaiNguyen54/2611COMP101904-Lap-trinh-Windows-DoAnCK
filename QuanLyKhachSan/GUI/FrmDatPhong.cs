using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using QuanLyKhachSan.BLL;
using QuanLyKhachSan.DTO;

namespace QuanLyKhachSan.GUI
{
    /// <summary>Lập, tra cứu, hủy đặt phòng và đánh dấu khách không đến (NV01, NV02).</summary>
    public sealed class FrmDatPhong : Form
    {
        private readonly NguoiDung _user;
        private readonly DatPhongBLL _bll = new DatPhongBLL();
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
        private bool _dangTai;

        public FrmDatPhong(NguoiDung user)
        {
            _user = user;
            Text = "Đặt phòng";
            ClientSize = new Size(1040, 600);
            MinimumSize = new Size(760, 460);
            StartPosition = FormStartPosition.CenterParent;
            AutoScaleMode = AutoScaleMode.Dpi;

            _taoDatPhong = GiaoDien.TaoNut("Tạo đặt phòng", Luu_Click, true);
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
            _loc.SelectedIndexChanged += delegate { TaiGrid(); };
            var tool = GiaoDien.ThanhNgang();
            tool.Controls.Add(GiaoDien.Nhom("Lọc trạng thái", _loc, 120));
            var nutHang = new FlowLayoutPanel { AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, WrapContents = false, Margin = new Padding(0, 16, 0, 6) };
            nutHang.Controls.Add(GiaoDien.TaoNut("Nhận phòng từ đặt này", NhanPhong_Click));
            nutHang.Controls.Add(GiaoDien.TaoNut("Hủy đặt phòng", Huy_Click));
            nutHang.Controls.Add(GiaoDien.TaoNut("Khách không đến", KhongDen_Click));
            nutHang.Controls.Add(GiaoDien.TaoNut("Làm mới", delegate { TaiGrid(); TaiPhongTrongKhoang(); }));
            tool.Controls.Add(nutHang);

            Controls.Add(_grid);
            Controls.Add(tool);
            Controls.Add(nhap);

            _den.MinDate = DateTime.Today;
            _den.Value = DateTime.Today;
            _di.MinDate = DateTime.Today.AddDays(1);
            _di.Value = DateTime.Today.AddDays(1);
            _den.ValueChanged += NgayDen_ValueChanged;
            _di.ValueChanged += delegate { if (!_dangDieuChinhNgay) TaiPhongTrongKhoang(); };
            _phong.SelectedIndexChanged += delegate { CapNhatGoiYCoc(); };
            Load += delegate { TaiCombo(); TaiPhongTrongKhoang(); TaiGrid(); };
            GiaoDien.ApDung(this);
        }

        private void TaiCombo()
        {
            try
            {
                _khach.DisplayMember = "HoTen";
                _khach.ValueMember = "MaKhachHang";
                _khach.DataSource = new KhachHangBLL().TimKiem();
                _phong.DisplayMember = "SoPhong";
                _phong.ValueMember = "MaPhong";
            }
            catch (Exception ex) { UiHelper.ThongBaoLoi(this, "Không tải được danh sách khách", ex); }
        }

        private void KhachMoi_Click(object sender, EventArgs e)
        {
            using (var f = new FrmKhachNhanh())
            {
                if (f.ShowDialog(this) != DialogResult.OK) return;
                TaiCombo();
                if (f.MaKhachMoi > 0) _khach.SelectedValue = f.MaKhachMoi;
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
            TaiPhongTrongKhoang();
        }

        private void TaiPhongTrongKhoang()
        {
            if (_di.Value.Date <= _den.Value.Date) return;
            try
            {
                object dangChon = _phong.SelectedValue;
                DataTable phong = new PhongBLL().TimKiemTrongKhoang(_den.Value.Date, _di.Value.Date);
                _phong.DataSource = phong;
                if (dangChon != null)
                    foreach (DataRow r in phong.Rows)
                        if (Convert.ToInt32(r["MaPhong"]) == Convert.ToInt32(dangChon)) { _phong.SelectedValue = dangChon; break; }

                bool co = phong.Rows.Count > 0;
                _phong.Enabled = co;
                _taoDatPhong.Enabled = co;
                if (!co) _goiY.Text = "Không có phòng trống trong khoảng ngày này.";
                else CapNhatGoiYCoc();
            }
            catch (Exception ex) { UiHelper.ThongBaoLoi(this, "Không tải được phòng trống", ex); }
        }

        private void CapNhatGoiYCoc()
        {
            DataRowView r = _phong.SelectedItem as DataRowView;
            if (r == null || !r.DataView.Table.Columns.Contains("GiaMoiDem")) return;
            decimal gia = Convert.ToDecimal(r["GiaMoiDem"]);
            decimal toiDa = QuyTacTinhTien.TienCocToiDa(gia, _den.Value.Date, _di.Value.Date);
            _goiY.Text = string.Format("{0} phòng phù hợp · giá {1:N0} đ/đêm · cọc tối đa {2:N0} đ", _phong.Items.Count, gia, toiDa);
        }

        private void TaiGrid()
        {
            if (_dangTai) return;
            _dangTai = true;
            try
            {
                string tt = Convert.ToString(_loc.SelectedItem);
                DataTable t = _bll.TimKiem(null, null, tt == "Tất cả" || string.IsNullOrEmpty(tt) ? null : tt);
                UiHelper.GanNguon(_grid, t, "-MaDatPhong", "-MaKhachHang", "-MaPhong", "HoTen|Khách hàng", "SoPhong|Phòng", "Tang|Tầng",
                    "NgayDen|Ngày đến|dd/MM/yyyy", "NgayDi|Ngày đi|dd/MM/yyyy", "TienCoc|Tiền cọc|N0", "TrangThai|Trạng thái");
            }
            catch (Exception ex) { UiHelper.ThongBaoLoi(this, "Không tải được danh sách đặt phòng", ex); }
            finally { _dangTai = false; }
        }

        private DataGridViewRow Dong()
        {
            if (_grid.CurrentRow == null)
            {
                MessageBox.Show(this, "Hãy chọn một dòng đặt phòng.", "Chưa chọn", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return null;
            }
            return _grid.CurrentRow;
        }

        private void Luu_Click(object sender, EventArgs e)
        {
            if (_khach.SelectedValue == null || _phong.SelectedValue == null)
            {
                MessageBox.Show(this, "Chọn khách hàng và phòng.", "Thiếu dữ liệu", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            try
            {
                _bll.Them(new DatPhong
                {
                    MaKhachHang = Convert.ToInt32(_khach.SelectedValue),
                    MaPhong = Convert.ToInt32(_phong.SelectedValue),
                    MaNguoiDung = _user.MaNguoiDung,
                    NgayDen = _den.Value.Date,
                    NgayDi = _di.Value.Date,
                    TienCoc = _coc.Value
                });
                _coc.Value = 0;
                TaiGrid();
                TaiPhongTrongKhoang();
                UiHelper.ThongBaoThanhCong(this, "Đã tạo đặt phòng.");
            }
            catch (Exception ex) { UiHelper.ThongBaoLoi(this, "Không thể đặt phòng", ex); }
        }

        private void Huy_Click(object sender, EventArgs e)
        {
            DataGridViewRow r = Dong();
            if (r == null || !UiHelper.XacNhan(this, "Hủy đặt phòng này? Phòng sẽ được giải phóng cho khoảng ngày đó.")) return;
            try { _bll.Huy(Convert.ToInt32(r.Cells["MaDatPhong"].Value)); TaiGrid(); TaiPhongTrongKhoang(); }
            catch (Exception ex) { UiHelper.ThongBaoLoi(this, "Không thể hủy", ex); }
        }

        private void KhongDen_Click(object sender, EventArgs e)
        {
            DataGridViewRow r = Dong();
            if (r == null || !UiHelper.XacNhan(this, "Đánh dấu khách không đến? Phòng sẽ được giải phóng.")) return;
            try { _bll.DanhDauKhongDen(Convert.ToInt32(r.Cells["MaDatPhong"].Value)); TaiGrid(); TaiPhongTrongKhoang(); }
            catch (Exception ex) { UiHelper.ThongBaoLoi(this, "Không thể cập nhật", ex); }
        }

        private void NhanPhong_Click(object sender, EventArgs e)
        {
            DataGridViewRow r = Dong();
            if (r == null) return;
            if (Convert.ToString(r.Cells["TrangThai"].Value) != "Đã đặt")
            {
                MessageBox.Show(this, "Chỉ đặt phòng ở trạng thái 'Đã đặt' mới nhận phòng được.", "Không thể nhận", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            using (var f = new FrmNhanPhong(_user, Convert.ToInt32(r.Cells["MaDatPhong"].Value)))
                f.ShowDialog(this);
            TaiGrid();
            TaiPhongTrongKhoang();
        }
    }
}
