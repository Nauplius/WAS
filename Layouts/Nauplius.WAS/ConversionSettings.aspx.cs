using Microsoft.Office.Word.Server.Conversions;
using Microsoft.SharePoint.WebControls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Nauplius.WAS.Layouts.Nauplius.WAS
{
    public partial class ConversionSettings : LayoutsPageBase
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (IsPostBack) return;
            var fileType = Request.QueryString["fileType"];
            cBoxDelSource.Attributes.Add("onmouseover", "DelSourceHelp()");
            cBoxDelSource.Attributes.Add("onmouseout", "RemoveHelp()");

            if (fileType == "pdf" || fileType == "xps")
            {
                lblPdfOps.Text = fileType == "pdf" ? "PDF Options" : "XPS Options";

                ListItem[] list =
                {
                    new ListItem("Bitmap Embeddable Fonts", "BitmapEmbeddedFonts"),
                    new ListItem("Include Document Properties", "IncludeDocumentProperties"), 
                    new ListItem("Include Document Structure", "IncludeDocumentStructure"), 
                    new ListItem("Optimize for Minimum Size", "OptimizeForMinimumSize"), 
                    new ListItem("Use PDF/A", "UsePdfA"), 
                };

                foreach (var listItem in list)
                {
                    listItem.Attributes.Add("onmouseover", "PdfHelp('" + listItem.Value + "')");
                    listItem.Attributes.Add("onmouseout", "RemoveHelp()");
                }

                cBoxList.Items.AddRange(list);
                ph1.Visible = true;
                BookmarkOptions();
                BalloonOptions();
            }
            else if (fileType == "docx" || fileType == "docm" || fileType == "dotx" ||
                fileType == "dotm" || fileType == "doc" || fileType == "dot")
            {
                lblWordOpts.Text = "Word Options";

                ListItem[] list =
                {
                    new ListItem("Add Thumbnail", "AddThumbnail"),
                    new ListItem("Embed Fonts", "EmbedFonts"),
                    new ListItem("Update Fields", "UpdateFields"), 
                };

                foreach (var listItem in list)
                {
                    listItem.Attributes.Add("onmouseover", "WordHelp('" + listItem.Value + "')");
                    listItem.Attributes.Add("onmouseout", "RemoveHelp()");
                }
                cBoxWordList.Items.AddRange(list);
                ph2.Visible = true;
                phCompat.Visible = true;
                CompatibilityOptions();
            }
            else
            {
                var lblNoOpts = new Label
                {
                    Text = "There are no options for this file type."
                };
                var lcBR = new LiteralControl("<br />");
                ph3.Controls.Add(lblNoOpts);
                ph3.Controls.Add(lcBR);
                ph3.Visible = true;
                btnSave.Enabled = false;
            }
        }

        protected void btnSave_OnClick(object sender, EventArgs e)
        {
            var fileType = Request.QueryString["fileType"];
            var element = Request.QueryString["ParentElement"];
            var fileName = Request.QueryString["fileName"];
            var fileSettings = Request.QueryString["settings"];

            if (fileType == "pdf" || fileType == "xps")
            {
                if (!string.IsNullOrEmpty(element))
                {
                    var pdfOptsOut = new List<string>
                    {
                        "x:" + fileType + ";b:" + dvddl1.SelectedValue + ";l:" + dvddl2.SelectedValue
                    };

                    pdfOptsOut.AddRange(from ListItem li in cBoxList.Items
                                        where li.Selected
                                        select li.Value);

                    var pdfOptsRtn = string.Join(";", pdfOptsOut) + ";d:" + cBoxDelSource.Checked;

                    var response =
                        string.Format(
                            "<script type='text/javascript'>var retArray = new Array; retArray.push(\'{0}\',\'{1}\',\'{2}\',\'{3}\');" +
                            "window.frameElement.commitPopup(retArray);</script>", pdfOptsRtn, element, fileName, fileSettings);
                    Context.Response.Write(response);
                    Context.Response.Flush();
                    Context.Response.End();
                }
            }
            else if (fileType == "docx" || fileType == "docm" || fileType == "dotx" ||
                fileType == "dotm" || fileType == "doc" || fileType == "dot")
            {
                if (!string.IsNullOrEmpty(element))
                {
                    var docOptsOut = new List<string>
                    {
                        "x:" + fileType + ";c:" + dvddl3.SelectedValue
                    };

                    docOptsOut.AddRange(from ListItem li in cBoxWordList.Items
                                            where li.Selected
                                            select li.Value);

                    var wordOptsRtn = string.Join(";", docOptsOut) + ";d:" + cBoxDelSource.Checked;

                    var response =
                        string.Format(
                            "<script type='text/javascript'>var retArray = new Array; retArray.push(\'{0}\',\'{1}\',\'{2}\',\'{3}\');" +
                            "window.frameElement.commitPopup(retArray);</script>", wordOptsRtn, element, fileName, fileSettings);
                    Context.Response.Write(response);
                    Context.Response.Flush();
                    Context.Response.End();
                }
            }
        }

        internal void BookmarkOptions()
        {
            dvddl1.DataSource = Enum.GetNames(typeof(FixedFormatBookmark));
            dvddl1.Attributes.Add("onChange", "BookmarkHelp('" + dvddl1.ClientID + "')");
            dvddl1.DataBind();
            lblBookmarkOpts.Visible = true;
            dvddl1.Visible = true;
        }

        internal void BalloonOptions()
        {
            dvddl2.DataSource = Enum.GetNames(typeof (BalloonState));
            dvddl2.Attributes.Add("onChange", "BalloonHelp('" + dvddl2.ClientID + "')");
            dvddl2.DataBind();
            lblBalloonOpts.Visible = true;
            dvddl2.Visible = true;
        }

        internal void CompatibilityOptions()
        {
            dvddl3.DataSource = Enum.GetNames(typeof (CompatibilityMode));
            dvddl3.Attributes.Add("onChange", "CompatibilityHelp('" + dvddl3.ClientID + "')");
            dvddl3.DataBind();
            dvddl3.Items.FindByText("MaintainCurrentSetting").Selected = true;
        }

        protected void btnCancel_Click(object sender, EventArgs e)
        {
            Page.Response.Clear();
            Page.Response.Write("<script type=\"text/javascript\">window.frameElement.commonModalDialogClose(0);</script>");
            Page.Response.End();
        }
    }
}
