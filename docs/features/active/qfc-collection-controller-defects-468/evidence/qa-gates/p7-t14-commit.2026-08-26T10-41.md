# [P7-T14] Phase 7 commit — issue #470 defect 2

Timestamp: 2026-08-26T10-41

Command:

```
git add -- QuickFiler/Controllers/QfcCollectionController.cs \
           QuickFiler.Test/Controllers/QfcCollectionControllerDefects468ConversationTests.cs \
           QuickFiler.Test/QuickFiler.Test.csproj \
           docs/features/active/qfc-collection-controller-defects-468/plan.2026-08-24T09-39.md \
           docs/features/active/qfc-collection-controller-defects-468/spec.md \
           docs/features/active/qfc-collection-controller-defects-468/evidence/
git commit -m "fix(470): derive the conversation insertion count from a single source of truth"
git show --name-only HEAD
```

EXIT_CODE: 0

## Output Summary

Commit `623224335407f5ac7836ace224adb619f1933e9d`
`fix(470): derive the conversation insertion count from a single source of truth`

12 files changed, 7,780 insertions, 25 deletions. The insertion count is dominated by the two TRX
evidence files.

## Acceptance verification — no path outside the owned file set

`git show --name-only HEAD` filtered to `\.(cs|csproj|sln)$` returns exactly three paths:

| Path | Owned because |
|---|---|
| `QuickFiler/Controllers/QfcCollectionController.cs` | `<CTRL>`, the feature's single production file |
| `QuickFiler.Test/Controllers/QfcCollectionControllerDefects468ConversationTests.cs` | D12 test file 4 of 5, created by P7-T1 |
| `QuickFiler.Test/QuickFiler.Test.csproj` | registers that new file, per D13 |

No `.sln` changed. Every other path in the commit is under
`docs/features/active/qfc-collection-controller-defects-468/`.

The csproj change is a single line: `git diff --stat` on it reported
`1 file changed, 1 insertion(+)` before staging. The new `<Compile Include>` sits immediately after
the `QfcCollectionControllerDefects468MoveTests.cs` entry and immediately before the
`QfcDatamodelTests.cs` entry, keeping the five feature entries contiguous per D13 and minimising the
merge-conflict surface with sibling epic children that share that block.

`.claude/agent-memory/**` and `.claude/state/**` remain unstaged; every `git add` used an explicit
pathspec.

Per D15 the commit also carries the plan checklist, `spec.md`, and this phase's evidence artifacts,
including `p6-t7-commit.2026-08-26T10-29.md`, which could only be written after the Phase 6 commit
existed.

## Production change

Two new pure static members and two rewired members in `QfcCollectionController`:

- `ResolveConversationInsertions(ConversationResolver, string)` — the member-resolution expression
  extracted verbatim from `EnumerateConversationMembers`.
- `ReconcileInsertionCount(string, int, int, int, int, int, System.Action<string>)` — returns the
  resolved count unconditionally, warns exactly once on disagreement, never throws (D5).
- `ToggleUnGroupConv` — early return on `baseEmailIndex == -1` restoring navigation and layout
  state; one resolution before `MakeSpaceForItems`; `insertCount` from `ReconcileInsertionCount`;
  the resolved list handed to `EnumerateConversationMembers`. The loop is not clamped.
- `EnumerateConversationMembers` — `conversationCount` replaced by
  `IReadOnlyList<MailItem> insertions`; the resolver query removed.

## Toolchain state at commit

| Step | Command | Result |
|---|---|---|
| Format | `dotnet tool run csharpier check .` | `EXIT_CODE 0`, 1,524 files checked |
| Build | `pwsh -NoProfile -File scripts/vscode/Invoke-VSBuild.ps1 -Target Build` | `EXIT_CODE 0`, 0 errors |
| Test | full `QuickFiler.Test` suite, P7-T13 | `EXIT_CODE 0`, 955 passed, 0 failed |

Line-ending and BOM state verified before staging: `QfcCollectionController.cs` retains its UTF-8
BOM and is 100% CRLF; the new test file carries no BOM and is 100% CRLF, matching its sibling test
files; `QuickFiler.Test.csproj` is 100% CRLF and unchanged in that respect.

## Acceptance criteria checked off in this commit

**AC-9 (#470 defect 2)** — marked `[x]` in `spec.md`.

| Clause | Evidence |
|---|---|
| resolves the insertion list exactly once before `MakeSpaceForItems` | source: the call at `:1635` precedes `MakeSpaceForItems` at `:1648`, and it is the only call to the helper in the method |
| derives `insertCount` from `insertions.Count` as the single source of truth | `ReconcileInsertionCount` returns its `insertionsCount` argument on every path; asserted by all three reconciliation tests |
| emits one `Warn` carrying all six values when the reservation disagrees | `ReconcileInsertionCount_AboveReservation_...WarnsOnce` asserts one invocation and asserts the message contains each of the six `name=value` tokens |
| does not warn when they agree | `ReconcileInsertionCount_EqualToReservation_...DoesNotWarn` asserts the sink is empty |
| the below-reservation direction is covered too | `ReconcileInsertionCount_BelowReservation_...WarnsOnce` |
| `baseEmailIndex == -1` is guarded before `_itemGroups[insertionIndex - 1]` | source: the early return sits above every statement that derives an index from `baseEmailIndex` |
| a direct test of the extracted pure helper | `ResolveConversationInsertions_ExcludesBaseEntryAndOrdersBySentOnDescending` |
| each reconciliation test arranged so no loop iteration executes | all three call the pure helper directly; no controller instance is involved |
| the loop is not clamped | source: `Enumerable.Range(0, insertions.Count)` is unchanged; the count is corrected at the reservation, not truncated at the loop |

The pre-fix red state for this phase is `ConversationReconciliationHelpersExist`
(`p7-t3-fail-before.2026-08-26T10-33.md`, `ExpectedExitCode: 1`, failed count 1). The two
behavioural pre-fix states that cannot be captured as a failing run — the above-reservation
`ArgumentOutOfRangeException` and the `baseEmailIndex == -1` subscript — are recorded with their
`WhyFailingRunImpossible:` reasons and alternative proofs in
`p7-t12-pass-after.2026-08-26T10-39.md`, per D7, as source material for P14-T1's dossier.

Result: PASS.
