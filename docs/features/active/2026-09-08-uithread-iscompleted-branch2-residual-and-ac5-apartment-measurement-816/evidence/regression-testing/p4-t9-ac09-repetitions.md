# P4-T9 — AC9 repetition projection (clause (ii) of issue #809's AC5)

Timestamp: 2026-09-13T23-47

## The two test assemblies, by repository-relative build-output path

```
UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll
QuickFiler.Test\bin\Debug\QuickFiler.Test.dll
```

They are named explicitly rather than discovered by a recursive search, because this worktree is
located beneath a `.claude` path segment and a stale-worktree exclusion expressed as an absolute
match would reject this worktree's own build output.

## Whether a settings file was passed

**No.** None of the three repetitions passed a `/Settings:` argument. The command form is the one
fixed by P0-T15, which carries no settings file. (The separate coverage collection in P4-T11 does
pass `scripts\vscode\TaskMaster.cli.runsettings`; it is not one of the three repetitions and is not
counted as one.)

## The shell-icon exclusion and its reason

Every repetition applies the test-case filter

```
/TestCaseFilter:TestCategory!=LiveOutlook&FullyQualifiedName!~ShellUtilities&FullyQualifiedName!~SysImageListHelper&FullyQualifiedName!~OSBrowser
```

The three name-fragment exclusions are applied because **two consecutive probe runs recorded in a
prior feature folder each produced one non-deterministic failure whose diagnostic was that a Win32
handle passed to `Icon` is not valid or is the wrong type.** The exclusion is **not** applied
because of the earlier stall, which no longer reproduces. That distinction is stated here
deliberately, because an artifact that gave the stall as the reason would be inaccurate.

The positive and negative selectors are never combined with an explicit test list anywhere in this
delivery, because those two vstest arguments are mutually exclusive.

## The three repetitions

### Repetition 1 — P4-T6

Command, verbatim:

```
$vstest = & "${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\vswhere.exe" -latest -products * -find "Common7\IDE\Extensions\TestPlatform\vstest.console.exe" | Select-Object -First 1
& $vstest UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /EnableCodeCoverage /InIsolation "/Logger:trx;LogFileName=p4-t6-run1.trx" /ResultsDirectory:coverage\trx\p4-t6 "/Blame:CollectHangDump;TestTimeout=4min;HangDumpType=None" "/TestCaseFilter:TestCategory!=LiveOutlook&FullyQualifiedName!~ShellUtilities&FullyQualifiedName!~SysImageListHelper&FullyQualifiedName!~OSBrowser"
```

Exit code: **0**.
Outcome of `UtilitiesCS.Test.Extensions.DictionaryExtensions_Tests.TryAddValuesAsync_UpdatesExistingValue`: **Passed**.

### Repetition 2 — P4-T7

Command, verbatim: identical to repetition 1 except
`"/Logger:trx;LogFileName=p4-t7-run2.trx"` and `/ResultsDirectory:coverage\trx\p4-t7`.

Exit code: **0**.
Outcome of `UtilitiesCS.Test.Extensions.DictionaryExtensions_Tests.TryAddValuesAsync_UpdatesExistingValue`: **Passed**.

### Repetition 3 — P4-T8

Command, verbatim: identical to repetition 1 except
`"/Logger:trx;LogFileName=p4-t8-run3.trx"` and `/ResultsDirectory:coverage\trx\p4-t8`.

Exit code: **0**.
Outcome of `UtilitiesCS.Test.Extensions.DictionaryExtensions_Tests.TryAddValuesAsync_UpdatesExistingValue`: **Passed**.

## Summary table

| Repetition | Task | Exit code | Total | Passed | Failed | Skipped | `TryAddValuesAsync_UpdatesExistingValue` |
|---|---|---|---|---|---|---|---|
| 1 | P4-T6 | 0 | 6336 | 6336 | 0 | 0 | **Passed** |
| 2 | P4-T7 | 0 | 6336 | 6336 | 0 | 0 | **Passed** |
| 3 | P4-T8 | 0 | 6336 | 6336 | 0 | 0 | **Passed** |

## PASS determination

Three repetitions are recorded, which is at least three. The per-repetition outcome of the named
non-deterministic test is recorded for every repetition, and **every recorded outcome is Passed**,
so the first branch of the PASS condition is satisfied and no failure attribution is required.

No repetition's artifact records a first attempt in which that test Failed, so no re-run was
performed under the clause in P4-T6 through P4-T8, and no re-run is recorded as an additional
repetition.

None of the three FAIL conditions is met: three repetitions are recorded, the per-repetition
outcome of that test is omitted for none of them, and no failure is recorded at all, let alone one
without attribution.

Clause (ii) of issue #809's AC5 is therefore **discharged**.
