using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using RentADad.Application.Bookings.Responses;
using RentADad.Application.Common.Paging;
using RentADad.Application.Jobs.Responses;
using RentADad.Application.Providers.Responses;

namespace RentADad.Tests.Api;

public sealed class BookingsSearchApiTests : IClassFixture<TestApplicationFactory>
{
    private readonly TestApplicationFactory _factory;

    public BookingsSearchApiTests(TestApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Can_search_bookings_by_provider_and_status()
    {
        var client = _factory.CreateClient();

        var providerResponse = await client.PostAsJsonAsync("/api/v1/providers", TestDataFactory.Provider());
        providerResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var provider = await providerResponse.Content.ReadFromJsonAsync<ProviderResponse>();
        provider.Should().NotBeNull();

        var jobResponse = await client.PostAsJsonAsync("/api/v1/jobs", TestDataFactory.Job());
        jobResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var job = await jobResponse.Content.ReadFromJsonAsync<JobResponse>();
        job.Should().NotBeNull();

        var bookingResponse = await client.PostAsJsonAsync(
            "/api/v1/bookings",
            TestDataFactory.Booking(job!.Id, provider!.Id));
        bookingResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var response = await client.GetAsync(
            $"/api/v1/bookings/search?page=1&pageSize=10&status=Pending&providerId={provider.Id}");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var payload = await response.Content.ReadFromJsonAsync<PagedResult<BookingResponse>>();
        payload.Should().NotBeNull();
        payload!.Items.Should().NotBeEmpty();
        payload.Items.Should().OnlyContain(booking => booking.ProviderId == provider.Id);
    }
}
