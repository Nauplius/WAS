using System;
using System.Linq;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Workflow;
using Microsoft.SharePoint.WorkflowActions;
using System.ComponentModel;
using System.Workflow.ComponentModel;
using System.Workflow.ComponentModel.Compiler;
using Nauplius.WAS.Layouts.Nauplius.WAS;

namespace Nauplius.WAS
{
    public partial class ConversionWorkflow : Activity
    {
        public ConversionWorkflow()
        {
            
        }
        
        public static DependencyProperty SourceItemProperty = DependencyProperty.Register("SourceItem",
                                                                                            typeof (string),
                                                                                            typeof (ConversionWorkflow));

        public static DependencyProperty DestFileProperty = DependencyProperty.Register("DestFile",
                                                                                           typeof(string), 
                                                                                           typeof(ConversionWorkflow));

        public static DependencyProperty FileTypeProperty = DependencyProperty.Register("FileType",
                                                                                        typeof (string),
                                                                                        typeof(ConversionWorkflow));

        public static DependencyProperty __ContextProperty = DependencyProperty.Register("__Context", 
                                                                                        typeof(WorkflowContext), 
                                                                                        typeof(ConversionWorkflow));

        [DescriptionAttribute("The source file to be converted")]
        [BrowsableAttribute(true)]
        [DesignerSerializationVisibilityAttribute(DesignerSerializationVisibility.Visible)]
        [ValidationOption(ValidationOption.Required)]
        public string SourceItem
        {
            get { return ((string)(GetValue(SourceItemProperty))); }
            set { SetValue(SourceItemProperty, value); }
        }

        [DescriptionAttribute("The destiniation file name")]
        [BrowsableAttribute(true)]
        [DesignerSerializationVisibilityAttribute(DesignerSerializationVisibility.Visible)]
        [ValidationOption(ValidationOption.Optional)]
        public string DestFile
        {
            get { return ((string)(GetValue(DestFileProperty))); }
            set { SetValue(DestFileProperty, value); }
        }

        [DescriptionAttribute("The file type to convert to")]
        [BrowsableAttribute(true)]
        [DesignerSerializationVisibilityAttribute(DesignerSerializationVisibility.Visible)]
        [ValidationOption(ValidationOption.Required)]
        public string FileType
        {
            get { return ((string)(GetValue(FileTypeProperty))); }
            set { SetValue(FileTypeProperty, value); }
        }

        [ValidationOption(ValidationOption.Required)]
        public WorkflowContext __Context
        {
            get
            {
                return (WorkflowContext)GetValue(__ContextProperty);
            }
            set
            {
                SetValue(__ContextProperty, value);
            }
        }

        protected override ActivityExecutionStatus Execute(ActivityExecutionContext executionContext)
        {
            var wfService = executionContext.GetService<ISharePointService>();

            using (SPSite site = new SPSite(__Context.Site.Url))
            {
                using (SPWeb web = site.OpenWeb())
                {
                    var item = web.GetListItem(__Context.CurrentItemUrl);

                    if (item.FileSystemObjectType == SPFileSystemObjectType.File)
                    {
                        if (Conversion.ValidateFileFormat(item))
                        {
                            wfService.LogToHistoryList(executionContext.ContextGuid, SPWorkflowHistoryEventType.WorkflowStarted,
                                0, TimeSpan.Zero, "Information", "Started conversion workflow for " + SourceItem, string.Empty);
                            bool result = ExecConversion.ConvertDocument(item, FileType, DestFile, true, executionContext, __Context, "", false);

                            return result ? ActivityExecutionStatus.Closed : ActivityExecutionStatus.Faulting;
                        }
                        return ActivityExecutionStatus.Closed;
                    }
                }
            }
            return ActivityExecutionStatus.Closed;
        }
    }
}
