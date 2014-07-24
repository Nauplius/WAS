using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Microsoft.Office.Word.Server.Conversions;
using Microsoft.Office.Word.Server.Service;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Administration;
using Microsoft.SharePoint.Workflow;
using Microsoft.SharePoint.WorkflowActions;
using System;
using System.Linq;
using System.Workflow.ComponentModel;
using Microsoft.Web.Hosting.Administration;

namespace Nauplius.WAS
{
    public class ExecConversion
    {
        private static SPServiceApplicationProxy _proxy;

        public static bool ConvertDocument(SPListItem listItem, string fileFormat, string newFileName, bool isWorkflow,
            ActivityExecutionContext executionContext, WorkflowContext __Context, SPFolder folder, string settings, bool isImmediate)
        {
            ISharePointService wfService = null;

            if (executionContext != null)
            {
                wfService = executionContext.GetService<ISharePointService>();
            }

            using (SPSite spSite = new SPSite(listItem.ParentList.ParentWeb.Url))
            {
                using (SPWeb spWeb = spSite.OpenWeb())
                {
                    int i = listItem.Url.IndexOf("/");
                    var listUrl = listItem.Url.Remove(i + 1);

                    var listItemUri = new Uri(listItem.Web.Url + "/" + listItem.Url);
                    var listItemLibraryUri = new Uri(listItem.Web.Url + "/" + listUrl);

                    var fileName = listItem.Name;
                    var idx = fileName.LastIndexOf(".", StringComparison.Ordinal);

                    if (string.IsNullOrEmpty(newFileName))
                    {
                        newFileName = fileName.Replace(fileName.Substring(idx, fileName.Length - idx),
                                                       "." + fileFormat);
                    }
                    else
                    {
                        if (isWorkflow)
                        {
                            newFileName = newFileName + fileName.Replace(fileName.Substring(idx, fileName.Length - idx),
                               "." + fileFormat);
                        }
                    }

                    try
                    {
                        var proxies = SPServiceContext.GetContext(spSite).GetProxies(typeof(WordServiceApplicationProxy));

                        if (proxies.Any())
                        {
                            _proxy = proxies.First();
                        }
                        else
                        {
                            var exception = new SPException();
                            throw exception;
                        }

                        #region ImmediateJob
                        if (isImmediate)
                        {
                            SyncConverter immJob;

                            if (isWorkflow)
                            {
                                immJob = new SyncConverter(_proxy.DisplayName) { UserToken = __Context.InitiatorUser.UserToken };
                            }
                            else
                            {
                                immJob = new SyncConverter(_proxy.DisplayName) { UserToken = spSite.UserToken };
                            }

                            if (spSite.SiteSubscription != null)
                            {
                                immJob.SubscriptionId = spSite.SiteSubscription.Id;
                            }

                            immJob.Settings.OutputFormat = DeriveFileFormat(fileFormat);

                            if (!string.IsNullOrEmpty(settings))
                            {
                                var splitSettings = settings.Split(';');

                                if (fileFormat.ToLower(CultureInfo.InvariantCulture) == splitSettings[0].Remove(0, 2).ToLower(CultureInfo.InvariantCulture))
                                {
                                    switch (fileFormat)
                                    {
                                        case "xps":
                                        case "pdf":
                                        {
                                            immJob.Settings.FixedFormatSettings.Bookmarks =
                                                (FixedFormatBookmark)
                                                    Enum.Parse(typeof (FixedFormatBookmark),
                                                        splitSettings[1].Remove(0, 2));
                                            immJob.Settings.FixedFormatSettings.BalloonState =
                                                (BalloonState)
                                                    Enum.Parse(typeof (BalloonState), splitSettings[2].Remove(0, 2));

                                            if (splitSettings.Contains("BitmapEmbeddedFonts"))
                                            {
                                                immJob.Settings.FixedFormatSettings.BitmapEmbeddedFonts = true;
                                            }

                                            if (splitSettings.Contains("IncludeDocumentProperties"))
                                            {
                                                immJob.Settings.FixedFormatSettings.IncludeDocumentProperties = true;
                                            }

                                            if (splitSettings.Contains("IncludeDocumentStructure"))
                                            {
                                                immJob.Settings.FixedFormatSettings.IncludeDocumentStructure = true;
                                            }

                                            if (splitSettings.Contains("OptimizeForMinimumSize"))
                                            {
                                                immJob.Settings.FixedFormatSettings.OutputQuality =
                                                    FixedFormatQuality.Minimum;
                                            }

                                            if (splitSettings.Contains("UsePdfA"))
                                            {
                                                immJob.Settings.FixedFormatSettings.UsePDFA = true;
                                            }

                                            break;
                                        }

                                        case "doc":
                                        case "docx":
                                        case "docm":
                                        case "dot":
                                        case "dotx":
                                        case "dotm":
                                        { 
                                            immJob.Settings.CompatibilityMode = (CompatibilityMode) 
                                                Enum.Parse(typeof(CompatibilityMode), 
                                                splitSettings[1].Remove(0,2));

                                            if (splitSettings.Contains("AddThumbnail"))
                                            {
                                                immJob.Settings.AddThumbnail = true;
                                            }

                                            if (splitSettings.Contains("EmbedFonts"))
                                            {
                                                immJob.Settings.AddThumbnail = true;
                                            }

                                            if (splitSettings.Contains("UpdateFields"))
                                            {
                                                immJob.Settings.UpdateFields = true;
                                            }

                                            break;
                                        }
                                    }
                                }
                            }
                            var sStream = new SPFileStream(spWeb, 0x1000);
                            var inputStream = listItem.File.OpenBinaryStream();

                            immJob.Convert(inputStream, sStream);

                            try
                            {
                                if (folder == null)
                                {
                                    listItem.Folder.Files.Add(newFileName, sStream);
                                }
                                else
                                {
                                    if (spWeb.Url != folder.ParentWeb.Url)
                                    {
                                        using (SPWeb web2 = spSite.OpenWeb(folder.ParentWeb.Url))
                                        {
                                            folder.Files.Add(newFileName, sStream);
                                        }
                                    }
                                    folder.Files.Add(newFileName, sStream);
                                }
                            }
                            catch (Exception exception)
                            {
                                if (wfService != null)
                                {
                                    Exceptions.CheckedOutException(exception, listItem, wfService, executionContext);
                                    return false;
                                }
                                Exceptions.CheckedOutException(exception, listItem, null, null);
                                return false;
                            }

                            return true;
                        }
                        #endregion

                        #region Timer Conversion Job
                        else
                        {
                            ConversionJob job;

                            if (isWorkflow)
                            {
                                job = new ConversionJob(_proxy.DisplayName) { UserToken = __Context.InitiatorUser.UserToken };
                            }
                            else
                            {
                                job = new ConversionJob(_proxy.DisplayName) { UserToken = spSite.UserToken };
                            }

                            if (spSite.SiteSubscription != null)
                            {
                                job.SubscriptionId = spSite.SiteSubscription.Id;
                            }

                            job.Settings.OutputFormat = DeriveFileFormat(fileFormat);
                            job.Name = listItem.Name + "-" + Guid.NewGuid();

                            if (!string.IsNullOrEmpty(settings))
                            {
                                var splitSettings = settings.Split(';');

                                if (fileFormat.ToLower(CultureInfo.InvariantCulture) == splitSettings[0].Remove(0, 2).ToLower(CultureInfo.InvariantCulture))
                                {
                                    switch (fileFormat)
                                    {
                                        case "xps":
                                        case "pdf":
                                            {
                                                job.Settings.FixedFormatSettings.Bookmarks =
                                                    (FixedFormatBookmark)
                                                        Enum.Parse(typeof(FixedFormatBookmark), splitSettings[1].Remove(0, 2));
                                                job.Settings.FixedFormatSettings.BalloonState =
                                                    (BalloonState)
                                                        Enum.Parse(typeof(BalloonState), splitSettings[2].Remove(0, 2));

                                                if (splitSettings.Contains("BitmapEmbeddedFonts"))
                                                {
                                                    job.Settings.FixedFormatSettings.BitmapEmbeddedFonts = true;
                                                }

                                                if (splitSettings.Contains("IncludeDocumentProperties"))
                                                {
                                                    job.Settings.FixedFormatSettings.IncludeDocumentProperties = true;
                                                }

                                                if (splitSettings.Contains("IncludeDocumentStructure"))
                                                {
                                                    job.Settings.FixedFormatSettings.IncludeDocumentStructure = true;
                                                }

                                                if (splitSettings.Contains("OptimizeForMinimumSize"))
                                                {
                                                    job.Settings.FixedFormatSettings.OutputQuality = FixedFormatQuality.Minimum;
                                                }

                                                if (splitSettings.Contains("UsePdfA"))
                                                {
                                                    job.Settings.FixedFormatSettings.UsePDFA = true;
                                                }
                                                break;
                                            }
                                            
                                        case "doc":
                                        case "docx":
                                        case "docm":
                                        case "dot":
                                        case "dotx":
                                        case "dotm":
                                            {
                                                job.Settings.CompatibilityMode = (CompatibilityMode)
                                                    Enum.Parse(typeof(CompatibilityMode),
                                                    splitSettings[1].Remove(0, 2));

                                                if (splitSettings.Contains("AddThumbnail"))
                                                {
                                                    job.Settings.AddThumbnail = true;
                                                }

                                                if (splitSettings.Contains("EmbedFonts"))
                                                {
                                                    job.Settings.EmbedFonts = true;
                                                }

                                                if (splitSettings.Contains("UpdateFields"))
                                                {
                                                    job.Settings.UpdateFields = true;
                                                }

                                                break;
                                            }
                                    }
                                }
                            }

                            try
                            {
                                if (folder == null)
                                {
                                    job.AddFile(listItemUri.ToString(), listItemLibraryUri + newFileName);
                                }
                                else
                                {
                                    job.AddFile(listItemUri.ToString(),
                                        string.Format("{0}/{1}/{2}", folder.ParentWeb.Url, folder.Url, newFileName));
                                }
                            }
                            catch (Exception exception)
                            {
                                if (wfService != null)
                                {
                                    Exceptions.CheckedOutException(exception, listItem, wfService, executionContext);
                                    return false;
                                }
                                Exceptions.CheckedOutException(exception, listItem, null, null);
                                return false;
                            }

                            job.Start();

                            if (wfService != null)
                            {
                                wfService.LogToHistoryList(executionContext.ContextGuid, SPWorkflowHistoryEventType.WorkflowCompleted,
                                    0, TimeSpan.Zero, "Information", "Conversion job queued for " + listItem.DisplayName, string.Empty);
                            }

                            return true;

                        #endregion
                        }
                    }
                    catch (SPException exception)
                    {
                        if (wfService != null)
                        {
                            Exceptions.SharePointException(exception, listItem, wfService, executionContext);
                            return false;
                        }
                        Exceptions.SharePointException(exception, listItem, null, null);
                        return false;
                    }
                    catch (InvalidOperationException exception)
                    {
                        if (wfService != null)
                        {
                            Exceptions.InvalidOperationException(exception, listItem, wfService, executionContext);
                            return false;
                        }
                        Exceptions.InvalidOperationException(exception, listItem, null, null);
                        return false;
                    }
                }
            }
        }

        public static bool ConvertFolder(SPFolder folderItem, string fileFormat, string location, bool isWorkflow, ActivityExecutionContext executionContext)
        {
            ISharePointService wfService = null;

            if (executionContext != null)
            {
                wfService = executionContext.GetService<ISharePointService>();
            }

            if (string.IsNullOrEmpty(location))
            {
                location = null;
            }

            using (SPSite spSite = new SPSite(location ?? SPContext.Current.Web.Url))
            {
                using (SPWeb spWeb = spSite.OpenWeb())
                {
                    try
                    {
                        var proxies =
                            SPServiceContext.GetContext(spSite).GetProxies(typeof(WordServiceApplicationProxy));

                        if (proxies.Any())
                        {
                            _proxy = proxies.First();
                        }
                        else
                        {
                            var exception = new SPException();
                            throw exception;
                        }

                        var job = new ConversionJob(_proxy.DisplayName) { UserToken = spSite.UserToken };

                        if (spSite.SiteSubscription != null)
                        {
                            job.SubscriptionId = spSite.SiteSubscription.Id;
                        }

                        job.Settings.OutputFormat = DeriveFileFormat(fileFormat);
                        job.Name = folderItem.Name + "-" + Guid.NewGuid();

                        if (string.IsNullOrEmpty(location))
                        {
                            job.AddFolder(folderItem, folderItem, true);
                        }
                        else
                        {
                            if (location.ToLower().Contains("http://"))
                            {
                                location = location.Remove(0, 7);
                            }
                            else if (location.ToLower().Contains("https://"))
                            {
                                location = location.Remove(0, 8);
                            }

                            var index = location.IndexOf('/');

                            if (index > 0)
                                location = location.Substring(index);

                            var list = spWeb.GetList(location);

                            try
                            {
                                var folder = list.Items.Add(list.RootFolder.ServerRelativeUrl, SPFileSystemObjectType.Folder,
                                    folderItem.Name);

                                folder["Title"] = folderItem.Name;
                                folder.Update();
                            }
                            catch (SPException)
                            {
                                //Folder already exists
                            }

                            var folder2 = list.RootFolder.SubFolders[folderItem.Name];

                            job.AddFolder(folderItem, folder2, true);
                        }

                        job.Start();

                        if (wfService != null)
                        {
                            wfService.LogToHistoryList(executionContext.ContextGuid, SPWorkflowHistoryEventType.WorkflowCompleted,
                                0, TimeSpan.Zero, "Information", "Conversion job queued for " + folderItem.Name, string.Empty);
                        }

                        return true;
                    }
                    catch (SPException exception)
                    {
                        SPDiagnosticsService.Local.WriteTrace(0, new SPDiagnosticsCategory("NaupliusWASStatus",
                            TraceSeverity.High, EventSeverity.Error), TraceSeverity.Unexpected,
                            "An unexpected error has occurred attempting to find the Word Automation Services Proxy", exception.StackTrace);

                        if (wfService != null)
                        {
                            wfService.LogToHistoryList(executionContext.ContextGuid, SPWorkflowHistoryEventType.WorkflowError,
                                0, TimeSpan.Zero, "Information", "An unexpected error has occurred attempting to find the" +
                                "Word Automation Services Proxy for " + folderItem.Name, exception.StackTrace);
                        }

                        return false;
                    }
                    catch (InvalidOperationException exception2)
                    {
                        SPDiagnosticsService.Local.WriteTrace(0, new SPDiagnosticsCategory("NaupliusWASStatus",
                            TraceSeverity.High, EventSeverity.Error), TraceSeverity.Unexpected,
                            "An unexpected error has occurred attempting to contact the Word Automation Services. Validate that the" +
                            "Word Automation Service is Started.", exception2.StackTrace);

                        if (wfService != null)
                        {
                            wfService.LogToHistoryList(executionContext.ContextGuid, SPWorkflowHistoryEventType.WorkflowError,
                                0, TimeSpan.Zero, "Information", "An unexpected error has occurred attempting to contact the " +
                                "Word Automation Services. Validate that the Word Automation Service is Started. Attempted to process file " +
                                folderItem.Name, exception2.StackTrace);
                        }

                        return false;
                    }
                }
            }
        }

        public static bool ConvertLibrary(SPList list, string fileFormat, bool isWorkflow, ActivityExecutionContext executionContext)
        {
            ISharePointService wfService = null;

            if (executionContext != null)
            {
                wfService = executionContext.GetService<ISharePointService>();
            }

            using (SPSite spSite = new SPSite(list.ParentWeb.Site.Url))
            {
                using (SPWeb spWeb = spSite.OpenWeb())
                {
                    try
                    {
                        var proxies =
                            SPServiceContext.GetContext(spSite).GetProxies(typeof(WordServiceApplicationProxy));

                        if (proxies.Any())
                        {
                            _proxy = proxies.First();
                        }
                        else
                        {
                            var exception = new SPException();
                            throw exception;
                        }

                        var job = new ConversionJob(_proxy.DisplayName) { UserToken = spSite.UserToken };

                        if (spSite.SiteSubscription != null)
                        {
                            job.SubscriptionId = spSite.SiteSubscription.Id;
                        }

                        job.Settings.OutputFormat = DeriveFileFormat(fileFormat);
                        job.Name = list.Title + "-" + Guid.NewGuid();
                        job.AddLibrary(list, list);
                        job.Start();

                        if (wfService != null)
                        {
                            wfService.LogToHistoryList(executionContext.ContextGuid, SPWorkflowHistoryEventType.WorkflowCompleted,
                                0, TimeSpan.Zero, "Information", "Conversion job queued for " + list.Title, string.Empty);
                        }

                        return true;
                    }
                    catch (SPException exception)
                    {
                        SPDiagnosticsService.Local.WriteTrace(0, new SPDiagnosticsCategory("NaupliusWASStatus",
                            TraceSeverity.High, EventSeverity.Error), TraceSeverity.Unexpected,
                            "An unexpected error has occurred attempting to find the Word Automation Services Proxy", exception.StackTrace);

                        if (wfService != null)
                        {
                            wfService.LogToHistoryList(executionContext.ContextGuid, SPWorkflowHistoryEventType.WorkflowError,
                                0, TimeSpan.Zero, "Information", "An unexpected error has occurred attempting to find the" +
                                "Word Automation Services Proxy for " + list.Title, exception.StackTrace);
                        }

                        return false;
                    }
                    catch (InvalidOperationException exception2)
                    {
                        SPDiagnosticsService.Local.WriteTrace(0, new SPDiagnosticsCategory("NaupliusWASStatus",
                            TraceSeverity.High, EventSeverity.Error), TraceSeverity.Unexpected,
                            "An unexpected error has occurred attempting to contact the Word Automation Services. Validate that the" +
                            "Word Automation Service is Started.", exception2.StackTrace);

                        if (wfService != null)
                        {
                            wfService.LogToHistoryList(executionContext.ContextGuid, SPWorkflowHistoryEventType.WorkflowError,
                                0, TimeSpan.Zero, "Information", "An unexpected error has occurred attempting to contact the " +
                                "Word Automation Services. Validate that the Word Automation Service is Started. Attempted to process file " +
                                list.Title, exception2.StackTrace);
                        }

                        return false;
                    }
                }
            }
        }

        protected static SaveFormat DeriveFileFormat(string fileFormat)
        {
            switch (fileFormat)
            {
                case "pdf": return SaveFormat.PDF;
                case "xps": return SaveFormat.XPS;
                case "docx": return SaveFormat.Document;
                case "docm": return SaveFormat.DocumentMacroEnabled;
                case "dotx": return SaveFormat.Template;
                case "dotm": return SaveFormat.TemplateMacroEnabled;
                case "doc": return SaveFormat.Document97;
                case "dot": return SaveFormat.Template97;
                case "rtf": return SaveFormat.RTF;
                case "mhtml": return SaveFormat.MHTML;
                case "xml": return SaveFormat.XML;
            }

            return SaveFormat.Automatic;
        }
    }

    class Exceptions
    {
        internal static void SharePointException(SPException exception, SPListItem listItem, 
            ISharePointService wfService, ActivityExecutionContext executionContext)
        {
            SPDiagnosticsService.Local.WriteTrace(0, new SPDiagnosticsCategory("NaupliusWASStatus",
                TraceSeverity.High, EventSeverity.Error), TraceSeverity.Unexpected,
                "An unexpected error has occurred attempting to find the Word Automation Services Proxy", exception.StackTrace);

            if (wfService != null)
            {
                wfService.LogToHistoryList(executionContext.ContextGuid, SPWorkflowHistoryEventType.WorkflowError,
                    0, TimeSpan.Zero, "Information", "An unexpected error has occurred attempting to find the" +
                    "Word Automation Services Proxy for " + listItem.DisplayName, exception.StackTrace);
            }
        }

        internal static void InvalidOperationException(InvalidOperationException exception, SPListItem listItem, 
            ISharePointService wfService, ActivityExecutionContext executionContext)
        {
            SPDiagnosticsService.Local.WriteTrace(0, new SPDiagnosticsCategory("NaupliusWASStatus",
                TraceSeverity.High, EventSeverity.Error), TraceSeverity.Unexpected,
                "An unexpected error has occurred attempting to contact the Word Automation Services. Validate that the" +
                "Word Automation Service is Started.", exception.StackTrace);

            if (wfService != null)
            {
                wfService.LogToHistoryList(executionContext.ContextGuid, SPWorkflowHistoryEventType.WorkflowError,
                    0, TimeSpan.Zero, "Information", "An unexpected error has occurred attempting to contact the " +
                    "Word Automation Services. Validate that the Word Automation Service is Started. Attempted to process file " +
                    listItem.DisplayName, exception.StackTrace);
            }
        }

        internal static void CheckedOutException(Exception exception, SPListItem listItem,
            ISharePointService wfService, ActivityExecutionContext executionContext)
        {
            SPDiagnosticsService.Local.WriteTrace(0, new SPDiagnosticsCategory("NaupliusWASStatus",
                TraceSeverity.Medium, EventSeverity.Warning), TraceSeverity.Unexpected,
                "An exception occurred attempting conversion. Make sure a file with the same name is not currently Checked Out.", 
                exception.StackTrace);

            if (wfService != null)
            {
                wfService.LogToHistoryList(executionContext.ContextGuid, SPWorkflowHistoryEventType.WorkflowError,
                    0, TimeSpan.Zero, "Information", "An exception occurred attempting conversion. " +
                    "Make sure a file with the same name is not currently Checked Out. " +
                    listItem.DisplayName, exception.StackTrace);
            }
        }
    }
}