# [P4-T6] Post-fix compile (Issue 638)

Timestamp: 2026-08-29T12-34

Command:

```
$mb = & "${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\vswhere.exe" -latest -requires Microsoft.Component.MSBuild -find "MSBuild\**\Bin\MSBuild.exe" | Select-Object -First 1
& $mb TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" 2>&1 | Tee-Object -FilePath 'TestResults\msbuild\p4-t6.log'
```

Same vswhere-resolved MSBuild as [P0-T10], including the `| Select-Object -First 1` suffix.
The resolved path is absolute and is therefore recorded unresolved.

EXIT_CODE: 0

Output Summary:

MSBuild's summary lines, quoted verbatim:

```
    5 Warning(s)
    0 Error(s)
```

The `TryGetArchiveRoot(out string archiveRoot)` helper added by [P4-T1] and the three call
sites added by [P4-T2] through [P4-T4] compile. The warning count is unchanged from the
[P0-T10], [P0-T11], [P2-T3] and [P3-T14] runs, so the fix introduces no new diagnostic.

[P4-T5] confirmed immediately before this build that
`Select-String -SimpleMatch 'OlAncestor = Globals.Ol.ArchiveRootPath'` returns **0** matches
and that `Select-String -SimpleMatch 'Globals.Ol.ArchiveRootPath'` returns exactly **1**
match, at `QuickFiler/Controllers/EfcDataModel.cs:284`, which is the executable assignment
`archiveRoot = Globals.Ol.ArchiveRootPath;` inside the `TryGetArchiveRoot` body declared at
`:280`. It is not a comment, and no commented-out copy of the old expression exists.
