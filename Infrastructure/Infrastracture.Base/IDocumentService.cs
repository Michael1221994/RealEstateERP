using Infrastracture.Base;
using Infrastracture.Base.DTOs.ERMS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastracture.Base.Contracts
{
    public interface IDocumentService
    {
        Task<Response<DocumentBase>> GetDocumentAsByteArray(DocumentRequestBase request);
        Task<Response<bool>> UploadDocument(DocumentBase document);
        Task<DocumentUploadResponseDTO> UploadDocumentAsync(DocumentBase documentBase, string tenantCode);
        Task<GetDocumentDTO> GetDocumentFileAsync(string documentId);
        Task<Response<bool>> RemoveDocument(DocumentBase document);
        Task<Response<TenantResponseDto>> CreateTenantAsync(CreateTenantDto createTenantDto);
        Task<ErmsFileResponseDTO?> GetDocumentAsync(string documentId);

    }
}
