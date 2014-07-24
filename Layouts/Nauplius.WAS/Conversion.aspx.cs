using Microsoft.SharePoint;
using Microsoft.SharePoint.WebControls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Nauplius.WAS.Layouts.Nauplius.WAS
{
    public partial class Conversion : LayoutsPageBase
    {
        private Dictionary<SPListItem, DropDownList> _dictionary = new Dictionary<SPListItem, DropDownList>(); 
        
        protected void Page_Load(object sender, EventArgs e)
        {

                string[] items = Request["Items"] != null
                                    ? Request["Items"].Split(new string[] { "|" }, StringSplitOptions.RemoveEmptyEntries)
                                    : new string[] { };

                SPList list = null;

                try
                {
                    using (SPSite site = new SPSite(SPContext.Current.Web.Url))
                    {
                        using (SPWeb web = site.OpenWeb())
                        {
                            list = web.Lists[new Guid(Request["ListId"])];
                        }
                    }
                }
                catch (SPException)
                {

                }

                if (list == null) return;

                CreateTable(items, list);

                if (table1.Rows.Count >= 1)
                {
                    btnConvert.Visible = true;
                    btnCancel.Visible = true;
                    p1.Visible = true;
                }
                else
                {
                    litErr.Visible = true;
                    th1.Visible = false;
                    hr1.Visible = false;
                }   
        }

        private void CreateTable(string[] items, SPList list)
        {
            foreach (var id in items)
            {
                var listItem = list.GetItemById(Int32.Parse(id));

                if (listItem.FileSystemObjectType != SPFileSystemObjectType.File) continue;

                if (!ValidateFileFormat(listItem)) continue;

                var row = new TableRow { ID = "tRow" + listItem.ID };
                table1.Rows.Add(row);

                var cell = new TableCell { Text = listItem.File.Name };
                row.Cells.Add(cell);
                var cell2 = new TableCell();
                cell2.Controls.Add(OutputFileFormats(listItem));
                row.Cells.Add(cell2);
                var dropDownList = (DropDownList)cell2.FindControl("ddl" + listItem.ID);
                _dictionary.Add(listItem, dropDownList);
                var cell3 = new TableCell();
                var textBox = new TextBox { ID = "tBox" + listItem.ID };
                textBox.Attributes.Add("onBlur", "RewriteOutput(this," + cell.ClientID + "," + dropDownList.ClientID + "); return false;");
                textBox.TextMode = TextBoxMode.SingleLine;
                cell3.Controls.Add(textBox);
                row.Cells.Add(cell3);
                var cell4 = new TableCell();
                var textBox1 = new TextBox {ID = "tBox1" + listItem.ID};
                cell4.Controls.AddAt(0, textBox1);
                var btn1 = new Button
                    {
                        ID = "btn1" + listItem.ID,
                        Text = "...",
                        Width = 25,
                        BorderStyle = BorderStyle.None,
                        BorderWidth = 2
                    };
                cell4.Controls.AddAt(1, btn1);
                row.Cells.Add(cell4);
                btn1.Attributes.Add("onclick", "ShowLocationTree(" + textBox1.ClientID + "); return false;");
            }   
        }

        protected string ReturnFileType(SPListItem item)
        {
            string fExt = item[SPBuiltInFieldId.DocIcon].ToString();
            return fExt;
        }

        public static bool ValidateFileFormat(SPListItem item)
        {
            string fExt = item[SPBuiltInFieldId.DocIcon].ToString().ToLower();
            var fileFormats = new SupportedFileFormats();

            if (fileFormats.ReadFormats.Contains(fExt))
            {
                return true;
            }

            return false;
        }

        protected DropDownList OutputFileFormats(SPListItem listItem)
        {
            var dropDownList = new DropDownList {ID = "ddl" + listItem.ID, Width = 100};
            var fileFormats = new SupportedFileFormats();
            var fExt = listItem[SPBuiltInFieldId.DocIcon].ToString().ToLower();

            foreach (var fileFormat in fileFormats.WriteFormats.Where(fileFormat => fileFormat != fExt))
            {
                dropDownList.Items.Add(fileFormat);
            }

            return dropDownList;
        }

        protected void InitializeConversion(object sender, EventArgs e)
        {
            var result = false;

            foreach (SPListItem listItem in _dictionary.Keys)
            {
                var location = "";
                var row = table1.FindControl("tRow" + listItem.ID);
                var cell3 = (TextBox) row.FindControl("tBox" + listItem.ID);
                var cell4 = (TextBox) row.FindControl("tBox1" + listItem.ID);

                if (!string.IsNullOrEmpty(cell3.Text))
                {
                    cell3.Text = cell3.Text.Substring(0, cell3.Text.LastIndexOf("."));                    
                }

                Uri uri;
                Uri.TryCreate(cell4.Text.Trim(), UriKind.Absolute, out uri);

                if (uri != null)
                {
                    location = uri.AbsoluteUri;
                }

                result = ExecConversion.ConvertDocument(listItem, _dictionary[listItem].SelectedValue, cell3.Text, false, null, null, location, false);  
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

    class SupportedFileFormats
    {
        private string[] _readFormats;
        private string[] _writeFormats;

        public string[] ReadFormats
        {
            get { return _readFormats = new[] {"docx", "docm", "dotx", "dotm", 
                "doc", "dot", "rtf", "mhtml", "html", "xml"}; }
        }

        public string[] WriteFormats
        {
            get { return _writeFormats = new[] {"pdf", "xps", "docx", "docm", "dotx", "dotm", 
                "doc", "dot", "rtf", "mhtml", "xml"}; }
        }
    }
}
