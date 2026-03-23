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
        /// Uploads a UTF-8 JSON string as a file to Dropbox.
        /// Returns the path of the uploaded file, or throws on failure.
        /// </summary>
        public async Task<string> UploadJsonAsync(string json, string fileName)
        {
            var accessToken = _config["Dropbox:AccessToken"]
                ?? throw new InvalidOperationException("Dropbox:AccessToken is not configured.");

            var folder = _config["Dropbox:UploadFolder"] ?? "/support-tickets";
            var remotePath = $"{folder.TrimEnd('/')}/{fileName}";

            var client = _httpClientFactory.CreateClient("Dropbox");
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", accessToken);

            // Dropbox upload metadata goes in a header as JSON
            var apiArg = JsonSerializer.Serialize(new
            {
                path = remotePath,
                mode = "add",
                autorename = true,
                mute = false,
            });
            client.DefaultRequestHeaders.Add("Dropbox-API-Arg", apiArg);
            // client.DefaultRequestHeaders.Add("Content-Type", "application/octet-stream");

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
