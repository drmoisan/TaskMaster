---
name: closing-keyword-fires-inside-negation
description: GitHub parses "fix #N"/"close #N" in commit messages and PR bodies even inside a negation like "does NOT fix #511" - scan and rewrite before merging to the default branch
metadata:
  type: feedback
---

GitHub's auto-close parser matches `close|closes|closed|fix|fixes|fixed|resolve|resolves|resolved`
followed by `#N`. It does **not** understand negation. A commit message reading
`this commit does NOT fix #511 or #571` contains the literal `fix #511` and will close #511 when
the commit lands on the default branch.

**Why:** On #511 two commits already on the branch said `does not claim to close #511` and
`does NOT fix #511 or #571`. Both were written specifically to disclaim a repair. Merging them to
`main` would have auto-closed #511 and could have flipped its `state_reason` from `NOT_PLANNED` to
`COMPLETED` — the exact false claim the whole branch existed to avoid. Caught only by an explicit
regex scan of the commit messages before the PR was opened.

**How to apply:**

- Scan before opening any PR: `git log f<base>..HEAD --format=%B | grep -Ein '(clos(e|es|ed)|fix(|es|ed)|resolv(e|es|ed))[[:space:]:]*#[0-9]+'`, and scan the PR body the same way.
- Write `is not delivered by`, `makes no repair claim for`, or `#N is not addressed here`. Never
  put a keyword stem immediately before the reference, even negated.
- **Scope:** GitHub parses commit messages and PR/issue bodies only. File CONTENTS are never
  parsed, so a `Fix #571` sitting in a committed plan or spec is harmless and does not need editing.
  Do not waste a cycle "fixing" prose inside files.
- To repair history non-interactively (interactive rebase is unavailable in this environment):
  `FILTER_BRANCH_SQUELCH_WARNING=1 git filter-branch -f --msg-filter 'sed -e "s/old/new/g"' <base>..HEAD`,
  then verify the tree is byte-identical with `git diff --stat <backup-ref> HEAD` before force-pushing.
- `collect_pr_context` makes this worse, not better: its "Author-asserted autoclose issues" list is
  scraped from text and listed #511 and #571 for a PR that must not close either. Never copy that
  list into a body; when GitHub validation is unavailable the skill's own fallback is a `None` bullet.

See [[project-epic-child-prs-no-ci]] for the related base-branch decision.
