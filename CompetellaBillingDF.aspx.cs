using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Diagnostics;
using System.Threading.Tasks;
using System.IO;
using System.Runtime.InteropServices;

using System.Threading;
using System.Globalization;
using System.Configuration;
using System.Reflection;

using System.Xml;
using System.Text;

using RPNAVConnect.NAVCustomersWS;
using RPNAVConnect.NAVOrdersWS;
using System.Net;

using System.Text.RegularExpressions;
using System.Data.OleDb;

namespace RPNAVConnect
{
    public partial class CompetellaBillingDF : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            System.Threading.Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");

            if (Page.IsPostBack == false)
            {
                GetData("");
            }
        }

        public string HandleCompetellasData0()
        {
            System.Threading.Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");

            string sSql = "select name, MasterDomain from Companies";

            string dbPath = ConfigurationManager.AppSettings["dbpath2"].ToString();
            System.Data.OleDb.OleDbConnection dbConn = new System.Data.OleDb.OleDbConnection(dbPath);
            dbConn.Open();

            System.Data.OleDb.OleDbDataReader oleReader;
            System.Data.OleDb.OleDbCommand cmd = new System.Data.OleDb.OleDbCommand(sSql, dbConn);
            oleReader = cmd.ExecuteReader();
            string sResult = "";
            while (oleReader.Read())
            {
                if (!oleReader.IsDBNull(0))
                {
                    sResult += oleReader.GetString(0) + ",";
                }
            }
            oleReader.Close();
            dbConn.Close();

            return sResult;
        }

        public string HandleCompetellasData6()
        {
            System.Threading.Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");

            string sSql = "";
            sSql += "select c.Name as 'CompanyName',statSched.Name as 'Scheduled Name' ";
            sSql += "from Statistics_Schedule statSched join Companies c ";
            sSql += "on c.CompanyID = statSched.CompanyId ";
            sSql += "order by c.Name ";

            string dbPath = ConfigurationManager.AppSettings["dbpath2"].ToString();
            System.Data.OleDb.OleDbConnection dbConn = new System.Data.OleDb.OleDbConnection(dbPath);
            dbConn.Open();

            System.Data.OleDb.OleDbDataReader oleReader;
            System.Data.OleDb.OleDbCommand cmd = new System.Data.OleDb.OleDbCommand(sSql, dbConn);
            oleReader = cmd.ExecuteReader();
            string sResult = "";
            string sCompany = "n/a";
            string sCurrentCompany = "n/a";
            int iCount = 0;
            while (oleReader.Read())
            {
                if ((!oleReader.IsDBNull(0)) && (!oleReader.IsDBNull(1)))
                {
                    sCompany = oleReader.GetString(0);
                    iCount++;

                    if ((sCompany == "n/a") || (sCompany != sCurrentCompany))
                    {
                        if (sCompany != "n/a")
                        {
                            sResult = sResult.Replace(sCurrentCompany + "#Number", iCount.ToString());
                        }

                        sResult += sCompany + ";" + sCompany + "#Number,";
                        sCurrentCompany = sCompany;
                        iCount = 0;
                    }
                }
            }
            sResult = sResult.Replace(sCompany + "#Number", (iCount + 1).ToString());
            oleReader.Close();
            dbConn.Close();

            return sResult;
        }


        public string HandleCompetellasData5()
        {
            System.Threading.Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");

            string sSql = "";
            sSql += "select c.Name as 'CompanyName', iep.SipAddress as 'EndPoint URI' from IVREndpoint iep join Companies c ";
            sSql += "on c.CompanyID = iep.TenantId ";
            sSql += "order by c.Name ";

            string dbPath = ConfigurationManager.AppSettings["dbpath2"].ToString();
            System.Data.OleDb.OleDbConnection dbConn = new System.Data.OleDb.OleDbConnection(dbPath);
            dbConn.Open();

            System.Data.OleDb.OleDbDataReader oleReader;
            System.Data.OleDb.OleDbCommand cmd = new System.Data.OleDb.OleDbCommand(sSql, dbConn);
            oleReader = cmd.ExecuteReader();
            string sResult = "";
            string sCompany = "n/a";
            string sCurrentCompany = "n/a";
            int iCount = 0;
            while (oleReader.Read())
            {
                if ((!oleReader.IsDBNull(0)) && (!oleReader.IsDBNull(1)))
                {
                    sCompany = oleReader.GetString(0);
                    iCount++;

                    if ((sCompany == "n/a") || (sCompany != sCurrentCompany))
                    {
                        if (sCompany != "n/a")
                        {
                            sResult = sResult.Replace(sCurrentCompany + "#Number", iCount.ToString());
                        }

                        sResult += sCompany + ";" + sCompany + "#Number,";
                        sCurrentCompany = sCompany;
                        iCount = 0;
                    }
                }
            }
            sResult = sResult.Replace(sCompany + "#Number", (iCount + 1).ToString());
            oleReader.Close();
            dbConn.Close();

            return sResult;
        }

        public string HandleCompetellasData4()
        {
            System.Threading.Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");

            string sSql = "";
            sSql += "WITH myCallBack_CTE(Name, QueueNumber) ";
            sSql += "AS ";
            sSql += "(";
            sSql += "  select c.Name, cb.QueueNumber from ServiceGroupName sgn ";
            sSql += "  inner join servicegroup sg on sg.ServiceGroupNameId = sgn.ServiceGroupNameId ";
            sSql += "  inner join CallBack cb on sg.ServiceID = cb.ServiceId ";
            sSql += "  inner join Companies c on c.CompanyID = sgn.CompanyId ";
            sSql += "  where Len(cb.QueueNumber) > 0 ";
            sSql += ") ";
            sSql += "select distinct Name as 'CompanyName',QueueNumber as 'URI Callback' from myCallBack_CTE order by Name; ";

            string dbPath = ConfigurationManager.AppSettings["dbpath2"].ToString();
            System.Data.OleDb.OleDbConnection dbConn = new System.Data.OleDb.OleDbConnection(dbPath);
            dbConn.Open();

            System.Data.OleDb.OleDbDataReader oleReader;
            System.Data.OleDb.OleDbCommand cmd = new System.Data.OleDb.OleDbCommand(sSql, dbConn);
            oleReader = cmd.ExecuteReader();
            string sResult = "";
            string sCompany = "n/a";
            string sCurrentCompany = "n/a";
            int iCount = 0;
            while (oleReader.Read())
            {
                if ((!oleReader.IsDBNull(0)) && (!oleReader.IsDBNull(1)))
                {
                    sCompany = oleReader.GetString(0);
                    iCount++;

                    if ((sCompany == "n/a") || (sCompany != sCurrentCompany))
                    {
                        if (sCompany != "n/a")
                        {
                            sResult = sResult.Replace(sCurrentCompany + "#Number", iCount.ToString());
                        }

                        sResult += sCompany + ";" + sCompany + "#Number,";
                        sCurrentCompany = sCompany;
                        iCount = 0;
                    }
                }
            }
            sResult = sResult.Replace(sCompany + "#Number", (iCount + 1).ToString());
            oleReader.Close();
            dbConn.Close();

            return sResult;
        }

        public string HandleCompetellasData1()
        {
            System.Threading.Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");

            string sSql = "";
            sSql += "declare @AllResults1 nvarchar(4000) ";
            sSql += "SET @AllResults1 = '' ";

            //sSql += "print 'AgentService users for all tenants'";
            sSql += "declare @companyId uniqueidentifier ";
            sSql += "declare @companyName varchar(255) ";
            sSql += "declare @counter  varchar(255) ";
            sSql += "DECLARE cur1 CURSOR for ";
            sSql += "( ";
            sSql += "    select Name, CompanyID from Companies ";
            sSql += ") ";
            sSql += "OPEN cur1 ";
            sSql += "FETCH NEXT FROM cur1 INTO @companyName, @companyId ";
            sSql += "WHILE(@@FETCH_STATUS <> -1) ";
            sSql += "BEGIN ";

            sSql += "    select @counter = count(distinct aag.AgentId) ";           
            sSql += "    from AgentGroup ag ";
            sSql += "    join Agent_AgentGroup aag ";
            sSql += "    on ag.Id = aag.AgentGroupId ";
            sSql += "    join DeployedConfiguration dc ";
            sSql += "    on dc.Configuration = ag.Deployment ";
            sSql += "    join Configuration c ";
            sSql += "    on c.Id = dc.Configuration ";
            sSql += "    join Agent a ";
            sSql += "    on aag.AgentId = a.Id ";
            sSql += "    where c.type = 3 and c.Company = @companyId ";

            sSql += "    SET @AllResults1 += @companyName + ';' + @counter + ',' ";
            //sSql += "    print 'Company Name: [' + @companyName + '] number of Agents users: [' + @counter + ']'";

            sSql += "    FETCH NEXT FROM cur1 INTO @companyName, @companyId ";
            sSql += "END ";
            sSql += "CLOSE cur1 ";
            sSql += "DEALLOCATE cur1 ";

            sSql += "SELECT @AllResults1 ";

            string dbPath = ConfigurationManager.AppSettings["dbpath2"].ToString();
            System.Data.OleDb.OleDbConnection dbConn = new System.Data.OleDb.OleDbConnection(dbPath);
            dbConn.Open();

            System.Data.OleDb.OleDbDataReader oleReader;
            System.Data.OleDb.OleDbCommand cmd = new System.Data.OleDb.OleDbCommand(sSql, dbConn);
            oleReader = cmd.ExecuteReader();
            string sResult = "n/a";
            if (oleReader.Read())
            {
                if (!oleReader.IsDBNull(0)) sResult = oleReader.GetString(0);
            }
            oleReader.Close();
            dbConn.Close();

            return sResult;
        }

        public string HandleCompetellasData2()
        {
            System.Threading.Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");

            string sSql = "";
            sSql += "declare @AllResults2 nvarchar(4000) ";
            sSql += "SET @AllResults2 = '' ";

            //sSql += "print '' ";
            //sSql += "print 'All CCD users for all tenants' ";
            sSql += "declare @companyId uniqueidentifier ";
            sSql += "declare @companyName varchar(255) ";
            sSql += "declare @counter  varchar(255) ";
            sSql += "DECLARE cur1 CURSOR for ";
            sSql += "(";
            sSql += "    select Name, CompanyID from Companies ";
            sSql += ") ";
            sSql += "                OPEN cur1 ";
            sSql += "FETCH NEXT FROM cur1 INTO @companyName, @companyId ";
            sSql += "WHILE(@@FETCH_STATUS <> -1) ";
            sSql += "BEGIN ";

            sSql += "select @counter = count(distinct aag.AgentId) ";
            sSql += "from AgentGroup ag ";
            sSql += "join Agent_AgentGroup aag ";
            sSql += "on ag.Id = aag.AgentGroupId ";
            sSql += "join DeployedConfiguration dc ";
            sSql += "on dc.Configuration = ag.Deployment ";
            sSql += "join Configuration c ";
            sSql += "on c.Id = dc.Configuration ";
            sSql += "join Agent a ";
            sSql += "on aag.AgentId = a.Id ";
            sSql += "where c.type = 35 and c.Company = @companyId ";

            sSql += "    SET @AllResults2 += @companyName + ';' + @counter + ',' ";
            //sSql += "print 'Company Name: [' + @companyName + '] number of CCD agents users: [' + @counter + ']' ";

            sSql += "FETCH NEXT FROM cur1 INTO @companyName, @companyId ";
            sSql += "END ";
            sSql += "CLOSE cur1 ";
            sSql += "DEALLOCATE cur1 ";

            sSql += "SELECT @AllResults2 ";

            string dbPath = ConfigurationManager.AppSettings["dbpath2"].ToString();
            System.Data.OleDb.OleDbConnection dbConn = new System.Data.OleDb.OleDbConnection(dbPath);
            dbConn.Open();

            System.Data.OleDb.OleDbDataReader oleReader;
            System.Data.OleDb.OleDbCommand cmd = new System.Data.OleDb.OleDbCommand(sSql, dbConn);
            oleReader = cmd.ExecuteReader();
            string sResult = "n/a";
            if (oleReader.Read())
            {
                if (!oleReader.IsDBNull(0)) sResult = oleReader.GetString(0);
            }
            oleReader.Close();
            dbConn.Close();

            return sResult;
        }

        public string HandleCompetellasData3()
        {
            System.Threading.Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");

            string sSql = "";

            sSql += "declare @AllResults3 nvarchar(4000) ";
            sSql += "SET @AllResults3 = '' ";

            //sSql += "print '' ";
            //sSql += "print 'All AD Imported Users for each Company.' ";
            sSql += "declare @serviceId uniqueidentifier ";
            sSql += "declare @companyName varchar(255) ";
            sSql += "declare @counter  varchar(255) ";
            sSql += "DECLARE cur1 CURSOR for ";
            sSql += "(";
            sSql += " select distinct s.ServiceID, c.Name as 'Company' from Services s ";
            sSql += "join ServiceGroup sg ";
            sSql += "on s.ServiceID = sg.ServiceId ";
            sSql += "join ServiceGroupName sgn ";
            sSql += "on sg.ServiceGroupNameId = sgn.ServiceGroupNameId ";
            sSql += "join Companies c ";
            sSql += "on sgn.CompanyId = c.CompanyID ";
            sSql += "where s.Type = 10 ";
            sSql += ") ";
            sSql += "OPEN cur1 ";
            sSql += "FETCH NEXT FROM cur1 INTO @serviceId, @companyName ";
            sSql += "WHILE(@@FETCH_STATUS <> -1) ";
            sSql += "BEGIN ";
            sSql += "select @counter = count(UserID) from users where ImportService = @serviceId and active = 1 ";
            
            sSql += "SET @AllResults3 += @companyName + ';' + @counter + ',' ";
            //sSql += "print 'Company Name: [' + @companyName + '] number of AD Imported users: [' + @counter + ']' ";

            sSql += "FETCH NEXT FROM cur1 INTO @serviceId, @companyName ";
            sSql += "END ";
            sSql += "CLOSE cur1 ";
            sSql += "DEALLOCATE cur1 ";

            sSql += "SELECT @AllResults3 ";

            string dbPath = ConfigurationManager.AppSettings["dbpath2"].ToString();
            System.Data.OleDb.OleDbConnection dbConn = new System.Data.OleDb.OleDbConnection(dbPath);
            dbConn.Open();

            System.Data.OleDb.OleDbDataReader oleReader;
            System.Data.OleDb.OleDbCommand cmd = new System.Data.OleDb.OleDbCommand(sSql, dbConn);
            oleReader = cmd.ExecuteReader();
            string sResult = "n/a";
            if (oleReader.Read())
            {
                if (!oleReader.IsDBNull(0)) sResult = oleReader.GetString(0);
            }
            oleReader.Close();
            dbConn.Close();

            return sResult;
        }

        private string GetCompanyBillingSubscriptionId(string sCompanyDBName)
        {
            string sResult = "n/a";

            string dbPath = ConfigurationManager.AppSettings["dbpath"].ToString();
            System.Data.OleDb.OleDbConnection dbConn = new System.Data.OleDb.OleDbConnection(dbPath);
            dbConn.Open();

            string sSql = "SELECT TOP 1 [Id] FROM [RPNAVConnect].[dbo].[BillingSubscriptions] WHERE [NavCustomerName] = '" + sCompanyDBName + "' AND [Deleted] is null";

            System.Data.OleDb.OleDbDataReader oleReader;
            System.Data.OleDb.OleDbCommand cmd = new System.Data.OleDb.OleDbCommand(sSql, dbConn);
            oleReader = cmd.ExecuteReader();
            if (oleReader.Read())
            {
                if (!oleReader.IsDBNull(0)) sResult = oleReader.GetInt32(0).ToString();
            }
            oleReader.Close();
            dbConn.Close();

            return sResult;
        }

        public void GetData(string sAction)
        {
            Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");

            CompetellaBillingDataL.Text = "<b>Company,Agents Users,CCD Agents Users,AD Imported Users,Call Back,IVR,Mobil pressence,Mail distribution,Advanced statistics per UC ser</b><br />";

            PushingDataL.Text = "";

            // get all companies
            string sData = HandleCompetellasData0();
            string[] sDataArray = sData.Split(',');
            foreach (string sCompany in sDataArray)
            {
                if (sCompany != "")
                {

                    string sCompanyBillingSubscriptionId = "n/a";
                    if (sAction == "push")
                    {
                        string sCompanyDBName = "n/a";
                        try
                        {
                            sCompanyDBName = ConfigurationManager.AppSettings[sCompany].ToString();
                        }
                        catch (Exception ex)
                        {
                            ex.ToString();
                            sCompanyDBName = "n/a";
                        }

                        if (sCompanyDBName != "n/a")
                        {
                            sCompanyBillingSubscriptionId = GetCompanyBillingSubscriptionId(sCompanyDBName);

                            PushingDataL.Text += "<br />";
                            PushingDataL.Text += "<b>Pushing data for company: " + sCompany + " [" + sCompanyBillingSubscriptionId + "]</b>";
                            PushingDataL.Text += "<br />";
                        }
                    }

                    CompetellaBillingDataL.Text += sCompany + ",";

                    // number of Agents users
                    string[] sAgentUsersNumberArray = HandleCompetellasData1().Split(',');
                    string sAgentUsersNumber = "";
                    if (sAgentUsersNumberArray.Length > 0)
                    {
                        foreach (string sSingleData in sAgentUsersNumberArray)
                        {
                            if (sSingleData != "")
                            {
                                if (sCompany == sSingleData.Split(';')[0])
                                {
                                    sAgentUsersNumber = sSingleData.Split(';')[1];
                                    break;
                                }
                            }
                        }
                    }
                    CompetellaBillingDataL.Text += sAgentUsersNumber + ",";

                    if (sAction == "push")
                    {
                        if (sCompanyBillingSubscriptionId != "n/a")
                        {
                            decimal dAmount = -1;
                            if (sAgentUsersNumber != "")
                            {
                                try
                                {
                                    dAmount = Convert.ToDecimal(sAgentUsersNumber);
                                }
                                catch (Exception ex)
                                {
                                    ex.ToString();
                                    PushingDataL.Text += ex.ToString() + " <br />";
                                    dAmount = -1;
                                }
                            }
                            if (dAmount != -1)
                            {
                                string sProductNavId = "n/a";
                                try
                                {
                                    sProductNavId = ConfigurationManager.AppSettings["Agents users"].ToString();
                                }
                                catch (Exception ex)
                                {
                                    ex.ToString();
                                    PushingDataL.Text += ex.ToString() + " <br />";
                                    sProductNavId = "n/a";
                                }

                                if (sProductNavId != "n/a")
                                {
                                    string sSql = "UPDATE [RPNAVConnect].[dbo].[BillingProducts] SET [UnitAmount] = " + dAmount.ToString() + " WHERE [BillingSubscriptionId] = " + sCompanyBillingSubscriptionId + " AND [NavProductNumber] = '" + sProductNavId + "'";
                                    string sDBResult = InsertUpdateDatabase(sSql);

                                    if (sDBResult == "DBOK")
                                    {
                                        PushingDataL.Text += "Agents users: " + dAmount.ToString() + " [Product NAV no: " + sProductNavId + "]";
                                        PushingDataL.Text += "<br />";
                                    }

                                    // Mail distribution is the same except for Bevola
                                    if (sCompany != "Bevola")
                                    {
                                        string sProductMailDistributionNavId = "n/a";
                                        try
                                        {
                                            sProductMailDistributionNavId = ConfigurationManager.AppSettings["Mail distribution"].ToString();
                                        }
                                        catch (Exception ex)
                                        {
                                            ex.ToString();
                                            sProductMailDistributionNavId = "n/a";
                                        }

                                        if (sProductMailDistributionNavId != "n/a")
                                        {
                                            sSql = "UPDATE [RPNAVConnect].[dbo].[BillingProducts] SET [UnitAmount] = " + dAmount.ToString() + " WHERE [BillingSubscriptionId] = " + sCompanyBillingSubscriptionId + " AND [NavProductNumber] = '" + sProductMailDistributionNavId + "'";
                                            sDBResult = InsertUpdateDatabase(sSql);

                                            if (sDBResult == "DBOK")
                                            {
                                                PushingDataL.Text += "Mail distributio: " + dAmount.ToString() + " [Product NAV no: " + sProductMailDistributionNavId + "]";
                                                PushingDataL.Text += "<br />";
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }

                    // number of CCD Agents users
                    string[] sCCDAgentUsersNumberArray = HandleCompetellasData2().Split(',');
                    string sCCDAgentUsersNumber = "";
                    if (sCCDAgentUsersNumberArray.Length > 0)
                    {
                        foreach (string sSingleData in sCCDAgentUsersNumberArray)
                        {
                            if (sSingleData != "")
                            {
                                if (sCompany == sSingleData.Split(';')[0])
                                {
                                    sCCDAgentUsersNumber = sSingleData.Split(';')[1];
                                    break;
                                }
                            }
                        }
                    }
                    CompetellaBillingDataL.Text += sCCDAgentUsersNumber + ",";

                    if (sAction == "push")
                    {
                        if (sCompanyBillingSubscriptionId != "n/a")
                        {
                            decimal dAmount = -1;
                            if (sCCDAgentUsersNumber != "")
                            {
                                try
                                {
                                    dAmount = Convert.ToDecimal(sCCDAgentUsersNumber);
                                }
                                catch (Exception ex)
                                {
                                    ex.ToString();
                                    PushingDataL.Text += ex.ToString() + " <br />";
                                    dAmount = -1;
                                }
                            }
                            if (dAmount != -1)
                            {
                                string sProductNavId = "n/a";
                                try
                                {
                                    sProductNavId = ConfigurationManager.AppSettings["CCD Agents users"].ToString();
                                }
                                catch (Exception ex)
                                {
                                    ex.ToString();
                                    PushingDataL.Text += ex.ToString() + " <br />";
                                    sProductNavId = "n/a";
                                }

                                if (sProductNavId != "n/a")
                                {
                                    string sSql = "UPDATE [RPNAVConnect].[dbo].[BillingProducts] SET [UnitAmount] = " + dAmount.ToString() + " WHERE [BillingSubscriptionId] = " + sCompanyBillingSubscriptionId + " AND [NavProductNumber] = '" + sProductNavId + "'";
                                    string sDBResult = InsertUpdateDatabase(sSql);

                                    if (sDBResult == "DBOK")
                                    {
                                        PushingDataL.Text += "CCD Agents users: " + dAmount.ToString() + " [Product NAV no: " + sProductNavId + "]";
                                        PushingDataL.Text += "<br />";
                                    }
                                }
                            }
                        }
                    }

                    // number of AD Imported users
                    string[] sADImportedUsersNumberArray = HandleCompetellasData3().Split(',');
                    string sADImportedUsersNumber = "";
                    if (sADImportedUsersNumberArray.Length > 0)
                    {
                        foreach (string sSingleData in sADImportedUsersNumberArray)
                        {
                            if (sSingleData != "")
                            {
                                if (sCompany == sSingleData.Split(';')[0])
                                {
                                    sADImportedUsersNumber = sSingleData.Split(';')[1];
                                    break;
                                }
                            }
                        }
                    }
                    CompetellaBillingDataL.Text += sADImportedUsersNumber + ",";

                    if (sAction == "push")
                    {
                        if (sCompanyBillingSubscriptionId != "n/a")
                        {
                            decimal dAmount = -1;
                            if (sADImportedUsersNumber != "")
                            {
                                try
                                {
                                    dAmount = Convert.ToDecimal(sADImportedUsersNumber);
                                }
                                catch (Exception ex)
                                {
                                    ex.ToString();
                                    PushingDataL.Text += ex.ToString() + " <br />";
                                    dAmount = -1;
                                }
                            }
                            if (dAmount != -1)
                            {
                                string sProductNavId = "n/a";
                                try
                                {
                                    sProductNavId = ConfigurationManager.AppSettings["AD Imported users"].ToString();
                                }
                                catch (Exception ex)
                                {
                                    ex.ToString();
                                    PushingDataL.Text += ex.ToString() + " <br />";
                                    sProductNavId = "n/a";
                                }

                                if (sProductNavId != "n/a")
                                {
                                    string sSql = "UPDATE [RPNAVConnect].[dbo].[BillingProducts] SET [UnitAmount] = " + dAmount.ToString() + " WHERE [BillingSubscriptionId] = " + sCompanyBillingSubscriptionId + " AND [NavProductNumber] = '" + sProductNavId + "'";
                                    string sDBResult = InsertUpdateDatabase(sSql);

                                    if (sDBResult == "DBOK")
                                    {
                                        PushingDataL.Text += "AD Imported users: " + dAmount.ToString() + " [Product NAV no: " + sProductNavId + "]";
                                        PushingDataL.Text += "<br />";
                                    }

                                    // Mobil pressences are the same except for Bevola
                                    if (sCompany != "Bevola")
                                    {
                                        string sProductMobilePresencesNavId = "n/a";
                                        try
                                        {
                                            sProductMobilePresencesNavId = ConfigurationManager.AppSettings["Mobil pressences"].ToString();
                                        }
                                        catch (Exception ex)
                                        {
                                            ex.ToString();
                                            sProductMobilePresencesNavId = "n/a";
                                        }

                                        if (sProductMobilePresencesNavId != "n/a")
                                        {
                                            sSql = "UPDATE [RPNAVConnect].[dbo].[BillingProducts] SET [UnitAmount] = " + dAmount.ToString() + " WHERE [BillingSubscriptionId] = " + sCompanyBillingSubscriptionId + " AND [NavProductNumber] = '" + sProductMobilePresencesNavId + "'";
                                            sDBResult = InsertUpdateDatabase(sSql);

                                            if (sDBResult == "DBOK")
                                            {
                                                PushingDataL.Text += "Mobil pressences: " + dAmount.ToString() + " [Product NAV no: " + sProductMobilePresencesNavId + "]";
                                                PushingDataL.Text += "<br />";
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }

                    // number of CallBack/Enpoints/Company
                    string[] sCallBackArray = HandleCompetellasData4().Split(',');
                    string sCallBack = "";
                    if (sCallBackArray.Length > 0)
                    {
                        foreach (string sSingleData in sCallBackArray)
                        {
                            if (sSingleData != "")
                            {
                                if (sCompany == sSingleData.Split(';')[0])
                                {
                                    sCallBack = sSingleData.Split(';')[1];
                                    break;
                                }
                            }
                        }
                    }
                    CompetellaBillingDataL.Text += sCallBack + ",";

                    if (sAction == "push")
                    {
                        if (sCompanyBillingSubscriptionId != "n/a")
                        {
                            decimal dAmount = -1;
                            if (sCallBack != "")
                            {
                                try
                                {
                                    dAmount = Convert.ToDecimal(sCallBack);
                                }
                                catch (Exception ex)
                                {
                                    ex.ToString();
                                    PushingDataL.Text += ex.ToString() + " <br />";
                                    dAmount = -1;
                                }
                            }
                            if (dAmount != -1)
                            {
                                string sProductNavId = "n/a";
                                try
                                {
                                    sProductNavId = ConfigurationManager.AppSettings["CallBacks"].ToString();
                                }
                                catch (Exception ex)
                                {
                                    ex.ToString();
                                    PushingDataL.Text += ex.ToString() + " <br />";
                                    sProductNavId = "n/a";
                                }

                                if (sProductNavId != "n/a")
                                {
                                    string sSql = "UPDATE [RPNAVConnect].[dbo].[BillingProducts] SET [UnitAmount] = " + dAmount.ToString() + " WHERE [BillingSubscriptionId] = " + sCompanyBillingSubscriptionId + " AND [NavProductNumber] = '" + sProductNavId + "'";
                                    string sDBResult = InsertUpdateDatabase(sSql);

                                    if (sDBResult == "DBOK")
                                    {
                                        PushingDataL.Text += "CallBacks: " + dAmount.ToString() + " [Product NAV no: " + sProductNavId + "]";
                                        PushingDataL.Text += "<br />";
                                    }
                                }
                            }
                        }
                    }

                    // number of IVR - all endpoints / company
                    string[] sIVRArray = HandleCompetellasData5().Split(',');
                    string sIVR = "";
                    if (sIVRArray.Length > 0)
                    {
                        foreach (string sSingleData in sIVRArray)
                        {
                            if (sSingleData != "")
                            {
                                if (sCompany == sSingleData.Split(';')[0])
                                {
                                    sIVR = sSingleData.Split(';')[1];
                                    break;
                                }
                            }
                        }
                    }
                    CompetellaBillingDataL.Text += sIVR + ",";

                    if (sAction == "push")
                    {
                        if (sCompanyBillingSubscriptionId != "n/a")
                        {
                            decimal dAmount = -1;
                            if (sIVR != "")
                            {
                                try
                                {
                                    dAmount = Convert.ToDecimal(sIVR);
                                }
                                catch (Exception ex)
                                {
                                    ex.ToString();
                                    PushingDataL.Text += ex.ToString() + " <br />";
                                    dAmount = -1;
                                }
                            }
                            if (dAmount != -1)
                            {
                                string sProductNavId = "n/a";
                                try
                                {
                                    sProductNavId = ConfigurationManager.AppSettings["IVRs"].ToString();
                                }
                                catch (Exception ex)
                                {
                                    ex.ToString();
                                    PushingDataL.Text += ex.ToString() + " <br />";
                                    sProductNavId = "n/a";
                                }

                                if (sProductNavId != "n/a")
                                {
                                    string sSql = "UPDATE [RPNAVConnect].[dbo].[BillingProducts] SET [UnitAmount] = " + dAmount.ToString() + " WHERE [BillingSubscriptionId] = " + sCompanyBillingSubscriptionId + " AND [NavProductNumber] = '" + sProductNavId + "'";
                                    string sDBResult = InsertUpdateDatabase(sSql);

                                    if (sDBResult == "DBOK")
                                    {
                                        PushingDataL.Text += "IVRs: " + dAmount.ToString() + " [Product NAV no: " + sProductNavId + "]";
                                        PushingDataL.Text += "<br />";
                                    }
                                }
                            }
                        }
                    }

                    // Mobil pressence
                    if (sCompany != "Bevola")
                    {
                        CompetellaBillingDataL.Text += sADImportedUsersNumber + ",";
                    }
                    else
                    {
                        CompetellaBillingDataL.Text += ",";
                    }

                    // Mail distribution
                    if (sCompany != "Bevola")
                    {
                        CompetellaBillingDataL.Text += sAgentUsersNumber + ",";
                    }
                    else
                    {
                        CompetellaBillingDataL.Text += ",";
                    }

                    // number of Statistics Schedule - configured reports
                    string[] sStatisticsScheduleArray = HandleCompetellasData6().Split(',');
                    string sStatisticsSchedule = "";
                    if (sStatisticsScheduleArray.Length > 0)
                    {
                        foreach (string sSingleData in sStatisticsScheduleArray)
                        {
                            if (sSingleData != "")
                            {
                                if (sCompany == sSingleData.Split(';')[0])
                                {
                                    sStatisticsSchedule = sSingleData.Split(';')[1];
                                    break;
                                }
                            }
                        }
                    }
                    CompetellaBillingDataL.Text += sStatisticsSchedule + "<br />";

                    if (sAction == "push")
                    {
                        if (sCompanyBillingSubscriptionId != "n/a")
                        {
                            decimal dAmount = -1;
                            if (sStatisticsSchedule != "")
                            {
                                try
                                {
                                    dAmount = Convert.ToDecimal(sStatisticsSchedule);
                                }
                                catch (Exception ex)
                                {
                                    ex.ToString();
                                    PushingDataL.Text += ex.ToString() + " <br />";
                                    dAmount = -1;
                                }
                            }
                            if (dAmount != -1)
                            {
                                string sProductNavId = "n/a";
                                try
                                {
                                    sProductNavId = ConfigurationManager.AppSettings["Advanced statistics"].ToString();
                                }
                                catch (Exception ex)
                                {
                                    ex.ToString();
                                    PushingDataL.Text += ex.ToString() + " <br />";
                                    sProductNavId = "n/a";
                                }

                                if (sProductNavId != "n/a")
                                {
                                    string sSql = "UPDATE [RPNAVConnect].[dbo].[BillingProducts] SET [UnitAmount] = " + dAmount.ToString() + " WHERE [BillingSubscriptionId] = " + sCompanyBillingSubscriptionId + " AND [NavProductNumber] = '" + sProductNavId + "'";
                                    string sDBResult = InsertUpdateDatabase(sSql);

                                    if (sDBResult == "DBOK")
                                    {
                                        PushingDataL.Text += "Advanced statistics: " + dAmount.ToString() + " [Product NAV no: " + sProductNavId + "]";
                                        PushingDataL.Text += "<br />";
                                    }
                                }
                            }
                        }
                    }
                }
            }

            string sCSVFile = "CompetellaBilling_";
            sCSVFile += DateTime.Now.Day.ToString().PadLeft(2, '0');
            sCSVFile += DateTime.Now.Month.ToString().PadLeft(2, '0');
            sCSVFile += DateTime.Now.Year.ToString().PadLeft(4, '0');
            sCSVFile += ".csv";
            string sCDRFileLocation = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\" + sCSVFile;
            string sCSVFileData = CompetellaBillingDataL.Text.Replace("<br />", "\n").Replace("<b>", "").Replace("</b>", "");
            CSVFilePathTB.Text = "n/a";
            try
            {
                File.WriteAllText(sCDRFileLocation, sCSVFileData);
                CSVFilePathTB.Text = sCDRFileLocation;
            }
            catch (Exception ex)
            {
                PushingDataL.Text += ex.ToString();
            }

            PushDataToSubscriptionsB.Visible = true;
            SendCSVFileToEmailB.Visible = true;
            EmailTB.Text = "USER@rackpeople.dk";
            EmailTB.Visible = true;

            // G-S;1,Forsyning Helsingor;13,Gowingu;0,Personalegruppen;0,Shared;0,RackPeople;3,Timelog;17,Bevola;0,Airsupport;0,
            /*
            CompetellaBillingDataL.Text += "<font color='Green'>AgentService users for all tenants</font><br />";
            sData = HandleCompetellasData1();
            sDataArray = sData.Split(',');
            foreach(string sSingleData in sDataArray)
            {
                if (sSingleData != "")
                {
                    string sCompanyName = sSingleData.Split(';')[0];
                    string sAgentUsersNumber = sSingleData.Split(';')[1];
                    CompetellaBillingDataL.Text += "Company Name: [" + sCompanyName + "] number of Agents users: [" + sAgentUsersNumber + "]<br />";
                }
            }

            CompetellaBillingDataL.Text += "<br />";

            CompetellaBillingDataL.Text += "<font color='Green'>All CCD users for all tenants</font><br />";
            sData = HandleCompetellasData2();
            sDataArray = sData.Split(',');
            foreach (string sSingleData in sDataArray)
            {
                if (sSingleData != "")
                {
                    string sCompanyName = sSingleData.Split(';')[0];
                    string sCCDAgentUsersNumber = sSingleData.Split(';')[1];
                    CompetellaBillingDataL.Text += "Company Name: [" + sCompanyName + "] number of CCD Agents users: [" + sCCDAgentUsersNumber + "]<br />";
                }
            }

            CompetellaBillingDataL.Text += "<br />";

            CompetellaBillingDataL.Text += "<font color='Green'>All AD Imported Users for each Company</font><br />";
            sData = HandleCompetellasData3();
            sDataArray = sData.Split(',');
            foreach (string sSingleData in sDataArray)
            {
                if (sSingleData != "")
                {
                    string sCompanyName = sSingleData.Split(';')[0];
                    string sADAgentUsersNumber = sSingleData.Split(';')[1];
                    CompetellaBillingDataL.Text += "Company Name: [" + sCompanyName + "] number of AD Imported users: [" + sADAgentUsersNumber + "]<br />";
                }
            }

            CompetellaBillingDataL.Text += "<br />";

            CompetellaBillingDataL.Text += "<font color='Green'>CallBack / Enpoints / Company</font><br />";
            sData = HandleCompetellasData4();
            sDataArray = sData.Split(',');
            foreach (string sSingleData in sDataArray)
            {
                if (sSingleData != "")
                {
                    string sCompanyName = sSingleData.Split(';')[0];
                    string sADAgentUsersNumber = sSingleData.Split(';')[1];
                    CompetellaBillingDataL.Text += "Company Name: [" + sCompanyName + "] number of CallBack/Enpoints/Company: [" + sADAgentUsersNumber + "]<br />";
                }
            }

            CompetellaBillingDataL.Text += "<br />";

            CompetellaBillingDataL.Text += "<font color='Green'>IVR - all endpoints / company</font><br />";
            sData = HandleCompetellasData5();
            sDataArray = sData.Split(',');
            foreach (string sSingleData in sDataArray)
            {
                if (sSingleData != "")
                {
                    string sCompanyName = sSingleData.Split(';')[0];
                    string sADAgentUsersNumber = sSingleData.Split(';')[1];
                    CompetellaBillingDataL.Text += "Company Name: [" + sCompanyName + "] number of IVR - all endpoints/company: [" + sADAgentUsersNumber + "]<br />";
                }
            }

            CompetellaBillingDataL.Text += "<br />";

            CompetellaBillingDataL.Text += "<font color='Green'>Statistics Schedule - configured reports / Company</font><br />";
            sData = HandleCompetellasData6();
            sDataArray = sData.Split(',');
            foreach (string sSingleData in sDataArray)
            {
                if (sSingleData != "")
                {
                    string sCompanyName = sSingleData.Split(';')[0];
                    string sADAgentUsersNumber = sSingleData.Split(';')[1];
                    CompetellaBillingDataL.Text += "Company Name: [" + sCompanyName + "] number of Statistics Schedule - configured reports / Company: [" + sADAgentUsersNumber + "]<br />";
                }
            }

            PushDataToSubscriptionsB.Visible = true;
            */

        }

        protected void PushDataToSubscriptionsB_Click(object sender, EventArgs e)
        {
            GetData("push");
        }

        protected void SendCSVFileToEmailB_Click(object sender, EventArgs e)
        {
            // send email
            if ((CSVFilePathTB.Text != "") && (CSVFilePathTB.Text != "n/a"))
            {
                if ((EmailTB.Text != "") && (EmailTB.Text.IndexOf("@") != -1))
                {
                    try
                    {
                        System.Net.Mail.MailMessage message = new System.Net.Mail.MailMessage();
                        if (EmailTB.Text.IndexOf(",") != -1)
                        {
                            string[] sRecepientEmailArray = EmailTB.Text.Split(',');
                            foreach (string sRecepient in sRecepientEmailArray)
                            {
                                message.To.Add(sRecepient);
                            }
                        }
                        else
                        {
                            message.To.Add(EmailTB.Text);
                        }
                        message.Subject = "Competella Billing Data";
                        message.IsBodyHtml = false;
                        message.BodyEncoding = System.Text.Encoding.UTF8;
                        message.Body = "CSV file attached.";
                        System.Net.Mail.Attachment att = new System.Net.Mail.Attachment(CSVFilePathTB.Text);
                        message.Attachments.Add(att);

                        System.Net.Mail.SmtpClient smtp = new System.Net.Mail.SmtpClient("relay.rackpeople.com");
                        message.From = new System.Net.Mail.MailAddress("webmaster@rackpeople.com");
                        //smtp.Credentials = new System.Net.NetworkCredential("webmaster@astellas.dk", "password");
                        smtp.Send(message);

                        PushingDataL.Text = "CSV sent to " + EmailTB.Text;

                        att.Dispose();
                        message.Dispose();
                        smtp.Dispose();
                    }
                    catch (Exception ex)
                    {
                        PushingDataL.Text += "<br /><br />" + ex.ToString();
                    }
                }
                else
                {
                    PushingDataL.Text += "<br /><br />Wrong email: " + EmailTB.Text + ";";
                }
            }
            else
            {
                PushingDataL.Text += "<br /><br />CSVFilePath: " + CSVFilePathTB.Text + ";";
            }
        }

        private string InsertUpdateDatabase(string SQL)
        {
            Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");

            string dbPath = ConfigurationManager.AppSettings["dbpath"].ToString();
            OleDbConnection dbConn = new OleDbConnection(dbPath);
            dbConn.Open();

            // Get Connection string
            string sResult = "DBOK";

            try
            {
                // Database Object instancing here
                OleDbCommand OleCommand;
                OleCommand = new OleDbCommand(SQL, dbConn);
                OleCommand.CommandTimeout = 600;
                OleCommand.ExecuteNonQuery();
            }
            catch (Exception Ex)
            {
                Ex.ToString();
                sResult = "DBERROR: " + Ex.ToString();
                PushingDataL.Text += sResult + " <br />";
                return sResult;
            }

            dbConn.Close();

            return sResult;
        }
    }
}