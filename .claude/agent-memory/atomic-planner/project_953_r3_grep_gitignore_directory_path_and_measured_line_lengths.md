---
name: project-953-r3-grep-gitignore-directory-path-and-measured-line-lengths
description: "#953 R3 preflight deltas - the Grep tool honours .gitignore when its path is a DIRECTORY (root path + glob artifacts/pester/pester-junit.xml returns no match because artifacts/ is ignored) but matches when given the absolute FILE path; the Glob tool returns the gitignored file either way; a 'none' outcome needs its positive control (root line matched exactly once); line-length predictions (testsuite lines, failure messages, plan lines) must be measured with ^.{N,} probes, not estimated; a leading < in a Grep pattern works when passed as written"
metadata:
  type: project
---

Round 3 preflight deltas on the issue #953 plan (worktree `.claude/worktrees/agent-a5292122c820774d3`, 2026-10-02). Four defects, no task-count change (47), version 1.2 to 1.3.

**Why:** every one is a tool-behaviour fact that reading the plan could not reveal; the reviewer found them by running the Grep tool over a sibling worktree's JUnit document and over the plan itself. The planner reproduced each one before editing.

**How to apply:** before any plan whose executor reads a gitignored document (JUnit, TRX, Cobertura under `artifacts/` or `coverage/`) with the Grep tool, and before any plan that states a line length or an omission prediction.

1. **Grep honours .gitignore for a directory path, Glob does not.** Grep with the worktree root as `path` plus a `glob` naming a gitignored file returns `No matches found`; the same pattern with the absolute file path as `path` returns the match. Glob with the worktree root as `path` returned the same gitignored file. Rule written into C5: a Grep over `artifacts/pester/pester-junit.xml` passes the absolute file path and no glob; Glob existence/absence checks keep the root path. A pattern that depends on this (every CMD-JUNIT-READ Grep) would otherwise record nothing and the root-count-derived `EXIT_CODE` (C3) would have no source.
2. **A `none` outcome needs a positive control in the same task.** `JUNIT-NOTPASSED: none` is valid only when the `<testsuites ` Grep matched exactly one line in that task; a no-match from the root Grep is a failed read, not an empty result. State the condition in every task that records the none outcome (here P0-T12, P1-T15, P2-T3), not only in the command definition.
3. **Measure line lengths, do not estimate them.** The plan said a testsuite line "carries the absolute path twice" so omission was "expected for every testsuite line"; measured at the `.claude/worktrees/agent-<id>` depth with `^.{390,}` / `^.{440,}` it is 390-439 characters and prints in full. The plan said test 14 was "about 3,000 characters"; `^.{2000,}` matches and `^.{2100,}` does not. A failure-message length can be computed from its construction (test 13: 72 + 140 + 88 + 20 + 13 = 333 characters) when it cannot be observed. Write the probe patterns into the plan so the figure is reproducible, and bump the version the figure is pinned to when the revision changes the file.
4. **Do not justify a tool choice by an unobserved property.** P1-T3 said the Read tool is used "because both failure lines are expected to exceed" the omission length; the definition already requires the Read tool for every not-passed testcase whatever the length. Cite the rule, not a prediction.
5. **A leading `<` in a Grep pattern works.** `<testsuites `, `<testsuite name=` and `<bindingRedirect` all matched through the Grep tool when passed as written; an earlier apparent miss came from the planner's own call encoding (`&lt;`). Re-probe with a `<`-free pattern (`testsuites`) before concluding the tool mangles the pattern.
6. **Directive text can carry a version pin.** The round 3 delta text said "at version 1.2"; the revision it belongs to produces version 1.3, so the pin was re-measured after the edits and written as 1.3, with the choice reported.

Related: [[project-953-r2-grep-long-line-omission-and-cr-anchor-seams]], [[project-953-r1-pwsh-refused-and-count-recheck-seams]], [[project-953-fizzler-redirect-sweep-and-ratchet-plan-seams]], [[zero-hit-grep-gates-need-carveouts]].
