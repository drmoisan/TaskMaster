# P4-T7 — Phase 4 Post-Format Line Counts

Timestamp: 2026-09-13T06-18
Task: [P4-T7]

Command: pwsh -NoProfile -Command '<Get-Content -LiteralPath over each of the three paths, element count of the returned content-line array>'
EXIT_CODE: 0

Measured after this phase's format step, so these are the figures the repository's 500-line ceiling
applies to.

```
scripts/vscode/Invoke-MSTest.ps1 262
tests/scripts/vscode/Invoke-MSTest.RunSettings.Tests.ps1 498
tests/scripts/vscode/Invoke-MSTest.Main.Tests.ps1 146
```

Output Summary: Three paths, three bare integers, every integer at most 500. Against the Phase 0
post-format baseline of 202, 491 and 144 respectively, the entry point grew by 60 lines for the two
new parameters, the summary part-file dot-source, the two resolved paths, the widened builder call and
the non-fatal summary-and-discard block; the shared argument-builder test file grew by 7 net lines
because converting its four plain-builder continuation calls to splatting reclaimed 8 lines against
the 13 lines of hashtable declaration and the 2 lines the exact-array assertion gained; and the main
test file grew by 2 lines for the two elements added to its exact-array assertion. The shared test
file stands 2 lines below the ceiling, which is why P4-T3 mandated splatting rather than continuation
calls: adding two arguments to four continuation call sites would have added 8 lines and taken the
file past 500.
