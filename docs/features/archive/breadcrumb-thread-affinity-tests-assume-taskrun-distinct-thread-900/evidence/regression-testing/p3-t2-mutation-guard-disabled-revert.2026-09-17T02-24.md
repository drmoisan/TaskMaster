# P3-T2 — Revert of Mutation M1

Timestamp: 2026-09-17T02-24

Command: edit removing the two inserted lines; then `CMD-CENSUS`;
`git diff --exit-code HEAD -- QuickFiler.Test/Viewers/ItemViewerBreadcrumbThreadAffinityTests.cs`;
`git status --porcelain -- QuickFiler.Test/Viewers/ItemViewerBreadcrumbThreadAffinityTests.cs`.

EXIT_CODE: 0

The exit code recorded is that of the anchored `git diff --exit-code HEAD` span.

CHANNEL: COMMAND

## Output Summary

TOKEN ClearViewerDispatcher(scope.Viewer); = 1

TOKEN action(); = 1

GIT_DIFF_EXIT_CODE: 0

PORCELAIN_LINE_COUNT: 0 (the scoped porcelain span printed nothing)

SHA256 = 8EBC19F829536957BEB0DAEC628929CA8DE3F11E10AA819DEA41362C6FCBB164

`LINES` returned to 490 and every other token returned to its "After P2-T1 and P2-T2" value.

## Acceptance

All four conditions hold.

- The census shows `ClearViewerDispatcher(scope.Viewer);` exactly 1, the single pre-existing
  occurrence in the out-of-scope null-owner test, and `action();` exactly 1, the helper's single
  invocation of the delegate.
- `EXIT_CODE: 0` for the anchored diff against `HEAD`, which is the P2-T5 fix commit.
- The scoped porcelain span printed nothing.
- `SHA256` equals `FIX-HASH:` from P2-T5,
  `8EBC19F829536957BEB0DAEC628929CA8DE3F11E10AA819DEA41362C6FCBB164`.

The four checks are complementary rather than redundant. The census proves the specific mutation
token is gone; the anchored diff proves the tracked content matches the committed fix; the porcelain
span is the diff's companion and would catch a state the anchored diff cannot express; and the hash
is an independent byte-level comparison that does not depend on git's content normalization. Under
the mutation the file's hash was `B79D6C9B5FC0FECB64858DCFDE257F1B835BE23BDC4C5DFBD9579F3F3AF557AF`
and the anchored diff would not have exited 0, so all four checks were observed in their failing
state during P3-T1 and in their passing state here.

The `git checkout --` recovery was not needed: the manual removal left no residual difference, and
the four checks passed on the first attempt.
