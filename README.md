# Client OIDC C#
Simple client for request OIDC token and requests. Having the standard version and the version for the .net classic system.

## Install Package Manager

```bash
PM> Install-Package jff_client_oidc_csharp
```

for .net classic

```bash
PM> Install-Package jff_client_oidc_csharp_legacy
```

## Install .NET CLI

```bash
> dotnet add package jff_client_oidc_csharp
```

for .net classic

```bash
> dotnet add package jff_client_oidc_csharp_legacy
```

## Install Paket CLI

```bash
> paket add jff_client_oidc_csharp
```

for .net classic

```bash
> paket add jff_client_oidc_csharp_legacy
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

for .net classic

```bash
using jff_client_oidc_csharp_legacy;

namespace ExampleConnectOIDC
{
    public class ConnectOIDC
    {
        private readonly ClientCredentials client;
        public ConnectOIDC(){}
            client = new ClientCredentials("{urlAuthority}", "{clientId}", "{clientSecret}", new string[] { "openid" });
        }

        public string GetToken(){
            return client.GetToken();
        }
    }
 }
```
