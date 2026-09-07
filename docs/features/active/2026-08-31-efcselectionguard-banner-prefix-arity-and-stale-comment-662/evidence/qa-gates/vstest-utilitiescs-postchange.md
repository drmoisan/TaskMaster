# UtilitiesCS.Test Full-Assembly Post-Change Run (P2-T8)

Timestamp: 2026-09-01T16-35

Command: `C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\Extensions\TestPlatform\vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /Settings:TaskMaster.runsettings /EnableCodeCoverage /InIsolation "/TestCaseFilter:TestCategory!=LiveOutlook" "/Logger:trx;LogFileName=utilitiescs-postchange.trx" /ResultsDirectory:docs/features/active/efcselectionguard-banner-prefix-arity-and-stale-comment-662/evidence/qa-gates/p2-t8`

EXIT_CODE: 0

Output Summary:

`<Counters ... />` line from the produced TRX:

```
<Counters total="4783" executed="4783" passed="4783" failed="0" error="0" timeout="0" aborted="0" inconclusive="0" passedButRunAborted="0" notRunnable="0" notExecuted="0" disconnected="0" warning="0" completed="0" inProgress="0" pending="0" />
```

- total: 4783
- executed: 4783
- passed: 4783
- failed: 0

Console summary, transcribed:

```
Test Run Successful.
Total tests: 4783
     Passed: 4783
 Total time: 17.2304 Seconds
```

## Gate evaluation

The P0-T12 baseline recorded `failed="0"` and `passed="4783"` for this assembly.
This run's `failed` attribute is 0, which does not exceed the baseline, so this
step does not fail and the Phase 2 loop does not restart from P2-T1 on account
of it. The `passed` count is 4783, equal to the baseline and therefore not lower
than it.

The count is unchanged rather than incremented because this change adds no test
to `UtilitiesCS.Test`; the single new test was added to `QuickFiler.Test`. The
`FolderSuggestionTree` edit is covered by this assembly's existing
`FolderSuggestionTree` tests, all of which still pass, which is the behavioural
evidence for AC8's claim that no behavioural change reaches
`FolderSuggestionTree.IsBanner` or `BreadcrumbRowBuilder`.

This run completed on its first attempt with no failures. The intermittently
flaky `PhysicalFileInfoAdapter` test in this assembly passed.

## Staleness guard

- The results directory `.../evidence/qa-gates/p2-t8` was deleted before the run
  with `if (Test-Path $dir) { [System.IO.Directory]::Delete((Resolve-Path $dir).Path, $true) }`,
  where `$true` is the recursive flag.
- Produced TRX `LastWriteTime`: `Tuesday, September 1, 2026 4:35:32 PM`
  (16:35:32).
- P2-T1's `Timestamp:` for the current (final) loop pass: `2026-09-01T15-59`.
- 16:35:32 is later than 15:59, so the TRX belongs to the current pass. The
  BLOCKED branch does not arise.

## Invocation notes

The runsettings is the repository-root `TaskMaster.runsettings`, not the
`scripts\vscode` CLI variant, for the collector-exclusion reason recorded in the
plan. `/InIsolation` was supplied, per Decisions Record D10.
