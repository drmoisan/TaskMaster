---
name: 791-review-residuals
description: "#791 QuickFiler HC deadline + Cancel teardown review: PASS/0 blocking, 6/6 AC; QfcDatamodel is [ExcludeFromCodeCoverage] so both partials emit ZERO Cobertura classes; the exception-safety invariant has an unprotected middle link in an AC5 non-goal file"
metadata:
  type: project
---

Review of `bug/quickfiler-high-confidence-cancel-teardown-and-deadline-defects-791`
(base `main` @ `7c8ac9ae`, head `59536368`), work mode `full-bug`. Outcome: PASS, 0 blocking,
6/6 AC, no remediation-inputs. Artifacts at timestamp `2026-09-06T15-31`.

## Durable facts about this file family

`QuickFiler/Controllers/QfcDatamodel.cs:25` carries `[ExcludeFromCodeCoverage]` on the **partial
class declaration**, so `QfcDatamodel.QueueProcessing.cs` and every other part emit **zero**
`<class>` elements in Cobertura, in both the baseline and the post-change document. Same shape as
`ItemViewer` in [[781-review-residuals]]. Any AC demanding a changed-line percentage on those files
is unevaluable by construction. Confirm by enumerating `<class filename=...>` over BOTH documents
before scoring it as something the branch introduced.

The teardown ownership chain is three links, and only two are protected:
`ActionCancelAsync` -> `finally` -> `QfcFormController.Cleanup()` (SetupDisposal.cs:213-261, **no
try/finally**, `_parentCleanup?.Invoke()` is the last statement) -> `QfcHomeController.Cleanup()` ->
`finally` -> `ParentCleanup` -> `RibbonController.ReleaseQuickFiler`. A throw from
`_formViewer?.Dispose()` at SetupDisposal.cs:251 skips the ribbon release. `SetupDisposal.cs` is an
explicit #791 AC5 non-goal, so the gap cannot be closed on that branch. When an audit says "the
release callback runs under a `finally`", walk **every** link, not the one the diff touches.

`RibbonController` never calls `QfcHomeController.Cleanup()` directly — it only supplies
`ReleaseQuickFiler` as the `parentCleanup` callback at `RibbonController.cs:106,120,141`. That is
what makes a repeat `QfcHomeController.Cleanup()` unreachable, and therefore what downgrades the
disposed-but-not-nulled `_tokenSource` (QfcHomeController.cs:389) from a live defect to a latent one.
The same CTS instance reaches the datamodel (`:125`) and the form controller (`:144`), and both
`QfcDatamodel.Cleanup()` and `QuiesceLoaderAsync()` open with `_tokenSource?.Cancel()`, which throws
`ObjectDisposedException` after `Dispose()`.

## Coverage figures this cycle (class-level `classes/class/lines/line`, nine first-party packages)

Baseline 55587/65783 = 84.50% line, 13204/16684 = 79.14% branch.
Post-change 55783/66009 = 84.51% line, 13292/16784 = 79.19% branch.
The delivery's `.//line` all-descendant selection reports ~2x those counters (112551/133187) — the
[[cobertura-class-line-double-count-trap]] — but the derived percentages match to the digit under
both selections, which is the useful cross-check.

Per-file, both documents, same selection: gate 97.54 -> 98.10; Deactivate 100 -> 100 (branch 90 ->
91.67); IQfcDatamodel 100 -> 100; `QfcHomeController.cs` 75.85 -> 76.36; **`QfcFormController.EventHandlers.cs`
49.61 -> 58.12**. The last two are below the 85% per-file floor and both improved; both carry
`using Microsoft.Office.Interop.Outlook` + `using System.Windows.Forms`, i.e. CLAUDE.md UT2 exemption
class (c). No delivery artifact reported per-file figures — computing them is what produced the row.

## Residuals owed at merge (none blocking)

1. Null `_tokenSource` after `Dispose()` in `QfcHomeController.Cleanup()`; promote to an issue.
2. Promote the unprotected `_parentCleanup?.Invoke()` in `QfcFormController.Cleanup()` (AC5 non-goal
   here, so it must be its own issue).
3. Promote the `QfcDatamodel` coverage exclusion as an extraction refactor: 115 new production lines
   landed inside the excluded type this cycle.
4. `LogScanBoundReached`'s content (`Bound=scan-cap` / `Bound=zero-acceptance-ceiling`,
   `Decision=stop`) is asserted by **no** test — grep for `scan bound reached` returns nothing —
   while the two sibling log lines are content-asserted. AC1 says "the bound decision is logged".
5. `runbooks/live-outlook-cancel-teardown-verification.runbook.md:16` embeds
   `C:\Users\<user>\repos\TaskMaster\...`; the only host-path leak on the branch.
6. Both `"… ribbon release callback invoked."` INFO lines (EventHandlers.cs:171,
   QfcHomeController.cs:401) are emitted unconditionally and can assert something that did not happen.
7. HI-1 (live-Outlook confirmation) outstanding by design; AC2 declares it non-gating.

## What was strong, and worth reusing as a pattern

The seven retargeted tests kept their pinning power: `sourceActive: () => true` so exhaustion is not
an available explanation for an empty batch; a cap of 4 substituted for a 4 s deadline so the
original take-count/residual assertions survive at exactly 4 and 6; a #608 pin with a deliberately
undersized cap so a widened guard fails it. `[P2-T15]` broke the #731 three-owner topology pin
because an `async` method hoists locals into a state-machine type — the repair moved the snapshot to
a **synchronous** helper rather than relaxing the pin from 3 to 4. That is the correct response and
is worth citing the next time a pin "has to" be loosened.
