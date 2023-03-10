#if !PNPPSCORE
using System;
using System.Management.Automation;
using Microsoft.SharePoint.Client;
using PnP.PowerShell.CmdletHelpAttributes;

namespace PnP.PowerShell.Commands.Branding
{
    [Cmdlet(VerbsLifecycle.Enable, "PnPResponsiveUI")]
    [CmdletHelp("Activates the PnP Response UI Add-on",
        "Enables the PnP Responsive UI implementation on a classic SharePoint Site", Category = CmdletHelpCategory.Branding)]
    [CmdletExample(
        Code = "PS:> Enable-PnPResponsiveUI",
        SortOrder = 1,
        Remarks = "Will upload a CSS file, a JavaScript file and adds a custom action to the root web of the current site collection, enabling the responsive UI on the site collection. The CSS and JavaScript files are located in the style library, in a folder called SP.Responsive.UI.")]    
    [Obsolete("The PnP responsive UI (classic view) has been deprecated.")]
    public class EnableResponsiveUI : PnPWebCmdlet
    {
        [Parameter(Mandatory = false, HelpMessage = "A full URL pointing to an infrastructure site. If specified, it will add a custom action pointing to the responsive UI JS code in that site.")]
        public string InfrastructureSiteUrl;

        protected override void ExecuteCmdlet()
        {
            var site = ClientContext.Site;
            site.EnableResponsiveUI(InfrastructureSiteUrl);
        }
    }
}
#endif