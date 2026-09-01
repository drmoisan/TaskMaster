# Atomic Implementation Plan — Issue #285: `TimeOutTask.RunWithTimeout<T1, TResult>` exception-type mismatch

- **Issue:** #285
- **Work Mode:** `full-bug` — `docs/features/active/2026-07-09-timeouttask-runwithtimeout-exception-type-mismatch-285/spec.md` is the sole acceptance-criteria source. No `user-story.md` exists and none is created.
- **Branch:** `bug/timeouttask-runwithtimeout-exception-type-mismatch-285`
- **Feature folder:** `docs/features/active/2026-07-09-timeouttask-runwithtimeout-exception-type-mismatch-285/`
- **Plan path (canonical, revised in place):** `docs/features/active/2026-07-09-timeouttask-runwithtimeout-exception-type-mismatch-285/plan.2026-09-01T00-30.md`. This timestamped name is the repository convention for active feature plans and is retained deliberately; no `plan.md` is created, and no sibling plan file is created across revision rounds.

## Scope Boundary (load-bearing)

This item runs concurrently with other items against the same `main`. The change may create or modify only:

1. `UtilitiesCS/Threading/TimeOutTask.cs`
2. `UtilitiesCS.Test/Threading/TimeOutTask_OverloadCoverageTests.cs`
3. paths under `docs/features/active/2026-07-09-timeouttask-runwithtimeout-exception-type-mismatch-285/`

No `.csproj` edit is required: `UtilitiesCS.Test/UtilitiesCS.Test.csproj` line 485 already carries `<Compile Include="Threading\TimeOutTask_OverloadCoverageTests.cs" />`, and no new source file is created.

Out-of-scope defects that live in the same production file are recorded in the spec's Non-Goals section and are each a separate follow-up issue. Following the spec's convention, their paths are written here without backticks so they are not harvested into this change's footprint: the two dead TimeoutException clauses at lines 818 and 914 of UtilitiesCS/Threading/TimeOutTask.cs; the four inert-timeout implementations at lines 405, 475, 638 and 720 of the same file; the inverted handler pair at lines 268 and 272 of the same file; and the 527-line UtilitiesCS.Test/Threading/TimeOutTask_AdditionalTests.cs file-size breach. None of them is touched by this plan.

## Acceptance-Criteria Identifier Map

`spec.md` carries exactly 12 unnumbered checklist bullets under its `## Acceptance Criteria` heading. This plan addresses them by position, first bullet through twelfth:

| ID | Spec bullet (leading text) |
| --- | --- |
| AC1 | "A new MSTest method named `RunWithTimeout_FuncT1TResult_ShouldRetryAfterTaskCanceledException` exists ..." |
| AC2 | "After the production fix, `RunWithTimeout_FuncT1TResult_ShouldRetryAfterTaskCanceledException` passes ..." |
| AC3 | "A text search of `UtilitiesCS.Test/Threading/TimeOutTask_OverloadCoverageTests.cs` returns zero matches ..." |
| AC4 | "`RunWithTimeout_FuncT1TResult_ShouldRetryAfterTimeoutException` ... and `RunWithTimeout_FuncT1TResult_ShouldReturnDefault_WhenTimeoutOccursWithoutRetries` ... both pass ..." |
| AC5 | "In `UtilitiesCS/Threading/TimeOutTask.cs` after the fix, a line-anchored search for `catch` clauses returns ..." |
| AC6 | "A text search of `UtilitiesCS/Threading/TimeOutTask.cs` returns zero matches for `OperationCanceledException` ..." |
| AC7 | "Both the public wrapper and the private implementation ... declare a trailing `Func<int, CancellationTokenSource>? timeoutSourceFactory = null` parameter ..." |
| AC8 | "`dotnet tool run csharpier check .` reports no unformatted files." |
| AC9 | "`msbuild TaskMaster.sln /t:Rebuild ... /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` completes with 0 errors and 0 new analyzer warnings." |
| AC10 | "`msbuild TaskMaster.sln /t:Rebuild ... /p:TreatWarningsAsErrors=true` completes with 0 errors, with no `/p:Nullable=enable` added ..." |
| AC11 | "`vstest.console.exe` with `/EnableCodeCoverage` runs the full `UtilitiesCS.Test` assembly and the `QuickFiler.Test` assembly with 0 failures ..." |
| AC12 | "`git status --porcelain` and the branch diff against the merge base list only ..." |

## Correction to the Spec's Recommended Edit (material, re-derived this pass)

`spec.md` line 148 and the research record recommend the clause `catch (Exception ex) when (ex is TaskCanceledException || ex is TimeoutException)`. **That form does not compile in this file.** `UtilitiesCS/Threading/TimeOutTask.cs` line 9 is `using Microsoft.Office.Interop.Outlook;`, and that namespace declares a type named `Exception`. A bare `Exception` in such a file raises `CS0104: 'Exception' is an ambiguous reference between 'Microsoft.Office.Interop.Outlook.Exception' and 'System.Exception'`. This is why every one of the ten general handlers in the file is written `catch (System.Exception e)` and never `catch (Exception e)`. The same ambiguity is documented in-repo at `TaskMaster/AppGlobals/AppOlObjects.cs` lines 19-22 and at `UtilitiesCS.Test/OutlookObjects/Table/OlToDoTable_Tests.cs` line 7, both of which introduce `using Exception = System.Exception;` for exactly this reason.

This plan therefore uses the type-qualified equivalent, which is semantically identical:

`catch (System.Exception e) when (e is TaskCanceledException || e is TimeoutException)`

The variable is named `e` rather than `ex` for a measured reason. With the file's 12-space clause indent, the `e` spelling occupies 97 columns and the `ex` spelling occupies exactly 100 columns, which is CSharpier's default `printWidth`. At 97 columns the clause is guaranteed to remain on a single physical line after `dotnet tool run csharpier format .`, which keeps the single-line census assertions in Phase 3 satisfiable. The precedent for CSharpier moving a `when` clause onto its own line when the combined line does not fit is visible at `UtilitiesCS/OutlookObjects/Folder/BreadcrumbSelectorMessages.cs` line 284; the same file keeps the clause inline at line 151 where it fits. Declaring `e` in this clause does not collide with the `e` declared by the sibling `catch (System.Exception e)` clause: the two are sibling catch clauses with disjoint scopes.

Two secondary literals this plan asserts on are quoted here verbatim so they are recognised as text the plan instructs the executor to create:

- `when (e is TaskCanceledException || e is TimeoutException)`
- `timeoutSourceFactory ?? (ms => new CancellationTokenSource(ms))`

## Verified Tree Facts Used by This Plan

All figures below were re-derived directly against the worktree in this authoring pass.

- `UtilitiesCS/Threading/TimeOutTask.cs` carries `#nullable enable` at line 1 and `using Microsoft.Office.Interop.Outlook;` at line 9.
- Catch-clause census, 23 clauses: `catch (TaskCanceledException)` at 65, 130, 268, 351, 429, 498, 581, 663, 744 (9); `catch (TimeoutException)` at 200, 272, 818, 914 (4); `catch (System.Exception e)` at 85, 149, 220, 290, 372, 450, 519, 603, 685, 766 (10). No exception filter clause exists in the file today.
- `OperationCanceledException` occurs 0 times in the file.
- The defective member: public wrapper at 165-175 (forwarding call at 174), private implementation at 177-230, timeout construction at 189, awaited `Task.Run(() => function(arg1), combinedToken.Token)` at 198, defective `catch (TimeoutException)` at 200, clause guard at 202, retry recursion at 206-213, general handler at 220-227, `return result!;` at 229.
- Seam precedent on the `Func<TResult>` sibling: parameter declarations at 27 and 47, construction at 52-54, recursion forwarding at 77, wrapper forwarding at 36.
- `new CancellationTokenSource(` occurs 10 times: once inside the seam coalesce at line 53, and 9 times as the statement `using var timeoutSource = new CancellationTokenSource(milliseconds);` at lines 119, 189, 256, 340, 418, 488, 570, 652, 734. Only the occurrence at 189 is replaced by this change; the statement count therefore falls from 9 to 8 and must not fall further.
- The bare identifier `timeoutSourceFactory` occurs 5 times today, at lines 27, 36, 47, 53, 77.
- `UtilitiesCS.Test/Threading/TimeOutTask_OverloadCoverageTests.cs` is 387 lines, declares `using System; using System.Threading; using System.Threading.Tasks; using FluentAssertions; using Microsoft.VisualStudio.TestTools.UnitTesting;` at lines 1-5, opens `public partial class TimeOutTask_Tests` at line 9, and closes the class at line 386 and the namespace at line 387. It contains 0 occurrences of `Task.Delay`, `Thread.Sleep`, and `Thread.SpinWait`.
- The at-risk tests: `RunWithTimeout_FuncT1TResult_ShouldReturnDefault_WhenTimeoutOccursWithoutRetries` at line 106 of that same file, and `RunWithTimeout_FuncT1TResult_ShouldRetryAfterTimeoutException` at line 190 of `UtilitiesCS.Test/Threading/TimeOutTask_AdditionalTests.cs`. Neither may be edited.
- `[TestClass]` and `[DoNotParallelize]` sit on the root partial at `UtilitiesCS.Test/Threading/TimeOutTask_Tests.cs` lines 9 and 10, so the new test inherits both. The fully qualified name of the new test will be `UtilitiesCS.Test.TimeOutTask_Tests.RunWithTimeout_FuncT1TResult_ShouldRetryAfterTaskCanceledException`.
- `UtilitiesCS.Test/UtilitiesCS.Test.csproj` sets `<LangVersion>Latest</LangVersion>` at line 18 and `<TargetFrameworkVersion>v4.8.1</TargetFrameworkVersion>` at line 17; `using var` declarations are already used across 58 files of that project.
- Test output assemblies for `Debug` / `Any CPU`: `UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll` and `QuickFiler.Test\bin\Debug\QuickFiler.Test.dll`.
- The QuickFiler dependency the spec cites is at `QuickFiler.Test/Controllers/QfcItemControllerTests.cs` line 62 (the spec and the research record both write this path as `QuickFiler.Test/QfcItemControllerTests.cs`, which does not resolve; the directory segment `Controllers/` is missing from both).
- `.gitignore` line 39 ignores `[Tt]est[Rr]esult*/` and lines 144-145 ignore `coverage/*` except `coverage/.gitkeep`, so TRX logs and raw Cobertura output written to those directories do not enter the footprint.
- `.github/workflows/_mstest-coverage.yml` line 83 is the canonical test invocation, `/EnableCodeCoverage /InIsolation /Logger:trx /TestCaseFilter:"TestCategory!=LiveOutlook"`, and line 45 of the same file restores packages with `nuget restore`.
- `global.json` pins SDK `8.0.205` with `rollForward: latestFeature` and search paths `.dotnet-sdk` then `$host$`; `dotnet-tools.json` at the repository root pins CSharpier `1.2.6` and is the only tool manifest.

## Pre-existing Policy Deviation (recorded, not fixed here)

`UtilitiesCS/Threading/TimeOutTask.cs` is 993 lines and already exceeds the 500-line ceiling in `.claude/rules/general-code-change.md`. This change grows it by roughly 12 lines. The breach cannot be corrected inside this item's scope boundary, because splitting the file would create a path outside the three permitted paths. The deviation is recorded as evidence in Phase 3 and is a candidate follow-up issue alongside the spec's existing Non-Goals list. `UtilitiesCS.Test/Threading/TimeOutTask_OverloadCoverageTests.cs` is 387 lines and stays under the ceiling after the change.

## Sequencing Rationale

The Bugfix Workflow in `CLAUDE.md` requires a failing regression test before the fix. The regression test cannot compile until the `timeoutSourceFactory` parameter exists, because it binds that parameter by name. Phase 1 therefore lands the determinism seam and the test together, leaving the defective `catch (TimeoutException)` clause at line 200 untouched. The seam is behaviour-preserving by construction — `timeoutSourceFactory ?? (ms => new CancellationTokenSource(ms))` with the parameter defaulted to `null` produces the identical `CancellationTokenSource` the current line 189 produces — so the Phase 1 red run observes the real defect: the injected pre-cancelled source makes `Task.Run` produce a `TaskCanceledException`, that type misses `catch (TimeoutException)`, it reaches the general handler at line 220, and `strict: true` rethrows it out of the `await`. Phase 2 then makes the single one-line handler change that turns the run green.

---

### Phase 0 — Baseline Capture and Toolchain Bootstrap

- [x] [P0-T1] Read, in this exact order, `CLAUDE.md`, `.claude/rules/general-code-change.md`, `.claude/rules/general-unit-test.md`, and `.claude/rules/csharp.md`, then read `docs/features/active/2026-07-09-timeouttask-runwithtimeout-exception-type-mismatch-285/spec.md` in full. Write `docs/features/active/2026-07-09-timeouttask-runwithtimeout-exception-type-mismatch-285/evidence/baseline/phase0-instructions-read.md` containing `Timestamp:`, `Policy Order:` naming the four policy files in the order read, and an explicit list of every file read. Acceptance: the artifact exists and lists all five paths.

- [x] [P0-T2] Record the branch and merge base. Run `git rev-parse --abbrev-ref HEAD`, `git rev-parse HEAD`, and `git merge-base origin/main HEAD`, and write all three values plus `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` to `docs/features/active/2026-07-09-timeouttask-runwithtimeout-exception-type-mismatch-285/evidence/baseline/p0-t2-branch-and-merge-base.md`. Acceptance: `git rev-parse --abbrev-ref HEAD` prints exactly `bug/timeouttask-runwithtimeout-exception-type-mismatch-285`, the merge-base command exits 0, and the recorded merge-base commit id is 40 hexadecimal characters. This merge-base value is referenced as MERGE_BASE by every later diff task, which reads it from this artifact rather than recomputing a possibly different value.

- [x] [P0-T3] Probe the .NET SDK. Run `dotnet --version`. If it exits non-zero or reports the `global.json` error message naming `.dotnet-sdk`, run `pwsh -File scripts/vscode/Install-RepoDotNetSdk.ps1` from the repository root and re-run `dotnet --version`. Write `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` for every invocation performed to `.../evidence/baseline/p0-t3-sdk-probe.md`. Acceptance: the artifact records a final `dotnet --version` invocation with `EXIT_CODE: 0` whose stdout is a version string of the form `8.` or higher, and records whether the bootstrap script was needed. The artifact also states whether the bootstrap actually ran and records that the SDK install directory the script creates is matched by the directory-only glob at `.gitignore` line 350, so it never appears in `git status --porcelain` and is outside the footprint sets asserted by P3-T1, P3-T11 and P4-T14. No exclusion-set entry follows from this observation; it is recorded so a reader of the footprint artifacts does not have to re-derive it.

- [x] [P0-T4] Restore NuGet packages. Run `nuget restore TaskMaster.sln` from the repository root. Write `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` to `.../evidence/baseline/p0-t4-nuget-restore.md`. Acceptance: `EXIT_CODE: 0` and the `Output Summary:` records either the count of packages installed or the literal indicating all packages were already present.

- [x] [P0-T5] Restore the CSharpier tool manifest. Run `dotnet tool restore` from the repository root. Write `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` to `.../evidence/baseline/p0-t5-dotnet-tool-restore.md`. Acceptance: `EXIT_CODE: 0` and the `Output Summary:` names `csharpier` and the version `1.2.6`.

- [x] [P0-T6] Capture the formatting baseline read-only. Run `dotnet tool run csharpier check .` from the repository root. This is a check-mode, non-writing invocation, so its exit code alone is a discriminating observation; nonetheless record both the exit code and the full list of any file paths CSharpier reports as needing formatting. Write `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` to `.../evidence/baseline/p0-t6-csharpier-check.md`, including the `Formatted` summary line CSharpier prints and the count of files reported as unformatted. Acceptance: the artifact records the exit code and an explicit unformatted-file list, which is the empty list when the tree is already clean. If the list is non-empty, the artifact must enumerate every path in it, because Phase 3's repository-wide format pass will rewrite exactly those files and the Phase 3 footprint task consumes this list.

- [x] [P0-T7] Capture the analyzer-build baseline. Resolve MSBuild with `& "${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\vswhere.exe" -latest -products * -requires Microsoft.Component.MSBuild -find "MSBuild\**\Bin\MSBuild.exe" | Select-Object -First 1` and invoke `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` through the resolved absolute path. Use `/t:Rebuild`, never `/t:Build`. Write `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` to `.../evidence/baseline/p0-t7-msbuild-analyzers.md`, recording the trailing `Warning(s)` and `Error(s)` counts MSBuild prints. Acceptance: `EXIT_CODE: 0`, and the `Output Summary:` records the exact baseline warning count as an integer, which Phase 3 compares against.

- [x] [P0-T8] Capture the nullable/type-check baseline. Invoke `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true` through the same resolved MSBuild path. Do not add `/p:Nullable=enable`. Write `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` to `.../evidence/baseline/p0-t8-msbuild-nullable.md`, quoting the full command line verbatim. Acceptance: `EXIT_CODE: 0`, the recorded command string contains `TreatWarningsAsErrors=true`, and the recorded command string contains no occurrence of `Nullable=enable`.

- [x] [P0-T9] Probe the coverage collector. Run `dotnet-coverage --version`. If it exits non-zero, run `dotnet tool install --global dotnet-coverage` and re-run `dotnet-coverage --version`. Write `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` for every invocation performed to `.../evidence/baseline/p0-t9-dotnet-coverage-probe.md`. Acceptance: the artifact records a final `dotnet-coverage --version` invocation with `EXIT_CODE: 0` and a printed version string, and records whether the install step was needed.

- [x] [P0-T10] Capture the `UtilitiesCS.Test` baseline test run with Cobertura coverage. Resolve vstest with `& "${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\vswhere.exe" -latest -products * -find "Common7\IDE\Extensions\TestPlatform\vstest.console.exe" | Select-Object -First 1`, then run the single assembly under the collector:

  ```text
  dotnet-coverage collect --output coverage\p0-t10.cobertura.xml --output-format cobertura --settings coverage.config -- <resolved-vstest> UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation /Logger:trx /ResultsDirectory:TestResults\p0-t10 /TestCaseFilter:TestCategory!=LiveOutlook
  ```

  Run this assembly on its own rather than as part of an aggregate assembly list. Write `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` to `.../evidence/baseline/p0-t10-vstest-utilitiescs.md`, recording the `Passed`, `Failed`, and `Skipped` counts vstest prints and the repository-wide `line-rate` attribute from the root `<coverage>` element of `coverage\p0-t10.cobertura.xml` expressed as a percentage. Acceptance: the artifact records all three integer test counts and one numeric coverage percentage, and `coverage\p0-t10.cobertura.xml` exists. A non-zero exit code is recorded rather than repaired; this is a baseline observation and the failing test identities, if any, are enumerated in the artifact as the BASELINE_FAILURE_SET that Phase 3 subtracts.

- [x] [P0-T11] Capture the `QuickFiler.Test` baseline test run. Run the resolved vstest against `QuickFiler.Test\bin\Debug\QuickFiler.Test.dll` alone with `/EnableCodeCoverage /InIsolation /Logger:trx /ResultsDirectory:TestResults\p0-t11 /TestCaseFilter:TestCategory!=LiveOutlook`. If the test host reports a crash or the run hangs, re-run the identical command exactly once and record both invocations. Write `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` to `.../evidence/baseline/p0-t11-vstest-quickfiler.md` with the `Passed`, `Failed`, and `Skipped` counts. Acceptance: the artifact records all three integer test counts, and enumerates the failing test identities as the QuickFiler BASELINE_FAILURE_SET if the count is non-zero.

- [x] [P0-T12] Capture the pre-change source census. Against `UtilitiesCS/Threading/TimeOutTask.cs`, record: the total line count; the count of lines matching the anchored pattern `^\s*catch \(TaskCanceledException\)\s*$`; the count matching `^\s*catch \(TimeoutException\)\s*$`; the count matching `^\s*catch \(System\.Exception e\)\s*$`; the count of `OperationCanceledException` occurrences; the count of lines matching `^\s*using var timeoutSource = new CancellationTokenSource\(milliseconds\);\s*$`; and the simple-match counts of `Func<int, CancellationTokenSource>? timeoutSourceFactory = null`, `timeoutSourceFactory ?? (ms => new CancellationTokenSource(ms))`, and the bare token `timeoutSourceFactory`. Also record, against `UtilitiesCS.Test/Threading/TimeOutTask_OverloadCoverageTests.cs`, its line count using `(Get-Content -LiteralPath <path>).Count` and a file-level simple-match count of `CancellationToken.None`. Write all figures to `.../evidence/baseline/p0-t12-source-census.md` with `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:`. Acceptance: the recorded values are 993, 9, 4, 10, 0, 9, 2, 1, 5, 387, and 16 respectively. Any disagreement is recorded in the artifact and reported before Phase 1 begins, because every later census gate is expressed as a delta from these numbers.

- [x] [P0-T13] Capture the pre-change changed-line coverage figures. From `coverage\p0-t10.cobertura.xml`, record the hit count for line number 189 (the timeout-source construction being replaced) and for line number 202 (`token.ThrowIfCancellationRequested();`, the first statement inside the `catch (TimeoutException)` clause at line 200), using this lookup rule for each target line. `RunWithTimeout<T1, TResult>` is `async`, so the compiler emits a nested state-machine type and the report can carry more than one `<class>` element with the same `filename` and overlapping `<line number=...>` entries whose `hits` differ. Read every `<line>` element whose `number` attribute equals the target line, across all `<class>` elements whose `filename` attribute ends with `TimeOutTask.cs`. Record the count of matching elements and the `hits` value of each. The recorded hit count for that line is the maximum over those elements. If no element matches, record `NOT PRESENT IN REPORT`. Write both recorded hit counts, the per-line element counts and per-element `hits` values they were derived from, and a statement of whether each line was present in the report at all, to `.../evidence/baseline/p0-t13-changed-line-coverage.md` with `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:`. Acceptance: the artifact records two explicit recorded hit counts, each being either a non-negative integer or the literal `NOT PRESENT IN REPORT`; for each of the two lines it records the number of matching `<line>` elements and the `hits` value of every one of them; and it enumerates every distinct Cobertura `filename` value that was matched.

---

### Phase 1 — Determinism Seam and Failing Regression Test

- [x] [P1-T1] In `UtilitiesCS/Threading/TimeOutTask.cs`, add the determinism seam to the private implementation declared at line 177. Append the trailing parameter `Func<int, CancellationTokenSource>? timeoutSourceFactory = null` after the `int attempt` parameter; replace the statement at line 189 with the three-line construction `using var timeoutSource = (` / `timeoutSourceFactory ?? (ms => new CancellationTokenSource(ms))` / `)(milliseconds);`, mirroring lines 52-54; and add `timeoutSourceFactory` as the trailing argument of the recursive `function.RunWithTimeout(...)` call inside the clause body at lines 206-213, mirroring line 77. The `?` annotation is mandatory: the file carries `#nullable enable`, so the non-annotated form yields CS8625, which `/p:TreatWarningsAsErrors=true` promotes to a build error. Do not touch the `catch (TimeoutException)` clause in this task. Acceptance: a simple-match count of `Func<int, CancellationTokenSource>? timeoutSourceFactory = null` in the file returns 3; a simple-match count of `timeoutSourceFactory ?? (ms => new CancellationTokenSource(ms))` returns 2; an anchored count of `^\s*using var timeoutSource = new CancellationTokenSource\(milliseconds\);\s*$` returns 8; and an anchored count of `^\s*catch \(TimeoutException\)\s*$` still returns 4.

- [x] [P1-T2] In the same file, add the trailing parameter `Func<int, CancellationTokenSource>? timeoutSourceFactory = null` to the public wrapper declared at line 165, and forward it as the trailing argument of the `function.RunWithTimeout(arg1, token, milliseconds, maxAttempts, strict, 0)` call at line 174, mirroring the wrapper at lines 21-38. Acceptance: a simple-match count of `Func<int, CancellationTokenSource>? timeoutSourceFactory = null` in the file returns 4, and a simple-match count of the bare token `timeoutSourceFactory` in the file returns 10.

- [x] [P1-T3] Append the regression test `RunWithTimeout_FuncT1TResult_ShouldRetryAfterTaskCanceledException` to `UtilitiesCS.Test/Threading/TimeOutTask_OverloadCoverageTests.cs`, inserting it inside the class body immediately before the class-closing brace at line 386. Add no using directives; lines 1-5 already supply `System`, `System.Threading`, `System.Threading.Tasks`, `FluentAssertions`, and `Microsoft.VisualStudio.TestTools.UnitTesting`. The method body is:

  ```csharp
  [TestMethod]
  public async Task RunWithTimeout_FuncT1TResult_ShouldRetryAfterTaskCanceledException()
  {
      // Arrange
      // Attempt 0 receives an already-cancelled timeout source, so the linked combined
      // token is cancelled before Task.Run is queued and the delegate is never dequeued.
      // The awaited task is Canceled by construction, so the await throws
      // TaskCanceledException with no wall-clock wait and no scheduling race. Attempt 1
      // receives a never-cancelled source so the retry can complete and be observed.
      using var canceledSource = new CancellationTokenSource();
      canceledSource.Cancel();
      using var liveSource = new CancellationTokenSource();

      int factoryCalls = 0;
      Func<int, CancellationTokenSource> timeoutSourceFactory = _ =>
          Interlocked.Increment(ref factoryCalls) == 1 ? canceledSource : liveSource;

      int delegateCalls = 0;
      Func<int, string> function = arg =>
      {
          Interlocked.Increment(ref delegateCalls);
          return $"result-{arg}";
      };

      // Act
      var result = await function.RunWithTimeout(
          42,
          CancellationToken.None,
          milliseconds: 30_000,
          maxAttempts: 1,
          strict: true,
          timeoutSourceFactory: timeoutSourceFactory
      );

      // Assert
      result.Should().Be("result-42");
      delegateCalls.Should().Be(1);
      factoryCalls.Should().Be(2);
  }
  ```

  Acceptance: a simple-match count of `RunWithTimeout_FuncT1TResult_ShouldRetryAfterTaskCanceledException` in that file returns 1; a simple-match count of `milliseconds: 30_000` in that file returns 1; a file-level simple-match count of `CancellationToken.None` in that file returns 17, one greater than the baseline of 16 recorded by P0-T12, which is the arithmetic consequence of the appended method carrying exactly one such caller token and no existing method being edited; `(Get-Content -LiteralPath UtilitiesCS.Test/Threading/TimeOutTask_OverloadCoverageTests.cs).Count` is at least 388 and at most 500; and combined anchored counts of `Task\.Delay`, `Thread\.Sleep`, and `Thread\.SpinWait` in that file return 0.

- [x] [P1-T4] Format the two changed files only. Run `dotnet tool run csharpier format UtilitiesCS/Threading/TimeOutTask.cs UtilitiesCS.Test/Threading/TimeOutTask_OverloadCoverageTests.cs`. Because a formatter exits 0 whether or not it rewrote anything, record two observations beyond the exit code: the `Formatted` summary line CSharpier prints, which reports the number of files processed rather than the number rewritten, and the output of `git status --porcelain -- UtilitiesCS/Threading/TimeOutTask.cs UtilitiesCS.Test/Threading/TimeOutTask_OverloadCoverageTests.cs` taken immediately afterwards. Write both to `.../evidence/regression-testing/p1-t4-scoped-format.md` with `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:`. Acceptance: `EXIT_CODE: 0`, the recorded porcelain output lists both files as modified, and the artifact quotes the `Formatted` line verbatim.

- [x] [P1-T5] Build the solution so the red run has current assemblies. Invoke `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU"` through the vswhere-resolved MSBuild path. Write `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` to `.../evidence/regression-testing/p1-t5-build.md` with the trailing `Error(s)` count. Acceptance: `EXIT_CODE: 0`, `0 Error(s)`, and both `UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll` and `QuickFiler.Test\bin\Debug\QuickFiler.Test.dll` exist with a write time later than the start of this task. A CS0104, CS8625, or CS1739 diagnostic here means the Phase 1 edits deviated from the plan text and must be corrected before proceeding.

- [x] [P1-T6] [expect-fail] Run the new regression test alone against the unfixed handler and capture its failure. Command:

  ```text
  <resolved-vstest> UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /InIsolation /Logger:trx /ResultsDirectory:TestResults\p1-t6 /TestCaseFilter:FullyQualifiedName=UtilitiesCS.Test.TimeOutTask_Tests.RunWithTimeout_FuncT1TResult_ShouldRetryAfterTaskCanceledException
  ```

  Write `Timestamp:`, `Command:`, `EXIT_CODE:`, `ExpectedExitCode: 1`, `Output Summary:` to `.../evidence/regression-testing/p1-t6-red-new-test.md`, and paste the failure message and stack frame verbatim. Acceptance: the run reports `Total tests: 1` with `Failed: 1` and `Passed: 0`; the recorded failure text contains the literal `System.Threading.Tasks.TaskCanceledException`; and the artifact records `EXIT_CODE: 1` against `ExpectedExitCode: 1`. A failure whose text does not name that exception type, or a passing run, invalidates the red gate and must be reported rather than worked around. This is the fail-before evidence required by the Bugfix Workflow; it is a real failing run, so no fail-before exception dossier is needed.

---

### Phase 2 — Minimal Production Fix and Green Verification

- [x] [P2-T1] In `UtilitiesCS/Threading/TimeOutTask.cs`, replace the single line `catch (TimeoutException)` that guards the retry ladder of the private `Func<T1, TResult>` implementation — the one whose immediately preceding line is the awaited `Task.Run(() => function(arg1), combinedToken.Token);` statement — with the following comment and clause, preserving the file's 12-space indentation and changing nothing inside the clause body:

  ```csharp
  // A timer-driven cancellation of Task.Run surfaces as TaskCanceledException, not
  // TimeoutException (issue #285). TimeoutException is retained because a wrapped
  // delegate may raise it directly, and existing callers and tests depend on that retry.
  // System.Exception is written out because Microsoft.Office.Interop.Outlook, imported at
  // line 9, also declares a type named Exception and a bare Exception is CS0104-ambiguous.
  catch (System.Exception e) when (e is TaskCanceledException || e is TimeoutException)
  ```

  Change no other clause anywhere in the file. Acceptance: a simple-match count of `when (e is TaskCanceledException || e is TimeoutException)` in the file returns 1; an anchored count of `^\s*catch \(TimeoutException\)\s*$` returns 3; an anchored count of `^\s*catch \(TaskCanceledException\)\s*$` returns 9; an anchored count of `^\s*catch \(System\.Exception e\)\s*$` returns 10; and a simple-match count of `OperationCanceledException` returns 0.

- [x] [P2-T2] Format the two changed files only. Run `dotnet tool run csharpier format UtilitiesCS/Threading/TimeOutTask.cs UtilitiesCS.Test/Threading/TimeOutTask_OverloadCoverageTests.cs`, then re-measure. Because the formatter's exit code is identical on a clean run and on a rewriting run, record the `Formatted` summary line it prints and the character length of the clause line. Write to `.../evidence/regression-testing/p2-t2-scoped-format.md` with `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:`. Acceptance: `EXIT_CODE: 0`; the simple-match count of `catch (System.Exception e) when (e is TaskCanceledException || e is TimeoutException)` in the file is 1 after formatting, proving CSharpier left the clause on one physical line; and the recorded character length of that line including its leading indentation is 97.

- [x] [P2-T3] Rebuild the solution. Invoke `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU"` through the vswhere-resolved MSBuild path. Write `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` to `.../evidence/regression-testing/p2-t3-build.md`. Acceptance: `EXIT_CODE: 0` and `0 Error(s)`.

- [x] [P2-T4] Run the new regression test alone and confirm it now passes. Use the same vstest command as P1-T6 with `/ResultsDirectory:TestResults\p2-t4`. Write `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` to `.../evidence/regression-testing/p2-t4-green-new-test.md`. Acceptance: `EXIT_CODE: 0`, and the run reports `Total tests: 1`, `Passed: 1`, `Failed: 0`.

- [x] [P2-T5] Run the two at-risk tests and confirm neither regressed. Command:

  ```text
  <resolved-vstest> UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /InIsolation /Logger:trx /ResultsDirectory:TestResults\p2-t5 /TestCaseFilter:"FullyQualifiedName=UtilitiesCS.Test.TimeOutTask_Tests.RunWithTimeout_FuncT1TResult_ShouldRetryAfterTimeoutException|FullyQualifiedName=UtilitiesCS.Test.TimeOutTask_Tests.RunWithTimeout_FuncT1TResult_ShouldReturnDefault_WhenTimeoutOccursWithoutRetries"
  ```

  Write `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` to `.../evidence/regression-testing/p2-t5-at-risk-tests.md`, naming both tests and their individual outcomes. Acceptance: `EXIT_CODE: 0`, and the run reports `Total tests: 2`, `Passed: 2`, `Failed: 0`.

- [x] [P2-T6] Prove the change to the test tree is additive only and that the second at-risk file was not touched. Read MERGE_BASE from the P0-T2 artifact, then run `git diff MERGE_BASE -- UtilitiesCS.Test/Threading/TimeOutTask_AdditionalTests.cs` and `git diff MERGE_BASE -- UtilitiesCS.Test/Threading/TimeOutTask_OverloadCoverageTests.cs`, substituting the recorded commit id for MERGE_BASE, and pair them with `git status --porcelain -- UtilitiesCS.Test` so untracked additions in that tree are also visible. Write all three outputs to `.../evidence/regression-testing/p2-t6-additive-only-diff.md` with `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:`. Acceptance: the first diff produces no output at all, proving `TimeOutTask_AdditionalTests.cs` is byte-identical to the merge base and that the at-risk test at its line 190 is unedited; the second diff contains zero lines beginning with a single `-` character once the `--- a/` file header line is excluded, proving the change to `TimeOutTask_OverloadCoverageTests.cs` is a pure insertion and that the at-risk test at its line 106 is unedited; and the porcelain output lists `UtilitiesCS.Test/Threading/TimeOutTask_OverloadCoverageTests.cs` as the only entry under `UtilitiesCS.Test`.

---

### Phase 3 — Final QC Toolchain Loop, Coverage, and Footprint

The four toolchain stages P3-T1 through P3-T6 run in the order given. If any stage fails, or if the formatter rewrites any file, restart the loop from P3-T1 and record the restart in the affected artifacts. Every command task in this phase is unconditional; `SKIPPED` is not a valid outcome for any of them.

- [x] [P3-T1] Format the repository. Run `dotnet tool run csharpier format .` from the repository root, then run `git status --porcelain` immediately afterwards. The formatter exits 0 whether or not it rewrote files and its `Formatted` line counts files processed rather than files changed, so the porcelain output taken after the run is the discriminating observation. Write the `Formatted` summary line verbatim, the full porcelain output, and `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` to `.../evidence/qa-gates/p3-t1-format.md`. Acceptance: `EXIT_CODE: 0`, and the porcelain output lists no path outside the three in-scope paths, `.claude/agent-memory/`, and the unformatted-file list recorded by P0-T6. Any path outside that set is recorded in the artifact as `REMEDIATION-REQUIRED` with the path named; it is neither reverted nor committed by this plan. If either `UtilitiesCS.Test/Threading/TimeOutTask_AdditionalTests.cs` or `UtilitiesCS.Test/Threading/TimeOutTask_OverloadCoverageTests.cs` appears in the P0-T6 unformatted-file list, re-run P2-T6 after this task and append the re-run to the P2-T6 artifact before P4-T4 checks off AC4. The artifact states explicitly whether either of those two paths appeared in the P0-T6 list and, if so, that the P2-T6 re-run was performed.

- [x] [P3-T2] Verify formatting. Run `dotnet tool run csharpier check .` from the repository root. Write `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` to `.../evidence/qa-gates/p3-t2-format-check.md`, recording the count of files reported as needing formatting. Acceptance: `EXIT_CODE: 0` and a reported unformatted-file count of 0.

- [x] [P3-T3] Run the analyzer gate. Invoke `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` through the vswhere-resolved MSBuild path. Write `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` to `.../evidence/qa-gates/p3-t3-analyzer-build.md`, recording the trailing `Warning(s)` and `Error(s)` counts and quoting every diagnostic whose text names `UtilitiesCS\Threading\TimeOutTask.cs` or `UtilitiesCS.Test\Threading\TimeOutTask_OverloadCoverageTests.cs`. Acceptance: `EXIT_CODE: 0`, `0 Error(s)`, the recorded warning count is less than or equal to the P0-T7 baseline warning count, and zero quoted diagnostics name either of the two changed files.

- [x] [P3-T4] Run the nullable/type-check gate. Invoke `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true` through the vswhere-resolved MSBuild path, and do not add `/p:Nullable=enable`. Write `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` to `.../evidence/qa-gates/p3-t4-nullable-build.md`, quoting the executed command line verbatim. Acceptance: `EXIT_CODE: 0`, `0 Error(s)`, the quoted command line contains `TreatWarningsAsErrors=true`, and the quoted command line contains no occurrence of `Nullable=enable`.

- [x] [P3-T5] Run the full `UtilitiesCS.Test` assembly with coverage enabled. Command:

  ```text
  <resolved-vstest> UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /Settings:scripts\vscode\TaskMaster.cli.runsettings /EnableCodeCoverage /InIsolation /Logger:trx /ResultsDirectory:TestResults\p3-t5 /TestCaseFilter:TestCategory!=LiveOutlook
  ```

  The `/Settings:scripts\vscode\TaskMaster.cli.runsettings` operand is load-bearing and must not be dropped: P0-T10 ran under the same run settings, and the two runs are compared against each other, so a run without them differs in MSTest parallelization and the comparison is not like-for-like. Run this assembly on its own, not as part of an aggregate assembly list. Write `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` to `.../evidence/qa-gates/p3-t5-vstest-utilitiescs.md` with the `Passed`, `Failed`, and `Skipped` counts, and quote the executed command line verbatim. Acceptance: the quoted command line contains `/Settings:scripts\vscode\TaskMaster.cli.runsettings`; the reported `Failed` count is 0 after subtracting the UtilitiesCS BASELINE_FAILURE_SET recorded by P0-T10, and the artifact states explicitly whether that set was empty; a failure that is not a member of that set is recorded as a regression and reported; and the sum of the reported `Passed` count and the cardinality of the UtilitiesCS BASELINE_FAILURE_SET is at least one greater than the P0-T10 `Passed` count.

- [x] [P3-T6] Run the full `QuickFiler.Test` assembly with coverage enabled. Command:

  ```text
  <resolved-vstest> QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /EnableCodeCoverage /InIsolation /Logger:trx /ResultsDirectory:TestResults\p3-t6 /TestCaseFilter:TestCategory!=LiveOutlook
  ```

  If the test host reports a crash, re-run the identical command exactly once and record both invocations. Write `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` to `.../evidence/qa-gates/p3-t6-vstest-quickfiler.md` with the `Passed`, `Failed`, and `Skipped` counts. Acceptance: the reported `Failed` count is 0 after subtracting the QuickFiler BASELINE_FAILURE_SET recorded by P0-T11, and the artifact states explicitly whether that set was empty. A failure that is not a member of that set is recorded as a regression and reported.

- [x] [P3-T7] Produce the post-change Cobertura report and verify changed-line coverage. Run:

  ```text
  dotnet-coverage collect --output coverage\p3-t7.cobertura.xml --output-format cobertura --settings coverage.config -- <resolved-vstest> UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation /Logger:trx /ResultsDirectory:TestResults\p3-t7 /TestCaseFilter:TestCategory!=LiveOutlook
  ```

  Then derive the two changed-line locations mechanically from the post-change source rather than from any line number recorded in this plan. L_FILTER is the line number of the unique line containing `when (e is TaskCanceledException || e is TimeoutException)`. L_GUARD is the smallest line number greater than L_FILTER whose text matches `^\s*token\.ThrowIfCancellationRequested\(\);\s*$`. L_CTOR is the second line number in the file, in ascending order, whose text matches `^\s*using var timeoutSource = \(\s*$`. From `coverage\p3-t7.cobertura.xml`, derive the recorded hit count for each target line using the same lookup rule P0-T13 uses: `RunWithTimeout<T1, TResult>` is `async`, so the compiler emits a nested state-machine type and the report can carry more than one `<class>` element with the same `filename` and overlapping `<line number=...>` entries whose `hits` differ. Read every `<line>` element whose `number` attribute equals the target line, across all `<class>` elements whose `filename` attribute ends with `TimeOutTask.cs`. Record the count of matching elements and the `hits` value of each. The recorded hit count for that line is the maximum over those elements. If no element matches, record `NOT PRESENT IN REPORT`. Apply that rule to L_GUARD, to every line in the closed range L_CTOR through L_CTOR plus 2, and to L_FILTER. Also record whether a `<line>` element exists at L_FILTER and, if so, its hits. L_GUARD is used as the coverage proxy for the filter clause because a `when` filter expression may emit no `<line>` element of its own; the clause body executing proves the filter matched. Write L_FILTER, L_GUARD, L_CTOR, the recorded hit count at L_GUARD, the recorded hit counts for every line in the closed range L_CTOR through L_CTOR plus 2, the L_FILTER presence statement and its hits when present, the per-line element counts and per-element `hits` values every recorded maximum was derived from, the root `line-rate` as a percentage, and the P0-T13 baseline figures for comparison to `.../evidence/qa-gates/p3-t7-coverage.md` with `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:`. Acceptance: the recorded maximum hit count at L_GUARD is greater than 0, proving the widened clause body executed; at least one line in the closed range L_CTOR through L_CTOR plus 2 has a recorded maximum hit count greater than 0, proving the replaced timeout-source construction executed; the artifact carries the L_FILTER presence statement; and the artifact records both post-change values alongside the two P0-T13 baseline values, showing no changed line moved from covered to uncovered. No repository-wide coverage percentage threshold is asserted by this plan; the percentage is recorded as a reported figure only.

- [x] [P3-T8] Run the post-change source census on `UtilitiesCS/Threading/TimeOutTask.cs`. Record the same nine measurements P0-T12 recorded for this file, plus the simple-match count of `when (e is TaskCanceledException || e is TimeoutException)`. Write them beside the P0-T12 values to `.../evidence/qa-gates/p3-t8-source-census.md` with `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:`. Acceptance: anchored `^\s*catch \(TaskCanceledException\)\s*$` returns 9; anchored `^\s*catch \(TimeoutException\)\s*$` returns 3; anchored `^\s*catch \(System\.Exception e\)\s*$` returns 10; simple-match `when (e is TaskCanceledException || e is TimeoutException)` returns 1; simple-match `OperationCanceledException` returns 0; anchored `^\s*using var timeoutSource = new CancellationTokenSource\(milliseconds\);\s*$` returns 8; simple-match `Func<int, CancellationTokenSource>? timeoutSourceFactory = null` returns 4; simple-match `timeoutSourceFactory ?? (ms => new CancellationTokenSource(ms))` returns 2; and simple-match of the bare token `timeoutSourceFactory` returns 10.

- [x] [P3-T9] Run the test-file hygiene census on `UtilitiesCS.Test/Threading/TimeOutTask_OverloadCoverageTests.cs`. Record the simple-match counts of `Task.Delay`, `Thread.Sleep`, and `Thread.SpinWait`; the simple-match count of `milliseconds: 30_000`; the simple-match count of `RunWithTimeout_FuncT1TResult_ShouldRetryAfterTaskCanceledException`; the simple-match count of `timeoutSourceFactory: timeoutSourceFactory`; and the file-level simple-match count of `CancellationToken.None`. Write them to `.../evidence/qa-gates/p3-t9-test-hygiene.md` with `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:`, recording the P0-T12 baseline of 16 for `CancellationToken.None` beside the post-change figure. Acceptance: the first three counts are each 0; the `milliseconds: 30_000` count is 1; the test-name count is 1; the named-argument count is 1; and the file-level `CancellationToken.None` count is 17.

- [x] [P3-T10] Run the file-size audit after the final format pass. Record `(Get-Content -LiteralPath UtilitiesCS/Threading/TimeOutTask.cs).Count` and `(Get-Content -LiteralPath UtilitiesCS.Test/Threading/TimeOutTask_OverloadCoverageTests.cs).Count`, and state the pre-existing deviation explicitly. Write to `.../evidence/other/p3-t10-file-size-audit.md` with `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:`. Acceptance: the recorded test-file line count is at most 500; the recorded production-file line count is recorded together with the sentence stating that the file already exceeded the 500-line ceiling at the merge base with 993 lines, that the growth attributable to this change is the difference between the two figures, and that the breach is a pre-existing condition that cannot be corrected inside this item's scope boundary.

- [x] [P3-T11] Verify the change footprint. Read MERGE_BASE from the P0-T2 artifact, then run `git diff --name-only MERGE_BASE` with the recorded commit id substituted, and pair it with `git status --porcelain`, because a name-listing diff enumerates tracked changes only and cannot see the untracked evidence files this plan creates. Write both outputs verbatim to `.../evidence/qa-gates/p3-t11-footprint.md` with `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:`. Acceptance: the union of the two outputs, after excluding entries under `.claude/agent-memory/` and entries whose path appears in the P0-T6 unformatted-file list, contains only `UtilitiesCS/Threading/TimeOutTask.cs`, `UtilitiesCS.Test/Threading/TimeOutTask_OverloadCoverageTests.cs`, and paths under `docs/features/active/2026-07-09-timeouttask-runwithtimeout-exception-type-mismatch-285/`. The exclusion set is exactly `.claude/agent-memory/` plus the P0-T6 unformatted-file list, and nothing else. Every excluded entry from either source is enumerated in the artifact by full path under the same auditability requirement, so the exclusion is auditable rather than silent; the artifact states which of the two sources each excluded path came from, and states the cardinality of the P0-T6 list, which is zero when P0-T6 recorded an empty list. Any other path is recorded as `REMEDIATION-REQUIRED` with the path named.

- [x] [P3-T12] Record toolchain loop closure. Write `.../evidence/qa-gates/p3-t12-loop-closure.md` stating, for the final pass, the four stage artifacts P3-T1 through P3-T6 and their recorded exit codes, whether P3-T1 rewrote any file in that pass, and how many times the loop was restarted. Acceptance: the artifact states that the final pass completed formatting, analyzers, type-check, and testing with no failure and no file rewritten by the formatter, and gives an explicit integer restart count.

---

### Phase 4 — Acceptance-Criteria Reconciliation and Commit

Each check-off task marks exactly one `spec.md` acceptance bullet by changing its leading `- [ ]` to `- [x]`, identified by the bullet's leading text as given in the Acceptance-Criteria Identifier Map above. No task marks more than one bullet. A bullet whose evidence does not support it stays unchecked and is recorded as `REMEDIATION-REQUIRED` in the P4-T13 summary.

- [x] [P4-T1] Mark AC1 in `docs/.../spec.md` (the bullet beginning "A new MSTest method named"). Acceptance: the bullet reads `- [x]`, and the P4-T13 summary cites `evidence/regression-testing/p1-t6-red-new-test.md` recording `Failed: 1` with a failure text naming `System.Threading.Tasks.TaskCanceledException`, plus `evidence/qa-gates/p3-t9-test-hygiene.md` recording a test-name count of 1. The summary states that the P1-T6 red run was taken with the determinism seam present and the defective `catch (TimeoutException)` clause untouched, cites the P1-T1 acceptance measurement showing the anchored `catch (TimeoutException)` count was still 4 at that point, and records this as the plan's declared deviation from AC1's literal wording `against unmodified production code`, justified by the seam being behaviour-preserving with `timeoutSourceFactory` defaulted to `null`.

- [x] [P4-T2] Mark AC2 (the bullet beginning "After the production fix"). Acceptance: the bullet reads `- [x]`, and the summary cites `evidence/regression-testing/p2-t4-green-new-test.md` recording `Passed: 1`, `Failed: 0`, together with the three assertions the test carries.

- [x] [P4-T3] Mark AC3 (the bullet beginning "A text search of `UtilitiesCS.Test/Threading/TimeOutTask_OverloadCoverageTests.cs`"). Acceptance: the bullet reads `- [x]`, and the summary cites `evidence/qa-gates/p3-t9-test-hygiene.md` recording zero counts for the three banned APIs, a count of 1 for `milliseconds: 30_000`, and a file-level `CancellationToken.None` count of 17 against the P0-T12 baseline of 16, which is the evidence for AC3's caller-token clause.

- [x] [P4-T4] Mark AC4 (the bullet beginning "`RunWithTimeout_FuncT1TResult_ShouldRetryAfterTimeoutException`"). Acceptance: the bullet reads `- [x]`, and the summary cites `evidence/regression-testing/p2-t5-at-risk-tests.md` recording `Passed: 2` and `evidence/regression-testing/p2-t6-additive-only-diff.md` recording an empty diff for `TimeOutTask_AdditionalTests.cs` and a deletion-free diff for `TimeOutTask_OverloadCoverageTests.cs`.

- [x] [P4-T5] Mark AC5 (the bullet beginning "In `UtilitiesCS/Threading/TimeOutTask.cs` after the fix"). Acceptance: the bullet reads `- [x]`, and the summary cites `evidence/qa-gates/p3-t8-source-census.md` recording 9, 3, 10, and 1 for the four clause categories.

- [x] [P4-T6] Mark AC6 (the bullet beginning "A text search of `UtilitiesCS/Threading/TimeOutTask.cs` returns zero matches"). Acceptance: the bullet reads `- [x]`, and the summary cites `evidence/qa-gates/p3-t8-source-census.md` recording an `OperationCanceledException` count of 0.

- [x] [P4-T7] Mark AC7 (the bullet beginning "Both the public wrapper and the private implementation"). Acceptance: the bullet reads `- [x]`, and the summary cites `evidence/qa-gates/p3-t8-source-census.md` recording a parameter-literal count of 4, a coalesce-literal count of 2, and a bare-token count of 10.

- [x] [P4-T8] Mark AC8 (the bullet beginning "`dotnet tool run csharpier check .`"). Acceptance: the bullet reads `- [x]`, and the summary cites `evidence/qa-gates/p3-t2-format-check.md` recording `EXIT_CODE: 0` and an unformatted-file count of 0.

- [x] [P4-T9] Mark AC9 (the bullet beginning "`msbuild TaskMaster.sln /t:Rebuild` ... `/p:EnableNETAnalyzers=true`"). Acceptance: the bullet reads `- [x]`, and the summary cites `evidence/qa-gates/p3-t3-analyzer-build.md` recording `0 Error(s)` and a warning count no greater than the P0-T7 baseline.

- [x] [P4-T10] Mark AC10 (the bullet beginning "`msbuild TaskMaster.sln /t:Rebuild` ... `/p:TreatWarningsAsErrors=true`"). Acceptance: the bullet reads `- [x]`, and the summary cites `evidence/qa-gates/p3-t4-nullable-build.md` recording `0 Error(s)` and a quoted command line free of `Nullable=enable`.

- [x] [P4-T11] Mark AC11 (the bullet beginning "`vstest.console.exe` with `/EnableCodeCoverage`"). Acceptance: the bullet reads `- [x]`, and the summary cites `evidence/qa-gates/p3-t5-vstest-utilitiescs.md` and `evidence/qa-gates/p3-t6-vstest-quickfiler.md` for the zero-failure results and `evidence/qa-gates/p3-t7-coverage.md` for the two changed-line hit counts. Also record whether a `<line>` element exists at L_FILTER and, if so, its hits. L_GUARD is used as the coverage proxy for the filter clause because a `when` filter expression may emit no `<line>` element of its own; the clause body executing proves the filter matched. AC11's literal wording is `0 failures`. If either the UtilitiesCS or the QuickFiler BASELINE_FAILURE_SET recorded in Phase 0 was non-empty, AC11 is not literally met: leave the bullet unchecked and record it as `REMEDIATION-REQUIRED` in the P4-T13 summary, naming the pre-existing failures.

- [x] [P4-T12] Mark AC12 (the bullet beginning "`git status --porcelain`"). Acceptance: the bullet reads `- [x]`, and the summary cites `evidence/qa-gates/p3-t11-footprint.md`, restates the full two-source exclusion set used there — `.claude/agent-memory/` plus the P0-T6 unformatted-file list — and enumerates every excluded entry by full path with the source it came from. AC12's literal wording is that the two outputs list only the three in-scope paths. If the P0-T6 unformatted-file list was non-empty and any path on it appears in the P3-T11 output, AC12 is not literally met: leave the bullet unchecked and record it as `REMEDIATION-REQUIRED` in the P4-T13 summary, naming each such path and identifying it as a pre-existing formatting drift that P3-T1's repository-wide pass repaired, not a change introduced by this item.

- [x] [P4-T13] Write the acceptance-criteria status summary to `.../evidence/other/acceptance-criteria-status.md`. It must contain `Timestamp:`, one row per identifier AC1 through AC12 giving the identifier, the checked state, and the evidence artifact path cited, and a closing count of checked and unchecked bullets. Acceptance: the artifact contains exactly 12 rows with the identifiers AC1 through AC12 appearing once each, and the closing counts sum to 12 and agree with the actual `- [x]` count under the `## Acceptance Criteria` heading of `spec.md`.

- [x] [P4-T14] Stage and commit the change, then re-verify the footprint against the commit. Stage only the in-scope paths with three explicit `git add` invocations naming `UtilitiesCS/Threading/TimeOutTask.cs`, `UtilitiesCS.Test/Threading/TimeOutTask_OverloadCoverageTests.cs`, and `docs/features/active/2026-07-09-timeouttask-runwithtimeout-exception-type-mismatch-285`. Commit with a message whose subject names issue #285 and whose body states the behavioural consequence at the two production call sites: the previously dead retry ladder becomes live, so `UtilitiesCS/OutlookObjects/Conversation/ConversationHelper.Formatting.cs` line 80 (`timeoutMs: 1000`, `maxAttempts: 3`) can take up to roughly four seconds instead of roughly one on a repeatedly stalled conversation table, and `UtilitiesCS/OneDriveHelpers/OneDriveDownloader.cs` line 139 gains the same retry shape. Then run `git diff --name-only MERGE_BASE..HEAD` with the P0-T2 commit id substituted and `git status --porcelain`, and write both to `.../evidence/qa-gates/p4-t14-commit-footprint.md` with `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:`. Acceptance: the commit succeeds; the `MERGE_BASE..HEAD` name-only diff lists only the three in-scope paths; and the porcelain output, after excluding entries under `.claude/agent-memory/` and entries whose path appears in the P0-T6 unformatted-file list, is empty. The exclusion set is exactly `.claude/agent-memory/` plus the P0-T6 unformatted-file list, and nothing else. Every excluded entry from either source is enumerated in the artifact by full path under the same auditability requirement; the artifact states which of the two sources each excluded path came from, and states the cardinality of the P0-T6 list, which is zero when P0-T6 recorded an empty list. The porcelain assertion is evaluated on the invocation recorded in this artifact. The `p4-t14-commit-footprint.md` artifact itself, and the `[x]` marks written to this plan file for P4-T13 and P4-T14, are a known residual written after that invocation; they are committed by the executing orchestrator after execution and are not a footprint violation. Do not push, do not create or edit a pull request, and do not invoke `gh`.

---

## Out of Scope for This Plan

- Pushing, opening, or editing a pull request, and any `gh` invocation. Delivery beyond the Phase 4 commit belongs to the executing orchestrator.
- Any edit to `UtilitiesCS.Test/Threading/TimeOutTask_AdditionalTests.cs`, including the pre-existing 527-line file-size breach.
- Any edit to a sibling `RunWithTimeout` overload or to either `TimeoutAfter` retry wrapper.
- Any `.csproj`, `.props`, or `.targets` edit.
- Any repository-wide coverage-percentage threshold assertion. The percentage is recorded as a reported figure by P0-T10 and P3-T7 and is not gated by this plan.
