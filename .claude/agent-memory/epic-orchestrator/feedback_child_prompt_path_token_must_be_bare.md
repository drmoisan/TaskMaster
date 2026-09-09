---
name: child-prompt-path-token-must-be-bare
description: Epic child delegation prompts must carry a BARE docs/features/active/<folder> token plus an explicit issue_num line, or the preimplementation gate and wave barrier deny the spawn
metadata:
  type: feedback
---

Write the child's feature-folder path as a **bare token with no trailing punctuation**, and always
include a literal `issue_num: <N>` line in the prompt.

**Why:** Two independent hooks resolve the delegation target by scanning the prompt text with the
regex `docs[\\/]+features[\\/]+active[\\/]+[^\s"'`+'`'+`]+`, taking the **longest** match, and using its
basename:

- `.claude/hooks/enforce-epic-wave-barrier.ps1` (`Find-EpicWaveBarrierFeatureFolderFromPrompt`)
- `.claude/hooks/enforce-orchestration-preimplementation-gate-modes.ps1`
  (`Find-OrchestrationModeRecord`, epic conjunct `target-record`)

The character class excludes only whitespace, quotes and backticks. It does **not** exclude `)`,
`,`, `.` or `;`. So writing

    You are resuming feature 825 (docs/features/active/2026-09-08-etl-deadline-mechanics-follow-ups-825)

matches the path **including the closing paren**, yielding the basename
`...-follow-ups-825)`, which equals no `features[].feature_folder` value. `Find-OrchestrationModeRecord`
returns `$null` and the spawn is denied with
`PREIMPLEMENTATION_GATE_BLOCKED: ... the failed readiness predicate is 'target-record'`. Verified
2026-09-09 on the review-residuals-2026-09-08 epic: the identical prompt succeeded once the paren was
removed. The gate is behaving correctly; the prompt was malformed.

`Find-OrchestrationModeRecord` falls back to matching `issue_num`, but only if the caller extracted an
issue number from the prompt — so a prompt lacking an explicit `issue_num:` line loses that safety net
too. My six successful launches all carried an `- issue_num: <N>` bullet; the one failure was the
resume prompt where I dropped it.

Note the gate normalizes the CHECKPOINT side to a basename (`Get-OrchestrationModeFolderBasename`), so
a full-path `feature_folder` in the checkpoint is tolerated *here* — but the wave-barrier hook does a
raw string equality and is not so forgiving. See [[epic-checkpoint-schema-gotchas]].

**How to apply:** In every `Agent(orchestrator)` epic prompt, put the folder on its own line as a
bullet value (`- feature folder: docs/features/active/<name>`) with nothing after it, keep an
`- issue_num: <N>` bullet beside it, and never wrap the path in parentheses or end the sentence with
it. When a `target-record` deny appears, suspect your own prompt punctuation before suspecting the
checkpoint.
