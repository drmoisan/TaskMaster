# Commit 2 — evidence, coverage comparison and acceptance check-off — issue #839

Timestamp: 2026-09-13T06-25
Command: git status --porcelain --untracked-files=all -- docs/features/active/2026-09-09-createcancellationtoken-has-no-production-caller-839
Command: git add -- docs/features/active/2026-09-09-createcancellationtoken-has-no-production-caller-839
Command: git commit -m "docs(839): evidence, coverage comparison and acceptance check-off"
EXIT_CODE: 0

## Output Summary

This artifact is written BEFORE the add and the commit, so the `EXIT_CODE:` above is the exit code of the scoped `git status --porcelain` invocation, per Decision D12. That invocation exits 0 whether or not it prints a line. The commit's own exit code and the resulting HEAD SHA are reported in the executor's return rather than written here, because this artifact is one of the files the commit stages.

Residue observed, seven lines, every one under the feature folder:

     M docs/features/active/2026-09-09-createcancellationtoken-has-no-production-caller-839/plan.2026-09-12T22-14.md
     M docs/features/active/2026-09-09-createcancellationtoken-has-no-production-caller-839/spec.md
    ?? docs/features/active/2026-09-09-createcancellationtoken-has-no-production-caller-839/evidence/qa-gates/ac-status-summary.md
    ?? docs/features/active/2026-09-09-createcancellationtoken-has-no-production-caller-839/evidence/qa-gates/commit-1.md
    ?? docs/features/active/2026-09-09-createcancellationtoken-has-no-production-caller-839/evidence/qa-gates/evidence-sanitization.md
    ?? docs/features/active/2026-09-09-createcancellationtoken-has-no-production-caller-839/evidence/qa-gates/post-checkoff-revalidation.md
    ?? docs/features/active/2026-09-09-createcancellationtoken-has-no-production-caller-839/evidence/qa-gates/scope-and-footprint.md

The residue is non-empty, which this task's acceptance requires. Its composition is exactly what the plan predicts at this point:

- The plan file carries the task check-off marks written after the [P3-T12] commit.
- spec.md carries the twelve acceptance-criteria transitions from `- [ ]` to `- [x]` written by [P3-T14] through [P3-T25], and no other edit.
- Five evidence artifacts were written after [P3-T12]: the commit-1 record itself, the AC10 footprint gate, the AC status summary, the post-check-off re-validation and the sanitisation gate.

No path outside the feature folder appears, and no residue line ends in .xml, .trx or .coverage. The D10 residue classes are empty in this worktree: no agent-memory, potential-tree or parallel-tree path is present, consistent with the [P0-T8] snapshot recording `INHERITED-PORCELAIN: NONE`.

This artifact itself is untracked at the moment of observation and so does not appear in the list above; it is staged by the very command whose result it records. One further path will remain modified after the commit, the plan file, because this task's own `[x]` mark can only be written after the commit it records. That mark is committed by a third, artifact-free commit described in this task's text, whose SHA is reported in the executor's return.

## Staging discipline

Explicit pathspec only, one of them, the feature folder. Neither `git add -A` nor `git add .` was used anywhere in this run, per Decision D10.

## Command-transport note

All three git invocations were addressed to the assigned worktree with a repository-location option in place of a working-directory change, forced by the Bash allowlist and by this executor's inherited working directory, which is a different worktree from the assigned one. The subcommands, switches, the `-m` message option and the pathspec operand are exactly as the plan writes them. The commit carries a second `-m` value supplying the required co-authorship trailer, which adds a message paragraph and no pathspec.
