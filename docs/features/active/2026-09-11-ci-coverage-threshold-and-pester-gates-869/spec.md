# 2026-09-11-ci-coverage-threshold-and-pester-gates (Spec)

- **Issue:** #869 (closes #561 and #562 on merge)
- **Parent (optional):** none
- **Owner:** drmoisan
- **Last Updated:** 2026-09-12
- **Status:** Ready for planning
- **Version:** 1.0
- **Work Mode:** full-bug — this file is the sole acceptance-criteria source.

## Context

CI collects C# coverage but enforces no threshold (#561), and runs no Pester at all, so the production PowerShell under scripts/vscode has zero CI coverage (#562). Both gates are wired in one delivery against the thresholds the maintainer settled on 2026-09-11 under #563: 80 percent line and 75 percent branch for C#, 80 percent line for PowerShell, 90 percent for new code.

The PowerShell metric basis is below the floor today, so this item also raises scripts/vscode coverage above 80 rather than lowering the floor. If it cannot reach the floor, the item halts and reports; see the HALT branch below.

Environment:
- OS/version: GitHub Actions `windows-latest`; local Windows 11 Pro 10.0.26200
- Languages/tooling: GitHub Actions YAML, PowerShell 7, Pester 5.x, vstest, dotnet-coverage
- Entry points under change: `.github/workflows/ci.yml`, `.github/workflows/_mstest-coverage.yml`, `scripts/vscode/Invoke-MSTestWithCoverage.ps1`
- Data source or fixture: `main` at 3cb974422

Impact / Severity:
- [ ] Blocker
- [x] High
- [ ] Medium
- [ ] Low

High: the quality gates the policy documents describe are not enforced on merge.

## Repro & Evidence

Steps to Reproduce:

1. Read `.github/workflows/_mstest-coverage.yml`: coverage is collected and uploaded; no step converts to Cobertura or asserts a floor. The required contexts on the `main` ruleset assert no percentage.
2. Search the workflow files under .github/workflows for `Pester` or `Invoke-Pester`: zero matches. No workflow, VS Code task, or script invokes Pester.
3. Run Pester with coverage over scripts/vscode: the measurement is below 80 percent (figures below).
4. Read `tests/scripts/vscode/Invoke-VSBuild.Tests.ps1`: its `BeforeAll` dot-sources the script's top-level body, which has no invocation guard, so a test run launches vswhere.exe and executes the package-reference sync script against the real repository root. That sync script writes `.csproj` files when a HintPath does not resolve. Measured coverage of the sync script was therefore non-deterministic between runs.

Measured prior basis (read directly from the report-level counters of the 2026-09-03 clean-pass JaCoCo artifact held under the #752 feature folder at docs/features/active/2026-09-03-coverage-assembly-discovery-excludes-own-worktree-root-752/evidence/qa-gates/pester-coverage-cleanpass.2026-09-03T07-23.xml, lines 1049-1057):

| Counter | Covered | Missed | Total | Percent |
| --- | --- | --- | --- | --- |
| INSTRUCTION (Pester "command") | 629 | 174 | 803 | 78.33 |
| LINE | 535 | 146 | 681 | 78.56 |

Per-file LINE counters from the same artifact that bear on the seam: the build script reads 36 covered of 43 (line 920); the package-reference sync script reads 53 covered of 84 (line 1010).

These are measurements, not projections. They are also **stale**: the artifact is dated 2026-09-03 and predates the first-party coverage part named Invoke-MSTestWithCoverage.FirstParty.ps1 and its test file, neither of which this delivery touches, so the denominator it reports is not the current one. Every threshold-delta figure in this specification is expressed as a formula over a Phase 0 re-measurement, never as a fixed number carried forward from this table.

Projection on the prior basis, shown as an estimate for sizing only: 80 percent of 681 lines requires ceil(0.80 x 681) = 545 covered lines. The recommended seam route loses the 53 lines that the accidental execution currently supplies in the sync script while retaining the 36 in the build script under mocks, giving 482 covered, so the uplift requirement is approximately +63 lines. Under a test-only seam the requirement rises to approximately +99 lines against a smaller pool, which is the route that halts.

Actual behaviour today: a coverage regression on `main` cannot fail CI, and a regression in the PowerShell coverage arithmetic or closure filter merges green.

## Scope & Non-Goals

In scope:

- Assert the C# line and branch floors inside the existing MSTest coverage callee, against the first-party projection.
- Add `Assert-CoberturaBranchCoverageThreshold` to `scripts/vscode/Invoke-MSTestWithCoverage.Threshold.ps1` and call it from `scripts/vscode/Invoke-MSTestWithCoverage.ps1` at the existing assertion site.
- Add a reusable Pester callee at `.github/workflows/_pester.yml` and call it from `.github/workflows/ci.yml`.
- Fix the seam defect in `scripts/vscode/Invoke-VSBuild.ps1` and rewrite `tests/scripts/vscode/Invoke-VSBuild.Tests.ps1` against the seams.
- Apply the same Main-extraction-plus-guard treatment to `scripts/vscode/Invoke-Restore.ps1` and add `tests/scripts/vscode/Invoke-Restore.Tests.ps1`.
- Add and extend Pester tests until the scripts/vscode line figure is at or above 80 percent.
- Update `.github/workflows/README.md` to match the changed gate.

Out of scope / non-goals:

- Adding a PSScriptAnalyzer or PoshQC gate to CI. Not run anywhere today; recorded as a follow-up candidate.
- Adding a PowerShell branch-coverage assertion. See decision D4 below.
- Editing the repository coverage settings file named coverage.config. It already excludes the third-party modules that would otherwise break instrumentation and needs no change.
- Adding the new assertion to the helpers script named Invoke-MSTestWithCoverage.Helpers.ps1. That file sits at 470 of its 500-line ceiling; its existing dot-source of the threshold part already resolves the new function for every caller.
- Editing the three rules files under the .claude rules directory (general-unit-test.md, quality-tiers.md, powershell.md). They are push-down-owned from an upstream repository and are overwritten with no templating.
- Editing the `main` branch ruleset. That is a repository-settings change performed by the maintainer out of band; see the manual follow-up section.

Explicitly excluded systems, integrations, or datasets: no product code, no C# source, no solution or project files.

## Root Cause Analysis

- The `build-ci-coverage-gate-fidelity` epic corrected the coverage gate arithmetic without wiring the gate into CI. The arithmetic is therefore correct and unenforced.
- The MSTest callee runs vstest directly with the code-coverage switch and uploads the raw output. Nothing converts it to Cobertura, so the existing assertion function is never reached in CI.
- No Pester invocation exists anywhere in the pipeline, so the production PowerShell that computes the C# coverage figure is itself unmeasured on merge.
- The build script under scripts/vscode is one of two scripts in that directory whose logic sits in an unguarded top-level body. Its test dot-sources the file, so the body executes as an uncontrolled side effect. This is simultaneously the source of the non-determinism and the source of a material share of the directory's measured coverage, which is why the fix must change the production file rather than the test alone.

## Proposed Fix

### Invariant

A merge to `main` must be impossible when first-party C# line coverage is below 80 percent, when first-party C# branch coverage is below 75 percent, when scripts/vscode PowerShell line coverage is below 80 percent, or when the document any of those figures is read from is absent, empty of branches, or otherwise unmeasurable.

### Settled decisions

These were decided by the orchestrator before planning and are recorded here, not re-opened.

**D1 — One CI callee, not two.** The C# threshold assertion goes inside the existing MSTest coverage callee. A separate coverage-threshold callee is rejected: the pipeline has zero `needs:` edges and the callees share no artifacts, so a second callee would duplicate the full restore, build and instrumented test run for no additional signal. Consequence: the delivery adds the single new check-run context predicted as `pester / Run Pester suite with coverage` and no other. The issue text anticipates two new contexts; that figure is **superseded**, because the C# assertion changes no job name and the existing context `mstest-coverage / Run MSTest suite with coverage` keeps reporting under its current name.

**D2 — The C# branch assertion reads the post-processed document.** Add `Assert-CoberturaBranchCoverageThreshold` to `scripts/vscode/Invoke-MSTestWithCoverage.Threshold.ps1`, mirroring the existing line function, reading the `branch-rate` attribute of the document root, and call it from `scripts/vscode/Invoke-MSTestWithCoverage.ps1` immediately after the existing line assertion at the same site. The input is first-party by construction: `ConvertTo-KoverageCoberturaXml` strips non-allowlisted packages and then overwrites the document root counters from the survivors, so the root `branch-rate` on the post-processed document equals the first-party branch rate. Reading a raw collector document instead would fail at roughly 66 percent against the 75 floor for reasons unrelated to first-party code, and is rejected. The assertion fails closed on a missing attribute, a non-numeric value, a value outside the interval from 0 to 1 inclusive, and on `branches-valid="0"`.

**D3 — The build-script fix is a production change.** Extract `Invoke-VSBuildMain`, add the three wrapper seams (`Get-MSBuildPath`, `Invoke-SyncPackageReferences`, `Invoke-MSBuildExe`), and add the `$MyInvocation.InvocationName -ne '.'` guard in `scripts/vscode/Invoke-VSBuild.ps1`, matching the pattern three sibling scripts already use. A test-only seam is rejected: it removes the coverage that the accidental execution currently supplies and pushes the item into the HALT branch. Behaviour is preserved for every existing caller, because callers invoke the file with `-File`, for which the invocation name is the script path and never `.`.

**D4 — The PowerShell gate asserts the LINE figure, not the command figure.** The #563 decision states "PowerShell line 80". The PowerShell rules file states that Pester's command (instruction) coverage carries no threshold and that the line threshold is the one that applies. The gate therefore asserts the JaCoCo `LINE` counter aggregated over scripts/vscode at 80 percent or above, and the evidence artifact additionally records Pester's `CoveragePercent` (the command figure) as informational. No PowerShell branch assertion is added: four independent repository sources agree that Pester measures no branch coverage, and the feature-review agent definition directs reviewers not to record a finding for an absent PowerShell branch figure, so emitting one would be treated as a policy violation.

**D5 — Both metric bases are below 80 today, and both are measurable.** See the measured table under Repro & Evidence.

**D6 — The restore script is planned work, not a reserve.** The research treats it as a contingency held against the command metric, where the margin is roughly 10 commands. On the line metric the margin without it is roughly 9 lines out of 681, which is 1.3 points, and the Phase 0 re-measurement will move the basis. The same Main-extraction-plus-guard treatment therefore applies to `scripts/vscode/Invoke-Restore.ps1`, and `tests/scripts/vscode/Invoke-Restore.Tests.ps1` is planned work rather than a reserve.

**D7 — The prior basis is stale and must be re-derived.** Phase 0 re-measures before any uplift work is scoped, and the required delta is computed as `N = ceil(0.80 * T) - C`, where `T` is the total line count and `C` is the covered line count from the Phase 0 run. The 2026-09-03 figures are quoted only as the prior basis.

**D8 — HALT branch.** If, after the planned uplift work and the tracked-fixture contingency for the package-reference sync script, the scripts/vscode line figure is still below 80, the item halts and reports. It does not lower the floor, does not exclude a production file from measurement, and does not merge the Pester job without its threshold assertion.

**D9 — Two behaviour changes arrive with adopting the local script in CI.** First, MSTest class-level parallelization: the script passes a runsettings file that CI does not pass today, and that file declares class-level scope with one worker per core. This is accepted for the first run, and any new failure is treated as a finding rather than a flake. Second, the trx logger is lost, because the script's argument builder does not pass it; the remedy is to repoint the upload step at the Cobertura document with `if-no-files-found: error`, which requires no production change and publishes the artifact the new gate actually reads.

**D10 — Threshold divergence is recorded, not resolved.** CLAUDE.md states 80 line and 90 new-code and makes no branch claim. Three rules files under the .claude rules directory state 85 line and 75 branch. The #563 decision implements C# line 80, C# branch 75, PowerShell line 80, and new code 90. The PowerShell 80 figure is the only one sitting below a stated rule-file number, so it is recorded here as a documented, maintainer-ratified exception under #563. The rules files are push-down-owned and must not be edited by this delivery.

### Design summary (what changes where)

| Path | Change |
| --- | --- |
| `.github/workflows/_mstest-coverage.yml` | Add the dotnet setup action and a pinned global dotnet-coverage install with the tools directory appended to the job path; replace the inline vstest block with a pwsh invocation of `scripts/vscode/Invoke-MSTestWithCoverage.ps1` that propagates the exit code; repoint the upload step at the Cobertura document with `if-no-files-found: error`. |
| `.github/workflows/_pester.yml` | New reusable callee on `windows-latest`, 10-minute timeout, `permissions: contents: read`, both `workflow_call` and `workflow_dispatch`, no `concurrency` block. Installs a pinned Pester 5.x, builds a `New-PesterConfiguration` over the vscode test tree with JaCoCo coverage over scripts/vscode written to an explicit path under the gitignored coverage directory, emits the counts, asserts the line floor, and exits non-zero on any test failure or sub-threshold figure. |
| `.github/workflows/ci.yml` | Add the `pester` job calling the new callee, keyed and named per the existing convention. |
| `.github/workflows/README.md` | Update the gate table row for the MSTest callee, add the Pester row, update the byte-identical claim about the vstest invocation, record the pinned Pester version, and update the verbatim required-context list. |
| `scripts/vscode/Invoke-MSTestWithCoverage.Threshold.ps1` | Add `Assert-CoberturaBranchCoverageThreshold`, mirroring the existing line assertion. The file moves from 56 lines to roughly 111, well inside the 500-line ceiling. |
| `scripts/vscode/Invoke-MSTestWithCoverage.ps1` | One added call at the existing assertion site, after the post-processing step and after the line assertion. |
| `scripts/vscode/Invoke-VSBuild.ps1` | Extract `Invoke-VSBuildMain`; add the three wrapper seams; add the invocation guard. |
| `scripts/vscode/Invoke-Restore.ps1` | Extract a main function behind an invocation guard, same pattern. |
| `tests/scripts/vscode/Invoke-VSBuild.Tests.ps1` | Rewrite against the seams; add the determinism regression test. |
| `tests/scripts/vscode/Invoke-Restore.Tests.ps1` | New. |
| `tests/scripts/vscode/Invoke-MSTestWithCoverage.Threshold.Tests.ps1` | Add branch-assertion scenarios and the missing out-of-range line scenario. |
| `tests/scripts/vscode/Invoke-MSTestWithCoverage.AssemblyDiscovery.Tests.ps1` | Add the call-site wiring assertion for both threshold functions, and repair the mocked post-processed document so it carries a branch rate and a positive valid-branch count. |
| `tests/scripts/vscode/Invoke-MSTest.RunSettings.Tests.ps1` | Fixture repair only. Three of its tests drive `Invoke-MSTestWithCoverageMain` through the assertion site with a mocked post-processed document that carries no branch rate, so the new branch assertion would throw and turn them red. The mocked document literal gains a branch rate and a positive valid-branch count. The file is at 496 of its 500-line ceiling, so the repair is an in-place literal substitution with no net line growth. |
| `tests/scripts/vscode/Invoke-MSTestWithCoverage.Merge.Tests.ps1` | Extend with the uncovered merge edge branches. |
| `tests/scripts/vscode/Invoke-MSTest.Main.Tests.ps1` | Extend with the vstest-console path resolver. |
| `tests/scripts/vscode/TestProcessCleanup.Tests.ps1` | New; the largest single uplift target. |
| `tests/scripts/vscode/Install-RepoDotNetSdk.Tests.ps1` | Extend with the pure path arithmetic and the two early-exit branches. |

### Boundaries and invariants to preserve

- Both threshold assertions receive the post-processed document string, never raw collector output, and are invoked from the same site in that order: line, then branch, then the first-party report.
- The invocation guard makes dot-sourcing a script a pure definition operation. Dot-sourcing the build script must launch no external process and must write no file.
- Every external dependency reached by the extracted main functions goes through a named wrapper seam. Tests mock the wrapper, never the executable.
- No test creates or reads a temporary file, performs network or archive I/O, or sleeps. A tracked, read-only fixture is permitted and must be byte-identical before and after the suite runs.
- No production file is excluded from coverage measurement.

### Trace: one accepted value through the new assertion

Given a post-processed document whose root carries `branch-rate="0.7500"` and `branches-valid="1200"`, the assertion parses the attribute with invariant culture, confirms it lies within the interval from 0 to 1 inclusive, confirms `branches-valid` is greater than zero, compares 0.7500 against the 0.75 floor, and returns without output, so control reaches the first-party report and the pwsh step exits 0. Given the same document with `branch-rate="0.7499"`, the comparison fails and the function throws a message naming the measured percentage and the 75 floor; the throw terminates the script, the pwsh step exits non-zero, and the MSTest coverage job fails. Given a document with `branches-valid="0"`, the zero-branch guard throws with a distinct message before the comparison is reached, because a first-party projection with nothing to measure must not be readable as a pass.

### Error handling and logging updates

The new assertion raises a terminating error with a distinct message for each of: below threshold, missing attribute, non-numeric value, value outside the interval, and zero valid branches. The Pester step prints the pass, fail, skip and total counts, then the measured line percentage and the informational command percentage, before evaluating the floor, so a failing run records the figures rather than only the exit code.

### Rollback considerations

Each gate is independently revertible by reverting its workflow file. Reverting the production PowerShell changes would restore the non-deterministic measurement and is not a supported rollback path for the Pester gate.

## Write Set

`.github/workflows/_mstest-coverage.yml`
`.github/workflows/_pester.yml`
`.github/workflows/ci.yml`
`.github/workflows/README.md`
`scripts/vscode/Invoke-MSTestWithCoverage.Threshold.ps1`
`scripts/vscode/Invoke-MSTestWithCoverage.ps1`
`scripts/vscode/Invoke-VSBuild.ps1`
`scripts/vscode/Invoke-Restore.ps1`
`tests/scripts/vscode/Invoke-MSTestWithCoverage.Threshold.Tests.ps1`
`tests/scripts/vscode/Invoke-MSTestWithCoverage.AssemblyDiscovery.Tests.ps1`
`tests/scripts/vscode/Invoke-MSTest.RunSettings.Tests.ps1`
`tests/scripts/vscode/Invoke-MSTestWithCoverage.Merge.Tests.ps1`
`tests/scripts/vscode/Invoke-MSTest.Main.Tests.ps1`
`tests/scripts/vscode/Invoke-VSBuild.Tests.ps1`
`tests/scripts/vscode/Invoke-Restore.Tests.ps1`
`tests/scripts/vscode/TestProcessCleanup.Tests.ps1`
`tests/scripts/vscode/Install-RepoDotNetSdk.Tests.ps1`
`docs/features/active/2026-09-11-ci-coverage-threshold-and-pester-gates-869/issue.md`
`docs/features/active/2026-09-11-ci-coverage-threshold-and-pester-gates-869/spec.md`
`docs/features/active/2026-09-11-ci-coverage-threshold-and-pester-gates-869/user-story.md`
`docs/features/active/2026-09-11-ci-coverage-threshold-and-pester-gates-869/plan.2026-09-12T10-25.md`
`docs/features/active/2026-09-11-ci-coverage-threshold-and-pester-gates-869/research/`
`docs/features/active/2026-09-11-ci-coverage-threshold-and-pester-gates-869/evidence/`
`docs/features/potential/promoted/2026-09-11-ci-coverage-threshold-and-pester-gates.md`
`docs/features/potential/2026-09-11-ci-coverage-threshold-and-pester-gates.md`

The last entry is a deletion performed by the promotion step; it is listed because the diff touches it.

Two further paths are created only if the tracked-fixture contingency described under decision D8 is taken. They are listed here so the declared path set stays complete in that case:

`tests/scripts/vscode/fixtures/sync-package-references/packages.config`
`tests/scripts/vscode/fixtures/sync-package-references/SyncFixture.Test.csproj`

The fixture is tracked and read-only. It drives the package-reference sync script down its zero-fix return path and never reaches that script's file-write call. Its project file carries the `.Test` suffix so the first-party project allowlist drops it and the C# coverage projection is unaffected.

## Assumptions, Constraints, Dependencies

Assumptions:

- The runner resolves vswhere.exe and vstest.console.exe at the paths the current MSTest gate already uses successfully, so those dependencies are verified by the existing green gate.
- The dotnet-coverage tool is installed globally rather than through the tool manifest, because the script resolves it with a command lookup and a manifest tool is not on the path. The repository already performs exactly this global install in its Codex environment setup, which is the in-repo precedent.
- An SDK satisfying the repository SDK pin is present on the runner image. This is indirect evidence from the currently green format-check gate, not a verified fact; if the tool install fails against the pin, the remedy is to add the pinned major version to the setup action's version list rather than to alter the SDK pin.

Constraints:

- Production PowerShell files changed by this delivery are `scripts/vscode/Invoke-MSTestWithCoverage.Threshold.ps1`, `scripts/vscode/Invoke-MSTestWithCoverage.ps1`, `scripts/vscode/Invoke-VSBuild.ps1` and `scripts/vscode/Invoke-Restore.ps1`. That set exceeds the three-production-file per-batch cap stated in the PowerShell rules file under the .claude rules directory, so the work is delivered in batches, each batch carrying its own toolchain pass.
- No file may exceed 500 lines.
- No temporary files in tests; the purity hook enforces this mechanically on any test path.
- Whichever parallel-run item touches a shared path first, the scheduler serializes. This specification declares its paths and plans no coordination with any sibling item.

External dependencies: Pester 5.x from the PowerShell Gallery at a pinned version, and dotnet-coverage at a pinned version.

## Data / API / Config Impact

- User-facing or API changes: none. The extracted main functions preserve guard order and messages exactly, and every existing caller invokes the scripts by file path.
- Artifact change: the MSTest coverage job publishes the Cobertura document instead of the trx and raw coverage files, with `if-no-files-found: error`. The Pester job publishes its JaCoCo document on the same terms.
- New check-run context published on pull requests, predicted as `pester / Run Pester suite with coverage`.
- Config: a pinned Pester version and a pinned dotnet-coverage version are recorded in the workflow files and in `.github/workflows/README.md`, so a future bump is a reviewable change rather than a silent image drift.

## Test Strategy

Seeded from the issue:

- Unit coverage areas: the branch-threshold assertion (pass, fail, missing counters), the build-script seam, and new tests on the least-covered scripts under scripts/vscode. Pester, no temporary files.
- Integration scenario to retest: a change that removes one test must go red on the new gate; the run on this item's own pull request must go green.
- Manual verification notes: actionlint passes, and the new job appears as a check run on the pull request.

Detail:

1. **Branch assertion.** Pass at exactly the floor, pass above it, fail below it with the measured percentage in the message, fail on a missing attribute, fail on a non-numeric value, fail on an out-of-range value, and fail on zero valid branches. All driven by one-line here-string documents, matching the technique the existing threshold tests already use.
2. **Call-site wiring.** Assert that the entry point invokes both threshold functions with the post-processed string, after the write-back and in the documented order, using the established filesystem-cmdlet mock set.
3. **Build-script main.** Drive the extracted function with all three seams mocked: both early guard throws, the missing-MSBuild throw, the branch where the sync script is absent, the no-execute path asserted to invoke the MSBuild seam zero times, the executing path, and the non-zero-exit throw.
4. **Determinism regression proof.** A test asserting that dot-sourcing the build script invokes neither the vswhere seam nor the sync seam. This is the regression test for the reported defect and must fail on the pre-fix tree.
5. **Process cleanup.** The no-matching-process early return, a non-matching command line, the breadth-first child walk, a null process lookup, and the confirmation-prompt branch, all under mocked CIM and process cmdlets returning plain object shapes.
6. **Restore script main.** Same shape as item 3 once the guard and main extraction are in place.
7. **Workflow self-validation.** actionlint already lints every workflow file, so the new callee is covered by an existing gate with no new configuration.
8. **Negative-path proof for both gates.** Demonstrate that removing one test turns the Pester gate red and that a deliberately introduced C# regression turns the MSTest gate red. Capture both under the feature folder's evidence directory. A step that deliberately invokes a failing command must reset the last exit code or terminate with an explicit zero exit, per the CI workflow rules.

Toolchain: format, lint, type-check, test, in that order, restarting from the beginning on any failure or auto-fix. Evidence artifacts are written under the feature folder's evidence directory, partitioned by kind.

## Acceptance Criteria

- [ ] (#561) `.github/workflows/_mstest-coverage.yml` produces a Cobertura document in CI by running `scripts/vscode/Invoke-MSTestWithCoverage.ps1`, and the step's non-zero exit code fails the job.
- [ ] (#561) The C# line floor of 80 percent is asserted in CI against the post-processed first-party projection, by the existing line assertion reaching execution on every run.
- [ ] (#561) `Assert-CoberturaBranchCoverageThreshold` exists in `scripts/vscode/Invoke-MSTestWithCoverage.Threshold.ps1`, reads the document-root branch rate, and enforces the 75 percent floor.
- [ ] (#561) `scripts/vscode/Invoke-MSTestWithCoverage.ps1` calls the branch assertion immediately after the existing line assertion, at the same site and on the same post-processed string; a test pins the invocation order and the argument.
- [ ] (#561) The branch assertion fails closed with a distinct terminating message for each of: below floor, missing attribute, non-numeric value, value outside the interval from 0 to 1 inclusive, and zero valid branches. Each case has a test.
- [ ] (#561) A deliberately introduced C# coverage regression turns the MSTest coverage job red, demonstrated and captured as evidence under the feature folder's evidence directory.
- [ ] (#561) Withholding the gate's input does not produce a green run: an absent coverage document fails the job through `if-no-files-found: error` on the upload step, and a zero-branch projection fails through the zero-branch guard.
- [ ] (#562) `.github/workflows/_pester.yml` exists as a reusable callee following the established callee convention: `windows-latest`, per-job timeout, `permissions: contents: read`, both `workflow_call` and `workflow_dispatch`, and no `concurrency` block of its own.
- [ ] (#562) `.github/workflows/ci.yml` calls the new callee, with the job key and job name matching the convention the other callers use.
- [ ] (#562) The Pester job runs the test tree under tests/scripts/vscode with code coverage scoped to scripts/vscode, writes its JaCoCo document to an explicit path rather than relying on the Pester default, and uploads it with `if-no-files-found: error`.
- [ ] (#562) The Pester job asserts the JaCoCo `LINE` figure aggregated over scripts/vscode at 80 percent or above and exits non-zero below it, after printing the measured figure.
- [ ] (#562) The Pester job exits non-zero on any test failure, through an explicit exit placed after the count-emitting statements rather than through the configuration's exit option.
- [ ] (#562) `scripts/vscode/Invoke-VSBuild.ps1` carries an invocation guard, an extracted `Invoke-VSBuildMain` function, and the named wrapper seams `Get-MSBuildPath`, `Invoke-SyncPackageReferences` and `Invoke-MSBuildExe`, and `tests/scripts/vscode/Invoke-VSBuild.Tests.ps1` drives the main function with those seams mocked.
- [ ] (#562) A regression test asserts that dot-sourcing `scripts/vscode/Invoke-VSBuild.ps1` launches no external process and executes no sibling script; the test fails on the pre-fix tree.
- [ ] (#562) The measured coverage of the package-reference sync script is the same on two consecutive clean runs, demonstrating the measurement is deterministic.
- [ ] (#562) `scripts/vscode/Invoke-Restore.ps1` receives the same invocation guard and main extraction, and `tests/scripts/vscode/Invoke-Restore.Tests.ps1` drives it under mocks.
- [ ] (#869) Phase 0 re-measures the scripts/vscode coverage basis before any uplift work is scoped, and the required delta is recorded as `N = ceil(0.80 * T) - C` evaluated against that fresh measurement, with the 2026-09-03 figures cited only as the prior basis.
- [ ] (#869) The final scripts/vscode JaCoCo `LINE` figure is at or above 80 percent, measured on a clean runner and captured as evidence under the feature folder's evidence directory.
- [ ] (#869) If the floor is not reached after the planned uplift work and the tracked-fixture contingency, the item halts and reports. It does not lower any floor, does not exclude any production file from measurement, and does not merge the Pester job without its threshold assertion.
- [ ] (#869) The delivery adds no coverage exclusion matching any production source path, in the repository coverage settings, in the Pester configuration, or by any attribute. The coverage exclusion policy in the general unit test rules treats such an entry as a blocking finding.
- [ ] (#869) No PowerShell branch-coverage assertion, threshold, or reported branch figure is introduced anywhere in the delivery.
- [ ] (#869) The delivery introduces the new required check-run context predicted as `pester / Run Pester suite with coverage` and introduces no other new context; the existing required contexts continue to report under unchanged names, because the C# assertion changes no job name. The issue's expectation of an additional context for the C# threshold is recorded as superseded, with the reason.
- [ ] (#869) The actual context name is captured from a live run against the pull request head SHA using the check-runs query the workflow README prescribes, and recorded in the evidence directory; it is labelled predicted until that capture confirms it.
- [ ] (#869) `.github/workflows/README.md` is updated: the MSTest callee's table row, the claim that its vstest invocation was moved and not edited, the pinned tool versions, the new Pester row, and the verbatim required-context list.
- [ ] (#869) The runsettings-driven MSTest class-level parallelization arriving in CI is recorded as an accepted behaviour change, and the trx logger loss is remedied by repointing the upload step at the Cobertura document.
- [ ] (#869) The threshold divergence is recorded in this specification as a documented, maintainer-ratified exception under #563, with citations, and no rules file under the .claude rules directory is edited.
- [ ] (#869) The production PowerShell work is delivered in batches that respect the per-batch production-file cap, with a full toolchain pass per batch.
- [ ] (#869) New PowerShell code added by this delivery reaches at least 90 percent line coverage, per the new-code floor.
- [ ] (#869) No test creates or reads a temporary file, performs network or archive I/O, or sleeps. If a tracked fixture is used, it is asserted byte-identical before and after the suite runs.
- [ ] (#869) actionlint passes on every changed workflow file, and the full toolchain passes in a single final pass.
- [ ] (#869) No file exceeds 500 lines after the change.

## Manual follow-up (maintainer, out of band)

This is a repository-settings change performed by the maintainer. It is not performed by the delivery, and no plan task covers it.

- The new required check-run context must be added to the `main` branch ruleset, id `18572843`, which has `strict_required_status_checks_policy: true`.
- The predicted context string is `pester / Run Pester suite with coverage`. It is **predicted**, not confirmed.
- The workflow README forbids hand-writing these strings. The value must be captured from a live run against the pull request head SHA using the check-runs query the README prescribes, and the captured value used verbatim.
- The edit is a single atomic PUT. A two-step remove-then-add is prohibited, because the intermediate state leaves the branch unprotected against the removed contexts.
- The existing required contexts stay required and unchanged. The C# threshold assertion changes no job name, so the MSTest coverage context continues to report under its current name and needs no ruleset edit.

## Risks & Mitigations

| # | Risk | Severity | Mitigation |
| --- | --- | --- | --- |
| 1 | The prior coverage basis is stale and predates a script-and-test pair that landed later; no later artifact exists. | High for planning | Re-measure in Phase 0 and re-derive the delta from the fresh totals. |
| 2 | Class-level MSTest parallelization arrives in CI with the runsettings file. | Medium | Accept for the first run; treat any new failure as a finding, not a flake. |
| 3 | The test-results artifact silently empties when the trx logger is dropped. | Low | Repoint the upload at the Cobertura document with `if-no-files-found: error`. |
| 4 | The repository SDK pin could block the global tool install. | Low | Verify on the pull request run; add the pinned major version to the setup action if it fails. |
| 5 | Pester version drift: an unpinned install changes the gate silently on a runner image bump. | Medium | Pin the required version and record it in `.github/workflows/README.md`. |
| 6 | The margin above the 80 percent floor is thin on the prior basis. | Medium | Plan the restore-script uplift as work rather than reserve; hold the tracked-fixture contingency; escalate to the maintainer before any budget override. |
| 7 | The PowerShell 80 floor sits below the 85 stated in three rules files. | Medium | Record the #563 decision as the governing authority and as a ratified exception; do not edit the push-down-owned rules files. |
| 8 | The issue expects two new required contexts; the chosen route produces one. | Low | Recorded as superseded above so the maintainer's ruleset edit uses the correct set. |
| 9 | A sibling item in the same parallel run touches one of the declared paths. | Low | Paths are declared in the write set; the scheduler serializes. No coordination is planned or requested. |

## Rollout & Follow-up

- Rollout: merge the pull request; the maintainer then performs the ruleset edit described above using the captured context name.
- Post-fix monitoring: watch the first few runs of the MSTest coverage job for failures attributable to class-level parallelization, and the first Pester runs for figure stability.
- Follow-up candidates, not in scope here: a PSScriptAnalyzer or PoshQC gate in CI, and reconciliation of the threshold divergence at its upstream source.
- Links: issue #869, closes #561 and #562; threshold decision #563; prior coverage work #752 and #815.

## Numeric assertions and their evidence

One population figure is used in this specification: scripts/vscode holds 12 production PowerShell scripts, all of which are in the coverage denominator. That figure is supported by the `## Numeric Derivation Evidence` section of the research record in this feature folder's research directory, which supplies two independently constructed enumerations over an exhaustive scope using distinct search strategies, enumerates both member sets, and compares them explicitly; both yield the same twelve names. The figure was re-verified against the current worktree on 2026-09-12.

All coverage counters and percentages quoted under Repro & Evidence are measurements read from the named 2026-09-03 artifact with line citations, and are labelled as the prior basis. All forward-looking figures are estimates and are labelled as such. The threshold values 80, 75 and 90 are constants fixed by the #563 maintainer decision, not derived quantities.
