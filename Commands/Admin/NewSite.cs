#if !ONPREMISES
using System.Management.Automation;
using Microsoft.SharePoint.Client;
using PnP.PowerShell.CmdletHelpAttributes;
using PnP.PowerShell.Commands.Base.PipeBinds;
using PnP.PowerShell.Commands.Enums;
using System;
using PnP.PowerShell.Commands.Base;

namespace PnP.PowerShell.Commands
{
    [Cmdlet(VerbsCommon.New, "PnPSite")]
    [CmdletHelp("Creates either a communication site or a Microsoft 365 group-connected team site",
        "The New-PnPSite cmdlet creates a new site collection for the current tenant. Currently only 'modern' sites like Communication Site and the Modern Microsoft 365 group-connected team sites are supported. If you want to create a classic site, use New-PnPTenantSite.",
        OutputType = typeof(string),
        OutputTypeDescription = "Returns the url of the newly created site collection",
        Category = CmdletHelpCategory.TenantAdmin, SupportedPlatform = CmdletSupportedPlatform.Online)]
    [CmdletExample(
        Code = @"PS:> New-PnPSite -Type CommunicationSite -Title Contoso -Url https://tenant.sharepoint.com/sites/contoso",
        Remarks = @"This will create a new Communications Site collection with the title 'Contoso' and the url 'https://tenant.sharepoint.com/sites/contoso'",
        SortOrder = 1)]
    [CmdletExample(
        Code = @"PS:> New-PnPSite -Type CommunicationSite -Title Contoso -Url https://tenant.sharepoint.com/sites/contoso -SiteDesign Showcase",
        Remarks = @"This will create a new Communications Site collection with the title 'Contoso' and the url 'https://tenant.sharepoint.com/sites/contoso'. It will use the 'Showcase' design for the site.",
        SortOrder = 2)]
    [CmdletExample(
        Code = @"PS:> New-PnPSite -Type CommunicationSite -Title Contoso -Url https://tenant.sharepoint.com/sites/contoso -SiteDesignId ae2349d5-97d6-4440-94d1-6516b72449ac",
        Remarks = @"This will create a new Communications Site collection with the title 'Contoso' and the url 'https://tenant.sharepoint.com/sites/contoso'. It will use the specified custom site design for the site.",
        SortOrder = 3)]
    [CmdletExample(
        Code = @"PS:> New-PnPSite -Type CommunicationSite -Title Contoso -Url https://tenant.sharepoint.com/sites/contoso -Classification ""HBI""",
        Remarks = @"This will create a new Communications Site collection with the title 'Contoso' and the url 'https://tenant.sharepoint.com/sites/contoso'. The classification for the site will be set to ""HBI""",
        SortOrder = 4)]
    [CmdletExample(
        Code = @"PS:> New-PnPSite -Type CommunicationSite -Title Contoso -Url https://tenant.sharepoint.com/sites/contoso -ShareByEmailEnabled",
        Remarks = @"This will create a new Communications Site collection with the title 'Contoso' and the url 'https://tenant.sharepoint.com/sites/contoso'. Allows owners to invite users outside of the organization.",
        SortOrder = 5)]
    [CmdletExample(
        Code = @"PS:> New-PnPSite -Type CommunicationSite -Title Contoso -Url https://tenant.sharepoint.com/sites/contoso -Lcid 1040",
        Remarks = @"This will create a new Communications Site collection with the title 'Contoso' and the url 'https://tenant.sharepoint.com/sites/contoso' and sets the default language to Italian.",
        SortOrder = 6)]
    [CmdletExample(
        Code = @"PS:> New-PnPSite -Type TeamSite -Title 'Team Contoso' -Alias contoso",
        Remarks = @"This will create a new Modern Team Site collection with the title 'Team Contoso' and the url 'https://tenant.sharepoint.com/sites/contoso' or 'https://tenant.sharepoint.com/teams/contoso' based on the managed path configuration in the SharePoint Online Admin portal.",
        SortOrder = 7)]
    [CmdletExample(
        Code = @"PS:> New-PnPSite -Type TeamSite -Title 'Team Contoso' -Alias contoso -IsPublic",
        Remarks = @"This will create a new Modern Team Site collection with the title 'Team Contoso' and the url 'https://tenant.sharepoint.com/sites/contoso' or 'https://tenant.sharepoint.com/teams/contoso' based on the managed path configuration in the SharePoint Online Admin portal and sets the site to public.",
        SortOrder = 8)]
    [CmdletExample(
        Code = @"PS:> New-PnPSite -Type TeamSite -Title 'Team Contoso' -Alias contoso -Lcid 1040",
        Remarks = @"This will create a new Modern Team Site collection with the title 'Team Contoso' and the url 'https://tenant.sharepoint.com/sites/contoso' or 'https://tenant.sharepoint.com/teams/contoso' based on the managed path configuration in the SharePoint Online Admin portal and sets the default language of the site to Italian.",
        SortOrder = 9)]
    [CmdletAdditionalParameter(ParameterType = typeof(string), ParameterName = "Title", Mandatory = true, HelpMessage = @"Specifies the title of the new site collection", ParameterSetName = ParameterSet_COMMUNICATIONBUILTINDESIGN)]
    [CmdletAdditionalParameter(ParameterType = typeof(string), ParameterName = "Title", Mandatory = true, HelpMessage = @"Specifies the title of the new site collection", ParameterSetName = ParameterSet_COMMUNICATIONCUSTOMDESIGN)]
    [CmdletAdditionalParameter(ParameterType = typeof(string), ParameterName = "Url", Mandatory = true, HelpMessage = @"Specifies the full url of the new site collection, this parameter only applies to Communications Sites", ParameterSetName = ParameterSet_COMMUNICATIONBUILTINDESIGN)]
    [CmdletAdditionalParameter(ParameterType = typeof(string), ParameterName = "Url", Mandatory = true, HelpMessage = @"Specifies the full url of the new site collection, this parameter only applies to Communications Sites", ParameterSetName = ParameterSet_COMMUNICATIONCUSTOMDESIGN)]
    [CmdletAdditionalParameter(ParameterType = typeof(string), ParameterName = "Description", Mandatory = false, HelpMessage = @"Specifies the description of the new site collection", ParameterSetName = ParameterSet_COMMUNICATIONBUILTINDESIGN)]
    [CmdletAdditionalParameter(ParameterType = typeof(string), ParameterName = "Description", Mandatory = false, HelpMessage = @"Specifies the description of the new site collection", ParameterSetName = ParameterSet_COMMUNICATIONCUSTOMDESIGN)]
    [CmdletAdditionalParameter(ParameterType = typeof(string), ParameterName = "Classification", Mandatory = false, HelpMessage = @"Specifies the classification of the new site collection", ParameterSetName = ParameterSet_COMMUNICATIONBUILTINDESIGN)]
    [CmdletAdditionalParameter(ParameterType = typeof(string), ParameterName = "Classification", Mandatory = false, HelpMessage = @"Specifies the classification of the new site collection", ParameterSetName = ParameterSet_COMMUNICATIONCUSTOMDESIGN)]    
    [CmdletAdditionalParameter(ParameterType = typeof(string), ParameterName = "SensitivityLabel", Mandatory = false, HelpMessage = @"Specifies the sensitivity label of the new site collection", ParameterSetName = ParameterSet_COMMUNICATIONBUILTINDESIGN)]
    [CmdletAdditionalParameter(ParameterType = typeof(string), ParameterName = "SensitivityLabel", Mandatory = false, HelpMessage = @"Specifies the sensitivity label of the new site collection", ParameterSetName = ParameterSet_COMMUNICATIONCUSTOMDESIGN)]
    [CmdletAdditionalParameter(ParameterType = typeof(string), ParameterName = "Owner", Mandatory = false, HelpMessage = @"Specifies the owner of the site. Specify the value as a string array: ""user@domain.com""", ParameterSetName = ParameterSet_COMMUNICATIONBUILTINDESIGN)]
    [CmdletAdditionalParameter(ParameterType = typeof(string), ParameterName = "Owner", Mandatory = false, HelpMessage = @"Specifies the owner of the site. Specify the value as a string array: ""user@domain.com""", ParameterSetName = ParameterSet_COMMUNICATIONCUSTOMDESIGN)]
    [CmdletAdditionalParameter(ParameterType = typeof(OfficeDevPnP.Core.Enums.Office365Geography), ParameterName = "PreferredDataLocation", Mandatory = false, HelpMessage = @"Allows specifying in which geography the SharePoint site collection should be created. I.e. NAM, EUR, APC. For a full list of available regions, see https://docs.microsoft.com/office365/enterprise/multi-geo-add-group-with-pdl#geo-location-codes. Only supported on multi-geo enabled tenants.", ParameterSetName = ParameterSet_COMMUNICATIONBUILTINDESIGN)]
    [CmdletAdditionalParameter(ParameterType = typeof(OfficeDevPnP.Core.Enums.Office365Geography), ParameterName = "PreferredDataLocation", Mandatory = false, HelpMessage = @"Allows specifying in which geography the SharePoint site collection should be created. I.e. NAM, EUR, APC. For a full list of available regions, see https://docs.microsoft.com/office365/enterprise/multi-geo-add-group-with-pdl#geo-location-codes. Only supported on multi-geo enabled tenants.", ParameterSetName = ParameterSet_COMMUNICATIONCUSTOMDESIGN)]
    [CmdletAdditionalParameter(ParameterType = typeof(SwitchParameter), ParameterName = "AllowFileSharingForGuestUsers", Mandatory = false, HelpMessage = @"Specifies if guest users can share files in the new site collection", ParameterSetName = ParameterSet_COMMUNICATIONBUILTINDESIGN)]
    [CmdletAdditionalParameter(ParameterType = typeof(SwitchParameter), ParameterName = "AllowFileSharingForGuestUsers", Mandatory = false, HelpMessage = @"Specifies if guest users can share files in the new site collection", ParameterSetName = ParameterSet_COMMUNICATIONCUSTOMDESIGN)]
    [CmdletAdditionalParameter(ParameterType = typeof(OfficeDevPnP.Core.Sites.CommunicationSiteDesign), ParameterName = "SiteDesign", Mandatory = false, HelpMessage = @"Specifies the site design of the new site collection. Defaults to 'Topic'", ParameterSetName = ParameterSet_COMMUNICATIONBUILTINDESIGN)]
    [CmdletAdditionalParameter(ParameterType = typeof(GuidPipeBind), ParameterName = "SiteDesignId", Mandatory = true, HelpMessage = @"Specifies the site design id to use for the new site collection. If specified will override SiteDesign", ParameterSetName = ParameterSet_COMMUNICATIONCUSTOMDESIGN)]
    [CmdletAdditionalParameter(ParameterType = typeof(uint), ParameterName = "Lcid", Mandatory = false, HelpMessage = @"Specifies the language of the new site collection. Defaults to the current language of the web connected to. For more information, see Locale IDs supported by SharePoint at https://github.com/pnp/PnP-PowerShell/wiki/Supported-LCIDs-by-SharePoint. To get the list of supported languages on a SharePoint environment use: Get-PnPAvailableLanguage.", ParameterSetName = ParameterSet_COMMUNICATIONBUILTINDESIGN)]
    [CmdletAdditionalParameter(ParameterType = typeof(uint), ParameterName = "Lcid", Mandatory = false, HelpMessage = @"Specifies the language of the new site collection. Defaults to the current language of the web connected to. For more information, see Locale IDs supported by SharePoint at https://github.com/pnp/PnP-PowerShell/wiki/Supported-LCIDs-by-SharePoint. To get the list of supported languages on a SharePoint environment use: Get-PnPAvailableLanguage.", ParameterSetName = ParameterSet_COMMUNICATIONCUSTOMDESIGN)]
    [CmdletAdditionalParameter(ParameterType = typeof(uint), ParameterName = "Lcid", Mandatory = false, HelpMessage = @"Specifies the language of the new site collection. Defaults to the current language of the web connected to. For more information, see Locale IDs supported by SharePoint at https://github.com/pnp/PnP-PowerShell/wiki/Supported-LCIDs-by-SharePoint. To get the list of supported languages on a SharePoint environment use: Get-PnPAvailableLanguage.", ParameterSetName = ParameterSet_TEAM)]
    [CmdletAdditionalParameter(ParameterType = typeof(string), ParameterName = "Title", Mandatory = true, HelpMessage = @"Specifies the title of the new site collection", ParameterSetName = ParameterSet_TEAM)]
    [CmdletAdditionalParameter(ParameterType = typeof(string), ParameterName = "Alias", Mandatory = true, HelpMessage = @"Specifies the alias of the new site collection which represents the part of the URL that will be assigned to the site behind 'https://tenant.sharepoint.com/sites/' or 'https://tenant.sharepoint.com/teams/' based on the managed path configuration in the SharePoint Online Admin portal, this parameter only applies to Modern Team Sites", ParameterSetName = ParameterSet_TEAM)]
    [CmdletAdditionalParameter(ParameterType = typeof(string), ParameterName = "Description", Mandatory = false, HelpMessage = @"Specifies the description of the new site collection", ParameterSetName = ParameterSet_TEAM)]
    [CmdletAdditionalParameter(ParameterType = typeof(string), ParameterName = "Classification", Mandatory = false, HelpMessage = @"Specifies the classification of the new site collection", ParameterSetName = ParameterSet_TEAM)]
    [CmdletAdditionalParameter(ParameterType = typeof(string), ParameterName = "SensitivityLabel", Mandatory = false, HelpMessage = @"Specifies the sensitivity label of the new site collection", ParameterSetName = ParameterSet_TEAM)]
    [CmdletAdditionalParameter(ParameterType = typeof(SwitchParameter), ParameterName = "IsPublic", Mandatory = false, HelpMessage = @"Specifies if the Office 365 Group should be public. Defaults to private.", ParameterSetName = ParameterSet_TEAM)]
    [CmdletAdditionalParameter(ParameterType = typeof(string[]), ParameterName = "Owners", Mandatory = false, HelpMessage = @"Specifies the owners of the site. Specify the value as a string array: ""user@domain.com"",""anotheruser@domain.com""", ParameterSetName = ParameterSet_TEAM)]
    [CmdletAdditionalParameter(ParameterType = typeof(OfficeDevPnP.Core.Enums.Office365Geography), ParameterName = "PreferredDataLocation", Mandatory = false, HelpMessage = @"Allows specifying in which geography the SharePoint site collection should be created. I.e. NAM, EUR, APC. For a full list of available regions, see https://docs.microsoft.com/office365/enterprise/multi-geo-add-group-with-pdl#geo-location-codes. Only supported on multi-geo enabled tenants.", ParameterSetName = ParameterSet_TEAM)]
    public class NewSite : PnPSharePointCmdlet, IDynamicParameters
    {
        private const string ParameterSet_COMMUNICATIONBUILTINDESIGN = "Communication Site with Built-In Site Design";
        private const string ParameterSet_COMMUNICATIONCUSTOMDESIGN = "Communication Site with Custom Design";
        private const string ParameterSet_TEAM = "Team Site";

        [Parameter(Mandatory = true, HelpMessage = "Specifies with type of site to create.")]
        public SiteType Type;

        [Parameter(Mandatory = false, HelpMessage = "If specified the site will be associated to the hubsite as identified by this id")]
        public GuidPipeBind HubSiteId;

        private CommunicationSiteParameters _communicationSiteParameters;
        private TeamSiteParameters _teamSiteParameters;

        [Parameter(Mandatory = false, HelpMessage = "If specified the cmdlet will wait until the site has been fully created and all site artifacts have been provisioned by SharePoint. Notice that this can take a while.")]
        public SwitchParameter Wait;

        public object GetDynamicParameters()
        {
            switch (Type)
            {
                case SiteType.CommunicationSite:
                    {
                        _communicationSiteParameters = new CommunicationSiteParameters();
                        return _communicationSiteParameters;
                    }
                case SiteType.TeamSite:
                    {
                        _teamSiteParameters = new TeamSiteParameters();
                        return _teamSiteParameters;
                    }
            }
            return null;
        }

        protected override void ExecuteCmdlet()
        {
            if (Type == SiteType.CommunicationSite)
            {
                if (!ParameterSpecified("Lcid"))
                {
                    ClientContext.Web.EnsureProperty(w => w.Language);
                    _communicationSiteParameters.Lcid = ClientContext.Web.Language;
                }
                var creationInformation = new OfficeDevPnP.Core.Sites.CommunicationSiteCollectionCreationInformation();
                creationInformation.Title = _communicationSiteParameters.Title;
                creationInformation.Url = _communicationSiteParameters.Url;
                creationInformation.Description = _communicationSiteParameters.Description;
                creationInformation.Classification = _communicationSiteParameters.Classification;
#pragma warning disable CS0618 // Type or member is obsolete
                creationInformation.ShareByEmailEnabled = _communicationSiteParameters.AllowFileSharingForGuestUsers || _communicationSiteParameters.ShareByEmailEnabled;
#pragma warning restore CS0618 // Type or member is obsolete
                creationInformation.Lcid = _communicationSiteParameters.Lcid;
                if (ParameterSpecified(nameof(HubSiteId)))
                {
                    creationInformation.HubSiteId = HubSiteId.Id;
                }
                if (ParameterSetName == ParameterSet_COMMUNICATIONCUSTOMDESIGN)
                {
                    creationInformation.SiteDesignId = _communicationSiteParameters.SiteDesignId.Id;
                }
                else
                {
                    creationInformation.SiteDesign = _communicationSiteParameters.SiteDesign;
                }
                creationInformation.Owner = _communicationSiteParameters.Owner;
                creationInformation.PreferredDataLocation = _communicationSiteParameters.PreferredDataLocation;
                creationInformation.SensitivityLabel = _communicationSiteParameters.SensitivityLabel;

                var returnedContext = OfficeDevPnP.Core.Sites.SiteCollection.Create(ClientContext, creationInformation, noWait: !Wait);
                //var results = ClientContext.CreateSiteAsync(creationInformation);
                //var returnedContext = results.GetAwaiter().GetResult();
                WriteObject(returnedContext.Url);
            }
            else
            {
                var creationInformation = new OfficeDevPnP.Core.Sites.TeamSiteCollectionCreationInformation();
                creationInformation.DisplayName = _teamSiteParameters.Title;
                creationInformation.Alias = _teamSiteParameters.Alias;
                creationInformation.Classification = _teamSiteParameters.Classification;
                creationInformation.Description = _teamSiteParameters.Description;
                creationInformation.IsPublic = _teamSiteParameters.IsPublic;
                creationInformation.Lcid = _teamSiteParameters.Lcid;
                if (ParameterSpecified(nameof(HubSiteId)))
                {
                    creationInformation.HubSiteId = HubSiteId.Id;
                }
                creationInformation.Owners = _teamSiteParameters.Owners;
                creationInformation.PreferredDataLocation = _teamSiteParameters.PreferredDataLocation;
                creationInformation.SensitivityLabel = _teamSiteParameters.SensitivityLabel;

                var returnedContext = OfficeDevPnP.Core.Sites.SiteCollection.Create(ClientContext, creationInformation, noWait: !Wait, graphAccessToken: PnPConnection.CurrentConnection.TryGetAccessToken(TokenAudience.MicrosoftGraph));
                //var results = ClientContext.CreateSiteAsync(creationInformation);
                //var returnedContext = results.GetAwaiter().GetResult();
                WriteObject(returnedContext.Url);
            }
        }

        public class CommunicationSiteParameters
        {
            [Parameter(Mandatory = true, ParameterSetName = ParameterSet_COMMUNICATIONBUILTINDESIGN)]
            [Parameter(Mandatory = true, ParameterSetName = ParameterSet_COMMUNICATIONCUSTOMDESIGN)]
            public string Title;

            [Parameter(Mandatory = true, ParameterSetName = ParameterSet_COMMUNICATIONBUILTINDESIGN)]
            [Parameter(Mandatory = true, ParameterSetName = ParameterSet_COMMUNICATIONCUSTOMDESIGN)]
            public string Url;

            [Parameter(Mandatory = false, ParameterSetName = ParameterSet_COMMUNICATIONBUILTINDESIGN)]
            [Parameter(Mandatory = false, ParameterSetName = ParameterSet_COMMUNICATIONCUSTOMDESIGN)]
            public string Description;

            [Parameter(Mandatory = false, ParameterSetName = ParameterSet_COMMUNICATIONBUILTINDESIGN)]
            [Parameter(Mandatory = false, ParameterSetName = ParameterSet_COMMUNICATIONCUSTOMDESIGN)]
            public string Classification;

            [Parameter(Mandatory = false, ParameterSetName = ParameterSet_COMMUNICATIONBUILTINDESIGN)]
            [Parameter(Mandatory = false, ParameterSetName = ParameterSet_COMMUNICATIONCUSTOMDESIGN)]
            [Obsolete("Use ShareByEmailEnabled instead.")]
            public SwitchParameter AllowFileSharingForGuestUsers;

            [Parameter(Mandatory = false, ParameterSetName = ParameterSet_COMMUNICATIONBUILTINDESIGN)]
            [Parameter(Mandatory = false, ParameterSetName = ParameterSet_COMMUNICATIONCUSTOMDESIGN)]
            public SwitchParameter ShareByEmailEnabled;

            [Parameter(Mandatory = false, ParameterSetName = ParameterSet_COMMUNICATIONBUILTINDESIGN)]
            public OfficeDevPnP.Core.Sites.CommunicationSiteDesign SiteDesign = OfficeDevPnP.Core.Sites.CommunicationSiteDesign.Topic;

            [Parameter(Mandatory = true, ParameterSetName = ParameterSet_COMMUNICATIONCUSTOMDESIGN)]
            public GuidPipeBind SiteDesignId;

            [Parameter(Mandatory = false, ParameterSetName = ParameterSet_COMMUNICATIONBUILTINDESIGN)]
            [Parameter(Mandatory = false, ParameterSetName = ParameterSet_COMMUNICATIONCUSTOMDESIGN)]
            public uint Lcid;

            [Parameter(Mandatory = false, ParameterSetName = ParameterSet_COMMUNICATIONBUILTINDESIGN)]
            [Parameter(Mandatory = false, ParameterSetName = ParameterSet_COMMUNICATIONCUSTOMDESIGN)]
            public string Owner;

            [Parameter(Mandatory = false, ParameterSetName = ParameterSet_COMMUNICATIONBUILTINDESIGN)]
            [Parameter(Mandatory = false, ParameterSetName = ParameterSet_COMMUNICATIONCUSTOMDESIGN)]
            public OfficeDevPnP.Core.Enums.Office365Geography? PreferredDataLocation;

            [Parameter(Mandatory = false, ParameterSetName = ParameterSet_COMMUNICATIONBUILTINDESIGN)]
            [Parameter(Mandatory = false, ParameterSetName = ParameterSet_COMMUNICATIONCUSTOMDESIGN)]
            public string SensitivityLabel;
        }

        public class TeamSiteParameters
        {
            [Parameter(Mandatory = true, ParameterSetName = ParameterSet_TEAM)]
            public string Title;

            [Parameter(Mandatory = true, ParameterSetName = ParameterSet_TEAM)]
            public string Alias;

            [Parameter(Mandatory = false, ParameterSetName = ParameterSet_TEAM)]
            public string Description;

            [Parameter(Mandatory = false, ParameterSetName = ParameterSet_TEAM)]
            public string Classification;

            [Parameter(Mandatory = false, ParameterSetName = ParameterSet_TEAM)]
            public SwitchParameter IsPublic;

            [Parameter(Mandatory = false, ParameterSetName = ParameterSet_TEAM)]
            public uint Lcid;

            [Parameter(Mandatory = false, ParameterSetName = ParameterSet_TEAM)]
            public string[] Owners;

            [Parameter(Mandatory = false, ParameterSetName = ParameterSet_TEAM)]
            public OfficeDevPnP.Core.Enums.Office365Geography? PreferredDataLocation;

            [Parameter(Mandatory = false, ParameterSetName = ParameterSet_TEAM)]
            public string SensitivityLabel;
        }
    }
}
#endif