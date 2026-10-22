# P1-T7 — Finding 1 regression tests: recorded RED against the defect-preserving seam

Timestamp: 2026-09-03T23-47

Command:

```
& $msbuild TaskMaster.sln /t:Build /m /p:Configuration=Debug "/p:Platform=Any CPU"
& $vstest TaskMaster.Test\bin\Debug\TaskMaster.Test.dll /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation "/TestCaseFilter:FullyQualifiedName~AppOlObjectsArchiveRootComGuardTests&TestCategory!=LiveOutlook" "/Logger:trx;LogFileName=p1-t7.trx" /ResultsDirectory:docs\features\active\2026-09-02-efc-archiveroot-boundary-sink-defects-736\evidence\regression-testing\p1-t7
```

Build EXIT_CODE: 0 (`Build succeeded.`, `0 Warning(s)`, `0 Error(s)`)

EXIT_CODE: 1
ExpectedExitCode: 1

## TRX results

Total **6**, passed **2**, failed **4**.

| Test method | Outcome |
|---|---|
| `ResolveValidatedArchiveRootPath_WhenBothReadsResolve_ReturnsPathAndEmitsNoDiagnostic` | Passed |
| `ResolveValidatedArchiveRootPath_WhenResolvedFolderIsNull_ThrowsUnresolvableWithNoInnerException` | Passed |
| `ResolveValidatedArchiveRootPath_WhenComposedReadThrowsComException_NormalizesToInvalidOperation` | Failed |
| `ResolveValidatedArchiveRootPath_WhenResolvedReadThrowsComException_NormalizesToInvalidOperation` | Failed |
| `ResolveValidatedArchiveRootPath_WhenComReadFails_MessageWithholdsPathAndMailboxAddress` | Failed |
| `ResolveValidatedArchiveRootPath_WhenComReadFailsTwice_ReReadsTheComposedPathOnTheSecondCall` | Failed |

The two passes are the success case and the guard-passthrough case, which exercise behaviour the
defect-preserving seam already delivers. The four failures are the two normalization cases, the
redaction case, and the retry case — exactly the split this task's acceptance requires.

## Each failure names `COMException` as the exception that escaped

Every one of the four failure messages opens with the same FluentAssertions sentence, quoted here
from the TRX with the stack frames elided:

- `ResolveValidatedArchiveRootPath_WhenComposedReadThrowsComException_NormalizesToInvalidOperation`:
  `Expected a <System.InvalidOperationException> to be thrown, but found <System.Runtime.InteropServices.COMException>: System.Runtime.InteropServices.COMException (0x80004005): Outlook is busy.`
- `ResolveValidatedArchiveRootPath_WhenResolvedReadThrowsComException_NormalizesToInvalidOperation`:
  `Expected a <System.InvalidOperationException> to be thrown, but found <System.Runtime.InteropServices.COMException>: System.Runtime.InteropServices.COMException (0x80004005): The folder collection is unavailable.`
- `ResolveValidatedArchiveRootPath_WhenComReadFails_MessageWithholdsPathAndMailboxAddress`:
  `Expected a <System.InvalidOperationException> to be thrown, but found <System.Runtime.InteropServices.COMException>: System.Runtime.InteropServices.COMException (0x80004005): Outlook is busy.`
- `ResolveValidatedArchiveRootPath_WhenComReadFailsTwice_ReReadsTheComposedPathOnTheSecondCall`:
  `Expected a <System.InvalidOperationException> to be thrown, but found <System.Runtime.InteropServices.COMException>: System.Runtime.InteropServices.COMException (0x80004005): Outlook is busy.`

This is the finding-1 defect exactly as spec.md states it: a member whose documented contract admits
only `InvalidOperationException` emits an undocumented `COMException`.

## TRX inventory

Exactly **one** TRX file exists under this task's results directory:
`p1-t7.trx`.

`vstest.console.exe` additionally created an MSTest deployment directory beside it, named
`Deploy_<account> <timestamp>_<pid>`. It was empty of files (0 files, three empty subdirectories) and
was removed immediately after this run, because its **directory name** carries both the account token
and the machine name that D10 forbids in any committed artifact, and P6-T12's sweep rewrites file
content only. The removal is a mechanical hygiene step: it deletes no evidence, and the TRX count
clause above is unaffected because a deployment directory is not a TRX file.

Output Summary: the build exited 0; the filtered vstest run exited 1 as expected. TRX total 6,
passed 2, failed 4. The two passes are the success and guard-passthrough cases; the four failures are
the two normalization cases, the redaction case, and the retry case, and each failure message names
`System.Runtime.InteropServices.COMException` as the exception that escaped where
`System.InvalidOperationException` was expected. Exactly one TRX file exists under this task's
results directory.
