# R2 Line 151 — `Resolve-ManifestPackageId` Returns Empty

- Timestamp: 2026-09-20T08-38-20
- Task: [P1-T2]
- Finding: R2
- Command: CMD-PESTER-FILTERED, `<FILE>` = `tests/scripts/vscode/Sync-PackageReferences.Tests.ps1`,
  `<FILTER>` = `*R2- returns no identifier*`
- EXIT_CODE: 0

## Test Added

`It 'R2- returns no identifier when the restore folder matches no manifest package'`

Calls `Resolve-ManifestPackageId` with `-FolderName 'Fabrikam.Core.1.0.0'` against a `-VersionMap`
declaring only `Contoso.Widgets`, and asserts the result is empty.

## Counts Line, Verbatim

```
PESTER Passed=1 Failed=0 Skipped=0 Executed=1 Total=7 NotRun=6
```

| Measurement | Required | Measured | Result |
|---|---|---|---|
| `EXIT_CODE` | 0 | 0 | PASS |
| `Executed` = `Passed + Failed + Skipped` | 1 | **1** | PASS |
| `Passed` | 1 | **1** | PASS |
| `Total` (context) | — | 7 | — |
| `NotRun` (context) | — | 6 | — |

`Total` is recorded as context only and is **not** asserted. Per **gate rule 2**, `TotalCount`
counts the filtered-out tests as `NotRun`, so it reports the whole file's `It` count and is
invariant under the filter, including a filter that matches nothing. `Executed` is the figure that
distinguishes a filter that selected one test from a filter that selected none. Pester's own
discovery line confirms the selection: `Filters selected 1 tests to run.`

## What This Exercises

**Line 151**, the `return ''` that ends `Resolve-ManifestPackageId`. The function iterates the
version map, finds that `'Fabrikam.Core.1.0.0'` does not start with `'Contoso.Widgets.'`, falls
out of the loop, and reaches line 151.

**How it fails.** If the function returned a non-empty identifier for a folder that no manifest key
prefixes, the assertion fails. That is the defect the assertion exists to catch: a false match
would make `Get-HintPathRepair` compose a target folder from a package the manifest never
declared, and rewrite a hint path to a directory that does not exist.

## Output Summary

One test added, one test executed, one passed, exit 0. Line 151 is now reached.
