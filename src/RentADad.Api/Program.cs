using System.Linq;
using System.Text;
using System.Threading.RateLimiting;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.IdentityModel.Tokens;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using RentADad.Api;
using RentADad.Api.Auth;
using RentADad.Api.Background;
using RentADad.Api.Dashboards;
using RentADad.Api.Health;
using RentADad.Api.Middleware;
using RentADad.Api.Notifications;
using RentADad.Api.Results;
using RentADad.Api.Auditing;
using RentADad.Api.Caching;
using RentADad.Application.Abstractions.Persistence;
using RentADad.Application.Abstractions.Repositories;
using RentADad.Application.Abstractions.Notifications;
using RentADad.Application.Abstractions.Auditing;
using RentADad.Application.Abstractions.ReadModels;
using RentADad.Application.Abstractions.Caching;
using RentADad.Application.Abstractions.Observability;
using RentADad.Application.Bookings;
using RentADad.Application.Bookings.Requests;
using RentADad.Application.Bookings.Validators;
using AppJobService = RentADad.Application.Jobs.JobService;
using RentADad.Application.Providers;
using RentADad.Application.Providers.Requests;
using RentADad.Application.Jobs.Requests;
using RentADad.Application.Jobs.Validators;
using RentADad.Application.Providers.Validators;
using RentADad.Api.Seed;
using RentADad.Domain.Bookings;
using DomainJobs = RentADad.Domain.Jobs;
using RentADad.Application.Jobs;
using RentADad.Infrastructure.Persistence;
using RentADad.Infrastructure.Persistence.Repositories;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddEnvironmentVariables(prefix: "RentADad_");

ValidateConfiguration(builder.Configuration, builder.Environment);

builder.Services.AddOpenApi();
builder.Services.AddDbContext<AppDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("Default");
    var testConnection = builder.Configuration.GetConnectionString("Test");
    var useSqlite = builder.Environment.IsEnvironment("Testing") && !string.IsNullOrWhiteSpace(testConnection);
    var allowTestMigrations = builder.Configuration.GetValue("Database:AllowTestMigrations", false);

    if (allowTestMigrations)
    {
        options.ConfigureWarnings(warnings =>
            warnings.Ignore(RelationalEventId.PendingModelChangesWarning));
    }

    if (useSqlite)
    {
        options.UseSqlite(testConnection);
        return;
    }

    options.UseNpgsql(
        connectionString,
        npgsql =>
        {
            npgsql.MigrationsAssembly(typeof(AppDbContext).Assembly.GetName().Name);
            npgsql.EnableRetryOnFailure(
                maxRetryCount: 3,
                maxRetryDelay: TimeSpan.FromSeconds(2),
                errorCodesToAdd: null);
        });
});
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IJobRepository, JobRepository>();
builder.Services.AddScoped<IJobListingReader, JobListingRepository>();
builder.Services.AddScoped<IJobListingWriter, JobListingRepository>();
builder.Services.AddScoped<IBookingRepository, BookingRepository>();
builder.Services.AddScoped<IProviderRepository, ProviderRepository>();
builder.Services.AddScoped<AppJobService>();
builder.Services.AddScoped<BookingService>();
builder.Services.AddScoped<ProviderService>();
builder.Services.AddValidatorsFromAssemblyContaining<CreateJobRequestValidator>();
builder.Services.AddProblemDetails();
builder.Services.AddScoped<DevSeeder>();
builder.Services.AddHttpClient();
builder.Services.AddScoped<INotificationSender, WebhookNotificationSender>();
builder.Services.AddScoped<IAuditSink, LogAuditSink>();
builder.Services.AddMemoryCache();
var cacheSettings = new CacheSettings
{
    ProviderAvailabilitySeconds = builder.Configuration.GetValue("Caching:ProviderAvailabilitySeconds", 30)
};
builder.Services.AddSingleton(cacheSettings);
builder.Services.AddSingleton<ICacheStore, MemoryCacheStore>();
var alertingThresholds = new AlertingThresholds
{
    ErrorRate5xxPercent = builder.Configuration.GetValue("Alerting:ErrorRate5xxPercent", 1.0),
    LatencyP95Ms = builder.Configuration.GetValue("Alerting:LatencyP95Ms", 1500),
    LatencyP99Ms = builder.Configuration.GetValue("Alerting:LatencyP99Ms", 3000),
    AuthFailurePercent = builder.Configuration.GetValue("Alerting:AuthFailurePercent", 5.0),
    DbP95Ms = builder.Configuration.GetValue("Alerting:DbP95Ms", 500),
    BackgroundJobStaleMinutes = builder.Configuration.GetValue("Alerting:BackgroundJobStaleMinutes", 10)
};
builder.Services.AddSingleton(alertingThresholds);
builder.Services.AddHealthChecks()
    .AddCheck("self", () => HealthCheckResult.Healthy(), tags: new[] { "live" })
    .AddCheck<DbReadyHealthCheck>("db", tags: new[] { "ready" });
builder.Services.AddHostedService<BookingExpiryService>();
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
        RateLimitPartition.GetFixedWindowLimiter(
            "global",
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 60,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0,
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst
            }));
});

builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource =>
        resource.AddService(serviceName: "RentADad.Api", serviceVersion: ApiVersion.Current))
    .WithTracing(tracing =>
        tracing
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation()
            .AddOtlpExporter(options =>
            {
                var endpoint = builder.Configuration["OpenTelemetry:OtlpEndpoint"];
                if (!string.IsNullOrWhiteSpace(endpoint))
                {
                    options.Endpoint = new Uri(endpoint);
                }
            }))
    .WithMetrics(metrics =>
        metrics
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation()
            .AddRuntimeInstrumentation()
            .AddOtlpExporter(options =>
            {
                var endpoint = builder.Configuration["OpenTelemetry:OtlpEndpoint"];
                if (!string.IsNullOrWhiteSpace(endpoint))
                {
                    options.Endpoint = new Uri(endpoint);
                }
            }));

var authEnabled = builder.Configuration.GetValue("Auth:Enabled", false);
if (authEnabled)
{
    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            options.RequireHttpsMetadata = true;
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = builder.Configuration["Auth:Issuer"],
                ValidateAudience = true,
                ValidAudience = builder.Configuration["Auth:Audience"],
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(builder.Configuration["Auth:SigningKey"] ?? string.Empty)),
                ValidateLifetime = true,
                ClockSkew = TimeSpan.FromMinutes(2)
            };
        });

    builder.Services.AddAuthorization(options =>
    {
        options.FallbackPolicy = new AuthorizationPolicyBuilder()
            .RequireAuthenticatedUser()
            .Build();
        options.AddPolicy("WriteAccess", policy =>
            policy.RequireRole("admin"));
    });
}


var app = builder.Build();
var appStartedUtc = DateTime.UtcNow;

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}
else
{
    app.UseHsts();
}

if (authEnabled)
{
    app.UseAuthentication();
    app.UseAuthorization();
}

app.UseMiddleware<ApiKeyAuthMiddleware>();

app.UseRateLimiter();
app.UseHttpsRedirection();
app.UseMiddleware<SecurityHeadersMiddleware>();
app.UseMiddleware<IdempotencyMiddleware>();

app.Use(async (context, next) =>
{
    var logger = context.RequestServices.GetRequiredService<ILoggerFactory>().CreateLogger("Request");
    var started = Stopwatch.GetTimestamp();

    if (!context.Request.Headers.TryGetValue("X-Correlation-Id", out var correlationId) || string.IsNullOrWhiteSpace(correlationId))
    {
        correlationId = Guid.NewGuid().ToString("N");
    }

    context.TraceIdentifier = correlationId!;
    context.Response.Headers["X-Correlation-Id"] = correlationId!;
    context.Response.Headers["X-Api-Version"] = ApiVersion.Current;

    try
    {
        await next();
    }
    finally
    {
        var requestHeaders = BuildRedactedRequestHeaders(context.Request);
        var responseHeaders = BuildRedactedResponseHeaders(context.Response);
        var elapsedMs = Stopwatch.GetElapsedTime(started).TotalMilliseconds;
        logger.LogInformation(
            "HTTP {Method} {Path} responded {StatusCode} in {ElapsedMs}ms CorrelationId={CorrelationId}",
            context.Request.Method,
            context.Request.Path,
            context.Response.StatusCode,
            elapsedMs,
            context.TraceIdentifier);
        logger.LogDebug(
            "Headers Request={RequestHeaders} Response={ResponseHeaders} CorrelationId={CorrelationId}",
            requestHeaders,
            responseHeaders,
            context.TraceIdentifier);
    }
});

app.MapHealthChecks("/health/live", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("live")
});

app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready")
});

var autoMigrate = app.Configuration.GetValue("Database:AutoMigrate", true);
var migrationsOnly = args.Contains("--apply-migrations-only");
var allowTestMigrations = app.Configuration.GetValue("Database:AllowTestMigrations", false);
var seedDemo = args.Contains("--seed-demo");
if (autoMigrate && (!app.Environment.IsEnvironment("Testing") || allowTestMigrations || migrationsOnly))
{
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await dbContext.Database.MigrateAsync();
}

if (migrationsOnly)
{
    return;
}

if (seedDemo && !app.Environment.IsEnvironment("Testing"))
{
    using var scope = app.Services.CreateScope();
    var seeder = scope.ServiceProvider.GetRequiredService<DevSeeder>();
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await seeder.SeedAsync(dbContext);
    return;
}

if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var seeder = scope.ServiceProvider.GetRequiredService<DevSeeder>();
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await seeder.SeedAsync(dbContext);
}

app.UseExceptionHandler(exceptionApp =>
{
    exceptionApp.Run(context =>
    {
        var exception = context.Features.Get<IExceptionHandlerPathFeature>()?.Error;
        if (exception is null)
        {
            return Results.Problem("An unexpected error occurred.").ExecuteAsync(context);
        }

        var logger = context.RequestServices.GetRequiredService<ILoggerFactory>().CreateLogger("Errors");
        logger.LogError(
            exception,
            "Unhandled exception for {Method} {Path} CorrelationId={CorrelationId}",
            context.Request.Method,
            context.Request.Path,
            context.TraceIdentifier);

        var problem = exception switch
        {
            JobDomainException ex => CreateProblem(StatusCodes.Status409Conflict, ex.ErrorCode, ex.Message),
            BookingDomainException ex => CreateProblem(StatusCodes.Status409Conflict, ex.ErrorCode, ex.Message),
            ProviderDomainException ex => CreateProblem(StatusCodes.Status409Conflict, ex.ErrorCode, ex.Message),
            DbUpdateConcurrencyException => CreateProblem(
                StatusCodes.Status409Conflict,
                "concurrency_conflict",
                "Resource was updated by another request."),
            _ => CreateProblem(StatusCodes.Status500InternalServerError, "server_error", "An unexpected error occurred.")
        };

        return problem.ExecuteAsync(context);
    });
});

var jobs = app.MapGroup("/api/v1/jobs").WithTags("Jobs");
var bookings = app.MapGroup("/api/v1/bookings").WithTags("Bookings");
var providers = app.MapGroup("/api/v1/providers").WithTags("Providers");
var admin = app.MapGroup("/api/v1/admin").WithTags("Admin");

if (app.Environment.IsEnvironment("Testing"))
{
    var authFilter = new AuthTestEndpointFilter();
    jobs.AddEndpointFilter(authFilter);
    bookings.AddEndpointFilter(authFilter);
    providers.AddEndpointFilter(authFilter);
    admin.AddEndpointFilter(authFilter);
}

if (app.Environment.IsDevelopment())
{
    app.MapPost("/api/v1/auth/dev-token", (IConfiguration config) =>
    {
        var signingKey = config["Auth:SigningKey"];
        var issuer = config["Auth:Issuer"];
        var audience = config["Auth:Audience"];

        if (string.IsNullOrWhiteSpace(signingKey) ||
            string.IsNullOrWhiteSpace(issuer) ||
            string.IsNullOrWhiteSpace(audience))
        {
            return Results.BadRequest(new { message = "Auth settings are required for dev token." });
        }

        var claims = new[]
        {
            new Claim(ClaimTypes.Name, "dev-user"),
            new Claim(ClaimTypes.Role, "admin")
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            issuer,
            audience,
            claims,
            expires: DateTime.UtcNow.AddHours(4),
            signingCredentials: creds);

        var jwt = new JwtSecurityTokenHandler().WriteToken(token);
        return Results.Ok(new { accessToken = jwt });
    }).AllowAnonymous().WithTags("Auth");
}

ApplyWriteAuth(admin.MapGet("/jobs/search", async (
    AppJobService jobService,
    int? page,
    int? pageSize,
    Guid? customerId,
    string? status) =>
{
    var errors = new Dictionary<string, string[]>();
    var (resolvedPage, resolvedPageSize) = NormalizePaging(page, pageSize, errors);

    DomainJobs.JobStatus? parsedStatus = null;
    if (!string.IsNullOrWhiteSpace(status))
    {
        if (Enum.TryParse<DomainJobs.JobStatus>(status, true, out var jobStatus))
        {
            parsedStatus = jobStatus;
        }
        else
        {
            errors["status"] = new[] { "Status must be a valid JobStatus value." };
        }
    }

    if (errors.Count > 0)
    {
        return Results.ValidationProblem(errors, extensions: BuildProblemExtensions());
    }

    var query = new JobListQuery(resolvedPage, resolvedPageSize, customerId, parsedStatus);
    var results = await jobService.ListAsync(query);
    return Results.Ok(results);
}), authEnabled);

ApplyWriteAuth(admin.MapGet("/dashboard/summary", async (
    AppJobService jobService,
    BookingService bookingService,
    ProviderService providerService,
    AlertingThresholds alerting) =>
{
    var jobsByStatus = new Dictionary<string, int>();
    foreach (var status in Enum.GetValues<DomainJobs.JobStatus>())
    {
        var result = await jobService.ListAsync(new JobListQuery(1, 1, null, status));
        jobsByStatus[status.ToString()] = result.TotalCount;
    }

    var bookingsByStatus = new Dictionary<string, int>();
    foreach (var status in Enum.GetValues<RentADad.Domain.Bookings.BookingStatus>())
    {
        var result = await bookingService.ListAsync(new BookingListQuery(1, 1, null, null, status, null, null));
        bookingsByStatus[status.ToString()] = result.TotalCount;
    }

    var providers = await providerService.ListAsync(new ProviderListQuery(1, 1, null));

    var now = DateTime.UtcNow;
    var response = new DashboardSummaryResponse(
        ApiVersion.Current,
        now,
        (long)(now - appStartedUtc).TotalSeconds,
        jobsByStatus,
        bookingsByStatus,
        providers.TotalCount,
        alerting);

    return Results.Ok(response);
}), authEnabled);

ApplyWriteAuth(admin.MapGet("/bookings/search", async (
    BookingService bookingService,
    int? page,
    int? pageSize,
    Guid? jobId,
    Guid? providerId,
    string? status,
    DateTime? startUtcFrom,
    DateTime? startUtcTo) =>
{
    var errors = new Dictionary<string, string[]>();
    var (resolvedPage, resolvedPageSize) = NormalizePaging(page, pageSize, errors);

    RentADad.Domain.Bookings.BookingStatus? parsedStatus = null;
    if (!string.IsNullOrWhiteSpace(status))
    {
        if (Enum.TryParse<RentADad.Domain.Bookings.BookingStatus>(status, true, out var bookingStatus))
        {
            parsedStatus = bookingStatus;
        }
        else
        {
            errors["status"] = new[] { "Status must be a valid BookingStatus value." };
        }
    }

    if (startUtcFrom is not null && startUtcTo is not null && startUtcFrom > startUtcTo)
    {
        errors["startUtcFrom"] = new[] { "StartUtcFrom must be earlier than StartUtcTo." };
    }

    if (errors.Count > 0)
    {
        return Results.ValidationProblem(errors, extensions: BuildProblemExtensions());
    }

    var query = new BookingListQuery(resolvedPage, resolvedPageSize, jobId, providerId, parsedStatus, startUtcFrom, startUtcTo);
    var results = await bookingService.ListAsync(query);
    return Results.Ok(results);
}), authEnabled);

jobs.MapGet("/", async (AppJobService jobService) =>
{
    var results = await jobService.ListAsync();
    return Results.Ok(results);
}).AllowAnonymous();

jobs.MapGet("/search", async (
    AppJobService jobService,
    int? page,
    int? pageSize,
    Guid? customerId,
    string? status) =>
{
    var errors = new Dictionary<string, string[]>();
    var (resolvedPage, resolvedPageSize) = NormalizePaging(page, pageSize, errors);

    DomainJobs.JobStatus? parsedStatus = null;
    if (!string.IsNullOrWhiteSpace(status))
    {
        if (Enum.TryParse<DomainJobs.JobStatus>(status, true, out var jobStatus))
        {
            parsedStatus = jobStatus;
        }
        else
        {
            errors["status"] = new[] { "Status must be a valid JobStatus value." };
        }
    }

    if (errors.Count > 0)
    {
        return Results.ValidationProblem(errors, extensions: BuildProblemExtensions());
    }

    var query = new JobListQuery(resolvedPage, resolvedPageSize, customerId, parsedStatus);
    var results = await jobService.ListAsync(query);
    return Results.Ok(results);
}).AllowAnonymous();

jobs.MapGet("/{jobId:guid}", async (AppJobService jobService, Guid jobId) =>
{
    var job = await jobService.GetAsync(jobId);
    return job is null ? Results.NotFound() : WithEtag(Results.Ok(job), job.UpdatedUtc);
}).AllowAnonymous();

ApplyWriteAuth(jobs.MapPost("/", async (AppJobService jobService, IValidator<CreateJobRequest> validator, CreateJobRequest request) =>
{
    var validation = await validator.ValidateAsync(request);
    if (!validation.IsValid) return Results.ValidationProblem(ToProblem(validation), extensions: BuildProblemExtensions());

    var job = await jobService.CreateAsync(request);
    return WithEtag(Results.Created($"/api/v1/jobs/{job.Id}", job), job.UpdatedUtc);
}), authEnabled);

ApplyWriteAuth(jobs.MapPut("/{jobId:guid}", async (AppJobService jobService, IValidator<UpdateJobRequest> validator, HttpRequest httpRequest, Guid jobId, UpdateJobRequest request) =>
{
    var validation = await validator.ValidateAsync(request);
    if (!validation.IsValid) return Results.ValidationProblem(ToProblem(validation), extensions: BuildProblemExtensions());

    var current = await jobService.GetAsync(jobId);
    if (current is null) return Results.NotFound();
    var etagProblem = ValidateIfMatch(httpRequest, current.UpdatedUtc);
    if (etagProblem is not null) return etagProblem;

    var job = await jobService.UpdateAsync(jobId, request);
    return job is null ? Results.NotFound() : WithEtag(Results.Ok(job), job.UpdatedUtc);
}), authEnabled);

ApplyWriteAuth(jobs.MapPatch("/{jobId:guid}", async (AppJobService jobService, IValidator<PatchJobRequest> validator, HttpRequest httpRequest, Guid jobId, PatchJobRequest request) =>
{
    var validation = await validator.ValidateAsync(request);
    if (!validation.IsValid) return Results.ValidationProblem(ToProblem(validation), extensions: BuildProblemExtensions());

    var current = await jobService.GetAsync(jobId);
    if (current is null) return Results.NotFound();
    var etagProblem = ValidateIfMatch(httpRequest, current.UpdatedUtc);
    if (etagProblem is not null) return etagProblem;

    var job = await jobService.PatchAsync(jobId, request);
    return job is null ? Results.NotFound() : WithEtag(Results.Ok(job), job.UpdatedUtc);
}), authEnabled);

ApplyWriteAuth(jobs.MapPost("/{jobId:guid}:post", async (AppJobService jobService, HttpRequest httpRequest, Guid jobId) =>
{
    var current = await jobService.GetAsync(jobId);
    if (current is null) return Results.NotFound();
    var etagProblem = ValidateIfMatch(httpRequest, current.UpdatedUtc);
    if (etagProblem is not null) return etagProblem;

    var job = await jobService.PostAsync(jobId);
    return job is null ? Results.NotFound() : WithEtag(Results.Ok(job), job.UpdatedUtc);
}), authEnabled);

ApplyWriteAuth(jobs.MapPost("/{jobId:guid}:accept", async (AppJobService jobService, IValidator<AcceptJobRequest> validator, HttpRequest httpRequest, Guid jobId, AcceptJobRequest request) =>
{
    var validation = await validator.ValidateAsync(request);
    if (!validation.IsValid) return Results.ValidationProblem(ToProblem(validation), extensions: BuildProblemExtensions());

    var current = await jobService.GetAsync(jobId);
    if (current is null) return Results.NotFound();
    var etagProblem = ValidateIfMatch(httpRequest, current.UpdatedUtc);
    if (etagProblem is not null) return etagProblem;

    var job = await jobService.AcceptAsync(jobId, request);
    return job is null ? Results.NotFound() : WithEtag(Results.Ok(job), job.UpdatedUtc);
}), authEnabled);

ApplyWriteAuth(jobs.MapPost("/{jobId:guid}:start", async (AppJobService jobService, HttpRequest httpRequest, Guid jobId) =>
{
    var current = await jobService.GetAsync(jobId);
    if (current is null) return Results.NotFound();
    var etagProblem = ValidateIfMatch(httpRequest, current.UpdatedUtc);
    if (etagProblem is not null) return etagProblem;

    var job = await jobService.StartAsync(jobId);
    return job is null ? Results.NotFound() : WithEtag(Results.Ok(job), job.UpdatedUtc);
}), authEnabled);

ApplyWriteAuth(jobs.MapPost("/{jobId:guid}:complete", async (AppJobService jobService, HttpRequest httpRequest, Guid jobId) =>
{
    var current = await jobService.GetAsync(jobId);
    if (current is null) return Results.NotFound();
    var etagProblem = ValidateIfMatch(httpRequest, current.UpdatedUtc);
    if (etagProblem is not null) return etagProblem;

    var job = await jobService.CompleteAsync(jobId);
    return job is null ? Results.NotFound() : WithEtag(Results.Ok(job), job.UpdatedUtc);
}), authEnabled);

ApplyWriteAuth(jobs.MapPost("/{jobId:guid}:close", async (AppJobService jobService, HttpRequest httpRequest, Guid jobId) =>
{
    var current = await jobService.GetAsync(jobId);
    if (current is null) return Results.NotFound();
    var etagProblem = ValidateIfMatch(httpRequest, current.UpdatedUtc);
    if (etagProblem is not null) return etagProblem;

    var job = await jobService.CloseAsync(jobId);
    return job is null ? Results.NotFound() : WithEtag(Results.Ok(job), job.UpdatedUtc);
}), authEnabled);

ApplyWriteAuth(jobs.MapPost("/{jobId:guid}:dispute", async (AppJobService jobService, HttpRequest httpRequest, Guid jobId) =>
{
    var current = await jobService.GetAsync(jobId);
    if (current is null) return Results.NotFound();
    var etagProblem = ValidateIfMatch(httpRequest, current.UpdatedUtc);
    if (etagProblem is not null) return etagProblem;

    var job = await jobService.DisputeAsync(jobId);
    return job is null ? Results.NotFound() : WithEtag(Results.Ok(job), job.UpdatedUtc);
}), authEnabled);

ApplyWriteAuth(jobs.MapPost("/{jobId:guid}:cancel", async (AppJobService jobService, HttpRequest httpRequest, Guid jobId) =>
{
    var current = await jobService.GetAsync(jobId);
    if (current is null) return Results.NotFound();
    var etagProblem = ValidateIfMatch(httpRequest, current.UpdatedUtc);
    if (etagProblem is not null) return etagProblem;

    var job = await jobService.CancelAsync(jobId);
    return job is null ? Results.NotFound() : WithEtag(Results.Ok(job), job.UpdatedUtc);
}), authEnabled);

bookings.MapGet("/{bookingId:guid}", async (BookingService bookingService, Guid bookingId) =>
{
    var booking = await bookingService.GetAsync(bookingId);
    return booking is null ? Results.NotFound() : WithEtag(Results.Ok(booking), booking.UpdatedUtc);
}).AllowAnonymous();

bookings.MapGet("/search", async (
    BookingService bookingService,
    int? page,
    int? pageSize,
    Guid? jobId,
    Guid? providerId,
    string? status,
    DateTime? startUtcFrom,
    DateTime? startUtcTo) =>
{
    var errors = new Dictionary<string, string[]>();
    var (resolvedPage, resolvedPageSize) = NormalizePaging(page, pageSize, errors);

    RentADad.Domain.Bookings.BookingStatus? parsedStatus = null;
    if (!string.IsNullOrWhiteSpace(status))
    {
        if (Enum.TryParse<RentADad.Domain.Bookings.BookingStatus>(status, true, out var bookingStatus))
        {
            parsedStatus = bookingStatus;
        }
        else
        {
            errors["status"] = new[] { "Status must be a valid BookingStatus value." };
        }
    }

    if (startUtcFrom is not null && startUtcTo is not null && startUtcFrom > startUtcTo)
    {
        errors["startUtcFrom"] = new[] { "StartUtcFrom must be earlier than StartUtcTo." };
    }

    if (errors.Count > 0)
    {
        return Results.ValidationProblem(errors, extensions: BuildProblemExtensions());
    }

    var query = new BookingListQuery(resolvedPage, resolvedPageSize, jobId, providerId, parsedStatus, startUtcFrom, startUtcTo);
    var results = await bookingService.ListAsync(query);
    return Results.Ok(results);
}).AllowAnonymous();

ApplyWriteAuth(bookings.MapPost("/", async (BookingService bookingService, IValidator<CreateBookingRequest> validator, CreateBookingRequest request) =>
{
    var validation = await validator.ValidateAsync(request);
    if (!validation.IsValid) return Results.ValidationProblem(ToProblem(validation), extensions: BuildProblemExtensions());

    var booking = await bookingService.CreateAsync(request);
    return WithEtag(Results.Created($"/api/v1/bookings/{booking.Id}", booking), booking.UpdatedUtc);
}), authEnabled);

ApplyWriteAuth(bookings.MapPost("/{bookingId:guid}:confirm", async (BookingService bookingService, HttpRequest httpRequest, Guid bookingId) =>
{
    var current = await bookingService.GetAsync(bookingId);
    if (current is null) return Results.NotFound();
    var etagProblem = ValidateIfMatch(httpRequest, current.UpdatedUtc);
    if (etagProblem is not null) return etagProblem;

    var booking = await bookingService.ConfirmAsync(bookingId);
    return booking is null ? Results.NotFound() : WithEtag(Results.Ok(booking), booking.UpdatedUtc);
}), authEnabled);

ApplyWriteAuth(bookings.MapPost("/{bookingId:guid}:decline", async (BookingService bookingService, HttpRequest httpRequest, Guid bookingId) =>
{
    var current = await bookingService.GetAsync(bookingId);
    if (current is null) return Results.NotFound();
    var etagProblem = ValidateIfMatch(httpRequest, current.UpdatedUtc);
    if (etagProblem is not null) return etagProblem;

    var booking = await bookingService.DeclineAsync(bookingId);
    return booking is null ? Results.NotFound() : WithEtag(Results.Ok(booking), booking.UpdatedUtc);
}), authEnabled);

ApplyWriteAuth(bookings.MapPost("/{bookingId:guid}:expire", async (BookingService bookingService, HttpRequest httpRequest, Guid bookingId) =>
{
    var current = await bookingService.GetAsync(bookingId);
    if (current is null) return Results.NotFound();
    var etagProblem = ValidateIfMatch(httpRequest, current.UpdatedUtc);
    if (etagProblem is not null) return etagProblem;

    var booking = await bookingService.ExpireAsync(bookingId);
    return booking is null ? Results.NotFound() : WithEtag(Results.Ok(booking), booking.UpdatedUtc);
}), authEnabled);

ApplyWriteAuth(bookings.MapPost("/{bookingId:guid}:cancel", async (BookingService bookingService, HttpRequest httpRequest, Guid bookingId) =>
{
    var current = await bookingService.GetAsync(bookingId);
    if (current is null) return Results.NotFound();
    var etagProblem = ValidateIfMatch(httpRequest, current.UpdatedUtc);
    if (etagProblem is not null) return etagProblem;

    var booking = await bookingService.CancelAsync(bookingId);
    return booking is null ? Results.NotFound() : WithEtag(Results.Ok(booking), booking.UpdatedUtc);
}), authEnabled);

providers.MapGet("/{providerId:guid}", async (ProviderService providerService, Guid providerId) =>
{
    var provider = await providerService.GetAsync(providerId);
    return provider is null ? Results.NotFound() : WithEtag(Results.Ok(provider), provider.UpdatedUtc);
}).AllowAnonymous();

providers.MapGet("/search", async (
    ProviderService providerService,
    int? page,
    int? pageSize,
    string? displayName) =>
{
    var errors = new Dictionary<string, string[]>();
    var (resolvedPage, resolvedPageSize) = NormalizePaging(page, pageSize, errors);

    if (errors.Count > 0)
    {
        return Results.ValidationProblem(errors, extensions: BuildProblemExtensions());
    }

    var query = new ProviderListQuery(resolvedPage, resolvedPageSize, displayName);
    var results = await providerService.ListAsync(query);
    return Results.Ok(results);
}).AllowAnonymous();

ApplyWriteAuth(providers.MapPost("/", async (ProviderService providerService, IValidator<RegisterProviderRequest> validator, RegisterProviderRequest request) =>
{
    var validation = await validator.ValidateAsync(request);
    if (!validation.IsValid) return Results.ValidationProblem(ToProblem(validation), extensions: BuildProblemExtensions());

    var provider = await providerService.RegisterAsync(request);
    return WithEtag(Results.Created($"/api/v1/providers/{provider.Id}", provider), provider.UpdatedUtc);
}), authEnabled);

ApplyWriteAuth(providers.MapPut("/{providerId:guid}", async (ProviderService providerService, IValidator<UpdateProviderRequest> validator, HttpRequest httpRequest, Guid providerId, UpdateProviderRequest request) =>
{
    var validation = await validator.ValidateAsync(request);
    if (!validation.IsValid) return Results.ValidationProblem(ToProblem(validation), extensions: BuildProblemExtensions());

    var current = await providerService.GetAsync(providerId);
    if (current is null) return Results.NotFound();
    var etagProblem = ValidateIfMatch(httpRequest, current.UpdatedUtc);
    if (etagProblem is not null) return etagProblem;

    var provider = await providerService.UpdateAsync(providerId, request);
    return provider is null ? Results.NotFound() : WithEtag(Results.Ok(provider), provider.UpdatedUtc);
}), authEnabled);

ApplyWriteAuth(providers.MapPost("/{providerId:guid}/availability", async (ProviderService providerService, IValidator<AddAvailabilityRequest> validator, HttpRequest httpRequest, Guid providerId, AddAvailabilityRequest request) =>
{
    var validation = await validator.ValidateAsync(request);
    if (!validation.IsValid) return Results.ValidationProblem(ToProblem(validation), extensions: BuildProblemExtensions());

    var current = await providerService.GetAsync(providerId);
    if (current is null) return Results.NotFound();
    var etagProblem = ValidateIfMatch(httpRequest, current.UpdatedUtc);
    if (etagProblem is not null) return etagProblem;

    var provider = await providerService.AddAvailabilityAsync(providerId, request);
    return provider is null ? Results.NotFound() : WithEtag(Results.Ok(provider), provider.UpdatedUtc);
}), authEnabled);

ApplyWriteAuth(providers.MapPut("/{providerId:guid}/availability", async (
    ProviderService providerService,
    IValidator<ReplaceAvailabilityRequest> validator,
    HttpRequest httpRequest,
    Guid providerId,
    ReplaceAvailabilityRequest request) =>
{
    var validation = await validator.ValidateAsync(request);
    if (!validation.IsValid) return Results.ValidationProblem(ToProblem(validation), extensions: BuildProblemExtensions());

    var current = await providerService.GetAsync(providerId);
    if (current is null) return Results.NotFound();
    var etagProblem = ValidateIfMatch(httpRequest, current.UpdatedUtc);
    if (etagProblem is not null) return etagProblem;

    var provider = await providerService.ReplaceAvailabilityAsync(providerId, request);
    return provider is null ? Results.NotFound() : WithEtag(Results.Ok(provider), provider.UpdatedUtc);
}), authEnabled);

ApplyWriteAuth(providers.MapDelete("/{providerId:guid}/availability/{availabilityId:guid}", async (ProviderService providerService, HttpRequest httpRequest, Guid providerId, Guid availabilityId) =>
{
    var current = await providerService.GetAsync(providerId);
    if (current is null) return Results.NotFound();
    var etagProblem = ValidateIfMatch(httpRequest, current.UpdatedUtc);
    if (etagProblem is not null) return etagProblem;

    var provider = await providerService.RemoveAvailabilityAsync(providerId, availabilityId);
    return provider is null ? Results.NotFound() : WithEtag(Results.Ok(provider), provider.UpdatedUtc);
}), authEnabled);

static Dictionary<string, string[]> ToProblem(ValidationResult validation)
{
    return validation.Errors
        .GroupBy(error => error.PropertyName)
        .ToDictionary(
            group => group.Key,
            group => group.Select(error => error.ErrorMessage).ToArray());
}

static IResult CreateProblem(int statusCode, string errorCode, string detail)
{
    return Results.Problem(
        statusCode: statusCode,
        title: "Domain rule violation",
        detail: detail,
        extensions: BuildProblemExtensions(errorCode));
}

static Dictionary<string, object?> BuildProblemExtensions(string? errorCode = null)
{
    var extensions = new Dictionary<string, object?>
    {
        ["traceId"] = Activity.Current?.TraceId.ToString() ?? string.Empty,
        ["version"] = ApiVersion.Current
    };

    if (!string.IsNullOrWhiteSpace(errorCode))
    {
        extensions["errorCode"] = errorCode;
    }

    return extensions;
}

static Dictionary<string, string> BuildRedactedRequestHeaders(HttpRequest request)
{
    var allowList = new[]
    {
        "User-Agent",
        "Content-Type",
        "Accept",
        "X-Correlation-Id"
    };

    var sensitive = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        "Authorization",
        "X-API-Key",
        "Cookie"
    };

    var headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
    foreach (var key in allowList)
    {
        if (!request.Headers.TryGetValue(key, out var value)) continue;
        headers[key] = sensitive.Contains(key) ? "***" : value.ToString();
    }

    return headers;
}

static Dictionary<string, string> BuildRedactedResponseHeaders(HttpResponse response)
{
    var allowList = new[]
    {
        "Content-Type",
        "Content-Length"
    };

    var headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
    foreach (var key in allowList)
    {
        if (!response.Headers.TryGetValue(key, out var value)) continue;
        headers[key] = value.ToString();
    }

    return headers;
}

static (int Page, int PageSize) NormalizePaging(int? page, int? pageSize, Dictionary<string, string[]> errors)
{
    var resolvedPage = page ?? 1;
    var resolvedPageSize = pageSize ?? 50;

    if (resolvedPage < 1)
    {
        errors["page"] = new[] { "Page must be 1 or greater." };
    }

    if (resolvedPageSize < 1 || resolvedPageSize > 200)
    {
        errors["pageSize"] = new[] { "PageSize must be between 1 and 200." };
    }

    return (resolvedPage, resolvedPageSize);
}

static IResult? ValidateIfMatch(HttpRequest httpRequest, DateTime updatedUtc)
{
    if (!httpRequest.Headers.TryGetValue("If-Match", out var values) || string.IsNullOrWhiteSpace(values))
    {
        return null;
    }

    var header = values.ToString();
    if (string.Equals(header, "*", StringComparison.OrdinalIgnoreCase))
    {
        return null;
    }

    if (!TryParseEtagTicks(header, out var ticks))
    {
        return CreateProblem(StatusCodes.Status400BadRequest, "etag_invalid", "If-Match must be a valid ETag.");
    }

    var currentTicks = updatedUtc.ToUniversalTime().Ticks;
    if (ticks != currentTicks)
    {
        return CreateProblem(StatusCodes.Status409Conflict, "etag_mismatch", "ETag does not match current resource version.");
    }

    return null;
}

static RouteHandlerBuilder ApplyWriteAuth(RouteHandlerBuilder builder, bool authEnabled)
{
    if (authEnabled)
    {
        builder.RequireAuthorization("WriteAccess");
    }

    return builder;
}

static bool TryParseEtagTicks(string header, out long ticks)
{
    ticks = 0;
    var value = header.Split(',')[0].Trim();
    if (value.StartsWith("W/"))
    {
        value = value[2..].Trim();
    }

    value = value.Trim('\"');
    return long.TryParse(value, out ticks);
}

static IResult WithEtag(IResult result, DateTime updatedUtc)
{
    return new ResultWithHeader(result, "ETag", CreateEtag(updatedUtc));
}

static string CreateEtag(DateTime updatedUtc)
{
    var ticks = updatedUtc.ToUniversalTime().Ticks;
    return $"W/\"{ticks}\"";
}


static void ValidateConfiguration(IConfiguration configuration, IHostEnvironment environment)
{
    if (environment.IsEnvironment("Testing")) return;

    var connectionString = configuration.GetConnectionString("Default");
    if (string.IsNullOrWhiteSpace(connectionString))
    {
        throw new InvalidOperationException("ConnectionStrings:Default is required.");
    }

    if (configuration.GetValue("Auth:Enabled", false))
    {
        var issuer = configuration["Auth:Issuer"];
        var audience = configuration["Auth:Audience"];
        var signingKey = configuration["Auth:SigningKey"];

        if (string.IsNullOrWhiteSpace(issuer) ||
            string.IsNullOrWhiteSpace(audience) ||
            string.IsNullOrWhiteSpace(signingKey))
        {
            throw new InvalidOperationException("Auth:Issuer, Auth:Audience, and Auth:SigningKey are required when Auth:Enabled is true.");
        }
    }
}

app.Run();

public partial class Program { }
