---
name: diff-gates-need-a-commit-task
description: A plan whose gates use `git diff <MERGE_BASE>..HEAD` passes vacuously unless the plan itself contains explicit commit tasks; and an "empty porcelain" Phase 0 acceptance is unsatisfiable when planning artifacts are already uncommitted
metadata:
  type: feedback
---

Any plan gate expressed as `git diff --numstat <MERGE_BASE>..HEAD` or `git diff --name-only <MERGE_BASE>..HEAD` verifies NOTHING unless the plan contains explicit commit tasks that advance HEAD past the merge-base. Pair every diff-based gate with a commit task earlier in the plan.

**Why:** #503 preflight (delta B1). The plan carried three diff-based gates (AC15 zero-line diff at P5-T1, the 500-line audit at P6-T3, and the post-format re-verification at P6-T10) but had no commit task anywhere. `git rev-parse HEAD` equalled the merge-base for the entire execution, so all three gates returned empty output and passed vacuously. Separately, Phase 0 asserted "an empty `git status --porcelain`" while 16 uncommitted lines of planning artifacts already existed, and Phase 0's own evidence tasks would add more — an unsatisfiable acceptance.

**How to apply:** Three rules when writing a plan with diff or worktree gates.

1. **Commit cadence.** Put a commit task at the end of Phase 0 (planning artifacts + baseline evidence), at the end of the last implementation phase (all source/test changes), and inside the final worktree-clean task. Each is a normal task with `EXIT_CODE: 0` and an evidence artifact under `<FEATURE>/evidence/`.
2. **Phase 0 porcelain is non-empty by construction.** Never write "empty `git status --porcelain`" as a Phase 0 acceptance. Record the verbatim output and make the binary outcome a *type* assertion instead: no `.cs`, `.csproj`, `.xml`, or `.sln` path appears. Docs/evidence/agent-memory paths are expected.
3. **Scope-lock diff gates must whitelist docs.** Once commits exist, the diff legitimately contains `docs/features/` and `.claude/agent-memory/` paths. Phrase the gate as "every path is a scope-lock member OR under docs/evidence" and keep the hard clause on source extensions only. In a companion file-size audit, state that Markdown docs are exempt from the 500-line cap per `.claude/rules/general-code-change.md`.

Related: [[never-pin-head-sha-as-plan-expectation]] — the fix is commit tasks plus tree-invariant gates, never a pinned SHA equality check.
