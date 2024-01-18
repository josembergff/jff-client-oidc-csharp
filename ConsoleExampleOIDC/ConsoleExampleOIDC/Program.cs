// See https://aka.ms/new-console-template for more information
using IdentityModel.Client;

Console.WriteLine("Hello, World!");

var urlToken = getUrlToken();

if (!string.IsNullOrEmpty(urlToken))
{
    var token = getTokenValue(urlToken);
    if (!string.IsNullOrEmpty(token))
    {
        callApi(token);
    }
}

static string getUrlToken()
{
    var client = new HttpClient();
    var disco = client.GetDiscoveryDocumentAsync("https://localhost:62862").Result;
    if (disco.IsError)
    {
        Console.WriteLine(disco.Error);
        return string.Empty;
    }
    else
    {
        Console.WriteLine(disco.TokenEndpoint);
        return disco?.TokenEndpoint ?? string.Empty;
    }
}

static string getTokenValue(string urlToken)
{
    var client = new HttpClient();
    var tokenResponse = client.RequestClientCredentialsTokenAsync(new ClientCredentialsTokenRequest
    {
        Address = urlToken,
        ClientId = "client",
        ClientSecret = "secret",
        Scope = "api1"
    }).Result;

    if (tokenResponse.IsError)
    {
        Console.WriteLine(tokenResponse.Error);
        return string.Empty;
    }

    Console.WriteLine(tokenResponse.AccessToken);

    return tokenResponse.AccessToken ?? string.Empty;
}

static void callApi(string accesToken)
{
    var apiClient = new HttpClient();
    apiClient.SetBearerToken(accesToken);

    var response = apiClient.GetAsync("https://localhost:3000").Result;
    if (!response.IsSuccessStatusCode)
    {
        Console.WriteLine(response.StatusCode);
    }
    else
    {
        var content = response.Content.ReadAsStringAsync().Result;
        Console.WriteLine(content);
    }
}