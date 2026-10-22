# Pass-After Build — [P4-T1]

Timestamp: 2026-09-14T11-35

Command:

```
pwsh -NoProfile -Command '& ([scriptblock]::Create((Get-Content -Raw "<build-lock-root>/acquire.txt"))) -Item "879"'
```

```
pwsh -NoProfile -Command '
Set-Location -LiteralPath "<repo-root>"
$vswhere = Join-Path ${env:ProgramFiles(x86)} "Microsoft Visual Studio\Installer\vswhere.exe"
$msb = @(& $vswhere -latest -products * -requires Microsoft.Component.MSBuild -find "MSBuild\**\Bin\amd64\MSBuild.exe")[0]
& $msb TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" *> "docs/features/active/2026-09-13-deedle-netstandard-21-bind-unsatisfiable-in-production-879/evidence/regression-testing/pass-after-build-console.2026-09-13T18-22.txt"
$LASTEXITCODE
'
```

```
pwsh -NoProfile -Command '& ([scriptblock]::Create((Get-Content -Raw "<build-lock-root>/release.txt"))) -Item "879"'
```

The `Set-Location` prefix is mechanically necessary and measurement-neutral: this executor was launched
without worktree isolation, so its inherited working directory is a different checkout and every
repository-relative path in the plan would otherwise resolve into the wrong tree. It is disclosed here
rather than omitted.

EXIT_CODE: 0

ExpectedExitCode: 0

Output Summary:

Build of `TaskMaster.sln` with the plain Debug configuration, after the Phase 3 production fix, succeeded.

```
L12393: Build succeeded.
L12394: 0 Warning(s)
L12395: 0 Error(s)
```

Anchored summary-line search over the console log:

```
ZERO_ERROR_LINES=1
WARNING_SUMMARY_LINES=1
TOTAL_LINES=12397
```

Acceptance Condition: MET. The console log contains one line matching `^\s+0 Error\(s\)$`.

Note for Phase 5: this console log is a raw MSBuild dump of 12,397 lines carrying absolute host paths, in
the same class as the `expect-fail-build-console` log that `## R6.5` describes. It is left raw here
because no Phase 4 task projects it; `[P5-T11]` and `[P5-T12]` are the tasks that handle raw console logs.
