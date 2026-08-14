# ci-parallel-job-split (Issue #553)

- Date captured: 2026-08-14
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/ci-parallel-job-split/ (Issue #553)

- Issue: #553
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/553
- Last Updated: 2026-08-14
- Work Mode: full-feature

## Problem / Why

`.github/workflows/ci.yml` runs the entire C# quality toolchain inside a single
`quality-gates` job on `windows-latest` with a 60-minute timeout. That job executes
five expensive stages strictly sequentially:

1. `nuget restore`
2. `dotnet csharpier check .` (formatting)
3. `msbuild /t:Build` with `EnableNETAnalyzers` + `EnforceCodeStyleInBuild` (analyzers)
4. `msbuild /t:Rebuild` with `TreatWarningsAsErrors=true` (nullable/type-check)
5. `vstest.console.exe` with `/EnableCodeCoverage` (MSTest suite)

Wall-clock CI latency is therefore the sum of all five stages. Stages 3 and 4 are
both full solution compiles of a 19-project solution, and stage 4 uses `/t:Rebuild`,
so it performs a complete recompile from scratch. A formatting violation is not
reported until after restore completes, and a test failure is not reported until
after two full builds have finished.

Two additional consequences of the monolithic shape:

- **No independent failure signal.** All five gates report as one required status
  check named `Format, build, analyze, and test`, so a red check does not identify
  which gate failed without opening the log.
- **No independent re-dispatch.** A transient failure in one stage requires
  re-running the entire 60-minute job.

## Proposed Behavior

Decompose the monolithic `quality-gates` job into independent gates that GitHub
Actions schedules concurrently, so wall-clock CI latency approaches the duration of
the slowest single gate rather than the sum of all gates.

Per `.claude/skills/orchestrate/SKILL.md` (`## GitHub Actions Reusable Workflows`),
each gate ships as a callable reusable workflow named `_<name>.yml` declaring both
`on: workflow_call:` and `on: workflow_dispatch:`, and the orchestrator workflow
references them via `uses: ./.github/workflows/_<name>.yml` with no inline `steps:`
of its own. Any file that must cross a job boundary uses explicit
`actions/upload-artifact` + `actions/download-artifact`; cross-job filesystem
reliance is not implicit.

## Acceptance Criteria (early draft)

- [x] The formatting gate, the analyzer build gate, the nullable build gate, and the
      MSTest gate each run as separate GitHub Actions jobs with no `needs:` edge
      forcing them to serialize, except where an edge is required to consume an
      uploaded build artifact.
- [x] Each gate is a callable reusable workflow `_<name>.yml` declaring both
      `on: workflow_call:` and `on: workflow_dispatch:`.
- [x] `ci.yml` becomes an orchestrator workflow containing only `uses:` references
      and contains no inline `steps:`.
- [x] Any file shared between jobs crosses the boundary via explicit
      `actions/upload-artifact` + `actions/download-artifact`.
- [ ] The `main` branch ruleset's `required_status_checks` contexts are updated to
      match the new job names, with no window in which a merge can bypass a gate.
- [x] `.github/workflows/README.md` documents the per-stage `workflow_dispatch`
      procedure and the branch-protection rename procedure.
- [x] The reworked pipeline produces a green run against the branch head, satisfying
      `modified-workflow-needs-green-run`.
- [ ] Every gate enforced by the current `quality-gates` job is still enforced after
      the split; no check is dropped, weakened, or made non-required.

## Constraints & Risks

- **Branch-protection coupling (blocking risk).** The `main` ruleset (id `18572843`,
  `strict_required_status_checks_policy: true`) requires exactly two contexts:
  `actionlint` and `Format, build, analyze, and test`. Splitting the job removes the
  second context. Until the ruleset is updated, every PR will block on a check that
  can never report. The ruleset update is automatable via
  `gh api --method PUT repos/drmoisan/TaskMaster/rulesets/18572843` (the session token
  holds `repo` scope and repository `admin: true`), but it is an outward-facing change
  to the repository's merge policy and must be applied deliberately, in the correct
  order relative to the merge of this change.
- **Windows runner setup cost is paid per job.** Each parallel job repeats checkout,
  `setup-dotnet`, `setup-msbuild`, `setup-nuget`, and `nuget restore`. If the split is
  naive, the added fixed cost per job can offset the parallelism gain. Caching
  (`actions/cache` on `packages` and `~/.nuget/packages`) and a decision about whether
  to share build output via artifact upload rather than rebuilding are both load-bearing.
- **Two full compiles are inherent to the current design.** The analyzer gate uses
  `/t:Build` and the nullable gate uses `/t:Rebuild`; the `/t:Rebuild` choice is
  deliberate and documented in-file (MSBuild's incremental up-to-date check does not
  invalidate on a command-line property change alone). Whether these two compiles can
  share output, must stay separate, or should run concurrently on separate runners is
  the central design question.
- **Test gate needs built assemblies.** `vstest.console.exe` discovers `*.Test.dll`
  under `bin/$BUILD_CONFIGURATION`. Running the test gate in a separate job requires
  either its own build or a downloaded build artifact.
- **Local vstest discovery hazard.** Recursive `*.Test.dll` discovery can pick up
  stale agent-worktree builds; the CI-side discovery filter must not regress.
- **Concurrency group.** `cancel-in-progress: true` is set at the workflow level and
  must continue to behave correctly across the reusable-workflow boundary.
- **Actions concurrency limits.** Windows runner concurrency on the account may cap
  how many `windows-latest` jobs actually run at once, bounding the realized speedup.

## Test Conditions to Consider

- [ ] `actionlint` passes against every new and modified workflow file.
- [ ] Each `_<name>.yml` is independently dispatchable via `workflow_dispatch` and
      succeeds standalone.
- [ ] A deliberate formatting violation fails only the formatting gate and reports a
      distinct red check.
- [ ] A deliberate nullable violation fails only the nullable gate.
- [ ] A deliberate test failure fails only the MSTest gate.
- [ ] Test results and coverage artifacts continue to upload with the same names.
- [ ] Total wall-clock duration of the reworked pipeline is measured against the
      current sequential baseline and recorded as evidence.
- [ ] No `pwsh` step leaks a residual non-zero `$LASTEXITCODE` per
      `.claude/rules/ci-workflows.md`.

## Next Step

- [ ] Promote to GitHub issue (feature request template)
- [ ] Create `docs/features/active/ci-parallel-job-split/` folder from the template

## References

- Workflows README: `.github/workflows/README.md` (created by #553) — documents
  the pipeline topology, the per-stage `workflow_dispatch` procedure, and the
  branch-protection rename procedure.
