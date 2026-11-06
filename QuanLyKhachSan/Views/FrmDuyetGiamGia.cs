using System;
using System.Drawing;
using System.Windows.Forms;
using QuanLyKhachSan.Services;

namespace QuanLyKhachSan.Views
{
    /// <summary>Quản lý nhập tài khoản để duyệt mức giảm giá trên 10% khi Lễ tân trả phòng.</summary>
    public class FrmDuyetGiamGia : Form
    {
        private readonly LuuTruService _bll;
        private readonly TextBox _ten = new TextBox();
        private readonly TextBox _matKhau = new TextBox { UseSystemPasswordChar = true };

        public int MaNguoiDuyet { get; private set; }

        public FrmDuyetGiamGia(LuuTruService bll)
        {
            _bll = bll ?? throw new ArgumentNullException(nameof(bll));
            Text = "Quản lý duyệt giảm giá";
            ClientSize = new Size(440, 200);
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowInTaskbar = false;
            AutoScaleMode = AutoScaleMode.Dpi;
            Padding = new Padding(12);

            var layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 4, Padding = new Padding(8) };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 130F));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 44F));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 34F));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 34F));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 48F));

            var ghiChu = new Label { Text = "Mức giảm vượt 10% tiền hàng. Quản lý nhập tài khoản để duyệt.", AutoSize = true, MaximumSize = new Size(380, 0), ForeColor = GiaoDien.Phu };
            layout.Controls.Add(ghiChu, 0, 0);
            layout.SetColumnSpan(ghiChu, 2);
            Truong(layout, 1, "Tên đăng nhập", _ten);
            Truong(layout, 2, "Mật khẩu", _matKhau);

            var duyet = new Button { Tag = GiaoDien.NutChinhTag, Text = "Duyệt", AutoSize = true, MinimumSize = new Size(95, 32) };
            duyet.Click += Duyet_Click;
            var huy = new Button { Text = "Hủy", AutoSize = true, MinimumSize = new Size(90, 32), DialogResult = DialogResult.Cancel };
            var nut = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft, WrapContents = false, AutoSize = true };
            nut.Controls.Add(huy);
            nut.Controls.Add(duyet);
            layout.Controls.Add(nut, 0, 3);
            layout.SetColumnSpan(nut, 2);

            Controls.Add(layout);
            AcceptButton = duyet;
            CancelButton = huy;
            GiaoDien.ApDung(this);
        }

        private static void Truong(TableLayoutPanel layout, int hang, string nhan, TextBox o)
        {
            layout.Controls.Add(new Label { Text = nhan, AutoSize = true, Anchor = AnchorStyles.Left, Margin = new Padding(0, 6, 0, 0) }, 0, hang);
            o.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            o.Margin = new Padding(0, 4, 0, 4);
            layout.Controls.Add(o, 1, hang);
        }

        private void Duyet_Click(object sender, EventArgs e)
        {
            try
            {
                MaNguoiDuyet = _bll.XacThucNguoiDuyet(_ten.Text, _matKhau.Text);
                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Không thể duyệt", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                _matKhau.Clear();
            }
        }
    }
}
