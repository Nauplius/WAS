<%@ Assembly Name="$SharePoint.Project.AssemblyFullName$" %>
<%@ Import Namespace="Microsoft.SharePoint.ApplicationPages" %>
<%@ Register Tagprefix="SharePoint" Namespace="Microsoft.SharePoint.WebControls" Assembly="Microsoft.SharePoint, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Register Tagprefix="Utilities" Namespace="Microsoft.SharePoint.Utilities" Assembly="Microsoft.SharePoint, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Register Tagprefix="asp" Namespace="System.Web.UI" Assembly="System.Web.Extensions, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35" %>
<%@ Import Namespace="Microsoft.SharePoint" %>
<%@ Assembly Name="Microsoft.Web.CommandUI, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Page Language="C#" AutoEventWireup="true" EnableViewState="true" CodeBehind="Convert.aspx.cs" Inherits="Nauplius.WAS.Layouts.Nauplius.WAS.Conversion" DynamicMasterPageFile="~masterurl/default.master" %>

<asp:Content ID="Main" ContentPlaceHolderID="PlaceHolderMain" runat="server">
    <p id="p1" runat="server" Visible="False">
        For each file, select the file type to convert to, and optionally enter a new file name with no extension. New files may take up to 15 minutes to appear in this Library.
    </p>
    <br/>
    <SharePoint:SPGridView runat="server" ID="gvItems" AutoGenerateColumns="False" Enabled="True" OnRowDataBound="gvItems_OnRowDataBound" EnableViewState="True">
        <HeaderStyle HorizontalAlign="Center"/>
        <RowStyle HorizontalAlign="Center" />
        </SharePoint:SPGridView>
    <br/>
    <div id="spinwait" class="wait">
        <br/>
        <br/>
        <br/>
        <br/>
        <br/>
    </div>
    <div id="textwait">
        Please wait...
    </div>
    <asp:Button runat="server" ID="btnConvert" Text="Convert" Visible="False" OnClick="InitializeConversion" UseSubmitBehavior="False" OnClientClick=" runSpinner() " />
    <asp:Button runat="server" ID="btnCancel" Text="Cancel" Visible="False" OnClick="btnCancel_Click" />
   </asp:Content>

<asp:Content ID="PageHead" ContentPlaceHolderID="PlaceHolderAdditionalPageHead" runat="server">
    <script type="text/javascript" src="js/spin.min.js"></script>
    <script type="text/javascript" src="js/jquery-1.9.1.min.js"></script>
    <script type="text/javascript" src="js/convert.js"></script>
    <link rel="stylesheet" type="text/css" href="css/convert.css"/>
</asp:Content>


<asp:Content ID="PageTitle" ContentPlaceHolderID="PlaceHolderPageTitle" runat="server">
Nauplius.WAS [Word Automation Services]
</asp:Content>

<asp:Content ID="PageTitleInTitleArea" ContentPlaceHolderID="PlaceHolderPageTitleInTitleArea" runat="server" >
</asp:Content>