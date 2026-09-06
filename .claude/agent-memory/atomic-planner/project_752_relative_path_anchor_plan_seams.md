---
name: project-752-relative-path-anchor-plan-seams
description: "#752 planning seams: GetRelativePath returns no leading separator so the directed relative-path fix silently breaks the regression test it must preserve; Invoke-Formatter re-indents an extracted fragment to column zero so a fragment-idempotence gate is unsatisfiable"
metadata:
  type: project
---

Seams found while authoring the #752 plan (`scripts/vscode/Invoke-MSTestWithCoverage.ps1` assembly discovery excludes its own worktree root).

**A directed "match the relative path instead of the absolute path" fix is not a pure substitution — the regex must gain a start anchor.** `[System.IO.Path]::GetRelativePath` returns a descendant path with **no leading separator**. The production predicate was `$_.FullName -notmatch '\\\.claude\\'`, which requires a backslash immediately before `.claude`. Relativized, the nested-sibling fixture `C:\repo\.claude\worktrees\agent-1\...` against root `C:\repo\.` becomes `.claude\worktrees\agent-1\...`, which the unchanged regex does **not** match, so the sibling worktree is retained and the very regression test the item must preserve (`tests/scripts/vscode/Invoke-MSTest.RunSettings.Tests.ps1` `It 'excludes assemblies discovered under a .claude worktree segment'`) fails. The correct literal is `(^|\\)\.claude\\`.

**Why:** the caller's fix direction named the match *target* only. A planner that transcribes the direction verbatim ships a change that passes AC1's wording and fails AC3, AC6, and the untouched preserved test — a sibling-invalidation the edited-line-only review never sees.

**How to apply:** whenever a fix changes what a path predicate matches *against* (absolute to relative, full to leaf, native to normalized), re-evaluate every existing fixture through the new target before accepting the old pattern. Plan an explicit probe task that prints the transformed value and both regex verdicts for each fixture, so the anchor decision rests on measurement.

**`Invoke-Formatter -ScriptDefinition` on an extracted, indented fragment re-indents it to column zero.** A gate of the form "format the eight-line block and assert the output is byte-identical to the input" can never return true for a block that lives inside a function, so its re-run-until-true loop does not terminate. Assert the authored line's leading-space count, the file's total line count, and the trimmed line text instead.

**Repo facts re-derived 2026-09-03 in `prep-752`:** `Invoke-MSTestWithCoverage.ps1` is 350 lines, predicate at line 301 at 16 spaces, `Where-Object {` at line 297 at 12 spaces, `$resolvedSearchRoot` assigned line 272, throw at line 306. `Invoke-MSTest.RunSettings.Tests.ps1` is 488 lines; its `Describe 'Invoke-MSTestWithCoverageMain'` starts at 346, `BeforeEach` 347-373, the preserved `It` 416-442. `Invoke-MSTest.AssemblyDiscovery.Tests.ps1` dot-sources `Invoke-MSTest.ps1` only and carries **no** AST parse — the AST-parse import pattern lives in `Invoke-MSTest.RunSettings.Tests.ps1` lines 13-23. `Invoke-MSTestWithCoverage.Helpers.ps1` line 4 chains to `Invoke-MSTestWithCoverage.Threshold.ps1`, so dot-sourcing Helpers alone resolves `Assert-CoberturaLineCoverageThreshold`, whose floor is 80 percent and which accepts the `line-rate="0.8"` fixture the existing tests use. A case-sensitive `.claude` sweep over `scripts/` returns exactly 5 lines, only one of which is an exclusion predicate.

See [[powershell-gate-observables]], [[poshqc-mcp-and-msbuild-invocation-facts]], [[acceptance-edits-must-be-false-before-true-after]].
