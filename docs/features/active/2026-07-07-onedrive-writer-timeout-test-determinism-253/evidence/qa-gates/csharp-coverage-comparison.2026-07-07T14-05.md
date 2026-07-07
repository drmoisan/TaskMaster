# C# Coverage Comparison (Issue #253)

Timestamp: 2026-07-07T16-58

## Sources

- Baseline: `docs/features/active/2026-07-07-onedrive-writer-timeout-test-determinism-253/evidence/baseline/csharp-vstest-coverage-baseline.2026-07-07T14-05.md`
- Targeted (Phase 1, class-scoped run): `docs/features/active/2026-07-07-onedrive-writer-timeout-test-determinism-253/evidence/regression-testing/targeted-vstest-coverage.2026-07-07T14-05.md`
- Final (post-change full suite): `docs/features/active/2026-07-07-onedrive-writer-timeout-test-determinism-253/evidence/qa-gates/csharp-vstest-coverage-final.2026-07-07T14-05.md`

## Numeric Comparison

| Metric | Baseline | Targeted (class-scoped) | Final |
|---|---|---|---|
| Repository-wide `line-rate` (all modules) | 60.23% | 1.24% (not meaningful — only 9/4170 tests ran) | 60.25% |
| `UtilitiesCS` package `line-rate` | 87.98% | n/a (class-level cited instead) | 87.99% |
| `UtilitiesCS.OneDriveHelpers.OneDriveDownloader` class `line-rate` | 100% | 100% | 100% |

## No-Regression Confirmation

- Repository-wide (baseline 60.23% -> final 60.25%): no regression; a small increase, attributable to the additional 18 valid lines / 57 covered lines introduced by the new `WriterTimeoutRunner` property and the modified call site, all fully exercised.
- `UtilitiesCS` package (baseline 87.98% -> final 87.99%): no regression.
- `OneDriveDownloader` class (100% at baseline, targeted run, and final): unchanged; already fully covered before this change and remains fully covered after.

## Changed-Lines Coverage Confirmation

The lines changed by this plan in `OneDriveDownloader.cs` are:
- The new `WriterTimeoutRunner` property (getter/setter) and its backing field `_writerTimeoutRunner` with the default lambda.
- The modified call site inside `TryGetFileStreamWriter` (`var stream = await WriterTimeoutRunner(GetFileStreamWriter, destinationPath, cancel, timeoutMs);`).

Every existing call to `TryGetFileStreamWriter` (production callers via `DownloadFileAsync`, and every `OneDriveDownloader_Tests`/`TestableOneDriveDownloader` test) routes through this call site, and the default `WriterTimeoutRunner` delegate is exercised whenever a test does not override it (e.g., `GetFileStreamWriter_DefaultWriterWithNulPath_ThrowsNotSupportedException`, which uses the real `OneDriveDownloader` with its default `GetFileStreamWriter`, is unrelated to the timeout runner directly but confirms the class's default-path wiring remains intact). The rewritten `TryGetFileStreamWriter_WhenWriterReturnsMemoryStream_ReturnsStream` and `TryGetFileStreamWriter_WhenWriterThrows_ReturnsNull` tests exercise the test-injected `WriterTimeoutRunner` path. The `UtilitiesCS.OneDriveHelpers.OneDriveDownloader` class's 100% line-rate in both the targeted (Phase 1) and final (Phase 2) coverage runs confirms both the new property and the modified call site are exercised — no regression on changed lines, consistent with the `>= 90%` new-code coverage requirement (`.claude/rules/csharp.md`, C#/general unit test policy).

## Output Summary

No repository-wide coverage regression (60.23% -> 60.25%), no `UtilitiesCS` package regression (87.98% -> 87.99%), and no regression on the changed lines in `OneDriveDownloader.cs` (100% class line-rate in both baseline and final runs, confirming both the new `WriterTimeoutRunner` property and the modified call site are exercised). Satisfies the coverage portion of AC5.
