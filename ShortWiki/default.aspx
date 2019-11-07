<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="default.aspx.cs" CodeFile="default.aspx.cs" Inherits="ShortWiki._default" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Wikipedia shortened</title>
    <link href='http://fonts.googleapis.com/css?family=Droid+Sans' rel='stylesheet' type='text/css'/>
    <link href='http://fonts.googleapis.com/css?family=Roboto+Condensed:400,300' rel='stylesheet' type='text/css'/>
    <link href="styles/style.css" rel="stylesheet" type="text/css"/>
    <%--<script src="http://ajax.googleapis.com/ajax/libs/jquery/1.9.1/jquery.min.js" type="text/javascript"></script>--%>
    <meta name="viewport" content="width=device-width, maximum-scale=1.0, initial-scale=1.0, user-scalable=0"/>
    <!-- Created by @getlaidanddie -->
</head>
<body>
    <div class="wrapper">
        <form id="form1" runat="server">
        <h1>
            <asp:ScriptManager ID="ToolkitScriptManager1" runat="server">

            </asp:ScriptManager>
            
            Shorter Wikipedia
        </h1>
        <div class="content">
            <asp:UpdatePanel ID="UpdatePanel1" runat="server">
                <ContentTemplate>
                    <span class="query">
                        <div class="queryWrap">
                        <asp:TextBox ID="searchQuery" runat="server"></asp:TextBox>
                        <ajaxToolkit:AutoCompleteExtender runat="server" ID="AutoComplete1" ServiceMethod="GetCompletionList"
                             TargetControlID="searchQuery" UseContextKey="True" Enabled="True" CompletionSetCount="3"
                             CompletionInterval="100" EnableCaching="True" MinimumPrefixLength="1"
                             CompletionListCssClass="searchCompletionList" CompletionListItemCssClass="searchCompletionItem" CompletionListHighlightedItemCssClass="searchCompletionHighlight"/>
                        <asp:Button ID="searchButton" runat="server" Text="Search" OnClick="searchButton_Click" />
                        </div>
                    </span>
                    <asp:UpdateProgress ID="UpdateProgress1" runat="server">
                        <ProgressTemplate>
                            <img src="images/loading.gif" class="loading"/>
                        </ProgressTemplate>
                    </asp:UpdateProgress>
                    <div class="result"><asp:Label ID="searchResult" runat="server"></asp:Label></div>
                </ContentTemplate>
                <Triggers>
                    <asp:AsyncPostBackTrigger ControlID="aboutLink"/>
                </Triggers>
            </asp:UpdatePanel>
            <span class="respect">From <a href="https://en.wikipedia.org/">Wikipedia</a>, the free encyclopedia | <asp:LinkButton ID="aboutLink" runat="server" OnClick="aboutLink_Click">What is this?</asp:LinkButton></span>
    
        </div>
        </form>
    </div>
</body>
    <script>
        (function (i, s, o, g, r, a, m) {
            i['GoogleAnalyticsObject'] = r; i[r] = i[r] || function () {
                (i[r].q = i[r].q || []).push(arguments)
            }, i[r].l = 1 * new Date(); a = s.createElement(o),
            m = s.getElementsByTagName(o)[0]; a.async = 1; a.src = g; m.parentNode.insertBefore(a, m)
        })(window, document, 'script', '//www.google-analytics.com/analytics.js', 'ga');

        ga('create', 'UA-55214263-1', 'auto');
        ga('send', 'pageview');

</script>
</html>
