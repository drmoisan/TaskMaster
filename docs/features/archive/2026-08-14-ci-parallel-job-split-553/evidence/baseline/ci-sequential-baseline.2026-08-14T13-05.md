# CI Sequential Baseline — Issue 553

- Captured: 2026-08-14T13:05:16Z
- Source run: https://github.com/drmoisan/TaskMaster/actions/runs/31749877507
- Run conclusion: success
- Head branch: `bug/coverage-threshold-policy-reconciliation-494`
- Workflow: `.github/workflows/ci.yml` (pre-split)
- Collection command: `gh api repos/drmoisan/TaskMaster/actions/runs/31749877507/jobs`

This is the measured sequential baseline the parallel split is compared against.
It is captured from a GitHub-hosted `windows-latest` runner, so it satisfies the
runner-environment parity requirement in `.claude/rules/benchmark-baselines.md`
for any latency comparison drawn against a later GitHub-hosted run.

## Job-level totals

| Job | Runner | Started | Completed | Duration |
| --- | --- | --- | --- | --- |
| `actionlint` | ubuntu-latest | 22:25:29Z | 22:26:05Z | 36s |
| `Format, build, analyze, and test` | windows-latest | 22:25:29Z | 22:32:53Z | **7m24s (444s)** |

The two jobs already run concurrently, so total pipeline wall clock equals the
`quality-gates` job duration: **444s**.

## Step-level breakdown of `Format, build, analyze, and test`

| # | Step | Duration | Class |
| --- | --- | --- | --- |
| 1 | Set up job | 1s | setup |
| 2 | Checkout repository | 41s | setup |
| 3 | Setup .NET SDK | 35s | setup |
| 4 | Setup MSBuild | 5s | setup |
| 5 | Setup NuGet | 0s | setup |
| 6 | Cache NuGet packages | 15s | setup |
| 7 | Restore solution | 11s | setup |
| 8 | Cache dotnet tools | 16s | setup |
| 9 | Setup CSharpier | 5s | setup |
| 10 | **Verify formatting** | **15s** | gate |
| 11 | **Build with analyzers and code style enforcement** | **101s** | gate |
| 12 | **Build with nullable warnings treated as errors** (`/t:Rebuild`) | **98s** | gate |
| 13 | **Run MSTest suite with coverage** | **88s** | gate |
| 14 | Upload test results | 2s | teardown |
| 15-19 | Post-cache / post-checkout / complete job | ~12s | teardown |

## Derived figures

- **Fixed per-job setup cost** (steps 1-9): **130s**. This cost is paid once today
  and would be paid once per parallel job after a split.
- **Gate work** (steps 10-13): **302s**, currently strictly serial.
- **Teardown** (steps 14-19): **~12s**.
- Sum: 130 + 302 + 12 = 444s, matching the job total.

## Consequences for the split design

The 130s fixed setup is the dominant constraint. A naive split into four
independent `windows-latest` jobs yields these estimated per-job durations:

| Split job | Estimated duration | Notes |
| --- | --- | --- |
| format | 130 + 15 + 12 = **157s** | needs restore + csharpier tool only |
| analyzer build | 130 + 101 + 12 = **243s** | full solution compile |
| nullable build | 130 + 98 + 12 = **240s** | full `/t:Rebuild` recompile |
| MSTest | 130 + 101 + 88 + 12 = **331s** | must build before it can discover `*.Test.dll` |

Estimated post-split wall clock is bounded by the slowest job: **~331s (5m31s)**,
against a 444s baseline — an estimated **~25% latency reduction**.

Estimated billed `windows-latest` minutes rise from ~7.4 to ~16.2 (a ~2.2x
increase before the GitHub Windows 2x cost multiplier), because the 130s setup is
paid four times instead of once.

These are estimates derived from the measured baseline, not measurements of a
split pipeline. The research stage must evaluate whether sharing build output
across jobs via `actions/upload-artifact` / `actions/download-artifact` beats
rebuilding per job, given that the upload and download of a 19-project solution's
`bin` output is itself not free.
