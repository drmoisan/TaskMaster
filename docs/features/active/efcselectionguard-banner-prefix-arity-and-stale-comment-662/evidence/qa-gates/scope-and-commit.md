# Scope Confirmation and Final Commit (P2-T23)

Timestamp: 2026-09-01T17-02

Command: `git diff 43dcc800e5c75ab1d1033f0eac0e4b61ac919b59 --name-only -- '*.cs' '*.csproj' '*.props' '*.targets' '*.xml' 'packages.config' ':(exclude)docs/features/active/efcselectionguard-banner-prefix-arity-and-stale-comment-662'`

EXIT_CODE: 0

Command: `git status --porcelain -uall -- '*.cs' '*.csproj' '*.props' '*.targets' '*.xml' 'packages.config' ':(exclude)docs/features/active/efcselectionguard-banner-prefix-arity-and-stale-comment-662'`

EXIT_CODE: 0

Output Summary:

## The two scope listings

**Anchored diff listing** — 4 paths:

```
QuickFiler.Test/Controllers/EfcSelectionGuardTests.cs
QuickFiler/Controllers/EfcFormController.cs
QuickFiler/Controllers/EfcSelectionGuard.cs
UtilitiesCS/OutlookObjects/Folder/FolderSuggestionTree.cs
```

**Companion status listing** — empty (no output). All source work was already
committed by the Phase 1 and loop-evidence commits, which is why the porcelain
status is empty; the anchored diff is what reports it.

**Union of the two listings** — exactly the four in-scope files named in the
Scope Boundary:

1. `QuickFiler/Controllers/EfcSelectionGuard.cs`
2. `QuickFiler/Controllers/EfcFormController.cs`
3. `UtilitiesCS/OutlookObjects/Folder/FolderSuggestionTree.cs`
4. `QuickFiler.Test/Controllers/EfcSelectionGuardTests.cs`

No additional entry appears, so the repository-wide format pass in P2-T1 rewrote
no file outside the three project directories that task observes — including no
`*.xml` and no `packages.config`, both of which CSharpier 1.2.6 also processes.
The "stop and report before committing" branch does not arise.

### Anchor

The anchor is the run-time-resolved `git merge-base origin/main HEAD`, per the
plan's Execution Amendment, which overrides the pinned
`2b85134b42872e405602e6064e02dc9cda6c319b` for P2-T16, P2-T18 and P2-T23. It was
re-derived at this final boundary after a `git fetch origin`:

```
git merge-base origin/main HEAD  -> 43dcc800e5c75ab1d1033f0eac0e4b61ac919b59
git rev-parse origin/main        -> 43dcc800e5c75ab1d1033f0eac0e4b61ac919b59
```

`origin/main` has not advanced during this run, so no further reconciliation
merge was required.

### Why each operand of the span is present

- The `:(exclude)` operand removes this feature folder from both listings.
  P0-T13 and P2-T9 write `coverage-baseline.cobertura.xml` and
  `coverage-postchange.cobertura.xml` under `evidence/`, and without the
  exclusion the `'*.xml'` operand would list those two evidence artifacts and the
  union could never equal the four in-scope files. The exclusion does not blind
  the check to a formatter rewrite, because `.csharpierignore:4` excludes
  `**/evidence/**` from CSharpier altogether.
- `-uall` is supplied on the status span because the default untracked-file mode
  collapses a new directory to one entry, which does not match the
  file-extension pathspecs and would hide an out-of-scope file created inside it.
- The anchored diff and the porcelain status are complementary: the anchored diff
  cannot report an untracked path, and the status goes empty once the change is
  committed.

## Artifact-hygiene sweep

The sweep defined in the Fail-Closed Evidence Rules was run over this feature's
`evidence/` tree before this task's check-off and before the `git add`, because
the `git add` span stages the artifacts as they stand on disk.

- `FilesRewritten=` 0
- `ResidualMatchCount=` 0

Token classes substituted: worktree-root prefix, user-profile path,
`computerName`, `runUser`, `storage`, Cobertura `filename`.

`FilesRewritten=` is 0 because the sweep run before the loop-evidence commit had
already rewritten the four artifacts that carried a host token — the two
post-change TRX files and the two scoped-run TRX files — and every artifact
authored since was written without one. `ResidualMatchCount=` is 0, so the
BLOCKED branch does not arise.

## Agent memory

Command: `git status --porcelain -uall -- .claude/agent-memory`

Result: recorded in the "Agent-memory status" section appended below, after the
main commit was made.

Files written under `.claude/agent-memory/` are tracked but lie outside every
pathspec in this plan. Keeping them in a separate commit is what prevents the
`git add` spans in this plan from sweeping an unrelated queued change onto this
branch.

## Agent-memory status

Observed after the main commit, which is the point at which this task's ordering
makes it observable.

Command: `git status --porcelain -uall -- .claude/agent-memory`

Output:

```
 M .claude/agent-memory/atomic-executor/MEMORY.md
?? .claude/agent-memory/atomic-executor/feedback_never_predict_an_observation_into_an_artifact.md
?? .claude/agent-memory/atomic-executor/project_full_suite_run_hangs_while_earlier_runs_idle.md
```

The status is non-empty, so `.claude/agent-memory` is staged and committed in a
separate follow-up commit whose message names agent memory and issue 662. This
artifact is staged in that same `git add`, per this task's stated ordering: the
artifact update is itself committed before the scoped status span whose
acceptance is "no output" is evaluated last, so that the update cannot leave the
feature folder dirty and make the acceptance unreachable.

The two new memory files record lessons from this run: a full-assembly test run
that hung although the byte-identical baseline command had passed, and the rule
that an observed value must never be written into an evidence artifact before it
is observed. `MEMORY.md` is modified because both entries were indexed and
because a repository hook required the index to be compacted below its read
limit; it is now 102 lines.

## Final scoped status

Evaluated last, after the follow-up commit above. Recorded in the run report.
