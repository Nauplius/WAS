using Microsoft.Office.Word.Server.Conversions;
using Microsoft.Office.Word.Server.Service;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Administration;
using Microsoft.SharePoint.WebControls;
using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Web.UI.WebControls;

namespace Nauplius.WAS.Layouts.Nauplius.WAS
{
    public partial class Status : LayoutsPageBase
    {
        ReadOnlyCollection<ConversionJobInfo> _jobStatuses;
        private SPServiceApplicationProxy _proxy;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!Page.IsPostBack)
            {
                foreach (var itemStatus in Enum.GetValues(typeof(ItemTypes)))
                {
                    ddlStatus.Items.Add(itemStatus.ToString());
                }                
            }
        }

        private void GetJobStatus()
        {
            try
            {
                using (SPSite spSite = new SPSite(Web.Url))
                {
                    using (SPWeb spWeb = spSite.OpenWeb())
                    {
                        try
                        {
                            var proxies = SPServiceContext.Current.GetProxies(typeof(WordServiceApplicationProxy));

                            if (proxies.Any())
                            {
                                _proxy = proxies.First();
                            }
                            else
                            {
                                litErr.Visible = true;
                                return;
                            }


                            if (spSite.SiteSubscription != null)
                            {
                                _jobStatuses = ConversionJobStatus.GetAllJobs(_proxy.DisplayName, spSite.UserToken, spSite.SiteSubscription.Id);
                            }
                            else
                            {
                                _jobStatuses = ConversionJobStatus.GetAllJobs(_proxy.DisplayName, spSite.UserToken, null);
                            }
                        }
                        catch (SPException exception)
                        {
                            SPDiagnosticsService.Local.WriteTrace(0, new SPDiagnosticsCategory("NaupliusWASStatus",
                                TraceSeverity.High, EventSeverity.Error), TraceSeverity.Unexpected,
                                "An unexpected error has occurred attempting to find the Word Automation Services Proxy", exception.StackTrace);
                            return;
                        }
                        catch (InvalidOperationException exception2)
                        {
                            SPDiagnosticsService.Local.WriteTrace(0, new SPDiagnosticsCategory("NaupliusWASStatus",
                                TraceSeverity.High, EventSeverity.Error), TraceSeverity.Unexpected,
                                "An unexpected error has occurred attempting to contact the Word Automation Services. Validate that the" +
                                "Word Automation Service is Started.", exception2.StackTrace);
                            return;
                        }
                    }
                }
            }
            catch (SPException exception)
            {
                SPDiagnosticsService.Local.WriteTrace(0, new SPDiagnosticsCategory("NaupliusWASStatus",
                    TraceSeverity.High, EventSeverity.Error), TraceSeverity.Unexpected,
                    "An unexpected error has occurred", exception.StackTrace);                
            }

            if (_jobStatuses.Count == 0 || _jobStatuses == null)
            {
                litErr.Visible = true;
                return;
            }

            foreach (var job in _jobStatuses.Reverse().Take(100))
            {
                var itemType = GetItemType();
                var cJobStatus = new ConversionJobStatus(_proxy.DisplayName, job.JobId, null).GetItems(itemType);

                foreach (var jS in cJobStatus)
                {
                    
                    if (itemType == ItemTypes.Canceled || itemType == ItemTypes.InProgress)
                    {
                        var row = new TableRow();
                        table1.Rows.Add(row);
                        var cell = new TableCell { Text = jS.ConversionId.ToString() };
                        var cell2 = new TableCell { Text = jS.StartTime.ToString() };
                        var cell3 = new TableCell {Text = jS.InputFile};
                        var cell4 = new TableCell {Text = jS.OutputFile};
                        row.Cells.Add(cell);
                        row.Cells.Add(cell2);
                        row.Cells.Add(cell3);
                        row.Cells.Add(cell4);
                    }
                    else if (itemType == ItemTypes.Failed)
                    {
                        var row = new TableRow();
                        table1.Rows.Add(row);
                        var cell = new TableCell {Text = jS.ConversionId.ToString()};
                        var cell2 = new TableCell {Text = jS.InputFile};
                        var cell3 = new TableCell {Text = jS.ErrorCode.ToString(CultureInfo.InvariantCulture)};
                        var cell4 = new TableCell {Text = jS.ErrorMessage};
                        row.Cells.Add(cell);
                        row.Cells.Add(cell2);
                        row.Cells.Add(cell3);
                        row.Cells.Add(cell4);
                    }
                    else if (itemType == ItemTypes.NotStarted)
                    {
                        var row = new TableRow();
                        table1.Rows.Add(row);
                        var cell = new TableCell { Text = jS.ConversionId.ToString() };
                        var cell2 = new TableCell { Text = jS.InputFile };
                        var cell3 = new TableCell {Text = jS.OutputFile};
                        row.Cells.Add(cell);
                        row.Cells.Add(cell2);
                        row.Cells.Add(cell3);
                    }
                    else if (itemType == ItemTypes.Succeeded)
                    {
                        var row = new TableRow();
                        table1.Rows.Add(row);
                        var cell = new TableCell { Text = jS.ConversionId.ToString() };
                        var cell2 = new TableCell { Text = jS.InputFile };
                        var cell3 = new TableCell { Text = jS.OutputFile };
                        var cell4 = new TableCell {Text = jS.CompleteTime.Value.ToString(CultureInfo.InvariantCulture)};
                        row.Cells.Add(cell);
                        row.Cells.Add(cell2);
                        row.Cells.Add(cell3);
                        row.Cells.Add(cell4);
                    }
                }
            }

            if (table1.Rows.Count < 2)
            {
                litErr.Visible = true;
            }
            else
            {
                litErr.Visible = false;
            }
        }

        private ItemTypes GetItemType()
        {
            switch (ddlStatus.SelectedValue)
            {
                case "Canceled":
                    return ItemTypes.Canceled;
                case "Failed":
                    return ItemTypes.Failed;
                case "InProgress":
                    return ItemTypes.InProgress;
                case "NotStarted":
                    return ItemTypes.NotStarted;
                case "Succeeded":
                    return ItemTypes.Succeeded;
            }
            return ItemTypes.Succeeded;
        }

        protected void GetStatus(object sender, EventArgs e)
        {
            switch (ddlStatus.SelectedValue)
            {
                case "Canceled":
                    {
                        var hCell = new TableHeaderCell { Text = "Conversion Id" };
                        var hCell2 = new TableHeaderCell { Text = "Start Time" };
                        var hCell3 = new TableHeaderCell { Text = "Input File" };
                        var hCell4 = new TableHeaderCell { Text = "Output File" };
                        th1.Cells.Add(hCell);
                        th1.Cells.Add(hCell2);
                        th1.Cells.Add(hCell3);
                        th1.Cells.Add(hCell4);
                    }
                    break;
                case "Failed":
                    {
                        var hCell = new TableHeaderCell { Text = "Conversion Id" };
                        var hCell2 = new TableHeaderCell {Text = "Input File"};
                        var hCell3 = new TableHeaderCell { Text = "Error Code" };
                        var hCell4 = new TableHeaderCell { Text = "Error Message" };
                        th1.Cells.Add(hCell);
                        th1.Cells.Add(hCell2);
                        th1.Cells.Add(hCell3);
                        th1.Cells.Add(hCell4);
                    }
                    break;
                case "InProgress":
                    {
                        var hCell = new TableHeaderCell { Text = "Conversion Id" };
                        var hCell2 = new TableHeaderCell { Text = "Start Time" };
                        var hCell3 = new TableHeaderCell { Text = "Input File" };
                        var hCell4 = new TableHeaderCell { Text = "Output File" };
                        th1.Cells.Add(hCell);
                        th1.Cells.Add(hCell2);
                        th1.Cells.Add(hCell3);
                        th1.Cells.Add(hCell4);
                    }
                    break;
                case "NotStarted":
                    {
                        var hCell = new TableHeaderCell { Text = "Conversion Id" };
                        var hCell2 = new TableHeaderCell { Text = "Input File" };
                        var hCell3 = new TableHeaderCell { Text = "Output File" };
                        th1.Cells.Add(hCell);
                        th1.Cells.Add(hCell2);
                        th1.Cells.Add(hCell3);
                    }
                    break;
                case "Succeeded":
                    {
                        var hCell = new TableHeaderCell { Text = "Conversion Id" };
                        var hCell2 = new TableHeaderCell { Text = "Input File" };
                        var hCell3 = new TableHeaderCell { Text = "Output File" };
                        var hCell4 = new TableHeaderCell { Text = "Completed Time" };
                        th1.Cells.Add(hCell);
                        th1.Cells.Add(hCell2);
                        th1.Cells.Add(hCell3);
                        th1.Cells.Add(hCell4);
                    }
                    break;
            }

            GetJobStatus();
        }
    }
}
