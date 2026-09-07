# [P2-T3] — #458 Owner Registry, Green

Timestamp: 2026-08-27T20-23

Command:
```
& $msbuild TaskMaster.sln /t:Build /m /p:Configuration=Debug "/p:Platform=Any CPU"
& $vstest QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation /TestCaseFilter:"FullyQualifiedName~SecondHost_DetachesThePredecessorAndTakesOwnership" "/Logger:trx;LogFileName=p2-t3-predecessor-detach-green.trx" /ResultsDirectory:docs\features\active\webview2-host-initializer-defects-476\evidence\regression-testing\p2-t3
```

EXIT_CODE: 0

## Output Summary

- Build: `EXIT_CODE=0`, `0 Error(s)`.
- Test run: **1 passed, 0 failed.** `Passed SecondHost_DetachesThePredecessorAndTakesOwnership` /
  `Test Run Successful.` / `Total tests: 1` / `Passed: 1`.
- TRX `<Counters>`: `total="1" executed="1" passed="1" failed="0"`.

## What was implemented in `QuickFiler/Viewers/WebView2BreadcrumbHost.cs`

- `private static readonly ConditionalWeakTable<WebView2, WebView2BreadcrumbHost> _owners` — the
  per-control owner registry.
- `private static readonly object _ownersGate` — the gate over the compound operation.
- `using System.Runtime.CompilerServices;` added to the existing using block. This is the only new
  using directive Phase 2 authorizes; `Volatile` in `[P2-T11]` is already covered by the existing
  `using System.Threading;`.
- In the constructor, under `_ownersGate`: `TryGetValue` with the out variable declared as
  `WebView2BreadcrumbHost? previous` and null-checked, then `previous?.DetachCore()`, then
  `Remove(_control)`, then `Add(_control, this)`. Only `TryGetValue`, `Remove`, and `Add` are used;
  `AddOrUpdate` is not, because its presence on `net481` is unverified.
- `private void DetachCore()` performs the real `-=` for `CoreWebView2InitializationCompleted` **on
  the predecessor instance**, whose delegate target matches, and sets that instance's attachment
  state to false.
- The dead line `_control.CoreWebView2InitializationCompleted -= OnCoreInitializationCompleted;` and
  the misleading comment above it that claimed idempotent hookup for pooled viewers were both
  deleted. The `+=` is kept. The comment was replaced by one that states what the code now does and
  why the previous mechanism could not work.

Verification that the dead unhook is gone and the `+=` remains is recorded in the `[P2-T3]` diff and
is the evidence for the corresponding Phase 5 criteria.

## Artifact hygiene

TRX written with an explicit `LogFileName=`; host identifiers replaced in place (`REPO-ROOT`, `USER`,
`HOST`); `<Counters>` unmodified; the empty vstest deployment and per-result directories removed.
