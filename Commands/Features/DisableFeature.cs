using System;
using System.Management.Automation;
using Microsoft.SharePoint.Client;
using PnP.PowerShell.CmdletHelpAttributes;
using PnP.PowerShell.Commands.Base.PipeBinds;
using PnP.PowerShell.Commands.Enums;

namespace PnP.PowerShell.Commands.Features
{
    [Cmdlet(VerbsLifecycle.Disable, "PnPFeature", SupportsShouldProcess = false)]
    [CmdletHelp("Disables a feature",
        "Deactivates a feature that was active on a site",
        Category = CmdletHelpCategory.Features)]
    [CmdletExample(
        Code = "PS:> Disable-PnPFeature -Identity 99a00f6e-fb81-4dc7-8eac-e09c6f9132fe",
        Remarks = @"This will disable the feature with the id ""99a00f6e-fb81-4dc7-8eac-e09c6f9132fe""",
        SortOrder = 1)]
    [CmdletExample(
        Code = "PS:> Disable-PnPFeature -Identity 99a00f6e-fb81-4dc7-8eac-e09c6f9132fe -Force",
        Remarks = @"This will disable the feature with the id ""99a00f6e-fb81-4dc7-8eac-e09c6f9132fe"" with force.",
        SortOrder = 2)]
    [CmdletExample(
        Code = "PS:> Disable-PnPFeature -Identity 99a00f6e-fb81-4dc7-8eac-e09c6f9132fe -Scope Web",
        Remarks = @"This will disable the feature with the id ""99a00f6e-fb81-4dc7-8eac-e09c6f9132fe"" with the web scope.",
        SortOrder = 3)]
    public class DisableFeature : PnPWebCmdlet
    {
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = ParameterAttribute.AllParameterSets, HelpMessage = "The id of the feature to disable.")]
        public GuidPipeBind Identity;

        [Parameter(Mandatory = false, ParameterSetName = ParameterAttribute.AllParameterSets, HelpMessage = "Specifies whether to continue if an error occurs when deactivating the feature.")]
        public SwitchParameter Force;

        [Parameter(Mandatory = false, HelpMessage = "Specify the scope of the feature to deactivate, either Web or Site. Defaults to Web.")]
        public FeatureScope Scope = FeatureScope.Web;

        protected override void ExecuteCmdlet()
        {
            Guid featureId = Identity.Id;

            if (Scope == FeatureScope.Web)
            {
                SelectedWeb.DeactivateFeature(featureId);
            }
            else
            {
                ClientContext.Site.DeactivateFeature(featureId);
            }
        }
    }
}
