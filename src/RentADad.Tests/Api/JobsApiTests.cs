using System.Net;
using System.Net.Http.Json;
using FluentAssertions;

namespace RentADad.Tests.Api;

public sealed class JobsApiTests : IClassFixture<TestApplicationFactory>
{
    private readonly TestApplicationFactory _factory;

    public JobsApiTests(TestApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Create_job_requires_services()
    {
        var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/v1/jobs", TestDataFactory.Job(serviceIds: new List<Guid>()));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
