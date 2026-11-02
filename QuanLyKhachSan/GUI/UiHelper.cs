using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;
using QuanLyKhachSan.BLL;

namespace QuanLyKhachSan.GUI
{
    /// <summary>Tiện ích giao diện dùng chung: lưới dữ liệu, thông báo, hộp thoại lưu CSV.</summary>
    internal static class UiHelper
    {
        public const string DinhDangTien = "N0";
        public const string DinhDangNgay = "dd/MM/yyyy";
        public const string DinhDangNgayGio = "dd/MM/yyyy HH:mm";

        /// <summary>Tạo lưới chỉ đọc, chọn cả dòng, thống nhất kiểu dáng giữa các màn hình.</summary>
        public static DataGridView TaoLuoi()
        {
            var grid = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AllowUserToResizeRows = false,
                MultiSelect = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                RowHeadersVisible = false,
                BackgroundColor = SystemColors.Window,
                BorderStyle = BorderStyle.None
            };
            grid.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(244, 247, 251);
            grid.ColumnHeadersDefaultCellStyle.Font = new Font(grid.Font, FontStyle.Bold);
            return grid;
        }

        /// <summary>
        /// Gán nguồn dữ liệu và đặt tiêu đề/định dạng cột. Mỗi phần tử cấu hình có dạng
        /// "TenCot|Tiêu đề|định dạng" (định dạng tùy chọn), hoặc "-TenCot" để ẩn cột.
        /// </summary>
        public static void GanNguon(DataGridView grid, DataTable nguon, params string[] cauHinh)
        {
            grid.DataSource = nguon;
            grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            foreach (string cfg in cauHinh)
            {
                if (cfg.StartsWith("-"))
                {
                    DataGridViewColumn an = grid.Columns[cfg.Substring(1)];
                    if (an != null) an.Visible = false;
                    continue;
                }

                string[] phan = cfg.Split('|');
                DataGridViewColumn cot = grid.Columns[phan[0]];
                if (cot == null) continue;
                if (phan.Length > 1) cot.HeaderText = phan[1];
                if (phan.Length > 2 && phan[2].Length > 0)
                {
                    cot.DefaultCellStyle.Format = phan[2];
                    cot.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                }
            }
        }

        public static void ThongBaoLoi(IWin32Window chu, string tieuDe, Exception ex)
        {
            MessageBox.Show(chu, ex.Message, tieuDe, MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        public static void ThongBaoThanhCong(IWin32Window chu, string noiDung)
        {
            MessageBox.Show(chu, noiDung, "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        public static bool XacNhan(IWin32Window chu, string noiDung)
        {
            return MessageBox.Show(chu, noiDung, "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Question)
                   == DialogResult.Yes;
        }

        /// <summary>Nhãn đặt cạnh ô nhập trong thanh công cụ ngang (căn dọc với ô nhập).</summary>
        public static Label NhanThanhCong(string noiDung)
        {
            return new Label { Text = noiDung, AutoSize = true, Margin = new Padding(8, 8, 2, 0) };
        }

        /// <summary>Xuất các cột đang hiển thị của lưới ra tệp CSV (hỏi nơi lưu).</summary>
        public static void XuatCsv(IWin32Window chu, DataGridView grid, string tenGoiY)
        {
            if (grid.Rows.Count == 0)
            {
                MessageBox.Show(chu, "Không có dữ liệu để xuất.", "Xuất CSV", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using (var dlg = new SaveFileDialog
            {
                Filter = "Tệp CSV (*.csv)|*.csv",
                FileName = tenGoiY + "_" + DateTime.Now.ToString("yyyyMMdd_HHmm") + ".csv",
                OverwritePrompt = true
            })
            {
                if (dlg.ShowDialog(chu) != DialogResult.OK) return;
                try
                {
                    var tieuDe = new List<string>();
                    var cot = new List<DataGridViewColumn>();
                    foreach (DataGridViewColumn c in grid.Columns)
                    {
                        if (!c.Visible) continue;
                        tieuDe.Add(c.HeaderText);
                        cot.Add(c);
                    }

                    var dong = new List<IList<string>>();
                    foreach (DataGridViewRow r in grid.Rows)
                    {
                        var o = new List<string>();
                        foreach (DataGridViewColumn c in cot)
                            o.Add(DinhDangO(r.Cells[c.Index]));
                        dong.Add(o);
                    }

                    CsvExporter.Ghi(dlg.FileName, tieuDe, dong);
                    MessageBox.Show(chu, "Đã xuất " + dong.Count + " dòng ra:\n" + dlg.FileName, "Xuất CSV",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    ThongBaoLoi(chu, "Không thể xuất CSV", ex);
                }
            }
        }

        /// <summary>
        /// Giá trị ô dạng chuỗi cho CSV: ngày theo định dạng hiển thị, số ghi thô (không dấu phân cách nghìn)
        /// để Excel nhận là số.
        /// </summary>
        public static string DinhDangO(DataGridViewCell cell)
        {
            object v = cell.Value;
            if (v == null || v == DBNull.Value) return string.Empty;
            if (v is DateTime)
            {
                string format = cell.InheritedStyle.Format;
                return ((DateTime)v).ToString(string.IsNullOrEmpty(format) ? DinhDangNgayGio : format,
                    CultureInfo.InvariantCulture);
            }
            if (v is decimal) return ((decimal)v).ToString("0.##", CultureInfo.InvariantCulture);
            if (v is double) return ((double)v).ToString("0.##", CultureInfo.InvariantCulture);
            if (v is int || v is long || v is short) return Convert.ToString(v, CultureInfo.InvariantCulture);
            return Convert.ToString(v);
        }
    }
}
