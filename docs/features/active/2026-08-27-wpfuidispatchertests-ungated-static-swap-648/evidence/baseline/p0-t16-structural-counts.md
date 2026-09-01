# P0-T16 — Structural Baseline for AC-1 and AC-2

Timestamp: 2026-09-01T13-55

Command:
```
git grep -n -F '"_dispatcher"' -- 'QuickFiler.Test/*.cs'
git grep -n -F '"_dispatcher"' -- '*.cs'
```
plus a second, independent recursive content search using a ripgrep-family tool, restricted to the
same two scopes and to `*.cs` by that tool's own glob filter (`**/*.cs`), plus a token search over
`QuickFiler.Test/Controllers/WpfUiDispatcherTests.cs` alone under both methods.

EXIT_CODE: 0

Output Summary:

Every count below is scoped to tracked `*.cs` files. The two methods agree on every measurement.

## Measurement 1 — quoted literal `"_dispatcher"` beneath `QuickFiler.Test/`

Count: **2** lines, under both methods.

Method one (`git grep -n -F '"_dispatcher"' -- 'QuickFiler.Test/*.cs'`):

```
QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixture.cs:136:                "_dispatcher",
QuickFiler.Test/Controllers/WpfUiDispatcherTests.cs:43:                "_dispatcher",
```

Method two (ripgrep-family recursive search, path scoped to `QuickFiler.Test`, glob `**/*.cs`):

```
QuickFiler.Test\Controllers\QfcItemController.UiThreadDispatcherFixture.cs:136:                "_dispatcher",
QuickFiler.Test\Controllers\WpfUiDispatcherTests.cs:43:                "_dispatcher",
```

The two lines are the shared fixture and the file this issue changes, which is the baseline AC-1
states.

## Measurement 2 — quoted literal `"_dispatcher"` across all tracked `*.cs` files

Count: **5** lines, under both methods.

Method one (`git grep -n -F '"_dispatcher"' -- '*.cs'`):

```
QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixture.cs:136:                "_dispatcher",
QuickFiler.Test/Controllers/WpfUiDispatcherTests.cs:43:                "_dispatcher",
UtilitiesCS.Test/Threading/IdleAsyncQueue_Tests.cs:144:                "_dispatcher",
UtilitiesCS.Test/Threading/ProgressTrackerAsync_Tests.cs:138:                    "_dispatcher",
UtilitiesCS.Test/Threading/ProgressTracker_Tests.cs:422:                "_dispatcher",
```

Method two returned the same five paths and the same five line numbers, in a different order and with
backslash separators.

The 3 of those 5 lying outside `QuickFiler.Test/` are exactly:

- `UtilitiesCS.Test/Threading/ProgressTracker_Tests.cs:422`
- `UtilitiesCS.Test/Threading/ProgressTrackerAsync_Tests.cs:138`
- `UtilitiesCS.Test/Threading/IdleAsyncQueue_Tests.cs:144`

These are the three out-of-scope cross-assembly mutators named under Summary in `issue.md`. No task
in this plan may change them.

## Measurement 3 — reflection tokens in `QuickFiler.Test/Controllers/WpfUiDispatcherTests.cs`

Under both methods, the same four lines matched:

```
1:using System.Reflection;
42:            FieldInfo field = typeof(UiThread).GetField(
51:                field.SetValue(null, dispatcher);
83:                field.SetValue(null, original);
```

- `GetField` — 1 occurrence (`:42`), which is at least once.
- `SetValue` — 2 occurrences (`:51`, `:83`), which is at least once.
- `using System.Reflection;` — 1 occurrence (`:1`), which is at least once.

## Deliberately unmatched occurrence

`QuickFiler.Test/Viewers/BreadcrumbPopupUiOperationsDirectAdapterTests.cs:207` carries an unquoted
lambda parameter also named `_dispatcher`:

```
(_dispatcher, _core, _owner, _navigate, _surfaceName) =>
```

It is unrelated to the static field this issue is about and is deliberately not matched, because the
search token includes the surrounding double quotes. Both methods excluded it, which is the intended
behavior of the token rather than an omission.

## Note on scoping

The `-- '*.cs'` pathspec and the equivalent `**/*.cs` glob are required for these counts to be stable
quantities. Unrestricted, the same literal also appears in Markdown across the repository, including
in this plan, in `issue.md`, in this feature's research artifact, in the #493 feature folder, and in
`.claude/agent-memory/`, so an unrestricted figure would be several times larger than 5 and would
additionally change during this plan's own execution once P2-T12 stages this artifact. Restricting to
tracked `*.cs` also reconciles the two methods: `git grep` reads tracked files only, while a
ripgrep-family search also reads untracked ones.
