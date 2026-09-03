# Code Review — issue #731 (quickfiler-controller-lifecycle-disposal-defects)

- Timestamp: 2026-09-03T15-35
- Branch: `bug/quickfiler-controller-lifecycle-disposal-defects-731` @ `c55bfad2`
- Diff base: `35583f7c7e1f1c9b97e4f6f1e7846a3f2693c17e`
- Scope: full branch diff — 6 production C# files, 5 test C# files, 1 project file, 30 documents,
  12 `.claude/agent-memory/` files. No caller-supplied narrowing was applied.

## Verdict

**No Blocking findings.** Nine advisory findings follow, ordered by severity. The change is
well-engineered: the design rejects the issue's first-listed option for each of findings 1 and 4 on
verified evidence rather than convenience, the fault-observing continuation converts a silently
dropped exception into a logged one, and every structural test carries an honest statement of what
it does and does not prove.

Two things deserve specific credit before the findings.

**The shared-monitor rejection is correct and I verified it independently.**
`EmailMoveMonitor.BeforeItemMove` resolves its target with
`_hookedItems.FirstOrDefault(x => x.Mail.EntryID == mail.EntryID)`, invokes exactly one
`MoveAction`, and then removes the entry (`QuickFiler/Helper Classes/EmailMoveMonitor.cs:216-223`);
`UnhookAll` clears the whole instance list under lock (`:189-204`). The three owners register three
genuinely different actions — `RemovedItemMonitor(x.EntryID)`, `_masterQueue.Remove(x)` and
`RemoveItem(x)` — across 16 live call sites. Collapsing to one instance would therefore drop two of
every three per-mail actions, exactly as `spec.md:95` claims. The class is also demonstrably live,
so replacing the "It is now malfunctioning. Temprorarily disabling." comment with an accurate
description is a genuine correction rather than a cosmetic one.

**The RED-first evidence is real, not ceremonial.** The finding-2 fail-before artifact carries the
actual `ObjectDisposedException` stack through `BlockingCollection.get_IsCompleted` at
`QuickFiler/Controllers/QfcFormController.Actions.cs:322`, which is precisely the defect #731
finding 2 describes. Findings 3 and 4 each show the assertion reading real source or real
reflection metadata. Finding 1's fail-before exception dossier correctly explains that a
comment-only change has no defect state to reproduce, and then does the harder thing: it argues the
guard is discriminating in both directions rather than vacuous.

---

## CR-1 — `_undoQueueDisposal` is a production field that production never reads (Advisory, Medium)

**Location:** `QuickFiler/Controllers/QfcFormController.SetupDisposal.cs:208`, assigned at `:234`
and `:238`.

```csharp
private Task _undoQueueDisposal;
```

**Observation.** A repository-wide search finds exactly three references: the declaration and the
two assignments. Nothing in `QuickFiler` ever reads the field. Its sole reader is
`QfcFormControllerCleanupTests.Cleanup_WithFaultedConsumer_ObservesAndLogsTheFault`, which reaches
it by string-keyed reflection (`UndoQueueDisposalField = "_undoQueueDisposal"`). The XML `<summary>`
is candid about this: "so tests can observe that continuation deterministically."

**Why it matters.** The General Code Change Policy asks that the public surface be small and
intentional (§4.2, C#5.2), and this issue's own finding 3 removes two constructor parameters on
precisely the grounds that an unused member "misrepresents the type's contract" (`spec.md:56`). A
private field written twice and read never is the same defect class in a smaller package. It will
not be caught by the analyzer gate: IDE0052 ("remove unread private member") is not configured in
`.editorconfig` and defaults below warning, which is why the `/t:Rebuild` analyzer build reports
zero warnings.

**Mitigating.** Holding the continuation handle is defensible on its own merits — it keeps the
disposal task rooted and gives a future caller something to await. And the field is `private`, so
it leaks nothing outside the type.

**Inconsistency worth noting.** `QfcFormController` already has an established pattern for
test-observable seams: `UndoConsumerStarter`, `UndoItemProcessor` and `TimeProvider` are settable
members the test assigns directly (`controller.UndoConsumerStarter = body => body();`).
`_undoQueueDisposal` is the one seam reached by reflection instead, which makes it brittle to
rename and invisible to a reader looking for the seam set.

**Suggested resolution (either is sufficient, neither blocks):**
1. Promote it to the same visibility as its sibling seams so the test binds to it by name rather
   than by string, and the seam set is discoverable in one place; or
2. Keep it private and add one clause to the `<summary>` recording that production deliberately
   does not read it and that the field exists to root the continuation and expose it to tests.

---

## CR-2 — Source-inspection helpers are copy-pasted across the three new test files (Advisory, Medium)

**Locations:**

| Helper | File | Lines |
|---|---|---|
| `NormalizeWhitespace` | `QfcFormControllerCleanupTests.cs` | 132-154 |
| `NormalizeWhitespace` | `QfcMoveMonitorTopologyTests.cs` | 52-79 |
| `NormalizeSourceWhitespace` | `QfcCollectionControllerDefects468Tests.Volatile.cs` | 37-59 |
| repository-root resolution (parent walk) | `QfcFormControllerCleanupTests.cs` | 105-130 |
| repository-root resolution (parent walk) | `QfcCollectionControllerDefects468Tests.Volatile.cs` | 15-35 |
| repository-root resolution (fixed `..\..\..`) | `QfcMoveMonitorTopologyTests.cs` | 32-46 |

**Observation.** The whitespace collapser is byte-identical in the first two and differs only in
name and a null guard in the third — roughly 25 lines duplicated three times in one commit. The
repository-root resolver appears in two *different* implementations: a parent-directory walk that
stops at the first ancestor containing a `QuickFiler` folder, and a fixed three-level
`AppDomain.CurrentDomain.BaseDirectory + "..", "..", ".."` climb.

**Rule.** `.claude/rules/general-code-change.md` § Design Principles, priority 2: "Reusability —
Factor out logic that is clearly reusable. Avoid copy-paste; share behaviour via composition or
helper methods."

**Additional risk in the fixed-climb variant.** `QfcMoveMonitorTopologyTests.ReadControllerSource`
hard-codes the depth from the test output directory to the repository root. It works today because
the assembly lands in `QuickFiler.Test/bin/Debug/`, but it will break silently — as a test failure,
not a compile error — if the output path ever gains or loses a level (a `net48` TFM subfolder, a
platform subfolder, a different `OutputPath`). The parent-walk variant used in the other two files
is robust to that and should be the single surviving implementation.

**Suggested resolution:** extract one internal static helper class in `QuickFiler.Test/Controllers/`
(for example `SourceInspection`) exposing `ReadRepositorySource(params string[] segments)` and
`NormalizeWhitespace(string)`, and have all three files call it. A fourth pre-existing copy already
lives in `QfcHighConfidencePreFilterTests`, so the extraction would pay for itself immediately.

---

## CR-3 — The `#233` structural pin is narrower than the criterion it replaces (Advisory, Low)

**Location:** `QuickFiler.Test/Controllers/QfcDatamodelTests.cs`,
`QfcRemainingQueueAdmission_DeclaresNoScoringDelegate`.

The test asserts that no constructor parameter and no field has type
`Func<MailItem, CancellationToken, Task<long>>` — the exact type that was removed. `spec.md` AC11
asks for the broader property that "the constructor declares no scoring-delegate parameter and the
type declares no scoring-delegate field." A future re-introduction under a different shape, for
instance `Func<MailItem, Task<double>>` or an `IScoreLoader` interface, would pass this test.

**Assessment.** The replacement is nonetheless *stronger* than what it replaced in the dimension
that matters most. The old test proved that admission did not call the scorer on one path; the new
test proves the type cannot score at all. Four sibling tests also carried throwing scorers as
negative pins and lost that mechanism in the same edit, and the structural test subsumes all four,
because a type with no scoring delegate cannot score on any path. I record this as a note rather
than a gap.

**Suggested resolution:** assert the constructor's full parameter type list equals the three
expected types, which pins the signature exactly and cannot be evaded by a differently shaped
delegate.

---

## CR-4 — The three per-owner comments are a single 290-character line, triplicated verbatim (Advisory, Low)

**Locations:** `QfcCollectionController.cs:84`, `QfcDatamodel.cs:104`, `QfcQueue.cs:41`.

Each is one unwrapped comment line of roughly 290 characters, and the three are byte-identical to
each other. The same commit wraps its `EmailMoveMonitor` class comment to about 100 columns across
five lines (`EmailMoveMonitor.cs:17-21`), which is the style the surrounding code uses. CSharpier
does not reflow comments, so the formatter neither caused nor objects to this; it is a free choice.

**Assessment.** Content is correct and complete — each comment states the mechanism, cites
`BeforeItemMove`/`FirstOrDefault` and instance-scoped `UnhookAll`, and references both `#731`
finding 1 and `#620`, satisfying AC1 in full. Only the presentation is inconsistent.

**Suggested resolution:** wrap the three comments to the same width as the `EmailMoveMonitor` one.
Note that this interacts with the AC19 diff bound on `QfcCollectionController.cs` (3 insertions,
1 deletion), so it is better done in the follow-up split work than here.

---

## CR-5 — `Cleanup_SourceContainsNoSynchronousWait` is a coarse whole-file substring scan (Advisory, Low)

**Location:** `QfcFormControllerCleanupTests.cs:377-397`.

The guard normalises every whitespace run in the whole file to a single space and then searches for
`.Wait(`, `.Result`, `Thread.Sleep` and `Task.Delay`. Two consequences:

- Normalisation can defeat it. `.Wait (` and `Task .Delay` survive as `.Wait (` and `Task .Delay`
  after collapsing, and neither matches the literal. A future author would have to be perverse to
  write those, but the guard promises more than it delivers.
- `.Result` is a bare substring, so an innocuous member such as `scanResult.Result` or a variable
  named `x.ResultCount` would trip it as a false positive.

The test's own `<summary>` is honest that it is "a forward guard, not a reproduction," and the
fail-before artifact records all four literals already absent pre-fix, so nothing is overstated.

**Suggested resolution:** scope the scan to the `Cleanup()` method body rather than the whole file,
and use a word-boundary regex rather than raw substrings. Low value; the current form is adequate
for its purpose.

---

## CR-6 — Repeated `Cleanup()` with a live consumer overwrites the disposal handle (Advisory, Low)

**Location:** `QfcFormController.SetupDisposal.cs:238`.

`_undoQueueDisposal = undoConsumer.ContinueWith(...)` overwrites any previous handle. If `Cleanup()`
ran twice while a consumer was in flight both times, the first continuation task would become
unreachable. Should that continuation ever fault — its only throw sources are `logger.Error` and a
second `Dispose` on an already-disposed `BlockingCollection`, which is idempotent — the fault would
be unobserved, which is a narrower instance of the very class of defect finding 2 fixes.

**Assessment.** Not reachable in practice and not a regression: today's code has the same shape with
a wider fault surface. Recorded for completeness only. The `Cleanup_CalledTwice_DoesNotThrow` test
covers the realistic repeated-cleanup path (null consumer).

---

## CR-7 — `Cleanup()` reads `_undoConsumerTask` without synchronisation (Advisory, Low; verified benign)

**Location:** `QfcFormController.SetupDisposal.cs:222`, `var undoConsumer = _undoConsumerTask;`.

`UndoConsumer`'s `finally` sets `_undoConsumerTask = null` from a pool thread (the loop awaits with
`ConfigureAwait(false)`), while `Cleanup()` reads it on the UI thread. There is a genuine cross-thread
read of a non-volatile reference field, in the same change that fixes an unsynchronised read of a
counter for finding 4.

**Why it is benign — traced rather than assumed.** Both observable outcomes are safe:

- Reading a stale non-`null` reference to an already-completed task causes `ContinueWith` to run the
  continuation immediately. Correct.
- Reading `null` can only happen once the consumer has entered its `finally`, and the `finally` is
  the last statement of `UndoConsumer`. After it, the state machine performs no further access to
  `_undoQueue`, so the immediate `undoQueue?.Dispose()` on the `null` branch cannot fault the
  consumer. The narrow window between the `finally` and task completion is therefore harmless.

Additionally, `UndoDialog()` — the only writer of `_undoConsumerTask` other than that `finally` —
returns immediately post-cleanup because its guard `_movedItems is null || _globals?.Ol?.App is null`
trips once `Cleanup()` has nulled both (`QfcFormController.Actions.cs:263-266`). I verified that
guard directly; `spec.md:113` relies on it and is correct.

**No change recommended.** Recorded so a future reader does not rediscover it as a defect.

---

## CR-8 — Field declaration placed between methods (Advisory, Low)

**Location:** `QfcFormController.SetupDisposal.cs:207-208`.

`_undoQueueDisposal` is declared between `UnregisterFormEventHandlers()` and `Cleanup()` rather than
with the type's other fields in `QfcFormController.cs:88-91`, where `_undoQueue` and
`_undoConsumerTask` live together. Placing it beside its collaborators would make the trio readable
as one unit. Cosmetic; the partial-class split makes a local declaration defensible.

---

## CR-9 — `evidence/qa-gates/file-size-audit.md` overstates where a follow-up is recorded (Advisory, Low)

**Location:** `evidence/qa-gates/file-size-audit.md:78`.

The artifact states that splitting `QfcCollectionController.cs` and `QfcQueue.cs` "is recorded as a
follow-up in spec.md and at the foot of the plan." The plan does record both
(`plan.2026-09-02T12-02.md:345-346`); `spec.md:248-250` records only the `QfcCollectionController`
split and never mentions `QfcQueue.cs` anywhere in its Scope & Non-Goals or Rollout sections.
`QfcQueue.cs` was already 505 lines at the merge-base and is 507 now, so the file crossed further
over the ceiling without a spec-level acknowledgement — caught by the `[P5-T10]` gate rather than
declared up front.

**Suggested resolution:** correct the clause, and add `QfcQueue.cs` to the spec's follow-up list (or
promote it directly to a potential entry, which is owed regardless — see policy audit PA-5).

---

## Design and correctness assessment of the delivered fixes

### Finding 2 — signal, then defer disposal

The shape is right. `CompleteAdding()` is the stop signal the consumer loop already reads at
`Actions.cs:322`, so no new cancellation token and no change to the loop were needed — the smallest
change that closes the defect. Calling it before `Dispose()` is required, because `CompleteAdding`
declares `ObjectDisposedException`; the ordering is correct in the source and pinned by
`Cleanup_WithRunningConsumer_CompletesAddingBeforeDisposing`.

The rejection of a synchronous wait is well founded, and I traced the caller rather than accepting
the spec's assertion. The only unqualified in-type call is `Cleanup();` at
`QuickFiler/Controllers/QfcFormController.EventHandlers.cs:93`, inside `ActionCancelAsync`,
immediately after `await _formViewer.UiSyncContext` at `:89` — so it runs on the UI thread. Blocking
there while the consumer sits at a dispatcher hop inside `ProcessUndoItemAsync` is a genuine
deadlock, not a theoretical one. (I verified this one call site; I did not exhaustively enumerate
interface-dispatched `IQfcFormController.Cleanup()` calls from other types, which would only add
further UI-thread-reachable callers, not remove this one.)

`TaskScheduler.Default` on the `ContinueWith` is the right choice: it prevents the continuation from
being scheduled back onto a captured UI context that `Cleanup()` may be tearing down.

The `catch (ObjectDisposedException)` is narrowly typed, documented in place, and covers exactly the
repeated-`Cleanup()` case the accompanying test exercises. It is not a broad swallow.

Accepted residual risk, correctly disclosed in `spec.md:117`: if the per-item processor hangs, the
consumer never drains and the queue is never disposed. That is strictly better than disposing under
an active consumer and does not block the UI thread.

### Finding 4 — `Volatile.Read` rather than `volatile`

The reasoning is correct and I verified the constraint is real rather than assumed: passing a
`volatile` field by `ref` to `Interlocked.Increment`/`Decrement` produces CS0420, and the type-check
gate runs `/p:TreatWarningsAsErrors=true` with no `NoWarn` on the affected projects, so the issue's
first-listed suggestion would convert two clean lines into two build errors. `Volatile.Read` is the
right instrument: a pure acquire load, no write traffic, and it reads as a read at the call site.

The honest limitation, which the author states himself in `<remarks>`, is that the regression test
is a structural proxy and proves nothing about memory ordering. Combined with the fact that
`QfcCollectionController.cs` is uninstrumented (policy audit PA-1), the changed line carries no
coverage signal at all. Execution of the enclosing method *is* proven by the two passing issue-#286
tests. This is the best available verification for a memory-visibility fix under a determinism rule
that forbids thread-racing tests, and I do not fault it.

### Finding 3 — removing both dead parameters

Removing `globals` alongside `scoreLoader` is the right call and is properly recorded as an explicit
in-scope decision beyond the literal issue text (`spec.md:56`). Both parameters were the same defect
class in the same 48-line file and the same constructor already being edited. The type is
`internal sealed` with an `internal` constructor, so no public contract moved, and both call sites
were updated in the same commit. A pleasing side effect this reviewer measured: the file's line
coverage rose from 92.00% to 100.00%, because the two lines removed were the dead parameter's null
guard and throw.

The instruction not to delete the issue-#233 pin outright but to replace it was followed, and the
rationale sentence is carried verbatim into the new assertion message.

### Finding 1 — document, do not share

Correct on the evidence, as set out at the head of this review. The topology pin is discriminating
in both directions: `NoTypeDeclaresMoreThanOneEmailMoveMonitorField` asserts exactly three declaring
types, so a collapse to fewer and an unnoticed addition of a fourth both fail, and
`EachOwnerDeclaresExactlyOneEmailMoveMonitorInitializer` fails if any single owner's initializer is
removed or duplicated. The reflection-based half of the pin is the stronger half, because it survives
source reformatting entirely.

The invariant "do not rename `_moveMonitor`" was respected; the field name is unchanged on all three
owners, and the suite's 6995 passing tests include every existing reflection-based injector.

---

## Test quality assessment

Positive:

- Determinism infrastructure is used correctly and completely: `FakeTimeProvider` for the clock, an
  inline `UndoConsumerStarter`, and an inert `UndoItemProcessor`. No `Thread.Sleep`, no `Task.Delay`,
  no polling spin, no thread racing. `clock.Advance(TimeSpan.FromSeconds(11))` advances simulated
  time explicitly past the ten-second idle threshold.
- Every test drains its deferred disposal before returning
  (`clock.Advance(...); await Task.WhenAny(consumer); _ = consumer.Exception;`), so no parked
  consumer or unobserved fault outlives the test and leaks into a sibling. This is careful work and
  is the reason the suite stays deterministic under `/InIsolation`.
- Failure messages interpolate the observed value where it helps. The RanToCompletion assertion
  passes the consumer's `AggregateException` into the `because:`, which is exactly why the
  fail-before artifact could record the real `ObjectDisposedException` stack instead of a bare
  "expected 5 but found 7".
- Scenario coverage for finding 2 is complete: running, parked, absent and faulted consumers, plus
  repeated invocation and the ordering property itself.

Neutral observations, not findings:

- Three of the ten new methods are source-inspection tests rather than behavioural ones. Each is
  justified in its own class or method documentation, and two of the three are explicitly labelled
  forward guards that pass on the pre-change tree. That labelling is what keeps them honest.
- `Cleanup_WithFaultedConsumer_ObservesAndLogsTheFault` verifies the *observe* half of its name by
  assertion and the *log* half by code inspection only; there is no logger seam on this type. The
  production code does call `logger.Error`, which I confirmed at
  `QfcFormController.SetupDisposal.cs:243`. Adding a logger seam for this alone would not be worth
  the surface it adds.

---

## Summary of findings

| ID | Severity | Blocking | Summary |
|---|---|---|---|
| CR-1 | Medium | No | `_undoQueueDisposal` is written twice and never read by production; reflection-reached seam inconsistent with the type's other seams. |
| CR-2 | Medium | No | `NormalizeWhitespace` triplicated and two divergent repository-root resolvers across the three new test files; the fixed `..\..\..` climb is brittle. |
| CR-3 | Low | No | The `#233` structural pin matches one exact delegate type rather than any scoring-delegate shape. |
| CR-4 | Low | No | Three 290-character unwrapped comment lines, inconsistent with the wrapped style used in the same commit. |
| CR-5 | Low | No | The no-synchronous-wait guard is a whole-file substring scan that normalisation can defeat. |
| CR-6 | Low | No | A repeated `Cleanup()` with a live consumer overwrites the disposal handle. |
| CR-7 | Low | No | Unsynchronised cross-thread read of `_undoConsumerTask`; traced and verified benign. |
| CR-8 | Low | No | Field declared between methods rather than with its collaborators. |
| CR-9 | Low | No | `file-size-audit.md:78` claims a follow-up is in `spec.md` when only the plan records it. |

Cross-referenced policy findings are in `policy-audit.2026-09-03T15-35.md` section 8: PA-1
(pre-existing `[ExcludeFromCodeCoverage]` on two production files), PA-2 (500-line ceiling), PA-3
(post-pass formatter probe), PA-4 (coverage collection path), PA-5 (unpromoted follow-ups).

**Nothing in this review blocks merge.**
