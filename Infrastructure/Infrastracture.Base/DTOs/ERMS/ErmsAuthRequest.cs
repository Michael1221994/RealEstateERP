using System.Text.Json.Serialization;

namespace Infrastracture.Base.DTOs.ERMS
{
    public class ErmsAuthRequest
    {
        [JsonPropertyName("client_id")]
        public string ClientId { get; set; }

        [JsonPropertyName("client_secret")]
        public string ClientSecret { get; set; }
    }
}
