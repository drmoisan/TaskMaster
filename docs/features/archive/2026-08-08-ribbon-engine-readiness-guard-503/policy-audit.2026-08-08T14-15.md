# Policy Compliance Audit — ribbon-engine-readiness-guard (Issue #503)

- Feature folder: `docs/features/active/2026-08-08-ribbon-engine-readiness-guard-503/`
- Base branch: `main`
- Merge-base: `003c5715055d7d1933db68a742531332756e30b2` (recomputed this session with `git merge-base HEAD origin/main`; matches the caller-supplied value)
- Head: `bug/ribbon-engine-readiness-guard-503` @ `d0955dc4c7be61b654dbeb0804d5520fde5a5a4c`
- Work mode: `full-bug` (marker `- Work Mode: full-bug` in `issue.md:12`); acceptance-criteria source is `spec.md` only
- Audit scope: the full branch diff against the resolved base branch (86 changed paths)
- Reviewed: 2026-08-08T14-15

## Executive Summary

The change closes an initialization race in which eight engine-backed Explorer-ribbon commands could be clicked before `AppItemEngines.InitAsync()` had populated `Globals.Engines.InboxEngines`, producing an unhandled `NullReferenceException` or `KeyNotFoundException` out of an `async void` handler. The fix introduces four new host-neutral `internal` decision types under `TaskMaster\Ribbon\`, none carrying `[ExcludeFromCodeCoverage]`, plus two thin partials inside pre-existing coverage-exempt COM shims, a `getEnabled` attribute on the eight affected buttons, and one post-initialization invalidation call.

All four toolchain stages were re-run or re-verified independently this session and pass. Formatting is clean repo-wide (exit 0 over 1498 files). The analyzer build and the type-check build both return exit 0. The 69 tests in the `TaskMaster.Test.Ribbon` namespace, including all 45 tests added by this change, pass under the repository's sanctioned test invocation. Repository C# line coverage is 85.85% and branch coverage is 79.25%, both above the uniform floors and both marginally improved against the captured merge-base baseline.

Nine findings are recorded, none of them blocking. Three are Medium: a vacuously-passing assertion in the ribbon-XML regression test that names AC5, the pre-existing incremental-build vacuity of the mandated type-check gate, and a 20-line increase to a UI resource file that was already above the repository line limit. The remaining six are Low or informational. The three MANUAL-ONLY acceptance criteria (AC19, AC20, AC21) correctly remain unchecked and require maintainer execution against a live Outlook profile before merge.

**Verdict: PASS with a pre-merge condition.** No remediation plan is required; the outstanding gate is maintainer execution of the recorded manual checklist, which no code change can satisfy.

## Rejected Scope Narrowing

None. The caller prompt supplied the base branch, merge-base, feature folder, and work mode, and explicitly delegated scope determination to this agent ("Scope determination is yours"). It contained no instruction to limit review to a plan, phase, task, or file subset, and no instruction to mark any language's coverage verdict as anything other than an explicit verdict. The caller in fact directed the reviewer away from over-trusting the generated PR-context summary, which is consistent with the full-diff scope invariant. The audit was performed against the complete `003c5715...HEAD` three-dot diff.

One upstream artifact defect was corrected rather than accepted (see Section 8, item 2): the generated PR-context summary classified every changed file as documentation, which would have caused the SubagentStop coverage gate to enumerate zero changed languages and skip enforcement entirely.

## 1. General Unit Test Policy Compliance

| Check | Verdict | Evidence |
|---|---|---|
| Independence (order-independent) | PASS | Every new test constructs its own `ConcurrentDictionary`, `Mock<IAppItemEngines>`, and sink list inside the test body. No static or shared mutable state is written. `EngineCommandCatalog` exposes only immutable read-only state. |
| Isolation (one unit per test) | PASS | Each of the 45 added tests exercises a single member: `IsEngineReady`, `TryGetEngine`, `TryGetEngineName`, `ControlIds`, `IsCommandEnabled`, `RunAsync`, `InvalidateAll`, or one XML/reflection assertion. |
| Fast execution | PASS | Measured this session: 69 tests in the `TaskMaster.Test.Ribbon` namespace complete in 0.69 s wall clock. |
| Determinism | PASS | No sleep, delay, wall-clock read, temporary file, message pump, `Form`, `MessageBox`, or live COM appears in any added test. Verified by direct grep over `TaskMaster.Test/Ribbon/*.cs`. The one asynchronous-completion test drives a `TaskCompletionSource` that the test itself completes synchronously. |
| Readability, AAA structure | PASS | Every added test carries explicit `// Arrange` / `// Act` / `// Assert` comments and a behaviour-named method name. Every assertion supplies a `because` reason string. |
| Scenario completeness | PASS | Positive, negative, boundary (null / empty / whitespace / unknown id / ordinal case), error-propagation, and state-transition (empty dictionary mutated to populated between two probes) paths are all represented. |
| No external dependencies | PASS | Moq doubles `IAppItemEngines` and `IConditionalEngine<MailItemHelper>`; the ribbon XML is read from the embedded manifest resource stream, not the filesystem. |
| Test file location mirrors source | PASS | `TaskMaster/Ribbon/Foo.cs` maps to `TaskMaster.Test/Ribbon/FooTests.cs` for all four new types. No test file was placed in a production source tree. |
| Coverage exclusion policy (no production path excluded) | PASS | No `coverage.config`, `.runsettings`, or exclusion configuration file appears in the branch diff. No `[ExcludeFromCodeCoverage]` attribute was added to any new type; verified by `grep -nE "^\s*\[ExcludeFromCodeCoverage\]" TaskMaster/Ribbon/Engine*.cs` returning no match. |

### 1.2 Coverage Verification (mandatory for every language with changed files)

Changed-language enumeration derived from `git diff --name-status 003c5715...HEAD`: **C# only**. The branch changes 10 `.cs` files, 2 `.csproj` files, and 1 embedded `.xml` UI resource; every remaining path is Markdown documentation, Markdown agent memory, or committed XML coverage summaries under the feature evidence tree.

- TypeScript: zero changed files on the branch (no `.ts` or `.tsx` path appears in the diff); no verdict is required for a language with zero changed files.
- Python: zero changed files on the branch (no `.py` path appears in the diff); no verdict is required for a language with zero changed files.
- PowerShell: zero changed files on the branch (no `.ps1` or `.psm1` path appears in the diff); no verdict is required for a language with zero changed files.

#### 1.2.1 C# coverage (changed language — explicit verdicts)

Artifact: `artifacts/csharp/coverage.xml` (canonical path, JaCoCo package-counter format). Verified this session to be whitespace-identical to the committed `evidence/qa-gates/coverage-final.jacoco.xml`. Both were projected from the executor's post-change Cobertura run of `scripts/vscode/Invoke-MSTestWithCoverage.ps1 -Configuration Debug`. Test assemblies are absent from the denominator; vendored third-party packages are excluded by `coverage.config`.

- **C# repo-wide line coverage: 85.85% (95,473 covered / 111,207 valid) — PASS** (>= the 85% uniform floor in `.claude/rules/quality-tiers.md`, and >= the 80% floor in CLAUDE.md UT2). Independently summed from the nine `<counter type="LINE">` elements of `artifacts/csharp/coverage.xml`.
  - Baseline: 85.85% (95,309 / 111,021) — `evidence/baseline/coverage-baseline.jacoco.xml`, captured at the merge-base before implementation.
  - Post-change: 85.85% (95,473 / 111,207).
  - Change: +0.0039 percentage points; +164 covered lines against +186 valid lines.
  - Disposition: improvement. No regression.
  - Evidence: `evidence/qa-gates/coverage-comparison.2026-08-08T14-56.md`; figures re-derived this session directly from the two committed JaCoCo summaries.
- **C# repo-wide branch coverage: 79.25% (22,131 covered / 27,926 valid) — PASS** (>= the 75% uniform floor).
  - Baseline: 79.24% (22,077 / 27,862). Post-change: 79.25% (22,131 / 27,926). Change: +0.0117 percentage points. Disposition: improvement.
- **C# new-code line coverage: 100.00% (186 covered / 186 executable lines across the four new decision types) — PASS** (>= the 90% new-module floor in CLAUDE.md UT2 and >= the 85% uniform floor). Independently corroborated at package level: the `TaskMaster` package `LINE` counter moved from `missed=1464 covered=3329` to `missed=1464 covered=3515`. The missed count is byte-identical across the two measurements while covered rose by exactly 186, which is exactly the sum of the four new types' measured line counts (48 + 48 + 72 + 18). Had any of the 186 new lines been uncovered, the missed count would necessarily have risen.
- **C# new-code branch coverage: 96.88% (62 covered / 64 new branches) — PASS** (>= the 75% uniform floor). Derived from the `TaskMaster` package `BRANCH` counter moving from `missed=387 covered=687` to `missed=389 covered=749`. The single documented uncovered branch is in `EngineGatedCommandRunner` (13 of 14, per `evidence/qa-gates/new-type-coverage.2026-08-08T14-54.md`); a second missed branch entered the package counter and is not individually attributed in the surviving evidence. The package branch rate nevertheless rose from 63.97% to 65.82%.
- **C# modified-file no-regression on changed lines — PASS.** The three modified production files (`RibbonViewer.cs`, `ThisAddIn.cs`, and the `RibbonExplorer.xml` resource) sit inside types that carry `[ExcludeFromCodeCoverage]` at the merge-base and still carry it at head, so no file moved from measured to unmeasured and no measured line lost coverage. The unchanged `missed=1464` figure for the whole `TaskMaster` package is the direct measurement supporting this.
- **New production files: 6.** Four are the measured decision types at 100.00%. Two (`RibbonController.EngineCommands.cs`, `RibbonViewer.EngineCommands.cs`) are new partials of types that were already coverage-exempt at the merge-base; they add no new exemption and introduce no new attribute.

Measurement note: the SubagentStop hook parses the canonical path with a JaCoCo `//counter` query and computes 85.85% line and 79.25% branch from this artifact, matching the figures above. The hook's parsers were invoked directly this session to confirm agreement.

## 2. General Code Change Policy Compliance

| Check | Verdict | Evidence |
|---|---|---|
| Simplicity first | PASS | Four small types totalling 388 lines, each with one responsibility. No new framework, no new abstraction layer, no new dependency. The guard is a single chokepoint rather than null-conditional operators scattered through eight handlers. |
| Reusability | PASS | The control-id to engine-key binding lives in exactly one place (`EngineCommandCatalog.Map`) and is consumed by the XML regression test, the `getEnabled` decision, the click guard, and the refresh planner. Adding a future engine-backed command is a one-line change. |
| Extensibility | PASS | Public behaviour is expressed through injected delegates (`Func<IAppItemEngines>`, `Action<string>`, `Action<string>` invalidator), so callers can extend without editing the decision types. |
| Separation of concerns | PASS | Every decision is host-neutral and carries zero `Microsoft.Office.*` reference; the only new Office-typed member is one `getEnabled` shim on the pre-existing COM-visible viewer. Verified by grep over the four new decision files: the only occurrences of `Microsoft.Office` are inside XML doc comments asserting their absence. |
| File size <= 500 lines (code and test files) | PASS | Measured with `awk NR`: EngineCommandCatalog.cs 88, EngineCommandRefreshPlanner.cs 58, EngineGatedCommandRunner.cs 139, EngineReadinessGate.cs 103, RibbonController.EngineCommands.cs 100, RibbonViewer.EngineCommands.cs 207, RibbonViewer.cs 388 (down from 487), ThisAddIn.cs 307, EngineCommandCatalogTests.cs 116, EngineCommandRefreshPlannerTests.cs 52, EngineGatedCommandRunnerTests.cs 346, EngineReadinessGateTests.cs 223, RibbonExplorerXmlTests.cs 309. The 487/500 pressure on `RibbonViewer.cs` was resolved by a partial-class split, which is the correct mechanism. |
| Embedded UI resource size | Exception recorded | `RibbonExplorer.xml` moved from 519 to 539 lines. It was already above 500 at the merge-base. It is a declarative embedded resource rather than production, test, or reusable script code, and `spec.md` AC25 records the overage as an accepted pre-existing exception. See Section 8, item 1. |
| Error handling — fail fast, no silent catches | PASS | `git diff 003c5715...HEAD -- '*.cs' \| grep "^+.*catch"` matches exactly one added line, which is prose inside an XML doc comment. No `catch` clause of any kind was added. Constructor preconditions throw `ArgumentNullException`; the guard suppresses invocation only and re-raises nothing, so an exception from a ready action propagates unchanged. |
| Logging pattern | PASS | The blocked-click notice uses the existing `logger.Warn` plus `MessageBox.Show` mechanism already used at six sites in the ribbon layer. No `Console.WriteLine` or ad-hoc output was added. |
| Naming | PASS | PascalCase types and members, camelCase locals and parameters, descriptive non-cryptic names throughout. |
| Public API compatibility | PASS | `IAppItemEngines` gains no member. All four decision types are `internal`. The one new `public` member is the required Office callback on an existing public type. Every existing test double compiles unmodified; no test-double file appears in the diff. |
| Dependencies | PASS | No package reference, project reference, or `packages.config` entry changed. The two `.csproj` edits are `<Compile Include>` registrations required by the legacy non-SDK project format. |
| I/O boundaries | PASS | The four decision types touch no disk, network, or COM. `EngineReadinessGate` performs one lock-free `ConcurrentDictionary.TryGetValue` plus null checks. |
| Bugfix workflow — failing regression test first | PASS | `evidence/regression-testing/fail-before-503.2026-08-08T13-22.md` and `fail-before-exception.2026-08-08T13-23.md` record the red state before the fix; `pass-after-503.2026-08-08T13-32.md` records the green state after. |
| Toolchain loop restarts honoured | PASS | `evidence/qa-gates/toolchain-clean-pass.2026-08-08T14-58.md` records three entries into the Phase 6 loop, with the first two restarted (formatter rewrote files; nullable verification surfaced three diagnostics) and the third clean. MD5 fingerprints of all 16 touched paths, captured at both ends of the clean pass, are identical to the files on disk today. |

## 3. Language-Specific Code Change Policy Compliance (C#)

| Check | Verdict | Evidence |
|---|---|---|
| Formatting — CSharpier | PASS | Re-run this session: `csharpier check .` returned exit 0 over 1498 files. |
| Linting — .NET analyzers | PASS | Re-run this session: `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform='Any CPU' /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` returned exit 0 with every project emitting an assembly. Only the pre-existing `System.Reactive` packages.config advisory warning appears. |
| Type checking — nullable analysis | PASS | Re-run this session: `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform='Any CPU' /p:Nullable=enable /p:TreatWarningsAsErrors=true` returned exit 0 with 0 errors. Independently strengthened by a forced `/t:Rebuild` of `TaskMaster\TaskMaster.csproj` under the same properties, which surfaced 195 errors of which 64 are `CS86xx` and **0 are in any of the six new production files**. See Section 8, item 3. |
| `dotnet format` avoided | PASS | Not present in any evidence artifact or plan step. |
| Strong contracts and explicit APIs | PASS | Every public and internal member carries XML documentation stating inputs, outputs, and failure modes. Explicit types are used at all boundaries. |
| Null safety | PASS | Three null-forgiving operators are used, each with an adjacent comment recording why null is a supported value rather than a defect: `TryGetEngineName`'s `out` parameter on the false path, `() => Globals?.Engines!`, and `control?.Id!`. Each corresponds to a documented contract enforced by a unit test. |
| Composition over inheritance | PASS | All four types are sealed or static; none introduces an inheritance hierarchy. Behaviour is composed through injected delegates. |
| Async and resource safety | PASS | `RunAsync` returns `Task` rather than being `async void`; it awaits nothing itself and returns the action's task directly, preserving the caller's await. No disposable is introduced. |
| Internal-first public surface | PASS | Four of the five new types are `internal`; `InternalsVisibleTo("TaskMaster.Test")` already existed, so no visibility widening was needed for testability. |
| Architecture boundaries (`.claude/rules/architecture-boundaries.md`) | PASS | Rule 3: no new `[ComVisible(true)]` type; grep over the diff shows the only added `ComVisible` occurrence is prose in an XML doc comment. Rules 1, 2, 4: the sole new Office-typed member is `public bool EngineCommand_GetEnabled(Office.IRibbonControl)` inside the pre-existing COM-visible shim. Rule 8: all readiness, catalog, guard, and refresh-planning behaviour is host-neutral and would port unchanged to a non-desktop command surface. |

## 4. Language-Specific Unit Test Policy Compliance (C#)

| Check | Verdict | Evidence |
|---|---|---|
| MSTest framework | PASS | All four new test classes use `[TestClass]`, `[TestMethod]`, and `[DataTestMethod]`/`[DataRow]` from `Microsoft.VisualStudio.TestTools.UnitTesting`. No xUnit or NUnit reference was introduced. |
| Moq for mocking | PASS | `new Mock<IAppItemEngines>()` and `new Mock<IConditionalEngine<MailItemHelper>>()` are the only doubles used. |
| FluentAssertions for assertions | PASS | Every assertion in the 45 added tests uses `.Should()`. No bare MSTest `Assert` call appears in the new files. |
| Deterministic test rules | PASS | No network, no filesystem, no clock, no external process, no implicit working directory. The XML is read through `GetManifestResourceStream`. |
| DI seams | PASS | Injectable-delegate seams (`Func<IAppItemEngines>`, `Action<string>`) are used where a full interface would be excessive, which is option 2 of the prescribed ordering, and the boundary being wrapped is a property read plus a notification sink. |
| Banned symbols | PASS | Grep for `Thread.Sleep`, `Task.Delay`, `DateTime.Now`, `DateTime.UtcNow`, `Path.GetTempPath`, `new Form`, `MessageBox`, and `Application.Run` across `TaskMaster.Test/Ribbon/*.cs` returns no match. The BannedApiAnalyzers stage is part of the passing analyzer build. |
| Readiness not reached through side-effecting getters | PASS | No test touches `RibbonController.SB`, `Triage`, or `TriageAsync`, each of which installs a real `WindowsFormsSynchronizationContext` on the calling thread. The gate is exercised through its injected accessor. |

## 5. Test Coverage Detail

| Scope | Line | Branch | Verdict |
|---|---|---|---|
| Repository, first-party denominator | 85.85% (95,473/111,207) | 79.25% (22,131/27,926) | PASS |
| `TaskMaster` package | 70.60% (3,515/4,979) | 65.82% (749/1,138) | Pre-existing sub-floor package, improved by this change from 69.46% / 63.97%. Not a gate for this feature; the repository denominator is the policy denominator and it passes. |
| `EngineCommandCatalog` | 100.00% (48/48) | 100.00% | PASS |
| `EngineReadinessGate` | 100.00% (48/48) | 100.00% | PASS |
| `EngineGatedCommandRunner` | 100.00% (72/72) | 92.86% (13/14) | PASS |
| `EngineCommandRefreshPlanner` | 100.00% (18/18) | 100.00% | PASS |
| `RibbonController.EngineCommands.cs`, `RibbonViewer.EngineCommands.cs` | not measured | not measured | Inside types already carrying `[ExcludeFromCodeCoverage]` at the merge-base; no new exemption introduced. |

## 6. Test Execution Metrics

| Metric | Value | Source |
|---|---|---|
| Full-suite tests, merge-base | 6,293 passed, 0 failed, 0 skipped | `evidence/baseline/tests-with-coverage.2026-08-08T13-11.md` |
| Full-suite tests, head | 6,338 passed, 0 failed, 0 skipped | `evidence/qa-gates/tests-with-coverage.2026-08-08T14-52.md` |
| Tests added by this change | 45 | Delta above; matches the enumerated per-class counts including data-row expansion |
| Re-run this session, `TaskMaster.Test.Ribbon` namespace | 69 passed, 0 failed, exit 0 | `vstest.console.exe TaskMaster.Test\bin\Debug\TaskMaster.Test.dll /Settings:TaskMaster.runsettings /TestCaseFilter:'FullyQualifiedName~TaskMaster.Test.Ribbon'` using the `Common7\IDE\Extensions\TestPlatform` binary |
| Wall clock, re-run | 0.69 s | Same run |
| Known pre-existing flake | `UtilitiesCS.Test...WpfDispatcherYieldTests.YieldAsync_WithoutDispatcher_RemainsStrict`, tracked as #508; passed on both the baseline and head full-suite runs | `evidence/baseline/preexisting-failures.2026-08-08T13-12.md` |

## 7. Code Quality Checks

| Check | Result |
|---|---|
| Formatting, re-run this session | PASS — exit 0, 1498 files |
| Analyzer build, re-run this session | PASS — exit 0, all assemblies emitted |
| Type-check build, re-run this session | PASS — exit 0, 0 errors |
| Forced-rebuild type-check of the changed project | PASS for authored code — 0 errors of any kind attributable to the six new production files |
| New-type unit tests, re-run this session | PASS — 69/69 |
| Attribute census | PASS — no `[ExcludeFromCodeCoverage]` and no `[ComVisible(true)]` added to any type |
| Broad-catch census | PASS — zero `catch` clauses added anywhere in the diff |
| Evidence integrity | PASS — canonical `artifacts/csharp/coverage.xml` whitespace-identical to the committed final JaCoCo summary; source MD5 fingerprints recorded in the clean-pass artifact match the files on disk |
| Zero-line-diff constraint on fenced files | PASS — `TaskMaster/AppGlobals/AppItemEngines.cs`, `UtilitiesCS/Interfaces/IGlobals/IAppItemEngines.cs`, and `TaskMaster/AppGlobals/ApplicationGlobals.cs` do not appear in `git diff --name-status 003c5715...HEAD` |

## 8. Gaps and Exceptions

1. **`RibbonExplorer.xml` grew from 519 to 539 lines** while already above the 500-line limit. The file is a declarative embedded UI resource, not production, test, or reusable script code, and `spec.md` AC25 records the overage as an accepted pre-existing exception that this bug fix does not remediate. Recorded as an exception rather than a policy FAIL, but noted that only 8 of the 23 added lines are functionally required: the three `TriageSet*` buttons were reformatted from one line each to six lines each, which added 12 avoidable lines to an already-oversized file. Non-blocking.

2. **PR-context summary misclassification.** The generated overview reported "Core logic changes: 0 files" and folded all 16 changed C# and project paths into the documentation bucket. This is the same recurring generator defect observed on prior C#-touching reviews. Its practical consequence here is specific: `Get-ChangedLanguageSet` in `.claude/hooks/validate-feature-review-coverage.ps1` derives changed languages from that section, so the uncorrected summary caused the hook to enumerate zero languages and skip every coverage check. The summary was corrected in place this session with an annotated correction block listing the 16 core-logic paths; the hook now enumerates `CSharp` and evaluates the coverage rows above. The underlying generator defect remains open tooling debt.

3. **The mandated type-check gate is incremental-vacuous.** `msbuild TaskMaster.sln /t:Build /p:Nullable=enable /p:TreatWarningsAsErrors=true` returns exit 0, but MSBuild's up-to-date check skips `CoreCompile` when only `/p:` values change, so the exit code alone does not establish that the tree is nullable-clean. Independently confirmed: a forced `/t:Rebuild` of `TaskMaster\TaskMaster.csproj` under the same properties returns exit 1 with 195 errors, 64 of them `CS86xx`, concentrated in untouched files (`OutlookItemTry.cs` 35, `OutlookItemFlaggableTry.cs` 30, `ItemInfo.cs` 20, `PropertyStore.cs` 17). **Zero** errors are attributable to any of the six new production files. This is pre-existing repository debt, was discovered and recorded by the executor (`evidence/qa-gates/msbuild-nullable.2026-08-08T14-49.md`), and is routed for promotion in `evidence/issue-updates/out-of-scope-promotions.2026-08-08T15-05.md`. Non-blocking for this feature; the gate defect itself warrants a governance issue.

4. **Coverage evidence reproducibility was reduced after the fact.** Commit `d0955dc4` replaced the two committed full-detail Cobertura reports (~20 MB, ~374,000 lines) with package-level JaCoCo summaries. The substitution is documented and the aggregate figures were verified lossless, but the per-file and per-type tables in `new-type-coverage.2026-08-08T14-54.md` and `coverage-comparison.2026-08-08T14-56.md` can no longer be re-derived from committed artifacts alone. This audit therefore corroborated AC23 by an independent route: the `TaskMaster` package missed-line count is identical across the two measurements while covered rose by exactly 186, the exact line total of the four new types. The size concern that motivated the substitution is legitimate; the trade-off should be made a documented repository convention rather than a per-feature decision.

5. **AC29 promotion receipts are not independently verifiable.** The GitHub CLI is unavailable in this environment, so the existence and content of issues #504 through #508 could not be confirmed. The five corresponding local entries under `docs/features/potential/promoted/` are present in the diff, and the in-repo half of the claim is verified. The GitHub half is UNVERIFIED for the stated environmental reason.

6. **Test-runner selection is unpinned.** CLAUDE.md CUT3 prescribes `vstest.console.exe <test-assembly-paths> /EnableCodeCoverage` without naming which of the two installed binaries to use or requiring `/Settings:TaskMaster.runsettings`. Running the changed assembly under `Common7\IDE\CommonExtensions\Microsoft\TestWindow\vstest.console.exe` without runsettings produced 26 spurious failures, all a `System.Threading.Tasks.Extensions, Version=4.2.0.1` assembly-load failure inside `Moq.Async.AwaitableFactory` caused by the app.config binding redirect not being applied. The same tests pass 69/69 under `Common7\IDE\Extensions\TestPlatform\vstest.console.exe` with the runsettings file, which is what `scripts/vscode/Invoke-MSTestWithCoverage.ps1` resolves. Three unrelated pre-existing tests failed identically, confirming an environment artifact rather than a defect in this change. Documentation debt, not a defect in this feature.

7. **`coverage-artifact-substitution.2026-08-08T17-40.md` is future-dated.** Its filename timestamp is 17-40 while the commit that introduced it is dated 14:05 and the current wall clock is 14:1x. Cosmetic; it breaks chronological ordering of the evidence tree.

8. **MCP template resolution was unavailable.** `mcp__drm-copilot__resolve_policy_audit_template_asset` and `mcp__drm-copilot__validate_orchestration_artifacts` are not exposed in this session, so the three review artifacts were authored against the canonical heading set enumerated in `.claude/skills/policy-audit-template-usage/SKILL.md` and the structure of the most recent accepted audit in this repository. Recorded as an assumption.

9. **AC19, AC20, and AC21 remain unverified by design.** They require a live Outlook process and mail profile, which the general unit-test policy forbids automated tests from depending on. The maintainer checklist at `evidence/manual-verification/ac19-ac21-checklist.2026-08-08T15-00.md` carries `Status: PENDING MAINTAINER EXECUTION`. They are correctly left unchecked in `spec.md`; checking them off from unit-test evidence would violate the criteria's own text.

## 9. Summary of Changes

- New production: `TaskMaster/Ribbon/EngineCommandCatalog.cs` (88), `EngineReadinessGate.cs` (103), `EngineGatedCommandRunner.cs` (139), `EngineCommandRefreshPlanner.cs` (58) — four host-neutral `internal` decision types, none coverage-exempt, all at 100.00% line coverage.
- New shims: `RibbonController.EngineCommands.cs` (100) and `RibbonViewer.EngineCommands.cs` (207) — partials of pre-existing `[ExcludeFromCodeCoverage]` types carrying the lazily-built runner, the Office `getEnabled` callback, the dispatcher-marshalled invalidation, and the eight relocated-and-guarded click handlers.
- Modified production: `RibbonViewer.cs` 487 to 388 lines via a partial-class split (`class` to `partial class`, two regions relocated); `ThisAddIn.cs` +7 lines (one refresh call plus a why-comment); `RibbonExplorer.xml` +23/-3 (eight `getEnabled` attributes plus reformatting of three buttons); two `.csproj` files gaining `<Compile Include>` entries.
- New tests: 45 across `EngineCommandCatalogTests` (116), `EngineCommandRefreshPlannerTests` (52), `EngineGatedCommandRunnerTests` (346), `EngineReadinessGateTests` (223), plus four additions to `RibbonExplorerXmlTests` (161 to 309).
- Docs and evidence: full feature folder (issue, spec, research, plan) plus 39 evidence artifacts under `evidence/baseline/`, `evidence/regression-testing/`, `evidence/qa-gates/`, `evidence/manual-verification/`, `evidence/issue-updates/`, and `evidence/other/`; five promoted potential-defect entries.
- Agent memory: atomic-executor, atomic-planner, prd-feature, and task-researcher Markdown memory updates.

## 10. Compliance Verdict

**PASS with a pre-merge condition.** Every policy gate passes on independent re-verification this session or on committed final-pass evidence that was cross-checked against the current working tree. Coverage clears every applicable floor at the repository, new-code, and changed-line level, and improved marginally against the captured merge-base baseline. No finding is blocking and no remediation plan is required; the three Medium findings are a test-robustness weakness compensated by a sibling assertion, a pre-existing toolchain-gate defect with zero impact on the authored code, and a size increase to a resource file already outside the limit by an accepted exception.

Recommendation: **conditional GO for PR.** The named condition is maintainer execution of `evidence/manual-verification/ac19-ac21-checklist.2026-08-08T15-00.md` against a live Outlook profile, recording the outcome under `evidence/manual-verification/`, before merge. No code change can satisfy that condition; it is a live-host verification of Office callback binding and cache invalidation, which `spec.md` Rollout already names as a pre-merge step.

## Evidence Location Compliance

- Scan of the full branch diff for files written under `artifacts/baselines/`, `artifacts/baseline/`, `artifacts/qa/`, `artifacts/qa-gates/`, `artifacts/evidence/`, `artifacts/coverage/`, `artifacts/regression-testing/`, or `artifacts/post-change/`: **zero occurrences.** All 39 evidence artifacts live under the canonical `docs/features/active/2026-08-08-ribbon-engine-readiness-guard-503/evidence/<kind>/` tree.
- `validate_evidence_locations.py` does not exist in this repository (no `scripts/dev_tools/` directory; confirmed by search). The equivalent check was performed manually against the complete diff file list.
- `artifacts/csharp/coverage.xml` is the allowed canonical gate-artifact path, is gitignored, and is not a diff-committed evidence file.
- No `EVIDENCE_LOCATION_OVERRIDE_REJECTED` condition arose; the caller supplied no non-canonical evidence path.

## Appendix A: Test Inventory

Tests added by this change (45 including data-row expansion):

1. `EngineReadinessGateTests` (11 methods, 13 cases): IsEngineReady_WhenAccessorReturnsNull_ReturnsFalse; IsEngineReady_WhenInboxEnginesIsNull_ReturnsFalse; IsEngineReady_WhenInboxEnginesIsEmpty_ReturnsFalse; IsEngineReady_WhenKeyPresentWithNonNullEngine_ReturnsTrue; IsEngineReady_WhenKeyPresentWithNullValue_ReturnsFalse; IsEngineReady_WithNullOrWhitespaceName_ReturnsFalse (3 rows); IsEngineReady_IsOrdinalCaseSensitive; IsEngineReady_AfterDictionaryPopulated_ReturnsTrue; TryGetEngine_WhenReady_OutputsSameInstance; TryGetEngine_WhenNotReady_OutputsNull; Constructor_WithNullAccessor_ThrowsArgumentNullException.

2. `EngineCommandCatalogTests` (6 methods, 13 cases): the 8-row control-id to engine-key mapping; unknown id; null id; empty id; ControlIds set equality; ControlIds duplicate-free.

3. `EngineGatedCommandRunnerTests` (13 methods): RunAsync_WhenEngineNotReady_DoesNotThrowNullReferenceException; RunAsync_WhenEngineNotReady_DoesNotThrowKeyNotFoundException; RunAsync_WhenEngineNotReady_EmitsExactlyOneNotificationContainingControlIdAndEngineName; RunAsync_WhenEngineReady_InvokesActionExactlyOnce; RunAsync_WhenEngineReady_AwaitsActionToCompletion; RunAsync_WhenActionThrows_PropagatesException; RunAsync_WithUnknownControlId_DoesNotInvokeAction; RunAsync_WithNullAction_ThrowsArgumentNullException; IsCommandEnabled_WhenEngineNotReady_ReturnsFalse; IsCommandEnabled_WhenEngineReady_ReturnsTrue; IsCommandEnabled_WithUnknownControlId_ReturnsFalse; Constructor_WithNullGate_ThrowsArgumentNullException; Constructor_WithNullNotificationSink_ThrowsArgumentNullException.

4. `EngineCommandRefreshPlannerTests` (2): InvalidateAll_InvokesDelegateOnceForEachEngineBackedControlId; InvalidateAll_WithNullDelegate_ThrowsArgumentNullException.

5. `RibbonExplorerXmlTests` additions (4): RibbonExplorerXml_EveryEngineBackedControlDeclaresGetEnabledCallback; RibbonExplorerXml_GetEnabledIsDeclaredOnlyOnEngineBackedControls; RibbonExplorerXml_EngineBackedControlsAreSchemaLegalForGetEnabled; RibbonExplorerXml_GetEnabledCallbackMatchesOfficeSignatureOnRibbonViewer.

Full-suite context: 6,338 tests across the solution's test assemblies, all passing in the executor's final unattended run.

## Appendix B: Toolchain Commands Reference

Commands executed this session (all check-only except the two builds, which write only to `bin`/`obj`):

- `git merge-base HEAD origin/main` -> `003c5715055d7d1933db68a742531332756e30b2`
- `git rev-parse HEAD` -> `d0955dc4c7be61b654dbeb0804d5520fde5a5a4c`
- `git diff --name-status 003c5715055d7d1933db68a742531332756e30b2...HEAD` (scope derivation)
- `git diff --numstat 003c5715055d7d1933db68a742531332756e30b2...HEAD` (language enumeration and line deltas)
- `C:\Users\DanMoisan\.dotnet\tools\csharpier.exe check .` -> exit 0, 1498 files
- `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform='Any CPU' /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` -> exit 0
- `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform='Any CPU' /p:Nullable=enable /p:TreatWarningsAsErrors=true` -> exit 0
- `msbuild TaskMaster\TaskMaster.csproj /t:Rebuild /p:Configuration=Debug /p:Nullable=enable /p:TreatWarningsAsErrors=true` -> exit 1, 195 errors, 0 in authored files (forced-recompile verification of the gate above)
- `vstest.console.exe TaskMaster.Test\bin\Debug\TaskMaster.Test.dll /Settings:TaskMaster.runsettings /TestCaseFilter:'FullyQualifiedName~TaskMaster.Test.Ribbon'` -> exit 0, 69/69 passed
- `awk 'END{print NR}'` over every changed non-Markdown path (file-size measurement; `Measure-Object -Line` deliberately avoided because it undercounts)
- `md5sum` over the 9 touched source paths, compared against the fingerprints in `evidence/qa-gates/toolchain-clean-pass.2026-08-08T14-58.md`
- Direct XML summation over `artifacts/csharp/coverage.xml` and the two committed JaCoCo summaries for all coverage figures
- `.claude/hooks/validate-feature-review-coverage.ps1` dot-sourced and its `Get-ChangedLanguageSet`, `Get-LanguageRepoCoverage`, and `Get-LanguageBranchCoverage` parsers invoked directly against the live artifacts
