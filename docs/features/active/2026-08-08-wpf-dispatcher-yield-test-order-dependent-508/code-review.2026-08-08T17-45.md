# Code Review — issue #508 (wpf-dispatcher-yield-test-order-dependent)

Timestamp: 2026-08-08T17-45
Reviewer: feature-review
Branch: `bug/wpf-dispatcher-yield-test-order-dependent-508`
Range: `003c5715055d7d1933db68a742531332756e30b2..7466096d73ef86f3cc5b9d5da6648cf156c02d6f`
Files reviewed: 2 source files (the complete source diff), plus all 45 feature and evidence documents

## Executive Summary

This is a small, well-targeted change that fixes the stated defect at its root rather than masking
it. The production edit is 37 added and 4 removed lines in one file; the test edit rewrites one
order-dependent test into four tests that arrange their own preconditions.

The central design question is whether an injectable seam genuinely removes the order dependence.
It does, and for both ambient operands, not just one. The pre-change resolution read two pieces of
ambient state — the dispatcher affinitized to whatever pooled thread the test happened to land on,
and the process-global set-once `UiThread.Dispatcher`. The new code reads neither directly; it reads
two injected delegates. In the strict test both delegates return null, so the outcome cannot be
influenced by thread assignment, by test ordering, or by whether `UiThread.Initialize()` ran earlier
in the process. This is the correct fix and it is materially better than the alternative the issue
itself considered and rejected (running the assertion on an owned thread), which would have arranged
only the first operand.

Quality is high across the dimensions that usually degrade in a "make it testable" change: the
public API is preserved exactly, the seam is `internal` rather than public, the defaults reproduce
the pre-change expressions character for character, resolution order is unchanged, and the exception
contract and its message text are byte-identical. The tests assert resolution *order* through
invocation counting rather than merely asserting outcomes, which is a stronger and more durable
specification of the behavior than the test it replaces.

**Blocking findings: 0.** Six advisory items are recorded below. The most substantive is the
unbounded `Thread.Join()` in the test host's teardown, which would convert a hypothetical shutdown
failure into a suite hang rather than a test failure.

## Findings Table

| Severity | File | Location | Finding | Recommendation | Rationale | Evidence |
|---|---|---|---|---|---|---|
| Advisory | `UtilitiesCS.Test/OutlookObjects/Folder/WpfDispatcherYieldTests.cs` | 193-198 (`StaDispatcherHost.Dispose`) | `_thread.Join()` is unbounded and no test carries a `[Timeout]`. If `BeginInvokeShutdown` were ever not processed, the run hangs indefinitely instead of failing. `IsBackground = true` (line 185) prevents the thread from blocking process exit but does not unblock the `Join` itself. | Use `_thread.Join(TimeSpan.FromSeconds(10))` and assert the result, or add a class-level `[Timeout]`. | A test harness should fail loudly, not hang. The fail-before probe used `[Timeout(30000)]` for exactly this reason and it was removed with the rest of the temporary probe. | `WpfDispatcherYieldTests.cs:193-198`; probe rationale at `evidence/regression-testing/fail-before.2026-08-08T16-26.md` |
| Advisory | `UtilitiesCS.Test/OutlookObjects/Folder/WpfDispatcherYieldTests.cs` | 167-199 | `StaDispatcherHost` is the ninth near-identical copy of this helper in the test tree. | Extract to shared test support in a follow-up. Not in this change. | Duplication conflicts with the reusability principle, but the established repo pattern is duplication and consolidating requires a `<Compile Include>` edit to a legacy non-SDK `.csproj`, which the scope boundary forbade. Matching existing style was the correct call here. | `grep -rln "class StaDispatcherHost" --include=*.cs .` returns 9 files; precedent at `FolderTreeSnapshotBuilderYieldTests.cs:118-147` |
| Advisory | `UtilitiesCS/OutlookObjects/Folder/WpfDispatcherYield.cs` | 46 | The default fallback lambda body is the one uncovered line in the class. | Accept. Do not chase it. | Executing it requires a test whose result depends on the process-global `UiThread.Dispatcher` — reintroducing the exact ambient dependence this change exists to remove. The residual is irreducible, not an omission. | `evidence/qa-gates/coverage-changed-lines.2026-08-08T17-06.md`; `UtilitiesCS/Threading/UiThread.cs:135-140` |
| Advisory | `UtilitiesCS/Threading/UiThread.cs` | 135-140 | `public static Dispatcher Dispatcher` is annotated non-nullable but is backed by `private static Dispatcher _dispatcher = null!`. The annotation is inaccurate; the value is null until `Initialize()` runs. | Track as a follow-up on `UiThread`. | The changed code handles this correctly by declaring its local `Dispatcher?` and keeping the null guard, so the annotation lie is contained. It remains a latent trap for future callers who trust the signature. | `UtilitiesCS/Threading/UiThread.cs:135-140`; defensive local at `WpfDispatcherYield.cs:60` |
| Advisory | `docs/.../evidence/qa-gates/coverage-changed-lines.2026-08-08T17-06.md` | source citation | Cites `coverage-postchange.cobertura.xml`, which the artifact substitution removed from the commit. The per-class figure is no longer re-derivable from committed artifacts alone. | When substituting raw reports for summaries, transcribe the per-changed-file line and branch counts inline. | Review should be self-contained from committed evidence. The figure was corroborated arithmetically against package totals, but corroboration is weaker than direct derivation. | Policy audit § 2.3; `evidence/qa-gates/coverage-artifact-substitution.2026-08-08T17-30.md` |
| Advisory | `artifacts/pr_context.summary.txt` | overview and header | Recorded a stale head and classified both `.cs` files as documentation, reporting `Core logic changes: 0 files`. | Regenerated during this review. Report the generator defect upstream. | The coverage hook derives its changed-language set from these bullets, so the misclassification would have caused it to skip enforcement entirely for this branch. | Policy audit § Review-Time Corrections |

No Blocking or Major findings.

## Production Change Review — `UtilitiesCS/OutlookObjects/Folder/WpfDispatcherYield.cs`

### Does the fix address the root cause?

Yes, and for both operands. This was verified by reading the resolution path rather than by trusting
the description.

Pre-change (`git show 003c5715:...`, lines 27-28):

```csharp
Dispatcher dispatcher =
    Dispatcher.FromThread(Thread.CurrentThread) ?? UtilitiesCS.UiThread.Dispatcher;
```

Post-change (lines 60-61):

```csharp
Dispatcher? dispatcher =
    _currentThreadDispatcherProvider() ?? _fallbackDispatcherProvider();
```

Both fields are assigned once in the constructor (lines 42-46). When a test supplies both delegates
through the `internal` constructor, neither default lambda is ever assigned, so
`Dispatcher.FromThread` and `UiThread.Dispatcher` are not merely bypassed at runtime — they are not
in the object graph at all. `YieldAsync_WithoutDispatcher_RemainsStrict` supplies
`CountingDispatcherProvider(null)` for both, so the strict path is reached by construction.

This is the distinction that matters. The issue text (`issue.md:69-72`) explicitly considered and
rejected the alternative of running the assertion on a dedicated owned thread, on the grounds that
it arranges only the first operand and leaves the process-global fallback unarranged. The
implemented shape is the one the issue reasoned toward, and it does not have that weakness.

### Behavior preservation

| Aspect | Assessment |
|---|---|
| Resolution order | Unchanged. `??` remains, in the same position, thread-affinitized first. |
| Short-circuiting | Unchanged. `??` short-circuits post-change exactly as it did pre-change, so `UiThread.Dispatcher` is still read only when the thread lookup returns null. Wrapping the operands in lambdas does not change when they are evaluated. |
| Evaluation thread | Unchanged. The default lambda calls `Thread.CurrentThread` when invoked inside `YieldAsync`, not at construction, so it still observes the calling thread. |
| Exception type and message | Byte-identical (lines 62-67). |
| Cancellation guard placement | Unchanged; still the first statement (line 51), before any lookup. |
| Public signature | Unchanged. The explicit `public WpfDispatcherYield()` restores the implicit constructor that adding any constructor would otherwise remove. |
| Allocation | Two delegate allocations per instance instead of zero. The lambdas capture nothing, so Roslyn caches the delegate instances statically. Negligible, and `WpfDispatcherYield` is constructed twice in the entire repository. |

### Design assessment

The seam shape is the right weight for the problem. Two `Func<Dispatcher?>` fields express exactly
what needs to vary and nothing more. An `IDispatcherResolver` interface with an implementation and a
test double would have been three more types for two call sites, and a DI-container registration
would have been worse. The choice matches the general policy's "simplicity first" ordering, and it
matches the repo's own guidance preferring a narrow `Func<>` for a single call path.

Keeping the `??` and the null guard *inside* the class, rather than externalizing the whole
resolution, is the important detail. It means the tests still verify the production ordering rather
than verifying a test-only reimplementation of it. A seam that had accepted a single
`Func<Dispatcher?>` "resolved dispatcher" would have moved the ordering logic out of the class under
test and made the ordering assertions vacuous.

`Dispatcher` to `Dispatcher?` on line 60 is a correctness improvement, not merely a compiler
appeasement. The pre-change local was annotated non-nullable while the code immediately tested it
for null on the next line — an internally inconsistent annotation. The new annotation matches the
actual contract.

The XML documentation (lines 17-36) is appropriate: it explains *why* the seam exists ("Tests use
this to arrange the dispatcher-free case explicitly instead of inheriting it from ambient thread and
process state") rather than restating the signature, and it documents the null-selects-production
convention for each parameter. The pre-existing rationale comment at lines 53-59 is preserved
verbatim.

### `[ExcludeFromCodeCoverage]` removal

Correct, and required rather than optional.

`.claude/rules/general-unit-test.md` states that no production file may be excluded from coverage
measurement and that the correct response to untestable lines is to refactor for testability. The
attribute was defensible only while the class was genuinely unreachable in a test host. This change
makes it reachable, so retaining the attribute would have been an exclusion for a file that is now
testable — the precise thing the policy prohibits.

`issue.md:83-84` anticipated this and required that the attribute "be reconsidered rather than left
in place by inertia". The removal was carried out honestly: it grew the measured denominator by 38
lines rather than quietly keeping them out of it, and the repo-wide rate still moved upward because
45 lines became covered. Removing the attribute while adding an equivalent `coverage.config`
exclusion would have been the failure mode here, and no such entry appears anywhere in the diff.

The unused `using System.Diagnostics.CodeAnalysis;` was removed with it, which is the correct
cleanup.

## Test Change Review — `UtilitiesCS.Test/OutlookObjects/Folder/WpfDispatcherYieldTests.cs`

### Coverage of behavior

Four tests replace one. The mapping to the resolution graph is complete:

| Test | Line | Behavior pinned |
|---|---|---|
| `YieldAsync_CanceledToken_ThrowsBeforeDispatcherYield` | 16 | The cancellation guard runs *before* either lookup — asserted by both counts being 0, not merely by the exception type |
| `YieldAsync_ThreadAffinitizedDispatcherPresent_YieldsWithoutFallback` | 53 | Branch 1; fallback count asserted 0, proving short-circuit |
| `YieldAsync_ThreadDispatcherAbsent_FallsBackToProcessGlobalDispatcher` | 85 | Branch 2; both counts asserted 1, proving order |
| `YieldAsync_WithoutDispatcher_RemainsStrict` | 118 | Branch 3, the strict contract; both counts asserted 1 |

The first test is a genuine addition beyond what the issue asked for, and a valuable one. Asserting
that an already-canceled token short-circuits *before* any dispatcher resolution is a real behavioral
contract that nothing previously pinned.

### Order assertions rather than outcome assertions

`CountingDispatcherProvider` (lines 148-165) is the strongest part of this test file. Asserting
invocation counts turns statements about resolution *order* into mechanically checked facts. A future
refactor that reversed the operands, or that eagerly evaluated both, would fail these tests even
though the observable outcome in most cases would be unchanged. This is a durable specification and
it is exactly what `issue.md:62-63` asked for when it required that the ordering stay verified.

Each count assertion carries a `because` string explaining the contract, so a failure reports the
violated invariant rather than a bare integer mismatch.

### Determinism

Verified rather than assumed:

- The two null-provider tests (lines 16 and 118) touch no ambient state whatsoever and are
  deterministic by construction.
- The two dispatcher-present tests own their dispatcher through `StaDispatcherHost`. The constructor
  blocks on `_ready.WaitOne()` until the thread has published its dispatcher, so there is no
  start-up race. `Dispatcher.Run()` means background-priority operations genuinely complete rather
  than waiting on a pump that never runs.
- `CountingDispatcherProvider._invocationCount` is a plain `int`, which was checked for a visibility
  hazard and does not have one. Both delegate invocations occur in the synchronous prologue of
  `YieldAsync` (line 61 of the production file), before the first suspension point at line 69, so
  they execute on the awaiting test's own thread; the subsequent `await` establishes the
  happens-before edge for the assertion read. No `Interlocked` or `volatile` is needed.
- No sleep, no retry, no wall-clock dependency, no `[DoNotParallelize]`.

Empirically corroborated by three consecutive full parallel runs with identical counts and per-test
duration variance of 12/21/33 ms on one test — scheduling demonstrably differed between runs while
outcomes did not, which is the property the fix claims.

### Does `StaDispatcherHost` create a visible window?

No. This was checked specifically because issue #511 tracks a related WinForms pump-host defect
whose symptom is a visible window.

The mechanism is different in the way that matters. This host runs
`System.Windows.Threading.Dispatcher.Run()`, which starts a message loop and nothing else. It is not
`System.Windows.Forms.Application.Run(form)`, and no `Window`, `Form`, or `Control` is constructed
anywhere in the file. A WPF dispatcher with no visual attached has nothing to display.

The default fallback lambda is also safe on this axis: `UiThread.Dispatcher`
(`UtilitiesCS/Threading/UiThread.cs:135-140`) is a plain static property over a backing field with no
initialization side effect. It does not call `Init()`. The sibling members `UiThread.UiSyncContext`
and `UiThread.AutoScaleFactor` do call `Init()` and would show a form, and the seam deliberately
touches neither.

### Test hygiene

`#nullable enable` on line 1 is appropriate and brings the test file in line with its peers and with
the production file. Every `StaDispatcherHost` use site is wrapped in `using`, and the awaited
assertion completes before the block exits in each case, so the dispatcher is alive for the duration
of the operation it serves. `Dispatcher` is exposed as `Dispatcher { get; private set; } = null!`,
which is honest: the constructor guarantees it is non-null by the time any caller can observe it,
and the `null!` is confined to the private setter's initial value.

## Verification Commands Used

```
git diff <base>..HEAD -- UtilitiesCS/OutlookObjects/Folder/WpfDispatcherYield.cs
git show 003c5715:UtilitiesCS/OutlookObjects/Folder/WpfDispatcherYield.cs
grep -rn "new WpfDispatcherYield" --include=*.cs .
grep -rln "class StaDispatcherHost" --include=*.cs .
grep -rn "InternalsVisibleTo" UtilitiesCS/Properties/AssemblyInfo.cs
git diff <base>..HEAD -- '*.cs' | grep -nE "DoNotParallelize|\[Ignore|Thread\.Sleep|Task\.Delay|GetTempPath|GetTempFileName|Retry|retry"
csharpier.exe check <2 changed files>            # exit 0, 0 unformatted
MSBuild.exe UtilitiesCS/UtilitiesCS.csproj -t:Rebuild -p:Nullable=enable -p:TreatWarningsAsErrors=true
awk 'END{print NR}' <each changed file>          # 77 and 201, both under the 500-line limit
```

## Verdict

**Approved.** No blocking or major findings. The change is minimal, addresses the root cause for
both ambient operands, preserves the public API and runtime behavior exactly, and improves the
specification of the behavior it touches. The six advisory items are follow-up candidates and none
should hold the merge.
