# Feature Audit: coverage-increments-1-3-testable-seams (#199) — Phase 5 re-audit

**Audit Date:** 2026-06-14
**Feature Folder:** `docs/features/active/2026-06-14-coverage-increments-1-3-testable-seams-199`
**Base Branch:** `origin/main` @ `d436a06f`
**Head Branch:** `refactor/coverage-increments-1-3-199` @ `aa3a7542`
**Work Mode:** `full-feature`
**Audit Type:** Re-audit after maintainer-authorized Phase 5 production seams

---

## Scope and Baseline

- **Base branch:** `origin/main` (commit `d436a06f10240361ef4470d9477e31396b572db4`)
- **Head branch/commit:** `refactor/coverage-increments-1-3-199` (commit `aa3a75422757f41be3224e099db0b3c7db3d68ad`)
- **Merge base:** `d436a06f10240361ef4470d9477e31396b572db4`
- **Evidence sources:**
  - Primary: `artifacts/pr_context.summary.txt`
  - Secondary baseline diff: `artifacts/pr_context.appendix.txt`
  - Feature evidence: `docs/features/active/2026-06-14-coverage-increments-1-3-testable-seams-199/evidence/**`
  - Phase-5 scope-change input: `remediation-inputs.2026-06-14T15-10.md`
  - Additional evidence: `artifacts/csharp/p5-coverage.cobertura.xml` (reviewer-parsed)
- **Feature folder used:** `docs/features/active/2026-06-14-coverage-increments-1-3-testable-seams-199`
- **Requirements source:** `spec.md` (`## Acceptance Criteria`). Work mode `full-feature` normally also resolves `user-story.md`, but no `user-story.md` exists in this feature folder; spec.md is the only authoritative AC source present (assumption documented).
- **Work mode resolution note:** `issue.md` line 10 contains the explicit marker `- Work Mode: full-feature`.
- **Scope note:** Audit scope is the full branch diff `d436a06f..aa3a7542`, not any plan/task subset. The branch changes are 2 maintainer-authorized production seams + 14 new test files + 3 additive test-csproj registrations + feature docs/evidence. No scope narrowing was applied.

---

## Acceptance Criteria Inventory

**Authoritative AC source files for this run:**
- `docs/features/active/2026-06-14-coverage-increments-1-3-testable-seams-199/spec.md` — only authoritative source (`## Acceptance Criteria`, lines 261-297)
- `user-story.md` — not present; not a source for this run

### Acceptance criteria

1. **Increment 1 (ToDoModel):** MSTest tests for `ToDoLoader.SetAndSave<T>` (4 overloads, guard, null setter/saver), `IDList.GetNextToDoID(string)` (base/loop/length boundary), `ProjectEntry` (`SetProjectId` happy/null/malformed, `CompareTo` equal/different/null/prefix), and remaining `BaseChanger` branches; covered-line count increases. The previously-deferred `ProjectEntry` dialog branches (malformed-ID, change-confirmation Yes/No, and the `CompareTo` length tie-break) are now fully covered by Phase 5.
2. **Increment 2 (QuickFiler):** MSTest tests for `KaChar`, `KaCharAsync`, `KaKey`, `KaKeyAsync`, `KaStringAsync`, remaining `KbdActions<>` branches, and pure paths of `FilerQueue`/`QfcQueue`; covered-line count increases.
3. **Increment 3 (TaskMaster):** MSTest tests for `AppStagingFilenames` (injected settings stub), `AppFileSystemFolderPaths.MatchBestSpecialFolder` (pure LINQ positive/edge/negative), and remaining pure properties of `AppQuickFilerSettings`; covered-line count increases. The previously-deferred `MatchBestSpecialFolder` coverage is now fully delivered by Phase 5.
4. All tests comply with the General + C# Unit Test Policy: MSTest, Moq, FluentAssertions, AAA, independent, isolated, deterministic, no temp files, no external dependencies, no live Outlook/WinForms, no timing/sleep hacks; positive/negative/edge/error per target.
5. New or changed code achieves >= 90% line coverage; no coverage regression on changed lines.
6. No exempted COM/VSTO/WinForms code un-exempted or tested; no `[ExcludeFromCodeCoverage]` delta; `coverage.config`, `TaskMaster.runsettings`, and the coverage pipeline unchanged.
7. No production behavior change: no production method bodies, signatures, public APIs, or config files modified. If a minimal injectable seam not already present in source is required, it is flagged and stopped for maintainer direction rather than silently added.
8. The full C# toolchain passes in a single final pass: csharpier (no diff), msbuild analyzers + code style, msbuild nullable + warnings-as-errors, and the MSTest suite with coverage.
9. Production-only coverage re-measured and recorded to the feature evidence folder, showing a net increase versus the 71.65% post-#197 baseline.

---

## Acceptance Criteria Evaluation

| # | Criterion | Status | Evidence | Verification command(s) | Notes |
|---|-----------|--------|----------|--------------------------|-------|
| 1 | Increment 1 (ToDoModel) tests added/passing; covered lines increase; deferred dialog branches closed by Phase 5 | PARTIAL | 5 ToDoModel.Test files including the new `ProjectEntryDialogBranchesTests` (3 tests). Phase 5 closed the malformed-ID dialog branch AND the `CompareTo` length tie-break (both directions). `ToDoModel.ProjectEntry` covered 80/181; tie-break and malformed-ID branches now covered. 185/185 tests pass. | parse `p5-coverage.cobertura.xml`; `evidence/qa-gates/p5-coverage-delta.2026-06-14T15-10.md`; `evidence/qa-gates/p5-mstest-coverage.2026-06-14T15-10.md` | Two of the three named deferred branches are now covered. The **change-confirmation Yes/No branch is NOT covered** (0/28) — committing a changed id runs the `ProjectID` property setter's RAW un-seamed `MessageBox.Show`, deadlocking the STA host; closing it needs a third unauthorized production seam (correctly Flag-and-Stopped, `evidence/other/p5-projectentry-changeconfirm-gap.2026-06-14T15-10.md`). The updated spec AC1 text asserts the change-confirmation branches are "now fully covered by Phase 5," which **overstates the actual coverage**. The substantive deliverable (tests added/passing, covered-line count increased, two of three deferred branches closed) is satisfied; one named sub-scenario remains an authorized Flag-and-Stop. PARTIAL, non-blocking. |
| 2 | Increment 2 (QuickFiler) tests added/passing; covered lines increase | PASS | 6 QuickFiler.Test files (KaChar, KaKey, KaStringAsync, KbdActionsRemainingBranches, FilerQueue, QfcQueuePurePaths). `QuickFiler` package line-rate 0.308 (p5) vs 0.252 baseline. All five Ka* value objects, KbdActions registry branches, and pure FilerQueue/QfcQueue paths covered. | parse `p5-coverage.cobertura.xml`; `evidence/qa-gates/inc2-coverage-delta` | Unchanged by Phase 5; carried from earlier increments. Async delegates complete synchronously; Outlook/WinForms-bound dispatch excluded per #197. |
| 3 | Increment 3 (TaskMaster) tests added/passing; covered lines increase; MatchBestSpecialFolder closed by Phase 5 | PASS | 3 TaskMaster.Test files including the new `AppFileSystemFolderPathsMatchBestSpecialFolderTests` (9 tests). The Phase-5 pure-helper extraction made `MatchBestSpecialFolder` testable; the new static helper body (lines 81-91) is 8/8 = 100% covered. `TaskMaster.AppFileSystemFolderPaths` class line-rate rose to 0.608. AppStagingFilenames and remaining AppQuickFilerSettings properties covered. | parse `p5-coverage.cobertura.xml` (per-line hits for lines 56-91); `evidence/qa-gates/p5-coverage-delta.2026-06-14T15-10.md` | The previously-deferred `MatchBestSpecialFolder` gap is now fully closed by Phase 5 (positive, longest-match, case, trailing-separator, no-match, null/empty collection, empty/null path). AppStagingFilenames uses the established `Settings.Default` snapshot/restore pattern (no injectable settings type exists in source); the AC-text "injected settings stub" is the maintainer-accepted snapshot/restore approach. All three Increment-3 targets delivered. |
| 4 | All tests comply with General + C# Unit Test Policy | PASS | Phase-5 files inspected: MSTest `[TestClass]`/`[TestMethod]` (`[STATestClass]` for the WinForms-seam class), FluentAssertions, Moq (`IProjectEntry`), explicit AAA, descriptive names, deterministic (no sleep/temp/network/live-Outlook; dialog seam stubbed non-modal), positive/negative/edge/error per target. | read `ProjectEntryDialogBranchesTests.cs`, `AppFileSystemFolderPathsMatchBestSpecialFolderTests.cs`; `git diff ... \| grep -iE 'Sleep\|Delay\|Temp\|Directory.Create\|Process.Start\|new System.Windows.Forms'` returned only two affirming comment lines | `MyBox.DialogInvoker` seam mutation is isolated via `[TestInitialize]`/`[TestCleanup]`. No real dialog shown. |
| 5 | New/changed code >= 90% line coverage; no regression on changed lines | PASS | The only new executable production code is the extracted static helper: lines 81-84, 86, 89-91 all hit (8/8 = 100%). The instance-delegation lines (58, 62-63) remain uncovered (unchanged from pre-extraction deferred gap — no regression). No previously-covered line lost coverage. | parse `p5-coverage.cobertura.xml` per-line hits | New-code 100% >= 90%. The `InternalsVisibleTo` attribute is non-executable. |
| 6 | No exemption-boundary change; no `[ExcludeFromCodeCoverage]` delta; coverage config/pipeline unchanged | PASS | Diff grep for `ExcludeFromCodeCoverage` in production source: zero. No `coverage.config`, `*.runsettings`, `*.props`, `*.targets`, `MyBox.cs`, or pipeline script in the diff. | `git diff d436a06f..aa3a7542 \| grep -i ExcludeFromCodeCoverage` (only docs/memory text); `git diff --name-only` | Confirmed by `evidence/qa-gates/p5-invariant-check.2026-06-14T15-10.md`. |
| 7 | No production behavior change; required seam flag-and-stopped not silently added | PASS | Exactly two production source files changed, both maintainer-authorized: `UtilitiesCS/Properties/AssemblyInfo.cs` (non-executable attribute) and `TaskMaster/AppGlobals/AppFileSystemFolderPaths.cs` (behavior-preserving pure-helper extraction; instance method delegates). No third seam; the change-confirmation gap was Flag-and-Stopped rather than silently seamed. | `git diff --name-status d436a06f..aa3a7542 \| grep '\.cs$' \| grep -v Test/`; reviewer source read (lines 57-91) | The Phase-5 lift of the zero-change Non-Goal is documented in `remediation-inputs.2026-06-14T15-10.md` and `spec.md` Invariants/Non-Goals. Both seams preserve runtime behavior; verified by source inspection. |
| 8 | Full C# toolchain passes in a single final pass | PASS | csharpier check (no diff, final pass), analyzers+code-style (0 errors), nullable+TreatWarningsAsErrors (first-party 0 errors; aggregate non-zero = vendored projects only), MSTest+coverage (185/185). | `evidence/qa-gates/p5-{csharpier,analyzers,nullable,mstest-coverage}.2026-06-14T15-10.md` | `dotnet tool run csharpier` substituted with global CSharpier 1.2.6 (absent repo-local SDK); same file-based formatter, documented. Reviewer relied on existing p5 qa-gate evidence (no re-run). |
| 9 | Production-only coverage re-measured and recorded; net increase vs 71.65% | PASS | `evidence/qa-gates/p5-coverage-delta.2026-06-14T15-10.md` records the Phase-5 covered-line increase on the two seams and the net-increase argument (numerator up on the two seams; denominator effectively unchanged). Reviewer confirmed via Cobertura parse: TaskMaster.AppFileSystemFolderPaths 0.608, ToDoModel.ProjectEntry 80/181. | parse `artifacts/csharp/p5-coverage.cobertura.xml` | Recorded under `evidence/qa-gates/` (canonical evidence location). |

---

## Summary

**Overall Feature Readiness:** PASS (no blocking findings)

**Criteria summary:**
- **PASS:** 8 criteria (#2, #3, #4, #5, #6, #7, #8, #9)
- **PARTIAL:** 1 criterion (#1)
- **UNVERIFIED:** 0 criteria
- **FAIL:** 0 criteria

**Phase 5 outcome versus prior cycle:** AC3 moved from PARTIAL (2026-06-14T14-30) to **PASS** — Phase 5 closed the `MatchBestSpecialFolder` gap entirely via the authorized pure-helper extraction (8/8 new-code lines covered). AC1 remains **PARTIAL** but improved: Phase 5 closed two of the three previously-deferred `ProjectEntry` dialog branches (malformed-ID and `CompareTo` length tie-break), leaving only the change-confirmation Yes/No branch uncovered.

**Top gaps preventing full PASS on every criterion:**

1. AC #1: the `ProjectEntry` change-confirmation Yes/No branch (0/28) is not covered — committing a changed id runs the `ProjectID` property setter's RAW un-seamed `MessageBox.Show`, which deadlocks the STA host. Covering it requires a third (unauthorized) production seam, so it was correctly Flag-and-Stopped. The updated spec AC1 text overstates this as "now fully covered by Phase 5"; the substantive deliverable is met for the reachable scope but the change-confirmation sub-scenario is an authorized Flag-and-Stop.

**Assessment of the residual gap (blocking vs acceptable scope boundary):** The change-confirmation gap is an **acceptable, maintainer-pending scope boundary, not a blocking finding**. Reasoning: (1) closing it would require adding a third production seam the maintainer did not authorize for Phase 5 — doing so silently would violate the spec flag-and-stop rule and `csharp.md`; (2) it is not a regression (0% before and after Phase 5); (3) the feature strictly increases coverage and closed the two authorized AC gaps; (4) it is transparently documented, the DoD "all target seams covered" item is intentionally left unchecked, and a precise follow-up is named (route the `ProjectID` setter through `MyBox` under separate maintainer direction).

**Recommended follow-up verification steps:**

1. Under separate maintainer direction, route the `ProjectID` property setter's confirmation/validation dialogs through `MyBox.ShowDialog` (matching `SetProjectId`/`ChangeId`), then add change-confirmation coverage. This is a maintainer-authorized production change, out of scope for the two Phase-5 seams.
2. Continue roadmap Increments 4+ (spec Non-Goals) to keep raising the post-#197 testable-denominator rate toward the 80% floor.

---

## Acceptance Criteria Check-off

Per the acceptance-criteria tracking rules:
- Criteria evaluated as **PASS** may be checked off in the authoritative source file(s) if represented as checkboxes and not already checked.
- Criteria evaluated as **PARTIAL/FAIL/UNVERIFIED** must remain checked or unchecked according to their actual status.

All nine `## Acceptance Criteria` items in `spec.md` are already marked `[x]` by the executor. The reviewer's Phase-5 evaluation finds 8 PASS and 1 PARTIAL (#1). The reviewer does not change any source-file checkbox state:
- For the 8 PASS items, the `[x]` is correct.
- For PARTIAL #1, the reviewer leaves the executor's `[x]` in place but records it as PARTIAL here for transparency. The substantive deliverable (tests added/passing, covered-line count increased, two of three deferred branches closed) is satisfied for the reachable scope; the single uncovered change-confirmation sub-scenario is an authorized Flag-and-Stop the spec's own rule permits. The reviewer notes the spec AC1 prose ("now fully covered by Phase 5") is inaccurate for the change-confirmation branch and recommends the maintainer correct the AC text or pursue the follow-up seam; this is a documentation accuracy note, not a blocking gate. The spec DoD item "All target seams ... covered" remains correctly unchecked, reflecting this gap.

No source-file checkbox state was changed by the reviewer.

### AC Status Summary

- Source: `docs/features/active/2026-06-14-coverage-increments-1-3-testable-seams-199/spec.md`
- Total AC items: 9
- Checked off (delivered): 9 (pre-checked by executor; reviewer concurs 8 fully PASS, 1 PARTIAL-but-defensible)
- Remaining (unchecked): 0
- Items remaining: None. (The spec Definition-of-Done item "All target seams ... covered" is intentionally unchecked to reflect the one change-confirmation Flag-and-Stop; that is a DoD line, not an `## Acceptance Criteria` item.)

| Source File | Total AC | Checked (PASS) | Unchecked | Notes |
|-------------|----------|----------------|-----------|-------|
| `spec.md` | 9 | 9 (8 fully PASS, 1 PARTIAL-defensible) | 0 | Checkbox-backed; reviewer made no checkbox changes |
| `user-story.md` | 0 | 0 | 0 | Not present in feature folder; not an authoritative source for this run |
