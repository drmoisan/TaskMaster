# Atomic Implementation Plan — quickfiler-test-uithread-dispatcher (#493)

- Feature folder (`<FEATURE>`): `docs/features/active/quickfiler-test-uithread-dispatcher-493`
- Work Mode: `full-bug` — `spec.md` is the sole acceptance-criteria source (AC-1 … AC-10)
- Requirements: `<FEATURE>/spec.md`; constraints: `<FEATURE>/issue.md`
- Design source: `<FEATURE>/research/2026-08-24T11-05-uithread-dispatcher-restore-scope-research.md`
- Epic: `quickfiler-bug-family`; integration branch `epic/quickfiler-bug-family-integration`

## Conventions

These conventions are binding on every task below. They are stated here once and are never
restated inside a task.

- **`WS`** — the workspace root, resolved at execution time by `git rev-parse --show-toplevel`.
  Every path in this plan is repo-relative and is joined to `WS` at execution time. No absolute
  host path, account name, or machine name may be written into this plan or into any artifact it
  instructs the executor to produce.
- **`TS`** — an ISO-8601 timestamp in the form `yyyy-MM-ddTHH-mm`, captured per task.
- **Citing another task's artifact.** Every artifact in this plan has a unique stem. When a task
  cites an artifact produced by an earlier task, it resolves the single file matching
  `<stem>.*.md` under the stated evidence kind, and records the resolved filename verbatim in its
  own artifact. `<TS>` inside a citation denotes the **producing** task's timestamp, never the
  citing task's; a citation is never resolved by substituting the citing task's own `TS`.
- **`FEATURE`** — `docs/features/active/quickfiler-test-uithread-dispatcher-493`.
- **Evidence artifacts** live under `<FEATURE>/evidence/<kind>/` where `<kind>` is one of
  `baseline`, `regression-testing`, `qa-gates`, `issue-updates`, `other`. No `artifacts/` path is
  a valid evidence location for this plan.
- **Every command-bearing task** writes a Markdown evidence artifact carrying, at minimum:
  `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:`. A task whose command is expected to
  fail additionally carries `ExpectedExitCode:`.
- **Raw tool output** (msbuild logs, TRX files, `.coverage` files, Cobertura XML) is written to
  `TestResults/plan-logs/<lower-case-task-id>/` under `WS`. That tree is git-ignored by the
  `.gitignore` pattern for test-result directories, so raw output never enters a commit and never
  leaks a host path into a tracked file. Evidence artifacts quote only redacted excerpts.
- **Redaction filter.** Before any text derived from tool output is written into an evidence
  artifact it is passed through these three replacements, in this order:

  ```powershell
      $redact = {
          param($line)
          $line -replace [regex]::Escape($WS), '<repo-root>' `
                -replace [regex]::Escape($env:USERPROFILE), '<user-profile>' `
                -replace [regex]::Escape($env:COMPUTERNAME), '<host>'
      }
  ```

- **Long-running commands.** The msbuild, vstest, and coverage tasks may exceed a single
  foreground tool budget. Launch them with `Start-Process -PassThru -NoNewWindow`
  `-RedirectStandardOutput` / `-RedirectStandardError` into the task's `plan-logs` directory,
  record the PID, poll the process object, and take `EXIT_CODE:` from `$proc.ExitCode`. Before any
  retry, terminate the whole tree (the runner shell, `vstest.console`, every `testhost` process,
  and `dotnet-coverage`).
- **Toolchain resolution.** `msbuild` and `vstest.console.exe` are not on `PATH` under
  `pwsh -NoProfile`. Both are resolved through `vswhere` at
  `Join-Path ${env:ProgramFiles(x86)} 'Microsoft Visual Studio\Installer\vswhere.exe'` and are
  recorded in the `P0-T2` artifact as `$MSBUILD` and `$VSTEST`. Later tasks invoke the recorded
  paths with `&`.
- **Git gates are always pathspec-scoped.** `.claude/agent-memory/` is tracked and is written by
  agents while this plan executes, so no unscoped `git status`, `git diff`, or diff-grep gate is
  used anywhere in this plan.

## Scope Lock

Files this plan may create or modify:

| Path | Disposition |
| --- | --- |
| `QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixture.cs` | new |
| `QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixtureTests.cs` | new |
| `QuickFiler.Test/Controllers/QfcItemController.TestSupport.cs` | modified |
| `QuickFiler.Test/Controllers/QfcItemController.InitializationTests.Part2.cs` | modified |
| `QuickFiler.Test/QuickFiler.Test.csproj` | modified — two `<Compile Include>` entries only |
| `<FEATURE>` and its evidence tree | plan, evidence, spec check-offs |

Files this plan must not write, under any circumstance: any QuickFiler production source;
`QuickFiler.Test/Controllers/QfcItemController.FocusAndThemeTests.cs` (sibling-owned);
`UtilitiesCS/Threading/UiThread.cs` (spec § Proposed Fix concludes the conditional permission is
not exercised, and AC-7 gates it).

## Notes — binding structural rules

1. **Single source of truth per measured value.** Every measured quantity this plan depends on is
   captured exactly once, in Phase 0, into one named artifact. The artifacts are
   `phase0-instructions-read`, `toolchain-resolution` (workspace root, `BASE_SHA`, resolved tool
   paths), `dotnet-tool-restore` (the pinned csharpier version), `csharpier-check-baseline`, `msbuild-analyzers-baseline`,
   `msbuild-nullable-baseline`, `unowned-file-diagnostics-baseline`, `file-inventory-baseline`,
   `quickfiler-test-run-baseline`, and `quickfiler-test-coverage-baseline`. Every later task that
   depends on one of those values **cites the artifact** and compares against it. No later task
   restates a literal value, and no later task restates the comparison in different words.

   Two quantities are deliberately outside Phase 0 and are named here so the exceptions are not read
   as lapses. The first is `AddedLineCount:`, the tolerance band `P3-T6` applies to the coverage
   denominator: it is a property of the post-change tree and cannot exist in Phase 0, since two of
   its four inputs are files Phase 1 creates. `P3-T6` establishes it, records it in
   `quickfiler-test-coverage.<TS>.md`, and is the only task that does so; `P4-T3` re-measures line
   counts for a different purpose (the 500-line ceiling) and does not restate or consume it. The
   second is `pairs(N)`, the cumulative `spec.md` checkbox-pair count defined in the § Phase 5
   preamble: it measures an edit Phase 5 is in the middle of making, so no Phase 0 value for it can
   exist. Each of `P5-T1` through `P5-T10` establishes its own `pairs(N)` once, records it in that
   task's `ac-checkoff-ac<N>` artifact, and reads `pairs(N-1)` by citing the preceding task's
   artifact rather than by re-deriving it.
2. **The unowned-file diagnostic comparison is stated exactly once, in `P4-T2`.** No other task in
   this plan states any condition — absolute or relative — about diagnostics naming
   `QuickFiler.Test/Controllers/QfcItemController.FocusAndThemeTests.cs` or
   `UtilitiesCS/Threading/UiThread.cs`. This plan never asserts an absolute diagnostic count for a
   file it does not create or own: `P4-T2` asserts set equality against the Phase 0 baseline, and
   `P5-T6` cites `P4-T2` rather than re-deriving it.

   This is a disclosed reading of spec **AC-6**, not a silent substitution. AC-6's final sentence
   reads "No analyzer diagnostic is raised at either call site under toolchain steps 2 and 3",
   which is an absolute. This plan discharges it as **non-regression** against the Phase 0
   baseline: if `unowned-file-diagnostics-baseline` is empty, set equality against it is exactly
   the absolute AC-6 states; if it is non-empty, the pre-existing diagnostics belong to the
   sibling-owned file and this feature can neither introduce nor remove them, so the strongest
   claim it can honestly gate is that it added none. `P5-T6` therefore checks AC-6 off on the
   non-regression result, and the executor records the baseline count in the `P5-T6` artifact so a
   reviewer can see which of the two cases held.
3. **Every field this plan instructs the executor to declare carries a stated initializer**, or is
   named as definitely assigned in a stated constructor. The declarations are written out verbatim
   in § Fixture Contract. `/p:TreatWarningsAsErrors=true` promotes `CS0649` and `CS0169` to build
   errors, so an uninitialised, never-assigned field would make `P3-T4` unreachable. `P2-T3` is not
   the gate for this: it passes `/p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` without
   `/p:TreatWarningsAsErrors=true`, so `CS0649` stays a warning there and `P2-T3` would still exit `0`.
4. **No task asserts a source-text signature literal that the same task's instruction changes.**
   Where a behaviour must be verified, this plan names a test and asserts its result. Where a
   textual search is unavoidable, the asserted token is short, single-line, non-interpolated, and
   quoted verbatim in this plan's prose outside the command span. The complete set of tokens this
   plan asserts on is `UiThreadDispatcherGate`, `SwapUiThreadDispatcher`, `GetDedicatedDispatcher`,
   `typeof(UiThread)`, `BindingFlags`, `System.Reflection`, `System.Windows.Threading`,
   `FluentAssertions`, `[TestMethod]`,
   `[Timeout(GateTimeoutMs)]`, `error CS`, `Thread.Sleep`, `Task.Delay`, `Path.GetTempFileName`,
   `Path.GetTempPath`, `Path.GetRandomFileName`, `QfcItemController.TestSupport.cs`,
   `QfcItemController.UiThreadDispatcherFixture.cs`,
   `QfcItemController.UiThreadDispatcherFixtureTests.cs`,
   `QfcItemController.FocusAndThemeTests.cs`, and `UiThread.cs`. Every one of those searches is
   scoped to a file this plan creates or owns, **with two deliberate exceptions**: the
   `QfcItemController.FocusAndThemeTests.cs` and `UiThread.cs` searches in `P0-T10` and `P4-T2`,
   which are run against msbuild **log** files rather than against source, and which are therefore
   baseline comparisons rather than absolute assertions (see rule 2).
5. **Pre-existing baseline drift, and the BLOCKED branch for every Phase 0 gate.** `P0-T7` is
   expected to return `0` because `.github/workflows/_format-check.yml` runs the same
   manifest-pinned CSharpier against the same tree. That workflow spells the invocation
   `dotnet csharpier check .` rather than `dotnet tool run csharpier check .`; both resolve the
   same `dotnet-tools.json` pin, and this plan uses the `dotnet tool run` spelling that CLAUDE.md
   mandates. No claim is made here about branch-protection configuration, which this plan does not
   verify. If `P0-T7` returns non-zero, the worktree carries formatting drift this feature did not
   introduce: the executor records the reported file list in the `P0-T7` artifact and reports
   `BLOCKED: pre-existing csharpier drift` to the orchestrator rather than formatting files
   outside the Scope Lock.

   The same branch applies to `P0-T8` and `P0-T9`. Those tasks assert `EXIT_CODE: 0` because a
   green base tree is the precondition for every later comparison in this plan, not because this
   feature can make a red base tree green. If either returns non-zero, the executor records the
   full redacted error list in that task's artifact and reports
   `BLOCKED: pre-existing base-tree build failure` to the orchestrator. It does not modify any file
   outside the Scope Lock to make the gate pass, and it does not proceed to Phase 1.
6. **Residual risk R-1 is live during execution.** `QuickFiler.Test/Controllers/WpfUiDispatcherTests.cs`
   remains an ungated mutator of the same static (spec § Risks R-1), so it can still interleave
   with a transaction. `P5-T12` files that as its own issue; it is not fixed here.
7. **Deferred acceptance-criteria check-off is a deliberate deviation.**
   `.claude/skills/acceptance-criteria-tracking/SKILL.md` directs that an AC item be checked off as
   soon as the corresponding task passes verification, rather than deferred to the end. This plan
   defers all ten check-offs to Phase 5 on purpose: every one of AC-1 through AC-10 is a claim about
   the **final** state of the tree, and seven of them (AC-5 through AC-10) cite Phase 3 or Phase 4
   artifacts that do not exist until the toolchain loop has completed a clean pass. Checking an item
   off earlier would record a verdict that a Phase 3 restart could invalidate. The deviation is
   recorded here so a reviewer reads it as a decision rather than an omission.

8. **`user-story.md` in this feature folder is an inert placeholder.** Work Mode is `full-bug`, so
   `spec.md` is the sole acceptance-criteria source. The placeholder exists only because
   `.claude/hooks/enforce-feature-folder-order.ps1` requires the file to exist before any write to
   `plan.md`, without consulting the `- Work Mode:` marker. No task in this plan reads or checks
   off anything in it.

## Decisions Record

- **D1 — the `Install`-twice contract is tested as a sixth test, R6.** `spec.md`
  § Rollout & Follow-up item 4 asks the planner to choose between a sixth test and folding the
  assertion into R5, and to state the choice. The choice is a sixth test,
  `Install_CalledTwiceOnTheSameTransaction_ThrowsInvalidOperationException`, because R5's subject
  is gate over-release and mixing an unrelated contract into it would give R5 two failure reasons.
- **D2 — AC-8 is a fresh measurement, never a restatement.** The line counts in research §8 are
  projections. `P0-T11` measures the current counts; `P4-T3` re-measures after the final formatter
  pass and gates against the policy ceiling of 500. Neither task restates a research figure.
- **D3 — `UtilitiesCS/Threading/UiThread.cs` is not modified.** The conditional permission in
  `issue.md` § Constraints is not exercised, per spec § Proposed Fix and research §6. `P4-T1` and
  `P4-T7` gate it.
- **D4 — `PumpHarness.Restore` keeps gate release last.** The rewritten body disposes the token
  source and then disposes the transaction, so the transaction's restore-then-release pair is the
  final action and the gate is released strictly after the static is restored — which is the
  invariant that matters. Note that this is a deliberate **reordering**, not a preservation of the
  current sequence: at `HEAD` the body is `SwapUiThreadDispatcher(_previousUiThreadDispatcher);`
  then `TokenSource.Dispose();` then `UiThreadDispatcherGate.Release();` (`Part2.cs:348-350`), so
  the restore and the release were not adjacent and the token-source disposal sat between them.
  Moving the restore after the token-source disposal is safe because `TokenSource.Dispose()` neither
  reads nor writes `UiThread._dispatcher`, and it is necessary because the restore and the release
  are now a single indivisible action inside `UiThreadDispatcherTransaction.Dispose()`.
- **D5 — coverage is reported, not floored, and the denominator is checked before the rate is.**
  Spec § Test Strategy records that no production line is added or altered, so this plan asserts no
  repository-wide coverage floor. `P0-T13` and `P3-T6` record numeric coverage; `P3-T6` gates only
  that the package-level line rate has not regressed beyond measurement noise.

  This is a disclosed deviation, not an omission. Three in-scope rule files state a coverage floor,
  and this plan asserts none of them:
  `.claude/rules/csharp.md` § Testing Standards states a repository-wide line-coverage floor of
  80%, a 90% floor for any new module, class, or method, and that a coverage regression on changed
  lines is a blocking finding; `.claude/rules/general-unit-test.md` § Coverage Requirements states
  a line floor of >= 85% and a branch floor of >= 75% across all tiers; and
  `.claude/rules/quality-tiers.md` § Uniform-vs-Tier-Dependent Gate Matrix restates the same
  85%/75% pair as uniform across T1-T4. All three are named rather than one, because a disclosure
  that cites the least demanding of the three would understate what is being set aside. The
  ratifying authority is
  `<FEATURE>/spec.md` § Test Strategy, which records that this is a test-only change with no
  production line in the diff, that the new fixture is test infrastructure sitting inside the
  test-file exclusion, and that consequently there is no production coverage delta to defend. A
  reviewer should read the absence of a coverage floor in this plan as that decision rather than as
  a missing gate. `P3-T6` still records numeric coverage on both sides so the decision remains
  auditable against measured values.

  The `P3-T6` line-rate gate named two paragraphs above is conditional on the denominator being
  comparable, because the tool is known to be
  non-deterministic in exactly this dimension. The repository's own measured record
  (`.claude/agent-memory/atomic-executor/project_dotnet_coverage_denominator_nondeterminism.md`)
  documents `Invoke-MSTestWithCoverage.ps1` producing 47.16% at `lines-valid=180246` and 81.02% at
  `lines-valid=97933` **on the same unchanged tree**, because `dotnet-coverage` instruments every
  runtime-loaded module and its merge step sometimes double-counts. Narrowing `-SearchRoot` to
  `QuickFiler.Test` reduces but does not remove the class, since `UtilitiesCS.dll` and its
  dependencies are still loaded and instrumented. A line-rate delta compared across two different
  denominators is therefore not evidence of a coverage regression, and — given Phase 3's
  restart-on-failure rule — gating on it unconditionally would produce a loop with no exit. `P3-T6`
  accordingly records `lines-valid` for both runs and gates the rate only when the denominators are
  comparable.

- **D6 — the two new test files are placed beside their siblings, not under a `tests/` tree.**
  `.claude/rules/general-unit-test.md` § Test File Location requires test files to live in a
  `tests/` directory tree mirroring the production source, and states that colocation is not
  permitted. This plan places both new files in `QuickFiler.Test/Controllers/`. The deviation is
  disclosed here rather than left implicit, and it rests on three points. First, `CLAUDE.md` is
  first in the reading order of `.claude/skills/policy-compliance-order/SKILL.md`, and its embedded
  § C# Unit Test Policy specifies framework, mocking library, and assertion library but imposes no
  location requirement, so the `tests/` requirement is not restated by the higher-precedence
  document. Second, the repository's `tests/` tree exists but holds no C# whatever — its only child
  is `tests/scripts/`, which carries Pester suites — while every C# test in the solution lives in a
  sibling `<Assembly>.Test` project (`QuickFiler.Test`, `UtilitiesCS.Test`, `TaskMaster.Test`, and
  six more). A conforming placement would therefore put these two files in a directory that no
  `<Compile Include>` reaches and no test assembly builds; the change would not compile, let alone
  run. Third, `<FEATURE>/spec.md` § Test Strategy and § Scope name
  both paths explicitly, and § Scope Lock binds this plan to them. A reviewer should read the
  placement as conformance to the repository's established C# layout rather than as an unnoticed
  rule violation. Relocating the C# suite is out of scope for a test-isolation bug fix and is not
  attempted here.

---

### Phase 0 — Policy reads, toolchain bootstrap, and baseline capture

- [x] [P0-T1] Read, in the order given by `.claude/skills/policy-compliance-order/SKILL.md`: `CLAUDE.md`, `.claude/rules/general-code-change.md`, `.claude/rules/general-unit-test.md`, `.claude/rules/csharp.md`, `.claude/rules/architecture-boundaries.md`, `.claude/rules/quality-tiers.md`, `.claude/rules/plan-acceptance-gates.md`, `.claude/rules/tonality.md`, then `.claude/skills/atomic-plan-contract/SKILL.md`, `.claude/skills/evidence-and-timestamp-conventions/SKILL.md`, `.claude/skills/acceptance-criteria-tracking/SKILL.md`, then `<FEATURE>/spec.md`, `<FEATURE>/issue.md`, and `<FEATURE>/research/2026-08-24T11-05-uithread-dispatcher-restore-scope-research.md`. Write `<FEATURE>/evidence/baseline/phase0-instructions-read.<TS>.md`. **Acceptance:** the artifact exists and carries `Timestamp:`, `Policy Order:`, and one list entry per file above, each entry naming the repo-relative path.

- [x] [P0-T2] Resolve and record the execution environment. Run `git rev-parse --show-toplevel` (giving `WS`), `git rev-parse HEAD` (giving `BASE_SHA`), `git status --porcelain -- '*.cs' '*.csproj' '*.sln'`, and resolve `msbuild` and `vstest.console.exe` through `vswhere` as described in § Conventions. Write `<FEATURE>/evidence/baseline/toolchain-resolution.<TS>.md`. **Acceptance:** the artifact records `BASE_SHA:` as a 40-character hexadecimal string, records the redacted resolved MSBuild path and the redacted resolved vstest path (ending in `MSBuild.exe` and `vstest.console.exe` respectively), and records that the scoped `git status --porcelain` above produced zero output lines.

- [x] [P0-T3] Provision the repo-local .NET SDK, which a fresh agent worktree does not carry, by running `pwsh -NoProfile -File scripts/vscode/Install-RepoDotNetSdk.ps1` from `WS`. Write `<FEATURE>/evidence/baseline/dotnet-sdk-bootstrap.<TS>.md`. **Acceptance:** `dotnet --version` run from `WS` exits `0` and prints a version beginning `8.0.`, and `Test-Path (Join-Path $WS '.dotnet-sdk/sdk/8.0.205')` returns `True`; both results are recorded redacted in the artifact. Do **not** assert anything about `dotnet --list-sdks`: that command does not consult `global.json` and enumerates only the host root of the muxer on `PATH`, so it prints the machine-wide SDK list and never names the repo-local install directory, whether or not the install succeeded. `8.0.205` is the version pinned by `global.json` and is the default `-Version` of `scripts/vscode/Install-RepoDotNetSdk.ps1`; it is also the exact marker path that script itself checks before deciding the SDK is already installed.

- [x] [P0-T4] Restore the manifest-pinned CSharpier by running `dotnet tool restore` from `WS`, then `dotnet tool run csharpier --version`. Write `<FEATURE>/evidence/baseline/dotnet-tool-restore.<TS>.md`. **Acceptance:** the artifact records `EXIT_CODE: 0` for `dotnet tool restore`, and the recorded `dotnet tool run csharpier --version` output begins with `1.2.6`, the version pinned by `dotnet-tools.json` at the repository root.

- [x] [P0-T5] Restore NuGet packages, which a fresh agent worktree does not carry, by running `pwsh -NoProfile -File scripts/vscode/Invoke-Restore.ps1 -SolutionPath TaskMaster.sln -Configuration Debug -Platform 'Any CPU'` from `WS`. Write `<FEATURE>/evidence/baseline/nuget-restore.<TS>.md`. **Acceptance:** the artifact records `EXIT_CODE: 0`, and the path `packages/Meziantou.Analyzer.3.0.174/build/Meziantou.Analyzer.props` — named by the `EnsureNuGetPackageBuildImports` error target of `QuickFiler.Test/QuickFiler.Test.csproj` — exists under `WS`.

- [x] [P0-T6] Back-fill the two analyzer package versions that the `<Analyzer Include>` items name but `packages.config` does not pin, so compilation does not fail with `CS0006`. Resolve the main checkout from `git -C $WS rev-parse --git-common-dir`, rooting the result against `WS` when it is relative, and copy `packages/Meziantou.Analyzer.3.0.156` and `packages/Roslynator.Analyzers.4.16.0` from there into `WS/packages/`; if either folder is absent in the main checkout, obtain it with `nuget.exe install <id> -Version <version> -OutputDirectory packages` instead. Write `<FEATURE>/evidence/baseline/analyzer-backfill.<TS>.md`. **Acceptance:** all five DLL paths named by the `<Analyzer Include>` items at `QuickFiler.Test/QuickFiler.Test.csproj` lines 466-470 exist under `WS`, and the artifact lists each of the five as a repo-relative path with a `True` existence result.

- [x] [P0-T7] Capture the formatter baseline: run `dotnet tool run csharpier check .` from `WS`. Write `<FEATURE>/evidence/baseline/csharpier-check-baseline.<TS>.md`. **Acceptance:** the artifact records `EXIT_CODE: 0` together with the verbatim final summary line emitted by the command. See § Notes rule 5 for the non-zero path.

- [x] [P0-T8] Capture the analyzer baseline: run `& $MSBUILD TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` with output redirected to `TestResults/plan-logs/p0-t8/msbuild-analyzers.log`. Write `<FEATURE>/evidence/baseline/msbuild-analyzers-baseline.<TS>.md`. **Acceptance:** the artifact records `EXIT_CODE: 0`, records the total warning count and total error count reported by the msbuild summary, and names the log path `TestResults/plan-logs/p0-t8/msbuild-analyzers.log`.

- [x] [P0-T9] Capture the type-check baseline: run `& $MSBUILD TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true` with output redirected to `TestResults/plan-logs/p0-t9/msbuild-nullable.log`. Do not add `/p:Nullable=enable` and do not substitute `/t:Build`. Write `<FEATURE>/evidence/baseline/msbuild-nullable-baseline.<TS>.md`. **Acceptance:** the artifact records `EXIT_CODE: 0`, records the total warning count and total error count reported by the msbuild summary, and names the log path `TestResults/plan-logs/p0-t9/msbuild-nullable.log`.

- [x] [P0-T10] Capture the **single source of truth** for unowned-file diagnostics. From the two logs produced by `P0-T8` and `P0-T9`, extract every line containing the simple string `QfcItemController.FocusAndThemeTests.cs` and every line containing the simple string `UiThread.cs`, redact each line per § Conventions, and write them to `<FEATURE>/evidence/baseline/unowned-file-diagnostics-baseline.<TS>.md`. **Acceptance:** the artifact records `AnalyzerStepMatchCount:` and `NullableStepMatchCount:` as integers, lists every matched line verbatim in redacted form under a heading naming its source log, and states explicitly that a zero count is a legitimate recorded value. This artifact is the only baseline for the comparison in `P4-T2`.

- [x] [P0-T11] Capture the **single source of truth** for file identity and size. For each of `QuickFiler.Test/Controllers/QfcItemController.TestSupport.cs`, `QuickFiler.Test/Controllers/QfcItemController.InitializationTests.Part2.cs`, `QuickFiler.Test/Controllers/QfcItemController.FocusAndThemeTests.cs`, `QuickFiler.Test/QuickFiler.Test.csproj`, and `UtilitiesCS/Threading/UiThread.cs`, record the line count and the SHA-256 hash. Write `<FEATURE>/evidence/baseline/file-inventory-baseline.<TS>.md`. **Acceptance:** the artifact contains one row per path above, each carrying an integer line count and a 64-character hexadecimal SHA-256 value, and repeats `BASE_SHA:` as recorded by `P0-T2`.

- [x] [P0-T12] Capture the **single source of truth** for the `QuickFiler.Test` pass/fail set, using the Debug output left in `QuickFiler.Test\bin\Debug` by the Phase 0 msbuild steps — that is, the output of `P0-T9`, which is the most recent `/t:Rebuild` at this point and which overwrote `P0-T8`'s output from the same sources: run `& $VSTEST QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /EnableCodeCoverage /InIsolation /TestCaseFilter:"TestCategory!=LiveOutlook" /Logger:"trx;LogFileName=quickfiler-test-baseline.trx" /ResultsDirectory:TestResults\plan-logs\p0-t12`. Write `<FEATURE>/evidence/baseline/quickfiler-test-run-baseline.<TS>.md`. **Acceptance:** the artifact records `EXIT_CODE:`, the integer total, passed, failed, and skipped counts parsed from the run summary, and — under a heading `BaselineFailedTests` — the fully-qualified name of every failed test, one per line, with the explicit note that an empty list is a legitimate recorded value. This artifact is the only baseline for the comparison in `P3-T5`.

- [x] [P0-T13] Capture the **single source of truth** for coverage. Create `TestResults/plan-logs/p0-t13`, then run `pwsh -NoProfile -File scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot QuickFiler.Test -Configuration Debug -CoverageOutput TestResults\plan-logs\p0-t13\coverage-baseline.cobertura.xml` from `WS`. Write `<FEATURE>/evidence/baseline/quickfiler-test-coverage-baseline.<TS>.md`. **Acceptance:** the artifact records `EXIT_CODE:` as observed; records, under a heading `CoverageBaselineFailedTests`, the name of every failed test **exactly as the run's own console output spells it**, one per line, with the explicit note that an empty list is a legitimate recorded value; records in `Output Summary:` the numeric `line-rate`, `branch-rate`, and `lines-valid` attribute values read from the root `coverage` element of the emitted Cobertura file; and records the discovered test-assembly list, which must be exactly the single path `QuickFiler.Test\bin\Debug\QuickFiler.Test.dll`. The name form is the console spelling and not a fully-qualified name on purpose: `Get-DotnetCoverageArgumentList` (`scripts/vscode/Invoke-MSTestWithCoverage.ps1:70-76`) appends only `/Settings:`, `/InIsolation`, and `/TestCaseFilter:` to the inner `vstest.console` invocation and supplies **no** `/Logger:trx`, so no TRX is produced and no fully-qualified name is available from this pipeline. Requiring one here would be an acceptance no executor could satisfy. The comparison in `P3-T6` is a set comparison against this artifact, and `P3-T6` runs the identical command, so both sides carry the same spelling and the comparison remains exact; this differs from `P0-T12`/`P3-T5`, which do pass `/Logger:trx` and therefore do record fully-qualified names. Do **not** assert `EXIT_CODE: 0` here. `scripts/vscode/Invoke-MSTestWithCoverage.ps1` contains `if ($coverageExitCode -ne 0) { throw ... }`, so a non-zero exit may mean some test anywhere in the search root failed, including tests in files this plan does not own — the same possibility `P0-T12` is written to accommodate. That is not the only non-zero path: `Assert-CoberturaLineCoverageThreshold` (`scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1:487-490`) throws when the post-processed root `line-rate` is below 80%, so a run in which **every** test passed can still exit non-zero. That outcome is a live possibility rather than an anomaly here — see § Decisions Record D5, which records two measured runs of this same script on one unchanged tree landing on opposite sides of that threshold, at 47.16% and 81.02% — and it is why `CoverageBaselineFailedTests` may legitimately be empty on a non-zero exit. When the script throws on either path, the Cobertura file still exists at the requested `-CoverageOutput` path, so the three root attributes are read from it regardless. Additionally record `CoberturaPostProcessed:` as `true` when this task's `EXIT_CODE:` is `0` and `false` otherwise. That field is load-bearing and is not a convenience. `ConvertTo-KoverageCoberturaXml` is the step which removes third-party `<package>` elements and then rewrites the root `line-rate`, `branch-rate`, `lines-covered`, and `lines-valid` attributes from the surviving packages, and the rewritten document reaches disk only at the `Set-Content` call that follows it (`scripts/vscode/Invoke-MSTestWithCoverage.ps1:339-343`). Neither of the two throwing paths reaches that write: the coverage-exit check throws **before** `ConvertTo-KoverageCoberturaXml` is called at all, and the 80% threshold check throws **after** it but **before** `Set-Content`, discarding the recomputed document in memory. A run that exits non-zero for either reason therefore leaves the raw all-modules-instrumented totals on the root element of the file on disk, while a clean run leaves recomputed first-party totals there. The `EXIT_CODE: 0` test is consequently an exact test for which of the two quantities the file carries, not an approximation of one. The two are different quantities, not two noisy samples of one quantity, so a triple recorded under one value of `CoberturaPostProcessed:` may not be compared against a triple recorded under the other. This artifact is the only baseline for the comparison in `P3-T6`.

- [x] [P0-T14] Capture part 1 of the AC-10 fail-before evidence: copy the verbatim pre-change body of `EnsureUiThreadDispatcher` from `QuickFiler.Test/Controllers/QfcItemController.TestSupport.cs` lines 238-249 into `<FEATURE>/evidence/regression-testing/fail-before-exception.<TS>.md`, together with a statement of why a red *test run* cannot exist for this defect: the helper returns `void` at the base branch, so the regression tests cannot compile against it, per spec § Test Strategy "Fail-before evidence". The filename stem is `fail-before-exception` and not any other spelling, because `.claude/skills/evidence-and-timestamp-conventions/SKILL.md` names `fail-before-exception.*.md` as the minimum search pattern a reviewer must use before writing a negative claim that no fail-before evidence exists; a differently-named artifact would be invisible to that search. **Acceptance:** the artifact quotes those twelve source lines verbatim inside a fenced `csharp` block, carries a `WhyFailingRunImpossible:` field of one to three sentences, and names `P1-T4` as the task that supplies the compile-level half of the demonstration.

---

### Phase 1 — Shared fixture, regression tests, and fail-before demonstration

- [x] [P1-T1] Create `QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixture.cs` in namespace `QuickFiler.Controllers.Tests`, containing `internal static class UiThreadDispatcherFixture` and `internal sealed class UiThreadDispatcherTransaction : IDisposable`, implemented exactly to § Fixture Contract below. Every field declaration listed there carries its initializer at the declaration or is assigned in the single constructor named there; no field may be declared without one. Reproduce `_dedicatedDispatcher`, `_dedicatedDispatcherLock`, and `GetDedicatedDispatcher` from `QfcItemController.TestSupport.cs` in this file, renamed `_parkedDispatcher`, `ParkedDispatcherLock`, and `GetParkedDispatcher`. This task writes only the new file: it does **not** delete the originals, and it does not edit `QfcItemController.TestSupport.cs` at all. `P2-T1` performs that deletion. The split is deliberate. `P0-T14` and `P2-T1` both cite absolute line numbers in `QfcItemController.TestSupport.cs` (213-220, 221-222, and 238-249), and those citations are only stable while Phase 1 leaves that file untouched; and `P1-T4` is written to observe a tree in which the only compile errors naming a source file are the ones in the new regression-test file. Leave `StartRunningDispatcher` and `ShutdownDispatcher` in `QfcItemControllerTestSupport`, because three unowned test files call them: `WpfUiDispatcherTests.cs`, `QfcItemController.FolderHandlingTests.cs`, and `QfcItemController.ViewerSetupTests.cs`. **Acceptance:** the file exists at that path and `Select-String -SimpleMatch -Pattern 'typeof(UiThread)' -Path 'QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixture.cs'` returns at least one match; the artifact `<FEATURE>/evidence/other/fixture-created.<TS>.md` records that match count and the file's line count.

- [x] [P1-T2] Add exactly two `<Compile Include>` entries to `QuickFiler.Test/QuickFiler.Test.csproj`, immediately after the existing entry for `Controllers\QfcItemController.TestSupport.cs`, in this order: `QfcItemController.UiThreadDispatcherFixture.cs` then `QfcItemController.UiThreadDispatcherFixtureTests.cs`. Change nothing else in the file. **Acceptance:** let `L` be the 1-based number of the single line of that csproj containing the simple string `QfcItemController.TestSupport.cs`; line `L+1` contains the simple string `QfcItemController.UiThreadDispatcherFixture.cs` and line `L+2` contains the simple string `QfcItemController.UiThreadDispatcherFixtureTests.cs`. The artifact `<FEATURE>/evidence/other/csproj-compile-entries.<TS>.md` records those three line numbers with the three matched lines quoted.

- [x] [P1-T3] Create `QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixtureTests.cs` in namespace `QuickFiler.Controllers.Tests`, hosting `[TestClass] public class QfcItemController_UiThreadDispatcherFixtureTests` with `private const int GateTimeoutMs = 60000;` and the six tests R1-R6 specified in § Regression Tests below. Every test method carries the attribute `[Timeout(GateTimeoutMs)]` on its own line. Use MSTest attributes and FluentAssertions assertions only; use `ManualResetEventSlim` or awaited `Task` completion for all cross-thread coordination; introduce no `Thread.Sleep`, no `Task.Delay`, no wall-clock wait, and no temporary file. Give the test class an XML doc comment that names R1 as the primary deterministic regression assertion and R4 as the supporting probabilistic one, as § Regression Tests requires and as spec AC-3 gates. **Acceptance:** the file exists at that path, `Select-String -SimpleMatch -Pattern '[TestMethod]'` against it returns exactly six matches, and `Select-String -SimpleMatch -Pattern '[Timeout(GateTimeoutMs)]'` against it returns exactly six matches; both counts are recorded in `<FEATURE>/evidence/other/regression-tests-created.<TS>.md`, together with the class-level doc sentence quoted verbatim from the file under the field `PrimaryAssertionDoc:`. That quotation is recorded rather than searched for because the sentence is prose that CSharpier may rewrap across lines, which would make a line-oriented search return zero matches whatever the executor wrote.

- [x] [P1-T4] `[expect-fail]` Capture part 2 of the AC-10 fail-before evidence. With the fixture and the regression tests in place but `QfcItemControllerTestSupport.EnsureUiThreadDispatcher` still declared `void`, run `& $MSBUILD TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` with output redirected to `TestResults/plan-logs/p1-t4/msbuild-failbefore.log`. Write `<FEATURE>/evidence/regression-testing/fail-before-compile.<TS>.md`. **Acceptance:** the artifact records `ExpectedExitCode: 1`, records a non-zero `EXIT_CODE:`, and records `FailBeforeErrorLineCount:` as an integer greater than zero, being the number of lines in that log containing both the simple string `QfcItemController.UiThreadDispatcherFixtureTests.cs` and the simple string `error CS`; at least one such line is quoted verbatim in redacted form.

---

### Phase 2 — Migrate the owned files and verify pass-after

- [x] [P2-T1] In `QuickFiler.Test/Controllers/QfcItemController.TestSupport.cs`, replace lines 238-249 with the single delegating expression member `internal static IDisposable EnsureUiThreadDispatcher() => UiThreadDispatcherFixture.EnsureDispatcher();`, retaining an updated XML doc comment stating that the return value is a scope whose `Dispose` conditionally reverts the seeding and that discarding it is permitted. Delete `_dedicatedDispatcher`, `_dedicatedDispatcherLock`, and `GetDedicatedDispatcher`, whose renamed replacements `P1-T1` created in the new fixture file; this task performs the deletion, which `P1-T1` deliberately did not. Also delete the orphaned XML doc block at lines 213-220, which describes a dispatcher-pumping helper and sits immediately above the two field declarations at lines 221-222 without documenting either of them; deleting only the fields would leave that block attached to nothing and immediately followed by a second doc block. (This has no build effect — `QuickFiler.Test.csproj` sets no `DocumentationFile`, so `CS1587` cannot fire — but it would leave the file incoherent to a reader.) Leave every other member of the file, including `StartRunningDispatcher` and `ShutdownDispatcher`, unchanged. **Acceptance:** against `QuickFiler.Test/Controllers/QfcItemController.TestSupport.cs`, `Select-String -SimpleMatch -Pattern 'typeof(UiThread)'` returns zero matches and `Select-String -SimpleMatch -Pattern 'GetDedicatedDispatcher'` returns zero matches; both counts are recorded in `<FEATURE>/evidence/other/testsupport-migrated.<TS>.md`.

- [x] [P2-T2] Rewrite the dispatcher-swap machinery in `QuickFiler.Test/Controllers/QfcItemController.InitializationTests.Part2.cs` exactly as specified in § Part2 Migration below: delete the `UiThreadDispatcherGate` field and its doc block at lines 36-51, replacing them with a two-to-three line comment that points at `UiThreadDispatcherFixture` and preserves the #230 rationale; delete `SwapUiThreadDispatcher` and its doc block at lines 143-158; route `BuildPumpHarnessAsync`, `BuildPumpHarnessCoreAsync`, and `PumpHarness` through `UiThreadDispatcherTransaction`; and delete the now-unused `using System.Reflection;`, `using System.Windows.Threading;`, and `using FluentAssertions;` directives. The replacement comment **must not contain the identifiers `UiThreadDispatcherGate` or `SwapUiThreadDispatcher`**, because `P4-T4` rows 1 and 2 assert zero matches for those two tokens against this file and a rationale comment that names them would silently defeat both rows. Keep the two-phase `BeginTransactionAsync` then `Install` shape, keep the acquisition at build start, and keep `PumpHarness.Restore` idempotent. Do not change the signature of `BuildPumpHarnessAsync`, which `QfcItemController.SeamFactoryTests.cs` calls at lines 313 and 384, or of `PumpHarness.Restore`, which the same file calls at lines 358 and 429. **Acceptance:** `<FEATURE>/evidence/other/part2-migrated.<TS>.md` records the file's post-edit line count together with the single-path diff-stat line produced by `git diff --stat -- QuickFiler.Test/Controllers/QfcItemController.InitializationTests.Part2.cs`, showing a non-zero deletion count; and, against that same path, `Select-String -SimpleMatch -Pattern 'BindingFlags'` returns zero matches, `Select-String -SimpleMatch -Pattern 'System.Reflection'` returns zero matches, `Select-String -SimpleMatch -Pattern 'System.Windows.Threading'` returns zero matches, and `Select-String -SimpleMatch -Pattern 'FluentAssertions'` returns zero matches. All five counts are recorded in the artifact. The four zero-match conditions are what gate the using-directive deletions and the completeness of the reflection removal; the diff-stat alone would be satisfied by deleting a single blank line and gates almost nothing on its own.

- [x] [P2-T3] Verify the pass-after counterpart to `P1-T4`: run `& $MSBUILD TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` with output redirected to `TestResults/plan-logs/p2-t3/msbuild-analyzers.log`. Write `<FEATURE>/evidence/regression-testing/pass-after-compile.<TS>.md`. **Acceptance:** the artifact records `EXIT_CODE: 0` and records that zero lines of that log contain both the simple string `QfcItemController.UiThreadDispatcherFixtureTests.cs` and the simple string `error CS`.

- [x] [P2-T4] Run the six regression tests: `& $VSTEST QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /InIsolation /Settings:scripts\vscode\TaskMaster.cli.runsettings /TestCaseFilter:"FullyQualifiedName~QfcItemController_UiThreadDispatcherFixtureTests" /Logger:"trx;LogFileName=regression.trx" /ResultsDirectory:TestResults\plan-logs\p2-t4`. Write `<FEATURE>/evidence/regression-testing/regression-tests-pass.<TS>.md`. **Acceptance:** the artifact records `EXIT_CODE: 0`, records `Passed: 6` and `Failed: 0`, and lists six fully-qualified test names that are exactly the R1-R6 names given in § Regression Tests.

- [x] [P2-T5] Run the affected consumer classes and the two unowned call sites: `& $VSTEST QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /InIsolation /Settings:scripts\vscode\TaskMaster.cli.runsettings /TestCaseFilter:"FullyQualifiedName~QfcItemController_InitializationTests|FullyQualifiedName~QfcItemController_SeamFactoryTests|FullyQualifiedName~SetThemeDark_FromNormal_SelectsDarkNormalTheme|FullyQualifiedName~SetThemeLight_FromNormal_SelectsLightNormalTheme" /Logger:"trx;LogFileName=consumers.trx" /ResultsDirectory:TestResults\plan-logs\p2-t5`. Write `<FEATURE>/evidence/regression-testing/consumer-classes-pass.<TS>.md`. **Acceptance:** the artifact records `EXIT_CODE:` as observed; records that the set of fully-qualified failed test names is a **subset** of the `BaselineFailedTests` set recorded by `P0-T12`; and records that both `SetThemeDark_FromNormal_SelectsDarkNormalTheme` and `SetThemeLight_FromNormal_SelectsLightNormalTheme` appear in the passed-test list. Do **not** assert an absolute `Failed: 0` here. The filter selects `QfcItemController_InitializationTests` (whose `Part3.cs` is not in § Scope Lock), `QfcItemController_SeamFactoryTests`, and two tests in the sibling-owned `QfcItemController.FocusAndThemeTests.cs`; an absolute all-green assertion over test files this feature does not own is unsatisfiable whenever any of them is already red, which is exactly the possibility `P0-T12` records a baseline for. The two named theme tests remain absolute pass assertions because spec AC-6 requires precisely that of them.

---

### Phase 3 — Final C# toolchain QA loop

Every task in this phase executes its stated command unconditionally; there is no skip branch and
`EXIT_CODE: SKIPPED` is not a valid outcome for any of them. If any task in this phase fails or
rewrites a file, restart the phase from `P3-T1`.

- [x] [P3-T1] Apply formatting to this plan's own C# paths only: `dotnet tool run csharpier format QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixture.cs QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixtureTests.cs QuickFiler.Test/Controllers/QfcItemController.TestSupport.cs QuickFiler.Test/Controllers/QfcItemController.InitializationTests.Part2.cs`. Write `<FEATURE>/evidence/qa-gates/csharpier-format.<TS>.md`. **Acceptance:** the artifact records `EXIT_CODE: 0` and records, for each of the four paths, its SHA-256 before and after this command, so a rewrite is visible as a changed pair rather than inferred from the tool's processed-file count.

- [x] [P3-T2] Verify formatting repository-wide, read-only: `dotnet tool run csharpier check .` from `WS`. Write `<FEATURE>/evidence/qa-gates/csharpier-check.<TS>.md`. **Acceptance:** the artifact records `EXIT_CODE: 0` together with the verbatim final summary line.

- [x] [P3-T3] Run the analyzer gate: `& $MSBUILD TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` with output redirected to `TestResults/plan-logs/p3-t3/msbuild-analyzers.log`. Write `<FEATURE>/evidence/qa-gates/msbuild-analyzers.<TS>.md`. **Acceptance:** the artifact records `EXIT_CODE: 0`, records the total warning count and total error count from the msbuild summary, and names the log path `TestResults/plan-logs/p3-t3/msbuild-analyzers.log` for consumption by `P4-T2`.

- [x] [P3-T4] Run the type-check gate: `& $MSBUILD TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true` with output redirected to `TestResults/plan-logs/p3-t4/msbuild-nullable.log`. Do not add `/p:Nullable=enable` and do not substitute `/t:Build`. Write `<FEATURE>/evidence/qa-gates/msbuild-nullable.<TS>.md`. **Acceptance:** the artifact records `EXIT_CODE: 0`, records the total warning count and total error count from the msbuild summary, and names the log path `TestResults/plan-logs/p3-t4/msbuild-nullable.log` for consumption by `P4-T2`.

- [x] [P3-T5] Run the CI-parity test gate: `& $VSTEST QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /EnableCodeCoverage /InIsolation /TestCaseFilter:"TestCategory!=LiveOutlook" /Logger:"trx;LogFileName=quickfiler-test-final.trx" /ResultsDirectory:TestResults\plan-logs\p3-t5`, with no `/Settings:` argument, matching `.github/workflows/_mstest-coverage.yml`. Write `<FEATURE>/evidence/qa-gates/quickfiler-test-run.<TS>.md`. **Acceptance:** the artifact records `EXIT_CODE:` with the total, passed, failed, and skipped counts; records that the set of fully-qualified failed test names is a subset of the `BaselineFailedTests` set recorded by `P0-T12` in `<FEATURE>/evidence/baseline/quickfiler-test-run-baseline.<TS>.md`; and records that the six R1-R6 names plus `SetThemeDark_FromNormal_SelectsDarkNormalTheme` and `SetThemeLight_FromNormal_SelectsLightNormalTheme` all appear in the passed-test list. The two theme tests are the plan's **only** absolute pass assertions over a file this feature does not own, and they are stated here for the same reason `P2-T5` states them: spec AC-6 requires precisely that of those two tests by name. Everything else in this task is a baseline comparison. If either theme test is already failing in the `P0-T12` baseline, this feature cannot make it pass and the correct outcome is `BLOCKED: pre-existing failure in a sibling-owned test blocks AC-6`, reported to the orchestrator rather than absorbed by widening the assertion.

- [x] [P3-T6] Run the coverage gate under the class-level parallelized runsettings, which is also the supplementary evidence spec § Test Strategy asks for because the CI invocation is sequential. Spec § Test Strategy names `TaskMaster.runsettings` for that supplementary run; this task instead uses `scripts/vscode/TaskMaster.cli.runsettings`, which `scripts/vscode/Invoke-MSTestWithCoverage.ps1` resolves unconditionally from its own script directory and which cannot be overridden by a parameter. The substitution is sound and is recorded rather than silently made: both files declare `<Scope>ClassLevel</Scope>`, so the parallelization the spec asks to exercise is identical, and the CLI file additionally omits the Code Coverage `<DataCollector>` so the inner vstest run does not activate a second collector alongside the outer `dotnet-coverage` instrumentation. Create `TestResults/plan-logs/p3-t6`, then run `pwsh -NoProfile -File scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot QuickFiler.Test -Configuration Debug -CoverageOutput TestResults\plan-logs\p3-t6\coverage-final.cobertura.xml`. Write `<FEATURE>/evidence/qa-gates/quickfiler-test-coverage.<TS>.md`. **Acceptance:** the artifact records `EXIT_CODE:` as observed and records that the set of failed test names — in the same console spelling `P0-T13` records, this task running the identical command — is a subset of the `CoverageBaselineFailedTests` set recorded by `P0-T13` (an absolute `EXIT_CODE: 0` is not asserted here, for the reason stated in `P0-T13`); records in `Output Summary:` the numeric post-change `line-rate`, `branch-rate`, and `lines-valid` from the root `coverage` element; cites the `quickfiler-test-coverage-baseline` artifact produced by `P0-T13`, resolved per § Conventions, for the baseline triple; records `CoberturaPostProcessed:` by the same rule `P0-T13` states (`true` when this task's `EXIT_CODE:` is `0`, `false` otherwise); records `AddedLineCount:` as defined in the next paragraph; and gates the line-rate delta as follows. The rate gate runs only when this task's `CoberturaPostProcessed:` equals the value recorded by `P0-T13`; when the two differ, the artifact records `PipelineMismatch: true` and the rate gate is skipped, because the two root-attribute triples were produced by different post-processing paths and are not comparable at all. When they match, and when the post-change `lines-valid` differs from the baseline `lines-valid` by no more than `AddedLineCount:`, the line-rate delta in percentage points must be at least `-0.50`. When they match but `lines-valid` differs by more, the run is recorded as `DenominatorAnomaly: true`. **`AddedLineCount:` is established by this task and by no other**, because no earlier artifact can hold it — `P0-T11` predates the two new files and `P4-T3` runs after this task. This task computes it as the sum of four measurements it takes itself and records in its own artifact: the line count of `QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixture.cs`, plus the line count of `QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixtureTests.cs`, plus, for each of `QuickFiler.Test/Controllers/QfcItemController.TestSupport.cs` and `QuickFiler.Test/Controllers/QfcItemController.InitializationTests.Part2.cs`, the greater of zero and (its measured line count minus the line count `P0-T11` recorded for the same path). Every input is available at this point in the plan and none is restated from a projection. The figure is a deliberate over-estimate of the denominator's plausible movement: `ConvertTo-DerivedCoverageSettingsXml` adds the module exclusion `.*\.Test\.dll$` before collection, so every line this feature adds sits in an assembly that is not instrumented and the expected `lines-valid` delta is in fact zero. `AddedLineCount:` is therefore a tolerance band, not a prediction, and a `lines-valid` movement larger than it is by construction attributable to the tool rather than to this diff. In either the `PipelineMismatch: true` or the `DenominatorAnomaly: true` case the coverage collection is repeated **once** and the second measurement is the one gated by the same rules; a second occurrence of either condition is recorded and reported to the orchestrator rather than retried further. See § Decisions Record D5 for why an unconditional delta gate has no exit under Phase 3's restart rule. Finally, the artifact records `ProductionSourcePathCount: PROVISIONAL — established by P4-T7`, using `P4-T7`'s own field name, because `P4-T7` has not yet run at this point and this task does not establish the value.

---

### Phase 4 — Invariant gates, scope lock, and commit

- [x] [P4-T1] Verify the two files this feature must not modify are byte-identical to their Phase 0 state: recompute the SHA-256 of `QuickFiler.Test/Controllers/QfcItemController.FocusAndThemeTests.cs` and of `UtilitiesCS/Threading/UiThread.cs`. Write `<FEATURE>/evidence/qa-gates/unowned-file-identity.<TS>.md`. **Acceptance:** each recomputed SHA-256 equals the value recorded for the same path by `P0-T11` in `<FEATURE>/evidence/baseline/file-inventory-baseline.<TS>.md`, and the artifact quotes both the recorded and the recomputed value for each path.

- [x] [P4-T2] Perform the plan's **single** unowned-file diagnostic comparison. From `TestResults/plan-logs/p3-t3/msbuild-analyzers.log` and `TestResults/plan-logs/p3-t4/msbuild-nullable.log`, extract the same two line sets `P0-T10` extracted — lines containing the simple string `QfcItemController.FocusAndThemeTests.cs`, and lines containing the simple string `UiThread.cs` — redact them, and compare them against the sets recorded in `<FEATURE>/evidence/baseline/unowned-file-diagnostics-baseline.<TS>.md`. Write `<FEATURE>/evidence/qa-gates/unowned-file-diagnostics-comparison.<TS>.md`. **Acceptance:** for each of the two source logs, the final match count equals the corresponding baseline count and the final line set is identical to the baseline line set after redaction; the artifact records both counts, both set-equality results, and any symmetric difference. No other task in this plan states a condition about diagnostics naming either file.

- [x] [P4-T3] Audit file size after the final formatter pass, against the 500-line ceiling in `.claude/rules/general-code-change.md`. Measure the line count of `QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixture.cs`, `QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixtureTests.cs`, `QuickFiler.Test/Controllers/QfcItemController.TestSupport.cs`, and `QuickFiler.Test/Controllers/QfcItemController.InitializationTests.Part2.cs`. Write `<FEATURE>/evidence/qa-gates/file-size-audit.<TS>.md`. **Acceptance:** the artifact records one row per path with its measured integer line count, and every measured count is at or below 500.

- [x] [P4-T4] Audit the removal of the duplicated #230 workaround and the uniqueness of the reflection swap, using this three-row matrix. Row 1: `Select-String -SimpleMatch -Pattern 'UiThreadDispatcherGate'` against `QuickFiler.Test/Controllers/QfcItemController.InitializationTests.Part2.cs` returns zero matches. Row 2: `Select-String -SimpleMatch -Pattern 'SwapUiThreadDispatcher'` against that same path returns zero matches. Row 3: `Select-String -SimpleMatch -Pattern 'typeof(UiThread)'` against each of `QuickFiler.Test/Controllers/QfcItemController.InitializationTests.Part2.cs`, `QuickFiler.Test/Controllers/QfcItemController.TestSupport.cs`, and `QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixtureTests.cs` returns zero matches, so the only in-scope file holding the reflection swap is the one `P1-T1` asserts holds it. Write `<FEATURE>/evidence/qa-gates/duplicate-swap-removal.<TS>.md`. **Acceptance:** all three rows hold and the artifact records each row's command, target path or paths, and match count.

- [x] [P4-T5] Audit determinism. `Select-String -SimpleMatch` for each of the five tokens `Thread.Sleep`, `Task.Delay`, `Path.GetTempFileName`, `Path.GetTempPath`, and `Path.GetRandomFileName`, against each of the four in-scope C# paths listed in `P4-T3`, must return zero matches. That is five tokens times four paths, so twenty combinations. `Path.GetRandomFileName` is used here in place of `Directory.CreateTempSubdirectory`, which is a .NET 7+ API that cannot exist in a `v4.8.1` assembly and therefore names a search no executor behaviour could ever make match. Write `<FEATURE>/evidence/qa-gates/determinism-audit.<TS>.md`. **Acceptance:** all twenty token-and-path combinations return zero matches, and the artifact records each of the twenty combinations with its match count.

- [ ] [P4-T6] Commit the source change and every evidence artifact produced so far, using explicit pathspecs: the five source paths in § Scope Lock plus `docs/features/active/quickfiler-test-uithread-dispatcher-493`. The commit message references `#493` but must not contain any GitHub closing keyword (`fixes`, `closes`, `resolves`) followed by an issue reference, including inside a negation, because a closing keyword auto-closes the issue on merge regardless of surrounding wording. **Acceptance:** the scoped `git status --porcelain` over the five § Scope Lock source paths plus `docs/features/active/quickfiler-test-uithread-dispatcher-493` produces at most one output line, which if present is the untracked `commit-1.<TS>.md` artifact this task is writing; every other path in the pathspec is clean. The full command, its output, and the resulting commit's short subject line are recorded in `<FEATURE>/evidence/qa-gates/commit-1.<TS>.md`. This task's own artifact is committed by `P5-T13`, which carries the amend step that closes the same self-reference; the strict clean-worktree gate for this feature is `P5-T13`'s `PostAmendStatus:` field, not this one.

- [ ] [P4-T7] Verify the scope lock against the committed diff. Run `git diff --name-only $BASE_SHA..HEAD -- '*.cs' '*.csproj' '*.sln' '*.props' '*.targets' '**/packages.config'`, using the `BASE_SHA` recorded by `P0-T2`. Write `<FEATURE>/evidence/qa-gates/scope-lock.<TS>.md`. **Acceptance:** the command's output is exactly the five source paths in § Scope Lock, in any order, with no sixth path; the artifact lists the returned paths verbatim and records `ProductionSourcePathCount: 0`.

---

### Phase 5 — Acceptance criteria check-off, follow-ups, and final commit

Tasks `P5-T1` through `P5-T10` each verify one acceptance criterion against evidence already
produced, then change that criterion's `- [ ]` to `- [x]` in `<FEATURE>/spec.md`, changing no other
text — one task per criterion, AC-1 through AC-10 in order. The remaining three tasks are not
check-offs and are not governed by the counting rule below: `P5-T11` and `P5-T12` discharge the
spec's follow-up items, and `P5-T13` commits.

**How the "exactly one further checkbox changed state" condition is measured.** Let `pairs(N)` be
the number of changed checkbox line pairs reported by
`git diff -- docs/features/active/quickfiler-test-uithread-dispatcher-493/spec.md` immediately
after task `P5-TN`. That count is cumulative across the phase, because none of these check-offs is
committed until `P5-T13`. Each task therefore records both `pairs(N)` and the previously recorded
`pairs(N-1)`, and its acceptance condition is `pairs(N) - pairs(N-1) == 1`. `P5-T1` uses
`pairs(0) == 0`. This mechanism is stated once here and is not restated in the individual tasks;
each of `P5-T1` through `P5-T10` refers to it only by the phrase "exactly one further checkbox in
`spec.md` changed state".

**Where each check-off task records its result.** Each of `P5-T1` through `P5-T10` writes exactly
one evidence artifact at `<FEATURE>/evidence/other/ac-checkoff-ac<N>.<TS>.md`, where `<N>` is the
number of the acceptance criterion that task checks off, so the ten stems are `ac-checkoff-ac1`
through `ac-checkoff-ac10` and each is unique. Every one of those artifacts carries `Timestamp:`,
`Command:` (the `git diff` named in the preceding paragraph), `EXIT_CODE:`, `Output Summary:`, the
resolved filename of each artifact the task cites — resolved per § Conventions — and the two fields
`PairsN:` and `PairsNMinus1:` holding `pairs(N)` and `pairs(N-1)`. That artifact is what the
counting rule above means by "records", and it is the artifact referred to by `P5-T6`'s acceptance
condition and by § Notes rule 2. This location is stated once here and is not restated in the
individual tasks. `P5-T11`, `P5-T12`, and `P5-T13` write their own separately named artifacts and
are outside this paragraph. All ten artifacts are committed by `P5-T13`, whose step-1 pathspec
already covers the whole feature folder.

- [ ] [P5-T1] Verify **AC-1**, restore exists and is idempotent, against the passing results for R2 and R3 in `<FEATURE>/evidence/regression-testing/regression-tests-pass.<TS>.md`, then check AC-1 off in `<FEATURE>/spec.md`. **Acceptance:** the cited artifact records both R2 and R3 as passed, and exactly one further checkbox in `spec.md` changed state.

- [ ] [P5-T2] Verify **AC-2**, concurrent callers cannot interleave install and restore, against the passing results for R1 and R4 in `<FEATURE>/evidence/regression-testing/regression-tests-pass.<TS>.md` and against `<FEATURE>/evidence/other/fixture-created.<TS>.md`, then check AC-2 off in `<FEATURE>/spec.md`. **Acceptance:** the cited artifacts record R1 and R4 as passed and record the fixture file's existence, and exactly one further checkbox in `spec.md` changed state.

- [ ] [P5-T3] Verify **AC-3**, bounded regression tests, against `<FEATURE>/evidence/other/regression-tests-created.<TS>.md` and `<FEATURE>/evidence/regression-testing/regression-tests-pass.<TS>.md`, then check AC-3 off in `<FEATURE>/spec.md`. **Acceptance:** the cited artifacts record the six-attribute count, six passing tests, and a non-empty `PrimaryAssertionDoc:` field naming R1 as primary and R4 as supporting — AC-3's third clause, which the other two artifacts do not cover — and exactly one further checkbox in `spec.md` changed state.

- [ ] [P5-T4] Verify **AC-4**, the #230 local workaround is removed rather than duplicated, against `<FEATURE>/evidence/qa-gates/duplicate-swap-removal.<TS>.md` and `<FEATURE>/evidence/other/part2-migrated.<TS>.md`, then check AC-4 off in `<FEATURE>/spec.md`. **Acceptance:** the cited artifacts record all three matrix rows holding, and exactly one further checkbox in `spec.md` changed state.

- [ ] [P5-T5] Verify **AC-5**, no `Thread.Sleep`, `Task.Delay`, wall-clock waits, or temporary files, against `<FEATURE>/evidence/qa-gates/determinism-audit.<TS>.md`, then check AC-5 off in `<FEATURE>/spec.md`. **Acceptance:** the cited artifact records zero matches for every audited combination, and exactly one further checkbox in `spec.md` changed state.

- [ ] [P5-T6] Verify **AC-6**, `QfcItemController.FocusAndThemeTests.cs` unmodified and unregressed, by citing three artifacts without restating their conditions: `<FEATURE>/evidence/qa-gates/unowned-file-identity.<TS>.md` for byte-identity, `<FEATURE>/evidence/qa-gates/unowned-file-diagnostics-comparison.<TS>.md` for the comparison established by `P4-T2`, and `<FEATURE>/evidence/qa-gates/quickfiler-test-run.<TS>.md` for the two named theme tests passing. Then check AC-6 off in `<FEATURE>/spec.md`. **Acceptance:** all three cited artifacts exist and record a satisfied result; this task's own `ac-checkoff-ac6` artifact repeats the baseline diagnostic counts `P4-T2` compared against, so a reviewer can see whether AC-6's diagnostic clause held absolutely or as non-regression per § Notes rule 2; and exactly one further checkbox in `spec.md` changed state.

- [ ] [P5-T7] Verify **AC-7**, `UtilitiesCS/Threading/UiThread.cs` unmodified and no production assembly changed, against `<FEATURE>/evidence/qa-gates/scope-lock.<TS>.md` and `<FEATURE>/evidence/qa-gates/unowned-file-identity.<TS>.md`, then check AC-7 off in `<FEATURE>/spec.md`. **Acceptance:** the cited artifacts record the five-path diff with `ProductionSourcePathCount: 0` and the unchanged `UiThread.cs` hash, and exactly one further checkbox in `spec.md` changed state.

- [ ] [P5-T8] Verify **AC-8**, every owned and new file at or under 500 lines with the two `<Compile Include>` entries added in the `Qfc*` neighbourhood, against `<FEATURE>/evidence/qa-gates/file-size-audit.<TS>.md` and `<FEATURE>/evidence/other/csproj-compile-entries.<TS>.md`, then check AC-8 off in `<FEATURE>/spec.md`. **Acceptance:** the cited artifacts record four measured counts at or below the ceiling and the adjacency of the two new entries, and exactly one further checkbox in `spec.md` changed state.

- [ ] [P5-T9] Verify **AC-9**, the full C# toolchain passing in a single final pass in order, against the six Phase 3 artifacts, then check AC-9 off in `<FEATURE>/spec.md` and record in the completion report the four commands run, verbatim, in the order csharpier check, analyzer msbuild, `TreatWarningsAsErrors` msbuild, vstest with coverage. **Acceptance:** all six Phase 3 artifacts exist; `P3-T1`, `P3-T2`, `P3-T3`, and `P3-T4` each record `EXIT_CODE: 0`; `P3-T5` records its observed `EXIT_CODE:` together with a satisfied subset comparison against `BaselineFailedTests`; `P3-T6` records its observed `EXIT_CODE:` together with a satisfied subset comparison against `CoverageBaselineFailedTests` and a satisfied line-rate condition in the form `P3-T6` states it; no Phase 3 artifact records `SKIPPED`; and exactly one further checkbox in `spec.md` changed state. Do **not** require `EXIT_CODE: 0` from `P3-T5` or `P3-T6`. Both run over a test assembly that also contains sibling-owned files, so an absolute all-green exit code over that assembly is unsatisfiable whenever any test outside this feature's owned set is already red — the precise possibility `P0-T12` and `P0-T13` are written to record baselines for, and restating it here as an absolute would contradict `P3-T5`, `P3-T6`, and § Notes rule 1. If either subset comparison fails — that is, a test fails that was not already failing at the Phase 0 baseline — the executor leaves AC-9 unchecked, records the offending fully-qualified test names in the completion report, and reports `BLOCKED: post-change test regression blocks AC-9` to the orchestrator rather than checking the criterion off.

- [ ] [P5-T10] Verify **AC-10**, fail-before evidence in the form the defect permits, against the `fail-before-exception` artifact produced by `P0-T14` and the `fail-before-compile` artifact produced by `P1-T4`, both under `<FEATURE>/evidence/regression-testing/` and both resolved per § Conventions, then check AC-10 off in `<FEATURE>/spec.md`. **Acceptance:** the first artifact quotes the pre-change excerpt and carries `WhyFailingRunImpossible:`, the second records a non-zero `EXIT_CODE:` with `FailBeforeErrorLineCount:` greater than zero, and exactly one further checkbox in `spec.md` changed state.

- [ ] [P5-T11] Discharge spec § Rollout & Follow-up item 2 by running `gh issue comment 584 --body-file <path>` to post a comment on issue #584 that records the injectable-seam conversion scope measured in research §7 — approximately 62 references across 29 first-party production files — and cross-links #493 as a second motivating defect. Mirror the exact posted text at `<FEATURE>/evidence/issue-updates/issue-584.<TS>.md`. Use the `gh issue comment` form named above and not `gh api ... -X POST` against the issues endpoint: the `PreToolUse` hook `.claude/hooks/enforce-promotion-mcp-only.ps1` denies the latter. **Acceptance:** the mirror carries `Timestamp:`, the exact comment text, `PostedAs: comment`, and the GitHub URL of the created comment; if posting is blocked, the mirror instead opens with a `POSTING BLOCKED` header and states the reason.

- [ ] [P5-T12] Discharge spec § Risks R-1 and § Rollout & Follow-up item 3 by promoting a follow-up bug for routing the `WpfUiDispatcherTests` static swap through the shared `UiThreadDispatcherFixture`, referencing #493 and naming `QuickFiler.Test/Controllers/WpfUiDispatcherTests.cs` as the affected file. Do **not** run `gh issue create`: the `PreToolUse` hook `.claude/hooks/enforce-promotion-mcp-only.ps1` is registered on `Bash` in `.claude/settings.json` and denies that command with `PROMOTION_MCP_ONLY_BLOCKED`, so the task cannot execute in that form. Use the MCP promotion path instead — create the potential bug entry with the `drm-copilot` potential-bug-entry tool, then promote it to an issue with the `drm-copilot` issue-promotion tool, passing an absolute `potential_path` because a workspace-relative path fails inside a worktree. Mirror the resulting issue body at `<FEATURE>/evidence/issue-updates/issue-r1-followup.<TS>.md`. **Acceptance:** the mirror carries `Timestamp:`, the exact body text, `PostedAs: body`, and the new issue's GitHub URL and number, and records the raw receipt payload returned by each of the two promotion calls; if promotion is blocked, the mirror instead opens with a `POSTING BLOCKED` header and states the reason.

- [ ] [P5-T13] Commit the `spec.md` check-offs and the Phase 5 evidence, then fold this task's own evidence artifact into the same commit. Execute in this exact order, because the artifact this task writes lives inside the pathspec it declares clean and would otherwise falsify the very condition it records: (1) `git add` and commit the check-offs and Phase 5 evidence with the explicit pathspec `docs/features/active/quickfiler-test-uithread-dispatcher-493`; (2) run `git status --porcelain -- QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixture.cs QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixtureTests.cs QuickFiler.Test/Controllers/QfcItemController.TestSupport.cs QuickFiler.Test/Controllers/QfcItemController.InitializationTests.Part2.cs QuickFiler.Test/QuickFiler.Test.csproj docs/features/active/quickfiler-test-uithread-dispatcher-493` and `git diff --name-only $BASE_SHA..HEAD -- '*.cs' '*.csproj' '*.sln' '*.props' '*.targets' '**/packages.config'`; (3) write both results to `<FEATURE>/evidence/qa-gates/commit-2.<TS>.md`; (4) `git add <FEATURE>/evidence/qa-gates/commit-2.<TS>.md` and `git commit --amend --no-edit`; (5) re-run the scoped `git status --porcelain` from step 2 and append its output to the artifact as a trailing `PostAmendStatus:` field. **Acceptance:** the step-2 diff still returns exactly the five source paths in § Scope Lock, and the artifact's `PostAmendStatus:` field is empty, meaning the worktree is clean across the full pathspec with every evidence artifact committed. The step-2 `git status` result is recorded for the audit trail but is not the gating condition; `PostAmendStatus:` is. One residual is expected and is not a defect: the executor's own check-off of `[P5-T13]` in `<FEATURE>/plan.md` necessarily follows step 5, so `plan.md` is modified again after `PostAmendStatus:` has been captured. `PostAmendStatus:` therefore certifies that the worktree was clean across the declared pathspec at the moment it was read, not that no file is modified once this task's own bookkeeping completes. That single trailing `plan.md` modification is committed by the orchestrator's PR-preparation step, and this task must not attempt a further amend to absorb it, because doing so would reopen the same self-reference the five-step order exists to close.

---

## Fixture Contract (consumed by P1-T1)

Namespace `QuickFiler.Controllers.Tests`. Required using directives: `System`,
`System.Reflection`, `System.Threading`, `System.Threading.Tasks`, `System.Windows.Threading`,
`FluentAssertions`, and `UtilitiesCS` — the last of these is `UtilitiesCS`, **not**
`UtilitiesCS.Threading`. The folder name is misleading: `UiThread` is declared in namespace
`UtilitiesCS` (`UtilitiesCS/Threading/UiThread.cs:15` is `namespace UtilitiesCS`), while
`UtilitiesCS.Threading` holds `IUiDispatcher` and `WpfUiDispatcher`
(`UtilitiesCS/Threading/IUiDispatcher.cs:7`), neither of which this file references. Transcribing
the folder-implied spelling would leave `typeof(UiThread)` unresolved with `CS0246`.
`QfcItemController.TestSupport.cs` carries both directives today (`:13` and `:14`) only because it
also uses `IUiDispatcher`; the new fixture file does not. The project targets net481
(`QuickFiler.Test.csproj:18` is `<TargetFrameworkVersion>v4.8.1</TargetFrameworkVersion>`), so
`init` accessors, `record`, and `record struct` must not be used — no framework in the 4.8.x line
carries `IsExternalInit`, and this repository defines no polyfill.

**Field declarations — every one carries the initializer shown; none may be declared without one.**

```csharp
    internal static class UiThreadDispatcherFixture
    {
        private static readonly object FieldLock = new object();
        private static readonly SemaphoreSlim TransactionGate = new SemaphoreSlim(1, 1);
        private static readonly object ParkedDispatcherLock = new object();
        private static readonly FieldInfo DispatcherField = ResolveDispatcherField();
        private static Dispatcher _parkedDispatcher = null;
```

`ResolveDispatcherField()` is declared `private static FieldInfo ResolveDispatcherField()`. It
performs the
`typeof(UiThread).GetField("_dispatcher", BindingFlags.NonPublic | BindingFlags.Static)` lookup and
asserts the result with
`field.Should().NotBeNull(because: "UiThread._dispatcher backing field must exist")` before
returning it, preserving the intent of the pre-change assertion. A static field initializer may
call a static method declared later in the same type, so its position in the file is free.

**Members.**

- `internal static Dispatcher Current` — a get-only property whose accessor has an explicit **block
  body** that reads `DispatcherField` inside `lock (FieldLock)` and returns the value. It must not
  be written as the auto-property `internal static Dispatcher Current { get; }`: an auto-property
  accessor cannot carry the required `lock`, and a get-only static auto-property that no static
  constructor assigns would not compile. Test observation only.
- `internal static Dispatcher Exchange(Dispatcher replacement)` — inside `lock (FieldLock)`, reads
  the previous value, writes `replacement`, returns the previous. Straight-line: no wait, no thread
  creation, no await inside the lock.
- `internal static bool CompareExchange(Dispatcher expected, Dispatcher restoreTo)` — inside
  `lock (FieldLock)`, writes `restoreTo` only when `ReferenceEquals` holds between the current value
  and `expected`; returns whether the write happened.
- `internal static void ReleaseTransactionGate()` — calls `TransactionGate.Release()`.
- `internal static IDisposable EnsureDispatcher()` — obtains the parked dispatcher by calling
  `GetParkedDispatcher()` **before** taking `FieldLock`; then, inside `lock (FieldLock)`, installs
  it only when the field is currently `null` and returns an `EnsureScope` carrying that instance;
  otherwise returns an `EnsureScope` carrying `null`, which is a no-op scope. Never acquires
  `TransactionGate`.
- `internal static async Task<UiThreadDispatcherTransaction> BeginTransactionAsync()` — awaits
  `TransactionGate.WaitAsync().ConfigureAwait(false)`, then returns a new, not-yet-installed
  `UiThreadDispatcherTransaction`.
- `private static Dispatcher GetParkedDispatcher()` — the body moved from
  `QfcItemControllerTestSupport.GetDedicatedDispatcher` at lines 257-285, using
  `ParkedDispatcherLock` and `_parkedDispatcher`, with the STA background thread renamed
  `UiThreadDispatcherFixture.ParkedDispatcher`.
- `private sealed class EnsureScope : IDisposable` with fields
  `private readonly Dispatcher _installed;` and `private bool _disposed = false;`. `_installed` is
  assigned in the single constructor `internal EnsureScope(Dispatcher installed)`, which also
  assigns `_disposed = false`. Its disposer is declared `public void Dispose()` — the implicit
  interface implementation of `IDisposable.Dispose` must be `public` even on a `private` nested
  type, and a narrower accessibility would fail with `CS0737`. `Dispose` returns immediately when
  `_disposed`; otherwise it sets `_disposed = true` and, only when `_installed` is not `null`,
  calls `UiThreadDispatcherFixture.CompareExchange(_installed, null)`.

**`internal sealed class UiThreadDispatcherTransaction : IDisposable`** — instance fields
`private Dispatcher _previous;`, `private Dispatcher _installedValue;`,
`private bool _hasInstalled;`, and `private bool _disposed;`. All four are definitely assigned in
the single constructor `internal UiThreadDispatcherTransaction()`, which sets `_previous = null;`,
`_installedValue = null;`, `_hasInstalled = false;`, and `_disposed = false;`. No other constructor
exists.

- `internal void Install(Dispatcher replacement)` — throws `InvalidOperationException` when
  `_hasInstalled` is already `true`; otherwise sets `_hasInstalled = true`, then assigns
  `_previous` from `UiThreadDispatcherFixture.Exchange(replacement)` and sets
  `_installedValue = replacement`. `replacement` may be `null`.
- `public void Dispose()` — returns immediately when `_disposed`; otherwise sets `_disposed = true`,
  then, only when `_hasInstalled`, calls
  `UiThreadDispatcherFixture.CompareExchange(_installedValue, _previous)`, and finally calls
  `UiThreadDispatcherFixture.ReleaseTransactionGate()`. Restore strictly precedes release. A second
  `Dispose` must neither re-write the field nor call `Release` again, because a second `Release` on
  a `SemaphoreSlim(1, 1)` throws `SemaphoreFullException`.

**Lock ordering:** `TransactionGate` then `FieldLock`, never the reverse. `FieldLock` is never held
while acquiring or awaiting `TransactionGate`, and nothing inside a `FieldLock` region blocks,
creates a thread, or awaits.

## Part2 Migration (consumed by P2-T2)

**Every line number in this section is a line number in the file as it stands at `HEAD`**, that is,
before any edit `P2-T2` makes. They identify the members to change; they are not positions to be
recomputed as the edits shift the file. `P2-T2` is the only task that edits this file, and
`P1-T1` through `P1-T4` deliberately leave it untouched, so these numbers are valid at the moment
`P2-T2` begins and are never invalidated by an earlier task.

- Lines 36-51, the `UiThreadDispatcherGate` field and its doc block, are deleted and replaced by a
  two-to-three line comment stating that the #230 serialization now lives in
  `UiThreadDispatcherFixture` and why it exists.
- `BuildPumpHarnessAsync` becomes:

  ```csharp
      internal static async Task<PumpHarness> BuildPumpHarnessAsync(
          WinFormsPumpHost host,
          bool darkMode
      )
      {
          UiThreadDispatcherTransaction transaction = await UiThreadDispatcherFixture
              .BeginTransactionAsync()
              .ConfigureAwait(false);
          try
          {
              return await BuildPumpHarnessCoreAsync(host, darkMode, transaction)
                  .ConfigureAwait(false);
          }
          catch
          {
              transaction.Dispose();
              throw;
          }
      }
  ```

  The signature is unchanged, so `QfcItemController.SeamFactoryTests.cs` lines 313 and 384 continue
  to compile.
- `BuildPumpHarnessCoreAsync` takes a third parameter `UiThreadDispatcherTransaction transaction`.
  Line 138 becomes `transaction.Install(viewer.UiDispatcher);` and the return becomes
  `return new PumpHarness(controller, viewer, cts, webView, transaction);`.
- Lines 143-158, `SwapUiThreadDispatcher` and its doc block, are deleted.
- `PumpHarness` replaces `private readonly Dispatcher _previousUiThreadDispatcher;` with
  `private readonly UiThreadDispatcherTransaction _transaction;`, assigned in its single
  constructor, whose fifth parameter changes type accordingly. The `private bool _restored;` field
  is retained unchanged. `Restore()` keeps its `_restored` guard and its body becomes
  `TokenSource.Dispose();` followed by `_transaction.Dispose();` — see Decisions Record D4.
- `using System.Reflection;` (line 4) is deleted; after this task the file has no remaining
  `FieldInfo` or `BindingFlags` reference.
- `using System.Windows.Threading;` (line 7) is deleted for the same reason. Every bare `Dispatcher`
  type name in this file today is removed by the edits above. The complete set of those type names,
  verified against the file at `HEAD`, is: the local declaration at line 138; the
  `SwapUiThreadDispatcher` return type and parameter type at line 148 and the local declaration and
  cast at line 155, both inside the block this task deletes; the `PumpHarness` field declaration at
  line 308; and the `PumpHarness` constructor parameter at line 316. Line 323 is the corresponding
  assignment `_previousUiThreadDispatcher = previousUiThreadDispatcher;`, which this task also
  rewrites but which carries no type name and is therefore not part of that set. What remains after
  the edits is `IUiDispatcher` at lines 365, 390, and 413, which comes from `UtilitiesCS.Threading`,
  not from `System.Windows.Threading`. Leaving the directive would contradict the reason this task
  deletes `using System.Reflection;`.
- `using FluentAssertions;` (line 8) is deleted for the same reason. The only FluentAssertions call
  in this file today is
  `field.Should().NotBeNull(...)` at line 154, inside the `SwapUiThreadDispatcher` body this task
  deletes; the rewritten `BuildPumpHarnessAsync`, `BuildPumpHarnessCoreAsync`, and `PumpHarness`
  members contain no assertion. This deletion is not a build requirement — `IDE0005` is not
  configured to `warning` in `.editorconfig`, so an unused directive would not fail `P3-T4` — but
  retaining it would leave dead code in a file this task is already rewriting, and would contradict
  the stated reason for deleting the other two directives.
- Every other using directive is left unchanged. In particular `using System.Threading;` is retained
  even though the `SemaphoreSlim` this task deletes came from it, because `CancellationTokenSource`
  and `CancellationToken` still do. `using UtilitiesCS;` (line 19) is retained even though the
  `UiThread` reference this task deletes came from **it** — `UiThread` is declared in namespace
  `UtilitiesCS`, not `UtilitiesCS.Threading`, despite its file path — because `IApplicationGlobals`
  (used at lines 100, 165, 206, 361, 385, and 403) also comes from `UtilitiesCS` and survives every
  edit in this task. `using UtilitiesCS.Threading;` (line 23) is likewise retained, because
  `IUiDispatcher` comes from it. Neither directive becomes dead, so neither is deleted, and the
  reason each survives is a live consumer rather than an oversight.

## Regression Tests (consumed by P1-T3)

`[TestClass] public class QfcItemController_UiThreadDispatcherFixtureTests` with
`private const int GateTimeoutMs = 60000;`. Every test carries `[Timeout(GateTimeoutMs)]` and is
declared `public async Task`. Each test that needs a known field value first calls
`UiThreadDispatcherFixture.BeginTransactionAsync()` and captures `original` from
`UiThreadDispatcherFixture.Current` **after** the transaction is acquired, so the observation is
made under the gate. Live dispatchers come from
`QfcItemControllerTestSupport.StartRunningDispatcher()` with
`QfcItemControllerTestSupport.ShutdownDispatcher(...)` in `finally`.

| # | Test name | Shape | Assertion |
| --- | --- | --- | --- |
| R1 | `EnsureDispatcher_WhileATransactionHoldsALiveDispatcher_DoesNotReplaceIt` | Begin transaction; capture `original`; `Install(liveA)`; call `QfcItemControllerTestSupport.EnsureUiThreadDispatcher()`; dispose that scope; dispose the transaction. | `Current` is `liveA` after the `Ensure` call and after disposing the `Ensure` scope; `Current` is `original` after disposing the transaction. |
| R2 | `EnsureDispatcher_WhenTheFieldIsNull_InstallsAndRestoresOnDispose` | Begin transaction; capture `original`; `Install(null)`; call `QfcItemControllerTestSupport.EnsureUiThreadDispatcher()`; dispose that scope; dispose the transaction. | `Current` is non-null after the `Ensure` call; `null` after disposing the `Ensure` scope; `original` after disposing the transaction. |
| R3 | `EnsureDispatcher_ScopeDisposedTwice_IsIdempotent` | R2's shape, likewise entered through `QfcItemControllerTestSupport.EnsureUiThreadDispatcher()`, plus a second `Dispose()` on the `Ensure` scope. | The second `Dispose` does not throw; `Current` is unchanged between the two disposals. |
| R4 | `Transaction_SecondCallerCannotInstallUntilTheFirstRestores` | Transaction A begins on the test thread, captures `original`, installs `liveA`; a `Task.Run` body begins transaction B, records `Current` immediately on acquisition before installing anything, then disposes B; the test thread disposes A and awaits the task. | B's recorded value is `original` and is never `liveA`. |
| R5 | `Transaction_DisposedTwice_DoesNotOverReleaseTheGate` | Begin a transaction, `Install(liveA)`, dispose it, dispose it again, then complete one further `BeginTransactionAsync()` and `Dispose()` round trip. | The second `Dispose` does not throw, so no `SemaphoreFullException` occurs, and the subsequent round trip completes within the `[Timeout]`. |
| R6 | `Install_CalledTwiceOnTheSameTransaction_ThrowsInvalidOperationException` | Begin a transaction; `Install(null)`; call `Install(liveA)` a second time; dispose in `finally`. | The second `Install` throws `InvalidOperationException`. |

R1, R2, and R3 enter through `QfcItemControllerTestSupport.EnsureUiThreadDispatcher()` rather than
through `UiThreadDispatcherFixture.EnsureDispatcher()` directly. This is required, not stylistic:
spec § Test Strategy "Fail-before evidence" rests on R1 and R2 being unable to compile against the
base branch, which is true only of the wrapper, whose return type is `void` at `HEAD`. Entering
through the fixture would compile at `HEAD` and would silently void the premise `P0-T14` and
`P1-T4` record. R3 follows R2's shape for the same reason.

R1 is documented in the test file's own XML comment as the primary deterministic regression
assertion and R4 as the supporting one, matching spec § Test Strategy "Honest limitation of R4":
under a broken implementation R4 fails only probabilistically, whereas R1 proves the clobber is
unreachable with no concurrency at all. That documentation is an acceptance criterion in its own
right (spec AC-3 requires it), so `P1-T3` records the sentence and `P5-T3` verifies the record.
