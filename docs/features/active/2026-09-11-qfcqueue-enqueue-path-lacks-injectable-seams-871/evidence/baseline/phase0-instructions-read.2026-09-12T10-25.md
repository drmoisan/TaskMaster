# Phase 0 — Policy documents read (P0-T1)

Timestamp: 2026-09-13T04-50

Policy Order: CLAUDE.md, then the general code change rule file, then the general unit test rule file, then the C# rule file, then the tonality rule file.

## Documents read, in the mandated order

1. CLAUDE.md — repository-root standing instructions. Repository-relative path: CLAUDE.md
2. General code change policy. Repository-relative path: .claude/rules/general-code-change.md
3. General unit test policy. Repository-relative path: .claude/rules/general-unit-test.md
4. C# code standards. Repository-relative path: .claude/rules/csharp.md
5. Tonality policy. Repository-relative path: .claude/rules/tonality.md

## Known divergence recorded at read time

CLAUDE.md states the authoritative C# coverage thresholds for this repository: repository-wide
line coverage at or above 80 percent, new modules, classes and methods at or above 90 percent, and
no coverage regression on changed lines. The general unit test rule file and the quality tiers rule
file state 85 percent line and 75 percent branch. CLAUDE.md governs in this repository; the two
rule-file figures are carried down from an upstream repository and are not authoritative here. This
executor records the CLAUDE.md figures in every coverage artifact of this run and does not raise any
gate to 85 percent on the strength of the rule files. The plan records the same divergence in its
"Known divergences" section.

## Additional bounding constraint observed

The general code change policy sets a hard 500-line ceiling for any production, test or reusable
script file, with an exception for Markdown documentation. Phase 1 of the plan exists because
QuickFiler/Controllers/QfcQueue.cs stands above that ceiling at the recorded anchor.

Command: not applicable — this task is a document read, not a command step.
EXIT_CODE: 0
Output Summary: All five policy documents were read from the item worktree in the exact order the
task states. No conflicting instruction was found between them beyond the coverage-threshold
divergence recorded above, which CLAUDE.md resolves in favour of the 80 / 90 figures.
