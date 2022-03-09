using HtmlAgilityPack;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Firefox;
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


        
        public DataResult<List<string>> GetirMusabakaLinkeri(List<string> urlList)
        {
            //try catch

            List<string> newUrlList = new List<string>();
            var driverService = FirefoxDriverService.CreateDefaultService(System.Windows.Forms.Application.StartupPath);
            driverService.HideCommandPromptWindow = true;
            var firefoxOptions = new FirefoxOptions();
            firefoxOptions.AddArgument("-headless");
            IWebDriver driver = new FirefoxDriver(driverService, firefoxOptions);

            try
            {
                bool isbottom = false;
               
                List<HtmlDocument> returnDocuments = new List<HtmlDocument>();
              
               // chromeOptions.BinaryLocation= System.Windows.Forms.Application.StartupPath+"\\Log";
                        
                
                IJavaScriptExecutor js = (IJavaScriptExecutor)driver;


                //Urller içerisinden bottom kaldırılıp maçların esas linkleri alınacak sonra işleme dahil edilecek

                foreach (var url in urlList)
                {
                    HtmlAgilityPack.HtmlDocument dokuman = new HtmlAgilityPack.HtmlDocument();
                    driver.Navigate().GoToUrl(url);
                    Thread.Sleep(2500);
                    if (!string.IsNullOrEmpty(driver.PageSource))
                    {
                        isbottom = dokuman.DocumentNode.SelectSingleNode("//*[@id='allCnt']/app-bottom-menu/div[1]") != null ? true : false;
                        if (isbottom)
                        {
                            var bottom = driver.FindElement(By.XPath("//*[@id='allCnt']/app-bottom-menu/div[1]"));
                            js.ExecuteScript("arguments[0].style.display = 'none';", bottom);
                        }

                        dokuman.LoadHtml(driver.PageSource);
                        dokuman.OptionStopperNodeName = url.ToString();

                        var matches = dokuman.DocumentNode.SelectNodes("//div[contains(@class,'fixture-container')]//div[contains(@class,'match-content')]");

                        if (matches != null && matches.Count > 0)
                        {
                            foreach (HtmlNode item in matches)
                            {
                                //her maçın istediğimiz formattıki Url'ini almak için 
                                var MatchUrl = item.SelectSingleNode(item.XPath + "/a").GetAttributes("href");
                                if (MatchUrl != null)
                                {
                                    var fullUrl = url.Replace("m.", "");
                                    var uri = new Uri(fullUrl);
                                    var klasorName = uri.Segments[uri.Segments.Length - 2] + uri.Segments[uri.Segments.Length - 1].ToString();
                                    var editKlasor = !string.IsNullOrEmpty(klasorName) ? klasorName.Replace("/", "-") : "klasornameDonmedi" ;
                                    var newUrl = uri.Host + MatchUrl.FirstOrDefault().Value;
                                    newUrlList.Add(newUrl+"*"+klasorName);

                                }
                                else
                                {
                                    var log = new LoggerTxt();
                                    log.Log(Message.GETIR_MUSABAKALARI_MACIN_URLSI_HATA + " " + url.ToString());
                                }
                            }
                        }
                        else
                        {
                            // errror basacak maçlar gelmedi  link arzıla veya maç listelenmemiş
                            var log = new LoggerTxt();
                            log.Log(Message.LINK_ARIZALI_VEYA_MACYOK + " " + url.ToString());
                        }

                    }
                    else
                    {
                        var log = new LoggerTxt();
                        log.Log(Message.PAGE_SOURCE_BOS_02);
                    }

                }



                driver.Quit();
                return new DataResult<List<string>>(newUrlList, true);
            }
            catch (Exception hata)
            {
                driver.Quit();
                var log = new LoggerTxt();
                log.Log(hata.ToString() + Message.GETIR_MUSABAKALARI_TRYCATCH);
            }
            finally
            {
                driver.Quit();
            }

            return new DataResult<List<string>>(new List<string>(), false);
        }



        public DataResult<List<HtmlDocument>> GetPageSources(List<string> urlList)
        {

            var driverService = ChromeDriverService.CreateDefaultService(System.Windows.Forms.Application.StartupPath);
            driverService.HideCommandPromptWindow = true;


            var chromeOptions = new ChromeOptions();
            chromeOptions.AddArgument("headless");

            IWebDriver driver2 = new ChromeDriver(driverService, chromeOptions);
            List<HtmlDocument> returnDocuments = new List<HtmlDocument>();
            try
            {
               
              
                foreach (var url in urlList)
                {
                    HtmlAgilityPack.HtmlDocument dokuman = new HtmlAgilityPack.HtmlDocument();

                    var data = url.Split('*')[0].ToString();
                    driver2.Navigate().GoToUrl("https://" + url.Split('*')[0]);
                
                   
                    Thread.Sleep(1500);
                    if (!string.IsNullOrEmpty(driver2.PageSource))
                    {
                       // var isdata = wait.Until(d => ((IJavaScriptExecutor)d).ExecuteScript("return document.readyState").Equals("complete"));
                        
                            dokuman.LoadHtml(driver2.PageSource);
                            dokuman.OptionStopperNodeName = url.ToString(); //.Split('*')[0].ToString();
                        
                        
                          if (!yuklendiMi(dokuman))
                           {
                               driver2.Navigate().GoToUrl("https://" +url.Split('*')[0].ToString());
                               Thread.Sleep(1500);
                               dokuman.LoadHtml(driver2.PageSource);
                               dokuman.OptionStopperNodeName = url.ToString();

                        }

                    }
                    else
                    {
                        var log = new LoggerTxt();
                        log.Log(Message.PAGE_SOURCE_BOS);
                    }
                    returnDocuments.Add(dokuman);
                }
            

                driver2.Quit();
            }
            catch (Exception hata)
            {
                 driver2.Quit();
                var log = new LoggerTxt();
                log.Log(hata.ToString() + Message.PAGE_SOURCE_TRYCATCH);
                
            }
            finally
            {
                driver2.Quit();
            }

            
            return new DataResult<List<HtmlDocument>>(returnDocuments, true);
        }


       

        /// <summary>
        /// Url txt den verilen linkleri alır 
        /// </summary>
        /// <returns></returns>
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
            var directory = url.Split('*')[1].ToString();

            if (!Directory.Exists("Bahis/"+directory))
            {
                Directory.CreateDirectory("Bahis/" + directory);
            }
            var fileName = "Bahis/"+directory +"/" + DateTime.Now.ToString("dd.MM.yyyy") + " "+ url.Split('*')[0].ToString().Substring(url.Split('*')[0].ToString().Length-8);

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
