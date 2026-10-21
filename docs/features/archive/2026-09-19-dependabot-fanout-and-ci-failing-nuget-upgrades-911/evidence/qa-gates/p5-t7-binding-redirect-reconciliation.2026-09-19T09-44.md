# P5-T7 — Binding-redirect reconciliation implemented in ProjectConsistency.psm1

Timestamp: 2026-09-19T09-44

Command:

```
pwsh -NoProfile -Command 'Set-Location "<execution-worktree-root>"; Import-Module "<execution-worktree-root>\scripts\dependencies\ProjectConsistency.psm1" -Force -ErrorAction Stop; Invoke-BindingRedirectReconciliation -AppConfigText <AC14 redirect fixture> -AssemblyName "Contoso.Widgets" -AssemblyVersion "2.0.0.0"; Invoke-BindingRedirectReconciliation -AppConfigText <AC14 no-redirect fixture> -AssemblyName "Contoso.Widgets" -AssemblyVersion "2.0.0.0"'
```

EXIT_CODE: 0

## Output Summary

```
IMPORT=ok
LINES=362
R1_EXAMINED=1 R1_REPAIRS=1
R1> <bindingRedirect oldVersion="0.0.0.0-2.0.0.0" newVersion="2.0.0.0" />
R2_UNCHANGED=True R2_REPAIRS=0 R2_EXAMINED=1
```

The first fixture entered the run with `oldVersion="0.0.0.0-1.0.3.0"` and
`newVersion="1.0.3.0"`. Both the upper bound of `oldVersion` and `newVersion` now name the
resolved assembly version `2.0.0.0`, and the lower bound `0.0.0.0` is untouched.

The second fixture declares a redirect for `Fabrikam.Core` only. It was returned
byte-identical, confirmed by a case-sensitive `-ceq` comparison against the input, with no
repair recorded. `R2_EXAMINED=1` shows the function did examine the one redirect block
present rather than returning early without looking, so the unchanged result is a decision
rather than a no-op.

## Acceptance

| Clause | Required | Measured |
|---|---|---|
| Module imports without error | yes | `IMPORT=ok`, run with `-ErrorAction Stop` |
| File remains at most 500 lines | <= 500 | 362 |

## Design notes

- Parsing is delegated to `ConvertFrom-AppConfigText` in `PackageGraph.psm1`, which supplies
  the examined count and decides whether a redirect for the named assembly exists at all.
  The rewrite itself is a byte-exact substitution over the document's own text confined to
  the matching `<dependentAssembly>` block, so no line ending and no unrelated attribute is
  disturbed. No external text-substitution executable is invoked.
- An `oldVersion` written as a range has its upper bound replaced and its lower bound left
  alone; an `oldVersion` written as a single version is replaced outright, there being no
  bound to preserve. Both behaviours are documented in the function's help.
- An application configuration carrying no redirect for the named assembly is returned
  unchanged. Adding a redirect for an assembly the project never redirected is a new
  decision rather than a reconciliation, and the central invariant this change enforces is
  about moving existing dependent elements into agreement, not about creating them.
