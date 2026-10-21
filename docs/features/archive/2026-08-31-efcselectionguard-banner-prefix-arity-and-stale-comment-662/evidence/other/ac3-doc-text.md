# AC3 — Replacement XML Documentation Text (P1-T4)

Timestamp: 2026-09-01T15-53

Command: `git grep -n -F -- 'BannerRejectionPrefix_RejectsThreeAndFourEqualsRowsOnBothPredicates' -- QuickFiler/Controllers/EfcSelectionGuard.cs`

EXIT_CODE: 0

Output Summary: the command returned exactly one line —

```
QuickFiler/Controllers/EfcSelectionGuard.cs:34:        /// test BannerRejectionPrefix_RejectsThreeAndFourEqualsRowsOnBothPredicates in
```

The identifier appears exactly once in the replacement doc text, which is what
P2-T13's single-matching-line assertion requires.

## Full replacement doc text, as written to the file

```csharp
        /// <summary>
        /// Prefix a selection must not begin with for either predicate to accept it.
        /// <para>
        /// This value is deliberately a PROPER PREFIX of
        /// <see cref="BreadcrumbRowBuilder.BannerPrefix"/>, the four-character prefix both row
        /// producers emit. It is therefore not a copy of the producers' constant and must not be
        /// kept in step with it.
        /// </para>
        /// <para>
        /// Because every row beginning with the producers' four-character prefix also begins with
        /// this three-character one, the guard rejects a strict superset of the producers' banner
        /// rows: every row a producer emits, plus a three-equals row that no producer emits today.
        /// </para>
        /// <para>
        /// It must not be widened to the producers' four-character value. That edit reads like a
        /// consistency fix and is a behavioural relaxation: this prefix is the only mechanism
        /// rejecting a three-equals row at either EFC classification site, because
        /// <see cref="MinimumCreationLength"/> is 3 and so the length rule accepts that input.
        /// Widening it would make <see cref="IsValidFilingSelection"/> and
        /// <see cref="IsValidCreationSelection"/> both return true for a three-equals row. The
        /// test BannerRejectionPrefix_RejectsThreeAndFourEqualsRowsOnBothPredicates in
        /// QuickFiler.Test/Controllers/EfcSelectionGuardTests.cs guards against that edit.
        /// </para>
        /// </summary>
        private const string BannerRejectionPrefix = "===";
```

## The three required statements, located in the text above

1. **It is deliberately a proper prefix of `BreadcrumbRowBuilder.BannerPrefix`.**
   First `<para>`: "This value is deliberately a PROPER PREFIX of
   `BreadcrumbRowBuilder.BannerPrefix`, the four-character prefix both row
   producers emit."

2. **It therefore rejects a strict superset of the producers' banner rows.**
   Second `<para>`: "Because every row beginning with the producers'
   four-character prefix also begins with this three-character one, the guard
   rejects a strict superset of the producers' banner rows: every row a producer
   emits, plus a three-equals row that no producer emits today."

3. **It must not be widened to the producers' four-character value, naming the
   AC6 test as the guard against that edit.** Third `<para>`: "It must not be
   widened to the producers' four-character value. ... The test
   BannerRejectionPrefix_RejectsThreeAndFourEqualsRowsOnBothPredicates in
   QuickFiler.Test/Controllers/EfcSelectionGuardTests.cs guards against that
   edit."

CSharpier does not reflow comment contents, so the test identifier remains a
single-line match after the Phase 2 format pass (Decisions Record D7).
