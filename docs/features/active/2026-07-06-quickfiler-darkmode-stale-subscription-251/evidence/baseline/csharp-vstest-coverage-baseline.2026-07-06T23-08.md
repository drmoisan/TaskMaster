# Baseline MSTest Coverage — QuickFiler.Test (Issue #251)

Timestamp: 2026-07-06T23-30

Command: vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /EnableCodeCoverage

(Executed as: `vstest.console.exe "QuickFiler.Test/bin/Debug/QuickFiler.Test.dll" /EnableCodeCoverage /InIsolation` in this Git Bash environment per the environment notes in the delegation prompt — `/InIsolation` is required so Moq-based test assemblies load correctly; behavior is otherwise identical to the plan-specified command.)

EXIT_CODE: 0

Output Summary: Total tests: 486. Passed: 486. Failed: 0. Total time: 8.55 seconds. Coverage attachment: `TestResults/acd32e6f-c34e-41a3-9669-7b862050fe97/DanMoisan_MEGALODON4_2026-07-06.23_22_05.coverage`.

Numeric baseline coverage headline: the `.coverage` attachment was converted to Cobertura via `dotnet-coverage merge <file> -f cobertura -o ...` (CodeCoverage.exe's `analyze` verb is non-functional in this VS18 install and only prints usage; `dotnet-coverage merge` is the working conversion path). The converted report is archived at `docs/features/active/2026-07-06-quickfiler-darkmode-stale-subscription-251/evidence/baseline/coverage-xml/baseline-coverage.cobertura.xml`.

- Repo-wide combined line-rate (all instrumented packages, includes vendored/third-party): `lines-covered=22083 / lines-valid=109433` = **20.18%** (`line-rate="0.20179470543620298"` in the report root).
- `QuickFiler` package line-rate (the assembly containing the sole production file touched by this fix): **72.42%** (`line-rate="0.7242424242424242"`, complexity 258).

Per the coverage note in scope: `QfcCollectionController` already carries `[ExcludeFromCodeCoverage]`, so this baseline is recorded for repository-wide no-regression comparison, not as a new coverage floor on the changed lines.
