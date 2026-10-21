# Phase 0 — Build-Output Premises for the Harness Host

Timestamp: 2026-09-13T23-16

Command:

```
pwsh -NoProfile -Command '
$paths = @("TaskMaster.Test/bin/Debug/Deedle.dll","TaskMaster.Test/bin/Debug/TaskMaster.Test.dll.config","TaskMaster.Test/bin/Debug/TaskMaster.dll.config")
foreach ($p in $paths) { Write-Output ($p + " EXISTS=" + (Test-Path -LiteralPath $p)) }
'
```

The build read is the one produced by `[P0-T7]`, the nullable-gate `/t:Rebuild` that exited 0
with `Build succeeded.` and zero `Skipping target "CoreCompile"` occurrences.

EXIT_CODE: 0

Output Summary:

```
TaskMaster.Test/bin/Debug/Deedle.dll EXISTS=True
TaskMaster.Test/bin/Debug/TaskMaster.Test.dll.config EXISTS=True
TaskMaster.Test/bin/Debug/TaskMaster.dll.config EXISTS=True
```

`TaskMaster.Test/TaskMaster.Test.csproj` carries no direct `Deedle` reference, so the presence
of `Deedle.dll` in its output is a transitive copy from its `UtilitiesCS` and `ToDoModel`
project references. The spec recorded this as unverified because no `bin` output existed when
it was written; it is verified here.

## TaskMaster.Test app.config Premise

Recorded per `[P0-T11]`.

Command:

```
pwsh -NoProfile -Command '
$hits = @(Select-String -LiteralPath "TaskMaster.Test/app.config" -SimpleMatch -Pattern "netstandard")
Write-Output ("TASKMASTER_TEST_APPCONFIG_NETSTANDARD_HITS=" + $hits.Count)
$fs = @(Select-String -LiteralPath "TaskMaster.Test/app.config" -SimpleMatch -Pattern "FSharp.Core")
Write-Output ("TASKMASTER_TEST_APPCONFIG_FSHARPCORE_HITS=" + $fs.Count)
'
```

```
TASKMASTER_TEST_APPCONFIG_NETSTANDARD_HITS=0
TASKMASTER_TEST_APPCONFIG_FSHARPCORE_HITS=1
```

The `FSharp.Core` search is the positive control: it proves the file path resolves and the
search mechanism is live, so the zero `netstandard` count is an observation rather than an
artefact of a search that could not match anything. `TaskMaster.Test/app.config` is outside
this plan's authorised write set, so it cannot gain a `netstandard` entry during this work,
which is what keeps the negative control valid after the Phase 3 hardening lands in
`TaskMaster/app.config`.

## Baseline Line Counts

Recorded per `[P0-T16]`.

Command:

```
pwsh -NoProfile -Command '
$paths = @("TaskMaster/ThisAddIn.cs","TaskMaster/app.config","UtilitiesCS/UtilitiesCS.csproj","UtilitiesCS.Test/UtilitiesCS.Test.csproj","TaskMaster.Test/TaskMaster.Test.csproj")
foreach ($p in $paths) { Write-Output ($p + " LINES=" + @(Get-Content -LiteralPath $p).Count) }
'
```

```
TaskMaster/ThisAddIn.cs LINES=307
TaskMaster/app.config LINES=649
UtilitiesCS/UtilitiesCS.csproj LINES=1322
UtilitiesCS.Test/UtilitiesCS.Test.csproj LINES=997
TaskMaster.Test/TaskMaster.Test.csproj LINES=397
```

Only `TaskMaster/ThisAddIn.cs` is subject to the 500-line production-source limit among these
five; it is at 307 lines and gains a four-line static constructor in `[P3-T3]`. The remaining
four are a config file and three project files, none of which is production or test code under
that limit.
