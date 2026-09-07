# P5-T4 — AC3 enforcement site

Timestamp: 2026-09-07T14-26
Task: [P5-T4]
Issue: #796
Channel used: A

## Branch taken

HOST. The commit-before-cancel ordering is enforced inside the member `FinishClose` in
`QuickFiler/Viewers/BreadcrumbDropDownHost.cs`.

Quoted from evidence/other/close-ordering-decision.md:

> AC3-ENFORCEMENT-SITE: HOST

and from the derivation recorded beneath that line:

> The enforcement site is therefore the host, meaning `FinishClose` in
> `QuickFiler/Viewers/BreadcrumbDropDownHost.cs`. This decision leaves
> `QuickFiler/Viewers/BreadcrumbDropDownOpenCoordinator.cs` with no required change; it
> remains in the write set as a bound, not as an obligation.

`QuickFiler/Viewers/BreadcrumbDropDownOpenCoordinator.cs` was accordingly NOT modified by this
task. The COORDINATOR branch was not taken and no change was made there.

## What changed in the host

Two edits, both anchored on member names:

1. Inside `FinishClose`, the cancel operation's condition gained the latch term, so an
   `Uncommitted`-reason close does not cancel while a commit is in flight. The condition remains
   conditional on the reason, so the suppression is scoped rather than global.
2. Inside `Close(BreadcrumbDropDownCloseReason reason)`, an `ExplicitCommit` reason now sets the
   latch. That is the point at which a commit reaches this host, and it is the only in-scope
   producer: the open coordinator reaches the host through `IBreadcrumbDropDownHost`, which this
   item does not change, so it cannot set an internal property on the concrete host.

Unchanged inside `FinishClose`, as the plan requires: the `DropDown.AutoClose = true` restore, which
remains the first operation, and the gated `FocusAnchorIfPermitted` argument, which remains the
last. The `MayTakeFocus` property default is likewise unchanged, still `() => true`.

## Compile check

Command:

```
pwsh -NoProfile -Command '$vswhere = Join-Path ${env:ProgramFiles(x86)} "Microsoft Visual Studio\Installer\vswhere.exe"; $msbuild = & $vswhere -latest -products * -find "MSBuild\**\Bin\MSBuild.exe" | Select-Object -First 1; & $msbuild TaskMaster.sln /t:Rebuild /m /nodeReuse:false /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true "/flp:LogFile=TestResults\796\p5-t4\analyzer-rebuild.log;Verbosity=detailed"; "EXIT_CODE=$LASTEXITCODE"'
```

EXIT_CODE: 0
Build summary: Build succeeded. 0 Warning(s). 0 Error(s).
Raw log (gitignored): TestResults/796/p5-t4/analyzer-rebuild.log

The transition of the two AC3 tests to Passed is measured by task P5-T7 and is that task's
acceptance rather than this one's.

Output Summary: HOST branch taken; `FinishClose` consults the latch and `Close` sets it on an
explicit commit; the open coordinator is untouched; the solution rebuilds clean.
