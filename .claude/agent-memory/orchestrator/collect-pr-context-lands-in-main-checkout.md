---
name: collect-pr-context-lands-in-main-checkout
description: From an isolated agent worktree, collect_pr_context returns ok:true but writes to the PRIMARY checkout and claims gh is unavailable - author the PR body from the real diff instead
metadata:
  type: project
---

Calling `mcp__drm-copilot__collect_pr_context` from a `.claude/worktrees/<agent-id>` worktree returns
`ok:true` and lists artifact paths **inside that worktree**, but the files it actually writes land in
the PRIMARY checkout (`C:\Users\DanMoisan\repos\TaskMaster\artifacts\`). The `workspace_root`
argument does not redirect it. Confirmed again 2026-08-22 (epic child #445): the returned paths had
an mtime ~10 minutes older than the call, while the primary checkout's copy was freshly written.

Two further defects make the artifact unusable rather than merely misplaced:

1. **It claims `gh` is unavailable** (`GitHub CLI unavailable: ... not installed`) when `gh auth
   status` in the same worktree authenticates fine. Never accept that claim; verify `gh` yourself.
2. **The primary checkout is on a different branch**, so the diff it computes is not your branch's
   diff at all. Copying it into the worktree would import a wrong changed-file list.

**Why:** a PR body built from that artifact misstates the change. In #445 the stale worktree copy
recorded a head SHA one commit behind and omitted all three review artifacts.

**Refinement (#327, 2026-07-16, dedicated agent worktree):** when the child orchestrator's session cwd IS the feature worktree (the harness gave me an isolated `.claude/worktrees/agent-<id>` worktree and I `git switch -c` the feature branch there), collect_pr_context wrote `pr_context.*` DIRECTLY into that worktree's `artifacts/` (returned paths were the worktree) and the hook read them there — no main-checkout copy step was needed. The main-checkout-landing behavior above applies when session cwd differs from the feature worktree. TWO quirks still bit: (a) collect_pr_context reported a `Head:` SHA one commit BEHIND my true branch tip and a second call did NOT rewrite the file (identical mtime) — do not trust the summary's `Head:` line; the pushed branch tip and GitHub's own base...head diff are authoritative, so it is harmless. (b) The receipt `created_at > summary mtime` check still held because I wrote the receipt after collect ran; no sleep was needed since minutes had elapsed. Child->integration PR #334 merged fine (merge commit 9559c73c) on blocking_count==0 with zero CI ([[project_epic_child_prs_no_ci]]).

**Quirk (a) is stronger than "does not rewrite on a second call" (#441, 2026-08-10).** `collect_pr_context` returned `ok:true` with worktree paths, but the on-disk `artifacts/pr_context.summary.txt` was untouched — mtime and content still belonged to a file the feature-review subagent had HAND-AUTHORED an hour earlier, whose own first line read "collect_pr_context MCP tool unavailable in this session" and whose `Base:` was the old pre-change baseline rather than the `--base` I passed. So the tool will report success over a pre-existing file it did not write. Consequences: (1) never read the summary to learn the diff — author the PR body from `git diff <base>..HEAD --stat` and `git log <base>..HEAD` ([[pr-context-summary-unreliable-gh-and-classification]]); (2) the receipt's `created_at > summary mtime` check gets *easier*, not harder, because the stale mtime is older — but verify it rather than assuming; (3) `feature-review` also lacks `collect_pr_context`, so a reviewer may leave a hand-authored decoy in `artifacts/` that survives your own collect call.

**CROSS-CHILD CONTAMINATION (#449, 2026-08-22) — the most dangerous variant, promoted to issue #589.** In an isolated agent worktree the tool returned `ok:true` with worktree paths, wrote NOTHING there, and wrote to the MAIN checkout instead — a location SHARED by every concurrently running epic/parallel child. Sibling child #491 ran its own collect in the gap between my collect and my copy, so the file I copied was ENTIRELY #491's: `Head ref (resolved): bug/quickfiler-test-form1-live-form-491-exec @ bec83397`, and its `Additional context files` listed 14 artifacts from #491's feature folder. Authoring from it would have described the wrong change on my PR, with `ok:true` and a well-formed file as the only signals.

**How to apply — verify OWNERSHIP, not just presence.** Presence checks and mtime checks both pass on a sibling's file. After copying, assert the summary's `Head ref (resolved)` SHA equals your own `git rev-parse HEAD`, and sanity-check that the feature-folder references in the file are YOURS (I counted 96 for my folder and 0 for #491's). Re-run and re-copy if it does not match. Then author the body from the real `git diff <base>..HEAD` regardless — the file is only safe as a file LIST, never as a narrative.

Two more defects confirmed in the same bundle: the summary reported "GitHub CLI (gh) is not installed" while `gh auth status` and `gh issue view` both worked in the same session; and the `author asserted` autoclose list contained `#AC-1`..`#AC-16` (acceptance-criterion IDs scraped as issue numbers) plus three issues that were not mine to close. Never emit `Closes` from that list. Note also that a child PR into an epic integration branch cannot auto-close anything — GitHub only honors closing keywords merging into the DEFAULT branch — so `Refs #NNN` is the correct form and the epic's final integration-to-main PR carries the close.

**Independent confirmation and the simplest safe remedy (#445, 2026-08-22).** Same run, same wave:
`ok:true`, worktree paths returned, nothing written there, primary checkout freshly written. The
worktree copy I would have used was a decoy the feature-review subagent had hand-authored (quirk (a)
above), recording a head SHA one commit behind and omitting all three review artifacts.

Rather than copy-then-verify-ownership, the cheaper remedy is **do not copy at all**. Treat
`collect_pr_context` purely as a receipt formality, then REGENERATE `artifacts/pr_context.summary.txt`
yourself in your own worktree from `git rev-parse HEAD`, `git merge-base`, `git log --oneline
<base>..HEAD`, and `git diff --numstat <base>...HEAD`. This is strictly safer than the ownership
check, because a file you wrote from your own git state cannot be a sibling's, and it simultaneously
satisfies the pr-author receipt's `created_at > summary mtime` check as long as you write the receipt
afterwards. `pr_context.*` is gitignored, so regenerating dirties nothing. See
[[pr-context-summary-unreliable-gh-and-classification]] and
[[pr-author-hook-blocks-gh-in-this-repo]].
