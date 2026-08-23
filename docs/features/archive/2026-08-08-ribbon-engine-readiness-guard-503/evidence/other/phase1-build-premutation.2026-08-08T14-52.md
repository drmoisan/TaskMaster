# Phase 1 — Pre-Mutation Build and Embedded-Content Assertion (Cycle 1, Issue #503)

Timestamp: 2026-08-08T14-52
Task: [P1-T3]

Command: `pwsh -NoProfile -Command "Set-Location 'C:\Users\DanMoisan\repos\TaskMaster-wt\2026-08-08T11-55'; & 'C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe' TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform='Any CPU'"`
EXIT_CODE: 0

Command: `pwsh -NoProfile -File <SCRATCH>\Assert-EmbeddedRibbon.ps1 -RepoRoot 'C:\Users\DanMoisan\repos\TaskMaster-wt\2026-08-08T11-55'`
EXIT_CODE: 0

## Output Summary

### Build

```text
    5 Warning(s)
    0 Error(s)

Time Elapsed 00:00:02.29
```

Zero errors. The five warnings are the pre-existing `System.Reactive` packages.config advisories recorded in the P0-T9 and P0-T10 baselines.

### Assembly write times (confirming the corrected test actually compiled)

| Assembly | LastWriteTimeUtc |
|---|---|
| `TaskMaster.Test\bin\Debug\TaskMaster.Test.dll` | 2026-08-08 19:03:12 |
| `TaskMaster.Test\bin\Debug\TaskMaster.dll` | 2026-08-08 18:57:19 |

`TaskMaster.Test.dll` was rewritten by this build, which confirms the P1-T1 edit to `RibbonExplorerXmlTests.cs` was compiled rather than skipped by the incremental up-to-date check. `TaskMaster.dll` was not rewritten, which is correct: `RibbonExplorer.xml` has not changed at this point in the plan, so the embedded resource is already current.

### Embedded-content assertion

```text
EMBEDDED_GETENABLED_COUNT=8
EMBEDDED_TRIAGESETA_SINGLELINE=False
EMBEDDED_ASSEMBLY_WRITETIME=2026-08-08T18:57:19.8234515Z
```

- `EMBEDDED_GETENABLED_COUNT=8` — the assembly under test carries all eight `getEnabled="EngineCommand_GetEnabled"` attributes. This is the **unmutated** state, which is what P1-T4 needs in order to establish that the corrected assertion is green before the mutation.
- `EMBEDDED_TRIAGESETA_SINGLELINE=False` — expected; F2 has not run yet.

Binary outcome satisfied: the build exits 0 and the helper reports `EMBEDDED_GETENABLED_COUNT=8`.
