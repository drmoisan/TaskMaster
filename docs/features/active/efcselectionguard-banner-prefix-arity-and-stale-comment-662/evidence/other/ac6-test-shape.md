# AC6 — New Test Literal Shape (P1-T8)

Timestamp: 2026-09-01T15-55

Command: `git grep -c -F -- '("===")' -- QuickFiler.Test/Controllers/EfcSelectionGuardTests.cs`

EXIT_CODE: 0

Output Summary:

```
QuickFiler.Test/Controllers/EfcSelectionGuardTests.cs:2
```

Reported count: **2**.

Command: `git grep -c -F -- '("====")' -- QuickFiler.Test/Controllers/EfcSelectionGuardTests.cs`

EXIT_CODE: 0

Output Summary:

```
QuickFiler.Test/Controllers/EfcSelectionGuardTests.cs:2
```

Reported count: **2**.

Both figures hold, so the new test pins both arities explicitly rather than
having drifted to an array-driven form.

The closing quote in each pattern is what prevents the three-character pattern
from matching the four-character call, so the two counts are independent. The
P0-T17 baseline established that neither pattern matched anything in this file
before the change, so both counts of 2 are wholly attributable to the four
assertions added by P1-T7:

- `EfcSelectionGuard.IsValidFilingSelection("===").Should().BeFalse(because);`
- `EfcSelectionGuard.IsValidCreationSelection("===").Should().BeFalse(because);`
- `EfcSelectionGuard.IsValidFilingSelection("====").Should().BeFalse(because);`
- `EfcSelectionGuard.IsValidCreationSelection("====").Should().BeFalse(because);`

The three pre-existing equals-run literals in this file are each the full
sentinel row `"==== SUGGESTIONS ===="`, in which the character following the
equals run is a space rather than the closing quote, so none of them is matched
by either pattern.

## P1-T9 — the explanatory message names the prohibited direction

Command: `git grep -c -F -- 'must not be widened' -- QuickFiler.Test/Controllers/EfcSelectionGuardTests.cs`

EXIT_CODE: 0

Output Summary:

```
QuickFiler.Test/Controllers/EfcSelectionGuardTests.cs:1
```

Reported count: **1**. The single figure follows from the shape mandated by
P1-T7: one local `because` constant shared by all four assertions. The token
sits inside a single string literal, and CSharpier does not reflow string-literal
contents, so it stays on one line through the Phase 2 format pass.

The `because` constant's full value is:

```
this constant must not be widened to the producers' four-character prefix: widening it is the prohibited direction, because the three-character prefix is the only mechanism rejecting a three-equals row at either EFC classification site
```
