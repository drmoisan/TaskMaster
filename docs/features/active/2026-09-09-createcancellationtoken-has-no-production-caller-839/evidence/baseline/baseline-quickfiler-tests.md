# Baseline: QuickFiler.Test under coverage — issue #839 — RED, Decision D14 HALT

Timestamp: 2026-09-13T03-00
Command: pwsh -NoProfile -Command '. ./scripts/vscode/Invoke-MSTestWithCoverage.ps1; $vswhere = Join-Path ${env:ProgramFiles(x86)} "Microsoft Visual Studio\Installer\vswhere.exe"; $vstest = & $vswhere -latest -products * -find "Common7\IDE\Extensions\TestPlatform\vstest.console.exe" | Select-Object -First 1; New-Item -ItemType Directory -Force -Path coverage | Out-Null; $derived = ConvertTo-DerivedCoverageSettingsXml -CanonicalSettingsXml (Get-Content -LiteralPath coverage.config -Raw -Encoding UTF8); Set-Content -LiteralPath coverage/839-effective-coverage.config -Value $derived -Encoding UTF8 -NoNewline; & dotnet-coverage collect --output coverage/839-baseline.cobertura.xml --output-format cobertura --settings coverage/839-effective-coverage.config -- $vstest QuickFiler.Test/bin/Debug/QuickFiler.Test.dll /Settings:scripts/vscode/TaskMaster.cli.runsettings /InIsolation "/logger:console;verbosity=normal" "/TestCaseFilter:TestCategory!=LiveOutlook" | Tee-Object -FilePath coverage/839-baseline-tests.log; $code = $LASTEXITCODE; "COVERAGE_RUN_EXIT=$code"; "COBERTURA_EXISTS=$(Test-Path -LiteralPath coverage/839-baseline.cobertura.xml)"; exit $code'
Command: pwsh -NoProfile -Command '$f = "coverage/839-baseline-tests.log"; $colon = [string][char]58; foreach ($k in @("Total tests", "Passed", "Failed", "Skipped")) { $pat = "^\s*" + [regex]::Escape($k + $colon) + "\s*(\d+)"; $m = @(Select-String -Path $f -Pattern $pat); $v = 0; if ($m.Count -gt 0) { $v = $m[-1].Matches[0].Groups[1].Value }; $k + " LINES=" + $m.Count + " VALUE=" + $v }'
EXIT_CODE: 1

## Verdict

HALT under Decision D14. The Phase 0 baseline QuickFiler.Test run is RED with three failed tests. The D14 bounded repeat was used and the same three named tests failed again, so the halt stands. Phase 1, Phase 2 and Phase 3 were not started and no source file was edited.

## Run 1 (first baseline attempt)

Timestamp: 2026-09-13T02-57
COVERAGE_RUN_EXIT=1
COBERTURA_EXISTS=True
Total tests LINES=1 VALUE=1393
Passed LINES=1 VALUE=1390
Failed LINES=1 VALUE=3
Skipped LINES=0 VALUE=0 (the line is absent; recorded as 0 with the absence stated, per the P0-T18 derivation rule)

Failed test lines, verbatim and trimmed:
Failed InitEmailQueue_ZeroBatchSize_ReturnsEmptyListWithoutThrowing [7 ms]
Failed InitEmailQueue_ZeroBatchSize_StillStartsBackgroundWorker [< 1 ms]
Failed InitEmailQueue_PositiveBatchSize_RetainsExistingProjectionAndFrameDrop [< 1 ms]

All three are declared on QuickFiler.Controllers.Tests.QfcInitEmailQueueZeroBatchTests. None is in this item's Write Set and none is one of the three tests this plan's gates name.

## Run 2 (the single Decision D14 repeat, NOT a toolchain-loop restart)

Timestamp: 2026-09-13T03-00
This is the one bounded repeat D14 permits. It was run with no intervening file change of any kind: no source file, no project file and no settings file was touched between run 1 and run 2.
COVERAGE_RUN_EXIT=1
COBERTURA_EXISTS=True
Test Run Failed.
Total tests: 1393
Passed: 1390
Failed: 3

The same three named tests failed. Under D14 the halt therefore stands.

## Root cause, measured

Every one of the three failures carries the same error message:

System.TypeInitializationException: The type initializer for 'Deedle.Reflection' threw an exception. ---> System.TypeInitializationException: The type initializer for '<StartupCode$Deedle>.$FrameUtils' threw an exception. ---> System.IO.FileNotFoundException: Could not load file or assembly 'netstandard, Version=2.1.0.0, Culture=neutral, PublicKeyToken=cc7b13ffcd2ddd51' or one of its dependencies. The system cannot find the file specified.

This is an assembly-resolution failure for the netstandard 2.1 reference assembly, reached through the Deedle F# data-frame library. Supporting measurements:

- QuickFiler.Test/packages.config line 7 pins Deedle 3.0.0 for net481. QuickFiler/packages.config line 6 pins the same Deedle 3.0.0 and line 31 additionally pins NETStandard.Library 2.0.3, which supplies netstandard.dll version 2.0, not 2.1. QuickFiler.Test/packages.config carries no NETStandard.Library entry.
- netstandard.dll is absent from QuickFiler.Test/bin/Debug (measured: False).
- QuickFiler.Test/app.config contains zero lines mentioning netstandard, so there is no binding redirect from 2.1.0.0 to 2.0.0.0.

Closing this gap would require editing tracked project or configuration files (a packages.config entry, a project reference, or an app.config binding redirect) in QuickFiler.Test. Every one of those files is outside this item's Write Set and outside this plan's scope, and no task in this plan authorizes such an edit. It is therefore reported rather than worked around.

## The failure is NOT caused by coverage instrumentation

A recorded prior observation on this repository is that dotnet-coverage instrumenting Deedle and FSharp assemblies fails this same InitEmailQueue family, and that the remedy is to exclude those modules from instrumentation. That remedy is already in force here and is not the explanation:

- coverage.config lines 14 and 15 exclude `.*Deedle.*` and `.*FSharp.*` from instrumentation.
- The derived settings file the plan generates, coverage/839-effective-coverage.config, was read back and carries both of those excludes plus the appended test-assembly module exclusion.

A confirming diagnostic was then run: the identical assembly, settings, isolation switch and test-case filter, with dotnet-coverage removed entirely so no instrumentation occurred at all.

Diagnostic timestamp: 2026-09-13T03-01
Command: pwsh -NoProfile -Command '$vswhere = Join-Path ${env:ProgramFiles(x86)} "Microsoft Visual Studio\Installer\vswhere.exe"; $vstest = & $vswhere -latest -products * -find "Common7\IDE\Extensions\TestPlatform\vstest.console.exe" | Select-Object -First 1; & $vstest QuickFiler.Test/bin/Debug/QuickFiler.Test.dll /Settings:scripts/vscode/TaskMaster.cli.runsettings /InIsolation "/logger:console;verbosity=normal" "/TestCaseFilter:TestCategory!=LiveOutlook" | Tee-Object -FilePath coverage/839-noinstrument-diagnostic-tests.log; "VSTEST_EXIT=$LASTEXITCODE"'
VSTEST_EXIT=1
Test Run Failed. Total tests: 1393, Passed: 1390, Failed: 3.
The same three tests failed:
Failed InitEmailQueue_ZeroBatchSize_ReturnsEmptyListWithoutThrowing [27 ms]
Failed InitEmailQueue_ZeroBatchSize_StillStartsBackgroundWorker [< 1 ms]
Failed InitEmailQueue_PositiveBatchSize_RetainsExistingProjectionAndFrameDrop [1 ms]
NETSTANDARD21_MESSAGE_LINES=3

The failure is therefore deterministic across three consecutive runs, reproduces with zero instrumentation, and is not the load-flakiness D14's bounded exception contemplates.

## Why this is pre-existing and not introduced by this run

- The tracked tree at HEAD differs from the BASE-SHA only by the five feature-folder Markdown documents recorded in the base-anchor artifact. No source, project or configuration file differs from the base.
- No source file has been edited by this run. Phase 1 and Phase 2 were never started.
- The only environment changes this run made are the Phase 0 bootstrap steps: the repo-local SDK install, the dotnet tool restore, the dotnet-coverage probe, the NuGet restore, and the provisioning of the missing gitignored Meziantou.Analyzer 3.0.203 folder recorded in the analyzers baseline. None of these affects runtime resolution of netstandard or Deedle, and the analyzer package participates only in compilation.

The conclusion the evidence supports is that a clean-worktree build of this repository at the base commit produces three failing QuickFiler.Test tests from an unresolvable netstandard 2.1 reference. Whether an established developer machine masks this through a lingering gitignored artifact was not determined, because reading another worktree was out of bounds for this run.

## Gate results against the [P0-T18] acceptance

- EXIT_CODE: 0 — NOT MET (observed 1 on both runs).
- COBERTURA_EXISTS=True — MET.
- RUN_SUCCESSFUL_LINES=1 — NOT MET. Run 2 printed `Test Run Failed.` instead.
- Failed count 0 — NOT MET (observed 3 on both runs).
- PASSED_Init_InitializesCorrectly=1 — not separately established, because the halt condition was already met and the run is red.
- PASSED_Cleanup_NullsTokenSourceSoLaterCancelCannotReachDisposedSource=1 — this test was observed as Passed in the run 1 console output.
- PASSED_Init_CreatesTokenSourceBeforeAnyLoaderObservesIt=0 — MET; the test does not exist yet.

## Consequence for the acceptance criteria

AC5 requires zero failed tests across the QuickFiler.Test assembly on the post-fix tree. With three pre-existing failures unrelated to this item, AC5 is unsatisfiable by this plan as written, and the exit-0 conditions on [P2-T6], [P3-T4] and [P3-T6] cannot pass either. Decision D14 anticipates exactly this and directs the executor to report BLOCKED with this artifact and stop rather than continue. That is what was done. All twelve acceptance criteria in spec.md remain unchecked.

Raw disposition: the Cobertura output, the msbuild detailed logs and the vstest console logs referenced above live under the gitignored coverage directory and are not committed. Only this projection is committed.
