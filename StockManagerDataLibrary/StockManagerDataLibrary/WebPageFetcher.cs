using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace StockManagerDataLibrary
{
    class WebPageFetcher
    {
        public static string fetchWebPage(string url)
        {
            CookieContainer CC = new CookieContainer();
            HttpWebRequest wRq = (HttpWebRequest)WebRequest.Create(url);
            wRq.UserAgent = "Mozilla/5.0 (Windows NT 6.1; rv:28.0) Gecko/20100101 Firefox/28.0";
            wRq.Proxy = null;
            wRq.UseDefaultCredentials = true;
            wRq.KeepAlive = false; 
            wRq.ProtocolVersion = HttpVersion.Version10; 
            wRq.CookieContainer = CC;
            WebResponse wRs = wRq.GetResponse();
            Stream strm = wRs.GetResponseStream();
            StreamReader sr = new StreamReader(strm, Encoding.UTF8);
            string s = sr.ReadToEnd();
            sr.Close();
            strm.Close();
            return s;
        }
    }
}
