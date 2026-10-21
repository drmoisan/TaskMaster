# [P9-T5] Phase 9 commit — issue #470 defect 3

Timestamp: 2026-08-26T11-03

Command:

```
git add -- QuickFiler/Controllers/QfcCollectionController.cs \
           QuickFiler.Test/Controllers/QfcCollectionControllerDefects468ConversationTests.cs \
           docs/features/active/qfc-collection-controller-defects-468/plan.2026-08-24T09-39.md \
           docs/features/active/qfc-collection-controller-defects-468/spec.md \
           docs/features/active/qfc-collection-controller-defects-468/evidence/
git commit -m "fix(470): skip groups with no controller or viewer in SetVisualDigits"
git show --name-only HEAD
```

EXIT_CODE: 0

## Output Summary

Commit `ffc10ff9549991902dc097f3ae0e9d978e7b4984`
`fix(470): skip groups with no controller or viewer in SetVisualDigits`

11 files changed, 7,323 insertions, 8 deletions. The insertion count is dominated by the two TRX
evidence files, one of which is a 958-test full-suite run.

## Acceptance verification — no path outside the owned file set

`git show --name-only HEAD` filtered to `\.(cs|csproj|sln)$` returns exactly two paths:

| Path | Owned because |
|---|---|
| `QuickFiler/Controllers/QfcCollectionController.cs` | `<CTRL>`, the feature's single production file |
| `QuickFiler.Test/Controllers/QfcCollectionControllerDefects468ConversationTests.cs` | D12 test file 4 of 5, registered in the csproj by P7-T2 |

No `.csproj` and no `.sln` changed. Every other path is under
`docs/features/active/qfc-collection-controller-defects-468/`.

`.claude/agent-memory/**` and `.claude/state/**` remain unstaged; every `git add` used an explicit
pathspec.

Per D15 the commit also carries the plan checklist, `spec.md`, and this phase's evidence artifacts,
including `p8-t6-commit.2026-08-26T10-58.md`, which could only be written after the Phase 8 commit
existed.

## Production change (P9-T2)

The `SetVisualDigits` group loop opens with a skip guard covering both members:

```
if (grp?.ItemController is null || grp.ItemViewer is null)
{
    return;
}
```

`return` inside a `ForEach` lambda skips one element and continues with the rest. Guarding only the
controller would have been insufficient: execution would then reach
`grp.ItemViewer.LblItemNumber` on the next line under the same arrangement and throw again.

The dead null-conditional in `grp.ItemController?.ItemNumber.ToString(format) ?? 0.ToString(format)`
collapsed to a plain call. It was unreachable protection — the first statement of the loop body
dereferenced the same reference unguarded — and `int.ToString(string)` never returns null, so the
`??` arm could not be taken in any case.

## Toolchain state at commit

| Step | Command | Result |
|---|---|---|
| Format | `dotnet tool run csharpier check .` | `EXIT_CODE 0`, 1,524 files checked |
| Build | `pwsh -NoProfile -File scripts/vscode/Invoke-VSBuild.ps1 -Target Build` | `EXIT_CODE 0`, 0 errors |
| Test | full `QuickFiler.Test` suite, P9-T4 | `EXIT_CODE 0`, 958 passed, 0 failed, first attempt |

Line-ending and BOM state verified before staging: `QfcCollectionController.cs` retains its UTF-8
BOM and is 100% CRLF (2,320 of 2,320 lines); the conversation test file carries no BOM, is 100%
CRLF, and stands at 432 lines, inside the 500-line cap.

## Acceptance criteria checked off in this commit

**AC-10 (#470 defect 3)** — marked `[x]` in `spec.md`.

| Clause | Evidence |
|---|---|
| `SetVisualDigits` skips a group entirely when its `ItemController` or `ItemViewer` is null | the guard precedes every dereference in the loop body; `VerifySet(..., Times.Never())` proves the skip happens before the controller write |
| and does not throw | `p9-t3-pass-after.2026-08-26T11-02.md`, 1 passed, 0 failed |
| a named MSTest test that throws `NullReferenceException` (wrapped in `TargetInvocationException`) before the fix | `p9-t1-fail-before.2026-08-26T11-00.md`, `ExpectedExitCode: 1`, failed count 1; the test catches the wrapper and asserts on `InnerException`, and the recorded message names `System.NullReferenceException` at `QfcCollectionController.cs:145` |
| asserts no throw and no viewer text written after | every group in the arrangement has a null `ItemViewer`, so writing viewer text would have thrown; the method completed and left `_digitRefreshNeeded == false`, which the test asserts |

## Delegated window closes here

Phases 6 through 9 are complete. Phase 10 has not been started, per the scope of this delegation.

| Phase | Defect | Commit |
|---|---|---|
| 6 | #469 defects 1 and 2 | `137ee3076ecae066c8a53306149b100dee29fb7e` |
| 7 | #470 defect 2 | `623224335407f5ac7836ace224adb619f1933e9d` |
| 8 | #470 defect 1 | `40381135ca2dc5ecc6c19c4c651a3fd7c9db7e9c` |
| 9 | #470 defect 3 | `ffc10ff9549991902dc097f3ae0e9d978e7b4984` |

The Phase 6 commit additionally carries the scoped host-identifier sanitisation remediation recorded
in `evidence/other/host-identifier-sanitisation.2026-08-26T10-11.md`.

Result: PASS.
