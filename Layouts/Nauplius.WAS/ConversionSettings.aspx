<%@ Assembly Name="$SharePoint.Project.AssemblyFullName$" %>
<%@ Import Namespace="Microsoft.SharePoint.ApplicationPages" %>
<%@ Register Tagprefix="SharePoint" Namespace="Microsoft.SharePoint.WebControls" Assembly="Microsoft.SharePoint, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Register Tagprefix="Utilities" Namespace="Microsoft.SharePoint.Utilities" Assembly="Microsoft.SharePoint, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Register Tagprefix="asp" Namespace="System.Web.UI" Assembly="System.Web.Extensions, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35" %>
<%@ Import Namespace="Microsoft.SharePoint" %>
<%@ Assembly Name="Microsoft.Web.CommandUI, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Page Language="C#" AutoEventWireup="true" EnableViewState="true" CodeBehind="ConversionSettings.aspx.cs" Inherits="Nauplius.WAS.Layouts.Nauplius.WAS.ConversionSettings" DynamicMasterPageFile="~masterurl/default.master" %>

<asp:Content ID="PageHead" ContentPlaceHolderID="PlaceHolderAdditionalPageHead" runat="server">
    <script type="text/javascript" src="js/conversionsettings.js"></script>
</asp:Content>

<asp:Content ID="Main" ContentPlaceHolderID="PlaceHolderMain" runat="server">
    <!-- PDF/XPS -->
    <asp:PlaceHolder runat="server" ID="ph1" Visible="False">
        <asp:Label ID="lblBookmarkOpts" runat="server" Text="Bookmark Options" />&nbsp;&nbsp;&nbsp;<SharePoint:DVDropDownList runat="server" ID="dvddl1" /><br />
        <asp:Label ID="lblBalloonOpts" runat="server" Text="Balloon Options" />&nbsp;&nbsp;&nbsp;<SharePoint:DVDropDownList runat="server" ID="dvddl2" /><br />
        <asp:Label runat="server" ID="lblPdfOps" /><br />
        <SharePoint:InputFormCheckBoxList runat="server" ID="cBoxList" />
    </asp:PlaceHolder>
    <!-- Word -->
    <asp:PlaceHolder runat="server" ID="ph2" Visible="False">
        <asp:Label runat="server" ID="lblWordOpts" /><br />
        <SharePoint:InputFormCheckBoxList runat="server" ID="cBoxWordList"/>
    </asp:PlaceHolder>
    <asp:PlaceHolder runat="server" ID="ph3"></asp:PlaceHolder>
    <br />
    <asp:PlaceHolder runat="server" ID="phCompat" Visible="False">
        <asp:Label runat="server" ID="lblCompatibilityOps" Text="Compatibility Options" />&nbsp;&nbsp;&nbsp;<SharePoint:DVDropDownList runat="server" ID="dvddl3" /><br />
    </asp:PlaceHolder>
    <asp:PlaceHolder runat="server" ID="phDelSource" Visible="False">
        <asp:Label runat="server" ID="lblDelSource" Text="Delete Source"/><SharePoint:InputFormCheckBox runat="server" ID="cBoxDelSource"/><br />
    </asp:PlaceHolder>
    <asp:Button runat="server" ID="btnSave" Text="Save" OnClick="btnSave_OnClick" />
    <asp:Button runat="server" ID="btnCancel" Text="Cancel" OnClick="btnCancel_Click" />
</asp:Content>

<asp:Content ID="PageTitle" ContentPlaceHolderID="PlaceHolderPageTitle" runat="server">
Application Page
</asp:Content>

<asp:Content ID="PageTitleInTitleArea" ContentPlaceHolderID="PlaceHolderPageTitleInTitleArea" runat="server" >
My Application Page
</asp:Content>
