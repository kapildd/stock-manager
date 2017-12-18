using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;
using System.IO;
using Newtonsoft.Json;
using Json;

namespace StockManagerDataLibrary
{
    abstract class QuoteFetcher
    {
        public Quote getQuote(string index, string scrip)
        {
            return formatData(WebPageFetcher.fetchWebPage(getDownloadLink(index, scrip)));
        }
        public List<Quote> getQuoteMultiple(List<string> index, List<string> scrip)
        {
            return formatDataMultiple(WebPageFetcher.fetchWebPage(getDownloadLinkMultiple(index, scrip)));
        }
        public abstract string getDownloadLink(string index, string scrip);
        public abstract string getDownloadLinkMultiple(List<string> index, List<string> scrip);
        public abstract Quote formatData(string data);
        public abstract List<Quote> formatDataMultiple(string data);
    }


    class SourceTwo : QuoteFetcher
    { // Google Finance
        public override string getDownloadLink(string index, string scrip)
        {
            string link = "http://www.google.com/finance/info?infotype=infoquoteall&q=";
            if (index.Equals("BSE")) link += "BOM:" + scrip;
            else if (index.Equals("NSE")) link += "NSE:" + scrip;
            else return "";
            return link;
        }
        public override string getDownloadLinkMultiple(List<string> index, List<string> scrip)
        {
            string link = "http://www.google.com/finance/info?infotype=infoquoteall&q=";
            link += scrip[0];
            for (int i = 1; i < scrip.Count; i++)
            {
                link += "," + scrip[i];
            }
            Console.WriteLine(link);
            return link;
        }
        public override Quote formatData(string data)
        {
            return formatDataMultiple(data)[0];
        }
        public override List<Quote> formatDataMultiple(string data)
        {
            List<Quote> quoteList = new List<Quote>();
            int last = 0;
            while (last < data.Length)
            {
                int index = data.IndexOf("{", last);
                if (index == -1) break;
                int endIndex = data.IndexOf("}", index + 1);
                last = endIndex + 1;
                Console.WriteLine(data.Substring(index, endIndex - index + 1));
                GoogleQuote stockData = JsonParser.Deserialize<GoogleQuote>(data.Substring(index, endIndex - index + 1).Replace('\n', ' '));
                Console.WriteLine(stockData.l);
                quoteList.Add(stockData.getQuote());
            }
            return quoteList;
        }
    }

    class SourceThree : QuoteFetcher
    { // moneycontrol.com formatter


        public override string getDownloadLinkMultiple(List<string> index, List<string> scrip)
        {
            return "";
        }
        public override List<Quote> formatDataMultiple(string data)
        {
            return new List<Quote>();
        }

        public override string getDownloadLink(string index, string scrip)
        {
            string link = "http://www.moneycontrol.com/mccode/common/get_pricechart_div.php?";
            if (index.Equals("BSE")) link += "bse_id=Y";
            else if (index.Equals("NSE")) link += "nse_id=Y";
            else return "";
            return link += "&sc_id=" + scrip;
        }

        public override Quote formatData(string data)
        {
            Console.WriteLine(data);
            string strippedData = StripHTML(data);
            Console.WriteLine(strippedData);
            Quote q = new Quote();
            StringReader strReader = new StringReader(strippedData);
            strReader.ReadLine();
            strReader.ReadLine();
            q.setLTPTime(strReader.ReadLine());
            q.setLTP(Double.Parse(extractNumbers(strReader.ReadLine())));
            string changeLine = strReader.ReadLine();
            q.setChange(Double.Parse(extractNumbers(changeLine)));
            changeLine = strReader.ReadLine();
            int index = changeLine.IndexOf('(') + 1;
            int endIndex = changeLine.IndexOf("%") - 1;
            q.setPercChange(Double.Parse(extractNumbers(changeLine.Substring(index, endIndex - index + 1))));
            strReader.ReadLine();
            q.setVolume(extractNumbers(strReader.ReadLine()));
            for (int i = 0; i < 15; i++) strReader.ReadLine();
            q.setPrevClose(Double.Parse(extractNumbers(strReader.ReadLine())));
            strReader.ReadLine();
            q.setDayOpen(Double.Parse(extractNumbers(strReader.ReadLine())));
            for (int i = 0; i < 4; i++) strReader.ReadLine();
            string dayHigh, dayLow, _52wkHigh, _52wkLow;
            /*
            string dayHLString = strReader.ReadLine().Substring("Today's Low/High ".Length+1);
            string _52wkHLString = strReader.ReadLine().Substring("52 Wk Low/High ".Length+1);
            dayHighLow = dayHLString.Split(' ');
            _52wkHighLow = _52wkHLString.Split(' ');
            */
            strReader.ReadLine();
            dayLow = strReader.ReadLine();
            dayHigh = strReader.ReadLine();
            strReader.ReadLine();
            _52wkLow = strReader.ReadLine();
            _52wkHigh = strReader.ReadLine();
            q.setDayLow(Double.Parse(extractNumbers(dayLow)));
            q.setDayHigh(Double.Parse(extractNumbers(dayHigh)));
            q.set_52WeekLow(Double.Parse(extractNumbers(_52wkLow)));
            q.set_52WeekHigh(Double.Parse(extractNumbers(_52wkHigh)));
            return q;
        }

        private string extractNumbers(string rawData)
        {
            string number = "";
            if (rawData == null) return "";
            int dot = 0;
            for (int i = 0; i < rawData.Length; i++)
            {
                if (isPartOfNumber(rawData[i]))
                    number += rawData[i];
                if (rawData[i] == '.' && dot == 0)
                {
                    number += ".";
                    dot++;
                }
            }
            //if (number[number.Length - 1] == '.')
            //    number += "0";
            return number;
        }

        private bool isPartOfNumber(char c)
        {
            if ((c >= '0' && c <= '9') || (c == '+' || c == '-'))
                return true;
            return false;
        }
        private static string StripHTML(string source)
        {
            //source = System.Text.RegularExpressions.Regex.Replace(source, "<div[^>]*>", "\n");
            string result2 = System.Text.RegularExpressions.Regex.Replace(source, "<[^>]*>", "\n");
            string result = "";
            StringReader strReader = new StringReader(result2);
            string line;
            while ((line = strReader.ReadLine()) != null)
                if (line.Trim().Length > 0)
                    result += line.Trim() + "\n";
            return result;
        }

        public static string ConvertHtml(string html)
        {
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(html);

            StringWriter sw = new StringWriter();
            ConvertTo(doc.DocumentNode, sw);
            sw.Flush();
            return sw.ToString();
        }

        private static void ConvertContentTo(HtmlNode node, TextWriter outText)
        {
            foreach (HtmlNode subnode in node.ChildNodes)
            {
                ConvertTo(subnode, outText);
            }
        }

        public static void ConvertTo(HtmlNode node, TextWriter outText)
        {
            string html;
            switch (node.NodeType)
            {
                case HtmlNodeType.Comment:
                    // don't output comments
                    break;

                case HtmlNodeType.Document:
                    ConvertContentTo(node, outText);
                    break;

                case HtmlNodeType.Text:
                    // script and style must not be output
                    string parentName = node.ParentNode.Name;
                    if ((parentName == "script") || (parentName == "style"))
                        break;

                    // get text
                    html = ((HtmlTextNode)node).Text;

                    // is it in fact a special closing node output as text?
                    if (HtmlNode.IsOverlappedClosingElement(html))
                        break;

                    // check the text is meaningful and not a bunch of whitespaces
                    if (html.Trim().Length > 0)
                    {
                        outText.Write(HtmlEntity.DeEntitize(html));
                    }
                    break;

                case HtmlNodeType.Element:
                    switch (node.Name)
                    {
                        case "p":
                            // treat paragraphs as crlf
                            outText.Write("\r\n");
                            break;
                    }

                    if (node.HasChildNodes)
                    {
                        ConvertContentTo(node, outText);
                    }
                    break;
            }
        }
    }



}
