using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Globalization;
using System.Threading.Tasks;

using System.Data.OleDb;
using System.Configuration;
using System.Xml;
using System.Text;
using System.Threading;

using System.Runtime.InteropServices;
using System.IO;
using Excel = Microsoft.Office.Interop.Excel;

using RPNAVConnect.NAVCustomersWS;
using RPNAVConnect.NAVOrdersWS;
using System.Net;

using System.Text.RegularExpressions;

namespace RPNAVConnect
{
    public partial class HitBillingDF : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            System.Threading.Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");

            if (Page.IsPostBack == false)
            {
                // delete
                string sAction = "n/a";
                string sId = "n/a";
                try
                {
                    sAction = Request.QueryString["action"];
                    sId = Request.QueryString["id"];
                }
                catch (Exception ex)
                {
                    sAction = "n/a";
                    sId = "n/a";
                    HitBillingDataL.Text = ex.ToString();
                }
                if ((sAction != "n/a") && (sId != "n/a"))
                {
                    if (sAction == "del")
                    {
                        string dbPath = ConfigurationManager.AppSettings["dbpath"].ToString();
                        System.Data.OleDb.OleDbConnection dbConn = new System.Data.OleDb.OleDbConnection(dbPath);
                        dbConn.Open();
                        string sSqlDel = "DELETE FROM [dbo].[HITCustomers] WHERE [Id] = " + sId;
                        string sDBResultDel = InsertUpdateDatabase(sSqlDel, dbConn);
                        if (sDBResultDel == "DBOK")
                        {
                            sSqlDel = "DELETE FROM [dbo].[HITUsers] WHERE [CustomerId] = " + sId;
                            sDBResultDel = InsertUpdateDatabase(sSqlDel, dbConn);
                        }
                        dbConn.Close();
                    }
                }

                GetAllCustomers();
            }
            else
            {
                GetAllCustomers();
            }
        }

        private string GetAllCustomers()
        {
            string sResult = "Ok";

            try
            {
                string dbPath = ConfigurationManager.AppSettings["dbpath"].ToString();
                System.Data.OleDb.OleDbConnection dbConn = new System.Data.OleDb.OleDbConnection(dbPath);
                dbConn.Open();

                HitBillingDataL.Text = "";
                string sSql = "SELECT hc.*, (SELECT Count(*) FROM [RPNAVConnect].[dbo].[HITUsers] as hcu WHERE hcu.[CustomerId] = hc.[Id]) FROM [RPNAVConnect].[dbo].[HITCustomers] as hc ORDER BY hc.[Name] ASC";

                System.Data.OleDb.OleDbDataReader oleReader;
                System.Data.OleDb.OleDbCommand cmd = new System.Data.OleDb.OleDbCommand(sSql, dbConn);
                oleReader = cmd.ExecuteReader();

                HitBillingDataL.Text += "<table class=\"table table-bordered table-striped\" style=\"width:700px;\">";

                HitBillingDataL.Text += "<tr class=\"bg-danger text-white\">";
                HitBillingDataL.Text += "<th>Name</th>";
                HitBillingDataL.Text += "<th>Azure Group</th>";
                HitBillingDataL.Text += "<th>Navision Id</th>";
                HitBillingDataL.Text += "<th>Users #</th>";
                HitBillingDataL.Text += "<th></th>";
                HitBillingDataL.Text += "<th></th>";
                HitBillingDataL.Text += "</tr>";

                while (oleReader.Read())
                {
                    if ((!oleReader.IsDBNull(0)) && (!oleReader.IsDBNull(1)) && (!oleReader.IsDBNull(2)) && (!oleReader.IsDBNull(3)))
                    {
                        string sId = oleReader.GetInt32(0).ToString();
                        string sName = oleReader.GetString(1);
                        string sADGroup = oleReader.GetString(2);
                        string sNAVId = oleReader.GetString(3);
                        string sCustomerUsersNumber = oleReader.GetInt32(31).ToString();

                        // customer
                        HitBillingDataL.Text += "<tr>";
                        HitBillingDataL.Text += "<td>" + sName + "</td>";
                        HitBillingDataL.Text += "<td>" + sADGroup + "</td>";
                        HitBillingDataL.Text += "<td>" + sNAVId + "</td>";
                        HitBillingDataL.Text += "<td>" + sCustomerUsersNumber + "</td>";
                        HitBillingDataL.Text += "<td><a href='HitBillingCustomersDF.aspx?id=" + sId + "'>edit</a></td>";
                        HitBillingDataL.Text += "<td><a href='HitBillingDF.aspx?action=del&id=" + sId + "' onclick=\"return confirm('Are you sure to delete " + sName + "?');\">del</a></td>";
                        HitBillingDataL.Text += "</tr>";
                    }
                }
                oleReader.Close();
                dbConn.Close();

                HitBillingDataL.Text += "</table>";
            }
            catch (Exception ex)
            {
                sResult = ex.ToString();
                HitBillingDataL.Text = "Error$ " + sResult;
            }

            return sResult;
        }

        public Excel.Application xlApp = null;
        public Excel.Workbook xlWorkbook = null;
        public Excel._Worksheet xlWorksheet = null;
        public Excel.Range xlRange = null;
        public Excel._Worksheet xlWorksheet2 = null;
        public Excel.Range xlRangeHitPrices = null;
        public Excel._Worksheet xlWorksheet3 = null;
        public Excel.Range xlRangeHitLicensesPrices = null;
        public System.Diagnostics.Process[] oldExcelProcesses = null;

        private string AddToDatabase(string sNo, string sDescription, double iAmount, string sUnits, double dUnitPrice, double dPrice, string sPeriod, string sCustomerNo, string sCustomerName, OleDbConnection dbConn)
        {
            string sSqlIns = "INSERT INTO [dbo].[HITInvoices] ([No], [Desription], [Amount], [Units], [UnitPrice], [Price], [Status], [Period], [CustomerNo], [CustomerName]) ";
            sSqlIns += "VALUES (";
            sSqlIns += "'" + sNo + "', ";
            sSqlIns += "'" + sDescription + "', ";
            sSqlIns += iAmount.ToString() + ", ";
            sSqlIns += "'" + sUnits + "', ";
            sSqlIns += dUnitPrice + ", ";
            sSqlIns += dPrice + ", ";
            sSqlIns += "'New', ";
            sSqlIns += "'" + sPeriod + "', ";
            sSqlIns += "'" + sCustomerNo + "', ";
            sSqlIns += "'" + sCustomerName + "'";
            sSqlIns += ")";

            string sResult = InsertUpdateDatabase(sSqlIns, dbConn);

            return sResult;
        }

        private string GetExcelData(int sUL, int sUB, int sUE, CustomerData cd)
        {
            string sResult = "";
            string filePath = ConfigurationManager.AppSettings["excelpath"].ToString();

            string dbPath = ConfigurationManager.AppSettings["dbpath"].ToString();
            System.Data.OleDb.OleDbConnection dbConn = new System.Data.OleDb.OleDbConnection(dbPath);
            dbConn.Open();

            string sDbOk = "";

            try
            {
                try
                {
                    oldExcelProcesses = System.Diagnostics.Process.GetProcessesByName("EXCEL");

                    //Create COM Objects. Create a COM object for everything that is referenced
                    xlApp = new Excel.Application();
                    xlApp.DisplayAlerts = false;
                    xlApp.Visible = false;

                    xlWorkbook = xlApp.Workbooks.Open(filePath);

                    // Tilbud
                    xlWorksheet = xlWorkbook.Sheets["Tilbud"];
                    xlRange = xlWorksheet.UsedRange;

                    // HIT priser
                    xlWorksheet2 = xlWorkbook.Sheets["HIT priser"];
                    xlRangeHitPrices = xlWorksheet2.UsedRange;

                    // HIT basis licenser
                    xlWorksheet3 = xlWorkbook.Sheets["HIT basis licenser"];
                    xlRangeHitLicensesPrices = xlWorksheet3.UsedRange;

                    // set user numbers
                    xlRange.Cells[2, 2] = sUE;
                    xlRange.Cells[3, 2] = sUB;
                    xlRange.Cells[4, 2] = sUL;

                    // Længde af kontrakt
                    xlRange.Cells[6, 2] = cd.sLak;

                    // set hit indstillinger
                    xlRange.Cells[10, 2] = cd.sHrw;
                    xlRange.Cells[11, 2] = cd.sHmn;
                    xlRange.Cells[12, 2] = cd.sSdm;
                    xlRange.Cells[13, 2] = cd.sSrpm;
                    xlRange.Cells[14, 2] = cd.sSrpmf;
                    xlRange.Cells[15, 2] = cd.sHmfa;
                    xlRange.Cells[16, 2] = cd.sHsikk;
                    xlRange.Cells[17, 2] = cd.sScsm;
                    xlRange.Cells[18, 2] = cd.sHmfsi;
                    xlRange.Cells[19, 2] = cd.sSwtene;
                    xlRange.Cells[20, 2] = cd.sMdm;

                    // Netværksudstyr
                    xlRange.Cells[23, 2] = cd.sFir;
                    xlRange.Cells[24, 2] = cd.sWifi;
                    xlRange.Cells[25, 2] = cd.sSwi24;
                    xlRange.Cells[26, 2] = cd.sSwi8;
                    xlRange.Cells[27, 2] = cd.sSwi48;
                    xlRange.Cells[28, 2] = cd.sEksi;
                    xlRange.Cells[29, 2] = cd.sDns;

                    // Ekstra tilføjelser
                    xlRange.Cells[32, 2] = cd.sPrin;
                    xlRange.Cells[33, 2] = cd.sBckep;
                    xlRange.Cells[34, 2] = cd.sOnsit;

                    // refresh all macros
                    //xlWorkbook.RefreshAll();

                    //System.Threading.Thread.Sleep(3000);

                    sResult = "<table class=\"table table-bordered table-striped\" style=\"width:700px;\">";

                    sResult += "<tr class=\"bg-danger text-white\">";
                    sResult += "<th>No</th>";
                    sResult += "<th>Desription</th>";
                    sResult += "<th>Amount</th>";
                    sResult += "<th>Units</th>";
                    sResult += "<th>Unit price</th>";
                    sResult += "<th>Price</th>";
                    sResult += "</tr>";

                    // CultureInfo.CurrentCulture.DateTimeFormat.GetAbbreviatedMonthName(dateTime.Month);
                    string sPeriod = "HIT " + CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(DateTime.Now.Month) + " " + DateTime.Now.Year.ToString();

                    sResult += "<tr>";
                    sResult += "<td></td>";
                    sResult += "<td>" + sPeriod + "</td>";
                    sResult += "<td></td>";
                    sResult += "<td></td>";
                    sResult += "<td></td>";
                    sResult += "<td></td>";
                    sResult += "</tr>";

                    sDbOk = AddToDatabase("", sPeriod, -1, "", -1, -1, sPeriod, cd.sNAVId, cd.sName, dbConn);
                    if (sDbOk != "DBOK")
                    {
                        sResult += "<tr><tdcolspan='6'>" + sDbOk + "</td></tr>";
                    }

                    double dSum = 0;

                    // light users
                    if (sUL > 0)
                    {
                        double dUM = 0;
                        double dUMSUM = 0;
                        double dUML = 0;
                        double dUMLSUM = 0;

                        if (xlRangeHitPrices.Cells[17, 12] != null && xlRangeHitPrices.Cells[17, 12].Value2 != null)
                        {
                            if (xlRangeHitLicensesPrices.Cells[6, 15] != null && xlRangeHitLicensesPrices.Cells[6, 15].Value2 != null)
                            {
                                try
                                {
                                    dUM = xlRangeHitPrices.Cells[17, 12].Value2;
                                    dUML = xlRangeHitLicensesPrices.Cells[6, 15].Value2;
                                    dUMSUM = dUM * sUL;
                                    dUMLSUM = dUML * sUL;
                                    dSum += dUMSUM + dUMLSUM;
                                }
                                catch (Exception ex)
                                {
                                    ex.ToString();
                                }
                            }
                        }

                        // users
                        sResult += "<tr>";
                        sResult += "<td>10059</td>";
                        sResult += "<td>HIT Light users</td>";
                        sResult += "<td>" + sUL.ToString() + "</td>";
                        sResult += "<td>Bruger/md (user/month)</td>";
                        sResult += "<td>" + dUM.ToString("N") + "</td>";
                        sResult += "<td>" + dUMSUM.ToString("N") + "</td>";
                        sResult += "</tr>";

                        // add to db
                        sDbOk = AddToDatabase("10059", "HIT Light users", sUL, "Bruger/md (user/month)", dUM, dUMSUM, sPeriod, cd.sNAVId, cd.sName, dbConn);
                        if (sDbOk != "DBOK")
                        {
                            sResult += "<tr><tdcolspan='6'>" + sDbOk + "</td></tr>";
                        }

                        // ms licences
                        sResult += "<tr>";
                        sResult += "<td>310</td>";
                        sResult += "<td>Microsoft license – HIT Light​</td>";
                        sResult += "<td>" + sUL.ToString() + "</td>";
                        sResult += "<td>Bruger/md (user/month)</td>";
                        sResult += "<td>" + dUML.ToString("N") + "</td>";
                        sResult += "<td>" + dUMLSUM.ToString("N") + "</td>";
                        sResult += "</tr>";

                        // add to db
                        sDbOk = AddToDatabase("310", "Microsoft license – HIT Light", sUL, "Bruger/md (user/month)", dUML, dUMLSUM, sPeriod, cd.sNAVId, cd.sName, dbConn);
                        if (sDbOk != "DBOK")
                        {
                            sResult += "<tr><tdcolspan='6'>" + sDbOk + "</td></tr>";
                        }
                    }

                    // enterprize users
                    if (sUE > 0)
                    {
                        double dUM = 0;
                        double dUMSUM = 0;
                        double dUML = 0;
                        double dUMLSUM = 0;

                        if (xlRangeHitPrices.Cells[16, 12] != null && xlRangeHitPrices.Cells[16, 12].Value2 != null)
                        {
                            string sBMOSDesc = xlRange.Cells[19, 2].Value2;
                            int iWinCol = -1;
                            if (sBMOSDesc == "Ja")
                            {
                                iWinCol = 7;
                            }
                            if (sBMOSDesc == "Nej")
                            {
                                iWinCol = 3;
                            }

                            if (xlRangeHitLicensesPrices.Cells[6, iWinCol] != null && xlRangeHitLicensesPrices.Cells[6, iWinCol].Value2 != null)
                            {
                                try
                                {
                                    dUM = xlRangeHitPrices.Cells[16, 12].Value2;
                                    dUML = xlRangeHitLicensesPrices.Cells[6, iWinCol].Value2;                                  
                                    dUMSUM = dUM * sUE;
                                    dUMLSUM = dUML * sUE;
                                    dSum += dUMSUM + dUMLSUM;
                                }
                                catch (Exception ex)
                                {
                                    ex.ToString();
                                }
                            }
                        }

                        // users
                        sResult += "<tr>";
                        sResult += "<td>10059</td>";
                        sResult += "<td>HIT Enterprise users</td>";
                        sResult += "<td>" + sUE.ToString() + "</td>";
                        sResult += "<td>Bruger/md (user/month)</td>";
                        sResult += "<td>" + dUM.ToString("N") + "</td>";
                        sResult += "<td>" + dUMSUM.ToString("N") + "</td>";
                        sResult += "</tr>";

                        // add to db
                        sDbOk = AddToDatabase("10059", "HIT Enterprise users", sUE, "Bruger/md (user/month)", dUM, dUMSUM, sPeriod, cd.sNAVId, cd.sName, dbConn);
                        if (sDbOk != "DBOK")
                        {
                            sResult += "<tr><tdcolspan='6'>" + sDbOk + "</td></tr>";
                        }

                        // ms licences
                        sResult += "<tr>";
                        sResult += "<td>310</td>";
                        sResult += "<td>Microsoft license – HIT Enterprise​</td>";
                        sResult += "<td>" + sUE.ToString() + "</td>";
                        sResult += "<td>Bruger/md (user/month)</td>";
                        sResult += "<td>" + dUML.ToString("N") + "</td>";
                        sResult += "<td>" + dUMLSUM.ToString("N") + "</td>";
                        sResult += "</tr>";

                        // add to db
                        sDbOk = AddToDatabase("310", "Microsoft license – HIT Enterprise", sUE, "Bruger/md (user/month)", dUML, dUMLSUM, sPeriod, cd.sNAVId, cd.sName, dbConn);
                        if (sDbOk != "DBOK")
                        {
                            sResult += "<tr><tdcolspan='6'>" + sDbOk + "</td></tr>";
                        }
                    }

                    // business users
                    if (sUB > 0)
                    {
                        double dUM = 0;
                        double dUMSUM = 0;
                        double dUML = 0;
                        double dUMLSUM = 0;

                        if (xlRangeHitPrices.Cells[16, 12] != null && xlRangeHitPrices.Cells[16, 12].Value2 != null)
                        {
                            if (xlRangeHitLicensesPrices.Cells[6, 11] != null && xlRangeHitLicensesPrices.Cells[6, 11].Value2 != null)
                            {
                                try
                                {
                                    dUM = xlRangeHitPrices.Cells[16, 12].Value2;
                                    dUML = xlRangeHitLicensesPrices.Cells[6, 11].Value2;
                                    dUMSUM = dUM * sUB;
                                    dUMLSUM = dUML * sUB;
                                    dSum += dUMSUM + dUMLSUM;
                                }
                                catch (Exception ex)
                                {
                                    ex.ToString();
                                }
                            }
                        }

                        // users
                        sResult += "<tr>";
                        sResult += "<td>10059</td>";
                        sResult += "<td>HIT Business users</td>";
                        sResult += "<td>" + sUB.ToString() + "</td>";
                        sResult += "<td>Bruger/md (user/month)</td>";
                        sResult += "<td>" + dUM.ToString("N") + "</td>";
                        sResult += "<td>" + dUMSUM.ToString("N") + "</td>";
                        sResult += "</tr>";

                        // add to db
                        sDbOk = AddToDatabase("10059", "HIT Business users", sUB, "Bruger/md (user/month)", dUM, dUMSUM, sPeriod, cd.sNAVId, cd.sName, dbConn);
                        if (sDbOk != "DBOK")
                        {
                            sResult += "<tr><tdcolspan='6'>" + sDbOk + "</td></tr>";
                        }

                        // ms licences
                        sResult += "<tr>";
                        sResult += "<td>310</td>";
                        sResult += "<td>Microsoft license – HIT Business​</td>";
                        sResult += "<td>" + sUB.ToString() + "</td>";
                        sResult += "<td>Bruger/md (user/month)</td>";
                        sResult += "<td>" + dUML.ToString("N") + "</td>";
                        sResult += "<td>" + dUMLSUM.ToString("N") + "</td>";
                        sResult += "</tr>";

                        // add to db
                        sDbOk = AddToDatabase("310", "Microsoft license – HIT Business", sUB, "Bruger/md (user/month)", dUML, dUMLSUM, sPeriod, cd.sNAVId, cd.sName, dbConn);
                        if (sDbOk != "DBOK")
                        {
                            sResult += "<tr><tdcolspan='6'>" + sDbOk + "</td></tr>";
                        }
                    }

                    // Backup af mail/mail, onedrive og sharepoint filer
                    if (xlRange.Cells[18, 2] != null && xlRange.Cells[18, 2].Value2 != null)
                    {
                        string sBMOSDesc = xlRange.Cells[18, 2].Value2;
                        double sBMOS = sUL + sUB + sUE;
                        string sNAVPN = "";
                        int iMOSRow = -1;
                        if (sBMOSDesc == "Mail, Onedrive og Sharepoint filer")
                        {
                            sNAVPN = "1347";
                            iMOSRow = 89;
                        }
                        if (sBMOSDesc == "Mail")
                        {
                            sNAVPN = "1346";
                            iMOSRow = 88;
                        }

                        if (sNAVPN != "")
                        {
                            if (xlRangeHitPrices.Cells[iMOSRow, 12] != null && xlRangeHitPrices.Cells[iMOSRow, 12].Value2 != null)
                            {
                                double sBMOSPris = xlRangeHitPrices.Cells[iMOSRow, 12].Value2;
                                double sBMOSPrisSum = sBMOS * sBMOSPris;
                                dSum += sBMOSPrisSum;

                                sResult += "<tr>";
                                sResult += "<td>" + sNAVPN + "</td>";
                                sResult += "<td>Backup af mail/mail, onedrive og sharepoint filer</td>";
                                sResult += "<td>" + sBMOS.ToString() + "</td>";
                                sResult += "<td>Bruger/md (user/month)</td>";
                                sResult += "<td>" + sBMOSPris.ToString("N") + "</td>";
                                sResult += "<td>" + sBMOSPrisSum.ToString("N") + "</td>";
                                sResult += "</tr>";

                                // add to db
                                sDbOk = AddToDatabase(sNAVPN, "Backup af mail/mail, onedrive og sharepoint filer", sBMOS, "Bruger/md (user/month)", sBMOSPris, sBMOSPrisSum, sPeriod, cd.sNAVId, cd.sName, dbConn);
                                if (sDbOk != "DBOK")
                                {
                                    sResult += "<tr><tdcolspan='6'>" + sDbOk + "</td></tr>";
                                }
                            }
                        }
                    }

                    // firewall
                    if (xlRange.Cells[23, 2] != null && xlRange.Cells[23, 2].Value2 != null)
                    {
                        double sFirWall = xlRange.Cells[23, 2].Value2;
                        if (xlRangeHitPrices.Cells[78, 12] != null && xlRangeHitPrices.Cells[78, 12].Value2 != null)
                        {
                            double sFireWallPris = xlRangeHitPrices.Cells[78, 12].Value2;
                            double sFireWallPrisSum = sFirWall * sFireWallPris;
                            dSum += sFireWallPrisSum;

                            sResult += "<tr>";
                            sResult += "<td>235</td>";
                            sResult += "<td>Firewall(s)​</td>";
                            sResult += "<td>" + sFirWall.ToString() + "</td>";
                            sResult += "<td></td>";
                            sResult += "<td>" + sFireWallPris.ToString("N") + "</td>";
                            sResult += "<td>" + sFireWallPrisSum.ToString("N") + "</td>";
                            sResult += "</tr>";

                            // add to db
                            sDbOk = AddToDatabase("235", "Firewall(s)", sFirWall, "Bruger/md (user/month)", sFireWallPris, sFireWallPrisSum, sPeriod, cd.sNAVId, cd.sName, dbConn);
                            if (sDbOk != "DBOK")
                            {
                                sResult += "<tr><tdcolspan='6'>" + sDbOk + "</td></tr>";
                            }
                        }
                    }

                    // WIFI - AccessPunkt(er)
                    if (xlRange.Cells[24, 2] != null && xlRange.Cells[24, 2].Value2 != null)
                    {
                        double sAP = xlRange.Cells[24, 2].Value2;
                        if (xlRangeHitPrices.Cells[76, 12] != null && xlRangeHitPrices.Cells[76, 12].Value2 != null)
                        {
                            double sAPPris = xlRangeHitPrices.Cells[76, 12].Value2;
                            double sAPPrisSum = sAP * sAPPris;
                            dSum += sAPPrisSum;

                            sResult += "<tr>";
                            sResult += "<td>225</td>";
                            sResult += "<td>WIFI - AccessPunkt(er)​</td>";
                            sResult += "<td>" + sAP.ToString() + "</td>";
                            sResult += "<td></td>";
                            sResult += "<td>" + sAPPris.ToString("N") + "</td>";
                            sResult += "<td>" + sAPPrisSum.ToString("N") + "</td>";
                            sResult += "</tr>";

                            // add to db
                            sDbOk = AddToDatabase("225", "WIFI - AccessPunkt(er)​", sAP, "", sAPPris, sAPPrisSum, sPeriod, cd.sNAVId, cd.sName, dbConn);
                            if (sDbOk != "DBOK")
                            {
                                sResult += "<tr><tdcolspan='6'>" + sDbOk + "</td></tr>";
                            }
                        }
                    }

                    // Switch (24 porte)
                    if (xlRange.Cells[25, 2] != null && xlRange.Cells[25, 2].Value2 != null)
                    {
                        double sSW = xlRange.Cells[25, 2].Value2;
                        if (xlRangeHitPrices.Cells[77, 12] != null && xlRangeHitPrices.Cells[77, 12].Value2 != null)
                        {
                            double sSWPris = xlRangeHitPrices.Cells[77, 12].Value2;
                            double sSWPrisSum = sSW * sSWPris;
                            dSum += sSWPrisSum;

                            sResult += "<tr>";
                            sResult += "<td>230</td>";
                            sResult += "<td>Switch (24 porte)​</td>";
                            sResult += "<td>" + sSW.ToString() + "</td>";
                            sResult += "<td></td>";
                            sResult += "<td>" + sSWPris.ToString("N") + "</td>";
                            sResult += "<td>" + sSWPrisSum.ToString("N") + "</td>";
                            sResult += "</tr>";

                            // add to db
                            sDbOk = AddToDatabase("230", "Switch (24 porte)​", sSW, "", sSWPris, sSWPrisSum, sPeriod, cd.sNAVId, cd.sName, dbConn);
                            if (sDbOk != "DBOK")
                            {
                                sResult += "<tr><tdcolspan='6'>" + sDbOk + "</td></tr>";
                            }
                        }
                    }

                    // Switch (8 porte)
                    if (xlRange.Cells[26, 2] != null && xlRange.Cells[26, 2].Value2 != null)
                    {
                        double sSW = xlRange.Cells[26, 2].Value2;
                        if (xlRangeHitPrices.Cells[81, 12] != null && xlRangeHitPrices.Cells[81, 12].Value2 != null)
                        {
                            double sSWPris = xlRangeHitPrices.Cells[81, 12].Value2;
                            double sSWPrisSum = sSW * sSWPris;
                            dSum += sSWPrisSum;

                            sResult += "<tr>";
                            sResult += "<td>230</td>";
                            sResult += "<td>Switch (8 porte)</td>";
                            sResult += "<td>" + sSW.ToString() + "</td>";
                            sResult += "<td></td>";
                            sResult += "<td>" + sSWPris.ToString("N") + "</td>";
                            sResult += "<td>" + sSWPrisSum.ToString("N") + "</td>";
                            sResult += "</tr>";

                            // add to db
                            sDbOk = AddToDatabase("230", "Switch (8 porte)​", sSW, "", sSWPris, sSWPrisSum, sPeriod, cd.sNAVId, cd.sName, dbConn);
                            if (sDbOk != "DBOK")
                            {
                                sResult += "<tr><tdcolspan='6'>" + sDbOk + "</td></tr>";
                            }
                        }
                    }

                    // Switch (48 porte)
                    if (xlRange.Cells[27, 2] != null && xlRange.Cells[27, 2].Value2 != null)
                    {
                        double sSW = xlRange.Cells[27, 2].Value2;
                        if (xlRangeHitPrices.Cells[80, 12] != null && xlRangeHitPrices.Cells[80, 12].Value2 != null)
                        {
                            double sSWPris = xlRangeHitPrices.Cells[80, 12].Value2;
                            double sSWPrisSum = sSW * sSWPris;
                            dSum += sSWPrisSum;

                            sResult += "<tr>";
                            sResult += "<td>230</td>";
                            sResult += "<td>Switch (48 porte)​</td>";
                            sResult += "<td>" + sSW.ToString() + "</td>";
                            sResult += "<td></td>";
                            sResult += "<td>" + sSWPris.ToString("N") + "</td>";
                            sResult += "<td>" + sSWPrisSum.ToString("N") + "</td>";
                            sResult += "</tr>";

                            // add to db
                            sDbOk = AddToDatabase("230", "Switch (48 porte)​", sSW, "", sSWPris, sSWPrisSum, sPeriod, cd.sNAVId, cd.sName, dbConn);
                            if (sDbOk != "DBOK")
                            {
                                sResult += "<tr><tdcolspan='6'>" + sDbOk + "</td></tr>";
                            }
                        }
                    }

                    // Eksisterende Ubiquity udstyr (antal som skal ind i løsningen)
                    if (xlRange.Cells[28, 2] != null && xlRange.Cells[28, 2].Value2 != null)
                    {
                        double sEKSI = xlRange.Cells[28, 2].Value2;
                        if (xlRangeHitPrices.Cells[75, 12] != null && xlRangeHitPrices.Cells[75, 12].Value2 != null)
                        {
                            double sEKSIPris = xlRangeHitPrices.Cells[75, 12].Value2;
                            double sEKSIPrisSum = sEKSI * sEKSIPris;
                            dSum += sEKSIPrisSum;

                            sResult += "<tr>";
                            sResult += "<td>220</td>";
                            sResult += "<td>Eksisterende Ubiquity udstyr (antal som skal ind i løsningen)</td>";
                            sResult += "<td>" + sEKSI.ToString() + "</td>";
                            sResult += "<td></td>";
                            sResult += "<td>" + sEKSIPris.ToString("N") + "</td>";
                            sResult += "<td>" + sEKSIPrisSum.ToString("N") + "</td>";
                            sResult += "</tr>";

                            // add to db
                            sDbOk = AddToDatabase("220", "Eksisterende Ubiquity udstyr(antal som skal ind i løsningen)", sEKSI, "", sEKSIPris, sEKSIPrisSum, sPeriod, cd.sNAVId, cd.sName, dbConn);
                            if (sDbOk != "DBOK")
                            {
                                sResult += "<tr><tdcolspan='6'>" + sDbOk + "</td></tr>";
                            }
                        }
                    }

                    // DNS administration (gængse domæner ex .com o.lign)
                    if (xlRange.Cells[29, 2] != null && xlRange.Cells[29, 2].Value2 != null)
                    {
                        double sDNS = xlRange.Cells[29, 2].Value2;
                        if (xlRangeHitPrices.Cells[79, 12] != null && xlRangeHitPrices.Cells[79, 12].Value2 != null)
                        {
                            double sDNSPris = xlRangeHitPrices.Cells[79, 12].Value2;
                            double sDNSPrisSum = sDNS * sDNSPris;
                            dSum += sDNSPrisSum;

                            sResult += "<tr>";
                            sResult += "<td>10160</td>";
                            sResult += "<td>DNS administration (gængse domæner ex .com o.lign)</td>";
                            sResult += "<td>" + sDNS.ToString() + "</td>";
                            sResult += "<td></td>";
                            sResult += "<td>" + sDNSPris.ToString("N") + "</td>";
                            sResult += "<td>" + sDNSPrisSum.ToString("N") + "</td>";
                            sResult += "</tr>";

                            // add to db
                            sDbOk = AddToDatabase("10160", "DNS administration (gængse domæner ex .com o.lign)", sDNS, "", sDNSPris, sDNSPrisSum, sPeriod, cd.sNAVId, cd.sName, dbConn);
                            if (sDbOk != "DBOK")
                            {
                                sResult += "<tr><tdcolspan='6'>" + sDbOk + "</td></tr>";
                            }
                        }
                    }

                    // Printer(e)
                    if (xlRange.Cells[32, 2] != null && xlRange.Cells[32, 2].Value2 != null)
                    {
                        double sPRINT = xlRange.Cells[32, 2].Value2;
                        if (xlRangeHitPrices.Cells[92, 12] != null && xlRangeHitPrices.Cells[92, 12].Value2 != null)
                        {
                            double sPRINTPris = xlRangeHitPrices.Cells[92, 12].Value2;
                            double sPRINTPrisSum = sPRINT * sPRINTPris;
                            dSum += sPRINTPrisSum;

                            sResult += "<tr>";
                            sResult += "<td>10059</td>";
                            sResult += "<td>Printer(e)</td>";
                            sResult += "<td>" + sPRINT.ToString() + "</td>";
                            sResult += "<td></td>";
                            sResult += "<td>" + sPRINTPris.ToString("N") + "</td>";
                            sResult += "<td>" + sPRINTPrisSum.ToString("N") + "</td>";
                            sResult += "</tr>";

                            // add to db
                            sDbOk = AddToDatabase("10059", "Printer(e)", sPRINT, "", sPRINTPris, sPRINTPrisSum, sPeriod, cd.sNAVId, cd.sName, dbConn);
                            if (sDbOk != "DBOK")
                            {
                                sResult += "<tr><tdcolspan='6'>" + sDbOk + "</td></tr>";
                            }
                        }
                    }

                    // Backup af ekstra postkasser
                    if (xlRange.Cells[33, 2] != null && xlRange.Cells[33, 2].Value2 != null)
                    {
                        double sBEP = xlRange.Cells[33, 2].Value2;
                        if (xlRangeHitPrices.Cells[90, 12] != null && xlRangeHitPrices.Cells[90, 12].Value2 != null)
                        {
                            double sBEPPris = xlRangeHitPrices.Cells[90, 12].Value2;
                            double sBEPPrisSum = sBEP * sBEPPris;
                            dSum += sBEPPrisSum;

                            sResult += "<tr>";
                            sResult += "<td>1346</td>";
                            sResult += "<td>Backup af ekstra postkasser</td>";
                            sResult += "<td>" + sBEP.ToString() + "</td>";
                            sResult += "<td></td>";
                            sResult += "<td>" + sBEPPris.ToString("N") + "</td>";
                            sResult += "<td>" + sBEPPrisSum.ToString("N") + "</td>";
                            sResult += "</tr>";

                            // add to db
                            sDbOk = AddToDatabase("1346", "Backup af ekstra postkasser", sBEP, "", sBEPPris, sBEPPrisSum, sPeriod, cd.sNAVId, cd.sName, dbConn);
                            if (sDbOk != "DBOK")
                            {
                                sResult += "<tr><tdcolspan='6'>" + sDbOk + "</td></tr>";
                            }
                        }
                    }

                    // Onsite support (OBS! kun Sjælland - angives I halve dage)
                    if (xlRange.Cells[34, 2] != null && xlRange.Cells[34, 2].Value2 != null)
                    {
                        double sOBS = xlRange.Cells[34, 2].Value2;
                        if (xlRangeHitPrices.Cells[93, 12] != null && xlRangeHitPrices.Cells[93, 12].Value2 != null)
                        {
                            double sOBSPris = xlRangeHitPrices.Cells[93, 12].Value2;
                            double sOBSPrisSum = sOBS * sOBSPris;
                            dSum += sOBSPrisSum;

                            sResult += "<tr>";
                            sResult += "<td>100</td>";
                            sResult += "<td>Onsite support (OBS kun Sjælland - angives I halve dage)</td>";
                            sResult += "<td>" + sOBS.ToString() + "</td>";
                            sResult += "<td></td>";
                            sResult += "<td>" + sOBSPris.ToString("N") + "</td>";
                            sResult += "<td>" + sOBSPrisSum.ToString("N") + "</td>";
                            sResult += "</tr>";

                            // add to db
                            sDbOk = AddToDatabase("10160", "Onsite support (OBS kun Sjælland - angives I halve dage)", sOBS, "", sOBSPris, sOBSPrisSum, sPeriod, cd.sNAVId, cd.sName, dbConn);
                            if (sDbOk != "DBOK")
                            {
                                sResult += "<tr><tdcolspan='6'>" + sDbOk + "</td></tr>";
                            }
                        }
                    }

                    // supportfee
                    double dSum15 = (dSum * 15) / 100;

                    // Supportfee (15%)​
                    sResult += "<tr>";
                    sResult += "<td>610</td>";
                    sResult += "<td>Supportfee (15%)​</td>";
                    sResult += "<td></td>";
                    sResult += "<td></td>";
                    sResult += "<td></td>";
                    sResult += "<td>" + dSum15.ToString("N") + "</td>";
                    sResult += "</tr>";

                    // add to db
                    sDbOk = AddToDatabase("610", "Supportfee (15%)​", 1, "", dSum15, dSum15, sPeriod, cd.sNAVId, cd.sName, dbConn);
                    if (sDbOk != "DBOK")
                    {
                        sResult += "<tr><tdcolspan='6'>" + sDbOk + "</td></tr>";
                    }

                    sResult += "</table>";
                }
                catch (Exception ex)
                {
                    sResult += "ErrorGED1: " + ex.ToString();
                }
                finally
                {
                    //close and release
                    xlWorkbook.Close(null, null, null);

                    //quit and release
                    xlApp.Quit();

                    if (xlApp != null)
                    {
                        Marshal.ReleaseComObject(xlApp);
                    }

                    if (xlWorkbook != null)
                    {
                        Marshal.ReleaseComObject(xlWorkbook);
                    }

                    if (xlRangeHitPrices != null)
                    {
                        Marshal.ReleaseComObject(xlRangeHitPrices);
                    }

                    if (xlWorksheet2 != null)
                    {
                        Marshal.ReleaseComObject(xlWorksheet2);
                    }

                    if (xlRange != null)
                    {
                        Marshal.ReleaseComObject(xlRange);
                    }

                    if (xlWorksheet != null)
                    {
                        Marshal.ReleaseComObject(xlWorksheet);
                    }

                    //cleanup
                    GC.Collect();
                    GC.WaitForPendingFinalizers();

                    //cleanup
                    GC.Collect();
                    GC.WaitForPendingFinalizers();

                    // kill the process if still around
                    if (xlApp != null)
                    {
                        System.Diagnostics.Process[] excelProcsNew = System.Diagnostics.Process.GetProcessesByName("EXCEL");
                        foreach (System.Diagnostics.Process procNew in excelProcsNew)
                        {
                            int exist = 0;
                            foreach (System.Diagnostics.Process procOld in oldExcelProcesses)
                            {
                                if (procNew.Id == procOld.Id)
                                {
                                    exist++;
                                }
                            }
                            if (exist == 0)
                            {
                                procNew.Kill();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                sResult += "ErrorGED2: " + ex.ToString();
            }

            dbConn.Close();

            PushDataToNavB.Visible = true;

            return sResult;
        }

        struct CustomerData
        {
            public string sId;
            public string sName;
            public string sADGroup;
            public string sNAVId;

            public string sClientId;
            public string sClientSecret;
            public string sClientScopesApplication;
            public string sTenant;
            public string sRedirectUri;

            public string sHrw;
            public string sHmn;
            public string sSdm;
            public string sSrpm;
            public string sSrpmf;
            public string sHmfa;
            public string sHsikk;
            public string sScsm;
            public string sHmfsi;
            public string sSwtene;
            public string sMdm;
            public string sLak;

            public string sPrin;
            public string sBckep;
            public string sOnsit;
            public string sFir;
            public string sWifi;
            public string sSwi24;
            public string sSwi8;
            public string sSwi48;
            public string sEksi;
            public string sDns;

            public int sCustomerUsersNumber;
        }

        protected void HitBillingDataB_Click(object sender, EventArgs e)
        {
            CustomerData cd = new CustomerData();

            try
            {
                string dbPath = ConfigurationManager.AppSettings["dbpath"].ToString();
                System.Data.OleDb.OleDbConnection dbConn = new System.Data.OleDb.OleDbConnection(dbPath);
                dbConn.Open();

                // delete not used old invoice data
                string sSqlDel = "DELETE FROM [dbo].[HITInvoices] WHERE [Status] = 'New'";
                string sDbOk = InsertUpdateDatabase(sSqlDel, dbConn);

                HitBillingInvoiceDataL.Text = "";

                string sSql = "SELECT hc.*, (SELECT Count(*) FROM [RPNAVConnect].[dbo].[HITUsers] as hcu WHERE hcu.[CustomerId] = hc.[Id]) FROM [RPNAVConnect].[dbo].[HITCustomers] as hc ORDER BY hc.[Name] ASC";

                System.Data.OleDb.OleDbDataReader oleReader;
                System.Data.OleDb.OleDbCommand cmd = new System.Data.OleDb.OleDbCommand(sSql, dbConn);
                oleReader = cmd.ExecuteReader();

                while (oleReader.Read())
                {
                    if ((!oleReader.IsDBNull(0)) && (!oleReader.IsDBNull(1)) && (!oleReader.IsDBNull(2)) && (!oleReader.IsDBNull(3)))
                    {
                        // get customer data
                        cd.sId = oleReader.GetInt32(0).ToString();
                        cd.sName = oleReader.GetString(1);
                        cd.sADGroup = oleReader.GetString(2);
                        cd.sNAVId = oleReader.GetString(3);

                        cd.sClientId = oleReader.GetString(4);
                        cd.sClientSecret = oleReader.GetString(5);
                        cd.sClientScopesApplication = oleReader.GetString(6);
                        cd.sTenant = oleReader.GetString(7);
                        cd.sRedirectUri = oleReader.GetString(8);

                        if (!oleReader.IsDBNull(9)) cd.sHrw = oleReader.GetString(9);
                        if (!oleReader.IsDBNull(10)) cd.sHmn = oleReader.GetString(10);
                        if (!oleReader.IsDBNull(11)) cd.sSdm = oleReader.GetString(11);
                        if (!oleReader.IsDBNull(12)) cd.sSrpm = oleReader.GetString(12);
                        if (!oleReader.IsDBNull(13)) cd.sSrpmf = oleReader.GetString(13);
                        if (!oleReader.IsDBNull(14)) cd.sHmfa = oleReader.GetString(14);
                        if (!oleReader.IsDBNull(15)) cd.sHsikk = oleReader.GetString(15);
                        if (!oleReader.IsDBNull(16)) cd.sScsm = oleReader.GetString(16);
                        if (!oleReader.IsDBNull(17)) cd.sHmfsi = oleReader.GetString(17);
                        if (!oleReader.IsDBNull(18)) cd.sSwtene = oleReader.GetString(18);
                        if (!oleReader.IsDBNull(19)) cd.sMdm = oleReader.GetString(19);
                        if (!oleReader.IsDBNull(20)) cd.sLak = oleReader.GetString(20);

                        if (!oleReader.IsDBNull(21)) cd.sPrin = oleReader.GetString(21);
                        if (!oleReader.IsDBNull(22)) cd.sBckep = oleReader.GetString(22);
                        if (!oleReader.IsDBNull(23)) cd.sOnsit = oleReader.GetString(23);
                        if (!oleReader.IsDBNull(24)) cd.sFir = oleReader.GetString(24);
                        if (!oleReader.IsDBNull(25)) cd.sWifi = oleReader.GetString(25);
                        if (!oleReader.IsDBNull(26)) cd.sSwi24 = oleReader.GetString(26);
                        if (!oleReader.IsDBNull(27)) cd.sSwi8 = oleReader.GetString(27);
                        if (!oleReader.IsDBNull(28)) cd.sSwi48 = oleReader.GetString(28);
                        if (!oleReader.IsDBNull(29)) cd.sEksi = oleReader.GetString(29);
                        if (!oleReader.IsDBNull(30)) cd.sDns = oleReader.GetString(30);

                        cd.sCustomerUsersNumber = oleReader.GetInt32(31);

                        if (cd.sCustomerUsersNumber != 0)
                        {

                            // get user data
                            string sSqlU = "SELECT hu.* FROM [RPNAVConnect].[dbo].[HITUsers] as hu WHERE hu.[CustomerId] = " + cd.sId + " ORDER BY hu.[UserName] ASC";

                            System.Data.OleDb.OleDbDataReader oleReaderU;
                            System.Data.OleDb.OleDbCommand cmdU = new System.Data.OleDb.OleDbCommand(sSqlU, dbConn);
                            oleReaderU = cmdU.ExecuteReader();

                            int iTypeLight = 0;
                            int iTypeBusiness = 0;
                            int iTypeEnterprise = 0;

                            while (oleReaderU.Read())
                            {
                                if ((!oleReaderU.IsDBNull(0)) && (!oleReaderU.IsDBNull(1)) && (!oleReaderU.IsDBNull(2)) && (!oleReaderU.IsDBNull(3)) && (!oleReaderU.IsDBNull(4)) && (!oleReaderU.IsDBNull(5)))
                                {
                                    string sUserId = oleReaderU.GetInt32(0).ToString();
                                    string sUserName = oleReaderU.GetString(2);
                                    string sUserEmail = oleReaderU.GetString(3);
                                    string sAzureId = oleReaderU.GetString(4);
                                    string sUserType = oleReaderU.GetString(5);

                                    if (sUserType == "Hverdags IT Light") iTypeLight++; 
                                    if (sUserType == "Hverdags IT Business") iTypeBusiness++;
                                    if (sUserType == "Hverdags IT Enterprise") iTypeEnterprise++;
                                }
                            }
                            oleReaderU.Close();

                            HitBillingInvoiceDataL.Text += "<br />";
                            HitBillingInvoiceDataL.Text += "<font size=\"4\"><b>" + cd.sName + "</b> (brugere antal - light: " + iTypeLight.ToString() + "; business: " + iTypeBusiness.ToString() + "; enterprise: " + iTypeEnterprise.ToString() + ")</font><br /><br />";
                            HitBillingInvoiceDataL.Text += GetExcelData(iTypeLight, iTypeBusiness, iTypeEnterprise, cd);
                            HitBillingInvoiceDataL.Text += "<br /><hr /><br />";
                        }
                    }
                }
                oleReader.Close();

                if (HitBillingInvoiceDataL.Text.IndexOf("<br />") != -1)
                {
                    HitBillingInvoiceDataL.Text = HitBillingInvoiceDataL.Text.Substring(0, HitBillingInvoiceDataL.Text.LastIndexOf("<br />"));
                }

                dbConn.Close();
            }
            catch (Exception ex)
            {
                TLInfoLabel.Text = "Error$ " + ex.ToString();
            }

        }

        protected void PushDataToNavB_Click(object sender, EventArgs e)
        {
            System.Threading.Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");

            string sResult = "";
            string sCustNo = "n/a";

            try
            {
                // action for navision
                string sNAVLogin = "rpnavapi";
                string sNAVPassword = "Telefon1";

                // get access to NAVDebtor
                CustomerInfo2_Service nav = new CustomerInfo2_Service();
                nav.UseDefaultCredentials = true;
                nav.Credentials = new NetworkCredential(sNAVLogin, sNAVPassword);

                // get access to NAVSalgsordre
                SalesInvoice_Service_Service sal = new SalesInvoice_Service_Service();
                sal.UseDefaultCredentials = true;
                sal.Credentials = new NetworkCredential(sNAVLogin, sNAVPassword);

                SalesInvoice_Service order = null;
                List<Sales_Invoice_Line> InvoiceLinesList = null;
                int iInvoiceLinesCount = 0;

                string dbPath = ConfigurationManager.AppSettings["dbpath"].ToString();
                System.Data.OleDb.OleDbConnection dbConn = new System.Data.OleDb.OleDbConnection(dbPath);
                dbConn.Open();

                HitBillingDataL.Text = "";
                string sSql = "SELECT * FROM [RPNAVConnect].[dbo].[HITInvoices] WHERE [Status] = 'New' AND [CustomerNo] <> '-1' ORDER BY [Id] ASC";

                System.Data.OleDb.OleDbDataReader oleReader;
                System.Data.OleDb.OleDbCommand cmd = new System.Data.OleDb.OleDbCommand(sSql, dbConn);
                oleReader = cmd.ExecuteReader();

                while (oleReader.Read())
                {
                    string sCustomerNo = "n/a";
                    string sPeriod = "n/a";
                    string sCustomerName = "n/a";

                    string sNo = "";
                    string sDescription = "";
                    decimal dAmount = 0;
                    string sUnits = "";
                    decimal dUnitPrice = 0;
                    decimal dPrice = 0;

                    if ((!oleReader.IsDBNull(8)) && (!oleReader.IsDBNull(9)) && (!oleReader.IsDBNull(10)))
                    {
                        sDescription = "";
                        if (!oleReader.IsDBNull(2)) sDescription = oleReader.GetString(2);

                        sPeriod = oleReader.GetString(8);
                        sCustomerNo = oleReader.GetString(9);
                        sCustomerName = oleReader.GetString(10);

                        if (sCustNo != sCustomerNo)
                        {
                            if (sCustNo != "n/a")
                            {
                                // extra empty line
                                Sales_Invoice_Line extraemptyLine = new Sales_Invoice_Line();

                                extraemptyLine.Type = NAVOrdersWS.Type.Item;
                                extraemptyLine.No = "";

                                // quantity and price
                                extraemptyLine.Quantity = 0;
                                extraemptyLine.Unit_Price = 0;

                                extraemptyLine.Total_Amount_Incl_VATSpecified = false;
                                extraemptyLine.Total_Amount_Excl_VATSpecified = false;
                                extraemptyLine.Total_VAT_AmountSpecified = false;

                                // extra line
                                extraemptyLine.Description = " ";

                                // add extra line
                                InvoiceLinesList.Add(extraemptyLine);

                                // count added lines
                                iInvoiceLinesCount++;

                                // finish order
                                order.SalesLines = new Sales_Invoice_Line[iInvoiceLinesCount];
                                for (int i = 0; i < iInvoiceLinesCount; i++)
                                {
                                    order.SalesLines[i] = new Sales_Invoice_Line();
                                }
                                sal.Update(ref order);

                                int iOrderLinesCount = 0;
                                foreach (Sales_Invoice_Line sil in InvoiceLinesList)
                                {
                                    order.SalesLines[iOrderLinesCount].Type = sil.Type;
                                    order.SalesLines[iOrderLinesCount].No = sil.No;
                                    order.SalesLines[iOrderLinesCount].Quantity = sil.Quantity;
                                    order.SalesLines[iOrderLinesCount].Unit_Price = sil.Unit_Price;
                                    order.SalesLines[iOrderLinesCount].Unit_of_Measure = sil.Unit_of_Measure;
                                    order.SalesLines[iOrderLinesCount].Total_Amount_Incl_VATSpecified = sil.Total_Amount_Incl_VATSpecified;
                                    order.SalesLines[iOrderLinesCount].Total_Amount_Excl_VATSpecified = sil.Total_Amount_Excl_VATSpecified;
                                    order.SalesLines[iOrderLinesCount].Total_VAT_AmountSpecified = sil.Total_VAT_AmountSpecified;
                                    order.SalesLines[iOrderLinesCount].Description = sil.Description;
                                    iOrderLinesCount++;
                                }
                                sal.Update(ref order);
                            }

                            // new customer starts now
                            sCustNo = sCustomerNo;

                            // create order first
                            order = new SalesInvoice_Service();
                            InvoiceLinesList = new List<Sales_Invoice_Line>();
                            iInvoiceLinesCount = 0;

                            sal.Create(ref order);

                            order.Sell_to_Customer_No = sCustomerNo;
                            order.Posting_Date = DateTime.Now;
                            sal.Update(ref order);
                        }

                        // month first line
                        if (sDescription == sPeriod)
                        {
                            Sales_Invoice_Line extraLine = new Sales_Invoice_Line();

                            extraLine.Type = NAVOrdersWS.Type.Item;
                            extraLine.No = "";

                            // quantity and price
                            extraLine.Quantity = 0;
                            extraLine.Unit_Price = 0;

                            extraLine.Total_Amount_Incl_VATSpecified = false;
                            extraLine.Total_Amount_Excl_VATSpecified = false;
                            extraLine.Total_VAT_AmountSpecified = false;

                            // extra line
                            extraLine.Description = sDescription;

                            // add extra line
                            InvoiceLinesList.Add(extraLine);

                            // count added lines
                            iInvoiceLinesCount++;
                        }
                        else
                        {
                            // create invoice line
                            Sales_Invoice_Line invoiceLine = new Sales_Invoice_Line();

                            // item
                            invoiceLine.Type = NAVOrdersWS.Type.Item;

                            sNo = "";
                            if (!oleReader.IsDBNull(1)) sNo = oleReader.GetString(1);

                            // product no
                            invoiceLine.No = sNo;

                            dAmount = 0;
                            if (!oleReader.IsDBNull(3)) dAmount = oleReader.GetDecimal(3);

                            // quantity
                            invoiceLine.Quantity = dAmount;

                            sUnits = "";
                            if (!oleReader.IsDBNull(4)) sUnits = oleReader.GetString(4);

                            // units of measure code
                            if (sUnits == "Bruger/md (user/month)") sUnits = "USER/MD";
                            if (sUnits == "") sUnits = "STK";
                            invoiceLine.Unit_of_Measure = sUnits;

                            dUnitPrice = 0;
                            if (!oleReader.IsDBNull(5)) dUnitPrice = oleReader.GetDecimal(5);

                            // unit price
                            invoiceLine.Unit_Price = dUnitPrice;

                            dPrice = 0;
                            if (!oleReader.IsDBNull(6)) dPrice = oleReader.GetDecimal(6);

                            // no vat values
                            invoiceLine.Total_Amount_Incl_VATSpecified = false;
                            invoiceLine.Total_Amount_Excl_VATSpecified = false;
                            invoiceLine.Total_VAT_AmountSpecified = false;

                            // description
                            string sLineDescription = sDescription;
                            if (sLineDescription.Length <= 50)
                            {
                                invoiceLine.Description = sLineDescription;

                                // add invoice line
                                InvoiceLinesList.Add(invoiceLine);

                                // count added lines
                                iInvoiceLinesCount++;
                            }
                            else
                            {
                                // remove multiple spaces & odd empty chars
                                RegexOptions options = RegexOptions.None;
                                Regex regex = new Regex(@"[ ]{2,}", options);
                                sLineDescription = regex.Replace(sLineDescription, @" ");
                                sLineDescription = Regex.Replace(sLineDescription, @"\p{Z}", " ");

                                // create as many new lines as needed to fit comment length
                                int partLength = 50;

                                string sLineDescriptionFriendlyChars2 = sLineDescription.Replace(" ", "≡");
                                string[] sLineDescriptionWords2 = sLineDescriptionFriendlyChars2.Split('≡');

                                // check if there are words nigger than 50 chars
                                string sLineDescriptionFriendlyChars = "";
                                foreach (var sLineDescriptionWord in sLineDescriptionWords2)
                                {
                                    if (sLineDescriptionWord.Length < partLength)
                                    {
                                        sLineDescriptionFriendlyChars += sLineDescriptionWord + "≡";
                                    }
                                    else
                                    {
                                        sLineDescriptionFriendlyChars += sLineDescriptionWord.Substring(0, partLength) + "≡";
                                        string sTmp = sLineDescriptionWord.Substring(partLength);
                                        if (sTmp.Length < partLength)
                                        {
                                            sLineDescriptionFriendlyChars += sTmp + "≡";
                                        }
                                        else
                                        {
                                            sLineDescriptionFriendlyChars += sTmp.Substring(0, partLength) + "≡";
                                            sTmp = sTmp.Substring(partLength);
                                            sLineDescriptionFriendlyChars += sTmp.Substring(partLength) + "≡";
                                        }
                                    }
                                }
                                string[] sLineDescriptionWords = sLineDescriptionFriendlyChars.Split('≡');

                                var parts = new Dictionary<int, string>();
                                string part = string.Empty;
                                int partCounter = 0;
                                foreach (var sLineDescriptionWord in sLineDescriptionWords)
                                {
                                    if (part.Length + sLineDescriptionWord.Length < partLength)
                                    {
                                        part += string.IsNullOrEmpty(part) ? sLineDescriptionWord : " " + sLineDescriptionWord;
                                    }
                                    else
                                    {
                                        parts.Add(partCounter, part);
                                        part = sLineDescriptionWord;
                                        partCounter++;
                                    }
                                }
                                parts.Add(partCounter, part);

                                int iPartsCount = 0;
                                foreach (var item in parts)
                                {
                                    if (iPartsCount == 0)
                                    {
                                        // include first 50 chars in the current line
                                        invoiceLine.Description = item.Value;

                                        // add invoice line
                                        InvoiceLinesList.Add(invoiceLine);

                                        // count added lines
                                        iInvoiceLinesCount++;
                                    }
                                    else
                                    {
                                        Sales_Invoice_Line extraLine = new Sales_Invoice_Line();

                                        extraLine.Type = NAVOrdersWS.Type.Item;
                                        extraLine.No = "";

                                        // quantity and price
                                        extraLine.Quantity = 0;
                                        extraLine.Unit_Price = 0;

                                        extraLine.Total_Amount_Incl_VATSpecified = false;
                                        extraLine.Total_Amount_Excl_VATSpecified = false;
                                        extraLine.Total_VAT_AmountSpecified = false;

                                        // extra line
                                        extraLine.Description = item.Value;

                                        // add extra line
                                        InvoiceLinesList.Add(extraLine);

                                        // count added lines
                                        iInvoiceLinesCount++;
                                    }
                                    iPartsCount++;
                                }
                            }
                        }
                    }
                }

                // extra empty line
                Sales_Invoice_Line extraemptyLineLast = new Sales_Invoice_Line();

                extraemptyLineLast.Type = NAVOrdersWS.Type.Item;
                extraemptyLineLast.No = "";

                // quantity and price
                extraemptyLineLast.Quantity = 0;
                extraemptyLineLast.Unit_Price = 0;

                extraemptyLineLast.Total_Amount_Incl_VATSpecified = false;
                extraemptyLineLast.Total_Amount_Excl_VATSpecified = false;
                extraemptyLineLast.Total_VAT_AmountSpecified = false;

                // extra line
                extraemptyLineLast.Description = " ";

                // add extra line
                InvoiceLinesList.Add(extraemptyLineLast);

                // count added lines
                iInvoiceLinesCount++;

                // finish order for last customer
                order.SalesLines = new Sales_Invoice_Line[iInvoiceLinesCount];
                for (int i = 0; i < iInvoiceLinesCount; i++)
                {
                    order.SalesLines[i] = new Sales_Invoice_Line();
                }
                sal.Update(ref order);

                int iOrderLinesCountLast = 0;
                foreach (Sales_Invoice_Line sil in InvoiceLinesList)
                {
                    order.SalesLines[iOrderLinesCountLast].Type = sil.Type;
                    order.SalesLines[iOrderLinesCountLast].No = sil.No;
                    order.SalesLines[iOrderLinesCountLast].Quantity = sil.Quantity;
                    order.SalesLines[iOrderLinesCountLast].Unit_Price = sil.Unit_Price;
                    order.SalesLines[iOrderLinesCountLast].Unit_of_Measure = sil.Unit_of_Measure;
                    order.SalesLines[iOrderLinesCountLast].Total_Amount_Incl_VATSpecified = sil.Total_Amount_Incl_VATSpecified;
                    order.SalesLines[iOrderLinesCountLast].Total_Amount_Excl_VATSpecified = sil.Total_Amount_Excl_VATSpecified;
                    order.SalesLines[iOrderLinesCountLast].Total_VAT_AmountSpecified = sil.Total_VAT_AmountSpecified;
                    order.SalesLines[iOrderLinesCountLast].Description = sil.Description;
                    iOrderLinesCountLast++;
                }
                sal.Update(ref order);

                sResult = "All invoice(s) pushed to Navision.";

                dbConn.Close();
            }
            catch (Exception ex)
            {
                sResult += ex.ToString();
            }

            PushingDataL.Text = sResult;
        }

        private string InsertUpdateDatabase(string SQL, OleDbConnection dbConn)
        {
            Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");

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
                HitBillingDataL.Text += sResult + "<br />" + SQL + " <br />";
                return sResult;
            }

            return sResult;
        }
    }
}