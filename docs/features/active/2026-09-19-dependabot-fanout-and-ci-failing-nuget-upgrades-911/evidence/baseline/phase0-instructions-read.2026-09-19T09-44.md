# P0-T2 — Phase 0 Policy Read

Timestamp: 2026-09-19T12-16

Policy Order: the order fixed by `.claude/skills/policy-compliance-order/SKILL.md`, extended by the
language-specific rule files in scope for this change (PowerShell and C#) and by the two
cross-cutting rule files the plan names: `CLAUDE.md`, then `.claude/rules/general-code-change.md`,
then `.claude/rules/general-unit-test.md`, then `.claude/rules/powershell.md`, then
`.claude/rules/csharp.md`, then `.claude/rules/quality-tiers.md`, then `.claude/rules/tonality.md`.

Command:
```
git -C "<execution-worktree-root>" hash-object <the seven files>
wc -l CLAUDE.md .claude/rules/general-code-change.md .claude/rules/general-unit-test.md \
      .claude/rules/powershell.md .claude/rules/csharp.md .claude/rules/quality-tiers.md \
      .claude/rules/tonality.md
```

EXIT_CODE: 0

## Files read, in order

| # | File (execution worktree) | Lines | Blob SHA-1 |
|---|---|---|---|
| 1 | `CLAUDE.md` | 463 | `0c650735e12f1c31c6522f53c4dd85a836793296` |
| 2 | `.claude/rules/general-code-change.md` | 80 | `69d31ef89270b44d8e2ccb5c382513e8de26c71d` |
| 3 | `.claude/rules/general-unit-test.md` | 105 | `6b70ee410f0630e3cf8e7e1ef9debf02f2295a1e` |
| 4 | `.claude/rules/powershell.md` | 97 | `ce86d6ec36ccf95b2454c27a35edf33e4e53b4c1` |
| 5 | `.claude/rules/csharp.md` | 96 | `143866c58a475920601e96239a1cd9b832a70857` |
| 6 | `.claude/rules/quality-tiers.md` | 51 | `28209fc80bb0be27446ee72cabc3aa6a59ae2d7e` |
| 7 | `.claude/rules/tonality.md` | 80 | `d971f5be28aa02722e216e7fe6b92aec04c40a52` |

Total 972 lines. Every file was read in full from the execution worktree
`<execution-worktree-root>`, not from the session worktree.

## Why the read was taken against the execution worktree

The blob SHA-1 of `CLAUDE.md` differs between the two worktrees: `0c650735e…` in the execution
worktree against `67f75c93d…` in the session worktree
`<session-worktree-root>`. The other six files are byte-identical
across the two. The session copy of `CLAUDE.md` is therefore not a valid substitute and the
execution-worktree copy is the governing text for this run.

## Substantive differences carried by the execution-worktree `CLAUDE.md`

Recorded because they bind later tasks in this plan.

1. **Coverage floors (section UT2, settled 2026-09-11, issue #563).** C# line coverage `>= 80%`
   and C# branch coverage `>= 75%`; PowerShell line coverage `>= 80%` with no branch floor because
   Pester does not measure branch coverage. These are the figures
   `scripts/vscode/Invoke-MSTestWithCoverage.ps1` enforces and the figures the plan's C# coverage
   margins row quotes (line 0.820056 against 0.80; branch 0.782406 against 0.75).

   **Divergence from the rule files, recorded rather than resolved.**
   `.claude/rules/general-unit-test.md` and `.claude/rules/quality-tiers.md` both state a uniform
   line floor of `>= 85%` and a branch floor of `>= 75%`. `CLAUDE.md` is first in the policy
   compliance order and states `>= 80%` line for both languages, citing a maintainer decision of
   2026-09-11. The `>= 80%` figure governs. No task in this plan is evaluated against an `85%`
   line floor, and the PowerShell aggregate baseline of 83.93 percent recorded at P0-T18 sits above
   the governing floor and below the rule-file figure, so the divergence is load-bearing rather
   than academic. `.claude/rules/**` is push-down-owned from drm-copilot and is not editable by
   this change; the divergence is reported upward, not repaired here.

2. **`## Committed Test Evidence Format` (lines 414-426).** This section is present in the
   execution-worktree copy and absent from the session copy. It states that committed test evidence
   must be a projection of a tool's output and never the tool's raw document, permits exactly three
   forms (a package-level JaCoCo projection of the post-processed Cobertura document, the one-line
   first-party coverage summary, and a trx-derived test-result summary), and prohibits a raw
   coverage collector document and a raw test-platform document from git **in any form, including
   under a feature folder's evidence tree**.

   **Interaction with this plan, flagged for the coordinator and not resolved by the executor.**
   P0-T18 and several later tasks direct Pester to write a JaCoCo document to a path under
   `evidence/baseline/` and `evidence/qa-gates/`, and P0-T25 commits the feature folder with a
   directory-level pathspec. Those XML documents are Pester's own coverage output rather than a
   projection of a post-processed Cobertura document. Whether they fall inside the prohibition is a
   scope question for the coordinator; it is recorded here at the point the policy was read rather
   than discovered at the commit gate. No Phase 0 task executed in this delegation writes or commits
   such a document: this delegation stops before P0-T18.

3. **Step 4 of the C# toolchain** names the `test: MSTest with Coverage (Koverage)` VS Code task or
   a direct invocation of `scripts/vscode/Invoke-MSTestWithCoverage.ps1`, and records that the
   built-in Code Coverage data collector is deliberately withheld from the inner `vstest.console.exe`
   invocation because it conflicts with the outer `dotnet-coverage` instrumentation. CMD-MSTEST-COVERAGE
   in this plan is that direct invocation and is therefore the policy-conformant command.

## Acceptance evaluation

Seven files are listed, in the order the policy-compliance skill fixes, each with a non-zero line
count (463, 80, 105, 97, 96, 51, 80). PASS.

Output Summary: All seven policy documents read in full from the execution worktree; line counts
463/80/105/97/96/51/80, none zero. `CLAUDE.md` differs from the session worktree's copy and the
execution copy governs. Two substantive items recorded: the governing coverage floors are 80 percent
line and 75 percent branch, which diverge from the 85 percent line figure in the two rule files; and
the execution copy carries a `## Committed Test Evidence Format` section whose prohibition on raw
coverage documents under a feature evidence tree is flagged for the coordinator against the JaCoCo
documents later tasks write.
