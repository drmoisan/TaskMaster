# P5-T5 — Row activation listener in FolderBreadcrumb.html

Timestamp: 2026-09-07T14-27
Task: [P5-T5]
Issue: #796
Channel used: A

## Branch taken

AC3-HTML-POINTERDOWN: NOT REQUIRED

NOT APPLICABLE. The row activation listener at QuickFiler/Resources/FolderBreadcrumb.html lines
289-291 is NOT moved from the `click` event to a pointer-down event, and the file is left unchanged
by this task.

Quoted from evidence/other/close-ordering-decision.md:

> AC3-HTML-POINTERDOWN: NOT REQUIRED

and from the derivation recorded beneath that line:

> The value REQUIRED is admissible only when the Gesture C transcript shows that no activation
> message was produced. It does not, and it cannot: the transcript is silent on activation
> altogether, and its silence carries no information.

and:

> The page `QuickFiler/Resources/FolderBreadcrumb.html` is therefore not changed by this item, and
> the sibling contention recorded against it does not need to be exercised.

## Consequences

- The page is untouched, so the recorded sibling contention with the concurrent item that owns the
  row text projection in this same page is not exercised at all.
- The `selectorActivate` post count assertion the REQUIRED branch would have run does not apply,
  because that assertion is stated by the plan only for the REQUIRED branch.

## Verification that the file is unchanged

Command:

```
git status --porcelain QuickFiler/Resources/FolderBreadcrumb.html
```

EXIT_CODE: 0
Output: empty. The file carries no working-tree modification, which is the observable form of the
NOT REQUIRED branch.

## Recorded limitation, carried forward from the decision record

This branch records that the evidence does not support the page change, not that the page change
has been shown unnecessary. If the AC2 seam lands and a row click still fails to select, the
question is reopened, and settling it then requires instrumenting the activation path rather than
re-reading the Phase 2 transcript.

Output Summary: NOT REQUIRED branch taken; FolderBreadcrumb.html unchanged and verified unchanged
by porcelain status.
