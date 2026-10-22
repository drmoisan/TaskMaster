# [P8-T6] Phase 8 commit — issue #470 defect 1

Timestamp: 2026-08-26T10-58

Command:

```
git add -- QuickFiler/Controllers/QfcCollectionController.cs \
           QuickFiler.Test/Controllers/QfcCollectionControllerDefects468ConversationTests.cs \
           docs/features/active/qfc-collection-controller-defects-468/plan.2026-08-24T09-39.md \
           docs/features/active/qfc-collection-controller-defects-468/spec.md \
           docs/features/active/qfc-collection-controller-defects-468/evidence/
git commit -m "fix(470): handle a missing conversation original explicitly instead of subscripting"
git show --name-only HEAD
```

EXIT_CODE: 0

## Output Summary

Commit `40381135ca2dc5ecc6c19c4c651a3fd7c9db7e9c`
`fix(470): handle a missing conversation original explicitly instead of subscripting`

14 files changed, 14,314 insertions, 160 deletions. The counts are dominated by the four TRX
evidence files, two of which are full-suite runs of 957 tests.

## Acceptance verification — no path outside the owned file set

`git show --name-only HEAD` filtered to `\.(cs|csproj|sln)$` returns exactly two paths:

| Path | Owned because |
|---|---|
| `QuickFiler/Controllers/QfcCollectionController.cs` | `<CTRL>`, the feature's single production file |
| `QuickFiler.Test/Controllers/QfcCollectionControllerDefects468ConversationTests.cs` | D12 test file 4 of 5, registered in the csproj by P7-T2 |

No `.csproj` and no `.sln` changed; the conversation test file was registered in Phase 7. Every
other path is under `docs/features/active/qfc-collection-controller-defects-468/`.

`.claude/agent-memory/**` and `.claude/state/**` remain unstaged; every `git add` used an explicit
pathspec.

Per D15 the commit also carries the plan checklist, `spec.md`, and this phase's evidence artifacts,
including `p7-t14-commit.2026-08-26T10-41.md`, which could only be written after the Phase 7 commit
existed.

## Production change (P8-T3)

Three guards, all using the early-return idiom Phase 7 introduced:

1. `PromoteFirstChild` returns the sentinel `-1` when its `FindIndex` misses, instead of evaluating
   `_itemGroups[indexOriginal].ItemViewer` with `-1`. One warning is logged; the caller's
   `ref int childCount` is not decremented, because no child was promoted.
2. `ToggleGroupConv(string)` returns when the promoted index is `-1`.
3. `ChangeConversationSilently(int, bool)` returns when the index is outside the group-list bounds,
   treating a null list as out of bounds.

Guard 3 is not redundant with guard 2: the overload is `public` and reachable from callers other
than `ToggleGroupConv`, and it covers the upper bound as well as the negative sentinel.

Per D4 none of the three throws. All sit on the VSTO UI event path where the state is recoverable,
and the repository has already ratified log-and-proceed there.

## Test-file rewrite carried in this commit

`QfcCollectionControllerDefects468ConversationTests.cs` was rewritten to a more compact
documentation style before P8-T1 ran, taking it from 461 to 333 lines with P8-T1 included and
ending at 360 with P8-T2. No test name, arrangement, act, or assertion changed; only XML
documentation prose and `because:` wording were shortened.

The driver is the repository's hard 500-line file cap. D12 assigns issue #470 defects 1, 2 and 3 to
this one file, and the P9-T1 test still to come would have carried the original style past the cap.
A sixth test file was rejected because D12 fixes the set at five and P14-T11's acceptance asserts
exactly five consecutive `Compile Include` entries.

The seven Phase 6 and Phase 7 tests in that file were re-verified by the P8-T5 full-suite run, which
passed 957/957.

## Toolchain state at commit

| Step | Command | Result |
|---|---|---|
| Format | `dotnet tool run csharpier check .` | `EXIT_CODE 0`, 1,524 files checked |
| Build | `pwsh -NoProfile -File scripts/vscode/Invoke-VSBuild.ps1 -Target Build` | `EXIT_CODE 0`, 0 errors |
| Test | full `QuickFiler.Test` suite, P8-T5 | `EXIT_CODE 0`, 957 passed, 0 failed |

The P8-T5 run needed one retry; the ten-test load flake is analysed in
`p8-t5-suite.2026-08-26T10-57.md` and the failing TRX is retained beside the passing one.

Line-ending and BOM state verified before staging: `QfcCollectionController.cs` retains its UTF-8
BOM and is 100% CRLF (2,308 of 2,308 lines); the conversation test file carries no BOM and is 100%
CRLF.

## Acceptance criteria checked off in this commit

**AC-8 (#470 defect 1)** — marked `[x]` in `spec.md`.

| Clause | Evidence |
|---|---|
| `PromoteFirstChild` handles a `-1` index explicitly and never subscripts with it | the early return precedes the only subscript in the method |
| `ChangeConversationSilently` handles a `-1` index explicitly and never subscripts with it | the bounds guard precedes the only subscript in the `int` overload |
| a named test calling `PromoteFirstChild` directly | `PromoteFirstChild_WithNoMatchingChild_ReturnsMinusOneWithoutSubscripting` |
| a named test driving `ToggleGroupConv(string)` end to end with no matching `ConvOriginID` | `ToggleGroupConv_WithNoMatchingOriginal_DoesNotSubscriptWithMinusOne` |
| each throws `ArgumentOutOfRangeException` before the fix | `p8-t1-fail-before.2026-08-26T10-45.md` and `p8-t2-fail-before.2026-08-26T10-45.md`, both `ExpectedExitCode: 1`, both failed count 1, both naming `System.ArgumentOutOfRangeException` |
| and does not after | `p8-t4-pass-after.2026-08-26T10-48.md`, 2 passed, 0 failed |

Result: PASS.
