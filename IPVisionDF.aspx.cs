using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.IO;
using System.Data.OleDb;
using System.Text;
using System.Threading;
using System.Globalization;
using System.Configuration;

namespace RPNAVConnect
{
    public partial class IPVisionDF : System.Web.UI.Page
    {
        public string sUploadedFile = "";

        protected void Page_Load(object sender, EventArgs e)
        {
            // do nothing
        }

        protected void IPVsionB_Click(object sender, EventArgs e)
        {
            Thread.CurrentThread.CurrentCulture = new CultureInfo("da-DK");

            if (IPVsionFileUpload.HasFile)
            {
                bool bResult = SaveFile(IPVsionFileUpload.PostedFile);
                if (bResult == true)
                {
                    IPVsionL.Text = "<table cellspacing='3' cellpadding='3' style='width:80%;'>";

                    // header
                    IPVsionL.Text += "<tr style='border-bottom:1pt solid black;'>";
                    IPVsionL.Text += "<td><b>VendorId</b></td>";
                    IPVsionL.Text += "<td><b>Number</b></td>";
                    IPVsionL.Text += "<td><b>Destination</b></td>";
                    IPVsionL.Text += "<td><b>Invoice group</b></td>";
                    IPVsionL.Text += "<td><b>DestinationType</b></td>";
                    IPVsionL.Text += "<td><b>NumberOfCalls</b></td>";
                    IPVsionL.Text += "<td><b>DurationOfCalls</b></td>";
                    IPVsionL.Text += "<td><b>Direction</b></td>";
                    IPVsionL.Text += "<td><b>Price</b></td>";
                    IPVsionL.Text += "</tr>";

                    int iNumberofRows = 0;

                    FileInfo csvFile = new FileInfo(sUploadedFile);
                    Encoding ediEncoding = Encoding.GetEncoding("windows-1252");
                    using (StreamReader sr = new StreamReader(csvFile.FullName, ediEncoding))
                    {
                        String line;
                        while ((line = sr.ReadLine()) != null)
                        {
                            IEnumerable<string> sElement = SplitString(line, ',');
                            if (sElement.Count() != 18)
                            {
                                sElement = SplitString(line, ';');
                            }

                            // do only if 18 elements exist in line
                            if ((sElement.Count() == 18) || (sElement.Count() == 21))
                            {
                                bool bRowEmpty = true;
                                try
                                {
                                    bRowEmpty = (
                                                        (sElement.ElementAt(0) == "") &
                                                        (sElement.ElementAt(1) == "") &
                                                        (sElement.ElementAt(2) == "") &
                                                        (sElement.ElementAt(3) == "") &
                                                        (sElement.ElementAt(4) == "") &
                                                        (sElement.ElementAt(5) == "") &
                                                        (sElement.ElementAt(6) == "") &
                                                        (sElement.ElementAt(7) == "") &
                                                        (sElement.ElementAt(8) == "") &
                                                        (sElement.ElementAt(9) == "") &
                                                        (sElement.ElementAt(10) == "") &
                                                        (sElement.ElementAt(11) == "") &
                                                        (sElement.ElementAt(12) == "") &
                                                        (sElement.ElementAt(13) == "") &
                                                        (sElement.ElementAt(14) == "") &
                                                        (sElement.ElementAt(15) == "") &
                                                        (sElement.ElementAt(16) == "") &
                                                        (sElement.ElementAt(17) == "")
                                                                    );
                                }
                                catch (Exception ex)
                                {
                                    ex.ToString();
                                    bRowEmpty = true;
                                }

                                // do if row is not empty
                                if (bRowEmpty == false)
                                {
                                    // do if this is not header
                                    if (sElement.ElementAt(0).Replace("'", "\"").ToLower() != "id")
                                    {
                                        if (sElement.ElementAt(0).Replace("'", "\"").ToLower()[0] != '#')
                                        {
                                            try
                                            {
                                                string sVendorId = sElement.ElementAt(0).Replace("'", "\"");
                                                string sNumber = sElement.ElementAt(2).Replace("'", "\"");
                                                string sDestination = sElement.ElementAt(4).Replace("'", "\"");
                                                string sDestinationType = "1";
                                                string sPrefix = sElement.ElementAt(6).Replace("'", "\"");
                                                if (sPrefix.ToLower().IndexOf("mobil") != -1) sDestinationType = "2";
                                                if (sPrefix.ToLower().IndexOf("mobile") != -1) sDestinationType = "2";
                                                string sNumberOfCalls = sElement.ElementAt(9).Replace("'", "\"");
                                                string sDurationOfCalls = sElement.ElementAt(9).Replace("'", "\"");
                                                string sDirection = sElement.ElementAt(8).Replace("'", "\"");
                                                string sPrice = sElement.ElementAt(10).Replace("'", "\"");
                                                decimal dPrice = 0;

                                                string sCSVType = "comma";
                                                if (sElement.Count() == 18) sCSVType = "comma";
                                                if (sElement.Count() == 21) sCSVType = "semicolon";

                                                if (sCSVType == "comma")
                                                {
                                                    dPrice = Convert.ToDecimal(sPrice, new CultureInfo("en-US"));
                                                }

                                                if (sCSVType == "semicolon")
                                                {
                                                    dPrice = Convert.ToDecimal(sPrice, new CultureInfo("da-DK"));
                                                }

                                                string sInvoiceGroup = sElement.ElementAt(5).Replace("'", "\"");
                                                if (sInvoiceGroup.ToLower().IndexOf("opkaldsforsøg") != -1)
                                                {
                                                    sDurationOfCalls = "0";
                                                    sDestinationType = "3";
                                                }

                                                if (sDestinationType != "0")
                                                {
                                                    IPVsionL.Text += "<tr style='border-bottom:1pt solid #ddd;'>";
                                                    IPVsionL.Text += "<td>" + sVendorId + "</td>";
                                                    IPVsionL.Text += "<td>" + sNumber + "</td>";
                                                    IPVsionL.Text += "<td>" + sDestination + "</td>";
                                                    IPVsionL.Text += "<td>" + sInvoiceGroup + "</td>";
                                                    IPVsionL.Text += "<td>" + sDestinationType + "</td>";
                                                    IPVsionL.Text += "<td>" + sNumberOfCalls + "</td>";
                                                    IPVsionL.Text += "<td>" + sDurationOfCalls + "</td>";
                                                    IPVsionL.Text += "<td>" + sDirection + "</td>";
                                                    IPVsionL.Text += "<td>" + dPrice.ToString("N") + "</td>";
                                                    IPVsionL.Text += "</tr>";
                                                }

                                                iNumberofRows++;
                                            }
                                            catch (Exception ex)
                                            {
                                                ex.ToString();
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }

                    IPVsionL.Text += "</table>";

                    if (iNumberofRows == 0)
                    {
                        IPVsionL.Text = "No lines found.";
                    }
                    else
                    {
                        PushIPVsionDataToNavB.Visible = true;

                        try
                        {
                            Session["stUploadedFile"] = sUploadedFile;
                        }
                        catch (Exception ex)
                        {
                            ex.ToString();
                        }
                    }
                }
            }
            else
            {
                IPVsionL.Text = "You did not specify a file to upload.";
            }
        }

        private static IEnumerable<string> SplitString(string input, char sComma)
        {
            char[] array = input.ToArray();
            bool insideQuotedString = false;

            for (int i = 0; i < array.Length; i++)
            {
                if (array[i] == sComma && insideQuotedString)
                {
                    array[i] = '\"';
                }
                else if (array[i] == '\"')
                {
                    insideQuotedString = !insideQuotedString;
                }
            }

            return new string(array).Split(sComma).Select(s => s.Trim(' ', '\"').Replace('\"', sComma));
        }

        private bool SaveFile(HttpPostedFile file)
        {
            bool bResult = false;
            try
            {
                string savePath = HttpContext.Current.Server.MapPath("~") + "\\uploaded\\";
                string fileName = IPVsionFileUpload.FileName;
                sUploadedFile = savePath.Replace("\\\\", "\\") + fileName;
                IPVsionFileUpload.SaveAs(sUploadedFile);
                if (System.IO.File.Exists(sUploadedFile))
                {
                    IPVsionL.Text = "Your file was uploaded successfully.";                    
                    bResult = true;
                }
            }
            catch (Exception ex)
            {
                IPVsionL.Text = ex.ToString();
                bResult = false;
            }

            return bResult;
        }

        protected void PushIPVsionDataToNavB_Click(object sender, EventArgs e)
        {
            Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");

            try
            {
                sUploadedFile = (string)Session["stUploadedFile"];
            }
            catch (Exception ex)
            {
                ex.ToString();
                sUploadedFile = "";
            }

            if (sUploadedFile != "")
            {
                int iNumberofRows = 0;

                // open db connection
                // open db connection
                string dbPath = ConfigurationManager.AppSettings["dbpath"].ToString();
                System.Data.OleDb.OleDbConnection dbConn = new System.Data.OleDb.OleDbConnection(dbPath);
                dbConn.Open();

                FileInfo csvFile = new FileInfo(sUploadedFile);
                Encoding ediEncoding = Encoding.GetEncoding("windows-1252");
                using (StreamReader sr = new StreamReader(csvFile.FullName, ediEncoding))
                {
                    String line;
                    while ((line = sr.ReadLine()) != null)
                    {
                        IEnumerable<string> sElement = SplitString(line, ',');

                        if (sElement.Count() != 18)
                        {
                            sElement = SplitString(line, ';');
                        }

                        // do only if 18 elements exist in line
                        if ((sElement.Count() == 18) || (sElement.Count() == 21))
                        {
                            bool bRowEmpty = true;
                            try
                            {
                                bRowEmpty = (
                                                    (sElement.ElementAt(0) == "") &
                                                    (sElement.ElementAt(1) == "") &
                                                    (sElement.ElementAt(2) == "") &
                                                    (sElement.ElementAt(3) == "") &
                                                    (sElement.ElementAt(4) == "") &
                                                    (sElement.ElementAt(5) == "") &
                                                    (sElement.ElementAt(6) == "") &
                                                    (sElement.ElementAt(7) == "") &
                                                    (sElement.ElementAt(8) == "") &
                                                    (sElement.ElementAt(9) == "") &
                                                    (sElement.ElementAt(10) == "") &
                                                    (sElement.ElementAt(11) == "") &
                                                    (sElement.ElementAt(12) == "") &
                                                    (sElement.ElementAt(13) == "") &
                                                    (sElement.ElementAt(14) == "") &
                                                    (sElement.ElementAt(15) == "") &
                                                    (sElement.ElementAt(16) == "") &
                                                    (sElement.ElementAt(17) == "")
                                                                );
                            }
                            catch (Exception ex)
                            {
                                ex.ToString();
                                bRowEmpty = true;
                            }

                            // do if row is not empty
                            if (bRowEmpty == false)
                            {
                                // do if this is not header
                                if (sElement.ElementAt(0).Replace("'", "\"").ToLower() != "id")
                                {
                                    if (sElement.ElementAt(0).Replace("'", "\"").ToLower()[0] != '#')
                                    {
                                        try
                                        {
                                            string sVendorId = sElement.ElementAt(0).Replace("'", "\"");
                                            string sNumber = sElement.ElementAt(2).Replace("'", "\"");
                                            string sDestination = sElement.ElementAt(4).Replace("'", "\"");
                                            string sDestinationType = "1";
                                            string sPrefix = sElement.ElementAt(6).Replace("'", "\"");
                                            if (sPrefix.ToLower().IndexOf("mobil") != -1) sDestinationType = "2";
                                            if (sPrefix.ToLower().IndexOf("mobile") != -1) sDestinationType = "2";
                                            string sNumberOfCalls = sElement.ElementAt(9).Replace("'", "\"");
                                            string sDurationOfCalls = sElement.ElementAt(9).Replace("'", "\"");
                                            string sDirection = sElement.ElementAt(8).Replace("'", "\"");
                                            string sPrice = sElement.ElementAt(10).Replace("'", "\"");
                                            decimal dPrice = 0;

                                            string sCSVType = "comma";
                                            if (sElement.Count() == 18) sCSVType = "comma";
                                            if (sElement.Count() == 21) sCSVType = "semicolon";

                                            if (sCSVType == "comma")
                                            {
                                                dPrice = Convert.ToDecimal(sPrice, new CultureInfo("en-US"));
                                            }

                                            if (sCSVType == "semicolon")
                                            {
                                                dPrice = Convert.ToDecimal(sPrice, new CultureInfo("da-DK"));
                                            }

                                            string sInvoiceGroup = sElement.ElementAt(5).Replace("'", "\"");
                                            if (sInvoiceGroup.ToLower().IndexOf("opkaldsforsøg") != -1)
                                            {
                                                sDurationOfCalls = "0";
                                                sDestinationType = "3";
                                            }

                                            if (sDestinationType != "0")
                                            {
                                                string sCurrentDateTime = DateTime.Now.Year.ToString().PadLeft(4, '0') + "-";
                                                sCurrentDateTime += DateTime.Now.Month.ToString().PadLeft(2, '0') + "-";
                                                sCurrentDateTime += DateTime.Now.Day.ToString().PadLeft(2, '0') + " ";
                                                sCurrentDateTime += DateTime.Now.Hour.ToString().PadLeft(2, '0') + ":";
                                                sCurrentDateTime += DateTime.Now.Minute.ToString().PadLeft(2, '0') + ":";
                                                sCurrentDateTime += DateTime.Now.Second.ToString().PadLeft(2, '0');

                                                string sSQL = "INSERT INTO [TeleBillingImport] ";
                                                sSQL += "(";
                                                sSQL += "[VendorId]";
                                                sSQL += ",[Number]";
                                                sSQL += ",[Destination]";
                                                sSQL += ",[DestinationType]";
                                                sSQL += ",[NumberOfCalls]";
                                                sSQL += ",[DurationOfCalls]";
                                                sSQL += ",[Direction]";
                                                sSQL += ",[Imported]";
                                                sSQL += ",[Price])";
                                                sSQL += "VALUES";
                                                sSQL += "(";
                                                sSQL += sVendorId + "";
                                                sSQL += ",'" + sNumber + "'";
                                                sSQL += ",'" + sDestination + "'";
                                                sSQL += "," + sDestinationType + "";
                                                sSQL += "," + sNumberOfCalls + "";
                                                sSQL += "," + sDurationOfCalls + "";
                                                sSQL += ",'" + sDirection + "'";
                                                sSQL += ",'" + sCurrentDateTime + "'";
                                                sSQL += "," + dPrice.ToString() + ")";

                                                string sDBResult = InsertUpdateDatabase(sSQL, dbConn);
                                                if (sDBResult == "DBOK")
                                                {
                                                    iNumberofRows++;
                                                }
                                                else
                                                {
                                                    IPVsionL.Text += sDBResult + "<br />" + sSQL + "<br />";
                                                }
                                            }
                                        }
                                        catch (Exception ex)
                                        {
                                            IPVsionL.Text += ex.ToString() + "<br />";
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                dbConn.Close();

                PushIPVsionDataToNavB.Visible = false;

                if (iNumberofRows == 0)
                {
                    IPVsionL.Text += "No data found.";
                }
                else
                {
                    IPVsionL.Text = "File " + sUploadedFile + " imported to the table TeleBillingImport<br />";
                    IPVsionL.Text += iNumberofRows.ToString() + " rows added to the database.<br />";

                    try
                    {
                        string sCurrentDateTime = DateTime.Now.Year.ToString().PadLeft(4, '0');
                        sCurrentDateTime += DateTime.Now.Month.ToString().PadLeft(2, '0');
                        sCurrentDateTime += DateTime.Now.Day.ToString().PadLeft(2, '0');
                        sCurrentDateTime += DateTime.Now.Hour.ToString().PadLeft(2, '0');
                        sCurrentDateTime += DateTime.Now.Minute.ToString().PadLeft(2, '0');
                        sCurrentDateTime += DateTime.Now.Second.ToString().PadLeft(2, '0');
                        File.Move(sUploadedFile, sUploadedFile.Replace(".csv", "_CSV2SQL_IMPORTED_AT_" + sCurrentDateTime + ".csv"));
                        IPVsionL.Text += "File renamed into<br />" + sUploadedFile.Replace(".csv", "_CSV2SQL_IMPORTED_AT_" + sCurrentDateTime + ".csv") + ".<br />";
                    }
                    catch (Exception ex)
                    {
                        IPVsionL.Text += ex.ToString() + "<br />";
                    }
                }
            }
        }

        private static string InsertUpdateDatabase(string SQL, OleDbConnection DatabaseFile)
        {
            Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");

            // Get Connection string
            string sResult = "DBOK";

            try
            {
                // Database Object instancing here
                OleDbCommand OleCommand;
                OleCommand = new OleDbCommand(SQL, DatabaseFile);
                OleCommand.CommandTimeout = 600;
                OleCommand.ExecuteNonQuery();
            }
            catch (Exception Ex)
            {
                Ex.ToString();
                sResult = "DBERROR: " + Ex.ToString();
                return sResult;
            }

            return sResult;
        }
    }
}