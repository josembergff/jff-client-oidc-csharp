using System.Collections.Generic;

namespace Models
{
    public class DefaultConfigTokenModel
    {
        public string issuer { get; set; }
        public string jwks_uri { get; set; }
        public string authorization_endpoint { get; set; }
        public string token_endpoint { get; set; }
        public string userinfo_endpoint { get; set; }
        public string end_session_endpoint { get; set; }
        public string check_session_iframe { get; set; }
        public string revocation_endpoint { get; set; }
        public string introspection_endpoint { get; set; }
        public string device_authorization_endpoint { get; set; }
        public bool frontchannel_logout_supported { get; set; }
        public bool frontchannel_logout_session_supported { get; set; }
        public bool backchannel_logout_supported { get; set; }
        public bool backchannel_logout_session_supported { get; set; }
        public IEnumerable<string> scopes_supported { get; set; }
        public IEnumerable<string> claims_supported { get; set; }
        public IEnumerable<string> grant_types_supported { get; set; }
        public IEnumerable<string> response_types_supported { get; set; }
        public IEnumerable<string> response_modes_supported { get; set; }
        public IEnumerable<string> token_endpoint_auth_methods_supported { get; set; }
        public IEnumerable<string> id_token_signing_alg_values_supported { get; set; }
        public IEnumerable<string> subject_types_supported { get; set; }
        public IEnumerable<string> code_challenge_methods_supported { get; set; }
        public bool request_parameter_supported { get; set; }
    }
}
