# Regression case 5 — whole-class removal (parts A and B)

Timestamp: 2026-08-11T00-50
Task: `[P1-T5]` `[expect-fail]`
Test file: `tests/scripts/vscode/Invoke-MSTestWithCoverage.ClosureFilter.Tests.ps1`
Fully-qualified Pester name:
`Remove-CoberturaExemptClosureCoverage.removes a closure class outright when every method resolves to an absent member`

Both parts are asserted inside this single named `It`, preserving the ten-case count required by
spec AC 10.

## Fixture (verbatim, inline here-string)

```xml
<coverage line-rate="0" branch-rate="0" lines-covered="0" lines-valid="0" branches-covered="0" branches-valid="0">
  <packages><package name="Ns" line-rate="0" branch-rate="0" complexity="1"><classes>
    <class name="Ns.A.&lt;&gt;c__DisplayClass1_0" filename="Ns\A.cs" line-rate="0" branch-rate="0" complexity="1">
      <methods><method name="&lt;Gone&gt;b__0" signature="()" line-rate="0" branch-rate="0"><lines><line number="30" hits="0" branch="False" /></lines></method></methods>
      <lines><line number="30" hits="0" branch="False" /></lines>
    </class>
    <class name="Ns.B" filename="Ns\B.cs" line-rate="0" branch-rate="0" complexity="1">
      <methods><method name="&lt;Exempt&gt;g__Local|7_0" signature="()" line-rate="0" branch-rate="0"><lines><line number="40" hits="0" branch="False" /></lines></method></methods>
      <lines><line number="40" hits="0" branch="False" /></lines>
    </class>
    <class name="Ns.B.&lt;&gt;c__DisplayClass2_0" filename="Ns\B.cs" line-rate="0" branch-rate="0" complexity="1">
      <methods><method name="&lt;Exempt&gt;b__0" signature="()" line-rate="0" branch-rate="0"><lines><line number="41" hits="0" branch="False" /></lines></method></methods>
      <lines><line number="41" hits="0" branch="False" /></lines>
    </class>
  </classes></package></packages>
</coverage>
```

Part A (`Ns\A.cs`): a closure class whose every method resolves to an absent member (`Gone`), with no
declaring-type class for that filename at all.

Part B (`Ns\B.cs`): a declaring-type class `Ns.B` whose ONLY method is the local-function shape
`&lt;Exempt&gt;g__Local|7_0`, plus a closure class `Ns.B.&lt;&gt;c__DisplayClass2_0` carrying
`&lt;Exempt&gt;b__0`.

## Assertion (verbatim)

```powershell
$declaringBeforeXml = $doc.SelectSingleNode('//class[@name="Ns.B"]').OuterXml

Remove-CoberturaExemptClosureCoverage -XmlDocument $doc

# Part A: the class element is gone and its filename no longer appears anywhere.
@($doc.SelectNodes('//class[@name="Ns.A.&lt;&gt;c__DisplayClass1_0"]')).Count | Should -Be 0
@($doc.SelectNodes('//class[@filename="Ns\A.cs"]')).Count | Should -Be 0
# Part B: a g__ local function on the declaring type does not admit 'Exempt'.
@($doc.SelectNodes('//class[@name="Ns.B.&lt;&gt;c__DisplayClass2_0"]')).Count | Should -Be 0
# Part B: the declaring type itself carries no '.<>c' marker and must not be mutated.
$doc.SelectSingleNode('//class[@name="Ns.B"]').OuterXml | Should -Be $declaringBeforeXml
@($doc.SelectNodes('//class[@filename="Ns\B.cs"]')).Count | Should -Be 1
```

Part B is the failing test that pins the "`g__` deliberately not admitted" rule of `[P2-T4]`. Without
it that rule has no discharging test: if `&lt;Exempt&gt;g__Local|7_0` were admitted to the presence
set, `Exempt` would be present and the closure class would wrongly survive. The additional
`OuterXml` equality assertion pins the "no behaviour change for non-closure classes" invariant — the
filter must not mutate a class whose name carries no `.<>c` marker — and the surviving
`Ns\B.cs` filename count of 1 confirms the file remains in the report via that class.

## Observed pre-implementation failure

EXIT_CODE: 1

```
[-] removes a closure class outright when every method resolves to an absent member
 at <ScriptBlock>, tests\scripts\vscode\Invoke-MSTestWithCoverage.ClosureFilter.Tests.ps1:168
 CommandNotFoundException: The term 'Remove-CoberturaExemptClosureCoverage' is not recognized as a
 name of a cmdlet, function, script file, or executable program.
```

Expected `[expect-fail]` reason.

## Passing result

Recorded by `[P3-T1]` (`<FEATURE>/evidence/regression-testing/pass-after-run.2026-08-11T01-30.md`).

- Timestamp: 2026-08-11T01-30
- Result: **Passed** (9ms)
- Test: `Remove-CoberturaExemptClosureCoverage.removes a closure class outright when every method resolves to an absent member`
- Run EXIT_CODE: 0 (Passed=29, Failed=0, Skipped=0 across both test files)
