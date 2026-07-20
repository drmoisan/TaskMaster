# Final QC — No Post-Condition Attributes / No Polyfill (P9-T7)

Timestamp: 2026-07-19T22-03

## Commands

1. `grep -rE "NotNullWhen|MaybeNullWhen|NotNullIfNotNull|\[MaybeNull\]|\[AllowNull\]|\[DisallowNull\]|DoesNotReturn|MemberNotNull" UtilitiesCS/ReusableTypeClasses <4 waiver NewtonsoftHelpers files>`
2. `grep -rn "namespace System.Diagnostics.CodeAnalysis" UtilitiesCS/ReusableTypeClasses`

## Results

- Prohibited nullable post-condition attribute usages in the remediated cluster (51 ReusableTypeClasses
  files + the four NewtonsoftHelpers waiver consumers): **0**.
- `System.Diagnostics.CodeAnalysis` polyfill namespace declarations introduced by this feature: **0**.

Zero prohibited attributes (`NotNullWhen`, `MaybeNullWhen`, `NotNullIfNotNull`, `MaybeNull`,
`AllowNull`, `DisallowNull`, `DoesNotReturn`, `MemberNotNull`) and no polyfill were added. Null-state
was expressed exclusively with plain `?`, `where TKey : notnull`, unconstrained `T?` / `out TValue?`,
guard clauses, and justified `!` — consistent with the net481 / C# 12 constraint that these
attributes are not available or polyfilled.
