using IdentityModel.Client;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net.Http;
using System;
using System.Text;
using System.Threading.Tasks;

namespace jff_client_oidc_csharp
{
    public class ClientCredentials
    {
        private readonly string urlAuthority;
        private readonly string clientId;
        private readonly string clientSecret;
        private readonly IEnumerable<string> scopes;
        private readonly HttpClient client;
        private readonly HttpClient apiClient;
        private string accessToken;
        private DateTime expireDate;

        public ClientCredentials(string urlAuthority, string clientId, string clientSecret, IEnumerable<string> scopes)
        {
            this.urlAuthority = urlAuthority;
            this.clientId = clientId;
            this.clientSecret = clientSecret;
            this.scopes = scopes;
            expireDate = DateTime.MinValue;
            client = new HttpClient();
            apiClient = new HttpClient();
            accessToken = string.Empty;
        }

        public async Task<string> GetToken()
        {
            if (!string.IsNullOrEmpty(urlAuthority))
            {
                if (expireDate <= DateTime.Now)
                {
                    var tokenEndpointResponse = await client.GetDiscoveryDocumentAsync(urlAuthority);
                    if (tokenEndpointResponse.IsError && !string.IsNullOrEmpty(tokenEndpointResponse?.TokenEndpoint))
                    {
                        Console.WriteLine(tokenEndpointResponse.Error);
                        accessToken = string.Empty;
                    }
                    else
                    {
                        await getTokenValue(tokenEndpointResponse?.TokenEndpoint ?? string.Empty);
                    }
                }
            }

            return accessToken;
        }

        private async Task getTokenValue(string urlToken)
        {
            if (!string.IsNullOrEmpty(urlToken))
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
                    Console.WriteLine(tokenResponse.Error);
                    accessToken = string.Empty;
                }
                else
                {
                    if (tokenResponse.ExpiresIn > 0)
                    {
                        expireDate = DateTime.Now.AddSeconds(tokenResponse.ExpiresIn);
                    }

                    accessToken = tokenResponse.AccessToken ?? string.Empty;

                    apiClient.SetBearerToken(accessToken);
                }
            }
            else
            {
                accessToken = string.Empty;
            }
        }

        public async Task<ReturnEntity> Get<ReturnEntity>(string urlApi)
        {
            await GetToken();
            var response = await apiClient.GetAsync(urlApi);
            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine(response.StatusCode);
            }
            else if (response.Content != null)
            {
                var objReturnString = await response.Content.ReadAsStringAsync();
                var objReturn = JsonConvert.DeserializeObject<ReturnEntity>(objReturnString);
                return objReturn;
            }

            return default;
        }

        public async Task<ReturnEntity> Post<SendEntity, ReturnEntity>(string urlApi, SendEntity obj)
        {
            await GetToken();
            string postData = JsonConvert.SerializeObject(obj);
            StringContent contentData = new StringContent(postData, Encoding.UTF8, "application/json");
            var response = await apiClient.PostAsync(urlApi, contentData);
            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine(response.StatusCode);
            }
            else if (response.Content != null)
            {

                var objReturnString = await response.Content.ReadAsStringAsync();
                var objReturn = JsonConvert.DeserializeObject<ReturnEntity>(objReturnString);
                return objReturn;
            }

            return default;
        }

        public async Task<ReturnEntity> Put<SendEntity, ReturnEntity>(string urlApi, SendEntity obj)
        {
            await GetToken();
            string postData = JsonConvert.SerializeObject(obj);
            StringContent contentData = new StringContent(postData, Encoding.UTF8, "application/json");
            var response = await apiClient.PutAsync(urlApi, contentData);
            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine(response.StatusCode);
            }
            else if (response.Content != null)
            {
                var objReturnString = await response.Content.ReadAsStringAsync();
                var objReturn = JsonConvert.DeserializeObject<ReturnEntity>(objReturnString);
                return objReturn;
            }

            return default;
        }

        public async Task<ReturnEntity> Delete<ReturnEntity>(string urlApi)
        {
            await GetToken();
            var response = await apiClient.DeleteAsync(urlApi);
            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine(response.StatusCode);
            }
            else if (response.Content != null)
            {
                var objReturnString = await response.Content.ReadAsStringAsync();
                var objReturn = JsonConvert.DeserializeObject<ReturnEntity>(objReturnString);
                return objReturn;
            }

            return default;
        }
    }
}
