# P3-T4 — Revert of Mutation M2

Timestamp: 2026-09-17T02-25

Command: edit removing the inserted `action();` line; then `CMD-CENSUS`;
`git diff --exit-code HEAD -- QuickFiler.Test/Viewers/ItemViewerBreadcrumbThreadAffinityTests.cs`;
`git status --porcelain -- QuickFiler.Test/Viewers/ItemViewerBreadcrumbThreadAffinityTests.cs`.

EXIT_CODE: 0

The exit code recorded is that of the anchored `git diff --exit-code HEAD` span.

CHANNEL: COMMAND

## Output Summary

TOKEN action(); = 1

TOKEN ClearViewerDispatcher(scope.Viewer); = 1

GIT_DIFF_EXIT_CODE: 0

PORCELAIN_LINE_COUNT: 0 (the scoped porcelain span printed nothing)

SHA256 = 8EBC19F829536957BEB0DAEC628929CA8DE3F11E10AA819DEA41362C6FCBB164

`LINES` returned to 490 and every other token returned to its "After P2-T1 and P2-T2" value.

## Acceptance

All four conditions hold.

- `action();` is exactly 1 and `ClearViewerDispatcher(scope.Viewer);` is exactly 1. Checking both in
  this task, rather than only the token M2 moved, is what makes this the mechanical proof that
  neither mutation left residue: M1's token is verified here as well as in P3-T2.
- `EXIT_CODE: 0` for the anchored diff against `HEAD`, the P2-T5 fix commit.
- The scoped porcelain span printed nothing.
- `SHA256` equals `FIX-HASH:`,
  `8EBC19F829536957BEB0DAEC628929CA8DE3F11E10AA819DEA41362C6FCBB164`.

Under the mutation the hash was `EF6B6917FB8B63A9DFD3D8C241A99FFECEEA5DA5BAA8CDF40FBD5C820402429F`
and `LINES` was 491, so each of the four checks was observed in its failing state during P3-T3 and
in its passing state here.

The `git checkout --` recovery was not needed.

## Role

This is the mechanical proof AC5 requires that the temporary insertions are gone before the final
pass-after run. The working tree is now byte-identical to the committed fix, so the rebuild in P4-T1
and the measured run in P4-T2 observe the source that will be delivered, not a mutated variant.
