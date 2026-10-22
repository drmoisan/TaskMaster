# Code Review — Issue #816, UiThread IsCompleted branch-2 residual and AC5 apartment measurement

- Date: 2026-09-14
- Reviewer: feature-review agent
- Branch: `bug/uithread-iscompleted-branch2-residual-and-ac5-apartment-measurement-816`
- Head reviewed: `b4941e25229497b18316023794fbc4a2dfe83159`
- Base: `main` @ `b63eaa4630d13da46f7ece130bedade53ac39e22` (recomputed merge base, identical to `origin/main`)
- Review scope: the full branch diff against the resolved base, not the plan-anchored subset

## Executive Summary

This is a small, well-argued change. One production predicate gains two conjuncts; three tests and one
invariance twin are added; one project-file item is registered. The production diff is two removed
lines and fifteen added lines in a single file, and I reproduced it rather than taking it on report.

The change is correct for the leg it targets. Pre-change, the captured-UI-context exit returned `true`
on any thread whose managed id happened to equal the captured UI thread id and whose awaited context
was reference-equal to the captured context — a state the CLR can produce by recycling a managed thread
id after a thread dies. Post-change it additionally requires a non-null captured dispatcher that is
reference-equal to the dispatcher of the executing thread, which is a second, independent proof of
thread identity rather than a restatement of the first. The `_dispatcher is not null` term is not
redundant: without it the two-null case would match and the exit would return `true` on exactly the
shape the guard exists to reject. The delivery proves that with a dedicated test rather than asserting
it, which is the right call.

Test quality is high. The two negative tests are red-then-green with the red run recorded at exit code
1 and the intended failure message; the positive twin is green in both states and is a genuine
invariance control rather than a tautology. The twin also correctly defeats the trap that
`Dispatcher.Invoke` installs a throwaway `DispatcherSynchronizationContext` on the UI thread for the
duration of an operation: it captures the ambient context, substitutes a fresh plain one, asserts, and
restores in a `finally`. Without that substitution the test would have exercised the ambient-identity
exit at the top of the accessor and proved nothing about the exit under change.

One substantive finding is recorded. The sibling dispatcher exit, two lines below the one this delivery
hardens, still contains the identical null-to-null hazard that the new code's own comment calls
load-bearing. It is pre-existing, it is unchanged by this delivery, and AC1 makes touching it a FAIL
condition — so it is correctly out of this item's reach. But it is not recorded in the spec's Non-Goals
and it is not tracked anywhere, and after this change the asymmetry between the two adjacent exits is
sharper than it was before. The recommendation is to promote it to a follow-up issue, not to widen this
one.

No finding in this review is Blocking. Two Low findings concern file cohesion and remaining
file-size headroom.

## Findings Table

| Severity | File | Location | Finding | Recommendation | Rationale | Evidence |
|---|---|---|---|---|---|---|
| Medium | `UtilitiesCS/Threading/UiThread.cs` | lines 197-201, the final `return` of `SynchronizationContextAwaiter.IsCompleted` | The sibling dispatcher exit reads `return _context is DispatcherSynchronizationContext && ReferenceEquals(Dispatcher.FromThread(Thread.CurrentThread), _dispatcher);`. When `_dispatcher` is null and the executing thread owns no dispatcher, that `ReferenceEquals` compares null to null and evaluates true, so the exit returns `true` on a thread that is not the UI thread — the identical defect shape the exit above it was just hardened against. Reachable when `Initialize()` throws between the `UiThreadId` assignment at line 81 and the `Dispatcher` assignment at line 82, leaving `_uiThreadId` set and `_dispatcher` null, and the awaited context is a `DispatcherSynchronizationContext` belonging to another thread. | Do not change it in this item. Promote a follow-up issue that adds the same `_dispatcher is not null` term to this exit, with a negative regression test mirroring `IsCompleted_WhenNoUiDispatcherWasCapturedAndTheExecutingThreadHasNone_ReturnsFalse`. | Pre-existing and explicitly fenced off: AC1 declares a diff that alters the dispatcher exit a FAIL condition, and the spec's Non-Goals reserve the dispatcher leg. Widening the change here would break the item's own acceptance criteria. But the residual is real, it is not recorded in the Non-Goals (which discuss the dispatcher leg only in its fully-initialized form), and this delivery's own seven-line comment states the reasoning that condemns it. | Post-change source at `UtilitiesCS/Threading/UiThread.cs:197-201`, read directly; the partial-initialization window is visible at `UtilitiesCS/Threading/UiThread.cs:79-82`; the reasoning is stated in the added comment at lines 175-181. |
| Low | `UtilitiesCS.Test/Threading/UiThreadApartmentMeasurement_Tests.cs` | whole file | The file declares two test classes, `UiThreadPredicateHardening_Tests` (lines 23-97) and `UiThreadApartmentMeasurement_Tests` (lines 113-162). The file name names only the second. A reader searching for the predicate-hardening regression tests by file name will not find them, and the General Code Change Policy's module-cohesion rule asks that a file have one clear purpose. | If the file is revisited, split the predicate-hardening class into `UiThreadPredicateHardening_Tests.cs` and register it in the project file. No change is warranted for this item alone. | The placement is spec-directed, not accidental: AC2 and AC3 name this file explicitly, and `evidence/qa-gates/p4-t13-ac13-file-sizes.md` records that the alternative host, `UiThread_Tests.cs`, had only 42 lines of headroom before the change and has 7 after. The constraint that produced the grouping is documented. | `UtilitiesCS.Test/Threading/UiThreadApartmentMeasurement_Tests.cs:21-23` and `:111-113`; `spec.md` AC2 and AC13; file line count 164, verified directly. |
| Low | `UtilitiesCS.Test/Threading/UiThread_Tests.cs` | end of file, line 493 | The file now sits 7 lines below the repository's 500-line ceiling. The next test method added to `SynchronizationContextAwaiter_Tests` will breach it, and the breach will surface as a policy failure rather than as a design signal. | Before the next addition to this file, split it along its existing class boundaries (it already declares more than one nested `StaDispatcherHost` helper at lines 354 and 464, which suggests a natural seam). | The limit is a hard rule in `.claude/rules/general-code-change.md` with no exception for test files, and the 7-line margin makes the next change a forced refactor at an inconvenient time. | Line count 493 verified directly in this review; `evidence/qa-gates/p4-t13-ac13-file-sizes.md` records the same value and names 7 as the tightest headroom in the set. |
| Low | `UtilitiesCS.Test/Threading/UiThreadInitContract_Tests.cs` | lines 79-128, 135-163 | `ApartmentThreadRunner` and `SharedStaDispatcherHost` are shared internal helpers consumed by at least three test files, but they live in a file whose name advertises a specific contract suite. `SharedStaDispatcherHost` also blocks on `_ready.WaitOne()` with no timeout in its constructor, so a failure to reach `Dispatcher.CurrentDispatcher` on the owned thread would hang the run rather than fail it. | Move both helpers to `UtilitiesCS.Test/TestHelpers/`, next to the existing `UiThreadStateScope`, when either is next touched. Consider a bounded `WaitOne(TimeSpan)` with an explicit failure, so a broken host fails the test instead of stalling it. | Both are pre-existing and neither was introduced or modified by this delivery; the new file merely consumes them. Recorded so the coupling is visible, not as a defect of this item. | `UtilitiesCS.Test/Threading/UiThreadInitContract_Tests.cs:79-128` and `:135-163`; consumers at `UiThreadApartmentMeasurement_Tests.cs:35,43,79,129` and within the declaring file at `:188,215,278,282,380`. |
| Informational | `UtilitiesCS/Threading/UiThread.cs` | lines 182-189 | The added condition spells the dispatcher type fully as `System.Windows.Threading.Dispatcher.FromThread(...)` even though `using System.Windows.Threading;` is present at line 11. | Keep it. | The simple name `Dispatcher` inside the nested struct resolves to the enclosing type's static property of the same name, so the qualification is required rather than stylistic. The pre-existing sibling exit at lines 197-201 already uses the same spelling, and the comment at lines 193-196 explains why. The new code matches the surrounding style, as the policy requires. | `UtilitiesCS/Threading/UiThread.cs:11`, `:185-188`, `:193-201`, `:238` (`Dispatcher` static property region). |
| Informational | `UtilitiesCS.Test/Threading/UiThreadApartmentMeasurement_Tests.cs` | lines 40, 76 | Both negative tests initialize `bool observed = true;` — the value that fails the assertion — before handing the delegate to the thread runner. | Keep it. | This makes a delegate that never ran indistinguishable from a delegate that returned `true`, so the test cannot pass vacuously if thread creation or apartment assignment silently failed. It is a deliberate strengthening, reinforced by the separate `thrown.Should().BeNull()` assertion. | `UtilitiesCS.Test/Threading/UiThreadApartmentMeasurement_Tests.cs:40,57-58` and `:76,93-94`. |
| Informational | `UtilitiesCS.Test/Threading/UiThread_Tests.cs` | lines 277-309 | The positive twin captures `SynchronizationContext.Current` inside `host.Dispatcher.Invoke`, substitutes a fresh plain `SynchronizationContext`, and restores the captured one in a `finally`. | Keep it, and treat it as the reference pattern for any future dispatcher-thread predicate test. | Without the substitution the ambient context during the assertion would be the throwaway `DispatcherSynchronizationContext` that WPF installs for the duration of a dispatcher operation. If that context were also the awaiter's `_context` the test would exit at the ambient-identity check at the top of the accessor and would prove nothing about the exit under change. The substitution forces control flow past the thread-id guard and into the exit this item modifies. | `UtilitiesCS.Test/Threading/UiThread_Tests.cs:288-302`; the corresponding accessor path at `UtilitiesCS/Threading/UiThread.cs:159-192`. |
| Informational | `UtilitiesCS.Test/UtilitiesCS.Test.csproj` | line 516 | The new `<Compile Include="Threading\UiThreadApartmentMeasurement_Tests.cs" />` item is placed between the `UiThreadInitContract_Tests.cs` and `WpfUiDispatcherTests.cs` items, which is alphabetically out of order relative to `UiThread_Tests.cs` above it. | Keep it. | AC12 specifies that exact insertion point, and the surrounding item group is not alphabetically ordered anyway (`CurrentStoreContextTests.cs` follows `WpfUiDispatcherTests.cs`). Matching the local convention is what the policy asks for. | `UtilitiesCS.Test/UtilitiesCS.Test.csproj:513-519`. |

## Design and Correctness Notes

**The predicate is a strict narrowing.** I walked all five exits of the post-change accessor against
the pre-change source. Exits 1 through 3 and exit 5 are byte-identical. Exit 4 returns `true` on a
proper subset of the states it previously did, because the added terms are conjuncts and no existing
term was relaxed. There is therefore no state in which the accessor now returns `true` where it
previously returned `false`, which is the property that makes the "does not change behaviour on the
out-of-scope leg" claim structurally sound rather than merely tested.

**The out-of-scope leg is genuinely unaffected.** When a caller stands on the captured UI thread inside
a WPF dispatcher operation, `Dispatcher.FromThread(Thread.CurrentThread)` returns the captured
dispatcher, so both added conjuncts evaluate true and exit 4 behaves as before. The spec states this
and the positive twin measures it; the structural argument and the test agree.

**Exception handling.** `ApartmentThreadRunner.RunOnThread` catches broad `Exception` inside the worker
delegate. That would normally be a policy concern, but here the catch immediately captures and returns
the exception to the caller, which asserts on it — it propagates with the thread boundary crossed
rather than swallowing. The helper is pre-existing and unmodified.

**Determinism.** No sleep, delay, stopwatch, wall-clock read, tick count or polling loop appears in any
new test body. Every thread created is a background thread, is given an explicit apartment before
start, and is joined before the creating method returns. The dispatcher host is shut down through
`BeginInvokeShutdown` followed by `Join`. The three recorded repetitions produced byte-identical
totals.

**Process-global state.** Both new test classes carry `[DoNotParallelize]` and every mutation of a
`UiThread` static is inside a `using (UiThreadStateScope.Enter())`. The assembly carries a
`Parallelize` attribute, so the per-class attribute is load-bearing rather than decorative. The
measurement class carries the attribute even though it writes no `UiThread` static, which is
conservative and harmless.

**Assertion strength.** The delivery strengthens rather than weakens. `failing.Should().Throw<InvalidOperationException>()`
gains `.WithMessage(FakeUiCaptureSource.CaptureFailureMessage)`, and the same test gains
`Thread.CurrentThread.GetApartmentState().Should().Be(ApartmentState.STA)` as its first Arrange step.
I verified independently that the two `InvalidOperationException` sources reachable from `UiThread.Init()`
carry different message text — the non-STA rejection is built from `NonStaInitMessagePrefix`, which
begins "UiThread.Init() must be called on the UI (STA) thread...", and the fake's message is
"FakeUiCaptureSource was configured to fail during CaptureUiVariables()." — so the constraint
discriminates and the pairing of the two edits closes a real blind spot rather than adding ceremony.

## Confidentiality and Artifact Hygiene

I searched every file in both touched feature folders and all nine changed `.claude/agent-memory/`
records for `C:\Users`, `C:/Users`, the account token and `TaskMaster-wt`. Zero matches. The coverage
evidence artifacts deliberately write `<repo-root>` in place of the Cobertura `filename` attributes and
say so. The mirror-parity artifact substitutes `<816 feature folder>` and `<809 feature folder>` into
quoted git warnings. No raw TRX or Cobertura document is committed. This is the cleanest hygiene result
I have seen on this branch family.

## Verdict

**PASS.** No Blocking finding. One Medium finding recommended for promotion to a follow-up issue
rather than remediation within this item, three Low findings that are recorded for the next time the
affected files are touched, and four informational notes, three of which record correct decisions that
would be easy to undo by accident.
