---
name: porcelain-overstates-formatter-rewrites-stat-cache
description: After CSharpier `format`, `git status --porcelain` lists files the formatter only TOUCHED (mtime bump, identical bytes); `git add` then resolves them to clean, so a "N files rewritten" claim taken from porcelain alone overstates.
metadata:
  type: project
---

`dotnet tool run csharpier format .` updates the modification time of files it
processes even when it writes back identical bytes. `git status --porcelain` compares
the stat cache first, so those files appear as ` M` with no content change behind them.
`git add` performs the content comparison and silently drops them from the index diff.

Measured 2026-09-07 (issue #799 Phase 3, item worktree `agent-a31e7f0c84197d9ba`):
porcelain listed 9 modified `.cs` files after the format pass. After `git add` of all
nine, only 5 remained staged. For the other four, `git diff --cached --numstat` returned
0 lines and the files were uniformly CRLF (CRLF count == total LF count), so it was not
a line-ending rewrite either — the bytes were unchanged.

**Why:** an evidence artifact that says "the formatter rewrote N files", sourced from
porcelain, is a claim about content that porcelain cannot support. It also inflates the
apparent blast radius of a format pass in a scope-boundary artifact.

**How to apply:** when a plan task requires recording what a write-mode formatter
changed, distinguish TOUCHED from CHANGED. Take the touched set from porcelain, then
stage and read `git diff --cached --numstat` per path for the changed set. State both
numbers. Cross-check with the anchored `git diff --stat <BASE-SHA>` before/after
capture, which shows the real line deltas and is blind to mtime-only touches.

Complement of [[project_porcelain_diff_cannot_detect_rewrite_of_already_modified_file]]:
that one is porcelain UNDER-reporting a real rewrite of an already-`M` file; this one is
porcelain OVER-reporting a no-op touch of a clean file. Neither direction is safe to read
off porcelain alone. Related: [[project_count_idiom_pitfalls_csharpier_and_measureobject]],
[[feedback_never_predict_an_observation_into_an_artifact]].
