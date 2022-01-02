using ScrapWeb.Controller;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ScrapWeb
{
    public partial class Form1 : Form
    {

        NotifyIcon MyIcon = new NotifyIcon();
        public Form1()
        {
            InitializeComponent();
        }


        //Not
        // Kullanıcıdan timer alabiliriz. Girilmez ise 5 dk limit belirledik. Progress Bar ile yeşil ve kırmızı olarak yazılan her doğru adımda ve yanlış adımda
        // progress bara yansıtalım
        //geriye listede log yerine yazılmayan urlleri de listeyebiliriz
        //timer tamamlanacak


        private void Form1_Load(object sender, EventArgs e)
        {


            timer1.Enabled = false;

           


        }
        
        private void timer1_Tick(object sender, EventArgs e)
        {

            

            Task.Run(() =>
            {

                run();

            });
        }

        private void run()
        {
            GetScrapData get = new GetScrapData();


            var urlList = get.GetUrlFromFile();

            if (urlList.Success && urlList.Data.Count > 0)
            {
                var pageSources = get.GetPageSources(urlList.Data);

                if (pageSources.Success && pageSources.Data.Count > 0)
                {
                    foreach (var item in pageSources.Data)
                    {

                        //kendim arkada url gönderiyorum bir alan içinde
                        var bahisler = get.GetirBahisleri(item, item.OptionStopperNodeName);
                        if (bahisler.Data.Count > 0)
                        {
                            get.WriteFileTxtBahis(bahisler.Data, item.OptionStopperNodeName);
                        }
                        else
                        {
                            get.WriteFileTxtBahis(new List<Model.BahisModel>(), item.OptionStopperNodeName + " " + Constants.Message.URL_BAHISLERI_DONMEDI);
                        }

                    }

                }

            }

        }
        
        

        private void button1_Click(object sender, EventArgs e)
        {
            textBox1.Text = System.Windows.Forms.Application.StartupPath.ToString();

           // MessageBox.Show(System.Windows.Forms.Application.StartupPath.ToString());
            if (dakikaAl.Value>0)
            {
                timer1.Interval= (int)TimeSpan.FromMinutes((double)dakikaAl.Value).TotalMilliseconds;

                timer1.Enabled = true;
                timer1.Start();
            } 
        }






        private void Form1_Resize(object sender, EventArgs e)
        {   
            if (FormWindowState.Minimized == WindowState)
            {
                Hide();
                MyIcon.Visible = true;
                MyIcon.Icon = SystemIcons.Application;
                MyIcon.Text = "Mikrojump V16 Entegrasyon";
                MyIcon.BalloonTipTitle = "Veri Çekme Uygulaması";
                MyIcon.BalloonTipText = "Veri Çekme Uygulaması Çalışıyor ";
                MyIcon.BalloonTipIcon = ToolTipIcon.Info;
                MyIcon.ShowBalloonTip(30000);
                MyIcon.MouseDoubleClick += new MouseEventHandler(MyIcon_MouseDoubleClick);
            }
        }
        void MyIcon_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            Show();
            MyIcon.Visible = false;
        }

        private void groupBox1_Enter(object sender, EventArgs e)
        {

        }
    }
}
