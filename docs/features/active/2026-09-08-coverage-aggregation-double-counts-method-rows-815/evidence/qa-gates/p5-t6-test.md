# P5-T6 — Final QA Loop, Stage 4: Test

Timestamp: 2026-09-09T11-32
Task: [P5-T6]
EXIT_CODE: 0

## Command 1 — the test run

Command: `mcp__drm-copilot__run_poshqc_test` with `scan_folders` = [`tests/scripts/vscode`]
EXIT_CODE: 0

```
{"ok":true,"tool":"run_poshqc_test","summary":"Ran bundled PoshQC test against the workspace with 1 selected scan folder(s)."}
```

The `workspace_root` value the tool echoes is an absolute host path and is omitted from the
transcription above.

## Command 2 — the JUnit reader

Command: `pwsh -NoProfile -Command '[xml]$j = Get-Content -Raw -LiteralPath "artifacts/pester/pester-junit.xml"; $r = $j.DocumentElement; if (-not $r.HasAttribute("tests")) { throw "junit root carries no tests attribute" }; Write-Output ("TESTS=" + $r.GetAttribute("tests") + " ERRORS=" + $r.GetAttribute("errors") + " FAILURES=" + $r.GetAttribute("failures"))'`
EXIT_CODE: 0

```
TESTS=103 ERRORS=0 FAILURES=0
```

## Comparison against the recorded baseline

| Measurement | TESTS | ERRORS | FAILURES |
| --- | --- | --- | --- |
| Baseline, `evidence/baseline/p0-t7-test-baseline.md` | 96 | 0 | 0 |
| Post-change, this run | **103** | **0** | **0** |

103 is strictly greater than the baseline 96, and the difference of 7 is exactly the seven tests
P1-T1 added.

## The first pass of this stage failed, and what it found

This is the second pass of the Phase 5 loop. On the first pass, run at 2026-09-09T11-26, the tool
exited with code 6 and the JUnit report recorded `TESTS=103 ERRORS=0 FAILURES=6`. All six failures
carried the same message, `RuntimeException: Cobertura XML does not contain a <packages> node.`, and
all six were pre-existing tests that this feature did not author:

| File | Test |
| --- | --- |
| `Invoke-MSTest.RunSettings.Tests.ps1` | `collects and post-processes coverage on the fully mocked main happy path` |
| `Invoke-MSTest.RunSettings.Tests.ps1` | `passes the generated Cobertura result to the threshold evaluator before completing successfully` |
| `Invoke-MSTest.RunSettings.Tests.ps1` | `excludes assemblies discovered under a .claude worktree segment` |
| `Invoke-MSTestWithCoverage.AssemblyDiscovery.Tests.ps1` | `includes an assembly directly beneath a search root that is itself under a .claude worktree segment` |
| `Invoke-MSTestWithCoverage.AssemblyDiscovery.Tests.ps1` | `excludes a nested sibling worktree beneath a non-dot-claude search root` |
| `Invoke-MSTestWithCoverage.AssemblyDiscovery.Tests.ps1` | `retains the root-level assembly and excludes a further-nested worktree beneath a dot-claude search root` |

**This falsified a premise stated in plan decision D3.** D3 asserts that
`Invoke-MSTestWithCoverageMain` "returns early at line 324 when `-NoExecute` is supplied. The
existing tests exercise it only through that early return, so no unit test can reach the
post-processing block at lines 339-345." That is not the case: these six tests call
`Invoke-MSTestWithCoverageMain` **without** `-NoExecute` and do reach the block, with
`ConvertTo-KoverageCoberturaXml` mocked to return the minimal stub `<coverage line-rate="0.8" />`.
`Assert-CoberturaLineCoverageThreshold` tolerates that stub because it reads only the root
`line-rate` attribute; the first-party report added by P2-T3 does not, because
`Get-CoberturaFirstPartyCoverageSummary` rejects a document with no `<packages>` node, matching
`Get-CoberturaCoverageSummary` exactly as plan decision D2 requires.

## The repair, and why it weakens nothing

The real `ConvertTo-KoverageCoberturaXml` always emits a `<packages>` element, so the stub was less
structurally valid than the output it stands in for. The repair adds an empty `<packages />` element
to the stub in both files and updates the one exact-match assertion that quotes the stub so it still
compares the whole string. Specifically, `<coverage line-rate="0.8" />` becomes
`<coverage line-rate="0.8"><packages /></coverage>` at three sites in
`Invoke-MSTest.RunSettings.Tests.ps1` and one in
`Invoke-MSTestWithCoverage.AssemblyDiscovery.Tests.ps1`.

No assertion was weakened, no assertion was deleted, no expected value was relaxed to match an
observed one, and no production behaviour was relaxed: the new function still rejects a document
carrying no `<packages>` node, and test T-D still asserts that rejection message. The numstat for the
repair is 3 insertions and 3 deletions in the first file and 1 and 1 in the second, so **neither file
gained or lost a line**: `Invoke-MSTest.RunSettings.Tests.ps1` remains at 496 and
`Invoke-MSTestWithCoverage.AssemblyDiscovery.Tests.ps1` at 99, and the counts recorded in
`evidence/qa-gates/p4-t5-file-line-counts.md` remain accurate.

The repair was committed before the loop restarted, so P5-T2's tree observation stayed satisfiable.
The loop then restarted from P5-T1 and completed a full clean pass; see
`evidence/qa-gates/p5-t9-toolchain-loop.md`.

**Deviation recorded.** `tests/scripts/vscode/Invoke-MSTest.RunSettings.Tests.ps1` and
`tests/scripts/vscode/Invoke-MSTestWithCoverage.AssemblyDiscovery.Tests.ps1` are not named in the
Write Set of `spec.md` and not named in the plan's D1 change-budget inventory, because both documents
inherited D3's false premise that no unit test reaches the post-processing block. Both files sit
under the permitted `tests/scripts/vscode/` prefix of AC13, so the scope boundary still holds. The
per-batch change budget in `.claude/rules/powershell.md` allows at most 3 test files; this delivery
now modifies exactly 3, so it remains within cap.

Output Summary: PoshQC test over `tests/scripts/vscode` returns ok, and the bundled JUnit report
records `TESTS=103 ERRORS=0 FAILURES=0`. The total is strictly greater than the recorded baseline of
96, by exactly the seven tests this feature added. The first pass of this stage failed with 6
failures caused by a false premise in plan decision D3; the repair made two pre-existing stub
fixtures structurally valid without weakening any assertion, and is recorded above as a deviation.
