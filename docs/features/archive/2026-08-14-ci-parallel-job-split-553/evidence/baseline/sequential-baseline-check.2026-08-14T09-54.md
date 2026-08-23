# Sequential Baseline Artifact Check — Issue #553

- Timestamp: 2026-08-14T09-54 (local) / 2026-08-14T13:54:53Z (UTC session timestamp)
- Task: [P0-T4]

Command:

```powershell
Select-String -Path 'docs/features/active/2026-08-14-ci-parallel-job-split-553/evidence/baseline/ci-sequential-baseline.2026-08-14T13-05.md' -Pattern '444s'
```

EXIT_CODE: 0

`Select-String` is a PowerShell cmdlet, not a native executable, so it does not
set `$LASTEXITCODE`. The recorded `EXIT_CODE: 0` is the exit status of the `pwsh`
process that ran the command, captured from the calling shell. The cmdlet
terminated without error and returned a non-empty match set.

## Output Summary

Four matched lines, quoted verbatim from the baseline artifact:

```
L20: | `Format, build, analyze, and test` | windows-latest | 22:25:29Z | 22:32:53Z | **7m24s (444s)** |
L23: `quality-gates` job duration: **444s**.
L51: - Sum: 130 + 302 + 12 = 444s, matching the job total.
L66: against a 444s baseline - an estimated **~25% latency reduction**.
```

MATCH_COUNT: 4

The comparison denominator for [P4-T6] is therefore confirmed present and
internally consistent within the baseline artifact:

- **444s** is the measured `Format, build, analyze, and test` job wall clock
  (L20), which equals the pipeline wall clock because the `actionlint` job already
  runs concurrently and finishes in 36s (baseline artifact L19, L22-23).
- The figure reconciles against its own step-level decomposition: 130s fixed setup
  + 302s serial gate work + ~12s teardown = 444s (L51).
- The baseline was captured from GitHub-hosted `windows-latest` run
  [31749877507](https://github.com/drmoisan/TaskMaster/actions/runs/31749877507)
  at 2026-08-14T13:05:16Z using
  `gh api repos/drmoisan/TaskMaster/actions/runs/31749877507/jobs`.

**Runner-environment parity (`.claude/rules/benchmark-baselines.md`):** the
baseline is a runner-captured measurement from a GitHub-hosted `windows-latest`
runner, not a developer workstation. [P4-T6] must capture the post-split
measurement with the same `gh api .../runs/<id>/jobs` collection method from a
GitHub-hosted run so the comparison satisfies the parity requirement. That rule's
`Unknown processor` rejection condition and sibling-provenance-file requirement
govern BenchmarkDotNet-style baselines under `scripts/benchmarks/**`; this CI
latency baseline is a workflow-run timing record whose provenance is the linked
run URL and collection command recorded above.

## Acceptance ([P0-T4])

- Artifact exists with `EXIT_CODE: 0`.
- `Output Summary:` quotes four matched `444s` lines (at least one required),
  confirming the comparison denominator for [P4-T6].
