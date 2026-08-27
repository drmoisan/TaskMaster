# [P1-T9] Dead-code removal commit

Timestamp: 2026-08-26T08-45

Command: `git add QuickFiler/Controllers/QfcCollectionController.cs <feature evidence, plan, spec>`
Command: `git commit -m "fix(468): remove unreachable load paths and the dead _templateTlp field" -m "<trailers>"`

EXIT_CODE: 0

ExpectedExitCode: 0

## Output Summary

Commit: `63eebd47ee29402cccb4868b1ac579ce42202626`
Subject: `fix(468): remove unreachable load paths and the dead _templateTlp field` — exactly the
string the plan specifies.

### Acceptance verification

`git show --name-only HEAD`:

```
63eebd47 fix(468): remove unreachable load paths and the dead _templateTlp field

QuickFiler/Controllers/QfcCollectionController.cs
docs/features/active/qfc-collection-controller-defects-468/evidence/baseline/p0-t14-tests-coverage.2026-08-26T08-25.md
docs/features/active/qfc-collection-controller-defects-468/evidence/baseline/p0-t16-commit.2026-08-26T08-25.md
docs/features/active/qfc-collection-controller-defects-468/evidence/other/p1-t1-reflective-caller-search.2026-08-26T08-25.md
docs/features/active/qfc-collection-controller-defects-468/evidence/qa-gates/p1-t3-dead-identifier-sweep.2026-08-26T08-45.md
docs/features/active/qfc-collection-controller-defects-468/evidence/qa-gates/p1-t4-live-member-nonregression.2026-08-26T08-45.md
docs/features/active/qfc-collection-controller-defects-468/evidence/qa-gates/p1-t5-format.2026-08-26T08-45.md
docs/features/active/qfc-collection-controller-defects-468/evidence/qa-gates/p1-t6-analyzers.2026-08-26T08-45.md
docs/features/active/qfc-collection-controller-defects-468/evidence/qa-gates/p1-t7-nullable.2026-08-26T08-45.md
docs/features/active/qfc-collection-controller-defects-468/evidence/qa-gates/p1-t8-suite.2026-08-26T08-45.md
docs/features/active/qfc-collection-controller-defects-468/evidence/qa-gates/p1-t8/p1-t8.trx
docs/features/active/qfc-collection-controller-defects-468/plan.2026-08-24T09-39.md
docs/features/active/qfc-collection-controller-defects-468/spec.md
```

**The list contains exactly one `.cs` path — `QuickFiler/Controllers/QfcCollectionController.cs` —
and zero `.csproj` paths.** Every other entry is a Markdown evidence artifact, the plan checklist,
the spec checklist, or the sanitised TRX; none is a production or test source change. The acceptance
condition is met.

`QuickFiler/Controllers/KbdActions.cs` is **absent** from the list, satisfying D2.

### The renumbering is a single reviewable hunk set

The only source change is 241 deletions and 0 insertions in one file, distributed over exactly ten
hunks that correspond one-to-one with the ten removal targets:

| Old-line range | Removed |
|---|---|
| 70 | `_templateTlp` field |
| 402 | commented-out `LoadGroups_02bAsync` reference |
| 587-606 | `LoadGroups_02cAsync` |
| 635-739 | `LoadGroups_02bAsync` + `LoadGroup_03bAsync` |
| 761-797 | `LoadConversationsAndFoldersAsync` + `LoadItemGroup` |
| 827-858 | `LoadSequentialAsync` + `LoadGroupSequential` |
| 865-875 | `CacheTlpForMove` + `SwapTlp` |
| 1254-1274 | `WireUpKeyboardHandler` |
| 1324-1329 | `AnyOpenDropDownsAsync` |
| 1991-1997 | `CaptureTlpTemplate` |

No other production or test file changed, so a reviewer can attribute the whole renumbering to this
one commit, as D15 and AC-17 require.

### Staging discipline

Explicit pathspecs were used. `git status --porcelain` after the commit still shows
`.claude/agent-memory/orchestrator/completion-gate-receipt-shapes.md` as modified and `.claude/state/`
as untracked; neither is owned by this feature and neither was staged or committed.

### Acceptance criteria checked off in this commit

Three criteria in `spec.md` moved from `- [ ]` to `- [x]`:

- **AC-2 (#468)** — verified by P1-T3: all thirteen identifiers return zero hits in `<CTRL>`, each
  contrasted against a non-zero P0-T15 baseline.
- **AC-3 (#468, non-regression)** — verified by P1-T4: all five live literals still present, and the
  changed-line set intersected with each of the five member bodies is empty.
- **AC-16 (#468 residual risk)** — verified by P1-T1: search (a) zero hits over 398 build-input
  files; search (b) 42 `GetMethod(` hits enumerated with a per-hit non-match statement, 0
  `InvokeMember(` hits.

AC-17 (fix order) remains unchecked: this commit satisfies its "dead-code removal isolated in its own
commit" clause, but the full commit sequence is not yet complete.

Result: PASS. Phase 1 is complete.
