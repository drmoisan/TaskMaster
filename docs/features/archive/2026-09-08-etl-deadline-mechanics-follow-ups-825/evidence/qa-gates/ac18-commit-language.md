# AC18 — Commit Language, Clean Tree and Write Set Accounting

Timestamp: 2026-09-09T17-36

CommitSha: 4c607bd9569f96516920bd7fe78fb0ab43a2c0f0

Command: git status --porcelain --untracked-files=all -- . ":(exclude).claude"
EXIT_CODE: 0
PorcelainLines: 0

Command: git log -1 --pretty=%B
EXIT_CODE: 0
SoakLinesInMessage: 0
Resolv500LinesInMessage: 0

Command: git diff --name-only $b -- . ":(exclude).claude", with $b re-derived from evidence/baseline/base-commit.md per D3
EXIT_CODE: 0
ReportedPaths: 59

## Commit language

The commit message contains zero lines containing the token `soak` and zero lines matching the
regular expression `resolv.*500`. It describes the TimeOutTask.cs change as taking the file "from
1011 lines to 966" and states in terms that "That is a reduction of the 500-line cap violation, not a
resolution of it; the file remains 466 lines above the cap". No artifact, code comment, commit
message or PR body produced by this feature claims the cap violation is resolved.

## Clean tree

The porcelain span produces zero output lines, so nothing was left untracked or uncommitted. It is
the companion the name-listing diff requires: the diff enumerates tracked changes only, and the
zero-line porcelain result is what proves nothing escaped it. The D4 exclusion pathspec is applied
because the .claude/agent-memory tree is tracked and this executor writes to it during the run.

## Write Set accounting

Direction one — every reported path is admissible. UnexpectedPaths is 0: each of the 59 paths is
either one of the eleven backticked Write Set paths from spec.md or a path under
docs/features/active/2026-09-08-etl-deadline-mechanics-follow-ups-825/, and nothing else.

Direction two — every Write Set path is present. MissingWriteSetPaths is 0. All eleven appear,
including UtilitiesCS.Test/Extensions/DfDeedleEtlTimeoutTests.cs, which the Write Set gained on
2026-09-09 with the AC6 amendment and which this plan edits at P3-T7 and P3-T8.

The 48 remaining paths are this feature's own documents: 44 evidence artifacts across baseline,
issue-updates, other, qa-gates and regression-testing, plus issue.md, plan.2026-09-08T23-51.md and
spec.md.

## D7 confirming check — no host account name in any committed log

Each of the five committed MSBuild file logs contains zero lines containing the token `C:\Users\`:

| Log | C:\Users\ lines |
| --- | --- |
| evidence/baseline/build-analyzers.txt | 0 |
| evidence/baseline/build-nullable.txt | 0 |
| evidence/other/ac8-createcancellationtokensource-proof.txt | 0 |
| evidence/qa-gates/qc-build-analyzers.txt | 0 |
| evidence/qa-gates/qc-build-nullable.txt | 0 |

Every log ends in .txt rather than .log, because .gitignore line 84 ignores files ending in .log and
such a log would never have been committed at all.

Each log was sanitised as the last action of the task that wrote it, after every count that task
read from it. The counted tokens carry no absolute path and are unaffected. Three rewrites were
applied in order: the worktree root to the literal `<repo-root>`, the main checkout root to the
literal `<main-checkout-root>`, and the user profile root to the literal `<user-profile-root>`. The
third is a documented deviation from D7, which names only the first two; it was required because two
further leak classes survive them, an MSBuildUserExtensionsPath property expanded from the
environment and a _DeploymentUrl property reassignment naming a OneDrive folder, and both carry the
host account name. The deviation is recorded in full at evidence/other/plan-deviations.md.
