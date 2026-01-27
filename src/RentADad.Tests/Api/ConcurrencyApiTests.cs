using System.Net;
using System.Net.Http.Json;
using System.Linq;
using FluentAssertions;
using RentADad.Application.Providers.Responses;

namespace RentADad.Tests.Api;

public sealed class ConcurrencyApiTests : IClassFixture<TestApplicationFactory>
{
    private readonly TestApplicationFactory _factory;

    public ConcurrencyApiTests(TestApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Concurrent_updates_return_conflict_for_second_request()
    {
        var client = _factory.CreateClient();

        var create = await client.PostAsJsonAsync("/api/v1/providers", TestDataFactory.Provider());
        create.StatusCode.Should().Be(HttpStatusCode.Created);
        var provider = await create.Content.ReadFromJsonAsync<ProviderResponse>();
        provider.Should().NotBeNull();

        create.Headers.TryGetValues("ETag", out var etagValues).Should().BeTrue();
        var etag = etagValues!.First();
        etag.Should().NotBeNullOrWhiteSpace();

        var firstUpdate = new HttpRequestMessage(HttpMethod.Put, $"/api/v1/providers/{provider!.Id}")
        {
            Content = JsonContent.Create(new { DisplayName = "Provider A" })
        };
        firstUpdate.Headers.TryAddWithoutValidation("If-Match", etag);
        var firstResponse = await client.SendAsync(firstUpdate);
        firstResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var secondUpdate = new HttpRequestMessage(HttpMethod.Put, $"/api/v1/providers/{provider.Id}")
        {
            Content = JsonContent.Create(new { DisplayName = "Provider B" })
        };
        secondUpdate.Headers.TryAddWithoutValidation("If-Match", etag);
        var secondResponse = await client.SendAsync(secondUpdate);
        secondResponse.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }
}
