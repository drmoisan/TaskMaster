# P5-T10 — Banned-Construct Audit of the Two Changed Test Files

Timestamp: 2026-08-31T20-14
Command: for each of the seven audited tokens, count `[regex]::Matches` of the escaped literal against every line of each file
EXIT_CODE: 0

## Per-token count table

| Token | `UtilitiesCS.Test/HelperClasses/FileIO2_Tests.cs` | `QuickFiler.Test/Controllers/QfcHomeControllerMetricsTests.cs` | Total |
|---|---|---|---|
| `Thread.Sleep` | 0 | 0 | 0 |
| `Task.Delay` | 0 | 0 | 0 |
| `GetTempPath` | 0 | 0 | 0 |
| `CreateDirectory` | 0 | 0 | 0 |
| `File.Create` | 0 | 0 | 0 |
| `File.WriteAllText` | 0 | 0 | 0 |
| `new FileStream(` | 0 | 0 | 0 |

Across both changed test files the occurrence count is 0 for every one of the seven audited tokens.

## What each zero establishes

- **`Thread.Sleep` and `Task.Delay` at 0** satisfy the banned-API rule in `.claude/rules/general-unit-test.md`, which prohibits real wall-clock waits in test code. Every timing-dependent branch in the six new tests is driven through the injected delay delegate, which returns `Task.CompletedTask`. The retry-exhaustion test drives 99 delay iterations and completes in 2 milliseconds; against the production default the same 99 iterations take approximately 9.9 seconds.
- **`GetTempPath`, `CreateDirectory`, `File.Create` and `File.WriteAllText` at 0** satisfy the General Unit Test Policy prohibition on creating files, directories or temporary paths in tests, for which the repository records no approved exception. The success-path test writes into an in-memory `StringWriter`, which is why the writer factory is typed `Func<string, TextWriter>` rather than `Func<string, StreamWriter>`.
- **`new FileStream(` at 0** records that the exclusive lock on the shared source-tree fixture is gone. Before this change, `UtilitiesCS.Test/HelperClasses/FileIO2_Tests.cs` line 35 held `UtilitiesCS.Test/TestData/FileIO2/sample.csv` open with `FileShare.None` for the full retry window. That fixture's exact contents are asserted by a sibling test in the same class, and `WriteTextFileAsync` opens in append mode, so a write that ever succeeded would have appended to the fixture and broken the sibling permanently. The suite was safe only because the write was guaranteed to fail. That hazard is now retired rather than merely tolerated.

The two remaining fixture-reading tests in `FileIO2_Tests` still read `sample.csv`, but read-only and without a lock, which is the pre-existing pattern this change does not alter.

Output Summary: All seven audited tokens count 0 across both changed test files. This artifact is the evidence P7-T18 reads to verify AC18.
