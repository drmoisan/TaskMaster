# P0-T5 — NuGet Restore Baseline

Timestamp: 2026-09-17T02-12

Command: `pwsh -NoProfile -File scripts/vscode/Invoke-Restore.ps1`

The script runs
`msbuild TaskMaster.sln /t:Restore /p:Configuration=Debug "/p:Platform=Any CPU" /p:RestorePackagesConfig=true /m`
and requires PowerShell 7.

EXIT_CODE: 0

CHANNEL: COMMAND

## Output Summary

RESTORE-RESULT: `Build succeeded.` with `0 Warning(s)` and `0 Error(s)`.

PACKAGES-INSTALLED: `Installed: 172 package(s) to packages.config projects`

RESTORE-ELAPSED: `Time Elapsed 00:00:02.55` on the recorded invocation.

PACKAGE-DIR-COUNT: 172

Computed as `@(Get-ChildItem -LiteralPath packages -Directory).Count`, which is at least 1 as the
acceptance condition requires.

FLUENTASSERTIONS-8100: True

MSTEST-TESTADAPTER-440: True

Both directories required by the acceptance condition exist:
`packages/FluentAssertions.8.10.0` and `packages/MSTest.TestAdapter.4.4.0`. FluentAssertions 8.10.0
is the pinned version whose `BeOfType<T>()` exact-type semantics the replacement assertions rely on,
and MSTest 4.4.0 is the pinned adapter whose `DefaultFactoryAsync` behaviour is the mechanism behind
this item's defect.

## Exit-code derivation

The restore was run twice. The first invocation performed the package installation and printed the
`Installed: 172 package(s)` and `Build succeeded.` lines quoted above. Its exit status was not
captured by that invocation's transport, so rather than infer the exit code from the success text,
the script was re-run under explicit capture: the second invocation ran to completion and reported
`RESTORE_EXIT: 0`. The restore is idempotent, so the second run re-verified the same 172 packages
without changing them, and the `EXIT_CODE: 0` recorded above is an observation rather than an
inference from the log text.

## Precondition note

The worktree was a fresh checkout with no `packages/` directory and no `bin\Debug` output before
this task, as P0-T4 recorded when the repo-local SDK had to be downloaded rather than found. Every
later build and test task in this plan depends on this step having populated `packages/`.

## Build lock

This task ran inside a held shared build lock for item 900, acquired before the restore and released
immediately after it completed.
