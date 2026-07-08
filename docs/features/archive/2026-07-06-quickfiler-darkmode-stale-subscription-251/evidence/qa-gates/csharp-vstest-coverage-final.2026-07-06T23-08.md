# Final Full-Suite MSTest Coverage — QuickFiler.Test (Issue #251)

Timestamp: 2026-07-07T00-05

Command: vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /EnableCodeCoverage

(Executed as: `vstest.console.exe "QuickFiler.Test/bin/Debug/QuickFiler.Test.dll" /EnableCodeCoverage /InIsolation` in this Git Bash environment per environment notes.)

EXIT_CODE: 0

Output Summary: Total tests: 488. Passed: 488. Failed: 0. Total time: 8.35 seconds. Coverage attachment: `TestResults/a795de02-37e9-43c0-af26-1fde57a37e47/DanMoisan_MEGALODON4_2026-07-06.23_36_12.coverage`. Test count increased from the baseline's 486 to 488 (the two new `QfcCollectionControllerDarkModeTests` regression tests); no other test count change.

Numeric post-change coverage headline: converted via `dotnet-coverage merge <file> -f cobertura -o ...`. Archived at `docs/features/active/2026-07-06-quickfiler-darkmode-stale-subscription-251/evidence/qa-gates/coverage-xml/final-coverage.cobertura.xml`.

- Repo-wide combined line-rate (all instrumented packages, includes vendored/third-party): `lines-covered=22150 / lines-valid=109500` = **20.23%** (`line-rate="0.20228310502283106"`).
- `QuickFiler` package line-rate: **72.42%** (`line-rate="0.7242424242424242"`, complexity 258) — bit-for-bit identical to the baseline value, confirming zero coverage impact from the issue #251 change, consistent with `QfcCollectionController` carrying `[ExcludeFromCodeCoverage]` (the touched class and its complexity count are unchanged from baseline).
