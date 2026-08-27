# Clean Toolchain Pass ([P4-T7])

Timestamp: 2026-08-27T23-22

Command:

```
md5sum <the six touched files>                      # before [P4-T1] format, after [P4-T1] format, and after [P4-T4]
git status --porcelain -- '*.cs' '*.csproj'         # after [P4-T4]
```

(run from the workspace root; the four gate commands themselves are recorded in their own artifacts,
listed below)

EXIT_CODE: 0

## The four steps in the mandated order

| # | Step | Artifact | Recorded EXIT_CODE |
| --- | --- | --- | --- |
| 1 | Formatting | `evidence/qa-gates/qa-1-csharpier.2026-08-27T23-13.md` | 0 |
| 2 | Linting / analyzers | `evidence/qa-gates/qa-2-analyzers-rebuild.2026-08-27T23-14.md` | 0 |
| 3 | Type checking / nullable | `evidence/qa-gates/qa-3-nullable-rebuild.2026-08-27T23-15.md` | 0 |
| 4 | Tests with coverage | `evidence/qa-gates/qa-4-tests-coverage.2026-08-27T23-17.md` | 0 |

All four recorded `EXIT_CODE: 0`. The narrow exception the acceptance permits, a non-zero test-step
exit accepted under the `[P4-T4]` baseline-comparison clause, was not needed: the test run reported
`Failed: 0` and exited 0, so that clause was never invoked and no pre-existing failure had to be
excused.

## Output Summary

- The four steps ran consecutively at 23-13, 23-14, 23-15 and 23-17 UTC on 2026-08-27, in the
  mandated order formatter, analyzers, type check, tests, with no other work interleaved.
- **No file was rewritten between the formatter and the test run.** MD5 digests of all six touched
  files were taken immediately before the `[P4-T1]` apply, immediately after it, and again after
  `[P4-T4]`. All three readings are identical:

  | File | MD5 (unchanged across all three readings) |
  | --- | --- |
  | `QuickFiler/Viewers/WebView2BreadcrumbHost.cs` | `a5518a12e4c658312c1b0ab872deeb69` |
  | `QuickFiler/Viewers/WebView2CoreInitializer.cs` | `28d26a87e1a3547eee1eb515117b046a` |
  | `QuickFiler/Viewers/IWebViewCoreInitializer.cs` | `53670e8b264cbe62c83d5853457e2290` |
  | `QuickFiler.Test/Controllers/WebView2CoreInitializerTests.cs` | `63855204f62c0c282de939000215424d` |
  | `QuickFiler.Test/Viewers/WebView2BreadcrumbHostTests.cs` | `df41a874bffc5881d3a21c6a463beceb` |
  | `QuickFiler.Test/Viewers/WebView2BreadcrumbHostContractTests.cs` | `af8ba41a6e78da2c3cdf752696810a96` |

- `git status --porcelain -- '*.cs' '*.csproj'` taken after `[P4-T4]` printed nothing, so no C#
  source file and no project file in the worktree differs from `HEAD`. The loop therefore did not
  need to restart, and this is one consecutive clean pass.
- Non-vacuity of the two msbuild steps was measured, not assumed: both used `/t:Rebuild`, and both
  logs contain **zero** `Skipping target "CoreCompile"` occurrences against 36 `csc.exe` invocations
  each. A warm `/t:Build` would have exited 0 with `CoreCompile` skipped on every project and would
  have run no analyzers; that did not happen here.

## Restart history for this phase

This is the second execution of Phase 4. The first, recorded at 2026-08-27T20-49 through T20-51,
covered only steps 1 through 3 and predates the merge of the integration base at `9cb2c4f6`
(`evidence/qa-gates/base-merge-reconciliation.2026-08-27T23-09.md`). Those artifacts no longer
describe the tree under test and were left on disk as history rather than reused; the phase was
restarted from `[P4-T1]` and every gate above was re-run from a clean start against the merged tree.

## Finding carried forward

This artifact records the toolchain loop only. The separate coverage-threshold comparison in
`evidence/qa-gates/coverage-delta.2026-08-27T23-20.md` records one gate that is **not met**: the
90% line-coverage floor on newly measured members falls short for `NavigateToString` (62.50%),
`DetachCore` (66.67%), `CreateEnvironmentAsync` (83.33%) and `EnsureCoreWebView2Async` (66.67%).
That is a coverage finding, not a toolchain-loop failure, and it does not alter the four exit codes
above. It is carried into the Phase 5 status summary and reported to the orchestrator.
