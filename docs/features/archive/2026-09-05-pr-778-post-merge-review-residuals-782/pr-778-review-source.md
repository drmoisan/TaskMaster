## Suggested title

fix(threading): guard UiThread.Dispatcher against a null dispatcher (#584)

## Summary

- `UiThread.Dispatcher` now throws a named `InvalidOperationException` when its backing field has not been captured, instead of silently returning `null` and leaving a downstream consumer to fail later with an unattributed `NullReferenceException`.
- The `null!` null-forgiving suppression is removed and the backing field is redeclared as `Dispatcher?`, so the nullable analyser verifies the guard rather than being suppressed around it.
- Two deterministic regression tests cover the guarded and the populated paths, with no sleeps, retries, or timing tolerances.
- `[DoNotParallelize]` is applied to the three `UtilitiesCS.Test` classes that reflectively write the process-global static, removing the class-level concurrency their `finally` restore cannot address.
- One reflective consumer, `QuickFiler.Test/Helper Classes/EmailMoveMonitorTests.cs`, is retargeted from the public property to the private backing field so its setup and teardown observe the same state without invoking the new guard.

## Why

`UtilitiesCS.Threading.UiThread.Dispatcher` was a static property backed by a `null!`-initialised field with no lazy initialisation and no guard. `ProgressTrackerAsync.InitializeAsync()` assigns the property's value at line 33 and dereferences it at line 35. When the static was read before `UiThread.Initialize()` completed, the property returned `null` and the consumer threw a `NullReferenceException` that named neither the missing initialisation nor the responsible component.

The failure was non-deterministic and order-dependent: it was observed once during a full-suite MSTest run under `[assembly: Parallelize(Workers = 0, Scope = ExecutionScope.ClassLevel)]`, and did not reproduce in isolation or in two subsequent clean full-suite runs. Making the accessor fail fast converts an intermittent, unattributed crash into a self-diagnosing exception raised at the point of misuse. The fix mirrors the `InvalidOperationException` contract already established for the same hazard in `UtilitiesCS/OutlookObjects/Folder/WpfDispatcherYield.cs`.

## What Changed

Six source files, verified against the merge base with `git diff --name-status 87cb4df3..HEAD`.

**Core logic (1 file)**

- `UtilitiesCS/Threading/UiThread.cs` — the `Dispatcher` getter gains a null guard that throws `InvalidOperationException` naming the required `UiThread.Init()` call; `private static Dispatcher _dispatcher = null!;` becomes `private static Dispatcher? _dispatcher;`. The property's public type remains non-nullable `Dispatcher`, so callers keep receiving a guaranteed non-null value.

**Tests (5 files)**

- `UtilitiesCS.Test/Threading/UiThread_Tests.cs` — two regression tests plus a `DispatcherField()` reflection helper (+75 lines).
- `UtilitiesCS.Test/Threading/IdleAsyncQueue_Tests.cs`, `ProgressTrackerAsync_Tests.cs`, `ProgressTracker_Tests.cs` — `[DoNotParallelize]` added; attribute-only, one line each.
- `QuickFiler.Test/Helper Classes/EmailMoveMonitorTests.cs` — the `[TestInitialize]`/`[TestCleanup]` reflective snapshot moves from `GetProperty("Dispatcher", Public|Static)` to `GetField("_dispatcher", NonPublic|Static)`. No assertion, test method, or mock setup is added, removed, or altered; the class keeps all 8 `[TestMethod]` members.

**Docs and evidence**

The feature folder `docs/features/active/uithread-dispatcher-null-race-progresstrackerasync-584/` carries the issue, spec, research, atomic plan, three audit artifacts, and 30-plus evidence artifacts recording every gate.

## Architecture / How It Fits Together

`UiThread` holds process-global static UI-thread state. `UiThread.Init()` runs `Initialize()`, which captures the WPF `Dispatcher` into `_dispatcher`. Consumers read the static `UiThread.Dispatcher` property; `ProgressTrackerAsync.InitializeAsync()` is the consumer that exposed the defect.

The change is confined to the accessor's contract. Control flow is unchanged on the initialised path: a captured dispatcher is returned exactly as before. Only the uninitialised path changes, from a silent `null` return to an immediate throw at the read site.

Because the getter can now throw, any reflective read through the **property** surfaces the exception via `PropertyInfo.GetValue`. A repository-wide census (plan task `P0-T14`) enumerated every such route across all `.cs` files: the qualified expression `UiThread.Dispatcher`, the reflective property name `"Dispatcher"`, and the reflective field name `"_dispatcher"`. Exactly one reflective property consumer existed, and it is the sixth file changed here. No production file reads the dispatcher reflectively.

## Verification

**Completed** (recorded under the feature folder's `evidence/` tree)

- Format: `dotnet tool run csharpier format .` scoped to the six owned paths, then `dotnet tool run csharpier check .` — `Checked 1576 files`, exit 0.
- Analyzers: `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` — exit 0, `0 Warning(s)`, `0 Error(s)`.
- Nullable: `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true` — exit 0, `0 Error(s)`.
- `UtilitiesCS.Test`: 4787 of 4787 passing, 0 failed, 0 skipped, against a 4785 baseline (+2 for the new regression tests).
- `QuickFiler.Test`: 1312 of 1312 passing, 0 failed, 0 skipped. All eight `EmailMoveMonitorTests` methods are named as passing; a recorded first pass had all eight failing before the sixth-file retarget, and that fail-before artifact is preserved.
- Changed-line coverage: 100% — 8 of 8 coverable added lines, each with 1 or more hits.
- Repository line-rate moved from 0.7073317 to 0.7073604, with a `lines-valid` delta of +42.
- Feature review: policy audit COMPLIANT with documented exceptions, code review APPROVE, feature audit ACCEPT, 7 of 7 acceptance criteria PASS, **0 blocking findings**.

**Recommended**

- Re-run the full four-step C# toolchain on the merge result.
- Confirm CI green on the branch head before merge.

## Backward Compatibility / Migration Notes

This is a behavioural change to a public API surface. `UiThread.Dispatcher` previously returned `null` when uninitialised and now throws `InvalidOperationException`.

The census described above establishes that no production consumer depends on the silent-null outcome; every production read either follows initialisation or is a documentation cross-reference. The one test consumer that did depend on it is updated in this change. No public type signature changes: the property's declared type remains non-nullable `Dispatcher`.

Callers that previously relied on a null return to detect uninitialised state must now call `UiThread.Init()` first, which is the intended contract and is named in the exception message.

## Risks and Mitigations

- **A consumer outside the census depends on the silent null.** The census covered the qualified expression, the reflective property name, and the reflective field name across every `.cs` file, and the review additionally checked the `using static UtilitiesCS.UiThread` route, which has zero hits. Rollback is a one-file revert with no data or migration considerations.
- **The coverage figures sit below the repository floor.** Both the baseline and the post-change figures are below it, and the shortfall is pre-existing rather than introduced here. These are raw unstripped `dotnet-coverage` figures for the whole `UtilitiesCS.Test` host process, which is a different denominator from the first-party testable one the policy governs; they are not comparable to the policy percentage. This change moves the figure up and achieves 100% changed-line coverage.
- **`[DoNotParallelize]` reduces test parallelism.** It applies to four classes that write one process-global static. The measured cost is immaterial against a 4787-test assembly, and the alternative is retaining a known order-dependent flake.

## Review Guide

1. `UtilitiesCS/Threading/UiThread.cs` — the entire behavioural change is here; it is small and self-contained.
2. `UtilitiesCS.Test/Threading/UiThread_Tests.cs` — confirm the regression tests are deterministic.
3. `QuickFiler.Test/Helper Classes/EmailMoveMonitorTests.cs` — the reflection retarget; confirm no assertion changed.
4. The three `[DoNotParallelize]` additions are one line each and need little scrutiny.
5. `spec.md` and `plan.2026-09-02T09-02.md` are large but are planning records, not shipped behaviour.

## Follow-ups

These are non-blocking findings from the feature review. They are deferred rather than promoted on this branch: the plan's footprint acceptance criterion asserts the branch diff lists only the six owned source paths plus the feature folder, and adding a potential-entry document would falsify evidence already committed. They should be filed as a consolidated issue after this PR is open.

- `UtilitiesCS.Test/Threading/ProgressTracker_Tests.cs` is 514 lines, over the 500-line limit. This is pre-existing at the merge base and the branch delta for that file is zero lines; `[TestClass, DoNotParallelize]` was used specifically to avoid adding one. A partial-class split is the natural remedy.
- `ProgressTrackerAsync_Tests.cs` still mutates the reflective static directly; the spec records syncing it to the shared helper idiom as a follow-up.
- `UiThread_Tests.cs` omits a `field.Should().NotBeNull()` guard that its sibling test carries.
- `EmailMoveMonitorTests.cs` uses `DispatcherField?.GetValue(null)`; if the field were ever renamed, both sides would be null and the cleanup assertion would pass vacuously. This null-conditional pattern is retained from the pre-change code.
- `UiThread.cs` retains a `_uiSyncContext!` suppression, an untouched instance of the same pattern this change removes for `_dispatcher`.

## GitHub Auto-close

- None (GitHub validation unavailable)

This pull request addresses issue #584. The automatic auto-close bullet is withheld because the PR-context bundle reports `GitHub CLI unavailable` and lists no verified closing issue, and the skill's reference rules forbid emitting a closing directive from unverified state. The bundle's author-asserted list additionally contained #449, #493 and #508, which appear in this branch's documents only as historical references — #449 as the run during which the defect was first observed — and must not be closed by this pull request.

🤖 Generated with [Claude Code](https://claude.com/claude-code)

https://claude.ai/code/session_01TzGiZSnVySFZcoC1BHN5Vv

---

## Post-merge code review (three-phase, 2026-09-05)

Review target: merge commit `1c3b210c`, diff `HEAD~1..HEAD` restricted to the six `.cs` files above, plus the 45 documentation and evidence files under the feature folder.

Method. Phase 1 ran ten independent finder angles (line-by-line scan, removed-behavior audit, cross-file tracer, C# pitfall specialist, wrapper/proxy correctness, simplification, reuse, efficiency, altitude, and CLAUDE.md conventions). Phase 2 deduplicated the ten candidate lists into 26 distinct claims and ran one adversarial verifier per claim, each instructed to attempt refutation first and to anchor its verdict to quoted source. Phase 3 ran four gap sweeps (residual code pass, blast-radius enumeration of 96 references across all assemblies, documentation and evidence consistency, and build/nullable/test-configuration), producing 12 further candidates; the two rated should-fix were independently verified and three others were spot-checked directly.

Result. No blocking finding. No functional regression introduced by this PR was confirmed. The accessor change does what it set out to do, and the regression test fails before the fix and passes after it.

| Outcome | Count |
|---|---|
| Blocking | 0 |
| Should-fix | 7 |
| Nit | 25 |
| Refuted | 6 |

Verdict meanings: CONFIRMED = claim factually true and consequence real for this PR; PLAUSIBLE = claim true but consequence latent or uncertain; REFUTED = claim false, pre-existing and unaffected, or immaterial.

### Should-fix (7)

**C10 — CONFIRMED.** `UtilitiesCS.Test/Threading/UiThread_Tests.cs:166`. `Dispatcher_WhenBackingFieldIsPopulated_ReturnsThatSameInstance` calls `Dispatcher.CurrentDispatcher` inside a plain `[TestMethod]`, on a pooled MTA MSTest worker thread, and never shuts the dispatcher down. `UtilitiesCS.Test/test.runsettings` documents that STA is opt-in via `[STATestMethod]`, and every other `CurrentDispatcher` use in the test projects runs under `[STATestMethod]`, `[STATestClass]`, or a dedicated STA thread with `BeginInvokeShutdown`. The leaked, never-pumped dispatcher stays affinitized to a reused thread; a later test on that thread that resolves `Dispatcher.FromThread(Thread.CurrentThread)` (production default in `WpfDispatcherYield.cs:44`; test helper in `FilterOlFoldersControllerRefreshDisposalTests.cs:257-264` awaited without timeout) would hang. Latent today because the class is `[DoNotParallelize]` and recorded runs were per-assembly with `/InIsolation`. Fix: obtain the sentinel on a dedicated STA thread (pattern at `ProgressTrackerAsync_Tests.cs:130` or the `StaDispatcherHost` in `WpfUiDispatcherTests.cs:161-207`) and shut it down in `finally`. Keep the test rather than deleting it, so the populated branch stays covered in the regression file.

**C02 — PLAUSIBLE.** `UtilitiesCS/Threading/UiThread.cs:138-146`. The getter reads the non-volatile static `_dispatcher` twice: once for the null check and once for the return. A null write landing between the two loads returns null despite the guard. Null-writers remain in the repository: `IdleAsyncQueue_Tests.ForceDispatcherNull`, `finally` restores of a null prior in the tracker tests, and QuickFiler.Test's ungated `EnsureScope.Dispose` (`CompareExchange` to null) under class-level parallelization. Within UtilitiesCS.Test all writers are now serialized, so the window is latent, and the failure mode is identical to pre-PR behavior. Fix: `Dispatcher? dispatcher = _dispatcher; if (dispatcher is null) throw ...; return dispatcher;` (or `Volatile.Read`), matching `OutlookFolderTreeService.cs:336` and `WebView2BreadcrumbHost.cs:159`.

**C18 — CONFIRMED.** `QuickFiler.Test/Helper Classes/EmailMoveMonitorTests.cs:39-65`. `DispatcherField?.GetValue(null)` with a null-conditional on a `static readonly FieldInfo` means a rename of `_dispatcher` makes both snapshots null, and FluentAssertions 8.10 `BeSameAs` on a null subject with null expected passes, so the order-independence guard the class exists to enforce degrades to a no-op. The PR increased exposure by retargeting from a public property (rename is a solution-wide compile break) to a private field name with no compile-time coupling. The same assembly's `QfcItemController.UiThreadDispatcherFixture.cs:38-48` exposes `internal static Dispatcher Current`, a lock-guarded read of the same field whose `ResolveDispatcherField` asserts the field exists. Fix: replace the local `FieldInfo` and both `?.` reads with `UiThreadDispatcherFixture.Current`, and remove the two "avoid WindowsBase" comment fragments at lines 29 and 53 (see C25).

**C19 — CONFIRMED.** `UtilitiesCS.Test/Threading/IdleAsyncQueue_Tests.cs:236-240, 266-267, 272`. The P27-T2 docstring, the Act comment, and the `NotThrow` reason string still say a `NullReferenceException` from `InvokeAsync` on a null Dispatcher is caught "after the await". After this PR the exception is `InvalidOperationException`, thrown synchronously by the getter at `IdleAsyncQueue.cs:72` inside the `try` and before the first await, and swallowed only because the catch at line 83 is `catch (Exception)`. The test still passes; the documented mechanism is wrong. The PR edited this file (`[DoNotParallelize]`) without updating the text. Fix: rewrite the three passages to describe the synchronous `InvalidOperationException` path.

**C20 — CONFIRMED.** `UtilitiesCS/OutlookObjects/Folder/WpfDispatcherYield.cs:57-66`. The comment "UiThread.Dispatcher ... is null outside a live host, so that null state is surfaced as InvalidOperationException" is now false: the production fallback provider `() => UtilitiesCS.UiThread.Dispatcher` (line 46) throws itself. The local `dispatcher is null` guard (lines 62-66) is unreachable on the production path and is retained only because the providers are typed `Func<Dispatcher?>` under `#nullable enable`. The same precondition now emits two different messages depending on path; production always emits UiThread's message, never "...before yielding folder tree work." The plan (line 1294) acknowledged the divergence and accepted it. Fix: rewrite the comment to state that the fallback provider throws and that the guard covers injected providers; route both throws through one shared message constant; optionally add `.WithMessage("*UiThread.Init()*")` to `YieldAsync_WithoutDispatcher_RemainsStrict`.

**C16 — CONFIRMED, pre-existing.** `UtilitiesCS.Test/Threading/ProgressTracker_Tests.cs` is 514 lines at both `HEAD~1` and `HEAD`. `.claude/rules/general-code-change.md:49` and `CLAUDE.md:106` set a 500-line limit with no pre-existing or baseline exemption; a grep of `.claude/rules` and `.claude/skills` finds no "baseline + 1" file-size clause. That clause exists only in `plan.2026-09-02T09-02.md:941`. A plan-local acceptance clause cannot waive a CLAUDE.md rule under the Policy Compliance Order. The PR's own policy audit (line 109) discloses this as PARTIAL, so it is disclosed debt rather than a regression. Fix: split the file in a follow-up, and do not describe the `p2-t3` "baseline + 1" pass as rule compliance in review artifacts.

**S3-2 — CONFIRMED.** `policy-audit.2026-09-04T04-05.md:123, 229, 421` and `feature-audit.2026-09-04T04-05.md:149` report that `dotnet tool run csharpier format .` was executed. `evidence/qa-gates/p4-t1-format.md:8` records a six-path scoped invocation, prescribed by plan P4-T1 (lines 1068-1084) to avoid repo-wide drift rewrites. The executor followed the plan; the misstatement is in the audits' transcription, and the deviation from the CLAUDE.md approved-command list is absent from the audit's section 8 "Gaps and Exceptions". Substantively equivalent: `p4-t2` ran whole-tree `check .` with exit 0 and an empty reported set. Fix: correct the command cells, amend row 3.1, and add a section 8 entry citing the plan rationale and the `check .` mitigation. Documentation only.

### Nits (25)

Code and tests:

- **C03 — PLAUSIBLE, pre-existing.** `UiThread.cs:36`. `Init()` sets the one-shot `ThreadSafeSingleShotGuard` before `Initialize()` runs and has no catch or reset. If `Initialize()` throws after the guard flips, `_dispatcher` stays null permanently and the new message's remedy ("Call UiThread.Init()") is a no-op. In production an `Initialize()` failure aborts `ThisAddIn_Startup` before any consumer runs. Consider setting the latch after line 61 succeeds, and wording the message to not promise a retry re-runs initialization.
- **C05 — CONFIRMED, pre-existing split.** `UiThread.cs:137`. `UiSyncContext` and `AutoScaleFactor` lazily call `Init()` on null; `Dispatcher` throws. Read order now decides whether a caller self-heals or faults. The divergence predates the PR (Dispatcher was already the only non-lazy accessor) and is logged as CR-7 in the PR's code review, but no in-code comment explains why lazy `Init()` from an arbitrary reader is deliberately avoided here (`Initialize()` shows a hidden WinForms window and must run on the UI thread). Add a two-line comment above the throw.
- **C06 — CONFIRMED.** `UiThread.cs:142` and `UiThread_Tests.cs:152`. The message and the regression test both name the private method `UiThread.Initialize()`. The plan mandated the exact string. The sibling message at `WpfDispatcherYield.cs:65` names only the public `Init()`. Follow-up: shorten to "Call UiThread.Init() before reading UiThread.Dispatcher." and assert `*UiThread.Init()*`.
- **C08 — CONFIRMED.** `UiThread.cs:135`. The public static property gains a throwing precondition and carries no XML doc (`CLAUDE.md` C#6.2 and C#3.3). No member of `UiThread.cs` is documented today, and the policy says "should", so this is a nit. Add `<summary>`, `<remarks>` noting the deliberate non-lazy contract, and `<exception cref="InvalidOperationException">`.
- **C09 — CONFIRMED, pre-existing gap.** `UiThread.cs:142`. The message omits that `Init()` must run on the UI (STA) thread during startup. Neither `Init()` nor `Initialize()` checks apartment state; `SyncContextForm.CaptureUiVariables` captures `Dispatcher.CurrentDispatcher` on whatever thread calls it, so a worker-thread `Init()` succeeds silently and installs a non-pumping dispatcher into set-once globals (`QfcHomeControllerRunAsyncTests.cs:329` already calls `UiThread.Init(false)` from a test thread). Append the thread requirement to the message; open a follow-up for `Init()` to reject non-STA callers.
- **C11 — CONFIRMED.** `UiThread_Tests.cs:135-167`. Test 1 asserts `field.Should().NotBeNull()`; test 2 calls `field.GetValue(null)` unguarded, so a renamed field fails test 2 with a bare NRE in Arrange. Test 1 uses a block-bodied lambda where 396 of 407 throw-assertion lambdas in UtilitiesCS.Test are expression-bodied. Move the null guard into `DispatcherField()`; use `Action act = () => _ = UiThread.Dispatcher;`.
- **C12 — CONFIRMED.** `UiThread_Tests.cs:125`. `DispatcherField()` is the sixth reflection site for `UiThread._dispatcher` (also `IdleAsyncQueue_Tests.cs:144`, `ProgressTracker_Tests.cs:421`, `ProgressTrackerAsync_Tests.cs:138`, `EmailMoveMonitorTests.cs:40`, `UiThreadDispatcherFixture.cs:135`), each handling a missing field differently. `UtilitiesCS/Properties/AssemblyInfo.cs:19` grants `InternalsVisibleTo("UtilitiesCS.Test")`, so an internal test seam on `UiThread` could replace reflection for the four UtilitiesCS.Test sites. Follow-up; a bugfix PR is the wrong vehicle.
- **C13 — CONFIRMED.** `UiThread_Tests.cs:139-176`. Both new tests hand-roll capture, `SetValue`, `try`/`finally` restore. `IdleAsyncQueue_Tests` has `ForceDispatcherNull`/`RestoreDispatcher` but they are private and cannot install a non-null value. Same remedy as C12: one `IDisposable` install scope under `UtilitiesCS.Test/TestHelpers/`.
- **C14 — PLAUSIBLE.** `UiThread_Tests.cs:167`. While test 2 holds a never-pumped MTA dispatcher in the static, background work left alive by parallel-phase tests can read it. Concretely, `IdleActionQueue_Tests` has no cleanup and leaves no-op entries queued with a live `ApplicationIdleTimer` heartbeat subscription; a heartbeat in the microsecond window would enqueue a `DispatcherOperation` that never runs. No test-visible effect. Optional hygiene: add a `TestCleanup` to `IdleActionQueue_Tests` that drains entries and unsubscribes.
- **C15 — CONFIRMED.** `ProgressTracker_Tests.cs:14`. `[TestClass, DoNotParallelize]` is the only comma-combined attribute list among 41 `DoNotParallelize` usages in the repository; chosen to avoid growing a 514-line file. Split when the file is next touched, paired with the C16 split.
- **C17 — CONFIRMED.** `ProgressTracker_Tests.cs:14`, `ProgressTrackerAsync_Tests.cs:14`, `IdleAsyncQueue_Tests.cs:29`. Exactly one method per class touches `UiThread._dispatcher` (readers included), so class-level `[DoNotParallelize]` moves 32 non-touching tests into the serial bucket where method-level placement on the three writer methods would give the same guarantee. Defensible per plan rationale (grep-verifiable per-file invariant) and repo precedent (all 18 pre-existing usages are class-level); runtime cost is negligible.
- **C21 — CONFIRMED, pre-existing.** `WpfDispatcherYieldTests.cs:118`. No test constructs `new WpfDispatcherYield()` and reaches the production fallback provider; the concurrency test marshals onto an STA host first. The PR's research (`defect-scoping.md:147-162`) scoped this out. Follow-up: one `[DoNotParallelize]` test that nulls `_dispatcher`, calls `YieldAsync` from a thread with no dispatcher, and asserts `InvalidOperationException` with `*UiThread.Init*`.
- **C25 — CONFIRMED, pre-existing text.** `EmailMoveMonitorTests.cs:29, 53`. The comment justifies reflection by "avoiding a compile-time WindowsBase dependency". `QuickFiler.Test.csproj:460` references WindowsBase directly and ten sibling files import `System.Windows.Threading`. The PR appended an accurate paragraph (lines 32-37) beneath the false premise without correcting it. Delete the two clauses.
- **C26 — PLAUSIBLE.** `ProgressTrackerAsync_Tests.cs`. No test drives `InitializeAsync()` or `ProgressTracker.Initialize()` with a null dispatcher; AC3's consumer-level conversion is verified by code reading only (`p3-t4-progresstrackerasync-unmodified.md:58-71`). The accessor-level test was a documented scoping decision and demonstrably fails before and passes after the fix. Optional: add `InitializeAsync_WhenDispatcherNotCaptured_ThrowsInvalidOperationException`.
- **S2-1 — CONFIRMED.** `QuickFiler.Test/Controllers/QfcItemController.InitializationTests.Part2.cs:121-125`. The Arrange comment says an unset static "cannot complete an InvokeAsync"; the unset case now throws `InvalidOperationException` synchronously before any `InvokeAsync`. Comment-only.
- **S4-1 — CONFIRMED.** `.claude/agent-memory/task-researcher/project_qfc_collection_defects_468.md:41-42` and `project_filerqueue_consumer_unsound_633.md:3` state that `UiThread.Dispatcher` is "permanently null in tests" and "NREs". Stale after this PR; a future planning session loading these notes would reason from the wrong exception type. Update the notes (push-down surface owned by drm-copilot).
- **S4-2.** `evidence/qa-gates/p4-t5-utilitiescs-tests.md:7`, `p4-t6-quickfiler-tests.md`. The local toolchain step 4 ran only `UtilitiesCS.Test.dll` and `QuickFiler.Test.dll`; `TaskMaster.Test` (host of the ribbon and startup consumers) and the other five test assemblies were not run locally. CI's `mstest-coverage` job discovers every `*.Test.dll` and concluded success on the PR head, so no regression is indicated. Evidence-scope gap only.

Documentation and evidence:

- **S3-1 — CONFIRMED, downgraded to nit on verification.** `evidence/regression-testing/p1-t4-expect-fail.md:3,48` (Timestamp 08-31, "P1-T3 recorded a clean build immediately before this run") vs `p1-t3-build-before-fix.md:3` (08-33); `evidence/qa-gates/p3-t1-analyzer-build.md:3,30-31` (08-38, "the first build that compiles ... the production fix") vs `p3-t2-regression-green.md:3` (08-34) and `p3-t3-at-risk-tests.md:34` (TRX mtime 08:35:42). Every artifact with a hard marker has a Timestamp matching it to the minute, so the most likely reading is that P3-T1 ran after P3-T2..T5 and an unrecorded build produced the assembly they executed against. The fail-before/pass-after proof does not depend on the ordering prose: P1-T4's recorded output ("no exception was thrown", 1 failed) and P3-T2's output stand on their own, and Phase 4 pass 2 independently confirms the final state. Neither the skill nor the plan defines what instant `Timestamp:` denotes. Fix: soften the ordering sentences in the two artifacts and in `feature-audit.md:38` / `policy-audit.md:115`; define `Timestamp:` semantics in `evidence-and-timestamp-conventions`.
- **S3-3 — CONFIRMED.** `policy-audit.2026-09-04T04-05.md:68` states "All 34 evidence artifacts"; `git ls-tree` shows 38 at the audit commit and at HEAD.
- **S3-4.** `evidence/issue-updates/issue-584.2026-09-02T09-02.md`. Filename timestamp is the plan's timestamp; the artifact's own `Timestamp:` is `2026-09-03T22-24`. A second update to #584 would collide or mis-order.
- **S3-5.** `evidence/baseline/p0-t6-mcp-probe.md:12` (`EXIT_CODE: non-zero (...)`), `p1-t5-donotparallelize.md:11-13`, `p3-t5-no-timing-tokens.md:12-16`. `EXIT_CODE:` is not a single integer as the evidence schema requires.
- **S3-6.** `spec.md:7` Status remains "Draft" with all seven ACs checked and the PR merged; "In scope" (lines 62-70) lists three files, "Files/modules to change" (160-163) lists two, the Write Set (92-99) lists six.
- **S3-7.** `spec.md:50, 172` say "~40 other call sites"; `spec.md:73-74` says "~62 remaining direct reads across ~29 files"; a grep at the research base yields about 49 live reads in 26 production files.
- **S3-8.** `feature-audit.md:117, 119`, `code-review.md:22, 191`, `policy-audit.md:111`, `p2-t3-file-size.md:42` use evaluative wording ("honest and correct", "the right call", "Exemplary", "a model instance", "comfortably inside") that `.claude/rules/tonality.md` classifies as non-neutral.
- **S3-9.** `code-review.md:85` and `policy-audit.md:329-330` recommend promoting the ProgressTrackerAsync_Tests synchronization follow-up to a GitHub issue before merge. The PR merged and the feature folder holds no record that this happened. Not verified against GitHub from the review environment.

### Refuted (6)

- **C01.** `TaskMaster/Ribbon/RibbonViewer.EngineCommands.cs:72, 115`. The `dispatcher != null` checks are now dead code (true), but the claimed regression (loss of a degrade-to-direct-call fallback) does not hold: every production caller of `InvalidateEngineCommands`/`InvalidateEngineToggle` runs after `Application_Startup`, which requires `ThisAddIn_Startup` to have already run `UiThread.Init()` synchronously (`ThisAddIn.cs:35-42`), and the IdleAsyncQueue refresh path dereferenced the accessor without a guard before this PR. Any hypothetical exception is caught by `IdleAsyncQueue`'s catch, `HandleToggleClickAsync`'s click boundary, or `CompletePrime`'s continuation. Optional cleanup: remove the redundant null comparisons.
- **C04.** `UiThread.cs:36`. `CheckAndSetFirstCall` is non-blocking, so a concurrent second `Init()` caller returns before `Initialize()` completes (true), but the mechanism is pre-existing, untouched by this PR, and unreachable under the production startup ordering; the PR's getter neither widens nor narrows the window.
- **C07.** `UiThread.cs:137`. Collapsing the getter to `get => _dispatcher ?? throw ...` is legal but the premise is false: the adjacent `UiSyncContext` and `AutoScaleFactor` getters use the same block form, the `.editorconfig` preference is `silent`, and CSharpier would wrap the long message literal anyway.
- **C22.** `UtilitiesCS/Threading/ProgressTrackerPane.cs:13, 16`. The double read exists, but pre-PR a null static already failed at line 13 (NRE on `.Invoke`); the setter is private and set-once, so no production path can swap the static between reads; no test reaches the constructor. Exception type change only.
- **C23.** `ProgressTrackerAsync.cs:39`, `ProgressTracker.cs:39`. The inner re-read inside the `InvokeAsync` lambda exists, but issue #584's recorded NRE was at line 35, from the outer read at line 33; the inner read was never reached. Production never mutates `_dispatcher` after `Initialize()`, and every reflective writer is now serialized with the lambda drained by `PushFrame` before any restore. Optional tidy-up: pass the captured `UiDispatcher` into the lambda.
- **C24.** `UtilitiesCS/Threading/WpfUiDispatcher.cs:25`. Pre-PR the same members threw NRE at the same call sites before `Init()`; the PR changes only the exception type. The `StoreLockupResponder` path cannot execute before `Init()` because `ThreadMonitor` is constructed inside `Initialize()` after the dispatcher is assigned. `IUiDispatcher.cs` was not touched.

### Verification notes

- Verifiers checked the diff at `1c3b210c`, the full touched files, callers and callees, the feature folder's issue, spec, research, plan, and evidence artifacts, `.claude/rules`, `.claude/skills`, `CLAUDE.md`, `.editorconfig`, the test runsettings and `AssemblyInfo` parallelization attributes, and the CI workflow definitions.
- The review environment (Linux) could not run `msbuild`, `vstest.console.exe`, or `dotnet`; all conclusions are from static reading of source, configuration, and recorded evidence. Toolchain results were taken from the committed evidence artifacts and the CI check runs on the PR head, all of which concluded success.
- Nothing in this review was applied to the code. The seven should-fix items are candidates for a consolidated follow-up issue alongside the "Follow-ups" section above, which already anticipates C16, C11, and C18.
