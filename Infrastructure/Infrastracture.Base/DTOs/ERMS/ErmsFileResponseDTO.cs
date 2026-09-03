using System.Text.Json.Serialization;

namespace Infrastracture.Base.DTOs.ERMS
{
    public class ErmsFileResponseDTO
    {
        [JsonPropertyName("buffer")]
        public ErmsBufferDTO Buffer { get; set; }

        [JsonPropertyName("contentType")]
        public string ContentType { get; set; }

        [JsonPropertyName("fileName")]
        public string FileName { get; set; }
    }

    public class ErmsBufferDTO
    {
        [JsonPropertyName("type")]
        public string Type { get; set; }

        [JsonPropertyName("data")]
        public List<byte> Data { get; set; }
    }
}
