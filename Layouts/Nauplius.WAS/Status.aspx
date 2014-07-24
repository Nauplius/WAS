<%@ Assembly Name="$SharePoint.Project.AssemblyFullName$" %>
<%@ Import Namespace="Microsoft.SharePoint.ApplicationPages" %>
<%@ Register Tagprefix="SharePoint" Namespace="Microsoft.SharePoint.WebControls" Assembly="Microsoft.SharePoint, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Register Tagprefix="Utilities" Namespace="Microsoft.SharePoint.Utilities" Assembly="Microsoft.SharePoint, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Register Tagprefix="asp" Namespace="System.Web.UI" Assembly="System.Web.Extensions, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35" %>
<%@ Import Namespace="Microsoft.SharePoint" %>
<%@ Assembly Name="Microsoft.Web.CommandUI, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Status.aspx.cs" Inherits="Nauplius.WAS.Layouts.Nauplius.WAS.Status" DynamicMasterPageFile="~masterurl/default.master" %>

<asp:Content ID="PageHead" ContentPlaceHolderID="PlaceHolderAdditionalPageHead" runat="server">
    <script type="text/javascript" src="js/spin.min.js"> </script>
    <script type="text/javascript" src="js/jquery-1.9.1.min.js"> </script>
    <script type="text/javascript">
        var opts = {
            lines: 11,
            length: 13,
            width: 4,
            radius: 15,
            corners: 0,
            rotate: 0,
            direction: 1,
            color: '#000',
            speed: 1.1,
            trail: 47,
            shadow: true,
            hwaccel: false,
            className: 'wait',
            zIndex: 2e9,
            top: 'auto',
            left: 'auto'
        };
        var spinner;

        function runSpinner() {
            var target = document.getElementById('spinwait');
            if (typeof(spinner) == 'undefined') {
                spinner = new Spinner(opts).spin(target);
            }
            var table = $('#ctl00_PlaceHolderMain_table1');
            var lit = $('#litMessage');
            table.hide();
            lit.hide();
        }
    </script>
    <style type="text/css">
        .wait { text-align: center; }
    </style>
</asp:Content>

<asp:Content ID="Main" ContentPlaceHolderID="PlaceHolderMain" runat="server">
    <p id="p1" runat="server" Visible="False">
    </p>
    <br/>
    Job Status: <asp:DropDownList runat="server" ID="ddlStatus"/>
    <br/>
    <br/>
    <asp:Button runat="server" ID="btnSearch" Text="Search" Visible="true" OnClick="GetStatus" OnClientClick=" runSpinner() "/>
    <div id="spinwait" class="wait"></div>
    <asp:Table runat="server" ID="table1" style="margin: 0 auto" CellSpacing="15">
        <asp:TableHeaderRow id="th1" runat="server">
        </asp:TableHeaderRow>    
    </asp:Table>
    <div id="litMessage" class="wait" >
        <asp:Literal runat="server" ID="litErr" Visible="False" Text="No records found."/>
    </div>
</asp:Content>

<asp:Content ID="PageTitle" ContentPlaceHolderID="PlaceHolderPageTitle" runat="server">
    Nauplius.WAS [Word Automation Services] - Status
</asp:Content>

<asp:Content ID="PageTitleInTitleArea" ContentPlaceHolderID="PlaceHolderPageTitleInTitleArea" runat="server" >
</asp:Content>