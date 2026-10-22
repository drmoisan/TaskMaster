# Phase 4 Residual Count — All Five Production Files (issue #742, [P4-T5])

Timestamp: 2026-09-14T02-20

Command: `git grep -c '[.]ToString[(]@\?"[^"]*[:/.\-][^"]*"[)]' -- QuickFiler/Controllers/QfcHomeController.Metrics.cs QuickFiler/Controllers/EfcHomeController.Metrics.cs QuickFiler/Controllers/QfcItemController.ViewerSetup.cs QuickFiler/Controllers/QfcCollectionController.cs QuickFiler/Controllers/EfcItemController.cs`

EXIT_CODE: 0

Output Summary:

```
QuickFiler/Controllers/QfcHomeController.Metrics.cs:2
```

The command printed exactly one line, and printed no line for any of the other four paths.

Acceptance: satisfied. The baseline total across all five files was 14 matching lines, distributed
4 / 4 / 1 / 3 / 2, recorded in
`../baseline/discovery-count-controls.2026-09-12T16-09.md` ([P0-T9] control 1). The post-fix total is
2, both in `QfcHomeController.Metrics.cs`.

## What the residual 2 are

The two remaining matches are the commented-out predecessor statements at the top of
`QuickFileMetrics_WRITE`:

```
//var curDateText = DateTime.Now.ToString("MM/dd/yyyy");
//var curTimeText = DateTime.Now.ToString("hh:mm");
```

They are inert comments, carry no runtime behaviour, and are deliberately not edited by this change.
`spec.md`'s residual-sweep acceptance criterion names this figure of 2 and identifies these two
lines as the expected residual.

## Discovery-control note (guard against a vacuous zero)

The pattern used here is the same one that returned the 4 / 4 / 1 / 3 / 2 baseline distribution
against the unfixed tree, and it still returns a non-empty result on this tree (the two lines
counted above). The four zero results are therefore real observations rather than a search that
cannot match. The `@` is written `@\?` because the search runs in basic-regular-expression mode, in
which a bare `?` is a literal character; the unescaped spelling matches nothing in this repository
whatever the executor does.
