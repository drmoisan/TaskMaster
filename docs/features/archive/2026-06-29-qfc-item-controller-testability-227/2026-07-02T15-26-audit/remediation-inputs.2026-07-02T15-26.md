# Remediation Inputs: QfcItemController / IItemViewer Testability Refactor — Cycle-3 Exit (Issue #227)

**Generated:** 2026-07-02T15-26
**Feature Folder:** `docs/features/active/2026-06-29-qfc-item-controller-testability-227/`
**Base Branch:** `main` (`4611fd60b7d1a782a8024f54cbfd4d28f6d4c264`)
**Head:** `TaskMaster-wt-2026-06-29-09-38` (`0a212191b780ad953d57a709b43aae99cbcd2959` committed + uncommitted cycle-3 working tree)
**Source audits:**
- `policy-audit.2026-07-02T15-26.md`
- `code-review.2026-07-02T15-26.md`
- `feature-audit.2026-07-02T15-26.md`

## Disposition Overview

Cycle 3's toolchain, structural, and process mechanics are fully compliant: green toolchain in order,
4440/4440 tests pass with zero regressions, all files ≤ 500 lines except the documented pre-existing
`FolderPredictor.cs` exception, and no evidence-location violations. An independent reduction-honesty
re-verification (explicitly requested for this cycle) found one material code-quality gap plus the
recurring uncommitted-delivery process gate. No maintainer-governance item is newly introduced this
cycle (exemption-boundary ratification remains pending from cycle 2, as before).

Overall recommendation: **Needs Revision** — resolve R1 and R2 (both implementation-routable), then the
change is ready for the maintainer-ratification step already pending from cycle 2.

---

## R1 — `ToggleFocus()`/`ToggleFocus(Enums.ToggleState)` de-exemption is not behaviorally verified

- **Severity: Blocking**
- **Type:** Code quality / test-quality (implementation task)
- **Finding:** `QuickFiler/Controllers/QfcItemController.FocusAndTheme.cs:27-67`
  (`ToggleFocus(Enums.ToggleState)`) and `:83-123` (`ToggleFocus()`) wrap their entire bodies in a
  single `_itemViewer.Invoke(...)` call. The two tests added to de-exempt these members
  (`QuickFiler.Test/Controllers/QfcItemController.FocusAndThemeTests.cs:124-151`,
  `ToggleFocus_StateOverload_MarshalsThroughItemViewerInvoke` and
  `ToggleFocus_ParameterlessOverload_MarshalsThroughItemViewerInvoke`) assert only that `Invoke` was
  called once; per the tests' own inline comment, "its delegate body is never executed." No test
  verifies the `_activeUI`/`_activeTheme` state transition, the `RegisterFocusAsyncActions`/
  `UnregisterFocusAsyncActions` calls, or the `_themes[_activeTheme].SetQfcTheme(async:false)` call —
  i.e., no test verifies anything the method actually does. Cycle-2's own accepted code review
  (`2026-07-02T10-47-audit/code-review.2026-07-02T10-47.md:124-128`) documented the underlying barrier
  (`Theme.SetQfcTheme(async:false)` synchronously faulting on the handle-less `BuildColorTheme` test
  double) as genuine when justifying the original exemption; cycle-3 does not resolve that barrier, it
  avoids triggering it by never executing the delegate.
- **Impact:** Overstates the cycle-3 reduction: 2 of the claimed 17 de-exemptions (and thus 2 of the 24
  residual-boundary members) are not behaviorally verified, contradicting AC8's "covered by ≥1 passing
  test" and AC10's "boundary is minimized ... no member reducible via an already-established
  seam/technique retains an exemption" claims (see `feature-audit.2026-07-02T15-26.md` AC8/AC10 rows).
- **Remediation (either):**
  1. Restructure `ToggleFocus()`/`ToggleFocus(Enums.ToggleState)` to perform the `_activeUI`/
     `_activeTheme` state-mutation and `RegisterFocusAsyncActions`/`UnregisterFocusAsyncActions` calls
     directly (outside the `Invoke` wrapper), mirroring how `ToggleFocusOnAsync`/`ToggleFocusOffAsync`
     (same file, lines 138-166) already decouple state-mutation from the Theme-render call; then add
     tests asserting the resulting `_activeUI`/`_activeTheme` field state, the same way
     `ToggleFocusOnAsync_ActivatesUiAndSwitchesToActiveTheme` (line 156) does. Verify: `_activeUI`/
     `_activeTheme` change as expected; the Theme-render call remains behind the `Invoke` wrapper (or a
     documented, still-exempt seam) so the genuine handle-less-Theme barrier is not silently violated.
  2. OR restore `[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]` on both members with the
     same per-member justification cycle-2 used ("faults on the handle-less `Theme` via
     `SetQfcTheme(async:false)`"), update `evidence/other/exemption-boundary.2026-07-02T15-05.md` and
     `spec.md` item 11 to reflect a 26-member (not 24-member) honestly-reconciled boundary, and update
     the P9-T5/T6 task disposition accordingly.
  - Whichever path is taken, re-run `grep -rnE "ExcludeFromCodeCoverage\]" QuickFiler/Controllers/QfcItemController*.cs UtilitiesCS/Threading/WpfUiDispatcher.cs QuickFiler/Viewers/WebView2CoreInitializer.cs QuickFiler/Interfaces/MailItemActionsAdapter.cs` and the full four-step toolchain, and update
    `evidence/qa-gates/final-residual-verification.<new-ts>.md` and
    `evidence/regression-testing/coverage-delta.<new-ts>.md` with the corrected count/coverage.
- **Route:** atomic_planner / atomic_executor (implementation + test task; no maintainer governance
  action required for this item).
- **Artifact paths:**
  `QuickFiler/Controllers/QfcItemController.FocusAndTheme.cs` (target);
  `QuickFiler.Test/Controllers/QfcItemController.FocusAndThemeTests.cs` (target);
  `docs/features/active/2026-06-29-qfc-item-controller-testability-227/evidence/other/exemption-boundary.2026-07-02T15-05.md` (update if re-exempting);
  `docs/features/active/2026-06-29-qfc-item-controller-testability-227/spec.md` (AC8/AC10/item-11 text, update either way);
  `docs/features/active/2026-06-29-qfc-item-controller-testability-227/2026-07-02T15-26-audit/policy-audit.2026-07-02T15-26.md` (finding, §8 item 1);
  `docs/features/active/2026-06-29-qfc-item-controller-testability-227/2026-07-02T15-26-audit/code-review.2026-07-02T15-26.md` (finding, Findings Table row 1);
  `docs/features/active/2026-06-29-qfc-item-controller-testability-227/2026-07-02T15-26-audit/feature-audit.2026-07-02T15-26.md` (finding, AC8/AC10 rows).

---

## R2 — Cycle-3 delivery is uncommitted

- **Severity: Blocking**
- **Type:** Process / merge-readiness gate (recurring from cycle 2)
- **Finding:** Committed HEAD `0a212191` carries no cycle-3 diff. All cycle-3 production, seam, test,
  csproj, and evidence files are modified/untracked in the working tree (`git status --short`).
- **Impact:** The branch cannot merge the reviewed cycle-3 work while it remains uncommitted; a PR diff
  against `main` would not include it.
- **Remediation:** Commit the full cycle-3 change set (recommend resolving R1 first so the commit
  reflects the corrected state, avoiding a second commit purely for the R1 fix), then confirm `git
  status` is clean and re-run the final toolchain against the committed head to confirm parity with the
  evidence.
- **Route:** atomic_executor (commit as part of closing out the plan) or the maintainer's standard
  commit workflow. Not a governance decision.
- **Artifact paths:** all files listed under "Code Under Test" in
  `docs/features/active/2026-06-29-qfc-item-controller-testability-227/2026-07-02T15-26-audit/policy-audit.2026-07-02T15-26.md`.

---

## R3 — Affected non-exempt denominator remains below the 80% target (deferred, non-blocking)

- **Severity: Non-blocking (carried forward, not newly introduced this cycle)**
- **Type:** Coverage uplift (tracked follow-up)
- **Finding:** The affected `QfcItemController` non-exempt denominator is 77.40% (1243/33/330 of 1606),
  improved +3.81pp from the cycle-3 baseline (73.59%) with no regression, but still below the spec's
  stated ≥80% target. This gap has persisted across all three cycles.
- **Impact:** AC5's ≥80% sub-target is not literally met; the spec's AC5 narrative treats this as
  resolved (`[x]`), which overstates the position slightly. Does not block R1/R2 remediation.
- **Remediation:** Either explicitly disposition this as an accepted, documented exception (mirroring
  the repo-wide-floor exception already granted under #223, with explicit maintainer sign-off) or
  continue coverage uplift in a future cycle.
- **Route:** Project maintainer decision on disposition framing; any further coverage uplift work routes
  to atomic_planner/atomic_executor as a future cycle.
- **Artifact paths:**
  `docs/features/active/2026-06-29-qfc-item-controller-testability-227/evidence/qa-gates/final-tests-coverage.2026-07-02T15-12.md`;
  `docs/features/active/2026-06-29-qfc-item-controller-testability-227/evidence/regression-testing/coverage-delta.2026-07-02T15-14.md`;
  `docs/features/active/2026-06-29-qfc-item-controller-testability-227/spec.md` (Coverage Target section).

---

## R4 — Exemption-boundary maintainer ratification still pending (carried forward, NOT routable to a planner)

- **Severity: Blocking on merge, but not an implementation task**
- **Type:** Governance / maintainer decision
- **Finding:** The 24-member (pending R1 resolution: 24 or 26) residual `[ExcludeFromCodeCoverage]`
  boundary has not yet been ratified by the maintainer, consistent with the cycle-2 precedent.
- **Impact:** AC8/AC10 cannot be checked off in `spec.md` until ratified (and until R1 is resolved, so
  the boundary submitted for ratification is accurate).
- **Remediation:** Resolve R1 first (so the boundary is honest), then the maintainer reviews the
  corrected boundary and records a decision (produce `maintainer-decision.<date>.md`, cycle-3 edition,
  analogous to the existing `maintainer-decision.2026-07-01.md`).
- **Route:** Project maintainer (Dan Moisan). Do NOT route to an implementation planner.
- **Artifact paths:**
  `docs/features/active/2026-06-29-qfc-item-controller-testability-227/evidence/other/exemption-boundary.2026-07-02T15-05.md`;
  target `docs/features/active/2026-06-29-qfc-item-controller-testability-227/maintainer-decision.<date>.md`.

---

## Items explicitly NOT flagged (verified clean)

- **Behavior preservation:** `Theme`'s `IUiDispatcher` retrofit preserves the exact original
  thread-marshaling behavior (verified via `git diff` of the three retrofitted call sites); the
  `FolderPredictor` factory-delegate seam mirrors an already-established pattern with production
  defaults applied on every construction path including `CreateAsync`/`CreateSequentialAsync`. No
  behavior-change risk identified beyond R1.
- **15 of the 17 claimed de-exemptions are genuinely behavior-verified:** independently spot-checked
  `WpfUiDispatcher` (real live-dispatcher execution), `MailItemActionsAdapter` (pre-existing coverage,
  clean attribute removal), `BtnFlagTask_Click` (sentinel-exception factory test), `PopulateControls`/
  `PopulateControlsAsync`, `RegisterExpandedActions`, `JumpToAsync(Control)`, the 5 `FolderPredictor`-
  cluster members, and the 3 `Theme`+`IUiDispatcher`-retrofit members (`ToggleFocusAsync`×2,
  `ApplyReadEmailFormat`) — all genuinely exercise and assert on real behavior.
- **500-line cap (AC6):** All touched/created files ≤ 500 lines except the documented pre-existing
  `FolderPredictor.cs` exception (823 lines, confirmed unchanged beyond `partial` via `git diff
  --numstat`).
- **Toolchain (AC7):** Four-step C# toolchain EXIT_CODE 0 in order at the final gate; 347/347 +
  4093/4093 tests pass.
- **Evidence location compliance:** All evidence under canonical `<FEATURE>/evidence/<kind>/`; no
  non-canonical `artifacts/{baselines,qa,evidence,coverage}/` paths in the diff.
- **Scope narrowing:** None supplied; full feature-vs-base audit performed on the working tree
  (committed `0a212191` + uncommitted cycle-3 changes).
- **Option B (leaf-control interfaces):** Not introduced; grep confirms no `IButton`/`ILabel`/
  `IComboBox`/`ITextBox`/`IList<IButton>` in the controller, `IItemViewer`, or `ItemViewer` partials.
