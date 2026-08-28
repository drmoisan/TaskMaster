# Type-Check Gate Baseline (P0-T9)

Timestamp: 2026-08-27T10-10
Task: [P0-T9]
Command: `& $MSBUILD TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true`
EXIT_CODE: 0
Output Summary: `Build succeeded.` with 5 warnings and 0 errors. All five are the same
`System.Reactive.PackagesConfigCheck.targets(31,5)` packages.config notice observed in the `P0-T8`
baseline. Zero `warning CS####` diagnostics appear anywhere in the log, so no per-file
`#nullable enable` opt-in produced a `CS86xx` diagnostic that `/p:TreatWarningsAsErrors=true` could
promote to an error.

## MSBuild summary counts

| Metric | Value |
| --- | --- |
| Build result | `Build succeeded.` |
| Total warnings | 5 |
| Total errors | 0 |

Log path: `TestResults/plan-logs/p0-t9/msbuild-nullable.log`

## Diagnostic-code inventory

A search of the whole log for the pattern `warning <CODE>` returned no matches, confirming that
none of the five warnings carries a compiler or analyzer diagnostic identifier. The five summary
entries are all the untagged MSBuild task warning from `System.Reactive.PackagesConfigCheck.targets`.

## Command-shape compliance

- `/t:Rebuild` was used, not `/t:Build`. MSBuild's up-to-date check does not invalidate on a
  command-line `/p:` change, so a warm `/t:Build` would return exit 0 having skipped `CoreCompile`
  on every project and the gate could not fail.
- `/p:Nullable=enable` was **not** added. This is character-for-character the command in
  `.github/workflows/ci.yml`, and the property is a solution-wide opt-in that conscripts every file
  which has never adopted the pragma.

## Interpretation

Exit code 0 satisfies the acceptance condition, so the non-zero branch of the plan's Notes rule 5
(`BLOCKED: pre-existing base-tree build failure`) was not taken. This run is also the most recent
`/t:Rebuild` at the end of Phase 0's build steps, so the Debug output it left in
`QuickFiler.Test/bin/Debug` is the output `P0-T12` consumes.

Raw log is git-ignored under `TestResults/` and is not committed.
