using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using ErpOnlineOrder.Application.DTOs;

namespace ErpOnlineOrder.WebMVC.Services
{
    public abstract class BaseApiClient
    {
        protected readonly HttpClient _httpClient;
        protected readonly JsonSerializerOptions _jsonOptions;

        protected BaseApiClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        }

        protected async Task<T?> GetAsync<T>(string url, CancellationToken cancellationToken = default)
        {
            try
            {
                var response = await _httpClient.GetAsync(url, cancellationToken);
                var content = await response.Content.ReadAsStringAsync(cancellationToken);
                
                if (!response.IsSuccessStatusCode)
                {
                    var errorResponse = JsonSerializer.Deserialize<ApiResponse<object>>(content, _jsonOptions);
                    throw new HttpRequestException(errorResponse?.Message ?? $"API returned {response.StatusCode}");
                }

                var apiResponse = JsonSerializer.Deserialize<ApiResponse<T>>(content, _jsonOptions);
                return apiResponse != null && apiResponse.Success ? apiResponse.Data : default;
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi khi kết nối API: {ex.Message}");
            }
        }

        protected async Task<(TOut? Data, string? ErrorMessage)> PostAsync<TIn, TOut>(string url, TIn payload, CancellationToken cancellationToken = default)
        {
            var response = await _httpClient.PostAsJsonAsync(url, payload, _jsonOptions, cancellationToken);
            return await HandleResponseWithDataAsync<TOut>(response, cancellationToken);
        }

        protected async Task<(TOut? Data, string? ErrorMessage)> PostAsync<TOut>(string url, CancellationToken cancellationToken = default)
        {
            var response = await _httpClient.PostAsync(url, null, cancellationToken);
            return await HandleResponseWithDataAsync<TOut>(response, cancellationToken);
        }

        protected async Task<(bool Success, string? ErrorMessage)> PostWithoutReturnAsync<TIn>(string url, TIn payload, CancellationToken cancellationToken = default)
        {
            var response = await _httpClient.PostAsJsonAsync(url, payload, _jsonOptions, cancellationToken);
            return await HandleResponseAsync(response, cancellationToken);
        }

        protected async Task<(bool Success, string? ErrorMessage)> PostWithoutReturnAsync(string url, CancellationToken cancellationToken = default)
        {
            var response = await _httpClient.PostAsync(url, null, cancellationToken);
            return await HandleResponseAsync(response, cancellationToken);
        }

        protected async Task<(bool Success, string? ErrorMessage)> PutAsync<TIn>(string url, TIn payload, CancellationToken cancellationToken = default)
        {
            var response = await _httpClient.PutAsJsonAsync(url, payload, _jsonOptions, cancellationToken);
            return await HandleResponseAsync(response, cancellationToken);
        }

        protected async Task<(bool Success, string? ErrorMessage)> DeleteAsync(string url, CancellationToken cancellationToken = default)
        {
            var response = await _httpClient.DeleteAsync(url, cancellationToken);
            return await HandleResponseAsync(response, cancellationToken);
        }

        protected async Task<(bool Success, string? ErrorMessage)> PatchAsync(string url, CancellationToken cancellationToken = default)
        {
            var response = await _httpClient.PatchAsync(url, null, cancellationToken);
            return await HandleResponseAsync(response, cancellationToken);
        }

        protected async Task<byte[]> GetByteArrayAsync(string url, CancellationToken cancellationToken = default)
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Accept.Clear();
            request.Headers.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"));
            var response = await _httpClient.SendAsync(request, cancellationToken);
            if (!response.IsSuccessStatusCode) return Array.Empty<byte>();
            return await response.Content.ReadAsByteArrayAsync(cancellationToken);
        }

        private async Task<(bool Success, string? ErrorMessage)> HandleResponseAsync(HttpResponseMessage response, CancellationToken cancellationToken = default)
        {
            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            try 
            {
                var apiResponse = JsonSerializer.Deserialize<ApiResponse<object>>(content, _jsonOptions);
                if (response.IsSuccessStatusCode && apiResponse?.Success == true)
                    return (true, null);
                    
                return (false, apiResponse?.Message ?? "Đã có lỗi xảy ra từ máy chủ.");
            }
            catch
            {
                return (response.IsSuccessStatusCode, response.IsSuccessStatusCode ? null : "Định dạng JSON từ máy chủ không hợp lệ.");
            }
        }

        private async Task<(T? Data, string? ErrorMessage)> HandleResponseWithDataAsync<T>(HttpResponseMessage response, CancellationToken cancellationToken = default)
        {
            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            try 
            {
                var apiResponse = JsonSerializer.Deserialize<ApiResponse<T>>(content, _jsonOptions);
                if (response.IsSuccessStatusCode && apiResponse?.Success == true)
                    return (apiResponse.Data, null);
                    
                return (default, apiResponse?.Message ?? "Đã có lỗi xảy ra từ máy chủ.");
            }
            catch
            {
                return (default, "Định dạng JSON từ máy chủ không hợp lệ.");
            }
        }
    }
}