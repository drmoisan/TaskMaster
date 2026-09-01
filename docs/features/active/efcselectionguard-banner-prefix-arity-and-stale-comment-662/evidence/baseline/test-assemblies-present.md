# Owned Test Assemblies Present (P0-T10)

Timestamp: 2026-09-01T15-51

Command: `Get-Item 'QuickFiler.Test\bin\Debug\QuickFiler.Test.dll','UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll' | Select-Object Name,Length,LastWriteTime | Format-List`

EXIT_CODE: 0

Output Summary:

Both owned test assemblies exist under `Debug` / `Any CPU` after the Phase 0
builds:

| Path | Size (bytes) | LastWriteTime |
|---|---|---|
| `QuickFiler.Test\bin\Debug\QuickFiler.Test.dll` | 1721344 | 9/1/2026 3:43:20 PM |
| `UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll` | 3940352 | 9/1/2026 3:43:24 PM |

Both last-write times fall inside the window of the P0-T8 analyzer rebuild,
which confirms the assemblies were produced by the Phase 0 builds rather than
carried in. Neither assembly was present in this worktree before Phase 0 ran,
so any test invocation attempted before this point would have had nothing to
run.
