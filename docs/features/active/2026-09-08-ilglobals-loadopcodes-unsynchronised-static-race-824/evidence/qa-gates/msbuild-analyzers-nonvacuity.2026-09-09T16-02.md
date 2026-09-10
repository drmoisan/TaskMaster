# Analyzer gate non-vacuity proof (Issue #824, task P5-T5)

Timestamp: 2026-09-09T16-02

Command: `pwsh -NoProfile -Command 'Set-Location "<worktree-root>"; $l = "coverage/msbuild-analyzers-final.log"; Write-Output ("SKIPPED_CORECOMPILE=" + @(Select-String -Path $l -Pattern "Skipping target \x22CoreCompile\x22").Count); Write-Output ("CORECOMPILE=" + @(Select-String -Path $l -Pattern "^\s*CoreCompile:").Count); Write-Output ("DIAG_ERRORS=" + @(Select-String -Path $l -Pattern ": error [A-Z]+[0-9]+:").Count); Write-Output ("DIAG_WARNINGS=" + @(Select-String -Path $l -Pattern ": warning [A-Z]+[0-9]+:").Count); Write-Output ("ZERO_ERROR_SUMMARY=" + @(Select-String -Path $l -Pattern "^\s*0 Error\(s\)$").Count)'`

EXIT_CODE: 0

## Output Summary

```
SKIPPED_CORECOMPILE=0
CORECOMPILE=12
DIAG_ERRORS=0
DIAG_WARNINGS=0
ZERO_ERROR_SUMMARY=1
```

| Measure | Value | Required | Holds |
|---|---|---|---|
| `SKIPPED_CORECOMPILE` | 0 | 0 | yes |
| `CORECOMPILE` | **12** | greater than 0 | yes |
| `DIAG_ERRORS` | 0 | 0 | yes |
| `ZERO_ERROR_SUMMARY` | 1 | at least 1 | yes |
| `DIAG_WARNINGS` | 0 | recorded as a number | yes |

Both directions are asserted, which is what makes the gate non-vacuous. Zero occurrences of
`Skipping target "CoreCompile"` establishes that no project's compile was skipped by MSBuild's
up-to-date check, and 12 occurrences of `^\s*CoreCompile:` establishes that compilation actually
ran. A gate evidenced only by `EXIT_CODE: 0` could not fail, because a warm `/t:Build` returns 0
having compiled nothing.

A search for the bare word `error` is deliberately not used. A successful msbuild run in this
repository prints that word roughly 35 times and `Select-String` is case-insensitive by default, so
an acceptance condition of the form "no console line contains `error`" is unsatisfiable. The MSBuild
diagnostic form `: error [A-Z]+[0-9]+:` is used instead, together with the summary line
`^\s*0 Error\(s\)$`.

The spelling `Task "Csc"` is likewise not used: it reads 0 on a genuinely non-vacuous compile at
default verbosity.

`DIAG_WARNINGS=0` means the analyzer stack produced no warning-severity diagnostic anywhere in the
solution. This is consistent with the repository's `.editorconfig` catch-all holding analyzer
diagnostics at `suggestion` severity, which msbuild does not surface as warnings.

## Comparison with the baseline analyzer build

| Measure | Baseline (P0-T9) | Final (P5-T5) |
|---|---|---|
| `SKIPPED_CORECOMPILE` | 0 | 0 |
| `CORECOMPILE` | 9 | 12 |
| `DIAG_ERRORS` | 0 | 0 |
| `DIAG_WARNINGS` | 0 | 0 |

The change introduced no analyzer diagnostic. The `CoreCompile:` counts differ between the two runs;
per plan D14 the count is a property of what MSBuild scheduled on a given invocation rather than a
fixed property of the tree, and a difference is not a defect. Both counts are greater than zero,
which is what the non-vacuity condition requires of each.
