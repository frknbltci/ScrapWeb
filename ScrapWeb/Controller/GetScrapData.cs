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
        public DataResult<List<BahisModel>> GetirBahisleri(HtmlDocument document,string url)
        {
            var modLast = new List<BahisModel>();
            try
            {
                if (document != null)
                {


                    var nodeCount = document.DocumentNode.SelectNodes("//div[contains(@class,'smart-col-item')]/div");
                    if (nodeCount != null && nodeCount.Count > 0)
                    {
                        foreach (HtmlNode Node in nodeCount)
                        {

                            var model = new BahisModel();
                            model.Oranlar = new List<string>();
                            // anaBaslik
                            var Baslik = Node.SelectSingleNode(Node.XPath + "/div[contains(@class,'modul-accordion match-market')]/div[contains(@class,'modul-header')]/span").InnerText;
                            model.Baslik = Baslik;
                            //basligin iceriği modulcontent 
                            var ModuleContent = Node.SelectNodes(Node.XPath + "/div[contains(@class,'modul-accordion match-market')]/div[contains(@class,'modul-content')]/div[contains(@class,'modul-accordion bet-type-group')]");

                            if (ModuleContent != null && ModuleContent.Count > 0)
                            {
                                foreach (HtmlNode htmlNode in ModuleContent)
                                {
                                    var altBaslik = htmlNode.SelectSingleNode(htmlNode.XPath + "/div[contains(@class,'modul-header')]/span").InnerText;


                                    model.Oranlar.Add("Başlık :" + altBaslik);

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


                                }
                            }
                            modLast.Add(model);
                        }

                    }
                    else
                    {
                        LoggerTxt loggerTxt = new LoggerTxt();
                        loggerTxt.Log(Message.BAHISLER_NODE_BULUNAMADI + " Url : " + url);
                    }


                }
                else
                {

                    LoggerTxt loggerTxt = new LoggerTxt();
                    loggerTxt.Log(Message.DOKUMAN_NULL);

                    // return new ErrorDataResult<List<BahisModel>>(Message.VERI_CEKME_BASARISIZ);
                }
            }
            catch (Exception hata)
            {
                LoggerTxt loggerTxt = new LoggerTxt();
                loggerTxt.Log(hata.ToString() + " "+ Message.BAHIS_GETIR_CATCH);
                
            }
            

            return new SuccessDataResult<List<BahisModel>>(modLast, Message.VERI_CEKME_BASARILI);

        }




        public DataResult<List<HtmlDocument>> GetPageSources(List<string> urlList)
        {

            List<HtmlDocument> returnDocuments = new List<HtmlDocument>();
            try
            {


                var driverService = ChromeDriverService.CreateDefaultService();
                driverService.HideCommandPromptWindow = true;
               
               

                var chromeOptions = new ChromeOptions();
                chromeOptions.AddArguments("headless");
                
                
                var path = System.Windows.Forms.Application.StartupPath;
                IWebDriver driver = new ChromeDriver(driverService,chromeOptions);
              
                foreach (var url in urlList)
                {
                    HtmlAgilityPack.HtmlDocument dokuman = new HtmlAgilityPack.HtmlDocument();
                    driver.Navigate().GoToUrl(url);
                    Thread.Sleep(2500);
                    if (!string.IsNullOrEmpty(driver.PageSource))
                    {
                       // var isdata = wait.Until(d => ((IJavaScriptExecutor)d).ExecuteScript("return document.readyState").Equals("complete"));
                        
                            dokuman.LoadHtml(driver.PageSource);
                            dokuman.OptionStopperNodeName = url.ToString();
                        if (!yuklendiMi(dokuman))
                        {
                            driver.Navigate().GoToUrl(url);
                            Thread.Sleep(2500);
                            dokuman.LoadHtml(driver.PageSource);
                           
                        }

                    }
                    else
                    {
                        var log = new LoggerTxt();
                        log.Log(Message.PAGE_SOURCE_BOS);
                    }
                    returnDocuments.Add(dokuman);
                }
                /*
                            var wait = new WebDriverWait(driver, TimeSpan.FromMilliseconds(timeout));
                            wait.Until(d => ((IJavaScriptExecutor)d).ExecuteScript("return document.readyState").Equals("complete"));
                            Thread.Sleep(2000);
                            */

                driver.Quit();
            }
            catch (Exception hata)
            {
                var log = new LoggerTxt();
                log.Log(hata.ToString() + Message.PAGE_SOURCE_TRYCATCH);
                
            }

            
            return new DataResult<List<HtmlDocument>>(returnDocuments, true);
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



        public void WriteFileTxtBahis(List<BahisModel> bahisModel, string url)
        {
            var fileName = "Bahis/" + DateTime.Now.ToString("dd.MM.yyyy") + " "+ url.Substring(url.Length-8);

            if (bahisModel.Count==0)
            {

                File.AppendAllText(fileName, Environment.NewLine + "**************************" + Environment.NewLine);
                File.AppendAllText(fileName, url);
                File.AppendAllText(fileName, DateTime.Now.ToString("dd.MM.yyyy HH:mm"));
                File.AppendAllText(fileName, Environment.NewLine + "**************************" + Environment.NewLine);
            }

            if (bahisModel.Count>0)
            {
                foreach (var item in bahisModel)
                {
                    File.AppendAllText(fileName, Environment.NewLine + "**************************" + Environment.NewLine);
                    File.AppendAllText(fileName, item.Baslik);
                    File.AppendAllText(fileName, DateTime.Now.ToString("dd.MM.yyyy HH:mm"));
                    File.AppendAllText(fileName, Environment.NewLine + "**************************" + Environment.NewLine);

                    foreach (var bahis in item.Oranlar)
                    {
                        if (bahis.Contains("Başlık"))
                        {
                            File.AppendAllText(fileName, Environment.NewLine);
                        }
                        File.AppendAllText(fileName, bahis);

                    }
                }
            }

            
        }


        private bool yuklendiMi(HtmlDocument document)
        {

            //Veri sayfada yok ise 
            bool controlData = document.DocumentNode.SelectSingleNode("//*[@id='container-main']/fixture-detail/message-box/div/div") == null ? true : false;

            if (controlData)
            {
                //data gelmesinin kontrolü

                var controlOfBahisData = document.DocumentNode.SelectNodes("//div[contains(@class,'smart-col-item')]/div");

                if (controlOfBahisData != null && controlOfBahisData.Count>0 )
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                //sayfa geldi veri bilgisi bulunamadı

                return true;

            }
            

   
        }
    }
}
