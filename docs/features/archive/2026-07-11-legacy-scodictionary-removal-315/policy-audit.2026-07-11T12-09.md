# Policy Audit — legacy-scodictionary-removal (#315)

- Timestamp: 2026-07-11T12-09
- Reviewer: feature-review
- Work Mode: full-feature (marker in issue.md line 10)
- Base commit: d2d5e73bfbce7fb73b9d5be1601612cc01e54f09
- Head commit: 7184d0d1 (`git diff d2d5e73b..HEAD`)
- Scope: full branch diff against the epic integration base (NOT a plan/task subset)
- Merge criterion for this artifact set: blocking_count == 0 (FAIL + blocking-PARTIAL across all three artifacts)

## Executive Summary

This change is a net removal of the legacy Swordfish-bound `ScoDictionary<TKey,TValue>` class (zero
production consumers), its two dedicated test files, and their `<Compile Include>` entries, plus pure
type-swaps in three `SmartSerializable*_Tests.cs` files and comment-only edits in two production files and
two test files. The change introduces zero new production executable lines. All committed toolchain evidence
is clean (CSharpier, analyzers, nullable+TreatWarningsAsErrors, MSTest 4223/4223). No policy FAIL or
blocking-PARTIAL findings were identified. Verdict: PASS.

## 1. Changed-File Inventory (verified against `git diff --numstat`)

Production (C#):
- `UtilitiesCS/ReusableTypeClasses/Serializable/Concurrent/SCO/SCODictionary.cs` — DELETED (-460).
- `UtilitiesCS/UtilitiesCS.csproj` — removed one `<Compile Include>` line (-1).
- `UtilitiesCS/OutlookObjects/Folder/FolderScorer.cs` — comment-only (+3/-3), no executable line changed.
- `ToDoModel/Data Model/People/PeopleScoDictionary.cs` — comment-only edit inside an already fully
  commented-out block (+1/-1), no compiled code.

Test (C#):
- `UtilitiesCS.Test/ReusableTypeClasses/SCODictionary_Tests.cs` — DELETED (-296).
- `UtilitiesCS.Test/ReusableTypeClasses/SCODictionary_Additional_Tests.cs` — DELETED (-370).
- `UtilitiesCS.Test/UtilitiesCS.Test.csproj` — removed two `<Compile Include>` lines (-2).
- `UtilitiesCS.Test/ReusableTypeClasses/SmartSerializableBase_Tests.cs` — type-swap to `ScoDictionaryNew<>`
  (+4/-4).
- `UtilitiesCS.Test/ReusableTypeClasses/SmartSerializableNonTyped_Tests.cs` — positives -> `ScoDictionaryNew<>`;
  the two negatives -> first-party `ConcurrentObservableCollection<int>`; added one `using` (+9/-8).
- `UtilitiesCS.Test/ReusableTypeClasses/SmartSerializableStatic_Tests.cs` — deleted the redundant
  `IsSmartSerializable_ScoDictionary_ReturnsFalse` negative (-13); a `ConcurrentObservableCollection` negative
  remains in the same file.
- `UtilitiesCS.Test/EmailIntelligence/FolderRemapController_Tests.cs` — comment-only (+1/-1).
- `UtilitiesCS.Test/EmailIntelligence/SubjectMapEncoder_Tests.cs` — comment-only (+2/-2).

Docs/evidence: research, plan, issue/spec/user-story, and evidence artifacts under the canonical feature
folder. No source files outside the list above were touched.

Language coverage set for this branch diff: C# only. No `.ts/.tsx`, `.py`, or `.ps1/.psm1` files changed;
coverage verdicts for those languages are not applicable because they have zero changed files on the branch.

## 2. General Code Change Policy (`.claude/rules/general-code-change.md`, CLAUDE.md §General)

| Rule | Verdict | Evidence |
|---|---|---|
| Simplicity first | PASS | Change deletes dead code and swaps a concrete stand-in type; no new indirection. |
| Reusability / no copy-paste | PASS | No logic added; retargets reuse the existing first-party `ScoDictionaryNew<>` and `ConcurrentObservableCollection<>`. |
| Separation of concerns | PASS | No structural boundary changed; removal only. |
| Public API compatibility | PASS | Removed class had zero production consumers (issue.md lines 18-19; spec.md lines 15-17). Interfaces `IScoDictionary`/`IPeopleScoDictionary` and converter/wrapper types retained (spec Non-Goals). |
| Error handling / logging | PASS | No error-handling or logging path changed; only deletions and type-swaps. |
| File size <= 500 lines | PASS | Net removal. The deleted `SCODictionary.cs` was 460 lines; no changed file grows across the 500-line limit. Largest edited test file (`SmartSerializableNonTyped_Tests.cs`) remains well under 500 after +9/-8. |
| Dependencies | PASS | No new dependency. Retargets bind first-party `UtilitiesCS.ReusableTypeClasses.*`, removing a `Swordfish.NET.Collections` binding rather than adding one. |
| I/O boundaries | PASS | No I/O path changed. |

## 3. Toolchain Loop (CLAUDE.md §C#; committed evidence, not re-run)

Reviewer does not have build/test tools; verdicts cite committed evidence artifacts under
`evidence/qa-gates/` and `evidence/baseline/`.

| Stage | Verdict | Evidence artifact | Result |
|---|---|---|---|
| 1. Format (CSharpier) | PASS | `qa-gates/final-csharpier.md` | `check .` exit 0, 1375 files, zero drift; only intended files modified. |
| 2. Analyzers (msbuild EnableNETAnalyzers) | PASS | `qa-gates/final-analyzer-build.md` | Build succeeded, 0 errors; warnings pre-existing and unrelated (same 76-warning categories as baseline). One in-loop CS1061 (`Add` vs `TryAdd`) was fixed and the loop restarted from CSharpier per policy. |
| 3. Type-check (nullable + TreatWarningsAsErrors) | PASS | `qa-gates/final-nullable-build.md` | Build succeeded, 0 warnings, 0 errors. |
| 4. Test (MSTest via vstest) | PASS | `qa-gates/final-tests-coverage.md` | 4223/4223 passed, 0 failed. Baseline 4255/0 (`baseline/baseline-tests-coverage.md`); delta -32 = intentional deletions only; zero regressions. |
| Regression (on-disk JSON compat) | PASS | `regression-testing/ondisk-compat-green.md` | `ScoDictionaryNew_OnDiskCompatibility_Tests` 5/5 passed. |

Toolchain loop restart discipline was observed (analyzer CS1061 -> fix -> restart from format), consistent
with the CLAUDE.md §8 loop requirement.

## 4. C# Coverage Verdict (mandatory for the changed language)

C# is the only language with changed files on this branch; it receives an explicit verdict here.

- C# line coverage verdict: PASS. This change is a net removal that adds zero new production executable
  lines, so there is no new-code coverage obligation and no changed production line whose coverage could
  regress. Evidence: `qa-gates/coverage-delta.md` records the only production edits as the `SCODictionary.cs`
  deletion (removes lines from both numerator and denominator) and a comment-only `FolderScorer.cs` edit.
- C# repo-wide coverage: the canonical repository-wide first-party testable-denominator floor is enforced by
  the full test-assembly suite run in CI, and is not recomputed in this review. The committed
  `coverage-delta.md` reports a single-assembly UtilitiesCS.Test line measurement of 60.20% (post) versus
  60.54% (baseline); that single-assembly whole-attachment figure includes vendored Swordfish/SVGControl
  lines in its denominator and is a stable comparison anchor, not the first-party floor. The -0.34 pp
  movement is a non-blocking incidental effect of deleting a wrapper plus its dedicated tests, whose vendored
  base lines remain in the whole-attachment denominator but are exercised by the separate
  `UtilitiesSwordfish.Test` assembly. This single-assembly number is not treated as a repo-wide floor FAIL.
- C# branch coverage: no branch-coverage regression is possible because zero production branches were added
  or modified; PASS.
- Canonical `artifacts/csharp/coverage.xml` was not generated for this review (per reviewer instruction for a
  net-removal change); no such artifact is asserted.

Coverage verdict for C#: PASS.

## 5. Evidence Location Compliance

All evidence artifacts are committed under the canonical `<FEATURE>/evidence/<kind>/` tree
(`evidence/baseline/`, `evidence/qa-gates/`, `evidence/regression-testing/`). The branch diff contains no
files written to `artifacts/baselines/`, `artifacts/qa/`, `artifacts/evidence/`, or `artifacts/coverage/`.
Verdict: PASS. No `EVIDENCE_LOCATION_OVERRIDE_REJECTED` conditions were triggered.

## 6. Scope Invariant / Rejected Scope Narrowing

The audit scope is the full branch diff `d2d5e73b..HEAD`. No caller instruction attempted to narrow the
audit to a plan/task/phase subset, to skip a language with changed files, or to mark C# out of scope. The
caller's coverage guidance (classify the single-assembly ~60% figure as a non-blocking single-assembly
measurement rather than a repo-wide floor FAIL) was evaluated and determined to be measurement-methodology
clarification, not a prohibited scope narrowing: C# remains fully in scope and is given an explicit PASS
verdict in Section 4, and the full branch diff was audited. No verbatim scope-narrowing text is on record to
quote.

## 7. Policy Verdict Summary

| Area | Verdict |
|---|---|
| General Code Change Policy | PASS |
| C# Code Change Policy (toolchain order/commands) | PASS |
| General + C# Unit Test Policy (retargets deterministic, isolated, AAA preserved) | PASS |
| C# coverage (changed language) | PASS |
| Evidence location compliance | PASS |
| Scope invariant | PASS |

Blocking policy findings: 0.
