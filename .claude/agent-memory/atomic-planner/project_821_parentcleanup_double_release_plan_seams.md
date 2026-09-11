---
name: project-821-parentcleanup-double-release-plan-seams
description: Planning seams for issue #821 (QfcHomeController ParentCleanup double ribbon release, four sites) — 498-line file with 2 lines of headroom, headless viewer NREs in the CancelSource setter, coverage runner writes before it throws
metadata:
  type: project
---

Seams found while authoring the F821 atomic plan (four production sites: `QfcHomeController.cs`,
`EfcHomeController.cs`, `ProgressViewer.cs`, `ProgressPane.cs`).

**Why:** each of these would have produced a preflight round or an execution halt if the plan had
been written from the spec's prose instead of from a direct re-derivation against the tree.

**How to apply:** check the analogous condition on any plan touching these files or this test shape.

- **`QfcHomeController.cs` is 498 lines.** The read-into-local-then-clear idiom replaces 1 line with
  3, landing the file at *exactly* 500. 500 is compliant (both policies say "may not **exceed** 500")
  but there is no headroom for an explanatory comment line. The fix: put the "why" as a **trailing
  comment on an existing statement line** (`System.Action parentCleanup = ParentCleanup; // ...`,
  94 chars, under CSharpier's 100-char print width). Do not reclaim lines by deleting the
  commented-out `//logger.Debug(...)` lines at 41/273/316/331/337 — the banned-API record depends on
  them being the only DateTime.Now matches. Do not spill into `QfcHomeController.Metrics.cs` or
  `.Iteration.cs`: a 9th file falsifies the footprint AC.
- **The Site A edit region starts at line 405, so earlier citations survive.** `_tokenSource?.Dispose()`
  at :389 (the AC12 single-disposal-site citation) and the two `catch (System.Exception e)` blocks at
  :382 and :399 (the AC15 citation) do not move. Worth asserting rather than assuming, because a
  numstat of 3 insertions / 1 deletion proves the edit stayed inside the `finally`.
- **`CreateHeadlessViewer()` breaks the `CancelSource` setter, not just the button read.**
  `ProgressViewer_Tests.cs` :33-34 uses `FormatterServices.GetUninitializedObject`, so `ButtonCancel`
  is null — and the `CancelSource` **setter** dereferences it (`ButtonCancel.Enabled = value != null;`
  at ProgressViewer.cs:60). Any test that *assigns* a source, not merely reads the button, must use
  the real constructor path with an installed SynchronizationContext.
- **`ProgressPane.Designer.cs` wires no Click handler.** `ProgressViewer.Designer.cs`:65 has
  `this.ButtonCancel.Click += new System.EventHandler(this.CancelButton_Click);`; the pane Designer
  has no equivalent line. The pane's handler is reachable only by reflection, which is what the
  existing test at `ProgressPane_Tests.cs`:125-133 already does. Do not write an acceptance clause
  asserting designer wiring for the pane.
- **`ProgressPane` lives in `UtilitiesCS.EmailIntelligence.TaskPane`, not `UtilitiesCS`.** Its test
  file imports that namespace explicitly. It also has no `CancelSource` property, so a fix shape that
  delegates to a property setter applies only to `ProgressViewer`.
- **`Invoke-MSTestWithCoverage.ps1` writes the Cobertura file *before* it can throw.** `Set-Content`
  of the post-processed XML is at line 342; `Assert-CoberturaLineCoverageThreshold` (which throws
  below a repo-wide 80% line-rate) is at line 344. So numeric coverage is readable even when the
  threshold gate throws — useful when policy makes the repo-wide figure report-only. The success-only
  observable for "vstest exited 0" is the literal `Post-processing coverage XML for Koverage
  compatibility...` at line 339; `Done. Coverage artifact:` at line 345 additionally implies the 80%
  assert passed. Gate on the first, report the second.
- **A near-limit test file bars the obvious remedy.** `ProgressViewer_Tests.cs` is 352 lines with a
  500 ceiling, leaving 147 lines for five new tests. Splitting into a sibling test file is barred
  because every test `.cs` in this legacy repo needs a `<Compile Include>` entry, which falsifies a
  no-`.csproj` AC. The plan must instead budget explicitly: shared `WithSynchronizationContext` and
  `GetCancelButton` private helpers plus single-line `/// <summary>` docs.
- **A footprint AC written as "the eight files plus the spec plus *the* evidence artifact" collides
  with the multi-artifact evidence contract.** Reconcile it explicitly in the plan (permitted set =
  write set + spec + plan file + everything under `<FEATURE>/evidence/`) rather than letting the
  executor discover the contradiction. See [[agent-memory-is-tracked-scope-git-gates]] for the
  related `.claude/agent-memory` carve-out on the clean-tree gate.

## Preflight round 1 findings (applied as a 15-delta revision)

- **`ProgressViewer.cs` is 92 lines, not 93.** The Read tool renders a trailing line number for the
  final newline, so reading a file to its end over-counts by one. `git grep -c "" HEAD -- <path>` is
  the authority. An exact-count Phase 0 gate pinned to the over-count is unsatisfiable. The same
  artifact affects `ProgressPane.cs` (61, rendered through 62).
- **`Select-String` is case-insensitive by default, so a zero-match gate on the removed statement
  fails when the delivered statement differs only in leading case.** Here `ParentCleanup?.Invoke();`
  is replaced by `parentCleanup?.Invoke();`. The gate needs `-CaseSensitive`. The sibling site is
  safe only because `_parentCleanup.Invoke();` → `parentCleanup?.Invoke();` differs by an underscore
  and a `?` as well.
- **A whole-member "no zero-hit line" coverage gate is unsatisfiable over a member that contains a
  pre-existing untested branch.** `QfcHomeController.Cleanup` (:371-408) holds a `catch` at :382-385
  whose `logger.Error` at :384 fires only if `_formViewer?.Worker` throws, which no test drives.
  Split the gate: blocking on *changed* lines, and per-member rate discharged by enumerating the
  zero-hit lines and showing each was already zero-hit at baseline.
- **Assert `!.Cancel()` removal at the task that removes it.** The null-forgiving dereference lives
  in `CancelButton_Click`, so the `RequestCancel`-adding task cannot satisfy that clause; the
  handler-rewrite task can.
- **The merge-base sits below the feature's own preparation commit**, so every anchored diff lists
  `spec.md`, `issue.md`, the research file and the plan file whether or not the plan edits them.
  A footprint AC must permit all four rather than treat them as out-of-scope.
- **The terminal clean-tree gate cannot demand an empty porcelain.** Enumerate three residual
  classes instead: `.claude/agent-memory/` entries, the plan file's own post-commit check-offs, and
  the post-commit status artifact itself.
- **A sanitisation task that quotes its own search tokens is self-defeating**, and the plan file is
  committed, so quoting the account name or the worktree directory name anywhere in the plan ships
  them to `main`. Derive both at run time: `$env:USERNAME` and
  `Split-Path -Leaf (git rev-parse --show-toplevel)`.
- **`Invoke-MSTestWithCoverage.ps1` has no `/Blame` and no timeout**, and its only filter is
  `TestCategory!=LiveOutlook` at :76. `UtilitiesCS.Test` declares no `TestCategory` at all, so the
  three shell-icon test classes are unfiltered and can stall the run. Budget a detached run with an
  idle-output timeout and a per-assembly fallback.
- **A green vstest run prints no `Failed:` and no `Skipped:` line**, so an artifact field demanding
  those counts must say to record `0` and note the lines were absent.

Related: [[project_810_teardown_dropdown_residuals_plan_seams]] (this issue exists because 810's
guard was not carried to its siblings), [[literal-call-clauses-block-file-size-tightening]],
[[repo-wide-cobertura-line-rate-is-nondeterministic]].
