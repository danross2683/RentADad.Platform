using System.Net;
using System.Net.Http.Json;
using FluentAssertions;

namespace RentADad.Tests.Api;

public sealed class AuthRequiredApiTests : IClassFixture<TestApplicationFactory>
{
    private readonly TestApplicationFactory _factory;

    public AuthRequiredApiTests(TestApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Write_endpoints_require_auth_when_enabled()
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Test-Auth-Disabled", "true");

        var createJob = await client.PostAsJsonAsync("/api/v1/jobs", TestDataFactory.Job());
        createJob.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        var registerProvider = await client.PostAsJsonAsync("/api/v1/providers", TestDataFactory.Provider());
        registerProvider.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        var health = await client.GetAsync("/health/live");
        health.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
