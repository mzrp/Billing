<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="RPNAVConnect.Default1" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>NavImport Dashboard</title>
    <meta charset="utf-8">
    <meta name="viewport" content="width=device-width, initial-scale=1">
    <link rel="stylesheet" href="bootstrap.min.css">
    <script src="jquery.min.js"></script>
    <script src="bootstrap.min.js"></script>
    <link rel="stylesheet" href="jqueryIntroLoader-master/dist/css/introLoader.min.css">
    <script src="jqueryIntroLoader-master/dist/jquery.introLoader.pack.min.js"></script>
</head>
<body>
    <form id="NavImportForm" runat="server">
        <div id="element" class="introLoading"></div>
        <div>
            <!--<div class="container" style="width:300px; height:200px; position:absolute;	left:50%; top:50%; margin:-100px 0 0 -150px; border-style: solid; border-width: 1px;">-->
            <div class="container" style="height: 97vh; margin:10px; padding:10px; border-style: solid; border-width: 0px;">
                <ul class="nav nav-tabs">
                    <li class="active"><a href="Default.aspx">Dashboard</a></li>
                    <li><a href="TimeLogImport.aspx">TimeLog</a></li>
                    <li><a href="#">TSM Backup</a></li>
                    <li><a href="#">SIP Trunk </a></li>
                </ul>
                <br />
                &nbsp;&nbsp;&nbsp;&nbsp;NAV Import Service. Please choose import source.
            </div>            
        </div>
        <script>
            $(document).ready(function () {
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
            });
        </script>
    </form>
</body>
</html>
