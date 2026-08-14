# 2026-08-14-ci-parallel-job-split — Spec

- **Issue:** #553
- **Parent (optional):** none
- **Owner:** drmoisan
- **Last Updated:** 2026-08-14
- **Status:** Ready for planning
- **Version:** 1.0

## Overview

### Problem statement

`.github/workflows/ci.yml` runs the entire C# quality toolchain inside a single
`quality-gates` job (`Format, build, analyze, and test`) on `windows-latest` with a
60-minute timeout. The job executes five expensive stages strictly sequentially:

1. `nuget restore`
2. `dotnet csharpier check .` (formatting)
3. `msbuild /t:Build` with `EnableNETAnalyzers` + `EnforceCodeStyleInBuild` (analyzers)
4. `msbuild /t:Rebuild` with `TreatWarningsAsErrors=true` (nullable/type-check)
5. `vstest.console.exe` with `/EnableCodeCoverage` (MSTest suite)

Wall-clock CI latency is the sum of all five stages. Stages 3 and 4 are both full
solution compiles (the solution contains 18 projects: 9 production, 9 test — the
earlier "19-project" figure was corrected during research), and stage 4 uses
`/t:Rebuild`, so it performs a complete recompile from scratch. A formatting
violation is not reported until after restore completes; a test failure is not
reported until after two full builds have finished.

Two additional consequences of the monolithic shape:

- **No independent failure signal.** All gates report as one required status check
  named `Format, build, analyze, and test`, so a red check does not identify which
  gate failed without opening the log.
- **No independent re-dispatch.** A transient failure in one stage requires
  re-running the entire job.

### Measured baseline

Source of record:
`docs/features/active/2026-08-14-ci-parallel-job-split-553/evidence/baseline/ci-sequential-baseline.2026-08-14T13-05.md`
(captured 2026-08-14T13:05:16Z from GitHub-hosted `windows-latest` run
[31749877507](https://github.com/drmoisan/TaskMaster/actions/runs/31749877507),
satisfying the runner-environment parity requirement of
`.claude/rules/benchmark-baselines.md`).

| Measured quantity | Value |
| --- | --- |
| `quality-gates` job wall clock (= pipeline wall clock) | **444s** (7m24s) |
| Fixed per-job setup (checkout through tool restore) | **130s** |
| Gate 1 — formatting (`dotnet csharpier check .`) | **15s** |
| Gate 2 — analyzer build (`/t:Build`) | **101s** |
| Gate 3 — nullable build (`/t:Rebuild`) | **98s** |
| Gate 4 — MSTest suite with coverage | **88s** |
| Teardown (artifact upload, post-steps) | ~12s |

## Behavior

### Adopted design

The design adopted for this feature is topology (c) from the research artifact
(`research/2026-08-14T13-30-ci-parallel-job-split-research.md`, Q1 and Q10): four
independent `windows-latest` gate jobs with per-job tailored setup, no build-output
artifact sharing, and the MSTest job performing its own plain in-workspace build.
Additionally, the `actionlint` job is extracted into a callee workflow of its own,
so `ci.yml` contains no inline `steps:` at all. These decisions are settled; the
implementation plan must not re-open them.

### Target architecture — five callee workflows

Each gate ships as a callable reusable workflow under `.github/workflows/`:

| File | Runner | Contents |
| --- | --- | --- |
| `_actionlint.yml` | `ubuntu-latest` | The existing actionlint job, moved verbatim: checkout (`fetch-depth: 1`), download actionlint 1.7.7, run `./actionlint`. |
| `_format-check.yml` | `windows-latest` | Checkout, `setup-dotnet` (10.0.x), dotnet-tools cache (`~/.nuget/packages` keyed on `dotnet-tools.json`), `dotnet tool restore`, `dotnet csharpier check .`. Deliberately omits `setup-msbuild`, `setup-nuget`, the `packages` cache, and `nuget restore` — CSharpier reads source text only and does not consume restored NuGet packages. |
| `_build-analyzers.yml` | `windows-latest` | Checkout, `setup-msbuild`, `setup-nuget`, `packages` cache (keyed on `**/packages.config`), `nuget restore`, then the analyzer gate: `msbuild /t:Build /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` with its existing `if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }` guard, byte-identical to today. |
| `_build-nullable.yml` | `windows-latest` | Same setup as `_build-analyzers.yml`, then the nullable gate: `msbuild /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true` with the same exit guard, byte-identical to today, **including the in-file rationale comment explaining why `/t:Rebuild` is used**. The comment moves with the step. |
| `_mstest-coverage.yml` | `windows-latest` | Same setup as `_build-analyzers.yml`, then a plain build (`msbuild /t:Build /m /p:Configuration=Debug "/p:Platform=Any CPU"`, no analyzer or warning-promotion properties), then the existing vstest step unchanged (vswhere discovery, recursive `*.Test.dll` discovery filtered to `\bin\Debug\` excluding `\obj\` and `\ref\`, the zero-assembly `throw` guard, `/EnableCodeCoverage /InIsolation /Logger:trx /TestCaseFilter:"TestCategory!=LiveOutlook"`), then the existing `test-results` artifact upload with `if: always()`. |

The msbuild-consuming callees (`_build-analyzers.yml`, `_build-nullable.yml`,
`_mstest-coverage.yml`) deliberately omit `setup-dotnet`, the dotnet-tools cache,
and `dotnet tool restore`. **Assumption to verify in the first green run:** nothing
in the msbuild build path depends on the pinned .NET 10 SDK. If this assumption
fails, the fallback is restoring the dropped setup steps at an estimated ~56s/job
cost (research Q1, topology (a) figures).

Each callee carries the `SOLUTION_PATH` / `BUILD_CONFIGURATION` / `BUILD_PLATFORM`
env values its steps consume.

### Target architecture — orchestrator `ci.yml`

`ci.yml` becomes a pure orchestrator:

- Retains its current `name:`, `on:` triggers (`push` to main/development,
  `pull_request` to main/development, `workflow_dispatch`), `permissions:
  contents: read`, and the existing workflow-level `concurrency` block
  (`group: ci-${{ github.workflow }}-${{ github.event.pull_request.number ||
  github.ref }}`, `cancel-in-progress: true`) unchanged.
- Contains exactly five jobs, each of the form
  `uses: ./.github/workflows/_<name>.yml`.
- Contains **no inline `steps:`** and **no `needs:` edges**. The adopted topology
  shares no files between jobs, so no artifact-consumption edge exists and the
  issue's "except where an edge is required to consume an uploaded build artifact"
  clause is vacuously satisfied with zero edges.

### Reusable-workflow contract (every callee)

Each callee workflow must satisfy all of the following:

1. Declares both `on: workflow_call:` and `on: workflow_dispatch:`, so it is
   invocable by the orchestrator and independently dispatchable for per-gate
   re-runs.
2. Declares its own `permissions: contents: read`.
3. Declares a right-sized `timeout-minutes`: actionlint ~10, format ~10, each
   build ~30, MSTest ~30 (replacing the single 60-minute monolith timeout).
4. Declares **no `concurrency` block.** The caller (`ci.yml`) owns the concurrency
   group; jobs of a called workflow run as part of the caller's run and are covered
   by the caller's group. Callee-level workflow `concurrency` under `workflow_call`
   is not clearly documented and is avoided rather than relied upon (research Q6).

### Behavior semantics

- **Success:** all five required contexts report success on the PR head. Each
  gate's pass criterion is byte-identical to the corresponding step in the
  monolithic job.
- **Failure isolation:** exactly the violated gate's context reports failure; the
  other gates run to completion independently (no `needs:` coupling), preserving
  full diagnostic signal in a single run.
- **Ordering:** none between gates. `cancel-in-progress: true` on the caller's
  group cancels all jobs of a superseded run together.
- **Edge cases:**
  - Zero discovered test assemblies in `_mstest-coverage.yml` must still `throw`
    (fail-closed protection against a discovery regression). This guard is
    preserved verbatim.
  - A `nuget restore` failure fails each of the three msbuild-consuming jobs
    independently; there is no shared restore.
  - Cache-save races between concurrent jobs sharing a cache key are benign:
    caches are immutable, creation is first-writer-wins, and a lost save emits a
    warning and never fails the job (research Q5). Under the tailored setup, the
    `~/.nuget/packages` cache is consumed only by the format job, eliminating
    concurrent saves of that key.
  - A callee dispatched standalone via `workflow_dispatch` forms its own run with
    no concurrency group. This is acceptable for the manual dispatch path.

## Inputs / Outputs

- **Inputs:** none new. Workflow triggers are unchanged. No workflow-level
  `inputs:` are defined on the callees at this stage.
- **Outputs:** the `test-results` artifact (name unchanged) containing
  `TestResults/**/*.trx` and `TestResults/**/*.coverage` (paths unchanged),
  uploaded with `if: always()` and `if-no-files-found: warn`, now produced by
  `_mstest-coverage.yml`. This upload targets workflow storage; it is not
  cross-job file sharing.
- **Config keys and defaults:** env values `SOLUTION_PATH: TaskMaster.sln`,
  `BUILD_CONFIGURATION: Debug`, `BUILD_PLATFORM: Any CPU` replicate into each
  callee that consumes them.
- **Backward-compatibility constraint:** the `main` ruleset's required-status-check
  contexts must be migrated in lockstep with the merge (see below); the old context
  `Format, build, analyze, and test` ceases to exist after the split.

## Required-Status-Check Contract

The `main` ruleset (id `18572843`, `strict_required_status_checks_policy: true`)
currently requires exactly two contexts: `actionlint` and
`Format, build, analyze, and test`. After the split, the pipeline reports five
contexts, and all five become required.

Contract terms:

1. **Context-name form.** For jobs of a called reusable workflow, the check-run
   context name takes the form `<caller job name> / <callee job name>`. Because
   the `actionlint` job moves into a callee, its context name also changes and
   must be included in the new required set.
2. **Names are captured, never assumed.** The exact context strings must be read
   from a live green run on the PR head
   (`gh api repos/drmoisan/TaskMaster/commits/<head-sha>/check-runs --jq
   '.check_runs[].name'`) before any ruleset mutation. Assuming the strings is the
   single most likely source of a botched migration (research Q8).
3. **Single atomic PUT.** The ruleset update is one
   `PUT /repos/drmoisan/TaskMaster/rulesets/18572843` carrying the **full writable
   ruleset object** (name, target, enforcement, bypass_actors, conditions, rules)
   with the complete new contexts set replacing the old set in the same request.
   Read-only fields returned by GET (`id`, `node_id`, `created_at`, `updated_at`,
   `_links`, `source`, `source_type`, `current_user_can_bypass`) are not part of
   the payload. A two-step remove-then-add edit is prohibited: it opens an
   under-gating window.
4. **Migration sequence (research Q8):** green run on the split PR → capture exact
   check-run names from the live head SHA → one atomic PUT → merge the split PR
   immediately (updating the branch first if `strict` requires it) → verify by GET
   that the ruleset holds exactly the five intended contexts and record the
   pre-PUT ruleset JSON, the PUT payload, and the post-PUT GET response as
   evidence under this feature folder's `evidence/` tree.
5. **Fail-closed property.** A required context that never reports blocks merging.
   Both orderings of PUT-vs-merge therefore over-block rather than under-gate; the
   only under-gating hazard is an incomplete contexts set in the PUT, which the
   atomicity requirement addresses.
6. **Rollback:** a single PUT restoring the previous contexts set reverts the merge
   policy; reverting the workflow change is an ordinary revert PR.

## Data & State

- No application data or state changes. The change is confined to workflow files,
  `.github/workflows/README.md` (new), and the `main` ruleset's required contexts.
- Cache state: the `packages` and `~/.nuget/packages` cache keys are unchanged;
  consumption is redistributed per job as described above.

## Invariants (must not regress)

1. **Every gate command byte-identical.** The csharpier command, both msbuild gate
   invocations (including `/t:Rebuild` and all properties), and the vstest
   invocation are moved, not edited.
2. **The `/t:Rebuild` rationale comment** in the nullable gate moves with the step
   and is preserved verbatim.
3. **The zero-test-assembly `throw` guard** in the vstest step is preserved
   verbatim.
4. **The `test-results` artifact upload** is preserved with `if: always()`, the
   same artifact name, and the same paths.
5. **The `$LASTEXITCODE` exit guards** on both msbuild gates are retained verbatim.
   No step in the pipeline uses the deliberately-failing-nested-command pattern of
   `.claude/rules/ci-workflows.md`, so no `exit 0` / reset additions are required
   (research Q9); any future negative-path self-validation step must comply with
   that rule.
6. **No gate is dropped, weakened, or made non-required.** All five contexts are
   required after the ruleset PUT; the analyzer and nullable gates keep their
   current enforcement semantics (no warning-promotion changes, no carve-outs).
7. **The workflow-level concurrency group** in `ci.yml` (group expression and
   `cancel-in-progress: true`) is unchanged.
8. **The vstest discovery filter discipline** (match `\bin\<config>\`, exclude
   `\obj\` and `\ref\`) is not weakened; the same script text is the reference for
   local runs.

## Non-Goals

Explicitly out of scope for this feature:

1. **Moving the format gate to `ubuntu-latest`.** CSharpier is cross-platform and
   Linux minutes bill at 1x, but the move introduces a platform-parity question
   (line endings, tool behavior) that is deliberately deferred as a separate
   follow-up concern. The format gate stays on `windows-latest` in this change.
2. **Merging the analyzer and nullable compiles.** A merged compile would either
   weaken the nullable gate (via carve-outs) or strengthen the analyzer gate
   without ratification, and it would collapse the independent failure signal
   (research Q2). Both gate commands stay separate and byte-identical.
3. **Build-output artifact sharing between jobs.** Rejected on critical-path
   arithmetic (own-build wins even at zero transfer cost) and fragility (`.pdb`
   coverage degradation, dependency-closure gaps, `upload-artifact@v4` path
   restructuring) (research Q1b, Q3).
4. **Changing any gate's pass criterion**, timeout semantics excepted (the
   monolith's single 60-minute timeout is replaced by right-sized per-callee
   timeouts, which is a scheduling bound, not a pass criterion).
5. **Any C# source, project, or test change.** This feature touches workflow YAML,
   the workflows README, and the ruleset only.

## Expected Outcomes (estimates)

All figures in this section are **estimates** derived arithmetically from the
measured 444s baseline; they are not measurements of a split pipeline and must be
verified against a measured post-split run.

| Metric | Baseline (measured) | Target (estimate) | Worst case (estimate) |
| --- | --- | --- | --- |
| Wall clock | 444s (7m24s) | ~277s (4m37s), bounded by the MSTest job, if the tailored setup holds (~38% reduction) | ~333s (5m33s) with full setup per job (~25% reduction) |
| Billed `windows-latest` seconds | ~444s | ~763s (~1.7x) | ~962s (~2.2x) |

The billed-minutes increase (~1.7–2.2x, before GitHub's Windows 2x billing
multiplier) is an accepted cost of the latency reduction and the independent
failure signal. Per-run job demand is 5 concurrent jobs (4 × `windows-latest` +
1 × `ubuntu-latest`), below every GitHub plan's concurrency ceiling; realized
speedup can be eroded only by cross-run account-level contention, which is not
determinable from repository data (research Q7).

## Constraints & Risks

### Constraints

- **Branch-protection coupling (blocking risk).** Until the ruleset PUT lands,
  every PR blocks on the old context, which after the split can never report. The
  Required-Status-Check Contract section above is the mitigation; the PUT is an
  outward-facing merge-policy change and must follow the recorded sequence.
- **Windows runner setup cost is paid per job.** Mitigated by the tailored per-job
  setup; the fallback (full setup everywhere) still improves on the baseline.
- **Concurrency group.** Owned solely by the caller; callees declare none.
- **`modified-workflow-needs-green-run`.** The reworked pipeline must produce a
  green run against the branch head before merge.

### Residual risks (carried from research Q10)

1. **Context-name mismatch in the ruleset PUT** — highest-likelihood failure.
   Mitigated by capturing names from the live run before the PUT and by
   fail-closed blocking if a name is wrong.
2. **Tailored-setup assumption** (msbuild jobs without `setup-dotnet`; format job
   without `nuget restore`) is unverified until the first green run. Fallback:
   restore the dropped steps at ~56s/job estimated cost.
3. **Estimated timings are estimates**; hosted-runner variance (checkout and setup
   steps in particular) can shift per-job durations by tens of seconds.
4. **Cross-run account-level runner contention** could queue jobs and erode the
   realized speedup; not determinable from repository data.
5. **Undocumented callee-level concurrency semantics** are avoided rather than
   relied upon.

## Implementation Strategy

- **Scope of change:** five new callee workflow files (`_actionlint.yml`,
  `_format-check.yml`, `_build-analyzers.yml`, `_build-nullable.yml`,
  `_mstest-coverage.yml`); `ci.yml` rewritten as the orchestrator; new
  `.github/workflows/README.md` documenting the per-stage `workflow_dispatch`
  procedure and the branch-protection rename procedure; one atomic ruleset PUT.
- **No new dependencies.** All actions used (`actions/checkout@v4`,
  `actions/setup-dotnet@v4`, `microsoft/setup-msbuild@v2`, `nuget/setup-nuget@v2`,
  `actions/cache@v4`, `actions/upload-artifact@v4`) are already in use.
- **Rollout:** the split PR's own run exercises the new pipeline (PR head-ref
  workflow files execute for `pull_request` events). Migration and rollback follow
  the Required-Status-Check Contract section.
- **Post-merge verification:** standalone `workflow_dispatch` smoke of each callee;
  post-split timing evidence captured with the same `gh api .../runs/<id>/jobs`
  method as the baseline (runner-environment parity per
  `.claude/rules/benchmark-baselines.md`), recorded under this feature folder's
  `evidence/` tree.

## Acceptance Criteria

- [x] The formatting gate, the analyzer build gate, the nullable build gate, and
      the MSTest gate each run as separate GitHub Actions jobs with **zero**
      `needs:` edges (no build-output artifact sharing exists, so no
      artifact-consumption edge is justified).
- [x] Five callee reusable workflows exist — `_actionlint.yml`,
      `_format-check.yml`, `_build-analyzers.yml`, `_build-nullable.yml`,
      `_mstest-coverage.yml` — each declaring both `on: workflow_call:` and
      `on: workflow_dispatch:`, its own `permissions:`, a right-sized
      `timeout-minutes`, and no `concurrency` block.
- [x] `ci.yml` is an orchestrator containing only `uses:` job references and no
      inline `steps:` (the `actionlint` extraction resolves the criterion as
      originally drafted in `issue.md`).
- [x] No file is shared between jobs; the only artifact operation is the preserved
      `test-results` upload (workflow storage, `if: always()`, same name and
      paths).
- [x] The four gate commands and the actionlint step are byte-identical to their
      pre-split counterparts, including the `/t:Rebuild` rationale comment, the
      `$LASTEXITCODE` guards, and the zero-test-assembly `throw` guard.
- [x] The `main` ruleset's `required_status_checks` contexts are replaced in one
      atomic PUT with the five context strings captured from a live green run on
      the PR head, with no window in which a merge can bypass a gate, and the
      pre-PUT JSON, PUT payload, and post-PUT GET response are recorded as
      evidence.
- [x] `.github/workflows/README.md` documents the per-stage `workflow_dispatch`
      procedure and the branch-protection rename procedure.
- [x] The reworked pipeline produces a green run against the branch head,
      satisfying `modified-workflow-needs-green-run`.
- [x] Every gate enforced by the current `quality-gates` job is still enforced
      after the split; no check is dropped, weakened, or made non-required.
- [x] Post-split wall-clock duration is measured with the same collection method
      as the baseline and recorded as evidence in this feature folder, compared
      against the measured 444s baseline.

## Definition of Done

- [ ] Acceptance criteria above delivered and individually verified.
- [ ] Seeded test conditions below exercised and their outcomes recorded.
- [ ] `.github/workflows/README.md` created and linked from the feature folder.
- [ ] Evidence (ruleset before/after, green-run reference, post-split timings)
      committed under
      `docs/features/active/2026-08-14-ci-parallel-job-split-553/evidence/`.
- [ ] No C# toolchain pass required unless the implementation unexpectedly touches
      `*.cs` / `*.csproj` files (none are planned).

## Seeded Test Conditions (from potential)

- [x] `actionlint` passes against every new and modified workflow file.
- [ ] Each `_<name>.yml` is independently dispatchable via `workflow_dispatch` and
      succeeds standalone.
- [x] A deliberate formatting violation fails only the formatting gate and reports
      a distinct red check. (Exercised as a temporary probe commit on the PR
      branch, then reverted — not as a permanent workflow step, so no
      deliberately-failing nested command enters the committed pipeline.)
- [x] A deliberate nullable violation fails only the nullable gate. (Same
      probe-commit method.)
- [x] A deliberate test failure fails only the MSTest gate. (Same probe-commit
      method.)
- [x] Test results and coverage artifacts continue to upload with the same names.
- [x] Total wall-clock duration of the reworked pipeline is measured against the
      current sequential baseline and recorded as evidence.
- [x] No `pwsh` step leaks a residual non-zero `$LASTEXITCODE` per
      `.claude/rules/ci-workflows.md`.
