using System.Text.Json.Serialization;

namespace Infrastracture.Base.DTOs.ERMS
{
    public class ErmsDocumentTypeDTO
    {
        public string Name { get; set; }
        public string Code { get; set; }
        public string Description { get; set; }
        public List<string> MetadataSchema { get; set; }
        public List<string> ClassificationScheme { get; set; }
        [JsonPropertyName("_id")]
        public string Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
