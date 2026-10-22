# Policy Audit: EFC and QFC folder tree selection and nesting (Issue #1002)

- Branch: `bug/efc-qfc-folder-tree-selection-and-nesting-1002`
- Head: `e96e65302de00d3b0f8174736c90a59f82525dc9`
- Base: `main` (resolved `origin/main`) at merge base `8e4ad1ecbf50aae00fe2c092c22ae457f4feb7a7`
- Review timestamp: 2026-10-22T10-39 (initial review; no prior remediation cycle)
- Work mode: `full-bug` (AC source: `spec.md` only)
- Reviewer: feature-review agent

## Template Provenance

The structure of this artifact follows the canonical major headings listed in `.claude/skills/policy-audit-template-usage/SKILL.md` step 5 (Executive Summary, sections 1 to 10, Appendix A and Appendix B); the template-asset selector step was not run from this agent session. The artifact is validated with `validate_orchestration_artifacts` (artifact type `policy-audit`).

## Rejected Scope Narrowing

No scope narrowing was detected in the caller prompt. The caller supplied the base branch, the merge base, the feature folder, the AC source and the canonical C# coverage artifact. `git merge-base main HEAD` returns `8e4ad1ecb`, equal to the supplied value, and the PR-context summary names head `e96e65302`, equal to `git rev-parse HEAD`, so it is current. The audit scope is the full branch diff `8e4ad1ecb...e96e65302` (75 files).

## Executive Summary

The branch fixes three QuickFiler defects: (1) EFC gains a keyed descendant tree, identity-addressed `descendantToggle` and `descendantActivate` bridge messages, recursive rendering and stale-completion guards; (2) the EFC trash gesture awaits a bind that preselects the trash row inside the delivered document; (3) the QFC `makeSubfolders` script appends the name before the toggle and indents each level by 16px.

Overall verdict: **REMEDIATION_REQUIRED (4 blocking findings: 3 autonomous, 1 human_decision_required)**.

- Toolchain: PASS (format, analyzer rebuild, warnings-as-errors rebuild, MSTest with coverage; loop pass 3 evidence, 7579 of 7579 tests passed).
- Repository C# coverage: PASS (86.14% line, 80.37% branch; floors 80% line and 75% branch from root CLAUDE.md).
- New and changed C# code: PASS (292 of 292 instrumented changed lines covered; both new files 100% line).
- Modified-file C# coverage: FAIL for three modified files below the governing floors (B-1, B-2, B-3), one of which was lowered by this change (B-1).
- Code review: one blocking functional gap (B-4: the selected EFC descendant has no visual highlight).
- AC status: 26 of 26 PASS.

## 1. General Unit Test Policy Compliance

### 1.1 Core principles

| Principle | Verdict | Evidence |
|---|---|---|
| Independence | PASS | Each test class builds its fixture in `[TestInitialize]` (`EfcDescendantRouterFixture`); no shared static state; no real `WebView2BreadcrumbHost` (0 references in new test files). |
| Isolation | PASS | One behaviour per test; codec, row, renderer, router, controller and asset-contract units are tested in separate files. |
| Fast execution | PASS | No sleeps or delays; full suite completed in the Koverage run. |
| Determinism | PASS | Ordering is driven by `TaskCompletionSource` barriers (5 in the stale-completion file); 0 `Thread.Sleep` and 0 `Task.Delay` in added lines. |
| Readability | PASS | Descriptive names from the spec Test Strategy; Arrange/Act/Assert comments; FluentAssertions `because` texts. |
| No temporary files | PASS | 0 `Path.GetTemp`, `GetTempFileName`, `File.Write` or `File.Create` in added lines. |

### 1.2 Coverage requirements

Coverage thresholds applied: the root `CLAUDE.md` states C# line `>= 80%` and branch `>= 75%`, and new modules, classes and methods `>= 90%`; these govern under the threshold precedence of `.claude/rules/general-unit-test.md`. The canonical artifact `artifacts/csharp/coverage.xml` is byte-identical (`cmp`) to the executor's final post-processed Cobertura and holds exactly nine first-party packages, so its root attributes are the first-party figures. `quality-tiers.yml` is absent at the repository root and on the base branch, so tier classification is not a finding.

**Coverage Metrics by Language:**

| Language | Files Changed | Tests | Test Result | Baseline Coverage | Post-Change Coverage | New Code Coverage |
|---|---|---|---|---|---|---|
| C# | 12 production (2 new, 10 modified), 8 test, 4 project files | MSTest | 7579 passed, 0 failed | 86.10% | 86.14% | 100% |
| JavaScript | 1 production asset, 1 node test | node --test | 80 passed, 0 failed | 100% | 100% | 100% |
| TypeScript | 0 | none | none | N/A | N/A | N/A |
| Python | 0 | none | none | N/A | N/A | N/A |
| PowerShell | 0 | none | none | N/A | N/A | N/A |

### Coverage Evidence Checklist

- C# baseline coverage artifact: `evidence/baseline/baseline-coverage.2026-10-22T09-54.md` (first-party summary line; the post-processed Cobertura was kept in the session scratchpad and re-read by the reviewer)
- C# post-change coverage artifact: `artifacts/csharp/coverage.xml` with summary `evidence/qa-gates/final-toolchain-tests.2026-10-22T10-26.md`
- JavaScript baseline coverage artifact: `evidence/baseline/baseline-node-suites.2026-10-22T09-53.md`
- JavaScript post-change coverage artifact: `evidence/regression-testing/node-suites-run.2026-10-22T10-14.md`
- TypeScript baseline coverage artifact: `N/A - out of scope`
- TypeScript post-change coverage artifact: `N/A - out of scope`
- PowerShell baseline coverage artifact: `N/A - out of scope`
- PowerShell post-change coverage artifact: `N/A - out of scope`
- Python baseline coverage artifact: `N/A - out of scope`
- Python post-change coverage artifact: `N/A - out of scope`
- Per-language comparison summary: section 1.2.1 of this document

### 1.2.1 Per-Language Coverage Comparison

- C#: Baseline: 86.10% lines (57272/66515) / 80.31% branches (13923/17337) -> Post-change: 86.14% lines (57523/66780) / 80.37% branches (14016/17439). Change: +0.04% lines, +0.06% branches. New/changed-code coverage: 100%. Disposition: FAIL. Evidence: evidence/qa-gates/coverage-comparison.2026-10-22T10-26.md; artifacts/csharp/coverage.xml; reviewer per-file union over class line nodes (section 5).
- JavaScript: Baseline: 100% lines (FolderBreadcrumb.js) -> Post-change: 100% lines (FolderBreadcrumb.js). Change: 0.00% lines. New/changed-code coverage: 100%. Disposition: PASS. Evidence: evidence/baseline/baseline-node-suites.2026-10-22T09-53.md; evidence/regression-testing/node-suites-run.2026-10-22T10-14.md.
- TypeScript: Baseline: N/A. Post-change: N/A. Change: N/A. Disposition: N/A. Evidence: N/A - zero TypeScript files changed on this branch.
- Python: Baseline: N/A. Post-change: N/A. Change: N/A. Disposition: N/A. Evidence: N/A - zero Python files changed on this branch.
- PowerShell: Baseline: N/A. Post-change: N/A. Change: N/A. Disposition: N/A. Evidence: N/A - zero PowerShell files changed on this branch.

### 1.2.2 Coverage Verdicts

C# repo-wide coverage verdict: PASS (86.14% line at or above 80, 80.37% branch at or above 75; both rose against the baseline).

C# new-code coverage verdict: PASS (`BreadcrumbRow.Descendants.cs` 100.00% line 82/82, 97.22% branch 35/36; `BreadcrumbBridgeRouter.Descendants.cs` 100.00% line 63/63, 91.67% branch 11/12; every new method at least 90% line).

C# changed-line coverage verdict: PASS (292 of 292 instrumented changed lines covered across the ten modified production files; no changed line lost coverage).

C# modified-file coverage verdict: FAIL (three modified files are below the 80% line or 75% branch floor: `EfcFormController.Breadcrumb.cs` 64.46% line / 52.38% branch, lowered from 67.83% / 57.14% by this change (B-1); `BreadcrumbMessages.cs` 100.00% line / 50.00% branch, unchanged from baseline (B-2); `EfcFormController.Actions.cs` 23.42% line / 33.33% branch, raised from 21.30% / 28.12% (B-3)).

JavaScript coverage verdict: PASS (node `--experimental-test-coverage` over `QuickFiler/Resources/FolderBreadcrumb.js`: 100% line, branch and function before and after, in normal, reversed and shuffled suite order). CI runs no node suite; the CI-visible contract for the script is the C# text-contract file `FolderBreadcrumbNestingIssue1002Tests.cs`.

### 1.3 Scenario completeness

PASS. Positive (toggle, activate, preferred trash row), negative (unknown row, unknown node, cross-row node, leaf toggle, empty or non-string `nodeId`), edge (out-of-root and archive-root activation, preferred row absent, host core not initialized, null router), error (provider fault, cancellation, bind fault at the controller boundary), concurrency (collapse, rebind and reverse completion during an awaited fetch) and state (generation counter, reset points) scenarios are each pinned by a named test.

## 2. General Code Change Policy Compliance

| Rule | Verdict | Evidence |
|---|---|---|
| Design: simplicity and separation of concerns | PASS | Tree state lives on `BreadcrumbRow` (pure model); I/O-bound fetch and guards live in a new router partial; rendering stays in the renderer. |
| Reuse | PASS | `BreadcrumbDescendantNode` reused unchanged; `TryGetRowSelection` extracted and shared by `SelectRow` and `SelectPreferredRow`. |
| Error handling | PASS | Rejected identities, stale completions, faults and cancellation are logged and leave state unchanged; `OnHostMessageReceived` still catches only `BreadcrumbMessageException`. |
| Logging | PASS | Existing `log.Error` router pattern. |
| File size (500 lines) | PASS | Largest changed source file `BreadcrumbBridgeRouter.cs` 471 lines; largest new test file 487 lines; `.csproj` files are project files. |
| Public contracts documented | PASS | XML documentation on every new public member and the new optional parameters. |
| Dependencies | PASS | None added. |
| Host-identifier hygiene in committed files | PASS | Added-line sweep for the account name, its 8.3 short form, the encoded scratchpad prefix, the user e-mail and drive-rooted user or program paths: 0 hits. |
| Bugfix workflow (failing test first) | PASS | Fail-before projections for all three defects (`efc-descendants-fail-before`, `efc-trash-fail-before`, `qfc-nesting-fail-before`) precede the pass-after projections. |
| Functional completeness of the fix | FAIL | B-4: the selected EFC descendant carries `aria-selected="true"` but no stylesheet rule renders it, so the user cannot see which subfolder is the filing destination (see code review). |

## 3. Language-Specific Code Change Policy Compliance

### 3.1 C# (`CLAUDE.md` C# Code Change Policy, `.claude/rules/csharp.md`)

| Rule | Verdict | Evidence |
|---|---|---|
| CSharpier formatting | PASS | `evidence/qa-gates/final-toolchain-format.2026-10-22T10-23.md`: `csharpier check .` exit 0, 0 files changed by the format pass. |
| Analyzer rebuild | PASS | `evidence/qa-gates/final-toolchain-analyzers.2026-10-22T10-23.md`: 0 errors, 0 warnings. |
| Nullable rebuild (warnings as errors) | PASS | `evidence/qa-gates/final-toolchain-typecheck.2026-10-22T10-23.md`: 0 errors, 0 warnings; no `/p:Nullable=enable`; new files carry `#nullable enable`. |
| Null-forgiving operator justified | PASS | `key!` in `BreadcrumbBridgeRouter.Descendants.cs` carries a comment explaining that net48 lacks `NotNullWhen`. |
| Thread affinity (spec I6, AC19) | PASS | 0 `ConfigureAwait(false)` and 0 `lock (` in `QuickFiler/Controllers/BreadcrumbBridgeRouter*.cs` (reviewer grep). |
| Public API compatibility | PASS | New parameters are optional (`nodeId`, `preferredRowText`); existing callers compile unchanged. |
| Suppressions | PASS | 0 `#pragma`, `SuppressMessage` or `ExcludeFromCodeCoverage` in added lines. |

### 3.2 JavaScript (`QuickFiler/Resources/FolderBreadcrumb.js`)

| Rule | Verdict | Evidence |
|---|---|---|
| Edit confined to `makeSubfolders` | PASS | Diff touches only `makeSubfolders`; pinned strings preserved (FolderBreadcrumbAssetContractTests 15 of 15, FolderBreadcrumbRecursiveAssetIssue997Tests 3 of 3). |
| File size | PASS | 350 lines. |

## 4. Language-Specific Unit Test Policy Compliance

| Rule | Verdict | Evidence |
|---|---|---|
| MSTest attributes | PASS | `[TestClass]` and `[TestMethod]` in all eight new C# test files. |
| Moq strict mocks | PASS | Every `new Mock<` in new files uses `MockBehavior.Strict` (6 of 6); router suites share the strict fixture. |
| FluentAssertions | PASS | 221 `Should()` assertions; 0 MSTest `Assert.` calls. |
| Compile registration | PASS | Explicit `Compile Include` for all new source and test files in the four legacy project files. |
| Test location | PASS | Sibling `.Test` projects mirroring production folders, as CLAUDE.md and every existing C# test do; node suite under `tests/quickfiler-breadcrumb/`. |

## 5. Test Coverage Detail

Per-file figures are the reviewer's union of `<class>/<lines>/<line>` nodes per `filename` in `artifacts/csharp/coverage.xml` (head) and the executor's baseline Cobertura; they reproduce the executor's figures exactly.

| File | Status | Baseline line / branch | Post-change line / branch | Gate |
|---|---|---|---|---|
| `UtilitiesCS/OutlookObjects/Folder/BreadcrumbRow.Descendants.cs` | new | absent | 100.00% (82/82) / 97.22% (35/36) | new code at least 90%: PASS |
| `QuickFiler/Controllers/BreadcrumbBridgeRouter.Descendants.cs` | new | absent | 100.00% (63/63) / 91.67% (11/12) | new code at least 90%: PASS |
| `UtilitiesCS/OutlookObjects/Folder/BreadcrumbRow.cs` | modified | 98.71% / 92.39% | 98.77% / 92.55% | PASS |
| `UtilitiesCS/OutlookObjects/Folder/BreadcrumbMessages.cs` | modified | 100.00% / 50.00% (7/14) | 100.00% / 50.00% (7/14) | branch below 75%: FAIL (B-2) |
| `UtilitiesCS/OutlookObjects/Folder/BreadcrumbMessageCodec.cs` | modified | 100.00% / 100.00% | 100.00% / 100.00% | PASS |
| `UtilitiesCS/OutlookObjects/Folder/BreadcrumbHtmlRenderer.cs` | modified | 96.99% / 90.00% | 97.77% / 93.10% | PASS |
| `UtilitiesCS/OutlookObjects/Folder/BreadcrumbDocumentAssets.cs` | modified | no executable lines | no executable lines | const strings only; no class node emitted |
| `QuickFiler/Controllers/BreadcrumbBridgeRouter.cs` | modified | 99.17% / 95.00% | 99.19% / 95.00% | PASS |
| `QuickFiler/Controllers/BreadcrumbBridgeRouter.Arrows.cs` | modified | 96.24% / 90.38% | 96.40% / 89.29% | PASS |
| `QuickFiler/Controllers/BreadcrumbBridgeRouter.Selection.cs` | modified | 98.04% / 96.15% | 98.37% / 95.59% | PASS |
| `QuickFiler/Controllers/EfcFormController.Actions.cs` | modified | 21.30% (23/108) / 28.12% (9/32) | 23.42% (26/111) / 33.33% (12/36) | below 80% / 75%: FAIL (B-3) |
| `QuickFiler/Controllers/EfcFormController.Breadcrumb.cs` | modified | 67.83% (78/115) / 57.14% (24/42) | 64.46% (78/121) / 52.38% (22/42) | below floor and lowered: FAIL (B-1, B-3) |

Changed-line detail: every instrumented added line in the ten modified files is covered (BreadcrumbRow.cs 8/8, BreadcrumbMessages.cs 3/3, BreadcrumbMessageCodec.cs 15/15, BreadcrumbHtmlRenderer.cs 49/49, BreadcrumbBridgeRouter.cs 5/5, .Arrows.cs 7/7, .Selection.cs 48/48, EfcFormController.Actions.cs 5/5, EfcFormController.Breadcrumb.cs 7/7). The only partially covered new branch is line 102 of `EfcFormController.Actions.cs` (3 of 4 conditions; the `_formViewer == null` arm is unreached).

The `EfcFormController.Breadcrumb.cs` drop is caused by the diff: the delete gesture previously reached `BindFolderRows` (6 of its 7 lines covered by `ActionDeleteAsync_AwaitedTwice_LeavesExactlyOneTrashRowInFolderRows`); it now awaits `BindBreadcrumbRowsAsync` directly, and the remaining caller `BindSourceFolderRows` has no test. Covered branches fell from 24 to 22.

## 6. Test Execution Metrics

- MSTest full suite with coverage (executor loop pass 3): 7579 total, 7579 passed, 0 failed (`evidence/qa-gates/final-toolchain-tests.2026-10-22T10-26.md`).
- New test classes: 75 results, 0 not passed; kept-green existing classes listed in Test Strategy: 0 failures (same evidence).
- Fail-before runs: `evidence/regression-testing/efc-descendants-fail-before.2026-10-22T10-04.md`, `efc-trash-fail-before.2026-10-22T10-10.md`, `qfc-nesting-fail-before.2026-10-22T10-13.md` (expected exit 1).
- Pass-after runs: `efc-descendants-pass-after.2026-10-22T10-08.md`, `efc-trash-pass-after.2026-10-22T10-12.md`, `qfc-nesting-pass-after.2026-10-22T10-14.md` (exit 0).
- Node suites (local, not run by CI): 80 of 80 in each of three orders (`evidence/regression-testing/node-suites-run.2026-10-22T10-14.md`).
- Reviewer did not re-run the suites; the reviewer re-derived every coverage figure from the canonical artifact, which is byte-identical to the executor's final Cobertura.

## 7. Code Quality Checks

| Check | Command | Result | Verdict |
|---|---|---|---|
| Confidentiality masking scan | `git diff 8e4ad1ecb...HEAD` added lines, case-insensitive match for the account name, its 8.3 short form, the encoded scratchpad prefix, the user e-mail and a drive-rooted user or program path regex | 0 hits | PASS |
| Suppression scan (added lines) | match for `SuppressMessage`, `#pragma`, `ExcludeFromCodeCoverage` in code | 0 in code (document mentions only) | PASS |
| Workflow change scan | `git diff --name-only 8e4ad1ecb...HEAD` filtered for `.github/workflows`, `.github/actions`, `scripts/benchmarks` | empty; rule `modified-workflow-needs-green-run` not triggered | PASS |
| Scope fence scan | `git diff --name-only` filtered for the spec's excluded files | empty | PASS |
| Raw evidence scan | `git diff --name-only` filtered for `.trx`, `.coverage`, `.xml`, `cobertura` | empty | PASS |

## Evidence Location Compliance

`git diff --name-only 8e4ad1ecb...HEAD` filtered for `^artifacts/(baselines|qa|evidence|coverage)/` is empty, so no file is committed under a non-canonical evidence path. All committed evidence is under `<FEATURE>/evidence/{baseline,qa-gates,regression-testing,other}/` as Markdown projections. The script `validate_evidence_locations.py` is not present in this checkout, so the check was performed by diff inspection. Verdict: PASS.

## 8. Gaps and Exceptions

- B-1 (Blocking, autonomous): `EfcFormController.Breadcrumb.cs` coverage lowered by the change and below the floor.
- B-2 (Blocking, autonomous): `BreadcrumbMessages.cs` branch coverage 50.00%, below the 75% floor; the uncovered branches are the null-argument guards of four constructors, including the constructor this diff modified.
- B-3 (Blocking, human_decision_required): pre-existing sub-floor residue in `EfcFormController.Actions.cs` and `EfcFormController.Breadcrumb.cs` sits in `MessageBox.Show`-bound action handlers and WebView2 wiring; reaching the floor needs either new seams outside the bug scope or the maintainer-ratified COM/VSTO/WinForms exemption in CLAUDE.md UT2.
- B-4 (Blocking, autonomous): no visual highlight for the selected EFC descendant.
- O-1 (non-blocking): the PR-context summary classifies every changed C# file as docs/tooling ("Core logic changes: 0 files"); language scope was derived from `git diff`.
- O-2 (non-blocking): `remediation-handoff-atomic-planner` prescribes `audit/<ts>/` and `remediation/<ts>/` folders, while the review-artifact hook requires the flat `<stem>.<timestamp>.md` form used here; `feature-review-workflow` step 8 asks the reviewer to create the plan file, while the handoff skill assigns plan authorship to `atomic-planner`. This review follows the hook and the handoff skill.
- O-3 (non-blocking): the manual Outlook check is pending human interaction by design (AC26).

## 9. Summary of Changes

- `UtilitiesCS/OutlookObjects/Folder/BreadcrumbRow.Descendants.cs` (new) and `BreadcrumbRow.cs`: keyed descendant tree, generation-checked attach, reset points.
- `BreadcrumbMessages.cs`, `BreadcrumbMessageCodec.cs`: two inbound types and an optional `NodeId`.
- `BreadcrumbHtmlRenderer.cs`, `BreadcrumbDocumentAssets.cs`: recursive keyed rendering, node affordance, bridge click and keydown branches, flex child rule.
- `QuickFiler/Controllers/BreadcrumbBridgeRouter.Descendants.cs` (new), `BreadcrumbBridgeRouter.cs`, `.Arrows.cs`, `.Selection.cs`: descendant handlers, first-level stale guard, preferred-row preselection.
- `EfcFormController.Actions.cs`, `EfcFormController.Breadcrumb.cs`: awaited delete gesture with the trash row preferred.
- `QuickFiler/Resources/FolderBreadcrumb.js`: name before toggle, per-depth indent.
- Eight new C# test files, one node suite, four project-file registrations; feature documents, evidence and two task-researcher memory notes.

## 10. Compliance Verdict

**REMEDIATION_REQUIRED.** Toolchain, repository coverage, new-code coverage, changed-line coverage, scope fences and all 26 acceptance criteria pass. Four blocking findings remain: B-1, B-2 and B-4 are autonomous; B-3 requires a maintainer decision.

## Appendix A: Test Inventory

| Test file | Tests | Status | Scope |
|---|---|---|---|
| `UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbMessageCodecIssue1002Tests.cs` | 8 results (5 methods) | new | codec contract |
| `UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbRowDescendantsIssue1002Tests.cs` | 24 | new | row tree state |
| `UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbHtmlRendererIssue1002Tests.cs` | 10 | new | renderer and bridge script contract |
| `QuickFiler.Test/Controllers/BreadcrumbBridgeRouterIssue1002Tests.cs` | 11 | new | descendant toggle and activation |
| `QuickFiler.Test/Controllers/BreadcrumbBridgeRouterStaleCompletionIssue1002Tests.cs` | 7 | new | stale-completion guards |
| `QuickFiler.Test/Controllers/BreadcrumbBridgeRouterTrashIssue1002Tests.cs` | 7 | new | preferred-row bind |
| `QuickFiler.Test/Controllers/EfcFormControllerTests.Issue1002.cs` | 4 | new | delete gesture end to end |
| `QuickFiler.Test/Viewers/FolderBreadcrumbNestingIssue1002Tests.cs` | 4 | new | QFC script text contract |
| `tests/quickfiler-breadcrumb/issue-1002-nesting.test.mjs` | 2 | new, local only | QFC DOM behaviour |

## Appendix B: Toolchain Commands Reference

| Stage | Command | Run by |
|---|---|---|
| C# format | `dotnet tool run csharpier format .` then `dotnet tool run csharpier check .` | executor |
| C# analyzers | `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` | executor |
| C# type check | `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true` | executor |
| C# test and coverage | `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot .` | executor |
| Node suites | `node --test --test-isolation=none --test-concurrency=1 --experimental-test-coverage --test-coverage-include=QuickFiler\Resources\FolderBreadcrumb.js` over the three suites | executor |
| Diff scope | `git diff --numstat 8e4ad1ecb...HEAD`; `git merge-base main HEAD` | reviewer |
| Coverage parsing | Python `xml.etree.ElementTree` union of class line nodes per filename plus `git diff -U0` new-side hunks, over `artifacts/csharp/coverage.xml` and the executor baseline Cobertura; `cmp` of the canonical and executor final XML | reviewer |
| Scans | `git diff 8e4ad1ecb...HEAD` added-line filters for identity tokens, suppressions, sleeps, temp files, `ConfigureAwait(false)`; `grep` of router files for `lock (` | reviewer |
