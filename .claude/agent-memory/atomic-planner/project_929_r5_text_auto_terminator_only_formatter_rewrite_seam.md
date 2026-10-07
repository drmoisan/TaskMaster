---
name: project-929-r5-text-auto-terminator-only-formatter-rewrite-seam
description: "#929 revision 3.2 (preflight round 5 delta D-1): a formatter-commit branch ('keep the rewrite, commit it, record the SHA') is unsatisfiable when the rewrite changed line terminators only, because .gitattributes line 4 sets * text=auto and git add normalises the terminators away, so git commit exits 1 with nothing to commit; key the branch on git diff --numstat HEAD -- <members> (non-empty = commit, empty = record FORMAT-REWRITE-TERMINATOR-ONLY and restart without a commit) and sweep every sibling that says 'that rewrite is committed at'"
metadata:
  type: project
---

Seam from applying the executor's round-5 preflight delta D-1 to plan 929 (worktree agent-a74dcedbc13b789fd) on 2026-09-30, no Bash in the planner session.

**A hash-detected formatter rewrite is not always a committable change.** TaskMaster's `.gitattributes` line 4 is `* text=auto`. When a PoshQC (or any) formatter rewrites a tracked file and the only byte change is CRLF to LF (or the reverse), `git add` normalises the terminators on check-in, the index blob is unchanged, and `git commit -m ... -- <members>` exits 1 with "nothing to commit". A plan branch that says "commit the rewritten members and record that commit SHA" therefore has no satisfiable outcome in that case. SHA-256 hash sets and porcelain disagree here: the hash changes, porcelain (which applies the clean filter) stays empty.

**Shape that works.** After a non-zero Write Set rewrite count, run `git diff --numstat HEAD -- <those members>` and record it verbatim (`REWRITE-NUMSTAT:`). Non-empty: the existing branch (re-run the test task, commit, record the SHA, restart the loop). Empty: record `FORMAT-REWRITE-TERMINATOR-ONLY:` with the member list and restart the loop without a commit. State where the post-revert porcelain clause is evaluated in each branch. The branch must key on the numstat output, not on the working-copy terminator state: a blob that already carries CRLF in the index is left unnormalised by text=auto, so the same rewrite is a real diff there and takes the commit branch.

**Sibling sweep.** Every sentence of the form "that rewrite is committed at <task>" (Phase 0 baseline format step plus its evidence commit, the residual-risk register, the handoff index's commit list) must admit the no-commit case. A Phase 0 evidence commit still succeeds because evidence is always added; the formatter member simply stages nothing, so "git show lists outside the folder only the members listed" is satisfied vacuously and the plain commit message applies. "any <task> formatter commit" phrasing already admits absence and needs no edit.

**How to apply:** for any plan with a formatter-output-wins commit branch on a repository carrying `* text=auto` (check `.gitattributes` first), gate the commit on `git diff --numstat HEAD` being non-empty, name the terminator-only record, and sweep every sibling that asserts the commit happens. Do not assert in plan text that working-copy files are CRLF unless observed; the numstat key makes that fact non-load-bearing.
