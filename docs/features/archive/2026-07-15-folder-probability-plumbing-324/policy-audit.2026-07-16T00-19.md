# Policy Compliance Audit — folder-probability-plumbing (#324)

- Timestamp: 2026-07-16T00-19
- Feature branch: feature/folder-probability-plumbing-324 @ d9bfe081
- Base (merge-base): origin/epic/folder-tree-percentage-ui-integration (6d4535c6)
- Diff: `git diff origin/epic/folder-tree-percentage-ui-integration...HEAD`
- Work mode: full-feature (AC sources: spec.md + user-story.md)
- Reviewer scope: full branch diff vs the resolved base branch (not any plan/task subset)

## Executive Summary

Verdict: PASS. This is an additive, strongly-typed contract on the folder scoring layer with no
scoring, ranking, or model change. All four core policies (CLAUDE.md, general-code-change,
general-unit-test, C# code-change + C# unit-test) are satisfied for the changed files. The full C#
toolchain is green for the two touched first-party projects. New and changed code clears the
stricter coverage regime. One non-blocking policy deviation is recorded: two pre-existing production
files that this additive change extended remain above the 500-line file-size limit (documented under
Finding P-1). No blocking findings.

Blocking findings: 0.

## Rejected Scope Narrowing

None. The caller supplied the authoritative base branch and full-diff scope. Coverage-scoping
guidance in the delegation prompt directs coverage assessment onto the changed/new code with the
ratified CLAUDE.md COM/VSTO/WinForms testable-denominator exemption applied to the pre-existing
repo-wide figure; that is consistent with CLAUDE.md and is not a scope narrowing of the branch diff.
Every language with changed files is assessed below with an explicit verdict.

## Changed-Language Inventory

| Language | Changed files | Coverage verdict |
|---|---|---|
| C# (.cs) | 2 production modified, 2 production new, 3 test new (+ 2 .csproj Include edits) | PASS (see C# Coverage) |
| TypeScript | none | not assessed (zero changed files) |
| Python | none | not assessed (zero changed files) |
| PowerShell | none | not assessed (zero changed files) |

Only C# has changed files on the branch; its coverage verdict is explicit below.

## 1. Core Code-Change Policy (general-code-change / CLAUDE.md)

| Rule | Verdict | Evidence |
|---|---|---|
| Simplicity, separation of concerns | PASS | Pure value types (FolderScore, FolderRow) separate from scoring/adapters; shared `OrderedScores()` factored to avoid duplication. |
| Reusability (no copy-paste) | PASS | `OrderedScores()` is consumed by both `ToArray*` and `ToScoredArray*`; `BuildScoredArray` centralizes normalization. |
| Extensibility / additive public API | PASS | All new surface is additive; existing signatures unchanged. |
| Error handling / fail-fast | PASS | Zero-guard on `TopScore == 0`; no broad catches added; `"Error"` sentinel rejection preserved. |
| Naming conventions | PASS | PascalCase types/members, camelCase locals; descriptive names. |
| Dependencies | PASS | No new third-party dependency added. |
| I/O isolation | PASS | New projections are read-only over in-memory `_folderNameScores`; no disk/network/COM introduced. |
| File-size limit (<= 500 lines) | PARTIAL (non-blocking) | Finding P-1: FolderPredictor.cs 974 (baseline 823) and FolderScorer.cs 663 (baseline 617) exceed 500; pre-existing overage extended by additive members. |

## 2. C# Code-Change Policy

| Rule | Verdict | Evidence |
|---|---|---|
| CSharpier formatting | PASS | evidence/qa-gates/qc-csharpier.md — `dotnet csharpier check .` EXIT 0, 0 files require formatting (1341 files). |
| .NET analyzers (EnableNETAnalyzers + EnforceCodeStyleInBuild) | PASS | evidence/qa-gates/qc-analyzer-build.md — Build succeeded, 0 errors, 0 warnings. |
| Nullable / TreatWarningsAsErrors | PASS (feature scope) | evidence/qa-gates/qc-nullable-build.md — UtilitiesCS 0, UtilitiesCS.Test 0 nullable errors. See Finding P-2 for pre-existing vendored SVGControl debt. |
| net48-safe value types (no record/init) | PASS | FolderScore/FolderRow are `public readonly struct` with ctor + get-only auto-properties; precedent ResourceTimingRow. |
| XML docs on non-obvious contract | PASS | FolderScore/FolderRow/FolderScorer/FolderPredictor new members carry XML docs. |
| Strong contracts / explicit types | PASS | Explicit public types at boundaries; nullable `FolderScore?` used only for non-suggestion rows. |

## 3. General Unit Test Policy

| Rule | Verdict | Evidence |
|---|---|---|
| Framework MSTest | PASS | `[TestClass]/[TestMethod]` in all three new test files. |
| Moq for mocking | PASS | FolderRowTests reuses the existing mocked-Outlook harness (CreateFolder/CreateApplication/CreateGlobals). |
| FluentAssertions | PASS | `.Should().Equal(...)` / `.BeEmpty()` throughout. |
| Arrange-Act-Assert + intent | PASS | All tests use explicit Arrange/Act/Assert with descriptive names and comments. |
| Independence / determinism | PASS | No shared mutable state; ordinal tie-break locked; no wall-clock/RNG. |
| No temp files / no external deps | PASS | Pure in-memory scorer seeding via `AddSuggestion`; no filesystem/network. |
| Test file location (tests/ mirror) | PASS | Tests live under UtilitiesCS.Test/OutlookObjects/Folder mirroring production path; not colocated in source. |
| Scenario completeness | PASS | Positive, empty, all-zero, tie, topN>count, mixed-source, and "Error"-rejection scenarios present. |

## 4. C# Coverage

Coverage evidence source: readable Cobertura produced by dotnet-coverage wrapping VS18
vstest.console.exe, recorded in evidence/qa-gates/qc-vstest-coverage.md and
evidence/qa-gates/coverage-delta.md (feature evidence). The canonical artifacts/csharp/coverage.xml
was intentionally not regenerated for this additive contract; verification is by inspection of the
feature's own Cobertura evidence per the delegation instruction and CLAUDE.md exemption authority.

| Language | Coverage verdict | Detail |
|---|---|---|
| C# (.cs) | PASS | New/changed-code coverage: FolderScore.cs 100% line/100% branch; FolderRow.cs 100% line/100% branch; FolderScorer.OrderedScores/ToScoredArray()/ToScoredArray(int)/BuildScoredArray 100% line with BuildScoredArray branch 100% (empty / zero-guard / topN paths); FolderPredictor.FolderRowArray 100%, FindFolderRows 95.7% line/100% branch, AddMatchRows/AddSuggestionRows/AddRecentRows 100% line. Both touched classes improved vs baseline (FolderScorer 97.75%->97.85%, FolderPredictor 86.71%->88.86%); no reduction on changed lines. Repo-wide coverage 59.42% line / 30.37% branch (baseline 59.35% / 30.28%, no regression), reflecting the ratified CLAUDE.md COM/VSTO/WinForms testable-denominator exemption and pre-existing before this change. No production file excluded from measurement. |

Baseline vs post-change (numeric):
- Baseline: 59.35% repo-wide line coverage.
- Post-change: 59.42% repo-wide line coverage.
- Change: +0.07 points.
- Disposition: PASS. New/changed-code coverage clears the stricter regime (>= 90% line on new
  members; branch coverage of empty/all-zero/tie/topN paths present). The repo-wide figure below the
  85% uniform floor is a pre-existing condition governed by the ratified COM/VSTO/WinForms
  testable-denominator exemption in CLAUDE.md; it did not regress and is not introduced by this
  feature.
- Evidence: evidence/qa-gates/coverage-delta.md, evidence/qa-gates/qc-vstest-coverage.md,
  evidence/baseline/baseline-vstest-coverage.md.

TypeScript / Python / PowerShell: zero changed files on the branch; no coverage assessment required
for those languages.

## 5. Evidence Location Compliance

All evidence this feature produced lives under the canonical
`docs/features/active/2026-07-15-folder-probability-plumbing-324/evidence/<kind>/` tree
(baseline, qa-gates, regression-testing, other). The branch diff contains no files written under
`artifacts/baselines/`, `artifacts/qa/`, `artifacts/evidence/`, or `artifacts/coverage/`. No
evidence-location violation found.

## 6. Backward-Compatibility Policy (spec-mandated)

| Protected output | Verdict | Evidence |
|---|---|---|
| FolderScorer.ToArray() / ToArray(int) | PASS | Refactored to consume shared `OrderedScores()`; golden-baseline regression tests lock ordering + content incl. ordinal tie-break (FolderScorerRegressionTests). |
| FolderPredictor.FolderArray | PASS | FolderRowArray uses a local list and does not mutate `_folderList`; FolderRowTests.FolderRowArray_DoesNotAlterFolderArrayOutput. |
| FolderPredictor.FindFolder(...) | PASS | FindFolderRows uses a local list, mirrors AddMatches/AddSuggestions/AddRecents exactly; does not touch cached `_folderList`. Verified structurally against source (AddMatches/AddSuggestions/AddRecents at FolderPredictor.cs L776-799). |
| IFolderSearchHandler shape | PASS | Unchanged; no member added. |

## 7. Findings

### Finding P-1 (Major, non-blocking) — pre-existing 500-line file-size overage extended
- Rule: general-code-change.md "No production code file may exceed 500 lines."
- Files: UtilitiesCS/OutlookObjects/Folder/FolderPredictor.cs (974 lines; baseline 823),
  UtilitiesCS/OutlookObjects/Folder/FolderScorer.cs (663 lines; baseline 617).
- Assessment: Both files were already above 500 lines at the base commit; the feature added cohesive
  instance members that legitimately access private state (`_folderNameScores`, `_folderList`,
  `_globals`). The two new value types were correctly placed in their own files (FolderScore.cs,
  FolderRow.cs). Refactoring the pre-existing oversized classes is outside this additive feature's
  scope and would risk the byte-for-byte backward-compat guarantee.
- Disposition: non-blocking. Recommendation: track partial-class extraction (e.g.
  FolderPredictor.Rows.cs / FolderScorer.Scored.cs) as separate tech-debt.

### Finding P-2 (Informational, non-blocking) — pre-existing out-of-feature diagnostics
- 34 nullable diagnostics in vendored SVGControl.csproj under a forced Rebuild are baseline-identical
  and untouched by this feature.
- 17 Deedle/DataFrame/ETL tests fail only under coverage instrumentation (green without it); none are
  Folder-scoring tests. Pre-existing flakiness unrelated to this change.
- Disposition: non-blocking; noted for the epic backlog.

## Verdict

Policy compliance: PASS. Blocking findings: 0. Two documented non-blocking deviations (P-1, P-2).
