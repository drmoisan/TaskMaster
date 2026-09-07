# P5-T6 — Pending-commit-versus-native-close scoping guard

Timestamp: 2026-09-07T14-28
Task: [P5-T6]
Issue: #796
Channel used: A

File: QuickFiler.Test/Viewers/BreadcrumbPendingOpenCloseTests.cs

## `[TestMethod]` count

Command:

```
pwsh -NoProfile -Command '(Select-String -Path QuickFiler.Test\Viewers\BreadcrumbPendingOpenCloseTests.cs -SimpleMatch "[TestMethod]").Count'
```

EXIT_CODE: 0
Measured count: 6

That is the five pre-existing tests, all kept, plus the one guard this task added,
CloseWhilePendingOpenAndCommitPending_DoesNotCancelSelection. No pre-existing test was renamed,
weakened or deleted.

## Physical line count

Command:

```
pwsh -NoProfile -Command '(Get-Content -LiteralPath QuickFiler.Test\Viewers\BreadcrumbPendingOpenCloseTests.cs).Count'
```

EXIT_CODE: 0

LINE-COUNT-IDIOM: (Get-Content -LiteralPath $_).Count

| Path | Baseline | Measured | Ceiling | Verdict |
|---|---|---|---|---|
| QuickFiler.Test/Viewers/BreadcrumbPendingOpenCloseTests.cs | 380 | 413 | 440 | within |

## The two pinned scoping assertions

Command:

```
pwsh -NoProfile -Command 'Select-String -Path QuickFiler.Test\Viewers\BreadcrumbPendingOpenCloseTests.cs -SimpleMatch "CancelCount.Should()" -Context 0,1'
```

EXIT_CODE: 0

| Line | Text | Status |
|---|---|---|
| 48 | `harness.CancelCount.Should().Be(1);` | unchanged, still `.Be(1)`, still at line 48 |
| 79 | `harness.CancelCount.Should().Be(1);` | unchanged, still `.Be(1)`, still at line 79 |
| 113 | `harness.CancelCount.Should().Be(1);` | unchanged, still `.Be(1)`, still at line 113 |

The two the spec pins, at lines 48 and 79, still read `.Be(1)` at their original positions. The
third, at line 113, is likewise unchanged; it is not named as a scoping guard by the spec but a fix
driving it to zero would be the same design signal, so its state is recorded too.

The new guard was inserted below all three, which is why none of the three moved. The
`FocusAnchorCount` assertions at lines 49, 80 and 114 are likewise unchanged and still read `.Be(1)`
because the harness leaves the may-take-focus predicate at its permissive default, which this item
does not change.

## Why the new guard is the complement of those assertions

The retained assertions run with no commit in flight and still cancel. The new guard runs the same
close path with the commit latch set and does not cancel, while still asserting
`FocusAnchorCount == 1` so the suppression is shown to be confined to the cancel step rather than
skipping the whole completion. Together they show the AC3 suppression is conditional rather than
global.

## Compile check

EXIT_CODE: 0 for the analyzer rebuild recorded at TestResults/796/p5-t6/analyzer-rebuild.log
(gitignored). Build summary: 0 Error(s).

Output Summary: 6 `[TestMethod]` members; 413 physical lines against the 440 ceiling; both pinned
`CancelCount` assertions still read `.Be(1)` at lines 48 and 79.
