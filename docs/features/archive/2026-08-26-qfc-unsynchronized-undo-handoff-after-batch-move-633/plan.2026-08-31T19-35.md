# 2026-08-26-qfc-unsynchronized-undo-handoff-after-batch-move (Plan)

- **Issue:** #633
- **Parent (optional):** none
- **Owner:** drmoisan
- **Last Updated:** 2026-09-01T11-22
- **Status:** Complete
- **Version:** 1.0
- **Work Mode:** full-bug — `spec.md` is the sole acceptance-criteria source. `user-story.md` does not
  exist for this issue and must not be created or required.

**Fail-closed evidence rule:** Every baseline task, final-QA task, and coverage-comparison task in this
plan produces a named artifact. If any required artifact is missing or has an incomplete field set, the
verdict is BLOCKED or INCOMPLETE, never PASS.

**Evidence accounting rule:** Each evidence-producing task names its artifact path. Do not check a task
box without the artifact on disk.

---

## Conventions used by this plan

- `FEATURE` denotes
  `docs/features/active/2026-08-26-qfc-unsynchronized-undo-handoff-after-batch-move-633`.
  All evidence paths in this plan resolve to `FEATURE/evidence/<kind>/` and no other location.
  `artifacts/baselines/`, `artifacts/qa/`, `artifacts/coverage/`, and `artifacts/evidence/` are invalid
  here and non-overridable.
- `TIMESTAMP` in an artifact filename denotes the ISO-8601 instant `yyyy-MM-ddTHH-mm` at which that task
  ran. Example concrete name: `p0-t7-csharpier-check.2026-08-31T21-05.md`.
- Every command-step artifact carries, at minimum, four fields: `Timestamp:`, `Command:`, `EXIT_CODE:`,
  `Output Summary:`. Baseline test artifacts additionally carry numeric coverage headline values.
  `UNVERIFIED` is not an acceptable value for any of these fields.
- `WORKTREE` denotes `<repo-root>/.claude/worktrees/agent-a2cb3799bdac5110d`.
  Every command in this plan runs with that directory as the working directory.
  (Execution correction, applied 2026-09-01: the authored value named a different worktree,
  `agent-ad3ffa06b9103d4cc`, which is not the checkout this plan is executed in. The value above is the
  worktree that actually holds branch `bug/qfc-unsynchronized-undo-handoff-after-batch-move-633`.)

## Executor environment notes (read before Phase 0)

1. **Invoke every C# tool through `pwsh`, not through the Bash tool.** The Bash tool rewrites MSBuild
   switches: `/m` becomes `M:/` and MSBuild fails with MSB1008. This worktree is isolated, so compound
   Bash commands are refused. Keep every command task simple and single-purpose.
2. **`/t:Rebuild`, never `/t:Build`.** MSBuild's up-to-date check does not invalidate on a command-line
   `/p:` change, so a warm `/t:Build` returns exit 0 with `CoreCompile` skipped on every project and
   runs no analyzers. The gate becomes vacuous.
3. **Never add `/p:Nullable=enable`.** No project in this repository carries a `<Nullable>` element and
   there is no `Directory.Build.props`. Forcing the property conscripts every file that never adopted
   the pragma; CI omits it deliberately.
4. **Never add `#nullable enable` to either in-scope production file.** Neither
   `QuickFiler/Controllers/FilerQueue.cs` nor `QuickFiler/Controllers/QfcFormController.EventHandlers.cs`
   carries the directive today (verified: zero matches). Nullable enforcement in this repository is
   per-file opt-in and `/p:TreatWarningsAsErrors=true` promotes `CS86xx` to errors in any file that
   opts in.
5. **Use the wrapper `scripts/vscode/Invoke-MSTestWithCoverage.ps1` for test runs, not a bare
   `vstest.console.exe` full-suite run.** The wrapper appends `/Settings`, `/InIsolation`, and
   `/TestCaseFilter:TestCategory!=LiveOutlook` (script line 76). A bare full-suite call omits the
   category filter and runs `LiveOutlookHookupIntegrationTests`, which launches a real Outlook process —
   an external process, prohibited by `.claude/rules/general-unit-test.md` — and produces results that
   are not comparable to the wrapper-produced baseline. Always pass `-SearchRoot .` explicitly.
   The single exception is the scoped single-assembly run defined in P5-T10 and P7-T8, which names one
   assembly path and one `FullyQualifiedName` filter and therefore performs no discovery.
   **`vstest.console.exe` is not on `PATH` in this worktree.** Every scoped run task in this plan is
   written as "the absolute path recorded by P0-T14"; substitute the concrete path that task resolved.
   `vswhere`, `msbuild`, `nuget`, and `dotnet-coverage` all resolve without help.
   **Every scoped run passes an explicit TRX file name and the whole switch is double-quoted.** A bare
   `/Logger:trx` makes vstest name the TRX file after the current account and host, and
   `.claude/agent-memory/_shared_no_absolute_host_paths.md` prohibits an account or machine name in any
   committed artifact. Content sanitisation does not reach a file name, so the name must be neutral at
   the point the run produces it. The eight scoped runs therefore pass
   `"/Logger:trx;LogFileName=p1-t5.trx"`, `"/Logger:trx;LogFileName=p2-t5.trx"`,
   `"/Logger:trx;LogFileName=p4-t6.trx"`, `"/Logger:trx;LogFileName=p5-t10.trx"`,
   `"/Logger:trx;LogFileName=p6-t8.trx"`, `"/Logger:trx;LogFileName=p6-t9.trx"`,
   `"/Logger:trx;LogFileName=p6-t10.trx"`, and `"/Logger:trx;LogFileName=p7-t8.trx"` respectively. The
   double quotes are required rather than cosmetic: an unquoted semicolon terminates the argument in
   `pwsh`, so the unquoted form passes `/Logger:trx` alone and silently restores the account-named file.
6. **.NET Framework 4.8.1 constraints.** No `init` accessor, no `record`, no `record struct` — this
   repository has no `IsExternalInit` polyfill and each fails with CS0518. `TaskCompletionSource<bool>`
   and `TaskCreationOptions.RunContinuationsAsynchronously` are available; the parameterless
   non-generic `TaskCompletionSource` is not and must not be used.
7. **Determinism.** `.claude/rules/general-unit-test.md` bans `Thread.Sleep`, `Task.Delay`, and every
   wall-clock wait in test code. Every concurrency assertion added by this plan is driven through the
   `internal Func<FilerQueueItem, Task> ItemProcessor` seam using `TaskCompletionSource<bool>` gates.
   Verified baseline: `QuickFiler.Test/Controllers/FilerQueueTests.cs` and
   `QuickFiler.Test/Controllers/QfcItemController.SeamFactoryTests.cs` contain zero matches for
   `Thread\.Sleep|Task\.Delay|\.Wait\(|\.Result\b|DateTime\.(Now|UtcNow)` today, so the AC14 gate is
   satisfiable. The dispatcher-facing assertions in P2-T3 and P5-T9 are ordered by an equal-priority
   probe operation on a pinned dispatcher rather than by any elapsed-time assumption.
8. **MSBuild's file logger does not create intermediate directories.** A `/flp:logfile=` target
   terminates the build with MSB1029 when the directory part of the target path does not exist. Create
   the directory with `New-Item -ItemType Directory -Force -Path` and the directory path before the
   first msbuild task that logs into it.
9. **`.gitignore:84` is `*.log`.** Every MSBuild file log this plan produces is therefore written with a
   `.msbuild.txt` extension rather than `.log`, so the committed evidence includes the log itself and
   not only the counts derived from it. Editing `.gitignore` is outside the authorized blast radius.
10. **Never dispose the `UiThreadDispatcherFixture` transaction implicitly.** Every acquisition of
   `BeginTransactionAsync()` in this plan sits inside a `using` statement, written as one physical
   line so it stays greppable. The gate is a `SemaphoreSlim(1, 1)` released only by `Dispose`, and
   neither `QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixture.cs` nor the only
   file this plan makes acquire the transaction,
   `QuickFiler.Test/Controllers/QfcFormControllerUndoHandoffTests.cs`, carries a `[Timeout]` attribute,
   so a permit leaked on an assertion-failure path hangs the assembly run rather than failing it. The
   claim is scoped to those two files deliberately:
   `QuickFiler.Test/Controllers/QfcItemController.SeamFactoryTests.cs` is also in the blast radius and
   does carry `[Timeout(PumpTimeoutMs)]` at lines 304 and 375, but it acquires no transaction, so the
   leak mechanism does not reach it.
   Do not append `.ConfigureAwait(false)` to the acquisition. CSharpier breaks that chained form onto
   its own line — `QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixtureTests.cs:108-110`
   is the worked instance in this repository — which would leave `BeginTransactionAsync` on a line
   carrying no `using (` and falsify the P2-T3 and P5-T9 acceptance checks. The single-line form
   `using (var transaction = await UiThreadDispatcherFixture.BeginTransactionAsync())` is 80 characters
   plus indentation, so 92 or 96 characters at twelve- or sixteen-space indentation; this repository has
   no `.csharpierrc` file, so CSharpier's default 100-column print width applies and the line is not
   wrapped.

## Coverage denominator (read before any coverage figure is recorded)

`scripts/vscode/Invoke-MSTestWithCoverage.ps1` produces two structurally different Cobertura files
depending on whether the run was green:

- **Green run.** `Invoke-DotnetCoverageCollection` returns, then the script post-processes
  (`ConvertTo-KoverageCoberturaXml`, helper line 393). Post-processing removes every `package` whose
  `name` is not a first-party project assembly name, injects a `sources` element, and rewrites
  `line-rate`, `branch-rate`, `lines-covered`, `lines-valid`, `branches-covered`, and `branches-valid`
  from the filtered set (helper lines 441-447). This filtered figure is the policy denominator.
- **Any failing test, or a filtered line rate below 80 percent.**
  `Invoke-DotnetCoverageCollection` throws at script line 236, or
  `Assert-CoberturaLineCoverageThreshold` throws at helper line 489, in both cases *before*
  `Set-Content` at script line 343. The file left on disk is therefore the **raw, unfiltered**
  dotnet-coverage output, which includes vendored packages such as `log4net` and `Mono.Reflection` and
  reads roughly fifteen points lower than the filtered figure.

**Consequence, and a rule this plan enforces:** comparing a coverage figure taken from a red run against
a figure taken from a green run manufactures a phantom regression. Baseline and final-QA coverage
figures may be compared **only when both runs are green**. Every task in this plan that records a
coverage figure must first record the sorted list of `package` `name` attribute values found in the
produced XML and state whether any of them is a vendored third-party assembly. A file whose package list
contains a third-party name is an unfiltered file and its `line-rate` must be recorded as
`DENOMINATOR: UNFILTERED` and must not be compared to a filtered figure.

## Authorized blast radius (do not exceed, do not pad)

Production, exactly two files:

1. `QuickFiler/Controllers/FilerQueue.cs`
2. `QuickFiler/Controllers/QfcFormController.EventHandlers.cs`

Test and project, exactly four files:

1. `QuickFiler.Test/Controllers/FilerQueueTests.cs` (extend)
2. `QuickFiler.Test/Controllers/QfcFormControllerUndoHandoffTests.cs` (new)
3. `QuickFiler.Test/Controllers/QfcItemController.SeamFactoryTests.cs` (repair)
4. `QuickFiler.Test/QuickFiler.Test.csproj` (one `Compile Include` entry)

Documentation updated by this plan: `FEATURE/plan.2026-08-31T19-35.md`, `FEATURE/spec.md` (AC check-off
only), `FEATURE/issue.md` (status note only), and files under `FEATURE/evidence/`. No file under
`.claude/rules/`, `.claude/skills/`, or `CLAUDE.md` may be edited.

## Verified tree facts this plan depends on

| Fact | Location | Verified value |
|---|---|---|
| `FilerQueue.cs` total lines | `QuickFiler/Controllers/FilerQueue.cs` | 83 |
| `Enqueue(FilerQueueItem)` adds before reading the guard | `QuickFiler/Controllers/FilerQueue.cs` | `Queue.Add` at 24, `guard.CheckAndSetFirstCall` at 25 |
| `Enqueue(EmailFiler, IList)` adds before reading the guard | `QuickFiler/Controllers/FilerQueue.cs` | `Queue.Add` at 33, `guard.CheckAndSetFirstCall` at 34 |
| `guard` private field | `QuickFiler/Controllers/FilerQueue.cs` | line 40 |
| `Consumer` declaration | `QuickFiler/Controllers/FilerQueue.cs` | line 42, `public Task Consumer { get; private set; } = Task.CompletedTask;` |
| Worker loop exit precedes guard reinstall | `QuickFiler/Controllers/FilerQueue.cs` | `while (Queue.TryTake(out var item))` at 48, `guard = new ThreadSafeSingleShotGuard();` at 63 |
| Hard-coded per-item call | `QuickFiler/Controllers/FilerQueue.cs` | line 52 |
| Existing per-item catch and diagnostic | `QuickFiler/Controllers/FilerQueue.cs` | lines 54-61 |
| `EmailFiler.SortAsync(IList)` return type | `UtilitiesCS/EmailIntelligence/EmailParsingSorting/EmailFiler.cs` | line 128, `Task<bool>`, assignable to `Task` |
| `QfcFormController.EventHandlers.cs` total lines | `QuickFiler/Controllers/QfcFormController.EventHandlers.cs` | 399 |
| `BackGroundMoveAsync` span | `QuickFiler/Controllers/QfcFormController.EventHandlers.cs` | 215-234 |
| Early-return guard, no `_parent` clause | `QuickFiler/Controllers/QfcFormController.EventHandlers.cs` | line 219 |
| Batch move await | `QuickFiler/Controllers/QfcFormController.EventHandlers.cs` | line 225 |
| `WriteMetrics` dispatch | `QuickFiler/Controllers/QfcFormController.EventHandlers.cs` | lines 228-231 |
| `CleanupBackground` dispatch | `QuickFiler/Controllers/QfcFormController.EventHandlers.cs` | line 233 |
| The only two `.Consumer` reads under `QuickFiler/` | `QuickFiler/Controllers/QfcFormController.EventHandlers.cs` | lines 167 and 193 |
| `_parent` is nulled during cleanup | `QuickFiler/Controllers/QfcFormController.SetupDisposal.cs` | line 224 |
| `WriteMetrics` is a private field of a private delegate type | `QuickFiler/Controllers/QfcFormController.cs` | delegate declared 82, field 83 |
| `_parent` field type | `QuickFiler/Controllers/QfcFormController.cs` | line 81, `IQfcHomeController` |
| Reflection into the `guard` field, which the fix removes | `QuickFiler.Test/Controllers/QfcItemController.SeamFactoryTests.cs` | lines 213-215; `ThreadSafeSingleShotGuard._state` set at 216-218; `Queue.Count` asserted at 234 |
| `SeamFactoryTests.cs` total lines (64 lines of headroom under 500) | `QuickFiler.Test/Controllers/QfcItemController.SeamFactoryTests.cs` | 436 |
| `FilerQueueTests.cs` total lines | `QuickFiler.Test/Controllers/FilerQueueTests.cs` | 89 |
| Retained-default assertion that must keep passing unmodified | `QuickFiler.Test/Controllers/FilerQueueTests.cs` | `FilerQueue_NewInstance_HasCompletedConsumerByDefault`, lines 76-87 |
| Class comment recording the deliberate `Enqueue`/`ConsumeAsync` exclusion | `QuickFiler.Test/Controllers/FilerQueueTests.cs` | lines 12-19 |
| Explicit compile items in the test project | `QuickFiler.Test/QuickFiler.Test.csproj` | `Controllers\FilerQueueTests.cs` at 113, `Controllers\QfcFormControllerTests.cs` at 147 |
| Internals reachable from tests | `QuickFiler/Properties/AssemblyInfo.cs` | line 5, `InternalsVisibleTo("QuickFiler.Test")` |
| Reflection helpers and construction fixture to mirror | `QuickFiler.Test/Controllers/QfcFormControllerSeamTests.cs` | `GetPrivateField`/`SetPrivateField` at 43-47, `CreateQfcFormController` at 64-76 |
| Pumping STA dispatcher helpers | `QuickFiler.Test/Controllers/QfcItemController.TestSupport.cs` | `StartRunningDispatcher` at 251, `ShutdownDispatcher` at 277 |
| Dispatcher static ownership and transaction gate | `QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixture.cs` | `EnsureDispatcher` at 99, `BeginTransactionAsync` at 122, `Install` at 242 |
| SDK pin and repo-local SDK path | `global.json` | version `8.0.205`, `paths` are `.dotnet-sdk` and `$host$` |
| CSharpier pin | `dotnet-tools.json` | `csharpier` `1.2.6`, `isRoot` true, repository root manifest |
| `.dotnet-sdk` present in this worktree | worktree root | ABSENT |
| `packages/` present in this worktree | worktree root | ABSENT |
| `vstest.console.exe` resolvable from `PATH` | `WORKTREE` | ABSENT — resolve it in P0-T14 |
| MSBuild file logs are ignored by git | `.gitignore` | line 84, `*.log` |
| Repo-sanctioned restore covers `packages.config` and `PackageReference` | `scripts/vscode/Invoke-Restore.ps1` | line 36, `/t:Restore /p:RestorePackagesConfig=true /m` |
| A discarded `EnsureUiThreadDispatcher()` scope leaks a non-null static | `QuickFiler.Test/Controllers/QfcItemController.TestSupport.cs` | lines 229-236 document the leak; `QuickFiler.Test/Controllers/QfcItemController.FocusAndThemeTests.cs` discards the scope at 452 and 468 |
| Both dispatcher calls in `BackGroundMoveAsync` are awaited | `QuickFiler/Controllers/QfcFormController.EventHandlers.cs` | `await UiThread.Dispatcher.InvokeAsync(` at 228 and 233 |
| Transaction gate is released only by `Dispose` | `QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixture.cs` | `BeginTransactionAsync` waits the `SemaphoreSlim(1,1)` at 122-126; `UiThreadDispatcherTransaction.Dispose` releases it at 261-276 |

## Fail-before strategy, stated honestly

`CLAUDE.md`'s Bugfix Workflow requires a failing regression test before the fix. This plan splits the
requirement into a real failing run and a documented exception, because the two halves of the defect
differ in what can be observed before the fix exists.

**A. Real failing run — the barrier defect (issue #633's actual subject).** With only the
behaviour-preserving `ItemProcessor` seam in place (Phase 1), a test can hold one enqueued item inside a
gated processor, call `BackGroundMoveAsync` without awaiting it, and observe whether the method
dispatched to the UI dispatcher while the queue was still undrained. That test compiles against the
Phase-1 tree, is deterministic, fails before the fix, passes after it, and survives verbatim. Phase 2
produces the failing-run artifact.

The discriminator does **not** depend on the ambient value of the static `UiThread.Dispatcher`, and this
plan does not use a null static as a witness.
`QuickFiler.Test/Controllers/QfcItemController.FocusAndThemeTests.cs:452` and `:468` both call
`QfcItemControllerTestSupport.EnsureUiThreadDispatcher()` and discard the returned scope, and
`QuickFiler.Test/Controllers/QfcItemController.TestSupport.cs:229-236` records that the scope reverts
the seeding only while the static still holds the exact instance that call installed, and that
discarding the scope is permitted and leaks. Once either of those tests has run, the static is non-null
for the remainder of the `QuickFiler.Test` assembly run. `.claude/rules/general-unit-test.md` requires
order-independence, so a witness whose mechanism depends on ambient static state left by unrelated tests
is not acceptable.

The dispatcher is therefore pinned rather than observed. Each barrier test takes
`UiThreadDispatcherFixture.BeginTransactionAsync()` inside a `using` statement and installs a running STA
dispatcher obtained from `QfcItemControllerTestSupport.StartRunningDispatcher()`, so the test runs
against a known pumping dispatcher in both worlds.

The deterministic edge is queue order on that dispatcher, not elapsed time:

- `_groups.MoveEmailsAsync(_movedItems)` is mocked to return `Task.CompletedTask`, so the `await` at
  `QuickFiler/Controllers/QfcFormController.EventHandlers.cs:225` completes synchronously and execution
  continues on the calling thread with no yield.
- Pre-fix the next statement is the awaited
  `UiThread.Dispatcher.InvokeAsync(..., DispatcherPriority.ContextIdle)` at `:228`, so the metrics
  operation is enqueued on the dispatcher **before `BackGroundMoveAsync()` returns to its caller**; that
  awaited `DispatcherOperation` is the method's first genuine yield.
- Post-fix the next statement is `await _parent.FilerQueue.WhenDrainedAsync()`, which is incomplete
  while the gate is closed, so the method yields there and nothing is posted to the dispatcher.

After the call returns, the test posts its own probe operation to that same dispatcher instance at
`DispatcherPriority.ContextIdle` and awaits it. A WPF dispatcher processes operations of equal priority
in enqueue order, so pre-fix the metrics operation runs strictly before the probe completes. Awaiting
the probe converts "has the method dispatched yet" from a timing question into an ordering fact. With
the probe complete and the gate still closed, the metrics recorder count is 1 pre-fix and 0 post-fix.
That single assertion is what makes both Phase 2 tests fail deterministically before the fix; each test
then releases the gate, awaits the returned task, and asserts its own named guarantee.

The returned task's `IsCompleted` is deliberately **not** the discriminator. Both dispatcher calls at
`:228` and `:233` are awaited, so under a running installed dispatcher the pre-fix task is incomplete at
the moment of return exactly as the post-fix task is, and the two states are separated only by timing.

**B. Exception dossier — the queue-level drain suite and the orphan-window regression.** Two distinct
groups, both recorded in one dossier:

1. The `WhenDrainedAsync_*` tests assert an API that does not exist before Phase 3. A run of them
   against the pre-fix tree does not fail; it does not compile, which is not a clean fail-before.
2. `Enqueue_AfterPreviousBatchDrained_ProcessesSecondBatch` names `WhenDrainedAsync()` and therefore
   cannot compile before Phase 3 either, which places it in the same structural category as the
   `WhenDrainedAsync_*` group. It is additionally not convertible into a compilable pre-fix witness:
   the orphaned-item window requires a producer `Queue.Add` to land strictly between the worker's loop
   exit (`QuickFiler/Controllers/FilerQueue.cs:48`) and the guard reinstall (`:63`), and no seam, await,
   or observable state change exists between those two statements, so no test can place a statement into
   that interval deterministically. It is a post-fix regression guard, and it is a discriminating one:
   it does fail against a handshake that leaves the consumer-running flag set after loop exit.

This is a stated divergence from the delegating agent's expectation that the orphan window would carry
the real failing run and the drain barrier the dossier. The derivation above is recorded in the dossier
so a reviewer can check it rather than take it on assertion.

## Acceptance-criterion identifiers

`spec.md` lists twenty acceptance criteria as unnumbered bullets under `## Acceptance Criteria`. This
plan assigns stable identifiers AC1 through AC20 in document order. Each identifier is anchored to a
verbatim, single-line, unique fragment of its own bullet, so check-off cannot bind to the wrong bullet.

| ID | spec.md line | Same-line anchor fragment |
|---|---|---|
| AC1 | 580 | `exposes` |
| AC2 | 583 | `The drain task does not complete while any enqueued item` |
| AC3 | 587 | `The drain task completes once every enqueued item has completed` |
| AC4 | 590 | `is idempotent: repeated and concurrent waiters all complete` |
| AC5 | 593 | `The orphaned-item window is closed` |
| AC6 | 596 | `An item whose processing throws still decrements` |
| AC7 | 599 | `awaits` |
| AC8 | 604 | `The existing metrics-before-cleanup ordering is preserved` |
| AC9 | 607 | `The early-return guard in` |
| AC10 | 610 | `The two production reads of` |
| AC11 | 616 | `remains declared with the same type` |
| AC12 | 619 | `still raises` |
| AC13 | 622 | `is reconciled with the new` |
| AC14 | 625 | `contains no banned wait API` |
| AC15 | 630 | `is introduced` |
| AC16 | 634 | `The production diff touches no file other than` |
| AC17 | 638 | `contains a` |
| AC18 | 641 | `Both changed production files remain under 500 lines` |
| AC19 | 644 | `The full C# toolchain passes in a single uninterrupted pass` |
| AC20 | 651 | `Coverage does not regress on any line changed by this fix` |

Every check-off search is constrained to lines between the `## Acceptance Criteria` heading
(`spec.md:574`) and the `## Risks & Mitigations` heading (`spec.md:659`) that also match `^- \[[ x]\] `.
Three anchors — `exposes`, `awaits`, and `is introduced` — occur elsewhere in `spec.md` and resolve
uniquely only under that constraint: `exposes` also at `:375`; `awaits` at `:44`, `:73`, `:188`, `:237`,
`:239`, `:305`, `:671`, and `:733`; `is introduced` at `:316` and `:532`. Under the constraint each of
the twenty anchors matches exactly one line.

---

### Phase 0 — Baseline capture and worktree toolchain bootstrap

The three bootstrap tasks P0-T4, P0-T5, and P0-T6 are blocking, not optional. This is a
`.claude/worktrees/<agent-id>` worktree in which `.dotnet-sdk` and `packages/` are both absent, and
without them every `dotnet` and `msbuild` command fails and every downstream `EXIT_CODE: 0` acceptance
in this plan is unreachable. Do not add analyzer back-fill tasks: all sixteen projects and every
`packages.config` already agree on `Meziantou.Analyzer 3.0.194` and `Roslynator.Analyzers 5.0.0`.

- [x] [P0-T1] Read, in this order, `CLAUDE.md`, `.claude/rules/general-code-change.md`,
      `.claude/rules/general-unit-test.md`, `.claude/rules/quality-tiers.md`,
      `.claude/rules/tonality.md`, and `.claude/rules/csharp.md` if it exists. Write
      `FEATURE/evidence/baseline/phase0-instructions-read.TIMESTAMP.md` containing `Timestamp:`,
      `Policy Order:`, and the explicit list of files read with the line count of each.
      Acceptance: the artifact exists and lists at least five read files, each with a numeric line count.
- [x] [P0-T2] Read `FEATURE/spec.md` in full and transcribe its twenty acceptance-criteria bullets into
      `FEATURE/evidence/baseline/ac-inventory.TIMESTAMP.md`, one row per criterion, each row carrying
      the identifier AC1 through AC20, the spec line number, and the anchor fragment from the table
      above. Acceptance: the artifact contains exactly twenty rows with identifiers AC1 through AC20 and
      no duplicate identifier.
- [x] [P0-T3] Run `git fetch origin main` then `git merge-base origin/main HEAD` in `WORKTREE`. Record
      both commands and the resulting SHA, plus the output of `git rev-parse origin/main`, in
      `FEATURE/evidence/baseline/p0-t3-merge-base.TIMESTAMP.md` with `Timestamp:`, `Command:`,
      `EXIT_CODE:`, `Output Summary:`. Acceptance: the artifact records a 40-character hexadecimal
      merge-base SHA and `EXIT_CODE: 0` for `git merge-base`.
- [x] [P0-T4] Run `pwsh -File scripts/vscode/Install-RepoDotNetSdk.ps1` from `WORKTREE`, then run
      `dotnet --version`. Record both in
      `FEATURE/evidence/baseline/p0-t4-dotnet-sdk-bootstrap.TIMESTAMP.md` with the four required fields.
      Acceptance: the directory `.dotnet-sdk` exists in `WORKTREE` and the recorded `dotnet --version`
      output begins with `8.0.`.
- [x] [P0-T5] Run `pwsh -File scripts/vscode/Invoke-Restore.ps1` from `WORKTREE`. This is the
      repo-sanctioned restore: `scripts/vscode/Invoke-Restore.ps1:36` runs
      `msbuild /t:Restore /p:RestorePackagesConfig=true /m`, which covers both the `packages.config`
      projects and the `PackageReference` projects, where a bare `nuget restore` covers only the former.
      Record it in `FEATURE/evidence/baseline/p0-t5-restore.TIMESTAMP.md` with the four required fields.
      Acceptance: the artifact records `EXIT_CODE: 0` and the directory `packages` exists in `WORKTREE`.
      If the exit code is non-zero, record the verbatim terminating message and stop with
      `REMEDIATION-REQUIRED`; every `EnsureNuGetPackageBuildImports` `Error` target fires at
      `BeforeTargets="PrepareForBuild"`, so msbuild hard-fails until this succeeds.
- [x] [P0-T6] Run `dotnet tool restore` from `WORKTREE`, then `dotnet tool run csharpier --version`.
      Record both in `FEATURE/evidence/baseline/p0-t6-dotnet-tool-restore.TIMESTAMP.md` with the four
      required fields. The success-case output of the second command has not been observed on this
      worktree, and `CLAUDE.md` records that CSharpier v1 requires a subcommand, so the version switch
      may not be accepted. Fallback, and it is not optional: if the second command exits non-zero or
      prints no version string, record the verbatim error text in `Output Summary:` and, on a separate
      recorded line, the version value read from the `tools.csharpier.version` field of
      `dotnet-tools.json`, which is `1.2.6`. Acceptance: `dotnet tool restore` records `EXIT_CODE: 0`,
      and the artifact records either an observed version output of `1.2.6` or both the verbatim error
      text and the `1.2.6` value read from `dotnet-tools.json`.
- [x] [P0-T7] Run `dotnet tool run csharpier check .` from `WORKTREE`. Record it in
      `FEATURE/evidence/baseline/p0-t7-csharpier-check.TIMESTAMP.md` with the four required fields, and
      record in `Output Summary:` the verbatim final summary line the command printed and the count of
      files it reported as unformatted. Acceptance: the artifact records the observed exit code and a
      numeric count of unformatted files. A non-zero exit code here is recorded as pre-existing
      formatting drift, is not a failure of this plan, and is carried forward to P7-T2.
- [x] [P0-T8] Run
      `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true /fl "/flp:logfile=FEATURE/evidence/baseline/p0-t8-analyze.msbuild.txt;verbosity=normal"`.
      Record it in `FEATURE/evidence/baseline/p0-t8-analyze.TIMESTAMP.md` with the four required fields,
      plus the verbatim `Error(s)` and `Warning(s)` summary lines and the count of occurrences of the
      literal `Skipping target "CoreCompile"` in
      `FEATURE/evidence/baseline/p0-t8-analyze.msbuild.txt`. The `.msbuild.txt` extension is required:
      `.gitignore:84` is `*.log`, so a `.log` file would never be committed. Acceptance: the artifact
      records `EXIT_CODE: 0` and a `Skipping target "CoreCompile"` occurrence count of 0 in
      `FEATURE/evidence/baseline/p0-t8-analyze.msbuild.txt`.
- [x] [P0-T9] Run
      `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true /fl "/flp:logfile=FEATURE/evidence/baseline/p0-t9-nullable.msbuild.txt;verbosity=normal"`.
      Record it in `FEATURE/evidence/baseline/p0-t9-nullable.TIMESTAMP.md` with the four required
      fields, plus the verbatim `Error(s)` summary line and the `Skipping target "CoreCompile"`
      occurrence count in `FEATURE/evidence/baseline/p0-t9-nullable.msbuild.txt`. Acceptance: the
      artifact records `EXIT_CODE: 0` and a `Skipping target "CoreCompile"` occurrence count of 0 in
      `FEATURE/evidence/baseline/p0-t9-nullable.msbuild.txt`. Do not add `/p:Nullable=enable`.
- [x] [P0-T10] Run
      `pwsh -File scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot . -Configuration Debug -CoverageOutput coverage\baseline.cobertura.xml`
      from `WORKTREE`. Record it in
      `FEATURE/evidence/baseline/p0-t10-test-coverage.TIMESTAMP.md` with the four required fields, plus
      the verbatim vstest result summary line and, if the run threw, the verbatim terminating message.
      If the run terminated before any test executed, record the verbatim terminating message and
      `REMEDIATION-REQUIRED: coverage wrapper prerequisite missing` instead of the test counts: the
      wrapper throws before any test runs when `vswhere`, `vstest.console.exe`, `dotnet-coverage`, or a
      `*.Test.dll` is missing, and prints no counts on those branches. Acceptance: the artifact exists
      and its `Output Summary:` records the observed exit code together with either the total, passed,
      and failed test counts as printed by the run, or the terminating message and the
      `REMEDIATION-REQUIRED` line.
- [x] [P0-T11] Read `coverage\baseline.cobertura.xml` and record, in
      `FEATURE/evidence/baseline/p0-t11-coverage-denominator.TIMESTAMP.md`, the sorted list of every
      `package` element `name` attribute value in the file, the `line-rate`, `lines-covered`,
      `lines-valid`, `branch-rate`, `branches-covered`, and `branches-valid` attribute values of the
      root `coverage` element, and whether a `sources` element is present. Classify the file as
      `DENOMINATOR: FILTERED` when no vendored third-party assembly name (for example `log4net` or
      `Mono.Reflection`) appears in the package list, and `DENOMINATOR: UNFILTERED` otherwise. Also
      record the baseline line coverage percentage as `line-rate` multiplied by 100, to two decimal
      places. Acceptance: the artifact records a numeric `line-rate`, a numeric percentage, the package
      list, and exactly one of `DENOMINATOR: FILTERED` or `DENOMINATOR: UNFILTERED`. `UNVERIFIED` is not
      an acceptable value. If the artifact records `DENOMINATOR: UNFILTERED`, stop and record
      `REMEDIATION-REQUIRED: baseline coverage run was not green or fell below the 80 percent filtered threshold; a filtered baseline is a precondition for P7-T9 and AC20`.
      Do not proceed to Phase 1. This gate is placed here rather than at Phase 7 because
      `Assert-CoberturaLineCoverageThreshold` throws before `Set-Content`, so a red or sub-80 run leaves
      an unfiltered file on disk and makes the P7-T9 comparison and AC20 unreachable; discovering that
      after all implementation work is done costs the whole run.
- [x] [P0-T12] Extract from the P0-T10 run output the set of test identifiers that failed, and write it
      to `FEATURE/evidence/baseline/p0-t12-baseline-failure-set.TIMESTAMP.md` as
      `BASELINE_FAILURE_SET:` followed by one fully qualified test name per line, or the single line
      `BASELINE_FAILURE_SET: NONE` when the baseline run was green. Acceptance: the artifact contains
      exactly one `BASELINE_FAILURE_SET:` declaration and its member list is consistent with the counts
      recorded in P0-T10.
- [x] [P0-T13] Record the current line count of each of the six in-scope files —
      `QuickFiler/Controllers/FilerQueue.cs`,
      `QuickFiler/Controllers/QfcFormController.EventHandlers.cs`,
      `QuickFiler.Test/Controllers/FilerQueueTests.cs`,
      `QuickFiler.Test/Controllers/QfcItemController.SeamFactoryTests.cs`,
      `QuickFiler.Test/QuickFiler.Test.csproj`, and the not-yet-created
      `QuickFiler.Test/Controllers/QfcFormControllerUndoHandoffTests.cs` (recorded as 0) — using
      `(Get-Content -LiteralPath <path>).Count`, not `Measure-Object -Line`. Write
      `FEATURE/evidence/baseline/p0-t13-file-line-budget.TIMESTAMP.md`. Acceptance: the artifact records
      six numeric counts and states the remaining headroom to 500 for each, with
      `QuickFiler.Test/Controllers/QfcItemController.SeamFactoryTests.cs` showing 64 lines of headroom.
- [x] [P0-T14] Resolve the absolute path of `vstest.console.exe`, which is not on `PATH` in this
      worktree. Run
      `& "${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\vswhere.exe" -latest -products * -find "Common7\IDE\Extensions\TestPlatform\vstest.console.exe"`
      from `WORKTREE` under `pwsh` and write the resolved path into
      `FEATURE/evidence/baseline/p0-t14-vstest-path.TIMESTAMP.md` with `Timestamp:`, `Command:`,
      `EXIT_CODE:`, and `Output Summary:`. Every scoped test-run task in this plan — P1-T5, P2-T5,
      P4-T6, P5-T10, P6-T8, P6-T9, P6-T10, and P7-T8 — begins with "the absolute path recorded by
      P0-T14"; substitute this concrete path there. Acceptance: the artifact records a path ending in
      `vstest.console.exe` that `Test-Path` reports exists.

### Phase 1 — Behaviour-preserving per-item processor seam

This phase adds only the seam. It changes no observable production behaviour, keeps the `guard` field
and `Consumer` semantics exactly as they are, and exists so that the Phase 2 fail-before tests compile
and run against a tree that still carries the defect.

- [x] [P1-T1] In `QuickFiler/Controllers/FilerQueue.cs`, add an `internal Func<FilerQueueItem, Task> ItemProcessor { get; set; }`
      auto-property initialized to `item => item.Filer.SortAsync(item.Helpers)`, with an XML doc comment
      that names issue 633 and states that the production default preserves current behaviour and that
      tests assign a fake so no live Outlook COM call is made. Place it adjacent to the `Consumer`
      declaration. The initializer references no instance member, so no CS0236 workaround is required.
      The XML doc comment prose must not contain the standalone word `record`. P6-T2 searches this file
      for `\binit\s*[;{]|\brecord\b` without distinguishing code from comment, so a comment containing
      that word alone would trip a gate written to detect a net48 language construct. The plural
      `records` and the past tense `recorded` do not match `\brecord\b`, because the character following
      `record` in each is a word character and the trailing `\b` therefore does not hold.
      Acceptance: `QuickFiler/Controllers/FilerQueue.cs` contains the literal token `ItemProcessor` at
      least once and the file still contains the literal token `ThreadSafeSingleShotGuard`. One
      occurrence is the satisfiable bound for this task alone: the property declaration supplies the
      first occurrence and the call site that supplies a second arrives only with P1-T2.
- [x] [P1-T2] In `QuickFiler/Controllers/FilerQueue.cs`, replace the body of the worker's per-item call
      at line 52 so that it invokes the seam instead of the hard-coded call, leaving the surrounding
      `try` block, the `catch (Exception e)` block, the `item.Helpers.First()` diagnostic, and the
      `logger.Error` call byte-identical. `EmailFiler.SortAsync(IList<MailItemHelper>)` returns
      `Task<bool>`, which is assignable to `Task`, so the seam type needs no generic parameter.
      Acceptance: `QuickFiler/Controllers/FilerQueue.cs` contains zero occurrences of the literal token
      `item.Filer.SortAsync(item.Helpers);` inside `ConsumeAsync` and still contains the literal token
      `logger.Error`.
- [x] [P1-T3] Confirm that `QuickFiler/Controllers/FilerQueue.cs` still contains no `#nullable`
      directive, no `record`, and no `init` accessor, by running a single search over that file for the
      pattern `#nullable|\brecord\b|\binit\s*[;{]`. Acceptance: the search returns zero matches.
- [x] [P1-T4] First run `New-Item -ItemType Directory -Force -Path FEATURE/evidence/other`. This step is
      required and is not decoration: MSBuild's file logger does not create intermediate directories and
      terminates the build with MSB1029 when the directory part of `/flp:logfile=` does not exist, and
      `FEATURE/evidence/other` does not exist before this task. Then, as a separate command, run
      `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true /fl "/flp:logfile=FEATURE/evidence/other/p1-t4-seam-build.msbuild.txt;verbosity=normal"`.
      Write `FEATURE/evidence/other/p1-t4-seam-build.TIMESTAMP.md` with the four required fields for the
      msbuild command and the `Skipping target "CoreCompile"` occurrence count in
      `FEATURE/evidence/other/p1-t4-seam-build.msbuild.txt`. Acceptance: the artifact records
      `EXIT_CODE: 0` and a `Skipping target "CoreCompile"` occurrence count of 0 in
      `FEATURE/evidence/other/p1-t4-seam-build.msbuild.txt`.
- [x] [P1-T5] Run the scoped, single-assembly verification of the pre-existing queue tests, using the
      absolute path recorded by P0-T14 in place of the leading executable name:
      `vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /InIsolation /TestCaseFilter:"FullyQualifiedName~FilerQueueTests" "/Logger:trx;LogFileName=p1-t5.trx" /ResultsDirectory:FEATURE\evidence\other\p1-t5`
      from `WORKTREE`. Write `FEATURE/evidence/other/p1-t5-existing-queue-tests.TIMESTAMP.md` with the
      four required fields and the count of `outcome="Passed"` occurrences in the produced TRX file.
      Acceptance: the artifact records `EXIT_CODE: 0` and a `outcome="Passed"` count of 5, matching the
      five test methods currently in `QuickFiler.Test/Controllers/FilerQueueTests.cs`.

### Phase 2 — Fail-before regression evidence

- [x] [P2-T1] Add exactly one line to `QuickFiler.Test/QuickFiler.Test.csproj`, immediately after the
      existing `Controllers\FilerQueueTests.cs` compile item, reading
      `Compile Include="Controllers\QfcFormControllerUndoHandoffTests.cs"` in the same XML element form
      as its neighbours. Acceptance: a search of `QuickFiler.Test/QuickFiler.Test.csproj` for the
      literal token `Controllers\QfcFormControllerUndoHandoffTests.cs` returns exactly one match.
- [x] [P2-T2] Create `QuickFiler.Test/Controllers/QfcFormControllerUndoHandoffTests.cs` containing a
      single `[TestClass]` named `QfcFormControllerUndoHandoffTests`, MSTest plus Moq plus
      FluentAssertions only. Mirror the construction fixture at
      `QuickFiler.Test/Controllers/QfcFormControllerSeamTests.cs:64-76` and the private reflection
      helpers at `:43-47`. The fixture must: build a `Mock<IApplicationGlobals>` whose `FS.Filenames` is
      non-null and whose `AF.MovedMails` is non-null; build a `Mock<IQfcCollectionController>` whose
      `MoveEmailsAsync` returns `Task.CompletedTask` and whose `CleanupBackground` records an
      invocation; build a `Mock<IQfcHomeController>` whose `FilerQueue` getter returns a real
      `FilerQueue`; inject those into the controller's private `_globals`, `_groups`, and `_parent`
      fields by reflection; and install a recording metrics delegate into the private `WriteMetrics`
      field by reading that field's `FieldType` at run time and calling
      `Delegate.CreateDelegate(fieldType, target, methodInfo)`, because the delegate type is declared
      `private` on `QfcFormController` and cannot be named from the test assembly. No `Thread.Sleep`, no
      `Task.Delay`, no polling, no timeout. The `MoveEmailsAsync` setup must return
      `Task.CompletedTask` rather than a task completed later: P2-T3's ordering argument depends on the
      `await` at `QuickFiler/Controllers/QfcFormController.EventHandlers.cs:225` completing
      synchronously, so that pre-fix the metrics operation is enqueued before `BackGroundMoveAsync()`
      returns.
      The recording metrics delegate must record its invocation as its first statement and then return
      an already-completed `Task`; it must not await anything before recording. The field's type is
      `private delegate Task WriteMetricsDelegate(string filename)`
      (`QuickFiler/Controllers/QfcFormController.cs:82`, field at `:83`), and the production call site
      wraps it in `async () => await WriteMetrics(...)`, so the `DispatcherOperation` at
      `QuickFiler/Controllers/QfcFormController.EventHandlers.cs:228` completes at that lambda's first
      suspension point. A delegate that suspends before recording lets the P2-T3 probe complete with the
      count still at 0, which destroys the pre-fix discriminator.
      Acceptance: the file exists, is under 500 lines, and contains the literal token
      `[TestClass]` exactly once.
- [x] [P2-T3] Into `QuickFiler.Test/Controllers/QfcFormControllerUndoHandoffTests.cs`, add the two
      barrier tests named `BackGroundMoveAsync_WithPendingQueueItem_DoesNotDispatchCleanupBeforeDrain`
      and `BackGroundMoveAsync_WithPendingQueueItem_DoesNotWriteMetricsBeforeDrain`. Each test is built
      from these five parts, in this order.
      (1) Open `using (var transaction = await UiThreadDispatcherFixture.BeginTransactionAsync())` as a
      `using` statement. The `using` is mandatory rather than stylistic:
      `QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixture.cs:122-126` takes a
      `SemaphoreSlim(1, 1)` that only `UiThreadDispatcherTransaction.Dispose` at `:261-276` releases,
      neither that file nor this new file carries a `[Timeout]` attribute, and a permit leaked on an
      assertion-failure path therefore hangs the whole assembly run unboundedly instead of failing it.
      The `using` statement form is used rather than a `using` declaration so the test compiles
      irrespective of the language version the test project resolves.
      (2) Obtain `Dispatcher dispatcher = QfcItemControllerTestSupport.StartRunningDispatcher();`, call
      `transaction.Install(dispatcher)`, and shut it down with
      `QfcItemControllerTestSupport.ShutdownDispatcher(dispatcher)` in a `finally`. Pinning the
      dispatcher is required: `QfcItemController.FocusAndThemeTests.cs:452` and `:468` discard the scope
      returned by `EnsureUiThreadDispatcher()`, which
      `QfcItemController.TestSupport.cs:229-236` documents as permitted and leaking, so the ambient
      static is non-null for the rest of the assembly run once either has executed, and a witness that
      depended on it would violate the order-independence requirement in
      `.claude/rules/general-unit-test.md`.
      (3) Arrange a real `FilerQueue` whose `ItemProcessor` awaits a `TaskCompletionSource<bool>` gate
      created with `TaskCreationOptions.RunContinuationsAsynchronously`, enqueue exactly one item before
      the act, and release the gate in a `finally` so no worker thread is left parked.
      (4) Act by calling `BackGroundMoveAsync()` without awaiting it, capturing the returned task, then
      `await` a probe operation posted to that same `dispatcher` instance at
      `System.Windows.Threading.DispatcherPriority.ContextIdle`. The probe is the determinism device and
      replaces any timing assumption: the mocked `MoveEmailsAsync` returns `Task.CompletedTask`, so
      pre-fix the metrics operation at
      `QuickFiler/Controllers/QfcFormController.EventHandlers.cs:228` is enqueued at `ContextIdle`
      synchronously before `BackGroundMoveAsync()` returns, a WPF dispatcher runs equal-priority
      operations in enqueue order, and the probe therefore cannot complete until that metrics operation
      has run. Post-fix the method yields on `WhenDrainedAsync()` and posts nothing.
      (5) Assert with the gate still closed, then release the gate, `await` the returned task, and
      assert the post-release state.
      `..._DoesNotWriteMetricsBeforeDrain` asserts the metrics recorder count is 0 with the gate closed
      and 1 after the release and await.
      `..._DoesNotDispatchCleanupBeforeDrain` asserts the cleanup recorder count is 0 and the metrics
      recorder count is 0 with the gate closed, and after the release and await asserts the cleanup
      recorder count is 1 and the shared ordered recorder list is metrics then cleanup. Its metrics
      clause is what makes it fail deterministically before the fix, and it is a sound part of this
      test's own claim: the production order at `:228-233` reaches the cleanup dispatch only through the
      metrics dispatch, so a metrics dispatch already made while the queue is undrained proves no
      barrier is withholding the cleanup dispatch either.
      Do not assert on the returned task's `IsCompleted` while the gate is closed. Both dispatcher calls
      at `:228` and `:233` are awaited, so under a pinned running dispatcher the pre-fix task is
      incomplete at the moment of return exactly as the post-fix task is; that assertion would be a
      timing race in both directions. Acceptance: both test method names appear verbatim in the file,
      the file contains zero matches for
      `Thread\.Sleep|Task\.Delay|\.Wait\(|\.Result\b|DateTime\.(Now|UtcNow)`, and every line of the file
      that contains the literal token `BeginTransactionAsync` also contains the literal token
      `using (`. That last clause is the mechanical form of the `using`-statement requirement stated in
      part (1) and is checkable by one line-oriented search: select the lines matching
      `BeginTransactionAsync`, and require that the count of those that also match `using (` equals the
      total. Satisfying it requires the acquisition to be written as one physical line with no
      `.ConfigureAwait(false)` continuation, per executor environment note 10.
- [x] [P2-T4] First run
      `New-Item -ItemType Directory -Force -Path FEATURE/evidence/regression-testing`. This step is
      required: MSBuild's file logger does not create intermediate directories and terminates the build
      with MSB1029 when the directory part of `/flp:logfile=` does not exist, and
      `FEATURE/evidence/regression-testing` does not exist before this task. Then, as a separate
      command, run
      `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true /fl "/flp:logfile=FEATURE/evidence/regression-testing/p2-t4-redbuild.msbuild.txt;verbosity=normal"`.
      Write `FEATURE/evidence/regression-testing/p2-t4-redbuild.TIMESTAMP.md` with the four required
      fields for the msbuild command. Acceptance: the artifact records `EXIT_CODE: 0`. The fail-before
      tests must compile; a compile failure here is a defect in P2-T2 or P2-T3, not a fail-before
      witness.
- [x] [P2-T5] [expect-fail] Run, using the absolute path recorded by P0-T14 in place of the leading
      executable name:
      `vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /InIsolation /TestCaseFilter:"FullyQualifiedName~QfcFormControllerUndoHandoffTests" "/Logger:trx;LogFileName=p2-t5.trx" /ResultsDirectory:FEATURE\evidence\regression-testing\p2-t5`
      from `WORKTREE`. Write
      `FEATURE/evidence/regression-testing/fail-before-run.TIMESTAMP.md` with `Timestamp:`, `Command:`,
      `EXIT_CODE:`, `ExpectedExitCode: 1`, and `Output Summary:` carrying the verbatim failure message
      for each of the two tests, which assertion produced each failure, and the count of
      `outcome="Failed"` occurrences in the produced TRX file. Acceptance: the artifact records a
      non-zero `EXIT_CODE`, an `outcome="Failed"` count of 2, both test names among the failures, and a
      named failing assertion for each. If either test passes against this tree, stop and record
      `REMEDIATION-REQUIRED: fail-before witness did not fail` rather than proceeding to Phase 3.
      (Execution correction, applied 2026-09-01: the `outcome="Failed"` count of 2 is the count of
      `UnitTestResult` elements carrying that attribute, not the raw count of the literal anywhere in
      the TRX. A TRX produced by a failing run also carries a run-level `<ResultSummary outcome="Failed">`
      element, so the raw literal count is 3 for two failing tests. The observed values were: 2
      `UnitTestResult` failures, `Counters failed="2"`, raw literal count 3. The equivalent
      `outcome="Passed"` counts elsewhere in this plan are unaffected, because `ResultSummary` carries
      `outcome="Completed"` on a successful run — as P1-T5's exact count of 5 confirms.)
- [x] [P2-T6] Write `FEATURE/evidence/regression-testing/fail-before-exception.TIMESTAMP.md` covering
      the two cases for which a failing run is structurally impossible. It must contain
      `WhyFailingRunImpossible:` with two labelled paragraphs: one for the seven `WhenDrainedAsync_*`
      and `ItemProcessor_ThatThrows_*` tests, which name an API that does not exist before Phase 3 and
      therefore cannot compile; and one for `Enqueue_AfterPreviousBatchDrained_ProcessesSecondBatch`,
      reproducing the derivation in the "Fail-before strategy" section above. That second paragraph must
      state that this test also names `WhenDrainedAsync()` — as specified in P5-T7 — and therefore does
      not compile before Phase 3, placing it in the same structural category as the
      `WhenDrainedAsync_*` group; that it is additionally not convertible into a compilable pre-fix
      witness, because the orphaned-item window requires a producer `Queue.Add` to land strictly between
      `QuickFiler/Controllers/FilerQueue.cs:48` and `:63` and no seam, await, or observable state change
      exists between those two statements; and that it is a post-fix regression guard that does fail
      against a handshake leaving the consumer-running flag set after loop exit. Do not record that the
      test is green before the fix as well as after: it does not compile before the fix, so that
      statement would be false.
      It must also contain an absence-of-test proof with `SearchScope:`, `SearchPatterns:`, and
      `SearchResult:` showing that no test naming `WhenDrainedAsync` exists in `QuickFiler.Test/` before
      Phase 3. Acceptance: the artifact contains one `WhyFailingRunImpossible:` field, one
      `SearchScope:`, one `SearchPatterns:`, and one `SearchResult:` field.
- [x] [P2-T7] Sanitise, then commit. First, across every file under `FEATURE/evidence/`, replace all
      case-insensitive occurrences of the absolute worktree path with the literal token `WORKTREE`,
      covering all three spellings that appear in this evidence set: the backslash form, the
      forward-slash form, and the doubled-backslash form. This is required because the TRX trees and the
      `.msbuild.txt` logs committed here embed this machine's full user-profile path, and repository
      artifact hygiene prohibits absolute host paths in committed artifacts. Re-scan and record only the
      COUNT of remaining matches in
      `FEATURE/evidence/qa-gates/p2-t7-sanitisation.TIMESTAMP.md` with `Timestamp:`, `Command:`,
      `EXIT_CODE:`, and `Output Summary:`. Do not quote a matched path in the artifact: a quoted host
      path becomes a match on the next sweep. Additionally record the count of files under
      `FEATURE/evidence/` whose file name contains the token produced by
      `Split-Path -Leaf $env:USERPROFILE`; do not write that token into the artifact. Acceptance
      additionally requires that this count is 0. Apply the same test to directory names under
      `FEATURE/evidence/` and record that count too; a directory whose name carries the account token
      puts the token in a committed path just as a file name does. Record both counts separately and
      require both to be 0. Then, as a separate command, commit the Phase 1 and
      Phase 2 changes with a message naming issue 633 and the phrase
      `fail-before`. That commit must also include this plan file and every artifact under
      `FEATURE/evidence/baseline/`, which the Phase 0 tasks produced and no earlier task has committed.
      The carve-out is required rather than tidy: this plan file is already dirty at this point from the
      P0-T1 through P2-T6 check-offs and it lives under the feature folder, so the restricted porcelain
      condition below is unsatisfiable unless the plan file is in the commit. Write this task's own
      checkbox after the commit, matching the ordering P8-T2 uses for the same reason.
      Acceptance: the recorded remaining-match count for the absolute worktree path is 0, the recorded
      account-token file-name count is 0, the recorded account-token directory-name count is 0,
      and `git status --porcelain` in `WORKTREE`, restricted to the paths
      `QuickFiler/`, `QuickFiler.Test/`, and `docs/features/active/2026-08-26-qfc-unsynchronized-undo-handoff-after-batch-move-633/`,
      lists no path other than this plan file, whose checkbox for this task is written after the commit.

### Phase 3 — FilerQueue drain barrier and handshake repair

- [x] [P3-T1] In `QuickFiler/Controllers/FilerQueue.cs`, add three private members: a
      `private readonly object` monitor, a `private int` outstanding-work counter, and a
      `private TaskCompletionSource<bool>` drain signal that is null when the queue is idle. Do not use
      the parameterless non-generic `TaskCompletionSource`; it does not exist on net481. Acceptance: the
      file declares exactly one `readonly object` field and exactly one
      `TaskCompletionSource<bool>` field.
- [x] [P3-T2] In `QuickFiler/Controllers/FilerQueue.cs`, add a `private bool` consumer-running flag and
      remove the `ThreadSafeSingleShotGuard guard` field at line 40 together with both
      `guard.CheckAndSetFirstCall` reads and the `guard = new ThreadSafeSingleShotGuard();` statement at
      line 63. Do not modify `UtilitiesCS/Threading/ThreadSafeSingleShotGuard.cs`; the counter-based
      design lives entirely inside `FilerQueue.cs`. Acceptance: a search of
      `QuickFiler/Controllers/FilerQueue.cs` for the literal token `ThreadSafeSingleShotGuard` returns
      zero matches, and a search of `UtilitiesCS/Threading/ThreadSafeSingleShotGuard.cs` for the literal
      token `class ThreadSafeSingleShotGuard` still returns one match.
- [x] [P3-T3] Rewrite `Enqueue(FilerQueueItem item)` in `QuickFiler/Controllers/FilerQueue.cs` so that,
      under the monitor, it increments the outstanding-work counter, performs `Queue.Add(item)`, and
      decides from the consumer-running flag whether a worker must be started; the worker is started
      outside the monitor and `Consumer` is still assigned. `Queue.Add` on an unbounded
      `BlockingCollection` never blocks, so holding the monitor across it is safe, and the monitor is
      never held across an `await`. Acceptance: the method body contains exactly one `lock` statement
      and one `Queue.Add(item);` statement, and `Consumer` is still assigned within the method.
- [x] [P3-T4] Rewrite `Enqueue(EmailFiler filer, IList<MailItemHelper> helpers)` in
      `QuickFiler/Controllers/FilerQueue.cs` to construct the `FilerQueueItem` in its own frame and
      delegate to the item overload. Constructing in this frame is load-bearing: it is what keeps a null
      helper surfacing as a synchronous `ArgumentNullException` to the caller, which
      `MoveMailAsync_WhenEnqueueThrows_WrapsArgumentNullException` depends on. Acceptance: the method
      body contains the literal token `new FilerQueueItem(filer, helpers)` and contains no `Queue.Add`
      call of its own.
- [x] [P3-T5] Rewrite `ConsumeAsync` in `QuickFiler/Controllers/FilerQueue.cs` so that the worker takes
      each item and clears the consumer-running flag in the **same** critical section in which `TryTake`
      fails, and so that the outstanding-work counter is decremented in a `finally` around the
      `ItemProcessor` call. When the counter reaches zero, complete and clear the drain signal inside
      the monitor. The existing `catch (Exception e)` block, the `item.Helpers.First()` diagnostic, and
      the `logger.Error` call stay wrapped around the seam call unchanged, so a failing item is still
      logged and the loop still continues. Acceptance: the method contains exactly one `finally` block
      and the literal tokens `logger.Error` and `item.Helpers.First()` each still appear exactly once in
      the file.
- [x] [P3-T6] Add `public Task WhenDrainedAsync()` to `QuickFiler/Controllers/FilerQueue.cs`. Under the
      monitor it returns `Task.CompletedTask` when the outstanding count is zero, and otherwise the
      lazily created drain signal's `Task`. The signal is created with
      `TaskCreationOptions.RunContinuationsAsynchronously`. The returned task completes; it never
      faults, so a logged item failure is not converted into an unhandled exception on the batch-move
      path. Give it an XML doc comment naming issue 633 and stating idempotency. That comment prose must
      not contain the standalone word `record`: P6-T2 searches this file for
      `\binit\s*[;{]|\brecord\b` without distinguishing code from comment. The plural `records` and the
      past tense `recorded` do not match `\brecord\b`, because the character following `record` in each
      is a word character and the trailing `\b` therefore does not hold. Acceptance:
      `QuickFiler/Controllers/FilerQueue.cs` contains the literal token
      `public Task WhenDrainedAsync()` exactly once.
- [x] [P3-T7] Verify that `Consumer` in `QuickFiler/Controllers/FilerQueue.cs` still has its original
      declaration. Acceptance: a search of that file for the literal token
      `public Task Consumer { get; private set; } = Task.CompletedTask;` returns exactly one match.
- [x] [P3-T8] Repair `QuickFiler.Test/Controllers/QfcItemController.SeamFactoryTests.cs`. In
      `MoveMailAsync_WhenOneDrivePresent_InvokesFactoryWithConfigAndEnqueues`, delete the reflection
      block at lines 213-218 that reads the private `guard` field and sets
      `ThreadSafeSingleShotGuard._state`, and delete the `filerQueue.Queue.Count.Should().Be(1)`
      assertion at line 234. Replace them with an observation of the item the queue actually handed to
      the seam: capture the `EmailFiler` the factory produced in a local, assign the queue's
      `ItemProcessor` to a delegate that completes a `TaskCompletionSource<FilerQueueItem>` with the
      received item and then returns the `Task` of a second `TaskCompletionSource<bool>` gate held
      closed for the test, `await` the received-item source after `MoveMailAsync()`, and assert exactly
      one item was received and that its `Filer` is the same instance the factory produced. Release the
      gate in a `finally`.
      The `Queue.Count` assertion cannot be retained and is not merely being relaxed: after P3-T3 and
      P3-T5, `Enqueue` starts a worker whose `TryTake` removes the item before `ItemProcessor` is
      invoked, so a gated processor parks with `Queue.Count` equal to 0, not 1, and reading that count
      at any other moment is a thread-pool race. Keeping the old assertion would make this task and
      P6-T9 mutually unsatisfiable. The received-item source is the deterministic replacement: it is
      completed by the worker itself, so awaiting it needs no timing assumption.
      Replace the comment at lines 210-211, which after this repair states something false. It currently
      reads that the queue is "A real FilerQueue whose single-shot guard is pre-tripped so Enqueue
      records the item without spinning up the background consumer (deterministic, no external I/O)."
      After P3-T2 there is no guard, and after P3-T3 `Enqueue` does start the background consumer.
      Write instead a comment stating that the queue is a real `FilerQueue` whose `ItemProcessor` seam
      is assigned to a gated delegate, so the worker hands the item to the test and parks there, and
      that this is deterministic and performs no external I/O.
      Remove exactly two now-unused `using` directives: `using System.Reflection;` at
      `QuickFiler.Test/Controllers/QfcItemController.SeamFactoryTests.cs:4`, which is needed only for
      `BindingFlags` at lines 214 and 217, and `using UtilitiesCS.Threading;` at `:18`, which is needed
      only for `ThreadSafeSingleShotGuard` at line 216. Both become unused once lines 213-218 are
      deleted. `ThreadSafeSingleShotGuard` and `BindingFlags` are type names rather than directives and
      are removed by deleting that block, not by editing the directive list. Lines 343, 347, and 421
      call the test-support helper `QfcItemControllerTestSupport.GetField`, not `Type.GetField`, so no
      other reflection use remains in the file; the only other occurrence of a `UtilitiesCS.Threading`
      identifier is `IUiDispatcher` inside a doc comment at line 300, which binds nothing. Acceptance:
      a search of
      `QuickFiler.Test/Controllers/QfcItemController.SeamFactoryTests.cs` for the literal token
      `GetField("guard"` returns zero matches; a search of the same file for each of the three
      single-line literal tokens `pre-tripped`, `System.Reflection`, and `UtilitiesCS.Threading`
      returns zero matches, each of which occurs exactly once in the file today, at lines 210, 4, and 18
      respectively; the test asserts that exactly one item was received and
      that its `Filer` is the factory-produced instance; the file contains zero matches for
      `Thread\.Sleep|Task\.Delay|\.Wait\(|\.Result\b|DateTime\.(Now|UtcNow)`; and
      `(Get-Content -LiteralPath QuickFiler.Test/Controllers/QfcItemController.SeamFactoryTests.cs).Count`
      is at most 500.

### Phase 4 — QfcFormController barrier, guard clause, and Consumer-await removal

- [x] [P4-T1] In `QuickFiler/Controllers/QfcFormController.EventHandlers.cs`, add a `_parent` null check
      to the early-return guard of `BackGroundMoveAsync` at line 219, keeping the existing three clauses
      and the existing early-return shape. The clause is required because P4-T2 makes the method
      dereference `_parent`, and `QuickFiler/Controllers/QfcFormController.SetupDisposal.cs:224` sets
      `_parent = null` during cleanup. Acceptance: the guard line contains the literal token
      `_parent is null` and the file still contains the literal token
      `_globals?.FS?.Filenames is null`.
- [x] [P4-T2] In `QuickFiler/Controllers/QfcFormController.EventHandlers.cs`, insert
      `await _parent.FilerQueue.WhenDrainedAsync();` between the `await _groups.MoveEmailsAsync(_movedItems);`
      statement and the `WriteMetrics` dispatch, with a short comment explaining that the barrier makes
      the undo-push ordering a control-flow property rather than an assumption. That comment prose must
      not contain the standalone word `record`: P6-T2 searches this file for
      `\binit\s*[;{]|\brecord\b` without distinguishing code from comment. The plural `records` and the
      past tense `recorded` do not match `\brecord\b`, because the character following `record` in each
      is a word character and the trailing `\b` therefore does not hold. Do not reorder the
      `WriteMetrics` dispatch and the `CleanupBackground` dispatch; `WriteMetricsAsync` reads state that
      `CleanupBackground` resets. Acceptance: the file contains the literal token
      `await _parent.FilerQueue.WhenDrainedAsync();` exactly once, and the line number of that statement
      is greater than the line number of `await _groups.MoveEmailsAsync(_movedItems);` and less than the
      line number of the first `UiThread.Dispatcher.InvokeAsync` call inside `BackGroundMoveAsync`.
- [x] [P4-T3] Delete the two `await _parent.FilerQueue.Consumer;` statements in
      `QuickFiler/Controllers/QfcFormController.EventHandlers.cs`, currently at lines 167 and 193. Both
      are strictly subsumed: each is immediately preceded by an await of the same `BackGroundMoveAsync`
      task, which now contains the barrier, and the barrier waits on the whole outstanding count rather
      than on a single worker task. Acceptance: a search of `QuickFiler/**/*.cs` for the pattern
      `\.Consumer\b` returns zero matches.
- [x] [P4-T4] Confirm that `QuickFiler/Controllers/QfcFormController.EventHandlers.cs` still contains no
      `#nullable` directive, no `record`, and no `init` accessor, by running a single search over that
      file for the pattern `#nullable|\brecord\b|\binit\s*[;{]`. Acceptance: the search returns zero
      matches.
- [x] [P4-T5] Run
      `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true /fl "/flp:logfile=FEATURE/evidence/other/p4-t5-build.msbuild.txt;verbosity=normal"`.
      Write `FEATURE/evidence/other/p4-t5-build.TIMESTAMP.md` with the four required fields and the
      `Skipping target "CoreCompile"` occurrence count in
      `FEATURE/evidence/other/p4-t5-build.msbuild.txt`. Acceptance: the artifact records `EXIT_CODE: 0`
      and a `Skipping target "CoreCompile"` occurrence count of 0 in
      `FEATURE/evidence/other/p4-t5-build.msbuild.txt`.
- [x] [P4-T6] Run, using the absolute path recorded by P0-T14 in place of the leading executable name:
      `vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /InIsolation /TestCaseFilter:"FullyQualifiedName~QfcFormControllerUndoHandoffTests" "/Logger:trx;LogFileName=p4-t6.trx" /ResultsDirectory:FEATURE\evidence\regression-testing\p4-t6`
      from `WORKTREE`. Write
      `FEATURE/evidence/regression-testing/pass-after-run.TIMESTAMP.md` with the four required fields
      and the count of `outcome="Passed"` occurrences in the produced TRX file. Acceptance: the artifact
      records `EXIT_CODE: 0` and an `outcome="Passed"` count of 2, and the same two test names that
      failed in P2-T5 now appear as passed. This pairs with P2-T5 to form the fail-before/pass-after
      record.

### Phase 5 — Queue-level and ordering regression suites

- [x] [P5-T1] Correct the class comment at `QuickFiler.Test/Controllers/FilerQueueTests.cs:12-19` so it
      no longer records that the `Enqueue`/`ConsumeAsync` path is deliberately not exercised; state
      instead that the path is exercised deterministically through the `ItemProcessor` seam added for
      issue 633. Acceptance: a search of that file for the literal token
      `intentionally NOT exercised` returns zero matches and a search for the literal token
      `ItemProcessor` returns at least one match.
- [x] [P5-T2] Add `WhenDrainedAsync_OnFreshQueue_ReturnsCompletedTask` to
      `QuickFiler.Test/Controllers/FilerQueueTests.cs`: construct a `FilerQueue`, enqueue nothing, and
      assert the returned task's `IsCompleted` is true. Acceptance: the test method name appears
      verbatim in the file exactly once.
- [x] [P5-T3] Add `WhenDrainedAsync_WithGatedItem_DoesNotCompleteBeforeItemCompletes` to
      `QuickFiler.Test/Controllers/FilerQueueTests.cs`: assign an `ItemProcessor` that signals an entry
      `TaskCompletionSource<bool>` and then awaits a gate `TaskCompletionSource<bool>`; enqueue one
      item; await the entry signal; assert the drain task's `IsCompleted` is false; release the gate in
      a `finally`. Acceptance: the test method name appears verbatim exactly once and
      `QuickFiler.Test/Controllers/FilerQueueTests.cs` contains zero matches for
      `Thread\.Sleep|Task\.Delay|\.Wait\(|\.Result\b|DateTime\.(Now|UtcNow)`.
- [x] [P5-T4] Add `WhenDrainedAsync_AfterGateReleased_CompletesAndItemRanOnce` to
      `QuickFiler.Test/Controllers/FilerQueueTests.cs`: enqueue one gated item, release the gate, await
      the drain task, and assert the processor invocation counter equals 1. Acceptance: the test method
      name appears verbatim exactly once, the test asserts an invocation count of 1, and
      `QuickFiler.Test/Controllers/FilerQueueTests.cs` contains zero matches for
      `Thread\.Sleep|Task\.Delay|\.Wait\(|\.Result\b|DateTime\.(Now|UtcNow)`.
- [x] [P5-T5] Add `WhenDrainedAsync_WithTwoGatedItems_CompletesOnlyAfterBothComplete` to
      `QuickFiler.Test/Controllers/FilerQueueTests.cs`: enqueue two items with one gate each, release
      the first gate only, assert the drain task's `IsCompleted` is false, then release the second gate,
      await the drain task, and assert both processors ran. Acceptance: the test method name appears
      verbatim exactly once, the test asserts an invocation count of 2, and
      `QuickFiler.Test/Controllers/FilerQueueTests.cs` contains zero matches for
      `Thread\.Sleep|Task\.Delay|\.Wait\(|\.Result\b|DateTime\.(Now|UtcNow)`.
- [x] [P5-T6] Add `WhenDrainedAsync_AwaitedTwice_BothWaitersComplete` to
      `QuickFiler.Test/Controllers/FilerQueueTests.cs`: obtain two drain tasks before the gate releases,
      release the gate, await both, then call `WhenDrainedAsync()` once more and assert the third
      returned task's `IsCompleted` is true. Acceptance: the test method name appears verbatim exactly
      once, the test obtains at least three drain tasks, and
      `QuickFiler.Test/Controllers/FilerQueueTests.cs` contains zero matches for
      `Thread\.Sleep|Task\.Delay|\.Wait\(|\.Result\b|DateTime\.(Now|UtcNow)`.
- [x] [P5-T7] Add `Enqueue_AfterPreviousBatchDrained_ProcessesSecondBatch` to
      `QuickFiler.Test/Controllers/FilerQueueTests.cs`: release the first gate, await the drain, enqueue
      a second item with a second gate, release it, await the new drain, and assert the second item was
      processed. This is the regression guard for the orphaned-item window; per the P2-T6 dossier it is
      green before the fix as well as after, and it guards against reintroducing the window under the
      repaired handshake. Acceptance: the test method name appears verbatim exactly once.
- [x] [P5-T8] Add `ItemProcessor_ThatThrows_StillDecrementsAndDrainCompletes` to
      `QuickFiler.Test/Controllers/FilerQueueTests.cs`: assign an `ItemProcessor` that throws for the
      first item and completes for the second, enqueue both, await the drain, and assert the drain task
      completed without faulting and that the second item was still processed. Acceptance: the test
      method name appears verbatim exactly once, the test asserts the drain task's `IsFaulted` is
      false, and `QuickFiler.Test/Controllers/FilerQueueTests.cs` contains zero matches for
      `Thread\.Sleep|Task\.Delay|\.Wait\(|\.Result\b|DateTime\.(Now|UtcNow)`.
      Each enqueued item's `Helpers` list must contain at least one `MailItemHelper`, using the existing
      `OneHelper()` factory at `QuickFiler.Test/Controllers/FilerQueueTests.cs:23`. This is load-bearing,
      not stylistic: the preserved `catch` block at `QuickFiler/Controllers/FilerQueue.cs:56` calls
      `item.Helpers.First()`, the `FilerQueueItem` constructor at `:70-78` accepts an empty list, and an
      `InvalidOperationException` raised inside that catch escapes the `while` loop at `:48`, leaving the
      second item's outstanding count undecremented so the awaited drain never completes. No `[Timeout]`
      attribute is present in this file, so that state hangs the assembly run rather than failing it. A
      default-constructed `MailItemHelper` is safe: `MailItemHelper()` at
      `UtilitiesCS/OutlookObjects/MailItem/MailItemHelper.cs:80` calls `InitializeSafeDefaults()`, which
      seeds `Subject`, `SenderName`, and `SentOn` with non-COM values.
- [x] [P5-T9] Add the three remaining ordering tests to
      `QuickFiler.Test/Controllers/QfcFormControllerUndoHandoffTests.cs`:
      `BackGroundMoveAsync_AfterQueueDrains_WritesMetricsThenCleansUp`, which installs a pumping STA
      dispatcher via `using (var transaction = await UiThreadDispatcherFixture.BeginTransactionAsync())`
      plus `transaction.Install(QfcItemControllerTestSupport.StartRunningDispatcher())`, releases the
      gate, awaits the returned task, and asserts a shared ordered recorder list equals metrics then
      cleanup with each invoked exactly once, shutting the dispatcher down in a `finally` via
      `ShutdownDispatcher`. The `using` statement is mandatory for the same reason as in P2-T3:
      `BeginTransactionAsync` at
      `QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixture.cs:122-126` takes a
      `SemaphoreSlim(1, 1)` released only by `UiThreadDispatcherTransaction.Dispose` at `:261-276`, no
      `[Timeout]` attribute is present in either file, and a permit leaked on an assertion-failure path
      hangs the run unboundedly instead of failing it. Also add
      `BackGroundMoveAsync_WhenParentIsNull_ReturnsWithoutThrowing`, which nulls `_parent` while leaving
      `_groups`, `_globals.FS.Filenames`, and `WriteMetrics` non-null, awaits the returned task, and
      asserts it completed without faulting and that `MoveEmailsAsync` was never invoked; and
      `BackGroundMoveAsync_WhenGroupsIsNull_ReturnsWithoutTouchingQueue`, which nulls `_groups` and
      asserts the returned task completed without faulting and that the queue was not touched. The
      outstanding-work counter added by P3-T1 is private and is not an observable, so this test names
      the two public observables instead: `WhenDrainedAsync().IsCompleted` is true and `Queue.Count`
      is 0. Both hold on a queue that was never enqueued to, and both would be falsified by an
      implementation that enqueued from this path. Acceptance: all three method names appear verbatim in
      the file, the third test asserts `WhenDrainedAsync().IsCompleted` is true and `Queue.Count` is 0,
      the file remains under
      500 lines, the file contains zero matches for
      `Thread\.Sleep|Task\.Delay|\.Wait\(|\.Result\b|DateTime\.(Now|UtcNow)`, and every line of the file
      that contains the literal token `BeginTransactionAsync` also contains the literal token
      `using (`, checked as in P2-T3.
- [x] [P5-T10] Run
      `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true /fl "/flp:logfile=FEATURE/evidence/other/p5-t10-build.msbuild.txt;verbosity=normal"`
      and then, using the absolute path recorded by P0-T14 in place of the leading executable name,
      `vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /InIsolation /TestCaseFilter:"FullyQualifiedName~FilerQueueTests" "/Logger:trx;LogFileName=p5-t10.trx" /ResultsDirectory:FEATURE\evidence\regression-testing\p5-t10`
      as two separate commands. Write
      `FEATURE/evidence/regression-testing/p5-t10-queue-suite.TIMESTAMP.md` recording both commands with
      the four required fields each, plus the count of `outcome="Passed"` and `outcome="Failed"`
      occurrences in the produced TRX file. Acceptance: the artifact records `EXIT_CODE: 0` for both
      commands, an `outcome="Failed"` count of 0, and an `outcome="Passed"` count of 12, which is the
      five pre-existing tests recorded in P1-T5 plus the seven added in P5-T2 through P5-T8.

### Phase 6 — Constraint verification sweep

- [x] [P6-T1] Search `QuickFiler.Test/Controllers/FilerQueueTests.cs`,
      `QuickFiler.Test/Controllers/QfcFormControllerUndoHandoffTests.cs`, and
      `QuickFiler.Test/Controllers/QfcItemController.SeamFactoryTests.cs` for the pattern
      `Thread\.Sleep|Task\.Delay|\.Wait\(|\.Result\b|DateTime\.(Now|UtcNow)`. Write
      `FEATURE/evidence/qa-gates/p6-t1-determinism-sweep.TIMESTAMP.md` with `Timestamp:`, `Command:`,
      `EXIT_CODE:`, `Output Summary:`, and the per-file match counts. Acceptance: the artifact records a
      total match count of 0 across the three files.
- [x] [P6-T2] Search `QuickFiler/Controllers/FilerQueue.cs` and
      `QuickFiler/Controllers/QfcFormController.EventHandlers.cs` for the pattern
      `\binit\s*[;{]|\brecord\b`. Write
      `FEATURE/evidence/qa-gates/p6-t2-net481-language-sweep.TIMESTAMP.md` with the four required fields
      and the per-file match counts. Acceptance: the artifact records a total match count of 0.
- [x] [P6-T3] Search `QuickFiler/Controllers/FilerQueue.cs` and
      `QuickFiler/Controllers/QfcFormController.EventHandlers.cs` for the literal token `#nullable`.
      Write `FEATURE/evidence/qa-gates/p6-t3-nullable-pragma-sweep.TIMESTAMP.md` with the four required
      fields. Acceptance: the artifact records a total match count of 0. This is a preservation gate,
      not a change gate: the pre-change count is also 0, and the gate exists because adding
      `#nullable enable` to either file would conscript it into nullable analysis and promote its
      `CS86xx` diagnostics to build errors under `/p:TreatWarningsAsErrors=true`.
- [x] [P6-T4] Search `QuickFiler/**/*.cs` for the pattern `\.Consumer\b`. Write
      `FEATURE/evidence/qa-gates/p6-t4-consumer-read-sweep.TIMESTAMP.md` with the four required fields
      and the full match list. Acceptance: the artifact records a match count of 0. The pre-change
      population was exactly two, at `QuickFiler/Controllers/QfcFormController.EventHandlers.cs:167` and
      `:193`, and both were removed by P4-T3.
- [x] [P6-T5] Record `(Get-Content -LiteralPath <path>).Count` for
      `QuickFiler/Controllers/FilerQueue.cs` and
      `QuickFiler/Controllers/QfcFormController.EventHandlers.cs` into
      `FEATURE/evidence/qa-gates/p6-t5-production-file-sizes.TIMESTAMP.md` with the four required
      fields. Do not use `Measure-Object -Line`. Acceptance: both recorded counts are at most 500.
- [x] [P6-T6] Record `(Get-Content -LiteralPath <path>).Count` for
      `QuickFiler.Test/Controllers/FilerQueueTests.cs`,
      `QuickFiler.Test/Controllers/QfcFormControllerUndoHandoffTests.cs`, and
      `QuickFiler.Test/Controllers/QfcItemController.SeamFactoryTests.cs` into
      `FEATURE/evidence/qa-gates/p6-t6-test-file-sizes.TIMESTAMP.md` with the four required fields.
      Acceptance: all three recorded counts are at most 500.
- [x] [P6-T7] Search `QuickFiler.Test/QuickFiler.Test.csproj` for the literal token
      `Controllers\QfcFormControllerUndoHandoffTests.cs`. Write
      `FEATURE/evidence/qa-gates/p6-t7-csproj-compile-item.TIMESTAMP.md` with the four required fields.
      Acceptance: the artifact records a match count of exactly 1.
- [x] [P6-T8] Run, using the absolute path recorded by P0-T14 in place of the leading executable name:
      `vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /InIsolation /TestCaseFilter:"FullyQualifiedName~MoveMailAsync_WhenEnqueueThrows_WrapsArgumentNullException" "/Logger:trx;LogFileName=p6-t8.trx" /ResultsDirectory:FEATURE\evidence\qa-gates\p6-t8`
      from `WORKTREE`. Write `FEATURE/evidence/qa-gates/p6-t8-enqueue-argnull.TIMESTAMP.md` with the
      four required fields and the count of `outcome="Passed"` occurrences in the produced TRX file.
      Acceptance: the artifact records `EXIT_CODE: 0` and an `outcome="Passed"` count of 1, and
      `git diff origin/main -- QuickFiler.Test/Controllers/QfcItemController.MailActionsTests.cs`
      produces no output, proving the test passed unmodified. The two-dot form is deliberate and this
      plan uses it consistently here and in P7-T10: it compares the **working tree** against the base,
      so it observes the Phase 3 through Phase 6 changes, which are still uncommitted at this point
      because the last commit before Phase 7 is the one taken by P2-T7 and it covers Phases 1 and 2
      only. A three-dot form is commit-to-commit and would report nothing for work that is not yet
      committed. If `git rev-parse origin/main` at this point differs from the value P0-T3 recorded,
      re-run the command with the merge-base SHA that P0-T3 recorded substituted for `origin/main` and
      record both outputs, so an upstream advance cannot be mistaken for a local edit.
- [x] [P6-T9] Run, using the absolute path recorded by P0-T14 in place of the leading executable name:
      `vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /InIsolation /TestCaseFilter:"FullyQualifiedName~MoveMailAsync_WhenOneDrivePresent_InvokesFactoryWithConfigAndEnqueues" "/Logger:trx;LogFileName=p6-t9.trx" /ResultsDirectory:FEATURE\evidence\qa-gates\p6-t9`
      from `WORKTREE`. Write `FEATURE/evidence/qa-gates/p6-t9-seamfactory-reconciled.TIMESTAMP.md` with
      the four required fields and the `outcome="Passed"` count. Acceptance: the artifact records
      `EXIT_CODE: 0` and an `outcome="Passed"` count of 1.
- [x] [P6-T10] Run, using the absolute path recorded by P0-T14 in place of the leading executable name:
      `vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /InIsolation /TestCaseFilter:"FullyQualifiedName~FilerQueue_NewInstance_HasCompletedConsumerByDefault" "/Logger:trx;LogFileName=p6-t10.trx" /ResultsDirectory:FEATURE\evidence\qa-gates\p6-t10`
      from `WORKTREE`. Write `FEATURE/evidence/qa-gates/p6-t10-consumer-default.TIMESTAMP.md` with the
      four required fields and the `outcome="Passed"` count. Obtain the pre-change text as a second
      command, `git show SHA:QuickFiler.Test/Controllers/FilerQueueTests.cs`, substituting for `SHA` the
      concrete merge-base commit hash that P0-T3 recorded, and quote lines 76-87 of that output in the
      artifact. Reading the pre-change body from the working file is not an option here: P5-T1 through
      P5-T8 add tests to the same file and shift the line numbers, so lines 76-87 of the current file no
      longer name this method. Acceptance: the artifact records `EXIT_CODE: 0`, an `outcome="Passed"`
      count of 1, and the body of that test method in the current file is byte-identical to the quoted
      lines 76-87 of the `git show` output.

### Phase 7 — Final QA loop

Run steps in the order format, analyze, type-check, test. If any step fails or rewrites a file, fix the
cause and restart this phase from P7-T1. `EXIT_CODE: SKIPPED` is not a passing outcome for any task in
this phase.

One exit is carved out of that restart rule, and it is the only one. If P7-T3 records
`REMEDIATION-REQUIRED: pre-existing formatting drift outside scope`, that branch **terminates** this
phase's loop and is reported; it does not restart the phase from P7-T1. Without the carve-out the two
tasks read together as an unbounded loop: P7-T2 mandates restoring, with `git checkout -- <path>`, any
out-of-scope file that `csharpier format .` rewrote, which guarantees P7-T3 keeps reporting those same
files as unformatted on every subsequent iteration, and restarting on that report can never converge.
The restart rule continues to apply to every other failure in this phase, including any P7-T3 failure
whose unformatted set includes one of the six in-scope files.

- [x] [P7-T1] Record `git status --porcelain` in `WORKTREE` before running the formatter, into
      `FEATURE/evidence/qa-gates/p7-t1-preformat-status.TIMESTAMP.md` with the four required fields and
      the verbatim output. Acceptance: the artifact records the verbatim porcelain output, which may be
      empty.
- [x] [P7-T2] Run `dotnet tool run csharpier format .` from `WORKTREE`, then immediately record
      `git status --porcelain` again. Write `FEATURE/evidence/qa-gates/p7-t2-csharpier-format.TIMESTAMP.md`
      with the four required fields, the verbatim summary line the command printed, and both the
      pre-run porcelain output from P7-T1 and the post-run porcelain output. Do not assert on the
      formatter's `Formatted N files` line as a rewritten-file count; in CSharpier 1.2.6 that value is
      the number of files processed, not the number rewritten. The rewritten set is the set difference
      between the two porcelain outputs. If that difference contains any path outside the six in-scope
      files, restore those paths with `git checkout -- <path>` and record each restored path in the
      artifact; the restoration is required because AC16 constrains the production diff to two files.
      The set difference detects a rewrite only of a path that was clean before the run. The six
      in-scope files are already dirty at this point, so a rewrite of any of them does not appear in the
      difference. That is intended, not a gap: rewrites within the blast radius are permitted, and only
      an out-of-scope rewrite falsifies AC16.
      Acceptance: the artifact records `EXIT_CODE: 0`, both porcelain outputs, and an explicit statement
      that the set difference contains no path outside the six in-scope files.
- [x] [P7-T3] Run `dotnet tool run csharpier check .` from `WORKTREE`. Write
      `FEATURE/evidence/qa-gates/p7-t3-csharpier-check.TIMESTAMP.md` with the four required fields and
      the verbatim summary line. Acceptance: the artifact records `EXIT_CODE: 0`. If P0-T7 recorded
      pre-existing drift and this check still reports unformatted files outside the six in-scope files,
      record `REMEDIATION-REQUIRED: pre-existing formatting drift outside scope` with the file list and
      report it rather than reformatting out-of-scope files.
- [x] [P7-T4] Run
      `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true /fl "/flp:logfile=FEATURE/evidence/qa-gates/p7-t4-analyze.msbuild.txt;verbosity=normal"`.
      Write `FEATURE/evidence/qa-gates/p7-t4-analyze.TIMESTAMP.md` with the four required fields, the
      verbatim `Error(s)` and `Warning(s)` summary lines, and the count of occurrences of the literal
      `Skipping target "CoreCompile"` in `FEATURE/evidence/qa-gates/p7-t4-analyze.msbuild.txt`.
      Acceptance: the artifact records `EXIT_CODE: 0` and a `Skipping target "CoreCompile"` occurrence
      count of 0 in `FEATURE/evidence/qa-gates/p7-t4-analyze.msbuild.txt`.
- [x] [P7-T5] Run
      `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true /fl "/flp:logfile=FEATURE/evidence/qa-gates/p7-t5-nullable.msbuild.txt;verbosity=normal"`.
      Write `FEATURE/evidence/qa-gates/p7-t5-nullable.TIMESTAMP.md` with the four required fields, the
      verbatim `Error(s)` summary line, and the `Skipping target "CoreCompile"` occurrence count in
      `FEATURE/evidence/qa-gates/p7-t5-nullable.msbuild.txt`. Do not add `/p:Nullable=enable` and do not
      substitute `/t:Build`. Acceptance: the artifact records `EXIT_CODE: 0` and a
      `Skipping target "CoreCompile"` occurrence count of 0 in
      `FEATURE/evidence/qa-gates/p7-t5-nullable.msbuild.txt`.
- [x] [P7-T6] Run
      `pwsh -File scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot . -Configuration Debug -CoverageOutput coverage\post-change.cobertura.xml`
      from `WORKTREE`. Write `FEATURE/evidence/qa-gates/p7-t6-test-coverage.TIMESTAMP.md` with the four
      required fields, the verbatim vstest result summary line, and the total, passed, failed, and
      skipped counts. Acceptance: the artifact records `EXIT_CODE: 0` and a failed count of 0. If the
      failed count is non-zero, compare the failing set against `BASELINE_FAILURE_SET` from P0-T12: any
      failure not in that set is a regression introduced by this change and must be fixed before this
      phase restarts; any failure inside that set is recorded as pre-existing, `EXIT_CODE: 0` becomes
      unreachable, and the task is recorded as `REMEDIATION-REQUIRED` rather than checked.
- [x] [P7-T7] Read `coverage\post-change.cobertura.xml` and record, in
      `FEATURE/evidence/qa-gates/p7-t7-coverage-denominator.TIMESTAMP.md`, the same field set that
      P0-T11 recorded for the baseline file: the sorted `package` `name` list, the root `coverage`
      element's `line-rate`, `lines-covered`, `lines-valid`, `branch-rate`, `branches-covered`, and
      `branches-valid`, whether a `sources` element is present, the percentage to two decimal places,
      and exactly one of `DENOMINATOR: FILTERED` or `DENOMINATOR: UNFILTERED`. Acceptance: the artifact
      records `DENOMINATOR: FILTERED` and a numeric percentage. If it records
      `DENOMINATOR: UNFILTERED`, the run was not green and P7-T6 must be resolved first.
- [x] [P7-T8] Run, using the absolute path recorded by P0-T14 in place of the leading executable name:
      `vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /InIsolation /TestCaseFilter:"FullyQualifiedName~QfcFormControllerUndoHandoffTests" "/Logger:trx;LogFileName=p7-t8.trx" /ResultsDirectory:FEATURE\evidence\qa-gates\p7-t8`
      from `WORKTREE`. Write `FEATURE/evidence/qa-gates/p7-t8-new-tests-visible.TIMESTAMP.md` with the
      four required fields and the verbatim list of test names found in the produced TRX file.
      Acceptance: the artifact records `EXIT_CODE: 0`, an `outcome="Failed"` count of 0, an
      `outcome="Passed"` count of 5, and all five names
      `BackGroundMoveAsync_WithPendingQueueItem_DoesNotDispatchCleanupBeforeDrain`,
      `BackGroundMoveAsync_WithPendingQueueItem_DoesNotWriteMetricsBeforeDrain`,
      `BackGroundMoveAsync_AfterQueueDrains_WritesMetricsThenCleansUp`,
      `BackGroundMoveAsync_WhenParentIsNull_ReturnsWithoutThrowing`, and
      `BackGroundMoveAsync_WhenGroupsIsNull_ReturnsWithoutTouchingQueue`.
- [x] [P7-T9] Compare the baseline and post-change coverage figures. Write
      `FEATURE/evidence/qa-gates/p7-t9-coverage-comparison.TIMESTAMP.md` recording: the baseline
      percentage and denominator classification from P0-T11; the post-change percentage and denominator
      classification from P7-T7; the delta to two decimal places; the `lines-covered` and `lines-valid`
      counters for both; and the per-file rate for `QuickFiler/Controllers/FilerQueue.cs` and
      `QuickFiler/Controllers/QfcFormController.EventHandlers.cs` computed from the post-change file as
      the count of `<line>` elements with a `hits` value greater than 0 divided by the total count of
      `<line>` elements, taken over the union of every `class` element whose `filename` attribute names
      that file. Do not average the `class` elements' `line-rate` attributes: an async method compiles
      to its own state-machine class, so a single source file appears as several `class` elements with
      different denominators, and the mean of their rates is not the file's rate. Acceptance: the
      artifact records both denominator classifications as `FILTERED`, a numeric repository-wide delta,
      and a per-file rate for `QuickFiler/Controllers/FilerQueue.cs` of at least 0.90, computed as
      stated. If either denominator classification is
      `UNFILTERED`, record `COMPARISON: NOT PERFORMED — mixed denominators` and do not report a delta;
      a red-run figure compared to a green-run figure manufactures a phantom regression.
- [x] [P7-T10] Record the changed-line coverage check. From
      `git diff origin/main -- QuickFiler/Controllers/FilerQueue.cs QuickFiler/Controllers/QfcFormController.EventHandlers.cs`,
      enumerate every added or modified production line number, then read the `line` elements of the
      post-change Cobertura file for those two files and record which of those line numbers have a
      `hits` value of 0. The two-dot form is required and matches P6-T8: it compares the working tree
      against the base, so it enumerates the same file text that P7-T6 measured. A three-dot form would
      be commit-to-commit, and the last commit before this phase is P2-T7, which covers Phases 1 and 2
      only, so the entire fix would be missing from the changed-line set. The working-tree form is also
      what survives P7-T2, which may reformat these two files after any earlier commit and shift their
      line numbers away from the numbers in the Cobertura file. If `git rev-parse origin/main` at this
      point differs from the value P0-T3 recorded, re-run with the merge-base SHA that P0-T3 recorded
      substituted for `origin/main` and record both outputs. Write
      `FEATURE/evidence/qa-gates/p7-t10-changed-line-coverage.TIMESTAMP.md`
      with the four required fields, the changed-line count, and the uncovered changed-line list.
      Acceptance: the artifact records a changed-line count greater than 0, and the uncovered
      changed-line list is a subset of one named exemption set whose sole member is the production
      default `ItemProcessor` initializer lambda in `QuickFiler/Controllers/FilerQueue.cs` added by
      P1-T1. The quote-and-justify requirement is conditional, and deliberately so: **only if** that
      initializer line appears in the uncovered list must the artifact quote the line and state why it
      was not executed by any test in this run. An empty uncovered list satisfies the acceptance without
      any quotation, and P8-T24 accepts an empty list on the same terms.
      The condition is written conditionally because the line may well be covered. A blanket claim that
      no unit test can execute it would be false:
      `QuickFiler.Test/Controllers/QfcItemController.TestSupport.cs:350` has
      `InjectFilingCollaborators` hand the controller an `IFilerHomeController` whose `FilerQueue`
      getter returns a real `FilerQueue`, which carries the P1-T1 production default `ItemProcessor`, so
      any test that files through that helper can enter the default lambda and give it `hits` greater
      than 0. When the line is uncovered, the justification the artifact records is that this run's
      tests assigned a fake processor on every path that reached the worker, and that the real
      `EmailFiler.SortAsync` path the lambda names casts to a live Outlook COM `Folder`.
      Any uncovered changed line outside that set is `REMEDIATION-REQUIRED`. If the changed-line count
      is 0 the gate is vacuous and the task must be recorded as `REMEDIATION-REQUIRED` rather than
      checked.

### Phase 8 — Traceability, acceptance check-off, and handoff

- [x] [P8-T1] Sanitise the evidence tree before it is committed. Across every file under
      `FEATURE/evidence/`, replace all case-insensitive occurrences of the absolute worktree path with
      the literal token `WORKTREE`, covering all three spellings that appear in this evidence set: the
      backslash form, the forward-slash form, and the doubled-backslash form. This is required because
      the eight TRX trees and the eight `.msbuild.txt` logs this plan commits both embed this machine's
      full user-profile path, and repository artifact hygiene prohibits absolute host paths in committed
      artifacts. Re-scan and record only the COUNT of remaining matches in
      `FEATURE/evidence/qa-gates/p8-t1-sanitisation.TIMESTAMP.md` with `Timestamp:`, `Command:`,
      `EXIT_CODE:`, and `Output Summary:`. Do not quote a matched path in the artifact: a quoted host
      path becomes a match on the next sweep. Additionally record the count of files under
      `FEATURE/evidence/` whose file name contains the token produced by
      `Split-Path -Leaf $env:USERPROFILE`; do not write that token into the artifact. Acceptance
      additionally requires that this count is 0. Apply the same test to directory names under
      `FEATURE/evidence/` and record that count too, for the reason given in P2-T7. Acceptance: the
      recorded remaining-match count for the absolute worktree path is 0, the recorded account-token
      file-name count is 0, and the recorded account-token directory-name count is 0.
- [x] [P8-T2] Commit every source, test, project, and evidence change produced by Phases 3 through 7,
      including the sanitised evidence and the P8-T1 artifact, with a message naming issue 633.
      Acceptance: `git status --porcelain` in `WORKTREE`, restricted to
      the paths `QuickFiler/` and `QuickFiler.Test/`, produces no output, and restricted to
      `docs/features/active/2026-08-26-qfc-unsynchronized-undo-handoff-after-batch-move-633/` lists no
      path other than this plan file, whose checkbox for this task is written after the commit.
- [x] [P8-T3] Run `git diff --name-only origin/main...HEAD` and `git status --porcelain` in `WORKTREE`
      as two commands, and write both outputs verbatim into
      `FEATURE/evidence/qa-gates/p8-t3-diff-scope.TIMESTAMP.md` with the four required fields for each
      command. The porcelain companion is required because a name-listing diff is blind to untracked
      files. Acceptance: the artifact records that the diff name list contains exactly two paths under
      `QuickFiler/`, namely `QuickFiler/Controllers/FilerQueue.cs` and
      `QuickFiler/Controllers/QfcFormController.EventHandlers.cs`; that every other listed path is under
      `QuickFiler.Test/`, `docs/`, or `.claude/agent-memory/`, or is exactly
      `artifacts/orchestration/orchestrator-state.json`; and that the porcelain output contains no
      path under `QuickFiler/` or `QuickFiler.Test/`, and that every other path it lists is under
      `docs/features/active/2026-08-26-qfc-unsynchronized-undo-handoff-after-batch-move-633/`, under
      `.claude/agent-memory/`, or is exactly `artifacts/orchestration/orchestrator-state.json`.
      An unrestricted "porcelain output is empty" condition would be unsatisfiable where this task runs
      and is deliberately not used: tracked files under `.claude/agent-memory/` are modified and are not
      committed until P8-T26; checking off P8-T2 itself dirties this plan file, which lives under the
      feature folder; and `artifacts/orchestration/orchestrator-state.json` is tracked,
      orchestrator-owned state written concurrently and outside the executor's control.
      The `.claude/agent-memory/` exception is recorded here rather than discovered late: that directory
      is tracked in this repository and the planning agent wrote planner-memory files into
      `.claude/agent-memory/atomic-planner/` during plan authoring. Those files are agent
      infrastructure, are neither production nor test code, and do not widen the production diff that
      AC16 constrains. Record the exact list of `.claude/agent-memory/` paths in the artifact so a
      reviewer can confirm none of them is source.
- [x] [P8-T4] Update `FEATURE/spec.md` `- **Last Updated:**` to the current ISO-8601 instant and add one
      sentence under `## Deviation from the research record` recording the fail-before split decided by
      this plan: that the barrier defect carries the real failing run and that the orphan-window
      regression and the drain suite carry the exception dossier, with the reason. Do not renumber,
      reword, or reorder any acceptance-criteria bullet; the AC anchors in this plan depend on their
      current text. This task runs before the twenty check-off tasks, so at this point the only spec
      changes are its own. Acceptance: `git diff HEAD -- docs/features/active/2026-08-26-qfc-unsynchronized-undo-handoff-after-batch-move-633/spec.md`
      shows changed lines only on the `Last Updated` metadata line and below the
      `## Deviation from the research record` heading, and shows zero changed lines between the
      `## Acceptance Criteria` heading and the `## Risks & Mitigations` heading. The `HEAD` operand is
      required and neither a three-dot nor a two-dot base-branch form works here. A
      `origin/main...HEAD` diff is commit-to-commit, so it cannot see this task's own uncommitted edit;
      and once P8-T2 has committed the previously untracked feature folder, a base-branch diff renders
      the whole of `spec.md` as added, which makes the zero-changed-lines condition false for reasons
      that have nothing to do with this edit. The `HEAD` form compares the working file against the
      just-committed version and therefore shows exactly this task's hunks.
- [x] [P8-T5] Mark AC1 complete in `FEATURE/spec.md`: change the checkbox of the acceptance bullet whose
      line contains the anchor `exposes` from `- [ ]` to `- [x]`. Acceptance: that line begins with
      `- [x]`, and `QuickFiler/Controllers/FilerQueue.cs` contains the literal token
      `public Task WhenDrainedAsync()`, and the P5-T10 artifact records
      `WhenDrainedAsync_OnFreshQueue_ReturnsCompletedTask` as passed.
- [x] [P8-T6] Mark AC2 complete: change the checkbox of the bullet whose line contains the anchor
      `The drain task does not complete while any enqueued item` to `- [x]`. Acceptance: that line
      begins with `- [x]` and the P5-T10 artifact records both
      `WhenDrainedAsync_WithGatedItem_DoesNotCompleteBeforeItemCompletes` and
      `WhenDrainedAsync_WithTwoGatedItems_CompletesOnlyAfterBothComplete` as passed.
- [x] [P8-T7] Mark AC3 complete: change the checkbox of the bullet whose line contains the anchor
      `The drain task completes once every enqueued item has completed` to `- [x]`. Acceptance: that
      line begins with `- [x]` and the P5-T10 artifact records
      `WhenDrainedAsync_AfterGateReleased_CompletesAndItemRanOnce` as passed.
- [x] [P8-T8] Mark AC4 complete: change the checkbox of the bullet whose line contains the anchor
      `is idempotent: repeated and concurrent waiters all complete` to `- [x]`. Acceptance: that line
      begins with `- [x]` and the P5-T10 artifact records
      `WhenDrainedAsync_AwaitedTwice_BothWaitersComplete` as passed.
- [x] [P8-T9] Mark AC5 complete: change the checkbox of the bullet whose line contains the anchor
      `The orphaned-item window is closed` to `- [x]`. Acceptance: that line begins with `- [x]`, the
      P5-T10 artifact records `Enqueue_AfterPreviousBatchDrained_ProcessesSecondBatch` as passed, and
      the P2-T6 dossier records why this test is not a fail-before witness: that it names
      `WhenDrainedAsync()` and so cannot compile before Phase 3, and that the orphaned-item window has
      no deterministic pre-fix witness at all.
- [x] [P8-T10] Mark AC6 complete: change the checkbox of the bullet whose line contains the anchor
      `An item whose processing throws still decrements` to `- [x]`. Acceptance: that line begins with
      `- [x]` and the P5-T10 artifact records
      `ItemProcessor_ThatThrows_StillDecrementsAndDrainCompletes` as passed.
- [x] [P8-T11] Mark AC7 complete: change the checkbox of the bullet whose line contains the anchor
      `awaits` to `- [x]`. Acceptance: that line begins with `- [x]`, the P4-T2 statement-order
      assertion holds, and the P7-T8 artifact records both
      `BackGroundMoveAsync_WithPendingQueueItem_DoesNotDispatchCleanupBeforeDrain` and
      `BackGroundMoveAsync_WithPendingQueueItem_DoesNotWriteMetricsBeforeDrain` as passed.
- [x] [P8-T12] Mark AC8 complete: change the checkbox of the bullet whose line contains the anchor
      `The existing metrics-before-cleanup ordering is preserved` to `- [x]`. Acceptance: that line
      begins with `- [x]` and the P7-T8 artifact records
      `BackGroundMoveAsync_AfterQueueDrains_WritesMetricsThenCleansUp` as passed.
- [x] [P8-T13] Mark AC9 complete: change the checkbox of the bullet whose line contains the anchor
      `The early-return guard in` to `- [x]`. Acceptance: that line begins with `- [x]`,
      `QuickFiler/Controllers/QfcFormController.EventHandlers.cs` contains the literal token
      `_parent is null` on the `BackGroundMoveAsync` guard line, and the P7-T8 artifact records
      `BackGroundMoveAsync_WhenParentIsNull_ReturnsWithoutThrowing` as passed.
- [x] [P8-T14] Mark AC10 complete: change the checkbox of the bullet whose line contains the anchor
      `The two production reads of` to `- [x]`. Acceptance: that line begins with `- [x]` and the P6-T4
      artifact records a `\.Consumer\b` match count of 0 over `QuickFiler/**/*.cs`.
- [x] [P8-T15] Mark AC11 complete: change the checkbox of the bullet whose line contains the anchor
      `remains declared with the same type` to `- [x]`. Acceptance: that line begins with `- [x]`, the
      P3-T7 declaration check holds, and the P6-T10 artifact records
      `FilerQueue_NewInstance_HasCompletedConsumerByDefault` as passed with its body byte-identical to
      the pre-change text.
- [x] [P8-T16] Mark AC12 complete: change the checkbox of the bullet whose line contains the anchor
      `still raises` to `- [x]`. Acceptance: that line begins with `- [x]` and the P6-T8 artifact
      records `MoveMailAsync_WhenEnqueueThrows_WrapsArgumentNullException` as passed with an empty diff
      for its file.
- [x] [P8-T17] Mark AC13 complete: change the checkbox of the bullet whose line contains the anchor
      `is reconciled with the new` to `- [x]`. Acceptance: that line begins with `- [x]`, the P3-T8
      zero-match check for `GetField("guard"` holds, and the P6-T9 artifact records
      `MoveMailAsync_WhenOneDrivePresent_InvokesFactoryWithConfigAndEnqueues` as passed.
- [x] [P8-T18] Mark AC14 complete: change the checkbox of the bullet whose line contains the anchor
      `contains no banned wait API` to `- [x]`. Acceptance: that line begins with `- [x]` and the P6-T1
      artifact records a total match count of 0 across the three named test files.
- [x] [P8-T19] Mark AC15 complete: change the checkbox of the bullet whose line contains the anchor
      `is introduced` to `- [x]`. Acceptance: that line begins with `- [x]`, the P6-T2 artifact records
      a total match count of 0, and the P7-T5 artifact records `EXIT_CODE: 0` with no CS0518 diagnostic
      in its log.
- [x] [P8-T20] Mark AC16 complete: change the checkbox of the bullet whose line contains the anchor
      `The production diff touches no file other than` to `- [x]`. Acceptance: that line begins with
      `- [x]` and the P8-T3 artifact satisfies its own acceptance condition.
- [x] [P8-T21] Mark AC17 complete: change the checkbox of the bullet whose line contains the anchor
      `contains a` to `- [x]`. Acceptance: that line begins with `- [x]`, the P6-T7 artifact records a
      match count of 1, and the P7-T8 artifact lists all five new test names.
- [x] [P8-T22] Mark AC18 complete: change the checkbox of the bullet whose line contains the anchor
      `Both changed production files remain under 500 lines` to `- [x]`. Acceptance: that line begins
      with `- [x]` and the P6-T5 artifact records both counts as at most 500.
- [x] [P8-T23] Mark AC19 complete: change the checkbox of the bullet whose line contains the anchor
      `The full C# toolchain passes in a single uninterrupted pass` to `- [x]`. Acceptance: that line
      begins with `- [x]` and the four artifacts P7-T3, P7-T4, P7-T5, and P7-T6 all record
      `EXIT_CODE: 0`, with P7-T4 and P7-T5 each recording a `Skipping target "CoreCompile"` occurrence
      count of 0 and P7-T6 recording a failed count of 0, all four produced within one uninterrupted
      pass of Phase 7 with no intervening file edit.
- [x] [P8-T24] Mark AC20 complete: change the checkbox of the bullet whose line contains the anchor
      `Coverage does not regress on any line changed by this fix` to `- [x]`. Acceptance: that line
      begins with `- [x]`, the P7-T9 artifact records both denominators as `FILTERED` with a
      `QuickFiler/Controllers/FilerQueue.cs` per-file rate of at least 0.90, and the P7-T10 artifact
      records an uncovered changed-line list that is a subset of the single-member exemption set defined
      in P7-T10 — the production default `ItemProcessor` initializer lambda in
      `QuickFiler/Controllers/FilerQueue.cs`. An empty list
      also satisfies this condition; an uncovered changed line outside that set does not. Do not require
      a quotation or a justification paragraph when the list is empty: per P7-T10 that requirement is
      conditional on the initializer line actually appearing in the uncovered list, because
      `QuickFiler.Test/Controllers/QfcItemController.TestSupport.cs:350` supplies a real `FilerQueue`
      carrying the production default processor and the line may therefore be covered.
- [x] [P8-T25] Write `FEATURE/evidence/issue-updates/issue-633.TIMESTAMP.md` containing `Timestamp:`,
      the exact text of the status update intended for GitHub issue 633 summarizing the barrier, the
      handshake repair, the `_parent` guard clause, and the two removed `Consumer` awaits, and
      `PostedAs: body`, `PostedAs: comment`, or a `POSTING BLOCKED` header with the reason. Mirror the
      same update into `FEATURE/issue.md` if `PostedAs: body` is used. Acceptance: the artifact exists
      and contains exactly one `PostedAs:` line or one `POSTING BLOCKED` header.
- [x] [P8-T26] Close out the plan file, then commit. In this order: set this plan file's
      `- **Last Updated:**` to the completion instant and `- **Status:**` to `Complete`; mark every
      remaining checkbox in this plan, including this one; then commit every remaining change other than
      `artifacts/orchestration/orchestrator-state.json`, including any file under
      `.claude/agent-memory/`. The order is load-bearing. Committing first and
      checking this box afterwards re-dirties the plan file, so a terminal clean-tree gate taken after
      the check-off could never pass. Acceptance: `git status --porcelain` in `WORKTREE` lists no path
      other than `artifacts/orchestration/orchestrator-state.json`, and a search of this plan file for
      the line-anchored pattern `^- \[ \] \[P` returns zero matches. The `^` anchor is required for a
      different reason than a self-match hazard: this task quotes the pattern in its escaped form,
      `- \[ \] \[P`, which does not contain the bare literal, so an unanchored search would not match
      the quotation either. The anchor is required because it pins the match to a task line at column 0,
      which is the only position an unchecked task box occupies in this plan. The single permitted
      residual path is orchestrator-owned state written
      concurrently and outside the executor's control.

---

## Planner Adversarial Self-Review

Every citation below was re-derived against the working tree during this authoring pass. No citation was
carried forward from the spec, the research record, or the delegation prompt without an independent
read. Sibling regions of every edited citation were re-checked. The revision pass dated 2026-08-31T22-10
re-derived every citation its own edits touched, listed first below; the 2026-08-31T21-25 pass and the
initial authoring pass follow it.

SELF-REVIEW: RE-DERIVED THIS PASS

Re-derived in the 2026-08-31T22-10 revision pass (each read directly against the working tree in this
pass; no figure was carried forward from the 2026-08-31T21-25 pass):

- `QuickFiler/Controllers/FilerQueue.cs:56` — `var first = item.Helpers.First();`, the first statement
  of the `catch (Exception e)` block that opens at 54. Re-read in this pass. This is the statement that
  makes an empty `Helpers` list a hang rather than a failure, and it is the citation B1 rests on.
- `QuickFiler/Controllers/FilerQueue.cs:70-78` — the `FilerQueueItem` constructor. `Filer.ThrowIfNull()`
  at 72, `Helpers.ThrowIfNull()` at 73, and `if (helpers.Any(h => h is null))` at 74 throwing at 76.
  Re-derived conclusion: an EMPTY list passes all three checks, because `Any` over an empty sequence is
  false. Sibling re-check: `Filer` at 80 and `Helpers` at 81 are get-only auto-properties, so no other
  member re-validates the list after construction.
- `QuickFiler/Controllers/FilerQueue.cs:48` — `while (Queue.TryTake(out var item))`. Sibling re-check:
  the `try` opens at 50, the seam call sits at 52, the `catch` spans 54-61, and the guard reinstall is
  at 63; an exception raised inside the catch body at 56 is not caught by that same catch and therefore
  exits the `while` loop, which is the hang mechanism B1 describes. The file is 83 lines.
- `QuickFiler.Test/Controllers/FilerQueueTests.cs:23` — `private static List<MailItemHelper> OneHelper()`
  with its expression body `new List<MailItemHelper> { new MailItemHelper() }` on line 24. Sibling
  re-check: the file is 89 lines, has five `[TestMethod]` members, and contains no `[Timeout]`
  attribute, confirmed by reading it in full in this pass.
- `UtilitiesCS/OutlookObjects/MailItem/MailItemHelper.cs:80` — the parameterless `MailItemHelper()`
  constructor, whose first statement at 82 is `InitializeSafeDefaults();`. Sibling re-check:
  `InitializeSafeDefaults` at 167 assigns `_sentOn` at 179, `_subject` at 180, and `_senderName` at 182
  to `string.Empty.ToLazy()`, so the three members the `logger.Error` diagnostic reads resolve without
  touching COM. `_globals` is set to null at 175 and `_item` is never assigned on this path.
- `QuickFiler/Controllers/QfcFormController.cs:82-83` — `private delegate Task WriteMetricsDelegate(string filename);`
  at 82 and `private WriteMetricsDelegate WriteMetrics;` at 83. Sibling re-check: `_parent` is declared
  `IQfcHomeController` at 81 and `IterateDelegate`/`Iterate` at 84-85, so the field ordering the P2-T2
  reflection fixture depends on is unchanged.
- `QuickFiler/Controllers/QfcFormController.EventHandlers.cs:228-231` — `await UiThread.Dispatcher.InvokeAsync(`
  at 228, the lambda `async () => await WriteMetrics(_globals.FS.Filenames.EmailSession),` at 229, and
  `System.Windows.Threading.DispatcherPriority.ContextIdle` at 230. Re-derived conclusion: the
  `DispatcherOperation` completes when that lambda returns its task, not when the task completes, which
  is the mechanism B2 addresses. Sibling re-check: the guard at 219 is
  `if (_groups is null || _globals?.FS?.Filenames is null || WriteMetrics is null)` with no `_parent`
  clause; line 225 is `await _groups.MoveEmailsAsync(_movedItems);`; line 233 is the cleanup dispatch;
  `BackGroundMoveAsync` spans 215-234.
- `QuickFiler.Test/Controllers/QfcItemController.SeamFactoryTests.cs:4` — `using System.Reflection;` and
  `:18` — `using UtilitiesCS.Threading;`. Re-derived by an exhaustive search of the file for
  `BindingFlags`, `GetField`, `ThreadSafeSingleShotGuard`, `Reflection`, `MethodInfo`, and `typeof(`:
  the only hits are 4, 213, 214, 216, 217, 343, 347, and 421. Lines 343, 347, and 421 are
  `QfcItemControllerTestSupport.GetField(...)`, the test-support helper, not `Type.GetField`. A second
  search for every type name under `UtilitiesCS/Threading/` returned `ThreadSafeSingleShotGuard` at 216
  and `IUiDispatcher` inside a doc comment at 300, which binds nothing. Both directives therefore become
  unused once 213-218 is deleted, and no third directive does.
- `QuickFiler.Test/Controllers/QfcItemController.SeamFactoryTests.cs:210-211` — the comment
  "A real FilerQueue whose single-shot guard is pre-tripped so Enqueue records the item / without
  spinning up the background consumer (deterministic, no external I/O)." Re-derived conclusion: both
  halves are falsified by P3-T2 and P3-T3, so the comment must be replaced rather than left. Sibling
  re-check: the assertions at 230-233 on `captured` are untouched by the repair, and the file carries
  `[Timeout(PumpTimeoutMs)]` at 304 and 375 with `PumpTimeoutMs` declared at 293 — on other test
  methods, not on the one P3-T8 repairs. That falsified executor-environment-note-10 claim that "no
  in-scope test file carries a `[Timeout]` attribute"; the note is corrected in this pass to name the
  two files the leak mechanism actually reaches.
- Single-occurrence check for the three new zero-hit tokens in
  `QuickFiler.Test/Controllers/QfcItemController.SeamFactoryTests.cs`: `pre-tripped` occurs once at 210,
  `System.Reflection` once at 4, and `UtilitiesCS.Threading` once at 18. Each gate is therefore false
  before the repair and true after, not vacuous in either direction.
- `QuickFiler.Test/Controllers/QfcItemController.TestSupport.cs:350` —
  `home.SetupGet(h => h.FilerQueue).Returns(new FilerQueue());` inside `InjectFilingCollaborators`,
  which is declared at 334 and closes at 355. Re-derived conclusion: that helper hands the controller a
  real `FilerQueue` carrying whatever default `ItemProcessor` P1-T1 installs, so the claim that no unit
  test can execute the default lambda is false and the P7-T10 justification must be conditional.
  Sibling re-check: `controller.ItemHelper = new MailItemHelper();` at 351 and the three `SetField`
  calls at 352-354 assign no processor, so nothing in the helper overrides the production default.
- `QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixtureTests.cs:108-110` —
  `UiThreadDispatcherTransaction transaction = await UiThreadDispatcherFixture` at 108,
  `.BeginTransactionAsync()` at 109, `.ConfigureAwait(false);` at 110. Re-derived conclusion: CSharpier
  breaks the `.ConfigureAwait(false)` chained form across lines, which would place
  `BeginTransactionAsync` on a line carrying no `using (` and falsify the N10 mechanical check. The
  plan now forbids that continuation and states the column arithmetic that keeps the single-line form
  under the print width. Sibling re-check: this file carries `[Timeout(GateTimeoutMs)]` at 104 and is
  NOT in the authorized blast radius, so it is not edited; a repo-wide search found
  `BeginTransactionAsync` in only four files, none of them the two the N10 clauses gate.
- `QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixture.cs` — a search for `Timeout`
  returns exactly one hit, a doc-comment mention at line 25, and no `[Timeout]` attribute. The
  transaction-leak argument in P2-T3, P5-T9, and executor environment note 10 therefore still holds for
  the two files those clauses name.
- Repository root — no `.csharpierrc`, `.csharpierrc.json`, or `.csharpierrc.yaml` file exists, so
  CSharpier's default 100-column print width applies. This is the figure the single-line
  `BeginTransactionAsync` acquisition is measured against.
- `dotnet-tools.json` — re-read in full in this pass: `"version": 1`, `"isRoot": true`, and
  `tools.csharpier.version` is `"1.2.6"` with `"rollForward": false`. This is the value the P0-T6
  fallback records when the version switch does not print.
- Plan-internal re-derivation for N6: this plan file quotes the P8-T26 pattern only in its escaped form
  `- \[ \] \[P`, which does not contain the bare literal, so the previously stated self-match rationale
  was wrong. The anchor's actual role is to pin the match to column 0.
- Plan-internal re-derivation for N2: before P1-T2 lands, the only occurrence of `ItemProcessor` that
  P1-T1 itself creates in `QuickFiler/Controllers/FilerQueue.cs` is the property declaration, so an
  "at least twice" acceptance was unsatisfiable by that task alone.
- Plan-internal re-derivation for N7: the P0-T1 through P2-T6 check-offs all edit this plan file, which
  lives under `docs/features/active/2026-08-26-qfc-unsynchronized-undo-handoff-after-batch-move-633/`,
  the third path in the P2-T7 porcelain restriction. The restriction was therefore unsatisfiable unless
  the plan file is in the commit, which the task now states.
- Structural re-derivation after all edits in this pass: nine `### Phase N — <Title>` headings
  numbered 0 through 8 in order, and 96 unchecked task lines matching `^- \[ \] \[P[0-9]+-T[0-9]+\]`.
  No task was added, removed, renumbered, or moved between phases.

Re-derived in the 2026-08-31T21-25 revision pass:

- `QuickFiler/Controllers/FilerQueue.cs` — total line count is **83**, not 84. Confirmed by a
  full-file read whose last content line is the namespace close at 83, and by a line-count query
  returning 83. The prior figure was wrong by one and is corrected in the verified-tree-facts table and
  in the citation list below. Sibling re-check: the region facts around it are unaffected —
  `Queue.Add` at 24 and 33, `guard` at 40, `Consumer` at 42, `TryTake` at 48, the per-item call at 52,
  the catch at 54-61, the guard reinstall at 63, and `FilerQueueItem` at 68-82 all re-read at those
  lines.
- `.gitignore` — line 84 is the pattern `*.log`. Every MSBuild file log this plan produced under a
  `.log` name would therefore be untracked and never committed. All eight logger targets are renamed to
  `.msbuild.txt`. Sibling re-check: lines 75-91 of `.gitignore` carry no pattern matching
  `*.msbuild.txt` and none matching `*.txt` or `*.trx`, so the renamed logs and the TRX trees are
  committable.
- `scripts/vscode/Invoke-Restore.ps1` — line 36 runs
  `msbuild <solution> /t:Restore /p:Configuration=... /p:Platform=... /p:RestorePackagesConfig=true /m`,
  resolving MSBuild through `vswhere` at line 27. This is the repo-sanctioned restore and covers both
  project styles, so P0-T5 calls it instead of `nuget restore`.
- `QuickFiler.Test/Controllers/QfcItemController.FocusAndThemeTests.cs` — lines 452 and 468 call
  `QfcItemControllerTestSupport.EnsureUiThreadDispatcher()` and discard the returned scope.
- `QuickFiler.Test/Controllers/QfcItemController.TestSupport.cs` — lines 229-236 state that the returned
  scope reverts the seeding only while the static still holds the exact instance installed and that
  discarding the scope is permitted and leaks. Sibling re-check: `StartRunningDispatcher` at 251-271
  returns a dispatcher on a running STA thread and `ShutdownDispatcher` at 277-280 calls
  `InvokeShutdown()`, so the pinned-dispatcher arrangement P2-T3 now requires is available and
  cleanly reversible. Consequence: a null `UiThread.Dispatcher` is not guaranteed, so the previous
  `NullReferenceException` premise for the fail-before discriminator is withdrawn.
- `QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixture.cs` — `BeginTransactionAsync`
  at 122-126 awaits `TransactionGate.WaitAsync()`; `UiThreadDispatcherTransaction.Dispose` at 261-276 is
  the only path that calls `ReleaseTransactionGate()`. Neither this file nor the new test file carries a
  `[Timeout]` attribute, so a leaked permit hangs rather than fails. P2-T3 and P5-T9 now require the
  `using` statement form.
- `QuickFiler/Controllers/QfcFormController.EventHandlers.cs` — lines 228-231 and 233 are both
  `await UiThread.Dispatcher.InvokeAsync(...)`, the first at `DispatcherPriority.ContextIdle`. Under a
  pinned running dispatcher the pre-fix method therefore yields at line 228 and its returned task is
  incomplete on return, so `IsCompleted` cannot discriminate; the discriminator is the equal-priority
  probe described in the fail-before strategy. Sibling re-check: line 225 is the awaited
  `_groups.MoveEmailsAsync(_movedItems)`, which the P2-T2 mock completes synchronously, which is what
  places the metrics operation in the dispatcher queue before the method returns.
- `QuickFiler.Test/Controllers/QfcItemController.SeamFactoryTests.cs` — `GetField("guard", ...)` at
  213-215, `ThreadSafeSingleShotGuard._state` set at 216-218, `filerQueue.Queue.Count.Should().Be(1)` at
  234. Sibling re-check: `BindingFlags` occurs only at 214 and 217 and `ThreadSafeSingleShotGuard` only
  at 216, so both usings become removable after the repair, and the surrounding assertions at 230-233
  on `captured` are untouched by it. The post-fix worker removes the item before invoking
  `ItemProcessor`, so the line-234 count assertion is deleted rather than preserved.
- `docs/features/active/2026-08-26-qfc-unsynchronized-undo-handoff-after-batch-move-633/spec.md` —
  `## Acceptance Criteria` at 574 and `## Risks & Mitigations` at 659 bound the check-off region.
  Outside it, `exposes` occurs at 375; `awaits` at 44, 73, 188, 237, 239, 305, 671, and 733; and
  `is introduced` at 316 and 532. Inside it each of the three occurs once, at 580, 599, and 630
  respectively, so all twenty anchors are unique under the stated constraint.

Re-derived in the initial authoring pass and unchanged by the revision:

- `QuickFiler/Controllers/FilerQueue.cs` — read in full; `Queue.Add` at 24 and 33;
  `guard.CheckAndSetFirstCall` at 25 and 34; `guard` field at 40; `Consumer` at 42; `TryTake` loop at
  48; per-item call at 52; `catch` at 54-61; guard reinstall at 63; `FilerQueueItem` at 68-82.
- `QuickFiler/Controllers/QfcFormController.EventHandlers.cs` — read lines 140-239 and counted 399 total
  lines; `MoveAndIterate` at 145; `.Consumer` reads at 167 and 193; `BackGroundMoveAsync` at 215-234;
  guard at 219 with no `_parent` clause; `MoveEmailsAsync` await at 225; metrics dispatch at 228-231;
  cleanup dispatch at 233.
- `QuickFiler/Controllers/QfcFormController.cs` — `_parent` field declared `IQfcHomeController` at 81;
  `WriteMetricsDelegate` declared at 82 and the `WriteMetrics` field at 83; `_movedItems` at 86;
  `_globals` at 71; `_groups` at 157; constructor assignments at 47 and 49.
- `QuickFiler/Controllers/QfcFormController.SetupDisposal.cs` — `_parent = null;` at 224, with the
  sibling lines 225 and 226 nulling `_movedItems` and `WriteMetrics`, which is why the P5-T9 null-parent
  test must set `_parent` to null without relying on `Cleanup()`.
- `UtilitiesCS/EmailIntelligence/EmailParsingSorting/EmailFiler.cs` — `SortAsync(IList<MailItemHelper>)`
  at 128 returns `Task<bool>`, confirming the `Func<FilerQueueItem, Task>` seam type needs no generic.
- `QuickFiler/Properties/AssemblyInfo.cs` — `InternalsVisibleTo("QuickFiler.Test")` at line 5.
- `QuickFiler.Test/Controllers/FilerQueueTests.cs` — read in full; 89 lines; class comment at 12-19;
  five `[TestMethod]` members; `FilerQueue_NewInstance_HasCompletedConsumerByDefault` at 76-87.
- `QuickFiler.Test/Controllers/QfcItemController.SeamFactoryTests.cs` — read lines 180-239 and counted
  436 total lines; `GetField("guard", ...)` at 213-215; `ThreadSafeSingleShotGuard._state` set at
  216-218; `filerQueue.Queue.Count.Should().Be(1)` at 234. Sibling check: the surrounding test also
  asserts `captured.Globals`, `captured.OlAncestor`, and `captured.FsAncestorEquivalent` at 231-233,
  none of which the P3-T8 repair touches.
- `QuickFiler.Test/Controllers/QfcFormControllerSeamTests.cs` — `PrivateInstance` at 41,
  `GetPrivateField` at 43-44, `SetPrivateField` at 46-47, `CreateQfcFormController` at 64-76 with its
  eight constructor arguments.
- `QuickFiler.Test/Controllers/QfcItemController.TestSupport.cs` — `SetField` at 40,
  `StartRunningDispatcher` at 251, `ShutdownDispatcher` at 277.
- `QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixture.cs` — `EnsureDispatcher` at
  99, `BeginTransactionAsync` at 122, `Install` at 242.
- `QuickFiler.Test/QuickFiler.Test.csproj` — explicit `Compile Include` items;
  `Controllers\FilerQueueTests.cs` at 113, `Controllers\QfcFormControllerTests.cs` at 147,
  `Controllers\QfcFormControllerSeamTests.cs` at 148.
- `scripts/vscode/Invoke-MSTestWithCoverage.ps1` — argument list built at 70-77 appending `/Settings`,
  `/InIsolation`, `/TestCaseFilter:TestCategory!=LiveOutlook`; throw on non-zero coverage exit at 236;
  post-processing at 338-344 with `Set-Content` at 343.
- `scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1` — `Get-KoverageProjectAllowlist` at 4 excluding
  `.Test` assembly names at 40-42; `ConvertTo-KoverageCoberturaXml` at 393 removing non-allowlisted
  packages at 417-421, injecting `sources` at 430-439, and rewriting the six summary attributes at
  441-447; `Assert-CoberturaLineCoverageThreshold` at 459 throwing below 80 percent at 487-490.
- `global.json` — SDK `8.0.205`, `paths` `.dotnet-sdk` and `$host$`, error message naming
  `scripts/vscode/Install-RepoDotNetSdk.ps1`, which was confirmed to exist.
- `dotnet-tools.json` — repository-root manifest, `isRoot` true, `csharpier` pinned to `1.2.6`. Also
  confirmed no `.config/dotnet-tools.json` exists, and that `.github/workflows/_format-check.yml` runs
  `dotnet tool restore` at line 37 against this manifest.
- Worktree state — `.dotnet-sdk` and `packages` both absent from `WORKTREE`.
- Zero-hit gates re-derived so they are satisfiable rather than vacuous:
  `#nullable|\brecord\b|\binit\s*[;{]` returns 0 matches over the two production files today;
  `Thread\.Sleep|Task\.Delay|\.Wait\(|\.Result\b|DateTime\.(Now|UtcNow)` returns 0 matches over
  `FilerQueueTests.cs` and `QfcItemController.SeamFactoryTests.cs` today; `\.Consumer\b` over
  `QuickFiler/**/*.cs` returns exactly the two matches at `QfcFormController.EventHandlers.cs:167` and
  `:193` today, so the post-change zero-match gate is false before and true after.

Sibling-invalidation checks performed:

- Removing the `guard` field invalidates `QfcItemController.SeamFactoryTests.cs:213-218`. Covered by
  P3-T8 in the same phase as the removal, so no full-suite run occurs between the two.
- Adding the seam at `FilerQueue.cs:52` sits inside the `try` whose `catch` at 54-61 dereferences
  `item.Helpers.First()`. P1-T2 and P3-T5 both require that block to remain unchanged.
- Adding the `_parent` clause at `EventHandlers.cs:219` affects
  `QuickFiler.Test/Controllers/QfcFormControllerTests.cs:431-455`, whose two tests are vacuous because
  `_groups` is null; the `_groups` clause short-circuits first and `_parent` is a non-null mock in that
  fixture, so those tests need no edit. They are not in the authorized blast radius.
- `SeamFactoryTests.cs` has only 64 lines of headroom under the 500-line limit. The P3-T8 repair
  deletes the six-line reflection block at 213-218 and the count assertion at 234, and adds a captured
  filer local, two `TaskCompletionSource` declarations, a seam assignment, an await, two assertions,
  and a `finally`. The net change is a small growth, not a shrink, and the headroom absorbs it with
  room to spare. P6-T6 verifies the resulting count against the 500-line limit regardless.
- Pinning the dispatcher in P2-T3 and P5-T9 introduces a second consumer of the process-wide
  `TransactionGate` inside the same assembly run. That is the intended use of the fixture: the gate
  serialises transactions, and every acquisition in this plan is inside a `using` statement, so no
  acquisition can outlive its test even on an assertion-failure path.
- The repo-wide `csharpier format .` in P7-T2 could rewrite files outside the blast radius and falsify
  AC16. P7-T2 mandates a before-and-after porcelain comparison and restoration of any out-of-scope path,
  and P0-T7 records pre-existing drift so the restoration set is bounded and auditable.
- That same restoration invalidates the Phase 7 header's unconditional restart rule. P7-T2 restores the
  out-of-scope paths CSharpier rewrote, so P7-T3 reports them again on every iteration and the loop
  cannot converge. The header now states that the P7-T3 `REMEDIATION-REQUIRED` branch terminates the
  loop; every other failure still restarts it.
- Naming an explicit TRX file name in the eight scoped runs does not invalidate any downstream
  acceptance: each of those tasks reads `outcome="Passed"` or `outcome="Failed"` counts and test names
  out of "the produced TRX file" in a per-task `/ResultsDirectory`, and none of them names the file.
  The change does invalidate the premise of the P2-T7 and P8-T1 account-token measurements, which
  previously could not be gated because the TRX names guaranteed a non-zero count; both now gate on 0.
- Deleting `QfcItemController.SeamFactoryTests.cs:213-218` invalidates the sibling comment at 210-211,
  which describes a guard that will no longer exist and a consumer that will now start. P3-T8 replaces
  it in the same task, and the acceptance gates the stale token `pre-tripped` to zero matches.
- Removing the two `using` directives from that file invalidates nothing else in it: an exhaustive
  search established that `System.Reflection` is needed only by `BindingFlags` at 214 and 217, and that
  the only `UtilitiesCS.Threading` identifier outside line 216 is a doc-comment mention of
  `IUiDispatcher` at 300.
- Executor environment note 10's claim that no in-scope test file carries `[Timeout]` was falsified by
  `QfcItemController.SeamFactoryTests.cs:304` and `:375`. The note is corrected to name the two files
  the transaction-leak mechanism actually reaches. The claim is unchanged in substance because that file
  acquires no transaction.
- The N10 mechanical clause interacts with P7-T2. If the acquisition were written in the repo's
  prevailing `.ConfigureAwait(false)` chained shape, CSharpier would split it and the post-format file
  would fail a clause that passed before formatting. Executor environment note 10 forbids that
  continuation and records the column arithmetic that keeps the single-line form intact.

## Planner Internal Review Record

PLANNER-INTERNAL-REVIEW: PASS

CITATION-TO-TREE: PASS
AC-TRACEABILITY: PASS
SCOPE-BOUNDARY: PASS

CITATION: QuickFiler/Controllers/FilerQueue.cs | line 24 Queue.Add before line 25 guard.CheckAndSetFirstCall
CITATION: QuickFiler/Controllers/FilerQueue.cs | line 33 Queue.Add before line 34 guard.CheckAndSetFirstCall
CITATION: QuickFiler/Controllers/FilerQueue.cs | line 40 ThreadSafeSingleShotGuard guard field
CITATION: QuickFiler/Controllers/FilerQueue.cs | line 42 public Task Consumer default Task.CompletedTask
CITATION: QuickFiler/Controllers/FilerQueue.cs | line 48 while (Queue.TryTake(out var item))
CITATION: QuickFiler/Controllers/FilerQueue.cs | line 52 hard-coded item.Filer.SortAsync(item.Helpers)
CITATION: QuickFiler/Controllers/FilerQueue.cs | lines 54-61 catch block and logger.Error diagnostic
CITATION: QuickFiler/Controllers/FilerQueue.cs | line 63 guard reinstall inside the Task.Run body
CITATION: QuickFiler/Controllers/FilerQueue.cs | file line count 83
CITATION: QuickFiler/Controllers/QfcFormController.EventHandlers.cs | line 167 await _parent.FilerQueue.Consumer
CITATION: QuickFiler/Controllers/QfcFormController.EventHandlers.cs | line 193 await _parent.FilerQueue.Consumer
CITATION: QuickFiler/Controllers/QfcFormController.EventHandlers.cs | lines 215-234 BackGroundMoveAsync
CITATION: QuickFiler/Controllers/QfcFormController.EventHandlers.cs | line 219 guard without a _parent clause
CITATION: QuickFiler/Controllers/QfcFormController.EventHandlers.cs | line 225 await _groups.MoveEmailsAsync(_movedItems)
CITATION: QuickFiler/Controllers/QfcFormController.EventHandlers.cs | lines 228-231 WriteMetrics dispatch
CITATION: QuickFiler/Controllers/QfcFormController.EventHandlers.cs | line 233 CleanupBackground dispatch
CITATION: QuickFiler/Controllers/QfcFormController.EventHandlers.cs | file line count 399
CITATION: QuickFiler/Controllers/QfcFormController.cs | line 81 private IQfcHomeController _parent
CITATION: QuickFiler/Controllers/QfcFormController.cs | lines 82-83 private WriteMetricsDelegate and WriteMetrics field
CITATION: QuickFiler/Controllers/QfcFormController.SetupDisposal.cs | line 224 _parent = null
CITATION: UtilitiesCS/EmailIntelligence/EmailParsingSorting/EmailFiler.cs | line 128 SortAsync returns Task of bool
CITATION: QuickFiler/Properties/AssemblyInfo.cs | line 5 InternalsVisibleTo QuickFiler.Test
CITATION: QuickFiler.Test/Controllers/FilerQueueTests.cs | lines 12-19 class comment recording the exclusion
CITATION: QuickFiler.Test/Controllers/FilerQueueTests.cs | lines 76-87 FilerQueue_NewInstance_HasCompletedConsumerByDefault
CITATION: QuickFiler.Test/Controllers/FilerQueueTests.cs | file line count 89 and five test methods
CITATION: QuickFiler.Test/Controllers/QfcItemController.SeamFactoryTests.cs | lines 213-218 reflection into guard and _state
CITATION: QuickFiler.Test/Controllers/QfcItemController.SeamFactoryTests.cs | line 234 filerQueue.Queue.Count.Should().Be(1)
CITATION: QuickFiler.Test/Controllers/QfcItemController.SeamFactoryTests.cs | file line count 436
CITATION: QuickFiler.Test/Controllers/QfcFormControllerSeamTests.cs | lines 43-47 GetPrivateField and SetPrivateField
CITATION: QuickFiler.Test/Controllers/QfcFormControllerSeamTests.cs | lines 64-76 CreateQfcFormController
CITATION: QuickFiler.Test/Controllers/QfcItemController.TestSupport.cs | line 251 StartRunningDispatcher and line 277 ShutdownDispatcher
CITATION: QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixture.cs | line 122 BeginTransactionAsync and line 242 Install
CITATION: QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixture.cs | lines 122-126 TransactionGate.WaitAsync acquisition
CITATION: QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixture.cs | lines 261-276 Dispose is the only ReleaseTransactionGate caller
CITATION: QuickFiler.Test/Controllers/QfcItemController.TestSupport.cs | lines 229-236 discarding the EnsureUiThreadDispatcher scope is permitted and leaks
CITATION: QuickFiler.Test/Controllers/QfcItemController.FocusAndThemeTests.cs | lines 452 and 468 discard the EnsureUiThreadDispatcher scope
CITATION: QuickFiler/Controllers/QfcFormController.EventHandlers.cs | lines 228 and 233 both await UiThread.Dispatcher.InvokeAsync
CITATION: .gitignore | line 84 pattern *.log
CITATION: scripts/vscode/Invoke-Restore.ps1 | line 36 msbuild /t:Restore /p:RestorePackagesConfig=true /m
CITATION: docs/features/active/2026-08-26-qfc-unsynchronized-undo-handoff-after-batch-move-633/spec.md | line 574 Acceptance Criteria heading and line 659 Risks and Mitigations heading
CITATION: docs/features/active/2026-08-26-qfc-unsynchronized-undo-handoff-after-batch-move-633/spec.md | anchor duplicates outside the AC region at 375, 44, 73, 188, 237, 239, 305, 671, 733, 316, 532
CITATION: QuickFiler.Test/QuickFiler.Test.csproj | line 113 Compile Include Controllers FilerQueueTests.cs
CITATION: QuickFiler.Test/QuickFiler.Test.csproj | line 147 Compile Include Controllers QfcFormControllerTests.cs
CITATION: scripts/vscode/Invoke-MSTestWithCoverage.ps1 | line 76 appends /Settings /InIsolation and the LiveOutlook TestCaseFilter
CITATION: scripts/vscode/Invoke-MSTestWithCoverage.ps1 | line 236 throw on non-zero coverage exit before post-processing
CITATION: scripts/vscode/Invoke-MSTestWithCoverage.ps1 | line 343 Set-Content writes the post-processed XML
CITATION: scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1 | lines 417-421 non-allowlisted package removal
CITATION: scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1 | lines 441-447 summary attribute rewrite
CITATION: scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1 | lines 487-490 80 percent line-rate throw
CITATION: global.json | SDK pin 8.0.205 with paths .dotnet-sdk and host
CITATION: dotnet-tools.json | csharpier pinned to 1.2.6 at the repository root
CITATION: .github/workflows/_format-check.yml | line 37 dotnet tool restore
CITATION: docs/features/active/2026-08-26-qfc-unsynchronized-undo-handoff-after-batch-move-633/spec.md | lines 580-657 the twenty acceptance criteria
CITATION: docs/features/active/2026-08-26-qfc-unsynchronized-undo-handoff-after-batch-move-633/research/2026-08-31T19-45-undo-handoff-ordering-research.md | section F two-file production blast radius
CITATION: docs/features/active/2026-08-26-qfc-unsynchronized-undo-handoff-after-batch-move-633/issue.md | line 15 Work Mode full-bug
CITATION: QuickFiler/Controllers/FilerQueue.cs | line 56 var first = item.Helpers.First() inside the catch
CITATION: QuickFiler/Controllers/FilerQueue.cs | lines 70-78 FilerQueueItem constructor accepts an empty helpers list because Any over an empty sequence is false
CITATION: QuickFiler/Controllers/QfcFormController.EventHandlers.cs | line 229 async lambda awaiting WriteMetrics inside the InvokeAsync opened at 228
CITATION: QuickFiler.Test/Controllers/FilerQueueTests.cs | line 23 OneHelper factory returning one MailItemHelper
CITATION: QuickFiler.Test/Controllers/FilerQueueTests.cs | no Timeout attribute anywhere in the 89-line file
CITATION: UtilitiesCS/OutlookObjects/MailItem/MailItemHelper.cs | line 80 parameterless constructor calling InitializeSafeDefaults at line 82
CITATION: UtilitiesCS/OutlookObjects/MailItem/MailItemHelper.cs | lines 167 and 179-182 InitializeSafeDefaults seeds _sentOn _subject and _senderName with string.Empty
CITATION: QuickFiler.Test/Controllers/QfcItemController.SeamFactoryTests.cs | line 4 using System.Reflection needed only by BindingFlags at 214 and 217
CITATION: QuickFiler.Test/Controllers/QfcItemController.SeamFactoryTests.cs | line 18 using UtilitiesCS.Threading needed only by ThreadSafeSingleShotGuard at 216
CITATION: QuickFiler.Test/Controllers/QfcItemController.SeamFactoryTests.cs | lines 210-211 stale single-shot-guard comment falsified by P3-T2 and P3-T3
CITATION: QuickFiler.Test/Controllers/QfcItemController.SeamFactoryTests.cs | lines 343 347 and 421 call QfcItemControllerTestSupport.GetField not Type.GetField
CITATION: QuickFiler.Test/Controllers/QfcItemController.SeamFactoryTests.cs | lines 293 304 and 375 PumpTimeoutMs and two Timeout attributes on other test methods
CITATION: QuickFiler.Test/Controllers/QfcItemController.TestSupport.cs | line 350 InjectFilingCollaborators returns a real FilerQueue carrying the production default processor
CITATION: QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixtureTests.cs | lines 108-110 CSharpier breaks the ConfigureAwait chain onto its own line
CITATION: QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixture.cs | line 25 the only Timeout occurrence is a doc comment and no attribute is present
CITATION: dotnet-tools.json | tools.csharpier.version is 1.2.6 with rollForward false
CITATION: worktree root | no .csharpierrc file of any extension exists so CSharpier's default 100-column print width applies

AC-INVENTORY: AC1, AC2, AC3, AC4, AC5, AC6, AC7, AC8, AC9, AC10, AC11, AC12, AC13, AC14, AC15, AC16, AC17, AC18, AC19, AC20

AC-MAPPING: AC1 | IMPLEMENTATION: P3-T6 | TESTS: P5-T2 | EVIDENCE: FEATURE/evidence/regression-testing/p5-t10-queue-suite.TIMESTAMP.md
AC-MAPPING: AC2 | IMPLEMENTATION: P3-T5 | TESTS: P5-T3, P5-T5 | EVIDENCE: FEATURE/evidence/regression-testing/p5-t10-queue-suite.TIMESTAMP.md
AC-MAPPING: AC3 | IMPLEMENTATION: P3-T5 | TESTS: P5-T4, P5-T5 | EVIDENCE: FEATURE/evidence/regression-testing/p5-t10-queue-suite.TIMESTAMP.md
AC-MAPPING: AC4 | IMPLEMENTATION: P3-T6 | TESTS: P5-T6 | EVIDENCE: FEATURE/evidence/regression-testing/p5-t10-queue-suite.TIMESTAMP.md
AC-MAPPING: AC5 | IMPLEMENTATION: P3-T2, P3-T3, P3-T5 | TESTS: P5-T7 | EVIDENCE: FEATURE/evidence/regression-testing/p5-t10-queue-suite.TIMESTAMP.md and FEATURE/evidence/regression-testing/fail-before-exception.TIMESTAMP.md
AC-MAPPING: AC6 | IMPLEMENTATION: P3-T5 | TESTS: P5-T8 | EVIDENCE: FEATURE/evidence/regression-testing/p5-t10-queue-suite.TIMESTAMP.md
AC-MAPPING: AC7 | IMPLEMENTATION: P4-T2 | TESTS: P2-T3 | EVIDENCE: FEATURE/evidence/regression-testing/fail-before-run.TIMESTAMP.md
AC-MAPPING: AC8 | IMPLEMENTATION: P4-T2 | TESTS: P5-T9 | EVIDENCE: FEATURE/evidence/qa-gates/p7-t8-new-tests-visible.TIMESTAMP.md
AC-MAPPING: AC9 | IMPLEMENTATION: P4-T1 | TESTS: P5-T9 | EVIDENCE: FEATURE/evidence/qa-gates/p7-t8-new-tests-visible.TIMESTAMP.md
AC-MAPPING: AC10 | IMPLEMENTATION: P4-T3 | TESTS: P6-T4 | EVIDENCE: FEATURE/evidence/qa-gates/p6-t4-consumer-read-sweep.TIMESTAMP.md
AC-MAPPING: AC11 | IMPLEMENTATION: P3-T7 | TESTS: P6-T10 | EVIDENCE: FEATURE/evidence/qa-gates/p6-t10-consumer-default.TIMESTAMP.md
AC-MAPPING: AC12 | IMPLEMENTATION: P3-T4 | TESTS: P6-T8 | EVIDENCE: FEATURE/evidence/qa-gates/p6-t8-enqueue-argnull.TIMESTAMP.md
AC-MAPPING: AC13 | IMPLEMENTATION: P3-T8 | TESTS: P6-T9 | EVIDENCE: FEATURE/evidence/qa-gates/p6-t9-seamfactory-reconciled.TIMESTAMP.md
AC-MAPPING: AC14 | IMPLEMENTATION: P2-T3, P5-T9, P3-T8 | TESTS: P6-T1 | EVIDENCE: FEATURE/evidence/qa-gates/p6-t1-determinism-sweep.TIMESTAMP.md
AC-MAPPING: AC15 | IMPLEMENTATION: P1-T3, P4-T4 | TESTS: P6-T2 | EVIDENCE: FEATURE/evidence/qa-gates/p6-t2-net481-language-sweep.TIMESTAMP.md
AC-MAPPING: AC16 | IMPLEMENTATION: P8-T2 | TESTS: P8-T3 | EVIDENCE: FEATURE/evidence/qa-gates/p8-t3-diff-scope.TIMESTAMP.md
AC-MAPPING: AC17 | IMPLEMENTATION: P2-T1 | TESTS: P6-T7, P7-T8 | EVIDENCE: FEATURE/evidence/qa-gates/p7-t8-new-tests-visible.TIMESTAMP.md
AC-MAPPING: AC18 | IMPLEMENTATION: P3-T1 through P4-T3 | TESTS: P6-T5 | EVIDENCE: FEATURE/evidence/qa-gates/p6-t5-production-file-sizes.TIMESTAMP.md
AC-MAPPING: AC19 | IMPLEMENTATION: P7-T2 | TESTS: P7-T3, P7-T4, P7-T5, P7-T6 | EVIDENCE: FEATURE/evidence/qa-gates/p7-t6-test-coverage.TIMESTAMP.md
AC-MAPPING: AC20 | IMPLEMENTATION: P3-T6, P4-T2 | TESTS: P7-T9, P7-T10 | EVIDENCE: FEATURE/evidence/qa-gates/p7-t10-changed-line-coverage.TIMESTAMP.md

UNRESOLVED-GAPS: NONE

DIRECTIVE: PREFLIGHT VALIDATION ONLY
PREFLIGHT: REQUESTED
