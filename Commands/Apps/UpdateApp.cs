#if !SP2013 && !SP2016
using OfficeDevPnP.Core.ALM;
using OfficeDevPnP.Core.Enums;
using PnP.PowerShell.CmdletHelpAttributes;
using PnP.PowerShell.Commands.Base.PipeBinds;
using System;
using System.Management.Automation;

namespace PnP.PowerShell.Commands.Apps
{
    [Cmdlet(VerbsData.Update, "PnPApp")]
    [CmdletHelp("Updates an available app from the app catalog",
        Category = CmdletHelpCategory.Apps, SupportedPlatform = CmdletSupportedPlatform.Online | CmdletSupportedPlatform.SP2019)]
    [CmdletExample(
        Code = @"PS:> Update-PnPApp -Identity 99a00f6e-fb81-4dc7-8eac-e09c6f9132fe",
        Remarks = @"This will update an already installed app if a new version is available in the tenant app catalog. Retrieve a list all available apps and the installed and available versions with Get-PnPApp",
        SortOrder = 1)]
    [CmdletExample(
        Code = @"PS:> Update-PnPApp -Identity 99a00f6e-fb81-4dc7-8eac-e09c6f9132fe -Scope Site",
        Remarks = @"This will update an already installed app if a new version is available in the site collection app catalog. Retrieve a list all available apps and the installed and available versions with Get-PnPApp -Scope Site",
        SortOrder = 2)]
    public class UpdateApp : PnPSharePointCmdlet
    {
        [Parameter(Mandatory = true, Position = 0, ValueFromPipeline = true, HelpMessage = "Specifies the Id or an actual app metadata instance")]
        public AppMetadataPipeBind Identity;

        [Parameter(Mandatory = false, HelpMessage = "Defines which app catalog to use. Defaults to Tenant")]
        public AppCatalogScope Scope = AppCatalogScope.Tenant;

        protected override void ExecuteCmdlet()
        {
            var manager = new AppManager(ClientContext);
            var app = Identity.GetAppMetadata(ClientContext, Scope);
            if (app != null)
            {
                manager.Upgrade(Identity.Id, Scope);
            }
            else
            {
                throw new Exception("Cannot find app");
            }
        }
    }
}
#endif