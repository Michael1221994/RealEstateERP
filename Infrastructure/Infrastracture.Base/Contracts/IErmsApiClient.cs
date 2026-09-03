using Infrastracture.Base.DTOs.ERMS;

namespace Infrastracture.Base.Contracts
{
    public interface IErmsApiClient
    {
        Task<ErmsDocumentDTO?> GetDocumentInfoAsync(string id);
        Task<IEnumerable<ErmsDocumentTypeDTO>?> GetDocumentTypesAsync();
        Task<ErmsFileResponseDTO?> GetDocumentFileAsync(string documentId);
        Task<ErmsMetaDataFieldDTO?> GetMetadataFieldAsync(string schemaId);
        Task<DocumentUploadResponseDTO> UploadDocumentAsync(MultipartFormDataContent multipartFormDataContent);
        Task<ErmsDocumentTypeWithMetadataDTO?> GetDocumentTypeByCodeAsync(string code);
        Task<TenantResponseDto?> CreateTenantAsync(CreateTenantDto createTenantDto);

    }
}
