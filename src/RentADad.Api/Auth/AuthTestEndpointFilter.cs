using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;

namespace RentADad.Api.Auth;

public sealed class AuthTestEndpointFilter : IEndpointFilter
{
    public ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var httpContext = context.HttpContext;
        if (httpContext.Request.Headers.ContainsKey("X-Test-Auth-Disabled"))
        {
            var endpoint = httpContext.GetEndpoint();
            var allowAnonymous = endpoint?.Metadata.GetMetadata<IAllowAnonymous>();
            if (allowAnonymous is null)
            {
                return ValueTask.FromResult<object?>(global::Microsoft.AspNetCore.Http.Results.Unauthorized());
            }
        }

        return next(context);
    }
}
