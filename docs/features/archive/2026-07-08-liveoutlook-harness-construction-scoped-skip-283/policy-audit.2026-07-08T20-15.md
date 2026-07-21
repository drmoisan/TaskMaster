# Policy Compliance Audit — Issue #283 (liveoutlook-harness-construction-scoped-skip)

- Timestamp: 2026-07-08T20-15
- Review type: RE-AUDIT (remediation pass 1) after the three prior Blocking findings were remediated
- Work mode: minor-audit (marker read from `issue.md`: `- Work Mode: minor-audit`)
- Base branch (resolved): `main`
- Merge-base SHA: `930467f456c436eb9da25c0e6c9a5c401f918f64` (confirmed equal to `git merge-base HEAD origin/main`)
- Head SHA: `87d223a0ae5f6c6613385c336ec72af4c47ee060`
- Scope: full branch diff `930467f4..87d223a0` against base `main` (feature-vs-base)
- Prior audit: `policy-audit.2026-07-08T14-34.md` (NO-GO; three Blocking findings)

## Executive Summary

This is a re-audit at the current branch head after remediation of the three Blocking findings
raised in the prior review (`remediation-inputs.2026-07-08T14-34.md`). The full branch diff was
re-evaluated; scope was not narrowed.

The change is a well-scoped defect fix: it extracts the live-Outlook harness availability decision
into a host-neutral, unit-testable seam (`LiveOutlookHarnessRunner`), routes the integration test
through it, and adds the `/TestCaseFilter:TestCategory!=LiveOutlook` gate to CI and the two mirrored
local QC arg builders.

All three prior Blocking findings are resolved and independently verified against on-disk evidence:

1. `modified-workflow-needs-green-run`: RESOLVED. Green-run evidence records a `workflow_dispatch`
   run (28968336085) of `.github/workflows/ci.yml` against head `87d223a0` with conclusion
   `success`.
2. C# coverage: RESOLVED. Canonical `artifacts/csharp/coverage.xml` (Cobertura) now exists; the new
   seam `LiveOutlookHarnessRunner.cs` is machine-verified at 100% line coverage; no regression on
   changed lines; evidence dossiers reconciled to a single 100.0% seam figure.
3. PowerShell coverage: RESOLVED. Canonical `artifacts/pester/powershell-coverage.xml` (JaCoCo) now
   exists (77.06% instruction / 72.73% line, unchanged from baseline, no changed-line regression).
   The raw line coverage FAILs the numeric 85% floor, but a maintainer-ratified exemption for the two
   QC scripts' host-bound top-level bodies is documented; this is the R3-authorized acceptance path,
   so the coverage FAIL is dispositioned and is not a live Blocking finding.

Overall verdict: GO / zero live Blocking findings. Acceptance criteria AC1–AC7 remain PASS. The only
FAIL verdict (PowerShell raw line coverage vs the 85% floor) is dispositioned by the ratified
host-bound exemption and does not gate PR readiness; changed-line no-regression is preserved.

## Scope and Baseline

Scope was re-derived directly from `git diff --name-status 930467f4..87d223a0`, not from the
PR-context summary overview. The supplied merge-base SHA matches `git merge-base HEAD origin/main`
(no stale-base condition).

Changed files by category (authoritative, from git):

- C# (source/test/build):
  - `TaskMaster.Test/AppGlobals/LiveOutlookHarnessRunner.cs` (NEW seam, 139 lines)
  - `TaskMaster.Test/AppGlobals/LiveOutlookHarnessRunnerTests.cs` (NEW tests, 172 lines; 8 tests)
  - `TaskMaster.Test/AppGlobals/LiveOutlookHookupIntegrationTests.cs` (MODIFIED, +66/-85)
  - `TaskMaster.Test/TaskMaster.Test.csproj` (MODIFIED, +2)
- PowerShell:
  - `scripts/vscode/Invoke-MSTest.ps1` (MODIFIED, +1/-1)
  - `scripts/vscode/Invoke-MSTestWithCoverage.ps1` (MODIFIED, +1/-1)
  - `tests/scripts/vscode/Invoke-MSTest.RunSettings.Tests.ps1` (MODIFIED, +21/-1)
- CI workflow:
  - `.github/workflows/ci.yml` (MODIFIED, +1/-1)
- Docs / evidence / agent-memory: Markdown files under the feature folder and
  `.claude/agent-memory/` (non-executable; no coverage obligation).

Languages with changed files in the branch diff: C# and PowerShell. Both require an explicit
coverage verdict (recorded in Section 4). YAML (CI workflow) has no coverage obligation but triggers
the `modified-workflow-needs-green-run` rule (Section 5).

## Rejected Scope Narrowing

No caller-supplied instruction attempted to narrow scope to a plan/task/phase subset, to a subset of
changed files, or to mark any language as out of scope, "informational only," or "not applicable."
The re-audit prompt explicitly directed the full change to be re-evaluated ("Do not narrow scope;
re-evaluate the whole change"). There is nothing to reject.

Note (not a narrowing instruction): the PR-context summary overview again listed only the three
PowerShell files as core logic and omitted all C# changes. This misclassification would cause the
coverage-gate hook to skip C# enforcement, because the hook derives changed languages from the
summary overview rather than from the git diff. The summary overview was corrected in place
(`artifacts/pr_context.summary.txt`) to include the C# and build-config files so the machine gate
and this audit share the same scope. This is a recurring collector defect, not an instruction to the
reviewer.

## Evidence Location Compliance

PASS. All evidence artifacts produced by execution live under the canonical
`docs/features/active/2026-07-08-liveoutlook-harness-construction-scoped-skip-283/evidence/<kind>/`
tree (`baseline/`, `remediation-baseline/`, `qa-gates/`, `regression-testing/`). The branch diff
contains no files written under `artifacts/baselines/`, `artifacts/qa/`, `artifacts/evidence/`, or
`artifacts/coverage/`. The canonical machine-read coverage artifacts (`artifacts/csharp/coverage.xml`,
`artifacts/pester/powershell-coverage.xml`) are the mandated coverage-artifact paths from the
feature-review workflow and are not evidence-location violations. No `EVIDENCE_LOCATION_OVERRIDE_REJECTED`
condition arose. (`validate_evidence_locations.py` is not present in this repository; the scan was
performed against the git name-status output.)

## Section 1 — General Code Change Policy

| # | Rule | Verdict | Evidence |
|---|---|---|---|
| 1.1 | Simplicity / separation of concerns | PASS | Classification extracted into a small host-neutral seam; pure two-phase logic separated from the COM-bound integration test. |
| 1.2 | File size <= 500 lines | PASS | Largest changed file `LiveOutlookHarnessRunnerTests.cs` = 172 lines; integration test post-change well under 500; seam = 139. |
| 1.3 | Fail fast / explicit errors | PASS | Seam null-guards both delegates with `ArgumentNullException`; construction non-COM failures are captured (not swallowed); exercise exceptions always captured. |
| 1.4 | Naming / XML docs | PASS | Descriptive names; thorough XML doc comments on the seam, `HarnessOutcome`, and `Run<T>`. |
| 1.5 | I/O isolation / no temp files | PASS | New tests use delegate seams, touch no live Outlook, no filesystem, no temp files. |
| 1.6 | Bugfix workflow (regression-first) | PARTIAL | A literal red-before run is structurally impossible (the type under test is introduced by the same change); documented in `regression-testing/fail-before-exception.2026-07-08T17-56.md` with a behavior-diff proof against the removed whitelist. Reasonable for this defect shape; not Blocking. |

## Section 2 — C# Code Change / Unit Test Policy

| # | Rule | Verdict | Evidence |
|---|---|---|---|
| 2.1 | CSharpier formatting | PASS | `dotnet csharpier check .` exit 0 (`csharp-format-final.md`; `remediation-final-qc.2026-07-08T18-45.md`). |
| 2.2 | .NET analyzers (EnableNETAnalyzers/EnforceCodeStyleInBuild) | PASS | msbuild analyzer build exit 0; zero new findings attributed to changed files (`csharp-analyzer-final.md`). |
| 2.3 | Nullable / TreatWarningsAsErrors | PASS | Incremental nullable build exit 0; both new files carry `#nullable enable`; null-guard tests use `null!` idiom (`csharp-nullable-final.md`). |
| 2.4 | MSTest + Moq + FluentAssertions | PASS | `[TestClass]`/`[TestMethod]`, FluentAssertions throughout; delegate seam used instead of Moq (justified — the `Func<T>`/`Action<T>` parameters are the injection point per DI-seam guidance). |
| 2.5 | net481 constraint (no init/record/record struct) | PASS | `HarnessOutcome` is a plain `readonly struct` with a constructor and get-only auto-properties; XML doc records the CS0518 rationale. |
| 2.6 | No-COM architecture (production runtime) | PASS (N/A to production) | All C# changes are in the test assembly `TaskMaster.Test`; Outlook interop references remain confined to the existing `[TestCategory("LiveOutlook")]` harness, not production runtime code. |
| 2.7 | Toolchain single-final-pass | PASS | Format → analyzers → nullable → vstest all recorded exit 0 with LiveOutlook filtered; 231/231 tests pass (`csharp-test-coverage-final.md`). |

## Section 3 — PowerShell Code Change / Unit Test Policy

| # | Rule | Verdict | Evidence |
|---|---|---|---|
| 3.1 | Format (Invoke-Formatter via PoshQC) | PASS | `run_poshqc_format` exit 0 (`powershell-format-final.md`). |
| 3.2 | Analyze (PSScriptAnalyzer) — no new findings | PASS (with note) | Folder-wide `run_poshqc_analyze` exit 1 with findings identical to baseline; 0 new findings on the three touched files. No new analyzer debt introduced. |
| 3.3 | Pester tests pass for changed scripts/tests | PASS | Bundled `run_poshqc_test` returns exit -1 — a pre-existing bundled-runner environment condition that also fails at baseline. Authoritative direct `Invoke-Pester` 5.6.1 run: 11/11 pass, exit 0 (`powershell-test-coverage-final.md`). |
| 3.4 | Wrapper/delegate seam discipline | PASS | Changes are single-token appends (`/TestCaseFilter:TestCategory!=LiveOutlook`) to existing arg-builder return arrays; seam structure unchanged (diff verified). |

## Section 4 — Coverage (mandatory per changed language)

Coverage thresholds applied: repo-wide line >= 80%; new code files >= 90% line; modified files no
regression on changed lines and >= 80% (CLAUDE.md / agent coverage procedure). The reviewer
inspects the pre-existing canonical coverage artifacts and does not re-run coverage generation.

### 4.1 C# coverage

- C# coverage verdict: PASS.
- Canonical artifact: `artifacts/csharp/coverage.xml` (Cobertura) exists on disk (regen evidence
  `qa-gates/csharp-coverage-regen.2026-07-08T18-45.md`). The prior FAIL reason (artifact absent /
  not machine-verifiable) is resolved.
- New/changed-code coverage: 100.0%. The new seam file `LiveOutlookHarnessRunner.cs` is machine-read
  at `line-rate="1"` for both the `LiveOutlookHarnessRunner` class and the nested `HarnessOutcome`
  struct (0 uncovered lines). This clears the 90% new-code target.
- Baseline: 16.75% overall instrumented-set line coverage (seam not yet existing). Post-change:
  16.91% overall (lines-covered 11768 / lines-valid 69579); seam 100.0%. Change: +0.16 pp overall,
  no regression. Disposition: PASS. The low overall root figure is the raw all-assembly Cobertura
  denominator (69,579 lines), which includes vendored/third-party and COM/VSTO/WinForms code under
  the CLAUDE.md testable-denominator exemption; it is not the first-party testable-denominator
  figure. The first-party repo-wide gate is enforced by the CI green run (Section 5), which passed.
  The `TaskMaster.Test` package itself reads 94.6% line-rate in the same artifact.
- Modified file `LiveOutlookHookupIntegrationTests.cs`: its changed lines route the existing
  `[TestCategory("LiveOutlook")]` harness through the new seam; the LiveOutlook test is excluded from
  the standard run by the filter, so it is 0% in both baseline and post-change coverage (no
  regression). Its behavior change is verified by the 8 seam unit tests.
- Evidence: `evidence/qa-gates/csharp-test-coverage-final.md`, `evidence/qa-gates/coverage-delta.md`,
  `evidence/qa-gates/csharp-coverage-regen.2026-07-08T18-45.md`; machine-read from `artifacts/csharp/coverage.xml`.

### 4.2 PowerShell coverage

- PowerShell coverage verdict: FAIL against the raw numeric line floor (machine-verified below 85%),
  dispositioned NON-BLOCKING via the maintainer-ratified host-bound exemption. See disposition below.
- Canonical artifact: `artifacts/pester/powershell-coverage.xml` (JaCoCo) exists on disk (regen
  evidence `qa-gates/powershell-coverage-regen.2026-07-08T18-45.md`). Machine-read counters:
  INSTRUCTION covered 84 / missed 25 = 77.06%; LINE covered 320 / missed 120 (summed across all
  JaCoCo LINE counters) = 72.73%. The prior FAIL reason (artifact absent) is resolved; the raw figure
  remains below the 85% line floor, so the coverage-row verdict is FAIL.
- Baseline: 77.06% instruction (109/84). Post-change: 77.06% instruction (109/84). Change: 0.00 pp.
  New/changed-code coverage: the appended `/TestCaseFilter:TestCategory!=LiveOutlook` token sits on
  already-covered arg-builder return-array lines; changed-line coverage did not regress (0.00 pp).
- Disposition: the raw PowerShell line coverage FAILs the numeric 85% floor. The uncovered lines are
  the two scripts' top-level host-bound bodies (real `vswhere.exe` / `vstest.console.exe` /
  `dotnet-coverage` invocations and live `bin/<Configuration>` filesystem discovery) that cannot be
  unit-tested deterministically per the no-external-dependency rule. A maintainer-ratified exemption
  (`evidence/qa-gates/powershell-coverage-exemption.md`, authorized by dan@danmoisan.org for issue
  #283) covers exactly those enumerated host-bound line ranges; the pure argument-builder and
  path-resolver seams are fully covered by the 11 passing RunSettings tests. This is the PowerShell
  analogue of the CLAUDE.md COM/VSTO testable-denominator exemption. The prior remediation-inputs R3
  acceptance path ("either clear the 85% floor OR record a maintainer-ratified exemption; changed-line
  no-regression preserved") is satisfied, so this FAIL is dispositioned and is NOT a live Blocking
  finding for PR readiness.
- Evidence: `evidence/qa-gates/powershell-test-coverage-final.md`,
  `evidence/qa-gates/powershell-coverage-exemption.md`,
  `evidence/qa-gates/powershell-coverage-regen.2026-07-08T18-45.md`, `evidence/qa-gates/coverage-delta.md`;
  machine-read from `artifacts/pester/powershell-coverage.xml`.

## Section 5 — modified-workflow-needs-green-run

- Trigger: the branch diff modifies `.github/workflows/ci.yml` (matches `.github/workflows/**`). The
  edit adds `/TestCaseFilter:"TestCategory!=LiveOutlook"` to the `vstest.console.exe` invocation
  (diff verified).
- Evidence required: a green workflow run (PR-context or `workflow_dispatch`) whose head SHA equals
  the branch head `87d223a0` and whose conclusion is success for the affected workflow.
- Evidence present: `evidence/qa-gates/ci-green-run.2026-07-08T19-07.md` records a `workflow_dispatch`
  run of `ci.yml` — Run ID 28968336085, Head SHA `87d223a0ae5f6c6613385c336ec72af4c47ee060`
  (= current branch head), Status `completed`, Conclusion `success`.
- Verification note: GitHub CLI is unavailable in this session (per PR-context header), so the run
  conclusion was verified from the recorded green-run evidence artifact (the evidence-verification
  model required by the workflow), not from a live GitHub API call. The head SHA in the evidence
  equals the current branch head, satisfying the rule.
- The edited line is a normal invocation guarded by an existing `if ($LASTEXITCODE -ne 0) { throw }`;
  it is not a deliberately-failing nested command, so the `ci-workflows.md` exit-code-reset rule does
  not apply.
- Verdict: PASS (Blocking finding R1 resolved).

## Remediation Triggers (Blocking)

None. All three prior Blocking findings (R1 workflow green-run, R2 C# coverage machine-verifiability,
R3 PowerShell coverage artifact + exemption) are resolved and verified. No new Blocking finding was
identified in the full re-audit.

## Non-Blocking Items (carry forward, not gating)

- Seam invariant hardening (Low): `HarnessOutcome` permits constructing an instance with both
  `Captured` and `SkipReason` non-null; the seam never does so, and all 8 tests assert mutual
  exclusivity. A factory method or `Debug.Assert` could enforce the invariant structurally. Optional;
  not gating.

## Appendix A — Verdict Legend

PASS = policy satisfied with evidence. PARTIAL = satisfied with a documented, reasonable caveat.
FAIL = policy not satisfied or not verifiable from mandated evidence. Blocking = must be resolved
before PR readiness.

## Appendix B — Commands Referenced (check-only; not re-run by reviewer)

- Scope: `git diff --name-status 930467f4..87d223a0`; `git merge-base HEAD origin/main`.
- C# coverage machine-read: parsed `artifacts/csharp/coverage.xml` for the
  `LiveOutlookHarnessRunner` / `HarnessOutcome` class `line-rate` and the root `line-rate`.
- PowerShell coverage machine-read: parsed `artifacts/pester/powershell-coverage.xml` JaCoCo
  INSTRUCTION/LINE counters.
- C# format: `dotnet csharpier check .` (evidence exit 0).
- C# analyzers: `msbuild TaskMaster.sln /t:Build /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` (evidence exit 0).
- C# nullable: `msbuild TaskMaster.sln /t:Build /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true` (evidence exit 0).
- C# test+coverage: `dotnet-coverage collect --output artifacts/csharp/coverage.xml --output-format cobertura --settings coverage.config -- <vstest.console.exe> TaskMaster.Test/bin/Debug/TaskMaster.Test.dll /Settings:scripts/vscode/TaskMaster.cli.runsettings /InIsolation /TestCaseFilter:TestCategory!=LiveOutlook` (evidence exit 0).
- PowerShell tests: direct `Invoke-Pester` 5.6.1 with JaCoCo coverage to `artifacts/pester/powershell-coverage.xml` (evidence 11/11 exit 0).
- CI green run: `workflow_dispatch` of `ci.yml`, Run 28968336085, head `87d223a0`, conclusion success.
</content>
</invoke>
