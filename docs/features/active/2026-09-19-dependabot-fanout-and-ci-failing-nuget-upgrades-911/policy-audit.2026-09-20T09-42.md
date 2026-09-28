# Policy Compliance Audit — Issue #911 (dependabot fan-out and CI-failing NuGet upgrades)

- Component: dependency-consistency tooling, Dependabot configuration, CI workflows
- Date: 2026-09-20
- Reviewer: feature-review
- Cycle: re-audit after remediation cycle 1
- Branch: `bug/dependabot-fanout-and-ci-failing-nuget-upgrades-911`
- Head: `db53ca1407592108af5d6792bb05803574b2b768`
- Base: `origin/main`
- Merge base: `b5621910c5b97d2471e368e87e80dc294207111b`
- Work mode: `full-bug` (marker read from `issue.md`)
- Acceptance-criteria source: `spec.md` only
- Prior cycle artifacts: `policy-audit.2026-09-20T01-37.md`, `code-review.2026-09-20T01-37.md`,
  `feature-audit.2026-09-20T01-37.md`, `remediation-inputs.2026-09-20T01-37.md`

## Executive Summary

The branch diff against `origin/main` is **288 files, +32,789 / -6,257, across 32 commits**. The
merge base is `b5621910c`, which is the tip of `origin/main`: the branch took a clean merge of
`origin/main` at `b76cb8c39`, so the two-dot and three-dot ranges coincide. Scope is the full branch
diff and was not narrowed.

**The scope moved between cycles and the move is material.** The prior audit ran against merge base
`734112ed2` and saw 15 `.csproj` files carrying the `Meziantou.Analyzer` realignment. Pull request
#913 landed that identical change on `origin/main`, and the merge absorbed it. **Zero `.csproj` and
zero `.cs` files now appear in the branch diff.** The end state is unchanged and was re-verified
from both ends: across all 18 project and manifest pairs there are 1,498 package restore-path
references (`Import` 234, `Error` 230, `HintPath` 872, `Analyzer` 162) and **zero** disagree with a
sibling manifest at head. What changed is the attribution: three criteria that this branch used to
deliver are now delivered by the base.

All eleven findings the cycle claims to have discharged were re-tested against the delivered code
rather than against the executor's statements. **All eleven are discharged.** The three labelled
narrower than "fixed" — R7 out of scope by D2, R9c visibility-only by D4, R4 working-tree only —
were each tested for label honesty. R7's and R4's labels are accurate. **R9c's is not: the remedy it
describes does not operate under the production invocation.**

Six findings are recorded, none of them Blocking in the sense that a remediation cycle could
discharge it. One Blocking gate remains open and is not remediable here: no CI run exists at the
current head.

| Area | Verdict |
|---|---|
| General Unit Test Policy | PARTIAL |
| General Code Change Policy | PASS |
| PowerShell code change and unit test policy | PASS |
| C# code change policy for build-configuration files | PASS |
| Coverage | FAIL |
| Workflow green-run rule | FAIL |
| Artifact hygiene | PASS |
| Evidence-location compliance | PASS |

Overall verdict: **PARTIAL. Zero remediable blocking findings; one pull-request-time gate
outstanding.**

## Rejected Scope Narrowing

No caller instruction attempted to narrow the audit scope to a plan, task, phase, file subset, or
language subset. The caller directed a full re-audit against `origin/main` and stated explicitly
that this is not a check of the delta. Four factual assertions the caller supplied were accepted as
directed and are not re-derived here: the R3 push-gate line, the R4 measurement of zero, the
coverage figures, and the single clean toolchain pass. Each was nevertheless spot-confirmed from a
primary source, and each confirmation agreed.

One correction is recorded. The caller's stated reason for R1's non-discharge — that this
repository's ruleset does not evaluate checks from a `workflow_dispatch` event — could not be
verified, because the GitHub CLI is unavailable in this environment and no ruleset can be queried.
The framing is also unnecessary. The sufficient reason is simpler and is the one the executor's own
artifact gives: no run of any event type exists at the current head `db53ca140`, and the merge head
is not knowable before the pull request exists.

## Evidence Location Compliance

`git diff --name-only b5621910c..HEAD` was scanned for paths under `artifacts/baselines/`,
`artifacts/qa/`, `artifacts/evidence/` and `artifacts/coverage/`. **Zero matches.** All 230 feature
files sit under the feature folder in the canonical `baseline/`, `issue-updates/`, `other/`,
`qa-gates/`, `regression-testing/` and `remediation-baseline/` subdirectories. No
`EVIDENCE_LOCATION_OVERRIDE_REJECTED` condition arose. Verdict: PASS.

## 1. General Unit Test Policy Compliance

| Requirement | State | Evidence |
|---|---|---|
| Independence, isolation, determinism | PASS | 318 Pester tests; every filesystem interaction sits behind an injected delegate |
| Test file location mirrors source tree | PASS | `tests/scripts/dependencies/` mirrors `scripts/dependencies/`; `tests/scripts/vscode/` mirrors `scripts/vscode/` |
| No temporary files in tests | PASS | the composition root and both rewritten scripts expose delegate seams so fixtures are in memory |
| Scenario completeness for negative, edge and error flows | PASS | the nine uncovered pure-logic lines the prior cycle recorded are each now driven by a behaviour-asserting test with a red-first artifact |
| Line coverage floor | FAIL | one rewritten production file measures 81.89 percent against the 85 percent floor in the rules files |
| No regression on changed lines | PASS | `Sync-PackageReferences.ps1` moved from 0.00 percent on `origin/main` to 81.89 percent; the aggregate moved from 83.93 to 94.43 |
| Coverage exclusion policy | PASS | no production path is excluded from measurement |

### Coverage Evidence Checklist

- C# baseline coverage artifact: `docs/features/active/2026-09-19-dependabot-fanout-and-ci-failing-nuget-upgrades-911/evidence/remediation-baseline/p0-t12-coverage-projection.2026-09-20T01-37.jacoco.xml`
- C# post-change coverage artifact: `docs/features/active/2026-09-19-dependabot-fanout-and-ci-failing-nuget-upgrades-911/evidence/qa-gates/p5-t7-coverage-projection.2026-09-20T01-37.jacoco.xml`
- TypeScript baseline coverage artifact: `N/A - zero TypeScript files changed on this branch`
- TypeScript post-change coverage artifact: `N/A - zero TypeScript files changed on this branch`
- PowerShell baseline coverage artifact: `coverage/p0-t18-pester-coverage.xml`
- PowerShell post-change coverage artifact: `coverage/p5-t3-pester-coverage.iter1.xml`
- Python baseline coverage artifact: `N/A - zero Python files changed on this branch`
- Python post-change coverage artifact: `N/A - zero Python files changed on this branch`
- Per-language comparison summary: the per-language coverage comparison block of this document

**Coverage Metrics by Language:**

| Language | Files Changed | Tests | Test Result | Baseline Coverage | Post-Change Coverage | New Code Coverage |
|---|---|---|---|---|---|---|
| PowerShell | 15 | 318 | 318 passed, 0 failed | 83.93% lines | 94.43% lines | 97.98% |
| C# | 35 | 7343 | passed | 85.92% lines / 80.09% branch | 85.93% lines / 80.10% branch | 100.00% |
| TypeScript | 0 | 0 | N/A | N/A | N/A | N/A |
| Python | 0 | 0 | N/A | N/A | N/A | N/A |

### 1.2.1 Per-Language Coverage Comparison

- PowerShell: Baseline: 83.93% lines (731/871). Post-change: 94.43% lines (1611/1706). Change: +10.50% lines (880 additional covered lines). New/changed-code coverage: 97.98%. Disposition: FAIL. Evidence: parsed directly from the JaCoCo documents by this review; the aggregate clears every floor but the rewritten file `scripts/vscode/Sync-PackageReferences.ps1` measures 81.89 percent, above the 80 in CLAUDE.md and below the 85 in the rules files.
- C#: Baseline: 85.92% lines (56482/65737) / 80.09% branch (13657/17052). Post-change: 85.93% lines (56486/65737) / 80.10% branch (13658/17052). Change: +0.01% lines and +0.01% branch on an identical denominator, inside run-to-run variation. New/changed-code coverage: 100.00%. Disposition: FAIL. Evidence: parsed directly from the two committed JaCoCo projections; both figures clear the 85 line and 75 branch floors, and the recorded FAIL is procedural, for the absent canonical artifact path recorded in the artifact-state table below.
- TypeScript: Baseline: N/A. Post-change: N/A. Change: N/A. Disposition: N/A. Evidence: N/A - zero TypeScript files changed on this branch.
- Python: Baseline: N/A. Post-change: N/A. Change: N/A. Disposition: N/A. Evidence: N/A - zero Python files changed on this branch.

### 1.2.2 Coverage Artifact State

| Language | Canonical artifact | Verdict | Disposition |
|---|---|---|---|
| PowerShell | `artifacts/pester/powershell-coverage.xml` absent | FAIL | Non-blocking; procedural on the absent path and structural on the per-file floor |
| C# | `artifacts/csharp/coverage.xml` absent | FAIL | Non-blocking and procedural; figures substituted from the two committed JaCoCo projections and both clear their floors |
| TypeScript | not required | PASS | zero changed files |
| Python | not required | PASS | zero changed files |

The C# new-code figure of 100.00 percent records a vacuously complete population. Zero `.cs` files
changed on this branch, so the set of changed executable C# lines is empty and no uncovered changed
line exists. The 35 C# files counted in the table are build-configuration files: 34 `app.config` and
`packages.config` manifests, plus `.csharpierignore`. The 15 `.csproj` files the prior cycle counted
left the diff when `origin/main` absorbed pull request #913.

The PowerShell new-code figure of 97.98 percent is the aggregate of the six new files under
`scripts/dependencies/`: 776 covered of 792 instrumented lines.

Per-file PowerShell measurement, read from the JaCoCo document rather than from a summary:

| File | Covered / instrumented | Percent | Class |
|---|---|---|---|
| `scripts/dependencies/AnalyzerItemRepair.psm1` | 106 / 106 | 100.00 | new |
| `scripts/dependencies/PackageGraph.psm1` | 164 / 164 | 100.00 | new |
| `scripts/dependencies/ProjectConsistency.psm1` | 103 / 103 | 100.00 | new |
| `scripts/dependencies/PackageCompatibility.psm1` | 33 / 33 | 100.00 | new |
| `scripts/dependencies/ConsistencyVerifier.psm1` | 158 / 160 | 98.75 | new |
| `scripts/dependencies/Repair-PackageManifestConsistency.ps1` | 212 / 226 | 93.81 | new |
| `scripts/vscode/Sync-PackageReferences.ps1` | 104 / 127 | 81.89 | modified, rewritten |

## 2. General Code Change Policy Compliance

| Requirement | State | Evidence |
|---|---|---|
| File size limit of 500 lines | PASS | every file in the 17-file footprint is at or under 500; the largest is `scripts/dependencies/ConsistencyVerifier.psm1` at 499 |
| Separation of pure logic from I/O | PASS | the five modules are pure over text; every disk interaction is a delegate supplied by the composition root |
| Fail fast and explicitly | PASS | the repair workflow's commit step now throws a named error on either missing identity input rather than writing a silent bad identity |
| No silent error swallowing | PASS | the push gate reads the write set, so a run that wrote a file commits and pushes it |
| Comment why, not what | PASS | every remediation edit carries a rationale comment naming the finding and the restoration condition |
| Toolchain loop run to a single clean pass | PASS | the final loop ran format, analyze, Pester, CSharpier, both msbuild gates and MSTest in order with no restart |
| Scope control against the declared write set | PASS | all 57 changed non-documentation paths appear in the `## Write Set` section of `spec.md`; zero outside it |
| Policy documents not modified | PASS | zero files under `.claude/rules/` or `.github/instructions/` in the diff |

The `Repair-PackageManifestConsistency.ps1` size finding from the prior cycle was discharged by
genuine extraction, not compaction: `Resolve-ReferenceAssemblyVersion` moved out to
`ProjectConsistency.psm1` and the file went from 498 to 470 lines. `ConsistencyVerifier.psm1` went
from 493 to 499 and carries one line of headroom, recorded with a standing instruction to extract
rather than append.

## 3. Language-Specific Code Change Policy Compliance

### PowerShell

| Requirement | State | Evidence |
|---|---|---|
| Formatter clean | PASS | PoshQC format rewrote 0 of 46 hashed files on the final pass |
| Analyzer at the recorded baseline | PASS | 13 findings, equal to the phase-0 baseline of 13, after a batch of nine surplus findings in a test file forced a documented restart at the formatter |
| Approved module and function structure | PASS | explicit `Export-ModuleMember`, `Set-StrictMode -Version Latest` in every module |
| Public surface minimal and intentional | PASS | `Invoke-ProjectConsistencyRepair` no longer rewrites a Reference assembly version it holds no evidence for; the destructive default is gone |
| `SupportsShouldProcess` on state-changing actions | PASS | every write in the composition root is guarded by `$PSCmdlet.ShouldProcess`, and the normalisation pass is passed `-WhatIf:$WhatIfPreference` explicitly |

### C# build-configuration files

| Requirement | State | Evidence |
|---|---|---|
| Formatter check clean | PASS | `dotnet tool run csharpier check .` reported no findings on the delivered tree |
| Analyzer build non-vacuous | PASS | `/t:Rebuild` used; the log carries 36 compiler invocations across 18 output assemblies, independently counted by this review |
| Nullable build non-vacuous | PASS | same command shape as `.github/workflows/_build-nullable.yml`, no `/p:Nullable=enable` added |
| `.csharpierignore` scope change justified | PASS | `**/packages.config` and `**/app.config` added with rationale, proven live by a control perturbation in the prior cycle |
| No `.cs` or `.csproj` file modified | PASS | the diff contains zero paths with either extension |

The 34 `app.config` and `packages.config` files in the diff change only in XML layout: they move
from the CSharpier-expanded multi-line form to the inline form the NuGet CLI writes. Ten binding
redirects across six `app.config` files remain stale; they were verified stale at the merge base and
are untouched by this change.

## 4. Language-Specific Unit Test Policy Compliance

| Requirement | State | Evidence |
|---|---|---|
| Pester 5 with `New-PesterConfiguration` | PASS | pinned to 5.6.1 on both install and import in `_pester.yml` |
| Arrange, Act, Assert structure | PASS | the three files with explicit markers carry 17/11/16, 14/14/14 and 13/13/12; the composition-root suite arranges and acts in a `Context`-level `BeforeAll` and asserts per `It`, which is the accepted Pester shape |
| Descriptive intent in test names | PASS | criterion-tagged and finding-tagged `It` blocks, for example `R2- warns and records no repair when no asset folder the target framework can consume ships the file` |
| Negative and error scenarios covered | PASS | eight new `It` blocks cover the nine previously uncovered pure-logic lines; two of them carry explicit non-vacuity guards on the warning set before asserting its text |
| Assertions not weakened | PASS | no test file lost an `It`, a `Should`, a `-Because` or an AAA marker across the cycle; every count is equal or higher |
| Property-based tests where the tier requires them | PASS | the changed projects are T4 scaffolding, for which the gate matrix requires none |

The compaction claim for the two files brought under their ceilings could not be tested by direct
comparison, because the 510-line and 479-line intermediates were never committed. It is corroborated
instead by a census across the whole cycle: `DependabotConfig.Tests.ps1` moved from 11 `It` blocks,
27 assertions and 11/7/10 AAA markers to 17, 55 and 17/11/16, and every one of its 55 assertions
carries a `-Because` clause. No file in the suite regressed on any of those counts.

## 5. Test Coverage Detail

PowerShell aggregate line coverage is 94.43 percent, 1,611 covered of 1,706 instrumented lines, read
from the JaCoCo report element by this review. The JaCoCo document contains no BRANCH counter, which
is the expected result: Pester measures command and line coverage only, so no branch figure exists
to evaluate and no branch threshold applies.

C# first-party coverage is 85.93 percent line and 80.10 percent branch, summed across the nine
packages of the committed JaCoCo projection. Both clear the 85 line and 75 branch floors in the
rules files and the 80 and 75 floors the runner enforces. The denominators are identical to the
remediation baseline at 65,737 lines and 17,052 branches, which is the expected result for a cycle
that changed no C# source.

The single coverage breach is `scripts/vscode/Sync-PackageReferences.ps1` at 81.89 percent. The nine
pure-logic lines the prior cycle recorded are all covered now. The 23 that remain are structural:

| Lines | Content | Reachable without the real filesystem |
|---|---|---|
| 60-91 (19 lines) | the `Get-PackageSyncSeam` production delegate table | no |
| 387, 390 | the default-resolution branches of `Invoke-PackageReferenceSync` | no |
| 410 | the non-zero-fix summary branch | **yes** |
| 422 | the dot-source guard on the bottom-line auto-invocation | no |

The ceiling claim in decision D5 holds. Reaching 85 percent needs 108 of 127 lines covered; only
line 410 is drivable through the injected seam, so the maximum reachable without engaging the real
filesystem is 105 of 127, or 82.68 percent. The Coverage Exclusion Policy forbids excluding the
delegate table from measurement and prescribes exactly the shape this file already has: all logic
extracted into testable functions, with the thinnest possible wiring left in the seam. The shortfall
is therefore a visible and permitted cost rather than an untested behaviour.

The baseline for this file on `origin/main` is 0.00 percent, 0 of 84 instrumented lines. The change
takes it from entirely untested to 104 covered lines.

## 6. Test Execution Metrics

| Suite | Command | Result |
|---|---|---|
| Pester | `Invoke-Pester` over `tests/scripts/dependencies` and `tests/scripts/vscode` with JaCoCo output | 318 passed, 0 failed, 0 skipped, exit 0 |
| MSTest | `scripts/vscode/Invoke-MSTestWithCoverage.ps1` | 7,343 tests, exit 0, both runner floors cleared |
| actionlint | `scripts/dev-tools/run-actionlint.ps1` | exit 0, zero bytes of output |
| CI at `de9a00106` | `gh workflow run CI --ref <branch>` | run 35513025198, six of six jobs success, event `workflow_dispatch` |

## 7. Code Quality Checks

| Check | Command | Result |
|---|---|---|
| Format check, PowerShell | PoshQC format over the four scan folders | PASS, 0 rewrites of 46 files |
| Lint check, PowerShell | PoshQC analyze over the four scan folders | PASS, 13 findings equal to baseline |
| Format check, CSharpier | `dotnet tool run csharpier check .` | PASS, 1,623 files, no findings |
| Analyzer build | `msbuild TaskMaster.sln /t:Rebuild ... /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` | PASS, 0 warnings, 0 errors, 36 compiler invocations |
| Nullable build | `msbuild TaskMaster.sln /t:Rebuild ... /p:TreatWarningsAsErrors=true` | PASS, 0 warnings, 0 errors |
| Confidentiality masking scan | pattern scan for the account name and for a drive-qualified user path across all 288 changed files | PASS, 0 and 0 |
| Workflow change scan | diff filter on `.github/workflows/**` | FAIL, 6 files changed with no run of any event type at head `db53ca140` |
| Suppression scan (added lines) | scan for suppression attributes and pragmas | PASS, none added |
| Write-set scope control | changed non-documentation paths against the `spec.md` write set | PASS, 57 of 57 declared |

## 8. Gaps and Exceptions

1. **The repair workflow has still never executed.** `.github/workflows/dependabot-repair.yml` runs
   with `contents: write` and `pull-requests: write`, and its only verification remains actionlint
   static validity plus unit assertions over its own text. All four remediation edits inside it were
   verified statically by this review — each defective construct measured at zero occurrences and
   each replacement at the expected count — but no runtime behaviour is observed.
2. **Three acceptance criteria are unverified.** AC18, AC19 and AC20 require a GitHub App credential
   and an open Dependabot pull request. The deferral is properly measured and is carried by #914.
3. **The coverage floor conflict is unresolved.** CLAUDE.md states 80 line and 90 for new modules;
   `.claude/rules/general-unit-test.md` and `.claude/rules/quality-tiers.md` state 85 line and 75
   branch. Tracked as open issue #668. Only `Sync-PackageReferences.ps1` sits between the two
   readings; every other figure in this audit clears both.
4. **Three criteria are now satisfied by the base rather than by the branch.** AC5, AC12 and AC13
   concern the analyzer-item realignment that pull request #913 landed on `origin/main`. The end
   state at head is correct and was re-derived independently, but a reader of the branch diff alone
   will not see the change.
5. **One footprint path falls outside the enumerated classes**: the promoted potential entry at
   `docs/features/potential/promoted/2026-09-19-dependabot-fanout-and-ci-failing-nuget-upgrades.md`.
   Accepted, as in the prior cycle.
6. **The pull-request context artifact has reintroduced two false autoclose candidates.** The
   regenerated `artifacts/pr_context.summary.txt` again lists `#MEZIANTOU-898` and `#SHA-256`,
   exactly as the merge-time instructions predicted it would. The obligation to strip them is live.

## 9. Summary of Changes

Relative to `origin/main` at `b5621910c`:

- `.github/dependabot.yml`: four topic groups collapsed to one catch-all `all-nuget-updates` group
  with `applies-to: version-updates`; `open-pull-requests-limit` reduced from 10 to 1; all eight
  baseline `ignore` entries preserved; a new unqualified Deedle ignore added; the inert `group-by`
  key removed from every group.
- `.github/workflows/dependabot-repair.yml`: new `workflow_run`-triggered repair job, 173 lines,
  carrying the four remediation edits.
- Four workflows: `nuget-version: latest` pinned to `'7.9.0'`.
- `.github/workflows/_pester.yml`: run and coverage paths widened to two-member arrays covering
  `scripts/dependencies` and `tests/scripts/dependencies`.
- `.csharpierignore`: `**/packages.config` and `**/app.config` added.
- Five new PowerShell modules and one composition root under `scripts/dependencies/`.
- `scripts/vscode/Sync-PackageReferences.ps1` rewritten onto the shared compatibility module.
- Eight new Pester suites carrying 318 tests.
- 34 `app.config` and `packages.config` files reflowed to the inline NuGet CLI form.
- 230 markdown and evidence files of specification, plan, research, runbook and audit trail.

## 10. Compliance Verdict

**PARTIAL.** The retroactive repair is verified correct and complete at head. Every one of the
eleven findings the remediation cycle claims to have discharged is discharged, tested against the
delivered code rather than against the executor's statements. The forward-prevention half of the
change is now free of the four defects the prior cycle found in it.

What remains: one pull-request-time gate that no remediation cycle can discharge (no CI run at the
current head), one operator-facing documentation statement that decision D2 made false, one remedy
that does not operate under the production invocation, and a per-file coverage figure that is
structurally capped below the rules floor.

**Remediable blocking findings: zero.** Remediation inputs are recorded in
`remediation-inputs.2026-09-20T09-42.md` as an itemised fix list, not as a cycle trigger.

## Appendix A: Test Inventory

| Suite | Tests | Subject |
|---|---|---|
| `tests/scripts/dependencies/PackageGraph.Tests.ps1` | 32 `It` | manifest and project parsing, normalisation |
| `tests/scripts/dependencies/ProjectConsistency.Tests.ps1` | 17 `It` | four-kind reconciliation, binding redirects, orphan detection, reference completeness, the #908 fixture |
| `tests/scripts/dependencies/AnalyzerItemRepair.Tests.ps1` | 13 `It` | preserve rule, four path shapes, missing-segment record, language and satellite exclusion |
| `tests/scripts/dependencies/ConsistencyVerifier.Tests.ps1` | 12 `It` | repair-and-verify entry point, both directions, the R5 preservation assertion |
| `tests/scripts/dependencies/PackageCompatibility.Tests.ps1` | 8 `It` | asset-level framework exclusion |
| `tests/scripts/dependencies/Repair-PackageManifestConsistency.Tests.ps1` | 31 `It` | composition root, skip-and-proceed, write-set reporting |
| `tests/scripts/dependencies/DependabotConfig.Tests.ps1` | 17 `It` | dependabot.yml shape, NuGet pin enumeration, workflow restrictions, README pin equality, the four workflow-text assertions for R3, R6, R7 and R8 |
| `tests/scripts/vscode/Sync-PackageReferences.Tests.ps1` | 14 `It` | six AC7-scoped assertions plus the eight R2 negative and error paths |

Total executed across the two scan folders: 318 Pester tests, 0 failed, 0 skipped.

## Appendix B: Toolchain Commands Reference

Commands referenced or reproduced during this audit:

- `git -C <worktree> diff --numstat b5621910c..db53ca140`
- `git -C <worktree> diff --name-only b5621910c..db53ca140`
- `git -C <worktree> log --oneline --name-status 794d34f02..HEAD`
- `git -C <worktree> ls-tree -r --name-only db53ca140`
- `dotnet tool run csharpier check .`
- `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
- `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true`
- `Invoke-Pester -Configuration <config with JaCoCo output over scripts/dependencies and scripts/vscode>`
- `scripts/dev-tools/run-actionlint.ps1`

Independent verification performed by this review, not reproduced from executor artifacts:

- Re-derived every package restore-path reference in all 18 project and manifest pairs at head and
  classified each as agreeing, disagreeing, or absent from the manifest: 1,498 references, zero
  disagreements, 11 orphans.
- Parsed `coverage/p0-t18-pester-coverage.xml`, `coverage/p0-t8-pester-coverage.xml` and
  `coverage/p5-t3-pester-coverage.iter1.xml` for the report LINE counter, every per-file LINE
  counter, and the uncovered line numbers of `scripts/vscode/Sync-PackageReferences.ps1`.
- Parsed both committed JaCoCo projections and summed the nine package LINE and BRANCH counters.
- Scanned all 288 changed files for the account name, for a drive-qualified user path, and for a
  Unix home path.
- Counted compiler invocations in the analyzer build log to confirm the build was not vacuous.
- Compared every changed non-documentation path against the `spec.md` write set.
- Censused 912 `<Reference Include="...">` elements against their sibling manifest identifiers for
  case divergence.
- Censused `It`, `Should`, `-Because` and Arrange/Act/Assert marker counts in all eight suites at
  the prior cycle head and at this head.
