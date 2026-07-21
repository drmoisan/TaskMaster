# Final QC — Coverage Delta / No-Regression Verification

Timestamp: 2026-07-11T12-02

Measurement method: UtilitiesCS.Test run with `/EnableCodeCoverage`, `.coverage` attachment merged to Cobertura via `dotnet-coverage merge -f cobertura`, whole-attachment line-rate read from the `<coverage>` root element. Same method used for both baseline and post-change for an apples-to-apples comparison.

- Baseline coverage: 60.54% line (line-rate 0.6054016; 98382 / 162507 lines) — P0-T9.
- Post-change coverage: 60.20% line (line-rate 0.6019975; 97464 / 161901 lines) — P5-T4.
- Changed/new-code coverage: N/A / not reduced. This change adds ZERO new production lines. The only production edits are: (a) deletion of `SCODictionary.cs` (removes lines from both numerator and denominator), and (b) a comment-only edit in `FolderScorer.cs` (no executable line changed). Test-side edits are type-swaps and deletions. Because no executable production line was added or modified, there is no changed production line whose coverage could regress; changed-line coverage is therefore not reduced.

Delta analysis: whole-attachment line coverage moved -0.34 pp (60.54% -> 60.20%). This is an expected incidental side effect of removing a `Sco*` wrapper and its dedicated tests: the deleted `SCODictionary_Tests.cs` / `SCODictionary_Additional_Tests.cs` incidentally exercised the vendored `ConcurrentObservableDictionary`/Swordfish base, whose lines remain in the whole-attachment denominator but are no longer hit by UtilitiesCS.Test. That vendored base is covered by the separate, out-of-scope `UtilitiesSwordfish.Test` assembly. The retargeted SmartSerializable tests retain their existing coverage of the generic serialization infrastructure (now via `ScoDictionaryNew`/`ConcurrentObservableCollection`).

Verdict: PASS. No coverage regression on changed lines (zero changed production lines). The -0.34 pp whole-attachment movement is a known, non-blocking incidental effect of net code removal, not a regression on any line this change authored. Test suite: 4223/4223 passed, 0 failed (no test regression).
