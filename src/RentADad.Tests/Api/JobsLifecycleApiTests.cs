using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using RentADad.Application.Jobs.Responses;

namespace RentADad.Tests.Api;

public sealed class JobsLifecycleApiTests : IClassFixture<TestApplicationFactory>
{
    private readonly TestApplicationFactory _factory;

    public JobsLifecycleApiTests(TestApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Can_post_accept_start_complete_close_job()
    {
        var client = _factory.CreateClient();

        var createResponse = await client.PostAsJsonAsync("/api/v1/jobs", TestDataFactory.Job());
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var job = await createResponse.Content.ReadFromJsonAsync<JobResponse>();
        job.Should().NotBeNull();

        var postResponse = await client.PostAsync($"/api/v1/jobs/{job!.Id}:post", null);
        postResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var posted = await postResponse.Content.ReadFromJsonAsync<JobResponse>();
        posted!.Status.Should().Be("Posted");

        var acceptResponse = await client.PostAsJsonAsync(
            $"/api/v1/jobs/{job.Id}:accept",
            new { BookingId = Guid.NewGuid() });
        acceptResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var accepted = await acceptResponse.Content.ReadFromJsonAsync<JobResponse>();
        accepted!.Status.Should().Be("Accepted");

        var startResponse = await client.PostAsync($"/api/v1/jobs/{job.Id}:start", null);
        startResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var started = await startResponse.Content.ReadFromJsonAsync<JobResponse>();
        started!.Status.Should().Be("InProgress");

        var completeResponse = await client.PostAsync($"/api/v1/jobs/{job.Id}:complete", null);
        completeResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var completed = await completeResponse.Content.ReadFromJsonAsync<JobResponse>();
        completed!.Status.Should().Be("Completed");

        var closeResponse = await client.PostAsync($"/api/v1/jobs/{job.Id}:close", null);
        closeResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var closed = await closeResponse.Content.ReadFromJsonAsync<JobResponse>();
        closed!.Status.Should().Be("Closed");
    }
}
