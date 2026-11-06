using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using QuanLyKhachSan.Services;
using QuanLyKhachSan.Models;

namespace QuanLyKhachSan.Views
{
    /// <summary>Ghi nhận và xóa dịch vụ phát sinh cho phiếu thuê đang ở (NV04).</summary>
    public sealed class FrmGhiDichVu : Form
    {
        private readonly LuuTruService _bll = new LuuTruService();
        private readonly ComboBox _phieu = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
        private readonly ComboBox _dichVu = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
        private readonly NumericUpDown _soLuong = new NumericUpDown { DecimalPlaces = 0, Minimum = 1, Maximum = 999, Value = 1 };
        private readonly DataGridView _grid = UiHelper.TaoLuoi();
        private readonly Label _tong = new Label { AutoSize = true, Font = new Font("Microsoft Sans Serif", 9F, FontStyle.Bold) };

        public FrmGhiDichVu(NguoiDung user)
        {
            Text = "Ghi dịch vụ";
            ClientSize = new Size(900, 560);
            MinimumSize = new Size(680, 420);
            StartPosition = FormStartPosition.CenterParent;
            AutoScaleMode = AutoScaleMode.Dpi;

            _phieu.DropDownWidth = 380;
            _dichVu.DropDownWidth = 320;
            var nhap = GiaoDien.ThanhNgang();
            nhap.Controls.Add(GiaoDien.Nhom("Phòng đang ở", _phieu, 260));
            nhap.Controls.Add(GiaoDien.Nhom("Dịch vụ", _dichVu, 220));
            nhap.Controls.Add(GiaoDien.Nhom("Số lượng", _soLuong, 80));
            var nut = new FlowLayoutPanel { AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, WrapContents = false, Margin = new Padding(0, 16, 0, 6) };
            nut.Controls.Add(GiaoDien.TaoNut("Ghi dịch vụ", Ghi_Click, true));
            nut.Controls.Add(GiaoDien.TaoNut("Xóa dòng đã chọn", Xoa_Click));
            nhap.Controls.Add(nut);

            var chan = GiaoDien.ThanhNgang(DockStyle.Bottom);
            chan.Controls.Add(_tong);

            Controls.Add(_grid);
            Controls.Add(chan);
            Controls.Add(nhap);
            _phieu.SelectedIndexChanged += delegate { TaiDong(); };
            Load += delegate { TaiPhieu(); TaiDichVu(); };
            GiaoDien.ApDung(this);
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
                if (t.Rows.Count == 0) _tong.Text = "Chưa có phòng nào đang có khách.";
            }
            catch (Exception ex) { UiHelper.ThongBaoLoi(this, "Không tải được phòng đang ở", ex); }
        }

        private void TaiDichVu()
        {
            try
            {
                DataTable t = new DichVuService().TimKiem(null, true);
                t.Columns.Add("Nhan", typeof(string));
                foreach (DataRow r in t.Rows)
                    r["Nhan"] = string.Format("{0} — {1:N0} đ/{2}", r["TenDichVu"], r["DonGia"], r["DonViTinh"]);
                _dichVu.DisplayMember = "Nhan";
                _dichVu.ValueMember = "MaDichVu";
                _dichVu.DataSource = t;
            }
            catch (Exception ex) { UiHelper.ThongBaoLoi(this, "Không tải được dịch vụ", ex); }
        }

        private void TaiDong()
        {
            if (_phieu.SelectedValue == null)
            {
                _grid.DataSource = null;
                return;
            }
            try
            {
                DataTable t = _bll.LayDichVu(Convert.ToInt32(_phieu.SelectedValue));
                UiHelper.GanNguon(_grid, t, "-MaChiTietDichVu", "-MaDichVu", "TenDichVu|Dịch vụ", "SoLuong|Số lượng|0.##",
                    "DonGia|Đơn giá|N0", "ThoiDiemSuDung|Thời điểm|dd/MM/yyyy HH:mm", "ThanhTien|Thành tiền|N0");
                decimal tong = 0;
                foreach (DataRow r in t.Rows) tong += Convert.ToDecimal(r["ThanhTien"]);
                _tong.Text = "Tổng dịch vụ: " + tong.ToString("N0") + " đ (" + t.Rows.Count + " dòng)";
            }
            catch (Exception ex) { UiHelper.ThongBaoLoi(this, "Không tải được dịch vụ đã ghi", ex); }
        }

        private void Ghi_Click(object sender, EventArgs e)
        {
            if (_phieu.SelectedValue == null || _dichVu.SelectedValue == null)
            {
                MessageBox.Show(this, "Chọn phòng đang ở và dịch vụ.", "Thiếu dữ liệu", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            try
            {
                _bll.GhiDichVu(Convert.ToInt32(_phieu.SelectedValue), Convert.ToInt32(_dichVu.SelectedValue), _soLuong.Value);
                _soLuong.Value = 1;
                TaiDong();
            }
            catch (Exception ex) { UiHelper.ThongBaoLoi(this, "Không thể ghi dịch vụ", ex); }
        }

        private void Xoa_Click(object sender, EventArgs e)
        {
            if (_grid.CurrentRow == null) return;
            if (!UiHelper.XacNhan(this, "Xóa dòng dịch vụ đã chọn?")) return;
            try
            {
                _bll.XoaDichVu(Convert.ToInt32(_grid.CurrentRow.Cells["MaChiTietDichVu"].Value));
                TaiDong();
            }
            catch (Exception ex) { UiHelper.ThongBaoLoi(this, "Không thể xóa", ex); }
        }
    }
}
