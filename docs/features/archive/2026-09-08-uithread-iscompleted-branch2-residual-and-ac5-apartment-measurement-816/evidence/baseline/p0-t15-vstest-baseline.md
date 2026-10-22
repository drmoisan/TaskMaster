# P0-T15 — Test baseline over the two named assemblies (baseline)

Timestamp: 2026-09-13T23-09

Command:

```
$vstest = & "${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\vswhere.exe" -latest -products * -find "Common7\IDE\Extensions\TestPlatform\vstest.console.exe" | Select-Object -First 1
& $vstest UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /EnableCodeCoverage /InIsolation "/Logger:trx;LogFileName=p0-t15-baseline.trx" /ResultsDirectory:coverage\trx\p0-t15 "/Blame:CollectHangDump;TestTimeout=4min;HangDumpType=None" "/TestCaseFilter:TestCategory!=LiveOutlook&FullyQualifiedName!~ShellUtilities&FullyQualifiedName!~SysImageListHelper&FullyQualifiedName!~OSBrowser"
```

EXIT_CODE: 0

Output Summary:

The run printed `Test Run Successful.` and completed in 1.0997 minutes.

| Count | Value | Read or derived |
|---|---|---|
| Total | 6332 | Read, from the `Total tests:` line |
| Passed | 6332 | Read, from the `Passed:` line |
| Failed | 0 | Derived: the summary carries no `Failed:` line, which means a Failed count of 0. Corroborated by the TRX `Counters` element, whose `failed` attribute is 0 |
| Skipped | 0 | Derived: Total minus Passed minus Failed. The summary carries no `Skipped:` line. The TRX `notExecuted` attribute is not used for this value, because the TRX logger hard-codes it to 0 |

TRX `Counters` element for cross-reference: `total=6332 executed=6332 passed=6332 failed=0 notExecuted=0`.

### Recorded outcome of the two named tests

| Fully qualified test | Outcome |
|---|---|
| `UtilitiesCS.Test.Extensions.DictionaryExtensions_Tests.TryAddValuesAsync_UpdatesExistingValue` | Passed |
| `UtilitiesCS.Test.NoLiveFormInTestAssemblyTests.ExecutingAssembly_ContainsNoFormDerivedType` | Passed |

The second name is fully qualified because more than one test assembly declares a test of that
short name. The run also executed
`QuickFiler.Test.NoLiveFormInTestAssemblyTests.ExecutingAssembly_ContainsNoFormDerivedType`, which
was likewise recorded Passed; it is a different test and is noted here only to show the qualified
name was resolved unambiguously.

### Notes

The name-fragment exclusion (`ShellUtilities`, `SysImageListHelper`, `OSBrowser`) is applied because
two consecutive probe runs recorded in a prior feature folder each produced one non-deterministic
failure diagnosing a Win32 handle passed to `Icon` that is not valid or is the wrong type. The
earlier stall no longer reproduces and is not the reason.

The blame hang timeout is an independent bound on run duration so that a stall from any source
terminates the run rather than the harness call.

The assemblies are named explicitly by repository-relative path rather than discovered by a
recursive search, because this worktree is located beneath a `.claude` path segment and a
stale-worktree exclusion expressed as an absolute match would reject this worktree's own build
output.

The TRX document and the binary coverage attachment are written under the gitignored coverage
directory. Neither is committed and neither is named as an evidence artifact.
