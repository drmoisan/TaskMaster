# R5 Fix — The Resolved Assembly Version Is Threaded Through the Entry Point

- Timestamp: 2026-09-20T08-50-30
- Task: [P2-T3]
- Finding: R5, decision D1
- EXIT_CODE: 0

## What Was Done

In `scripts/dependencies/ConsistencyVerifier.psm1`, inside `Invoke-ProjectConsistencyRepair`'s
package loop and immediately before the `Invoke-VersionReconciliation` call:

```powershell
        # No assembly evidence here, so the resolver returns the version the project already declares and the Reference line is written back unchanged; omitting the argument would rewrite it to the package version.
        $assemblyVersion = Resolve-ReferenceAssemblyVersion -PackageId $entry.Id -PackageVersion $entry.Version -ProjectText $text
        $reconciled = Invoke-VersionReconciliation -ProjectText $text -PackageId $entry.Id `
            -ManifestVersion $entry.Version -AssemblyVersion $assemblyVersion
```

`Resolve-ReferenceAssemblyVersion` is called with no `-IdentityProvider`, so it returns
`$declared` — the version the project's own `Include` attribute already carries — and
`Get-RewrittenReferenceVersionLine` writes back what is already there. The line comes out
byte-identical, `$after -ceq $before` holds, and no repair record is emitted for it.

The function is reachable here because [P2-T1] moved it into `ProjectConsistency.psm1`, which
`ConsistencyVerifier.psm1` already imports at line 30.

## The `.DESCRIPTION` Contract

Added to `Invoke-ProjectConsistencyRepair`'s `.DESCRIPTION`:

```
        The folder-segment kinds are reconciled to the manifest version, but this function
        preserves the declared Reference assembly version, because an assembly version need not
        track its package version and this function holds no assembly evidence. A consumer needing
        evidence-based reference resolution uses the composition root Repair-PackageManifestConsistency.ps1.
```

All three clauses the plan requires are present: the folder-segment kinds reconcile to the
manifest version; the declared Reference assembly version is preserved and the reason is the
absence of assembly evidence; and the composition root is named as the route for evidence-based
resolution.

## Acceptance

| Clause | Required | Measured | Result |
|---|---|---|---|
| `Select-String -SimpleMatch '-AssemblyVersion'` in the file | at least 1 | **1** | PASS |
| File line count | at most 500 | **499** | PASS |
| `.DESCRIPTION` contains `preserves the declared Reference assembly version` | yes, 1 occurrence | **1** | PASS |

Anchored numstat: `7  1  scripts/dependencies/ConsistencyVerifier.psm1` — seven additions, one
deletion, the deletion being the replaced `Invoke-VersionReconciliation` call line.

## The Size Constraint Was Binding and Is Recorded

The first form of this edit took the file to **510 lines**, over the 500-line cap in
`.claude/rules/general-code-change.md`. It carried a five-line rationale comment at the call
site and an eleven-line `.DESCRIPTION` paragraph.

Rather than halting, the edit was **tightened to fit**, which is the action available before the
plan's halt branch applies: the halt branch exists so the executor does not delete unrelated
content to make room, and no unrelated content was deleted. Two compressions were applied:

- the call-site rationale went from five comment lines to one, keeping both facts — that the
  resolver returns the declared version, and that omitting the argument would rewrite it to the
  package version;
- the `.DESCRIPTION` paragraph went from eleven lines to four, keeping all three required
  clauses and the exact mandated fragment.

Nothing that existed before this task was removed. The single deleted line is the
`Invoke-VersionReconciliation` call the edit replaced.

**Observation for the reviewer.** `ConsistencyVerifier.psm1` now measures **499 of 500**, one line
of headroom. R9d declared it at capacity at 493 and it is more so now. The next addition to this
file must extract rather than append, exactly as decision D1 did for the composition root. This is
recorded as an observation rather than acted on, because extracting a second function is a new
independent outcome this plan does not describe.

## Output Summary

`Invoke-ProjectConsistencyRepair` now resolves the assembly version per package and passes it to
`Invoke-VersionReconciliation`. The failure mode is removed for **every** caller, whether or not
it supplies anything, which is a stronger property than the parameter the review proposed. The
file measures 499 lines, one under the cap, and the binding size constraint is recorded.
