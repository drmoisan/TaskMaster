---
name: project-959-r1-census-initializer-displayname-and-worktree-claude-filter-seams
description: Preflight round-1 seams on #959 (SortEmail latent defects) - whitespace-stripped census tokens match field initializers, PD-5 DisplayName rows inflate method-name counts, an absolute-path `*\.claude\*` filter excludes every file of an item worktree, AC26 permits only three evidence subfolders, flaky-branch ACs need conditional check-off, compile-red spans need a no-commit rule
metadata:
  type: project
---

Round-1 preflight on #959 returned fifteen defects; none changed the write set. The classes worth keeping:

- **Census tokens match declarations.** A whitespace-stripped token such as `_attachmentsAltName=YesNoToAllResponse.Empty;` matches the field initializer `private static YesNoToAllResponse _attachmentsAltName = YesNoToAllResponse.Empty;` as well as the reset statement. Count every declaration line that ends in the same assignment before fixing a BASE value.
- **DisplayName inflation (PD-5).** When every `[DataRow]` carries `DisplayName = "<method> [<tag>]"`, a method-name census token counts the declaration plus one per row (four rows gave 5, not 1). Audit every method-name token in every TOKENS table whenever a listing uses DisplayName rows.
- **Item-worktree `.claude` filter.** The item worktree lives under `<main>\.claude\worktrees\agent-...`, so `$f.FullName -like "*\.claude\*"` is true for every file and a payload that skips `.claude` silently counts zero. Filter on `$f.FullName.Substring($root.Length)` and add a positive-control count (`CS-FILES:` at least 1). CMD-COVERAGE-DIRECT already used the root-relative form; CMD-GREP-FACTS did not.
- **`ShowDialog\(` regex over the partials** also matches `InputBox.ShowDialog(` and a commented-out call; a "call expressions only" prose count (8) differs from the regex count (9). State which one the payload measures.
- **Evidence subfolders.** The evidence skill lists `evidence/other/`, but this item's spec AC26 and D17 permit only baseline, regression-testing and qa-gates. Read the AC text before choosing a subfolder; add a `NONCANONICAL-SUBFOLDER-FILES:` count to the evidence-fields sweep.
- **Porcelain after a deletion.** A `git status --porcelain -- <directory>` acceptance written for a later task must list the ` D` row of a file an earlier task deleted in that directory.
- **Flaky-branch ACs.** An AC phrased "pass in one final pass with zero exit codes" (AC24) or "the full test run passes" (AC21) cannot be checked when the plan carries a tolerated branch (b) for the known #780 failure; make the check-off conditional on branch (a) and name the `NOT MET` report for branch (b), and give the final inventory task both expected end states.
- **Compile-red spans.** When a test listing is written before the seam it calls (P4-T1..P4-T7, P5-T4..P5-T6), declare the span in the first task and in Execution Conventions with a `COMPILE-RED SPAN OPEN` report, so no checkpoint commit lands inside it.
- **Anchor gates.** `git merge-base --is-ancestor origin/main HEAD` returning 0 is not a stop (the branch may already contain main); gate only `CITED-TREE-EXIT`, and define `PATHS-CITED` as the exact cited-file list (quote paths with spaces; exclude FEATURE/ because preparation commits change it, and .claude/ because P0-T1 reads policy at run time).
- **Line counts.** Grep `^` equals the `Get-Content` count; the Read tool shows one more row when the file ends with a newline (verify with a Read of the tail, not by assumption). Three files (S, M, L) had been cited one short.

**Why:** each of these produced an unsatisfiable or vacuous acceptance value in an otherwise mechanical plan; the executor found them in one pass.

**How to apply:** before handing off a census-heavy C# plan, re-derive every method-name and assignment token against both the tree and the listings, check every payload filter against the item worktree's own path, and read the AC text for evidence-location and pass-condition wording.
