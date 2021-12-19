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

            if (dakikaAl.Value>0)
            {
                timer1.Interval = (int)dakikaAl.Value;
                timer1.Enabled = true;
            }
            else
            {
                //timer setlenmemiş ise 5 dakikada bir.
                timer1.Interval = 5000;
                timer1.Enabled = true;
            }

            GetScrapData get = new GetScrapData();


            var urlList= get.GetUrlFromFile();

            if (urlList.Success && urlList.Data.Count>0)
            { 
               var pageSources = get.GetPageSources(urlList.Data);

                if (pageSources.Success && pageSources.Data.Count>0 )
                {
                    foreach (var item in pageSources.Data)
                    {

                        //kendim arkada url gönderiyorum bir alan içinde
                        var bahisler = get.GetirBahisleri(item,item.OptionStopperNodeName);
                        if (bahisler.Data.Count>0)
                        {
                            get.WriteFileTxtBahis(bahisler.Data, item.OptionStopperNodeName);
                        }
                        else
                        {
                            get.WriteFileTxtBahis(new List<Model.BahisModel>(),item.OptionStopperNodeName+" "+ Constants.Message.URL_BAHISLERI_DONMEDI);
                        }

                    }

                }

            }


        }

        private void timer1_Tick(object sender, EventArgs e)
        {

        }

        
        

        private void button1_Click(object sender, EventArgs e)
        {
            if (dakikaAl.Value>0)
            {
                timer1.Interval= (int)TimeSpan.FromMinutes((double)dakikaAl.Value).TotalMilliseconds;
            } 
        }
    }
}
