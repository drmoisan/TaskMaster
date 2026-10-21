# P4-T5 — Nullable / type-check gate (Phase 4, loop iteration 1)

Timestamp: 2026-08-28T03-59
Task: [P4-T5]
LoopIteration: 1
Command: msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true /v:normal /fl "/flp:LogFile=docs\features\active\itemviewer-surface-defects-489\evidence\qa-gates\rem1-p4-t5-nullable-build.2026-08-28T03-59.msbuild.txt;Verbosity=normal"
EXIT_CODE: 0

## The command line, verified against both prohibitions

The command quoted above is the exact command executed. Two properties of it are load-bearing and both
were verified rather than asserted:

| Prohibition | Check | Result |
|---|---|---|
| No solution-wide nullable property | the recorded command contains no `/p:Nullable=enable`; a case-insensitive search of the build log for `Nullable=enable` returns **0** occurrences | **Satisfied** |
| `/t:Rebuild`, never `/t:Build` | the recorded command contains `/t:Rebuild`; a search of the log for the literal `/t:Build` returns **0** occurrences | **Satisfied** |

Why the nullable property must not be added: no project in this repository carries a `<Nullable>`
element and there is no `Directory.Build.props`, so `/p:Nullable=enable` would be a solution-wide
opt-in conscripting every file that has never adopted the `#nullable enable` pragma. Nullable
enforcement here is **per-file opt-in**: a file participates when it carries the pragma, and
`/p:TreatWarningsAsErrors=true` then promotes its `CS86xx` diagnostics to build errors. The command
above is character-for-character the one in `.github/workflows/ci.yml`, apart from the added `/v:normal`
and `/fl` logging switches this plan requires for the non-vacuity proof.

Why `/t:Build` must not be used locally: MSBuild's up-to-date check does not invalidate on a
command-line `/p:` change, so a warm `/t:Build` returns exit 0 having skipped `CoreCompile` on every
project — the gate could not fail. CI can use `/t:Build` because a runner checkout is always cold; a
local working tree is not.

## Result

```
Build succeeded.
    5 Warning(s)
    0 Error(s)
```

`EXIT_CODE: 0`. Zero errors, so no `CS86xx` nullable-flow diagnostic was promoted to an error in any
file that has opted into nullable analysis. A search of the log for the pattern `CS86` returns **0**
occurrences: no nullable diagnostic was emitted at all, at any severity.

The five warnings are the same pre-existing `System.Reactive` `packages.config` advisory recorded at
P0-T5 and P4-T3; they are not compiler diagnostics and are not promoted by
`TreatWarningsAsErrors`, since MSBuild task warnings of this kind are emitted outside the compiler.

### Against the baseline

| | Baseline (P0-T5) | P4-T5 |
|---|---:|---:|
| `EXIT_CODE` | 0 | **0** |
| Errors | 0 | 0 |

## Non-vacuity

| Signal | Count |
|---|---:|
| Occurrences of `Skipping target "CoreCompile"` | **0** |
| `csc.exe /noconfig` compiler invocations | 18 |

Zero skips and eighteen real compiler invocations across the 11889-line log: the compiler ran, so the
type-check gate had the opportunity to fail and did not.

## Log

`FEATURE/evidence/qa-gates/rem1-p4-t5-nullable-build.2026-08-28T03-59.msbuild.txt`, 11889 lines,
normal verbosity, `.msbuild.txt` extension. Sanitised in place, case-insensitively: worktree root to
`<repo-root>` (13631), main checkout root to `<main-checkout-root>` (36), machine name to `<host>` (0),
account name to `<user>` (0). Zero residual host tokens; only `C:\Program Files` and
`C:\Program Files (x86)` remain as absolute paths.

## Acceptance

| P4-T5 condition | Result |
|---|---|
| `EXIT_CODE: 0` | **Yes** — observed `0` |
| Zero occurrences of the skip literal in the log | **Yes** — 0 |
| The recorded command contains neither the solution-wide nullable property nor `/t:Build` | **Yes** — both absent from the command and, independently, 0 occurrences of each in the log |

Output Summary: The nullable gate **passes**. `msbuild TaskMaster.sln /t:Rebuild` with
`/p:TreatWarningsAsErrors=true` and the spaced platform spelling exited **0** with `Build succeeded.`,
`5 Warning(s)`, `0 Error(s)`, matching the P0-T5 baseline exit code of 0. No `CS86xx` diagnostic
appears anywhere in the log, so no file that has opted into nullable analysis regressed. Both
prohibitions were verified against the log as well as the command line: **0** occurrences of
`Nullable=enable` and **0** occurrences of `/t:Build`. Non-vacuity holds — **0** occurrences of
`Skipping target "CoreCompile"` and 18 `csc.exe` invocations across 11889 lines. The log is sanitised
with zero residual host tokens.
