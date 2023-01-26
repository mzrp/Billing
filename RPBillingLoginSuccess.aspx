<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="RPBillingLoginSuccess.aspx.cs" Inherits="RPNAVConnect.RPBillingLoginSuccess" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>TimeLog To Nav Import</title>

    <!-- Bootstrap Styles-->
    <link href="assets/css/bootstrap.css" rel="stylesheet" />
    <!-- FontAwesome Styles-->
    <link href="assets/css/font-awesome.css" rel="stylesheet" />
    <!-- Morris Chart Styles-->
    <link href="assets/js/morris/morris-0.4.3.min.css" rel="stylesheet" />
    <!-- Custom Styles-->
    <link href="assets/css/custom-styles2.css" rel="stylesheet" />
    <!-- Google Fonts-->
    <link href='https://fonts.googleapis.com/css?family=Open+Sans' rel='stylesheet' type='text/css' /> 
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
    </script>
    <style>
        tr:nth-child(even) {background: #DDD}
        tr:nth-child(odd) {background: #FFF}
    </style>
</head>
<body>
    <form id="RPNAVConnectForm" runat="server">
        <div id="element" class="introLoading"></div>

        <div id="wrapper">

            <div class="panel-body">
                <div style="margin:10px; padding:10px; border-style: solid; border-width: 0px;">   
                    <b>RACKPEOPLE BILLING, Business Central</b>
                    <br />
                    <br />
                    <asp:Label ID="InfoDataL" runat="server" Text=""></asp:Label>
                    <asp:Button ID="GetTokenBtn" runat="server" Text="Authenticate RP Billing" Visible="true" OnClientClick="invokeLoader();" Height="58px" Width="286px" OnClick="GetTokenBtn_Click" /> 

                    <br />
                    <br />
                    <asp:Label ID="SubscriptionsDueL" runat="server" Text=""></asp:Label>
                </div>
            </div>

        </div>

        <div id="lastscriptdiv" runat="server"></div>

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


