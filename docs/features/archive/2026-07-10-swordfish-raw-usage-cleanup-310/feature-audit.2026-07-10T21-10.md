# Feature Audit — swordfish-raw-usage-cleanup (Issue #310, epic swordfish-removal child F4)

- Reviewed branch: `feature/swordfish-raw-usage-cleanup-310`
- Reviewed commit (HEAD): `97b0da88`
- Base: `origin/epic/swordfish-removal-integration` @ `0b72b11bb1145dd00f70fe9de8d7a6ed3bef79bb`
- Work mode: `full-feature`
- AC source files: `docs/features/active/2026-07-10-swordfish-raw-usage-cleanup-310/spec.md`, `docs/features/active/2026-07-10-swordfish-raw-usage-cleanup-310/user-story.md`
- Timestamp: 2026-07-10T21-10

## Scope and Baseline

This review evaluates the full branch diff of `feature/swordfish-raw-usage-cleanup-310` against
its resolved base `origin/epic/swordfish-removal-integration` (merge-base
`0b72b11bb1145dd00f70fe9de8d7a6ed3bef79bb`, confirmed via `git merge-base`, matching the current
tip of the integration branch after fetch — no divergence). The branch is a single commit
(`97b0da88`) containing five production `.cs` file changes plus feature-document and evidence
updates. The audit scope is the full diff, not any plan phase subset; no scope-narrowing was
attempted or applied (see `policy-audit.2026-07-10T21-10.md`, `## Rejected Scope Narrowing`).

## Acceptance Criteria Inventory

| AC | Source | Text (abbreviated) |
|---|---|---|
| AC1 | `spec.md`, `user-story.md` | `KbdActions.cs` `_list` re-pointed to `List<UClass>`; no `Swordfish.NET.Collections` reference; public API unchanged; existing `KbdActions` tests pass. |
| AC2 | `spec.md`, `user-story.md` | `using Swordfish.NET.Collections;` removed from `KeyboardHandler.cs`, `FlagDetails.cs`, `FolderRemapController.cs`; solution rebuilds clean. |
| AC3 | `spec.md`, `user-story.md` | Stale `"UtilitiesSwordfish.NET.General"` / `"UtilitiesSwordfish.NET.Test"` literals in `TraceUtility.cs` deleted, rationale recorded. |
| AC4 | `spec.md`, `user-story.md` | No Sco* lineage class or its consumers modified beyond the `KbdActions` swap; `UtilitiesSwordfish` project, `ProjectReference` entries, `TaskMaster.sln` untouched. |
| AC5 | `spec.md`, `user-story.md` | Full C# toolchain (CSharpier, .NET analyzers, nullable, MSTest) passes; changed/new code meets coverage thresholds with no regression on changed lines. |

`issue.md` also carries its own copy of AC1-AC5 (identical text, still unchecked `- [ ]` at time of
review — `issue.md` is not the AC source of record for `full-feature` work mode per the
acceptance-criteria-tracking skill's mode-resolution table; `spec.md` and `user-story.md` are).

## Acceptance Criteria Evaluation

### AC1 — KbdActions collection swap

- **Verdict: PASS.**
- `QuickFiler/Controllers/KbdActions.cs:31` declares `private List<UClass> _list = new();`; both
  constructors (`KbdActions()` at line 22, `KbdActions(IEnumerable<UClass> list)` at line 27)
  assign `new List<UClass>()` / `new List<UClass>(list)`. `using Swordfish.NET.Collections;` is
  removed (confirmed: `rg -n "Swordfish\.NET" QuickFiler/Controllers/KbdActions.cs` returns no
  matches).
- Public API: read the complete post-change file; all public constructor/method signatures
  (`this[TKey]`, `ContainsKey`, `FilterKeys`, `Find`, `FindIndex(TKey)`, both `Add` overloads,
  `Remove`, `GetEnumerator`, `Keys`) are textually unchanged. `_list` is `private` both before and
  after.
- `FindIndex(Predicate<T>)`, `Add`, `RemoveAt(int)`, the enumerator, and LINQ are all natively
  provided by `List<T>` and are exercised by the call sites at lines 80 and 125.
- Existing `KbdActions` tests: 13/13 pass unchanged (`evidence/regression-testing/kbdactions-regression.2026-07-10T20-20.md`); both test files (`KbdActionsTests.cs`, `KbdActionsRemainingBranchesTests.cs`) are confirmed unmodified.
- Coverage: `KbdActions<TKey,UClass,VDelegate>` class line-rate 93.98%/branch-rate 100%, identical baseline vs post-change (independently re-derived from `evidence/qa-gates/final-coverage-repository.xml`).

### AC2 — Unused `using` removal

- **Verdict: PASS.**
- `using Swordfish.NET.Collections;` removed from `KeyboardHandler.cs:17`, `FlagDetails.cs:13`, and
  `FolderRemapController.cs:10` (confirmed via diff and post-change `rg` scan — no matches in any
  of the three files).
- No other line changed in any of the three files (confirmed via diff inspection: each file's diff
  hunk is a single-line deletion).
- Solution rebuild: `EXIT_CODE: 0`, 0 errors, no warning/error attributed to any of the three files
  (`evidence/qa-gates/phase2-unused-using-build.2026-07-10T20-20.md`).

### AC3 — TraceUtility literal disposition

- **Verdict: PASS.**
- `"UtilitiesSwordfish.NET.General"` and `"UtilitiesSwordfish.NET.Test"` are deleted from the
  `_projectNames` initializer in `UtilitiesCS/HelperClasses/Logging/TraceUtility.cs` (confirmed via
  diff; `rg -n "UtilitiesSwordfish\.NET" UtilitiesCS/HelperClasses/Logging/TraceUtility.cs` returns
  no matches).
- Rationale is recorded in `spec.md`'s "Research Resolution" section and in
  `research/swap-target-decision-record.md`: both strings are assembly simple-names of Swordfish
  projects the epic deletes in F5, so once those projects no longer exist in the solution the
  entries are dead filter names; `List<string>.Contains`-style membership is per-element
  independent, so the deletion is behavior-neutral for every surviving filter name. This is a
  documented disposition with rationale, satisfying the AC's requirement.
- Build verification: `EXIT_CODE: 0`, 0 errors, no warning/error attributed to the file
  (`evidence/qa-gates/phase3-traceutility-build.2026-07-10T20-20.md`).

### AC4 — Scope boundary

- **Verdict: PASS.**
- Independently confirmed (not solely from executor evidence): `git diff origin/epic/swordfish-removal-integration...HEAD --name-only`
  lists exactly the five production files plus feature-doc/evidence files; no file path matching
  `Sco(Dictionary|Collection|Stack|SortedDictionary)` or `UtilitiesSwordfish` appears.
  `git diff origin/epic/swordfish-removal-integration...HEAD -- '*.sln' '*.csproj'` is empty,
  confirming no `TaskMaster.sln` or `ProjectReference` change.
- No interface migration occurred (`IScoCollection`/`IScoCollection2` not referenced in the diff).
- No new production type, test, package, or project reference was added.

### AC5 — Full C# toolchain and coverage

- **Verdict: PASS.**
- CSharpier: baseline and final passes both clean, 0 files reformatted.
- .NET analyzers: baseline and final builds `EXIT_CODE: 0`, 0 errors; no analyzer diagnostic
  attributed to any changed file in either pass.
- Nullable / `TreatWarningsAsErrors`: baseline and final builds `EXIT_CODE: 0`, 0 warnings, 0
  errors.
- MSTest: 4758/4758 tests passed in both the baseline and final coverage-mode runs (numeric,
  genuine Cobertura coverage data — not a placeholder; see `policy-audit.2026-07-10T21-10.md`
  `## 5. Test Coverage Detail` for the toolchain-command-deviation verification and the full
  per-file coverage table).
- No regression on changed lines: independently re-derived, PASS for all five changed files (see
  policy-audit for the per-class table). No new production files were added, so the >=90% new-file
  coverage gate does not apply.
- One non-blocking, documentation-accuracy finding is carried into the policy audit regarding the
  "Repo-wide" coverage label being a two-project aggregate rather than a true solution-wide figure;
  it does not change this AC's verdict, which depends on the per-file no-regression comparison.

## Acceptance Criteria Status

- Source: `docs/features/active/2026-07-10-swordfish-raw-usage-cleanup-310/spec.md`, `docs/features/active/2026-07-10-swordfish-raw-usage-cleanup-310/user-story.md`
- Total AC items: 5 (AC1-AC5), tracked identically in both source files (10 checkbox lines total)
- Checked off (delivered): 10 of 10 (all AC1-AC5 checkboxes in both `spec.md` and `user-story.md`
  were already `- [x]` at the start of this review; this audit independently verified each PASS
  claim above against the branch diff and evidence rather than accepting the check-off at face
  value)
- Remaining (unchecked): 0
- Items remaining: none

## Acceptance Criteria Check-off

No new check-offs were required by this review: all ten AC checkboxes (AC1-AC5 in both `spec.md`
and `user-story.md`) were already marked `- [x]` prior to this audit, and this audit's independent
verification confirms every one evaluates to PASS. No AC was found to require unchecking.

## Summary

All five acceptance criteria (AC1-AC5) evaluate to **PASS** under independent verification against
the branch diff, consumer-impact grep, merge-base recomputation, and re-derived coverage data — not
solely on the executor's self-reported evidence. No Blocking findings. Recommended verdict:
**Go for merge.**
