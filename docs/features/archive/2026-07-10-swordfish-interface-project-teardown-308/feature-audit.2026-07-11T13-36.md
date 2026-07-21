# Feature Acceptance Audit — swordfish-interface-project-teardown (#308), epic child F5

- **Timestamp:** 2026-07-11T13-36
- **Work mode:** full-feature — AC sources: `spec.md` (AC-1..AC-16) and `user-story.md` (identical AC-1..AC-16 mapping).
- **Base / head:** epic/swordfish-removal-integration `1b65f7a7` → feature branch `0ec111b2`.
- **Epic role:** F5 is the epic-terminal feature; the epic completes here (AC-13).

## Scope and Baseline

The audit compares the full branch diff `git diff 1b65f7a7..0ec111b2` against the AC set in
`spec.md`/`user-story.md`. Each AC is evaluated against the committed evidence artifacts under
`evidence/` plus independent reviewer re-verification (git grep, `gh issue view`, diff inspection).
The baseline for coverage and test comparison is `evidence/baseline/baseline-vstest-coverage.md`
(pre-change, at the integration tip `1b65f7a7`) and the post-change final at
`evidence/qa-gates/finalqc-vstest-coverage.md`.

## Acceptance Criteria Inventory

16 criteria (AC-1..AC-16) across WI-0 (preflight), WI-1 (interfaces), WI-2 (project references),
WI-3 (solution/folders), WI-4 (tests), and cross-cutting epic-completion gates. All 16 are marked
`[x]` in both source files; this audit independently re-verifies each.

## Acceptance Criteria Evaluation

| AC | Criterion (abbrev.) | Verdict | Evidence / re-verification |
|---|---|---|---|
| AC-1 | Preflight: zero `Swordfish` in production `*.cs` (category A) | PASS | `evidence/regression-testing/wi0-preflight-precondition.md`; all matches at base were inside F5 deletion targets or one documentary comment; prior HALT (SCODictionary.cs) resolved by #315/PR #316. |
| AC-2 | Preflight: `TraceUtility.cs` holds no `UtilitiesSwordfish.NET.*` literal | PASS | wi0-preflight (git grep exit 1 on TraceUtility.cs). |
| AC-3 | `IScoCollection.cs` removed | PASS | diff shows deletion; reviewer `git grep -E "IScoCollection2?\b" -- "*.cs"` → zero. |
| AC-4 | `IScoCollection2.cs` removed | PASS | diff deletion; same grep zero. |
| AC-5 | `ISubjectMapSco.cs` removed | PASS | diff deletion; `git grep "ISubjectMapSco"` → zero. |
| AC-6 | Dead `UpdateForMove` removed from `QfcExplorerController.cs`, no dangling symbol | PASS | diff shows the 11-line method removed; reviewer grep confirms the only surviving `UpdateForMove` is an unrelated method in `ToDoModel/Email Utilities/SortItemsToExistingFolder.cs` (distinct, with a real call site); build green with no unresolved `ISubjectMapSco`. |
| AC-7 | All NINE `UtilitiesSwordfish.NET.General.csproj` ProjectReferences removed; Tags/TaskVisualization/TaskVisualization.Test confirmed stale | PASS | `evidence/regression-testing/wi2-projectreference-zero.md` + `wi2-stale-reference-search.md` (precise grep zero; clean-type bindings resolve to first-party UtilitiesCS); reviewer `git grep "UtilitiesSwordfish" -- "*.csproj"` → zero. |
| AC-8 | `TaskMaster.sln` Project(...) declarations for both GUIDs removed | PASS | `evidence/regression-testing/wi3-sln-declarations-removed.md`; reviewer grep for both GUIDs in `.sln` → zero. |
| AC-9 | `TaskMaster.sln` ProjectConfigurationPlatforms rows for both GUIDs removed | PASS | `wi3-sln-config-rows-removed.md`; diff shows 28 lines removed from `.sln`; GUID grep zero (no orphaned rows). |
| AC-10 | Both project folders deleted via `git rm -r` (csprojs, `*.cs`, XAML, nested `.sln`) | PASS | diff shows wholesale deletion of `UtilitiesSwordfish/` and `UtilitiesSwordfish.Test/` incl. `Swordfish.NET.sln`, XAML, resx, config; `wi3-solution-folder-teardown.md`. |
| AC-11 | Three direct-Swordfish test files removed | PASS | diff shows deletion of `ObservableDictionary_Tests.cs`, `ConcurrentObservableCollectionSenderTests.cs`, `ConcurrentObservableCollectionLockRecursionTests.cs`; test count dropped 5358→5346 (12 methods). |
| AC-12 | F2 clean-base regression coverage confirmed; new issue raised if absent (F5 does not author) | PASS | `evidence/other/f2-regression-coverage-confirmation.md`: sender-identity present on the clean base; lock-recursion absent post-removal, issue #317 raised. Reviewer confirmed **#317 OPEN** via `gh issue view 317`. Disposition matches spec Out-of-Scope / Non-Goals. |
| AC-13 | Repo-wide `Swordfish` over `*.cs`/`*.csproj`/`*.sln` returns only archived docs/memory | PASS | `evidence/regression-testing/repo-wide-swordfish-zero.md`; reviewer `git grep "Swordfish" -- "*.cs" "*.csproj" "*.sln"` → exit 1 (zero). Six comment rewordings in F1/F2 files (verified comment-only) were the mechanically-necessary edits; within F5's cross-cutting epic-completion scope. |
| AC-14 | Solution builds green with both projects removed, no unresolved type | PASS | `evidence/qa-gates/finalqc-build-green.md`: analyzer + nullable builds succeeded, 0 errors, no unresolved `Swordfish.NET.*`/`UtilitiesSwordfish` reference. |
| AC-15 | Full C# toolchain passes in order, all four steps green in a single final pass | PASS | `evidence/qa-gates/finalqc-single-pass.md`: csharpier check clean, analyzers 0 errors, nullable 0 errors/0 warnings, MSTest shows only 22 pre-existing environmental Deedle `eng`-model failures — byte-identical to baseline; F5-attributable failures: 0. |
| AC-16 | Coverage thresholds hold; no regression on surviving first-party code from removed tests | PASS | `evidence/qa-gates/coverage-delta-verification.md`: repo-wide raw 68.93%→69.44% (+0.51pp); per-package table shows no surviving first-party production package regressed (UtilitiesCS rose); changed/new executable production lines = 0, so >=90% new-code threshold is met vacuously; 80% testable-denominator floor policy per CLAUDE.md. Non-blocking note: raw Cobertura XML not committed (see policy-audit / code-review). |

## Points-of-Scrutiny Disposition (from caller prompt)

1. **Comment-only rewording of F1/F2 files:** Verified comment/XML-doc only, no behavior change
   (diff inspected). Within F5's AC-13 cross-cutting scope ("epic completes here"), which requires
   the literal code-glob `Swordfish` search to return zero. Appropriate and necessary.
2. **Pre-existing environmental test failures:** Confirmed 22 at baseline and 22 post-change,
   asserted byte-identical by sorted-name diff; all are Deedle `eng`-model environmental failures
   unrelated to F5. Passed/total counts move consistently with the 12 removed test methods only.
   F5 introduced zero new failures.
3. **Coverage:** No surviving first-party production package regressed; F5 owes no backfill
   (numerator and denominator dropped together with the deleted vendored package). 80% testable-
   denominator floor applied per CLAUDE.md (not a 85% raw-repo floor).
4. **AC-12 / issue #317:** Correct disposition per spec Out-of-Scope — F5 flags and files #317
   rather than authoring lock-recursion coverage against the clean base it does not own. #317 OPEN.

## Acceptance Criteria Check-off

All AC-1..AC-16 evaluate to PASS and were already marked `[x]` in `spec.md` and `user-story.md` by
the executor. No check-off change required; no criterion needed to be reverted.

### Acceptance Criteria Status
- Source: `spec.md`, `user-story.md`
- Total AC items: 16
- Checked off (delivered): 16
- Remaining (unchecked): 0
- Items remaining: none

## Summary

All 16 acceptance criteria PASS with corroborating evidence and independent reviewer re-verification.
No PARTIAL, FAIL, or UNVERIFIED criterion. Blocking findings from this feature audit: **0**.
