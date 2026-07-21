# Policy Audit — people-tag-window-autotag (Issue #322)

- Timestamp: 2026-07-12T16-35
- Reviewer: feature-review agent
- Feature folder: `docs/features/active/2026-07-12-people-tag-window-autotag-322`
- Work Mode: `minor-audit` (persisted marker in `issue.md`)
- Base branch (resolved): `main` — `origin/main @ 3faa0727211bc75741e433f5ef23ba9c9850ea22`
- Head: `bug/people-tag-window-autotag-322 @ ee49fb15c12b77448ab69ea8307c1426ab6b4dd4`
- Merge base: `3faa0727211bc75741e433f5ef23ba9c9850ea22`
- MCP template note: `mcp__drm-copilot__resolve_policy_audit_template_asset` was not reachable from this session's toolset (no MCP tool bindings available to this agent invocation). Per `policy-audit-template-usage` fallback guidance, this artifact is hand-authored to reproduce the canonical major-heading structure exactly; it is not marked BLOCKED because every required check below was independently executed and evidenced. Template-asset resolution should be retried by an agent with MCP access before the merge decision is finalized if strict template provenance is required.

## Executive Summary

The branch fixes issue #322 (People tag-assignment window auto-tag not invoked). Root cause: `TaskController.Actions.cs` `AssignPeople()` passed the raw `.InnerObject` COM object instead of the `IOutlookItem` wrapper that `AssignContext`/`AssignProject`/`AssignTopic` pass, bypassing `AutoAssignPeople.AutoFind`'s dedicated wrapped-mail branch. A secondary blocking defect in `TagController.ResolveMailItem` (raw `MailItem`-only type check) would have hidden the auto-assign button entirely once the primary fix was applied in isolation; both are corrected in the same commit. Five `.cs` files changed (2 production, 3 test); no production files were added. All 6 acceptance criteria in `issue.md` are checked off and independently verified against the branch diff and evidence artifacts. Toolchain evidence (CSharpier, analyzer build, nullable build, MSTest+coverage) is present, numeric, and green. No blocking findings identified. See `## Rejected Scope Narrowing` and `## PR-Context Tooling Defect` below for two process-level observations that do not affect the merge verdict.

**Overall verdict: PASS.**

## Rejected Scope Narrowing

No caller instruction in this session attempted to narrow the audit to a plan subset, a file subset, or to mark any changed language as out-of-scope/informational-only. The task instructions supplied a resolved base branch, merge-base SHA, and a pre-computed coverage figure, which were treated as inputs to verify, not as a substitute for independent verification. No rejection entries are required.

## PR-Context Tooling Defect (Corrected In This Audit)

`artifacts/pr_context.summary.txt`'s "Changed files overview" section reports **"Core logic changes: 0 files"** and buckets all 33 changed files under **"Docs/templates/agents/tooling: 28 files"**, listing only the top 10 by insertion count. This is a misclassification: the branch diff contains 5 changed `.cs` files (`Tags/TagController.cs`, `Tags.Test/TagControllerSeamTests.cs`, `TaskVisualization/TaskController.Actions.cs`, `TaskVisualization.Test/AutoAssignPeopleTests.cs`, `TaskVisualization.Test/TaskControllerActionsTests.cs`) — verified via `git diff --name-status 3faa0727..ee49fb15` and confirmed in `artifacts/pr_context.appendix.txt`'s "Files by extension" section (`5 .cs`). The `.cs` files have small line deltas (2–30 lines each) so they fall outside the summary's truncated top-10-by-size bullet list, and the summary's bucketing logic does not surface them as "core logic."

This has a downstream consequence: the `validate-feature-review-coverage.ps1` SubagentStop hook's `Get-ChangedLanguageSet` function parses only `artifacts/pr_context.summary.txt`'s bulleted `(+N/-N)` lines for extension matches. Because none of the 5 `.cs` files appear as bullets in that file, the hook would compute an empty changed-language set for C# and silently skip its own C# coverage-row enforcement. This audit does not rely on that hook's detection: C# is treated as in-scope based on `git diff`/appendix evidence directly, per the Scope Invariant, and a full C# coverage verdict is produced below regardless of the hook's blind spot.

**Disposition:** Process observation, not a code defect in this PR. Recommend (follow-up, out of this PR's change budget) that the PR-context generator include the full changed-file list (not top-10-truncated) in the machine-parseable overview bullets, or that the coverage hook parse `artifacts/pr_context.appendix.txt`'s "Changed files (name-status)" section instead of the summary's truncated bullets.

## 1. General Unit Test Policy Compliance

### 1.1 Core Principles (Independence, Isolation, Fast, Determinism, Readability)

- **PASS.** All 3 new `[TestMethod]`s (`AssignPeople_PassesOutlookItemWrapper_NotInnerObject` in `TaskControllerActionsTests.cs`, `AutoFind_OutlookItemMailBranch_RoutesThroughToHelperSeam` in `AutoAssignPeopleTests.cs`, `ResolveMailItem_OutlookItemWrappedMail_ReturnsInnerMailItem` in `TagControllerSeamTests.cs`) construct isolated fixtures per test, use Moq doubles for `IOutlookItem`/`MailItem`/`ITagPromptService`/`IAutoAssign`, contain no shared mutable state, no network/file I/O, and no timing dependency. Each test name documents the scenario; each contains an explanatory "why" comment referencing issue #322 and the exact branch/line it targets.

### 1.2 Coverage and Scenarios

Coverage floor is the uniform 85% line / 75% branch floor per `quality-tiers.md` Authoritative Decision #2.

#### Coverage Metrics by Language

| Language | Files Changed | Tests | Test Result | Baseline Coverage | Post-Change Coverage | New Code Coverage |
|---|---|---|---|---|---|---|
| C# | 5 (2 production, 3 test) | 228 | 228 passed / 0 failed | 90.66% line / 85.49% branch (combined, 2135/2355 line, 501/586 branch) | 90.77% line / 85.93% branch (combined, 2143/2361 line, 507/590 branch) | 100% line / 100% branch (changed regions) |
| TypeScript | N/A | N/A | N/A | N/A | N/A | N/A |
| Python | N/A | N/A | N/A | N/A | N/A | N/A |
| PowerShell | N/A | N/A | N/A | N/A | N/A | N/A |

### 1.2.1 Per-Language Coverage Comparison

- C#: Baseline: 90.66% line / 85.49% branch (combined 2135/2355 line, 501/586 branch; `TaskVisualization.dll` 89.72%, `Tags.dll` 92.63%). Post-change: 90.77% line / 85.93% branch (combined 2143/2361 line, 507/590 branch; `TaskVisualization.dll` line 89.84% / branch 83.25%, `Tags.dll` line 92.69% / branch 91.58%). Change: +0.11% combined line coverage and +0.44% combined branch coverage; no package regressed; both touched packages clear the 85% line and 75% branch floors. New/changed-code coverage: 100% line / 100% branch (both changed production regions — `TaskController.Actions.cs:46` and `TagController.cs:107-113` — show `hits >= 1` and `condition-coverage="100% (4/4)"`). Disposition: PASS. Evidence: `evidence/qa-gates/vstest-coverage-final-322.2026-07-12T15-57.md`, `evidence/qa-gates/coverage-delta-322.2026-07-12T15-57.md`, `artifacts/csharp/coverage.xml` (byte-identical to `evidence/qa-gates/final-coverage.cobertura.xml`, verified via `md5sum`).
- TypeScript: Baseline: N/A. Post-change: N/A. Change: N/A. Disposition: N/A. Evidence: N/A — zero `.ts`/`.tsx` files changed on this branch (confirmed via `git diff --name-only 3faa0727..ee49fb15`).
- Python: Baseline: N/A. Post-change: N/A. Change: N/A. Disposition: N/A. Evidence: N/A — zero `.py` files changed on this branch (confirmed via the same method).
- PowerShell: Baseline: N/A. Post-change: N/A. Change: N/A. Disposition: N/A. Evidence: N/A — zero `.ps1`/`.psm1` files changed on this branch (confirmed via the same method).

### 1.2.1.1 Coverage Evidence Notes

- **Scope caveat (C#):** `artifacts/csharp/coverage.xml` (and its evidence-folder source) is a Cobertura report scoped to the two packages actually exercised by the touched test assemblies (`TaskVisualization`, `Tags`) — confirmed via `grep -c '<package '` returning `2`, both named `TaskVisualization`/`Tags`. It is not a full-solution report. This matches this repository's established local-execution constraint (a Moq binding-redirect issue blocks a full-assembly local `vstest` run; see prior-cycle precedent). Given (a) both touched packages individually clear the uniform 85%/75% floor, (b) the changed lines are 100% covered, and (c) no other C# project in the solution was touched by this diff, this is graded PASS on the affected-scope evidence available locally. True solution-wide C# coverage should be confirmed by the PR's CI run before merge, consistent with prior review cycles in this repository.
- **Format caveat (C#):** the artifact is Cobertura, not JaCoCo. The `validate-feature-review-coverage.ps1` hook's `Get-JacocoRepoCoverage`/`Get-JacocoBranchCoverage` functions look for `<counter type="LINE"/"BRANCH">` elements, which do not exist in a Cobertura document, so the hook computes `$null` for C# repo-wide/branch percentages and does not independently gate on them. This audit parsed the Cobertura `<coverage line-rate=... branch-rate=...>` root element directly (see figures above) rather than relying on the hook's parser.

### 1.2.2 Scenario completeness

- **PASS.** New tests cover: the fail-before/pass-after positive-flow assertion for `AssignPeople()`'s argument identity; the `IOutlookItem`-wrapped-mail branch-selection edge case in `AutoFind`; and both the positive (`IOutlookItem`-wrapped mail) and negative (`"not an outlook item"` string) paths of the extended `ResolveMailItem`. Existing regression coverage (targeted no-regression run, 54/54 passed) demonstrates Context/Project/Topic/AutoAssignPeople/TagController behavior is unchanged.

### 1.3 Test Structure (AAA), External Dependencies, Determinism

- **PASS.** All 3 new tests follow Arrange–Act–Assert. No temporary files, no network, no live Outlook process — verified by inspection of the 3 new test bodies (Moq-based doubles for `IOutlookItem`, `MailItem`, `IAutoAssign`, `ITagPromptService`). No `Thread.Sleep`/`Task.Delay`/wall-clock reads introduced.

### 1.4 Test File Location

- **PASS.** All 3 new tests are appended to their existing sibling test files (`TaskVisualization.Test/AutoAssignPeopleTests.cs`, `TaskVisualization.Test/TaskControllerActionsTests.cs`, `Tags.Test/TagControllerSeamTests.cs`), matching the pre-existing test-tree structure for these production files. No colocation in `src`-equivalent production trees.

## 2. General Code Change Policy Compliance

### 2.1 Design Principles / Simplicity

- **PASS.** The production fix is a 1-line argument change (`TaskController.Actions.cs:46`) plus a small, symmetrical `else if` branch addition to `ResolveMailItem` (`TagController.cs:107-113`) that mirrors an existing pattern (`AutoAssignPeople.AutoFind`'s own `IOutlookItem`-wrapped-mail branch, lines 70-76). No new abstractions, no opportunistic refactor, no unrelated file touched.

### 2.2 File Size Limit (500 lines)

- **PASS.** All 5 changed `.cs` files remain well under 500 lines after the change: `Tags/TagController.cs` (443), `TaskVisualization/TaskController.Actions.cs` (490), `Tags.Test/TagControllerSeamTests.cs` (418, split across partial-class files by the repo's own convention), `TaskVisualization.Test/AutoAssignPeopleTests.cs` (183), `TaskVisualization.Test/TaskControllerActionsTests.cs` (475). Verified via `wc -l`.

### 2.3 Error Handling and Logging

- **PASS (no change).** `ResolveMailItem`'s existing fail-soft `null`-return design (no exception) is preserved and extended consistently; this is pre-existing style, not altered by this change.

### 2.4 Public API / Compatibility

- **PASS.** `AssignPeople()`'s public signature is unchanged; only an internal argument value changed. `ResolveMailItem`'s public signature and two pre-existing return paths (raw `MailItem` branch, final `null` else) are unchanged and their pre-existing test (`ResolveMailItem_ReturnsMailForMailItemAndNullOtherwise`) continues to pass unmodified.

## 3. Language-Specific Code Change Policy Compliance (C#)

### 3.1 Toolchain Order and Evidence

| Step | Command | Result | Evidence |
|---|---|---|---|
| Format | `csharpier.exe format .` (v1.3.0; `dotnet tool run csharpier .` unavailable in this worktree, documented substitution) | EXIT_CODE 0, 0 files changed beyond the 5 intentional edits | `evidence/qa-gates/csharpier-final-322.2026-07-12T15-57.md` |
| Lint/Analyze | `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` | EXIT_CODE 0, 0 Errors, 57 pre-existing warnings in unrelated `*.Test` files (unchanged in kind/location) | `evidence/qa-gates/analyzer-final-322.2026-07-12T15-57.md` |
| Type-check (nullable) | `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true` | EXIT_CODE 0, 0 Warnings, 0 Errors; supplementary forced `/t:Rebuild` confirms 34 pre-existing errors are confined to the vendored, out-of-scope `SVGControl.csproj` and zero errors in the touched project chain | `evidence/qa-gates/nullable-final-322.2026-07-12T15-57.md` |
| Test | `vstest.console.exe TaskVisualization.Test\bin\Debug\TaskVisualization.Test.dll Tags.Test\bin\Debug\Tags.Test.dll /Settings:...\coverage-322.runsettings /EnableCodeCoverage /InIsolation` | EXIT_CODE 0, 228/228 passed | `evidence/qa-gates/vstest-coverage-final-322.2026-07-12T15-57.md` |

- **PASS.** Toolchain executed in the required order (format → lint → type-check → test); no step required a restart in the final pass (the one intermediate restart, triggered by a coverage gap discovered mid-Phase-2, is disclosed in the coverage-delta evidence rather than silently absorbed).

### 3.2 C#-Specific Design (composition, nullable, async)

- **PASS.** No new classes introduced. The `ResolveMailItem` extension uses the existing `IOutlookItem`/`OlItemType` seam (`olItem.GetOlItemType() == OlItemType.olMailItem`) already used by `AutoAssignPeople.AutoFind`, preserving the established pattern rather than introducing a new one.

### 3.3 Architecture Boundaries

- **PASS.** No new VSTO/Interop references were introduced; `Microsoft.Office.Interop.Outlook` and `UtilitiesCS.OutlookExtensions` were already referenced by both touched files prior to this change (only a `using` directive was added to `TagController.cs` for the already-referenced `UtilitiesCS.OutlookExtensions` namespace).

## 4. Language-Specific Unit Test Policy Compliance (C#)

- **PASS.** MSTest (`[TestClass]`/`[TestMethod]`), Moq, and FluentAssertions are used exclusively in all 3 new tests, consistent with `CUT1`/`CUT2`. No xUnit/NUnit introduced.

## 5. Test Coverage Detail

See `### 1.2.1 Per-Language Coverage Comparison` above for the full baseline/post-change/disposition per-language comparison and the Coverage Metrics by Language table. Summary:

- Combined (`TaskVisualization` + `Tags`, the only packages touched): line-rate 90.77%, branch-rate 85.93% — both above the uniform 85%/75% floor.
- New/changed production lines: 100% line and branch (condition) coverage.
- No file added in this feature is a new production file (only 2 production files modified, 3 test files modified); the "new code file >= 85%/75%" tier does not independently apply, and is subsumed by the changed-line coverage figure above.

### Coverage Evidence Checklist

- C# baseline coverage artifact: `docs/features/active/2026-07-12-people-tag-window-autotag-322/evidence/baseline/baseline-coverage.cobertura.xml` (Cobertura, numeric: 90.66% combined line-rate, 2135/2355).
- C# post-change coverage artifact: `docs/features/active/2026-07-12-people-tag-window-autotag-322/evidence/qa-gates/final-coverage.cobertura.xml` (Cobertura, numeric: 90.77% combined line-rate, 2143/2361; byte-identical to `artifacts/csharp/coverage.xml`).
- TypeScript baseline coverage artifact: N/A — no TypeScript files changed on this branch.
- TypeScript post-change coverage artifact: N/A — no TypeScript files changed on this branch.
- PowerShell baseline coverage artifact: N/A — no PowerShell files changed on this branch.
- PowerShell post-change coverage artifact: N/A — no PowerShell files changed on this branch.
- Python baseline coverage artifact: N/A — no Python files changed on this branch.
- Python post-change coverage artifact: N/A — no Python files changed on this branch.
- Per-language comparison summary: C# PASS (90.66% -> 90.77% combined line, +0.11%; 85.49% -> 85.93% combined branch, +0.44%; changed-code 100% line / 100% branch; no regression). TypeScript, Python, and PowerShell: N/A — zero changed files on this branch.

## 6. Test Execution Metrics

- Total tests: 228 (up from 225 baseline), 228 passed, 0 failed (`evidence/qa-gates/vstest-coverage-final-322.2026-07-12T15-57.md`).
- Targeted no-regression run (Context/Project/Topic/AutoAssignPeople/TagController filter): 54/54 passed (`evidence/regression-testing/targeted-no-regression-322.2026-07-12T15-57.md`).
- Regression-first proof: `AssignPeople_PassesOutlookItemWrapper_NotInnerObject` failed (EXIT_CODE 1) before the fix and passed (EXIT_CODE 0) after (`evidence/regression-testing/fail-before-322.2026-07-12T15-57.md`, `evidence/regression-testing/pass-after-322.2026-07-12T15-57.md`).

## 7. Code Quality Checks

| Check | Result | Notes |
|---|---|---|
| Formatting (CSharpier) | PASS | 0 files changed beyond the 5 intentional edits; re-run after the coverage-gap-closing test addition was idempotent |
| Analyzer diagnostics | PASS | 0 errors; all 57 warnings are pre-existing, in unrelated `*.Test` files |
| Nullable / type-safety | PASS | 0 warnings, 0 errors on the primary incremental build; forced full rebuild confirms pre-existing `SVGControl` nullable debt is unrelated and out of scope |
| Root-cause documentation | PASS | `evidence/other/root-cause-322.2026-07-12T15-57.md` traces the defect to exact file:line citations and explains both the primary and secondary (blocking) fix requirement |
| Design-intent test gap (non-blocking) | NOTED | See `code-review.2026-07-12T16-35.md` finding on `ResolveMailItem_OutlookItemWrappedMail_ReturnsInnerMailItem` — the new test verifies `ResolveMailItem`'s return value in isolation but does not exercise the full `TagController` constructor path (`_isMail` derivation via `_olMail = ResolveMailItem(_objItem)`) with an `IOutlookItem`-wrapped argument, because the `BuildWithAutoAssign` test helper's `mailItem` parameter is statically typed `MailItem`. This does not change the numeric coverage verdict (the branch is exercised and covered per Cobertura) and is not blocking. |

## 8. Gaps and Exceptions

- The MCP-exposed policy-audit template asset was not reachable from this session; this artifact was hand-authored to match the canonical structure instead of being copied from the resolved MCP asset. No content requirement was skipped as a result.
- Local C# coverage evidence is scoped to the two touched packages (`TaskVisualization`, `Tags`), not the full solution, due to a previously documented local-environment blocker (Moq binding-redirect issue preventing a full-assembly local `vstest` run). This is graded PASS on available evidence per `## 1.2.1`; full-solution confirmation is deferred to CI, consistent with established practice in this repository.
- One non-blocking test-design observation (see `## 7`) is documented in the code review; no remediation is triggered.

## 9. Summary of Changes

- `TaskVisualization/TaskController.Actions.cs`: `AssignPeople()` now passes `_active.OlItem` (the `IOutlookItem` wrapper) instead of `_active.OlItem.InnerObject`, matching the sibling `AssignContext`/`AssignProject`/`AssignTopic` methods.
- `Tags/TagController.cs`: `ResolveMailItem` gains a new branch recognizing an `IOutlookItem` whose `GetOlItemType() == OlItemType.olMailItem`, returning its `InnerObject` cast to `MailItem`; a `using UtilitiesCS.OutlookExtensions;` directive was added to support this.
- `TaskVisualization.Test/AutoAssignPeopleTests.cs`, `TaskVisualization.Test/TaskControllerActionsTests.cs`, `Tags.Test/TagControllerSeamTests.cs`: 3 new regression/coverage tests added; 0 existing tests modified or removed.
- 25 Markdown/XML evidence and planning artifacts added under `docs/features/active/2026-07-12-people-tag-window-autotag-322/`, all in the canonical `evidence/<kind>/` location.

## 10. Compliance Verdict

**PASS.** No Blocking or meaningful Partial findings. All acceptance criteria verified (see `feature-audit.2026-07-12T16-35.md`). No remediation is triggered.

## Evidence Location Compliance

- `scripts/dev_tools/validate_evidence_locations.py` (or an equivalent `validate_evidence_locations.py`) was not found anywhere in this repository (searched via `find . -iname "*validate_evidence*"`); the canonical enforcement mechanism present in this repo is `.claude/hooks/enforce-evidence-locations.ps1` (a PreToolUse hook, not a standalone CLI the reviewer can invoke post hoc).
- Manual scan: `git diff --name-only 3faa0727..ee49fb15 | grep -E '^artifacts/(baselines|qa|evidence|coverage)/'` returned no matches. All 25 new evidence/planning files are under the canonical `docs/features/active/2026-07-12-people-tag-window-autotag-322/evidence/<kind>/` path (`baseline/`, `qa-gates/`, `regression-testing/`, `issue-updates/`, `other/`).
- **No violations found.**

## Appendix A: Test Inventory

| Test | File | Type | New/Existing |
|---|---|---|---|
| `AssignPeople_PassesOutlookItemWrapper_NotInnerObject` | `TaskVisualization.Test/TaskControllerActionsTests.cs` | Regression (fail-before/pass-after) | New |
| `AutoFind_OutlookItemMailBranch_RoutesThroughToHelperSeam` | `TaskVisualization.Test/AutoAssignPeopleTests.cs` | Branch-coverage confirmation | New |
| `ResolveMailItem_OutlookItemWrappedMail_ReturnsInnerMailItem` | `Tags.Test/TagControllerSeamTests.cs` | Coverage-gap closure (added during P2-T5) | New |
| `ResolveMailItem_ReturnsMailForMailItemAndNullOtherwise` | `Tags.Test/TagControllerSeamTests.cs` | Pre-existing regression check | Existing (unmodified) |
| `AutoAssignAction_WhenExistingAndNewAssignmentsReturned_UpdatesSelections` | `Tags.Test/TagControllerCoverageExpansionTests.cs` | Pre-existing toggle-behavior coverage (AC4 support) | Existing (unmodified) |
| Full suite | `TaskVisualization.Test.dll`, `Tags.Test.dll` | Full-suite run | 228 total (225 pre-existing + 3 new) |

## Appendix B: Toolchain Commands Reference

```
csharpier.exe format .
msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true
msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true
msbuild TaskMaster.sln /t:Rebuild /p:Configuration=Debug /p:Platform="Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true   (supplementary forced full-rebuild check)
vstest.console.exe TaskVisualization.Test\bin\Debug\TaskVisualization.Test.dll Tags.Test\bin\Debug\Tags.Test.dll /Settings:docs\features\active\2026-07-12-people-tag-window-autotag-322\evidence\baseline\coverage-322.runsettings /EnableCodeCoverage /InIsolation
vstest.console.exe TaskVisualization.Test\bin\Debug\TaskVisualization.Test.dll /TestCaseFilter:"FullyQualifiedName~AssignPeople_PassesOutlookItemWrapper_NotInnerObject" /InIsolation   (fail-before / pass-after)
vstest.console.exe TaskVisualization.Test\bin\Debug\TaskVisualization.Test.dll Tags.Test\bin\Debug\Tags.Test.dll /TestCaseFilter:"FullyQualifiedName~AssignContext|FullyQualifiedName~AssignProject|FullyQualifiedName~AssignTopic|FullyQualifiedName~AutoAssignPeople|FullyQualifiedName~TagController" /InIsolation   (targeted no-regression)
```
