# QA gate — CSharpier check (read-only) ([P4-T2])

- Issue: #644
- Task: `[P4-T2]`
- Timestamp: 2026-08-29T08-15

**Restarted pass.** This artifact records the re-run triggered by the `[P4-T8]` net-line finding
described in `evidence/qa-gates/p4-t1-csharpier-format.2026-08-29T08-15.md`.

Command: `dotnet tool run csharpier check .`
Working directory: repository root (`<repo-root>`)
EXIT_CODE: 0

This is the read-only verify form and the CI-parity form. It exits non-zero and names each
unformatted file when the tree has drift, so exit 0 with no named file is a decisive observation
rather than an ambiguous one — unlike the write-mode `format` command gated in `[P4-T1]`.

Output:

```
Checked 1562 files in 4524ms.
```

## Acceptance

- The command **exits 0**.
- Its output **names no unformatted file**.

Both clauses hold, so the loop does **not** restart from `[P4-T1]`.

## Convergence

`[P0-T8]` checked 1561 files at the pre-change base and found no drift. This run checks 1562 — one
more, the new `QuickFiler.Test/Controllers/QfcCollectionControllerNavigationLedgerTests.cs` — and
likewise finds none. The comment condensation that triggered the restart was written in
already-formatted style, which is consistent with `[P4-T1]` having rewritten no file in this pass.

This artifact is the first of the four that `[P5-T16]` requires to belong to one uninterrupted pass
for AC-15. No step in this pass has rewritten a tracked file.

Output Summary: `dotnet tool run csharpier check .` checked **1562 files**, exited **0**, and named
**no unformatted file**. Formatting is clean across the whole repository. No loop restart required.
