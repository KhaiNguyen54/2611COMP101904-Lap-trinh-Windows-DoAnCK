using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using QuanLyKhachSan.Services;
using QuanLyKhachSan.Models;

namespace QuanLyKhachSan.Views
{
    /// <summary>Nhận phòng từ đặt trước hoặc cho khách vãng lai (NV03).</summary>
    public sealed class FrmNhanPhong : Form
    {
        private readonly NguoiDung _user;
        private readonly RadioButton _tuDat = new RadioButton { Text = "Khách đã đặt trước", AutoSize = true, Checked = true };
        private readonly RadioButton _vangLai = new RadioButton { Text = "Khách vãng lai", AutoSize = true };
        private readonly ComboBox _datPhong = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
        private readonly ComboBox _khach = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
        private readonly ComboBox _phong = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
        private readonly DateTimePicker _di = new DateTimePicker { Format = DateTimePickerFormat.Short };
        private readonly Label _thongTin = new Label { AutoSize = true, ForeColor = GiaoDien.Phu, MaximumSize = new Size(460, 0) };
        private readonly Button _nhan;
        private DataTable _dsDat;
        private bool _dangNap;

        public FrmNhanPhong(NguoiDung user, int? maDatPhong = null)
        {
            _user = user;
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
            _nhan = GiaoDien.TaoNut("Nhận phòng", Nhan_Click, true);
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
            _tuDat.CheckedChanged += delegate { CapNhatCheDo(); };
            _datPhong.SelectedIndexChanged += delegate { ChonDatPhong(); };
            _di.ValueChanged += delegate { if (!_dangNap && _vangLai.Checked) TaiPhongTrong(); };
            Load += delegate
            {
                TaiDat(maDatPhong);
                TaiKhach();
                CapNhatCheDo();
            };
            GiaoDien.ApDung(this);
        }

        private void CapNhatCheDo()
        {
            bool dat = _tuDat.Checked;
            _datPhong.Enabled = dat;
            _khach.Enabled = !dat;
            _phong.Enabled = !dat;
            if (dat) ChonDatPhong();
            else
            {
                TaiPhongTrong();
                _thongTin.Text = "Khách vãng lai: chỉ hiện phòng trống từ hôm nay đến ngày đi dự kiến.";
            }
        }

        private void TaiDat(int? maDatPhong)
        {
            try
            {
                _dsDat = new DatPhongService().LayDangDat();
                _dsDat.Columns.Add("Nhan", typeof(string));
                foreach (DataRow r in _dsDat.Rows)
                    r["Nhan"] = string.Format("Phòng {0} · {1} · {2:dd/MM}–{3:dd/MM}", r["SoPhong"], r["HoTen"], r["NgayDen"], r["NgayDi"]);
                _dangNap = true;
                _datPhong.DisplayMember = "Nhan";
                _datPhong.ValueMember = "MaDatPhong";
                _datPhong.DataSource = _dsDat;
                _dangNap = false;
                if (maDatPhong.HasValue) _datPhong.SelectedValue = maDatPhong.Value;
                if (_dsDat.Rows.Count == 0) _vangLai.Checked = true;
            }
            catch (Exception ex) { UiHelper.ThongBaoLoi(this, "Không tải được đặt phòng", ex); }
        }

        private void TaiKhach()
        {
            try
            {
                _khach.DisplayMember = "HoTen";
                _khach.ValueMember = "MaKhachHang";
                _khach.DataSource = new KhachHangService().TimKiem();
            }
            catch (Exception ex) { UiHelper.ThongBaoLoi(this, "Không tải được khách hàng", ex); }
        }

        private void ChonDatPhong()
        {
            if (_dangNap || !_tuDat.Checked) return;
            DataRowView r = _datPhong.SelectedItem as DataRowView;
            if (r == null)
            {
                _thongTin.Text = "Không có đặt phòng nào đang chờ nhận.";
                _nhan.Enabled = false;
                return;
            }
            _nhan.Enabled = true;
            DateTime ngayDi = Convert.ToDateTime(r["NgayDi"]).Date;
            DateTime den = Convert.ToDateTime(r["NgayDen"]).Date;
            _dangNap = true;
            _di.MinDate = DateTime.Today.AddDays(1);
            _di.Value = ngayDi > DateTime.Today ? ngayDi : DateTime.Today.AddDays(1);
            _dangNap = false;
            _thongTin.Text = string.Format("Cọc đã nhận: {0:N0} đ. Ngày đến đặt: {1:dd/MM/yyyy}{2}", r["TienCoc"], den,
                den > DateTime.Today ? " (khách đến sớm hơn lịch đặt)." : ".");
        }

        private void TaiPhongTrong()
        {
            try
            {
                _phong.DisplayMember = "SoPhong";
                _phong.ValueMember = "MaPhong";
                DataTable t = new PhongService().TimKiemTrongKhoang(DateTime.Today, _di.Value.Date);
                _phong.DataSource = t;
                _nhan.Enabled = t.Rows.Count > 0;
                if (t.Rows.Count == 0) _thongTin.Text = "Không có phòng trống trong khoảng này.";
            }
            catch (Exception ex) { UiHelper.ThongBaoLoi(this, "Không tải được phòng trống", ex); }
        }

        private void KhachMoi_Click(object sender, EventArgs e)
        {
            using (var f = new FrmKhachNhanh())
            {
                if (f.ShowDialog(this) != DialogResult.OK) return;
                TaiKhach();
                if (f.MaKhachMoi > 0) _khach.SelectedValue = f.MaKhachMoi;
            }
        }

        private void Nhan_Click(object sender, EventArgs e)
        {
            try
            {
                int maPhong, maKhach;
                int? maDat = null;
                if (_tuDat.Checked)
                {
                    DataRowView r = _datPhong.SelectedItem as DataRowView;
                    if (r == null) return;
                    maDat = Convert.ToInt32(r["MaDatPhong"]);
                    maPhong = Convert.ToInt32(r["MaPhong"]);
                    maKhach = Convert.ToInt32(r["MaKhachHang"]);
                }
                else
                {
                    if (_phong.SelectedValue == null || _khach.SelectedValue == null)
                    {
                        MessageBox.Show(this, "Chọn khách hàng và phòng.", "Thiếu dữ liệu", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                    maPhong = Convert.ToInt32(_phong.SelectedValue);
                    maKhach = Convert.ToInt32(_khach.SelectedValue);
                }

                int ma = new LuuTruService().NhanPhong(maPhong, maKhach, _user.MaNguoiDung, _di.Value.Date, maDat);
                UiHelper.ThongBaoThanhCong(this, "Đã nhận phòng. Mã phiếu thuê: " + ma);
                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex) { UiHelper.ThongBaoLoi(this, "Không thể nhận phòng", ex); }
        }
    }
}
