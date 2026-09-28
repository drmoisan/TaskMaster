---
name: whatif-does-not-reach-module-session-state
description: A script's -WhatIf does not propagate into a module function's own SupportsShouldProcess; the module writes during a what-if run unless you pass -WhatIf:$WhatIfPreference explicitly
metadata:
  type: project
---

A `.ps1` with `[CmdletBinding(SupportsShouldProcess = $true)]` run with `-WhatIf` sets
`$WhatIfPreference` in the **script** scope only. A function exported from a `.psm1` runs in the
module's own session state and reads `$WhatIfPreference` from there, so its `ShouldProcess` returns
`$true` and it writes files while the calling script's own `ShouldProcess` sites correctly decline.

**Why:** measured on issue #911. `Repair-PackageManifestConsistency.ps1 -WhatIf` printed three
`What if:` lines for its own writes, then `Invoke-ManifestNormalization` (in `PackageGraph.psm1`)
silently rewrote a fixture manifest. The tell was a what-if run whose store came back modified
while the transcript showed only the script's own what-if messages, never the module's.

**How to apply:** when a script delegates a state change to a module function that declares
`SupportsShouldProcess`, pass the preference across the boundary explicitly:
`Invoke-Thing -Foo $bar -WhatIf:$WhatIfPreference`. Write one test that runs the whole entry point
under `-WhatIf` and asserts the store or tree is byte-identical afterwards; a test that only checks
the return value cannot see this. The same boundary applies to `$ErrorActionPreference` and
`$VerbosePreference`. Related: [[project_relative_path_in_pwsh_dotnet_io_hits_wrong_worktree]] —
both are "the tool resolved something against the wrong scope and reported success".
