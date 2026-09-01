# P0-T6 — Formatting Baseline (read-only)

Timestamp: 2026-09-01T08-05

Command: `dotnet tool run csharpier check .` (run from the repository root)

EXIT_CODE: 0

Output Summary: The check-mode, non-writing invocation exited 0 and reported no unformatted files.
The complete captured output was a single summary line:

```text
Checked 1565 files in 4683ms.
```

Note on the summary-line wording: in check mode CSharpier 1.2.6 prints a `Checked <n> files` summary
rather than a `Formatted <n> files` summary. `Formatted` is the wording emitted by the write-mode
`csharpier format` subcommand, which P1-T4, P2-T2, and P3-T1 use. The line recorded verbatim above is
the summary line this check-mode invocation actually printed. 1565 files were examined.

Because this is a check-mode invocation, the exit code alone is a discriminating observation: a
non-zero exit is CSharpier's signal that at least one file needs formatting. The exit code here is 0.

## Unformatted-File List

**The unformatted-file list is the EMPTY LIST. Cardinality: 0.**

CSharpier emitted no per-file diagnostic line and no file path. The tree is already fully formatted
at the merge base plus the current working state.

This empty list is consumed by three later tasks:

- **P3-T1** — the porcelain output after the repository-wide format pass may list no path outside the
  three in-scope paths, `.claude/agent-memory/`, and this list. Since this list is empty, it
  contributes no allowance.
- **P3-T11** — the exclusion set is exactly `.claude/agent-memory/` plus this list. Since this list is
  empty, the exclusion set reduces to `.claude/agent-memory/` alone, and the cardinality of the
  P0-T6 list to be stated in that artifact is **0**.
- **P4-T14** — same exclusion set and same cardinality of **0**.

Neither `UtilitiesCS.Test/Threading/TimeOutTask_AdditionalTests.cs` nor
`UtilitiesCS.Test/Threading/TimeOutTask_OverloadCoverageTests.cs` appears in this list, because the
list is empty. The conditional P2-T6 re-run that P3-T1 describes is therefore not triggered.

Acceptance: met. The artifact records the exit code (0) and an explicit unformatted-file list, which
is the empty list because the tree is already clean. No enumeration of paths is required, because
there are none.
