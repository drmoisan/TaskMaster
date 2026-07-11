# Phase 6 — Coverage Delta and No-Regression-on-Changed-Lines Verification

Timestamp: 2026-07-10T23-58

## Repo-wide

- Baseline: line-rate 0.7713545978866421 (77.14%), branch-rate 0.5261691301187303 (52.62%);
  lines-valid 142522.
- Post-change: line-rate 0.771175975301712 (77.12%), branch-rate 0.526047976738551 (52.60%);
  lines-valid 142520.
- Delta: -0.018 percentage points. Explained entirely by the denominator shrinking by 2 lines
  (the two deleted `TraceUtility.cs` literal entries, which were previously-covered trivial
  statements). No previously-covered production line became uncovered.

## Affected production packages

| Package | Baseline line-rate | Post-change line-rate | Delta |
|---|---|---|---|
| `QuickFiler` | 0.7255711533168181 (72.56%) | 0.7254335260115607 (72.54%) | -0.014 pp (denominator shrink from removed `using` lines) |
| `UtilitiesCS` | 0.8824581005586593 (88.25%) | 0.882551585429444 (88.26%) | +0.009 pp |

## Affected-module (per-class) coverage

| Class | Baseline line-rate | Post-change line-rate | Baseline branch-rate | Post-change branch-rate | Verdict |
|---|---|---|---|---|---|
| `QuickFiler.Controllers.KbdActions<TKey, UClass, VDelegate>` | 0.9397590361445783 (93.98%) | 0.9397590361445783 (93.98%) | 1 (100%) | 1 (100%) | Identical — the re-typed field and both constructor bodies remain fully exercised by the existing `KbdActions` tests. |
| `UtilitiesCS.TraceUtility` | 0.900709219858156 (90.07%) | 0.9 (90.00%) | 0.8076923076923077 (80.77%) | 0.8076923076923077 (80.77%) | Denominator shrink only (2 deleted, previously-covered dead literal lines removed from both numerator and denominator); no line that was executable and covered before is now uncovered. |
| `UtilitiesCS.FlagDetails` | 1 (100%) | 1 (100%) | 0.9583333333333334 (95.83%) | 0.9583333333333334 (95.83%) | Identical. |
| `UtilitiesCS.EmailIntelligence.FolderRemap.FolderRemapController` | 0.875 (87.5%) | 0.875 (87.5%) | 0.7380952380952381 (73.81%) | 0.7380952380952381 (73.81%) | Identical. |
| `QuickFiler.Controllers.KeyboardHandler` | Not present as a distinct `<class>` entry (no measurable executable-line data) | Not present as a distinct `<class>` entry (no measurable executable-line data) | n/a | n/a | Unchanged absence; the `using` removal is verified by rebuild success (Phase 2), not by a coverage delta, per the plan's own acceptance criterion for Phase 2. |

## No regression on changed lines: PASS

All executable-line changes in this feature are (a) a field/constructor re-typing in
`KbdActions.cs` that remains at identical coverage, or (b) pure deletions of dead, previously-
covered code (`TraceUtility.cs` literals) or non-executable `using` directives
(`KeyboardHandler.cs`, `FlagDetails.cs`, `FolderRemapController.cs`, `KbdActions.cs`). No line
that was covered before this change is uncovered after it. Repository-wide and per-package
coverage remain within rounding noise of the baseline (deltas explained entirely by denominator
shrink from deleted dead code, not by any newly-uncovered line). Outcome: PASS, no remediation
required.
