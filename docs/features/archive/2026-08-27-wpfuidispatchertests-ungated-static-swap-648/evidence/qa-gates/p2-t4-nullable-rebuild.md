# P2-T4 — Nullable and Type-Check Gate (Post-Change)

Timestamp: 2026-09-01T14-31

Command:
```
"C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe" TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true
```
MSBuild was re-resolved through `vswhere.exe` and the command was issued through `pwsh` from the
checkout root. `/p:Nullable=enable` was not added.

EXIT_CODE: 0

Output Summary:

```
Build succeeded.
    5 Warning(s)
    0 Error(s)
```

## Acceptance conditions

1. **`0 Error(s)` appears in the output.** It does, at log line 11786, and is recorded verbatim above.
2. **This run's `EXIT_CODE:` equals the baseline's.** This run recorded `EXIT_CODE: 0`;
   `docs/features/active/2026-08-27-wpfuidispatchertests-ungated-static-swap-648/evidence/baseline/p0-t12-nullable-rebuild.md`
   records `EXIT_CODE: 0`. They are equal.
3. **The summary warning count does not exceed the baseline warning count.** This run: 5. Baseline: 5.
4. **No diagnostic names the path `Controllers\WpfUiDispatcherTests.cs`.** Filtering the log to lines
   matching `: error ` or `: warning ` and then searching those lines for the filename
   `WpfUiDispatcherTests.cs` returned **0** matches. As in P2-T3, the count is taken after the
   diagnostic-line restriction, because an unrestricted search of an MSBuild log also matches the
   `csc.exe` command line and the `BuildResponseFile` echo, both of which enumerate the project's
   compile items on a clean build.

   The one-directory-deep form is asserted rather than the bare filename, which is ambiguous against
   `UtilitiesCS.Test/Threading/WpfUiDispatcherTests.cs:12` — a file that is out of scope and could not
   be repaired on a restart without breaching AC-6 — and rather than the project-qualified form, which
   a legacy non-SDK project's project-relative compile item at
   `QuickFiler.Test/QuickFiler.Test.csproj:191` can leave unmatched in the printed diagnostic.

Reaching this task at all establishes that the Phase 0 nullable baseline recorded `EXIT_CODE: 0` and a
zero summary error count, because P0-T12 halts on a red nullable baseline. That is what makes the
absolute `0 Error(s)` demand here satisfiable rather than vacuous or unreachable.

The rewritten test introduces `async Task` and an `await`, so the compiler's async-flow diagnostics
apply to it under this gate; none was emitted.
