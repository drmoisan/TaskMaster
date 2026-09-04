# P2-T3 — File-size accounting across the five files this plan writes

Timestamp: 2026-09-03T08-37

Command:
```text
env -C <worktree-root> wc -l UtilitiesCS/Threading/UiThread.cs UtilitiesCS.Test/Threading/UiThread_Tests.cs UtilitiesCS.Test/Threading/IdleAsyncQueue_Tests.cs UtilitiesCS.Test/Threading/ProgressTrackerAsync_Tests.cs UtilitiesCS.Test/Threading/ProgressTracker_Tests.cs
```

This is character-for-character the command P0-T13 ran (apart from the working-directory prefix), so
the before and after counts come from one counting idiom.

EXIT_CODE: 0

## Output Summary

```text
  172 UtilitiesCS/Threading/UiThread.cs
  179 UtilitiesCS.Test/Threading/UiThread_Tests.cs
  348 UtilitiesCS.Test/Threading/IdleAsyncQueue_Tests.cs
  206 UtilitiesCS.Test/Threading/ProgressTrackerAsync_Tests.cs
  514 UtilitiesCS.Test/Threading/ProgressTracker_Tests.cs
 1419 total
```

The trailing `total` row is ignored.

| File | P0-T13 baseline | Post-change | Delta | Clause | Result |
|---|---|---|---|---|---|
| `UtilitiesCS/Threading/UiThread.cs` | 163 | **172** | +9 | 1 (< 500) | PASS |
| `UtilitiesCS.Test/Threading/UiThread_Tests.cs` | 104 | **179** | +75 | 1 (< 500) | PASS |
| `UtilitiesCS.Test/Threading/IdleAsyncQueue_Tests.cs` | 347 | **348** | +1 | 1 (< 500) | PASS |
| `UtilitiesCS.Test/Threading/ProgressTrackerAsync_Tests.cs` | 205 | **206** | +1 | 1 (< 500) | PASS |
| `UtilitiesCS.Test/Threading/ProgressTracker_Tests.cs` | 514 | **514** | 0 | 2 (<= baseline + 1) | PASS |

PRE-EXISTING FILE-SIZE OVERAGE: UtilitiesCS.Test/Threading/ProgressTracker_Tests.cs

That overage exists at BASE `87cb4df338322844abfa580abea14df77e738e5c`, where the file is already
514 lines — above the 500-line limit in `.claude/rules/general-code-change.md` — and it is not
introduced by this change. P1-T5 added the attribute to that file by extending its existing attribute
list on line 14 to `[TestClass, DoNotParallelize]` rather than by adding a line, so the post-change
count is unchanged at 514, comfortably inside the baseline-plus-one tolerance. The tolerance exists
only because P4-T1's `csharpier` pass may split that attribute list onto two lines; P4-T8 re-audits
after the formatter has run.

## Acceptance

Clause 1 satisfied for the four in-scope files (172, 179, 348, and 206, each strictly less than 500).
Clause 2 satisfied for `ProgressTracker_Tests.cs` (514, equal to its baseline of 514 and therefore
less than or equal to baseline plus 1).
