# P0-T10 — QuickFiler.Test baseline test result

Timestamp: 2026-09-07T14-12
Task: [P0-T10]
Issue: #796
Channel used: A

Command:

```
pwsh -NoProfile -Command '$vswhere = Join-Path ${env:ProgramFiles(x86)} "Microsoft Visual Studio\Installer\vswhere.exe"; $vstest = & $vswhere -latest -products * -find "Common7\IDE\Extensions\TestPlatform\vstest.console.exe" | Select-Object -First 1; & $vstest QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation "/TestCaseFilter:TestCategory!=LiveOutlook" /ResultsDirectory:TestResults\796\p0-t10 "/Logger:trx;LogFileName=p0-t10.trx"; "EXIT_CODE=$LASTEXITCODE"'
```

EXIT_CODE: 0

## Run summary, verbatim

```
Test Run Successful.
Total tests: 1370
     Passed: 1370
 Total time: 12.8905 Seconds
```

| Figure | Value | How obtained |
|---|---|---|
| Total | 1370 | read from the run summary |
| Passed | 1370 | read from the run summary |
| Failed | 0 | NOT PRINTED ON A PASSING RUN |
| Skipped | 0 | DERIVED as Total minus the sum of Passed and Failed |

The Failed count is recorded as 0 with the note `NOT PRINTED ON A PASSING RUN`
because vstest.console.exe emits no `Failed:` line when the run has no failures.

The Skipped count is DERIVED rather than read. vstest.console.exe prints no
`Skipped:` line on a run with no skipped tests, and the TRX `notExecuted` attribute
is hard-coded to 0, so neither source can supply the figure. The derivation used is
Total minus the sum of Passed and Failed: 1370 - (1370 + 0) = 0.

The `TestCategory!=LiveOutlook` filter EXCLUDES rather than skips. Tests it removes
appear in neither the Total figure nor the derived Skipped figure, so the derived
Skipped value is not inflated by filtering.

## BASELINE_FAILURE_SET

EMPTY. No test is Failed at baseline.

This named set is what the Phase 9 final run is compared against. This plan asserts
non-growth relative to this set; it does not assert a repository-wide "zero failed"
expectation anywhere.

## Raw output

The TRX is written to the gitignored path TestResults/796/p0-t10/p0-t10.trx and is
never committed, because a TRX embeds the host account name and machine name in its
`runUser` and `computerName` attributes. `.gitignore` line 39 ignores TestResults/
through the bracket class `[Tt]est[Rr]esult*/`.

Output Summary: 1370 tests run under the `TestCategory!=LiveOutlook` filter with
`/InIsolation`; 1370 Passed, 0 Failed, 0 Skipped (derived). EXIT_CODE 0.
BASELINE_FAILURE_SET is empty.
