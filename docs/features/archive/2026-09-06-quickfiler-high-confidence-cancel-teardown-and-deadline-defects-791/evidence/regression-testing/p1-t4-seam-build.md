# [P1-T4] Seam build

Timestamp: 2026-09-06T14-34

Command: `msbuild TaskMaster.sln /t:Build /m /p:Configuration=Debug "/p:Platform=Any CPU"`

EXIT_CODE: 0

Output Summary: `Build succeeded. 0 Warning(s) 0 Error(s). Time Elapsed 00:00:08.57.` Seventeen
projects built.

This is an iterative build, not a gate build (R9). It uses `/t:Build` with no `/p:` gate switches,
because its purpose is to produce assemblies the test project compiles against, not to run a gate;
every source edit in [P1-T1] through [P1-T3] changed a file timestamp, so `CoreCompile` is not
skipped. The two gate builds in Phase 3 use `/t:Rebuild` with the CLAUDE.md switches.

## Seam declarations proved available by this build

- `QfcDequeueStop.ScanCapReached` and `IQfcDatamodel.QuiesceLoaderAsync(TimeSpan)` in
  `QuickFiler/Interfaces/IQfcDatamodel.cs` ([P1-T1]).
- `QfcDatamodel.QuiesceDebugLog` and the declaration-only `QfcDatamodel.QuiesceLoaderAsync`
  in `QuickFiler/Controllers/QfcDatamodel.QueueProcessing.cs` ([P1-T2]).
- `DefaultMaxScanWithoutAcceptance`, `DefaultZeroAcceptanceCeiling`, the two new optional
  constructor parameters, and the `MaxScanWithoutAcceptance` / `ZeroAcceptanceCeiling` internal
  get-only auto-properties in `QuickFiler/Controllers/QfcStreamingDequeueConfidenceGate.cs`
  ([P1-T3]).

## Constructor shape, read from the built assembly by reflection

```
ctor params=5  : tryTakeNext, scoreLoader, threshold, timeProvider, debugLog
ctor params=11 : tryTakeNext, scoreLoader, threshold, timeProvider, debugLog, sourceActive,
                 firstBatchDeadline, progressCallback, onRejected, maxScanWithoutAcceptance,
                 zeroAcceptanceCeiling
```

The wide constructor now declares exactly eleven parameters, which is the [P1-T3] acceptance and the
shape [P1-T5] must widen the fail-closed reflection helper to.

The bounds are stored in internal get-only auto-properties rather than `private readonly` fields
(D9). A private field assigned and never read raises CS0414, which the Phase 3 nullable gate's
`/p:TreatWarningsAsErrors=true` would promote to an error; this build's zero-warning result confirms
the auto-property form is warning-clean before [P2-T1] reads the values.
