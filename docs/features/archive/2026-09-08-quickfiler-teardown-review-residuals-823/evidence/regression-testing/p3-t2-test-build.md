# Phase 3 — QuickFiler.Test build with the rewritten rejection test and no production change

Timestamp: 2026-09-09T14-18

Task: [P3-T2]

Command: `msbuild QuickFiler.Test\QuickFiler.Test.csproj /t:Rebuild /m /p:Configuration=Debug /p:Platform=AnyCPU`

The one-word `AnyCPU` spelling is required for the same project-scoped reason [P1-T2] gives.

EXIT_CODE: 0

Verbatim `Build succeeded.` block:

```
Build succeeded.
    0 Warning(s)
    0 Error(s)
```

The two literal null arguments in the rewritten test raise no diagnostic here because
`QuickFiler.Test/Viewers/BreadcrumbPopupOwnerRegistryTests.cs` is nullable-oblivious and gains no
`#nullable enable` directive (D18). This build carries no `/p:TreatWarningsAsErrors=true`; the
confirming nullable gate is [P6-T4].

D7 check: no MSB3061 and no MSB3021 warning was reported.

Output Summary: `QuickFiler.Test` rebuilt at exit 0 with 0 warnings and 0 errors on the tree
carrying only the rewritten rejection test and no production guard change. The assembly is
therefore ready for the [P3-T3] fail-before run.
