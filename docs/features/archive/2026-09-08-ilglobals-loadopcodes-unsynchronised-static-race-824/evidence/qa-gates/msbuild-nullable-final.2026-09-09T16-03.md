# QC loop step 4 — nullable gate (Issue #824, task P5-T6)

Timestamp: 2026-09-09T16-03

Command: `pwsh -NoProfile -Command 'Set-Location "<worktree-root>"; $msb = & "${env:ProgramFiles(x86)}/Microsoft Visual Studio/Installer/vswhere.exe" -latest -requires Microsoft.Component.MSBuild -find "MSBuild\**\Bin\MSBuild.exe" | Select-Object -First 1; & $msb TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true 2>&1 | Tee-Object -FilePath coverage/msbuild-nullable-final.log; exit $LASTEXITCODE'`

EXIT_CODE: 0

Output Summary:

```
Build succeeded.
    0 Warning(s)
    0 Error(s)

Time Elapsed 00:00:11.38
```

This is AC11 step 3, run character-for-character in the form CLAUDE.md quotes and the form
`.github/workflows/_build-nullable.yml` uses.

Two properties of the command are load-bearing and were preserved:

- **`/p:Nullable=enable` was not added.** No project in this repository carries a `<Nullable>`
  element and there is no `Directory.Build.props`, so that property would be a solution-wide opt-in
  conscripting every file that has never adopted the pragma. Omitting it loses no enforcement over
  any file that has opted in, and `UtilitiesCS/NewtonsoftHelpers/SDIL Reader/ILGlobals.cs` has opted
  in at line 1.
- **`/t:Build` was not substituted for `/t:Rebuild`.** A warm `/t:Build` returns exit 0 having
  skipped `CoreCompile` on every project, so the gate could not fail.

The non-vacuity proof and the CS86xx counts are recorded separately in
`evidence/qa-gates/msbuild-nullable-nonvacuity.2026-09-09T16-03.md`.

The log stays under `coverage/`, which is gitignored, and is not committed.
