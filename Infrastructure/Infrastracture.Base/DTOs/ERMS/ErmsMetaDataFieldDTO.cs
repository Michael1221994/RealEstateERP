using System.Text.Json.Serialization;

namespace Infrastracture.Base.DTOs.ERMS
{
    public class ErmsMetaDataFieldDTO
    {
        public string Field { get; set; }
        public string Type { get; set; }
        public bool Required { get; set; }
        public string Label { get; set; }
        public bool Indexed { get; set; }
        public object DefaultValue { get; set; }
        public List<MetaDataFieldOption> Options { get; set; }
        public MetaDataFieldValidations Validations { get; set; }
        [JsonPropertyName("_id")]
        public string Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
    public class MetaDataFieldOption
    {
        public string Value { get; set; }
        public string Label { get; set; }
    }

    public class MetaDataFieldValidations
    {
        public int? MinLength { get; set; }
        public int? MaxLength { get; set; }
        public int? Min { get; set; }
        public int? Max { get; set; }
        public string Pattern { get; set; }
    }
}
