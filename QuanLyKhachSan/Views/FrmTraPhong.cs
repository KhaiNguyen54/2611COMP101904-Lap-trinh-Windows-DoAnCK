using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using QuanLyKhachSan.Services;
using QuanLyKhachSan.Models;

namespace QuanLyKhachSan.Views
{
    /// <summary>Trả phòng, tính tiền và lập hóa đơn (NV05, NV06).</summary>
    public sealed class FrmTraPhong : Form
    {
        private readonly NguoiDung _user;
        private readonly LuuTruService _bll = new LuuTruService();
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
        private BaoGiaHoaDon _baoGia;

        public FrmTraPhong(NguoiDung user)
        {
            _user = user;
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

            _tra = GiaoDien.TaoNut("Trả phòng và lập hóa đơn", Tra_Click, true);
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

            _phieu.SelectedIndexChanged += delegate { TinhLai(); };
            _ngayTra.ValueChanged += delegate { TinhLai(); };
            _giam.ValueChanged += delegate { HienTong(); };
            Load += delegate { TaiPhieu(); };
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

        private void TaiPhieu()
        {
            try
            {
                DataTable t = _bll.LayPhieuDangO();
                t.Columns.Add("Nhan", typeof(string));
                foreach (DataRow r in t.Rows)
                    r["Nhan"] = string.Format("Phòng {0} · {1} · từ {2:dd/MM}", r["SoPhong"], r["HoTen"], r["NgayNhan"]);
                _phieu.DisplayMember = "Nhan";
                _phieu.ValueMember = "MaPhieuThue";
                _phieu.DataSource = t;
                _tra.Enabled = t.Rows.Count > 0;
                if (t.Rows.Count == 0) _ghiChu.Text = "Chưa có phòng nào đang có khách.";
            }
            catch (Exception ex) { UiHelper.ThongBaoLoi(this, "Không tải được phòng đang ở", ex); }
        }

        private void TinhLai()
        {
            if (_phieu.SelectedValue == null) return;
            try
            {
                _baoGia = _bll.TinhTamTinh(Convert.ToInt32(_phieu.SelectedValue), _ngayTra.Value);
                _soDem.Text = _baoGia.SoDem.ToString();
                _tienPhong.Text = Tien(_baoGia.TienPhong);
                _tienDv.Text = Tien(_baoGia.TienDichVu);
                _phuThu.Text = Tien(_baoGia.PhuThu);
                _coc.Text = (_baoGia.TienCoc > 0 ? "-" : string.Empty) + Tien(_baoGia.TienCoc);
                decimal co = QuyTacTinhTien.CoSoGiamGia(_baoGia.TienPhong, _baoGia.TienDichVu, _baoGia.PhuThu);
                _giam.Maximum = Math.Max(0, co);
                _ghiChu.Text = "Trả sau 12:00: ≤1 giờ miễn phí, 1–3 giờ 30%, 3–6 giờ 50%, trên 6 giờ 100% giá một đêm. " +
                               "Giảm giá tối đa 10% tiền hàng; vượt mức cần Quản lý duyệt" +
                               (_user.VaiTro == VaiTroNguoiDung.QuanLy ? " (bạn là Quản lý nên được ghi là người duyệt)." : " (nhập tài khoản Quản lý khi trả phòng).");
                _tra.Enabled = true;
                HienTong();
            }
            catch (Exception ex)
            {
                _tra.Enabled = false;
                _baoGia = null;
                _ghiChu.Text = ex.Message;
            }
        }

        private void HienTong()
        {
            if (_baoGia == null) return;
            _giamLb.Text = (_giam.Value > 0 ? "-" : string.Empty) + Tien(_giam.Value);
            decimal tong = QuyTacTinhTien.TinhTongThanhToan(_baoGia.TienPhong, _baoGia.TienDichVu, _baoGia.PhuThu, _giam.Value, _baoGia.TienCoc);
            _tong.Text = Tien(tong);
        }

        private static string Tien(decimal v) { return v.ToString("N0") + " đ"; }

        private void Tra_Click(object sender, EventArgs e)
        {
            if (_phieu.SelectedValue == null) return;
            if (!UiHelper.XacNhan(this, "Xác nhận trả phòng và lập hóa đơn? Thao tác này không hoàn tác được.")) return;
            try
            {
                int maPhieu = Convert.ToInt32(_phieu.SelectedValue);
                bool laQuanLy = _user.VaiTro == VaiTroNguoiDung.QuanLy;
                int? nguoiDuyet = null;
                if (_baoGia != null && !laQuanLy && QuyTacTinhTien.CanQuanLyDuyet(_giam.Value,
                        QuyTacTinhTien.CoSoGiamGia(_baoGia.TienPhong, _baoGia.TienDichVu, _baoGia.PhuThu)))
                {
                    using (var duyet = new FrmDuyetGiamGia(_bll))
                    {
                        if (duyet.ShowDialog(this) != DialogResult.OK) return;
                        nguoiDuyet = duyet.MaNguoiDuyet;
                    }
                }
                int maHd = _bll.TraPhong(maPhieu, _user.MaNguoiDung, _ngayTra.Value, _giam.Value,
                    Convert.ToString(_hinhThuc.SelectedItem), laQuanLy, nguoiDuyet);

                if (UiHelper.XacNhan(this, "Đã lập hóa đơn số " + maHd + ". In hóa đơn ngay?"))
                    InHoaDon(maHd, maPhieu);
                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex) { UiHelper.ThongBaoLoi(this, "Không thể trả phòng", ex); }
        }

        private void InHoaDon(int maHd, int maPhieu)
        {
            try
            {
                DataTable hd = new HoaDonService().TimKiem(DateTime.Today.AddDays(-1), DateTime.Today, maHd.ToString());
                if (hd.Rows.Count == 0) return;
                HoaDonPrinter.XemTruoc(this, hd.Rows[0], _bll.LayDichVu(maPhieu));
            }
            catch (Exception ex) { UiHelper.ThongBaoLoi(this, "Không thể mở bản in", ex); }
        }
    }
}
