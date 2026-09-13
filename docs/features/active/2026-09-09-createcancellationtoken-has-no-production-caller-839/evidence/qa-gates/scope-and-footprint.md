# AC10 scope and footprint gate — issue #839

Timestamp: 2026-09-13T06-21
Command: git diff --name-only 2405a829d6afd3b12eb7c228d57158a97cb4e2ca HEAD
Command: git diff --name-only --diff-filter=A 2405a829d6afd3b12eb7c228d57158a97cb4e2ca HEAD
Command: git status --porcelain --untracked-files=all
EXIT_CODE: 0

## Output Summary

The anchored name-only diff against HEAD prints 37 lines. Every line matches one of the three Write Set entries: the two named source files, or a path beginning docs/features/active/2026-09-09-createcancellationtoken-has-no-production-caller-839/.

INHERITED-AND-EXCLUDED:

    docs/features/active/2026-09-09-createcancellationtoken-has-no-production-caller-839/issue.md
    docs/features/active/2026-09-09-createcancellationtoken-has-no-production-caller-839/plan.2026-09-12T22-14.md
    docs/features/active/2026-09-09-createcancellationtoken-has-no-production-caller-839/research/2026-09-12T18-05-createcancellationtoken-init-path-research.md
    docs/features/active/2026-09-09-createcancellationtoken-has-no-production-caller-839/spec.md
    docs/features/active/2026-09-09-createcancellationtoken-has-no-production-caller-839/user-story.md

Those five paths are exactly the [P0-T8] `INHERITED-DIFF-PATHS:` set, recorded in evidence/baseline/base-anchor.md lines 20 to 24 and re-read for this gate rather than recalled. Every one of them also begins with the feature-folder prefix, so each is simultaneously an inherited path and a Write Set path; listing them here records the subtraction the plan requires without implying they are foreign.

THIS-ITEM-FOOTPRINT:

    QuickFiler/Controllers/QfcHomeController.cs
    QuickFiler.Test/Controllers/QfcHomeControllerTests.cs
    docs/features/active/2026-09-09-createcancellationtoken-has-no-production-caller-839/evidence/baseline/base-anchor.md
    docs/features/active/2026-09-09-createcancellationtoken-has-no-production-caller-839/evidence/baseline/baseline-analyzers.md
    docs/features/active/2026-09-09-createcancellationtoken-has-no-production-caller-839/evidence/baseline/baseline-format-check.md
    docs/features/active/2026-09-09-createcancellationtoken-has-no-production-caller-839/evidence/baseline/baseline-nullable.md
    docs/features/active/2026-09-09-createcancellationtoken-has-no-production-caller-839/evidence/baseline/baseline-quickfiler-tests.md
    docs/features/active/2026-09-09-createcancellationtoken-has-no-production-caller-839/evidence/baseline/bootstrap-dotnet-coverage.md
    docs/features/active/2026-09-09-createcancellationtoken-has-no-production-caller-839/evidence/baseline/bootstrap-nuget-restore.md
    docs/features/active/2026-09-09-createcancellationtoken-has-no-production-caller-839/evidence/baseline/bootstrap-sdk.md
    docs/features/active/2026-09-09-createcancellationtoken-has-no-production-caller-839/evidence/baseline/bootstrap-tool-restore.md
    docs/features/active/2026-09-09-createcancellationtoken-has-no-production-caller-839/evidence/baseline/coverage-baseline.md
    docs/features/active/2026-09-09-createcancellationtoken-has-no-production-caller-839/evidence/baseline/outlook-precondition.md
    docs/features/active/2026-09-09-createcancellationtoken-has-no-production-caller-839/evidence/baseline/phase0-instructions-read.md
    docs/features/active/2026-09-09-createcancellationtoken-has-no-production-caller-839/evidence/baseline/pre-fix-facts.md
    docs/features/active/2026-09-09-createcancellationtoken-has-no-production-caller-839/evidence/qa-gates/coverage-comparison.md
    docs/features/active/2026-09-09-createcancellationtoken-has-no-production-caller-839/evidence/qa-gates/family-count.md
    docs/features/active/2026-09-09-createcancellationtoken-has-no-production-caller-839/evidence/qa-gates/file-size-audit.md
    docs/features/active/2026-09-09-createcancellationtoken-has-no-production-caller-839/evidence/qa-gates/final-analyzers.md
    docs/features/active/2026-09-09-createcancellationtoken-has-no-production-caller-839/evidence/qa-gates/final-format.md
    docs/features/active/2026-09-09-createcancellationtoken-has-no-production-caller-839/evidence/qa-gates/final-nullable.md
    docs/features/active/2026-09-09-createcancellationtoken-has-no-production-caller-839/evidence/qa-gates/final-tests.md
    docs/features/active/2026-09-09-createcancellationtoken-has-no-production-caller-839/evidence/qa-gates/production-file-gates.md
    docs/features/active/2026-09-09-createcancellationtoken-has-no-production-caller-839/evidence/qa-gates/sibling-and-followup-gates.md
    docs/features/active/2026-09-09-createcancellationtoken-has-no-production-caller-839/evidence/qa-gates/test-file-gates.md
    docs/features/active/2026-09-09-createcancellationtoken-has-no-production-caller-839/evidence/qa-gates/toolchain-final-pass.md
    docs/features/active/2026-09-09-createcancellationtoken-has-no-production-caller-839/evidence/regression-testing/init-token-source-fail-before.md
    docs/features/active/2026-09-09-createcancellationtoken-has-no-production-caller-839/evidence/regression-testing/init-token-source-pass-after.md
    docs/features/active/2026-09-09-createcancellationtoken-has-no-production-caller-839/evidence/regression-testing/init-token-source-scoped-pass.md
    docs/features/active/2026-09-09-createcancellationtoken-has-no-production-caller-839/evidence/regression-testing/p1-build.md
    docs/features/active/2026-09-09-createcancellationtoken-has-no-production-caller-839/evidence/regression-testing/p2-build.md
    docs/features/active/2026-09-09-createcancellationtoken-has-no-production-caller-839/evidence/regression-testing/post-fix-structure.md

The AC10 judgement is made on `THIS-ITEM-FOOTPRINT:`, and every one of its 32 entries is a Write Set path: two named source files and thirty paths under the feature-folder glob.

Added-files diff, 35 lines, all under the feature folder. No source file is added by this item, which is consistent with the plan adding no new .cs file and editing no project file.

## Excluded files, none present

None of the files named in the plan's "Out of scope" section appears in either list. Checked individually against both: the Metrics partial of the home controller, the collection controller, the item controller and its Initialization partial, the form controller and its Actions partial, EfcHomeController.cs, RibbonController.cs, QfcHomeControllerMetricsTests.cs, QuickFiler.csproj and QuickFiler.Test.csproj. QfcHomeControllerCleanupTests.cs is likewise absent, which is the same fact the empty stat diff in evidence/qa-gates/sibling-and-followup-gates.md establishes for AC6.

The two legacy project files being absent confirms that no compile item was added: the regression test went into a file already registered in QuickFiler.Test.csproj.

## Raw-artifact conditions

No line of either diff command ends in .xml, .trx or .coverage. All 37 diff lines end in .cs or .md.

Porcelain companion span, the observation the anchored diff cannot make because it is blind to untracked files. It prints two lines and neither ends in .xml, .trx or .coverage:

     M docs/features/active/2026-09-09-createcancellationtoken-has-no-production-caller-839/plan.2026-09-12T22-14.md
    ?? docs/features/active/2026-09-09-createcancellationtoken-has-no-production-caller-839/evidence/qa-gates/commit-1.md

Both are admitted. The plan-file line is present on every correct run: the check-off protocol writes a mark into that file as each task's verification passes, and [P3-T12] committed the file in the state it held before its own mark existed. It is admitted for the porcelain span only and is not subtracted from the anchored diff, where it remains a Write Set path inside `THIS-ITEM-FOOTPRINT:`. The commit-1.md line is a path under the feature folder's evidence tree written after [P3-T12], committed by [P3-T29].

No path under docs/features/potential/ appears in either diff or in the porcelain span, which is the condition AC12 reads from this artifact. The D10 residue classes are empty in this worktree at this observation: the [P0-T8] snapshot recorded `INHERITED-PORCELAIN: NONE`, and no agent-memory, potential-tree or parallel-tree path appears above.

## Command-transport note

All three git invocations were addressed to the assigned worktree with a repository-location option in place of a working-directory change, forced by the Bash allowlist and by this executor's inherited working directory, which is a different worktree from the assigned one. Both diffs carry the explicit base-commit and HEAD operands exactly as the plan writes them, so neither is an unanchored worktree-against-index comparison.
