using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace RPNAVConnect
{
    public partial class RPBillingLoginSuccess : System.Web.UI.Page
    {
        //Get value from cokkie    
        public string GetCookieValue(string _str)
        {
            if (Request.Cookies[_str] != null)
                return Request.Cookies[_str].Value;
            else
                return "n/a";
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            System.Threading.Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");

            string sAuthToken = "n/a";
            string sTokenType = "n/a";
            long lExpiresIn = -1;
            DateTime dExpiresAt = DateTime.MinValue;
            bool bTokenExpired = false;

            string dbPath = ConfigurationManager.AppSettings["dbpath"].ToString();
            System.Data.OleDb.OleDbConnection dbConn = new System.Data.OleDb.OleDbConnection(dbPath);
            dbConn.Open();

            string sUserId = "n/a";
            try
            {
                if (System.Web.HttpContext.Current.Session["UserId"] != null)
                {
                    sUserId = System.Web.HttpContext.Current.Session["UserId"].ToString();
                }
            }
            catch (Exception ex)
            {
                ex.ToString();
                sUserId = "n/a";
            }

            string strSqlQuery = "SELECT TOP 1 * FROM [RPNAVConnect].[dbo].[BCLoginLog] WHERE [UserId] = '" + sUserId + "' ORDER BY Id DESC";
            System.Data.OleDb.OleDbDataReader oleReader;
            System.Data.OleDb.OleDbCommand cmd = new System.Data.OleDb.OleDbCommand(strSqlQuery, dbConn);
            oleReader = cmd.ExecuteReader();
            if (oleReader.Read())
            {
                if (!oleReader.IsDBNull(1))
                {
                    sAuthToken = oleReader.GetString(1);
                    sTokenType = oleReader.GetString(2);
                    lExpiresIn = oleReader.GetInt32(3);
                    dExpiresAt = oleReader.GetDateTime(4);

                    if (DateTime.Now > dExpiresAt)
                    {
                        bTokenExpired = true;
                    }
                }
            }
            oleReader.Close();

            dbConn.Close();

            InfoDataL.Text += "Token:<br />" + sAuthToken + "<br />";
            InfoDataL.Text += "Token type:<br />" + sTokenType + "<br />";
            InfoDataL.Text += "Token expires in:<br />" + lExpiresIn.ToString() + "<br />";
            InfoDataL.Text += "Token expires at:<br />" + dExpiresAt.ToString() + "<br /><br />";
            if (bTokenExpired == true)
            {
                InfoDataL.Text += "<font color='red'>Token expired. Please restart RPBilling.</font><br /><br />";
            }

        }
    }
}