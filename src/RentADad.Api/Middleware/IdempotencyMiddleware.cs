using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using RentADad.Infrastructure.Persistence;

namespace RentADad.Api.Middleware;

public sealed class IdempotencyMiddleware
{
    private const string HeaderName = "Idempotency-Key";
    private const int MaxBodyBytes = 1024 * 1024;
    private readonly RequestDelegate _next;

    public IdempotencyMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, AppDbContext dbContext, IConfiguration configuration)
    {
        if (!IsWriteMethod(context.Request.Method))
        {
            await _next(context);
            return;
        }

        if (!context.Request.Headers.TryGetValue(HeaderName, out var headerValues))
        {
            await _next(context);
            return;
        }

        var key = headerValues.ToString();
        if (string.IsNullOrWhiteSpace(key))
        {
            await _next(context);
            return;
        }

        var method = context.Request.Method;
        var path = context.Request.Path.ToString();
        var requestBody = await ReadRequestBodyAsync(context.Request);
        var requestHash = ComputeHash($"{method}:{path}:{requestBody}");

        var existing = await dbContext.IdempotencyKeys
            .AsNoTracking()
            .FirstOrDefaultAsync(item =>
                item.Key == key &&
                item.Method == method &&
                item.Path == path);

        if (existing is not null)
        {
            if (!string.Equals(existing.RequestHash, requestHash, StringComparison.Ordinal))
            {
                context.Response.StatusCode = StatusCodes.Status409Conflict;
                await context.Response.WriteAsJsonAsync(new
                {
                    error = "idempotency_conflict",
                    message = "Idempotency key was reused with a different request payload."
                });
                return;
            }

            context.Response.StatusCode = existing.ResponseStatusCode;
            context.Response.ContentType = existing.ContentType;
            await context.Response.WriteAsync(existing.ResponseBody);
            return;
        }

        var originalBody = context.Response.Body;
        await using var responseBuffer = new MemoryStream();
        context.Response.Body = responseBuffer;

        await _next(context);

        responseBuffer.Position = 0;
        var responseBody = await new StreamReader(responseBuffer).ReadToEndAsync();
        context.Response.Body = originalBody;
        await context.Response.WriteAsync(responseBody);

        var ttlHours = configuration.GetValue("Idempotency:RetentionHours", 24);
        var now = DateTime.UtcNow;
        var entry = new IdempotencyKey(
            key,
            method,
            path,
            requestHash,
            context.Response.StatusCode,
            responseBody,
            context.Response.ContentType ?? "application/json",
            now,
            now.AddHours(ttlHours));

        dbContext.IdempotencyKeys.Add(entry);
        await dbContext.SaveChangesAsync();
    }

    private static bool IsWriteMethod(string method)
        => HttpMethods.IsPost(method) || HttpMethods.IsPut(method) || HttpMethods.IsPatch(method) || HttpMethods.IsDelete(method);

    private static async Task<string> ReadRequestBodyAsync(HttpRequest request)
    {
        request.EnableBuffering();

        if (request.ContentLength is > MaxBodyBytes)
        {
            return string.Empty;
        }

        using var reader = new StreamReader(request.Body, Encoding.UTF8, leaveOpen: true);
        var body = await reader.ReadToEndAsync();
        request.Body.Position = 0;
        return body;
    }

    private static string ComputeHash(string input)
    {
        using var sha = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(input);
        var hash = sha.ComputeHash(bytes);
        return Convert.ToHexString(hash);
    }
}
