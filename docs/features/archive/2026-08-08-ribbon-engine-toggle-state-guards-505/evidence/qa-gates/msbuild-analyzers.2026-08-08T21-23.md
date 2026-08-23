> **SUPERSEDED — attempt 1 of Phase 5.** This pass was aborted at P5-T6 by an environmental
> failure in `QuickFiler.Test` (see
> `<FEATURE>\evidence\other\phase5-attempt1-aborted.2026-08-08T21-30.md`), and the phase was
> restarted at P5-T1. The authoritative Phase 5 evidence is the second, uninterrupted pass at
> timestamps `2026-08-08T21-3x`. This artifact is retained as an audit trail only.
# P5-T4 — Analyzer Gate (`/t:Rebuild`)

Timestamp: 2026-08-08T21-23

Command:

```
pwsh -NoProfile -Command "Set-Location '<REPO>'; & '<MSBUILD>' TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug /p:Platform='Any CPU' /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true /fl /flp:'logfile=<REPO>\coverage\analyzer-p5t4.log;verbosity=normal'"
```

EXIT_CODE: **0**

## Output Summary

- **Errors: 0** — `: error ` occurs **0** times across the 11,597-line log.
- **Warnings: 6**

### No new analyzer diagnostics relative to the P0-T7 baseline

| Diagnostic | P0-T7 baseline | P5-T4 | Delta |
|---|---|---|---|
| `CS2002` (`UtilitiesCS.Test` duplicate `<Compile Include>` for `PercentageFormatterTests.cs`) | 2 | 2 | **0** |
| Untagged `System.Reactive.PackagesConfigCheck.targets(31,5)` `packages.config` advisory | 4 | 4 | **0** |
| Any other code | 0 | 0 | **0** |
| **Total** | 6 | 6 | **0** |

The warning set is byte-for-byte the merge-base set. **No new analyzer diagnostic was introduced
by this change**, and in particular neither new type
(`EngineToggleCatalog`, `EngineToggleStateCoordinator`) nor either glue file produced any
diagnostic.

### Mandatory non-vacuity proof

`csc.exe` invocation count read from `coverage\analyzer-p5t4.log`: **18** (lines matching
`csc\.exe /noconfig`). `Skipping target "CoreCompile"` occurs **0** times.

Assemblies compiled (from each invocation's `/out:`):

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

The count is greater than zero and includes both **`TaskMaster.csproj`**
(`/out:...TaskMaster.dll`) and **`TaskMaster.Test.csproj`** (`/out:...TaskMaster.Test.dll`). The
gate is therefore not vacuous, and the count matches the P0-T7 baseline exactly (18), so the
comparison is like-for-like.

`/t:Rebuild` rather than `/t:Build` is required here: MSBuild's legacy non-SDK up-to-date check is
timestamp-based and does not invalidate on a `/p:` change, so a `/t:Build` analyzer run following
any earlier build of the same tree skips `CoreCompile` for every project and reports `EXIT 0`
having analyzed nothing.

The `.log` file is written under the gitignored `coverage\` directory and is never committed
(rule 9).

Binary outcome: **PASS**.
