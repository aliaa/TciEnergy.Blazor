using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using TciEnergy.Blazor.Shared.Utils;

namespace TciEnergy.Blazor.Client
{
    public class HttpClientX : HttpClient
    {
        static readonly JsonSerializerOptions jsonOptions;
        static HttpClientX()
        {
            jsonOptions = new JsonSerializerOptions();
            jsonOptions.Converters.Add(new ObjectIdJsonConverter());
            jsonOptions.Converters.Add(new JsonStringEnumConverter());
            jsonOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        }

        private readonly NavigationManager nav;
        private readonly IJSRuntime js;

        public HttpClientX(NavigationManager nav, IJSRuntime js)
        {
            this.nav = nav;
            this.js = js;
        }

        public async Task<T> GetFromJsonAsync<T>(string requestUri, CancellationToken cancellationToken = default)
        {
            HttpResponseMessage resp;
            try
            {
                resp = await GetAsync(requestUri, cancellationToken);
            }
            catch (Exception ex)
            {
                throw new Exception("Errr on GET", ex);
            }
            if (resp.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                nav.NavigateTo("/login/out");
                return default;
            }
            else if ((int)resp.StatusCode >= 400)
                throw await CreateHttpResponseException(resp);
            return await resp.Content.ReadFromJsonAsync<T>(jsonOptions);
        }

        public async Task<HttpResponseMessage> PostAsJsonAsync<T>(string requestUri, T value, CancellationToken cancellationToken = default)
        {
            JsonContent content = JsonContent.Create(value, mediaType: null, jsonOptions);
            HttpResponseMessage resp;
            try
            {
                resp = await PostAsync(requestUri, content, cancellationToken);
            }
            catch (Exception ex)
            {
                throw new Exception("Errr on POST", ex);
            }
            if (resp.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                nav.NavigateTo("/login");
            else if ((int)resp.StatusCode >= 400)
                throw await CreateHttpResponseException(resp);
            return resp;
        }

        public async Task<Res> PostAsJsonAsync<Req, Res>(string requestUri, Req value, CancellationToken cancellationToken = default)
        {
            JsonContent content = JsonContent.Create(value, mediaType: null, jsonOptions);
            HttpResponseMessage resp;
            try
            {
                resp = await PostAsync(requestUri, content, cancellationToken);
            }
            catch (Exception ex)
            {
                throw new Exception("Errr on POST 2", ex);
            }

            if (resp.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                nav.NavigateTo("/login");
                return default;
            }
            else if ((int)resp.StatusCode >= 400)
                throw await CreateHttpResponseException(resp);
            return await resp.Content.ReadFromJsonAsync<Res>(jsonOptions);
        }

        public async Task DownloadFile(string url)
        {
            var currentUri = nav.Uri;
            await js.InvokeVoidAsync("downloadFile", url);
            nav.NavigateTo(currentUri);
        }

        private async Task<HttpResponseException> CreateHttpResponseException(HttpResponseMessage resp)
        {
            var content = await resp.Content.ReadAsStringAsync();
            Dictionary<string, List<string>> errors;
            try
            {
                errors = JsonSerializer.Deserialize<Dictionary<string, List<string>>>(content, jsonOptions);
            }
            catch
            {
                return new HttpResponseException((int)resp.StatusCode, null, content);
            }
            return new HttpResponseException((int)resp.StatusCode, errors, content);
        }

        [Serializable]
        public class HttpResponseException : Exception
        {
            public int StatusCode { get; private set; } = -1;
            public Dictionary<string, List<string>> Errors { get; private set; }
            public HttpResponseException(int StatusCode, Dictionary<string, List<string>> Errors, string Message = null) : base(Message)
            {
                this.StatusCode = StatusCode;
                this.Errors = Errors;
            }

            public HttpResponseException() { }
            public HttpResponseException(string message) : base(message) { }
            public HttpResponseException(string message, Exception innerException) : base(message, innerException) { }

            protected HttpResponseException(System.Runtime.Serialization.SerializationInfo serializationInfo, System.Runtime.Serialization.StreamingContext streamingContext) 
                : base(serializationInfo, streamingContext) { }
        }

        public async Task<Res> UploadFile<Res>(string requestUri, Stream stream, string fileName)
        {
            var content = new MultipartFormDataContent();
            content.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data");
            content.Add(new StreamContent(stream, Convert.ToInt32(stream.Length)), "file", fileName);

            HttpResponseMessage resp;
            try
            {
                resp = await PostAsync(requestUri, content);
            }
            catch(Exception ex)
            {
                throw new Exception("Errr on UploadFile", ex);
            }
            if (resp.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                nav.NavigateTo("/login");
                return default;
            }
            else if ((int)resp.StatusCode >= 400)
                throw await CreateHttpResponseException(resp);
            return await resp.Content.ReadFromJsonAsync<Res>(jsonOptions);
        }
    }
}
