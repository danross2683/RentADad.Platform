using System.Net;
using FluentAssertions;

namespace RentADad.Tests.Api;

public sealed class PagingValidationApiTests : IClassFixture<TestApplicationFactory>
{
    private readonly TestApplicationFactory _factory;

    public PagingValidationApiTests(TestApplicationFactory factory)
    {
        _factory = factory;
    }

    [Theory]
    [InlineData("/api/v1/jobs/search?page=0&pageSize=10")]
    [InlineData("/api/v1/bookings/search?page=-1&pageSize=10")]
    [InlineData("/api/v1/providers/search?page=1&pageSize=0")]
    [InlineData("/api/v1/providers/search?page=1&pageSize=201")]
    public async Task Invalid_paging_returns_bad_request(string url)
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync(url);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Invalid_booking_status_returns_bad_request()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/v1/bookings/search?page=1&pageSize=10&status=NotAStatus");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Invalid_job_status_returns_bad_request()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/v1/jobs/search?page=1&pageSize=10&status=Nope");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Invalid_booking_date_range_returns_bad_request()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/v1/bookings/search?page=1&pageSize=10&startUtcFrom=2026-01-10T00:00:00Z&startUtcTo=2026-01-01T00:00:00Z");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
