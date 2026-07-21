# Baseline Coverage-Gap Confirmation (#317)

Timestamp: 2026-07-11T19-53

Command: `rg "LockRecursion" UtilitiesCS.Test/` and cross-reference of `rg "CollectionChanged" UtilitiesCS.Test/`
(files_with_matches) against `rg "DoesNotThrow" UtilitiesCS.Test/` (files_with_matches).

EXIT_CODE: 0

Output Summary:
- `LockRecursion` pattern: 0 matches across `UtilitiesCS.Test/**/*.cs`.
- Combination of `CollectionChanged` and `DoesNotThrow`: 0 matches. The `CollectionChanged` pattern
  matched 10 files; the `DoesNotThrow` pattern matched a disjoint set of 40 files; no file appears in
  both lists, confirming zero files combine the two patterns. This confirms the coverage-gap premise
  from spec.md/research.md prior to restoration.
