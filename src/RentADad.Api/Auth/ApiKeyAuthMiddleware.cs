using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace RentADad.Api.Auth;

public sealed class ApiKeyAuthMiddleware
{
    private const string HeaderName = "X-API-Key";
    private readonly RequestDelegate _next;
    private readonly HashSet<string> _keys;

    public ApiKeyAuthMiddleware(RequestDelegate next, IConfiguration configuration)
    {
        _next = next;
        _keys = configuration.GetSection("Auth:ApiKeys").Get<string[]>()?.ToHashSet(StringComparer.Ordinal) ?? new HashSet<string>(StringComparer.Ordinal);
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (_keys.Count == 0)
        {
            await _next(context);
            return;
        }

        if (!context.Request.Headers.TryGetValue(HeaderName, out var values))
        {
            await _next(context);
            return;
        }

        var provided = values.ToString();
        if (_keys.Contains(provided))
        {
            var identity = new ClaimsIdentity("ApiKey");
            identity.AddClaim(new Claim(ClaimTypes.Name, "api-key"));
            identity.AddClaim(new Claim(ClaimTypes.Role, "admin"));
            context.User = new ClaimsPrincipal(identity);
        }

        await _next(context);
    }
}
