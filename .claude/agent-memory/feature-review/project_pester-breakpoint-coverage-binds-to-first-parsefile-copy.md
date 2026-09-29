---
name: pester-breakpoint-coverage-binds-to-first-parsefile-copy
description: Pester 5.6.1 breakpoint coverage credits an entry-point line only if the FIRST test file that executed the function (by sort order) reaches it; suites that import via Parser::ParseFile+GetScriptBlock each run a separate compiled copy, so a line reached only by a later-sorting suite reads 0 hits although its test passes (#928). Path-loaded part files ARE credited from any suite (measured at #928 remediation) - relocate logic there.
metadata:
  type: project
---

Observed at #928 (2026-09-29). `scripts/vscode/Invoke-MSTestWithCoverage.ps1` is imported by four suites
through `[Parser]::ParseFile(...).GetScriptBlock()` and dot-sourced per file. Under the CI route
(`_pester.yml`: Pester 5.6.1, default `CodeCoverage.UseBreakpoints`), PowerShell binds each pending line
breakpoint to the first compiled copy in which the function runs. `Invoke-MSTest.RunSettings.Tests.ps1`
sorts first (a period sorts before `W`), so every entry-point line reached only by
`Invoke-MSTestWithCoverage.*.Tests.ps1` records no hit. The executor proved it with ordered runs: new file
alone -> hit; AssemblyDiscovery then new file -> 0; reversed -> hit; `UseBreakpoints=$false` -> hit.

Part files dot-sourced BY PATH from inside the entry point are credited from every suite. This was MEASURED
at the #928 remediation (r1-p0-t7, 2026-09-29): a control run over the AssemblyDiscovery suite alone left
`Threshold.ps1` throw lines 53/54/123/124 uncovered; the ordered two-file run (AssemblyDiscovery, then the
Scope suite that is the only one reaching those throws) credited all four, with `CONTAINER_ORDER` proving the
order. The relocation fix then read 13/13 on the part file and restored the entry point to its base figure.
The interpreter mechanism (per-path compiled-script cache) is still an inference; the crediting behaviour is
the measured fact - word comments and audits accordingly.

**Why:** a "changed line uncovered" gate can fail on a line that a passing test demonstrably executes.
Treat it as a crediting defect, not a missing test, but do NOT wave it through: the changed-line rule in
`.claude/rules/powershell.md` is blocking and the AC text is explicit.

**How to apply:**
- When an entry-point changed line reads 0 hits, check whether the only test reaching it lives in a
  suite that sorts after the first suite importing that script via ParseFile. Ask for the ordered
  two-file runs if the executor did not do them.
- Remediation that worked: relocate the logic into a path-loaded part file behind one unconditional
  entry-point call (thin wiring). Gate it on a two-file diagnostic whose subject is a part-file line reached
  ONLY by the later-sorting suite (the new part file's own lines were reached by every suite, so they could
  not fail the diagnostic - pick a subject that can). Not `UseBreakpoints=$false` locally (diverges from CI,
  invalidates the baseline) and not editing the first-binding sibling (out of scope; order-dependent).
- Pre-existing repo-wide fix worth promoting: switch the four suites to `. $script:coverageScript`
  (the `InvocationName` guard at the entry point's tail makes path dot-sourcing safe), or change CI to
  profiler coverage as a workflow item with a green run.
