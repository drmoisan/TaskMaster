# Phase 1 — Test project build with only the AC1 regression test added

Timestamp: 2026-09-09T14-03

Task: [P1-T2]

Command: `msbuild UtilitiesCS.Test\UtilitiesCS.Test.csproj /t:Rebuild /m /p:Configuration=Debug /p:Platform=AnyCPU`

The one-word `AnyCPU` spelling is correct for a project-scoped invocation and is deliberately not
reconciled with the spaced `"/p:Platform=Any CPU"` used for the solution-scoped commands: the
spaced name is a solution-level platform that MSBuild maps onto a project GUID only when the
solution is the entry point, so pointed straight at the `.csproj` no property group would match,
`OutputPath` would never be set, and the build would fail before `CoreCompile`.

EXIT_CODE: 0

Verbatim `Build succeeded.` block:

```
Build succeeded.
    0 Warning(s)
    0 Error(s)
```

D7 check: the console output reported no MSB3061 and no MSB3021 warning, so no process was holding
the build output.

Output Summary: `UtilitiesCS.Test` rebuilt at exit 0 with 0 warnings and 0 errors on the tree
carrying only the new `PopulateWithCurrent_OnTwoFailingStoresInOneController_RetriesEachStoreOnce`
test and no production change. The assembly is therefore ready for the [P1-T3] fail-before run.
