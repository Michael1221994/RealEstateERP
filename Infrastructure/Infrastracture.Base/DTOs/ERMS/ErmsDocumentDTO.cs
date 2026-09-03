using System.Text.Json.Serialization;

namespace Infrastracture.Base.DTOs.ERMS
{
    public class ErmsDocumentDTO
    {
        [JsonPropertyName("_id")]
        public string Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string FileName { get; set; }
        public long FileSize { get; set; }
        public string FileType { get; set; }
        public string FileExtension { get; set; }
        public string FileHash { get; set; }
        public string MimeType { get; set; }
        public string DocumentType { get; set; }
        public List<string> ClassificationOverride { get; set; }
        public string AccessLevelOverride { get; set; }
        public string ConfidentialityOverride { get; set; }
        public object Metadata { get; set; }
        public object Source { get; set; }
        public object Retention { get; set; }
        public object Storage { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
