# [P1-T12] Fixture purity and size audit

Timestamp: 2026-08-11T01-10
Command: `grep -n "<pattern>" <test files>`; `wc -l <test files>`; `git status --porcelain -uall -- tests/`
EXIT_CODE: 0 (audit); the prohibited-pattern grep returned exit 1, meaning zero matches

## Files in this feature's test scope

- `tests/scripts/vscode/Invoke-MSTestWithCoverage.ClosureFilter.Tests.ps1` (new)
- `tests/scripts/vscode/Invoke-MSTestWithCoverage.Helpers.Tests.ps1` (extended with case 6)

## Prohibited-pattern search — zero matches required

A single `grep -n` across both files for all eight patterns returned **no output** and exit status 1
(grep's "no lines selected"). Per-pattern breakdown:

| # | Pattern | `ClosureFilter.Tests.ps1` | `Helpers.Tests.ps1` |
|---|---|---|---|
| 1 | `New-TemporaryFile` | 0 | 0 |
| 2 | `[System.IO.Path]::GetTempPath` | 0 | 0 |
| 3 | `$env:TEMP` | 0 | 0 |
| 4 | `$env:TMP` | 0 | 0 |
| 5 | `TestDrive` | 0 | 0 |
| 6 | `Out-File` | 0 | 0 |
| 7 | `Set-Content` | 0 | 0 |
| 8 | `Add-Content` | 0 | 0 |

Corroborated with a second, independent search tool over
`tests/scripts/vscode/Invoke-MSTestWithCoverage*.Tests.ps1` using the same alternation: "Found 0
total occurrences across 0 files."

## Every fixture is an inline here-string

`tests/scripts/vscode/Invoke-MSTestWithCoverage.ClosureFilter.Tests.ps1` contains **8** `@'`
here-string openers, one per document fixture: cases 1, 2, 3, 4, 5, 7, 8 and 10. Case 9 has no
document fixture at all (it is a pure unit test over literal name strings), which is why the count is
8 rather than 9. Case 6's fixture in `Invoke-MSTestWithCoverage.Helpers.Tests.ps1` is likewise an
inline here-string, matching the ten pre-existing here-string fixtures already in that file.

No fixture is read from disk. No fixture is written to disk.

## No `.cs` file added under `tests/`

`git status --porcelain -uall -- tests/` (verbatim):

```
 M tests/scripts/vscode/Invoke-MSTestWithCoverage.Helpers.Tests.ps1
?? tests/scripts/vscode/Invoke-MSTestWithCoverage.ClosureFilter.Tests.ps1
```

Exactly two paths, both PowerShell. No `.cs` file, and no file of any other kind, is added under
`tests/`.

## File sizes — every file under 500 lines

| File | Lines | Ceiling | Headroom |
|---|---|---|---|
| `tests/scripts/vscode/Invoke-MSTestWithCoverage.ClosureFilter.Tests.ps1` | **367** | 500 | 133 |
| `tests/scripts/vscode/Invoke-MSTestWithCoverage.Helpers.Tests.ps1` | **490** | 500 | 10 |

Both are strictly below 500. These are pre-format measurements; `[P3-T10]` re-measures both against
the post-format state, because formatting can change line counts.

## Pre-authorized split decision (recorded, as this task requires)

**The `[P1-T12]` pre-authorized split was NOT taken.**

The measurement was made in `[P1-T8]` before cases 9 and 10 were authored, as that task requires:
the file stood at 248 lines, and cases 9 and 10 together added 119 lines to reach 367 — 133 lines
below the ceiling. There was therefore no trigger for the split.

Consequently `tests/scripts/vscode/Invoke-MSTestWithCoverage.ClosureFilter.Unit.Tests.ps1` does not
exist (`ls` returns "No such file or directory") and is named nowhere in this execution: not in any
`scan_folders` list, not in any `Run.Path` list, not in any `run_poshqc_format` /
`run_poshqc_analyze` file set, and not in any expected changed-file set. `[P1-T11]`'s scan set was
correct on its first execution. Two test files remain well within the 3-test-file per-batch cap in
`.claude/rules/powershell.md`.

Line counts of "all three files" are therefore not applicable; the two-file table above is complete.

## Related size action recorded

`tests/scripts/vscode/Invoke-MSTestWithCoverage.Helpers.Tests.ps1` reached 499 lines on the first
draft of case 6, leaving 1 line of headroom and making `[P3-T10]`'s post-format ceiling check
fragile. Case 6 was compacted to 490 lines before this audit: the comment block reduced from 5 to 4
lines, the redundant XML declaration removed, each `<class>` placed on a single line (the compact
fixture style already used by the #441 tests in the same file), and one intermediate variable
inlined. No assertion was weakened or removed. The plan authorizes no split of this file, so
creating headroom inside the newly added test was the available remedy.

## Output Summary

Zero matches for all eight prohibited patterns across both test files. All 9 document fixtures are
inline here-strings (8 in the new file, 1 in the extended file); case 9 uses no document fixture. No
`.cs` file is added under `tests/`. Both test files are strictly under 500 lines (367 and 490). The
pre-authorized split was not taken and the third test file does not exist.
