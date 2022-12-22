<%@ Page MaintainScrollPositionOnPostback="true" Async="true" Language="C#" AutoEventWireup="true" CodeBehind="HitBillingCustomersDF.aspx.cs" Inherits="RPNAVConnect.HitBillingCustomersDF" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>HIT Billing Customers</title>

    <!-- Bootstrap Styles-->
    <link href="assets/css/bootstrap.css" rel="stylesheet" />
    <!-- FontAwesome Styles-->
    <link href="assets/css/font-awesome.css" rel="stylesheet" />
    <!-- Morris Chart Styles-->
    <link href="assets/js/morris/morris-0.4.3.min.css" rel="stylesheet" />
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

        function toogleUsrView(usrViewId) {
            var lnk_obj = document.getElementById("toggleusrview_" + usrViewId);
            var lnk_tbl = document.getElementById("users_" + usrViewId);
            if ((lnk_tbl.style.display == 'none')) {
                lnk_tbl.style.display = 'inline';
                lnk_obj.innerHTML = 'Close User View';
            }
            else {
                lnk_tbl.style.display = 'none';
                lnk_obj.innerHTML = 'Open User View';
            }
        }

        function toogleCustView(usrViewId) {
            var lnk_obj = document.getElementById("togglecustview_" + usrViewId);
            var lnk_tbl = document.getElementById("customers_" + usrViewId);
            if ((lnk_tbl.style.display == 'none')) {
                lnk_tbl.style.display = 'inline';
                lnk_obj.innerHTML = 'Close Customer View';
            }
            else {
                lnk_tbl.style.display = 'none';
                lnk_obj.innerHTML = 'Open Customer View';
            }
        }

        function changeUsrType(optionValToSelect, selectId) {            
            var selectElement = document.getElementById(selectId);
            var selectOptions = selectElement.options;
            for (var opt, j = 0; opt = selectOptions[j]; j++) {
                if (opt.value == optionValToSelect) {
                    selectElement.selectedIndex = j;
                    break;
                }
            }
        }   

        function changeCusSelValue(optionValToSelect, selectId) {
            var selectElement = document.getElementById(selectId);
            var selectOptions = selectElement.options;
            for (var opt, j = 0; opt = selectOptions[j]; j++) {
                if (opt.value == optionValToSelect) {
                    selectElement.selectedIndex = j;
                    break;
                }
            }
        } 

        function changeCusEdtValue(optionValToSelect, selectId) {
            var selectElement = document.getElementById(selectId);
            selectElement.value = optionValToSelect;
        } 

        </script>

</head>
<body>
    <form id="HitBillingCustomersForm" runat="server">
        <div id="element" class="introLoading"></div>

        <div id="wrapper">
            <div class="panel-body">
                <div style="margin:10px; padding:10px; border-style: solid; border-width: 0px;"> 
                    
                    <asp:Label ID="InfoLabel" runat="server" Text=""></asp:Label>

                    <font size="5"><b>HIT BIlling</b></font><font size="4"> :: <b><asp:Label ID="CustomerNameTitle" runat="server" Text=""></asp:Label></b> :: <a href='HitBillingDF.aspx'>All Customers</a></font>
                    <br /><br />

                    <asp:Label ID="HitBillingDataL" runat="server" Text=""></asp:Label>
                    <br />

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
