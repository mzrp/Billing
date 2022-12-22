<%@ Page Async="true" Language="C#" AutoEventWireup="true" CodeBehind="CompetellaBillingDF.aspx.cs" Inherits="RPNAVConnect.CompetellaBillingDF" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Competella Billing</title>

    <!-- Bootstrap Styles-->
    <link href="assets/css/bootstrap.css" rel="stylesheet" />
    <!-- FontAwesome Styles-->
    <link href="assets/css/font-awesome.css" rel="stylesheet" />
    <!-- Morris Chart Styles-->
    <link href="assets/js/morris/morris-0.4.3.min.css" rel="stylesheet" />
    <!-- Custom Styles-->
    <link href="assets/css/custom-styles2.css" rel="stylesheet" />
    <!-- Google Fonts-->
    <link href='http://fonts.googleapis.com/css?family=Open+Sans' rel='stylesheet' type='text/css' /> 
    <!-- Intro loader -->
    <link rel="stylesheet" href="jqueryIntroLoader-master/dist/css/introLoader.css">

    <script>
        function toogleInvoices(eventid) {
            if ((document.getElementById('cg_' + eventid).style.display == 'none')) {
                document.getElementById('cg_' + eventid).style.display = 'inline';
                document.getElementById('cgs_' + eventid).style.display = 'inline';
            }
            else {
                document.getElementById('cg_' + eventid).style.display = 'none';
                document.getElementById('cgs_' + eventid).style.display = 'none';
            }
        }
        function invokeLoader() {
            $("#element").introLoader({
                spinJs: {
                    lines: 13, // The number of lines to draw
                    length: 20, // The length of each line
                    width: 10, // The line thickness
                    radius: 30, // The radius of the inner circle
                    corners: 1, // Corner roundness (0..1)
                    color: '#000', // #rgb or #rrggbb or array of colors
                    speed: 1, // Rounds per second
                    trail: 60, // Afterglow percentage
                    shadow: false // Whether to render a shadow
                }
            });
        }
        function toogleMarkup() {
            var lnk_obj = document.getElementById('tmlink');
            var lnk_tbl = document.getElementById('resellermarkup');
            if ((lnk_tbl.style.display == 'none')) {
                lnk_tbl.style.display = 'inline';
                lnk_obj.innerHTML = 'Close Reseller Percentage';
            }
            else {
                lnk_tbl.style.display = 'none';
                lnk_obj.innerHTML = 'Open Reseller Percentage';
            }
        }
        function toogleINVDETAILS(invNo) {
            var lnk_obj = document.getElementById('tmidlink');
            var lnk_tbl = document.getElementById('INVDETAILS_' + invNo);
            if ((lnk_tbl.style.display == 'none')) {
                lnk_tbl.style.display = 'inline';
                lnk_obj.innerHTML = 'Hide Invoice Details';
            }
            else {
                lnk_tbl.style.display = 'none';
                lnk_obj.innerHTML = 'Show Invoice Details';
            }
        }
    </script>

</head>
<body>
    <form id="AzureBillingForm" runat="server">
        <div id="element" class="introLoading"></div>

        <div id="wrapper">
            <div class="panel-body">
                <div style="margin:10px; padding:10px; border-style: solid; border-width: 0px;">     

                    <font size="3"><b><asp:Label ID="TLInfoLabel" runat="server" Text="Competella RP Billing"></asp:Label></b></font>
                    <br /><br />

                        <asp:Label ID="CompetellaBillingDataL" runat="server" Text=""></asp:Label>
                        <br /><hr />
                        <table>
                            <tr>
                                <td>
                                    <asp:TextBox ID="EmailTB" runat="server" Visible="false" Width="230px" Text="USER@rackpeople.dk"></asp:TextBox><asp:TextBox ID="CSVFilePathTB" runat="server" Visible="false"></asp:TextBox>
                                </td>
                            </tr>
                            <tr>
                                <td style="line-height: 10px;">&nbsp;</td>
                            </tr>
                            <tr>
                                <td>
                                    <asp:Button ID="SendCSVFileToEmailB" runat="server" Text="Send CSV File to Email" Visible="false" OnClientClick="invokeLoader();" Height="58px" Width="286px" OnClick="SendCSVFileToEmailB_Click" /> 
                                </td>
                            </tr>
                        </table>
                         <hr /><br />
                        <asp:Button ID="PushDataToSubscriptionsB" runat="server" Text="Push Data To Subscriptions" Visible="false" OnClientClick="invokeLoader();" Height="58px" Width="286px" OnClick="PushDataToSubscriptionsB_Click" /> 
                        <br />
                        <br />
                        <font size="3"><asp:Label ID="PushingDataL" runat="server" Text=""></asp:Label></font>

                    </div>
            </div>
        </div>

        <div id="lastscriptdiv"></div>

        <!-- JS Scripts-->
        <!-- jQuery Js -->
        <script src="assets/js/jquery-1.10.2.js"></script>
        
        <!-- Bootstrap Js -->
        <script src="assets/js/bootstrap.min.js"></script>
	 
        <!-- Metis Menu Js -->
        <script src="assets/js/jquery.metisMenu.js"></script>

        <!-- Morris Chart Js -->
        <script src="assets/js/morris/raphael-2.1.0.min.js"></script>
        <script src="assets/js/morris/morris.js"></script>
	
	
	    <script src="assets/js/easypiechart.js"></script>
	    <script src="assets/js/easypiechart-data.js"></script>
	
	    <script src="assets/js/Lightweight-Chart/jquery.chart.js"></script>
	
        <!-- Custom Js -->
        <script src="assets/js/custom-scripts.js"></script>

        <!-- Introloader -->
        <script src="jqueryIntroLoader-master/dist/jquery.introLoader.pack.min.js"></script>

    </form>
</body>
</html>
