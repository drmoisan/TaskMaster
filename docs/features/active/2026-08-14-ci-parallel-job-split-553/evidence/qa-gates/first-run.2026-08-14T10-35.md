# First Run of the Split Pipeline — Issue #553

- Timestamp: 2026-08-14T10-35 (local) / 2026-08-14T14:35:51Z (UTC)
- Task: [P3-T4]
- Run: [31809697953](https://github.com/drmoisan/TaskMaster/actions/runs/31809697953)
- Head SHA: `0b016c81a78f3fafc0864de472f4139cc0938002`
- Branch: `feature/ci-parallel-job-split-553`
- Trigger event: `workflow_dispatch`

Commands:

```powershell
gh api repos/drmoisan/TaskMaster/actions/runs/31809697953 --jq '{id,event,head_sha,head_branch,status,conclusion,created_at,updated_at,html_url}'
gh api repos/drmoisan/TaskMaster/actions/runs/31809697953/jobs --jq '.jobs[] | {name, conclusion, started_at, completed_at}'
```

EXIT_CODE: 0

## Trigger note (why `workflow_dispatch` rather than `push`)

`ci.yml` fires on `push` to `[main, development]` and on `pull_request` against
those branches. A push to a feature branch therefore starts no run, and
[P3-T3] (pull-request creation) is orchestrator-gated and still deferred. The run
was started explicitly:

```
gh workflow run ci.yml --ref feature/ci-parallel-job-split-553
```

This is the same dispatch form used for every subsequent run in Phase 4. The
`modified-workflow-needs-green-run` rule is satisfied by a green run against the
branch head regardless of trigger event, and remediation finding B1
(`remediation-inputs.2026-08-14T10-21.md`) explicitly accepts a green
`workflow_dispatch` run against the branch head when the pull-request path is
blocked.

## Output Summary — GREEN, branch (a)

Run conclusion: **success**. All five jobs concluded `success`.

| Job (check-run context) | Conclusion | Started | Completed | Duration |
| --- | --- | --- | --- | --- |
| `actionlint / actionlint` | success | 14:28:53Z | 14:29:29Z | 36s |
| `format-check / Verify formatting` | success | 14:28:53Z | 14:31:04Z | 131s |
| `build-analyzers / Build with analyzers and code style enforcement` | success | 14:28:53Z | 14:31:59Z | 186s |
| `build-nullable / Build with nullable warnings treated as errors` | success | 14:28:55Z | 14:32:03Z | 188s |
| `mstest-coverage / Run MSTest suite with coverage` | success | 14:28:53Z | 14:33:12Z | 259s |

- **Pipeline wall clock: 259s** — latest `completed_at` (14:33:12Z) minus earliest
  `started_at` (14:28:53Z).
- **All five jobs started within 2 seconds of each other** (14:28:53Z–14:28:55Z),
  confirming they were scheduled concurrently and that the zero-`needs:` topology
  produces genuine parallelism rather than incidental interleaving.
- Wall clock is bounded by the MSTest job, as the spec predicted.

## Branch outcome (explicit, per the task's three-way branching)

**(a) All five jobs succeeded → GREEN.** [P3-T5] therefore takes its
NOT-REQUIRED branch. Neither branch (b) (a trimmed-setup symptom) nor branch (c)
(a failure from any other cause) occurred.

## What this run independently confirms

1. **The tailored-setup assumption holds** (spec Residual risk 2). The three
   msbuild callees ran green with **no** `setup-dotnet`, no dotnet-tools cache,
   and no `dotnet tool restore`; the format callee ran green with **no**
   `setup-msbuild`, no `setup-nuget`, no `packages` cache, and no
   `nuget restore`. Nothing in the msbuild build path required the pinned .NET 10
   SDK, and CSharpier required no restored NuGet packages.
2. **The reusable-workflow wiring executes**, not merely lints. All five
   `uses: ./.github/workflows/_<name>.yml` references resolved and each callee's
   `workflow_call` trigger accepted the invocation.
3. **The check-run context name form is now measured, not assumed.** It is
   `<caller job id> / <callee job name>` — for example the caller job `actionlint`
   invoking the callee job whose `name:` is `actionlint` yields
   `actionlint / actionlint`, and the caller job `format-check` invoking the
   callee job named `Verify formatting` yields `format-check / Verify formatting`.
   **The previously required bare `actionlint` context no longer reports**, so
   both of the ruleset's current required contexts must be replaced, not just the
   `Format, build, analyze, and test` one. These five strings are recorded here
   for reference only; [P5-T16] captures the authoritative list from the final
   head SHA, which will have moved by then.
4. **The MSTest job's own plain build works.** It discovered and ran test
   assemblies without inheriting build output from any other job, which is the
   premise of the no-artifact-sharing topology.

Runner environment (from `gh api .../jobs --jq '.jobs[] | {runner_name, labels}'`):
all four gate jobs on `windows-latest`, the actionlint job on `ubuntu-latest`,
runner group `GitHub Actions` (GitHub-hosted). This matches the baseline's runner
class and satisfies the parity requirement of
`.claude/rules/benchmark-baselines.md` for the [P4-T6] comparison.

## Acceptance ([P3-T4])

- Artifact exists and records branch outcome (a) explicitly, with run id, head
  SHA, and per-job names and conclusions.
