# AC1 Verification (P2-T11)

Timestamp: 2026-09-01T16-48

Command: `git grep -n -F -- '= "===";' -- '*.cs'`

EXIT_CODE: 0

Command: `git grep -nE -- '"={3}";' -- '*.cs'`

EXIT_CODE: 0

Output Summary:

Primary query — exactly one line:

```
QuickFiler/Controllers/EfcSelectionGuard.cs:38:        private const string BannerRejectionPrefix = "===";
```

Cross-check query — exactly one line:

```
QuickFiler/Controllers/EfcSelectionGuard.cs:38:        private const string BannerRejectionPrefix = "===";
```

**Both queries return the same single-member set**, whose one member is located
in `QuickFiler/Controllers/EfcSelectionGuard.cs`. That is what AC1 requires.

The declaration moved from `:15` to `:38` because P1-T4 replaced the constant's
one-line XML doc with a multi-line one. The line number is not part of AC1's
assertion; the count and the containing file are, and both hold.

The guard's rejection breadth is therefore unchanged: its constant still holds
the three-character value. This is the directional constraint the issue exists
to protect — the value was not widened to the producers' four-character form.

**AC1 checked off in `issue.md`.**
