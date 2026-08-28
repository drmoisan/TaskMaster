---
name: pr-author-receipt-staleness-is-mtime-vs-created-at
description: PR_AUTHOR_RECEIPT_STALE compares receipt created_at against the filesystem mtime of the session-root pr_context.summary.txt, so stage the summary BEFORE computing the timestamp
metadata:
  type: project
---

`PR_AUTHOR_RECEIPT_STALE` fires when `artifacts/pr_body_<N>.receipt.json`'s `created_at` is not
strictly newer than the **last-write mtime** of `artifacts/pr_context.summary.txt` — resolved
against the **session cwd**, not the feature worktree.

The trap: if you compute `created_at` and then copy the summary to the session root, the copy's
mtime becomes *newer* than the timestamp you just wrote, and the gate denies a receipt whose body
hash is perfectly valid. The body bytes are irrelevant to this particular denial.

**Why:** the check is a freshness ordering — it exists to prove the body was authored *after* the
PR context it claims to summarize. It reads mtime, so any file copy resets the comparison.

**How to apply:** stage `pr_context.summary.txt` (and the appendix) at the session root FIRST,
then read that staged file's mtime and set `created_at` to a few seconds past it. Re-copying the
summary afterwards re-breaks it. Only the receipt needs rewriting to repair — leave the body and
its SHA-256 alone. Relates to [[child-orchestrator-pr-hook-reads-session-root]] and
[[pr-context-summary-unreliable-gh-and-classification]].
