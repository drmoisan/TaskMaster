---
name: project-948-sibling-first-shape-reconciliation-seams
description: "#948 v0.3 completion pass: when a parallel sibling (947) edits the same method and lands FIRST, express every edit by content anchor under two named shapes (S = sibling guard present, M = absent), gate AC text that counts try/catch as baseline-relative counts derived at Phase 0, re-budget the 500-line ceiling against the sibling's file, prefer insertion over region replacement so the sibling's wording survives, and prove both guard outcomes by hit counts (record hits >= 1, guard hits > record hits) rather than relying on the condition-coverage attribute"
metadata:
  type: project
---

Seams from completing plan 948 (repeat prime-fault suppression in `EngineToggleStateCoordinator.CompletePrime`) after the coordinator decided that sibling 947 (throwing-sink guard, same method) merges first (2026-10-01, no Bash tool in the session; the 947 worktree was read but never modified).

**Why:** the v0.2 draft and the spec were written against the pre-947 tree. 947's worktree showed `CompletePrime` wrapping the sink call in `try { _logError } catch (Exception) { }`, a nested sink catch in `HandleToggleClickAsync` (three `catch` code lines file-wide), a reworded `GetPrimeTask` returns element (the v0.2 E4 anchor sentence no longer existed), a fifth fixture partial and csproj entry, and a 476-line file. The spec's AC-L ("no try/catch/finally in CompletePrime; only the click-boundary catch hit") became unsatisfiable, the E4 anchor vanished, and the delivered text's +31 lines would have breached the 500-line ceiling (476 + 31).

**How to apply:**
- Classify the merge-base body as shape S or M with one payload that also serves post-edit (admit "the record line directly after the sink line" in the S test); stop with `ANCHOR SHAPE CHANGED` on anything else; admit M only with an explicit orchestrator statement, else `SIBLING NOT MERGED`.
- Rewrite try/catch/finally AC text as "no keyword beyond the merge base" and gate SPAN-TRY/CATCH/FINALLY and FILE-CATCH-CODE-LINES equal to Phase 0 baseline values; no digits in the AC line.
- Under S, insert (guard `if` + `{` above `try`, record after the sink line inside the try, `}` after the catch arm's own-indent close) rather than replacing the region, so the sibling's final wording is preserved even if it differs from the worktree copy; the record inside the try block is what keeps "a throwing sink leaves the pair unrecorded" true.
- Re-derive the size budget: SIZE-BUDGET = 499 - MERGE-BASE-LINES, EXPECTED-DELTA per shape, stop `SIZE BUDGET EXCEEDED`; trimming levers were a 2-line field summary, a 4-line method summary (one shorter than the sibling's 5), a 4-line `<para>`, a 4-line comment and a one-line E4 under S.
- Anchor E4 on "the text line directly above `/// </returns>`" with a per-shape replacement text, never on a sentence the sibling may reword.
- Guard-branch proof by hit counts (record >= 1, guard > record) with the `condition-coverage` row gated only when `branch=True` lands on the guard line.
- Enumerate fixture partials dynamically (`Get-ChildItem ... EngineToggleStateCoordinatorTests*.cs`, `git ls-tree` at MERGE-BASE) and insert the csproj entry after the LAST fixture entry, so a sibling-added partial is counted and protected without naming it.
- Writing a ~1000-line plan with no Bash and an output cap: Write the head with a sentinel comment, then Edit-append pieces; plant distinct sentinels so the last appends can run in parallel; the Edit tool kept LF endings on a LF file.

Related: [[project-947-passa-second-call-site-consolidation-seams]], [[project-940-r3-per-file-coverage-rule-and-post-merge-reanchor-seams]], [[project-927-r8-powershell-batch-phases-and-post-merge-reanchor-seams]].
