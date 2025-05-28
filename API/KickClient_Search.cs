/*
    Copyright (C) 2023-2025 Sehelitar

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU Affero General Public License as published
    by the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU Affero General Public License for more details.

    You should have received a copy of the GNU Affero General Public License
    along with this program.  If not, see <https://www.gnu.org/licenses/>.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Kick.API
{
    public sealed partial class KickClient
    {
        private const string KickSearchBaseUri = "https://search.kick.com";

        public static class SearchCollections
        {
            public const string Categories = "subcategory_index";
        }
        
        private async Task<SearchResult<T>> ApiSearch<T>(string collectionName = "default", Dictionary<string, dynamic> query = null)
        {
            try
            {
                var queryString = query != null
                    ? "?" + string.Join("&",
                        query.Select(kvp =>
                            $"{Uri.EscapeDataString(kvp.Key)}={Uri.EscapeDataString(kvp.Value.ToString())}"
                        )
                    )
                    : string.Empty;
                var target = new Uri($"{KickSearchBaseUri}/collections/{collectionName}/documents/search{queryString}");
                
                var jsPayload =
                    $@"return fetch( ""{target}"", {{ method: 'GET', headers: {{ 'Accept': 'application/json, text/plain, */*', 'X-Typesense-Api-Key': 'nXIMW0iEN6sMujFYjFuhdrSwVow3pDQu' }} }} ).then(resp => resp.text()).catch(err => {{ console.log(err); return Promise.resolve(''); }});";

                var result = await Browser.ExecuteAsyncFetch(target.ToString(), jsPayload);
                if (result == string.Empty) // Try again
                    result = await Browser.ExecuteAsyncFetch(target.ToString(), jsPayload);
                return JsonConvert.DeserializeObject<SearchResult<T>>(result);
            }
            catch (Exception ex)
            {
                throw new Exception("Kick search failed.", ex);
            }
        }
        
        internal class SearchResult<T>
        {
            [JsonProperty("facet_counts")] public object[] FacetCounts { get; internal set; }
            [JsonProperty("found")] public long Found { get; internal set; }
            [JsonProperty("hits")] public SearchResultHit<T>[] Hits { get; internal set; }
            [JsonProperty("out_of")] public long OutOf { get; internal set; }
            [JsonProperty("page")] public long Page { get; internal set; }
            [JsonProperty("request_params")] public SearchRequestParams RequestParams { get; internal set; }
            [JsonProperty("search_cutoff")] public bool SearchCutoff { get; internal set; }
            [JsonProperty("search_time_ms")] public long SearchTimeMs { get; internal set; }
        }

        internal class SearchRequestParams
        {
            [JsonProperty("collection_name")] public string CollectionName { get; internal set; }
            [JsonProperty("first_q")] public string Suggestion { get; internal set; }
            [JsonProperty("per_page")] public long PageSize { get; internal set; }
            [JsonProperty("q")] public string Query { get; internal set; }
        }

        internal class SearchResultHit<T>
        {
            [JsonProperty("document")] public T Document { get; internal set; }

            [JsonProperty("highlights")]
            public SearchResultHitHighlightEntryWithField[] Highlights { get; internal set; }

            [JsonProperty("highlight")]
            public Dictionary<string, SearchResultHitHighlightEntry> Highlight { get; internal set; }

            [JsonProperty("text_match")] public long TextMatch { get; internal set; }
            [JsonProperty("text_match_info")] public SearchTextMatchInfo TextMatchInfo { get; internal set; }
        }

        internal class SearchResultHitHighlightEntryWithField
        {
            [JsonProperty("field")] public string Field { get; internal set; }
            [JsonProperty("matched_tokens")] public string[] MatchedTokens { get; internal set; }
            [JsonProperty("snippet")] public string Snippet { get; internal set; }
        }

        internal class SearchResultHitHighlightEntry
        {
            [JsonProperty("matched_tokens")] public string[] MatchedTokens { get; internal set; }
            [JsonProperty("snippet")] public string Snippet { get; internal set; }
        }

        internal class SearchTextMatchInfo
        {
            [JsonProperty("best_field_score")] public string BestFieldScore { get; internal set; }
            [JsonProperty("best_field_weight")] public long BestFieldWeight { get; internal set; }
            [JsonProperty("fields_matched")] public long FieldsMatched { get; internal set; }
            [JsonProperty("num_tokens_dropped")] public long NumTokensDropped { get; internal set; }
            [JsonProperty("score")] public string Score { get; internal set; }
            [JsonProperty("tokens_matched")] public long TokensMatched { get; internal set; }
            [JsonProperty("type_prefix_score")] public long TypePrefixScore { get; internal set; }
        }
    }
}