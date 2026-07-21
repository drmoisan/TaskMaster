# Policy Compliance Audit — Issue #283 (liveoutlook-harness-construction-scoped-skip)

- Timestamp: 2026-07-08T14-34
- Work mode: minor-audit (marker read from `issue.md`)
- Base branch (resolved): `main`
- Merge-base SHA: `930467f456c436eb9da25c0e6c9a5c401f918f64`
- Head SHA: `143da1ed4906ac02fc02635f99692720e9326632`
- Scope: full branch diff `930467f4..143da1ed` against base `main` (feature-vs-base)

## Executive Summary

The change is a well-scoped defect fix: it extracts the live-Outlook harness availability
decision into a host-neutral, unit-testable seam (`LiveOutlookHarnessRunner`), routes the
integration test through it, and adds the `/TestCaseFilter:TestCategory!=LiveOutlook` gate to CI
and the two mirrored local QC arg builders. The C# design, toolchain, and new-code tests are of
good quality and pass by inspection and per the executor evidence dossiers.

The overall verdict is NO-GO pending remediation. Acceptance criteria AC1–AC7 are substantively
satisfied, but three policy-level gates block PR readiness independently of AC status:

1. `modified-workflow-needs-green-run` fires (`.github/workflows/ci.yml` changed) with no
   green-run evidence — Blocking.
2. The canonical C# coverage artifact (`artifacts/csharp/coverage.xml`) is absent — coverage is
   not machine-verifiable — FAIL.
3. The canonical PowerShell coverage artifact (`artifacts/pester/powershell-coverage.xml`) is
   absent, and the executor-reported QC-script line coverage (77.06%) is below the 85% line
   floor — FAIL.

## Scope and Baseline

Scope was re-derived directly from `git diff --numstat 930467f4..143da1ed`, not from the
PR-context summary overview. The supplied merge-base SHA matches `git merge-base HEAD origin/main`
(no stale-base condition).

Changed files by category (authoritative, from git):

- C# (source/test/build):
  - `TaskMaster.Test/AppGlobals/LiveOutlookHarnessRunner.cs` (NEW, +139)
  - `TaskMaster.Test/AppGlobals/LiveOutlookHarnessRunnerTests.cs` (NEW, +172)
  - `TaskMaster.Test/AppGlobals/LiveOutlookHookupIntegrationTests.cs` (MODIFIED, +66/-85)
  - `TaskMaster.Test/TaskMaster.Test.csproj` (MODIFIED, +2)
- PowerShell:
  - `scripts/vscode/Invoke-MSTest.ps1` (MODIFIED, +1/-1)
  - `scripts/vscode/Invoke-MSTestWithCoverage.ps1` (MODIFIED, +1/-1)
  - `tests/scripts/vscode/Invoke-MSTest.RunSettings.Tests.ps1` (MODIFIED, +21/-1)
- CI workflow:
  - `.github/workflows/ci.yml` (MODIFIED, +1/-1)
- Docs / evidence / agent-memory: 20+ Markdown files under the feature folder and
  `.claude/agent-memory/`.

Languages with changed files in the branch diff: C# and PowerShell. Both require an explicit
coverage verdict (recorded below). YAML (CI workflow) has no coverage obligation but triggers the
`modified-workflow-needs-green-run` rule.

## Rejected Scope Narrowing

No caller-supplied instruction attempted to narrow scope to a plan/task/phase subset, to a subset
of changed files, or to mark any language as out of scope. The orchestrator prompt explicitly
delegated scope determination to the reviewer and reaffirmed the scope invariant. There is nothing
to reject.

Note (not a narrowing instruction): the PR-context summary overview originally listed only the
three PowerShell files as core logic and omitted all C# and CI-workflow changes. This
misclassification would have caused the coverage-gate hook to skip C# enforcement, because the hook
derives changed languages from the summary overview rather than from the git diff. The summary
overview was corrected in place (`artifacts/pr_context.summary.txt`) to include the C# and CI files
so the machine gate and this audit share the same scope.

## Evidence Location Compliance

PASS. All evidence artifacts produced by execution live under the canonical
`docs/features/active/2026-07-08-liveoutlook-harness-construction-scoped-skip-283/evidence/<kind>/`
tree (`baseline/`, `qa-gates/`, `regression-testing/`). The branch diff contains no files written
under `artifacts/baselines/`, `artifacts/qa/`, `artifacts/evidence/`, or `artifacts/coverage/`. No
`EVIDENCE_LOCATION_OVERRIDE_REJECTED` condition arose. (`validate_evidence_locations.py` is not
present in this repository; the scan was performed against the git name-status output.)

## Section 1 — General Code Change Policy

| # | Rule | Verdict | Evidence |
|---|---|---|---|
| 1.1 | Simplicity / separation of concerns | PASS | Classification extracted into a small host-neutral seam; pure logic separated from the COM-bound integration test. |
| 1.2 | File size <= 500 lines | PASS | Largest changed file `LiveOutlookHarnessRunnerTests.cs` = 172 lines; integration test = 177; seam = 139. |
| 1.3 | Fail fast / explicit errors | PASS | Seam null-guards both delegates with `ArgumentNullException`; construction non-COM failures are captured (not swallowed). |
| 1.4 | Naming / XML docs | PASS | Descriptive names; thorough XML doc comments on the seam and `HarnessOutcome`. |
| 1.5 | I/O isolation / no temp files | PASS | New tests use delegate seams, touch no live Outlook, no filesystem, no temp files. |
| 1.6 | Bugfix workflow (regression-first) | PARTIAL | A literal red-before run is structurally impossible (the type under test is introduced by the same change); documented in `regression-testing/fail-before-exception.2026-07-08T17-56.md` with a behavior-diff proof against the removed whitelist. Reasonable for this defect shape. |

## Section 2 — C# Code Change / Unit Test Policy

| # | Rule | Verdict | Evidence |
|---|---|---|---|
| 2.1 | CSharpier formatting | PASS | `dotnet csharpier check .` exit 0 (`csharp-format-final.md`). |
| 2.2 | .NET analyzers (EnableNETAnalyzers/EnforceCodeStyleInBuild) | PASS | msbuild analyzer build exit 0; zero new findings attributed to changed files (`csharp-analyzer-final.md`). |
| 2.3 | Nullable / TreatWarningsAsErrors | PASS | Incremental nullable build exit 0; both new files carry `#nullable enable`; null-guard tests use `null!` idiom (`csharp-nullable-final.md`). |
| 2.4 | MSTest + Moq + FluentAssertions | PASS | `[TestClass]`/`[TestMethod]`, FluentAssertions throughout; delegate seam used instead of Moq (justified — the seam is the injection point). |
| 2.5 | net481 constraint (no init/record/record struct) | PASS | `HarnessOutcome` is a plain `readonly struct` with a constructor and get-only auto-properties. |
| 2.6 | No-COM architecture (production runtime) | PASS (N/A to production) | All C# changes are in the test assembly `TaskMaster.Test`; the Outlook interop references are confined to the existing `[TestCategory("LiveOutlook")]` harness, not production runtime code. |
| 2.7 | Toolchain single-final-pass | PASS | Format → analyzers → nullable → vstest all recorded exit 0 with LiveOutlook filtered; 231/231 tests pass. |

## Section 3 — PowerShell Code Change / Unit Test Policy

| # | Rule | Verdict | Evidence |
|---|---|---|---|
| 3.1 | Format (Invoke-Formatter via PoshQC) | PASS | `run_poshqc_format` exit 0 (`powershell-format-final.md`). |
| 3.2 | Analyze (PSScriptAnalyzer) — no new findings | PASS (with note) | Folder-wide `run_poshqc_analyze` exit 1 with 16 findings = identical to baseline; 0 new findings on the three touched files (the two `PSAvoidUsingWriteHost` findings in `Invoke-MSTest.ps1` are pre-existing and outside the edited function). No new analyzer debt introduced. |
| 3.3 | Pester tests pass for changed scripts/tests | PASS | Mandated `run_poshqc_test` returns exit -1 — a pre-existing bundled-runner environment condition that also fails at baseline (independent of this change). Authoritative direct `Invoke-Pester` 5.6.1 run: 11/11 pass, exit 0 (`powershell-test-coverage-final.md`). |
| 3.4 | Wrapper/delegate seam discipline | PASS | Changes are single-token appends to existing arg-builder return arrays; seam structure unchanged. |

## Section 4 — Coverage (mandatory per changed language)

Coverage thresholds applied: line >= 85%, branch >= 75% (uniform tier rule, `quality-tiers.md`);
new code files target >= 85% line; no regression on changed lines. The reviewer inspects
pre-existing coverage artifacts and does not re-run coverage generation.

### 4.1 PowerShell coverage

- PowerShell coverage verdict: FAIL.
- Canonical PowerShell coverage artifact `artifacts/pester/powershell-coverage.xml` is absent, so
  repo-wide and branch PowerShell coverage cannot be machine-verified from the mandated artifact.
- Executor-reported PowerShell line coverage of the two in-scope QC scripts (direct Pester 5.6.1)
  is 77.06% (commands 109, executed 84) at both baseline and head — below the 85% line floor.
- Baseline: 77.06% (109/84). Post-change: 77.06% (109/84). Change: 0.00 pp (no regression on
  changed lines; the appended `/TestCaseFilter` token sits on already-covered return-array lines).
- Disposition: FAIL — canonical artifact absent and measured PowerShell line coverage is below the
  85% floor. New/changed-code coverage: changed lines covered (no regression), but the file-level
  PowerShell figure does not clear the floor.
- Evidence: `evidence/qa-gates/powershell-test-coverage-final.md`, `evidence/qa-gates/coverage-delta.md`.

### 4.2 C# coverage

- C# coverage verdict: FAIL.
- Canonical C# coverage artifact `artifacts/csharp/coverage.xml` is absent (no scratch Cobertura,
  no populated `TestResults/` file remains), so the reviewer could not independently confirm the
  executor-reported figures from a machine-readable artifact.
- Executor-reported C# figures (prose): new seam file `LiveOutlookHarnessRunner.cs` line coverage
  100.0% (30/30), clearing the 90% new-code target; overall instrumented-set line coverage moved
  from 16.75% baseline to 16.93% head (no regression). The low overall figure reflects the whole
  instrumented set including COM/VSTO/WinForms code under the CLAUDE.md testable-denominator
  exemption and is not the testable-denominator coverage.
- Baseline: 16.75% overall / seam n/e. Post-change: 16.93% overall / seam 100.0% (reported).
  Change: +0.18 pp overall. Disposition: FAIL — canonical C# coverage artifact absent; figures are
  not independently machine-verifiable by this reviewer. New/changed-code coverage: reported 100.0%
  for the new seam file (meets the 90% target if the reported figure is accurate).
- Internal inconsistency to correct: `evidence/qa-gates/ac-verification-summary.md` states the seam
  at 90.0%, while `coverage-delta.md` and `csharp-test-coverage-final.md` state 100.0% (30/30). The
  100.0% figure is the later, coordinator-directed value after the non-COM catch test was added;
  the 90.0% figure in the AC summary is stale.
- Evidence: `evidence/qa-gates/csharp-test-coverage-final.md`, `evidence/qa-gates/coverage-delta.md`.

## Section 5 — modified-workflow-needs-green-run

- Trigger: the branch diff modifies `.github/workflows/ci.yml` (matches `.github/workflows/**`).
- Evidence required: a green workflow run (PR-context or `workflow_dispatch`) whose head SHA equals
  the branch head `143da1ed` and whose conclusion is success for the affected workflow.
- Evidence present: none. PR-context CI status is "not available"; no green-run evidence is recorded
  in the feature folder or in remediation inputs.
- Verdict: FAIL (Blocking). The edited line
  `& $vstestPath $testAssemblies /EnableCodeCoverage /InIsolation /Logger:trx /TestCaseFilter:"TestCategory!=LiveOutlook"`
  is a normal invocation guarded by an existing `if ($LASTEXITCODE -ne 0) { throw }`; it is not a
  deliberately-failing nested command, so `ci-workflows.md` exit-code-reset rule does not apply.
  The Blocking status is solely the absence of a green-run receipt against the branch head.

## Remediation Triggers (Blocking)

1. `modified-workflow-needs-green-run`: obtain and record a green CI or `workflow_dispatch` run
   against head `143da1ed` for `.github/workflows/ci.yml`.
2. C# coverage: persist a canonical C# coverage artifact at `artifacts/csharp/coverage.xml` (or the
   repo-standard Cobertura path consumed by the gate) so the new-seam 100% and overall no-regression
   figures are machine-verifiable; reconcile the 90.0% vs 100.0% seam discrepancy in the evidence.
3. PowerShell coverage: persist a canonical PowerShell coverage artifact at
   `artifacts/pester/powershell-coverage.xml`, and either raise the two QC scripts to the 85% line
   floor or record a maintainer-ratified exemption for the pre-existing untested host-output lines.

## Appendix A — Verdict Legend

PASS = policy satisfied with evidence. PARTIAL = satisfied with a documented, reasonable caveat.
FAIL = policy not satisfied or not verifiable from mandated evidence. Blocking = must be resolved
before PR readiness.

## Appendix B — Commands Referenced (check-only; not re-run by reviewer)

- Scope: `git diff --numstat 930467f4..143da1ed`; `git merge-base HEAD origin/main`.
- C# format: `dotnet csharpier check .` (evidence exit 0).
- C# analyzers: `msbuild TaskMaster.sln /t:Build /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` (evidence exit 0).
- C# nullable: `msbuild TaskMaster.sln /t:Build /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true` (evidence exit 0).
- C# test+coverage: `dotnet-coverage collect --output <scratch>.cobertura.xml --output-format cobertura --settings coverage.config -- <vstest.console.exe> TaskMaster.Test/bin/Debug/TaskMaster.Test.dll /Settings:scripts/vscode/TaskMaster.cli.runsettings /InIsolation /TestCaseFilter:TestCategory!=LiveOutlook` (evidence exit 0; scratch output not persisted to canonical path).
- PowerShell format: `mcp__drm-copilot__run_poshqc_format` (evidence exit 0).
- PowerShell analyze: `mcp__drm-copilot__run_poshqc_analyze` (evidence exit 1, pre-existing folder findings).
- PowerShell test: `mcp__drm-copilot__run_poshqc_test` (evidence exit -1, pre-existing env); direct `Invoke-Pester` 5.6.1 (evidence 11/11 exit 0).
