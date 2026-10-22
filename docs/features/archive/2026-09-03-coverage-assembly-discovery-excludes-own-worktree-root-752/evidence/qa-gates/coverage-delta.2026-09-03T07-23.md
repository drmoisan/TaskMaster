# Coverage Delta ([P3-T6])

Timestamp: 2026-09-03T12-16

Command: `pwsh -NoProfile -Command 'Set-Location "<repo-root>"; $p = "docs/features/active/2026-09-03-coverage-assembly-discovery-excludes-own-worktree-root-752/evidence/qa-gates/pester-coverage-postchange.iter1.2026-09-03T07-23.xml"; [xml]$x = Get-Content -LiteralPath $p -Raw; $sf = @($x.GetElementsByTagName("sourcefile") | Where-Object { $_.name -like "*Invoke-MSTestWithCoverage.ps1" }); "SOURCEFILE NODE COUNT=" + $sf.Count; $n = @(); foreach ($s in $sf) { $n += @($s.ChildNodes | Where-Object { $_.LocalName -eq "line" -and $_.nr -eq "301" }) }; "LINE301 NODE COUNT=" + $n.Count; if ($n.Count -gt 0 -and [int]$n[0].ci -gt 0) { "BASELINE CHANGED-LINE 301 COVERED: true" } else { "BASELINE CHANGED-LINE 301 COVERED: false" }; exit 0'`

EXIT_CODE: 0

POST-CHANGE XML READ: pester-coverage-postchange.iter1.2026-09-03T07-23.xml

The `.iter1` segment is correct: the toolchain loop `[P3-T1]` through `[P3-T5]` completed on its first pass, with `WRITE SET REWRITTEN BY FORMATTER: NONE` recorded in `evidence/qa-gates/poshqc-format.iter1.2026-09-03T07-23.md` and no stage failure, so iteration 1 is the final clean iteration and no later iteration exists.

BASELINE LINE COVERAGE PERCENT: 78.3042394014963

POST-CHANGE LINE COVERAGE PERCENT: 78.3312577833126

DELTA: +0.0270183818162906

BASELINE CHANGED-LINE 301 COVERED: false (copied from `evidence/baseline/pester-coverage-baseline.2026-09-03T07-23.md`)

## Emitted lines from the extraction command run here, verbatim (all three)

```
SOURCEFILE NODE COUNT=1
LINE301 NODE COUNT=1
BASELINE CHANGED-LINE 301 COVERED: true
```

Reading note for the third line: the extraction command is reproduced character-for-character from `[P0-T10]`, as `[P3-T6]` requires, so its hard-coded output label still spells the word `BASELINE` even though the file it read here is the post-change XML. The value `true` on that line is therefore the **post-change** observation, not a restatement of the baseline. The baseline value is the separate `BASELINE CHANGED-LINE 301 COVERED: false` line above, copied from the `[P0-T10]` artifact.

CHANGED-LINE COVERAGE: COVERED

## PLAN BRANCH DIVERGENCE (recorded rather than resolved)

`[P3-T6]` anticipates two combinations and classifies every other as a stop-and-report. The measured combination is a third: the baseline recorded `false` and the post-change measurement is `COVERED`. The measurement is reported here as observed; it was not adjusted to fit either branch.

Mechanism, stated from the measurements in hand rather than inferred:

- The baseline JaCoCo XML carried one `sourcefile` node for `Invoke-MSTestWithCoverage.ps1` and **no** `line` node numbered 301 (`LINE301 NODE COUNT=0`).
- The post-change XML carries one `sourcefile` node and **one** `line` node numbered 301, with a non-zero covered-instruction counter.
- Pester's own console summary moved from `802 analyzed Commands in 11 Files` at baseline to `803 analyzed Commands in 11 Files` post-change: exactly one additional analyzable command, which is the `[System.IO.Path]::GetRelativePath($resolvedSearchRoot, $_.FullName)` call the fix introduces on line 301. Before the change that line held only an operand of the `-and` chain begun on line 298 and carried no analyzable command of its own, which is why no counter was attributed to it.

Consequence: this is a coverage gain on the changed line, not a regression. The changed line moved from unmeasurable to measured-and-covered, and the file-level percentage rose. Neither branch's failure condition — a regression on the changed line, or an unexplained absence of a counter — is present. The `NOT REPORTED` branch could not be written truthfully, because its mandated note asserts that no per-line counter is attributed to that file under this suite, and the post-change XML shows one.

Blocking-condition assessment: the plan's blocking coverage condition, post-change greater than or equal to baseline, holds with a positive delta, and `[P0-T11]` already recorded that the absolute 85 percent floor was missed before this item touched anything. This divergence is raised in the executor's completion report.

Output Summary: Post-change line coverage over `scripts/vscode` is 78.3312577833126 percent against a baseline of 78.3042394014963 percent, a delta of +0.0270183818162906, so the post-change percentage is greater than or equal to the baseline percentage. The changed line, file line 301, is covered post-change; at baseline it carried no per-line counter at all. Pester reports command and line coverage only and emits no branch counter, so no branch figure is recorded or asserted.
