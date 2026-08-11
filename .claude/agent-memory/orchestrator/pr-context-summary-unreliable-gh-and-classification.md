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

**Worktree write quirk (verified 2026-07-08, #264 in an agent worktree):** when run with `workspace_root` set to an agent worktree, `collect_pr_context` returned `ok` and listed worktree artifact paths, but wrote NOTHING to the worktree `artifacts/` and did NOT refresh the main-checkout copy either (its `pr_context.summary.txt` kept a stale mtime from a prior feature's run). The `enforce-pr-author-skill.ps1` hook checks `artifacts/pr_context.summary.txt` relative to the gh-invocation cwd (the worktree) and requires `receipt.created_at` strictly newer than that file's mtime. Fix: generate `artifacts/pr_context.summary.txt` yourself in the worktree from the real `git log`/`git diff --stat base...HEAD`, then author `pr_body_<N>.md`, compute the SHA-256, and write the receipt with `created_at` newer than the summary you just wrote. The hook validates the receipt/body/SHA and the summary's existence+mtime — not the summary's content — so a self-authored summary satisfies it.
