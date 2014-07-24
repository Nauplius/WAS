<%@ Assembly Name="$SharePoint.Project.AssemblyFullName$" %>
<%@ Import Namespace="Microsoft.SharePoint.ApplicationPages" %>
<%@ Register Tagprefix="SharePoint" Namespace="Microsoft.SharePoint.WebControls" Assembly="Microsoft.SharePoint, Version=14.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Register Tagprefix="Utilities" Namespace="Microsoft.SharePoint.Utilities" Assembly="Microsoft.SharePoint, Version=14.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Register Tagprefix="asp" Namespace="System.Web.UI" Assembly="System.Web.Extensions, Version=3.5.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35" %>
<%@ Import Namespace="Microsoft.SharePoint" %>
<%@ Assembly Name="Microsoft.Web.CommandUI, Version=14.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Conversion.aspx.cs" Inherits="Nauplius.WAS.Layouts.Nauplius.WAS.Conversion" DynamicMasterPageFile="~masterurl/default.master" %>

<asp:Content ID="Main" ContentPlaceHolderID="PlaceHolderMain" runat="server">
    <p id="p1" runat="server" Visible="False">
        For each file, select the file type to convert to, and optionally enter a new file name with no extension. New files may take up to 15 minutes to appear in this Library.
    </p>
    <br/>
    <asp:Table runat="server" ID="table1" style="margin:0 auto" CellSpacing="15">
        <asp:TableHeaderRow id="th1" runat="server">
            <asp:TableHeaderCell Text="Source File" />
            <asp:TableHeaderCell Text="File Type" />
            <asp:TableHeaderCell Text="File Name" />
            <asp:TableHeaderCell Text="Destination" />
        </asp:TableHeaderRow>    
    </asp:Table>
    <hr id="hr1" runat="server"/>
    <br/>
    <asp:Button runat="server" ID="btnConvert" Text="Convert" Visible="False" OnClick="InitializeConversion"/> <asp:Button runat="server" ID="btnCancel" Text="Cancel" Visible="False" OnClick="btnCancel_Click"/>
    <br/>
    <asp:Literal runat="server" ID="litErr" Visible="False" Text="No valid files selected. Please select a supported Word file format."/>
   </asp:Content>

<asp:Content ID="PageHead" ContentPlaceHolderID="PlaceHolderAdditionalPageHead" runat="server">
    <script type="text/javascript">
        function RewriteOutput(elementId,inputFile,dropDownList) {
            var text = document.getElementById(elementId.id);
            var ddl = document.getElementById(dropDownList.id).value;

            if (text.value.indexOf('.') !== -1) {
                text.value = text.value.substr(0, text.value.lastIndexOf('.'));
            }
            
            text.value = text.value + "." + ddl;
            
            if (text.value == "." + ddl) {
                text.value = "";
            }
        }
    </script>
   
    <script type="text/javascript">
        function ShowLocationTree(elementId) {
            var tBox = document.getElementById(elementId.id);
            var options = {
                url: "/_layouts/Nauplius.WAS/SiteBrowser.aspx?ParentElement=" + tBox.id,
                args: null,
                title: 'Save Location',
                dialogReturnValueCallback: dialogCallback,
            };
            SP.UI.ModalDialog.showModalDialog(options);
            
            function dialogCallback(dialogResult, returnValue) {
                if (returnValue != null) {
                    var tBox1 = document.getElementById(returnValue[1]);
                    if (document.all) {
                        tBox1.innerText = returnValue[0]; //IE8 and below support
                    } else {
                        tBox1.textContent = returnValue[0]; //Everything else
                    }
                }
            }
        }
    </script>
</asp:Content>


<asp:Content ID="PageTitle" ContentPlaceHolderID="PlaceHolderPageTitle" runat="server">
Nauplius.WAS [Word Automation Services]
</asp:Content>

<asp:Content ID="PageTitleInTitleArea" ContentPlaceHolderID="PlaceHolderPageTitleInTitleArea" runat="server" >
</asp:Content>
