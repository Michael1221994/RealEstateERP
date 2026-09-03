namespace Infrastracture.Base.BaseEntities
{
    public class DocumentSignatureConfigurationModel
    {
        public const string SectionName = "DocumentSignature";
        public string SecretKey { get; set; } = String.Empty;
        public string Algorithm { get; set; } = String.Empty;
    }
}
