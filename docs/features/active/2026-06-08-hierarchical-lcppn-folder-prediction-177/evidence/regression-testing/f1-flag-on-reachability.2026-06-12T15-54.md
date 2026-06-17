# F1 Flag-On Reachability + AC13 Flag-Off Preservation — Regression Evidence (#177 Cycle 1)

- Timestamp: 2026-06-12T17-20 (UTC)
- Task: [P3-T3]
- Test file: `UtilitiesCS.Test/EmailIntelligence/FolderPredictorSeam_Tests.cs`
- Command: `vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /InIsolation /TestCaseFilter:"FullyQualifiedName~FolderPredictorSeam_Tests"`

## F1 — flag-on reachability through a fresh per-call instance (P1-T7)

- Test: `GetFolderPredictorAsync_FlagOn_ReachableThroughFreshPerCallInstance`
- Result: **PASS**
- What it proves: the flag-on LCPPN predictor held on the shared `Globals.AF.FolderPredictor` holder
  is returned by two independent `new OlFolderClassifierGroup(globals)` instances (the production
  per-call construction pattern used by `EmailFiler`, `SortEmail`, `FolderScorer`), not only by the
  build-time instance. Both fresh instances resolve the same `LcppnFolderPredictor`. This closes the
  F1 Major finding (flag-on path previously unreachable because the predictor was per-instance state).

## AC13 — flag-off behavior unchanged through a fresh per-call instance (P1-T8)

- Test: `GetFolderPredictorAsync_FlagOff_FreshPerCallInstance_ReturnsFlat`
- Result: **PASS**
- What it proves: with `UseLcppnPredictor` off (default) and `Globals.AF.FolderPredictor` null, a fresh
  per-call instance returns the exact flat `Manager["Folder"]` `BayesianClassifierGroup` instance,
  byte-for-byte unchanged. AC13 (backward compatibility / flat predictor) is preserved.

## Full seam suite

All 8 `FolderPredictorSeam_Tests` passed (8/8), including the four pre-existing AC13/AC14 tests that now
route `SetLcppnPredictor` through the shared holder and the two new F1 regression tests above.

## Outcome

- Flag-on reachability: PASS.
- Flag-off (AC13) unchanged: PASS.
