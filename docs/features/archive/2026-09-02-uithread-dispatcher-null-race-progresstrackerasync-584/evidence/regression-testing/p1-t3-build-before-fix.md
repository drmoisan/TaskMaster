# P1-T3 — Build with the new regression test against the UNFIXED production code

Timestamp: 2026-09-03T08-33

Command:
```text
env -C <worktree-root> MSYS_NO_PATHCONV=1 msbuild.exe TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true
```

EXIT_CODE: 0

## Output Summary

Trailing MSBuild summary, verbatim:

```text
Build succeeded.
    0 Warning(s)
    0 Error(s)

Time Elapsed 00:00:17.50
```

- Errors: **0**
- Warnings: **0** (unchanged from the P0-T8 baseline warning count of 0)

At this point `UtilitiesCS/Threading/UiThread.cs` still carries the unfixed accessor
(`get => _dispatcher;` over a `null!`-initialised field); only P1-T1 and P1-T2 have been applied.
The solution compiles cleanly with the new `UiThread_Dispatcher_Tests` class present, which is the
property that makes P1-T4's red a runtime assertion failure rather than a compile failure — a genuine
fail-before.
