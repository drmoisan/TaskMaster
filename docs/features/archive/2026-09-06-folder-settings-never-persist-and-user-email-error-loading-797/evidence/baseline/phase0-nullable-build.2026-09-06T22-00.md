# Phase 0 — Nullable and Warnings-as-Errors Rebuild Baseline (Issue #797)

Timestamp: 2026-09-07T09-17

Command: `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true "/flp:Verbosity=detailed;LogFile=coverage/plan797-baseline-nullable.log"`

EXIT_CODE: 0

No solution-wide nullable enable property was supplied to this command. `/p:Nullable=enable` is
deliberately absent from CI and from this run: no project in this repository carries a `<Nullable>`
element, so the property is a solution-wide opt-in that would conscript every file that has never
adopted the per-file `#nullable enable` pragma. Nullable enforcement in this repository is per-file
opt-in and `/p:TreatWarningsAsErrors=true` promotes the `CS86xx` diagnostics of files that have opted
in. `/t:Rebuild` was used, not `/t:Build`, so the compile target ran on every project.

## Discrimination, per rule R4

- Process exit code: 0.
- Summary line `    0 Error(s)` is present in the file log, at log line 66779.
- Warning count from the summary: 0, on the immediately preceding line as `    0 Warning(s)`.
- The summary block reads `Build succeeded.` followed by the two count lines above.

BASELINE-DIAGNOSTIC-IDS:

(empty — the build is clean, so no diagnostic identifier was reported as an error)

Both branches are recorded. The clean branch applies, so the subset comparison the non-clean branch
would use is not entered here; P5-T4 carries the same two branches and resolves against this empty
set.

Output Summary: The warnings-as-errors rebuild is clean at the baseline. Exit code 0, zero warnings,
zero errors, empty baseline diagnostic identifier set, and no solution-wide nullable property
supplied. The full detailed log was written to the git-ignored coverage directory and is not
committed; only these sanitized summary fields are recorded here.
