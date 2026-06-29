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
