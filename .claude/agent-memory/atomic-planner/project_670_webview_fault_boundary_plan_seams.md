---
name: project-670-webview-fault-boundary-plan-seams
description: Issue #670 planning seams — the awaiter's IsCompleted identity check makes a "no pump" test fault on the wrong exception, a spec-fixed three-test set cannot reach the 90% new-module floor, and the coverage runner merges Cobertura classes BY FILENAME
metadata:
  type: project
---

Planning seams found while authoring the #670 atomic plan (QfcItemController `InitializeWebViewAsync`
fault boundary). Recorded because each was invisible from the spec and the research, both of which had
already been reviewed.

**1. `await _itemViewer.UiSyncContext` faults on the WRONG exception unless the supplied context is
already `SynchronizationContext.Current`.** `UiThread.SynchronizationContextAwaiter.IsCompleted` is
`_context == SynchronizationContext.Current` (`UtilitiesCS/Threading/UiThread.cs:100`). Hand a
controller a `Mock<IItemViewer>` returning a fresh `new SynchronizationContext()` and the awaiter is
NOT complete, so `OnCompleted` posts through the base `SynchronizationContext.Post`, which queues to
the thread pool — where `Current` is null and `TaskScheduler.FromCurrentSynchronizationContext()`
(`QfcItemController.ViewerSetup.cs:67`) throws `InvalidOperationException`. A test asserting the
seam's sentinel exception then captures the wrong type. Remedy: install that same instance as
`Current` for the test and restore the previous one in a `finally`; the whole path then runs inline.

**Why:** the research's §4.5 "no pump, no dispatcher, no `[Timeout]`" test design is correct in
outline and silently unrunnable as written.

**How to apply:** any plan that drives a member past `await <viewer>.UiSyncContext` outside a pump must
state the current-context arrangement, not just the mock's return value.

**2. A spec that fixes the test NAMES can still under-specify the test COUNT.** #670's spec names
exactly three tests and separately demands `>= 90%` line coverage on the new file (AC13) while AC3
requires a `catch (OperationCanceledException)` arm. Those three tests leave that arm uncovered, and
on a ~10-coverable-line file one uncovered arm is well under 90%. The spec's "if the planner elects to
add that arm" sentence is not optional — it is load-bearing. Elect it, put it in a file the ACs do not
pin, and say in prose why it is required rather than discretionary.

**3. `Invoke-MSTestWithCoverage.ps1` merges Cobertura `class` nodes BY FILENAME**
(`Merge-CoberturaClassesByFilename`, `scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1:262`),
which is what makes a per-file figure available for a partial class at all. But
`Assert-CoberturaLineCoverageThreshold` (`:341` of the runner) throws at the 80% repo-wide floor
BEFORE `Set-Content` writes the merged document at `:343`, so on a sub-80% run the file on disk is the
RAW unmerged output. Derive per-file and repo-wide figures by grouping `//class/lines/line` on
(filename, number) and taking max hits — that arithmetic is identical to the merge and is correct on
either document.

**4. Line-length budgeting decides the member's spelling.** `internal System.Action<string,
System.Exception> WebViewInitializationErrorSink { get; set; } =` is 102 columns at 8-space indent,
past CSharpier's 100 print width, so its post-format shape is unpredictable and cannot be asserted.
The sibling `EfcFormController.cs:128` fits only because `BoundaryErrorSink` is 13 characters shorter.
Split the accessors onto their own lines, format the new file immediately in the phase that creates
it, and assert only short single-line tokens afterwards. Same arithmetic killed an interpolated sink
message: `WebViewInitializationErrorSink($"WebView2 initialization failed: {ex.Message}", ex);` lands
on exactly column 100 at indent 16. A plain string literal is safe and log4net renders the exception
anyway.

**5. `InitializeWebViewAsync` is NOT a substring of `InitializeWebViewGuardedAsync`,** so the two
occurrence counts in `QfcItemController.Initialization.cs` are independent and can both be gated
exactly (3 guarded call sites; 5 residual `InitializeWebViewAsync` lines at 165, 193, 200, 256, 345,
of which only 256 is executable). Forbid the "at most a short `#670` comment" the spec allows at the
call sites — it is net-zero-line substitution or every line citation in the plan shifts.

See [[plan-rescope-after-sibling-landed-the-fix]] and [[evidence-path-normalization]]: #670's AC14
names `evidence/coverage/`, a non-canonical kind, and must be split to `evidence/baseline/` plus
`evidence/qa-gates/`.
