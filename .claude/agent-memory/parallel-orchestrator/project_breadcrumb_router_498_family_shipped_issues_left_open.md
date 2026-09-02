---
name: breadcrumb-router-498-family-shipped-issues-left-open
description: Fifth multi-issue family — #499 (and siblings #440, #439) shipped under the docs/features/active/breadcrumb-router-navigation-defects-498 folder with 31/31 ACs checked, yet #499 is still OPEN; here the fix subject names the issue number directly, proving even that does not close it
metadata:
  type: project
---

`docs/features/active/breadcrumb-router-navigation-defects-498` is a multi-issue feature folder whose
spec.md line 4 reads `**Also closes:** #440, #499`. Its spec carries **31 acceptance criteria, all
`[x]`, zero unchecked** on `origin/main` — so the family has no residual scope of any kind.

**#499 is delivered and still OPEN.** Commit `0c9dcf42 fix(quickfiler): clear stale
SelectedFolderPath on re-bind (#499)` is an ancestor of `origin/main`. The guard site confirms it:
`BreadcrumbBridgeRouter.BindRowsAsync` on `main` now runs `_selectedRowId = null;` followed by the
guarded clear that raises `SelectedFolderPathChanged` with null only when the previous value was
non-null. ACs AC-4, AC-5, AC-6, AC-26 (the four the commit claims) are all `[x]`.

**Why this instance matters beyond being a fifth data point:** the delivering commit names the issue
number **directly in its subject line**, in parentheses — the easiest possible case for the
bare-number grep, and the opposite of
[[breadcrumb-coordinator-501-family-shipped-issues-left-open]], where the subject named neither the
issue nor the slug. It still left the issue OPEN, because a parenthesized `(#499)` carries no closing
keyword. That rules out "the reference was too obscure for GitHub to act on" as the explanation and
leaves only the structural one: **this repository does not use closing keywords, so issue state is
decoupled from delivery across the board.** Treat an OPEN issue as carrying no information about
whether its work shipped.

**Why:** Confirms the pattern is repository-wide rather than a property of hard-to-find commits.

**How to apply:** Continue running the bare-number grep first on every `/parallel-add` candidate per
[[verify-delivery-before-preparing-an-admission]]. A subject-line issue reference is a strong
delivery signal, not a weak one — go straight to the guard site. The other four families are
[[qfc-collection-468-family-shipped-issues-left-open]],
[[efc-464-family-shipped-issues-left-open]],
[[webview2-host-476-family-shipped-issues-left-open]], and
[[breadcrumb-coordinator-501-family-shipped-issues-left-open]].

A trap to avoid in this specific spec: line 78 is a scope-reconciliation table reading
`| #499 | **STILL IN SCOPE.** Unfixed. ...`. That is a snapshot taken at spec-authoring time, not
current status. The AC table at the end of the file is the current status; read that, not the
narrative table.
