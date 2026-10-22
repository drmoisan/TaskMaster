# P5-T15 — Closure Record

Timestamp: 2026-09-17T02-40

Command (the git spans of this task, each a separate invocation):

1. `git add -- QuickFiler.Test/Viewers/ItemViewerBreadcrumbThreadAffinityTests.cs docs/features/active/breadcrumb-thread-affinity-tests-assume-taskrun-distinct-thread-900`
2. `git commit -F docs/features/active/breadcrumb-thread-affinity-tests-assume-taskrun-distinct-thread-900/evidence/other/commit-message-final.txt`
3. `git rev-parse HEAD`
4. `git diff --name-only origin/main...HEAD -- QuickFiler QuickFiler.Test`
5. `git diff --name-only 66b65a4626095ade5a01643aee4a43c90cc58cbf..HEAD -- QuickFiler QuickFiler.Test`
6. `git add -- docs/features/active/breadcrumb-thread-affinity-tests-assume-taskrun-distinct-thread-900`
7. `git commit --amend --no-edit`
8. `git status --porcelain -- QuickFiler QuickFiler.Test docs/features/active/breadcrumb-thread-affinity-tests-assume-taskrun-distinct-thread-900`
9. `git rev-parse HEAD`

CHANNEL: NONE

## Commit 1

COMMIT-1-EXIT: 0

HEAD-AFTER-COMMIT: `59644e182e8104c262d693749430b66d7f5f7aed`

Commit summary as reported: `24 files changed, 1661 insertions(+), 34 deletions(-)`, subject
`docs(900): evidence, acceptance check-off and plan state for the thread-affinity test fix`.

The message file carries no attribution trailer, for the reason recorded in P0-T11 and P2-T5.

## Confirming AC7 lists, on the committed tip

`git diff --name-only origin/main...HEAD -- QuickFiler QuickFiler.Test`:

    QuickFiler.Test/Viewers/ItemViewerBreadcrumbThreadAffinityTests.cs

`git diff --name-only 66b65a4626095ade5a01643aee4a43c90cc58cbf..HEAD -- QuickFiler QuickFiler.Test`:

    QuickFiler.Test/Viewers/ItemViewerBreadcrumbThreadAffinityTests.cs

Each list is exactly the one test file path. P5-T9 measured the same boundary on the working tree
and on the history before this commit; these two lists confirm it on the committed tip, after every
evidence artifact was staged. The `git add` span in step 1 is the staging companion that makes these
name-listing diffs complete, since an anchored name-listing diff is otherwise blind to files that
are not yet tracked.

No production file is modified. No file under `QuickFiler/` appears in either list, and no other
file under `QuickFiler.Test/` appears in either list.

## Follow-ups

Four potential entries are handed to the orchestrator for filing through
`mcp__drm-copilot__new_potential_bug_entry`. The bodies are in
`evidence/other/p5-t14-follow-up-handoff.2026-09-17T02-39.md`. The executor has no MCP tool surface
and created no file under `docs/features/potential/`.

## Acceptance Criteria Status

- Source: `docs/features/active/breadcrumb-thread-affinity-tests-assume-taskrun-distinct-thread-900/spec.md`
- Total AC items: 8
- Checked off (delivered): 8
- Remaining (unchecked): 0
- Items remaining: none

The `Checked off` count is taken from the `- [x] AC` lines on disk: a search for
`^- \[[ x]\] AC[1-8]\. ` over `spec.md` returns 8 lines, and all 8 begin `- [x] `. `spec.md` is the
sole acceptance-criteria source for this item, because `issue.md` line 4 carries
`- Work Mode: full-bug` and `user-story.md` does not exist.

Evidence per criterion:

| AC | Checked at | Measured evidence |
| --- | --- | --- |
| AC1 | P4-T4 | P2-T2 census: `Task.Run(` 3 to 1, `.GetAwaiter()` 3 to 1, `new Thread(` 0 to 1, `thread.Join();` 0 to 1, `Exception captured = RunOnDedicatedWorkerThread(` 0 to 2; P4-T2 both pass |
| AC2 | P4-T5 | P2-T2 census: `UiDispatcher.CheckAccess()` 0 to 2, `isOwnerThread` 0 to 4; P3-T3 both tests observed failing on that precondition with `vacuously` in the message |
| AC3 | P4-T6 | P2-T2 census: `BeOfType<InvalidOperationException>()` 2, `captured.Message.Should().Contain(` 2, `NotBeOfType<ObjectDisposedException>()` 2; P4-T2 pass; P3-T1 message assertion observed failing |
| AC4 | P1-T3 | `fail-before-exception.2026-09-17T02-19.md` with all four required sections |
| AC5 | P4-T7 | P3-T1 guard-disabled failing run with `ExpectedExitCode: 1`; P3-T2 and P3-T4 revert proofs (anchored diff exit 0, empty porcelain, `SHA256` equal to `FIX-HASH:`); P4-T2 pass-after |
| AC6 | P4-T8 | P4-T2 measured: exit 0, 2 of 2 passed under the CLI runsettings with `RUNSETTINGS-HASH-NOW:` unchanged; P4-T3 confirming; P5-T5 both pass inside the full 7288-test run |
| AC7 | P5-T10 | P4-T3 all seven pass; P5-T9 `CHANGED-PATHS:` exactly the test file; the two lists above on the committed tip |
| AC8 | P5-T11 | P5-T8 `LOOP: CLEAN PASS` over P5-T1 through P5-T7 in order, `ITERATIONS: 2`; P5-T6 numeric baseline and final coverage with `COMPARABILITY: A` and `CHANGED-CODE COVERAGE: NOT MEASURABLE` |

## Headline figures

- Repository-wide suite after the change: 7288 of 7288 passed across 9 assemblies, collector exit 0.
- Baseline `line-rate` 0.852658; final `line-rate` 0.852566; `lines-valid` identical at 65616.
- Target file: 490 lines, under the 500-line limit; SHA-256
  `8EBC19F829536957BEB0DAEC628929CA8DE3F11E10AA819DEA41362C6FCBB164`, unchanged since the P2-T5 fix
  commit.

## Post-amend state

EXIT_CODE: 0

This artifact's exit-code field is scoped to the step-5 porcelain span, the observation this task
actually runs and names. The commit exit codes are recorded separately: `COMMIT-1-EXIT:` above, and
`COMMIT-2-EXIT:` in the executor's final message, because an artifact cannot record the exit code of
the commit that includes it. This is the fail-closed evidence rule's scoping carve-out.

POST-AMEND-PORCELAIN: EMPTY

`git status --porcelain -- QuickFiler QuickFiler.Test docs/features/active/breadcrumb-thread-affinity-tests-assume-taskrun-distinct-thread-900`
printed nothing. The source tree and the feature tree are both clean: every artifact this run
produced is committed, and no source file carries an uncommitted edit.

FINAL-HEAD: `acd13553065e88612696225b5485fb66f51c8a0e`

That value is the tip after the first amend, read before the second amend that folds these three
fields into the commit. The second amend necessarily produces a further commit object, whose SHA the
executor reports in its final message; an artifact cannot name the SHA of the commit that contains
it.

`.claude/agent-memory/` paths lie outside every pathspec used by this task and are not this task's
residual. Nothing under that prefix was created or modified by this run.

