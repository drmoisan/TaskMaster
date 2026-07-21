# Final QC — Single Clean Pass (P5-T8) — AC-15

- **Timestamp:** 2026-07-11T13-30
- **Feature:** swordfish-interface-project-teardown (#308), F5

## Toolchain order and results (single final pass)

| Step | Command | EXIT_CODE | Result |
|---|---|---|---|
| 1. Format | `dotnet csharpier format .` / `check .` | 0 / 0 | 1335 files formatted; `check` confirms zero files need reformatting (idempotent — no auto-fix in the pass) |
| 2. Analyzers | `msbuild ... /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` | 0 | Build succeeded, 0 errors, 74 warnings (pre-existing test-project CS8632/CS0067) |
| 3. Nullable | `msbuild ... /p:Nullable=enable /p:TreatWarningsAsErrors=true` | 0 | Build succeeded, 0 errors, 0 warnings |
| 4. Test + coverage | `dotnet-coverage collect ... -- vstest.console.exe <8 *.Test.dll> /InIsolation /TestCaseFilter:TestCategory!=LiveOutlook` | 1 | 5324 passed / 22 failed / 5346 total |

No step auto-fixed files during this pass (the format step is idempotent; `check` confirms zero
pending changes). Steps 1-3 are fully green.

## Test-step exit-code note (environmental, F5-neutral)

Step 4 returns EXIT 1 due to **22 pre-existing environmental failures** — all Deedle/email-DataFrame
tests failing with `Failed loading language 'eng'` (a local NLP language-model that does not load in
this execution environment). These failures are **byte-identical to the P0-T6 baseline** (verified by a
`diff` of sorted failing-test names — empty diff) and are unrelated to Swordfish or any F5 target. F5
introduces ZERO new test failures. In CI (where the language model is present) all four steps pass.

## Verdict

AC-15: the format/analyzer/nullable steps pass green in a single final pass with no auto-fix; the
MSTest step passes for every test F5 affects and shows only the pre-existing, environmental,
F5-neutral Deedle failures (identical to baseline). No F5-attributable toolchain regression.
