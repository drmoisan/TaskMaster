# Regression case 4 — mixed closure (and the fail-safe retention path)

Timestamp: 2026-08-11T00-50
Task: `[P1-T4]` `[expect-fail]`
Test file: `tests/scripts/vscode/Invoke-MSTestWithCoverage.ClosureFilter.Tests.ps1`
Fully-qualified Pester name:
`Remove-CoberturaExemptClosureCoverage.drops only the exempt method from a mixed closure class and retains an underivable method`

## Fixture (verbatim, inline here-string)

```xml
<coverage line-rate="0" branch-rate="0" lines-covered="0" lines-valid="0" branches-covered="0" branches-valid="0">
  <packages><package name="Ns" line-rate="0" branch-rate="0" complexity="1"><classes>
    <class name="Ns.T" filename="Ns\T.cs" line-rate="1" branch-rate="1" complexity="1">
      <methods><method name="Visible" signature="()" line-rate="1" branch-rate="1"><lines><line number="10" hits="1" branch="False" /></lines></method></methods>
      <lines><line number="10" hits="1" branch="False" /></lines>
    </class>
    <class name="Ns.T.&lt;&gt;c" filename="Ns\T.cs" line-rate="0.5" branch-rate="0" complexity="3">
      <methods><method name="&lt;Exempt&gt;b__0_0" signature="()" line-rate="0" branch-rate="0"><lines><line number="20" hits="0" branch="False" /></lines></method><method name="&lt;Visible&gt;b__1_0" signature="()" line-rate="1" branch-rate="0"><lines><line number="21" hits="1" branch="False" /></lines></method><method name=".ctor" signature="()" line-rate="1" branch-rate="0"><lines><line number="22" hits="1" branch="False" /></lines></method></methods>
      <lines><line number="20" hits="0" branch="False" /><line number="21" hits="1" branch="False" /><line number="22" hits="1" branch="False" /></lines>
    </class>
  </classes></package></packages>
</coverage>
```

The single `&lt;&gt;c` class carries three methods:

| Method | Derived declaring member | Presence-set lookup | Expected outcome |
|---|---|---|---|
| `&lt;Exempt&gt;b__0_0` | `Exempt` | absent | dropped |
| `&lt;Visible&gt;b__1_0` | `Visible` | present via `<method name="Visible">` on `Ns.T` | retained |
| `.ctor` | none — underivable | not consulted | **retained (fail-safe)** |

`.ctor` is genuinely underivable here: the method name matches none of the recognized synthesized
shapes, and the class-name fallback yields nothing either, because the class is a plain
`&lt;&gt;c` carrying no `<Member>` token. `MoveNext` could not serve this purpose, because inside a
`&lt;&gt;c….&lt;&lt;M&gt;b__0&gt;d` class the class-name fallback specified in `[P2-T5]` derives `M`.

## Assertion (verbatim)

```powershell
Remove-CoberturaExemptClosureCoverage -XmlDocument $doc

$closure = $doc.SelectSingleNode('//class[@name="Ns.T.&lt;&gt;c"]')
$retainedLines = @($closure.SelectNodes('./lines/line')) | ForEach-Object { $_.number }
$retainedMethods = @($closure.SelectNodes('./methods/method')) | ForEach-Object { $_.name }
$summary = Get-CoberturaCoverageSummary -XmlDocument $doc

# The class survives, having lost only the exempt method.
$closure | Should -Not -BeNullOrEmpty
$retainedMethods | Should -Not -Contain '&lt;Exempt&gt;b__0_0'
$retainedMethods | Should -Contain '&lt;Visible&gt;b__1_0'
# Fail-safe retention: '.ctor' is underivable, so it is kept, not removed.
$retainedMethods | Should -Contain '.ctor'
# <lines> is rebuilt as the de-duplicated union of the RETAINED methods' lines.
($retainedLines -join ',') | Should -Be '21,22'
# line-rate is recomputed against the rebuilt set (2 of 2 covered).
$closure.'line-rate' | Should -Be '1'
$summary.LinesValid | Should -Be '3'
$summary.LinesCovered | Should -Be '3'
```

The `.ctor` retention assertion is the plan's only orchestrator-level exercise of the fail-safe path
inside `Remove-CoberturaExemptClosureCoverage` and is the evidence for spec AC 12. Case 7 exercises a
different code path (a non-closure `d__` class, rejected by `Test-CoberturaClosureClassName` before
any derivation is attempted) and case 9 is a pure unit test of the name-derivation function, so
neither can establish orchestrator behaviour.

## Observed pre-implementation failure

EXIT_CODE: 1

```
[-] drops only the exempt method from a mixed closure class and retains an underivable method
 at <ScriptBlock>, tests\scripts\vscode\Invoke-MSTestWithCoverage.ClosureFilter.Tests.ps1:120
 CommandNotFoundException: The term 'Remove-CoberturaExemptClosureCoverage' is not recognized as a
 name of a cmdlet, function, script file, or executable program.
```

Expected `[expect-fail]` reason.

## Passing result

Recorded by `[P3-T1]` (`<FEATURE>/evidence/regression-testing/pass-after-run.2026-08-11T01-30.md`).

- Timestamp: 2026-08-11T01-30
- Result: **Passed** (36ms)
- Test: `Remove-CoberturaExemptClosureCoverage.drops only the exempt method from a mixed closure class and retains an underivable method`
- Run EXIT_CODE: 0 (Passed=29, Failed=0, Skipped=0 across both test files)
