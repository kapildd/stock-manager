using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockManagerDataLibrary
{
    class Quote
    {
        string companyName;
        double LTP, change, percChange, prevClose, dayOpen, faceValue;
        double dayHigh, dayLow;
        double _52WeekHigh, _52WeekLow;
        string LTPTime, volume;
        public Quote()
        { }

        public Quote(double LTP, double change, double percChange, double prevClose, double faceValue, double dayHigh, double dayLow, double _52WeekHigh, double _52WeekLow, string LTPTime, string companyName)
        {
            this.LTP = LTP;
            this.change = change;
            this.percChange = percChange;
            this.prevClose = prevClose;
            this.faceValue = faceValue;
            this.dayHigh = dayHigh;
            this.dayLow = dayLow;
            this._52WeekHigh = _52WeekHigh;
            this._52WeekLow = _52WeekLow;
            this.LTPTime = LTPTime;
            this.companyName = companyName;
        }

        public string getVolume()
        {
            return volume;
        }

        public void setVolume(string volume)
        {
            this.volume = volume;
        }

        public double getDayOpen()
        {
            return dayOpen;
        }

        public void setDayOpen(double dayOpen)
        {
            this.dayOpen = dayOpen;
        }

        public double getLTP()
        {
            return LTP;
        }

        public void setLTP(double LTP)
        {
            this.LTP = LTP;
        }

        public double getChange()
        {
            return change;
        }

        public void setChange(double change)
        {
            this.change = change;
        }

        public double getPercChange()
        {
            return percChange;
        }

        public void setPercChange(double percChange)
        {
            this.percChange = percChange;
        }

        public double getPrevClose()
        {
            return prevClose;
        }

        public void setPrevClose(double prevClose)
        {
            this.prevClose = prevClose;
        }

        public double getFaceValue()
        {
            return faceValue;
        }

        public void setFaceValue(double faceValue)
        {
            this.faceValue = faceValue;
        }

        public double getDayHigh()
        {
            return dayHigh;
        }

        public void setDayHigh(double dayHigh)
        {
            this.dayHigh = dayHigh;
        }

        public double getDayLow()
        {
            return dayLow;
        }

        public void setDayLow(double dayLow)
        {
            this.dayLow = dayLow;
        }

        public double get_52WeekHigh()
        {
            return _52WeekHigh;
        }

        public void set_52WeekHigh(double _52WeekHigh)
        {
            this._52WeekHigh = _52WeekHigh;
        }

        public double get_52WeekLow()
        {
            return _52WeekLow;
        }

        public void set_52WeekLow(double _52WeekLow)
        {
            this._52WeekLow = _52WeekLow;
        }

        public string getLTPTime()
        {
            return LTPTime;
        }

        public void setLTPTime(string LTPTime)
        {
            this.LTPTime = LTPTime;
        }

        public string getCompanyName()
        {
            return companyName;
        }

        public void setCompanyName(string companyName)
        {
            this.companyName = companyName;
        }

        public string toString()
        {
            return "Quote{" +
                    "companyName='" + companyName + '\'' +
                    ", LTP=" + LTP +
                    ", change=" + change +
                    ", percChange=" + percChange +
                    ", prevClose=" + prevClose +
                    ", faceValue=" + faceValue +
                    ", dayHigh=" + dayHigh +
                    ", dayLow=" + dayLow +
                    ", _52WeekHigh=" + _52WeekHigh +
                    ", _52WeekLow=" + _52WeekLow +
                    ", LTPTime='" + LTPTime + '\'' +
                    '}';
        }
    }
}
