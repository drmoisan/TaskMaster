# `2026-08-14-ci-parallel-job-split` — User Story

- Issue: #553
- Owner: drmoisan
- Status: Ready for planning
- Last Updated: 2026-08-14

## Story Statement

- As a **repository contributor waiting on CI**, I want the formatting, analyzer,
  nullable, and MSTest gates to run as concurrent jobs, so that a pull request's
  wall-clock CI latency approaches the duration of the slowest single gate
  (estimated ~277s) instead of the measured 444s sum of all gates, and so that a
  fast gate such as formatting reports within minutes instead of after restore and
  two full builds.
- As a **maintainer diagnosing a red check**, I want each gate to report as its own
  named required status check, so that I can identify the failed gate from the PR
  checks list without opening a log, and re-dispatch only that gate via
  `workflow_dispatch` instead of re-running the entire pipeline.

## Problem / Why

`.github/workflows/ci.yml` runs the entire C# quality toolchain inside a single
`quality-gates` job on `windows-latest` with a 60-minute timeout. That job executes
five expensive stages strictly sequentially:

1. `nuget restore`
2. `dotnet csharpier check .` (formatting)
3. `msbuild /t:Build` with `EnableNETAnalyzers` + `EnforceCodeStyleInBuild` (analyzers)
4. `msbuild /t:Rebuild` with `TreatWarningsAsErrors=true` (nullable/type-check)
5. `vstest.console.exe` with `/EnableCodeCoverage` (MSTest suite)

Wall-clock CI latency is therefore the sum of all five stages — measured at **444s**
in the sequential baseline
(`evidence/baseline/ci-sequential-baseline.2026-08-14T13-05.md`). Stages 3 and 4
are both full solution compiles, and stage 4 uses `/t:Rebuild`, so it performs a
complete recompile from scratch. A formatting violation is not reported until after
restore completes, and a test failure is not reported until after two full builds
have finished.

Two additional consequences of the monolithic shape:

- **No independent failure signal.** All gates report as one required status check
  named `Format, build, analyze, and test`, so a red check does not identify which
  gate failed without opening the log.
- **No independent re-dispatch.** A transient failure in one stage requires
  re-running the entire job.

## Personas & Scenarios

- **Persona: contributor waiting on CI.**
  - Pushes commits to a PR branch several times per session and waits for the
    required checks before requesting review or merging.
  - Cares about time-to-first-signal (a formatting or analyzer failure known
    early) and total time-to-green.
  - Constraint: cannot merge until every required context reports success under
    the `main` ruleset's strict policy.
  - Frustration today: a 15-second formatting check is not reported until restore
    completes, and total latency is 7m24s per push even when nothing is wrong.

- **Persona: maintainer diagnosing a red check.**
  - Triages failed PR runs and decides whether a failure is a code defect, a gate
    regression, or a transient infrastructure error.
  - Cares about attributing a failure to the correct gate immediately and about
    re-running only the affected gate.
  - Constraint: must keep every gate enforced as a required check with no window
    in which a merge can bypass a gate during the migration.
  - Frustration today: one aggregate check name forces opening the 60-minute job
    log to find which of five stages failed, and any re-run repeats all stages.

- **Scenario: fast failure attribution.** A contributor pushes a commit with a
  nullable violation. Under the split pipeline, the analyzer, format, actionlint,
  and MSTest checks all complete green; only the nullable gate's context reports
  red. The contributor identifies the failed gate from the checks list, fixes the
  violation, and pushes; the superseded run is cancelled as a group by the caller's
  `cancel-in-progress` concurrency setting.

- **Scenario: transient infrastructure failure.** A maintainer sees the MSTest
  gate fail with a runner-provisioning error while all other gates are green.
  Instead of re-running the whole pipeline, the maintainer dispatches
  `_mstest-coverage.yml` standalone via `workflow_dispatch` (procedure documented
  in `.github/workflows/README.md`) to confirm the failure is transient, then
  re-runs the failed job on the PR.

- **Scenario: required-check migration.** A maintainer merges this feature. The
  split PR's own run reports the five new contexts; the maintainer captures the
  exact check-run names from the live head SHA, applies one atomic PUT replacing
  the ruleset's required contexts with the complete new set, and merges
  immediately. At no point is any gate non-required: a missing context blocks
  merging (fail-closed) rather than allowing a bypass.

## Acceptance Criteria

These criteria are consistent with the draft in `issue.md`, resolved against the
adopted design (research topology (c) plus actionlint extraction). Where a draft
criterion admitted alternatives, the resolved form is stated.

- [x] The formatting gate, the analyzer build gate, the nullable build gate, and
      the MSTest gate each run as separate GitHub Actions jobs with no `needs:`
      edge forcing them to serialize. Resolved form: the adopted topology shares
      no build output between jobs, so the pipeline contains **zero** `needs:`
      edges (the draft's artifact-consumption exception is unused).
- [x] Each gate is a callable reusable workflow `_<name>.yml` declaring both
      `on: workflow_call:` and `on: workflow_dispatch:`.
- [x] `ci.yml` becomes an orchestrator workflow containing only `uses:` references
      and no inline `steps:`. Resolved form: the `actionlint` job is also
      extracted into `_actionlint.yml`, so this criterion holds without exception
      and the pipeline comprises five callee workflows and five required
      status-check contexts.
- [x] Any file shared between jobs crosses the boundary via explicit
      `actions/upload-artifact` + `actions/download-artifact`. Resolved form: no
      cross-job file sharing exists in the adopted topology; the only artifact
      operation is the preserved `test-results` upload to workflow storage, with
      `if: always()` and the same artifact name and paths as today.
- [x] The `main` branch ruleset's `required_status_checks` contexts are updated to
      match the new context names, with no window in which a merge can bypass a
      gate: exact context strings are captured from a live green run on the PR
      head, and the update is a single atomic PUT of the full writable ruleset
      object.
- [x] `.github/workflows/README.md` documents the per-stage `workflow_dispatch`
      procedure and the branch-protection rename procedure.
- [x] The reworked pipeline produces a green run against the branch head,
      satisfying `modified-workflow-needs-green-run`.
- [x] Every gate enforced by the current `quality-gates` job is still enforced
      after the split; no check is dropped, weakened, or made non-required. In
      particular, every gate command is byte-identical to its pre-split
      counterpart, the `/t:Rebuild` rationale comment and the zero-test-assembly
      `throw` guard are preserved verbatim, and the analyzer and nullable compiles
      remain separate.

## Expected Outcomes (estimates)

Comparison point: the measured 444s sequential baseline. The following are
estimates derived from the baseline, not measurements of a split pipeline:

- Wall clock ~277s if the tailored per-job setup holds (~38% reduction); worst
  case ~333s with full setup per job (~25% reduction).
- Billed `windows-latest` time rises an estimated ~1.7–2.2x, an accepted cost of
  the latency reduction and independent failure signal.

## Non-Goals

Explicitly excluded from this feature:

- **Moving the format gate to `ubuntu-latest`.** Deferred as a separate
  platform-parity follow-up; the format gate stays on `windows-latest` here.
- **Merging the analyzer and nullable compiles.** Both compiles stay separate and
  byte-identical, including `/t:Rebuild` and its in-file rationale comment.
- **Build-output artifact sharing between jobs.** Rejected by the research on
  critical-path arithmetic and fragility grounds.
- **Changing any gate's pass criterion.**
