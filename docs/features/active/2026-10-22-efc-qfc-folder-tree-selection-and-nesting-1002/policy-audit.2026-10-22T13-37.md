# Policy Audit: EFC and QFC folder tree selection and nesting (Issue #1002)

- Branch: `bug/efc-qfc-folder-tree-selection-and-nesting-1002`
- Head: `b72ac1dbbfdb6b45d01a14dde9e91428a6bf41ac`
- Base: `main` (resolved `origin/main`) at merge base `8e4ad1ecbf50aae00fe2c092c22ae457f4feb7a7`
- Review timestamp: 2026-10-22T13-37 (re-audit after remediation cycle 1; prior review 2026-10-22T10-39)
- Work mode: `full-bug` (AC source: `spec.md` only)
- Reviewer: feature-review agent

## Template Provenance

The structure of this artifact follows the canonical major headings listed in `.claude/skills/policy-audit-template-usage/SKILL.md` step 5 (Executive Summary, sections 1 to 10, Appendix A and Appendix B), in the same shape as the cycle-0 audit `policy-audit.2026-10-22T10-39.md`; the template-asset selector step was not run from this agent session. The artifact is validated with `validate_orchestration_artifacts` (artifact type `policy-audit`).

## Rejected Scope Narrowing

No scope narrowing was detected in the caller prompt. The caller supplied the base branch, the merge base, the feature folder, the AC source, the canonical C# coverage artifact and the prior remediation inputs. `git merge-base main HEAD` and `git merge-base origin/main HEAD` both return `8e4ad1ecb`, equal to the supplied value, and the PR-context summary names head `b72ac1dbb`, equal to `git rev-parse HEAD`, so it is current. The audit scope is the full branch diff `8e4ad1ecb...b72ac1dbb` (105 files), not only the cycle-1 commit.

## Executive Summary

The branch fixes three QuickFiler defects (EFC keyed descendant tree with identity-addressed bridge messages; EFC trash gesture that preselects the trash row inside the bound document; QFC nested-row affordance order and per-depth indent). Remediation cycle 1 (commit `b72ac1dbb`) addressed all four cycle-0 blocking findings: B-1 and B-2 by new tests, B-3 by the maintainer-ratified member-level `[ExcludeFromCodeCoverage]` exemption, and B-4 by `.child[aria-selected="true"]` theme rules; it also applied the recommended CR-1 and CR-4 items.

Overall verdict: **REMEDIATION_REQUIRED (1 blocking finding: 1 autonomous)**.

- Toolchain: PASS (cycle-1 pass: format, format check, analyzer rebuild, warnings-as-errors rebuild, MSTest with coverage; 7597 of 7597 tests passed; reviewer read the trx counters).
- Repository C# coverage: PASS (86.30% line, 80.58% branch; floors 80% line and 75% branch from root CLAUDE.md).
- New and changed C# code: PASS (both new files 100% line; every instrumented changed line covered).
- Modified-file C# coverage: PASS (every modified production file at or above 80% line and 75% branch; reviewer recomputation).
- Cycle-0 findings B-1, B-2, B-3, B-4: resolved (section 8).
- New blocking finding R-1: spec AC23 states "no coverage exclusion is added", which the ratified B-3 exemption made false while the criterion remains checked; the spec text was not amended to record the maintainer decision.

## 1. General Unit Test Policy Compliance

### 1.1 Core principles

| Principle | Verdict | Evidence |
|---|---|---|
| Independence | PASS | Each cycle-1 test builds its own controller through `CreateIssue1002Controller`; no shared static state; message-constructor tests are pure. |
| Isolation | PASS | One behaviour per test (null viewer, null router, null rows, bind path; one constructor argument per null-guard test; one CSS rule per contract test). |
| Fast execution | PASS | No sleeps or delays; full suite completed in the coverage run. |
| Determinism | PASS | The four async bind tests await a `TaskCompletionSource` set by the host mock's `NavigateToString` callback; 0 `Thread.Sleep`, `Task.Delay` or temp-file APIs in added lines (reviewer scan). Code-review N-3 records that these waits have no failure exit if a bind faults. |
| Readability | PASS | Descriptive names; Arrange/Act/Assert comments; FluentAssertions `because` texts. |
| No temporary files | PASS | 0 `Path.GetTemp`, `GetTempFileName`, `File.Write` or `File.Create` in added lines. |

### 1.2 Coverage requirements

Coverage thresholds applied: the root `CLAUDE.md` states C# line `>= 80%` and branch `>= 75%`, and new modules, classes and methods `>= 90%`; these govern under the threshold precedence of `.claude/rules/general-unit-test.md`. The canonical artifact `artifacts/csharp/coverage.xml` (written 13:31Z) is byte-identical (`cmp`) to `coverage/coverage.cobertura.xml` from the cycle-1 run, whose trx (`coverage/test-results/mstest-coverage-run.trx`, 13:30Z) reports 7597 passed; every cycle-1 source edit predates the run (latest 13:27Z), and the commit `b72ac1dbb` (13:33Z) adds no further source change. Root attributes `lines-covered="57541" lines-valid="66677" branches-covered="14036" branches-valid="17419"` are the first-party totals. `quality-tiers.yml` is absent at the repository root and on `origin/main`, so tier classification is not a finding.

**Coverage Metrics by Language:**

| Language | Files Changed | Tests | Test Result | Baseline Coverage | Post-Change Coverage | New Code Coverage |
|---|---|---|---|---|---|---|
| C# | 12 production (2 new, 10 modified), 9 test, 4 project files | MSTest | 7597 passed, 0 failed | 86.10% | 86.30% | 100% |
| JavaScript | 1 production asset, 1 node test | node --test | 80 passed, 0 failed | 100% | 100% | 100% |
| TypeScript | 0 | none | none | N/A | N/A | N/A |
| Python | 0 | none | none | N/A | N/A | N/A |
| PowerShell | 0 | none | none | N/A | N/A | N/A |

### Coverage Evidence Checklist

- C# baseline coverage artifact: `evidence/baseline/baseline-coverage.2026-10-22T09-54.md` (first-party summary line; the post-processed baseline Cobertura was kept in the session scratchpad and re-read by the reviewer)
- C# post-change coverage artifact: `artifacts/csharp/coverage.xml` with summaries `evidence/qa-gates/remediation-tests.2026-10-22T13-32.md` and `evidence/qa-gates/coverage-comparison.2026-10-22T13-33.md`
- JavaScript baseline coverage artifact: `evidence/baseline/baseline-node-suites.2026-10-22T09-53.md`
- JavaScript post-change coverage artifact: `evidence/regression-testing/node-suites-run.2026-10-22T10-14.md` (cycle 1 did not change `FolderBreadcrumb.js`)
- TypeScript baseline coverage artifact: `N/A - out of scope`
- TypeScript post-change coverage artifact: `N/A - out of scope`
- PowerShell baseline coverage artifact: `N/A - out of scope`
- PowerShell post-change coverage artifact: `N/A - out of scope`
- Python baseline coverage artifact: `N/A - out of scope`
- Python post-change coverage artifact: `N/A - out of scope`
- Per-language comparison summary: section 1.2.1 of this document

### 1.2.1 Per-Language Coverage Comparison

- C#: Baseline: 86.10% lines (57272/66515) / 80.31% branches (13923/17337) -> Post-change: 86.30% lines (57541/66677) / 80.58% branches (14036/17419). Change: +0.20% lines, +0.27% branches. New/changed-code coverage: 100%. Disposition: PASS. Evidence: evidence/qa-gates/coverage-comparison.2026-10-22T13-33.md; artifacts/csharp/coverage.xml; reviewer per-file union over class line nodes (section 5).
- JavaScript: Baseline: 100% lines (FolderBreadcrumb.js) -> Post-change: 100% lines (FolderBreadcrumb.js). Change: 0.00% lines. New/changed-code coverage: 100%. Disposition: PASS. Evidence: evidence/baseline/baseline-node-suites.2026-10-22T09-53.md; evidence/regression-testing/node-suites-run.2026-10-22T10-14.md.
- TypeScript: Baseline: N/A. Post-change: N/A. Change: N/A. Disposition: N/A. Evidence: N/A - zero TypeScript files changed on this branch.
- Python: Baseline: N/A. Post-change: N/A. Change: N/A. Disposition: N/A. Evidence: N/A - zero Python files changed on this branch.
- PowerShell: Baseline: N/A. Post-change: N/A. Change: N/A. Disposition: N/A. Evidence: N/A - zero PowerShell files changed on this branch.

### 1.2.2 Coverage Verdicts

C# repo-wide coverage verdict: PASS (86.30% line at or above 80, 80.58% branch at or above 75; both rose against the baseline).

C# new-code coverage verdict: PASS (`BreadcrumbRow.Descendants.cs` 100.00% line 82/82, 97.22% branch 35/36; `BreadcrumbBridgeRouter.Descendants.cs` 100.00% line 63/63, 91.67% branch 11/12).

C# changed-line coverage verdict: PASS (every instrumented changed line in the ten modified production files is covered; reviewer `git diff -U0` new-side hunks intersected with Cobertura line nodes). One changed line is no longer instrumented: the formatter-reordered `public async Task ActionOkAsync()` signature, now inside a ratified exempt member; it carries no behaviour change.

C# modified-file coverage verdict: PASS (every modified production file at or above 80% line and 75% branch; `EfcFormController.Breadcrumb.cs` 92.08% / 80.95%, `EfcFormController.Actions.cs` 92.86% / 81.25%, `BreadcrumbMessages.cs` 100.00% / 100.00%; the two controller partials reach the floor with the ratified member-level exemption in force).

JavaScript coverage verdict: PASS (node `--experimental-test-coverage` over `QuickFiler/Resources/FolderBreadcrumb.js`: 100% line, branch and function before and after; unchanged by cycle 1). CI runs no node suite; the CI-visible contract for the script is the C# text-contract file `FolderBreadcrumbNestingIssue1002Tests.cs`.

### 1.3 Scenario completeness

PASS. Cycle 1 added the missing negative and edge scenarios named by the cycle-0 findings: null viewer, null router and null rows for `BindSourceFolderRows` and `BindFolderRows`; the null-viewer arm of `ApplyDeleteGestureAsync`; each null constructor argument of `BreadcrumbInboundMessage`, `BreadcrumbOutboundMessage`, `BreadcrumbRenderMessage` and `BreadcrumbSubfolderResultMessage`; and the light, dark and base CSS contracts for the descendant highlight and name truncation.

## 2. General Code Change Policy Compliance

| Rule | Verdict | Evidence |
|---|---|---|
| Design: simplicity and separation of concerns | PASS | Cycle 1 production change is limited to two CSS constant lines, one CSS rule split, five attributes with comments and two `using` directives. |
| Reuse | PASS | Cycle-1 tests reuse the existing `CreateIssue1002Controller`, `SetPrivateField` and `GetPrivateField` helpers. |
| Error handling | PASS | No production error path changed in cycle 1. |
| Logging | PASS | No change. |
| File size (500 lines) | PASS | Reviewer `awk` counts: largest test file 487 (`BreadcrumbBridgeRouterIssue1002Tests.cs`), largest production file 471 (`BreadcrumbBridgeRouter.cs`); cycle-1 files 313, 98, 343, 182, 206, 219. |
| Public contracts documented | PASS | No public signature changed in cycle 1 other than the formatter modifier reorder on `ActionOkAsync`. |
| Dependencies | PASS | None added. |
| Host-identifier hygiene in committed files | PASS | Added-line sweep for the account name, its 8.3 short form, the encoded scratchpad prefix, the user e-mail, and drive-rooted or `Program Files` / `AppData` paths: 0 hits (three GitHub issue URLs only). |
| Bugfix workflow (failing test first) | PASS | B-4 and CR-1 fail-before projection `regression-testing/css-highlight-fail-before.2026-10-22T13-27.md` (3 expected failures) precedes `css-highlight-pass-after.2026-10-22T13-28.md`. |
| Requirements traceability | FAIL | R-1: spec AC23 asserts "no coverage exclusion is added", but the diff adds five `[ExcludeFromCodeCoverage]` attributes; the maintainer ratification is recorded only in `remediation-inputs.2026-10-22T10-39.md`, and AC23 remains checked with unamended text. |

## 3. Language-Specific Code Change Policy Compliance

### 3.1 C# (`CLAUDE.md` C# Code Change Policy, `.claude/rules/csharp.md`)

| Rule | Verdict | Evidence |
|---|---|---|
| CSharpier formatting | PASS | `evidence/qa-gates/remediation-format.2026-10-22T13-32.md` (no tracked change on the last format run) and `remediation-format-check.2026-10-22T13-32.md` (`Checked 1675 files`, exit 0). |
| Analyzer rebuild | PASS | `evidence/qa-gates/remediation-analyzers.2026-10-22T13-32.md`: `/t:Rebuild`, 0 warnings, 0 errors. |
| Nullable rebuild (warnings as errors) | PASS | `evidence/qa-gates/remediation-typecheck.2026-10-22T13-32.md`: `/t:Rebuild`, 0 warnings, 0 errors, no `/p:Nullable=enable`; the new UtilitiesCS test file carries `#nullable enable`. |
| Coverage exemption form | PASS | Five member-level `[ExcludeFromCodeCoverage]` attributes on exactly the members the maintainer named (`ActionOkAsync`, `ActionCancelAsync`, `CreateFolderAsync`, `RefreshSuggestionsAsync`, `ConfigureBreadcrumbControl`); no class-level attribute; `git diff --stat 8e4ad1ecb...HEAD -- coverage.config` empty. The `Justification` property requested by the decision does not exist on the .NET Framework 4.8.1 attribute (CS0246 probe, `evidence/other/exclude-attribute-form.2026-10-22T13-25.md`), so each attribute carries an adjacent comment naming the dependency and #1002. CLAUDE.md UT2 names this mechanism; under `policy-compliance-order` CLAUDE.md precedes `.claude/rules/general-unit-test.md`, whose Blocking clause addresses coverage-config `exclude` entries. |
| `RefreshSuggestionsAsync` conditional scope | PASS | The decision admitted it only if uncovered after the B-1 tests; `evidence/qa-gates/b3-refresh-suggestions-decision.2026-10-22T13-24.md` records 0 of 9 lines covered after those tests. |
| Thread affinity (spec I6, AC19) | PASS | 0 `ConfigureAwait(false)` and 0 `lock (` in `QuickFiler/Controllers/BreadcrumbBridgeRouter*.cs` (reviewer grep). |
| Public API compatibility | PASS | No signature change in cycle 1. |
| Suppressions | PASS | 0 `#pragma` or `SuppressMessage` in added lines; the five `ExcludeFromCodeCoverage` lines are the ratified exemption above. |

### 3.2 JavaScript and CSS assets

| Rule | Verdict | Evidence |
|---|---|---|
| `FolderBreadcrumb.js` edit confined to `makeSubfolders` | PASS | Unchanged by cycle 1; cycle-0 evidence stands. |
| EFC CSS constants | PASS | `.child[aria-selected="true"]` added to `LightThemeCss` (`#cce8ff`) and `DarkThemeCss` (`#264f78`), matching the row selection colours; truncation moved to `.child .name`. |
| File size | PASS | `FolderBreadcrumb.js` 350 lines; `BreadcrumbDocumentAssets.cs` 182 lines. |

## 4. Language-Specific Unit Test Policy Compliance

| Rule | Verdict | Evidence |
|---|---|---|
| MSTest attributes | PASS | `[TestClass]` and `[TestMethod]` in all nine new C# test files, including `BreadcrumbMessagesIssue1002Tests.cs`. |
| Moq strict mocks | PASS | Cycle-1 controller tests use the strict host and provider mocks of `CreateIssue1002Controller`; the message tests need no mocks. |
| FluentAssertions | PASS | 0 MSTest `Assert.` calls in added lines. |
| Compile registration | PASS | `UtilitiesCS.Test.csproj` gains `Compile Include="OutlookObjects\Folder\BreadcrumbMessagesIssue1002Tests.cs"`. |
| Test location | PASS | Sibling `.Test` projects mirroring production folders, as CLAUDE.md and every existing C# test do. |

## 5. Test Coverage Detail

Per-file figures are the reviewer's union of `<class>/<lines>/<line>` nodes per `filename` in `artifacts/csharp/coverage.xml` (head) and the executor's baseline Cobertura, with changed lines from `git diff -U0 8e4ad1ecb...HEAD`; they reproduce the executor's `b1`, `b2`, `b3` and `coverage-comparison` figures exactly.

| File | Status | Baseline line / branch | Post-change line / branch | Gate |
|---|---|---|---|---|
| `UtilitiesCS/OutlookObjects/Folder/BreadcrumbRow.Descendants.cs` | new | absent | 100.00% (82/82) / 97.22% (35/36) | new code at least 90%: PASS |
| `QuickFiler/Controllers/BreadcrumbBridgeRouter.Descendants.cs` | new | absent | 100.00% (63/63) / 91.67% (11/12) | new code at least 90%: PASS |
| `UtilitiesCS/OutlookObjects/Folder/BreadcrumbRow.cs` | modified | 98.71% / 92.39% | 98.77% / 92.55% | PASS |
| `UtilitiesCS/OutlookObjects/Folder/BreadcrumbMessages.cs` | modified | 100.00% / 50.00% (7/14) | 100.00% / 100.00% (14/14) | PASS (B-2 resolved) |
| `UtilitiesCS/OutlookObjects/Folder/BreadcrumbMessageCodec.cs` | modified | 100.00% / 100.00% | 100.00% / 100.00% | PASS |
| `UtilitiesCS/OutlookObjects/Folder/BreadcrumbHtmlRenderer.cs` | modified | 96.99% / 90.00% | 97.77% / 93.10% | PASS |
| `UtilitiesCS/OutlookObjects/Folder/BreadcrumbDocumentAssets.cs` | modified | no executable lines | no executable lines | const strings only; no class node emitted |
| `QuickFiler/Controllers/BreadcrumbBridgeRouter.cs` | modified | 99.17% / 95.00% | 99.19% / 95.00% | PASS |
| `QuickFiler/Controllers/BreadcrumbBridgeRouter.Arrows.cs` | modified | 96.24% / 90.38% | 96.40% / 89.29% | PASS |
| `QuickFiler/Controllers/BreadcrumbBridgeRouter.Selection.cs` | modified | 98.04% / 96.15% | 98.37% / 95.59% | PASS |
| `QuickFiler/Controllers/EfcFormController.Actions.cs` | modified | 21.30% (23/108) / 28.12% (9/32) | 92.86% (26/28) / 81.25% (13/16) | PASS (B-3 resolved by ratified exemption) |
| `QuickFiler/Controllers/EfcFormController.Breadcrumb.cs` | modified | 67.83% (78/115) / 57.14% (24/42) | 92.08% (93/101) / 80.95% (34/42) | PASS (B-1 resolved: 93 covered lines and 34 covered branches exceed the baseline 78 and 24) |

Changed-line detail: instrumented changed lines covered per modified file: BreadcrumbRow.cs 8/8, BreadcrumbMessages.cs 3/3, BreadcrumbMessageCodec.cs 15/15, BreadcrumbHtmlRenderer.cs 49/49, BreadcrumbBridgeRouter.cs 5/5, .Arrows.cs 7/7, .Selection.cs 48/48, EfcFormController.Actions.cs 5/5, EfcFormController.Breadcrumb.cs 7/7. The `_formViewer == null` arm of `ApplyDeleteGestureAsync` (cycle-0 CR-4) is now exercised by `ApplyDeleteGestureAsync_NullViewer_RetainsTrashRowAndDoesNotBind`, raising the file's covered branches from 12 to 13.

Exemption consequence: the valid-line counts of the two controller partials fell (Actions 111 to 28; Breadcrumb 121 to 101) because the five attributed members, including their async state machines and lambda closures, are no longer instrumented; the Cobertura document holds one `QuickFiler.Controllers.EfcFormController` class node per file and no residual generated class for those members. Those members had 0 covered lines at baseline and at the cycle-0 head, so no previously covered line was removed from measurement.

## 6. Test Execution Metrics

- MSTest full suite with coverage (cycle-1 pass): 7597 total, 7597 passed, 0 failed; reviewer read `<Counters total="7597" executed="7597" passed="7597" failed="0" ...>` in `coverage/test-results/mstest-coverage-run.trx` (summary `evidence/qa-gates/remediation-tests.2026-10-22T13-32.md`).
- Cycle-1 tests: 18 new results (8 controller, 7 message-constructor, 3 CSS contract), each `outcome="Passed"` in that trx (reviewer grep by test name).
- Fail-before and pass-after for cycle 1: `evidence/regression-testing/css-highlight-fail-before.2026-10-22T13-27.md` (exit 1, 3 expected failures), `css-highlight-pass-after.2026-10-22T13-28.md`, `efc-bind-tests-pass.2026-10-22T13-22.md`, `breadcrumb-messages-null-guards-pass.2026-10-22T13-26.md`.
- Cycle-0 fail-before and pass-after projections for the three defects remain in `evidence/regression-testing/`.
- Node suites (local, not run by CI): 80 of 80 in each of three orders (`evidence/regression-testing/node-suites-run.2026-10-22T10-14.md`); cycle 1 changed no JavaScript.
- The reviewer did not re-run the suites; every coverage figure above was re-derived from the canonical artifact.

## 7. Code Quality Checks

| Check | Command | Result | Verdict |
|---|---|---|---|
| Confidentiality masking scan | `git diff 8e4ad1ecb...HEAD` added lines, case-insensitive match for the account name, its 8.3 short form, the encoded scratchpad prefix, the user e-mail, and a drive-letter, `Program Files` or `AppData` path regex | 0 hits (three GitHub issue URLs matched the drive-letter pattern and were inspected) | PASS |
| Suppression scan (added lines) | match for `SuppressMessage`, `#pragma`, `ExcludeFromCodeCoverage` in `*.cs` | 5 `[ExcludeFromCodeCoverage]`, all on the ratified members; 0 others | PASS |
| Workflow change scan | `git diff --name-only 8e4ad1ecb...HEAD` filtered for `.github/` | empty; rule `modified-workflow-needs-green-run` not triggered | PASS |
| Scope fence scan | `git diff --name-only` filtered for the spec's excluded files | empty | PASS |
| Raw evidence scan | `git diff --name-only` filtered for `.trx`, `.coverage`, `.xml`, `cobertura` | empty | PASS |
| Coverage configuration scan | `git diff --stat 8e4ad1ecb...HEAD -- coverage.config` | empty | PASS |

## Evidence Location Compliance

`git diff --name-only 8e4ad1ecb...HEAD` filtered for `^artifacts/(baselines|qa|evidence|coverage)/` is empty, so no file is committed under a non-canonical evidence path. All committed evidence is under `<FEATURE>/evidence/{baseline,qa-gates,regression-testing,remediation-baseline,other}/` as Markdown projections. The script `validate_evidence_locations.py` is not present in this checkout, so the check was performed by diff inspection. Verdict: PASS.

## 8. Gaps and Exceptions

Cycle-0 findings:

- B-1 (resolved): `EfcFormController.Breadcrumb.cs` 92.08% line / 80.95% branch; 93 covered lines and 34 covered branches against the required 84 and 24.
- B-2 (resolved): `BreadcrumbMessages.cs` 14/14 branches; lines 84, 85, 124, 144, 173, 174 and 175 each `condition-coverage` 100% (2/2).
- B-3 (resolved): maintainer decision recorded 2026-10-22T10:50Z (option a); five member-level attributes on exactly the named members; both controller partials above the floor; no `coverage.config` change.
- B-4 (resolved): two `aria-selected` rules in `BreadcrumbDocumentAssets.cs` (reviewer read); the renderer emits the nested `.children` container as a sibling after the `.child` node, so the highlight covers only the selected node's line.

New findings:

- R-1 (Blocking, autonomous): spec AC23 text ("no coverage exclusion is added") contradicts the ratified B-3 exemption; the criterion remains checked and its text was not amended, so the merged spec would record a criterion that the merged code does not meet as written.
- O-1 (non-blocking): the PR-context summary classifies every changed C# file as docs/tooling ("Core logic changes: 0 files"); language scope was derived from `git diff`.
- O-2 (non-blocking): the decision asked for a `Justification` on each attribute; the property does not exist on net48, and an adjacent comment was used instead (evidenced by the CS0246 probe).
- O-3 (non-blocking): the manual Outlook check remains pending human interaction by design (AC26); step 3 ("confirm it is highlighted") is now satisfiable by the B-4 rules.
- O-4 (non-blocking): skill layout conflicts recorded in cycle 0 (flat review-artifact names versus `audit/<ts>/` folders; plan authorship) are unchanged; this review follows the hook and the handoff skill.

## 9. Summary of Changes

- Cycle 0 (`e96e65302`): keyed descendant tree (`BreadcrumbRow.Descendants.cs`, `BreadcrumbRow.cs`), two inbound message types and `NodeId` (`BreadcrumbMessages.cs`, `BreadcrumbMessageCodec.cs`), recursive rendering and bridge script branches (`BreadcrumbHtmlRenderer.cs`, `BreadcrumbDocumentAssets.cs`), router descendant handlers, stale guards and preferred-row bind (`BreadcrumbBridgeRouter*.cs`), awaited trash-preselecting delete gesture (`EfcFormController.Actions.cs`, `.Breadcrumb.cs`), QFC `makeSubfolders` order and indent (`FolderBreadcrumb.js`); eight C# test files, one node suite.
- Cycle 1 (`b72ac1dbb`): 8 controller tests in `EfcFormControllerTests.Issue1002.cs`; new `BreadcrumbMessagesIssue1002Tests.cs` (7 tests) and its `Compile Include`; 3 CSS contract tests; `.child[aria-selected="true"]` theme rules and `.child .name` truncation rule; five ratified `[ExcludeFromCodeCoverage]` attributes; remediation evidence.

## 10. Compliance Verdict

**REMEDIATION_REQUIRED.** Toolchain, repository, new-code, changed-line and modified-file coverage, scope fences and evidence form pass, and all four cycle-0 blocking findings are resolved. One blocking finding remains: R-1, an autonomous spec amendment to AC23 recording the maintainer-ratified exemption.

## Appendix A: Test Inventory

| Test file | Tests | Status | Scope |
|---|---|---|---|
| `UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbMessageCodecIssue1002Tests.cs` | 8 results (5 methods) | new | codec contract |
| `UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbMessagesIssue1002Tests.cs` | 7 | new (cycle 1) | message constructor null guards |
| `UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbRowDescendantsIssue1002Tests.cs` | 24 | new | row tree state |
| `UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbHtmlRendererIssue1002Tests.cs` | 13 | new (3 added in cycle 1) | renderer, bridge script and CSS contract |
| `QuickFiler.Test/Controllers/BreadcrumbBridgeRouterIssue1002Tests.cs` | 11 | new | descendant toggle and activation |
| `QuickFiler.Test/Controllers/BreadcrumbBridgeRouterStaleCompletionIssue1002Tests.cs` | 7 | new | stale-completion guards |
| `QuickFiler.Test/Controllers/BreadcrumbBridgeRouterTrashIssue1002Tests.cs` | 7 | new | preferred-row bind |
| `QuickFiler.Test/Controllers/EfcFormControllerTests.Issue1002.cs` | 12 | new (8 added in cycle 1) | delete gesture, bind helpers |
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
| Diff scope | `git diff --numstat 8e4ad1ecb...HEAD`; `git merge-base main HEAD`; `git merge-base origin/main HEAD` | reviewer |
| Coverage parsing | Python `xml.etree.ElementTree` union of class line nodes per filename plus `git diff -U0` new-side hunks, over `artifacts/csharp/coverage.xml` and the executor baseline Cobertura; `cmp` of the canonical and run Cobertura | reviewer |
| Test outcome check | grep of `coverage/test-results/mstest-coverage-run.trx` for `<Counters` and each cycle-1 test name | reviewer |
| Scans | `git diff 8e4ad1ecb...HEAD` added-line filters for identity tokens and host paths, suppressions, sleeps, temp files, `ConfigureAwait(false)`; `grep` of router files for `lock (` | reviewer |
