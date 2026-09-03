using System.Text.Json.Serialization;

namespace Infrastracture.Base.DTOs.ERMS
{
    public class DocumentUploadResponseDTO : ErmsDocumentDTO
    {
        public new DocumentTypeResponseDto DocumentType { get; set; }
        public new SourceDto Source { get; set; }
        public new RetentionDto Retention { get; set; }
        public new StorageDto Storage { get; set; }
    }

    public class DocumentTypeResponseDto
    {
        [JsonPropertyName("Id")]
        public string Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }
    }

    public class SourceDto
    {
        public string Type { get; set; }
        public string UserId { get; set; }
        public string TenantId { get; set; }
        public string Application { get; set; }
        public DateTime Timestamp { get; set; }
    }

    public class RetentionDto
    {
        public string PolicyId { get; set; }
        public DateTime ExpiryDate { get; set; }
        public bool LegalHold { get; set; }
        public string CustomDisposalMethod { get; set; }
        public string ApprovedBy { get; set; }
        public DateTime? ApprovedAt { get; set; }
    }

    public class StorageDto
    {
        public string Location { get; set; }
        public string StorageId { get; set; }
        public bool IsCompressed { get; set; }
    }
}
