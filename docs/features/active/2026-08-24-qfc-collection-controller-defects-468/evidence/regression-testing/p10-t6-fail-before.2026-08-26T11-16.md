# [P10-T6] [expect-fail] Issue #471 STA regression test, red before the fix

Timestamp: 2026-08-26T11-16

Command:

```
# Precondition (build first; a build failure blocks the task and is not a test result)
pwsh -NoProfile -File scripts/vscode/Invoke-VSBuild.ps1 -Target Build      # EXIT_CODE 0, 0 errors

$vswhere = Join-Path ${env:ProgramFiles(x86)} 'Microsoft Visual Studio\Installer\vswhere.exe'
$vstest  = & $vswhere -latest -products * -find 'Common7\IDE\Extensions\TestPlatform\vstest.console.exe' | Select-Object -First 1
& $vstest QuickFiler.Test\bin\Debug\QuickFiler.Test.dll `
    /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation `
    /TestCaseFilter:"FullyQualifiedName~EliminateSpaceForItems_ReducesMinimumHeightByTemplateHeightTimesRemovalCount" `
    /Logger:"trx;LogFileName=p10-t6.trx" `
    /ResultsDirectory:docs\features\active\qfc-collection-controller-defects-468\evidence\regression-testing\p10-t6
```

EXIT_CODE: 1

ExpectedExitCode: 1

## Output Summary

`Test Run Failed. Total tests: 1  Failed: 1`. Total time 1.84 s.

TRX `<Counters>`, verbatim from `evidence/regression-testing/p10-t6/p10-t6.trx`:

```
total="1" executed="1" passed="0" failed="1" error="0" timeout="0" aborted="0" inconclusive="0"
```

The failed count is exactly `1`, as the task's acceptance requires.

## The failure message shows a grow, not a shrink

Verbatim `<Message>` from the TRX:

```
Expected _panel.MinimumSize.Height to be 150 because removing 2 template rows of 25 px must
shrink the panel's minimum height from 200 px to 150 px; a larger value means the panel grew when
it should have shrunk, which is issue #471, but found 250 (difference of 100).
```

Starting minimum height 200 px, two template rows of 25 px removed. The correct result is 150 px.
The observed result is **250 px** — the panel grew by exactly the amount it should have shrunk by,
which is the signature of the double negation at the call site: the removal path negates the row
count and `ShrinkByRows` subtracts the resulting negative product, so the two negations cancel.
The 100 px difference is twice the 50 px row block, i.e. the shrink that did not happen plus the
grow that did.

## Why this run is red and must stay red until P10-T8

This is a genuine fail-before run against the real tree at commit
`6cac5a82` (the behaviour-neutral `ShrinkByRows` seam). No fix has been written. The test was
executed and this artifact records the observed non-zero exit code; no red state was back-filled
after a fix.

## Test-hygiene properties of the failing test

| Property | State |
|---|---|
| `Show()` / `ShowDialog()` | never called |
| Window handle created | no — the panel is never parented and never shown |
| Message pump required | no |
| Panel disposed | yes, in `[TestCleanup]` |
| Temporary files | none |
| Wall-clock waits | none |

The removal index passed is `_panel.RowCount + 1`, which is at or beyond the panel's row count, so
`TableLayoutHelper.RemoveSpecificRow` takes its `rowIndex >= panel.RowCount` early return and the
test observes the size arithmetic in isolation.

## Host-identifier sanitisation

The TRX was sanitised case-insensitively in binary mode before commit: 11 substitutions. The empty
`Deploy_<user> <timestamp>_<pid>` scaffolding directory vstest creates alongside the TRX (whose own
subdirectory name is the machine name) was removed. A post-sanitisation sweep over the TRX returns
zero hits for every token class recorded in
`evidence/other/host-identifier-sanitisation.2026-08-26T10-11.md`.
