# [P2-T5] — Disposed-Control Self-Detach, Green

Timestamp: 2026-08-27T20-26

Command:
```
& $msbuild TaskMaster.sln /t:Build /m /p:Configuration=Debug "/p:Platform=Any CPU"
& $vstest QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation /TestCaseFilter:"FullyQualifiedName~ControlDisposed_DetachesTheHost" "/Logger:trx;LogFileName=p2-t5-disposed-self-detach-green.trx" /ResultsDirectory:docs\features\active\webview2-host-initializer-defects-476\evidence\regression-testing\p2-t5
```

EXIT_CODE: 0

## Output Summary

- Build: `EXIT_CODE=0`, `0 Error(s)`.
- Test run: **1 passed, 0 failed.** `Passed ControlDisposed_DetachesTheHost` /
  `Test Run Successful.` / `Total tests: 1` / `Passed: 1`.
- TRX `<Counters>`: `total="1" executed="1" passed="1" failed="0"`.

## What was implemented

- The constructor now subscribes `_control.Disposed += OnControlDisposed;`.
- `private void OnControlDisposed(object? sender, EventArgs e)` calls `DetachCore()` and then, under
  the same `_ownersGate`, removes this control's registry entry — but only when the registered owner
  is still this instance, checked with `ReferenceEquals`. That guard matters: if a successor host has
  already taken ownership, a late `Disposed` notification from the predecessor's subscription must not
  evict the successor's entry.
- A disposed control therefore leaves no attached host and no registry entry behind.

This is secondary hygiene, not the #458 fix. It does not address the defect's stated failure — two
live hosts over one **undisposed** control — which the owner registry from `[P2-T3]` handles.
