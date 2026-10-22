# P0-T4 — CSharpier formatting baseline (cycle 1)

Timestamp: 2026-08-28T03-42
Task: [P0-T4]
Command: dotnet tool run csharpier check .
EXIT_CODE: 0

Run from the worktree root, through `dotnet tool run` so the manifest-pinned CSharpier is used.
`dotnet-tools.json` at the repository root pins `csharpier` to **1.2.6** with `rollForward: false`,
and `dotnet tool run csharpier --version` reports `1.2.6`, so this is the same version
`.github/workflows/ci.yml` runs after `dotnet tool restore`. No globally installed CSharpier was
invoked.

## Complete output

```
Checked 1547 files in 4818ms.
```

That is the whole of stdout and stderr combined. CSharpier's `check` mode prints one
`Error ... was not formatted` block per offending file and a diff; the output contains **no such
block**, and a case-insensitive search for `was not formatted` and for `Error ` over the captured
output returns **0** lines.

## The unformatted baseline set

**Empty.** Zero files are reported unformatted on the untouched worktree at REM_BASE.

This is the comparison set for P4-T2. Because it is empty, the P4-T2 acceptance condition reduces to
its strict form: after the two source edits and the P4-T1 format pass, `dotnet tool run csharpier
check .` must again report zero unformatted files. Any file appearing in the P4-T2 report would be an
addition over this baseline and would fail that gate.

Note for the P4-T2 comparison: `1547` is the count of files CSharpier **processed**, not a count of
findings. It is recorded so that a materially different processed count at P4-T2 — which would mean
the scan set changed rather than the formatting — is visible.

## Acceptance

| P0-T4 condition | Result |
|---|---|
| `EXIT_CODE: 0` | **Yes** — observed `0` |
| Zero files reported unformatted, or the exact set recorded | **Yes** — zero; the baseline set is empty |

Output Summary: `dotnet tool run csharpier check .` exited **0** over the untouched worktree,
reporting `Checked 1547 files in 4818ms.` and **no** unformatted file. The Phase 4 comparison set is
therefore empty, so P4-T2 must likewise report zero unformatted files for its gate to pass. CSharpier
1.2.6 was resolved from the repository-root `dotnet-tools.json` manifest via `dotnet tool run`.
