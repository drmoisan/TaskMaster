# Analyzer Baseline Correction

Timestamp: 2026-07-21T17:13:00Z

Command:

```powershell
$baselineSha = 'df5ad49c909f6b739edef45d0336151f44e827a6'; $pattern = '<Compile Include="OutlookObjects\\Folder\\PercentageFormatterTests\.cs"\s*/>'; $currentText = Get-Content -LiteralPath 'UtilitiesCS.Test/UtilitiesCS.Test.csproj' -Raw; $baselineText = git show "$baselineSha`:UtilitiesCS.Test/UtilitiesCS.Test.csproj"; if ($LASTEXITCODE -ne 0) { throw 'Unable to read baseline project.' }; $currentMatches = [regex]::Matches($currentText, $pattern).Count; $baselineMatches = [regex]::Matches(($baselineText -join "`n"), $pattern).Count; if ($baselineMatches -ne 2 -or $currentMatches -ne 2) { exit 1 }
```

EXIT_CODE: 0

BaselineCommitSHA: `df5ad49c909f6b739edef45d0336151f44e827a6`

BASELINE_DUPLICATE_INCLUDE_COUNT: 2

CURRENT_DUPLICATE_INCLUDE_COUNT: 2

CorrectedEffectiveWarningCount: 6

CorrectedEffectiveErrorCount: 0

Output Summary: The first full recompilation in final QA surfaced `CS2002` because `UtilitiesCS.Test/OutlookObjects/Folder/PercentageFormatterTests.cs` is specified twice. Both duplicate `Compile Include` entries already exist at the captured baseline SHA and remain unchanged by issue #400. The P0 post-restore build was incremental and did not emit this latent baseline diagnostic. The corrected effective analyzer baseline is the five recorded `System.Reactive` warnings plus this pre-existing `CS2002` warning, for six warnings and zero errors. The unrelated project entry is not edited.
