# Phase 0 — Upstream Landing Check (P0-T17)

Timestamp: 2026-08-27T23-32
Command: (git grep -n -E "UnwireEvents|UnwireIntentEvents|UnwireControlTreeEvents|MoveFailureNotifier|TryResolveCidResource|DetachWebResourceRequestedHandler|SyncExpandedRegistrations" -- QuickFiler/ | Measure-Object).Count
EXIT_CODE: 0

MatchCount: 16
Upstream484Landed: true
Upstream444Landed: true

## BOTH UPSTREAMS HAVE LANDED — the planning premise has changed

`spec.md:401` and research section 3.1 both record that this grep returned **zero** matches on
2026-08-25 and conclude that both siblings were "prepared, not executed". On this branch head the same
grep returns **16** matches. Both upstreams are on the branch.

`issue.md:49-50` in the same feature folder already stated the current position — "Both upstreams are
already on the integration branch and their post-change shape is authoritative for planning" — so the
divergence noted in the P0-T5 artifact is resolved in favour of `issue.md`. `spec.md` and the research
document are stale on this single point.

## The 16 matching lines, verbatim

```
QuickFiler/Controllers/QfcItemController.EventWiring.cs:392:        internal void UnwireEvents()
QuickFiler/Controllers/QfcItemController.EventWiring.cs:394:            UnwireControlTreeEvents();
QuickFiler/Controllers/QfcItemController.EventWiring.cs:395:            UnwireIntentEvents();
QuickFiler/Controllers/QfcItemController.EventWiring.cs:396:            DetachWebResourceRequestedHandler();
QuickFiler/Controllers/QfcItemController.EventWiring.cs:399:        internal void UnwireControlTreeEvents()
QuickFiler/Controllers/QfcItemController.EventWiring.cs:445:        internal void UnwireIntentEvents()
QuickFiler/Controllers/QfcItemController.EventWiring.cs:447:            // #481: same intentional asymmetry as UnwireControlTreeEvents() - teardown tolerates a
QuickFiler/Controllers/QfcItemController.MailActions.cs:30:        internal System.Action<string> MoveFailureNotifier { get; set; } =
QuickFiler/Controllers/QfcItemController.MailActions.cs:37:            var notifier = MoveFailureNotifier;
QuickFiler/Controllers/QfcItemController.Navigation.cs:186:        private void SyncExpandedRegistrations(bool expanded)
QuickFiler/Controllers/QfcItemController.Navigation.cs:211:            SyncExpandedRegistrations(_expanded);
QuickFiler/Controllers/QfcItemController.Navigation.cs:228:            SyncExpandedRegistrations(_expanded);
QuickFiler/Controllers/QfcItemController.ViewerSetup.cs:95:                if (!TryResolveCidResource(e.Request.Uri, map, out var payload, out var mimeType))
QuickFiler/Controllers/QfcItemController.ViewerSetup.cs:215:        internal static bool TryResolveCidResource(
QuickFiler/Controllers/QfcItemController.ViewerSetup.cs:458:            UnwireEvents();
QuickFiler/Controllers/QfcItemController.ViewerSetup.cs:486:        private void DetachWebResourceRequestedHandler()
```

## Attribution

**Upstream 484 — landed.** Five of its six named members are present with the exact accessibility and
static-ness its contract table promises: `internal void UnwireEvents()`
(`EventWiring.cs:392`), `internal void UnwireControlTreeEvents()` (`:399`),
`internal void UnwireIntentEvents()` (`:445`),
`internal System.Action<string> MoveFailureNotifier { get; set; }` (`MailActions.cs:30`),
`internal static bool TryResolveCidResource(` (`ViewerSetup.cs:215`) and
`private void DetachWebResourceRequestedHandler()` (`ViewerSetup.cs:486`). The unwire ordering its
contract specifies is visible at `EventWiring.cs:394-396`: `UnwireControlTreeEvents()`, then
`UnwireIntentEvents()`, then `DetachWebResourceRequestedHandler()`. `Cleanup()` calls `UnwireEvents()`
at `ViewerSetup.cs:458`. A source comment at `EventWiring.cs:447` cites issue `#481`, which is one of
the issues 484 closes.

**Upstream 444 — landed.** `private void SyncExpandedRegistrations(bool expanded)` is at
`Navigation.cs:186`, carrying no attribute, exactly as 444's contract requires, and is called from
both toggle overloads at `:211` and `:228`.

## Binding consequence for the rest of this plan

P0-T17's own text states the rule: "If either is `true`, every anchor re-derived in P0-T18 governs and
no line number from this plan or from either upstream table may be used." Both are `true`, so that
rule is now in force without exception.

Every line number printed anywhere in `plan.2026-08-25T01-04.md`, in `spec.md`, in the research
document, or in either upstream contract table for `QfcItemController.EventWiring.cs`,
`.FocusAndTheme.cs`, `.MailActions.cs`, `.FolderHandling.cs`, `.Navigation.cs` and `.ViewerSetup.cs` is
a **pre-upstream** number and is stale. The plan anticipated this contingency: its § Fact base states
that every edit is anchored on a quoted member signature, quoted source text, or quoted project-file
entry, never on a printed line number. P0-T18 records the re-derived anchors.

The measured line-count growth recorded in the P0-T15 artifact is consistent with this finding:
`EventWiring.cs` moved from 391 to 482 lines, `MailActions.cs` from 224 to 257, and
`FocusAndTheme.cs` from 326 to 338.

## Count idiom

The `git grep` is wrapped in `(... | Measure-Object).Count` so the pipeline's own exit code is `0`
whatever the match count. The plan requires that idiom because a bare `git grep` exits `1` on zero
matches, which was the expected result at authoring time and would have recorded `EXIT_CODE: 1` and
normalized this artifact to `fail` even though the gate passed. The idiom is equally correct here,
where the count is non-zero.

Output Summary: The grep returned **16** matches, so `Upstream484Landed: true` and
`Upstream444Landed: true`. **Both upstreams have landed**, reversing the premise recorded in
`spec.md:401` and research section 3.1 that both returned zero matches on 2026-08-25, and confirming
the position already stated in `issue.md:49-50`. All six of 484's named members and 444's
`SyncExpandedRegistrations` are present at the lines quoted above. Every line number this plan, this
spec, the research document, or either upstream table prints for the six sibling-owned
`QfcItemController` partials is therefore stale, and the re-derived anchors recorded by P0-T18 govern
in their place. `EXIT_CODE: 0`, produced by the `Measure-Object` wrapper as the plan requires.
