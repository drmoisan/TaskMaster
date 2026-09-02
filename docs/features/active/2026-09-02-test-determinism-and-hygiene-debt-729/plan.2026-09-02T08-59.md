# test-determinism-and-hygiene-debt (Atomic Plan)

- **Issue:** #729
- **Parent:** none
- **Owner:** drmoisan
- **Last Updated:** 2026-09-02T14-10
- **Status:** Ready for Preflight (revision round 5)
- **Version:** 1.4
- **Work Mode:** full-bug (resolved from `docs/features/active/2026-09-02-test-determinism-and-hygiene-debt-729/issue.md` line 12, `- Work Mode: full-bug`)
- **Sole requirements/AC source:** `docs/features/active/2026-09-02-test-determinism-and-hygiene-debt-729/spec.md` (AC1–AC21). No `user-story.md` exists or is expected for this item.
- **Research artifact:** `docs/features/active/2026-09-02-test-determinism-and-hygiene-debt-729/research/research-729.2026-09-02T09-30.md`
- **Branch:** `bug/test-determinism-and-hygiene-debt-729`
- **Workspace root:** `<repo-root>` — the git worktree this plan executes in. No absolute host path, account name, or machine name is written into this plan or into any artifact it produces; see P7-T8.

**Fail-closed evidence rule:** Every command-bearing task writes its own evidence artifact carrying `Timestamp:`, `Command:`, `EXIT_CODE:`, and `Output Summary:`. A task whose artifact is missing or whose fields are incomplete stays UNCHECKED. `EXIT_CODE: SKIPPED` is never a passing outcome unless the task text itself authorizes a skip branch.

**Evidence location invariant:** All evidence artifacts live under exactly one of these four canonical directories: `docs/features/active/2026-09-02-test-determinism-and-hygiene-debt-729/evidence/baseline/`, `docs/features/active/2026-09-02-test-determinism-and-hygiene-debt-729/evidence/regression-testing/`, `docs/features/active/2026-09-02-test-determinism-and-hygiene-debt-729/evidence/qa-gates/`, and `docs/features/active/2026-09-02-test-determinism-and-hygiene-debt-729/evidence/other/`. No artifact is written to any non-canonical location such as an `artifacts/` evidence sub-path. Two transient raw tool outputs are not evidence artifacts and are deliberately kept out of those directories: the TRX files that P6-T5's two scoped node-outcome confirmation runs write to `coverage\trx\p6t5\utilitiescs-noliveform.trx` and `coverage\trx\p6t5\svgcontrol-noliveform.trx`, both untracked under `.gitignore` line 144 `coverage/*`. One derived result line per TRX is transcribed into the canonical P6-T5 artifact, which is what the acceptance reads.

**Evidence filename timestamp:** Every evidence filename in this plan carries the plan-assigned fixed stamp `2026-09-02T10-30` so that each path is concrete and harvestable. The `Timestamp:` field *inside* each artifact records the actual ISO-8601 execution time, which may differ from the filename stamp.

---

## Scope Recap (orchestrator decisions already made — do not re-litigate)

**Finding 1 — `TimeProvider` seam.** `TaskMaster/AppGlobals/NonBlockingDelay.cs` is the only production (non-test) file this plan writes. `WaitAsync` is split into an explicit overload pair. An optional `TimeProvider? = null` parameter is **rejected**: it removes the only method-group-conversion candidate at `TaskMaster/AppGlobals/StoreRehookCoordinator.cs` line 102 and yields CS0123 (research §1.2, re-derived below).

**Finding 2 — live `Form` types, expanded beyond the issue's literal citation.** Issue #729 names only `UtilitiesCS.Test/ResourceTests.cs:20`. That file is an orphan never compiled into `UtilitiesCS.Test`, so acting on the literal citation alone would leave the actual defect untouched. The orchestrator's decision is to **include `SVGControl.Test`**, which is the only site in the repository where a `Form`-derived type is genuinely compiled into a unit-test assembly and therefore the only site where fail-before evidence exists. This expansion is deliberate and is recorded in `docs/features/active/2026-09-02-test-determinism-and-hygiene-debt-729/spec.md` lines 74–79.

**Finding 3 — parallel-execution hazard, corrected.** The hazard is not a two-class conflict; it is process-global `Console.Out` mutation under the class-level parallel scope declared at `UtilitiesCS.Test/Properties/AssemblyInfo.cs` lines 18–21. Two compiled classes are unprotected. The new hazard comments cite `AssemblyInfo.cs`, not `TaskMaster.runsettings`: CI passes no `/Settings:` argument, so the existing precedent comments' `TaskMaster.runsettings` citation is stale and must not be repeated.

**Finding 4 — out of scope.** No file under `QuickFiler/` is added, modified, or deleted by this plan. Finding 4 (pump-hosted `QfcItemController` / `PumpTimeoutMs` load sensitivity) was scoped out and promoted as follow-up issue **#743** (`docs/features/potential/promoted/2026-09-02-quickfiler-itemviewer-ui-marshalling-seam.md`). No action is required from this plan beyond recording the citation. The prior standalone tracker #711 was already closed as superseded by #729, so #743 exists specifically so that closing #729 does not drop the finding a second time.

**Known harvester limitation (surfaced, not worked around).** The repository-relative path `UtilitiesCS.Test/OutlookObjects/Filter DASL/DASLFilterParserTests.cs` contains a space in the directory segment `Filter DASL`. It is written throughout this plan as a single backticked token, but it will **not** survive a whitespace-splitting blast-radius/contention extractor as one token. Renaming the directory is out of scope for this minimal bugfix. Downstream schedulers must treat this one path specially.

---

## Decisions Record (binding on the executor)

- **D1 — No optional parameter.** Neither `WaitAsync` overload declares an optional parameter. The 1-arg overload must remain the unique 1-parameter candidate so the method-group conversion at `TaskMaster/AppGlobals/StoreRehookCoordinator.cs` line 102 keeps binding to `Func<TimeSpan, Task>`.
- **D2 — No null guard on `timeProvider`.** The 2-arg overload does **not** validate `timeProvider` for null. `NonBlockingDelay` is `internal static`; its only in-repo caller of the 2-arg overload is the 1-arg overload, which always supplies `TimeProvider.System`. A guard would add a branch outcome that no in-scope test can reach without a fourth test method that the spec's Test Strategy does not authorize, and an unreached branch outcome lowers branch coverage on a changed production file. This is a deliberate minimal-change decision.
- **D3 — No production project-file edit for `TimeProvider`.** Re-derived against the current tree: `TaskMaster/TaskMaster.csproj` already carries `<Reference Include="Microsoft.Bcl.TimeProvider, ...>` at lines 148–149 and `TaskMaster/packages.config` already pins `Microsoft.Bcl.TimeProvider` 10.0.11 at line 16. `System.TimeProvider` and `System.Threading.ITimer` therefore resolve in the `TaskMaster` project with the `using System;` and `using System.Threading;` directives already present in `TaskMaster/AppGlobals/NonBlockingDelay.cs`. No edit to `TaskMaster/TaskMaster.csproj` or `TaskMaster/packages.config` is planned or permitted.
- **D4 — Scoped formatting pass.** The mutating `csharpier format` pass in Phase 6 is scope-locked to the seven formattable paths this plan owns. A repo-wide `dotnet tool run csharpier format .` is prohibited because it would rewrite files that are unformatted at the merge base and thereby break AC17/AC18/AC19/AC20. The repo-wide read-only `dotnet tool run csharpier check .` remains the gate.
- **D5 — Coverage floor conflict, recorded not resolved.** `CLAUDE.md` § UT2 states a repository-wide line floor of `>= 80%` with `>= 90%` for new modules. `.claude/rules/general-unit-test.md` and `.claude/rules/quality-tiers.md` state `>= 85%` line and `>= 75%` branch uniformly. `scripts/vscode/Invoke-MSTestWithCoverage.ps1` enforces only the 80% line figure (helper `Assert-CoberturaLineCoverageThreshold`, `Invoke-MSTestWithCoverage.Helpers.ps1` lines 459–491). This plan does **not** adopt an absolute repository-wide floor as its own gate, because the pre-existing repository figure is not owned by this change and an absolute floor could be unsatisfiable for reasons outside this change. The binding coverage gates for this plan are: (a) the repository-wide post-change line-rate and branch-rate must be no lower than the Phase 0 baseline figures, and (b) the covered/total line counts for `TaskMaster/AppGlobals/NonBlockingDelay.cs` must not regress. Both conflicting floors are recorded verbatim in the Phase 0 baseline artifact so a reviewer can see the conflict rather than an agent-invented reconciliation.
- **D6 — Project-level vs solution-level platform spelling.** Solution builds use `"/p:Platform=Any CPU"` (with the space, quoted). Single-project builds use `/p:Platform=AnyCPU` (no space), which is the spelling in the `Debug|AnyCPU` condition of the legacy project files (verified at `SVGControl.Test/SVGControl.Test.csproj` lines 10 and 29). Using the solution spelling on a single project selects no configuration and silently produces the wrong output path.
- **D7 — `vstest.console.exe`, `MSBuild.exe`, and `vswhere.exe` are not on PATH.** Every task that runs them resolves them exactly as `scripts/vscode/Invoke-Restore.ps1` lines 22–30 do.
- **D8 — Scoped test runs, not `Invoke-MSTest.ps1`.** `scripts/vscode/Invoke-MSTest.ps1` throws under `Set-StrictMode` when `-SearchRoot` resolves to exactly one test assembly, so scoped single-assembly verification in this plan calls `vstest.console.exe` directly with `/Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation` and an explicit `/TestCaseFilter:`.
- **D9 — Discovered-assembly assertion form.** This workspace root is itself under a `\worktrees\` path, so an assertion that "no discovered assembly path contains `\.claude\`" is unsatisfiable here. Full-suite tasks assert that the discovered-assembly count matches the runner's `Discovered N test assemblies.` line and that zero discovered paths contain a `\worktrees\` segment below the resolved search root. The artifact records the integer count, the boolean result, and the repository-relative form of each assembly path (for example `UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll`). No absolute path is written into any artifact. The runner prints the count at `scripts/vscode/Invoke-MSTestWithCoverage.ps1` line 315 and enumerates absolute `FullName` values at lines 296-302, so the repository-relative form is derived by removing the resolved search-root prefix from each `FullName`.
- **D10 — No `.claude/**` writes by this plan's own tasks.** No task in this plan writes a file under `.claude/**`, `.codex/**`, `.agents/**`, `config/blast-radius.json`, or `config/orchestration-routing.json`, and no task commits one. That prohibition covers `.claude/agent-memory/`, which is a tracked directory in this repository. It is a prohibition on this plan's tasks, not a claim that the directory is clean: the persistent-memory system of delegated agents writes to `.claude/agent-memory/<agent>/` outside any plan task, and P0-T15 records that some such paths are already dirty before Phase 1 begins. The `git diff $base HEAD` half of P7-T4 stays an unconditional empty-output assertion because no plan task commits any such path. P7-T2, P7-T4, and P8-T22 therefore evaluate cleanliness against the P0-T15 recorded set plus the `.claude/agent-memory/` allowance rather than against the empty set.
- **D11 — `$base` is re-derived per task.** The shell resets between tasks, so every task whose command text contains `$base` begins its payload with `$base = (git merge-base origin/main HEAD).Trim()` and then asserts that `$base` equals the `BaseRef:` value recorded by P0-T14. A task that finds a different value stops and reports rather than proceeding on a drifted base.
- **D12 — Test-count assertions are numeric, not whitespace-matched.** `vstest.console.exe` summary formatting varies between versions, so no acceptance in this plan matches the console line's internal spacing. Each test task records the passed count and the failed count as integers in its evidence artifact and its acceptance compares those integers.
- **D13 — Cobertura artifacts carry a declared processing state.** `scripts/vscode/Invoke-MSTestWithCoverage.ps1` calls `ConvertTo-KoverageCoberturaXml` at line 340, asserts the 80% line threshold at line 341, and writes the post-processed XML at line 343. Any throw before line 343 — from the collection call at line 326 or from the threshold assertion at line 341 — leaves the raw dotnet-coverage output on disk. The processed and raw forms are not on the same denominator, because the conversion removes non-allowlisted packages and then rewrites the root `line-rate`, `branch-rate`, `lines-covered`, `lines-valid`, `branches-covered`, and `branches-valid` attributes (`Invoke-MSTestWithCoverage.Helpers.ps1` lines 417-447). Both full-suite tasks therefore record `CoberturaProcessingState:` and P6-T6 refuses to compare across a mismatch without converting the raw side first.
- **D14 — One authorized re-run for #743-tracked `QuickFiler.Test` failures, and nothing wider.** `Invoke-DotnetCoverageCollection` throws on any non-zero coverage exit code (`Invoke-MSTestWithCoverage.ps1` lines 235-237), so a single load-sensitive `QuickFiler.Test` pump timeout aborts the whole gate. `spec.md` line 87 records that failure mode as out of scope and carried by issue #743, and this plan modifies no `QuickFiler/` file. The full-suite tasks therefore authorize exactly one mechanical re-run when every failing node is in `QuickFiler.Test`, require the failing node identifiers to be enumerated, and treat a failure in any other assembly as a genuine gate failure.
- **D15 — The Phase 6 formatter pass is staged by P6-T9.** The scope-locked `csharpier format` pass in P6-T1 runs after all five phase commits, so any rewrite it applies to the seven plan-owned formattable source paths is left unstaged by every earlier task. P6-T9 therefore stages those seven paths in addition to the Phase 6 evidence, and P8-T22 amends its own commit to absorb its own check-off line.

---

## Complete file-write inventory

Production source (exactly one file):

- `TaskMaster/AppGlobals/NonBlockingDelay.cs` — modified

Test sources modified:

- `TaskMaster.Test/AppGlobals/NonBlockingDelayTests.cs`
- `UtilitiesCS.Test/OutlookObjects/Filter DASL/DASLFilterParserTests.cs`
- `UtilitiesCS.Test/ReusableTypeClasses/StackGeek_Tests.cs`

Test sources created:

- `UtilitiesCS.Test/NoLiveFormInTestAssemblyTests.cs`
- `SVGControl.Test/NoLiveFormInTestAssemblyTests.cs`

Test project files and package manifests modified:

- `TaskMaster.Test/TaskMaster.Test.csproj`
- `TaskMaster.Test/packages.config`
- `UtilitiesCS.Test/UtilitiesCS.Test.csproj`
- `SVGControl.Test/SVGControl.Test.csproj`

Files deleted (17):

- `UtilitiesCS.Test/ResourceTests.cs`
- `UtilitiesCS.Test/Form1.cs`
- `UtilitiesCS.Test/Form1.Designer.cs`
- `UtilitiesCS.Test/Form1.resx`
- `UtilitiesCS.Test/Form2.cs`
- `UtilitiesCS.Test/Form2.Designer.cs`
- `UtilitiesCS.Test/Form2.resx`
- `UtilitiesCS.Test/Form3.cs`
- `UtilitiesCS.Test/Form3.Designer.cs`
- `UtilitiesCS.Test/Form3.resx`
- `UtilitiesCS.Test/OutlookObjects/DASLFilterParser_Tests.cs`
- `SVGControl.Test/Form1.cs`
- `SVGControl.Test/Form1.Designer.cs`
- `SVGControl.Test/Form1.resx`
- `SVGControl.Test/Form2.cs`
- `SVGControl.Test/Form2.Designer.cs`
- `SVGControl.Test/Form2.resx`

Feature documentation and evidence written:

- `docs/features/active/2026-09-02-test-determinism-and-hygiene-debt-729/plan.2026-09-02T08-59.md`
- `docs/features/active/2026-09-02-test-determinism-and-hygiene-debt-729/spec.md` (AC checkbox state, plus the one Block L insertion under the Finding 4 out-of-scope bullet required by AC16; no other spec content changes)
- `docs/features/active/2026-09-02-test-determinism-and-hygiene-debt-729/issue.md` — authored before this plan and untracked until P8-T22's directory-level `git add` commits it; this plan does not edit its content
- `docs/features/active/2026-09-02-test-determinism-and-hygiene-debt-729/research/research-729.2026-09-02T09-30.md` — authored before this plan and untracked until P8-T22's directory-level `git add` commits it; this plan does not edit its content except for any host-identifier substitution P7-T8 applies
- all artifacts named in Phases 0–8 under `docs/features/active/2026-09-02-test-determinism-and-hygiene-debt-729/evidence/`

Follow-up promotion record committed by this plan:

- `docs/features/potential/promoted/2026-09-02-quickfiler-itemviewer-ui-marshalling-seam.md` — the promotion record for follow-up issue #743, created by this run's MCP promotion and cited by `spec.md` line 87 and by AC16. It is real output of this work rather than incidental drift, so P8-T22 stages and commits it. This plan does not edit its content.

Read-only, never written by this plan: `TaskMaster/AppGlobals/StoreRehookCoordinator.cs`, `TaskMaster/AppGlobals/AppEvents.cs`, `TaskMaster/TaskMaster.csproj`, `TaskMaster/packages.config`, `TaskMaster.Test/app.config`, `UtilitiesCS/OutlookObjects/Filter DASL/DASLFilterParser.cs`, `QuickFiler.Test/NoLiveFormInTestAssemblyTests.cs`, `UtilitiesCS.Test/Properties/AssemblyInfo.cs`, `UtilitiesCS.Test/HelperClasses/PrettyPrint_Tests.cs`, `UtilitiesCS.Test/OutlookObjects/Table/OlTableExtensions_Tests.cs`, `UtilitiesCS.Test/HelperClasses/NLogTraceWriter_Test.cs`.

---

## Authoritative content blocks

The executor writes these blocks verbatim. No content selection is delegated.

#### Block A — full replacement content of `TaskMaster/AppGlobals/NonBlockingDelay.cs`

```csharp
using System;
using System.Threading;
using System.Threading.Tasks;

namespace TaskMaster
{
    /// <summary>
    /// Non-blocking, pump-independent replacement for <c>Task.Delay</c> (Issue #207, AC10).
    /// </summary>
    /// <remarks>
    /// <para>
    /// <see cref="WaitAsync(TimeSpan)"/> returns a <see cref="Task"/> that completes when a one-shot
    /// timer fires its callback. The awaiting code yields control to the message loop, so an STA keeps
    /// pumping window messages during the wait. The timer callback fires on a threadpool thread and
    /// sets a <see cref="TaskCompletionSource{TResult}"/>; the <c>await</c> continuation then resumes
    /// on the captured STA <see cref="SynchronizationContext"/>, so subsequent COM work still runs on
    /// the STA.
    /// </para>
    /// <para>
    /// Unlike the prior <c>DispatcherTimer</c>-backed design, this helper completes whether or not a
    /// <see cref="System.Windows.Threading.Dispatcher"/> is running on the current thread. That
    /// pump-independence is required for the helper to be unit-testable on the pump-less MSTest host
    /// (the <c>DispatcherTimer</c> design completed only on a Dispatcher tick and hung the host).
    /// </para>
    /// <para>
    /// Timer scheduling goes through <see cref="TimeProvider"/> (Issue #729, Finding 1) so a test can
    /// drive completion from virtual time instead of a real <c>Stopwatch</c> wait. The seam is an
    /// explicit overload pair rather than an optional parameter: <c>WaitAsync</c> is consumed as a
    /// method group at <c>StoreRehookCoordinator</c>, and C# ignores a candidate method whose optional
    /// parameter has no corresponding parameter in the target delegate type, which would produce
    /// CS0123 at that call site.
    /// </para>
    /// <para>
    /// Neither <see cref="TimeProvider"/> nor <c>System.Threading.Timer</c> is a banned API (the
    /// banned list is <c>DateTime.Now</c>, <c>DateTime.UtcNow</c>, <c>Random.Shared</c>,
    /// <c>Thread.Sleep</c>, <c>Task.Delay</c>), so the helper satisfies AC10. It carries the new-code
    /// coverage obligation (it is not COM/VSTO-exempt).
    /// </para>
    /// </remarks>
    internal static class NonBlockingDelay
    {
        /// <summary>
        /// Returns a <see cref="Task"/> that completes after <paramref name="delay"/> elapses,
        /// without blocking the calling thread and without requiring a running
        /// <see cref="System.Windows.Threading.Dispatcher"/>. Scheduling is supplied by
        /// <see cref="TimeProvider.System"/>.
        /// </summary>
        /// <param name="delay">The interval to wait before completing the task.</param>
        /// <returns>A task that completes when the one-shot timer callback fires.</returns>
        public static Task WaitAsync(TimeSpan delay)
        {
            return WaitAsync(delay, TimeProvider.System);
        }

        /// <summary>
        /// Returns a <see cref="Task"/> that completes after <paramref name="delay"/> elapses on the
        /// supplied <paramref name="timeProvider"/>'s clock, without blocking the calling thread and
        /// without requiring a running <see cref="System.Windows.Threading.Dispatcher"/>. A one-shot
        /// <see cref="ITimer"/> is created with a due time of <paramref name="delay"/> and an infinite
        /// period; in its callback the returned task is completed and the timer is disposed.
        /// </summary>
        /// <param name="delay">The interval to wait before completing the task.</param>
        /// <param name="timeProvider">The clock that schedules the one-shot completion callback.</param>
        /// <returns>A task that completes when the one-shot timer callback fires.</returns>
        public static Task WaitAsync(TimeSpan delay, TimeProvider timeProvider)
        {
            var tcs = new TaskCompletionSource<bool>(
                TaskCreationOptions.RunContinuationsAsynchronously
            );
            // This file has no project-level <Nullable> element and no whole-file #nullable
            // pragma; this `?` annotation on a self-referencing local (assigned to itself inside
            // its own closure below) needs an explicit annotations context to avoid CS8632.
            // Scoping narrowly to annotations-only avoids introducing new CS86xx diagnostics
            // elsewhere in this file (no behavior change).
#nullable enable annotations
            ITimer? timer = null;
#nullable restore annotations
            timer = timeProvider.CreateTimer(
                _ =>
                {
                    timer?.Dispose();
                    tcs.TrySetResult(true);
                },
                null,
                delay,
                Timeout.InfiniteTimeSpan
            );
            return tcs.Task;
        }
    }
}
```

#### Block B — full replacement content of `TaskMaster.Test/AppGlobals/NonBlockingDelayTests.cs`

```csharp
using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Time.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TaskMaster.Test.AppGlobals
{
    /// <summary>
    /// Deterministic unit tests for <see cref="NonBlockingDelay"/>, the pump-independent
    /// <see cref="TimeProvider"/>-backed replacement for <c>Task.Delay</c> (Issue #207 AC10;
    /// Issue #729 Finding 1). These tests run on the standard pump-less MSTest host with NO running
    /// <c>System.Windows.Threading.Dispatcher</c>, proving the helper completes whether or not a
    /// Dispatcher is present. Virtual time is supplied by <see cref="FakeTimeProvider"/>, so no
    /// <c>Stopwatch</c> and no real wall-clock wait is used. No Moq, no filesystem, no temporary
    /// files, and no banned API (<c>Thread.Sleep</c>/<c>Task.Delay</c>) are used.
    /// </summary>
    [TestClass]
    public class NonBlockingDelayTests
    {
        /// <summary>
        /// Scenario: with no Dispatcher running on the test thread, the task returned by the
        /// <see cref="TimeProvider"/> overload stays incomplete until virtual time reaches the
        /// requested interval, then completes.
        /// Expected: the task is not completed before <c>Advance</c>, and transitions to
        /// RanToCompletion after it. Asserting non-completion before the advance is strictly stronger
        /// than the previous elapsed-time check, because it proves the task cannot complete early.
        /// The outer MSTest <c>[Timeout]</c> is a deadlock bound, not a wait.
        /// </summary>
        [TestMethod]
        [Timeout(5000)]
        public async Task WaitAsync_WithNoDispatcher_CompletesAfterInterval()
        {
            // Arrange
            SynchronizationContext
                .Current.Should()
                .BeNull(
                    "the pump-less MSTest host must not have a Dispatcher SynchronizationContext"
                );
            var interval = TimeSpan.FromMilliseconds(30);
            var fakeTimeProvider = new FakeTimeProvider();

            // Act
            var waitTask = NonBlockingDelay.WaitAsync(interval, fakeTimeProvider);
            waitTask
                .IsCompleted.Should()
                .BeFalse("the one-shot timer must not fire before virtual time reaches the interval");
            fakeTimeProvider.Advance(interval);
            await waitTask;

            // Assert
            waitTask
                .Status.Should()
                .Be(
                    TaskStatus.RanToCompletion,
                    "the one-shot timer callback completes the task successfully"
                );
        }

        /// <summary>
        /// Scenario: a zero-length wait still completes deterministically without a Dispatcher.
        /// Expected: FakeTimeProvider fires a due timer on the next advance rather than at creation,
        /// so the task is incomplete immediately after creation and completes after
        /// <c>Advance(TimeSpan.Zero)</c>. This confirms the helper does not depend on any message pump.
        /// </summary>
        [TestMethod]
        [Timeout(5000)]
        public async Task WaitAsync_ZeroDelay_CompletesWithoutPump()
        {
            // Arrange
            var fakeTimeProvider = new FakeTimeProvider();

            // Act
            var waitTask = NonBlockingDelay.WaitAsync(TimeSpan.Zero, fakeTimeProvider);
            waitTask
                .IsCompleted.Should()
                .BeFalse("FakeTimeProvider fires a due timer on the next advance, not at creation");
            fakeTimeProvider.Advance(TimeSpan.Zero);
            await waitTask;

            // Assert
            waitTask
                .Status.Should()
                .Be(
                    TaskStatus.RanToCompletion,
                    "a zero-delay wait completes via the timer callback"
                );
        }

        /// <summary>
        /// Scenario: the single-argument overload, which is the one production callers bind to as a
        /// method group, completes on the real system clock.
        /// Expected: the task completes successfully under the timeout guard. This is a completion
        /// assertion, not a duration assertion, so no wall-clock dependency is reintroduced. The test
        /// exists because StoreRehookCoordinatorTests supplies an explicit delay at both construction
        /// sites and therefore never reaches the NonBlockingDelay.WaitAsync fallback, leaving the
        /// single-argument body otherwise uncovered.
        /// </summary>
        [TestMethod]
        [Timeout(5000)]
        public async Task WaitAsync_SingleArgumentOverload_CompletesOnSystemTimeProvider()
        {
            // Arrange
            var interval = TimeSpan.Zero;

            // Act
            var waitTask = NonBlockingDelay.WaitAsync(interval);
            await waitTask;

            // Assert
            waitTask
                .Status.Should()
                .Be(
                    TaskStatus.RanToCompletion,
                    "the single-argument overload delegates to TimeProvider.System and completes "
                        + "without a Dispatcher"
                );
        }
    }
}
```

#### Block C — `TaskMaster.Test/TaskMaster.Test.csproj` insertions (verbatim mirror of `UtilitiesCS.Test/UtilitiesCS.Test.csproj` lines 591–593 and 643–645)

Insertion 1 — between the `</Reference>` that closes `Microsoft.Bcl.AsyncInterfaces` and the following `<Reference Include="Microsoft.Build" />` line:

```xml
    <Reference Include="Microsoft.Bcl.TimeProvider, Version=10.0.0.11, Culture=neutral, PublicKeyToken=cc7b13ffcd2ddd51, processorArchitecture=MSIL">
      <HintPath>..\packages\Microsoft.Bcl.TimeProvider.10.0.11\lib\net462\Microsoft.Bcl.TimeProvider.dll</HintPath>
    </Reference>
```

Insertion 2 — between the `</Reference>` that closes `Microsoft.Extensions.Primitives` and the following `<Reference Include="Microsoft.Identity.Client, ...">` line:

```xml
    <Reference Include="Microsoft.Extensions.TimeProvider.Testing, Version=10.9.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35, processorArchitecture=MSIL">
      <HintPath>..\packages\Microsoft.Extensions.TimeProvider.Testing.10.9.0\lib\net462\Microsoft.Extensions.TimeProvider.Testing.dll</HintPath>
    </Reference>
```

#### Block D — `TaskMaster.Test/packages.config` insertions

Insertion 1 — immediately after the `Microsoft.Bcl.AsyncInterfaces` entry and before the `Microsoft.CodeAnalysis.BannedApiAnalyzers` entry:

```xml
  <package id="Microsoft.Bcl.TimeProvider" version="10.0.11" targetFramework="net481" />
```

Insertion 2 — immediately after the `Microsoft.Extensions.Primitives` entry and before the `Microsoft.Identity.Client` entry:

```xml
  <package
    id="Microsoft.Extensions.TimeProvider.Testing"
    version="10.9.0"
    targetFramework="net481"
  />
```

#### Block E — full content of `SVGControl.Test/NoLiveFormInTestAssemblyTests.cs`

```csharp
using System;
using System.Linq;
using System.Reflection;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SVGControl.Test
{
    /// <summary>
    /// Structural guard: no live WinForms window type may be compiled into this unit-test
    /// assembly. Reflection is over type metadata only; nothing is instantiated.
    /// </summary>
    [TestClass]
    public class NoLiveFormInTestAssemblyTests
    {
        [TestMethod]
        public void ExecutingAssembly_ContainsNoFormDerivedType()
        {
            // Arrange - metadata only; scoped to the executing assembly, never a referenced one.
            Type formType = typeof(System.Windows.Forms.Form);
            Assembly executing = Assembly.GetExecutingAssembly();

            // Act
            string[] formDerivedTypeNames = GetLoadableTypes(executing)
                .Where(candidate => formType.IsAssignableFrom(candidate))
                .Select(candidate => candidate.FullName)
                .OrderBy(name => name, StringComparer.Ordinal)
                .ToArray();

            // Assert
            formDerivedTypeNames
                .Should()
                .BeEmpty(
                    "a unit-test assembly must not compile a live System.Windows.Forms.Form type"
                );
        }

        // Reflection over a large test assembly can hit a single type whose dependencies fail to
        // resolve, and GetTypes then throws for the whole assembly. That would leave this guard
        // permanently red for a reason unrelated to what it measures, so the loaded subset carried
        // on the exception is used instead; its null entries are the types that did not load.
        private static Type[] GetLoadableTypes(Assembly assembly)
        {
            try
            {
                return assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException ex)
            {
                return ex.Types.Where(candidate => candidate != null).ToArray();
            }
        }
    }
}
```

#### Block F — full content of `UtilitiesCS.Test/NoLiveFormInTestAssemblyTests.cs`

Byte-identical to Block E except the namespace line, which reads:

```csharp
namespace UtilitiesCS.Test
```

#### Block G — `SVGControl.Test/SVGControl.Test.csproj` and `UtilitiesCS.Test/UtilitiesCS.Test.csproj` compile-entry insertion

Both projects receive exactly this line, indented with four spaces, inside their first `<ItemGroup>` that holds `<Compile>` items:

```xml
    <Compile Include="NoLiveFormInTestAssemblyTests.cs" />
```

In `SVGControl.Test/SVGControl.Test.csproj` it is inserted immediately before the existing `<Compile Include="GetRelativePath_Test.cs" />` line. In `UtilitiesCS.Test/UtilitiesCS.Test.csproj` it is inserted immediately after the existing `<Compile Include="TestAssemblyInitializer.cs" />` line.

#### Block H — `SVGControl.Test/SVGControl.Test.csproj` removals

Remove these six contiguous-in-file item elements in their entirety, including their child elements and closing tags:

```xml
    <Compile Include="Form1.cs">
      <SubType>Form</SubType>
    </Compile>
    <Compile Include="Form1.Designer.cs">
      <DependentUpon>Form1.cs</DependentUpon>
    </Compile>
    <Compile Include="Form2.cs">
      <SubType>Form</SubType>
    </Compile>
    <Compile Include="Form2.Designer.cs">
      <DependentUpon>Form2.cs</DependentUpon>
    </Compile>
```

```xml
    <EmbeddedResource Include="Form1.resx">
      <DependentUpon>Form1.cs</DependentUpon>
    </EmbeddedResource>
    <EmbeddedResource Include="Form2.resx">
      <DependentUpon>Form2.cs</DependentUpon>
    </EmbeddedResource>
```

Nothing else in that project file changes. In particular `<Reference Include="System.Windows.Forms" />` stays, because Block E references `System.Windows.Forms.Form` by fully-qualified name.

#### Block I — hazard comment inserted above `[TestClass]` in `UtilitiesCS.Test/OutlookObjects/Filter DASL/DASLFilterParserTests.cs`

```csharp
    // PrintTree_WritesIndentedTreeToConsole captures and restores Console.Out, which is
    // process-wide state. Under the class-level parallel scope declared by the Parallelize
    // attribute at UtilitiesCS.Test/Properties/AssemblyInfo.cs lines 18-21, a sibling test
    // class's Console.SetOut overrides this class's redirect mid-test and makes the captured
    // output empty. The assembly attribute, not TaskMaster.runsettings, is what takes effect:
    // the CI vstest invocation passes no /Settings: argument.
    [DoNotParallelize]
```

#### Block J — hazard comment inserted above `[TestClass]` in `UtilitiesCS.Test/ReusableTypeClasses/StackGeek_Tests.cs`

```csharp
    // Main_RunsSampleScenarioWithoutThrowing captures and restores Console.Out, which is
    // process-wide state. Under the class-level parallel scope declared by the Parallelize
    // attribute at UtilitiesCS.Test/Properties/AssemblyInfo.cs lines 18-21, a sibling test
    // class's Console.SetOut overrides this class's redirect mid-test and makes the captured
    // output empty. The assembly attribute, not TaskMaster.runsettings, is what takes effect:
    // the CI vstest invocation passes no /Settings: argument.
    [DoNotParallelize]
```

#### Block K — canonical tool-resolution prelude

Every task that runs MSBuild or vstest begins its PowerShell payload with this prelude, executed from the workspace root:

```powershell
$vswhere = Join-Path ${env:ProgramFiles(x86)} 'Microsoft Visual Studio\Installer\vswhere.exe'
$msbuild = & $vswhere -latest -requires Microsoft.Component.MSBuild -find 'MSBuild\**\Bin\MSBuild.exe' | Select-Object -First 1
$vstest  = & $vswhere -latest -products * -find 'Common7\IDE\Extensions\TestPlatform\vstest.console.exe' | Select-Object -First 1
```

#### Block L — the four Finding-4 reasons, quoted from research §4.2

Research artifact `docs/features/active/2026-09-02-test-determinism-and-hygiene-debt-729/research/research-729.2026-09-02T09-30.md` line 240 states that a test-side replacement of `WinFormsPumpHost` "is nevertheless not *sufficient*, for four independently verified reasons", and enumerates them at lines 242, 248, 250, and 252. Their four bolded lead sentences are reproduced verbatim below. This block is written into two places: the P0-T2 scope-recap artifact, and the `spec.md` out-of-scope bullet (P7-T7). The heading line is part of the block and is written exactly as shown.

```text
Finding 4 — reasons no test-only fix exists:
1. The production code reads the context off the control, not from an injected seam.
2. The fixture's cost is the real WinForms control tree, not the pump.
3. `[DoNotParallelize]` would be a no-op.
4. Removing `[Timeout]` trades a bounded failure for an unbounded hang.
```

---

### Phase 0 — Policy reads, worktree bootstrap, and baseline capture

- [ ] [P0-T1] Read, in this order, `CLAUDE.md`, `.claude/rules/general-code-change.md`, `.claude/rules/general-unit-test.md`, `.claude/rules/csharp.md`, `.claude/rules/quality-tiers.md`, `.claude/rules/tonality.md`, and `.claude/rules/plan-acceptance-gates.md`, then write `docs/features/active/2026-09-02-test-determinism-and-hygiene-debt-729/evidence/baseline/phase0-instructions-read.2026-09-02T10-30.md` containing `Timestamp:`, `Policy Order:` listing those seven paths in that order, and the line `Files Read: 7`. Acceptance: the artifact exists and contains all seven paths and the literal `Files Read: 7`.
- [ ] [P0-T2] Write `docs/features/active/2026-09-02-test-determinism-and-hygiene-debt-729/evidence/other/scope-recap.2026-09-02T10-30.md` recording (a) the Finding 2 expansion to `SVGControl.Test`, (b) that Finding 4 is out of scope and promoted as issue #743 at `docs/features/potential/promoted/2026-09-02-quickfiler-itemviewer-ui-marshalling-seam.md`, (c) the whitespace-in-path harvester limitation for `UtilitiesCS.Test/OutlookObjects/Filter DASL/DASLFilterParserTests.cs`, and (d) Block L verbatim, so the four verified reasons no test-only fix exists for Finding 4 are enumerated as four numbered lines under the Block L heading line. Acceptance: the artifact exists, contains the literal tokens `#743`, `SVGControl.Test`, and `Filter DASL`, contains one line equal to `Finding 4 — reasons no test-only fix exists:`, and contains four lines immediately below it beginning `1. `, `2. `, `3. `, and `4. ` respectively.
- [ ] [P0-T3] Provision the repository-pinned .NET SDK by running `pwsh -NoProfile -File .\scripts\vscode\Install-RepoDotNetSdk.ps1` from the workspace root, then record `docs/features/active/2026-09-02-test-determinism-and-hygiene-debt-729/evidence/baseline/dotnet-sdk-bootstrap.2026-09-02T10-30.md`. Acceptance: `dotnet --version` prints `8.0.205`, pasted into `Output Summary:`, and the artifact records `RepoLocalSdkPresent: True` derived from whether any `dotnet --list-sdks` entry's path ends `.dotnet-sdk\sdk`. The `dotnet --list-sdks` output itself is not pasted, because it prints an absolute host path.
- [ ] [P0-T4] Run `dotnet tool restore` from the workspace root and record `docs/features/active/2026-09-02-test-determinism-and-hygiene-debt-729/evidence/baseline/dotnet-tool-restore.2026-09-02T10-30.md`. Acceptance: `EXIT_CODE: 0`, and `dotnet tool run csharpier --version` prints `1.2.6`, pasted into `Output Summary:`.
- [ ] [P0-T5] Run `pwsh -NoProfile -File .\scripts\vscode\Invoke-Restore.ps1` from the workspace root and record `docs/features/active/2026-09-02-test-determinism-and-hygiene-debt-729/evidence/baseline/nuget-restore.2026-09-02T10-30.md`. Acceptance: `EXIT_CODE: 0` and `Test-Path .\packages\MSTest.TestFramework.4.3.3` returns `True`, pasted into `Output Summary:`.
- [ ] [P0-T6] Enumerate every `Analyzer` `Include` value from every non-`packages` `*.csproj` in the workspace, resolve each one against the declaring project's own directory, `Test-Path` each resolved path, and record the count of resolved and unresolved paths in `docs/features/active/2026-09-02-test-determinism-and-hygiene-debt-729/evidence/baseline/analyzer-path-audit.2026-09-02T10-30.md`. If any path is unresolved, install the named package version into `packages\` with `nuget install` or copy it from the main checkout and re-run the audit until the unresolved count is 0. Record only repository-relative paths and package identifiers in the artifact. Do not write any absolute host path, account name, or machine name into it. Acceptance: the artifact records `Unresolved: 0`; `Select-String -SimpleMatch ':\' -Path 'docs/features/active/2026-09-02-test-determinism-and-hygiene-debt-729/evidence/baseline/analyzer-path-audit.2026-09-02T10-30.md'` returns zero matches; and `Select-String -SimpleMatch (Split-Path -Leaf $env:USERPROFILE) -Path 'docs/features/active/2026-09-02-test-determinism-and-hygiene-debt-729/evidence/baseline/analyzer-path-audit.2026-09-02T10-30.md'` returns zero matches.
- [ ] [P0-T7] Ensure the `dotnet-coverage` global tool is present by running `if (-not (Get-Command dotnet-coverage -ErrorAction SilentlyContinue)) { dotnet tool install --global dotnet-coverage }`, then record `docs/features/active/2026-09-02-test-determinism-and-hygiene-debt-729/evidence/baseline/dotnet-coverage-tool.2026-09-02T10-30.md`. Acceptance: `dotnet-coverage --version` prints a version string, pasted into `Output Summary:`.
- [ ] [P0-T8] Run the read-only formatter gate `dotnet tool run csharpier check .` from the workspace root and record `docs/features/active/2026-09-02-test-determinism-and-hygiene-debt-729/evidence/baseline/csharpier-check.2026-09-02T10-30.md`, pasting the complete list of files it reports as unformatted (an empty list if there are none) under a heading `Baseline unformatted set:`. This list is the comparison basis for P6-T2; it is not merely a pass/fail. Acceptance: the artifact records `EXIT_CODE:` and a `Baseline unformatted set:` section that is either empty or enumerates every reported path.
- [ ] [P0-T9] Run `& $msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` using the Block K prelude and record `docs/features/active/2026-09-02-test-determinism-and-hygiene-debt-729/evidence/baseline/msbuild-analyzers.2026-09-02T10-30.md`. Acceptance: `EXIT_CODE: 0` and the MSBuild summary line reporting the error count is pasted into `Output Summary:`.
- [ ] [P0-T10] Run `& $msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true` using the Block K prelude and record `docs/features/active/2026-09-02-test-determinism-and-hygiene-debt-729/evidence/baseline/msbuild-nullable.2026-09-02T10-30.md`. Do not add `/p:Nullable=enable` and do not substitute `/t:Build`. Acceptance: `EXIT_CODE: 0` and the MSBuild error-count summary line pasted into `Output Summary:`.
- [ ] [P0-T11] Run `pwsh -NoProfile -File .\scripts\vscode\Invoke-MSTestWithCoverage.ps1 -SearchRoot . -Configuration Debug -CoverageOutput 'docs\features\active\2026-09-02-test-determinism-and-hygiene-debt-729\evidence\baseline\coverage-baseline.cobertura.xml'` and record `docs/features/active/2026-09-02-test-determinism-and-hygiene-debt-729/evidence/baseline/mstest-coverage.2026-09-02T10-30.md`. Paste into `Output Summary:` the script's `Discovered N test assemblies.` line, the vstest total/passed/failed counts, and the `line-rate` and `branch-rate` attribute values read from the `<coverage>` root element of `docs/features/active/2026-09-02-test-determinism-and-hygiene-debt-729/evidence/baseline/coverage-baseline.cobertura.xml`. Authorized non-zero branch: if the script exits non-zero and its error text contains the literal `is below the required 80% threshold`, record that observed exit code together with the line `ExpectedExitCode: 1` and the literal note `Threshold assertion fired; raw Cobertura was written before the assertion and the numeric rates below were read from it.` Authorized mechanical re-run branch: if the run reports failures and every failing test node is in `QuickFiler.Test` and is one of the pump-hosted classes tracked by issue #743, re-run this exact command once, record both runs and the full list of failing test node identifiers in the artifact, and treat the second run's counts as authoritative. If a #743-tracked failure persists after the second run, record it in the artifact and in the delivery-record task as a known out-of-scope flake. A failure in any other assembly is not covered by this branch. Record the field `CoberturaProcessingState:` with the value `processed` when the script exited 0, and the value `raw` when the script exited non-zero under either authorized branch above; the value is `raw` in those cases because `scripts/vscode/Invoke-MSTestWithCoverage.ps1` writes the post-processed XML at line 343 only after both the collection call at line 326 and the threshold assertion at line 341 have returned, so a throw at either point leaves the dotnet-coverage output on disk unrewritten. The runner writes a transient derived coverage-settings file adjacent to the Cobertura output and deletes it in a finally block. Confirm no stranded derived settings file remains before checking this task off: `Test-Path 'docs\features\active\2026-09-02-test-determinism-and-hygiene-debt-729\evidence\baseline\coverage-baseline.cobertura.xml.effective-coverage.config'` returns `False`, and `Get-ChildItem -Path 'docs/features/active/2026-09-02-test-determinism-and-hygiene-debt-729/evidence/baseline' -Filter '*.effective-coverage.config'` returns zero items; if either shows the file present, delete it and repeat the confirmation. Acceptance: the artifact records a numeric `line-rate` value, a numeric `branch-rate` value, a `CoberturaProcessingState:` value of `processed` or `raw`, and the two stranded-settings observations above; and it records, per D9, the integer discovered-assembly count matching the `Discovered N test assemblies.` line, the boolean result that zero discovered paths contain a `\worktrees\` segment below the resolved search root, and the repository-relative form of each discovered assembly path with no absolute path written.
- [ ] [P0-T12] From `docs/features/active/2026-09-02-test-determinism-and-hygiene-debt-729/evidence/baseline/coverage-baseline.cobertura.xml`, aggregate across every `<class>` element whose `filename` attribute ends with `NonBlockingDelay.cs` the count of `<line>` children with a `hits` attribute greater than zero and the total count of `<line>` children, and record both integers in `docs/features/active/2026-09-02-test-determinism-and-hygiene-debt-729/evidence/baseline/nonblockingdelay-coverage-baseline.2026-09-02T10-30.md` as `BaselineCoveredLines:` and `BaselineTotalLines:`. Aggregate by filename rather than by class so any compiler-generated partitioning is summed. Acceptance: the artifact records both integers with `BaselineTotalLines:` greater than 0.
- [ ] [P0-T13] Re-verify each of these twelve anchors against the current tree and record the observed line number and observed text for each in `docs/features/active/2026-09-02-test-determinism-and-hygiene-debt-729/evidence/baseline/citation-verification.2026-09-02T10-30.md`: `TaskMaster/AppGlobals/NonBlockingDelay.cs` line 42; `TaskMaster/AppGlobals/NonBlockingDelay.cs` lines 52-54; `TaskMaster/AppGlobals/StoreRehookCoordinator.cs` line 102; `TaskMaster/AppGlobals/AppEvents.cs` line 456; `TaskMaster/TaskMaster.csproj` line 148; `TaskMaster.Test/packages.config` line 17; `TaskMaster.Test/TaskMaster.Test.csproj` line 73; `SVGControl.Test/SVGControl.Test.csproj` lines 55-66; `SVGControl.Test/SVGControl.Test.csproj` lines 86-91; `UtilitiesCS.Test/UtilitiesCS.Test.csproj` line 76; `UtilitiesCS.Test/Properties/AssemblyInfo.cs` lines 18-21; `UtilitiesCS.Test/OutlookObjects/Filter DASL/DASLFilterParserTests.cs` line 8. Acceptance: the artifact records twelve entries, each with an observed line number and the observed text at that line.
- [ ] [P0-T14] Compute the merge base with `$base = (git merge-base origin/main HEAD).Trim()` and record it in `docs/features/active/2026-09-02-test-determinism-and-hygiene-debt-729/evidence/baseline/base-ref.2026-09-02T10-30.md` under the field `BaseRef:`. Every later git-diff acceptance in this plan anchors to this ref via `$base`. Acceptance: the artifact records a 40-character hexadecimal `BaseRef:` value.
- [ ] [P0-T15] Record the pre-existing uncommitted worktree state by running `git status --porcelain` and writing every reported path to `docs/features/active/2026-09-02-test-determinism-and-hygiene-debt-729/evidence/baseline/preexisting-worktree-state.2026-09-02T10-30.md` under the field `PreExistingPaths:`, one path per line. These paths are not introduced by this plan and every later whole-worktree or `.claude`-scoped cleanliness assertion is evaluated against this recorded set rather than against the empty set. Acceptance: the artifact records a `PreExistingPaths:` section listing every path `git status --porcelain` reports at this point.

### Phase 1 — Finding 1: production `TimeProvider` seam on `NonBlockingDelay`

- [ ] [P1-T1] Replace the entire contents of `TaskMaster/AppGlobals/NonBlockingDelay.cs` with Block A. Acceptance: `Select-String -SimpleMatch 'public static Task WaitAsync(TimeSpan delay, TimeProvider timeProvider)' -Path 'TaskMaster/AppGlobals/NonBlockingDelay.cs'` returns exactly one match, `Select-String -SimpleMatch 'public static Task WaitAsync(TimeSpan delay)' -Path 'TaskMaster/AppGlobals/NonBlockingDelay.cs'` returns exactly one match, and `Select-String -SimpleMatch 'new Timer(' -Path 'TaskMaster/AppGlobals/NonBlockingDelay.cs'` returns zero matches.
- [ ] [P1-T2] Confirm the nullable pragma pair survived P1-T1. Acceptance: `Select-String -SimpleMatch '#nullable enable annotations' -Path 'TaskMaster/AppGlobals/NonBlockingDelay.cs'` returns exactly one match, `Select-String -SimpleMatch '#nullable restore annotations' -Path 'TaskMaster/AppGlobals/NonBlockingDelay.cs'` returns exactly one match, and the line between them is `            ITimer? timer = null;`.
- [ ] [P1-T3] Apply both insertions from Block C to `TaskMaster.Test/TaskMaster.Test.csproj`. Acceptance: `Select-String -SimpleMatch 'Microsoft.Bcl.TimeProvider.10.0.11' -Path 'TaskMaster.Test/TaskMaster.Test.csproj'` returns exactly one match and `Select-String -SimpleMatch 'Microsoft.Extensions.TimeProvider.Testing.10.9.0' -Path 'TaskMaster.Test/TaskMaster.Test.csproj'` returns exactly one match.
- [ ] [P1-T4] Apply both insertions from Block D to `TaskMaster.Test/packages.config`. Acceptance: `Select-String -SimpleMatch 'id="Microsoft.Bcl.TimeProvider"' -Path 'TaskMaster.Test/packages.config'` returns exactly one match and `Select-String -SimpleMatch 'id="Microsoft.Extensions.TimeProvider.Testing"' -Path 'TaskMaster.Test/packages.config'` returns exactly one match.
- [ ] [P1-T5] Confirm `TaskMaster.Test/app.config` was not modified by P1-T3 or P1-T4. Acceptance: `git diff --name-only $base HEAD -- TaskMaster.Test/app.config` and `git status --porcelain -- TaskMaster.Test/app.config` both return empty output, using the `$base` value recorded by P0-T14.
- [ ] [P1-T6] Re-run `pwsh -NoProfile -File .\scripts\vscode\Invoke-Restore.ps1` so the two newly declared packages are downloaded, and record `docs/features/active/2026-09-02-test-determinism-and-hygiene-debt-729/evidence/qa-gates/nuget-restore-after-package-edit.2026-09-02T10-30.md`. Acceptance: `EXIT_CODE: 0`, and `Test-Path .\packages\Microsoft.Extensions.TimeProvider.Testing.10.9.0\lib\net462\Microsoft.Extensions.TimeProvider.Testing.dll` returns `True`, pasted into `Output Summary:`.
- [ ] [P1-T7] Build the solution with the analyzer gate using the Block K prelude and the command `& $msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`, and record `docs/features/active/2026-09-02-test-determinism-and-hygiene-debt-729/evidence/qa-gates/seam-build-analyzers.2026-09-02T10-30.md`. Acceptance: `EXIT_CODE: 0`, and the artifact's `Output Summary:` states that the build log contains zero occurrences of `CS0123` and zero occurrences of `CS8632`.
- [ ] [P1-T8] Stage and commit the Phase 1 changes with `git add TaskMaster/AppGlobals/NonBlockingDelay.cs TaskMaster.Test/TaskMaster.Test.csproj TaskMaster.Test/packages.config` followed by a commit whose subject begins `fix(729): `. Acceptance: `git status --porcelain -- TaskMaster/AppGlobals/NonBlockingDelay.cs TaskMaster.Test/TaskMaster.Test.csproj TaskMaster.Test/packages.config` returns empty output.

### Phase 2 — Finding 1: deterministic `NonBlockingDelayTests` rewrite

- [ ] [P2-T1] Replace the entire contents of `TaskMaster.Test/AppGlobals/NonBlockingDelayTests.cs` with Block B. Acceptance: `Select-String -SimpleMatch 'Stopwatch' -Path 'TaskMaster.Test/AppGlobals/NonBlockingDelayTests.cs'` returns zero matches and `Select-String -SimpleMatch 'System.Diagnostics' -Path 'TaskMaster.Test/AppGlobals/NonBlockingDelayTests.cs'` returns zero matches.
- [ ] [P2-T2] Confirm the file declares exactly the three required test methods and retains both timeout guards. Acceptance: `Select-String -SimpleMatch 'WaitAsync_WithNoDispatcher_CompletesAfterInterval' -Path 'TaskMaster.Test/AppGlobals/NonBlockingDelayTests.cs'` returns exactly one match, the same for `WaitAsync_ZeroDelay_CompletesWithoutPump` and for `WaitAsync_SingleArgumentOverload_CompletesOnSystemTimeProvider`, and `Select-String -SimpleMatch '[Timeout(5000)]' -Path 'TaskMaster.Test/AppGlobals/NonBlockingDelayTests.cs'` returns exactly three matches.
- [ ] [P2-T3] Build the `TaskMaster.Test` project with the Block K prelude and `& $msbuild TaskMaster.Test\TaskMaster.Test.csproj /t:Rebuild /m /p:Configuration=Debug /p:Platform=AnyCPU`, and record `docs/features/active/2026-09-02-test-determinism-and-hygiene-debt-729/evidence/qa-gates/build-taskmaster-test.2026-09-02T10-30.md`. Acceptance: `EXIT_CODE: 0` and `Test-Path TaskMaster.Test\bin\Debug\TaskMaster.Test.dll` returns `True`, pasted into `Output Summary:`.
- [ ] [P2-T4] Run the three rewritten tests with the Block K prelude and `& $vstest TaskMaster.Test\bin\Debug\TaskMaster.Test.dll /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation /TestCaseFilter:"FullyQualifiedName~NonBlockingDelayTests"`, and record `docs/features/active/2026-09-02-test-determinism-and-hygiene-debt-729/evidence/regression-testing/nonblockingdelay-tests.2026-09-02T10-30.md`. Authorized mechanical retry branch: if `WaitAsync_ZeroDelay_CompletesWithoutPump` fails on its `waitTask.Status` assertion, change the single line `fakeTimeProvider.Advance(TimeSpan.Zero);` in that method to `fakeTimeProvider.Advance(TimeSpan.FromTicks(1));`, rebuild via P2-T3's command, re-run this command, and record both runs in the artifact. Acceptance: `EXIT_CODE: 0`, and the artifact records `PassedCount: 3` and `FailedCount: 0` with the three test-method names listed.
- [ ] [P2-T5] Write `docs/features/active/2026-09-02-test-determinism-and-hygiene-debt-729/evidence/other/no-fail-before-rationale.2026-09-02T10-30.md` recording, as an auditable negative claim, that no fail-before run is claimed for Finding 1 or for the `UtilitiesCS.Test` guard, with `SearchScope:` naming `docs/features/active/2026-09-02-test-determinism-and-hygiene-debt-729/evidence/regression-testing/`, `SearchPatterns:` naming `fail-before-exception.*.md`, `SearchResult:` naming what was found, and a `WhyFailingRunImpossible:` section stating (a) for Finding 1, that the replacement tests reference the 2-arg overload and therefore cannot compile against the pre-change production file, so a red-before state is a compile error rather than a test failure, and (b) for `UtilitiesCS.Test`, that the guard is green from birth because no `Form`-derived type has ever been compiled into that assembly. Acceptance: the artifact exists and contains the literal field names `SearchScope:`, `SearchPatterns:`, `SearchResult:`, and `WhyFailingRunImpossible:`.
- [ ] [P2-T6] Stage and commit with `git add TaskMaster.Test/AppGlobals/NonBlockingDelayTests.cs` plus the Phase 2 evidence paths, using a commit subject beginning `test(729): `. Acceptance: `git status --porcelain -- TaskMaster.Test/AppGlobals/NonBlockingDelayTests.cs` returns empty output.

### Phase 3 — Finding 2: `SVGControl.Test` red-before guard, deletions, green-after

- [ ] [P3-T1] Create `SVGControl.Test/NoLiveFormInTestAssemblyTests.cs` with the exact contents of Block E. Acceptance: `Select-String -SimpleMatch 'namespace SVGControl.Test' -Path 'SVGControl.Test/NoLiveFormInTestAssemblyTests.cs'` returns exactly one match and `Select-String -SimpleMatch 'ReflectionTypeLoadException' -Path 'SVGControl.Test/NoLiveFormInTestAssemblyTests.cs'` returns exactly one match.
- [ ] [P3-T2] Insert the Block G compile entry into `SVGControl.Test/SVGControl.Test.csproj` immediately before the existing `<Compile Include="GetRelativePath_Test.cs" />` line. Acceptance: `Select-String -SimpleMatch '<Compile Include="NoLiveFormInTestAssemblyTests.cs" />' -Path 'SVGControl.Test/SVGControl.Test.csproj'` returns exactly one match.
- [ ] [P3-T3] Build the `SVGControl.Test` project with the Block K prelude and `& $msbuild SVGControl.Test\SVGControl.Test.csproj /t:Rebuild /m /p:Configuration=Debug /p:Platform=AnyCPU`, and record `docs/features/active/2026-09-02-test-determinism-and-hygiene-debt-729/evidence/regression-testing/svgcontrol-build-before.2026-09-02T10-30.md`. Acceptance: `EXIT_CODE: 0` and `Test-Path SVGControl.Test\bin\Debug\SVGControl.Test.dll` returns `True`, pasted into `Output Summary:`.
- [ ] [P3-T4] [expect-fail] Run the guard against the pre-deletion assembly with the Block K prelude and `& $vstest SVGControl.Test\bin\Debug\SVGControl.Test.dll /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation /TestCaseFilter:"FullyQualifiedName~NoLiveFormInTestAssemblyTests"`, and record `docs/features/active/2026-09-02-test-determinism-and-hygiene-debt-729/evidence/regression-testing/svgcontrol-guard-fail-before.2026-09-02T10-30.md` with `ExpectedExitCode: 1`. Acceptance: the artifact records `PassedCount: 0` and `FailedCount: 1`, and the pasted failure text contains the token `SVGControl.Test.Form1` and the token `SVGControl.Test.Form2`.
- [ ] [P3-T5] Apply the Block H removals to `SVGControl.Test/SVGControl.Test.csproj`. Acceptance: `Select-String -SimpleMatch 'Form1.cs' -Path 'SVGControl.Test/SVGControl.Test.csproj'` returns zero matches, and the same command returns zero matches for each of `Form1.Designer.cs`, `Form1.resx`, `Form2.cs`, `Form2.Designer.cs`, and `Form2.resx`.
- [ ] [P3-T6] Delete the six form sources with `git rm -f SVGControl.Test/Form1.cs SVGControl.Test/Form1.Designer.cs SVGControl.Test/Form1.resx SVGControl.Test/Form2.cs SVGControl.Test/Form2.Designer.cs SVGControl.Test/Form2.resx`. Acceptance: `Test-Path` returns `False` for all six paths and `git ls-files SVGControl.Test/Form1.cs SVGControl.Test/Form1.Designer.cs SVGControl.Test/Form1.resx SVGControl.Test/Form2.cs SVGControl.Test/Form2.Designer.cs SVGControl.Test/Form2.resx` returns empty output.
- [ ] [P3-T7] Rebuild the `SVGControl.Test` project with the P3-T3 command and record `docs/features/active/2026-09-02-test-determinism-and-hygiene-debt-729/evidence/regression-testing/svgcontrol-build-after.2026-09-02T10-30.md`. Acceptance: `EXIT_CODE: 0` and the artifact's `Output Summary:` states the build log contains zero occurrences of `CS2001`.
- [ ] [P3-T8] Re-run the guard with the P3-T4 command and record `docs/features/active/2026-09-02-test-determinism-and-hygiene-debt-729/evidence/regression-testing/svgcontrol-guard-pass-after.2026-09-02T10-30.md`. Acceptance: `EXIT_CODE: 0`, and the artifact records `PassedCount: 1` and `FailedCount: 0` for the test node `SVGControl.Test.NoLiveFormInTestAssemblyTests.ExecutingAssembly_ContainsNoFormDerivedType`.
- [ ] [P3-T9] Stage and commit with `git add SVGControl.Test/NoLiveFormInTestAssemblyTests.cs SVGControl.Test/SVGControl.Test.csproj` plus the recorded deletions and the Phase 3 evidence paths, using a commit subject beginning `test(729): `. Acceptance: `git status --porcelain -- SVGControl.Test` returns empty output.

### Phase 4 — Finding 2: `UtilitiesCS.Test` orphan removal and prevention guard

- [ ] [P4-T1] Confirm, before deleting anything, that `UtilitiesCS.Test/UtilitiesCS.Test.csproj` references none of the ten orphan files. Acceptance: `Select-String -SimpleMatch 'ResourceTests.cs' -Path 'UtilitiesCS.Test/UtilitiesCS.Test.csproj'` returns zero matches, and the same command returns zero matches for each of `Form1.cs`, `Form1.Designer.cs`, `Form1.resx`, `Form2.cs`, `Form2.Designer.cs`, `Form2.resx`, `Form3.cs`, `Form3.Designer.cs`, and `Form3.resx`.
- [ ] [P4-T2] Delete the ten orphan files with `git rm -f UtilitiesCS.Test/ResourceTests.cs UtilitiesCS.Test/Form1.cs UtilitiesCS.Test/Form1.Designer.cs UtilitiesCS.Test/Form1.resx UtilitiesCS.Test/Form2.cs UtilitiesCS.Test/Form2.Designer.cs UtilitiesCS.Test/Form2.resx UtilitiesCS.Test/Form3.cs UtilitiesCS.Test/Form3.Designer.cs UtilitiesCS.Test/Form3.resx`. Acceptance: `Test-Path` returns `False` for all ten paths and `git ls-files UtilitiesCS.Test/ResourceTests.cs UtilitiesCS.Test/Form1.cs UtilitiesCS.Test/Form1.Designer.cs UtilitiesCS.Test/Form1.resx UtilitiesCS.Test/Form2.cs UtilitiesCS.Test/Form2.Designer.cs UtilitiesCS.Test/Form2.resx UtilitiesCS.Test/Form3.cs UtilitiesCS.Test/Form3.Designer.cs UtilitiesCS.Test/Form3.resx` returns empty output.
- [ ] [P4-T3] Create `UtilitiesCS.Test/NoLiveFormInTestAssemblyTests.cs` with the exact contents of Block F. Acceptance: `Select-String -SimpleMatch 'namespace UtilitiesCS.Test' -Path 'UtilitiesCS.Test/NoLiveFormInTestAssemblyTests.cs'` returns exactly one match and `Select-String -SimpleMatch 'ReflectionTypeLoadException' -Path 'UtilitiesCS.Test/NoLiveFormInTestAssemblyTests.cs'` returns exactly one match.
- [ ] [P4-T4] Insert the Block G compile entry into `UtilitiesCS.Test/UtilitiesCS.Test.csproj` immediately after the existing `<Compile Include="TestAssemblyInitializer.cs" />` line. Acceptance: `Select-String -SimpleMatch '<Compile Include="NoLiveFormInTestAssemblyTests.cs" />' -Path 'UtilitiesCS.Test/UtilitiesCS.Test.csproj'` returns exactly one match.
- [ ] [P4-T5] Build the `UtilitiesCS.Test` project with the Block K prelude and `& $msbuild UtilitiesCS.Test\UtilitiesCS.Test.csproj /t:Rebuild /m /p:Configuration=Debug /p:Platform=AnyCPU`, and record `docs/features/active/2026-09-02-test-determinism-and-hygiene-debt-729/evidence/regression-testing/utilitiescs-build.2026-09-02T10-30.md`. Acceptance: `EXIT_CODE: 0` and `Test-Path UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll` returns `True`, pasted into `Output Summary:`.
- [ ] [P4-T6] Run the new guard with the Block K prelude and `& $vstest UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation /TestCaseFilter:"FullyQualifiedName~NoLiveFormInTestAssemblyTests"`, and record `docs/features/active/2026-09-02-test-determinism-and-hygiene-debt-729/evidence/regression-testing/utilitiescs-guard-pass.2026-09-02T10-30.md`. The artifact must state in `Output Summary:` that this guard is green from birth and is regression prevention, not a fail-before/pass-after regression test, because `UtilitiesCS.Test` compiles zero `Form`-derived types today. Acceptance: `EXIT_CODE: 0`, the artifact records `PassedCount: 1` and `FailedCount: 0` for the test node `UtilitiesCS.Test.NoLiveFormInTestAssemblyTests.ExecutingAssembly_ContainsNoFormDerivedType`, and the artifact contains the literal token `green-from-birth`.
- [ ] [P4-T7] Stage and commit with `git add UtilitiesCS.Test/NoLiveFormInTestAssemblyTests.cs UtilitiesCS.Test/UtilitiesCS.Test.csproj` plus the recorded deletions and the Phase 4 evidence paths, using a commit subject beginning `test(729): `. Acceptance: `git status --porcelain -- UtilitiesCS.Test` returns empty output.

### Phase 5 — Finding 3: parallel-execution hazard marking and orphan duplicate removal

- [ ] [P5-T1] Insert Block I immediately above the existing `[TestClass]` attribute line in `UtilitiesCS.Test/OutlookObjects/Filter DASL/DASLFilterParserTests.cs`, changing nothing else in the file. Acceptance: `Select-String -SimpleMatch '[DoNotParallelize]' -Path 'UtilitiesCS.Test/OutlookObjects/Filter DASL/DASLFilterParserTests.cs'` returns exactly one match, `Select-String -SimpleMatch 'AssemblyInfo.cs' -Path 'UtilitiesCS.Test/OutlookObjects/Filter DASL/DASLFilterParserTests.cs'` returns exactly one match, and `Select-String -SimpleMatch 'TaskMaster.runsettings' -Path 'UtilitiesCS.Test/OutlookObjects/Filter DASL/DASLFilterParserTests.cs'` returns exactly one match on the line stating that the runsettings file is not what takes effect.
- [ ] [P5-T2] Insert Block J immediately above the existing `[TestClass]` attribute line in `UtilitiesCS.Test/ReusableTypeClasses/StackGeek_Tests.cs`, changing nothing else in the file. Acceptance: `Select-String -SimpleMatch '[DoNotParallelize]' -Path 'UtilitiesCS.Test/ReusableTypeClasses/StackGeek_Tests.cs'` returns exactly one match and `Select-String -SimpleMatch 'AssemblyInfo.cs' -Path 'UtilitiesCS.Test/ReusableTypeClasses/StackGeek_Tests.cs'` returns exactly one match.
- [ ] [P5-T3] Confirm that P5-T1 and P5-T2 changed no test body, assertion, or test-method name. Acceptance: `git diff --unified=0 $base -- 'UtilitiesCS.Test/OutlookObjects/Filter DASL/DASLFilterParserTests.cs' 'UtilitiesCS.Test/ReusableTypeClasses/StackGeek_Tests.cs'` (anchored to the P0-T14 base ref and therefore covering both committed and uncommitted state at this point in the plan) shows only added lines and zero removed lines, and the added lines consist solely of comment lines and `[DoNotParallelize]` lines; paste the full diff into `docs/features/active/2026-09-02-test-determinism-and-hygiene-debt-729/evidence/regression-testing/donotparallelize-diff.2026-09-02T10-30.md`.
- [ ] [P5-T4] Confirm `UtilitiesCS.Test/UtilitiesCS.Test.csproj` does not reference the orphan duplicate. Acceptance: `Select-String -SimpleMatch 'DASLFilterParser_Tests.cs' -Path 'UtilitiesCS.Test/UtilitiesCS.Test.csproj'` returns zero matches.
- [ ] [P5-T5] Delete the orphan duplicate with `git rm -f UtilitiesCS.Test/OutlookObjects/DASLFilterParser_Tests.cs`. Acceptance: `Test-Path UtilitiesCS.Test/OutlookObjects/DASLFilterParser_Tests.cs` returns `False` and `git ls-files UtilitiesCS.Test/OutlookObjects/DASLFilterParser_Tests.cs` returns empty output.
- [ ] [P5-T6] Rebuild `UtilitiesCS.Test` with the P4-T5 command, then run both marked classes with the Block K prelude and `& $vstest UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation /TestCaseFilter:"FullyQualifiedName~DASLFilterParserTests|FullyQualifiedName~StackGeek_Tests"`, and record `docs/features/active/2026-09-02-test-determinism-and-hygiene-debt-729/evidence/regression-testing/donotparallelize-classes.2026-09-02T10-30.md`. Acceptance: `EXIT_CODE: 0`, and the artifact records `FailedCount: 0` together with a `PassedCount:` integer greater than 0.
- [ ] [P5-T7] Write the fail-before exception dossier `docs/features/active/2026-09-02-test-determinism-and-hygiene-debt-729/evidence/regression-testing/fail-before-exception.2026-09-02T10-30.md` containing `Timestamp:`, a `WhyFailingRunImpossible:` section stating that the failure requires a specific interleaving of `Console.SetOut` across two threads and is therefore a race with no deterministic red run, and an alternative-proof section citing the two in-repo precedent classes `UtilitiesCS.Test/HelperClasses/PrettyPrint_Tests.cs` lines 14-20 and `UtilitiesCS.Test/OutlookObjects/Table/OlTableExtensions_Tests.cs` lines 17-21 with their comment text quoted. Acceptance: the artifact exists and contains the literal field `WhyFailingRunImpossible:` plus both precedent file paths.
- [ ] [P5-T8] Stage and commit with `git add 'UtilitiesCS.Test/OutlookObjects/Filter DASL/DASLFilterParserTests.cs' UtilitiesCS.Test/ReusableTypeClasses/StackGeek_Tests.cs` plus the recorded deletion and the Phase 5 evidence paths, using a commit subject beginning `test(729): `. Acceptance: `git status --porcelain -- UtilitiesCS.Test` returns empty output.

### Phase 6 — Final QA loop and coverage verification

- [ ] [P6-T1] Capture the SHA-256 of each of the seven plan-owned formattable files with `Get-FileHash -Algorithm SHA256`, run the scope-locked mutating format pass `dotnet tool run csharpier format 'TaskMaster/AppGlobals/NonBlockingDelay.cs' 'TaskMaster.Test/AppGlobals/NonBlockingDelayTests.cs' 'TaskMaster.Test/packages.config' 'UtilitiesCS.Test/NoLiveFormInTestAssemblyTests.cs' 'UtilitiesCS.Test/OutlookObjects/Filter DASL/DASLFilterParserTests.cs' 'UtilitiesCS.Test/ReusableTypeClasses/StackGeek_Tests.cs' 'SVGControl.Test/NoLiveFormInTestAssemblyTests.cs'`, capture the seven hashes again, and record all fourteen hashes plus the derived rewritten-file count in `docs/features/active/2026-09-02-test-determinism-and-hygiene-debt-729/evidence/qa-gates/csharpier-format.2026-09-02T10-30.md`. The console line `Formatted N files` is a processed-file count and must NOT be used as the rewritten-file count. A repo-wide `dotnet tool run csharpier format .` is prohibited by D4. Acceptance: `EXIT_CODE: 0` and the artifact records fourteen hash values and a `RewrittenFileCount:` integer.
- [ ] [P6-T2] Run the read-only gate `dotnet tool run csharpier check .` and record `docs/features/active/2026-09-02-test-determinism-and-hygiene-debt-729/evidence/qa-gates/csharpier-check.2026-09-02T10-30.md`. Authorized mechanical branch: if the reported set contains `TaskMaster.Test/packages.config` (which CSharpier processes by filename and which a per-file invocation may skip), run `dotnet tool run csharpier format TaskMaster.Test` in directory form, re-run this check, and record both runs in the artifact. That directory-form invocation is broader than the D4 scope lock, so immediately after it run `git status --porcelain -- TaskMaster.Test` and paste the result into the artifact; every path it reports must already be one of the seven plan-owned formattable paths, and any other path it reports must be reverted to its committed state by running `git checkout HEAD --` applied to that path, with the restoration recorded, before this task is checked off. Acceptance: either `EXIT_CODE: 0`, or the final reported unformatted set is a subset of the `Baseline unformatted set:` recorded by P0-T8 after removing the seventeen paths deleted by Phases 3-5, and contains none of the seven plan-owned formattable paths and none of `UtilitiesCS/OutlookObjects/Filter DASL/DASLFilterParser.cs`; the artifact states which of the two outcomes held, lists the reported set verbatim, and lists the subset derivation it performed.
- [ ] [P6-T3] Run `& $msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` using the Block K prelude and record `docs/features/active/2026-09-02-test-determinism-and-hygiene-debt-729/evidence/qa-gates/msbuild-analyzers.2026-09-02T10-30.md`. Acceptance: `EXIT_CODE: 0` and the artifact's `Output Summary:` states the diagnostic count is no higher than the count recorded by P0-T9.
- [ ] [P6-T4] Run `& $msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true` using the Block K prelude and record `docs/features/active/2026-09-02-test-determinism-and-hygiene-debt-729/evidence/qa-gates/msbuild-nullable.2026-09-02T10-30.md`. Do not add `/p:Nullable=enable`; do not substitute `/t:Build`. Acceptance: `EXIT_CODE: 0` and the artifact's `Output Summary:` states the log contains zero occurrences of `CS8632`.
- [ ] [P6-T5] Run `pwsh -NoProfile -File .\scripts\vscode\Invoke-MSTestWithCoverage.ps1 -SearchRoot . -Configuration Debug -CoverageOutput 'docs\features\active\2026-09-02-test-determinism-and-hygiene-debt-729\evidence\qa-gates\coverage-final.cobertura.xml'` and record `docs/features/active/2026-09-02-test-determinism-and-hygiene-debt-729/evidence/qa-gates/mstest-coverage.2026-09-02T10-30.md`, pasting into `Output Summary:` the `Discovered N test assemblies.` line, the vstest total/passed/failed counts, and the `line-rate` and `branch-rate` attribute values read from the `<coverage>` root element of `docs/features/active/2026-09-02-test-determinism-and-hygiene-debt-729/evidence/qa-gates/coverage-final.cobertura.xml`. Authorized non-zero branch: if the script exits non-zero and its error text contains the literal `is below the required 80% threshold`, record that observed exit code together with the line `ExpectedExitCode: 1` and the literal note `Threshold assertion fired; raw Cobertura was written before the assertion and the numeric rates below were read from it.` Authorized mechanical re-run branch: if the run reports failures and every failing test node is in `QuickFiler.Test` and is one of the pump-hosted classes tracked by issue #743, re-run this exact command once, record both runs and the full list of failing test node identifiers in the artifact, and treat the second run's counts as authoritative. If a #743-tracked failure persists after the second run, record it in the artifact and in the delivery-record task as a known out-of-scope flake. A failure in any other assembly is not covered by this branch. Record the field `CoberturaProcessingState:` with the value `processed` when the script exited 0, and the value `raw` when the script exited non-zero under either authorized branch above; the value is `raw` in those cases because `scripts/vscode/Invoke-MSTestWithCoverage.ps1` writes the post-processed XML at line 343 only after both the collection call at line 326 and the threshold assertion at line 341 have returned, so a throw at either point leaves the dotnet-coverage output on disk unrewritten. The runner writes a transient derived coverage-settings file adjacent to the Cobertura output and deletes it in a finally block. Confirm no stranded derived settings file remains before checking this task off: `Test-Path 'docs\features\active\2026-09-02-test-determinism-and-hygiene-debt-729\evidence\qa-gates\coverage-final.cobertura.xml.effective-coverage.config'` returns `False`, and `Get-ChildItem -Path 'docs/features/active/2026-09-02-test-determinism-and-hygiene-debt-729/evidence/qa-gates' -Filter '*.effective-coverage.config'` returns zero items; if either shows the file present, delete it and repeat the confirmation. Then, because no logger is configured for the runner's inner vstest invocation (`scripts/vscode/Invoke-MSTestWithCoverage.ps1` line 76 passes only `/Settings:`, `/InIsolation`, and `/TestCaseFilter:`) and the default console logger does not enumerate passing test nodes, obtain the two per-node outcomes from two immediately following scoped confirmation runs using the Block K prelude: `& $vstest UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation /TestCaseFilter:"FullyQualifiedName~NoLiveFormInTestAssemblyTests" /Logger:"trx;LogFileName=utilitiescs-noliveform.trx" /ResultsDirectory:coverage\trx\p6t5` and `& $vstest SVGControl.Test\bin\Debug\SVGControl.Test.dll /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation /TestCaseFilter:"FullyQualifiedName~NoLiveFormInTestAssemblyTests" /Logger:"trx;LogFileName=svgcontrol-noliveform.trx" /ResultsDirectory:coverage\trx\p6t5`. Each TRX contains exactly one `<UnitTestResult>` element; read its `outcome` attribute and pair it with the fully-qualified identifier of the assembly that produced that TRX. Transcribe into `Output Summary:` two lines of the form `<fully-qualified identifier> outcome="Passed"`, one per run, plus each run's exit code. Two single-assembly runs are used rather than one combined run because both assemblies declare a class named `NoLiveFormInTestAssemblyTests` with a method named `ExecutingAssembly_ContainsNoFormDerivedType`, and a TRX `<UnitTestResult>` element carries only the bare method name, so a combined TRX cannot distinguish the two nodes without a `<TestMethod className=...>` join. Transcribe only the derived fully-qualified test-node identifier and its `outcome` value. Do not paste a raw `<UnitTestResult>` element, which carries a `computerName` host identifier. `coverage/` is gitignored at `.gitignore` line 144, so both TRX files leave the tracked tree clean. This task runs three commands; the artifact's single `Command:` and `EXIT_CODE:` fields carry the `Invoke-MSTestWithCoverage.ps1` run, because that is the run the coverage gate and any `ExpectedExitCode:` apply to, and the two scoped confirmation commands with their own exit codes are recorded inside `Output Summary:`. Acceptance: the artifact records `FailedCount: 0` — or, under the #743 branch above, a `FailedCount:` whose every failing test node identifier is listed in the artifact and is in `QuickFiler.Test` — plus a `PassedCount:` integer greater than 0, a numeric `line-rate`, a numeric `branch-rate`, a `CoberturaProcessingState:` value of `processed` or `raw`, the two stranded-settings observations above, and both literal tokens `UtilitiesCS.Test.NoLiveFormInTestAssemblyTests.ExecutingAssembly_ContainsNoFormDerivedType` and `SVGControl.Test.NoLiveFormInTestAssemblyTests.ExecutingAssembly_ContainsNoFormDerivedType` each recorded with `outcome="Passed"`; and the artifact records, per D9, the integer discovered-assembly count matching the `Discovered N test assemblies.` line, the boolean result that zero discovered paths contain a `\worktrees\` segment below the resolved search root, and the repository-relative form of each discovered assembly path with no absolute path written.
- [ ] [P6-T6] Compare coverage against the Phase 0 baseline and record `docs/features/active/2026-09-02-test-determinism-and-hygiene-debt-729/evidence/qa-gates/coverage-delta.2026-09-02T10-30.md` with the fields `BaselineLineRate:`, `PostChangeLineRate:`, `BaselineBranchRate:`, `PostChangeBranchRate:`, `BaselineCoveredLines:`, `PostChangeCoveredLines:`, `BaselineTotalLines:`, `PostChangeTotalLines:`, where the four line-count fields aggregate every `<class>` element whose `filename` attribute ends with `NonBlockingDelay.cs`. Before reading any value, compare the `CoberturaProcessingState:` recorded by P0-T11 against the one recorded by P6-T5: both artifacts must declare the same `CoberturaProcessingState:`; if they differ, dot-source `scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1`, call `ConvertTo-KoverageCoberturaXml` with `-RepoRoot (Resolve-Path .).Path` and with `-XmlContent` set to the raw side's content read via `Get-Content -Raw -Encoding UTF8` from `docs\features\active\2026-09-02-test-determinism-and-hygiene-debt-729\evidence\baseline\coverage-baseline.cobertura.xml` when the baseline is the raw side, or from `docs\features\active\2026-09-02-test-determinism-and-hygiene-debt-729\evidence\qa-gates\coverage-final.cobertura.xml` when the post-change run is the raw side, and read the four rate fields and both `NonBlockingDelay.cs` line counts for that side from the converted string rather than from the file on disk, recording in this artifact that this conversion was performed and which side it was applied to. Comparing a raw Cobertura against a processed one is prohibited because `ConvertTo-KoverageCoberturaXml` removes non-allowlisted packages and then rewrites the root `line-rate`, `branch-rate`, `lines-covered`, `lines-valid`, `branches-covered`, and `branches-valid` attributes (`scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1` lines 417-447), so the two forms are not on the same denominator. Acceptance: the artifact records both `CoberturaProcessingState:` values and states whether a conversion was performed; `PostChangeTotalLines` is greater than 0; `PostChangeLineRate` is greater than or equal to `BaselineLineRate`, `PostChangeBranchRate` is greater than or equal to `BaselineBranchRate`, and `PostChangeCoveredLines` divided by `PostChangeTotalLines` is greater than or equal to `BaselineCoveredLines` divided by `BaselineTotalLines`; all eight values and the two comparisons are written into the artifact. `BaselineTotalLines` is already required to be greater than 0 by P0-T12, so both ratios have a non-zero denominator.
- [ ] [P6-T7] Audit file length for the six plan-owned `.cs` files after formatting and record `docs/features/active/2026-09-02-test-determinism-and-hygiene-debt-729/evidence/qa-gates/file-size-audit.2026-09-02T10-30.md` with one line count per file for `TaskMaster/AppGlobals/NonBlockingDelay.cs`, `TaskMaster.Test/AppGlobals/NonBlockingDelayTests.cs`, `UtilitiesCS.Test/NoLiveFormInTestAssemblyTests.cs`, `SVGControl.Test/NoLiveFormInTestAssemblyTests.cs`, `UtilitiesCS.Test/ReusableTypeClasses/StackGeek_Tests.cs`, and `UtilitiesCS.Test/OutlookObjects/Filter DASL/DASLFilterParserTests.cs`. Acceptance: the artifact records six counts and every recorded count is less than or equal to 500.
- [ ] [P6-T8] Record the single-pass toolchain result in `docs/features/active/2026-09-02-test-determinism-and-hygiene-debt-729/evidence/qa-gates/toolchain-single-pass.2026-09-02T10-30.md`, listing the five commands from P6-T1 through P6-T5 in order with their exit codes. If P6-T1 reported a `RewrittenFileCount:` greater than 0, or if any of P6-T2 through P6-T5 did not meet its acceptance, re-execute P6-T1 through P6-T5 in order and record the final pass in this same artifact. A P6-T5 artifact recording `ExpectedExitCode: 1` with the literal threshold message and `FailedCount: 0` satisfies this task. A P6-T2 artifact whose final reported unformatted set is a subset of the `Baseline unformatted set:` recorded by P0-T8 after removing the seventeen paths deleted by Phases 3-5, and contains none of the seven plan-owned formattable paths and none of `UtilitiesCS/OutlookObjects/Filter DASL/DASLFilterParser.cs`, also satisfies this task. Acceptance: the artifact records a final pass in which `RewrittenFileCount:` is 0 and P6-T2 through P6-T5 each met their acceptance, counting the two allowances stated above as met.
- [ ] [P6-T9] Stage and commit all Phase 6 evidence together with any rewrite the P6-T1 scope-locked formatter pass applied to the seven plan-owned formattable source paths, which are committed by Phases 1 through 5 and are therefore not staged by any earlier task. Run `git add docs/features/active/2026-09-02-test-determinism-and-hygiene-debt-729/evidence/qa-gates 'TaskMaster/AppGlobals/NonBlockingDelay.cs' 'TaskMaster.Test/AppGlobals/NonBlockingDelayTests.cs' 'TaskMaster.Test/packages.config' 'UtilitiesCS.Test/NoLiveFormInTestAssemblyTests.cs' 'UtilitiesCS.Test/OutlookObjects/Filter DASL/DASLFilterParserTests.cs' 'UtilitiesCS.Test/ReusableTypeClasses/StackGeek_Tests.cs' 'SVGControl.Test/NoLiveFormInTestAssemblyTests.cs'` and then commit with a subject beginning `chore(729): `. Acceptance: `git status --porcelain` returns empty output for `docs/features/active/2026-09-02-test-determinism-and-hygiene-debt-729/evidence/qa-gates` and for each of the seven plan-owned formattable paths.

### Phase 7 — Scope-boundary verification and delivery record

- [ ] [P7-T1] Verify the Finding 3 production file was not touched. Run `git diff --name-only $base HEAD -- 'UtilitiesCS/OutlookObjects/Filter DASL/DASLFilterParser.cs'` and `git status --porcelain -- 'UtilitiesCS/OutlookObjects/Filter DASL/DASLFilterParser.cs'`, and record both outputs in `docs/features/active/2026-09-02-test-determinism-and-hygiene-debt-729/evidence/qa-gates/scope-boundary-ac17.2026-09-02T10-30.md`. Acceptance: both commands return empty output, and `Select-String -SimpleMatch 'TextWriter' -Path 'UtilitiesCS/OutlookObjects/Filter DASL/DASLFilterParser.cs'` returns zero matches.
- [ ] [P7-T2] Verify that `TaskMaster/AppGlobals/NonBlockingDelay.cs` is the only non-test production source file changed. Run `git diff --name-status $base HEAD` and `git status --porcelain`, and record the union of both outputs in `docs/features/active/2026-09-02-test-determinism-and-hygiene-debt-729/evidence/qa-gates/scope-boundary-ac18.2026-09-02T10-30.md`, classifying every path into exactly one of four buckets: production source; test project asset; feature documentation and evidence, which for this purpose includes the follow-up promotion record `docs/features/potential/promoted/2026-09-02-quickfiler-itemviewer-ui-marshalling-seam.md` that P8-T22 commits as deliberate output of this work; or agent-memory scratch, pre-existing or newly written by delegated agents during this plan's execution. The fourth bucket exists because `.claude/agent-memory/` is a tracked directory whose contents are written by the persistent-memory system of delegated agents rather than by any task in this plan, and because P0-T15 records that some such paths are already dirty before Phase 1 begins. A path may be placed in the fourth bucket only if it is under `.claude/agent-memory/` or is listed in the P0-T15 `PreExistingPaths:` set; `docs/features/potential/promoted/2026-09-02-quickfiler-itemviewer-ui-marshalling-seam.md` is excluded from the fourth bucket and belongs in the third even though P0-T15 lists it. For the same reason, every path under `docs/features/active/2026-09-02-test-determinism-and-hygiene-debt-729` belongs in the third bucket even though P0-T15 lists the untracked feature folder: the P0-T15 clause admits a path to the fourth bucket, it does not compel one, and the fourth bucket's subject is agent-memory scratch. Acceptance: exactly one path is classified as production source and it is `TaskMaster/AppGlobals/NonBlockingDelay.cs`; every other path is assigned to one of the remaining three buckets and each fourth-bucket path names which of the two allowances covers it.
- [ ] [P7-T3] Verify no `QuickFiler/` path changed. Run `git diff --name-only $base HEAD -- QuickFiler` and `git status --porcelain -- QuickFiler`, and record both outputs in `docs/features/active/2026-09-02-test-determinism-and-hygiene-debt-729/evidence/qa-gates/scope-boundary-ac19.2026-09-02T10-30.md`. Acceptance: both commands return empty output.
- [ ] [P7-T4] Verify no push-down-owned path changed. Run `git diff --name-only $base HEAD -- .claude .codex .agents config/blast-radius.json config/orchestration-routing.json` and `git status --porcelain -- .claude .codex .agents config/blast-radius.json config/orchestration-routing.json`, and record both outputs in `docs/features/active/2026-09-02-test-determinism-and-hygiene-debt-729/evidence/qa-gates/scope-boundary-ac20.2026-09-02T10-30.md`. Acceptance: `git diff --name-only $base HEAD -- .claude .codex .agents config/blast-radius.json config/orchestration-routing.json` returns empty output, and every path reported by `git status --porcelain -- .claude .codex .agents config/blast-radius.json config/orchestration-routing.json` is either listed in the P0-T15 `PreExistingPaths:` set or is under `.claude/agent-memory/`, which holds per-agent scratch memory rather than repository policy or configuration content; the artifact enumerates each such path and states which of the two allowances covers it.
- [ ] [P7-T5] Write the changed-file inventory `docs/features/active/2026-09-02-test-determinism-and-hygiene-debt-729/evidence/other/changed-file-inventory.2026-09-02T10-30.md` from `git diff --name-status $base HEAD` plus `git status --porcelain`, and compare it line by line against the "Complete file-write inventory" section of this plan. Four differences are known in advance and must be named explicitly in `Deltas:` rather than left unexplained: the `spec.md` Block L insertion authored by P7-T7 and the Phase 8 acceptance-criteria checkbox edits, both of which occur after this task and therefore cannot appear in its diff; and `docs/features/active/2026-09-02-test-determinism-and-hygiene-debt-729/issue.md` together with `docs/features/active/2026-09-02-test-determinism-and-hygiene-debt-729/research/research-729.2026-09-02T09-30.md`, which are authored before this plan, are still untracked at this point, and are committed by P8-T22's directory-level `git add` rather than by any earlier phase commit. Acceptance: the artifact records a `Deltas:` section that is either empty or explains every difference, and that names those two pending `spec.md` changes when the diff does not yet show them and those two untracked feature-documentation paths; the inventory contains exactly seventeen `D` entries.
- [ ] [P7-T6] Write the delivery record `docs/features/active/2026-09-02-test-determinism-and-hygiene-debt-729/evidence/other/delivery-record.2026-09-02T10-30.md` stating (a) that `UtilitiesCS.Test/NoLiveFormInTestAssemblyTests.cs` is green from birth and is regression prevention rather than a fail-before/pass-after regression test, so no reviewer should expect a red run for it; (b) that `SVGControl.Test/NoLiveFormInTestAssemblyTests.cs` is the genuine red-before/green-after regression test, with the two evidence paths from P3-T4 and P3-T8; (c) the five final toolchain commands with their exit codes from Phase 6; (d) that Finding 4 is out of scope and carried by issue #743; and (e) a section headed `Known out-of-scope flakes:` that either states `None observed` or lists every failing `QuickFiler.Test` test node identifier recorded by the P6-T5 #743 re-run branch. Acceptance: the artifact contains the literal tokens `green-from-birth`, `#743`, `Known out-of-scope flakes:`, and both Phase 3 evidence filenames.
- [ ] [P7-T7] Insert Block L into `docs/features/active/2026-09-02-test-determinism-and-hygiene-debt-729/spec.md` immediately after the Finding 4 out-of-scope bullet's final sentence and before the next top-level bullet, indenting the heading line and each of the four numbered lines by exactly two spaces so they render as a continuation of that bullet. The Finding 4 bullet is the single line beginning `- **Finding 4 — pump-hosted` in the `### Out of scope / non-goals` section. AC16 requires the four verified reasons to be recorded in the spec itself; today that bullet cites `four independent reasons, §4.2` without enumerating them, so this insertion is what makes AC16 true. Change nothing else in `spec.md` in this task. Acceptance: `Select-String -SimpleMatch 'Finding 4 — reasons no test-only fix exists:' -Path 'docs/features/active/2026-09-02-test-determinism-and-hygiene-debt-729/spec.md'` returns exactly one match; `Select-String -SimpleMatch 'The production code reads the context off the control, not from an injected seam.' -Path 'docs/features/active/2026-09-02-test-determinism-and-hygiene-debt-729/spec.md'` returns exactly one match, and the same holds for each of the other three Block L reason sentences; before the insertion, record `(Get-Content 'docs/features/active/2026-09-02-test-determinism-and-hygiene-debt-729/spec.md').Count`; after the insertion, the count has increased by exactly 5, which is the number of lines in Block L (its heading line plus its four numbered lines), and `Select-String -SimpleMatch '- **Finding 4 — pump-hosted' -Path 'docs/features/active/2026-09-02-test-determinism-and-hygiene-debt-729/spec.md'` still returns exactly one match, and `Select-String -SimpleMatch 'four independent reasons, §4.2' -Path 'docs/features/active/2026-09-02-test-determinism-and-hygiene-debt-729/spec.md'` still returns exactly one match. A ref-anchored diff is deliberately not used here: `spec.md` is untracked until P8-T22, so an anchored diff reports nothing about it and could not fail. Record both line counts and all six search results in `docs/features/active/2026-09-02-test-determinism-and-hygiene-debt-729/evidence/other/spec-block-l-insertion.2026-09-02T10-30.md`.
- [ ] [P7-T8] Sweep the feature folder for host identifiers before it is committed. Run a case-insensitive search over every file under `docs/features/active/2026-09-02-test-determinism-and-hygiene-debt-729` for four things. The first three are fixed-string searches: the account name, derived as `Split-Path -Leaf $env:USERPROFILE`; the machine name, read from `$env:COMPUTERNAME`; and the absolute workspace-root prefix, derived at run time as `(Resolve-Path .).Path`. The fourth is a pattern search rather than a fixed-string search and serves as a residual detector only: a single ASCII letter immediately followed by the two-character drive-root sequence, that sequence constructed at run time as `([string][char]58 + [string][char]92)` so that neither this plan nor this task's own artifact contains the literal. The letter-anchored form is required because the bare two-character sequence occurs three times inside the swept file set in non-host-path contexts — twice in this plan file, where it is preceded by a straight quote and by a backtick, and once in the research artifact inside a regular-expression literal, where it is preceded by an asterisk. Rewriting any of the three would corrupt a recorded acceptance command or a research citation, and leaving them would make the acceptance below unreachable. The letter-anchored form matches none of the three and still matches every genuine absolute path. Replace every workspace-root-prefix hit with `<repo-root>` in its entirety — the whole absolute prefix, never only its drive-root characters, because substituting two characters inside an absolute path drives the residual count to zero while leaving the remainder of that path disclosed — and replace every account-name hit with `<user>` and every machine-name hit with `<host>`, using the XML-escaped forms in any `.xml`, `.cobertura.xml`, `.trx`, or `.coveragexml` artifact, because those placeholders contain angle brackets that would otherwise make the document ill-formed, and re-parse every rewritten XML-family file with `[xml](Get-Content -Raw -Encoding UTF8 $path)` to confirm it is still well-formed. After those three substitutions have been applied, replace every remaining letter-anchored absolute path — the whole path token, from its drive letter to the end of the path — with `<external-path>`, using the XML-escaped form in any XML-family artifact and re-parsing it afterwards on the same terms. This fourth substitution is required because a raw, unprocessed Cobertura artifact carries third-party build-machine source paths that lie outside the workspace root and are therefore untouched by the workspace-root-prefix substitution, while still being matched by the letter-anchored residual detector: the raw dotnet-coverage output for this solution embeds `Mono.Reflection` source paths rooted at a drive-level `sources` directory. Three raw Cobertura artifacts already committed under `docs/features/active/` in this repository exhibit exactly that — `docs/features/active/2026-08-07-efcviewer-missing-lineage-and-segment-navigation-439/evidence/qa-gates/issue-439-final.cobertura.xml`, `docs/features/active/2026-08-25-quickfiler-high-confidence-partial-screen-backfill-608/evidence/qa-gates/r1-csharp-coverage.2026-08-25T12-33.cobertura.xml`, and `docs/features/active/2026-08-25-quickfiler-high-confidence-partial-screen-backfill-608/evidence/baseline/r3-csharp-coverage.2026-08-25T13-32.cobertura.xml` — each carrying 2,114 lines that the letter-anchored detector matches, of which 15 lines per artifact are the drive-level `sources` third-party paths that lie outside any workspace root and that the workspace-root-prefix substitution therefore cannot reach. A processed Cobertura carries no such path, because `scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1` lines 417-424 remove every non-allowlisted `<package>` and rewrite each retained `class/@filename` to a repository-relative path, so this fourth substitution is a no-op there; every other `*.cobertura.xml` committed under `docs/features/active/` returns zero letter-anchored matches, which confirms it empirically. The fourth substitution is load-bearing only when `CoberturaProcessingState:` is `raw`, which P0-T11 and P6-T5 both explicitly authorize. Without it, `ResidualMatchCount: 0` is unreachable on a plan-authorized execution path. The two Cobertura artifacts are the expected rewrite targets, because dotnet-coverage writes absolute `<source>` and `filename` values into them; both have already been read by P6-T6, which runs earlier, so no later task depends on their pre-substitution content. Record only the post-substitution values; do not quote the pre-substitution values in the evidence artifact itself, and do not spell the account name, the machine name, the absolute workspace-root prefix, or the two-character drive-root sequence literally in it. Rewrites land on files that earlier phase commits already committed, so they are staged and committed by P8-T22's directory-level `git add`. Write `docs/features/active/2026-09-02-test-determinism-and-hygiene-debt-729/evidence/qa-gates/host-path-sanitisation.2026-09-02T10-30.md` with `ResidualMatchCount:`, `FilesRewritten:`, `ExternalPathsRewritten:`, and `XmlReparseFailures:`, where `ResidualMatchCount:` is the total number of hits the account-name search, the machine-name search, and the letter-anchored drive-root search return after the substitution pass, counted over the same file set including this artifact, and `ExternalPathsRewritten:` is the number of path tokens the fourth substitution replaced with the `<external-path>` placeholder, which is `0` when no swept file carried an out-of-workspace absolute path. Acceptance: `ResidualMatchCount: 0`, `XmlReparseFailures: 0`, and an integer `ExternalPathsRewritten:` value is present.

### Phase 8 — Acceptance-criteria check-off

Each task below verifies exactly one acceptance criterion in `docs/features/active/2026-09-02-test-determinism-and-hygiene-debt-729/spec.md` and, only when the stated verification passes, changes that one criterion's checkbox from `- [ ]` to `- [x]` in that file.

- [ ] [P8-T1] Verify and check off AC1 using the P1-T1 acceptance evidence plus a confirmation that neither `WaitAsync` declaration contains a `=` default in its parameter list and that the 1-arg body is `return WaitAsync(delay, TimeProvider.System);`.
- [ ] [P8-T2] Verify and check off AC2 using P1-T1: `Select-String -SimpleMatch 'timeProvider.CreateTimer(' -Path 'TaskMaster/AppGlobals/NonBlockingDelay.cs'` returns exactly one match, `Select-String -SimpleMatch 'Timeout.InfiniteTimeSpan' -Path 'TaskMaster/AppGlobals/NonBlockingDelay.cs'` returns exactly one match, `Select-String -SimpleMatch 'TaskCreationOptions.RunContinuationsAsynchronously' -Path 'TaskMaster/AppGlobals/NonBlockingDelay.cs'` returns exactly one match, and `Select-String -SimpleMatch 'timer?.Dispose();' -Path 'TaskMaster/AppGlobals/NonBlockingDelay.cs'` returns exactly one match.
- [ ] [P8-T3] Verify and check off AC3 using the P1-T2 acceptance and the P6-T4 artifact's zero-`CS8632` statement.
- [ ] [P8-T4] Verify and check off AC4 using the P6-T3 and P6-T4 artifacts plus `git diff --name-only $base HEAD -- TaskMaster/AppGlobals/StoreRehookCoordinator.cs TaskMaster/AppGlobals/AppEvents.cs` and `git status --porcelain -- TaskMaster/AppGlobals/StoreRehookCoordinator.cs TaskMaster/AppGlobals/AppEvents.cs`, both of which must return empty output.
- [ ] [P8-T5] Verify and check off AC5 using the P2-T1 and P2-T2 acceptances and the P2-T4 artifact.
- [ ] [P8-T6] Verify and check off AC6 using the P2-T4 artifact, which must show all three test methods passing, and the P6-T6 artifact's `PostChangeCoveredLines:` and `PostChangeTotalLines:` values for `TaskMaster/AppGlobals/NonBlockingDelay.cs`.
- [ ] [P8-T7] Verify and check off AC7 using the P1-T3, P1-T4, and P1-T5 acceptances.
- [ ] [P8-T8] Verify and check off AC8 using the P4-T2 acceptance.
- [ ] [P8-T9] Verify and check off AC9 using the P4-T3, P4-T4, and P4-T6 acceptances and the P7-T6 delivery record's green-from-birth statement.
- [ ] [P8-T10] Verify and check off AC10 using the P3-T5 and P3-T6 acceptances.
- [ ] [P8-T11] Verify and check off AC11 using the P3-T1, P3-T2, P3-T8 acceptances and the existence of `docs/features/active/2026-09-02-test-determinism-and-hygiene-debt-729/evidence/regression-testing/svgcontrol-guard-fail-before.2026-09-02T10-30.md`.
- [ ] [P8-T12] Verify and check off AC12 using the P6-T5 artifact, whose `Output Summary:` must record both test node identifiers `UtilitiesCS.Test.NoLiveFormInTestAssemblyTests.ExecutingAssembly_ContainsNoFormDerivedType` and `SVGControl.Test.NoLiveFormInTestAssemblyTests.ExecutingAssembly_ContainsNoFormDerivedType` with `outcome="Passed"` against each, alongside the run-level `FailedCount:` recorded by P6-T5 for the whole discovered set; record both identifiers in the check-off note. A per-assembly failure breakdown is not available, because `scripts/vscode/Invoke-MSTestWithCoverage.ps1` line 76 issues one aggregate vstest invocation over the whole discovered assembly set and reports no per-assembly counts.
- [ ] [P8-T13] Verify and check off AC13 using the P5-T1, P5-T2, and P5-T3 acceptances.
- [ ] [P8-T14] Verify and check off AC14 using the P5-T4 and P5-T5 acceptances.
- [ ] [P8-T15] Verify and check off AC15 using the P5-T7 acceptance.
- [ ] [P8-T16] Verify and check off AC16 by confirming `Select-String -SimpleMatch '#743' -Path 'docs/features/active/2026-09-02-test-determinism-and-hygiene-debt-729/spec.md'` returns at least one match, `Select-String -SimpleMatch '#711' -Path 'docs/features/active/2026-09-02-test-determinism-and-hygiene-debt-729/spec.md'` returns at least one match, and `Select-String -SimpleMatch 'docs/features/potential/promoted/2026-09-02-quickfiler-itemviewer-ui-marshalling-seam.md' -Path 'docs/features/active/2026-09-02-test-determinism-and-hygiene-debt-729/spec.md'` returns at least one match. Additionally confirm the four-reasons clause of AC16: the P0-T2 scope-recap artifact `docs/features/active/2026-09-02-test-determinism-and-hygiene-debt-729/evidence/other/scope-recap.2026-09-02T10-30.md` contains one line equal to `Finding 4 — reasons no test-only fix exists:` followed by four numbered reason lines beginning `1. `, `2. `, `3. `, and `4. `, and `docs/features/active/2026-09-02-test-determinism-and-hygiene-debt-729/spec.md` contains the heading line and each of the four reason sentences, verified with `Select-String -SimpleMatch` so the two-space continuation indent P7-T7 applies in `spec.md` does not affect the match. The `#743`, `#711`, and promoted-path clauses are satisfied by spec content that already exists and are verified rather than authored here; the four-reasons clause is authored by P7-T7 and verified here.
- [ ] [P8-T17] Verify and check off AC17 using the P7-T1 acceptance.
- [ ] [P8-T18] Verify and check off AC18 using the P7-T2 acceptance.
- [ ] [P8-T19] Verify and check off AC19 using the P7-T3 acceptance.
- [ ] [P8-T20] Verify and check off AC20 using the P7-T4 acceptance. AC20 may be checked off under the P7-T4 carve-out, provided every path reported by `git status --porcelain -- .claude .codex .agents config/blast-radius.json config/orchestration-routing.json` is covered by one of the two allowances P7-T4 states — listed in the P0-T15 `PreExistingPaths:` set, or under `.claude/agent-memory/`. The rationale is that AC20's subject is repository policy and configuration content, and `.claude/agent-memory/` holds per-agent scratch memory written by the persistent-memory system of delegated agents outside any task in this plan. Record in the check-off note the enumerated list of reported paths and, for each, which allowance covers it. If any reported path is covered by neither allowance, leave AC20 unchecked and report it as an outstanding acceptance criterion.
- [ ] [P8-T21] Verify and check off AC21 using the P6-T8 single-pass artifact, which must record `EXIT_CODE: 0` for the csharpier check, the analyzer rebuild, and the nullable rebuild, and `FailedCount: 0` for the coverage-enabled test run, all in the same pass. A P6-T5 artifact recording `ExpectedExitCode: 1` with the literal threshold message and `FailedCount: 0` satisfies this task. A P6-T2 artifact whose final reported unformatted set is a subset of the `Baseline unformatted set:` recorded by P0-T8 after removing the seventeen paths deleted by Phases 3-5, and contains none of the seven plan-owned formattable paths and none of `UtilitiesCS/OutlookObjects/Filter DASL/DASLFilterParser.cs`, also satisfies this task. A P6-T5 artifact recording a non-zero `FailedCount:` whose every failing test node identifier is enumerated in that artifact and is in `QuickFiler.Test` also satisfies this task, provided P7-T6's `Known out-of-scope flakes:` section lists the same identifiers. If the P6-T2 subset allowance is the outcome that held, leave AC21 unchecked, record the residual unformatted set in the check-off note as a pre-existing condition not owned by this change, and report it as an outstanding acceptance criterion; AC21's literal text requires that `csharpier check` report no unformatted files, which the subset allowance does not establish.
- [ ] [P8-T22] Stage and commit every remaining change with `git add docs/features/active/2026-09-02-test-determinism-and-hygiene-debt-729 docs/features/potential/promoted/2026-09-02-quickfiler-itemviewer-ui-marshalling-seam.md` and a commit subject beginning `docs(729): `. The promoted record is included because it is the follow-up promotion output of this work that `spec.md` line 87 and AC16 cite, not incidental drift. Acceptance: every path reported by `git status --porcelain` is covered by at least one of three allowances — listed in the P0-T15 `PreExistingPaths:` set; under `.claude/agent-memory/`, which holds per-agent scratch memory written by the persistent-memory system of delegated agents outside any task in this plan; or the single modified-plan-file entry for `docs/features/active/2026-09-02-test-determinism-and-hygiene-debt-729/plan.2026-09-02T08-59.md` representing this task's own check-off — and the check-off note enumerates each reported path with an allowance that covers it. The three allowances deliberately overlap rather than partition: a path recorded by P0-T15 that also sits under `.claude/agent-memory/` is covered twice, which satisfies this acceptance. If any reported path is covered by none of the three, stop and report it. After checking this task off, run `git add docs/features/active/2026-09-02-test-determinism-and-hygiene-debt-729/plan.2026-09-02T08-59.md` and `git commit --amend --no-edit`, then re-run `git status --porcelain` and confirm every remaining reported path is covered by the first two allowances.

---

## Planner Adversarial Self-Review

SELF-REVIEW: RE-DERIVED THIS PASS

Revision round 5 — citations re-derived directly against the current working tree in *this* revision pass. This round applies exactly one delta, the R5-B1 fix confined to P7-T8, so the re-derivation covers that task's own citations and the sibling region that shares its subject matter.

U1. Bare drive-root-sequence hits in the swept file set — a search of every file under `docs/features/active/2026-09-02-test-determinism-and-hygiene-debt-729` for the bare two-character drive-root sequence returns exactly three hits in the current tree, unchanged from round 4: `plan.2026-09-02T08-59.md` at the P0-T6 acceptance command, `plan.2026-09-02T08-59.md` at the round-2 sibling-re-check bullet naming P0-T6's absence assertions, and `research/research-729.2026-09-02T09-30.md` line 289 inside a regular-expression literal. The character immediately preceding the sequence is, respectively, a straight single quote (the hit sits inside `-SimpleMatch ':` followed by the escape character and a closing quote), a backtick, and an asterisk closing a `\s*` quantifier. None is an ASCII letter, so the letter-anchored residual detector matches none of the three and none needs rewriting.

U2. Letter-anchored hits in the swept file set — a search of the same file set for an ASCII letter immediately followed by the drive-root sequence returns zero hits in the current tree, re-run after this round's own edits to P7-T8. This round's inserted prose therefore introduces no account name, no machine name, and no letter-anchored drive-root sequence of its own: it names the placeholder token `<external-path>` and describes the drive-level `sources` directory in prose rather than spelling any absolute path. The delta does not reintroduce the violation class it exists to close.

U3. Third-party absolute-path class in raw Cobertura output — of every `*.cobertura.xml` committed under `docs/features/active/` in the current tree, exactly three return any letter-anchored absolute path: `2026-08-07-efcviewer-missing-lineage-and-segment-navigation-439/evidence/qa-gates/issue-439-final.cobertura.xml`, `2026-08-25-quickfiler-high-confidence-partial-screen-backfill-608/evidence/qa-gates/r1-csharp-coverage.2026-08-25T12-33.cobertura.xml`, and `2026-08-25-quickfiler-high-confidence-partial-screen-backfill-608/evidence/baseline/r3-csharp-coverage.2026-08-25T13-32.cobertura.xml`. Each returns exactly 2,114 such lines, of which exactly 15 are rooted at a drive-level `sources` directory and name `Mono.Reflection` sources — for example the `<class name="Mono.Reflection.BackingFieldResolver" filename=...>` element in the first of the three. Those 15 lie outside any workspace root, so the workspace-root-prefix substitution cannot reach them while the letter-anchored detector still matches them. Every other Cobertura artifact under `docs/features/active/` returns zero letter-anchored matches, confirming that the fourth substitution is a no-op on processed output.

U4. `scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1` lines 417-424 — lines 417-421 iterate `$packagesNode.ChildNodes` and `RemoveChild` every element whose `name` is not in `$ProjectNames`; lines 423-424 iterate `//class[@filename]` and assign `ConvertTo-KoverageRelativePath -Path $classNode.filename -RepoRoot $RepoRoot`. This is the mechanism behind U3's empirical finding and is the citation the new P7-T8 sentence carries.

U5. One authoring correction was applied to the supplied round-5 delta text, for the same reason the round-4 pass corrected two clauses of its own supplied delta: the delta attributed the figure "2,114 such paths per artifact" to the `Mono.Reflection` drive-level `sources` paths, but 2,114 is the count of *all* letter-anchored absolute-path lines per raw artifact and 15 is the count of the drive-level `sources` subset. Writing the supplied figure would have placed a false citation in the plan. Both figures are now stated with the quantity each actually measures, and the three artifacts are named so a third party can re-derive both. The correction does not change which paths the fourth substitution rewrites or what the acceptance requires. The supplied phrase "in a sibling worktree of this repository" was likewise narrowed to "in this repository", because the three artifacts were re-derived in this worktree rather than in a sibling one.

Sibling-region re-checks performed in revision round 5:

- Around the P7-T8 edit, first sibling: P0-T6's own acceptance command at the task's `Select-String -SimpleMatch` clause still carries the bare drive-root sequence and is byte-for-byte undisturbed by this round's edit, which touched only text inside P7-T8. P0-T6 remains satisfiable for the reason recorded in round 2 — its artifact enumerates analyzer `Include` values resolved against each declaring project's own directory, which are repository-relative by construction.
- Around the P7-T8 edit, second sibling: the round-2 sibling-re-check bullet that also contains the bare sequence is likewise undisturbed. Both plan-file occurrences are non-letter-anchored, so the fourth substitution does not match them and cannot corrupt either a recorded acceptance command or a recorded self-review finding.
- Around the fourth substitution and P6-T6: P6-T6 is the only task that reads the two Cobertura artifacts, and it runs before P7-T8, so rewriting an out-of-workspace path in a raw artifact strands no later read. The rewrite replaces a `class/@filename` value in a package P6-T6 does not aggregate — P6-T6 aggregates only `<class>` elements whose `filename` ends with `NonBlockingDelay.cs` — so even a re-read would be unaffected.
- Around the fourth substitution and rule 3: a workspace-rooted path is replaced by rule 3 with `<repo-root>` before the fourth rule runs, so the result no longer begins with a drive letter and the fourth rule does not double-rewrite it. The two rules do not compete for the same token.
- Around the new `ExternalPathsRewritten:` field: it is a recorded count, not a threshold. Its acceptance requires only that an integer value be present, so a processed-Cobertura run that rewrites nothing records `0` and passes, while a raw-Cobertura run records the non-zero count. Requiring a non-zero value would have been unsatisfiable on the processed path, which is the mirror image of the defect this round closes.
- Around the AC mapping: this round touches no AC-mapped implementation, test, or evidence task. P7-T8 carries no `AC-MAPPING` entry, so all 21 mappings are unchanged from round 4 and no inventory entry gains or loses a mapping.

Revision round 4 — citations re-derived directly against the working tree in the round-4 revision pass, covering every citation the four round-3 preflight deltas touched and the sibling region of each edit:

T1. `docs/features/active/2026-09-02-test-determinism-and-hygiene-debt-729/plan.2026-09-02T08-59.md` and `docs/features/active/2026-09-02-test-determinism-and-hygiene-debt-729/research/research-729.2026-09-02T09-30.md` — a search of the whole feature folder for the bare two-character drive-root sequence returns exactly three hits, all non-host-path: the P0-T6 acceptance command in this plan file, the round-2 sibling-re-check bullet in this plan file that names the absence checks, and the `research-729.2026-09-02T09-30.md` "Primary Search Strategy" bullet whose regular-expression literal contains `\s*` after a colon. Line numbers are deliberately not cited for these three, because they shift with every revision of this plan and the B2 wording does not depend on them.

T2. Preceding-character re-derivation for the three T1 hits — the P0-T6 hit is preceded by a straight single quote (it sits inside `-SimpleMatch ':` followed by the escape character and a closing quote); the round-2 bullet hit is preceded by a backtick; the research hit is preceded by an asterisk closing the `\s*` quantifier. None of the three is preceded by an ASCII letter, so the letter-anchored residual detector introduced by delta B2 matches none of them, while every genuine absolute path in this environment begins with a drive letter and is matched. `ResidualMatchCount: 0` is therefore reachable without rewriting any of the three.

T3. Feature-folder account-name search — a search of every file under `docs/features/active/2026-09-02-test-determinism-and-hygiene-debt-729` for the account name returns zero hits in the current tree, so the account-name component of the P7-T8 residual count is already at zero before the substitution pass and the round-3 B4(i) removals are confirmed still in effect.

T4. `.claude/agent-memory/task-researcher/project_test_determinism_debt_729.md`, `.claude/agent-memory/atomic-planner/project_729_dirty_tree_and_host_leak_plan_seams.md`, and `docs/features/potential/promoted/2026-09-02-quickfiler-itemviewer-ui-marshalling-seam.md` — all three confirmed present on disk in this pass. They are the concrete instances behind the S9 reconstruction, the B1 allowance at P8-T22, and the M1 third-bucket assignment at P7-T2.

T5. Self-defeat re-check of this round's own edit text — the replacement P7-T8 prose, the replacement P8-T22 acceptance, the amended D10 tail, the amended P7-T2 bucket list, and the rewritten S9 entry each contain no absolute workspace-root prefix, no account name, no machine name, and no ASCII letter immediately followed by the drive-root sequence. This round therefore adds no new hit to any of the four P7-T8 searches and does not make the P7-T8 acceptance self-defeating.

Sibling-region re-checks performed in revision round 4:

- Around the D10 tail amendment: P8-T20 is not named in D10's amended final sentence, and does not need to be. P8-T20 does not state a cleanliness rule of its own; it defers explicitly to the two allowances P7-T4 states, and P7-T4 is named. The `git diff $base HEAD` clause that the round-3 wording carried in the same sentence is preserved as its own preceding sentence rather than dropped, so no claim is lost.
- Around the P8-T22 acceptance replacement: P8-T22 still stages exactly the two spans it staged before (`docs/features/active/2026-09-02-test-determinism-and-hygiene-debt-729` and the promoted record), so P7-T5's changed-file inventory, its `Deltas:` expectations, and its "exactly seventeen `D` entries" acceptance are unaffected. The amend step and the plan-file self-check-off fixpoint are unchanged; only the set of allowances the post-amend status is measured against widened.
- Around the P7-T2 bucket amendment: P7-T5 compares `git status --porcelain` against the "Complete file-write inventory", which already lists the promoted record under "Follow-up promotion record committed by this plan", so the promoted record produces no unexplained delta there. Agent-memory paths are not in that inventory and therefore do appear as differences at P7-T5, but P7-T5's acceptance requires the `Deltas:` section to *explain* every difference rather than to be empty, so an entry naming them as agent-memory scratch written outside any plan task satisfies it. No edit to P7-T5 is required.
- Around the P7-T8 substitution amendment: P0-T6's acceptance still searches its own artifact for the bare sequence with `Select-String -SimpleMatch`, and that check is unchanged and independent of P7-T8. P0-T6's artifact records analyzer `Include` values resolved against each declaring project's own directory, which are repository-relative by construction, so P0-T6 remains satisfiable. P7-T8 does not rewrite either of the two plan-file occurrences, so it cannot corrupt P0-T6's recorded acceptance command.
- Around the P7-T8 residual-count amendment: the workspace-root-prefix search is a substitution target but is deliberately not one of the three residual searches, because any residual absolute path that survived substitution necessarily begins with a drive letter and is therefore already counted by the letter-anchored search. Omitting it from the residual definition removes a double count, not a check.
- Two authoring corrections were applied to the round-3 delta text itself, each because the supplied wording would have reintroduced the violation class it was written to close. First, the B1 acceptance was supplied as "covered by exactly one of three allowances"; the P0-T15 set and the `.claude/agent-memory/` allowance overlap by construction for the two `task-researcher` paths, so "exactly one" is falsified by the very paths the delta exists to admit. It is written as "at least one", with the overlap stated explicitly. Second, the B2 wording described the letter-anchored detector as part of a "fixed-string search"; a single-ASCII-letter anchor is not expressible as a fixed string, so the first three searches are stated as fixed-string searches and the fourth as a pattern search. Neither correction changes which paths or which text the two tasks accept.
- Around the M1 fourth-bucket restriction: the same admission-versus-compulsion ambiguity that M1 closes for the promoted record also existed for the untracked feature folder, which P0-T15 lists and which the fourth-bucket clause therefore admitted. The restriction now states that feature-folder paths belong in the third bucket for the same reason, so the four buckets partition rather than overlap and the "exactly one of four buckets" instruction is decidable.

Revision round 3 — citations re-derived directly against the working tree in the round-3 revision pass, covering every citation the twelve round-2 preflight deltas touched and the sibling region of each edit. Entries S8 and S9 were additionally re-derived and corrected in revision round 4:

S1. `SVGControl.Test/SVGControl.Test.csproj` — line 54 is the opening `<ItemGroup>`; lines 55-57 `<Compile Include="Form1.cs">` with `<SubType>Form</SubType>`; lines 58-60 `<Compile Include="Form1.Designer.cs">` with `<DependentUpon>Form1.cs</DependentUpon>`; lines 61-63 `<Compile Include="Form2.cs">`; lines 64-66 `<Compile Include="Form2.Designer.cs">`; line 67 `<Compile Include="GetRelativePath_Test.cs" />`. The four Form `<Compile>` elements therefore span 55-66, not 54-66. This is delta m2, re-derived against the tree in this pass rather than accepted from the delta text.
S2. `SVGControl.Test/SVGControl.Test.csproj` — sibling region of S1: line 84 closes the `<ItemGroup>` holding the `<Compile>` items and line 85 opens the resource `<ItemGroup>`; lines 86-88 `<EmbeddedResource Include="Form1.resx">`, lines 89-91 `<EmbeddedResource Include="Form2.resx">`, lines 92-95 `<EmbeddedResource Include="Resources.resx">`, line 96 closes that group. The `86-91` citation is correct and unchanged, and Block H's resource removals still leave `Resources.resx` intact. `.gitignore` lines 144-145 `coverage/*` and `!coverage/.gitkeep` re-derived unchanged.
S3. `scripts/vscode/Invoke-MSTestWithCoverage.ps1` — line 76 appends `@($TestAssembly)` followed by `/Settings:`, `/InIsolation`, and `/TestCaseFilter:TestCategory!=LiveOutlook` to a single `dotnet-coverage collect ... -- <vstest>` argument list. That is one aggregate vstest invocation over the whole discovered assembly set with no per-assembly reporting and no `/Logger:`, which is the basis for delta M3's replacement of P8-T12's per-assembly `FailedCount:` demand and for P6-T5's separate scoped confirmation runs.
S4. `scripts/vscode/Invoke-MSTestWithCoverage.ps1` — lines 296-302 build `$testAssemblies` from `Get-ChildItem -Path $resolvedSearchRoot -Recurse -Filter '*.Test.dll'` selecting `FullName`, filtered to `\bin\<Configuration>\` and excluding `\obj\` and `\ref\`; line 315 writes `"Discovered $($testAssemblies.Count) test assemblies."`. Discovered paths are therefore absolute, which is why the round-3 D9 records an integer count, a boolean, and repository-relative forms derived by removing the resolved search-root prefix, rather than asserting an absolute prefix.
S5. `docs/features/active/2026-09-02-test-determinism-and-hygiene-debt-729/spec.md` — line 87 is a single-line bullet beginning `- **Finding 4 — pump-hosted ` containing the substring `four independent reasons, §4.2` exactly once; line 88 is the next top-level bullet, `- **All \`QuickFiler/\` production sources.**`. This confirms the M2 insertion point (between lines 87 and 88) and confirms both M1 survivor tokens exist exactly once before the insertion.
S6. `docs/features/active/2026-09-02-test-determinism-and-hygiene-debt-729/spec.md` — line 275 AC20 reads "No file under .claude/\*\*, .codex/\*\*, .agents/\*\*, config/blast-radius.json, or config/orchestration-routing.json is added, modified, or deleted." Its literal text is unqualified, so the B1 carve-out is a documented reading of AC20's subject rather than a match to its literal wording; P8-T20 now states that reading explicitly and requires each reported path to be enumerated against one of the two allowances.
S7. `docs/features/active/2026-09-02-test-determinism-and-hygiene-debt-729/spec.md` — line 276 AC21 requires that `dotnet tool run csharpier check .` "reports no unformatted files". The P6-T2 subset allowance does not establish that, which is delta m1; P8-T21 now leaves AC21 unchecked under that branch. Lines 254-276 still carry AC1 through AC21, one per line, with no duplicates and no gaps, so the 21-entry inventory below is unchanged.
S8. Feature-folder host-identifier sweep — a case-insensitive search of every file under `docs/features/active/2026-09-02-test-determinism-and-hygiene-debt-729` for the account name and for a genuine absolute workspace-root prefix returned exactly two hits before revision round 3, both in this plan file: the `Workspace root:` header line and the D9 decision. Delta B4(i) removed both. `spec.md`, `issue.md`, and the research artifact contain no such hit; the two Cobertura artifacts do not yet exist and are the expected rewrite targets. Corrected in revision round 4 as T2 below: the *bare* two-character drive-root sequence, as distinct from a genuine absolute path, has three surviving non-host-path occurrences in the swept set, so the round-3 wording of this entry understated the bare-sequence hit count and the round-3 P7-T8 acceptance was unreachable as written.
S9. Worktree state — `git status --porcelain` now reports six paths rather than the four recorded in revision round 2. The reconstructed list is: a modified `.claude/agent-memory/task-researcher/MEMORY.md`; an untracked `.claude/agent-memory/task-researcher/project_test_determinism_debt_729.md`; a modified `.claude/agent-memory/atomic-planner/MEMORY.md`; an untracked `.claude/agent-memory/atomic-planner/project_729_dirty_tree_and_host_leak_plan_seams.md`; the untracked feature folder `docs/features/active/2026-09-02-test-determinism-and-hygiene-debt-729/`; and the untracked promoted record `docs/features/potential/promoted/2026-09-02-quickfiler-itemviewer-ui-marshalling-seam.md`. The four agent-memory entries are tracked-directory writes and are the basis for B1; the promoted record is the #743 promotion output and is the basis for B2 and for M1. The two `atomic-planner` entries were written by the planner's own persistent-memory system after the round-2 observation, which is the direct evidence that `.claude/agent-memory/` paths appear between P0-T15 and P8-T22 and therefore the reason P8-T22 carries the same allowance as P7-T4. Uncertainty is recorded explicitly: the four `.claude/agent-memory/` files and both `docs/` paths were each confirmed present on disk in this pass, and the two `atomic-planner` writes were made by this agent, but no `git status --porcelain` invocation was available to this pass, so the modified-versus-untracked classification of each entry and the exhaustiveness of the six-path list are a reconstruction rather than a direct observation. P0-T15 is the task that records the authoritative set at execution time, and every downstream allowance is evaluated against that recorded set rather than against this reconstruction.

Sibling-region re-checks performed in revision round 3:

- Around delta B5's split: both TRX files now live in the same `coverage\trx\p6t5` directory under distinct `LogFileName` values, so neither the round-2 collision analysis nor the `.gitignore` line 144 untracked conclusion changes. The B5 rationale is re-derived rather than asserted: Blocks E and F declare the same class name and the same method name in two namespaces, and a TRX `<UnitTestResult>` carries only the bare method name, so one combined TRX genuinely cannot disambiguate the two nodes.
- Around delta B4(iv)'s `computerName` prohibition: P3-T4, P3-T8, P4-T6, P5-T6, and P2-T4 record counts and node names rather than raw TRX elements, and none of them passes `/Logger:trx` at all, so P6-T5 is the only task that could have leaked a `computerName` attribute. No other task text needs the same restriction.
- Around delta B4(v)'s new P7-T8: it runs after P6-T6, which is the only task that reads the two Cobertura artifacts, so rewriting them strands no later read. Its rewrites land on files already committed by earlier phase commits, and P8-T22's directory-level `git add` stages them, so P7-T8 leaves nothing uncommitted. Its own search-token derivation avoids writing the drive-root literal into the plan or the artifact, so the `ResidualMatchCount: 0` acceptance is not self-defeating.
- Around delta M2's indentation split: P0-T2 still requires the scope-recap artifact to carry unindented `1. ` through `4. ` lines, P7-T7 now requires a two-space indent in `spec.md`, and P8-T16 now verifies each side with `Select-String -SimpleMatch` against the reason sentences rather than demanding the two files match line-for-line. The three tasks are consistent under the new wording.
- Around delta M1's replacement: P7-T7 no longer uses a git-diff acceptance for `spec.md`. The remaining anchored-diff acceptances in the plan (P1-T5, P5-T3, P7-T1, P7-T2, P7-T3, P7-T4, P8-T4) all target tracked paths, so none of them inherits the untracked-blindness defect M1 closes.
- Around delta B1's carve-out: D10 was rewritten so it no longer asserts a clean `.claude/**` tree, which would have contradicted P0-T15 and P7-T4. The `git diff $base HEAD` half of P7-T4 stays an unconditional empty-output assertion, because no plan task commits a `.claude/**` path and the agent-memory writes are never staged.
- Around delta B2's `git add` span: `docs/features/potential/promoted/` is outside the P7-T8 sweep scope, which is the active feature folder only. The promoted record is authored by the MCP promotion route and contains no host identifier; the sweep is not widened to cover it, and P8-T22 commits it as-is.

Round-2 record, retained for audit and not claimed as re-derived in this pass except where an entry above supersedes it:

R1. `scripts/vscode/Invoke-MSTestWithCoverage.ps1` — line 76 `Get-DotnetCoverageArgumentList` returns the inner vstest arguments as exactly `/Settings:$RunSettingsPath`, `/InIsolation`, `/TestCaseFilter:TestCategory!=LiveOutlook`. No `/Logger:` and no `/ResultsDirectory` is passed, so the runner's own output cannot enumerate passing test nodes. This is why P6-T5 obtains the two AC12 node outcomes from a separate TRX-logged scoped run rather than from the full-suite console text.
R2. `scripts/vscode/Invoke-MSTestWithCoverage.ps1` — lines 232-237 `Invoke-DotnetCoverageCollection` sets `$global:LASTEXITCODE = 0`, invokes dotnet-coverage, and `throw "MSTest with coverage failed with exit code $coverageExitCode"` on any non-zero code. Basis for D14 and for the authorized #743 re-run branch in P0-T11 and P6-T5.
R3. `scripts/vscode/Invoke-MSTestWithCoverage.ps1` — lines 238-242 the `finally` block removes the derived settings file only when `$shouldRemoveDerivedSettings` is true; lines 118-136 `Get-DerivedCoverageSettingsPath` returns `Join-Path $outputDirectory "$outputName.effective-coverage.config"`. The two concrete stranded-file paths asserted by P0-T11 and P6-T5 are therefore `coverage-baseline.cobertura.xml.effective-coverage.config` and `coverage-final.cobertura.xml.effective-coverage.config` in their respective evidence directories.
R4. `scripts/vscode/Invoke-MSTestWithCoverage.ps1` — lines 326-343: `Invoke-DotnetCoverageCollection` at 326, `ConvertTo-KoverageCoberturaXml` at 340, `Assert-CoberturaLineCoverageThreshold` at 341, `Set-Content` of the processed XML at 343. A throw at 326 or 341 leaves the raw dotnet-coverage file on disk. Basis for D13 and for the `CoberturaProcessingState:` field.
R5. `scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1` — lines 417-447 `ConvertTo-KoverageCoberturaXml` removes every `<package>` whose `name` is not in the allowlist, calls `Remove-CoberturaExemptClosureCoverage` and `Merge-CoberturaClassesByFilename`, then `SetAttribute`s all six root aggregates (`line-rate`, `branch-rate`, `lines-covered`, `lines-valid`, `branches-covered`, `branches-valid`). Confirms raw and processed Cobertura are not on the same denominator, which is what P6-T6's new same-state precondition guards.
R6. `scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1` — line 489 `throw "Cobertura line coverage $formattedPercentage% is below the required 80% threshold."`. The literal substring asserted by the authorized non-zero branch in both P0-T11 and P6-T5 is `is below the required 80% threshold`, which is present in that message verbatim.
R7. `.gitignore` — line 144 `coverage/*` and line 145 `!coverage/.gitkeep`. Confirms the P6-T5 TRX destinations, now `coverage\trx\p6t5\utilitiescs-noliveform.trx` and `coverage\trx\p6t5\svgcontrol-noliveform.trx` after the round-3 B5 split, are untracked and cannot dirty the worktree ahead of P6-T9 or P8-T22. Re-derived again in round 3 as S2 below.
R8. `.csharpierignore` — lines 4-8 `**/evidence/**`, `*.cobertura.xml`, `*.coverage`, `*.coveragexml`, `*.trx`; lines 12-14 `*.csproj`, `*.props`, `*.targets`. Markdown is not processed by CSharpier at all, so the P7-T7 `spec.md` insertion cannot be rewritten by the P6-T1 format pass, and the new TRX is doubly exempt.
R9. `scripts/vscode/TaskMaster.cli.runsettings` — present in the tree; it is the `/Settings:` file the P6-T5 scoped confirmation run passes, matching the form already used by P2-T4, P3-T4, P4-T6, and P5-T6.
R10. `docs/features/active/2026-09-02-test-determinism-and-hygiene-debt-729/research/research-729.2026-09-02T09-30.md` — line 238 heading `### 4.2 Why the pump cannot be faked from the test side`; line 240 states the replacement "is nevertheless not *sufficient*, for four independently verified reasons"; the four bolded lead sentences are at lines 242, 248, 250, and 252 and are reproduced verbatim in Block L.
R11. `docs/features/active/2026-09-02-test-determinism-and-hygiene-debt-729/spec.md` — line 87 is the `### Out of scope / non-goals` bullet beginning `- **Finding 4 — pump-hosted `; it cites "four independent reasons, §4.2" but does **not** enumerate them, and a whole-file search for the four reason sentences returns zero matches. AC16 at line 271 requires the four reasons to be recorded "in this spec". The prior round's P8-T16 claim that AC16 "is satisfied by spec content that already exists" was therefore false for that clause, which is why P7-T7 now authors the enumeration and P8-T16 verifies it. This is a sibling-region finding produced by the DEF-8 re-derivation, not one of the ten supplied deltas.
R12. `docs/features/active/2026-09-02-test-determinism-and-hygiene-debt-729/spec.md` — line 267 AC12 requires both guard tests "passing in the final full test run"; line 271 AC16 as quoted in R11. The acceptance-criteria block still runs AC1 through AC21 with no duplicates and no gaps, so the 21-entry AC inventory below is unchanged.
R13. `docs/features/active/2026-09-02-test-determinism-and-hygiene-debt-729/spec.md` — line 235 states the `SVGControl.Test` guard is the genuine red-before/green-after test and line 234 states the `UtilitiesCS.Test` guard is green from birth. Unchanged by this revision; P8-T12's rewording asserts node outcomes and does not disturb that distinction.
R14. Sibling region of the DEF-4 edit — P1-T8, P2-T6, P3-T9, P4-T7, and P5-T8 stage only their own phase paths, and none of them runs after P6-T1. Confirms that without the P6-T9 change no task stages a P6-T1 rewrite, and that adding the seven paths to P6-T9 double-stages nothing.
R15. Sibling region of the DEF-6 edit — P6-T2's authorized branch runs `csharpier format` in *directory* form over `TaskMaster.Test`, which is broader than the D4 scope lock. That branch now carries a `git status --porcelain -- TaskMaster.Test` observation and a revert instruction, because an out-of-scope rewrite there would survive P6-T9's path-scoped staging and only surface as a dirty worktree at P8-T22. This is a sibling-region finding produced by the DEF-4/DEF-6 re-derivation.
R16. Sibling region of the DEF-9 edit — the delta's literal wording ("no file other than the named Cobertura artifact and this task's evidence markdown is present in that evidence directory") is unsatisfiable as written, because `evidence/baseline/` also holds the P0-T1 through P0-T10, P0-T12, P0-T13, and P0-T14 artifacts by construction. The intent — no stranded derived settings file — is preserved as a concrete two-part absence assertion naming the exact `*.effective-coverage.config` path.

Citations re-derived directly against the working tree in the original authoring pass. No edit in revision round 2 touched any file or region cited below, except `.csharpierignore` (entry 25), `.gitignore` (entry 26), `scripts/vscode/Invoke-MSTestWithCoverage.ps1` (entry 28), `scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1` (entry 29), and `spec.md` (entry 32) — each of which was re-derived again in the round-2 pass as R1 through R12 above and found unchanged. Revision round 3 additionally re-derived entry 13 (`SVGControl.Test/SVGControl.Test.csproj`, corrected to lines 55-66), entry 26 (`.gitignore`), entry 28 (`scripts/vscode/Invoke-MSTestWithCoverage.ps1`), and entry 32 (`spec.md`) against the current tree as S1 through S8 above:

1. `TaskMaster/AppGlobals/NonBlockingDelay.cs` — line 42 `public static Task WaitAsync(TimeSpan delay)`; file is 68 lines total.
2. `TaskMaster/AppGlobals/NonBlockingDelay.cs` — lines 52-54 `#nullable enable annotations` / `Timer? timer = null;` / `#nullable restore annotations`.
3. `TaskMaster/AppGlobals/NonBlockingDelay.cs` — lines 55-64 `timer = new Timer(...)` with `delay` and `Timeout.InfiniteTimeSpan`; line 44-46 `TaskCreationOptions.RunContinuationsAsynchronously`.
4. `TaskMaster/AppGlobals/StoreRehookCoordinator.cs` — line 102 `_delay = delay ?? NonBlockingDelay.WaitAsync;`; line 55 `private readonly Func<TimeSpan, Task> _delay;`; line 83 `Func<TimeSpan, Task>? delay = null`.
5. `TaskMaster/AppGlobals/AppEvents.cs` — line 456 `await NonBlockingDelay.WaitAsync(TimeSpan.FromMilliseconds(100));`.
6. `TaskMaster/TaskMaster.csproj` — lines 148-149 `<Reference Include="Microsoft.Bcl.TimeProvider, Version=10.0.0.11, ...>` with its `HintPath`; `TaskMaster/packages.config` line 16 `<package id="Microsoft.Bcl.TimeProvider" version="10.0.11" targetFramework="net481" />`. This is a NEW fact not present in the research artifact and it is why D3 forbids any production project-file edit.
7. `TaskMaster.Test/AppGlobals/NonBlockingDelayTests.cs` — line 2 `using System.Diagnostics;`; lines 38-39 `var interval = TimeSpan.FromMilliseconds(30);` and `var stopwatch = Stopwatch.StartNew();`; lines 53-58 the elapsed-time assertion; `[Timeout(5000)]` at lines 29 and 67.
8. `TaskMaster.Test/TaskMaster.Test.csproj` — line 73 `</Reference>` closing `Microsoft.Bcl.AsyncInterfaces`, line 74 `<Reference Include="Microsoft.Build" />`; line 121 `</Reference>` closing `Microsoft.Extensions.Primitives`, line 122 `<Reference Include="Microsoft.Identity.Client, ...>`. Both Block C insertion points confirmed at the line numbers research §1.5 stated.
9. `TaskMaster.Test/packages.config` — line 17 `Microsoft.Bcl.AsyncInterfaces`, line 18 `Microsoft.CodeAnalysis.BannedApiAnalyzers`; line 82 `Microsoft.Extensions.Primitives`, line 83 `Microsoft.Identity.Client`. Both Block D insertion points confirmed. Neither new package id occurs anywhere in the file today.
10. `UtilitiesCS.Test/UtilitiesCS.Test.csproj` — line 591-593 and 643-645 carry the two `Reference` blocks Block C mirrors verbatim; `UtilitiesCS.Test/packages.config` lines 23 and 90-94 carry the two `package` entries Block D mirrors verbatim.
11. `TaskMaster.Test/app.config` — lines 267-271 already carry the `Microsoft.Bcl.TimeProvider` binding redirect `oldVersion="0.0.0.0-10.0.0.11" newVersion="10.0.0.11"`; no redirect exists for `Microsoft.Extensions.TimeProvider.Testing`. Confirms AC7's "app.config is unmodified" is achievable.
12. `UtilitiesCS/Threading/ThreadMonitor.cs` — line 37 `private readonly TimeProvider _timeProvider;`, line 43 `private ITimer? _pollTimer;`, usings at lines 2 and 6 are `using System;` and `using System.Threading;`. Confirms `TimeProvider` and `ITimer` resolve on net481 with the usings already present in Block A.
13. `SVGControl.Test/SVGControl.Test.csproj` — lines 55-66 hold the four `<Compile>` entries for `Form1.cs`, `Form1.Designer.cs`, `Form2.cs`, `Form2.Designer.cs`; line 54 is the opening `<ItemGroup>` that contains them and is retained; line 67 is `<Compile Include="GetRelativePath_Test.cs" />` (the Block G insertion anchor); lines 86-91 hold the two `<EmbeddedResource>` entries for `Form1.resx` and `Form2.resx`. Corrected from `54-66` in round 3 per delta m2 and re-derived again in round 3 as S1 below.
14. `SVGControl.Test/SVGControl.Test.csproj` — line 133 `FluentAssertions, Version=8.10.0.0`, line 233 `MSTest.TestFramework, Version=4.3.3.0`, line 344 `<Reference Include="System.Windows.Forms" />`. The `System.Windows.Forms` reference is a standalone framework reference and survives the Block H removals, so Block E compiles.
15. `SVGControl.Test/SVGControl.Test.csproj` — lines 9-10 default `Configuration` to `Debug` and `Platform` to `AnyCPU`; line 29 is the `Debug|AnyCPU` condition with `OutputPath bin\Debug\`. This is the basis for D6.
16. `SVGControl.Test/` directory listing — all six form files present on disk: `Form1.cs`, `Form1.Designer.cs`, `Form1.resx`, `Form2.cs`, `Form2.Designer.cs`, `Form2.resx`.
17. `QuickFiler.Test/NoLiveFormInTestAssemblyTests.cs` — 54 lines, namespace at line 7, `typeof(System.Windows.Forms.Form)` at line 20, `GetLoadableTypes` with the `ReflectionTypeLoadException` fallback at lines 42-52. Blocks E and F are this file with only the namespace line changed.
18. `UtilitiesCS.Test/UtilitiesCS.Test.csproj` — line 76 `<Compile Include="TestAssemblyInitializer.cs" />` (the Block G insertion anchor); line 270 `<Compile Include="OutlookObjects\Filter DASL\DASLFilterParserTests.cs" />`. A search for `ResourceTests.cs`, `Form1`, `Form2`, `Form3`, and `DASLFilterParser_Tests` across that project file returns matches only for line 270's `DASLFilterParserTests.cs`, confirming all eleven deletion targets in `UtilitiesCS.Test/` are unreferenced orphans.
19. `UtilitiesCS.Test/` directory listing — all ten orphan files present on disk; `UtilitiesCS.Test/OutlookObjects/DASLFilterParser_Tests.cs` present on disk.
20. `UtilitiesCS.Test/Properties/AssemblyInfo.cs` — lines 18-21 `[assembly: Parallelize(Workers = 0, Scope = Microsoft.VisualStudio.TestTools.UnitTesting.ExecutionScope.ClassLevel)]`. The citation the new hazard comments must carry.
21. `UtilitiesCS.Test/HelperClasses/PrettyPrint_Tests.cs` — lines 14-20 hold the precedent comment plus `[DoNotParallelize]` at line 19 and `[TestClass]` at line 20; the comment cites `TaskMaster.runsettings`, which is the stale citation Blocks I and J must not repeat as the operative source.
22. `UtilitiesCS.Test/OutlookObjects/Table/OlTableExtensions_Tests.cs` — lines 17-21 hold the second precedent comment plus `[DoNotParallelize]` at line 20 and `[TestClass]` at line 21.
23. `UtilitiesCS.Test/OutlookObjects/Filter DASL/DASLFilterParserTests.cs` — `[TestClass]` at line 8, `public class DASLFilterParserTests` at line 9, no `[DoNotParallelize]` present; `PrintTree_WritesIndentedTreeToConsole` declared at line 95; `var originalOut = Console.Out;` at line 101, `Console.SetOut(writer);` at line 102, `Console.SetOut(originalOut);` at line 111. Research §3.2 cited the method as beginning at line 94; the declaration is at line 95 and the three `Console` lines match research exactly.
24. `UtilitiesCS.Test/ReusableTypeClasses/StackGeek_Tests.cs` — `[TestClass]` at line 9, `public class StackGeek_Tests` at line 10, no `[DoNotParallelize]` present; `Main_RunsSampleScenarioWithoutThrowing` declared at line 140; `var originalOut = Console.Out;` at line 144, `Console.SetOut(writer);` at line 146, `Console.SetOut(originalOut);` at line 155. Research §3.4 cited the method as beginning at line 139; the declaration is at line 140.
25. `.csharpierignore` — line 4 `**/evidence/**`, line 12 `*.csproj`, lines 13-14 `*.props` / `*.targets`. `packages.config` is not listed, confirming CSharpier will process `TaskMaster.Test/packages.config` and that evidence artifacts are exempt from the formatter.
26. `.gitignore` — line 144 `coverage/*`, line 191 `**/[Pp]ackages/*`. Confirms `packages/` and `coverage/` stay untracked, so no restore or coverage run dirties the tree.
27. `scripts/vscode/Invoke-Restore.ps1` — parameters `-SolutionPath` (default `TaskMaster.sln`), `-Configuration`, `-Platform`; vswhere resolution at lines 22-30; restore command at line 36 with `/t:Restore /p:RestorePackagesConfig=true /m`.
28. `scripts/vscode/Invoke-MSTestWithCoverage.ps1` — parameters `-SearchRoot`, `-Configuration`, `-CoverageOutput`, `-NoExecute`; search-root resolution at lines 271-272; discovery filter at lines 296-302 (excludes `\obj\` and `\ref\` only, which is the basis for D9); `dotnet-coverage` presence check at lines 292-293; prints `Discovered N test assemblies.` at line 315; `Assert-CoberturaLineCoverageThreshold` called at line 341 before the post-processed `Set-Content` at line 343.
29. `scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1` — lines 459-491 `Assert-CoberturaLineCoverageThreshold` throws when the root `<coverage>` `line-rate` times 100 is below 80. This is the basis for the authorized non-zero branch in P0-T11 and for D5.
30. `scripts/vscode/` directory listing — `Install-RepoDotNetSdk.ps1`, `Invoke-Restore.ps1`, `Invoke-MSTest.ps1`, `Invoke-MSTestWithCoverage.ps1`, and `TaskMaster.cli.runsettings`'s sibling scripts all present.
31. `docs/features/active/2026-09-02-test-determinism-and-hygiene-debt-729/issue.md` — line 12 `- Work Mode: full-bug`, which resolves the mode per the contract's precedence rule.
32. `docs/features/active/2026-09-02-test-determinism-and-hygiene-debt-729/spec.md` — 21 acceptance criteria at lines 256-276, one per line, IDs AC1 through AC21 with no duplicates and no gaps.

Sibling-region re-checks performed in revision round 2:

- Around P6-T5's TRX confirmation: the two assemblies it names, `UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll` and `SVGControl.Test\bin\Debug\SVGControl.Test.dll`, are exactly the outputs P4-T5 and P3-T3 assert with `Test-Path`, so both exist by the time Phase 6 runs. Each `/Logger:trx` form carries both `/ResultsDirectory` and an explicit `LogFileName`, and writes into a task-specific subdirectory under distinct filenames, so the two runs cannot collide with each other or with any other TRX. Superseded in round 3 by the B5 split into two single-assembly runs; re-derived as S3 below.
- Around P6-T9's widened staging: the seventh path, `UtilitiesCS.Test/OutlookObjects/Filter DASL/DASLFilterParserTests.cs`, contains a space and is single-quoted in the `git add` span, matching the quoting P5-T8 already uses for the same path.
- Around P7-T7's new `spec.md` edit: `P7-T2` (AC18) classifies every changed path as production source, test project asset, or feature documentation and evidence — `spec.md` falls in the third class, so the AC18 "exactly one production source path" acceptance is unaffected. `P7-T5` asserts exactly seventeen `D` entries; a content edit to `spec.md` adds an `M` entry, not a `D` entry, and `spec.md` is already named in the Complete file-write inventory, whose wording this revision updated to cover the Block L insertion.
- Around P8-T22's amend: the amended commit is local and unpushed at that point in the plan, and `P8-T22` is the last task in the plan, so no later task depends on the pre-amend commit hash.
- Around P0-T6's new absence assertions: the artifact enumerates analyzer `Include` values resolved against each declaring project's own directory, which are repository-relative by construction, so the `:\` and account-name absence checks are satisfiable by the artifact this task is instructed to write rather than being a constraint it cannot meet.

Sibling-region re-checks performed in the original authoring pass:

- Around `NonBlockingDelay.cs` lines 42-66: the class remarks at lines 10-30 reference `<see cref="Timer"/>` and `<see cref="WaitAsync(TimeSpan)"/>`. Block A rewrites the remarks so no `cref` points at a type the file no longer constructs, while keeping `<see cref="WaitAsync(TimeSpan)"/>` valid against the surviving 1-arg overload. No `.csproj` in the solution sets `DocumentationFile`, so no CS0419 ambiguity arises from the added overload.
- Around `StoreRehookCoordinator.cs` line 102: the sibling XML doc at line 72 carries `<see cref="NonBlockingDelay.WaitAsync"/>` with no parameter list. That cref stays valid and is unaffected by the added overload for the same `DocumentationFile` reason. The plan does not edit that file.
- Around `SVGControl.Test/SVGControl.Test.csproj` lines 55-66: the sibling `<Compile>` entries for `GetRelativePath_Test.cs`, `RelativePathCoverageTests.cs`, `SvgAssemblyProbeDirectoryTests.cs`, `SvgRendererNullToleranceTests.cs`, `SvgRendererParseContractTests.cs`, `Properties\AssemblyInfo.cs`, and `Resources.Designer.cs` are untouched by Block H, and the sibling `<EmbeddedResource Include="Resources.resx">` at lines 92-95 is untouched by Block H's resource removals.
- Around `UtilitiesCS.Test/UtilitiesCS.Test.csproj` line 76: the sibling entries at lines 73-75 and 77 are untouched by the Block G insertion.
- Around the two Finding 3 classes: `UtilitiesCS.Test/HelperClasses/NLogTraceWriter_Test.cs` also captures and restores `Console.Out` but asserts through Moq rather than on captured text, so it has no failing mode of its own and is deliberately left unmarked per spec lines 90 and 293. This plan does not touch it.
- Around the deleted `UtilitiesCS.Test/OutlookObjects/DASLFilterParser_Tests.cs`: its sibling `UtilitiesCS.Test/OutlookObjects/Filter DASL/DASLFilterParserTests.cs` is the compiled copy at csproj line 270 and is retained; deleting the orphan removes no compiled test.

---

## Planner Internal Review Record

PLANNER-INTERNAL-REVIEW: PASS

CITATION-TO-TREE: PASS
AC-TRACEABILITY: PASS
SCOPE-BOUNDARY: PASS

CITATION: TaskMaster/AppGlobals/NonBlockingDelay.cs | line 42 `public static Task WaitAsync(TimeSpan delay)`
CITATION: TaskMaster/AppGlobals/NonBlockingDelay.cs | lines 52-54 nullable annotations pragma pair around `Timer? timer = null;`
CITATION: TaskMaster/AppGlobals/StoreRehookCoordinator.cs | line 102 `_delay = delay ?? NonBlockingDelay.WaitAsync;`
CITATION: TaskMaster/AppGlobals/AppEvents.cs | line 456 `await NonBlockingDelay.WaitAsync(TimeSpan.FromMilliseconds(100));`
CITATION: TaskMaster/TaskMaster.csproj | lines 148-149 Microsoft.Bcl.TimeProvider reference already present
CITATION: TaskMaster/packages.config | line 16 Microsoft.Bcl.TimeProvider 10.0.11 already pinned
CITATION: TaskMaster.Test/AppGlobals/NonBlockingDelayTests.cs | line 2 `using System.Diagnostics;` and lines 38-39 Stopwatch arrangement
CITATION: TaskMaster.Test/TaskMaster.Test.csproj | line 73 close of Microsoft.Bcl.AsyncInterfaces and line 121 close of Microsoft.Extensions.Primitives
CITATION: TaskMaster.Test/packages.config | line 17 Microsoft.Bcl.AsyncInterfaces and line 82 Microsoft.Extensions.Primitives
CITATION: TaskMaster.Test/app.config | lines 267-271 Microsoft.Bcl.TimeProvider binding redirect already present
CITATION: UtilitiesCS.Test/UtilitiesCS.Test.csproj | line 76 `<Compile Include="TestAssemblyInitializer.cs" />` and line 270 the compiled DASL test
CITATION: UtilitiesCS.Test/Properties/AssemblyInfo.cs | lines 18-21 assembly Parallelize ClassLevel
CITATION: UtilitiesCS.Test/HelperClasses/PrettyPrint_Tests.cs | lines 14-20 precedent hazard comment and `[DoNotParallelize]`
CITATION: UtilitiesCS.Test/OutlookObjects/Table/OlTableExtensions_Tests.cs | lines 17-21 precedent hazard comment and `[DoNotParallelize]`
CITATION: UtilitiesCS.Test/OutlookObjects/Filter DASL/DASLFilterParserTests.cs | line 8 `[TestClass]` and lines 101-111 Console.Out capture and restore
CITATION: UtilitiesCS.Test/ReusableTypeClasses/StackGeek_Tests.cs | line 9 `[TestClass]` and lines 144-155 Console.Out capture and restore
CITATION: SVGControl.Test/SVGControl.Test.csproj | lines 55-66 four Form Compile entries inside the ItemGroup opened at line 54, and lines 86-91 two Form EmbeddedResource entries
CITATION: SVGControl.Test/SVGControl.Test.csproj | line 133 FluentAssertions 8.10.0, line 233 MSTest.TestFramework 4.3.3, line 344 System.Windows.Forms
CITATION: QuickFiler.Test/NoLiveFormInTestAssemblyTests.cs | lines 1-54 the guard source ported verbatim into Blocks E and F
CITATION: UtilitiesCS/Threading/ThreadMonitor.cs | line 43 `private ITimer? _pollTimer;` proving ITimer availability on net481
CITATION: scripts/vscode/Invoke-Restore.ps1 | lines 22-36 vswhere-resolved MSBuild restore with RestorePackagesConfig
CITATION: scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1 | lines 459-491 Assert-CoberturaLineCoverageThreshold 80 percent throw
CITATION: scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1 | line 489 throw text containing `is below the required 80% threshold`
CITATION: scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1 | lines 417-447 ConvertTo-KoverageCoberturaXml package removal and six root aggregate SetAttribute calls
CITATION: scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1 | lines 417-424 non-allowlisted package RemoveChild loop followed by the `//class[@filename]` rewrite to a repository-relative path
CITATION: docs/features/active/2026-08-07-efcviewer-missing-lineage-and-segment-navigation-439/evidence/qa-gates/issue-439-final.cobertura.xml | 2,114 letter-anchored absolute-path lines, of which 15 are Mono.Reflection sources under a drive-level `sources` directory
CITATION: docs/features/active/2026-08-25-quickfiler-high-confidence-partial-screen-backfill-608/evidence/qa-gates/r1-csharp-coverage.2026-08-25T12-33.cobertura.xml | same raw-output signature, 2,114 letter-anchored lines and 15 drive-level `sources` lines
CITATION: docs/features/active/2026-08-25-quickfiler-high-confidence-partial-screen-backfill-608/evidence/baseline/r3-csharp-coverage.2026-08-25T13-32.cobertura.xml | same raw-output signature, 2,114 letter-anchored lines and 15 drive-level `sources` lines
CITATION: scripts/vscode/Invoke-MSTestWithCoverage.ps1 | line 76 one aggregate inner vstest invocation over the whole assembly set, carrying no /Logger: and no /ResultsDirectory and reporting no per-assembly counts
CITATION: scripts/vscode/Invoke-MSTestWithCoverage.ps1 | lines 296-302 discovery selects absolute FullName values, and line 315 prints `Discovered N test assemblies.`
CITATION: scripts/vscode/Invoke-MSTestWithCoverage.ps1 | lines 118-136 Get-DerivedCoverageSettingsPath returns `.effective-coverage.config` adjacent to the output
CITATION: scripts/vscode/Invoke-MSTestWithCoverage.ps1 | lines 232-242 throw on any non-zero coverage exit code plus the finally-block removal of the derived settings file
CITATION: scripts/vscode/Invoke-MSTestWithCoverage.ps1 | lines 326-343 collection, conversion at 340, threshold assertion at 341, processed write at 343
CITATION: scripts/vscode/TaskMaster.cli.runsettings | present in tree; the /Settings: file used by every scoped run in this plan
CITATION: .gitignore | line 144 `coverage/*` and line 145 `!coverage/.gitkeep`
CITATION: .csharpierignore | line 4 `**/evidence/**`, line 8 `*.trx`, and line 12 `*.csproj`
CITATION: docs/features/active/2026-09-02-test-determinism-and-hygiene-debt-729/research/research-729.2026-09-02T09-30.md | lines 238-252 section 4.2 and its four numbered reasons at 242, 248, 250, 252
CITATION: docs/features/active/2026-09-02-test-determinism-and-hygiene-debt-729/spec.md | line 87 Finding 4 out-of-scope bullet citing four reasons without enumerating them, with line 88 the next top-level bullet marking the P7-T7 insertion point
CITATION: docs/features/active/2026-09-02-test-determinism-and-hygiene-debt-729/spec.md | line 275 AC20 unqualified `.claude/**` no-change wording
CITATION: docs/features/active/2026-09-02-test-determinism-and-hygiene-debt-729/spec.md | line 276 AC21 requires csharpier check to report no unformatted files
CITATION: docs/features/active/2026-09-02-test-determinism-and-hygiene-debt-729/spec.md | line 267 AC12 both guards passing in the final full test run
CITATION: docs/features/active/2026-09-02-test-determinism-and-hygiene-debt-729/issue.md | line 12 `- Work Mode: full-bug`
CITATION: docs/features/active/2026-09-02-test-determinism-and-hygiene-debt-729/spec.md | lines 256-276 acceptance criteria AC1 through AC21

AC-INVENTORY: AC1, AC2, AC3, AC4, AC5, AC6, AC7, AC8, AC9, AC10, AC11, AC12, AC13, AC14, AC15, AC16, AC17, AC18, AC19, AC20, AC21

AC-MAPPING: AC1 | IMPLEMENTATION: P1-T1 | TESTS: P2-T4 | EVIDENCE: P8-T1
AC-MAPPING: AC2 | IMPLEMENTATION: P1-T1 | TESTS: P2-T4 | EVIDENCE: P8-T2
AC-MAPPING: AC3 | IMPLEMENTATION: P1-T2 | TESTS: P6-T4 | EVIDENCE: P8-T3
AC-MAPPING: AC4 | IMPLEMENTATION: P1-T1 | TESTS: P6-T3 | EVIDENCE: P8-T4
AC-MAPPING: AC5 | IMPLEMENTATION: P2-T1 | TESTS: P2-T4 | EVIDENCE: P8-T5
AC-MAPPING: AC6 | IMPLEMENTATION: P2-T2 | TESTS: P2-T4 | EVIDENCE: P8-T6
AC-MAPPING: AC7 | IMPLEMENTATION: P1-T3 | TESTS: P1-T6 | EVIDENCE: P8-T7
AC-MAPPING: AC8 | IMPLEMENTATION: P4-T2 | TESTS: P4-T5 | EVIDENCE: P8-T8
AC-MAPPING: AC9 | IMPLEMENTATION: P4-T3 | TESTS: P4-T6 | EVIDENCE: P8-T9
AC-MAPPING: AC10 | IMPLEMENTATION: P3-T5 | TESTS: P3-T7 | EVIDENCE: P8-T10
AC-MAPPING: AC11 | IMPLEMENTATION: P3-T1 | TESTS: P3-T4 | EVIDENCE: P8-T11
AC-MAPPING: AC12 | IMPLEMENTATION: P3-T6 | TESTS: P6-T5 | EVIDENCE: P8-T12
AC-MAPPING: AC13 | IMPLEMENTATION: P5-T1 | TESTS: P5-T6 | EVIDENCE: P8-T13
AC-MAPPING: AC14 | IMPLEMENTATION: P5-T5 | TESTS: P5-T4 | EVIDENCE: P8-T14
AC-MAPPING: AC15 | IMPLEMENTATION: P5-T7 | TESTS: P5-T6 | EVIDENCE: P8-T15
AC-MAPPING: AC16 | IMPLEMENTATION: P7-T7 | TESTS: P8-T16 | EVIDENCE: P0-T2
AC-MAPPING: AC17 | IMPLEMENTATION: P7-T1 | TESTS: P6-T5 | EVIDENCE: P8-T17
AC-MAPPING: AC18 | IMPLEMENTATION: P7-T2 | TESTS: P7-T5 | EVIDENCE: P8-T18
AC-MAPPING: AC19 | IMPLEMENTATION: P7-T3 | TESTS: P7-T5 | EVIDENCE: P8-T19
AC-MAPPING: AC20 | IMPLEMENTATION: P7-T4 | TESTS: P7-T5 | EVIDENCE: P8-T20
AC-MAPPING: AC21 | IMPLEMENTATION: P6-T8 | TESTS: P6-T5 | EVIDENCE: P8-T21

UNRESOLVED-GAPS: NONE
