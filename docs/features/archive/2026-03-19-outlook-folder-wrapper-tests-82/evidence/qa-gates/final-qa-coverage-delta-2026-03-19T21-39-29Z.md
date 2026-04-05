Timestamp: 2026-03-19T21:39:29.3239635Z
Command: pwsh -NoProfile -Command "$baselineRepo = [double](([regex]::Match((Get-Content (Get-ChildItem 'docs/features/active/2026-03-19-outlook-folder-wrapper-tests-82/evidence/baseline/baseline-test-*.md' | Sort-Object Name | Select-Object -Last 1).FullName -Raw), 'Repo Line Coverage: ([0-9]+(?:\.[0-9]+)?)%')).Groups[1].Value); [xml]$coverage = Get-Content 'coverage/coverage.cobertura.xml'; $repo = [math]::Round([double]$coverage.coverage.'line-rate' * 100, 2); $targets = @('UtilitiesCS/OutlookObjects/Folder/FolderConverter.cs','UtilitiesCS/OutlookObjects/Folder/FolderMinimalWrapper.cs','UtilitiesCS/OutlookObjects/Folder/FolderNavigator.cs','UtilitiesCS/OutlookObjects/Folder/FolderPredictor.cs','UtilitiesCS/OutlookObjects/Folder/FolderScorer.cs','UtilitiesCS/OutlookObjects/Folder/FolderTree.cs','UtilitiesCS/OutlookObjects/Folder/FolderWrapper .cs','UtilitiesCS/OutlookObjects/Folder/FolderWrapperNameAndParentNameComparer.cs','UtilitiesCS/OutlookObjects/Folder/FolderWrapperNameComparer.cs','UtilitiesCS/OutlookObjects/Folder/FolderWrapperNameCountSizeComparer.cs','UtilitiesCS/OutlookObjects/Folder/FolderWrapperNodeComparer.cs','UtilitiesCS/OutlookObjects/Folder/FolderWrapperNodeContentsComparer.cs','UtilitiesCS/OutlookObjects/Folder/MsgToMime/MAPIMethods.cs'); $under = New-Object System.Collections.Generic.List[string]; foreach ($target in $targets) { $normalizedTarget = $target.Replace('/','\\'); $class = $coverage.SelectNodes('//class') | Where-Object { $_.filename -eq $normalizedTarget } | Select-Object -First 1; if (-not $class) { Write-Output ('Missing=' + $target); exit 1 }; $rate = [math]::Round([double]$class.'line-rate' * 100, 2); Write-Output ($target + '=' + $rate + '%'); if ($rate -lt 80) { $under.Add($target + '=' + $rate + '%') } }; $changedFiles = @('UtilitiesCS/OutlookObjects/Folder/FolderConverter.cs','UtilitiesCS/OutlookObjects/Folder/FolderPredictor.cs') | Where-Object { (Test-Path $_) -and [bool](git diff --name-only -- $_) }; $changedCoverageMet = $true; foreach ($changedFile in $changedFiles) { $normalizedChangedFile = $changedFile.Replace('/','\\'); $class = $coverage.SelectNodes('//class') | Where-Object { $_.filename -eq $normalizedChangedFile } | Select-Object -First 1; $hitsByLine = @{}; foreach ($lineNode in $class.SelectNodes('./lines/line')) { $hitsByLine[[int]$lineNode.number] = [int]$lineNode.hits }; $diff = git diff --unified=0 -- $changedFile; $changedCovered = 0; $changedTotal = 0; foreach ($line in $diff) { if ($line -match '^@@ -(\d+)(?:,(\d+))? \+(\d+)(?:,(\d+))? @@') { $newStart = [int]$Matches[3]; $newCount = if ($Matches[4]) { [int]$Matches[4] } else { 1 }; if ($newCount -gt 0) { for ($i = $newStart; $i -le ($newStart + $newCount - 1); $i++) { if ($hitsByLine.ContainsKey($i)) { $changedTotal++; if ($hitsByLine[$i] -gt 0) { $changedCovered++ } } } } } }; $changedPercent = if ($changedTotal -eq 0) { 100 } else { [math]::Round(($changedCovered / $changedTotal) * 100, 2) }; Write-Output ($changedFile + ':ChangedCoverage=' + $changedPercent + '%'); if ($changedPercent -lt 90) { $changedCoverageMet = $false } }; $repoDelta = [math]::Round($repo - $baselineRepo, 2); $repoCoverageBelow80 = $repo -lt 80; Write-Output ('BaselineRepo=' + $baselineRepo + '%'); Write-Output ('FinalRepo=' + $repo + '%'); Write-Output ('RepoDelta=' + $repoDelta + '%'); Write-Output ('RepoCoverageBelow80=' + $repoCoverageBelow80); Write-Output ('RepoCoverageExceptionRequired=' + $repoCoverageBelow80); Write-Output ('AnyFileUnder80=' + [bool]($under.Count -gt 0)); Write-Output ('ChangedProductionCoverageMet=' + $changedCoverageMet); if ($repo -lt $baselineRepo -or $under.Count -gt 0 -or -not $changedCoverageMet) { exit 1 }"
EXIT_CODE: 0
Output Summary:
UtilitiesCS/OutlookObjects/Folder/FolderConverter.cs=95.95%
UtilitiesCS/OutlookObjects/Folder/FolderMinimalWrapper.cs=91.11%
UtilitiesCS/OutlookObjects/Folder/FolderNavigator.cs=100%
UtilitiesCS/OutlookObjects/Folder/FolderPredictor.cs=85.86%
UtilitiesCS/OutlookObjects/Folder/FolderScorer.cs=93.34%
UtilitiesCS/OutlookObjects/Folder/FolderTree.cs=85.18%
UtilitiesCS/OutlookObjects/Folder/FolderWrapper .cs=81.58%
UtilitiesCS/OutlookObjects/Folder/FolderWrapperNameAndParentNameComparer.cs=97.73%
UtilitiesCS/OutlookObjects/Folder/FolderWrapperNameComparer.cs=100%
UtilitiesCS/OutlookObjects/Folder/FolderWrapperNameCountSizeComparer.cs=100%
UtilitiesCS/OutlookObjects/Folder/FolderWrapperNodeComparer.cs=82.42%
UtilitiesCS/OutlookObjects/Folder/FolderWrapperNodeContentsComparer.cs=92.86%
UtilitiesCS/OutlookObjects/Folder/MsgToMime/MAPIMethods.cs=100%
UtilitiesCS/OutlookObjects/Folder/FolderConverter.cs:ChangedCoverage=100%
UtilitiesCS/OutlookObjects/Folder/FolderPredictor.cs:ChangedCoverage=100%
BaselineRepo=42.2%
FinalRepo=44.66%
RepoDelta=2.46%
RepoCoverageBelow80=True
RepoCoverageExceptionRequired=True
AnyFileUnder80=False
ChangedProductionCoverageMet=True
Notes:
- Cobertura class filenames use Windows-style backslashes, so the verification normalizes target paths before matching XML nodes.
- Changed production line coverage is evaluated only for executable lines that Cobertura reports, which is the applicable subset for the plan's `when applicable` requirement.
- Repository-wide coverage remains below 80%, so the documented repo-wide coverage exception is required, but coverage did not regress below baseline.