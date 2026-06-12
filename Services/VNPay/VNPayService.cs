using System.Net;
using System.Security.Cryptography;
using System.Text;

namespace ToursAndTravelsManagement.Services.VNPay;

public class VNPayService
{
    private readonly IConfiguration _configuration;

    public VNPayService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string CreatePaymentUrl(
        HttpContext context,
        int bookingId,
        decimal amount)
    {
        var tmnCode = _configuration["VNPay:TmnCode"];
        var hashSecret = _configuration["VNPay:HashSecret"];
        var baseUrl = _configuration["VNPay:BaseUrl"];
        var returnUrl = _configuration["VNPay:ReturnUrl"];

        var pay = new VnPayLibrary();

        pay.AddRequestData("vnp_Version", "2.1.0");
        pay.AddRequestData("vnp_Command", "pay");
        pay.AddRequestData("vnp_TmnCode", tmnCode!);

        pay.AddRequestData(
            "vnp_Amount",
            ((long)(amount * 100)).ToString());

        pay.AddRequestData(
            "vnp_CreateDate",
            DateTime.UtcNow.ToString("yyyyMMddHHmmss"));

        pay.AddRequestData("vnp_CurrCode", "VND");

        pay.AddRequestData(
            "vnp_IpAddr",
            context.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1");

        pay.AddRequestData("vnp_Locale", "vn");

        pay.AddRequestData(
            "vnp_OrderInfo",
            $"Thanh toan booking {bookingId}");

        pay.AddRequestData("vnp_OrderType", "other");

        pay.AddRequestData("vnp_ReturnUrl", returnUrl!);

        pay.AddRequestData(
            "vnp_TxnRef",
            DateTime.UtcNow.Ticks.ToString());

        return pay.CreateRequestUrl(baseUrl!, hashSecret!);
    }

    public bool ValidateSignature(IQueryCollection queryCollection)
    {
        var hashSecret = _configuration["VNPay:HashSecret"];

        var vnpayData = queryCollection
            .Where(x => x.Key.StartsWith("vnp_"))
            .ToDictionary(
                x => x.Key,
                x => x.Value.ToString());

        var secureHash =
            queryCollection["vnp_SecureHash"].ToString();

        vnpayData.Remove("vnp_SecureHash");
        vnpayData.Remove("vnp_SecureHashType");

        var signData = string.Join("&",
            vnpayData
                .OrderBy(x => x.Key)
                .Select(x =>
                    $"{x.Key}={WebUtility.UrlEncode(x.Value)}"));

        var checkHash =
            HmacSHA512(hashSecret!, signData);

        return checkHash.Equals(
            secureHash,
            StringComparison.InvariantCultureIgnoreCase);
    }

    private string HmacSHA512(
        string key,
        string inputData)
    {
        var hash = new StringBuilder();

        byte[] keyBytes = Encoding.UTF8.GetBytes(key);
        byte[] inputBytes = Encoding.UTF8.GetBytes(inputData);

        using var hmac = new HMACSHA512(keyBytes);

        byte[] hashValue = hmac.ComputeHash(inputBytes);

        foreach (var theByte in hashValue)
        {
            hash.Append(theByte.ToString("x2"));
        }

        return hash.ToString();
    }
}

public class VnPayLibrary
{
    private readonly SortedList<string, string> _requestData =
        new(StringComparer.Ordinal);

    public void AddRequestData(
        string key,
        string value)
    {
        if (!string.IsNullOrEmpty(value))
        {
            _requestData.Add(key, value);
        }
    }

    public string CreateRequestUrl(
        string baseUrl,
        string hashSecret)
    {
        var data = new StringBuilder();

        foreach (var kv in _requestData)
        {
            if (data.Length > 0)
            {
                data.Append("&");
            }

            data.Append(WebUtility.UrlEncode(kv.Key));
            data.Append("=");
            data.Append(WebUtility.UrlEncode(kv.Value));
        }

        var queryString = data.ToString();

        var secureHash =
            HmacSHA512(hashSecret, queryString);

        return $"{baseUrl}?{queryString}&vnp_SecureHash={secureHash}";
    }

    private string HmacSHA512(
        string key,
        string inputData)
    {
        var hash = new StringBuilder();

        byte[] keyBytes = Encoding.UTF8.GetBytes(key);
        byte[] inputBytes = Encoding.UTF8.GetBytes(inputData);

        using var hmac = new HMACSHA512(keyBytes);

        byte[] hashValue = hmac.ComputeHash(inputBytes);

        foreach (var theByte in hashValue)
        {
            hash.Append(theByte.ToString("x2"));
        }

        return hash.ToString();
    }
}