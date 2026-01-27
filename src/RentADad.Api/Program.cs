using System.Linq;
using System.Text;
using System.Threading.RateLimiting;
using System.Diagnostics;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.IdentityModel.Tokens;
using RentADad.Api.Health;
using RentADad.Application.Abstractions.Persistence;
using RentADad.Application.Abstractions.Repositories;
using RentADad.Application.Bookings;
using RentADad.Application.Bookings.Requests;
using RentADad.Application.Bookings.Validators;
using RentADad.Application.Jobs;
using RentADad.Application.Providers;
using RentADad.Application.Providers.Requests;
using RentADad.Application.Jobs.Requests;
using RentADad.Application.Jobs.Validators;
using RentADad.Application.Providers.Validators;
using RentADad.Api.Seed;
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

    if (useSqlite)
    {
        options.UseSqlite(testConnection);
        return;
    }

    options.UseNpgsql(
        connectionString,
        npgsql => npgsql.MigrationsAssembly(typeof(AppDbContext).Assembly.GetName().Name));
});
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IJobRepository, JobRepository>();
builder.Services.AddScoped<IBookingRepository, BookingRepository>();
builder.Services.AddScoped<IProviderRepository, ProviderRepository>();
builder.Services.AddScoped<JobService>();
builder.Services.AddScoped<BookingService>();
builder.Services.AddScoped<ProviderService>();
builder.Services.AddValidatorsFromAssemblyContaining<CreateJobRequestValidator>();
builder.Services.AddProblemDetails();
builder.Services.AddScoped<DevSeeder>();
builder.Services.AddHealthChecks()
    .AddCheck("self", () => HealthCheckResult.Healthy(), tags: new[] { "live" })
    .AddCheck<DbReadyHealthCheck>("db", tags: new[] { "ready" });
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
        options.FallbackPolicy = new Microsoft.AspNetCore.Authorization.AuthorizationPolicyBuilder()
            .RequireAuthenticatedUser()
            .Build();
    });
}

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

if (authEnabled)
{
    app.UseAuthentication();
    app.UseAuthorization();
}

app.UseRateLimiter();
app.UseHttpsRedirection();

app.Use(async (context, next) =>
{
    if (!context.Request.Headers.TryGetValue("X-Correlation-Id", out var correlationId) || string.IsNullOrWhiteSpace(correlationId))
    {
        correlationId = Guid.NewGuid().ToString("N");
    }

    context.TraceIdentifier = correlationId!;
    context.Response.Headers["X-Correlation-Id"] = correlationId!;
    context.Response.Headers["X-Api-Version"] = ApiVersion.Current;

    await next();
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
if (autoMigrate && !app.Environment.IsEnvironment("Testing"))
{
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await dbContext.Database.MigrateAsync();
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

        var problem = exception switch
        {
            JobDomainException ex => CreateProblem(StatusCodes.Status409Conflict, ex.ErrorCode, ex.Message),
            BookingDomainException ex => CreateProblem(StatusCodes.Status409Conflict, ex.ErrorCode, ex.Message),
            ProviderDomainException ex => CreateProblem(StatusCodes.Status409Conflict, ex.ErrorCode, ex.Message),
            _ => CreateProblem(StatusCodes.Status500InternalServerError, "server_error", "An unexpected error occurred.")
        };

        return problem.ExecuteAsync(context);
    });
});

var jobs = app.MapGroup("/api/v1/jobs").WithTags("Jobs");
var bookings = app.MapGroup("/api/v1/bookings").WithTags("Bookings");
var providers = app.MapGroup("/api/v1/providers").WithTags("Providers");

jobs.MapGet("/", async (JobService jobService) =>
{
    var results = await jobService.ListAsync();
    return Results.Ok(results);
}).AllowAnonymous();

jobs.MapGet("/{jobId:guid}", async (JobService jobService, Guid jobId) =>
{
    var job = await jobService.GetAsync(jobId);
    return job is null ? Results.NotFound() : Results.Ok(job);
}).AllowAnonymous();

jobs.MapPost("/", async (JobService jobService, IValidator<CreateJobRequest> validator, CreateJobRequest request) =>
{
    var validation = await validator.ValidateAsync(request);
    if (!validation.IsValid) return Results.ValidationProblem(ToProblem(validation), extensions: BuildProblemExtensions());

    var job = await jobService.CreateAsync(request);
    return Results.Created($"/api/v1/jobs/{job.Id}", job);
});

jobs.MapPut("/{jobId:guid}", async (JobService jobService, IValidator<UpdateJobRequest> validator, Guid jobId, UpdateJobRequest request) =>
{
    var validation = await validator.ValidateAsync(request);
    if (!validation.IsValid) return Results.ValidationProblem(ToProblem(validation), extensions: BuildProblemExtensions());

    var job = await jobService.UpdateAsync(jobId, request);
    return job is null ? Results.NotFound() : Results.Ok(job);
});

jobs.MapPatch("/{jobId:guid}", async (JobService jobService, IValidator<PatchJobRequest> validator, Guid jobId, PatchJobRequest request) =>
{
    var validation = await validator.ValidateAsync(request);
    if (!validation.IsValid) return Results.ValidationProblem(ToProblem(validation), extensions: BuildProblemExtensions());

    var job = await jobService.PatchAsync(jobId, request);
    return job is null ? Results.NotFound() : Results.Ok(job);
});

jobs.MapPost("/{jobId:guid}:post", async (JobService jobService, Guid jobId) =>
{
    var job = await jobService.PostAsync(jobId);
    return job is null ? Results.NotFound() : Results.Ok(job);
});

jobs.MapPost("/{jobId:guid}:accept", async (JobService jobService, IValidator<AcceptJobRequest> validator, Guid jobId, AcceptJobRequest request) =>
{
    var validation = await validator.ValidateAsync(request);
    if (!validation.IsValid) return Results.ValidationProblem(ToProblem(validation), extensions: BuildProblemExtensions());

    var job = await jobService.AcceptAsync(jobId, request);
    return job is null ? Results.NotFound() : Results.Ok(job);
});

jobs.MapPost("/{jobId:guid}:start", async (JobService jobService, Guid jobId) =>
{
    var job = await jobService.StartAsync(jobId);
    return job is null ? Results.NotFound() : Results.Ok(job);
});

jobs.MapPost("/{jobId:guid}:complete", async (JobService jobService, Guid jobId) =>
{
    var job = await jobService.CompleteAsync(jobId);
    return job is null ? Results.NotFound() : Results.Ok(job);
});

jobs.MapPost("/{jobId:guid}:close", async (JobService jobService, Guid jobId) =>
{
    var job = await jobService.CloseAsync(jobId);
    return job is null ? Results.NotFound() : Results.Ok(job);
});

jobs.MapPost("/{jobId:guid}:dispute", async (JobService jobService, Guid jobId) =>
{
    var job = await jobService.DisputeAsync(jobId);
    return job is null ? Results.NotFound() : Results.Ok(job);
});

jobs.MapPost("/{jobId:guid}:cancel", async (JobService jobService, Guid jobId) =>
{
    var job = await jobService.CancelAsync(jobId);
    return job is null ? Results.NotFound() : Results.Ok(job);
});

bookings.MapGet("/{bookingId:guid}", async (BookingService bookingService, Guid bookingId) =>
{
    var booking = await bookingService.GetAsync(bookingId);
    return booking is null ? Results.NotFound() : Results.Ok(booking);
}).AllowAnonymous();

bookings.MapPost("/", async (BookingService bookingService, IValidator<CreateBookingRequest> validator, CreateBookingRequest request) =>
{
    var validation = await validator.ValidateAsync(request);
    if (!validation.IsValid) return Results.ValidationProblem(ToProblem(validation), extensions: BuildProblemExtensions());

    var booking = await bookingService.CreateAsync(request);
    return Results.Created($"/api/v1/bookings/{booking.Id}", booking);
});

bookings.MapPost("/{bookingId:guid}:confirm", async (BookingService bookingService, Guid bookingId) =>
{
    var booking = await bookingService.ConfirmAsync(bookingId);
    return booking is null ? Results.NotFound() : Results.Ok(booking);
});

bookings.MapPost("/{bookingId:guid}:decline", async (BookingService bookingService, Guid bookingId) =>
{
    var booking = await bookingService.DeclineAsync(bookingId);
    return booking is null ? Results.NotFound() : Results.Ok(booking);
});

bookings.MapPost("/{bookingId:guid}:expire", async (BookingService bookingService, Guid bookingId) =>
{
    var booking = await bookingService.ExpireAsync(bookingId);
    return booking is null ? Results.NotFound() : Results.Ok(booking);
});

bookings.MapPost("/{bookingId:guid}:cancel", async (BookingService bookingService, Guid bookingId) =>
{
    var booking = await bookingService.CancelAsync(bookingId);
    return booking is null ? Results.NotFound() : Results.Ok(booking);
});

providers.MapGet("/{providerId:guid}", async (ProviderService providerService, Guid providerId) =>
{
    var provider = await providerService.GetAsync(providerId);
    return provider is null ? Results.NotFound() : Results.Ok(provider);
}).AllowAnonymous();

providers.MapPost("/", async (ProviderService providerService, IValidator<RegisterProviderRequest> validator, RegisterProviderRequest request) =>
{
    var validation = await validator.ValidateAsync(request);
    if (!validation.IsValid) return Results.ValidationProblem(ToProblem(validation), extensions: BuildProblemExtensions());

    var provider = await providerService.RegisterAsync(request);
    return Results.Created($"/api/v1/providers/{provider.Id}", provider);
});

providers.MapPut("/{providerId:guid}", async (ProviderService providerService, IValidator<UpdateProviderRequest> validator, Guid providerId, UpdateProviderRequest request) =>
{
    var validation = await validator.ValidateAsync(request);
    if (!validation.IsValid) return Results.ValidationProblem(ToProblem(validation), extensions: BuildProblemExtensions());

    var provider = await providerService.UpdateAsync(providerId, request);
    return provider is null ? Results.NotFound() : Results.Ok(provider);
});

providers.MapPost("/{providerId:guid}/availability", async (ProviderService providerService, IValidator<AddAvailabilityRequest> validator, Guid providerId, AddAvailabilityRequest request) =>
{
    var validation = await validator.ValidateAsync(request);
    if (!validation.IsValid) return Results.ValidationProblem(ToProblem(validation), extensions: BuildProblemExtensions());

    var provider = await providerService.AddAvailabilityAsync(providerId, request);
    return provider is null ? Results.NotFound() : Results.Ok(provider);
});

providers.MapDelete("/{providerId:guid}/availability/{availabilityId:guid}", async (ProviderService providerService, Guid providerId, Guid availabilityId) =>
{
    var provider = await providerService.RemoveAvailabilityAsync(providerId, availabilityId);
    return provider is null ? Results.NotFound() : Results.Ok(provider);
});

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

static class ApiVersion
{
    public const string Current = "v1";
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
