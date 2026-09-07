# [P9-T3] Pass-after run for issue #470 defect 3

Timestamp: 2026-08-26T11-02

Command:

```
dotnet tool run csharpier check .                                       # EXIT_CODE 0, 1,524 files
pwsh -NoProfile -File scripts/vscode/Invoke-VSBuild.ps1 -Target Build   # precondition, EXIT_CODE 0

$vswhere = Join-Path ${env:ProgramFiles(x86)} 'Microsoft Visual Studio\Installer\vswhere.exe'
$vstest  = & $vswhere -latest -products * -find 'Common7\IDE\Extensions\TestPlatform\vstest.console.exe' | Select-Object -First 1
& $vstest QuickFiler.Test\bin\Debug\QuickFiler.Test.dll `
    /Settings:scripts\vscode\TaskMaster.cli.runsettings `
    /InIsolation `
    /TestCaseFilter:"FullyQualifiedName~SetVisualDigits_WithNullItemController_SkipsTheGroupWithoutThrowing" `
    /Logger:"trx;LogFileName=p9-t3.trx" `
    /ResultsDirectory:docs\features\active\qfc-collection-controller-defects-468\evidence\regression-testing\p9-t3
```

EXIT_CODE: 0

## Output Summary

`Test Run Successful. Total tests: 1  Passed: 1`.

TRX `<Counters>`, verbatim from `evidence/regression-testing/p9-t3/p9-t3.trx`:

```
total="1" executed="1" passed="1" failed="0" error="0" timeout="0" aborted="0" inconclusive="0"
```

Passed count is exactly `1`, as the task's acceptance requires.

## Acceptance: the test additionally asserts that no viewer text was written

The test carries three assertions, and the second and third are the "no viewer text was written"
half of the acceptance:

| # | Assertion | What it establishes |
|---|---|---|
| 1 | `captured.Should().BeNull(...)` on the unwrapped `TargetInvocationException.InnerException` | the method no longer throws for a group with a missing controller or viewer |
| 2 | `_digitRefreshNeeded` reads `false` after the call | the method reached its final statement rather than aborting midway; combined with the arrangement, this is the direct proof that no viewer text was written |
| 3 | `liveController.VerifySet(item => item.ItemNumberDigits = It.IsAny<int>(), Times.Never())` | the group with a live controller and a null viewer was skipped *before* the controller write, not after it |

Assertion 2 is a proof, not a proxy, given the arrangement: **every group in the list has a null
`ItemViewer`**. Writing viewer text requires evaluating `grp.ItemViewer.LblItemNumber`, which on a
null viewer raises `NullReferenceException`. The method completed and set `_digitRefreshNeeded` to
`false`, so no such evaluation occurred. `ItemViewer` is a concrete WinForms `UserControl` rather
than an interface, so it can be neither mocked nor constructed in a unit test; this argument is the
strongest available observation and it is exact.

Assertion 3 closes the remaining gap. Without it, a fix that guarded only the viewer would still
pass assertions 1 and 2 while having written `ItemNumberDigits` on a group it was supposed to skip.

## Red-to-green

| Test | Fail-before artifact | Pre-fix observation | Post-fix |
|---|---|---|---|
| `SetVisualDigits_WithNullItemController_SkipsTheGroupWithoutThrowing` | `p9-t1-fail-before.2026-08-26T11-00.md` | inner exception `System.NullReferenceException` at `QfcCollectionController.cs:145` | passed |

The committed test is byte-for-byte the test that produced that red state; the arrangement was not
extended after the fix.

## What P9-T2 changed

The `SetVisualDigits` group loop now opens with a skip guard:

```
if (grp?.ItemController is null || grp.ItemViewer is null)
{
    return;
}
```

`return` inside the `ForEach` lambda is a per-element skip, so the remaining groups are still
processed. Both members are tested before either is dereferenced, which is what makes the guard
sufficient: guarding only the controller would leave `grp.ItemViewer.LblItemNumber` on the next
line reachable with the same arrangement, which is exactly the failure mode P9-T2's task text
identifies.

The now-redundant null-conditional in
`grp.ItemController?.ItemNumber.ToString(format) ?? 0.ToString(format)` collapsed to
`grp.ItemController.ItemNumber.ToString(format)`. That fallback was already dead code — the first
statement of the loop body dereferenced the same reference unguarded — and `int.ToString(string)`
never returns null, so the `??` arm could not be taken even in principle. Retaining it would have
left a second unreachable branch of exactly the kind Phase 6 removed from `GetMoveDiagnostics`.

## Host-identifier sanitisation

The TRX was sanitised case-insensitively before commit: 10 substitutions. Post-sanitisation the
file contains zero occurrences of any of the four host-identifier patterns recorded in
`evidence/other/host-identifier-sanitisation.2026-08-26T10-11.md`.
