# Policy Compliance Audit — Issue #614 (efc-store-root-selection-leaks-full-outlook-path-into-filing-boundary)

- Artifact timestamp: 2026-08-26T16-55
- Reviewer: feature-review agent
- Component: `UtilitiesCS`, `QuickFiler`, `TaskMaster` (VSTO add-in, .NET Framework 4.8.1)
- Branch: `bug/efc-store-root-selection-leaks-full-outlook-path-into-filing-boundary-614`
- Base branch (resolved): `main`
- Merge-base SHA: `c279d40bddacdba00c29a9724d1b5b17f9ebbc90`
- Head SHA: `02092504e50ede2527ae35f14629f0bc4c4c94ff` (21 commits ahead; working tree clean)
- Work Mode: `full-bug` (marker read from `issue.md`) — acceptance-criteria source is `spec.md` only
- Scope: full branch diff against the resolved base branch — 80 files, +7599 / -158

## Template Provenance

`policy-audit-template-usage` requires resolving the artifact template through the MCP tool
`mcp__drm-copilot__resolve_policy_audit_template_asset`. No MCP tool is exposed to this agent
session, and no policy-audit template asset file exists anywhere in this checkout
(`find . -iname "*policy-audit*template*"` returns only skill directories, no asset). The canonical
major section set named by the skill is therefore reproduced from the skill text itself. This
substitution is recorded here rather than marking the artifact BLOCKED, because every canonical
section below is populated with evidence.

## Executive Summary

The change fixes a nine-defect path-representation chain (D1–D9) that allowed a full Outlook store
path to reach the EFC filing boundary. It introduces one shared pure contract type
(`ArchiveStemContract`), two small guard types (`EfcSelectionGuard`, `ArchiveRootPathGuard`), and
routes ten production files through them. Ten new test files add 80 test methods.

The four-step C# toolchain was re-run independently by this reviewer and passed in the mandated
order. All 6093 tests in the three test assemblies this change touches pass. New-code line coverage
is 100% on every type and method the change introduces.

One FAIL is recorded: repository-wide C# line coverage measured from the canonical coverage artifact
is 84.8696%, below the 85% floor in `.claude/rules/quality-tiers.md`. The shortfall is pre-existing
and this change **improves** it by +0.0899 points. The FAIL is dispositioned non-blocking (see § 5).

Two PARTIAL results are recorded (file-size ceiling under a reinterpreted reading; manual validation
not executed). Neither is a code defect. No blocking finding was identified.

**Overall verdict: PASS with 1 non-blocking FAIL and 2 non-blocking PARTIAL results. 0 blocking findings.**

## Rejected Scope Narrowing

The caller prompt supplied a base branch, a merge-base SHA, an active feature folder, an AC source,
and a pointer to the coverage artifact. It explicitly stated "Scope determination is yours." No
instruction in the caller prompt attempted to narrow the audit to a plan, task, phase, or file
subset, and no instruction attempted to mark any language's coverage as informational, contextual,
or inapplicable.

One caller assertion was verified rather than accepted:

> "`artifacts/csharp/coverage.xml` exists and is current for this branch head. It was generated from
> the post-change filtered Cobertura output and reports LINE 53972/63594 and BRANCH 12741/16162.
> Measure and interpret it yourself."

Verified independently. The artifact root element is `<report name="TaskMaster">` (a JaCoCo-shaped
summary, not raw Cobertura). Re-summing its nine `<package>` LINE counters yields exactly
53972 covered / 63594 valid, and its single BRANCH counter yields 12741 covered / 16162 valid. The
caller's figures are accurate. The caller's framing did **not** state the resulting verdict, and the
verdict this reviewer derives (below the 85% floor) is recorded as FAIL in § 5.

**No scope narrowing was detected and none was applied. The audit covers the full feature-vs-base diff.**

## PR Context Artifacts

`artifacts/pr_context.summary.txt` and `artifacts/pr_context.appendix.txt` were **absent** at the
start of this review. Per `pr-context-artifacts`, both were regenerated against the resolved base
branch. The drm-copilot `collect_pr_context` MCP tool is not exposed to this session, so both were
hand-authored from git plumbing (`git diff --numstat`, `git log`, `git diff`), using the canonical
`- <path> (+N/-N)` bullet shape for every one of the 80 changed paths. That shape avoids the
recurring collector defect in which C# files are dropped from the "Changed files overview" and the
coverage validator then detects zero changed languages.

Verified by dot-sourcing `.claude/hooks/validate-feature-review-coverage.ps1` and calling
`Get-ChangedLanguageSet` against the regenerated summary: the enumerated changed-language set is
exactly `CSharp`.

## Evidence Location Compliance

`validate_evidence_locations.py` does not exist in this repository. The equivalent scan was run
directly:

```
git diff --name-only c279d40b..HEAD | grep -E "^artifacts/(baselines?|qa|qa-gates|evidence|coverage|regression-testing|post-change)/"
```

Result: **no matches**. All 30 evidence artifacts are under the canonical
`docs/features/active/<feature>/evidence/{baseline,regression-testing,qa-gates,issue-updates,other}/`
tree. Verdict: **PASS**. No `EVIDENCE_LOCATION_OVERRIDE_REJECTED` record was required.

## modified-workflow-needs-green-run

`git diff --name-only c279d40b..HEAD` contains zero paths matching `.github/workflows/**`,
`.github/actions/**`, or `scripts/benchmarks/**`. The rule does not fire, and no Blocking finding is
emitted under it.

## 1. General Unit Test Policy Compliance

| Rule | Verdict | Evidence |
| --- | --- | --- |
| Independence | PASS | All 80 new test methods construct their own subject. No `[AssemblyInitialize]`/`[ClassInitialize]` shared mutable state was added. One order-dependency defect *was* found by the executor during Phase 9 (a shared log4net `MemoryAppender` bound per-type) and fixed by replacing exact-count assertions with existence assertions; recorded in `evidence/qa-gates/toolchain-clean-pass.2026-08-26T19-58.md`, attempt 2. |
| Isolation | PASS | Each test targets one method. `ArchiveStemContractTests` (23), `FolderConverterIssue614Tests` (19), `EfcSelectionGuardTests` (9), `BreadcrumbBridgeRouterIssue614Tests` (8), `EfcDataModelIssue614Tests` (8), `AppFileSystemFolderPathsOneDriveResolutionTests` (7), `AppOlObjectsArchiveRootValidationTests` (6). |
| Fast execution | PASS | Reviewer-run: UtilitiesCS.Test 4748 tests / 19 s; QuickFiler.Test 964 / 5 s; TaskMaster.Test 381 / 1 s. |
| Determinism | PASS | Grep of all new and modified test files for `Thread.Sleep`, `Task.Delay`, `DateTime.Now`, `DateTime.UtcNow`, `Random`, `Environment.SetEnvironmentVariable`, `Path.GetTempPath`, `Path.GetTempFileName`, `File.WriteAllText`: **zero hits**. Environment reads go through an injected `Func<string,string>` delegate; no test mutates process environment state. |
| Readability / AAA | PASS | Every new test carries explicit `// Arrange` / `// Act` / `// Assert` comments and a behaviour-describing name. FluentAssertions `because` strings supply actionable failure text. |
| No temporary files | PASS | Zero filesystem writes in any new test; verified by the grep above. |
| No external dependencies | PASS | Outlook COM is mocked (`Mock<IOlObjects>`, `Mock<IFolderHierarchyProvider>`, `Mock<IBreadcrumbWebHost>`). The pure contract types touch no I/O by construction. |
| Test file location (mirrored tree) | PASS | All ten new test files are in the mirrored `*.Test` project trees (`UtilitiesCS.Test/OutlookObjects/Folder/`, `QuickFiler.Test/Controllers/`, `TaskMaster.Test/AppGlobals/`). No colocation in a production source tree. |
| Coverage exclusion policy | PASS | `git diff \| grep "^+.*ExcludeFromCodeCoverage"` returns **no added occurrences**. No production path was added to any coverage `exclude` list. |
| Scenario completeness | PASS | Positive, negative, boundary (`Archive2` near-miss, separator-only root, sub-three-character ancestor, UNC ancestor, case-differing ancestor), and error-message-content scenarios are all covered. |
| Coverage thresholds | See § 5 | Repo-wide line coverage FAIL (non-blocking); new-code and changed-line gates PASS. |

## 2. General Code Change Policy Compliance

| Rule | Verdict | Evidence |
| --- | --- | --- |
| Simplicity first | PASS | The nine defects are collapsed onto one 147-line pure static contract type rather than nine local fixes. |
| Reusability | PASS | `ArchiveStemContract.TryMakeArchiveRelative` is the single prefix-anchored, separator-terminated, `OrdinalIgnoreCase` implementation, consumed by `BreadcrumbBridgeRouter`, `EmailFilerConfig`, `EfcDataModel`, and `FolderConverter`. Four previously divergent ad-hoc `Replace`/`Contains`/`Substring` implementations were deleted. |
| Extensibility | PASS | Guards are additive; no public interface was widened. |
| Separation of concerns | PASS | `ArchiveStemContract`, `EfcSelectionGuard`, and `ArchiveRootPathGuard` contain zero I/O, COM, or UI types. `ArchiveRootPathGuard` takes the two already-resolved strings plus an `Action<string>` sink so the COM property reads stay in the caller. |
| Error handling — fail fast | PASS | Every rejection path throws a specific `ArgumentException`/`InvalidOperationException` or returns a documented `false`. No broad catch was added. |
| Logging pattern | PASS | Rejections use the existing per-type `log4net.ILog` (`log.Error` in the router, `logger.Error` in `AppFileSystemFolderPaths`). No `Console.WriteLine` was added. |
| Contracts / invariants | PASS | `EmailFilerConfig.ResolvePaths` (both overloads) enforces the stem contract before concatenation; `EfcDataModel.ToArchiveRelativeStem` enforces it before filing. |
| Module cohesion | PASS | Each new type has one responsibility. |
| **File size <= 500 lines** | **PARTIAL** | Every new file and every in-scope edited file with a sub-500 baseline finishes under 500. Three files remain over the ceiling: `QuickFiler/Controllers/EfcFormController.cs` (1084 -> **1072**), `QuickFiler/Controllers/BreadcrumbBridgeRouter.cs` (596 -> **596**), `QuickFiler.Test/Controllers/BreadcrumbBridgeRouterIssue439Tests.cs` (694 -> **694**). All three exceeded the ceiling before this change and none grew. Counted independently with `awk 'END{print NR}'`. See § 8 GAP-1. |
| Naming | PASS | `PascalCase` types and public members, `camelCase` locals, descriptive names throughout. |
| Docs / comments | PASS | Every new public and internal member carries XML documentation stating the contract and the `#614` defect it addresses; comments explain *why*, not *what*. |
| Public API compatibility | PASS (breaking change, declared) | `FolderConverter.ToFsFolderpath(this string, string, string, bool ask = true)` lost its `ask` parameter. A repo-wide search found 11 call sites, none supplying the argument positionally or by name; the whole-solution rebuild exits 0. The removal and its rationale are recorded in `change-description.2026-08-26.md` § (d). |
| Dependencies | PASS | One reference added: `log4net 3.3.2` to `QuickFiler.Test` (already the pinned repository version, needed to assert the new router diagnostics via `MemoryAppender`). No new third-party package version was introduced. |
| I/O boundaries | PASS | Core decision logic is testable without filesystem, network, or COM. |
| Bugfix workflow — failing test first | PASS | `evidence/regression-testing/p1-t2-primary-regression-fail-before.2026-08-26T11-45.md` (EXIT_CODE 1, ExpectedExitCode 1, 1 failed / 0 passed) and `p1-t4-producer-companion-fail-before.2026-08-26T16-05.md` (EXIT_CODE 1, 1 failed / 0 passed) both predate the fix commits `33bcd218` / `cee78979`. |
| **Mandatory toolchain loop** | PASS | See § 7 and Appendix B; independently re-executed by this reviewer. |

## 3. Language-Specific Code Change Policy Compliance (C#)

| Rule | Verdict | Evidence |
| --- | --- | --- |
| C#1.1 CSharpier formatting via `dotnet tool run` | PASS | Reviewer ran `dotnet tool run csharpier check .` -> `Checked 1530 files in 3954ms.`, exit 0. Manifest-pinned version; no global install invoked. |
| C#1.2 .NET analyzers with `/t:Rebuild` | PASS | Reviewer ran the exact CLAUDE.md command. Exit 0, 0 errors, 18 project DLL outputs produced by the rebuild (non-vacuous). The only warnings are 5 pre-existing `System.Reactive.PackagesConfigCheck` advisories, also present on `main`. |
| C#1.3 Nullable / type check with `/t:Rebuild`, no `/p:Nullable=enable` | PASS | Reviewer ran the exact CLAUDE.md command. Exit 0, **0** `CS86xx` diagnostics, 0 errors, 18 DLL outputs. `/p:Nullable=enable` was not added and `/t:Build` was not substituted. |
| C#2.1 Strong contracts | PASS | All new public and internal members use explicit types at boundaries; `var` only where the type is evident. |
| C#2.2 Null-safety | PASS | `ArchiveStemContract.cs` and `EfcSelectionGuard.cs` carry `#nullable enable`; `FolderConverter.FindInvalidSegmentRule` returns `string?`; `BreadcrumbBridgeRouter.ToHierarchyPath` returns `string?`. Guard clauses precede every dereference. |
| C#2.3 Composition, focused types | PASS | Three new static utility types; no inheritance added. |
| C#2.4 Async / resource safety | PASS | No new async or disposable resource was introduced. |
| C#4 Exceptions, logging, contracts | PASS | Fail-fast with named parameters; the diagnostic is logged before the throw in `ArchiveRootPathGuard` and `ResolveOneDriveRoot` so the failure is recorded even if a caller swallows the exception. |
| C#5 Module structure, minimal surface | PASS | `EfcSelectionGuard` and `ArchiveRootPathGuard` are `internal`; only `ArchiveStemContract` is `public`, because it is consumed across assembly boundaries. |
| C#6 Naming, XML docs, why-comments | PASS | Verified by reading all three new production files end to end. |
| C#7 Analyzer configuration | PASS | No suppression, `#pragma warning disable`, or `[SuppressMessage]` was added. Verified by diff grep. |
| Every new file registered with an explicit `<Compile Include>` | PASS | `UtilitiesCS.csproj` (+1), `QuickFiler.csproj` (+1), `TaskMaster.csproj` (+1), `QuickFiler.Test.csproj` (+3), `TaskMaster.Test.csproj` (+2), `UtilitiesCS.Test.csproj` (+2) — 10 new files, 10 `<Compile Include>` entries. |

## 4. Language-Specific Unit Test Policy Compliance (C#)

| Rule | Verdict | Evidence |
| --- | --- | --- |
| CUT1 MSTest framework | PASS | Every new test file uses `[TestClass]` / `[TestMethod]` from `Microsoft.VisualStudio.TestTools.UnitTesting`. No xUnit or NUnit reference was added. |
| CUT2 Moq for mocking | PASS | `Mock<IOlObjects>`, `Mock<IFolderHierarchyProvider>`, `Mock<IBreadcrumbWebHost>` (several with `MockBehavior.Strict`). Plain `Func<string,string>` lambdas are used where a delegate seam is simpler than a mock, which the policy permits. |
| CUT2 FluentAssertions | PASS | `.Should()...` used throughout; no bare MSTest `Assert` in the new files. |
| CUT3 Toolchain command selection | PASS | See Appendix B. |
| Existing tests treated as spec | PARTIAL (documented) | Two existing assertion sets were deliberately rewritten because they codified the defects under fix: `FolderConverterTests.cs:329` (`BeEmpty()` -> `Be("BadName")`, defect D5f) and `BreadcrumbBridgeRouterIssue439Tests.Issue439ArchiveRootBoundarySelectionAndHostEventRemainDeterministic` (three assertions, defects D1/D9). Both were pre-registered in `plan.2026-08-26T09-59.md` (P5-T4; P3-T4 under execution delta E1), both are quoted old-versus-new with rationale in `change-description.2026-08-26.md` items (c) and (h), and both make the assertions **more** specific rather than weaker. See § 8 GAP-3. |
| Test policy self-audit performed | PASS | `evidence/qa-gates/p10-t22-test-policy-audit.2026-08-26T20-05.md`. |

## 5. Test Coverage Detail

The coverage artifact was inspected, not regenerated, per the review contract.

- Artifact: `artifacts/csharp/coverage.xml`. Root element `<report name="TaskMaster">` (JaCoCo-shaped summary of the post-change filtered Cobertura output).
- Reviewer re-summed the nine `<package>` LINE counters: covered 53972, missed 9622, valid 63594.
- Reviewer read the single BRANCH counter: covered 12741, missed 3421, valid 16162.
- Cross-check: `Get-JacocoRepoCoverage` and `Get-JacocoBranchCoverage` from `.claude/hooks/validate-feature-review-coverage.ps1`, dot-sourced and invoked directly, return `84.87` and `78.83`.

### Language coverage verdicts

C# repo-wide line coverage verdict: **FAIL** — 84.8696% (53972 / 63594) is 0.1304 points below the 85% floor.

C# repo-wide branch coverage verdict: **PASS** — 78.8331% (12741 / 16162) clears the 75% floor by 3.8331 points.

C# new-code line coverage verdict: **PASS** — every type and method introduced by this change measures 100.0000%.

C# changed-line coverage verdict: **PASS** — no changed line lost coverage relative to the merge-base baseline.

| Language | Changed files on branch | Artifact state | Repo-wide line | Repo-wide branch | Verdict |
| --- | ---: | --- | ---: | ---: | --- |
| C# | 22 `.cs` | present and parsed | 84.8696% | 78.8331% | FAIL on line, PASS on branch |
| TypeScript | 0 | no artifact expected | — | — | zero changed files, nothing to evaluate |
| Python | 0 | no artifact expected | — | — | zero changed files, nothing to evaluate |
| PowerShell | 0 | no artifact expected | — | — | zero changed files, nothing to evaluate |

#### Disposition of the C# repo-wide line FAIL

The FAIL is recorded honestly and is dispositioned **non-blocking**, on these grounds:

1. The shortfall is **pre-existing**. The merge-base baseline measured on the same machine, with the
   same counting method, is 84.7797% (53769 / 63422) — recorded in
   `evidence/qa-gates/coverage-delta.2026-08-26T19-50.md` § (a) and traceable to
   `evidence/baseline/test-coverage.2026-08-26T11-36.md`.
2. This change **improves** the figure by **+0.0899** points (84.7797% -> 84.8696%) and improves the
   branch figure by +0.1393 points (78.6938% -> 78.8331%). It moves the repository toward the floor,
   not away from it.
3. Every gate this change can control is met: new code at 100%, zero changed lines regressed, and
   all 18 uncovered changed lines individually enumerated with a COM, WinForms, or
   environment-binding justification.
4. Closing a 0.13-point repo-wide gap requires covering roughly 83 additional lines somewhere in a
   63594-line first-party denominator, in code this defect fix does not touch. Requiring that inside
   a bug fix would be an out-of-scope refactor.
5. A standing repository contradiction is noted: `CLAUDE.md` § UT2 states a `>= 80%` repo-wide floor
   while `.claude/rules/general-unit-test.md` and `.claude/rules/quality-tiers.md` state `>= 85%`.
   This audit applies the stricter 85% figure, which is also what the local validator enforces. At
   the `CLAUDE.md` 80% figure the same measurement would clear the floor.

No remediation-inputs artifact is produced for this FAIL. It is not a defect introduced by this
change, and remediating it is a repository-wide coverage-uplift task rather than a #614 task.

### New-code line coverage (gate `>= 90%`; source `evidence/qa-gates/coverage-delta.2026-08-26T19-50.md` § (c))

| Item | Line measurement | Verdict |
| --- | ---: | --- |
| `UtilitiesCS/OutlookObjects/Folder/ArchiveStemContract.cs` | 100.0000% (51/51) | PASS |
| `QuickFiler/Controllers/EfcSelectionGuard.cs` | 100.0000% (9/9) | PASS |
| `TaskMaster/AppGlobals/ArchiveRootPathGuard.cs` | 100.0000% (20/20) | PASS |
| `EfcDataModel.ToArchiveRelativeStem` | 100.0000% (12/12) | PASS |
| `EmailFilerConfig.ResolvePaths` (both overloads) | 100.0000% (25/25) | PASS |
| `EmailFilerConfig.GetStem` / `.IsDeleteRelevant` | 100.0000% (9/9, 10/10) | PASS |
| `FolderConverter.ToFsFolderpath` (all overloads) | 100.0000% (60/60) | PASS |
| `FolderConverter.ResolveOlRoot` | 100.0000% (23/23) | PASS |
| `FolderConverter.FindInvalidSegmentRule` / `.IsReservedDeviceName` / `.RemoveIllegalCharacters` | 100.0000% (22/22, 5/5, 4/4) | PASS |
| `AppFileSystemFolderPaths.ResolveOneDriveRoot` | 100.0000% (14/14) | PASS |
| `BreadcrumbBridgeRouter.SelectRow` / `.SelectHierarchyPath` / `.CommitSelection` / `.ToHierarchyPath` | 100.0000% (18/18, 14/14, 8/8, 10/10) | PASS |

### Changed-line measurement and no-regression (§ (d) and § (e) of the same artifact)

- 18 of 328 measurable added production lines are not exercised. All 18 are enumerated with a
  justification: WinForms UI event glue (`EfcFormController.cs:706, :1038`), live-COM property reads
  (`EfcDataModel.cs:345`, `AppOlObjects.cs:259-263`), and machine-dependent environment wiring
  (`AppFileSystemFolderPaths.cs:30-35, :37-39, :265`). Every pure branch introduced is exercised.
- Per-file covered counts are unchanged or higher in six of seven modified production files. The one
  decrease (`BreadcrumbBridgeRouter.cs`, -3 covered) is accompanied by a -3 valid-line change and an
  unchanged uncovered count of 8, i.e. three exercised lines were deleted, not abandoned. Reviewer
  arithmetic confirms the executor's reading.

## 6. Test Execution Metrics

Independently re-executed by this reviewer against head `02092504`:

| Assembly | Total | Passed | Failed | Skipped | Duration |
| --- | ---: | ---: | ---: | ---: | ---: |
| `UtilitiesCS.Test.dll` (net481) | 4748 | 4748 | 0 | 0 | 19 s |
| `QuickFiler.Test.dll` (net481) | 964 | 964 | 0 | 0 | 5 s |
| `TaskMaster.Test.dll` (net481) | 381 | 381 | 0 | 0 | 1 s |
| **Total (changed assemblies)** | **6093** | **6093** | **0** | **0** | — |

Exit code 0. A prior targeted run restricted to the changed subject areas (filter on `Issue614`,
`ArchiveStemContract`, `EfcSelectionGuard`, `ArchiveRootValidation`, `OneDriveResolution`,
`Issue439`, `Issue609`, `FolderConverter`, `BreadcrumbBridgeRouter`, `EmailFilerConfig`) returned
213 tests, 213 passed, 0 failed.

The executor's own full-solution run recorded 6569 total / 6569 passed / 0 failed
(`evidence/qa-gates/final-test-coverage.2026-08-26T19-45.md`). The 476-test difference is the test
assemblies this change does not touch (`ToDoModel.Test`, `TaskVisualization.Test`, and siblings),
which this reviewer did not re-run.

Fail-before evidence (bugfix workflow, § 2):

| Test | Pre-fix result | Artifact |
| --- | --- | --- |
| `Issue614_ResolvePaths_WithStoreRootStem_RejectsNonRelativeStemWithoutLeakingIdentifiers` | 1 run, 1 failed, EXIT_CODE 1 (ExpectedExitCode 1) | `evidence/regression-testing/p1-t2-primary-regression-fail-before.2026-08-26T11-45.md` |
| `Issue614_SegmentActivate_StoreRootSegment_DoesNotStoreFullOutlookPath` | 1 run, 1 failed, EXIT_CODE 1 (ExpectedExitCode 1) | `evidence/regression-testing/p1-t4-producer-companion-fail-before.2026-08-26T16-05.md` |

## 7. Code Quality Checks

| # | Gate | Command | Exit code | Verdict |
| --- | --- | --- | ---: | --- |
| 1 | Format | `dotnet tool run csharpier check .` | 0 | PASS |
| 2 | Lint / analyzers | `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` | 0 | PASS |
| 3 | Type check / nullable | `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true` | 0 | PASS |
| 4 | Tests | `vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll QuickFiler.Test\bin\Debug\QuickFiler.Test.dll TaskMaster.Test\bin\Debug\TaskMaster.Test.dll /Settings:TaskMaster.runsettings /InIsolation` | 0 | PASS |

Non-vacuity of gates 2 and 3: both used `/t:Rebuild`, and each produced 18 project DLL outputs, so
`CoreCompile` ran on every project rather than being skipped by the incremental up-to-date check.
Gate 2's only warnings are 5 pre-existing `System.Reactive.PackagesConfigCheck` advisories that are
also present on `main`; gate 3 emitted 0 `CS86xx` diagnostics.

Redaction and host-identifier sweep (this reviewer, independent of the executor's P8-T3 artifact):

```
git diff c279d40b..HEAD | grep -icE "<account>|<account-email>"   ->  0
```

All host identifiers in added lines are fabricated placeholders (`\\mailbox@example.com`,
`\\other@example.org`, `C:\Users\testuser\OneDrive - Contoso`). Verdict: **PASS**.

## 8. Gaps and Exceptions

**GAP-1 (PARTIAL, non-blocking) — three files remain over the 500-line ceiling.**
`.claude/rules/general-code-change.md` states "No production code, test code, or reusable script
file may exceed 500 lines," with no pre-existing-file exception. After this change,
`EfcFormController.cs` is 1072, `BreadcrumbBridgeRouter.cs` is 596, and
`BreadcrumbBridgeRouterIssue439Tests.cs` is 694. All three exceeded the ceiling before this change
and none grew (-12, 0, 0). The reinterpretation applied is "net non-growth for pre-existing
over-limit files," recorded in `artifacts/orchestration/orchestrator-state.json` under
`orchestrator_adjudications` at 2026-08-26T10:38:00Z (two files) and 2026-08-26T15:25:00Z (the
third). That state file is untracked and gitignored, so the ratification will not be visible in the
PR or after merge. *Recommendation:* transcribe the ratification into `issue.md` or the PR body, and
open a follow-up issue to split the three files. Not blocking: this change strictly improves the
metric it is measured against, and the executor recorded the deviation openly in
`evidence/qa-gates/p9-t6-file-size-audit.2026-08-26T19-55.md`.

**GAP-2 (PARTIAL, non-blocking) — the five manual validation steps were not executed.**
All five live-Outlook validation steps are recorded as NOT EXECUTED, with the reason "no interactive
Windows desktop, no signed-in Outlook profile," in
`evidence/qa-gates/manual-validation.2026-08-26T18-55.md`. The criterion's own text permits recorded
non-execution with a reason, so its letter is satisfied, but the end-to-end user path — spec Risk 7 —
remains unverified by any means. Each step has a named headless automated counterpart that passed.
*Recommendation:* a maintainer with a live Outlook profile should execute steps 1–5 before release
and append the observed results to the same artifact.

**GAP-3 (documented exception, non-blocking) — two existing assertion sets were rewritten.**
`FolderConverterTests.cs:329` and three assertions in
`Issue439ArchiveRootBoundarySelectionAndHostEventRemainDeterministic`. CLAUDE.md § 7.3 treats
existing tests as part of the spec. Both changes are deliberate spec corrections of assertions that
codified the defects under fix, both were pre-registered in the plan before execution, and both are
quoted old-versus-new with rationale in `change-description.2026-08-26.md`. Both make the assertions
more specific. Accepted.

**GAP-4 (informational, outside this change's control) — pre-existing `<PublishUrl>` identifier leak.**
`TaskMaster/TaskMaster.csproj:37` contains a real user-profile path and organization name. No hunk
of this change touches it, and it is already promoted to
`docs/features/potential/promoted/2026-08-26-taskmaster-csproj-publishurl-leaks-user-profile-path-and-org-name.md`.
Recorded for completeness only.

**GAP-5 (informational) — one out-of-plan-list path in the diff.**
`QuickFiler.Test/packages.config` (+1) is outside the plan's enumerated file list. It is the
mechanically required companion of the allowlisted `QuickFiler.Test.csproj` `log4net` reference: a
`packages.config` project cannot resolve a `<Reference>` whose package is not listed there.
Justified in `evidence/qa-gates/p8-t2-scope-audit.2026-08-26T18-40.md`. Accepted.

**GAP-6 (informational) — six pre-plan branch paths in the merge-base diff.**
`.gitignore` (commit `5a429486`, adding `.claude/state/`) and five
`docs/features/potential/promoted/2026-08-26-*.md` promotion documents were committed to this branch
before the plan was authored. `git diff --name-only HEAD` confirms none was modified by the
implementation phases. Two further promotion documents were added by the final commit `02092504`
under the plan's off-chain-promotion mandate. Accepted.

No blocking gap was identified.

## 9. Summary of Changes

| Category | Count | Detail |
| --- | ---: | --- |
| C# production files changed | 10 | 3 new (`ArchiveStemContract.cs`, `EfcSelectionGuard.cs`, `ArchiveRootPathGuard.cs`), 7 modified |
| C# test files changed | 12 | 10 new; `FolderConverterTests.cs` and `BreadcrumbBridgeRouterIssue439Tests.cs` corrected; `EmailFilerConfig_Tests.cs` and `BreadcrumbBridgeRouterTests.cs` extended |
| `.csproj` files changed | 5 | 10 `<Compile Include>` additions, 1 `<Reference>` addition |
| `packages.config` changed | 1 | `log4net 3.3.2` for `QuickFiler.Test` |
| Repository config changed | 1 | `.gitignore` (+4, pre-plan commit) |
| Docs, evidence, agent memory | 51 | spec, plan, issue, research, change description, 30 evidence artifacts, 7 promotion documents, 6 agent-memory files |
| New test methods | 80 | across 7 new test classes plus additions to 2 existing classes |
| Total | 80 files | +7599 / -158 |

Defects addressed: D1 (router verbatim pass-through), D2 (out-of-root segment activation), D3
(`SelectRow` filing-target leak and `ToHierarchyPath` fabrication), D4 (unvalidated concatenation at
the filing boundary plus `GetStem` / `IsDeleteRelevant` unanchored matching), D5a–D5g plus the dead
`ask` parameter (`FolderConverter`), D6 (unverified archive-root combine), D7 (silent OneDrive
fallback chain), D8 (`EfcDataModel` unanchored replace), D9 (OK-path guard asymmetry). All nine are
addressed; none is deferred.

## 10. Compliance Verdict

| Section | Verdict |
| --- | --- |
| 1. General Unit Test Policy | PASS |
| 2. General Code Change Policy | PARTIAL (GAP-1, file size ceiling) |
| 3. C# Code Change Policy | PASS |
| 4. C# Unit Test Policy | PARTIAL (GAP-3, documented assertion rewrites) |
| 5. C# repo-wide line coverage | FAIL (non-blocking; pre-existing and improved by this change) |
| 5. C# repo-wide branch, new-code, and changed-line coverage | PASS |
| 6. Test Execution Metrics | PASS |
| 7. Code Quality Checks (all four toolchain gates) | PASS |
| 8. Gaps and Exceptions | 0 blocking, 2 PARTIAL, 4 informational |
| Evidence Location Compliance | PASS |
| Rejected Scope Narrowing | none detected |
| modified-workflow-needs-green-run | rule not triggered |

**Overall: PASS. 0 blocking findings. Recommended for PR, with GAP-1 and GAP-2 tracked as follow-ups.**

### Remediation Determination

Remediation is **not triggered**, and no `remediation-inputs.<timestamp>.md` artifact is produced.
Each trigger in the feature-review workflow was evaluated explicitly:

| Trigger | State | Fires? |
| --- | --- | --- |
| Toolchain checks fail | All four gates re-executed by this reviewer, all exit 0 | No |
| Code review contains blockers | 0 Blocking findings (4 Major, 4 Minor, 2 Info — all non-blocking) | No |
| Repo-wide coverage below the 80% remediation threshold | 84.8696% measured, above 80% | No |
| Modified files below 80% or regressed | No changed line lost coverage; six of seven files improved | No |
| New files below 90% | Every new type and method measures 100.0000% | No |
| Coverage artifact absent for a changed language | `artifacts/csharp/coverage.xml` present and parseable; C# is the only changed language | No |
| Required acceptance criteria FAIL or PARTIAL | AC25 PARTIAL; AC1–AC24 and AC26 PASS; 0 FAIL | See note |
| Policy audit contains a meaningful FAIL or PARTIAL | 1 FAIL (repo-wide line coverage vs the 85% floor), 2 PARTIAL | See note |

Note on the two "see note" rows. Both concern the same two items — the repo-wide line-coverage
shortfall against the 85% floor and the AC25 file-size reinterpretation — and neither is a defect
introduced by this change:

- The coverage shortfall is pre-existing, is improved by +0.0899 points by this change, and clears
  the workflow's own numeric remediation threshold of 80% by 4.87 points. Remediating it means
  covering roughly 83 additional lines in code #614 does not touch.
- The AC25 shortfall is three files that were already over the 500-line ceiling before this change
  and that none of this change's hunks grew. Remediating it means splitting a 1072-line controller,
  a 596-line router, and a 694-line test class inside a defect fix — precisely the out-of-scope
  refactor the repository's scope discipline forbids, and the reason the reinterpretation was
  ratified in the first place.

Opening a remediation cycle for either item would direct work that is out of scope for #614 while
adding no defect correction. Both are instead recorded as GAP-1 and GAP-2 with named follow-up
actions, and the code review records four further follow-up recommendations. This determination is
recorded here so the decision is auditable rather than implicit.

## Appendix A: Test Inventory

| Test class | File | Methods | Status |
| --- | --- | ---: | --- |
| `ArchiveStemContractTests` | `UtilitiesCS.Test/OutlookObjects/Folder/ArchiveStemContractTests.cs` | 23 | new, all pass |
| `FolderConverterIssue614Tests` | `UtilitiesCS.Test/OutlookObjects/Folder/FolderConverterIssue614Tests.cs` | 19 | new, all pass |
| `EfcSelectionGuardTests` | `QuickFiler.Test/Controllers/EfcSelectionGuardTests.cs` | 9 | new, all pass |
| `BreadcrumbBridgeRouterIssue614Tests` | `QuickFiler.Test/Controllers/BreadcrumbBridgeRouterIssue614Tests.cs` | 8 | new, all pass |
| `EfcDataModelIssue614Tests` | `QuickFiler.Test/Controllers/EfcDataModelIssue614Tests.cs` | 8 | new, all pass |
| `AppFileSystemFolderPathsOneDriveResolutionTests` | `TaskMaster.Test/AppGlobals/AppFileSystemFolderPathsOneDriveResolutionTests.cs` | 7 | new, all pass |
| `AppOlObjectsArchiveRootValidationTests` | `TaskMaster.Test/AppGlobals/AppOlObjectsArchiveRootValidationTests.cs` | 6 | new, all pass |
| `EmailFilerConfig_Tests` | `UtilitiesCS.Test/EmailIntelligence/EmailFilerConfig_Tests.cs` | extended (includes the primary regression test) | all pass |
| `BreadcrumbBridgeRouterTests` | `QuickFiler.Test/Controllers/BreadcrumbBridgeRouterTests.cs` | extended (producer companion test) | all pass |
| `FolderConverterTests` | `UtilitiesCS.Test/OutlookObjects/Folder/FolderConverterTests.cs` | 1 assertion corrected | all pass |
| `BreadcrumbBridgeRouterIssue439Tests` | `QuickFiler.Test/Controllers/BreadcrumbBridgeRouterIssue439Tests.cs` | 3 assertions corrected | all pass |

## Appendix B: Toolchain Commands Reference

Commands executed by this reviewer, in the CLAUDE.md-mandated order, against head `02092504`:

```
dotnet tool restore
dotnet tool run csharpier check .
```
Exit 0. `Checked 1530 files in 3954ms.`

```
msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true
```
Exit 0. 0 errors. 18 project DLL outputs. 5 pre-existing System.Reactive advisories.

```
msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true
```
Exit 0. 0 errors. 0 CS86xx diagnostics. 18 project DLL outputs.

```
vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll QuickFiler.Test\bin\Debug\QuickFiler.Test.dll TaskMaster.Test\bin\Debug\TaskMaster.Test.dll /Settings:TaskMaster.runsettings /InIsolation
```
Exit 0. 6093 total, 6093 passed, 0 failed, 0 skipped.

Scope, evidence-location, and redaction verification statements:

```
git merge-base main HEAD
git diff --numstat c279d40b..HEAD
git diff --name-only c279d40b..HEAD | grep -E "^artifacts/(baselines?|qa|qa-gates|evidence|coverage|regression-testing|post-change)/"
git diff --name-only c279d40b..HEAD | grep -E "^(\.github/workflows/|scripts/benchmarks/|\.github/actions/)"
git diff c279d40b..HEAD | grep -icE "<account>|<account-email>"
```

Coverage-artifact verification, dot-sourced from the local validator with no regeneration:

```
. .\.claude\hooks\validate-feature-review-coverage.ps1
Get-ChangedLanguageSet -Lines (Get-ArtifactFileContent -Path 'artifacts/pr_context.summary.txt').Lines
Get-LanguageRepoCoverage   -Language 'CSharp'
Get-LanguageBranchCoverage -Language 'CSharp'
```

Returned `CSharp`, `84.87`, and `78.83` respectively.

Build host used: the Visual Studio 18 Community `MSBuild\Current\Bin\MSBuild.exe`.
Test host used: the Visual Studio 18 Community `Common7\IDE\Extensions\TestPlatform\vstest.console.exe`,
which honours the repository binding redirects.
