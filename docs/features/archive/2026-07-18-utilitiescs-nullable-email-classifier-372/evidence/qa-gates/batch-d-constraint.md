# Batch D — DO-NOT-ALTER Constraint Verification (Engine Bases / Generic Engines)

Timestamp: 2026-07-19T03-00

Scope: engine bases and generic engines. Per-file confirmation that only nullability annotations and justified `!` changed; `ThrowIfNull` bare-statement sites were remediated by justified `!` (honoring the #363 no-narrowing contract) rather than new throwing guards; `GetTristate` thresholds, `MulticlassEngine` `Condition`/filtering, and `ProbabilityThreshold` are unchanged.

## TristateEngine.cs (abstract base)
- `#nullable enable`; the eight null-by-default delegate/threshold members annotated nullable: `Tokenize`/`TokenizeAsync`/`CalculateProbability`/`CalculateProbabilityAsync`/`GetTristateAsync` (`Func<...>?`), `Callback` (`Action<object>?`), `CallbackAsync` (`Func<object,bool,Task>?`), `Threshhold` (`TristateThreshhold?`) — both property and backing field.
- `#363 ThrowIfNull` no-narrowing honored: after `Tokenize.ThrowIfNull(...)`, `TokenizeAsync.ThrowIfNull(...)`, and `Threshhold.ThrowIfNull(...)` bare statements, the subsequent dereferences use justified `!` (`Tokenize!(item)`, `await TokenizeAsync!(item)`, `Threshhold!.MinimumTrue`/`Threshhold!.MaximumFalse`) with `// why` comments. No `if (x is null) throw` added. The `Callback is not null`/`CallbackAsync is not null` narrowing checks are unchanged. `GetTristate(double)` threshold comparison logic is unchanged.

## ConditionalItemEngine.cs
- `#nullable enable`; nine builder/ctor-populated auto-properties given `= null!` (`TypedItem` given `= default!` as `T`). The functional constructor still assigns via the #363 `ThrowIfNull()` capture; `Serialize()`'s `SerializationEngine is not null` guard unchanged.

## MulticlassEngine.cs (abstract base)
- `#nullable enable`; `Globals`/`CgUtilities`/`ClassifierGroup`/`AsyncAction`/`AsyncCondition`/`EngineName`/`TypedItem` given `= null!` (set by constructor/InitAsync/builder). `IsActivated => ClassifierGroup is not null` and `Config => ClassifierGroup.Config` runtime behavior preserved.
- `InitAsync` and `CreateEngineAsync` return `Task<T?>` (the existing `return default` non-activated path); `LoadStagingData` returns `Task<MinedMailInfo[]?>` (the existing `return default` AppData-missing path). The caller asserts the staging collection non-null with a justified `!` (pre-existing non-null assumption at the `LoadClassifierGroup` call). `Condition`/`GetOlItemString` filtering and `ProbabilityThreshold = 0.8` unchanged.

## ManagerAsyncLazy.cs
- `#nullable enable`; `Configuration`/`_privateConfig` given `= null!` (set by `ResetConfigAsyncLazy`, a method the compiler does not track as ctor init). `GetAltLoader` reflection locals annotated (`MethodInfo? staticMethod`, `Func<BayesianClassifierGroup>? altLoader`, return `Func<...>?`); the deferred lambda uses justified `!` (only created on the non-null branch). The `if (Configuration is null) ResetConfigAsyncLazy();` reset is preserved with a justified `!` on the subsequent `await Configuration!`.

## ClassifierGroupUtilities.cs
- `#nullable enable`; the unconstrained-generic returns annotated nullable: `Deserialize<T>` -> `T?`, `DeserializeAsync<T>` -> `Task<T?>` (both already `return default(T)`), `TryLoadObjectAndGetMemorySize<T>` -> `(T? Object, long Size)` (already `return (default, 0)`); `ValidateJson<T>`'s consuming local annotated `T?` with the existing `if (obj != null)` check.

Confirmation:
- No `System.Diagnostics.CodeAnalysis` post-condition attribute added.
- No new `if (x is null) throw` guard added at any `ThrowIfNull` bare-statement site or scoring path; existing guards and `is not null` checks preserved (AC3, AC4).
- Base delegate/return annotations set so derived overrides in Batch E remain consistent (AC5).
