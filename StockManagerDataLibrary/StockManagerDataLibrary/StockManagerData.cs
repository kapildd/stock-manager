using System;
using System.Data;
using System.Data.SqlClient;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace StockManagerDataLibrary
{

    class Scrip 
    {
        string index = "", scrip = "";
        public Scrip(string index, string scrip)
        {
            this.index = index;
            this.scrip = scrip;
        }
        public string getIndex()
        {
            return index;
        }
        public string getScrip()
        {
            return scrip;
        }
    }



    public class StockManagerData
    {
        
        public static void Main()
        {
            StockManagerData smd = new StockManagerData();
            smd.update();
        }

        void GoogleScrip()
        {
            //open connection to DB
            SqlConnection conn = new SqlConnection(@"Data source=.\SQLEXPRESS;Integrated Security=true;DataBase=StockManager;");
            conn.Open();
            string selectStatement = "select * from company";
            SqlCommand selectCommand = new SqlCommand(selectStatement, conn);
            SqlDataReader reader = selectCommand.ExecuteReader();
            List<string> companyScrips = new List<string>();
            List<int> cidList = new List<int>();
            while (reader.Read())
            {
                cidList.Add(Int32.Parse(reader[0].ToString()));
                if (reader[3].ToString().Equals("NSE"))
                    companyScrips.Add("NSE:" + reader[2].ToString());
                else
                    companyScrips.Add("BOM:" + reader[2].ToString());
            }
            reader.Close();
            SqlCommand insertCommand = new SqlCommand("insert into ScripMapper(CID,SourceID,ScripCode) values (@CID,2,@Scrip)", conn);
            insertCommand.Parameters.Add("@CID", SqlDbType.Int);
            insertCommand.Parameters.Add("@Scrip", SqlDbType.VarChar);
            for (int i = 0; i < companyScrips.Count; i++)
            {
                insertCommand.Parameters["@CID"].Value = cidList[i];
                insertCommand.Parameters["@Scrip"].Value = companyScrips[i];
                insertCommand.ExecuteNonQuery();
            }
            conn.Close();
        }

        void update()
        { 
            //get list of cids
            List<int> cidList = getListOfCids();
            //source moneycontrol
            SourceThree source3=new SourceThree();
            //source google finance
            SourceTwo source2 = new SourceTwo();
            new Thread(delegate()
            {
                    foreach (int cid in cidList)
                    {
                        try
                        {
                        //get scrip code
                        Scrip scripCode = getScripFor(3, cid);
                        //download
                        Quote q = source3.getQuote(scripCode.getIndex(), scripCode.getScrip());
                        //update db
                        updateDatabase(cid, q);
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e);
                        }
                    }
            }).Start();
            
            
            new Thread(delegate()
            {
                int marked = 0;
                while (marked < cidList.Count)
                {
                    List<string> scripList = new List<string>();
                    for (int i = 0; i < 100 && marked < cidList.Count; i++, marked++)
                    {
                        int cid = cidList[marked];
                        Scrip s = getScripFor(2, cid);
                        scripList.Add(s.getScrip());
                        Console.WriteLine(scripList[scripList.Count - 1]);
                    }
                    try
                    {
                        List<Quote> list = source2.getQuoteMultiple(new List<string>(), scripList);
                        for (int i = 0; i < scripList.Count; i++)
                            updateDatabase(cidList[marked - i - 1], list[scripList.Count - i - 1]);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }
                    System.Threading.Thread.Sleep(new Random(new DateTime().Millisecond).Next(6000, 14000));
                }
            }).Start();
            
        }



        private void updateDatabase(int cid, Quote q)
        {
            //open connection to DB
            SqlConnection conn = new SqlConnection(@"Data source=.\SQLEXPRESS;Integrated Security=true;DataBase=StockManager;");
            conn.Open();

            //update all values
            string selectStatement = "update stockData set LTP="+q.getLTP()+",LTPTime='"+q.getLTPTime()+"', change="+q.getChange()+", percChange="+q.getPercChange()+", dayOPen="+q.getDayOpen()+", prevClose="+q.getPrevClose()+", dayHigh="+q.getDayHigh()+", dayLow="+q.getDayLow()+", Week52High="+q.get_52WeekHigh()+", Week52Low="+q.get_52WeekLow()+", volume='"+q.getVolume()+"' where cid="+cid;
            SqlCommand selectCommand = new SqlCommand(selectStatement, conn);
            selectCommand.ExecuteNonQuery();
            conn.Close();
        }

        private List<int> getListOfCids()
        {
            //open connection to DB
            SqlConnection conn = new SqlConnection(@"Data source=.\SQLEXPRESS;Integrated Security=true;DataBase=StockManager;");
            conn.Open();

            //get company id
            List<int> cidList = new List<int>();
            int cid = -1;
            string selectStatement = "select CID from Company";
            SqlCommand selectCommand = new SqlCommand(selectStatement, conn);
            SqlDataReader r = selectCommand.ExecuteReader();
            while (r.Read())
            {
                cid = r.GetInt32(0);
                cidList.Add(cid);
            }
            r.Close();
            conn.Close();
            
            return cidList;
        }

        private Scrip getScripFor(int sourceId, int cid)
        {
            Scrip sc;
            
            string scrip="", index="";
            //open connection to DB
            SqlConnection conn = new SqlConnection(@"Data source=.\SQLEXPRESS;Integrated Security=true;DataBase=StockManager;");
            conn.Open();

            //get scrip for source
            string selectStatement = "select scripCode from ScripMapper where CID="+cid+" and SourceID="+sourceId;
            SqlCommand selectCommand = new SqlCommand(selectStatement, conn);
            SqlDataReader r = selectCommand.ExecuteReader();
            while (r.Read())
            {
                scrip = r[0].ToString();
            }
            r.Close();
            //get exchange name
            selectStatement = "select ExchangeName from Company where CID=" + cid;
            selectCommand = new SqlCommand(selectStatement, conn);
            r = selectCommand.ExecuteReader();
            while (r.Read())
            {
                index = r[0].ToString();
            }
            r.Close();
            conn.Close();
            sc = new Scrip(index, scrip.Replace("&","%26"));
            return sc;
        }
    }
}

