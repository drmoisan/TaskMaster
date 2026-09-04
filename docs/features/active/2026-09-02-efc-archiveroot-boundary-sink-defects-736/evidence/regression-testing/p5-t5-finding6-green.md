# P5-T5 — Finding 6: whole `EfcDataModelArchiveRootTests` class recorded GREEN

Timestamp: 2026-09-04T00-16

Command:

```
& $msbuild TaskMaster.sln /t:Build /m /p:Configuration=Debug "/p:Platform=Any CPU"
& $vstest QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation "/TestCaseFilter:FullyQualifiedName~EfcDataModelArchiveRootTests" "/Logger:trx;LogFileName=p5-t5.trx" /ResultsDirectory:docs\features\active\2026-09-02-efc-archiveroot-boundary-sink-defects-736\evidence\regression-testing\p5-t5
```

Build EXIT_CODE: 0 (`Build succeeded.`, `0 Warning(s)`, `0 Error(s)`)

EXIT_CODE: 0

## TRX results

Total **11**, passed **11**, failed **0**. `Test Run Successful.`

The TRX names `MoveToFolderAsync_WhenArchiveRootThrowsComException_StillPropagates` among the passing
results, so the frozen COM-propagation contract still holds with its assertion unchanged. The
rewritten `MoveToFolderAsync_WhenArchiveRootResolves_StillReadsItOnce` is green.

All eleven passing methods:

| Test method | Outcome |
|---|---|
| `ArchiveRootFailureDiagnostic_DoesNotContainTheArchivePathOrMailboxAddress` | Passed |
| `MoveToFolderAsync_WhenArchiveRootIsCrossStoreUnresolvable_ReturnsFalseInsteadOfThrowing` | Passed |
| `MoveToFolderAsync_WhenArchiveRootIsUnresolvable_ReturnsFalseInsteadOfThrowing` | Passed |
| `MoveToFolderAsync_WhenArchiveRootResolves_StillReadsItOnce` | Passed |
| `MoveToFolderAsync_WhenArchiveRootThrowsComException_StillPropagates` | Passed |
| `MoveToFolderAsync_WhenMailInfoIsNull_ReturnsFalseWithoutReadingArchiveRoot` | Passed |
| `MoveToFolderAsync_WhenOneDriveIsMissing_ReturnsFalseWithoutReadingArchiveRoot` | Passed |
| `OpenFsFolderAsync_WhenArchiveRootIsUnresolvable_ReportsAndReturns` | Passed |
| `OpenFsFolderAsync_WhenOneDriveIsMissing_ReturnsWithoutReadingArchiveRoot` | Passed |
| `OpenOlFolderAsync_WhenArchiveRootIsUnresolvable_ReportsAndReturns` | Passed |
| `OpenOlFolderAsync_WhenOneDriveIsMissing_ReturnsWithoutReadingArchiveRoot` | Passed |

Exactly **one** TRX file exists under this task's results directory: `p5-t5.trx`.

## P5-T3's recorded observations

P5-T3 is a source edit that writes no evidence artifact of its own.

**The five-parameter `MoveToFolderAsync` overload's post-change span** is declaration line **303** to
closing-brace line **346**. Within that span there are **zero** occurrences of `new EmailFiler(` —
the two-statement inline pair is replaced by the single line
`var result = await InvokeFilerAsync(config, mailHelpers);`. The `EmailFilerConfig` object
initializer and the `SortEmail.Cleanup_Files();` call remain inside the span.

**The three post-change `OlAncestor = olAncestor,` line numbers** in
`QuickFiler/Controllers/EfcDataModel.cs` are **339**, **380**, and **404**, still one per public
entry point. Line 339 — the first in file order, inside the five-parameter `MoveToFolderAsync`
overload — is the occurrence finding 6's remedy is required to keep covered.

**The two `catch (` observations**, which are the artifact that discharges AC9's conjunct that
`EfcDataModel.TryGetArchiveRoot`'s `catch (InvalidOperationException)` is not widened to
`COMException`:

- Exactly **one** line matches `catch (InvalidOperationException ex)`, at post-change line **287**,
  inside `TryGetArchiveRoot` declared at line 280. That is the same clause and the same enclosing
  member as before the change.
- Exactly **zero** lines match `catch (COMException`. The catch was not widened.

**Seam counts:** exactly one `InvokeFilerAsync` declaration and exactly one invocation of it (token
total 2); exactly three `new EmailFiler(` occurrences in the file, at lines 360 (inside the new
seam), 384 (`OpenOlFolderAsync`), and 408 (`OpenFsFolderAsync`), the latter two untouched. File line
count **499**, under the 500-line ceiling.

Note recorded for the reader: the seam's XML documentation was authored, measured at 502 lines —
three over the ceiling — and then trimmed of its `<param>` and `<returns>` elements and one summary
line to reach 499. The trim removed documentation only; no code changed.

## P5-T4's recorded observations

P5-T4 is a source edit that writes no evidence artifact of its own. Measured in
`QuickFiler.Test/Controllers/EfcDataModelArchiveRootTests.cs`:

| Observation | Value |
|---|---|
| Occurrences of the token `InvokeFilerAsync` | **1** (the override declaration) |
| `[TestMethod]` count | **11** (unchanged) |
| Post-change line count | **399**, under the 500-line ceiling |

The override is `protected internal override Task<bool> InvokeFilerAsync(EmailFilerConfig config, IList<MailItemHelper> mailHelpers) => Task.FromResult(true);`
on the nested `TestableEfcDataModel` class, carrying a comment stating it is the deliberate stop
replacing the incidental downstream dereference. One using directive was added for the parameter
type `EmailFilerConfig`, which lives in `UtilitiesCS.EmailIntelligence.EmailParsingSorting`;
`MailItemHelper` and `IList<>` already resolved from existing directives.

Output Summary: the build exited 0 and the whole-class run exited 0 with TRX total 11, passed 11,
failed 0, naming `MoveToFolderAsync_WhenArchiveRootThrowsComException_StillPropagates` among the
passes. Exactly one TRX file exists under this task's results directory. P5-T3's span for the
five-parameter `MoveToFolderAsync` overload is lines 303-346 with zero `new EmailFiler(` inside it;
the three `OlAncestor` lines are at 339, 380, and 404; the single `catch (InvalidOperationException ex)`
is at line 287 and there are zero `catch (COMException` lines. P5-T4 left one `InvokeFilerAsync`
token in the test file, an unchanged `[TestMethod]` count of 11, and a line count of 399.
