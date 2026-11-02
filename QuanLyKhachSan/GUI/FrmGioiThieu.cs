using System;
using System.Drawing;
using System.Windows.Forms;

namespace QuanLyKhachSan.GUI
{
    /// <summary>Hộp thoại giới thiệu ứng dụng và phím tắt.</summary>
    public sealed class FrmGioiThieu : Form
    {
        public FrmGioiThieu()
        {
            Text = "Giới thiệu";
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            AutoScaleMode = AutoScaleMode.Dpi;
            AutoSize = true;
            AutoSizeMode = AutoSizeMode.GrowAndShrink;
            Padding = new Padding(16);

            var bang = new TableLayoutPanel { AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, ColumnCount = 1, Dock = DockStyle.Fill };
            bang.Controls.Add(new Label { Text = "Autumn Hotel", AutoSize = true, Font = new Font("Microsoft Sans Serif", 16F, FontStyle.Bold), ForeColor = GiaoDien.MauThu, Margin = new Padding(0, 0, 0, 4) });
            bang.Controls.Add(new Label { Text = "Phần mềm quản lý khách sạn · WinForms, .NET, SQL Server", AutoSize = true, ForeColor = GiaoDien.Phu, Margin = new Padding(0, 0, 0, 12) });
            bang.Controls.Add(new Label
            {
                AutoSize = true,
                MaximumSize = new Size(420, 0),
                Text = "Quy trình: Đặt phòng → Nhận phòng → Ghi dịch vụ → Trả phòng → Hóa đơn → Dọn phòng." + Environment.NewLine + Environment.NewLine +
                       "Phím tắt: F2 Đặt phòng · F3 Nhận phòng · F4 Ghi dịch vụ · F5 Sơ đồ phòng · F6 Trả phòng · F7 Hóa đơn.",
                Margin = new Padding(0, 0, 0, 14)
            });
            var dong = GiaoDien.TaoNut("Đóng", null, true);
            dong.DialogResult = DialogResult.OK;
            dong.Anchor = AnchorStyles.Right;
            bang.Controls.Add(dong);
            Controls.Add(bang);
            AcceptButton = dong;
            CancelButton = dong;
            GiaoDien.ApDung(this);
        }
    }
}
