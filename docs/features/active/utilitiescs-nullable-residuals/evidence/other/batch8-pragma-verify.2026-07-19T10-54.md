# Batch 8 Pragma Verification (P9-T3)

Timestamp: 2026-07-19T10-54

Batch 8 opted-in files (2, large COM helpers; both pre-existing >500-line breaches, NOT split — AC8):
1. UtilitiesCS/OutlookObjects/Fields/UserDefinedFields.cs (725 lines) — `SafeGetPropertyAccessorValue`/
   `TryGetProperty`/`GetUdfValue` returns → `object?`; `GetUdfString` (all overloads) → `string?`;
   `GetUdfValue<T>` and `TryGetProperty<T>` → `T?`; `UserProperty? objProperty`; obsolete `object`
   overloads' returns aligned. Justified `!` at guarded COM sites (`property!.Value!` inside the
   `property != null && property.Value != null` guard) and the null-safe extension `errors!.IsNullOrEmpty()`.
2. UtilitiesCS/OutlookObjects/AppointmentItem/MeetingItemHelper.cs (849 lines) — top-level `#nullable enable`
   added; the pre-existing inline `#nullable enable`/`#nullable disable` island around `_emailHeader`
   removed as redundant (whole file now enabled; `_emailHeader` was already `string?`). 35 ctor-unset
   `Lazy<...>`/COM fields → `= null!`; `Sw` → `SegmentStopWatch?`; `PropertyChanged` event → `?`;
   `_html = null` → `= null!`. 14 `?.Value` getters wrapped `(_xxx?.Value)!` and 4 Lazy-factory lambda
   returns wrapped `(...)!` — behavior-preserving (compile-time no-ops returning the same value, null
   included) that keep the public property contracts non-null to match the oblivious `IItemInfo` and
   avoid cascading nullability into many internal consumers.

## Trustworthy isolated CS86xx gate

Command: `msbuild UtilitiesCS/UtilitiesCS.csproj -t:Rebuild -p:Configuration=Debug -p:Platform=AnyCPU -p:TreatWarningsAsErrors=true -p:WarningsNotAsErrors=CS0649;CS0618;CS0168 -p:BuildProjectReferences=false`

EXIT_CODE: 0

Output Summary: Build succeeded. 0 errors, 0 CS86xx, 15 pre-existing out-of-scope warnings. First
passes surfaced UserDefinedFields cascade (CS8600/CS8602/CS8603/CS8604) and MeetingItemHelper's
218 CS8618 + 36 CS8603 + 2 CS8625; all resolved annotation-only as above. Neither file was split;
both remain over 500 lines as they were before any edit (AC8).

## Deviations from the plan's suggested annotation
- UserDefinedFields: `TryGetProperty<T>` return also made `T?` (plan named only `GetUdfValue<T>`); it has
  `return default(T)` for unconstrained T, so `T?` was mechanically required. `property!.Value!` inside
  the null-guard was required because the compiler does not narrow the non-null COM property through the
  compound `&&` guard (contrary to the plan's "COM chains need no `!`" expectation for this one site).
- MeetingItemHelper: used behavior-preserving `(...)!` on getters/lambdas rather than widening ~14 public
  properties to nullable, to keep the `IItemInfo`-matching public contracts stable and avoid a large
  cascade of CS8602 into internal consumers. This is the plan's sanctioned "(or justified `!`)" path.
