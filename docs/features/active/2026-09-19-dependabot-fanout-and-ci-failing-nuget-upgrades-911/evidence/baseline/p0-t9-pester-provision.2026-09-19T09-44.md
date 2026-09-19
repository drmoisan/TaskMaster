# P0-T9 — Pester 5.6.1 Provisioning

Timestamp: 2026-09-19T12-34

Command:
```
pwsh -NoProfile -Command 'Install-Module Pester -RequiredVersion 5.6.1 -Force -SkipPublisherCheck -Scope CurrentUser;
  Get-Module Pester -ListAvailable | Select-Object Name,Version'
pwsh -NoProfile -Command 'Import-Module Pester -RequiredVersion 5.6.1; (Get-Module Pester).Version;
  (Get-Module Pester).Path; New-PesterConfiguration'
```

EXIT_CODE: 0

## `Get-Module Pester -ListAvailable | Select-Object Name,Version`

Identical before and after the install:

```
Name   Version
----   -------
Pester 5.6.1
Pester 3.4.0
```

## Install output

```
WARNING: The version '5.6.1' of module 'Pester' is currently in use. Retry the operation after
closing the applications.
```

`$?` was `True`; the command did not throw. The warning means `Install-Module` declined to overwrite
files for a version already loaded by some process on this machine. It is not a provisioning
failure: the required version was already installed before the command ran and is still installed
after it, which is the state the task exists to establish. Recorded rather than absorbed because a
module reported in use indicates another process — plausibly a Pester run in a sibling worktree on
this machine — holds it; see the shared-tooling hazard that affects concurrent runs.

## Functional confirmation

An in-use warning is a claim about file replacement, not about usability, so the module was
exercised directly:

```
IMPORTED_VERSION=5.6.1
IMPORTED_PATH=C:\Users\DanMoisan\OneDrive\Documents\PowerShell\Modules\Pester\5.6.1\Pester.psm1
NEW_PESTER_CONFIGURATION_OK=True
Invoke-Pester resolves to module version 5.6.1
```

`New-PesterConfiguration` returned a configuration object, so the v5 configuration API this plan's
CMD-PESTER-BASELINE and CMD-PESTER-ALL depend on is available.

## Acceptance evaluation

- The recorded list contains the exact version `5.6.1`. PASS.

**Failing-condition reachability.** The failing condition is that only the legacy Pester 3.4.0
module shipped with Windows PowerShell is present. It is reachable — 3.4.0 *is* present on this
machine and is listed above alongside 5.6.1 — and it matters because 3.4.0 has no
`New-PesterConfiguration` and no JaCoCo output format, so every Pester command in this plan would
fail to parse. The check distinguishes the two states rather than merely observing that some Pester
exists: the assertion is on the exact string `5.6.1`, and the import above pinned
`-RequiredVersion 5.6.1` so the 3.4.0 copy could not satisfy it.

Output Summary: Pester 5.6.1 is installed for the current user at
`…\PowerShell\Modules\Pester\5.6.1\Pester.psm1` and coexists with the legacy 3.4.0 module.
`Install-Module` warned that 5.6.1 is in use by another process and made no change; the required
version was already present. A pinned import succeeded and `New-PesterConfiguration` returned a
configuration object, confirming the v5 API needed by CMD-PESTER-BASELINE is usable.
