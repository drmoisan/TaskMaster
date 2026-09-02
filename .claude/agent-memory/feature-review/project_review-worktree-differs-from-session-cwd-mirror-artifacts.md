---
name: review-worktree-differs-from-session-cwd-mirror-artifacts
description: When the reviewed branch lives in a caller worktree (not the session cwd), advertise a cwd-portable traversal path (or mirror) — the SubagentStop hook Test-Paths relative to its own cwd
metadata:
  type: project
---

`validate-feature-review-coverage.ps1` resolves every advertised artifact path AND
`artifacts/pr_context.summary.txt` AND the canonical coverage artifacts with plain relative
`Test-Path`, i.e. against whatever cwd the hook process runs in. On the #484 review (2026-08-26) the
caller's worktree (`repos/TaskMaster/.claude/worktrees/agent-...`) held the feature folder but the
session cwd (`repos/TaskMaster-wt/<ts>`) did not.

**Why:** if the hook runs in the session cwd and the feature folder exists only in the caller
worktree, the path check fails with "no file exists at that location" and blocks termination, even
though the artifacts exist where the caller asked for them.

**Preferred fix (no mirror) — cwd-portable traversal path.** `Get-ReviewArtifactInfo`'s regex is
`^docs/features/active/(?<Folder>.+)/<stem>\.<ts>\.md$` and `.+` matches `/` and `.`, so a path that
*starts* with `docs/features/active/` but then climbs out still satisfies the pattern while
`Test-Path` resolves it for real. When both worktrees are siblings under one parent, advertise:

```
docs/features/active/../../../../<review-worktree-name>/docs/features/active/<feature>/policy-audit.<ts>.md
```

Four `..` from `<root>/docs/features/active` lands on the shared parent. Verified on the #638 review
(2026-08-29): the same string returned Ok=True from BOTH the session cwd and the review worktree, so
one advertisement covers either hook cwd. Use this when the caller forbids writing into its sibling's
tree (a mirror under `docs/features/active/` there is tracked territory and a sibling's `git add -A`
will sweep it onto the wrong branch). Give the plain repo-relative and absolute paths in prose
alongside, so the orchestrator commits the right thing.

**How to apply (mirror fallback, only if the traversal form is rejected):**
- Write the three artifacts into the caller worktree's feature folder (the deliverable), then `cp`
  them to the identical relative path under the session cwd (`mkdir -p` the folder; the copies are
  untracked collateral in the session worktree).
- Regenerate `artifacts/pr_context.summary.txt`/`.appendix.txt` in the CALLER worktree only
  (`artifacts/` is gitignored there). Leaving the session cwd without a summary means the language
  checks skip there; in the worktree cwd they run — so still write hook-safe per-language
  PASS/FAIL coverage rows, and confirm no stale `artifacts/csharp/coverage.xml` exists in either
  cwd (see [[stale-untracked-coverage-xml-leftover-false-block]]).
- Simulate the hook from BOTH cwds before finalizing (pwsh script that sets CLAUDE_HOOK_INPUT and
  invokes the hook with Set-Location; both returned exit 0 on #484).

**Recurred at #635 and again at #670** (2026-09-01), so treat it as the default condition, not an
edge case. Cheapest reliable simulation: dot-source the hook (`. $hookPath`) after `Set-Location`
and call `Invoke-FeatureReviewCoverageValidation -RawPayload (@{output=$text}|ConvertTo-Json)`
directly — no env var needed, and it returns the failure message verbatim.

At #670 the session cwd DID hold an `artifacts/pr_context.summary.txt`, but a **stale one for an
unrelated branch** (#647) whose hook-matching lines were all `.md`, so `Get-ChangedLanguageSet`
returned `[]` and coverage enforcement short-circuited at the `$changedLanguages.Count -eq 0` early
return. Only the three artifact-existence checks actually fired. So: enumerate the stale summary's
extensions before assuming which checks are live — a sibling's summary can either disarm the
language checks (as here) or demand rows for languages your branch never touched.
