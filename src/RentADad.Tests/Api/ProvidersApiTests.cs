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

    [Fact]
    public async Task Cannot_add_overlapping_availability()
    {
        var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync(
            "/api/v1/providers",
            TestDataFactory.Provider());
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = await response.Content.ReadFromJsonAsync<ProviderResponse>();
        created.Should().NotBeNull();

        var start = DateTime.UtcNow.AddDays(1).Date.AddHours(9);
        var end = start.AddHours(4);
        var overlapStart = start.AddHours(2);
        var overlapEnd = overlapStart.AddHours(2);

        var firstAvailability = await client.PostAsJsonAsync(
            $"/api/v1/providers/{created!.Id}/availability",
            new { StartUtc = start, EndUtc = end });
        firstAvailability.StatusCode.Should().Be(HttpStatusCode.OK);

        var overlappingAvailability = await client.PostAsJsonAsync(
            $"/api/v1/providers/{created.Id}/availability",
            new { StartUtc = overlapStart, EndUtc = overlapEnd });
        overlappingAvailability.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Can_remove_availability()
    {
        var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync(
            "/api/v1/providers",
            TestDataFactory.Provider());
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = await response.Content.ReadFromJsonAsync<ProviderResponse>();
        created.Should().NotBeNull();

        var start = DateTime.UtcNow.AddDays(1).Date.AddHours(9);
        var end = start.AddHours(2);

        var addResponse = await client.PostAsJsonAsync(
            $"/api/v1/providers/{created!.Id}/availability",
            new { StartUtc = start, EndUtc = end });
        addResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var updated = await addResponse.Content.ReadFromJsonAsync<ProviderResponse>();
        updated.Should().NotBeNull();
        updated!.Availabilities.Should().HaveCount(1);

        var availabilityId = updated.Availabilities[0].Id;
        var deleteResponse = await client.DeleteAsync($"/api/v1/providers/{created.Id}/availability/{availabilityId}");
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var afterDelete = await deleteResponse.Content.ReadFromJsonAsync<ProviderResponse>();
        afterDelete.Should().NotBeNull();
        afterDelete!.Availabilities.Should().BeEmpty();
    }
}
