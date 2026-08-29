# Search Scope and Tracked-File Census (P0-T5) — discharges AC-3

- **Issue:** #635
- **Plan task:** [P0-T5]

Timestamp: 2026-08-29T06-25

## Output Summary

The Partition A search scope is 683 tracked non-`.cs` files outside the docs tree and the .claude
tree. The comparable scope of the AC-16 six-extension build-input search over the same excluded trees
is 153 files, so the widening adds 530 files. The extension census of the Partition A scope carries
twelve rows led by `.md 190`. Both commands exit through a `pwsh` wrapper whose process exit code is
not asserted; the printed values are the evidence.

SCOPE_FILES: 683
AC16_SIX_EXTENSION_SCOPE: 153
WIDENING_DELTA: 530

## Command 1 — Partition A scope and extension census

Command:

```
pwsh -NoProfile -Command '$f = git ls-files -- ":(exclude)*.cs" ":(exclude)docs/*" ":(exclude).claude/*"; Write-Output ("SCOPE_FILES=" + $f.Count); $f | Group-Object { [System.IO.Path]::GetExtension($_) } | Sort-Object Count -Descending | Select-Object -First 12 | ForEach-Object { Write-Output ((($_.Name -replace "^$","(none)")) + " " + $_.Count) }'
```

Output, verbatim:

```
SCOPE_FILES=683
.md 190
.toml 96
.svg 77
.resx 62
.ps1 51
.config 38
.png 28
.json 28
.csproj 18
.bak 11
.txt 9
.sh 9
```

The printed first line is `SCOPE_FILES=683`, as the acceptance condition requires. The printed
extension census carries twelve rows and includes the line `.md 190`.

The census matches the reference rows recorded in the plan exactly: `.md 190`, `.toml 96`, `.svg 77`,
`.resx 62`, `.ps1 51`, `.config 38`, `.png 28`, `.json 28`, `.csproj 18`, `.bak 11`, `.txt 9`,
`.sh 9`. Eight of the twelve census extensions — `.md`, `.toml`, `.svg`, `.ps1`, `.png`, `.bak`, `.txt`
and `.sh` — lie outside the six build-input extensions the AC-16 search covered. The remaining four —
`.resx`, `.config`, `.json` and `.csproj` — are inside it. That eight-to-four split is the substance of
the widening.

Exit-code handling: the `pwsh -NoProfile -Command` wrapper exits `0` regardless of the exit code of any
command inside it, so the wrapper's exit code carries no information about the measurement. The
printed values are asserted; the wrapper's exit code is not.

EXIT_CODE: 0

## Command 2 — repository census and the AC-16 comparable scope

Command:

```
pwsh -NoProfile -Command 'Write-Output ("TRACKED_TOTAL=" + (git ls-files).Count); Write-Output ("TRACKED_CS=" + (git ls-files -- "*.cs").Count); Write-Output ("TRACKED_NON_CS=" + (git ls-files -- ":(exclude)*.cs").Count); Write-Output ("AC16_SIX_EXTENSION_SCOPE=" + (git ls-files -- "*.csproj" "*.resx" "*.config" "*.xaml" "*.json" "*.settings" ":(exclude)docs/*" ":(exclude).claude/*").Count)'
```

Output, verbatim:

```
TRACKED_TOTAL=11873
TRACKED_CS=1599
TRACKED_NON_CS=10274
AC16_SIX_EXTENSION_SCOPE=153
```

EXIT_CODE: 0

The same exit-code handling applies: only the printed values are asserted.

## Derived widening figure

WIDENING_DELTA is defined by the plan as the printed `SCOPE_FILES` value minus the printed
`AC16_SIX_EXTENSION_SCOPE` value:

```
683 - 153 = 530
```

WIDENING_DELTA: 530

The printed `AC16_SIX_EXTENSION_SCOPE` value of 153 is greater than zero and less than the printed
`SCOPE_FILES` value of 683, as the acceptance condition requires. The AC-16 comparable scope is
therefore a proper non-empty subset of the Partition A scope, and the widening this item performs adds
530 tracked files that the earlier search could not reach.

## Reconciliation against the plan's reference values

The plan records base-commit reference values that are not asserted. Comparison against what was
printed in this worktree at HEAD `d6cfb21c2185088847df5f6e209f79f05c6483ce`:

| Value | Plan reference at base commit | Printed here | Difference |
|---|---|---|---|
| `TRACKED_TOTAL` | 11866 | 11873 | +7 |
| `TRACKED_CS` | 1599 | 1599 | 0 |
| `TRACKED_NON_CS` | 10267 | 10274 | +7 |
| `AC16_SIX_EXTENSION_SCOPE` | 153 | 153 | 0 |
| `WIDENING_DELTA` | 530 | 530 | 0 |
| `SCOPE_FILES` | 683 (asserted) | 683 | 0 |

The seven-file difference in the repository-wide totals is accounted for by commits on this branch
between the specification's base commit `b56400ab663a85b6039139d4548f408821e957ce` and the current
HEAD, all of which added tracked Markdown under the docs tree. The difference does not reach the two
asserted values: `SCOPE_FILES` and `AC16_SIX_EXTENSION_SCOPE` are both measured over a pathspec that
excludes the docs tree and the .claude tree, so neither can be moved by docs-tree additions, and
neither can be moved by this item's own artifact writes.

`TRACKED_CS` is unchanged at 1,599, which corroborates independently that no C# file has been added or
removed on this branch.
