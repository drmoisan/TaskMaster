# Coverage Floor Exception Runbook

## Cue

Use this runbook only for issue #242 after the orchestrator records that the
repository-wide C# coverage floor remains below policy while the issue-specific
implementation, changed-code coverage, and required C# verification commands
have passed.

## Prerequisites

1. The issue #242 implementation has passed the approved C# verification
   sequence.
2. The feature evidence records 100.00% changed executable production coverage.
3. The feature evidence records repository-wide C# line coverage at 13.64%,
   below the 80% floor.
4. A one-time human exception has been explicitly authorized for this PR.
5. The exception does not modify repository policy and does not apply to future
   work.

## Step-by-step Instructions

1. Record the exception in `artifacts/orchestration/orchestrator-state.json`
   under `human_interaction.requirements[]` with `response` set to `exception`
   and `runbook_path` set to this file.
2. Proceed with PR creation for branch
   `bug/app-events-readiness-comexception-242`.
3. Verify that GitHub CI is green for the PR head SHA before merging.
4. Merge the PR with a merge commit only after CI is green.
5. Do not treat this exception as a general change to repository coverage
   policy.

## Verification

The exception is valid for this workflow only when all of the following are
true:

1. The PR context or PR body documents the coverage exception.
2. The PR head SHA has passing CI.
3. The merge method is `merge`.
4. The checkpoint records the exception and this runbook path.

## Source and Citation

- Source URL: https://github.com/drmoisan/TaskMaster/issues/242
- Source detail: issue comment id `4895097546`
- Captured at: 2026-07-06
- Authorization text: Dan Moisan authorized a one-time exception to the
  repository-wide C# coverage floor for issue #242, limited to opening,
  validating, and merging this PR.
