Timestamp: 2026-03-18T22:29:25-04:00
Command: pwsh -NoProfile -Command '[xml]$coverage = Get-Content "coverage/coverage.cobertura.xml"; $class = $coverage.SelectNodes("//class") | Where-Object { $_.filename -eq "UtilitiesCS\\OutlookObjects\\AppointmentItem\\MeetingItemHelper.cs" } | Select-Object -First 1; if (-not $class) { exit 1 }; $fileCoverage = [math]::Round([double]$class."line-rate" * 100, 2); Write-Output ("MeetingItemHelper Line Coverage: $fileCoverage%")'
EXIT_CODE: 0
Output Summary: MeetingItemHelper Line Coverage: 27.65%
