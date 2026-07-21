# Final Verification — AC5 Signature Compatibility (P6-T9)

Timestamp: 2026-07-19T05-40

Method: `git diff -- UtilitiesCS/Extensions/` reviewed for every public/internal signature change. Diff totals: 23 files changed, 167 insertions(+), 78 deletions(-) — all additions are `#nullable enable` pragmas, nullability annotations (`?`), justified `!` operators, and `// why` comments. The normal solution build (no TWAE) compiled clean at every batch, proving no existing caller breaks.

Every signature-level change is an additive nullability annotation that reflects the method's actual null behavior; no parameter was added or removed, no type semantics changed, no method was renamed, and no runtime behavior changed:

- ArrayExtensions.cs: `TryFlattenArrayTree<T>` -> `T[]?`; internal `FlattenArrayTree<T>(...bool strict)` -> `List<T>?` (both genuinely return null). `ToString()`/`default(T)`/`FlattenArrayTree(...)!.ToArray()` use justified `!`.
- IEnumerableExtensions.cs: `CompareTo`/`IsSubsetOf` params -> `IEnumerable<T>?` (methods explicitly null-handle); `ToList` `Action<int>?`; `WithProgressReporting` `Stopwatch? sw`; `CastNullSafe` local `?`, iterator `default(TResult)!`; `SelectGroup` `x.Key!`.
- IListExtensions.cs: `Find<T>` -> `T?`; `TryFindMax` `out T? max`; `CompareTo` params `IList<T>?`; `IsNullOrEmpty(this IList<string>?)`; `Split` params `IList<T>?`/`IEqualityComparer<T>?`; `TryAddRange` params nullable — all reflect existing null handling.
- DictionaryExtensions.cs: `ContentEquals` params `Dictionary<TKey,TValue>?` (uses `?? new()`); `UpdateOrRemove` `out TValue?` (assigns `default`).
- JsonExtensions.cs: `Deserialize<T>` -> `T?` (`... as T` can be null). StringExtensions.cs: `IsNullOrEmpty(this string?)` (matches `string.IsNullOrEmpty`). ImageExtensions.cs: `ToByte` `!` on `ConvertTo`.
- TraceExtensions.cs: reflection returns -> nullable (`GetCallerByName` `MethodBase?`, `GetParameterName`/`TryGetParameterName` `string?`, `GetParameterNames` `string?[]`); logger `!`; `string.IsNullOrEmpty(methodName)` (behavior-identical overload disambiguation).
- WinFormsExtensions.cs: `GetAncestor<T>` (both) -> `T?` (returns null when no ancestor); `IsRegistered(this EventHandler?, ...)` (existing null guard); `GetEventHandlerList` keeps non-null `(EventHandlerList, object)` via `!`. The three PUBLIC `Clone<T>` overloads and `Clone(this RowStyle)`/`Clone(this ColumnStyle)` are UNCHANGED (the downstream #374 dialogs-misc contract), returning non-null `T`.
- DfMLNet.cs / DfDeedle.cs / DfDeedle.FrameUtilities.cs: `GetFirstNonNull` -> `object?`; `FromArray2D` -> `Frame<int,string>?`; `FromDefaultFolder`/`FromDefaultFolderAsync` -> nullable frame returns (all genuinely return null on null/empty input); `DescribeSynchronizationContext(SynchronizationContext?)`; `LogDfTiming(..., string?)`.

Conclusion: All public-signature changes are limited to additive nullability annotations that reflect actual null behavior and are safe cross-module contracts for downstream epic consumers (including #374, whose `Clone<T>` contract is unchanged). AC5 satisfied.
