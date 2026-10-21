# P2-T6 — #498 Host-Event, Control and Short-Circuit Tests

Timestamp: 2026-08-26T09-23

Command: `pwsh -NoProfile -Command '& "scripts\vscode\Invoke-VSBuild.ps1" -Target Build; $vsw = Join-Path ${env:ProgramFiles(x86)} "Microsoft Visual Studio\Installer\vswhere.exe"; $vs = & $vsw -latest -products * -find "Common7\IDE\Extensions\TestPlatform\vstest.console.exe" | Select-Object -First 1; & $vs "QuickFiler.Test\bin\Debug\QuickFiler.Test.dll" /InIsolation "/TestCaseFilter:FullyQualifiedName~SegmentDoubleClick_IndexAboveRange_ViaHostEvent|FullyQualifiedName~SegmentDoubleClick_NegativeIndex_ViaHostEvent|FullyQualifiedName~SegmentDoubleClick_ValidIndex_ViaHostEvent|FullyQualifiedName~SegmentDoubleClick_BannerRow_ViaHostEvent" "/Logger:trx;LogFileName=results.trx" "/ResultsDirectory:docs/features/active/breadcrumb-router-navigation-defects-498/evidence/regression-testing/trx/p2-t6"; "EXIT_CODE: $LASTEXITCODE"'`

EXIT_CODE: 0

## Output Summary

**All four pass.** TRX at
`docs/features/active/breadcrumb-router-navigation-defects-498/evidence/regression-testing/trx/p2-t6/results.trx`
records `<Counters total="4" executed="4" passed="4" failed="0" error="0" timeout="0" aborted="0" ... />`.

| Test | Purpose | Outcome |
|---|---|---|
| `SegmentDoubleClick_IndexAboveRange_ViaHostEvent_DoesNotThrowAndLeavesStateUnchanged` | AC-1 literal `_host.Raise(...)` boundary containment, `segmentIndex` 99 | Passed |
| `SegmentDoubleClick_NegativeIndex_ViaHostEvent_DoesNotThrowAndLeavesStateUnchanged` | AC-1 literal `_host.Raise(...)` boundary containment, `segmentIndex` -1 | Passed |
| `SegmentDoubleClick_ValidIndex_ViaHostEvent_CollapsesRowAndPostsRender` | AC-3 valid-index clause: the guard rejects invalid input only | Passed |
| `SegmentDoubleClick_BannerRow_ViaHostEvent_ShortCircuitsBeforeRangeCheck` | Banner short-circuit in `BreadcrumbRow.CollapseAfter` (`BreadcrumbRow.cs:202-205`) | Passed |

All four raise the host event through the async void seam
`_host.Raise(h => h.MessageReceived += null, _host.Object, ...)`, following the pattern already used
by `MalformedInboundJson_ViaHostEvent_IsContainedAtTheBoundary`
(`QuickFiler.Test/Controllers/BreadcrumbBridgeRouterQueueTests.cs:193-205`).

### Why these were authored and first executed here

The plan places these four after `P2-T4` deliberately, and the `P2-T3` RED run confirms why.
Pre-fix, the `SegmentDoubleClick` arm faulted the task returned by `ProcessInboundAsync` with an
`ArgumentOutOfRangeException`, which the single `catch (BreadcrumbMessageException)` at
`QuickFiler/Controllers/BreadcrumbBridgeRouter.cs:272` does not match. Through the async void seam
that exception would have reached `AsyncVoidMethodBuilder.SetException` on a thread with no
`SynchronizationContext`, producing an unhandled thread-pool exception capable of aborting the
vstest test host rather than failing a test. Running them before the guard existed would therefore
have produced either a false GREEN or an aborted run with no readable TRX. They were authored and
run here for the first time.

### Notes on the two control tests

- **Valid index.** Row `row-0` carries two segments, so index 0 is the valid non-leaf segment. The
  test asserts the posted count rose by exactly one and that the new payload is the row-scoped
  render naming `"rowId":"row-0"`. This confirms the `P2-T4` guard rejects invalid input only and
  does not suppress the legitimate collapse path.
- **Banner row.** The banner is bound as `row-0` ahead of the suggestion row. A banner carries a
  single inert segment, so index 0 passes the router's range guard and control reaches
  `BreadcrumbRow.CollapseAfter`, which returns `false` at its `Kind != BreadcrumbRowKind.Suggestion`
  short-circuit BEFORE its own range check. No throw and no render post result. This is the
  behavior the test name describes, and it is exercised rather than bypassed.

### Authoring correction made within this task

The first compile of the banner test failed with `CS0246` on `FolderScore`. The type is declared in
namespace `UtilitiesCS`, not `UtilitiesCS.OutlookObjects.Folder`, despite living under that folder
path. The `using` directive was corrected to `using UtilitiesCS;` and the assembly then compiled
with `0 Error(s)`. No other change was required and no production file was touched.

`QuickFiler.Test/Controllers/BreadcrumbBridgeRouterQueueTests.Part2.cs` now measures 195 lines,
well inside the 500-line limit, with six of its twelve planned methods present.

Satisfies AC-1 and the AC-3 valid-index clause.
