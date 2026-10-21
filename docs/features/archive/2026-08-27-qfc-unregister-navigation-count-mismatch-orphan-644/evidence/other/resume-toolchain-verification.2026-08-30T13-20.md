# Resume verification — independent re-run of the four C# gates at branch tip

- Timestamp: 2026-08-30T13-20
- Issue: #644
- Branch: `bug/qfc-unregister-navigation-count-mismatch-orphan-644`
- Head verified: `4572fef5`
- Working directory: repository root of a fresh branch worktree, recorded as
  `<REPO_ROOT>`. No absolute host path, account name or machine name is written here.

## Why this artifact exists

The run that produced commit `d7faef54` was interrupted by an API rate limit before it
could open a pull request. Its commit message asserts that the four C# gates passed in one
clean pass with 1254 of 1254 tests passing. That assertion is a claim carried across a
process boundary, so it was re-derived here rather than accepted. Every gate below was
executed by the resuming orchestrator, in a worktree that had never built this branch, at
the current branch tip.

## Worktree bootstrap performed before the first gate

The worktree had neither `.dotnet-sdk` nor `packages/`, so no C# gate could run. Both were
supplied as directory junctions to existing local trees; no tracked file was modified and
nothing was added to the branch footprint.

| Step | Result |
|---|---|
| `.dotnet-sdk` junction | `dotnet --version` reports `8.0.205`, matching the `global.json` pin |
| `packages/` junction | `Meziantou.Analyzer.3.0.156` and `Roslynator.Analyzers.4.16.0` both present, so the unconditional `<Analyzer Include>` items resolve |
| `dotnet tool restore` | `Tool 'csharpier' (version '1.2.6') was restored.` |

## Gate 1 — formatting

- Command: `dotnet tool run csharpier check .`
- EXIT_CODE: 0
- Output Summary: `Checked 1562 files in 5187ms.` No file was reported as needing
  formatting. The exit code was captured on a second, separate invocation to confirm it
  independently of the console text.

## Gate 2 — analyzers

- Command: `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
- EXIT_CODE: 0
- Output Summary: All 16 first-party projects were rebuilt and emitted their assemblies.
  `/t:Rebuild` was used rather than `/t:Build`, so `CoreCompile` ran on every project and
  the gate is not vacuous. The only warnings emitted are the pre-existing
  `System.Reactive.PackagesConfigCheck.targets` `packages.config` notices, which are
  present on `main` and are unrelated to this change.

## Gate 3 — nullable analysis

- Command: `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true`
- EXIT_CODE: 0
- Output Summary: Rebuild completed with zero lines matching `error`. `/p:Nullable=enable`
  was deliberately NOT added: no project in this repository carries a `<Nullable>` element,
  the property is a solution-wide opt-in that conscripts files which never adopted the
  pragma, and `.github/workflows/ci.yml` omits it.

## Gate 4 — tests

- Command: `vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /InIsolation /TestCaseFilter:"TestCategory!=LiveOutlook"`
- EXIT_CODE: 0
- Output Summary:

```
Test Run Successful.
Total tests: 1254
     Passed: 1254
 Total time: 11.8437 Seconds
```

`/InIsolation` and the `TestCategory!=LiveOutlook` filter were both supplied. Omitting the
filter admits a test that requires a live Outlook process and is not a valid local gate.

## Result

The commit-message claim is confirmed against an independently bootstrapped worktree at the
branch tip. One clean pass, all four gates green, 1254 of 1254 tests passing.

## Non-gate observations

- `git status --porcelain` is empty after all four gates, so no gate rewrote a tracked file.
- Coverage was NOT re-measured. AC-16 remains adjudicated as PARTIAL with accepted residual
  risk and is not re-opened by this artifact.
