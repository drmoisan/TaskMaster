# Commit 1 — source fix, regression test and evidence — issue #839

Timestamp: 2026-09-13T06-20
Command: git add -- QuickFiler/Controllers/QfcHomeController.cs QuickFiler.Test/Controllers/QfcHomeControllerTests.cs docs/features/active/2026-09-09-createcancellationtoken-has-no-production-caller-839
Command: git commit -m "fix(quickfiler): call CreateCancellationToken first in QfcHomeController.Init (#839)"
Command: git rev-parse HEAD
EXIT_CODE: 0
COMMIT-1-SHA: 3b6cd70b468603a9b89af7791c4d6b18f3abc019

## Output Summary

The commit succeeded and reported 14 files changed, 504 insertions and 13 deletions. Paths the commit reported:

Modified, the two owned source files and the plan:

    QuickFiler/Controllers/QfcHomeController.cs
    QuickFiler.Test/Controllers/QfcHomeControllerTests.cs
    docs/features/active/2026-09-09-createcancellationtoken-has-no-production-caller-839/plan.2026-09-12T22-14.md

Created, eleven evidence artifacts under evidence/qa-gates/:

    coverage-comparison.md
    family-count.md
    file-size-audit.md
    final-analyzers.md
    final-format.md
    final-nullable.md
    final-tests.md
    production-file-gates.md
    sibling-and-followup-gates.md
    test-file-gates.md
    toolchain-final-pass.md

`COMMIT-1-SHA:` is 3b6cd70b468603a9b89af7791c4d6b18f3abc019, which differs from the `HEAD-SHA:` recorded in evidence/baseline/base-anchor.md, so the branch advanced.

`git status --porcelain -- QuickFiler QuickFiler.Test` prints nothing after the commit: both owned source files are committed and neither source tree carries any residue.

## Staging discipline

Explicit pathspecs only, three of them, all inside the Write Set. Neither `git add -A` nor `git add .` was used at any point in this run, per Decision D10 and the recorded prior incident in which a blanket add swept another item's queued promotion file onto the wrong branch.

The Phase 0, Phase 1 and Phase 2 boundary commits that precede this one staged only the feature folder, deliberately leaving the two source files uncommitted until this task, so that this task had real content to stage and its acceptance conditions could fail. Their SHAs are reported in the executor's return.

`EXIT_CODE: 0` above is the exit code of the `git rev-parse HEAD` invocation, per the artifact's single-field rule. The commit's own result is reported in the executor's return rather than written here, because this artifact is committed by the following commit task.

## Command-transport note

All three git invocations were addressed to the assigned worktree with a repository-location option in place of a working-directory change, forced by the Bash allowlist and by this executor's inherited working directory, which is a different worktree from the assigned one. The subcommands, the `-m` message option and every pathspec operand are exactly as the plan writes them. The commit carries a second `-m` value supplying the required co-authorship trailer, which adds a message paragraph and no pathspec.
