using Codeplex.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using System.Text;

namespace GistsApi
{
    public class GistClient
    {
        #region Fields
        private readonly string _clientId;
        private readonly string _clientSecret;
        private readonly string _scope;
        private string _accessToken;
        private readonly string _userAgent;
        private CancellationTokenSource cancellationTS;
        private HttpResponseHeaders _responseHeaders;
        #endregion

        #region Properties
        public string FirstLinkUrl { get; protected set; }
        public string LastLinkUrl { get; protected set; }
        public string NextLinkUrl { get; protected set; }
        public string PrevLinkUrl { get; protected set; }
        public Uri AuthorizeUrl => new(string.Format("https://github.com/login/oauth/authorize?client_id={0}&scope={1}",
                    _clientId, _scope));
        #endregion

        #region Constructors
        public GistClient(string clientKey, string clientSecret, string userAgent)
        {
            _clientId = clientKey;
            _clientSecret = clientSecret;
            cancellationTS = new CancellationTokenSource();
            _scope = "gist";
            _userAgent = userAgent;
        }
        #endregion

        #region Methods
        public async Task Authorize(string authCode)
        {
            var requestUri = new Uri(string.Format("https://github.com/login/oauth/access_token?client_id={0}&client_secret={1}&code={2}", _clientId, _clientSecret, authCode));
            using HttpClient httpClient = CreateHttpClient();
            var response = await httpClient.PostAsync(requestUri, null, this.cancellationTS.Token);
            string responseString = await response.EnsureSuccessStatusCode().Content.ReadAsStringAsync();
            object json = DynamicJson.Parse(responseString);
            _accessToken = (string)((dynamic)json).access_token;
        }

        public void Cancel() => cancellationTS.Cancel();

        public async Task<GistObject> CreateAGist(string description, bool isPublic, IEnumerable<Tuple<string, string>> fileContentCollection)
        {
            using HttpClient httpClient = CreateHttpClient();
            var requestUri = new Uri(string.Format("https://api.github.com/gists?access_token={0}", _accessToken));

            string content = MakeCreateContent(description, isPublic, fileContentCollection);
            var data = new StringContent(content, Encoding.UTF8, "application/json");

            var response = await httpClient.PostAsync(requestUri, data, cancellationTS.Token);
            _responseHeaders = response.Headers;

            string json = await response.EnsureSuccessStatusCode().Content.ReadAsStringAsync();
            return (GistObject)DynamicToGistObject(DynamicJson.Parse(json));
        }

        protected HttpClient CreateHttpClient()
        {
            if (cancellationTS.IsCancellationRequested)
            {
                cancellationTS = new CancellationTokenSource();
            }
            var client = new HttpClient();
            client.DefaultRequestHeaders.Accept.ParseAdd("application/json");
            client.DefaultRequestHeaders.UserAgent.ParseAdd(_userAgent);
            return client;
        }

        public async Task<GistObject> DeleteAFile(string id, string description, string filename)
        {
            using HttpClient httpClient = CreateHttpClient();
            var requestUri = new Uri(string.Format("https://api.github.com/gists/{0}?access_token={1}", id, _accessToken));
            var request = new HttpRequestMessage(new HttpMethod("PATCH"), requestUri);

            string content = MakeDeleteFileContent(description, filename);
            request.Content = new StringContent(content, Encoding.UTF8, "application/json");

            var response = await httpClient.SendAsync(request, cancellationTS.Token);
            _responseHeaders = response.Headers;

            string json = await response.EnsureSuccessStatusCode().Content.ReadAsStringAsync();
            return (GistObject)DynamicToGistObject(DynamicJson.Parse(json));
        }

        public async Task DeleteAGist(string id)
        {
            using HttpClient httpClient = CreateHttpClient();
            var requestUri = new Uri(string.Format("https://api.github.com/gists/{0}?access_token={1}", id, _accessToken));
            var response = await httpClient.DeleteAsync(requestUri);
            _responseHeaders = response.Headers;
            response.EnsureSuccessStatusCode();
        }

        public async Task<string> DownloadRawText(Uri rawUrl)
        {
            using HttpClient httpClient = CreateHttpClient();
            var response = await httpClient.GetAsync(rawUrl, cancellationTS.Token);
            _responseHeaders = response.Headers;
            return await response.EnsureSuccessStatusCode().Content.ReadAsStringAsync();
        }

        protected static GistObject DynamicToGistObject(dynamic json)
        {
            var gist = (GistObject)json;
            var files = ((DynamicJson)json.files).DeserializeMembers(member =>
              new File()
              {
                  filename = member.filename,
                  raw_url = member.raw_url,
                  size = member.size
              });

            gist.files = new Files(files.ToArray());
            return gist;
        }

        public async Task<GistObject> EditAGist(string id, string description, string targetFilename, string content)
        {
            using HttpClient httpClient = CreateHttpClient();
            var requestUri = new Uri(string.Format("https://api.github.com/gists/{0}?access_token={1}", id, _accessToken));
            var request = new HttpRequestMessage(new HttpMethod("PATCH"), requestUri);

            string editData = MakeEditContent(description, targetFilename, content);
            request.Content = new StringContent(editData, Encoding.UTF8, "application/json");

            var response = await httpClient.SendAsync(request, cancellationTS.Token);
            _responseHeaders = response.Headers;

            string json = await response.EnsureSuccessStatusCode().Content.ReadAsStringAsync();
            return (GistObject)DynamicToGistObject(DynamicJson.Parse(json));
        }

        public async Task<GistObject> EditAGist(string id, string description, string oldFilename, string newFilename, string content)
        {
            using HttpClient httpClient = CreateHttpClient();
            var requestUri = new Uri(string.Format("https://api.github.com/gists/{0}?access_token={1}", id, _accessToken));
            var request = new HttpRequestMessage(new HttpMethod("PATCH"), requestUri);

            var editData = MakeEditContent(description, oldFilename, newFilename, content);
            request.Content = new StringContent(editData, Encoding.UTF8, "application/json");

            var response = await httpClient.SendAsync(request, cancellationTS.Token);
            _responseHeaders = response.Headers;

            string json = await response.EnsureSuccessStatusCode().Content.ReadAsStringAsync();
            return (GistObject)DynamicToGistObject(DynamicJson.Parse(json));
        }

        public async Task<GistObject> ForkAGist(string id)
        {
            using HttpClient httpClient = CreateHttpClient();
            var requestUri = new Uri(string.Format("https://api.github.com/gists/{0}/forks?access_token={1}", id, _accessToken));

            var response = await httpClient.PostAsync(requestUri, null, cancellationTS.Token);
            _responseHeaders = response.Headers;

            string json = await response.EnsureSuccessStatusCode().Content.ReadAsStringAsync();
            return (GistObject)DynamicToGistObject(DynamicJson.Parse(json));
        }

        public async Task<GistObject> GetSingleGist(string id)
        {
            var requestUrl = new Uri(string.Format("https://api.github.com/gists/{0}?access_token={1}", id, _accessToken));
            var json = await GetStringAsync(requestUrl);
            return (GistObject)DynamicToGistObject(DynamicJson.Parse(json));
        }

        public async Task<IEnumerable<GistObject>> ListGists()
        {
            var requestUrl = new Uri(string.Format("https://api.github.com/gists?access_token={0}", this._accessToken));
            return await ListGists(requestUrl);
        }

        public async Task<IEnumerable<GistObject>> ListGists(ListMode mode)
        {
            Uri requestUrl;
            requestUrl = mode == ListMode.PublicGists
                ? new Uri("https://api.github.com/gists/public")
                : new Uri(string.Format("https://api.github.com/gists{0}?access_token={1}",
                    (mode == ListMode.AuthenticatedUserStarredGists) ? "/starred" : "", _accessToken));
            return await ListGists(requestUrl);
        }

        public async Task<IEnumerable<GistObject>> ListGists(string user)
        {
            var requestUrl = new Uri(string.Format("https://api.github.com/users/{0}/gists", user));
            return await ListGists(requestUrl);
        }

        public async Task<IEnumerable<GistObject>> ListGists(Uri requestUrl)
        {
            //GET /gists
            using var httpClient = CreateHttpClient();
            var response = await GetStringAsync(requestUrl);
            SetLinkUrl();
            var json = (dynamic[])DynamicJson.Parse(response);
            return json.Select(j => (GistObject)DynamicToGistObject(j));
        }

        public async Task<IEnumerable<GistObject>> ListGists(string user, DateTime since)
        {
            Uri requestUrl = new(string.Format("https://api.github.com/users/{0}/gists?since={1}", user, since.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssK")));
            return await ListGists(requestUrl);
        }

        public async Task StarAGist(string id)
        {
            using HttpClient httpClient = CreateHttpClient();
            var requestUri = new Uri(string.Format("https://api.github.com/gists/{0}/star?access_token={1}", id, _accessToken));
            var response = await httpClient.PutAsync(requestUri, null);
            _responseHeaders = response.Headers;
            response.EnsureSuccessStatusCode();
        }

        public async Task UnstarAGist(string id)
        {
            using HttpClient httpClient = CreateHttpClient();
            var requestUri = new Uri(string.Format("https://api.github.com/gists/{0}/star?access_token={1}", id, _accessToken));
            var response = await httpClient.DeleteAsync(requestUri);
            _responseHeaders = response.Headers;
            response.EnsureSuccessStatusCode();
        }

        protected async Task<string> GetStringAsync(Uri requestUrl)
        {
            using HttpClient httpClient = CreateHttpClient();
            var response = await httpClient.GetAsync(requestUrl, cancellationTS.Token);
            _responseHeaders = response.Headers;
            return await response.EnsureSuccessStatusCode().Content.ReadAsStringAsync();
        }

        protected static string MakeCreateContent(string _description, bool _isPublic, IEnumerable<Tuple<string, string>> fileContentCollection)
        {
            dynamic _result = new DynamicJson();
            _result.description = _description;
            _result.@public = _isPublic.ToString().ToLower();
            _result.files = new { };
            foreach (var fileContent in fileContentCollection)
            {
                _result.files[fileContent.Item1] = new { filename = fileContent.Item1, content = fileContent.Item2 };
            }
            return _result.ToString();
        }

        protected static string MakeEditContent(string _description, string _targetFileName, string _content)
        {
            dynamic _result = new DynamicJson();
            _result.description = _description;
            _result.files = new { };
            _result.files[_targetFileName] = new { content = _content };
            return _result.ToString();
        }

        protected static string MakeEditContent(string _description, string _oldFileName, string _newFileName, string _content)
        {
            dynamic _result = new DynamicJson();
            _result.description = _description;
            _result.files = new { };
            _result.files[_oldFileName] = new { filename = _newFileName, content = _content };
            return _result.ToString();
        }

        protected static string MakeDeleteFileContent(string _description, string filename)
        {
            dynamic _result = new DynamicJson();
            _result.description = _description;
            _result.files = new { };
            _result.files[filename] = "null";
            return _result.ToString();
        }

        protected void SetLinkUrl()
        {
            FirstLinkUrl = "";
            PrevLinkUrl = "";
            NextLinkUrl = "";
            LastLinkUrl = "";
            var pair = _responseHeaders.FirstOrDefault(h => h.Key == "Link");
            if (pair.Key != "Link")
            { return; }
            string linkValue = pair.Value.FirstOrDefault();
            if (linkValue == null)
            { return; }

            foreach (string item in linkValue.Split(new char[] { ',' }).Select(s => s.Trim()))
            {
                var token = item.Split(new char[] { ';' });
                if (token.Length < 2)
                { continue; }

                var url = token[0].Trim().TrimStart(new char[] { '<' }).TrimEnd(new char[] { '>' });
                switch (token[1].Trim())
                {
                    case "rel=\"first\"":
                        FirstLinkUrl = url;
                        break;

                    case "rel=\"prev\"":
                        PrevLinkUrl = url;
                        break;

                    case "rel=\"next\"":
                        NextLinkUrl = url;
                        break;
                    case "rel=\"last\"":
                        LastLinkUrl = url;
                        break;
                }
            }
        }

        #endregion

        #region Nested Classes
        public enum ListMode
        {
            PublicGists,
            UsersGists,
            AuthenticatedUserGists,
            AuthenticatedUserStarredGists
        }
        #endregion
    }
}
