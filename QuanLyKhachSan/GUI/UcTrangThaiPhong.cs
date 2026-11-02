using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using QuanLyKhachSan.BLL;
using QuanLyKhachSan.DTO;

namespace QuanLyKhachSan.GUI
{
    /// <summary>
    /// Sơ đồ phòng theo tầng, tô màu theo trạng thái (NV03, BC05). Bấm vào phòng để dọn xong / bảo trì.
    /// Kích thước thẻ tính từ cỡ chữ nên không bị cắt chữ khi đổi DPI.
    /// </summary>
    public sealed class UcTrangThaiPhong : UserControl
    {
        private static readonly string[] CacTrangThai =
        {
            TrangThaiPhong.Trong, TrangThaiPhong.DaDat, TrangThaiPhong.DangO, TrangThaiPhong.DonDep, TrangThaiPhong.BaoTri
        };

        private readonly NguoiDung _nguoiDung;
        private readonly FlowLayoutPanel _chuThich = new FlowLayoutPanel();
        private readonly Panel _noiDung = new Panel();
        private readonly Label _thongBao = new Label();
        private readonly ToolTip _goiY = new ToolTip();

        /// <summary>Phát ra sau khi người dùng đổi trạng thái phòng thành công.</summary>
        public event EventHandler TrangThaiPhongThayDoi;

        public UcTrangThaiPhong() : this(null) { }

        public UcTrangThaiPhong(NguoiDung nguoiDung)
        {
            _nguoiDung = nguoiDung;

            _chuThich.Dock = DockStyle.Top;
            _chuThich.AutoSize = true;
            _chuThich.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            _chuThich.WrapContents = true;
            _chuThich.Padding = new Padding(8, 6, 8, 2);

            _noiDung.Dock = DockStyle.Fill;
            _noiDung.AutoScroll = true;
            _noiDung.Padding = new Padding(8, 0, 8, 8);

            _thongBao.Dock = DockStyle.Fill;
            _thongBao.TextAlign = ContentAlignment.MiddleCenter;
            _thongBao.ForeColor = GiaoDien.MauThu;
            _thongBao.Visible = false;

            Controls.Add(_noiDung);
            Controls.Add(_thongBao);
            Controls.Add(_chuThich);
            BackColor = GiaoDien.BeMat;
        }

        public static Color MauTrangThai(string trangThai)
        {
            switch (trangThai)
            {
                case TrangThaiPhong.DaDat: return Color.FromArgb(246, 224, 187);
                case TrangThaiPhong.DangO: return Color.FromArgb(234, 190, 173);
                case TrangThaiPhong.DonDep: return Color.FromArgb(205, 218, 232);
                case TrangThaiPhong.BaoTri: return Color.FromArgb(210, 197, 184);
                default: return Color.FromArgb(214, 226, 200);
            }
        }

        /// <summary>Tải lại sơ đồ từ CSDL; lỗi kết nối hiển thị tại chỗ thay vì làm sập màn hình.</summary>
        public void TaiDuLieu()
        {
            try
            {
                TaiDuLieu(new PhongBLL().LaySoDo());
            }
            catch (Exception ex)
            {
                HienLoi("Không tải được sơ đồ phòng:" + Environment.NewLine + ex.Message);
            }
        }

        public void HienLoi(string noiDung)
        {
            _noiDung.Visible = false;
            _thongBao.Text = noiDung;
            _thongBao.Visible = true;
        }

        /// <summary>Vẽ sơ đồ từ bảng <see cref="PhongBLL.LaySoDo"/> (hoặc <see cref="PhongBLL.TimKiem"/>).</summary>
        public void TaiDuLieu(DataTable phong)
        {
            _thongBao.Visible = false;
            _noiDung.Visible = true;
            _noiDung.SuspendLayout();
            XoaCon(_noiDung);
            XoaCon(_chuThich);

            var dem = new Dictionary<string, int>();
            foreach (string t in CacTrangThai) dem[t] = 0;

            var theoTang = new SortedDictionary<int, List<DataRow>>();
            foreach (DataRow row in phong.Rows)
            {
                if (phong.Columns.Contains("HoatDong") && !Convert.ToBoolean(row["HoatDong"])) continue;
                int tang = Convert.ToInt32(row["Tang"]);
                if (!theoTang.ContainsKey(tang)) theoTang[tang] = new List<DataRow>();
                theoTang[tang].Add(row);
                string tt = Convert.ToString(row["TrangThai"]);
                if (dem.ContainsKey(tt)) dem[tt]++;
            }

            // Dock.Top xếp ngược thứ tự thêm: thêm tầng cao trước để tầng thấp nằm trên cùng.
            var cacTang = new List<int>(theoTang.Keys);
            for (int i = cacTang.Count - 1; i >= 0; i--)
            {
                int tang = cacTang[i];
                var khung = new FlowLayoutPanel
                {
                    Dock = DockStyle.Top,
                    AutoSize = true,
                    AutoSizeMode = AutoSizeMode.GrowAndShrink,
                    WrapContents = true,
                    Padding = new Padding(0, 2, 0, 6),
                    BackColor = GiaoDien.BeMat
                };
                foreach (DataRow row in theoTang[tang]) khung.Controls.Add(TaoThePhong(row, phong));

                var tieuDe = new Label
                {
                    Dock = DockStyle.Top,
                    AutoSize = true,
                    Padding = new Padding(0, 8, 0, 2),
                    Text = "Tầng " + tang + "  —  " + theoTang[tang].Count + " phòng",
                    Font = new Font(Font, FontStyle.Bold),
                    ForeColor = GiaoDien.Chu
                };
                _noiDung.Controls.Add(khung);
                _noiDung.Controls.Add(tieuDe);
            }

            if (theoTang.Count == 0)
                _noiDung.Controls.Add(new Label { Dock = DockStyle.Top, AutoSize = true, Padding = new Padding(0, 12, 0, 0), Text = "Chưa có phòng nào đang hoạt động." });

            foreach (string t in CacTrangThai) _chuThich.Controls.Add(TaoChuThich(t, dem[t]));
            _noiDung.ResumeLayout(true);
        }

        private static void XoaCon(Control cha)
        {
            while (cha.Controls.Count > 0)
            {
                Control c = cha.Controls[0];
                cha.Controls.RemoveAt(0);
                c.Dispose();
            }
        }

        private Control TaoChuThich(string trangThai, int soLuong)
        {
            var o = new FlowLayoutPanel { AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, Margin = new Padding(0, 0, 14, 0), WrapContents = false };
            int canh = Math.Max(14, Font.Height - 2);
            o.Controls.Add(new Panel
            {
                Width = canh,
                Height = canh,
                Margin = new Padding(0, 2, 5, 0),
                BackColor = MauTrangThai(trangThai),
                BorderStyle = BorderStyle.FixedSingle
            });
            o.Controls.Add(new Label { AutoSize = true, Text = trangThai + ": " + soLuong, Margin = new Padding(0, 1, 0, 0) });
            return o;
        }

        private static string Chuoi(DataRow row, string cot)
        {
            return row.Table.Columns.Contains(cot) && row[cot] != DBNull.Value ? Convert.ToString(row[cot]) : null;
        }

        private Control TaoThePhong(DataRow row, DataTable bang)
        {
            string trangThai = Convert.ToString(row["TrangThai"]);
            string trangThaiGoc = Chuoi(row, "TrangThaiGoc") ?? trangThai;
            int maPhong = Convert.ToInt32(row["MaPhong"]);
            string loai = Chuoi(row, "TenLoaiPhong") ?? string.Empty;

            string dong3 = trangThai;
            string khach = Chuoi(row, "KhachDangO");
            if (trangThai == TrangThaiPhong.DangO && khach != null)
            {
                dong3 = khach;
                string ngayDi = Chuoi(row, "NgayDiDuKien");
                if (ngayDi != null) dong3 += " · đi " + Convert.ToDateTime(row["NgayDiDuKien"]).ToString("dd/MM");
            }

            int cao = Font.Height * 3 + 18;
            var the = new Button
            {
                Width = Font.Height * 14,
                Height = cao,
                Margin = new Padding(0, 0, 8, 8),
                FlatStyle = FlatStyle.Flat,
                UseVisualStyleBackColor = false,
                BackColor = MauTrangThai(trangThai),
                ForeColor = GiaoDien.Chu,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(6, 0, 4, 0),
                AutoEllipsis = true,
                Tag = GiaoDien.KhongTo,
                Text = "Phòng " + row["SoPhong"] + Environment.NewLine + loai + Environment.NewLine + dong3
            };
            the.FlatAppearance.BorderColor = GiaoDien.Vien;

            ContextMenuStrip menu = TaoMenu(maPhong, trangThaiGoc);
            if (menu.Items.Count > 0)
            {
                the.Cursor = Cursors.Hand;
                the.Click += delegate { menu.Show(the, new Point(0, the.Height)); };
            }

            string gia = bang.Columns.Contains("GiaMoiDem") ? string.Format(" · {0:N0} đ/đêm", row["GiaMoiDem"]) : string.Empty;
            _goiY.SetToolTip(the, "Phòng " + row["SoPhong"] + " · tầng " + row["Tang"] + " · " + loai + gia + Environment.NewLine + trangThai);
            return the;
        }

        private ContextMenuStrip TaoMenu(int maPhong, string trangThaiGoc)
        {
            var menu = new ContextMenuStrip();
            bool quanLy = _nguoiDung != null && NguoiDungBLL.CoQuyenQuanLy(_nguoiDung);

            if (trangThaiGoc == TrangThaiPhong.DonDep)
                menu.Items.Add("Đã dọn xong (chuyển sang Trống)", null,
                    delegate { ThucHien(delegate { new PhongBLL().HoanTatDonDep(maPhong); }); });

            if (quanLy && trangThaiGoc == TrangThaiPhong.Trong)
                menu.Items.Add("Chuyển sang Bảo trì", null,
                    delegate { ThucHien(delegate { new PhongBLL().BatDauBaoTri(maPhong, true); }); });

            if (quanLy && trangThaiGoc == TrangThaiPhong.BaoTri)
                menu.Items.Add("Kết thúc bảo trì (chuyển sang Trống)", null,
                    delegate { ThucHien(delegate { new PhongBLL().KetThucBaoTri(maPhong, true); }); });

            return menu;
        }

        private void ThucHien(Action thaoTac)
        {
            try
            {
                thaoTac();
            }
            catch (Exception ex)
            {
                UiHelper.ThongBaoLoi(FindForm(), "Không thể đổi trạng thái phòng", ex);
            }

            if (TrangThaiPhongThayDoi != null) TrangThaiPhongThayDoi(this, EventArgs.Empty);
            else TaiDuLieu();
        }
    }
}
