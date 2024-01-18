using IdentityModel.Client;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net.Http;
using System;
using System.Text;
using System.Threading.Tasks;
using jff_client_oidc_csharp.Models;
using static IdentityModel.OidcConstants;

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
                        var tokenEndpointResponse = await client.GetDiscoveryDocumentAsync(urlAuthority);
                        if (tokenEndpointResponse.IsError || string.IsNullOrEmpty(tokenEndpointResponse?.TokenEndpoint))
                        {
                            objReturn.ListErrors.Add($"An error has occurred in request initial configurations to '{urlAuthority}'.");
                            objReturn.ListErrors.Add(tokenEndpointResponse.Error);
                            objReturn.Extract(tokenEndpointResponse.Exception);
                            accessToken = string.Empty;
                        }
                        else
                        {
                            var resultToken = await getTokenValue(tokenEndpointResponse?.TokenEndpoint ?? string.Empty);
                            objReturn.Extract(resultToken);
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
                apiClient.SetBearerToken(accessToken);
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
                    var tokenResponse = await client.RequestClientCredentialsTokenAsync(new ClientCredentialsTokenRequest
                    {
                        Address = urlToken,
                        ClientId = clientId,
                        ClientSecret = clientSecret,
                        Scope = string.Join(" ", scopes)
                    });

                    if (tokenResponse.IsError)
                    {
                        objReturn.ListErrors.Add($"An error has occurred in request to '{urlToken}'.");
                        objReturn.ListErrors.Add(tokenResponse.Error);
                        objReturn.Extract(tokenResponse.Exception);
                        accessToken = string.Empty;
                    }
                    else
                    {
                        if (tokenResponse.ExpiresIn > 0)
                        {
                            expireDate = DateTime.Now.AddSeconds(tokenResponse.ExpiresIn);
                        }

                        accessToken = tokenResponse.AccessToken ?? string.Empty;

                        objReturn.Result = accessToken;
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
