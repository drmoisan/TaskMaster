# Tailored-Setup Fallback — Issue #553

- Timestamp: 2026-08-14T10-35 (local) / 2026-08-14T14:35:51Z (UTC)
- Task: [P3-T5]

## Result: NOT REQUIRED — tailored-setup assumption held

This is the task's explicitly authorized non-executing outcome, taken because
[P3-T4] recorded `GREEN` (branch (a)).

- Green run id: **31809697953** — <https://github.com/drmoisan/TaskMaster/actions/runs/31809697953>
- Head SHA: `0b016c81a78f3fafc0864de472f4139cc0938002`
- All five jobs concluded `success`; pipeline wall clock 259s.

EXIT_CODE: 0 (no fallback command was run; the REQUIRED branch was not entered)

## What was NOT restored, and the evidence that it was not needed

The spec's tailored per-job setup deliberately omits steps from each callee. The
green run exercised every one of those omissions on a real runner:

| Callee | Steps deliberately omitted | Green? |
| --- | --- | --- |
| `_build-analyzers.yml` | `Setup .NET SDK`, `Cache dotnet tools`, `Setup CSharpier` (`dotnet tool restore`) | yes — 186s |
| `_build-nullable.yml` | same three | yes — 188s |
| `_mstest-coverage.yml` | same three | yes — 259s |
| `_format-check.yml` | `Setup MSBuild`, `Setup NuGet`, `Cache NuGet packages`, `Restore solution` (`nuget restore`) | yes — 131s |

Both halves of the assumption are therefore confirmed by execution rather than by
inspection:

1. Nothing in the msbuild build path depends on the pinned .NET 10 SDK. The
   analyzer `/t:Build`, the nullable `/t:Rebuild`, and the MSTest job's plain
   build all completed without `actions/setup-dotnet`.
2. CSharpier reads source text only and does not consume restored NuGet packages.
   `dotnet csharpier check .` completed without `nuget restore`.

**No setup step was restored to any callee.** The workflow files are unchanged
from their [P1-T1]–[P1-T5] authored state, so the [P1-T6] byte-identity results
and the [P2-T1] structural results remain valid without re-verification.

Had the REQUIRED branch been taken, the fallback would have restored only the
specific implicated steps, copied verbatim from
`evidence/other/pre-split/ci.yml.pre-split.txt`, at an estimated ~56s/job cost
(spec Residual risk 2, research Q1 topology (a) figures). That cost was avoided.

## Measured value of the tailored setup

The spec's estimate bracketed wall clock between ~277s (tailored setup holds) and
~333s (full setup everywhere). The measured 259s is **below both estimates**,
so the tailored setup delivered at least the modelled benefit. Per-job setup cost
is visibly lower than the baseline's 130s: the format job's total 131s is roughly
the baseline's setup cost alone, because it installs only the .NET SDK and the
CSharpier tool.

## Acceptance ([P3-T5])

- Artifact exists recording exactly one branch: NOT REQUIRED.
- The pipeline is green on the current head (`0b016c81`) at task completion.
