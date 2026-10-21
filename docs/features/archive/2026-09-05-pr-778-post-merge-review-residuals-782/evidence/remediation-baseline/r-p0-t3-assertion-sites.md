# [P0-T3] Assertion sites — the before state that [P1-T10] inverts

Timestamp: 2026-09-06T01-28

Command:

```powershell
$paths = @('UtilitiesCS.Test\Threading\UiThread_Tests.cs','UtilitiesCS.Test\OutlookObjects\Folder\WpfDispatcherYieldTests.cs')
Select-String -SimpleMatch 'WithMessage("*UiThread.Init()*")' -Path $paths
Select-String -SimpleMatch 'WithMessage(UiThread.DispatcherNotInitializedMessage)' -Path $paths
```

Both searches were run from the worktree root against those two files only. `-SimpleMatch` is used so
the asterisks and parentheses in the searched literals are matched as ordinary characters rather than
as regular-expression metacharacters.

EXIT_CODE: 0

Output Summary:

BEFORE-WILDCARD-MATCHES: 2
BEFORE-CONSTANT-MATCHES: 0

### Search 1 — `WithMessage("*UiThread.Init()*")`

```text
UiThread_Tests.cs:142:            act.Should().Throw<InvalidOperationException>().WithMessage("*UiThread.Init()*");
WpfDispatcherYieldTests.cs:136:                .WithMessage("*UiThread.Init()*");
```

Two matching lines, one in each file, which is the count the task requires. The match in
`UtilitiesCS.Test/Threading/UiThread_Tests.cs` is inside
`Dispatcher_WhenBackingFieldIsNull_ThrowsInvalidOperationExceptionNamingInitialize`. The match in
`UtilitiesCS.Test/OutlookObjects/Folder/WpfDispatcherYieldTests.cs` is the trailing call of the
chained assertion inside `YieldAsync_WithoutDispatcher_RemainsStrict`.

### Search 2 — `WithMessage(UiThread.DispatcherNotInitializedMessage)`

```text
(no matching lines)
```

Zero matching lines. The constant-reference form is absent from both files before Phase 1.

## Why these two counts are the discriminating before state

[P1-T10] asserts the inverse pair — two matching lines for the constant form and zero for the
wildcard form — over the same two files with the same two searches. Recording both counts here means
the inversion is decided by an observed change rather than by a single search that could pass
vacuously.

The search is scoped to these two files because `"*UiThread.Init()*"` legitimately survives elsewhere
in the tree: in `spec.md` before Phase 2, in the reviewer's own artifacts, and in
`evidence/qa-gates/p1-t9-phase1-tests.md`, none of which this remediation asserts over.
