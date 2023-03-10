using System.Management.Automation;
using Microsoft.SharePoint.Client;
using PnP.PowerShell.CmdletHelpAttributes;

namespace PnP.PowerShell.Commands.Site
{
    [Cmdlet(VerbsCommon.Get, "PnPAuditing")]
    [CmdletHelp("Get the Auditing setting of a site",
        Category = CmdletHelpCategory.Sites,
        OutputType = typeof(Audit),
        OutputTypeLink = "https://msdn.microsoft.com/en-us/library/microsoft.sharepoint.client.audit.aspx")]
    [CmdletExample(
        Code = @"PS:> Get-PnPAuditing",
        Remarks = "Gets the auditing settings of the current site",
        SortOrder = 1)]
    public class GetAuditing : PnPSharePointCmdlet
    {
        protected override void ExecuteCmdlet()
        {
            var audit = ClientContext.Site.Audit;
            ClientContext.Load(audit);
            ClientContext.ExecuteQueryRetry();
            WriteObject(audit);
        }
    }
}
