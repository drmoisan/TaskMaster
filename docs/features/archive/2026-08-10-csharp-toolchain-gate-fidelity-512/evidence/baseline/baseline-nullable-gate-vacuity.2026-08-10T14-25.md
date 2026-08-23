# Baseline — nullable type-check gate vacuity and divergence (#512, #492, #522)

Timestamp: 2026-08-10T14-25
Branch: bug/csharp-toolchain-gate-fidelity-512 (from origin/epic/build-ci-coverage-gate-fidelity-integration @ edf3d34c)
Worktree: C:\Users\DanMoisan\repos\TaskMaster\.claude\worktrees\agent-af19fe9c37ece6a65
MSBuild: C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\amd64\MSBuild.exe
Bootstrap: `scripts/vscode/Install-RepoDotNetSdk.ps1` then `scripts/vscode/Invoke-Restore.ps1` (NuGet restore), both EXIT_CODE 0.

All four runs added `/nologo /v:m /fl "/flp:logfile=<path>;verbosity=normal"` purely to capture a file
log for the compile-execution assertion. Those switches do not alter build semantics.

## M1 — documented analyzer step (CLAUDE.md step 2), establishes warm outputs

Command: `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
EXIT_CODE: 0
Elapsed: 25.8 s
Output Summary: Cold build succeeds. 0 errors, 2 warnings. Leaves all project outputs current, which
is the precondition that makes the following step vacuous.

## M2 — documented type-check step (CLAUDE.md step 3), run immediately after M1

Command: `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug "/p:Platform=Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true`
EXIT_CODE: 0
Elapsed: 1.8 s
Output Summary: **The gate passed without compiling anything.** The MSBuild file log contains 18
occurrences of:

```
Skipping target "CoreCompile" because all output files are up-to-date with respect to the input files.
```

That is one skip for every project in the solution. Zero `CoreCompile` executions, zero errors, zero
warnings, 1.8 s wall time. `/p:Nullable=enable` and `/p:TreatWarningsAsErrors=true` never reached the
compiler. This is issue #512 reproduced exactly: MSBuild's incremental up-to-date check does not
invalidate on a command-line `/p:` change alone.

## M3 — CI's actual command

Command: `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true`
EXIT_CODE: 0
Elapsed: 20.0 s
Output Summary: **Genuine full recompile that passes.** 0 occurrences of `Skipping target
"CoreCompile"`; 74 `CoreCompile` target executions. 0 errors, 2 warnings. This is the command at
`.github/workflows/ci.yml` step "Build with nullable warnings treated as errors". It both compiles and
passes on this branch, so it is a gate that can fail and does not manufacture false findings.

## M4 — forced rebuild retaining the documented `/p:Nullable=enable`

Command: `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true`
EXIT_CODE: 1
Elapsed: 4.3 s
Output Summary: **195 errors, all in `UtilitiesCS.csproj`, 0 warnings.** MSBuild's own summary block
reports `195 Error(s)`. Breakdown by diagnostic:

| Diagnostic | Count |
|---|---|
| CS8766 | 130 |
| CS8618 | 23 |
| CS8625 | 12 |
| CS8600 | 9 |
| CS8601 | 8 |
| CS8604 | 7 |
| CS8602 | 3 |
| CS8603 | 2 |
| CS8714 | 1 |
| **Total** | **195** |

This reproduces the breakdown recorded in issue #492 exactly, diagnostic for diagnostic, on
2026-08-10.

### Counting caveat (important for anyone re-measuring)

A naive `Select-String 'error CS'` over the file log returns **390**, exactly twice the true figure,
because the normal-verbosity file logger prints each error once inline (prefixed with a node id such
as `19>`) and once again in the terminal error summary block. 195 lines carry the node prefix and 195
do not; deduplicating the node-prefixed set by message body yields 195. Trust MSBuild's own
`N Error(s)` summary line, or count only node-prefixed lines. Divergent figures in the historical
record (195, 220, ~414) are plausibly explained by this and by differing termination points.

### The 195 figure is a lower bound, not a solution-wide total

M4 terminated after 4.3 s having executed only 16 `CoreCompile` targets, against 74 for the successful
M3 rebuild. `UtilitiesCS` is a foundational dependency; once it failed, its dependents were never
compiled and their nullable diagnostics were never counted. The solution-wide figure under
`/p:Nullable=enable` is therefore greater than or equal to 195 and remains unmeasured. This is
consistent with issue #507's reported ~414 and with issue #512's attribution of errors to
`TaskMaster.csproj`: different sessions stopped at different points.

## Conclusions

1. **#512 confirmed.** The documented type-check command returns EXIT 0 having compiled nothing
   whenever outputs are current. Exit code alone cannot distinguish a real pass from a vacuous one.
2. **#492 confirmed.** Forcing a genuine recompile under the documented properties surfaces 195
   previously masked nullable errors in `UtilitiesCS.csproj`.
3. **#522 confirmed.** The documented command is unpassable on a clean tree, while CI's command
   (`/t:Rebuild`, no `/p:Nullable=enable`) both compiles and returns EXIT 0. The divergence is the
   `/p:Nullable=enable` flag, not the target.
4. **A reliable non-vacuous-compile assertion exists.** Asserting that the MSBuild file log contains
   zero occurrences of `Skipping target "CoreCompile"` cleanly separates M2 (18 skips, vacuous) from
   M3 and M4 (0 skips, genuine). Counting `csc.exe` invocations does **not** work at normal verbosity;
   all four logs report zero.
5. **Measured cost of `/t:Rebuild`.** 20.0 s versus 1.8 s for a warm vacuous `/t:Build`, against a
   25.8 s cold `/t:Build`. The honest gate costs roughly 18 s more per toolchain pass than the
   dishonest one, and slightly less than a cold build. Issue #492 asked for this measurement before
   committing to `/t:Rebuild`; it is modest.
