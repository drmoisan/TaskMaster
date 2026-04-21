# P2-T5 — Coverage Comparison: Baseline vs Post-Fix

Timestamp: 2026-04-21T16:06:00Z

## Coverage Comparison

Baseline Coverage: 78.20%
Post-fix Coverage: 78.21%
Delta: +0.01 pp (no regression)

## Test Count Comparison

Baseline tests: 3943 (3941 passed, 2 skipped)
Post-fix tests: 3945 (3943 passed, 2 skipped)
Test count delta: +2 (2 new regression tests added in Phase 1)

## Affected Production Files

New/changed production files: `UtilitiesCS/EmailIntelligence/ClassifierGroups/Triage/Triage_OlLogic.cs`

### File-level and package-level coverage (post-fix)

- `Triage_OlLogic.cs` (UtilitiesCS.EmailIntelligence.ClassifierGroups.Triage_OlLogic): 70%
  - This 70% is pre-existing file-level coverage, not a regression from the fix.
  - The fix added exactly 1 new production line: `.Take(1)` in `TrainSelectionAsync`.
  - That new line is executed in every `TrainSelectionAsync` regression test; new-code coverage is 100%.
- UtilitiesCS package overall: 87.23%

## Coverage Threshold Met

Coverage Threshold Met: yes

**Rationale:**
1. Overall line coverage threshold "≥ 80% or no regression": the no-regression condition is met (78.21% ≥ 78.20% baseline).
2. New UtilitiesCS code ≥ 90%: the single new production line (`.Take(1)` + comment) is 100% covered by all three `TrainSelectionAsync` regression tests. The file-level 70% is attributable to pre-existing untested methods in `Triage_OlLogic.cs`, not to the code introduced by this fix.
