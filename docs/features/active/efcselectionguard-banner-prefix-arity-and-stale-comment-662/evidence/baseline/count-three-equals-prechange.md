# Pre-Change Count — Three-Character Literal Declaration (P0-T14)

Timestamp: 2026-09-01T15-48

Command: `git grep -n -F -- '= "===";' -- '*.cs'`

EXIT_CODE: 0

Command: `git grep -nE -- '"={3}";' -- '*.cs'`

EXIT_CODE: 0

Output Summary:

Primary query (anchored fixed-string search), full member set — 1 line:

```
QuickFiler/Controllers/EfcSelectionGuard.cs:15:        private const string BannerPrefix = "===";
```

Cross-check query (bounded-repetition regex, structurally different because it
constrains the run length from both sides rather than matching fixed text), full
member set — 1 line:

```
QuickFiler/Controllers/EfcSelectionGuard.cs:15:        private const string BannerPrefix = "===";
```

**The two member sets are identical element for element.** Each consists of the
single entry `QuickFiler/Controllers/EfcSelectionGuard.cs:15`. Both queries
returned exactly one line, which is the figure this task's acceptance requires.

Both patterns are anchored on the closing quote and semicolon precisely because
the three-character literal is a prefix of the four-character one; an unanchored
search for the shorter literal would also match every four-character
declaration. Both queries are scoped with the `-- '*.cs'` pathspec per Decisions
Record D5; no unscoped figure is asserted.
