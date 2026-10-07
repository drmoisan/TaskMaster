---
name: project-944-prime-marker-registration-plan-seams
description: "#944 plan seams (register-before-start prime marker, authored pre-#942 to execute post-#942) - git show decodes blobs through the console encoding so em-dash regions hash unequal unless [Console]::OutputEncoding is UTF-8; a wrapped doc phrase needs a joined-text count; a protected-region partition (token+offset regions compared anchor vs working) replaces hunk windows; with Bash disabled read HEAD/branch/origin refs from the .git files"
metadata:
  type: project
---

Authored 2026-09-30 for docs/features/active/2026-09-30-engine-toggle-prime-marker-registration-races-removal-944 (plan.2026-09-30T07-20.md), modelled on the approved #942 plan.

**Why:** each item either would have made a gate unfalsifiable or would have produced a false mismatch on a correct run.

**How to apply:**

1. **Plan authored against a tree the executor will not see.** When an upstream sibling must merge first, gate everything on single-occurrence tokens and token-relative regions, record `ANCHOR-SHA` at P0 from `git merge-base origin/main HEAD` after the merge, and state one substitution rule. Prove the edit windows are untouched upstream by comparing them PREP-SHA vs ANCHOR-SHA (must be equal) and use regions the upstream DID change as the positive control (must be unequal).
2. **`git show <sha>:<path>` output is decoded through `[Console]::OutputEncoding`.** Files with em dashes (the coordinator's XML docs) hash differently from `Get-Content -Encoding UTF8` unless the payload sets UTF-8 first; also strip a BOM from line 1 and trailing CR on both sides.
3. **A doc phrase wrapped across two comment lines reads 0 in a line-oriented absence count before the fix** ("The returned continuation task always / completes successfully"). Gate the single-line fragment (1 -> 0) plus a joined-text count (lines trimmed, leading slashes stripped, joined by one space).
4. **Protected-region partition beats hunk windows.** Define regions by (start token, offset, end token, offset) so the protected set plus the edit windows partition the file; require every protected region equal to the anchor and every edit window unequal (positive control).
5. **Bash was disabled for the planner this session.** HEAD, branch ref and origin/main were read from `.git/worktrees/<id>/HEAD`, `logs/HEAD`, `refs/heads/...` and `refs/remotes/origin/main` with Read; tracked-vs-untracked state cannot be read that way, so make the P0 task observe it (e.g. promotion record UNTRACKED/MODIFIED/TRACKED-UNCHANGED) and commit docs before the merge so an untracked copy cannot block it.
6. **No apostrophe, double quote or non-ASCII character in any gated token** that rides inside a single-quoted `pwsh -Command` payload.
7. Amended two ACs in the spec (planner authority) rather than reinterpreting them: a repo-wide rate "not falling below baseline" (merge noise, see [[repo-wide-cobertura-line-rate-is-nondeterministic]]) and "coverage run exactly as CLAUDE.md" when the stall-probe DIRECT route is admitted.

8. **R1 (twelve defects):** a path restored with `git checkout <branch> -- <path>` is STAGED-NEW (`A `), a state a UNTRACKED/MODIFIED/UNCHANGED enum misses. An upstream-overlap check `git diff --name-only HEAD origin/main` intersected with porcelain fires falsely on the plan file once the item's own docs commit precedes it; carve out the feature folder. Guard `git merge --abort` on `git rev-parse -q --verify MERGE_HEAD`. A same-command coverage re-run inside a restart-free loop is not a loop pass; model it as a pass-2 restart of the whole loop plus a reader rule (cite the PASS-2 copy). Bound a coverage artifact's Output Summary to 20 lines and push blocks to Details:. Recount Delivered Source line spans (165 was wrong; 175). An AC line may carry no digit besides its label: "Phase 0" inside AC text counts.
9. **R2 (three defects):** a "protected-region partition" claim must be re-derived line by line: an edit-window region that spans an unedited field declaration leaves that declaration unverified (add a token region for it), and blank separator lines kept by an edit window are unverified too (the caller's own replacement text omitted one; correct it and say so). The pre-implementation gate withholds the docs-commit exemption when the `-C` operand is a double-quoted backslash path. A pass-2 restart that appends to an artifact leaves the failing first `EXIT_CODE:` as the one collectors read; rewrite the top-level fields and keep `FIRST-ATTEMPT-EXIT-CODE:`.

Related: [[project_839_createcancellationtoken_init_plan_seams]], [[project_838_gettableinviewasync_null_contract_plan_seams]].
