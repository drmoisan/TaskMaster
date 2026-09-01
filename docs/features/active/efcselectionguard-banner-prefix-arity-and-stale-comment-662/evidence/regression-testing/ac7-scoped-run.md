# AC7 Scoped Test Run (P2-T6)

Timestamp: 2026-09-01T16-01

Command: `C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\Extensions\TestPlatform\vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation /Tests:IsSelectableFolder_AndIsBannerRow_ClassifyThreeAndFourEqualsRowsIdentically "/Logger:trx;LogFileName=ac7-scoped.trx" /ResultsDirectory:docs/features/active/efcselectionguard-banner-prefix-arity-and-stale-comment-662/evidence/regression-testing/p2-t6`

EXIT_CODE: 0

Output Summary:

`<Counters ... />` line from the produced TRX:

```
<Counters total="1" executed="1" passed="1" failed="0" error="0" timeout="0" aborted="0" inconclusive="0" passedButRunAborted="0" notRunnable="0" notExecuted="0" disconnected="0" warning="0" completed="0" inProgress="0" pending="0" />
```

`passed` attribute: **1**. `failed` attribute: **0**. Both match the figures
AC7 names.

Console summary line, transcribed verbatim:

```
Test Run Successful.
Total tests: 1
     Passed: 1
 Total time: 0.7577 Seconds
```

The pre-existing test
`IsSelectableFolder_AndIsBannerRow_ClassifyThreeAndFourEqualsRowsIdentically`
therefore still passes after this change, which is the behavioural half of AC7.

## Staleness guard

- The results directory `.../evidence/regression-testing/p2-t6` was deleted
  before the run with
  `if (Test-Path $dir) { [System.IO.Directory]::Delete((Resolve-Path $dir).Path, $true) }`,
  where `$true` is the recursive flag.
- Produced TRX `LastWriteTime`: `Tuesday, September 1, 2026 4:01:55 PM`
  (16:01:55).
- P2-T1's `Timestamp:` for the current (final) loop pass: `2026-09-01T15-59`.
- 16:01:55 is later than 15:59, so the TRX belongs to the current pass. The
  BLOCKED branch does not arise.

## Invocation notes

`/Settings:scripts\vscode\TaskMaster.cli.runsettings` is used here rather than
the repository-root file: this span passes no `/EnableCodeCoverage`, so no
collector is activated and no module-exclusion list is needed. `/InIsolation` was
supplied, per Decisions Record D10.
