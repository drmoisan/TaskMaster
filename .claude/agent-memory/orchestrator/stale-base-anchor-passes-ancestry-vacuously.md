---
name: stale-base-anchor-passes-ancestry-vacuously
description: On a preparation resume, a plan's pinned base commit stays an ancestor of HEAD after main advances, so the plan's own merge-base check passes while every diff gate silently attributes another issue's work to this plan
metadata:
  type: project
---

When resuming an interrupted preparation run, re-fetch `origin/main` and diff it against the plan's pinned base anchor before doing anything else. If main advanced, merge it into the item branch and re-anchor every `git diff` in the plan to the merge commit.

**Why:** A plan pins a base SHA and asserts `git merge-base --is-ancestor <BASE> HEAD` exits 0. After main advances and is merged in, that assertion *still passes* — the old commit is still an ancestor — so the plan's own self-check reports a clean baseline while `git diff <BASE>..HEAD` already lists the files the intervening PR changed. Every downstream "exactly N files changed", "no hunk in this range", and changed-line-coverage gate then measures the other issue's work as if this plan produced it. Ancestry is not currency.

Concrete instance (issue #637, 2026-08-29): base `ecdb1c84`, main advanced to `fa2ddefa` via PR #700 (issue #638). Ancestry check passed; three files were already changed against the anchor. `EfcDataModel.cs` had gone 423 to 485 lines, moving every cited line number, and the plan still instructed writing `Globals.Ol.ArchiveRootPath` at a site where #638 had just replaced that unguarded read with a guarded `TryGetArchiveRoot(out var olAncestor)` local. Following the plan verbatim would have regressed #638 and failed its merged regression test, and the 62 added lines had cut 500-line headroom from 77 to 15, making the plan's own file-size gate unsatisfiable.

**How to apply:** Replace the vacuous ancestry assertion with one that can fail — assert `git diff --name-only <BASE>..HEAD -- <owned trees>` produces no output — and pair it with a `git status --porcelain` companion, since a commit-to-commit diff is blind to both untracked files and modified-but-unstaged tracked ones. See [[prepared-epic-child-invalidated-by-sibling-merge]] for the related case where a sibling merge invalidates a prepared child outright.
