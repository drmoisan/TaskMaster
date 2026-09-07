# AC3 Verification (P2-T13)

Timestamp: 2026-09-01T16-50

Command: `git grep -n -F -- 'BannerRejectionPrefix_RejectsThreeAndFourEqualsRowsOnBothPredicates' -- QuickFiler/Controllers/EfcSelectionGuard.cs`

EXIT_CODE: 0

Output Summary — exactly one line:

```
QuickFiler/Controllers/EfcSelectionGuard.cs:34:        /// test BannerRejectionPrefix_RejectsThreeAndFourEqualsRowsOnBothPredicates in
```

The single-matching-line figure holds after the Phase 2 format pass, because
CSharpier does not reflow comment contents.

## The doc text as it stands on disk (`EfcSelectionGuard.cs:14-38`)

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

This is byte-identical to the text transcribed by P1-T4 into
`evidence/other/ac3-doc-text.md`.

## Three-item checklist — each required statement present

- [x] **It is deliberately a proper prefix of `BreadcrumbRowBuilder.BannerPrefix`.**
  Present in the first `<para>`: "This value is deliberately a PROPER PREFIX of
  `BreadcrumbRowBuilder.BannerPrefix`, the four-character prefix both row
  producers emit."

- [x] **It therefore rejects a strict superset of the producers' banner rows.**
  Present in the second `<para>`: "Because every row beginning with the
  producers' four-character prefix also begins with this three-character one,
  the guard rejects a strict superset of the producers' banner rows..." The
  `<para>` states the containment argument that makes "strict superset"
  justified rather than merely asserted.

- [x] **It must not be widened to the producers' four-character value, naming
  the AC6 test as the guard against that edit.** Present in the third `<para>`:
  "It must not be widened to the producers' four-character value... The test
  BannerRejectionPrefix_RejectsThreeAndFourEqualsRowsOnBothPredicates in
  QuickFiler.Test/Controllers/EfcSelectionGuardTests.cs guards against that
  edit." The named test is the AC6 test, and the `<para>` additionally records
  why the widening is a relaxation rather than a consistency fix.

All three required statements are present.

**AC3 checked off in `issue.md`.**
