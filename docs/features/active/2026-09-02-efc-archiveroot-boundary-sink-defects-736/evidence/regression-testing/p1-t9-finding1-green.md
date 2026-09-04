# P1-T9 — Finding 1 regression tests: recorded GREEN after the minimal fix

Timestamp: 2026-09-03T23-48

Command:

```
& $msbuild TaskMaster.sln /t:Build /m /p:Configuration=Debug "/p:Platform=Any CPU"
& $vstest TaskMaster.Test\bin\Debug\TaskMaster.Test.dll /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation "/TestCaseFilter:FullyQualifiedName~AppOlObjectsArchiveRootComGuardTests&TestCategory!=LiveOutlook" "/Logger:trx;LogFileName=p1-t9.trx" /ResultsDirectory:docs\features\active\2026-09-02-efc-archiveroot-boundary-sink-defects-736\evidence\regression-testing\p1-t9
```

Build EXIT_CODE: 0 (`Build succeeded.`, `0 Warning(s)`, `0 Error(s)`)

EXIT_CODE: 0

## TRX results

Total **6**, passed **6**, failed **0**. `Test Run Successful.`

All four tests that P1-T7 recorded red are now green: the two normalization cases, the redaction
case, and the retry case. The two that were already green — the success case and the guard-passthrough
case — remain green, so the fix changed no behaviour outside the COM-failure path.

## TRX inventory

Exactly **one** TRX file exists under this task's results directory: `p1-t9.trx`. No MSTest
deployment directory was created, because the run passed.

## P1-T8's recorded observations

P1-T8 is a source edit that writes no evidence artifact of its own. Its four observations are
recorded here:

1. **Which of the two constructions the implementation used.** The implementation uses the
   **two-argument `InvalidOperationException` construction**, not an `InnerException` assignment.
   The count of `new InvalidOperationException(` in
   `TaskMaster/AppGlobals/AppOlObjects.ArchiveRoot.cs` is **1**, and the count of the token
   `InnerException` in that file is **0**. The throw reads
   `throw new InvalidOperationException(ArchiveRootPathGuard.UnresolvableRule, comFailure);`, so the
   caught `COMException` becomes the inner exception of the normalized one.
2. **`catch (COMException` line count in that file: 1.**
3. **`catch (` line count in that file: 1.** The single guarded block wraps both read-delegate
   invocations, which is why a COM failure on the composed read short-circuits the resolved read —
   the behaviour the retry test asserts.
4. **Post-change line count of `TaskMaster/AppGlobals/AppOlObjects.ArchiveRoot.cs`: 95**, under the
   500-line ceiling.

The diagnostic is emitted through `logDiagnostic` before the throw, carrying
`ArchiveRootPathGuard.UnresolvableRule`, so the frozen guard's rule text and its
diagnostic-before-throw ordering are both reused rather than duplicated. ArchiveRootPathGuard.cs is
not modified.

Output Summary: the build exited 0 and the filtered vstest run exited 0 with TRX total 6, passed 6,
failed 0. Exactly one TRX file exists under this task's results directory. P1-T8 used the
two-argument `InvalidOperationException` construction (1 occurrence; 0 occurrences of the
`InnerException` token), with a `catch (COMException` line count of 1, a `catch (` line count of 1,
and a post-change file line count of 95.
