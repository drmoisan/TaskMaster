---
name: project-751-sync-barrier-revision-seams
description: Issue #751 (terminal-hook sync barrier) round-2 seams — research undercounted the awaiting call sites, the reviewer's .gitignore and restore claims were partly wrong, and a numeric coverage pair is obtainable from vstest .coverage attachments without the repo coverage script
metadata:
  type: project
---

Round-2 preflight revision of the #751 plan surfaced four facts that a later plan in this
area should re-derive rather than inherit.

**Why:** two of the reviewer's own citations and one research-record figure were wrong, and a
planner that trusted any of them would have written a false claim into a committed artifact.

**How to apply:** when planning in `TaskMaster.Test/AppGlobals/AppOlObjectsFolderTreeService*`
or authoring any C# coverage-evidence task in this repo.

1. **Research §1's "six sibling call sites await `run.Terminal`" is an undercount.** The true
   figure is seven. `Select-String -Path 'TaskMaster.Test\AppGlobals\*.cs' -Pattern '\.Terminal\b'`
   returns 7 lines; research §1 enumerates six and omits
   `AppOlObjectsFolderTreeServiceLifecycleTests.cs:118` (`VerifyBlockingDisposalAsync`), which is
   the same shape as the `:38` site it does list. The class is `partial` across three files
   (`...Tests.cs`, `...LifecycleTests.cs`, `...LifecycleTests.Coverage.cs`), so any "only test in
   its class" claim must be qualified. See [[verify-test-provenance-before-planning-deletion]].

2. **`.gitignore` reality (re-derived, contradicts a reviewer claim):** `[Tt]est[Rr]esult*/`
   at `:39` DOES ignore `TestResults/`; `*.coverage` at `:140` is ignored globally; `coverage/*`
   at `:144` and `logs/` at `:348` are ignored. `*.trx` has **no** ignore rule anywhere, so a TRX
   written outside an ignored directory shows in porcelain. See
   [[gitignore-bracket-classes-defeat-literal-grep]].

3. **`packages/` does not exist in a fresh worktree and `msbuild /t:Restore` will not create it.**
   Every project is `packages.config`-style, so the CI-parity command is
   `nuget restore TaskMaster.sln` (`.github/workflows/_mstest-coverage.yml:61`); fallback is
   `/t:Restore /m /p:RestorePackagesConfig=true`. Gate on `(Test-Path 'packages')` plus a
   subdirectory count, not on exit code — both restore commands exit 0 without creating it.
   The tool manifest is `dotnet-tools.json` at the **repo root**, not under `.config/`.
   `Directory.Build.props` DOES exist at root and sets only `RxUseUnsupportedPackagesConfig`;
   "there is no Directory.Build.props" is false.

4. **A numeric coverage pair is obtainable without `Invoke-MSTestWithCoverage.ps1`.** That script
   throws at `:235-237` before the post-processing at `:339-342` whenever the inner run exits
   non-zero, so on a suite with pre-existing failures it yields no figure — but that limitation is
   scoped to the script route. Every `/EnableCodeCoverage` vstest run writes a `.coverage`
   attachment under its `/ResultsDirectory` regardless of test outcomes. Convert it with
   `dotnet-coverage merge <f> --output <o> --output-format cobertura` (rung 1) or the vswhere-resolved
   `Team Tools\Dynamic Code Coverage Tools\CodeCoverage.exe analyze` (rung 2), and read
   `lines-covered`/`lines-valid`. This is the proportionate way to satisfy the Coverage Evidence
   Contract on a test-only change; a plan may not self-grant a no-numeric-value PASS waiver.
   See [[deletion-adjusted-coverage-no-regression-gate]].
