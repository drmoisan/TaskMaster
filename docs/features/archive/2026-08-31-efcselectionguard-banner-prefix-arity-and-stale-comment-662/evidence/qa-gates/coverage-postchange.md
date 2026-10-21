# Post-Change Numeric Coverage (P2-T9)

Timestamp: 2026-09-01T16-45

Command: `pwsh -NoProfile -File .\scripts\vscode\Invoke-MSTestWithCoverage.ps1 -SearchRoot . -Configuration Debug`

EXIT_CODE: 0

`ExpectedExitCode:` is not recorded, because the observed exit code is 0.

## Staleness guard

- `coverage\coverage.cobertura.xml` was deleted before the run.
- `$started` = `2026-09-01T16:44:49.0153637-04:00`
- `(Get-Item 'coverage\coverage.cobertura.xml').LastWriteTime` =
  `2026-09-01T16:45:35.8107977-04:00`
- Boolean comparison `LastWriteTime -gt $started` = `True`

The document copied to `evidence/qa-gates/coverage-postchange.cobertura.xml` was
therefore produced by this run. The copy was taken immediately after the run,
before any later run could overwrite `coverage\coverage.cobertura.xml` in place.

Test run inside the coverage script: `Passed: 6927`, total time 28.6916 seconds.
That is the baseline's 6926 plus the one new test. The script printed
`Post-processing coverage XML for Koverage compatibility...` and
`Done. Coverage artifact: <repo-root>\coverage\coverage.cobertura.xml`, placing
the run past the test-failure throw at `Invoke-MSTestWithCoverage.ps1:236`, past
the threshold assertion at `:341`, and through the post-processing write at
`:343`.

## Output Summary

### (1) PostProcessed

PostProcessed: yes

Discriminator: the `<class filename>` attributes are repository-relative, not
absolute host paths. Sampled first class node:
`QuickFiler\Controllers\EfcHomeController.cs`.

### (2) Root attributes

```
<coverage line-rate="0.853741" branch-rate="0.793761" complexity="25605" version="1.9" timestamp="1788295519" lines-covered="54969" lines-valid="64386" branches-covered="13105" branches-valid="16510">
```

- `line-rate` = 0.853741
- `lines-covered` = 54969
- `lines-valid` = 64386

### (3) Derived line percentage

`lines-covered / lines-valid * 100` = 54969 / 64386 * 100 = **85.37** (two
decimal places).

### (4) Per-filename `<class>` nodes

Total `<class>` nodes in the document: 559 — the same figure as the baseline.

| Filename ends with | `<class>` node count | `name` | `line-rate` |
|---|---|---|---|
| `EfcSelectionGuard.cs` | 1 | `QuickFiler.Controllers.EfcSelectionGuard` | 1 |
| `FolderSuggestionTree.cs` | 1 | `UtilitiesCS.FolderSuggestionTree` | 0.9849624060150376 |

Neither file is recorded as `NOT APPLICABLE`; both are present with exactly one
node, matching the baseline's shape.

### (5) The three changed executable statements — post-format line spans and hits

Each statement is identified by its enclosing member, then resolved to its
post-format line span from the file as it stands after P2-T1. The pre-change
numbers `EfcSelectionGuard.cs:49`, `EfcSelectionGuard.cs:75` and
`FolderSuggestionTree.cs:197` are NOT carried forward: the multi-line XML doc
written by P1-T4, the declaration deleted by P1-T6, and the CSharpier wrap
described in P1-T6 each shift them.

**Statement A — the `return` statement in `EfcSelectionGuard.IsValidFilingSelection`
that reads `StartsWith(BannerRejectionPrefix`.** Resolved post-format span:
`EfcSelectionGuard.cs:72-73` (`return` keyword on `:72`; terminating semicolon on
`:73`).

| line `number` | `hits` |
|---|---|
| 72 | 1 |
| 73 | 1 |
| 72 | 1 |
| 73 | 1 |

**Statement B — the `return` statement in `EfcSelectionGuard.IsValidCreationSelection`
that reads `StartsWith(BannerRejectionPrefix`.** Resolved post-format span:
`EfcSelectionGuard.cs:97-99` (`return` keyword on `:97`, carrying the
minimum-length comparison; the renamed call site is the second operand on `:98`;
terminating semicolon on `:99`).

| line `number` | `hits` |
|---|---|
| 97 | 1 |
| 98 | 1 |
| 99 | 1 |
| 97 | 1 |
| 98 | 1 |
| 99 | 1 |

**Statement C — the `return` statement in `FolderSuggestionTree.IsBanner` that
reads `BreadcrumbRowBuilder.BannerPrefix`.** Resolved post-format span:
`FolderSuggestionTree.cs:196-200` (`return` keyword on `:196`; the qualified
constant reference on `:198`; terminating semicolon on `:200`). The span grew
from one line to five because CSharpier wrapped the reader in P2-T1.

| line `number` | `hits` |
|---|---|
| 196 | 1 |
| 197 | 1 |
| 198 | 1 |
| 199 | 1 |
| 200 | 1 |
| 196 | 1 |
| 197 | 1 |
| 198 | 1 |
| 199 | 1 |
| 200 | 1 |

Each span is listed twice per line number because the Cobertura document carries
each `<line>` element both under the class-level `<lines>` collection and under
the enclosing `<method>`'s own `<lines>` collection.

Every span contains at least one line element, so the BLOCKED branch for an
empty span does not arise. Every element in every span carries `hits` greater
than zero, so all three statements are covered: **3/3**.
