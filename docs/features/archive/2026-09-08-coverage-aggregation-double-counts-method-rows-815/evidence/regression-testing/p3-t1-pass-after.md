# P3-T1 — Pass-After Evidence

Timestamp: 2026-09-09T11-03
Task: [P3-T1]
Command: `pwsh -NoProfile -Command '$r = Invoke-Pester -Path "tests/scripts/vscode/Invoke-MSTestWithCoverage.FirstParty.Tests.ps1" -PassThru; Write-Output ("PASSED=" + $r.PassedCount + " FAILED=" + $r.FailedCount)'`
EXIT_CODE: 0

## Result

```
Starting discovery in 1 files.
Discovery found 7 tests in 110ms.
[+] tests\scripts\vscode\Invoke-MSTestWithCoverage.FirstParty.Tests.ps1 508ms (165ms|252ms)
Tests completed in 518ms
Tests Passed: 7, Failed: 0, Skipped: 0, Inconclusive: 0, NotRun: 0
PASSED=7 FAILED=0
```

## Fail-before / pass-after pair

| Run | Artifact | Result |
| --- | --- | --- |
| Before the fix | `evidence/regression-testing/p1-t2-fail-before.md` | `PASSED=0 FAILED=7` |
| After the fix | this artifact | `PASSED=7 FAILED=0` |

Both runs used the identical command over the identical test path, so the pair is auditable in one
place. The seven tests are T-A through T-G of plan decision D9. T-B and T-C, which discharge AC4, are
among the seven that pass.

## Recorded finding — one structural change was required between the two runs

The first attempt at this task returned `PASSED=6 FAILED=1`. The single failure was
`CommandNotFoundException: The term 'Get-DescendantAxisCoverageTally' is not recognized`, raised from
test T-A. The cause is a Pester 5 scoping rule rather than a defect in the fix: Pester runs each `It`
block in a child scope of its containing block, and a function defined at file scope is evaluated
during the discovery pass into a session state the run pass does not share, so it is not resolvable
from an `It`. Moving the private differential helper from file scope into the `BeforeAll` block, with
no change to its body, its parameters or its arithmetic, made it resolvable and the run returned
`PASSED=7 FAILED=0`.

This is recorded rather than silently corrected because it changed the delivered test file after the
P1-T2 red run was captured. The change is confined to the helper's placement. The seven `It` names,
their assertions, the fixtures and the single occurrence of the descendant-axis literal are all
unchanged, so the P1-T2 red result remains representative of the delivered file: re-run without the
production file, six of the seven tests would still fail with an unresolved
`Get-CoberturaFirstPartyCoverageSummary` and the seventh with an unresolved
`Get-CoberturaFirstPartyCoverageReport`. The file's physical line count rose from 269 to 271, which
is recorded as an appended re-measurement in `evidence/regression-testing/p1-t3-test-file-size.md`.

Output Summary: `PASSED=7 FAILED=0`. All seven regression tests pass against the delivered
implementation, where all seven failed against its absence. No assertion was weakened, and no
expected value was edited to match an observed one.
