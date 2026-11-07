using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using QuanLyKhachSan.Presenters;
using QuanLyKhachSan.Services;

namespace QuanLyKhachSan.Views
{
    /// <summary>Tra cứu, in hóa đơn và xuất CSV (NV07).</summary>
    public sealed class FrmHoaDon : Form, IHoaDonView
    {
        private readonly HoaDonPresenter _presenter;
        private readonly DateTimePicker _tu = new DateTimePicker { Format = DateTimePickerFormat.Short };
        private readonly DateTimePicker _den = new DateTimePicker { Format = DateTimePickerFormat.Short };
        private readonly TextBox _tuKhoa = new TextBox();
        private readonly DataGridView _grid = UiHelper.TaoLuoi();
        private readonly Label _tong = new Label { AutoSize = true, Font = new Font("Microsoft Sans Serif", 9F, FontStyle.Bold) };

        public FrmHoaDon()
        {
            _presenter = new HoaDonPresenter(this, new HoaDonService(), new LuuTruService());
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
            nut.Controls.Add(GiaoDien.TaoNut("Tìm", delegate { _presenter.Tai(); }, true));
            nut.Controls.Add(GiaoDien.TaoNut("In hóa đơn", delegate { _presenter.In(); }));
            nut.Controls.Add(GiaoDien.TaoNut("Xuất CSV", delegate { UiHelper.XuatCsv(this, _grid, "HoaDon"); }));
            thanh.Controls.Add(nut);

            var chan = GiaoDien.ThanhNgang(DockStyle.Bottom);
            chan.Controls.Add(_tong);

            Controls.Add(_grid);
            Controls.Add(chan);
            Controls.Add(thanh);
            AcceptButton = null;
            _tuKhoa.KeyDown += delegate (object s, KeyEventArgs e) { if (e.KeyCode == Keys.Enter) { e.SuppressKeyPress = true; _presenter.Tai(); } };
            Load += delegate { _presenter.Tai(); };
            GiaoDien.ApDung(this);
        }

        // ----- IHoaDonView -----
        public DateTime TuNgay { get { return _tu.Value; } }
        public DateTime DenNgay { get { return _den.Value; } }
        public string TuKhoa { get { return _tuKhoa.Text; } }

        public DataRow HoaDonChon
        {
            get
            {
                if (_grid.CurrentRow == null) return null;
                var dong = _grid.CurrentRow.DataBoundItem as DataRowView;
                return dong == null ? null : dong.Row;
            }
        }

        public void HienDanhSach(DataTable t, string tong)
        {
            UiHelper.GanNguon(_grid, t, "MaHoaDon|Mã HĐ", "-MaPhieuThue", "NgayLap|Ngày lập|dd/MM/yyyy HH:mm", "HoTen|Khách hàng", "SoPhong|Phòng",
                "TienPhong|Tiền phòng|N0", "TienDichVu|Dịch vụ|N0", "PhuThu|Phụ thu|N0", "GiamGia|Giảm giá|N0", "TienCoc|Cọc|N0",
                "TongThanhToan|Thanh toán|N0", "HinhThucThanhToan|Hình thức");
            _tong.Text = tong;
        }

        public void MoBanIn(DataRow hoaDon, DataTable dichVu)
        {
            HoaDonPrinter.XemTruoc(this, hoaDon, dichVu);
        }

        public void HienThongBao(string tieuDe, string noiDung, MucThongBao muc)
        {
            UiHelper.HienThongBao(this, tieuDe, noiDung, muc);
        }

        public bool XacNhan(string noiDung) { return UiHelper.XacNhan(this, noiDung); }
    }
}
