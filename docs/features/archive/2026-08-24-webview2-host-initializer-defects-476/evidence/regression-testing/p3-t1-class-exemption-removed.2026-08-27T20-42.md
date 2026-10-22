# [P3-T1] — Class-Level Coverage Exemption Removed, Green

Timestamp: 2026-08-27T20-42

Command:
```
& $msbuild TaskMaster.sln /t:Build /m /p:Configuration=Debug "/p:Platform=Any CPU"
& $vstest QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation /TestCaseFilter:"FullyQualifiedName~WebView2BreadcrumbHost_CarriesNoClassLevelCoverageExemption" "/Logger:trx;LogFileName=p3-t1-class-exemption-removed.trx" /ResultsDirectory:docs\features\active\webview2-host-initializer-defects-476\evidence\regression-testing\p3-t1
Select-String -SimpleMatch -Pattern '1:1' -Path 'QuickFiler\Viewers\WebView2BreadcrumbHost.cs'
```

EXIT_CODE: 0

## Output Summary

- Build: `EXIT_CODE=0`, `0 Error(s)`.
- Test run: **1 passed, 0 failed.**
  `Passed WebView2BreadcrumbHost_CarriesNoClassLevelCoverageExemption` / `Test Run Successful.` /
  `Total tests: 1` / `Passed: 1`.
- TRX `<Counters>`: `total="1" executed="1" passed="1" failed="0"`.

## `1:1` search — before and after

Before this task ran:

```
PRE MATCH_COUNT=2
  line 15: /// 1:1 SDK-forwarding adapter implementing <see cref="IBreadcrumbWebHost"/> over the
  line 25: /// member forwards 1:1 to the WebView2 SDK or reacts to its events on a live control that
```

After this task ran:

```
POST MATCH_COUNT=0   (no output)
```

The file carried the false forwarding claim **twice** — once in the type `<summary>` (line 15) and
once in the `<remarks>` (line 25) — and both were corrected. Leaving the `<summary>` claim in place
would have preserved exactly the false-rationale statement #477 reports, so the zero-match result is
the evidence that both occurrences are gone, not just the `<remarks>` one.

## What was implemented

- The class-level `[ExcludeFromCodeCoverage]` attribute on `WebView2BreadcrumbHost` is removed.
- `using System.Diagnostics.CodeAnalysis;` is **kept** (search count 1), because the member-level
  attributes applied by `[P3-T2]` still require it.
- The type `<summary>` no longer describes the type as a "1:1 SDK-forwarding adapter"; it now states
  what the type actually does, including that every SDK touch outside the SDK's own event callbacks
  is marshalled through one dispatcher callback and that exactly one host owns a given control at a
  time. The stale claim that the type "hooks CoreWebView2 events idempotently for pooled-viewer
  re-initialization (EfcViewerQueue)" was also dropped, because the queue is a pre-warm pool of fresh
  instances rather than a recycle pool and the idempotent-hookup mechanism it described was the #458
  defect.
- The `<remarks>` block now states accurately which members remain exempt and why: the two SDK event
  handlers, whose event-argument types have no public constructor and which only the live SDK raises,
  and the two extracted private SDK forwards. It records explicitly that `InitializeAsync` is
  deliberately not exempt because its only SDK-reaching statements go through the mockable
  `IWebViewCoreInitializer` seam, and that the constructors, the marshalling decisions, the registry
  detach path and the state accessor are measured.

## Artifact hygiene

TRX written with an explicit `LogFileName=`; host identifiers replaced in place; `<Counters>`
unmodified; empty vstest directories removed.
