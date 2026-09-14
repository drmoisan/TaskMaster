# Pass-After Ladder Measurement, Revision R7

This artifact supersedes `[P4-T2]`'s ladder measurement as the ladder suite's measurement of
record. It measures the `UtilitiesCS.Test.Bootstrap` namespace after `[P4-T13]` added ten
edge-case tests and `[P4-T14]` registered the new file.

Timestamp: 2026-09-14T12-42

Command:
```
pwsh -NoProfile -Command 'Set-Location -LiteralPath <worktree-root>
$vswhere = Join-Path ${env:ProgramFiles(x86)} "Microsoft Visual Studio\Installer\vswhere.exe"
$vstest = @(& $vswhere -latest -products * -find "Common7\IDE\Extensions\TestPlatform\vstest.console.exe")[0]
& $vstest "UtilitiesCS.Test/bin/Debug/UtilitiesCS.Test.dll" /Settings:scripts/vscode/TaskMaster.cli.runsettings /InIsolation /TestCaseFilter:"FullyQualifiedName~UtilitiesCS.Test.Bootstrap" "/Logger:trx;LogFileName=p4-ladder-r7.trx" /ResultsDirectory:TestResults/p4-ladder-r7
$LASTEXITCODE
'
```

EXIT_CODE: 0
ExpectedExitCode: 0

Output Summary:

```
PRERUN_TRX_COUNT=0
TRX_MATCH_COUNT=1
RESULT_SUMMARY_OUTCOME=Completed
COUNTERS_TOTAL=21
COUNTERS_PASSED=21
COUNTERS_FAILED=0
```

`PRERUN_TRX_COUNT=0` was taken before the run, after removing every TRX already in
`TestResults/p4-ladder-r7`. That ordering is what makes `TRX_MATCH_COUNT=1` a measurement of
this run rather than of a residue from an earlier one.

The passed count of 21 clears the floor of 21: eleven existing methods in
`AssemblyBindingFallbackTests` plus the ten `[P4-T13]` adds. The pre-Revision-R7 assembly
carried eleven, so the floor is discriminating.

Per-test outcomes:

```
LadderResolve_WhenProbeDirectoryHoldsTheAssembly_RungFourLoadsItByPath OUTCOME=Passed
Resolve_WhenAlreadyResolvingTheSameSimpleName_ReturnsNull OUTCOME=Passed
LadderResolve_WhenRuntimeFacadeFileIsAbsent_RungThreeDeclinesWithoutLoading OUTCOME=Passed
Resolve_WhenLoadedTokenDiffers_DoesNotReturnTheLoadedAssembly OUTCOME=Passed
LadderResolve_WhenRequestedSimpleNameIsAbsent_RungFourDeclinesOnTheNameGuard OUTCOME=Passed
LadderResolve_WhenRuntimeFacadeLoadThrows_RungThreeAbsorbsAndDeclines OUTCOME=Passed
OnAssemblyResolve_WhenEventArgsCarryNoName_ReturnsNullWithoutResolving OUTCOME=Passed
Resolve_WhenARungThrows_AbsorbsAndContinuesToTheNextRung OUTCOME=Passed
LadderResolve_WhenProbeDirectoryLoadThrows_RungFourAbsorbsAndDeclines OUTCOME=Passed
Install_ThenLoadOfUnresolvableName_LeavesTheLoadFailingWithoutHandlerThrowing OUTCOME=Passed
Resolve_WhenRequestedTokenIsNull_DoesNotMatchStronglyNamedLoadedAssembly OUTCOME=Passed
Install_CalledTwice_AttachesExactlyOneHandler OUTCOME=Passed
Resolve_WhenRequestedIdentityIsNull_ReturnsNullBeforeAnyRung OUTCOME=Passed
Resolve_WhenAlreadyLoadedMatches_ReturnsItWithoutCallingLoadByDisplayName OUTCOME=Passed
LadderResolve_WhenRequestedIdentityIsNull_ReturnsNull OUTCOME=Passed
Resolve_WhenRequestedSimpleNameIsAbsent_ReturnsNullBeforeAnyRung OUTCOME=Passed
Resolve_WhenDisplayNameRungMisses_LoadsFacadeFromRuntimeDirectory OUTCOME=Passed
LadderResolve_WhenLoadedTokenLengthDiffers_RungOneRejectsTheLoadedAssembly OUTCOME=Passed
Resolve_WhenNoRungApplies_ReturnsNullWithoutThrowing OUTCOME=Passed
Resolve_WhenRequestedTokenIsEmpty_DoesNotMatchStronglyNamedLoadedAssembly OUTCOME=Passed
Resolve_WhenNothingLoaded_AsksFullDisplayNameAtFacadeVersion OUTCOME=Passed
```

All ten method names pinned by `[P4-T13]` appear above with `OUTCOME=Passed`.

The results directory `TestResults/p4-ladder-r7` and the TRX name `p4-ladder-r7.trx` are
disjoint from `[P2-T11]`'s `TestResults/p2-expect-fail` and `p2-expect-fail.trx`, so this run
could not overwrite the fail-before evidence.

Build lock: acquired for item 879 before the run and released after it.
