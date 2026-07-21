# Policy Compliance Audit — Issue #398 (breadcrumb-suggestions-upgrade-race)

- Timestamp: 2026-07-20T22-30
- Reviewer: feature-review
- Work Mode: minor-audit (from issue.md marker)
- Base branch (resolved): main @ cd6362f0264217d9ed94487f44c193df96eb1fa6 (merge-base; confirmed current — equals origin/main)
- Head: bug/breadcrumb-suggestions-upgrade-race-398 @ 1cb031f6ea2a7dfba2d035433208042a01f32fe6
- Scope: full branch diff cd6362f0..1cb031f6 (5 C# files, 15 Markdown files)

## Executive Summary

The fix is a targeted, well-designed bug remediation: `FolderBreadcrumbBridgeRouter.SetSuggestionsAsync`
now builds upgraded rows into a local list and swaps them into `BreadcrumbStateModel` atomically via a
new `ReplaceRows` seam, removing the mid-rebuild empty window that produced the reported
`ArgumentOutOfRangeException`. The executor toolchain evidence is green (CSharpier, .NET analyzers,
nullable build, and 5061/5061 MSTest all EXIT_CODE 0), and the documented new/changed-code coverage is
100%.

Two remediation-required policy findings prevent an unqualified PASS:

1. **File-size limit (FAIL).** Two changed test files exceed the repository 500-line limit
   (`.claude/rules/general-code-change.md` File Size Limit; CLAUDE.md General Code Change Policy §4).
   Test code is not exempt.
2. **C# coverage artifact (FAIL, procedural).** No valid HEAD-reflecting canonical C# coverage artifact
   exists. The file previously present at `artifacts/csharp/coverage.xml` was a stale, untracked
   leftover unrelated to this branch and has been removed (see Section 5). Coverage verification for a
   changed language is mandatory and cannot be satisfied from a valid machine artifact at HEAD; the
   substantive figures are supported only by the executor's narrative evidence.

Overall verdict: PARTIAL — substantively sound bug fix with two procedural/structural remediation items.

## 1. Policy Reading Order Applied

1. CLAUDE.md (all sections)
2. `.claude/rules/general-code-change.md`
3. `.claude/rules/general-unit-test.md`
4. C#: CLAUDE.md C# Code Change Policy + C# Unit Test Policy; `.claude/rules/quality-tiers.md`

## 2. Rejected Scope Narrowing

No scope-narrowing instructions were detected in the caller prompt. The caller supplied the full
branch diff scope and explicitly delegated scope determination to this reviewer. Scope was audited as
the full branch diff cd6362f0..1cb031f6.

## 3. PR-Context Integrity Correction

The PR-context summary overview originally reported "Core logic changes: 0 files" and classified all 20
changed files (including 5 `.cs` files) as "Docs/templates/agents/tooling." This is the recurring
C#-as-docs misclassification. The 5 C# files were enumerated from `git diff --numstat`:

| File | Adds/Dels | Type |
|---|---|---|
| UtilitiesCS/OutlookObjects/Folder/BreadcrumbStateModel.cs | +28/-1 | modified production |
| UtilitiesCS/OutlookObjects/Folder/FolderBreadcrumbBridgeRouter.cs | +15/-10 | modified production |
| UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbStateModelTests.cs | +62/-0 | modified test |
| UtilitiesCS.Test/OutlookObjects/Folder/FolderBreadcrumbBridgeRouterTests.cs | +119/-0 | modified test |
| QuickFiler.Test/Viewers/BreadcrumbBridgeCoordinatorTests.cs | +62/-0 | modified test |

`artifacts/pr_context.summary.txt` was corrected in place so the coverage gate enumerates C# rather
than silently skipping it. No new files were added (all 5 `.cs` files are modifications to pre-existing
files, confirmed via `git diff --diff-filter=A`).

## 4. Toolchain Verdicts (from executor qa-gate evidence)

| Stage | Command (executor) | EXIT_CODE | Verdict |
|---|---|---|---|
| Format | `csharpier format .` then `csharpier check .` | 0 | PASS |
| Analyzer | `msbuild TaskMaster.sln -t:Build ... -p:EnableNETAnalyzers=true -p:EnforceCodeStyleInBuild=true` | 0 | PASS |
| Nullable | `msbuild TaskMaster.sln -t:Build ... -p:Nullable=enable -p:TreatWarningsAsErrors=true` | 0 | PASS |
| Tests | `vstest.console.exe UtilitiesCS.Test... QuickFiler.Test... /Settings:cobertura.runsettings` | 0 | PASS (5061/5061) |

Evidence: `docs/features/active/2026-07-20-breadcrumb-suggestions-upgrade-race-398/evidence/qa-gates/*.2026-07-20T21-41.md`.
These verdicts rely on committed executor evidence; they were not independently re-executed by the
reviewer.

## 5. Coverage Verification (mandatory for every changed language)

### 5.1 C# — FAIL (procedural: no valid HEAD-reflecting canonical coverage artifact)

- Changed C# files: 2 modified production, 3 modified test (Section 3). C# coverage is mandatory.
- The file at the canonical path `artifacts/csharp/coverage.xml` was a stale, untracked leftover
  (mtime 2026-07-20 14:34, predating the source changes at 22:08; not part of the branch — `git status`
  reported it untracked/ignored). Parsed metrics before removal: repo-wide line 16.26% (9024/55511),
  branch 13.61% (1869/13736); the touched class `BreadcrumbStateModel` read line 29/67 (43%) with no
  coverage of the new `ReplaceRows` method; assemblies `Tags`, `TaskVisualization`, and `ToDoModel` read
  0% (their test projects were absent from that partial run). These metrics do not reflect HEAD and are
  unrepresentative. The stale leftover was removed to prevent a false gate result and to make the true
  state explicit: there is no valid HEAD C# coverage artifact.
- Coverage disposition: FAIL. Line coverage evidence at HEAD from a valid canonical artifact is absent;
  branch coverage evidence at HEAD from a valid canonical artifact is absent.
- Substantive evidence (executor narrative, not independently reproduced by the reviewer):
  - Baseline vs post-change instrumented scope (UtilitiesCS.dll + QuickFiler.dll): line 86.54% -> 86.54%,
    branch 80.25% -> 80.26%. No overall regression.
  - Post-change per touched file: FolderBreadcrumbBridgeRouter.cs line 97.55% / branch 90.00%;
    BreadcrumbStateModel.cs line 100.00% / branch 94.64%; BreadcrumbBridgeCoordinator.cs (unchanged)
    line 97.32% / branch 83.33%.
  - New/changed-code line coverage: 100% (>= 90% target). No regression on changed lines.
  - Source: `evidence/qa-gates/coverage-delta.2026-07-20T21-41.md` and
    `evidence/qa-gates/tests-coverage.2026-07-20T21-41.md`.
- Remediation: regenerate `artifacts/csharp/coverage.xml` at HEAD scoped to first-party instrumented
  packages (Cobertura -> JaCoCo conversion per the repo convention), or cite the PR CI coverage run URL
  once available. This is a procedural evidence-integrity item, not a code defect; the substantive
  targets appear met.

### 5.2 Other languages

- TypeScript: zero changed files. Not evaluated (no `.ts`/`.tsx` in diff).
- Python: zero changed files. Not evaluated (no `.py` in diff).
- PowerShell: zero changed files. Not evaluated (no `.ps1`/`.psm1` in diff).

## 6. File-Size Limit — FAIL

Two changed test files exceed the 500-line limit after the regression-test additions. Test code is not
exempt (exemptions are limited to throwaway scripts, raw text fixtures, and Markdown).

| File | Baseline lines | Head lines | Verdict |
|---|---|---|---|
| UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbStateModelTests.cs | 474 | 536 | FAIL (crossed 500) |
| UtilitiesCS.Test/OutlookObjects/Folder/FolderBreadcrumbBridgeRouterTests.cs | 426 | 545 | FAIL (crossed 500) |
| QuickFiler.Test/Viewers/BreadcrumbBridgeCoordinatorTests.cs | 398 | 460 | PASS (under 500) |

Remediation: split each over-limit test file into cohesive, scenario-grouped files (each < 500 lines).

## 7. Evidence Location Compliance

- All committed evidence for this feature is under
  `docs/features/active/2026-07-20-breadcrumb-suggestions-upgrade-race-398/evidence/<kind>/` (canonical).
- Branch-diff scan for `artifacts/baselines/`, `artifacts/qa/`, `artifacts/evidence/`,
  `artifacts/coverage/`: none present. Verdict: PASS.
- The removed `artifacts/csharp/coverage.xml` is the hook's tooling-input path (allowed), not an
  evidence-output location; its removal is unrelated to evidence-location compliance.

## 8. modified-workflow-needs-green-run

Not triggered. The branch diff modifies no path under `.github/workflows/**`, `scripts/benchmarks/**`,
or `.github/actions/**`.

## Appendix A — Coverage Checklist

- TypeScript (`coverage/lcov.info`): no changed files — not evaluated.
- Python (`artifacts/python/lcov.info`): no changed files — not evaluated.
- PowerShell (`artifacts/pester/powershell-coverage.xml`): no changed files — not evaluated.
- C# (`artifacts/csharp/coverage.xml`): changed files present — coverage FAIL (canonical HEAD artifact
  absent; stale leftover removed; regeneration required). Substantive executor evidence indicates
  new/changed-code coverage 100% and no regression.

Baseline vs Post-change vs Disposition (C# instrumented scope, executor evidence):
- Baseline: line 86.54%, branch 80.25%.
- Post-change: line 86.54%, branch 80.26%.
- Change: line +0.00 pt, branch +0.01 pt.
- Disposition: no regression on the instrumented scope; new/changed-code coverage 100%. Canonical
  HEAD artifact absent — regeneration required before merge (procedural remediation).

## Appendix B — Command Reference (reviewer, check-only)

- `git merge-base HEAD origin/main` -> cd6362f0 (base confirmed current)
- `git diff --numstat cd6362f0..HEAD` -> 5 `.cs` + 15 `.md`
- `git diff --diff-filter=A --name-only cd6362f0..HEAD | grep '\.cs$'` -> none (no new files)
- `git show cd6362f0:<test>.cs | awk 'END{print NR}'` and `awk 'END{print NR}' <test>.cs` -> baseline/head line counts
- JaCoCo aggregation of the (removed) stale `artifacts/csharp/coverage.xml` via Python ElementTree
- `grep EXIT_CODE evidence/qa-gates/*.md` -> all 0
