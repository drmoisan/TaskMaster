## [P2-T7] Repository-Wide Coverage No-Regression Gate — Pass 1 (FAIL, restart triggered)

- Timestamp: 2026-08-08T21-30
- Command: `pwsh -NoProfile -Command "[xml]$b = Get-Content .../coverage-remediation-baseline.cobertura.xml; [xml]$f = Get-Content .../coverage-remediation-final.cobertura.xml; '{0} {1} {2} {3}' -f $b.coverage.'line-rate', $b.coverage.'branch-rate', $f.coverage.'line-rate', $f.coverage.'branch-rate' ; exit $LASTEXITCODE"`
- EXIT_CODE: 0
- Output Summary: baseline `line-rate=0.858512 branch-rate=0.792359`; final (pass 1) `line-rate=0.858548 branch-rate=0.792717`.

### Gate evaluation against the fixed floor (0.858665 line / 0.792502 branch)

- `branch-rate = 0.792717 >= 0.792502`: PASS.
- `line-rate = 0.858548 >= 0.858665`: **FAIL** (short by `0.000117`, i.e. 13 lines out of 111204).

**Disposition:** FAIL. Per the plan's Phase 2 preamble ("Run in order; if any step fails or changes files, restart from P2-T1"), Phase 2 is restarted from P2-T1. This miss is consistent with the run-to-run coverage nondeterminism already root-caused in P0-T8's evidence (isolated to three classes outside the R1 scope: `EfcHomeController.cs`, `PropertyStore.cs` (a wall-clock-timing-adjacent helper), `SegmentStopWatch.cs`); the R1 target file's coverage is confirmed correct and stable at P2-T6 (4/4 branches, 5/5 lines). No production or test file is modified as part of this restart — only the toolchain loop is re-run to obtain one clean, uninterrupted passing pass, per plan mandate. This pass-1 result is superseded by the pass-2 result recorded separately; it is retained here as an honest record of the actual measured outcome, per the rule that a failing verification must be recorded as a FAIL, not silently discarded.
