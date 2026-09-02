---
name: pr-context-summary-unreliable-gh-and-classification
description: artifacts/pr_context.summary.txt can wrongly report "GitHub CLI unavailable" and misclassify C# production files as docs; verify gh and the diff directly
metadata:
  type: feedback
---

`mcp__drm-copilot__collect_pr_context` writes `artifacts/pr_context.summary.txt`, but two of its fields are unreliable and must be independently verified:

1. **"GitHub CLI unavailable"** — the summary reported gh was not installed, but `which gh` showed gh 2.87.3 installed and authenticated (account drmoisan). The collector's gh probe runs in a shell/PATH context that does not match the Bash tool's. Do not conclude the PR cannot be created from this line. Verify with `which gh && gh auth status` directly before deciding to hand PR creation back to the user.

2. **"Changed files overview: Core logic changes: 0 files / Docs N files"** — on issue #222 it classified the 5 changed C# production controllers as docs and reported 0 core-logic changes. feature-review independently flagged the same misclassification. Author the PR body from the real `git diff` (the appendix carries it), not from the summary's classification.

**Why:** Acting on the summary's "gh unavailable" line would wrongly stop the workflow before PR creation; acting on its file classification would mis-describe the PR. Both fields were verifiably wrong on #222 while the rest of the bundle (base/head/merge-base, autoclose candidates, evidence list) was correct.

**How to apply:** Trust the bundle for base/head/merge-base, autoclose candidates, and the additional-context-file enumeration. For PR creation feasibility, run `which gh && gh auth status`. For the PR body's "What Changed", read the actual diff. Note the autoclose list may also contain non-issue tokens (e.g. `#COV-001`, an exception ID) — emit `Closes` only for the real canonical issue number.

**Stale-content variant (verified 2026-08-10, #394 in an agent worktree):** `collect_pr_context` returned `ok:true` and listed the worktree artifact paths, but did NOT overwrite an existing `artifacts/pr_context.summary.txt` that a `feature-review` subagent had hand-authored earlier in the same run. The stale file still described a file the remediation cycle had since deleted, so authoring the PR body from it would have described the wrong diff. Always read the summary after the collector call and check its head/merge-base SHAs against `git rev-parse HEAD` / `git merge-base`; if they do not match, regenerate the summary yourself before authoring.

**Wrong-branch head resolution (verified 2026-08-27, epic child 442).** Run with `workspace_root` set to the **session root** while the feature lived in a separate worktree, the collector resolved `Head ref` to the *session* worktree's own scratch branch (`TaskMaster-wt-2026-08-23T22-51 @ 096f8493`) instead of the target branch (`bug/quickfiler-home-controller-metrics-442 @ 1fd417dd`), reported the merge base against `main`, and then described a **different feature entirely** — it named sibling `quickfiler-bug-family-446` and listed that sibling's three audit files as the whole changed-file set. It also repeated the false "GitHub CLI unavailable" line. Every substantive field was wrong; only the requested/resolved *base* ref was right. The collector resolves the head from the workspace_root's `HEAD`, so it cannot describe a branch that lives in a different worktree. Verify the summary names YOUR branch and YOUR head SHA before using any of it, and author from the real `git diff <base>...HEAD` when it does not.

**Acceptance-criteria IDs scraped as issue numbers (verified 2026-09-01, #656).** The
author-asserted autoclose list contained **twenty non-issue tokens** `#AC-1` through `#AC-20`, which
are the acceptance-criterion identifiers in `spec.md`, alongside four unrelated real issues (462,
488, 500, 501) harvested from the spec's cross-references. The scraper matches `#<token>` in the
feature documents with no check that the token is numeric or that the issue belongs to this item. On
#656 all four real issues happened to be already closed, so the error was benign; on an earlier item
the same list named an issue that was OPEN and explicitly out of scope. Verify every number with
`gh issue view <n>` and emit `Closes` only for this item's own issue. Note the same run again
repeated the false "GitHub CLI unavailable" line while `gh` worked — that is now five consecutive
items.

**The docs-misclassification silently disables the C# coverage gate (verified 2026-09-01, #663).** This is not merely cosmetic. `.claude/hooks/validate-feature-review-coverage.ps1` derives its changed-language set by scanning `artifacts/pr_context.summary.txt` for extensions (`.cs` -> CSharp, `.ps1`/`.psm1` -> PowerShell, and so on) and then requires the policy audit to carry a coverage-scoped PASS/FAIL verdict for each language found. On #663 the summary reported `Core logic changes: 0 files` and classified all three changed `.cs` files as documentation, so they never appeared in the changed-files overview at all; `feature-review` confirmed by dot-sourcing the hook that `changedLanguages` comes back **empty** and the C# coverage requirement is skipped without any diagnostic. Treat a `feature-review` run that reports no coverage verdict as unproven rather than as clean, and instruct the reviewer explicitly to record a CSharp coverage PASS or FAIL.

**Non-issue tokens in the autoclose list are the norm, not an edge case (2026-09-01, #663).** The `Author asserted` list carried 22 entries of which exactly one (`#663`) was a real in-scope issue. The rest were acceptance-criterion identifiers scraped from prose (`#AC-1` through `#AC-15`), verification-command labels (`#VC-1`, `#VC-2`), an evidence-convention token (`#SHA-256`), issues cited only as precedent (`#464`, `#467`, `#469`), and one issue (`#713`) that was OPEN and documented as explicitly out of scope. Verify each number with `gh issue view <N>`. When you have that direct verification, do **not** fall back to the skill's no-`Closes`-bullet rule: that fallback exists to protect against closing an UNVERIFIED issue, not a verified one. Confirm the result afterwards with `gh pr view <PR> --json closingIssuesReferences`, which reads GitHub's own parse of the body and is the authoritative check.

The bundle is still worth generating: the `enforce-pr-author-skill.ps1` hook checks that `artifacts/pr_context.summary.txt` **exists** and that `receipt.created_at` is strictly newer than its mtime, not that its content is correct. Generate it first, then write the body and receipt.

**Worktree write quirk (verified 2026-07-08, #264 in an agent worktree):** when run with `workspace_root` set to an agent worktree, `collect_pr_context` returned `ok` and listed worktree artifact paths, but wrote NOTHING to the worktree `artifacts/` and did NOT refresh the main-checkout copy either (its `pr_context.summary.txt` kept a stale mtime from a prior feature's run). The `enforce-pr-author-skill.ps1` hook checks `artifacts/pr_context.summary.txt` relative to the gh-invocation cwd (the worktree) and requires `receipt.created_at` strictly newer than that file's mtime. Fix: generate `artifacts/pr_context.summary.txt` yourself in the worktree from the real `git log`/`git diff --stat base...HEAD`, then author `pr_body_<N>.md`, compute the SHA-256, and write the receipt with `created_at` newer than the summary you just wrote. The hook validates the receipt/body/SHA and the summary's existence+mtime — not the summary's content — so a self-authored summary satisfies it.
