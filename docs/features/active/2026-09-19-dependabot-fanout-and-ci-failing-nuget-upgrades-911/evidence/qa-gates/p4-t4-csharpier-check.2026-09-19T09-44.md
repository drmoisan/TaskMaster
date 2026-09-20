# P4-T4 — CSharpier check, Batch B close-out

Timestamp: 2026-09-20T01-12

Command: CMD-CSHARPIER-CHECK.

```
pwsh -NoProfile -Command 'Set-Location "<execution-worktree-root>"; dotnet tool run csharpier check .'
```

EXIT_CODE: 0

## Verbatim output

```
Checked 1623 files in 4506ms.
```

Exactly one output line, which is the success-case form CMD-CSHARPIER-CHECK documents: it begins
`Checked ` and ends `ms.`. `N` is **1623**, an integer, and it is the **scanned** count, not a
rewrite count, per gate rule 6.

## Zero files reported with findings

The command printed no per-file finding line at all. CSharpier's check mode prints one block per
non-conforming file before the summary; the output above carries only the summary, so the count
of files reported with findings is **0**.

That zero is guarded by the positive scanned count of 1623: a run that resolved no files would
print `Checked 0 files` and would fail the integer clause, so a clean tree and a vacuous run are
distinguishable. The figure is unchanged from the 1623 the P2-T4 Batch A gate recorded, which is
expected: Batch B added no `.cs`, `.csproj`, `.xml` or `packages.config` file to the scanned
population, and the two normalised config kinds remain excluded by `.csharpierignore`.

## What this gate fails on

A normalised `packages.config` or `app.config` reported here would mean the `.csharpierignore`
patterns P1-T2 added no longer match, which would put AC2 and AC3 back at risk. Neither kind is
reported. `.csharpierignore` line 4 also excludes `**/evidence/**`, so the coverage-projection
copy P2-T7 placed under the evidence tree does not reach the formatter.

## Acceptance evaluation

| Clause | Required | Measured | Verdict |
|---|---|---|---|
| `EXIT_CODE` | 0 | 0 | PASS |
| Verbatim `Checked N files in Xms.` line with `N` an integer | recorded | `Checked 1623 files in 4506ms.` | PASS |
| Files reported with findings | 0 | 0 | PASS |

Output Summary: CMD-CSHARPIER-CHECK returned EXIT_CODE 0 with the single line
`Checked 1623 files in 4506ms.` and no per-file finding block, so **0** files were reported with
findings against **1623** scanned. The scanned count is unchanged from the Batch A gate, as
expected for a batch that changed no C# compilation input, and no normalised `packages.config` or
`app.config` was reported.
