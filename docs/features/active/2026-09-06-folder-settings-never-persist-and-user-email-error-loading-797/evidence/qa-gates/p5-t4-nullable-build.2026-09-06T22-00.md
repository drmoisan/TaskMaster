# P5-T4 — Final Type-check Rebuild (Issue #797)

Timestamp: 2026-09-07T10-02

Command: `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true "/flp:Verbosity=detailed;LogFile=coverage/plan797-final-nullable.log"`

EXIT_CODE: 0

No solution-wide nullable enable property was supplied to this command. `/p:Nullable=enable` is
deliberately absent from CI and from this run: no project in this repository carries a `<Nullable>`
element, so the property is a solution-wide opt-in that would conscript every file which has never
adopted the per-file `#nullable enable` pragma. Nullable enforcement here is per-file opt-in, and
`/p:TreatWarningsAsErrors=true` promotes the `CS86xx` diagnostics of the files that have opted in.
`/t:Rebuild` was used, not `/t:Build`.

## Discrimination, per rule R4

- Process exit code: 0.
- Summary line `    0 Error(s)` is present in the file log, at log line 69081, preceded by
  `Build succeeded.` and `    0 Warning(s)`.

The Phase 0 nullable baseline was clean, so the primary acceptance branch applies and the subset
comparison of the alternative branch is not entered.

Three of the five modified production files and both created production files carry the per-file
nullable pragma and therefore participate in this gate: the store wrapper, the store wrapper
controller, the new display partial and the serializer all open with `#nullable enable`. The new
display partial carries that pragma on its first line precisely so the annotations on the relocated
members keep their nullable context; without it the compiler would report CS8632 on each of them and
this gate would fail.

Output Summary: The warnings-as-errors rebuild is clean after all four implementation phases. Exit
code 0, zero warnings, zero errors, and no solution-wide nullable property supplied.
