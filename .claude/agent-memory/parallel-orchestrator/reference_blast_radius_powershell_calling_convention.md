---
name: blast-radius-powershell-calling-convention
description: How to actually invoke Test-BlastRadiusConflict on TaskMaster — absolute Import-Module path, ConvertFrom-Json -AsHashtable -DateKind String, and read the conflict key; two failure modes both look like "no conflict"
metadata:
  type: reference
---

TaskMaster has no `scripts/dev_tools/` and no poetry, so the PowerShell port at
`.claude/lib/blast-radius/BlastRadius.psm1` is the ONLY way to compute conflict edges
here. `pwsh` is available (7.6.5). Working invocation:

```powershell
Import-Module (Join-Path $Repo '.claude/lib/blast-radius/BlastRadius.psm1') -Force
$cfg = Get-Content (Join-Path $Repo 'config/blast-radius.json') -Raw |
    ConvertFrom-Json -AsHashtable -DateKind String
$v = Test-BlastRadiusConflict -RadiusA $radiusA -RadiusB $radiusB -Config $cfg
$v['conflict']   # the verdict
$v['reasons']    # @( @{ kind; detail } ), in fixed kind order
```

**Why the details matter — three traps, and all three present as "no conflict":**

1. **`Import-Module` needs an ABSOLUTE path.** A repo-relative path fails to resolve even
   after `Set-Location`. Without `$ErrorActionPreference = 'Stop'` the script continues,
   `Test-BlastRadiusConflict` is then an unrecognized command, and `$v['conflict']` reads
   EMPTY — which coerces false. Every pair silently reports no contention. Guard with an
   explicit `Get-Command Test-BlastRadiusConflict` check that throws.
2. **`ConvertFrom-Json -AsHashtable` coerces `computed_at` into a `DateTime`**, and the
   library rejects it with `computed_at must be a string, got DateTime`. Add
   `-DateKind String` (PS 7.5+) to every `ConvertFrom-Json` that touches a radius or the
   checkpoint.
3. **Never boolean-test the returned object.** The module's own doc comment is explicit: a
   hashtable is unconditionally truthy in PowerShell, so `if ($result)` marks every pair
   as contending. Read `$result['conflict']`.

Note traps 1 and 3 fail in OPPOSITE directions — 1 reports nothing contends, 3 reports
everything does — so neither an all-clear nor an all-conflict result is self-validating.

4. **The `path_overlap` `detail` string is not always the pair that actually matched.**
   The `conflict` verdict is trustworthy; the `detail` is not, once either radius holds a
   glob. Observed 2026-08-29 deriving item 637: every one of its three edges reported
   `**/evidence/**/*.md ~ <first path of the other radius>` — including
   `~ .claude/settings.local.json`, which that glob cannot match. `**/evidence/**/*.md`
   sorts first in 637's paths because it begins with `*`, so the reported pair looks like a
   first-pair artifact rather than the matching pair. Radii whose first sorted path happens
   to be the matching one (the planner-seeded 638/644/647 edges) report accurately, which is
   why the quirk stays hidden on a replay check.

   **How to apply:** corroborate any `path_overlap` verdict with a plain set intersection of
   the two `paths` lists before writing an edge, and record a confirmed exact-overlap pair as
   the `detail`. Invariant 15 constrains only `a`, `b`, and `reason`, so `detail` is free-form
   and an accurate value is the useful one. A derived radius carrying a broad `**/…` glob will
   also contend with essentially every item forever — worth reporting when you see one.

**How to apply:** Before trusting a fresh computation, replay it against the edges already
in `parallel-orchestrator-state.json` and require an exact match on `conflict`, on the
reason `kind` set, and on each `detail` string. That replay is what proves the convention
rather than the result. Verified 2026-08-29 on run bugs-638-644-647: all three recorded
edges reproduced exactly. `Get-BlastRadius -PlanText -SpecText -FeatureFolder -Config
-ComputedAt` is the derivation entry point in the same module; the library never reads the
clock, so `ComputedAt` is always caller-supplied. `.ps1` files cannot be created with the
Write tool (the pre-implementation gate is extension-gated) — emit them with a bash
heredoc into the scratchpad instead. See [[parallel-run-execution-playbook]].
