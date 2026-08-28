# [P2-T11] — Explicit `Volatile`-Accessed Backing Field, Green

Timestamp: 2026-08-27T20-34

Command:
```
& $msbuild TaskMaster.sln /t:Build /m /p:Configuration=Debug "/p:Platform=Any CPU"
& $vstest QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation /TestCaseFilter:"FullyQualifiedName~IsCoreInitialized_HasAnExplicitBackingField" "/Logger:trx;LogFileName=p2-t11-volatile-field-green.trx" /ResultsDirectory:docs\features\active\webview2-host-initializer-defects-476\evidence\regression-testing\p2-t11
```

EXIT_CODE: 0

## Output Summary

- Build: `EXIT_CODE=0`, `0 Error(s)`.
- Test run: **1 passed, 0 failed.** `Passed IsCoreInitialized_HasAnExplicitBackingField` /
  `Test Run Successful.` / `Total tests: 1` / `Passed: 1`.
- TRX `<Counters>`: `total="1" executed="1" passed="1" failed="0"`.

## What was implemented

- `private bool _isCoreInitialized;` declared as the explicit backing field. The auto-property
  `public bool IsCoreInitialized { get; private set; }` no longer exists.
- `public bool IsCoreInitialized => Volatile.Read(ref _isCoreInitialized);` — an acquire load.
- The write became `Volatile.Write(ref _isCoreInitialized, true);` — a release store.
- No new using directive was needed: `using System.Threading;` was already present in the file.
- The write remains **strictly after** the `core.WebMessageReceived` subscription and **strictly
  before** `CoreInitialized?.Invoke(this, EventArgs.Empty)`. A comment at the write site records that
  the pairing is load-bearing and that the three statements must not be reordered. The mechanical
  line-number check is `[P2-T12]`.

The structural test that failed at `[P2-T2]` with
`Expected explicitField not to be <null>` now passes: a non-public instance `bool` field named
`_isCoreInitialized` exists, it does not carry `CompilerGeneratedAttribute`, and no
`<IsCoreInitialized>k__BackingField` is declared.

This evidence is a structural proxy for the memory-ordering fix, not a proof that the race is
eliminated. The test's own XML documentation states that, and so does the corresponding acceptance
criterion.

## Artifact hygiene

TRX written with an explicit `LogFileName=`; host identifiers replaced in place; `<Counters>`
unmodified; empty vstest directories removed.
