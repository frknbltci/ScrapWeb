using ScrapWeb.Controller;
using ScrapWeb.Helper.Log;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
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
            timer2.Enabled = false;

        }
        
        private void timer1_Tick(object sender, EventArgs e)
        {
            //run();
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

                var macLinkleri = get.GetirMusabakaLinkeri(urlList.Data);
            //    var macLinkleri = new List<string>();
            ///var data = "https://gizabet741.com/tr/bet/fixture-detail/36883182";

                //macLinkleri.Add(data);
                if (macLinkleri !=null && macLinkleri.Data.Count>0)
                {
                    var pageSources = get.GetPageSources(macLinkleri.Data);

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

        }
        
        
        //Kaydet Butonu
        private void button1_Click(object sender, EventArgs e)
        {
            textBox1.Text = System.Windows.Forms.Application.StartupPath.ToString();

            // MessageBox.Show(System.Windows.Forms.Application.StartupPath.ToString());
               if (dakikaAl.Value>0)
            {
      
                timer1.Interval= (int)TimeSpan.FromMinutes((double)dakikaAl.Value).TotalMilliseconds;
                timer2.Interval= (int)TimeSpan.FromMinutes(15).TotalMilliseconds;

                timer1.Enabled = true;
                timer2.Enabled = true;
        
                timer1.Start();
                timer2.Start();

                MessageBox.Show("Program Başlatıldı..");
            }
        }


        private bool killProcessOfChromeAndFirefox() {

            try
            {
                
                //firefox,chrome
                Process[] chromeDriverProcesses = Process.GetProcessesByName("chrome");
                Process[] firefoxDriverProcesses = Process.GetProcessesByName("firefox");
                var processList= chromeDriverProcesses.Concat(firefoxDriverProcesses);
                chromeDriverProcesses =chromeDriverProcesses.OrderByDescending(x => x.StartTime).ToArray();
                foreach (var chromeDriverProcess in processList)
                {
                        chromeDriverProcess.Kill();
                    
                }
               
            }
            catch (Exception hata)
            {
                
                var log = new LoggerTxt();
                log.Log(hata.ToString() + "Chrome ve Firefox processleri kill olmadı veya olmasına rağmen hata basıldı dikkate almayın.");
                return false;
            }


            return true;
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

        private void timer2_Tick(object sender, EventArgs e)
        {
            Task.Run(() =>
            {
                killProcessOfChromeAndFirefox();
            });
        }
    }
}
