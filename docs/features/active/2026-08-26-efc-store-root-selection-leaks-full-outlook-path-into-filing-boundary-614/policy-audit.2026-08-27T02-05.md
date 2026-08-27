# Policy Compliance Audit: Issue #614 post-remediation cycle 2

**Audit Date:** 2026-08-27
**Code Under Test:** Full branch diff from merge base `c279d40bddacdba00c29a9724d1b5b17f9ebbc90` to head `8188cff9537125255bdd0415ce4b9b701c138c99`; C# production, tests, legacy project configuration, and feature documentation.
**Policy Order:** `AGENTS.md` general code policy, `AGENTS.md` general unit-test policy, `.agents/skills/csharp/SKILL.md`, feature-review shared skills.

**Coverage Metrics by Language:**

| Language | Files Changed | Tests | Test Result | Baseline Coverage | Post-Change Coverage | New/Changed Code Coverage |
|---|---:|---:|---|---|---|---|
| C# | 10 production `.cs`, 11 test `.cs`, 6 project/package files | 6,586 | 6,586 pass, 0 fail | 84.8758% line; 78.8585% branch | 84.8841% line; 78.8692% branch | `EfcSelectionGuard` 100% line/branch; `ArchiveStemContract` 100% retained; `GetStem` branch 2/2 |

Baseline evidence: `evidence/qa-gates/coverage-delta.2026-08-26T22-28.md` section (a).
Post-change and changed-code evidence: the same artifact sections (b)-(e).
Final test evidence: `evidence/qa-gates/final-test-coverage.2026-08-26T22-27.md`, with the schema defect recorded below.

### Coverage Evidence Checklist

- TypeScript baseline coverage artifact: N/A - no TypeScript files changed.
- TypeScript post-change coverage artifact: N/A - no TypeScript files changed.
- PowerShell baseline coverage artifact: N/A - no PowerShell files changed.
- PowerShell post-change coverage artifact: N/A - no PowerShell files changed.
- Per-language comparison summary: C# baseline 84.8758%, post-change 84.8841%, change +0.0083 percentage points, new/changed-code 100% for `EfcSelectionGuard` line/branch and retained `ArchiveStemContract` line coverage, with `GetStem` branch 100% (2/2); PASS.

## Executive Summary

The C# implementation, tests, coverage, scope, formatting, analyzer, and nullable results meet the current repository policies. The authoritative cycle-2 toolchain evidence records CSharpier over 1,530 files, analyzer and nullable rebuilds with zero errors, and 6,586/6,586 passing tests. Repository line coverage is above the current `AGENTS.md` floor of 80%, both line and branch rates improve against the measured baseline, and the named changed contract code meets or exceeds 90% coverage.

The audit is **PARTIALLY COMPLIANT / ACCEPTED RISK** for one evidence-policy failure. `final-test-coverage.2026-08-26T22-27.md` places the successful authoritative run (`EXIT_CODE: 0`) and a separate expected-failure preservation run (`ExpectedExitCode: 1`) in one file. The canonical collector consequently reports the final test artifact as a normalized failure. The underlying test result is verified, but the required evidence representation remains noncompliant. A second retained documentation finding is that the change description maps the unguarded archive-root-resolution crash to #637 rather than #638.

After this full re-review and before any documentation remediation or PR authoring, the user approved a `scope_change` that removes exactly those two documentation/evidence findings from remaining remediation scope. The findings and consequences remain disclosed; neither affected source file is to be edited solely for these findings, and the normalized evidence row is not reported as passing.

**Policy documents evaluated:**

- PASS — repository `AGENTS.md` tone, general code-change, general unit-test, and C# policy sections.
- PASS — `.agents/skills/csharp/SKILL.md` toolchain, nullable, MSTest, Moq, FluentAssertions, and deterministic-test rules.
- FAIL — evidence-and-timestamp-conventions one-expectation-per-file rule for the cycle-2 final test artifact.
- PASS — accepted file-size handling in canonical orchestrator adjudications: new and under-limit files stay under 500; three pre-existing over-limit touched files do not grow.

**Temporary artifacts cleanup:** DEFERRED TO ORCHESTRATOR. The temporary runtime-only commit-steward profile is untracked, is excluded from this review, and must not be staged or committed. Its required removal before clean completion remains an orchestrator lifecycle responsibility. No `coverage/` path was committed; the other current worktree additions are review/checkpoint artifacts.

## 1. General Unit Test Policy Compliance

### 1.1 Core Principles

| Requirement | Status | Evidence |
|---|---|---|
| Independence | PASS | Tests use per-test objects, local mocks, and reset log appenders where shared log4net state is observed. |
| Isolation | PASS | Pure contract, router, resolver, converter, and data-model seams are tested separately; the composition test explicitly crosses the filing guard/boundary pair. |
| Fast execution | PASS | Full runner and targeted assembly evidence complete without a product-code delay or retry added by this feature. |
| Determinism | PASS | Check-only scan found no `Thread.Sleep`, `Task.Delay`, wall-clock, randomness, temporary-file, process-environment mutation, network, or live-Outlook dependency in changed tests. |
| Readability and maintainability | PASS | MSTest names encode scenario and outcome; FluentAssertions messages state the contract rationale. |

### 1.2 Coverage and Scenarios

| Requirement | Status | Evidence |
|---|---|---|
| Baseline coverage documented | PASS | 53,998/63,620 lines (84.8758%) and 12,753/16,172 branches (78.8585%). |
| No coverage regression | PASS | Post 53,988/63,602 lines (84.8841%) and 12,750/16,166 branches (78.8692%); deletion-adjusted and retained-file confirmation gates pass. |
| New/changed code at least 90% | PASS | `EfcSelectionGuard` 17/17 lines and 10/10 branches; `ArchiveStemContract` 51/51 lines retained; `GetStem` branch 2/2. |
| Positive flows | PASS | Valid relative stems, under-root activation, child activation, dotted names, UNC roots, short roots, and OneDrive priority selection are covered. |
| Negative flows | PASS | Null/empty/whitespace, rooted, cross-store, boundary near-miss, reserved name, invalid segment, unresolvable roots, and redaction cases are covered. |
| Edge cases | PASS | Separator-only root, case variants, repeated ancestor text, root-exact input, one- and two-character stems, trailing dot/space, and device-name extensions are covered. |
| Error handling | PASS | Tests assert exception type, parameter, diagnostic delivery, state preservation, and value redaction. |
| Concurrency | N/A | New logic is pure or synchronous decision logic; existing router async behavior is not changed into a shared-state algorithm by #614. |
| State transitions | PASS | Router rejection preserves the prior non-null selection and valid activation commits the archive-relative stem. |

### 1.2.1 Per-Language Coverage Comparison

- C#: Baseline: 84.8758% line / 78.8585% branch. Post-change: 84.8841% line / 78.8692% branch. Change: +0.0083 percentage points line and +0.0107 percentage points branch. New/changed-code coverage: 100% for `EfcSelectionGuard` line/branch, retained `ArchiveStemContract` line coverage, and `GetStem` branch (2/2). Disposition: PASS. Evidence: `coverage-delta.2026-08-26T22-28.md`.

### 1.3 Test Structure and Diagnostics

| Requirement | Status | Evidence |
|---|---|---|
| Clear failure messages | PASS | FluentAssertions because-text identifies rootedness, archive boundaries, redaction, and expected state. |
| Arrange-Act-Assert | PASS | Changed tests use explicit AAA comments or equivalent short AAA layout. |
| Document intent | PASS | Descriptive names include issue number or behavior, input class, and expected result. |

### 1.4 External Dependencies and Environment

| Requirement | Status | Evidence |
|---|---|---|
| Avoid external dependencies | PASS | Automated tests exclude `LiveOutlook`; no network or filesystem fixture is introduced. |
| Use mocks/stubs | PASS | Moq is used for provider, Outlook, and globals collaborators; delegates cover pure environment/root seams. |
| Environment stability | PASS | No test mutates process environment or creates temporary files. |

### 1.5 Policy Audit Requirement

| Requirement | Status | Evidence |
|---|---|---|
| Pre-submission review | PASS | This artifact, the code review, and the feature audit cover the complete branch diff. |

## 2. General Code Change Policy Compliance

### 2.1 Before Making Changes

| Requirement | Status | Evidence |
|---|---|---|
| Clarify objective | PASS | `issue.md`, `spec.md`, and the defect-census research define the rooted-path contract defect set. |
| Read existing plans | PASS | Original plan and two remediation plans are complete and present in the feature folder. |
| Document the plan | PASS | Atomic plans use phased task IDs and contain final QA/evidence tasks. |

### 2.2 Design Principles

| Requirement | Status | Evidence |
|---|---|---|
| Simplicity first | PASS | One pure shared contract replaces repeated substring/replacement logic. |
| Reusability | PASS | Router, data model, boundary, and converter reuse `ArchiveStemContract`. |
| Extensibility | PASS | Public contract methods have explicit, stable inputs and output/exception behavior. |
| Separation of concerns | PASS | Pure path decisions are separate from COM, UI, filesystem, and logging boundaries. |

### 2.3 Module and File Structure

| Requirement | Status | Evidence |
|---|---|---|
| Cohesive modules | PASS | New contract and guard files each model one responsibility. |
| Under 500 lines | PASS under approved existing-file exception | New/under-limit files comply. `BreadcrumbBridgeRouter` 596->596, `EfcFormController` 1084->1073, and `BreadcrumbBridgeRouterIssue439Tests` 694->694 under the checkpoint's approved net-non-growth adjudications. |
| Public versus internal | PASS | Only the cross-assembly contract is public; application-specific guards remain internal. |
| No circular dependencies | PASS | Legacy project builds succeed with explicit compile includes and no new cycle. |

### 2.4 Naming, Docs, and Comments

| Requirement | Status | Evidence |
|---|---|---|
| Descriptive names | PASS | `ArchiveStemContract`, `TryMakeArchiveRelative`, and `IsValidFilingSelection` state intent directly. |
| Documentation | PASS | Public contract members and non-obvious internal failure semantics have XML documentation. |
| Comment why | PASS | Comments explain separator boundaries, redaction, short-name filing, and deferred #637 normalization. |

### 2.5 After Making Changes - Toolchain Execution

| Requirement | Status | Evidence |
|---|---|---|
| Formatting | PASS | `dotnet tool run csharpier format .` then `check .`; 1,530 files; exit 0. |
| Linting/analyzers | PASS | Analyzer-enabled `/t:Rebuild`; zero errors. |
| Type checking | PASS | warning-as-error `/t:Rebuild`; zero errors; no `/p:Nullable=enable`. |
| Testing | PASS underlying run; FAIL evidence representation | Authoritative runner: 6,586/6,586, exit 0. The same file's later expectation 1 causes normalized fail and must be split. |
| Full toolchain loop | PASS | Cycle-2 clean-pass artifact records steps in order with restart count 0. |
| Explicit reporting | PARTIAL | Commands/results are present, but the final test artifact violates the per-file expectation schema. |

### 2.6 Summarize and Document

| Requirement | Status | Evidence |
|---|---|---|
| Summarize changes | PASS | `change-description.2026-08-26.md` records original delivery and both remediation cycles. |
| Design choices explained | PASS | Spec records rejection versus clamping, drive-root rejection, and fail-fast root behavior. |
| Update supporting documents | PARTIAL | Root-resolution follow-up on change-description line 313 incorrectly names #637 instead of #638. |
| Provide next steps | PASS | Producer normalization #637 and UI-bound archive-root failure #638 are separately promoted. |

## 3. Language-Specific Code Change Policy Compliance

### 3A. C# Code Change Policy

| Requirement | Status | Evidence |
|---|---|---|
| Manifest-pinned CSharpier | PASS | Invoked through `dotnet tool run`; 1,530 files checked. |
| Analyzer rebuild | PASS | `/t:Rebuild`, `EnableNETAnalyzers=true`, `EnforceCodeStyleInBuild=true`; zero errors. |
| Nullable/type rebuild | PASS | `/t:Rebuild`, warnings as errors; no global nullable opt-in; zero errors. |
| Strong contracts | PASS | Rootedness and conversion behavior are expressed in methods and exceptions. |
| Null safety | PASS | Nullable-enabled new files guard nullable values before use. |
| Focused types | PASS | Shared pure contract, selection guard, and root guard are cohesive. |
| Resource/asynchrony safety | PASS | No new disposable or blocking wait was introduced. |
| Logging and exceptions | PASS with follow-up | Diagnostics are redacted and specific; the pre-existing UI propagation path is tracked by #638. |
| Suppressions/dependencies | PASS | No suppression added; the log4net test reference is paired correctly in csproj and packages.config. |

## 4. Language-Specific Unit Test Policy Compliance

### 4A. C# Unit Test Policy

| Requirement | Status | Evidence |
|---|---|---|
| MSTest | PASS | New tests use `[TestClass]` and `[TestMethod]`; no xUnit/NUnit introduced. |
| Moq | PASS | Moq is used where collaborators exist; pure tests use direct calls or narrow delegates. |
| FluentAssertions | PASS | New and modified assertions use FluentAssertions. |
| Deterministic CLI population | PASS | `TestCategory!=LiveOutlook` is used in affected-assembly integration evidence; full runner applies the canonical population. |
| Coverage-enabled final run | PASS underlying run | Numeric Cobertura line/branch results recorded; metadata split remains required. |

## 5. Test Coverage Detail

| Component | Tests/Evidence | Line Coverage | Branch Coverage | Status |
|---|---|---:|---:|---|
| `ArchiveStemContract` | 23 named contract tests | 51/51 | reported 100% retained | PASS |
| `EfcSelectionGuard` | 25 predicate/composition tests after cycle 2 | 17/17 | 10/10 | PASS |
| `EmailFilerConfig.GetStem` fallback | RC-4 test | covered | 2/2 | PASS |
| Router rooted-path behavior | Issue #614 and #439/#609 regression suites | covered by targeted and full runs | scenario complete | PASS |
| Root/OneDrive guards | AppGlobals test suites | new methods at least 90% line in authoritative delivery evidence | covered positive/error paths | PASS |

Uncovered changed `EfcFormController` sequence points are host-bound WinForms glue and were also uncovered at baseline; the pure predicates they call are fully covered. No changed or retained file has a confirmed coverage loss under the cycle-2 comparison procedure.

## 6. Test Execution Metrics

| Metric | Value | Status |
|---|---:|---|
| Total tests | 6,586 | PASS |
| Tests passed | 6,586 (100%) | PASS |
| Tests failed | 0 | PASS |
| Filtered line coverage | 84.8841% (53,988/63,602) | PASS against 80% floor and baseline |
| Filtered branch coverage | 78.8692% (12,750/16,166) | PASS versus baseline |
| Affected-assembly integration | 6,109/6,109 | PASS |
| Evidence normalization | authoritative row currently reports fail | FAIL until artifact split |

## 7. Code Quality Checks

| Check | Command | Result | Status |
|---|---|---|---|
| CSharpier | `dotnet tool run csharpier format .` then `dotnet tool run csharpier check .` | 1,530 files, exit 0 | PASS |
| .NET analyzers | `MSBuild.exe TaskMaster.sln /t:Rebuild ... /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` | zero errors | PASS |
| Nullable/type | `MSBuild.exe TaskMaster.sln /t:Rebuild ... /p:TreatWarningsAsErrors=true` | zero errors | PASS |
| MSTest with coverage | `pwsh -NoProfile -File scripts\vscode\Invoke-MSTestWithCoverage.ps1 -SearchRoot .` | 6,586/6,586; exit 0 | PASS underlying, evidence PARTIAL |
| Cached diff | `git diff --check merge-base..HEAD` | exit 0 | PASS |

## 8. Gaps and Exceptions

### Identified Gaps

1. **Evidence schema — FAIL, accepted risk.** `final-test-coverage.2026-08-26T22-27.md` carries `EXIT_CODE: 0` for the authoritative run and `ExpectedExitCode: 1` for another run. `artifacts/pr_context.summary.txt:407-412` normalizes it as fail. The technically available correction would split the runs and refresh through `drm-copilot`; the approved scope change expressly excludes that remediation.
2. **Follow-up mapping — PARTIAL, accepted risk.** `change-description.2026-08-26.md:313` names #637 for the unguarded archive-root-resolution crash, although #638 is the accurate follow-up; line 315 correctly retains #637 for producer normalization. The approved scope change expressly excludes correcting this reference.
3. **Live manual validation — recorded exception.** Five live-Outlook steps were not executable in the automated environment. The evidence names the reason and automated counterparts exactly as AC26 permits.

### Approved Exceptions

- **Pre-existing file size:** Canonical checkpoint adjudications approve net non-growth for the three pre-existing over-limit touched files. All new and previously compliant files remain under 500 lines.
- **Coverage note:** Older Claude-derived artifacts discuss an 85% threshold. Current repository `AGENTS.md` requires at least 80%; the branch is above 80% and improves its measured baseline.
- **User-approved scope decision / accepted risk:** Checkpoint requirement `issue-614-approved-documentation-findings-scope-change` records approval at `2026-08-27T02:11:56.137Z`, after this re-review and before documentation remediation or PR authoring. Exactly the mixed-expectation evidence representation and the #637/#638 change-description reference are excluded from remediation. The canonical row remains normalized fail; the separate 6,586/6,586 exit-0 QA fact remains verified; the inaccurate reference remains present; and strict completion validation may still reject the unchanged artifacts.

### Removed/Skipped Tests

- Cycle 2 intentionally removed one cycle-1 guard test while preserving the correct rooted-rejection cases; suite arithmetic is 6,587 minus 1 = 6,586 and all current tests pass.
- No planned command was skipped in the authoritative cycle-2 toolchain pass.

## 9. Summary of Changes

The branch contains promotion/research/specification artifacts, a validated original plan, two remediation cycles, ten C# production-file changes, eleven C# test-file changes, legacy project wiring, canonical QA evidence, and promoted follow-up issues. The current remediation commit is `98b7a5e14afcf5580bf9351be3ca18e9e306dca9`; documentation close-out is `8188cff9537125255bdd0415ce4b9b701c138c99`.

Principal implementation files:

- `QuickFiler/Controllers/BreadcrumbBridgeRouter.cs`, `EfcDataModel.cs`, `EfcFormController.cs`, `EfcSelectionGuard.cs`.
- `UtilitiesCS/.../EmailFilerConfig.cs`, `ArchiveStemContract.cs`, `FolderConverter.cs`.
- `TaskMaster/AppGlobals/AppFileSystemFolderPaths.cs`, `AppOlObjects.cs`, `ArchiveRootPathGuard.cs`.
- Mirrored MSTest files in `QuickFiler.Test`, `UtilitiesCS.Test`, and `TaskMaster.Test`.

## 10. Compliance Verdict

### Overall Status: PARTIALLY COMPLIANT — ACCEPTED RISK; REVIEW PASS

The product code and tests satisfy the applicable C# and unit-test policies, and the underlying final toolchain is green. Full policy compliance is not claimed because canonical PR context still represents the final test artifact as failed and the change-description reference remains inaccurate. Under the recorded user-approved scope change, these two findings are accepted risks rather than remaining remediation work, so this review does not generate remediation inputs or a remediation plan.

### Policy-by-Policy Summary

- General Code Change Policy: PARTIAL — implementation and QA pass; one documentation reference is inaccurate.
- C# Code Change Policy: PASS.
- General Unit Test Policy: PASS for behavior and coverage; PARTIAL for evidence packaging.
- C# Unit Test Policy: PASS.
- Evidence and Timestamp Conventions: FAIL on the mixed-expectation final test artifact.

### Metrics Summary

- 6,586/6,586 authoritative tests passed.
- 84.8841% line and 78.8692% branch coverage, both improved from baseline.
- `EfcSelectionGuard` 100% line and branch coverage.
- Analyzer and nullable rebuilds: zero errors.
- CSharpier: 1,530 files checked.

### Recommendation

**Proceed to PR authoring with the two accepted risks disclosed.** Do not edit either affected documentation/evidence file solely for these findings. Preserve the canonical normalized-fail row, the independently verified 6,586/6,586 exit-0 QA fact, and the inaccurate #637 reference in the review record. If strict completion validation rejects the accepted risks, report that blocker without implementing the waived changes.

## Appendix A: Test Inventory

Primary changed suites reviewed:

- `BreadcrumbBridgeRouterIssue614Tests` — eight selection/root-boundary scenarios.
- `BreadcrumbBridgeRouterTests` and `BreadcrumbBridgeRouterIssue439Tests` — original #614 fail-before and #439/#609 regression behavior.
- `EfcDataModelIssue614Tests` — eight prefix, root, cross-store, case, and repetition cases.
- `EfcSelectionGuardTests` — filing, creation, short-name, rootedness, and composition cases.
- `ArchiveStemContractTests` — rootedness, validation, conversion, boundary, case, and separator-only cases.
- `FolderConverterIssue614Tests` and `FolderConverterTests` — derived-segment rules, root mapping, redaction, and D5f behavior.
- `EmailFilerConfig_Tests` — both boundary overloads, `IsDeleteRelevant`, #609 preservation, and `GetStem` fallback.
- `AppOlObjectsArchiveRootValidationTests` and `AppFileSystemFolderPathsOneDriveResolutionTests` — root validation, diagnostics, and environment-priority resolution.

## Appendix B: Toolchain Commands Reference

```powershell
dotnet tool run csharpier format .
dotnet tool run csharpier check .
& "C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe" TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true
& "C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe" TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true
pwsh -NoProfile -File scripts\vscode\Invoke-MSTestWithCoverage.ps1 -SearchRoot .
git diff --check c279d40bddacdba00c29a9724d1b5b17f9ebbc90..HEAD
```

**Audit Completed By:** Codex feature-reviewer-c3-elevated
**Audit Date:** 2026-08-27
**Policy Version:** Repository state at head `8188cff9537125255bdd0415ce4b9b701c138c99`
