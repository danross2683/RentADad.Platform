param(
    [switch]$RunMigrations = $true,
    [switch]$SeedDemo = $false,
    [switch]$RunTests = $false
)

$ErrorActionPreference = "Stop"

Write-Host "Starting RentADad.Platform dev bootstrap..."

if (-not (Get-Command dotnet -ErrorAction SilentlyContinue)) {
    throw "dotnet SDK not found."
}

if (-not (Get-Command docker -ErrorAction SilentlyContinue)) {
    throw "Docker not found."
}

Write-Host "Restoring packages..."
dotnet restore .\RentADad.Platform.sln

Write-Host "Starting docker dependencies..."
docker compose up -d

if ($RunMigrations) {
    Write-Host "Applying migrations..."
    $env:ASPNETCORE_ENVIRONMENT = "Development"
    dotnet run --project .\src\RentADad.Api\RentADad.Api.csproj -- --apply-migrations-only
}

if ($SeedDemo) {
    Write-Host "Seeding demo data..."
    $env:ASPNETCORE_ENVIRONMENT = "Development"
    dotnet run --project .\src\RentADad.Api\RentADad.Api.csproj -- --seed-demo
}

if ($RunTests) {
    Write-Host "Running tests..."
    dotnet test .\src\RentADad.Tests\RentADad.Tests.csproj -v:n
}

Write-Host "Bootstrap complete."
