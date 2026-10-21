# P4-T1 — Toolchain step 1 of 4: formatting

Timestamp: 2026-09-01T20-12
Command: `git status --porcelain -- QuickFiler QuickFiler.Test`, then `dotnet tool run csharpier format .`, then `git status --porcelain -- QuickFiler QuickFiler.Test` again
EXIT_CODE: 0

## The before-and-after tree comparison

    BEFORE:  git status --porcelain -- QuickFiler QuickFiler.Test
             (no output)

    AFTER:   git status --porcelain -- QuickFiler QuickFiler.Test
             (no output)

Both listings print nothing. The repo-wide formatter rewrote **no file** under `QuickFiler/` or `QuickFiler.Test/`.

The "before" listing is empty because P3-T15 committed every source edit, which is what makes this comparison meaningful: any output in the "after" listing would be attributable to this formatter run alone and to nothing else.

## Why the exit code is not the observation

`csharpier format` is write-mode. It exits 0 whether or not it rewrote tracked source, so its exit code is identical on a clean run and on a repairing run and cannot distinguish the two. Its summary line has the same property:

    Formatted 1567 files in 4791ms.

That line reports the number of files **processed**, not the number rewritten, so it reads identically on a clean tree and on a drifted one. It is reproduced verbatim as the task requires, but the tree comparison above is the actual observation.

## Whole-tree confirmation

A tree-wide porcelain listing was also taken, to confirm the formatter rewrote nothing anywhere rather than merely nothing in the two directories under gate:

    M  docs/features/active/2026-08-28-qfc-initializewebviewasync-fault-is-unobserved-670/plan.2026-08-31T20-20.md
    ?? docs/features/active/2026-08-28-qfc-initializewebviewasync-fault-is-unobserved-670/evidence/other/p3-t15-implementation-commit.md

Neither entry is a formatter effect. The first is the plan checklist, modified by this delivery run's own check-offs after the Phase 3 commit; the second is the P3-T15 evidence artifact, written after that commit by design. No `.cs`, `.xml` or `packages.config` file appears, so the repo-wide format was a genuine no-op across the whole tree.

## Why this is a no-op

Three earlier actions make it so, and each was sequenced deliberately:

- **P0-T9** captured a read-only CSharpier baseline **before** any write-mode formatter ran, and found the tree entirely clean — 1566 files checked, zero named. Had drift existed elsewhere in the repository, this repo-wide `format` would have repaired it and the resulting rewrite would have been indistinguishable from a rewrite of this plan's own files.
- **P1-T5** normalized the newly authored production file, which the formatter did rewrite at that point.
- **P3-T12** confirmed both touched test files were already formatter-stable.

By the time this task ran there was nothing left for the formatter to do, which is precisely the condition the plan's Phase 4 pass requires and the condition that makes the zero-changed-lines gate on `ViewerSetup.cs` in P2-T4 hold through to the end.

Base-ref note: this task states no `git diff` against a ref. The re-anchored base used throughout this delivery run is `988d35a8f8eb7436cc46a9f6424db917ed93807a`, replacing the plan-pinned `2b85134b42872e405602e6064e02dc9cda6c319b`, which is a stale ancestor rather than the current merge base.
