# Regression case 9 — unit purity of name derivation

Timestamp: 2026-08-11T01-02
Task: `[P1-T8]` `[expect-fail]`
Test file: `tests/scripts/vscode/Invoke-MSTestWithCoverage.ClosureFilter.Tests.ps1`
Fully-qualified Pester name:
`Cobertura closure name derivation.derives declaring member, declaring type and closure classification purely from names`

## Pre-authoring size measurement and split decision ([P1-T12] pre-authorized split)

Measured before authoring case 9, as `[P1-T8]` requires:

| Point | `tests/scripts/vscode/Invoke-MSTestWithCoverage.ClosureFilter.Tests.ps1` |
|---|---|
| before case 9 | 248 lines |
| after cases 9 and 10 | 367 lines |
| ceiling | 500 lines |
| headroom after both cases | 133 lines |

Cases 9 and 10 together add 119 lines, reaching 367 — well below 500. The `[P1-T12]` pre-authorized
split into `tests/scripts/vscode/Invoke-MSTestWithCoverage.ClosureFilter.Unit.Tests.ps1` is therefore
**NOT taken**. That file is not created and is named nowhere: no `scan_folders` list, no `Run.Path`
list, no `run_poshqc_format` / `run_poshqc_analyze` file set, and no expected changed-file set
references it. `[P3-T10]` re-measures this file post-format against the same ceiling.

## Fixture

No document fixture. This case is a pure unit test: every assertion is a direct call with a literal
name string. No temporary file, no on-disk fixture, no `.cs` source.

## Assertion (verbatim)

```powershell
$expectedMembers = [ordered]@{
    '<M>b__0'                              = 'M'
    '<M>b__1_2'                            = 'M'
    '<M>g__L|3_0'                          = 'M'
    'Ns.T.<M>d__4'                         = 'M'
    'Ns.T.<>c__DisplayClass5_0.<<M>b__0>d' = 'M'
    'MoveNext'                             = $null
    '.ctor'                                = $null
}

foreach ($name in $expectedMembers.Keys) {
    $because = "input '$name'"
    { Get-CoberturaClosureDeclaringMemberName -Name $name } | Should -Not -Throw -Because $because

    $errorRecords = $null
    $warningRecords = $null
    $informationRecords = $null
    # Verbose (stream 4) is merged into success output and then partitioned by type, so
    # both the success-object count and the verbose-record count can be asserted.
    $rawOutput = @(
        Get-CoberturaClosureDeclaringMemberName -Name $name `
            -ErrorVariable errorRecords `
            -WarningVariable warningRecords `
            -InformationVariable informationRecords 4>&1
    )

    $successObjects = [System.Collections.ArrayList]::new()
    $verboseCount = 0
    foreach ($item in $rawOutput) {
        if ($item -is [System.Management.Automation.VerboseRecord]) { $verboseCount++ }
        else { $null = $successObjects.Add($item) }
    }

    # Exactly one object on the success stream (its return value, which may be $null).
    $successObjects.Count | Should -Be 1 -Because $because
    $successObjects[0] | Should -Be $expectedMembers[$name] -Because $because
    # Nothing on the error, warning, verbose or information streams.
    @($errorRecords).Count | Should -Be 0 -Because $because
    @($warningRecords).Count | Should -Be 0 -Because $because
    @($informationRecords).Count | Should -Be 0 -Because $because
    $verboseCount | Should -Be 0 -Because $because
}

# Get-CoberturaDeclaringTypeName truncates at the first '.<'; a name with no '.<' is
# returned unchanged.
Get-CoberturaDeclaringTypeName -Name 'Ns.T.<>c' | Should -Be 'Ns.T'
Get-CoberturaDeclaringTypeName -Name 'Ns.T.<>c__DisplayClass5_0' | Should -Be 'Ns.T'
Get-CoberturaDeclaringTypeName -Name 'Ns.T.<M>d__4' | Should -Be 'Ns.T'
Get-CoberturaDeclaringTypeName -Name 'Ns.T' | Should -Be 'Ns.T'
Get-CoberturaDeclaringTypeName -Name 'Ns.Outer.Inner' | Should -Be 'Ns.Outer.Inner'

# Test-CoberturaClosureClassName is true for the '.<>c' marker in every shape and
# deliberately false for a Type.<Member>d__N state machine and for a plain type.
Test-CoberturaClosureClassName -Name 'Ns.T.<>c' | Should -BeTrue
Test-CoberturaClosureClassName -Name 'Ns.T.<>c__DisplayClass5_0' | Should -BeTrue
Test-CoberturaClosureClassName -Name 'Ns.T.<>c__DisplayClass5_0.<<M>b__0>d' | Should -BeTrue
Test-CoberturaClosureClassName -Name 'Ns.T.<M>d__4' | Should -BeFalse
Test-CoberturaClosureClassName -Name 'Ns.T' | Should -BeFalse
```

## Coverage of the three plan acceptance criteria this case discharges

| Function | Discharges | Assertions |
|---|---|---|
| `Get-CoberturaClosureDeclaringMemberName` | `[P2-T2]` | 7 inputs, expected token or `$null`, plus stream and no-throw assertions per input |
| `Get-CoberturaDeclaringTypeName` | `[P2-T3]` | 5 inputs; sole exercise of this function anywhere in the plan |
| `Test-CoberturaClosureClassName` | `[P2-T1]` | 5 inputs; sole exercise of this function anywhere in the plan |

All three groups stay inside this single named `It`, preserving the ten-case count required by
spec AC 10.

`MoveNext` and `.ctor` returning `$null` is the unit-level evidence for the fail-safe retention path
(spec AC 12); case 4's `.ctor` retention is the orchestrator-level evidence.

## Observed pre-implementation failure

EXIT_CODE: 1

```
FAIL: derives declaring member, declaring type and closure classification purely from names
      => Expected no exception to be thrown, because input '<M>b__0', but an exception
         "The term 'Get-CoberturaClosureDeclaringMemberName' is not recognized as a name of a
         cmdlet, function, script file, or executable program."
```

Expected `[expect-fail]` reason: `CommandNotFoundException` on
`Get-CoberturaClosureDeclaringMemberName`, surfaced through the `Should -Not -Throw` wrapper that is
the case's first assertion. Not a discovery error, not a here-string syntax error, not a
malformed-XML harness error — Pester discovered 9 tests in the file.

## Passing result

Recorded by `[P3-T1]` (`<FEATURE>/evidence/regression-testing/pass-after-run.2026-08-11T01-30.md`).

- Timestamp: 2026-08-11T01-30
- Result: **Passed** (57ms)
- Test: `Cobertura closure name derivation.derives declaring member, declaring type and closure classification purely from names`
- Run EXIT_CODE: 0 (Passed=29, Failed=0, Skipped=0 across both test files)
