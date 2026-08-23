# Toolchain Step 2 (lint) — .NET Analyzers

Timestamp: 2026-08-08T16-37

Task: [P2-T3] — final QC loop, pass 1

Command: `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true /m`

MSBuild: `C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe`
(18.8.2 for .NET Framework)

EXIT_CODE: 0

```
    6 Warning(s)
    0 Error(s)

Time Elapsed 00:00:06.29
```

## Comparison against the P0-T8 baseline

| Metric | Baseline (P0-T8) | Post-change (P2-T3) | Delta |
|---|---|---|---|
| Errors | 0 | 0 | 0 |
| Warnings | 6 | 6 | 0 |
| CS86xx (nullable) diagnostics | 0 | 0 | 0 |
| Analyzer-rule diagnostics (CA/S/MA/RCS/AsyncFixer/RS) | 0 | 0 | 0 |

No new diagnostic of any kind. The warning set is identical to baseline, item for item:

| Count | Diagnostic | Assessment |
|---|---|---|
| 5 | `System.Reactive.PackagesConfigCheck.targets(31,5)`: packages.config not supported by System.Reactive v7.0+ | Pre-existing packaging warning; fixing requires a PackageReference migration, out of scope |
| 1 | `CSC : warning CS2002`: `PercentageFormatterTests.cs` specified multiple times | Pre-existing duplicate `<Compile Include>` in `UtilitiesCS.Test.csproj`; fixing requires a `.csproj` edit, which the scope boundary forbids |

## Why this is the effective nullable check on the changed file

`UtilitiesCS/OutlookObjects/Folder/WpfDispatcherYield.cs` carries a file-scoped `#nullable enable`
on line 1 (unchanged from pre-change, see
`<FEATURE>/evidence/baseline/source-under-test.2026-08-08T16-12.md`), and
`UtilitiesCS.Test/OutlookObjects/Folder/WpfDispatcherYieldTests.cs` gained one at P1-T8. Nullable
flow analysis therefore runs on both files in this ordinary analyzer build, independently of the
`/p:Nullable=enable` gate at P2-T4.

Both projects genuinely recompiled in this run — the `CS2002` warning is emitted by the
`CoreCompile` target and is present, and the build took 6.29s rather than the ~1s of an up-to-date
no-op. So this run did evaluate the changed code.

The count stayed at exactly 6 with zero CS86xx, which satisfies the P1-T6 acceptance condition
("no new CS86xx diagnostic is introduced") with a non-vacuous measurement.

## Loop state

Step passed, no file rewritten. No restart. Proceed to P2-T4.

Output Summary: PASS, EXIT_CODE 0. Solution-wide analyzer build produced 6 warnings and 0 errors in
6.29s — identical to the P0-T8 baseline, with zero new analyzer or nullable diagnostics. Both
warning categories are pre-existing and out of scope. `CoreCompile` ran (CS2002 present), so the
changed files were genuinely analyzed; zero CS86xx confirms P1-T6.
