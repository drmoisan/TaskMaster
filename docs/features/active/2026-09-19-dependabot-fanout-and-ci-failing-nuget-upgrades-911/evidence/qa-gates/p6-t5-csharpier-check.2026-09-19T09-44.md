# P6-T5 — CSharpier check, Batch C close-out

Timestamp: 2026-09-19T09-44

Command: CMD-CSHARPIER-CHECK

```
pwsh -NoProfile -Command 'Set-Location "<execution-worktree-root>"; dotnet tool run csharpier check .'
```

Invoked through `dotnet tool run` so the manifest-pinned version is used. The manifest is
`dotnet-tools.json` at the repository root, which pins CSharpier 1.2.6, and
`.github/workflows/_format-check.yml` hashes that same file, so this invocation is the CI
one.

EXIT_CODE: 0

## Output Summary, verbatim

```
Checked 1623 files in 4380ms.
```

## Acceptance

| Clause | Required | Measured |
|---|---|---|
| `EXIT_CODE` | 0 | 0 |
| Verbatim `Checked N files in Xms.` line recorded | yes | above, `N` = 1623 |
| Files reported with findings | zero | zero — the command printed the summary line alone |

`N` is the **scanned** count, not a rewrite count, per gate rule 6. `check` is read-only and
rewrites nothing; a file with findings would be named on its own line above the summary,
and no such line was printed.

1623 is the same scanned count P2-T4 and P4-T4 recorded. Batch C added six PowerShell files
and no C# file, and CSharpier's scan set is unchanged, which is consistent with P6-T4's
finding that the batch changed no C# compilation input.

No normalised `packages.config` or `app.config` is reported, which would have indicated
that the `.csharpierignore` patterns added by P1-T2 stopped matching.
