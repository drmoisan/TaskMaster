# P1-T5 — AC-1 Verified by Measurement (Single Reflection Owner)

Timestamp: 2026-09-01T14-08

Command:
```
git grep -n -F '"_dispatcher"' -- 'QuickFiler.Test/*.cs'
git grep -n -F '"_dispatcher"' -- '*.cs'
```
plus an equivalent ripgrep-family recursive search restricted to the same two scopes and to `*.cs` by
that tool's own glob filter (`**/*.cs`). Both counts are over tracked `*.cs` files only, for the
reasons P0-T16 records.

EXIT_CODE: 0

Output Summary:

## Restricted count — beneath `QuickFiler.Test/`

Count: **1** line, under both methods.

Method one:

```
QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixture.cs:136:                "_dispatcher",
```

Method two:

```
QuickFiler.Test\Controllers\QfcItemController.UiThreadDispatcherFixture.cs:136:                "_dispatcher",
```

That single line is in
`QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixture.cs`, which is the file AC-1
names as the intended sole owner. The baseline was 2 lines (P0-T16); the line formerly at
`QuickFiler.Test/Controllers/WpfUiDispatcherTests.cs:43` is gone.

## Tree-wide count — all tracked `*.cs` files

Count: **4** lines, under both methods.

Method one:

```
QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixture.cs:136:                "_dispatcher",
UtilitiesCS.Test/Threading/IdleAsyncQueue_Tests.cs:144:                "_dispatcher",
UtilitiesCS.Test/Threading/ProgressTrackerAsync_Tests.cs:138:                    "_dispatcher",
UtilitiesCS.Test/Threading/ProgressTracker_Tests.cs:422:                "_dispatcher",
```

Method two returned the same four paths and the same four line numbers, in a different order and with
backslash separators.

The other 3 lines are exactly the three `UtilitiesCS.Test/Threading/` paths P0-T16 named, unchanged in
both path and line number:

- `UtilitiesCS.Test/Threading/ProgressTracker_Tests.cs:422`
- `UtilitiesCS.Test/Threading/ProgressTrackerAsync_Tests.cs:138`
- `UtilitiesCS.Test/Threading/IdleAsyncQueue_Tests.cs:144`

The baseline was 5 lines; the count fell by exactly one, and the one that disappeared is the line in
the file this issue changes. AC-1 holds.
