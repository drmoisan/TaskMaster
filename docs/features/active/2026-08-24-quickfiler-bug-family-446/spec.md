# quickfiler-queue-datamodel-defects (Spec)

- **Issue:** #446
- **Also delivered:** #448, #426, and the producer half of issue #427 (Scope 427-A only — see Scope & Non-Goals)
- **Parent (optional):** epic `quickfiler-bug-family` (integration branch `epic/quickfiler-bug-family-integration`)
- **Owner:** drmoisan
- **Last Updated:** 2026-08-24
- **Status:** Approved
- **Version:** 1.0
- **Work Mode:** `full-bug`

> **Acceptance-criteria authority.** Work mode is `full-bug`. Per
> `.claude/skills/acceptance-criteria-tracking/SKILL.md`, this file is the **sole** authoritative
> acceptance-criteria source for this feature. No `user-story.md` exists for it and none is to be
> created. The `## Acceptance Criteria` section below is what the executor and the reviewer check off.

**Primary evidence source.** `research/2026-08-24T09-50-quickfiler-queue-datamodel-defects-research.md`, whose
citations were verified against `988e819b` and have NOT been re-verified since. PR #610 shifted every line number in
`QuickFiler/Controllers/QfcStreamingDequeueConfidenceGate.cs`, so a gate line number read from the research must be
re-resolved against this specification, whose every citation was re-verified against branch HEAD (PR #610, `507a40a5`).
The four promoted potential documents were captured at `fb32b923`; where they disagree with the research, the research
is authoritative on FINDINGS but not on line numbers. Three of their claims are corrected in this specification
(Root Cause Analysis §4.4, §2.5 and §4.5 of the research).

---

## Context

Four pre-existing defects sit on or beside a single code path:
`QfcStreamingDequeueConfidenceGate.DequeueAsync` -> `QfcDatamodel.DequeueWithHighConfidenceGateAsync`
-> `IQfcDatamodel` -> `QfcHomeController.IterateQueueAsync`. They are corrected together rather than as
four independent patches, because each of them separately changes the same gate signature and therefore
separately invalidates the same reflective test helper.

| Issue | Defect | Severity |
| --- | --- | --- |
| #446 | `QfcHomeController.IterateQueueAsync` treats a deadline-expired empty dequeue as proof of source exhaustion and irreversibly closes the UI queue, silently dropping queued items for the rest of the session. | High — silent data loss |
| #448 | `QfcFormController.UndoConsumer()` has a loop that never terminates; past its 10-second idle threshold it busy-spins on a thread-pool thread for the life of the process. | High — hang and CPU burn |
| #426 | Mail items rejected by the high-confidence dequeue gate are removed from the master queue but never unhooked from `EmailMoveMonitor`, retaining a live `MailItem` COM reference and a `BeforeItemMove` subscription per rejected candidate. | Medium — session-scoped COM retention |
| #427 | Every accepted mail item is scored twice: the gate computes and discards the top folder, then `QfcItemController` re-runs the identical `FolderPredictor` sequence after `Show()`. | Low — wasted work |

**#446 is the driver.** It is the only one of the four that loses user work: in high-confidence mode
against a low-yield folder, the user is denied the remainder of their queue for the session, with no
error shown and no recovery short of relaunching QuickFiler.

- **Observed environment:** Windows 11 Pro 10.0.26200; C# / .NET Framework 4.8.1 VSTO add-in for Outlook;
  QuickFiler launched from the TaskMaster ribbon with `QfSettings.HighConfidenceModeEnabled = true`.
- **Customer impact:** any QuickFiler user running high-confidence mode against a mailbox where a low
  proportion of items clears the confidence threshold. #446 and #427 are high-confidence-mode-only.
  #448 affects any session in which the undo dialog is opened. #426 scales with the number of rejected
  candidates, so the issue #424 fix — which made high-confidence scanning practical — increased exposure.
- **First observed / versions impacted:** all four were found by static analysis during read-only research
  in 2026-08 (#426 and #427 during the issue #424 investigation; #446 and #448 during epic #136 coverage
  research). #446 was **introduced** by issue #424, which gave the two-argument dequeue overload a
  first-batch deadline the post-UI iteration call site was not written for. #426, #427 and #448 predate it.
  All four are live at branch HEAD, which carries PR #610 (`507a40a5`); nothing in the intervening work, including PR #610, fixed any of them.

---

## Repro & Evidence

All citations below are file:line values at branch HEAD, which carries PR #610 (`507a40a5`). Where a promoted
potential document cited a different location, the correction is noted.

### #446 — deadline-expired empty batch closes the queue

**Steps to reproduce**

1. Enable High Confidence mode with a threshold few messages clear.
2. Launch QuickFiler against a folder large enough that scoring a batch exceeds the 12-second first-batch
   deadline.
3. File the first batch so `IterateQueueAsync` runs for the next batch.
4. Observe whether further batches are ever presented.

**Expected vs actual.** Expected: an empty batch caused by a scan deadline leaves the queue open so a
later iteration can supply items; the queue closes only on genuine source exhaustion. Actual: the empty
result routes to `QfcQueue.CompleteAddingAsync`, which reaches `BlockingCollection<T>.CompleteAdding()` at
`QuickFiler/Controllers/QfcQueue.cs:59`. That operation is irreversible and `QfcQueue` never reassigns
`_queue` outside `ChangeIterationSize` (`QfcQueue.cs:501`), so no further items can be enqueued for the
remainder of the session while unscanned items remain in the master queue.

**Evidence.**

- `QuickFiler/Controllers/QfcHomeController.Iteration.cs:21-24` calls the two-argument
  `_datamodel.DequeueNextItemGroupAsync(_formController.ItemsPerIteration, 2000)`.
- `QuickFiler/Controllers/QfcDatamodel.QueueProcessing.cs:66-76` delegates that overload to the
  deadline-bearing path with `QfcStreamingDequeueConfidenceGate.DefaultFirstBatchDeadline`.
- `QuickFiler/Controllers/QfcStreamingDequeueConfidenceGate.cs:22` — the default is 12 seconds.
- `QuickFiler/Controllers/QfcHomeController.Iteration.cs:32-36` — the `else` branch infers exhaustion from
  `listObjects.Count == 0`; the irreversible close is the `CompleteAddingAsync` call at `:35`.
  (The potential document cited `:32` for the whole branch; the close is specifically at `:35`.)

**Frequency / determinism.** Deterministic given the data condition: it fires whenever a high-confidence
scan reaches the deadline with zero accepted candidates. Not reproducible in normal (non-high-confidence)
mode, which routes through `DequeueDirectAsync` (`QfcDatamodel.QueueProcessing.cs:101-108`) and never sees
a deadline.

**Logs.** The deadline path emits a `logger.Debug` line via `LogDeadlineExpiry`
(`QfcStreamingDequeueConfidenceGate.cs:157-165`), but the production wiring passes `null` for `debugLog`
(`QfcDatamodel.QueueProcessing.cs:122`) and the caller cannot read log4net output. The failure therefore
presents as QuickFiler ending the session early, with no error.

### #448 — `UndoConsumer` never terminates

**Steps to reproduce**

1. Open QuickFiler and file at least one item so `_movedItems` is populated.
2. Click the Undo button, which reaches `UndoDialog()` and starts the consumer at
   `QuickFiler/Controllers/QfcFormController.Actions.cs:211`.
3. Leave the undo queue empty for more than ten seconds.
4. Observe sustained CPU consumption on a thread-pool thread for the remaining life of the Outlook process.

**Expected vs actual.** Expected: the loop terminates once the queue is drained and the idle threshold
elapses, the empty-queue path always yields, and `_undoConsumerTask` is reset so a later `UndoDialog()`
starts a fresh consumer. Actual: the loop never exits and, past the threshold, never yields.

**Evidence** (`QuickFiler/Controllers/QfcFormController.Actions.cs:253-292`, lines unchanged from the
potential document):

- `:258` — `while (!_undoQueue.IsCompleted || exit)`. `_undoQueue` is a
  `BlockingCollection<IMovedMailInfo>` declared at `QuickFiler/Controllers/QfcFormController.cs:90`, and a
  repo-wide search for `CompleteAdding` shows no code path calls it on that collection. `IsCompleted` is
  therefore permanently `false`, so `!_undoQueue.IsCompleted` alone holds the condition true regardless of
  `exit`; the disjunction makes setting `exit` strengthen the condition rather than break it.
- `:279-282` — the `sw.ElapsedMilliseconds > 10000` branch sets `exit = true`, contains no `await` and no
  yield, and remains true forever because `sw` is never stopped or reset. The only awaits in the loop are
  inside the take branch (`:262`, `:268`, `:274`) and the `Task.Delay(200)` at `:285`, which is in the
  third branch and is unreachable once the threshold is crossed.
- `:288-291` — the `_undoConsumerTask = null;` reset is unreachable in any normal termination, so the
  `??=` guard at `:211` prevents any later `UndoDialog()` from starting a fresh consumer.

**Frequency / determinism.** Deterministic: every session that reaches `UndoDialog()` and then idles for
ten seconds enters the spin.

**Correction to the potential document.** It states that epic child F6 "works around it by introducing an
injectable start-delegate around `Task.Run(UndoConsumer)`". **That seam does not exist at `988e819b`.**
`Actions.cs:211` is a bare `_undoConsumerTask ??= Task.Run(UndoConsumer);`; a repo-wide search for
`UndoConsumer` finds only `Actions.cs:211`, `Actions.cs:253` and
`QuickFiler.Test/Controllers/QfcFormControllerTests.cs:688`; and no `docs/features/**/*435*` folder
exists. This feature builds the seam from scratch.

### #426 — rejected candidates retain their `EmailMoveMonitor` hook

**Steps to reproduce**

1. Enable High Confidence mode and launch QuickFiler against a folder where most messages score below the
   threshold.
2. Let the gate scan and reject a large number of candidates while assembling the first batch.
3. Inspect `EmailMoveMonitor._hookedItems`, or observe process COM-reference growth, while the session
   remains open.

**Expected vs actual.** Expected: an item removed from the master queue is unhooked from the move monitor
whether the gate accepted or rejected it, so `_hookedItems` tracks only items still under management.
Actual: only accepted items are unhooked; `_hookedItems` accumulates one `EmailMoveAction` per rejected
candidate for the life of the session, each holding a live `MailItem` COM reference and a live
`Folder.BeforeItemMove` subscription.

**Evidence.**

- Accepted items are released by `UnhookDequeuedNodes`
  (`QuickFiler/Controllers/QfcDatamodel.QueueProcessing.cs:145-166`, **moved** from the cited `:107-128`),
  which calls `TryUnhookOrReplace` (`:29-64`, **moved** from `:18`) and reaches
  `_moveMonitor.UnhookItem(node)` at `:44` (**moved** from `:33`).
- The gate takes candidates through the bare delegate `() => _masterQueue.TryTakeFirst()` at
  `QuickFiler/Controllers/QfcDatamodel.QueueProcessing.cs:118` (**moved** from `:82`).
- `QuickFiler/Controllers/QfcStreamingDequeueConfidenceGate.cs:139-151` has **no `else`** on the accept
  decision. A below-cutoff item has already been removed from `_masterQueue` by `_tryTakeNext()` at
  `:122`, is never added to `accepted`, and therefore never reaches `UnhookDequeuedNodes`.
- `EmailMoveMonitor.HookItem` (`QuickFiler/Helper Classes/EmailMoveMonitor.cs:46-61`, **corrected** from
  `:46-58`) subscribes `folder.BeforeItemMove` at `:57` and adds to `_hookedItems` (`:58`). Nothing removes
  an entry except `UnhookItem` (`:63-88`) or `UnhookAll` (`:185-200`), and the datamodel's `UnhookAll` runs
  only from `QuickFiler/Controllers/QfcDatamodel.cs:80` at cleanup.

**Frequency / determinism.** Deterministic and proportional to the number of rejected candidates. No data
loss or incorrect filing results; everything is released at `Cleanup()`.

### #427 — post-`Show()` duplicate scoring

**Steps to reproduce**

1. Enable High Confidence mode and launch QuickFiler.
2. Enable debug logging and inspect the `Probability debug` entries for a single accepted item.
3. Observe one entry tagged `[QfcDatamodel.ScoreRemainingQueueMailItemAsync (master-queue admission)]`
   during the pre-UI scan and a second, independent classification tagged
   `[QfcItemController.LoadFolderHandlerAsync (FromField)]` after the form is shown.

**Expected vs actual.** Expected: an accepted item carries its already-computed top-folder suggestion
forward. Actual: the score is computed, the folder is discarded, and the full sequence runs a second time
per accepted item after `Show()`.

**Evidence.**

- `QuickFiler/Controllers/QfcDatamodel.cs:363-377` (**moved** from the cited `:346-360`) calls
  `FolderScoringService.ScoreAsync`, which returns `(long Score, string TopFolder)`
  (`QuickFiler/Controllers/QfcHighConfidencePreFilter.cs:143-147`, implemented `:170-189`), and returns
  only `score.Score` at `:376`. The `TopFolder` computed at `QfcHighConfidencePreFilter.cs:187` is dropped,
  and so is the fully-initialised `FolderPredictor` built at `:179-184`.
- `QuickFiler/Controllers/QfcItemController.FolderHandling.cs:57-131` (**corrected** from the cited
  `:57-90`; the body is longer now) re-runs the same `FolderPredictor` + `InitAsync(FromField)` sequence at
  `:64-85`.
- The dormant carrier overload `LoadItemsAsync(IList<QfcPreScoredItem>)` is at
  `QuickFiler/Controllers/QfcFormController.Actions.cs:114-117` and `:120-164`; the plain overload at
  `:62-65` and `:67-105` is what the live path selects, from the non-owned
  `QuickFiler/Controllers/QfcHomeController.cs:310`.

**Frequency / determinism.** Deterministic; one redundant scoring pass per accepted item in
high-confidence mode.

---

## Scope & Non-Goals

### In scope — files this feature owns

No sibling epic child writes these:

| File | Lines at branch HEAD (post-PR #610) | Role in this change |
| --- | --- | --- |
| `QuickFiler/Controllers/QfcStreamingDequeueConfidenceGate.cs` | 177 | gate result shape, `onRejected` callback, widened score loader, `QfcGateBatch` |
| `QuickFiler/Controllers/QfcDatamodel.QueueProcessing.cs` | 177 | datamodel-side wiring, rejection release, outcome-bearing dequeue |
| `QuickFiler/Controllers/QfcDatamodel.cs` | **496** | `ScoreRemainingQueueMailItemAsync` tuple return plus the adapter lambda at `:355` |
| `QuickFiler/Controllers/QfcFormController.Actions.cs` | 302 | `TimeProvider` and `UndoConsumerStarter` seams, corrected `UndoConsumer` loop |
| `QuickFiler/Controllers/QfcHomeController.Iteration.cs` | 86 | `IterateQueueAsync` stop-reason guard |
| `QuickFiler/Interfaces/IQfcDatamodel.cs` | 59 | additive interface member, `QfcDequeueStop`, `QfcDequeueBatch` |
| `QuickFiler/Controllers/QfcItemController.FolderHandling.cs` | 235 | expected untouched under Scope 427-A |
| `QuickFiler/Helper Classes/EmailMoveMonitor.cs` | 262 | expected untouched — the #426 defect is in the caller, not the monitor |

Ownership boundaries that must not be crossed:

- `QfcHomeController.Iteration.cs` is ours; the sibling partials `QfcHomeController.cs` and
  `QfcHomeController.Metrics.cs` belong to feature 442.
- `QfcItemController.FolderHandling.cs` is ours; every other `QfcItemController` partial belongs to
  features 484, 444 or 489.

### In scope — test files

Existing test files only. Per research §6.1 every regression test fits an existing file:
`QfcStreamingDequeueConfidenceGateTests.cs`, `QfcStreamingDequeueConfidenceGateTests.Part3.cs`,
`QfcHomeControllerIterationTests.cs`, `QfcFormControllerSeamTests.cs`, `QfcQueuePurePathsTests.cs`,
`QfcDatamodelTests.cs`.

### Non-goals

1. **No non-owned production file is written.** The sibling-owned files named in Root Cause Analysis §427
   are out of bounds.
2. **No project file is edited.** `QuickFiler/QuickFiler.csproj` uses 125 explicit `Compile Include`
   entries and is shared with sibling epic children, so a new production file would force an edit to a
   non-owned project file. No new production file is created (decision D2). Likewise
   `QuickFiler.Test/QuickFiler.Test.csproj` is not edited, because no new test file is created
   (decision D4).
3. **The drop-on-reject contract is preserved unchanged.**
   `QuickFiler.Test/Controllers/QfcStreamingDequeueConfidenceGateTests.cs:298-310`
   (`DequeueAsync_BelowThresholdItemsAreDiscarded`) pins that a below-cutoff item is absent from the
   result and gone from the source queue. This work adds one new observable — the monitor hook is released
   — and does not change what that test asserts. Do not modify that test.
4. **The STA thread-affinity contract of issues #214 and #420 is preserved.** The invariant is exactly one
   `_marshalToSta` invocation per public `EmailMoveMonitor` operation with no COM member touched outside
   it, pinned by `QuickFiler.Test/Helper Classes/EmailMoveMonitorTests.cs:176-198`. Calling
   `UnhookItem` once per rejected item is automatically compliant; batching several unhooks into one
   marshal hop is prohibited because it would require a new monitor member and break that pin.
5. **The consumer half of issue #427 is out of scope.** See the scope decision immediately below.
6. **The dead `scoreLoader` parameter on `QfcRemainingQueueAdmission` is not removed.** That file is not
   owned, and `QfcDatamodelTests.cs:21-46` constructs the type with five arguments.
7. **`#nullable enable` is not added to any owned file.** None of the eight owned production files
   participates in nullable analysis today (research §9); opting a file in retroactively is a scope
   expansion unrelated to these defects, and `QfcDatamodel.cs` has no line budget for it.
8. **Pre-existing line-cap violations are not remediated.** `QfcQueue.cs` (610),
   `QfcCollectionController.cs` (2349) and `QfcFormControllerTests.cs` (827) are already over the cap.
   This work must not grow any of them.

### Scope decision on issue #427 — partial delivery only (decision D1)

Issue #427 is delivered at **Scope 427-A, the producer side only**. This feature stops discarding
`TopFolder` and carries it to the datamodel boundary. **Nothing consumes it yet.**

The potential document's premise — that activating the dormant
`LoadItemsAsync(IList<QfcPreScoredItem>)` overload eliminates the duplicate scoring — is **false**, and
the research proves it (§4.5). `_predeterminedFolder` is consumed only for *selection* inside
`AssignFolderComboBox` (`QuickFiler/Controllers/QfcItemController.FolderHandling.cs:193-199`); the
surrounding code at `:170`, `:182` and `:189-192` still requires `_folderHandler.FolderArray`,
`.Suggestions` and `.FolderRowArray`, all of which come only from a fully-initialised `FolderPredictor`
produced by `LoadFolderHandlerAsync`. Carrying only `TopFolder` forward therefore changes which entry is
preselected — behaviour the code already implements — and saves no scoring pass.

A genuine fix requires threading the initialised predictor through six files owned by sibling epic
children: `QfcHomeController.cs`, `QfcCollectionController.cs`, `QfcItemController.cs`,
`QfcItemController.Initialization.cs`, `QfcHighConfidencePreFilter.cs` and `QfcItemGroup.cs`. It would
also require deliberately rewriting five pinned assertions in `QfcHomeControllerIssue218Tests.cs:137-259`
and `QfcHomeControllerRunAsyncHighConfidenceTests.cs:246`/`:277`.

**Consequences that bind this work:**

- Issue #427 is only partially addressed here and **must remain open after this work merges**. It is not
  to be auto-closed.
- No GitHub closing keyword for #427 appears anywhere: not in this document, not in any commit message,
  not in the pull-request body. A negated sentence containing a closing keyword still auto-closes the
  issue, so the keyword must not be written at all.
- The pull-request body carries closing keywords for **#446, #448 and #426 only**. The `Also closes:` line
  in `issue.md` lists #427; that listing is superseded by this decision and must not be transcribed into
  the pull-request body.
- The consumer half is recorded as a follow-up (see Rollout & Follow-up).

---

## Root Cause Analysis

### #446 — the dequeue result carries no reason field

**Confirmed root cause.** `QfcStreamingDequeueConfidenceGate.DequeueAsync`
(`QuickFiler/Controllers/QfcStreamingDequeueConfidenceGate.cs:89-93`) returns
`Task<IList<MailItem>>`. Its return type carries no reason field, and every exit returns the same
`accepted` list:

| Line | Exit | Meaning |
| --- | --- | --- |
| `:100` | `return accepted;` (empty) | `quantity <= 0` — degenerate |
| `:119` | `return accepted;` | **deadline expired** (guarded at `:112-116`, logged at `:118`) — after PR #610 this exit is reachable only when `accepted.Count == 0` |
| `:128` | `return accepted;` | take returned null and (`timeOut <= 0`) or (already waited once and `_sourceActive()` false) — **source exhausted** |
| `:154` | `return accepted;` | `accepted.Count == quantity` — quantity satisfied |

`IterateQueueAsync` observes only `listObjects.Count`. `Count == 0` is produced by both `:119` and `:128`,
and there is no other observable difference on the production path. The caller therefore cannot
distinguish a bounded scan from an exhausted source by any means, and infers exhaustion.

**Affected components.** `QuickFiler/Controllers/QfcStreamingDequeueConfidenceGate.cs`,
`QuickFiler/Controllers/QfcDatamodel.QueueProcessing.cs`, `QuickFiler/Interfaces/IQfcDatamodel.cs`,
`QuickFiler/Controllers/QfcHomeController.Iteration.cs`, `QuickFiler/Controllers/QfcQueue.cs` (read-only —
the irreversible `CompleteAdding()` at `:59`).

**Complete production caller inventory for `DequeueNextItemGroupAsync`**, which constrains the fix shape:

| # | Call site | Overload | Owned |
| --- | --- | --- | --- |
| 1 | `QfcHomeController.Iteration.cs:21-24` (`IterateQueueAsync`) | 2-arg | Yes |
| 2 | `QfcHomeController.Iteration.cs:62-65` (`Iterate`) | 2-arg | Yes |
| 3 | `QfcHomeController.cs:260-263` (`Run`) | 2-arg | No |
| 4 | `QfcHomeController.cs:299-304` (`RunAsync`) | 4-arg | No |
| 5 | `QfcQueue.cs:476-479` (`ChangeIterationSize`) | 2-arg | No |

`QfcDatamodel` (`QuickFiler/Controllers/QfcDatamodel.cs:26`) is the only in-repo implementer of
`IQfcDatamodel`. `QuickFiler/Notes/notes_interfaces.cs:26` declares an unrelated same-named interface but
that file carries no `Compile Include` entry and does not compile. Every other reference is
`Mock<IQfcDatamodel>` in tests, and Moq generates missing members automatically.

### #448 — the loop condition is a disjunction and the idle timer never resets

**Confirmed root cause, two compounding faults plus a third the potential document missed.**

1. `while (!_undoQueue.IsCompleted || exit)` at `Actions.cs:258` is a disjunction. `IsCompleted` is
   permanently `false` because nothing calls `CompleteAdding()` on `_undoQueue`, so setting `exit`
   strengthens the condition rather than breaking it.
2. The threshold branch at `:279-282` reaches no `await` and no yield, so past ten seconds the loop is a
   tight CPU-bound spin on a thread-pool thread.
3. **Extra finding not in the potential document: the idle timer is never reset after a successful take.**
   `sw` is started once at `:256` and never restarted, so elapsed time measures *session* duration, not
   *idle* duration. Correcting only the loop condition would therefore convert a hang into a **premature
   exit**: a long-running undo session would cross the ten-second threshold while still productively
   draining the queue, and the consumer would stop with work outstanding. The timer must be reset on every
   successful take as part of the fix.

**Secondary hazard.** `QuickFiler/Controllers/QfcFormController.SetupDisposal.cs:216` disposes
`_undoQueue` in `Cleanup()` without calling `CompleteAdding()`, without cancelling, and without awaiting
`_undoConsumerTask`; it then sets `_globals = null` (`:217`) and `_groups = null` (`:220`). A spinning
consumer calls `TryTake` on a disposed `BlockingCollection<T>`, which throws `ObjectDisposedException` on
a fire-and-forget task whose exception is never observed, and the reset at `:288-291` is skipped because
the throw unwinds out of the loop body. `SetupDisposal.cs` is **not** owned; the fix addresses the hazard
from the owned side by making the reset unconditional and placing it in a `finally`.

**Determinism gap.** `QfcFormController` has **no** `TimeProvider` seam and **no** consumer-start seam.
`Actions.cs:255` uses `new Stopwatch()` and `:285` uses `await Task.Delay(200)` directly in production.
`.claude/rules/general-unit-test.md` bans `Thread.Sleep`, `Task.Delay`, real wall-clock waits and
`DateTime.Now` in test code and requires an injected `TimeProvider` plus `FakeTimeProvider`, so both
production constructs must be replaced before a compliant test can exist.

### #426 — the gate has no rejection path

**Confirmed root cause.** `QfcStreamingDequeueConfidenceGate.cs:139-151` has no `else` on the
`score >= _cutoff` decision. The candidate has already been removed from `_masterQueue` by `_tryTakeNext()`
at `:122`; if it is not added to `accepted` it is simply dropped, so it never reaches
`UnhookDequeuedNodes` and its `EmailMoveAction` stays in `_hookedItems` for the session.

**Extra finding not in the potential document: three independent monitor instances exist.**
`EmailMoveMonitor` is field-initialised in three unrelated places and is never injected in production:
`QfcDatamodel.cs:103`, `QfcQueue.cs:40`, and `QfcCollectionController.cs:78`. Consequently
`_moveMonitor.UnhookItem(group.MailItem)` at `QfcQueue.cs:76` can never match a hook registered by the
datamodel, because it consults a different `_hookedItems` list. This does not change the fix shape, but it
fixes which instance the fix must use: **the datamodel's own `_moveMonitor`**, and no other component can
incidentally release those hooks.

**Thread-affinity contract, stated exactly.** Documented at
`QuickFiler/Interfaces/IEmailMoveMonitor.cs:6-12` and `QuickFiler/Helper Classes/EmailMoveMonitor.cs:24-28`.
Operations that must run on the captured STA thread (all currently inside a `_marshalToSta(...)` lambda):
`mail.Parent` cast to `Folder` (`:54`, `:75`, `:142`); `folder.EntryID` / `mail.EntryID` reads (`:55`,
`:74`, `:75`, `:110-111`, `:160`, and inside `EmailMoveAction`'s constructor at `:239-240`);
`folder.BeforeItemMove += / -=` (`:57`, `:83`, `:119`, `:195`); and construction of `EmailMoveAction`
(`:58`). Operations that must **not** be marshalled: the null-argument early return in `UnhookItem`
(`:65-68`, pinned by `EmailMoveMonitorTests.cs:134-145` asserting zero marshal invocations), LINQ over the
cached `MailEntryId`/`FolderEntryId` strings, and `_hookedItems` bookkeeping.

### #427 — `TopFolder` is computed and discarded

**Confirmed root cause.** `QfcDatamodel.ScoreRemainingQueueMailItemAsync`
(`QuickFiler/Controllers/QfcDatamodel.cs:363-377`) narrows `FolderScoringService.ScoreAsync`'s
`(long Score, string TopFolder)` tuple to `score.Score` at `:376`.

**Extra finding — the second half of the fix is not where the potential document says it is.** See the
Scope & Non-Goals correction above. Removing the second scoring pass requires carrying the initialised
`FolderPredictor` / `IFolderSearchHandler`, not the `TopFolder` string, and that crosses six non-owned
files. Scope 427-A closes the producer half only.

### The reflective test helper fails **open** — a hazard that governs all three gate changes

**Extra finding not in any potential document.**
`QuickFiler.Test/Controllers/QfcStreamingDequeueConfidenceGateTests.cs:26-156` builds the gate by
reflection using a **descending fallback chain** of `GetConstructor` lookups: 8-type with progress
(`:45-76`), 7-type with deadline (`:78-107`), 6-type with sourceActive (`:109-136`), 5-type base
(`:138-155`). Only two constructors are actually declared — the 8-parameter one at
`QfcStreamingDequeueConfidenceGate.cs:57-66` and the 5-parameter convenience overload at `:33-40`.

Adding a ninth parameter makes the 8-type lookup return `null`; the 7-type and 6-type lookups also return
`null`; and the chain then **succeeds** on the 5-type lookup, silently constructing a gate with
`sourceActive = null`, the default deadline and no progress callback. Twenty-three test methods across the
three parts consume these helpers, so every deadline and source-active test would exercise a
differently-configured gate while still passing or failing for the wrong reason. The helper must be made
fail-closed as part of this work.

By contrast, the `DequeueAsync` invocation at `:192-205` casts to `Task<IList<MailItem>>`, so a return-type
change produces an `InvalidCastException` — a loud failure, which is the desirable outcome.

---

## Proposed Fix

### Design summary (what changes where)

One coherent result shape serves all three of #446, #427-A and #426, so the gate signature churns once
instead of three times.

**Layer 1 — gate internals, `QuickFiler/Controllers/QfcStreamingDequeueConfidenceGate.cs` (owned).**

```csharp
// internal readonly struct — net481 has no IsExternalInit, so no `record` and no `init` accessors.
internal readonly struct QfcGateBatch
{
    public QfcGateBatch(IList<QfcPreScoredItem> accepted, QfcDequeueStop stop, int scanned) { ... }
    public IList<QfcPreScoredItem> Accepted { get; }   // #427-A: carries TopFolder
    public QfcDequeueStop Stop { get; }                // #446: the reason
    public int Scanned { get; }
}

// #427-A: the score loader widens from Task<long> to Task<(long Score, string TopFolder)>.
private readonly Func<MailItem, CancellationToken, Task<(long Score, string TopFolder)>> _scoreLoader;

// #426: a new optional final constructor parameter, invoked in a new `else` at the accept decision.
private readonly Action<MailItem> _onRejected;

internal async Task<QfcGateBatch> DequeueAsync(int quantity, int timeOut, CancellationToken token)
```

Return-site mapping: `:100` -> `QuantitySatisfied` (degenerate, empty); `:119` -> `DeadlineExpired`;
`:128` -> `SourceExhausted`; `:154` -> `QuantitySatisfied`.

The `onRejected` invocation is wrapped so a monitor failure cannot abort the scan:
`try { _onRejected?.Invoke(item); } catch (System.Exception e) { logger.Error(...); }`. Do **not** reuse
`TryUnhookOrReplace` for this — its recovery path pulls a replacement item out of `_masterQueue`, which is
meaningless for a discarded candidate.

**Layer 2 — datamodel, `QfcDatamodel.QueueProcessing.cs` and `QfcDatamodel.cs` (both owned).**

- `ScoreRemainingQueueMailItemAsync` (`QfcDatamodel.cs:363-377`) returns
  `Task<(long Score, string TopFolder)>` instead of `Task<long>`. Its two consumers are
  `QueueProcessing.cs:119` (the gate, which wants the tuple) and `QfcDatamodel.cs:355` (a method-group
  conversion into `QfcRemainingQueueAdmission`). The second is repaired by an adapter lambda **inside the
  owned file**: `async (m, t) => (await ScoreRemainingQueueMailItemAsync(m, t)).Score,`.
  `QuickFiler/Controllers/QfcRemainingQueueAdmission.cs` (not owned) declares that `scoreLoader` at `:17`,
  null-checks it at `:23-26`, and **never assigns or invokes it** — the parameter is dead, so no change to
  that file is required.
- `DequeueWithHighConfidenceGateAsync` (`QueueProcessing.cs:110-130`) wires
  `onRejected: item => TryReleaseRejectedHook(item)`, which calls the **datamodel's own**
  `_moveMonitor.UnhookItem`, and returns the gate batch.
- `UnhookDequeuedNodes` continues to operate on the accepted `MailItem` list, unchanged.

**Layer 3 — interface, `QuickFiler/Interfaces/IQfcDatamodel.cs` (owned).** One additive member and the two
public carrier types (decision D2 — this file has 441 lines of headroom):

```csharp
public enum QfcDequeueStop { QuantitySatisfied, SourceExhausted, DeadlineExpired }

public readonly struct QfcDequeueBatch
{
    public QfcDequeueBatch(IList<MailItem> items, IList<QfcPreScoredItem> preScored, QfcDequeueStop stop) { ... }
    public IList<MailItem> Items { get; }              // always populated, all modes
    public IList<QfcPreScoredItem> PreScored { get; }  // populated in high-confidence mode; empty otherwise
    public QfcDequeueStop Stop { get; }
}

/// <summary>
/// Issue #446. Dequeues the next group and reports WHY the gate stopped, so a caller can distinguish a
/// deadline-bounded empty batch from genuine source exhaustion. The three pre-existing overloads are
/// unchanged and remain the batch-only contract.
/// </summary>
Task<QfcDequeueBatch> DequeueNextItemGroupWithOutcomeAsync(
    int quantity,
    int timeOut,
    TimeSpan firstBatchDeadline,
    Action<int, int, int> progress
);
```

`QfcDequeueStop` and `QfcDequeueBatch` are `public` because `IQfcDatamodel` is public and a public
member's parameter and return types must be at least as accessible. `QfcPreScoredItem` is already a
`public readonly struct` at `QuickFiler/Controllers/QfcHighConfidencePreFilter.cs:98-122`; this feature
**consumes** it and does not modify that file. The internal `QfcGateBatch` may reference the public
`QfcDequeueStop` without an accessibility conflict.

**All three existing `IQfcDatamodel` overloads remain, unchanged, and keep delegating internally**, so the
four non-owned production call sites (`QfcHomeController.cs:260`, `:299`, `QfcQueue.cs:476`, and
`QfcHomeController.Iteration.cs:62` if left alone) compile untouched.

**Layer 4 — caller, `QuickFiler/Controllers/QfcHomeController.Iteration.cs` (owned).**

```csharp
var batch = await _datamodel.DequeueNextItemGroupWithOutcomeAsync(
    _formController.ItemsPerIteration, 2000,
    QfcStreamingDequeueConfidenceGate.DefaultFirstBatchDeadline, null);

if (batch.Items.Count > 0)
{
    await QfcQueue.EnqueueAsync(batch.Items, _formController.Groups).ConfigureAwait(false);
}
else if (batch.Stop == QfcDequeueStop.SourceExhausted)
{
    await QfcQueue.CompleteAddingAsync(Token, 10000);
}
// DeadlineExpired / QuantitySatisfied: leave the queue open; a later iteration supplies items.
```

**Layer 5 — `QfcFormController` seams and the corrected loop,
`QuickFiler/Controllers/QfcFormController.Actions.cs` (owned).** Both properties are declared in this
partial file, which is legal because `QfcFormController` is a partial class; no edit to the non-owned
`QfcFormController.cs` (where `_undoQueue` and `_undoConsumerTask` are declared) is required.

```csharp
/// <summary>
/// Injectable time/delay seam (issue #448). Defaults to TimeProvider.System so production timing is
/// unchanged; tests assign a FakeTimeProvider to drive the idle-exit path with no wall-clock wait.
/// Mirrors QfcDatamodel.TimeProvider and QfcHomeController.TimeProvider.
/// </summary>
internal TimeProvider TimeProvider { get; set; } = TimeProvider.System;

/// <summary>
/// Injectable consumer-start seam (issue #448). Tests assign `body => body()` to run the loop inline.
/// </summary>
internal Func<Func<Task>, Task> UndoConsumerStarter { get; set; } = body => Task.Run(body);

// Call site at :211 becomes:
_undoConsumerTask ??= UndoConsumerStarter(UndoConsumer);

internal async Task UndoConsumer()
{
    try
    {
        long start = TimeProvider.GetTimestamp();
        while (!_undoQueue.IsCompleted)
        {
            if (_undoQueue.TryTake(out var item))
            {
                ... existing take-branch body, unchanged ...
                start = TimeProvider.GetTimestamp();   // reset the IDLE timer on every take
            }
            else if (TimeProvider.GetElapsedTime(start) > UndoConsumerIdleTimeout)
            {
                break;
            }
            else
            {
                await TimeProvider.Delay(TimeSpan.FromMilliseconds(200)).ConfigureAwait(false);
            }
        }
    }
    finally
    {
        _undoConsumerTask = null;   // unconditional, so a later UndoDialog() starts a fresh consumer
    }
}
```

`UndoConsumerIdleTimeout` is a private constant of ten seconds, preserving the current threshold.
`TimeProvider` is chosen over a separate `IClock` plus `Func<TimeSpan, Task>` delay delegate because it is
the repo-wide convention and is already proven in this exact subsystem
(`QfcStreamingDequeueConfidenceGate.cs:104`, `:115`, `:132-134`; `QfcDatamodel.QueueProcessing.cs:173`;
`QfcHomeController.Metrics.cs:222`), and because both packages are already referenced
(`QuickFiler.Test/packages.config:18` and `:85`).

### Boundaries and invariants to preserve

- Every drop-on-reject assertion of `QfcStreamingDequeueConfidenceGateTests.cs:298-310` stays true and the
  test body is not edited.
- Exactly one `_marshalToSta` invocation per public `EmailMoveMonitor` operation. `UnhookItem`
  self-marshals at `EmailMoveMonitor.cs:72`, so the gate calling it from any thread is exactly the
  documented calling convention.
- The accepted-path behaviour pinned by `QfcQueuePurePathsTests.cs:119-133` (`UnhookItem` exactly once per
  accepted item) is unchanged; rejected items add one marshal hop each.
- The overload-selection discipline pinned by `QfcHomeControllerIssue218Tests.cs:137-259` and
  `QfcHomeControllerRunAsyncHighConfidenceTests.cs:246`/`:277` is untouched under Scope 427-A, because
  `QfcHomeController.RunAsync` still calls the plain `LoadItemsAsync(IList<MailItem>)`.
- The `CompleteAddingAsync` timeout-throws behaviour pinned by
  `QfcQueueCoverageExpansionTests.cs:178-190` is unchanged.
- `QuickFiler/Controllers/QfcDatamodel.cs` must not exceed 500 lines. It is at **496**, so net growth there
  is at most **4 lines** (decision D3). If a change would exceed it, relocate code into
  `QfcDatamodel.QueueProcessing.cs` (177 lines, 323 of headroom). No new type goes into `QfcDatamodel.cs`.

### Dependencies or blocked work

- No dependency on any sibling epic child. Every production change is inside the owned file set
  (research §10 confirms zero non-owned production files for all four scopes as specified here).
- **Landing order (decision D6).** #426, #446 and #427-A land as **one change set**, because each of them
  separately invalidates the same reflective `CreateGate` helper at
  `QfcStreamingDequeueConfidenceGateTests.cs:26-156`. Do not interleave them across phases with
  intervening green gates: each intermediate state re-breaks the same helper. #448 is independent of the
  gate and may land separately.

### Implementation strategy (what changes, not sequencing)

#### Files/modules to change

Production (all owned): `QfcStreamingDequeueConfidenceGate.cs`, `QfcDatamodel.QueueProcessing.cs`,
`QfcDatamodel.cs`, `IQfcDatamodel.cs`, `QfcHomeController.Iteration.cs`, `QfcFormController.Actions.cs`.
`QfcItemController.FolderHandling.cs` and `EmailMoveMonitor.cs` are owned but expected untouched.

Test (all existing, no `.csproj` edit): `QfcStreamingDequeueConfidenceGateTests.cs` (helper migration plus
#426 gate tests), `QfcStreamingDequeueConfidenceGateTests.Part3.cs` (#446 and #427-A gate tests),
`QfcHomeControllerIterationTests.cs` (dedup plus #446 caller tests), `QfcFormControllerSeamTests.cs`
(#448), `QfcQueuePurePathsTests.cs` (#426 datamodel wiring), `QfcDatamodelTests.cs` (#427-A tuple return).
`QfcFormControllerTests.cs` may be touched **only** to replace the tautological `UndoConsumer` placeholder
at `:687-701` with a shorter or equal body, retiring the `MSTEST0032` suppression at `:698-700`; that file
must not grow.

#### Functions/classes/CLI commands impacted

`QfcStreamingDequeueConfidenceGate` (both constructors, `DequeueAsync`, the accept decision at `:144-147`);
`QfcDatamodel.ScoreRemainingQueueMailItemAsync`; `QfcDatamodel.DequeueWithHighConfidenceGateAsync`; a new
private `TryReleaseRejectedHook`; `IQfcDatamodel`; `QfcHomeController.IterateQueueAsync`;
`QfcFormController.UndoConsumer` and `QfcFormController.UndoDialog`. New types: `QfcDequeueStop`,
`QfcDequeueBatch`, `QfcGateBatch`. No CLI surface exists in this subsystem.

#### Data flow and validation changes

The gate now emits `(accepted-with-top-folder, stop-reason, scanned)` instead of a bare accepted list. The
datamodel projects that into `QfcDequeueBatch` for the new interface member and continues to project it to
`IList<MailItem>` for the three legacy overloads. `IterateQueueAsync` validates the stop reason before
closing the queue. Rejected candidates gain a release step that was previously absent.

#### Error handling and logging updates

- The `onRejected` invocation is wrapped in a `try`/`catch` that logs at error level and continues the
  scan, so a monitor failure cannot abort a batch.
- `UndoConsumer`'s reset is moved into a `finally` so it runs on the exception path too.
- Existing `logger.Debug` output is preserved: `LogDeadlineExpiry`
  (`QfcStreamingDequeueConfidenceGate.cs:157-165`), `LogScore` (`:167-175`, invoked at `:142`), and the two `Probability debug`
  lines. No log message text is changed, so no log-parsing consumer is affected.

#### Rollback/feature-flag considerations

No feature flag is added. The change is behavioural and small; rollback is a revert of the change set. The
three legacy `IQfcDatamodel` overloads keep their current behaviour, so a partial revert of only
`QfcHomeController.Iteration.cs` restores the pre-change caller behaviour without touching the datamodel.

### Technical specifications (interfaces/contracts)

#### Inputs/outputs and formats

- `DequeueNextItemGroupWithOutcomeAsync(int quantity, int timeOut, TimeSpan firstBatchDeadline, Action<int,int,int> progress)`
  returns `Task<QfcDequeueBatch>`. `Items` is never null (empty list when nothing was accepted).
  `PreScored` is never null (empty list outside high-confidence mode). `Stop` is always one of the three
  enum members.
- `QfcGateBatch.Accepted` is never null. `Scanned` is the number of candidates scored during the call.

#### Required configuration keys and defaults

None added. Existing behaviour retained: `QfcStreamingDequeueConfidenceGate.DefaultFirstBatchDeadline`
= 12 seconds (`:22`); `Timeout.InfiniteTimeSpan` disables the deadline (`:77`);
`QfSettings.HighConfidenceThreshold` selects the cutoff; the `UndoConsumer` idle threshold stays at ten
seconds and the idle poll interval at 200 ms.

#### Backward-compatibility expectations

- The change is **additive** at every public boundary. No existing `IQfcDatamodel` member changes name,
  arity, parameter types or return type.
- `Mock<IQfcDatamodel>` picks the new member up automatically and returns `default(QfcDequeueBatch)`
  unless a test sets it up, so no existing Moq setup breaks by arity mismatch.
- `QfcStreamingDequeueConfidenceGate` is `internal`; its constructor arity and `DequeueAsync` return type
  do change, which is source-breaking only for `QuickFiler.Test` (reachable via
  `[assembly: InternalsVisibleTo("QuickFiler.Test")]` at `QuickFiler/Properties/AssemblyInfo.cs:5`). That
  breakage is handled by the `CreateGate` helper migration and is intended to be loud.

#### Performance constraints

- No new I/O, no new COM traffic on the accepted path. The #426 fix adds exactly one STA marshal hop per
  **rejected** candidate, which replaces an unbounded session-scoped retention with a bounded per-item
  cost.
- The #448 fix removes a CPU-bound spin, so it can only reduce CPU consumption.
- The #446 fix does not change how long a scan runs; it changes only how an empty result is interpreted.
- #427-A carries one extra `string` per accepted item; the allocation is negligible relative to the COM
  work already performed.

---

## Assumptions, Constraints, Dependencies

**Assumptions**

- Branch HEAD, which carries PR #610 (`507a40a5`), is the base; every citation in this document was verified against it.
- No sibling epic child modifies the eight owned files during this work.
- `QfcRemainingQueueAdmission.scoreLoader` remains dead (declared, null-checked, never invoked). Verified
  at `QfcRemainingQueueAdmission.cs:17`, `:23-26`, `:34-46`.

**Constraints**

- `.claude/rules/general-code-change.md` 500-line cap applies to every production and test file touched.
  `QfcDatamodel.cs` is at 496 (4 lines of headroom, decision D3);
  `QfcHomeControllerIterationTests.cs` is at 464 (36 lines, decision D4);
  `QfcStreamingDequeueConfidenceGateTests.Part2.cs` is at 460 and
  `QfcHomeControllerRunAsyncHighConfidenceTests.cs` at 473 — neither is to be grown;
  `QfcFormControllerTests.cs` is at 827, already over the cap, and must only shrink if touched at all.
- `net481` has no `IsExternalInit`, so `record`, `record struct` and `init` accessors are unavailable. All
  new carrier types are plain `readonly struct` with a constructor and get-only properties.
- No new production file and no new test file, therefore no `.csproj` edit (decisions D2 and D4).
- Bugfix Workflow in `CLAUDE.md`: a failing regression test comes first for every defect.
- Tests use MSTest, Moq and FluentAssertions. No live Outlook COM, no temporary files, no wall-clock waits.

**Dependencies**

- `Microsoft.Bcl.TimeProvider 10.0.11` and `Microsoft.Extensions.TimeProvider.Testing`, already referenced
  at `QuickFiler.Test/packages.config:18` and `:85`. No new package is added.
- `[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]` at
  `QuickFiler/Controllers/QfcHighConfidencePreFilter.cs:11` lets Moq proxy internal interfaces such as
  `IEmailMoveMonitor`. Already present.

---

## Data / API / Config Impact

- **User-facing changes:** QuickFiler continues supplying batches after a bounded scan instead of ending
  the session; the undo consumer stops after the idle threshold instead of spinning; no UI text, layout or
  setting changes.
- **API changes:** one additive `IQfcDatamodel` member plus two new public types in
  `QuickFiler.Interfaces`. `QuickFiler` is not a published package, so there is no external consumer.
- **Data or migration considerations:** none. No persisted format, schema or settings file changes.
- **Logging/telemetry:** no message text changes. The new rejection-release failure path adds one
  error-level message; it fires only when the move monitor throws.
- **Compatibility notes:** no CLI flags, no config schema, no versioning impact.

---

## Test Strategy

Framework: MSTest (`Microsoft.VisualStudio.TestTools.UnitTesting`) with Moq and FluentAssertions, per the
C# Unit Test Policy. `MailItem` and `Folder` are mocked as in `QfcStreamingDequeueConfidenceGateTests.cs:18-24`
and `EmailMoveMonitorTests.cs:72-85`.

### Failing-first regression tests (one per defect), with target files from research §6.1

| Defect | Regression test(s) | Target file (lines / headroom) |
| --- | --- | --- |
| #446 gate | `DequeueAsync_DeadlineExpiresWithZeroAccepted_ReportsDeadlineExpiredStop`; `DequeueAsync_SourceDrained_ReportsSourceExhaustedStop` | `QfcStreamingDequeueConfidenceGateTests.Part3.cs` (152 / 348) |
| #446 caller | `IterateQueueAsync_EmptyBatchWithDeadlineExpired_DoesNotCompleteAdding`; `IterateQueueAsync_EmptyBatchWithSourceExhausted_CompletesAddingOnce` | `QfcHomeControllerIterationTests.cs` (464 / 36 — dedup first) |
| #448 | `UndoConsumer_EveryIdleIteration_InvokesTimeProviderDelay` (the failing-first shape assertion); `UndoConsumer_IdleBeyondThreshold_Completes`; `UndoConsumer_SuccessfulTake_ResetsIdleTimer`; `UndoConsumer_OnExit_ResetsUndoConsumerTask` | `QfcFormControllerSeamTests.cs` (378 / 122) |
| #426 gate | `DequeueAsync_BelowThresholdCandidate_InvokesOnRejectedOnce`; `DequeueAsync_AcceptedCandidate_DoesNotInvokeOnRejected`; `DequeueAsync_OnRejectedThrows_ScanContinues` | `QfcStreamingDequeueConfidenceGateTests.cs` (424 / 76) |
| #426 datamodel | `DequeueNextItemGroupAsync_HighConfidenceRejectedItem_UnhooksFromMoveMonitor` | `QfcQueuePurePathsTests.cs` (136 / 364) |
| #427-A | `ScoreRemainingQueueMailItemAsync_ReturnsScoreAndTopFolder`; `DequeueAsync_AcceptedCandidate_CarriesTopFolderInPreScoredResult` | `QfcDatamodelTests.cs` (317 / 183) and `...GateTests.Part3.cs` |

**No new test file is required, therefore `QuickFiler.Test/QuickFiler.Test.csproj` is not edited.**

### RED-state shape (decision D5)

- **#448 must not fail by hanging.** A hanging RED test is not a usable gate. The failing-first assertion
  is on **shape**: the delay seam is invoked on every idle iteration, which is false today because the
  threshold branch at `Actions.cs:279-282` reaches no `await`. The RED state is an assertion failure, not
  a timeout. `[Timeout(...)]` is not the primary mechanism.
- **#426 must not fail by compile error.** The `onRejected` seam and its test land in one task, and the
  `else` clause that invokes it lands in the next, so the RED state is an assertion failure against a
  compiling tree.
- **#446** fails today because the current contract has no `Stop` and the empty branch is unconditional.
- **#427-A** fails today because `QfcDatamodel.cs:376` is `return score.Score;`.

### Determinism (mandatory)

Every new test uses `FakeTimeProvider` from `Microsoft.Extensions.Time.Testing`, following the established
patterns at `QfcStreamingDequeueConfidenceGateTests.Part2.cs:36-69` (low-yield gate whose score loader
advances the fake clock per candidate) and `QfcDatamodelTests.cs:231-241`
(`CreateUninitializedDatamodel` + `SetPrivateField` + `model.TimeProvider = fake`). **No `Thread.Sleep`,
no `Task.Delay`, no `DateTime.Now`/`DateTime.UtcNow`, no real wall-clock wait, no temporary file, and no
live Outlook COM appears in any test added or modified by this work.** `Mock<IEmailMoveMonitor>` is
injected by reflection using the pattern at `QfcQueuePurePathsTests.cs:119-125`.

### Existing tests that must be updated (and how)

- `QfcStreamingDequeueConfidenceGateTests.cs:26-156` — `CreateGate` must be migrated to the new
  constructor and made **fail-closed**: replace the four-step descending `GetConstructor` fallback chain
  with a single exact lookup guarded by `Should().NotBeNull()`. `DequeueAsync` at `:192-205` must cast to
  the new return type.
- `QfcHomeControllerIterationTests.cs` — the four `IterateQueueAsync_*` setups at `:84`, `:130`, `:201`,
  `:268` (and `:372` for `Iterate`) must be retargeted to the new member.
  `IterateQueueAsync_QueueEmpty` (`:123-182`) becomes two tests, one per stop reason. Before absorbing
  them, the file must be deduplicated by extracting a shared `ArrangeIterate(...)` helper from the four
  setups (research §6.1 mitigation 1), which frees roughly 80-100 lines and keeps the file under the cap.
- `QfcFormControllerTests.cs:687-701` — the tautological `UndoConsumer_ShouldConsumeUndoQueue` placeholder
  may be replaced by a shorter or equal body, retiring the `MSTEST0032` suppression. Optional; the file
  must not grow.

### Tests that must not be weakened

`QfcStreamingDequeueConfidenceGateTests.cs:298-310` (drop-on-reject);
`QfcFormControllerSeamTests.cs:352-374` (source-text overload order);
`QfcHomeControllerIssue218Tests.cs:137-259` and `QfcHomeControllerRunAsyncHighConfidenceTests.cs:246`/`:277`
(overload selection); `EmailMoveMonitorTests.cs:134-145` and `:176-198` (marshal accounting);
`QfcQueueCoverageExpansionTests.cs:178-190` (`CompleteAddingAsync` timeout throws);
`QfcQueuePurePathsTests.cs:119-133` (accepted-path unhook exactly once).

### Scenario completeness

- **Positive:** the fixed behaviour for each defect.
- **Negative:** a `SourceExhausted` empty batch still closes the queue; an accepted item is still not
  unhooked twice; a take resets the idle timer rather than extending the exit.
- **Boundary:** deadline exactly at the bound (already pinned,
  `...GateTests.Part2.cs:124-143`); score exactly at cutoff (already pinned,
  `QfcStreamingDequeueConfidenceGateTests.cs:265-278`); zero-quantity dequeue.
- **Error handling:** a throwing `onRejected` callback must not abort the scan; `CompleteAddingAsync`
  timeout still throws.
- **State transition:** `_undoConsumerTask` null -> task -> null across two `UndoDialog()` calls.

### Coverage impact and targets

`QfcDatamodel` is `[ExcludeFromCodeCoverage]` at `QuickFiler/Controllers/QfcDatamodel.cs:25` — a
type-level attribute on one partial declaration, so it covers `QfcDatamodel.QueueProcessing.cs` as well —
and `FolderScoringService` is excluded at `QuickFiler/Controllers/QfcHighConfidencePreFilter.cs:166`.
**Coverage credit for this work therefore accrues only to `QfcStreamingDequeueConfidenceGate`,
`QfcFormController` and `QfcHomeController`.** The coverage comparison must not expect the datamodel edits
to move the number, and a flat datamodel figure is not evidence of missing tests.

No merge-base coverage baseline exists for this feature folder, so the repository-wide figure is a
**record-and-report** obligation, not a blocking gate. The blocking coverage conditions are change-scoped:
no regression on changed lines, and `>= 90%` line coverage on the three non-excluded types named above.

### Toolchain commands (run in this exact order; restart from step 1 on any failure or auto-fix)

1. `dotnet tool run csharpier format .` (verify with `dotnet tool run csharpier check .`)
2. `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
3. `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true`
4. `vstest.console.exe <test-assembly-paths> /EnableCodeCoverage`

Use `/t:Rebuild`, never `/t:Build` — MSBuild's up-to-date check does not invalidate on a command-line
`/p:` change, so a warm `/t:Build` returns exit 0 with `CoreCompile` skipped and the gate cannot fail; a
compile that carries no `/p:` gate property is not a toolchain gate and may use `/t:Build`.
**Never add `/p:Nullable=enable`**; it is not in CI's command and it conscripts files that never adopted
the pragma.

### Manual validation steps

Optional and non-blocking (no live-COM automation is permitted in tests): launch QuickFiler in
high-confidence mode against a low-yield folder, confirm later batches still arrive after a bounded scan,
confirm the folder-combo preselection is unchanged, open the undo dialog and confirm CPU returns to idle
after roughly ten seconds.

---

## Acceptance Criteria

Each criterion names the artifact, test or command that verifies it. `<mb>` denotes the merge-base commit
of this branch against its integration branch.

**Regression tests (failing-first)**

- [x] AC1 — #446 gate: `DequeueAsync_DeadlineExpiresWithZeroAccepted_ReportsDeadlineExpiredStop` and `DequeueAsync_SourceDrained_ReportsSourceExhaustedStop` exist in `QuickFiler.Test/Controllers/QfcStreamingDequeueConfidenceGateTests.Part3.cs`, are driven by `FakeTimeProvider`, fail against the pre-fix gate and pass after.
- [x] AC2 — #446 caller: `IterateQueueAsync_EmptyBatchWithDeadlineExpired_DoesNotCompleteAdding` asserts `IQfcQueue.CompleteAddingAsync` was invoked `Times.Never`, and `IterateQueueAsync_EmptyBatchWithSourceExhausted_CompletesAddingOnce` asserts `Times.Once`. Both live in `QuickFiler.Test/Controllers/QfcHomeControllerIterationTests.cs`.
- [x] AC3 — #448: `UndoConsumer_EveryIdleIteration_InvokesTimeProviderDelay` is present in `QuickFiler.Test/Controllers/QfcFormControllerSeamTests.cs` and its pre-fix failure is an **assertion failure, not a hang or a test-host timeout**. Verified by running that single test against the pre-fix tree and recording the failure message.
- [x] AC4 — #426 gate: `DequeueAsync_BelowThresholdCandidate_InvokesOnRejectedOnce` is present in `QuickFiler.Test/Controllers/QfcStreamingDequeueConfidenceGateTests.cs`, and its pre-fix state is an **assertion failure against a compiling tree** (the seam and the test land in one task, the `else` invocation in the next), not a compile error.
- [x] AC5 — #427-A: `ScoreRemainingQueueMailItemAsync_ReturnsScoreAndTopFolder` is present in `QuickFiler.Test/Controllers/QfcDatamodelTests.cs` and fails against the current `return score.Score;` at `QuickFiler/Controllers/QfcDatamodel.cs:376`.

**Fixed behaviour**

- [x] AC6 — `QuickFiler/Controllers/QfcHomeController.Iteration.cs` calls `CompleteAddingAsync` only inside a branch guarded by `Stop == QfcDequeueStop.SourceExhausted`. Verified by AC2 plus reading the diff of that file.
- [x] AC7 — `QuickFiler/Controllers/QfcStreamingDequeueConfidenceGate.cs` returns a stop reason from all four exits, mapped `:100`/`:154` -> `QuantitySatisfied`, `:119` -> `DeadlineExpired`, `:128` -> `SourceExhausted`. Verified by AC1.
- [x] AC8 — `UndoConsumer` terminates once the queue is drained and the idle threshold elapses. Verified by `UndoConsumer_IdleBeyondThreshold_Completes` completing without a `[Timeout]` trip on a `FakeTimeProvider`.
- [x] AC9 — the `UndoConsumer` idle timer is **reset after every successful take**. Verified by `UndoConsumer_SuccessfulTake_ResetsIdleTimer`, which advances the fake clock past the threshold in aggregate while keeping every idle gap below it and asserts the loop kept draining.
- [x] AC10 — `_undoConsumerTask` is reset to `null` on every exit path, including the exception path, so a subsequent `UndoDialog()` starts a fresh consumer. Verified by `UndoConsumer_OnExit_ResetsUndoConsumerTask` and by the reset sitting in a `finally` block in `QuickFiler/Controllers/QfcFormController.Actions.cs`.
- [x] AC11 — the empty-queue path always yields; no loop branch reaches the loop head without an `await` or a `break`. Verified by AC3 and by reading the rewritten loop.
- [x] AC12 — a below-cutoff candidate causes the datamodel's own `_moveMonitor.UnhookItem` to be invoked exactly once for that item. Verified by `DequeueNextItemGroupAsync_HighConfidenceRejectedItem_UnhooksFromMoveMonitor` in `QuickFiler.Test/Controllers/QfcQueuePurePathsTests.cs`.
- [x] AC13 — a throwing rejection callback does not abort the scan. Verified by `DequeueAsync_OnRejectedThrows_ScanContinues`.
- [x] AC14 — the drop-on-reject contract is intact: `DequeueAsync_BelowThresholdItemsAreDiscarded` at `QuickFiler.Test/Controllers/QfcStreamingDequeueConfidenceGateTests.cs:298-310` still passes and its body is byte-unchanged. Verified by `git diff <mb>...HEAD -- "QuickFiler.Test/Controllers/QfcStreamingDequeueConfidenceGateTests.cs"` showing no hunk inside that test method.
- [x] AC15 — the STA thread-affinity contract is preserved: `EmailMoveMonitorTests.cs` `AllComAccess_FlowsThroughInjectedMarshalDelegate` (`:176-198`) and the null-argument no-op test (`:134-145`) still pass unmodified, and `QuickFiler/Helper Classes/EmailMoveMonitor.cs` appears in no diff hunk.
- [x] AC16 — #427-A producer side: an accepted candidate's `TopFolder` survives to the datamodel boundary as `QfcDequeueBatch.PreScored`, and `ScoreRemainingQueueMailItemAsync` returns `(long Score, string TopFolder)`. Verified by AC5 and `DequeueAsync_AcceptedCandidate_CarriesTopFolderInPreScoredResult`.

**Scope containment**

- [x] AC17 — **Issue #427 remains open after this work merges.** No GitHub closing keyword immediately precedes a reference to that issue number in any commit message, in the pull-request body, or in any file in this change set. The keyword set to scan for is `close`, `closes`, `closed`, `fix`, `fixes`, `fixed`, `resolve`, `resolves`, `resolved`, each optionally followed by a colon. The pull-request body carries closing keywords for **#446, #448 and #426 only**. Verified by reading the pull-request body file and `git log <mb>..HEAD --format=%B` before opening the pull request.
- [x] AC18 — no non-owned production file is modified. `git diff --name-only <mb>...HEAD -- "QuickFiler/**/*.cs"` returns only paths from the owned list in Scope & Non-Goals. In particular `QfcHomeController.cs`, `QfcHomeController.Metrics.cs`, `QfcCollectionController.cs`, `QfcItemController.cs`, `QfcItemController.Initialization.cs`, `QfcHighConfidencePreFilter.cs`, `QfcItemGroup.cs`, `QfcQueue.cs`, `QfcRemainingQueueAdmission.cs`, `QfcFormController.cs`, `QfcFormController.EventHandlers.cs` and `QfcFormController.SetupDisposal.cs` are absent from the output.
- [x] AC19 — no project file is modified. `git diff --name-only <mb>...HEAD -- "*.csproj" "*.props" "*.targets" "packages.config"` returns an empty result.
- [x] AC20 — the three pre-existing `IQfcDatamodel` overloads are unchanged. `git diff <mb>...HEAD -- "QuickFiler/Interfaces/IQfcDatamodel.cs"` contains only additions; no line of the existing `DequeueNextItemGroupAsync` or `DequeueNextItemGroup` declarations is removed or altered.
- [x] AC21 — the overload-selection pins are untouched. `git diff --name-only <mb>...HEAD` does not list `QuickFiler.Test/Controllers/QfcHomeControllerIssue218Tests.cs` or `QuickFiler.Test/Controllers/QfcHomeControllerRunAsyncHighConfidenceTests.cs`.
- [x] AC22 — no new file is added under `QuickFiler/` or `QuickFiler.Test/`. `git diff --name-status <mb>...HEAD -- "QuickFiler/" "QuickFiler.Test/"` reports no `A` status entry.

**Quality gates**

- [x] AC23 — the reflective gate helper is **fail-closed**. The descending `GetConstructor` fallback chain at `QuickFiler.Test/Controllers/QfcStreamingDequeueConfidenceGateTests.cs:26-156` is replaced by a single exact lookup guarded by a `Should().NotBeNull()` assertion. Verified by `git grep -c "GetConstructor" -- "QuickFiler.Test/Controllers/QfcStreamingDequeueConfidenceGateTests.cs"` reporting `1`, and by all 29 gate test methods passing (the 23 present on the tree that carries PR #610, plus the six added by `[P1-T2]`, `[P1-T3]`, `[P1-T4]`, `[P1-T9]`, `[P1-T10]` and `[P1-T13]`).
- [x] AC24 — the 500-line cap holds on every file this change touches. For each path in `git diff --name-only <mb>...HEAD -- "*.cs"`, the post-change line count is `<= 500`. The single permitted exception is `QuickFiler.Test/Controllers/QfcFormControllerTests.cs` (pre-existing 827); if it is touched at all its post-change line count must be strictly less than 827. `QuickFiler/Controllers/QfcDatamodel.cs` must be `<= 500` (its pre-change count is 496).
- [x] AC25 — the source-text signature-order test survives formatting: `LoadItemsAsync_MailItemPath_DoesNotApplyPostDisplayHighConfidenceRemoval` (`QuickFiler.Test/Controllers/QfcFormControllerSeamTests.cs:352-374`) passes **after** `dotnet tool run csharpier format .` has run, not before.
- [x] AC26 — determinism: no test file added or modified by this change set contains `Thread.Sleep`, `Task.Delay`, `DateTime.Now`, `DateTime.UtcNow`, `Path.GetTempPath` or `Path.GetTempFileName`. Verified by grepping the changed test paths from `git diff --name-only <mb>...HEAD -- "QuickFiler.Test/**/*.cs"`.
- [x] AC27 — full four-step C# toolchain green in a single final pass: `dotnet tool run csharpier check .` clean; both `msbuild TaskMaster.sln /t:Rebuild ...` commands (analyzer and `TreatWarningsAsErrors`) exit 0 with zero errors and zero warnings-as-errors; `vstest.console.exe <assemblies> /EnableCodeCoverage` reports zero failed tests. Neither msbuild invocation uses `/t:Build` and neither adds `/p:Nullable=enable`. The exact commands run and their results are recorded in the feature's evidence folder.
- [ ] AC28 — coverage: no regression on changed lines, and `>= 90%` line coverage on `QfcStreamingDequeueConfidenceGate`, `QfcFormController` and `QfcHomeController` (the three non-excluded types this work touches). The repository-wide figure is recorded and reported from the `/EnableCodeCoverage` run but is not a blocking threshold for this change, because no merge-base baseline exists for this feature folder and `QfcDatamodel` is `[ExcludeFromCodeCoverage]`.

---

## Risks & Mitigations

| # | Risk | Likelihood / impact | Mitigation |
| --- | --- | --- | --- |
| R1 | **The reflective `CreateGate` helper fails open.** Adding a ninth constructor parameter makes the 8-, 7- and 6-type lookups all return `null`; the chain succeeds on the 5-type lookup and silently builds a gate with `sourceActive = null`, the default deadline and no progress callback. Twenty-three tests would then pass or fail for the wrong reason. | High / high — silent, and it defeats the very tests that gate this change | Migrate the helper in the same task that changes the constructor, and trim it to a **single exact lookup** with `Should().NotBeNull()` so it fails closed (AC23). The `DequeueAsync` return-type cast at `:192-205` already fails loudly with `InvalidCastException`. |
| R2 | **CSharpier reflow breaks the source-text test.** `QfcFormControllerSeamTests.cs:352-374` reads `QfcFormController.Actions.cs` from disk and requires two exact single-line signature literals in order. CSharpier reflows a signature when it exceeds the print width; both are currently within it. | Medium / medium | Do not add parameters to either `LoadItemsAsync` signature and do not reorder the overloads (Scope 427-A requires neither). Run the format step **before** judging that test green (AC25). |
| R3 | **`QfcDatamodel.cs` is at 496 of the 500-line cap.** A four-line overrun is easy to produce. | Medium / medium | Decision D3: net growth there is at most 4 lines; no new type goes into that file. Relocate anything larger into `QfcDatamodel.QueueProcessing.cs` (177 / 323 headroom). Verified by AC24. |
| R4 | **`QfcHomeControllerIterationTests.cs` is at 464/500** and must absorb two new tests plus four retargeted setups. | High / medium | Deduplicate first by extracting a shared `ArrangeIterate(...)` helper from the four `IterateQueueAsync_*` setups (research §6.1 mitigation 1), which frees roughly 80-100 lines. Do not add a `Part2` partial, which would require a `.csproj` edit (AC19, AC22). |
| R5 | **`QfcFormControllerTests.cs` is already 827 lines**, 327 over the cap. | Medium / medium | Place all #448 tests in `QfcFormControllerSeamTests.cs` (378 / 122 headroom), which exists explicitly so that file is not grown further. Touch `QfcFormControllerTests.cs` only to shrink it (AC24). |
| R6 | **Fixing the loop condition without resetting the idle timer converts a hang into a premature exit**, stopping a productive undo session at ten seconds of total runtime. | Medium / high — a silent behaviour regression | The timer reset on every successful take is a first-class requirement (AC9) with its own regression test, not an incidental detail. |
| R7 | **Landing #426, #446 and #427-A separately re-breaks the same helper at each intermediate state**, producing repeated churn and repeated fail-open exposure. | High / medium | Decision D6: land the three as one change set with one helper migration. #448 is independent and may land separately. |
| R8 | **Moq loose mocks turn a retargeted call into a `NullReferenceException` instead of a compile error.** `Mock<IQfcDatamodel>` returns `default(QfcDequeueBatch)` for the new member unless set up, and `QfcDequeueBatch.Items` would then be `null`. | Medium / medium | Make `QfcDequeueBatch` defensive: `Items` and `PreScored` return an empty list when the backing field is null, so a defaulted struct is inert rather than a null-reference trap. Retarget all five identified setups explicitly. |
| R9 | **`Cleanup()` disposes `_undoQueue` while the consumer may be mid-take**, and `SetupDisposal.cs` is not owned. | Low / medium | Address from the owned side only: the unconditional `finally` reset (AC10) means the fault path no longer leaves `_undoConsumerTask` stuck non-null. Recorded as a follow-up rather than fixed here, since the disposal file belongs to another child. |
| R10 | **Scope creep into #427-B.** The full duplicate-scoring fix is tempting once `TopFolder` is flowing. | Medium / high — it would write six sibling-owned files and rewrite five pinned assertions | Decision D1 is recorded in Scope & Non-Goals and enforced by AC18. The consumer half is a follow-up issue, not a stretch goal. |

---

## Rollout & Follow-up

**Release/rollout steps**

1. Land the change set on the feature branch; the epic integrates it into
   `epic/quickfiler-bug-family-integration`.
2. The pull-request body carries closing keywords for **#446, #448 and #426 only** (AC17).
3. No feature flag, no migration, no configuration change is required. Rollback is a revert of the change
   set; a partial revert of `QfcHomeController.Iteration.cs` alone restores the pre-change caller
   behaviour.

**Post-fix monitoring and clean-up**

- Confirm on the next high-confidence session against a low-yield folder that batches continue to arrive
  after a bounded scan.
- Confirm CPU returns to idle after the undo dialog is closed and the idle threshold elapses.

**Follow-up items to record as issues (not delivered here)**

1. **Issue #427 consumer half.** Thread the initialised `FolderPredictor` / `IFolderSearchHandler` from
   `FolderScoringService.ScoreAsync` through `QfcHighConfidencePreFilter.cs`, `QfcItemGroup.cs`,
   `QfcCollectionController.cs`, `QfcItemController.cs`, `QfcItemController.Initialization.cs` and
   `QfcHomeController.cs`, and rewrite the five pinned overload-selection assertions. Requires explicit
   coordination with the sibling epic children that own those files. Issue #427 stays open to track it.
2. **Three independent `EmailMoveMonitor` instances.** `QfcDatamodel.cs:103`, `QfcQueue.cs:40` and
   `QfcCollectionController.cs:78` each construct their own monitor, so `QfcQueue.cs:76` unhooks against a
   list that can never contain the datamodel's hooks. Worth a consolidation issue.
3. **`QfcFormController.Cleanup()` disposal ordering.** `SetupDisposal.cs:216-220` disposes `_undoQueue`
   and nulls `_globals`/`_groups` without cancelling or awaiting `_undoConsumerTask`.
4. **Dead `scoreLoader` parameter on `QfcRemainingQueueAdmission`** (`:17`, `:23-26`) — declared,
   null-checked, never invoked.
5. **Pre-existing 500-line cap violations:** `QfcQueue.cs` (610), `QfcCollectionController.cs` (2349),
   `QfcFormControllerTests.cs` (827), and `QfcItemController.FolderHandlingTests.cs` at 498.

**Links**

- Issue: https://github.com/drmoisan/TaskMaster/issues/446
- Related issues: #448, #426, #427 (partially addressed; remains open)
- Feature issue record: `docs/features/active/quickfiler-bug-family-446/issue.md`
- Research: `docs/features/active/quickfiler-bug-family-446/research/2026-08-24T09-50-quickfiler-queue-datamodel-defects-research.md`
- Promoted potential documents:
  `docs/features/potential/promoted/2026-08-07-iteratequeueasync-deadline-closes-queue-early.md`,
  `docs/features/potential/promoted/2026-08-07-quickfiler-undoconsumer-nonterminating-loop.md`,
  `docs/features/potential/promoted/2026-08-07-emailmovemonitor-rejected-item-hook-retention.md`,
  `docs/features/potential/promoted/2026-08-07-quickfiler-post-show-duplicate-scoring.md`
