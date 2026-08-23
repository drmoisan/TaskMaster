Timestamp: 2026-08-11T13-15
Command: Read and line-number `evidence/other/upstream-claude-policy-reconciliation-prompt.2026-08-11T12-41.md`
EXIT_CODE: 0

Required Prompt Sections:
- Lines 9-17, `## Usage boundary`: “Use this prompt in the upstream customization source repository” and “Do not apply the requested `CLAUDE.md` or `.claude/**` changes directly in the TaskMaster repository.”
- Lines 19-29, `## Objective`: identifies TaskMaster issue `#494` and its TaskMaster requirement inputs.
- Lines 43-45, required upstream work item 4: requires documented hook behavior, artifact format, missing-input behavior, line-coverage decision, and branch-coverage disposition to agree with policy.
- Lines 46-48, required upstream work item 5: requires deterministic upstream tests, including a below-threshold negative-path failure signal.
- Lines 49-51, required upstream work item 6: requires regeneration/publication and forbids pushing generated Claude files into TaskMaster.
- Lines 55-63, `## Acceptance criteria`: requires internally consistent coverage rules, fail-closed absent/invalid input behavior, deterministic test coverage, affected generated paths, validation commands, results, and release/publication instructions.
- Lines 67-72, `## Non-goals and constraints`: prohibits TaskMaster `CLAUDE.md` and `.claude/**` edits, preserves the issue #512 C# boundary, and prohibits silently choosing or lowering a threshold.

Determination: `UPSTREAM-ONLY HANDOFF INPUT READY`

Output Summary: The immutable upstream prompt exists and expressly supplies every required boundary and behavior input. It was inspected only; no copy or edit was made.
