# Client OIDC C#
Simple client for request OIDC token and requests. Having the standard version.

## Install Package Manager

```bash
PM> Install-Package jff_client_oidc_csharp
```

## Install .NET CLI

```bash
> dotnet add package jff_client_oidc_csharp
```

## Install Paket CLI

```bash
> paket add jff_client_oidc_csharp
```

## Example Usage

```bash
using jff_client_oidc_csharp;

namespace ExampleConnectOIDC
{
    public class ConnectOIDC
    {
        private readonly ClientCredentials client;
        public ConnectOIDC(){}
            client = new ClientCredentials("{urlAuthority}", "{clientId}", "{clientSecret}", new string[] { "openid" });
        }

        public async Task<dynamic> GetApiOIDC(string url){
            return await client.Get<dynamic>(url);
        }

        public async Task<string> GetToken(){
            return await client.GetToken();
        }
    }
 }
```
