# Baseline — Analyzer gate ([P0-T9])

- Issue: #644
- Task: `[P0-T9]`
- Timestamp: 2026-08-29T08-15

Command: `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
Working directory: repository root (`<repo-root>`)
Shell: PowerShell (`pwsh -NoProfile`) — `msbuild` resolves only on the PowerShell PATH in this
environment and does not resolve from a POSIX shell.
EXIT_CODE: 0

## `/t:Rebuild` is load-bearing and was used

`/t:Rebuild` was used, not `/t:Build`. MSBuild's incremental up-to-date check does not invalidate
on a command-line `/p:` change, so a warm `/t:Build` returns exit 0 with `CoreCompile` skipped on
every project and runs no analyzers at all. That the compilation genuinely ran was verified
rather than assumed: the captured build log contains **36 `csc.exe` invocations** across 4981 log
lines, so the analyzers actually executed.

## msbuild final summary block

```
    5 Warning(s)
    0 Error(s)

Time Elapsed 00:00:19.67
```

**Warning count: 5. Error count: 0.**

## Warning composition

All five warnings are the same pre-existing `System.Reactive` package-format advisory, emitted
once per affected project by
`packages\System.Reactive.7.0.0\build\System.Reactive.PackagesConfigCheck.targets(31,5)`:

> The project contains a packages.config file, which is not supported by System.Reactive v7.0 or
> later. Please migrate to PackageReference.

Distribution of warning-bearing log lines by project (each project's warning is reported twice in
the log — once inline and once in the trailing summary block — which is why the grouped line
count is 2 per project against a summary count of 5 distinct warnings):

```
QuickFiler.csproj
TaskMaster.csproj
ToDoModel.csproj
UtilitiesCS.csproj
UtilitiesCS.Test.csproj
```

No warning in the build carries a `CS`, `CA`, or `IDE` diagnostic identifier. The advisory is
emitted by a package targets file and has no diagnostic code at all, so the analyzer diagnostic
count in this baseline is zero.

## Gate outcome

The command exited **0**, so the `REMEDIATION-REQUIRED` reporting branch this task authorizes was
**not** taken and Phase 1 may proceed. `[P1-T3]`, `[P4-T3]`, and `[P4-T4]` all require this build
to be green and it is.

`[P4-T3]`'s acceptance compares its warning count against this baseline. The value it must be at
or below is therefore **5**.

Output Summary: `/t:Rebuild` analyzer gate green at the pre-change base. **0 errors, 5 warnings**,
all five being the pre-existing `System.Reactive` `packages.config` advisory across
QuickFiler, TaskMaster, ToDoModel, UtilitiesCS, and UtilitiesCS.Test. 36 `csc.exe` invocations
confirm compilation and analyzer execution actually occurred rather than being skipped by
incrementality. `QuickFiler.Test\bin\Debug\QuickFiler.Test.dll` exists after this build, which is
the assembly `[P0-T11]` tests.
