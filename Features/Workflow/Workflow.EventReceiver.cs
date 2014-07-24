using System;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Administration;

namespace Nauplius.WAS._2013.Features.Workflow
{
    /// <summary>
    /// This class handles events raised during feature activation, deactivation, installation, uninstallation, and upgrade.
    /// </summary>
    /// <remarks>
    /// The GUID attached to this class may be used during packaging and should not be modified.
    /// </remarks>

    [Guid("3a3f7dd4-154b-4c74-aaa2-d8cb601f6a38")]
    public class WorkflowEventReceiver : SPFeatureReceiver
    {
        // Uncomment the method below to handle the event raised after a feature has been activated.

        public override void FeatureActivated(SPFeatureReceiverProperties properties)
        {
            var webApplication = properties.Feature.Parent as SPWebApplication;
            ModifyWebConfig(webApplication, false);
        }


        // Uncomment the method below to handle the event raised before a feature is deactivated.

        public override void FeatureDeactivating(SPFeatureReceiverProperties properties)
        {
            var webApplication = properties.Feature.Parent as SPWebApplication;
            ModifyWebConfig(webApplication, true);
        }


        // Uncomment the method below to handle the event raised after a feature has been installed.

        //public override void FeatureInstalled(SPFeatureReceiverProperties properties)
        //{
        //}


        // Uncomment the method below to handle the event raised before a feature is uninstalled.

        public override void FeatureUninstalling(SPFeatureReceiverProperties properties)
        {
            var webApplication = properties.Feature.Parent as SPWebApplication;
            ModifyWebConfig(webApplication, true);
        }

        // Uncomment the method below to handle the event raised when a feature is upgrading.

        //public override void FeatureUpgrading(SPFeatureReceiverProperties properties, string upgradeActionName, System.Collections.Generic.IDictionary<string, string> parameters)
        //{
        //}

        private static void ModifyWebConfig(SPWebApplication webApplication, bool remove)
        {
            const string assembly = @"Nauplius.WAS.2013, Version=1.0.0.0, Culture=neutral, PublicKeyToken=1458f7a17eac05e9";
            const string _namespace = @"Nauplius.WAS";

            var modification = new SPWebConfigModification
            {
                Owner = "Nauplius.WAS",
                Sequence = 0,
                Path = "configuration/System.Workflow.ComponentModel.WorkflowCompiler/authorizedTypes/targetFx[@version='v4.0']",
                Name =
                    string.Format(
                        "authorizedType[@Assembly='{0}'][@Namespace='{1}'][@TypeName='*'][@Authorized='True']", assembly,
                        _namespace),
                Value =
                    string.Format("<authorizedType Assembly='{0}' Namespace='{1}' TypeName='*' Authorized='True'/>",
                                  assembly, _namespace)
            };

            if (!remove)
            {
                webApplication.WebConfigModifications.Add(modification);
            }
            else
            {
                webApplication.WebConfigModifications.Remove(modification);
            }

            webApplication.Update();
            webApplication.Farm.Services.GetValue<SPWebService>().ApplyWebConfigModifications();
        }
    }
}
