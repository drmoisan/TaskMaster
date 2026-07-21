# Batch A — DO-NOT-ALTER Constraint Verification

Timestamp: 2026-07-19T01-00

Scope: Batch A pure-data / contract-leaf files. Per-file confirmation that only nullability annotations (and, where used, justified `!` with `// why` comments) changed; no scoring/corpus/probability math changed; no operation reordered; no new `if (x is null) throw` guard added.

| File | CS86xx at P0-T6 | Change applied | Behavior/logic change? |
|---|---|---|---|
| Bayesian/Prediction.cs | 1 | `#nullable enable`; `_class`/`Class` typed `T?` (unconstrained-generic null-state); `CompareTo(Prediction<T>? other)` parameter made nullable to match the existing `other is null → return 1` contract. `_probability.CompareTo(other._probability)` ordering untouched. | None. No `!`, no new guard. The pre-existing `if (other is null) return 1` branch is unchanged; the parameter annotation reflects that branch. |
| Bayesian/FolderHierarchyNode.cs | 0 (clean) | `#nullable enable` only. | None. Retains get-only `sealed record` shape with constructor-set `NodeKey`/`Children` and existing `?? throw new ArgumentNullException` guards; no `init` accessor added. |
| Bayesian/LcppnFolderPredictorConfig.cs | 0 (clean) | `#nullable enable` only. | None. |
| Bayesian/DoNotSerializeContractResolver.cs | 0 (clean) | `#nullable enable` only. `_propertyNames` remains constructor-initialized non-null; `CreateProperties` override untouched. | None. |
| Bayesian/BayesianClassifierExtensions.cs | 0 (clean) | `#nullable enable` only. | None. |

Confirmation:
- No `System.Diagnostics.CodeAnalysis` post-condition attribute added to any Batch A file.
- The `Prediction<T>.CompareTo` ordering and its `other is null → 1` null contract are unchanged (AC3).
- No new `if (x is null) throw` guard added on any path.
- No `init`/`record`/`record struct` introduced; `FolderHierarchyNode` keeps its existing `sealed record` + get-only + constructor shape.
- Public signature changes are limited to additive nullability annotations reflecting actual null behavior (AC5).
