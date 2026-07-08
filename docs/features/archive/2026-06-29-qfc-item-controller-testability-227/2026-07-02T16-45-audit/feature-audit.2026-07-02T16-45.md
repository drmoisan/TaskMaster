# Feature Audit: QfcItemController Testability — Cycle-4 Exit Reaudit (#227)

**Audit Date:** 2026-07-02
**Feature Folder:** `docs/features/active/2026-06-29-qfc-item-controller-testability-227/`
**Base Branch:** `main` (merge-base `4611fd60b7d1a782a8024f54cbfd4d28f6d4c264`)
**Head Branch:** `TaskMaster-wt-2026-06-29-09-38` — committed HEAD `48eb71cecff5dfa50dbb884df623fbf0ce5801fd` (clean working tree, independently re-verified via `git status --short`).
**Work Mode:** `full-feature` (from `issue.md` line 11 → AC sources are `spec.md` and `user-story.md`)
**Audit Type:** Post-remediation acceptance verification (cycle 4, exit reaudit)

---

## Scope and Baseline

- **Base branch:** `main` (commit `4611fd60b7d1a782a8024f54cbfd4d28f6d4c264`)
- **Head branch/commit:** `48eb71cecff5dfa50dbb884df623fbf0ce5801fd` (committed, clean working tree)
- **Merge base:** `4611fd60b7d1a782a8024f54cbfd4d28f6d4c264`
- **Evidence sources:**
  - Primary cycle-4: `evidence/qa-gates/final-tests-coverage.2026-07-02T16-25.md`, `final-analyzers.2026-07-02T16-25.md`, `final-nullable.2026-07-02T16-25.md`, `final-csharpier.2026-07-02T16-25.md`, `final-residual-and-file-size-verification.2026-07-02T16-30.md`, `p1-toggle-focus-verification.2026-07-02T16-20.md`
  - Regression: `evidence/regression-testing/coverage-delta.2026-07-02T16-30.md`
  - Baseline (cycle-4 entry): `evidence/remediation-baseline/baseline-tests-coverage.2026-07-02T15-35.md`, `baseline-exemption-inventory.2026-07-02T15-35.md`
  - Cycle-3 exit reaudit (source of the R1 finding this cycle resolves): `2026-07-02T15-26-audit/code-review.2026-07-02T15-26.md`, `feature-audit.2026-07-02T15-26.md`, `policy-audit.2026-07-02T15-26.md`
  - Cycle-4 remediation trail: `2026-07-02T15-35-remediation/remediation-inputs.2026-07-02T15-35.md`, `remediation-plan.2026-07-02T15-35.md`
  - Direct source inspection of `QuickFiler/Controllers/QfcItemController.FocusAndTheme.cs`,
    `QuickFiler.Test/Controllers/QfcItemController.FocusAndThemeTests.cs`,
    `UtilitiesCS/HelperClasses/ThemeHelpers/Theme.cs`, `Theme.Rendering.cs` (independent of the
    delivered evidence narrative)
  - Independent command execution: `git diff --stat/--numstat 6291bdf6..48eb71ce`, `git status --short`,
    `git log 4611fd60..48eb71ce`, `dotnet tool run csharpier check` on the changed file, `grep` for the
    exemption count, direct `vstest.console.exe` runs of the 4 named tests and the full `QuickFiler.Test.dll`
    suite.
- **Feature folder used:** `docs/features/active/2026-06-29-qfc-item-controller-testability-227/` (feature root; no `vN/` selected-version subfolder)
- **Requirements source:** `spec.md` v0.4 (AC1-AC10, unchanged this cycle — confirmed `spec.md` is absent from `git diff --name-only 6291bdf6..48eb71ce`). `user-story.md` was not located in the feature folder for this feature (confirmed again this cycle: only `issue.md`, `maintainer-decision.2026-07-01.md`, `plan.2026-06-29T09-51.md`, `plan.2026-06-29T10-15.md`, `spec.md`, plus the cycle audit/remediation/evidence folders exist). Per `full-feature` mode rules, `spec.md` is treated as the sole available authoritative AC source; this is the same, unchanged assumption documented in the cycle-2 and cycle-3 feature audits.
- **Scope note:** This audit evaluates the **full branch diff against the resolved base branch** (`main` at `4611fd60`), per the scope invariant — not any cycle, plan, task, or phase subset. No caller instruction in this delegation attempted to narrow scope; the delegation prompt explicitly frames "cycle 4" as the delta under review while directing the reviewer to confirm the full commit range and working-tree state, which this audit does (Acceptance Criteria Evaluation below reflects the state of the entire branch at `48eb71ce` versus `main`, not merely the cycle-4 diff). Cycle 4 itself is a narrow, test-only delta (one `.cs` file) targeting the sole finding carried forward from the cycle-3 exit reaudit; AC1-AC7 and AC9 were already independently PASS-verified in the cycle-3 audit and are re-confirmed here rather than re-derived from scratch, per the same evidence (unchanged production code, independently re-confirmed via `git diff --numstat` showing zero production-file deltas since cycle-3's exit).

---

## Acceptance Criteria Inventory

**Authoritative AC source files for this run:**
- `spec.md` (v0.4) — sole located source for AC1-AC10 (`user-story.md` absent for this feature)

### Acceptance criteria (spec.md v0.4)

1. **AC1:** `QfcItemController` is split into partial-class files, each under 500 lines, with a logical responsibility-based structure; no behavior change; all existing tests pass.
2. **AC2:** `private ItemViewer _itemViewer` and the public constructor parameters are changed to `IItemViewer`; `Mock<IItemViewer>` is injectable into the controller.
3. **AC3:** `IItemViewer` is narrowed to intent-level members (display-state properties, command events, intent methods); raw clickable/raw control types are removed from the interface; `ItemViewer.cs` provides forwarding implementations and remains `[ExcludeFromCodeCoverage]`.
4. **AC4:** Test files mirror the new partial-class structure (one test file per testable cluster), each under 500 lines, with explicit csproj entries.
5. **AC5:** Coverage of the affected testable (non-exempt) denominator is >= 80%; new/extracted code (including the new seam types) >= 90%; changed lines do not regress. Repo-wide floor handled under the authority-scoped exception precedent (#197).
6. **AC6:** No production file modified exceeds 500 lines after the change (re-verified after the redesign, including the new seam files).
7. **AC7:** Full C# toolchain passes in order — csharpier, .NET analyzers, nullable/TreatWarningsAsErrors, MSTest with coverage — with no regressions (re-verified after the redesign).
8. **AC8:** The cycle-1 exemption set is reduced by removing `[ExcludeFromCodeCoverage]` from the members that have no genuine testability barrier and covering them with tests; no member that can be exercised through the narrowed `IItemViewer` or a mockable collaborator retains an exemption. Cycle-3 scope: the 17 members the cycle-2 residual re-audit found actionable (9 test-only, Tier 1; 8 via the new `FolderPredictor` factory-delegate and `Theme`+`IUiDispatcher` seams, Tier 2) are de-exempted and covered — 41→24.
9. **AC9:** The four behavioral seams (`IUiDispatcher`, `IWebViewCoreInitializer`, `IMailItemActions` + collaborator factory delegates, and thin-delegator `async void` handlers) are introduced per the DI-seam rule ordering, are covered to >= 90%, and preserve runtime behavior. No leaf-control interface layer is introduced. Cycle 3 extends (does not replace) this seam set.
10. **AC10:** Every residual `[ExcludeFromCodeCoverage]` is individually justified with a specific per-member technical reason (no blanket/category exemption), the boundary is minimized (no member reducible via an already-established seam/technique in this codebase retains an exemption), and the boundary is documented for maintainer ratification at review.

---

## Acceptance Criteria Evaluation

| # | Criterion | Status | Evidence | Verification command(s) | Notes |
|---|-----------|--------|----------|--------------------------|-------|
| AC1 | Partial-class split, each < 500 lines, no behavior change | PASS | Delivered cycle-1; unmodified since. `git diff --numstat 6291bdf6..48eb71ce -- QuickFiler/Controllers/QfcItemController*.cs` shows zero production diffs this cycle. | `git diff --numstat 6291bdf6..48eb71ce` | Unchanged from cycle-3 PASS; independently re-confirmed no regression this cycle. |
| AC2 | `_itemViewer`/ctor params are `IItemViewer`; `Mock<IItemViewer>` injectable | PASS | Unmodified this cycle; `Mock<IItemViewer>`/`BuildExecutingViewer()` (a `Mock<IItemViewer>` subclass pattern) continue to be injected throughout the test suite, including the 4 tests newly strengthened this cycle. | source read | Unchanged from cycle-3 PASS. |
| AC3 | `IItemViewer` narrowed to intent members; `ItemViewer` forwards; exempt | PASS | Unmodified this cycle. | source read | Unchanged from cycle-3 PASS. |
| AC4 | Test files mirror partial structure, < 500 lines, csproj-wired | PASS | `QfcItemController.FocusAndThemeTests.cs` grew from 380 to 497 lines this cycle (still <= 500, independently re-measured); no new test file was added and no csproj entry changed (`git diff --name-status` confirms zero `.csproj` deltas this cycle). | `wc -l "QuickFiler.Test/Controllers/QfcItemController.FocusAndThemeTests.cs"` → 497; `git diff --name-status 6291bdf6..48eb71ce` | The file is now within 3 lines of the 500-line cap — a risk to flag for any future cycle that touches this file, not a current violation. |
| AC5 | Affected non-exempt denom ≥ 80%; new/extracted ≥ 90%; no changed-line regression | PARTIAL | Unchanged headline figure from cycle 3 (77.40%, per-`QfcItemController`-denominator metric) — not recomputed this cycle (explicitly deferred per `remediation-inputs.2026-07-02T15-35.md`). Repo-wide/per-module deltas that were recomputed this cycle show no regression and a small improvement: whole-process 63.21%→63.28%, `QuickFiler.dll` 47.69%→48.32%, `UtilitiesCS.dll` 85.86%→85.96%. The two newly-genuinely-covered `ToggleFocus` bodies (previously instrumented-but-uncovered) plausibly raise the affected-denominator figure above 77.40%, but this was not independently recomputed by the executor or by this audit. | `evidence/regression-testing/coverage-delta.2026-07-02T16-30.md`; `evidence/qa-gates/final-tests-coverage.2026-07-02T16-25.md` | Same disposition as cycle 3: an open, non-newly-introduced gap. The AC8/AC10-specific coverage-quality defect that made cycle-3's AC5 evidence partially unreliable (the two `ToggleFocus` members) is now resolved, but the ≥80% affected-denominator target itself remains unconfirmed as crossed. |
| AC6 | No modified/created production file > 500 lines | PASS | Zero production files modified this cycle (`git diff --numstat` empty for all `.cs` files except the one test file). All file sizes from cycle 3 stand unchanged. | `git diff --numstat 6291bdf6..48eb71ce -- '*.cs'` | Unchanged from cycle-3 PASS. |
| AC7 | Full toolchain green in order, no regressions | PASS | csharpier/analyzers/nullable EXIT_CODE 0 (`evidence/qa-gates/final-{csharpier,analyzers,nullable}.2026-07-02T16-25.md`); 349/349 QuickFiler.Test + 4093/4093 UtilitiesCS.Test = 4442/4442 pass, 0 fail, 0 regression (`evidence/qa-gates/final-tests-coverage.2026-07-02T16-25.md`). Independently re-verified this cycle: `dotnet tool run csharpier check` on the sole changed file (exit 0); direct `vstest.console.exe` run of the full `QuickFiler.Test.dll` (349/349 pass, matching the recorded figure exactly). | `dotnet tool run csharpier check "QuickFiler.Test/Controllers/QfcItemController.FocusAndThemeTests.cs"`; `vstest.console.exe QuickFiler.Test.dll /InIsolation` | Independent re-execution corroborates the delivered evidence exactly; no discrepancy found. |
| AC8 | Over-broad exemptions removed + covered; no seam/`IItemViewer`-reachable member stays exempt | PASS | 24-member boundary unchanged and re-confirmed by independent grep (24 matches, exact match to cycle-3). The specific defect that made cycle-3's AC8 evaluation PARTIAL — `ToggleFocus()`/`ToggleFocus(Enums.ToggleState)` being covered only by a non-executing marshal-verification test — is resolved this cycle: independent re-execution of the 4 `ToggleFocus*` tests confirms all pass, and independent source comparison confirms the tests now genuinely execute and assert both directions of both overloads' `_activeUI`/`_activeTheme` state transitions, not merely the `Invoke` call. | `grep -rnE "ExcludeFromCodeCoverage\]" QuickFiler/Controllers/QfcItemController*.cs UtilitiesCS/Threading/WpfUiDispatcher.cs QuickFiler/Viewers/WebView2CoreInitializer.cs QuickFiler/Interfaces/MailItemActionsAdapter.cs` (24 matches); direct `vstest.console.exe` run of the 4 named `ToggleFocus*` tests (4/4 pass); source read of `FocusAndTheme.cs:27-123` vs. `FocusAndThemeTests.cs:188-269` | Upgraded from cycle-3's PARTIAL to PASS: the sole quality defect underlying the PARTIAL verdict is independently confirmed resolved. All 17 cycle-3 de-exemptions (and the 7 cycle-2 carryovers within the 24) are now genuinely behavior-verified, not merely 15 of 17. |
| AC9 | Four seams introduced per DI-seam ordering, covered ≥ 90%, behavior preserved; no leaf-control layer | PASS | Unmodified this cycle (no `IUiDispatcher`/`IWebViewCoreInitializer`/`IMailItemActions`/`IFolderSearchHandler` production file touched). `grep -rn "IButton\|ILabel\|IComboBox\|ITextBox" QuickFiler/` continues to show no matches (independently re-confirmed no new leaf-control interface was introduced this cycle, since zero production files changed). | source read; `git diff --numstat` (no production seam-type file touched) | Unchanged from cycle-3 PASS; this AC never implicated the `ToggleFocus` finding (per cycle-3's own audit note, re-confirmed here). |
| AC10 | Residual exemptions individually justified per-member; boundary documented for ratification | PASS | Same 24-member boundary as cycle 3, now genuinely justified without qualification: cycle-3's own AC10 finding was specifically that the "no member reducible via an already-established seam/technique retains an exemption" claim did not hold for the two `ToggleFocus` members (they were de-exempted but not genuinely covered, which cycle-3 characterized as an honesty gap in the *de-exemption*, not the *retained* 22). That gap is now closed: all 24 retained exemptions and all 17 cycle-3 de-exemptions (verified in AC8) are accurately characterized. The boundary itself (`evidence/other/exemption-boundary.2026-07-02T15-05.md`) was not re-authored this cycle (no production/exemption change occurred), which is correct since the boundary composition (24 members) is unchanged. | `evidence/other/exemption-boundary.2026-07-02T15-05.md` (unchanged, still accurate); independent AC8 re-verification | Upgraded from cycle-3's PARTIAL to PASS on the same basis as AC8. Maintainer ratification of the 24-member boundary remains an outstanding governance action (see Acceptance Criteria Check-off below) — a distinct gate from this audit's technical PASS determination. |

---

## Summary

**Overall Feature Readiness:** PASS (cycle-4 scope); maintainer ratification of the 24-member exemption boundary remains an outstanding governance action, not an audit blocker.

**Criteria summary:**
- **PASS:** 9 criteria (AC1, AC2, AC3, AC4, AC6, AC7, AC8, AC9, AC10)
- **PARTIAL:** 1 criterion (AC5)
- **UNVERIFIED:** 0 criteria
- **FAIL:** 0 criteria

**Top gaps preventing a full closure of every AC:**

1. **AC5 (coverage floor, unchanged/open, non-blocking for this cycle):** the affected non-exempt
   `QfcItemController` denominator (77.40% as of cycle 3) was not recomputed after cycle 4's fix, even
   though the fix plausibly raises it. This is a pre-existing, not newly-introduced, gap that has been
   carried and explicitly deferred across cycles 3 and 4 (tracked under #197 for repo-wide uplift). It
   does not block this cycle's exit, since it was not this cycle's assigned scope and the delivered
   evidence honestly discloses the non-recompute rather than overstating a number.
2. **Governance, not an AC (informational):** maintainer ratification of the reduced 24-member
   `[ExcludeFromCodeCoverage]` boundary remains outstanding. This is a distinct approval gate from AC8/AC10's
   technical-accuracy determination (which this audit finds PASS); it is not something this audit, or any
   automated cycle, can self-certify.

**Recommended follow-up verification steps:**

1. If a future cycle touches coverage instrumentation or the `QfcItemController`/`Theme` seam surface
   again, recompute the affected non-exempt denominator figure fresh rather than reusing 77.40%, since it
   is known-stale (favorably) after this cycle's fix.
2. Route the 24-member exemption boundary (`evidence/other/exemption-boundary.2026-07-02T15-05.md`) to
   the maintainer for the outstanding ratification decision referenced in AC8/AC10.
3. Consider refreshing the canonical `artifacts/csharp/coverage.xml` (cycle-1-dated) in a future cycle for
   evidence-artifact hygiene; non-blocking.

---

## Acceptance Criteria Check-off

Per the acceptance-criteria tracking rules: criteria evaluated PASS may be checked off if represented as
checkboxes and not already checked; criteria evaluated PARTIAL/FAIL/UNVERIFIED must remain unchecked.

`spec.md`'s AC1-AC4, AC6, AC7, AC9 were already marked `[x]` prior to this cycle and remain correctly
checked per this audit's PASS findings (unchanged). `spec.md`'s AC8 and AC10 are currently marked `[ ]`
(unchecked). This audit's independent re-verification finds AC8 and AC10 to be technically PASS (the
specific reduction-honesty defect that caused cycle-3's PARTIAL verdict is resolved and independently
re-confirmed). However, `spec.md`'s own AC8/AC10 text explicitly and deliberately ties the checkbox to a
separate governance event — **maintainer ratification of the 24-member exemption boundary** — which has
not occurred as of this audit. Per the check-off protocol's "Evidence before check-off" rule, this audit
does **not** unilaterally check AC8/AC10, because the specific evidence the source document itself
requires for check-off (a maintainer ratification record) does not yet exist; only the *audit-verifiable*
component of AC8/AC10 (technical accuracy of the boundary and its justifications) has been confirmed.
`spec.md`'s AC5 remains correctly unchecked per this audit's PARTIAL finding. No check-off change was made
to `spec.md` by this audit.

This is a deliberate, evidence-based deviation from a default "PASS → check the box" rule, made explicit
here rather than silently applied, consistent with the acceptance-criteria-tracking skill's requirement to
document any gap between an audit's PASS finding and an unchecked source checkbox.

### AC Status Summary

- Source: `spec.md` (v0.4)
- Total AC items: 10 (AC1-AC10)
- Checked off (delivered): 7 (AC1, AC2, AC3, AC4, AC6, AC7, AC9) — unchanged this cycle
- Remaining (unchecked): 3 (AC5, AC8, AC10)
- Items remaining:
  - AC5: Coverage of the affected testable (non-exempt) denominator is >= 80% — last recomputed at 77.40% (cycle 3); not recomputed this cycle.
  - AC8: The cycle-1 exemption set is reduced ... no member that can be exercised through the narrowed `IItemViewer` or a mockable collaborator retains an exemption — technically PASS per this audit; checkbox withheld pending the maintainer-ratification event `spec.md` itself requires.
  - AC10: Every residual `[ExcludeFromCodeCoverage]` is individually justified ... the boundary is minimized — technically PASS per this audit; checkbox withheld for the same governance reason as AC8.

| Source File | Total AC | Checked (PASS) | Unchecked | Notes |
|-------------|----------|----------------|-----------|-------|
| `spec.md` | 10 | 7 | 3 | Checkbox-backed. AC8/AC10 are audit-PASS but checkbox-withheld pending maintainer ratification (a condition `spec.md` itself imposes); AC5 remains genuinely open (denominator not recomputed, plausibly still below 80%). |

**Feature-audit blocking-finding count: 0** (AC5's sub-80%/non-recomputed reading is a pre-existing,
non-newly-introduced, explicitly-deferred open item, consistent with its cycle-3 disposition — not counted
as a cycle-4 blocking finding since it was outside this cycle's assigned remediation scope and the
evidence surrounding it is honest, not overstated).
