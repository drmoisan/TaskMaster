# Phase 1 — Post-Restore Build and Embedded-Content Assertion (Cycle 1, Issue #503)

Timestamp: 2026-08-08T14-52
Task: [P1-T9]

Command: `pwsh -NoProfile -Command "Set-Location 'C:\Users\DanMoisan\repos\TaskMaster-wt\2026-08-08T11-55'; & 'C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe' TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform='Any CPU'"`
EXIT_CODE: 0

Command: `pwsh -NoProfile -File <SCRATCH>\Assert-EmbeddedRibbon.ps1 -RepoRoot 'C:\Users\DanMoisan\repos\TaskMaster-wt\2026-08-08T11-55'`
EXIT_CODE: 0

## Output Summary

### Build

```text
    6 Warning(s)
    0 Error(s)

Time Elapsed 00:00:07.99
```

Zero errors. Six warnings, all pre-existing: five `System.Reactive` packages.config advisories plus the `CS2002` duplicate-compile warning in `UtilitiesCS.Test.csproj` (issue **#510**, out of scope).

### Embedded-content assertion

```text
EMBEDDED_GETENABLED_COUNT=8
EMBEDDED_TRIAGESETA_SINGLELINE=False
EMBEDDED_ASSEMBLY_WRITETIME=2026-08-08T19:07:05.2659559Z
```

- **`EMBEDDED_GETENABLED_COUNT=8`** — the assembly under test again carries all eight attributes. The P1-T8 restoration has propagated into the built artifact, so the P1-T10 pass-after run reads the restored resource rather than the mutated one.
- `EMBEDDED_TRIAGESETA_SINGLELINE=False` — still expected; F2 (Phase 2) has not run yet.
- `EMBEDDED_ASSEMBLY_WRITETIME=2026-08-08T19:07:05.2659559Z` — advanced past the mutated-build value `2026-08-08T19:04:58.9399552Z`, confirming the assembly was rewritten rather than left carrying the mutation.

### Embedded-resource write-time sequence across Phase 1

| Task | Embedded `getEnabled` count | Assembly write time (UTC) |
|---|---|---|
| P0-T3 (baseline) | 8 | 2026-08-08T17:48:38.5907327Z |
| P1-T3 (pre-mutation) | 8 | 2026-08-08T18:57:19.8234515Z |
| P1-T6 (mutated) | **7** | 2026-08-08T19:04:58.9399552Z |
| P1-T9 (restored) | 8 | 2026-08-08T19:07:05.2659559Z |

Monotonically increasing write times with the count dropping to 7 only in the mutation window is direct evidence that no measurement in this phase was taken against a stale assembly.

Binary outcome satisfied: the helper reports `EMBEDDED_GETENABLED_COUNT=8`.
