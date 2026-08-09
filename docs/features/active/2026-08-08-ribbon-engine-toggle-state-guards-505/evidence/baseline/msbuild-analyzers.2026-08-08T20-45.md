# P0-T7 — Analyzer Build Baseline (`/t:Rebuild`)

Timestamp: 2026-08-08T20-45

Command:

```
pwsh -NoProfile -Command "Set-Location '<REPO>'; & '<MSBUILD>' TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug /p:Platform='Any CPU' /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true /fl /flp:'logfile=<REPO>\coverage\analyzer-p0t7.log;verbosity=normal'"
```

EXIT_CODE: 0

## Output Summary

- **Errors: 0**
- **Warnings: 6**
- Elapsed: 00:00:15.29

Distinct diagnostic codes present in the baseline log:

| Code | Count | Note |
|---|---|---|
| `CS2002` | 2 | Source file `UtilitiesCS.Test\OutlookObjects\Folder\PercentageFormatterTests.cs` specified multiple times. Pre-existing latent duplicate `<Compile Include>` in `UtilitiesCS.Test.csproj`; out of scope. |
| (untagged) | 4 | `System.Reactive.PackagesConfigCheck.targets(31,5)` advisory that `packages.config` is unsupported by System.Reactive v7.0+. Emitted per project; carries no diagnostic ID. |

## Mandatory non-vacuity proof

`csc.exe` invocation count read from `coverage\analyzer-p0t7.log`: **18** (counted as lines
matching the regex `csc\.exe /noconfig`).

Assemblies compiled (from each invocation's `/out:` argument):

```
QuickFiler.dll            QuickFiler.Test.dll
SVGControl.dll            SVGControl.Test.dll
Tags.dll                  Tags.Test.dll
TaskMaster.dll            TaskMaster.Test.dll
TaskTree.dll              TaskTree.Test.dll
TaskVisualization.dll     TaskVisualization.Test.dll
ToDoModel.dll             ToDoModel.Test.dll
UtilitiesCS.dll           UtilitiesCS.Test.dll
VBFunctions.dll           VBFunctions.Test.dll
```

The count is greater than zero and includes both **`TaskMaster.csproj`** (`/out:...TaskMaster.dll`)
and **`TaskMaster.Test.csproj`** (`/out:...TaskMaster.Test.dll`). `Skipping target "CoreCompile"`
occurs **0** times. The baseline is therefore non-vacuous.

Note on the count method: an initial probe using `Select-String -SimpleMatch 'csc\.exe'` returned
0 because the regex escape was interpreted literally under `-SimpleMatch`. The authoritative count
above uses a genuine regex match and is corroborated by the 18 distinct `/out:` assemblies, which
equals the solution's project count.

The `.log` file is written under the gitignored `coverage\` directory and is never committed
(plan rule 9).

Binary outcome: **PASS** — EXIT 0 with a non-zero `csc.exe` count covering both target projects.
