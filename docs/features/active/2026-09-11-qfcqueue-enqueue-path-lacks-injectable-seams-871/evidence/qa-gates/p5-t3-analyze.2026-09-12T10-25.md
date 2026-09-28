# P5-T3 — Analyzer rebuild gate, final QC loop

Timestamp: 2026-09-13T16-49
Command: msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true
EXIT_CODE: 0
LoopPass: 1

The msbuild executable was resolved through the Visual Studio locator to
Microsoft Visual Studio 18 Community, MSBuild Current, Bin, MSBuild.exe. The command was run while this
item held the shared cross-item build lock, which was acquired immediately before it and released
immediately after it returned. Outlook was confirmed closed before the command ran; a process
enumeration for the Outlook image name returned a count of zero, so no running add-in host held the
build output open.

## Counts captured by the anchored-pattern rule of P0-T9

The two integers below are captured by an anchored regular expression matched against the whole build
summary line, not by a substring search. The anchored pattern requires the line to consist of optional
leading whitespace, the integer, a single space, the literal word, the parenthesised plural marker and
optional trailing whitespace, and nothing else. A substring search is not used because the zero-error
text is also a substring of a ten-error line, so a substring match cannot distinguish a clean build from
one reporting ten errors.

```
    0 Warning(s)
    0 Error(s)
```

AnalyzerErrorCount: 0
AnalyzerWarningCount: 0

## Evidence that the compile actually ran

MSBuild's incremental up-to-date check does not invalidate on a command-line property change, so a build
target run would return exit 0 with the compile skipped on every project and the gate could not fail.
This command uses the rebuild target for exactly that reason, and the captured log confirms the compile
occurred rather than being skipped:

```
CscInvocationsInLog: 36
CoreCompileTargetOccurrences: present, including projects 15 and 17
BuildResultLine: Build succeeded.
TimeElapsed: 00:00:16.54
CapturedLogLineCount: 12093
```

Thirty-six compiler invocations appear in the log, so every project in the solution was recompiled from
source under the analyzer properties rather than being reported up to date.

## Movement against the baseline

P0-T9 recorded 0 errors and 0 warnings at the anchor. This run records 0 errors and 0 warnings after the
full change. The analyzer gate is unmoved: the five new C# files and the edits to the four existing ones
introduced no analyzer diagnostic of any severity.

Output Summary: The analyzer rebuild gate exited 0 with 0 Error(s) and 0 Warning(s), both captured by an
anchored regular expression over the whole summary line. The log shows 36 compiler invocations and
`Build succeeded.` in 16.54 seconds, so the rebuild target genuinely recompiled rather than skipping on
the incremental check. Both counts are unmoved from the P0-T9 baseline of 0 and 0. Acceptance met.
