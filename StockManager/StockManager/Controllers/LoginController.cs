using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.SqlClient;
using System.Data;
using System.Configuration;

namespace StockManager.Controllers
{
    public class LoginController : Controller
    {
        //
        // GET: /Login/

        public ActionResult Index()
        {
            return View();
        }
        
        //
        // POST: /Login/Authenticate
        [HttpPost]
        public ActionResult Authenticate()
        {
            string username = Request.Form["username"], password = Request.Form["password"];
            string userid="";
            string connString = ConfigurationManager.ConnectionStrings["connString"].ConnectionString; 
            SqlConnection cn = new SqlConnection(connString);
            cn.Open();
            SqlCommand cm = new SqlCommand("select userid from UserInfo where username='"+username+"'",cn);
            SqlDataReader reader = cm.ExecuteReader();
            while (reader.Read())
            {
                userid = reader[0].ToString();
            }
            reader.Close();
            cm = new SqlCommand("select password from LoginInfo where userid='" + userid + "'", cn);
            reader = cm.ExecuteReader();

            if (reader.Read() && reader[0].ToString().Equals(password))
            {
                ViewBag.Message = "Success.";
                ViewBag.User = username;
                cn.Close();
                Session["Username"] = username;
                Session["Userid"] = userid;
                return Redirect("../Home");
            }
            else
            {
                ViewBag.Message = "Invalid username or password.";
                ViewBag.User = "Invalid ";
                cn.Close();
                return View();
            }
        }

        public ActionResult Logout()
        {
            Session["Username"] = null;
            return Redirect("../Home");
        }

        public ActionResult Register()
        {

            if (Request.Form["Register"] == "Register")
            {
                string username = Request.Form["username"], password = Request.Form["password"], rpassword = Request.Form["rpassword"];
                string email = Request.Form["email"];
                bool valid = true;
                string connString = ConfigurationManager.ConnectionStrings["connString"].ConnectionString;
                SqlConnection cn = new SqlConnection(connString);
                cn.Open();
                //unique username
                SqlCommand cmd = new SqlCommand("select count(*) from UserInfo where username='"+username+"'",cn);
                SqlDataReader reader = cmd.ExecuteReader();
                int countUsers = 0;
                while (reader.Read())
                {
                    countUsers = Int32.Parse(reader[0].ToString());
                }
                if (countUsers > 0)
                {
                    valid = false;
                    ViewBag.UserName = username;
                    ViewBag.UsernameNotUnique = "Username not available.";
                }
                else
                {
                    ViewBag.UsernameNotUnique = "";
                }
                reader.Close();
                //password match
                if (password.Equals(rpassword))
                {
                    ViewBag.PasswordMismatch = "";
                }
                else
                {
                    ViewBag.PasswordMismatch = "Passwords do not match";
                    valid = false;
                }
                //unique email
                cmd = new SqlCommand("select count(*) from UserInfo where email='" + email + "'", cn);
                reader = cmd.ExecuteReader();
                int countEmail = 0;
                while (reader.Read())
                {
                    countEmail = Int32.Parse(reader[0].ToString());
                }
                reader.Close();
                if (countEmail > 0)
                {
                    valid = false;
                    ViewBag.Email = email;
                    ViewBag.EmailRegistered = "Email already registered";
                }
                else
                {
                    ViewBag.EmailRegistered = "";
                }
                if (valid)
                {
                    //insert user
                    Int32 newUserID = 0;
                    string strSQL = "Insert into UserInfo (Username, Email) values (@Field1, @Field2);";  
                    strSQL += "SELECT CAST(scope_identity() AS int)";
                    string insertPassword = "Insert into LoginInfo (UserId, Password) values (@Field1, @Field2);";
                    try  
                    {  
                      using (SqlCommand comd = new SqlCommand(strSQL, cn))  
                      {  
                        comd.Parameters.Add("@Field1", SqlDbType.VarChar);  
                        comd.Parameters["@Field1"].Value = username;  
                        comd.Parameters.Add("@Field2", SqlDbType.VarChar);  
                        comd.Parameters["@Field2"].Value = email;  
                        newUserID = (Int32)comd.ExecuteScalar();  
                      }

                      using (SqlCommand comd = new SqlCommand(insertPassword, cn))
                      {
                          comd.Parameters.Add("@Field1", SqlDbType.Int);
                          comd.Parameters["@Field1"].Value = newUserID;
                          comd.Parameters.Add("@Field2", SqlDbType.VarChar);
                          comd.Parameters["@Field2"].Value = password;
                          comd.ExecuteNonQuery();
                      }
                    }  
                    catch (Exception err)  
                    {
                        Console.WriteLine(err.Source);
                    }  
                    cn.Close();
                    Response.Redirect("../Login");
                }
                cn.Close();
            }
            return View();
        }
    }
}
