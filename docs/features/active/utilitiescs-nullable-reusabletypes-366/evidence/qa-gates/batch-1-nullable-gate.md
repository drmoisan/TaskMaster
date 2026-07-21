# Batch 1 — Nullable Pragma Gate (P1-T3)

Timestamp: 2026-07-19T09-12

## Commands

1. Format: `dotnet tool run csharpier format .` (CSharpier v1.2.6 requires the `format` subcommand;
   the legacy `csharpier .` form is not valid in v1). EXIT_CODE: 0 — "Formatted 1406 files".
2. Pragma gate (plan's exact command):
   `msbuild UtilitiesCS/UtilitiesCS.csproj /t:Rebuild /p:Configuration=Debug /p:Platform=AnyCPU /p:TreatWarningsAsErrors=true`
   (WITHOUT `/p:Nullable=enable`; `AnyCPU` per the csproj Platform condition — see baseline
   P0-T5). This literal command fails fast on the pre-existing vendored `SVGControl` CS0649
   (documented at P0-T5), before UtilitiesCS compiles, so it emits no cluster signal.
3. Isolated cluster measurement (mechanically necessary, per P0-T5 methodology): SVGControl
   pre-built clean, then
   `msbuild UtilitiesCS/UtilitiesCS.csproj /t:Rebuild /p:Configuration=Debug /p:Platform=AnyCPU /p:TreatWarningsAsErrors=true /p:BuildProjectReferences=false`
   which compiles UtilitiesCS and emits its diagnostics.

`/p:Nullable=enable` is NOT passed in any command.

EXIT_CODE (isolated compile): 1 (whole-project; attributable solely to the 14 pre-existing
non-cluster CS0168/CS0618 TWAE errors enumerated at P0-T5; unchanged by this batch).

## Output Summary

Batch 1 (13 files) cluster diagnostics under the per-file pragma:
- CS86xx count attributed to `ReusableTypeClasses/`: 0 (AC1 satisfied for Batch 1)
- CS8714 count attributed to `ReusableTypeClasses/`: 0
- Pre-existing non-cluster UtilitiesCS TWAE errors: 14 (unchanged from baseline; out of scope)

An intermediate build surfaced 5 CS8618 (unconstrained-generic auto-properties on
`BagChangedEventArgs<T>` NewValue/OldValue and `DictionaryChangedEventArgs<TKey,TValue>`
Key/NewValue/OldValue). Fixed by declaring those carriers as `T?` / `TKey?` / `TValue?` (they are
`default(T)` when the action-only constructor is used) — nullable annotation only, no behavior
change. Also annotated `LockingObservableLinkedListChangedEventArgs<T>` NewNode/OldNode as
`...Node?` and `ObserverHelper<T>._unsubscriber` as `IDisposable?` with a justified `!` at the
deferred-init dereference. Final gate: zero CS86xx.

No `System.Diagnostics.CodeAnalysis` post-condition attribute was added.
