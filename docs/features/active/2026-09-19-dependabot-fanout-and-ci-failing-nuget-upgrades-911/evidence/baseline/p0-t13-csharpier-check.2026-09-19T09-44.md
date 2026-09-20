# P0-T13 — CSharpier Formatter Baseline

Timestamp: 2026-09-19T22-55

Command:

```
dotnet tool run csharpier check .
```

EXIT_CODE: 0

## Verbatim output

```
Checked 1658 files in 4377ms.
```

`N` recorded as an integer: **1658**.

## Files reported with findings

None. The command emitted no per-file finding line at all: its entire output is the single
`Checked ` summary line quoted above.

Per gate rule 6, `Checked N files` is the **scanned** count, not a rewrite or finding count, so the
1658 figure is not evidence of a clean tree by itself. The clean-tree evidence is the exit code 0
paired with the empty finding list: CSharpier exits non-zero and names each offending path when a
file is not formatted.

## Scope note for the later `.csharpierignore` change

This baseline is the pre-change scan population. P1-T2 adds `**/packages.config` and `**/app.config`
to `.csharpierignore`, which removes the 18 manifests and 17 `app.config` files from the scanned
set, so the `N` recorded by P2-T4 is expected to be lower than 1658. That drop is the intended
effect of the change and not a regression; P2-T4's acceptance is exit 0 with zero files reported,
not a fixed `N`.

## Acceptance evaluation

- The artifact records `EXIT_CODE:` as returned — **0**. PASS.
- The verbatim `Checked N files in Xms.` line is recorded with `N` as an integer — **1658**. PASS.
- The full list of files reported with findings is recorded — the list is empty and is recorded as
  such, alongside the exit code that makes the empty list meaningful. PASS.
- The failing condition is reachable: an absent `Checked ` line would mean the command did not run.
  The line is present.

Output Summary: CSharpier check returned EXIT_CODE 0 and printed
`Checked 1658 files in 4377ms.`, with no file reported with findings. The C# tree is formatter-clean
at the merge-base. The scanned count of 1658 is expected to fall at P2-T4 once P1-T2 excludes the 35
manifest and `app.config` files from the scan.
