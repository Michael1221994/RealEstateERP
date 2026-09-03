using Infrastracture.Base.Contracts;
using Infrastracture.Base.DTOs.ERMS;
using Infrastracture.Base.EF.ERMS;
using Infrastracture.Base.Helpers;
using iText.StyledXmlParser.Jsoup.Nodes;
using Minio;
using Minio.DataModel;
using Minio.DataModel.Args;
using Minio.Exceptions;
using SES.WINSSAS.Common;
using SES.WINSSAS.Common.Enums;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.AccessControl;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
namespace Infrastracture.Base.EF.Minio
{
    public class MinioDocumentService : IDocumentService
    {
        IMinioClient client;
        IErmsApiClient _ermsApiClient;

        /// <summary>
        /// Used to download presigned object URLs. Static so the service - which is registered both
        /// scoped and singleton - does not churn through sockets.
        /// </summary>
        private static readonly HttpClient Http = new HttpClient();

        private const int PresignedUrlExpirySeconds = 300;

        public MinioDocumentService(IMinioClient client, IErmsApiClient ermsApiClient)
        {
            this.client = client;
            _ermsApiClient = ermsApiClient;
        }

        public async Task<Response<bool>> UploadDocument(DocumentBase document)
        {
            try
            {
                await this.CreateBucketIfNotExists(document.Bucket);

                await this.RemoveDocument(document);

                var documentName = string.IsNullOrEmpty(document.Folder) ? document.DocumentIdentifier.ToString() + document.Extension :
                                document.Folder + "/" + document.DocumentIdentifier.ToString() + document.Extension;
                using (var memoryStream = new MemoryStream(document.Content))
                {
                    // Set object metadata (optional)
                    var putObjectArgs = new PutObjectArgs()
                                        .WithBucket(document.Bucket)
                                        .WithObject(documentName)
                                        .WithStreamData(memoryStream)
                                        .WithContentType(document.ContentType)
                                        .WithObjectSize(memoryStream.Length);

                    if (document.Metadata.Count > 0)
                        putObjectArgs.WithHeaders(document.Metadata);

                    var response = await client.PutObjectAsync(putObjectArgs).ConfigureAwait(false);

                    return new Response<bool>(ResponseStatus.Success, true, "Document uploaded");
                }
            }
            catch (Exception ex)
            {
                return new Response<bool>(ResponseStatus.Error, false, ex.Message, ex);
            }
        }

        public async Task<Response<DocumentBase>> GetDocumentAsByteArray(DocumentRequestBase request)
        {
            var objectName = request.DocumentIdentifier.ToString() + request.Extension;

            try
            {
                // Do not use StatObjectAsync/GetObjectAsync here. Both issue a HEAD, and the CDN in
                // front of this endpoint answers HEAD with 403 for any key whose extension it treats as
                // a cacheable asset (.pdf, .jpg, ...), whether or not the object exists. Minio 6.0.3
                // does not handle that response: it either throws a NullReferenceException or builds a
                // bogus ObjectStat (499 bytes, "application/xml") and then streams the S3 error
                // document as if it were the file. Plain GET is unaffected, so presign the object -
                // which is signed locally, without any round trip - and fetch it over HTTP.
                var url = await this.client.PresignedGetObjectAsync(new PresignedGetObjectArgs()
                    .WithBucket(request.Bucket)
                    .WithObject(objectName)
                    .WithExpiry(PresignedUrlExpirySeconds));

                using (var httpResponse = await Http.GetAsync(url).ConfigureAwait(false))
                {
                    if (httpResponse.StatusCode == System.Net.HttpStatusCode.NotFound)
                    {
                        return new Response<DocumentBase>(
                            ResponseStatus.NotFound,
                            null,
                            $"Document '{request.Bucket}/{objectName}' not found.");
                    }

                    if (!httpResponse.IsSuccessStatusCode)
                    {
                        return new Response<DocumentBase>(
                            ResponseStatus.Error,
                            null,
                            $"Document '{request.Bucket}/{objectName}' could not be retrieved (HTTP {(int)httpResponse.StatusCode}).");
                    }

                    var content = await httpResponse.Content.ReadAsByteArrayAsync().ConfigureAwait(false);

                    DocumentBase document = new DocumentBase
                    {
                        DocumentIdentifier = request.DocumentIdentifier,
                        Bucket = request.Bucket,
                        Extension = Path.GetExtension(objectName),
                        Content = content,
                        ContentType = httpResponse.Content.Headers.ContentType?.MediaType
                                      ?? ResolveContentType(objectName)
                    };

                    return new Response<DocumentBase>(ResponseStatus.Success, document, "");
                }
            }
            catch (ObjectNotFoundException)
            {
                return new Response<DocumentBase>(
                    ResponseStatus.NotFound,
                    null,
                    $"Document '{request.Bucket}/{objectName}' not found.");
            }
            catch (Exception ex)
            {
                return new Response<DocumentBase>(ResponseStatus.Error, null, ex.Message, ex);
            }
        }

        /// <summary>
        /// Fallback content type for an object key, used only when the storage response omits one.
        /// Documents the system generates itself are stored without an extension and are always PDFs.
        /// </summary>
        private static string ResolveContentType(string objectName)
        {
            var extension = Path.GetExtension(objectName);
            if (string.IsNullOrEmpty(extension))
                return "application/pdf";

            switch (extension.ToLowerInvariant())
            {
                case ".pdf": return "application/pdf";
                case ".jpg":
                case ".jpeg": return "image/jpeg";
                case ".png": return "image/png";
                case ".gif": return "image/gif";
                case ".csv": return "text/csv";
                case ".txt": return "text/plain";
                case ".xlsx": return "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                case ".xls": return "application/vnd.ms-excel";
                case ".docx": return "application/vnd.openxmlformats-officedocument.wordprocessingml.document";
                case ".doc": return "application/msword";
                default: return "application/octet-stream";
            }
        }
        public async Task<Response<bool>> RemoveDocument(DocumentBase document)
        {
            try
            {
                var args = new RemoveObjectArgs()
                              .WithBucket(document.Bucket)
                              .WithObject(document.DocumentIdentifier.ToString());

                await client.RemoveObjectAsync(args).ConfigureAwait(false);
                return new Response<bool>(ResponseStatus.Success, true, "Document removed");
            }
            catch (ObjectNotFoundException ex)
            {
                return new Response<bool>(ResponseStatus.Success, true, "Document removed");
            }
            catch (Exception ex)
            {
                return new Response<bool>(ResponseStatus.Error, false, ex.Message, ex);
            }
        }

        private async Task CreateBucketIfNotExists(string bucketName)
        {
            var beArgs = new BucketExistsArgs()
               .WithBucket(bucketName);
            if (!await this.client.BucketExistsAsync(beArgs))
            {
                var mbArgs = new MakeBucketArgs()
                       .WithBucket(bucketName);
                await this.client.MakeBucketAsync(mbArgs);
            }
        }



        public async Task<ErmsDocumentDTO?> GetDocumentInfoAsync(string documentId)
        {
            ErmsDocumentDTO? documentInfo = await _ermsApiClient.GetDocumentInfoAsync(documentId);

            if (documentInfo == null)
                throw new Exception("Document not found in ERMS");

            return documentInfo;
        }
        public async Task<ErmsFileResponseDTO?> GetDocumentAsync(string documentId)
        {
            ErmsFileResponseDTO? documentInfo = await _ermsApiClient.GetDocumentFileAsync(documentId);
            if (documentInfo == null)
                throw new Exception("Document type not found in ERMS");
            return documentInfo;
        }
        public async Task<GetDocumentDTO> GetDocumentFileAsync(string documentId)
        {
            var fileResult = await _ermsApiClient.GetDocumentFileAsync(documentId);
            GetDocumentDTO document = new GetDocumentDTO();
            if (fileResult == null)
                throw new Exception("Document file not found");

            byte[] fileBytes = fileResult.Buffer.Data.ToArray();
            string contentType = fileResult.ContentType;
            document.content = fileBytes;
            document.fileType = contentType;
            return document;
        }
     
        public async Task<IReadOnlyList<string>> GetRequiredMetadataAsync(string documentType)
        {
            var documentTypeWithMetadata = await _ermsApiClient.GetDocumentTypeByCodeAsync(
                documentType);

            if (documentTypeWithMetadata == null)
                throw new ArgumentException($"Document type {documentType} not found in ERMS");

            return documentTypeWithMetadata.MetadataFields
                .Select(mf => mf.Field)
                .ToList()
                .AsReadOnly();
        }

        public async Task<IReadOnlyList<ErmsMetaDataFieldDTO>> GetMetadataDetailsAsync(string documentType)
        {
            var documentTypeWithMetadata = await _ermsApiClient.GetDocumentTypeByCodeAsync(
                documentType);

            if (documentTypeWithMetadata == null)
                throw new ArgumentException($"Document type {documentType} not found in ERMS");

            return documentTypeWithMetadata.MetadataFields.AsReadOnly();
        }

        public async Task<DocumentUploadResponseDTO> UploadDocumentAsync(DocumentBase documentBase, string tenantCode)
        {

            var formData = new MultipartFormDataContent();

            var fileContent = new ByteArrayContent(documentBase.Content);
            fileContent.Headers.ContentType = new MediaTypeHeaderValue(documentBase.ContentType);
            string ermsCode = EnumHelper.GetDocumentTypeCode(documentBase.DocumentTypeEnum);

            formData.Add(fileContent, "file", documentBase.DocumentIdentifier + documentBase.Extension);
            formData.Add(new StringContent(tenantCode), "tenantCode");
            formData.Add(new StringContent(documentBase.DocumentIdentifier.ToString()), "title");
            //formData.Add(new StringContent($"Document for case {caseEntity.CaseNumber}"), "description");
            formData.Add(new StringContent(ermsCode), "documentType");
            //formData.Add(new StringContent(null), "source[userId]");
            formData.Add(new StringContent("system-generated"), "source[ipAddress]");
            formData.Add(new StringContent("registered-application"), "source[type]"); 

            int index = 0;
            foreach (var kvp in documentBase.Metadata)
            {
                formData.Add(new StringContent(kvp.Key), $"metadata[{index}][field]");
                var val = kvp.Value?.ToString() ?? "";
                formData.Add(new StringContent(val), $"metadata[{index}][value]");

                index++;
            }

            var response = await _ermsApiClient.UploadDocumentAsync(formData);
            return response;
        }

        public async Task SetMetadataFields(DocumentBase documentBase)
        {
            IReadOnlyList<string> requiredMetadataList = await GetRequiredMetadataAsync(documentBase.DocumentTypeEnum.ToString());

            foreach (var field in requiredMetadataList)
            {
                documentBase.Metadata.Add(field, null);
            }
        }
        public async Task<Response<TenantResponseDto>> CreateTenantAsync(CreateTenantDto createTenantDto)
        {
            try
            {
                // Call the client method implemented in the previous step
                var result = await _ermsApiClient.CreateTenantAsync(createTenantDto);

                if (result == null)
                {
                    return new Response<TenantResponseDto>(
                        ResponseStatus.Error,
                        null,
                        "Received empty response from ERMS API."
                    );
                }

                return new Response<TenantResponseDto>(
                    ResponseStatus.Success,
                    result,
                    "Tenant created successfully."
                );
            }
            catch (Exception ex)
            {
                // Follows your existing pattern of catching exceptions and returning them in the Response object
                return new Response<TenantResponseDto>(
                    ResponseStatus.Error,
                    null,
                    ex.Message,
                    ex
                );
            }
        }

    }

    }
