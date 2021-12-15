using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScrapWeb.Constants
{
   public class Message
    {
        public static string VERI_CEKME_BASARILI = "Veriler başarıyla çekilmiştir.";

        public static string VERI_CEKME_BASARISIZ = "Veriler çekilirken bir hata ile karşılaşıldı.Logları inceleyin.";

        public static string URL_BASARILI_MESAJI = "Urller başarılı bir şekilde gelmiştir.";

        public static string URL_BASARISIZ_MESAJI = "Urller okunamadı veya url.txt boş. (GetScrapData > GetUrlFromFile)";

        public static string URL_BASARISIZ_CATCH_MESAJI = "Urller okunamadı CATCH'E TAKILDI. (GetScrapData > GetUrlFromFile)";



    }
}
