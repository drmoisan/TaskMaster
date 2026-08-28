---
name: per-child-ci-never-gates-integrated-tree
description: A child's CI run gates the head it was dispatched on, which predates every sibling that merges afterwards — so the combined tree goes untested; dispatch ci.yml against the integration branch after each merge
metadata:
  type: feedback
---

After any child merge that combined its work with siblings merged since its CI dispatch, run
`gh workflow run ci.yml --ref <integration-branch>` and require it green. **Per-child CI gates never
test the integrated result.**

**Why:** On the quickfiler-bug-family epic, feature 501 dispatched CI on head `d86ee40d` and passed.
Sibling 476 then merged (`5793b8c7`). 501 merged anyway — GitHub reported MERGEABLE, so there was no
conflict — producing `4cb709db` with parents `5793b8c7` and `d86ee40d`. I verified with
`git merge-base --is-ancestor` that **`5793b8c7` is not an ancestor of `d86ee40d`**: 476's code was
absent from the only tree 501's CI ever tested. The combination first existed at the merge commit and
had never been validated. Worse, `gh run list --branch <integration>` returned **empty** — across
*eight* merged features the integration branch had never been CI-tested once.

This is structural, not a child's mistake. A child's head is fixed at dispatch time; every sibling
that lands afterwards is invisible to it. Telling a child to "re-dispatch if the base moves" helps but
is racy and, in this case, was simply not done.

**How to apply:** Treat the per-child gate as necessary but not sufficient. Gate the integration
branch itself after merges, not only at the final integration PR — by then attribution across a dozen
features is hardest. Note the direction of the check: `--is-ancestor <new-tip> <tested-head>` answers
"did the tested tree contain the sibling's work", which is what you actually want to know. Related:
[[child-pr-ci-gap-integration-base]], [[cross-child-annotation-fanin-debt]].
