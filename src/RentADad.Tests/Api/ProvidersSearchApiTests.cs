using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using RentADad.Application.Common.Paging;
using RentADad.Application.Providers.Responses;

namespace RentADad.Tests.Api;

public sealed class ProvidersSearchApiTests : IClassFixture<TestApplicationFactory>
{
    private readonly TestApplicationFactory _factory;

    public ProvidersSearchApiTests(TestApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Can_search_providers_by_name()
    {
        var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync(
            "/api/v1/providers",
            TestDataFactory.Provider(displayName: "Rent-A-Dad Alice"));
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var search = await client.GetAsync("/api/v1/providers/search?page=1&pageSize=10&displayName=Alice");
        search.StatusCode.Should().Be(HttpStatusCode.OK);

        var payload = await search.Content.ReadFromJsonAsync<PagedResult<ProviderResponse>>();
        payload.Should().NotBeNull();
        payload!.Items.Should().ContainSingle();
        payload.Items[0].DisplayName.Should().Contain("Alice");
    }
}
