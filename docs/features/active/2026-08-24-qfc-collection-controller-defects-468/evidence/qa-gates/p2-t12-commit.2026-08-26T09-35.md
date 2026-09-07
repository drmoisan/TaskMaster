# [P2-T12] Phase 2 commit — issue #474 defect 1

Timestamp: 2026-08-26T09-35

Command:

```
git add -- QuickFiler/Controllers/QfcCollectionController.cs \
           QuickFiler.Test/Controllers/QfcCollectionControllerDarkModeTests.cs \
           QuickFiler.Test/Controllers/QfcCollectionController.TestSupport.cs \
           QuickFiler.Test/Controllers/QfcCollectionControllerDefects468Tests.cs \
           QuickFiler.Test/QuickFiler.Test.csproj \
           docs/features/active/qfc-collection-controller-defects-468/plan.2026-08-24T09-39.md \
           docs/features/active/qfc-collection-controller-defects-468/spec.md \
           docs/features/active/qfc-collection-controller-defects-468/evidence/
git commit -m "fix(474): retype _parent to IQfcFormController and drop the runtime downcast"
git show --name-only HEAD
```

EXIT_CODE: 0

## Output Summary

Commit `122dcd8db7335eb326643397e6035c4cd27e34f9`
`fix(474): retype _parent to IQfcFormController and drop the runtime downcast`

### Acceptance verification — the code-file set

`git show --name-only HEAD` lists exactly these five `.cs`/`.csproj` paths, and no other:

| Path | Role in P2-T12's required list |
|---|---|
| `QuickFiler/Controllers/QfcCollectionController.cs` | `<CTRL>` |
| `QuickFiler.Test/Controllers/QfcCollectionControllerDarkModeTests.cs` | named explicitly |
| `QuickFiler.Test/Controllers/QfcCollectionController.TestSupport.cs` | new test file 1 of 2 |
| `QuickFiler.Test/Controllers/QfcCollectionControllerDefects468Tests.cs` | new test file 2 of 2 |
| `QuickFiler.Test/QuickFiler.Test.csproj` | named explicitly |

Absence check, as required by the task:

| Path required absent | Present in commit? |
|---|---|
| `QuickFiler/Controllers/EfcFormController.cs` | **no** |
| `QuickFiler/Controllers/QfcFormController.Actions.cs` | **no** |

`QfcFormController.Actions.cs` holds all three production construction sites of
`QfcCollectionController` (`:49`, `:83`, `:139`). They needed no edit: each already passes a
`QfcFormController`, which implements `QuickFiler.Controllers.IQfcFormController`, so widening the
parameter from `IFilerFormController` to `IQfcFormController` binds without a source change at any
call site. The build after P2-T7/P2-T8 reported `0 Error(s)`, which is the proof.

For completeness, `git diff <MERGE_BASE> --name-only -- '*.cs' '*.csproj'` over the whole feature
branch (merge base `61edc19befcf6c4e95b5acd32542f2dcdab41b78`) returns exactly the five paths above
— no `.cs` or `.csproj` outside the owned file set has been touched by Phases 0 through 2.

### Non-code paths in this commit

Per D15 and the precedent set by the P0-T16 and P1-T9 commits, the phase commit also carries the
plan checklist, `spec.md` (for the AC check-off), and this phase's evidence artifacts. The P2-T12
"lists exactly" condition is read as scoping the `.cs`/`.csproj` set, exactly as P1-T9's
"lists `<CTRL>` and no other `.cs` or `.csproj` path" states the same requirement explicitly. The
commit additionally carries `evidence/qa-gates/p1-t9-commit.2026-08-26T08-45.md`, which the crashed
run wrote but never committed.

`.claude/agent-memory/**` and `.claude/state/**` are modified/untracked in this worktree but are not
owned by this feature. Every `git add` used an explicit pathspec, and `git status --porcelain`
confirms both remain unstaged.

### Acceptance criteria checked off in this commit

**AC-14 (#474 defect 1)** — marked `[x]` in `spec.md`. All four of its clauses are positively
verified:

| Clause | Evidence |
|---|---|
| Constructor parameter 5 typed `QuickFiler.Controllers.IQfcFormController` | P2-T10 TRX, passed 1 / failed 0 |
| `_parent` field typed `QuickFiler.Controllers.IQfcFormController` | same test, same run |
| `(QfcFormController)_parent` appears nowhere in the file | P2-T9, 0 hits against a baseline of 1 |
| `EfcFormController.cs` and all three production construction sites unmodified | absent from the branch-wide changed-file list above |

Result: PASS.
