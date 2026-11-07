using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using QuanLyKhachSan.Presenters;
using QuanLyKhachSan.Services;
using QuanLyKhachSan.Models;

namespace QuanLyKhachSan.Views
{
    /// <summary>Đổi phòng cho khách đang lưu trú (NV08); phòng cũ chuyển sang Dọn dẹp, phòng mới sang Đang ở.</summary>
    public sealed class FrmDoiPhong : Form, IDoiPhongView
    {
        private readonly DoiPhongPresenter _presenter;
        private readonly ComboBox _phieu = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
        private readonly ComboBox _phongMoi = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
        private readonly TextBox _lyDo = new TextBox { MaxLength = QuyTacDoiPhong.LyDoToiDa };
        private readonly DataGridView _grid = UiHelper.TaoLuoi();
        private readonly Label _loiNhac = new Label { AutoSize = true, MaximumSize = new Size(860, 0) };

        public FrmDoiPhong(NguoiDung user)
        {
            _presenter = new DoiPhongPresenter(this, new LuuTruService(), user);
            Text = "Đổi phòng";
            ClientSize = new Size(900, 520);
            MinimumSize = new Size(700, 400);
            StartPosition = FormStartPosition.CenterParent;
            AutoScaleMode = AutoScaleMode.Dpi;

            _phieu.DropDownWidth = 380;
            _phongMoi.DropDownWidth = 340;
            var nhap = GiaoDien.ThanhNgang();
            nhap.Controls.Add(GiaoDien.Nhom("Phòng đang ở", _phieu, 260));
            nhap.Controls.Add(GiaoDien.Nhom("Đổi sang phòng", _phongMoi, 240));
            nhap.Controls.Add(GiaoDien.Nhom("Lý do (tùy chọn)", _lyDo, 220));
            var nut = new FlowLayoutPanel { AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, WrapContents = false, Margin = new Padding(0, 16, 0, 6) };
            nut.Controls.Add(GiaoDien.TaoNut("Đổi phòng", delegate { _presenter.Doi(); }, true));
            nhap.Controls.Add(nut);

            var chan = GiaoDien.ThanhNgang(DockStyle.Bottom);
            chan.Controls.Add(_loiNhac);

            Controls.Add(_grid);
            Controls.Add(chan);
            Controls.Add(nhap);
            _phieu.SelectedIndexChanged += delegate { _presenter.TaiPhongMoi(); _presenter.TaiLichSu(); };
            _phongMoi.SelectedIndexChanged += delegate { _presenter.CapNhatLoiNhac(); };
            Load += delegate { _presenter.TaiPhieu(); };
            GiaoDien.ApDung(this);
        }

        // ----- IDoiPhongView -----
        public int? PhieuChon { get { return _phieu.SelectedValue == null ? (int?)null : Convert.ToInt32(_phieu.SelectedValue); } }
        public int? PhongMoiChon { get { return _phongMoi.SelectedValue == null ? (int?)null : Convert.ToInt32(_phongMoi.SelectedValue); } }
        public string LyDo { get { return _lyDo.Text; } }

        public void HienPhieu(DataTable phieu)
        {
            _phieu.DisplayMember = "Nhan";
            _phieu.ValueMember = "MaPhieuThue";
            _phieu.DataSource = phieu;
        }

        public void HienPhongMoi(DataTable phong)
        {
            _phongMoi.DisplayMember = "Nhan";
            _phongMoi.ValueMember = "MaPhong";
            _phongMoi.DataSource = phong;
        }

        public void HienLichSu(DataTable lichSu)
        {
            if (lichSu == null) { _grid.DataSource = null; return; }
            UiHelper.GanNguon(_grid, lichSu, "ThoiDiemDoi|Thời điểm|dd/MM/yyyy HH:mm", "PhongCu|Từ phòng", "PhongMoi|Sang phòng",
                "GiaCu|Giá cũ|N0", "GiaMoi|Giá mới|N0", "LyDo|Lý do", "NguoiThucHien|Người thực hiện");
        }

        public void DatLoiNhac(string noiDung) { _loiNhac.Text = noiDung; }
        public void XoaLyDo() { _lyDo.Clear(); }

        public void HienThongBao(string tieuDe, string noiDung, MucThongBao muc)
        {
            UiHelper.HienThongBao(this, tieuDe, noiDung, muc);
        }

        public bool XacNhan(string noiDung) { return UiHelper.XacNhan(this, noiDung); }
    }
}
