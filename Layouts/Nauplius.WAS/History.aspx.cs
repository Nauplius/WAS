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
    public partial class History : LayoutsPageBase
    {
        ReadOnlyCollection<ConversionJobInfo> _jobStatuses;
        private SPServiceApplicationProxy _proxy;

        protected void Page_Load(object sender, EventArgs e)
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
                            return;
                        }


                        if (spSite.SiteSubscription != null)
                        {
                            _jobStatuses = ConversionJobStatus.GetAllJobs(_proxy.DisplayName, spSite.UserToken,
                                                                     spSite.SiteSubscription.Id);
                        }
                        else
                        {
                            _jobStatuses = ConversionJobStatus.GetAllJobs(_proxy.DisplayName, spSite.UserToken, null);
                        }
                    }
                    catch (SPException exception)
                    {
                        SPDiagnosticsService.Local.WriteTrace(0, new SPDiagnosticsCategory("NaupliusWASHistory",
                            TraceSeverity.High, EventSeverity.Error),
                            TraceSeverity.Unexpected, "An unexpected error has occurred", exception.StackTrace);
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

                    GetJobHistory();
                }
            }
        }
        private void GetJobHistory()
        {
            if (_jobStatuses != null)
            {
                foreach (var job in _jobStatuses.Reverse())
                {
                    var row = new TableRow { ID = "tRow" + job.JobId };
                    table1.Rows.Add(row);
                    var cell = new TableCell { Text = job.Name };
                    row.Cells.Add(cell);
                    var cell2 = new TableCell { Text = job.JobId.ToString() };
                    row.Cells.Add(cell2);
                    var cell3 = new TableCell { Text = job.SubmittedTime.ToString(CultureInfo.InvariantCulture) };
                    row.Cells.Add(cell3);
                    var cell4 = new TableCell { Text = job.Canceled.ToString() };
                    row.Cells.Add(cell4);
                }
            }
        }
    }
}
