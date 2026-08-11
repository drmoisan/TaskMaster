# ANALYZE verification ([P5-T3], AC2 / AC7)

Timestamp: 2026-08-10T23-53
Command: `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true /nologo /v:m /fl "/flp:logfile=coverage/qa-analyze.log;verbosity=normal"`
EXIT_CODE: 0

`MSBUILD` = `C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\amd64\MSBuild.exe`,
invoked via `pwsh -NoProfile -ExecutionPolicy Bypass -File coverage/run-analyze.ps1 -LogName qa-analyze`.

This is the corrected **ANALYZE** command as now documented at `CLAUDE.md` § C#1 item 2, § CUT3
step 2, § "C# Toolchain (run in this exact order)" step 2, `.claude/rules/csharp.md` § Toolchain
item 2, and `.claude/skills/csharp-qa-gate/SKILL.md` step 2, executed against the post-edit tree.

## Measurements

| Metric | Value | Acceptance |
|---|---|---|
| `EXIT_CODE` | **0** | required 0 — PASS |
| `Skipping target "CoreCompile"` count | **0** | required 0 — PASS |
| Elapsed | **20.3 s** | recorded |
| MSBuild summary | `0 Error(s)` | — |
| MSBuild summary | `6 Warning(s)` | not gated |

## AC2 counting-mechanism deviation (restated)

AC2 names "a `csc.exe` invocation count greater than zero in an MSBuild file log" as the non-vacuity
proof. **That count is zero at `verbosity=normal` even for genuine compiles**, so the parenthetical
as literally written is not satisfiable by the described log. AC2's substantive requirement — a
non-vacuous compile assertion, not exit code alone — is satisfied instead by asserting **zero**
occurrences of the literal string `Skipping target "CoreCompile"` in the `/fl` log. That message is
emitted **only** when the target is actually skipped, so it is strictly more discriminating.
`CoreCompile:` header lines are **not** counted, because they print even when the target is skipped;
counting them is the trap that produced the contradictory historical artifact `spec.md` cites. This
deviation is recorded in `spec.md` § "The non-vacuity assertion mechanism" and § "Recorded
deviations" item 1, and no criterion text is changed.

Measured skip count: **0**. The step compiled genuinely.

## Contrast with the defective documented form

| Run | Command | EXIT | Elapsed | Skip count |
|---|---|---|---|---|
| [P0-T10] | DOC-ANALYZE warm (`/t:Build`) | 0 | 2.8 s | **18** (ran no analyzers) |
| [P5-T3] (this run) | **ANALYZE** (`/t:Rebuild /m`) | 0 | 20.3 s | **0** |

## Output Summary

The corrected analyzer command executes, compiles every project genuinely, and passes against the
post-edit tree: `EXIT_CODE: 0`, `0 Error(s)`, **zero** `Skipping target "CoreCompile"` occurrences,
20.3 s. The documentation edits introduced no analyzer regression.
