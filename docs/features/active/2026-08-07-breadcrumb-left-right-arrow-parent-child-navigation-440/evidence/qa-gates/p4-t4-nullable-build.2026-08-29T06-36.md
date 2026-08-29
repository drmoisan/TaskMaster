# Phase 4 — Type-Check Step (issue #440, plan task P4-T4)

Timestamp: 2026-08-29T06-36

Command:

```
& $msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true /v:normal /fl "/flp:LogFile=coverage\logs\p4-t4-nullable.msbuild.txt;Verbosity=normal"
```

`/p:Nullable=enable` was deliberately **not** added, per CLAUDE.md C#1.3 and plan
Global rule 4. The log path sits under the gitignored `coverage/` tree per Global
rule 8.

EXIT_CODE: 0

## Output Summary

MSBuild summary lines, read from the tail of the file-logger output:

```
    5 Warning(s)
    0 Error(s)

Time Elapsed 00:00:15.17
```

- Error count: **0**. Gate requires 0. PASS.
- Warning count: **5**, the same pre-existing System.Reactive 7.0.0 packages-config
  advisories the P0-T12 baseline recorded. They are raised by an MSBuild targets file
  rather than by the compiler, so `/p:TreatWarningsAsErrors=true` does not promote
  them.

`UtilitiesCS/OutlookObjects/Folder/BreadcrumbStateModel.cs` carries a
`#nullable enable` directive at line 1, so any CS86xx diagnostic introduced in it
would be a hard error at this gate. The error count is 0, so the P2-T1 guard
relaxation introduced no nullable-flow diagnostic. The removed conjunct was an
`int`-valued comparison, and the retained `activeIndex.HasValue` conjunct still
short-circuits before `activeIndex.Value` is dereferenced, so the null-state analysis
is unchanged.

## Non-vacuity counts, read from `coverage\logs\p4-t4-nullable.msbuild.txt`

Both counted with `Select-String -SimpleMatch`. The log carries 11656 lines.

| Literal | Count | Gate | Result |
| --- | --- | --- | --- |
| `Skipping target "CoreCompile"` | **0** | must be 0 | PASS |
| `(Rebuild target(s))` | **40** | must be at least 1 | PASS |

The first count is the negative half of the non-vacuity proof: no project skipped
`CoreCompile`, so the compiler and nullable-flow diagnostics actually ran.

The second count is the positive half and is what fails if the log is empty or was
never written. The parenthesised `(s)` is part of the string MSBuild emits in its
ProjectStarted and `Done Building Project` messages at normal verbosity, which appear
on every run. The shorter spelling `(Rebuild target)` appears only inside the terminal
warning and error summary block, which MSBuild emits only when the build produced
diagnostics, so counting that spelling would fail exactly when the build is clean.

This is the type-check half of AC-14.
