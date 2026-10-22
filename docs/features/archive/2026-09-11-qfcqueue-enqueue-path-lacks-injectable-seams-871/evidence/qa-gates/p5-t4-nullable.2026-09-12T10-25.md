# P5-T4 — Nullable rebuild gate, final QC loop

Timestamp: 2026-09-13T16-50
Command: msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true
EXIT_CODE: 0
LoopPass: 1

The msbuild executable was resolved through the Visual Studio locator to
Microsoft Visual Studio 18 Community, MSBuild Current, Bin, MSBuild.exe. The command was run while this
item held the shared cross-item build lock, which was acquired immediately before it and released
immediately after it returned. Outlook was confirmed closed before the command ran.

## Counts captured by the anchored-pattern rule of P0-T9

```
    0 Warning(s)
    0 Error(s)
```

NullableErrorCount: 0
NullableWarningCount: 0

Both integers are captured by an anchored regular expression matched against the whole build summary
line rather than by a substring search, under the same rule P0-T9 states.

## Command shape, recorded because two of its properties are load-bearing

No solution-wide nullable property is supplied. No project in this repository carries a nullable
element and there is no directory-level build props file, so forcing that property would conscript every
file that never adopted the per-file pragma and would produce a large error count that reflects nothing
about this change. Nullable enforcement here is per-file opt-in: a file participates when it carries the
pragma, and treating warnings as errors then promotes that file's nullable diagnostics to build errors.

The rebuild target is used rather than the build target, because MSBuild's incremental up-to-date check
does not invalidate on a command-line property change and a warm build target would return exit 0 with
the compile skipped on every project.

## Evidence that the compile actually ran

```
CscInvocationsInLog: 36
CoreCompileTargetOccurrences: 78
BuildResultLine: Build succeeded.
TimeElapsed: 00:00:15.93
CapturedLogLineCount: 12008
```

Thirty-six compiler invocations appear in the captured log, so the gate compiled from source rather than
reporting projects up to date.

## Nullable participation of the files this item created

`QuickFiler/Interfaces/IUiIdleDispatcher.cs` is the one new file this change permitted to carry the
per-file nullable pragma, so it participates in nullable analysis and its diagnostics would be promoted
to errors by this command. It produces none. The relocated partial parts
`QuickFiler/Controllers/QfcQueue.Tlp.cs` and `QuickFiler/Controllers/QfcQueue.UiIdle.cs` carry no
pragma, as P1-T9 and P2-T5 verified, so they do not participate — which is the intended outcome, because
adding a pragma to relocated code would have broken the verbatim-move property AC18 gates.

## Movement against the baseline

P0-T10 recorded 0 errors and 0 warnings at the anchor. This run records 0 and 0 after the full change.
The nullable gate is unmoved.

Output Summary: The nullable rebuild gate exited 0 with 0 Error(s) and 0 Warning(s), both captured by an
anchored regular expression over the whole summary line. The log shows 36 compiler invocations, 78
CoreCompile target occurrences and `Build succeeded.` in 15.93 seconds, so the rebuild genuinely
recompiled. Both counts are unmoved from the P0-T10 baseline of 0 and 0. Acceptance met.
