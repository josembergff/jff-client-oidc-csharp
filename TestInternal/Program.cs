using jff_client_oidc_csharp;

Console.WriteLine("Hello, World!");

testeClient();

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