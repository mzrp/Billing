using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;

namespace RackPeople.BillingAPI.Services
{
    public class BCService
    {
        public string GetBCToken()
        {
            string sResult = "n/a";

            try
            {
                string dbPath = ConfigurationManager.AppSettings["dbpath"].ToString();
                System.Data.OleDb.OleDbConnection dbConn = new System.Data.OleDb.OleDbConnection(dbPath);
                dbConn.Open();

                string sDate = DateTime.Now.Year.ToString().PadLeft(4, '0') + "-";
                sDate += DateTime.Now.Month.ToString().PadLeft(2, '0') + "-";
                sDate += DateTime.Now.Day.ToString().PadLeft(2, '0') + " ";
                sDate += DateTime.Now.AddMinutes(10).Hour.ToString().PadLeft(2, '0') + ":";
                sDate += DateTime.Now.AddMinutes(10).Minute.ToString().PadLeft(2, '0') + ":";
                sDate += DateTime.Now.AddMinutes(10).Second.ToString().PadLeft(2, '0') + ".000";

                string strSqlQuery = "SELECT TOP 1 * FROM [RPNAVConnect].[dbo].[BCLoginLog] WHERE [TokenExpiresAt] > '" + sDate + "' ORDER BY Id DESC";
                System.Data.OleDb.OleDbDataReader oleReader;
                System.Data.OleDb.OleDbCommand cmd = new System.Data.OleDb.OleDbCommand(strSqlQuery, dbConn);
                oleReader = cmd.ExecuteReader();
                if (oleReader.Read())
                {
                    if (!oleReader.IsDBNull(1))
                    {
                        string sAuthToken = oleReader.GetString(1);
                        string sTokenType = oleReader.GetString(2);
                        int lExpiresIn = oleReader.GetInt32(3);
                        DateTime dExpiresAt = oleReader.GetDateTime(4);

                        if (DateTime.Now.AddMinutes(15) < dExpiresAt)
                        {
                            sResult = sAuthToken;
                        }
                    }
                }
                oleReader.Close();

                dbConn.Close();
            }
            catch (Exception ex)
            {
                ex.ToString();
                sResult = "n/a";
            }

            return sResult;
        }
    }
}