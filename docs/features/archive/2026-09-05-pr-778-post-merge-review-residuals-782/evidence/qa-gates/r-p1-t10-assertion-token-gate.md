# [P1-T10] Assertion-token gate — the inversion, before and after in one place

Timestamp: 2026-09-06T01-41

Command:

```powershell
$paths = @('UtilitiesCS.Test\Threading\UiThread_Tests.cs','UtilitiesCS.Test\OutlookObjects\Folder\WpfDispatcherYieldTests.cs')
Select-String -SimpleMatch 'WithMessage(UiThread.DispatcherNotInitializedMessage)' -Path $paths
Select-String -SimpleMatch 'WithMessage("*UiThread.Init()*")' -Path $paths
```

Both searches were run from the worktree root against those two files only, with `-SimpleMatch` so
that the asterisks, parentheses, and dots in the searched literals are matched as ordinary
characters.

EXIT_CODE: 0

Output Summary: the two counts are inverted relative to the [P0-T3] before state.

| Search | Before ([P0-T3]) | After (this task) |
|---|---|---|
| `WithMessage(UiThread.DispatcherNotInitializedMessage)` | 0 | 2 |
| `WithMessage("*UiThread.Init()*")` | 2 | 0 |

AFTER-CONSTANT-MATCHES: 2
AFTER-WILDCARD-MATCHES: 0

### Search 1 — `WithMessage(UiThread.DispatcherNotInitializedMessage)`

```text
UiThread_Tests.cs:144:                    .WithMessage(UiThread.DispatcherNotInitializedMessage);
WpfDispatcherYieldTests.cs:136:                .WithMessage(UiThread.DispatcherNotInitializedMessage);
```

Exactly two matching lines, one in each file, which is the count the task requires.

### Search 2 — `WithMessage("*UiThread.Init()*")`

```text
(no matching lines)
```

Zero matching lines. The wildcard form is gone from both files.

## Why the search is scoped to these two files

The literal `"*UiThread.Init()*"` legitimately survives elsewhere in the tree and this task asserts
over none of it:

- in `spec.md`, until [P2-T2] and [P2-T8] rewrite the three occurrences the [P0-T2] inventory records
  at lines 193, 657, and 661;
- in the reviewer's own artifacts, which the plan's scope boundary places out of scope;
- in `evidence/qa-gates/p1-t9-phase1-tests.md`, a timestamped run record of a Phase 1 run at
  2026-09-05, which is not rewritten to match a later tree.

A repository-wide zero-hit search would therefore fail for reasons unrelated to this remediation, and
would also pass vacuously in this artifact once written, since this artifact quotes the literal
itself.

## Recording both counts rather than one

A zero-hit search alone can pass vacuously — a re-wrapped line, a renamed file, or a mistyped path
all produce zero matches. The positive count of exactly 2 cannot be satisfied without the intended
edit, and the [P0-T3] before state establishes that neither count held before Phase 1. The two
together are what make the inversion an observation rather than an assertion.
