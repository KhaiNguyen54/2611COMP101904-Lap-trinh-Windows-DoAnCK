using System;
using System.Data;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using QuanLyKhachSan.BLL;
using QuanLyKhachSan.DTO;

namespace QuanLyKhachSan.GUI
{
    /// <summary>
    /// Trang tổng quan lễ tân và điều hướng tới các nghiệp vụ quản lý khách sạn.
    /// </summary>
    public class FrmMain : Form
    {
        private readonly MenuStrip _menu = new MenuStrip();
        private readonly StatusStrip _thanhTrangThai = new StatusStrip();
        private readonly ToolStripStatusLabel _lblNguoiDung = new ToolStripStatusLabel();
        private readonly ToolStripStatusLabel _lblCapNhat = new ToolStripStatusLabel { Spring = true, TextAlign = ContentAlignment.MiddleRight };
        private readonly NguoiDung _nguoiDung;
        private readonly UcTrangThaiPhong _bangPhong;
        private readonly KhungAnhKhachSan _anhKhachSan = new KhungAnhKhachSan();
        private readonly Label[] _soPhong = new Label[4];
        private readonly Label _homNay = new Label { AutoSize = true, MaximumSize = new Size(0, 0) };
        private readonly LinkLabel _lamMoi = new LinkLabel { Text = "Làm mới", AutoSize = true, Anchor = AnchorStyles.Right, Margin = new Padding(8, 0, 0, 0) };
        private bool _dangTai;

        /// <summary>Đặt true khi người dùng chọn Hệ thống → Đăng xuất để Program mở lại màn hình đăng nhập.</summary>
        public bool DangXuat { get; private set; }

        [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
        public Image AnhMinhHoaKhachSan
        {
            get { return _anhKhachSan.BackgroundImage; }
            set { _anhKhachSan.BackgroundImage = value; }
        }

        public FrmMain(NguoiDung nguoiDung)
        {
            _nguoiDung = nguoiDung ?? throw new ArgumentNullException(nameof(nguoiDung));
            _bangPhong = new UcTrangThaiPhong(nguoiDung);
            KhoiTaoGiaoDien();
            NapAnhKhachSan();
            FormClosed += delegate
            {
                if (AnhMinhHoaKhachSan != null)
                {
                    AnhMinhHoaKhachSan.Dispose();
                    AnhMinhHoaKhachSan = null;
                }
            };
            _bangPhong.TrangThaiPhongThayDoi += async delegate { await TaiTongQuanAsync(); };
            GiaoDien.ApDung(this);
        }

        private void NapAnhKhachSan()
        {
            string duongDan = System.IO.Path.Combine(AppContext.BaseDirectory, "Resources", "hotel.png");
            if (!System.IO.File.Exists(duongDan)) return;

            using (Image anhGoc = Image.FromFile(duongDan))
                AnhMinhHoaKhachSan = new Bitmap(anhGoc);
        }

        private bool LaQuanLy { get { return _nguoiDung.VaiTro == VaiTroNguoiDung.QuanLy; } }

        private void KhoiTaoGiaoDien()
        {
            Text = "Autumn Hotel | Quản lý khách sạn";
            ClientSize = new Size(1180, 720);
            WindowState = FormWindowState.Maximized;
            StartPosition = FormStartPosition.CenterScreen;
            MinimumSize = new Size(900, 600);
            AutoScaleMode = AutoScaleMode.Dpi;
            KeyPreview = true;

            DungMenu();

            _lblNguoiDung.Text = "Đăng nhập: " + _nguoiDung.HoTen + " (" + (LaQuanLy ? "Quản lý" : "Lễ tân") + ")";
            _thanhTrangThai.Items.Add(_lblNguoiDung);
            _thanhTrangThai.Items.Add(_lblCapNhat);
            _thanhTrangThai.Dock = DockStyle.Bottom;

            MainMenuStrip = _menu;
            Control trang = TaoTrangChinh();
            Controls.Add(trang);
            Controls.Add(_menu);
            Controls.Add(_thanhTrangThai);
            Load += async delegate { await TaiTongQuanAsync(); };
        }

        private void DungMenu()
        {
            var heThong = Muc("Hệ thống");
            heThong.DropDownItems.Add(Muc("Đổi mật khẩu", delegate { MoHopThoai(new FrmDoiMatKhau(_nguoiDung)); }));
            if (LaQuanLy)
                heThong.DropDownItems.Add(Muc("Quản lý tài khoản", delegate { MoHopThoai(new FrmTaiKhoan(_nguoiDung)); }));
            heThong.DropDownItems.Add(new ToolStripSeparator());
            var toi = new ToolStripMenuItem("Giao diện tối") { CheckOnClick = true, Checked = GiaoDien.Toi, ShortcutKeys = Keys.Control | Keys.D };
            toi.Click += delegate { GiaoDien.DatCheDo(toi.Checked); };
            heThong.DropDownItems.Add(new ToolStripSeparator());
            heThong.DropDownItems.Add(toi);
            heThong.DropDownItems.Add(new ToolStripSeparator());
            heThong.DropDownItems.Add(Muc("Đăng xuất", delegate { DangXuat = true; Close(); }));
            heThong.DropDownItems.Add(Muc("Thoát", delegate { Close(); }));

            var danhMuc = Muc("Danh mục");
            danhMuc.DropDownItems.Add(Muc("Khách hàng", delegate { MoHopThoai(new FrmKhachHang(_nguoiDung)); }));
            danhMuc.DropDownItems.Add(Muc(LaQuanLy ? "Loại phòng" : "Loại phòng (chỉ xem)", delegate { MoHopThoai(new FrmLoaiPhong(_nguoiDung)); }));
            danhMuc.DropDownItems.Add(Muc(LaQuanLy ? "Phòng" : "Phòng (chỉ xem)", delegate { MoHopThoai(new FrmPhong(_nguoiDung)); }));
            danhMuc.DropDownItems.Add(Muc(LaQuanLy ? "Dịch vụ" : "Dịch vụ (chỉ xem)", delegate { MoHopThoai(new FrmDichVu(_nguoiDung)); }));

            var nghiepVu = Muc("Nghiệp vụ");
            nghiepVu.DropDownItems.Add(Muc("Sơ đồ phòng \t(F5)", delegate { MoHopThoai(new FrmSoDoPhong(_nguoiDung)); }));
            nghiepVu.DropDownItems.Add(new ToolStripSeparator());
            nghiepVu.DropDownItems.Add(Muc("Đặt phòng \t(F2)", delegate { MoHopThoai(new FrmDatPhong(_nguoiDung)); }));
            nghiepVu.DropDownItems.Add(Muc("Nhận phòng \t(F3)", delegate { MoHopThoai(new FrmNhanPhong(_nguoiDung)); }));
            nghiepVu.DropDownItems.Add(Muc("Ghi dịch vụ \t(F4)", delegate { MoHopThoai(new FrmGhiDichVu(_nguoiDung)); }));
            nghiepVu.DropDownItems.Add(Muc("Trả phòng \t(F6)", delegate { MoHopThoai(new FrmTraPhong(_nguoiDung)); }));

            var traCuu = Muc("Tra cứu");
            traCuu.DropDownItems.Add(Muc("Khách đang ở", delegate { MoHopThoai(new FrmKhachDangO()); }));
            traCuu.DropDownItems.Add(Muc("Hóa đơn \t(F7)", delegate { MoHopThoai(new FrmHoaDon()); }));
            traCuu.DropDownItems.Add(new ToolStripSeparator());
            traCuu.DropDownItems.Add(Muc("Mã QR phòng và giá", delegate { MoHopThoai(new FrmQrPhong()); }));

            var baoCao = Muc("Báo cáo");
            baoCao.Visible = LaQuanLy;
            baoCao.DropDownItems.Add(Muc("Doanh thu", delegate { MoHopThoai(new FrmBaoCao(0)); }));
            baoCao.DropDownItems.Add(Muc("Công suất phòng", delegate { MoHopThoai(new FrmBaoCao(1)); }));

            var troGiup = Muc("Trợ giúp");
            troGiup.DropDownItems.Add(Muc("Giới thiệu", delegate { MoHopThoai(new FrmGioiThieu()); }));

            _menu.Items.AddRange(new ToolStripItem[] { heThong, danhMuc, nghiepVu, traCuu, baoCao, troGiup });
        }

        private static ToolStripMenuItem Muc(string chu, EventHandler click = null)
        {
            var m = new ToolStripMenuItem(chu);
            if (click != null) m.Click += click;
            return m;
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            Form f = null;
            switch (e.KeyCode)
            {
                case Keys.F2: f = new FrmDatPhong(_nguoiDung); break;
                case Keys.F3: f = new FrmNhanPhong(_nguoiDung); break;
                case Keys.F4: f = new FrmGhiDichVu(_nguoiDung); break;
                case Keys.F5: f = new FrmSoDoPhong(_nguoiDung); break;
                case Keys.F6: f = new FrmTraPhong(_nguoiDung); break;
                case Keys.F7: f = new FrmHoaDon(); break;
            }
            if (f != null)
            {
                e.Handled = true;
                MoHopThoai(f);
            }
        }

        private Control TaoTrangChinh()
        {
            var trang = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(12, 8, 12, 8),
                ColumnCount = 1,
                RowCount = 4,
                BackColor = GiaoDien.Nen
            };
            trang.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            trang.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            trang.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            trang.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            trang.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            trang.Controls.Add(TaoTieuDe(), 0, 0);
            trang.Controls.Add(TaoTheThongKe(), 0, 1);
            trang.Controls.Add(TaoTacVuNhanh(), 0, 2);
            trang.Controls.Add(TaoNoiDungChinh(), 0, 3);
            return trang;
        }

        private Control TaoTieuDe()
        {
            var hang = new TableLayoutPanel { Dock = DockStyle.Top, AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, ColumnCount = 2, Margin = new Padding(0, 0, 0, 8) };
            hang.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60));
            hang.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40));
            var loiChao = new FlowLayoutPanel { AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, Anchor = AnchorStyles.Left, WrapContents = false, Margin = new Padding(0) };
            if (GiaoDien.Logo != null)
                loiChao.Controls.Add(new PictureBox { Image = GiaoDien.Logo, SizeMode = PictureBoxSizeMode.Zoom, Size = new Size(40, 40), Margin = new Padding(0, 0, 10, 0) });
            loiChao.Controls.Add(new Label
            {
                Text = "Xin chào, " + (LaQuanLy ? "Quản lý Vạn Năng" : "Lễ tân 0 Lương"),
                AutoSize = true,
                Anchor = AnchorStyles.Left,
                Margin = new Padding(0, 8, 0, 0),
                Font = new Font("Microsoft Sans Serif", 14F, FontStyle.Bold),
                ForeColor = GiaoDien.Chu
            });
            hang.Controls.Add(loiChao, 0, 0);
            hang.Controls.Add(new Label
            {
                Text = DateTime.Now.ToString("dddd, dd/MM/yyyy") + Environment.NewLine + "Autumn Hotel · Quản lý lưu trú",
                AutoSize = true,
                Anchor = AnchorStyles.Right,
                TextAlign = ContentAlignment.MiddleRight,
                ForeColor = GiaoDien.Phu
            }, 1, 0);
            return hang;
        }

        private Control TaoTheThongKe()
        {
            var hang = new TableLayoutPanel { Dock = DockStyle.Top, AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, ColumnCount = 4, Margin = new Padding(0, 0, 0, 8) };
            for (int i = 0; i < 4; i++) hang.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));
            string[] nhan = { "PHÒNG TRỐNG", "ĐÃ ĐẶT", "ĐANG Ở", "DỌN DẸP / BẢO TRÌ" };
            for (int i = 0; i < nhan.Length; i++)
            {
                var the = new TableLayoutPanel
                {
                    Dock = DockStyle.Fill,
                    AutoSize = true,
                    AutoSizeMode = AutoSizeMode.GrowAndShrink,
                    ColumnCount = 1,
                    RowCount = 2,
                    BackColor = GiaoDien.BeMat,
                    Margin = new Padding(0, 0, i == nhan.Length - 1 ? 0 : 8, 0),
                    Padding = new Padding(10, 6, 10, 6),
                    CellBorderStyle = TableLayoutPanelCellBorderStyle.Outset
                };
                the.Controls.Add(new Label { Text = nhan[i], AutoSize = true, Font = new Font("Microsoft Sans Serif", 8.25F, FontStyle.Bold), ForeColor = GiaoDien.Phu }, 0, 0);
                _soPhong[i] = new Label { Text = "—", AutoSize = true, Font = new Font("Microsoft Sans Serif", 16F, FontStyle.Bold), ForeColor = GiaoDien.Chu };
                the.Controls.Add(_soPhong[i], 0, 1);
                hang.Controls.Add(the, i, 0);
            }
            return hang;
        }

        private Control TaoTacVuNhanh()
        {
            var hang = new FlowLayoutPanel { Dock = DockStyle.Top, AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, WrapContents = true, Margin = new Padding(0, 0, 0, 8) };
            ThemTacVu(hang, "Đặt phòng (F2)", delegate { MoHopThoai(new FrmDatPhong(_nguoiDung)); });
            ThemTacVu(hang, "Nhận phòng (F3)", delegate { MoHopThoai(new FrmNhanPhong(_nguoiDung)); });
            ThemTacVu(hang, "Ghi dịch vụ (F4)", delegate { MoHopThoai(new FrmGhiDichVu(_nguoiDung)); });
            ThemTacVu(hang, "Trả phòng (F6)", delegate { MoHopThoai(new FrmTraPhong(_nguoiDung)); });
            ThemTacVu(hang, "Khách đang ở", delegate { MoHopThoai(new FrmKhachDangO()); });
            return hang;
        }

        private static void ThemTacVu(FlowLayoutPanel hang, string chu, EventHandler click)
        {
            var nut = GiaoDien.TaoNut(chu, click);
            nut.MinimumSize = new Size(130, 34);
            nut.Margin = new Padding(0, 0, 8, 4);
            nut.Font = new Font("Microsoft Sans Serif", 8.25F, FontStyle.Bold);
            hang.Controls.Add(nut);
        }

        private Control TaoNoiDungChinh()
        {
            var cot = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1, Margin = new Padding(0) };
            cot.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 68));
            cot.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 32));
            cot.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            var phong = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2, BackColor = GiaoDien.BeMat, BorderStyle = BorderStyle.Fixed3D, Margin = new Padding(0, 0, 10, 0) };
            phong.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            phong.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            var tieuDe = new TableLayoutPanel { Dock = DockStyle.Top, AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, ColumnCount = 2, Padding = new Padding(8, 6, 8, 4) };
            tieuDe.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            tieuDe.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            tieuDe.Controls.Add(new Label { Text = "Tình trạng phòng", AutoSize = true, Anchor = AnchorStyles.Left, Font = new Font("Microsoft Sans Serif", 9F, FontStyle.Bold) }, 0, 0);
            _lamMoi.LinkColor = GiaoDien.MauThu;
            _lamMoi.Click += async delegate { await TaiTongQuanAsync(); };
            tieuDe.Controls.Add(_lamMoi, 1, 0);
            _bangPhong.Dock = DockStyle.Fill;
            phong.Controls.Add(tieuDe, 0, 0);
            phong.Controls.Add(_bangPhong, 0, 1);
            cot.Controls.Add(phong, 0, 0);

            var ben = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2, Margin = new Padding(0) };
            ben.RowStyles.Add(new RowStyle(SizeType.Percent, 55));
            ben.RowStyles.Add(new RowStyle(SizeType.Percent, 45));
            _anhKhachSan.Dock = DockStyle.Fill;
            _anhKhachSan.Margin = new Padding(0, 0, 0, 10);
            ben.Controls.Add(_anhKhachSan, 0, 0);

            var vanHanh = new Panel { Dock = DockStyle.Fill, BackColor = GiaoDien.BeMat, BorderStyle = BorderStyle.Fixed3D, Padding = new Padding(10), AutoScroll = true };
            _homNay.Dock = DockStyle.Top;
            _homNay.Text = "Đang tải…";
            vanHanh.Controls.Add(_homNay);
            vanHanh.Controls.Add(new Label { Text = "VẬN HÀNH HÔM NAY", Dock = DockStyle.Top, AutoSize = true, Padding = new Padding(0, 0, 0, 6), Font = new Font("Microsoft Sans Serif", 8.25F, FontStyle.Bold), ForeColor = GiaoDien.CamLa });
            ben.Controls.Add(vanHanh, 0, 1);
            cot.Controls.Add(ben, 1, 0);
            return cot;
        }

        private async Task TaiTongQuanAsync()
        {
            if (_dangTai || IsDisposed) return;
            _dangTai = true;
            _lamMoi.Enabled = false;
            _lamMoi.Text = "Đang tải…";
            try
            {
                var ketQua = await Task.Run(() => new { Phong = new PhongBLL().LaySoDo(), Dat = new DatPhongBLL().LayDangDat() });
                if (IsDisposed || Disposing) return;
                _bangPhong.TaiDuLieu(ketQua.Phong);

                int trong = 0, daDat = 0, dangO = 0, canXuLy = 0, diHomNay = 0;
                foreach (DataRow p in ketQua.Phong.Rows)
                {
                    switch (Convert.ToString(p["TrangThai"]))
                    {
                        case TrangThaiPhong.Trong: trong++; break;
                        case TrangThaiPhong.DaDat: daDat++; break;
                        case TrangThaiPhong.DangO:
                            dangO++;
                            if (p["NgayDiDuKien"] != DBNull.Value && Convert.ToDateTime(p["NgayDiDuKien"]).Date <= DateTime.Today) diHomNay++;
                            break;
                        case TrangThaiPhong.DonDep:
                        case TrangThaiPhong.BaoTri: canXuLy++; break;
                    }
                }
                int denHomNay = 0, quaHan = 0;
                foreach (DataRow d in ketQua.Dat.Rows)
                {
                    DateTime den = Convert.ToDateTime(d["NgayDen"]).Date;
                    if (den == DateTime.Today) denHomNay++;
                    else if (den < DateTime.Today) quaHan++;
                }

                _soPhong[0].Text = trong.ToString();
                _soPhong[1].Text = daDat.ToString();
                _soPhong[2].Text = dangO.ToString();
                _soPhong[3].Text = canXuLy.ToString();
                _homNay.Text = "• Khách đến hôm nay: " + denHomNay + Environment.NewLine +
                               "• Khách đến hạn trả phòng: " + diHomNay + Environment.NewLine +
                               "• Đặt phòng quá ngày đến chưa nhận: " + quaHan + Environment.NewLine +
                               "• Phòng cần dọn / bảo trì: " + canXuLy;
                _lblCapNhat.Text = "Cập nhật lúc " + DateTime.Now.ToString("HH:mm:ss");
            }
            catch (Exception ex)
            {
                if (!IsDisposed && !Disposing)
                {
                    _bangPhong.HienLoi("Không tải được tình trạng phòng:" + Environment.NewLine + ex.Message);
                    _homNay.Text = "Chưa tải được dữ liệu. Kiểm tra kết nối cơ sở dữ liệu rồi bấm Làm mới.";
                }
            }
            finally
            {
                _dangTai = false;
                if (!IsDisposed && !Disposing)
                {
                    _lamMoi.Enabled = true;
                    _lamMoi.Text = "Làm mới";
                }
            }
        }

        private async void MoHopThoai(Form form)
        {
            using (form) form.ShowDialog(this);
            await TaiTongQuanAsync();
        }
    }
}
