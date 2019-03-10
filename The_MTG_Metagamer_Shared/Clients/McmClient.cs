using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using The_MTG_Metagamer_Shared.Clients.McmModels;

namespace The_MTG_Metagamer_Shared.Clients
{

    public enum Method
    {
        GET,
        POST,
        PUT,
        PATCH,
        DELETE
    }

    public static class Constants
    {
        public const string McmBaseUrl = "https://api.cardmarket.com/ws/v2.0/output.json/";

        public const string Account = "account";
        public const string FindProductExact = "products/find";
        public const string FindProductById = "products";
    }



    /// <summary>
    /// Source: https://www.mkmapi.eu/ws/documentation/API:Auth_csharp
    /// </summary>
    public static class McmClient
    {
        public static async Task<string> GetAccountAsync()
        {
            var urlPart = "account";
            return await GetJsonResponseAsync(urlPart, Method.GET);
        }

        public static async Task<ProductResults> GetExactProductAsync(string productName)
        {
            var urlPart = Constants.FindProductExact;
            var queryParameters = new KeyValuePair<string, string>[2]
            {
                new KeyValuePair<string, string>("search", productName),
                new KeyValuePair<string, string>("exact", "true")
            };

            var result = await GetJsonResponseAsync(urlPart, Method.GET, queryParameters);
            return JsonConvert.DeserializeObject<ProductResults>(result);
        }

        public static async Task<ProductResult> GetProductByIdAsync(int id)
        { 
            var urlPart = $"{Constants.FindProductById}/{id}";
            var result = await GetJsonResponseAsync(urlPart, Method.GET);
            return JsonConvert.DeserializeObject<ProductResult>(result);
        }

        private static async Task<string> GetJsonResponseAsync(string urlPart, Method method, params KeyValuePair<string, string>[] queryParams)
        {
            var url = RequestUrl(urlPart);
            HttpWebRequest request;
            if (queryParams.Count() > 0)
            {
                var fullUrl = $"{url}?{string.Join("&", queryParams.Select(v => $"{v.Key}={v.Value}"))}";
                request = WebRequest.CreateHttp(fullUrl);
            }
            else
            {
                request = WebRequest.CreateHttp(url);
            }
            var header = new OAuthHeader(queryParams);
            request.Headers.Add(HttpRequestHeader.Authorization, header.GetAuthorizationHeader(method.ToString(), url));
            request.Method = method.ToString();

            var response = await request.GetResponseAsync();
            string result = default;
            using (var stream = response.GetResponseStream())
            {
                var reader = new StreamReader(stream, Encoding.UTF8);
                result = await reader.ReadToEndAsync();
            }
            return result;
        }

        private static string RequestUrl(string urlPart)
        {
            var url = $"{Constants.McmBaseUrl}{urlPart}";
            return url;
        }
    }

    /// <summary>
    /// Class encapsulates tokens and secret to create OAuth signatures and return Authorization headers for web requests.
    /// </summary>
    public class OAuthHeader
    {
        /// <summary>App Token</summary>
        protected string appToken = "DNUdthHkSx2rfKof";
        /// <summary>App Secret</summary>
        protected string appSecret = "hOxLfH6DDJqjVoNocgz5jg6hnzSZwkkK";
        /// <summary>Access Token (Class should also implement an AccessToken property to set the value)</summary>
        protected string accessToken = "pRGOxB3hFRPNJO7G2E9r4caLKTq2HZkG";
        /// <summary>Access Token Secret (Class should also implement an AccessToken property to set the value)</summary>
        protected string accessSecret = "hVTaWtkhNbBMqK6fERK5wH0pdEMx6Ro5";
        /// <summary>OAuth Signature Method</summary>
        protected string signatureMethod = "HMAC-SHA1";
        /// <summary>OAuth Version</summary>
        protected string version = "1.0";
        /// <summary>All Header params compiled into a Dictionary</summary>
        protected IDictionary<string, string> headerParams;
        protected IDictionary<string, string> additionalParams;

        /// <summary>
        /// Constructor
        /// </summary>
        public OAuthHeader()
        {
            var nonce = Guid.NewGuid().ToString("n");
            var rawTimestamp = DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds.ToString();
            var timestamp = rawTimestamp.Remove(rawTimestamp.IndexOf(','));

            /// Initialize all class members
            headerParams = new Dictionary<string, string>
            {
                { "oauth_consumer_key", appToken },
                { "oauth_nonce", nonce },
                { "oauth_signature_method", signatureMethod },
                { "oauth_timestamp", timestamp },
                { "oauth_token", accessToken },
                { "oauth_version", version }
            };

            additionalParams = new Dictionary<string, string>();
        }

        public OAuthHeader(params KeyValuePair<string, string>[] queryParams) : this()
        {
            foreach (var q in queryParams)
                additionalParams.Add(q);
        }

        // sort order has to be like this
        //oauth_consumer_key - bfaD9xOU0SXBhtBP
        //oauth_nonce - 53eb1f44909d6
        //oauth_signature_method - HMAC-SHA1
        //oauth_timestamp - 1407917892
        //oauth_token - lBY1xptUJ7ZJSK01x4fNwzw8kAe5b10Q
        //oauth_version - 1.0

        /// <summary>
        /// Pass request method and URI parameters to get the Authorization header value
        /// </summary>
        /// <param name="method">Request Method</param>
        /// <param name="url">Request URI</param>
        /// <returns>Authorization header value</returns>
        public string GetAuthorizationHeader(string method, string url)
        {
            /// Add the realm parameter to the header params
            headerParams.Add("realm", url);

            /// Start composing the base string from the method and request URI
            var basestring = method.ToUpper()
                              + "&"
                              + Uri.EscapeDataString(url)
                              + "&";

            /// Gather, encode, and sort the base string parameters
            var encodedParams = new SortedDictionary<string, string>();
            foreach (var parameter in headerParams)
            {
                if (false == parameter.Key.Equals("realm"))
                {
                    encodedParams.Add(Uri.EscapeDataString(parameter.Key), Uri.EscapeDataString(parameter.Value));
                }
            }
            foreach (var parameter in additionalParams)
            {
                encodedParams.Add(Uri.EscapeDataString(parameter.Key), Uri.EscapeDataString(parameter.Value));
            }

            /// Expand the base string by the encoded parameter=value pairs
            var paramstrings = new List<string>();
            foreach (var parameter in encodedParams)
            {
                paramstrings.Add(parameter.Key + "=" + parameter.Value);
            }
            var paramstring = Uri.EscapeDataString(string.Join<string>("&", paramstrings.OrderBy(s => s)));
            basestring += paramstring;

            /// Create the OAuth signature
            var signatureKey = Uri.EscapeDataString(appSecret) + "&" + Uri.EscapeDataString(accessSecret);
            var hasher = HMACSHA1.Create("HMACSHA1");
            hasher.Key = Encoding.UTF8.GetBytes(signatureKey);
            var rawSignature = hasher.ComputeHash(Encoding.UTF8.GetBytes(basestring));
            var oAuthSignature = Convert.ToBase64String(rawSignature);

            /// Include the OAuth signature parameter in the header parameters array
            headerParams.Add("oauth_signature", oAuthSignature);

            /// Construct the header string
            var headerParamstrings = new List<string>();
            foreach (var parameter in headerParams)
            {
                headerParamstrings.Add(parameter.Key + "=\"" + parameter.Value + "\"");
            }
            var authHeader = "OAuth " + string.Join<string>(", ", headerParamstrings.OrderBy(k => k));

            return authHeader;
        }
    }
}
