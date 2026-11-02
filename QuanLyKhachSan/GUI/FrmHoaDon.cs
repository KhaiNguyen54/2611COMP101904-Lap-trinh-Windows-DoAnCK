using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using QuanLyKhachSan.BLL;

namespace QuanLyKhachSan.GUI
{
    /// <summary>Tra cứu, in hóa đơn và xuất CSV (NV07).</summary>
    public sealed class FrmHoaDon : Form
    {
        private readonly DateTimePicker _tu = new DateTimePicker { Format = DateTimePickerFormat.Short };
        private readonly DateTimePicker _den = new DateTimePicker { Format = DateTimePickerFormat.Short };
        private readonly TextBox _tuKhoa = new TextBox();
        private readonly DataGridView _grid = UiHelper.TaoLuoi();
        private readonly Label _tong = new Label { AutoSize = true, Font = new Font("Microsoft Sans Serif", 9F, FontStyle.Bold) };

        public FrmHoaDon()
        {
            Text = "Tra cứu hóa đơn";
            ClientSize = new Size(1040, 580);
            MinimumSize = new Size(760, 420);
            StartPosition = FormStartPosition.CenterParent;
            AutoScaleMode = AutoScaleMode.Dpi;

            _tu.Value = DateTime.Today.AddDays(-30);
            _den.Value = DateTime.Today;
            var thanh = GiaoDien.ThanhNgang();
            thanh.Controls.Add(GiaoDien.Nhom("Từ ngày", _tu, 120));
            thanh.Controls.Add(GiaoDien.Nhom("Đến ngày", _den, 120));
            thanh.Controls.Add(GiaoDien.Nhom("Mã HĐ / khách / phòng", _tuKhoa, 200));
            var nut = new FlowLayoutPanel { AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, WrapContents = false, Margin = new Padding(0, 16, 0, 6) };
            nut.Controls.Add(GiaoDien.TaoNut("Tìm", delegate { Tai(); }, true));
            nut.Controls.Add(GiaoDien.TaoNut("In hóa đơn", In_Click));
            nut.Controls.Add(GiaoDien.TaoNut("Xuất CSV", delegate { UiHelper.XuatCsv(this, _grid, "HoaDon"); }));
            thanh.Controls.Add(nut);

            var chan = GiaoDien.ThanhNgang(DockStyle.Bottom);
            chan.Controls.Add(_tong);

            Controls.Add(_grid);
            Controls.Add(chan);
            Controls.Add(thanh);
            AcceptButton = null;
            _tuKhoa.KeyDown += delegate (object s, KeyEventArgs e) { if (e.KeyCode == Keys.Enter) { e.SuppressKeyPress = true; Tai(); } };
            Load += delegate { Tai(); };
            GiaoDien.ApDung(this);
        }

        private void Tai()
        {
            try
            {
                if (_tu.Value.Date > _den.Value.Date)
                {
                    MessageBox.Show(this, "Từ ngày phải trước hoặc bằng đến ngày.", "Khoảng ngày", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                DataTable t = new HoaDonBLL().TimKiem(_tu.Value.Date, _den.Value.Date, _tuKhoa.Text);
                UiHelper.GanNguon(_grid, t, "MaHoaDon|Mã HĐ", "-MaPhieuThue", "NgayLap|Ngày lập|dd/MM/yyyy HH:mm", "HoTen|Khách hàng", "SoPhong|Phòng",
                    "TienPhong|Tiền phòng|N0", "TienDichVu|Dịch vụ|N0", "PhuThu|Phụ thu|N0", "GiamGia|Giảm giá|N0", "TienCoc|Cọc|N0",
                    "TongThanhToan|Thanh toán|N0", "HinhThucThanhToan|Hình thức");
                decimal tong = 0;
                foreach (DataRow r in t.Rows) tong += Convert.ToDecimal(r["TongThanhToan"]);
                _tong.Text = t.Rows.Count + " hóa đơn · Tổng thanh toán: " + tong.ToString("N0") + " đ";
            }
            catch (Exception ex) { UiHelper.ThongBaoLoi(this, "Không thể tra cứu hóa đơn", ex); }
        }

        private void In_Click(object sender, EventArgs e)
        {
            if (_grid.CurrentRow == null)
            {
                MessageBox.Show(this, "Chọn một hóa đơn để in.", "Chưa chọn", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            try
            {
                DataRow hd = ((DataRowView)_grid.CurrentRow.DataBoundItem).Row;
                DataTable dv = new LuuTruBLL().LayDichVu(Convert.ToInt32(hd["MaPhieuThue"]));
                HoaDonPrinter.XemTruoc(this, hd, dv);
            }
            catch (Exception ex) { UiHelper.ThongBaoLoi(this, "Không thể in hóa đơn", ex); }
        }
    }
}
