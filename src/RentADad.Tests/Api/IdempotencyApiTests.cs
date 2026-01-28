using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using RentADad.Application.Jobs.Responses;

namespace RentADad.Tests.Api;

public sealed class IdempotencyApiTests : IClassFixture<TestApplicationFactory>
{
    private readonly TestApplicationFactory _factory;

    public IdempotencyApiTests(TestApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Create_job_is_idempotent_with_same_key()
    {
        var client = _factory.CreateClient();

        var request = TestDataFactory.Job();
        var key = Guid.NewGuid().ToString("N");

        var first = new HttpRequestMessage(HttpMethod.Post, "/api/v1/jobs")
        {
            Content = JsonContent.Create(request)
        };
        first.Headers.TryAddWithoutValidation("Idempotency-Key", key);
        var firstResponse = await client.SendAsync(first);
        firstResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var firstJob = await firstResponse.Content.ReadFromJsonAsync<JobResponse>();
        firstJob.Should().NotBeNull();

        var second = new HttpRequestMessage(HttpMethod.Post, "/api/v1/jobs")
        {
            Content = JsonContent.Create(request)
        };
        second.Headers.TryAddWithoutValidation("Idempotency-Key", key);
        var secondResponse = await client.SendAsync(second);
        secondResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var secondJob = await secondResponse.Content.ReadFromJsonAsync<JobResponse>();
        secondJob.Should().NotBeNull();

        secondJob!.Id.Should().Be(firstJob!.Id);
    }
}
