using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace InventoryApp.Web.Services
{
    public class SalesforceContactRequest
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Company { get; set; } = string.Empty;
        public string Industry { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
    }

    public class SalesforceService
    {
        private readonly IConfiguration _config;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<SalesforceService> _logger;

        private string? _accessToken;
        private string? _instanceUrl;

        public SalesforceService(
            IConfiguration config,
            IHttpClientFactory httpClientFactory,
            ILogger<SalesforceService> logger)
        {
            _config = config;
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        /// <summary>
        /// Authenticates with Salesforce using the Username-Password OAuth 2.0 flow.
        /// </summary>
        private async Task AuthenticateAsync()
        {
            var clientId = _config["Salesforce:ClientId"]!;
            var clientSecret = _config["Salesforce:ClientSecret"]!;
            // var username = _config["Salesforce:Username"]!;
            // var password = _config["Salesforce:Password"]!;
            // var securityToken = _config["Salesforce:SecurityToken"] ?? string.Empty;
            // var loginUrl = _config["Salesforce:LoginUrl"] ?? "https://login.salesforce.com";
            var loginUrl = _config["Salesforce:LoginUrl"]!;
            var client = _httpClientFactory.CreateClient("Salesforce");

            var content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["grant_type"] = "client_credentials", // FIXED
                ["client_id"] = clientId,
                ["client_secret"] = clientSecret,
                // ["username"] = username,
                // ["password"] = password + securityToken,
            });

            var response = await client.PostAsync(
                $"{loginUrl}/services/oauth2/token", content);

            var body = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Salesforce auth failed: {Body}", body);
                throw new InvalidOperationException($"Salesforce authentication failed: {body}");
            }

            var json = JsonDocument.Parse(body).RootElement;
            _accessToken = json.GetProperty("access_token").GetString();
            // _instanceUrl = json.GetProperty("instance_url").GetString();
             _instanceUrl = _config["Salesforce:InstanceUrl"];
        }

        /// <summary>
        /// Creates an Account and a linked Contact in Salesforce for the given user info.
        /// Returns (accountId, contactId) on success.
        /// </summary>
        public async Task<(string AccountId, string ContactId)> CreateAccountAndContactAsync(
            SalesforceContactRequest req)
        {
            await AuthenticateAsync();

            var client = _httpClientFactory.CreateClient("Salesforce");
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", _accessToken);

            var apiVersion = _config["Salesforce:ApiVersion"] ?? "v59.0";
            var baseApi = $"{_instanceUrl}/services/data/{apiVersion}/sobjects";

            // 1. Create Account
            var accountPayload = JsonSerializer.Serialize(new
            {
                Name = string.IsNullOrWhiteSpace(req.Company) ? $"{req.FirstName} {req.LastName}".Trim() : req.Company,
                Industry = req.Industry,
                BillingCountry = req.Country,
            });

            var accountResponse = await client.PostAsync(
                $"{baseApi}/Account",
                new StringContent(accountPayload, Encoding.UTF8, "application/json"));

            var accountBody = await accountResponse.Content.ReadAsStringAsync();
            if (!accountResponse.IsSuccessStatusCode)
            {
                _logger.LogError("Salesforce Account creation failed: {Body}", accountBody);
                throw new InvalidOperationException($"Failed to create Salesforce Account: {accountBody}");
            }

            var accountJson = JsonDocument.Parse(accountBody).RootElement;
            var accountId = accountJson.GetProperty("id").GetString()!;

            // 2. Create Contact linked to the Account
            var contactPayload = JsonSerializer.Serialize(new
            {
                FirstName = req.FirstName,
                LastName = string.IsNullOrWhiteSpace(req.LastName) ? "Unknown" : req.LastName,
                Email = req.Email,
                Phone = req.Phone,
                AccountId = accountId,
            });

            var contactResponse = await client.PostAsync(
                $"{baseApi}/Contact",
                new StringContent(contactPayload, Encoding.UTF8, "application/json"));

            var contactBody = await contactResponse.Content.ReadAsStringAsync();
            if (!contactResponse.IsSuccessStatusCode)
            {
                _logger.LogError("Salesforce Contact creation failed: {Body}", contactBody);
                throw new InvalidOperationException($"Failed to create Salesforce Contact: {contactBody}");
            }

            var contactJson = JsonDocument.Parse(contactBody).RootElement;
            var contactId = contactJson.GetProperty("id").GetString()!;

            _logger.LogInformation(
                "Salesforce: created Account {AccountId} and Contact {ContactId}",
                accountId, contactId);

            return (accountId, contactId);
        }
    }
}
