using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace InventoryApp.Web.Services
{
    public class DropboxService
    {
        private readonly IConfiguration _config;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<DropboxService> _logger;

        public DropboxService(
            IConfiguration config,
            IHttpClientFactory httpClientFactory,
            ILogger<DropboxService> logger)
        {
            _config = config;
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        /// <summary>
        /// Gets a valid access token. Uses RefreshToken if configured, otherwise falls back to AccessToken.
        /// </summary>
        private async Task<string> GetAccessTokenAsync()
        {
            var refreshToken = _config["Dropbox:RefreshToken"];
            var appKey = _config["Dropbox:AppKey"];
            var appSecret = _config["Dropbox:AppSecret"];

            if (!string.IsNullOrWhiteSpace(refreshToken) &&
                !string.IsNullOrWhiteSpace(appKey) &&
                !string.IsNullOrWhiteSpace(appSecret))
            {
                var client = _httpClientFactory.CreateClient();
                var body = new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    ["grant_type"]    = "refresh_token",
                    ["refresh_token"] = refreshToken,
                    ["client_id"]     = appKey,
                    ["client_secret"] = appSecret,
                });

                var response = await client.PostAsync(
                    "https://api.dropboxapi.com/oauth2/token", body);

                var json = await response.Content.ReadAsStringAsync();
                if (!response.IsSuccessStatusCode)
                    throw new InvalidOperationException($"Dropbox token refresh failed: {json}");

                using var doc = JsonDocument.Parse(json);
                return doc.RootElement.GetProperty("access_token").GetString()
                    ?? throw new InvalidOperationException("No access_token in Dropbox refresh response.");
            }

            // Fallback: use static access token from config
            return _config["Dropbox:AccessToken"]
                ?? throw new InvalidOperationException("Dropbox is not configured. Set Dropbox:RefreshToken+AppKey+AppSecret or Dropbox:AccessToken.");
        }

        /// <summary>
        /// Uploads a UTF-8 JSON string as a file to Dropbox.
        /// Returns the path of the uploaded file, or throws on failure.
        /// </summary>
        public async Task<string> UploadJsonAsync(string json, string fileName)
        {
            var accessToken = await GetAccessTokenAsync();

            var folder = _config["Dropbox:UploadFolder"] ?? "/support-tickets";
            var remotePath = $"{folder.TrimEnd('/')}/{fileName}";

            var client = _httpClientFactory.CreateClient("Dropbox");
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", accessToken);

            var apiArg = JsonSerializer.Serialize(new
            {
                path = remotePath,
                mode = "add",
                autorename = true,
                mute = false,
            });
            client.DefaultRequestHeaders.Add("Dropbox-API-Arg", apiArg);

            var content = new ByteArrayContent(Encoding.UTF8.GetBytes(json));
            content.Headers.ContentType =
                new MediaTypeHeaderValue("application/octet-stream");

            var response = await client.PostAsync(
                "https://content.dropboxapi.com/2/files/upload", content);

            var body = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Dropbox upload failed ({Status}): {Body}",
                    response.StatusCode, body);
                throw new InvalidOperationException(
                    $"Dropbox upload failed ({response.StatusCode}): {body}");
            }

            _logger.LogInformation("Ticket uploaded to Dropbox: {Path}", remotePath);
            return remotePath;
        }
    }
}
