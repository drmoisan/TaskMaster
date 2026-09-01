# Phase 0 — Scoped test-runner search-root probe ([P0-T11])

Timestamp: 2026-09-01T22-04

Command: `pwsh -NoProfile -File scripts/vscode/Invoke-MSTest.ps1 -SearchRoot QuickFiler.Test -Configuration Debug -NoExecute`

The `-NoExecute` switch returns at line 125 after the discovery and count logic on lines 107 through 120
has run, so the probe exercises exactly the scalar-versus-array question without launching vstest.

EXIT_CODE: 1

## Complete stdout and stderr, verbatim

stdout: empty. Nothing was written to standard output.

stderr, verbatim with the ANSI colour escape sequences removed:

```
Invoke-MSTest.ps1: The property 'Count' cannot be found on this object. Verify that the property exists.
```

## Which of the two outcomes occurred

The second outcome occurred: **a terminating error was raised.** The line
`Discovered 1 test assemblies.` was **not** printed.

The error text names the `Count` property, which is the read the plan predicted would fail. Under
`Set-StrictMode -Version Latest` (set at `scripts/vscode/Invoke-MSTest.ps1` line 77), the discovery
pipeline on lines 107 through 113 ends in `Select-Object -ExpandProperty FullName`, which yields a bare
`System.String` rather than an array when exactly one assembly matches. Line 115 then evaluates
`if (-not $testAssemblies -or $testAssemblies.Count -eq 0)`; the left operand is false for a non-empty
string, so `-or` goes on to evaluate the right operand and reads `.Count` on a scalar, which StrictMode
rejects.

## Disposition

No later task in this plan depends on this outcome. The record exists to justify this plan's use of
`-SearchRoot .`, which matches nine assemblies, is therefore array-valued, and is unaffected by this
defect. It also supplies evidence for issue **#713**, which was opened for this defect during preparation.

The defect is **not fixed here**. It is out of scope for issue #663, and
`scripts/vscode/Invoke-MSTest.ps1` is not among this plan's three authorised source paths.

Output Summary: The scoped form `-SearchRoot QuickFiler.Test` raises a terminating error and exits 1
before running any test. The error is the StrictMode `.Count`-on-a-scalar read predicted for the
single-assembly discovery case; `Discovered 1 test assemblies.` was never printed. This is a measurement
that confirms the plan's stated reason for using `-SearchRoot .` in every wrapper invocation, and it is
recorded as evidence for issue #713 rather than remediated here.
