# Phase 3 — QuickFiler.Test rebuild after the R3 change

Timestamp: 2026-09-09T14-21

Task: [P3-T7]

Command: `msbuild QuickFiler.Test\QuickFiler.Test.csproj /t:Rebuild /m /p:Configuration=Debug /p:Platform=AnyCPU`

This is the [P3-T2] command verbatim. A `/t:Rebuild` of this project transitively rebuilds
`QuickFiler`, so the guard change in `QuickFiler/Viewers/BreadcrumbPopupOwnerRegistry.cs` is picked
up without a separate solution build.

EXIT_CODE: 0

Verbatim `Build succeeded.` block:

```
Build succeeded.
    0 Warning(s)
    0 Error(s)
```

D7 check: no MSB3061 and no MSB3021 warning was reported.

Output Summary: `QuickFiler.Test` and its `QuickFiler` project reference rebuilt at exit 0 with 0
warnings and 0 errors on the tree carrying the two explicit `ArgumentNullException` throws and the
two rewritten XML docs.
