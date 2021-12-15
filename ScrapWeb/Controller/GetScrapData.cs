using HtmlAgilityPack;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using ScrapWeb.Constants;
using ScrapWeb.Helper.Log;
using ScrapWeb.Model;
using ScrapWeb.Utilites.Results;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ScrapWeb.Controller
{
    public class GetScrapData
    {

        /// <summary>
        /// Bir url den getirilen bütün sayfa'nın verileri.
        /// </summary>
        /// <param name="document"></param>
        /// <returns></returns>
        public DataResult<List<BahisModel>> GetirBahisleri(HtmlDocument document)
        {
            var modLast = new List<BahisModel>();

            if (document !=null)
            {
                var nodeCount = document.DocumentNode.SelectNodes("//div[contains(@class,'smart-col-item')]/div");
                if (nodeCount.Count > 0)
                {
                    foreach (HtmlNode Node in nodeCount)
                    {

                        var model = new BahisModel();
                        model.Oranlar = new List<string>();
                        // anaBaslik
                        var Baslik = Node.SelectSingleNode(Node.XPath + "/div[contains(@class,'modul-accordion match-market')]/div[contains(@class,'modul-header')]/span").InnerText;
                        model.Baslik = Baslik;
                        //basligin iceriği modulcontent 
                        var ModuleContent = Node.SelectNodes(Node.XPath + "/div[contains(@class,'modul-accordion match-market')]/div[contains(@class,'modul-content')]/div");

                        if (ModuleContent != null && ModuleContent.Count > 0)
                        {
                            foreach (HtmlNode htmlNode in ModuleContent)
                            {
                                var altBaslik = htmlNode.SelectSingleNode(htmlNode.XPath + "/div[contains(@class,'modul-header')]/span").InnerText;

                                model.Oranlar.Add("Baslik : ->" + altBaslik);
                                model.Oranlar.Add(" ");
                                var icModuleCont = htmlNode.SelectNodes(htmlNode.XPath + "/div[contains(@class,'modul-content')]/div[contains(@class,'flex-container bet-type-btn-group')]/a");

                                if (icModuleCont != null && icModuleCont.Count > 0)
                                {
                                    foreach (var item in icModuleCont)
                                    {

                                        //Bahislerin içinde gezip Yazıları ve Oranları aldığımız kısım
                                        var Bahis = item.SelectSingleNode(item.XPath + "/span[contains(@class,'flex-item bet-btn-text')]").InnerText;
                                        var Oran = item.SelectSingleNode(item.XPath + "/span[contains(@class,'bet-btn-odd')]").InnerText;

                                        model.Oranlar.Add(Bahis + " " + Oran);
                                    }
                                }

                                modLast.Add(model);
                            }
                        }

                    }

                }

                return new SuccessDataResult<List<BahisModel>>(modLast, Message.VERI_CEKME_BASARILI);
            }
            else
            {

                //Logg

                return new ErrorDataResult<List<BahisModel>>(Message.VERI_CEKME_BASARISIZ);
            }
        }




        public HtmlDocument GetPageSource(string url)
        {
            HtmlAgilityPack.HtmlDocument dokuman = new HtmlAgilityPack.HtmlDocument();

            //  Thread.Sleep(2000);
            // var data =dokuman.DocumentNode.SelectSingleNode("/html/body/app-root/app-out-component/div[1]/main/app-placebet/div/div/fixture-detail/div/div[2]/div/div/div[1]/div/div[1]/span").InnerText;


            var chromeOptions = new ChromeOptions();
            chromeOptions.AddArguments("headless");

            // var cdRunpath = @"~/chromedrive.exe";
            var path = System.Windows.Forms.Application.StartupPath;
           // var dataX = Path.Combine("C:/Users/pc/source/repos/BetNewScrap/YapılabilirlikScrapBetNew");
            IWebDriver driver = new ChromeDriver(path, chromeOptions);

            Thread.Sleep(2000);
            driver.Navigate().GoToUrl(url);


            var timeout = 3000; /* Maximum wait time of 10 seconds */
            var wait = new WebDriverWait(driver, TimeSpan.FromMilliseconds(timeout));
            wait.Until(d => ((IJavaScriptExecutor)d).ExecuteScript("return document.readyState").Equals("complete"));

            Thread.Sleep(2000);
            dokuman.LoadHtml(driver.PageSource);

            return dokuman;
        }


        public DataResult<List<string>> GetUrlFromFile()
        {

            string path = System.Windows.Forms.Application.StartupPath + "\\url.txt";
            var urlListesi = new List<string>();

            try
            {
                
                if (File.ReadAllLines(path).Length > 0)
                {
                    foreach (var item in File.ReadAllLines(path))
                    {
                        urlListesi.Add(item.Trim().ToString());
                    }

                }
                else
                {
                    var log = new LoggerTxt();
                    log.Log(Message.URL_BASARISIZ_MESAJI);
                }

                

            }
            catch (Exception hata)
            {
                var log = new LoggerTxt();
                log.Log(hata.ToString() + Message.URL_BASARISIZ_CATCH_MESAJI);
                return new ErrorDataResult<List<string>>(urlListesi, Message.URL_BASARISIZ_CATCH_MESAJI);
              
            }
            return new DataResult<List<string>>(urlListesi, true, Message.URL_BASARILI_MESAJI);

        }

    }
}
