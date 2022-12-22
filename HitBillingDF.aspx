<%@ Page MaintainScrollPositionOnPostback="true" Async="true" Language="C#" AutoEventWireup="true" CodeBehind="HitBillingDF.aspx.cs" Inherits="RPNAVConnect.HitBillingDF" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>HIT Billing</title>

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

</head>
<body>
    <form id="HitBillingForm" runat="server">

        <div id="element" class="introLoading"></div>

        <div id="wrapper">
            <div class="panel-body">
                <div style="margin:10px; padding:10px; border-style: solid; border-width: 0px;">   

                    <font size="4"><b><asp:Label ID="TLInfoLabel" runat="server" Text="RACKPEOPLE - HIT BILLING"></asp:Label></b></font>
                    <br /><br />

                    <asp:Button ID="CreateNewCustomer" Visible="true" runat="server" Text="Create New Customer" OnClientClick="window.location.href = 'HitBillingCustomersDF.aspx?id=-1'; return false;" Height="48px" Width="200px" />
                    <br />
                    <br />

                    <asp:Label ID="HitBillingDataL" runat="server" Text=""></asp:Label>
                    <br />

                    <asp:Button ID="HitBillingDataB" Visible="true" runat="server" Text="Get Billing Data" OnClientClick="invokeLoader();" Height="58px" Width="286px" OnClick="HitBillingDataB_Click" />
                    <br /><br />                    

                    <asp:Label ID="HitBillingInvoiceDataL" runat="server" Text=""></asp:Label>
                    <br /><br />

                    <asp:Button ID="PushDataToNavB" runat="server" Text="Push Data To NAV" Visible="false" OnClientClick="invokeLoader();" Height="58px" Width="286px" onClick="PushDataToNavB_Click" /> 
                    <br />
                    <br />
            
                    <font size="3"><b><asp:Label ID="PushingDataL" runat="server" Text=""></asp:Label></b></font>

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
