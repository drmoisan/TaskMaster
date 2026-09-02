# P2-T8 — `[ExcludeFromCodeCoverage]` attribute invariant, remediation cycle 1

Timestamp: 2026-09-02T01-40

## Commands

```
git add -A -- QuickFiler QuickFiler.Test
git diff --cached 807fb0bb6e5e49f43efa6b256b05960bf078ca19 -- QuickFiler QuickFiler.Test
```

The staging step is required because a name-listing or content diff enumerates tracked
changes only, so the file this cycle created would otherwise be invisible to it.

The diff spans **3714** lines of output.

## Acceptance clauses

| # | Clause | Result |
|---|---|---|
| 1 | zero added lines and zero removed lines carrying the token `ExcludeFromCodeCoverage`, both counts stated as 0 | PASS |
| 2 | the diff's total added-line and removed-line counts recorded | PASS |

### Clause 1 — attribute counts

| Count | Value |
|---|---|
| Added lines carrying `ExcludeFromCodeCoverage` | **0** |
| Removed lines carrying `ExcludeFromCodeCoverage` | **0** |

No `[ExcludeFromCodeCoverage]` attribute was added or removed anywhere under `QuickFiler/` or
`QuickFiler.Test/` across the whole branch relative to the base ref.

### Clause 2 — total line counts, so the zero is not taken over an empty diff

| Count | Value |
|---|---|
| Total added lines (`+`, excluding `+++` headers) | **2127** |
| Total removed lines (`-`, excluding `---` headers) | **620** |

Both totals are far greater than zero, so the two zeros in clause 1 are taken over a real
change rather than over an empty diff. This is what makes the gate falsifiable: had this cycle
added or removed such an attribute, the clause-1 counts would be non-zero while the clause-2
counts stayed large.

## The one attribute in scope, unchanged

`QuickFiler/Controllers/QfcDatamodel.cs:25` carries a class-level `[ExcludeFromCodeCoverage]`
on `public partial class QfcDatamodel`. It is pre-existing, it is untouched by this cycle, and
it is the reason `QuickFiler/Controllers/QfcDatamodel.QueueProcessing.cs` has no row in the
coverage report at either the P0-T10 baseline or the P2-T7 post-change comparison. The only
other occurrence under the two prefixes is the pre-existing attribute on `FolderScoringService`
in `QuickFiler/Controllers/QfcHighConfidencePreFilter.cs`, whose per-file occurrence count is
1 both before and after this cycle's edits to that file.
