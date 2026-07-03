# Code Review: QfcItemController Testability — Cycle-4 Exit Reaudit (#227)

**Review Date:** 2026-07-02
**Reviewer:** feature-reviewer (Claude)
**Feature Folder:** `docs/features/active/2026-06-29-qfc-item-controller-testability-227/`
**Feature Folder Selection Rule:** Selected version is the feature root (no `vN/` subfolder present); per-cycle audit artifacts are grouped under `<exit-ts>-audit/` subfolders per the convention established in commit `0a212191`. This is the cycle-4 exit reaudit, written to `2026-07-02T16-45-audit/`.
**Base Branch:** `main` (merge-base `4611fd60b7d1a782a8024f54cbfd4d28f6d4c264`)
**Head Branch:** `TaskMaster-wt-2026-06-29-09-38` — committed HEAD `48eb71cecff5dfa50dbb884df623fbf0ce5801fd` (`git status --short` confirmed clean; independently re-verified).
**Review Type:** Post-remediation re-review (cycle 4)

---

## Executive Summary

Cycle 4 is a narrow, test-only remediation that resolves the sole material finding from the cycle-3 exit
reaudit (`2026-07-02T15-26-audit/`): `QfcItemController.ToggleFocus()` and
`ToggleFocus(Enums.ToggleState)` were de-exempted in cycle 3 but tested only for the fact that
`_itemViewer.Invoke(...)` was called, without ever executing the delegate that carries the methods'
actual logic. Cycle 4 replaces the non-executing `new Mock<IItemViewer>()` with the file's own
already-proven `BuildExecutingViewer()` helper (which runs the passed delegate via `DynamicInvoke`),
adds a 16-field handle-less-`Theme` reflection-injection helper (`EnableHandlelessThemeInvoke`) mirroring
the already-proven technique in `Theme.DispatcherTests.cs`, and asserts the real `_activeUI`/`_activeTheme`
state transitions for both directions of both overloads (4 tests: 2 modified in place, 2 new). The second
cycle-3 finding (uncommitted delivery) was already resolved before this cycle opened (commit `6291bdf6`),
and cycle 4 itself is committed (`48eb71ce`, clean working tree).

**What changed:**
- `QuickFiler.Test/Controllers/QfcItemController.FocusAndThemeTests.cs` — the only source file touched
  this cycle (independently confirmed: `git diff --numstat 6291bdf6..48eb71ce` shows exactly one `.cs`
  file changed; `QfcItemController.FocusAndTheme.cs` and every other production file are byte-identical
  to `6291bdf6`). Net +140/-11 lines; file is 497 lines (<= 500 cap, independently re-measured).
- Two pre-existing tests (`ToggleFocus_StateOverload_MarshalsThroughItemViewerInvoke`,
  `ToggleFocus_ParameterlessOverload_MarshalsThroughItemViewerInvoke`) rewired from a bare
  `new Mock<IItemViewer>()` to `BuildExecutingViewer()` and extended with `_activeUI`/`_activeTheme`
  assertions; two new tests (`..._Off_FromActive_...`, `..._FromActive_...`) added to cover the
  previously-untested opposite branch of each overload.
- Three scope-change findings surfaced and resolved mechanically, test-file-only, no production or
  csproj change (documented in `evidence/qa-gates/p1-toggle-focus-verification.2026-07-02T16-20.md` and
  independently verified below): a missing compile-time reference for two `Theme` fields (worked around
  via `Activator.CreateInstance(field.FieldType)`), `QfcItemController`'s own `_tableLayoutPanels` field
  needing direct Arrange-section population, and the `Invoke` call count being genuinely 2 (not 1) once
  the delegate actually executes (`ToggleTips`'s nested `InvokeBeginInvoke` call) — corrected from
  `Times.Once()` to `Times.Exactly(2)` in all 4 tests.

**Independent verification performed for this reaudit (not accepted from the delivered narrative at face
value):**
- `git diff --stat 6291bdf6..48eb71ce -- '*.cs' '*.csproj'` → exactly one file,
  `QuickFiler.Test/Controllers/QfcItemController.FocusAndThemeTests.cs`. `git diff --numstat` on
  `QfcItemController.FocusAndTheme.cs` returns no output (zero-line diff) — production code is unchanged.
- `dotnet tool run csharpier check "QuickFiler.Test/Controllers/QfcItemController.FocusAndThemeTests.cs"`
  → `Checked 1 files in 365ms`, exit 0.
- `wc -l`-equivalent line count on the changed test file → 497 lines.
- `grep -rnE "ExcludeFromCodeCoverage\]" QuickFiler/Controllers/QfcItemController*.cs
  UtilitiesCS/Threading/WpfUiDispatcher.cs QuickFiler/Viewers/WebView2CoreInitializer.cs
  QuickFiler/Interfaces/MailItemActionsAdapter.cs` → 24 matches, unchanged from cycle 3.
- Ran the 4 named `ToggleFocus*` tests directly against the built `QuickFiler.Test.dll`
  (`vstest.console.exe ... /Tests:ToggleFocus_StateOverload_MarshalsThroughItemViewerInvoke,
  ToggleFocus_StateOverload_Off_FromActive_DeactivatesUiAndSwitchesToNormalTheme,
  ToggleFocus_ParameterlessOverload_MarshalsThroughItemViewerInvoke,
  ToggleFocus_ParameterlessOverload_FromActive_DeactivatesUiAndSwitchesToNormalTheme /InIsolation`) →
  `Passed: 4, Total: 4`.
- Ran the full `QuickFiler.Test.dll` suite (`/InIsolation`, no filter) → `Total tests: 349, Passed: 349` —
  matches the evidence-recorded 347 baseline + 2 new, zero failures, zero regressions.
- Read `QfcItemController.FocusAndTheme.cs:27-123` (both `ToggleFocus` overloads) and confirmed the
  `ToggleFocus(Enums.ToggleState.On)`/`Off` and parameterless active/inactive branches match the tests'
  asserted `_activeUI`/`_activeTheme` outcomes exactly, including the nested `ToggleTips(async: false, ...)`
  call inside the outer `Invoke` delegate that explains the `Times.Exactly(2)` correction.
- Read `Theme.cs:414-432` (`SetQfcTheme(bool)`) and `Theme.Rendering.cs:8-103` (the private, no-arg
  `SetQfcTheme()` it falls through to when `async: false` and `InvokeRequired == false`) and confirmed
  all 16 fields reflection-injected by `EnableHandlelessThemeInvoke` are the exact fields dereferenced by
  that body, with no field omitted and no extraneous field added.

**Top risks (residual, none blocking):**
1. Two of the four `ToggleFocus` test names (`..._MarshalsThroughItemViewerInvoke`) are now stale —
   they describe only the pre-cycle-4 marshal-only behavior, while the tests now also assert real state
   transitions. Naming drift, not a correctness issue (see Findings Table).
2. The affected non-exempt `QfcItemController` denominator (77.40% as of cycle 3) was not recomputed
   this cycle even though the newly-executed `ToggleFocus` lines plausibly raise it; this was explicitly
   deferred by the remediation plan and is unchanged, not newly introduced (see Findings Table and the
   companion policy audit §8).
3. The canonical `artifacts/csharp/coverage.xml` remains cycle-1-dated (2026-06-29); cycle-3 and cycle-4
   coverage live only in evidence markdown. Unchanged, non-blocking, carried from cycle 3.

**PR readiness recommendation:** **Go** — the sole cycle-3 blocking finding (`ToggleFocus` reduction
honesty) is genuinely resolved and independently re-verified by direct test execution and source
inspection; the recurring uncommitted-delivery gate is resolved (clean working tree at `48eb71ce`); no
new Major or Blocker finding was identified in this reaudit.

---

## Findings Table

| Severity | File | Location | Finding | Recommendation | Rationale | Evidence |
|---|---|---|---|---|---|---|
| Minor | `QuickFiler.Test/Controllers/QfcItemController.FocusAndThemeTests.cs` | `189`, `231` (test names `ToggleFocus_StateOverload_MarshalsThroughItemViewerInvoke`, `ToggleFocus_ParameterlessOverload_MarshalsThroughItemViewerInvoke`) | The two modified tests' names still say "MarshalsThroughItemViewerInvoke," describing only their pre-cycle-4 scope. Since cycle 4, both tests also assert the full `_activeUI`/`_activeTheme` state transition, which the name does not convey. | Rename to something reflecting both the marshal and the state-transition assertion (e.g. `ToggleFocus_StateOverload_On_FromInactive_ActivatesUiAndSwitchesToActiveTheme`, mirroring the naming pattern already used for the two new sibling tests), or leave as-is with a one-line acknowledgment in a follow-up naming pass. | Test names are documentation; a stale name increases the chance a future reader misjudges what the test actually verifies (the exact failure mode this cycle was chartered to fix for the assertions themselves). | Source read of `FocusAndThemeTests.cs:188-207`, `230-248` vs. the new sibling names at `209`, `250`. |
| Minor (carried, non-blocking) | `docs/features/active/2026-06-29-qfc-item-controller-testability-227/spec.md` | AC5 Coverage Target section | The affected non-exempt `QfcItemController` denominator (77.40% as of cycle 3) was not recomputed after cycle 4, even though the two `ToggleFocus` overloads' ~35-40 previously-instrumented-but-uncovered lines are now covered, which plausibly raises the percentage. Cycle 4's own remediation-inputs explicitly deferred this recompute ("No action this cycle"), consistent with cycle-3's disposition. | If a future cycle revisits AC5, recompute the affected-denominator percentage post-cycle-4 rather than reusing the cycle-3 figure verbatim, since it is now stale in the favorable direction. | Keeps the coverage narrative accurate; this is not a regression, just an unrefreshed figure. | `docs/features/active/2026-06-29-qfc-item-controller-testability-227/2026-07-02T15-35-remediation/remediation-inputs.2026-07-02T15-35.md` ("Deferred" section); `evidence/regression-testing/coverage-delta.2026-07-02T16-30.md` (reports per-module deltas but not the affected-denominator figure). |
| Minor (carried, non-blocking) | `artifacts/csharp/coverage.xml` | n/a | Canonical C# coverage artifact remains cycle-1-dated (2026-06-29 12:36, independently re-confirmed via `ls -la`); cycle-3 and cycle-4 coverage live only in evidence markdown files. | Regenerate the canonical `artifacts/csharp/coverage.xml` from the latest full run in a future cycle. | Keeps the standard gate artifact current; numeric evidence already exists elsewhere so this is non-blocking, unchanged from cycle 3. | `ls -la artifacts/csharp/coverage.xml` (dated Jun 29 12:36). |
| Info | `QuickFiler.Test/Controllers/QfcItemController.FocusAndThemeTests.cs` | `99-114`, `136-178` | `BuildExecutingViewer()` (pre-existing, cycle-2) and the new `EnableHandlelessThemeInvoke`/`SetThemeField`/`SetThemeFieldViaActivator` helpers are well-documented with XML doc comments explaining exactly why each of the 16 reflection-injected fields is needed and citing the precedent (`Theme.DispatcherTests.cs:91-134`) they mirror. | None required. | Demonstrates the reduction-honesty fix follows an established, previously-verified technique rather than introducing a novel, unverified one. | Source read. |

No Blocker or Major findings.

---

## Implementation Audit

### C# implementation audit

#### What changed well

- **The fix genuinely resolves the flagged gap, not merely relocates it.** `BuildExecutingViewer()` was
  already proven elsewhere in the same file (`ToggleTips_Synchronous_DispatchesAndExecutesDelegate`,
  `ToggleNavigation_*` tests) before cycle 4; cycle 4 correctly identifies that the two flagged
  `ToggleFocus` tests simply omitted it and applies the existing helper rather than inventing a new one —
  minimizing the risk surface of the fix itself.
- **The 16-field `EnableHandlelessThemeInvoke` helper is scoped exactly to the dependency set it needs to
  satisfy.** Independent line-by-line comparison of the helper (`FocusAndThemeTests.cs:136-158`) against
  `Theme.SetQfcTheme(bool)` (`Theme.cs:414-432`) and the private `SetQfcTheme()` it falls through to
  (`Theme.Rendering.cs:8-103`) confirms every field the production code dereferences is populated, and no
  extraneous field is added.
- **Reused, not modified, the pre-existing test builders.** `BuildFocusController`, `BuildAllThemes`, and
  `BuildColorTheme` are unchanged this cycle (confirmed via diff — the only changed lines are inside the
  four `ToggleFocus*` test methods and the three new private helpers); the fix layers new Arrange-section
  calls onto the existing builders rather than mutating shared fixture state, limiting blast radius to the
  four tests under repair.
- **The `Times.Exactly(2)` correction is a genuine strengthening, not a weakening.** Independently traced:
  `ToggleFocus`'s delegate calls `ToggleTips(async: false, ...)` (`QfcItemController.FocusAndTheme.cs:44`,
  `:60`, `:100`, `:116`), which itself calls `InvokeBeginInvoke(false, ...)` → `_itemViewer.Invoke(action)`
  (`QfcItemController.FocusAndTheme.cs:255-256`) — a second, nested `Invoke` call that only fires once the
  outer delegate actually executes. The old, non-executing tests never observed this because the nested
  call never fired under a bare `Mock<IItemViewer>()`. The corrected assertion reflects real, independently
  re-traced production behavior.

#### Type safety and API notes

- No production API surface changed this cycle (test-only). The `Activator.CreateInstance(field.FieldType)`
  workaround for `_topicThread`/`_webView2` is confined to the test file and does not alter any production
  contract; both types (`BrightIdeasSoftware.FastObjectListView`, `Microsoft.Web.WebView2.WinForms.WebView2`)
  have accessible parameterless constructors, independently confirmed by the test run succeeding without a
  `MissingMethodException`.

#### Error handling and logging

- No new error-handling or logging paths were introduced this cycle; it is a test-only change.

---

## Test Quality Audit

The cycle-4 tests are well-designed and genuinely exercise the previously-unverified production behavior:
`BuildExecutingViewer()` executes the delegate synchronously via `DynamicInvoke`; `EnableHandlelessThemeInvoke`
populates every dependency the executed body touches with a handle-less double proven viable by an existing
precedent test; and each of the 4 tests asserts the resulting `_activeUI`/`_activeTheme` field state in
addition to the `Invoke` call count. All four state-transition directions required by this reaudit's scope
are covered: `ToggleFocus(On)` inactive→active, `ToggleFocus(Off)` active→inactive,
`ToggleFocus()` inactive→active, and `ToggleFocus()` active→inactive.

### Reviewed test and QA artifacts

- `QuickFiler.Test/Controllers/QfcItemController.FocusAndThemeTests.cs` — independently re-run; all 4
  `ToggleFocus*` tests pass in isolation and the full 349-test `QuickFiler.Test.dll` suite passes with
  zero failures.
- `docs/features/active/2026-06-29-qfc-item-controller-testability-227/evidence/qa-gates/p1-toggle-focus-verification.2026-07-02T16-20.md`
  — the executor's own record of the fix and the three scope-change findings; each finding independently
  re-verified against source in this reaudit (see Executive Summary).
- `docs/features/active/2026-06-29-qfc-item-controller-testability-227/evidence/regression-testing/coverage-delta.2026-07-02T16-30.md`
  — reports per-line `<range ... covered="yes" />` for both `ToggleFocus` overloads' full bodies
  (lines 29-66, 85-122) from a dedicated `QuickFiler.Test.dll`-only coverage run; consistent with this
  review's own passing direct test execution.
- `docs/features/active/2026-06-29-qfc-item-controller-testability-227/evidence/qa-gates/final-tests-coverage.2026-07-02T16-25.md`
  — combined 4442/4442 pass (4440 baseline + 2 new), 0 fail; independently spot-checked via the full
  `QuickFiler.Test.dll` run (349/349) in this reaudit.

### Quality assessment prompts

- **Determinism:** No network/clock/temp-file dependence. `Activator.CreateInstance(field.FieldType)` is
  deterministic for both `FastObjectListView` and `WebView2` (both have accessible parameterless
  constructors; verified by the test run's consistent pass, re-run independently in this reaudit with no
  flake observed).
- **Isolation:** Each of the 4 tests constructs its own controller/viewer/theme doubles from the existing
  builders; no shared mutable state across tests.
- **Speed:** The 4 named tests complete in ~424ms combined (per this reaudit's own run); the full
  `QuickFiler.Test.dll` suite (349 tests) completes in ~1.1s (targeted) / a few seconds (full suite).
- **Diagnostics:** FluentAssertions (`.Should().Be(...)`) used for state assertions; Moq `Verify(..., Times.Exactly(2))` for the call-count assertion, with inline comments explaining why 2 (not 1) is correct.

---

## Security / Correctness Checks

| Check | Status | Evidence |
|---|---|---|
| No secrets in code | ✅ PASS | Diff inspected (test file only); none present. |
| No unsafe subprocess or command construction | N/A | No process/shell construction in scope. |
| Input validation at boundaries | N/A | Test-only change; no new production input boundary. |
| Error handling remains explicit | ✅ PASS | No new error-handling paths; existing production error handling in `ToggleFocus`/`Theme.SetQfcTheme` unchanged. |
| Configuration / path handling is safe | N/A | No new config/path handling introduced. |

---

## Research Log

No external research was required for this reaudit. Evidence reviewed: `spec.md` (unchanged this cycle),
the cycle-3 exit reaudit artifacts (`2026-07-02T15-26-audit/`), the cycle-4 remediation inputs/plan
(`2026-07-02T15-35-remediation/`), the cycle-4 evidence artifacts under `evidence/qa-gates/`,
`evidence/regression-testing/`, and `evidence/remediation-baseline/`, and direct source inspection of
`QfcItemController.FocusAndTheme.cs`, `QfcItemController.FocusAndThemeTests.cs`, `Theme.cs`, and
`Theme.Rendering.cs`. This reaudit additionally independently re-executed the toolchain-relevant checks
(csharpier check on the changed file, the 4 named tests, and the full `QuickFiler.Test.dll` suite) rather
than accepting the delivered evidence narrative at face value.

---

## Verdict

Cycle 4 is a clean, narrowly-scoped, test-only fix that genuinely resolves the sole material finding from
the cycle-3 exit reaudit. Independent re-verification in this reaudit — direct execution of the 4 named
tests, a full `QuickFiler.Test.dll` run, a `git diff`-based confirmation that zero production files
changed, a csharpier check on the sole changed file, and a line-by-line comparison of the new
reflection-injection helper against the production code it unblocks — corroborates every claim in the
delivered evidence. No Blocker or Major finding was identified. The two Minor findings (stale test names
on the two modified tests; the affected-denominator figure not being recomputed) and one carried Minor
finding (stale canonical `coverage.xml`) are cosmetic/informational and do not block merge.

**Code-review blocking-finding count: 0.**
