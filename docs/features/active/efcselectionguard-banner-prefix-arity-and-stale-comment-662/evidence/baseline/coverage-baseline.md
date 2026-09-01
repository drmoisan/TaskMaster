# Numeric Coverage Baseline (P0-T13)

Timestamp: 2026-09-01T15-58

Command: `pwsh -NoProfile -File .\scripts\vscode\Invoke-MSTestWithCoverage.ps1 -SearchRoot . -Configuration Debug`

EXIT_CODE: 0

`ExpectedExitCode:` is not recorded, because the observed exit code is 0.

## Tool provisioning

`Get-Command dotnet-coverage` found the global tool already installed at
`<user-profile>\.dotnet\tools\dotnet-coverage.exe`, so the conditional
`dotnet tool install --global dotnet-coverage` step did not need to run and no
network access was required for it.

## Staleness guard

- `coverage\coverage.cobertura.xml` was deleted before the run (the file did not
  exist in this fresh worktree, so the guarded `Remove-Item` was a no-op).
- `$started` = `2026-09-01T15:46:12.8580470-04:00`
- `(Get-Item 'coverage\coverage.cobertura.xml').LastWriteTime` =
  `2026-09-01T15:46:57.9367500-04:00`
- Boolean comparison `LastWriteTime -gt $started` = `True`

The document copied to `evidence/baseline/coverage-baseline.cobertura.xml` was
therefore produced by this run and is not a stale document from an earlier task.

Test run inside the coverage script: `Test Run Successful. Total tests: 6926,
Passed: 6926`. The script printed
`Post-processing coverage XML for Koverage compatibility...` followed by
`Done. Coverage artifact: <repo-root>\coverage\coverage.cobertura.xml`, which
places the run past the test-failure throw at `Invoke-MSTestWithCoverage.ps1:236`
and past the threshold assertion at `:341`, and through the post-processing
write at `:343`.

## Output Summary

### (1) PostProcessed

PostProcessed: yes

Discriminator: the `<class filename>` attributes in the copied Cobertura are
repository-relative, not absolute host paths. Sampled first class node:
`QuickFiler\Controllers\EfcHomeController.cs`. `ConvertTo-KoverageCoberturaXml`
always rewrites those paths, and the script writes the post-processed document
only at `Invoke-MSTestWithCoverage.ps1:343`, after both the test-failure throw
at `:236` and the threshold assertion at `:341`.

### (2) Root attributes

```
<coverage line-rate="0.853763" branch-rate="0.793822" complexity="25605" version="1.9" timestamp="1788292001" lines-covered="54967" lines-valid="64382" branches-covered="13106" branches-valid="16510">
```

- `line-rate` = 0.853763
- `lines-covered` = 54967
- `lines-valid` = 64382

### (3) Derived line percentage

`lines-covered / lines-valid * 100` = 54967 / 64382 * 100 = **85.38** (two
decimal places).

### (4) Per-filename `<class>` nodes

Total `<class>` nodes in the document: 559.

| Filename ends with | `<class>` node count | `name` | `line-rate` |
|---|---|---|---|
| `EfcSelectionGuard.cs` | 1 | `QuickFiler.Controllers.EfcSelectionGuard` | 1 |
| `FolderSuggestionTree.cs` | 1 | `UtilitiesCS.FolderSuggestionTree` | 0.9844961240310077 |

Both files are first-party non-`.Test` projects and are therefore in the
`Get-KoverageProjectAllowlist` set. Both are present, so neither is recorded as
`NOT APPLICABLE` and neither filter — the `coverage.config` module exclusions or
the Koverage project allowlist — removed them. Exactly one node per filename is
present, which is the expected shape after `Merge-CoberturaClassesByFilename`
has run (`PostProcessed: yes`).

### (5) The three changed executable statements — pre-change line spans and hits

The spans below are the pre-change spans, valid here because this baseline was
captured before any edit. P2-T9 and P2-T10 resolve the same three statements to
their post-format spans by the same enclosing-member identification.

**Statement A — the `return` statement in `EfcSelectionGuard.IsValidFilingSelection`
that reads `StartsWith(BannerPrefix`.** Span: `EfcSelectionGuard.cs:49-50`
(the `return` keyword is on `:49`; the statement's terminating semicolon is on
`:50`).

| line `number` | `hits` |
|---|---|
| 49 | 1 |
| 50 | 1 |
| 49 | 1 |
| 50 | 1 |

**Statement B — the `return` statement in `EfcSelectionGuard.IsValidCreationSelection`
that reads `StartsWith(BannerPrefix`.** Span: `EfcSelectionGuard.cs:74-76`
(the `return` keyword is on `:74`, carrying the minimum-length comparison; the
renamed call site is the second operand on `:75`; the terminating semicolon is
on `:76`).

| line `number` | `hits` |
|---|---|
| 74 | 1 |
| 75 | 1 |
| 76 | 1 |
| 74 | 1 |
| 75 | 1 |
| 76 | 1 |

**Statement C — the `return` statement in `FolderSuggestionTree.IsBanner` that
reads `StartsWith(BannerPrefix`.** Span: `FolderSuggestionTree.cs:197-197`
(the `return` keyword and the terminating semicolon are on the same line).

| line `number` | `hits` |
|---|---|
| 197 | 1 |
| 197 | 1 |

Each span is listed twice per line number because the Cobertura document
carries each `<line>` element both under the class-level `<lines>` collection
and under the enclosing `<method>`'s own `<lines>` collection, and the
enumeration above uses a descendant query over the class node. Every element in
every span carries `hits` greater than zero, so all three statements are
covered in the baseline: **3/3**.
