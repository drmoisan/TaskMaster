# [P5-T7] Phase 5 commit — issue #473 defect 2

Timestamp: 2026-08-26T10-43

Command:

```
git add -- QuickFiler/Controllers/QfcCollectionController.cs \
           QuickFiler.Test/Controllers/QfcCollectionControllerDefects468MoveTests.cs \
           docs/features/active/qfc-collection-controller-defects-468/plan.2026-08-24T09-39.md \
           docs/features/active/qfc-collection-controller-defects-468/spec.md \
           docs/features/active/qfc-collection-controller-defects-468/evidence/
git commit -m "fix(473): stop swallowing cancellation and double-logging one root failure"
git show --name-only HEAD
```

EXIT_CODE: 0

## Output Summary

Commit `8637aaa814e6dd3f148eba732ed2d669b6665fe8`
`fix(473): stop swallowing cancellation and double-logging one root failure`

### Acceptance verification — no path outside the owned file set

`git show --name-only HEAD | grep -E '\.(cs|csproj)$'` returns exactly two paths:

| Path | Owned because |
|---|---|
| `QuickFiler/Controllers/QfcCollectionController.cs` | `<CTRL>`, the feature's single production file |
| `QuickFiler.Test/Controllers/QfcCollectionControllerDefects468MoveTests.cs` | D12 test file 3 of 5, already registered in the csproj by P4-T2 |

No `.csproj` changed, because the move test file was registered in Phase 4. No path outside the
owned set appears. `git status --porcelain`, filtered to paths outside `.claude/`, is empty after
the commit.

Per D15 the commit also carries the plan checklist, `spec.md` (for the AC check-off), and this
phase's evidence artifacts, including the P4-T9 commit artifact, which could only be written after
the Phase 4 commit existed.

`.claude/agent-memory/**` and `.claude/state/**` remain unstaged; every `git add` used an explicit
pathspec.

### Acceptance criteria checked off in this commit

**AC-13 (#473 defect 2)** — marked `[x]` in `spec.md`. Both halves are positively verified, each by
a named test that was red before the fix:

| Clause | Evidence |
|---|---|
| `OperationCanceledException` propagates out of `MoveEmailsAsync` rather than being swallowed by the broad catch | `MoveEmailsAsync_WhenMoveIsCancelled_PropagatesOperationCanceledException` — red in P5-T1 ("but no exception was thrown"), green in P5-T5 |
| A single root failure produces a single log entry, proven by `VerifyGet(x => x.Subject, Times.Never())` after the first catch | `MoveEmailsAsync_AfterFirstFailure_DoesNotReadSubjectASecondTime` — red in P5-T2 ("should never have been performed, but was 1 times"), green in P5-T5 |
| Verified by two named MSTest tests, each red before the fix | both `[expect-fail]` runs recorded with `ExpectedExitCode: 1` and a failed count of exactly 1, executed **before** P5-T3 applied the fix |

A third test, `MoveEmailsAsync_WithNullGroupFromIndexLookup_DoesNotThrow` (P5-T4), covers the new
guarded null-group path. It is not part of AC-13's two-test requirement and carries no fail-before
artifact, because the plan does not tag it `[expect-fail]`.

### Phase 5 closes the delegated execution window

Phases 2 through 5 are complete. Phase 6 has not been started.

Result: PASS.
