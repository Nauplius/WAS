<%@ Assembly Name="$SharePoint.Project.AssemblyFullName$" %>
<%@ Import Namespace="Microsoft.SharePoint.ApplicationPages" %>
<%@ Register Tagprefix="SharePoint" Namespace="Microsoft.SharePoint.WebControls" Assembly="Microsoft.SharePoint, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Register Tagprefix="Utilities" Namespace="Microsoft.SharePoint.Utilities" Assembly="Microsoft.SharePoint, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Register Tagprefix="asp" Namespace="System.Web.UI" Assembly="System.Web.Extensions, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35" %>
<%@ Import Namespace="Microsoft.SharePoint" %>
<%@ Assembly Name="Microsoft.Web.CommandUI, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Folder.aspx.cs" Inherits="Nauplius.WAS.Layouts.Nauplius.WAS.Folder" DynamicMasterPageFile="~masterurl/default.master" %>

<asp:Content ID="Main" ContentPlaceHolderID="PlaceHolderMain" runat="server">
    Folder to Convert: <asp:Label runat="server" ID="lblFolderName" /> <br/>
    Convert to Format: <asp:DropDownList runat="server" ID="ddlFolder" Width="100"/> <br />
    Save to Location: <asp:TextBox runat="server" ID="txtDest" /><asp:Button runat="server" ID="btnElip" Text="..." Width="25" BorderStyle="None" BorderWidth="2"/>
    <br/>
    <hr id="hr1" runat="server"/>
    <asp:Button runat="server" ID="btnConvert" Text="Convert" OnClick="InitializeConversion"/> <asp:Button runat="server" ID="btnCancel" Text="Cancel" OnClick="btnCancel_Click"/>
</asp:Content>

<asp:Content ID="PageHead" ContentPlaceHolderID="PlaceHolderAdditionalPageHead" runat="server">
        <script type="text/javascript">
            function ShowLocationTree(elementId) {
                var tBox = document.getElementById(elementId.id);
                var options = {
                    url: "/_layouts/15/Nauplius.WAS/SiteBrowser.aspx?ParentElement=" + tBox.id + "&IsDlg=1",
                    args: null,
                    title: 'Save Location',
                    dialogReturnValueCallback: dialogCallback,
                };
                SP.SOD.execute('sp.ui.dialog.js', 'SP.UI.ModalDialog.showModalDialog', options);

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
Nauplius.WAS [Word Automation Services] - Folder Conversion
</asp:Content>

<asp:Content ID="PageTitleInTitleArea" ContentPlaceHolderID="PlaceHolderPageTitleInTitleArea" runat="server" >
</asp:Content>
