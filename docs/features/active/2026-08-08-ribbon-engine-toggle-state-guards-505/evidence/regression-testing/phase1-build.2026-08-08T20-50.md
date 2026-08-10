# P1-T3 — Phase 1 Build (the red must be a runtime red)

Timestamp: 2026-08-08T20-50

Command:

```
pwsh -NoProfile -Command "Set-Location '<REPO>'; & '<MSBUILD>' TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform='Any CPU'"
```

EXIT_CODE: 0

Output Summary:

- **Errors: 0**, **Warnings: 5** (the untagged `System.Reactive.PackagesConfigCheck.targets(31,5)`
  advisories; no `CS`-tagged warning was emitted for the changed projects).
- Elapsed: 00:00:02.13 — an incremental build; only the two changed projects required
  recompilation.
- `TaskMaster.Test\bin\Debug\TaskMaster.Test.dll` file timestamp after the build: **20:49**, i.e.
  written by this invocation, confirming `TaskMaster.Test.csproj` recompiled with the new
  `<Compile Include="Ribbon\RibbonViewerEngineCallbackShapeTests.cs" />` entry and the modified
  `EngineCommandCatalogTests.cs`.

The new regression tests therefore **compile against the pre-fix production code**. The Phase 1
red is a genuine runtime red (assertions failing on real pre-fix behavior), not a compile failure,
which is what the plan requires and what distinguishes this delivery from #503's compile-time red.
No fail-before exception dossier is needed.

Binary outcome: PASS.
