# AC2 fail-before evidence, re-derived against a working log capture

Timestamp: 2026-09-07T02-45
Task: correction A2, applied before Phase 4
Issue: #798
Supersedes: `p2-ac2-timing-fail-before.md` as the AC2 fail-before evidence

Host-specific absolute paths, user account names and machine names are redacted to `<repo-root>`,
`<msbuild>`, `<vstest>`, `<user>` and `<machine>` tokens.

## Why this artifact exists

`p2-ac2-timing-fail-before.md` recorded both AC2 tests as failing, and read as valid fail-before
evidence at the time. Phase 3 then established that the log capture those tests depend on was inert:
neither `UtilitiesCS` nor `UtilitiesCS.Test` carries a log4net `XmlConfigurator` attribute, so the
default repository was unconfigured, `Hierarchy.IsDisabled` reported every level disabled, and the
production `Debug` call was a no-op. Both tests therefore captured zero events **whether or not the
AC2 instrumentation existed**.

The consequence is that the two failures `p2-ac2-timing-fail-before.md` records would have occurred
either way. That gate did not discriminate between an instrumented and an uninstrumented production
file, so it cannot support AC2. The full mechanism and the third assertion strategy Phase 3
implemented to repair it are recorded in
`../baseline/log4net-capture-probe.md` under "Addendum — verdict not reproducible in Phase 3".

This artifact re-derives the fail-before observation against the repaired, working capture, so the
AC2 gate is shown to discriminate by direct experiment rather than by argument.

## Method

The experiment removes the production instrumentation, observes the two AC2 tests, restores the
instrumentation byte-identically, and observes the same two tests again. Only
`UtilitiesCS/Extensions/DfDeedle.QfcColumns.cs` is touched, and only temporarily.

The seven `LogDfTiming(` call sites added by P3-T3 and P3-T4 are the removal target: six for the
column operations in `AddQfcColumns` and one for the property enumeration in
`HasUserDefinedProperty`. The surrounding `Stopwatch` declarations, `Restart()` and `Stop()` calls
were left in place, so the removal deletes the AC2 emission and nothing else.

### Pre-removal state of the instrumented file

- SHA-256: `D9AD49BF7D5A2243594C9EF2FE4837082C4D19D020E0CBD03E5D468C95828649`
- Line count: 259
- `LogDfTiming(` occurrences: 7, at lines 41, 49, 57, 65, 73, 81 and 236

A copy was taken outside the worktree before any edit, so the restore is a copy-back rather than a
re-typing of the removed code.

## Step 1 and 2 — instrumentation REMOVED

After the seven call sites were removed, a counted search reported `LogDfTiming(` occurrences: 0,
and the file measured 231 lines.

Command: `<msbuild> <repo-root>\UtilitiesCS.Test\UtilitiesCS.Test.csproj /t:Build /m /p:Configuration=Debug /p:Platform=AnyCPU /v:minimal /nologo`
EXIT_CODE: 0

Command: `<vstest> <repo-root>\UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /InIsolation /Logger:trx /ResultsDirectory:coverage\trx\a2-removed /Blame:CollectHangDump;TestTimeout=4min;HangDumpType=None /TestCaseFilter:"TestCategory!=LiveOutlook&(FullyQualifiedName~AddQfcColumns_EmitsDfTimingLineForEachColumnOperation|FullyQualifiedName~HasUserDefinedProperty_EmitsDfTimingLineForPropertyEnumeration)&FullyQualifiedName!~ShellUtilities&FullyQualifiedName!~SysImageListHelper&FullyQualifiedName!~OSBrowser"`
EXIT_CODE: 1
ExpectedExitCode: 1

The filter carries the shell-icon exclusion extension because P0-T8 recorded
`SHELL_ICON_EXCLUSION: REQUIRED`.

TRX: `coverage\trx\a2-removed\<user>_<machine>_2026-09-07_02_47_27_net481.trx`

- total: 2
- passed: 0
- failed: 2
- total time: 2.1429 s

| Test | Observed status | Duration |
|---|---|---|
| `AddQfcColumns_EmitsDfTimingLineForEachColumnOperation` | **Failed** | 336 ms |
| `HasUserDefinedProperty_EmitsDfTimingLineForPropertyEnumeration` | **Failed** | 2 ms |

Failure messages, quoted verbatim:

```
Expected HasTimingLineNaming(messages, columnName) to be True because no [Df timing] line names the column operation 'SentOn', but found False.
```

```
Expected HasTimingLineNaming(messages, "HasUserDefinedProperty") to be True because the property enumeration must emit its own [Df timing] line, but found False.
```

Both tests reached their assertion, so the reflective invocation succeeded and the uninstrumented
production methods ran to completion. The assertion failed because no `[Df timing]` line was emitted.

## Step 3 — instrumentation RESTORED, byte-identity verified

The file was restored by copying back the pre-edit copy taken in the Method section.

- SHA-256 after restore: `D9AD49BF7D5A2243594C9EF2FE4837082C4D19D020E0CBD03E5D468C95828649`
- Matches the pre-removal hash: **true**
- Line count: **259**
- `LogDfTiming(` occurrences: **7**, at lines **41, 49, 57, 65, 73, 81 and 236**

All four observations match the pre-removal state exactly, so the restore is byte-identical and the
seven call sites sit at the expected lines.

### A vacuous run was detected and discarded before it was recorded as a result

The first restored-state test run reported both tests as **failed**. That result was not the state of
the restored source; it was an artifact of the build. `Copy-Item` preserves the source file's
`LastWriteTime`, so the restored `DfDeedle.QfcColumns.cs` carried its pre-removal timestamp of
`2026-09-07T02:25:01`, which is **older** than the `UtilitiesCS.dll` built at `2026-09-07T02:47:07`
from the uninstrumented source. MSBuild's up-to-date check therefore skipped `CoreCompile`, the build
reported exit 0 without recompiling, and the test run exercised the uninstrumented binary a second
time.

The two timestamps were compared directly and the comparison `CS_NEWER_THAN_DLL` returned `False`,
which is the observation that identified the cause. The file's `LastWriteTime` was then set to the
current time and the project rebuilt. The content hash after that touch was re-measured and still
equals the pre-removal hash, so the touch changed the timestamp only and byte-identity is preserved.

This is recorded rather than silently corrected because the discarded run would have read as a
genuine pass-after failure, and because the same mechanism can make any restore-and-rerun step
vacuous.

Command: `<msbuild> <repo-root>\UtilitiesCS.Test\UtilitiesCS.Test.csproj /t:Build /m /p:Configuration=Debug /p:Platform=AnyCPU /v:minimal /nologo`
EXIT_CODE: 0

## Step 4 — instrumentation RESTORED, tests re-run

Command: `<vstest> <repo-root>\UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /InIsolation /Logger:trx /ResultsDirectory:coverage\trx\a2-restored2 /Blame:CollectHangDump;TestTimeout=4min;HangDumpType=None /TestCaseFilter:"TestCategory!=LiveOutlook&(FullyQualifiedName~AddQfcColumns_EmitsDfTimingLineForEachColumnOperation|FullyQualifiedName~HasUserDefinedProperty_EmitsDfTimingLineForPropertyEnumeration)&FullyQualifiedName!~ShellUtilities&FullyQualifiedName!~SysImageListHelper&FullyQualifiedName!~OSBrowser"`
EXIT_CODE: 0

TRX: `coverage\trx\a2-restored2\<user>_<machine>_2026-09-07_02_49_13_net481.trx`

- total: 2
- passed: 2
- failed: 0

| Test | Observed status | Duration |
|---|---|---|
| `AddQfcColumns_EmitsDfTimingLineForEachColumnOperation` | **Passed** | 323 ms |
| `HasUserDefinedProperty_EmitsDfTimingLineForPropertyEnumeration` | **Passed** | < 1 ms |

## Conclusion

The two AC2 tests fail when the seven `LogDfTiming(` call sites are absent and pass when they are
present, against an otherwise identical tree and against a build proven to have recompiled. The gate
discriminates on the presence of the AC2 instrumentation, which is the property
`p2-ac2-timing-fail-before.md` was intended to establish and did not.

This artifact supersedes `p2-ac2-timing-fail-before.md` as the AC2 fail-before evidence. That older
artifact is retained as the contemporaneous Phase 2 record and carries a one-line cross-reference to
this file.

Both AC2 tests are named here by fully-qualified name:

- `UtilitiesCS.Test.Extensions.DfDeedleQfcColumnTimeoutTests.AddQfcColumns_EmitsDfTimingLineForEachColumnOperation`
- `UtilitiesCS.Test.Extensions.DfDeedleQfcColumnTimeoutTests.HasUserDefinedProperty_EmitsDfTimingLineForPropertyEnumeration`

Output Summary: With the seven `LogDfTiming(` call sites removed, both AC2 tests failed, 2 total,
0 passed, 2 failed, EXIT_CODE 1 matching ExpectedExitCode 1. The file was restored byte-identically,
verified by SHA-256 equality, 259 lines and 7 `LogDfTiming(` occurrences at lines 41, 49, 57, 65, 73,
81 and 236. A first restored-state run was discarded as vacuous because `Copy-Item` preserved an old
`LastWriteTime` and MSBuild skipped `CoreCompile`; after touching the timestamp and rebuilding, both
AC2 tests passed, 2 total, 2 passed, 0 failed, EXIT_CODE 0. The AC2 gate therefore discriminates on
the presence of the instrumentation, and this artifact supersedes `p2-ac2-timing-fail-before.md`.
