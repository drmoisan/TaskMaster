# Phase 2 — Post-Collapse Build and Embedded-Content Assertion (Cycle 1, Issue #503)

> **SUPERSEDED — this measurement no longer describes the tree.** The P2-T1 collapse was **reverted** at [P3-T2]; `EMBEDDED_TRIAGESETA_SINGLELINE` is `False` again and the embedded resource is back to the formatter-mandated multi-line form. See `evidence/qa-gates/f2-formatter-conflict.2026-08-08T14-52.md`. The record below is retained as the evidence of the attempt. The `EMBEDDED_GETENABLED_COUNT=8` finding is unaffected by the revert.

Timestamp: 2026-08-08T14-52
Task: [P2-T4]

Command: `pwsh -NoProfile -Command "Set-Location 'C:\Users\DanMoisan\repos\TaskMaster-wt\2026-08-08T11-55'; & 'C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe' TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform='Any CPU'"`
EXIT_CODE: 0

Command: `pwsh -NoProfile -File <SCRATCH>\Assert-EmbeddedRibbon.ps1 -RepoRoot 'C:\Users\DanMoisan\repos\TaskMaster-wt\2026-08-08T11-55'`
EXIT_CODE: 0

## Output Summary

### Build

```text
    6 Warning(s)
    0 Error(s)

Time Elapsed 00:00:07.97
```

Zero errors. Six warnings, all pre-existing: five `System.Reactive` packages.config advisories plus the `CS2002` duplicate-compile warning in `UtilitiesCS.Test.csproj` (issue **#510**, out of scope).

### Embedded-content assertion

```text
EMBEDDED_GETENABLED_COUNT=8
EMBEDDED_TRIAGESETA_SINGLELINE=True
EMBEDDED_ASSEMBLY_WRITETIME=2026-08-08T19:09:25.8990703Z
```

- **`EMBEDDED_GETENABLED_COUNT=8`** — all eight `getEnabled="EngineCommand_GetEnabled"` attributes survive the collapse in the built artifact, not merely on disk.
- **`EMBEDDED_TRIAGESETA_SINGLELINE=True`** — the gate condition, and the first time in this cycle it reports `True`. The helper tests for the exact byte sequence `<button id="TriageSetA" onAction="TriageSetA_Click" getEnabled="EngineCommand_GetEnabled" label="Set A" />` inside the embedded resource, so the collapsed single-line form is confirmed in the assembly the tests read.
- `EMBEDDED_ASSEMBLY_WRITETIME=2026-08-08T19:09:25.8990703Z` — advanced past the P1-T9 post-restore value `2026-08-08T19:07:05.2659559Z`, confirming the assembly was rewritten and P2-T5 will not read a stale artifact.

### Full embedded-resource sequence across Phases 0-2

| Task | Embedded `getEnabled` count | TriageSetA single-line | Assembly write time (UTC) |
|---|---|---|---|
| P0-T3 (baseline) | 8 | False | 2026-08-08T17:48:38.5907327Z |
| P1-T3 (pre-mutation) | 8 | False | 2026-08-08T18:57:19.8234515Z |
| P1-T6 (mutated) | **7** | False | 2026-08-08T19:04:58.9399552Z |
| P1-T9 (restored) | 8 | False | 2026-08-08T19:07:05.2659559Z |
| **P2-T4 (collapsed)** | **8** | **True** | 2026-08-08T19:09:25.8990703Z |

Binary outcome satisfied: the helper reports `EMBEDDED_GETENABLED_COUNT=8` **and** `EMBEDDED_TRIAGESETA_SINGLELINE=True`.
