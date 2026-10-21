# P9-T4 — C# QA step 1, CSharpier check (iteration 1)

Timestamp: 2026-09-20T09-44

Command: CMD-CSHARPIER-CHECK.

```
pwsh -NoProfile -Command 'Set-Location "<execution-worktree-root>"; dotnet tool run csharpier check .'
```

Invoked through `dotnet tool run` so the manifest-pinned CSharpier 1.2.6 is used rather than any
global install, which is what keeps the result in parity with `.github/workflows/_format-check.yml`.

EXIT_CODE: 0

## Output, verbatim

```
Checked 1623 files in 4237ms.
```

That is the entire output. CSharpier prints one line per file that would be reformatted, so an empty
body above the summary line is the observation that **zero files were reported with findings**.

## Acceptance

| Clause | Required | Measured | Result |
|---|---|---|---|
| `EXIT_CODE` | 0 | 0 | PASS |
| Verbatim summary line recorded | yes | `Checked 1623 files in 4237ms.` | PASS |
| `N` is an integer greater than 900 | greater than 900 | 1623 | PASS |
| Files reported with findings | 0 | 0 | PASS |

The greater-than-900 clause is the non-vacuity guard: `check` exits 0 over an empty file set as
readily as over a clean one, so the count is what distinguishes a clean run from a run that resolved
nothing. It resolved 1623 files.

This change modifies no `.cs` file. The gate is run because the repository toolchain requires all
four C# steps in one pass on the delivered tree, not because a C# input changed.
