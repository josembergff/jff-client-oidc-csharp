using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net.Http;
using System;
using System.Text;
using System.Threading.Tasks;
using jff_client_oidc_csharp.Models;
using System.Linq;
using System.Net.Http.Headers;

namespace jff_client_oidc_csharp
{
    public class ClientCredentials
    {
        private readonly string urlAuthority;
        private readonly string clientId;
        private readonly string clientSecret;
        private readonly IEnumerable<string> scopes;
        private readonly string mediaType;
        private readonly Encoding encoding;
        private readonly HttpClient client;
        private readonly HttpClient apiClient;
        private string accessToken;
        private DateTime expireDate;

        /// <summary>
        /// Initialize configs to request token and requests
        /// </summary>
        /// <param name="urlAuthority">Required - URL from authority OIDC</param>
        /// <param name="clientId">Required - Identify client in authority OIDC type Client Credentials</param>
        /// <param name="clientSecret">Required - Password client in authority OIDC type Client Credentials</param>
        /// <param name="scopes">Required - Lista scopes from client in authority OIDC type Client Credentials</param>
        /// <param name="encoding">Optional - Default value: UTF8 - Type encoding from values send in POST and PUT request to API</param>
        /// <param name="mediaType">Optional - Default value: application/json - Type media from values send in POST and PUT request to API</param>
        public ClientCredentials(string urlAuthority, string clientId, string clientSecret, IEnumerable<string> scopes, Encoding encoding = null, string mediaType = null)
        {
            this.urlAuthority = urlAuthority;
            this.clientId = clientId;
            this.clientSecret = clientSecret;
            this.scopes = scopes;
            this.mediaType = mediaType == null ? "application/json" : mediaType;
            this.encoding = encoding == null ? Encoding.UTF8 : encoding;
            expireDate = DateTime.MinValue;
            client = new HttpClient();
            apiClient = new HttpClient();
            accessToken = string.Empty;
        }

        public async Task<DefaultResponseModel<string>> GetToken()
        {
            var objReturn = new DefaultResponseModel<string>();
            if (!string.IsNullOrEmpty(urlAuthority))
            {
                if (expireDate <= DateTime.Now)
                {
                    try
                    {
                        HttpResponseMessage tokenEndpointResponse = await client.GetAsync($"{urlAuthority}/.well-known/openid-configuration");
                        if (tokenEndpointResponse.IsSuccessStatusCode)
                        {
                            string objReturnString = await tokenEndpointResponse.Content.ReadAsStringAsync();
                            var objToken = JsonConvert.DeserializeObject<DefaultConfigTokenModel>(objReturnString);
                            var resultToken = await getTokenValue(objToken.token_endpoint);
                            objReturn.Extract(resultToken);
                        }
                        else
                        {
                            objReturn.ListErrors.Add($"An error has occurred in request initial configurations to '{urlAuthority}'.");
                            var errorContent = await tokenEndpointResponse.Content.ReadAsStringAsync();
                            objReturn.Error = errorContent;
                            accessToken = string.Empty;
                        }
                    }
                    catch (Exception ex) { objReturn.Extract(ex); }
                }
            }
            else
            {
                objReturn.ListErrors.Add("The parameter 'urlAuthority' is required in the create new instance class.");
            }

            if (!string.IsNullOrEmpty(accessToken) && objReturn.Success)
            {
                apiClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                objReturn.Result = accessToken;
            }

            return objReturn;
        }

        private async Task<DefaultResponseModel<string>> getTokenValue(string urlToken)
        {
            var objReturn = new DefaultResponseModel<string>();
            if (!string.IsNullOrEmpty(urlToken))
            {
                try
                {
                    var dict = new Dictionary<string, string>();
                    dict.Add("grant_type", "client_credentials");
                    dict.Add("client_id", clientId);
                    dict.Add("client_secret", clientSecret);
                    if (scopes?.Any() == true)
                    {
                        var uniqueScope = string.Join("", scopes);
                        dict.Add("scope", uniqueScope);
                    }
                    var req = new HttpRequestMessage(HttpMethod.Post, urlToken) { Content = new FormUrlEncodedContent(dict) };
                    var tokenResponse = await client.SendAsync(req);

                    if (tokenResponse.IsSuccessStatusCode)
                    {
                        string objReturnString = await tokenResponse.Content.ReadAsStringAsync();
                        var objToken = JsonConvert.DeserializeObject<DefaultResponseTokenModel>(objReturnString);
                        if (objToken.expires_in > 0)
                        {
                            expireDate = DateTime.Now.AddSeconds(objToken.expires_in);
                        }

                        accessToken = objToken.access_token ?? string.Empty;

                        objReturn.Result = accessToken;
                    }
                    else
                    {
                        objReturn.ListErrors.Add($"An error has occurred in request to '{urlToken}'.");
                        var errorContent = await tokenResponse.Content.ReadAsStringAsync();
                        objReturn.Error = errorContent;
                        accessToken = string.Empty;
                    }
                }
                catch (Exception ex) { objReturn.Extract(ex); }
            }
            else
            {
                objReturn.ListErrors.Add("The parameter 'urlToken' is required.");
                accessToken = string.Empty;
            }

            return objReturn;
        }

        public async Task<DefaultResponseModel<ReturnEntity>> Get<ReturnEntity>(string urlApi)
        {
            var objReturn = new DefaultResponseModel<ReturnEntity>();
            var resultToken = await GetToken();
            objReturn.Extract(resultToken);

            if (objReturn.Success)
            {
                try
                {
                    var response = await apiClient.GetAsync(urlApi);
                    if (!response.IsSuccessStatusCode)
                    {
                        objReturn.ListErrors.Add($"An error has occurred in GET request to '{urlApi}'.");
                        var errorContent = await response.Content.ReadAsStringAsync();
                        objReturn.Error = errorContent;
                    }
                    else if (response.Content != null)
                    {
                        var objReturnString = await response.Content.ReadAsStringAsync();
                        objReturn.Result = JsonConvert.DeserializeObject<ReturnEntity>(objReturnString);
                    }
                }
                catch (Exception ex) { objReturn.Extract(ex); }
            }

            return objReturn;
        }

        public async Task<DefaultResponseModel<ReturnEntity>> Post<SendEntity, ReturnEntity>(string urlApi, SendEntity obj)
        {
            var objReturn = new DefaultResponseModel<ReturnEntity>();
            var resultToken = await GetToken();
            objReturn.Extract(resultToken);

            try
            {
                string postData = JsonConvert.SerializeObject(obj);
                StringContent contentData = new StringContent(postData, encoding, mediaType);
                var response = await apiClient.PostAsync(urlApi, contentData);
                if (!response.IsSuccessStatusCode)
                {
                    objReturn.ListErrors.Add($"An error has occurred in POST request to '{urlApi}'.");
                    var errorContent = await response.Content.ReadAsStringAsync();
                    objReturn.Error = errorContent;
                }
                else if (response.Content != null)
                {
                    var objReturnString = await response.Content.ReadAsStringAsync();
                    objReturn.Result = JsonConvert.DeserializeObject<ReturnEntity>(objReturnString);
                }
            }
            catch (Exception ex) { objReturn.Extract(ex); }

            return objReturn;
        }

        public async Task<DefaultResponseModel<ReturnEntity>> Put<SendEntity, ReturnEntity>(string urlApi, SendEntity obj)
        {
            var objReturn = new DefaultResponseModel<ReturnEntity>();
            var resultToken = await GetToken();
            objReturn.Extract(resultToken);

            try
            {
                string postData = JsonConvert.SerializeObject(obj);
                StringContent contentData = new StringContent(postData, encoding, mediaType);
                var response = await apiClient.PutAsync(urlApi, contentData);
                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine(response.StatusCode);
                }
                else if (response.Content != null)
                {
                    var objReturnString = await response.Content.ReadAsStringAsync();
                    objReturn.Result = JsonConvert.DeserializeObject<ReturnEntity>(objReturnString);
                }
            }
            catch (Exception ex) { objReturn.Extract(ex); }

            return objReturn;
        }

        public async Task<DefaultResponseModel<ReturnEntity>> Delete<ReturnEntity>(string urlApi)
        {
            var objReturn = new DefaultResponseModel<ReturnEntity>();
            var resultToken = await GetToken();
            objReturn.Extract(resultToken);
            try
            {
                var response = await apiClient.DeleteAsync(urlApi);
                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine(response.StatusCode);
                }
                else if (response.Content != null)
                {
                    var objReturnString = await response.Content.ReadAsStringAsync();
                    objReturn.Result = JsonConvert.DeserializeObject<ReturnEntity>(objReturnString);
                }
            }
            catch (Exception ex) { objReturn.Extract(ex); }

            return default;
        }
    }
}
