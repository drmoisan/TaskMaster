# Nullable gate non-vacuity proof (Issue #824, task P5-T7)

Timestamp: 2026-09-09T16-03

Command: `pwsh -NoProfile -Command 'Set-Location "<worktree-root>"; $l = "coverage/msbuild-nullable-final.log"; Write-Output ("SKIPPED_CORECOMPILE=" + @(Select-String -Path $l -Pattern "Skipping target \x22CoreCompile\x22").Count); Write-Output ("CORECOMPILE=" + @(Select-String -Path $l -Pattern "^\s*CoreCompile:").Count); Write-Output ("DIAG_ERRORS=" + @(Select-String -Path $l -Pattern ": error [A-Z]+[0-9]+:").Count); Write-Output ("CS86_ERRORS=" + @(Select-String -Path $l -Pattern ": error CS86[0-9][0-9]:").Count); Write-Output ("CS86_WARNINGS=" + @(Select-String -Path $l -Pattern ": warning CS86[0-9][0-9]:").Count); Write-Output ("ZERO_ERROR_SUMMARY=" + @(Select-String -Path $l -Pattern "^\s*0 Error\(s\)$").Count)'`

EXIT_CODE: 0

## Output Summary

```
SKIPPED_CORECOMPILE=0
CORECOMPILE=13
DIAG_ERRORS=0
CS86_ERRORS=0
CS86_WARNINGS=0
ZERO_ERROR_SUMMARY=1
```

| Measure | Value | Required | Holds |
|---|---|---|---|
| `SKIPPED_CORECOMPILE` | 0 | 0 | yes |
| `CORECOMPILE` | **13** | greater than 0 | yes |
| `DIAG_ERRORS` | 0 | 0 | yes |
| `CS86_ERRORS` | 0 | 0 | yes |
| `CS86_WARNINGS` | 0 | 0 | yes |
| `ZERO_ERROR_SUMMARY` | 1 | at least 1 | yes |

## This discharges the CS8618 risk spec.md records as unverified

`spec.md` records, in its Assumptions block and again as risk 1 of its Risks section, that a
non-nullable `static readonly` field definitely assigned in the static constructor was *claimed* by
the research to satisfy the compiler's null-state analysis, but that the claim was never verified by
running a build, and that it must be confirmed at this gate.

It is confirmed. Under `/p:TreatWarningsAsErrors=true`, which promotes CS86xx diagnostics to build
errors, the count of `: error CS86[0-9][0-9]:` is 0 and the count of
`: warning CS86[0-9][0-9]:` is also 0. No CS8618 is raised on either
`public static readonly OpCode[] multiByteOpCodes;` or
`public static readonly OpCode[] singleByteOpCodes;`, both of which carry no initializer and are
assigned only in the static constructor.

The mitigation `spec.md` names for the failure case — keep the fields non-nullable and adjust the
static constructor so every path assigns them, rather than reintroducing a `null!` suppression — did
not need to be applied. No `null!` suppression exists in the file, which P4-T3 verified
independently.

This also completes AC6: P4-T3 discharged its comment and annotation half, and this artifact
discharges its CS86xx half.

## Non-vacuity

Zero occurrences of `Skipping target "CoreCompile"` with 13 occurrences of `^\s*CoreCompile:`
establishes that the gate compiled rather than short-circuiting on MSBuild's up-to-date check. Both
directions are asserted because an exit code alone cannot distinguish a clean compile from a skipped
one.

`CORECOMPILE=13` differs from the analyzer gate's `CORECOMPILE=12` recorded in
`evidence/qa-gates/msbuild-analyzers-nonvacuity.2026-09-09T16-02.md`. Per plan D14 the two gates
legitimately produce different counts on the same tree and a mismatch between them is not a defect.
Both are greater than zero, which is the condition each must satisfy.

## Comparison with the baseline nullable build

| Measure | Baseline (P0-T10) | Final (P5-T7) |
|---|---|---|
| `SKIPPED_CORECOMPILE` | 0 | 0 |
| `CORECOMPILE` | 13 | 13 |
| `DIAG_ERRORS` | 0 | 0 |
| `CS86_ERRORS` | 0 | 0 |

The change introduced no compiler or nullable diagnostic.
