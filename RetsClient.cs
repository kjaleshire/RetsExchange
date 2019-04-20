using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace RetsExchange
{
    public class RetsClient
    {
        public bool IsLoggedIn { get => _capabilityUrls != null; }
        private readonly Uri _retsBaseUrl;
        private readonly Uri _retsLoginPath;
        private readonly HttpClient _httpClient;
        private CapabilityUrls _capabilityUrls;
        private Random _rng;

        public RetsClient(string retsLoginEndpoint, string retsUsername, string retsPassword)
        {
            var retsLoginUrl = new Uri(retsLoginEndpoint);
            _retsBaseUrl = new Uri(retsLoginUrl.GetLeftPart(UriPartial.Authority));
            _retsLoginPath = new Uri(retsLoginUrl.AbsolutePath, UriKind.Relative);
            _rng = new Random();

            var handler = new HttpClientHandler()
            {
                Credentials = new NetworkCredential(retsUsername, retsPassword),
            };
            _httpClient = new HttpClient(handler) { BaseAddress = _retsBaseUrl };
        }

        public async Task LoginAsync()
        {
            _capabilityUrls = null;

            var responseString = await _httpClient.GetStringAsync(_retsLoginPath);
            var responseRoot = XElement.Parse(responseString);
            validateReplyCode(responseRoot);

            var capabilityUrlsStrings = responseRoot.Element("RETS-RESPONSE").Value;

            _capabilityUrls = new CapabilityUrls(_retsBaseUrl, capabilityUrlsStrings);
        }

        public async Task LogoutAsync()
        {
            var logoutPath = _capabilityUrls.LogoutPath;
            _capabilityUrls = null;
            await _httpClient.GetStringAsync(logoutPath);
        }

        public async Task<RetsSearchResponse<RetsType>> SearchAsync<RetsType>(RetsSearchRequest<RetsType> searchRequest, uint retries = 0)
        where RetsType : new()
        {
            var content = new FormUrlEncodedContent(searchRequest.Parameters);

            var response = await postSearchWithRetry(_capabilityUrls.SearchPath, content, retries);
            var responseString = await response.Content.ReadAsStringAsync();
            var responseRoot = XElement.Parse(responseString);

            validateReplyCode(responseRoot);

            var columns = responseRoot.Element("COLUMNS").Value;
            var rawData = responseRoot.Elements("DATA").Select(xe => xe.Value);
            var recordCount = responseRoot.Element("COUNT")?.Attribute("Records").Value;
            var delimiter = responseRoot.Element("DELIMITER")?.Attribute("value").Value;

            return new RetsSearchResponse<RetsType>(columns, rawData, recordCount, delimiter);
        }

        private async Task<HttpResponseMessage> postSearchWithRetry(Uri searchPath, FormUrlEncodedContent content, uint retries)
        {
            while (true)
            {
                try
                {
                    return await _httpClient.PostAsync(searchPath, content);
                }
                catch (System.IO.IOException)
                {
                    if (retries == 0) throw;
                    retries = retries - 1;
                    // Some issue pulling from RETS server, retry after some
                    // random milliseconds
                    Thread.Sleep(_rng.Next(1000));
                }
            }
        }

        private static void validateReplyCode(XElement responseRoot)
        {
            var replyCode = responseRoot.Attribute("ReplyCode").Value;
            if (replyCode != "0")
                throw new ResponseException($"Got unexpected ReplyCode {replyCode}", responseRoot.ToString());
        }
    }

    public class ResponseException : Exception
    {
        public string ResponseBody { get; }
        public ResponseException(string message, string responseString) : base(message) =>
            ResponseBody = responseString;
    }
}
