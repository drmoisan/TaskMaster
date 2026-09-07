# [P3-T7] Line-Cap Check for the Phase 3 Files

Timestamp: 2026-08-26T10-50

Task: [P3-T7]
Feature: docs/features/active/quickfiler-bug-family-446

Command: `wc -l QuickFiler/Controllers/QfcFormController.Actions.cs QuickFiler.Test/Controllers/QfcFormControllerSeamTests.cs`
EXIT_CODE: 0

## Recorded counts (post-change, post-`csharpier format`)

| Lines | File | Cap |
|---|---|---|
| 360 | `QuickFiler/Controllers/QfcFormController.Actions.cs` | 500 |
| 496 | `QuickFiler.Test/Controllers/QfcFormControllerSeamTests.cs` | 500 |

Both recorded counts are **at most 500**.

## What the test file needed to get there

`QfcFormControllerSeamTests.cs` was 378 lines at the merge base and reached **576** once
`[P3-T2]` and `[P3-T4]` through `[P3-T6]` had added their four tests. The
`ArrangeUndoConsumer(...)` extraction this task prescribes was applied — it is the shared arrange
for all four tests and also seeds `_undoQueue` — but extraction alone was not sufficient: measured
after it, the file still stood at 532. No new test file was added and
`QuickFiler.Test/QuickFiler.Test.csproj` was not edited, per D4. All 118 `Compile Include` entries
in that project are explicit, with no wildcard, so a new file would have required a csproj edit.

The remaining reduction came from four changes inside the same file, none of which removes or
weakens an assertion:

1. `GetPrivateField<T>` / `SetPrivateField<T>` collapsed to expression bodies over a new
   `private const BindingFlags PrivateInstance` (`using System.Reflection;` added). 21 lines to 8.
2. `ReadControllerSource` collapsed to an expression body, and `ResolveRepositoryPath`'s manual
   `foreach` accumulator replaced with `pathParts.Aggregate(dir.FullName, Path.Combine)`
   (`using System.Linq;` added). 25 lines to 14.
3. `UndoConsumer_OnExit_ResetsUndoConsumerTask` restructured to arrange both exit paths once
   instead of running two arrange/act/assert cycles in sequence.
4. Doc comments on the members this plan authored were tightened, and the blank line preceding each
   `// Act` / `// Assert` marker inside the four new tests was removed. The Arrange–Act–Assert
   markers themselves are all retained, so the UT3 structure requirement still holds.

Changes 1 and 2 are behaviour-preserving simplifications of pre-existing helpers, verified by the
run below; they are recorded here because they touch lines this plan's tasks did not name.

## Verification after the reduction

Command: `dotnet tool run csharpier check "QuickFiler.Test/Controllers/QfcFormControllerSeamTests.cs"`
EXIT_CODE: 0

Command: `& $msbuild TaskMaster.sln /t:Build /m /p:Configuration=Debug "/p:Platform=Any CPU"`
EXIT_CODE: 0

Command: `& $vstest "QuickFiler.Test\bin\Debug\QuickFiler.Test.dll" /InIsolation "/Settings:scripts\vscode\TaskMaster.cli.runsettings" "/TestCaseFilter:FullyQualifiedName~QfcFormControllerSeamTests" "/Logger:trx;LogFileName=p3-t7.trx" "/ResultsDirectory:docs\features\active\quickfiler-bug-family-446\evidence\regression-testing\p3-t7"`
EXIT_CODE: 0

TRX: `docs/features/active/quickfiler-bug-family-446/evidence/regression-testing/p3-t7/p3-t7.trx`

Counters: total 16, executed 16, **passed 16**, failed 0, error 0, timeout 0, aborted 0. That is the
twelve pre-existing seam tests plus the four added by Phase 3, all green after the reduction.

TRX hygiene: scrubbed of the absolute worktree path, account name and machine name, then re-parsed
as XML; `<Counters .../>`, all test names and all outcomes unchanged. A case-insensitive search
for the account name and the machine name across the feature folder returns no match.

## Output Summary

**Both recorded counts are at most 500**: 360 and 496. No new test file was created and the test
project file was not edited.
