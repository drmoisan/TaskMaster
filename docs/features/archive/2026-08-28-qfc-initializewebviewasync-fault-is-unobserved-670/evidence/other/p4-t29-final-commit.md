# P4-T29 — Final commit

Timestamp: 2026-09-01T20-24
Command: `git add QuickFiler QuickFiler.Test docs/features/active/2026-08-28-qfc-initializewebviewasync-fault-is-unobserved-670`, then `git commit`, then `git rev-parse HEAD`, `git status --porcelain -- QuickFiler QuickFiler.Test`, and `git diff --name-status 988d35a8f8eb7436cc46a9f6424db917ed93807a HEAD -- QuickFiler QuickFiler.Test`
EXIT_CODE: 0

## Commit SHA

    88969efa

Commit subject: `docs(issue-670): record the Phase 4 QA gates and check off the acceptance criteria`

Nineteen files changed. The large insertion count is dominated by `evidence/qa-gates/postchange.cobertura.xml`, which is a generated coverage document.

This is the first of the two commits this task makes. It carries the Phase 4 QA-gate artifacts, the `evidence/other` records for P3-T15, P4-T26, P4-T27 and P4-T28, the post-change Cobertura document, the fourteen `spec.md` acceptance-criteria check-offs, and the plan checklist state through P4-T28.

## Acceptance conditions

**Condition 1 — the two source directories are clean:**

    git status --porcelain -- QuickFiler QuickFiler.Test
    (no output)

**Condition 2 — the changed-file set is still exactly the five paths P4-T11 enumerated:**

    M	QuickFiler.Test/Controllers/QfcItemController.InitializationTests.Part3.cs
    M	QuickFiler.Test/Controllers/QfcItemController.InitializationTests.cs
    M	QuickFiler/Controllers/QfcItemController.Initialization.cs
    A	QuickFiler/Controllers/QfcItemController.WebViewFaultBoundary.cs
    M	QuickFiler/QuickFiler.csproj

Five paths, unchanged from the P4-T11 measurement. This commit touched only the feature folder, so it could not have altered the source set — and the re-run confirms it did not.

**Condition 3 — this artifact names the first commit's SHA.** It is `88969efa`, recorded above.

## The second commit

After this artifact is written and P4-T29 is marked `[x]` in the plan, a closing `git add` and `git commit` carry both. The second commit's SHA is reported in the executor's completion message rather than written into a file, because a checkbox cannot gate a tree state that the act of ticking it invalidates: recording that SHA here would leave this file dirty again the moment it was written, and the fixpoint would never close.

## Base-ref substitution

The plan's stated `git diff` names `2b85134b42872e405602e6064e02dc9cda6c319b`. That SHA is superseded and is a stale ancestor rather than the current merge base, so `988d35a8f8eb7436cc46a9f6424db917ed93807a` was used. Rationale and the measurement showing the superseded SHA reports 22 paths where this gate demands five: `evidence/baseline/p0-t7-base-ref.md` and `evidence/qa-gates/p4-t11-changed-file-set.md`.

## Scope of this delivery run

No branch was created, nothing was pushed, no pull request was opened or edited, and no merge was performed. Both commits are local to `bug/qfc-initializewebviewasync-fault-is-unobserved-670`. No `git update-index` command was run at any point, and `artifacts/orchestration/orchestrator-state.json` was never written.
