# Helper Scripts (R1, issue #985)

Timestamp: 2026-10-09T15-15
Command: git hash-object --no-filters <SCRATCH>\985r1-cmd\<name> (for each of the four helpers)
EXIT_CODE: 0
Output Summary:
- 4 helper scripts written with the Write tool into `<SCRATCH>\985r1-cmd\` (outside the repository; never committed).
- `985-pester.ps1`, `985-junit.ps1`, `985-repair.ps1`: copied from `plan.2026-10-09T13-06.md` lines 248-307, 313-329, 335-361, with `Format-SafeLine` (lines 85-94) pasted directly after each `param` block.
- `985r1-identity-sweep.ps1`: verbatim from remediation plan section 5.
- Backslash check: the doubled-backslash character classes survived the write (Grep for the two-backslash-slash form: 2 lines in the sweep script, 1 line in each of the other three).

| Name | Path | git hash-object --no-filters |
|---|---|---|
| 985-pester.ps1 | `<SCRATCH>\985r1-cmd\985-pester.ps1` | 74ddc15e1a935c4b5dbd0efa4a72b63d9fcc0913 |
| 985-junit.ps1 | `<SCRATCH>\985r1-cmd\985-junit.ps1` | 4df03fc0b964ac2d337c37141a5266f89b5c96a3 |
| 985-repair.ps1 | `<SCRATCH>\985r1-cmd\985-repair.ps1` | 5cc7c5caaa7f90e37076de4ead07ce66332fe948 |
| 985r1-identity-sweep.ps1 | `<SCRATCH>\985r1-cmd\985r1-identity-sweep.ps1` | dbc71b4b34ca68a13ccfa15b37b6005d473f6a9e |
