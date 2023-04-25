using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Web;
using System.Web.Mvc;
using TeleBilling_v02_.Models;
using TeleBilling_v02_.Models.DisplayModels;
using TeleBilling_v02_.NavCustomerInfo;
using TeleBilling_v02_.Repository;
using TeleBilling_v02_.Repository.Navision;
using TeleBilling_v02_.Models.Navision;

using System.IO;
using Newtonsoft.Json;
using System.Text;
using System.Web.Routing;
using System.Net.Mail;

namespace TeleBilling_v02_.Controllers
{
    public class DidwwController : Controller
    {
        private IFileRepository fileRepository;
        private IAgreementRepository agreementRepository;

        public DidwwController()
        {
            this.fileRepository = new FileRepository(new DBModelsContainer());
            this.agreementRepository = new AgreementRepository(new DBModelsContainer());
        }

        public DidwwController(IFileRepository fileRepository, IAgreementRepository agreementRepository)
        {
            this.fileRepository = fileRepository;
            this.agreementRepository = agreementRepository;
        }

        public ActionResult ViewDidww(int id=0)
        {
            DidwwDisplayOutboundExtended alldids = new DidwwDisplayOutboundExtended();
            DidwwDisplayOutbound item = new DidwwDisplayOutbound();
            alldids.alldidwws = new List<DidwwDisplayOutbound>();
            alldids.alldidwws.Add(item);
            alldids.pushresults = "Please upload outbound call list.";
            return View(alldids);
        }

        string[] GetFilePath(HttpPostedFileBase postedFile)
        {
            string path = string.Empty;
            string filename = string.Empty;
            string msg = string.Empty;
            if (postedFile != null && postedFile.ContentLength > 0)
            {
                if (postedFile.FileName.EndsWith(".csv"))
                {
                    filename = Path.GetFileName(postedFile.FileName);
                    path = AppDomain.CurrentDomain.BaseDirectory + "upload\\" + filename;
                    postedFile.SaveAs(path);
                }
                else
                {
                    msg = "It is not a .csv file!";
                }
            }
            else
            {
                msg = "file is empty!";
            }
            return new[] { path, filename };
        }

        [HttpPost]
        public ActionResult ViewDidww(HttpPostedFileBase postedFile)
        {
            DidwwDisplayOutboundExtended alldids = new DidwwDisplayOutboundExtended();
            alldids.alldidwws = new List<DidwwDisplayOutbound>();

            // blank line
            DidwwDisplayOutbound itemheader = new DidwwDisplayOutbound();
            itemheader.TimeStart = "";
            itemheader.CLI = "";
            itemheader.Duration = "";
            itemheader.DisconnectCode = "";
            itemheader.DisconnectReason = "";
            itemheader.Rate = "";
            itemheader.Charged = "";
            itemheader.CDRType = "";
            itemheader.CountryName = "";
            itemheader.TrunkName = "";
            itemheader.MinutePrice = "";
            itemheader.SecondPrice = "";
            itemheader.FinalChargeO = "";
            itemheader.NetworkName = "";
            itemheader.FinalChargeK = "";
            itemheader.Counter = "";
            itemheader.Destination = "";
            itemheader.DestinationNetwork = "";
            itemheader.NetworkName = "";
            itemheader.Counter = "#";
            itemheader.Destination = "Network";
            itemheader.BillingDuration = "Duration";
            itemheader.Prefix = "Prefix";
            itemheader.RackpeopleCharge = "Charge";

            string sInfoMsg = "";
            IEnumerable<Agreement> agreementList = null;

            string[] info = null;
            if (postedFile == null)
            {
                if (Session["sesFilePathInfo"] != null)
                {
                    info = (string[])Session["sesFilePathInfo"];
                }
            }
            else
            {
                info = GetFilePath(postedFile);
            }

            string filename = string.Empty;

            try
            {
                //string[] info = GetFilePath(postedFile);

                string filePath = info[0];
                filename = info[1];
                if (filePath != string.Empty)
                {
                    Session.Add("sesFilePathInfo", info);

                    // process csv file
                    //int csvFileId = -1; // back door
                    ///* back door 
                    var typeId = fileRepository.GetType("PriceFile").Id;
                    List<Supplier> list = fileRepository.GetSuppliers().ToList();
                    int supplierId = -1;
                    int csvFileId = -1;
                    foreach (var sup in list)
                    {
                        if (sup.Name == "Didww")
                        {
                            supplierId = sup.Id;
                            foreach (var csvfilefirst in sup.CSVFile)
                            {
                                csvFileId = fileRepository.GetFileByName(csvfilefirst.Name).Id;
                                break;
                            }                            
                            break;
                        }
                    }
                    CSVFile priceFile = fileRepository.GetFileBySupplierID(supplierId, typeId);
                    //*/

                    if (postedFile == null)
                    {
                        agreementList = agreementRepository.GetAgreements(csvFileId).ToList();
                    }

                    // load price list
                    List<string> priceList = new List<string>();
                    List<string> destList = new List<string>();
                    string priceFilePath = AppDomain.CurrentDomain.BaseDirectory + "upload\\" + priceFile.Name;
                    //string priceFilePath = AppDomain.CurrentDomain.BaseDirectory + "upload\\" + "DIDWW - pricelist calculated 1.1.csv";
                    using (StreamReader sr = new StreamReader(priceFilePath, System.Text.Encoding.GetEncoding("iso-8859-1")))
                    {
                        string line;
                        while ((line = sr.ReadLine()) != null)
                        {
                            priceList.Add(line);
                            destList.Add(line.Split(',')[2]);
                        }
                    }

                    // load didww call list
                    string callsFilePath = AppDomain.CurrentDomain.BaseDirectory + "upload\\" + filename;
                    using (StreamReader sr = new StreamReader(callsFilePath, System.Text.Encoding.GetEncoding("iso-8859-1")))
                    {
                        string line;
                        int iCounter = 0;
                        while ((line = sr.ReadLine()) != null)
                        {
                            string[] parts = line.Split(',');
                            DidwwDisplayOutbound item = new DidwwDisplayOutbound();

                            if (iCounter == 0)
                            {
                                item.Counter = "#";
                            }
                            else
                            {
                                item.Counter = iCounter.ToString();
                            }

                            // Time Start,Source,CLI,Destination,Duration,Billing Duration,
                            // Disconnect Code,Disconnect Reason,Rate,Charged,CDR Type,Country Name,Network Name,Trunk Name
                            /*
                            item.TimeStart = parts[0].Replace("\"", "");
                            item.Source = parts[1].Replace("\"", "");
                            item.CLI = parts[2].Replace("\"", "");
                            item.Destination = parts[3].Replace("\"", "");
                            item.Duration = parts[4].Replace("\"", "");
                            item.BillingDuration = parts[5].Replace("\"", "");
                            item.DisconnectCode = parts[6].Replace("\"", "");
                            item.DisconnectReason = parts[7].Replace("\"", "");
                            item.Rate = parts[8].Replace("\"", "");
                            item.Charged = parts[9].Replace("\"", "");
                            item.CDRType = parts[10].Replace("\"", "");
                            item.CountryName = parts[11].Replace("\"", "");
                            item.NetworkName = parts[12].Replace("\"", "");
                            item.TrunkName = parts[13].Replace("\"", "");
                            item.MinutePrice = "";
                            item.SecondPrice = "";
                            item.FinalChargeK = "";
                            item.FinalChargeO = "";

                            // calculate charges
                            string sDestination = parts[3].Replace("\"", "");

                            */

                            // Date/Time Start(UTC),Date/Time Connect(UTC),Date/Time End(UTC),Source,CLI,Destination,Duration,Billing Duration,
                            // Disconnect Code,Disconnect Reason, Rate, Charged, CDR Type,Country Name, Network Name,Trunk Name
                            item.TimeStart = parts[0].Replace("\"", "");
                            item.Source = parts[3].Replace("\"", "");
                            item.CLI = parts[4].Replace("\"", "");
                            item.Destination = parts[5].Replace("\"", "");
                            item.Duration = parts[6].Replace("\"", "");
                            item.BillingDuration = parts[7].Replace("\"", "");
                            item.DisconnectCode = parts[8].Replace("\"", "");
                            item.DisconnectReason = parts[9].Replace("\"", "");
                            item.Rate = parts[10].Replace("\"", "");
                            item.Charged = parts[11].Replace("\"", "");
                            item.CDRType = parts[12].Replace("\"", "");
                            item.CountryName = parts[13].Replace("\"", "");
                            item.NetworkName = parts[14].Replace("\"", "");
                            item.TrunkName = parts[15].Replace("\"", "");
                            item.MinutePrice = "";
                            item.SecondPrice = "";
                            item.FinalChargeK = "";
                            item.FinalChargeO = "";

                            // calculate charges
                            string sDestination = parts[5].Replace("\"", "");

                            string sPrefix = "";
                            string sChargeLine = "";
                            string sChargeLineToShow = "";

                            if (iCounter != 0)
                            {
                                string sDestinationNetwork = "";
                                string sFinalChargeK = "";
                                string sFinalChargeO = "";
                                string sSecondPrice = "";
                                string sMinutePrice = "";

                                if (sDestination.Length > 0)
                                {
                                    if (sDestination[0] == '+')
                                    {
                                        sDestination = sDestination.Substring(1);
                                    }

                                    // search destList
                                    for (int i = sDestination.Length; i > 0; i--)
                                    {
                                        string sMatchNumber = "," + sDestination.Substring(0, i) + ",";
                                        var matchingvalues = priceList.Where(x => x.Contains(sMatchNumber));
                                        if (matchingvalues.Count() > 0)
                                        {
                                            sPrefix = sDestination.Substring(0, i);
                                            sChargeLine = matchingvalues.First();
                                            string[] sChargeLineArray = sChargeLine.Split(',');

                                            sDestinationNetwork = sChargeLineArray[0] + "-" + sChargeLineArray[1];

                                            //string sDuration = parts[5].Replace("\"", "");
                                            string sDuration = parts[7].Replace("\"", "");

                                            int iDuration = 0;
                                            try
                                            {
                                                iDuration = Convert.ToInt32(sDuration);
                                            }
                                            catch (Exception ex)
                                            {
                                                ex.ToString();
                                                iDuration = 0;
                                            }

                                            if (iDuration > 0)
                                            {
                                                //string sType = parts[10].Replace("\"", "");
                                                string sType = parts[12].Replace("\"", "");

                                                sDestinationNetwork = sChargeLineArray[0] + "-" + sChargeLineArray[1];

                                                string sBillingType = sChargeLineArray[3];
                                                string sBillingInitialCost = "0";
                                                int iBillingInitialCost = 0;
                                                string sBillingInterval = "0";
                                                if (sBillingType.IndexOf("/") != -1)
                                                {
                                                    try
                                                    {
                                                        // 1/1 60/60
                                                        sBillingInitialCost = sBillingType.Substring(0, sBillingType.IndexOf("/"));
                                                        sBillingInterval = sBillingType.Substring(sBillingType.IndexOf("/") + 1);
                                                        iBillingInitialCost = Convert.ToInt32(sBillingInitialCost);
                                                    }
                                                    catch (Exception ex)
                                                    {
                                                        ex.ToString();
                                                        iBillingInitialCost = 0;
                                                    }
                                                }

                                                string sBillingCharge = "0";
                                                if (sType == "International") sBillingCharge = sChargeLineArray[7];
                                                if (sType == "Origin") sBillingCharge = sChargeLineArray[8];
                                                if (sType == "Local") sBillingCharge = sChargeLineArray[9];

                                                double dBillingCharge = 0;
                                                try
                                                {
                                                    dBillingCharge = Convert.ToDouble(sBillingCharge);
                                                }
                                                catch (Exception ex)
                                                {
                                                    ex.ToString();
                                                    dBillingCharge = 0;
                                                }

                                                double dCost = (double)iBillingInitialCost;

                                                // seconds
                                                if (sBillingInterval == "1")
                                                {
                                                    dCost += ((double)iDuration * (double) dBillingCharge) / (double)60;
                                                    sChargeLineToShow = iBillingInitialCost + " + " + iDuration.ToString() + " * " + dBillingCharge.ToString() + "/60 = " + dCost.ToString() + " øre (" + (dCost / 100).ToString() + " krone" + ")";

                                                    sSecondPrice = dBillingCharge.ToString();
                                                    sMinutePrice = "0";
                                                }

                                                // minutes
                                                if (sBillingInterval == "60")
                                                {
                                                    double dMunutes = Math.Round((double)iDuration / 60, MidpointRounding.AwayFromZero);
                                                    dCost += dMunutes * dBillingCharge;
                                                    sChargeLineToShow = iBillingInitialCost + " + " + dMunutes.ToString() + " * " + dBillingCharge.ToString() + " = " + dCost.ToString() + " øre (" + (dCost / 100).ToString() + " krone" + ")";

                                                    sMinutePrice = dBillingCharge.ToString();
                                                    sSecondPrice = "0";
                                                }

                                                sFinalChargeK = (dCost / 100).ToString();
                                                sFinalChargeO = dCost.ToString();

                                            }

                                            break;
                                        }
                                    }
                                }

                                item.DestinationNetwork = sDestinationNetwork;
                                item.FinalChargeK = sFinalChargeK;
                                item.FinalChargeO = sFinalChargeO;
                                item.Prefix = sPrefix;
                                item.RackpeopleCharge = sChargeLineToShow;
                                item.MinutePrice = sMinutePrice;
                                item.SecondPrice = sSecondPrice;
                            }

                            alldids.alldidwws.Add(item);

                            iCounter++;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.ToString() }, JsonRequestBehavior.AllowGet);
            }

            string msg = filename + " added successfully";

            if (postedFile != null)
            {
                alldids.pushresults = "DIDWW outgoing call log processed.";
            }

            // group alldids by destination
            DidwwDisplayOutboundExtended alldidsgrouped = new DidwwDisplayOutboundExtended();
            alldidsgrouped.alldidwws = new List<DidwwDisplayOutbound>();
            int iSumCounter = 1;
            foreach (var singledid in alldids.alldidwws)
            {
                string sDestinationNetwork = singledid.DestinationNetwork;
                string sPrefix = singledid.Prefix;
                string sSource = singledid.Source;

                if (alldidsgrouped.alldidwws.Any(x => x.Source == sSource.Replace("+", "") && x.Prefix == sPrefix) == false)
                {
                    if (sSource != null)
                    {
                        if (sSource != "Source")
                        {
                            DidwwDisplayOutbound item = new DidwwDisplayOutbound();

                            // 2020-06-09 12:48:57
                            DateTime dtCT = DateTime.Now;
                            string sCT = dtCT.Year.ToString().PadLeft(4, '0') + "-";
                            sCT += dtCT.Month.ToString().PadLeft(2, '0') + "-";
                            sCT += dtCT.Day.ToString().PadLeft(2, '0') + " 00:00:00";
                            item.TimeStart = sCT;

                            item.CLI = "";
                            item.Destination = sDestinationNetwork;
                            item.Duration = "";
                            item.DisconnectCode = "";
                            item.DisconnectReason = "";
                            item.Rate = "";
                            item.Charged = "";
                            item.CDRType = "";
                            item.CountryName = "";
                            item.TrunkName = "";
                            item.MinutePrice = "";
                            item.SecondPrice = "";
                            item.FinalChargeO = "";
                            item.BillingDuration = "";
                            item.NetworkName = sDestinationNetwork;
                            item.Prefix = sPrefix;
                            item.FinalChargeK = "";
                            item.RackpeopleCharge = "";
                            item.Source = sSource.Replace("+", "");
                            item.Counter = iSumCounter.ToString();
                            item.BillingDuration = "0";
                            item.NetworkName = sDestinationNetwork;
                            item.Prefix = sPrefix;
                            item.FinalChargeK = "0";
                            item.RackpeopleCharge = "kr. 0";
                            alldidsgrouped.alldidwws.Add(item);

                            iSumCounter++;
                        }
                    }              
                }

                var singledidprefix = alldidsgrouped.alldidwws.Where(x => x.Source == sSource.Replace("+", "") && x.Prefix == sPrefix).FirstOrDefault();

                if (sSource != null)
                {
                    if (sSource != "Source")
                    {
                        // add cost
                        if (singledid.FinalChargeK != "")
                        {
                            double dCalcCost = Convert.ToDouble(singledidprefix.FinalChargeK);
                            dCalcCost += Convert.ToDouble(singledid.FinalChargeK);
                            alldidsgrouped.alldidwws.Where(x => x.Source == sSource && x.Prefix == sPrefix).FirstOrDefault().FinalChargeK = dCalcCost.ToString();
                            alldidsgrouped.alldidwws.Where(x => x.Source == sSource && x.Prefix == sPrefix).FirstOrDefault().RackpeopleCharge = "kr. " + dCalcCost.ToString();
                        }

                        // add duration
                        if (singledid.BillingDuration != "")
                        {
                            int iDuration = Convert.ToInt32(singledidprefix.BillingDuration);
                            iDuration += Convert.ToInt32(singledid.BillingDuration);
                            alldidsgrouped.alldidwws.Where(x => x.Source == sSource && x.Prefix == sPrefix).FirstOrDefault().BillingDuration = iDuration.ToString();
                        }
                    }
                }
            }

            // add blank line
            DidwwDisplayOutbound itemblank = new DidwwDisplayOutbound();
            itemblank.TimeStart = "";
            itemblank.CLI = "";
            itemblank.Destination = "";
            itemblank.Duration = "";
            itemblank.DisconnectCode = "";
            itemblank.DisconnectReason = "";
            itemblank.Rate = "";
            itemblank.Charged = "";
            itemblank.CDRType = "";
            itemblank.CountryName = "";
            itemblank.TrunkName = "";
            itemblank.MinutePrice = "";
            itemblank.SecondPrice = "";
            itemblank.FinalChargeO = "";
            itemblank.BillingDuration = "";
            itemblank.NetworkName = "";
            itemblank.Prefix = "";
            itemblank.FinalChargeK = "";
            itemblank.RackpeopleCharge = "";
            itemblank.Counter = "";
            itemblank.Destination = "";
            itemblank.DestinationNetwork = "";
            itemblank.NetworkName = "";
            alldids.alldidwws.Add(itemblank);

            // add header
            alldids.alldidwws.Add(itemheader);

            // add calculated values for preview
            foreach (var singledid in alldidsgrouped.alldidwws)
            {
                alldids.alldidwws.Add(singledid);
            }

            if (postedFile == null)
            {
                if (alldids.alldidwws.Count > 0) 
                {
                    List<InvoiceModel> appliedAgreements = new List<InvoiceModel>();
                    foreach (var singledid in alldids.alldidwws)
                    {
                        int iBilDur = 0;
                        try
                        {
                            iBilDur = Convert.ToInt32(singledid.BillingDuration);
                        }
                        catch (Exception ex)
                        {
                            ex.ToString();
                            iBilDur = 0;
                        }

                        if (iBilDur != 0)
                        {
                            if (singledid.FinalChargeK != null)
                            {
                                if (singledid.RackpeopleCharge != null)
                                {
                                    if ((singledid.RackpeopleCharge.IndexOf("kr. ") != -1) && (singledid.FinalChargeK != ""))
                                    {

                                        long lLongCheck = -1;
                                        try
                                        {
                                            lLongCheck = Convert.ToInt64(singledid.Source);
                                        }
                                        catch (Exception ex)
                                        {
                                            ex.ToString();
                                            lLongCheck = -1;
                                        }

                                        if (lLongCheck != -1)
                                        {
                                            if (agreementList.Any(x => Convert.ToInt64(x.Subscriber_range_start) <= Convert.ToInt64(singledid.Source)
                                                                                        && Convert.ToInt64(x.Subscriber_range_end) >= Convert.ToInt64(singledid.Source)))
                                            {
                                                var tempAgreement = agreementList.Single(x => Convert.ToInt64(x.Subscriber_range_start) <= Convert.ToInt64(singledid.Source)
                                                               && Convert.ToInt64(x.Subscriber_range_end) >= Convert.ToInt64(singledid.Source));

                                                InvoiceModel temInvoice = new InvoiceModel();
                                                temInvoice.CVR = tempAgreement.Customer_cvr;

                                                var existedInvoice = appliedAgreements.Where(x => x.CVR == temInvoice.CVR).FirstOrDefault();

                                                if (existedInvoice == null)
                                                {
                                                    temInvoice.LineCollections = new List<InvoiceLineCollectionModel>();

                                                    InvoiceLineCollectionModel invoiceLine = new InvoiceLineCollectionModel()
                                                    {
                                                        //Id = record.Id,
                                                        StartDate = Convert.ToDateTime(singledid.TimeStart),
                                                        EndDate = Convert.ToDateTime(singledid.TimeStart),
                                                        Subscriber_Range_Start = tempAgreement.Subscriber_range_start,
                                                        Subscriber_Range_End = tempAgreement.Subscriber_range_end,
                                                        Agreement_Description = tempAgreement.Description
                                                    };

                                                    string sZN = singledid.Destination;
                                                    if ((singledid.DestinationNetwork != "") && (singledid.DestinationNetwork != null))
                                                    {
                                                        sZN = singledid.DestinationNetwork + " - " + singledid.Destination;
                                                    }
                                                    int iZNSec = Convert.ToInt32(singledid.BillingDuration);
                                                    double dZNMinPrice = 0;
                                                    if (Convert.ToDecimal(singledid.BillingDuration) != 0)
                                                    {
                                                        dZNMinPrice = 60 * Convert.ToDouble(singledid.FinalChargeK) / Convert.ToDouble(singledid.BillingDuration);
                                                    }

                                                    ZoneLinesModel temZone = new ZoneLinesModel()
                                                    {
                                                        ZoneName = sZN,
                                                        ZoneCalls = 1,
                                                        ZoneCallNo = "10036",
                                                        ZoneSeconds = iZNSec,
                                                        ZoneMinuteNo = "10037",
                                                        ZonePriceMinute = Convert.ToDecimal(dZNMinPrice.ToString()),
                                                        ZonePriceCall = Convert.ToDecimal(singledid.FinalChargeK)
                                                    };

                                                    invoiceLine.ZoneLines = new List<ZoneLinesModel>();
                                                    invoiceLine.ZoneLines.Add(temZone);

                                                    temInvoice.LineCollections.Add(invoiceLine);
                                                    appliedAgreements.Add(temInvoice);
                                                }
                                                else
                                                {
                                                    var rangeExisted = existedInvoice.LineCollections.Find(a => Convert.ToInt64(a.Subscriber_Range_Start) <= Convert.ToInt64(singledid.Source)
                                                                                                             && Convert.ToInt64(a.Subscriber_Range_End) >= Convert.ToInt64(singledid.Source));

                                                    if (rangeExisted == null)
                                                    {
                                                        InvoiceLineCollectionModel invoiceLine = new InvoiceLineCollectionModel()
                                                        {
                                                            //Id = record.Id,
                                                            StartDate = Convert.ToDateTime(singledid.TimeStart),
                                                            EndDate = Convert.ToDateTime(singledid.TimeStart),
                                                            Subscriber_Range_Start = tempAgreement.Subscriber_range_start,
                                                            Subscriber_Range_End = tempAgreement.Subscriber_range_end,
                                                            Agreement_Description = tempAgreement.Description
                                                        };

                                                        string sZN = singledid.Destination;
                                                        if ((singledid.DestinationNetwork != "") && (singledid.DestinationNetwork != null))
                                                        {
                                                            sZN = singledid.DestinationNetwork + " - " + singledid.Destination;
                                                        }
                                                        int iZNSec = Convert.ToInt32(singledid.BillingDuration);
                                                        double dZNMinPrice = 0;
                                                        if (Convert.ToDecimal(singledid.BillingDuration) != 0)
                                                        {
                                                            dZNMinPrice = 60 * Convert.ToDouble(singledid.FinalChargeK) / Convert.ToDouble(singledid.BillingDuration);
                                                        }

                                                        ZoneLinesModel temZone = new ZoneLinesModel()
                                                        {
                                                            ZoneName = sZN,
                                                            ZoneCalls = 1,
                                                            ZoneCallNo = "10036",
                                                            ZoneSeconds = iZNSec,
                                                            ZoneMinuteNo = "10037",
                                                            ZonePriceMinute = Convert.ToDecimal(dZNMinPrice.ToString()),
                                                            ZonePriceCall = Convert.ToDecimal(singledid.FinalChargeK)
                                                        };

                                                        foreach (var i in appliedAgreements)
                                                        {
                                                            if (i.CVR == temInvoice.CVR)
                                                            {
                                                                invoiceLine.ZoneLines = new List<ZoneLinesModel>();
                                                                invoiceLine.ZoneLines.Add(temZone);
                                                                i.LineCollections.Add(invoiceLine);
                                                            }
                                                        }
                                                    }
                                                    else
                                                    {
                                                        DateTime StartDate;
                                                        DateTime EndDate;

                                                        if (DateTime.Compare(rangeExisted.StartDate, Convert.ToDateTime(singledid.TimeStart)) < 0) //is earlier than
                                                        {
                                                            StartDate = rangeExisted.StartDate;
                                                        }
                                                        else
                                                        {
                                                            StartDate = Convert.ToDateTime(singledid.TimeStart);
                                                        }

                                                        if (DateTime.Compare(rangeExisted.EndDate, Convert.ToDateTime(singledid.TimeStart)) > 0)// is later than
                                                        {
                                                            EndDate = rangeExisted.EndDate;
                                                        }
                                                        else
                                                        {
                                                            EndDate = Convert.ToDateTime(singledid.TimeStart);
                                                        }

                                                        string sZN = singledid.Destination;
                                                        if ((singledid.DestinationNetwork != "") && (singledid.DestinationNetwork != null))
                                                        {
                                                            sZN = singledid.DestinationNetwork + " - " + singledid.Destination;
                                                        }
                                                        int iZNSec = Convert.ToInt32(singledid.BillingDuration);
                                                        double dZNMinPrice = 0;
                                                        if (Convert.ToDouble(singledid.BillingDuration) != 0)
                                                        {
                                                            dZNMinPrice = 60 * Convert.ToDouble(singledid.FinalChargeK) / Convert.ToDouble(singledid.BillingDuration);
                                                        }

                                                        ZoneLinesModel temZone = new ZoneLinesModel()
                                                        {
                                                            ZoneName = sZN,
                                                            ZoneCalls = 1,
                                                            ZoneCallNo = "10036",
                                                            ZoneSeconds = iZNSec,
                                                            ZoneMinuteNo = "10037",
                                                            ZonePriceMinute = Convert.ToDecimal(dZNMinPrice.ToString()),
                                                            ZonePriceCall = Convert.ToDecimal(singledid.FinalChargeK)
                                                        };
                                                        foreach (var i in appliedAgreements)
                                                        {
                                                            if (i.CVR == temInvoice.CVR)
                                                            {
                                                                foreach (var ii in i.LineCollections)
                                                                {
                                                                    if (Convert.ToInt64(ii.Subscriber_Range_Start) <= Convert.ToInt64(singledid.Source)
                                                                      && Convert.ToInt64(ii.Subscriber_Range_End) >= Convert.ToInt64(singledid.Source))
                                                                    {
                                                                        ii.StartDate = StartDate;
                                                                        ii.EndDate = EndDate;
                                                                        ii.ZoneLines.Add(temZone);
                                                                    }
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }

                    // push to nav now
                    if (appliedAgreements.Count > 0)
                    {
                        var ab = Accumulate(appliedAgreements);
                        List<string> errorMsg = InvoiceGenerator.BillDidww(ab, filename);

                        if (errorMsg.Count == 0)
                        {
                            List<string> emailResult = new List<string>();
                            sInfoMsg = "";
                            foreach (var singleab in appliedAgreements)
                            {
                                sInfoMsg += "Customer NavId: " + singleab.CVR + " pushed to BC. It is now ready to be sent to the customer.\n";
                                emailResult.Add(sInfoMsg);
                                emailResult.Add("");
                                emailResult.Add("Details:");
                                emailResult.Add("");
                            }

                            // send email now
                            try
                            {
                                emailResult.Add("#, TimeStart, Source, Destination, BillingDuration, BillingDuration, DisconnectCode, CDRType, Prefix, RackpeopleCharge");
                                foreach (var item in alldids.alldidwws)
                                {
                                    if ((item.Counter != "#") && (item.Counter != ""))
                                    {
                                        emailResult.Add(item.Counter.PadLeft(3, '0') + ". " + item.TimeStart + ", " + item.Source + ", " + item.Destination + ", " + item.BillingDuration + ", " + item.BillingDuration + ", " + item.DisconnectCode + ", " + item.CDRType + ", " + item.Prefix + ", " + item.RackpeopleCharge);
                                    }
                                }

                                string recipients = "finance@rackpeople.com;sa@rackpeople.dk;mz@rackpeople.dk";
                                SendResultEmail(emailResult, recipients);
                            }
                            catch (Exception ex)
                            {
                                ex.ToString();
                            }
                        }
                        else
                        {
                            int i = 0;
                            foreach (string error in errorMsg)
                            {
                                i += 1;
                                sInfoMsg += i + ". " + error + ";\n";
                            }
                        }
                    }
                }

                alldids.pushresults = sInfoMsg;
            }

            return View(alldids);
            //return RedirectToAction("ViewDidww", new RouteValueDictionary(new { controller = "Didww", action = "ViewDidww", msg = msg }));
        }

        protected void SendResultEmail(List<string> result, string recipients)
        {
            // If there aren't any lines in the result array, we assume 
            // nothing has been submitted.
            if (result.Count == 0)
            {
                return;
            }

            // Compose the result message
            MailMessage msg = new MailMessage();
            msg.From = new MailAddress("billing@rackpeople.dk", "RackPeople NAV Hub");
            foreach (var adr in recipients.Split(';'))
            {
                msg.To.Add(adr);
            }

            // additional recepients
            //msg.To.Add("sa@rackpeople.dk");
            //msg.To.Add("aop@rackpeople.dk");

            msg.Subject = "New invoices are pending in RPBilling";
            msg.IsBodyHtml = true;
            msg.Body = String.Join("<br />", result);

            // Send the message through RP relay
            SmtpClient client = new SmtpClient("relay.rackpeople.com", 25);
            client.UseDefaultCredentials = true;
            client.Send(msg);
        }

        public List<InvoiceModel> Accumulate(List<InvoiceModel> billableList)
        {
            foreach (InvoiceModel invoice in billableList)
            {
                foreach (InvoiceLineCollectionModel line in invoice.LineCollections)
                {
                    line.Accumulated = new List<AccumulatedModel>();
                    foreach (ZoneLinesModel record in line.ZoneLines)
                    {
                        line.Accumulated.Add(
                                new AccumulatedModel()
                                {
                                    Subscriber = line.Subscriber_Range_Start,
                                    ZoneName = record.ZoneName,
                                    Call_No = record.ZoneCallNo,
                                    Minute_No = record.ZoneMinuteNo,
                                    Call_price = record.ZonePriceCall,
                                    Seconds = record.ZoneSeconds,
                                    Minute_price = record.ZonePriceMinute,
                                    styk = 1,
                                    Total = record.ZonePriceCall
                                });
                    }
                }
            }
            return billableList;
        }

        /*
        [HttpPost]
        public ActionResult ViewDidww()
        {
            string sMonth = "";
            string sYear = "";

            bool bPushToNav = false;
            if (Request.Form["PushToNav"] != null)
            {
                bPushToNav = true;
            }

            if (Request.Form["DidwwMonth"] != null)
            {
                sMonth = Request.Form["DidwwMonth"].ToString();
            }

            if (Request.Form["DidwwYear"] != null)
            {
                sYear = Request.Form["DidwwYear"].ToString();
            }

            List<DidwwDisplay> alldids = new List<DidwwDisplay>();
            string API_KEY = "m5wv5bhvsagpqwwctgzkjef6oojsd1c5";

            var webRequestDIDS = WebRequest.Create("https://api.didww.com/v3/dids") as HttpWebRequest;
            if (webRequestDIDS != null)
            {
                webRequestDIDS.Method = "GET";
                webRequestDIDS.ContentType = "application/vnd.api+json";
                webRequestDIDS.Accept = "application/vnd.api+json";
                webRequestDIDS.Headers["Api-Key"] = API_KEY;

                List<string> sCDRExportedGUIDs = new List<string>();
                using (var s = webRequestDIDS.GetResponse().GetResponseStream())
                {
                    using (var sr = new StreamReader(s))
                    {
                        var didsAsJson = sr.ReadToEnd();
                        var dids = JsonConvert.DeserializeObject<Dids>(didsAsJson);

                        for (int i = 0; i < dids.data.Count; i++)
                        {
                            if (dids.data[i].attributes.number != null)
                            {
                                string sDIDNumber = dids.data[i].attributes.number;
                                string sDIDDescription = dids.data[i].attributes.description;
                                if (sDIDNumber != "")
                                {
                                    // create CDR export now
                                    var webRequestCDRE = WebRequest.Create("https://api.didww.com/v3/cdr_exports") as HttpWebRequest;
                                    if (webRequestCDRE != null)
                                    {
                                        webRequestCDRE.Method = "POST";
                                        webRequestCDRE.ContentType = "application/vnd.api+json";
                                        webRequestCDRE.Accept = "application/vnd.api+json";
                                        webRequestCDRE.Headers["Api-Key"] = API_KEY;

                                        string sCDRR = "";
                                        sCDRR += "{";
                                        sCDRR += "\"data\": {";
                                        sCDRR += "   \"type\": \"cdr_exports\",";
                                        sCDRR += "   \"attributes\": {";
                                        sCDRR += "      \"filters\": {";
                                        sCDRR += "         \"year\": \"" + sYear + "\",";
                                        sCDRR += "         \"month\": \"" + sMonth + "\",";
                                        sCDRR += "         \"did_number\": \"" + sDIDNumber + "\"";
                                        sCDRR += "      }";
                                        sCDRR += "    }";
                                        sCDRR += "  }";
                                        sCDRR += "}";

                                        var data = Encoding.ASCII.GetBytes(sCDRR);
                                        webRequestCDRE.ContentLength = data.Length;

                                        using (var sW = webRequestCDRE.GetRequestStream())
                                        {
                                            sW.Write(data, 0, data.Length);
                                        }

                                        using (var rW = webRequestCDRE.GetResponse().GetResponseStream())
                                        {
                                            using (var srW = new StreamReader(rW))
                                            {
                                                var CDRExportAsJson = srW.ReadToEnd();
                                                var CDRExport = JsonConvert.DeserializeObject<CDRExportReportResponse>(CDRExportAsJson);

                                                string sCDRExportStatus = CDRExport.data.attributes.status;
                                                string sCDRExportGUID = CDRExport.data.id;

                                                if (sCDRExportGUID != null)
                                                {
                                                    if (sCDRExportGUID != "")
                                                    {
                                                        sCDRExportedGUIDs.Add(sCDRExportGUID + "ђ" + sDIDDescription + "ђ" + sDIDNumber);
                                                        //Console.WriteLine("Request pending for " + sDIDNumber + ", Guid: " + sCDRExportGUID + ".");
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                // Getting CDR Exports now
                System.Threading.Thread.Sleep(3000);

                string sCSVContent = "";

                // now get all exports
                if (sCDRExportedGUIDs.Count > 0)
                {
                    int iCDRGuid = 0;

                    while (iCDRGuid < sCDRExportedGUIDs.Count)
                    {
                        var webRequestCDR = WebRequest.Create("https://api.didww.com/v3/cdr_exports/" + sCDRExportedGUIDs[iCDRGuid].Split('ђ')[0]) as HttpWebRequest;
                        if (webRequestCDR != null)
                        {
                            webRequestCDR.Method = "GET";
                            webRequestCDR.ContentType = "application/vnd.api+json";
                            webRequestCDR.Accept = "application/vnd.api+json";
                            webRequestCDR.Headers["Api-Key"] = API_KEY;

                            using (var s = webRequestCDR.GetResponse().GetResponseStream())
                            {
                                using (var sr = new StreamReader(s))
                                {
                                    var cdrSingleExportAsJson = sr.ReadToEnd();
                                    var cdrSingleExport = JsonConvert.DeserializeObject<CDRSingleExport>(cdrSingleExportAsJson);

                                    if (cdrSingleExport.data.attributes.url != null)
                                    {
                                        string sCDRUrl = cdrSingleExport.data.attributes.url;
                                        string sCDRStatus = cdrSingleExport.data.attributes.status;

                                        //Console.WriteLine("CSV Data request status for: " + sCDRExportedGUIDs[iCDRGuid] + ": " + sCDRStatus);

                                        if (sCDRStatus == "Completed")
                                        {
                                            if (sCDRUrl != "")
                                            {
                                                // now get csv
                                                var webRequestCSV = WebRequest.Create(sCDRUrl) as HttpWebRequest;
                                                if (webRequestCSV != null)
                                                {
                                                    webRequestCSV.Method = "GET";
                                                    webRequestCSV.Accept = "text/csv";
                                                    webRequestCSV.Headers["Api-Key"] = API_KEY;

                                                    using (var sCSV = webRequestCSV.GetResponse().GetResponseStream())
                                                    {
                                                        using (var srCSV = new StreamReader(sCSV))
                                                        {
                                                            if (iCDRGuid != 0)
                                                            {
                                                                srCSV.ReadLine();
                                                            }

                                                            sCSVContent += "DID:ђ" + sCDRExportedGUIDs[iCDRGuid].Split('ђ')[1] + "ђ" + sCDRExportedGUIDs[iCDRGuid].Split('ђ')[2] + "\n";
                                                            sCSVContent += srCSV.ReadToEnd();
                                                            sCSVContent += "\nDURATION:ђ\n";

                                                            //Console.WriteLine("CSV Data found for: " + sCDRExportedGUIDs[iCDRGuid]);
                                                            iCDRGuid++;
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                        else
                                        {
                                            // wait a little
                                            System.Threading.Thread.Sleep(2000);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                if (sCSVContent != "")
                {
                    string[] sCSVContentArray = sCSVContent.Split('\n');
                    int iCount = 0;
                    int iDurationSum = 0;
                    string sDIDDescription = "";
                    string sDIDNumber = "";
                    foreach (string sCSVContentLine in sCSVContentArray)
                    {
                        if (sCSVContentLine != "")
                        {
                            if (sCSVContentLine.IndexOf("DID:ђ") == 0)
                            {
                                // first did line
                                sDIDDescription = sCSVContentLine.Split('ђ')[1];
                                sDIDNumber = sCSVContentLine.Split('ђ')[2];
                                iCount = 0;
                                iDurationSum = 0;
                            }

                            if (sCSVContentLine.IndexOf("DURATION:ђ") == 0)
                            {
                                // sums
                                DidwwDisplay item = new DidwwDisplay();
                                item.DIDDate = sDIDDescription;
                                item.DIDSource = "SUMLINE";
                                item.DID = sDIDNumber;
                                item.DIDDuration = iDurationSum.ToString();
                                alldids.Add(item);
                            }

                            if ((sCSVContentLine.IndexOf("DID:ђ") != 0) && (sCSVContentLine.IndexOf("DURATION:ђ") != 0))
                            {
                                string[] sCSVContentLineArray = sCSVContentLine.Split(',');
                                DidwwDisplay item = new DidwwDisplay();
                                item.DIDCounter = iCount.ToString();
                                item.DIDDate = sCSVContentLineArray[0];
                                item.DIDSource = sCSVContentLineArray[1];
                                item.DID = sCSVContentLineArray[2];
                                item.DIDDestination = sCSVContentLineArray[3];
                                item.DIDDuration = sCSVContentLineArray[4];
                                item.DisconnectInitiator = sCSVContentLineArray[5];
                                item.DisconnectCode = sCSVContentLineArray[6];
                                item.Response = sCSVContentLineArray[7];
                                item.TollFreeAmount = sCSVContentLineArray[8];
                                item.TerminationAmount = sCSVContentLineArray[9];
                                item.MeteredChannelsAmount = sCSVContentLineArray[10];
                                alldids.Add(item);
                                iCount++;

                                int iDuration = 0;
                                try
                                {
                                    iDuration = Convert.ToInt32(sCSVContentLineArray[4]);
                                    iDurationSum += iDuration;
                                }
                                catch (Exception ex)
                                {
                                    ex.ToString();
                                    iDuration = 0;
                                }

                            }
                            
                        }
                    }
                }
            }

            return View(alldids);
        }
        */

    }
}