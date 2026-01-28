param(
    [string]$BaseUrl = "http://localhost:8080",
    [int]$TimeoutSec = 10
)

$ErrorActionPreference = "Stop"

function Invoke-SmokeRequest {
    param(
        [string]$Url
    )

    $response = Invoke-WebRequest -Uri $Url -Method Get -TimeoutSec $TimeoutSec
    if ($response.StatusCode -ne 200) {
        throw "Smoke test failed for $Url (status $($response.StatusCode))."
    }
}

Write-Host "Running smoke tests against $BaseUrl"

Invoke-SmokeRequest "$BaseUrl/health/live"
Invoke-SmokeRequest "$BaseUrl/health/ready"
Invoke-SmokeRequest "$BaseUrl/api/v1/jobs"

Write-Host "Smoke tests passed."
