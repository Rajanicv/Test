using PnP.PowerShell.CmdletHelpAttributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;

namespace PnP.PowerShell.Commands
{
    [Cmdlet(VerbsCommon.Get, "PnPTimeZoneId")]
    [CmdletHelp("Returns a time zone ID",
        "In order to create a new classic site you need to specify the timezone this site will use. Use the cmdlet to retrieve a list of possible values.",
         Category = CmdletHelpCategory.TenantAdmin,
         OutputType = typeof(IEnumerable<Zone>),
         OutputTypeDescription =
             "Returns a list of matching zones. Use the ID property of the object for use in New-SPOTenantSite")]
    [CmdletExample(Code = @"PS:> Get-PnPTimeZoneId",
         Remarks = @"This will return all time zone IDs in use by Office 365.", SortOrder = 1)]
    [CmdletExample(Code = @"PS:> Get-PnPTimeZoneId -Match Stockholm",
         Remarks = @"This will return the time zone IDs for Stockholm", SortOrder = 2)]
    public class GetTimeZoneId : PSCmdlet
    {
        [Parameter(Mandatory = false, Position = 0, HelpMessage = "A string to search for like 'Stockholm'")]
        public string Match;

        protected override void ProcessRecord()
        {
            if (Match != null)
            {
                WriteObject(FindZone(Match));
            }
            else
            {
                WriteObject(AllZones());
            }
        }

        private IEnumerable<Zone> FindZone(string match)
        {
            var zones = AllZones();

            var results =
                zones.Where(
                    x =>
                        x.Description.ToLower().IndexOf(match.ToLower(), StringComparison.Ordinal) > -1 ||
                        x.Identifier.ToLower().Contains(match.ToLower()));

            return results;
        }



        public IEnumerable<Zone> AllZones()
        {
            foreach (var zone in Enum.GetValues(typeof(OfficeDevPnP.Core.Enums.TimeZone)))
            {
                var description = zone.ToString();
                var identifier = description.Split('_')[0];
                identifier = identifier.Replace("PLUS", "+").Replace("MINUS", "-");
                if (identifier.Length > 3)
                {
                    identifier = identifier.Substring(0, identifier.Length - 2) + ":" +
                                 identifier.Substring(identifier.Length - 2, 2);
                }

                description = description.Substring(description.IndexOf('_') + 1).Replace("_", " ");

                yield return new Zone((int)zone, identifier, description);

            }
        }

        public class Zone
        {
            public int Id { get; set; }
            public string Description { get; set; }
            public string Identifier { get; set; }

            public Zone(int id, string identifier, string description)
            {
                Id = id;
                Identifier = identifier;
                Description = description;
            }
        }
    }
}
