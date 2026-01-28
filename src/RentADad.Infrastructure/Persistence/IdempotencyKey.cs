using System;

namespace RentADad.Infrastructure.Persistence;

public sealed class IdempotencyKey
{
    private IdempotencyKey()
    {
        Key = string.Empty;
        Method = string.Empty;
        Path = string.Empty;
        RequestHash = string.Empty;
        ResponseBody = string.Empty;
        ContentType = "application/json";
    }

    public IdempotencyKey(
        string key,
        string method,
        string path,
        string requestHash,
        int responseStatusCode,
        string responseBody,
        string contentType,
        DateTime createdUtc,
        DateTime expiresUtc)
    {
        Key = key;
        Method = method;
        Path = path;
        RequestHash = requestHash;
        ResponseStatusCode = responseStatusCode;
        ResponseBody = responseBody;
        ContentType = contentType;
        CreatedUtc = createdUtc;
        ExpiresUtc = expiresUtc;
    }

    public Guid Id { get; private set; }
    public string Key { get; private set; }
    public string Method { get; private set; }
    public string Path { get; private set; }
    public string RequestHash { get; private set; }
    public int ResponseStatusCode { get; private set; }
    public string ResponseBody { get; private set; }
    public string ContentType { get; private set; }
    public DateTime CreatedUtc { get; private set; }
    public DateTime ExpiresUtc { get; private set; }
}
