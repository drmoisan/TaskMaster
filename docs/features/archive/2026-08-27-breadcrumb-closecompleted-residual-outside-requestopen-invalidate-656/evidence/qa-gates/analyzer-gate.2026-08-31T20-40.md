# QA Gate — Analyzer Gate (Issue #656)

Timestamp: 2026-09-01T14-50
Task: [P4-T3] (toolchain loop pass 1, step 2)
Satisfies: AC-15 (together with the P4-T5 section below), AC-16 (the Non-Vacuity section below)

Gate Start: 2026-09-01T14:49:50.4380965-04:00

Command:
```
$vswhere = 'C:\Program Files (x86)\Microsoft Visual Studio\Installer\vswhere.exe'
$msbuild = & $vswhere -latest -requires Microsoft.Component.MSBuild -find 'MSBuild\**\Bin\MSBuild.exe' | Select-Object -First 1
& $msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true "/flp:LogFile=TestResults\msbuild\p4-t3-analyzer.log;Verbosity=normal"
```

EXIT_CODE: 0

Acceptance measurement:
`@(Select-String -Path TestResults\msbuild\p4-t3-analyzer.log -SimpleMatch '0 Error(s)').Count` = **1**,
which is greater than 0 as required. Elapsed 00:00:11.80.

Output Summary: Analyzer gate passed. `0 Error(s)` under `/p:EnableNETAnalyzers=true`
`/p:EnforceCodeStyleInBuild=true` with `/t:Rebuild`. No analyzer diagnostic is attributed to the
changed production file.

---

## Non-Vacuity:

Task: [P4-T4]
Satisfies: AC-16

Measurements:

- `@(Select-String -Path TestResults\msbuild\p4-t3-analyzer.log -SimpleMatch 'Skipping target "CoreCompile"').Count` = **0**
- `(Get-Item QuickFiler\bin\Debug\QuickFiler.dll).LastWriteTime` = `2026-09-01T14:49:56.4605766-04:00`
- `(Get-Item QuickFiler.Test\bin\Debug\QuickFiler.Test.dll).LastWriteTime` = `2026-09-01T14:49:58.6941383-04:00`

Both assembly timestamps are later than the `Gate Start:` value of
`2026-09-01T14:49:50.4380965-04:00` — by roughly 6.0 and 8.3 seconds respectively.

Why both measurements are needed: the zero skip-count is the assertion AC-16 states, but a zero
count is also what an empty log, a mis-scoped log, or a log written by a build that never ran would
produce. The two `LastWriteTime` values are the positive control. They prove both assemblies were
actually recompiled inside this gate's window, so the zero count reports a genuinely absent
`Skipping target "CoreCompile"` line rather than an absent log. The changed files therefore really
were compiled and really were seen by the analyzers.

`/t:Rebuild` is what makes this hold. MSBuild's up-to-date check does not invalidate on a
command-line `/p:` change, so a warm `/t:Build` returns exit 0 with `CoreCompile` skipped on every
project and runs no analyzers at all.

---

## Post-Change Warning Codes For BreadcrumbDropDownOpenCoordinator.cs:

Task: [P4-T5]
Satisfies: AC-15 (together with the P4-T3 result above)

none

Derivation:
```
Select-String -Path TestResults\msbuild\p4-t3-analyzer.log -SimpleMatch 'BreadcrumbDropDownOpenCoordinator.cs' | Select-String -SimpleMatch 'warning'
```
returned a match count of **0**, and the matched-line list is empty.

Subset comparison required by this task:

| Set | Value |
|---|---|
| `Baseline Warning Codes For BreadcrumbDropDownOpenCoordinator.cs:` (from `evidence/baseline/analyzer-gate.2026-08-31T20-40.md`) | none (empty set) |
| `Post-Change Warning Codes For BreadcrumbDropDownOpenCoordinator.cs:` | none (empty set) |

The empty set is a subset of the empty set, so the acceptance condition holds. The change introduced
no new warning attributed to the changed production file. This is the strongest form the comparison
can take: because the baseline set was empty, any single new warning on the changed file would have
broken the subset relation.

Solution-wide warning count is unchanged from the Phase 0 baseline; the pre-existing System.Reactive
`packages.config` diagnostics are the only warnings the build reports, and none of them names a file
in this item's footprint.
