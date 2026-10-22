# AC6 Verification (P2-T17)

Timestamp: 2026-09-01T16-54

Source: the TRX produced by P2-T5 at
`docs/features/active/efcselectionguard-banner-prefix-arity-and-stale-comment-662/evidence/regression-testing/p2-t5/ac6-scoped.trx`

EXIT_CODE: 0 (the P2-T5 run)

Output Summary:

`<Counters ... />` line from the P2-T5 TRX:

```
<Counters total="1" executed="1" passed="1" failed="0" error="0" timeout="0" aborted="0" inconclusive="0" passedButRunAborted="0" notRunnable="0" notExecuted="0" disconnected="0" warning="0" completed="0" inProgress="0" pending="0" />
```

`passed="1"` and `failed="0"`, which are exactly the figures AC6 names for a
scoped run with
`/Tests:BannerRejectionPrefix_RejectsThreeAndFourEqualsRowsOnBothPredicates`.

The P2-T5 artifact records the staleness guard for this TRX: the results
directory was deleted before the run, and the produced file's `LastWriteTime`
(16:01:18) is later than P2-T1's `Timestamp:` for the current loop pass
(15-59), so these counters belong to the final pass.

## Full source text of the new test method

`QuickFiler.Test/Controllers/EfcSelectionGuardTests.cs:294-314`:

```csharp
        [TestMethod]
        public void BannerRejectionPrefix_RejectsThreeAndFourEqualsRowsOnBothPredicates()
        {
            // Pins the intended relationship between the guard's three-character rejection
            // prefix and the producers' four-character banner prefix (#662). The guard is
            // deliberately the broader of the two: it rejects both arities on both predicates.
            // A contributor who "unifies" the guard upward to the producers' four-character
            // value relaxes it, and the two three-equals assertions below then fail.
            // Arrange
            const string because =
                "this constant must not be widened to the producers' four-character prefix: "
                + "widening it is the prohibited direction, because the three-character prefix "
                + "is the only mechanism rejecting a three-equals row at either EFC "
                + "classification site";

            // Act / Assert
            EfcSelectionGuard.IsValidFilingSelection("===").Should().BeFalse(because);
            EfcSelectionGuard.IsValidCreationSelection("===").Should().BeFalse(because);
            EfcSelectionGuard.IsValidFilingSelection("====").Should().BeFalse(because);
            EfcSelectionGuard.IsValidCreationSelection("====").Should().BeFalse(because);
        }
```

## AC6's clauses, each satisfied

- **A new MSTest test method named
  `BannerRejectionPrefix_RejectsThreeAndFourEqualsRowsOnBothPredicates` is added
  to `QuickFiler.Test/Controllers/EfcSelectionGuardTests.cs`.** Present at
  `:295`, carrying `[TestMethod]`. It was added to the existing file; no new file
  was created and no `.csproj` was edited, because
  `QuickFiler.Test/QuickFiler.Test.csproj:63` already includes this file.
- **It asserts that `IsValidFilingSelection` and `IsValidCreationSelection` each
  return false for `"==="` and for `"===="`.** Four assertions at `:310-313`,
  one per predicate-and-arity combination. P1-T8 confirmed the literal shape by
  count: `("===")` on 2 lines and `("====")` on 2 lines, so both arities are
  pinned explicitly rather than driven from an array.
- **Its FluentAssertions `because` message states that widening the guard to the
  producers' four-character prefix is the prohibited direction.** The local
  `const string because` at `:303-307` states exactly that, and all four
  assertions pass it as the `because` argument. P1-T9 confirmed the token
  `must not be widened` appears on exactly one line.
- **Verified by a scoped `vstest.console.exe` run reporting `Passed: 1` and
  `Failed: 0`.** The P2-T5 counters above report `passed="1"` and `failed="0"`.

The test uses MSTest attributes and FluentAssertions only: no Moq, no clock, no
randomness, no async, and no temporary file, so it satisfies the determinism and
external-dependency rules in `.claude/rules/general-unit-test.md`.

**AC6 checked off in `issue.md`.**
