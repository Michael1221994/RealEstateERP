namespace Infrastracture.Base.DTOs.ERMS
{
    public class ErmsDocumentTypeWithMetadataDTO : ErmsDocumentTypeDTO
    {
        public List<ErmsMetaDataFieldDTO> MetadataFields { get; set; }
    }
}
