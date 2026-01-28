param(
    [string]$BaseUrl = "http://localhost:8080",
    [int]$Requests = 200,
    [int]$Concurrency = 10
)

$ErrorActionPreference = "Stop"
Write-Host "Load test: $Requests requests at concurrency $Concurrency"
Write-Host "Target: $BaseUrl/api/v1/jobs"

$codes = [System.Collections.Concurrent.ConcurrentDictionary[int,int]]::new()

$jobs = for ($i = 0; $i -lt $Requests; $i++) {
    Start-ThreadJob -ThrottleLimit $Concurrency -ScriptBlock {
        param($url, $codesRef)
        try {
            $resp = Invoke-WebRequest -Uri $url -Method Get -TimeoutSec 10
            $code = $resp.StatusCode
        } catch {
            if ($_.Exception.Response -and $_.Exception.Response.StatusCode) {
                $code = [int]$_.Exception.Response.StatusCode
            } else {
                $code = 0
            }
        }
        $codesRef.AddOrUpdate($code, 1, { param($k, $v) $v + 1 }) | Out-Null
    } -ArgumentList "$BaseUrl/api/v1/jobs", $codes
}

$jobs | Wait-Job | Out-Null

foreach ($key in ($codes.Keys | Sort-Object)) {
    "{0} {1}" -f $key, $codes[$key]
}
