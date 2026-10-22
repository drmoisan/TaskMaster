# Policy Compliance Audit — Issue #677 (quickfiler-keyboard-hook-leaks-to-outlook)

- **Artifact:** `policy-audit.2026-08-28T12-31.md`
- **Reviewer:** feature-review agent
- **Date:** 2026-08-28
- **Branch under review:** `bug/quickfiler-keyboard-hook-leaks-to-outlook-677` @ `59bc263020b6c07678be53102f0c72fab3dd0fcf`
- **Resolved base branch:** `main`; merge base `361a49b884a4e3fe192bf04bae05151c598398fa` (per `pr-base-branch-merge-base`; caller-supplied value re-confirmed against `git merge-base` output implied by the PR-context range)
- **Work mode:** `full-bug` (persisted marker in `issue.md`); AC source: `spec.md` only
- **Scope:** full branch diff vs merge base — 74 changed paths: 17 C#/csproj files (11 production, 4 test, 2 project files), 55 feature-folder/promotion docs and evidence artifacts, 11 `.claude/agent-memory` files
- **Template provenance:** the MCP tool `resolve_policy_audit_template_asset` is not exposed in this review session (no MCP tools available). Per `policy-audit-template-usage` fallback guidance, this artifact reproduces the full canonical major-heading set from the skill rather than a degraded stub; this is the only deviation and it is documented here.

## Executive Summary

The branch delivers a two-part focus-routing fix for issue #677 (WebView2 focus retention leaking Outlook keyboard input): an execution-time focus-permission predicate on `BreadcrumbDropDownHost`, and a `Form.Deactivate`-routed focus-parking and selector-cancel handler through `QfcFormViewer`/`QfcFormController`. Seventeen new MSTest regression tests accompany the fix.

All four C# toolchain gates pass; format, analyzer, and nullable gates were **independently re-run by this reviewer** in this session (not merely accepted from executor evidence), and the 17 new tests were independently re-executed (38/38 passing in the scoped filter run). Coverage figures were independently re-parsed from the two committed Cobertura XMLs and match the executor's claims exactly. Zero blocking findings. One per-file coverage row records FAIL on pre-existing debt and is dispositioned non-blocking with full rationale in section 5. Three of ten spec acceptance criteria remain intentionally unchecked pending manual live-Outlook verification with an authored runbook; this is the correct handling under `acceptance-criteria-tracking` (see feature audit).

**Verdict: PASS — 0 blocking findings. PR-ready subject to the documented manual-verification residual.**

## Rejected Scope Narrowing

None. The caller's task prompt supplied context notes (executor deviations, manual-AC handling) but explicitly disclaimed scope instruction ("not a scope instruction — determine scope independently"). No narrowing attempt was detected. The audit scope is the full branch diff vs `main` at merge base `361a49b8`.

Note recorded for transparency: `artifacts/pr_context.summary.txt` as generated misclassified the branch as docs-only ("Core logic changes: 0 files"), a recurring classifier defect. The reviewer verified scope directly against `git diff --name-status` and corrected the summary in place with the 17 code files. This was a tooling defect, not a caller narrowing attempt.

## Evidence Location Compliance

- Scan method: `git diff --name-status 361a49b8..HEAD` over all 74 changed paths. The helper script `validate_evidence_locations.py` does not exist in this repository (searched repo-wide); the equivalent scan was performed manually.
- Files written under `artifacts/baselines/`, `artifacts/qa/`, `artifacts/evidence/`, or `artifacts/coverage/`: **0**. No violations.
- All evidence artifacts live under the canonical `docs/features/active/2026-08-28-quickfiler-keyboard-hook-leaks-to-outlook-677/evidence/<kind>/` tree with kinds `baseline`, `regression-testing`, `qa-gates`, `issue-updates`, `other`. **PASS.**
- No `EVIDENCE_LOCATION_OVERRIDE_REJECTED` events: no caller instruction specified a non-canonical evidence path.

## 1. General Unit Test Policy Compliance

| Requirement | Verdict | Evidence |
|---|---|---|
| Independence / any-order execution | PASS | New tests use per-test harnesses (`PredicateHarness` per `using` block), fresh Moq mocks in `[TestInitialize]`, no shared static state. Reviewer re-ran the scoped 38-test filter green. |
| Isolation (one behavior per test) | PASS | Each of the 17 tests targets one guard branch or wiring contract; names encode scenario and expectation. |
| Fast execution | PASS | Scoped 38-test run completed in seconds (reviewer re-run, vstest exit 0). |
| Determinism | PASS | The popup UI boundary is a captured `CapturingSynchronizationContext` drained explicitly on the creating thread — never installed as ambient — so scheduling/execution gaps are deterministic, not timing-dependent. No sleeps, no timers, no wall-clock reads, no randomness. |
| Readability / documented intent | PASS | Every test carries an XML doc comment summarizing scenario and expected outcome; Arrange–Act–Assert sections are commented explicitly. |
| Scenario completeness | PASS | Positive (predicate-true control cases), negative (predicate-false on all three focus paths), edge (predicate flips after scheduling; unset predicate default; already-open refocus), error handling (`FormDeactivated_ItemCancelThrows_DoesNotPropagateAndContinues`), state (null groups / null item-groups / null viewer). |
| No external dependencies | PASS | Headless: no window shown, no handle created in controller tests; `CoreWebView2Environment` obtained via `FormatterServices.GetUninitializedObject` (pre-existing repo pattern), never initialized against the runtime. |
| No temporary files in tests | PASS | No filesystem access anywhere in the three new test files (inspected in full). |
| Test file location | PASS | Repository convention is per-project `<Project>.Test` trees mirroring production structure (`QuickFiler.Test/Viewers/...`, `QuickFiler.Test/Controllers/...`); the three new files follow it exactly. No colocation with production sources. |
| Coverage requirements | PASS with one dispositioned per-file FAIL row | Section 5. |

## 2. General Code Change Policy Compliance

| Requirement | Verdict | Evidence |
|---|---|---|
| Bugfix workflow — failing regression test first | PASS (adjudicated) | A conventional failing *run* was structurally impossible: the 17 tests reference guard-surface members (`MayTakeFocus`, `FormDeactivated`, `IsWebView2Focused`, `ParkFocusOffWebView2`, `CancelBreadcrumbSelector`) via compile-time typed references that did not exist pre-fix, so the test assembly could not compile. The equivalent proof is the P1-T5 `[expect-fail]` build: EXIT_CODE 1 against declared `ExpectedExitCode: 1`, 22 diagnostics (20 CS1061 + 2 collateral CS1503), every one located in the three new test files, zero in production files (`evidence/regression-testing/p1-t5-expectfail-build.md` + full teed log + `fail-before-exception.2026-08-28T15-55.md` dossier). Green flip P3-T10 re-ran the byte-identical command at EXIT_CODE 0 with proof no assertion or test body changed in between. This compile-red-plus-dossier form is a valid RED-first equivalent: the typed (non-reflection) references make absence a compiler-proved fact. |
| Minimal, targeted fix | PASS | Production diff is +205/-3 lines across 11 files, all on the focus-permission / deactivation-routing / selector-cancellation surface enumerated in `spec.md` Scope. `KeyboardHandler.cs` untouched (reviewer-verified: empty diff). No opportunistic refactors. |
| Simplicity / reusability / extensibility / separation of concerns | PASS | Injectable `Func<bool>` predicate with safe default; Seam-B event routing matching the existing pattern; interface members are narrow and documented. Pure predicate logic (`MayRestoreBreadcrumbFocus`) is separate from the WinForms glue. |
| 500-line file limit | PASS | Reviewer re-counted all 15 touched/created source files with `awk NR`; counts match the executor's `file-size-audit.md` exactly. Maximum: `BreadcrumbDropDownHost.cs` at 498. All others <= 467. Residual risk: 498/500 leaves 2 lines of headroom (section 8). |
| Error handling — fail fast, no silent swallow | PASS | The single new `catch (Exception)` is a deliberate per-item boundary catch inside a WinForms event handler; it logs via the class's existing log4net logger with context and continues cancelling remaining items. Rationale is documented in-code; an escaping exception would surface as an unhandled UI-thread failure inside Outlook. Analyzer run raises no diagnostic on it. |
| Logging pattern | PASS | Uses the existing `logger` (log4net) member; no ad-hoc console output. |
| Naming / docs / comments | PASS | XML docs on all new public/internal surface; comments explain *why* (execution-time rule, `ContainsFocus` rejection rationale, property-vs-constructor seam rationale). |
| No new dependencies | PASS | No new package references. The two NuGet analyzer package versions provisioned into gitignored `packages/` are environment provisioning only (section 8, deviation D-1). |
| Public API compatibility | PASS | All additions are additive (`internal` property; three additive `IQfcFormViewer` members; one additive member on `IItemViewer`/`IQfcItemController`). Constructor signatures unchanged, preserving five reflection-bound test harnesses. Sole non-Moq implementer (`FakeQfcItemController`) gained the interface-completing no-op member — the sanctioned structural enabler. |
| Toolchain loop (single clean pass) | PASS | Reviewer independently re-ran: CSharpier check (1558 files, 0 violations, exit 0), analyzer rebuild (exit 0, 0 errors, exactly the 5 pre-existing System.Reactive packages.config advisories), nullable rebuild (exit 0, 0 CS86xx), scoped vstest (38/38, exit 0). Executor's full-suite evidence: 6838/6838 passing with coverage (section 6). |
| Supporting docs updated | PASS | `spec.md`, `issue.md`, `pr-notes.md`, plan check-offs, `evidence/issue-updates/issue-677.md`, manual-verification record, and runbook all updated/authored. |

## 3. Language-Specific Code Change Policy Compliance (C#)

| Requirement | Verdict | Evidence |
|---|---|---|
| CSharpier formatting via `dotnet tool run` (pinned 1.2.6) | PASS | Reviewer re-ran `./.dotnet-sdk/dotnet.exe tool run csharpier check .`: "Checked 1558 files in 4447ms", exit 0. |
| .NET analyzers (`/t:Rebuild`, `EnableNETAnalyzers`, `EnforceCodeStyleInBuild`) | PASS | Reviewer re-ran the exact CR-ANALYZE command: exit 0, 0 errors; the only warnings are the 5 pre-existing uncoded `System.Reactive.PackagesConfigCheck.targets` advisories (QuickFiler, TaskMaster, ToDoModel, UtilitiesCS, UtilitiesCS.Test) — zero delta vs baseline. |
| Nullable/type-check (`/t:Rebuild`, `TreatWarningsAsErrors`, no `/p:Nullable=enable`) | PASS | Reviewer re-ran the exact CR-NULLABLE command: exit 0, CS86xx count 0. The two edited `#nullable enable` files (`BreadcrumbDropDownHost.cs`, `BreadcrumbDropDownHost.Open.cs`) introduce no null-state defect. |
| Nullable annotations quality | PASS | `Func<bool> MayTakeFocus` is non-nullable with a non-null initializer; guard bodies null-safe (`_groups?.ItemGroups`, `ItemController?.`, `_itemViewer?.`, `BreadcrumbCoordinator?.`). The deliberate omission of a `_formViewer` null guard in the deactivate handler is documented in-code as unreachable (handler reachable only through `_formViewer.FormDeactivated`). |
| `internal` for non-public API | PASS | `MayTakeFocus` is `internal` (test-reachable via pre-existing `InternalsVisibleTo`); new interface members are on already-internal-consumed interfaces. |
| Naming conventions | PASS | PascalCase members, camelCase locals throughout the diff. |
| No banned APIs / analyzer debt / suppressions | PASS | No `DateTime.Now`/`Thread.Sleep`/`Task.Delay` in the diff; no new suppressions; no `.editorconfig` or analyzer wiring changes. |

## 4. Language-Specific Unit Test Policy Compliance (C#)

| Requirement | Verdict | Evidence |
|---|---|---|
| MSTest framework | PASS | `[TestClass]`/`[TestMethod]` from `Microsoft.VisualStudio.TestTools.UnitTesting`; Part3 partial correctly omits the duplicate `[TestClass]` (CS0579), documented in-file. |
| Moq for mocking | PASS | `Mock<IQfcFormViewer>`, `Mock<IQfcItemController>`, `Mock<IQfcCollectionController>` etc., with `VerifyAdd`/`VerifyRemove`/`Raise` for event contracts. |
| FluentAssertions preferred | PASS | All assertions use FluentAssertions with because-messages; no bare MSTest `Assert`. |
| Arrange–Act–Assert | PASS | Explicit `// Arrange` / `// Act` / `// Assert` sections in all 17 tests. |
| Deterministic test rules / seam-based mocking | PASS | All external boundaries reached via seams (viewer interface mocks, injected delegates, captured sync context); reflection-based private-field injection follows the established `QfcFormControllerSeamTests` / `QfcItemController.PropertiesTests` patterns. |

## 5. Test Coverage Detail

Verification method: independent re-parse of the two committed Cobertura artifacts — `evidence/baseline/coverage-baseline.cobertura.xml` (P0-T9) and `evidence/qa-gates/coverage-final.cobertura.xml` (P5-T5) — aggregating `<class>` entries by `filename` with per-line max-hit (the async-state-machine rule). The canonical path `artifacts/csharp/coverage.xml` is absent in this worktree; per the established evidence model, the committed feature-evidence Cobertura pair is the verification artifact and was parsed directly. The reviewer did not regenerate coverage; figures below are reviewer-recomputed from the committed XMLs and match the executor's `coverage-delta.md` exactly.

### Language coverage rows (branch diff contains changed files in exactly one coverage language: C#)

| Language | Metric | Value | Floor | Verdict |
|---|---|---|---|---|
| C# | Repo-wide line coverage (final) | 85.2804% (54721/64166) | >= 85% | **PASS** |
| C# | Repo-wide branch coverage (final) | 79.23% (13027/16442) | >= 75% | **PASS** |
| C# | Repo-wide line coverage delta vs baseline | +0.0083 pts (85.2721% -> 85.2804%) | no regression | **PASS** |
| C# | New-file line coverage — `QfcFormController.Deactivate.cs` | 100.00% (24/24) | >= 90% | **PASS** |
| C# | Changed-line coverage, all five coverage-bearing files | 100.00% (0 uncovered changed lines; 0 regressed lines) | >= 90%, no regression | **PASS** |

TypeScript, Python, and PowerShell have zero changed files on this branch (verified against `git diff --name-status`); no coverage verdict is required for them.

### Per-file detail (whole-file line coverage, reviewer-recomputed)

| File | Kind | Baseline | Final | Changed-line cov. | Verdict |
|---|---|---|---|---|---|
| `QuickFiler/Controllers/QfcFormController.Deactivate.cs` | new | — | 100.00% (24/24) | 100% (24/24) | PASS |
| `QuickFiler/Viewers/BreadcrumbDropDownHost.cs` | modified | 99.29% (279/281) | 99.31% (288/290) | 100% (11/11) | PASS |
| `QuickFiler/Viewers/BreadcrumbDropDownHost.Open.cs` | modified | 100.00% (14/14) | 100.00% (14/14) | 100% (1/1) | PASS |
| `QuickFiler/Controllers/QfcItemController.FolderHandling.cs` | modified | 95.24% (140/147) | 95.27% (141/148) | 100% (1/1) | PASS |
| `QuickFiler/Controllers/QfcFormController.SetupDisposal.cs` | modified | 70.32% (109/155) | 70.70% (111/157) | 100% (2/2) | **FAIL** vs the 85% modified-file whole-file floor — dispositioned non-blocking, see below |

Changed-line spot-verification: the two residual uncovered lines in `BreadcrumbDropDownHost.cs` (352, 394) and the seven in `QfcItemController.FolderHandling.cs` (95–98, 169–171) all fall **outside** every changed hunk (new ranges 199–217, 286–301, 452–453 and 133–136 respectively), confirming 100% changed-line coverage and zero changed-line regression independently of the executor's tooling.

### Disposition of the `QfcFormController.SetupDisposal.cs` FAIL row (non-blocking)

The remediation trigger for a sub-floor modified file fired mechanically and was adjudicated non-blocking on these grounds:

1. **The repo-authoritative policy set is fully satisfied.** CLAUDE.md (policy authority #1) imposes three coverage requirements — repo-wide floor (85.28% >= 80%, and also >= the 85% uniform threshold in `.claude/rules/quality-tiers.md`), new-code >= 90% (100%), and no reduction on changed lines (zero) — and imposes **no whole-file floor on modified files**. The whole-file floor exists only in the review contract.
2. **Every uncovered line predates this branch.** All 46 uncovered lines at final were uncovered at baseline (reviewer-verified line-set comparison); they are pre-existing WinForms control-tree wiring (`TableLayoutHelper` row surgery, screen/working-area metrics, `ForAllControls` key-event lambdas requiring live control trees) untouched by this diff.
3. **The change improved the file** (70.32% -> 70.70%) and its own 2 changed lines are both covered by dedicated tests (`RegisterFormEventHandlers_SubscribesFormDeactivated` / `UnregisterFormEventHandlers_UnsubscribesFormDeactivated`).
4. **In-branch remediation would violate the bugfix minimal-fix mandate** ("Change only what is needed... If you uncover deeper design problems, open a new issue instead of widening scope").

Required follow-up (non-blocking): promote the pre-existing under-coverage of `QfcFormController.SetupDisposal.cs` wiring (and the analyzer-package version skew, section 8) through the MCP promotion lifecycle rather than leaving it as feature-folder prose. Recorded in section 8 and in the code review residuals.

## 6. Test Execution Metrics

| Run | Scope | Result | Source |
|---|---|---|---|
| P0-T11 baseline | `QuickFiler.Test` whole assembly | 1201/1201 passed | committed TRX, reviewer-parsed: outcome Completed, total=executed=passed=1201 |
| P4-T1 | `BreadcrumbDropDownHostTests` (all partials) | 29/29 passed | committed TRX, reviewer-parsed |
| P4-T2 | Deactivate + CancelBreadcrumbSelector tests | 9/9 passed | committed TRX, reviewer-parsed |
| P4-T3 | `QuickFiler.Test` whole assembly post-fix | 1218/1218 passed (= 1201 + 17 new; no pre-existing test dropped) | committed TRX, reviewer-parsed |
| P5-T5 final | Full suite, 9 assemblies, with coverage | 6838/6838 passed, 0 failed, 0 skipped | `evidence/qa-gates/coverage-final.md` |
| Reviewer re-run (this session) | Scoped filter over the new-test classes | 38/38 passed, vstest exit 0 | this session, against the assembly rebuilt by the reviewer's own analyzer/nullable rebuilds |

`BASELINE_FAILURE_SET` is empty and zero failures occurred at final, so no non-baseline failure exists.

## 7. Code Quality Checks

| Check | Command | Result |
|---|---|---|
| Format (check-only) | `./.dotnet-sdk/dotnet.exe tool run csharpier check .` | PASS — 1558 files, 0 violations, exit 0 (reviewer re-run) |
| Lint / analyzers | CR-ANALYZE (`msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`) | PASS — exit 0, 0 errors, 5 pre-existing uncoded advisories, zero delta (reviewer re-run) |
| Type check / nullable | CR-NULLABLE (`msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true`) | PASS — exit 0, 0 CS86xx (reviewer re-run) |
| Tests | vstest scoped re-run + committed TRX/coverage evidence | PASS — see section 6 |
| Host-path hygiene of committed evidence | `grep -cE 'C:\\+Users|<account>|<host-name>' <full 29.8 MB branch diff>` | PASS — 0 matches for the user-profile prefix, account identifier, and machine name across every committed byte, including the four TRX files (runUser sanitized to `<host>\<user>`) and both sanitized msbuild logs. TRX sanitization did not corrupt integrity: all four TRX files parse as XML and their counters match the claimed totals. |
| File-size ceiling | `awk NR` per touched file (reviewer re-count) | PASS — max 498 (`BreadcrumbDropDownHost.cs`); all 15 source files <= 500 |

## 8. Gaps and Exceptions

1. **Manual live-Outlook verification outstanding (AC-1, AC-2, manual half of AC-3).** These require Outlook's native message pump, real WebView2 runtime child windows, and real Win32 activation transitions — none reproducible headlessly without violating the determinism/no-external-process test policies. Handled correctly per `acceptance-criteria-tracking`: the AC checkboxes remain unchecked with a documented reason, a verification-status record exists (`evidence/other/manual-verification-pending.md`), and a human-exception runbook is authored (`runbooks/manual-live-outlook-verification.runbook.md`) with owner assigned (maintainer). Not a blocking gap; not remediable by an atomic plan.
2. **Deviation D-1 — analyzer package provisioning (accepted).** The executor installed `Meziantou.Analyzer 3.0.156` and `Roslynator.Analyzers 4.16.0` into the gitignored `packages/` directory to cure 10 `CS0006` errors caused by a **pre-existing committed skew**: 16 first-party `.csproj` files carry `<Analyzer Include>` HintPaths naming 3.0.156/4.16.0 while `packages.config` names 3.0.174/4.16.1. Reviewer verification: the branch diff contains no change to any `packages.config` and the only `.csproj` changes are additive `<Compile Include>` items — no tracked file was altered as a workaround, matching the executor's before/after `git status --porcelain` evidence. **Follow-up owed (non-blocking): promote the 16-csproj HintPath/packages.config version skew to a GitHub issue** — it blocks clean-worktree analyzer baselines for every future contributor and feature-folder prose disappears at merge.
3. **Deviation D-2 — evidence host-path sanitization (accepted).** Not itemized by the plan but mandated by the repository-wide no-host-paths rule; both msbuild logs record the substitution counts and post-condition sweeps, and the reviewer's independent full-diff scan (section 7) returned zero hits. Evidence integrity preserved (TRX parse + count consistency verified).
4. **Pre-existing coverage debt in `QfcFormController.SetupDisposal.cs`** — FAIL row dispositioned non-blocking in section 5; follow-up promotion recommended.
5. **`BreadcrumbDropDownHost.cs` at 498/500 lines.** Compliant but with 2 lines of headroom; the plan's D13 relocation remedy (move `FocusAnchorIfPermitted` to the `.Open.cs` partial) remains available to the next change that touches this file.
6. **PR-context classifier misclassification** (docs-only claim for a 17-code-file branch) — corrected in place by the reviewer; recurring tooling defect upstream of this repo.

## 9. Summary of Changes

- **Production (11 files, +205/-3):** execution-time focus-permission predicate (`MayTakeFocus`, default `() => true`) guarding `_focusAnchor` and `_focusPending` on both close and open paths of `BreadcrumbDropDownHost`; `MayRestoreBreadcrumbFocus` predicate implementation on `ItemViewer` (`Form.ActiveForm` identity, deliberately not `ContainsFocus`); additive Seam-B deactivation contract (`FormDeactivated`, `IsWebView2Focused`, `ParkFocusOffWebView2`) on `IQfcFormViewer`/`QfcFormViewer`; new `QfcFormController.Deactivate.cs` partial with the focus-parking and per-item selector-cancel fan-out; `CancelBreadcrumbSelector` fan-out hops on `IQfcItemController`/`QfcItemController` and `IItemViewer`/`ItemViewer`; subscription wiring in `SetupDisposal.cs`; two csproj `<Compile Include>` additions.
- **Tests (4 files, +701):** 17 regression tests in three new files plus the sanctioned one-member structural enabler in `QfcThemeHelperTests.cs`.
- **Docs/evidence (55 files):** feature folder (spec/issue/plan/research/pr-notes/runbook), full evidence tree (baseline, regression-testing, qa-gates, issue-updates, other), two promotion records.
- **Agent memory (11 files):** planner/executor/researcher memory updates; no host paths (verified).

## 10. Compliance Verdict

**PASS — 0 blocking findings.**

- All policy gates verified, with format/analyzer/nullable/tests independently re-executed by the reviewer this session.
- C# coverage: repo-wide line 85.2804% PASS, branch 79.23% PASS, new-code 100% PASS, changed-line 100% with zero regression PASS; one pre-existing-debt per-file FAIL row dispositioned non-blocking with follow-up promotion owed.
- Residuals: manual live-Outlook session (runbook authored, owner assigned) and two follow-up promotions (analyzer version skew; `SetupDisposal.cs` coverage debt).

## Appendix A: Test Inventory

New tests (17), all passing in committed TRX evidence and in the reviewer's scoped re-run:

`QuickFiler.Test/Viewers/BreadcrumbDropDownHostTests.Part3.cs` (8):
1. `FinishClose_DropDownClosedPath_PredicateFalse_DoesNotFocusAnchor`
2. `FinishClose_ProgrammaticClose_PredicateFalse_DoesNotFocusAnchor`
3. `FinishClose_PredicateTrue_FocusAnchorInvoked`
4. `FinishClose_PredicateFlipsFalseAfterScheduling_DoesNotFocusAnchor` (AC-5 execution-time proof: asserts `PendingCount > 0` before the flip, then drains)
5. `AlreadyOpenRefocus_PredicateFalse_DoesNotFocusPending`
6. `AlreadyOpenRefocus_PredicateTrue_FocusPendingInvoked`
7. `FreshOpenFocus_PredicateFalse_DoesNotFocusPending`
8. `UnsetPredicate_DefaultsTrue_FocusAnchorStillInvoked`

`QuickFiler.Test/Controllers/QfcFormControllerDeactivateTests.cs` (7):
9. `RegisterFormEventHandlers_SubscribesFormDeactivated`
10. `UnregisterFormEventHandlers_UnsubscribesFormDeactivated`
11. `FormDeactivated_WebView2Focused_ParksFocusOnce`
12. `FormDeactivated_NoWebView2Focus_DoesNotPark`
13. `FormDeactivated_CancelsSelectorOnEveryItemController`
14. `FormDeactivated_NullGroupsOrNullItemGroups_DoesNotThrow`
15. `FormDeactivated_ItemCancelThrows_DoesNotPropagateAndContinues`

`QuickFiler.Test/Controllers/QfcItemController.CancelBreadcrumbSelectorTests.cs` (2):
16. `CancelBreadcrumbSelector_ForwardsToViewer`
17. `CancelBreadcrumbSelector_NullViewer_DoesNotThrow`

## Appendix B: Toolchain Commands Reference

Commands executed by the reviewer in this session (all check-only or rebuild-verification; no file mutated):

1. `./.dotnet-sdk/dotnet.exe tool run csharpier check .` — exit 0
2. `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` (msbuild resolved via vswhere) — exit 0
3. `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true` — exit 0, 0 CS86xx
4. `vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation "/TestCaseFilter:FullyQualifiedName~BreadcrumbDropDownHostTests|FullyQualifiedName~QfcFormControllerDeactivateTests|FullyQualifiedName~CancelBreadcrumbSelectorTests"` — 38 tests, exit 0
5. Cobertura re-parse (PowerShell XML aggregation by filename, per-line max-hit) over the committed baseline and final XMLs
6. `git diff --name-status` / `--numstat` / `--unified=0` against merge base `361a49b8` for scope, changed-hunk mapping, and `KeyboardHandler.cs` invariance
7. Full-diff host-identifier grep over the 29.8 MB branch diff — 0 matches
