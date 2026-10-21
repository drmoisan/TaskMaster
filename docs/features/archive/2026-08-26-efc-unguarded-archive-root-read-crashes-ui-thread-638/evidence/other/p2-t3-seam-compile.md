# [P2-T3] Seam-declaration compile (Issue 638)

Timestamp: 2026-08-29T12-27

Command:

```
$mb = & "${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\vswhere.exe" -latest -requires Microsoft.Component.MSBuild -find "MSBuild\**\Bin\MSBuild.exe" | Select-Object -First 1
& $mb TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" 2>&1 | Tee-Object -FilePath 'TestResults\msbuild\p2-t3.log'
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

The declaration-only seam added by [P2-T1] compiles. `[P2-T2]` confirmed the seam has
exactly one occurrence across `QuickFiler`, `TaskMaster`, `UtilitiesCS` and `ToDoModel`
(excluding `obj` and `bin`), so it adds no call site and changes no behavior. Phase 3's
expected red will therefore be caused by the missing archive-root guard rather than by the
seam.

Console output was tee'd to `TestResults\msbuild\p2-t3.log`, outside the diff under
`.gitignore:39`.
