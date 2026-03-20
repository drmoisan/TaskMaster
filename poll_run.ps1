$runId = "23322037558"
$maxWait = 1200
$elapsed = 0
$pollInterval = 30
Write-Host "Polling run $runId every $pollInterval seconds (max 20 min)..."
while ($elapsed -lt $maxWait) {
    $data = gh run view $runId --json status,conclusion,jobs | ConvertFrom-Json
    $status = $data.status
    $conclusion = $data.conclusion
    Write-Host "[$(Get-Date -Format 'HH:mm:ss')] Status: $status | Conclusion: $conclusion"
    if ($status -eq "completed") {
        Write-Host ""; Write-Host "=== Run Completed ===" 
        Write-Host "Conclusion: $conclusion"
        Write-Host ""; Write-Host "--- Jobs and Steps ---"
        foreach ($job in $data.jobs) {
            Write-Host ""; Write-Host "Job: $($job.name) | Conclusion: $($job.conclusion)"
            foreach ($step in $job.steps) {
                Write-Host "  Step: $($step.name) | Conclusion: $($step.conclusion)"
            }
        }
        if ($conclusion -ne "success") {
            Write-Host ""; Write-Host "=== Failure Logs ==="
            gh run view $runId --log-failed 2>&1 | Select-String -Pattern "error CS|##\[error\]|Build FAILED|Error\(s\)" | Select-Object -First 30 | ForEach-Object { $_.Line }
        }
        exit 0
    }
    Start-Sleep -Seconds $pollInterval
    $elapsed += $pollInterval
}
Write-Host "TIMEOUT after 20 min. Last status: $status"
exit 1
