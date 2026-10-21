# Code Review — Issue #677 (quickfiler-keyboard-hook-leaks-to-outlook)

- **Artifact:** `code-review.2026-08-28T12-31.md`
- **Branch:** `bug/quickfiler-keyboard-hook-leaks-to-outlook-677` @ `59bc2630` vs `main` @ merge base `361a49b8`
- **Files reviewed:** all 17 changed C#/csproj files in full; evidence and doc artifacts as supporting material

## Executive Summary

The fix is well-designed and tightly scoped. Part 1 (the execution-time `MayTakeFocus` predicate) attacks the root cause precisely at the scheduling/execution gap identified by the research artifact: the guard is read inside the scheduled action bodies (`FocusPending`, `FocusAnchorIfPermitted`), never captured at scheduling time, and a dedicated test proves the flip-after-scheduling case. Part 2 (deactivate-time focus parking and selector cancellation) closes both the WebView2 focus-retention path and the modal-menu-mode popup path. The seam choices are disciplined: a settable internal property preserves all six constructor arities (protecting five reflection-bound test harnesses), the predicate is assigned on the concrete host so `IBreadcrumbDropDownHost` and its mocks are untouched, and the deactivation contract follows the existing Seam-B event-routing pattern exactly.

The rejection of `ContainsFocus` in favor of `Form.ActiveForm` identity for the predicate is correct and well-argued in-code: the failure state this guard suppresses is precisely a WebView2 child of the deactivated form still holding Win32 thread focus, which keeps `ContainsFocus` true and would neuter the guard.

No blocking findings. Four non-blocking findings below; two follow-up promotions are owed at merge time (recorded in the policy audit, section 8).

## Findings Table

| Severity | File | Location | Finding | Recommendation | Rationale | Evidence |
|---|---|---|---|---|---|---|
| Minor | QuickFiler/Controllers/QfcFormController.Deactivate.cs | `FormViewer_Deactivated`, foreach over `_groups?.ItemGroups` | The handler enumerates the live `ItemGroups` list. The per-item try/catch protects the loop body, but an `InvalidOperationException` from the enumerator itself (if any `CancelBreadcrumbSelector` implementation ever mutates the group collection, e.g., a future cancel path that removes an item) would escape the WinForms event handler and surface as an unhandled UI-thread failure inside Outlook. Similarly, `IsWebView2Focused`/`ParkFocusOffWebView2` sit outside any catch. | Snapshot before iterating (`foreach (var group in groups.ToList())`) in a future touch of this file; no in-branch change requested. | Today's cancel chain (`viewer -> BreadcrumbCoordinator?.CancelSelector()`) does not mutate `ItemGroups`, so the risk is latent, not live; forcing the change now would violate the minimal-fix mandate. | Reviewed chain: `QfcItemController.FolderHandling.cs` line 135, `ItemViewer.FolderSearch.cs` line 43; handler at `QfcFormController.Deactivate.cs` lines 27–59 |
| Minor | QuickFiler/Viewers/QfcFormViewer.cs | `IsWebView2Focused` getter | The ActiveControl-chain walk identifies the WinForms-tracked focus leaf. In the exact #951 failure state, the WebView2 runtime's child HWND can hold real Win32 focus while the WinForms `ActiveControl` chain points elsewhere, in which case the parking step is skipped. Part 1's predicate still prevents every re-steal, so the composed fix remains sound, but the parking half is best-effort. | None in-branch. The live-Outlook runbook session (AC-1/AC-2) is the correct instrument to confirm the composed behavior; if the symptom recurs, the spec's focused-HWND logging fallback (Rollout item 3) is the designed next step. | WinForms cannot observe Win32 focus held by a foreign child HWND through `ActiveControl`; a `GetFocus()` P/Invoke would widen scope beyond the spec. | `QfcFormViewer.cs` lines 192–204; spec.md Rollout & Follow-up item 3 |
| Minor | QuickFiler/Viewers/BreadcrumbDropDownHost.cs | whole file | 498/500 lines after formatting. Compliant, but any future two-line addition breaches the ceiling. | Next change touching this file should apply the plan's D13 relocation remedy (move `FocusAnchorIfPermitted` to the `BreadcrumbDropDownHost.Open.cs` partial, currently 78 lines). | Prevents a forced refactor inside an unrelated future change. | Reviewer `awk NR` count; `evidence/qa-gates/file-size-audit.md` |
| Info | QuickFiler/Viewers/ItemViewer.Breadcrumb.cs | `MayRestoreBreadcrumbFocus` | Uses static `Form.ActiveForm` with a `ReferenceEquals` identity check against `FindForm()`. Correct for multi-form sessions (a second QuickFiler-family form active does not satisfy the predicate for this viewer's form) and correctly true on in-form Escape/commit paths, preserving #438/#400 behavior. The in-code remark explaining the `ContainsFocus` rejection is exemplary why-documentation. | None. | — | `ItemViewer.Breadcrumb.cs` lines 257–275 |

## Design and Test-Quality Notes

- **Execution-time guarantee is genuinely tested.** `FinishClose_PredicateFlipsFalseAfterScheduling_DoesNotFocusAnchor` asserts `PendingCount > 0` before flipping the predicate, proving the close work was queued rather than run inline — without that assertion the test would be vacuous under an ambient-context inline dispatch. The harness deliberately never installs the capturing context as ambient `SynchronizationContext`, and documents why that is load-bearing.
- **Both control cases are present** (predicate-true and unset-predicate-default), so the pre-fix behavior is pinned, not just the new suppression behavior. The cancel step is asserted un-gated (`CancelCount == 1` under predicate-false), matching the design rule that only the focus step is guarded.
- **Error-handling test** (`FormDeactivated_ItemCancelThrows_DoesNotPropagateAndContinues`) verifies both non-propagation and continuation to the remaining item — the two properties the per-item boundary catch exists to provide.
- **RED-first equivalence** is sound: the compile-red gate (22 diagnostics, all in the new test files, production compiling cleanly) plus the byte-identical green-flip command with a proof that no assertion changed in between is a valid substitute for a failing run when the referenced surface cannot exist pre-fix (see policy audit, section 2).
- **Structural enabler discipline:** the only pre-existing test-file edit is the interface-completing no-op on `FakeQfcItemController` (4 lines, no test behavior), exactly as sanctioned by the plan.
- **Interfaces carry XML docs** for every new member, including the failure-mode contract ("safe no-op when no viewer is attached").

## Deviation Verification (executor-disclosed)

1. **NuGet analyzer provisioning:** confirmed confined to the gitignored `packages/` directory. The branch diff contains no `packages.config` change and no `.csproj` change other than three additive `<Compile Include>` items; the pre-existing 16-csproj HintPath version skew (3.0.156/4.16.0 vs 3.0.174/4.16.1) is committed repository state that predates this branch. Follow-up promotion owed (policy audit section 8, item 2).
2. **Evidence sanitization:** confirmed lossless for adjudication purposes. All four committed TRX files parse as XML with internally consistent counters (1201/1201, 29/29, 9/9, 1218/1218) matching every prose claim, and a reviewer grep over the full 29.8 MB branch diff finds zero occurrences of the user-profile path prefix, account name, or machine name.

## Verdict

**0 blocking findings.** The change is ready from a code-quality standpoint. Non-blocking residuals: the two Minor hardening notes above, the 498-line file headroom, and the two follow-up promotions recorded in the policy audit.
