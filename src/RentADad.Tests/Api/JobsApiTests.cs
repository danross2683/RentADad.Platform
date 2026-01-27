using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using RentADad.Application.Bookings.Requests;
using RentADad.Application.Jobs.Requests;
using RentADad.Application.Providers.Requests;

namespace RentADad.Tests.Api;

public sealed class JobsApiTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public JobsApiTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Create_job_requires_services()
    {
        var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/v1/jobs", new CreateJobRequest(Guid.NewGuid(), "Somewhere", new List<Guid>()));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
