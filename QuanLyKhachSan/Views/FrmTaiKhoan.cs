using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using QuanLyKhachSan.Presenters;
using QuanLyKhachSan.Services;
using QuanLyKhachSan.Models;

namespace QuanLyKhachSan.Views
{
    /// <summary>Quản lý tài khoản nhân viên (chỉ Quản lý): tạo, đặt lại mật khẩu, khóa.</summary>
    public sealed class FrmTaiKhoan : Form, ITaiKhoanView
    {
        private readonly TaiKhoanPresenter _presenter;
        private readonly DataGridView _grid = UiHelper.TaoLuoi();
        private readonly TextBox _login = new TextBox();
        private readonly TextBox _name = new TextBox();
        private readonly TextBox _password = new TextBox { UseSystemPasswordChar = true };
        private readonly ComboBox _role = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };

        public FrmTaiKhoan(NguoiDung user)
        {
            _presenter = new TaiKhoanPresenter(this, new NguoiDungService(), user);
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
            nut.Controls.Add(GiaoDien.TaoNut("Tạo tài khoản", delegate { _presenter.Tao(); }, true));
            nut.Controls.Add(GiaoDien.TaoNut("Đặt lại mật khẩu", delegate { _presenter.DatLaiMatKhau(); }));
            nut.Controls.Add(GiaoDien.TaoNut("Khóa tài khoản", delegate { _presenter.Khoa(); }));
            nut.Controls.Add(new Label { AutoSize = true, ForeColor = GiaoDien.Phu, Margin = new Padding(8, 6, 0, 0), Text = "Mật khẩu mạnh: tối thiểu 8 ký tự, có chữ hoa, chữ thường và chữ số." });

            Controls.Add(_grid);
            Controls.Add(nut);
            Controls.Add(top);
            Load += delegate { _presenter.TaiDuLieu(); };
            GiaoDien.ApDung(this);
        }

        // ----- ITaiKhoanView -----
        public string TenDangNhap { get { return _login.Text; } }
        public string HoTen { get { return _name.Text; } }
        public string MatKhau { get { return _password.Text; } }
        public string VaiTro { get { return Convert.ToString(_role.SelectedItem); } }

        public int MaDangChon
        {
            get { return _grid.CurrentRow == null ? 0 : Convert.ToInt32(_grid.CurrentRow.Cells["MaNguoiDung"].Value); }
        }

        public void HienDanhSach(DataTable bang)
        {
            UiHelper.GanNguon(_grid, bang, "-MaNguoiDung", "TenDangNhap|Tên đăng nhập", "HoTen|Họ tên", "VaiTro|Vai trò",
                "HoatDong|Hoạt động", "NgayTao|Ngày tạo|dd/MM/yyyy HH:mm");
        }

        public void XoaFormTao()
        {
            _password.Clear();
            _login.Clear();
            _name.Clear();
        }

        public void XoaMatKhau() { _password.Clear(); }

        public void HienThongBao(string tieuDe, string noiDung, MucThongBao muc)
        {
            UiHelper.HienThongBao(this, tieuDe, noiDung, muc);
        }

        public bool XacNhan(string noiDung) { return UiHelper.XacNhan(this, noiDung); }
    }
}
