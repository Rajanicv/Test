using System;
using System.Collections;
using System.Management.Automation;
using Microsoft.SharePoint.Client;
using Microsoft.SharePoint.Client.Search.Query;
using PnP.PowerShell.CmdletHelpAttributes;
using System.Collections.Generic;
using System.Linq;

namespace PnP.PowerShell.Commands.Search
{
    [Cmdlet(VerbsLifecycle.Submit, "PnPSearchQuery", DefaultParameterSetName = "Limit")]
    [Alias("Invoke-PnPSearchQuery")]
    [CmdletHelp("Executes an arbitrary search query against the SharePoint search index",
        Category = CmdletHelpCategory.Search,
        OutputType = typeof(List<dynamic>))]
    [CmdletExample(
        Code = @"PS:> Submit-PnPSearchQuery -Query ""finance""",
        Remarks = "Returns the top 500 items with the term finance",
        SortOrder = 1)]
    [CmdletExample(
        Code = @"PS:> Submit-PnPSearchQuery -Query ""Title:Intranet*"" -MaxResults 10",
        Remarks = "Returns the top 10 items indexed by SharePoint Search of which the title starts with the word Intranet",
        SortOrder = 2)]
    [CmdletExample(
        Code = @"PS:> Submit-PnPSearchQuery -Query ""Title:Intranet*"" -All",
        Remarks = "Returns absolutely all items indexed by SharePoint Search of which the title starts with the word Intranet",
        SortOrder = 3)]
    [CmdletExample(
        Code = @"PS:> Submit-PnPSearchQuery -Query ""Title:Intranet*"" -Refiners ""contentclass,FileType(filter=6/0/*)""",
        Remarks = "Returns absolutely all items indexed by SharePoint Search of which the title starts with the word Intranet, and return refiners for contentclass and FileType managed properties",
        SortOrder = 4)]
    public class SubmitSearchQuery : PnPWebCmdlet
    {
        [Parameter(Mandatory = true, ValueFromPipeline = true, Position = 0, HelpMessage = "Search query in Keyword Query Language (KQL).", ParameterSetName = ParameterAttribute.AllParameterSets)]
        public string Query = string.Empty;

        [Parameter(Mandatory = false, HelpMessage = "Search result item to start returning the results from. Useful for paging. Leave at 0 to return all results.", ParameterSetName = "Limit")]
        public int StartRow = 0;

        [Parameter(Mandatory = false, HelpMessage = "Maximum amount of search results to return. Default and max per page is 500 search results.", ParameterSetName = "Limit")]
        [ValidateRange(0, 500)]
        public int MaxResults = 500;

        [Parameter(Mandatory = false, HelpMessage = "Automatically page results until the end to get more than 500. Use with caution!", ParameterSetName = "All")]
        public SwitchParameter All;

        [Parameter(Mandatory = false, HelpMessage = "Specifies whether near duplicate items should be removed from the search results.", ParameterSetName = ParameterAttribute.AllParameterSets)]
        public bool TrimDuplicates = false;

        [Parameter(Mandatory = false, HelpMessage = "Extra query properties. Can for example be used for Office Graph queries.", ParameterSetName = ParameterAttribute.AllParameterSets)]
        public Hashtable Properties;

        [Parameter(Mandatory = false, HelpMessage = "The list of refiners to be returned in a search result.", ParameterSetName = ParameterAttribute.AllParameterSets)]
        public string Refiners;

        [Parameter(Mandatory = false, HelpMessage = "The locale for the query.", ParameterSetName = ParameterAttribute.AllParameterSets)]
        public int Culture;

        [Parameter(Mandatory = false, HelpMessage = "Specifies the query template that is used at run time to transform the query based on user input.", ParameterSetName = ParameterAttribute.AllParameterSets)]
        public string QueryTemplate;

        [Parameter(Mandatory = false, HelpMessage = "The list of properties to return in the search results.", ParameterSetName = ParameterAttribute.AllParameterSets)]
        public string[] SelectProperties;

        [Parameter(Mandatory = false, HelpMessage = "The set of refinement filters used.", ParameterSetName = ParameterAttribute.AllParameterSets)]
        public string[] RefinementFilters;

        [Parameter(Mandatory = false, HelpMessage = "The list of properties by which the search results are ordered.", ParameterSetName = ParameterAttribute.AllParameterSets)]
        public Hashtable SortList;

        [Parameter(Mandatory = false, HelpMessage = "The identifier (ID) of the ranking model to use for the query.", ParameterSetName = ParameterAttribute.AllParameterSets)]
        public string RankingModelId;

        [Parameter(Mandatory = false, HelpMessage = "Specifies the name of the client which issued the query.", ParameterSetName = ParameterAttribute.AllParameterSets)]
        public string ClientType = "PnP";

        [Parameter(Mandatory = false, HelpMessage = "Limit the number of items per the collapse specification. See https://docs.microsoft.com/en-us/sharepoint/dev/general-development/customizing-search-results-in-sharepoint#collapse-similar-search-results-using-the-collapsespecification-property for more information.", ParameterSetName = ParameterAttribute.AllParameterSets)]
        public string CollapseSpecification;

        [Parameter(Mandatory = false, HelpMessage = "The keyword query’s hidden constraints.", ParameterSetName = ParameterAttribute.AllParameterSets)]
        public string HiddenConstraints;

        [Parameter(Mandatory = false, HelpMessage = "The identifier for the search query time zone.", ParameterSetName = ParameterAttribute.AllParameterSets)]
        public int TimeZoneId;

        [Parameter(Mandatory = false, HelpMessage = "Specifies whether the phonetic forms of the query terms are used to find matches.", ParameterSetName = ParameterAttribute.AllParameterSets)]
        public bool EnablePhonetic;

        [Parameter(Mandatory = false, HelpMessage = "Specifies whether stemming is enabled.", ParameterSetName = ParameterAttribute.AllParameterSets)]
        public bool EnableStemming;

        [Parameter(Mandatory = false, HelpMessage = "Specifies whether Query Rules are enabled for this query.", ParameterSetName = ParameterAttribute.AllParameterSets)]
        public bool EnableQueryRules;

        [Parameter(Mandatory = false, HelpMessage = "Specifies the identifier (ID or name) of the result source to be used to run the query.", ParameterSetName = ParameterAttribute.AllParameterSets)]
        public Guid SourceId;

        [Parameter(Mandatory = false, HelpMessage = "Determines whether Best Bets are enabled.", ParameterSetName = ParameterAttribute.AllParameterSets)]
        public bool ProcessBestBets;

        [Parameter(Mandatory = false, HelpMessage = "Determines whether personal favorites data is processed or not.", ParameterSetName = ParameterAttribute.AllParameterSets)]
        public bool ProcessPersonalFavorites;

        [Parameter(Mandatory = false, HelpMessage = "Specifies whether only relevant results are returned", ParameterSetName = ParameterAttribute.AllParameterSets)]
        public SwitchParameter RelevantResults;

        internal IEnumerable<object> Run()
        {
            int startRow = StartRow;
            int rowLimit = MaxResults;
            // Used for internal Microsoft telemetry
            string clientFunction = "Search";
            if (All.IsPresent)
            {
                clientFunction = "Scanner";
                startRow = 0;
                rowLimit = 500;
                SortList = new Hashtable()
                    {
                        {"[DocId]", "ascending"}    // Special optimized sorting when iterating all items
                    };
            }

            var currentCount = 0;
            string lastDocId = "0";
            PnPResultTableCollection finalResults = null;
            do
            {
                KeywordQuery keywordQuery = CreateKeywordQuery(clientFunction);
                keywordQuery.StartRow = startRow;
                keywordQuery.RowLimit = rowLimit;

                if (All.IsPresent)
                {
                    if (currentCount != 0)
                    {
                        keywordQuery.Refiners = null; // Only need to set on first page for auto paging
                    }
                    keywordQuery.StartRow = 0;
                    keywordQuery.QueryText += " IndexDocId>" + lastDocId;
                }

                var searchExec = new SearchExecutor(ClientContext);
                var results = searchExec.ExecuteQuery(keywordQuery);
                ClientContext.ExecuteQueryRetry();

                if (results.Value != null)
                {
                    if (finalResults == null)
                    {
                        finalResults = (PnPResultTableCollection)results.Value;
                        foreach (ResultTable resultTable in results.Value)
                        {
                            if (resultTable.TableType == "RelevantResults")
                            {
                                currentCount = resultTable.RowCount;
                                lastDocId = resultTable.ResultRows.Last()["DocId"].ToString();
                            }
                        }
                    }
                    else
                    {
                        // we're in paging mode
                        foreach (ResultTable resultTable in results.Value)
                        {
                            PnPResultTable pnpResultTable = (PnPResultTable)resultTable;
                            var existingTable = finalResults.SingleOrDefault(t => t.TableType == resultTable.TableType);
                            if (existingTable != null)
                            {
                                existingTable.ResultRows.AddRange(pnpResultTable.ResultRows);
                            }
                            else
                            {
                                finalResults.Add(pnpResultTable);
                            }
                            if (pnpResultTable.TableType == "RelevantResults")
                            {
                                currentCount = resultTable.RowCount;
                                if (currentCount > 0)
                                {
                                    lastDocId = resultTable.ResultRows.Last()["DocId"].ToString();
                                }
                            }
                        }
                    }

                }
                startRow += rowLimit;
            } while (currentCount == rowLimit && All.IsPresent);

            if (!RelevantResults.IsPresent)
            {
                return finalResults;
            }
            else
            {
                var results = finalResults.FirstOrDefault(t => t.TableType == "RelevantResults")?
                    .ResultRows.Select(r => ConvertToPSObject(r));
                return results;
            }
        }

        protected override void ExecuteCmdlet()
        {
            var results = Run();
            WriteObject(results, true);
        }

        private object ConvertToPSObject(IDictionary<string, object> r)
        {
            PSObject res = new PSObject();
            if (r != null)
            {
                foreach (var kvp in r)
                {
                    res.Properties.Add(new PSNoteProperty(kvp.Key, kvp.Value));
                }
            }
            return res;
        }

        private KeywordQuery CreateKeywordQuery(string clientFunction)
        {
            var keywordQuery = new KeywordQuery(ClientContext);

            // Construct query to execute
            var query = "";
            if (!string.IsNullOrEmpty(Query))
            {
                query = Query;
            }

            keywordQuery.QueryText = query;
            keywordQuery.ClientType = ClientType;
            keywordQuery.TrimDuplicates = TrimDuplicates;
            if (ParameterSpecified(nameof(Refiners))) keywordQuery.Refiners = Refiners;
            if (ParameterSpecified(nameof(Culture))) keywordQuery.Culture = Culture;
            if (ParameterSpecified(nameof(QueryTemplate))) keywordQuery.QueryTemplate = QueryTemplate;
            if (ParameterSpecified(nameof(RankingModelId))) keywordQuery.RankingModelId = RankingModelId;
            if (ParameterSpecified(nameof(HiddenConstraints))) keywordQuery.HiddenConstraints = HiddenConstraints;
            if (ParameterSpecified(nameof(TimeZoneId))) keywordQuery.TimeZoneId = TimeZoneId;
            if (ParameterSpecified(nameof(EnablePhonetic))) keywordQuery.EnablePhonetic = EnablePhonetic;
            if (ParameterSpecified(nameof(EnableStemming))) keywordQuery.EnableStemming = EnableStemming;
            if (ParameterSpecified(nameof(EnableQueryRules))) keywordQuery.EnableQueryRules = EnableQueryRules;
            if (ParameterSpecified(nameof(SourceId))) keywordQuery.SourceId = SourceId;
            if (ParameterSpecified(nameof(ProcessBestBets))) keywordQuery.ProcessBestBets = ProcessBestBets;
            if (ParameterSpecified(nameof(ProcessPersonalFavorites))) keywordQuery.ProcessPersonalFavorites = ProcessPersonalFavorites;
            if (ParameterSpecified(nameof(CollapseSpecification))) keywordQuery.CollapseSpecification = CollapseSpecification;

            if (SortList != null)
            {
                var sortList = keywordQuery.SortList;
                sortList.Clear();
                foreach (string key in SortList.Keys)
                {
                    SortDirection sort = (SortDirection)Enum.Parse(typeof(SortDirection), SortList[key] as string, true);
                    sortList.Add(key, sort);
                }
            }
            if (SelectProperties != null)
            {
                keywordQuery.SelectProperties.Clear();
                var selectProperties = keywordQuery.SelectProperties;
                if (SelectProperties.Length == 1 && SelectProperties[0].Contains(","))
                {
                    SelectProperties = SelectProperties[0].Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                }

                foreach (string property in SelectProperties)
                {
                    selectProperties.Add(property.Trim());
                }
            }
            if (RefinementFilters != null)
            {
                var refinementFilters = keywordQuery.RefinementFilters;
                refinementFilters.Clear();
                foreach (string property in RefinementFilters)
                {
                    refinementFilters.Add(property);
                }
            }
            if (Properties != null)
            {
                foreach (string key in Properties.Keys)
                {
                    QueryPropertyValue propVal = new QueryPropertyValue();
                    var value = Properties[key];
                    if (value is string)
                    {
                        propVal.StrVal = (string)value;
                        propVal.QueryPropertyValueTypeIndex = 1;
                    }
                    else if (value is int)
                    {
                        propVal.IntVal = (int)value;
                        propVal.QueryPropertyValueTypeIndex = 2;
                    }
                    else if (value is bool)
                    {
                        propVal.BoolVal = (bool)value;
                        propVal.QueryPropertyValueTypeIndex = 3;
                    }
                    else if (value is string[])
                    {
                        propVal.StrArray = (string[])value;
                        propVal.QueryPropertyValueTypeIndex = 4;
                    }
                    keywordQuery.Properties.SetQueryPropertyValue(key, propVal);
                }
            }

            QueryPropertyValue clientFuncPropVal = new QueryPropertyValue();
            clientFuncPropVal.StrVal = clientFunction;
            clientFuncPropVal.QueryPropertyValueTypeIndex = 1;

            keywordQuery.Properties.SetQueryPropertyValue("ClientFunction", clientFuncPropVal);
            return keywordQuery;
        }
    }
}

