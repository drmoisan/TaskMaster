---
name: webview2-endinit-creates-handles
description: ItemViewer construction already creates both WebView2 child window handles (and thus the viewer's own), measured; plus three pre-existing UtilitiesCS.Test flakes that surface under contention
metadata:
  type: project
---

`new QuickFiler.ItemViewer()` constructed on a pump thread already reports
`L0v2h2_WebView2.IsHandleCreated == true` AND `L0vhBreadcrumb_WebView2.IsHandleCreated == true`,
with no harness, no `SaveParameters`, and no `.Handle` read anywhere. The handles come from
`InitializeComponent`'s Designer-emitted `((ISupportInitialize)(...)).EndInit()` calls on the two
`Microsoft.Web.WebView2.WinForms.WebView2` children. Because WinForms creates a parent's handle when
a child's handle is created, this is also why the `ItemViewer`'s own `IsHandleCreated` is already
`true` on every run.

**Why:** #511/#571 planning predicted, from static reading, that the children would be handle-less
and that a `viewer.Handle` read (non-recursive) versus `CreateControl()` (Visible-gated, recursive)
would be observable through those two properties. Measured over four configurations, it is not: both
children are handle-created before either instrument runs. A plan task asserting
`IsHandleCreated == false` on them is unsatisfiable and its test fails for a reason unrelated to the
change under test. Full measurement in
`docs/features/active/winformspumphost-suite-determinism-511/evidence/regression-testing/webview-child-handle-measurement.2026-08-21T18-10.md`.

**How to apply:** Before authoring or accepting any assertion about `ItemViewer` child-control handle
state, measure it — the WebView2 source is not in this repository, so static reading cannot settle
it. Do not treat a `viewer.Handle` insertion as the cause when such an assertion fails; comment the
insertion out and re-run to attribute it. See [[project-418-plan-rationale-clauses-are-evidence]] for
the general class: plan prose stating unmeasured world state.

## Companion finding: three pre-existing `UtilitiesCS.Test` flakes under load

Across 30 post-fix full-suite runs, three distinct `UtilitiesCS.Test` tests failed intermittently,
none related to QuickFiler or the pump harness:

1. `Extensions.DfDeedle_COM_Tests.GetEmailDataInViewAsync_SeparatesTableSnapshotFromDataFrameTransform`
   — `NullReferenceException` under 100% CPU saturation.
2. `OutlookObjects.FilterDASL.DASLFilterParserTests.PrintTree_WritesIndentedTreeToConsole`
   — `Expected writer.ToString() "" to contain "AND"`.
3. `ReusableTypeClasses.StackGeek_Tests.Main_RunsSampleScenarioWithoutThrowing`
   — `Expected writer.ToString() "" to contain "Middle Element :"`.

Items 2 and 3 share a root cause: both assert on a redirected `Console.Out` writer that comes back
EMPTY, the signature of a shared-`Console.Out` race between parallel test classes.

**How to apply:** A plan gate demanding `failed == 0` across all nine assemblies on every one of N
consecutive runs is sensitive to these, and will fail for reasons a QuickFiler-scoped change cannot
influence. Expect roughly 1 failure per 10 full-suite runs under contention from this population.
Scope such a gate to the assemblies the change touches, or state the known-flaky carve-out
explicitly.
