# Policy Compliance Audit — Issue #398 (breadcrumb-suggestions-upgrade-race)

- Timestamp: 2026-07-20T23-28
- Reviewer: feature-review
- Work Mode: minor-audit (from issue.md marker)
- Base branch (resolved): main @ cd6362f0264217d9ed94487f44c193df96eb1fa6 (merge-base; recomputed via `git merge-base HEAD origin/main`, equals origin/main)
- Head: bug/breadcrumb-suggestions-upgrade-race-398 @ 4412d2dabb0b3b32a47215d07a780e0e0decf913
- Scope: full branch diff cd6362f0..4412d2da (12 C# files: 2 production + 6 test + 1 test .csproj + 3 memory .md, 34 Markdown/evidence files)
- Cycle: remediation cycle 1 re-audit (R4). Prior audit timestamp 2026-07-20T22-30.

## Executive Summary

This is the R4 re-audit following remediation of the two findings raised in the 2026-07-20T22-30
audit. The underlying bug fix is unchanged and remains sound: `FolderBreadcrumbBridgeRouter.SetSuggestionsAsync`
builds upgraded rows into a local list and swaps them into `BreadcrumbStateModel` atomically via the
`ReplaceRows` seam, removing the mid-rebuild empty window that produced the reported
`ArgumentOutOfRangeException`. Executor toolchain evidence is green (CSharpier, .NET analyzers, nullable
build, MSTest 5061/5061, all EXIT_CODE 0).

Both prior remediation-required findings are now resolved:

1. **File-size limit (was FAIL, now PASS).** The two over-limit test files were split into cohesive,
   scenario-grouped files. All six changed test files are now under the 500-line limit (Section 6).
2. **C# coverage artifact (was FAIL procedural, now PASS).** The canonical HEAD coverage artifact was
   regenerated at `artifacts/csharp/coverage.xml` (JaCoCo format, first-party UtilitiesCS + QuickFiler
   denominator). It parses under the coverage-gate hook functions to line 86.54% and branch 80.85%,
   both above the repository floors (Section 5).

Overall verdict: PASS. No remediation-required findings remain. One procedural observation is recorded:
the regenerated coverage artifact is at a gitignored tooling-input path and will not travel with the PR;
the repo-wide floor of record for merge is the PR CI coverage run.

## 1. Policy Reading Order Applied

1. CLAUDE.md (all sections)
2. `.claude/rules/general-code-change.md`
3. `.claude/rules/general-unit-test.md`
4. C#: `.claude/rules/csharp.md`; CLAUDE.md C# Code Change Policy + C# Unit Test Policy; `.claude/rules/quality-tiers.md`

## 2. Rejected Scope Narrowing

No scope-narrowing instructions were detected in the caller prompt. The caller supplied the full branch
diff scope and explicitly delegated scope determination to this reviewer. The audit scope is the full
branch diff cd6362f0..4412d2da, not any plan, task, or phase subset. No caller instruction attempted to
mark any language as out of audit, skip a toolchain check, or limit the changed-file set.

## 3. PR-Context Integrity Correction

The refreshed PR-context summary overview again reported "Core logic changes: 0 files" and classified all
C# files (2 production + 5 test) as "Docs/templates/agents/tooling." This is the recurring C#-as-docs
misclassification: the automated overview lists only the top Markdown files in the hook-parsed
`- path (+N/-N)` bullet format, so the coverage-gate hook's `Get-ChangedLanguageSet` would enumerate no
language and silently skip C# coverage enforcement. The C# files were enumerated from `git diff --numstat`:

| File | Adds/Dels | Type |
|---|---|---|
| UtilitiesCS/OutlookObjects/Folder/BreadcrumbStateModel.cs | +28/-1 | modified production |
| UtilitiesCS/OutlookObjects/Folder/FolderBreadcrumbBridgeRouter.cs | +15/-10 | modified production |
| UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbStateModelSequenceTests.cs | +235/-0 | new test (R1 split) |
| UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbStateModelTests.cs | +4/-158 | modified test (R1 split) |
| UtilitiesCS.Test/OutlookObjects/Folder/FolderBreadcrumbBridgeRouterInFlightTests.cs | +256/-0 | new test (R1 split) |
| UtilitiesCS.Test/OutlookObjects/Folder/FolderBreadcrumbBridgeRouterTests.cs | +5/-117 | modified test (R1 split) |
| QuickFiler.Test/Viewers/BreadcrumbBridgeCoordinatorTests.cs | +62/-0 | modified test |
| UtilitiesCS.Test/UtilitiesCS.Test.csproj | +2/-0 | test project (wires 2 new split files) |

`artifacts/pr_context.summary.txt` was corrected in place (seven space-free `.cs` paths added to the
overview) so the coverage gate enumerates C# rather than skipping it. After the correction,
`Get-ChangedLanguageSet` returns `CSharp` and the gate reads the canonical artifact at 86.54% / 80.85%.
Confirmed via `git diff --diff-filter=A --name-only cd6362f0..HEAD -- '*.cs'`: the only added `.cs` files
are the two R1 split test files; no new production `.cs` file was added (the two production files are
modifications).

## 4. Toolchain Verdicts (from executor qa-gate evidence)

| Stage | Command (executor) | EXIT_CODE | Verdict |
|---|---|---|---|
| Format | `csharpier format .` then `csharpier check .` | 0 | PASS |
| Analyzer | `msbuild TaskMaster.sln /t:Build ... /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` | 0 | PASS |
| Nullable | `msbuild TaskMaster.sln /t:Build ... /p:Nullable=enable /p:TreatWarningsAsErrors=true` | 0 | PASS |
| Tests | `vstest.console.exe UtilitiesCS.Test... QuickFiler.Test... /EnableCodeCoverage /InIsolation` | 0 | PASS (5061/5061) |

Evidence: `docs/features/active/2026-07-20-breadcrumb-suggestions-upgrade-race-398/evidence/qa-gates/*.2026-07-20T22-30.md`
(post-remediation) and `*.2026-07-20T21-41.md` (original fix). These verdicts rely on committed executor
evidence; they were not independently re-executed by the reviewer. The two new split test files and the
`.csproj` compile-item wiring are covered by the same 5061/5061 passing run.

## 5. Coverage Verification (mandatory for every changed language)

### 5.1 C# — PASS (canonical HEAD coverage artifact present; both floors met)

- Changed C# files: 2 modified production, 6 test (Section 3). C# coverage is mandatory.
- The canonical HEAD coverage artifact at `artifacts/csharp/coverage.xml` (JaCoCo format, first-party
  UtilitiesCS + QuickFiler denominator, `[ExcludeFromCodeCoverage]` honored) was regenerated during
  remediation and reflects HEAD (4412d2da). Independently parsed by this reviewer via the coverage-gate
  hook functions `Get-JacocoRepoCoverage` / `Get-JacocoBranchCoverage`:
  - C# repo-wide line coverage: 86.54% (43143/49851) — >= 85% line floor: PASS.
  - C# branch coverage: 80.85% (9331/11541) — >= 75% branch floor: PASS.
- New/changed-code line coverage: 100% (>= 90% target): PASS. Per-file executor evidence:
  `FolderBreadcrumbBridgeRouter.cs` line 97.55% / branch 90.00%; `BreadcrumbStateModel.cs` (including the
  new `ReplaceRows` seam) line 100.00% / branch 94.64%; `BreadcrumbBridgeCoordinator.cs` (unchanged)
  line 97.32% / branch 83.33%. No regression on changed lines.
- Coverage disposition: PASS. Both repository floors are met by the canonical HEAD artifact and the
  new/changed-code target is met.
- Evidence: `evidence/qa-gates/jacoco-coverage-artifact.2026-07-20T22-30.md`,
  `evidence/qa-gates/coverage-floor-verification.2026-07-20T22-30.md`,
  `evidence/qa-gates/coverage-delta.2026-07-20T21-41.md`.
- Procedural observation (non-blocking): the coverage artifact is at a gitignored tooling-input path
  (`git check-ignore` confirms) and therefore is not committed with the PR. The repo-wide floor of record
  at merge is the PR CI coverage run; the local canonical artifact substantiates the verdict for this
  review.

### 5.2 Other languages

- TypeScript: no changed files in the branch diff; coverage not evaluated (no `.ts`/`.tsx` in diff).
- Python: no changed files in the branch diff; coverage not evaluated (no `.py` in diff).
- PowerShell: no changed files in the branch diff; coverage not evaluated (no `.ps1`/`.psm1` in diff).

## 6. File-Size Limit — PASS (R1 remediation resolved)

The R1 remediation split the two over-limit test files into cohesive, scenario-grouped files. All changed
test files are now under the 500-line limit. Head line counts verified via `awk 'END{print NR}'`:

| File | Baseline lines | Head lines | Verdict |
|---|---|---|---|
| UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbStateModelTests.cs | 536 (prior head) | 320 | PASS |
| UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbStateModelSequenceTests.cs | new | 235 | PASS |
| UtilitiesCS.Test/OutlookObjects/Folder/FolderBreadcrumbBridgeRouterTests.cs | 545 (prior head) | 314 | PASS |
| UtilitiesCS.Test/OutlookObjects/Folder/FolderBreadcrumbBridgeRouterInFlightTests.cs | new | 256 | PASS |
| QuickFiler.Test/Viewers/BreadcrumbBridgeCoordinatorTests.cs | 398 | 460 | PASS |

The two production files are well under the limit. No file in the branch diff exceeds 500 lines.

## 7. Evidence Location Compliance — PASS

- All committed evidence for this feature is under
  `docs/features/active/2026-07-20-breadcrumb-suggestions-upgrade-race-398/evidence/<kind>/` (canonical).
- Branch-diff scan for `artifacts/baselines/`, `artifacts/qa/`, `artifacts/evidence/`,
  `artifacts/coverage/`: none present (`git diff --name-only cd6362f0..HEAD | grep -E 'artifacts/(baselines|qa|evidence|coverage)/'` returned no matches). Verdict: PASS.
- `artifacts/csharp/coverage.xml` is the coverage-gate tooling-input path (gitignored), not an
  evidence-output location; its presence is not an evidence-location violation.
- The repository has no `validate_evidence_locations.py` script; the scan was performed via `git diff`.

## 8. modified-workflow-needs-green-run

Not triggered. The branch diff modifies no path under `.github/workflows/**`, `.github/actions/**`, or
`scripts/benchmarks/**` (`git diff --name-only` confirmed no matches).

## Appendix A — Coverage Checklist

- TypeScript (`coverage/lcov.info`): no changed files in the branch diff — coverage not evaluated.
- Python (`artifacts/python/lcov.info`): no changed files in the branch diff — coverage not evaluated.
- PowerShell (`artifacts/pester/powershell-coverage.xml`): no changed files in the branch diff — coverage not evaluated.
- C# (`artifacts/csharp/coverage.xml`): changed files present — coverage PASS (repo-wide line 86.54% >= 85%,
  branch 80.85% >= 75%; new/changed-code line 100% >= 90%).

Baseline vs Post-change vs Disposition (C# instrumented first-party scope):
- Baseline: line 86.54%, branch 80.25%.
- Post-change: line 86.54%, branch 80.85%.
- Change: line +0.00 pt, branch +0.60 pt.
- Disposition: PASS. Both repository floors met by the canonical HEAD artifact; no regression on changed
  lines; new/changed-code line coverage 100%.
- Evidence: `artifacts/csharp/coverage.xml` (HEAD), `evidence/qa-gates/coverage-floor-verification.2026-07-20T22-30.md`.

## Appendix B — Command Reference (reviewer, check-only)

- `git merge-base HEAD origin/main` -> cd6362f0 (base confirmed current)
- `git diff --numstat cd6362f0..4412d2da` -> 2 production `.cs` + 6 test `.cs` + 1 `.csproj` + 37 `.md`
- `git diff --diff-filter=A --name-only cd6362f0..HEAD -- '*.cs'` -> 2 new test files only (no new production)
- `awk 'END{print NR}' <test>.cs` -> head line counts (all < 500)
- `git check-ignore artifacts/csharp/coverage.xml` -> ignored (tooling-input path)
- `. ./.claude/hooks/validate-feature-review-coverage.ps1; Get-JacocoRepoCoverage/Get-JacocoBranchCoverage` -> 86.54% / 80.85%
- `Get-ChangedLanguageSet` after summary correction -> CSharp enumerated
