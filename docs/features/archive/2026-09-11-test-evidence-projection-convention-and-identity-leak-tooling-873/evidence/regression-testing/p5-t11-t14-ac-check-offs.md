# P5-T11 through P5-T14 — Acceptance Criteria Check-Offs

Timestamp: 2026-09-13T06-18
Tasks: [P5-T11], [P5-T12], [P5-T13], [P5-T14]

Exactly one criterion is checked off by each task. Four criteria total: AC16, AC17, AC18 and AC19.

## [P5-T11] — AC16, editor settings correction

AC16's checkbox is marked. The P5-T2 artifact records all three observations as holding: the array
element begins with the workspace-folder variable and carries no drive letter and 0 matches for the
run-time-derived account token; the file parses as JSON; and the directory the value resolves to exists
in the repository and contains its symbols document.

## [P5-T12] — AC17, the five named memory files

AC17's checkbox is marked. The P5-T6 artifact records 0 case-insensitive matches for both the account
token and the host token in each of the five files, records that every edited line's backtick delimiters
remain even in number, and records the preserved case-sensitivity contrast for the rewritten file: the
rewritten sentence contains the word case-sensitive and the word case-insensitive, preserves the
nineteen-file figure, contains neither token, and contains no angle-bracket placeholder at all, so it
cannot place the same placeholder twice.

The five named files are this item's named scope. The criterion does not assert that five is the
complete repository population, which the research artifact's broader pattern shows it is not; the
remainder belongs to the repository-wide sweep item.

## [P5-T13] — AC18, convention recorded in a TaskMaster-owned document

AC18's checkbox is marked. The P5-T3 artifact records a heading count of exactly 1 against a baseline of
0, and the P5-T4 artifact records case-sensitive counts of 2 for `/ResultsDirectory:` and 2 for
`/Logger:trx;LogFileName=`, each at least 2, against a baseline of 0 for each.

Command: `git diff --name-status refs/base-anchor-873 -- .claude .vscode config CLAUDE.md TaskMaster`
EXIT_CODE: 0

```
M	.claude/agent-memory/_shared_no_absolute_host_paths.md
M	.claude/agent-memory/epic-orchestrator/feedback_measure_whole_volume_before_blaming_worktrees.md
M	.claude/agent-memory/feature-review/project_464-review-residuals.md
M	.claude/agent-memory/feature-review/project_488-review-residuals.md
M	.claude/agent-memory/orchestrator/angle-bracket-redaction-breaks-trx-xml.md
M	.claude/agent-memory/orchestrator/collect-pr-context-lands-in-main-checkout.md
M	.vscode/settings.json
M	CLAUDE.md
M	TaskMaster/TaskMaster.csproj
```

Command: `git status --porcelain --untracked-files=all`
EXIT_CODE: 0

```
 M .claude/agent-memory/_shared_no_absolute_host_paths.md
 M .claude/agent-memory/epic-orchestrator/feedback_measure_whole_volume_before_blaming_worktrees.md
 M .claude/agent-memory/feature-review/project_464-review-residuals.md
 M .claude/agent-memory/feature-review/project_488-review-residuals.md
 M .claude/agent-memory/orchestrator/angle-bracket-redaction-breaks-trx-xml.md
 M .claude/agent-memory/orchestrator/collect-pr-context-lands-in-main-checkout.md
 M .vscode/settings.json
 M CLAUDE.md
 M TaskMaster/TaskMaster.csproj
 M docs/features/active/<feature-folder>/plan.2026-09-12T10-26.md
 M docs/features/active/<feature-folder>/spec.md
?? docs/features/active/<feature-folder>/evidence/regression-testing/p5-t1-project-file-correction.md
?? docs/features/active/<feature-folder>/evidence/regression-testing/p5-t2-editor-settings-correction.md
?? docs/features/active/<feature-folder>/evidence/regression-testing/p5-t3-convention-section.md
?? docs/features/active/<feature-folder>/evidence/regression-testing/p5-t4-toolchain-step-amendment.md
?? docs/features/active/<feature-folder>/evidence/regression-testing/p5-t5-hygiene-rule-amendment.md
?? docs/features/active/<feature-folder>/evidence/regression-testing/p5-t6-memory-substitutions.md
```

The pairing is required because a name-listing diff enumerates tracked changes only and cannot report an
untracked addition, while a porcelain status goes empty once the change is committed. Neither observation
alone would close the criterion.

Governance findings, both observations agreeing:

- EDITOR_AGENT_RULES_PATH_COUNT: 0. No path under the editor-agent rules directory.
- EDITOR_AGENT_SKILLS_PATH_COUNT: 0.
- EDITOR_AGENT_AGENTS_PATH_COUNT: 0.
- EDITOR_AGENT_HOOKS_PATH_COUNT: 0.
- EDITOR_AGENT_LIB_PATH_COUNT: 0.
- EDITOR_AGENT_SETTINGS_DOCUMENT_COUNT: 0. The editor-agent settings document is the agent settings file
  under the editor-agent directory, which is untouched. `.vscode/settings.json` is a different document:
  it is in this delivery's Write Set, it is the subject of P5-T2 and AC16, and it is not push-down owned.
- SHARED_CONFIGURATION_DOCUMENT_COUNT: 0. Neither of the two shared configuration documents under the
  repository configuration directory — the blast-radius configuration and the orchestration-routing
  configuration — appears in either listing.

The only paths under the editor-agent directory in either listing are the six agent-memory documents
this delivery's Write Set names, which the criterion does not exclude.

## [P5-T14] — AC19, hygiene rule text amended

AC19's checkbox is marked. The P5-T5 artifact records both rules present in
`.claude/agent-memory/_shared_no_absolute_host_paths.md`, each as its own numbered statement in a new
section, and records a fenced-code-block delimiter count of 0 for the whole file, so no executable sweep
was added.

Output Summary: Four criteria checked off, one per task — AC16, AC17, AC18 and AC19. The anchored
name-listing diff paired with the porcelain status shows zero paths under any push-down-owned governance
tree and zero shared configuration documents.
