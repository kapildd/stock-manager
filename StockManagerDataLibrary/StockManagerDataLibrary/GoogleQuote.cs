using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockManagerDataLibrary
{
    class GoogleQuote
    {
        public string id { get; set; }
        public string s { get; set; }
        public string t { get; set; }
        public string e { get; set; }
        public string l_cur { get; set; }
        public string ltt { get; set; }
        public string lt { get; set; }
        public string ccol { get; set; }
        public string eo { get; set; }
        public string delay { get; set; }
        public string mc { get; set; }
        public string beta { get; set; }
        public string vo { get; set; }
        public string shares { get; set; }
        public string inst_own { get; set; }
        public string name { get; set; }
        public string type { get; set; }
        public string l { get; set; }
        public string l_fix { get; set; }
        public string c { get; set; }
        public string c_fix { get; set; }
        public string cp { get; set; }
        public string cp_fix { get; set; }
        public string pcls_fix { get; set; }
        public string op { get; set; }
        public string hi { get; set; }
        public string lo { get; set; }
        public string avvo { get; set; }
        public string hi52 { get; set; }
        public string lo52 { get; set; }
        public string pe { get; set; }
        public string fwpe { get; set; }
        public string eps { get; set; }

        public Quote getQuote()
        {
            Quote q = new Quote();
            q.setCompanyName(name);
            q.setLTP(Double.Parse(l));
            string s = lt.Remove(lt.IndexOf("GMT"));
            q.setLTPTime(s);
            q.setChange(Double.Parse(c));
            q.setPercChange(Double.Parse(cp));
            q.set_52WeekHigh(Double.Parse(hi52));
            q.set_52WeekLow(Double.Parse(lo52));
            q.setDayHigh(Double.Parse(hi));
            q.setDayLow(Double.Parse(lo));
            q.setDayOpen(Double.Parse(op));
            q.setVolume(vo);
            q.setPrevClose(Double.Parse(pcls_fix));
            return q;
        }
    }
}
