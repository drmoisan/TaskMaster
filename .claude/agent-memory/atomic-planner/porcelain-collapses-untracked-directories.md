---
name: porcelain-collapses-untracked-directories
description: git status --porcelain collapses a wholly untracked directory to one entry, so a gate that must enumerate individual untracked evidence artifact paths needs --untracked-files=all
metadata:
  type: feedback
---

`git status --porcelain` reports a wholly untracked directory as a single entry (`?? path/to/dir/`). It does not list the files inside it. Use `git status --porcelain --untracked-files=all` whenever an acceptance condition must enumerate individual untracked paths.

**Why:** an active feature folder is entirely untracked at plan start, so every evidence artifact a plan writes lives inside a directory that plain porcelain collapses. A gate worded "the recorded `git status --porcelain` output must list every `EVIDENCE/<kind>/` artifact path this plan names" is then unsatisfiable — the output contains one directory line and zero artifact lines, whatever the executor does.

**How to apply:** the two forms are for different jobs and a plan usually needs both.

- Use the **collapsed default form** when the gate asserts a *negative* over the whole tree ("no path outside the write set plus the disclosed baseline"). The feature folder is one entry already covered by the disclosed baseline, so evidence artifacts written into it during execution do not have to be re-enumerated and do not trip the gate.
- Use **`--untracked-files=all`** when the gate asserts a *positive* over specific untracked files (every named evidence artifact is present) or when a Phase 0 baseline record needs a precise, per-file disclosed set.

A Phase 0 tree-invariants task should record both forms so later gates can reference whichever one they need. Related: [[diff-gates-need-a-commit-task]], [[agent-memory-is-tracked-scope-git-gates]] and the `git add -N` companion rule in [[untracked-file-and-linecount-gate-seams]].
