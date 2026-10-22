# [P2-T2] — CSharpier check (read-only verify)

- Timestamp: 2026-08-30T02-08
- Task: `[P2-T2]`
- Issue: #644
- Branch: `bug/qfc-unregister-navigation-count-mismatch-orphan-644`
- Working directory: repository root of the branch worktree (recorded as a generic
  placeholder; no absolute host path is written to this artifact).
- Command: `dotnet tool run csharpier check .`
- EXIT_CODE: 0

## Console output

```
Checked 1562 files in 4532ms.
```

CSharpier's `check` subcommand is the read-only, CI-parity verify form. It exits
non-zero and prints a per-file diff for any file that would be reformatted. It printed
no file and exited 0, so every C# source file in the tree, including
`QuickFiler.Test/Controllers/QfcCollectionControllerNavigationDigitsTests.cs` with its
100-column line 179, is already formatter-clean.

The file count of 1562 matches the count `[P2-T1]` reported, confirming both
invocations covered the same file set through the manifest-pinned CSharpier 1.2.6.

## Acceptance

| Clause | Required | Measured | Result |
|---|---|---|---|
| `EXIT_CODE:` | 0 | **0** | PASS |

## Output Summary

`dotnet tool run csharpier check .` exited 0 and printed
`Checked 1562 files in 4532ms.` with no file listed as needing reformatting. The
formatting gate passes.
