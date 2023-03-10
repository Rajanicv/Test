#if !SP2013
using System;
using System.Linq;
using System.Management.Automation;
using Microsoft.Online.SharePoint.TenantAdministration;
using Microsoft.SharePoint.Client;
using PnP.PowerShell.CmdletHelpAttributes;
using PnP.PowerShell.Commands.Base;
using PnP.PowerShell.Commands.Enums;
using System.Collections.Generic;

namespace PnP.PowerShell.Commands
{
    [Cmdlet(VerbsCommon.Get, "PnPTenantSite")]
    [CmdletHelp(@"Retrieve site information.", "Use this cmdlet to retrieve site information from your tenant administration.",
        Category = CmdletHelpCategory.TenantAdmin,
        SupportedPlatform = CmdletSupportedPlatform.SP2016 | CmdletSupportedPlatform.SP2019 | CmdletSupportedPlatform.Online,
        OutputType = typeof(Microsoft.Online.SharePoint.TenantAdministration.SiteProperties),
        OutputTypeLink = "https://msdn.microsoft.com/en-us/library/microsoft.online.sharepoint.tenantadministration.siteproperties.aspx")]
    [CmdletExample(Code = @"PS:> Get-PnPTenantSite", Remarks = "Returns all site collections", SortOrder = 1)]
    [CmdletExample(Code = @"PS:> Get-PnPTenantSite -Url http://tenant.sharepoint.com/sites/projects", Remarks = "Returns information about the project site", SortOrder = 2)]
    [CmdletExample(Code = @"PS:> Get-PnPTenantSite -Detailed", Remarks = "Returns all sites with the full details of these sites", SortOrder = 3)]
#if !ONPREMISES
    [CmdletExample(Code = @"PS:> Get-PnPTenantSite -IncludeOneDriveSites", Remarks = "Returns all sites including all OneDrive for Business sites", SortOrder = 4)]
    [CmdletExample(Code = @"PS:> Get-PnPTenantSite -IncludeOneDriveSites -Filter ""Url -like '-my.sharepoint.com/personal/'""", Remarks = "Returns all OneDrive for Business sites", SortOrder = 5)]
    [CmdletExample(Code = @"PS:> Get-PnPTenantSite -Template SITEPAGEPUBLISHING#0", Remarks = "Returns all Communication sites", SortOrder = 6)]
    [CmdletExample(Code = @"PS:> Get-PnPTenantSite -Filter ""Url -like 'sales'"" ", Remarks = "Returns all sites including 'sales' in the url", SortOrder = 7)]
#endif
    public class GetTenantSite : PnPAdminCmdlet
    {
        [Parameter(Mandatory = false, HelpMessage = "The URL of the site", Position = 0, ValueFromPipeline = true)]
        [Alias("Identity")]
        public string Url;

#if !ONPREMISES
        [Parameter(Mandatory = false, HelpMessage = @"By default, all sites will be returned. Specify a template value alike ""STS#0"" here to filter on the template")]
        public string Template;
#endif

        [Parameter(Mandatory = false, HelpMessage = "By default, not all returned attributes are populated. This switch populates all attributes. It can take several seconds to run. Without this, some attributes will show default values that may not be correct.")]
        public SwitchParameter Detailed;

#if !ONPREMISES
        [Parameter(Mandatory = false, HelpMessage = "By default, the OneDrives are not returned. This switch includes all OneDrives.")]
        public SwitchParameter IncludeOneDriveSites;

        [Obsolete]
        [Parameter(Mandatory = false, HelpMessage = "When the switch IncludeOneDriveSites is used, this switch ignores the question shown that the command can take a long time to execute")]
        public SwitchParameter Force;

        [Obsolete("Use Template")]
        [Parameter(Mandatory = false, HelpMessage = "Limit results to a specific web template name")]
        public string WebTemplate;

        [Parameter(Mandatory = false, HelpMessage = "Specifies the script block of the server-side filter to apply. See https://technet.microsoft.com/en-us/library/fp161380.aspx")]
        public string Filter;
#endif

        protected override void ExecuteCmdlet()
        {
            if (!string.IsNullOrEmpty(Url))
            {
                var list = Tenant.GetSitePropertiesByUrl(Url, Detailed);
                list.Context.Load(list);
                list.Context.ExecuteQueryRetry();
                WriteObject(list, true);
            }
            else
            {
#if !ONPREMISES
                SPOSitePropertiesEnumerableFilter filter = new SPOSitePropertiesEnumerableFilter()
                {
                    IncludePersonalSite = IncludeOneDriveSites.IsPresent ? PersonalSiteFilter.Include : PersonalSiteFilter.UseServerDefault,
                    IncludeDetail = Detailed,
#pragma warning disable CS0618 // Type or member is obsolete
                    Template = Template ?? WebTemplate,
#pragma warning restore CS0618 // Type or member is obsolete
                    Filter = Filter,
                };

#endif
                SPOSitePropertiesEnumerable sitesList = null;
                var sites = new List<SiteProperties>();
#if !ONPREMISES
                do
                {
                    sitesList = Tenant.GetSitePropertiesFromSharePointByFilters(filter);
                    Tenant.Context.Load(sitesList);
                    Tenant.Context.ExecuteQueryRetry();
                    sites.AddRange(sitesList.ToList());
                    filter.StartIndex = sitesList.NextStartIndexFromSharePoint;
                } while (!string.IsNullOrWhiteSpace(sitesList.NextStartIndexFromSharePoint));
#else
                sitesList = Tenant.GetSiteProperties(0, Detailed);
                Tenant.Context.Load(sitesList);
                Tenant.Context.ExecuteQueryRetry();
                sites.AddRange(sitesList.ToList());
#endif

#if !ONPREMISES
                if (Template != null)
                {
                    WriteObject(sites.Where(t => t.Template == Template).OrderBy(x => x.Url), true);
                }
                else
                {
                    WriteObject(sites.OrderBy(x => x.Url), true);
                }
#else
                WriteObject(sites.OrderBy(x => x.Url), true);
#endif
            }
        }
    }
}
#endif