# [P2-T7] — `PostMessageJson` Marshalled Through One Dispatch Callback, Green

Timestamp: 2026-08-27T20-29

Command:
```
& $msbuild TaskMaster.sln /t:Build /m /p:Configuration=Debug "/p:Platform=Any CPU"
& $vstest QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation /TestCaseFilter:"FullyQualifiedName~PostMessageJson_PostsExactlyOnceToTheUiContext" "/Logger:trx;LogFileName=p2-t7-post-marshalled-green.trx" /ResultsDirectory:docs\features\active\webview2-host-initializer-defects-476\evidence\regression-testing\p2-t7
```

EXIT_CODE: 0

## Output Summary

- Build: `EXIT_CODE=0`, `0 Error(s)`.
- Test run: **1 passed, 0 failed.** `Passed PostMessageJson_PostsExactlyOnceToTheUiContext` /
  `Test Run Successful.` / `Total tests: 1` / `Passed: 1`.
- TRX `<Counters>`: `total="1" executed="1" passed="1" failed="0"`.

## What was implemented

`PostMessageJson` now contains one local function holding all four steps in order:

1. the `_control.CoreWebView2` read,
2. the null guard,
3. the existing `log.Error("PostMessageJson called before CoreWebView2 initialization; payload dropped.")`
   message, unchanged in content,
4. the `core.PostWebMessageAsJson(json)` forward.

That single unit is passed to `dispatcher.Dispatch(...)` when a dispatcher exists, and invoked inline
on the calling thread when it does not (the pre-initialization window), which is the same
null-dispatcher fallback `[P2-T6]` applies to `NavigateToString`.

The read is **not** performed as a separate `DispatchValue` step. `DispatchValue` runs inline only
when `ReferenceEquals(_executingDispatcher, this)` holds — that is, only from inside an
already-executing `Dispatch` callback — and otherwise faults on an owner-thread-only test dispatcher
(`BreadcrumbUiDispatcher.cs:166`, `:180-188`).

A local function was used rather than a new named private method so that the four steps stay inside
the measured `PostMessageJson` member and no member outside the Phase 3 exemption table is
introduced.

## Artifact hygiene

TRX written with an explicit `LogFileName=`; host identifiers replaced in place; `<Counters>`
unmodified; empty vstest directories removed.
