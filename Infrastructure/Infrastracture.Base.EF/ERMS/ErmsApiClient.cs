using Infrastracture.Base.Contracts;
using Infrastracture.Base.DTOs.ERMS;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using System;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace Infrastracture.Base.EF.ERMS
{
    public class ErmsApiClient : IErmsApiClient
    {
        private readonly HttpClient _client;
        private readonly IMemoryCache _cache;
        private readonly IConfiguration _config;
        private string _applicationID;

        private const string CacheKey = "ErmsAccessToken";

        public ErmsApiClient(HttpClient client, IMemoryCache cache, IConfiguration config)
        {
            _client = client;
            _cache = cache;
            _config = config;
        }

        private async Task<string> GetAccessTokenAsync(bool forceRefresh = false)
        {
            if (!forceRefresh && _cache.TryGetValue(CacheKey, out string cachedToken))
            {
                return cachedToken;
            }

            var authData = new ErmsAuthRequest
            {
                ClientId = _config["ErmsApi:ClientID"],
                ClientSecret = _config["ErmsApi:ClientSecret"]
            };
            var jsonOptions = new JsonSerializerOptions { PropertyNamingPolicy = null };

            var response = await _client.PostAsJsonAsync("/auth/token", authData, jsonOptions);
            response.EnsureSuccessStatusCode();

            var authResult = await response.Content.ReadFromJsonAsync<ErmsAuthResponse>();

            if (authResult == null || string.IsNullOrEmpty(authResult.AccessToken))
                throw new Exception("Failed to retrieve access token.");

            var expiry = TimeSpan.FromSeconds(authResult.ExpiresIn - 60);
            _cache.Set(CacheKey, authResult.AccessToken, expiry);

            return authResult.AccessToken;
        }

        private async Task<HttpResponseMessage> SendAuthenticatedAsync(Func<Task<HttpResponseMessage>> action)
        {
            string token = await GetAccessTokenAsync();
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await action();

            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                token = await GetAccessTokenAsync(forceRefresh: true);
                _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                response = await action();
            }

            return response;
        }

        public async Task<ErmsDocumentDTO?> GetDocumentInfoAsync(string documentId)
        {
            var response = await SendAuthenticatedAsync(() => _client.GetAsync($"/documents/{documentId}"));
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<ErmsDocumentDTO>();
        }


        public async Task<ErmsFileResponseDTO?> GetDocumentFileAsync(string documentId)
        {
            // Append the required query parameter 'trackDownload' set to false
            var requestUrl = $"/documents/{documentId}/file?trackDownload=false";

            var response = await SendAuthenticatedAsync(() => _client.GetAsync(requestUrl));

            // This will throw an exception if the API still returns 404 or other errors
            response.EnsureSuccessStatusCode();

            var fileResponse = await response.Content.ReadFromJsonAsync<ErmsFileResponseDTO>();

            // Node.js/NestJS serializes a Buffer as an object with a 'Data' property (byte array)
            if (fileResponse?.Buffer?.Data == null)
                throw new Exception("File content is empty or invalid format");

            return fileResponse;
        }


        public async Task<IEnumerable<ErmsDocumentTypeDTO>?> GetDocumentTypesAsync()
        {
            var response = await SendAuthenticatedAsync(() => _client.GetAsync("/document-types"));
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<IEnumerable<ErmsDocumentTypeDTO>>();
        }
        public async Task<ErmsDocumentTypeWithMetadataDTO?> GetDocumentTypeByCodeAsync(string code)
        {
            var response = await SendAuthenticatedAsync(() =>
                _client.GetAsync($"/document-types/code/{code}"));
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<ErmsDocumentTypeWithMetadataDTO>();
        }

        public async Task<ErmsMetaDataFieldDTO?> GetMetadataFieldAsync(string schemaId)
        {
            var response = await SendAuthenticatedAsync(() => _client.GetAsync($"/document-metadata-filed/{schemaId}"));
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<ErmsMetaDataFieldDTO>();
        }

        public async Task<DocumentUploadResponseDTO> UploadDocumentAsync(MultipartFormDataContent multipartFormDataContent)
        {
            var response = await SendAuthenticatedAsync(() => _client.PostAsync("/documents/upload", multipartFormDataContent));

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new Exception($"Upload failed: {response.StatusCode} - {errorContent}");
            }

            var jsonString = await response.Content.ReadAsStringAsync();
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

            return JsonSerializer.Deserialize<DocumentUploadResponseDTO>(jsonString, options);
        }
        public async Task<TenantResponseDto?> CreateTenantAsync(CreateTenantDto createTenantDto)
        {
            // 1. Define camelCase naming policy
            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                PropertyNameCaseInsensitive = true // Good practice for reading response
            };

            // 2. Pass the options into PostAsJsonAsync
            var response = await SendAuthenticatedAsync(() =>
                _client.PostAsJsonAsync("tenants", createTenantDto, options));

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new Exception($"Failed to create tenant: {response.StatusCode} - {errorContent}");
            }

            // 3. Use the same options for deserializing the response
            return await response.Content.ReadFromJsonAsync<TenantResponseDto>(options);
        }
    }

}
