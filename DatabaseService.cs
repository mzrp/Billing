using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Configuration;
using System.Data.OleDb;
using System.Globalization;
using Newtonsoft.Json.Linq;

namespace RPNAVConnect
{
    public class DatabaseService
    {
        public string GetBCToken(string sUserId)
        {
            string sResult = "n/a";

            try
            {
                string dbPath = ConfigurationManager.AppSettings["dbpath"].ToString();
                System.Data.OleDb.OleDbConnection dbConn = new System.Data.OleDb.OleDbConnection(dbPath);
                dbConn.Open();

                string strSqlQuery = "SELECT TOP 1 * FROM [RPNAVConnect].[dbo].[BCLoginLog] WHERE [UserId] = '" + sUserId + "' ORDER BY Id DESC";
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
        public bool IsTokenValid(string sUserId)
        {
            bool bResult = false;

            try
            {
                string dbPath = ConfigurationManager.AppSettings["dbpath"].ToString();
                System.Data.OleDb.OleDbConnection dbConn = new System.Data.OleDb.OleDbConnection(dbPath);
                dbConn.Open();

                string strSqlQuery = "SELECT TOP 1 * FROM [RPNAVConnect].[dbo].[BCLoginLog] WHERE [UserId] = '" + sUserId + "' ORDER BY Id DESC";
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
                            bResult = true;
                        }
                    }
                }
                oleReader.Close();

                dbConn.Close();
            }
            catch (Exception ex)
            {
                ex.ToString();
                bResult = false;
            }

            return bResult;
        }

        public string InsertUpdateDatabase(string SQL)
        {
            System.Threading.Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");

            string dbPath = ConfigurationManager.AppSettings["dbpath"].ToString();
            System.Data.OleDb.OleDbConnection dbConn = new System.Data.OleDb.OleDbConnection(dbPath);
            dbConn.Open();

            // Get Connection string
            string sResult = "DBOK";

            try
            {
                // Database Object instancing here
                OleDbCommand OleCommand;
                OleCommand = new OleDbCommand(SQL, dbConn);
                OleCommand.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                ex.ToString();
                sResult = ex.ToString();
                return sResult;
            }

            dbConn.Close();

            return sResult;
        }

    }
}