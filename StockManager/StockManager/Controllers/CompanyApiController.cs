using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace StockManager.Controllers
{
    public class CompanyApiController : ApiController
    {
        [HttpGet]
        public IEnumerable<string> GetCompanies(string query = "")
        {
            string connString = ConfigurationManager.ConnectionStrings["connString"].ConnectionString;
            SqlConnection cn = new SqlConnection(connString);
            cn.Open();
            SqlCommand cmd = new SqlCommand("select Name,ExchangeName,Scrip from Company where name like '%" + query + "%' or scrip like '%" + query + "%'", cn);
            SqlDataReader reader = cmd.ExecuteReader();
            List<string> companyList=new List<string>();
            while (reader.Read())
            {
                companyList.Add(reader[0].ToString()+" {"+reader[1].ToString()+":"+reader[2].ToString()+"}");
            }
            reader.Close();
            cn.Close();
            return companyList;
        }


    }
}
