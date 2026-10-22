# Fail-Before Exception Dossier (P1-T10)

Timestamp: 2026-09-01T15-57

WhyFailingRunImpossible: This change is behaviour-preserving at every call site
— no predicate's return value changes for any input, because the guard's
constant keeps the value `"==="` exactly and only its name and XML
documentation change, while `FolderSuggestionTree.IsBanner` moves from a local
`"===="` constant to `BreadcrumbRowBuilder.BannerPrefix`, whose value is the
identical `"===="`. The new test therefore passes both before and after the
production edit, so no failing run exists to record.

## Alternative proof

### Item 1 — the absence-of-test proof

Artifact: `docs/features/active/efcselectionguard-banner-prefix-arity-and-stale-comment-662/evidence/baseline/absence-of-three-equals-test-prechange.md`

That artifact records `git grep -c -F -- '("===")' -- QuickFiler.Test/Controllers/EfcSelectionGuardTests.cs`
returning no output with `EXIT_CODE: 1` (`ExpectedExitCode: 1`), and enumerates
the file's only three equals-run literals, all at `:43`, `:183` and `:245`, each
the full four-character sentinel row `"==== SUGGESTIONS ===="`.

It therefore establishes that **before this change, no test in
`QuickFiler.Test/Controllers/EfcSelectionGuardTests.cs` asserted a bare
three-equals row against either guard predicate.** The new test adds a
previously unasserted behaviour rather than re-asserting an existing one, which
is why no pre-existing test could have failed.

### Item 2 — two traces showing the prohibited widening edit is test-detected

The prohibited edit is: change `BannerRejectionPrefix` from `"==="` to the
producers' `"===="`.

**Trace A — the new test
`BannerRejectionPrefix_RejectsThreeAndFourEqualsRowsOnBothPredicates`.**

Under that edit, `"===".StartsWith("====", Ordinal)` is false, so the guard no
longer rejects a three-equals row on the prefix rule. On the filing path there
is no other rule that rejects it: `IsValidFilingSelection` carries no
minimum-length rule and `ArchiveStemContract.IsFullOutlookPath("===")` is false,
so `IsValidFilingSelection("===")` returns **true**. On the creation path,
`MinimumCreationLength` is 3 and `"===".Length` is 3, so `3 >= 3` is true and
the length rule rejects nothing either, so `IsValidCreationSelection("===")`
also returns **true**.

Two of the new test's four assertions therefore fail:

- `EfcSelectionGuard.IsValidFilingSelection("===").Should().BeFalse(because);`
- `EfcSelectionGuard.IsValidCreationSelection("===").Should().BeFalse(because);`

Both carry the `because` message whose text contains `must not be widened`, so
the failure output names the prohibited direction rather than only reporting a
boolean mismatch. The remaining two assertions, which pass `"===="`, still pass
under the edit, so the test is not trivially all-red: exactly the two assertions
that pin the broader rejection go red.

**Trace B — the pre-existing test
`IsSelectableFolder_AndIsBannerRow_ClassifyThreeAndFourEqualsRowsIdentically`
in `QuickFiler.Test/Controllers/EfcFormControllerTests.cs`.**

The test iterates `new[] { "===", "====" }` and makes two assertions per row, at
`QuickFiler.Test/Controllers/EfcFormControllerTests.cs:462-463`:

```csharp
                creationPath.Should().Be(filingPath, $"both sites must classify {row} alike");
                creationPath.Should().BeFalse($"{row} is rejected at both sites");
```

For the row `"==="` under the prohibited edit:

- `EfcFormController.IsBannerRow("===")` classifies by the producers'
  four-character constant, so it is false both before and after the edit.
- `creationPath = IsSelectableFolder("===")` becomes **true**, because
  `IsSelectableFolder` composes `!IsBannerRow(row)` with the guard, and the
  guard now accepts the row.
- `filingPath = !IsBannerRow("===") && IsValidFilingSelection("===")` likewise
  becomes **true**.

`:462` compares `creationPath` against `filingPath`. Both are now true, so they
still agree and **`:462` still passes** — even though it is the assertion a
reader would expect to catch a consistency relaxation.

`:463` asserts `creationPath.Should().BeFalse(...)`. `creationPath` is now true,
so **`:463` is the assertion that fails** under the prohibited edit.

This is the hazard the plan's "The Directional Constraint" section names: the
sibling assertion that reads like the consistency guard does not catch the
relaxation, and only the explicit rejection assertion does.
