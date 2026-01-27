using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using RentADad.Application.Bookings.Responses;
using RentADad.Application.Jobs.Responses;
using RentADad.Application.Providers.Responses;

namespace RentADad.Tests.Api;

public sealed class BookingsApiTests : IClassFixture<TestApplicationFactory>
{
    private readonly TestApplicationFactory _factory;

    public BookingsApiTests(TestApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Create_booking_requires_end_after_start()
    {
        var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync(
            "/api/v1/bookings",
            TestDataFactory.Booking(Guid.NewGuid(), Guid.NewGuid(), DateTime.UtcNow, DateTime.UtcNow.AddMinutes(-10)));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Can_create_and_confirm_booking()
    {
        var client = _factory.CreateClient();

        var providerResponse = await client.PostAsJsonAsync(
            "/api/v1/providers",
            TestDataFactory.Provider());
        providerResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var provider = await providerResponse.Content.ReadFromJsonAsync<ProviderResponse>();
        provider.Should().NotBeNull();

        var jobResponse = await client.PostAsJsonAsync(
            "/api/v1/jobs",
            TestDataFactory.Job());
        jobResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var job = await jobResponse.Content.ReadFromJsonAsync<JobResponse>();
        job.Should().NotBeNull();

        var start = DateTime.UtcNow.AddHours(2);
        var end = start.AddHours(2);
        var bookingResponse = await client.PostAsJsonAsync(
            "/api/v1/bookings",
            TestDataFactory.Booking(job!.Id, provider!.Id, start, end));
        bookingResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var booking = await bookingResponse.Content.ReadFromJsonAsync<BookingResponse>();
        booking.Should().NotBeNull();

        var confirmResponse = await client.PostAsync($"/api/v1/bookings/{booking!.Id}:confirm", null);
        confirmResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var confirmed = await confirmResponse.Content.ReadFromJsonAsync<BookingResponse>();
        confirmed.Should().NotBeNull();
        confirmed!.Status.Should().Be("Confirmed");
    }

    [Fact]
    public async Task Cannot_confirm_booking_twice()
    {
        var client = _factory.CreateClient();

        var providerResponse = await client.PostAsJsonAsync(
            "/api/v1/providers",
            TestDataFactory.Provider());
        providerResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var provider = await providerResponse.Content.ReadFromJsonAsync<ProviderResponse>();
        provider.Should().NotBeNull();

        var jobResponse = await client.PostAsJsonAsync(
            "/api/v1/jobs",
            TestDataFactory.Job());
        jobResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var job = await jobResponse.Content.ReadFromJsonAsync<JobResponse>();
        job.Should().NotBeNull();

        var bookingResponse = await client.PostAsJsonAsync(
            "/api/v1/bookings",
            TestDataFactory.Booking(job!.Id, provider!.Id));
        bookingResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var booking = await bookingResponse.Content.ReadFromJsonAsync<BookingResponse>();
        booking.Should().NotBeNull();

        var confirmResponse = await client.PostAsync($"/api/v1/bookings/{booking!.Id}:confirm", null);
        confirmResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var secondConfirm = await client.PostAsync($"/api/v1/bookings/{booking.Id}:confirm", null);
        secondConfirm.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Cannot_cancel_booking_after_confirm_is_cancelled()
    {
        var client = _factory.CreateClient();

        var providerResponse = await client.PostAsJsonAsync(
            "/api/v1/providers",
            TestDataFactory.Provider());
        providerResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var provider = await providerResponse.Content.ReadFromJsonAsync<ProviderResponse>();
        provider.Should().NotBeNull();

        var jobResponse = await client.PostAsJsonAsync(
            "/api/v1/jobs",
            TestDataFactory.Job());
        jobResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var job = await jobResponse.Content.ReadFromJsonAsync<JobResponse>();
        job.Should().NotBeNull();

        var bookingResponse = await client.PostAsJsonAsync(
            "/api/v1/bookings",
            TestDataFactory.Booking(job!.Id, provider!.Id));
        bookingResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var booking = await bookingResponse.Content.ReadFromJsonAsync<BookingResponse>();
        booking.Should().NotBeNull();

        var confirmResponse = await client.PostAsync($"/api/v1/bookings/{booking!.Id}:confirm", null);
        confirmResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var cancelResponse = await client.PostAsync($"/api/v1/bookings/{booking.Id}:cancel", null);
        cancelResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var secondCancel = await client.PostAsync($"/api/v1/bookings/{booking.Id}:cancel", null);
        secondCancel.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Cannot_expire_confirmed_booking()
    {
        var client = _factory.CreateClient();

        var providerResponse = await client.PostAsJsonAsync(
            "/api/v1/providers",
            TestDataFactory.Provider());
        providerResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var provider = await providerResponse.Content.ReadFromJsonAsync<ProviderResponse>();
        provider.Should().NotBeNull();

        var jobResponse = await client.PostAsJsonAsync(
            "/api/v1/jobs",
            TestDataFactory.Job());
        jobResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var job = await jobResponse.Content.ReadFromJsonAsync<JobResponse>();
        job.Should().NotBeNull();

        var bookingResponse = await client.PostAsJsonAsync(
            "/api/v1/bookings",
            TestDataFactory.Booking(job!.Id, provider!.Id));
        bookingResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var booking = await bookingResponse.Content.ReadFromJsonAsync<BookingResponse>();
        booking.Should().NotBeNull();

        var confirmResponse = await client.PostAsync($"/api/v1/bookings/{booking!.Id}:confirm", null);
        confirmResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var expireResponse = await client.PostAsync($"/api/v1/bookings/{booking.Id}:expire", null);
        expireResponse.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }
}
