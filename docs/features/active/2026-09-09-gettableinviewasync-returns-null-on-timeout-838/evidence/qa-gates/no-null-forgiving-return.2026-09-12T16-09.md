# P4-T12 — The null-forgiving suppression is gone from the return statement

Timestamp: 2026-09-13T03-19

Command: the plan's fixed search-gate form applied to `UtilitiesCS/OutlookObjects/Table/OlTableExtensions.TableAccess.cs`, first for the fixed literal `return table!;` and then for the case-sensitive regular expression `return [A-Za-z_][A-Za-z0-9_]*!;`, which was echoed before use and delivered intact.

EXIT_CODE: 0

```
PATTERN=return [A-Za-z_][A-Za-z0-9_]*!;
RETURN_BANG_COUNT=0
RETURN_ANY_BANG_COUNT=0
```

Output Summary: both acceptance clauses hold, both counts being exactly 0. P0-T21 recorded a pre-change count of 1 for the fixed literal in this same file through this same search form, so the gate has a demonstrated non-zero before-state and a zero here is a real transition rather than a search that never matched.

The second count generalises the first: the pattern matches a null-forgiving suppression on the returned expression of any simple-identifier return in the file, not only on the one named `table`, so the suppression cannot have been preserved by renaming the local. Its zero value therefore closes the obvious way of satisfying the first gate without satisfying its intent.

The suppression is removable because the guard immediately above the return narrows the local to non-null on every path that reaches the return, which P4-T4's nullable build confirms empirically: that gate reports `ERROR_CS86_LINES=0` with warnings treated as errors and the file carrying the per-file nullable opt-in directive, so the unsuppressed return produces no nullable-flow diagnostic. Together with P4-T4 this decides acceptance criterion 7.
