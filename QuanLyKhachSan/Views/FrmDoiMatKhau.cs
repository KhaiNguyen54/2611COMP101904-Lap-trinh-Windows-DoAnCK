using System;
using System.Drawing;
using System.Windows.Forms;
using QuanLyKhachSan.Presenters;
using QuanLyKhachSan.Services;
using QuanLyKhachSan.Models;

namespace QuanLyKhachSan.Views
{
    public class FrmDoiMatKhau : Form, IDoiMatKhauView
    {
        private readonly DoiMatKhauPresenter _presenter;
        private readonly TextBox _cu=new TextBox{UseSystemPasswordChar=true};
        private readonly TextBox _moi=new TextBox{UseSystemPasswordChar=true};
        private readonly TextBox _xacNhan=new TextBox{UseSystemPasswordChar=true};

        public FrmDoiMatKhau(NguoiDung nguoiDung)
        {
            _presenter=new DoiMatKhauPresenter(this,new NguoiDungService(),nguoiDung??throw new ArgumentNullException(nameof(nguoiDung)));
            Text="Đổi mật khẩu";ClientSize=new Size(440,220);StartPosition=FormStartPosition.CenterParent;FormBorderStyle=FormBorderStyle.FixedDialog;MaximizeBox=false;AutoScaleMode=AutoScaleMode.Dpi;Padding=new Padding(12);

            var layout=new TableLayoutPanel{Dock=DockStyle.Fill,ColumnCount=2,RowCount=5,Padding=new Padding(8)};
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 170F));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 34F));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 34F));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 34F));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 48F));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 32F));

            AddField(layout,0,"Mật khẩu hiện tại",_cu);AddField(layout,1,"Mật khẩu mới",_moi);AddField(layout,2,"Nhập lại mật khẩu mới",_xacNhan);

            var save=new Button{Tag=GiaoDien.NutChinhTag,Text="Cập nhật",AutoSize=true,MinimumSize=new Size(95,32)};save.Click+=delegate{_presenter.Luu();};
            var cancel=new Button{Text="Đóng",AutoSize=true,MinimumSize=new Size(90,32),DialogResult=DialogResult.Cancel};
            var buttonWrap=new FlowLayoutPanel{Dock=DockStyle.Fill,FlowDirection=FlowDirection.RightToLeft,WrapContents=false,AutoSize=true,Padding=new Padding(0),Margin=new Padding(0)};
            buttonWrap.Controls.Add(cancel);buttonWrap.Controls.Add(save);
            layout.Controls.Add(buttonWrap,0,3);layout.SetColumnSpan(buttonWrap,2);

            Controls.Add(layout);AcceptButton=save;CancelButton=cancel;GiaoDien.ApDung(this);
        }

        private static void AddField(TableLayoutPanel layout,int row,string label,TextBox box){
            layout.Controls.Add(new Label{Text=label,AutoSize=true,Anchor=AnchorStyles.Left,TextAlign=ContentAlignment.MiddleLeft,Margin=new Padding(0,6,0,0)},0,row);
            box.Anchor=AnchorStyles.Left | AnchorStyles.Right;box.Size=new Size(0,25);box.Margin=new Padding(0,4,0,4);layout.Controls.Add(box,1,row);
        }

        // ----- IDoiMatKhauView -----
        public string MatKhauCu{get{return _cu.Text;}}
        public string MatKhauMoi{get{return _moi.Text;}}
        public string XacNhanMatKhau{get{return _xacNhan.Text;}}
        public void Dong(){Close();}
        public void HienThongBao(string tieuDe,string noiDung,MucThongBao muc){UiHelper.HienThongBao(this,tieuDe,noiDung,muc);}
        public bool XacNhan(string noiDung){return UiHelper.XacNhan(this,noiDung);}
    }
}
