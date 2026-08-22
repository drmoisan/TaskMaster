# AC-16 — 500-Line File-Size Cap Over the Diff (Issue #449, [P7-T13])

Timestamp: 2026-08-22T09-16
WORKTREE: `C:\Users\DanMoisan\repos\TaskMaster\.claude\worktrees\agent-a5600546d71e73061`
Merge-base SHA (from [P0-T7]): `c551eabab0aa0a6b1a284252811a2e1de819634e`
HEAD at measurement: `05156a3adca741bb3cdfa4d92da836f87814e600`

Command:
```
git diff --stat c551eabab0aa0a6b1a284252811a2e1de819634e..HEAD
git diff --name-only c551eabab0aa0a6b1a284252811a2e1de819634e..HEAD | grep -v '\.md$'
grep -c '' <each non-Markdown file>
```
EXIT_CODE: 0

This gate is evaluated only AFTER the [P7-T12] commit. Before it, HEAD equalled the merge base
(recorded in `../baseline/git-state.2026-08-22T09-16.md`) and the diff was empty, so the gate would
have been vacuous and would also have missed the two new test files, which were untracked until the
commit.

## Diff stat

```
46 files changed, 4420 insertions(+), 225 deletions(-)
```

Of the 46 changed files, **5 are non-Markdown** (C# source and one project file) and **41 are
Markdown** (the plan file plus 40 evidence artifacts).

## Every non-Markdown file in the diff, with its post-change line count

Measured with `grep -c ''`, not `wc -l`, because `wc -l` under-reports by one for a file with no
terminating newline — which `QuickFiler.Test/QuickFiler.Test.csproj` is.

| File | Post-change lines | Under 500? |
| --- | --- | --- |
| `QuickFiler/Controllers/QfcExplorerController.cs` | **182** | yes |
| `QuickFiler/Interfaces/IQfcExplorerController.cs` | **14** | yes |
| `QuickFiler.Test/QuickFiler.Test.csproj` | **486** | yes |
| `QuickFiler.Test/Controllers/QfcExplorerControllerTests.cs` | **387** | yes |
| `QuickFiler.Test/Controllers/QfcExplorerController.ConversationViewTests.cs` | **205** | yes |

**Every non-Markdown file in the diff measures under 500 lines.** The largest is the project file at
486, and the largest source file is 387.

`QfcExplorerController.cs` moved 323 -> 182 (it shrank by 141 net: minus 6 for the [P3-T2] member
removal, minus 139 for the [P4-T1] dead region, minus 10 `using` directives, plus the 15-line [P5-T3]
seam block and formatting). It was never close to the cap; see the kickoff-premise correction below.

## Markdown exemption

`.claude/rules/general-code-change.md` exempts Markdown documentation files from the 500-line cap:

> Exceptions: temporary throwaway scripts created and deleted within an agent session; raw text
> fixtures for language-processing test data; **Markdown documentation files**.

The 41 Markdown files in this diff are the plan file and the 40 evidence artifacts under
`<FEATURE>/evidence/`, all of which are documentation and all of which are exempt.

Two further Markdown documents in this feature folder exceed 500 lines and are likewise exempt:
`spec.md` at **1,136** lines and
`research/qfc-explorer-controller-defects.2026-08-21T18-20.md` at **1,039** lines. Both were already
committed at the merge base, so neither appears in THIS diff; `spec.md` enters the diff at [P7-T33]
when the acceptance-criteria check-offs are committed. Either way the Markdown exemption covers them.

## The two pre-existing over-cap files are ABSENT from the diff

Command: `git diff --name-only c551eabab0aa0a6b1a284252811a2e1de819634e..HEAD | grep -E "SortEmail.cs|QuickFileController.cs"`
EXIT_CODE: 1
Output: (no match)

| File | Lines | In the diff? | Edited by this change? |
| --- | --- | --- | --- |
| `UtilitiesCS/EmailIntelligence/EmailParsingSorting/SortEmail.cs` | 1,429 | **NO** | **NO** |
| `QuickFiler/Legacy/QuickFileController.cs` | 1,065 | **NO** | **NO** |

**Pre-emptive attribution statement.** Both files are **pre-existing** violations of the 500-line cap.
Neither is edited by this change and neither appears in the diff stat above. They are recorded here so
that a reviewer measuring the repository after this change does not attribute either violation to
issue #449.

- `SortEmail.cs` (1,429 lines) is the surviving maintained copy of the helpers that were duplicated
  inside the dead region deleted by [P4-T1]. It carries its own tests in `UtilitiesCS.Test`.
  Consolidating the three copies is a separate, larger change and is explicitly not planned here. No
  split refactor was performed on it.
- `QuickFiler/Legacy/QuickFileController.cs` (1,065 lines) is **not compiled** —
  `QuickFiler/QuickFiler.csproj` contains zero `Compile Include` entries for the `Legacy\` directory —
  so it is invisible to every build gate. No split refactor was performed on it.

### Correction of the epic kickoff's premise

The epic kickoff described `QuickFiler/Controllers/QfcExplorerController.cs` as 1,065 lines and
predicted that this change would produce a cap violation requiring a partial-class split. That is a
**misattribution**: the 1,065 figure belongs to `QuickFiler/Legacy/QuickFileController.cs`, the
uncompiled legacy file above. The controller measured **323** lines at the merge base and **182**
after this change, comfortably under the cap throughout. **No partial-class split of the production
file was needed and none was performed.** This agrees with [P0-T15], recorded in
`../baseline/file-line-counts.2026-08-22T09-16.md`.

A split WAS required for the TEST code, for an unrelated reason: `QfcExplorerControllerTests.cs`
reached 569 lines after [P6-T12], so [P6-T14] split the conversation-view tests into a second file.
Both resulting files are under the cap at 387 and 205. See
`../other/test-file-size.2026-08-22T09-16.md`.

## Output Summary

`git diff --stat c551eabab0aa0a6b1a284252811a2e1de819634e..HEAD` reports **46 files changed, 4,420
insertions, 225 deletions**. Of these, 5 are non-Markdown and **every one measures under 500 lines**:
`QfcExplorerController.cs` **182**, `IQfcExplorerController.cs` **14**, `QuickFiler.Test.csproj`
**486**, `QfcExplorerControllerTests.cs` **387**, and `QfcExplorerController.ConversationViewTests.cs`
**205**. The remaining 41 files are Markdown and are exempt from the cap under
`.claude/rules/general-code-change.md`. The two pre-existing over-cap files —
`SortEmail.cs` (1,429) and `QuickFiler/Legacy/QuickFileController.cs` (1,065) — are **absent from the
diff** and are neither edited nor attributable to this change. AC-16 is satisfied.
