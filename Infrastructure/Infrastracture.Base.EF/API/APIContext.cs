using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Net.Http;
using System.Text.Json.Serialization;
using System.Text.Json;
using Microsoft.AspNetCore.Http;

namespace Infrastracture.Base.EF.API
{
    public class ApiContext
    {
        protected HttpClient Client;
        private readonly HttpClientProperty httpClientProperty;
        IHttpContextAccessor httpContextAccessor;
        private string BaseUrl { get; set; }

        public ApiContext(string baseUrl, IHttpContextAccessor httpContextAccessor)
        {
            this.httpContextAccessor = httpContextAccessor;
            this.BaseUrl = baseUrl;
            this.Client = new HttpClient();
            this.Client.DefaultRequestHeaders.Add("If-Modified-Since", DateTime.UtcNow.ToString("r")); // disable cashing
            this.Client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            Client.BaseAddress = new Uri(BaseUrl);
        }
        public ApiContext(HttpClientProperty httpClientProperty)
        {
            this.httpClientProperty = httpClientProperty;

            Client = new HttpClient();
            Client.DefaultRequestHeaders.Add("If-Modified-Since", DateTime.UtcNow.ToString("r")); // disable cashing
            Client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            Client.BaseAddress = new Uri(BaseUrl);
            //            SetDefaultHeaders(Client);
        }
        protected async Task<ResultModel<TOutbound>> Put<TInbound, TOutbound>(TInbound data, string id, string controller)
        {
            StringContent content = new StringContent(JsonSerializer.Serialize(data), Encoding.UTF8, "application/json");
            HttpResponseMessage result = await Client.PutAsync(controller + "/" + id, content);

            if (result.IsSuccessStatusCode)
            {
                return new ResultModel<TOutbound>()
                {
                    Success = true,
                    Value = JsonSerializer.Deserialize<TOutbound>(await result.Content.ReadAsStringAsync())
                };
            }

            var error = new ResultModel<TOutbound>()
            {
                Error = result.StatusCode.ToString(),
                Message = result.ReasonPhrase,
                Success = false
            };

            return error;
        }

        protected async Task<HttpResponseMessage> Post<TInbound>(TInbound data, string controller) where TInbound : class
        {
            try
            {
                var options = new JsonSerializerOptions();
                options.Converters.Add(new JsonStringEnumConverter());
                options.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
                string seralizedData=string.Empty;
                InjectAuthorizationHeader("");
                if (data != null)
                 seralizedData = JsonSerializer.Serialize(data, options);

                StringContent content = new StringContent(seralizedData, Encoding.UTF8, "application/json");
                HttpResponseMessage result = await Client.PostAsync(controller, content);

                return result;
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        protected async Task<ResultModel<TOutbound>> Post<TInbound, TOutbound>(TInbound data, string controller) where TOutbound : class
        {
            try
            {
                var options = new JsonSerializerOptions();
                options.Converters.Add(new JsonStringEnumConverter());
                //options.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;


                string seralizedData = JsonSerializer.Serialize(data, options);

                StringContent content = new StringContent(seralizedData, Encoding.UTF8, "application/json");
                HttpResponseMessage result = await Client.PostAsync(controller, content);

                if (result.IsSuccessStatusCode)
                {
                    Task<string> response = Task.Run(() => result.Content.ReadAsStringAsync().Result);

                    try
                    {
                        var value = JsonSerializer.Deserialize<TOutbound>(response.Result, options);

                        return new ResultModel<TOutbound>()
                        {
                            Success = true,
                            Value = value,
                            Message = response.Result
                        };
                    }
                    catch (Exception ex)
                    {
                        throw;
                    }

                    return new ResultModel<TOutbound>()
                    {
                        Success = false,
                        //                    Value = value,
                        Message = response.Result
                    };
                }
                else
                {
                    Task<string> response = Task.Run(() => result.Content.ReadAsStringAsync().Result);
                    var error = new ResultModel<TOutbound>()
                    {
                        Error = result.StatusCode.ToString(),
                        Message = response.Result,
                        Success = false
                    };

                    return error;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        protected async Task<ResultModel<TOutbound>> Send<TInbound, TOutbound>(string headers, string url, string token, string body)
        {
            try
            {
                string headersParam = headers;
                string tokenParam = token;

                using (HttpClient client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, url)
                    {
                        Content = new StringContent(body)
                    };

                    using (HttpResponseMessage response = await client.SendAsync(request))
                    {
                        if (response.IsSuccessStatusCode)
                        {
                            return new ResultModel<TOutbound>()
                            {
                                Success = true,
                                Value = JsonSerializer.Deserialize<TOutbound>(await response.Content.ReadAsStringAsync())
                            };
                        }

                        return new ResultModel<TOutbound>()
                        {
                            Error = response.StatusCode.ToString(),
                            Message = response.ReasonPhrase,
                            Success = false
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                return new ResultModel<TOutbound>()
                {
                    Error = ex.Message,
                    Message = "Exception",
                    Success = false
                };
            }
        }
        public async void InjectAuthorizationHeader(string token)
        {
            if (string.IsNullOrEmpty(token))
                token = await GetTokenFromRequest();

            if (!string.IsNullOrEmpty(token))
                Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        public async Task<string> GetTokenFromRequest()
        {
            var authorizationHeader = httpContextAccessor.HttpContext.Request.Headers["Authorization"];
            if (authorizationHeader.Count > 0)
            {
                var token = authorizationHeader[0].Replace("Bearer ", "");
                return token;
            }

            return null; // Or handle missing token appropriately
        }
    }
}
