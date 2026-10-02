# 2026-10-02-focus-and-theme-tests-leak-shared-dispatcher-setup (Plan)

- **Issue:** #968
- **Parent (optional):** none
- **Owner:** drmoisan
- **Work Mode:** full-bug (acceptance criteria come from `docs/features/active/2026-10-02-focus-and-theme-tests-leak-shared-dispatcher-setup-968/spec.md` only; no user story exists for this item and none is to be authored)
- **Last Updated:** 2026-10-02T07-30
- **Status:** Ready for preflight (revision round 0)
- **Version:** 1.0
- **Plan path continuity:** this file is updated in place for every preflight revision round. No timestamped sibling plan file is created for this cycle.

**Fail-closed evidence rule:** every command-bearing task writes one evidence artifact carrying `Timestamp:`, `Command:`, `EXIT_CODE:` and `Output Summary:`. A task whose artifact is missing or incomplete stays unchecked, and the plan outcome is BLOCKED or INCOMPLETE, never PASS.

**Evidence accounting rule:** the artifact path is named in the task text. Do not mark an evidence-bearing task complete without the artifact on disk at that exact path.

**Evidence location:** every artifact lives under `docs/features/active/2026-10-02-focus-and-theme-tests-leak-shared-dispatcher-setup-968/evidence/` in the canonical sub-kinds `baseline/`, `regression-testing/`, `qa-gates/` and `other/`. EVIDENCE_LOCATION_OVERRIDE_REJECTED: none supplied; no artifacts-tree evidence path appears in this plan. In task text the token FEATURE abbreviates `docs/features/active/2026-10-02-focus-and-theme-tests-leak-shared-dispatcher-setup-968`; the Write Set spells every path in full.

## Caller instructions applied with a recorded adjustment

1. **Absolute worktree path in `Command:` rows.** The repository hygiene guard (`scripts/hygiene/Test-RepositoryHygiene.Rules.ps1`, function `Get-UserProfilePathPattern`, line 21) rejects any committed line matching a drive-letter user-profile path, and both this plan and every evidence artifact are committed. Every `Command:` row therefore records the payload with the literal token `WORKTREE` in place of the absolute path, and every payload prints `WORKTREE-LEAF:` followed by the leaf name of its working directory. The acceptance condition is that `WORKTREE-LEAF: agent-a291a7fbabf9d0229` is recorded; a payload that ran in another tree prints a different leaf and fails that condition.
2. **Quote character of the pwsh channel.** The caller's example uses double quotes around the command string. Payloads contain `$`, so they are passed in outer single quotes (`pwsh -NoProfile -Command '...'`) and use double quotes only inside; the Bash channel cannot carry a single quote inside a single-quoted argument, so no payload and no asserted token contains an apostrophe.
3. **D1 placement.** The caller lists D1 to D6 with the Phase 3 edits. D1 (the fixture's own doc comments) is applied in Phase 2 together with the fixture fix, because it edits the same file and the same regions; D2 to D6 remain in Phase 3.
4. **Spec amendment.** The orchestrator's correction to the spec is applied by this planner in `docs/features/active/2026-10-02-focus-and-theme-tests-leak-shared-dispatcher-setup-968/spec.md` as amendment 1.1 (header note, decision 4, the R4 bullet, the census criterion, labels `AC1:` to `AC24:` on the twenty-four acceptance lines, and the sibling sentences listed in the planner's handoff message). P0-T2 verifies the amended text is the text on disk.

## Requirement sources

- Acceptance criteria: `docs/features/active/2026-10-02-focus-and-theme-tests-leak-shared-dispatcher-setup-968/spec.md`, section `## Acceptance Criteria`: twenty-four checkbox lines `- [ ] AC1:` through `- [ ] AC24:`, each on one line. The check-off edit changes only `- [ ] ACn:` to `- [x] ACn:`.
- Design record: `docs/features/active/2026-10-02-focus-and-theme-tests-leak-shared-dispatcher-setup-968/research/2026-10-02T05-50-dispatcher-pin-call-sites-research.md` (sections 1, 2.1, 3, 4, 5 and 6 govern this plan; read-only).
- Issue metadata: `docs/features/active/2026-10-02-focus-and-theme-tests-leak-shared-dispatcher-setup-968/issue.md` carries `- Work Mode: full-bug` at line 12 and no acceptance-criteria section. It is not an acceptance-criteria source for this cycle.
- Structural reference: `docs/features/active/2026-09-30-quickfiler-tests-depend-on-wall-clock-timing-950/plan.2026-10-01T07-11.md` and its evidence folder (bootstrap, command-macro, coverage-route and evidence conventions); every citation below was re-derived against this worktree, not carried from that plan.

## Write Set (every file this plan creates or modifies)

Code files (the only paths outside the feature folder this plan may change; spec "Files/modules to change"):

- `QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixture.cs` (modified: pin counter, ownership flag, counted dispose, D1 docs)
- `QuickFiler.Test/Controllers/QfcItemController.FocusAndThemeTests.cs` (modified: two dead calls deleted, D5, D6)
- `QuickFiler.Test/Controllers/QfcItemController.TestSupport.cs` (modified: D2 wrapper doc, D5 shared-helper doc)
- `QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixtureTests.cs` (modified: D3 doc rewrite, R4 pin removal, D4 try/finally)
- `QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherPinCountTests.cs` (new: regression test and three specification tests)
- `QuickFiler.Test/QuickFiler.Test.csproj` (modified: one new `Compile Include` item)

No production file changes. No other project file changes.

Feature documents (committed by this plan; the research record, issue.md and spec.md are untracked at authoring time and enter the tree at the Phase 0 commit):

- `docs/features/active/2026-10-02-focus-and-theme-tests-leak-shared-dispatcher-setup-968/spec.md` (amendment 1.1 applied by the planner; acceptance-criteria check-off edits at execution)
- `docs/features/active/2026-10-02-focus-and-theme-tests-leak-shared-dispatcher-setup-968/issue.md` (unchanged content, committed)
- `docs/features/active/2026-10-02-focus-and-theme-tests-leak-shared-dispatcher-setup-968/research/2026-10-02T05-50-dispatcher-pin-call-sites-research.md` (unchanged content, committed)
- `docs/features/active/2026-10-02-focus-and-theme-tests-leak-shared-dispatcher-setup-968/plan.2026-10-02T05-42.md` (task check-off edits only)

Evidence files, all new, all fixed names:

- `FEATURE/evidence/baseline/`: `phase0-instructions-read.md`, `scope-and-anchor.md`, `bootstrap-sdk.md`, `bootstrap-tool-restore.md`, `bootstrap-nuget-restore.md`, `analyzer-alignment.md`, `bootstrap-dotnet-coverage.md`, `csharpier-check-baseline.md`, `msbuild-analyzer-baseline.md`, `msbuild-nullable-baseline.md`, `census-baseline.md`, `concurrent-set-baseline.md`, `stall-probe.md`, `coverage-summary.md`, `coverage-jacoco-projection.md`, `toolchain-baseline.md`, `phase0-commit.md`
- `FEATURE/evidence/regression-testing/`: `pin-count-file-census.md`, `fail-before-build.md`, `fail-before-pin-count.md`, `specification-tests-before-fix.md`, `pass-after-build.md`, `pass-after-pin-count.md`, `implementation-build.md`, `pin-count-class-pass-after.md`, `fixture-class-pass-after.md`, `focus-and-theme-class-pass-after.md`, `concurrent-set-test-summary.md`
- `FEATURE/evidence/qa-gates/`: `fixture-change-census.md`, `test-edit-census.md`, `scoped-format.md`, `post-format-census.md`, `implementation-commit.md`, `call-site-census.md`, `prohibited-constructs-grep.md`, `csharpier-format-final.md`, `csharpier-check-final.md`, `msbuild-analyzer-final.md`, `msbuild-nullable-final.md`, `coverage-summary.md`, `coverage-jacoco-projection.md`, `coverage-comparison.md`, `toolchain-final.md`, `file-line-counts.md`, `footprint-scope.md`, `evidence-hygiene.md`, `final-commit.md`
- `FEATURE/evidence/other/`: `ac-status-summary.md`

Files this plan must not touch, stated so the executor fails closed rather than infers: every file under QuickFiler/ (production), UtilitiesCS/Threading/UiThread.cs, UtilitiesCS/HelperClasses/ThemeHelpers/Theme.cs, QuickFiler.Test/Helper Classes/EmailMoveMonitorTests.cs, every other file under QuickFiler.Test/ not listed above, TaskMaster.runsettings, scripts/vscode/TaskMaster.cli.runsettings, every file under scripts/, every file under .github/, every file under .claude/ (uncommitted .claude/agent-memory/ files are session memory and are never staged by this plan), every file under docs/features/potential/, and the research document's content. No raw test-result document (trx), raw coverage document (cobertura, coverage, coveragexml) or msbuild log is copied into the feature folder under any name; raw documents stay under the repository coverage directory, which .gitignore line 150 ignores.

## AC identity table

Each ID names one checkbox in the spec's `## Acceptance Criteria` section, in document order; the label `ACn:` is part of the line text after amendment 1.1.

| ID | Opening words of the criterion | Evidence read by its check-off task |
|---|---|---|
| AC1 | Counted pin, non-last release | `regression-testing/pass-after-pin-count.md`, `qa-gates/coverage-summary.md` |
| AC2 | Counted pin, last release reverts only the fixture's own seeding | same two artifacts |
| AC3 | A foreign transaction value is never nulled by pin release | same two plus `regression-testing/fixture-class-pass-after.md`, `qa-gates/post-format-census.md` |
| AC4 | Ownership flag is cleared on the last release | `regression-testing/pass-after-pin-count.md`, `qa-gates/coverage-summary.md` |
| AC5 | Fail-before and pass-after evidence isolates the fixture change | `regression-testing/fail-before-pin-count.md`, `regression-testing/pass-after-pin-count.md`, `regression-testing/pin-count-file-census.md` |
| AC6 | The pin-count test class labels its tests | `qa-gates/post-format-census.md` |
| AC7 | The dead theme-test calls are removed | `qa-gates/post-format-census.md`, `regression-testing/focus-and-theme-class-pass-after.md` |
| AC8 | Gated-caller census invariant holds | `qa-gates/call-site-census.md` |
| AC9 | The pin counter and install-ownership flag are private statics | `qa-gates/post-format-census.md` |
| AC10 | All existing fixture tests pass unchanged in behaviour | `regression-testing/fixture-class-pass-after.md`, `qa-gates/post-format-census.md` |
| AC11 | Fixture documentation describes the counted pin | `qa-gates/post-format-census.md` |
| AC12 | Wrapper documentation describes the counted pin | `qa-gates/post-format-census.md` |
| AC13 | The second-caller transaction test's doc no longer asserts the obsolete invariant | `qa-gates/post-format-census.md` |
| AC14 | The second-caller transaction test releases its gate on any throw | `qa-gates/post-format-census.md`, `regression-testing/fixture-class-pass-after.md` |
| AC15 | The duplicated viewer helper is removed | `qa-gates/post-format-census.md`, `regression-testing/focus-and-theme-class-pass-after.md` |
| AC16 | The theme-test arrange comment is corrected | `qa-gates/post-format-census.md` |
| AC17 | `EnsureSynchronizationContext` is unchanged | `qa-gates/post-format-census.md` |
| AC18 | File-size limit | `qa-gates/file-line-counts.md` |
| AC19 | No prohibited constructs | `qa-gates/prohibited-constructs-grep.md` |
| AC20 | No production code change | `qa-gates/footprint-scope.md` |
| AC21 | The new test file is built and discovered | `qa-gates/post-format-census.md`, `qa-gates/coverage-summary.md` |
| AC22 | Full toolchain pass | `qa-gates/toolchain-final.md` |
| AC23 | Coverage not reduced | `qa-gates/coverage-comparison.md` |
| AC24 | Parallel run of the three classes together passes | `regression-testing/concurrent-set-test-summary.md` |

## Verified tree facts (re-derived against this worktree while authoring)

Line totals are content-line counts (the Grep tool's count of lines matching `^`, which equals `git grep -c ""`); every one of the four existing files ends every line with a carriage return (CRLF), and `.gitattributes` line 4 sets `* text=auto`.

1. `QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixture.cs` is 342 lines. Class doc 12 to 31 (`/// </para>` at 30, `/// </summary>` at 31; design note on `EnsureDispatcher` never taking the gate at 25 to 30). Statics 34 to 38 (`_parkedDispatcher` at 38), counters 44 to 46. `Exchange` 77 to 85; `CompareExchange` declared at 92 with its `lock (FieldLock)` at 94. `EnsureDispatcher` doc 116 to 121 (line 120: `returned scope is optional: a discarded scope leaks exactly as the pre-fix helper did.`); declaration 122; comment 124 to 125; `Dispatcher parked = GetParkedDispatcher();` 126; blank 127; body 128 to 137 (`lock (FieldLock)` 128, `if (DispatcherField.GetValue(null) == null)` 130, `return new EnsureScope(parked);` 133, `return new EnsureScope(null);` 137); method close 138. `TransactionGateAcquireTimeoutMs` at 146. Parked thread name at 231. `EnsureScope` doc 243 to 248 (line 245: `static still holds the exact instance this scope installed. A scope that installed nothing`, line 246: `carries <c>null</c> and is a no-op, which is what keeps a discarded scope from clobbering a`); class 249 to 274 with `UiThreadDispatcherFixture.CompareExchange(_installed, null);` at 271. `UiThreadDispatcherTransaction` declared at 284, its `CompareExchange` call at 336. Counts: `lock (FieldLock)` 4 (66, 79, 94, 128); `CompareExchange(` 3 (92, 271, 336); `return new EnsureScope(` 2; `leaks exactly` 1; `installed nothing carries` 0 as a single-line token (the phrase wraps across 245 and 246, so the AC11 grep is satisfied vacuously before the change and the gate therefore uses the two single-line tokens `leaks exactly` and `A scope that installed nothing`, each 1 before and 0 after); `_pinCount` 0; `_fixtureInstalledParked` 0; `pins for the process lifetime` 0; `install-ownership flag` 0; bare `EnsureDispatcher` 5 lines (26, 27, 122, 195, 244).
2. `QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixtureTests.cs` is 470 lines. `private const int GateTimeoutMs = 60000;` at 33; `[Timeout(GateTimeoutMs)]` 8 occurrences; `[TestMethod]` 8. R1 declared 44 (transaction 50 to 52, `Install(liveA)` 56, pin 59 to 60 with `QfcItemControllerTestSupport.EnsureUiThreadDispatcher();` on 60, `ensureScope.Dispose();` 62, `transaction.Dispose();` 81 and 91, `ShutdownDispatcher(liveA)` 96). R2 declared 107 (transaction 110 to 112, `Install(null)` 116, pin 119, dispose 121, `transaction.Dispose();` 137 and 147). R3 declared 157 (transaction 160 to 162, `Install(null)` 165, pin 166, disposes 169 and 171, `transaction.Dispose();` 188). R4 doc 192 to 209 with the `<para>` 196 to 208 (line 205: `/// Invariant for future editors: no other class may dispose an ensure scope holding the`, line 206: `/// parked dispatcher (W2), and UiThread.Initialize (W5) must not latch during this test;`); attributes 210 and 211; declared 212; `// Arrange` 214; `liveA` 215; `try` 216; `{` 217; transaction A 218 to 220; `using (` 221; `IDisposable baseline = QfcItemControllerTestSupport.EnsureUiThreadDispatcher()` 222; `)` 223; `{` 224; `Dispatcher original = UiThreadDispatcherFixture.Current;` 225; `transactionA.Install(liveA);` 226 (unique in the file); `using (var secondCallerStarted = new ManualResetEventSlim(false))` 228; waiter 232 to 247 with its `finally` at 243 and `transactionB.Dispose();` 245; `secondCallerStarted.Wait();` 250; `transactionA.Dispose();` 251 (unique); assertions 255 to 268 with `issue #230 lost update` at 267 (unique in the file); `}` 269 (closes the `secondCallerStarted` using), `}` 270 (closes the `baseline` using), `}` 271 (closes the `try`), `finally` 272, `{` 273, `QfcItemControllerTestSupport.ShutdownDispatcher(liveA);` 274, `}` 275, method close 276. R5 declared 285. `EnsureUiThreadDispatcher()` 4 lines (60, 119, 166, 222); bare `EnsureDispatcher` 10 lines (44, 60, 70, 107, 119, 128, 157, 166, 198, 222); `no other class may dispose` 1; `(W5) must not latch` 1; `.BeginTransactionAsync()` 12 lines in the file. No `Issue #968` text.
3. `QuickFiler.Test/Controllers/QfcItemController.FocusAndThemeTests.cs` is 497 lines, class `QfcItemController_FocusAndThemeTests` at 27, `[TestMethod]` 17. Private `BuildExecutingViewer` 99 to 115 with blank lines at 98 and 116; `/// <summary>` of `EnableHandlelessThemeInvoke` at 117. Comment block 181 to 186 (line 182 ends `BuildExecutingViewer() executes the`). `var viewer = BuildExecutingViewer();` at 193, 213, 235, 254, 314, 331 and 367 (seven lines, all with twelve leading spaces). `SetThemeDark_FromNormal_SelectsDarkNormalTheme` 448 to 462: comment 450 to 451, `QfcItemControllerTestSupport.EnsureUiThreadDispatcher();` 452, `var controller = new FocusController();` 453. `SetThemeLight_FromNormal_SelectsLightNormalTheme` 465 to 478: `// Arrange` 467, the ensure call 468, `var controller = new FocusController();` 469. Counts: `EnsureUiThreadDispatcher` 2; `BuildExecutingViewer` 9 (99, 182, and the seven callers); `private static Mock<IItemViewer> BuildExecutingViewer` 1; `QfcItemControllerTestSupport.BuildExecutingViewer()` 0.
4. `QuickFiler.Test/Controllers/QfcItemController.TestSupport.cs` is 440 lines. `EnsureSynchronizationContext` doc 85 to 89, method 90 to 96. `BuildColorTheme` remarks 161 to 168 and method 169 to 181 (injects a `Mock<IUiDispatcher>` whose `InvokeAsync` returns `Task.CompletedTask` at 175 to 179). `EnsureUiThreadDispatcher` doc 216 to 237 (`Becomes moot` at 226, `leaks exactly` at 233, `still delegate to a callee` at 219), declaration 238, body `UiThreadDispatcherFixture.EnsureDispatcher();` 239. `BuildExecutingViewer` doc 282 to 288 (line 287: `/// <c>QfcItemController.FocusAndThemeTests.cs</c>, which is not reachable from another test file.`), method 289 to 305. `StartRunningDispatcher` 251 to 271; `ShutdownDispatcher` 277 to 280. Bare `EnsureDispatcher` 2 lines (238, 239). The only other caller of the shared helper is `QuickFiler.Test/Controllers/QfcItemController.MailActionsTests.cs` line 203.
5. Repository-wide call sites (Grep over `*.cs`, pattern `EnsureUiThreadDispatcher|EnsureDispatcher`): 20 lines in 5 files, exactly as research section 2.1 records (fixture 5, test support 2, fixture tests 10, focus-and-theme 2, `QuickFiler.Test/Controllers/QfcItemController.InitializationTests.Part2.cs` line 124, a comment). Lines matching `EnsureUiThreadDispatcher\(\)|EnsureDispatcher\(\)`: 9 (fixture 122, test support 238 and 239, fixture tests 60, 119, 166, 222, focus-and-theme 452, 468). `BeginTransactionAsync\(` matches 23 lines in 6 files (the census positive control). `[DoNotParallelize]` occurs in QuickFiler.Test only at `Helper Classes/EmailMoveMonitorTests.cs` 24 and `Helper Classes/ViewerQueueStaticWrapperTests.cs` 11, both outside the Write Set.
6. Theme path (read-only context for D6): `QuickFiler/Controllers/QfcItemController.FocusAndTheme.cs` 274 to 286 (`SetThemeDark` calls `_themes["DarkNormal"].SetQfcTheme(async)` then sets `_activeTheme`); `UtilitiesCS/HelperClasses/ThemeHelpers/Theme.cs` 427 to 445 (`SetQfcTheme(bool async)`: the async branch is `_uiDispatcher.InvokeAsync(() => SetQfcTheme());` at 431; the former static read is the commented line 441); `UtilitiesCS/Threading/UiThread.cs` 266 to 285 (the `Dispatcher` getter throws `InvalidOperationException` when `_dispatcher` is null; private backing field at 285). No statement on the theme path reads `UiThread.Dispatcher`.
7. `QuickFiler.Test/SetupAssemblyInitializer.cs` 14 to 25: `[AssemblyInitialize]` installs an assembly resolver and WinForms rendering defaults and does not write `UiThread._dispatcher`, so a class run alone starts from a null baseline.
8. `QuickFiler.Test/QuickFiler.Test.csproj`: `<TargetFrameworkVersion>v4.8.1</TargetFrameworkVersion>` at 17, `OutputPath` `bin\Debug\` at 35, no `LangVersion` element; `Compile Include` items for the Write Set files at 200 (TestSupport), 201 (fixture), 203 (fixture tests), 212 (focus-and-theme), each with four leading spaces; no item for the pin-count file.
9. `scripts/vscode/TaskMaster.cli.runsettings` lines 4 to 7 set Workers 0 and Scope ClassLevel. `scripts/vscode/Invoke-MSTestWithCoverage.ps1` (461 lines): `Get-DotnetCoverageArgumentList` appends `/InIsolation`, `/TestCaseFilter:TestCategory!=LiveOutlook`, the results directory and the trx logger at 89 to 93 with no extension point; `Invoke-DotnetCoverageCollection` throws `MSTest with coverage failed with exit code` at 262 after the collector exits non-zero, before post-processing; defaults `coverage\test-results` and `mstest-coverage-run.trx` at 297 to 298; discovery 348 to 355 filters `bin\Debug` and a `.claude` segment; post-processing 399 to 402; `First-party coverage:` printed at 410; projection 415 to 423; trx summary 430 to 447; entry guard 459 to 461 so dot-sourcing is safe. Helper functions: `Get-TrxRunSummary` and `Format-TrxRunSummary` in `scripts/vscode/Invoke-MSTest.TrxSummary.ps1` (12, 103); `ConvertTo-KoverageCoberturaXml` in `scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1` (407); `Get-CoberturaFirstPartyCoverageReport` in `scripts/vscode/Invoke-MSTestWithCoverage.FirstParty.ps1` (123; the line format `First-party coverage: lines a/b (p%), branches c/d (q%)` at 117 to 120); `Assert-CoberturaLineCoverageThreshold` and `Assert-CoberturaBranchCoverageThreshold` in `scripts/vscode/Invoke-MSTestWithCoverage.Threshold.ps1` (3, 58); `ConvertTo-JacocoPackageProjection` and `Assert-JacocoProjectionReconciliation` in `scripts/vscode/Invoke-MSTestWithCoverage.Projection.ps1` (14, 83). The Helpers file dot-sources its part files, so dot-sourcing it alone resolves the FirstParty, Threshold and Projection functions, as the #950 run's CMD-COVERAGE-POST showed.
10. `.gitignore`: `*.coverage` 140, `*.coveragexml` 141, `*.trx` 146, `coverage/*` 150, `!coverage/.gitkeep` 151, `[Bb]in/` 26. `.csharpierignore` excludes `**/evidence/**` (4) and `*.csproj` (12). `global.json` pins SDK 8.0.205 under `.dotnet-sdk` with `latestFeature` roll-forward (lines 2 to 9); `dotnet-tools.json` at the repository root pins csharpier 1.2.6 (line 6). `scripts/vscode/Install-RepoDotNetSdk.ps1` defaults `-Version` to `8.0.205` (line 3); `scripts/vscode/Invoke-Restore.ps1` takes `SolutionPath`, `Configuration` and `Platform` (lines 1 to 10). `coverage.config` exists at the repository root.
11. Branch `bug/focus-and-theme-tests-leak-shared-dispatcher-setup-968`; BASE `94287369908cc920b21b0e3256314f988ad7d2f5` (origin/main at the branch cut, supplied by the caller and confirmed by research section header). Commits already on the branch above BASE are documentation and agent-memory commits (the harness lists `3956fa351`, `4f9da66ea`, `89f0283c1`, `c0d74296b`, `17ad045a1`); `docs/features/potential/promoted/2026-10-02-focus-and-theme-tests-leak-shared-dispatcher-setup.md` exists on the tree. HEAD is recorded as an observation in P0-T3, never as an expectation. This planning session has no shell, so the `.dotnet-sdk`, `packages` and `bin` trees were not probed; every bootstrap task is guarded and gated on its post-task marker.
12. Host constraint carried from #950 on this workstation (its `evidence/baseline/stall-probe.md`): the four UtilitiesCS.Test shell-icon classes did not stall but one test failed with `Win32 handle that was passed to Icon is not valid`, so the runner route (which throws before post-processing on any failure) could not produce figures and the DIRECT route was used. Whether that reproduces today is unknown, so P0-T14 measures it and the result selects the coverage route (D-6).
13. The #950 run observed the FluentAssertions `BeSameAs` failure shape on this fixture: `Expected observedByB to refer to <null> because ..., but found System.Windows.Threading.Dispatcher { ... Name = "UiThreadDispatcherFixture.ParkedDispatcher" ... }`. The pin-count regression inverts the operands, so its expected message is `Expected afterFirstRelease to refer to System.Windows.Threading.Dispatcher { ... Name = "UiThreadDispatcherFixture.ParkedDispatcher" ... } because a holder that did not take the last pin must not lose the dispatcher, but found <null>.`; the gate reads `to refer to`, `ParkedDispatcher`, the because text and `but found <null>`, never the subject name (caller identification can fail and then prints `object`).

## Design decisions (do not redesign)

- **D-1 Fixture fix (spec decision 1, research Approach A).** `UiThreadDispatcherFixture` gains two private statics without initializers, `_pinCount` and `_fixtureInstalledParked`, read and written only inside `lock (FieldLock)` (Delivered Source F-FIELDS). `EnsureDispatcher` increments the count and, when the field is null, writes the parked dispatcher and sets the flag; it always returns `new EnsureScope(parked)` (F-ENSURE-BODY). `EnsureScope` keeps the parked reference; its idempotent `Dispose` decrements under `FieldLock` and, in the same critical section, writes null and clears the flag only when the count reaches zero, the flag is set and the field still references the parked instance (F-SCOPE). The nested class reaches `FieldLock`, `DispatcherField` and the two statics directly, so the re-locking `CompareExchange` helper is not called from the scope. D1 docs: F-CLASSDOC, F-ENSURE-DOC and the F-SCOPE doc.
- **D-2 R4 restructure: option (a), the baseline pin is removed.** The `using (IDisposable baseline = ...)` block that #950 added to `Transaction_SecondCallerCannotInstallUntilTheFirstRestores` is deleted and replaced by a `try/finally` over `transactionA` (Delivered Source R-BODY); the body lines keep their indentation because the `try` block replaces the `using` block one-for-one. Rationale: the pin fenced the gate-free writers of the two theme tests, which Phase 3 deletes; after the deletion the census (P5-T1, P5-T2) proves every remaining pin is acquired and released inside a held transaction, so while `transactionA` holds the gate no other class can write the field and every other transaction restores before it releases, which is all R4's two assertions need; the pin as placed was released after `transactionA.Dispose()` and after `transactionB` completed, so it outlived its gate hold, and it installed `liveA` over a pinned parked value, the only shape in the repository reaching the flag-true-but-field-changed branch. Option (b), releasing the pin before `transactionA.Dispose()`, was rejected because it keeps that shape and leaves the parked dispatcher installed with the flag set after every R4 run. R4's two assertions and their `because` texts are unchanged (gated by R4SPAN tokens).
- **D-3 Fail-before evidence.** The new test file is compiled against the unmodified fixture (Phase 1) and test 1 is run alone by fully qualified name, tagged `[expect-fail]` (P1-T5); tests 2 to 4 are run on the same unmodified fixture and must pass (P1-T6), which is the evidence for their "passes before and after" labels. The fixture fix alone is then applied (Phase 2) and the four tests are run again (P2-T8). The porcelain spans recorded at P1-T3 and P2-T8 show that the only difference between the two runs is the fixture file.
- **D-4 Temporary state and byte-level checks.** No temporary edits are made in this plan; every edit is a delivered edit. Build gates use `/t:Build` for the scoped test runs (CMD-BUILD, gated on the test assembly timestamp advancing) and `/t:Rebuild` for the baseline and final analyzer and nullable gates (CMD-REBUILD, gated on zero `Skipping target "CoreCompile"` lines and at least one `Csc` output line per Write Set project).
- **D-5 Hang handling.** Every direct vstest run carries `/Blame:CollectHangDump;TestTimeout=4min;HangDumpType=None`; a `Sequence_*.xml` file in the results directory, or a `Timeout` or `Aborted` outcome, is a stop.
- **D-6 Coverage route is selected by a recorded observation.** P0-T14 runs the four shell-icon classes alone and records `STALL-PROBE: CLEAR` or `REPRODUCES`. `COVERAGE-ROUTE: RUNNER` (CLEAR) runs `scripts/vscode/Invoke-MSTestWithCoverage.ps1` verbatim (CLAUDE.md step 4). `COVERAGE-ROUTE: DIRECT` (REPRODUCES) issues the runner's own inner collector invocation with the four-class exclusion appended and post-processes with the runner's own helpers, because the runner hard-codes its filter (fact 9). Both routes yield the same committed forms: the `First-party coverage:` line, the JaCoCo package projection text and the trx-derived summary, transcribed into Markdown. Under DIRECT, AC22 cannot be met as worded (the runner was not run) and its check-off records `AC22: NOT MET (ENVIRONMENTAL: COVERAGE-ROUTE DIRECT)` for the orchestrator's decision, as the #950 run did.
- **D-7 Coverage obligations.** Every changed file is in a test assembly, which the runner's derived settings exclude from instrumentation (`.*\.Test\.dll$`), so no changed line has a coverage figure: `CHANGED-CODE-COVERAGE: NOT MEASURED (TEST ASSEMBLY EXCLUDED)` is recorded at both stages. The first-party line and the root counters are recorded at both stages; the repository-wide rate is compared in two branches (denominators within 1 percent: tolerance 0.5 percentage points; otherwise recorded, not gated), because that rate is not reproducible across runs of an identical tree. AC23 is worded without a tolerance, so its check-off is MET only when both printed first-party percentages are not lower than baseline, otherwise NOT MET with the figures and `COVERAGE-VARIANCE` recorded for the orchestrator. The 80 percent line and 75 percent branch floors are applied by the runner or by its threshold functions under DIRECT.
- **D-8 Baseline-relative test outcomes.** The baseline full run (P0-T15) may contain pre-existing failures; they are recorded as `BASELINE-FAILED-SET:` and do not stop Phase 0. The final run passes only when its failed set contains no name absent from the baseline failed set (`NEW-FAILURES: NONE`) and all fourteen target tests (NAMES-TARGETS) are `Passed`. AC22 additionally requires the runner route and exit 0.
- **D-9 Anchor.** Every diff gate uses `94287369908cc920b21b0e3256314f988ad7d2f5` as its ref operand (`BASE` in prose). P0-T3 verifies it is an ancestor of HEAD and equals `git merge-base origin/main HEAD`; a mismatch is `BASE-SHA MISMATCH` (ancestor check fails) or `BASE AHEAD OF BRANCH` (merge-base differs): stop and report, because `refs/remotes/origin/main` is shared by every worktree and a fetch elsewhere can move it. Paths changed between BASE and HEAD at P0-T3 form the inherited set `INHERITED-COMMITTED:`; each must be under FEATURE, under `docs/features/potential/`, or under `.claude/agent-memory/`, otherwise `INHERITED SET OUT OF SCOPE`: stop. Late in the run P6-T9 re-reads `git rev-parse origin/main`; a value other than BASE is recorded as `BASE REF MOVED` and does not stop the run, because every gate names BASE explicitly.
- **D-10 Commits.** Three commits: P0-T17 (FEATURE only), P4-T8 (the six Write Set code files plus FEATURE, staged after the P4-T1 scoped format so the committed text is formatter-stable), P6-T38 (FEATURE). Each is a `git -C WORKTREE add -- <pathspecs>` invocation followed by a separate `git -C WORKTREE commit -m "<message>"` invocation, one command per call, never chained, never `git add -A`. No commit message contains an angle bracket, a dollar sign or a backtick. A PreToolUse refusal of any `git add`, `git commit`, `.cs` edit, `.csproj` edit or spec edit is recorded verbatim as `PRE-IMPLEMENTATION GATE BLOCKED` and stops the run; the executor does not retry with a rephrased edit or another tool and does not modify hooks, checkpoints or permission configuration.
- **D-11 Git gates are pathspec-scoped and anchored.** Every `git diff` names BASE or HEAD as its ref operand; every name-listing diff is paired with a porcelain span in the same task; no gate asserts an unscoped empty porcelain. Uncommitted `.claude/agent-memory/` paths and this plan file may appear in porcelain output and are admitted by every scope gate; their count is deliberately unrecorded.
- **D-12 Check-offs follow the loop.** Every check-off task sits in Phase 6 after the final toolchain pass and reads an artifact that survived it. Each flips exactly one checkbox and, when its evidence does not hold, completes with the box unchecked and records `ACn: NOT MET` with the reason in `FEATURE/evidence/other/ac-status-summary.md`.
- **D-13 Restart rules.** Phase 4: if P4-T4, P4-T5, P4-T6 or P4-T7 shows a target test not `Passed`, the executor corrects the Write Set file at fault (within the delivered design; no prohibited construct) and restarts at P4-T1, recording `P4-RESTART: n` in the restarted artifacts. Phase 6: `ITERATION` starts at 1. If P6-T1 rewrites any Write Set file, the executor commits exactly the rewritten Write Set files (`style(968): apply csharpier output`), increments ITERATION and restarts at P6-T1. If P6-T2, P6-T3, P6-T4 or P6-T5 fails because of a Write Set file, the executor corrects it, restarts at P4-T1 (scoped format, census, build, pass-after runs, commit), re-runs Phase 5 in full, increments ITERATION and resumes at P6-T1. A rewrite of a file outside the Write Set, or a failure not attributable to the Write Set beyond the D-8 baseline rule, is a stop with the failing artifact. P6-T7 records the final iteration only.
- **D-14 Line endings of the new file.** The four sibling files are CRLF in the worktree (fact header). P1-T1 normalises the new file to CRLF with CMD-EOL after writing it, so `csharpier check .` and the committed blob (normalised to LF by `text=auto`) behave as for the siblings; the gate is `BARE_LF: 0`.

## Risks (recorded; no mitigation is in scope)

- **AC22 under the DIRECT route.** If P0-T14 records `REPRODUCES`, the runner is not run and AC22 ends `NOT MET (ENVIRONMENTAL: COVERAGE-ROUTE DIRECT)`; the orchestrator decides, as for #950 AC17.
- **AC23 has no tolerance.** The repository-wide first-party rate varied by 0.01 percentage points between two #950 runs of an identical instrumented tree; a downward variation of that size fails AC23 as worded although no instrumented line changed. D-7 records the figures and the executor does not weaken the criterion.
- **CSharpier layout.** The P4-T1 scoped format may re-lay the delivered C# (chained assertions, the multi-line `if` in F-SCOPE, the `using (` headers). Its output wins. Every gate token below is a string literal, a single-line statement or a doc-comment line, none of which CSharpier splits or joins, so no gate depends on the layout.

## Delivered source (the executor writes these texts; CSharpier output wins on any layout difference)

Every block below is shown at its in-file indentation (four, eight, twelve, sixteen or twenty leading spaces), so it is written exactly as shown. Prose-quoted gate tokens are each confined to one physical line of the delivered text. Every edit to an existing file is an in-place edit of the named lines (pre-edit numbering, facts 1 to 4); no existing file is rewritten whole.

**F-FIELDS — `QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixture.cs`, inserted after line 38** (`private static Dispatcher _parkedDispatcher = null;`): one blank line, then four lines:

        // Issue #968: the count of live ensure scopes and whether the fixture itself seeded the parked
        // dispatcher into a null field. Both are read and written only inside lock (FieldLock).
        private static int _pinCount;
        private static bool _fixtureInstalledParked;

**F-CLASSDOC — the same file, inserted after line 30** (`/// </para>`, the last line of the design-note paragraph) and before line 31 (`/// </summary>`), thirteen lines:

    /// <para>
    /// Issue #968: ensure pins are reference counted. A pin counter and an install-ownership flag
    /// live under <c>FieldLock</c>. The first pin on a <c>null</c> field seeds the parked dispatcher
    /// and sets the flag; the last release writes <c>null</c> back only when the flag is set and the
    /// field still holds the parked instance, then clears the flag. A discarded scope therefore
    /// pins for the process lifetime and leaves the parked dispatcher installed, so every caller
    /// disposes its scope, and every pin is acquired and released while its caller holds a
    /// transaction, which keeps the count at zero whenever a transaction is acquired. Residual: a
    /// transaction that installs over a pinned parked value and restores it after the last pin
    /// released leaves the parked value installed with zero pins and the flag set; the next pin
    /// cycle reverts it. No test in this assembly installs over a pinned value, so the residual is
    /// documented rather than exercised.
    /// </para>

Gate tokens quoted from F-CLASSDOC: install-ownership flag; pins for the process lifetime.

**F-ENSURE-DOC — the same file, replaces lines 116 to 121** (the `EnsureDispatcher` summary), nine lines replacing six:

        /// <summary>
        /// Takes one counted pin on the shared static (issue #968). The first pin on a <c>null</c>
        /// field seeds the parked dispatcher and records that the fixture owns the seeding; a pin
        /// taken while the field is non-null installs nothing. Disposing the returned scope releases
        /// the pin, and the field reverts to <c>null</c> only on the last release, only when the
        /// fixture owns the seeding, and only when the field still holds the parked instance. Never
        /// acquires <c>TransactionGate</c> and never blocks on anything a caller must release. A
        /// discarded scope pins for the process lifetime, so every caller disposes its scope.
        /// </summary>

**F-ENSURE-BODY — the same file, replaces lines 128 to 137** (from `lock (FieldLock)` through `return new EnsureScope(null);`; the comment 124 to 125, the `parked` local 126, the blank 127 and the method close 138 are unchanged), eleven lines replacing ten:

            lock (FieldLock)
            {
                _pinCount++;
                if (DispatcherField.GetValue(null) == null)
                {
                    DispatcherField.SetValue(null, parked);
                    _fixtureInstalledParked = true;
                }
            }

            return new EnsureScope(parked);

**F-SCOPE — the same file, replaces lines 243 to 274** (the `EnsureScope` documentation and class), forty-two lines replacing thirty-two:

        /// <summary>
        /// The scope returned by <see cref="EnsureDispatcher"/>: one counted pin. Disposal is
        /// idempotent and performs the decrement and the conditional revert inline in one
        /// <c>FieldLock</c> critical section, so no other pin can interleave between them. The revert
        /// writes <c>null</c> only when this release brings the count to zero, the fixture itself
        /// seeded the parked dispatcher, and the field still holds that instance; a value some other
        /// owner installed in the meantime is left in place.
        /// </summary>
        private sealed class EnsureScope : IDisposable
        {
            private readonly Dispatcher _parked;
            private bool _disposed = false;

            internal EnsureScope(Dispatcher parked)
            {
                _parked = parked;
                _disposed = false;
            }

            public void Dispose()
            {
                if (_disposed)
                {
                    return;
                }

                _disposed = true;

                lock (FieldLock)
                {
                    _pinCount--;
                    if (
                        _pinCount == 0
                        && _fixtureInstalledParked
                        && ReferenceEquals(DispatcherField.GetValue(null), _parked)
                    )
                    {
                        DispatcherField.SetValue(null, null);
                        _fixtureInstalledParked = false;
                    }
                }
            }
        }

After F-FIELDS to F-SCOPE the fixture is 374 lines before formatting; `_pinCount` occurs on exactly four lines (declaration, `_pinCount++;`, `_pinCount--;`, `_pinCount == 0`), `_fixtureInstalledParked` on exactly four (declaration, `_fixtureInstalledParked = true;`, `&& _fixtureInstalledParked`, `_fixtureInstalledParked = false;`), `lock (FieldLock)` on five, `CompareExchange(` on two (the helper's declaration and the transaction's call), `return new EnsureScope(` on one, and neither identifier appears in any comment.

**N1 — `QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherPinCountTests.cs`, new file, whole content:**

    using System;
    using System.Threading.Tasks;
    using System.Windows.Threading;
    using FluentAssertions;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    namespace QuickFiler.Controllers.Tests
    {
        /// <summary>
        /// Issue #968 tests for the reference-counted ensure pin of
        /// <see cref="UiThreadDispatcherFixture"/>. The regression lives at the fixture level because
        /// the theme tests the issue was filed against never read the shared static: their path
        /// dispatches through the theme's injected <c>IUiDispatcher</c> mock, so no value in the static
        /// can make a theme test fail, while the defect (the installing pin's release writing
        /// <c>null</c> while another pin is still live) is observable here on one thread with no
        /// concurrency.
        /// <para>
        /// Every test acquires a transaction first, so the pin count is zero and the baseline is known
        /// for the whole test, and carries the 60-second MSTest timeout of the sibling fixture test
        /// file. No sleep, delay, wall-clock wait, mock or temporary file is used; Moq is therefore
        /// not imported.
        /// </para>
        /// </summary>
        [TestClass]
        public class QfcItemController_UiThreadDispatcherPinCountTests
        {
            private const int GateTimeoutMs = 60000;

            /// <summary>
            /// Regression test: fails before the fix. With two pins held on a null baseline, releasing
            /// the first pin must leave the parked dispatcher in place. Before counting, the first pin
            /// was the installer and its release wrote <c>null</c> while the second pin was still live.
            /// </summary>
            [TestMethod]
            [Timeout(GateTimeoutMs)]
            public async Task EnsureDispatcher_TwoPinsHeld_ReleasingTheFirstKeepsTheDispatcherUntilTheLastRelease()
            {
                // Arrange
                UiThreadDispatcherTransaction transaction = await UiThreadDispatcherFixture
                    .BeginTransactionAsync()
                    .ConfigureAwait(false);
                try
                {
                    transaction.Install(null);
                    IDisposable pinA = QfcItemControllerTestSupport.EnsureUiThreadDispatcher();
                    IDisposable pinB = QfcItemControllerTestSupport.EnsureUiThreadDispatcher();
                    Dispatcher afterBothPins = UiThreadDispatcherFixture.Current;

                    // Act
                    pinA.Dispose();
                    Dispatcher afterFirstRelease = UiThreadDispatcherFixture.Current;
                    pinB.Dispose();
                    Dispatcher afterLastRelease = UiThreadDispatcherFixture.Current;

                    // Assert
                    afterBothPins
                        .Should()
                        .NotBeNull(
                            because: "the first pin seeds the parked dispatcher into a null field"
                        );
                    afterFirstRelease
                        .Should()
                        .BeSameAs(
                            afterBothPins,
                            because: "a holder that did not take the last pin must not lose the dispatcher"
                        );
                    afterLastRelease
                        .Should()
                        .BeNull(because: "the last release reverts the fixture's own seeding");
                }
                finally
                {
                    transaction.Dispose();
                }
            }

            /// <summary>
            /// Specification test: passes before and after the fix. Releasing the pins in the reverse
            /// order must produce the same outcome, so the count rather than the identity of the
            /// installing scope decides when the field reverts.
            /// </summary>
            [TestMethod]
            [Timeout(GateTimeoutMs)]
            public async Task EnsureDispatcher_TwoPinsHeld_ReleaseOrderDoesNotChangeTheOutcome()
            {
                // Arrange
                UiThreadDispatcherTransaction transaction = await UiThreadDispatcherFixture
                    .BeginTransactionAsync()
                    .ConfigureAwait(false);
                try
                {
                    transaction.Install(null);
                    IDisposable pinA = QfcItemControllerTestSupport.EnsureUiThreadDispatcher();
                    IDisposable pinB = QfcItemControllerTestSupport.EnsureUiThreadDispatcher();
                    Dispatcher afterBothPins = UiThreadDispatcherFixture.Current;

                    // Act
                    pinB.Dispose();
                    Dispatcher afterFirstRelease = UiThreadDispatcherFixture.Current;
                    pinA.Dispose();
                    Dispatcher afterLastRelease = UiThreadDispatcherFixture.Current;

                    // Assert
                    afterBothPins
                        .Should()
                        .NotBeNull(
                            because: "the first pin seeds the parked dispatcher into a null field"
                        );
                    afterFirstRelease
                        .Should()
                        .BeSameAs(
                            afterBothPins,
                            because: "a holder that did not take the last pin must not lose the dispatcher"
                        );
                    afterLastRelease
                        .Should()
                        .BeNull(because: "the last release reverts the fixture's own seeding");
                }
                finally
                {
                    transaction.Dispose();
                }
            }

            /// <summary>
            /// Specification test: passes before and after the fix; extends the existing fixture test
            /// EnsureDispatcher_WhileATransactionHoldsALiveDispatcher_DoesNotReplaceIt. While a
            /// transaction holds a live dispatcher, two pins install nothing, and releasing both must
            /// leave the live dispatcher in place: the count reaching zero writes nothing because the
            /// fixture did not seed the field. The live dispatcher is shut down in a finally block.
            /// </summary>
            [TestMethod]
            [Timeout(GateTimeoutMs)]
            public async Task EnsureDispatcher_UnderATransactionHoldingALiveDispatcher_ReleasingAllPinsLeavesTheLiveDispatcher()
            {
                // Arrange
                Dispatcher live = QfcItemControllerTestSupport.StartRunningDispatcher();
                try
                {
                    UiThreadDispatcherTransaction transaction = await UiThreadDispatcherFixture
                        .BeginTransactionAsync()
                        .ConfigureAwait(false);
                    try
                    {
                        transaction.Install(live);
                        IDisposable pinA = QfcItemControllerTestSupport.EnsureUiThreadDispatcher();
                        IDisposable pinB = QfcItemControllerTestSupport.EnsureUiThreadDispatcher();

                        // Act
                        pinA.Dispose();
                        pinB.Dispose();
                        Dispatcher afterAllReleased = UiThreadDispatcherFixture.Current;

                        // Assert
                        afterAllReleased
                            .Should()
                            .BeSameAs(
                                live,
                                because: "pins that installed nothing must not write over the transaction value "
                                    + "when the count reaches zero"
                            );
                    }
                    finally
                    {
                        transaction.Dispose();
                    }
                }
                finally
                {
                    QfcItemControllerTestSupport.ShutdownDispatcher(live);
                }
            }

            /// <summary>
            /// Specification test: passes before and after the fix. After a full two-pin cycle inside the
            /// same transaction, a fresh single pin on the null baseline must still seed the parked
            /// dispatcher and its release must still restore null, which shows the install-ownership
            /// flag was cleared by the earlier cycle's last release.
            /// </summary>
            [TestMethod]
            [Timeout(GateTimeoutMs)]
            public async Task EnsureDispatcher_AfterAFullPinCycle_AFreshSinglePinStillInstallsAndRestores()
            {
                // Arrange
                UiThreadDispatcherTransaction transaction = await UiThreadDispatcherFixture
                    .BeginTransactionAsync()
                    .ConfigureAwait(false);
                try
                {
                    transaction.Install(null);
                    IDisposable pinA = QfcItemControllerTestSupport.EnsureUiThreadDispatcher();
                    IDisposable pinB = QfcItemControllerTestSupport.EnsureUiThreadDispatcher();
                    pinA.Dispose();
                    pinB.Dispose();

                    // Act
                    IDisposable freshPin = QfcItemControllerTestSupport.EnsureUiThreadDispatcher();
                    Dispatcher afterFreshPin = UiThreadDispatcherFixture.Current;
                    freshPin.Dispose();
                    Dispatcher afterFreshRelease = UiThreadDispatcherFixture.Current;

                    // Assert
                    afterFreshPin
                        .Should()
                        .NotBeNull(
                            because: "a pin on a null field seeds the parked dispatcher whatever earlier cycles did"
                        );
                    afterFreshRelease
                        .Should()
                        .BeNull(
                            because: "the fresh pin is the only live pin, so its release reverts the seeding"
                        );
                }
                finally
                {
                    transaction.Dispose();
                }
            }
        }
    }

N1 counts (each token on one physical line): `EnsureUiThreadDispatcher()` 9 (tests 1, 2 and 3 two each, test 4 three); bare `EnsureDispatcher` 14 lines (the 9 invocations, the 4 method names, the test 3 doc reference); `Regression test: fails before the fix` 1; `Specification test: passes before and after the fix` 3; `never read the shared static` 1; `[TestClass]` 1; `[TestMethod]` 4; `[Timeout(GateTimeoutMs)]` 4; `private const int GateTimeoutMs = 60000;` 1; `transaction.Install(null);` 3; `transaction.Install(live);` 1; `transaction.Dispose();` 4; `QfcItemControllerTestSupport.ShutdownDispatcher(live);` 1; `a holder that did not take the last pin must not lose the dispatcher` 2; `the last release reverts the fixture` 2; `using Moq;` 0; `Thread.Sleep` 0; `Task.Delay` 0; `public class QfcItemController_UiThreadDispatcherPinCountTests` 1. Every `.Install(` line precedes every pin acquisition in its test, and every pin is disposed before its test's `transaction.Dispose();`.

**T1 — `QuickFiler.Test/QuickFiler.Test.csproj`, one line inserted after line 203** (`    <Compile Include="Controllers\QfcItemController.UiThreadDispatcherFixtureTests.cs" />`), four leading spaces:

        <Compile Include="Controllers\QfcItemController.UiThreadDispatcherPinCountTests.cs" />

**A1 — `QuickFiler.Test/Controllers/QfcItemController.FocusAndThemeTests.cs`, delete lines 99 to 116** (the private `BuildExecutingViewer` method and the blank line that follows it; line 98 stays blank and line 117 `/// <summary>` becomes line 99). Eighteen lines removed.

**A2 — the same file, two edits, applied after A1 (so by content, not by pre-edit line number):** (i) every line whose trimmed text is `var viewer = BuildExecutingViewer();` (seven lines) becomes, at the same twelve-space indentation, `var viewer = QfcItemControllerTestSupport.BuildExecutingViewer();`; (ii) the six-line comment block beginning `// Cycle-3 P9-T5/P9-T6 (members #33/#35, de-exempted); cycle-4 remediation R1: the entire body` and ending `// state machine) and assert the resulting state transitions, not merely the Invoke marshal.` is replaced by these seven lines (eight-space indent):

        // Cycle-3 P9-T5/P9-T6 (members #33/#35, de-exempted); cycle-4 remediation R1: the entire body
        // runs inside a single _itemViewer.Invoke(...) delegate. The shared
        // QfcItemControllerTestSupport.BuildExecutingViewer() executes the delegate synchronously
        // (issue #968 removed this file's private copy) and EnableHandlelessThemeInvoke() populates
        // the terminal _themes[_activeTheme].SetQfcTheme(async: false) call's dependencies with
        // handle-less doubles, so these tests exercise the full method body (the _activeUI/_activeTheme
        // state machine) and assert the resulting state transitions, not merely the Invoke marshal.

**A3 — the same file, `SetThemeDark_FromNormal_SelectsDarkNormalTheme`:** the two comment lines beginning `// Arrange — async:true queues the theme application on the dispatcher without executing it,` and `// so no handle-less control is touched; the observable effect is the active-theme switch.` and the following line `QfcItemControllerTestSupport.EnsureUiThreadDispatcher();` (three lines, pre-A1 450 to 452) are replaced by these five lines (twelve-space indent):

            // Arrange — async:true queues the theme application through the theme's injected
            // IUiDispatcher mock (see BuildColorTheme), which absorbs the delegate without running it,
            // so no handle-less control is touched and the shared UiThread static is irrelevant to this
            // path (issue #968 deleted the former ensure call); the observable effect is the
            // active-theme switch.

**A4 — the same file, `SetThemeLight_FromNormal_SelectsLightNormalTheme`:** the line `// Arrange` and the following line `QfcItemControllerTestSupport.EnsureUiThreadDispatcher();` (pre-A1 467 to 468) are replaced by these two lines (twelve-space indent):

            // Arrange — same injected-mock arrangement as SetThemeDark_FromNormal_SelectsDarkNormalTheme:
            // the theme's IUiDispatcher mock absorbs the queued application (issue #968).

After A1 to A4 the file is 482 lines before formatting (497 minus 18 plus 1 plus 2). Gate tokens quoted from A2 to A4: QfcItemControllerTestSupport.BuildExecutingViewer(); absorbs the delegate without running it; shared UiThread static is irrelevant; absorbs the queued application.

**S1 — `QuickFiler.Test/Controllers/QfcItemController.TestSupport.cs`, replaces lines 216 to 237** (the `EnsureUiThreadDispatcher` documentation; the declaration 238 and body 239 are unchanged), twenty-four lines replacing twenty-two:

        /// <summary>
        /// Takes one reference-counted pin on the shared static <c>UiThread.Dispatcher</c> through
        /// <see cref="UiThreadDispatcherFixture.EnsureDispatcher"/> (issue #968): the first pin on a
        /// <c>null</c> field seeds a dedicated dispatcher hosted on a parked background thread that is
        /// never pumped, later pins install nothing, and the field reverts to <c>null</c> only when
        /// the last live pin releases and the fixture itself seeded it. The remaining legitimate
        /// callers are the fixture tests <c>QfcItemController_UiThreadDispatcherFixtureTests</c> and
        /// <c>QfcItemController_UiThreadDispatcherPinCountTests</c>, each of which acquires and
        /// releases its pin while holding a <see cref="UiThreadDispatcherTransaction"/>.
        /// <para>
        /// A dedicated (non-<c>CurrentDispatcher</c>) instance is used deliberately for test
        /// isolation: fire-and-forget <c>BeginInvoke</c>/<c>InvokeAsync</c> operations posted to the
        /// parked dispatcher are enqueued and never execute, so they cannot leak onto the test
        /// thread's own dispatcher and be run (and fault on a handle-less control) by an unrelated
        /// later test that pumps <c>Dispatcher.CurrentDispatcher</c>.
        /// </para>
        /// <para>
        /// Dispose the returned scope inside the same transaction that was held when it was taken: a
        /// discarded scope pins for the process lifetime, and a pin released outside its caller's
        /// transaction is released while another class may hold the gate. The implementation lives
        /// in <see cref="UiThreadDispatcherFixture"/>, the single owner of every mutation of that
        /// static made from this assembly's owned files.
        /// </para>
        /// </summary>

Gate tokens quoted from S1: remaining legitimate; QfcItemController_UiThreadDispatcherPinCountTests.

**S2 — the same file, replaces lines 282 to 288** (the `BuildExecutingViewer` documentation; applied after S1, at which point these lines are 284 to 290), seven lines replacing seven:

        /// <summary>
        /// Issue #480 shared arrange helper. Builds a viewer mock whose <c>Invoke</c> and
        /// <c>BeginInvoke</c> execute the supplied delegate synchronously, so a dispatch made through
        /// either path produces a countable call on whatever collaborator the delegate targets. Since
        /// issue #968 this is the single implementation; QfcItemController.FocusAndThemeTests.cs calls
        /// it instead of carrying a private copy.
        /// </summary>

After S1 and S2 the file is 442 lines before formatting.

**R-DOC — `QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixtureTests.cs`, replaces lines 196 to 208** (the R4 `<para>` ... `</para>`), thirteen lines replacing thirteen:

        /// <para>
        /// Issue #950 traced the earlier intermittent failure to a race on the shared static, not to
        /// a timing defect: a gate-free ensure call in another class could seed the parked dispatcher
        /// between the baseline read and the install, or between the restore and the second caller's
        /// read, and the test pinned a non-null baseline inside the gate to fence it. Issue #968
        /// removed that pin: the fixture now counts pins, so only the last release can revert the
        /// fixture's own seeding, and every remaining ensure call is acquired and released while its
        /// caller holds a transaction, so no class can write the field while transaction A holds the
        /// gate and every other transaction restores before it releases (W3/W4). Invariant for future
        /// editors: a pin must stay nested inside its caller's transaction, and UiThread.Initialize
        /// (W5) must not latch during this test; either would change the value the second caller
        /// observes.
        /// </para>

Gate tokens quoted from R-DOC: removed that pin: the fixture now counts pins; (W5) must not latch.

**R-BODY — the same file, R4 body, two edits against the pre-edit numbering (R-DOC replaces thirteen lines with thirteen, so the numbering below holds whether R-DOC is applied first or second; the plan applies R-DOC first):**

1. Replace lines 221 to 224 (the four-line `using (` header `using (` / `IDisposable baseline = QfcItemControllerTestSupport.EnsureUiThreadDispatcher()` / `)` / `{`) with these two lines (sixteen-space indent):

                try
                {

2. Replace line 270 (the sixteen-space `}` that closed the `baseline` using block, the line immediately after the `}` closing the `secondCallerStarted` using block and immediately before the twelve-space `}` closing the outer `try`) with these five lines:

                }
                finally
                {
                    transactionA.Dispose();
                }

Lines 225 to 269 (the `original` read, `transactionA.Install(liveA);`, the waiter, the explicit `transactionA.Dispose();` at 251, both assertions) are unchanged in text and indentation, because the `try` block body sits at the same twenty-space indentation the `using` block body had. The file is 472 lines before formatting (470 minus 4 plus 2 minus 1 plus 5). Within R4 after the edit: `EnsureUiThreadDispatcher()` 0, `using (` 1, `transactionA.Dispose();` 2, `finally` 3 (the waiter's, the new one, the `ShutdownDispatcher` one), `.BeSameAs(` 1, `.NotBeSameAs(` 1, `issue #230 lost update` 1.

## Execution conventions

- **Tokens.** `WORKTREE` denotes the absolute path of the item worktree supplied in the delegation prompt; the executor substitutes it into every payload and every `git -C` argument at run time and writes the token, never the path, into artifacts. `FEATURE` abbreviates the feature folder path. `BASE` denotes `94287369908cc920b21b0e3256314f988ad7d2f5`, which is written in full in every command. `QCT.` abbreviates the namespace prefix `QuickFiler.Controllers.Tests.` in this document only and is expanded in full in every executed filter. No artifact, and no line of this plan, carries an absolute host path, an account name or a machine name.
- **Payload channel.** Each indented payload in the Command Reference runs as one Bash tool call of the form `pwsh -NoProfile -Command '<payload>'`, newlines included, with the substitutions applied. Payloads use double quotes only, so the outer single quotes never conflict. Git commands run as single Bash calls of the form `git -C WORKTREE <arguments>`, never chained. No `cd`, no `&&`, `;` or `|` between Bash commands. File edits use the Edit and Write tools with absolute paths.
- **Command rows.** The `Command:` field of a payload artifact records the full payload as executed, with `WORKTREE` in place of the path, followed on the next line by the canonical command it implements. Every payload artifact records `WORKTREE-LEAF: agent-a291a7fbabf9d0229`; any other value means the payload ran in the wrong tree, and the task fails.
- **Exit codes.** `EXIT_CODE:` records the payload's principal exit value as named in each Command Reference entry. Deliberately failing runs carry `ExpectedExitCode:` equal to the deterministic value the task states. A recorded observation whose exit value is not gated carries `ExpectedExitCode:` equal to the observed value when non-zero and says so; the field is omitted when the observed value is 0.
- **Transcription.** Any absolute path inside a transcribed line is replaced by `REDACTED-PATH`. Trx and coverage documents are never copied into FEATURE.
- **Long-running payloads.** `CMD-COVERAGE-RUNNER` and `CMD-COVERAGE-DIRECT` are started with the Bash tool's `run_in_background` option and no shell redirection; the payload's own output, captured by the tool, is the record, and completion is the background-task notification together with a final output line `PAYLOAD-COMPLETE`. A run still in progress after 120 minutes is `COVERAGE RUN STALLED`: stop and report. Before any re-invocation after a timed-out or interrupted call, the executor runs `pwsh -NoProfile -Command '$leaf = "agent-a291a7fbabf9d0229"; $runner = "Invoke-MSTest" + "WithCoverage"; "STRAY_TEST_PROCESSES: " + @(Get-CimInstance Win32_Process -ErrorAction SilentlyContinue | Where-Object { $_.ProcessId -ne $PID -and $null -ne $_.CommandLine -and $_.CommandLine.Contains($leaf) -and ($_.Name -like "vstest*" -or $_.Name -like "testhost*" -or $_.Name -like "dotnet-coverage*" -or ($_.Name -like "pwsh*" -and $_.CommandLine.Contains($runner))) }).Count'` and proceeds only at `STRAY_TEST_PROCESSES: 0`. Both coverage payloads end with the unconditional line `Write-Output "PAYLOAD-COMPLETE"`; a run that ends without that line is `COVERAGE RUN ABORTED`.
- **No wall-clock construct anywhere.** No payload sleeps; no test edit adds a sleep, delay, retry, timeout change, parallelism attribute or temporary file.

## Command reference

**PREFIX** (the first three lines of every payload):

    Set-Location -LiteralPath "WORKTREE"
    [System.IO.Directory]::SetCurrentDirectory((Get-Location).Path)
    Write-Output ("WORKTREE-LEAF: " + (Split-Path -Leaf (Get-Location).Path))

**TOOLS** (the next lines of every build and test payload):

    $vswhere = Join-Path ${env:ProgramFiles(x86)} "Microsoft Visual Studio\Installer\vswhere.exe"
    $msbuild = & $vswhere -latest -products * -find "MSBuild\**\Bin\MSBuild.exe" | Select-Object -First 1
    $vstest = & $vswhere -latest -products * -find "Common7\IDE\Extensions\TestPlatform\vstest.console.exe" | Select-Object -First 1
    New-Item -ItemType Directory -Path "coverage\logs" -Force | Out-Null

**File tokens.** `FIX` is `QuickFiler.Test\Controllers\QfcItemController.UiThreadDispatcherFixture.cs`; `FAT` is `QuickFiler.Test\Controllers\QfcItemController.FocusAndThemeTests.cs`; `TS` is `QuickFiler.Test\Controllers\QfcItemController.TestSupport.cs`; `FT` is `QuickFiler.Test\Controllers\QfcItemController.UiThreadDispatcherFixtureTests.cs`; `PC` is `QuickFiler.Test\Controllers\QfcItemController.UiThreadDispatcherPinCountTests.cs`; `PROJ` is `QuickFiler.Test\QuickFiler.Test.csproj`. **CS4** is `"FIX", "FAT", "TS", "FT"` (the four existing code files, in that order, each expanded); **CS5** is CS4 plus `"PC"`; **CODE6-GIT** is the six Write Set code paths with forward slashes, space-separated, for git pathspecs: `QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixture.cs QuickFiler.Test/Controllers/QfcItemController.FocusAndThemeTests.cs QuickFiler.Test/Controllers/QfcItemController.TestSupport.cs QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixtureTests.cs QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherPinCountTests.cs QuickFiler.Test/QuickFiler.Test.csproj`.

**CMD-REBUILD** (`GATEARGS` is either `/p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` or `/p:TreatWarningsAsErrors=true`; `TASKID` substituted; `EXIT_CODE:` is `MSBUILD_EXIT_CODE:`; canonical command `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" GATEARGS`, resolved through vswhere against this worktree's TaskMaster.sln, plus /nodeReuse:false and a normal-verbosity file logger under the ignored coverage directory):

    PREFIX
    TOOLS
    $log = "coverage\logs\TASKID.msbuild.log"
    if (Test-Path -LiteralPath $log) { Remove-Item -LiteralPath $log -Force }
    $global:LASTEXITCODE = 0
    & $msbuild TaskMaster.sln /t:Rebuild /m /nodeReuse:false /p:Configuration=Debug "/p:Platform=Any CPU" GATEARGS "/flp:LogFile=$log;Verbosity=normal" | Out-Null
    Write-Output ("MSBUILD_EXIT_CODE: " + $LASTEXITCODE)
    $lines = Get-Content -LiteralPath $log -Encoding UTF8
    Write-Output ("ERRORS: " + (($lines | Select-String -Pattern "^\s*(\d+) Error\(s\)" | Select-Object -Last 1).Matches[0].Groups[1].Value))
    Write-Output ("WARNINGS: " + (($lines | Select-String -Pattern "^\s*(\d+) Warning\(s\)" | Select-Object -Last 1).Matches[0].Groups[1].Value))
    Write-Output ("SKIP_CORECOMPILE_LINES: " + @($lines | Where-Object { $_.Contains("Skipping target ""CoreCompile""") }).Count)
    Write-Output ("CSC_OUT_QUICKFILER: " + @($lines | Where-Object { $_.Contains("/out:obj\Debug\QuickFiler.dll") }).Count)
    Write-Output ("CSC_OUT_QUICKFILER_TEST: " + @($lines | Where-Object { $_.Contains("/out:obj\Debug\QuickFiler.Test.dll") }).Count)
    $files = @("QfcItemController.UiThreadDispatcherFixture.cs(", "QfcItemController.FocusAndThemeTests.cs(", "QfcItemController.TestSupport.cs(", "QfcItemController.UiThreadDispatcherFixtureTests.cs(", "QfcItemController.UiThreadDispatcherPinCountTests.cs(", "QuickFiler.Test.csproj(")
    $diag = @($lines | Where-Object { $l = $_; (@($files | Where-Object { $l.Contains($_) }).Count -gt 0) -and ($l -match "(error|warning) [A-Z]+[0-9]+") })
    Write-Output ("WRITESET_DIAGNOSTIC_LINES: " + $diag.Count)
    Write-Output ("WRITESET_DIAGNOSTIC_CODES: " + ((@($diag | ForEach-Object { [regex]::Match($_, "(error|warning) ([A-Z]+[0-9]+)").Groups[2].Value }) | Sort-Object -Unique) -join ","))
    Write-Output ("TEST_DLL_EXISTS: " + (Test-Path -LiteralPath "QuickFiler.Test\bin\Debug\QuickFiler.Test.dll"))
    Write-Output ("UCS_TEST_DLL_EXISTS: " + (Test-Path -LiteralPath "UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll"))

`ERRORS:` is read from the build summary line, so `0 Error(s)` is not mistaken for a substring of a larger count. Under /t:Rebuild `SKIP_CORECOMPILE_LINES` is 0 by construction; the two `CSC_OUT_` counts show the compiler ran for the test project and its production dependency (MSBuild echoes each csc command line at normal verbosity; the #950 run observed 2 for each).

**CMD-BUILD** (incremental build so a scoped test run observes a fresh assembly; `TASKID` substituted; `EXIT_CODE:` is `MSBUILD_EXIT_CODE:`; canonical command `msbuild TaskMaster.sln /t:Build /m /p:Configuration=Debug "/p:Platform=Any CPU"`):

    PREFIX
    TOOLS
    $log = "coverage\logs\TASKID.msbuild.log"
    if (Test-Path -LiteralPath $log) { Remove-Item -LiteralPath $log -Force }
    $before = (Get-Item -LiteralPath "QuickFiler.Test\bin\Debug\QuickFiler.Test.dll" -ErrorAction SilentlyContinue).LastWriteTimeUtc
    $global:LASTEXITCODE = 0
    & $msbuild TaskMaster.sln /t:Build /m /nodeReuse:false /p:Configuration=Debug "/p:Platform=Any CPU" "/flp:LogFile=$log;Verbosity=normal" | Out-Null
    Write-Output ("MSBUILD_EXIT_CODE: " + $LASTEXITCODE)
    $lines = Get-Content -LiteralPath $log -Encoding UTF8
    Write-Output ("ERRORS: " + (($lines | Select-String -Pattern "^\s*(\d+) Error\(s\)" | Select-Object -Last 1).Matches[0].Groups[1].Value))
    $after = (Get-Item -LiteralPath "QuickFiler.Test\bin\Debug\QuickFiler.Test.dll").LastWriteTimeUtc
    Write-Output ("TEST_DLL_ADVANCED: " + ($null -eq $before -or $after -gt $before))
    Write-Output ("CSC_OUT_QUICKFILER_TEST: " + @($lines | Where-Object { $_.Contains("/out:obj\Debug\QuickFiler.Test.dll") }).Count)

**CMD-VSTEST** (`FILTER`, `TASKID` and `NAMES` substituted; an empty `NAMES` prints every result; `EXIT_CODE:` is `VSTEST_EXIT_CODE:`, or 3 when the trx is absent; canonical command `vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation "/TestCaseFilter:FILTER" "/ResultsDirectory:coverage\test-results\968\TASKID" "/Logger:trx;LogFileName=TASKID.trx" "/Blame:CollectHangDump;TestTimeout=4min;HangDumpType=None"`, resolved through vswhere; `ASSEMBLY` is `QuickFiler.Test\bin\Debug\QuickFiler.Test.dll` except for the stall probe, which uses `UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll`):

    PREFIX
    TOOLS
    $results = "coverage\test-results\968\TASKID"
    if (Test-Path -LiteralPath $results) { Remove-Item -LiteralPath $results -Recurse -Force }
    $names = @(NAMES)
    $global:LASTEXITCODE = 0
    & $vstest "ASSEMBLY" /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation "/TestCaseFilter:FILTER" "/ResultsDirectory:$results" "/Logger:trx;LogFileName=TASKID.trx" "/Blame:CollectHangDump;TestTimeout=4min;HangDumpType=None" 2>&1 | Tee-Object -FilePath "coverage\logs\TASKID.vstest.log" | Out-Null
    Write-Output ("VSTEST_EXIT_CODE: " + $LASTEXITCODE)
    $trxPath = Join-Path $results "TASKID.trx"
    Write-Output ("TRX_PRESENT: " + (Test-Path -LiteralPath $trxPath))
    Write-Output ("SEQUENCE_FILES: " + @(Get-ChildItem -LiteralPath $results -Recurse -Filter "Sequence_*.xml" -ErrorAction SilentlyContinue).Count)
    if (-not (Test-Path -LiteralPath $trxPath)) { exit 3 }
    [xml]$trx = Get-Content -LiteralPath $trxPath -Raw -Encoding UTF8
    $ns = New-Object System.Xml.XmlNamespaceManager($trx.NameTable)
    $ns.AddNamespace("t", "http://microsoft.com/schemas/VisualStudio/TeamTest/2010")
    $counters = $trx.SelectSingleNode("//t:ResultSummary/t:Counters", $ns)
    Write-Output ("COUNTERS total=" + $counters.GetAttribute("total") + " executed=" + $counters.GetAttribute("executed") + " passed=" + $counters.GetAttribute("passed") + " failed=" + $counters.GetAttribute("failed"))
    $all = @($trx.SelectNodes("//t:UnitTestResult", $ns))
    Write-Output ("RESULT_COUNT: " + $all.Count)
    foreach ($r in $all) { if ($names.Count -eq 0 -or $names -contains $r.GetAttribute("testName")) { Write-Output ("RESULT " + $r.GetAttribute("testName") + " = " + $r.GetAttribute("outcome") + " duration=" + $r.GetAttribute("duration")) } }
    foreach ($r in $all) { if ($r.GetAttribute("outcome") -ne "Passed") { $msg = $r.SelectSingleNode("t:Output/t:ErrorInfo/t:Message", $ns); Write-Output ("MESSAGE " + $r.GetAttribute("testName") + " :: " + $(if ($msg) { $msg.InnerText -replace "\s+", " " } else { "(no message)" })) } }

The trx stays under the ignored coverage directory; the artifact transcribes the `COUNTERS`, `RESULT_COUNT:`, `RESULT` and `MESSAGE` lines.

Filters and name lists (every `QCT.` expanded to `QuickFiler.Controllers.Tests.` when executed):

- `NAMES-PC`: `"EnsureDispatcher_TwoPinsHeld_ReleasingTheFirstKeepsTheDispatcherUntilTheLastRelease", "EnsureDispatcher_TwoPinsHeld_ReleaseOrderDoesNotChangeTheOutcome", "EnsureDispatcher_UnderATransactionHoldingALiveDispatcher_ReleasingAllPinsLeavesTheLiveDispatcher", "EnsureDispatcher_AfterAFullPinCycle_AFreshSinglePinStillInstallsAndRestores"`.
- `NAMES-FT`: `"EnsureDispatcher_WhileATransactionHoldsALiveDispatcher_DoesNotReplaceIt", "EnsureDispatcher_WhenTheFieldIsNull_InstallsAndRestoresOnDispose", "EnsureDispatcher_ScopeDisposedTwice_IsIdempotent", "Transaction_SecondCallerCannotInstallUntilTheFirstRestores", "Transaction_DisposedTwice_DoesNotOverReleaseTheGate", "Install_CalledTwiceOnTheSameTransaction_ThrowsInvalidOperationException", "TransactionGate_WhileThisTestHoldsATransaction_HasExactlyOneUnreleasedAcquisition", "BeginTransactionAsync_ZeroBoundWhileThisTestHoldsThePermit_ThrowsTimeoutExceptionAndReleasesNothing"`.
- `NAMES-THEME`: `"SetThemeDark_FromNormal_SelectsDarkNormalTheme", "SetThemeLight_FromNormal_SelectsLightNormalTheme"`.
- `NAMES-TARGETS`: NAMES-PC, NAMES-FT and NAMES-THEME together (fourteen names).
- `FILTER-PC-T1`: `FullyQualifiedName=QCT.QfcItemController_UiThreadDispatcherPinCountTests.EnsureDispatcher_TwoPinsHeld_ReleasingTheFirstKeepsTheDispatcherUntilTheLastRelease`.
- `FILTER-PC-T234`: the `FullyQualifiedName=` expressions for the other three NAMES-PC methods, joined with `|` (vstest rejects `OR`).
- `FILTER-PC-CLASS`: `FullyQualifiedName~QCT.QfcItemController_UiThreadDispatcherPinCountTests.`
- `FILTER-FT-CLASS`: `FullyQualifiedName~QCT.QfcItemController_UiThreadDispatcherFixtureTests.`
- `FILTER-FAT-CLASS`: `FullyQualifiedName~QCT.QfcItemController_FocusAndThemeTests.`
- `FILTER-CONCURRENT`: the three class filters joined with `|` (29 tests: 8, 4 and 17).
- `FILTER-BASELINE-CONCURRENT`: `FILTER-FT-CLASS` and `FILTER-FAT-CLASS` joined with `|` (25 tests; the pin-count class does not exist at Phase 0).
- `FILTER-STALL`: `FullyQualifiedName~HelperClasses.ShellUtilities_Tests|FullyQualifiedName~HelperClasses.ShellUtilitiesStatic_Tests|FullyQualifiedName~HelperClasses.SysImageListHelperTests|FullyQualifiedName~EmailIntelligence.OSBrowser_Tests`.

**CMD-COVERAGE-RUNNER** (CLAUDE.md step 4 route; `STAGE` is `baseline` or `final`; `EXIT_CODE:` is `RUNNER_EXIT_CODE:`; canonical command `pwsh -NoProfile -File scripts\vscode\Invoke-MSTestWithCoverage.ps1` run from the worktree root):

    PREFIX
    New-Item -ItemType Directory -Path "coverage\logs" -Force | Out-Null
    foreach ($f in @("coverage\coverage.cobertura.xml", "coverage\coverage.cobertura.jacoco.xml", "coverage\test-results\mstest-coverage-run.trx", "coverage\test-results\mstest-coverage-run.summary.txt", "coverage\STAGE-968.cobertura.xml", "coverage\STAGE-968.trx")) { if (Test-Path -LiteralPath $f) { Remove-Item -LiteralPath $f -Force } }
    $script = Join-Path (Get-Location).Path "scripts\vscode\Invoke-MSTestWithCoverage.ps1"
    $global:LASTEXITCODE = 0
    & pwsh -NoProfile -File $script 2>&1 | Tee-Object -FilePath "coverage\logs\STAGE-968.runner.log" | Out-Null
    Write-Output ("RUNNER_EXIT_CODE: " + $LASTEXITCODE)
    $log = Get-Content -LiteralPath "coverage\logs\STAGE-968.runner.log" -Raw -Encoding UTF8
    Write-Output ("DISCOVERED_LINE: " + [regex]::Match($log, "Discovered \d+ test assemblies\.").Value)
    Write-Output ("FIRST_PARTY_LINE: " + [regex]::Match($log, "First-party coverage: [^\r\n]*").Value)
    Write-Output ("THRESHOLD_MESSAGE: " + [regex]::Match($log, "Cobertura (line|branch) coverage [^\r\n]*threshold\.").Value)
    Write-Output ("COLLECT_FAILURE_MESSAGE: " + [regex]::Match($log, "MSTest with coverage failed with exit code \d+").Value)
    Write-Output ("DOCUMENT_PRESENT: " + (Test-Path -LiteralPath "coverage\coverage.cobertura.xml"))
    Write-Output ("TRX_PRESENT: " + (Test-Path -LiteralPath "coverage\test-results\mstest-coverage-run.trx"))
    if (Test-Path -LiteralPath "coverage\coverage.cobertura.xml") { Copy-Item -LiteralPath "coverage\coverage.cobertura.xml" -Destination "coverage\STAGE-968.cobertura.xml" -Force }
    if (Test-Path -LiteralPath "coverage\test-results\mstest-coverage-run.trx") { Copy-Item -LiteralPath "coverage\test-results\mstest-coverage-run.trx" -Destination "coverage\STAGE-968.trx" -Force }
    Write-Output "PAYLOAD-COMPLETE"

**CMD-COVERAGE-DIRECT** (the runner's inner invocation issued directly with the four-class exclusion; `STAGE` substituted; `EXIT_CODE:` is `COLLECT_EXIT_CODE:`; canonical command `dotnet-coverage collect --output coverage\STAGE-968.cobertura.xml --output-format cobertura --settings coverage\effective-coverage-968.config -- vstest.console.exe <discovered test assemblies> /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation "/TestCaseFilter:<filter>" "/ResultsDirectory:coverage\test-results\968\STAGE" "/Logger:trx;LogFileName=STAGE-968.trx" "/Blame:CollectHangDump;TestTimeout=4min;HangDumpType=None"`):

    PREFIX
    . (Join-Path (Get-Location).Path "scripts\vscode\Invoke-MSTestWithCoverage.ps1")
    $ErrorActionPreference = "Continue"
    $repo = (Get-Location).Path
    New-Item -ItemType Directory -Path "coverage\logs" -Force | Out-Null
    foreach ($f in @("coverage\STAGE-968.cobertura.xml", "coverage\STAGE-968.trx")) { if (Test-Path -LiteralPath $f) { Remove-Item -LiteralPath $f -Force } }
    $canonical = Get-Content -LiteralPath "coverage.config" -Raw -Encoding UTF8
    $derived = ConvertTo-DerivedCoverageSettingsXml -CanonicalSettingsXml $canonical
    $effective = Join-Path $repo "coverage\effective-coverage-968.config"
    Set-Content -LiteralPath $effective -Value $derived -Encoding UTF8 -NoNewline
    $vswhere = Join-Path ${env:ProgramFiles(x86)} "Microsoft Visual Studio\Installer\vswhere.exe"
    $vstest = & $vswhere -latest -products * -find "Common7\IDE\Extensions\TestPlatform\vstest.console.exe" | Select-Object -First 1
    $rootLen = $repo.TrimEnd([char]92).Length
    $asm = @(Get-ChildItem -Path $repo -Recurse -Filter "*.Test.dll" | Where-Object { $_.FullName -like "*\bin\Debug\*" -and $_.FullName -notlike "*\obj\*" -and $_.FullName -notlike "*\ref\*" -and $_.FullName.Substring($rootLen) -notlike "\.claude\*" } | Select-Object -ExpandProperty FullName)
    $filter = "TestCategory!=LiveOutlook&FullyQualifiedName!~HelperClasses.ShellUtilities_Tests&FullyQualifiedName!~HelperClasses.ShellUtilitiesStatic_Tests&FullyQualifiedName!~HelperClasses.SysImageListHelperTests&FullyQualifiedName!~EmailIntelligence.OSBrowser_Tests"
    $output = Join-Path $repo "coverage\STAGE-968.cobertura.xml"
    $settings = Join-Path $repo "scripts\vscode\TaskMaster.cli.runsettings"
    $results = Join-Path $repo "coverage\test-results\968\STAGE"
    if (Test-Path -LiteralPath $results) { Remove-Item -LiteralPath $results -Recurse -Force }
    $global:LASTEXITCODE = 0
    & dotnet-coverage collect --output $output --output-format cobertura --settings $effective -- $vstest @asm "/Settings:$settings" /InIsolation "/TestCaseFilter:$filter" "/ResultsDirectory:$results" "/Logger:trx;LogFileName=STAGE-968.trx" "/Blame:CollectHangDump;TestTimeout=4min;HangDumpType=None" 2>&1 | Tee-Object -FilePath "coverage\logs\STAGE-968.collect.log" | Out-Null
    Write-Output ("COLLECT_EXIT_CODE: " + $LASTEXITCODE)
    Write-Output ("ASSEMBLY_COUNT: " + $asm.Count)
    $asm | ForEach-Object { Write-Output ("ASSEMBLY: " + $_.Substring($rootLen)) }
    Write-Output ("SEQUENCE_FILES: " + @(Get-ChildItem -LiteralPath $results -Recurse -Filter "Sequence_*.xml" -ErrorAction SilentlyContinue).Count)
    if (Test-Path -LiteralPath (Join-Path $results "STAGE-968.trx")) { Copy-Item -LiteralPath (Join-Path $results "STAGE-968.trx") -Destination "coverage\STAGE-968.trx" -Force }
    Write-Output ("TRX_PRESENT: " + (Test-Path -LiteralPath "coverage\STAGE-968.trx"))
    Write-Output ("DOCUMENT_PRESENT: " + (Test-Path -LiteralPath $output))
    Write-Output "PAYLOAD-COMPLETE"

**CMD-COVERAGE-POST** (summarise the trx, post-process if raw, apply the floors, print the committed forms and the figures; `STAGE` substituted; `RAW` is `True` under DIRECT and under a RUNNER run whose `COLLECT_FAILURE_MESSAGE:` is non-empty, otherwise `False`, because a completed runner run has already post-processed the document in place; `EXIT_CODE:` is the payload's own exit status, 0 when every line printed):

    PREFIX
    . (Join-Path (Get-Location).Path "scripts\vscode\Invoke-MSTestWithCoverage.Helpers.ps1")
    . (Join-Path (Get-Location).Path "scripts\vscode\Invoke-MSTest.TrxSummary.ps1")
    $ErrorActionPreference = "Continue"
    $repo = (Get-Location).Path
    $trxText = Get-Content -LiteralPath "coverage\STAGE-968.trx" -Raw -Encoding UTF8
    $summary = Get-TrxRunSummary -TrxContent $trxText
    Write-Output "SUMMARY-BEGIN"
    Write-Output (Format-TrxRunSummary -Summary $summary)
    Write-Output "SUMMARY-END"
    Write-Output ("FAILED-SET: " + ((@($summary.FailedTestName) | Sort-Object -Unique) -join ", "))
    [xml]$trx = $trxText
    $ns = New-Object System.Xml.XmlNamespaceManager($trx.NameTable)
    $ns.AddNamespace("t", "http://microsoft.com/schemas/VisualStudio/TeamTest/2010")
    $names = @(NAMES-TARGETS)
    foreach ($r in @($trx.SelectNodes("//t:UnitTestResult", $ns))) { if ($names -contains $r.GetAttribute("testName")) { Write-Output ("RESULT " + $r.GetAttribute("testName") + " = " + $r.GetAttribute("outcome")) } }
    $doc = Get-Content -LiteralPath "coverage\STAGE-968.cobertura.xml" -Raw -Encoding UTF8
    if ("RAW" -eq "True") { $doc = ConvertTo-KoverageCoberturaXml -XmlContent $doc -RepoRoot $repo; Set-Content -LiteralPath "coverage\STAGE-968.cobertura.xml" -Value $doc -Encoding UTF8 -NoNewline }
    try { Assert-CoberturaLineCoverageThreshold -CoberturaXml $doc; Write-Output "LINE-FLOOR: MET" } catch { Write-Output ("LINE-FLOOR: NOT MET " + $_.Exception.Message) }
    try { Assert-CoberturaBranchCoverageThreshold -CoberturaXml $doc; Write-Output "BRANCH-FLOOR: MET" } catch { Write-Output ("BRANCH-FLOOR: NOT MET " + $_.Exception.Message) }
    Write-Output (Get-CoberturaFirstPartyCoverageReport -CoberturaXml $doc)
    [xml]$xml = $doc
    $root = $xml.SelectSingleNode("/coverage")
    Write-Output ("ROOT line-rate=" + $root.GetAttribute("line-rate") + " branch-rate=" + $root.GetAttribute("branch-rate") + " lines-covered=" + $root.GetAttribute("lines-covered") + " lines-valid=" + $root.GetAttribute("lines-valid") + " branches-covered=" + $root.GetAttribute("branches-covered") + " branches-valid=" + $root.GetAttribute("branches-valid"))
    $projection = ConvertTo-JacocoPackageProjection -XmlDocument $xml
    Assert-JacocoProjectionReconciliation -XmlDocument $xml -ProjectionXml $projection
    Write-Output "PROJECTION-BEGIN"
    Write-Output $projection
    Write-Output "PROJECTION-END"
    Write-Output ("TEST_ASSEMBLY_PACKAGES: " + @($xml.SelectNodes("//package") | Where-Object { $_.GetAttribute("name") -like "*.Test" }).Count)

The projection and the summary block are the two committed forms (CLAUDE.md "Committed Test Evidence Format"); the remaining lines are figures. `TEST_ASSEMBLY_PACKAGES:` is the observation behind D-7: 0 means no test assembly was instrumented, so no changed line of this plan has a coverage figure.

**CMD-HASH** (`FILES` substituted with CS4 or CS5; hashes only):

    PREFIX
    foreach ($p in @(FILES)) { Write-Output ("HASH " + $p + " = " + (Get-FileHash -Algorithm SHA256 -LiteralPath $p).Hash) }

**CMD-LINECOUNT** (`FILES` substituted):

    PREFIX
    foreach ($p in @(FILES)) { Write-Output ("LINES " + $p + " = " + @(Get-Content -LiteralPath $p -Encoding UTF8).Count) }

**CMD-TOKEN-COUNT** (`FILE` and the `TOKENS` list substituted; ordinal, case-sensitive substring counts per physical line, so a token wrapped across two lines reads 0):

    PREFIX
    $src = Get-Content -LiteralPath "FILE" -Encoding UTF8
    foreach ($t in @(TOKENS)) { Write-Output ("TOKEN [" + $t + "] = " + @($src | Where-Object { $_.Contains($t) }).Count) }

**CMD-SPAN-TOKEN-COUNT** (`FILE`, `START`, `END` and `TOKENS` substituted; the span runs from the first line containing START up to, not including, the next line containing END, or to the end of the file when END is the literal `EOF`; exit 4 when an anchor is missing):

    PREFIX
    $src = Get-Content -LiteralPath "FILE" -Encoding UTF8
    $s = -1; for ($i = 0; $i -lt $src.Count; $i++) { if ($src[$i].Contains("START")) { $s = $i; break } }
    $e = -1; if ($s -ge 0) { if ("END" -eq "EOF") { $e = $src.Count } else { for ($i = $s + 1; $i -lt $src.Count; $i++) { if ($src[$i].Contains("END")) { $e = $i; break } } } }
    Write-Output ("SPAN: " + ($s + 1) + "-" + $e)
    if ($s -lt 0 -or $e -lt 0) { exit 4 }
    $span = $src[$s..($e - 1)]
    foreach ($t in @(TOKENS)) { Write-Output ("SPAN-TOKEN [" + $t + "] = " + @($span | Where-Object { $_.Contains($t) }).Count) }

Span anchors used by this plan (file, START, END):

- `R4SPAN`: FT, `public async Task Transaction_SecondCallerCannotInstallUntilTheFirstRestores()`, `public async Task Transaction_DisposedTwice_DoesNotOverReleaseTheGate()`. Baseline `SPAN: 212-284`.
- `R4HEAD`: FT, the R4SPAN START, `Dispatcher original = UiThreadDispatcherFixture.Current;` (the first occurrence after the declaration). Baseline `SPAN: 212-224`.
- `R4TAIL`: FT, `issue #230 lost update`, `QfcItemControllerTestSupport.ShutdownDispatcher(liveA);` (the first occurrence after the START). Baseline `SPAN: 267-273`.
- `R1SPAN`: FT, `public async Task EnsureDispatcher_WhileATransactionHoldsALiveDispatcher_DoesNotReplaceIt()`, `public async Task EnsureDispatcher_WhenTheFieldIsNull_InstallsAndRestoresOnDispose()`.
- `R2SPAN`: FT, the R1SPAN END, `public async Task EnsureDispatcher_ScopeDisposedTwice_IsIdempotent()`.
- `R3SPAN`: FT, the R2SPAN END, the R4SPAN START.
- `ENSURE`: FIX, `internal static IDisposable EnsureDispatcher()`, `internal const int TransactionGateAcquireTimeoutMs = 120000;`. Baseline `SPAN: 122-145`.
- `SCOPE`: FIX, `private sealed class EnsureScope : IDisposable`, `internal sealed class UiThreadDispatcherTransaction : IDisposable`. Baseline `SPAN: 249-283`.
- `T1SPAN`: PC, `public async Task EnsureDispatcher_TwoPinsHeld_ReleasingTheFirstKeepsTheDispatcherUntilTheLastRelease()`, `public async Task EnsureDispatcher_TwoPinsHeld_ReleaseOrderDoesNotChangeTheOutcome()`.
- `T2SPAN`: PC, the T1SPAN END, `public async Task EnsureDispatcher_UnderATransactionHoldingALiveDispatcher_ReleasingAllPinsLeavesTheLiveDispatcher()`.
- `T3SPAN`: PC, the T2SPAN END, `public async Task EnsureDispatcher_AfterAFullPinCycle_AFreshSinglePinStillInstallsAndRestores()`.
- `T4SPAN`: PC, the T3SPAN END, `EOF`.

**CMD-PIN-NESTING** (`FILE`, `START`, `END` substituted; same span rule as CMD-SPAN-TOKEN-COUNT; prints, in file order, every span line that carries one of the six nesting tokens, so the acquisition order can be read from the line numbers):

    PREFIX
    $src = Get-Content -LiteralPath "FILE" -Encoding UTF8
    $s = -1; for ($i = 0; $i -lt $src.Count; $i++) { if ($src[$i].Contains("START")) { $s = $i; break } }
    $e = -1; if ($s -ge 0) { if ("END" -eq "EOF") { $e = $src.Count } else { for ($i = $s + 1; $i -lt $src.Count; $i++) { if ($src[$i].Contains("END")) { $e = $i; break } } } }
    Write-Output ("SPAN: " + ($s + 1) + "-" + $e)
    if ($s -lt 0 -or $e -lt 0) { exit 4 }
    for ($i = $s; $i -lt $e; $i++) { $t = $src[$i]; foreach ($k in @("BeginTransactionAsync()", "EnsureUiThreadDispatcher()", "Dispose()", ".Install(", "finally", "ShutdownDispatcher(")) { if ($t.Contains($k)) { Write-Output ("NEST " + ($i + 1) + " [" + $k + "] " + $t.Trim()); break } } }

**CMD-CENSUS** (the two independent search strategies of research section 2.1 over every `*.cs` file under the worktree outside `packages`, `.claude`, `obj` and `bin`):

    PREFIX
    $root = (Get-Location).Path
    $rootLen = $root.TrimEnd([char]92).Length
    $files = @(Get-ChildItem -Path $root -Recurse -Filter "*.cs" | Where-Object { $rel = $_.FullName.Substring($rootLen); $rel -notlike "\packages\*" -and $rel -notlike "\.claude\*" -and $rel -notlike "*\obj\*" -and $rel -notlike "*\bin\*" })
    Write-Output ("CS_FILES: " + $files.Count)
    $primary = @($files | Select-String -Pattern "EnsureUiThreadDispatcher\(\)|EnsureDispatcher\(\)" -CaseSensitive)
    Write-Output ("PRIMARY_LINES: " + $primary.Count)
    foreach ($g in @($primary | Group-Object Path)) { Write-Output ("PRIMARY-FILE " + $g.Name.Substring($rootLen) + " = " + $g.Count) }
    foreach ($m in $primary) { Write-Output ("PRIMARY " + $m.Path.Substring($rootLen) + ":" + $m.LineNumber + " :: " + $m.Line.Trim()) }
    $cross = @($files | Select-String -Pattern "EnsureUiThreadDispatcher|EnsureDispatcher" -CaseSensitive)
    Write-Output ("CROSS_LINES: " + $cross.Count)
    foreach ($g in @($cross | Group-Object Path)) { Write-Output ("CROSS-FILE " + $g.Name.Substring($rootLen) + " = " + $g.Count) }
    foreach ($m in $cross) { Write-Output ("CROSS " + $m.Path.Substring($rootLen) + ":" + $m.LineNumber + " :: " + $m.Line.Trim()) }
    Write-Output ("CONTROL_LINES: " + @($files | Select-String -Pattern "BeginTransactionAsync\(" -CaseSensitive).Count)

`CONTROL_LINES:` is the positive control (fact 5: 23 before the change, 27 after N1 adds four).

**CMD-ADDED-SCAN** (added lines of the anchored diff over the six Write Set code files; run after the P4-T8 commit so the new file is tracked):

    PREFIX
    $diff = @(git diff 94287369908cc920b21b0e3256314f988ad7d2f5 -- CODE6-GIT)
    Write-Output ("GIT_DIFF_EXIT_CODE: " + $LASTEXITCODE)
    $added = @($diff | Where-Object { $_.StartsWith("+") -and -not $_.StartsWith("+++") })
    Write-Output ("ADDED_LINES: " + $added.Count)
    foreach ($t in @("Thread.Sleep", "Task.Delay", "DoNotParallelize", "Retry(", "Path.GetTempFileName", "Path.GetTempPath", "Workers", "Timeout(", "[Timeout(GateTimeoutMs)]", "GateTimeoutMs = ", "_pinCount")) { Write-Output ("ADDED-TOKEN [" + $t + "] = " + @($added | Where-Object { $_.Contains($t) }).Count) }
    foreach ($l in @($added | Where-Object { $_.Contains("GateTimeoutMs = ") })) { Write-Output ("ADDED-LINE " + $l.Substring(1).Trim()) }

`_pinCount` is the positive control: it is added by this plan, so a scan that cannot see added lines reports 0 for it and the gate fails.

**CMD-HUNKS** (`FILE-GIT` substituted with one forward-slash path; prints the hunk headers of the anchored diff so a region can be shown untouched):

    PREFIX
    $d = @(git diff 94287369908cc920b21b0e3256314f988ad7d2f5 -- "FILE-GIT")
    Write-Output ("GIT_DIFF_EXIT_CODE: " + $LASTEXITCODE)
    foreach ($l in $d) { if ($l.StartsWith("@@")) { Write-Output ("HUNK " + $l) } }
    Write-Output ("HUNK_COUNT: " + @($d | Where-Object { $_.StartsWith("@@") }).Count)

**CMD-EOL** (normalises the new file to CRLF after the Write tool creates it, then reports the line-ending census):

    PREFIX
    $p = "QuickFiler.Test\Controllers\QfcItemController.UiThreadDispatcherPinCountTests.cs"
    $t = [System.IO.File]::ReadAllText($p)
    $crlf = [string][char]13 + [string][char]10
    $n = [regex]::Replace($t, "\r?\n", $crlf)
    if (-not $n.EndsWith($crlf)) { $n = $n + $crlf }
    [System.IO.File]::WriteAllText($p, $n, (New-Object System.Text.UTF8Encoding($false)))
    $b = [System.IO.File]::ReadAllText($p)
    Write-Output ("BARE_LF: " + [regex]::Matches($b, "(?<!\r)\n").Count)
    Write-Output ("CRLF_COUNT: " + [regex]::Matches($b, "\r\n").Count)
    Write-Output ("LINES: " + @(Get-Content -LiteralPath $p -Encoding UTF8).Count)

### Phase 0 — Policy Reads, Anchor, Bootstrap and Baseline Capture

- [ ] [P0-T1] Read the policy documents in the mandatory order — CLAUDE.md, then .claude/rules/general-code-change.md, then .claude/rules/general-unit-test.md, then .claude/rules/csharp.md — plus .claude/rules/plan-acceptance-gates.md, .claude/rules/tonality.md, .claude/skills/evidence-and-timestamp-conventions/SKILL.md and .claude/skills/acceptance-criteria-tracking/SKILL.md, and record the read in FEATURE/evidence/baseline/phase0-instructions-read.md.
  - Acceptance: the artifact carries `Timestamp:`, a `Policy Order:` line naming the four mandatory documents in that order, and one line per document read. No policy document is modified.
- [ ] [P0-T2] Read FEATURE/spec.md, FEATURE/issue.md and FEATURE/research/2026-10-02T05-50-dispatcher-pin-call-sites-research.md in full and record the Write Set, the prohibited paths, the acceptance-criteria inventory and the amendment 1.1 literals in FEATURE/evidence/baseline/scope-and-anchor.md (this task creates the file; P0-T3 appends to it).
  - Commands: `CMD-TOKEN-COUNT` on `docs\features\active\2026-10-02-focus-and-theme-tests-leak-shared-dispatcher-setup-968\spec.md` with TOKENS `"- [ ] AC", "- [x] AC", "- [ ] AC24:", "acquired and released inside a held", "the removal of its baseline pin", "inherited committed set", "Amendment 1.1"`.
  - Acceptance: the artifact lists the six code paths of the Write Set verbatim; names the prohibited paths from the Write Set section; records that issue.md line 12 reads `- Work Mode: full-bug`; records the spec tokens as `- [ ] AC` 24, `- [x] AC` 0, `- [ ] AC24:` 1, and each of the four amendment literals at least 1 (a value of 0 for any of them is `SPEC AMENDMENT MISSING`: stop, because the plan is written against the amended spec); and records that `docs/features/potential/promoted/2026-10-02-focus-and-theme-tests-leak-shared-dispatcher-setup.md` exists on the tree.
- [ ] [P0-T3] Record the anchor and the pre-change tree state by appending to FEATURE/evidence/baseline/scope-and-anchor.md.
  - Commands, each a separate Bash call: `git -C WORKTREE rev-parse HEAD`; `git -C WORKTREE rev-parse --abbrev-ref HEAD`; `git -C WORKTREE rev-parse origin/main`; `git -C WORKTREE merge-base --is-ancestor 94287369908cc920b21b0e3256314f988ad7d2f5 HEAD`; `git -C WORKTREE merge-base origin/main HEAD`; `git -C WORKTREE diff --name-status 94287369908cc920b21b0e3256314f988ad7d2f5 HEAD`; `git -C WORKTREE diff --exit-code 94287369908cc920b21b0e3256314f988ad7d2f5 HEAD -- QuickFiler QuickFiler.Test UtilitiesCS/Threading/UiThread.cs UtilitiesCS/HelperClasses/ThemeHelpers/Theme.cs scripts/vscode TaskMaster.runsettings coverage.config .csharpierignore .gitignore global.json dotnet-tools.json`; `git -C WORKTREE status --porcelain --untracked-files=all`.
  - Acceptance, all required: `HEAD-SHA:` records the first output as an observation; `BRANCH:` reads `bug/focus-and-theme-tests-leak-shared-dispatcher-setup-968`; `ORIGIN-MAIN-SHA:` records the third output as an observation (the P6-T9 `BASE REF MOVED` comparison basis); the ancestor check exits 0 (otherwise `BASE-SHA MISMATCH`: record and stop); the merge-base command prints exactly `94287369908cc920b21b0e3256314f988ad7d2f5` (otherwise `BASE AHEAD OF BRANCH`: record both values and stop; the operator brings the branch up to origin/main outside the plan and restarts at P0-T1); `INHERITED-COMMITTED:` lists every name-status line or `NONE`, and every listed path is under FEATURE, under `docs/features/potential/`, or under `.claude/agent-memory/` (otherwise `INHERITED SET OUT OF SCOPE`: stop); the scoped `--exit-code` diff exits 0, recorded as `CODE-TREE-AT-BASE: UNCHANGED` (otherwise `CITED TREE ADVANCED`: stop, because every line citation in this plan is against BASE); `PRE-EXISTING-WORKTREE-PATHS:` lists every porcelain line verbatim or `NONE`, and no line names a path under QuickFiler/ or QuickFiler.Test/ (otherwise `CODE TREE DIRTY AT ANCHOR`: stop). The porcelain output is not asserted empty: untracked FEATURE documents and modified `.claude/agent-memory/` files are expected.
- [ ] [P0-T4] Provision the repository .NET SDK with scripts/vscode/Install-RepoDotNetSdk.ps1 (guarded) and record it in FEATURE/evidence/baseline/bootstrap-sdk.md.
  - Command: `pwsh -NoProfile -Command 'PREFIX; if (-not (Test-Path -LiteralPath ".dotnet-sdk\sdk\8.0.205")) { & (Join-Path (Get-Location).Path "scripts\vscode\Install-RepoDotNetSdk.ps1") }; "SDK_MARKER=$(Test-Path -LiteralPath ".dotnet-sdk\sdk\8.0.205")"; dotnet --version; "DOTNET_EXIT=$LASTEXITCODE"'` (PREFIX expanded to its three lines, joined with semicolons).
  - Acceptance: `WORKTREE-LEAF: agent-a291a7fbabf9d0229`, `SDK_MARKER=True`, `DOTNET_EXIT=0`, and the version line is a version string rather than the global.json `errorMessage` text. Any installer line naming a resolved path is transcribed with `REDACTED-PATH`.
- [ ] [P0-T5] Restore the manifest tools with `dotnet tool restore` at the worktree root (manifest dotnet-tools.json) and record it in FEATURE/evidence/baseline/bootstrap-tool-restore.md.
  - Command: `pwsh -NoProfile -Command 'PREFIX; dotnet tool restore; "RESTORE_EXIT=$LASTEXITCODE"; dotnet tool list --local; dotnet tool run csharpier check --help | Out-Null; "CHECK_HELP_EXIT=$LASTEXITCODE"'`.
  - Acceptance: `WORKTREE-LEAF: agent-a291a7fbabf9d0229`, `RESTORE_EXIT=0`, a local tool row with Package Id `csharpier` and Version `1.2.6`, and `CHECK_HELP_EXIT=0`. Only the Package Id and Version columns are transcribed (the Manifest column carries an absolute path).
- [ ] [P0-T6] Restore NuGet packages with scripts/vscode/Invoke-Restore.ps1 and record it in FEATURE/evidence/baseline/bootstrap-nuget-restore.md.
  - Command: `pwsh -NoProfile -Command 'PREFIX; $env:MSBUILDDISABLENODEREUSE = "1"; & (Join-Path (Get-Location).Path "scripts\vscode\Invoke-Restore.ps1"); "RESTORE_EXIT=$LASTEXITCODE"; "PACKAGE_DIRS=$(@(Get-ChildItem -LiteralPath packages -Directory -ErrorAction SilentlyContinue).Count)"'`.
  - Acceptance: `WORKTREE-LEAF: agent-a291a7fbabf9d0229`, `RESTORE_EXIT=0` and `PACKAGE_DIRS=` at least 1.
- [ ] [P0-T7] Verify analyzer-path alignment across every first-party project file (every `*.csproj` outside `packages\` and `.claude\`) and record it in FEATURE/evidence/baseline/analyzer-alignment.md.
  - Command: `pwsh -NoProfile -Command 'PREFIX; $root = (Get-Location).Path; $rootLen = $root.TrimEnd([char]92).Length; $projs = @(Get-ChildItem -Path $root -Recurse -Filter "*.csproj" | Where-Object { $rel = $_.FullName.Substring($rootLen); $rel -notlike "\packages\*" -and $rel -notlike "\.claude\*" }); "PROJECTS=$($projs.Count)"; $missing = 0; $skew = 0; foreach ($p in $projs) { $dir = $p.DirectoryName; [xml]$x = Get-Content -LiteralPath $p.FullName -Raw; foreach ($a in @($x.SelectNodes("//*[local-name()=""Analyzer""]"))) { $inc = $a.GetAttribute("Include"); if (-not (Test-Path -LiteralPath (Join-Path $dir $inc))) { $missing++; "MISSING " + $p.FullName.Substring($rootLen) + " :: " + $inc } }; $pc = Join-Path $dir "packages.config"; if (Test-Path -LiteralPath $pc) { [xml]$c = Get-Content -LiteralPath $pc -Raw; foreach ($id in @("Meziantou.Analyzer", "Roslynator.Analyzers")) { $pin = @($c.SelectNodes("//package[@id=""$id""]") | ForEach-Object { $_.GetAttribute("version") }); $inc = @($x.SelectNodes("//*[local-name()=""Analyzer""]") | ForEach-Object { $_.GetAttribute("Include") } | Where-Object { $_.Contains("\$id.") }); foreach ($i in $inc) { if ($pin.Count -eq 0 -or -not $i.Contains("\$id." + $pin[0] + "\")) { $skew++; "SKEW " + $p.FullName.Substring($rootLen) + " :: " + $i } } } } }; "ANALYZER_MISSING=$missing"; "VERSION_SKEW=$skew"'`.
  - Acceptance: `WORKTREE-LEAF: agent-a291a7fbabf9d0229`, `PROJECTS=` at least 1, `ANALYZER_MISSING=0` and `VERSION_SKEW=0`. A non-zero value is `ANALYZER PATH SKEW`: record every `MISSING` and `SKEW` line and stop; it is an environment defect, not a plan defect, and no version number is asserted here because pins move.
- [ ] [P0-T8] Provision the dotnet-coverage global tool (guarded) and record it in FEATURE/evidence/baseline/bootstrap-dotnet-coverage.md.
  - Command: `pwsh -NoProfile -Command 'if (-not (Get-Command dotnet-coverage -ErrorAction SilentlyContinue)) { dotnet tool install --global dotnet-coverage }; "DOTNET_COVERAGE_RESOLVED=$($null -ne (Get-Command dotnet-coverage -ErrorAction SilentlyContinue))"; dotnet-coverage --version'` (no PREFIX: the command touches no repository path; `WORKTREE-LEAF:` is recorded as `not applicable`).
  - Acceptance: `DOTNET_COVERAGE_RESOLVED=True`, a version line is printed, `EXIT_CODE: 0`.
- [ ] [P0-T9] Capture the read-only formatter baseline with `dotnet tool run csharpier check .` and record it in FEATURE/evidence/baseline/csharpier-check-baseline.md.
  - Command: `pwsh -NoProfile -Command 'PREFIX; dotnet tool run csharpier check .; "CSHARPIER_EXIT_CODE: $LASTEXITCODE"'`.
  - Acceptance: `WORKTREE-LEAF: agent-a291a7fbabf9d0229`; `EXIT_CODE:` is the printed `CSHARPIER_EXIT_CODE:` value; the success-case line beginning `Checked ` and ending `ms.` (observed in the #950 baseline as `Checked 1637 files in 5251ms.`) is transcribed; `EXIT_CODE: 0` is the gate. A non-zero value lists every reported path and is `FORMAT BASELINE NOT CLEAN`: stop, because the CLAUDE.md format step would then rewrite files outside the Write Set and the decision belongs to the orchestrator.
- [ ] [P0-T10] Capture the analyzer baseline with `CMD-REBUILD` (analyzer GATEARGS, `TASKID` p0-t10) against WORKTREE/TaskMaster.sln and record it in FEATURE/evidence/baseline/msbuild-analyzer-baseline.md.
  - Acceptance, all required: `WORKTREE-LEAF: agent-a291a7fbabf9d0229`; `EXIT_CODE: 0`; `ERRORS: 0`; `SKIP_CORECOMPILE_LINES: 0`; `CSC_OUT_QUICKFILER:` and `CSC_OUT_QUICKFILER_TEST:` each at least 1; `TEST_DLL_EXISTS: True`; `UCS_TEST_DLL_EXISTS: True`; `WARNINGS:` recorded as `ANALYZER-BASELINE-WARNINGS:`; `WRITESET_DIAGNOSTIC_LINES:` and `WRITESET_DIAGNOSTIC_CODES:` recorded as `ANALYZER-BASELINE-WRITESET-LINES:` and `ANALYZER-BASELINE-WRITESET-CODES:` (the comparison basis for P6-T3). A non-zero exit is `ANALYZER BASELINE NOT CLEAN`: stop.
- [ ] [P0-T11] Capture the nullable baseline with `CMD-REBUILD` (GATEARGS `/p:TreatWarningsAsErrors=true`, no Nullable property override, `TASKID` p0-t11) against WORKTREE/TaskMaster.sln and record it in FEATURE/evidence/baseline/msbuild-nullable-baseline.md.
  - Acceptance, all required: `WORKTREE-LEAF: agent-a291a7fbabf9d0229`; `EXIT_CODE: 0`; `ERRORS: 0`; `SKIP_CORECOMPILE_LINES: 0`; both `CSC_OUT_` counts at least 1; both `_DLL_EXISTS:` values `True`. A non-zero exit is `NULLABLE BASELINE NOT CLEAN`: stop.
- [ ] [P0-T12] Record the pre-change census of the four existing Write Set code files in FEATURE/evidence/baseline/census-baseline.md, using `CMD-LINECOUNT` and `CMD-HASH` on CS4, `CMD-CENSUS`, `CMD-TOKEN-COUNT` once per CS4 file and `CMD-SPAN-TOKEN-COUNT` for `R4SPAN`, `R4HEAD`, `R4TAIL`, `ENSURE` and `SCOPE`.
  - Token lists: FIX `"_pinCount", "_fixtureInstalledParked", "lock (FieldLock)", "CompareExchange(", "return new EnsureScope(", "leaks exactly", "A scope that installed nothing", "pins for the process lifetime", "install-ownership flag"`; FAT `"EnsureUiThreadDispatcher", "private static Mock<IItemViewer> BuildExecutingViewer", "QfcItemControllerTestSupport.BuildExecutingViewer()", "BuildExecutingViewer", "absorbs the delegate without running it", "shared UiThread static is irrelevant", "absorbs the queued application", "[TestMethod]"`; TS `"Becomes moot", "leaks exactly", "still delegate to a callee", "not reachable from another test file", "remaining legitimate", "QfcItemController_UiThreadDispatcherPinCountTests", "internal static void EnsureSynchronizationContext()", "UiThreadDispatcherFixture.EnsureDispatcher();"`; FT `"no other class may dispose", "removed that pin: the fixture now counts pins", "(W5) must not latch", "[Timeout(GateTimeoutMs)]", "private const int GateTimeoutMs = 60000;", "EnsureUiThreadDispatcher()", "issue #230 lost update", "the waiter cannot observe the pre-restore value", "[TestMethod]"`; `R4SPAN` `"EnsureUiThreadDispatcher()", "using (", "transactionA.Dispose();", "finally", ".BeSameAs(", ".NotBeSameAs(", "issue #230 lost update"`; `R4HEAD` `"EnsureUiThreadDispatcher()", "using (", "try"`; `R4TAIL` `"}", "finally", "transactionA.Dispose();"`; `ENSURE` `"_pinCount++", "_fixtureInstalledParked = true;", "lock (FieldLock)", "return new EnsureScope("`; `SCOPE` `"CompareExchange(", "lock (FieldLock)", "_pinCount--", "_fixtureInstalledParked = false;", "DispatcherField.SetValue(null, null);"`.
  - Acceptance (each value is derived in facts 1 to 5 and is a falsifiable pre-change observation): `WORKTREE-LEAF: agent-a291a7fbabf9d0229`; LINES 342, 497, 440, 470 in CS4 order; four HASH values recorded as `BASE-HASH:` lines; CMD-CENSUS `PRIMARY_LINES: 9` with PRIMARY-FILE fixture 1, test support 2, fixture tests 4, focus-and-theme 2, `CROSS_LINES: 20` with CROSS-FILE fixture 5, test support 2, InitializationTests.Part2 1, fixture tests 10, focus-and-theme 2, and `CONTROL_LINES: 23`; FIX tokens 0, 0, 4, 3, 2, 1, 1, 0, 0; FAT tokens 2, 1, 0, 9, 0, 0, 0, 17; TS tokens 1, 1, 1, 1, 0, 0, 1, 1; FT tokens 1, 0, 1, 8, 1, 4, 1, 1, 8; `R4SPAN` 1, 2, 1, 2, 1, 1, 1 with `SPAN: 212-284`; `R4HEAD` 1, 1, 1 with `SPAN: 212-224`; `R4TAIL` 3, 1, 0 with `SPAN: 267-273`; `ENSURE` 0, 0, 1, 2 with `SPAN: 122-145`; `SCOPE` 1, 0, 0, 0, 0 with `SPAN: 249-283`. Any differing value is `CENSUS MISMATCH`: record and stop, because a later gate is defined against these values. The non-zero counts (the two focus-and-theme ensure calls, the private helper, the four stale doc tokens, the R4 pin) are the positive controls for the zero gates of P4-T2 and P5-T1.
- [ ] [P0-T13] Capture the pre-change concurrent run of QfcItemController_UiThreadDispatcherFixtureTests and QfcItemController_FocusAndThemeTests from QuickFiler.Test/bin/Debug/QuickFiler.Test.dll with `CMD-VSTEST` (`FILTER-BASELINE-CONCURRENT`, `TASKID` p0-t13, empty `NAMES`) and record it in FEATURE/evidence/baseline/concurrent-set-baseline.md.
  - Acceptance: `WORKTREE-LEAF: agent-a291a7fbabf9d0229`; `TRX_PRESENT: True`; `SEQUENCE_FILES: 0`; `RESULT_COUNT: 25`; the `COUNTERS` line recorded as `BASELINE-CONCURRENT-COUNTERS:`; every `RESULT` line transcribed; `BASELINE-CONCURRENT-FAILED:` lists every non-Passed name with its `MESSAGE` line, or `NONE` (the comparison basis for P4-T7). Outcomes are observations and are not gated; `ExpectedExitCode:` carries the observed value when non-zero. A `Timeout` or `Aborted` outcome or a Sequence file is `BASELINE HANG`: stop.
- [ ] [P0-T14] Run the stall probe from UtilitiesCS.Test/bin/Debug/UtilitiesCS.Test.dll with `CMD-VSTEST` (`ASSEMBLY` the UtilitiesCS.Test assembly, `FILTER-STALL`, `TASKID` p0-t14, empty `NAMES`) and record FEATURE/evidence/baseline/stall-probe.md.
  - Acceptance: the artifact records `WORKTREE-LEAF:`, `EXIT_CODE:`, `ExpectedExitCode:` equal to the observed value when non-zero (presentational), `TRX_PRESENT:`, `SEQUENCE_FILES:`, the `COUNTERS` line when present and every `MESSAGE` line; then exactly one `STALL-PROBE:` line — `CLEAR` when `EXIT_CODE: 0`, `failed=0` and `SEQUENCE_FILES: 0`, otherwise `REPRODUCES` — and exactly one `COVERAGE-ROUTE:` line — `RUNNER` under CLEAR, `DIRECT` under REPRODUCES. The probe runs once and is never re-run. Both values complete this task.
- [ ] [P0-T15] Capture the baseline repository-wide test-and-coverage run by the route P0-T14 fixed and record FEATURE/evidence/baseline/coverage-summary.md and FEATURE/evidence/baseline/coverage-jacoco-projection.md: under RUNNER run `CMD-COVERAGE-RUNNER` with `STAGE` baseline, under DIRECT run `CMD-COVERAGE-DIRECT` with `STAGE` baseline; then, unless branch (d) applies, run `CMD-COVERAGE-POST` with `STAGE` baseline and `RAW` per its rule.
  - Artifacts: coverage-summary.md carries `Timestamp:`, `Command:` (both payloads, with the route's canonical command), `EXIT_CODE:` (`RUNNER_EXIT_CODE:` or `COLLECT_EXIT_CODE:`), `ExpectedExitCode:` equal to the observed value when non-zero (a baseline observation), and an `Output Summary:` recording `WORKTREE-LEAF:`, `COVERAGE-ROUTE:`, `RAW:`, `DISCOVERED_LINE:` or `ASSEMBLY_COUNT:` with every `ASSEMBLY:` line, `TRX_PRESENT:`, `SEQUENCE_FILES:` (DIRECT), `THRESHOLD_MESSAGE:` and `COLLECT_FAILURE_MESSAGE:` (RUNNER), `LINE-FLOOR:`, `BRANCH-FLOOR:`, the `First-party coverage:` line (the numeric baseline headline: lines covered over valid with percentage, branches likewise), the `ROOT` line, the five summary lines verbatim between `SUMMARY-BEGIN` and `SUMMARY-END`, `FAILED-SET:` recorded as `BASELINE-FAILED-SET:`, the `RESULT` lines (ten at baseline: the pin-count tests do not exist yet), `TEST_ASSEMBLY_PACKAGES:` and `CHANGED-CODE-COVERAGE: NOT MEASURED (TEST ASSEMBLY EXCLUDED)`; coverage-jacoco-projection.md carries `Timestamp:`, a `Source:` line naming coverage-summary.md, and the projection verbatim between `PROJECTION-BEGIN` and `PROJECTION-END`.
  - Branches, checked in order: (d) `TRX_PRESENT: False`, `SEQUENCE_FILES:` greater than 0, or a non-zero exit with an empty `FAILED-SET:` and no floor message, is `COVERAGE RUN ABORTED`: stop, report the last lines of the log with paths redacted, do not re-run. (c) `TEST_ASSEMBLY_PACKAGES:` other than 0 is `TEST ASSEMBLY INSTRUMENTED`: stop, because D-7 rests on the derived exclusion. (b) a non-zero exit with a non-empty `FAILED-SET:` or a floor `NOT MET` is recorded as `BASELINE-STATE: PRE-EXISTING FAILURES` (with `BASELINE-FLOOR:` naming any floor not met) and completes this task (D-8). (a) exit 0 with both floors met is `BASELINE-STATE: GREEN` and completes this task.
- [ ] [P0-T16] Write the baseline toolchain index FEATURE/evidence/baseline/toolchain-baseline.md from the P0-T9, P0-T10, P0-T11 and P0-T15 artifacts.
  - Acceptance: `Timestamp:`; one row per step in order — csharpier check, analyzer rebuild, TreatWarningsAsErrors rebuild, coverage run (route) — each with its canonical command, `EXIT_CODE:` copied from the step artifact, and the step artifact's path; `BASELINE-STATE:` copied from P0-T15; the `First-party coverage:` line copied from P0-T15. This file is an index over the per-step artifacts, not a substitute for them.
- [ ] [P0-T17] Commit the Phase 0 evidence and the feature documents (FEATURE only) and record it in FEATURE/evidence/baseline/phase0-commit.md.
  - Commands, separate Bash calls: `git -C WORKTREE add -- docs/features/active/2026-10-02-focus-and-theme-tests-leak-shared-dispatcher-setup-968`; `git -C WORKTREE commit -m "docs(968): record phase 0 baseline evidence and the amended spec"`; `git -C WORKTREE rev-parse HEAD`; `git -C WORKTREE status --porcelain --untracked-files=all`.
  - Acceptance: both git commands exit 0; `PHASE0-COMMIT:` records the new HEAD as an observation; no porcelain line names a path under FEATURE other than this plan file (whose check-off mark is written after the commit) and FEATURE/evidence/baseline/phase0-commit.md (written after the commit), and no porcelain line names a path under QuickFiler/ or QuickFiler.Test/. This artifact itself is committed in P4-T8.

### Phase 1 — Regression Test First (fail-before on the unmodified fixture)

- [ ] [P1-T1] Create QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherPinCountTests.cs with the whole content of Delivered Source N1 (Write tool, absolute path), then normalise its line endings with `CMD-EOL`.
  - Acceptance (CMD-EOL output recorded by P1-T3): `WORKTREE-LEAF: agent-a291a7fbabf9d0229`; `BARE_LF: 0`; `CRLF_COUNT:` equals `LINES:`; `LINES:` at most 500 and at least 200.
- [ ] [P1-T2] Insert Delivered Source T1 into QuickFiler.Test/QuickFiler.Test.csproj immediately after line 203 (`<Compile Include="Controllers\QfcItemController.UiThreadDispatcherFixtureTests.cs" />`), as an in-place Edit.
  - Acceptance (recorded by P1-T3): exactly one line of the project file contains `Controllers\QfcItemController.UiThreadDispatcherPinCountTests.cs`, it begins with four spaces and `<Compile Include=`, and the line before it is the fixture-tests item.
- [ ] [P1-T3] Record the new-file census in FEATURE/evidence/regression-testing/pin-count-file-census.md with the P1-T1 CMD-EOL output, `CMD-TOKEN-COUNT` on PC (TOKENS `"EnsureUiThreadDispatcher()", "Regression test: fails before the fix", "Specification test: passes before and after the fix", "never read the shared static", "[TestClass]", "[TestMethod]", "[Timeout(GateTimeoutMs)]", "private const int GateTimeoutMs = 60000;", "transaction.Install(null);", "transaction.Install(live);", "transaction.Dispose();", "QfcItemControllerTestSupport.ShutdownDispatcher(live);", "a holder that did not take the last pin must not lose the dispatcher", "the last release reverts the fixture", "using Moq;", "Thread.Sleep", "Task.Delay", "public class QfcItemController_UiThreadDispatcherPinCountTests"`), `CMD-TOKEN-COUNT` on PROJ (TOKENS `"Controllers\QfcItemController.UiThreadDispatcherPinCountTests.cs", "Controllers\QfcItemController.UiThreadDispatcherFixtureTests.cs"`), `git -C WORKTREE diff --numstat HEAD -- QuickFiler.Test/QuickFiler.Test.csproj` paired with `git -C WORKTREE status --porcelain -- QuickFiler QuickFiler.Test`.
  - Acceptance: PC tokens 9, 1, 3, 1, 1, 4, 4, 1, 3, 1, 4, 1, 2, 2, 0, 0, 0, 1; PROJ tokens 1 and 1; numstat for the project file reads `1	0`; the porcelain span lists exactly ` M QuickFiler.Test/QuickFiler.Test.csproj` and `?? QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherPinCountTests.cs` (recorded as `PHASE1-PORCELAIN:`, the comparison basis for P2-T8). Any other value: correct the edit and re-run this task.
- [ ] [P1-T4] Build the tree with the new test file compiled against the unmodified fixture using `CMD-BUILD` (`TASKID` p1-t4) against WORKTREE/TaskMaster.sln and record it in FEATURE/evidence/regression-testing/fail-before-build.md.
  - Acceptance: `WORKTREE-LEAF: agent-a291a7fbabf9d0229`, `EXIT_CODE: 0`, `ERRORS: 0`, `TEST_DLL_ADVANCED: True`, `CSC_OUT_QUICKFILER_TEST:` at least 1. A compile error in the new file is a defect in N1: correct it within the delivered design, re-run P1-T3 and this task.
- [ ] [P1-T5] [expect-fail] Run the regression test alone against the unmodified fixture from QuickFiler.Test/bin/Debug/QuickFiler.Test.dll with `CMD-VSTEST` (`FILTER-PC-T1`, `TASKID` p1-t5, `NAMES` `"EnsureDispatcher_TwoPinsHeld_ReleasingTheFirstKeepsTheDispatcherUntilTheLastRelease"`) and record FEATURE/evidence/regression-testing/fail-before-pin-count.md.
  - Acceptance: `WORKTREE-LEAF: agent-a291a7fbabf9d0229`; `EXIT_CODE: 1`; `ExpectedExitCode: 1`; `TRX_PRESENT: True`; `SEQUENCE_FILES: 0`; `RESULT_COUNT: 1`; the `RESULT` line reads `Failed` with its duration recorded; the `MESSAGE` line contains `to refer to`, `ParkedDispatcher`, `a holder that did not take the last pin must not lose the dispatcher` and `but found <null>` (fact 13: the first-release assertion failed because the first pin's release nulled the field). The artifact states that the fixture is at BASE content (P0-T12 `BASE-HASH:` for FIX is re-derived with `CMD-HASH` on `"FIX"` in this task and must match), that the test ran alone so the baseline was null (fact 7), and that this run is the fail-before half of AC5. A `Passed` result is `REGRESSION DID NOT FAIL`: stop and report, because the root-cause claim rests on it. A `Failed` result whose `MESSAGE` lacks `to refer to` or the because text (for example an exception, or the `NotBeNull` because text `the first pin seeds the parked dispatcher into a null field`) is `FAIL-BEFORE WRONG REASON`: stop and report, because the test is then defective rather than the fixture.
- [ ] [P1-T6] Run the three specification tests against the unmodified fixture from QuickFiler.Test/bin/Debug/QuickFiler.Test.dll with `CMD-VSTEST` (`FILTER-PC-T234`, `TASKID` p1-t6, `NAMES` the last three names of `NAMES-PC`) and record FEATURE/evidence/regression-testing/specification-tests-before-fix.md.
  - Acceptance: `WORKTREE-LEAF: agent-a291a7fbabf9d0229`; `EXIT_CODE: 0`; `TRX_PRESENT: True`; `SEQUENCE_FILES: 0`; `RESULT_COUNT: 3`; three `RESULT` lines, one per name, each `Passed` with duration recorded (the "passes before and after the fix" half of the labels AC6 requires). Any `Failed` is `SPECIFICATION TEST FAILS BEFORE FIX`: stop and report, because the spec's design trace for that test is then wrong.

### Phase 2 — Fixture Fix (reference-counted pin and D1 documentation)

- [ ] [P2-T1] Insert Delivered Source F-FIELDS into QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixture.cs after line 38 (`private static Dispatcher _parkedDispatcher = null;`), as an in-place Edit.
  - Acceptance (recorded by P2-T6): one line contains `private static int _pinCount;` and one contains `private static bool _fixtureInstalledParked;`, both inside the class's static field block above the issue #743 counters.
- [ ] [P2-T2] Insert Delivered Source F-CLASSDOC into QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixture.cs after pre-edit line 30 (`/// </para>` closing the design-note paragraph, now shifted by P2-T1 only if P2-T1 is applied first; apply this task by content, locating the `/// </para>` line that immediately precedes the class `/// </summary>`).
  - Acceptance (recorded by P2-T6): `install-ownership flag` 1 and `pins for the process lifetime` at least 1 in the file; the class doc ends with the new paragraph followed by `/// </summary>`.
- [ ] [P2-T3] Replace the `EnsureDispatcher` summary (pre-edit lines 116 to 121, located by content: from the `/// <summary>` immediately above `internal static IDisposable EnsureDispatcher()` to the `/// </summary>` immediately above it) in QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixture.cs with Delivered Source F-ENSURE-DOC.
  - Acceptance (recorded by P2-T6): `leaks exactly` 0; `pins for the process lifetime` 2 in the file.
- [ ] [P2-T4] Replace the `EnsureDispatcher` body (pre-edit lines 128 to 137, located by content: from the `lock (FieldLock)` line after `Dispatcher parked = GetParkedDispatcher();` through `return new EnsureScope(null);`) in QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixture.cs with Delivered Source F-ENSURE-BODY.
  - Acceptance (recorded by P2-T6): within `ENSURE`, `_pinCount++` 1, `_fixtureInstalledParked = true;` 1, `lock (FieldLock)` 1, `return new EnsureScope(` 1.
- [ ] [P2-T5] Replace the `EnsureScope` documentation and class (pre-edit lines 243 to 274, located by content: from the `/// <summary>` immediately above `private sealed class EnsureScope : IDisposable` through the class's closing brace, the `}` immediately before the fixture class's closing `}`) in QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixture.cs with Delivered Source F-SCOPE.
  - Acceptance (recorded by P2-T6): within `SCOPE`, `CompareExchange(` 0, `lock (FieldLock)` 1, `_pinCount--` 1, `_fixtureInstalledParked = false;` 1, `DispatcherField.SetValue(null, null);` 1; `A scope that installed nothing` 0 in the file.
- [ ] [P2-T6] Record the fixture-change census in FEATURE/evidence/qa-gates/fixture-change-census.md with `CMD-LINECOUNT` on `"FIX"`, `CMD-TOKEN-COUNT` on FIX (the P0-T12 FIX token list), `CMD-SPAN-TOKEN-COUNT` on `ENSURE` and `SCOPE` (the P0-T12 token lists), `git -C WORKTREE diff --numstat HEAD -- QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixture.cs`, `git -C WORKTREE diff HEAD -- QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixture.cs`, paired with `git -C WORKTREE status --porcelain -- QuickFiler QuickFiler.Test`.
  - Acceptance: `WORKTREE-LEAF: agent-a291a7fbabf9d0229`; FIX LINES 374; FIX tokens 4, 4, 5, 2, 1, 0, 0, 2, 1 (in the P0-T12 order: `_pinCount`, `_fixtureInstalledParked`, `lock (FieldLock)`, `CompareExchange(`, `return new EnsureScope(`, `leaks exactly`, `A scope that installed nothing`, `pins for the process lifetime`, `install-ownership flag`); `ENSURE` 1, 1, 1, 1; `SCOPE` 0, 1, 1, 1, 1; the porcelain span lists exactly ` M QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixture.cs`, ` M QuickFiler.Test/QuickFiler.Test.csproj` and `?? QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherPinCountTests.cs`; and a `FIELDLOCK-ENCLOSURE:` section that, reading the transcribed diff, names the two `lock (FieldLock)` blocks (in `EnsureDispatcher` and in `EnsureScope.Dispose`) and states that the three non-declaration occurrences of each new field lie inside them and that no `CompareExchange` call appears in the scope class (the AC9 reading). Any other count: correct the edit and re-run this task.
- [ ] [P2-T7] Build the fixed fixture with `CMD-BUILD` (`TASKID` p2-t7) against WORKTREE/TaskMaster.sln and record it in FEATURE/evidence/regression-testing/pass-after-build.md.
  - Acceptance: `WORKTREE-LEAF: agent-a291a7fbabf9d0229`, `EXIT_CODE: 0`, `ERRORS: 0`, `TEST_DLL_ADVANCED: True`, `CSC_OUT_QUICKFILER_TEST:` at least 1.
- [ ] [P2-T8] Run the four pin-count tests against the fixed fixture from QuickFiler.Test/bin/Debug/QuickFiler.Test.dll with `CMD-VSTEST` (`FILTER-PC-CLASS`, `TASKID` p2-t8, `NAMES-PC`) and record FEATURE/evidence/regression-testing/pass-after-pin-count.md, together with `git -C WORKTREE status --porcelain -- QuickFiler QuickFiler.Test`.
  - Acceptance: `WORKTREE-LEAF: agent-a291a7fbabf9d0229`; `EXIT_CODE: 0`; `TRX_PRESENT: True`; `SEQUENCE_FILES: 0`; `RESULT_COUNT: 4`; four `RESULT` lines, one per NAMES-PC name, each `Passed` with duration recorded; the porcelain span (`PHASE2-PORCELAIN:`) equals `PHASE1-PORCELAIN:` from P1-T3 plus exactly one extra line ` M QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixture.cs`, recorded as the AC5 statement that the only difference between the P1-T5 run and this run is the fixture file. A `Failed` regression test is `FIX DID NOT TAKE`: correct the fixture within D-1 and re-run from P2-T6.

### Phase 3 — Theme-Test Deletions, R4 Restructure and D2 to D6

- [ ] [P3-T1] Apply Delivered Source A1 to QuickFiler.Test/Controllers/QfcItemController.FocusAndThemeTests.cs: delete lines 99 to 116 (the private `BuildExecutingViewer` method and the blank line after it), located by content as the block from `private static Mock<IItemViewer> BuildExecutingViewer()` through its closing `}` plus the following blank line.
  - Acceptance (recorded by P3-T9): `private static Mock<IItemViewer> BuildExecutingViewer` 0; the line after the `BuildFocusController` method's closing `}` and one blank line is the `/// <summary>` of `EnableHandlelessThemeInvoke`.
- [ ] [P3-T2] Apply Delivered Source A2 to QuickFiler.Test/Controllers/QfcItemController.FocusAndThemeTests.cs: replace all seven `var viewer = BuildExecutingViewer();` lines with `var viewer = QfcItemControllerTestSupport.BuildExecutingViewer();` and replace the six-line cycle-3 comment block with the seven-line A2 block.
  - Acceptance (recorded by P3-T9): `QfcItemControllerTestSupport.BuildExecutingViewer()` 8 and `BuildExecutingViewer` 8 (every remaining mention is prefixed).
- [ ] [P3-T3] Apply Delivered Source A3 to `SetThemeDark_FromNormal_SelectsDarkNormalTheme` in QuickFiler.Test/Controllers/QfcItemController.FocusAndThemeTests.cs: replace the two arrange comment lines and the `QfcItemControllerTestSupport.EnsureUiThreadDispatcher();` line with the five A3 comment lines.
  - Acceptance (recorded by P3-T9): `absorbs the delegate without running it` 1 and `shared UiThread static is irrelevant` 1; the first statement of the test is `var controller = new FocusController();`.
- [ ] [P3-T4] Apply Delivered Source A4 to `SetThemeLight_FromNormal_SelectsLightNormalTheme` in QuickFiler.Test/Controllers/QfcItemController.FocusAndThemeTests.cs: replace the `// Arrange` line and the `QfcItemControllerTestSupport.EnsureUiThreadDispatcher();` line with the two A4 comment lines.
  - Acceptance (recorded by P3-T9): `absorbs the queued application` 1; `EnsureUiThreadDispatcher` 0 in the file.
- [ ] [P3-T5] Replace the `EnsureUiThreadDispatcher` documentation (lines 216 to 237, located by content: from the `/// <summary>` whose next line begins `/// Ensures the static <c>UiThread.Dispatcher</c>` to the `/// </summary>` immediately above `internal static IDisposable EnsureUiThreadDispatcher() =>`) in QuickFiler.Test/Controllers/QfcItemController.TestSupport.cs with Delivered Source S1.
  - Acceptance (recorded by P3-T9): `Becomes moot` 0, `leaks exactly` 0, `still delegate to a callee` 0, `remaining legitimate` 1, `QfcItemController_UiThreadDispatcherPinCountTests` 1; the declaration and body lines are unchanged (`UiThreadDispatcherFixture.EnsureDispatcher();` 1).
- [ ] [P3-T6] Replace the `BuildExecutingViewer` documentation (pre-edit lines 282 to 288, located by content: from the `/// <summary>` whose next line begins `/// Issue #480 shared arrange helper.` to the `/// </summary>` immediately above `internal static Mock<IItemViewer> BuildExecutingViewer()`) in QuickFiler.Test/Controllers/QfcItemController.TestSupport.cs with Delivered Source S2.
  - Acceptance (recorded by P3-T9): `not reachable from another test file` 0; `Issue #480 shared arrange helper` 1.
- [ ] [P3-T7] Replace the R4 `<para>` paragraph (lines 196 to 208, located by content: from the `/// <para>` whose next line begins `/// Issue #950: the earlier intermittent failure` to the following `/// </para>`) in QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixtureTests.cs with Delivered Source R-DOC.
  - Acceptance (recorded by P3-T9): `no other class may dispose` 0, `removed that pin: the fixture now counts pins` 1, `(W5) must not latch` 1.
- [ ] [P3-T8] Apply Delivered Source R-BODY to `Transaction_SecondCallerCannotInstallUntilTheFirstRestores` in QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixtureTests.cs: replace the four-line `using (` header (lines 221 to 224) with `try` and `{`, and replace the sixteen-space `}` at line 270 with the five-line `}` / `finally` / `{` / `transactionA.Dispose();` / `}` block; lines 225 to 269 are unchanged.
  - Acceptance (recorded by P3-T9): `R4SPAN` `EnsureUiThreadDispatcher()` 0, `using (` 1, `transactionA.Dispose();` 2, `finally` 3, `.BeSameAs(` 1, `.NotBeSameAs(` 1, `issue #230 lost update` 1; `R4HEAD` 0, 0, 2; `R4TAIL` `}` 4, `finally` 2, `transactionA.Dispose();` 1; `[Timeout(GateTimeoutMs)]` 8 and `private const int GateTimeoutMs = 60000;` 1 unchanged.
- [ ] [P3-T9] Record the test-edit census in FEATURE/evidence/qa-gates/test-edit-census.md with `CMD-LINECOUNT` on CS5, `CMD-TOKEN-COUNT` on FAT, TS and FT (the P0-T12 lists, FT extended with `"Issue #480 shared arrange helper"` for TS), `CMD-SPAN-TOKEN-COUNT` on `R4SPAN`, `R4HEAD` and `R4TAIL` (the P0-T12 lists), `CMD-HUNKS` on `QuickFiler.Test/Controllers/QfcItemController.TestSupport.cs` and on `QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixtureTests.cs`, and `git -C WORKTREE diff --numstat HEAD -- QuickFiler.Test` paired with `git -C WORKTREE status --porcelain -- QuickFiler QuickFiler.Test`.
  - Acceptance: `WORKTREE-LEAF: agent-a291a7fbabf9d0229`; LINES 374, 482, 442, 472 for FIX, FAT, TS, FT and at most 500 for PC; FAT tokens 0, 0, 8, 8, 1, 1, 1, 17; TS tokens 0, 0, 0, 0, 1, 1, 1, 1 then `Issue #480 shared arrange helper` 1; FT tokens 0, 1, 1, 8, 1, 3, 1, 1, 8; `R4SPAN` 0, 1, 2, 3, 1, 1, 1; `R4HEAD` 0, 0, 2; `R4TAIL` 4, 2, 1; TestSupport `HUNK_COUNT: 2` with every `HUNK` old-range start at or above 200 (the `EnsureSynchronizationContext` region 85 to 96 is untouched, AC17); fixture-tests hunks each with old-range start at or above 190 and old-range start plus old-range length at or below 285 (every hunk lies in R4's doc and body, AC10); the porcelain span lists exactly the five `.cs` Write Set paths (four ` M`, one `??`) and ` M QuickFiler.Test/QuickFiler.Test.csproj`. Any other value: correct the edit and re-run this task.

### Phase 4 — Scoped Format, Pass-After Runs and Implementation Commit

- [ ] [P4-T1] Format the five Write Set `.cs` files (CS5) with a scoped CSharpier pass and record the before-and-after hashes in FEATURE/evidence/qa-gates/scoped-format.md.
  - Command: `CMD-HASH` on CS5; then `pwsh -NoProfile -Command 'PREFIX; dotnet tool run csharpier format QuickFiler.Test\Controllers\QfcItemController.UiThreadDispatcherFixture.cs QuickFiler.Test\Controllers\QfcItemController.FocusAndThemeTests.cs QuickFiler.Test\Controllers\QfcItemController.TestSupport.cs QuickFiler.Test\Controllers\QfcItemController.UiThreadDispatcherFixtureTests.cs QuickFiler.Test\Controllers\QfcItemController.UiThreadDispatcherPinCountTests.cs; "CSHARPIER_EXIT_CODE: $LASTEXITCODE"'`; then `CMD-HASH` on CS5 again.
  - Acceptance: `WORKTREE-LEAF: agent-a291a7fbabf9d0229`; `CSHARPIER_EXIT_CODE: 0`; the line beginning `Formatted ` is transcribed and labelled as a processed-file count, not a rewrite count; `REWRITTEN:` lists every CS5 path whose hash differs between the two captures, or `NONE` (the rewrite observation is the hash difference, not the console line). Either value completes this task: the pass exists so the committed text is formatter-stable.
- [ ] [P4-T2] Record the post-format census in FEATURE/evidence/qa-gates/post-format-census.md by re-running every P2-T6, P3-T9 and P1-T3 command (CMD-LINECOUNT on CS5, the five CMD-TOKEN-COUNT lists including PC and PROJ, the ENSURE, SCOPE, R4SPAN, R4HEAD and R4TAIL spans, CMD-HUNKS on TestSupport and the fixture tests, numstat paired with the porcelain span), plus `CMD-PIN-NESTING` on `T3SPAN`.
  - Acceptance: every P2-T6, P3-T9 and P1-T3 token, span and hunk value holds after formatting (LINES may differ from 374, 482, 442, 472 only if `REWRITTEN:` named the file, and every CS5 LINES value is at most 500); `T3SPAN` NEST output ends with four lines whose tokens are, in order, `finally`, `Dispose()` (the `transaction.Dispose();` line), `finally`, `ShutdownDispatcher(` (the AC3 reading that the live dispatcher is shut down in a finally block); the FIELDLOCK-ENCLOSURE reading of P2-T6 is restated against the formatted diff. This artifact is the evidence for AC6, AC7, AC9, AC11, AC12, AC13, AC15, AC16, AC17 and the census half of AC3, AC10, AC14 and AC21.
- [ ] [P4-T3] Build the formatted tree with `CMD-BUILD` (`TASKID` p4-t3) against WORKTREE/TaskMaster.sln and record it in FEATURE/evidence/regression-testing/implementation-build.md.
  - Acceptance: `WORKTREE-LEAF: agent-a291a7fbabf9d0229`, `EXIT_CODE: 0`, `ERRORS: 0`, `TEST_DLL_ADVANCED: True`, `CSC_OUT_QUICKFILER_TEST:` at least 1.
- [ ] [P4-T4] Run the pin-count class alone from QuickFiler.Test/bin/Debug/QuickFiler.Test.dll with `CMD-VSTEST` (`FILTER-PC-CLASS`, `TASKID` p4-t4, `NAMES-PC`) and record FEATURE/evidence/regression-testing/pin-count-class-pass-after.md.
  - Acceptance: `WORKTREE-LEAF: agent-a291a7fbabf9d0229`; `EXIT_CODE: 0`; `TRX_PRESENT: True`; `SEQUENCE_FILES: 0`; `RESULT_COUNT: 4`; four `RESULT` lines, each `Passed`, durations recorded. Any other outcome invokes the D-13 Phase 4 restart rule.
- [ ] [P4-T5] Run the fixture test class alone from QuickFiler.Test/bin/Debug/QuickFiler.Test.dll with `CMD-VSTEST` (`FILTER-FT-CLASS`, `TASKID` p4-t5, `NAMES-FT`) and record FEATURE/evidence/regression-testing/fixture-class-pass-after.md.
  - Acceptance: `WORKTREE-LEAF: agent-a291a7fbabf9d0229`; `EXIT_CODE: 0`; `TRX_PRESENT: True`; `SEQUENCE_FILES: 0`; `RESULT_COUNT: 8`; eight `RESULT` lines, one per NAMES-FT name, each `Passed`, durations recorded (R1 to R6 and the #743 and #882 tests pass with their assertions unchanged, AC10; R4 passes without its pin and with the `try/finally`, AC14). Any other outcome invokes the D-13 Phase 4 restart rule.
- [ ] [P4-T6] Run the focus-and-theme class alone from QuickFiler.Test/bin/Debug/QuickFiler.Test.dll with `CMD-VSTEST` (`FILTER-FAT-CLASS`, `TASKID` p4-t6, `NAMES-THEME`) and record FEATURE/evidence/regression-testing/focus-and-theme-class-pass-after.md.
  - Acceptance: `WORKTREE-LEAF: agent-a291a7fbabf9d0229`; `EXIT_CODE: 0`; `TRX_PRESENT: True`; `SEQUENCE_FILES: 0`; `RESULT_COUNT: 17`; the two NAMES-THEME `RESULT` lines each `Passed` with durations recorded; `COUNTERS` shows `passed=17 failed=0` (every test in the class passes after the helper switch, AC15, and the theme tests pass without the deleted calls, AC7). Any other outcome invokes the D-13 Phase 4 restart rule.
- [ ] [P4-T7] Run the three classes together in one invocation from QuickFiler.Test/bin/Debug/QuickFiler.Test.dll under the CLI runsettings with `CMD-VSTEST` (`FILTER-CONCURRENT`, `TASKID` p4-t7, empty `NAMES`) and record FEATURE/evidence/regression-testing/concurrent-set-test-summary.md.
  - Acceptance: `WORKTREE-LEAF: agent-a291a7fbabf9d0229`; `EXIT_CODE: 0`; `TRX_PRESENT: True`; `SEQUENCE_FILES: 0`; `RESULT_COUNT: 29`; the `COUNTERS` line shows `total=29 executed=29 passed=29 failed=0`; every `RESULT` line transcribed and `Passed`; `CONCURRENT-NOT-PASSED: NONE`. The artifact states that this is a supporting observation under Workers 0 and ClassLevel scope (MSTest cannot be made to interleave classes on demand), not the regression gate. Any non-Passed outcome is compared with P0-T13 `BASELINE-CONCURRENT-FAILED:`: a name present there is recorded as pre-existing and completes the task with `ExpectedExitCode:` equal to the observed value; a new failure invokes the D-13 Phase 4 restart rule.
- [ ] [P4-T8] Commit the implementation (the six Write Set code files and FEATURE) and record FEATURE/evidence/qa-gates/implementation-commit.md.
  - Commands, separate Bash calls: `git -C WORKTREE add -- QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixture.cs QuickFiler.Test/Controllers/QfcItemController.FocusAndThemeTests.cs QuickFiler.Test/Controllers/QfcItemController.TestSupport.cs QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixtureTests.cs QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherPinCountTests.cs QuickFiler.Test/QuickFiler.Test.csproj docs/features/active/2026-10-02-focus-and-theme-tests-leak-shared-dispatcher-setup-968`; `git -C WORKTREE commit -m "fix(968): reference-count the UiThreadDispatcherFixture ensure pin and remove the dead theme-test calls"`; `git -C WORKTREE rev-parse HEAD`; `git -C WORKTREE diff --name-status 94287369908cc920b21b0e3256314f988ad7d2f5 HEAD`; `git -C WORKTREE status --porcelain --untracked-files=all`.
  - Acceptance: both git writes exit 0; `IMPLEMENTATION-COMMIT:` records the new HEAD as an observation; the name-status diff lists the five modified Write Set code paths with status `M`, the pin-count file with status `A`, FEATURE paths, and otherwise only paths from P0-T3 `INHERITED-COMMITTED:`; no porcelain line names a path under QuickFiler/ or QuickFiler.Test/. This artifact is committed in P6-T38.

### Phase 5 — Call-Site Census and Prohibited-Construct Gate

- [ ] [P5-T1] Record the post-change call-site census (two independent strategies and the member-set comparison) in FEATURE/evidence/qa-gates/call-site-census.md with `CMD-CENSUS`.
  - Acceptance: `WORKTREE-LEAF: agent-a291a7fbabf9d0229`; `CS_FILES:` recorded; `PRIMARY_LINES: 15` with PRIMARY-FILE fixture 1, test support 2, fixture tests 3, pin-count tests 9 and no focus-and-theme entry; `CROSS_LINES: 31` with CROSS-FILE fixture 5, test support 3, InitializationTests.Part2 1, fixture tests 8, pin-count tests 14 and no focus-and-theme entry; `CONTROL_LINES: 27`; every `PRIMARY` and `CROSS` line transcribed. The artifact then classifies every PRIMARY line as one of `DECLARATION` (fixture `internal static IDisposable EnsureDispatcher()`, test support `internal static IDisposable EnsureUiThreadDispatcher() =>`), `FORWARDER` (test support `UiThreadDispatcherFixture.EnsureDispatcher();`) or `INVOCATION` (the twelve test-side lines), and every CROSS line not in the PRIMARY set as `DOC`, `COMMENT` or `TEST-NAME` by reading its transcribed text, and records `MEMBER-SET-COMPARISON: AGREE` when the CROSS set contains every PRIMARY line and every CROSS-only line is non-executable (sixteen such lines: fixture 4, test support 1, Part2 1, fixture tests 5, pin-count tests 5). Any CROSS-only line that is an invocation (for example a call written across two lines) is `CENSUS MISMATCH`: stop and report. A `PRIMARY_LINES` or `CROSS_LINES` value other than stated is likewise `CENSUS MISMATCH`.
- [ ] [P5-T2] Append the nesting classification to FEATURE/evidence/qa-gates/call-site-census.md with `CMD-PIN-NESTING` on `R1SPAN`, `R2SPAN`, `R3SPAN`, `R4SPAN`, `T1SPAN`, `T2SPAN`, `T3SPAN` and `T4SPAN`.
  - Acceptance: for each of R1SPAN, R2SPAN, R3SPAN, T1SPAN, T2SPAN, T3SPAN and T4SPAN the NEST output shows, by ascending line number, a `BeginTransactionAsync()` line, then every `.Install(` line, then every `EnsureUiThreadDispatcher()` line, then the `Dispose()` line of every pin variable (`ensureScope`, `pinA`, `pinB`, `freshPin`), then the first `Dispose()` line of the transaction variable (`transaction.Dispose();`); R4SPAN shows no `EnsureUiThreadDispatcher()` line and shows two `transactionA.Dispose();` lines, the second inside a `finally`; the artifact records, per method, `NESTED: YES` and `INSTALL-BETWEEN-PIN-ACQUIRE-AND-RELEASE: NONE` (no `.Install(` line number lies between a pin's acquisition and its release), and records `INVOCATIONS-CLASSIFIED: 12 of 12 nested` (three in the fixture tests, nine in the pin-count tests). Any other ordering is `NESTING VIOLATION`: stop and report, because the spec's determinism invariant then does not hold.
- [ ] [P5-T3] Record the prohibited-construct gate in FEATURE/evidence/qa-gates/prohibited-constructs-grep.md with `CMD-ADDED-SCAN`, `git -C WORKTREE diff --exit-code 94287369908cc920b21b0e3256314f988ad7d2f5 HEAD -- scripts/vscode/TaskMaster.cli.runsettings TaskMaster.runsettings`, `git -C WORKTREE status --porcelain -- scripts/vscode/TaskMaster.cli.runsettings TaskMaster.runsettings`, and `CMD-TOKEN-COUNT` on FT (TOKENS `"[Timeout(GateTimeoutMs)]", "private const int GateTimeoutMs = 60000;"`).
  - Acceptance: `WORKTREE-LEAF: agent-a291a7fbabf9d0229`; `GIT_DIFF_EXIT_CODE: 0`; `ADDED_LINES:` greater than 0; `ADDED-TOKEN` counts 0 for `Thread.Sleep`, `Task.Delay`, `DoNotParallelize`, `Retry(`, `Path.GetTempFileName`, `Path.GetTempPath` and `Workers`; `Timeout(` 4 and `[Timeout(GateTimeoutMs)]` 4 (every added timeout attribute is the sibling file's constant convention); `GateTimeoutMs = ` 1 with the single `ADDED-LINE` reading `private const int GateTimeoutMs = 60000;` (no timeout increase: the value equals the sibling constant); `_pinCount` at least 1 (positive control); the runsettings diff exits 0 and the porcelain span prints nothing; the FT counts read 8 and 1, equal to P0-T12. Any non-zero prohibited count is `PROHIBITED CONSTRUCT ADDED`: stop and report.

### Phase 6 — Final QA Loop, Coverage Comparison, Static Gates, Check-offs and Final Commit

- [ ] [P6-T1] Run the CLAUDE.md formatting step `dotnet tool run csharpier format .` at the worktree root (WORKTREE/.) with a tree observation before and after, and record FEATURE/evidence/qa-gates/csharpier-format-final.md.
  - Commands: `git -C WORKTREE status --porcelain --untracked-files=all`; `CMD-HASH` on CS5; `pwsh -NoProfile -Command 'PREFIX; dotnet tool run csharpier format .; "CSHARPIER_EXIT_CODE: $LASTEXITCODE"'`; `CMD-HASH` on CS5; `git -C WORKTREE status --porcelain --untracked-files=all`.
  - Acceptance: `ITERATION:` recorded; `WORKTREE-LEAF: agent-a291a7fbabf9d0229`; `CSHARPIER_EXIT_CODE: 0`; the `Formatted ` line transcribed and labelled as a processed-file count; `REWRITTEN-WRITESET:` lists every CS5 path whose hash changed, or `NONE`; `REWRITTEN-OTHER:` lists every porcelain path present after and absent before, or `NONE`. Clean pass: both `NONE`. A non-empty `REWRITTEN-WRITESET:` with `REWRITTEN-OTHER: NONE` invokes the D-13 format restart; a non-empty `REWRITTEN-OTHER:` is `FORMAT TOUCHED OUT-OF-SCOPE FILE`: stop.
- [ ] [P6-T2] Run the read-only formatter gate `dotnet tool run csharpier check .` at the worktree root (WORKTREE/.) and record FEATURE/evidence/qa-gates/csharpier-check-final.md.
  - Command: `pwsh -NoProfile -Command 'PREFIX; dotnet tool run csharpier check .; "CSHARPIER_EXIT_CODE: $LASTEXITCODE"'`.
  - Acceptance: `ITERATION:` recorded; `WORKTREE-LEAF: agent-a291a7fbabf9d0229`; `EXIT_CODE: 0`; the success-case line beginning `Checked ` and ending `ms.` transcribed verbatim.
- [ ] [P6-T3] Run the analyzer rebuild with `CMD-REBUILD` (analyzer GATEARGS, `TASKID` p6-t3) against WORKTREE/TaskMaster.sln and record FEATURE/evidence/qa-gates/msbuild-analyzer-final.md.
  - Acceptance: `ITERATION:` recorded; `WORKTREE-LEAF: agent-a291a7fbabf9d0229`; `EXIT_CODE: 0`; `ERRORS: 0`; `SKIP_CORECOMPILE_LINES: 0`; both `CSC_OUT_` counts at least 1; `WRITESET_DIAGNOSTIC_LINES:` not greater than `ANALYZER-BASELINE-WRITESET-LINES:` and every code in `WRITESET_DIAGNOSTIC_CODES:` present in `ANALYZER-BASELINE-WRITESET-CODES:` (no new analyzer diagnostic in a Write Set file); `WARNINGS:` recorded beside `ANALYZER-BASELINE-WARNINGS:`.
- [ ] [P6-T4] Run the type-check rebuild with `CMD-REBUILD` (GATEARGS `/p:TreatWarningsAsErrors=true`, no Nullable override, `TASKID` p6-t4) against WORKTREE/TaskMaster.sln and record FEATURE/evidence/qa-gates/msbuild-nullable-final.md.
  - Acceptance: `ITERATION:` recorded; `WORKTREE-LEAF: agent-a291a7fbabf9d0229`; `EXIT_CODE: 0`; `ERRORS: 0`; `SKIP_CORECOMPILE_LINES: 0`; both `CSC_OUT_` counts at least 1; both `_DLL_EXISTS:` values `True`.
- [ ] [P6-T5] Run the final repository-wide test-and-coverage run by the P0-T14 route (`CMD-COVERAGE-RUNNER` or `CMD-COVERAGE-DIRECT` with `STAGE` final, then `CMD-COVERAGE-POST` with `STAGE` final) and record FEATURE/evidence/qa-gates/coverage-summary.md and FEATURE/evidence/qa-gates/coverage-jacoco-projection.md.
  - Artifacts: the same fields as P0-T15, with `ITERATION:`, `FAILED-SET:` recorded as `FINAL-FAILED-SET:`, and fourteen `RESULT` lines (NAMES-TARGETS).
  - Acceptance, all required: the route equals P0-T14's; `TRX_PRESENT: True`; `SEQUENCE_FILES:` 0 under DIRECT; `TEST_ASSEMBLY_PACKAGES: 0`; the fourteen `RESULT` lines all `Passed` (the four pin-count tests appear as passed in the coverage route's test-result summary, AC21); `NEW-FAILURES:` (names in `FINAL-FAILED-SET:` absent from `BASELINE-FAILED-SET:`) is `NONE`; `FIGURES-COMPARED:` restates the `Total`, `executed`, `error`, `timeout`, `aborted` and `notExecuted` figures from the P0-T15 summary block and from this run's summary block, and this run holds `Total` equal to the P0-T15 value plus 4 (this plan adds four test methods and removes none), `executed` not less than the P0-T15 value plus 4, and each of `error`, `timeout`, `aborted` and `notExecuted` not greater than its P0-T15 value; `LINE-FLOOR:` and `BRANCH-FLOOR:` each `MET`, or `NOT MET` only where P0-T15 recorded the same floor as not met; the `First-party coverage:` line is recorded as the numeric post-change headline. `RUNNER-GREEN: YES` is recorded when the route is RUNNER and `RUNNER_EXIT_CODE: 0`, otherwise `RUNNER-GREEN: NO` with the reason (`COVERAGE-ROUTE DIRECT` or `PRE-EXISTING FAILURES`). A failure of any target test or a new failure attributable to the Write Set invokes the D-13 failure restart; any other new failure, and any `FIGURES-COMPARED:` breach, is `NEW FAILURE OUTSIDE SCOPE`: stop and report, without re-running.
- [ ] [P6-T6] Compare baseline and post-change coverage and test outcomes and record FEATURE/evidence/qa-gates/coverage-comparison.md from FEATURE/evidence/baseline/coverage-summary.md and FEATURE/evidence/qa-gates/coverage-summary.md.
  - Acceptance: the artifact records the baseline and post-change `First-party coverage:` lines (lines and branches, numeric) and the two `ROOT` lines; `FIRST-PARTY-LINE-DELTA:` and `FIRST-PARTY-BRANCH-DELTA:` as post-change percentage minus baseline percentage (two decimals, signed); `AC23-STATUS: MET` when both deltas are at least 0.00, otherwise `AC23-STATUS: NOT MET (COVERAGE-VARIANCE)` with both deltas; the repository-wide comparison in exactly one named branch: `BRANCH A` when the two `lines-valid` figures differ by at most 1 percent of the baseline figure (the post-change line rate must not be lower than baseline by more than 0.5 percentage points), otherwise `BRANCH B` (recorded and not gated, with one sentence stating the denominators are not comparable); `CHANGED-CODE-COVERAGE: NOT MEASURED (TEST ASSEMBLY EXCLUDED)` with `TEST_ASSEMBLY_PACKAGES: 0` at both stages as its reason; and `BASELINE-FAILED-SET:`, `FINAL-FAILED-SET:` and `NEW-FAILURES: NONE` restated. A Branch A breach is `COVERAGE REGRESSION`: stop; an `AC23-STATUS: NOT MET` with Branch A satisfied is recorded and does not stop the run (D-7).
- [ ] [P6-T7] Record the final toolchain pass in FEATURE/evidence/qa-gates/toolchain-final.md from the P6-T1 to P6-T5 artifacts of the final iteration.
  - Acceptance: one row per step, in order — csharpier format (`REWRITTEN-WRITESET: NONE`, `REWRITTEN-OTHER: NONE`), csharpier check (exit 0), analyzer rebuild (exit 0, `SKIP_CORECOMPILE_LINES: 0`), TreatWarningsAsErrors rebuild (exit 0, `SKIP_CORECOMPILE_LINES: 0`), coverage run (route, exit code, `RUNNER-GREEN:`) — each with its exact command, `EXIT_CODE:` and the same `ITERATION:` value; `SINGLE-PASS: YES` when all five rows come from one iteration with no restart after P6-T1; `AC22-STATUS: MET` only when `SINGLE-PASS: YES` and `RUNNER-GREEN: YES`, otherwise `AC22-STATUS: NOT MET` with the reason.
- [ ] [P6-T8] Record the file-size gate in FEATURE/evidence/qa-gates/file-line-counts.md with `CMD-LINECOUNT` on CS5 and `CMD-TOKEN-COUNT` on PROJ (TOKENS `"<Compile Include="`).
  - Acceptance: `WORKTREE-LEAF: agent-a291a7fbabf9d0229`; five `LINES` values, each at most 500, each equal to the P4-T2 value for the same file (the P6-T1 format rewrote nothing); the project file's `<Compile Include=` count recorded (the project file is not a C# source file and the 500-line limit does not apply to it; it is recorded for completeness).
- [ ] [P6-T9] Record the footprint gate in FEATURE/evidence/qa-gates/footprint-scope.md with `git -C WORKTREE diff --name-status 94287369908cc920b21b0e3256314f988ad7d2f5 HEAD` paired with `git -C WORKTREE status --porcelain --untracked-files=all`, and `git -C WORKTREE rev-parse origin/main`.
  - Acceptance: `THIS-ITEM-FOOTPRINT:` (name-status paths outside FEATURE, excluding every path P0-T3 listed in `INHERITED-COMMITTED:`) is exactly the five modified Write Set code paths with status `M` and the pin-count file with status `A`; every path in it is under `QuickFiler.Test/`; the excluded inherited paths are recorded as `INHERITED-AND-EXCLUDED:`; no status `A` outside FEATURE other than the pin-count file and `INHERITED-AND-EXCLUDED:` paths; no porcelain line names a path under QuickFiler/, QuickFiler.Test/ or scripts/; `ORIGIN-MAIN-NOW:` records the rev-parse output and `BASE-REF-MOVED:` reads `NO` when it equals `94287369908cc920b21b0e3256314f988ad7d2f5` and otherwise `YES` (recorded, not a stop: every gate names BASE explicitly).
- [ ] [P6-T10] Record the evidence hygiene gate in FEATURE/evidence/qa-gates/evidence-hygiene.md by scanning every file under FEATURE/evidence/.
  - Command: `pwsh -NoProfile -Command 'PREFIX; $b = [char]92; $files = @(Get-ChildItem -LiteralPath "docs\features\active\2026-10-02-focus-and-theme-tests-leak-shared-dispatcher-setup-968\evidence" -Recurse -File); "EVIDENCE_FILES=$($files.Count)"; "RAW_DOCUMENTS=$(@($files | Where-Object { $_.Extension -in @(".trx", ".xml", ".coverage", ".coveragexml", ".log") }).Count)"; $pattern = "[a-z]:[" + $b + $b + "/]+users[" + $b + $b + "/]+[a-z0-9_.~-]"; "PROFILE_PATH_LINES=$(@($files | Select-String -Pattern $pattern).Count)"'`.
  - Acceptance: `WORKTREE-LEAF: agent-a291a7fbabf9d0229`; `EVIDENCE_FILES=` at least 1; `RAW_DOCUMENTS=0`; `PROFILE_PATH_LINES=0` (the pattern is the repository hygiene rule's, built from `[char]92` so the Bash channel cannot collapse its backslashes). A non-zero value names the offending files; the executor redacts them and re-runs this task.

- [ ] [P6-T11] Check off AC1 in FEATURE/spec.md when `EnsureDispatcher_TwoPinsHeld_ReleasingTheFirstKeepsTheDispatcherUntilTheLastRelease` is `Passed` in FEATURE/evidence/regression-testing/pass-after-pin-count.md and FEATURE/evidence/qa-gates/coverage-summary.md.
  - Acceptance: only `- [ ] AC1:` changes to `- [x] AC1:`; otherwise the box stays unchecked and `AC1: NOT MET` with the reason is recorded for P6-T35.
- [ ] [P6-T12] Check off AC2 in FEATURE/spec.md when `EnsureDispatcher_TwoPinsHeld_ReleasingTheFirstKeepsTheDispatcherUntilTheLastRelease` and `EnsureDispatcher_TwoPinsHeld_ReleaseOrderDoesNotChangeTheOutcome` are both `Passed` in FEATURE/evidence/regression-testing/pass-after-pin-count.md and FEATURE/evidence/qa-gates/coverage-summary.md.
  - Acceptance: only `- [ ] AC2:` changes; otherwise unchecked with `AC2: NOT MET`.
- [ ] [P6-T13] Check off AC3 in FEATURE/spec.md when `EnsureDispatcher_UnderATransactionHoldingALiveDispatcher_ReleasingAllPinsLeavesTheLiveDispatcher` is `Passed` in FEATURE/evidence/regression-testing/pass-after-pin-count.md and FEATURE/evidence/qa-gates/coverage-summary.md, `EnsureDispatcher_WhileATransactionHoldsALiveDispatcher_DoesNotReplaceIt` is `Passed` in FEATURE/evidence/regression-testing/fixture-class-pass-after.md, and FEATURE/evidence/qa-gates/post-format-census.md shows the T3SPAN NEST tail `finally`, `Dispose()`, `finally`, `ShutdownDispatcher(` and the PC token `QfcItemControllerTestSupport.ShutdownDispatcher(live);` 1.
  - Acceptance: only `- [ ] AC3:` changes; otherwise unchecked with `AC3: NOT MET`.
- [ ] [P6-T14] Check off AC4 in FEATURE/spec.md when `EnsureDispatcher_AfterAFullPinCycle_AFreshSinglePinStillInstallsAndRestores` is `Passed` in FEATURE/evidence/regression-testing/pass-after-pin-count.md and FEATURE/evidence/qa-gates/coverage-summary.md.
  - Acceptance: only `- [ ] AC4:` changes; otherwise unchecked with `AC4: NOT MET`.
- [ ] [P6-T15] Check off AC5 in FEATURE/spec.md when FEATURE/evidence/regression-testing/fail-before-pin-count.md records `Failed` with a `MESSAGE` containing `to refer to`, the because text and `but found <null>`, FEATURE/evidence/regression-testing/pass-after-pin-count.md records the same test `Passed`, and the `PHASE2-PORCELAIN:` span differs from `PHASE1-PORCELAIN:` (FEATURE/evidence/regression-testing/pin-count-file-census.md) by exactly the fixture file.
  - Acceptance: only `- [ ] AC5:` changes; otherwise unchecked with `AC5: NOT MET`.
- [ ] [P6-T16] Check off AC6 in FEATURE/spec.md when FEATURE/evidence/qa-gates/post-format-census.md shows the PC tokens `Regression test: fails before the fix` 1, `Specification test: passes before and after the fix` 3 and `never read the shared static` 1, and FEATURE/evidence/regression-testing/specification-tests-before-fix.md records the three specification tests `Passed` before the fix.
  - Acceptance: only `- [ ] AC6:` changes; otherwise unchecked with `AC6: NOT MET`.
- [ ] [P6-T17] Check off AC7 in FEATURE/spec.md when FEATURE/evidence/qa-gates/post-format-census.md shows the FAT token `EnsureUiThreadDispatcher` 0 and FEATURE/evidence/regression-testing/focus-and-theme-class-pass-after.md records `SetThemeDark_FromNormal_SelectsDarkNormalTheme` and `SetThemeLight_FromNormal_SelectsLightNormalTheme` both `Passed`.
  - Acceptance: only `- [ ] AC7:` changes; otherwise unchecked with `AC7: NOT MET`.
- [ ] [P6-T18] Check off AC8 in FEATURE/spec.md when FEATURE/evidence/qa-gates/call-site-census.md records `MEMBER-SET-COMPARISON: AGREE`, `INVOCATIONS-CLASSIFIED: 12 of 12 nested` and `INSTALL-BETWEEN-PIN-ACQUIRE-AND-RELEASE: NONE` for every pin-bearing method.
  - Acceptance: only `- [ ] AC8:` changes; otherwise unchecked with `AC8: NOT MET`.
- [ ] [P6-T19] Check off AC9 in FEATURE/spec.md when FEATURE/evidence/qa-gates/post-format-census.md shows FIX tokens `_pinCount` 4, `_fixtureInstalledParked` 4 and `lock (FieldLock)` 5, SCOPE `CompareExchange(` 0 and `lock (FieldLock)` 1, and its `FIELDLOCK-ENCLOSURE:` reading states both fields are private statics whose every use lies inside a `lock (FieldLock)` block.
  - Acceptance: only `- [ ] AC9:` changes; otherwise unchecked with `AC9: NOT MET`.
- [ ] [P6-T20] Check off AC10 in FEATURE/spec.md when FEATURE/evidence/regression-testing/fixture-class-pass-after.md records all eight NAMES-FT tests `Passed` and FEATURE/evidence/qa-gates/post-format-census.md shows every fixture-tests hunk inside the R4 doc-and-body range and the R4SPAN tokens `.BeSameAs(` 1, `.NotBeSameAs(` 1, `issue #230 lost update` 1, with the FT token `the waiter cannot observe the pre-restore value` 1.
  - Acceptance: only `- [ ] AC10:` changes; otherwise unchecked with `AC10: NOT MET`.
- [ ] [P6-T21] Check off AC11 in FEATURE/spec.md when FEATURE/evidence/qa-gates/post-format-census.md shows FIX tokens `leaks exactly` 0, `A scope that installed nothing` 0, `pins for the process lifetime` 2 and `install-ownership flag` 1.
  - Acceptance: only `- [ ] AC11:` changes; otherwise unchecked with `AC11: NOT MET`.
- [ ] [P6-T22] Check off AC12 in FEATURE/spec.md when FEATURE/evidence/qa-gates/post-format-census.md shows TS tokens `Becomes moot` 0, `leaks exactly` 0, `still delegate to a callee` 0, `remaining legitimate` 1 and `QfcItemController_UiThreadDispatcherPinCountTests` 1.
  - Acceptance: only `- [ ] AC12:` changes; otherwise unchecked with `AC12: NOT MET`.
- [ ] [P6-T23] Check off AC13 in FEATURE/spec.md when FEATURE/evidence/qa-gates/post-format-census.md shows FT tokens `no other class may dispose` 0, `removed that pin: the fixture now counts pins` 1 and `(W5) must not latch` 1.
  - Acceptance: only `- [ ] AC13:` changes; otherwise unchecked with `AC13: NOT MET`.
- [ ] [P6-T24] Check off AC14 in FEATURE/spec.md when FEATURE/evidence/qa-gates/post-format-census.md shows R4SPAN `transactionA.Dispose();` 2 and R4TAIL `finally` 2 with `transactionA.Dispose();` 1, and FEATURE/evidence/regression-testing/fixture-class-pass-after.md records `Transaction_SecondCallerCannotInstallUntilTheFirstRestores` and `Transaction_DisposedTwice_DoesNotOverReleaseTheGate` both `Passed`.
  - Acceptance: only `- [ ] AC14:` changes; otherwise unchecked with `AC14: NOT MET`.
- [ ] [P6-T25] Check off AC15 in FEATURE/spec.md when FEATURE/evidence/qa-gates/post-format-census.md shows FAT tokens `private static Mock<IItemViewer> BuildExecutingViewer` 0, `QfcItemControllerTestSupport.BuildExecutingViewer()` 8 and `BuildExecutingViewer` 8, the TS token `not reachable from another test file` 0, and FEATURE/evidence/regression-testing/focus-and-theme-class-pass-after.md records `passed=17 failed=0`.
  - Acceptance: only `- [ ] AC15:` changes; otherwise unchecked with `AC15: NOT MET`.
- [ ] [P6-T26] Check off AC16 in FEATURE/spec.md when FEATURE/evidence/qa-gates/post-format-census.md shows FAT tokens `absorbs the delegate without running it` 1, `shared UiThread static is irrelevant` 1 and `absorbs the queued application` 1.
  - Acceptance: only `- [ ] AC16:` changes; otherwise unchecked with `AC16: NOT MET`.
- [ ] [P6-T27] Check off AC17 in FEATURE/spec.md when FEATURE/evidence/qa-gates/post-format-census.md shows the TestSupport `HUNK_COUNT: 2` with every hunk old-range start at or above 200 and the TS token `internal static void EnsureSynchronizationContext()` 1.
  - Acceptance: only `- [ ] AC17:` changes; otherwise unchecked with `AC17: NOT MET`.
- [ ] [P6-T28] Check off AC18 in FEATURE/spec.md when FEATURE/evidence/qa-gates/file-line-counts.md shows every CS5 `LINES` value at most 500.
  - Acceptance: only `- [ ] AC18:` changes; otherwise unchecked with `AC18: NOT MET`.
- [ ] [P6-T29] Check off AC19 in FEATURE/spec.md when FEATURE/evidence/qa-gates/prohibited-constructs-grep.md holds every P5-T3 condition.
  - Acceptance: only `- [ ] AC19:` changes; otherwise unchecked with `AC19: NOT MET`.
- [ ] [P6-T30] Check off AC20 in FEATURE/spec.md when FEATURE/evidence/qa-gates/footprint-scope.md shows `THIS-ITEM-FOOTPRINT:` entirely under `QuickFiler.Test/` and every other name-status path under FEATURE or in `INHERITED-AND-EXCLUDED:`.
  - Acceptance: only `- [ ] AC20:` changes; otherwise unchecked with `AC20: NOT MET`.
- [ ] [P6-T31] Check off AC21 in FEATURE/spec.md when FEATURE/evidence/qa-gates/post-format-census.md shows the PROJ token `Controllers\QfcItemController.UiThreadDispatcherPinCountTests.cs` 1 and FEATURE/evidence/qa-gates/coverage-summary.md records all four NAMES-PC tests `Passed`.
  - Acceptance: only `- [ ] AC21:` changes; otherwise unchecked with `AC21: NOT MET`.
- [ ] [P6-T32] Check off AC22 in FEATURE/spec.md when FEATURE/evidence/qa-gates/toolchain-final.md reads `AC22-STATUS: MET`.
  - Acceptance: only `- [ ] AC22:` changes; otherwise the box stays unchecked and the recorded reason (`COVERAGE-ROUTE DIRECT`, `PRE-EXISTING FAILURES` or a restart after P6-T1) is carried to P6-T35 as `AC22: NOT MET`.
- [ ] [P6-T33] Check off AC23 in FEATURE/spec.md when FEATURE/evidence/qa-gates/coverage-comparison.md reads `AC23-STATUS: MET`.
  - Acceptance: only `- [ ] AC23:` changes; otherwise the box stays unchecked and `AC23: NOT MET (COVERAGE-VARIANCE)` with both deltas is carried to P6-T35.
- [ ] [P6-T34] Check off AC24 in FEATURE/spec.md when FEATURE/evidence/regression-testing/concurrent-set-test-summary.md records `RESULT_COUNT: 29` and `CONCURRENT-NOT-PASSED: NONE`.
  - Acceptance: only `- [ ] AC24:` changes; otherwise unchecked with `AC24: NOT MET`.
- [ ] [P6-T35] Write the acceptance-criteria status summary FEATURE/evidence/other/ac-status-summary.md.
  - Acceptance: the artifact carries `Timestamp:` and the acceptance-criteria-tracking status block (Source: the spec path; Total AC items: 24; Checked off; Remaining; Items remaining with each `ACn: NOT MET` reason), with the counts read from FEATURE/spec.md after P6-T34 (lines beginning `- [x] AC` and `- [ ] AC`, summing to 24).
- [ ] [P6-T36] Re-run the P6-T10 hygiene command over FEATURE/evidence/ after P6-T35 and append the result to FEATURE/evidence/qa-gates/evidence-hygiene.md as a `## Re-run after check-offs` section.
  - Acceptance: `RAW_DOCUMENTS=0` and `PROFILE_PATH_LINES=0` for the evidence tree as it will be committed.
- [ ] [P6-T37] Verify that FEATURE/spec.md changed only in its checkbox lines by recording `git -C WORKTREE diff --numstat HEAD -- docs/features/active/2026-10-02-focus-and-theme-tests-leak-shared-dispatcher-setup-968/spec.md` and `git -C WORKTREE diff HEAD -- docs/features/active/2026-10-02-focus-and-theme-tests-leak-shared-dispatcher-setup-968/spec.md`, paired with `git -C WORKTREE status --porcelain -- docs/features/active/2026-10-02-focus-and-theme-tests-leak-shared-dispatcher-setup-968/spec.md`, in a `## Spec check-off diff` section appended to FEATURE/evidence/other/ac-status-summary.md.
  - Acceptance: added and deleted line counts are equal and equal the checked-off count; every deleted line begins `- [ ] AC` and every added line begins `- [x] AC` with identical remaining text.
- [ ] [P6-T38] Commit the remaining feature evidence and check-offs (FEATURE only) and record FEATURE/evidence/qa-gates/final-commit.md.
  - Commands, separate Bash calls: `git -C WORKTREE add -- docs/features/active/2026-10-02-focus-and-theme-tests-leak-shared-dispatcher-setup-968`; `git -C WORKTREE commit -m "docs(968): record final QA evidence and acceptance check-offs"`; `git -C WORKTREE rev-parse HEAD`; `git -C WORKTREE diff --name-status 94287369908cc920b21b0e3256314f988ad7d2f5 HEAD`; `git -C WORKTREE status --porcelain --untracked-files=all`.
  - Acceptance: both git writes exit 0; `FINAL-COMMIT:` records HEAD as an observation; the name-status paths outside FEATURE are exactly the P6-T9 footprint plus `INHERITED-AND-EXCLUDED:`; the porcelain output names no path under FEATURE other than this plan file (whose final check-off mark follows the commit) and FEATURE/evidence/qa-gates/final-commit.md (written after the commit), and no path under QuickFiler/, QuickFiler.Test/ or scripts/. The final-commit artifact and this plan's check-off marks are committed by the orchestrator with the plan file. No PR is opened and no merge is performed by this plan.

## Planner self-review and internal review record

SELF-REVIEW: RE-DERIVED THIS PASS

Original authoring pass (this pass). Every citation below was read directly from the item worktree files during this planning session (no shell was available, so HEAD was not queried; P0-T3 records it):

1. QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixture.cs — read in full (342 lines): class doc 12 to 31 with `/// </para>` at 30; `_parkedDispatcher` 38; counters 44 to 46; `Current` 62 to 71; `Exchange` 77 to 85; `CompareExchange` 92 to 104; `EnsureDispatcher` doc 116 to 121 and body 122 to 138 (`lock (FieldLock)` 128, `return new EnsureScope(parked);` 133, `return new EnsureScope(null);` 137); `TransactionGateAcquireTimeoutMs` 146; parked thread name 231; `EnsureScope` doc 243 to 248 and class 249 to 274 (`CompareExchange(_installed, null)` 271); `UiThreadDispatcherTransaction` 284 to 341 (`CompareExchange` call 336). Token counts re-derived: `lock (FieldLock)` 4; `CompareExchange(` 3; `installed nothing carries` 0 on any single line (the phrase wraps 245 to 246), which is why P0-T12 and AC11's check-off use `A scope that installed nothing` instead.
2. QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixtureTests.cs — read in full (470 lines): R1 44 to 98 (pin 59 to 60, dispose 62, `transaction.Dispose();` 81 and 91); R2 107 to 149 (pin 119, dispose 121, `transaction.Dispose();` 137 and 147); R3 157 to 190 (pin 166, disposes 169 and 171); R4 doc 192 to 209 with `<para>` 196 to 208; R4 212 to 276 (`using (` 221, pin 222, `original` 225, `Install(liveA)` 226, inner `using (` 228, waiter `finally` 243, `transactionA.Dispose();` 251, `issue #230 lost update` 267, braces 269 to 271, `finally` 272, `ShutdownDispatcher(liveA)` 274); R5 285; `[Timeout(GateTimeoutMs)]` 8; `[TestMethod]` 8. Derived spans: `R4SPAN` 212-284, `R4HEAD` 212-224, `R4TAIL` 267-273 (two `}` lines plus the `secondCallerStarted` close: `}` 3, `finally` 1).
3. QuickFiler.Test/Controllers/QfcItemController.FocusAndThemeTests.cs — read in full (497 lines): private helper 99 to 115 between blanks 98 and 116; comment block 181 to 186; seven callers 193, 213, 235, 254, 314, 331, 367; theme tests 447 to 478 with ensure calls at 452 and 468 and comments 450 to 451 and 467; `[TestMethod]` 17 (Grep count).
4. QuickFiler.Test/Controllers/QfcItemController.TestSupport.cs — read in full (440 lines): `EnsureSynchronizationContext` 85 to 96; `BuildColorTheme` 161 to 181; wrapper doc 216 to 237 and method 238 to 239; `BuildExecutingViewer` doc 282 to 288 and method 289 to 305; `StartRunningDispatcher` 251 to 271; `ShutdownDispatcher` 277 to 280.
5. QuickFiler.Test/QuickFiler.Test.csproj — lines 17, 35, 196 to 215 (Compile items 200, 201, 203, 212); no `LangVersion` (Grep). QuickFiler.Test/SetupAssemblyInitializer.cs 14 to 25.
6. Repository-wide Grep results: `EnsureUiThreadDispatcher|EnsureDispatcher` 20 lines in 5 files (listed in fact 5); `EnsureUiThreadDispatcher\(\)|EnsureDispatcher\(\)` 9 lines; `BeginTransactionAsync\(` 23 lines in 6 files; `BuildExecutingViewer` 12 lines in 3 files (MailActionsTests.cs 203 is the other shared-helper caller); `DoNotParallelize` 2 lines in QuickFiler.Test, both outside the Write Set; `\r$` matches every line of the four files (CRLF).
7. Read-only context: QuickFiler/Controllers/QfcItemController.FocusAndTheme.cs 274 to 286; UtilitiesCS/HelperClasses/ThemeHelpers/Theme.cs 427 to 445; UtilitiesCS/Threading/UiThread.cs 266 to 285.
8. scripts/vscode/Invoke-MSTestWithCoverage.ps1 — 41 to 95, 97 to 134, 190 to 269, 274 to 461 (fact 9 line references); function inventory of every `Invoke-MSTest*.ps1` part file (Grep `^function `); `First-party coverage:` format at Invoke-MSTestWithCoverage.FirstParty.ps1 117 to 120; scripts/vscode/TaskMaster.cli.runsettings 1 to 9; scripts/vscode/Install-RepoDotNetSdk.ps1 1 to 11; scripts/vscode/Invoke-Restore.ps1 1 to 10; scripts/hygiene/Test-RepositoryHygiene.Rules.ps1 5 to 22 (pattern at 21).
9. .gitignore 26, 140, 141, 146, 150, 151; .gitattributes 4; .csharpierignore 4 and 12; global.json 2 to 9; dotnet-tools.json 6; coverage.config present at the root.
10. docs/features/active/2026-10-02-focus-and-theme-tests-leak-shared-dispatcher-setup-968/spec.md — read in full before amendment (290 lines; acceptance lines 250 to 273, twenty-four `- [ ] ` lines) and re-read after amendment 1.1 for the `ACn:` labels and the amended sentences; issue.md line 12; the research record in full; docs/features/potential/promoted/2026-10-02-focus-and-theme-tests-leak-shared-dispatcher-setup.md lines 1 to 12.
11. #950 evidence consulted for success-case output shapes (not for citations): `evidence/baseline/csharpier-check-baseline.md` (`Checked 1637 files in 5251ms.`), `evidence/baseline/msbuild-analyzer-baseline.md` (`CSC_OUT_` 2 and 2, `SKIP_CORECOMPILE_LINES: 0`), `evidence/baseline/coverage-baseline.md` (First-party line, ROOT line, projection and summary shapes), `evidence/regression-testing/r4-fail-before.md` (the `BeSameAs` message shape), `evidence/baseline/stall-probe.md` (`REPRODUCES` on this host), `evidence/baseline/bootstrap-sdk.md` and `bootstrap-tool-restore.md` (marker and tool-row shapes).

Sibling-region re-checks: the three `transaction.Dispose();` call shapes in R1, R2 and R3 (explicit plus `finally`) are the model for N1 and R-BODY; R1's `Install(liveA)` precedes its pin and R2/R3's `Install(null)` precede theirs, so the nesting gate's "install before pin" ordering holds for every existing site; the waiter's own `finally` at 243 lies inside R4SPAN and R4HEAD excludes it, so the R4SPAN `finally` baseline is 2 (not 1) and the post-change value 3; `ShutdownDispatcher(liveA)` also occurs at 96, 310 and 351, so R4TAIL's END is "the first occurrence after the START" and its baseline span 267-273 was checked against the text; `Dispatcher original = UiThreadDispatcherFixture.Current;` also occurs at 55 and 115, so R4HEAD's END is likewise the first occurrence after the R4 declaration; the `A scope that installed nothing` token sits on line 245 alone, so its post-change 0 is a real transition; the `pins for the process lifetime` token appears in both F-CLASSDOC and F-ENSURE-DOC and once in S1, so the FIX gate is 2 and the TS count is not gated; `_pinCount` and `_fixtureInstalledParked` appear in no delivered comment, so the FIX counts are exactly 4 and 4; the two theme tests are the only `[TestMethod]`s in FocusAndThemeTests that call the wrapper, and the seven `BuildExecutingViewer` callers plus the line-182 comment account for all nine pre-change mentions; `EmailMoveMonitorTests` and `ViewerQueueStaticWrapperTests` carry the only `[DoNotParallelize]` attributes in QuickFiler.Test and are outside every edited file, so the added-line scan's 0 is a real gate on this plan's additions.

PLANNER-INTERNAL-REVIEW: PASS
CITATION-TO-TREE: PASS
AC-TRACEABILITY: PASS
SCOPE-BOUNDARY: PASS
CITATION: QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixture.cs | lines 12-31, 34-46, 92-104, 116-138, 146, 231, 243-274, 284-341
CITATION: QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixtureTests.cs | lines 33, 44-98, 107-149, 157-190, 192-276 (R4 header 221-224, body 225-269, close 270), 285
CITATION: QuickFiler.Test/Controllers/QfcItemController.FocusAndThemeTests.cs | lines 27, 98-117, 181-186, 193, 213, 235, 254, 314, 331, 367, 447-478
CITATION: QuickFiler.Test/Controllers/QfcItemController.TestSupport.cs | lines 85-96, 161-181, 216-239, 251-280, 282-305
CITATION: QuickFiler.Test/QuickFiler.Test.csproj | lines 17, 35, 196-215
CITATION: QuickFiler.Test/SetupAssemblyInitializer.cs | lines 14-25
CITATION: QuickFiler.Test/Controllers/QfcItemController.InitializationTests.Part2.cs | line 124
CITATION: QuickFiler.Test/Controllers/QfcItemController.MailActionsTests.cs | line 203
CITATION: QuickFiler/Controllers/QfcItemController.FocusAndTheme.cs | lines 274-286
CITATION: UtilitiesCS/HelperClasses/ThemeHelpers/Theme.cs | lines 427-445
CITATION: UtilitiesCS/Threading/UiThread.cs | lines 266-285
CITATION: scripts/vscode/Invoke-MSTestWithCoverage.ps1 | lines 89-93, 97-134, 262, 297-298, 348-355, 399-423, 430-453, 459-461
CITATION: scripts/vscode/Invoke-MSTestWithCoverage.FirstParty.ps1 | lines 117-123
CITATION: scripts/vscode/TaskMaster.cli.runsettings | lines 4-7
CITATION: scripts/hygiene/Test-RepositoryHygiene.Rules.ps1 | line 21
CITATION: scripts/vscode/Install-RepoDotNetSdk.ps1 | line 3
CITATION: scripts/vscode/Invoke-Restore.ps1 | lines 1-10
CITATION: .gitignore | lines 26, 140, 141, 146, 150, 151
CITATION: .gitattributes | line 4
CITATION: .csharpierignore | lines 4, 12
CITATION: global.json | lines 2-9
CITATION: dotnet-tools.json | line 6
CITATION: docs/features/active/2026-10-02-focus-and-theme-tests-leak-shared-dispatcher-setup-968/spec.md | header lines 6-9, decision 4, R4 bullet, acceptance lines AC1 to AC24
CITATION: docs/features/active/2026-10-02-focus-and-theme-tests-leak-shared-dispatcher-setup-968/issue.md | line 12
CITATION: docs/features/active/2026-10-02-focus-and-theme-tests-leak-shared-dispatcher-setup-968/research/2026-10-02T05-50-dispatcher-pin-call-sites-research.md | sections 1.1, 2.1, 2.2, 3, 4, 5, 6, 7
AC-INVENTORY: AC1, AC2, AC3, AC4, AC5, AC6, AC7, AC8, AC9, AC10, AC11, AC12, AC13, AC14, AC15, AC16, AC17, AC18, AC19, AC20, AC21, AC22, AC23, AC24
AC-MAPPING: AC1 | IMPLEMENTATION: P1-T1, P2-T1 to P2-T5 | TESTS: P1-T5, P2-T8, P6-T5 | EVIDENCE: FEATURE/evidence/regression-testing/pass-after-pin-count.md
AC-MAPPING: AC2 | IMPLEMENTATION: P1-T1, P2-T4, P2-T5 | TESTS: P2-T8, P6-T5 | EVIDENCE: FEATURE/evidence/regression-testing/pass-after-pin-count.md
AC-MAPPING: AC3 | IMPLEMENTATION: P1-T1, P2-T5 | TESTS: P1-T6, P2-T8, P4-T5, P4-T2 | EVIDENCE: FEATURE/evidence/regression-testing/pass-after-pin-count.md
AC-MAPPING: AC4 | IMPLEMENTATION: P1-T1, P2-T5 | TESTS: P1-T6, P2-T8, P6-T5 | EVIDENCE: FEATURE/evidence/regression-testing/pass-after-pin-count.md
AC-MAPPING: AC5 | IMPLEMENTATION: P1-T1, P1-T2, P2-T1 to P2-T5 | TESTS: P1-T5, P2-T8 | EVIDENCE: FEATURE/evidence/regression-testing/fail-before-pin-count.md
AC-MAPPING: AC6 | IMPLEMENTATION: P1-T1 | TESTS: P1-T6, P4-T2 | EVIDENCE: FEATURE/evidence/qa-gates/post-format-census.md
AC-MAPPING: AC7 | IMPLEMENTATION: P3-T3, P3-T4 | TESTS: P4-T6, P4-T2 | EVIDENCE: FEATURE/evidence/regression-testing/focus-and-theme-class-pass-after.md
AC-MAPPING: AC8 | IMPLEMENTATION: P3-T3, P3-T4, P3-T8 | TESTS: P5-T1, P5-T2 | EVIDENCE: FEATURE/evidence/qa-gates/call-site-census.md
AC-MAPPING: AC9 | IMPLEMENTATION: P2-T1, P2-T4, P2-T5 | TESTS: P2-T6, P4-T2 | EVIDENCE: FEATURE/evidence/qa-gates/post-format-census.md
AC-MAPPING: AC10 | IMPLEMENTATION: P3-T7, P3-T8 | TESTS: P4-T5, P4-T2 | EVIDENCE: FEATURE/evidence/regression-testing/fixture-class-pass-after.md
AC-MAPPING: AC11 | IMPLEMENTATION: P2-T2, P2-T3, P2-T5 | TESTS: P4-T2 | EVIDENCE: FEATURE/evidence/qa-gates/post-format-census.md
AC-MAPPING: AC12 | IMPLEMENTATION: P3-T5 | TESTS: P4-T2 | EVIDENCE: FEATURE/evidence/qa-gates/post-format-census.md
AC-MAPPING: AC13 | IMPLEMENTATION: P3-T7 | TESTS: P4-T2 | EVIDENCE: FEATURE/evidence/qa-gates/post-format-census.md
AC-MAPPING: AC14 | IMPLEMENTATION: P3-T8 | TESTS: P4-T5, P4-T2 | EVIDENCE: FEATURE/evidence/regression-testing/fixture-class-pass-after.md
AC-MAPPING: AC15 | IMPLEMENTATION: P3-T1, P3-T2, P3-T6 | TESTS: P4-T6, P4-T2 | EVIDENCE: FEATURE/evidence/qa-gates/post-format-census.md
AC-MAPPING: AC16 | IMPLEMENTATION: P3-T3, P3-T4 | TESTS: P4-T2 | EVIDENCE: FEATURE/evidence/qa-gates/post-format-census.md
AC-MAPPING: AC17 | IMPLEMENTATION: P3-T5, P3-T6 | TESTS: P3-T9, P4-T2 | EVIDENCE: FEATURE/evidence/qa-gates/post-format-census.md
AC-MAPPING: AC18 | IMPLEMENTATION: P1-T1 to P3-T8, P4-T1 | TESTS: P6-T8 | EVIDENCE: FEATURE/evidence/qa-gates/file-line-counts.md
AC-MAPPING: AC19 | IMPLEMENTATION: P1-T1 to P3-T8 | TESTS: P5-T3 | EVIDENCE: FEATURE/evidence/qa-gates/prohibited-constructs-grep.md
AC-MAPPING: AC20 | IMPLEMENTATION: P4-T8 | TESTS: P6-T9 | EVIDENCE: FEATURE/evidence/qa-gates/footprint-scope.md
AC-MAPPING: AC21 | IMPLEMENTATION: P1-T2 | TESTS: P1-T4, P6-T5 | EVIDENCE: FEATURE/evidence/qa-gates/coverage-summary.md
AC-MAPPING: AC22 | IMPLEMENTATION: P6-T1 to P6-T5 | TESTS: P6-T5 | EVIDENCE: FEATURE/evidence/qa-gates/toolchain-final.md
AC-MAPPING: AC23 | IMPLEMENTATION: P0-T15, P6-T5 | TESTS: P6-T6 | EVIDENCE: FEATURE/evidence/qa-gates/coverage-comparison.md
AC-MAPPING: AC24 | IMPLEMENTATION: P1-T1 to P3-T8 | TESTS: P4-T7 | EVIDENCE: FEATURE/evidence/regression-testing/concurrent-set-test-summary.md
UNRESOLVED-GAPS: NONE

DIRECTIVE: PREFLIGHT VALIDATION ONLY
Executor preflight has not yet run; the signal below is the planner's request line, not a self-approval and not a discovered defect.
PREFLIGHT: REVISIONS REQUIRED

