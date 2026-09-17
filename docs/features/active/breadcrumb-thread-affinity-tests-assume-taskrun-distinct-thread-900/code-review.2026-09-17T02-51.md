# Code Review — Issue #900: breadcrumb thread-affinity tests assume Task.Run yields a distinct thread

- Timestamp: 2026-09-17T02-51
- Branch: `bug/breadcrumb-thread-affinity-tests-assume-taskrun-distinct-thread-900` @ `3538c3617edb1c0e26186bec2a2e3aa26171b7d1`
- Base: `origin/main` @ `66b65a4626095ade5a01643aee4a43c90cc58cbf` (ancestor of head; two-dot and three-dot diffs coincide by construction)
- Source scope: `QuickFiler.Test/Viewers/ItemViewerBreadcrumbThreadAffinityTests.cs` (+101/-30; 419 -> 490 lines). No other source, project, package, workflow, or settings file changed.
- Companion artifacts: `policy-audit.2026-09-17T02-51.md`, `feature-audit.2026-09-17T02-51.md`

## Executive Summary

Verdict: **PASS — ready to merge**. Blocking: **0**. Non-blocking: **3 Low, 4 Info**.

The change does what the item exists to do. The scheduling property is now controlled by construction rather than tolerated: the worker is a `Thread` the test creates, which cannot be object-identical to the `Thread` captured by `Dispatcher.CurrentDispatcher` in the viewer constructor, and the delegate asserts that exact predicate (`UiDispatcher.CheckAccess()` is false) before calling the guarded member. Nothing on the branch serialises, pins, retries, sleeps, times out, or widens a tolerance; `scripts/vscode/TaskMaster.cli.runsettings` is byte-identical to `origin/main` and still carries `Workers 0` and `Scope ClassLevel`.

The non-vacuity proof is real. Both mutation runs named their failing assertion in advance (plan D-3 and D-4), the observed failure messages match those predictions on their literal content, and each revert is proven by an anchored `git diff --exit-code HEAD`, an empty scoped porcelain, and SHA-256 equality with the fix commit. The reviewer re-derived the two hashes independently (head file and CRLF-normalized base blob) and confirmed the mutation tokens never entered history.

The `Thread.Join()` safety argument holds on its merits, with one wording caveat recorded as Info.

## Review Emphases (caller priority order)

### 1. Controlled, not tolerated

- `RunOnDedicatedWorkerThread` (lines 385-403) constructs `new Thread(...)`, sets `IsBackground = true`, `Start()`s, `Join()`s with no timeout, and returns the captured exception. There is no `Task.Run`, `LongRunning` hint, retry loop, `Thread.Sleep`, `Task.Delay`, `SpinWait`, `Stopwatch`, `[Timeout]`, or `[DoNotParallelize]` anywhere in the diff (reviewer grep of the added lines; executor census 0 for each at P1-T1, P2-T1, P5-T7).
- The in-delegate precondition (lines 230-236 and 279-285) reads `scope.Viewer.UiDispatcher.CheckAccess()`, which is the same `Dispatcher` instance and the same predicate the production guard uses (`ThrowIfOffUiBoundary`, `ItemViewer.Breadcrumb.cs:432-447`: `Dispatcher owning = UiDispatcher; ... if (!owning.CheckAccess()) throw ...`). The test therefore proves the boundary is crossed using the guard's own definition of the boundary, not a proxy such as `ManagedThreadId`.
- `git diff origin/main..HEAD -- scripts/vscode/TaskMaster.cli.runsettings` is empty (0 bytes); the file reads `<Workers>0</Workers>` and `<Scope>ClassLevel</Scope>`. Every executor run and the reviewer's re-run passed `/Settings:` with that file and recorded the same SHA-256 (`98EF03A8D3B0EBB2ED7A765E3B5E1B58E774D20202DF2F294C03A7260B9CEF57`).
- Verdict: controlled. PASS.

### 2. Non-vacuity proof

| Mutation | Predicted failing assertion (plan) | Observed message (verbatim from the artifact) | Match |
| --- | --- | --- | --- |
| M1 (P3-T1): `ClearViewerDispatcher(scope.Viewer);` inserted after the precondition and before the guarded call in both delegates | D-3: both fail on `captured.Message.Should().Contain(...)` with the `CaptureCurrent()` text, which proves `NotBeNull` and `BeOfType<InvalidOperationException>` passed first | `Expected captured.Message "Breadcrumb UI components must be constructed on an owning UI synchronization context." to contain "InitializeBreadcrumbPipeline".` and the `ConfigureBreadcrumbDropDown` twin | Yes. The subject `captured.Message` and the phrase `to contain` identify the `Contain` assertion; the quoted text is the exact string thrown by `BreadcrumbUiDispatcher.CaptureCurrent()` (`BreadcrumbUiDispatcher.cs:44-51`, read directly: `throw new InvalidOperationException("Breadcrumb UI components must be constructed on an owning UI synchronization context.")`), which is exactly `InvalidOperationException`, so `BeOfType` necessarily passed before it. |
| M2 (P3-T3): `action();` inserted as the first statement of the helper, before the thread is created | D-4: both fail at the precondition with `vacuously` in the message | `Expected isOwnerThread to be False because the dedicated worker thread must not be the thread that constructed the viewer, or the boundary assertion would pass vacuously, but found True.` (both tests) | Yes. The failure is the `BeFalse` on `isOwnerThread`, thrown on the calling thread outside the helper's `try`/`catch`, which is the refactor-hazard the precondition was added to catch. |

Reverts: P3-T2 and P3-T4 each record census token counts back to 1, `git diff --exit-code HEAD -- <file>` exit 0, empty scoped porcelain, and SHA-256 `8EBC19F829536957BEB0DAEC628929CA8DE3F11E10AA819DEA41362C6FCBB164`. The reviewer confirmed: the head file hashes to that value; `git diff 63142ec73..HEAD -- <file>` is empty (the file has not changed since the fix commit); `git log -S"ClearViewerDispatcher(scope.Viewer);" origin/main..HEAD -- <file>` returns no commit, so neither mutation was ever committed. The pass-after run P4-T2 loaded a freshly rebuilt assembly (P4-T1 `DLL_ADVANCED: True`), so it measured the reverted source and not the M2 binary.

Verdict: the proof is real and each observed failure matches its prediction on the literal message. PASS.

### 3. `Thread.Join()` under the class-level parallel run

The remark's claim is that the waiting thread and the waited-for thread are never both thread-pool workers. Assessed:

- The waiting thread is the MSTest worker, which under 4.4.0 is a `Task.Run` pool thread (`DefaultFactoryAsync`; neither test carries `[Timeout]`). The waited-for thread is a dedicated `Thread`. So the claim as stated is true.
- The property that matters is that the wait cannot form a cycle: the dedicated thread's progress does not depend on the pool. Its delegate calls `CheckAccess()` (no scheduling) and then the guarded member, whose first statement throws (`ItemViewer.Breadcrumb.cs:51` and `:233`, read directly), or in the M1 case reaches `CaptureCurrent()` and throws. No pool work item, dispatcher pump, or `Control.Invoke` marshalling is on that path, so the join completes regardless of how many pool workers are parked. This is precisely what the original `GetAwaiter().GetResult()` shape lacked: a pool worker waiting on a work item that itself needed a pool worker.
- What the wait does do is hold one pool worker for the duration of a sub-millisecond call, the same cost as any synchronous test body. The helper remark says the wait "cannot starve the pool", which is broader than what the argument shows (no circular starvation). Recorded as Info (finding 5); the conclusion that the join is safe under `Workers=0`/`ClassLevel` stands.
- Apartment state: `new Thread` defaults to MTA, the same as the previous `Task.Run` worker; the guard throws before any control access, so no STA requirement is introduced.

Verdict: the argument holds. PASS.

### 4. AC traceability

All eight boxes in `spec.md` are checked and each is supported by evidence the reviewer read and, where possible, re-derived (see `feature-audit.2026-09-17T02-51.md`). No box is checked without support. AC1-AC8: PASS.

### 5. Evidence completeness and hygiene

- Fields: every command-step artifact carries `Timestamp:`, `Command`, `EXIT_CODE:` and `CHANNEL:`; 35 of 40 evidence files carry `## Output Summary`. Five command-step artifacts carry no `Output Summary` field or heading (finding 3, Low).
- Filename stamps equal `Timestamp:` fields in all 39 stamped artifacts. One seconds-scale skew between a stamp and the containing commit (finding 4, Info).
- Host identifiers: 0 drive-letter `Users` paths, 0 account-name hits, 0 machine-name hits across the added lines of all 52 diff paths (including the five agent-memory files and the four artifacts written after the executor's own P5-T13 sweep). The one absolute path the toolchain emitted (the iteration-1 `TaskMaster.sln` IOException) was recorded as `<repo-root>\TaskMaster.sln`.
- Evidence locations: all 40 artifacts under `<FEATURE>/evidence/<kind>/`; 0 files under `artifacts/baselines|baseline|qa|evidence|coverage/`.

Verdict: complete; two hygiene deviations, both non-blocking.

## Findings Table

| Severity | File | Location | Finding | Recommendation | Rationale | Evidence |
| --- | --- | --- | --- | --- | --- | --- |
| Low | `QuickFiler.Test/Viewers/ItemViewerBreadcrumbThreadAffinityTests.cs` | whole file | The file is 490 lines against the 500-line limit (419 at base); the next helper or test added to this class will breach it. | In a future item, move the file-local support types (`InertDropDownHost`, `DrainableSynchronizationContext`, `ViewerScope`) or the new helper into an `internal` test-support file under `QuickFiler.Test/`, reconciling with the class remark (lines 21-28) that currently prefers private duplicates. No change required on this branch. | Policy limit is 500 lines for test code; 10 lines of headroom is a maintenance hazard rather than a violation. | `wc -l` = 490; P5-T7 `LINES = 490`. |
| Low | `QuickFiler.Test/Viewers/ItemViewerBreadcrumbThreadAffinityTests.cs` | lines 241-249, 294-302, 385-403 | If the precondition `BeFalse` ever fails on the dedicated thread, the resulting `AssertFailedException` is captured by the helper and reported by `captured.Should().BeOfType<InvalidOperationException>()` as a bare type mismatch; the `BeFalse` reason string is not surfaced. Only the inline-refactor path (M2) surfaces the reason, because there the assertion throws outside the `try`. | Optional: pass the captured exception into the exact-type assertion's reason, e.g. `BeOfType<InvalidOperationException>("the captured exception was {0}", captured)`, or have the helper rethrow `AssertFailedException` via `ExceptionDispatchInfo`. | On the current tree the precondition cannot fail on a genuinely new thread, so this is a diagnostic-quality point, not a correctness point. | Diff read directly; M2 message shape in P3-T3. |
| Low | `evidence/regression-testing/p2-t1-token-census-after-edit.2026-09-17T02-21.md`; `evidence/qa-gates/p5-t8-toolchain-loop-closure.2026-09-17T02-37.md`; `evidence/qa-gates/p5-t9-scope-boundary.2026-09-17T02-38.md`; `evidence/other/p5-t15-closure.2026-09-17T02-40.md`; `evidence/other/p5-t5-iteration-1-environmental-failure.2026-09-17T02-32.md` | headings | Five command-step artifacts carry no `Output Summary` field or heading; the observations sit under `## Census`, `## Reconciliation ...`, `## The four path lists ...`, `## Commit 1`, `## What happened`. | Future executors should emit the literal `## Output Summary` heading (or `Output Summary:` field) in every command-step artifact, as the plan's own conventions section requires. | The required observations are present, so this is a labelling deviation; it is recorded because the caller's hygiene rule names the field explicitly. | `grep -c -i 'Output Summary'` = 0 in each of the five files. |
| Info | `evidence/other/p5-t15-closure.2026-09-17T02-40.md` | filename and `Timestamp:` | The stamp `02-40` postdates the committer time of the commit that contains the artifact (`3538c361`, amended 02:39:57 per reflog; first commit 02:39:12, first amend 02:39:41). | None required. Read `Timestamp:` values as minute-granularity authoring stamps, not as a proof of sequence. | A seconds-scale skew; no evidence claim depends on the ordering of this artifact relative to the commit. | `git reflog --date=iso`; `git log -1 --format='%ai | %ci'`. |
| Info | `QuickFiler.Test/Viewers/ItemViewerBreadcrumbThreadAffinityTests.cs` | lines 211-214, 264-266, 381-383 | The remarks say the untimed join "adds no starvation risk" / "cannot starve the pool". The argument actually establishes the absence of a circular wait (the waited-for thread never needs a pool worker); the wait still parks one pool worker for the call's duration, as any synchronous test body does. | Optional wording tightening in a future edit: "cannot form a pool-on-pool wait" rather than "cannot starve the pool". | Accuracy of documentation; the safety conclusion is unaffected. | Emphasis 3 above. |
| Info | `QuickFiler.Test/Viewers/ItemViewerBreadcrumbThreadAffinityTests.cs` | lines 249, 302 | `captured.Should().NotBeOfType<ObjectDisposedException>()` is redundant after the exact-type `BeOfType<InvalidOperationException>()`. | Keep; it documents AC3's explicit exclusion (plan D-2) and costs nothing. | Documented design choice. | Diff; FluentAssertions 8.10.0 `BeOfType` semantics. |
| Info | `QuickFiler.Test/Viewers/ItemViewerBreadcrumbThreadAffinityTests.cs` | lines 390-397 | The helper's `catch (Exception error)` is a broad catch. | None; it is a marshalling seam whose captured value is asserted immediately by the caller, so nothing is swallowed. | Consistent with the in-repo `ApartmentThreadRunner` precedent. | Diff read directly. |

## Best-Practice Checklist

- Naming, XML docs, AAA markers, reason strings: present.
- No new `using` directive needed; `System.Threading.Tasks` remains in use by `InertDropDownHost` and the unchanged third test.
- No nullable directive in the file (unchanged); no CS86xx diagnostics introduced; nullable rebuild clean.
- CSharpier canonical on first write (file hash unchanged across scoped and repository-wide format runs).
- Explicit types at the helper boundary; `var` only where the initializer makes the type obvious.
- No public surface change; helper is `private static`.
- Repository style matched: the helper mirrors `UtilitiesCS.Test/Threading/UiThreadInitContract_Tests.cs` `ApartmentThreadRunner`.
- Existing tests treated as spec: the five sibling tests are byte-identical (hunk audit in P2-T1 shows every hunk within pre-edit lines 199-265 plus one pure insertion at 333).

## Follow-ups

Recorded for the coordinator, who holds the promotion tools. None is filed here and none widens this branch.

1. **`breadcrumb-dispatcher-tests-assume-taskrun-distinct-thread`** — `QuickFiler.Test/Viewers/BreadcrumbPopupBoundaryCoverageTests.cs:58` (`Task dispatch = Task.Run(() => dispatcher.Dispatch(() => executions++));`, blocking wait; same exposure as the two tests fixed here) and `QuickFiler.Test/Viewers/BreadcrumbUiThreadDispatchTests.cs:301` (`await Task.Run(...)`; not wait-inlined, exposed only to idle-thread reuse). Both verified present at head. The third `Task.Run(` in the subject file (line 332, `InitializeBreadcrumbPipeline_NullOwningDispatcher_DoesNotThrow`) is deliberately unchanged and out of scope; its `NotThrow()` assertion is not defeated by inlining.
2. **`breadcrumb-null-owner-test-discrimination-remark-holds-only-when-stolen`** — remark at lines 311-317 of the head file (`the pre-fix guard reads the non-null captured context and rejects the worker-thread call`) states unconditionally a property that holds only in the stolen branch. Documentation-accuracy defect; no pass/fail effect.
3. **`analyzer-hintpath-versions-lag-packages-config-breaking-cold-worktrees`** — 15 `<Analyzer Include>` lines across 15 `.csproj` files on `origin/main` pin `Meziantou.Analyzer.3.0.203` (reviewer count via `git grep`), while `packages.config` and 33 other references say `3.0.235`. This branch changes no `.csproj` or `packages.config`. Two framings need reconciling before filing: (a) the executor observed a real build failure on this cold worktree (`P0-T7`: `MSBUILD_EXIT_CODE: 1`, `2 Error(s)`, `CSC : error CS0006: Metadata file '..\packages\Meziantou.Analyzer.3.0.203\...\Meziantou.Analyzer.dll' could not be found`, `CSC_OUT_LINES: 0`) and repaired the environment by materialising the missing package into git-ignored `packages/`; (b) the caller's note that CI runs cold checkouts and is green, so the defect is a silently non-loading analyzer rather than a broken build. The reviewer checked `.github/workflows/_build-analyzers.yml:35-58`: CI caches `packages/` under `actions/cache@v4` with a bare-prefix `restore-keys: nuget-<os>-` fallback, so a CI runner inherits earlier version folders and is not cold; the workflow comment's own premise that orphaned folders are "inert" does not hold when a stale HintPath still points at one. CI being green therefore does not establish that the missing analyzer loads silently; the direct observation is that a cold restore fails with CS0006. Whoever files this should verify with a cache-miss run, and should first check whether an existing issue (the reviewer's notes indicate #898 was opened for the same skew on 2026-09-17) already tracks it, to avoid a duplicate.
4. **`fileinfowrapper-openread-test-depends-on-a-real-repository-file`** — `UtilitiesCS.Test/HelperClasses/FileInfoWrapper_Tests.OpenRead_ShouldReturnReadableStreamForWrappedFile` opens the repository's own `TaskMaster.sln` and failed once on this run (P5-T5 iteration 1) because a resident MSBuild `/nodeReuse:true` worker from the plan's own rebuilds held the file. Remedy is an injectable seam or sentinel stream, not a temporary file (policy prohibits temp files). `DirectoryInfoWrapper_Tests.cs:60,79` assert on the same file name and share the exposure.
5. **Test file headroom** (finding 1) — 490/500 lines; consider extracting file-local support types before the next addition to this class.

## Verdict

PASS. 0 blocking, 3 Low, 4 Info. Ready to merge; no remediation cycle required.
