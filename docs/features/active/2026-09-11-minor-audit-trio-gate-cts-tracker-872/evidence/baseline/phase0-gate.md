# Phase 0 — Halt Gate Evaluation

Timestamp: 2026-09-13T05-15
Task: [P0-T14]

PHASE0_GATE: RED

## Gate Table

The gate tabulates the recorded `EXIT_CODE:` of the six command-bearing baseline tasks and states, per row,
whether it is zero.

| Task | Gate | Recorded EXIT_CODE | Is zero |
| --- | --- | --- | --- |
| P0-T5 | CSharpier check, read-only | 0 | yes |
| P0-T6 | Analyzer rebuild | 0 | yes |
| P0-T7 | Nullable rebuild | 0 | yes |
| P0-T8 | vstest, UtilitiesCS test assembly | 0 | yes |
| P0-T9 | vstest, QuickFiler test assembly | 0 | yes |
| P0-T10 | Repository-wide coverage runner | 1 | NO |

Five of the six rows are zero. One row is non-zero.

## Failing Row

**P0-T10 — repository-wide coverage baseline. Observed `EXIT_CODE: 1`.**

The repository coverage runner ran 7221 tests, of which 7218 passed and 3 failed, printed
`Test Run Failed.`, and then threw from its own line 236 with the message
`MSTest with coverage failed with exit code 1`. The three failing tests all belong to
`QuickFiler.Controllers.Tests.QfcInitEmailQueueZeroBatchTests`:

```
Failed InitEmailQueue_ZeroBatchSize_ReturnsEmptyListWithoutThrowing
Failed InitEmailQueue_ZeroBatchSize_StillStartsBackgroundWorker
Failed InitEmailQueue_PositiveBatchSize_RetainsExistingProjectionAndFrameDrop
```

All three fail with a `TypeInitializationException` for `Deedle.Reflection`, whose innermost cause is a
`FileNotFoundException` for `netstandard, Version=2.1.0.0`.

Because the runner threw before its post-processing step, it printed neither its `First-party coverage: `
line nor its terminating `Done. Coverage artifact: ` line, so P0-T10's requirement that the two figure sets
reconcile could not be satisfied either. Both halves of that task's acceptance therefore fail.

No other row failed, and no gate other than P0-T10 is implicated.

## Classification Of The Failure

This is a pre-existing, already-diagnosed tooling defect in the repository's coverage runner. It is not a
regression introduced by this delivery, and no Phase 1 work has begun. The mechanism is recorded in the
plan's decision D14: the runner appends the MSTest runsettings file at its line 76 and resolves that path
internally, exposing no override parameter; that file's entire content is an MSTest Parallelize block with
ClassLevel scope and a worker count of zero; under that class-level parallelism one race during concurrent
class initialization poisons the Deedle reflection type, and the CLR caches a failed static initializer for
the process lifetime, so repeated runs reproduce the failure and it reads as deterministic.

The corroborating control is P0-T9, which ran the identical QuickFiler test assembly without that
runsettings file minutes earlier and passed 1394 of 1394 at exit 0. A no-coverage control on the same
process would not be a control, because of the cached-initializer behaviour; a separate process without the
runsettings file is.

## Why This Gate Halts Rather Than Proceeding

An admitted red baseline would make every Phase 2 exit-zero demand unmeetable by any work this plan
performs. In particular, the Phase 2 coverage task invokes the same runner and cannot avoid the same
switch, so a Phase 2 run would fail identically and the failure would surface as a false Phase 2 regression
attributed to this delivery. The divergence must be resolved by the caller before implementation starts.

Phase 1 has NOT begun. No Write Set path has been created, modified or deleted.

## What Was Deliberately Not Done

- `scripts/vscode/Invoke-MSTestWithCoverage.ps1` was not edited; it is outside the Write Set.
- `scripts/vscode/TaskMaster.cli.runsettings` was not edited; it is outside the Write Set.
- P0-T10 was not dropped, narrowed, or substituted with a run over a narrower population.
- The defect was not re-diagnosed.
- No file named `coverage.xml` was created under any artifacts path, so no repository coverage floor was
  activated by this run.

## Environment Bootstrap Performed Within Phase 0

Three environment actions were mechanically necessary before the gates could produce a measurement at all.
Each is recorded in full in its own task artifact, each edits no tracked file, and none is a plan
deviation:

- The repo-local .NET SDK 8.0.205 was installed into the worktree-local git-ignored SDK directory, because
  `dotnet` could not resolve at all in this fresh worktree. Recorded in the P0-T3 artifact.
- NuGet packages were restored for the packages.config projects, installing 172 packages. This is P0-T4
  itself.
- Meziantou.Analyzer 3.0.203 was provisioned into the git-ignored packages directory, because 15 of the 16
  first-party projects carry an `<Analyzer Include>` HintPath naming 3.0.203 while packages.config resolves
  3.0.235. The skew is pre-existing: the main branch carries it identically and this branch modified no
  project file. Recorded in the P0-T6 artifact.

After those three actions, `git status --porcelain --untracked-files=all -- "*.csproj" "*.config" "*.props" "*.targets"`
produced no output.

## Verdict

PHASE0_GATE: RED. The executor halts at P0-T14 and reports BLOCKED to the caller, naming P0-T10 as the
failing row, without beginning Phase 1.
