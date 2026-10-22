# Policy Audit: EFC and QFC folder tree selection and nesting (Issue #1002)

- Branch: `bug/efc-qfc-folder-tree-selection-and-nesting-1002`
- Head: `54b36112abf0a4ac26384226694b02520842061b`
- Base: `main` (resolved `origin/main`) at merge base `8e4ad1ecbf50aae00fe2c092c22ae457f4feb7a7`
- Review timestamp: 2026-10-22T14-10 (re-audit after remediation cycle 2; prior reviews 2026-10-22T10-39 and 2026-10-22T13-37)
- Work mode: `full-bug` (AC source: `spec.md` only)
- Reviewer: feature-review agent

## Template Provenance

The structure of this artifact follows the canonical major headings listed in `.claude/skills/policy-audit-template-usage/SKILL.md` step 5 (Executive Summary, sections 1 to 10, Appendix A and Appendix B), in the same shape as the cycle-0 and cycle-1 audits `policy-audit.2026-10-22T10-39.md` and `policy-audit.2026-10-22T13-37.md`; the template-asset selector step was not run from this agent session. The artifact is validated with `validate_orchestration_artifacts` (artifact type `policy-audit`).

## Rejected Scope Narrowing

No scope narrowing was detected in the caller prompt. The caller supplied the base branch, the merge base, the feature folder, the AC source, the canonical C# coverage artifact and the prior remediation inputs. `git merge-base main HEAD` and `git merge-base origin/main HEAD` both return `8e4ad1ecb`, equal to the supplied value, and the PR-context summary names head `54b36112a`, equal to `git rev-parse HEAD`, so it is current. The audit scope is the full branch diff `8e4ad1ecb...54b36112a` (136 files), not only the cycle-2 commits.

## Executive Summary

The branch fixes three QuickFiler defects (EFC keyed descendant tree with identity-addressed bridge messages; EFC trash gesture that preselects the trash row inside the bound document; QFC nested-row affordance order and per-depth indent). Remediation cycle 2 (commits `b4b0364a3` to `54b36112a`) resolved the single cycle-1 blocking finding R-1 by amending the AC23 clause in `spec.md` and adding a dated `## Planner Amendments` entry, and applied the three recommended in-scope items N-1 (exemption comment), N-3 (fault exit for the delivered waits) and N-5 (name span in the bridge `subfolderResult` handler, narrowed by the orchestrator AC9 ruling).

Overall verdict: **PASS (0 blocking findings)**.

- Toolchain: PASS (cycle-2 pass: format, format check, analyzer rebuild, warnings-as-errors rebuild, MSTest with coverage; 7599 of 7599 tests passed; reviewer read the trx counters).
- Repository C# coverage: PASS (86.31% line, 80.59% branch; floors 80% line and 75% branch from root CLAUDE.md).
- New and changed C# code: PASS (both new files 100% line; every instrumented changed line covered).
- Modified-file C# coverage: PASS (every modified production file with instrumented lines at or above 80% line and 75% branch; reviewer recomputation).
- Cycle-1 finding R-1: resolved (section 8). AC23 now states the ratified exception and every clause is true of the head tree.

## 1. General Unit Test Policy Compliance

### 1.1 Core principles

| Principle | Verdict | Evidence |
|---|---|---|
| Independence | PASS | The cycle-2 tests build their own controller or read a constant; no shared static state. |
| Isolation | PASS | `BindFolderRows_FaultBeforeDelivery_FaultsDeliveredTaskPromptly` asserts one behaviour (pre-delivery fault fails the wait); `BridgeJs_SubfolderResultHandler_WrapsChildNameInNameSpan` asserts one script branch. |
| Fast execution | PASS | Trx durations 1.27 ms and 0.52 ms for the two cycle-2 tests. |
| Determinism | PASS | The new fault test observes `delivered.Task.IsFaulted` synchronously with no await, sleep, delay or timer; `CreateIssue1002Controller` now completes the `TaskCompletionSource` from `BoundaryErrorSink`, so the three existing delivered waits fault instead of hanging on a pre-delivery fault (cycle-1 N-3 resolved). 0 `Thread.Sleep`, `Task.Delay` or temp-file APIs in added lines. |
| Readability | PASS | Descriptive names; Arrange/Act/Assert comments; FluentAssertions `because` texts. |
| No temporary files | PASS | 0 `Path.GetTemp`, `GetTempFileName`, `File.Write` or `File.Create` in added lines. |

### 1.2 Coverage requirements

Coverage thresholds applied: the root `CLAUDE.md` states C# line `>= 80%` and branch `>= 75%`, and new modules, classes and methods `>= 90%`; these govern under the threshold precedence of `.claude/rules/general-unit-test.md`. The canonical artifact `artifacts/csharp/coverage.xml` (written 14:06:14Z) is the post-processed Cobertura of the cycle-2 run, whose trx (`coverage/test-results/mstest-coverage-run.trx`, 14:05:24Z) reports 7599 passed; every cycle-2 source edit predates the run (latest 14:00:53Z), and commit `54b36112a` (14:07:49Z) adds no further source change. Root attributes `lines-covered="57546" lines-valid="66677" branches-covered="14038" branches-valid="17419"` are the first-party totals. `quality-tiers.yml` is absent at the repository root and on `origin/main`, so tier classification is not a finding.

**Coverage Metrics by Language:**

| Language | Files Changed | Tests | Test Result | Baseline Coverage | Post-Change Coverage | New Code Coverage |
|---|---|---|---|---|---|---|
| C# | 12 production (2 new, 10 modified), 9 test, 4 project files | MSTest | 7599 passed, 0 failed | 86.10% | 86.31% | 100% |
| JavaScript | 1 production asset, 1 node test | node --test | 80 passed, 0 failed | 100% | 100% | 100% |
| TypeScript | 0 | none | none | N/A | N/A | N/A |
| Python | 0 | none | none | N/A | N/A | N/A |
| PowerShell | 0 | none | none | N/A | N/A | N/A |

### Coverage Evidence Checklist

- C# baseline coverage artifact: `evidence/baseline/baseline-coverage.2026-10-22T09-54.md` (branch baseline) and `evidence/remediation-baseline/cycle2-baseline-tests.2026-10-22T13-54.md` (cycle-2 pre-change)
- C# post-change coverage artifact: `artifacts/csharp/coverage.xml` with summaries `evidence/qa-gates/cycle2-tests.2026-10-22T14-06.md` and `evidence/qa-gates/cycle2-coverage-comparison.2026-10-22T14-06.md`
- JavaScript baseline coverage artifact: `evidence/baseline/baseline-node-suites.2026-10-22T09-53.md`
- JavaScript post-change coverage artifact: `evidence/regression-testing/node-suites-run.2026-10-22T10-14.md` (cycles 1 and 2 did not change `FolderBreadcrumb.js`)
- TypeScript baseline coverage artifact: `N/A - out of scope`
- TypeScript post-change coverage artifact: `N/A - out of scope`
- PowerShell baseline coverage artifact: `N/A - out of scope`
- PowerShell post-change coverage artifact: `N/A - out of scope`
- Python baseline coverage artifact: `N/A - out of scope`
- Python post-change coverage artifact: `N/A - out of scope`
- Per-language comparison summary: section 1.2.1 of this document

### 1.2.1 Per-Language Coverage Comparison

- C#: Baseline: 86.10% lines (57272/66515) / 80.31% branches (13923/17337) -> Post-change: 86.31% lines (57546/66677) / 80.59% branches (14038/17419). Change: +0.21% lines, +0.28% branches. New/changed-code coverage: 100%. Disposition: PASS. Evidence: evidence/qa-gates/cycle2-coverage-comparison.2026-10-22T14-06.md; artifacts/csharp/coverage.xml; reviewer per-file union over class line nodes (section 5).
- JavaScript: Baseline: 100% lines (FolderBreadcrumb.js) -> Post-change: 100% lines (FolderBreadcrumb.js). Change: 0.00% lines. New/changed-code coverage: 100%. Disposition: PASS. Evidence: evidence/baseline/baseline-node-suites.2026-10-22T09-53.md; evidence/regression-testing/node-suites-run.2026-10-22T10-14.md.
- TypeScript: Baseline: N/A. Post-change: N/A. Change: N/A. Disposition: N/A. Evidence: N/A - zero TypeScript files changed on this branch.
- Python: Baseline: N/A. Post-change: N/A. Change: N/A. Disposition: N/A. Evidence: N/A - zero Python files changed on this branch.
- PowerShell: Baseline: N/A. Post-change: N/A. Change: N/A. Disposition: N/A. Evidence: N/A - zero PowerShell files changed on this branch.

### 1.2.2 Coverage Verdicts

C# repo-wide coverage verdict: PASS (86.31% line at or above 80, 80.59% branch at or above 75; both rose against the branch baseline, and the cycle-2 run gained 4 covered lines against its own pre-change run with branches unchanged).

C# new-code coverage verdict: PASS (`BreadcrumbRow.Descendants.cs` 100.00% line 82/82, 97.22% branch 35/36; `BreadcrumbBridgeRouter.Descendants.cs` 100.00% line 63/63, 91.67% branch 11/12).

C# changed-line coverage verdict: PASS (every instrumented changed line in the ten modified production files is covered; reviewer `git diff -U0` new-side hunks intersected with Cobertura line nodes). The cycle-2 production edits are a comment line in `EfcFormController.Actions.cs` and string-constant text in `BreadcrumbDocumentAssets.cs`; neither adds an instrumented line.

C# modified-file coverage verdict: PASS (every modified production file with instrumented lines at or above 80% line and 75% branch; `EfcFormController.Breadcrumb.cs` 92.08% / 80.95%, `EfcFormController.Actions.cs` 92.86% / 81.25%, both unchanged from cycle 1; `BreadcrumbDocumentAssets.cs` holds only `const` strings and emits no class node).

JavaScript coverage verdict: PASS (node `--experimental-test-coverage` over `QuickFiler/Resources/FolderBreadcrumb.js`: 100% line, branch and function before and after; unchanged by cycles 1 and 2). CI runs no node suite; the CI-visible contract for the script is the C# text-contract file `FolderBreadcrumbNestingIssue1002Tests.cs`.

### 1.3 Scenario completeness

PASS. Cycle 2 adds the negative scenario named by cycle-1 N-3 (a bind that faults before delivery fails the caller's wait) and a text-contract test for the `subfolderResult` name span. Earlier cycles' positive, negative, edge and stale-completion scenarios are unchanged and pass.

## 2. General Code Change Policy Compliance

| Rule | Verdict | Evidence |
|---|---|---|
| Design: simplicity and separation of concerns | PASS | Cycle-2 production change is one comment line and a four-line replacement inside a JavaScript string constant. |
| Reuse | PASS | The fault exit is wired once in the shared `CreateIssue1002Controller` helper, so all three delivered waits gain it. |
| Error handling | PASS | No production error path changed in cycle 2. |
| Logging | PASS | No change. |
| File size (500 lines) | PASS | Reviewer `awk` counts over every changed `.cs`, `.js` and `.mjs` file: maximum 487 (`BreadcrumbBridgeRouterIssue1002Tests.cs`) and 471 (`BreadcrumbBridgeRouter.cs`); cycle-2 files 339, 368, 185, 206. |
| Public contracts documented | PASS | No public signature changed in cycle 2. |
| Dependencies | PASS | None added. |
| Host-identifier hygiene in committed files | PASS | Added-line sweep for the account name, its 8.3 short form, the encoded scratchpad prefix, the user e-mail, and drive-rooted or `Program Files` / `AppData` paths: the only hits are two prose lines of the cycle-1 audit that name the scan itself; no path or identity value. |
| Bugfix workflow (failing test first) | PASS | `regression-testing/cycle2-delivered-wait-fail-before.2026-10-22T13-58.md` and `cycle2-child-name-fail-before.2026-10-22T14-00.md` each record exactly the one new test failing on assertion before the fix, followed by the matching pass-after projections. |
| Requirements traceability | PASS | R-1 resolved: AC23 names the five ratified members and the maintainer decision, and `spec.md` carries a dated `## Planner Amendments` entry with authority and reason. Reviewer count of added `ExcludeFromCodeCoverage` lines in `*.cs` against the merge base: 5; `git diff --stat 8e4ad1ecb...HEAD -- coverage.config` is empty. |

## 3. Language-Specific Code Change Policy Compliance

### 3.1 C# (`CLAUDE.md` C# Code Change Policy, `.claude/rules/csharp.md`)

| Rule | Verdict | Evidence |
|---|---|---|
| CSharpier formatting | PASS | `evidence/qa-gates/cycle2-format.2026-10-22T14-06.md` (no tracked change beyond the plan file set) and `cycle2-format-check.2026-10-22T14-06.md` (`Checked 1675 files`, exit 0). |
| Analyzer rebuild | PASS | `evidence/qa-gates/cycle2-analyzers.2026-10-22T14-06.md`: `/t:Rebuild`, 0 warnings, 0 errors. |
| Nullable rebuild (warnings as errors) | PASS | `evidence/qa-gates/cycle2-typecheck.2026-10-22T14-06.md`: `/t:Rebuild`, 0 warnings, 0 errors, no `/p:Nullable=enable`. |
| Coverage exemption form | PASS | Five member-level `[ExcludeFromCodeCoverage]` attributes on exactly the maintainer-named members; no class-level attribute; no `coverage.config` change. The `ActionCancelAsync` comment now names the concrete `EfcViewer` dependency (`UiSyncContext` awaiter and `Close`) rather than `MessageBox` (cycle-1 N-1 resolved). CLAUDE.md UT2 names this mechanism; under `policy-compliance-order` CLAUDE.md precedes `.claude/rules/general-unit-test.md`, whose Blocking clause addresses coverage-config `exclude` entries. |
| Thread affinity (spec I6, AC19) | PASS | 0 `ConfigureAwait(false)` and 0 `lock (` in `QuickFiler/Controllers/BreadcrumbBridgeRouter*.cs` (reviewer grep; files unchanged in cycle 2). |
| Public API compatibility | PASS | No signature change in cycle 2. |
| Suppressions | PASS | 0 `#pragma` or `SuppressMessage` in added lines; the five `ExcludeFromCodeCoverage` lines are the ratified exemption above. |

### 3.2 JavaScript and CSS assets

| Rule | Verdict | Evidence |
|---|---|---|
| `FolderBreadcrumb.js` edit confined to `makeSubfolders` | PASS | Unchanged since cycle 0. |
| EFC bridge script (`BreadcrumbDocumentAssets.BridgeJs`) | PASS | The `subfolderResult` handler now appends a `span.name` holding the display name, so the cycle-1 `.child .name` truncation rule applies to the transient render. Every click branch resolves its target with `closest(...)`, so a click on the inner span still reaches the `.child` branches. |
| AC9 coordinator ruling | PASS | The key-less branch of `BreadcrumbHtmlRenderer` was not edited (no cycle-2 diff to that file); `RenderRowFragment_KeylessChildren_RenderUnchanged` passes with its assertions unchanged. |
| File size | PASS | `FolderBreadcrumb.js` 350 lines; `BreadcrumbDocumentAssets.cs` 185 lines. |

## 4. Language-Specific Unit Test Policy Compliance

| Rule | Verdict | Evidence |
|---|---|---|
| MSTest attributes | PASS | `[TestMethod]` on both cycle-2 tests inside existing `[TestClass]` types. |
| Moq strict mocks | PASS | The fault test uses the strict host and provider mocks of `CreateIssue1002Controller`; the text-contract test needs no mocks. |
| FluentAssertions | PASS | 0 MSTest `Assert.` calls in added lines. |
| Compile registration | PASS | No new file in cycle 2; earlier files registered. |
| C# 7.3 compatibility of `QuickFiler.Test` | PASS | No `#nullable enable` or nullable annotations added; builds clean. |
| Test location | PASS | Sibling `.Test` projects mirroring production folders, as CLAUDE.md and every existing C# test do. |

## 5. Test Coverage Detail

Per-file figures are the reviewer's union of `<class>/<lines>/<line>` nodes per `filename` in `artifacts/csharp/coverage.xml` (head), with changed lines from `git diff -U0 8e4ad1ecb...HEAD`; they reproduce the executor's `cycle2-coverage-comparison` figures exactly.

| File | Status | Baseline line / branch | Post-change line / branch | Gate |
|---|---|---|---|---|
| `UtilitiesCS/OutlookObjects/Folder/BreadcrumbRow.Descendants.cs` | new | absent | 100.00% (82/82) / 97.22% (35/36) | new code at least 90%: PASS |
| `QuickFiler/Controllers/BreadcrumbBridgeRouter.Descendants.cs` | new | absent | 100.00% (63/63) / 91.67% (11/12) | new code at least 90%: PASS |
| `UtilitiesCS/OutlookObjects/Folder/BreadcrumbRow.cs` | modified | 98.71% / 92.39% | 98.77% / 92.55% | PASS |
| `UtilitiesCS/OutlookObjects/Folder/BreadcrumbMessages.cs` | modified | 100.00% / 50.00% (7/14) | 100.00% / 100.00% (14/14) | PASS |
| `UtilitiesCS/OutlookObjects/Folder/BreadcrumbMessageCodec.cs` | modified | 100.00% / 100.00% | 100.00% / 100.00% | PASS |
| `UtilitiesCS/OutlookObjects/Folder/BreadcrumbHtmlRenderer.cs` | modified | 96.99% / 90.00% | 97.77% / 93.10% | PASS |
| `UtilitiesCS/OutlookObjects/Folder/BreadcrumbDocumentAssets.cs` | modified | no executable lines | no executable lines | const strings only; no class node emitted |
| `QuickFiler/Controllers/BreadcrumbBridgeRouter.cs` | modified | 99.17% / 95.00% | 99.19% / 95.00% | PASS |
| `QuickFiler/Controllers/BreadcrumbBridgeRouter.Arrows.cs` | modified | 96.24% / 90.38% | 96.40% / 89.29% | PASS |
| `QuickFiler/Controllers/BreadcrumbBridgeRouter.Selection.cs` | modified | 98.04% / 96.15% | 98.37% / 95.59% | PASS |
| `QuickFiler/Controllers/EfcFormController.Actions.cs` | modified | 21.30% (23/108) / 28.12% (9/32) | 92.86% (26/28) / 81.25% (13/16) | PASS (ratified member-level exemption in force) |
| `QuickFiler/Controllers/EfcFormController.Breadcrumb.cs` | modified | 67.83% (78/115) / 57.14% (24/42) | 92.08% (93/101) / 80.95% (34/42) | PASS |

Changed-line detail: instrumented changed lines covered per modified file: BreadcrumbRow.cs 8/8, BreadcrumbMessages.cs 3/3, BreadcrumbMessageCodec.cs 15/15, BreadcrumbHtmlRenderer.cs 49/49, BreadcrumbBridgeRouter.cs 5/5, .Arrows.cs 7/7, .Selection.cs 48/48, EfcFormController.Actions.cs 5/5, EfcFormController.Breadcrumb.cs 7/7.

## 6. Test Execution Metrics

- MSTest full suite with coverage (cycle-2 pass): 7599 total, 7599 passed, 0 failed; reviewer read `<Counters total="7599" executed="7599" passed="7599" failed="0" ...>` in `coverage/test-results/mstest-coverage-run.trx` (summary `evidence/qa-gates/cycle2-tests.2026-10-22T14-06.md`). Cycle-2 pre-change run: 7597 passed.
- Cycle-2 tests: `BindFolderRows_FaultBeforeDelivery_FaultsDeliveredTaskPromptly` and `BridgeJs_SubfolderResultHandler_WrapsChildNameInNameSpan`, each `outcome="Passed"` in that trx (reviewer grep by test name).
- Fail-before and pass-after for cycle 2: `evidence/regression-testing/cycle2-delivered-wait-fail-before.2026-10-22T13-58.md`, `cycle2-delivered-wait-pass-after.2026-10-22T13-59.md`, `cycle2-child-name-fail-before.2026-10-22T14-00.md`, `cycle2-child-name-pass-after.2026-10-22T14-01.md`.
- Node suites (local, not run by CI): 80 of 80 in each of three orders (`evidence/regression-testing/node-suites-run.2026-10-22T10-14.md`); no JavaScript asset changed since.
- The reviewer did not re-run the suites; every coverage figure above was re-derived from the canonical artifact.

## 7. Code Quality Checks

| Check | Command | Result | Verdict |
|---|---|---|---|
| Confidentiality masking scan | `git diff 8e4ad1ecb...HEAD` added lines, case-insensitive match for the account name, its 8.3 short form, the encoded scratchpad prefix, the user e-mail, and a drive-letter, `Program Files` or `AppData` path regex | 0 path or identity values (two prose lines of the cycle-1 audit naming the scan; GitHub issue URLs excluded) | PASS |
| Suppression scan (added lines) | match for `SuppressMessage`, `#pragma`, `ExcludeFromCodeCoverage` in `*.cs` | 5 `[ExcludeFromCodeCoverage]`, all on the ratified members; 0 others | PASS |
| Workflow change scan | `git diff --name-only 8e4ad1ecb...HEAD` filtered for `.github/` | empty; rule `modified-workflow-needs-green-run` not triggered | PASS |
| Scope fence scan | `git diff --name-only` filtered for the spec's excluded files | empty | PASS |
| Raw evidence scan | `git diff --name-only` filtered for `^artifacts/`, `.trx`, `.coverage`, `cobertura` | empty | PASS |
| Coverage configuration scan | `git diff --stat 8e4ad1ecb...HEAD -- coverage.config` | empty | PASS |

## Evidence Location Compliance

`git diff --name-only 8e4ad1ecb...HEAD` filtered for `^artifacts/(baselines|qa|evidence|coverage)/` is empty, so no file is committed under a non-canonical evidence path. All committed evidence is under `<FEATURE>/evidence/{baseline,qa-gates,regression-testing,remediation-baseline,other}/` as Markdown projections. The script `validate_evidence_locations.py` is not present in this checkout, so the check was performed by diff inspection. Verdict: PASS.

## 8. Gaps and Exceptions

Prior findings:

- B-1 to B-4 (cycle 0): remain resolved; per-file figures unchanged from cycle 1 (section 5).
- R-1 (cycle 1, resolved): AC23 clause replaced with the ratified-exception text; `## Planner Amendments` entry dated 2026-10-22 names the authority (maintainer decision 2026-10-22T10:50Z, B-3) and reason; AC22 and AC24 text unchanged; 5 added attributes; empty `coverage.config` stat.
- N-1 (cycle 1, resolved): comment names `UiSyncContext` and `Close`.
- N-3 (cycle 1, resolved): `BoundaryErrorSink` in `CreateIssue1002Controller` calls `TrySetException` on the delivered source.
- N-5 (cycle 1, resolved as narrowed): `subfolderResult` handler wraps the name; renderer key-less branch deliberately left unchanged under the AC9 ruling recorded in `remediation-plan.2026-10-22T13-43.md`.

Non-blocking observations:

- O-1: the PR-context summary classifies every changed C# file as docs/tooling; language scope was derived from `git diff`.
- O-2: net48 `ExcludeFromCodeCoverage` has no `Justification` property; adjacent comments are used.
- O-3: the manual Outlook check stays pending human interaction by design (AC26).
- O-4: remediation-plan task P4-T5 asserted an empty diff of `BreadcrumbHtmlRenderer.cs` against the merge base, which cannot hold because cycles 0 and 1 changed that file; the executor measured against `HEAD` instead and recorded the deviation in `self-review.2026-10-22T14-07.md`. The outcome is correct; the plan wording was imprecise.
- O-5: the cycle-2 self-review records that `docs/features/templates/policy_audit/` is absent in this repository; the executor used the cycle-1 checklists.

## 9. Summary of Changes

- Cycle 0 (`e96e65302`): keyed descendant tree, two inbound message types and `NodeId`, recursive rendering and bridge script branches, router descendant handlers, stale guards and preferred-row bind, awaited trash-preselecting delete gesture, QFC `makeSubfolders` order and indent; eight C# test files, one node suite.
- Cycle 1 (`b72ac1dbb`): controller and message-constructor tests, CSS contract tests, `.child[aria-selected="true"]` and `.child .name` rules, five ratified `[ExcludeFromCodeCoverage]` attributes.
- Cycle 2 (`54b36112a` and three docs commits): AC23 amendment and `## Planner Amendments` entry; `ActionCancelAsync` exemption comment; fault exit for the delivered waits plus one new test; `span.name` in the bridge `subfolderResult` handler plus one new test; cycle-2 evidence.

## 10. Compliance Verdict

**PASS.** Toolchain, repository, new-code, changed-line and modified-file coverage, scope fences, evidence form and requirements traceability pass. All prior blocking findings (B-1 to B-4, R-1) are resolved, and no new blocking finding was identified.

## Appendix A: Test Inventory

| Test file | Tests | Status | Scope |
|---|---|---|---|
| `UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbMessageCodecIssue1002Tests.cs` | 8 results (5 methods) | new | codec contract |
| `UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbMessagesIssue1002Tests.cs` | 7 | new | message constructor null guards |
| `UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbRowDescendantsIssue1002Tests.cs` | 24 | new | row tree state |
| `UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbHtmlRendererIssue1002Tests.cs` | 14 | new (1 added in cycle 2) | renderer, bridge script and CSS contract |
| `QuickFiler.Test/Controllers/BreadcrumbBridgeRouterIssue1002Tests.cs` | 11 | new | descendant toggle and activation |
| `QuickFiler.Test/Controllers/BreadcrumbBridgeRouterStaleCompletionIssue1002Tests.cs` | 7 | new | stale-completion guards |
| `QuickFiler.Test/Controllers/BreadcrumbBridgeRouterTrashIssue1002Tests.cs` | 7 | new | preferred-row bind |
| `QuickFiler.Test/Controllers/EfcFormControllerTests.Issue1002.cs` | 13 | new (1 added in cycle 2) | delete gesture, bind helpers, pre-delivery fault |
| `QuickFiler.Test/Viewers/FolderBreadcrumbNestingIssue1002Tests.cs` | 4 | new | QFC script text contract |
| `tests/quickfiler-breadcrumb/issue-1002-nesting.test.mjs` | 2 | new, local only | QFC DOM behaviour |

## Appendix B: Toolchain Commands Reference

| Stage | Command | Run by |
|---|---|---|
| C# format | `dotnet tool run csharpier format .` then `dotnet tool run csharpier check .` | executor |
| C# analyzers | `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` | executor |
| C# type check | `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true` | executor |
| C# test and coverage | `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot .` | executor |
| Node suites | `node --test --test-isolation=none --test-concurrency=1 --experimental-test-coverage` over the three breadcrumb suites | executor (cycle 0) |
| Diff scope | `git diff --stat 8e4ad1ecb...HEAD`; `git merge-base main HEAD`; `git merge-base origin/main HEAD` | reviewer |
| Coverage parsing | Python `xml.etree.ElementTree` union of class line nodes per filename plus `git diff -U0` new-side hunks, over `artifacts/csharp/coverage.xml` | reviewer |
| Test outcome check | grep of `coverage/test-results/mstest-coverage-run.trx` for `<Counters` and each cycle-2 test name | reviewer |
| Scans | `git diff 8e4ad1ecb...HEAD` added-line filters for identity tokens and host paths, suppressions, sleeps, temp files, `ConfigureAwait(false)`; `grep` of router files for `lock (` | reviewer |
