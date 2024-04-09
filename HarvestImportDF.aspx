<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="HarvestImportDF.aspx.cs" Inherits="RPNAVConnect.HarvestImportDF" %>

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
    </script>
</head>
<body>
    <form id="RPNAVConnectForm" runat="server">
        <div id="element" class="introLoading"></div>

        <div id="wrapper">

                                <div class="panel-body">
        <div style="margin:10px; padding:10px; border-style: solid; border-width: 0px;">     

            <asp:Label ID="HarvestInfoLabel" runat="server" Text="Harvest Web Service Information"></asp:Label> 
            <br />
            <br />

            <asp:Button ID="GetBCCustomersB" runat="server" Text="Get New BC Customers" OnClientClick="invokeLoader();" Height="58px" Width="286px" OnClick="GetBCCustomersB_Click" />
            
            <br />
            <asp:Label ID="GetBCCustomersL" runat="server" Text=""></asp:Label>

            <br />

            <!--
            Please select CVR and date period<br />  <br />      
            
            <asp:Label ID="VATNoL" runat="server" Text="CVR nummer (efterlades tomt for at hente samtlige kunder):"></asp:Label>
            <br />
            -->

            <asp:TextBox ID="VATNoTB" runat="server" Width="500px" Visible="false"></asp:TextBox>
            <!--
            <br />
            <br />
            <asp:Label ID="InvoiceStatusL" runat="server" Text="Invoice status:"></asp:Label>
            &nbsp;
            -->
            <asp:TextBox ID="InvoiceStatusTB" runat="server" Width="35px" Visible="false">-1</asp:TextBox>
            <br />
            <asp:Label ID="StartL" runat="server" Text="Start (Month+Year):"></asp:Label>
            &nbsp;
            <asp:TextBox ID="StartMonthTB" runat="server" Width="24px"></asp:TextBox>
            <asp:TextBox ID="StartYearTB" runat="server" Width="51px"></asp:TextBox>
            &nbsp;&nbsp;
            <asp:Label ID="EndL" runat="server" Text="End (Month+Year):"></asp:Label>
            &nbsp;
            <asp:TextBox ID="EndMonthTB" runat="server" Width="24px"></asp:TextBox>
            <asp:TextBox ID="EndYearTB" runat="server" Width="51px"></asp:TextBox>
            <br />
            <br />
            <asp:Button ID="HarvestDataB" runat="server" Text="Hent data fra Harvest" OnClientClick="invokeLoader();" Height="58px" Width="286px" OnClick="HarvestDataB_Click" />
            <br />
            <br />
            <asp:Label ID="HarvestDataL" runat="server" Text=""></asp:Label>
            <asp:Label ID="PPSep1" runat="server" Text="<br />"></asp:Label>
            <asp:CheckBox ID="AllowInvoicesWithoutLinesCB" runat="server" Visible="false" Text="Allow Invoices without lines" />
            <asp:Label ID="PPSep2" runat="server" Text="<br />"></asp:Label>
            <asp:Button ID="PushDataToNavB" runat="server" Text="Push Data To Nav" Visible="false" OnClientClick="invokeLoader();" Height="58px" Width="286px" OnClick="PushDataToNavB_Click" /> 
            <asp:Button ID="DeleteMarkedInvoicesB" runat="server" Text="Delete Marked Invoices" Visible="false" OnClientClick="invokeLoader();" Height="58px" Width="286px" OnClick="DeleteMarkedInvoicesB_Click" /> 
            <br />
            <br />
            <asp:Label ID="PushingDataL" runat="server" Text=""></asp:Label>
            <asp:Label ID="PushingDataLErrorData" runat="server" Text=""></asp:Label>
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
