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

using Microsoft.Identity.Client;
using Microsoft.Graph;
using Microsoft.Graph.Auth;

namespace RPNAVConnect
{
    public partial class HitBillingCustomersDF : System.Web.UI.Page
    {
        protected async void Page_Load(object sender, EventArgs e)
        {
            if (Page.IsPostBack == false)
            {
                string sCustomerId = "n/a";

                try
                {
                    sCustomerId = Request.QueryString["id"];
                }
                catch (Exception ex)
                {
                    sCustomerId = "n/a";
                    HitBillingDataL.Text = ex.ToString();
                }

                string sResult = await GetCustomerData(sCustomerId);
            }

            if (Page.IsPostBack == true)
            {
                InfoLabel.Text = "";

                System.Collections.Specialized.NameValueCollection FormPageVars;
                FormPageVars = Request.Form;

                var eventTarget = Request.Form["__EVENTTARGET"].ToString();

                // Check if some button is pressed
                if (eventTarget != null)
                {
                    if (eventTarget != "")
                    {
                        // Customer Main Data Update Button is pressed
                        if (eventTarget.IndexOf("Sav_New") == 0)
                        {
                            string sCustId = "New";

                            string dbPath = ConfigurationManager.AppSettings["dbpath"].ToString();
                            System.Data.OleDb.OleDbConnection dbConn = new System.Data.OleDb.OleDbConnection(dbPath);
                            dbConn.Open();

                            string sJSSet = "";

                            string sName = "";
                            string sADGRoup = "";
                            string sNavId = "";

                            foreach (string formitem in FormPageVars = Request.Form)
                            {
                                if (formitem.IndexOf("CustomerName_" + sCustId) == 0)
                                {
                                    sName = Request.Form[formitem].ToString();
                                    sJSSet += "changeCusEdtValue('" + Request.Form[formitem].ToString() + "', '" + formitem + "');\r\n";
                                }
                                if (formitem.IndexOf("AzureGroup_" + sCustId) == 0)
                                {
                                    sADGRoup = Request.Form[formitem].ToString();
                                    sJSSet += "changeCusEdtValue('" + Request.Form[formitem].ToString() + "', '" + formitem + "');\r\n";
                                }
                                if (formitem.IndexOf("NavisionId_" + sCustId) == 0)
                                {
                                    sNavId =  Request.Form[formitem].ToString();
                                    sJSSet += "changeCusEdtValue('" + Request.Form[formitem].ToString() + "', '" + formitem + "');\r\n";
                                }
                            }

                            if ((sName != "") && (sADGRoup != "") && (sNavId != ""))
                            {
                                string sSql = "SELECT TOP 1 [Id] FROM [RPNAVConnect].[dbo].[HITCustomers] WHERE [Name] = '" + sName + "'";
                                System.Data.OleDb.OleDbDataReader oleReader;
                                System.Data.OleDb.OleDbCommand cmd = new System.Data.OleDb.OleDbCommand(sSql, dbConn);
                                oleReader = cmd.ExecuteReader();
                                bool bExists = false;
                                if (oleReader.Read())
                                {
                                    if (!oleReader.IsDBNull(0))
                                    {
                                        bExists = true;
                                    }
                                }
                                oleReader.Close();

                                if (bExists == false)
                                {
                                    string sSqlIns = "INSERT INTO [dbo].[HITCustomers] ([Name], [ADGroup], [NavId]) ";
                                    sSqlIns += "VALUES ('" + sName.Replace("'", "''") + "', '" + sADGRoup.Replace("'", "''") + "', '" + sNavId.Replace("'", "''") + "')";

                                    string sDBResultIns = InsertUpdateDatabase(sSqlIns, dbConn);

                                    if (sDBResultIns == "DBOK")
                                    {
                                        // get customer id
                                        string sSqlId = "SELECT TOP 1 [Id] FROM [RPNAVConnect].[dbo].[HITCustomers] WHERE [Name] = '" + sName + "'";
                                        System.Data.OleDb.OleDbDataReader oleReader2;
                                        System.Data.OleDb.OleDbCommand cmd2 = new System.Data.OleDb.OleDbCommand(sSqlId, dbConn);
                                        oleReader2 = cmd2.ExecuteReader();
                                        if (oleReader2.Read())
                                        {
                                            if (!oleReader2.IsDBNull(0))
                                            {
                                                sCustId = oleReader2.GetInt32(0).ToString();
                                            }
                                        }
                                        oleReader2.Close();

                                        if (sCustId != "New")
                                        {
                                            string sSet = "";

                                            foreach (string formitem in FormPageVars = Request.Form)
                                            {
                                                if (formitem.IndexOf("ClientId_New") == 0)
                                                {
                                                    sSet += "[ClientId] = '" + Request.Form[formitem].ToString() + "', ";
                                                    sJSSet += "changeCusEdtValue('" + Request.Form[formitem].ToString() + "', '" + formitem + "');\r\n";
                                                }
                                                if (formitem.IndexOf("ClientSecret_New") == 0)
                                                {
                                                    sSet += "[ClientSecret] = '" + Request.Form[formitem].ToString() + "', ";
                                                    sJSSet += "changeCusEdtValue('" + Request.Form[formitem].ToString() + "', '" + formitem + "');\r\n";
                                                }
                                                if (formitem.IndexOf("TenantId_New") == 0)
                                                {
                                                    sSet += "[Tenant] = '" + Request.Form[formitem].ToString() + "', ";
                                                    sJSSet += "changeCusEdtValue('" + Request.Form[formitem].ToString() + "', '" + formitem + "');\r\n";
                                                }
                                                if (formitem.IndexOf("Lak_New") == 0)
                                                {
                                                    sSet += "[Lak] = '" + Request.Form[formitem].ToString() + "', ";
                                                    sJSSet += "changeCusEdtValue('" + Request.Form[formitem].ToString() + "', '" + formitem + "');\r\n";
                                                }
                                                if (formitem.IndexOf("Hrw_New") == 0)
                                                {
                                                    sSet += "[Hrw] = '" + Request.Form[formitem].ToString() + "', ";
                                                    sJSSet += "changeCusSelValue('" + Request.Form[formitem].ToString() + "', '" + formitem + "');\r\n";
                                                }
                                                if (formitem.IndexOf("Hmn_New") == 0)
                                                {
                                                    sSet += "[Hmn] = '" + Request.Form[formitem].ToString() + "', ";
                                                    sJSSet += "changeCusEdtValue('" + Request.Form[formitem].ToString() + "', '" + formitem + "');\r\n";
                                                }
                                                if (formitem.IndexOf("Sdm_New") == 0)
                                                {
                                                    sSet += "[Sdm] = '" + Request.Form[formitem].ToString() + "', ";
                                                    sJSSet += "changeCusSelValue('" + Request.Form[formitem].ToString() + "', '" + formitem + "');\r\n";
                                                }
                                                if (formitem.IndexOf("Srpm_New") == 0)
                                                {
                                                    sSet += "[Srpm] = '" + Request.Form[formitem].ToString() + "', ";
                                                    sJSSet += "changeCusSelValue('" + Request.Form[formitem].ToString() + "', '" + formitem + "');\r\n";
                                                }
                                                if (formitem.IndexOf("Srpmf_New") == 0)
                                                {
                                                    sSet += "[Srpmf] = '" + Request.Form[formitem].ToString() + "', ";
                                                    sJSSet += "changeCusSelValue('" + Request.Form[formitem].ToString() + "', '" + formitem + "');\r\n";
                                                }
                                                if (formitem.IndexOf("Hmfa_New") == 0)
                                                {
                                                    sSet += "[Hmfa] = '" + Request.Form[formitem].ToString() + "', ";
                                                    sJSSet += "changeCusSelValue('" + Request.Form[formitem].ToString() + "', '" + formitem + "');\r\n";
                                                }
                                                if (formitem.IndexOf("Hsikk_New") == 0)
                                                {
                                                    sSet += "[Hsikk] = '" + Request.Form[formitem].ToString() + "', ";
                                                    sJSSet += "changeCusSelValue('" + Request.Form[formitem].ToString() + "', '" + formitem + "');\r\n";
                                                }
                                                if (formitem.IndexOf("Scsm_New") == 0)
                                                {
                                                    sSet += "[Scsm] = '" + Request.Form[formitem].ToString() + "', ";
                                                    sJSSet += "changeCusSelValue('" + Request.Form[formitem].ToString() + "', '" + formitem + "');\r\n";
                                                }
                                                if (formitem.IndexOf("Hmfsi_New") == 0)
                                                {
                                                    sSet += "[Hmfsi] = '" + Request.Form[formitem].ToString() + "', ";
                                                    sJSSet += "changeCusSelValue('" + Request.Form[formitem].ToString() + "', '" + formitem + "');\r\n";
                                                }
                                                if (formitem.IndexOf("Swtene_New") == 0)
                                                {
                                                    sSet += "[Swtene] = '" + Request.Form[formitem].ToString() + "', ";
                                                    sJSSet += "changeCusSelValue('" + Request.Form[formitem].ToString() + "', '" + formitem + "');\r\n";
                                                }
                                                if (formitem.IndexOf("Mdm_New") == 0)
                                                {
                                                    sSet += "[Mdm] = '" + Request.Form[formitem].ToString() + "', ";
                                                    sJSSet += "changeCusSelValue('" + Request.Form[formitem].ToString() + "', '" + formitem + "');\r\n";
                                                }
                                            }

                                            sSet += "[ClientScopesApplication] = 'User.Read.All Group.Read.All GroupMember.Read.All', ";
                                            sSet += "[redirectUri] = 'https://nav.gowingu.net:8091/', ";

                                            if (sSet.IndexOf(", ") != -1)
                                            {
                                                int iLastItem = sSet.LastIndexOf(", ");
                                                sSet = sSet.Substring(0, iLastItem);
                                            }

                                            string sSqlUpd = "UPDATE [dbo].[HITCustomers] SET " + sSet + " WHERE [Id] = " + sCustId;
                                            string sDBResultUpd = InsertUpdateDatabase(sSqlUpd, dbConn);

                                            if (sDBResultUpd == "DBOK")
                                            {
                                                lastscriptdiv.InnerHtml = "<script>";
                                                lastscriptdiv.InnerHtml += "window.location.href = 'HitBillingDF.aspx';";
                                                lastscriptdiv.InnerHtml += "</script>";
                                            }
                                        }
                                        else
                                        {
                                            InfoLabel.Text = "Problem with database!<br /><br />";
                                        }
                                    }
                                    else
                                    {
                                        InfoLabel.Text = sDBResultIns + "<br /><br />"; ;
                                    }
                                }
                                else
                                {
                                    InfoLabel.Text = "Customer " + sName + " already exists!<br /><br />";
                                }
                            }
                            else
                            {
                                InfoLabel.Text = "Please define all fields!<br /><br />";
                            }
                            
                            dbConn.Close();
                        }


                        // Customer Main Data Update Button is pressed
                        if (eventTarget.IndexOf("Hed_") == 0)
                        {
                            string sCustId = eventTarget.Substring(eventTarget.IndexOf("Hed_") + 4);

                            string dbPath = ConfigurationManager.AppSettings["dbpath"].ToString();
                            System.Data.OleDb.OleDbConnection dbConn = new System.Data.OleDb.OleDbConnection(dbPath);
                            dbConn.Open();

                            lastscriptdiv.InnerHtml = "<script>";
                            string sSet = "SET ";
                            string sJSSet = "";

                            foreach (string formitem in FormPageVars = Request.Form)
                            {
                                if (formitem.IndexOf("CustomerName_" + sCustId) == 0)
                                {
                                    sSet += "[Name] = '" + Request.Form[formitem].ToString() + "', ";
                                    sJSSet += "changeCusEdtValue('" + Request.Form[formitem].ToString() + "', '" + formitem + "');\r\n";
                                }
                                if (formitem.IndexOf("AzureGroup_" + sCustId) == 0)
                                {
                                    sSet += "[ADGroup] = '" + Request.Form[formitem].ToString() + "', ";
                                    sJSSet += "changeCusEdtValue('" + Request.Form[formitem].ToString() + "', '" + formitem + "');\r\n";
                                }
                                if (formitem.IndexOf("NavisionId_" + sCustId) == 0)
                                {
                                    sSet += "[NavId] = '" + Request.Form[formitem].ToString() + "', ";
                                    sJSSet += "changeCusEdtValue('" + Request.Form[formitem].ToString() + "', '" + formitem + "');\r\n";
                                }
                            }

                            if (sSet.IndexOf(", ") != -1)
                            {
                                int iLastItem = sSet.LastIndexOf(", ");
                                sSet = sSet.Substring(0, iLastItem);
                            }

                            string sSqlUpd = "UPDATE [dbo].[HITCustomers] " + sSet + " WHERE [Id] = " + sCustId;
                            string sDBResultIns = InsertUpdateDatabase(sSqlUpd, dbConn);

                            if (sDBResultIns == "DBOK")
                            {
                                lastscriptdiv.InnerHtml += sJSSet;
                            }

                            lastscriptdiv.InnerHtml += "</script>";
                            dbConn.Close();
                        }

                        // Customer Azure Update Button is pressed
                        if (eventTarget.IndexOf("Azu_") == 0)
                        {
                            string sCustId = eventTarget.Substring(eventTarget.IndexOf("Azu_") + 4);

                            string dbPath = ConfigurationManager.AppSettings["dbpath"].ToString();
                            System.Data.OleDb.OleDbConnection dbConn = new System.Data.OleDb.OleDbConnection(dbPath);
                            dbConn.Open();

                            lastscriptdiv.InnerHtml = "<script>";
                            string sSet = "SET ";
                            string sJSSet = "";

                            foreach (string formitem in FormPageVars = Request.Form)
                            {
                                if (formitem.IndexOf("ClientId_" + sCustId) == 0)
                                {
                                    sSet += "[ClientId] = '" + Request.Form[formitem].ToString() + "', ";
                                    sJSSet += "changeCusEdtValue('" + Request.Form[formitem].ToString() + "', '" + formitem + "');\r\n";
                                }
                                if (formitem.IndexOf("ClientSecret_" + sCustId) == 0)
                                {
                                    sSet += "[ClientSecret] = '" + Request.Form[formitem].ToString() + "', ";
                                    sJSSet += "changeCusEdtValue('" + Request.Form[formitem].ToString() + "', '" + formitem + "');\r\n";
                                }
                                if (formitem.IndexOf("TenantId_" + sCustId) == 0)
                                {
                                    sSet += "[Tenant] = '" + Request.Form[formitem].ToString() + "', ";
                                    sJSSet += "changeCusEdtValue('" + Request.Form[formitem].ToString() + "', '" + formitem + "');\r\n";
                                }
                            }

                            if (sSet.IndexOf(", ") != -1)
                            {
                                int iLastItem = sSet.LastIndexOf(", ");
                                sSet = sSet.Substring(0, iLastItem);
                            }

                            string sSqlUpd = "UPDATE [dbo].[HITCustomers] " + sSet + " WHERE [Id] = " + sCustId;
                            string sDBResultIns = InsertUpdateDatabase(sSqlUpd, dbConn);

                            if (sDBResultIns == "DBOK")
                            {
                                lastscriptdiv.InnerHtml += sJSSet;
                            }

                            lastscriptdiv.InnerHtml += "</script>";
                            dbConn.Close();
                        }

                        // Ekstra tilføjelser Update Button is pressed
                        if (eventTarget.IndexOf("Eks_") == 0)
                        {
                            string sCustId = eventTarget.Substring(eventTarget.IndexOf("Eks_") + 4);

                            string dbPath = ConfigurationManager.AppSettings["dbpath"].ToString();
                            System.Data.OleDb.OleDbConnection dbConn = new System.Data.OleDb.OleDbConnection(dbPath);
                            dbConn.Open();

                            lastscriptdiv.InnerHtml = "<script>";
                            string sSet = "SET ";
                            string sJSSet = "";

                            foreach (string formitem in FormPageVars = Request.Form)
                            {
                                if (formitem.IndexOf("Prin_" + sCustId) == 0)
                                {
                                    sSet += "[Prin] = '" + Request.Form[formitem].ToString() + "', ";
                                    sJSSet += "changeCusEdtValue('" + Request.Form[formitem].ToString() + "', '" + formitem + "');\r\n";
                                }
                                if (formitem.IndexOf("Bckep_" + sCustId) == 0)
                                {
                                    sSet += "[Bckep] = '" + Request.Form[formitem].ToString() + "', ";
                                    sJSSet += "changeCusEdtValue('" + Request.Form[formitem].ToString() + "', '" + formitem + "');\r\n";
                                }
                                if (formitem.IndexOf("Onsit_" + sCustId) == 0)
                                {
                                    sSet += "[Onsit] = '" + Request.Form[formitem].ToString() + "', ";
                                    sJSSet += "changeCusEdtValue('" + Request.Form[formitem].ToString() + "', '" + formitem + "');\r\n";
                                }
                            }

                            if (sSet.IndexOf(", ") != -1)
                            {
                                int iLastItem = sSet.LastIndexOf(", ");
                                sSet = sSet.Substring(0, iLastItem);
                            }

                            string sSqlUpd = "UPDATE [dbo].[HITCustomers] " + sSet + " WHERE [Id] = " + sCustId;
                            string sDBResultIns = InsertUpdateDatabase(sSqlUpd, dbConn);

                            if (sDBResultIns == "DBOK")
                            {
                                lastscriptdiv.InnerHtml += sJSSet;
                            }

                            lastscriptdiv.InnerHtml += "</script>";
                            dbConn.Close();
                        }

                        // Netværksudstyr Update Button is pressed
                        if (eventTarget.IndexOf("Net_") == 0)
                        {
                            string sCustId = eventTarget.Substring(eventTarget.IndexOf("Net_") + 4);

                            string dbPath = ConfigurationManager.AppSettings["dbpath"].ToString();
                            System.Data.OleDb.OleDbConnection dbConn = new System.Data.OleDb.OleDbConnection(dbPath);
                            dbConn.Open();

                            lastscriptdiv.InnerHtml = "<script>";
                            string sSet = "SET ";
                            string sJSSet = "";

                            foreach (string formitem in FormPageVars = Request.Form)
                            {
                                if (formitem.IndexOf("Fir_" + sCustId) == 0)
                                {
                                    sSet += "[Fir] = '" + Request.Form[formitem].ToString() + "', ";
                                    sJSSet += "changeCusEdtValue('" + Request.Form[formitem].ToString() + "', '" + formitem + "');\r\n";
                                }
                                if (formitem.IndexOf("Wifi_" + sCustId) == 0)
                                {
                                    sSet += "[Wifi] = '" + Request.Form[formitem].ToString() + "', ";
                                    sJSSet += "changeCusEdtValue('" + Request.Form[formitem].ToString() + "', '" + formitem + "');\r\n";
                                }
                                if (formitem.IndexOf("Swi24_" + sCustId) == 0)
                                {
                                    sSet += "[Swi24] = '" + Request.Form[formitem].ToString() + "', ";
                                    sJSSet += "changeCusEdtValue('" + Request.Form[formitem].ToString() + "', '" + formitem + "');\r\n";
                                }
                                if (formitem.IndexOf("Swi8_" + sCustId) == 0)
                                {
                                    sSet += "[Swi8] = '" + Request.Form[formitem].ToString() + "', ";
                                    sJSSet += "changeCusEdtValue('" + Request.Form[formitem].ToString() + "', '" + formitem + "');\r\n";
                                }
                                if (formitem.IndexOf("Swi48_" + sCustId) == 0)
                                {
                                    sSet += "[Swi48] = '" + Request.Form[formitem].ToString() + "', ";
                                    sJSSet += "changeCusEdtValue('" + Request.Form[formitem].ToString() + "', '" + formitem + "');\r\n";
                                }
                                if (formitem.IndexOf("Eksi_" + sCustId) == 0)
                                {
                                    sSet += "[Eksi] = '" + Request.Form[formitem].ToString() + "', ";
                                    sJSSet += "changeCusEdtValue('" + Request.Form[formitem].ToString() + "', '" + formitem + "');\r\n";
                                }
                                if (formitem.IndexOf("Dns_" + sCustId) == 0)
                                {
                                    sSet += "[Dns] = '" + Request.Form[formitem].ToString() + "', ";
                                    sJSSet += "changeCusEdtValue('" + Request.Form[formitem].ToString() + "', '" + formitem + "');\r\n";
                                }
                            }

                            if (sSet.IndexOf(", ") != -1)
                            {
                                int iLastItem = sSet.LastIndexOf(", ");
                                sSet = sSet.Substring(0, iLastItem);
                            }

                            string sSqlUpd = "UPDATE [dbo].[HITCustomers] " + sSet + " WHERE [Id] = " + sCustId;
                            string sDBResultIns = InsertUpdateDatabase(sSqlUpd, dbConn);

                            if (sDBResultIns == "DBOK")
                            {
                                lastscriptdiv.InnerHtml += sJSSet;
                            }

                            lastscriptdiv.InnerHtml += "</script>";
                            dbConn.Close();
                        }

                        // Customer Update Button is pressed
                        if (eventTarget.IndexOf("Cus_") == 0)
                        {
                            string sCustId = eventTarget.Substring(eventTarget.IndexOf("Cus_") + 4);

                            string dbPath = ConfigurationManager.AppSettings["dbpath"].ToString();
                            System.Data.OleDb.OleDbConnection dbConn = new System.Data.OleDb.OleDbConnection(dbPath);
                            dbConn.Open();

                            lastscriptdiv.InnerHtml = "<script>";
                            string sSet = "SET ";
                            string sJSSet = "";

                            foreach (string formitem in FormPageVars = Request.Form)
                            {
                                if (formitem.IndexOf("Lak_" + sCustId) == 0)
                                {
                                    sSet += "[Lak] = '" + Request.Form[formitem].ToString() + "', ";
                                    sJSSet += "changeCusEdtValue('" + Request.Form[formitem].ToString() + "', '" + formitem + "');\r\n";
                                }
                                if (formitem.IndexOf("Hrw_" + sCustId) == 0)
                                {
                                    sSet += "[Hrw] = '" + Request.Form[formitem].ToString() + "', ";
                                    sJSSet += "changeCusSelValue('" + Request.Form[formitem].ToString() + "', '" + formitem + "');\r\n";
                                }
                                if (formitem.IndexOf("Hmn_" + sCustId) == 0)
                                {
                                    sSet += "[Hmn] = '" + Request.Form[formitem].ToString() + "', ";
                                    sJSSet += "changeCusEdtValue('" + Request.Form[formitem].ToString() + "', '" + formitem + "');\r\n";
                                }
                                if (formitem.IndexOf("Sdm_" + sCustId) == 0)
                                {
                                    sSet += "[Sdm] = '" + Request.Form[formitem].ToString() + "', ";
                                    sJSSet += "changeCusSelValue('" + Request.Form[formitem].ToString() + "', '" + formitem + "');\r\n";
                                }
                                if (formitem.IndexOf("Srpm_" + sCustId) == 0)
                                {
                                    sSet += "[Srpm] = '" + Request.Form[formitem].ToString() + "', ";
                                    sJSSet += "changeCusSelValue('" + Request.Form[formitem].ToString() + "', '" + formitem + "');\r\n";
                                }
                                if (formitem.IndexOf("Srpmf_" + sCustId) == 0)
                                {
                                    sSet += "[Srpmf] = '" + Request.Form[formitem].ToString() + "', ";
                                    sJSSet += "changeCusSelValue('" + Request.Form[formitem].ToString() + "', '" + formitem + "');\r\n";
                                }
                                if (formitem.IndexOf("Hmfa_" + sCustId) == 0)
                                {
                                    sSet += "[Hmfa] = '" + Request.Form[formitem].ToString() + "', ";
                                    sJSSet += "changeCusSelValue('" + Request.Form[formitem].ToString() + "', '" + formitem + "');\r\n";
                                }
                                if (formitem.IndexOf("Hsikk_" + sCustId) == 0)
                                {
                                    sSet += "[Hsikk] = '" + Request.Form[formitem].ToString() + "', ";
                                    sJSSet += "changeCusSelValue('" + Request.Form[formitem].ToString() + "', '" + formitem + "');\r\n";
                                }
                                if (formitem.IndexOf("Scsm_" + sCustId) == 0)
                                {
                                    sSet += "[Scsm] = '" + Request.Form[formitem].ToString() + "', ";
                                    sJSSet += "changeCusSelValue('" + Request.Form[formitem].ToString() + "', '" + formitem + "');\r\n";
                                }
                                if (formitem.IndexOf("Hmfsi_" + sCustId) == 0)
                                {
                                    sSet += "[Hmfsi] = '" + Request.Form[formitem].ToString() + "', ";
                                    sJSSet += "changeCusSelValue('" + Request.Form[formitem].ToString() + "', '" + formitem + "');\r\n";
                                }
                                if (formitem.IndexOf("Swtene_" + sCustId) == 0)
                                {
                                    sSet += "[Swtene] = '" + Request.Form[formitem].ToString() + "', ";
                                    sJSSet += "changeCusSelValue('" + Request.Form[formitem].ToString() + "', '" + formitem + "');\r\n";
                                }
                                if (formitem.IndexOf("Mdm_" + sCustId) == 0)
                                {
                                    sSet += "[Mdm] = '" + Request.Form[formitem].ToString() + "', ";
                                    sJSSet += "changeCusSelValue('" + Request.Form[formitem].ToString() + "', '" + formitem + "');\r\n";
                                }
                            }

                            if (sSet.IndexOf(", ") != -1)
                            {
                                int iLastItem = sSet.LastIndexOf(", ");
                                sSet = sSet.Substring(0, iLastItem);
                            }

                            string sSqlUpd = "UPDATE [dbo].[HITCustomers] " + sSet + " WHERE [Id] = " + sCustId;
                            string sDBResultIns = InsertUpdateDatabase(sSqlUpd, dbConn);

                            if (sDBResultIns == "DBOK")
                            {
                                lastscriptdiv.InnerHtml += sJSSet;
                            }

                            lastscriptdiv.InnerHtml += "</script>";
                            dbConn.Close();
                        }

                        // User Update Button is pressed
                        if (eventTarget.IndexOf("Usr_") == 0)
                        {
                            string sCustId = eventTarget.Substring(eventTarget.IndexOf("Usr_") + 4);

                            string dbPath = ConfigurationManager.AppSettings["dbpath"].ToString();
                            System.Data.OleDb.OleDbConnection dbConn = new System.Data.OleDb.OleDbConnection(dbPath);
                            dbConn.Open();

                            lastscriptdiv.InnerHtml = "<script>";

                            foreach (string formitem in FormPageVars = Request.Form)
                            {
                                if (formitem.IndexOf("type_" + sCustId + "_") == 0)
                                {
                                    string sUsrAzureId = formitem.Substring(("type_" + sCustId + "_").Length);
                                    string sUsrType = Request.Form[formitem];

                                    string sSqlUpd = "UPDATE [dbo].[HITUsers] SET [Type] = '" + sUsrType + "' WHERE [AzureId] = '" + sUsrAzureId + "'";
                                    string sDBResultIns = InsertUpdateDatabase(sSqlUpd, dbConn);

                                    if (sDBResultIns == "DBOK")
                                    {
                                        lastscriptdiv.InnerHtml += "changeUsrType('" + sUsrType + "', '" + formitem + "');\r\n";
                                    }
                                }
                            }

                            lastscriptdiv.InnerHtml += "</script>";
                            dbConn.Close();
                        }
                    }
                }
            }

        }

        private async Task<string> GetCustomerData(string sCustomerId)
        {
            string sResult = "Ok";

            try
            {
                string dbPath = ConfigurationManager.AppSettings["dbpath"].ToString();
                System.Data.OleDb.OleDbConnection dbConn = new System.Data.OleDb.OleDbConnection(dbPath);
                dbConn.Open();

                HitBillingDataL.Text = "";
                string sSql = "SELECT TOP 1 * FROM [RPNAVConnect].[dbo].[HITCustomers] WHERE [Id] = " + sCustomerId;

                System.Data.OleDb.OleDbDataReader oleReader;
                System.Data.OleDb.OleDbCommand cmd = new System.Data.OleDb.OleDbCommand(sSql, dbConn);
                oleReader = cmd.ExecuteReader();

                HitBillingDataL.Text += "<table class=\"table table-bordered table-striped\" style=\"width:700px;\">";

                HitBillingDataL.Text += "<tr class=\"bg-danger text-white\">";
                HitBillingDataL.Text += "<th width=\"200px\">Name</th>";
                HitBillingDataL.Text += "<th width=\"200px\">Azure Group</th>";
                HitBillingDataL.Text += "<th>Navision Id</th>";
                HitBillingDataL.Text += "<th>Users #</th>";
                HitBillingDataL.Text += "</tr>";

                if ((oleReader.Read()) || (sCustomerId == "-1"))
                {
                    string sId = "New";
                    string sName = "";
                    string sADGroup = "";
                    string sNAVId = "";

                    string sClientId = "";
                    string sClientSecret = "";
                    string sClientScopesApplication = "";
                    string sTenant = "";
                    string sRedirectUri = "";

                    string sHrw = "";
                    string sHmn = "";
                    string sSdm = "";
                    string sSrpm = "";
                    string sSrpmf = "";
                    string sHmfa = "";
                    string sHsikk = "";
                    string sScsm = "";
                    string sHmfsi = "";
                    string sSwtene = "";
                    string sMdm = "";
                    string sLak = "";

                    string sPrin = "";
                    string sBckep = "";
                    string sOnsit = "";
                    string sFir = "";
                    string sWifi = "";
                    string sSwi24 = "";
                    string sSwi8 = "";
                    string sSwi48 = "";
                    string sEksi = "";
                    string sDns = "";

                    if (sCustomerId != "-1")
                    {
                        sId = oleReader.GetInt32(0).ToString();
                        sName = oleReader.GetString(1);
                        sADGroup = oleReader.GetString(2);
                        sNAVId = oleReader.GetString(3);

                        sClientId = oleReader.GetString(4);
                        sClientSecret = oleReader.GetString(5);
                        sClientScopesApplication = oleReader.GetString(6);
                        sTenant = oleReader.GetString(7);
                        sRedirectUri = oleReader.GetString(8);

                        if (!oleReader.IsDBNull(9)) sHrw = oleReader.GetString(9);
                        if (!oleReader.IsDBNull(10)) sHmn = oleReader.GetString(10);
                        if (!oleReader.IsDBNull(11)) sSdm = oleReader.GetString(11);
                        if (!oleReader.IsDBNull(12)) sSrpm = oleReader.GetString(12);
                        if (!oleReader.IsDBNull(13)) sSrpmf = oleReader.GetString(13);
                        if (!oleReader.IsDBNull(14)) sHmfa = oleReader.GetString(14);
                        if (!oleReader.IsDBNull(15)) sHsikk = oleReader.GetString(15);
                        if (!oleReader.IsDBNull(16)) sScsm = oleReader.GetString(16);
                        if (!oleReader.IsDBNull(17)) sHmfsi = oleReader.GetString(17);
                        if (!oleReader.IsDBNull(18)) sSwtene = oleReader.GetString(18);
                        if (!oleReader.IsDBNull(19)) sMdm = oleReader.GetString(19);
                        if (!oleReader.IsDBNull(20)) sLak = oleReader.GetString(20);

                        if (!oleReader.IsDBNull(21)) sPrin = oleReader.GetString(21);
                        if (!oleReader.IsDBNull(22)) sBckep = oleReader.GetString(22);
                        if (!oleReader.IsDBNull(23)) sOnsit = oleReader.GetString(23);
                        if (!oleReader.IsDBNull(24)) sFir = oleReader.GetString(24);
                        if (!oleReader.IsDBNull(25)) sWifi = oleReader.GetString(25);
                        if (!oleReader.IsDBNull(26)) sSwi24 = oleReader.GetString(26);
                        if (!oleReader.IsDBNull(27)) sSwi8 = oleReader.GetString(27);
                        if (!oleReader.IsDBNull(28)) sSwi48 = oleReader.GetString(28);
                        if (!oleReader.IsDBNull(29)) sEksi = oleReader.GetString(29);
                        if (!oleReader.IsDBNull(30)) sDns = oleReader.GetString(30);

                    }

                    // customer
                    if (sCustomerId == "-1")
                    {
                        HitBillingDataL.Text += "<tr>";
                        HitBillingDataL.Text += "<td width=\"200px\"><input style=\"height:35px;\" type='text' id='CustomerName_New' name='CustomerName_New' value='' /></td>";
                        HitBillingDataL.Text += "<td width=\"200px\"><input style=\"height:35px;\" type='text' id='AzureGroup_New' name='AzureGroup_New' value='' /></td>";
                        HitBillingDataL.Text += "<td><input style=\"height:35px;\" type='text' id='NavisionId_New' name='NavisionId_New' value='' /></td>";
                        HitBillingDataL.Text += "<td></td>";
                        HitBillingDataL.Text += "</tr>";
                    }
                    else
                    {
                        CustomerNameTitle.Text = sName;
                        HitBillingDataL.Text += "<tr>";
                        HitBillingDataL.Text += "<td width=\"200px\"><input style=\"height:35px;\" type='text' id='CustomerName_" + sId + "' name='CustomerName_" + sId + "' value='" + sName + "' /></td>";
                        HitBillingDataL.Text += "<td width=\"200px\"><input style=\"height:35px;\" type='text' id='AzureGroup_" + sId + "' name='AzureGroup_" + sId + "' value='" + sADGroup + "' /></td>";
                        HitBillingDataL.Text += "<td><input style=\"height:35px;\" type='text' id='NavisionId_" + sId + "' name='NavisionId_" + sId + "' value='" + sNAVId + "' /></td>";
                        HitBillingDataL.Text += "<td style=\"vertical-align: middle;\" align='center'>##USRNUM_" + sId + "##</td>";
                        HitBillingDataL.Text += "</tr>";
                    }

                    if (sCustomerId != "-1")
                    {
                        HitBillingDataL.Text += "<tr>";
                        HitBillingDataL.Text += "<td colspan=\"2\" width=\"400px\">&nbsp;";
                        HitBillingDataL.Text += "</td>";
                        HitBillingDataL.Text += "<td colspan=\"2\">";
                        HitBillingDataL.Text += "<input style=\"margin-top:8px; height:35px;\" type='button' id='Hed_" + sId + "' name='Hed_" + sId + "' value='Update Customer Main Data' onclick=\"__doPostBack('Hed_" + sId + "','')\" />";
                        HitBillingDataL.Text += "</td>";
                        HitBillingDataL.Text += "</tr>";
                    }

                    HitBillingDataL.Text += "</table>";
                    HitBillingDataL.Text += "</br>";
                    HitBillingDataL.Text += "<table class=\"table table-bordered table-striped\" style=\"width:700px;\">";

                    // customer azure
                    HitBillingDataL.Text += "<tr>";
                    HitBillingDataL.Text += "<td colspan=\"2\"><font size=\"3\">Azure App Data</font></td>";
                    HitBillingDataL.Text += "</tr>";

                    HitBillingDataL.Text += "<tr>";
                    HitBillingDataL.Text += "<td width=\"400px\">Client Id</td>";
                    HitBillingDataL.Text += "<td><input style=\"width:270px;height:35px;\" type='text' id='ClientId_" + sId + "' name='ClientId_" + sId + "' value='" + sClientId + "' /></td>";
                    HitBillingDataL.Text += "</tr>";

                    HitBillingDataL.Text += "<tr>";
                    HitBillingDataL.Text += "<td width=\"400px\">Client Secret</td>";
                    HitBillingDataL.Text += "<td><input style=\"width:270px;height:35px;\" type='text' id='ClientSecret_" + sId + "' name='ClientSecret_" + sId + "' value='" + sClientSecret + "' /></td>";
                    HitBillingDataL.Text += "</tr>";

                    HitBillingDataL.Text += "<tr>";
                    HitBillingDataL.Text += "<td width=\"400px\">Tenant Id</td>";
                    HitBillingDataL.Text += "<td><input style=\"width:270px;height:35px;\" type='text' id='TenantId_" + sId + "' name='TenantId_" + sId + "' value='" + sTenant + "' /></td>";
                    HitBillingDataL.Text += "</tr>";

                    if (sCustomerId != "-1")
                    {
                        HitBillingDataL.Text += "<tr>";
                        HitBillingDataL.Text += "<td width=\"400px\">&nbsp;";
                        HitBillingDataL.Text += "</td>";
                        HitBillingDataL.Text += "<td>";
                        HitBillingDataL.Text += "<input style=\"margin-top:8px; height:35px;\" type='button' id='Azu_" + sId + "' name='Azu_" + sId + "' value='Update Azure App Data' onclick=\"__doPostBack('Azu_" + sId + "','')\" />";
                        HitBillingDataL.Text += "</td>";
                        HitBillingDataL.Text += "</tr>";
                    }

                    HitBillingDataL.Text += "</table>";
                    HitBillingDataL.Text += "</br>";
                    HitBillingDataL.Text += "<table class=\"table table-bordered table-striped\" style=\"width:700px;\">";

                    // HIT indstillinger
                    HitBillingDataL.Text += "<tr>";
                    HitBillingDataL.Text += "<td colspan=\"2\"><font size=\"3\">HIT indstillinger</font></td>";
                    HitBillingDataL.Text += "</tr>";

                    HitBillingDataL.Text += "<tr>";
                    HitBillingDataL.Text += "<td width=\"400px\">Længde af kontrakt</td>";
                    HitBillingDataL.Text += "<td><input style=\"width:50px;height:35px;\" type='text' id='Lak_" + sId + "' name='Lak_" + sId + "' value='" + sLak + "' /></td>";
                    HitBillingDataL.Text += "</tr>";

                    string sRackpeopleSel = "";
                    if (sHrw == "Rackpeople") sRackpeopleSel = "selected";
                    string sKundenSel = "";
                    if (sHrw == "Kunden") sKundenSel = "selected";

                    HitBillingDataL.Text += "<tr>";
                    HitBillingDataL.Text += "<td width=\"400px\">Hvem reinstallerer Windows (på kundens eksisterende PC'er)</td>";
                    HitBillingDataL.Text += "<td><select style=\"height:35px;\" id=\"Hrw_" + sId + "\" name=\"Hrw_" + sId + "\" >";
                    HitBillingDataL.Text += "<option " + sRackpeopleSel + " value=\"Rackpeople\" >Rackpeople</option> ";
                    HitBillingDataL.Text += "<option " + sKundenSel + " value=\"Kunden\" >Kunden</option> ";
                    HitBillingDataL.Text += "</select></td>";
                    HitBillingDataL.Text += "</tr>";

                    HitBillingDataL.Text += "<tr>";
                    HitBillingDataL.Text += "<td width=\"400px\">Hvor mange nye PC'er købes efter anbefaling af RP</td>";
                    HitBillingDataL.Text += "<td><input style=\"width:50px;height:35px;\" type='text' id='Hmn_" + sId + "' name='Hmn_" + sId + "' value='" + sHmn + "' /></td>";
                    HitBillingDataL.Text += "</tr>";

                    string sSdmJaSel = "";
                    if (sSdm == "Ja") sSdmJaSel = "selected";
                    string sSdmNejSel = "";
                    if (sSdm == "Nej") sSdmNejSel = "selected";

                    HitBillingDataL.Text += "<tr>";
                    HitBillingDataL.Text += "<td width=\"400px\">Skal der migreres til en frisk Office 365 tenant?</td>";
                    HitBillingDataL.Text += "<td><select style=\"height:35px;\" id=\"Sdm_" + sId + "\" name=\"Sdm_" + sId + "\" >";
                    HitBillingDataL.Text += "<option " + sSdmJaSel + " value=\"Ja\" >Ja</option> ";
                    HitBillingDataL.Text += "<option " + sSdmNejSel + " value=\"Nej\" >Nej</option> ";
                    HitBillingDataL.Text += "</select></td>";
                    HitBillingDataL.Text += "</tr>";

                    string sSrpmJaSel = "";
                    if (sSrpm == "Ja") sSrpmJaSel = "selected";
                    string sSrpmNejSel = "";
                    if (sSrpm == "Nej") sSrpmNejSel = "selected";

                    HitBillingDataL.Text += "<tr>";
                    HitBillingDataL.Text += "<td width=\"400px\">Skal RP migrere data for brugerne (Exchange/Onedrive)?</td>";
                    HitBillingDataL.Text += "<td><select style=\"height:35px;\" id=\"Srpm_" + sId + "\" name=\"Srpm_" + sId + "\" >";
                    HitBillingDataL.Text += "<option " + sSrpmJaSel + " value=\"Ja\" >Ja</option> ";
                    HitBillingDataL.Text += "<option " + sSrpmNejSel + " value=\"Nej\" >Nej</option> ";
                    HitBillingDataL.Text += "</select></td>";
                    HitBillingDataL.Text += "</tr>";

                    string sSrpmfJaSel = "";
                    if (sSrpmf == "Ja") sSrpmfJaSel = "selected";
                    string sSrpmfNejSel = "";
                    if (sSrpmf == "Nej") sSrpmNejSel = "selected";

                    HitBillingDataL.Text += "<tr>";
                    HitBillingDataL.Text += "<td width=\"400px\">Skal RP migrere firma data (Sharepoint filer)?</td>";
                    HitBillingDataL.Text += "<td><select style=\"height:35px;\" id=\"Srpmf_" + sId + "\" name=\"Srpmf_" + sId + "\" >";
                    HitBillingDataL.Text += "<option " + sSrpmfJaSel + " value=\"Ja\" >Ja</option> ";
                    HitBillingDataL.Text += "<option " + sSrpmfNejSel + " value=\"Nej\" >Nej</option> ";
                    HitBillingDataL.Text += "</select></td>";
                    HitBillingDataL.Text += "</tr>";

                    string sHmfaIngenSel = "";
                    if (sHmfa == "Ingen") sHmfaIngenSel = "selected";
                    string sHmfaStandardSel = "";
                    if (sHmfa == "Standard") sHmfaStandardSel = "selected";
                    string sHmfaLocationbasedSel = "";
                    if (sHmfa == "Locationbased") sHmfaLocationbasedSel = "selected";
                    string sHmfaOnPremSel = "";
                    if (sHmfa == "OnPrem") sHmfaOnPremSel = "selected";

                    HitBillingDataL.Text += "<tr>";
                    HitBillingDataL.Text += "<td width=\"400px\">Hvilken MFA skal implementeres</td>";
                    HitBillingDataL.Text += "<td><select style=\"height:35px;\" id=\"Hmfa_" + sId + "\" name=\"Hmfa_" + sId + "\" >";
                    HitBillingDataL.Text += "<option " + sHmfaIngenSel + " value=\"Ingen\" >Ingen</option> ";
                    HitBillingDataL.Text += "<option " + sHmfaStandardSel + " value=\"Standard\" >Standard</option> ";
                    HitBillingDataL.Text += "<option " + sHmfaLocationbasedSel + " value=\"Locationbased\" >Locationbased</option> ";
                    HitBillingDataL.Text += "<option " + sHmfaOnPremSel + " value=\"OnPrem\" >OnPrem</option> ";
                    HitBillingDataL.Text += "</select></td>";
                    HitBillingDataL.Text += "</tr>";

                    string sHsikkStandardSel = "";
                    if (sHsikk == "Standard") sHsikkStandardSel = "selected";
                    string sHsikkATPSel = "";
                    if (sHsikk == "ATP") sHsikkATPSel = "selected";

                    HitBillingDataL.Text += "<tr>";
                    HitBillingDataL.Text += "<td width=\"400px\">Hvilken sikkerhedspakke (standard Office 365/ATP - safelinks og vedhæftning scanning, inkluderet I business)</td>";
                    HitBillingDataL.Text += "<td><select style=\"height:35px;\" id=\"Hsikk_" + sId + "\" name=\"Hsikk_" + sId + "\" >";
                    HitBillingDataL.Text += "<option " + sHsikkStandardSel + " value=\"Standard\" >Standard</option> ";
                    HitBillingDataL.Text += "<option " + sHsikkATPSel + " value=\"ATP\" >ATP</option> ";
                    HitBillingDataL.Text += "</select></td>";
                    HitBillingDataL.Text += "</tr>";

                    string sScsmJaSel = "";
                    if (sScsm == "Ja") sScsmJaSel = "selected";
                    string sScsmNejSel = "";
                    if (sScsm == "Nej") sScsmNejSel = "selected";

                    HitBillingDataL.Text += "<tr>";
                    HitBillingDataL.Text += "<td width=\"400px\">Standard central styret mailsignatur</td>";
                    HitBillingDataL.Text += "<td><select style=\"height:35px;\" id=\"Scsm_" + sId + "\" name=\"Scsm_" + sId + "\" >";
                    HitBillingDataL.Text += "<option " + sScsmJaSel + " value=\"Ja\" >Ja</option> ";
                    HitBillingDataL.Text += "<option " + sScsmNejSel + " value=\"Nej\" >Nej</option> ";
                    HitBillingDataL.Text += "</select></td>";
                    HitBillingDataL.Text += "</tr>";

                    string sHmfsiIngenSel = "";
                    if (sHmfsi == "Ingen") sHmfsiIngenSel = "selected";
                    string sHmfsiMailSel = "";
                    if (sHmfsi == "Mail") sHmfsiMailSel = "selected";
                    string sHmfsiMOSFSel = "";
                    if (sHmfsi == "Mail, Onedrive og Sharepoint filer") sHmfsiMOSFSel = "selected";

                    HitBillingDataL.Text += "<tr>";
                    HitBillingDataL.Text += "<td width=\"400px\">Backup af mail/mail, onedrive og sharepoint filer</td>";
                    HitBillingDataL.Text += "<td><select style=\"height:35px;\" id=\"Hmfsi_" + sId + "\" name=\"Hmfsi_" + sId + "\" >";
                    HitBillingDataL.Text += "<option " + sHmfsiIngenSel + " value=\"Ingen\" >Ingen</option> ";
                    HitBillingDataL.Text += "<option " + sHmfsiMailSel + " value=\"Mail\" >Mail</option> ";
                    HitBillingDataL.Text += "<option " + sHmfsiMOSFSel + " value=\"Mail, Onedrive og Sharepoint filer\" >Mail, Onedrive og Sharepoint filer</option> ";
                    HitBillingDataL.Text += "</select></td>";
                    HitBillingDataL.Text += "</tr>";

                    string sSwteneJaSel = "";
                    if (sSwtene == "Ja") sSwteneJaSel = "selected";
                    string sSwteneNejSel = "";
                    if (sSwtene == "Nej") sSwteneNejSel = "selected";

                    HitBillingDataL.Text += "<tr>";
                    HitBillingDataL.Text += "<td width=\"400px\">Skal Windows 10 Enterprise  være inkluderet<br />(Win10 pro inkluderet I HIT business)</td>";
                    HitBillingDataL.Text += "<td><select style=\"height:35px;\" id=\"Swtene_" + sId + "\" name=\"Swtene_" + sId + "\" >";
                    HitBillingDataL.Text += "<option " + sSwteneJaSel + " value=\"Ja\" >Ja</option> ";
                    HitBillingDataL.Text += "<option " + sSwteneNejSel + " value=\"Nej\" >Nej</option> ";
                    HitBillingDataL.Text += "</select></td>";
                    HitBillingDataL.Text += "</tr>";

                    string sMdmIngenSel = "";
                    if (sMdm == "Ingen") sMdmIngenSel = "selected";
                    string sMdmAndroidSel = "";
                    if (sMdm == "Android") sMdmAndroidSel = "selected";
                    string sMdmiOSSel = "";
                    if (sMdm == "iOS (Iphone/Ipad)") sMdmiOSSel = "selected";
                    string sMdmAndroidiOSSel = "";
                    if (sMdm == "Android/iOS") sMdmAndroidiOSSel = "selected";

                    HitBillingDataL.Text += "<tr>";
                    HitBillingDataL.Text += "<td width=\"400px\">Mobile Device Management</td>";
                    HitBillingDataL.Text += "<td><select style=\"height:35px;\" id=\"Mdm_" + sId + "\" name=\"Mdm_" + sId + "\" >";
                    HitBillingDataL.Text += "<option " + sMdmIngenSel + " value=\"Ingen\" >Ingen</option> ";
                    HitBillingDataL.Text += "<option " + sMdmAndroidSel + " value=\"Android\" >Android</option> ";
                    HitBillingDataL.Text += "<option " + sMdmiOSSel + " value=\"iOS (Iphone/Ipad)\" >iOS (Iphone/Ipad)</option> ";
                    HitBillingDataL.Text += "<option " + sMdmAndroidiOSSel + " value=\"Android/iOS\" >Android/iOS</option> ";
                    HitBillingDataL.Text += "</select></td>";
                    HitBillingDataL.Text += "</tr>";

                    if (sCustomerId != "-1")
                    {
                        HitBillingDataL.Text += "<tr>";
                        HitBillingDataL.Text += "<td width=\"400px\">&nbsp;";
                        HitBillingDataL.Text += "</td>";
                        HitBillingDataL.Text += "<td>";
                        HitBillingDataL.Text += "<input style=\"margin-top:8px;height:35px;\" type='button' id='Cus_" + sId + "' name='Cus_" + sId + "' value='Update Customer Data' onclick=\"__doPostBack('Cus_" + sId + "','')\" />";
                        HitBillingDataL.Text += "</td>";
                        HitBillingDataL.Text += "</tr>";
                    }
                    else
                    {
                        HitBillingDataL.Text += "<tr>";
                        HitBillingDataL.Text += "<td width=\"400px\">&nbsp;";
                        HitBillingDataL.Text += "</td>";
                        HitBillingDataL.Text += "<td style=\"width:300px';\"><br />";
                        HitBillingDataL.Text += "<input style=\"margin-top:8px;height:35px;\" type='button' id='Sav_" + sId + "' name='Sav_" + sId + "' value='Save Customer' onclick=\"__doPostBack('Sav_" + sId + "','')\" />";
                        HitBillingDataL.Text += "</td>";
                        HitBillingDataL.Text += "</tr>";
                    }

                    HitBillingDataL.Text += "</table>";

                    // Netværksudstyr
                    HitBillingDataL.Text += "</br>";
                    HitBillingDataL.Text += "<table class=\"table table-bordered table-striped\" style=\"width:700px;\">";

                    HitBillingDataL.Text += "<tr>";
                    HitBillingDataL.Text += "<td colspan=\"2\"><font size=\"3\">Netværksudstyr</font></td>";
                    HitBillingDataL.Text += "</tr>";

                    HitBillingDataL.Text += "<tr>";
                    HitBillingDataL.Text += "<td width=\"400px\">Firewall(s)</td>";
                    HitBillingDataL.Text += "<td><input style=\"width:50px;height:35px;\" type='text' id='Fir_" + sId + "' name='Fir_" + sId + "' value='" + sFir + "' /></td>";
                    HitBillingDataL.Text += "</tr>";

                    HitBillingDataL.Text += "<tr>";
                    HitBillingDataL.Text += "<td width=\"400px\">WIFI - AccessPunkt(er)</td>";
                    HitBillingDataL.Text += "<td><input style=\"width:50px;height:35px;\" type='text' id='Wifi_" + sId + "' name='Wifi_" + sId + "' value='" + sWifi + "' /></td>";
                    HitBillingDataL.Text += "</tr>";

                    HitBillingDataL.Text += "<tr>";
                    HitBillingDataL.Text += "<td width=\"400px\">Switch (24 porte)</td>";
                    HitBillingDataL.Text += "<td><input style=\"width:50px;height:35px;\" type='text' id='Swi24_" + sId + "' name='Swi24_" + sId + "' value='" + sSwi24 + "' /></td>";
                    HitBillingDataL.Text += "</tr>";

                    HitBillingDataL.Text += "<tr>";
                    HitBillingDataL.Text += "<td width=\"400px\">Switch (8 porte)</td>";
                    HitBillingDataL.Text += "<td><input style=\"width:50px;height:35px;\" type='text' id='Swi8_" + sId + "' name='Swi8_" + sId + "' value='" + sSwi8 + "' /></td>";
                    HitBillingDataL.Text += "</tr>";

                    HitBillingDataL.Text += "<tr>";
                    HitBillingDataL.Text += "<td width=\"400px\">Switch (48 porte)</td>";
                    HitBillingDataL.Text += "<td><input style=\"width:50px;height:35px;\" type='text' id='Swi48_" + sId + "' name='Swi48_" + sId + "' value='" + sSwi48 + "' /></td>";
                    HitBillingDataL.Text += "</tr>";

                    HitBillingDataL.Text += "<tr>";
                    HitBillingDataL.Text += "<td width=\"400px\">Eksisterende Ubiquity udstyr (antal som skal ind i løsningen)</td>";
                    HitBillingDataL.Text += "<td><input style=\"width:50px;height:35px;\" type='text' id='Eksi_" + sId + "' name='Eksi_" + sId + "' value='" + sEksi + "' /></td>";
                    HitBillingDataL.Text += "</tr>";

                    HitBillingDataL.Text += "<tr>";
                    HitBillingDataL.Text += "<td width=\"400px\">DNS Adminstration (gængse domæner ex.com o.lign)</td>";
                    HitBillingDataL.Text += "<td><input style=\"width:50px;height:35px;\" type='text' id='Dns_" + sId + "' name='Dns_" + sId + "' value='" + sDns + "' /></td>";
                    HitBillingDataL.Text += "</tr>";

                    if (sCustomerId != "-1")
                    {
                        HitBillingDataL.Text += "<tr>";
                        HitBillingDataL.Text += "<td width=\"400px\">&nbsp;";
                        HitBillingDataL.Text += "</td>";
                        HitBillingDataL.Text += "<td>";
                        HitBillingDataL.Text += "<input style=\"margin-top:8px; height:35px;\" type='button' id='Net_" + sId + "' name='Net_" + sId + "' value='Update Netværksudstyr' onclick=\"__doPostBack('Net_" + sId + "','')\" />";
                        HitBillingDataL.Text += "</td>";
                        HitBillingDataL.Text += "</tr>";
                    }

                    HitBillingDataL.Text += "</table>";

                    // Ekstra tilføjelser
                    HitBillingDataL.Text += "</br>";
                    HitBillingDataL.Text += "<table class=\"table table-bordered table-striped\" style=\"width:700px;\">";

                    HitBillingDataL.Text += "<tr>";
                    HitBillingDataL.Text += "<td colspan=\"2\"><font size=\"3\">Ekstra tilføjelser</font></td>";
                    HitBillingDataL.Text += "</tr>";

                    HitBillingDataL.Text += "<tr>";
                    HitBillingDataL.Text += "<td width=\"400px\">Printer(e)</td>";
                    HitBillingDataL.Text += "<td><input style=\"width:50px;height:35px;\" type='text' id='Prin_" + sId + "' name='Prin_" + sId + "' value='" + sPrin + "' /></td>";
                    HitBillingDataL.Text += "</tr>";

                    HitBillingDataL.Text += "<tr>";
                    HitBillingDataL.Text += "<td width=\"400px\">Backup af ekstra postkasser</td>";
                    HitBillingDataL.Text += "<td><input style=\"width:50px;height:35px;\" type='text' id='Bckep_" + sId + "' name='Bckep_" + sId + "' value='" + sBckep + "' /></td>";
                    HitBillingDataL.Text += "</tr>";

                    HitBillingDataL.Text += "<tr>";
                    HitBillingDataL.Text += "<td width=\"400px\">Onsite support (OBS! kun Sjælland - angives I halve dage)</td>";
                    HitBillingDataL.Text += "<td><input style=\"width:50px;height:35px;\" type='text' id='Onsit_" + sId + "' name='Onsit_" + sId + "' value='" + sOnsit + "' /></td>";
                    HitBillingDataL.Text += "</tr>";

                    if (sCustomerId != "-1")
                    {
                        HitBillingDataL.Text += "<tr>";
                        HitBillingDataL.Text += "<td width=\"400px\">&nbsp;";
                        HitBillingDataL.Text += "</td>";
                        HitBillingDataL.Text += "<td>";
                        HitBillingDataL.Text += "<input style=\"margin-top:8px; height:35px;\" type='button' id='Eks_" + sId + "' name='Eks_" + sId + "' value='Update Ekstra tilføjelser' onclick=\"__doPostBack('Eks_" + sId + "','')\" />";
                        HitBillingDataL.Text += "</td>";
                        HitBillingDataL.Text += "</tr>";
                    }

                    HitBillingDataL.Text += "</table>";

                    // users
                    if (sCustomerId != "-1")
                    {
                        // recreate users
                        string sSqlDel = "DELETE FROM [RPNAVConnect].[dbo].[HITUsers] WHERE [CustomerId] = " + sCustomerId;
                        string sDBResultDel = InsertUpdateDatabase(sSqlDel, dbConn);

                        int iUsersCount = 0;

                        if (sADGroup != "")
                        {
                            if (sClientId != "")
                            {
                                if (sClientSecret != "")
                                {
                                    if (sTenant != "")
                                    {                                        
                                        string sAllUsers = await GetADGroupUsers(sADGroup, sClientScopesApplication, sClientId, sClientSecret, sRedirectUri, sTenant);
                                        if (sAllUsers.IndexOf("Error:") == -1)
                                        {
                                            HitBillingDataL.Text += "</br>";
                                            HitBillingDataL.Text += "<table class=\"table table-bordered table-striped\" style=\"width:700px;\">";

                                            HitBillingDataL.Text += "<tr>";
                                            HitBillingDataL.Text += "<td colspan=\"2\"><font size=\"3\">Group Users</font></td>";
                                            HitBillingDataL.Text += "</tr>";

                                            string[] sAllUsersArray = sAllUsers.Split('ш');
                                            foreach (string sUserData in sAllUsersArray)
                                            {
                                                if (sUserData != "")
                                                {
                                                    string sUsername = sUserData.Split('ђ')[0];
                                                    string sUseremail = sUserData.Split('ђ')[1];
                                                    string sAzureId = sUserData.Split('ђ')[2];
                                                    string sUserLic = sUserData.Split('ђ')[3];
                                                    if (sUserLic == "")
                                                    {
                                                        sUserLic = "No assigned licenses";
                                                    }
                                                    if (sUserLic[sUserLic.Length - 1] == 'ћ')
                                                    {
                                                        sUserLic = sUserLic.Substring(0, sUserLic.Length - 1);
                                                    }

                                                    if (sUserLic != "No assigned licenses")
                                                    {
                                                        // user license type
                                                        string sUserLicType = "n/a";
                                                        if (
                                                            (sUserLic.IndexOf("SPE_E3") != -1) ||
                                                            (sUserLic.IndexOf("ENTERPRISEPACK") != -1) ||
                                                            (sUserLic.IndexOf("SPE_E5") != -1) ||
                                                            (sUserLic.IndexOf("EMS") != -1)
                                                           )
                                                        {
                                                            sUserLicType = "Hverdags IT Enterpris";
                                                        }
                                                        if (sUserLic.IndexOf("SPB") != -1)
                                                        {
                                                            sUserLicType = "Hverdags IT Business";
                                                        }
                                                        if (sUserLic.IndexOf("SPE_F1") != -1)
                                                        {
                                                            sUserLicType = "Hverdags IT Light";
                                                        }

                                                        if (sUserLicType != "n/a")
                                                        {


                                                            string sSqlIfExists = "SELECT TOP 1 [Type] FROM [RPNAVConnect].[dbo].[HITUsers] WHERE [AzureId] = '" + sAzureId + "'";
                                                            System.Data.OleDb.OleDbDataReader oleReader2;
                                                            System.Data.OleDb.OleDbCommand cmd2 = new System.Data.OleDb.OleDbCommand(sSqlIfExists, dbConn);
                                                            oleReader2 = cmd2.ExecuteReader();
                                                            string sType = "n/a";
                                                            if (oleReader2.Read())
                                                            {
                                                                if (!oleReader2.IsDBNull(0))
                                                                {
                                                                    sType = oleReader2.GetString(0);
                                                                }
                                                            }
                                                            oleReader2.Close();

                                                            /*
                                                            string sEnterpriseSelected = "";
                                                            if ((sType == "Hverdags IT Enterpris") || (sType == "n/a")) sEnterpriseSelected = "selected";
                                                            string sBusinessSelected = "";
                                                            if (sType == "Hverdags IT Business") sBusinessSelected = "selected";
                                                            string sLightSelected = "";
                                                            if (sType == "Hverdags IT Light") sLightSelected = "selected";
                                                            */

                                                            HitBillingDataL.Text += "<tr>";
                                                            HitBillingDataL.Text += "<td width=\"400px\">";
                                                            HitBillingDataL.Text += sUsername + " (" + sUseremail + ")";
                                                            HitBillingDataL.Text += "<div style='font-size: 10px;'><br />Licenses:<br />" + sUserLic.Replace("ћ", ", ") + "</div>";
                                                            HitBillingDataL.Text += "</td>";

                                                            HitBillingDataL.Text += "<td>";
                                                            HitBillingDataL.Text += sUserLicType;
                                                            HitBillingDataL.Text += "</td>";

                                                            /*
                                                            HitBillingDataL.Text += "<td>";
                                                            HitBillingDataL.Text += "<select style=\"height:35px;\" id=\"type_" + sId + "_" + sAzureId + "\" name=\"type_" + sId + "_" + sAzureId + "\" >";
                                                            HitBillingDataL.Text += "<option " + sEnterpriseSelected + " value= \"Hverdags IT Enterprise\" >Hverdags IT Enterprise</option> ";
                                                            HitBillingDataL.Text += "<option " + sBusinessSelected + " value = \"Hverdags IT Business\" >Hverdags IT Business</option> ";
                                                            HitBillingDataL.Text += "<option " + sLightSelected + " value= \"Hverdags IT Light\" >Hverdags IT Light</option> ";
                                                            HitBillingDataL.Text += "</select>";
                                                            HitBillingDataL.Text += "</td>";
                                                            */

                                                            HitBillingDataL.Text += "</tr>";

                                                            iUsersCount++;

                                                            // add user to db
                                                            if (sType == "n/a")
                                                            {
                                                                string sSqlIns = "INSERT INTO [dbo].[HITUsers] ([CustomerId], [Username], [Email], [AzureId], [Type]) ";
                                                                sSqlIns += "VALUES (" + sId + ", '" + sUsername.Replace("'", "''") + "', '" + sUseremail.Replace("'", "''") + "', '" + sAzureId.Replace("'", "''") + "', 'Hverdags IT Enterprise')";
                                                                string sDBResultIns = InsertUpdateDatabase(sSqlIns, dbConn);
                                                            }
                                                            else
                                                            {
                                                                if (sType != sUserLicType)
                                                                {
                                                                    string sSqlIns = "UPDATE [dbo].[HITUsers] SET [Type] = '" + sUserLicType + "' WHERE [AzureId] = '" + sAzureId + "'";
                                                                    string sDBResultIns = InsertUpdateDatabase(sSqlIns, dbConn);
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                            }

                                            /*
                                            HitBillingDataL.Text += "<tr>";
                                            HitBillingDataL.Text += "<td width=\"400px\">&nbsp;";
                                            HitBillingDataL.Text += "</td>";
                                            HitBillingDataL.Text += "<td>";
                                            HitBillingDataL.Text += "<input style=\"margin-top:8px;height:35px;\" type='button' id='Usr_" + sId + "' name='Usr_" + sId + "' value='Update Users Data' onclick=\"__doPostBack('Usr_" + sId + "','')\" />";
                                            HitBillingDataL.Text += "</td>";
                                            HitBillingDataL.Text += "</tr>";
                                            */
                                        }

                                        HitBillingDataL.Text += "</table>";
                                       
                                    }
                                }
                            }
                        }

                        HitBillingDataL.Text = HitBillingDataL.Text.Replace("##USRNUM_" + sId + "##", iUsersCount.ToString());

                    }
                }
                oleReader.Close();
                dbConn.Close();                
            }
            catch (Exception ex)
            {
                sResult = ex.ToString();
                HitBillingDataL.Text = "Error$ " + sResult;
            }

            return sResult;
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
                HitBillingDataL.Text += sResult + " <br />";
                return sResult;
            }

            return sResult;
        }

        private async Task<string> GetADGroupUsers(string sGroupName, string sClientScopesApplication, string sClientId, string sClientSecret, string sRedirectUri, string sTenant)
        {
            string sResult = "";
            try
            {
                string[] scopes = sClientScopesApplication.Split(' ');
                string clientId = sClientId;
                string clientSecret = sClientSecret;
                string redirectUri = sRedirectUri;
                string tenantId = sTenant;

                IConfidentialClientApplication confidentialClientApplication = ConfidentialClientApplicationBuilder
                    .Create(clientId)
                    .WithRedirectUri(redirectUri)
                    .WithClientSecret(clientSecret)
                    .WithTenantId(tenantId)
                    .Build();

                // azure app permission level
                ClientCredentialProvider authenticationProvider = new ClientCredentialProvider(confidentialClientApplication);

                var graphClient = new GraphServiceClient(authenticationProvider);

                IGraphServiceGroupsCollectionPage groups = await graphClient.Groups.Request().GetAsync();

                do
                {
                    foreach (var groupgraph in groups)
                    {
                        if (groupgraph.DisplayName.ToLower() == sGroupName.ToLower())
                        {
                            var users = await graphClient.Groups[groupgraph.Id].Members
                                .Request()
                                .GetAsync();
                            do
                            {
                                foreach (var singleuser in users)
                                {
                                    var user = await graphClient.Users[singleuser.Id]
                                        .Request()
                                        .GetAsync();

                                    var lic = await graphClient.Users[singleuser.Id].LicenseDetails
                                        .Request()
                                        .GetAsync();

                                    string sAllLic = "";
                                    foreach (var license in lic)
                                    {
                                        sAllLic += license.SkuPartNumber + "ћ";
                                    }

                                    string sDisplayName = "";
                                    if (user.GivenName != null) sDisplayName = user.DisplayName;
                                    sResult += sDisplayName + "ђ" + user.Mail + "ђ" + user.Id + "ђ" + sAllLic + "ш";
                                }
                            }
                            while (users.NextPageRequest != null && (users = await users.NextPageRequest.GetAsync()).Count > 0);

                            break;
                        }
                    }
                }
                while (groups.NextPageRequest != null && (groups = await groups.NextPageRequest.GetAsync()).Count > 0);

            }
            catch (Exception ex)
            {
                sResult = "Error: " + ex.ToString();
                HitBillingDataL.Text += sResult;
            }

            return sResult;
        }

    }
}