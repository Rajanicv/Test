using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using Microsoft.SharePoint.Client;
using PnP.PowerShell.CmdletHelpAttributes;
using PnP.PowerShell.Commands.Enums;

namespace PnP.PowerShell.Commands.Branding
{
    [Cmdlet(VerbsCommon.Get, "PnPJavaScriptLink")]
    [CmdletHelp("Returns all or a specific custom action(s) with location type ScriptLink", 
        Category = CmdletHelpCategory.Branding,
        OutputType = typeof(UserCustomAction),
        OutputTypeLink = "https://msdn.microsoft.com/en-us/library/microsoft.sharepoint.client.usercustomaction.aspx")]
    [CmdletExample(Code = "PS:> Get-PnPJavaScriptLink",
                   Remarks = "Returns all web scoped JavaScript links",
                   SortOrder = 1)]
    [CmdletExample(Code = "PS:> Get-PnPJavaScriptLink -Scope All",
                   Remarks = "Returns all web and site scoped JavaScript links",
                   SortOrder = 2)]
    [CmdletExample(Code = "PS:> Get-PnPJavaScriptLink -Scope Web",
                   Remarks = "Returns all Web scoped JavaScript links",
                   SortOrder = 3)]
    [CmdletExample(Code = "PS:> Get-PnPJavaScriptLink -Scope Site",
                   Remarks = "Returns all Site scoped JavaScript links",
                   SortOrder = 4)]
    [CmdletExample(Code = "PS:> Get-PnPJavaScriptLink -Name Test",
                   Remarks = "Returns the web scoped JavaScript link named Test",
                   SortOrder = 5)]
    public class GetJavaScriptLink : PnPWebCmdlet
    {
        [Parameter(Mandatory = false, ValueFromPipeline = true, Position = 0, HelpMessage = "Name of the Javascript link. Omit this parameter to retrieve all script links")]
        [Alias("Key")]
        public string Name = string.Empty;

        [Parameter(Mandatory = false, HelpMessage = "Scope of the action, either Web, Site or All to return both, defaults to Web")]
        public CustomActionScope Scope = CustomActionScope.Web;

        [Parameter(Mandatory = false, HelpMessage = "Switch parameter if an exception should be thrown if the requested JavaScriptLink does not exist (true) or if omitted, nothing will be returned in case the JavaScriptLink does not exist")]
        public SwitchParameter ThrowExceptionIfJavaScriptLinkNotFound;

        protected override void ExecuteCmdlet()
        {
            var actions = new List<UserCustomAction>();

            if (Scope == CustomActionScope.All || Scope == CustomActionScope.Web)
            {
                actions.AddRange(SelectedWeb.GetCustomActions().Where(c => c.Location == "ScriptLink"));
            }
            if (Scope == CustomActionScope.All || Scope == CustomActionScope.Site)
            {
                actions.AddRange(ClientContext.Site.GetCustomActions().Where(c => c.Location == "ScriptLink"));
            }

            if (!string.IsNullOrEmpty(Name))
            {
                var foundAction = actions.FirstOrDefault(x => x.Name == Name);
                if (foundAction != null || !ThrowExceptionIfJavaScriptLinkNotFound)
                {
                    WriteObject(foundAction, true);
                }
                else
                {
                    throw new PSArgumentException($"No JavaScriptLink found with the name '{Name}' within the scope '{Scope}'", "Name");
                }
            }
            else
            {
                WriteObject(actions, true);
            }
        }
    }
}