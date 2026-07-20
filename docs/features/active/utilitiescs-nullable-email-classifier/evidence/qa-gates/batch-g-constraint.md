# Batch G — DO-NOT-ALTER Constraint Verification (Performance Tooling)

Timestamp: 2026-07-19T05-40

Scope-boundary note: `Performance/` was CONFIRMED IN SCOPE at P0-T6 (3 of 3 REMEDIATE candidates emit CS86xx). This phase is applicable. The `Performance/` Designer and `Form`-derived viewer files (ConfusionViewer(.Designer), MetricChartViewer(.Designer)) remain EXCLUDE.

Per-file confirmation that only nullability annotations and justified `!` changed; no measurement/serialization behavior change; no new `if (x is null) throw` guard; no `record`/`record struct`/`init` introduced.

## BayesianMetricTypes.cs
- `#nullable enable`; the DTO/record members that are populated by object-initializer or JSON deserialization (`Actual`/`Predicted`/`Class` strings, `Source`, `Drivers`, `Details`, `Series` `Precision`/`Recall`/`F1`) given `= null!` (no `record`/`init` introduced — the existing `record`/`class` shapes and mutable `{ get; set; }` are unchanged). The lazily-populated `VerboseOutcomes` dictionaries annotated nullable (`Dictionary<...>?`) to match their `?.`-guarded getters and `value?.ToDictionary()` setters.

## BayesianSerializationHelper.cs
- `#nullable enable`; the generic deserialization returns annotated nullable to reflect true I/O null behavior: `Deserialize<T>` -> `T?`, `DeserializeAsync<T>` (both overloads) -> `Task<T?>`, local `T? item = default`. `GetDisk` retains its non-null return type with a justified `return null!` on the AppData-missing branch (pre-existing behavior: callers dereference the disk without a guard).

## BayesianPerformanceMeasurement.cs (>500 lines, NOT split)
- `#nullable enable`; the `= null` default parameters and locals (`dataMiner`, `collection`, `testSource`, `testOutcomes`, `classifierGroup`, `ppkg`, `timer`, `verboseTestOutcomes`, `classifier`) annotated nullable; both `LoadIfNullAsync` overloads' parameters annotated nullable (matching their `??=` resolve pattern), with justified `!` after the `??=` chain (post-load the serialized test data is expected non-null; a length mismatch throws — pre-existing contract). `x ??= (await DeserializeAsync<...>())!` and `x = (await DeserializeAsync<...>())!` at the "reload-if-empty" sites (the reloaded artifacts are treated as non-null). `Prediction<T>.Class` (`T?`, from Batch A) absorbed via `.Class!`; measurement-loop derefs of assigned locals (`ppkg`/`ppkg2`/`timer`/`testOutcomes`/`verboseTestOutcomes`/`testScores`) use justified `!`. `RunSensitivityAsync(VerboseTestOutcome[]? ...)` parameter annotated nullable (called with `null`). No scoring math, threshold, or measurement logic changed.

Confirmation:
- No `System.Diagnostics.CodeAnalysis` post-condition attribute added; no polyfill.
- No `record`/`record struct`/`init` introduced (existing records untouched).
- No measurement/serialization behavior change; no new `if (x is null) throw` guard (AC3, AC5).
- `BayesianPerformanceMeasurement.cs` (>500 lines) was NOT split; the Designer/WinForms viewer files remain EXCLUDE.
