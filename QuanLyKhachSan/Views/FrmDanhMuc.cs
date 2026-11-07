using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;
using QuanLyKhachSan.Presenters;
using QuanLyKhachSan.Services;
using QuanLyKhachSan.Models;

namespace QuanLyKhachSan.Views
{
    /// <summary>
    /// Màn hình danh mục dùng chung: Khách hàng, Loại phòng, Phòng, Dịch vụ.
    /// Lễ tân chỉ được sửa Khách hàng; các danh mục còn lại chỉ xem (ma trận quyền ở docs/04).
    /// </summary>
    public class FrmDanhMuc : Form, IDanhMucView
    {
        private readonly LoaiDanhMuc _loai;
        private readonly DanhMucPresenter _presenter;
        private readonly bool _chiXem;
        private readonly TextBox _tuKhoa = new TextBox();
        private readonly DataGridView _grid = UiHelper.TaoLuoi();
        private readonly string[] _nhan;
        private readonly string[] _cot;
        private readonly Control[] _o;
        private readonly Button _luu = GiaoDien.TaoNut("Lưu", null, true);
        private readonly Button _ngung = GiaoDien.TaoNut("Ngừng sử dụng");
        private readonly Label _trangThai = new Label { AutoSize = true, ForeColor = GiaoDien.Phu, Margin = new Padding(8, 6, 0, 0) };
        private int _ma;

        public FrmDanhMuc(LoaiDanhMuc loai, NguoiDung nguoiDung)
        {
            _loai = loai;
            _presenter = new DanhMucPresenter(this, loai, new LoaiPhongService(), new PhongService(), new KhachHangService(), new DichVuService());
            _chiXem = DanhMucPresenter.ChiXem(loai, nguoiDung);

            Text = "Danh mục " + DanhMucPresenter.Ten(loai).ToLower() + (_chiXem ? " (chỉ xem)" : string.Empty);
            ClientSize = new Size(980, 640);
            MinimumSize = new Size(720, 520);
            StartPosition = FormStartPosition.CenterParent;
            AutoScaleMode = AutoScaleMode.Dpi;

            switch (loai)
            {
                case LoaiDanhMuc.LoaiPhong:
                    _nhan = new[] { "Tên loại", "Mô tả", "Giá mỗi đêm (đ)", "Sức chứa (người)" };
                    _cot = new[] { "TenLoaiPhong", "MoTa", "GiaMoiDem", "SucChua" };
                    break;
                case LoaiDanhMuc.Phong:
                    _nhan = new[] { "Số phòng", "Tầng", "Loại phòng", "Ghi chú" };
                    _cot = new[] { "SoPhong", "Tang", "MaLoaiPhong", "GhiChu" };
                    break;
                case LoaiDanhMuc.KhachHang:
                    _nhan = new[] { "Họ tên (*)", "Số giấy tờ (*)", "Điện thoại", "Email", "Địa chỉ" };
                    _cot = new[] { "HoTen", "SoGiayTo", "SoDienThoai", "Email", "DiaChi" };
                    break;
                default:
                    _nhan = new[] { "Tên dịch vụ", "Đơn giá (đ)", "Đơn vị tính" };
                    _cot = new[] { "TenDichVu", "DonGia", "DonViTinh" };
                    break;
            }

            _o = new Control[_nhan.Length];
            for (int i = 0; i < _o.Length; i++)
                _o[i] = TaoO(_cot[i]);

            var tim = GiaoDien.ThanhNgang();
            tim.Controls.Add(GiaoDien.Nhom("Tìm kiếm", _tuKhoa, 260));
            tim.Controls.Add(Hang(GiaoDien.TaoNut("Tìm", delegate { _presenter.TaiDuLieu(); }, true)));
            _tuKhoa.KeyDown += delegate (object s, KeyEventArgs e) { if (e.KeyCode == Keys.Enter) { e.SuppressKeyPress = true; _presenter.TaiDuLieu(); } };

            var nhap = new TableLayoutPanel
            {
                Dock = DockStyle.Bottom,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                ColumnCount = 1,
                Padding = new Padding(10, 4, 10, 6),
                BackColor = GiaoDien.BeMat,
                BorderStyle = BorderStyle.Fixed3D
            };
            nhap.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            nhap.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            var truong = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, WrapContents = true, Margin = new Padding(0) };
            for (int i = 0; i < _o.Length; i++)
                truong.Controls.Add(GiaoDien.Nhom(_nhan[i], _o[i], i == 1 && loai == LoaiDanhMuc.LoaiPhong || _cot[i] == "DiaChi" || _cot[i] == "GhiChu" ? 260 : 170));
            nhap.Controls.Add(truong, 0, 0);

            var nut = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, WrapContents = true, Margin = new Padding(0, 4, 0, 0) };
            nut.Controls.Add(GiaoDien.TaoNut("Tạo mới", delegate { LamMoiFormNhap(); }));
            _luu.Click += delegate { _presenter.Luu(); };
            nut.Controls.Add(_luu);
            if (loai != LoaiDanhMuc.KhachHang)
            {
                _ngung.Click += delegate { _presenter.Ngung(); };
                nut.Controls.Add(_ngung);
            }
            nut.Controls.Add(_trangThai);
            nhap.Controls.Add(nut, 0, 1);

            if (_chiXem)
            {
                foreach (Control c in _o) c.Enabled = false;
                _luu.Enabled = false;
                _ngung.Enabled = false;
                _trangThai.Text = "Chỉ Quản lý được thêm/sửa danh mục này.";
            }

            Controls.Add(_grid);
            Controls.Add(nhap);
            Controls.Add(tim);
            _grid.SelectionChanged += ChonDong;
            Load += delegate { _presenter.TaiBanDau(); };
            GiaoDien.ApDung(this);
        }

        private static Control Hang(Button b)
        {
            var p = new FlowLayoutPanel { AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, WrapContents = false, Margin = new Padding(0, 16, 0, 6) };
            p.Controls.Add(b);
            return p;
        }

        private Control TaoO(string cot)
        {
            if (cot == "GiaMoiDem" || cot == "DonGia")
                return new NumericUpDown { Maximum = 1000000000, Increment = 10000, ThousandsSeparator = true };
            if (cot == "SucChua")
                return new NumericUpDown { Minimum = 1, Maximum = 20, Value = 2 };
            if (cot == "Tang")
                return new NumericUpDown { Minimum = 1, Maximum = 99, Value = 1 };
            if (cot == "MaLoaiPhong")
                return new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
            return new TextBox();
        }

        // ----- IDanhMucView -----
        public string TuKhoa { get { return _tuKhoa.Text; } }
        public int MaDangSua { get { return _ma; } }

        public string DocChu(int viTri) { return _o[viTri].Text; }
        public decimal DocSo(int viTri) { return ((NumericUpDown)_o[viTri]).Value; }
        public int? DocChon(int viTri)
        {
            object v = ((ComboBox)_o[viTri]).SelectedValue;
            return v == null ? (int?)null : Convert.ToInt32(v);
        }

        public void HienLoaiPhong(DataTable loaiPhong)
        {
            var cb = (ComboBox)_o[2];
            cb.DisplayMember = "TenLoaiPhong";
            cb.ValueMember = "MaLoaiPhong";
            cb.DataSource = loaiPhong.DefaultView;
        }

        public void HienDanhSach(DataTable t)
        {
            switch (_loai)
            {
                case LoaiDanhMuc.LoaiPhong:
                    UiHelper.GanNguon(_grid, t, "-MaLoaiPhong", "TenLoaiPhong|Tên loại", "MoTa|Mô tả", "GiaMoiDem|Giá/đêm (đ)|N0", "SucChua|Sức chứa|0", "HoatDong|Hoạt động");
                    break;
                case LoaiDanhMuc.Phong:
                    UiHelper.GanNguon(_grid, t, "-MaPhong", "-MaLoaiPhong", "SoPhong|Số phòng", "Tang|Tầng", "TenLoaiPhong|Loại phòng", "TrangThai|Trạng thái", "GhiChu|Ghi chú", "HoatDong|Hoạt động");
                    break;
                case LoaiDanhMuc.KhachHang:
                    UiHelper.GanNguon(_grid, t, "-MaKhachHang", "HoTen|Họ tên", "SoGiayTo|Số giấy tờ", "SoDienThoai|Điện thoại", "Email|Email", "DiaChi|Địa chỉ");
                    break;
                default:
                    UiHelper.GanNguon(_grid, t, "-MaDichVu", "TenDichVu|Tên dịch vụ", "DonGia|Đơn giá (đ)|N0", "DonViTinh|Đơn vị", "HoatDong|Hoạt động");
                    break;
            }
        }

        public void LamMoiFormNhap()
        {
            _ma = 0;
            _grid.ClearSelection();
            foreach (Control c in _o)
            {
                if (c is TextBox) ((TextBox)c).Clear();
                else if (c is NumericUpDown) ((NumericUpDown)c).Value = Math.Max(((NumericUpDown)c).Minimum, 0);
            }
            if (!_chiXem) _trangThai.Text = "Đang tạo bản ghi mới.";
            if (_o.Length > 0) _o[0].Focus();
        }

        public void DatTrangThai(string noiDung) { _trangThai.Text = noiDung; }

        public void HienThongBao(string tieuDe, string noiDung, MucThongBao muc)
        {
            UiHelper.HienThongBao(this, tieuDe, noiDung, muc);
        }

        public bool XacNhan(string noiDung) { return UiHelper.XacNhan(this, noiDung); }

        private void ChonDong(object sender, EventArgs e)
        {
            if (_grid.CurrentRow == null || _grid.CurrentRow.IsNewRow || !_grid.CurrentRow.Selected) return;
            _ma = Convert.ToInt32(_grid.CurrentRow.Cells[0].Value);
            for (int i = 0; i < _o.Length; i++)
            {
                DataGridViewColumn c = _grid.Columns[_cot[i]];
                if (c == null) continue;
                object v = _grid.CurrentRow.Cells[c.Index].Value;
                if (v == null || v == DBNull.Value) v = string.Empty;
                if (_o[i] is NumericUpDown)
                {
                    decimal d;
                    var n = (NumericUpDown)_o[i];
                    if (decimal.TryParse(Convert.ToString(v, CultureInfo.InvariantCulture), NumberStyles.Any, CultureInfo.InvariantCulture, out d))
                        n.Value = Math.Min(n.Maximum, Math.Max(n.Minimum, d));
                }
                else if (_o[i] is ComboBox)
                {
                    if (v is null || v == DBNull.Value)
                        ((ComboBox)_o[i]).SelectedValue = 0;
                    else if (v is string s && string.IsNullOrEmpty(s))
                        ((ComboBox)_o[i]).SelectedValue = 0;
                    else
                        ((ComboBox)_o[i]).SelectedValue = v is int ? v : Convert.ToInt32(v, CultureInfo.InvariantCulture);
                }
                else
                    _o[i].Text = Convert.ToString(v);
            }
            if (!_chiXem) _trangThai.Text = "Đang sửa bản ghi đã chọn.";
        }
    }

    public sealed class FrmLoaiPhong : FrmDanhMuc { public FrmLoaiPhong(NguoiDung u) : base(LoaiDanhMuc.LoaiPhong, u) { } }
    public sealed class FrmPhong : FrmDanhMuc { public FrmPhong(NguoiDung u) : base(LoaiDanhMuc.Phong, u) { } }
    public sealed class FrmKhachHang : FrmDanhMuc { public FrmKhachHang(NguoiDung u) : base(LoaiDanhMuc.KhachHang, u) { } }
    public sealed class FrmDichVu : FrmDanhMuc { public FrmDichVu(NguoiDung u) : base(LoaiDanhMuc.DichVu, u) { } }
}
