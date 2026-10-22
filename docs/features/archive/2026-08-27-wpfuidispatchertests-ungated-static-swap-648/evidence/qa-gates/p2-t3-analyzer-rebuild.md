# P2-T3 — Analyzer Gate (Post-Change)

Timestamp: 2026-09-01T14-27

Command:
```
"C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe" TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true
```
MSBuild was re-resolved through `vswhere.exe` as in P0-T9 and the command was issued through `pwsh`
from the checkout root.

EXIT_CODE: 0

Output Summary:

```
Build succeeded.
    5 Warning(s)
    0 Error(s)
```

## Acceptance conditions

1. **`0 Error(s)` appears in the output.** It does, at log line 11669, and is recorded verbatim above.
2. **This run's `EXIT_CODE:` equals the baseline's.** This run recorded `EXIT_CODE: 0`;
   `docs/features/active/2026-08-27-wpfuidispatchertests-ungated-static-swap-648/evidence/baseline/p0-t11-analyzer-rebuild.md`
   records `EXIT_CODE: 0`. They are equal.
3. **The summary warning count does not exceed the baseline warning count.** This run: 5. Baseline: 5.
   5 does not exceed 5. The distinct diagnostic text is unchanged from the baseline: every one of the
   five is the System.Reactive `packages.config` warning, emitted once per owning project. Enumerating
   the distinct diagnostic texts in this log returned exactly one entry:

   ```
   warning : The project contains a packages.config file, which is not supported by System.Reactive
   v7.0 or later. Please migrate to PackageReference.
   ```

4. **No diagnostic in this run names the path `Controllers\WpfUiDispatcherTests.cs`.** Filtering the
   log to lines matching `: error ` or `: warning ` and then searching those lines for the filename
   `WpfUiDispatcherTests.cs` returned **0** matches.

   The asserted string is the one-directory-deep form rather than the bare filename, which is
   ambiguous against the out-of-scope `UtilitiesCS.Test/Threading/WpfUiDispatcherTests.cs:12`, and
   rather than the project-qualified form, which a legacy non-SDK project's project-relative compile
   item at `QuickFiler.Test/QuickFiler.Test.csproj:191` can leave unmatched in the printed diagnostic.

   A note on the search method, recorded because it is a live hazard here. A fixed-string search of
   the whole log for `Controllers\WpfUiDispatcherTests.cs` returns 2 matches, at log lines 10384 and
   10387. Neither is a diagnostic: 10384 is the `csc.exe` command line and 10387 is the
   `BuildResponseFile` echo of the same arguments, both of which enumerate the project's compile items
   including the owned file. A search that did not first restrict to diagnostic lines would therefore
   report a non-zero count on a clean build and make this condition unsatisfiable. The count of 0
   above is taken after the diagnostic-line restriction.

Reaching this task at all establishes that the Phase 0 analyzer baseline recorded `EXIT_CODE: 0` and a
zero summary error count, because P0-T11 halts on a red analyzer baseline. That is what makes the
absolute `0 Error(s)` demand here satisfiable rather than vacuous or unreachable.
