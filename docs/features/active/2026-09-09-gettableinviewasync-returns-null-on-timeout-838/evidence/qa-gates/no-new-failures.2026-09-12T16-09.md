# P4-T7 — No-new-failures comparison against the Phase 0 baseline

Timestamp: 2026-09-13T03-16

Command: a single pwsh payload that reads the declared `BASELINE-FAILED:` row from the P0-T20 artifact and the declared `FINAL-FAILED:` row from the P4-T5 artifact, computes the set difference in both directions, and prints `NEWLY-FAILING:` and `NO-LONGER-FAILING:`.

A first attempt at this extraction matched the two tokens where they also occur inside each artifact's prose `Output Summary:`, which produced a nonsense difference and exited 1. The extraction was corrected to select only the declared row, the line that begins with the token, and both artifacts were confirmed to carry exactly one such row each. The corrected extraction is the one recorded here. The underlying data was never in doubt: both declared rows read `NONE`.

EXIT_CODE: 0

```
BASELINE_DECLARED_ROWS=1
BASELINE-FAILED: NONE
FINAL_DECLARED_ROWS=1
FINAL-FAILED: NONE
NEWLY-FAILING: NONE
NO-LONGER-FAILING: NONE
```

## Comparability statement

Both sides were produced with the identical test-case filter and the identical assembly-discovery rule, which the plan's test-population section fixes once and which both P0-T20 and P4-T5 used without variation, including the `[char]92` construction of the four discovery patterns. Both runs discovered 9 assemblies. A figure produced under a different filter or a different discovery rule would not be comparable to either side, which is why the plan fixes both once.

Output Summary: `NEWLY-FAILING: NONE`, so the acceptance clause holds directly and the named exception for the issue-780 flake was not needed: `TryAddValuesAsync_UpdatesExistingValue` failed in neither run, so `ISSUE-780-FLAKE-OBSERVED:` is not recorded. `NO-LONGER-FAILING: NONE` as well, since the baseline had no failing test to begin with. The comparison is between two fully green runs whose totals differ by exactly the five tests this change adds, so no existing test changed outcome.
