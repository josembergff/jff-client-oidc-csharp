
namespace jff_client_oidc_csharp_legacy.Models
{
    public class SendRequestTokenModel
    {
        public string grant_type { get; set; }
        public string client_id { get; set; }
        public string client_secret { get; set; }
        public string scope { get; set; }
    }
}
