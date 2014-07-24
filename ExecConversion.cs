using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.Office.Word.Server.Conversions;
using Microsoft.Office.Word.Server.Service;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Administration;
using Microsoft.SharePoint.Workflow;
using System;
using System.Linq;
using System.Workflow.ComponentModel;
using Microsoft.SharePoint.WorkflowActions;

namespace Nauplius.WAS
{
    public class ExecConversion
    {
        private static SPServiceApplicationProxy _proxy;

        public static bool ConvertDocument(SPListItem listItem, string fileFormat, string newFileName, bool isWorkflow,
            ActivityExecutionContext executionContext, WorkflowContext __Context, string location, bool deleteSource)
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
                        else
                        {
                            newFileName = string.Format("{0}.{1}", newFileName, fileFormat);                            
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

                        ConversionJob job;
                        if (isWorkflow)
                        {
                            job = new ConversionJob(_proxy.DisplayName) {UserToken = __Context.InitiatorUser.UserToken};
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

                        //AddFile must be last step prior to submitting job
                        if (string.IsNullOrEmpty(location))
                        {
                            job.AddFile(listItemUri.ToString(), listItemLibraryUri + newFileName);
                        }
                        else
                        {
                            job.AddFile(listItemUri.ToString(), location + "/" + newFileName);
                        }

                        if (deleteSource)
                        {
                            //not reachable
                            job.Start();
                        }
                        else
                        {
                            job.Start();
                        }


                        if (wfService != null)
                        {
                            wfService.LogToHistoryList(executionContext.ContextGuid, SPWorkflowHistoryEventType.WorkflowCompleted,
                                0, TimeSpan.Zero, "Information", "Conversion job queued for " + listItem.DisplayName, string.Empty);
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
                                "Word Automation Services Proxy for " + listItem.DisplayName, exception.StackTrace);
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
                                "Word Automation Services. Validate that the Word Automation Service is Started. Attempted to process file "  +
                                listItem.DisplayName, exception2.StackTrace);
                        }

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
                            SPServiceContext.GetContext(spSite).GetProxies(typeof (WordServiceApplicationProxy));

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
                case "pdf" : return SaveFormat.PDF;
                case "xps" : return SaveFormat.XPS;
                case "docx" : return SaveFormat.Document;
                case "docm" : return SaveFormat.DocumentMacroEnabled;
                case "dotx" : return SaveFormat.Template;
                case "dotm" : return SaveFormat.TemplateMacroEnabled;
                case "doc" : return SaveFormat.Document97;
                case "dot" : return SaveFormat.Template97;
                case "rtf" : return SaveFormat.RTF;
                case "mhtml" : return SaveFormat.MHTML;
                case "xml" : return SaveFormat.XML;
            }

            return SaveFormat.Automatic;
        }
    }
}