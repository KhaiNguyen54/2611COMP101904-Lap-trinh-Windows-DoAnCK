using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using QuanLyKhachSan.Presenters;
using QuanLyKhachSan.Services;

namespace QuanLyKhachSan.Views
{
    /// <summary>Danh sách khách đang lưu trú: xem, lọc nhanh, in và xuất CSV (UC19).</summary>
    public sealed class FrmKhachDangO : Form, IKhachDangOView
    {
        private readonly DataGridView _grid = UiHelper.TaoLuoi();
        private readonly TextBox _loc = new TextBox();
        private readonly Label _dem = new Label { AutoSize = true, Font = new Font("Microsoft Sans Serif", 9F, FontStyle.Bold) };
        private readonly KhachDangOPresenter _presenter;

        public FrmKhachDangO()
        {
            _presenter = new KhachDangOPresenter(this, new LuuTruService());
            Text = "Khách đang ở";
            ClientSize = new Size(980, 540);
            MinimumSize = new Size(700, 380);
            StartPosition = FormStartPosition.CenterParent;
            AutoScaleMode = AutoScaleMode.Dpi;

            var thanh = GiaoDien.ThanhNgang();
            thanh.Controls.Add(GiaoDien.Nhom("Lọc theo tên / phòng", _loc, 220));
            var nut = new FlowLayoutPanel { AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, WrapContents = false, Margin = new Padding(0, 16, 0, 6) };
            nut.Controls.Add(GiaoDien.TaoNut("Làm mới", delegate { _presenter.Tai(); }, true));
            nut.Controls.Add(GiaoDien.TaoNut("In danh sách", In_Click));
            nut.Controls.Add(GiaoDien.TaoNut("Xuất CSV", delegate { UiHelper.XuatCsv(this, _grid, "KhachDangO"); }));
            thanh.Controls.Add(nut);
            var chan = GiaoDien.ThanhNgang(DockStyle.Bottom);
            chan.Controls.Add(_dem);

            Controls.Add(_grid);
            Controls.Add(chan);
            Controls.Add(thanh);
            _loc.TextChanged += delegate { _presenter.Loc(); };
            Load += delegate { _presenter.Tai(); };
            GiaoDien.ApDung(this);
        }

        // ----- IKhachDangOView -----
        public string TuKhoaLoc { get { return _loc.Text; } }

        public void HienDanhSach(DataTable bang)
        {
            UiHelper.GanNguon(_grid, bang, "-MaPhieuThue", "SoPhong|Phòng", "Tang|Tầng", "TenLoaiPhong|Loại phòng", "HoTen|Khách hàng",
                "SoGiayTo|Giấy tờ", "SoDienThoai|Điện thoại", "NgayNhan|Ngày nhận|dd/MM/yyyy HH:mm", "NgayDiDuKien|Đi dự kiến|dd/MM/yyyy", "SoDemDaO|Số đêm đã ở|0");
        }

        public void HienSoLuong(string noiDung) { _dem.Text = noiDung; }

        public void HienThongBao(string tieuDe, string noiDung, MucThongBao muc)
        {
            UiHelper.HienThongBao(this, tieuDe, noiDung, muc);
        }

        public bool XacNhan(string noiDung) { return UiHelper.XacNhan(this, noiDung); }

        private void In_Click(object sender, EventArgs e)
        {
            var cot = new List<string>();
            var canPhai = new List<bool>();
            var idx = new List<int>();
            foreach (DataGridViewColumn c in _grid.Columns)
            {
                if (!c.Visible) continue;
                cot.Add(c.HeaderText);
                canPhai.Add(false);
                idx.Add(c.Index);
            }
            var dong = new List<string[]>();
            foreach (DataGridViewRow r in _grid.Rows)
            {
                var o = new string[idx.Count];
                for (int i = 0; i < idx.Count; i++) o[i] = UiHelper.DinhDangO(r.Cells[idx[i]]);
                dong.Add(o);
            }
            BaoCaoPrinter.XemTruoc(this, "DANH SÁCH KHÁCH ĐANG Ở", new[] { "Ngày in: " + DateTime.Now.ToString("dd/MM/yyyy HH:mm"), _dem.Text },
                cot.ToArray(), canPhai.ToArray(), dong, null);
        }
    }
}
