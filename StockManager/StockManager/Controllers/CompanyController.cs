using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data;
using System.Net;
using System.IO;
using System.Text;

namespace StockManager.Controllers
{
    public class CompanyController : Controller
    {
        //
        // GET: /Company/

        public ActionResult Index()
        {
            ViewBag.show = false;
            string companyName, exchangeName;
            if (Request.Form["SearchButton"] == "Search")
            {
                companyName = Request.Form["companyName"];
                int index = companyName.IndexOf("{") + 1, endIndex = companyName.IndexOf("}") - 1;
                companyName = companyName.Substring(index, endIndex - index + 1);
                endIndex = companyName.IndexOf(":");
                exchangeName = companyName.Substring(0, endIndex);
                companyName = companyName.Substring(endIndex + 1, companyName.Length - endIndex - 1);
            }
            else {
                companyName = Request.QueryString["q"];
                exchangeName = Request.QueryString["ex"];
            }
            ViewBag.companyName = companyName;
            ViewBag.exchangeName = exchangeName;

            string connString = ConfigurationManager.ConnectionStrings["connString"].ConnectionString;
            SqlConnection cn = new SqlConnection(connString);
            cn.Open();
            SqlCommand cm = new SqlCommand("Select LTP,LTPTime,change,percChange,PrevClose,DayOpen,DayHigh,DayLow,Week52High,Week52Low,Volume,C.Name from Company C,StockData S where C.ExchangeName = '" + exchangeName + "' and C.Scrip='" + companyName + "' and C.CID=S.CID", cn);
            SqlDataReader reader = cm.ExecuteReader();
            while (reader.Read())
            {
                ViewBag.LTP = reader[0].ToString();
                ViewBag.LTPTime = reader[1].ToString();
                ViewBag.change = reader[2].ToString();
                ViewBag.percChange = reader[3].ToString();
                ViewBag.prevClose = reader[4].ToString();
                ViewBag.dayOpen = reader[5].ToString();
                ViewBag.dayHigh = reader[6].ToString();
                ViewBag.dayLow = reader[7].ToString();
                ViewBag.week52High = reader[8].ToString();
                ViewBag.week52Low = reader[9].ToString();
                ViewBag.volume = reader[10].ToString();
                ViewBag.companyName2 = reader[11].ToString();
                ViewBag.show = true;
            }
            reader.Close();

            return View();
        }

        #region Historical Data

        public string getHistoricalData()
        {
            string scrip=Request.QueryString[0], index=Request.QueryString[1],interval=Request.QueryString[2],period=Request.QueryString[3];
            return fetchHistoricalDataFromGoogle(interval, "http://www.google.com/finance/getprices?q=" + scrip + "&x=" + ((index=="BSE")?"BOM":index) + "&i=" + interval + "&p=" + period + "&f=d,c&df=cpct&auto=0");
        }

        public string fetchHistoricalDataFromGoogle(string interval, string url)
        {
            string data = fetchWebPage(url);
            int int_interval = Int32.Parse(interval);
            //parse
            string parsedData = "";
            StringReader strReader = new StringReader(data);
            for (int i = 0; i < 7; i++) strReader.ReadLine();
            string line="";
            DateTime date= new DateTime(0),origDate=new DateTime(0);
            string[] words;
            while ((line = strReader.ReadLine()) != null)
            {
                words=line.Split(new char[]{','});
                string dtime=words[0], rate=words[1];
                if (dtime[0] == 'a')
                {
                    date = new DateTime(1970,1,1,0,0,0,DateTimeKind.Utc);
                    long time = Int64.Parse(dtime.Substring(1));
                    date = date.AddSeconds((time) % 60);
                    date = date.AddMinutes((time / 60) % 60);
                    date = date.AddHours((((time / 60) / 60) % 24));
                    date = date.AddDays((((time / 60) / 60) / 24));
                    origDate = date;
                }
                else {
                    long time = int_interval * Int64.Parse(dtime);
                    date = origDate.AddSeconds((time)%60);
                    date = date.AddMinutes((time / 60) % 60);
                    date = date.AddHours((((time / 60) / 60) % 24));
                    date = date.AddDays((((time / 60) / 60) / 24));
                }
                date = date.ToLocalTime();
                parsedData += date.Year + "-" + date.Month + "-" + date.Day + " "+date.Hour+":"+date.Minute+"," + rate + "\n";
            }
            return parsedData;
        }

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
        #endregion
    }
}
