<%@ Page MaintainScrollPositionOnPostback="true" Async="true" Language="C#" AutoEventWireup="true" CodeBehind="AzureBillingDF.aspx.cs" Inherits="RPNAVConnect.AzureBillingDF" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Azure Billing</title>

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
        function toogleCustomer() {
            var lnk_obj = document.getElementById('cclink');
            var lnk_tbl = document.getElementById('customercomments');
            if ((lnk_tbl.style.display == 'none')) {
                lnk_tbl.style.display = 'inline';
                lnk_obj.innerHTML = 'Close Customer Comments';
            }
            else {
                lnk_tbl.style.display = 'none';
                lnk_obj.innerHTML = 'Open Customer Comments';
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

                    <font size="4"><b><asp:Label ID="TLInfoLabel" runat="server" Text="MICROSOFT PARTNER CENTER"></asp:Label></b></font>
                    <br /><br />

                    <font size="2"><i><asp:Label ID="GetCustSubsLabel" runat="server" Text=""></asp:Label></i></font>
                    <br />
                    <asp:Button ID="GetCustSubs" runat="server" Text="Update Customers & Subscriptions" Visible="true" OnClientClick="invokeLoader();" Height="58px" Width="286px" OnClick="GetCustSubs_Click" /> 
                    <br />
                    <br />
            
                    Please select billing type:<br /><br />

                    <asp:RadioButton ID="rbtnSeats" runat="server" GroupName="BillingType" Text="" Checked="true" AutoPostBack="True" OnCheckedChanged="rbtnSeats_CheckedChanged" /> Charges for Office 365 and Microsoft Azure subscriptions
                    <br />
                    <asp:RadioButton ID="rtbnUsage" runat="server" GroupName="BillingType" Text="" AutoPostBack="True" OnCheckedChanged="rtbnUsage_CheckedChanged" /> Charges for Azure plan, Azure reservations, Software and Marketplace products
                    <br /><br />

                    Invoice top comment - all customers (50 chars max):<br />
                    <asp:TextBox ID="InvoiceCommentTB" AutoPostBack="True" runat="server" Width="300px" OnTextChanged="InvoiceCommentTB_TextChanged"></asp:TextBox>
                    <br /><br />

                        <asp:Label ID="LBFileInfo" Visible="false" runat="server" Text="License-based pricing: <b>April-2020-Cloud-Reseller-Pricelist</b><br /><br />"></asp:Label>                        
                        Please select date period (leave blank for current month):<br />  <br />   

                        <asp:Label ID="MonthL" runat="server" Text="Month (MM):"></asp:Label>
                        &nbsp;
                        <asp:TextBox ID="MonthTB" runat="server" Width="100px"></asp:TextBox>
                        &nbsp;&nbsp;
                        <asp:Label ID="YearL" runat="server" Text="Year (YYYY):"></asp:Label>
                        &nbsp;
                        <asp:TextBox ID="YearTB" runat="server" Width="100px"></asp:TextBox>
                        <br />
                        <br />

                        <font size="3"><b><asp:Label ID="MarkupType" runat="server" Text=""></asp:Label></b></font><br /><br />

                        <a href="javascript:toogleCustomer();" id="cclink">Open Customer Comments</a> 
                        <div id="customercomments" style='display: none;' ">
                            <br /><br />
                            <asp:ListView ID="CustomerComments" runat="server">
                                <LayoutTemplate> 
                                    <table class="table table-bordered table-striped" style="width:600px;">  
                                        <tr class="bg-danger text-white">  
                                            <th>Name</th>  
                                            <th id="thCCName">Comment</th>  
                                        </tr>  
                                    <tbody>  
                                        <asp:PlaceHolder ID="itemPlaceHolder" runat="server" />  
                                    </tbody>  
                                </table>
                                </LayoutTemplate> 
                                <ItemTemplate>  
                                    <tr>  
                                        <td><%# Eval("Name")%></td>  
                                        <td><asp:TextBox runat='server' AutoPostBack="True" OnTextChanged="Unnamed_TextChanged1" Text='<%# Eval("Comment")%>' CustName='<%# Eval("Name")%>' CustId='<%# Eval("Id")%>' ></asp:TextBox></td>  
                                    </tr>  
                                </ItemTemplate>  
                            </asp:ListView>
                        </div>

                        <br />

                        <a href="javascript:toogleMarkup();" id="tmlink">Open Reseller Percentage</a> 
                        <div id="resellermarkup" style='display: none;' ">
                            <br /><br />
                            <asp:ListView ID="CustomersMarkup" runat="server">
                                <LayoutTemplate> 
                                    <table class="table table-bordered table-striped" style="width:600px;">  
                                        <tr class="bg-danger text-white">  
                                            <th>Name</th>  
                                            <th id="thMRName">Markup %</th>  
                                        </tr>  
                                    <tbody>  
                                        <asp:PlaceHolder ID="itemPlaceHolder" runat="server" />  
                                    </tbody>  
                                </table>
                                </LayoutTemplate> 
                                <ItemTemplate>  
                                    <tr>  
                                        <td><%# Eval("Name")%></td>  
                                        <td><asp:TextBox runat='server' AutoPostBack="True" OnTextChanged="Unnamed_TextChanged" Text='<%# Eval("Markup")%>' CustName='<%# Eval("Name")%>' CustId='<%# Eval("Id")%>' ProdId='<%# Eval("ProdId")%>' ></asp:TextBox></td>  
                                    </tr>  
                                </ItemTemplate>  
                            </asp:ListView>
                        </div>

                        <br />
                        <br />

                        <asp:Button ID="AzureBillingDataB" Visible="true" runat="server" Text="Hent data fra Microsoft Partner Center" OnClientClick="invokeLoader();" Height="58px" Width="286px" OnClick="AzureBillingDataB_Click" />
                        <br />
                        <br />

                        <asp:Label ID="AzureBillingDataL" runat="server" Text=""></asp:Label>
                        <br /><hr /><br /><br />

                        <asp:Button ID="PushDataToNavB" runat="server" Text="Push Data To NAV" Visible="false" OnClientClick="invokeLoader();" Height="58px" Width="286px" OnClick="PushDataToNavB_Click" /> 
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

        <script>
            //parent.document.body.style.overflow = "hidden";
        </script>

    </form>
</body>
</html>
