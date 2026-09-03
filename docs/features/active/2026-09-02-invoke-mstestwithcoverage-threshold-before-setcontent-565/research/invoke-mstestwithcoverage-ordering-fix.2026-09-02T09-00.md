---
title: Invoke-MSTestWithCoverage.ps1 — Assert-before-SetContent ordering fix
issue: 565
date: 2026-09-02T09-00
status: research
---

## 1. Current state of `scripts/vscode/Invoke-MSTestWithCoverage.ps1`

File: `scripts/vscode/Invoke-MSTestWithCoverage.ps1`
Content ends at line 491 in the sibling helpers file (see section 2) and at line 349 in this
file; a bare trailing newline is reported as an extra blank "line 350"/"line 492" by the read
tool. Confirmed 349 lines of content (not 341-343 as the issue's line numbers suggested — those
have not drifted for the two calls in question, which are exactly where the issue describes, but
the file's total content length differs slightly from any prior report).

The two calls under investigation are at their exact current line numbers, unchanged from the
issue's description:

```
333	    # Post-process the Cobertura XML for Koverage compatibility:
334	    #   1. Rewrite absolute paths to workspace-relative paths using native separators.
335	    #   2. Inject <sources><source>.</source></sources> (required by cobertura-parse).
336	    #   3. Remove <package> elements for third-party assemblies that are not part
337	    #      of the solution (dotnet-coverage instruments all loaded DLLs at runtime).
338	    Write-Output 'Post-processing coverage XML for Koverage compatibility...'
339	    $xmlContent = Get-Content $resolvedOutputPath -Raw -Encoding UTF8
340	    $processedXmlContent = ConvertTo-KoverageCoberturaXml -XmlContent $xmlContent -RepoRoot $repoRoot
341	    Assert-CoberturaLineCoverageThreshold -CoberturaXml $processedXmlContent
342	
343	    Set-Content -Path $resolvedOutputPath -Value $processedXmlContent -Encoding UTF8 -NoNewline
344	    Write-Output "Done. Coverage artifact: $resolvedOutputPath"
345	}
```

- **`Assert-CoberturaLineCoverageThreshold` call: line 341** (unchanged from the issue).
- **`Set-Content` call: line 343** (unchanged from the issue).
- Both calls are the last two statements of `Invoke-MSTestWithCoverageMain`, which spans lines
  248-345. There is one blank line (342) between them; no other statement sits between the two
  calls today.
- `Set-Content` uses **`-Path`** (not `-LiteralPath`) — this differs from the other `Set-Content`
  call in the same file (`Invoke-DotnetCoverageCollection`, line 219, which uses `-LiteralPath`
  for the derived coverage-settings file). Any new Pester mock/parameter-filter targeting this
  call must filter on `$Path`, not `$LiteralPath`.
- The fix is a pure statement reorder: swap the order of lines 341 and 343 (moving the
  `Set-Content` call above the `Assert-CoberturaLineCoverageThreshold` call) so `$resolvedOutputPath`
  always receives `$processedXmlContent` before the threshold check can throw. No other line in
  the function needs to change; `$processedXmlContent` is already fully computed by line 340 in
  both call orders.
- `Invoke-MSTestWithCoverageMain` is dot-sourced by `Invoke-MSTestWithCoverage.Helpers.ps1` at
  line 261 (`. (Join-Path $ScriptRoot 'Invoke-MSTestWithCoverage.Helpers.ps1')`), and the script
  guards its own top-level auto-invocation with `if ($MyInvocation.InvocationName -ne '.')` at
  lines 347-349 — this guard is unaffected by the fix.

## 2. `Assert-CoberturaLineCoverageThreshold` (Helpers.ps1)

File: `scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1`, lines 459-491 (491 lines of content
total in the file).

Exact signature:

```powershell
function Assert-CoberturaLineCoverageThreshold {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true)]
        [string]$CoberturaXml
    )
    ...
}
```

- Single mandatory `[string]$CoberturaXml` parameter. No `OutputType` attribute (the function
  never returns a value on success — it either throws or falls through to the end of the function
  body, which is `$null`).
- Body: parses `$CoberturaXml` into a **new local** `[xml]$coverageDocument` variable (line 466);
  it does not mutate the `$CoberturaXml` string parameter, nor any variable outside its own local
  scope. It reads `/coverage/line-rate`, throws on a missing/non-numeric/out-of-range line-rate,
  computes `$percentage = $lineRate * 100`, and throws `"Cobertura line coverage {0}% is below the
  required 80% threshold."` when `$percentage -lt 80` (line 487, threshold literal `80` — out of
  scope per the task background, tracked separately by #563).
- No I/O, no writes, no global state changes, and no return value consumed by the caller (the call
  site at line 341 does not capture a return value). This confirms reordering `Set-Content` ahead
  of this call is safe: the function has no side effect that `Set-Content` could observe or that
  could observe `Set-Content`'s effect, and it does not depend on the output file having already
  been written.

## 3. Testability of `Invoke-MSTestWithCoverageMain` via Pester

`Invoke-MSTestWithCoverageMain` **is already exercised directly by an existing Pester suite**
without any live `dotnet-coverage`/`vstest` run:
`tests/scripts/vscode/Invoke-MSTest.RunSettings.Tests.ps1`, `Describe 'Invoke-MSTestWithCoverageMain'`
(lines 345-414).

Dot-sourcing pattern used to make the function callable (lines 3-25 of that test file):

```powershell
BeforeAll {
    $script:repoRoot = (Resolve-Path (Join-Path $PSScriptRoot '..\..\..')).Path
    $script:mstestScript = Join-Path $script:repoRoot 'scripts\vscode\Invoke-MSTest.ps1'
    $script:coverageScript = Join-Path $script:repoRoot 'scripts\vscode\Invoke-MSTestWithCoverage.ps1'
    $script:scriptDir = Join-Path $script:repoRoot 'scripts\vscode'

    try { . $script:mstestScript -NoExecute } catch { Write-Verbose "Invoke-MSTest body skipped: $_" }

    $tokens = $null
    $parseErrors = $null
    $coverageAst = [System.Management.Automation.Language.Parser]::ParseFile(
        $script:coverageScript,
        [ref]$tokens,
        [ref]$parseErrors)
    $parseErrors | Should -BeNullOrEmpty
    . $coverageAst.GetScriptBlock()
    . (Join-Path $script:scriptDir 'Invoke-MSTestWithCoverage.Helpers.ps1')

    $script:expectedRunSettings = Join-Path $script:scriptDir 'TaskMaster.cli.runsettings'
}
```

The script is parsed via `[System.Management.Automation.Language.Parser]::ParseFile` and then
dot-sourced as a scriptblock (`. $coverageAst.GetScriptBlock()`) rather than invoked with `&` or
by path. Because dot-sourcing sets `$MyInvocation.InvocationName` to `'.'` for the scriptblock's
own top-level execution, the file's own bottom guard (`if ($MyInvocation.InvocationName -ne '.')`)
evaluates false and `Invoke-MSTestWithCoverageMain` is **not** auto-invoked — only its (and every
other function's) definition is imported. This is the same technique the two sibling helper test
files use for `Invoke-MSTestWithCoverage.Helpers.ps1` (plain `. $helperScriptPath`, since that
file has no top-level guard/auto-invocation at all).

`Invoke-DotnetCoverageCollection` — the function that shells out to `dotnet-coverage`/`vstest` — is
mocked with plain `Mock` in the existing `BeforeEach` (line 366: `Mock Invoke-DotnetCoverageCollection
{ $script:coverageCallCount++ }`), confirming it is directly mockable via Pester's `Mock` cmdlet
with no additional seam work required. The same `BeforeEach` (lines 346-372) also mocks every
other I/O boundary the fixed-up main function touches: `Resolve-Path`, `Test-Path`,
`Resolve-RunSettingsPath`, `Invoke-VsWhereExe`, `Get-Command`, `Get-ChildItem`, `Get-Content`,
`ConvertTo-KoverageCoberturaXml`, and `Set-Content`. This means the entire post-collection block
(lines 333-344, containing the two statements under investigation) is already fully reachable and
fully mockable in-process — a new regression test needs no new seam.

## 4. Deterministic reproduction of a sub-threshold run (no temp files)

The existing `BeforeEach` for `Describe 'Invoke-MSTestWithCoverageMain'` (lines 346-372) already
mocks `ConvertTo-KoverageCoberturaXml` to return a fixed in-memory string:

```powershell
Mock ConvertTo-KoverageCoberturaXml { '<coverage line-rate="0.8" />' }
```

and one existing `It` (lines 400-406) demonstrates overriding that mock per-test and reading the
value that reaches `Assert-CoberturaLineCoverageThreshold` by mocking the assert function itself:

```powershell
It 'passes the generated Cobertura result to the threshold evaluator before completing successfully' {
    $script:evaluatedCoberturaXml = $null
    Mock Assert-CoberturaLineCoverageThreshold { param([string]$CoberturaXml) $script:evaluatedCoberturaXml = $CoberturaXml }
    Mock ConvertTo-KoverageCoberturaXml { '<coverage line-rate="0.8" />' }
    Invoke-MSTestWithCoverageMain -ScriptRoot $script:scriptDir
    $script:evaluatedCoberturaXml | Should -Be '<coverage line-rate="0.8" />'
}
```

The smallest deterministic way to reproduce a sub-threshold run for this issue's regression test
is the same pattern, but leaving `Assert-CoberturaLineCoverageThreshold` **unmocked** (real) so it
genuinely throws, and overriding only `ConvertTo-KoverageCoberturaXml`'s mock return value to a
fixed Cobertura XML string whose `line-rate` is below 0.80:

```powershell
Mock ConvertTo-KoverageCoberturaXml { '<coverage line-rate="0.5" />' }
```

**Recommended fixture line-rate value: `0.5`** (50%). It is comfortably below the 80% threshold
(avoiding any boundary/rounding ambiguity — contrast with the existing boundary-focused fixtures
`0.799999`, `0.8`, `0.800001` in `Invoke-MSTestWithCoverage.Helpers.Tests.ps1` lines 495-497,
which exist specifically to pin the boundary and are not appropriate for a "clearly below
threshold, persistence still expected" regression test) and formats without decimal noise in the
thrown message (`"Cobertura line coverage 50% is below the required 80% threshold."`).

This is the convention already used throughout the repo's Helper tests: every Cobertura fixture in
`Invoke-MSTestWithCoverage.Helpers.Tests.ps1` and `Invoke-MSTestWithCoverage.ClosureFilter.Tests.ps1`
is an in-memory PowerShell here-string (`@'...'@`) assigned to a local variable and passed directly
as a string parameter — none of them touch the filesystem. The `Invoke-MSTest.RunSettings.Tests.ps1`
`Describe 'Invoke-MSTestWithCoverageMain'` block goes one step further and mocks `Get-Content`
itself (`Mock Get-Content { '<coverage />' }`, line 369) so that even the *raw* pre-post-processing
read never touches disk; a new regression test in that `Describe` block should follow the same
in-memory-only mocking, matching repo policy against temporary files in tests.

An alternative, higher-fidelity approach mentioned in the task — leaving `ConvertTo-KoverageCoberturaXml`
unmocked and exercising it for real against a raw fixture — is possible in principle (the function
is pure and accepts an `-XmlContent` string plus a `-ProjectNames` override per the Helpers tests),
but it is not necessary here: the ordering defect and its fix are entirely about the *order of two
statements* operating on an already-computed `$processedXmlContent` string, not about the content
of that string. Mocking `ConvertTo-KoverageCoberturaXml` directly (as the existing `Describe` block
already does for its other cases) is both the minimal-diff option and the one consistent with this
file's established convention.

**Assertion shape for the new regression test** (illustrative, not a directive to write test code
here): call `Invoke-MSTestWithCoverageMain -ScriptRoot $script:scriptDir` inside `{ ... } | Should
-Throw`, then assert `Should -Invoke Set-Content -Times 1 -Exactly` (optionally with a
`-ParameterFilter { $Path -eq <expected output path> -and $Value -eq '<coverage line-rate="0.5" />' }`,
using `$Path` per the exact parameter name at line 343, not `$LiteralPath`). Against the current
(pre-fix) statement order this assertion fails, because `Assert-CoberturaLineCoverageThreshold`
throws before `Set-Content` is ever reached; after the fix (swap lines 341/343) it passes, because
`Set-Content` runs first, unconditionally, regardless of the following threshold outcome.

## 5. Other call sites of the two functions

Repository-wide search (production `.ps1` files and all test files) confirms:

- `Assert-CoberturaLineCoverageThreshold` has exactly **one production call site**: line 341 of
  `scripts/vscode/Invoke-MSTestWithCoverage.ps1` (the one under investigation). All other matches
  are Pester test files (`Invoke-MSTestWithCoverage.Helpers.Tests.ps1` — direct unit tests of the
  function in isolation; `Invoke-MSTest.RunSettings.Tests.ps1` — the mocked
  `Invoke-MSTestWithCoverageMain` cases) and historical/archived Markdown research, plan, and audit
  documents under `docs/features/`.
- `Invoke-MSTestWithCoverageMain` has exactly **one production call site**: line 348 of the same
  file, inside its own bottom `if ($MyInvocation.InvocationName -ne '.')` guard. All other matches
  are the same Pester test file's mocked calls and archived Markdown documents.
- No other production script in the repository dot-sources or otherwise depends on the current
  (pre-fix) call order between these two functions. A grep for `Invoke-MSTestWithCoverage\.ps1|
  Invoke-MSTestWithCoverage\.Helpers\.ps1` across all `*.ps1` files returns only: the two subject
  files themselves (Helpers.ps1 is dot-sourced by the main script at line 261) and the three
  sibling Pester test files (`Invoke-MSTestWithCoverage.Helpers.Tests.ps1`,
  `Invoke-MSTestWithCoverage.ClosureFilter.Tests.ps1`, `Invoke-MSTest.RunSettings.Tests.ps1`).

## 6. File size

- `scripts/vscode/Invoke-MSTestWithCoverage.ps1`: 349 lines of content (the read tool reports an
  extra blank "line 350" for the file's trailing newline). Well under the repository's 500-line
  limit. The fix is a pure two-line swap (net zero or +0 line-count change if no comment is added,
  or a small positive delta if a one-line clarifying comment is added) — it cannot push the file
  toward the limit.
- `scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1`: 491 lines of content (again, the read
  tool reports an extra trailing blank "line 492"). This file is not touched by the fix (the
  `Assert-CoberturaLineCoverageThreshold` function itself is unchanged; only its caller's statement
  order in the other file changes) and remains under the 500-line limit.

## 7. Dot-sourcing by other production scripts

`scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1` is dot-sourced by exactly one production
script: `scripts/vscode/Invoke-MSTestWithCoverage.ps1` line 261
(`. (Join-Path $ScriptRoot 'Invoke-MSTestWithCoverage.Helpers.ps1')`), inside
`Invoke-MSTestWithCoverageMain`. `Invoke-MSTestWithCoverage.Helpers.ps1` itself dot-sources one
further sibling, `Invoke-MSTestWithCoverage.ClosureFilter.ps1`, at its own line 2
(`. (Join-Path $PSScriptRoot 'Invoke-MSTestWithCoverage.ClosureFilter.ps1')`).

`scripts/vscode/Invoke-MSTestWithCoverage.ps1` itself is not dot-sourced by any other production
script in the repository — only by the three Pester test files identified in section 5 (one via
plain `. $script:coverageScript -NoExecute`-style invocation is *not* used for this file; it is
always parsed via `ParseFile`/`GetScriptBlock` per section 3, precisely because the file has a
top-level auto-invocation guard that a plain `. <path>` dot-source would still respect but that the
test authors chose to make explicit via AST parsing). No other production `.ps1`/`.psm1` file
references either of the two subject file paths.

An agent editing lines 341/343 of `Invoke-MSTestWithCoverage.ps1` therefore has exactly one
production caller to consider (the file's own bottom guard) and one function-definition dependency
to preserve (the dot-source of `Invoke-MSTestWithCoverage.Helpers.ps1` at line 261, which must
continue to precede the two calls being reordered — it already does, at line 261, far above line
341/343, and is unaffected by the fix).

## Candidate approaches

**Approach A (recommended): pure statement reorder.** Swap lines 341 and 343 so `Set-Content`
executes immediately after `ConvertTo-KoverageCoberturaXml` (line 340) and before
`Assert-CoberturaLineCoverageThreshold`. No signature changes, no new parameters, no new
functions. Confirmed safe by section 2 (the assert function is a pure read-and-throw over its own
local `[xml]` copy) and section 5 (no other caller depends on the old order). This is the smallest
possible diff and matches the issue's stated fix.

**Rejected alternative:** wrapping both calls in a `try`/`finally` that always writes
`$processedXmlContent` regardless of assert outcome. Rejected because it is strictly more complex
than swapping two statements for the same observable result — the assert function has no output to
preserve past a throw, and `finally` semantics add an indirection with no behavioral benefit here.

## Testing implications

- Add one new `It` inside the existing `Describe 'Invoke-MSTestWithCoverageMain'` block in
  `tests/scripts/vscode/Invoke-MSTest.RunSettings.Tests.ps1` (see section 4 for the exact fixture
  value and assertion shape). This follows the file's existing `BeforeEach` mocking conventions
  exactly and requires no new seam, no new mock target, and no temporary file.
  This is a **bugfix regression test** per the repository's Bugfix Workflow: it must fail against
  the current (pre-fix) statement order and pass after the fix.
- No changes are needed to `Invoke-MSTestWithCoverage.Helpers.Tests.ps1` or
  `Invoke-MSTestWithCoverage.ClosureFilter.Tests.ps1` — the threshold-boundary tests in the former
  (lines 492-498) already fully cover `Assert-CoberturaLineCoverageThreshold`'s own behavior in
  isolation and remain unaffected by the caller-side reorder.
- No numeric spec.md acceptance criterion in this issue depends on a population/enumeration count,
  so the Numeric Derivation Evidence section is not applicable here; the two "exactly one call
  site" findings in section 5 are call-site verifications via a repository-wide grep across
  production `.ps1` files, not spec.md-driven numeric claims.
