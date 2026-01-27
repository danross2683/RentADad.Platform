using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace RentADad.Api.Results;

public sealed class ResultWithHeader : IResult
{
    private readonly IResult _inner;
    private readonly string _name;
    private readonly string _value;

    public ResultWithHeader(IResult inner, string name, string value)
    {
        _inner = inner;
        _name = name;
        _value = value;
    }

    public Task ExecuteAsync(HttpContext httpContext)
    {
        httpContext.Response.Headers[_name] = _value;
        return _inner.ExecuteAsync(httpContext);
    }
}

