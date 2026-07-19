# Final Changed-Line Coverage Delta (AC4, P6-T5)

Timestamp: 2026-07-19T05-55

Inputs:
- Baseline coverage: `evidence/baseline/baseline-coverage.cobertura.xml` (repo line 83.7787%, branch 76.3368%).
- Post-change coverage: `evidence/qa-gates/final-coverage.cobertura.xml` (repo line 83.7816%, branch 76.3446%).

Method: For every `UtilitiesCS/Extensions/` file, the per-`<line>` hit data was aggregated across all `<class>` blocks (max hits per line) from both Cobertura XMLs, and covered-line / total-executable-line counts were compared. All 23 remediated files plus the 2 verify-only files were checked.

Per-file executable-line coverage (covered/total) — baseline vs post-change:

| File | Baseline | Final | dCovered |
|---|---|---|---|
| ArrayExtensions.cs | 303/339 | 303/339 | 0 |
| AsyncSerialization.cs | 105/124 | 105/124 | 0 |
| CompilerServicesExtensions.cs | 4/4 | 4/4 | 0 |
| DfDeedle.cs | 225/238 | 225/238 | 0 |
| DfDeedle.FrameUtilities.cs | 117/169 | 117/169 | 0 |
| DfMLNet.cs | 166/171 | 166/171 | 0 |
| DictionaryExtensions.cs | 101/108 | 101/108 | 0 |
| DrawingExtensions.cs | 11/11 | 11/11 | 0 |
| EnumExtensions.cs | 105/114 | 105/114 | 0 |
| ExceptionExtensions.cs | 5/5 | 5/5 | 0 |
| IAsyncEnumerableExtensions.cs | 100/106 | 100/106 | 0 |
| IControlExtensions.cs | 3/3 | 3/3 | 0 |
| IEnumerableExtensions.cs | 246/284 | 246/284 | 0 |
| IListExtensions.cs | 153/164 | 153/164 | 0 |
| ImageExtensions.cs | 36/39 | 36/39 | 0 |
| JsonExtensions.cs | 14/14 | 14/14 | 0 |
| JsonSerializerExtensions.cs | 106/106 | 106/106 | 0 |
| LazyExtension.cs | 21/21 | 21/21 | 0 |
| NullExtensions.cs | 69/69 | 69/69 | 0 |
| QueueExtensions.cs | 6/6 | 6/6 | 0 |
| StreamExtensions.cs | 16/16 | 16/16 | 0 |
| StringExtensions.cs | 41/43 | 41/43 | 0 |
| TraceExtensions.cs | 74/76 | 74/76 | 0 |
| WinFormsExtensions.cs | 229/275 | 229/275 | 0 |
| TOTAL | 2256/2505 | 2256/2505 | 0 |

Result:
- Repo-wide coverage: baseline line 83.7787% / branch 76.3368% -> post-change line 83.7816% / branch 76.3446% (tiny positive deltas within run-to-run instrumentation variance; not a real increase).
- Per changed file: covered-line count AND total-executable-line count are IDENTICAL in baseline and post-change for every Extensions file. Total executable lines are unchanged at 2505, confirming the annotation-only remediation added ZERO new executable lines (all additions were `#nullable enable` pragmas, `?`/`!` annotations on existing lines, and comments — non-executable or same-line). Covered lines unchanged at 2256.

Conclusion: There is NO coverage regression on changed lines (dCovered = 0 for every file). AC4 SATISFIED. Outcome is PASS, not remediation-required.
