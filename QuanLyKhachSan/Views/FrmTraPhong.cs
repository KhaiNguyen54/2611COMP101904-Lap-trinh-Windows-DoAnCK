using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using QuanLyKhachSan.Presenters;
using QuanLyKhachSan.Services;
using QuanLyKhachSan.Models;

namespace QuanLyKhachSan.Views
{
    /// <summary>Trả phòng, tính tiền và lập hóa đơn (NV05, NV06).</summary>
    public sealed class FrmTraPhong : Form, ITraPhongView
    {
        private readonly LuuTruService _bll = new LuuTruService();
        private readonly TraPhongPresenter _presenter;
        private readonly ComboBox _phieu = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
        private readonly DateTimePicker _ngayTra = new DateTimePicker { Format = DateTimePickerFormat.Custom, CustomFormat = "dd/MM/yyyy HH:mm" };
        private readonly NumericUpDown _giam = new NumericUpDown { Maximum = 1000000000, Increment = 10000, ThousandsSeparator = true };
        private readonly ComboBox _hinhThuc = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
        private readonly Label _soDem = TaoGiaTri();
        private readonly Label _tienPhong = TaoGiaTri();
        private readonly Label _tienDv = TaoGiaTri();
        private readonly Label _phuThu = TaoGiaTri();
        private readonly Label _coc = TaoGiaTri();
        private readonly Label _giamLb = TaoGiaTri();
        private readonly Label _tong = TaoGiaTri(true);
        private readonly Label _ghiChu = new Label { AutoSize = true, ForeColor = GiaoDien.Phu, MaximumSize = new Size(520, 0) };
        private readonly Button _tra;

        public FrmTraPhong(NguoiDung user)
        {
            _presenter = new TraPhongPresenter(this, _bll, user);
            Text = "Trả phòng";
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            AutoScaleMode = AutoScaleMode.Dpi;
            AutoSize = true;
            AutoSizeMode = AutoSizeMode.GrowAndShrink;
            MinimumSize = new Size(560, 0);

            _phieu.DropDownWidth = 400;
            _hinhThuc.Items.AddRange(new object[] { "Tiền mặt", "Chuyển khoản", "Thẻ" });
            _hinhThuc.SelectedIndex = 0;
            _ngayTra.Value = DateTime.Now;

            var nhap = GiaoDien.BangNhap(
                new System.Collections.Generic.KeyValuePair<string, Control>("Phòng đang ở", _phieu),
                new System.Collections.Generic.KeyValuePair<string, Control>("Thời điểm trả", _ngayTra),
                new System.Collections.Generic.KeyValuePair<string, Control>("Giảm giá (đ)", _giam),
                new System.Collections.Generic.KeyValuePair<string, Control>("Hình thức", _hinhThuc));

            var kq = new TableLayoutPanel { AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, ColumnCount = 2, Padding = new Padding(10, 4, 10, 4) };
            kq.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            kq.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            Dong(kq, "Số đêm", _soDem);
            Dong(kq, "Tiền phòng", _tienPhong);
            Dong(kq, "Tiền dịch vụ", _tienDv);
            Dong(kq, "Phụ thu trả muộn", _phuThu);
            Dong(kq, "Trừ tiền cọc", _coc);
            Dong(kq, "Giảm giá", _giamLb);
            Dong(kq, "KHÁCH CẦN THANH TOÁN", _tong, true);

            _tra = GiaoDien.TaoNut("Trả phòng và lập hóa đơn", delegate { _presenter.Tra(); }, true);
            var dong = GiaoDien.TaoNut("Đóng");
            dong.DialogResult = DialogResult.Cancel;
            var nut = GiaoDien.HangNut(FlowDirection.RightToLeft);
            nut.Controls.Add(dong);
            nut.Controls.Add(_tra);

            var goc = GiaoDien.GocTuCo();
            foreach (Control c in new Control[] { nhap, kq, _ghiChu, nut })
            {
                c.Dock = DockStyle.Fill;
                goc.RowStyles.Add(new RowStyle(SizeType.AutoSize));
                goc.Controls.Add(c, 0, goc.RowCount++);
            }
            _ghiChu.Margin = new Padding(10, 4, 10, 4);
            Controls.Add(goc);
            CancelButton = dong;

            _phieu.SelectedIndexChanged += delegate { _presenter.TinhLai(); };
            _ngayTra.ValueChanged += delegate { _presenter.TinhLai(); };
            _giam.ValueChanged += delegate { _presenter.HienTong(); };
            Load += delegate { _presenter.TaiPhieu(); };
            GiaoDien.ApDung(this);
        }

        private static Label TaoGiaTri(bool lon = false)
        {
            return new Label
            {
                AutoSize = true,
                Anchor = AnchorStyles.Right,
                TextAlign = ContentAlignment.MiddleRight,
                Font = new Font("Microsoft Sans Serif", lon ? 12F : 8.25F, FontStyle.Bold),
                ForeColor = lon ? GiaoDien.MauThu : GiaoDien.Chu,
                Margin = new Padding(8, 3, 0, 3)
            };
        }

        private static void Dong(TableLayoutPanel b, string nhan, Label gia, bool dam = false)
        {
            int r = b.RowCount++;
            b.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            b.Controls.Add(new Label { Text = nhan, AutoSize = true, Anchor = AnchorStyles.Left, Margin = new Padding(0, 3, 0, 3), Font = dam ? new Font("Microsoft Sans Serif", 9F, FontStyle.Bold) : null }, 0, r);
            b.Controls.Add(gia, 1, r);
        }

        // ----- ITraPhongView -----
        public int? PhieuChon { get { return _phieu.SelectedValue == null ? (int?)null : Convert.ToInt32(_phieu.SelectedValue); } }
        public DateTime NgayTra { get { return _ngayTra.Value; } }
        public decimal GiamGia { get { return _giam.Value; } }
        public string HinhThuc { get { return Convert.ToString(_hinhThuc.SelectedItem); } }

        public void HienPhieu(DataTable phieu)
        {
            _phieu.DisplayMember = "Nhan";
            _phieu.ValueMember = "MaPhieuThue";
            _phieu.DataSource = phieu;
        }

        public void HienBaoGia(string soDem, string tienPhong, string tienDichVu, string phuThu, string tienCoc)
        {
            _soDem.Text = soDem;
            _tienPhong.Text = tienPhong;
            _tienDv.Text = tienDichVu;
            _phuThu.Text = phuThu;
            _coc.Text = tienCoc;
        }

        public void DatGiamToiDa(decimal toiDa) { _giam.Maximum = toiDa; }

        public void HienGiamVaTong(string giam, string tong)
        {
            _giamLb.Text = giam;
            _tong.Text = tong;
        }

        public void DatGhiChu(string noiDung) { _ghiChu.Text = noiDung; }
        public void DatChoPhepTra(bool duocTra) { _tra.Enabled = duocTra; }

        public int? YeuCauQuanLyDuyet()
        {
            using (var duyet = new FrmDuyetGiamGia(_bll))
            {
                if (duyet.ShowDialog(this) != DialogResult.OK) return null;
                return duyet.MaNguoiDuyet;
            }
        }

        public void MoBanIn(int maHoaDon, int maPhieu)
        {
            try
            {
                DataTable hd = new HoaDonService().TimKiem(DateTime.Today.AddDays(-1), DateTime.Today, maHoaDon.ToString());
                if (hd.Rows.Count == 0) return;
                HoaDonPrinter.XemTruoc(this, hd.Rows[0], _bll.LayDichVu(maPhieu));
            }
            catch (Exception ex) { UiHelper.ThongBaoLoi(this, "Không thể mở bản in", ex); }
        }

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
