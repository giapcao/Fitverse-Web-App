using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Application.Abstractions.Messaging;
using Microsoft.Extensions.Logging;
using SharedLibrary.Common.ResponseModel;
using SharedLibrary.Contracts.Payments;

namespace Application.Momo.Queries;

public sealed record GetMomoCheckoutUrlQuery(
    Guid PaymentId,
    long AmountVnd,
    string OrderId,
    string OrderInfo,
    Guid UserId,
    PaymentFlow Flow,
    string ClientIp,
    string? ExtraData,
    MomoConfiguration Configuration) : IQuery<MomoCheckoutResponse>;

public sealed record MomoCheckoutResponse(
    string OrderId,
    string RequestId,
    string PayUrl,
    string? Deeplink,
    string? QrCodeUrl,
    int ResultCode,
    string Message,
    string? Signature);

internal sealed class GetMomoCheckoutUrlQueryHandler : IQueryHandler<GetMomoCheckoutUrlQuery, MomoCheckoutResponse>
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<GetMomoCheckoutUrlQueryHandler> _logger;

    public GetMomoCheckoutUrlQueryHandler(
        IHttpClientFactory httpClientFactory,
        ILogger<GetMomoCheckoutUrlQueryHandler> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public async Task<Result<MomoCheckoutResponse>> Handle(GetMomoCheckoutUrlQuery request, CancellationToken cancellationToken)
    {
        var configuration = request.Configuration;
        if (!configuration.IsConfiguredFor(request.Flow))
        {
            _logger.LogWarning("MoMo configuration missing for flow {Flow}.", request.Flow);
            return Result.Failure<MomoCheckoutResponse>(MomoErrors.ConfigurationMissing);
        }

        if (request.AmountVnd <= 0)
        {
            return Result.Failure<MomoCheckoutResponse>(MomoErrors.AmountMustBeGreaterThanZero);
        }

        var orderId = request.OrderId?.Trim();
        if (string.IsNullOrWhiteSpace(orderId))
        {
            return Result.Failure<MomoCheckoutResponse>(MomoErrors.OrderIdRequired);
        }

        var redirectUrl = configuration.GetRedirectUrl(request.Flow)!;
        var extraData = request.ExtraData ?? string.Empty;
        var requestId = MomoHelper.GenerateRequestId(request.PaymentId);

        var rawSignature = MomoHelper.BuildRawSignature(
            configuration.AccessKey,
            request.AmountVnd,
            extraData,
            configuration.IpnUrl,
            orderId,
            request.OrderInfo,
            configuration.PartnerCode,
            redirectUrl,
            requestId,
            configuration.RequestType);

        var signature = MomoHelper.ComputeSignature(rawSignature, configuration.SecretKey);

        var payload = new MomoCreatePaymentRequest
        {
            PartnerCode = configuration.PartnerCode,
            PartnerName = configuration.PartnerName,
            StoreId = configuration.StoreId,
            RequestType = configuration.RequestType,
            IpnUrl = configuration.IpnUrl,
            RedirectUrl = redirectUrl,
            OrderId = orderId,
            OrderInfo = request.OrderInfo,
            Amount = request.AmountVnd,
            Lang = configuration.Lang,
            RequestId = requestId,
            ExtraData = extraData,
            Signature = signature,
            AccessKey = configuration.AccessKey,
            AutoCapture = configuration.AutoCapture,
            IpAddress = request.ClientIp,
            UserInfo = new MomoUserInfo
            {
                UserId = request.UserId.ToString()
            }
        };

        if (!string.IsNullOrWhiteSpace(configuration.ExtraDataTemplate) && string.IsNullOrWhiteSpace(request.ExtraData))
        {
            payload.ExtraData = configuration.ExtraDataTemplate!
                .Replace("{paymentId}", request.PaymentId.ToString(), StringComparison.OrdinalIgnoreCase)
                .Replace("{userId}", request.UserId.ToString(), StringComparison.OrdinalIgnoreCase)
                .Replace("{flow}", request.Flow.ToString(), StringComparison.OrdinalIgnoreCase);
        }

        var client = _httpClientFactory.CreateClient();
        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, configuration.CreatePaymentUrl)
        {
            Content = new StringContent(JsonSerializer.Serialize(payload, SerializerOptions), Encoding.UTF8, "application/json")
        };

        HttpResponseMessage httpResponse;
        try
        {
            httpResponse = await client.SendAsync(httpRequest, cancellationToken);
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            _logger.LogError(ex, "MoMo checkout request failed for order {OrderId}.", orderId);
            return Result.Failure<MomoCheckoutResponse>(MomoErrors.RequestFailed(-1, ex.Message));
        }

        var responseBody = await httpResponse.Content.ReadAsStringAsync();
        if (!httpResponse.IsSuccessStatusCode)
        {
            _logger.LogError(
                "MoMo checkout returned non-success status {StatusCode} for order {OrderId}. Response: {Response}",
                httpResponse.StatusCode,
                orderId,
                responseBody);
            return Result.Failure<MomoCheckoutResponse>(MomoErrors.RequestFailed((int)httpResponse.StatusCode, responseBody));
        }

        var momoResponse = JsonSerializer.Deserialize<MomoCreatePaymentResponse>(responseBody, SerializerOptions);
        if (momoResponse is null)
        {
            _logger.LogError("Unable to deserialize MoMo response for order {OrderId}. Body: {Body}", orderId, responseBody);
            return Result.Failure<MomoCheckoutResponse>(MomoErrors.UnexpectedResponse);
        }

        if (momoResponse.ResultCode != 0)
        {
            _logger.LogWarning(
                "MoMo checkout failed for order {OrderId}. ResultCode: {ResultCode}, Message: {Message}.",
                orderId,
                momoResponse.ResultCode,
                momoResponse.Message ?? momoResponse.LocalMessage);

            return Result.Failure<MomoCheckoutResponse>(
                MomoErrors.RequestFailed(momoResponse.ResultCode, momoResponse.Message ?? momoResponse.LocalMessage));
        }

        if (string.IsNullOrWhiteSpace(momoResponse.PayUrl))
        {
            _logger.LogError("MoMo response missing payUrl for order {OrderId}. Body: {Body}", orderId, responseBody);
            return Result.Failure<MomoCheckoutResponse>(MomoErrors.UnexpectedResponse);
        }

        _logger.LogInformation("MoMo checkout success for order {OrderId} with requestId {RequestId}.", orderId, momoResponse.RequestId ?? requestId);

        var checkoutResponse = new MomoCheckoutResponse(
            momoResponse.OrderId ?? orderId,
            momoResponse.RequestId ?? requestId,
            momoResponse.PayUrl,
            momoResponse.Deeplink,
            momoResponse.QrCodeUrl,
            momoResponse.ResultCode,
            momoResponse.Message ?? momoResponse.LocalMessage ?? "Success",
            momoResponse.Signature);

        return Result.Success(checkoutResponse);
    }

    private sealed class MomoCreatePaymentRequest
    {
        [JsonPropertyName("partnerCode")]
        public string PartnerCode { get; set; } = string.Empty;

        [JsonPropertyName("partnerName")]
        public string? PartnerName { get; set; }

        [JsonPropertyName("storeId")]
        public string? StoreId { get; set; }

        [JsonPropertyName("requestType")]
        public string RequestType { get; set; } = string.Empty;

        [JsonPropertyName("ipnUrl")]
        public string IpnUrl { get; set; } = string.Empty;

        [JsonPropertyName("redirectUrl")]
        public string RedirectUrl { get; set; } = string.Empty;

        [JsonPropertyName("orderId")]
        public string OrderId { get; set; } = string.Empty;

        [JsonPropertyName("orderInfo")]
        public string OrderInfo { get; set; } = string.Empty;

        [JsonPropertyName("amount")]
        public long Amount { get; set; }

        [JsonPropertyName("lang")]
        public string Lang { get; set; } = "vi";

        [JsonPropertyName("requestId")]
        public string RequestId { get; set; } = string.Empty;

        [JsonPropertyName("extraData")]
        public string ExtraData { get; set; } = string.Empty;

        [JsonPropertyName("signature")]
        public string Signature { get; set; } = string.Empty;

        [JsonPropertyName("accessKey")]
        public string AccessKey { get; set; } = string.Empty;

        [JsonPropertyName("autoCapture")]
        public bool AutoCapture { get; set; } = true;

        [JsonPropertyName("ipAddress")]
        public string? IpAddress { get; set; }

        [JsonPropertyName("userInfo")]
        public MomoUserInfo? UserInfo { get; set; }
    }

    private sealed class MomoUserInfo
    {
        [JsonPropertyName("userId")]
        public string? UserId { get; set; }
    }

    private sealed class MomoCreatePaymentResponse
    {
        [JsonPropertyName("orderId")]
        public string? OrderId { get; set; }

        [JsonPropertyName("requestId")]
        public string? RequestId { get; set; }

        [JsonPropertyName("payUrl")]
        public string? PayUrl { get; set; }

        [JsonPropertyName("deeplink")]
        public string? Deeplink { get; set; }

        [JsonPropertyName("qrCodeUrl")]
        public string? QrCodeUrl { get; set; }

        [JsonPropertyName("resultCode")]
        public int ResultCode { get; set; }

        [JsonPropertyName("message")]
        public string? Message { get; set; }

        [JsonPropertyName("localMessage")]
        public string? LocalMessage { get; set; }

        [JsonPropertyName("signature")]
        public string? Signature { get; set; }
    }
}
