# R1 — Leg A carrier reconciliation and the doc-block correction

- Timestamp: 2026-09-02T01-27
- Issue: #678
- Tasks: [P1-T3] (the fix) and [P1-T4] (the `QfcDatamodel.QueueProcessing.cs` doc block)

## The invariant this pins

> The set of mail items displayed on leg A is exactly the set that survived
> `UnhookDequeuedNodes`. No item whose `UnhookItem` call failed may be displayed, and no item
> that `TryUnhookOrReplace` pulled out of the master queue may go undisplayed.

The fix pins the invariant at the boundary that consumes the value. It does not make
`PreScored` and `Items` textually agree, and it does not relax an assertion. Leg B already
avoided the hazard by resolving carriers per row from the item spine; the fix mirrors leg B
by making `batch.Items` the leg A spine too, and generalises leg B's own matching helper so
exactly one EntryID-and-identity matching body exists in the tree.

## The three edited paths, with post-edit Derivation D8 counts

| Path | Before | After | Headroom to 500 |
|---|---|---|---|
| `QuickFiler/Controllers/QfcHighConfidencePreFilter.cs` | 228 | **301** | 199 |
| `QuickFiler/Controllers/QfcQueue.Enqueue.cs` | 216 | **200** | 300 |
| `QuickFiler/Controllers/QfcHomeController.cs` | 465 | **472** | **28** |

`QfcQueue.Enqueue.cs` shrank because the 26-line `ResolveCarriedHandler` body collapsed to a
single expression-bodied delegation. `QfcHomeController.cs` is the binding constraint at 472
lines with 28 lines of headroom; P2-T9 re-measures it after CSharpier reflow.

The fourth edited path, `QuickFiler/Controllers/QfcDatamodel.QueueProcessing.cs` (P1-T4),
grows from 292 to **298** lines: the corrected doc block is six lines longer than the one it
replaces. Headroom to 500 is 202. No executable line in that file changes.

## Members added to `QfcPreScoredItem`

`QfcPreScoredItem.ResolveCarrier` and `QfcPreScoredItem.ReconcileCarriersToItems`, both
`internal static`, inside the `public readonly struct QfcPreScoredItem` declaration in
`QuickFiler/Controllers/QfcHighConfidencePreFilter.cs`.
`QuickFiler/Properties/AssemblyInfo.cs:5` carries
`[assembly: InternalsVisibleTo("QuickFiler.Test")]`, so both are reachable from the test
assembly exactly as the existing `internal static QfcQueue.ResolveCarriedHandler` already is.

They live on `QfcPreScoredItem` rather than on `QfcQueue` for two reasons. First, correctness:
`QfcHomeController` declares an instance property `internal IQfcQueue QfcQueue { get; set; }`
at `QuickFiler/Controllers/QfcHomeController.cs:153`, so inside a `QfcHomeController` member
the simple name `QfcQueue` binds to that property, whose type is `IQfcQueue` and not
`QfcQueue`. `QfcQueue.ReconcileCarriersToItems(...)` would therefore fail to compile.
`QfcPreScoredItem` has no such shadow. Second, cohesion: the carrier type owns carrier-list
reconciliation.

An item with no matching carrier receives `new QfcPreScoredItem(item, null)`, which coerces
`PredeterminedFolder` to `string.Empty` (`QfcHighConfidencePreFilter.cs:130`) and leaves
`FolderHandler` null, so the item controller falls back to its own scoring pass and to
index-1 selection — the pre-#678 behaviour for a row with no carrier. It is not a fabricated
carrier.

## Why reference identity is tried before `EntryID`

The existing passing test `RunAsync_HighConfidenceEnabled_LoadsFirstPageFromStreamingDequeue`
(`QuickFiler.Test/Controllers/QfcHomeControllerRunAsyncHighConfidenceTests.cs:130-258`) builds
its carrier from `new Mock<MailItem>().Object` with no `EntryID` setup, so `EntryID` is null.
A matcher that returned null on an empty `EntryID` before trying reference identity would
strand that item's handler and break the assertion at `:228-240`, which scope constraint 4
forbids. On the happy path the objects are literally the same instances, because
`QfcDatamodel.QueueProcessing.cs:192` builds `nodes` from `accepted.Select(x => x.MailItem)`.

## Before and after — `QfcQueue.Enqueue.cs` doc block 1 (`ResolveCarriedHandler`)

Before:

```csharp
        /// <summary>
        /// Resolves the folder search handler carried for <paramref name="mailItem"/>, or null when
        /// no carrier list was supplied or none of its entries matches. Matching is by
        /// <c>EntryID</c>: a null or empty carrier list, a null mail item, and a mail item absent
        /// from the list all yield null, which is the pre-#678 behaviour for every row.
        /// </summary>
```

After:

```csharp
        /// <summary>
        /// Resolves the folder search handler carried for <paramref name="mailItem"/>, or null when
        /// no carrier list was supplied or none of its entries matches. A carrier is matched first
        /// by reference identity and then by <c>EntryID</c>: a null or empty carrier list, a null
        /// mail item, and a mail item absent from the list all yield null, which is the pre-#678
        /// behaviour for every row. #678 R1a: the matching body itself now lives on
        /// <see cref="QfcPreScoredItem.ResolveCarrier"/>, so exactly one implementation of it
        /// exists in the tree and leg A and leg B cannot drift apart.
        /// </summary>
```

## Before and after — `QfcQueue.Enqueue.cs` doc block 2 (`EnqueueAsync`)

Before, the two lines that stated matching is by `EntryID` alone:

```csharp
        /// Carriers are matched to items by <c>EntryID</c> rather than by position, because
        /// <c>UnhookDequeuedNodes</c> can replace an element of the item list in place.
```

After:

```csharp
        /// Carriers are matched to items first by reference identity and then by <c>EntryID</c>,
        /// rather than by position, because <c>UnhookDequeuedNodes</c> can replace an element of the
        /// item list in place. #678 R1b: identity is tried first because the happy path builds the
        /// item list from the carriers' own mail items, so an item whose <c>EntryID</c> is null or
        /// empty is still matchable.
```

## P1-T4 — before and after, `QfcDatamodel.QueueProcessing.cs:165-170`

Before:

```csharp
        /// <summary>
        /// Issue #446 and Scope 427-A. The high-confidence dequeue with the gate's outcome intact.
        /// <see cref="QfcDequeueBatch.Items"/> is taken from the same accepted set as
        /// <see cref="QfcDequeueBatch.PreScored"/>, after <see cref="UnhookDequeuedNodes"/> has run
        /// over it, so the two collections describe one dequeue rather than two.
        /// </summary>
```

After:

```csharp
        /// <summary>
        /// Issue #446 and Scope 427-A. The high-confidence dequeue with the gate's outcome intact.
        /// <see cref="QfcDequeueBatch.Items"/> is taken from the same accepted set as
        /// <see cref="QfcDequeueBatch.PreScored"/>, after <see cref="UnhookDequeuedNodes"/> has run
        /// over it. #678 R1: that correspondence holds on the happy path only. On the
        /// <c>UnhookItem</c> throw path <see cref="TryUnhookOrReplace"/> (:31-66) removes the failed
        /// item and inserts a substitute pulled from the master queue, so <c>PreScored</c> can name
        /// an item absent from <c>Items</c> and <c>Items</c> can name an item absent from
        /// <c>PreScored</c>. Leg A reconciles the two at the load boundary through
        /// <see cref="QfcPreScoredItem.ReconcileCarriersToItems"/>; leg B already resolves per row
        /// from the item spine.
        /// </summary>
```

### P1-T4 acceptance clauses

| # | Clause | Result |
|---|---|---|
| 1 | `describe one dequeue rather than two` occurs zero times in the file | PASS — 0 occurrences |
| 2 | `#678 R1` occurs exactly once in the file | PASS — 1 occurrence |
| 3 | that token is on a single line | PASS — 1 matching line |
| 4 | the analyzer build exits 0 | PASS — exit 0, `CoreCompile:` 63 |

The corrected block states all three things R1 acceptance clause 3 requires: that the
correspondence holds on the happy path only; that on the `UnhookItem` throw path
`TryUnhookOrReplace` removes the failed item and inserts a substitute so each collection can
name an item the other does not; and that leg A reconciles the two at the load boundary. It
no longer claims an unconditional correspondence. The file measures 298 lines by Derivation
D8 after the edit, 202 short of the 500-line cap.

## P1-T3 acceptance clauses

| # | Clause | Result |
|---|---|---|
| 1 | analyzer build exits 0 | PASS — exit 0, `5 Warning(s)` / `0 Error(s)`, `CoreCompile:` 66 |
| 2 | nullable build exits 0 | PASS — exit 0, zero `CS86`, `CoreCompile:` 59 |
| 3 | `#678 R1` occurs exactly once in `QfcHomeController.cs` | PASS — 1 occurrence, on 1 line |
| 4 | `QfcHomeController.cs` at most 500 lines (D8) | PASS — 472 |
| 5 | `ReferenceEquals` occurs at least once in `QfcHighConfidencePreFilter.cs` | PASS — 1 occurrence |
| 6 | `#678 R1a` occurs exactly once in `QfcQueue.Enqueue.cs`, single line | PASS — 1 occurrence, 1 line |
| 7 | `#678 R1b` occurs exactly once in `QfcQueue.Enqueue.cs`, single line | PASS — 1 occurrence, 1 line |
| 8 | no `[ExcludeFromCodeCoverage]` added or removed in the three edited files | PASS — see below |

Clause 8 evidence. `git diff HEAD -- QuickFiler QuickFiler.Test` piped through a count of
lines carrying `ExcludeFromCodeCoverage` returns **0**, so no such line is added or removed.
Independently, the per-file occurrence counts are unchanged:
`QfcHighConfidencePreFilter.cs` 1 (the pre-existing attribute on `FolderScoringService`),
`QfcQueue.Enqueue.cs` 0, `QfcHomeController.cs` 0.

Clause 7 note on the shared prefix. The token `#678 R1` occurs twice in
`QfcQueue.Enqueue.cs`, but those two occurrences are the prefixes of the single `#678 R1a`
and the single `#678 R1b`. The plan deliberately does not assert a `#678 R1` count in that
file, so the shared prefix creates no confound; clauses 6 and 7 assert the two distinct
four-character-suffixed tokens instead, and each is exactly 1.

## Output Summary

Three production files edited. `QfcPreScoredItem.ResolveCarrier` and
`QfcPreScoredItem.ReconcileCarriersToItems` added; `QfcQueue.ResolveCarriedHandler` rewritten
to delegate to the former without a signature or accessibility change; the leg A assignment
at `QfcHomeController.cs` rewritten to reconcile against `batch.Items`. Both gate builds exit
0. All eight P1-T3 acceptance clauses pass. Post-edit D8 counts 301 / 200 / 472, all under
the 500-line cap.
