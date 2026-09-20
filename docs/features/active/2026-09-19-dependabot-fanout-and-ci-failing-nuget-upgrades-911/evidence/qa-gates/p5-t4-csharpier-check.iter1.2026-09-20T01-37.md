# Final QA Step 4 — CSharpier Check, Iteration 1

- Timestamp: 2026-09-20T09-09-58
- Task: [P5-T4]
- Command: CMD-CSHARPIER-CHECK
- EXIT_CODE: 0

## Command

```
pwsh -NoProfile -Command 'Set-Location "<execution-worktree-root>"; dotnet tool run csharpier check .; exit $LASTEXITCODE'
```

Invoked through `dotnet tool run` so the manifest-pinned CSharpier 1.2.6 is used, which is what
keeps this in parity with `.github/workflows/_format-check.yml`.

## Verbatim Output

```
Checked 1623 files in 4446ms.
```

## Acceptance

| Clause | Required | Measured | Result |
|---|---|---|---|
| `EXIT_CODE` | 0 | **0** | PASS |
| The `Checked N files in Xms.` line, verbatim | present | **present** | PASS |
| `N` | integer greater than 900 | **1623** | PASS |
| Files reported with findings | 0 | **0** | PASS |

A clean run prints only the `Checked` line; a run with findings prints one line per offending
file before it. No such line appeared.

`N = 1623` is the **scanned** count, not a finding count, per **gate rule 6**. It is the
non-vacuity observation: a run that resolved no files would still exit 0 and print
`Checked 0 files`.

The scanned count is identical to the [P0-T9] baseline of 1623, which is the expected result:
this cycle changed no `.cs`, `.csproj`, `packages.config`, `app.config` or `.csharpierignore`
file, so the set CSharpier resolves is unchanged.

## Output Summary

`dotnet tool run csharpier check .` exited 0 having checked 1623 files with zero findings, the
same count as the [P0-T9] baseline. Step 4 of the final loop passes.
