# [P0-T1] Dependency #441 verification

Timestamp: 2026-08-11T00-02
Command: `pwsh -NoProfile -File <scratchpad>/p0t1-verify-441.ps1` (dot-sources `scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1` and runs the three checks below)
EXIT_CODE: 0

Working directory: `C:\Users\DanMoisan\repos\TaskMaster\.claude\worktrees\agent-a3f0c78078ca2265a`
Branch: `bug/excludefromcodecoverage-nested-lambdas-457`
HEAD: `1c221399a72d9102c357e4d5164f5f0bb5c7fd2e`

Path redirection note: the plan was authored in a different worktree. All repo-relative paths in this plan resolve against the working directory above. No absolute path from another checkout was used.

## Result

RESULT: all three checks PASS. Issue #441's corrections are present on this branch. Plan execution proceeds to `[P0-T2]`.

## Check 1 — static

Search for the descendant-axis literal `.//lines/line` across `scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1`:

- `DESCENDANT_AXIS_LITERAL_MATCH_COUNT: 0` (expected 0)

Child-axis selections present (verbatim, with line numbers):

```
line 196: $lineNodes = @($ClassNode.SelectNodes('./lines/line')) +
line 197: @($ClassNode.SelectNodes('./methods/method/lines/line'))
line 313: foreach ($lineNode in @($classNode.SelectNodes('./lines/line'))) {
```

`Get-CoberturaCoverageSummary` (line 98) delegates per class to the post-#441 pure helper
`Get-CoberturaClassLineSummary` (line 161), which selects the child axis `./lines/line` (line 196)
unioned with `./methods/method/lines/line` (line 197) and de-duplicates by line number. That is the
post-#441 contract: the class-level rollup and the method-level view of the same lines are keyed by
line number and counted once. `Merge-CoberturaClassesByFilename` likewise uses `./lines/line`
(line 313) and recomputes the merged rate via `Get-CoberturaClassLineSummary` (line 364).

`MODULE_LINE_COUNT: 455`

STATIC CHECK: PASS

## Check 2 — functional (double count)

Inline here-string fixture: one `<class name="Ns.T">` carrying line numbers 10 and 11 both under
`<methods>/<method name="Visible">/<lines>` and under the class-level `<lines>`.

- `FUNCTIONAL_LinesValid: 2` (expected `2`; `4` would prove the pre-#441 double count)
- `FUNCTIONAL_LinesCovered: 1`

FUNCTIONAL CHECK: PASS

## Check 3 — blended denominator (functional)

Inline here-string fixture: two `<class>` elements sharing `filename="Ns\T.cs"`.

- primary `Ns.T`: line 10 with `hits="1"` under both `<methods>/<method>/<lines>` and the class-level `<lines>`
- sibling closure `Ns.T.&lt;&gt;c`: line 11 with `hits="0"` under its class-level `<lines>`

Run through `Merge-CoberturaClassesByFilename`, then the surviving merged `<class>` was imported into a
scratch `<coverage><packages><package><classes /></package></packages></coverage>` document and
`Get-CoberturaCoverageSummary` was called on that scratch document. The merged class's `line-rate`
attribute was deliberately NOT used as the denominator proof.

- `MERGED_CLASS_COUNT: 1`
- `MERGED_CLASS_NAME: Ns.T`
- `BLENDED_LinesValid: 2` (expected exactly `2`; `3` would prove the pre-#441 blend)
- `BLENDED_LinesCovered: 1` (expected exactly `1`; `2` would prove the pre-#441 blend)

BLENDED CHECK: PASS

This check is load-bearing for regression case 6: it establishes that the merge retains the closure's
lines in the merged class-level `<lines>`, so a filter placed after the merge would be a no-op and the
ordering gate in `[P2-T9]` / case 6 is non-vacuous.

## Output Summary

All three checks pass. `EXIT_CODE: 0`. Issue #441 (PR #538, base commit
`fb257cd6e0c56cbf5eacf7e6a73641cc0414c930`) is present in this branch's base. No BLOCKED state.
