# Policy Compliance Audit — QuickFiler Search Keystroke Focus Steal (Issue #438)

- **Component:** QuickFiler folder-search / breadcrumb drop-down pipeline (`QuickFiler`, `UtilitiesCS`)
- **Date:** 2026-08-08T13-25
- **Reviewer:** feature-review agent
- **Branch:** `bug/quickfiler-search-keystroke-focus-steal-438` @ `ff9d14ab32d7e6d25c1c1c5b9011ccf9ae6286f5`
- **Base:** `main` @ merge-base `003c5715055d7d1933db68a742531332756e30b2` (recomputed via `git merge-base HEAD origin/main`; matches the caller-supplied value)
- **Work mode:** `full-bug` (persisted marker in `issue.md`); AC source: `spec.md` (AC-1..AC-14 gating; HV-1 non-gating)
- **Scope:** full branch diff vs base — 76 files: 30 `.cs`, 4 `.csproj`, 42 Markdown/XML documentation, evidence, and agent-memory files. Languages with changed files: **C# only**. TypeScript, Python, and PowerShell have zero changed files on this branch.
- **Template note:** the MCP tool `resolve_policy_audit_template_asset` is not available in this session (no MCP tools are exposed); the canonical major headings were reproduced from `.claude/skills/policy-audit-template-usage/SKILL.md` and the template instruction block is omitted by construction.

## Executive Summary

The branch delivers the #438 fix as specified: the per-keystroke handler composition is replaced by a single additive presentation intent, the open pipeline carries an explicit `takeFocus` intent through an additive interface overload, row replacement preserves the open selector session, and the search highlight is pending-only. All 6348 tests pass (EXIT 0), the four-stage C# toolchain passed in order in a final uninterrupted pass, repository-wide coverage improved on both axes and clears both uniform floors, and all 14 gating acceptance criteria verify PASS against independent evidence.

Two per-file coverage-floor findings prevent a clean verdict:

1. **FAIL (blocking):** the new production file `QuickFiler/Viewers/BreadcrumbItemViewerLifecycleCoordinator.Search.cs` measures 100% line but **50% branch coverage (2/4)** — below the uniform 75% branch floor for new code. The unexercised arms are the null-arms of the two null-conditional calls (`_openCoordinator?.`, `_bridgeCoordinator?.`), i.e., the documented no-open-coordinator fallback behavior has no test.
2. **FAIL (procedural, dispositioned non-blocking):** the modified file `QuickFiler/Controllers/QfcItemController.EventHandlers.cs` measures 78.65% line / 61.11% branch — below the 85%/75% modified-file floor. Verified attribution: the floor was already unmet at baseline (79.57%/65.00%), every changed line is covered, the uncovered-line set is identical before and after (19 lines both sides), and the ratio decrease is pure denominator arithmetic from deleting covered defective lines. Zero changed-line regression.

A third finding is a tooling defect, corrected in place: the generated PR-context summary misclassified all 30 changed C# files as documentation ("Core logic changes: 0 files"), which would have silently disabled per-language coverage enforcement in the review-termination hook.

Verdict: **CONDITIONAL — one blocking finding (new-file branch floor), remediation required.** See `remediation-inputs.2026-08-08T13-25.md`.

## Evidence Location Compliance

Scanned the full branch diff (76 files) for evidence written under non-canonical paths (`artifacts/baselines/`, `artifacts/qa/`, `artifacts/evidence/`, `artifacts/coverage/`): **zero occurrences** (`git diff --name-only 003c5715..HEAD | grep -cE "^artifacts/"` → 0). All executor evidence lives under the canonical `docs/features/active/2026-08-07-quickfiler-search-keystroke-focus-steal-438/evidence/{baseline,regression-testing,qa-gates,other}/` tree. The script `validate_evidence_locations.py` does not exist in this repository (verified); the scan above was performed directly. **PASS.**

## 1. General Unit Test Policy Compliance

| Check | Verdict | Evidence |
|---|---|---|
| Independence / isolation / determinism of new tests | PASS | 55 new MSTest tests across 7 new files; Moq seams and host-neutral harnesses; no shared mutable state observed in review; full run green in one attempt (`evidence/qa-gates/test-coverage-final.2026-08-08T11-41.md`) |
| No external dependencies, no temporary files | PASS | Grep of all 7 new test files for `Thread.Sleep`, `Task.Delay`, `GetTempFileName`, `GetTempPath`, `WinFormsPumpHost`, `Application.DoEvents`, `Show()` → zero matches |
| Arrange–Act–Assert and documented intent | PASS | Sampled `QfcItemController.SearchFocusRegressionTests.cs`, `BreadcrumbSelectionSessionHighlightTests.cs`, rewritten method in `QfcItemController.EventHandlersTests.cs` — AAA sections and descriptive names throughout |
| Scenario completeness (positive/negative/edge/error) | PASS | Empty result set, banner-only set, closed session, null input, disposed pipeline, negative index, index-beyond-last all covered (test inventory, Appendix A) |
| No test weakened, disabled, or relaxed | PASS | Full test-file diff review: the single sanctioned method rewrite (see §7) strengthens assertions (adds captured-query assertion and three `Times.Never()` negatives); the three fake edits and two `partial` tokens alter no test method; no `[Ignore]`, no removed assertions anywhere in the diff |

### 1.2 Coverage floors (uniform tier rule: line >= 85%, branch >= 75%)

#### 1.2.1 Per-language comparison

- **C# (repo-wide)** — coverage artifact `artifacts/csharp/coverage.xml` (Cobertura, byte-identical to committed `evidence/qa-gates/coverage-final.cobertura.xml`). Baseline: line 85.8261% / branch 79.2082%. Post-change: line 85.8665% / branch 79.2502%. Change: +0.0404 line / +0.0420 branch points. New/changed-code coverage: 95.24% minimum member line coverage (12 of 14 members at 100%). Disposition: **PASS** — repo-wide C# line coverage 85.87% >= 85% and branch coverage 79.25% >= 75%, both improved vs baseline. Evidence: independent parse of the Cobertura root `<coverage>` element by this reviewer, matching `evidence/qa-gates/coverage-delta.2026-08-08T11-41.md`.
- **C# (new files)** — Baseline: no prior figures (files added by this branch). Post-change: 5 of 6 new production files at 100% line with branch 87.5–100%; `BreadcrumbItemViewerLifecycleCoordinator.Search.cs` at 100% line / 50.00% branch (2/4). Change: new code. Disposition: **FAIL** — one new file below the 75% branch floor; blocking; remediation item R1. Evidence: reviewer per-file parse of `artifacts/csharp/coverage.xml` (lines 40 and 42 each `50% (1/2)`).
- **C# (modified files)** — Baseline: `QfcItemController.EventHandlers.cs` 79.57% line / 65.00% branch (74/93, 13/20). Post-change: 78.65% line / 61.11% branch (70/89, 11/18). Change: −0.92 line points via denominator shrinkage (4 covered lines deleted; uncovered-line count identical at 19). New/changed-code coverage: 100% — every changed line in the `TextBoxSearch_TextChanged` region has hits >= 1. Disposition: **FAIL** on the 85%/75% modified-file floor, dispositioned non-blocking: pre-existing sub-floor condition, zero changed-line regression, identical uncovered-line set; remediation item R2 records the residual for maintainer disposition. All other modified production files measure above the floors (see §5). Evidence: reviewer parse of baseline and final Cobertura per-file line tables.
- **TypeScript** — zero changed files on this branch; no comparison performed. Baseline: n/a. Post-change: n/a. Disposition: not evaluated (no changed files).
- **Python** — zero changed files on this branch; no comparison performed. Baseline: n/a. Post-change: n/a. Disposition: not evaluated (no changed files).
- **PowerShell** — zero changed files on this branch; no comparison performed. Baseline: n/a. Post-change: n/a. Disposition: not evaluated (no changed files).

Coverage checklist:
- [x] TypeScript coverage: no TypeScript files changed on this branch (verdict not required for languages with zero changed files)
- [x] PowerShell coverage: no PowerShell files changed on this branch (verdict not required for languages with zero changed files)
- [x] C# repo-wide coverage: PASS (85.87% line / 79.25% branch)
- [x] C# new/modified per-file coverage: FAIL (two per-file floor findings, R1 blocking / R2 dispositioned)

## 2. General Code Change Policy Compliance

| Check | Verdict | Evidence |
|---|---|---|
| Bugfix workflow (failing regression test first) | PASS | `evidence/regression-testing/fail-before.2026-08-08T11-41.md`: 5 tests discovered, 4 FAIL pre-fix with Moq `MockException`, EXIT 1; `fail-before-exception` dossier covers new-seam criteria whose members did not compile pre-change; `pass-after`: 180/180 |
| Minimal, targeted fix; no opportunistic refactor | PASS | Production diff confined to the 12 spec-named files + 6 new partials; the only relocation (`OpenAsync` body, `FocusCurrentSurface`) is verbatim-plus-guard and motivated by the 500-line ceiling |
| 500-line file limit (production and test, at head) | PASS | Reviewer count via `awk 'END{print NR}'` on all 30 changed `.cs` files: maximum 499 (`BreadcrumbDropDownHostTests.cs`, unchanged length: 1 insertion / 1 deletion). All new files <= 394 lines |
| Fail fast / explicit error handling | PASS | `ArgumentNullException` on null items (router and coordinator); disposed-pipeline no-op documented and tested; no broad catch added |
| Logging | PASS | No new logging surface; existing boundaries unchanged (spec-conformant) |
| Approved dependencies only | PASS | No `packages.config`, no new package references; csproj diffs are `<Compile Include>` wiring only |
| Public surface minimal and intentional | PASS | 4-parameter `OpenAsync` implemented explicitly on the concrete host (D11), keeping one public `OpenAsync`; new session/router members are the minimal spec-mandated seams |
| Supporting documents updated | PASS | `issue.md` Outcome section, `spec.md` check-offs, runbook, and full evidence tree updated on-branch |

## 3. Language-Specific Code Change Policy Compliance

C# is the only language with changed files.

| Check | Verdict | Evidence |
|---|---|---|
| CSharpier formatting (check-only) | PASS | `evidence/qa-gates/final-format.2026-08-08T11-41.md` — csharpier 1.2.6 `format` + `check`, EXIT 0, final uninterrupted pass |
| .NET analyzers (`EnableNETAnalyzers`, `EnforceCodeStyleInBuild`) | PASS | `evidence/qa-gates/final-analyze.2026-08-08T11-41.md` — msbuild EXIT 0, zero errors |
| Nullable analysis, warnings as errors | PASS | `evidence/qa-gates/final-nullable.2026-08-08T11-41.md` — msbuild `/p:Nullable=enable /p:TreatWarningsAsErrors=true`, EXIT 0; all 6 new production files declare `#nullable enable` |
| net481 constraints (no `init`/`record`) | PASS | New files use plain classes/partials; no C#9+ syntax outside the supported set |
| Additive-only contract changes (spec AC-10) | PASS | Diff of `IItemViewer.cs` (+10/−0) and `IBreadcrumbDropDownHost.cs` (+26/−0): exactly one new member and one new overload, both purely additive; 3-parameter `OpenAsync` delegates with `takeFocus: true`; internal `BreadcrumbDropDownOpenLifetime.OpenAsync` signature change is internal-only with all in-repo callers updated |
| XML documentation on non-obvious public APIs | PASS | New interface members, overloads, and partials carry full XML docs including rationale |

## 4. Language-Specific Unit Test Policy Compliance

| Check | Verdict | Evidence |
|---|---|---|
| MSTest framework | PASS | All 7 new test files use `[TestClass]`/`[TestMethod]` |
| Moq for mocking | PASS | `Mock<IFolderSearchHandler>`, `Mock<IItemViewer>` at the controller seam; hand-written recording fakes (pre-existing pattern) at the host/coordinator seams |
| FluentAssertions | PASS | `.Should()` assertions throughout new tests and the rewritten method (`capturedQuery.Should().Be("*query*")`) |
| No xUnit/NUnit introduced | PASS | Grep of new files: no `Xunit`/`NUnit` references |
| Legacy csproj `<Compile Include>` wiring | PASS | All 13 new `.cs` files (6 production, 7 test) have explicit entries; reviewer matched each numstat addition to a csproj entry |

## 5. Test Coverage Detail

Reviewer-computed per-file figures from `artifacts/csharp/coverage.xml` (identical to the committed final Cobertura):

| File | Status | Line | Branch | Floor verdict |
|---|---|---:|---:|---|
| `BreadcrumbBridgeCoordinator.Search.cs` | new | 100% (32/32) | 93.75% (15/16) | PASS |
| `BreadcrumbDropDownHost.Open.cs` | new | 100% (14/14) | 100% (4/4) | PASS |
| `BreadcrumbDropDownOpenLifetime.Focus.cs` | new | 100% (19/19) | 87.50% (7/8) | PASS |
| `BreadcrumbItemViewerLifecycleCoordinator.Search.cs` | new | 100% (5/5) | **50.00% (2/4)** | **FAIL — R1 (blocking)** |
| `BreadcrumbSelectionSession.Highlight.cs` | new | 100% (11/11) | 100% (6/6) | PASS |
| `FolderBreadcrumbBridgeRouter.SearchPresentation.cs` | new | 100% (29/29) | 100% (4/4) | PASS |
| `QfcItemController.EventHandlers.cs` | modified | **78.65% (70/89)** | **61.11% (11/18)** | **FAIL — R2 (pre-existing floor miss; zero changed-line regression; dispositioned non-blocking)** |
| `BreadcrumbDropDownOpenCoordinator.cs` | modified | 98.31% (233/237) | 91.84% (90/98) | PASS |
| `BreadcrumbDropDownHost.cs` | modified | 99.29% (279/281) | 91.18% (93/102) | PASS |
| `BreadcrumbDropDownOpenLifetime.cs` | modified | 99.07% (318/321) | 92.50% (74/80) | PASS |
| `BreadcrumbBridgeCoordinator.cs` | modified | 100% (280/280) | 87.36% (76/87) | PASS |
| `FolderBreadcrumbBridgeRouter.cs` | modified | 98.38% (303/308) | 91.43% (64/70) | PASS |
| `BreadcrumbSelectionSession.cs` | modified | 100% (316/316) | 96.36% (106/110) | PASS |
| `BreadcrumbItemViewerLifecycleCoordinator.cs` | modified (partial-token only) | 90.57% (288/318) | 66.44% (97/146) | Bit-identical to baseline (90.57%/66.44%); one-token diff; pre-existing branch debt unchanged; no regression |
| `ItemViewer.Breadcrumb.cs`, `ItemViewer.FolderSearch.cs` | modified | not measured | not measured | `[ExcludeFromCodeCoverage]` `ItemViewer` partial per the ratified CLAUDE.md § UT2 COM/VSTO/WinForms exemption; new members are thin forwarding, exercised end-to-end by `BreadcrumbDropDownSearchIntegrationTests` |
| `IItemViewer.cs`, `IBreadcrumbDropDownHost.cs` | modified | no executable code | no executable code | Interface-only files; legitimately absent from measurement per `.claude/rules/general-unit-test.md` |

Additional verifications:

- **`BeginOpenCore` single uncovered line is pre-existing:** baseline Cobertura shows `BeginOpenCore` 15/16 with line 187 (`return ClosedTask;`) uncovered; post-change 20/21 with the same statement at line 221 uncovered (+34-line offset from the insertion). Every added line (226, 227, 238–240) has hits >= 1. Claim in `coverage-delta.2026-08-08T11-41.md` independently confirmed.
- **R1 root cause:** in `BreadcrumbItemViewerLifecycleCoordinator.Search.cs`, lines 40 (`_openCoordinator?.LatchNextOpenTakesNoFocus();`) and 42 (`_bridgeCoordinator?.PresentSearchResults(items);`) each measure `50% (1/2)`: only the non-null arms execute. The file's own XML remark documents the no-open-coordinator fallback ("performs no Focus call at all"), but no test constructs the coordinator in that configuration.

## 6. Test Execution Metrics

- Final full run: **6348/6348 passed, 0 failed**, EXIT 0, 40.76 s, 9 first-party test assemblies, no `\.claude\` worktree assembly collected (`evidence/qa-gates/test-coverage-final.2026-08-08T11-41.md`).
- Baseline run: 6293 passed → **+55 tests** added by this branch.
- Scoped gate runs (all with non-vacuous filters, see §7 D10): P2 78/78; P3 128/128; P4 24/24; fail-before 4-of-5 failing pre-fix (expected); pass-after 180/180.
- Pre-existing flaky tests (`WpfDispatcherYieldTests.YieldAsync_WithoutDispatcher_RemainsStrict`, `QfcItemController_InitializationTests.*ThroughThePumpHost*`) did not fail in the final run; both are attributed to CPU-saturation and files untouched by this branch (attribution verified: neither file is in the diff).

## 7. Code Quality Checks

| Check | Verdict | Evidence |
|---|---|---|
| D10 vacuous-filter remediation (all gates select > 0 tests) | PASS | Every executed `TestCaseFilter` in the evidence tree uses the underscore class-name form or a genuine type-name substring; each gate log records a non-zero `Total tests`; the dotted form appears only as the documented counter-example ("No test matches ... EXIT_CODE 0") in `fail-before.2026-08-08T11-41.md` |
| Single sanctioned test-method rewrite (spec AC-11) | PASS | Exactly one test method renamed/rewritten (`TextBoxSearch_TextChanged_UsesInjectedFolderSearchHandler_PresentsSearchResultsWithoutFocusOrCommit`); it re-asserts all four durable behaviors and adds three negative assertions; no assertion weakened |
| Sanctioned structural test edits only | PASS | Three additive 4-parameter fake members (`ControlledHost`, `RecordingHost`, `RecordingDropDownHost`), each delegating from the 3-parameter member with `takeFocus: true` and recording intent; two one-token `partial` keywords; nothing else in existing test files |
| Deviations D10–D14 reviewed | PASS | All five are command-string, sequencing, or structural-enabler deviations recorded in `scope-guard`/`p3-gate` evidence; none alters an acceptance criterion, assertion, or scope boundary |
| Modified-workflow green-run rule | Not triggered | No diff under `.github/workflows/**`, `scripts/benchmarks/**`, or `.github/actions/**` (verified via `git diff --name-only`) |
| Comment quality (why, not what) | PASS | Load-bearing ordering, latch semantics, and overload-dispatch rationale documented at each decision point |

## 8. Gaps and Exceptions

1. **R1 (blocking):** new-file branch floor miss in `BreadcrumbItemViewerLifecycleCoordinator.Search.cs` (50% < 75%). Remediation: add unit test(s) exercising `PresentSearchResults` with no open coordinator and/or no bridge coordinator. See `remediation-inputs.2026-08-08T13-25.md` R1.
2. **R2 (dispositioned non-blocking):** `QfcItemController.EventHandlers.cs` below the modified-file floor, pre-existing at baseline, zero changed-line regression. Residual recorded for maintainer disposition (cover remaining handlers or record an exemption). See remediation inputs R2.
3. **Tooling (corrected in place):** the generated `artifacts/pr_context.summary.txt` "Changed files overview" misclassified all 30 changed C# files as documentation ("Core logic changes: 0 files"). This is the recurring PR-context classifier defect (previously observed on #171, #181, #244, #251, #253, #270, #278, #283, #208, #292, #328, #418, #424). Corrected by appending the full C#/.csproj enumeration to the summary; the underlying classifier defect remains open as a tooling issue.
4. **Pre-existing, confirmed at merge-base:** `UtilitiesCS.Test.csproj` duplicate `<Compile Include>` for `PercentageFormatterTests.cs` (lines 302 and 354 at merge-base; CS2002). Untouched by this branch; the promised promotion to its own issue is still outstanding.
5. **Pre-existing:** `WinFormsPumpHost`-based tests are load-flaky and display a visible window; files untouched by this branch.
6. **HV-1:** documented human-verification exception (live-Outlook typing check), explicitly not a merge gate per spec; runbook exists at `runbooks/verify-search-focus-retention.runbook.md`. A negative outcome is promoted as a new issue.
7. **MCP template/validators:** `resolve_policy_audit_template_asset` and `validate_orchestration_artifacts` are not available in this session; artifact shapes follow the skill-documented canonical headings, and the repository's operative gate (`.claude/hooks/validate-feature-review-coverage.ps1`) was simulated against this artifact before finalization.

## 9. Summary of Changes

- 18 production `.cs` files changed (14 QuickFiler, 4 UtilitiesCS): one behavior flip in `TextBoxSearch_TextChanged`; two additive contract members; a latch on the open coordinator; a `takeFocus` guard threaded host → lifetime; session-preserving row replacement and pending-only highlight in the router/session; non-focusing viewer/coordinator paths; five `partial`-token conversions and two verbatim body relocations to stay under the 500-line ceiling.
- 12 test `.cs` files changed: 7 new test files (+55 tests), one sanctioned method rewrite, three additive fake members, two `partial` tokens.
- 4 csproj files: `<Compile Include>` wiring only.
- Zero workflow, package, configuration, or persisted-state changes. EfcViewer search path has zero diff (spec AC-13).

## 10. Compliance Verdict

**CONDITIONAL PASS — one blocking finding.** All policy sections pass except the per-file coverage gate: R1 (new-file branch floor, blocking) requires remediation; R2 is a pre-existing floor miss with zero changed-line regression, recorded for disposition. Repo-wide C# coverage, toolchain, test execution, contract additivity, file-size, and evidence-location checks all pass. Recommendation: run the R1 remediation cycle (small, test-only) before PR creation.

## Appendix A: Test Inventory

New test files (55 tests total; per-suite counts from the executor's gate logs, method inventory verified by reviewer grep):

| File | Methods | Seam |
|---|---|---|
| `QuickFiler.Test/Controllers/QfcItemController.SearchFocusRegressionTests.cs` | 8 | Controller (`Mock<IItemViewer>` + `Mock<IFolderSearchHandler>`) — AC-1, AC-5, AC-6 |
| `QuickFiler.Test/Viewers/BreadcrumbDropDownHostTests.Part2.cs` | 8 | Real-host delegate counts (`FocusPendingCount`/`FocusAnchorCount`) — AC-2 |
| `QuickFiler.Test/Viewers/BreadcrumbDropDownOpenCoordinatorTests.Part3.cs` | 6 | Open-coordinator latch FIFO — AC-2, AC-3 |
| `QuickFiler.Test/Viewers/BreadcrumbDropDownSearchIntegrationTests.cs` | 9 | Headless viewer + `Mock<IBreadcrumbDropDownHost>` harness — AC-3, AC-4, AC-8, AC-9 |
| `QuickFiler.Test/Viewers/BreadcrumbDropDownSearchIntegrationTests.Part2.cs` | 3 | Same harness — AC-6, error paths |
| `UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbSelectionSessionHighlightTests.cs` | 11 | Host-neutral session — AC-4, AC-5, AC-9 |
| `UtilitiesCS.Test/OutlookObjects/Folder/FolderBreadcrumbBridgeRouterReplaceItemsTests.cs` | 10 | Host-neutral router — AC-3, AC-8, AC-9 |

Modified existing test files: `QfcItemController.EventHandlersTests.cs` (one sanctioned rewrite), `BreadcrumbDropDownHostTests.cs` (+`partial`), `BreadcrumbDropDownOpenCoordinatorTests.cs` / `BreadcrumbItemViewerLifecycleCoordinatorTests.cs` / `BreadcrumbSelectorOpenRetryTests.cs` (additive fake members).

## Appendix B: Toolchain Commands Reference

Commands executed by the executor in the final uninterrupted pass (evidence under `evidence/qa-gates/`), verified check-only/reproducible by this reviewer from the evidence logs:

1. Format: `./.dotnet-sdk/dotnet.exe tool run csharpier format .` then `... csharpier check .` — EXIT 0 (csharpier 1.2.6; D1: v1 subcommand form)
2. Analyze: `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` — EXIT 0
3. Type-check: `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true` — EXIT 0
4. Test + coverage: `./scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot . -CoverageOutput 'docs/features/active/2026-08-07-quickfiler-search-keystroke-focus-steal-438/evidence/qa-gates/coverage-final.cobertura.xml'` — EXIT 0, 6348/6348

Reviewer verification commands (check-only, this session):

- `git merge-base HEAD origin/main` → `003c5715...` (base confirmed)
- `git diff --numstat 003c5715..HEAD` (scope enumeration)
- `awk 'END{print NR}'` per changed `.cs` file (500-line audit)
- Python `xml.etree` parse of `artifacts/csharp/coverage.xml`, `evidence/baseline/coverage-baseline.cobertura.xml`, `evidence/qa-gates/coverage-final.cobertura.xml` (repo-wide, per-file, per-member, per-line verification)
- `git show 003c5715:UtilitiesCS.Test/UtilitiesCS.Test.csproj | grep -n PercentageFormatterTests` (pre-existing CS2002 attribution)
