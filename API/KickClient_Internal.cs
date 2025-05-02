/*
    Copyright (C) 2023 Sehelitar

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

using Kick.API.Internal;
using Newtonsoft.Json;
using System;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using Kick.Bot;

namespace Kick.API
{
    public sealed partial class KickClient
    {
        private const string KickBaseUri = "https://kick.com";

        private KickEventListener EventListener { get; set; }

        public bool IsAuthenticated => Browser?.IsAuthenticated ?? false;

        internal KickBrowser Browser { get; }

        internal KickClient(KickBrowser browser)
        {
            Browser = browser;
        }

        private static Uri ApiTarget(string endpoint)
        {
            return new Uri(KickBaseUri + endpoint);
        }

        private async Task<T> ApiGet<T>(string endpoint)
        {
            var target = ApiTarget(endpoint);
            var jsPayload =
                $@"return cookieStore.get('session_token').then(cookie => fetch( ""{target}"", {{ method: 'GET', headers: {{ 'Accept': 'application/json, text/plain, */*', 'Authorization': 'Bearer ' + decodeURIComponent(cookie.value) }} }} )).then(resp => resp.text()).catch(err => {{ console.log(err); return Promise.resolve(''); }});";

            var result = await Browser.ExecuteAsyncFetch(endpoint, jsPayload);
            if (result == string.Empty) // Try again
                result = await Browser.ExecuteAsyncFetch(endpoint, jsPayload);
            return JsonConvert.DeserializeObject<T>(result);
        }

        private async Task<T> ApiFormPost<T>(string endpoint, NameValueCollection payload)
        {
            var q = string.Join("&", payload.AllKeys.Select(a => a + "=" + System.Web.HttpUtility.UrlEncode(payload[a])));

            var target = ApiTarget(endpoint);
            var jsPayload =
                $@"return cookieStore.get('session_token').then(cookie => fetch( ""{target}"", {{ method: 'POST', headers: {{ 'Content-Type': 'application/x-www-form-urlencoded;charset=UTF-8', 'Accept': 'application/json, text/plain, */*', 'Authorization': 'Bearer ' + decodeURIComponent(cookie.value) }}, body: ""{q}"" }} )).then(resp => resp.text()).catch(err => {{ console.log(err); return Promise.resolve(''); }});";

            var result = await Browser.ExecuteAsyncFetch(endpoint, jsPayload);
            if (result == string.Empty) // Try again
                result = await Browser.ExecuteAsyncFetch(endpoint, jsPayload);
            return JsonConvert.DeserializeObject<T>(result);
        }

        private async Task<T> ApiJsonPost<T>(string endpoint, object payload)
        {
            var json = JsonConvert.SerializeObject(payload);

            var target = ApiTarget(endpoint);
            var jsPayload =
                $@"return cookieStore.get('session_token').then(cookie => fetch( ""{target}"", {{ method: 'POST', headers: {{ 'Content-Type': 'application/json', 'Accept': 'application/json, text/plain, */*', 'Authorization': 'Bearer ' + decodeURIComponent(cookie.value) }}, body: JSON.stringify({json}) }} )).then(resp => resp.text()).catch(err => {{ console.log(err); return Promise.resolve(''); }});";

            var result = await Browser.ExecuteAsyncFetch(endpoint, jsPayload);
            if (result == string.Empty) // Try again
                result = await Browser.ExecuteAsyncFetch(endpoint, jsPayload);
            return JsonConvert.DeserializeObject<T>(result);
        }

        private async Task<T> ApiJsonPut<T>(string endpoint, object payload)
        {
            var json = JsonConvert.SerializeObject(payload);

            var target = ApiTarget(endpoint);
            var jsPayload =
                $@"return cookieStore.get('session_token').then(cookie => fetch( ""{target}"", {{ method: 'PUT', headers: {{ 'Content-Type': 'application/json', 'Accept': 'application/json, text/plain, */*', 'Authorization': 'Bearer ' + decodeURIComponent(cookie.value) }}, body: JSON.stringify({json}) }} )).then(resp => resp.text()).catch(err => {{ console.log(err); return Promise.resolve(''); }});";

            var result = await Browser.ExecuteAsyncFetch(endpoint, jsPayload);
            if (result == string.Empty) // Try again
                result = await Browser.ExecuteAsyncFetch(endpoint, jsPayload);
            return JsonConvert.DeserializeObject<T>(result);
        }

        private async Task<T> ApiPost<T>(string endpoint)
        {
            var target = ApiTarget(endpoint);
            var jsPayload =
                $@"return cookieStore.get('session_token').then(cookie => fetch( ""{target}"", {{ method: 'POST', headers: {{ 'Content-Type': 'application/json', 'Accept': 'application/json, text/plain, */*', 'Authorization': 'Bearer ' + decodeURIComponent(cookie.value) }} }} )).then(resp => resp.text()).catch(err => {{ console.log(err); return Promise.resolve(''); }});";

            var result = await Browser.ExecuteAsyncFetch(endpoint, jsPayload);
            if (result == string.Empty) // Try again
                result = await Browser.ExecuteAsyncFetch(endpoint, jsPayload);
            return JsonConvert.DeserializeObject<T>(result);
        }

        private async Task<T> ApiDelete<T>(string endpoint)
        {
            var target = ApiTarget(endpoint);
            var jsPayload =
                $@"return cookieStore.get('session_token').then(cookie => fetch( ""{target}"", {{ method: 'DELETE', headers: {{ 'Content-Type': 'application/json', 'Accept': 'application/json, text/plain, */*', 'Authorization': 'Bearer ' + decodeURIComponent(cookie.value) }} }} )).then(resp => resp.text()).catch(err => {{ console.log(err); return Promise.resolve(''); }});";

            var result = await Browser.ExecuteAsyncFetch(endpoint, jsPayload);
            if (result == string.Empty) // Try again
                result = await Browser.ExecuteAsyncFetch(endpoint, jsPayload);
            return JsonConvert.DeserializeObject<T>(result);
        }

        /// <summary>
        /// Get an EventListener that will help you listen to channel's events (Follow/Sub/Gifts...)
        /// </summary>
        /// <returns>A KickEventListener to listen to channel's events.</returns>
        /// <exception cref="UnauthenticatedException">Thrown if user is unauthenticated.</exception>
        public KickEventListener GetEventListener()
        {
            if (!IsAuthenticated)
                throw new UnauthenticatedException();

            return EventListener ?? (EventListener = new KickEventListener(this, new KickEventAuthorizer(this)));
        }

        public class UnauthenticatedException : Exception
        {
            public UnauthenticatedException() : base("This Kick client must be authenticated before use.") { }
        }

        internal class KickEventAuthorizer : PusherClient.IAuthorizer
        {
            private readonly KickClient _client;
            public KickEventAuthorizer(KickClient client)
            {
                _client = client;
            }

            public string Authorize(string channelName, string socketId)
            {
                var payload = new NameValueCollection() {
                    { "socket_id", socketId },
                    { "channel_name", channelName }
                };
                var response = _client.ApiFormPost<AuthResponse>("/broadcasting/auth", payload).Result;
                return $"{{\"auth\":\"{response.Auth}\"}}";
            }

            internal class AuthResponse
            {
                [JsonProperty("auth")]
                public string Auth { get; set; }
            }
        }

        internal class KickApiOperationResponse<T>
        {
            [JsonProperty("data")]
            public T Data;
            [JsonProperty("status")]
            public KickApiOperationStatus Status { get; set; }
        }

        internal class KickApiOperationStatus
        {
            [JsonProperty("code")]
            public long Code { get; set; } = 200;
            [JsonProperty("error")]
            public bool Error { get; set; }
            [JsonProperty("message")]
            public string Message { get; set; } = "OK";
        }
    }
}