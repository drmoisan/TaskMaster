# CSharpier Check Baseline — Remediation Cycle 1, Issue #911

- Timestamp: 2026-09-20T08-32-59
- Task: [P0-T9]
- Command: CMD-CSHARPIER-CHECK
- EXIT_CODE: 0

## Command

```
pwsh -NoProfile -Command 'Set-Location "<execution-worktree-root>"; dotnet tool run csharpier check .; exit $LASTEXITCODE'
```

Invoked through `dotnet tool run` so the manifest-pinned CSharpier 1.2.6 is used, never a global
install, which is what keeps this in parity with `.github/workflows/_format-check.yml`.

## Verbatim Output

```
Checked 1623 files in 4542ms.
```

Exit code 0.

## Acceptance

| Clause | Required | Measured | Result |
|---|---|---|---|
| `EXIT_CODE` | 0 | 0 | PASS |
| The `Checked N files in Xms.` line, verbatim | present | present | PASS |
| `N` | integer greater than 900 | **1623** | PASS |
| Files reported with findings | 0 | 0 | PASS |

`N = 1623` is the **scanned** count, not a finding count, per **gate rule 6**. It is the
non-vacuity observation: a run that resolved no files would still exit 0 and would print a
`Checked 0 files` line, so the count is what distinguishes a real clean pass from an empty one.

A clean run prints only the `Checked` line; a run with findings prints one line per offending file
before it. No such line appeared, so zero files were reported with findings.

## Gate Rule 14 Note

This is a **post-merge** measurement. The branch has taken a clean merge of `origin/main` since the
review, and the merge could have brought an unformatted C# file. It did not: 1623 files check clean.

## Output Summary

`dotnet tool run csharpier check .` exited 0 having checked 1623 files with zero findings. The
`origin/main` merge brought no unformatted file.
