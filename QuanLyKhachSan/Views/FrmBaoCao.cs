using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using QuanLyKhachSan.Presenters;
using QuanLyKhachSan.Services;

namespace QuanLyKhachSan.Views
{
    /// <summary>Báo cáo doanh thu (BC01–BC03) và công suất phòng (BC04), có biểu đồ, in và xuất CSV.</summary>
    public sealed class FrmBaoCao : Form, IBaoCaoView
    {
        private readonly BaoCaoPresenter _presenter;
        private readonly TabControl _tab = new TabControl { Dock = DockStyle.Fill };

        // Doanh thu
        private readonly DateTimePicker _dtTu = new DateTimePicker { Format = DateTimePickerFormat.Short };
        private readonly DateTimePicker _dtDen = new DateTimePicker { Format = DateTimePickerFormat.Short };
        private readonly ComboBox _kieu = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
        private readonly ChartPanel _dtChart = new ChartPanel { Dock = DockStyle.Fill, Kieu = KieuBieuDo.Cot, DonVi = " đ" };
        private readonly DataGridView _dtGrid = UiHelper.TaoLuoi();
        private readonly Label _dtTong = new Label { AutoSize = true, Font = new Font("Microsoft Sans Serif", 9F, FontStyle.Bold) };
        private DataTable _dtData;

        // Công suất
        private readonly DateTimePicker _csTu = new DateTimePicker { Format = DateTimePickerFormat.Short };
        private readonly DateTimePicker _csDen = new DateTimePicker { Format = DateTimePickerFormat.Short };
        private readonly ChartPanel _csChart = new ChartPanel { Dock = DockStyle.Fill, Kieu = KieuBieuDo.Duong, DonVi = "%", TrucYToiDa = 100 };
        private readonly DataGridView _csGrid = UiHelper.TaoLuoi();
        private readonly Label _csTong = new Label { AutoSize = true, Font = new Font("Microsoft Sans Serif", 9F, FontStyle.Bold) };
        private DataTable _csData;

        public FrmBaoCao(int tabBanDau = 0)
        {
            _presenter = new BaoCaoPresenter(this, new BaoCaoService());
            Text = "Báo cáo";
            ClientSize = new Size(1040, 700);
            MinimumSize = new Size(780, 520);
            StartPosition = FormStartPosition.CenterParent;
            AutoScaleMode = AutoScaleMode.Dpi;

            var trangDt = new TabPage("Doanh thu") { BackColor = GiaoDien.Nen };
            var trangCs = new TabPage("Công suất phòng") { BackColor = GiaoDien.Nen };
            DungDoanhThu(trangDt);
            DungCongSuat(trangCs);
            _tab.TabPages.Add(trangDt);
            _tab.TabPages.Add(trangCs);
            _tab.SelectedIndex = Math.Max(0, Math.Min(1, tabBanDau));
            Controls.Add(_tab);
            Load += delegate { if (_tab.SelectedIndex == 0) _presenter.TaiDoanhThu(); else _presenter.TaiCongSuat(); };
            _tab.SelectedIndexChanged += delegate
            {
                if (_tab.SelectedIndex == 1 && _csData == null) _presenter.TaiCongSuat();
                if (_tab.SelectedIndex == 0 && _dtData == null) _presenter.TaiDoanhThu();
            };
            GiaoDien.ApDung(this);
        }

        private static Control Nut(params Button[] cacNut)
        {
            var p = new FlowLayoutPanel { AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, WrapContents = false, Margin = new Padding(0, 16, 0, 6) };
            p.Controls.AddRange(cacNut);
            return p;
        }

        private void DungDoanhThu(TabPage trang)
        {
            _dtTu.Value = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
            _dtDen.Value = DateTime.Today;
            _kieu.Items.AddRange(new object[] { "Theo ngày", "Theo tháng", "Theo loại phòng" });
            _kieu.SelectedIndex = 0;

            var thanh = GiaoDien.ThanhNgang();
            thanh.Controls.Add(GiaoDien.Nhom("Từ ngày", _dtTu, 120));
            thanh.Controls.Add(GiaoDien.Nhom("Đến ngày", _dtDen, 120));
            thanh.Controls.Add(GiaoDien.Nhom("Gom theo", _kieu, 140));
            thanh.Controls.Add(Nut(GiaoDien.TaoNut("Thống kê", delegate { _presenter.TaiDoanhThu(); }, true),
                GiaoDien.TaoNut("In báo cáo", delegate { InDoanhThu(); }),
                GiaoDien.TaoNut("Xuất CSV", delegate { UiHelper.XuatCsv(this, _dtGrid, "DoanhThu"); })));

            var chan = GiaoDien.ThanhNgang(DockStyle.Bottom);
            chan.Controls.Add(_dtTong);

            var giua = new SplitContainer { Dock = DockStyle.Fill, Orientation = Orientation.Horizontal, BackColor = GiaoDien.Vien };
            giua.Panel1.Controls.Add(_dtChart);
            giua.Panel2.Controls.Add(_dtGrid);
            giua.Panel1MinSize = 120;
            giua.Panel2MinSize = 90;
            trang.Controls.Add(giua);
            trang.Controls.Add(chan);
            trang.Controls.Add(thanh);
            trang.HandleCreated += delegate { if (giua.Height > 300) giua.SplitterDistance = Math.Max(120, giua.Height * 55 / 100); };
        }

        private void DungCongSuat(TabPage trang)
        {
            _csTu.Value = DateTime.Today.AddDays(-29);
            _csDen.Value = DateTime.Today;
            var thanh = GiaoDien.ThanhNgang();
            thanh.Controls.Add(GiaoDien.Nhom("Từ ngày", _csTu, 120));
            thanh.Controls.Add(GiaoDien.Nhom("Đến ngày", _csDen, 120));
            thanh.Controls.Add(Nut(GiaoDien.TaoNut("Thống kê", delegate { _presenter.TaiCongSuat(); }, true),
                GiaoDien.TaoNut("In báo cáo", delegate { InCongSuat(); }),
                GiaoDien.TaoNut("Xuất CSV", delegate { UiHelper.XuatCsv(this, _csGrid, "CongSuat"); })));
            var chan = GiaoDien.ThanhNgang(DockStyle.Bottom);
            chan.Controls.Add(_csTong);

            var giua = new SplitContainer { Dock = DockStyle.Fill, Orientation = Orientation.Horizontal, BackColor = GiaoDien.Vien };
            giua.Panel1.Controls.Add(_csChart);
            giua.Panel2.Controls.Add(_csGrid);
            giua.Panel1MinSize = 120;
            giua.Panel2MinSize = 90;
            trang.Controls.Add(giua);
            trang.Controls.Add(chan);
            trang.Controls.Add(thanh);
            trang.HandleCreated += delegate { if (giua.Height > 300) giua.SplitterDistance = Math.Max(120, giua.Height * 55 / 100); };
        }

        // ----- IBaoCaoView -----
        public DateTime DoanhThuTu { get { return _dtTu.Value; } }
        public DateTime DoanhThuDen { get { return _dtDen.Value; } }
        public DateTime CongSuatTu { get { return _csTu.Value; } }
        public DateTime CongSuatDen { get { return _csDen.Value; } }

        public KieuGomDoanhThu KieuGom
        {
            get
            {
                switch (_kieu.SelectedIndex)
                {
                    case 1: return KieuGomDoanhThu.Thang;
                    case 2: return KieuGomDoanhThu.LoaiPhong;
                    default: return KieuGomDoanhThu.Ngay;
                }
            }
        }

        public void HienDoanhThu(DataTable bang, IList<string> nhan, IList<double> giaTri, string tong)
        {
            _dtData = bang;
            UiHelper.GanNguon(_dtGrid, bang, "Nhan|" + _kieu.SelectedItem, "SoHoaDon|Số hóa đơn|N0", "DoanhThu|Doanh thu (đ)|N0");
            _dtChart.TieuDe = "Doanh thu " + ((string)_kieu.SelectedItem).ToLower();
            _dtChart.Kieu = KieuGom == KieuGomDoanhThu.Ngay ? KieuBieuDo.Duong : KieuBieuDo.Cot;
            _dtChart.DatDuLieu(new List<string>(nhan), new List<double>(giaTri));
            _dtTong.Text = tong;
        }

        public void HienCongSuat(DataTable bang, IList<string> nhan, IList<double> giaTri, string tong)
        {
            _csData = bang;
            UiHelper.GanNguon(_csGrid, bang, "Ngay|Ngày|dd/MM/yyyy", "SoPhongCoKhach|Phòng có khách|0", "TongPhong|Tổng phòng|0", "TyLe|Tỷ lệ lấp đầy (%)|0.##");
            _csChart.TieuDe = "Công suất phòng theo ngày";
            _csChart.DatDuLieu(new List<string>(nhan), new List<double>(giaTri));
            _csTong.Text = tong;
        }

        public void HienThongBao(string tieuDe, string noiDung, MucThongBao muc)
        {
            UiHelper.HienThongBao(this, tieuDe, noiDung, muc);
        }

        public bool XacNhan(string noiDung) { return UiHelper.XacNhan(this, noiDung); }

        private void InDoanhThu()
        {
            if (_dtData == null || _dtData.Rows.Count == 0) { MessageBox.Show(this, "Chưa có dữ liệu để in.", "In báo cáo"); return; }
            var dong = new List<string[]>();
            foreach (DataRow r in _dtData.Rows)
                dong.Add(new[] { Convert.ToString(r["Nhan"]), Convert.ToInt64(r["SoHoaDon"]).ToString("N0"), Convert.ToDecimal(r["DoanhThu"]).ToString("N0") });
            BaoCaoPrinter.XemTruoc(this, "BÁO CÁO DOANH THU",
                new[] { "Từ " + _dtTu.Value.ToString("dd/MM/yyyy") + " đến " + _dtDen.Value.ToString("dd/MM/yyyy") + " · " + _kieu.SelectedItem, _dtTong.Text },
                new[] { (string)_kieu.SelectedItem, "Số hóa đơn", "Doanh thu (đ)" }, new[] { false, true, true }, dong, _dtChart.VeRaBitmap(900, 360));
        }

        private void InCongSuat()
        {
            if (_csData == null || _csData.Rows.Count == 0) { MessageBox.Show(this, "Chưa có dữ liệu để in.", "In báo cáo"); return; }
            var dong = new List<string[]>();
            foreach (DataRow r in _csData.Rows)
                dong.Add(new[] { Convert.ToDateTime(r["Ngay"]).ToString("dd/MM/yyyy"), Convert.ToString(r["SoPhongCoKhach"]), Convert.ToString(r["TongPhong"]), Convert.ToDecimal(r["TyLe"]).ToString("0.##") });
            BaoCaoPrinter.XemTruoc(this, "BÁO CÁO CÔNG SUẤT PHÒNG",
                new[] { "Từ " + _csTu.Value.ToString("dd/MM/yyyy") + " đến " + _csDen.Value.ToString("dd/MM/yyyy"), _csTong.Text },
                new[] { "Ngày", "Phòng có khách", "Tổng phòng", "Tỷ lệ (%)" }, new[] { false, true, true, true }, dong, _csChart.VeRaBitmap(900, 360));
        }
    }
}
