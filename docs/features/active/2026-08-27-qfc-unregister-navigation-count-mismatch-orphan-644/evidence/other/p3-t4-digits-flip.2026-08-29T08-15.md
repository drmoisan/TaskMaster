# Test reconciliation — Digits-file assertion flipped ([P3-T4])

- Issue: #644
- Task: `[P3-T4]`
- Timestamp: 2026-08-29T08-15
- File modified: `QuickFiler.Test/Controllers/QfcCollectionControllerNavigationDigitsTests.cs`
- Test amended: `UnregisterNavigation_AfterRegisteringAtTwoDigitsAndShrinkingToNine_RemovesTheTwoDigitKeys`

## Edit 1 — the assertion, flipped to empty-collection

Replaced:

```csharp
            remaining
                .Should()
                .Equal(
                    new[] { "10" },
                    "only the key the shortened loop bound cannot reach survives, which is the separately-promoted count mismatch"
                );
```

With:

```csharp
            remaining
                .Should()
                .BeEmpty(
                    "issue #644 replaced the count-bounded removal loop with a ledger that replays "
                        + "the recorded registration set verbatim, so no key survives unregistration"
                );
```

The `because:` string names issue #644 and states that the ledger replays the recorded
registration set verbatim, as the task requires.

## Edit 2 — the XML-documentation paragraph, rewritten

The residual-pinning paragraph was rewritten from "The single residual `"10"` entry is expected and
is NOT this fix's scope" to a record that the residual is **closed** by #644:

```
        /// The single residual "10" entry this test used to pin is now closed by issue #644.
        /// It came from the removal loop being bounded by the current <c>_itemGroups.Count</c>,
        /// nine here, so the tenth key was never visited whatever the digit width. #644 replaced
        /// that bound with a ledger that replays the recorded set verbatim, so unregistration is
        /// total and the assertion below is now empty-collection. #472 is strengthened, not undone.
```

The paragraph closes with an explicit supersession statement, which is the mitigation the spec's
Risk 9 names: deleting `_registeredDigits` in this commit could otherwise be mistaken for a revert
of #472.

The rewritten paragraph occupies **five lines**, the same count as the paragraph it replaced. A
first draft ran to seven lines and pushed the file to 228, two above the `[P0-T7]` baseline of 226,
which `[P4-T7]` gates as "at or below the baseline". The paragraph was compacted to five lines
before this task was checked off, and the file is back at exactly 226. This is recorded rather than
silently corrected because the constraint is a real one on this file.

## Acceptance clause 1 — the flipped literal is gone

Command: `git grep -F -n 'new[] { "10" }' -- QuickFiler.Test/Controllers/QfcCollectionControllerNavigationDigitsTests.cs`
EXIT_CODE: 1 (no output)

The literal was present on a single line of this file before the edit (line 184), so this gate was
false before and is true after; it is not vacuous.

## Acceptance clause 2 — `#644` is present

Command: `git grep -F -n '#644' -- QuickFiler.Test/Controllers/QfcCollectionControllerNavigationDigitsTests.cs`
EXIT_CODE: 0

Three matching lines: two in the rewritten XML-documentation paragraph and one in the new
`because:` string.

## Acceptance clause 3 — the `[TestMethod]` count is unchanged

Command: `(Select-String -Path QuickFiler.Test\Controllers\QfcCollectionControllerNavigationDigitsTests.cs -Pattern '\[TestMethod\]').Count`

```
testmethods=3
```

Equals the `[P0-T7]` baseline value of **3**. No `[TestMethod]` was added or removed.

## The sibling assertion is unchanged

The `.Where(k => k.StartsWith("0", StringComparison.Ordinal)).Should().BeEmpty(…)` assertion that
sits immediately above was left exactly as it was, as the task requires:

```csharp
            remaining
                .Where(k => k.StartsWith("0", StringComparison.Ordinal))
                .Should()
                .BeEmpty(
                    "the recorded registration width is replayed, so the '0'-prefixed keys go"
                );
```

## File length

```
lines=226
```

Exactly the `[P0-T7]` baseline of 226.

EXIT_CODE: 0

Output Summary: The `Equal(new[] { "10" }, …)` assertion in
`UnregisterNavigation_AfterRegisteringAtTwoDigitsAndShrinkingToNine_RemovesTheTwoDigitKeys` was
flipped to `BeEmpty(…)` with a `because:` string naming #644, and the residual-pinning XML
paragraph was rewritten to record the residual as closed rather than out of scope, with an explicit
"#472 is strengthened, not undone" supersession statement. `git grep` for `new[] { "10" }` produces
no output and exits 1; `git grep` for `#644` exits 0; the `[TestMethod]` count is **3**, equal to
the baseline; the sibling `StartsWith("0")` assertion is unchanged; and the file is **226** lines,
exactly the baseline.
