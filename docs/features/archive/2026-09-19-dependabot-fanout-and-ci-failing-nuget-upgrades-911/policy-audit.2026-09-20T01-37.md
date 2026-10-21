# Policy Compliance Audit — Issue #911 (dependabot fan-out and CI-failing NuGet upgrades)

- Component: dependency-consistency tooling, Dependabot configuration, CI workflows
- Date: 2026-09-20
- Reviewer: feature-review
- Branch: `bug/dependabot-fanout-and-ci-failing-nuget-upgrades-911`
- Head: `794d34f02647214030fc3c2b076112dee731ff62`
- Base: `origin/main`
- Merge base: `734112ed25bba293cb074e71fee2286bc3b72fae`
- Work mode: `full-bug` (marker read from `issue.md`)
- Acceptance-criteria source: `spec.md` only

## Executive Summary

The branch diff against `origin/main` is **214 files, +23,626 / -6,272, across 21 commits**. The
caller-supplied figure of 199 files does not reconcile against the tree; this audit uses the measured
214. Scope is the full branch diff and was not narrowed.

The central defect this change targets is verified fixed, independently of the evidence the executor
produced. Across all 18 project and manifest pairs there are 1,498 package restore-path references
in `.csproj` files (`Import` 234, `Error` 230, `HintPath` 872, `Analyzer` 162). At the merge base 15
of them named a version no sibling `packages.config` declared; at head zero do. All 15 were
`Meziantou.Analyzer.3.0.203` against a manifest declaring `3.0.235`.

Four blocking findings and five major findings are recorded. None of them invalidates the retroactive
repair of the tree. All of them concern the forward-prevention half of the change, the PowerShell
coverage floor, or artifact hygiene.

| Area | Verdict |
|---|---|
| General Unit Test Policy | PARTIAL |
| General Code Change Policy | PASS |
| PowerShell code change and unit test policy | PARTIAL |
| C# code change policy for build-configuration files | PASS |
| Coverage | FAIL |
| Workflow green-run rule | FAIL |
| Artifact hygiene | FAIL |
| Evidence-location compliance | PASS |

Overall verdict: **PARTIAL. Remediation is required before the pull request is opened.**

## Rejected Scope Narrowing

No caller instruction attempted to narrow the audit scope to a plan, task, phase, file subset, or
language subset. The caller explicitly directed the full branch diff against `origin/main`. One
factual correction was applied: the caller stated 199 changed files; the measured count is 214.

## Evidence Location Compliance

`git diff --name-only 734112ed2..794d34f02` was scanned for paths under `artifacts/baselines/`,
`artifacts/qa/`, `artifacts/evidence/` and `artifacts/coverage/`. **Zero matches.** All 136 evidence
files sit under the feature folder in the canonical `baseline/`, `qa-gates/`, `regression-testing/`,
`issue-updates/` and `other/` subdirectories. No `EVIDENCE_LOCATION_OVERRIDE_REJECTED` condition
arose. Verdict: PASS.

## 1. General Unit Test Policy Compliance

| Requirement | State | Evidence |
|---|---|---|
| Independence, isolation, determinism | PASS | 302 Pester tests; every filesystem interaction sits behind an injected delegate |
| Test file location mirrors source tree | PASS | `tests/scripts/dependencies/` mirrors `scripts/dependencies/`; `tests/scripts/vscode/` mirrors `scripts/vscode/` |
| No temporary files in tests | PASS | the composition root and both rewritten scripts expose delegate seams so fixtures are in memory |
| Scenario completeness for negative, edge and error flows | FAIL | `scripts/vscode/Sync-PackageReferences.ps1` carries 9 uncovered pure-logic lines, all negative or error paths |
| Line coverage floor | FAIL | one changed production file measures 74.80 percent |
| No regression on changed lines | PASS | PowerShell aggregate moved from 83.93 to 93.89; C# deltas are -0.02 points on an identical denominator |
| Coverage exclusion policy | PASS | no production path is excluded from measurement |

### Coverage Evidence Checklist

- C# baseline coverage artifact: `docs/features/active/2026-09-19-dependabot-fanout-and-ci-failing-nuget-upgrades-911/evidence/baseline/p2-t7-mstest-numeric-baseline.2026-09-19T09-44.md`
- C# post-change coverage artifact: `docs/features/active/2026-09-19-dependabot-fanout-and-ci-failing-nuget-upgrades-911/evidence/qa-gates/p9-t7-coverage-projection.2026-09-19T09-44.jacoco.xml`
- TypeScript baseline coverage artifact: `N/A - zero TypeScript files changed on this branch`
- TypeScript post-change coverage artifact: `N/A - zero TypeScript files changed on this branch`
- PowerShell baseline coverage artifact: `docs/features/active/2026-09-19-dependabot-fanout-and-ci-failing-nuget-upgrades-911/evidence/baseline/p0-t18-pester.2026-09-19T09-44.md`
- PowerShell post-change coverage artifact: `coverage/p9-t3-pester-coverage.iter1.xml`
- Python baseline coverage artifact: `N/A - zero Python files changed on this branch`
- Python post-change coverage artifact: `N/A - zero Python files changed on this branch`
- Per-language comparison summary: the per-language coverage comparison block of this document

**Coverage Metrics by Language:**

| Language | Files Changed | Tests | Test Result | Baseline Coverage | Post-Change Coverage | New Code Coverage |
|---|---|---|---|---|---|---|
| PowerShell | 15 | 302 | 302 passed, 0 failed | 83.93% lines | 93.89% lines | 97.97% |
| C# | 50 | 7343 | passed | 85.93% lines / 80.09% branch | 85.91% lines / 80.07% branch | 100.00% |
| TypeScript | 0 | 0 | N/A | N/A | N/A | N/A |
| Python | 0 | 0 | N/A | N/A | N/A | N/A |

### 1.2.1 Per-Language Coverage Comparison

- PowerShell: Baseline: 83.93% lines. Post-change: 93.89% lines (1598/1702). Change: +9.96% lines. New/changed-code coverage: 97.97%. Disposition: FAIL. Evidence: parsed directly from `coverage/p9-t3-pester-coverage.iter1.xml`; the aggregate clears every floor but the changed file `scripts/vscode/Sync-PackageReferences.ps1` measures 74.80 percent, below both the 80 in CLAUDE.md and the 85 in the rules file.
- C#: Baseline: 85.93% lines (56486/65737) / 80.09% branch (13657/17052). Post-change: 85.91% lines (56476/65737) / 80.07% branch (13654/17052). Change: -0.02% lines and -0.02% branch, inside run-to-run variation on an identical denominator. New/changed-code coverage: 100.00%. Disposition: FAIL. Evidence: parsed directly from the committed JaCoCo projection; both figures clear the 85 line and 75 branch floors, and the recorded FAIL is procedural, for the absent canonical artifact path recorded in the artifact-state table below.
- TypeScript: Baseline: N/A. Post-change: N/A. Change: N/A. Disposition: N/A. Evidence: N/A - zero TypeScript files changed on this branch.
- Python: Baseline: N/A. Post-change: N/A. Change: N/A. Disposition: N/A. Evidence: N/A - zero Python files changed on this branch.

### 1.2.2 Coverage Artifact State

| Language | Canonical artifact | Verdict | Disposition |
|---|---|---|---|
| PowerShell | `artifacts/pester/powershell-coverage.xml` absent | FAIL | Blocking on the per-file floor breach; procedural on the absent path |
| C# | `artifacts/csharp/coverage.xml` absent | FAIL | Non-blocking and procedural; figures substituted from the committed JaCoCo projection and both clear their floors |
| TypeScript | not required | PASS | zero changed files |
| Python | not required | PASS | zero changed files |

The C# new-code figure of 100.00 percent records a vacuously complete population. Zero `.cs` files
changed on this branch, so the set of changed executable C# lines is empty and no uncovered changed
line exists. The 50 C# files counted in the table are build-configuration files: 15 `.csproj`, 34
`app.config` and `packages.config` manifests, and `.csharpierignore`.

The PowerShell new-code figure of 97.97 percent is the aggregate of the six new files under
`scripts/dependencies/`: 772 covered of 788 instrumented lines.

Per-file PowerShell measurement, read from the JaCoCo document rather than from a summary:

| File | Covered / instrumented | Percent | Class |
|---|---|---|---|
| `scripts/dependencies/AnalyzerItemRepair.psm1` | 106 / 106 | 100.00 | new |
| `scripts/dependencies/PackageGraph.psm1` | 164 / 164 | 100.00 | new |
| `scripts/dependencies/ProjectConsistency.psm1` | 88 / 88 | 100.00 | new |
| `scripts/dependencies/PackageCompatibility.psm1` | 33 / 33 | 100.00 | new |
| `scripts/dependencies/ConsistencyVerifier.psm1` | 157 / 159 | 98.74 | new |
| `scripts/dependencies/Repair-PackageManifestConsistency.ps1` | 224 / 238 | 94.12 | new |
| `scripts/vscode/Sync-PackageReferences.ps1` | 95 / 127 | 74.80 | modified, rewritten |

## 2. General Code Change Policy Compliance

| Requirement | State | Evidence |
|---|---|---|
| File size limit of 500 lines | PASS | largest changed file is `scripts/dependencies/Repair-PackageManifestConsistency.ps1` at 498; `ConsistencyVerifier.psm1` is 493; all other 14 changed scripts are lower |
| Separation of pure logic from I/O | PASS | the five modules are pure over text; every disk interaction is a delegate supplied by the composition root |
| Fail fast and explicitly | PASS | `Invoke-AnalyzerItemRepair` throws on an empty restored listing rather than emitting a guessed path |
| No silent error swallowing | PARTIAL | the repair workflow discards writes when its push gate reads a count that excludes two write classes |
| Comment why, not what | PASS | the non-obvious decisions (character-code separators, preserve rule, pinned NuGet) carry rationale comments |
| Toolchain loop run to a single clean pass | PASS | the phase-9 loop restarted at the formatter after the analyzer reported 18 findings against 13; the restart preceded every later stage, so the final pass is one clean pass |
| Policy documents not modified | PASS | zero files under `.claude/rules/` or `.github/instructions/` in the diff |

The two `scripts/vscode` files that remain unformatted under PSScriptAnalyzer defaults
(`Invoke-MSTest.ps1`, `Invoke-MSTestWithCoverage.ps1`) are unformatted on `main` as well, are clean
under the PoshQC ruleset this repository runs, and were not touched by this change. They are not a
finding against this branch.

## 3. Language-Specific Code Change Policy Compliance

### PowerShell

| Requirement | State | Evidence |
|---|---|---|
| Formatter clean | PASS | PoshQC format rewrote 0 of 46 hashed files on the final pass |
| Analyzer at the recorded baseline | PASS | 13 findings, equal to the phase-0 baseline of 13, after five findings in a new test file were corrected |
| Approved module and function structure | PASS | explicit `Export-ModuleMember`, `Set-StrictMode -Version Latest` in every module |
| Public surface minimal and intentional | FAIL | `Invoke-ProjectConsistencyRepair` is exported from a production module and cannot be called correctly by any consumer |

### C# build-configuration files

| Requirement | State | Evidence |
|---|---|---|
| CSharpier check clean | PASS | `dotnet tool run csharpier check .` reported no findings on the delivered tree |
| Analyzer build non-vacuous | PASS | `/t:Rebuild` used; the phase-9 log carries no skipped compile target |
| Nullable build non-vacuous | PASS | same command shape as `.github/workflows/_build-nullable.yml`, no `/p:Nullable=enable` added |
| `.csharpierignore` scope change justified | PASS | `**/packages.config` and `**/app.config` added with rationale; the scope change was proven live by a control perturbation of a C# file that the same run did report |
| No `.cs` file modified | PASS | the diff contains zero `.cs` paths |

The 34 `app.config` and `packages.config` files in the diff change only in XML layout: they move from
the CSharpier-expanded multi-line form to the inline form the NuGet CLI writes. Ten binding redirects
across six `app.config` files are stale; they were verified stale at the merge base and are untouched
by this change.

## 4. Language-Specific Unit Test Policy Compliance

| Requirement | State | Evidence |
|---|---|---|
| Pester 5 with `New-PesterConfiguration` | PASS | pinned to 5.6.1 on both install and import in `_pester.yml` |
| Arrange, Act, Assert structure | PASS | sampled across all eight new suites |
| Descriptive intent in test names | PASS | criterion-tagged `It` blocks, for example `AC7- resolves net481 through the shared module when net481 is present` |
| Negative and error scenarios covered | FAIL | `tests/scripts/vscode/Sync-PackageReferences.Tests.ps1` carries six tests, all positive-path and all AC7-scoped; the merge-conflict skip, the empty-project-list path, the empty-repair path, the unresolvable-identifier path and the compatibility-rejection handler are untested |
| Property-based tests where the tier requires them | PASS | the changed projects are T4 scaffolding, for which the gate matrix requires none |

## 5. Test Coverage Detail

PowerShell aggregate line coverage is 93.89 percent, 1,598 covered of 1,702 instrumented lines,
read from the JaCoCo report element. The JaCoCo document contains no BRANCH counter, which is the
expected result: Pester measures command and line coverage only, so no branch figure exists to
evaluate and no branch threshold applies.

C# first-party coverage is 85.91 percent line and 80.07 percent branch, summed across the nine
packages of the committed JaCoCo projection. Both clear the 85 line and 75 branch floors in the rules
file and the 80 and 75 floors the runner enforces.

The single coverage breach is `scripts/vscode/Sync-PackageReferences.ps1` at 74.80 percent. Of its 32
uncovered lines, 19 are the `Get-PackageSyncSeam` delegate table and 4 are the top-level invocation.
The remaining 9 are pure logic:

| Line | Uncovered behaviour |
|---|---|
| 151 | `Get-PackageIdentifier` returns empty when no manifest identifier matches the folder |
| 180 | `Resolve-PackageAssetFolder` returns empty when the library directory is absent |
| 248 | the issue #902 rejection handler: no asset folder the target framework can consume ships the file |
| 290 | `Set-ReferenceAssemblyVersion` early return when the Include attribute does not match |
| 293 | `Set-ReferenceAssemblyVersion` early return when the version already agrees |
| 330 | no project file found beside the manifest |
| 336 | merge-conflict markers detected |
| 337 | the corresponding skip return |
| 345 | no repairs produced |

Line 248 is the handling of the exact condition issue #902 introduced. AC7 asserts the shared
selector returns no selection for an unconsumable asset set; nothing asserts what the script does
when it receives that answer.

## 6. Test Execution Metrics

| Suite | Command | Result |
|---|---|---|
| Pester | `Invoke-Pester` over `tests/scripts/dependencies` and `tests/scripts/vscode` with JaCoCo coverage | 302 passed, 0 failed, 0 skipped, exit 0 |
| MSTest | `scripts/vscode/Invoke-MSTestWithCoverage.ps1` | 7,343 tests, exit 0, both runner floors cleared |
| actionlint | `scripts/dev-tools/run-actionlint.ps1` | exit 0, zero bytes of output, 9 workflow files enumerated |

## 7. Code Quality Checks

| Check | Command | Result |
|---|---|---|
| Format check, PowerShell | PoshQC format over the four scan folders | PASS, 0 rewrites of 46 files |
| Lint check, PowerShell | PoshQC analyze over the four scan folders | PASS, 13 findings equal to baseline |
| Format check, C# | `dotnet tool run csharpier check .` | PASS, no findings |
| Analyzer build | `msbuild TaskMaster.sln /t:Rebuild ... /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` | PASS |
| Nullable build | `msbuild TaskMaster.sln /t:Rebuild ... /p:TreatWarningsAsErrors=true` | PASS |
| Confidentiality masking scan | grep for absolute host paths in added lines | FAIL, 74 occurrences across 27 files |
| Workflow change scan | diff filter on `.github/workflows/**` | FAIL, 6 files changed with no green-run evidence at head |
| Suppression scan (added lines) | grep for suppression attributes and pragmas | PASS, none added |

## 8. Gaps and Exceptions

1. **The repair workflow has never executed.** `.github/workflows/dependabot-repair.yml` is new, runs
   with `contents: write` and `pull-requests: write`, and its only verification is actionlint static
   validity plus unit tests of the modules it calls. No run exists.
2. **Three acceptance criteria are unverified.** AC18, AC19 and AC20 require a GitHub App credential
   and an open Dependabot pull request. The deferral is properly measured: the credential query
   returned an empty secrets list and the pull-request query returned zero. It is carried by #914.
3. **The coverage floor conflict is unresolved.** CLAUDE.md states 80 line and 90 for new modules;
   `.claude/rules/general-unit-test.md` and `.claude/rules/quality-tiers.md` state 85 line and 75
   branch. Tracked as open issue #668. Every figure in this audit except `Sync-PackageReferences.ps1`
   clears both readings, so the conflict does not change any verdict here.
4. **One footprint path falls outside the enumerated classes**: the promoted potential entry at
   `docs/features/potential/promoted/2026-09-19-dependabot-fanout-and-ci-failing-nuget-upgrades.md`,
   produced by the feature-promotion commit that predates Phase 0. Accepted.
5. **`Invoke-ProjectConsistencyRepair` ships a latent defect.** Recorded as a major finding rather
   than a gap because it is a property of the delivered code, not of the verification.

## 9. Summary of Changes

- `.github/dependabot.yml`: four topic groups collapsed to one catch-all `all-nuget-updates` group
  with `applies-to: version-updates`; `open-pull-requests-limit` reduced from 10 to 1; all eight
  baseline `ignore` entries preserved verbatim; a new unqualified Deedle ignore added; the inert
  `group-by` key removed from every group.
- `.github/workflows/dependabot-repair.yml`: new `workflow_run`-triggered repair job.
- Four workflows: `nuget-version: latest` pinned to `'7.9.0'`.
- `.github/workflows/_pester.yml`: run and coverage paths extended to `scripts/dependencies` and
  `tests/scripts/dependencies`.
- `.csharpierignore`: `**/packages.config` and `**/app.config` added.
- Five new PowerShell modules and one composition root under `scripts/dependencies/`.
- `scripts/vscode/Sync-PackageReferences.ps1` rewritten; its own framework-preference array deleted
  in favour of the shared compatibility module.
- 15 `.csproj` files: the stale `Meziantou.Analyzer.3.0.203` analyzer item realigned to `3.0.235`.
- 34 `app.config` and `packages.config` files reflowed to the inline NuGet CLI form.
- 139 markdown files of specification, plan, research, runbook and evidence.

## 10. Compliance Verdict

**PARTIAL.** The retroactive repair is verified correct and complete. The forward-prevention half
carries three defects in code that nothing exercises, the PowerShell per-file coverage floor is
breached on one rewritten production file, six workflow files changed without a green run at head,
and 27 committed artifacts leak an absolute host path including the account name.

Remediation inputs are recorded in `remediation-inputs.2026-09-20T01-37.md`.

## Appendix A: Test Inventory

| Suite | Tests | Subject |
|---|---|---|
| `tests/scripts/dependencies/PackageGraph.Tests.ps1` | manifest and project parsing, normalisation | new |
| `tests/scripts/dependencies/ProjectConsistency.Tests.ps1` | four-kind reconciliation, binding redirects, orphan detection, reference completeness, the #908 fixture | new |
| `tests/scripts/dependencies/AnalyzerItemRepair.Tests.ps1` | preserve rule, four path shapes, missing-segment record, language and satellite exclusion | new |
| `tests/scripts/dependencies/ConsistencyVerifier.Tests.ps1` | repair-and-verify entry point, both directions | new |
| `tests/scripts/dependencies/PackageCompatibility.Tests.ps1` | asset-level framework exclusion | new |
| `tests/scripts/dependencies/Repair-PackageManifestConsistency.Tests.ps1` | composition root, skip-and-proceed | new |
| `tests/scripts/dependencies/DependabotConfig.Tests.ps1` | dependabot.yml shape, NuGet pin enumeration, workflow restrictions, README pin equality | new |
| `tests/scripts/vscode/Sync-PackageReferences.Tests.ps1` | six AC7-scoped assertions | new |

Total executed: 302 Pester tests, 0 failed.

## Appendix B: Toolchain Commands Reference

Commands referenced or reproduced during this audit:

- `git -C <worktree> diff --numstat 734112ed2..794d34f02`
- `git -C <worktree> diff --name-only 734112ed2..794d34f02`
- `git -C <worktree> archive 734112ed2 -- '*/*.csproj' '*/packages.config'`
- `dotnet tool run csharpier check .`
- `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
- `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true`
- `Invoke-Pester -Configuration <config with JaCoCo coverage over scripts/dependencies and scripts/vscode>`
- `scripts/dev-tools/run-actionlint.ps1`

Independent verification performed by this review, not reproduced from executor artifacts:

- Parsed `coverage/p9-t3-pester-coverage.iter1.xml` for the report LINE counter, every per-file LINE
  counter, and the uncovered line numbers of `scripts/vscode/Sync-PackageReferences.ps1`.
- Parsed the committed JaCoCo projection and summed the nine package LINE and BRANCH counters.
- Re-derived every restore-path reference in all 18 project and manifest pairs at both the merge base
  and head, and classified each as agreeing, disagreeing, or absent from the manifest.
- Enumerated the Roslyn folders `Meziantou.Analyzer.3.0.235` and `Roslynator.Analyzers.5.0.0` ship on
  disk, and compared them against the folder every analyzer item names.
