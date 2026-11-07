using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using QuanLyKhachSan.Presenters;
using QuanLyKhachSan.Services;
using QuanLyKhachSan.Models;

namespace QuanLyKhachSan.Views
{
    /// <summary>Ghi nhận và xóa dịch vụ phát sinh cho phiếu thuê đang ở (NV04).</summary>
    public sealed class FrmGhiDichVu : Form, IGhiDichVuView
    {
        private readonly GhiDichVuPresenter _presenter;
        private readonly ComboBox _phieu = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
        private readonly ComboBox _dichVu = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
        private readonly NumericUpDown _soLuong = new NumericUpDown { DecimalPlaces = 0, Minimum = 1, Maximum = 999, Value = 1 };
        private readonly DataGridView _grid = UiHelper.TaoLuoi();
        private readonly Label _tong = new Label { AutoSize = true, Font = new Font("Microsoft Sans Serif", 9F, FontStyle.Bold) };

        public FrmGhiDichVu(NguoiDung user)
        {
            _presenter = new GhiDichVuPresenter(this, new LuuTruService(), new DichVuService());
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
            nut.Controls.Add(GiaoDien.TaoNut("Ghi dịch vụ", delegate { _presenter.Ghi(); }, true));
            nut.Controls.Add(GiaoDien.TaoNut("Xóa dòng đã chọn", delegate { _presenter.Xoa(); }));
            nhap.Controls.Add(nut);

            var chan = GiaoDien.ThanhNgang(DockStyle.Bottom);
            chan.Controls.Add(_tong);

            Controls.Add(_grid);
            Controls.Add(chan);
            Controls.Add(nhap);
            _phieu.SelectedIndexChanged += delegate { _presenter.TaiChiTiet(); };
            Load += delegate { _presenter.TaiBanDau(); };
            GiaoDien.ApDung(this);
        }

        // ----- IGhiDichVuView -----
        public int? PhieuChon { get { return _phieu.SelectedValue == null ? (int?)null : Convert.ToInt32(_phieu.SelectedValue); } }
        public int? DichVuChon { get { return _dichVu.SelectedValue == null ? (int?)null : Convert.ToInt32(_dichVu.SelectedValue); } }
        public decimal SoLuong { get { return _soLuong.Value; } }

        public int? ChiTietChon
        {
            get { return _grid.CurrentRow == null ? (int?)null : Convert.ToInt32(_grid.CurrentRow.Cells["MaChiTietDichVu"].Value); }
        }

        public void HienPhieu(DataTable phieu)
        {
            _phieu.DisplayMember = "Nhan";
            _phieu.ValueMember = "MaPhieuThue";
            _phieu.DataSource = phieu;
        }

        public void HienDichVu(DataTable dichVu)
        {
            _dichVu.DisplayMember = "Nhan";
            _dichVu.ValueMember = "MaDichVu";
            _dichVu.DataSource = dichVu;
        }

        public void HienChiTiet(DataTable chiTiet)
        {
            if (chiTiet == null) { _grid.DataSource = null; return; }
            UiHelper.GanNguon(_grid, chiTiet, "-MaChiTietDichVu", "-MaDichVu", "TenDichVu|Dịch vụ", "SoLuong|Số lượng|0.##",
                "DonGia|Đơn giá|N0", "ThoiDiemSuDung|Thời điểm|dd/MM/yyyy HH:mm", "ThanhTien|Thành tiền|N0");
        }

        public void DatTong(string noiDung) { _tong.Text = noiDung; }
        public void DatLaiSoLuong() { _soLuong.Value = 1; }

        public void HienThongBao(string tieuDe, string noiDung, MucThongBao muc)
        {
            UiHelper.HienThongBao(this, tieuDe, noiDung, muc);
        }

        public bool XacNhan(string noiDung) { return UiHelper.XacNhan(this, noiDung); }
    }
}
