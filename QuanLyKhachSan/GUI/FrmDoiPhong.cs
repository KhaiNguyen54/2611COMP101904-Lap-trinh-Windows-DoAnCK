using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using QuanLyKhachSan.BLL;
using QuanLyKhachSan.DTO;

namespace QuanLyKhachSan.GUI
{
    /// <summary>Đổi phòng cho khách đang lưu trú (NV08); phòng cũ chuyển sang Dọn dẹp, phòng mới sang Đang ở.</summary>
    public sealed class FrmDoiPhong : Form
    {
        private readonly NguoiDung _user;
        private readonly LuuTruBLL _bll = new LuuTruBLL();
        private readonly ComboBox _phieu = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
        private readonly ComboBox _phongMoi = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
        private readonly TextBox _lyDo = new TextBox { MaxLength = QuyTacDoiPhong.LyDoToiDa };
        private readonly DataGridView _grid = UiHelper.TaoLuoi();
        private readonly Label _loiNhac = new Label { AutoSize = true, MaximumSize = new Size(860, 0) };
        private DataTable _dsPhieu;

        public FrmDoiPhong(NguoiDung user)
        {
            _user = user;
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
            nut.Controls.Add(GiaoDien.TaoNut("Đổi phòng", Doi_Click, true));
            nhap.Controls.Add(nut);

            var chan = GiaoDien.ThanhNgang(DockStyle.Bottom);
            chan.Controls.Add(_loiNhac);

            Controls.Add(_grid);
            Controls.Add(chan);
            Controls.Add(nhap);
            _phieu.SelectedIndexChanged += delegate { TaiPhongMoi(); TaiLichSu(); };
            _phongMoi.SelectedIndexChanged += delegate { CapNhatLoiNhac(); };
            Load += delegate { TaiPhieu(); };
            GiaoDien.ApDung(this);
        }

        private DataRow PhieuChon()
        {
            var v = _phieu.SelectedItem as DataRowView;
            return v == null ? null : v.Row;
        }

        private void TaiPhieu()
        {
            try
            {
                _dsPhieu = _bll.LayPhieuDangO();
                _dsPhieu.Columns.Add("Nhan", typeof(string));
                foreach (DataRow r in _dsPhieu.Rows)
                    r["Nhan"] = string.Format("Phòng {0} · {1} · đến {2:dd/MM}", r["SoPhong"], r["HoTen"], r["NgayDiDuKien"]);
                _phieu.DisplayMember = "Nhan";
                _phieu.ValueMember = "MaPhieuThue";
                _phieu.DataSource = _dsPhieu;
                if (_dsPhieu.Rows.Count == 0) _loiNhac.Text = "Chưa có phòng nào đang có khách.";
            }
            catch (Exception ex) { UiHelper.ThongBaoLoi(this, "Không tải được phòng đang ở", ex); }
        }

        private void TaiPhongMoi()
        {
            DataRow p = PhieuChon();
            if (p == null) { _phongMoi.DataSource = null; return; }
            try
            {
                DataTable t = _bll.LayPhongCoTheDoi(Convert.ToInt32(p["MaPhong"]), Convert.ToDateTime(p["NgayDiDuKien"]));
                t.Columns.Add("Nhan", typeof(string));
                foreach (DataRow r in t.Rows)
                    r["Nhan"] = string.Format("{0} · tầng {1} · {2} · {3:N0} đ", r["SoPhong"], r["Tang"], r["TenLoaiPhong"], r["GiaMoiDem"]);
                _phongMoi.DisplayMember = "Nhan";
                _phongMoi.ValueMember = "MaPhong";
                _phongMoi.DataSource = t;
                if (t.Rows.Count == 0) _loiNhac.Text = "Không có phòng trống trọn thời gian còn lại của kỳ lưu trú.";
                else CapNhatLoiNhac();
            }
            catch (Exception ex) { UiHelper.ThongBaoLoi(this, "Không tải được danh sách phòng trống", ex); }
        }

        private void CapNhatLoiNhac()
        {
            DataRow p = PhieuChon();
            var m = _phongMoi.SelectedItem as DataRowView;
            if (p == null || m == null) return;
            string nhac = QuyTacDoiPhong.LoiNhacGia(Convert.ToDecimal(p["GiaPhongMoiDem"]), Convert.ToDecimal(m.Row["GiaMoiDem"]));
            _loiNhac.Text = nhac ?? "Phòng mới cùng mức giá; dịch vụ đã ghi vẫn giữ nguyên trong phiếu thuê.";
        }

        private void TaiLichSu()
        {
            DataRow p = PhieuChon();
            if (p == null) { _grid.DataSource = null; return; }
            try
            {
                DataTable t = _bll.LayLichSuDoiPhong(Convert.ToInt32(p["MaPhieuThue"]));
                UiHelper.GanNguon(_grid, t, "ThoiDiemDoi|Thời điểm|dd/MM/yyyy HH:mm", "PhongCu|Từ phòng", "PhongMoi|Sang phòng",
                    "GiaCu|Giá cũ|N0", "GiaMoi|Giá mới|N0", "LyDo|Lý do", "NguoiThucHien|Người thực hiện");
            }
            catch (Exception ex) { UiHelper.ThongBaoLoi(this, "Không tải được lịch sử đổi phòng", ex); }
        }

        private void Doi_Click(object sender, EventArgs e)
        {
            DataRow p = PhieuChon();
            if (p == null || _phongMoi.SelectedValue == null)
            {
                MessageBox.Show(this, "Chọn phòng đang ở và phòng mới.", "Thiếu dữ liệu", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            string cauHoi = string.Format("Đổi khách {0} từ phòng {1} sang phòng {2}?\nPhòng cũ sẽ chuyển sang Dọn dẹp.",
                p["HoTen"], p["SoPhong"], ((DataRowView)_phongMoi.SelectedItem).Row["SoPhong"]);
            if (!UiHelper.XacNhan(this, cauHoi)) return;
            try
            {
                _bll.DoiPhong(Convert.ToInt32(p["MaPhieuThue"]), Convert.ToInt32(_phongMoi.SelectedValue), _user.MaNguoiDung, _lyDo.Text);
                UiHelper.ThongBaoThanhCong(this, "Đã đổi phòng.");
                _lyDo.Clear();
                TaiPhieu();
            }
            catch (Exception ex) { UiHelper.ThongBaoLoi(this, "Không thể đổi phòng", ex); }
        }
    }
}
