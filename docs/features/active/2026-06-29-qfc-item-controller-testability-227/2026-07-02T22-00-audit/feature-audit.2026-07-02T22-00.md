# Feature Audit: QfcItemController Testability — Cycle-5 Exit Reaudit (#227)

**Audit Date:** 2026-07-02
**Feature Folder:** `docs/features/active/2026-06-29-qfc-item-controller-testability-227/`
**Base Branch:** `main` (merge-base `4611fd60b7d1a782a8024f54cbfd4d28f6d4c264`)
**Head Branch:** `TaskMaster-wt-2026-06-29-09-38` — committed HEAD `74a0eac699879dabdd1c4501fdb6b2a53f2ccb7b` (clean working tree, independently re-verified via `git status --short`).
**Work Mode:** `full-feature` (from `issue.md` line 11 → AC sources are `spec.md` and `user-story.md`)
**Audit Type:** Post-remediation acceptance verification (cycle 5, exit reaudit)

---

## Scope and Baseline

- **Base branch:** `main` (commit `4611fd60b7d1a782a8024f54cbfd4d28f6d4c264`)
- **Head branch/commit:** `74a0eac699879dabdd1c4501fdb6b2a53f2ccb7b` (committed, clean working tree)
- **Merge base:** `4611fd60b7d1a782a8024f54cbfd4d28f6d4c264`
- **Evidence sources:**
  - Primary cycle-5: `evidence/qa-gates/final-tests-coverage.2026-07-02T17-00.md`, `final-analyzers.2026-07-02T17-00.md`, `final-nullable.2026-07-02T17-00.md`, `final-csharpier.2026-07-02T17-00.md`, `final-residual-and-file-size-verification.2026-07-02T17-00.md`, `p1-r1r3-verification.2026-07-02T17-00.md`, `p2-r2-verification.2026-07-02T17-00.md`, `p2-t4-itemviewer-build-clean.2026-07-02T17-00.md`
  - Regression: `evidence/regression-testing/coverage-delta.2026-07-02T17-00.md`
  - Baseline (cycle-5 entry): `evidence/remediation-baseline/baseline-tests-coverage.2026-07-02T17-00.md`, `baseline-exemption-inventory.2026-07-02T17-00.md`
  - Boundary: `evidence/other/exemption-boundary.2026-07-02T17-00.md`, `evidence/other/p2-t3-containercontrol-accessibility-groundtruth.2026-07-02T17-00.md`
  - Research: `artifacts/research/2026-07-02T16-15-qfc-item-controller-headless-itemviewer-research.md`
  - Cycle-4 exit reaudit (prior state): `2026-07-02T16-45-audit/code-review.2026-07-02T16-45.md`, `feature-audit.2026-07-02T16-45.md`, `policy-audit.2026-07-02T16-45.md`
  - Cycle-5 remediation trail: `2026-07-02T17-00-remediation/remediation-inputs.2026-07-02T17-00.md`, `remediation-plan.2026-07-02T17-00.md`
  - Direct source inspection of `QuickFiler/Controllers/QfcItemController.{ViewerSetup,EventWiring,Navigation}.cs`,
    `QuickFiler/Helper Classes/TlpCellSnapShot.cs`, `QuickFiler/Viewers/{IItemViewer,ItemViewer}.cs`,
    `QuickFiler.Test/Controllers/QfcItemController.{ViewerSetupTests,EventWiringTests,NavigationTests}.cs`,
    `QuickFiler.Test/Helper Classes/TlpCellSnapShotTests.cs`,
    `UtilitiesCS/Interfaces/IWinForm/IContainerControl.cs` (independent of the delivered evidence narrative)
  - Independent command execution: `git log --oneline 4611fd60..74a0eac6`, `git diff --numstat 808ea8f1..74a0eac6 -- '*.cs' '*.csproj'`, `git status --short`, `grep -rnE "ExcludeFromCodeCoverage\]" ...` (19 matches), per-file `awk 'END{print NR}'` line counts.
- **Feature folder used:** `docs/features/active/2026-06-29-qfc-item-controller-testability-227/` (feature root; no `vN/` selected-version subfolder)
- **Requirements source:** `spec.md` v0.5 (AC1-AC10). `user-story.md` was not located in the feature folder (confirmed again this cycle: only `issue.md`, `maintainer-decision.2026-07-01.md`, `plan.2026-06-29T09-51.md`, `plan.2026-06-29T10-15.md`, `spec.md`, plus the cycle audit/remediation/evidence folders exist). Per `full-feature` mode rules, `spec.md` is treated as the sole available authoritative AC source; this is the same, unchanged assumption documented in the cycle-2, cycle-3, and cycle-4 feature audits.
- **Work mode resolution note:** `issue.md` line 11 explicitly states `Work Mode: full-feature`, independently re-confirmed by direct read of the file this cycle.
- **Scope note:** This audit evaluates the **full branch diff against the resolved base branch** (`main` at `4611fd60`), per the scope invariant — not any cycle, plan, task, or phase subset. No caller instruction in this delegation attempted to narrow scope; the delegation prompt frames "cycle 5" as the delta under review while explicitly requiring independent verification of scope discipline against the entire feature (e.g., confirming the other 19 residuals were untouched, confirming the working tree is fully committed). AC1-AC4, AC6, AC7, AC9 were already independently PASS-verified in the cycle-4 audit and are re-confirmed here (unchanged production code for those clusters, independently re-confirmed via `git diff --numstat 808ea8f1..74a0eac6` scoped to the relevant files) rather than re-derived from scratch; AC5, AC8, AC10 are re-evaluated fresh against the cycle-5 delta.

---

## Acceptance Criteria Inventory

**Authoritative AC source files for this run:**
- `spec.md` (v0.5) — sole located source for AC1-AC10 (`user-story.md` absent for this feature)

### Acceptance criteria (spec.md v0.5)

1. **AC1:** `QfcItemController` is split into partial-class files, each under 500 lines, with a logical responsibility-based structure; no behavior change; all existing tests pass.
2. **AC2:** `private ItemViewer _itemViewer` and the public constructor parameters are changed to `IItemViewer`; `Mock<IItemViewer>` is injectable into the controller.
3. **AC3:** `IItemViewer` is narrowed to intent-level members (display-state properties, command events, intent methods); raw clickable/raw control types are removed from the interface; `ItemViewer.cs` provides forwarding implementations and remains `[ExcludeFromCodeCoverage]`.
4. **AC4:** Test files mirror the new partial-class structure (one test file per testable cluster), each under 500 lines, with explicit csproj entries.
5. **AC5:** Coverage of the affected testable (non-exempt) denominator is >= 80%; new/extracted code (including the new seam types) >= 90%; changed lines do not regress. Repo-wide floor handled under the authority-scoped exception precedent (#197).
6. **AC6:** No production file modified exceeds 500 lines after the change (re-verified after the redesign, including the new seam files).
7. **AC7:** Full C# toolchain passes in order — csharpier, .NET analyzers, nullable/TreatWarningsAsErrors, MSTest with coverage — with no regressions (re-verified after the redesign).
8. **AC8:** The cycle-1 exemption set is reduced by removing `[ExcludeFromCodeCoverage]` from the members that have no genuine testability barrier and covering them with tests; no member that can be exercised through the narrowed `IItemViewer` or a mockable collaborator retains an exemption. Cycle history: 103→41 (cycle 2), 41→24 (cycle 3), unchanged (cycle 4, test-honesty fix only), **24→19 (cycle 5)**.
9. **AC9:** The four behavioral seams (`IUiDispatcher`, `IWebViewCoreInitializer`, `IMailItemActions` + collaborator factory delegates, and thin-delegator `async void` handlers) are introduced per the DI-seam rule ordering, are covered to >= 90%, and preserve runtime behavior. No leaf-control interface layer is introduced. Unchanged this cycle.
10. **AC10:** Every residual `[ExcludeFromCodeCoverage]` is individually justified with a specific per-member technical reason (no blanket/category exemption), the boundary is minimized (no member reducible via an already-established seam/technique in this codebase retains an exemption), and the boundary is documented for maintainer ratification at review.

---

## Acceptance Criteria Evaluation

| # | Criterion | Status | Evidence | Verification command(s) | Notes |
|---|-----------|--------|----------|--------------------------|-------|
| AC1 | Partial-class split, each < 500 lines, no behavior change | PASS | Delivered cycle-1; unmodified in structure this cycle (the 3 partial files touched — `ViewerSetup.cs`, `EventWiring.cs`, `Navigation.cs` — receive only attribute-removal/cast-removal edits, no structural change; independently re-confirmed at 282/389/228 lines respectively). | `git diff --numstat 808ea8f1..74a0eac6 -- QuickFiler/Controllers/QfcItemController*.cs`; `awk 'END{print NR}'` on each file | Unchanged from cycle-4 PASS; independently re-confirmed no structural regression this cycle. |
| AC2 | `_itemViewer`/ctor params are `IItemViewer`; `Mock<IItemViewer>` injectable | PASS | Unmodified this cycle. `Mock<IItemViewer>` continues to be injected in the R2 `ToggleExpansionOff`/`On` tests and the new `TlpCellSnapShotTests.cs`, confirming the seam remains functional and is exercised by new tests this cycle, not merely inherited unchanged. | source read of `NavigationTests.cs`, `TlpCellSnapShotTests.cs` | Unchanged from cycle-4 PASS; positively re-exercised by 4 new tests this cycle. |
| AC3 | `IItemViewer` narrowed to intent members; `ItemViewer` forwards; exempt | PASS | `IItemViewer` gains `IContainerControlLocal` as an additional base interface this cycle (an addition, not a re-widening of raw control exposure — `IContainerControlLocal` exposes `Controls`/`ActiveControl`/scaling members already conceptually part of `IUserControl`'s container-control surface, not raw clickable/data controls). `ItemViewer.cs` remains `[ExcludeFromCodeCoverage]` (independently re-confirmed: the class-level attribute at `ItemViewer.cs:16` is unchanged in this cycle's diff — only the base-interface list and `using` changed). | `git show 74a0eac6 -- QuickFiler/Viewers/IItemViewer.cs QuickFiler/Viewers/ItemViewer.cs` | Unchanged disposition from cycle-4 PASS; the one interface-surface change this cycle (`IContainerControlLocal`) is additive and does not reintroduce raw clickable-control exposure. |
| AC4 | Test files mirror partial structure, < 500 lines, csproj-wired | PASS | `TlpCellSnapShotTests.cs` (NEW, 122 lines) mirrors its production file (`TlpCellSnapShot.cs`) and is wired via a new `<Compile Include>` entry in `QuickFiler.Test.csproj` (independently confirmed via `git show 74a0eac6 -- QuickFiler.Test/QuickFiler.Test.csproj`). All 3 extended test files (`ViewerSetupTests.cs` 407, `EventWiringTests.cs` 374, `NavigationTests.cs` 391) remain ≤ 500 lines. | `git show 74a0eac6 -- QuickFiler.Test/QuickFiler.Test.csproj`; `awk 'END{print NR}'` on all 4 test files | Improves on cycle-4's flagged risk (a different file, `FocusAndThemeTests.cs`, was at 497/500 — untouched this cycle, still a risk for any future cycle touching it, not a cycle-5 regression). |
| AC5 | Affected non-exempt denom ≥ 80%; new/extracted ≥ 90%; no changed-line regression | PARTIAL | Unchanged headline figure from cycle 3 (77.40%, per-`QfcItemController`-denominator metric) — not recomputed this cycle (explicitly out of scope per `remediation-inputs.2026-07-02T17-00.md`, which scopes cycle 5 to the exemption-count reduction only). Whole-process/repo-wide metrics recomputed this cycle show no regression and an improvement: 63.62%→63.75% (+0.13pp). All 7 newly-covered members (the 5 de-exempted plus the incidentally-covered `TlpCellSnapShotList.ApplyState`) independently confirmed to have non-zero `line-rate` (0.5556-1.0) in the post-change Cobertura report — a genuine, not merely nominal, coverage gain that plausibly raises the affected-denominator figure above 77.40%, but this was not independently recomputed by the executor or by this audit. | `evidence/regression-testing/coverage-delta.2026-07-02T17-00.md`; `evidence/qa-gates/final-tests-coverage.2026-07-02T17-00.md` | Same disposition as cycles 3-4: an open, non-newly-introduced gap, narrower in scope now (5 fewer exempt members feed into the eventual recompute). |
| AC6 | No modified/created production file > 500 lines | PASS | All 6 modified production files independently re-measured: `ViewerSetup.cs` 282, `EventWiring.cs` 389, `Navigation.cs` 228, `TlpCellSnapShot.cs` 213, `IItemViewer.cs` 120, `ItemViewer.cs` 437 — all ≤ 500. | `awk 'END{print NR}'` on each of the 6 files | Unchanged from cycle-4 PASS; independently re-confirmed for this cycle's specific file set. |
| AC7 | Full toolchain green in order, no regressions | PASS | csharpier/analyzers/nullable EXIT_CODE 0 (`evidence/qa-gates/final-{csharpier,analyzers,nullable}.2026-07-02T17-00.md`); 4449/4449 pass (4442 baseline + 7 new), 0 fail, 0 regression (`evidence/qa-gates/final-tests-coverage.2026-07-02T17-00.md`). Independently spot-checked: `grep` re-count of the exemption boundary (19, matches exactly); per-file line counts (all ≤ 500, matches exactly); `git status --short` clean at HEAD. | `grep -rnE "ExcludeFromCodeCoverage\]" ...`; `awk 'END{print NR}'`; `git status --short` | Independent re-execution corroborates the delivered evidence exactly; no discrepancy found. |
| AC8 | Over-broad exemptions removed + covered; no seam/`IItemViewer`-reachable member stays exempt | PASS | Boundary reduced 24→19 this cycle, independently re-confirmed by grep (19 matches) and by tracing all 19 residuals to their correct source location and bucket (9 orchestration/message-pump-blocked + 0 `TlpCellSnapShot`-follow-up (fully resolved) + 3 deliberate virtual test seams + 6 `async void` shells + 1 external-runtime dependency = 19, exact match, no drift). The 5 newly-removed exemptions (`ResolveControlGroups(ItemViewer)`, `WireControlTreeEvents()`, `WireEvents()`, `ToggleExpansionOff`, `ToggleExpansionOn`) are each independently confirmed covered by ≥1 genuinely-behavior-verifying test (not merely construction/no-throw): `ResolveControlGroups`/`WireControlTreeEvents` assert real control-collection population and `Mock<IQfcKeyboardHandler>.Verify(..., Times.Once())`-backed wiring plus a real `BackColor` mutation; `ToggleExpansionOff`/`On` assert genuine `Enabled`/`Visible` restore plus the `_expanded` flag transition. | `grep -rnE "ExcludeFromCodeCoverage\]" QuickFiler/Controllers/QfcItemController*.cs UtilitiesCS/Threading/WpfUiDispatcher.cs QuickFiler/Viewers/WebView2CoreInitializer.cs QuickFiler/Interfaces/MailItemActionsAdapter.cs` (19 matches); source read of all 4 touched/new test files vs. the 5 de-exempted production methods | Upgraded scope from cycle-3/4's 24-member boundary to 19; all 5 new de-exemptions independently confirmed genuinely behavior-verified (no repeat of the cycle-3 `ToggleFocus` reduction-honesty defect cycle 4 had to fix). |
| AC9 | Four seams introduced per DI-seam ordering, covered ≥ 90%, behavior preserved; no leaf-control layer | PASS | Unmodified this cycle (no `IUiDispatcher`/`IWebViewCoreInitializer`/`IMailItemActions`/`IFolderSearchHandler` production file touched — independently confirmed via `git diff --name-status 808ea8f1..74a0eac6`). `grep -rn "IButton\|ILabel\|IComboBox\|ITextBox" QuickFiler/` continues to show no matches (independently re-confirmed no new leaf-control interface was introduced this cycle; `IContainerControlLocal` is a container-level, not leaf-control, interface and was already scoped as out-of-Option-B by the research). | source read; `git diff --name-status 808ea8f1..74a0eac6` (no seam-type production file touched) | Unchanged from cycle-4 PASS; this AC never implicated the cycle-5 scope. |
| AC10 | Residual exemptions individually justified per-member; boundary documented for ratification | PASS | The reduced 19-member boundary (`evidence/other/exemption-boundary.2026-07-02T17-00.md`) is individually justified by category and per-member; this audit independently re-traced all 19 grep hits to their documented bucket with zero mismatches (see AC8 evidence). The boundary composition change (24→19) is accurately reflected in both `spec.md` v0.5 and the boundary document; no residual is exempted "merely because it currently carries the attribute" — each of the 9 orchestration-bucket members' barrier (the unbuilt WinForms message-pump seam for `UiSyncContext`-await paths, or `InitializeWebViewAsync`'s raw-WebView2-accessor barrier) is independently distinct from the R1/R2 techniques just applied, confirmed via direct source read of `Initialization.cs` and `ViewerSetup.cs`'s remaining exempt members. | `evidence/other/exemption-boundary.2026-07-02T17-00.md`; independent AC8 re-verification; source read of `QfcItemController.Initialization.cs` | Upgraded scope from the 24-member boundary to 19, same disposition (technically PASS, checkbox withheld pending maintainer ratification — see Check-off section). |

---

## Summary

**Overall Feature Readiness:** PASS (cycle-5 scope); maintainer ratification of the (now 19-member) exemption boundary remains an outstanding governance action, not an audit blocker.

**Criteria summary:**
- **PASS:** 9 criteria (AC1, AC2, AC3, AC4, AC6, AC7, AC8, AC9, AC10)
- **PARTIAL:** 1 criterion (AC5)
- **UNVERIFIED:** 0 criteria
- **FAIL:** 0 criteria

**Top gaps preventing a full closure of every AC:**

1. **AC5 (coverage floor, unchanged/open, non-blocking for this cycle):** the affected non-exempt
   `QfcItemController` denominator (77.40% as of cycle 3) was not recomputed after cycle 5's fix, even
   though the fix plausibly raises it. This is a pre-existing, not newly-introduced, gap that has been
   carried and explicitly deferred across cycles 3, 4, and 5 (tracked under #197 for repo-wide uplift). It
   does not block this cycle's exit, since it was not this cycle's assigned scope and the delivered
   evidence honestly discloses the non-recompute rather than overstating a number.
2. **Governance, not an AC (informational):** maintainer ratification of the reduced 19-member
   `[ExcludeFromCodeCoverage]` boundary remains outstanding. This is a distinct approval gate from AC8/AC10's
   technical-accuracy determination (which this audit finds PASS); it is not something this audit, or any
   automated cycle, can self-certify.

**Recommended follow-up verification steps:**

1. If a future cycle touches coverage instrumentation or the `QfcItemController`/`TlpCellSnapShot` seam
   surface again, recompute the affected non-exempt denominator figure fresh rather than reusing 77.40%,
   since it is known-stale (favorably) after this cycle's fix.
2. Route the 19-member exemption boundary (`evidence/other/exemption-boundary.2026-07-02T17-00.md`) to
   the maintainer for the outstanding ratification decision referenced in AC8/AC10.
3. Consider refreshing the canonical `artifacts/csharp/coverage.xml` (cycle-1-dated) in a future cycle for
   evidence-artifact hygiene; non-blocking.
4. The 9-member orchestration bucket (blocked by an unbuilt WinForms `Application.Run()`-on-background-thread
   message-pump test seam) is explicitly tracked as a distinct, larger follow-up per
   `remediation-inputs.2026-07-02T17-00.md` — not assumed as automatically in scope for any future cycle of
   this feature without its own dedicated research/plan.

---

## Acceptance Criteria Check-off

Per the acceptance-criteria tracking rules: criteria evaluated PASS may be checked off if represented as
checkboxes and not already checked; criteria evaluated PARTIAL/FAIL/UNVERIFIED must remain unchecked.

`spec.md`'s AC1-AC4, AC6, AC7, AC9 were already marked `[x]` prior to this cycle and remain correctly
checked per this audit's PASS findings (unchanged). `spec.md`'s AC8 and AC10 are currently marked `[ ]`
(unchecked). This audit's independent re-verification finds AC8 and AC10 to be technically PASS for the
reduced 19-member boundary (the specific reduction the maintainer requested is genuinely delivered and
independently re-confirmed). However, `spec.md`'s own AC8/AC10 text explicitly and deliberately ties the
checkbox to a separate governance event — **maintainer ratification of the exemption boundary** — which has
not occurred as of this audit (the boundary is re-submitted for ratification, not yet ratified). Per the
check-off protocol's "Evidence before check-off" rule, this audit does **not** unilaterally check AC8/AC10,
because the specific evidence the source document itself requires for check-off (a maintainer ratification
record) does not yet exist; only the *audit-verifiable* component of AC8/AC10 (technical accuracy of the
reduced boundary and its justifications) has been confirmed. `spec.md`'s AC5 remains correctly unchecked
per this audit's PARTIAL finding. No check-off change was made to `spec.md` by this audit.

This is a deliberate, evidence-based deviation from a default "PASS → check the box" rule, made explicit
here rather than silently applied, consistent with the acceptance-criteria-tracking skill's requirement to
document any gap between an audit's PASS finding and an unchecked source checkbox — the same disposition
independently reached in the cycle-4 exit reaudit for the prior (24-member) boundary.

### AC Status Summary

- Source: `spec.md` (v0.5)
- Total AC items: 10 (AC1-AC10)
- Checked off (delivered): 7 (AC1, AC2, AC3, AC4, AC6, AC7, AC9) — unchanged this cycle
- Remaining (unchecked): 3 (AC5, AC8, AC10)
- Items remaining:
  - AC5: Coverage of the affected testable (non-exempt) denominator is >= 80% — last recomputed at 77.40% (cycle 3); not recomputed this cycle.
  - AC8: The cycle-1 exemption set is reduced ... no member that can be exercised through the narrowed `IItemViewer` or a mockable collaborator retains an exemption — technically PASS per this audit for the reduced 19-member boundary; checkbox withheld pending the maintainer-ratification event `spec.md` itself requires.
  - AC10: Every residual `[ExcludeFromCodeCoverage]` is individually justified ... the boundary is minimized — technically PASS per this audit; checkbox withheld for the same governance reason as AC8.

| Source File | Total AC | Checked (PASS) | Unchecked | Notes |
|-------------|----------|----------------|-----------|-------|
| `spec.md` | 10 | 7 | 3 | Checkbox-backed. AC8/AC10 are audit-PASS but checkbox-withheld pending maintainer ratification of the 19-member boundary (a condition `spec.md` itself imposes); AC5 remains genuinely open (denominator not recomputed, plausibly still below 80%). |

**Feature-audit blocking-finding count: 0** (AC5's sub-80%/non-recomputed reading is a pre-existing,
non-newly-introduced, explicitly-deferred open item, consistent with its cycle-3/4 disposition — not counted
as a cycle-5 blocking finding since it was outside this cycle's assigned remediation scope and the evidence
surrounding it is honest, not overstated).
