using System;
using System.Linq;
using System.Web.UI.WebControls;
using Microsoft.SharePoint;
using Microsoft.SharePoint.WebControls;

namespace Nauplius.WAS.Layouts.Nauplius.WAS
{
    public partial class Folder : LayoutsPageBase
    {
        private string listId;
        private string itemId;

        protected void Page_Load(object sender, EventArgs e)
        {
            listId = Request.QueryString["List"];
            itemId = Request.QueryString["Item"];
            
            listId = listId.Remove(0, 1);
            listId = listId.Remove(listId.Length - 1);

            var index = itemId.LastIndexOf(';');
            if (index > 0)
                itemId = itemId.Substring(0, index);

            using (SPContext.Current.Web)
            {
                var listGuid = new Guid(listId);
                var list = Web.Lists[listGuid];
                var folder = list.GetItemById(Convert.ToInt32(itemId));
                lblFolderName.Text = folder.DisplayName;
                var fileFormats = new SupportedFileFormats();

                foreach (var fileFormat in fileFormats.WriteFormats)
                {
                    ddlFolder.Items.Add(fileFormat);
                }

                btnElip.Attributes.Add("onclick", "ShowLocationTree(" + txtDest.ClientID + "); return false;");
            }


        }

        protected void InitializeConversion(object sender, EventArgs e)
        {
            var location = "";
            var result = false;

            using (SPContext.Current.Web)
            {
                var listGuid = new Guid(listId);
                var list = Web.Lists[listGuid];
                var folder = list.GetItemById(Convert.ToInt32(itemId));

                Uri uri;
                Uri.TryCreate(txtDest.Text.Trim(), UriKind.Absolute, out uri);

                if (uri != null)
                {
                    location = uri.AbsoluteUri;
                }

                result = ExecConversion.ConvertFolder(folder.Folder, ddlFolder.SelectedValue, location, false, null);
            }

            Page.Response.Clear();
            Page.Response.Write(result
                                    ? "<script type=\"text/javascript\">window.frameElement.commonModalDialogClose(1);</script>"
                                    : "<script type=\"text/javascript\">window.frameElement.commonModalDialogClose(2);</script>");
            Page.Response.End();
        }

        protected void btnCancel_Click(object sender, EventArgs e)
        {
            Page.Response.Clear();
            Page.Response.Write("<script type=\"text/javascript\">window.frameElement.commonModalDialogClose(0);</script>");
            Page.Response.End();
        }
    }
}
