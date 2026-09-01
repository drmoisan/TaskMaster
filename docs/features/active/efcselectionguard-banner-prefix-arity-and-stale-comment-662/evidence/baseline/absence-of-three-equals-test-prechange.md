# Pre-Change Absence of a Three-Equals-Only Literal in the Test File (P0-T17)

Timestamp: 2026-09-01T16-02

Command: `git grep -c -F -- '("===")' -- QuickFiler.Test/Controllers/EfcSelectionGuardTests.cs`

EXIT_CODE: 1

ExpectedExitCode: 1

Output Summary:

The command returned no output at all. `git grep -c` reports no count line when
the file contains no match, and exits 1. That exit code is the expected outcome
for this task: it is asserting an absence, so exit 1 is the passing result.

Before this change, no test in
`QuickFiler.Test/Controllers/EfcSelectionGuardTests.cs` passes a bare
three-equals value to either guard predicate.

The file's only equals-run literals are three occurrences of the four-character
banner sentinel, confirmed by `git grep -n -- '===' -- QuickFiler.Test/Controllers/EfcSelectionGuardTests.cs`:

```
QuickFiler.Test/Controllers/EfcSelectionGuardTests.cs:43:            EfcSelectionGuard.IsValidFilingSelection("==== SUGGESTIONS ====").Should().BeFalse();
QuickFiler.Test/Controllers/EfcSelectionGuardTests.cs:183:                "==== SUGGESTIONS ====",
QuickFiler.Test/Controllers/EfcSelectionGuardTests.cs:245:            EfcSelectionGuard.IsValidCreationSelection("==== SUGGESTIONS ====").Should().BeFalse();
```

Each of the three is the full sentinel row `"==== SUGGESTIONS ===="`, whose
prefix is the producers' four-character value. None of the three is a bare
three-equals value, and none of the three would be matched by the P1-T8
patterns `("===")` or `("====")`, because in each case the character following
the equals run is a space rather than the closing quote.

This is the absence proof the Phase 1 fail-before exception dossier (P1-T10)
cites.
