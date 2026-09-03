using System.Text.Json.Serialization;

namespace Infrastracture.Base.DTOs.ERMS
{
    public class ErmsAuthResponse
    {
        [JsonPropertyName("access_token")]
        public string AccessToken { get; set; }

        [JsonPropertyName("expires_in")]
        public int ExpiresIn { get; set; }
    }
}
