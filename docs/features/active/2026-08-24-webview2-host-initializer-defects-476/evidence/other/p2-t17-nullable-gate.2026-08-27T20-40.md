# [P2-T17] — Phase 2 Nullable Gate

Timestamp: 2026-08-27T20-40

Command:
```
& $msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true
```
(run through `pwsh -NoProfile` from the workspace root; MSBuild resolved through `vswhere` to
`C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe`)

`/p:Nullable=enable` was **not** added.

EXIT_CODE: 0

## Output Summary

- `0 Error(s)`; `5 Warning(s)` — the same five pre-existing `packages.config` / System.Reactive
  packaging advisories recorded in the Phase 0 baseline, unchanged in count and content.
- Distinct `: error XXnnnn` lines: 0.
- Occurrences of the string `CS86` anywhere in the build log: **0**. No nullable-flow diagnostic was
  emitted from any file.
- **Non-vacuity check: `Skipping target "CoreCompile"` lines = 0.** `/t:Rebuild` recompiled every
  project, so the nullable-flow analysis actually ran over the new code. A warm `/t:Build` would have
  returned exit 0 with `CoreCompile` skipped and could not have failed.

This proves that all new code in the `#nullable enable` file
`QuickFiler/Viewers/WebView2BreadcrumbHost.cs` produces no `CS86xx` diagnostic. The nullable-sensitive
additions in that file are:

- `private BreadcrumbUiDispatcher? _dispatcher;`, declared nullable and read into a local that is
  null-checked before every use;
- the internal constructor's `BreadcrumbUiDispatcher? dispatcher` parameter;
- the `ConditionalWeakTable.TryGetValue` out variable, declared `WebView2BreadcrumbHost? previous`
  and null-checked with `?.`;
- `CoreWebView2? core` in `DetachCore` and in the `PostMessageJson` callback, both null-checked;
- `object? sender` on the new `OnControlDisposed` handler.

The Phase 0 baseline for this same command was also `EXIT_CODE: 0` with `0 Error(s)`, so the gate has
not been weakened and the result is a genuine no-regression.
