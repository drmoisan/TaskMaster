# F1 — Mutated Resource Confirmed Embedded in the Assembly Under Test (Cycle 1, Issue #503)

Timestamp: 2026-08-08T14-52
Task: [P1-T6]

Command: `pwsh -NoProfile -Command "Set-Location 'C:\Users\DanMoisan\repos\TaskMaster-wt\2026-08-08T11-55'; & 'C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe' TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform='Any CPU'"`
EXIT_CODE: 0

Command: `pwsh -NoProfile -File <SCRATCH>\Assert-EmbeddedRibbon.ps1 -RepoRoot 'C:\Users\DanMoisan\repos\TaskMaster-wt\2026-08-08T11-55'`
EXIT_CODE: 0

## Why this task is a hard gate, not an implementation detail

`RibbonExplorer.xml` is an **embedded resource**. `RibbonExplorerXmlTests` reads it through `assembly.GetManifestResourceStream("TaskMaster.Ribbon.RibbonExplorer.xml")` on the `TaskMaster.dll` copied into `TaskMaster.Test\bin\Debug\`. Editing the `.xml` on disk is invisible to the test until the assembly is rebuilt and re-copied.

Without this gate, an edit-then-run sequence would read a stale assembly still carrying eight attributes, the test would report **Passed**, and the fail-proof would be silently converted into a second vacuous check — proving nothing while appearing to prove everything. Asserting the embedded byte content **before** the failing run is attempted is the only way to distinguish "the assertion cannot fail" from "the assembly was stale".

## Output Summary

### Build

```text
    6 Warning(s)
    0 Error(s)

Time Elapsed 00:00:08.23
```

Zero errors. Six warnings: the five pre-existing `System.Reactive` packages.config advisories plus the pre-existing `CS2002` duplicate-compile warning in `UtilitiesCS.Test.csproj` (issue **#510**, out of scope). Both were present in the P0-T9 baseline.

### Embedded-content assertion

```text
EMBEDDED_GETENABLED_COUNT=7
EMBEDDED_TRIAGESETA_SINGLELINE=False
EMBEDDED_ASSEMBLY_WRITETIME=2026-08-08T19:04:58.9399552Z
```

- **`EMBEDDED_GETENABLED_COUNT=7`** — the gate condition. The assembly the test will read carries **seven** `getEnabled="EngineCommand_GetEnabled"` attributes, down from eight. The `TrainSpam` attribute deleted by P1-T5 is genuinely absent from the assembly under test.
- `EMBEDDED_ASSEMBLY_WRITETIME=2026-08-08T19:04:58.9399552Z` — later than the pre-mutation value `2026-08-08T18:57:19.8234515Z` recorded in `evidence/other/phase1-build-premutation.2026-08-08T14-52.md`, confirming the assembly was rewritten by this build rather than left stale.

### `/t:Rebuild` fallback

**Not required.** MSBuild's incremental resource check picked up the `.xml` edit on the ordinary `/t:Build` invocation, as evidenced by the count dropping to 7 and the assembly write time advancing. The `msbuild TaskMaster\TaskMaster.csproj /t:Rebuild ...` fallback specified by the task text was therefore not executed, and no second build invocation is recorded.

Binary outcome satisfied: the helper reports `EMBEDDED_GETENABLED_COUNT=7`.
