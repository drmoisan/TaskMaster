# P1-T3 — Compile the Changed Assembly

Timestamp: 2026-09-01T14-05

Command:
```
"C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe" QuickFiler.Test/QuickFiler.Test.csproj /t:Rebuild /m /p:Configuration=Debug /p:Platform=AnyCPU
```
MSBuild was re-resolved through `vswhere.exe` as in P0-T9 and the command was issued through `pwsh`
from the checkout root.

EXIT_CODE: 0

Output Summary:

```
Build succeeded.
    3 Warning(s)
    0 Error(s)
```

The MSBuild summary line `0 Error(s)` appears in the output, which is this task's acceptance
condition, and no `error CS` or `error MSB` line appears anywhere in the log.

The platform operand is `AnyCPU` with no space, unlike the two solution gates. This command names a
project file rather than the solution: `QuickFiler.Test/QuickFiler.Test.csproj` declares configuration
groups only for `Debug|AnyCPU`, `Release|AnyCPU`, `Debug|x86` and `Release|x86` (`:32`, `:41`, `:49`,
`:53`) and contains no occurrence of the string `Any CPU`, so the space-bearing solution platform
would match no group, leave `OutputPath` undefined, and fail in
`_CheckForInvalidConfigurationAndPlatform`. `Debug|AnyCPU` sets `OutputPath` to `bin\Debug\` at
`QuickFiler.Test/QuickFiler.Test.csproj:36`, which is the directory P1-T8 reads
`QuickFiler.Test/bin/Debug/QuickFiler.Test.dll` from.

This is a compile check only. The two policy gates with their analyzer and nullable properties are
run in Phase 2 with `/t:Rebuild` on the full solution.
