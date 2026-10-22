# P6-T6 — Full suite with coverage, post-change

Timestamp: 2026-09-04T01-57

Command: the P6-T6 command block from the plan, run verbatim. As P0-T6 records, the runner script
Invoke-MSTestWithCoverage.ps1 is deliberately not invoked as an entry point; it is dot-sourced and
its post-discovery sequence is reproduced with the single `\.claude\` filter clause removed and every
other clause character-identical. That clause is upstream defect #752, owned by a separate item;
scripts/vscode/** is outside this item's ratified Write Set and no task in this plan modifies it.

```
$repoRoot = (Resolve-Path '.').Path
$repoRoot -ieq ((git rev-parse --show-toplevel) -replace '/', '\').TrimEnd('\')
$discovered = @(Get-ChildItem -Path $repoRoot -Recurse -Filter '*.Test.dll' | Where-Object { $_.FullName -match '\\bin\\Debug\\' -and $_.FullName -notmatch '\\obj\\' -and $_.FullName -notmatch '\\ref\\' } | Select-Object -ExpandProperty FullName)
$discovered.Count
$discovered | ForEach-Object { Split-Path $_ -Leaf } | Sort-Object -Unique
@($discovered | Where-Object { -not $_.StartsWith($repoRoot, [System.StringComparison]::OrdinalIgnoreCase) }).Count
@($discovered | Where-Object { $_.Substring($repoRoot.Length) -match '\\\.claude\\worktrees\\' }).Count
. (Join-Path $repoRoot 'scripts\vscode\Invoke-MSTestWithCoverage.ps1')
. (Join-Path $repoRoot 'scripts\vscode\Invoke-MSTestWithCoverage.Helpers.ps1')
$testAssemblies = @(Get-ChildItem -Path $repoRoot -Recurse -Filter '*.Test.dll' | Where-Object { $_.FullName -match '\\bin\\Debug\\' -and $_.FullName -notmatch '\\obj\\' -and $_.FullName -notmatch '\\ref\\' } | Select-Object -ExpandProperty FullName)
@(Compare-Object -ReferenceObject $discovered -DifferenceObject $testAssemblies).Count
$output = Join-Path $repoRoot 'coverage\p6-t6-postchange.cobertura.xml'
New-Item -ItemType Directory -Force -Path (Split-Path $output -Parent) | Out-Null
$vstestPath = Invoke-VsWhereExe -VsWherePath (Join-Path ${env:ProgramFiles(x86)} 'Microsoft Visual Studio\Installer\vswhere.exe') -VsWhereArgs @('-latest', '-products', '*', '-find', 'Common7\IDE\Extensions\TestPlatform\vstest.console.exe') | Select-Object -First 1
$runSettingsPath = Resolve-RunSettingsPath -ScriptRoot (Join-Path $repoRoot 'scripts\vscode')
$coverageStatus = 'PASS'
try {
    Invoke-DotnetCoverageCollection -OutputPath $output -CoverageConfig (Join-Path $repoRoot 'coverage.config') -VsTestPath $vstestPath -TestAssembly $testAssemblies -RunSettingsPath $runSettingsPath
    $processedXml = ConvertTo-KoverageCoberturaXml -XmlContent (Get-Content $output -Raw -Encoding UTF8) -RepoRoot $repoRoot
    Set-Content -Path $output -Value $processedXml -Encoding UTF8 -NoNewline
    Assert-CoberturaLineCoverageThreshold -CoberturaXml $processedXml
}
catch { $coverageStatus = $_.Exception.Message }
$coverageStatus
```

EXIT_CODE: 0

`$coverageStatus` printed the literal `PASS`.

**This artifact records the second execution of P6-T6**, run after the toolchain-loop restart that
P6-T13 caused. P6-T13 added three tests, so the Cobertura document produced by the first execution
went stale the moment it completed; the document measured here is the refreshed one, and it is the
document P6-T8, P6-T9 and P6-T10 read.

## Preconditions and discovery

- `$repoRoot` equality line printed `True`: **yes**. Neither path is written into this artifact.
- Discovered assembly count: **9**. Not greater than 9, so the drift branch P0-T6 defines is not
  entered.
- Off-root count: **0**. Structural self-check of the block — 0 by construction because the
  enumeration is rooted at `$repoRoot` — recorded for the reader on that basis.
- Nested-worktree count: **0**. This is the clause that can fail; its zero is the recorded proof that
  no worktree created inside this one contributed an assembly, which is the one path by which a
  sibling assembly could satisfy the `$repoRoot` prefix and reintroduce the duplicate-instrumentation
  condition upstream issue #752 added the runner's fourth filter clause to prevent.
- Leaf file names, deduplicated and sorted: `QuickFiler.Test.dll`, `SVGControl.Test.dll`,
  `Tags.Test.dll`, `TaskMaster.Test.dll`, `TaskTree.Test.dll`, `TaskVisualization.Test.dll`,
  `ToDoModel.Test.dll`, `UtilitiesCS.Test.dll`, `VBFunctions.Test.dll` — exactly the required
  nine-name set.
- `Compare-Object` count: **0**. The set the clauses above were evaluated over is the set the
  `-TestAssembly` parameter received, unchanged between the two evaluations.

## Test run

`Test Run Successful.` — Total tests: **7013**, Passed: **7013**, **Failed: 0**. Total time 35.1547
seconds.

The total rose from the 6995 the P0-T6 baseline records by exactly **18**, which is the number of
test methods this plan authors: six in `AppOlObjectsArchiveRootComGuardTests` (P1-T5), and twelve in
`EfcFormControllerTests.Part2.cs` — four from P2-T2, two from P2-T7, one from P3-T1, two from P4-T3,
and three from P6-T13. The first execution of this task recorded 7010, before P6-T13's three. No
pre-existing test was removed or renamed.

## Cobertura document

- Path (gitignored): `coverage/p6-t6-postchange.cobertura.xml`
- Byte size: 12731976
- SHA-256: `A462D34E34BCA57A8AFC77A861562C1CBD5674B27EAC062BFE3DBC729044A777`

Repository-wide rates read from the `/coverage` root element of the emitted, post-processed document:

| Attribute | Raw value | Percentage to two decimals |
|---|---|---|
| `line-rate` | 0.85459 | **85.46%** |
| `branch-rate` | 0.795242 | **79.52%** |

Supporting root attributes: `lines-covered` = 55321, `lines-valid` = 64734.

## Session isolation

This command block was run in a PowerShell session started for it alone and used for no other task in
this plan: **yes**. It was executed from a `.ps1` file under the gitignored `coverage` directory via
`pwsh -NoProfile -File`, in a process of its own. That keeps the `Set-StrictMode -Version Latest` and
`$ErrorActionPreference = 'Stop'` the dot-source installs at Invoke-MSTestWithCoverage.ps1 lines
245-246 from reaching any `[expect-fail]` run task, whose expected non-zero `vstest.console.exe`
exit would otherwise be converted into a thrown error and mis-recorded.

Per D10, neither the absolute worktree root nor any absolute assembly path is written into this
artifact.

Output Summary: 9 test assemblies discovered, matching the required nine-name set exactly; off-root
0, nested-worktree 0, `Compare-Object` 0, `$repoRoot` equality `True`. Test run successful with 7013
total, 7013 passed, **0 failed** — 18 more than the 6995 baseline, exactly the number of tests this
item adds. `$coverageStatus` printed `PASS`. Repository-wide line-rate **85.46%** and branch-rate
**79.52%** (raw 0.85459 and 0.795242; lines-covered 55321, lines-valid 64734). Cobertura document
12731976 bytes, SHA-256 `A462D34E34BCA57A8AFC77A861562C1CBD5674B27EAC062BFE3DBC729044A777`.
