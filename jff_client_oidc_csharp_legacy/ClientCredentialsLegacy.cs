using Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace jff_client_oidc_csharp_legacy
{
    public class ClientCredentialsLegacy
    {
        private readonly string urlAuthority;
        private readonly string clientId;
        private readonly string clientSecret;
        private readonly IEnumerable<string> scopes;
        private string accessToken;
        private DateTime expireDate;

        public ClientCredentialsLegacy(string urlAuthority, string clientId, string clientSecret, IEnumerable<string> scopes)
        {
            this.urlAuthority = urlAuthority;
            this.clientId = clientId;
            this.clientSecret = clientSecret;
            this.scopes = scopes;
            expireDate = DateTime.MinValue;
            accessToken = string.Empty;
        }

        public DefaultResponseModel<string> GetToken()
        {
            var objReturn = new DefaultResponseModel<string>();
            if (!string.IsNullOrEmpty(urlAuthority))
            {
                if (expireDate <= DateTime.Now)
                {
                    try
                    {
                        using (WebClient webClient = new WebClient())
                        {
                            webClient.BaseAddress = urlAuthority + "/";
                            var json = webClient.DownloadString(".well-known/openid-configuration");
                            var objToken = JsonConvert.DeserializeObject<DefaultConfigTokenModel>(json);
                            var resultToken = getTokenValue(objToken.token_endpoint);
                            objReturn.Extract(resultToken);
                        }
                    }
                    catch (Exception ex)
                    {
                        objReturn.ListErrors.Add($"An error has occurred in request initial configurations to '{urlAuthority}'.");
                        accessToken = string.Empty;
                        objReturn.Extract(ex);
                    }
                }
            }
            else
            {
                objReturn.ListErrors.Add("The parameter 'urlAuthority' is required in the create new instance class.");
            }

            return objReturn;
        }

        private DefaultResponseModel<string> getTokenValue(string urlToken)
        {
            var objReturn = new DefaultResponseModel<string>();
            if (!string.IsNullOrEmpty(urlToken))
            {
                try
                {
                    using (WebClient webClient = new WebClient())
                    {
                        var baseUrl = urlAuthority + "/";
                        var pathUrl = urlToken.Replace(baseUrl, "");
                        webClient.BaseAddress = baseUrl;
                        webClient.Headers[HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded";
                        webClient.Headers[HttpRequestHeader.Accept] = "*/*";
                        var scope = "";
                        if (scopes?.Any() == true)
                        {
                            var uniqueScope = string.Join("", scopes);
                            scope = uniqueScope;
                        }
                        string data = $"client_id={clientId}&client_secret={clientSecret}&grant_type=client_credentials&scope={scope}";
                        var response = webClient.UploadString(pathUrl, "POST", data);
                        var objToken = JsonConvert.DeserializeObject<DefaultResponseTokenModel>(response);

                        if (objToken.expires_in > 0)
                        {
                            expireDate = DateTime.Now.AddSeconds(objToken.expires_in);
                        }

                        accessToken = objToken.access_token ?? string.Empty;

                        objReturn.Result = accessToken;
                    }
                }
                catch (Exception ex)
                {
                    objReturn.ListErrors.Add($"An error has occurred in request to '{urlToken}'.");
                    accessToken = string.Empty;
                    objReturn.Extract(ex);
                }
            }
            else
            {
                objReturn.ListErrors.Add("The parameter 'urlToken' is required.");
                accessToken = string.Empty;
            }

            return objReturn;
        }
    }
}
