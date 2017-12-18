using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.IO;
using System.Text;

namespace StockManager.Controllers
{
    public class HomeController : Controller
    {
        //
        // GET: /Home/

        public ActionResult Index()
        {
            /* We can make use of WebSecurity.CurrentUserId
             * More on: http://www.w3schools.com/aspnet/prop_websecurity_currentuserid.asp
             *          http://www.w3schools.com/aspnet/webpages_ref_websecurity.asp
             */

            if (Session["Username"] != null)
                ViewBag.Username = Session["Username"];
            else
                ViewBag.Username = "";
            
            List<string> Company = new List<string>();
            List<Nullable<decimal>> Change = new List<Nullable<decimal>>();
            List<Nullable<decimal>> LTP = new List<Nullable<decimal>>();
            var SMDb = new StockManagerEntities();
            var results = from row in SMDb.Companies
                          join row2 in SMDb.StockDatas on row.CID equals row2.CID
                          orderby row.Name
                          select new { CompanyName = row.Name, LTP = row2.LTP, Change = row2.Change };
            foreach(var reader in results)
            {
                Company.Add(reader.CompanyName);
                Change.Add(reader.Change);
                LTP.Add(reader.LTP);
            }
            ViewBag.Company = Company;
            ViewBag.Change = Change;
            ViewBag.LTP = LTP;
            
            
            ISearchResult searchClass = new GoogleSearch("BSE | NSE | SENSEX | NIFTY");
            var list = searchClass.Search();
            ViewBag.S = list;
            
            return View();
        }

        //
        // GET: /Home/Portfolios

        public ActionResult Portfolios()
        {

            if (Session["Username"] == null)
            {
                return Redirect("../Login");
            }

            var SMDb = new StockManagerEntities();
            //user id
            Nullable<int> userid = new Nullable<int>(Convert.ToInt32(Session["Userid"]));
            var queryResult = from row in SMDb.Portfolios
                              where row.UID == userid
                              select new { Name = row.Name };
            List<string> portfolioList = new List<string>();
            int count = 0;
            foreach(var reader in queryResult)
            {
                portfolioList.Add(reader.Name);
                count++;
            }

            ViewBag.portfolioCount = count;
            ViewBag.portfolioList = portfolioList;

            return View();
        }
        //
        // GET: /Home/PortfolioDetails

        public ActionResult PortfolioDetails()
        {
            if (Session["Username"] == null)
                return Redirect("../Login");
            else
            {
                string portfolioName = "";
                var SMDb = new StockManagerEntities();
                
                if (Request.Form["Portfolio"] == "Add new portfolio")
                {//post
                    portfolioName = Request.Form["PortfolioName"];
                    Portfolio entry = new Portfolio { Name = portfolioName, UID = new Nullable<int>(Convert.ToInt32(Session["Userid"])) };
                    SMDb.Portfolios.Add(entry);
                    List<List<string>> portfolioData = new List<List<string>>();
                    ViewBag.portfolioData = portfolioData;
                }
                else
                {//get
                    portfolioName = Request.QueryString["name"];
                    var pidresult = from row in SMDb.Portfolios
                              where row.Name == portfolioName
                                        select new {PID=row.PID};
                    int pid=-1;
                    foreach(var portfolioid in pidresult)
                    {
                        pid = Convert.ToInt32(portfolioid.PID);
                    }
                    var result = from row in SMDb.PortfolioDatas
                                 join row2 in SMDb.StockDatas on row.CID equals row2.CID
                                 join row3 in SMDb.Companies on row2.CID equals row3.CID
                                 where row.PID == pid
                                 select new { Name = row3.Name, Exchange = row3.ExchangeName, Cost = row.Cost, CurrentHolding= row.CurrentHolding, RealizedProfitLoss = row.RealizedProfitLoss, LTP = row2.LTP };

                    List<List<string>> portfolioData = new List<List<string>>();
                    foreach(var reader in result)
                    {
                        List<string> row = new List<string>();
                        row.Add(reader.Name);
                        row.Add(reader.Exchange+"");
                        row.Add(reader.Cost+"");
                        row.Add(reader.CurrentHolding+"");
                        row.Add(reader.RealizedProfitLoss+"");
                        row.Add(reader.LTP+"");
                        double cost = Double.Parse(row[2]), ltp = Double.Parse(row[5]);
                        int currHold = Int32.Parse(row[3]);
                        row[5] = "" + (ltp - cost) * (double)currHold;
                        portfolioData.Add(row);
                    }
                    ViewBag.portfolioData = portfolioData;
                }
                Session["PortfolioName"] = portfolioName;
                ViewBag.portfolioName = portfolioName;
            }
            return View();
        }

        public ActionResult About()
        {
            return View();
        }

        //
        // GET: /Home/Watchlist
        public ActionResult Watchlist()
        {
            ViewBag.hide = true;
            if (Session["Username"] == null)
                return Redirect("../Login");
            else
            {
                string connString = ConfigurationManager.ConnectionStrings["connString"].ConnectionString;
                SqlConnection cn = new SqlConnection(connString);
                cn.Open();
                SqlCommand cm;
                SqlDataReader reader;

                //var SMDb = new StockManagerEntities();

                bool added = false; // notes down whether a watchlist was added , helps in setting up a name
                ViewBag.alreadyExistsError = false;// indicates error while inserting new watchlist

                if (Request.Form["Watchlist"] == "Add new watchlist")
                {//post
                    string watchlistName = Request.Form["WatchlistName"];
                    //check existence
                    /*int uid =  Convert.ToInt32(Session["Userid"]);
                    var queryResult = from row in SMDb.Watchlists
                                      where row.WLName == watchlistName && row.UID == uid
                                      select new { Name = row.WLName};
                    */
                    cm = new SqlCommand("select count(*) from Watchlist where WLName='" + watchlistName + "' and UID=" + Session["Userid"], cn);
                    SqlDataReader reader2 = cm.ExecuteReader();
                    int count = 0;
                    while (reader2.Read())
                        count = Int32.Parse(reader2[0].ToString());
                    reader2.Close();
                    if (count == 0)
                    {
                        //insert
                        cm = new SqlCommand("Insert into Watchlist(WLName,UID) values ('" + watchlistName + "'," + Session["Userid"] + ")", cn);
                        cm.ExecuteNonQuery();
                        added = true;
                    }
                    else
                        //exists
                        ViewBag.alreadyExistsError = true;
                }
                //load watchlists
                int userid = Int32.Parse(Session["Userid"].ToString());
                cm = new SqlCommand("Select WatchlistID,WLName from Watchlist where UID =" + Session["Userid"], cn);
                reader = cm.ExecuteReader();
                List<int> WatchListID = new List<int>();
                List<string> WatchListName = new List<string>();
                while (reader.Read())
                {
                    WatchListID.Add(Int32.Parse(reader[0].ToString()));
                    WatchListName.Add(reader[1].ToString());
                }
                ViewBag.WatchListID = WatchListID;
                ViewBag.WatchListName = WatchListName;
                //useful to show alert messages
                if (added)
                    ViewBag.addedListName = Request.Form["WatchlistName"];
                else
                    ViewBag.addedListName = "";
                reader.Close();
                cn.Close();
            }
            return View();
        }

        #region Watchlist Related Functions
        //AJAX call handler
        public string GetWatchList()
        {
            if (Request.Params[0].ToString() == "") return "";
            int wid = Int32.Parse(Request.Params[0].ToString());
            Response.Headers["Cache-Control"] = "no-cache, must-revalidate";
            Response.Headers["Pragma"] = "no-cache";
            string connString = ConfigurationManager.ConnectionStrings["connString"].ConnectionString;
            SqlConnection cn = new SqlConnection(connString);
            cn.Open();
            int userid = Int32.Parse(Session["Userid"].ToString());
            List<string> headers = new List<string>();
            headers.Add("Company"); headers.Add("Exchange"); headers.Add("Price"); headers.Add("Change"); headers.Add("% Change"); headers.Add("Day High"); headers.Add("Day Low");
            headers.Add("Open"); headers.Add("Previous Close"); headers.Add("52 Week High"); headers.Add("52 Week Low"); headers.Add("Update Time");
            SqlCommand cm = new SqlCommand("Select C.Name,C.ExchangeName,SD.LTP,SD.Change,SD.PercChange,SD.DayHigh,SD.DayLow,SD.DayOpen,SD.PrevClose,SD.Week52High,SD.Week52Low,SD.LTPTime from WatchlistData WD, StockData SD, Company C where C.CID=WD.CID and WD.CID=SD.CID and WD.WatchlistID =" + wid + " order by 1", cn);
            SqlDataReader reader = cm.ExecuteReader();
            List<List<string>> data = new List<List<string>>();
            List<int> color = new List<int>();
            while (reader.Read())
            {
                List<string> row = new List<string>();
                for (int i = 0; i < headers.Count; i++)
                    row.Add(reader[i].ToString());
                data.Add(row);
                if (Double.Parse(row[3]) < 0) color.Add(0);
                else if (Double.Parse(row[3]) > 0) color.Add(1);
                else
                    color.Add(2);
            }
            reader.Close();
            cn.Close();
            return ConvertDataToHTMLTable(headers, data, color);
        }

        private string ConvertDataToHTMLTable(List<string> headers, List<List<string>> data, List<int> color)
        {
            string htmlCode = "<table id=\"stocktable\"><tr>";
            int[][] colorVal = new int[][] { new int[] { 255, 125, 125 }, new int[] { 125, 255, 125 }, new int[] { 255, 255, 255 } };
            string[] clr = new string[] { "red", "green", "white" };
            foreach (string name in headers)
            {
                htmlCode += "<th>" + name + "</th>";
            }
            htmlCode += "</tr>";
            int r = 0;
            foreach (List<string> row in data)
            {
                htmlCode += "<tr>";
                int column = 0;
                foreach (string val in row)
                {
                    string cssclass = "";
                    if (clr[color[r]] == "red")
                        cssclass = " class=wldown";
                    else if (clr[color[r]] == "green")
                        cssclass = " class=wlup";
                    if (column == 2)
                        htmlCode += "<td" + cssclass + ">" + val + "</td>";
                    else if (column == 1)
                        htmlCode += "<td name=\"exchange\">" + val + "</td>";
                    else if (column == 0)
                        htmlCode += "<td name=\"company\">" + val + "</td>";
                    else if (column > 4)
                        htmlCode += "<td>" + val + "</td>";
                    else //column == 3
                        htmlCode += "<td style=\"background-color:rgb(" + colorVal[color[r]][0] + "," + colorVal[color[r]][1] + "," + colorVal[color[r]][2] + "\"))>" + val + "</td>";
                    column++;
                }
                htmlCode += "</tr>";
                r++;
            }
            //htmlCode += "<tr><a href=\""++"\"></a></tr>"; code for delete company
            htmlCode += "</table>";
            return htmlCode;
        }

        //
        //
        [HttpPost]
        public string AddToWatchlist()
        {
            string companyName = Request.Params[0], watchlistname = Request.Params[1];
            if (companyName == "" || watchlistname == "")
                return "Enter company name";
            if (companyName.IndexOf("{") == -1 || companyName.IndexOf("}") == -1)
                return "Enter valid company name";
            int index = companyName.IndexOf("{") + 1, endIndex = companyName.IndexOf("}") - 1;
            companyName = companyName.Substring(index, endIndex - index + 1);
            if (companyName.IndexOf(":") == -1)
                return "Enter valid company name";
            endIndex = companyName.IndexOf(":");
            string exchange = companyName.Substring(0, endIndex);
            string scrip = companyName.Substring(endIndex + 1, companyName.Length - endIndex - 1);
            string connString = ConfigurationManager.ConnectionStrings["connString"].ConnectionString;
            SqlConnection cn = new SqlConnection(connString);
            cn.Open();
            SqlCommand cm = new SqlCommand("Select CID from Company where ExchangeName = '" + exchange + "' and Scrip='" + scrip + "'", cn);
            SqlDataReader reader = cm.ExecuteReader();
            int count = 1, cid = -1;
            while (reader.Read())
            {
                cid = Int32.Parse(reader[0].ToString());
            }
            reader.Close();
            if (cid == -1)
                return "No such company";
            int wid = Int32.Parse(watchlistname);
            cm = new SqlCommand("Select count(*) from WatchlistData where watchlistID = " + wid + " and CID=" + cid, cn);
            reader = cm.ExecuteReader();
            while (reader.Read())
            {
                count = Int32.Parse(reader[0].ToString());
            }
            reader.Close();
            if (count == 0)
            {
                cm = new SqlCommand("insert into WatchlistData(WatchlistId,CID) values(" + wid + "," + cid + ")", cn);
                cm.ExecuteNonQuery();
                cn.Close();
                return "";
            }
            else
            {
                cn.Close();
                return "This company is already in the watchlist";
            }
        }

        [HttpPost]
        public string DeleteFromWatchlist()
        {
            string wid = Request.Params[0], companyName = Request.Params[1], exchange = Request.Params[2];
            string connString = ConfigurationManager.ConnectionStrings["connString"].ConnectionString;
            SqlConnection cn = new SqlConnection(connString);
            cn.Open();
            SqlCommand cm = new SqlCommand("Select CID from Company where ExchangeName = '" + exchange + "' and Name='" + companyName + "'", cn);
            SqlDataReader reader = cm.ExecuteReader();
            int cid = -1;
            while (reader.Read())
            {
                cid = Int32.Parse(reader[0].ToString());
            }
            reader.Close();
            cm = new SqlCommand("Delete from WatchlistData where watchlistID = " + wid + " and CID=" + cid, cn);
            cm.ExecuteNonQuery();
            cn.Close();
            return "Successfully deleted";
        }

        [HttpPost]
        public string RenameWatchlist()
        {
            string oldNameOfWatchlist = Request.Params[0], newNameOfWatchlist = Request.Params[1];
            if (oldNameOfWatchlist == newNameOfWatchlist)
                return "New name is same as old name!";
            string connString = ConfigurationManager.ConnectionStrings["connString"].ConnectionString;
            SqlConnection sqlConn = new SqlConnection(connString);
            sqlConn.Open();
            SqlCommand cm = new SqlCommand("Select count(*) from Watchlist where UID=" + Session["Userid"] + " and WLName='" + newNameOfWatchlist + "'", sqlConn);
            SqlDataReader reader = cm.ExecuteReader();
            int count = 0;
            while (reader.Read())
            {
                count = Int32.Parse(reader[0].ToString());
            }
            reader.Close();
            if (count != 0)
                return "There exists a watchlist with the same name!";
            cm = new SqlCommand("update watchlist set WLName='" + newNameOfWatchlist + "' where UID=" + Session["Userid"] + " and WLName='" + oldNameOfWatchlist + "'", sqlConn);
            cm.ExecuteNonQuery();
            sqlConn.Close();
            return "Successfully changed the name";
        }

        [HttpPost]
        public string DeleteWatchlist()
        {
            string watchlistID = Request.Params[0];
            string connString = ConfigurationManager.ConnectionStrings["connString"].ConnectionString;
            SqlConnection sqlConn = new SqlConnection(connString);
            sqlConn.Open();
            SqlCommand cm = new SqlCommand("delete from WatchlistData where WatchlistID=" + watchlistID, sqlConn);
            cm.ExecuteNonQuery();
            cm = new SqlCommand("delete from Watchlist where WatchlistID=" + watchlistID, sqlConn);
            cm.ExecuteNonQuery();
            sqlConn.Close();
            return "Successfully deleted watchlist!";
        }

        #endregion

        #region Portfolio Related Functions
        //
        //
        [HttpPost]
        public string AddTransactionToPortfolio()
        {
            string companyName = Request.Params[0], op = Request.Params[1];
            string s = Request.Params[2];
            int quantity = Int32.Parse(Request.Params[2]);
            double price = Double.Parse(Request.Params[3]);
            string datetime = Request.Params[4].Replace("/", "-") + ":00.000";
            if (companyName.IndexOf("{") == -1 || companyName.IndexOf("}") == -1)
                return "Enter valid company name";
            int index = companyName.IndexOf("{") + 1, endIndex = companyName.IndexOf("}") - 1;
            companyName = companyName.Substring(index, endIndex - index + 1);
            if (companyName.IndexOf(":") == -1)
                return "Enter valid company name";
            endIndex = companyName.IndexOf(":");
            string exchange = companyName.Substring(0, endIndex);
            string scrip = companyName.Substring(endIndex + 1, companyName.Length - endIndex - 1);
            string connString = ConfigurationManager.ConnectionStrings["connString"].ConnectionString;
            SqlConnection cn = new SqlConnection(connString);
            cn.Open();
            SqlCommand cm = new SqlCommand("Select CID from Company where ExchangeName = '" + exchange + "' and Scrip='" + scrip + "'", cn);
            SqlDataReader reader = cm.ExecuteReader();
            int cid = -1;
            while (reader.Read())
            {
                cid = Int32.Parse(reader[0].ToString());
            }
            reader.Close();
            if (cid == -1)
                return "No such company";
            int portfolioID = 0;
            cm = new SqlCommand("Select PID from Portfolio where Name = '" + Session["portfolioName"] + "' and UID=" + Session["Userid"], cn);
            reader = cm.ExecuteReader();
            while (reader.Read())
            {
                portfolioID = Int32.Parse(reader[0].ToString());
            }
            reader.Close();
            //get current info
            int currentHoldings = 0;
            double cost = 0, realizedProfitLoss = 0;
            cm = new SqlCommand("Select Cost, CurrentHolding, RealizedProfitLoss from PortfolioData where PID = " + portfolioID + " and CID=" + cid, cn);
            reader = cm.ExecuteReader();
            while (reader.Read())
            {
                cost = Double.Parse(reader[0].ToString());
                currentHoldings = Int32.Parse(reader[1].ToString());
                realizedProfitLoss = Double.Parse(reader[2].ToString());
            }
            reader.Close();

            //check if buy or sell
            if (op == "Buy")
            {
                //make entry for buy
                double newCost = (cost * currentHoldings + price * quantity) / (double)(currentHoldings + quantity);
                currentHoldings += quantity;
                cm = new SqlCommand("insert into [Transaction](TransID,[Type],NoOfShares,PricePerShare,PID,CID,[DateTime]) values(" + DateTime.Now.Ticks % 1007 + ",'Buy'," + quantity + "," + price + "," + portfolioID + "," + cid + ",'" + datetime + "')", cn);
                cm.ExecuteNonQuery();
                //update
                if (cost == 0)
                {
                    cm = new SqlCommand("insert into portfoliodata(PID,CID,Cost,CurrentHolding,RealizedProfitLoss,UnrealizedProfitLoss) values(" + portfolioID + "," + cid + "," + price + "," + quantity + ",0,0)", cn);
                    cm.ExecuteNonQuery();
                }
                else
                {
                    cm = new SqlCommand("update portfoliodata set Cost =" + newCost + ", currentHolding=" + currentHoldings + " where PID=" + portfolioID + " and CID=" + cid, cn);
                    cm.ExecuteNonQuery();
                }
            }
            else
            {
                //check feasibility for sell
                if (currentHoldings < quantity)
                    return "You cannot sell more than current holding value.";
                //calc
                double newprofit = (price * quantity - cost * quantity);
                currentHoldings -= quantity;
                cm = new SqlCommand("insert into [transaction](TransID,[Type],NoOfShares,PricePerShare,PID,CID,[DateTime]) values(" + DateTime.Now.Ticks % 1007 + ",'Sell'," + quantity + "," + price + "," + portfolioID + "," + cid + ",'" + datetime + "')", cn);
                cm.ExecuteNonQuery();
                //update
                cm = new SqlCommand("update portfoliodata set realizedProfitLoss=" + (realizedProfitLoss + newprofit) + ", currentHolding=" + currentHoldings + " where PID=" + portfolioID + " and CID=" + cid, cn);
                cm.ExecuteNonQuery();

            }
            return "Successfully added transaction.";
        }
        #endregion







        public struct SearchType
        {
            public string url;
            public string title;
            public string content;
            public FindingEngine engine;
            public enum FindingEngine { Google, Bing, GoogleAndBing };
        }

        public interface ISearchResult
        {
            SearchType.FindingEngine Engine { get; set; }
            string SearchExpression { get; set; }
            List<SearchType> Search();
        }

        public class BingSearch : ISearchResult
        {
            public BingSearch(string searchExpression)
            {
                this.Engine = SearchType.FindingEngine.Bing;
                this.SearchExpression = searchExpression;
            }
            public SearchType.FindingEngine Engine { get; set; }
            public string SearchExpression { get; set; }

            public List<SearchType> Search()
            {
                // our appid from bing - 3F4313687C37F1....23A79E181D0A25
                const string urlTemplate = @"http://api.search.live.net/json.aspx?AppId=3F4313687C37F1...23A79E181D0A25& \
                                            Market=en-US&Sources=Web&Adult=Strict&Query={0}&Web.Count=50";
                const string offsetTemplate = "&Web.Offset={1}";
                var resultsList = new List<SearchType>();
                int[] offsets = { 0, 50, 100, 150 };
                Uri searchUrl;
                foreach (var offset in offsets)
                {
                    if (offset == 0)
                        searchUrl = new Uri(string.Format(urlTemplate, SearchExpression));
                    else
                        searchUrl = new Uri(string.Format(urlTemplate + offsetTemplate,
                                            SearchExpression, offset));

                    var page = new WebClient().DownloadString(searchUrl);
                    var o = (JObject)JsonConvert.DeserializeObject(page);

                    var resultsQuery = from result in o["SearchResponse"]["Web"]["Results"].Children()
                                       select new SearchType
                                       {
                                           url = result.Value<string>("Url").ToString(),
                                           title = Regex.Replace((HttpUtility.HtmlDecode(result.Value<string>("title").ToString())), "<[^>]+>", "").ToString(),
                                           content = Regex.Replace((result.Value<string>("content").ToString()), "<[^>]+>", "").ToString(),
                                           engine = this.Engine
                                       };

                    resultsList.AddRange(resultsQuery);
                }
                return resultsList;
            }
        }

        public class GoogleSearch : ISearchResult
        {
            public GoogleSearch(string searchExpression)
            {
                this.Engine = SearchType.FindingEngine.Google;
                this.SearchExpression = searchExpression;
            }
            public SearchType.FindingEngine Engine { get; set; }
            public string SearchExpression { get; set; }

            public List<SearchType> Search()
            {
                const string urlTemplate = @"http://ajax.googleapis.com/ajax/services/search/web?v=1.0& \
                                        rsz=large&safe=active&q={0}&start={1}&tbm=nws";
                var resultsList = new List<SearchType>();
                int[] offsets = { 0, 8, 16, 24, 32, 40, 48 };
                foreach (var offset in offsets)
                {
                    WebClient w = new WebClient();
                    var searchUrl = new Uri(string.Format(urlTemplate, SearchExpression, offset));

                    CookieContainer CC = new CookieContainer();
                    HttpWebRequest wRq = (HttpWebRequest)WebRequest.Create(searchUrl);
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

                    var page = s;// w.DownloadString(searchUrl);
                    var o = (JObject)JsonConvert.DeserializeObject(page);

                    var resultsQuery =
                      from result in o["responseData"]["results"].Children()
                      select new SearchType
                      {
                          url = result.Value<string>("url").ToString(),
                          title = Regex.Replace((HttpUtility.HtmlDecode(result.Value<string>("title").ToString())), "<[^>]+>", "").ToString(),
                          content = Regex.Replace(HttpUtility.HtmlDecode((result.Value<string>("content").ToString())), "<[^>]+>", "").ToString(),
                          engine = this.Engine
                      };

                    resultsList.AddRange(resultsQuery);
                }
                return resultsList;
            }
        }
    }
}