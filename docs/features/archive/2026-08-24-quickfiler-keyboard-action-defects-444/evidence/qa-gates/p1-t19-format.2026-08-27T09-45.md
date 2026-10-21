# [P1-T19] Phase 1 formatting

Timestamp: 2026-08-27T09-45

## Mutating pass

Command:

```
dotnet tool run csharpier format QuickFiler\Controllers\KbdActions.cs QuickFiler.Test\Controllers\KbdActionsTests.cs QuickFiler.Test\Controllers\KbdActionsRemainingBranchesTests.cs QuickFiler.Test\Controllers\QfcCollectionControllerNavigationDigitsTests.cs
```

The four paths are named explicitly. A bare dot is never used for the mutating pass, so no file
outside this feature's ownership is rewritten.

Output (verbatim):

```
Formatted 4 files in 1873ms.
```

EXIT_CODE: 0

Note on the `Formatted N files` idiom: CSharpier reports the number of files it **processed**, not the
number it rewrote. The read-only verification below is the authoritative check.

## Read-only verification

Command:

```
dotnet tool run csharpier check QuickFiler\Controllers\KbdActions.cs QuickFiler.Test\Controllers\KbdActionsTests.cs QuickFiler.Test\Controllers\KbdActionsRemainingBranchesTests.cs QuickFiler.Test\Controllers\QfcCollectionControllerNavigationDigitsTests.cs
```

Output (verbatim):

```
Checked 4 files in 1296ms.
```

EXIT_CODE: 0

CSharpier emits one warning line per unformatted file before its summary. No such line was emitted, so
the unformatted-file count is **0**.

## Post-format diff shape

```
91	0	QuickFiler.Test/Controllers/KbdActionsRemainingBranchesTests.cs
37	0	QuickFiler.Test/Controllers/KbdActionsTests.cs
36	0	QuickFiler/Controllers/KbdActions.cs
```

All three changed files are additions only, with zero removed lines.
`QuickFiler.Test/Controllers/QfcCollectionControllerNavigationDigitsTests.cs` is a new untracked file
and therefore absent from `git diff --numstat`.

## Acceptance evaluation

- The `check` invocation reports `EXIT_CODE: 0` and zero unformatted files. PASS.

Output Summary: format pass exit 0 over the four explicitly named owned paths; check pass exit 0 with
zero unformatted files.
