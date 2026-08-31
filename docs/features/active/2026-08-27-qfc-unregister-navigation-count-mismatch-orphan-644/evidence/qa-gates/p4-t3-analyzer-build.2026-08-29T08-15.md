# QA gate — Analyzer gate ([P4-T3])

- Issue: #644
- Task: `[P4-T3]`
- Timestamp: 2026-08-29T08-15

**Restarted pass.** This artifact records the re-run triggered by the `[P4-T8]` net-line finding
described in `evidence/qa-gates/p4-t1-csharpier-format.2026-08-29T08-15.md`.

Command: `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
Working directory: repository root (`<repo-root>`)
Shell: PowerShell (`pwsh -NoProfile`)
EXIT_CODE: 0

## msbuild final summary block

```
    5 Warning(s)
    0 Error(s)

Time Elapsed 00:00:15.55
```

## Acceptance

| Clause | Required | Observed | Verdict |
|---|---|---|---|
| Exit code | 0 | **0** | PASS |
| Summary errors | 0 | **0** | PASS |
| Warning count at or below `[P0-T9]` | <= 5 | **5** | PASS |

All three clauses hold. The step did not fail and rewrote no file, so the loop does **not** restart
from `[P4-T1]`.

## `/t:Rebuild` actually ran the analyzers

`/t:Rebuild` was used, not `/t:Build`, because a warm `/t:Build` skips `CoreCompile` and runs no
analyzers at all. Verified rather than assumed:

```
T3_csc=36
```

36 `csc.exe` invocations, matching the `[P0-T9]` baseline, so every project recompiled and every
analyzer ran against the final source state including the condensed comments.

## Warning composition — no new analyzer diagnostic

Command: pattern search for `warning (CS|CA|IDE|MA|RCS)[0-9]+` over the captured log.

```
T3_diags=0
```

**Zero** compiler, .NET-analyzer, code-style, Meziantou, or Roslynator diagnostics. All five
warnings are the pre-existing `System.Reactive` `packages.config` advisory emitted by
`packages\System.Reactive.7.0.0\build\System.Reactive.PackagesConfigCheck.targets`, which carries
no diagnostic identifier and is not attributable to this change.

| Measure | `[P0-T9]` baseline | `[P4-T3]` final |
|---|---|---|
| Exit code | 0 | 0 |
| Errors | 0 | 0 |
| Warnings | 5 | 5 |
| `CS`/`CA`/`IDE`/`MA`/`RCS` diagnostics | 0 | 0 |
| `csc.exe` invocations | 36 | 36 |

The analyzer surface is unchanged by this fix, which is what AC-15's "no new analyzer diagnostic"
clause requires.

Output Summary: Analyzer gate **green**. EXIT_CODE 0, **0 errors, 5 warnings** — at the `[P0-T9]`
baseline of 5 and therefore satisfying the at-or-below clause — with **zero** `CS`/`CA`/`IDE`/`MA`/`RCS`
diagnostics anywhere in the output. 36 `csc.exe` invocations confirm `/t:Rebuild` recompiled every
project. This artifact also supports AC-8 (`[P5-T9]`).
