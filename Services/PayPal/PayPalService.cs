using System.Net.Http.Headers;
using System.Text;
using Newtonsoft.Json;

namespace ToursAndTravelsManagement.Services.PayPal;

public class PayPalService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;

    public PayPalService(
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
    }

    public async Task<string?> CreateOrderAsync(
        int bookingId,
        decimal amount)
    {
        var accessToken = await GetAccessTokenAsync();

        var client = _httpClientFactory.CreateClient();

        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", accessToken);

        var requestBody = new
        {
            intent = "CAPTURE",
            purchase_units = new[]
            {
                new
                {
                    reference_id = bookingId.ToString(),
                    amount = new
                    {
                        currency_code = "USD",
                        value = amount.ToString("0.00")
                    }
                }
            },
            application_context = new
            {
                return_url =
                    "https://vivavivu-travel.onrender.com/Payment/PayPalSuccess",

                cancel_url =
                    "https://vivavivu-travel.onrender.com/Payment/PayPalCancel"
            }
        };

        var content = new StringContent(
            JsonConvert.SerializeObject(requestBody),
            Encoding.UTF8,
            "application/json");

        var response = await client.PostAsync(
            $"{_configuration["PayPal:BaseUrl"]}/v2/checkout/orders",
            content);

        response.EnsureSuccessStatusCode();

        var json =
            await response.Content.ReadAsStringAsync();

        dynamic result =
            JsonConvert.DeserializeObject(json)!;

        foreach (var link in result.links)
        {
            if (link.rel == "approve")
            {
                return link.href;
            }
        }

        return null;
    }

    public async Task<string> GetAccessTokenAsync()
    {
        var client = _httpClientFactory.CreateClient();

        var clientId =
            _configuration["PayPal:ClientId"];

        var clientSecret =
            _configuration["PayPal:ClientSecret"];

        var authToken =
            Convert.ToBase64String(
                Encoding.UTF8.GetBytes(
                    $"{clientId}:{clientSecret}"));

        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue(
                "Basic",
                authToken);

        var content =
            new FormUrlEncodedContent(
                new Dictionary<string, string>
                {
                    { "grant_type", "client_credentials" }
                });

        var response = await client.PostAsync(
            $"{_configuration["PayPal:BaseUrl"]}/v1/oauth2/token",
            content);

        response.EnsureSuccessStatusCode();

        var json =
            await response.Content.ReadAsStringAsync();

        dynamic token =
            JsonConvert.DeserializeObject(json)!;

        return token.access_token;
    }

    public async Task CaptureOrderAsync(string orderId)
    {
        var accessToken =
            await GetAccessTokenAsync();

        var client =
            _httpClientFactory.CreateClient();

        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                accessToken);

        var content = new StringContent(
            "{}",
            Encoding.UTF8,
            "application/json");

        var response = await client.PostAsync(
            $"{_configuration["PayPal:BaseUrl"]}/v2/checkout/orders/{orderId}/capture",
            content);

        response.EnsureSuccessStatusCode();
    }
}