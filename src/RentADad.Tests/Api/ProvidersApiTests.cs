using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using RentADad.Application.Providers.Responses;

namespace RentADad.Tests.Api;

public sealed class ProvidersApiTests : IClassFixture<TestApplicationFactory>
{
    private readonly TestApplicationFactory _factory;

    public ProvidersApiTests(TestApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Can_register_and_fetch_provider()
    {
        var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync(
            "/api/v1/providers",
            TestDataFactory.Provider());

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = await response.Content.ReadFromJsonAsync<ProviderResponse>();
        created.Should().NotBeNull();

        var getResponse = await client.GetAsync($"/api/v1/providers/{created!.Id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var fetched = await getResponse.Content.ReadFromJsonAsync<ProviderResponse>();
        fetched.Should().NotBeNull();
        fetched!.Id.Should().Be(created.Id);
    }
}
