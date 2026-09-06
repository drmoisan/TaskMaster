---
name: bug-corpus-is-quickfiler-concentrated
description: TaskMaster's open-bug corpus was ~71% QuickFiler and near-serial; a 2026-09-02 consolidation sweep collapsed it from ~78 issues to 20 umbrella issues and broke the monopoly — always re-measure the histogram, and expect ~7 of any 20 to be upstream-owned or ordering-blocked
metadata:
  type: project
---

**2026-09-02 UPDATE — the corpus was consolidated and the headline conclusion below no longer
holds.** A review sweep closed ~50 small bugs and refiled them as ten umbrella issues
(#727-#737), taking the open `bug`-labelled corpus from ~78 to **20**. Each umbrella issue
carries 1-7 enumerated defects with file-and-line citations in the body, so module assignment is
readable directly from the issue text — no `git ls-files` class-name resolution needed, unlike the
pre-consolidation corpus. The QuickFiler monopoly is gone: the 13 in-repo-implementable items
spread across `QuickFiler`, `QuickFiler.Test`, `UtilitiesCS`, `UtilitiesCS.Test`, `TaskMaster`,
`TaskMaster.Test`, `scripts/vscode`, `.github/workflows` and root docs, with a largest module
clique of about 6 rather than 51.

**The durable finding is the SHAPE of the exclusion set, not the numbers.** Of 20 open bugs on
2026-09-02, seven were not parallel-plannable and they failed in three distinct ways worth
checking for every time:

1. **Wholly upstream (1).** #691's only fix site is `.claude/hooks/**`, mirrored at `.codex/hooks/`.
   Both are push-down artifacts. See [[claude-files-are-pushdown-owned]] territory — an edit here is
   silently overwritten.
2. **Ordering-blocked (3).** #563 (coverage-threshold contradiction) must be settled before #561 and
   #562 can gate against it, and #563 itself is half-upstream plus needs a maintainer number
   decision. The parallel surface cannot express that; see
   [[parallel-cannot-express-ordering]].
3. **Mixed in-repo/upstream scope (3).** #727, #671 and #728 each have a real in-repo half and a
   half that only push-down can fix. #727 additionally spanned QuickFiler + QuickFiler.Test +
   UtilitiesCS + CLAUDE.md + coverage.config + docs, which would have made it adjacent to nearly
   every other item.

That 20 - 1 - 3 - 3 = **13** is the set that is fully implementable inside the checkout, and it
matched the count the operator asked for independently. When an operator's stated bug count is
lower than `gh issue list --label bug` returns, the difference is usually this exclusion set rather
than a miscount.

**Two mandate-read traps specific to this corpus.** `config/blast-radius.json` lists
`scripts/vscode/**` in `mandate_reads`, so an item that genuinely REWRITES the coverage tooling
(#733, #565) has those paths dropped from its derived radius and the planner must hand-append them
after normalization or the two items will be scheduled concurrently against the same file. The same
applies to `.claude/agent-memory/**` for any item touching it. Separately, issue #576 is still open:
`shared_surfaces` carries only six entries and none of TaskMaster's root build files
(`TaskMaster.sln`, `Directory.Build.targets`, `coverage.config`, `.editorconfig`), so those fail
OPEN and must also be hand-appended.

**Still true from the 2026-08-21 measurement:** do not derive radii from issue bodies as a proxy for
plan radii, and do not delete a module from the map to widen cohorts. Also still true: a hub item
that touches five project trees at once (here, #730's `System.Reactive` suppression across five
`packages.config` files) will contend with everything — direct the preparation child toward a
single repository-root `Directory.Build.props` instead, in the delegation prompt, before it plans.

**How to apply:** measure the module histogram FIRST and report cohort depth before launching any
preparation child; that guidance is unchanged and is what made the 2026-09-02 run viable at ~6
cohorts rather than the 51 the pre-consolidation corpus would have produced. See
[[blast-radius-extractor-mechanics]] for the backtick rule that governs whether derivation fires at
all — instruct preparation children to write every write-target as a backticked concrete path.
