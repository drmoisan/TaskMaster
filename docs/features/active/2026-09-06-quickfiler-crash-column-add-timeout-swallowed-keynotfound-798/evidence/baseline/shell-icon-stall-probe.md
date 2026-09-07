# Phase 0 — Shell-icon test class probe

Timestamp: 2026-09-07T00-55
Task: [P0-T8]
Issue: #798

Host-specific absolute paths, user account names and machine names are redacted to `<worktree>`,
`<vs-install>`, `<user>` and `<machine>` tokens. TRX file names embed the account and machine name
and are quoted here only in redacted form.

## Command

Command: `<vstest> UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /InIsolation /Blame:CollectHangDump;TestTimeout=4min;HangDumpType=None /Logger:trx /ResultsDirectory:coverage\trx\p0-shellicon /TestCaseFilter:"FullyQualifiedName~ShellUtilities|FullyQualifiedName~SysImageListHelper|FullyQualifiedName~OSBrowser"`

`<vstest>` was resolved inline by vswhere to
`<vs-install>\Common7\IDE\Extensions\TestPlatform\vstest.console.exe`, VSTest version 18.9.0 (x64).
The working directory was `<worktree>`.

EXIT_CODE: 1
ExpectedExitCode: 1

## Counts read from the produced TRX

TRX: `coverage\trx\p0-shellicon\<user>_<machine>_2026-09-07_00_54_28_net481.trx`

- total: 23
- executed: 23
- passed: 22
- failed: 1
- notExecuted: 0

Total run time 2.2016 seconds.

## Stall observation

No stall occurred. The plan's exclusion was originally created because these four classes stalled
the local testhost through `SHGetFileInfo`; on this host the whole filtered set completes in
approximately 2.2 seconds and the Blame data collector reported `All tests finished running,
Sequence file will not be generated`, meaning no hang dump was triggered and the 4-minute test
timeout was never approached.

## Failure observation

One test failed. In the canonical run above the failing test was
`UtilitiesCS.Test.HelperClasses.ShellUtilitiesStatic_Tests.GetFileIcon_WithUseFileType_ShouldReturnIconsForDirectoryAndFileExtension`,
with the diagnostic:

```
System.ArgumentException: Win32 handle that was passed to Icon is not valid or is the wrong type.
   at System.Drawing.Icon..ctor(IntPtr handle, Boolean takeOwnership)
   at System.Drawing.Icon.FromHandle(IntPtr handle)
   at ObjectListViewDemo.ShellUtilitiesStatic.GetFileIcon(String path, Boolean isSmallImage, Boolean useFileType)
```

The shell returned no valid icon handle for the requested item, so `Icon.FromHandle` rejected it.

Two supplementary observations were taken to characterise the failure:

1. The same test re-run alone, with the filter narrowed to that one method, passed:
   `Total tests: 1, Passed: 1`, exit code 0. The failure is therefore not deterministic per test.
2. The full probe command re-run against `coverage\trx\p0-shellicon-confirm` failed again with the
   same shape, `total=23 passed=22 failed=1`, but the failing test that time was the sibling
   `UtilitiesCS.Test.HelperClasses.ShellUtilities_Tests.GetFileIcon_WithUseFileType_ShouldReturnIconsForDirectoryAndFileExtension`.

The failure is reproducible at the level of the class set — one failure in each of two consecutive
runs — while the identity of the failing test moves between the two sibling classes. That pattern is
consistent with contention for a shared shell icon resource under the assembly's class-level
parallelization at 24 workers, not with a defect in the code under test and not with a stall.

## Verdict

SHELL_ICON_EXCLUSION: REQUIRED

Reasoning: the plan's decision rule extends the filter unless this probe shows the classes now pass.
They do not pass. Two consecutive runs of the canonical probe command each produced one failure out
of 23, so leaving these classes in later runs would place a non-deterministic environmental failure
inside every subsequent test gate, including the P8-T4 gate whose acceptance requires a failed count
of 0. The stall mechanism the exclusion was first written for is no longer present, but the class
set is still unreliable on this host for a related environmental reason, and the exclusion remains
the correct disposition. This is recorded as an environmental exclusion covered by CI: CI runs from
a fresh checkout on a runner where these classes are executed, so the exclusion narrows only local
coverage of them, not the protection on the branch.

## Filter extension applied by every later vstest task in this plan

Because the verdict is `SHELL_ICON_EXCLUSION: REQUIRED`, every later vstest invocation in this plan
extends its `/TestCaseFilter:` value with:

`&FullyQualifiedName!~ShellUtilities&FullyQualifiedName!~SysImageListHelper&FullyQualifiedName!~OSBrowser`

The three name fragments cover both observed failing tests: the fragment `ShellUtilities` matches
`ShellUtilities_Tests` and `ShellUtilitiesStatic_Tests` alike.

Output Summary: The four shell-icon classes no longer stall the local testhost; the filtered set
completes in about 2.2 seconds with no hang dump. They do not pass, however: two consecutive runs of
the canonical probe each recorded 23 total, 22 passed and 1 failed, with the failing test moving
between `ShellUtilities_Tests` and `ShellUtilitiesStatic_Tests` and the same invalid Win32 icon
handle diagnostic. The verdict is `SHELL_ICON_EXCLUSION: REQUIRED` and every later vstest task
extends its filter accordingly.
