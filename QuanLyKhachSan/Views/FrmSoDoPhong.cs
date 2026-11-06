using System;
using System.Drawing;
using System.Windows.Forms;
using QuanLyKhachSan.Models;

namespace QuanLyKhachSan.Views
{
    /// <summary>Cửa sổ sơ đồ phòng theo tầng (dùng chung UserControl với màn hình chính).</summary>
    public sealed class FrmSoDoPhong : Form
    {
        private readonly UcTrangThaiPhong _bang;

        public FrmSoDoPhong(NguoiDung nguoiDung)
        {
            Text = "Sơ đồ trạng thái phòng";
            ClientSize = new Size(960, 620);
            MinimumSize = new Size(560, 380);
            StartPosition = FormStartPosition.CenterParent;
            AutoScaleMode = AutoScaleMode.Dpi;

            _bang = new UcTrangThaiPhong(nguoiDung) { Dock = DockStyle.Fill };

            var thanh = GiaoDien.ThanhNgang();
            thanh.Controls.Add(GiaoDien.TaoNut("Làm mới", delegate { _bang.TaiDuLieu(); }, true));
            var ghiChu = new Label
            {
                AutoSize = true,
                Margin = new Padding(8, 6, 0, 0),
                Text = "Bấm vào thẻ phòng để đánh dấu dọn xong" + (nguoiDung != null && nguoiDung.VaiTro == VaiTroNguoiDung.QuanLy ? " hoặc chuyển bảo trì." : "."),
                ForeColor = GiaoDien.Phu
            };
            thanh.Controls.Add(ghiChu);

            Controls.Add(_bang);
            Controls.Add(thanh);
            Load += delegate { _bang.TaiDuLieu(); };
            GiaoDien.ApDung(this);
        }
    }
}
