# Test reconciliation — Comment synchronisation in the #468 defects file ([P3-T5])

- Issue: #644
- Task: `[P3-T5]`
- Timestamp: 2026-08-29T08-15
- File modified: `QuickFiler.Test/Controllers/QfcCollectionControllerDefects468Tests.cs`

Three comment regions corrected. **No assertion was changed**: the asserted exception types, the
`.Should()` chains, and the counter assertions all stay as they are, and no test outcome changes.

`CLAUDE.md` C#6.3 requires comments to stay synchronised with behaviour, which is what puts these
corrections in scope for this fix.

## Region 1 — XML documentation of `RemoveSpecificControlGroupAsync_ThrowAtFirstStatement_RestoresReentrancyCounter`

The block previously said the `NullReferenceException` arises because
`UnregisterNavigation()` dereferences the null `_itemGroups`. Under the ledger that is false:
`UnregisterNavigation()` iterates an empty ledger and completes. The exception still occurs and
still propagates, but it now originates one statement later, at the `_itemGroups[selection - 1]`
dereference inside `RemoveSpecificControlGroupAsync` (`QfcCollectionController.cs` line 1024,
`bool activeUI = _itemGroups[selection - 1].ItemController.IsActiveUI;`).

The rewritten block attributes it correctly and names #644 as the cause of the shift. The test's
expected outcome is explicitly recorded as unchanged.

## Region 2 — the `because:` string of the same test

```csharp
                    because: "the null _itemGroups field is dereferenced at _itemGroups[selection - 1] "
                        + "inside RemoveSpecificControlGroupAsync, so the decrement must run on that path"
```

The **two-literal concatenated shape is preserved** and the replacement is **longer** than the
text it replaces (149 characters of literal content against 111), as the task requires. That
matters because it keeps CSharpier's break decision for the enclosing
`await act.Should().ThrowAsync<NullReferenceException>( … );` chain unchanged, so the assertion
lines do not reflow into the diff that `[P4-T9]` gates. The diff below confirms this: the
`await act.Should()` and `.ThrowAsync<NullReferenceException>(` lines appear as unchanged context.

## Region 3 — the `//` comment inside `RemoveSpecificControlGroupAsync_ThrowLaterInBody_RestoresReentrancyCounter`

The comment previously read "A real (empty) KbdActions instance rather than a mock:
UnregisterNavigation calls Remove(...) on it directly, and it must succeed so the throw lands later
in the body." Under the ledger, `UnregisterNavigation()` on that reflection-built controller
iterates an empty ledger and calls `Remove` **zero** times, so the stated reason is false after the
fix. The correction records that the real `KbdActions` instance is retained so the arrangement
stays valid, not because `UnregisterNavigation` still calls `Remove` on it.

The corrected comment is deliberately free of the literal `_itemGroups[selection - 1]`, so it
introduces no third occurrence of that literal and the exactly-two-lines gate below stays exact.

## Acceptance clause 1 — `_itemGroups[selection - 1]` present on exactly two lines

Command: `git grep -F -n '_itemGroups[selection - 1]' -- QuickFiler.Test/Controllers/QfcCollectionControllerDefects468Tests.cs`
EXIT_CODE: 0

```
159:        /// <c>_itemGroups[selection - 1]</c> dereference inside
178:                    because: "the null _itemGroups field is dereferenced at _itemGroups[selection - 1] "
```

**Exactly two lines** — one in the rewritten XML-documentation block and one in the rewritten
`because:` string, as the acceptance names. Verified before the edit that this literal appeared on
**no** line of the file (`git grep` exited 1), so the gate was false before and true after; it is
not vacuous.

## Acceptance clause 2 — the old phrase is gone

Command: `git grep -F -n 'UnregisterNavigation() is the first statement' -- QuickFiler.Test/Controllers/QfcCollectionControllerDefects468Tests.cs`
EXIT_CODE: 1 (no output)

Verified before the edit that this phrase was present on **exactly one** line of this file (line
176) and, by a repository-wide `*.cs` search, on **no other line in the repository**. The gate was
therefore true before and false after.

## Diff shape (anchored to the substituted base `e968a1a8`)

Every changed line is an XML documentation line beginning with `///`, a `//` comment line, or a
string-literal line inside the `because:` argument. No added or removed line contains the token
`Should()`, `ThrowAsync`, or `[TestMethod]`; the lines carrying `await act.Should()` and
`.ThrowAsync<NullReferenceException>(` appear only as unchanged context. The full diff is quoted in
`evidence/qa-gates/p4-t9-comment-only-diff.2026-08-29T08-15.md`, which is the task that formally
gates this property.

## File length

```
lines=498
```

Up from 494 by four comment lines, still under the 500-line repository ceiling.

EXIT_CODE: 0

Output Summary: Three comment regions corrected in
`QuickFiler.Test/Controllers/QfcCollectionControllerDefects468Tests.cs` — the XML documentation
block and the `because:` string of
`RemoveSpecificControlGroupAsync_ThrowAtFirstStatement_RestoresReentrancyCounter`, and the `//`
comment inside the sibling `…_ThrowLaterInBody_…` test. No assertion changed.
`git grep -F '_itemGroups[selection - 1]'` exits 0 and prints **exactly two lines**;
`git grep -F 'UnregisterNavigation() is the first statement'` produces no output and exits 1. The
two-literal `because:` shape is preserved with a longer replacement, so the enclosing chain did not
reflow. File is 498 lines.
