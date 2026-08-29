---
name: project-635-reflective-caller-audit-plan-seams
description: Issue #635 evidence-only audit plan — self-scanning gates, tracked-plan-inflates-its-own-sweep, spec-vs-tree site-count mismatch, and pathspec breadth changing a reported count
metadata:
  type: project
---

Planning seams found while authoring the issue #635 residual-reflective-caller audit plan (a
Markdown-only, evidence-producing audit that modifies no `.cs` file).

**Why:** four defects were only visible by re-deriving the delegation's figures against the tree; each
would have failed at execution.

**How to apply:** on any plan whose deliverable is a repository-wide search recorded as evidence.

1. **A tracked plan file inflates its own sweep.** The plan is tracked under the docs tree and quotes
   every identifier it searches for, so a `git grep` partition that includes the docs tree returns
   more hits after the plan is written than the base measurement the caller supplied. Never assert
   that partition's total; assert the classification identity (per-category counts sum to the printed
   total, the "genuine caller" category is `0`) and record the reason the total moved.

2. **A host-identity / forbidden-token scan hits its own pattern list.** Both the scan's artifact and
   the plan file quote the patterns verbatim, so a zero-hit gate over the feature folder is
   unsatisfiable. Carve out exactly those two files by name filter, state the reason, and verify by
   hand at planning time that they carry no real leak.

3. **Enumerate from disk, not from the index, for a late-phase folder scan.** `git ls-files` misses
   the Phase 4 artifacts that are still untracked; `Get-ChildItem -Recurse -File -Name` covers both
   and prints folder-relative names, so no resolved provider path (which carries the account name)
   reaches the artifact. `Select-String`'s `MatchInfo.Path` is resolved and absolute — print the
   enumeration variable instead.

4. **A spec's site count can disagree with the tree.** Spec AC-9 named "six variable-argument
   reflection call sites"; the tree has eight against `typeof(QfcCollectionController)` (five
   `GetField` plus one `GetMethod` in the test-support partial class, plus one in the navigation-digits
   test file and one in the main test file). Enumerate the mechanically derived superset, note that it
   names each of the spec's six individually and therefore discharges the AC, and record the
   discrepancy as an evidence note rather than editing an approved spec.

5. **A git pathspec is broader than a language.** `-- 'QuickFiler/*'` reaches tracked `.csproj`,
   `.csproj.bak` and `packages.config`, so a `System.Reflection` count taken that way exceeds the
   first-party `.cs` count (32) that a delegation may have measured. Assert a floor plus a total
   classification whose classes include a manifest-reference class, never the bare number.

Related: [[zero-hit-grep-gates-need-carveouts]], [[absolute-counts-in-shared-files-go-stale]],
[[acceptance-edits-must-be-false-before-true-after]].
