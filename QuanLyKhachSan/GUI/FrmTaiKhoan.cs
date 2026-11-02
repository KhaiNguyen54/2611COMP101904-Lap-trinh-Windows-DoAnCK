using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using QuanLyKhachSan.BLL;
using QuanLyKhachSan.DTO;

namespace QuanLyKhachSan.GUI
{
    /// <summary>Quản lý tài khoản nhân viên (chỉ Quản lý): tạo, đặt lại mật khẩu, khóa.</summary>
    public sealed class FrmTaiKhoan : Form
    {
        private readonly NguoiDung _user;
        private readonly NguoiDungBLL _bll = new NguoiDungBLL();
        private readonly DataGridView _grid = UiHelper.TaoLuoi();
        private readonly TextBox _login = new TextBox();
        private readonly TextBox _name = new TextBox();
        private readonly TextBox _password = new TextBox { UseSystemPasswordChar = true };
        private readonly ComboBox _role = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };

        public FrmTaiKhoan(NguoiDung user)
        {
            _user = user;
            Text = "Quản lý tài khoản";
            ClientSize = new Size(960, 540);
            MinimumSize = new Size(700, 400);
            StartPosition = FormStartPosition.CenterParent;
            AutoScaleMode = AutoScaleMode.Dpi;

            _role.Items.AddRange(new object[] { VaiTroNguoiDung.LeTan, VaiTroNguoiDung.QuanLy });
            _role.SelectedIndex = 0;

            var top = GiaoDien.ThanhNgang();
            top.Controls.Add(GiaoDien.Nhom("Tên đăng nhập", _login, 150));
            top.Controls.Add(GiaoDien.Nhom("Họ tên", _name, 180));
            top.Controls.Add(GiaoDien.Nhom("Mật khẩu (đặt mới / đặt lại)", _password, 190));
            top.Controls.Add(GiaoDien.Nhom("Vai trò", _role, 110));

            var nut = GiaoDien.ThanhNgang();
            nut.Controls.Add(GiaoDien.TaoNut("Tạo tài khoản", Tao_Click, true));
            nut.Controls.Add(GiaoDien.TaoNut("Đặt lại mật khẩu", DatLai_Click));
            nut.Controls.Add(GiaoDien.TaoNut("Khóa tài khoản", Khoa_Click));
            nut.Controls.Add(new Label { AutoSize = true, ForeColor = GiaoDien.Phu, Margin = new Padding(8, 6, 0, 0), Text = "Mật khẩu mạnh: tối thiểu 8 ký tự, có chữ hoa, chữ thường và chữ số." });

            Controls.Add(_grid);
            Controls.Add(nut);
            Controls.Add(top);
            Load += delegate { TaiDuLieu(); };
            GiaoDien.ApDung(this);
        }

        private void TaiDuLieu()
        {
            try
            {
                UiHelper.GanNguon(_grid, _bll.LayDanhSach(_user), "-MaNguoiDung", "TenDangNhap|Tên đăng nhập", "HoTen|Họ tên", "VaiTro|Vai trò",
                    "HoatDong|Hoạt động", "NgayTao|Ngày tạo|dd/MM/yyyy HH:mm");
            }
            catch (Exception ex) { UiHelper.ThongBaoLoi(this, "Không tải được danh sách tài khoản", ex); }
        }

        private int MaChon()
        {
            return _grid.CurrentRow == null ? 0 : Convert.ToInt32(_grid.CurrentRow.Cells["MaNguoiDung"].Value);
        }

        private void Tao_Click(object sender, EventArgs e)
        {
            try
            {
                _bll.TaoTaiKhoan(_user, _login.Text, _name.Text, Convert.ToString(_role.SelectedItem), _password.Text);
                _password.Clear();
                _login.Clear();
                _name.Clear();
                TaiDuLieu();
                UiHelper.ThongBaoThanhCong(this, "Đã tạo tài khoản.");
            }
            catch (Exception ex) { UiHelper.ThongBaoLoi(this, "Không thể tạo tài khoản", ex); }
        }

        private void DatLai_Click(object sender, EventArgs e)
        {
            try
            {
                _bll.DatLaiMatKhau(_user, MaChon(), _password.Text);
                _password.Clear();
                UiHelper.ThongBaoThanhCong(this, "Đã đặt lại mật khẩu.");
            }
            catch (Exception ex) { UiHelper.ThongBaoLoi(this, "Không thể đặt lại mật khẩu", ex); }
        }

        private void Khoa_Click(object sender, EventArgs e)
        {
            try
            {
                if (MaChon() == 0 || !UiHelper.XacNhan(this, "Khóa tài khoản đang chọn?")) return;
                _bll.KhoaTaiKhoan(_user, MaChon());
                TaiDuLieu();
            }
            catch (Exception ex) { UiHelper.ThongBaoLoi(this, "Không thể khóa tài khoản", ex); }
        }
    }
}
