using jff_client_oidc_csharp;
using jff_client_oidc_csharp_legacy;

Console.WriteLine("Hello, World!");

testeClientLegacy();

void testeClientLegacy()
{
    var client = new ClientCredentialsLegacy("https://localhost:62862", "api", "secret", new string[] { "openid" });

    var token = client.GetToken().Result;
}

void testeClient()
{
    var client = new ClientCredentials("https://localhost:62862", "api", "secret", new string[] { "openid" });

    var token = client.GetToken().Result;

    if (token.Success)
    {
        var resultAPI = client.Get<dynamic>("https://localhost:3000").Result;

        Console.WriteLine(resultAPI.Result.ToString());
    }
    else
    {
        Console.WriteLine(token.Error);
        Console.WriteLine(string.Join(";", token.ListErrors));
    }
}