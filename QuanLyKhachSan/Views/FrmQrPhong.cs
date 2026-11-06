using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Drawing.Printing;
using System.Globalization;
using System.IO;
using System.Text;
using System.Windows.Forms;
using QuanLyKhachSan.Services;
using QuanLyKhachSan.Repositories;

namespace QuanLyKhachSan.Views
{
    /// <summary>
    /// Tạo mã QR tra cứu phòng hoặc bảng giá: quét bằng điện thoại sẽ mở trang web tra cứu (project QuanLyKhachSan.Web).
    /// Cho phép lưu PNG, in thẻ dán cửa phòng và xuất hàng loạt.
    /// </summary>
    public sealed class FrmQrPhong : Form
    {
        private sealed class Muc
        {
            public string Nhan;
            public string Url;
            public string TieuDe;
            public string PhuDe;
            public string TenTep;
            public override string ToString() { return Nhan; }
        }

        private readonly ComboBox _cboMuc = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 330 };
        private readonly PictureBox _hinh = new PictureBox { SizeMode = PictureBoxSizeMode.Zoom, BackColor = Color.White, BorderStyle = BorderStyle.FixedSingle, Dock = DockStyle.Fill, MinimumSize = new Size(260, 260) };
        private readonly TextBox _txtUrl = new TextBox { ReadOnly = true, Anchor = AnchorStyles.Left | AnchorStyles.Right };
        private readonly List<Muc> _tatCa = new List<Muc>();
        private Bitmap _the;

        public FrmQrPhong()
        {
            Text = "Mã QR tra cứu phòng và giá";
            ClientSize = new Size(560, 640);
            MinimumSize = new Size(480, 560);
            StartPosition = FormStartPosition.CenterParent;
            AutoScaleMode = AutoScaleMode.Dpi;

            var thanh = GiaoDien.ThanhNgang();
            thanh.Controls.Add(GiaoDien.Nhom("Nội dung mã QR", _cboMuc, 330));

            var goc = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2, Padding = new Padding(12, 4, 12, 8) };
            goc.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            goc.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            goc.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            goc.Controls.Add(_hinh, 0, 0);
            goc.Controls.Add(_txtUrl, 0, 1);

            var nut = GiaoDien.HangNut();
            nut.Dock = DockStyle.Bottom;
            nut.Controls.Add(GiaoDien.TaoNut("Lưu PNG...", delegate { LuuMot(); }, true));
            nut.Controls.Add(GiaoDien.TaoNut("In...", delegate { In(); }));
            nut.Controls.Add(GiaoDien.TaoNut("Xuất tất cả...", delegate { XuatTatCa(); }));
            nut.Controls.Add(GiaoDien.TaoNut("Sao chép liên kết", delegate { Clipboard.SetText(_txtUrl.Text); }));
            var dong = GiaoDien.TaoNut("Đóng", null);
            dong.DialogResult = DialogResult.Cancel;
            nut.Controls.Add(dong);
            CancelButton = dong;

            Controls.Add(goc);
            Controls.Add(thanh);
            Controls.Add(nut);

            _cboMuc.SelectedIndexChanged += delegate { HienThi(); };
            Load += delegate { TaiDuLieu(); };
            FormClosed += delegate { if (_the != null) _the.Dispose(); };
            GiaoDien.ApDung(this);
        }

        private void TaiDuLieu()
        {
            try
            {
                string goc = CauHinhWeb.UrlGoc;
                DataTable loai = new LoaiPhongService().TimKiem();
                var gia = new Dictionary<int, decimal>();
                foreach (DataRow r in loai.Rows)
                    gia[Convert.ToInt32(r["MaLoaiPhong"])] = Convert.ToDecimal(r["GiaMoiDem"]);

                _tatCa.Clear();
                _tatCa.Add(new Muc
                {
                    Nhan = "Bảng giá toàn khách sạn",
                    Url = TraCuuUrl.BangGia(goc),
                    TieuDe = "BẢNG GIÁ PHÒNG",
                    PhuDe = "Quét mã để xem giá các loại phòng",
                    TenTep = "QR_BangGia"
                });
                DataTable phong = new PhongService().TimKiem();
                foreach (DataRow r in phong.Rows)
                {
                    if (!Convert.ToBoolean(r["HoatDong"])) continue;
                    string so = Convert.ToString(r["SoPhong"]).Trim();
                    decimal g;
                    string tenLoai = Convert.ToString(r["TenLoaiPhong"]);
                    string giaChu = gia.TryGetValue(Convert.ToInt32(r["MaLoaiPhong"]), out g)
                        ? g.ToString("N0", CultureInfo.CurrentCulture) + " đ/đêm" : "";
                    _tatCa.Add(new Muc
                    {
                        Nhan = "Phòng " + so + " · Tầng " + r["Tang"] + " · " + tenLoai + (giaChu.Length > 0 ? " · " + giaChu : ""),
                        Url = TraCuuUrl.Phong(goc, so),
                        TieuDe = "PHÒNG " + so,
                        PhuDe = tenLoai + (giaChu.Length > 0 ? " · " + giaChu : ""),
                        TenTep = "QR_Phong_" + TenTepAnToan(so)
                    });
                }
                _cboMuc.Items.Clear();
                foreach (Muc m in _tatCa) _cboMuc.Items.Add(m);
                _cboMuc.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                UiHelper.ThongBaoLoi(this, "Mã QR", ex);
            }
        }

        private void HienThi()
        {
            var m = _cboMuc.SelectedItem as Muc;
            if (m == null) return;
            Bitmap cu = _the;
            _the = QrHinh.TaoThe(m.Url, m.TieuDe, m.PhuDe);
            _hinh.Image = _the;
            if (cu != null) cu.Dispose();
            _txtUrl.Text = m.Url;
        }

        private void LuuMot()
        {
            var m = _cboMuc.SelectedItem as Muc;
            if (m == null || _the == null) return;
            using (var hop = new SaveFileDialog { Filter = "Ảnh PNG|*.png", FileName = m.TenTep + ".png" })
            {
                if (hop.ShowDialog(this) != DialogResult.OK) return;
                try
                {
                    _the.Save(hop.FileName, System.Drawing.Imaging.ImageFormat.Png);
                    UiHelper.ThongBaoThanhCong(this, "Đã lưu: " + hop.FileName);
                }
                catch (Exception ex) { UiHelper.ThongBaoLoi(this, "Lưu mã QR", ex); }
            }
        }

        private void XuatTatCa()
        {
            if (_tatCa.Count == 0) return;
            using (var hop = new FolderBrowserDialog { Description = "Chọn thư mục lưu tất cả mã QR (bảng giá và từng phòng)" })
            {
                if (hop.ShowDialog(this) != DialogResult.OK) return;
                try
                {
                    int dem = 0;
                    foreach (Muc m in _tatCa)
                        using (Bitmap the = QrHinh.TaoThe(m.Url, m.TieuDe, m.PhuDe))
                        {
                            the.Save(Path.Combine(hop.SelectedPath, m.TenTep + ".png"), System.Drawing.Imaging.ImageFormat.Png);
                            dem++;
                        }
                    UiHelper.ThongBaoThanhCong(this, "Đã xuất " + dem + " mã QR vào: " + hop.SelectedPath);
                }
                catch (Exception ex) { UiHelper.ThongBaoLoi(this, "Xuất mã QR", ex); }
            }
        }

        private void In()
        {
            if (_the == null) return;
            var tai = new PrintDocument();
            tai.PrintPage += delegate (object s, PrintPageEventArgs e)
            {
                RectangleF vung = e.MarginBounds;
                float tyLe = Math.Min(vung.Width / _the.Width, vung.Height / _the.Height);
                float r = _the.Width * tyLe, c = _the.Height * tyLe;
                e.Graphics.DrawImage(_the, vung.Left + (vung.Width - r) / 2, vung.Top, r, c);
                e.HasMorePages = false;
            };
            using (var xem = new PrintPreviewDialog { Document = tai, Width = 900, Height = 700 })
                xem.ShowDialog(this);
        }

        private static string TenTepAnToan(string s)
        {
            var sb = new StringBuilder();
            foreach (char ch in s)
                sb.Append(Array.IndexOf(Path.GetInvalidFileNameChars(), ch) >= 0 ? '_' : ch);
            return sb.ToString();
        }
    }
}
