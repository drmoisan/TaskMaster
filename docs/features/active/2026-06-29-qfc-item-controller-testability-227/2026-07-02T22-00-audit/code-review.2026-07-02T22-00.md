# Code Review: QfcItemController Testability — Cycle-5 Exit Reaudit (#227)

**Review Date:** 2026-07-02
**Reviewer:** feature-reviewer (Claude)
**Feature Folder:** `docs/features/active/2026-06-29-qfc-item-controller-testability-227/`
**Feature Folder Selection Rule:** Selected version is the feature root (no `vN/` subfolder present); per-cycle audit artifacts are grouped under `<exit-ts>-audit/` subfolders per the convention established in commit `0a212191`. This is the cycle-5 exit reaudit, written to `2026-07-02T22-00-audit/`.
**Base Branch:** `main` (merge-base `4611fd60b7d1a782a8024f54cbfd4d28f6d4c264`)
**Head Branch:** `TaskMaster-wt-2026-06-29-09-38` — committed HEAD `74a0eac699879dabdd1c4501fdb6b2a53f2ccb7b` (`git status --short` confirmed clean; independently re-verified).
**Review Type:** Post-remediation re-review (cycle 5)

---

## Executive Summary

Cycle 5 reduces the residual `[ExcludeFromCodeCoverage]` boundary from 24 to 19 members, directly answering
the maintainer's question of whether the ratified-pending 24-member boundary was genuinely untestable.
Research (`artifacts/research/2026-07-02T16-15-qfc-item-controller-headless-itemviewer-research.md`) found
two independently-safe reduction paths, both executed exactly as scoped:

- **R1/R3 (3 members):** `ResolveControlGroups(ItemViewer)` and `WireControlTreeEvents()` are de-exempted by
  constructing a real, headless `new QuickFiler.ItemViewer()` in-test — a pattern already proven safe in
  this exact repo for `ProgressPane`/`ProgressViewer` (identical constructor shape:
  `InitializeComponent(); _context = SynchronizationContext.Current; _uiScheduler =
  TaskScheduler.FromCurrentSynchronizationContext();`). `WireEvents()` follows as a free, trivial 2-line
  pass-through de-exemption once `WireControlTreeEvents` is testable.
- **R2 (2 members):** `ToggleExpansionOff`/`ToggleExpansionOn` are de-exempted via a small
  `TlpCellSnapShot`/`IContainerControlLocal` retrofit — retyping `ApplyState(Control)` →
  `ApplyState(IContainerControlLocal)`, adding `IContainerControlLocal` to `IItemViewer`'s base-interface
  list (so `Mock<IItemViewer>` satisfies it automatically via Moq's proxy generation), and to `ItemViewer`'s
  class declaration. An empirical build-time check (not an assumption) confirmed
  `CurrentAutoScaleDimensions`/`PerformAutoScale` are already public on `ContainerControl` in this build, so
  no explicit-interface forwarders were needed — matching the diff exactly.

**What changed:**
- 6 production files (`QfcItemController.{EventWiring,Navigation,ViewerSetup}.cs`, `TlpCellSnapShot.cs`,
  `IItemViewer.cs`, `ItemViewer.cs`) — 5 `[ExcludeFromCodeCoverage]` attributes removed;
  `TlpCellSnapShot.ApplyState`/`TlpCellSnapShotList.ApplyState` retyped from `Control` to
  `IContainerControlLocal`; `IItemViewer`/`ItemViewer` extended to implement `IContainerControlLocal`; the
  two `ToggleExpansionOff`/`On` concrete `(ItemViewer)` casts removed in favor of the narrowed `IItemViewer`.
- 4 test files — 3 modified (`ViewerSetupTests.cs`, `EventWiringTests.cs`, `NavigationTests.cs`), 1 new
  (`TlpCellSnapShotTests.cs`), 7 new test methods total, plus 1 `<Compile Include>` csproj wiring entry for
  the new test file.
- Results (independently re-derived, not merely quoted from the commit message): exemptions 24→19
  (grep-confirmed); 4449/4449 tests (4442 baseline + 7 new), zero regressions; whole-process coverage
  63.62%→63.75%; csharpier/analyzers/nullable all green; all 10 touched/new files ≤ 500 lines (largest,
  `ItemViewer.cs`, at 437).

**Independent verification performed for this reaudit (not accepted from the delivered narrative at face
value):**
- `git diff --numstat 808ea8f1..74a0eac6 -- '*.cs' '*.csproj'` → exactly 10 files, matching the commit's
  own description.
- `grep -rnE "ExcludeFromCodeCoverage\]" QuickFiler/Controllers/QfcItemController*.cs
  UtilitiesCS/Threading/WpfUiDispatcher.cs QuickFiler/Viewers/WebView2CoreInitializer.cs
  QuickFiler/Interfaces/MailItemActionsAdapter.cs` → 19 matches, independently re-run; the per-member
  classification of all 19 into the exemption-boundary document's 5 buckets (9 orchestration + 0
  `TlpCellSnapShot`-follow-up + 3 virtual-seam + 6 `async void` shells + 1 external-runtime dependency)
  independently traced line-by-line to source — exact match, no drift.
- Direct source read of `ResolveControlGroups_WithHeadlessItemViewer_...`,
  `WireControlTreeEvents_WithHeadlessItemViewer_...`, and `WireEvents_WithHeadlessItemViewer_...` confirms
  each genuinely constructs `new QuickFiler.ItemViewer()` (not `Mock<IItemViewer>` or a subclass), installs
  and restores `SynchronizationContext.Current` in a per-test `try/finally` with no shared/static state, and
  asserts genuine outcomes: `controller.TableLayoutPanels`/`controller.Buttons` are `NotBeNullOrEmpty()`
  (real population, not merely "did not throw"); `mockKbd.Verify(k =>
  k.KeyboardHandler_PreviewKeyDownAsync(viewer.LblAcOpen, It.IsAny<PreviewKeyDownEventArgs>()),
  Times.Once())` and the companion `KeyDownAsync` verification prove the keyboard-handler wiring is real, not
  vacuous; `btnDelItem.BackColor.Should().Be(Color.Yellow)` after invoking the real protected
  `Control.OnMouseEnter` via reflection proves the mouse-enter theme-hover wiring executed.
- Direct source read of `ToggleExpansionOff_AppliesCompressedSnapshotAndClearsExpandedFlag`,
  `ToggleExpansionOn_AppliesExpandedSnapshotAndSetsExpandedFlag`, and both `TlpCellSnapShotTests.cs` tests
  confirms genuine `Enabled`/`Visible`/`Text` restore from a deliberately-mutated live state (not a no-op
  replay) using a bare `Control`/`TableLayoutPanel`/`Label` host plus a `Mock<IItemViewer>` whose `Controls`
  getter returns the host's real `ControlCollection`.
- Read `P2-T3`'s ground-truth artifact (`evidence/other/p2-t3-containercontrol-accessibility-groundtruth.2026-07-02T17-00.md`)
  and `UtilitiesCS/Interfaces/IWinForm/IContainerControl.cs` and confirmed the empirical build check
  genuinely ran (`EXIT_CODE 0`, zero forwarder-related diagnostics) and the diff matches — `ItemViewer.cs`
  gains only the `IContainerControlLocal` class-declaration addition, no forwarder methods.
- Independent line count (`awk 'END{print NR}'`) of all 10 touched/new files confirms every reported count
  in `evidence/qa-gates/final-residual-and-file-size-verification.2026-07-02T17-00.md` exactly.
- `git status --short` at HEAD `74a0eac6` returns no output — working tree is clean.

**Top 3 risks (residual, none blocking):**
1. The `QfcItemController`-scoped affected non-exempt denominator (77.40% as of cycle 3) was not recomputed
   this cycle even though the 7 newly-covered members plausibly raise it. Carried, explicitly out of this
   cycle's assigned scope (unchanged disposition from cycles 3-4).
2. The canonical `artifacts/csharp/coverage.xml` remains cycle-1-dated (2026-06-29); coverage for cycles
   2-5 lives only in evidence markdown/committed Cobertura XML. Unchanged, non-blocking, carried.
3. Maintainer ratification of the (now 19-member) exemption boundary remains an outstanding governance
   action — a distinct approval gate from this review's technical determination, not a code-quality risk.

**PR readiness recommendation:** **Go** — the exact 5-member reduction the maintainer requested is
delivered, independently re-verified as genuine (not vacuous) behavior coverage, with zero test regressions
and a clean, committed working tree.

---

## Findings Table

| Severity | File | Location | Finding | Recommendation | Rationale | Evidence |
|---|---|---|---|---|---|---|
| Minor (carried, non-blocking) | `docs/features/active/2026-06-29-qfc-item-controller-testability-227/spec.md` | AC5 Coverage Target section | The affected non-exempt `QfcItemController` denominator (77.40% as of cycle 3) was not recomputed after cycle 5, even though the 7 newly-covered members (`ResolveControlGroups`, `WireControlTreeEvents`, `WireEvents`, `ToggleExpansionOff`/`On`, both `TlpCellSnapShot`/`TlpCellSnapShotList.ApplyState` overloads) plausibly raise it. Consistent with cycles 3-4's disposition; cycle 5's own remediation-inputs scope this cycle to the exemption-count reduction only, not a denominator recompute. | If a future cycle revisits AC5, recompute the affected-denominator percentage post-cycle-5 rather than reusing the cycle-3 figure verbatim. | Keeps the coverage narrative accurate; this is not a regression, just an unrefreshed figure. | `docs/features/active/2026-06-29-qfc-item-controller-testability-227/2026-07-02T17-00-remediation/remediation-inputs.2026-07-02T17-00.md` (scope explicitly limited to R1-R3); `evidence/regression-testing/coverage-delta.2026-07-02T17-00.md` (reports whole-process/per-member deltas but not the affected-denominator figure). |
| Minor (carried, non-blocking) | `artifacts/csharp/coverage.xml` | n/a | Canonical C# coverage artifact remains cycle-1-dated (2026-06-29 12:36, independently re-confirmed via `ls -la`); cycles 3-5 coverage lives only in evidence markdown and committed Cobertura XML under `evidence/qa-gates/`/`evidence/remediation-baseline/`. | Regenerate the canonical `artifacts/csharp/coverage.xml` from the latest full run in a future cycle. | Keeps the standard gate artifact current; numeric evidence already exists elsewhere so this is non-blocking, unchanged from cycles 3-4. | `ls -la artifacts/csharp/coverage.xml` (dated Jun 29 12:36). |
| Minor (carried, non-blocking, unrelated to this cycle) | `QuickFiler.Test/Controllers/QfcItemController.FocusAndThemeTests.cs` | `189`, `231` | Two `ToggleFocus*` test names remain stale (describe only "MarshalsThroughItemViewerInvoke," omitting the state-transition assertions cycle 4 added). Carried unchanged from cycle 4; this file was not touched in cycle 5. | No action needed this cycle; rename in a future pass touching this file. | Documented as carried, not newly introduced, to avoid double-counting a cycle-4 finding as a cycle-5 regression. | `git diff --name-only 808ea8f1..74a0eac6` (file absent from this cycle's change set); source read confirms the names are unchanged. |
| Info | `QuickFiler/Controllers/QfcItemController.EventWiring.cs`, `Navigation.cs`, `ViewerSetup.cs` | De-exemption comment sites | Each removed `[ExcludeFromCodeCoverage]` attribute is replaced with an inline "De-exempted cycle-5 (R1/R2/R3): ..." comment citing the covering test file, consistent with the pattern established in prior cycles' de-exemption sites. | None required. | Keeps the in-source rationale trail intact for future readers and auditors, matching the established convention. | `git show 74a0eac6 -- QuickFiler/Controllers/QfcItemController.{EventWiring,Navigation,ViewerSetup}.cs`. |
| Info | `QuickFiler/Viewers/ItemViewer.cs`, `IItemViewer.cs` | `IContainerControlLocal` addition | The `IContainerControlLocal` retrofit is minimal and behavior-preserving: it widens `TlpCellSnapShot.ApplyState`'s accepted parameter type only, adds no explicit-interface forwarders (empirically confirmed unnecessary via a live compiler run, `p2-t3-containercontrol-accessibility-groundtruth.2026-07-02T17-00.md`), and does not touch `ItemViewer.Designer.cs`. | None required. | Demonstrates the retrofit followed the plan's own "verify before assuming" instruction rather than defaulting to the more invasive forwarder-based branch. | `git show 74a0eac6 -- QuickFiler/Viewers/ItemViewer.cs QuickFiler/Viewers/IItemViewer.cs`; `evidence/other/p2-t3-containercontrol-accessibility-groundtruth.2026-07-02T17-00.md`. |

No Blocker or Major findings.

---

## Implementation Audit

### C# implementation audit

#### What changed well

- **Both reduction lines reuse already-proven, in-repo patterns rather than inventing new test
  infrastructure.** The headless `ItemViewer` construction technique (`SynchronizationContext` install via
  `try/finally`) is a direct, line-for-line application of the `ProgressPane_Tests.cs`/`ProgressViewer_Tests.cs`
  pattern already merged in this repo — independently confirmed by comparing the new test bodies against
  that precedent's structure. The `IContainerControlLocal` retrofit reuses a pre-existing (but
  zero-implementer) interface rather than defining a new one.
- **The empirical-check discipline is genuine, not decorative.** The plan called for an empirical
  build-time check of whether `CurrentAutoScaleDimensions`/`PerformAutoScale` needed explicit-interface
  forwarders, rather than assuming the answer from the research artifact's own uncertainty on this point.
  `p2-t3-containercontrol-accessibility-groundtruth.2026-07-02T17-00.md` records a genuine `EXIT_CODE 0`
  build run with no forwarder methods present, and the delivered diff exactly matches that outcome (no
  forwarder code added) — independently confirmed this is not merely an assumption dressed as a check.
- **Test assertions genuinely exercise behavior, not just absence-of-exception.** The `WireControlTreeEvents`
  test's `Mock<IQfcKeyboardHandler>.Verify(..., Times.Once())` calls plus the real `btnDelItem.BackColor`
  assertion (after invoking the real, reflection-obtained `Control.OnMouseEnter`) prove the wiring is real;
  the `ToggleExpansionOff`/`On` tests prove genuine `Enabled`/`Visible`/`Text` restore from a deliberately
  mutated live state, not a no-op replay — independently traced against `TlpCellSnapShot.ApplyState`'s
  actual `Find`/style-copy logic.
- **Blast radius is tightly scoped.** `IContainerControlLocal`'s addition to `IItemViewer`'s base-interface
  list is a pure widening (adds one base interface `Mock<IItemViewer>` satisfies automatically via Moq); the
  two `ToggleExpansionOff`/`On` cast removals (`(ItemViewer)_itemViewer` → `_itemViewer`) are the only
  call-site change, and both are one-line, behavior-preserving edits (independently confirmed via diff — no
  other call site of `ApplyState` exists).

#### Type safety and API notes

- `IItemViewer : IUserControl, IContainerControlLocal` is an explicit, additive interface extension — no
  existing member signature changed, no breaking change to any consumer beyond the two `TlpCellSnapShot`
  overloads (which are internal to `QuickFiler` and have exactly the one call site each, both updated in
  this same commit).
- `TlpCellSnapShot.ApplyState(IContainerControlLocal root)` / `TlpCellSnapShotList.ApplyState(IContainerControlLocal
  root)` retypes are pure widenings of the accepted parameter's static type (from `Control` to an interface
  `Control`-derived types already implicitly satisfy through `ItemViewer`); no nullable-flow change,
  independently confirmed by the green nullable/TWAE build.

#### Error handling and logging

- No new error-handling or logging paths were introduced this cycle. `SetThemeField`-style guard patterns
  are not applicable here (no reflection-injected doubles beyond the `Mock<IQfcKeyboardHandler>` already
  used elsewhere in the same test file).

---

## Test Quality Audit

The cycle-5 tests are well-designed and genuinely exercise the previously-uninstrumented production
behavior. Each of the 3 R1/R3 tests constructs a real, headless `ItemViewer` with a correctly-scoped
`SynchronizationContext` install/restore (no cross-test leakage — each test installs its own context inside
its own `try/finally`, independently confirmed by source read of all three test bodies; no shared static
context field exists). The 2 R2 controller-level tests plus the 2 dedicated `TlpCellSnapShotTests.cs` tests
prove genuine state restoration (mutate-then-restore, not a trivial pass-through) using a bare `Control` host
— consistent with the research's own finding that this seam is independent of the headless-`ItemViewer`
line of work.

### Reviewed test and QA artifacts

- `QuickFiler.Test/Controllers/QfcItemController.EventWiringTests.cs`,
  `QfcItemController.ViewerSetupTests.cs`, `QfcItemController.NavigationTests.cs`,
  `QuickFiler.Test/Helper Classes/TlpCellSnapShotTests.cs` — independently read in full; all 7 new test
  methods construct genuine test doubles/real objects and assert genuine outcomes (population,
  `Mock<IQfcKeyboardHandler>.Verify`, real control-state restore), not merely absence-of-exception.
- `docs/features/active/2026-06-29-qfc-item-controller-testability-227/evidence/qa-gates/p1-r1r3-verification.2026-07-02T17-00.md`,
  `p2-r2-verification.2026-07-02T17-00.md` — the executor's own per-phase test-run records (3/3 and 4/4
  respectively); independently cross-checked against the final combined 4449/4449 run.
- `docs/features/active/2026-06-29-qfc-item-controller-testability-227/evidence/regression-testing/coverage-delta.2026-07-02T17-00.md`
  — reports per-member `line-rate` for all 7 de-exempted members from the post-change Cobertura report;
  independently cross-checked against `evidence/qa-gates/final-coverage.2026-07-02T17-00.cobertura.xml`'s
  `<method>` entries under their correct `<class>` scoping (the artifact explicitly notes it disambiguated
  from an unrelated same-named `Tags.TagController.WireEvents` method — a genuine, non-trivial verification
  step, independently spot-checked).
- `docs/features/active/2026-06-29-qfc-item-controller-testability-227/evidence/qa-gates/final-tests-coverage.2026-07-02T17-00.md`
  — 4449/4449 pass, 0 fail; consistent with the phase-level 3/3 and 4/4 sub-runs.

### Quality assessment prompts

- **Determinism:** No network/clock/temp-file dependence. `SynchronizationContext.SetSynchronizationContext`
  install/restore is deterministic and scoped per-test via `try/finally`; the `Mock<IItemViewer>`-backed
  tests use in-memory `Control`/`ControlCollection` objects with no window-handle dependency.
- **Isolation:** Each of the 7 tests constructs its own `viewer`/`controller`/`host` from scratch; no shared
  mutable state across tests (independently confirmed by source read — no static field is written by any
  new test).
- **Speed:** All 4449 tests complete in 28.4147s total per `final-tests-coverage.2026-07-02T17-00.md`; no
  sleeps/retries/polling observed in any of the 7 new tests.
- **Diagnostics:** FluentAssertions (`.Should().NotBeNullOrEmpty()`, `.Should().Be(...)`, `.Should().BeTrue()/BeFalse()`)
  used throughout; Moq `Verify(..., Times.Once())` for the wiring assertions, with inline comments explaining
  the expected outcome in each case.

---

## Security / Correctness Checks

| Check | Status | Evidence |
|---|---|---|
| No secrets in code | ✅ PASS | Diff inspected (production + test files); none present. |
| No unsafe subprocess or command construction | N/A | No process/shell construction in scope. |
| Input validation at boundaries | N/A | No new externally-facing input boundary; `ApplyState`'s widened parameter type is an internal seam, not a public API boundary receiving untrusted input. |
| Error handling remains explicit | ✅ PASS | No new error-handling paths; existing production logic (`ApplyState`'s `Find`/style-copy, `ResolveControlGroups`'s control-tree classification) unchanged in behavior, only in accepted parameter/field type. |
| Configuration / path handling is safe | N/A | No new config/path handling introduced. |

---

## Research Log

No new external research was required for this reaudit beyond independently reading the cycle-5 research
artifact (`artifacts/research/2026-07-02T16-15-qfc-item-controller-headless-itemviewer-research.md`) and
cross-checking its constructor-barrier and `IContainerControlLocal`-scope analysis against the delivered
diff. This reaudit additionally independently read: the cycle-5 remediation inputs/plan
(`2026-07-02T17-00-remediation/`), all cycle-5 evidence artifacts under `evidence/{qa-gates,remediation-baseline,regression-testing,other}/`,
the `P2-T3` ground-truth artifact, `UtilitiesCS/Interfaces/IWinForm/IContainerControl.cs`, and direct source
of all 6 changed production files and all 4 changed/new test files, rather than accepting the delivered
evidence narrative at face value.

---

## Verdict

Cycle 5 is a focused, well-scoped remediation that delivers exactly the maintainer-requested 24→19 exemption
reduction via two independently-verified, no-open-risk techniques. Independent re-verification in this
reaudit — direct source read of all 6 production and 4 test files, an exemption-count re-grep with full
per-member bucket tracing, an independent file-size re-measurement, and a direct read of the empirical
`ContainerControl`-accessibility ground-truth artifact — corroborates every claim in the delivered evidence.
No Blocker or Major finding was identified. The three carried Minor findings (affected-denominator recompute;
stale canonical `coverage.xml`; the unrelated stale `ToggleFocus*` test names) are cosmetic/informational and
do not block merge.

**Code-review blocking-finding count: 0.**
