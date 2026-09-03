using Infrastracture.Base.EF.API;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Infrastracture.Base.EF.Comm
{
    public class SMSGatwaySetting : APISetting
    {
        public string SendEndPoint { get; set; }
        public string BearerToken { get; set; }
    }
    public class SMSAPI : ISMSAPI
    {
        readonly SMSGatwaySetting setting;
        readonly bool isConfigured;
        public SMSAPI(IOptions<SMSGatwaySetting> settings)
        {
            this.setting = settings?.Value ?? new SMSGatwaySetting();
            // The gateway is optional. When it is left as an empty placeholder we skip
            // sending instead of throwing, so resolving this service can never fail.
            this.isConfigured = Uri.TryCreate(this.setting.URL, UriKind.Absolute, out _);
        }

        public async Task<Response<bool>> SendSMS(string phone, string msg, Guid transactionId)
        {
            if (!this.isConfigured)
            {
                return new Response<bool>(ResponseStatus.Warning, false, "SMS gateway is not configured; SMS skipped.");
            }

            var payload = new
            {
                to = phone,
                text = msg,
            };

            var content = new StringContent(
                JsonSerializer.Serialize(payload),
                Encoding.UTF8,
                "application/json"
            );

            try
            {
                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Authorization =
                        new AuthenticationHeaderValue("Bearer", this.setting.BearerToken);
                    client.DefaultRequestHeaders.Accept.Add(
                        new MediaTypeWithQualityHeaderValue("application/json"));

                    var response = await client.PostAsync(this.setting.URL, content);

                    if (response.IsSuccessStatusCode)
                    {
                        var responseContent = await response.Content.ReadAsStringAsync();
                        var result = JsonSerializer.Deserialize<ApiResponse>(responseContent);

                        if (result?.Success == true)
                        {
                            return new Response<bool> { Data = true };
                        }
                        else
                        {
                            return Response<bool>.Error("API returned success but operation failed");
                        }
                    }
                    else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    {
                        return Response<bool>.Error("Authentication failed - invalid token");
                    }
                    else
                    {
                        var errorContent = await response.Content.ReadAsStringAsync();
                        return Response<bool>.Error($"Unable to Send Message: {response.StatusCode} - {errorContent}");
                    }
                }
            }
            catch (Exception ex)
            {
                return Response<bool>.Error(ex.Message, ex);
            }

        }
        public class ApiResponse
        {
            [JsonPropertyName("success")]
            public bool Success { get; set; }

            [JsonPropertyName("messageId")]
            public string? MessageId { get; set; }

            [JsonPropertyName("smppMessageId")]
            public string? SmppMessageId { get; set; }

            [JsonPropertyName("status")]
            public string? Status { get; set; }

            [JsonPropertyName("error")]
            public string? Error { get; set; }
        }
    }
}
