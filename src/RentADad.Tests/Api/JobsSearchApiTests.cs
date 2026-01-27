using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using RentADad.Application.Common.Paging;
using RentADad.Application.Jobs.Responses;

namespace RentADad.Tests.Api;

public sealed class JobsSearchApiTests : IClassFixture<TestApplicationFactory>
{
    private readonly TestApplicationFactory _factory;

    public JobsSearchApiTests(TestApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Can_search_jobs_by_status_and_customer()
    {
        var client = _factory.CreateClient();
        var customerId = Guid.NewGuid();

        var first = await client.PostAsJsonAsync("/api/v1/jobs", TestDataFactory.Job(customerId: customerId));
        first.StatusCode.Should().Be(HttpStatusCode.Created);
        var second = await client.PostAsJsonAsync("/api/v1/jobs", TestDataFactory.Job());
        second.StatusCode.Should().Be(HttpStatusCode.Created);

        var response = await client.GetAsync($"/api/v1/jobs/search?page=1&pageSize=10&status=Draft&customerId={customerId}");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var payload = await response.Content.ReadFromJsonAsync<PagedResult<JobResponse>>();
        payload.Should().NotBeNull();
        payload!.Items.Should().OnlyContain(job => job.CustomerId == customerId);
    }
}
