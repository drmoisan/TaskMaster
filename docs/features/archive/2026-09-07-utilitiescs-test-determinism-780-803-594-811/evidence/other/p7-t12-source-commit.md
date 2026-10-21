# P7-T12 — Source commit

Timestamp: 2026-09-08T10-23
Task: [P7-T12]
Command: `git add -A -- . ":(exclude).claude"`, then `git commit -m "fix(811): make UtilitiesCS.Test deterministic under parallel coverage (AC1-AC3, AC5)"` with a body listing the three defects
EXIT_CODE: 0

SOURCE-HEAD: 03b7bd57cacfc902a8b9f4e917ace627ffcac464

## Commit contents

55 files changed, 3092 insertions, 343 deletions. Three files created:

```
UtilitiesCS.Test/Extensions/DfDeedleEtlTimeoutTests.cs
UtilitiesCS.Test/OutlookObjects/Table/OlTableExtensionsEtlClockTests.cs
UtilitiesCS.Test/TestHelpers/ArmingBarrierTimeProvider.cs
```

The remaining new files are the 32 evidence artifacts written by Phases 0 through 7. The
`:(exclude).claude` pathspec kept the tracked dot-claude agent-memory tree out of the commit.

## Committed source diff — exactly the 20 Write Set paths

`git diff --name-only bb1c7d4b60f7b782227956f36859314d5c47bb03..HEAD -- "*.cs" "*.csproj"`

```
UtilitiesCS.Test/Extensions/DfDeedleEtlTimeoutTests.cs
UtilitiesCS.Test/Extensions/DfDeedleQfcColumnTimeoutTests.cs
UtilitiesCS.Test/Extensions/DfDeedle_COM_Tests.cs
UtilitiesCS.Test/Extensions/DictionaryExtensions_Tests.cs
UtilitiesCS.Test/HelperClasses/NLogTraceWriter_Test.cs
UtilitiesCS.Test/HelperClasses/PrettyPrint_Tests.cs
UtilitiesCS.Test/OutlookObjects/Filter DASL/DASLFilterParserTests.cs
UtilitiesCS.Test/OutlookObjects/Table/OlTableExtensionsEtlClockTests.cs
UtilitiesCS.Test/OutlookObjects/Table/OlTableExtensions_Tests.cs
UtilitiesCS.Test/ReusableTypeClasses/StackGeek_Tests.cs
UtilitiesCS.Test/TestHelpers/ArmingBarrierTimeProvider.cs
UtilitiesCS.Test/UtilitiesCS.Test.csproj
UtilitiesCS/Extensions/DfDeedle.FrameUtilities.cs
UtilitiesCS/Extensions/DfDeedle.cs
UtilitiesCS/Extensions/DictionaryExtensions.cs
UtilitiesCS/HelperClasses/PrettyPrint.cs
UtilitiesCS/OutlookObjects/Filter DASL/DASLFilterParser.cs
UtilitiesCS/OutlookObjects/Table/OlTableExtensions.Etl.cs
UtilitiesCS/OutlookObjects/Table/OlTableExtensions.TableAccess.cs
UtilitiesCS/ReusableTypeClasses/Other/StackGeek.cs
```

20 paths, equal to the Write Set. This is now a two-dot range against the committed history rather
than a worktree comparison, so it cannot pass vacuously.

## Tree state after the commit

| Observation | Value |
|---|---|
| `git status --porcelain -- "*.cs" "*.csproj"` | empty |
| `git status --porcelain --untracked-files=all -- . ":(exclude).claude"` immediately after the commit | empty |
| The same command after this artifact was written | lists exactly one path, this artifact |

This artifact is written after the commit, so it is deliberately the single untracked path until
P8-T12 commits it. Nothing else is written between the commit and that point except the Phase 8
artifacts, which P8-T12 also commits.

## Host-token scan of the commit message

| Token class | Hits |
|---|---|
| Account name (derived at run time, never written here) | 0 |
| Machine name (derived at run time, never written here) | 0 |
| Drive-rooted profile-path shape | 0 |

## Acceptance evaluation

- Commit exit 0. PASS
- `git status --porcelain --untracked-files=all -- . ":(exclude).claude"` lists exactly one path,
  this task's artifact. PASS
- `git status --porcelain -- "*.cs" "*.csproj"` is empty. PASS
- `git diff --name-only $Base..HEAD -- "*.cs" "*.csproj"` lists exactly the 20 Write Set paths.
  PASS
- The commit message contains no absolute path, account name or machine name. PASS

## Output Summary

Source commit `03b7bd57cacfc902a8b9f4e917ace627ffcac464` created on
`bug/utilitiescs-test-determinism-780-803-594-811`. The committed source diff against the anchor
is exactly the 20 declared paths. Working tree is clean apart from this artifact.
