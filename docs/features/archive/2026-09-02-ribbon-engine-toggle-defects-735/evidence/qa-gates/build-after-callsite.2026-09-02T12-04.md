# Finding 2 — Build After the Call-Site Rewrite (P2-T10)

Timestamp: 2026-09-03T02-16
Task: [P2-T10]
Command: `& $msbuild TaskMaster.sln /t:Build /m /p:Configuration=Debug "/p:Platform=Any CPU"`
EXIT_CODE: 0

## Trailing counts

```
    5 Warning(s)
    0 Error(s)

Time Elapsed 00:00:05.31
```

Five warnings, unchanged from the P0-T6 and P0-T7 baselines (the System.Reactive `packages.config`
advisory, one per consuming project). Zero errors.

`Ribbon\RibbonController.Intelligence.cs` appears twice in the build log as an input on the
`csc.exe` command line for `TaskMaster.csproj`, confirming the rewritten partial was recompiled
rather than served from an up-to-date output.

Output Summary: Build succeeded with EXIT_CODE 0, 5 warnings and 0 errors after the Clear Spam
Manager call site was rerouted through the new gate.
