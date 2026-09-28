---
name: nested-import-module-force-unloads-session-wide
description: Import-Module -Force inside a .psm1 removes the target from the WHOLE session before re-importing it privately, so a test that already imported it loses every command
metadata:
  type: project
---

A `.psm1` that does `Import-Module (Join-Path $PSScriptRoot 'Other.psm1') -Force` at load
time **removes `Other` from the entire session** and re-imports it into the importing
module's own session state only. A caller that had already imported `Other` globally is left
with none of its commands.

**Why:** measured on #911 Batch C. `ProjectConsistency.Tests.ps1` did
`Import-Module ProjectConsistency.psm1 -Force` then
`Import-Module ConsistencyVerifier.psm1 -Force`; the verifier's own nested `-Force` import of
`ProjectConsistency` stripped it back out, and all six AC11 and AC14 cases failed with
`The term 'Invoke-VersionReconciliation' is not recognized as a name of a cmdlet`. Removing
`-Force` from the three intra-module imports took the suite from `Failed=7` to `Failed=1`.
The failure looks like a missing export or a typo, not an import-ordering problem, so it
costs real time to diagnose.

**How to apply:** intra-module imports take no `-Force`. Reserve `-Force` for the top-level
import in a test's `BeforeAll`, which is where picking up an edited module actually matters;
each `pwsh -Command` invocation is a fresh process, so staleness across runs is not a risk.
Add a one-line comment at the import site giving the reason, or the next author will
"restore" the `-Force`.

Related: [[project_pester5_helper_function_must_live_in_beforeall]].
