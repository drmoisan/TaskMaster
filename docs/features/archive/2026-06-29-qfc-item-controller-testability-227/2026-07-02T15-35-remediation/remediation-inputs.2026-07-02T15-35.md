# Remediation Inputs — Cycle 4 (Issue #227)

**Generated:** 2026-07-02T15-35 (orchestrator, cycle entry)
**Feature Folder:** `docs/features/active/2026-06-29-qfc-item-controller-testability-227/`
**Base Branch:** `main` (`4611fd60b7d1a782a8024f54cbfd4d28f6d4c264`)
**Head:** `TaskMaster-wt-2026-06-29-09-38` (`6291bdf6` — cycle-3 delivery, committed)
**Trigger:** Cycle-3 exit reaudit (`2026-07-02T15-26-audit/`) found `blocking_count = 6` (2 distinct
issues, each flagged in all three audit docs): a reduction-honesty defect in the test coverage of two
of the seventeen cycle-3 de-exemptions, and an uncommitted-delivery process gate. The process gate is
now resolved (commit `6291bdf6`). This cycle addresses the sole remaining code-quality finding.

## Cycle scope

### R1 — `ToggleFocus()`/`ToggleFocus(Enums.ToggleState)` reduction-honesty gap (Severity: Blocking, implementable)

- **Finding:** Cycle-3 de-exempted `QfcItemController.FocusAndTheme.cs`'s `ToggleFocus()` and
  `ToggleFocus(Enums.ToggleState)` (member #33/#35 in the seam-redesign re-audit research), citing the
  same handle-less-`Theme`/`_themes`-reflection-injection technique already proven for 14 sibling
  members. The delivered tests
  (`QuickFiler.Test/Controllers/QfcItemController.FocusAndThemeTests.cs:123-151`,
  `ToggleFocus_StateOverload_MarshalsThroughItemViewerInvoke` and
  `ToggleFocus_ParameterlessOverload_MarshalsThroughItemViewerInvoke`) construct a bare
  `new Mock<IItemViewer>()` and assert only `viewer.Verify(v => v.Invoke(It.IsAny<Delegate>()),
  Times.Once())` — Moq's default `Invoke` setup does not execute the passed delegate, so the method's
  entire substantive body (the `_activeUI`/`_activeTheme` state-machine, the `ToggleTips`/
  `RegisterFocusAsyncActions`/`UnregisterFocusAsyncActions` calls, and the terminal
  `_themes[_activeTheme].SetQfcTheme(async: false)` call) is never exercised or asserted.
- **The fix already exists in the same file.** `QfcItemController.FocusAndThemeTests.cs:99-114` defines
  `BuildExecutingViewer()` — a `Mock<IItemViewer>` whose `Invoke`/`BeginInvoke` setups call
  `d.DynamicInvoke()` on the passed delegate — already used elsewhere in this same test file. The two
  flagged tests simply did not use it.
- **Remediation:** Replace `new Mock<IItemViewer>()` with `BuildExecutingViewer()` in both tests (or add
  new tests alongside them if the marshaling-only assertion is still considered worth keeping
  separately), and assert the actual resulting behavior for both branches of each overload:
  - `ToggleFocus(Enums.ToggleState.On)` from an inactive state: `_activeUI` becomes `true`,
    `_activeTheme` switches to the `*Active` variant, and the call does not throw (the terminal
    `SetQfcTheme(async: false)` call against a handle-less `Theme` built via the existing
    `_themes`/`BuildAllThemes()` double must complete without a live control tree, mirroring the
    already-proven sibling tests for the 14 other `FocusAndTheme` members).
  - `ToggleFocus(Enums.ToggleState.Off)` from an active state: `_activeUI` becomes `false`,
    `_activeTheme` switches to the `*Normal` variant.
  - `ToggleFocus()` (parameterless): both the active→inactive and inactive→active toggle directions
    (mirrors the two-branch body at `QfcItemController.FocusAndTheme.cs:83-123`).
  - Keep or fold in a marshaling assertion (`Invoke` called) so the existing coverage is not lost, only
    strengthened.
- **Acceptance:** both tests exercise the full method body via `BuildExecutingViewer()`; assertions cover
  the `_activeUI`/`_activeTheme` transitions in both directions for each overload; no test relies on a
  live WinForms handle; the existing `BuildFocusController()`/`BuildAllThemes()` doubles are reused
  without modification (this is a test-only fix — no production code change is anticipated, but if the
  planner finds the terminal `SetQfcTheme(async:false)` call genuinely cannot complete against the
  handle-less double for a reason distinct from the 14 already-passing siblings, that is a scope-change
  finding to report, not to route around).
- **Route:** atomic_planner / atomic_executor (test-only fix).

### R2 — Uncommitted delivery (Severity: was Blocking, now RESOLVED)

- Cycle-3's full change set is committed as `6291bdf6`. No action needed this cycle. Recorded here only
  for continuity with the cycle-3 reaudit's finding list.

### Deferred (not a blocker this cycle, unchanged from cycle-3)

- Affected non-exempt denominator (77.40%) remains below the spec's 80% target on an absolute basis
  though improved +3.81pp with no regression this cycle — tracked under #197, consistent with prior
  cycles' disposition. No action this cycle.
- Canonical `artifacts/csharp/coverage.xml` is stale (cycle-1 dated) — minor evidence-freshness item,
  non-blocking; may be refreshed as part of this cycle's final QA gate if convenient, but is not a gate
  requirement.
- Ratification of the 24-member exemption boundary (`evidence/other/exemption-boundary.2026-07-02T15-05.md`)
  remains a maintainer governance action, not routed to an implementation delegate.

## Exit condition for cycle 4

`blocking_count == 0` across the re-audit (`code-review`, `feature-audit`, `policy-audit`), which
requires: both `ToggleFocus` tests genuinely exercise and assert the method's substantive behavior (not
merely the `Invoke` marshaling call), no regression in the other 15 already-verified cycle-3
de-exemptions, toolchain green, all files <= 500 lines, and the working tree committed before the next
reaudit.
