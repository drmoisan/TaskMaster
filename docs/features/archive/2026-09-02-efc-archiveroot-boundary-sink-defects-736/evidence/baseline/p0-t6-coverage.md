# P0-T6 — Full-suite coverage baseline

Timestamp: 2026-09-03T23-37

Command: the P0-T6 command block from the plan, run verbatim. The runner script
Invoke-MSTestWithCoverage.ps1 is deliberately **not** invoked as an entry point; it is dot-sourced,
and its post-discovery sequence is reproduced with the single `\.claude\` filter clause removed and
every other clause character-identical. That clause is upstream defect #752, owned by a separate item
in this run; scripts/vscode/** is outside this item's ratified Write Set and no task in this plan
modifies it.

```
$repoRoot = (Resolve-Path '.').Path
$repoRoot -ieq ((git rev-parse --show-toplevel) -replace '/', '\').TrimEnd('\')
$discovered = @(Get-ChildItem -Path $repoRoot -Recurse -Filter '*.Test.dll' | Where-Object { $_.FullName -match '\\bin\\Debug\\' -and $_.FullName -notmatch '\\obj\\' -and $_.FullName -notmatch '\\ref\\' } | Select-Object -ExpandProperty FullName)
$discovered.Count
$discovered | ForEach-Object { Split-Path $_ -Leaf } | Sort-Object -Unique
@($discovered | Where-Object { -not $_.StartsWith($repoRoot, [System.StringComparison]::OrdinalIgnoreCase) }).Count
@($discovered | Where-Object { $_.Substring($repoRoot.Length) -match '\\\.claude\\worktrees\\' }).Count
Get-Command dotnet-coverage -ErrorAction SilentlyContinue | Select-Object -ExpandProperty Source
dotnet-coverage --version
. (Join-Path $repoRoot 'scripts\vscode\Invoke-MSTestWithCoverage.ps1')
. (Join-Path $repoRoot 'scripts\vscode\Invoke-MSTestWithCoverage.Helpers.ps1')
$testAssemblies = @(Get-ChildItem -Path $repoRoot -Recurse -Filter '*.Test.dll' | Where-Object { $_.FullName -match '\\bin\\Debug\\' -and $_.FullName -notmatch '\\obj\\' -and $_.FullName -notmatch '\\ref\\' } | Select-Object -ExpandProperty FullName)
@(Compare-Object -ReferenceObject $discovered -DifferenceObject $testAssemblies).Count
$output = Join-Path $repoRoot 'coverage\p0-t6-baseline.cobertura.xml'
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

`$coverageStatus` printed the literal `PASS`, so this task records `EXIT_CODE: 0` per the plan's
mapping. Neither the below-80% threshold branch nor the failed-collection branch was reached.

## Preconditions and discovery

- `$repoRoot` equality line printed `True`: **yes**. Neither path is written into this artifact.
- Discovered assembly count (`$discovered.Count`): **9**. Not greater than 9, so the drift branch is
  not entered and no directory enumeration is required.
- Off-root count: **0**. This is a structural self-check of the block — the enumeration is rooted at
  `$repoRoot`, so the value is 0 by construction — and is recorded for the reader on that basis.
- Nested-worktree count: **0**. This is the clause that can fail. Its zero is the recorded proof that
  no worktree created inside this one contributed an assembly, which is the one path by which a
  sibling assembly could satisfy the `$repoRoot` prefix and reintroduce the duplicate-instrumentation
  condition that upstream issue #752 added the runner's fourth filter clause to prevent.
- Leaf file names printed by the leaf-name line, deduplicated and sorted:
  `QuickFiler.Test.dll`, `SVGControl.Test.dll`, `Tags.Test.dll`, `TaskMaster.Test.dll`,
  `TaskTree.Test.dll`, `TaskVisualization.Test.dll`, `ToDoModel.Test.dll`, `UtilitiesCS.Test.dll`,
  `VBFunctions.Test.dll`. This is exactly the required nine-name set, the assembly names of the nine
  `*.Test.csproj` projects the solution carries.
- `Compare-Object` count: **0**. The set the four clauses above were evaluated over (`$discovered`)
  is the same set the `-TestAssembly` parameter received (`$testAssemblies`), and it did not change
  between the two evaluations.

## dotnet-coverage resolution

- `Get-Command dotnet-coverage` resolved: **yes**.
- Leaf file name it resolved to: `dotnet-coverage.exe`. The containing directory is deliberately not
  recorded, because a global tool resolves under the user profile and D10 forbids that token in a
  committed artifact.
- `dotnet-coverage --version` output, verbatim: `18.10.0+f4cc39224845ffa74bf246c9da2399d50e5d6342`

This is the exact condition the runner tests at Invoke-MSTestWithCoverage.ps1 line 292 before it
throws `dotnet-coverage not found.` at line 293, ahead of any assembly discovery.

## Test run

`Test Run Successful.` — Total tests: **6995**, Passed: **6995**, Failed: **0**. Total time 46.9746
seconds.

## Cobertura document

- Path (gitignored): `coverage/p0-t6-baseline.cobertura.xml`
- Byte size: **12723580** (non-zero)
- SHA-256: `47EEBBF1B92D308E73813C2A4047AD76BF20D04CF0BDC2CA13B66937665EDA91`

Repository-wide rates read from the `/coverage` root element of the emitted, post-processed Cobertura
document:

| Attribute | Raw value | Percentage to two decimals |
|---|---|---|
| `line-rate` | 0.854332 | **85.43%** |
| `branch-rate` | 0.795348 | **79.53%** |

Supporting root attributes: `lines-covered` = 55265, `lines-valid` = 64688.

## Session isolation

This command block was run in a PowerShell session started for it alone and used for no other task in
this plan: **yes**. That is what keeps the `Set-StrictMode -Version Latest` and
`$ErrorActionPreference = 'Stop'` the dot-source installs at Invoke-MSTestWithCoverage.ps1 lines
245-246 from reaching a later `[expect-fail]` run task, whose expected `vstest.console.exe` exit of 1
would otherwise be converted into a thrown error and mis-recorded.

Per D10, neither the absolute worktree root nor any absolute assembly path is written into this
artifact.

Output Summary: 9 test assemblies discovered, matching the required nine-name set exactly;
off-root 0, nested-worktree 0, `Compare-Object` 0, `$repoRoot` equality `True`. dotnet-coverage
resolved (`dotnet-coverage.exe`, version 18.10.0+f4cc39224845ffa74bf246c9da2399d50e5d6342). Test run
successful with 6995 total, 6995 passed, 0 failed. `$coverageStatus` printed `PASS`. Repository-wide
line-rate **85.43%** and branch-rate **79.53%** (raw 0.854332 and 0.795348; lines-covered 55265,
lines-valid 64688). Cobertura document 12723580 bytes, SHA-256
`47EEBBF1B92D308E73813C2A4047AD76BF20D04CF0BDC2CA13B66937665EDA91`.
