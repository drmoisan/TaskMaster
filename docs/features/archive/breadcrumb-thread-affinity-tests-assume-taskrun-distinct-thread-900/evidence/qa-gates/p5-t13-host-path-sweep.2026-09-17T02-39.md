# P5-T13 — Host Identifier Sweep of the Feature Folder

Timestamp: 2026-09-17T02-39

Command: over every file under
`docs/features/active/breadcrumb-thread-affinity-tests-assume-taskrun-distinct-thread-900`,
including the plan and `spec.md`:

    $t = [regex]::Escape((Split-Path -Leaf $env:USERPROFILE))
    @(Get-ChildItem -Recurse -File -LiteralPath <feature-folder> | Select-String -Pattern "(?i)$t").Count
    @(Get-ChildItem -Recurse -File -LiteralPath <feature-folder> | Select-String -Pattern "[A-Za-z]:[\\/]Users[\\/]").Count

EXIT_CODE: 0

CHANNEL: COMMAND

## Output Summary

ACCOUNT-TOKEN-MATCHES: 0

USERS-PATH-MATCHES: 0

FILES-SCANNED: 42

MACHINE-NAME-MATCHES: 0 (an additional check beyond the two the task requires, over
`$env:COMPUTERNAME`)

The account token is derived at run time from the leaf of `$env:USERPROFILE` and is never written
into this artifact; the same applies to the machine name. Both searches are case-insensitive.

## Acceptance

Both required counts are 0, over 42 files. No repair was needed and no re-run was required.

The scan is not vacuous: it enumerates and reads every file in the tree, and `FILES-SCANNED: 42`
records the population it covered. A zero result from an empty enumeration would be indistinguishable
from a clean tree without that count.

Two places where a host path could plausibly have leaked were handled at write time rather than
here, which is why the sweep is clean:

- The pre-implementation checkpoint P0-T3 read carries two absolute host paths in a delegation
  receipt. P0-T3 recorded only the repository-relative `feature-folder` value and the final path
  segment of the worktree root, and transcribed neither absolute value.
- The P5-T5 iteration 1 failure message from `vstest.console.exe` quoted an absolute path to
  `TaskMaster.sln`. It was recorded with `<repo-root>` substituted for the directory portion.

The MSBuild file logs, the TRX documents, the raw Cobertura documents and the detached-run console
logs all carry absolute paths, and none of them is copied into the feature folder: they remain under
the git-ignored `coverage/` and `TestResults/` trees. Only derived figures are transcribed into
evidence, which is also what the repository's committed-test-evidence rule requires.
