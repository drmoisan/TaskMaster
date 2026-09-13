# Phase 0 — Analyzer Rebuild Baseline

Timestamp: 2026-09-13T05-05
Task: [P0-T6]

Command: msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true "/flp:LogFile=TestResults/msbuild/p0-t6-analyzers.txt;Verbosity=detailed"
EXIT_CODE: 0
WarningCount: 0
ErrorCount: 0

MSBuild was resolved through vswhere at the explicit installer path, per D1, and resolved to the
Visual Studio 18 Community MSBuild under `MSBuild\Current\Bin`. `/t:Rebuild` was used rather than
`/t:Build`, per D2.

Output Summary: the build succeeded. MSBuild printed `Build succeeded.` followed by the summary lines
`0 Warning(s)` and `0 Error(s)`, and `Time Elapsed 00:00:18.51`.

The two counts were read from the summary lines that end in `Warning(s)` and `Error(s)` using a
start-anchored whole-line match, not a bare substring search, because a substring search for a
zero-valued count also matches a ten-valued one. Exactly one line matched each pattern, and the two
matched lines read:

```
0 Warning(s)
0 Error(s)
```

## Non-Vacuity Observation

The detailed file log was searched for the two csc command-line literals that MSBuild echoes under each
project's CoreCompile heading. Both are present:

- `/out:obj\Debug\UtilitiesCS.dll` — 2 matching lines
- `/out:obj\Debug\UtilitiesCS.Test.dll` — 2 matching lines

Each count is at least one, which is what distinguishes a real compilation from a build whose
CoreCompile was skipped as up to date. The exit code cannot distinguish the two cases. Per D10 the
detailed log remains at the git-ignored transient path and is not committed; `git check-ignore -v`
resolves it to the `[Tt]est[Rr]esult*/` pattern on line 39 of the repository ignore file.

## Pre-Existing Analyzer HintPath Skew, Remediated Without Editing Any Tracked File

The first invocation of this command failed with `EXIT_CODE: 1`, `0 Warning(s)` and `2 Error(s)`. Both
errors were `CS0006: Metadata file '..\packages\Meziantou.Analyzer.3.0.203\analyzers\dotnet\roslyn5.0\cs\Meziantou.Analyzer.dll' could not be found`,
raised by the VBFunctions project and the UtilitiesCS project; every dependent project then reported
FAILED transitively.

The cause is a version skew internal to the project files rather than a fault in the restore. Each
project's packages.config resolves Meziantou.Analyzer to 3.0.235, which is the single version the
restore installed into the packages directory, while the `<Analyzer Include>` item in 15 of the 16
first-party projects still names 3.0.203. The TaskMaster project alone names 3.0.235, which is why the
skew count is 15 and not 16. The restore therefore reports success and gives no hint: the disagreement
is between two different places in the same project file.

The skew is pre-existing and was not introduced by this branch. Two observations establish that:

- `git diff --name-only origin/main...HEAD -- "*.csproj" "*.config" "*.props" "*.targets"` produced no
  output, so this branch modified no project file, configuration file, properties file or targets file.
- `git grep -c "Meziantou.Analyzer.3.0.203" origin/main -- UtilitiesCS/UtilitiesCS.csproj VBFunctions/VBFunctions.csproj`
  reported one match in each file on the main branch, so the main branch carries the identical skew.

The condition only affects a cold worktree. A long-lived worktree accumulates package directories across
version bumps and the restore never deletes superseded ones, so the older directory is still present and
the stale HintPath still resolves. This worktree was created fresh and had no such directory.

The remedy applied was to provision the HintPath-named version into the git-ignored packages tree rather
than to edit any project file:

```
nuget install Meziantou.Analyzer -Version 3.0.203 -OutputDirectory packages -DependencyVersion Ignore
```

That command exited 0 and the HintPath target now resolves on disk. It changed no tracked file:
`git check-ignore -v` resolves the provisioned path to the `**/[Pp]ackages/*` pattern on line 191 of the
repository ignore file, and
`git status --porcelain --untracked-files=all -- "*.csproj" "*.config" "*.props" "*.targets"` produced no
output after the provisioning. Phase 0's rule that no source is edited is therefore intact, and
UtilitiesCS.csproj and UtilitiesCS.Test.csproj remain unmodified even though both are Write Set members
whose Phase 1 edits must be the only changes they carry.

This remediation is a mechanically necessary micro-action of the same class as the repo-local SDK
install and the packages.config restore: without it the task's command cannot produce a measurement at
all, and an admitted red baseline here would make every Phase 2 exit-zero demand unmeetable. It creates
no independent outcome. After it was applied, the task's command was re-run unchanged and produced the
result recorded above. The underlying repository defect is reported to the caller rather than fixed here,
because repairing the 15 skewed `<Analyzer Include>` items would modify 14 project files outside the
Write Set.

## Build Lock

The cross-item build lock was held across each MSBuild invocation only. The failing first invocation
held it from 2026-09-13T05:03:38 to 2026-09-13T05:03:55; the analyzer provisioning ran outside the lock
because it contends on no build output; the passing invocation held it from 2026-09-13T05:04:55 to
2026-09-13T05:05:31. Outlook was confirmed not running before either invocation, so no test host held
the build output and no MSB3021 occurred.
